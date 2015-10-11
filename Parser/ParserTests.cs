using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class ParserTests {
    [Test]
    public void _const_char() {
        var scanner = new Scanner();
        scanner.src = @"'a'";
        scanner.Lex();

        var input = new Parser2.ParserInput(new Parser2.ParserEnvironment(), scanner.tokens);
        var result = Parser2.ConstChar(input);

    }
}
