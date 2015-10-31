using System.IO;
using System.Linq;
using NUnit.Framework;
using Parsing;
using static ParserTestUtils;

[TestFixture]
public class TestExternalDefinitions {
    [Test]
    public void TestParser() {
        System.Console.WriteLine(Directory.GetCurrentDirectory());

        var files = Directory
            .GetFiles("../../TestPrograms")
            .Where(_ => _.EndsWith(".c"));

        var sources = files.Select(File.ReadAllText).ToArray();

        foreach (var source in sources) {
            var scanner = new Scanner(source);
            var tokens = scanner.Tokens;
            var env = new ParserEnvironment();
            var input = new ParserInput(env, scanner.Tokens);
            var result = CParser.TranslationUnit.Parse(input);
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsTrue(result.Source.Count() == 1);
        }
    }
}