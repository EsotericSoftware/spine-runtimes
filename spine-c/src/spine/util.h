#ifndef SPINE_UTIL_H_
#define SPINE_UTIL_H_

#include <stdlib.h>
#include <string.h>

/** Used to cast away const on an lvalue. */
#define CAST(TYPE,VALUE) *(TYPE*)&VALUE

#define MALLOC_STR(TO,FROM) strcpy(CAST(char*, TO) = malloc(strlen(FROM)), FROM);

#define FREE(E) free((void*)E);

const char* readFile (const char* path);

#endif /* SPINE_UTIL_H_ */
