using System;
using Driver;
using LexicalAnalysis;
using NUnit.Framework;

namespace CompilerTests {
    [TestFixture]
    public class DiagnosticsTests {
        private static CompilationError CompileError(String source) =>
            Assert.Throws<CompilationError>(() => Compiler.FromSource(source));

        [Test]
        public void TestProgramWithComments() {
            var compiler = Compiler.FromSource(@"
// Returns the answer.
int answer(void) {
    return /* forty-two */ 42;
}
");
            StringAssert.Contains("answer:", compiler.Assembly);
        }

        [Test]
        public void TestMissingSemicolonBeforeKeyword() {
            var error = CompileError("int main() {\n    int x = 1\n    return x;\n}\n");
            Assert.AreEqual("unexpected 'return'", error.Message);
            Assert.AreEqual(new SourceLocation(3, 5), error.Location);
        }

        [Test]
        public void TestMissingClosingBrace() {
            var error = CompileError("int main() {\n    return 0;\n");
            Assert.AreEqual("unexpected end of file", error.Message);
            Assert.AreEqual(new SourceLocation(3, 1), error.Location);
        }

        [Test]
        public void TestUnexpectedToken() {
            var error = CompileError("int main() {\n    while (1 {}\n}\n");
            Assert.AreEqual("unexpected '{'", error.Message);
            Assert.AreEqual(new SourceLocation(2, 14), error.Location);
        }

        [Test]
        public void TestExpectedIdentifier() {
            var error = CompileError("struct S { int a; } s;\nint f() { return s.; }\n");
            Assert.AreEqual("expected identifier before ';'", error.Message);
            Assert.AreEqual(new SourceLocation(2, 20), error.Location);
        }

        [Test]
        public void TestFormat() {
            var error = new CompilationError("unexpected 'return'", new SourceLocation(3, 5));
            Assert.AreEqual("a.c:3:5: error: unexpected 'return'", error.Format("a.c"));
            Assert.AreEqual("a.c: error: oops", new CompilationError("oops").Format("a.c"));
        }
    }
}
