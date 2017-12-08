//
//  main.cpp
//  spine_unit_test
//
//  Created by Stephen Gowen on 12/7/17.
//  Copyright © 2017 Noctis Games. All rights reserved.
//

#ifdef WIN32
#include <direct.h>
#else
#include <unistd.h>
#endif // WIN32

#include <ctime>
#include "KString.h"
#include <stdio.h>

#include "spine/Extension.h"

#include "SimpleTest.h"
#include "MemoryTest.h"

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
    void* ptr = spineAlloc(num * size, file, line);
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
    
    SimpleTest::test();
    MemoryTest::test();
    
    // End Timing
    time(&end_time);
    double secs = difftime(end_time,start_time);
    printf("\n\n%i minutes and %i seconds of your life taken from you by these tests.\n", ((int)secs) / 60, ((int)secs) % 60);
    
    return 0;
}
