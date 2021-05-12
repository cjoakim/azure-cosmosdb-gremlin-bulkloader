// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

// Chris Joakim, Microsoft, May 2021

namespace CosmosGemlinBulkLoader.Csv
{
    using System;

    /**
     * The first row of each input file must be a tradititional CSV Header Row.
     * This row is parsed into an instance of class HeaderRow, which contains
     * a collection of CsvField objects.
     * 
     * Class CsvField defines the index, name, and optional datatype for each
     * csv header field, as well as validation logic.
     */

    public class CsvField
    {
        // Instance variables
        protected int index;
        protected string name;
        protected string datatype;
        protected string csvHelperKey;
        protected bool valid = false;

        public CsvField(int index, string name, string datatype, string key=null)
        {
            this.index = index;
            this.name = name.Trim();
            this.datatype = null;

            if (key == null)
            {
                this.csvHelperKey = this.name;
            }
            else
            {
                this.csvHelperKey = key;
            }

            switch (datatype.ToLower().Trim())
            {
                // Accept only predefined datatype values
                case "string":
                    this.datatype = datatype.ToLower().Trim();
                    break;
                case "string[]":
                    this.datatype = datatype.ToLower().Trim();
                    break;
                case "int":
                    this.datatype = datatype.ToLower().Trim();
                    break;
                case "long":
                    this.datatype = datatype.ToLower().Trim();
                    break;
                case "double":
                    this.datatype = datatype.ToLower().Trim();
                    break;
                case "bool":
                    this.datatype = datatype.ToLower().Trim();
                    break;
            }
            Validate();
        }

        public bool IsValid()
        {
            return valid;
        }

        private void Validate()
        {
            valid = true;

            if (name.Length < 1)
            {
                valid = false;
            }
            if (name.IndexOfAny(" ,|!@#$%^&*()+={}[]~\t`".ToCharArray()) >= 0)
            {
                valid = false;
            }
            if (datatype == null)
            {
                valid = false;
            }
        }

        public int GetIndex()
        {
            return index;
        }

        public string GetName()
        {
            return name;
        }

        public string GetDatatype()
        {
            return datatype;
        }

        public string GetCsvHelperKey()
        {
            return csvHelperKey;
        }

        public override string ToString()
        {
            return String.Format(
                "CsvField index: {0}, key: {1}, name: {2}, datatype: {3}, valid: {4}",
                index, csvHelperKey, name, datatype, valid);
        }
    }
}
