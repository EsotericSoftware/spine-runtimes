/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine {
	export class SkeletonData {
		name: string;
		bones = new Array<BoneData>(); // Ordered parents first.
		slots = new Array<SlotData>(); // Setup pose draw order.
		skins = new Array<Skin>();
		defaultSkin: Skin;
		events = new Array<EventData>();
		animations = new Array<Animation>();
		ikConstraints = new Array<IkConstraintData>();
		transformConstraints = new Array<TransformConstraintData>();
		pathConstraints = new Array<PathConstraintData>();
		x: number; y: number; width: number; height: number;
		version: string; hash: string;

		// Nonessential
		fps = 0;
		imagesPath: string;
		audioPath: string;

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

		findSkin (skinName: string) {
			if (skinName == null) throw new Error("skinName cannot be null.");
			let skins = this.skins;
			for (let i = 0, n = skins.length; i < n; i++) {
				let skin = skins[i];
				if (skin.name == skinName) return skin;
			}
			return null;
		}

		findEvent (eventDataName: string) {
			if (eventDataName == null) throw new Error("eventDataName cannot be null.");
			let events = this.events;
			for (let i = 0, n = events.length; i < n; i++) {
				let event = events[i];
				if (event.name == eventDataName) return event;
			}
			return null;
		}

		findAnimation (animationName: string) {
			if (animationName == null) throw new Error("animationName cannot be null.");
			let animations = this.animations;
			for (let i = 0, n = animations.length; i < n; i++) {
				let animation = animations[i];
				if (animation.name == animationName) return animation;
			}
			return null;
		}

		findIkConstraint (constraintName: string) {
			if (constraintName == null) throw new Error("constraintName cannot be null.");
			let ikConstraints = this.ikConstraints;
			for (let i = 0, n = ikConstraints.length; i < n; i++) {
				let constraint = ikConstraints[i];
				if (constraint.name == constraintName) return constraint;
			}
			return null;
		}

		findTransformConstraint (constraintName: string) {
			if (constraintName == null) throw new Error("constraintName cannot be null.");
			let transformConstraints = this.transformConstraints;
			for (let i = 0, n = transformConstraints.length; i < n; i++) {
				let constraint = transformConstraints[i];
				if (constraint.name == constraintName) return constraint;
			}
			return null;
		}

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
