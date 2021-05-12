// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

// Chris Joakim, Microsoft, May 2021

using CosmosGemlinBulkLoader.Element;

namespace CosmosGemlinBulkLoader.Csv
{
    using System;
    using System.Collections.Generic;

    /**
     * The first row of each input file must be a tradititional CSV Header Row.
     * This row is parsed into an instance of class HeaderRow, which contains
     * a collection of CsvField objects.
     * 
     * This CosmosGemlinBulkLoader program supports two types of input files, Vertices or Edges.
     * The type is determined from the "--file-type" command-line arg.  
     * 
     * The CosmosGemlinBulkLoader program is prescriptive on the format of the header rows
     * and input files, as follows:
     * 
     * CSV Field Separator: You can specify this value with the "--csv-field-sep" command-line arg,
     * it defaults to a comma.
     * 
     * Property-Datatype Separator: You can specify this value with the "--datatype-sep" command-line arg,
     * it defaults to a vertibar (:).  This allows you to define Property names AND their datatype
     * in the CSV header line like this 'population:long'.
     * 
     * Array-Value Separator: You can specify this value with the "--array-sep" command-line arg,
     * it defaults to a caret (^).  This is used for Vertex or Edge Properties of type 'string[]'
     * (i.e. - string array).  For example: 'cities:Charlotte^Redmond^New Delhi'.
     * 
     * Vertex Files:
     *   Field  0:   Id, string
     *   Field  1:   Pk (Partition Key), string
     *   Field  2:   Label, string
     *   Fields 3-n: Properties
     *     Format 1:  <vertex-property-name>
     *     Format 2:  <vertex-property-name><datatype-separator><data-type>
     *     <vertex-property-name> should have no embedded spaces
     *     <separator> This should be the value of --datatype-sep
     *     <data-type> is one of 'string','string[]','int','long', 'double' or 'bool'; defaults to 'string'
     * 
     * Example:
     *    
     * 
     * Edge Files:
     *   Field  0:   EdgeId, string
     *   Field  1:   EdgePk (Partition Key), string
     *   Field  2:   EdgeLabel, string
     *   Field  3:   FromVertexId, string
     *   Field  4:   FromVertexPk, string
     *   Field  5:   FromVertexLabel, string
     *   Field  6:   ToVertexId, string
     *   Field  7:   ToVertexPk, string
     *   Field  8:   ToVertexLabel, string
     *   Fields 9-n: Properties
     *     Format 1:  <vertex-property-name>
     *     Format 2:  <vertex-property-name><datatype-separator><data-type>
     *     <vertex-property-name> should have no embedded spaces
     *     <separator> This should be the value of --datatype-sep
     *     <data-type> is one of 'string','string[]','int','long', 'double' or 'bool'; defaults to 'string
     */

    public class HeaderRow
    {
        // Instance variables
        protected string source;
        protected string[] headerFields;
        protected string fileType;
        protected char   fieldSep;
        protected char   datatypeSep;
        protected List<CsvField> fields = null;
        protected CsvField[] fieldsArray = null;
        protected List<string> errors = null;
        protected bool verbose = false;


        public HeaderRow(string source, string[] headerFields, string fileType, char fieldSep, char datatypeSep, bool verbose = false)
        {
            this.source = source;
            this.headerFields = headerFields;
            this.fieldSep = fieldSep;
            this.fileType = fileType;
            this.datatypeSep = datatypeSep;
            this.fields = new List<CsvField>();
            this.errors = new List<string>();
            this.verbose = verbose;

            switch (this.fileType)
            {
                case Config.FILE_TYPE_VERTEX:
                    ParseVertexHeader();
                    break;
                case Config.FILE_TYPE_EDGE:
                    ParseEdgeHeader();
                    break;
                default:
                    errors.Add(
                        String.Format("Invalid fileType '{0}' given, see the {1} command-line arg",
                        this.fileType, Config.FILE_TYPE_KEYWORD));
                    break;
            }
        }

        public bool IsValid()
        {
            foreach (CsvField field in fields)
            {
                if (!field.IsValid())
                {
                    errors.Add(
                        String.Format("CsvField is invalid; index: {0} name: {1} datatype: {2}",
                        field.GetIndex(), field.GetName(), field.GetDatatype()));
                }
            }
            return errors.Count == 0;
        }

        public string GetSource()
        {
            return source;
        }

        public string GetFileType()
        {
            return fileType;
        }

        public List<string> GetErrors()
        {
            return errors;
        }

        public List<CsvField> GetFields()
        {
            return fields;
        }

        public CsvField[] GetFieldsArray()
        {
            if (fieldsArray  == null)
            {
                fieldsArray = fields.ToArray();
            }
            return fieldsArray;
        }

        public int GetFieldCount()
        {
            return fields.Count;
        }

        public void Display()
        {
            Console.WriteLine("HeaderRow:");
            Console.WriteLine($"  source:       {source}");
            Console.WriteLine($"  fileType:     {fileType}");
            Console.WriteLine($"  fieldSep:     {fieldSep}");
            Console.WriteLine($"  datatypeSep:  {datatypeSep}");
            Console.WriteLine($"  field count:  {fields.Count}");
            Console.WriteLine($"  errors count: {errors.Count}");

            for (int i = 0; i < GetFieldsArray().Length; i++)
            {
                Console.WriteLine($"  {fieldsArray[i].ToString()}");
            }
            foreach (string error in errors)
            {
                Console.WriteLine($"  {error}");
            }
        }

        private void ParseVertexHeader()
        {
            if (headerFields.Length < Config.MIN_VERTEX_ROW_FIELD_COUNT)
            {
                errors.Add("vertex header row contains too few fields");
                return;
            }

            for (int i = 0; i < headerFields.Length; i++)
            {
                string token = headerFields[i];

                switch (i)
                {
                    case 0:
                        AddRequiredField(i, "Id", token);
                        break;
                    case 1:
                        AddRequiredField(i, "Pk", token);
                        break;
                    case 2:
                        AddRequiredField(i, "Label", token);
                        break;
                    default:
                        AddPropertyField(i, token);
                        break;
                }
            }
        }

        private void ParseEdgeHeader()
        {
            if (headerFields.Length < Config.MIN_VERTEX_ROW_FIELD_COUNT)
            {
                errors.Add("edge header row contains too few fields");
                return;
            }

            for (int i = 0; i < headerFields.Length; i++)
            {
                string token = headerFields[i];

                switch (i)
                {
                    case 0:
                        AddRequiredField(i, "EdgeId", token);
                        break;
                    case 1:
                        AddRequiredField(i, "EdgePk", token);
                        break;
                    case 2:
                        AddRequiredField(i, "EdgeLabel", token);
                        break;
                    case 3:
                        AddRequiredField(i, "FromVertexId", token);
                        break;
                    case 4:
                        AddRequiredField(i, "FromVertexPk", token);
                        break;
                    case 5:
                        AddRequiredField(i, "FromVertexLabel", token);
                        break;
                    case 6:
                        AddRequiredField(i, "ToVertexId", token);
                        break;
                    case 7:
                        AddRequiredField(i, "ToVertexPk", token);
                        break;
                    case 8:
                        AddRequiredField(i, "ToVertexLabel", token);
                        break;
                    default:
                        AddPropertyField(i, token);
                        break;
                }
            }
        }

        private void AddRequiredField(int index, string name, string token)
        {
            string[] nameTypeTokens = TokenizeFieldNameType(token);
            string nameToken = nameTypeTokens[0];
            string typeToken = nameTypeTokens[1];

            if (name.ToLower().Trim() == nameToken.ToLower().Trim())
            {
                fields.Add(new CsvField(index, nameToken, typeToken));
            }
            else
            {
                errors.Add(
                    String.Format("field at index {0} should be named {1} not {2}", index, name, nameToken));
            }
        }

        private void AddPropertyField(int index, string token)
        {
            string[] nameTypeTokens = TokenizeFieldNameType(token.Trim());
            string nameToken = nameTypeTokens[0];
            string typeToken = nameTypeTokens[1];

            fields.Add(new CsvField(index, nameToken, typeToken, token.Trim()));
        }

        /**
         * Split the given string and return a two-element string[]
         */
        public string[] TokenizeFieldNameType(string token)
        {
            if (token.Contains(datatypeSep))
            {
                string[] subtokens = token.Split(datatypeSep);
                if (verbose)
                {
                    Console.WriteLine(String.Format("TokenizeFieldNameType '{0}' {1}", token, subtokens.Length));
                    foreach (string tok in subtokens)
                    {
                        Console.WriteLine("  TokenizeFieldNameType, tok: " + tok);
                    }
                }
                if (subtokens.Length > 2)
                {
                    errors.Add(String.Format("malformed header field '{0}'", token));
                }
                return subtokens;
            }
            else
            {
                return new string[] { token, "string" };
            }
        }
    }
}
