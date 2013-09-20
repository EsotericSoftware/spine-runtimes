/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine {
import spine.animation.Animation;

public class SkeletonData {
	public var name:String;
	public var bones:Vector.<BoneData> = new Vector.<BoneData>(); // Ordered parents first.
	public var slots:Vector.<SlotData> = new Vector.<SlotData>(); // Setup pose draw order.
	public var skins:Vector.<Skin> = new Vector.<Skin>();
	public var defaultSkin:Skin;
	public var animations:Vector.<Animation> = new Vector.<Animation>();

	// --- Bones.

	public function addBone (bone:BoneData) : void {
		if (bone == null)
			throw new ArgumentError("bone cannot be null.");
		bones.push(bone);
	}

	/** @return May be null. */
	public function findBone (boneName:String) : BoneData {
		if (boneName == null)
			throw new ArgumentError("boneName cannot be null.");
		for (var i:int = 0, n:int = bones.length; i < n; i++) {
			var bone:BoneData = bones[i];
			if (bone._name == boneName)
				return bone;
		}
		return null;
	}

	/** @return -1 if the bone was not found. */
	public function findBoneIndex (boneName:String) : int {
		if (boneName == null)
			throw new ArgumentError("boneName cannot be null.");
		for (var i:int = 0, n:int = bones.length; i < n; i++)
			if (bones[i]._name == boneName)
				return i;
		return -1;
	}

	// --- Slots.

	public function addSlot (slot:SlotData) : void {
		if (slot == null)
			throw new ArgumentError("slot cannot be null.");
		slots.push(slot);
	}

	/** @return May be null. */
	public function findSlot (slotName:String) : SlotData {
		if (slotName == null)
			throw new ArgumentError("slotName cannot be null.");
		for (var i:int = 0, n:int = slots.length; i < n; i++) {
			var slot:SlotData = slots[i];
			if (slot._name == slotName)
				return slot;
		}
		return null;
	}

	/** @return -1 if the bone was not found. */
	public function findSlotIndex (slotName:String) : int {
		if (slotName == null)
			throw new ArgumentError("slotName cannot be null.");
		for (var i:int = 0, n:int = slots.length; i < n; i++)
			if (slots[i]._name == slotName)
				return i;
		return -1;
	}

	// --- Skins.

	public function addSkin (skin:Skin) : void {
		if (skin == null)
			throw new ArgumentError("skin cannot be null.");
		skins.push(skin);
	}

	/** @return May be null. */
	public function findSkin (skinName:String) : Skin {
		if (skinName == null)
			throw new ArgumentError("skinName cannot be null.");
		for each (var skin:Skin in skins)
			if (skin._name == skinName)
				return skin;
		return null;
	}

	// --- Animations.

	public function addAnimation (animation:Animation) : void {
		if (animation == null)
			throw new ArgumentError("animation cannot be null.");
		animations.push(animation);
	}

	/** @return May be null. */
	public function findAnimation (animationName:String) : Animation {
		if (animationName == null)
			throw new ArgumentError("animationName cannot be null.");
		for (var i:int = 0, n:int = animations.length; i < n; i++) {
			var animation:Animation = animations[i];
			if (animation.name == animationName)
				return animation;
		}
		return null;
	}

	// ---

	public function toString () : String {
		return name != null ? name : super.toString();
	}
}

}
