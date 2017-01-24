# 3.6.x
* [c] Modified kvec.h used by SkeletonBinary.c to use Spine's MALLOC/FREE macros. That way there's only one place 
  to inject custom allocators ([extension.h](https://github.com/EsotericSoftware/spine-runtimes/blob/master/spine-c/spine-c/include/spine/extension.h)) [commit](https://github.com/EsotericSoftware/spine-runtimes/commit/c2cfbc6cb8709daa082726222d558188d75a004f)
