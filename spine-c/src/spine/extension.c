/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/extension.h>
#include <stdio.h>

static void* (*mallocFunc) (size_t size) = malloc;
static void* (*debugMallocFunc) (size_t size, const char* file, int line) = NULL;
static void (*freeFunc) (void* ptr) = free;

void* _malloc (size_t size, const char* file, int line) {
	if(debugMallocFunc)
		return debugMallocFunc(size, file, line);

	return mallocFunc(size);
}
void* _calloc (size_t num, size_t size, const char* file, int line) {
	void* ptr = _malloc(num * size, file, line);
	if (ptr) memset(ptr, 0, num * size);
	return ptr;
}
void _free (void* ptr) {
	freeFunc(ptr);
}

void _setDebugMalloc(void* (*malloc) (size_t size, const char* file, int line)) {
	debugMallocFunc = malloc;
}

void _setMalloc (void* (*malloc) (size_t size)) {
	mallocFunc = malloc;
}
void _setFree (void (*free) (void* ptr)) {
	freeFunc = free;
}

char* _readFile (const char* path, int* length) {
	char *data;
	FILE *file = fopen(path, "rb");
	if (!file) return 0;

	fseek(file, 0, SEEK_END);
	*length = (int)ftell(file);
	fseek(file, 0, SEEK_SET);

	data = MALLOC(char, *length);
	fread(data, 1, *length, file);
	fclose(file);

	return data;
}

/*
 * Default TrackEntry create/dispose implementations
 */
spTrackEntry* _spDefaultAnimationState_createTrackEntry (spAnimationState* self){return _spTrackEntry_create();}
void _spDefaultAnimationState_disposeTrackEntry (spAnimationState* self, spTrackEntry* entry){_spTrackEntry_dispose(entry);}

/*
 * TrackEntry create/dispose function pointers
 */
spTrackEntry* (*_spAnimationState_createTrackEntryFunc) (spAnimationState* self) = _spDefaultAnimationState_createTrackEntry;
void (*_spAnimationState_disposeTrackEntryFunc) (spAnimationState* self, spTrackEntry* entry) = _spDefaultAnimationState_disposeTrackEntry;

/*
 * TrackEntry create/dispose function setters
 *
 * Use these methods to override the default implementation of TrackEntry creation
 * to include renderObject(s)* on the TrackEntry created within the context of the
 * renderObject* on the spAnimationState.
 */
void _spSetAnimationState_createTrackEntry( spTrackEntry* (*animationState_createTrackEntry) (spAnimationState* self) ){
	if(animationState_createTrackEntry)
		_spAnimationState_createTrackEntryFunc = animationState_createTrackEntry;
	else
		_spAnimationState_createTrackEntryFunc = _spDefaultAnimationState_createTrackEntry;
}

void _spSetAnimationState_disposeTrackEntry( void (*animationState_disposeTrackEntry) (spAnimationState* self, spTrackEntry* entry) ){
	if(animationState_disposeTrackEntry)
		_spAnimationState_disposeTrackEntryFunc = animationState_disposeTrackEntry;
	else
		_spAnimationState_disposeTrackEntryFunc = _spDefaultAnimationState_disposeTrackEntry;
}


/*
 * Concrete TrackEntry create/dispose Functions
 */
spTrackEntry* _spAnimationState_createTrackEntry( spAnimationState* self ){
	return _spAnimationState_createTrackEntryFunc(self);
}

void _spAnimationState_disposeTrackEntry( spAnimationState* self, spTrackEntry* entry ){
	_spAnimationState_disposeTrackEntryFunc(self, entry);
}