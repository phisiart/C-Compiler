int awesome_add(int a, int b) {
    return a + b;
}

int awesome_cmp(void *a, void *b) {
    return *(int *)a - *(int *)b;
}

void qsort(void *base, unsigned int nitems, unsigned int size, int (*compar)(const void *, const void*));
int printf(char *, ...);

int main() {
    int (*fp)(int, int);
    int arr[] = { 1, 6, 3, 8, 4, 5, 0, 2 };
    int i;

    fp = &awesome_add;
    fp = awesome_add;
    fp(1, 2);
    (*fp)(1, 2);
    
    qsort(arr, sizeof(arr) / sizeof(int), sizeof(int), &awesome_cmp);

    for (i = 0; i < sizeof(arr) / sizeof(int); ++i) {
        printf("%d\t", arr[i]);
    }

    return 0;
}
