#include "spine_flutter.h"
#include <spine/spine.h>
#include <spine/Version.h>

int32_t spine_major_version() {
    return SPINE_MAJOR_VERSION;
}

int32_t spine_minor_version() {
    return SPINE_MINOR_VERSION;
}

spine::SpineExtension *spine::getDefaultExtension() {
   return new spine::DefaultSpineExtension();
}
