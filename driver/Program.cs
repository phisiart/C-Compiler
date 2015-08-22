using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class TestingClass {
    public TestingClass() {
        Console.WriteLine("constructor");
    }
}

namespace driver {
    class Program {
        static void Main(String[] args) {
            Scanner scanner = new Scanner();
            scanner.src =
                @"
int foo() {
	return 0;
}
struct S {
    int a;
};
int printf(char *s, int);
int main(int argc, char **argv) {
    int b = 3 * 4;
    char c;
    float local_variable_2;
    double local_variable_3;
    const int * const * volatile a[3][4];
    struct S s;
    s;
    b;
    foo();
    3.25f;
    ""3.0"";
    c;
    printf(""%d"", 3);
    if (3) 4;
    b++;
}
                ";

            scanner.src =
                @"
struct B {
    int b1;
    int b2;
};
struct A {
    int a1;
    int a2;
    struct B b;
};

int main() {
    struct A a = { 1, 1, { {1} , 1 } };
}
";

            scanner.src = @"
int printf(char *, int);
int i;
int main() {
    int a;
    struct S {
        int member;
    } s;
    a = 1;
    printf(""%d\n"", (char)a - 1);
    printf(""%d\n"", s.member = 10);
}
";
            scanner.src = @"
int main() {
    int arr[] = { 1, 2, 3 };
}
";
            scanner.Lex();
            Console.WriteLine("Source code:");
            Console.WriteLine("======================");
            Console.WriteLine(scanner.src);

            Console.WriteLine("Tokens:");
            Console.WriteLine("======================");
            Console.WriteLine(scanner);

            List<Token> tokens = scanner.tokens;

            SyntaxTree.TranslationUnit unit;
			if (_translation_unit.Parse(tokens, 0, out unit) != tokens.Count - 1) {
				throw new InvalidOperationException("Error: not finished parsing");
			}

            Tuple<AST.Env, AST.TranslnUnit> ast = unit.GetTranslationUnit();

            //AST.TStructOrUnion type = (AST.TStructOrUnion)ast.Item1.Find("struct A").type;
            //AST.MemberIterator iter = new AST.MemberIterator(type);
            //iter.Read(new AST.TLong());
            //iter.Next();
            //iter.Read(new AST.TLong());
            //iter.Next();
            //iter.Read(new AST.TLong());
            //iter.Next();
            //iter.Read(new AST.TLong());

            CGenState state = new CGenState();
            ast.Item2.CodeGenerate(state);

            Console.WriteLine("x86 Assembly:");
            Console.WriteLine("======================");
            Console.WriteLine(state);


        }
    }
}
