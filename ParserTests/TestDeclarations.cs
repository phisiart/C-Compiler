using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Parsing;
using static ParserTestUtils;

[TestFixture]
public class TestDeclarations {
    [Test]
    // TODO: how to test this?
    public void TypeDefName() {
    
    }

    [Test]
    public void TypeName() {

    }

    [Test]
    public void TypeSpecifier() {
        TestParserRule(
            CParser.TypeSpecifier,
            "void",
            "char",
            "short",
            "int",
            "long",
            "float",
            "double",
            "signed",
            "unsigned",
            "struct MyStruct",
            "enum MyEnum"
            // add typedef-name
        );
    }

    [Test]
    public void TypeQualifier() {
        TestParserRule(
            CParser.TypeQualifier,
            "const",
            "volatile"
        );
    }
}