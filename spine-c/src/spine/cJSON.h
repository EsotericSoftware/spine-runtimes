/*
 Copyright (c) 2009 Dave Gamble
 
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
 */

/* Esoteric Software: Removed everything except parsing, added cJSON_GetObject*, formatted, double to float. */

#ifndef cJSON__h
#define cJSON__h

#ifdef __cplusplus
extern "C" {
#endif

#include <stdlib.h>

/* cJSON Types: */
#define cJSON_False 0
#define cJSON_True 1
#define cJSON_NULL 2
#define cJSON_Number 3
#define cJSON_String 4
#define cJSON_Array 5
#define cJSON_Object 6

#define cJSON_IsReference 256

/* The cJSON structure: */
typedef struct cJSON {
	struct cJSON *next, *prev; /* next/prev allow you to walk array/object chains. Alternatively, use GetArraySize/GetArrayItem/GetObjectItem */
	struct cJSON *child; /* An array or object item will have a child pointer pointing to a chain of the items in the array/object. */

	int type; /* The type of the item, as above. */

	const char* valuestring; /* The item's string, if type==cJSON_String */
	int valueint; /* The item's number, if type==cJSON_Number */
	float valuefloat; /* The item's number, if type==cJSON_Number */

	const char* name; /* The item's name string, if this item is the child of, or is in the list of subitems of an object. */
} cJSON;

/* Supply a block of JSON, and this returns a cJSON object you can interrogate. Call cJSON_dispose when finished. */
extern cJSON *cJSON_Parse (const char* value);

/* Delete a cJSON entity and all subentities. */
extern void cJSON_dispose (cJSON *c);

/* Returns the number of items in an array (or object). */
extern int cJSON_GetArraySize (cJSON *array);

/* Retrieve item number "item" from array "array". Returns NULL if unsuccessful. */
extern cJSON *cJSON_GetArrayItem (cJSON *array, int item);

/* Get item "string" from object. Case insensitive. */
extern cJSON *cJSON_GetObjectItem (cJSON *object, const char* string);
extern const char* cJSON_GetObjectString (cJSON* object, const char* name, const char* defaultValue);
extern float cJSON_GetObjectFloat (cJSON* value, const char* name, float defaultValue);
extern int cJSON_GetObjectInt (cJSON* value, const char* name, int defaultValue);

/* For analysing failed parses. This returns a pointer to the parse error. You'll probably need to look a few chars back to make sense of it. Defined when cJSON_Parse() returns 0. 0 when cJSON_Parse() succeeds. */
extern const char* cJSON_GetErrorPtr (void);

#ifdef __cplusplus
}
#endif

#endif
