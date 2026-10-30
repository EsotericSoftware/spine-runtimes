#ifndef SPINE_SPINE_INHERIT_H
#define SPINE_SPINE_INHERIT_H

#ifdef __cplusplus
extern "C" {
#endif

typedef enum spine_inherit {
	SPINE_INHERIT_NORMAL = 0,
	SPINE_INHERIT_ONLY_TRANSLATION,
	SPINE_INHERIT_NO_ROTATION_OR_REFLECTION,
	SPINE_INHERIT_NO_SCALE,
	SPINE_INHERIT_NO_SCALE_OR_REFLECTION
} spine_inherit;

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_INHERIT_H */
