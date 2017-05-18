// SexyKanjiTestSuite.cpp : Defines the entry point for the console application.
//

#include "MiniCppUnit.hxx"

#ifdef WIN32
#include <direct.h>
#else
#include <unistd.h>
#endif // WIN32

#include <ctime>
#include "KString.h"
#include <stdio.h>

#include "spine/extension.h"
#include "spine/spine.h"

#include "KMemory.h" // last include

void RegisterMemoryLeakDetector()
{
	// Register our malloc and free functions to track memory leaks
	#ifdef KANJI_MEMTRACE
	_setDebugMalloc(_kanjimalloc);
	#endif
	_setMalloc(_kanjimalloc);
	_setRealloc(_kanjirealloc);
	_setFree(_kanjifree);
}

int main(int argc, char* argv[])
{
	RegisterMemoryLeakDetector();

	// Start Timing
	time_t start_time, end_time;
	time(&start_time);


	/* Set working directory to current location for opening test data */
#ifdef WIN32
	_chdir( GetFileDir(argv[0], false).c_str() );
#else
	chdir(GetFileDir(argv[0], false).c_str());
#endif

	// Run Test Suite
	if(JetBrains::underTeamcity()) gTeamCityListener.startSuite("Spine-C Test Suite");
	int ret_val = TestFixtureFactory::theInstance().runTests() ? 0 : -1;
	if(JetBrains::underTeamcity()) gTeamCityListener.endSuite("Spine-C Test Suite");

	// End Timing
	time(&end_time);
	double secs = difftime(end_time,start_time);
	printf("\n\n%i minutes and %i seconds of your life taken from you by these tests.\n", ((int)secs) / 60, ((int)secs) % 60);

	spAnimationState_disposeStatics(); // Fix for #775

	return ret_val;
}



extern "C" { // probably unnecessary 

	void _spAtlasPage_createTexture(spAtlasPage* self, const char* path) {
		self->rendererObject = 0;
		self->width = 2048;
		self->height = 2048;
	}

	void _spAtlasPage_disposeTexture(spAtlasPage* self) {
	}

	char* _spUtil_readFile(const char* path, int* length) {
		return _readFile(path, length);
	}
}
