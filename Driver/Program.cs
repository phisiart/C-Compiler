using System;
using System.Linq;

namespace Driver {
    class Program {
        static void Main(String[] args) {
            if (!args.Any()) {
                String src = @"
int printf(char *, ...);
int main(int argc, char **argv) {
    printf(""%d"", argc);
    return 0;
}
";
                Compiler compiler = Compiler.FromSource(src);
                Console.WriteLine(compiler.Assembly);
            } else {
                Compiler compiler = Compiler.FromFile(args[0]);
                Console.WriteLine(compiler.Assembly);
            }
        }
    }
}
