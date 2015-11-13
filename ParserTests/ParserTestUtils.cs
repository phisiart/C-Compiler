using System;
using System.Linq;
using LexicalAnalysis;
using NUnit.Framework;
using Parsing;

namespace CompilerTests {
    public static class ParserTestUtils {
        public static ParserInput CreateInput(String source) {
            var scanner = new Scanner(source);
            return new ParserInput(new ParserEnvironment(), scanner.Tokens);
        }

        public static void TestParserRule<R>(String source, ParserEnvironment env, IParser<R> parser) {
            var scanner = new Scanner(source);
            var input = new ParserInput(env, scanner.Tokens);
            var result = parser.Parse(input);
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsTrue(result.Source.Count() == 1);
        }

        public static void TestParserRule<R>(String source, IParser<R> parser) =>
            TestParserRule(source, new ParserEnvironment(), parser);

        public static void TestParserRule<R>(IParser<R> parser, params String[] sources) {
            foreach (var source in sources) {
                TestParserRule(source, parser);
            }
        }

    
    }
}