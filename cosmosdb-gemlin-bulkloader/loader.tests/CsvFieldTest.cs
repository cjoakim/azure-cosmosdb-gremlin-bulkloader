// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

using System;
using Xunit;
using CosmosGemlinBulkLoader;
using CosmosGemlinBulkLoader.Csv;

namespace loader.tests
{
    public class CsvFieldTest
    {
        [Fact]
        public void NameTest()
        {
            Assert.False(new CsvField(12, "", "string").IsValid());
            Assert.False(new CsvField(12, "  ", "string").IsValid());
            Assert.False(new CsvField(12, "so^2008", "string").IsValid());
            Assert.False(new CsvField(12, "so 2008 late", "string").IsValid());
            Assert.True(new CsvField(12, "fergie", "string").IsValid());
        }

        [Fact]
        public void DatatypeTest()
        {
            Assert.True(new CsvField(12, "someName", "string").IsValid());
            Assert.True(new CsvField(12, "someName", "string[]").IsValid());
            Assert.True(new CsvField(12, "someName", "int").IsValid());
            Assert.True(new CsvField(12, "someName", "long").IsValid());
            Assert.True(new CsvField(12, "someName", "double").IsValid());
            Assert.True(new CsvField(12, "someName", "bool").IsValid());

            Assert.True(new CsvField(12, "someName", " int ").IsValid());

            Assert.False(new CsvField(12, "someName", "").IsValid());
            Assert.False(new CsvField(12, "someName", "  ").IsValid());
            Assert.False(new CsvField(12, "someName", "decimal").IsValid());
        }
        
        [Fact]
        public void InstanceTest()
        {
            CsvField f = new CsvField(6, "Knopfler", "string[]");
            Assert.True(f.GetIndex() == 6);
            Assert.True(f.GetName() == "Knopfler");
            Assert.True(f.GetDatatype() == "string[]");
            Assert.True(f.GetCsvHelperKey() == "Knopfler");
            string s = f.ToString();
            string expected = "CsvField index: 6, key: Knopfler, name: Knopfler, datatype: string[], valid: True";
            Assert.True(s == expected);
            
            f = new CsvField(6, "Knopfler", "string[]", "DireStraits");
            Assert.True(f.GetIndex() == 6);
            Assert.True(f.GetName() == "Knopfler");
            Assert.True(f.GetDatatype() == "string[]");
            Assert.True(f.GetCsvHelperKey() == "DireStraits");
            s = f.ToString();
            expected = "CsvField index: 6, key: DireStraits, name: Knopfler, datatype: string[], valid: True";
            Assert.True(s == expected);
        }
    }
}
