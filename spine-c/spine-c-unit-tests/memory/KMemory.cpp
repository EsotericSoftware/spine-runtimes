#include <list>
#include <map>
#include <stdarg.h>
#include <string>
#include <time.h>

#include "KString.h"

#include "KMemory.h" // last include

///////////////////////////////////////////////////////////////////////////////
//
//	KANJI_DUMP_LEAKED_MEM will print out the memory block that was leaked.
//	This is helpful when leaked objects have string identifiers.
//
///////////////////////////////////////////////////////////////////////////////
//#define KANJI_DUMP_LEAKED_MEM


///////////////////////////////////////////////////////////////////////////////
//
//	KANJI_TRACK_MEM_USAGE will print out all memory allocations.
//
///////////////////////////////////////////////////////////////////////////////
//#define KANJI_TRACK_MEM_USAGE



///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
// Our memory system is thread-safe, but instead of linking massive libraries,
// we attempt to use C++11 std::mutex.
#ifdef USE_CPP11_MUTEX
#include <mutex>
typedef std::recursive_mutex KSysLock; // rentrant
struct KAutoLock {
	KAutoLock(KSysLock& lock) :mLock(lock) { mLock.lock(); }	// acquire 
	~KAutoLock() { mLock.unlock(); }							// release

	KSysLock& mLock;
};
#else // Fallback to unsafe.  don't spawn threads
typedef int KSysLock;
struct KAutoLock {
	KAutoLock(KSysLock) {}	// acquire 
	~KAutoLock() {}			// release
};
#endif



///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
struct KANJI_ALLOC_INFO
{
    size_t  size;
    std::string file;
    int     line;
};
static bool showLeaks = false;
class KAllocMap : public std::map<void*, KANJI_ALLOC_INFO>
{
public:
    KSysLock crit;
    static bool allocMapValid;

public:
    KAllocMap() { allocMapValid = true; }
    ~KAllocMap()
    {
        if (showLeaks)
            KMemoryDumpUnfreed();

        allocMapValid = false;
    }
};
bool KAllocMap::allocMapValid = false;
static KAllocMap allocMap; // once this static object destructs, it dumps unfreed memory


///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
#ifdef KANJI_TRACK_MEM_USAGE
void KMemoryDumpUsage(); // forward declaration
class KAllocStat
{
public:
    typedef std::map<int, int> allocCount; // [size] = count
    typedef std::map<std::pair<std::string, int>, allocCount> allocInfo; // [file, line] = allocCount

    allocInfo memInfo;
    static bool allocMapValid;

public:

    KAllocStat()
    {
        allocMapValid = true;
    }
    ~KAllocStat()
    {
        if (showLeaks)
            KMemoryDumpUsage();

        allocMapValid = false;
    }
    void addTrack(const char* fname, int lnum, int asize)
    {
        allocCount& info = memInfo[std::pair<std::string, int>(fname, lnum)];
        info[asize]++;
    }
};
bool KAllocStat::allocMapValid = false;
static KAllocStat allocStat;
#endif // KANJI_TRACK_MEM_USAGE



///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
extern void KMemoryAddTrack( void* addr, size_t asize, const char* fname, int lnum )
{
    if (!KAllocMap::allocMapValid || asize == 0)
        return;

    KAutoLock aCrit(allocMap.crit);
    showLeaks = true;

    KANJI_ALLOC_INFO& info = allocMap[addr];
    info.file = fname;
    info.line = lnum;
    info.size = asize;

#ifdef KANJI_TRACK_MEM_USAGE
    if (KAllocStat::allocMapValid)
        allocStat.addTrack(fname, lnum, asize);
#endif
};

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
void KMemoryRemoveTrack(void* addr)
{
    if (!KAllocMap::allocMapValid)
        return;

    KAutoLock aCrit(allocMap.crit);
    KAllocMap::iterator anItr = allocMap.find(addr);
    if (anItr != allocMap.end())
        allocMap.erase(anItr);
};

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
void KMemoryDumpUnfreed()
{
    if (!KAllocMap::allocMapValid)
        return;

    KAutoLock aCrit(allocMap.crit); // prevent modification of the map while iterating

    size_t totalSize = 0;
    char buf[8192];

    FILE* f = fopen("mem_leaks.txt", "wt");
    if (!f)
        return;

    time_t aTime = time(NULL);
    sprintf(buf, "Memory Leak Report for %s\n", asctime(localtime(&aTime)));
    fprintf(f, "%s", buf);
    KOutputDebug(DEBUGLVL, "\n");
    KOutputDebug(INFOLVL, buf);
    for(KAllocMap::iterator i = allocMap.begin(); i != allocMap.end(); ++i)
    {
        sprintf(buf, "%s(%d) : Leak %u byte%s @0x%08X\n", i->second.file.c_str(), i->second.line, i->second.size, i->second.size > 1 ? "s" : "", (size_t)i->first);
        KOutputDebug(ERRORLVL, buf);
        fprintf(f, "%s", buf);

#ifdef KANJI_DUMP_LEAKED_MEM
        unsigned char* data = (unsigned char*)i->first;
        int count = 0;
        char hex_dump[1024];
        char ascii_dump[1024];

        for (int index = 0; index < i->second.size; index++)
        {
            unsigned char _c = *data;

            if (count == 0)
                sprintf(hex_dump, "\t%02X ", _c);
            else
                sprintf(hex_dump, "%s%02X ", hex_dump, _c); // technically, this is undefined behavior

            if ((_c < 32) || (_c > 126))
                _c = '.';

            if (count == 7)
                sprintf(ascii_dump, "%s%c ", ascii_dump, _c);
            else
                sprintf(ascii_dump, "%s%c", count == 0 ? "\t" : ascii_dump, _c); // technically, this is undefined behavior


            if (++count == 16)
            {
                count = 0;
                sprintf(buf, "%s\t%s\n", hex_dump, ascii_dump);
                fprintf(f, buf);

                memset((void*)hex_dump, 0, 1024);
                memset((void*)ascii_dump, 0, 1024);
            }

            data++;
        }

        if (count != 0)
        {
            fprintf(f, hex_dump);
            for (int index = 0; index < 16 - count; index++)
                fprintf(f, "\t");

            fprintf(f, ascii_dump);

            for (int index = 0; index < 16 - count; index++)
                fprintf(f, ".");
        }

        count = 0;
        fprintf(f, "\n\n");
        memset((void*)hex_dump, 0, 1024);
        memset((void*)ascii_dump, 0, 1024);

#endif // KANJI_DUMP_LEAKED_MEM

        totalSize += i->second.size;
    }

	ErrorLevel lvl = (totalSize > 0) ? ERRORLVL : INFOLVL;

    sprintf(buf, "-----------------------------------------------------------\n");
    fprintf(f, "%s", buf);
    KOutputDebug(lvl, buf);
    sprintf(buf, "Total Unfreed: %u bytes (%luKB)\n\n", totalSize, totalSize / 1024);
    KOutputDebug(lvl, buf);
    fprintf(f, "%s", buf);
    fclose(f);
}

#ifdef KANJI_TRACK_MEM_USAGE
void KMemoryDumpUsage()
{
    if (!KAllocStat::allocMapValid)
        return;

    char buf[8192];
    FILE* f = fopen("mem_usage.txt", "wt");

    time_t aTime = time(NULL);
    sprintf(buf, "Memory Usage Report for %s\n", asctime(localtime(&aTime)));
    if (f) fprintf(f, "%s", buf);
    KOutputDebug("\n");
    KOutputDebug(buf);

    for(KAllocStat::allocInfo::iterator i = allocStat.memInfo.begin(); i != allocStat.memInfo.end(); ++i)
    {
        int aBytesTotal = 0;
        int aCallsTotal = 0;
        for (KAllocStat::allocCount::iterator index = i->second.begin(); index != i->second.end(); ++index)
        {
            aBytesTotal += index->first;
            aCallsTotal += index->second;
            sprintf(buf, "%s(%d) : %d bytes (%d %s)\n", i->first.first.c_str(), i->first.second, index->first, index->second, index->second == 1 ? "call" : "calls");
            if (f) fprintf(f, "%s", buf);
            KOutputDebug(buf);
        }

        if (i->second.size() > 1)
        {
            sprintf(buf, "    %s(%d) : %d KB total (%d calls)\n", i->first.first.c_str(), i->first.second, aBytesTotal / 1024, aCallsTotal);
            if (f) fprintf(f, "%s", buf);
            KOutputDebug(buf);
        }
    }
    if (f) fclose(f);
}
#endif // KANJI_TRACK_MEM_USAGE

size_t KMemoryAllocated()
{
    if (!KAllocMap::allocMapValid)
        return 0;

    KAutoLock aCrit(allocMap.crit);

    size_t size = 0;
    for(auto i = allocMap.begin(); i != allocMap.end(); ++i)
    {
        KANJI_ALLOC_INFO& info = i->second;
        size += info.size;
    }
    return size;
}