#ifndef SPINE_FORWARDDECL_H_
#define SPINE_FORWARDDECL_H_

/*
 Notes:
 
    - Forward declares reduce build times, especially for large game projects
    - Also prevents polluting by not requiring types that you don't need.
    
    - Cannot forward declare enumerated types, consider using 'int' and (c-style) casting
      or moving all the enums to this file and using it as universal header for spine
    
    - Other forward declare types can be added, just follow example of spAtlas
    
Usage:

    - #include "spine/ForwardDecl.h" in the header files for your game that uses Spine data type
      pointers* and references&
      
    - #include "spine/ " relevant headers in the cpp files that actually use the Spine objects
 */
 
#ifdef __cplusplus
extern "C" {
#endif

    typedef struct spEvent spEvent;
    
    typedef struct spAtlas spAtlas;
    
    typedef struct spAnimationState spAnimationState;
    typedef struct spAnimationStateData spAnimationStateData;
    
    typedef struct spSkeleton spSkeleton;
    typedef struct spSkeletonBounds spSkeletonBounds;
    typedef struct spSkeletonData spSkeletonData;
    typedef struct spSlot spSlot;
    typedef struct spAttachment spAttachment;

#ifdef __cplusplus
};
#endif

#endif /* SPINE_FORWARDDECL_H_ */