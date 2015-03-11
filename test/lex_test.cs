using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class LexTest {
    [TestMethod]
    public void test_lex() {
        Scanner lex = new Scanner();
        lex.src = "int main() { return 0; }";
        lex.Lex();
        String output = lex.ToString();
    }
}
