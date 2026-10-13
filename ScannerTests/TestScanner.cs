using System;
using System.Linq;
using LexicalAnalysis;
using NUnit.Framework;

namespace CompilerTests {
    [TestFixture]
    public class TestScanner {
        private static Token[] Lex(String source) =>
            new Scanner(source).Tokens.ToArray();

        private static String[] Texts(String source) =>
            Lex(source).Where(_ => _.Kind != TokenKind.NONE).Select(_ => _.Text).ToArray();

        [Test]
        public void TestLineComment() {
            Assert.AreEqual(
                new[] { "int", "a", ";", "int", "b", ";" },
                Texts("int a; // a comment ; int c;\nint b;")
            );
        }

        [Test]
        public void TestLineCommentAtEndOfFile() {
            Assert.AreEqual(new[] { "x" }, Texts("x // no newline"));
        }

        [Test]
        public void TestBlockComment() {
            Assert.AreEqual(
                new[] { "a", "+", "b" },
                Texts("a /* one */ + /* multi\nline ** comment */b")
            );
        }

        [Test]
        public void TestBlockCommentWithoutSpaces() {
            Assert.AreEqual(new[] { "a", "/", "b" }, Texts("a/**//b"));
        }

        [Test]
        public void TestDivisionIsNotAComment() {
            Assert.AreEqual(new[] { "a", "/", "b", "/=", "c" }, Texts("a / b /= c"));
        }

        [Test]
        public void TestCommentMarkersInStringLiteral() {
            var tokens = Lex("\"// not /* a comment\"");
            Assert.AreEqual(TokenKind.STRING, tokens[0].Kind);
            Assert.AreEqual("// not /* a comment", ((TokenString)tokens[0]).Raw);
        }

        [Test]
        public void TestTokenLocations() {
            var tokens = Lex("int main() {\n\treturn 42; /* x */ }\n");
            Assert.AreEqual(
                new[] {
                    new SourceLocation(1, 1),  // int
                    new SourceLocation(1, 5),  // main
                    new SourceLocation(1, 9),  // (
                    new SourceLocation(1, 10), // )
                    new SourceLocation(1, 12), // {
                    new SourceLocation(2, 2),  // return
                    new SourceLocation(2, 9),  // 42
                    new SourceLocation(2, 11), // ;
                    new SourceLocation(2, 21), // }
                    new SourceLocation(3, 1),  // end of file
                },
                tokens.Select(_ => _.Location).ToArray()
            );
        }

        [Test]
        public void TestEndOfFileToken() {
            var tokens = Lex("");
            Assert.AreEqual(1, tokens.Length);
            Assert.AreEqual(TokenKind.NONE, tokens[0].Kind);
            Assert.AreEqual(new SourceLocation(1, 1), tokens[0].Location);
        }

        private static CompilationError LexError(String source) =>
            Assert.Throws<CompilationError>(() => new Scanner(source));

        [Test]
        public void TestUnexpectedCharacter() {
            var error = LexError("int a;\nint b = 1 $ 2;");
            Assert.AreEqual("unexpected character '$'", error.Message);
            Assert.AreEqual(new SourceLocation(2, 11), error.Location);
        }

        [Test]
        public void TestUnterminatedComment() {
            var error = LexError("int a; /* never closed\n int b;");
            Assert.AreEqual("unterminated comment", error.Message);
            Assert.AreEqual(new SourceLocation(1, 8), error.Location);
        }

        [Test]
        public void TestUnterminatedStringAtEndOfLine() {
            var error = LexError("char *s = \"abc;\nint b;");
            Assert.AreEqual("unterminated string literal", error.Message);
            Assert.AreEqual(new SourceLocation(1, 11), error.Location);
        }

        [Test]
        public void TestUnterminatedStringAtEndOfFile() {
            var error = LexError("char *s = \"abc");
            Assert.AreEqual("unterminated string literal", error.Message);
            Assert.AreEqual(new SourceLocation(1, 11), error.Location);
        }
    }
}
