using NUnit.Framework;
using Parsing;
using SyntaxTree;
using static ParserTestUtils;

[TestFixture]
public class TestDeclarations {

    [Test]
    public void Declaration() {
        TestParserRule(
            CParser.Declaration,
            "const int i = 3;",
            "const volatile *i = 0, (* const j)[3] = { 0 };"
        );
    }

    [Test]
    public void DeclarationSpecifiers() {
        TestParserRule(
            CParser.DeclarationSpecifiers,
            "auto int const",
            "typedef struct MyStruct volatile",
            "int auto int const short"
        );
    }

    [Test]
    public void InitDeclaratorList() {
        TestParserRule(
            CParser.InitDeclaratorList,
            "var1, var2",
            "*var1 = 0, * const var2()[]"
        );
    }

    [Test]
    public void InitDeclarator() {
        TestParserRule(
            CParser.InitDeclarator,
            "name",
            "name = 3",
            "*name = 3",
            "* const name = { 3 }"
        );
    }

    [Test]
    public void StorageClassSpecifier() {
        TestParserRule(
            CParser.StorageClassSpecifier,
            "auto",
            "register",
            "static",
            "extern",
            "typedef"
        );
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
        );

        TestParserRule(
            source: "MyAwesomeType",
            env: new Parsing.ParserEnvironment().AddSymbol("MyAwesomeType", StorageClsSpec.TYPEDEF),
            parser: CParser.TypeDefName
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

    [Test]
    public void Declarator() {
        TestParserRule(
            CParser.Declarator,
            "*name[3]",
            "* const *name",
            "name[3][]",
            "*name()"
        );
    }

    [Test]
    public void Pointer() {
        TestParserRule(
            CParser.Pointer,
            "*",
            "* const volatile",
            "* const * const *"
        );
    }

    [Test]
    public void ParameterTypeList() {
        TestParserRule(
            CParser.ParameterTypeList,
            "int a, int b, int",
            "int a, int b, int, ..."
        );
    }

    [Test]
    public void ParameterList() {
        TestParserRule(
            CParser.ParameterList,
            "int a, int b, int"
        );
    }

    [Test]
    public void TypeQualifierList() {
        TestParserRule(
            CParser.TypeQualifierList,
            "const const"
        );
    }

    [Test]
    public void DirectDeclarator() {
        TestParserRule(
            CParser.DirectDeclarator,
            "name",
            "(name)",
            "name[]",
            "name[3]",
            "name(int)",
            "name[3][4]",
            "name[][]",
            "(*name)[3](int arg)",
            "name()"
        );
    }

    [Test]
    public void EnumSpecifier() {
        TestParserRule(
            CParser.EnumSpecifier,
            "enum MyEnum",
            "enum MyEnum { NAME = 1, NAME }",
            "enum { NAME = 1, NAME }"
        );
    }

    [Test]
    public void EnumeratorList() {
        TestParserRule(
            CParser.EnumeratorList,
            "NAME, NAME = 3, NAME, NAME = 4"
        );
    }

    [Test]
    public void Enumerator() {
        TestParserRule(
            CParser.Enumerator,
            "NAME = 3",
            "NAME"
        );
    }

    [Test]
    public void EnumerationConstant() {
        TestParserRule(
            CParser.EnumerationConstant,
            "name"
        );
    }

    [Test]
    public void StructOrUnionSpecifier() {
        TestParserRule(
            CParser.StructOrUnion,
            "struct",
            "union"
        );
    }

    [Test]
    public void StructDeclarationList() {
        TestParserRule(
            CParser.StructDeclarationList,
            "int a; int b;"
        );
    }

    [Test]
    public void StructDeclaration() {
        TestParserRule(
            CParser.StructDeclaration,
            "int const a[3], b : 3, * const c(int arg);"
        );
    }

    [Test]
    public void SpecifierQualifierList() {
        TestParserRule(
            CParser.SpecifierQualifierList,
            "int const",
            "const int",
            "char char char",
            "short int const volatile"
        );
    }

    [Test]
    public void StructDeclaratorList() {
        TestParserRule(
            CParser.StructDeclaratorList,
            "name, name, name[3]",
            "name : 3, name()"
        );
    }

    [Test]
    public void StructDeclarator() {
        TestParserRule(
            CParser.StructDeclarator,
            "name",
            "name : 3"
        );
    }

    [Test]
    public void ParameterDeclaration() {
        TestParserRule(
            CParser.ParameterDeclaration,
            "register int name[3]",
            "register int[3]"
        );
    }

    [Test]
    public void AbstractDeclarator() {
        TestParserRule(
            CParser.AbstractDeclarator,
            "* const (*)()[]"
        );
    }

    [Test]
    public void DirectAbstractDeclarator() {
        TestParserRule(
            CParser.DirectAbstractDeclarator,
            "[3][]",
            "(int arg1, char arg2)()",
            "[]()",
            "(*)()"
        );
    }

    [Test]
    public void Initializer() {
        TestParserRule(
            CParser.Initializer,
            "3",
            "{ 3 }",
            "{ 3, 4, }",
            "{ 3, { 3, 4 }, 5 }"
        );
    }

    [Test]
    public void InitializerList() {
        TestParserRule(
            CParser.InitializerList,
            "3",
            "3, 4",
            "3, 4, 5"
        );
    }

    [Test]
    public void TypeName() {
        TestParserRule(
            CParser.TypeName,
            "int",
            "const int *",
            "const int (*)(int arg)"
        );
    }

    [Test]
    public void TypeDefName() {
        TestParserRule(
            source: "MyAwesomeType",
            env: new Parsing.ParserEnvironment().AddSymbol("MyAwesomeType", StorageClsSpec.TYPEDEF),
            parser: CParser.TypeDefName
        );
    }

}