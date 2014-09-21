using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


[TestClass]
public class ParserTest {
    [TestMethod]
    public void test_primary_expression() {
        Assert.IsTrue(_primary_expression.Test());
    }

    [TestMethod]
    public void test_postfix_expression() {
        Assert.IsTrue(_postfix_expression.Test());
    }

    [TestMethod]
    public void test_unary_expression() {
        Assert.IsTrue(_unary_expression.Test());
    }

    [TestMethod]
    public void test_cast_expression() {
        Assert.IsTrue(_cast_expression.Test());
    }

    [TestMethod]
    public void test_multiplicative_expression() {
        Assert.IsTrue(_multiplicative_expression.Test());
    }

    [TestMethod]
    public void test_additive_expression() {
        Assert.IsTrue(_additive_expression.Test());
    }

    [TestMethod]
    public void test_shift_expression() {
        Assert.IsTrue(_shift_expression.Test());
    }

    [TestMethod]
    public void test_relational_expression() {
        Assert.IsTrue(_relational_expression.Test());
    }

    [TestMethod]
    public void test_declaration_specifiers() {
        Assert.IsTrue(_declaration_specifiers.Test());
    }

    [TestMethod]
    public void test_storage_class_specifier() {
        Assert.IsTrue(_storage_class_specifier.Test());
    }

    [TestMethod]
    public void test_type_specifier() {
        Assert.IsTrue(_type_specifier.Test());
    }
}

