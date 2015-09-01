int printf(const char *, ...);
int main() {
    int a = 4;
    switch (a) {
        case 1:
        case 2:
        case 3:
            switch (a) {
                case 4:
                    printf("inner switch 4\n");
                default:
                    break;
            }
            break;
        default:
            break;
    }
    return 0;
}
