using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Parsing;
using static ParserTestUtils;

[TestFixture]
public class TestExpressions {

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
    public void Variable() {
        TestParserRule("a", CParser.Variable);
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
    }

    [Test]
    public void Unaryexpression() {
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
            "++ -- & * + - ~ ! a"
        );
    }

    [Test]
    public void CastExpression() {
        TestParserRule(
            CParser.CastExpression,
            "a"
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
        TestParserRule("a << b", CParser.ShiftExpression);
    }

    [Test]
    public void RelationalExpression() {
        TestParserRule("a < b", CParser.RelationalExpression);
    }

    [Test]
    public void EqualityExpression() {
        TestParserRule("a == b", CParser.EqualityExpression);
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
