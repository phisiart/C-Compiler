struct S {
	int member;
};

struct S foo() {
	struct S s = { 1 };
	return s;
}

int printf(char *, ...);

int main() {
	struct S s = foo();
	printf("%d", s.member);
}