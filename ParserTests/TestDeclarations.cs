using NUnit.Framework;
using Parsing;
using AST;
using static CompilerTests.ParserTestUtils;

namespace CompilerTests {
    [TestFixture]
    public class TestDeclarations {

        [Test]
        public void Declaration() {
            TestParserRule(
                CParsers.Declaration,
                "const int i = 3;",
                "const volatile *i = 0, (* const j)[3] = { 0 };"
                );
        }

        [Test]
        public void DeclarationSpecifiers() {
            TestParserRule(
                CParsers.DeclarationSpecifiers,
                "auto int const",
                "typedef struct MyStruct volatile",
                "int auto int const short"
                );
        }

        [Test]
        public void InitDeclaratorList() {
            TestParserRule(
                CParsers.InitDeclaratorList,
                "var1, var2",
                "*var1 = 0, * const var2()[]"
                );
        }

        [Test]
        public void InitDeclarator() {
            TestParserRule(
                CParsers.InitDeclarator,
                "name",
                "name = 3",
                "*name = 3",
                "* const name = { 3 }"
                );
        }

        [Test]
        public void StorageClassSpecifier() {
            TestParserRule(
                CParsers.StorageClassSpecifier,
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
                CParsers.TypeSpecifier,
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

            TestParserRule("MyAwesomeType", new ParserEnvironment().AddSymbol("MyAwesomeType", StorageClsSpec.TYPEDEF), CParsers.TypeDefName
                );
        }

        [Test]
        public void TypeQualifier() {
            TestParserRule(
                CParsers.TypeQualifier,
                "const",
                "volatile"
                );
        }

        [Test]
        public void Declarator() {
            TestParserRule(
                CParsers.Declarator,
                "*name[3]",
                "* const *name",
                "name[3][]",
                "*name()"
                );
        }

        [Test]
        public void Pointer() {
            TestParserRule(
                CParsers.Pointer,
                "*",
                "* const volatile",
                "* const * const *"
                );
        }

        [Test]
        public void ParameterTypeList() {
            TestParserRule(
                CParsers.ParameterTypeList,
                "int a, int b, int",
                "int a, int b, int, ..."
                );
        }

        [Test]
        public void ParameterList() {
            TestParserRule(
                CParsers.ParameterList,
                "int a, int b, int"
                );
        }

        [Test]
        public void TypeQualifierList() {
            TestParserRule(
                CParsers.TypeQualifierList,
                "const const"
                );
        }

        [Test]
        public void DirectDeclarator() {
            TestParserRule(
                CParsers.DirectDeclarator,
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
                CParsers.EnumSpecifier,
                "enum MyEnum",
                "enum MyEnum { NAME = 1, NAME }",
                "enum { NAME = 1, NAME }"
                );
        }

        [Test]
        public void EnumeratorList() {
            TestParserRule(
                CParsers.EnumeratorList,
                "NAME, NAME = 3, NAME, NAME = 4"
                );
        }

        [Test]
        public void Enumerator() {
            TestParserRule(
                CParsers.Enumerator,
                "NAME = 3",
                "NAME"
                );
        }

        [Test]
        public void EnumerationConstant() {
            TestParserRule(
                CParsers.EnumerationConstant,
                "name"
                );
        }

        [Test]
        public void StructOrUnionSpecifier() {
            TestParserRule(
                CParsers.StructOrUnion,
                "struct",
                "union"
                );
        }

        [Test]
        public void StructDeclarationList() {
            TestParserRule(
                CParsers.StructDeclarationList,
                "int a; int b;"
                );
        }

        [Test]
        public void StructDeclaration() {
            TestParserRule(
                CParsers.StructDeclaration,
                "int const a[3], b : 3, * const c(int arg);"
                );
        }

        [Test]
        public void SpecifierQualifierList() {
            TestParserRule(
                CParsers.SpecifierQualifierList,
                "int const",
                "const int",
                "char char char",
                "short int const volatile"
                );
        }

        [Test]
        public void StructDeclaratorList() {
            TestParserRule(
                CParsers.StructDeclaratorList,
                "name, name, name[3]",
                "name : 3, name()"
                );
        }

        [Test]
        public void StructDeclarator() {
            TestParserRule(
                CParsers.StructDeclarator,
                "name",
                "name : 3"
                );
        }

        [Test]
        public void ParameterDeclaration() {
            TestParserRule(
                CParsers.ParameterDeclaration,
                "register int name[3]",
                "register int[3]"
                );
        }

        [Test]
        public void AbstractDeclarator() {
            TestParserRule(
                CParsers.AbstractDeclarator,
                "* const (*)()[]"
                );
        }

        [Test]
        public void DirectAbstractDeclarator() {
            TestParserRule(
                CParsers.DirectAbstractDeclarator,
                "[3][]",
                "(int arg1, char arg2)()",
                "[]()",
                "(*)()"
                );
        }

        [Test]
        public void Initializer() {
            TestParserRule(
                CParsers.Initializer,
                "3",
                "{ 3 }",
                "{ 3, 4, }",
                "{ 3, { 3, 4 }, 5 }"
                );
        }

        [Test]
        public void InitializerList() {
            TestParserRule(
                CParsers.InitializerList,
                "3",
                "3, 4",
                "3, 4, 5"
                );
        }

        [Test]
        public void TypeName() {
            TestParserRule(
                CParsers.TypeName,
                "int",
                "const int *",
                "const int (*)(int arg)"
                );
        }

        [Test]
        public void TypeDefName() {
            TestParserRule("MyAwesomeType", new ParserEnvironment().AddSymbol("MyAwesomeType", StorageClsSpec.TYPEDEF), CParsers.TypeDefName
                );
        }

    }
}