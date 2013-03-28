#include <spine/util.h>
#include <stdio.h>

const char* readFile (const char* path) {
	FILE *file = fopen(path, "rb");
	if (!file) return 0;

	fseek(file, 0, SEEK_END);
	long length = ftell(file);
	fseek(file, 0, SEEK_SET);

	char* data = (char*)malloc(length + 1);
	fread(data, 1, length, file);
	fclose(file);
	data[length] = '\0';

	return data;
}
