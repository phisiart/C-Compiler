int printf(char *, ...);
int main() {
    float f1;
    float f2;
    double d1;
    double d2;
    f1 = 2.333f;
    f2 = 33.3f;
    d1 = 1.23456789;
    d2 = 2.3456789;
    printf("lf\n", f1 + f2);
    printf("lf\n", d1 + d2);
    printf("lf\n", f1 + d1);
    return 0;
}
