using NUnit.Framework;
using Parsing;
using static ParserTestUtils;

[TestFixture]
public class TestExpressions {

    [Test]
    public void Expression() {
        TestParserRule(
            CParsers.Expression,
            "a = b, c = d, e = f"
        );
    }

    [Test]
    public void PrimaryExpression() {
        TestParserRule("a", CParsers.PrimaryExpression);
        TestParserRule("3", CParsers.PrimaryExpression);
        TestParserRule("3.0f", CParsers.PrimaryExpression);
        TestParserRule("'a'", CParsers.PrimaryExpression);
        TestParserRule("\"Hello, world!\"", CParsers.PrimaryExpression);
        TestParserRule("(a)", CParsers.PrimaryExpression);
    }

    [Test]
    public void Variable() {
        TestParserRule("a", CParsers.Variable);
    }

    [Test]
    public void Constant() {
        TestParserRule(
            CParsers.Constant,
            "'a'",
            "3",
            "3.0f"
        );
    }
    
    [Test]
    public void ConstChar() {
        TestParserRule("'a'", CParsers.ConstChar);
    }

    [Test]
    public void ConstInt() {
        TestParserRule("3", CParsers.ConstInt);
    }

    [Test]
    public void ConstFloat() {
        TestParserRule("3.0f", CParsers.ConstFloat);
    }

    [Test]
    public void StringLiteral() {
        TestParserRule("\"Haha\"", CParsers.StringLiteral);
    }

    [Test]
    public void ConstantExpression() {
        TestParserRule(
            CParsers.ConstantExpression,
            "a || b ? a = b : a ? b : c"
        );
    }

    [Test]
    public void ConditionalExpression() {
        TestParserRule(
            CParsers.ConditionalExpression,
            "a || b",
            "a || b ? a = b : a ? b : c"
        );
    }

    [Test]
    public void AssignmentExpression() {
        TestParserRule(
            CParsers.AssignmentExpression,
            "a ? b : c",
            "a = b",
            "a"
        );
    }

    [Test]
    public void PostfixExpression() {
        TestParserRule("a++", CParsers.PostfixExpression);
        TestParserRule("a--", CParsers.PostfixExpression);
        TestParserRule("a->b", CParsers.PostfixExpression);
        TestParserRule("a.b", CParsers.PostfixExpression);
        TestParserRule("a(1)", CParsers.PostfixExpression);
        TestParserRule("a()", CParsers.PostfixExpression);
        TestParserRule("a(1, 2)", CParsers.PostfixExpression);
        TestParserRule("a[b]", CParsers.PostfixExpression);
        TestParserRule("a[b]->b++", CParsers.PostfixExpression);
        TestParserRule("qsort(arr, sizeof(arr) / sizeof(int), sizeof(int), &awesome_cmp)", CParsers.PostfixExpression);
    }

    [Test]
    public void ArgumentExpressionList() {
        TestParserRule(
            CParsers.ArgumentExpressionList,
            "a = 3, 3",
            "3"
        );
    }

    [Test]
    public void UnaryExpression() {
        TestParserRule(
            CParsers.UnaryExpression,
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
            CParsers.CastExpression,
            "a",
            "(int)a",
            "(int)(char)a"
        );
    }

    [Test]
    public void MultiplicativeExpression() {
        TestParserRule(
            CParsers.MultiplicativeExpression,
            "a * b",
            "a / b",
            "a % c",
            "a * b / c % d"
        );
    }

    [Test]
    public void AdditiveExpression() {
        TestParserRule(
            CParsers.AdditiveExpression,
            "a * b / c % d + e",
            "a - b",
            "a + b - c"
        );
    }

    [Test]
    public void ShiftExpression() {
        TestParserRule(
            CParsers.ShiftExpression,
            "a << b",
            "a >> b"
        );
    }

    [Test]
    public void RelationalExpression() {
        TestParserRule(
            CParsers.RelationalExpression,
            "a < b",
            "a > b",
            "a <= b",
            "a >= b"
        );
    }

    [Test]
    public void EqualityExpression() {
        TestParserRule(
            CParsers.EqualityExpression,
            "a == b",
            "a != b"
        );
    }

    [Test]
    public void AndExpression() {
        TestParserRule("a & b", CParsers.AndExpression);
    }

    [Test]
    public void ExclusiveOrExpression() {
        TestParserRule("a ^ b", CParsers.ExclusiveOrExpression);
    }

    [Test]
    public void InclusiveOrExpression() {
        TestParserRule("a | b", CParsers.InclusiveOrExpression);
    }

    [Test]
    public void LogicalAndExpression() {
        TestParserRule("a && b", CParsers.LogicalAndExpression);
    }

    [Test]
    public void LogicalOrExpression() {
        TestParserRule("a || b", CParsers.LogicalOrExpression);
    }
}
