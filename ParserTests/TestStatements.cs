using NUnit.Framework;
using Parsing;
using SyntaxTree;
using static ParserTestUtils;

[TestFixture]
public class TestStatements {

    [Test]
    public void Statement() {
        TestParserRule(
            CParser.Statement,
            "finish: a = 3;",
            "{ a = 3; b = 4; }",
            "a = 3;",
            "if (1) { 1; }",
            "for (;;) ;",
            "goto finish;"
        );
    }

    [Test]
    public void JumpStatement() {
        TestParserRule(
            CParser.Statement,
            "goto finish;",
            "continue;",
            "break;",
            "return;",
            "return 3;"
        );
    }

    [Test]
    public void CompoundStatement() {
        TestParserRule(
            CParser.CompoundStatement,
            "{ }",
            "{ int a; }",
            "{ a; }",
            "{ int a; a; }"
        );
    }

    [Test]
    public void DeclarationList() {
        TestParserRule(
            CParser.DeclarationList,
            "int a, b = 3; const char *str = 0;"
        );
    }

    [Test]
    public void StatementList() {
        TestParserRule(
            CParser.StatementList,
            "a = 3; 4; a = b = 5;"
        );
    }

    [Test]
    public void ExpressionStatement() {
        TestParserRule(
            CParser.ExpressionStatement,
            "3;",
            "a = b;"
        );
    }

    [Test]
    public void IterationStatement() {
        TestParserRule(
            CParser.IterationStatement,
            "while (1) { 1; }",
            "do { 1; } while (1);",
            "for (1; 1; 1) { 1; }"
        );
    }

    [Test]
    public void SelectionStatement() {
        TestParserRule(
            CParser.SelectionStatement,
            "if (1) 1; else 2;",
            "if (1) 1;",
            "switch (1) 1;"
        );
    }

    [Test]
    public void LabeledStatement() {
        TestParserRule(
            CParser.LabeledStatement,
            "finish: return;",
            "case 3: return;",
            "default: return;"
        );
    }
}