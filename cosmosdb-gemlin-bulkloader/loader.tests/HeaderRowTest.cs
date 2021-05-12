// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using Xunit;

using CosmosGemlinBulkLoader;
using CosmosGemlinBulkLoader.Csv;

// dotnet test --filter loader.tests.HeaderRowTest.MalformedVertexHeaderTest

// TODO - revisit these tests after refactoring program to use CsvHelper.

namespace loader.tests
{
    public class HeaderRowTest
    {
    //    [Fact]
    //    public void TokenizeFieldNameTypeTest()
    //    {
    //        string line = "";
    //        string fileType = Config.FILE_TYPE_VERTEX;
    //        char fieldSep = Config.DEFAULT_CSV_FIELD_SEPARATOR;
    //        char datatypeSep = Config.DEFAULT_DATATYPE_SEPARATOR;
    //        HeaderRow hr = new HeaderRow("in.csv", line, fileType, fieldSep, datatypeSep, true);

    //        string[] tokens = hr.TokenizeFieldNameType("Id");
    //        Assert.True(tokens.Length == 2);
    //        Assert.True(tokens[0] == "Id");
    //        Assert.True(tokens[1] == "string");

    //        tokens = hr.TokenizeFieldNameType("Altitude:double");
    //        Assert.True(tokens.Length == 2);
    //        Assert.True(tokens[0] == "Altitude");
    //        Assert.True(tokens[1] == "double");

    //        tokens = hr.TokenizeFieldNameType("Altitude:");
    //        Assert.True(tokens.Length == 2);
    //        Assert.True(tokens[0] == "Altitude");
    //        Assert.True(tokens[1] == "");

    //        hr = new HeaderRow("in.csv", line, fileType, fieldSep, datatypeSep, true);
    //        tokens = hr.TokenizeFieldNameType("Altitude:double:oops");
    //        Assert.True(tokens.Length == 3);
    //        Assert.True(tokens[0] == "Altitude");
    //        Assert.True(tokens[1] == "double");
    //        Assert.True(tokens[2] == "oops");
    //        Assert.True(hr.GetErrors().Count > 0);
    //        Assert.True(ErrorMessagePresent("malformed header field 'Altitude:double:oops'", hr));
    //        hr.Display();
    //    }

    //    [Fact]
    //    public void InvalidFiletypeTest()
    //    {
    //        string line = "";
    //        string fileType = "node";
    //        char fieldSep = Config.DEFAULT_CSV_FIELD_SEPARATOR;
    //        char datatypeSep = Config.DEFAULT_DATATYPE_SEPARATOR;

    //        HeaderRow hr = new HeaderRow("in.csv", line, fileType, fieldSep, datatypeSep);
    //        DisplayErrors(hr);

    //        Assert.False(hr.IsValid());
    //        Assert.True(ErrorMessagePresent("Invalid fileType 'node' given", hr));
    //    }

    //    [Fact]
    //    public void EmptyVertexHeaderTest()
    //    {
    //        string line = "";
    //        string fileType = Config.FILE_TYPE_VERTEX;
    //        char fieldSep = Config.DEFAULT_CSV_FIELD_SEPARATOR;
    //        char datatypeSep = Config.DEFAULT_DATATYPE_SEPARATOR;

    //        HeaderRow hr = new HeaderRow("in.csv", line, fileType, fieldSep, datatypeSep);
    //        DisplayErrors(hr);

    //        Assert.False(hr.IsValid());
    //        Assert.True(ErrorMessagePresent("vertex header row contains too few fields", hr));
    //    }

    //    [Fact]
    //    public void EmptyEdgeHeaderTest()
    //    {
    //        string line = "";
    //        string fileType = Config.FILE_TYPE_EDGE;
    //        char fieldSep = Config.DEFAULT_CSV_FIELD_SEPARATOR;
    //        char datatypeSep = Config.DEFAULT_DATATYPE_SEPARATOR;

    //        HeaderRow hr = new HeaderRow("in.csv", line, fileType, fieldSep, datatypeSep);
    //        DisplayErrors(hr);

    //        Assert.False(hr.IsValid());
    //        Assert.True(ErrorMessagePresent("edge header row contains too few fields", hr));
    //    }

    //    [Fact]
    //    public void MalformedVertexHeaderTest()
    //    {
    //        string fileType = Config.FILE_TYPE_VERTEX;
    //        char fieldSep = Config.DEFAULT_CSV_FIELD_SEPARATOR;
    //        char datatypeSep = Config.DEFAULT_DATATYPE_SEPARATOR;

    //        HeaderRow hr = new HeaderRow("in.csv", "Oops,Pk,Label", fileType, fieldSep, datatypeSep);
    //        DisplayErrors(hr);
    //        Assert.False(hr.IsValid());
    //        Assert.True(ErrorMessagePresent("field at index 0 should be named Id not Oops", hr));

    //        hr = new HeaderRow("in.csv", "Id,Oops,Label", fileType, fieldSep, datatypeSep);
    //        DisplayErrors(hr);
    //        Assert.False(hr.IsValid());
    //        Assert.True(ErrorMessagePresent("field at index 1 should be named Pk not Oops", hr));

    //        hr = new HeaderRow("in.csv", "Id,Pk,Oops", fileType, fieldSep, datatypeSep);
    //        DisplayErrors(hr);
    //        Assert.False(hr.IsValid());
    //        Assert.True(ErrorMessagePresent("field at index 2 should be named Label not Oops", hr));

    //        hr = new HeaderRow("in.csv", "Id,Pk,Oops:decimal", fileType, fieldSep, datatypeSep);
    //        DisplayErrors(hr);
    //        DisplayFields(hr);
    //        Assert.False(hr.IsValid());
    //        Assert.True(hr.GetErrors().Count == 1);
    //        Assert.True(ErrorMessagePresent("field at index 2 should be named Label not Oops", hr));
    //        //Assert.True(ErrorMessagePresent("CsvField is invalid; index: 2 name: Label", hr));
    //    }

    //    [Fact]
    //    public void MalformedEdgeHeaderTest()
    //    {
    //        string fileType = Config.FILE_TYPE_EDGE;
    //        char fieldSep = Config.DEFAULT_CSV_FIELD_SEPARATOR;
    //        char datatypeSep = Config.DEFAULT_DATATYPE_SEPARATOR;

    //        HeaderRow hr = new HeaderRow("in.csv", "f1,f2,f3,f4,f5,f6,f7,f8,ToVertexLabel:none", fileType, fieldSep, datatypeSep);
    //        DisplayErrors(hr);
    //        Assert.False(hr.IsValid());
    //        Assert.True(ErrorMessagePresent("field at index 0 should be named EdgeId not f1", hr));
    //        Assert.True(ErrorMessagePresent("field at index 1 should be named EdgePk not f2", hr));
    //        Assert.True(ErrorMessagePresent("field at index 2 should be named EdgeLabel not f3", hr));
    //        Assert.True(ErrorMessagePresent("field at index 3 should be named FromVertexId not f4", hr));
    //        Assert.True(ErrorMessagePresent("field at index 4 should be named FromVertexPk not f5", hr));
    //        Assert.True(ErrorMessagePresent("field at index 5 should be named FromVertexLabel not f6", hr));
    //        Assert.True(ErrorMessagePresent("field at index 6 should be named ToVertexId not f7", hr));
    //        Assert.True(ErrorMessagePresent("field at index 7 should be named ToVertexPk not f8", hr));
    //        Assert.True(ErrorMessagePresent("CsvField is invalid; index: 8 name: ToVertexLabel", hr));
    //    }

    //    [Fact]
    //    public void WellFormedSimpleVertexHeaderTest()
    //    {
    //        string fileType = Config.FILE_TYPE_VERTEX;
    //        char fieldSep = Config.DEFAULT_CSV_FIELD_SEPARATOR;
    //        char datatypeSep = Config.DEFAULT_DATATYPE_SEPARATOR;

    //        HeaderRow hr = new HeaderRow("in.csv", "Id,Pk,Label", fileType, fieldSep, datatypeSep);
    //        Assert.True(hr.GetInfile() == "in.csv");
    //        Assert.True(hr.GetLine() == "Id,Pk,Label");

    //        DisplayErrors(hr);
    //        Assert.True(hr.GetFileType() == Config.FILE_TYPE_VERTEX);
    //        Assert.True(hr.IsValid());
    //        Assert.True(hr.GetErrors().Count == 0);
    //        CsvField[] fields = hr.GetFields().ToArray();
    //        DisplayFields(hr);

    //        Assert.True(fields[0].GetIndex() == 0);
    //        Assert.True(fields[0].GetName() == "Id");
    //        Assert.True(fields[0].GetDatatype() == "string");

    //        Assert.True(fields[1].GetIndex() == 1);
    //        Assert.True(fields[1].GetName() == "Pk");
    //        Assert.True(fields[1].GetDatatype() == "string");

    //        Assert.True(fields[2].GetIndex() == 2);
    //        Assert.True(fields[2].GetName() == "Label");
    //        Assert.True(fields[2].GetDatatype() == "string");

    //        Assert.True(hr.GetFieldCount() == 3);
    //    }

    //    [Fact]
    //    public void WellFormedComplexVertexHeaderTest()
    //    {
    //        string fileType = Config.FILE_TYPE_VERTEX;
    //        char fieldSep = Config.DEFAULT_CSV_FIELD_SEPARATOR;
    //        char datatypeSep = Config.DEFAULT_DATATYPE_SEPARATOR;

    //        HeaderRow hr = new HeaderRow("in.csv", "Id:int,Pk:string,Label:string,Altitude:double,Distance:long", fileType, fieldSep, datatypeSep);
    //        DisplayErrors(hr);
    //        Assert.True(hr.GetFileType() == Config.FILE_TYPE_VERTEX);
    //        Assert.True(hr.IsValid());
    //        Assert.True(hr.GetErrors().Count == 0);
    //        CsvField[] fields = hr.GetFields().ToArray();
    //        DisplayFields(hr);

    //        Assert.True(fields[0].GetIndex() == 0);
    //        Assert.True(fields[0].GetName() == "Id");
    //        Assert.True(fields[0].GetDatatype() == "int");

    //        Assert.True(fields[1].GetIndex() == 1);
    //        Assert.True(fields[1].GetName() == "Pk");
    //        Assert.True(fields[1].GetDatatype() == "string");

    //        Assert.True(fields[2].GetIndex() == 2);
    //        Assert.True(fields[2].GetName() == "Label");
    //        Assert.True(fields[2].GetDatatype() == "string");

    //        Assert.True(fields[3].GetIndex() == 3);
    //        Assert.True(fields[3].GetName() == "Altitude");
    //        Assert.True(fields[3].GetDatatype() == "double");

    //        Assert.True(fields[4].GetIndex() == 4);
    //        Assert.True(fields[4].GetName() == "Distance");
    //        Assert.True(fields[4].GetDatatype() == "long");

    //        Assert.True(hr.GetFieldCount() == 5);

    //        hr.Display();
    //    }

    //    [Fact]
    //    public void WellFormedComplexEdgeHeaderTest()
    //    {
    //        string fileType = Config.FILE_TYPE_EDGE;
    //        char fieldSep = Config.DEFAULT_CSV_FIELD_SEPARATOR;
    //        char datatypeSep = Config.DEFAULT_DATATYPE_SEPARATOR;

    //        HeaderRow hr = new HeaderRow("in.csv", "EdgeId:int,EdgePk,EdgeLabel,FromVertexId,FromVertexPk,FromVertexLabel,ToVertexId,ToVertexPk,ToVertexLabel,Age:int,Distance:long,Name", fileType, fieldSep, datatypeSep);
    //        DisplayErrors(hr);
    //        Assert.True(hr.GetFileType() == Config.FILE_TYPE_EDGE);
    //        Assert.True(hr.IsValid());
    //        Assert.True(hr.GetErrors().Count == 0);
    //        CsvField[] fields = hr.GetFields().ToArray();
    //        DisplayFields(hr);

    //        Assert.True(fields[0].GetIndex() == 0);
    //        Assert.True(fields[0].GetName() == "EdgeId");
    //        Assert.True(fields[0].GetDatatype() == "int");

    //        Assert.True(fields[1].GetIndex() == 1);
    //        Assert.True(fields[1].GetName() == "EdgePk");
    //        Assert.True(fields[1].GetDatatype() == "string");

    //        Assert.True(fields[2].GetIndex() == 2);
    //        Assert.True(fields[2].GetName() == "EdgeLabel");
    //        Assert.True(fields[2].GetDatatype() == "string");

    //        Assert.True(fields[3].GetIndex() == 3);
    //        Assert.True(fields[3].GetName() == "FromVertexId");
    //        Assert.True(fields[3].GetDatatype() == "string");

    //        Assert.True(fields[4].GetIndex() == 4);
    //        Assert.True(fields[4].GetName() == "FromVertexPk");
    //        Assert.True(fields[4].GetDatatype() == "string");

    //        Assert.True(fields[5].GetIndex() == 5);
    //        Assert.True(fields[5].GetName() == "FromVertexLabel");
    //        Assert.True(fields[5].GetDatatype() == "string");

    //        Assert.True(fields[6].GetIndex() == 6);
    //        Assert.True(fields[6].GetName() == "ToVertexId");
    //        Assert.True(fields[6].GetDatatype() == "string");

    //        Assert.True(fields[7].GetIndex() == 7);
    //        Assert.True(fields[7].GetName() == "ToVertexPk");
    //        Assert.True(fields[7].GetDatatype() == "string");

    //        Assert.True(fields[8].GetIndex() == 8);
    //        Assert.True(fields[8].GetName() == "ToVertexLabel");
    //        Assert.True(fields[8].GetDatatype() == "string");

    //        Assert.True(fields[9].GetIndex() == 9);
    //        Assert.True(fields[9].GetName() == "Age");
    //        Assert.True(fields[9].GetDatatype() == "int");

    //        Assert.True(fields[10].GetIndex() == 10);
    //        Assert.True(fields[10].GetName() == "Distance");
    //        Assert.True(fields[10].GetDatatype() == "long");

    //        Assert.True(fields[11].GetIndex() == 11);
    //        Assert.True(fields[11].GetName() == "Name");
    //        Assert.True(fields[11].GetDatatype() == "string");

    //        Assert.True(hr.GetFieldCount() == 12);

    //        hr.Display();

    //        //* Field  0:   EdgeId, string
    //        //* Field  1:   EdgePk(Partition Key), string
    //        //* Field  2:   EdgeLabel, string
    //        //* Field  3:   FromVertexId, string
    //        //* Field  4:   FromVertexPk, string
    //        //* Field  5:   FromVertexLabel, string
    //        //* Field  6:   ToVertexId, string
    //        //* Field  7:   ToVertexPk, string
    //        //* Field  8:   ToVertexLabel, string
    //    }

    //    private bool ErrorMessagePresent(string msg, HeaderRow hr)
    //    {
    //        string[] errors = hr.GetErrors().ToArray();
    //        bool result = false;
    //        for (int i = 0; i < errors.Length; i++)
    //        {
    //            Console.WriteLine(errors[i]);
    //            if (errors[i].Contains(msg))
    //            {
    //                return true;
    //            }
    //        }
    //        return result;
    //    }

    //    private void DisplayErrors(HeaderRow hr)
    //    {
    //        string[] errors = hr.GetErrors().ToArray();
    //        if (errors.Length > 0)
    //        {
    //            Console.WriteLine("DisplayErrors:");
    //            for (int i = 0; i < errors.Length; i++)
    //            {
    //                Console.WriteLine("  {0}: {1}", i, errors[i]);
    //            }
    //        }
    //    }

    //    private void DisplayFields(HeaderRow hr)
    //    {
    //        CsvField[] fields = hr.GetFields().ToArray();
    //        if (fields.Length > 0)
    //        {
    //            Console.WriteLine("DisplayFields {0}:", fields.Length);
    //            for (int i = 0; i < fields.Length; i++)
    //            {
    //                Console.WriteLine("  {0}", fields[i]);
    //            }
    //        }
    //    }
    }
}
