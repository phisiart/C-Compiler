static int a = 3;

int add(int a, int b) {
    return a + b;
}

typedef int MyType;

struct MyStruct {
    int attrib1;
    char attrib2;
};

int main(int argc, char* argv[]) {
    MyType val = 0;
    int a = 3;
    int b = 4;
    int c = add(a, b);
    return 0;
}