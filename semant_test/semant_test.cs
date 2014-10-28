using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


[TestClass]
public class SemantTest {
    [TestMethod]
    public void TestEnumSpec() {
        var src = Parser.GetTokensFromString("enum MyEnum { E1, E2, E3, E4 }");
        DeclnSpecs spec;
        int current = _declaration_specifiers.Parse(src, 0, out spec);
        Assert.IsTrue(current != -1);
        ScopeSandbox scope = new ScopeSandbox();
        scope = spec.Semant(scope);
    }

    [TestMethod]
    public void TestDecln() {
        var src = Parser.GetTokensFromString("int *a, **b, c;");
        Decln decln;
        int current = _declaration.Parse(src, 0, out decln);
        Assert.IsTrue(current != -1);
        ScopeSandbox scope = new ScopeSandbox();
        scope = decln.Semant(scope);
    }

    [TestMethod]
    public void TestExpression() {
        var src = Parser.GetTokensFromString("sizeof(int)");
        Expression expr;
        int current = _expression.Parse(src, 0, out expr);

    }
}
