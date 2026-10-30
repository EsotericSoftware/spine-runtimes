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

#ifndef Spine_BoneData_h
#define Spine_BoneData_h

#include <spine/PosedData.h>
#include <spine/BonePose.h>
#include <spine/SpineString.h>
#include <spine/Color.h>
#include <spine/RTTI.h>

namespace spine {
	class SP_API BoneData : public PosedDataGeneric<BonePose> {
		friend class SkeletonBinary;

		friend class SkeletonJson;

		friend class AnimationState;

		friend class RotateTimeline;

		friend class ScaleTimeline;

		friend class ScaleXTimeline;

		friend class ScaleYTimeline;

		friend class ShearTimeline;

		friend class ShearXTimeline;

		friend class ShearYTimeline;

		friend class TranslateTimeline;

		friend class TranslateXTimeline;

		friend class TranslateYTimeline;

		friend class Slot;

	public:
		BoneData(int index, const String &name, BoneData *parent = NULL);

		/// The Skeleton::getBones() index for this bone.
		int getIndex();

		/// The parent bone, or NULL if this bone is the root.
		BoneData *getParent();

		float getLength();

		void setLength(float inValue);

		Color &getColor();

		/// The bone icon name as it was in Spine, or empty if nonessential data was not exported.
		const String &getIcon();

		void setIcon(const String &icon);

		/// The bone icon's display size scale, or 1 if nonessential data was not exported.
		float getIconSize();

		void setIconSize(float iconSize);

		/// The bone icon's display rotation in degrees, or 0 if nonessential data was not exported.
		float getIconRotation();

		void setIconRotation(float iconRotation);

		bool getVisible();

		void setVisible(bool inValue);

	private:
		const int _index;
		BoneData *_parent;
		float _length;
		Color _color;
		String _icon;
		float _iconSize;
		float _iconRotation;
		bool _visible;
	};
}

#endif /* Spine_BoneData_h */
