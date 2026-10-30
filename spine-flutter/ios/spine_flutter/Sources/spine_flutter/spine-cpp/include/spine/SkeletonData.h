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

#ifndef Spine_SkeletonData_h
#define Spine_SkeletonData_h

#include <spine/Array.h>
#include <spine/SpineString.h>
#include <spine/ConstraintData.h>

namespace spine {
	class BoneData;

	class SlotData;

	class Skin;

	class EventData;

	class Animation;

	class IkConstraintData;

	class TransformConstraintData;

	class PathConstraintData;

	class PhysicsConstraintData;

	class ConstraintData;

	/// Stores the setup pose and all of the stateless data for a skeleton.
	///
	/// See <a href="https://esotericsoftware.com/spine-runtime-architecture#Data-objects">Data objects</a> in the Spine Runtimes
	/// Guide.
	class SP_API SkeletonData : public SpineObject {
		friend class SkeletonBinary;

		friend class SkeletonJson;

		friend class Skeleton;

	public:
		SkeletonData();

		~SkeletonData();

		/// Finds a bone by comparing each bone's name.
		/// It is more efficient to cache the results of this method than to call it multiple times.
		/// @return May be NULL.
		BoneData *findBone(const String &boneName);

		/// @return May be NULL.
		SlotData *findSlot(const String &slotName);

		/// @return May be NULL.
		Skin *findSkin(const String &skinName);

		/// @return May be NULL.
		EventData *findEvent(const String &eventDataName);

		/// @return May be NULL.
		Animation *findAnimation(const String &animationName);

		/// Collects animations used by slider constraints.
		Array<Animation *> &findSliderAnimations(Array<Animation *> &animations);

		/// The skeleton's name, which by default is the name of the skeleton data file when possible, or null when a name hasn't been
		/// set.
		const String &getName();

		void setName(const String &inValue);

		/// The skeleton's bones, sorted parent first. The root bone is always the first bone.
		Array<BoneData *> &getBones();

		/// The skeleton's slots in the setup pose draw order.
		Array<SlotData *> &getSlots();

		/// All skins, including the default skin.
		Array<Skin *> &getSkins();

		/// The skeleton's default skin.
		/// By default this skin contains all attachments that were not in a skin in Spine.
		/// @return May be NULL.
		Skin *getDefaultSkin();

		void setDefaultSkin(Skin *inValue);

		/// The skeleton's events.
		Array<EventData *> &getEvents();

		/// The skeleton's animations.
		Array<Animation *> &getAnimations();

		/// The skeleton's constraints.
		Array<ConstraintData *> &getConstraints();

		/// Finds a constraint of the specified type by comparing each constraint's name. It is more efficient to cache the results of
		/// this method than to call it multiple times.
		/// @return May be NULL.
		template<class T>
		T *findConstraint(const String &constraintName) {
			getConstraints();// Ensure constraints array is populated
			for (size_t i = 0, n = _constraints.size(); i < n; i++) {
				ConstraintData *constraint = _constraints[i];
				if (constraint->getName() == constraintName && constraint->getRTTI().instanceOf(T::rtti)) {
					return static_cast<T *>(constraint);
				}
			}
			return NULL;
		}

		/// The X coordinate of the skeleton's axis aligned bounding box in the setup pose.
		float getX();

		void setX(float inValue);

		/// The Y coordinate of the skeleton's axis aligned bounding box in the setup pose.
		float getY();

		void setY(float inValue);

		/// The width of the skeleton's axis aligned bounding box in the setup pose.
		float getWidth();

		void setWidth(float inValue);

		/// The height of the skeleton's axis aligned bounding box in the setup pose.
		float getHeight();

		void setHeight(float inValue);

		/// Baseline scale factor for applying physics and other effects based on distance to non-scalable properties, such as angle or
		/// scale. Default is 100.
		float getReferenceScale();

		void setReferenceScale(float inValue);

		/// The Spine version used to export this data, or NULL.
		const String &getVersion();

		void setVersion(const String &inValue);

		/// The skeleton data hash. This value will change if any of the skeleton data has changed.
		const String &getHash();

		void setHash(const String &inValue);

		/// The path to the images folder as defined in Spine, or null if nonessential data was not exported.
		const String &getImagesPath();

		void setImagesPath(const String &inValue);

		/// The path to the audio folder as defined in Spine, or null if nonessential data was not exported.
		const String &getAudioPath();

		void setAudioPath(const String &inValue);

		/// The dopesheet FPS in Spine. Available only when nonessential data was exported.
		float getFps();

		void setFps(float inValue);

	private:
		String _name;
		Array<BoneData *> _bones;// Ordered parents first
		Array<SlotData *> _slots;// Setup pose draw order.
		Array<Skin *> _skins;
		Skin *_defaultSkin;
		Array<EventData *> _events;
		Array<Animation *> _animations;
		Array<ConstraintData *> _constraints;
		float _x, _y, _width, _height;
		float _referenceScale;
		String _version;
		String _hash;
		Array<char *> _strings;

		// Nonessential.
		float _fps;
		String _imagesPath;
		String _audioPath;
	};
}

#endif /* Spine_SkeletonData_h */
