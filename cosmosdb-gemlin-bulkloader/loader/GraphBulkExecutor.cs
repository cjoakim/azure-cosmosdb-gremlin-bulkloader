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
        private readonly Container container;

        private VertexDocumentHelper vertexDocumentHelper;
        private EdgeDocumentHelper edgeDocumentHelper;
        private CosmosClient client;
        private string VertexPartitionProperty;
        private bool isDisposed = false;
        private bool isSuccessfullyInitialized = false;

        public GraphBulkExecutor(
            string connectionString,
            string databaseName,
            string containerName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            this.client = new CosmosClient(connectionString,
                new CosmosClientOptions()
                {
                    AllowBulkExecution = true,
                    ApplicationName = GraphBulkExecutor.GetApplicationName()
                });

            this.container = this.client.GetContainer(databaseName, containerName);
            Console.WriteLine("GraphBulkExecutor#constructor completed");
        }

        public async Task BulkImportAsync(
            IEnumerable<IGremlinElement> gremlinElements,
            bool enableUpsert = true,
            CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            await this.InitializeAsync(cancellationToken);

            List<JObject> elements =
                gremlinElements.Select(
                    element => this.GetGraphElementDocument(element)).ToList();
            List<Task> tasks = new List<Task>(elements.Count);

            foreach (JObject element in elements)
            {
                if (enableUpsert)
                {
                    tasks.Add(this.container.UpsertItemAsync(element, cancellationToken: cancellationToken).CaptureOperationResponse(element));
                }
                else
                {
                    tasks.Add(this.container.CreateItemAsync(element, cancellationToken: cancellationToken).CaptureOperationResponse(element));
                }
            }
            await Task.WhenAll(tasks);
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
            return string.Format("GraphBulkImportSDK-{0}-{1}",
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
