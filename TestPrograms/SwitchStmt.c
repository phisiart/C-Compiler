int printf(char *, ...);

int main() {
    int a = 3;
    switch (a) {
        case 1:
            printf("1\n");
            break;
        case 2:
            printf("2\n");
            break;
        case 3:
            printf("3\n");
            break;
        default:
            printf("default\n");
            break;
    }
    return 0;
}
