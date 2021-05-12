// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

// Chris Joakim, Microsoft, May 2021

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using CosmosGemlinBulkLoader.Csv;
using CosmosGemlinBulkLoader.Element;
using Microsoft.Azure.Cosmos;

namespace CosmosGemlinBulkLoader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class Program
    {
        // Class variables:
        private static Config config = null;
        private static GraphBulkExecutor graphBulkExecutor = null;
        private static HeaderRow headerRow = null;
        private static CsvRowParser parser = null;
        private static BlobServiceClient blobServiceClient = null;
        private static BlobContainerClient containerClient = null;
        private static BlobClient blobClient = null;
        private static string infile = null;
        private static string source = null;
        private static string fileType = null;
        private static bool doLoad = false;
        private static bool verbose = false;
        private static long rowCount = 0;
        private static long batchSize = Config.DEFAULT_BATCH_SIZE;
        private static long batchCount = 0;

        private static void DisplayCliOptions(string msg)
        {
            Console.WriteLine("");
            Console.WriteLine("Command-Line Options:");
            if (msg != null) {
                Console.WriteLine($"ERROR: {msg}");
            }
            Console.WriteLine("$ dotnet run preprocess --file-type vertex --csv-infile /data/vertex.csv");
            Console.WriteLine("$ dotnet run preprocess --file-type vertex --csv-infile Data/amtrak-station-vertices.csv");
            Console.WriteLine("");
        }

        static async Task Main(string[] args)
        {
            try
            {
                long startTime = EpochMsTime();
                Console.WriteLine($"start timestamp: {CurrentTimestamp()}");
                config = LoadConfig(args);
                if (!config.IsValid())
                {
                    System.Environment.Exit(1);
                }
                if (config.DoLoad())
                {
                    graphBulkExecutor = CreateGraphBulkExecutor();
                }
                
                parser = new CsvRowParser(config);
                List<IGremlinElement> elements = new List<IGremlinElement>();
                
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    NewLine = Environment.NewLine,
                    Delimiter = config.GetCsvFieldSeparator().ToString()
                };
                using (var csv = new CsvReader(GetSourceStreamReader(), csvConfig))
                {
                    // Header Row Processing and Validation
                    csv.Read();
                    csv.ReadHeader();
                    rowCount++;
                    string json = JsonConvert.SerializeObject(csv.HeaderRecord, Formatting.Indented);
                    Console.WriteLine("CsvReader header row fields: {0}", json);
                    ParseHeaderRow(csv.HeaderRecord);
                    if (!headerRow.IsValid())
                    {
                        System.Environment.Exit(2);
                    }

                    // Data Row Processing Loop
                    while (csv.Read())
                    {
                        var dict = csv.GetRecord<dynamic>() as IDictionary<string, object>;  // System.Dynamic.ExpandoObject;
                        // See https://joshclose.github.io/CsvHelper/examples/reading/reading-by-hand/
                        // See https://docs.microsoft.com/en-us/dotnet/api/system.dynamic.expandoobject?view=net-5.0
                        rowCount++;
                        if (verbose || (rowCount < 5)) { 
                            json = JsonConvert.SerializeObject(dict, Formatting.Indented);
                            Console.WriteLine("  row dict: {0}", json);
                        }
                        IGremlinElement element = parser.ParseRow(dict);
                        if (element == null)
                        {
                            json = JsonConvert.SerializeObject(dict, Formatting.Indented);
                            Console.WriteLine("ERROR: Unable to parse row number: {0} {1}", rowCount, json);
                        }
                        else
                        {
                            if (verbose || (rowCount < 5))
                            {
                                Console.WriteLine(JsonConvert.SerializeObject(element, Formatting.Indented));
                            }
                            if (doLoad)
                            {
                                elements.Add(element);

                                // Process the Bulk Loads in configurable batches so as to handle huge input files
                                if (elements.Count == batchSize)
                                {
                                    await Load(elements);
                                    elements = new List<IGremlinElement>();  // reset the List for the next batch
                                }
                            }
                        }
                    }
                }

                if (doLoad)
                {
                    if (elements.Count > 0)
                    {
                        await Load(elements);  // load the last batch
                    }
                }
                long elapsedTime = EpochMsTime() - startTime;
                Console.WriteLine($"finish timestamp: {CurrentTimestamp()}");
                Console.WriteLine("Main completed in: {0} ms, rowCount: {1}", elapsedTime, rowCount);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: Exception in Main() - ", e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                if (graphBulkExecutor != null)
                {
                    Console.WriteLine("Disposing GraphBulkExecutor...");
                    graphBulkExecutor.Dispose();
                }
            }
            await Task.Delay(0);
        }

        static Config LoadConfig(string[] args)
        {
            config = new Config(args);
            if (config.IsValid())
            {
                fileType = config.GetFileType();
                doLoad   = config.DoLoad();
                verbose  = config.IsVerbose();
                batchSize = config.GetBatchSize(); 
                config.Display();
            }
            return config;
        }

        static GraphBulkExecutor CreateGraphBulkExecutor()
        {
            string connStr  = config.GetCosmosConnString();
            string dbName   = config.GetCosmosDbName();
            string collName = config.GetCosmosGraphName();
            Console.WriteLine("Creating GraphBulkExecutor...");
            Console.WriteLine("  dbName:   {0}", dbName);
            Console.WriteLine("  collName: {0}", collName);
            Console.WriteLine("  connStr:  {0}", connStr.Substring(0, 80));
            return new GraphBulkExecutor(connStr, dbName, collName); 
        }
        
        /**
         * Return a StreamReader sourced from either a local file, or an Azure Storage Blob,
         * per command-line inputs.  If Blob, then also connect to Azure Storage.
         */
        static StreamReader GetSourceStreamReader()
        {
            if (config.IsBlobInput())
            {
                string connStr = config.GetStorageConnString();
                if (config.IsVerbose())
                {
                    Console.WriteLine($"connection string: {connStr}");
                }
                blobServiceClient = new BlobServiceClient(config.GetStorageConnString());
                containerClient = blobServiceClient.GetBlobContainerClient(config.GetStorageContainerName());
                blobClient = containerClient.GetBlobClient(config.GetStorageBlobName());
                source = $"blob: {config.GetStorageContainerName()} {config.GetStorageBlobName()}";
                return new StreamReader(blobClient.OpenRead());
            }
            else
            {
                infile = config.GetCsvInfile();
                source = $"file: {infile}";
                return new StreamReader(infile);
            }
        }
        
        static void ParseHeaderRow(string[] headerFields)
        {
            headerRow = new HeaderRow(
                source,
                headerFields,
                config.GetFileType(),
                config.GetCsvFieldSeparator(),
                config.GetDatatypeSeparator());

            if (headerRow.IsValid())
            {
                parser.SetHeaderRow(headerRow);
                headerRow.Display();
            }
            else
            {
                Console.WriteLine("ERROR: headerRow is invalid, program will exit...");
            }
        }

        static async Task Load(List<IGremlinElement> elements)
        {
            batchCount++;
            long startTime = EpochMsTime();
            Console.WriteLine("Start of batch load {0}, with {1} elements, at {2}", 
                batchCount, elements.Count, CurrentTimestamp());
            await graphBulkExecutor.BulkImportAsync(elements, false);
            Console.WriteLine("Batch load {0} completed in {1}ms", batchCount, EpochMsTime() - startTime);
        }

        private static long EpochMsTime()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        private static string CurrentTimestamp()
        {
            return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }
    }
}
