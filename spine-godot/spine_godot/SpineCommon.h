/******************************************************************************
* Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

#ifndef SPINE_COMMON_H
#define SPINE_COMMON_H

#include "core/version.h"
#if VERSION_MAJOR > 3
#include "core/core_bind.h"
#include "core/error/error_macros.h"
#define REFCOUNTED RefCounted
#define EMPTY(x) ((x).is_empty())
#define EMPTY_PTR(x) ((x)->is_empty())
#define INSTANTIATE(x) (x).instantiate()
#define NOTIFY_PROPERTY_LIST_CHANGED() notify_property_list_changed()
#else
#include "core/object.h"
#include "core/reference.h"
#include "core/error_macros.h"
#define REFCOUNTED Reference
#define EMPTY(x) ((x).empty())
#define EMPTY_PTR(x) ((x)->empty())
#define INSTANTIATE(x) (x).instance()
#define NOTIFY_PROPERTY_LIST_CHANGED() property_list_changed_notify()
#endif

#define SPINE_CHECK(obj, ret) \
 if (!(obj)) { \
  ERR_PRINT("Native Spine object not set."); \
  return ret; \
 }

#define SPINE_STRING(x) spine::String((x).utf8())

#endif
