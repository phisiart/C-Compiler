using NUnit.Framework;
using Parsing;
using static ParserTestUtils;

[TestFixture]
public class TestExpressions {

    [Test]
    public void Expression() {
        TestParserRule(
            CParser.Expression,
            "a = b, c = d, e = f"
        );
    }

    [Test]
    public void PrimaryExpression() {
        TestParserRule("a", CParser.PrimaryExpression);
        TestParserRule("3", CParser.PrimaryExpression);
        TestParserRule("3.0f", CParser.PrimaryExpression);
        TestParserRule("'a'", CParser.PrimaryExpression);
        TestParserRule("\"Hello, world!\"", CParser.PrimaryExpression);
        TestParserRule("(a)", CParser.PrimaryExpression);
    }

    [Test]
    public void Variable() {
        TestParserRule("a", CParser.Variable);
    }

    [Test]
    public void Constant() {
        TestParserRule(
            CParser.Constant,
            "'a'",
            "3",
            "3.0f"
        );
    }
    
    [Test]
    public void ConstChar() {
        TestParserRule("'a'", CParser.CONST_CHAR);
    }

    [Test]
    public void ConstInt() {
        TestParserRule("3", CParser.CONST_INT);
    }

    [Test]
    public void ConstFloat() {
        TestParserRule("3.0f", CParser.CONST_FLOAT);
    }

    [Test]
    public void StringLiteral() {
        TestParserRule("\"Haha\"", CParser.STRING_LITERAL);
    }

    [Test]
    public void ConstantExpression() {
        TestParserRule(
            CParser.ConstantExpression,
            "a || b ? a = b : a ? b : c"
        );
    }

    [Test]
    public void ConditionalExpression() {
        TestParserRule(
            CParser.ConditionalExpression,
            "a || b",
            "a || b ? a = b : a ? b : c"
        );
    }

    [Test]
    public void AssignmentExpression() {
        TestParserRule(
            CParser.AssignmentExpression,
            "a ? b : c",
            "a = b",
            "a"
        );
    }

    [Test]
    public void PostfixExpression() {
        TestParserRule("a++", CParser.PostfixExpression);
        TestParserRule("a--", CParser.PostfixExpression);
        TestParserRule("a->b", CParser.PostfixExpression);
        TestParserRule("a.b", CParser.PostfixExpression);
        TestParserRule("a(1)", CParser.PostfixExpression);
        TestParserRule("a()", CParser.PostfixExpression);
        TestParserRule("a(1, 2)", CParser.PostfixExpression);
        TestParserRule("a[b]", CParser.PostfixExpression);
        TestParserRule("a[b]->b++", CParser.PostfixExpression);
        TestParserRule("qsort(arr, sizeof(arr) / sizeof(int), sizeof(int), &awesome_cmp)", CParser.PostfixExpression);
    }

    [Test]
    public void ArgumentExpressionList() {
        TestParserRule(
            CParser.ArgumentExpressionList,
            "a = 3, 3",
            "3"
        );
    }

    [Test]
    public void UnaryExpression() {
        TestParserRule(
            CParser.UnaryExpression,
            "++a",
            "--a",
            "&a",
            "*a",
            "+a",
            "-a",
            "~a",
            "!a",
            "++ -- & * + - ~ ! a",
            "sizeof 3",
            "sizeof(3)",
            "sizeof(int)"
        );
    }

    [Test]
    public void CastExpression() {
        TestParserRule(
            CParser.CastExpression,
            "a",
            "(int)a",
            "(int)(char)a"
        );
    }

    [Test]
    public void MultiplicativeExpression() {
        TestParserRule(
            CParser.MultiplicativeExpression,
            "a * b",
            "a / b",
            "a % c",
            "a * b / c % d"
        );
    }

    [Test]
    public void AdditiveExpression() {
        TestParserRule(
            CParser.AdditiveExpression,
            "a * b / c % d + e",
            "a - b",
            "a + b - c"
        );
    }

    [Test]
    public void ShiftExpression() {
        TestParserRule(
            CParser.ShiftExpression,
            "a << b",
            "a >> b"
        );
    }

    [Test]
    public void RelationalExpression() {
        TestParserRule(
            CParser.RelationalExpression,
            "a < b",
            "a > b",
            "a <= b",
            "a >= b"
        );
    }

    [Test]
    public void EqualityExpression() {
        TestParserRule(
            CParser.EqualityExpression,
            "a == b",
            "a != b"
        );
    }

    [Test]
    public void AndExpression() {
        TestParserRule("a & b", CParser.AndExpression);
    }

    [Test]
    public void ExclusiveOrExpression() {
        TestParserRule("a ^ b", CParser.ExclusiveOrExpression);
    }

    [Test]
    public void InclusiveOrExpression() {
        TestParserRule("a | b", CParser.InclusiveOrExpression);
    }

    [Test]
    public void LogicalAndExpression() {
        TestParserRule("a && b", CParser.LogicalAndExpression);
    }

    [Test]
    public void LogicalOrExpression() {
        TestParserRule("a || b", CParser.LogicalOrExpression);
    }
}
