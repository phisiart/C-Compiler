int printf(char *, ...);
int main() {
	int a = 3;
	if (a == 3) {
		int b = 4;
		goto fail;
	}
fail:
	printf("%d\n", a);
	return 0;
}