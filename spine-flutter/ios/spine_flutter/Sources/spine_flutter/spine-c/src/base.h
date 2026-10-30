/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#ifndef SPINE_C_BASE_H
#define SPINE_C_BASE_H

#include <stdint.h>
#include <stddef.h>
#include <stdbool.h>

#ifdef __cplusplus
#if _WIN32
#define SPINE_C_API extern "C" __declspec(dllexport)
#else
#ifdef __EMSCRIPTEN__
#define SPINE_C_API extern "C" __attribute__((used))
#else
#define SPINE_C_API extern "C"
#endif
#endif
#else
#if _WIN32
#define SPINE_C_API __declspec(dllexport)
#else
#ifdef __EMSCRIPTEN__
#define SPINE_C_API __attribute__((used))
#else
#define SPINE_C_API
#endif
#endif
#endif

#if defined(__clang__)
/* Clang needs to suppress both pedantic and C/C++ compatibility warnings */
#define SPINE_OPAQUE_TYPE(name)                                                                                                                      \
	_Pragma("clang diagnostic push") _Pragma("clang diagnostic ignored \"-Wpedantic\"")                                                              \
		_Pragma("clang diagnostic ignored \"-Wextern-c-compat\"") typedef struct name##_wrapper {                                                    \
	} name##_wrapper;                                                                                                                                \
	_Pragma("clang diagnostic pop") typedef name##_wrapper *name;
#elif defined(__GNUC__)
/* GCC only needs to suppress pedantic warning */
#define SPINE_OPAQUE_TYPE(name)                                                                                                                      \
	_Pragma("GCC diagnostic push") _Pragma("GCC diagnostic ignored \"-Wpedantic\"") typedef struct name##_wrapper {                                  \
	} name##_wrapper;                                                                                                                                \
	_Pragma("GCC diagnostic pop") typedef name##_wrapper *name;
#else
/* Other compilers - generic version */
#define SPINE_OPAQUE_TYPE(name)                                                                                                                      \
	typedef struct name##_wrapper {                                                                                                                  \
	} name##_wrapper;                                                                                                                                \
	typedef name##_wrapper *name;
#endif

typedef long long spine_property_id;

#endif// SPINE_C_BASE_H
