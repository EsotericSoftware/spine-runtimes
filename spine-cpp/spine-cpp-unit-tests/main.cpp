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

#include "spine/Extension.h"

#include "KMemory.h" // last include

using namespace Spine;

class KanjiSpineExtension : public DefaultSpineExtension
{
public:
    static KanjiSpineExtension* getInstance();
    
    virtual ~KanjiSpineExtension();
    
    virtual void* spineAlloc(size_t size, const char* file, int line);
    
    virtual void* spineCalloc(size_t num, size_t size, const char* file, int line);
    
    virtual void* spineRealloc(void* ptr, size_t size, const char* file, int line);
    
    virtual void spineFree(void* mem);
    
protected:
    KanjiSpineExtension();
};

KanjiSpineExtension* KanjiSpineExtension::getInstance()
{
    static KanjiSpineExtension ret;
    return &ret;
}

KanjiSpineExtension::~KanjiSpineExtension()
{
    // Empty
}

void* KanjiSpineExtension::spineAlloc(size_t size, const char* file, int line)
{
    return _kanjimalloc(size);
}

void* KanjiSpineExtension::spineCalloc(size_t num, size_t size, const char* file, int line)
{
    void* ptr = _kanjimalloc(num * size, file, line);
    if (ptr)
    {
        memset(ptr, 0, num * size);
    }
    
    return ptr;
}

void* KanjiSpineExtension::spineRealloc(void* ptr, size_t size, const char* file, int line)
{
    return _kanjirealloc(ptr, size);
}

void KanjiSpineExtension::spineFree(void* mem)
{
    _kanjifree(mem);
}

KanjiSpineExtension::KanjiSpineExtension() : DefaultSpineExtension()
{
    // Empty
}

int main(int argc, char* argv[])
{
	SpineExtension::setInstance(KanjiSpineExtension::getInstance());

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
	if(JetBrains::underTeamcity()) gTeamCityListener.startSuite("Spine-CPP Test Suite");
	int ret_val = TestFixtureFactory::theInstance().runTests() ? 0 : -1;
	if(JetBrains::underTeamcity()) gTeamCityListener.endSuite("Spine-CPP Test Suite");

	// End Timing
	time(&end_time);
	double secs = difftime(end_time,start_time);
	printf("\n\n%i minutes and %i seconds of your life taken from you by these tests.\n", ((int)secs) / 60, ((int)secs) % 60);

	return ret_val;
}

