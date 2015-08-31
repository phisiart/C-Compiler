struct S {
    int a;
    int b;
};

static struct S s_init = { 1, 2 };
static struct S s;
struct S e_init = { 1, 2 };
struct S e;
int printf(char *, ...);

int main() {
    printf("%d\n", s_init.a);
    printf("%d\n", s.a);
    printf("%d\n", e_init.a);
    printf("%d\n", e.a);
    return 0;
}
