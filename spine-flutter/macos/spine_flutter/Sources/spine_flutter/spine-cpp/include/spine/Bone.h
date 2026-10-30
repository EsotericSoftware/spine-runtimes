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

#ifndef Spine_Bone_h
#define Spine_Bone_h

#include <spine/Posed.h>
#include <spine/PosedActive.h>
#include <spine/BoneData.h>
#include <spine/BonePose.h>
#include <spine/Array.h>

namespace spine {
	/// A node in a skeleton's hierarchy with a transform that affects its children and their attachments. A bone has a number of
	/// poses:
	/// - getData(): The setup pose data.
	/// - getPose(): The unconstrained local pose. Set by animations and application code.
	/// - getAppliedPose(): The local pose to use for rendering. Possibly modified by constraints.
	/// - World transform: the local pose combined with the parent world transform. Computed on a pose by
	///   BonePose::updateWorldTransform(Skeleton) and Skeleton::updateWorldTransform(Physics).
	class SP_API Bone : public PosedGeneric<BoneData, BonePose, BonePose>, public PosedActive, public Update {
		friend class AnimationState;
		friend class RotateTimeline;
		friend class IkConstraint;
		friend class TransformConstraint;
		friend class VertexAttachment;
		friend class PathConstraint;
		friend class PhysicsConstraint;
		friend class Skeleton;
		friend class Slider;
		friend class RegionAttachment;
		friend class PointAttachment;
		friend class AttachmentTimeline;
		friend class RGBATimeline;
		friend class RGBTimeline;
		friend class AlphaTimeline;
		friend class RGBA2Timeline;
		friend class RGB2Timeline;
		friend class ScaleTimeline;
		friend class ScaleXTimeline;
		friend class ScaleYTimeline;
		friend class ShearTimeline;
		friend class ShearXTimeline;
		friend class ShearYTimeline;
		friend class TranslateTimeline;
		friend class TranslateXTimeline;
		friend class TranslateYTimeline;
		friend class InheritTimeline;

		RTTI_DECL

	public:
		/// @param parent May be NULL.
		Bone(BoneData &data, Bone *parent);

		/// Copy constructor. Does not copy the child bones.
		Bone(Bone &bone, Bone *parent);

		/// The parent bone, or null if this is the root bone.
		Bone *getParent();

		/// The immediate children of this bone.
		Array<Bone *> &getChildren();

		static bool isYDown() {
			return yDown;
		}
		static void setYDown(bool value) {
			yDown = value;
		}

		virtual void update(Skeleton &skeleton, Physics physics) override {
			// No-op, need to extend Update so we can stuff Bone into Skeleton.updateCache temporarily.
			// See Skeleton::updateCache().
		}

	private:
		static bool yDown;
		Bone *const _parent;
		Array<Bone *> _children;
		bool _sorted;
	};
}

#endif /* Spine_Bone_h */
