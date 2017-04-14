#ifndef __KANJIMEMORY_H__
#define __KANJIMEMORY_H__

#include <stdlib.h>
#include <stdint.h>

#if defined(_DEBUG) && !defined(KANJI_MEMTRACE)
#define KANJI_MEMTRACE
#endif

#ifdef WIN32
#pragma warning(disable : 4595)
#endif

//////////////////////////////////////////////////////////////////////////
//                      HOW TO USE THIS FILE
//
//          In the desired .CPP file (NOT header file), AFTER ALL of your
//  #include declarations, do a #include "KMemory.h" or whatever you renamed
//  this file to. It's very important that you do it only in the .cpp and
//  after every other include file, otherwise it won't compile.  The memory leaks
//  will appear in a file called mem_leaks.txt and they will also be printed out
//  in the output window when the program exits.
//
//////////////////////////////////////////////////////////////////////////

#ifndef SAFE_DELETE
#define SAFE_DELETE(pPtr) { if(pPtr) delete pPtr; pPtr = 0; }
#endif

#ifndef SCOPED_AUTO_SAFE_DELETE
template <typename T>
class ScopedAutoDeletePointerHelper
{
public:
    ScopedAutoDeletePointerHelper(T pPtr) : _pPtr(pPtr) {}
    ~ScopedAutoDeletePointerHelper() { SAFE_DELETE(_pPtr); }

    T _pPtr;
};
#define SCOPED_AUTO_SAFE_DELETE(p) ScopedAutoDeletePointerHelper<decltype(p)> anAutoDelete##p(p);
#endif

#ifndef SAFE_DELETE_ARRAY
#define SAFE_DELETE_ARRAY(pPtr) { if(pPtr) delete [] pPtr; pPtr = 0; }
#endif

extern void KMemoryDumpUnfreed();
extern size_t KMemoryAllocated();

#ifdef WIN32
#define KMEM_CALLTYPE __cdecl
#else
#define KMEM_CALLTYPE
#endif

#ifdef __APPLE__
#define KMEM_THROWSPEC throw(std::bad_alloc)
#define KMEM_THROWS_BADALLOC
#include <new>
#else
#define KMEM_THROWSPEC
#endif

#if defined(KANJI_MEMTRACE)

/////////////////////////////////////////////
// DO NOT CALL THESE TWO METHODS DIRECTLY  //
/////////////////////////////////////////////

extern void KMemoryAddTrack(void* addr, size_t asize, const char* fname, int lnum);
extern void KMemoryRemoveTrack(void* addr);

//Replacement for the standard malloc/free, records size of allocation and the file/line number it was on
inline void* _kanjimalloc (size_t size, const char* file, int line)
{
    void* ptr = (void*)malloc(size);
    KMemoryAddTrack(ptr, size, file, line);
    return(ptr);
}

inline void* _kanjimalloc (size_t size)
{
    return _kanjimalloc(size, "", 0);
}

inline void _kanjifree (void* ptr)
{
    KMemoryRemoveTrack(ptr);
    free(ptr);
}

inline void* _kanjirealloc (void* ptr, size_t size, const char* file, int line)
{
    void* ptr2 = (void*)realloc(ptr, size);
    if (ptr2)
    {
        KMemoryRemoveTrack(ptr);
        KMemoryAddTrack(ptr2, size, file, line);
    }
    return ptr2;
}

inline void* _kanjirealloc (void* ptr, size_t size)
{
    return _kanjirealloc(ptr, size, "", 0);
}

#define kanjimalloc(size) _kanjimalloc((size), __FILE__, __LINE__)
#define kanjifree _kanjifree
#define kanjirealloc(ptr, size) _kanjirealloc(ptr, size, __FILE__, __LINE__)

//Replacement for the standard "new" operator, records size of allocation and the file/line number it was on
inline void* KMEM_CALLTYPE operator new(size_t size, const char* file, int line)
{
    void* ptr = (void*)malloc(size);
    KMemoryAddTrack(ptr, size, file, line);
    return(ptr);
}

//Same as above, but for arrays
inline void* KMEM_CALLTYPE operator new[](size_t size, const char* file, int line)
{
    void* ptr = (void*)malloc(size);
    KMemoryAddTrack(ptr, size, file, line);
    return(ptr);
}


// These single argument new operators allow vc6 apps to compile without errors
inline void* KMEM_CALLTYPE operator new(size_t size) KMEM_THROWSPEC
{
    void* ptr = (void*)malloc(size);
#ifdef KMEM_THROWS_BADALLOC
    if(!ptr) throw std::bad_alloc();
#endif
    return(ptr);
}

inline void* KMEM_CALLTYPE operator new[](size_t size) KMEM_THROWSPEC
{
    void* ptr = (void*)malloc(size);
#ifdef KMEM_THROWS_BADALLOC
    if(!ptr) throw std::bad_alloc();
#endif // KMEM_THROWS_BADALLOC
    return(ptr);
}


//custom delete operators
inline void KMEM_CALLTYPE operator delete(void* p) throw()
{
    KMemoryRemoveTrack(p);
    free(p);
}

inline void KMEM_CALLTYPE operator delete[](void* p) throw()
{
    KMemoryRemoveTrack(p);
    free(p);
}

//needed in case in the constructor of the class we're newing, it throws an exception
inline void KMEM_CALLTYPE operator delete(void* pMem, const char* file, int line)
{
    free(pMem);
}

inline void KMEM_CALLTYPE operator delete[](void* pMem, const char* file, int line)
{
    free(pMem);
}

#define KDEBUG_NEW new(__FILE__, __LINE__)
#define new KDEBUG_NEW

#else // KANJI_MEMTRACE NOT DEFINED

#define kanjimalloc malloc
#define kanjifree free
#define kanjirealloc realloc

inline void* _kanjimalloc(size_t size) { return malloc(size); }
inline void  _kanjifree(void* ptr) { free(ptr); }
inline void* _kanjirealloc(void* ptr, size_t size) { return realloc(ptr, size); }

#endif // KANJI_MEMTRACE

#endif // __KANJIMEMORY_H__