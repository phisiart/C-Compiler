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

            if (args.Count() == 0) {
                String src = @"
int printf(char *, int);
void *malloc(unsigned int nbytes);

typedef struct Node {
    int value;
    struct Node *next;
} Node;

Node *cons(int value, Node *tl) {
    Node *hd = malloc(sizeof(struct Node));
    hd->value = value;
    hd->next = tl;
    return hd;
}

void print_list(Node *hd) {
    Node *cur = hd;
    for (; cur != (void *)0; cur = cur->next) {
        printf(""%d\t"", cur->value);
    }
}

void print_list_recursive(Node *hd) {
    if (hd != (void *)0) {
        printf(""%d\t"", hd->value);
        print_list_recursive(hd->next);
    }
}

int main() {
    Node *hd = cons(2, cons(1, cons(0, (void *)0)));
    print_list(hd);
    print_list_recursive(hd);
    return 0;
}
";

                Compiler compiler = Compiler.FromSrc(src);
                Console.WriteLine(compiler.assembly);
            } else {
                Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
                Compiler compiler = Compiler.FromFile(args[0]);
                Console.WriteLine(compiler.assembly);
            }
            

//            Scanner scanner = new Scanner();
//            scanner.src =
//                @"
//int foo() {
//	return 0;
//}
//struct S {
//    int a;
//};
//int printf(char *s, int);
//int main(int argc, char **argv) {
//    int b = 3 * 4;
//    char c;
//    float local_variable_2;
//    double local_variable_3;
//    const int * const * volatile a[3][4];
//    struct S s;
//    s;
//    b;
//    foo();
//    3.25f;
//    ""3.0"";
//    c;
//    printf(""%d"", 3);
//    if (3) 4;
//    b++;
//}
//                ";

//            scanner.src =
//                @"
//struct B {
//    int b1;
//    int b2;
//};
//struct A {
//    int a1;
//    int a2;
//    struct B b;
//};

//int main() {
//    struct A a = { 1, 1, { {1} , 1 } };
//}
//";

//            scanner.src = @"
//int printf(char *, int);
//int i;
//int main() {
//    int a;
//    struct S {
//        int member;
//    } s;
//    a = 1;
//    printf(""%d\n"", (char)a - 1);
//    printf(""%d\n"", s.member = 10);
//}
//";
//            scanner.src = @"
//int main() {
//    int arr[] = { 1, 2, 3 };
//}
//";

//            scanner.src = @"
//int printf(char *, int);
//void *malloc(unsigned int nbytes);

//typedef struct Node {
//    int value;
//    struct Node *next;
//} Node;

//Node *cons(int value, Node *tl) {
//    Node *hd = malloc(sizeof(struct Node));
//    hd->value = value;
//    hd->next = tl;
//    return hd;
//}

//void print_list(Node *hd) {
//    Node *cur = hd;
//    for (; cur != (void *)0; cur = cur->next) {
//        printf(""%d\t"", cur->value);
//    }
//}

//void print_list_recursive(Node *hd) {
//    if (hd != (void *)0) {
//        printf(""%d\t"", hd->value);
//        print_list_recursive(hd->next);
//    }
//}

//int main() {
//    Node *hd = cons(2, cons(1, cons(0, (void *)0)));
//    print_list(hd);
//    print_list_recursive(hd);
//    return 0;
//}
//";

//            scanner.Lex();
//            Console.WriteLine("Source code:");
//            Console.WriteLine("======================");
//            Console.WriteLine(scanner.src);

//            Console.WriteLine("Tokens:");
//            Console.WriteLine("======================");
//            Console.WriteLine(scanner);

//            List<Token> tokens = scanner.tokens;

//            SyntaxTree.TranslnUnit unit;
//			if (_translation_unit.Parse(tokens, 0, out unit) != tokens.Count - 1) {
//				throw new InvalidOperationException("Error: not finished parsing");
//			}

//            Tuple<AST.Env, AST.TranslnUnit> ast = unit.GetTranslationUnit();

//            //AST.TStructOrUnion type = (AST.TStructOrUnion)ast.Item1.Find("struct A").type;
//            //AST.MemberIterator iter = new AST.MemberIterator(type);
//            //iter.Read(new AST.TLong());
//            //iter.Next();
//            //iter.Read(new AST.TLong());
//            //iter.Next();
//            //iter.Read(new AST.TLong());
//            //iter.Next();
//            //iter.Read(new AST.TLong());

//            CGenState state = new CGenState();
//            ast.Item2.CodeGenerate(state);

//            Console.WriteLine("x86 Assembly:");
//            Console.WriteLine("======================");
//            Console.WriteLine(state);


        }
    }
}
