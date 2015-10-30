using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Parsing;

[TestFixture]
public static class ParserTestUtils {
    public static ParserInput CreateInput(String source) {
        var scanner = new Scanner(source);
        return new ParserInput(new Parsing.ParserEnvironment(), scanner.Tokens);
    }

    public static void TestParserRule<R>(String source, IParser<R> parser) {
        var scanner = new Scanner(source);
        var input = new ParserInput(new Parsing.ParserEnvironment(), scanner.Tokens);
        var result = parser.Parse(input);
        Assert.IsTrue(result.IsSuccessful);
        Assert.IsTrue(result.Source.Count() == 1);
    }

    public static void TestParserRule<R>(IParser<R> parser, params String[] sources) {
        foreach (var source in sources) {
            TestParserRule(source, parser);
        }
    }
}