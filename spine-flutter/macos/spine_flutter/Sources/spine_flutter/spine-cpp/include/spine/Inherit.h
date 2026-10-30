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

#ifndef Spine_TransformMode_h
#define Spine_TransformMode_h

#include <string.h>

namespace spine {
	/// Determines how a bone inherits world transforms from parent bones.
	enum Inherit {
		Inherit_Normal = 0,
		Inherit_OnlyTranslation,
		Inherit_NoRotationOrReflection,
		Inherit_NoScale,
		Inherit_NoScaleOrReflection
	};

	inline Inherit Inherit_valueOf(const char *value) {
		if (strcmp(value, "normal") == 0)
			return Inherit_Normal;
		else if (strcmp(value, "onlyTranslation") == 0)
			return Inherit_OnlyTranslation;
		else if (strcmp(value, "noRotationOrReflection") == 0)
			return Inherit_NoRotationOrReflection;
		else if (strcmp(value, "noScale") == 0)
			return Inherit_NoScale;
		else if (strcmp(value, "noScaleOrReflection") == 0)
			return Inherit_NoScaleOrReflection;
		else
			return Inherit_Normal;// default
	}
}

#endif /* Spine_TransformMode_h */
