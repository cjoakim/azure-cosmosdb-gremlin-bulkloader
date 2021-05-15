// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

// Chris Joakim, Microsoft, May 2021

namespace CosmosGemlinBulkLoader
{
    using System;
    using Newtonsoft.Json;

    /**
     * This class is the source of all configuration values for this application -  
     * including environment variables and command-line arguments.  It does this 
     * to support either command-line/terminal/shell or Docker container execution.   
     * With Docker, the command-line can be passed in as environment variable 'CLI_ARGS_STRING'.
     */

    public class Config
    {
        // Constants; environment variables:
        public const string AZURE_COSMOSDB_GRAPHDB_CONN_STRING = "AZURE_COSMOSDB_GRAPHDB_CONN_STRING";
        public const string AZURE_COSMOSDB_GRAPHDB_DBNAME      = "AZURE_COSMOSDB_GRAPHDB_DBNAME";
        public const string AZURE_COSMOSDB_GRAPHDB_GRAPH       = "AZURE_COSMOSDB_GRAPHDB_GRAPH";
        public const string AZURE_STORAGE_CONNECTION_STRING    = "AZURE_STORAGE_CONNECTION_STRING";

        // Constants; command-line and keywords:
        public const string CLI_ARGS_STRING                = "CLI_ARGS_STRING";  // for executing in a Docker container
        public const string CLI_FUNCTION_PREPROCESS        = "preprocess";
        public const string CLI_FUNCTION_LOAD              = "load";
        public const string CSV_INFILE_KEYWORD             = "--csv-infile";
        public const string CSV_FIELD_SEPARATOR_KEYWORD    = "--csv-field-sep";
        public const string FILE_TYPE_KEYWORD              = "--file-type";      // vertex or edge
        public const string DATATYPE_SEPARATOR_KEYWORD     = "--datatype-sep";
        public const string ARRAY_SEPARATOR_KEYWORD        = "--array-sep";
        public const string BATCH_SIZE_KEYWORD             = "--batch-size";
        public const string PARTITION_KEY_KEYWORD          = "--partition-key";
        public const string BLOB_CONTAINER_KEYWORD         = "--blob-container";
        public const string BLOB_NAME_KEYWORD              = "--blob-name";
        public const string THROTTLE_KEYWORD               = "--throttle";
        public const string THROTTLE_TASK_MS_KEYWORD       = "--throttle-task-ms";
        public const string VERBOSE_FLAG                   = "--verbose";

        public const char   DEFAULT_CSV_FIELD_SEPARATOR    = ',';
        public const char   DEFAULT_DATATYPE_SEPARATOR     = ':';
        public const char   DEFAULT_ARRAY_SEPARATOR        = '^';
        public const string DEFAULT_PARTITON_KEY_ATTR      = "pk";
        public const string FILE_TYPE_VERTEX               = "vertex";  
        public const string FILE_TYPE_EDGE                 = "edge";
        public const int    DEFAULT_BATCH_SIZE             = 10000;
        public const int    DEFAULT_THROTTLE               = 5; 
        public const int    BASE_THROTTLE_TASK_MS          = 12000;
        public const int    MIN_VERTEX_ROW_FIELD_COUNT     = 3;
        public const int    MIN_EDGE_ROW_FIELD_COUNT       = 9;
        public const int    MAX_BATCH_SIZE                 = 20000;
        public const int    MIN_BATCH_SIZE                 = 10;
        public const int    MAX_THROTTLE                   = 10; 
        public const int    MIN_THROTTLE                   = 1; 
        public const int    MIN_RU_SETTING                 = 5000; 
        
        // Instance variables:
        private string[] cliArgs = { };

        public Config(string[] args)
        {
            cliArgs = args;  // dotnet run xxx yyy -> args:["xxx","yyy"]
            if (cliArgs.Length == 0)
            {
                // If no args, then the Program was invoked in a Docker container, 
                // so use the CLI_ARGS_STRING environment variable instead.
                cliArgs = GetEnvVar(CLI_ARGS_STRING, "").Split();
                Console.WriteLine("CLI_ARGS: " + JsonConvert.SerializeObject(cliArgs));
            }
        }

        public bool IsValid()
        {
            Console.WriteLine("Config#IsValid args: " + JsonConvert.SerializeObject(cliArgs));

            if (cliArgs.Length < 2)
            {
                Console.WriteLine("ERROR: empty command-line args");
                return false;
            }

            string fileType = GetFileType();
            if (fileType == null)
            {
                Console.WriteLine("ERROR: Invalid Config: no {0}", FILE_TYPE_KEYWORD);
                return false;
            }
            else
            {
                switch (fileType)
                {
                    case FILE_TYPE_VERTEX:
                        break;
                    case FILE_TYPE_EDGE:
                        break;
                    default:
                        Console.WriteLine("Invalid Config: unknown file type {0}", fileType);
                        return false;
                }
            }

            if (IsBlobInput())
            {
                Console.WriteLine("blob input specified for this run");
                return true;
            }
            else
            {
                Console.WriteLine("file input specified for this run");
                if (GetCsvInfile() == null)
                {
                    Console.WriteLine("ERROR: Invalid Config: no {0}", CSV_INFILE_KEYWORD);
                    return false;
                }
            }

            if (GetBatchSize() > MAX_BATCH_SIZE)
            {
                Console.WriteLine("ERROR: Batch size is too large {0}, max is {1}", GetBatchSize(), MAX_BATCH_SIZE);
                return false;
            }
            if (GetBatchSize() < MIN_BATCH_SIZE)
            {
                Console.WriteLine("ERROR: Batch size is too small {0}, min is {1}", GetBatchSize(), MIN_BATCH_SIZE);
                return false;
            }
            
            if (GetThrottle() > MAX_THROTTLE)
            {
                Console.WriteLine("ERROR: Throttle value is too large {0}, max is {1}", GetThrottle(), MAX_THROTTLE);
                return false;
            }
            if (GetThrottle() < MIN_THROTTLE)
            {
                Console.WriteLine("ERROR: Throttle value is too small {0}, min is {1}", GetThrottle(), MIN_THROTTLE);
                return false;
            }
            
            return true;
        }

        public string[] GetCliArgs()
        {
            return cliArgs;
        }

        public string GetRunType()
        {
            return cliArgs[0].ToLower();
        }

        public bool DoLoad()
        {
            return GetRunType() == CLI_FUNCTION_LOAD;
        }

        public bool DoPreprocess()
        {
            return GetRunType() == CLI_FUNCTION_PREPROCESS;
        }

        /**
         * This method is intended for unit-testing purposes only; see ConfigTest.cs
         */
        public void SetCliArgs(string commandLine)
        {
            cliArgs = commandLine.Split(" ");
        }

        public string GetCosmosConnString()
        {
            return GetEnvVar(AZURE_COSMOSDB_GRAPHDB_CONN_STRING, null);
        }

        public string GetCosmosDbName()
        {
            return GetEnvVar(AZURE_COSMOSDB_GRAPHDB_DBNAME, null);
        }

        public string GetCosmosGraphName()
        {
            return GetEnvVar(AZURE_COSMOSDB_GRAPHDB_GRAPH, null);
        }

        public string GetStorageConnString()
        {
            return GetEnvVar(AZURE_STORAGE_CONNECTION_STRING, null);
        }
        
        public string GetStorageContainerName()
        {
            return GetCliKeywordArg(BLOB_CONTAINER_KEYWORD);
        }
        
        public string GetStorageBlobName()
        {
            return GetCliKeywordArg(BLOB_NAME_KEYWORD);
        }

        public bool IsBlobInput()
        {
            if (GetStorageContainerName() == null)
            {
                return false;
            }
            if (GetStorageBlobName() == null)
            {
                return false;
            }
            return true;
        }
        
        public string GetEnvVar(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        public string GetEnvVar(string name, string defaultValue=null)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (value == null)
            {
                return defaultValue;
            }
            else
            {
                return value;
            }
        }

        public string GetCsvInfile()
        {
            return GetCliKeywordArg(CSV_INFILE_KEYWORD);
        }

        public char GetCsvFieldSeparator()
        {
            string sep = GetCliKeywordArg(CSV_FIELD_SEPARATOR_KEYWORD);
            if (sep == null)
            {
                return DEFAULT_CSV_FIELD_SEPARATOR;
            }
            else {
                return sep.ToCharArray()[0];
            }
        }

        public char GetDatatypeSeparator()
        {
            string sep = GetCliKeywordArg(DATATYPE_SEPARATOR_KEYWORD);
            if (sep == null)
            {
                return DEFAULT_DATATYPE_SEPARATOR;
            }
            else
            {
                return sep.ToCharArray()[0];
            }
        }

        public char GetArraySeparator()
        {
            string sep = GetCliKeywordArg(ARRAY_SEPARATOR_KEYWORD);
            if (sep == null)
            {
                return DEFAULT_ARRAY_SEPARATOR;
            }
            else
            {
                return sep.ToCharArray()[0];
            }
        }

        public int GetBatchSize()
        {
            string num = GetCliKeywordArg(BATCH_SIZE_KEYWORD);
            if (num == null)
            {
                return DEFAULT_BATCH_SIZE;
            }
            else
            {
                try
                {
                    return int.Parse(num);
                }
                catch
                {
                    Console.WriteLine("WARNING: unable to parse batch size {0}, using default", num);
                    return DEFAULT_BATCH_SIZE;
                }
            }
        }
        
        public int GetBaseThrottleTaskMilliseconds()
        {
            return BASE_THROTTLE_TASK_MS;
        }

        /**
         * Return an int between 0 and 100.  0 = Autothrottle.  Defaults to DEFAULT_THROTTLE (75).
         */
        public int GetThrottle()
        {
            try
            {
                return Int32.Parse(GetCliKeywordArg(THROTTLE_KEYWORD));
            }
            catch (Exception e)
            {
                int d = DEFAULT_THROTTLE;
                Console.Write($"WARNING: non-numeric --throttle, defaulting to {d} due to {e.Message}");
                return d;
            }
        }
        
        public string GetFileType()
        {
            return GetCliKeywordArg(FILE_TYPE_KEYWORD);
        }

        public string GetPartitionKeyAttr()
        {
            string attrName = GetCliKeywordArg(PARTITION_KEY_KEYWORD);
            if (attrName == null)
            {
                return DEFAULT_PARTITON_KEY_ATTR;
            }
            else
            {
                return attrName;
            }
        }

        public bool IsVertexFiletype()
        {
            return GetFileType() == FILE_TYPE_VERTEX;
        }

        public bool IsEdgeFiletype()
        {
            return GetFileType() == FILE_TYPE_EDGE;
        }

        public bool IsVerbose()
        {
            for (int i = 0; i < cliArgs.Length; i++)
            {
                if (cliArgs[i] == VERBOSE_FLAG)
                {
                    return true;
                }
            }
            return false;
        }

        public string GetCliKeywordArg(string keyword, string defaultValue=null)
        {
            try
            {
                for (int i = 0; i < cliArgs.Length; i++)
                {
                    if (keyword == cliArgs[i])
                    {
                        return cliArgs[i + 1];
                    }
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public void Display()
        {
            Console.WriteLine("Config:");
            Console.WriteLine($"  args:            {JsonConvert.SerializeObject(GetCliArgs())}");
            Console.WriteLine($"  run type:        {GetRunType()}");
            Console.WriteLine($"  infile:          {GetCsvInfile()}");
            Console.WriteLine($"  file type:       {GetFileType()}");
            Console.WriteLine($"  batch size:      {GetBatchSize()}");
            Console.WriteLine($"  csv field sep:   {GetCsvFieldSeparator()}");
            Console.WriteLine($"  datatype sep:    {GetDatatypeSeparator()}");
            Console.WriteLine($"  array value sep: {GetArraySeparator()}");
        }
    }
}
