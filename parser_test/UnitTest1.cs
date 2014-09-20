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
}

