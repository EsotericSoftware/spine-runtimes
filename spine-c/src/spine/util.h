#ifndef SPINE_UTIL_H_
#define SPINE_UTIL_H_

#include <stdlib.h>
#include <string.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

/* Used to cast away const on an lvalue. */
#define CAST(TYPE,VALUE) *(TYPE*)&VALUE

#define CALLOC(TYPE,COUNT) (TYPE*)calloc(1, sizeof(TYPE) * COUNT);
#define MALLOC(TYPE,COUNT) (TYPE*)malloc(sizeof(TYPE) * COUNT);

#define MALLOC_STR(TO,FROM) strcpy(CAST(char*, TO) = (char*)malloc(strlen(FROM)), FROM);

#define FREE(E) free((void*)E);

const char* readFile (const char* path);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_UTIL_H_ */
