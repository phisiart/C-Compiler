int printf(char *, ...);
int main() {
    int a = 3;
    int b = 4;
    if (a > b) {
        a = 3;
        printf("a > b\n");
        break;
    } else {
        printf("a <= b\n");
    }

    if (a <= b) {
        printf("a <= b\n");
    } else {
        printf("a > b\n");
    }

    return 0;
}
