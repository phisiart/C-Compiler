int printf(char *, ...);
void *malloc(unsigned int nbytes);

typedef struct Node {
	int value;
	struct Node *next;
} Node;

Node *cons(int value, Node *tl) {
	Node *hd = malloc(sizeof(struct Node));
	hd->value = value;
	hd->next = tl;
	return hd;
}

void print_list(Node *hd) {
	Node *cur = hd;
	for (; cur != (void *)0; cur = cur->next) {
		printf("%d\t", cur->value);
	}
}

void print_list_recursive(Node *hd) {
	if (hd != (void *)0) {
		printf("%d\t", hd->value);
		print_list_recursive(hd->next);
	}
}

void *flip_list(Node *hd) {
	Node *cur = hd;
	hd = 0;
	while (cur != 0) {
		Node *next = cur->next;
		cur->next = hd;
		hd = cur;
		cur = next;
	}
	return hd;
}

int main() {
	Node *hd = cons(2, cons(1, cons(0, (void *)0)));
	print_list(hd);
	hd = flip_list(hd);
	print_list(hd);
	print_list_recursive(hd);
	return 0;
}
