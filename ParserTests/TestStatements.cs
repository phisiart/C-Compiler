using NUnit.Framework;
using Parsing;
using static ParserTestUtils;

[TestFixture]
public class TestStatements {

    [Test]
    public void Statement() {
        TestParserRule(
            CParsers.Statement,
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
            CParsers.Statement,
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
            CParsers.CompoundStatement,
            "{ }",
            "{ int a; }",
            "{ a; }",
            "{ int a; a; }"
        );
    }

    [Test]
    public void DeclarationList() {
        TestParserRule(
            CParsers.DeclarationList,
            "int a, b = 3; const char *str = 0;"
        );
    }

    [Test]
    public void StatementList() {
        TestParserRule(
            CParsers.StatementList,
            "a = 3; 4; a = b = 5;"
        );
    }

    [Test]
    public void ExpressionStatement() {
        TestParserRule(
            CParsers.ExpressionStatement,
            "3;",
            "a = b;"
        );
    }

    [Test]
    public void IterationStatement() {
        TestParserRule(
            CParsers.IterationStatement,
            "while (1) { 1; }",
            "do { 1; } while (1);",
            "for (1; 1; 1) { 1; }"
        );
    }

    [Test]
    public void SelectionStatement() {
        TestParserRule(
            CParsers.SelectionStatement,
            "if (1) 1; else 2;",
            "if (1) 1;",
            "switch (1) 1;"
        );
    }

    [Test]
    public void LabeledStatement() {
        TestParserRule(
            CParsers.LabeledStatement,
            "finish: return;",
            "case 3: return;",
            "default: return;"
        );
    }
}