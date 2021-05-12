// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

// Chris Joakim, Microsoft, May 2021

using System.Collections.Generic;
using CosmosGemlinBulkLoader.Element;

namespace CosmosGemlinBulkLoader.Csv
{
    using System;

    /**
     * This class is used to parse a CSV input row into an IGremlinElement - 
     * either a GremlinVertex or GremlinEdge, using the given HeaderRow object
     * which defines the fields within the csv row.  
     * 
     * The Vertex and Edge line definitions each contain required fields which
     * must be in a defined sequence.  These values are of the string datatype.
     * 
     * However, after the required fields, these CSV lines may include additional
     * vertex/edge Properties, and these may be parsed into one of several 
     * datatypes - string, string[], int, long, double, or bool.
     */

    public class CsvRowParser
    {
        // Instance variables
        protected HeaderRow headerRow = null;
        protected Config config;
        protected string fileType = null;
        protected int fieldCount = 0;

        public CsvRowParser(Config config)
        {
            this.config = config;
            this.fileType = config.GetFileType();
        }

        public void SetHeaderRow(HeaderRow header)
        {
            this.headerRow = header;
            this.fieldCount = header.GetFieldCount();
        }

        public IGremlinElement ParseRow(IDictionary<string, object> dict)
        {
            if (fileType == Config.FILE_TYPE_VERTEX)
            {
                return ParseVertexRow(dict);
            }
            else
            {
                return ParseEdgeRow(dict);
            }
        }

        public GremlinVertex ParseVertexRow(IDictionary<string, object> dict)
        {
            if (dict.Count == headerRow.GetFieldCount())
            {
                try
                {
                    string id = GetFieldValue(dict, 0);
                    string pk = GetFieldValue(dict, 1);
                    string label = GetFieldValue(dict, 2);

                    GremlinVertex v = new GremlinVertex(id, label);
                    v.AddProperty(config.GetPartitionKeyAttr(), pk);

                    if (headerRow.GetFieldCount() > Config.MIN_VERTEX_ROW_FIELD_COUNT)
                    {
                        for (int i = Config.MIN_VERTEX_ROW_FIELD_COUNT; i < fieldCount; i++)
                        {
                            string name = headerRow.GetFieldsArray()[i].GetName();
                            string datattype = headerRow.GetFieldsArray()[i].GetDatatype();
                            string strVal = GetFieldValue(dict, i);

                            switch (datattype)
                            {
                                case "int":
                                    v.AddProperty(name, int.Parse(strVal));
                                    break;
                                case "long":
                                    v.AddProperty(name, long.Parse(strVal));
                                    break;
                                case "double":
                                    v.AddProperty(name, double.Parse(strVal));
                                    break;
                                case "bool":
                                    v.AddProperty(name, bool.Parse(strVal));
                                    break;
                                case "string":
                                    v.AddProperty(name, strVal);
                                    break;
                                case "string[]":
                                    v.AddProperty(name, strVal.Split(config.GetArraySeparator()));
                                    break;
                            }
                        }
                    }
                    return v;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
            }
            return null;
        }

        public GremlinEdge ParseEdgeRow(IDictionary<string, object> dict)
        {
            if (dict.Count == headerRow.GetFieldCount())
            {
                try
                {
                    string id = GetFieldValue(dict, 0);
                    string pk = GetFieldValue(dict, 1);
                    string label = GetFieldValue(dict, 2);

                    string edgeId = GetFieldValue(dict, 0);
                    string edgePk = GetFieldValue(dict, 1);
                    string edgeLabel = GetFieldValue(dict, 2);
                    string fromVertexId = GetFieldValue(dict, 3);
                    string fromVertexPk = GetFieldValue(dict, 4);
                    string fromVertexLabel = GetFieldValue(dict, 5);
                    string toVertexId = GetFieldValue(dict, 6);
                    string toVertexPk = GetFieldValue(dict, 7);
                    string toVertexLabel = GetFieldValue(dict, 8);

                    GremlinEdge e = new GremlinEdge(
                        edgeId,
                        edgeLabel,
                        fromVertexId,
                        toVertexId,
                        fromVertexLabel,
                        toVertexLabel,
                        fromVertexPk,
                        toVertexPk
                    );
                    //e.AddProperty(config.GetPartitionKeyAttr(), pk);

                    if (fieldCount > Config.MIN_EDGE_ROW_FIELD_COUNT)
                    {
                        for (int i = Config.MIN_EDGE_ROW_FIELD_COUNT; i < fieldCount; i++)
                        {
                            string name = headerRow.GetFieldsArray()[i].GetName();
                            string datattype = headerRow.GetFieldsArray()[i].GetDatatype();
                            string strVal = GetFieldValue(dict, i);

                            switch (datattype)
                            {
                                case "int":
                                    e.AddProperty(name, int.Parse(strVal));
                                    break;
                                case "long":
                                    e.AddProperty(name, long.Parse(strVal));
                                    break;
                                case "double":
                                    e.AddProperty(name, double.Parse(strVal));
                                    break;
                                case "bool":
                                    e.AddProperty(name, bool.Parse(strVal));
                                    break;
                                case "string":
                                    e.AddProperty(name, strVal);
                                    break;
                                case "string[]":
                                    e.AddProperty(name, strVal.Split(config.GetArraySeparator()));
                                    break;
                            }
                        }
                    }
                    return e;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
            }
            return null;
        }

        private string GetFieldValue(IDictionary<string, object> dict, int fieldIndex)
        {
            return ((string) dict[headerRow.GetFieldsArray()[fieldIndex].GetCsvHelperKey()]).Trim();
        }
    }
}
