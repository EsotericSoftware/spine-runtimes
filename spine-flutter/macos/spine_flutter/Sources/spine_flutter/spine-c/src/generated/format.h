#ifndef SPINE_SPINE_FORMAT_H
#define SPINE_SPINE_FORMAT_H

#ifdef __cplusplus
extern "C" {
#endif

typedef enum spine_format {
	SPINE_FORMAT_ALPHA,
	SPINE_FORMAT_INTENSITY,
	SPINE_FORMAT_LUMINANCE_ALPHA,
	SPINE_FORMAT_RGB565,
	SPINE_FORMAT_RGBA4444,
	SPINE_FORMAT_RGB888,
	SPINE_FORMAT_RGBA8888
} spine_format;

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_FORMAT_H */
