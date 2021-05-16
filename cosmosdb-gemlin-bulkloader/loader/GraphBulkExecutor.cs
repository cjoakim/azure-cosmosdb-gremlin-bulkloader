// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

//  Original code by Matias Quaranta, see https://github.com/ealsur/GraphBulkExecutorV3

namespace CosmosGemlinBulkLoader
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using CosmosGemlinBulkLoader.Bulk;
    using CosmosGemlinBulkLoader.Element;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Fluent;
    using Newtonsoft.Json.Linq;


    public class GraphBulkExecutor : IDisposable
    {
        private readonly SemaphoreSlim initializationSyncLock = new SemaphoreSlim(1, 1);
        private readonly bool isMultiValuedAndMetaPropertiesDisabled = false;  // cj!
        private readonly Database database = null;
        private readonly Container container = null;

        private Config config;
        private VertexDocumentHelper vertexDocumentHelper;
        private EdgeDocumentHelper edgeDocumentHelper;
        private CosmosClient client;
        public Throttle throttle = null;
        private string VertexPartitionProperty;
        private bool isDisposed = false;
        private bool isSuccessfullyInitialized = false;

        public GraphBulkExecutor(Config config)
        {
            this.config     = config;
            string connStr  = config.GetCosmosConnString();
            string dbName   = config.GetCosmosDbName();
            string collName = config.GetCosmosGraphName();
            string appName  = GraphBulkExecutor.GetApplicationName();
            
            Console.WriteLine("GraphBulkExecutor constructor:");
            Console.WriteLine("  appName:  {0}", appName);
            Console.WriteLine("  dbName:   {0}", dbName);
            Console.WriteLine("  collName: {0}", collName);
            Console.WriteLine("  connStr:  {0}", connStr.Substring(0, 80)); 
            
            if (string.IsNullOrEmpty(connStr))
            {
                throw new ArgumentNullException(nameof(connStr));
            }

            this.client = new CosmosClient(connStr,
                new CosmosClientOptions()
                {
                    ApplicationName = appName,
                    AllowBulkExecution = true,
                    ConnectionMode = ConnectionMode.Direct,
                    MaxRetryAttemptsOnRateLimitedRequests = 30,
                    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromMilliseconds(90000)
                });
                // See https://docs.microsoft.com/en-us/azure/cosmos-db/performance-tips-dotnet-sdk-v3-sql
                // MaxRetryAttemptsOnRateLimitedRequests - default is 9
                // MaxRetryWaitTimeOnRateLimitedRequests - default time is 30 seconds
            
            this.database = this.client.GetDatabase(dbName);
            this.container = this.client.GetContainer(dbName, collName);
            Console.WriteLine("GraphBulkExecutor#constructor completed");
        }
        
        public async Task<int> InitializeThrottle()
        {
            Console.WriteLine("GraphBulkExecutor#InitializeThrottle...");
            int targetRuSetting = await this.GetThroughputRU();
            Console.WriteLine($"GraphBulkExecutor#InitializeThrottle targetRuSetting {targetRuSetting}");
            this.throttle = new Throttle(config, targetRuSetting);
            throttle.Display();
            return targetRuSetting;
        }

        /**
         * Return the throughput in RU of either the database, or the graph.
         * Return 0 if unable to determine the RU setting.
         */
        public async Task<int> GetThroughputRU()
        {
            try
            {
                // Note: the ReadThroughputAsync methods return the CURRENT RU setting,
                // and not the potentially higher autoscaled setting.
                // For example, can return 5000 when the autoscale max is 50000.
                
                int? databaseRU = await database.ReadThroughputAsync();
                Console.WriteLine($"GraphBulkExecutor#GetThroughputRU databaseRU {databaseRU}");
                
                int? containerRU = await container.ReadThroughputAsync();
                Console.WriteLine($"GraphBulkExecutor#GetThroughputRU containerRU {containerRU}");
                
                if (databaseRU != null)
                {
                    return (int) databaseRU;
                }
                if (containerRU != null)
                {
                    return (int) containerRU;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return 0;
        }
        public async Task BulkImportAsync(
            IEnumerable<IGremlinElement> gremlinElements,
            bool enableUpsert = true,
            CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            await this.InitializeAsync(cancellationToken);

            List<Task> tasks = new List<Task>();
            List<JObject> elements =
                gremlinElements.Select(
                    element => this.GetGraphElementDocument(element)).ToList();
            
            foreach (JObject element in elements)
            {
                if (enableUpsert)
                {
                    tasks.Add(this.container.UpsertItemAsync(
                        element, cancellationToken: cancellationToken).CaptureOperationResponse(element));
                }
                else
                {
                    tasks.Add(this.container.CreateItemAsync(
                        element, cancellationToken: cancellationToken).CaptureOperationResponse(element));
                }
            }
            
            List<Task> shuffledTasks = throttle.AddShuffleThrottlingTasks(tasks);
            //Console.WriteLine($"GraphBulkExecutor awaiting {shuffledTasks.Count} tasks...");

            // https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall?view=net-5.0
            Task allTasks = Task.WhenAll(shuffledTasks.ToArray());
            await allTasks;
            int failCount = 0;
            foreach (var t in shuffledTasks) {
                if (t.Status != TaskStatus.RanToCompletion)
                {
                    failCount++;
                    Console.WriteLine("TASK Id: {0}, Status: {1}, failCount: {2}", t.Id, t.Status, failCount); 
                }
            }
        }
        
        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }
            if (this.client != null)
            {
                this.client.Dispose();
            }
            this.isDisposed = true;
            Console.WriteLine("GraphBulkExecutor#Dispose completed");
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (this.isSuccessfullyInitialized)
            {
                return;
            }

            await this.initializationSyncLock.WaitAsync(cancellationToken);
            try
            {
                if (this.isSuccessfullyInitialized)
                {
                    return;
                }

                await this.VerifyCollectionPropertiesAsync(cancellationToken);

                bool isPartitionedCollection = (!string.IsNullOrEmpty(this.VertexPartitionProperty));
                // Console.WriteLine($"isPartitionedCollection: {isPartitionedCollection}");

                // Parameters of the constructors below would come from GraphConnection
                this.vertexDocumentHelper = new VertexDocumentHelper(
                    isPartitionedCollection: isPartitionedCollection, 
                    isMultiValuedAndMetaPropertiesDisabled: this.isMultiValuedAndMetaPropertiesDisabled, 
                    partitionKey: this.VertexPartitionProperty);
                
                this.edgeDocumentHelper = new EdgeDocumentHelper(
                    isPartitionedCollection: isPartitionedCollection,
                    isMultiValuedAndMetaPropertiesDisabled: this.isMultiValuedAndMetaPropertiesDisabled,
                    partitionKey: this.VertexPartitionProperty);
            }
            finally
            {
                this.initializationSyncLock.Release();
            }
        }

        private async Task VerifyCollectionPropertiesAsync(CancellationToken cancellationToken)
        {
            ContainerProperties properties = await this.container.ReadContainerAsync(cancellationToken: cancellationToken);
            string partitionFullPath = properties.PartitionKeyPath;

            string partitionTopLevelPath =
                    partitionFullPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[0];

            if (partitionFullPath == $"/{GremlinKeywords.KW_DOC_ID}")
            {
                throw new Exception("A container with /id as partition key cannot be used for graph operations.");
            }
            else if (partitionFullPath == $"/{GremlinKeywords.KW_DOC_LABEL}")
            {
                throw new Exception("A container with /label as partition key cannot be used for graph operations.");
            }
            else if (partitionFullPath == $"/{GremlinKeywords.KW_DOC_PARTITION}")
            {
                // When the document collection is physically partitioned by "/_partition",
                // The value of this property mirrors the value of the property VertexPartitionProperty.
                // VertexPartitionProperty is "id" by default, unless it's set to another vertex property.
                this.VertexPartitionProperty = this.VertexPartitionProperty ?? GremlinKeywords.KW_DOC_ID;
            }
            else
            {
                this.VertexPartitionProperty = partitionTopLevelPath;
            }
        }

        private static string GetApplicationName()
        {
            return string.Format("GraphBulkImporter-{0}-{1}",
                typeof(GraphBulkExecutor).GetTypeInfo().Assembly.GetName().Version,
                typeof(GraphBulkExecutor).GetTypeInfo().Assembly.ImageRuntimeVersion);
        }
        
        private JObject GetGraphElementDocument(IGremlinElement element)
        {
            if (element is GremlinVertex vertex)
            {
                return this.vertexDocumentHelper.GetVertexDocument(vertex);
            }
            if (element is GremlinEdge edge)
            {
                return this.edgeDocumentHelper.GetEdgeDocument(edge);
            }
            throw new Exception($"Only {typeof(GremlinVertex)} and {typeof(GremlinEdge)} is supported by Graph Bulk Executor tool.");
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("GraphBulkExecutor");
            }
        }
    }
}
