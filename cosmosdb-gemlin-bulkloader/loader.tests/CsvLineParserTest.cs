// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

using Xunit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CosmosGemlinBulkLoader;
using CosmosGemlinBulkLoader.Csv;
using CosmosGemlinBulkLoader.Element;
using Newtonsoft.Json;

// TODO - revisit these tests after refactoring program to use CsvHelper.

namespace loader.tests
{
    public class CsvLineParserTest
    {
//        [Fact]
//        public void VertexTest()
//        {
//            string fileType = Config.FILE_TYPE_VERTEX;
//            char fieldSep = Config.DEFAULT_CSV_FIELD_SEPARATOR;
//            char datatypeSep = Config.DEFAULT_DATATYPE_SEPARATOR;

//            string[] args = "load --verbose --file-type vertex".Split();
//            Config config = new Config(args);

//            string header = "Id,Pk,Label,i:int,l:long,d:double,b:bool,sa:string[],s:string";
//            HeaderRow hr = new HeaderRow("in.csv", header, fileType, fieldSep, datatypeSep);
//            Assert.True(hr.GetFileType() == Config.FILE_TYPE_VERTEX);
//            Assert.True(hr.IsValid());

//            CsvLineParser parser = new CsvLineParser(config);
//            parser.SetHeaderRow(hr);

//            // a well-formed line
//            string csvLine = "CLT,CLT,City,740,740000,123.45678,true,a^b^c,cosmos";
//            IGremlinElement element = parser.parseDataLine(csvLine);
//            GremlinVertex v = (GremlinVertex) element;
//            Assert.True(v != null);
//            Assert.True(v.GetType().Name == "GremlinVertex");
//            //CaptureFixture("CLT Vertex", v);
//            string actualJson = JsonConvert.SerializeObject(v, Formatting.Indented);
//            string expectedJson = ExpectedCltVertex();
//            Assert.True(actualJson == expectedJson);

//            // a malformed line, xxx123.45678 is not a double
//            csvLine = "CLT,CLT,City,740,740000,xxx123.45678,true,a^b^c,cosmos";
//            element = parser.parseDataLine(csvLine);
//            Assert.True(element == null);
//        }

//        [Fact]
//        public void EdgeTest()
//        {
//            string fileType = Config.FILE_TYPE_EDGE;
//            char fieldSep = Config.DEFAULT_CSV_FIELD_SEPARATOR;
//            char datatypeSep = Config.DEFAULT_DATATYPE_SEPARATOR;

//            string[] args = "load --verbose --file-type edge".Split();
//            Config config = new Config(args);

//            // the 9 required fields, plus one property = 10 fields
//            string header = "EdgeId,EdgePk,EdgeLabel,FromVertexId,FromVertexPk,FromVertexLabel,ToVertexId,ToVertexPk,ToVertexLabel,i:int,l:long,d:double,b:bool,sa:string[],s:string";
//            HeaderRow hr = new HeaderRow("in.csv", header, fileType, fieldSep, datatypeSep);

//            Assert.True(hr.GetFileType() == Config.FILE_TYPE_EDGE);
//            Assert.True(hr.IsValid());
//            Assert.True(hr.GetFieldCount() == 15);

//            CsvLineParser parser = new CsvLineParser(config);
//            parser.SetHeaderRow(hr);

//            // a well-formed line
//            string csvLine = "POSTALCODE-STATE-00501-NY,POSTALCODE-STATE-00501-NY,PostalCodeInState,PC-00501,PC-00501,PostalCodeInState,ST-NY,ST-NY,PostalCodeInState,740,740000,123.45678,false,a^b^c,cosmos";
//            IGremlinElement element = parser.parseDataLine(csvLine);
//            GremlinEdge e = (GremlinEdge) element;
//            Assert.True(e != null);
//            Assert.True(e.GetType().Name == "GremlinEdge");
//            //hr.Display();
//            //CaptureFixture("PostalCodeEdge", e);

//            string actualJson = JsonConvert.SerializeObject(e, Formatting.Indented);
//            string expectedJson = ExpectedPostalCodeEdge();
//            Assert.True(actualJson == expectedJson);

//            // a malformed line, qq740000 is not a double
//            csvLine = "POSTALCODE-STATE-00501-NY,POSTALCODE-STATE-00501-NY,PostalCodeInState,PC-00501,PC-00501,PostalCodeInState,ST-NY,ST-NY,PostalCodeInState,740,qq740000,123.45678,false,a^b^c,cosmos";
//            element = parser.parseDataLine(csvLine);
//            Assert.True(element == null);
//        }

//        // Fixture methods below:

//        private string ExpectedCltVertex()
//        {
//            return @"
//{
//  |Id|: |CLT|,
//  |Label|: |City|,
//  |Properties|: {
//    |pk|: [
//      {
//        |Id|: null,
//        |Key|: |pk|,
//        |Value|: |CLT|
//      }
//    ],
//    |i|: [
//      {
//        |Id|: null,
//        |Key|: |i|,
//        |Value|: 740
//      }
//    ],
//    |l|: [
//      {
//        |Id|: null,
//        |Key|: |l|,
//        |Value|: 740000
//      }
//    ],
//    |d|: [
//      {
//        |Id|: null,
//        |Key|: |d|,
//        |Value|: 123.45678
//      }
//    ],
//    |b|: [
//      {
//        |Id|: null,
//        |Key|: |b|,
//        |Value|: true
//      }
//    ],
//    |sa|: [
//      {
//        |Id|: null,
//        |Key|: |sa|,
//        |Value|: [
//          |a|,
//          |b|,
//          |c|
//        ]
//      }
//    ],
//    |s|: [
//      {
//        |Id|: null,
//        |Key|: |s|,
//        |Value|: |cosmos|
//      }
//    ]
//  }
//}
//".Replace("|", "\"").Trim();
//        }

//        private string ExpectedPostalCodeEdge()
//        {
//            return @"
//{
//  |Id|: |POSTALCODE-STATE-00501-NY|,
//  |Label|: |PostalCodeInState|,
//  |InVertexId|: |ST-NY|,
//  |OutVertexId|: |PC-00501|,
//  |InVertexLabel|: |PostalCodeInState|,
//  |OutVertexLabel|: |PostalCodeInState|,
//  |InVertexPartitionKey|: |ST-NY|,
//  |OutVertexPartitionKey|: |PC-00501|,
//  |Properties|: [
//    {
//      |Key|: |i|,
//      |Value|: 740
//    },
//    {
//      |Key|: |l|,
//      |Value|: 740000
//    },
//    {
//      |Key|: |d|,
//      |Value|: 123.45678
//    },
//    {
//      |Key|: |b|,
//      |Value|: false
//    },
//    {
//      |Key|: |sa|,
//      |Value|: [
//        |a|,
//        |b|,
//        |c|
//      ]
//    },
//    {
//      |Key|: |s|,
//      |Value|: |cosmos|
//    }
//  ]
//}
//".Replace("|", "\"").Trim();
//        }

//        /**
//         * This method is used to capture an object as JSON.  It was used to create
//         * Fixture methods ExpectedCltVertex and ExpectedPostalCodeEdge, above.
//         */
//        private void CaptureFixture(string name, object obj)
//        {
//            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
//            json.Replace("\"", "|");
//            Console.WriteLine($"CaptureFixture: {name}");
//            Console.WriteLine(json.Replace("\"", "|"));
//        }
    }
}
