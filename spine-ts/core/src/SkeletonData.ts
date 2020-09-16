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

module spine {
	/** Stores the setup pose and all of the stateless data for a skeleton.
	 *
	 * See [Data objects](http://esotericsoftware.com/spine-runtime-architecture#Data-objects) in the Spine Runtimes
	 * Guide. */
	export class SkeletonData {

		/** The skeleton's name, which by default is the name of the skeleton data file, if possible. May be null. */
		name: string;

		/** The skeleton's bones, sorted parent first. The root bone is always the first bone. */
		bones = new Array<BoneData>(); // Ordered parents first.

		/** The skeleton's slots. */
		slots = new Array<SlotData>(); // Setup pose draw order.
		skins = new Array<Skin>();

		/** The skeleton's default skin. By default this skin contains all attachments that were not in a skin in Spine.
		 *
		 * See {@link Skeleton#getAttachmentByName()}.
		 * May be null. */
		defaultSkin: Skin;

		/** The skeleton's events. */
		events = new Array<EventData>();

		/** The skeleton's animations. */
		animations = new Array<Animation>();

		/** The skeleton's IK constraints. */
		ikConstraints = new Array<IkConstraintData>();

		/** The skeleton's transform constraints. */
		transformConstraints = new Array<TransformConstraintData>();

		/** The skeleton's path constraints. */
		pathConstraints = new Array<PathConstraintData>();

		/** The X coordinate of the skeleton's axis aligned bounding box in the setup pose. */
		x: number;

		/** The Y coordinate of the skeleton's axis aligned bounding box in the setup pose. */
		y: number;

		/** The width of the skeleton's axis aligned bounding box in the setup pose. */
		width: number;

		/** The height of the skeleton's axis aligned bounding box in the setup pose. */
		height: number;

		/** The Spine version used to export the skeleton data, or null. */
		version: string;

		/** The skeleton data hash. This value will change if any of the skeleton data has changed. May be null. */
		hash: string;

		// Nonessential
		/** The dopesheet FPS in Spine. Available only when nonessential data was exported. */
		fps = 0;

		/** The path to the images directory as defined in Spine. Available only when nonessential data was exported. May be null. */
		imagesPath: string;

		/** The path to the audio directory as defined in Spine. Available only when nonessential data was exported. May be null. */
		audioPath: string;

		/** Finds a bone by comparing each bone's name. It is more efficient to cache the results of this method than to call it
		 * multiple times.
		 * @returns May be null. */
		findBone (boneName: string) {
			if (boneName == null) throw new Error("boneName cannot be null.");
			let bones = this.bones;
			for (let i = 0, n = bones.length; i < n; i++) {
				let bone = bones[i];
				if (bone.name == boneName) return bone;
			}
			return null;
		}

		findBoneIndex (boneName: string) {
			if (boneName == null) throw new Error("boneName cannot be null.");
			let bones = this.bones;
			for (let i = 0, n = bones.length; i < n; i++)
				if (bones[i].name == boneName) return i;
			return -1;
		}

		/** Finds a slot by comparing each slot's name. It is more efficient to cache the results of this method than to call it
		 * multiple times.
		 * @returns May be null. */
		findSlot (slotName: string) {
			if (slotName == null) throw new Error("slotName cannot be null.");
			let slots = this.slots;
			for (let i = 0, n = slots.length; i < n; i++) {
				let slot = slots[i];
				if (slot.name == slotName) return slot;
			}
			return null;
		}

		findSlotIndex (slotName: string) {
			if (slotName == null) throw new Error("slotName cannot be null.");
			let slots = this.slots;
			for (let i = 0, n = slots.length; i < n; i++)
				if (slots[i].name == slotName) return i;
			return -1;
		}

		/** Finds a skin by comparing each skin's name. It is more efficient to cache the results of this method than to call it
		 * multiple times.
		 * @returns May be null. */
		findSkin (skinName: string) {
			if (skinName == null) throw new Error("skinName cannot be null.");
			let skins = this.skins;
			for (let i = 0, n = skins.length; i < n; i++) {
				let skin = skins[i];
				if (skin.name == skinName) return skin;
			}
			return null;
		}

		/** Finds an event by comparing each events's name. It is more efficient to cache the results of this method than to call it
		 * multiple times.
		 * @returns May be null. */
		findEvent (eventDataName: string) {
			if (eventDataName == null) throw new Error("eventDataName cannot be null.");
			let events = this.events;
			for (let i = 0, n = events.length; i < n; i++) {
				let event = events[i];
				if (event.name == eventDataName) return event;
			}
			return null;
		}

		/** Finds an animation by comparing each animation's name. It is more efficient to cache the results of this method than to
		 * call it multiple times.
		 * @returns May be null. */
		findAnimation (animationName: string) {
			if (animationName == null) throw new Error("animationName cannot be null.");
			let animations = this.animations;
			for (let i = 0, n = animations.length; i < n; i++) {
				let animation = animations[i];
				if (animation.name == animationName) return animation;
			}
			return null;
		}

		/** Finds an IK constraint by comparing each IK constraint's name. It is more efficient to cache the results of this method
		 * than to call it multiple times.
		 * @return May be null. */
		findIkConstraint (constraintName: string) {
			if (constraintName == null) throw new Error("constraintName cannot be null.");
			let ikConstraints = this.ikConstraints;
			for (let i = 0, n = ikConstraints.length; i < n; i++) {
				let constraint = ikConstraints[i];
				if (constraint.name == constraintName) return constraint;
			}
			return null;
		}

		/** Finds a transform constraint by comparing each transform constraint's name. It is more efficient to cache the results of
		 * this method than to call it multiple times.
		 * @return May be null. */
		findTransformConstraint (constraintName: string) {
			if (constraintName == null) throw new Error("constraintName cannot be null.");
			let transformConstraints = this.transformConstraints;
			for (let i = 0, n = transformConstraints.length; i < n; i++) {
				let constraint = transformConstraints[i];
				if (constraint.name == constraintName) return constraint;
			}
			return null;
		}

		/** Finds a path constraint by comparing each path constraint's name. It is more efficient to cache the results of this method
		 * than to call it multiple times.
		 * @return May be null. */
		findPathConstraint (constraintName: string) {
			if (constraintName == null) throw new Error("constraintName cannot be null.");
			let pathConstraints = this.pathConstraints;
			for (let i = 0, n = pathConstraints.length; i < n; i++) {
				let constraint = pathConstraints[i];
				if (constraint.name == constraintName) return constraint;
			}
			return null;
		}

		findPathConstraintIndex (pathConstraintName: string) {
			if (pathConstraintName == null) throw new Error("pathConstraintName cannot be null.");
			let pathConstraints = this.pathConstraints;
			for (let i = 0, n = pathConstraints.length; i < n; i++)
				if (pathConstraints[i].name == pathConstraintName) return i;
			return -1;
		}
	}
}
