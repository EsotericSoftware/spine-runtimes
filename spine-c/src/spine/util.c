#include <spine/util.h>

char* readFile (const char* path) {
	FILE *file = fopen(path, "rb");
	if (!file) return 0;

	fseek(file, 0, SEEK_END);
	long length = ftell(file);
	fseek(file, 0, SEEK_SET);

	char* data = (char*)malloc(sizeof(char) * length + 1);
	fread(data, sizeof(char), length, file);
	fclose(file);
	data[length] = '\0';

	return data;
}
