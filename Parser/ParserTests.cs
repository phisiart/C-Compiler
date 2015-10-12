using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Parsing;

[TestFixture]
public class ParserTests {
    [Test]
    public void _const_char() {
        var scanner = new Scanner();
        scanner.src = @"'a'";
        scanner.Lex();

        var input = new ParserInput(new Parsing.ParserEnvironment(), scanner.tokens);
        var result = Parser2.ConstChar(input);

    }
}
