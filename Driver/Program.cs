using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace driver {
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

                Compiler compiler = Compiler.FromSrc(src);
                Console.WriteLine(compiler.assembly);
            } else {
                // Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
                Compiler compiler = Compiler.FromFile(args[0]);
                Console.WriteLine(compiler.assembly);
            }
            
        }
    }
}
