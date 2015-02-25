using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace driver {
    class Program {
        static void Main(string[] args) {
            Scanner scanner = new Scanner();
            scanner.OpenFile("hello.c");
            scanner.Lex();
            List<Token> tokens = scanner.tokens;

            TranslationUnit unit;
            int r = _translation_unit.Parse(tokens, 0, out unit);

            Tuple<AST.Env, AST.TranslnUnit> ast = unit.GetTranslationUnit();

            CGenState state = new CGenState();
            ast.Item2.CodeGenerate(state);

            Console.WriteLine(state);
        }
    }
}
