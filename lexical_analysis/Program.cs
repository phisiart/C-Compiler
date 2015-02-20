using System;
using System.Collections.Generic;

namespace lexical_analysis {
    class Program {
        static void Main(String[] args) {
            Scanner lex = new Scanner();
            lex.src = "int main() { return 0; }";
            lex.Lex();
            List<Token> tokens = lex.tokens;
            String output = lex.ToString();
        }
    }
}
