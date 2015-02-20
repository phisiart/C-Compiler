using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexical_analysis {
    class Program {
        static void Main(string[] args) {
            Scanner lex = new Scanner();
            lex.src = "int main() { return 0; }";
            lex.Lex();
            List<Token> tokens = lex.tokens;
            string output = lex.ToString();
        }
    }
}
