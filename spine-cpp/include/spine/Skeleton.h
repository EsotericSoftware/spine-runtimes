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

#ifndef Spine_Skeleton_h
#define Spine_Skeleton_h

#include <spine/Array.h>
#include <spine/MathUtil.h>
#include <spine/SpineObject.h>
#include <spine/SpineString.h>
#include <spine/Color.h>
#include <spine/Physics.h>
#include <spine/Update.h>
#include <spine/Posed.h>
#include <spine/Constraint.h>

namespace spine {
	class SkeletonData;

	class Bone;

	class BonePose;

	class Updatable;

	class Slot;

	class IkConstraint;

	class PathConstraint;

	class PhysicsConstraint;

	class TransformConstraint;

	class Skin;

	class Attachment;

	class SkeletonClipping;

	class SP_API Skeleton : public SpineObject {
		friend class AnimationState;

		friend class SkeletonBounds;

		friend class SkeletonClipping;

		friend class SlotCurveTimeline;

		friend class AttachmentTimeline;

		friend class RGBATimeline;

		friend class RGBTimeline;

		friend class AlphaTimeline;

		friend class RGBA2Timeline;

		friend class RGB2Timeline;

		friend class DeformTimeline;

		friend class DrawOrderFolderTimeline;

		friend class DrawOrderTimeline;

		friend class EventTimeline;

		friend class IkConstraintTimeline;

		friend class InheritTimeline;

		friend class PathConstraint;

		friend class PathConstraintMixTimeline;

		friend class PathConstraintPositionTimeline;

		friend class PathConstraintSpacingTimeline;

		friend class SliderTimeline;

		friend class SliderMixTimeline;

		friend class ScaleTimeline;

		friend class ScaleXTimeline;

		friend class ScaleYTimeline;

		friend class ShearTimeline;

		friend class ShearXTimeline;

		friend class ShearYTimeline;

		friend class TransformConstraintTimeline;

		friend class BoneTimeline1;

		friend class BoneTimeline2;

		friend class RotateTimeline;

		friend class TranslateTimeline;

		friend class TranslateXTimeline;

		friend class TranslateYTimeline;

		friend class TwoColorTimeline;

		friend class PhysicsConstraint;

		friend class BonePose;

		friend class IkConstraint;

		friend class PathConstraint;

		friend class PhysicsConstraint;

		friend class TransformConstraint;

		friend class Slider;

	public:
		explicit Skeleton(SkeletonData &skeletonData);

		~Skeleton();

		/// Caches information about bones and constraints. Must be called if bones, constraints or weighted path attachments are added
		/// or removed.
		void updateCache();

		void printUpdateCache();

		void constrained(Posed &object);

		void sortBone(Bone *bone);

		static void sortReset(Array<Bone *> &bones);

		/// Updates the world transform for each bone and applies all constraints.
		///
		/// See [World transforms](http://esotericsoftware.com/spine-runtime-skeletons#World-transforms) in the Spine
		/// Runtimes Guide.
		void updateWorldTransform(Physics physics);


		/// Sets the bones, constraints, and slots to their setup pose values.
		void setupPose();

		/// Sets the bones and constraints to their setup pose values.
		void setupPoseBones();

		void setupPoseSlots();

		SkeletonData &getData();

		Array<Bone *> &getBones();

		Array<Update *> &getUpdateCache();

		Bone *getRootBone();

		/// @return May be NULL.
		Bone *findBone(const String &boneName);

		Array<Slot *> &getSlots();

		/// @return May be NULL.
		Slot *findSlot(const String &slotName);

		Array<Slot *> &getDrawOrder();

		Skin *getSkin();

		/// Sets a skin by name (see setSkin).
		void setSkin(const String &skinName);

		/// Attachments from the new skin are attached if the corresponding attachment from the old skin was attached.
		/// If there was no old skin, each slot's setup mode attachment is attached from the new skin.
		/// After changing the skin, the visible attachments can be reset to those attached in the setup pose by calling
		/// See Skeleton::setSlotsToSetupPose()
		/// Also, often AnimationState::apply(Skeleton&) is called before the next time the
		/// skeleton is rendered to allow any attachment keys in the current animation(s) to hide or show attachments from the new skin.
		/// @param newSkin May be NULL.
		void setSkin(Skin *newSkin);

		/// @return May be NULL.
		Attachment *getAttachment(const String &slotName, const String &attachmentName);

		/// @return May be NULL.
		Attachment *getAttachment(int slotIndex, const String &attachmentName);

		/// @param attachmentName May be empty.
		void setAttachment(const String &slotName, const String &attachmentName);

		Array<Constraint *> &getConstraints();

		Array<PhysicsConstraint *> &getPhysicsConstraints();

		template<class T>
		T *findConstraint(const String &constraintName) {
			if (constraintName.isEmpty()) return NULL;
			for (size_t i = 0; i < _constraints.size(); i++) {
				Constraint *constraint = _constraints[i];
				if (constraint->getRTTI().isExactly(T::rtti)) {
					if (constraint->getData().getName() == constraintName) {
						return (T *) constraint;
					}
				}
			}
			return NULL;
		}

		/// Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose.
		/// @param outX The horizontal distance between the skeleton origin and the left side of the AABB.
		/// @param outY The vertical distance between the skeleton origin and the bottom side of the AABB.
		/// @param outWidth The width of the AABB
		/// @param outHeight The height of the AABB.
		void getBounds(float &outX, float &outY, float &outWidth, float &outHeight);

		/// Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose.
		/// @param outX The horizontal distance between the skeleton origin and the left side of the AABB.
		/// @param outY The vertical distance between the skeleton origin and the bottom side of the AABB.
		/// @param outWidth The width of the AABB
		/// @param outHeight The height of the AABB.
		/// @param outVertexBuffer Reference to hold an array of floats. This method will assign it with new floats as needed.
		/// @param clipping Pointer to a SkeletonClipping instance or NULL. If a clipper is given, clipping attachments will be taken into account.
		void getBounds(float &outX, float &outY, float &outWidth, float &outHeight, Array<float> &outVertexBuffer, SkeletonClipping *clipping);

		Color &getColor();

		void setColor(Color &color);

		void setColor(float r, float g, float b, float a);

		float getScaleX();

		void setScaleX(float inValue);

		float getScaleY();

		void setScaleY(float inValue);

		void setScale(float scaleX, float scaleY);

		float getX();

		void setX(float inValue);

		float getY();

		void setY(float inValue);

		void setPosition(float x, float y);

		void getPosition(float &x, float &y);

		float getWindX();

		void setWindX(float windX);

		float getWindY();

		void setWindY(float windY);

		float getGravityX();

		void setGravityX(float gravityX);

		float getGravityY();

		void setGravityY(float gravityY);

		/// Rotates the physics constraint so next {@link #update(Physics)} forces are applied as if the bone rotated around the
		/// specified point in world space.
		void physicsTranslate(float x, float y);

		/// Calls {@link PhysicsConstraint#rotate(float, float, float)} for each physics constraint. */
		void physicsRotate(float x, float y, float degrees);

		float getTime();

		void setTime(float time);

		void update(float delta);

	protected:
		SkeletonData &_data;
		Array<Bone *> _bones;
		Array<Slot *> _slots;
		Array<Slot *> _drawOrder;
		Array<Constraint *> _constraints;
		Array<PhysicsConstraint *> _physics;
		Array<Update *> _updateCache;
		Array<Posed *> _resetCache;
		Skin *_skin;
		Color _color;
		float _x, _y;
		float _scaleX, _scaleY;
		float _windX, _windY, _gravityX, _gravityY;
		float _time;
		int _update;
	};
}// namespace spine

#endif /* Spine_Skeleton_h */
