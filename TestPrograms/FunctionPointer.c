int awesome_add(int a, int b) {
    return a + b;
}

int main() {
    int (*fp)(int, int) = &awesome_add;
    fp = awesome_add;
    fp(1, 2);
    (*fp)(1, 2);
    return 0;
}
