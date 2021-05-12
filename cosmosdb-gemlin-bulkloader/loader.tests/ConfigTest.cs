// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

using System;
using System.Text;
using Xunit;
using CosmosGemlinBulkLoader;

namespace loader.tests
{
    public class ConfigTest
    {
        [Fact]
        public void GetEnvVarTest()
        {
            string[] args = { };
            Config c = new Config(args);
            Assert.True((c.GetEnvVar("???")) == null);
            Assert.True((c.GetEnvVar("???","!")) == "!");
            Assert.True((c.GetEnvVar("AZURE_COSMOSDB_GRAPHDB_CONN_STRING", "")).Length > 0);
            Assert.True((c.GetEnvVar("AZURE_STORAGE_CONNECTION_STRING", "")).Length > 0);
        }

        [Fact]
        public void RequiredEnvironmentVariablesTest()
        {
            string[] args = { };
            Config c = new Config(args);

            // This test assumes that the environment variables have been set
            // and have reasonable, but not exact, values.

            // https://xunit.net/xunit.analyzers/rules/xUnit2009
            Assert.StartsWith("AccountEndpoint=https://", c.GetCosmosConnString());

            Assert.True(c.GetCosmosConnString().IndexOf(".documents.azure.com:443/;AccountKey=") > 0);
            Assert.True(c.GetCosmosDbName().Length > 2);
            Assert.True(c.GetCosmosGraphName().Length > 2);
            Assert.True(c.GetStorageConnString().IndexOf("DefaultEndpointsProtocol=https;AccountName=") == 0);
        }

        [Fact]
        public void DefaultSeparatorsTest()
        {
            string[] args = { };
            Config c = new Config(args);
            c.SetCliArgs("");

            Assert.True((c.GetCsvFieldSeparator()) == ',');
            Assert.True((c.GetDatatypeSeparator()) == ':');
            Assert.True((c.GetArraySeparator())    == '^');
        }
        
        [Fact]
        public void FlagsTest()
        {
            string[] args = { };
            Config c = new Config(args);
            Assert.False(c.IsVerbose());
            Assert.False(c.DoLoad());
            Assert.False(c.DoPreprocess());
            Assert.False(c.IsVertexFiletype());
            Assert.False(c.IsEdgeFiletype());

            args = "load --verbose --file-type vertex".Split();
            c = new Config(args);
            Assert.True(c.IsVerbose());
            Assert.True(c.DoLoad());
            Assert.False(c.DoPreprocess());
            Assert.True(c.IsVertexFiletype());
            Assert.False(c.IsEdgeFiletype());

            args = "preprocess --verbose --file-type edge".Split();
            c = new Config(args);
            Assert.True(c.IsVerbose());
            Assert.False(c.DoLoad());
            Assert.True(c.DoPreprocess());
            Assert.False(c.IsVertexFiletype());
            Assert.True(c.IsEdgeFiletype());
        }

        [Fact]
        public void BatchSizeTest()
        {
            string[] args = { };
            Config c = new Config(args);
            Assert.True(c.GetBatchSize() == 25000);

            args = "load --batch-size".Split();
            c = new Config(args);
            Assert.True(c.GetBatchSize() == Config.DEFAULT_BATCH_SIZE);

            args = "load --batch-size NaN".Split();
            c = new Config(args);
            Assert.True(c.GetBatchSize() == Config.DEFAULT_BATCH_SIZE);

            args = "load --batch-size 100000".Split();
            c = new Config(args);
            Assert.True(c.GetBatchSize() == 100000);
        }

        [Fact]
        public void PartitionKeyTest()
        {
            string[] args = { };
            Config c = new Config(args);
            Assert.True(c.GetPartitionKeyAttr() == Config.DEFAULT_PARTITON_KEY_ATTR);

            args = "load --partition-key objectId".Split();
            c = new Config(args);
            Assert.True(c.GetBatchSize() == Config.DEFAULT_BATCH_SIZE);
            Assert.True(c.GetPartitionKeyAttr() == "objectId");
        }

        [Fact]
        public void InvalidArgsTest()
        {
            string[] args = { };
            Config c = new Config(args);
            Assert.False(c.IsValid());

            args = "load --file-type".Split();
            c = new Config(args);
            Assert.True(c.DoLoad());
            Assert.True(c.GetFileType() == null);
            Assert.False(c.IsValid());

            args = "load --file-type incorrect".Split();
            c = new Config(args);
            Assert.False(c.IsValid());

            args = "load --file-type vertex --infile".Split();
            c = new Config(args);
            Assert.False(c.IsValid());
            Assert.True(c.GetFileType() == "vertex");

            args = "load --file-type edge --csv-infile".Split();
            c = new Config(args);
            Assert.False(c.IsValid());
            Assert.True(c.GetFileType() == "edge");
            Assert.True(c.GetCsvInfile() == null);
        }

        [Fact]
        public void FileArgsTest()
        {
            string[] args = "load --file-type edge --csv-infile".Split();
            Config c = new Config(args);
            Assert.True(c.GetCliArgs() == args);
            Assert.False(c.IsValid());
            Assert.False(c.IsBlobInput());
            
            // valid file input
            args = "load --file-type edge --csv-infile data/edge.csv".Split();
            c = new Config(args);
            Assert.True(c.GetCliArgs() == args);
            Assert.True(c.IsValid());
            Assert.False(c.IsBlobInput());
            Assert.True(c.GetFileType() == "edge");
            Assert.True(c.GetCsvInfile() == "data/edge.csv");
            
            c.Display();
        }
        
        [Fact]
        public void BlobArgsTest()
        {
            string[] args = "load --file-type vertex --blob-container".Split();
            Config c = new Config(args);
            Assert.False(c.IsValid());
            Assert.False(c.IsBlobInput());

            args = "load --file-type vertex --blob-container xxx".Split();
            c = new Config(args);
            Assert.False(c.IsValid());
            Assert.False(c.IsBlobInput());
            
            args = "load --file-type vertex --blob-container xxx --blob-name".Split();
            c = new Config(args);
            Assert.False(c.IsValid());
            Assert.False(c.IsBlobInput());
            
            // valid blob input
            args = "load --file-type vertex --blob-container bulkloader --blob-name imdb/movie_vertices.csv".Split();
            c = new Config(args);
            Assert.True(c.IsValid());
            Assert.True(c.IsBlobInput());
            Assert.True(c.GetFileType() == "vertex");
            Assert.True(c.GetStorageContainerName() == "bulkloader");
            Assert.True(c.GetStorageBlobName() == "imdb/movie_vertices.csv");
        }
        
        [Fact]
        public void SimulatedCommandLineTest()
        {
            string[] args = { };
            Config c = new Config(args);

            StringBuilder s = new StringBuilder("");
            s.Append("--csv-infile /data/vertex.csv ");
            s.Append("--csv-field-sep ^ ");
            s.Append("--datatype-sep ! ");
            s.Append("--array-sep @ ");
            s.Append("--file-type vertex ");
            s.Append("--cat elsa ");
            s.Append("--end-of-array-nothing-follows");
            c.SetCliArgs(s.ToString());
            Console.WriteLine(s.ToString());

            Assert.True((c.GetCsvInfile()) == "/data/vertex.csv");
            Assert.True((c.GetCsvFieldSeparator()) == '^');
            Assert.True((c.GetDatatypeSeparator()) == '!');
            Assert.True((c.GetArraySeparator())    == '@');
            Assert.True((c.GetFileType()) == "vertex");
            Assert.True((c.GetCliKeywordArg("--cat")) == "elsa");
            Assert.True((c.GetCliKeywordArg("--oops")) == null);
            Assert.True((c.GetCliKeywordArg("--oops","42")) == "42");
            Assert.True((c.GetCliKeywordArg("--end-of-array-nothing-follows")) == null);
        }
    }
}
