/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
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
 ******************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;

public class SkeletonData {
	String name;
	final Array<BoneData> bones = new Array(); // Ordered parents first.
	final Array<SlotData> slots = new Array(); // Setup pose draw order.
	final Array<Skin> skins = new Array();
	Skin defaultSkin;
	final Array<Animation> animations = new Array();

	public void clear () {
		bones.clear();
		slots.clear();
		skins.clear();
		animations.clear();
		defaultSkin = null;
	}

	// --- Bones.

	public void addBone (BoneData bone) {
		if (bone == null) throw new IllegalArgumentException("bone cannot be null.");
		bones.add(bone);
	}

	public Array<BoneData> getBones () {
		return bones;
	}

	/** @return May be null. */
	public BoneData findBone (String boneName) {
		if (boneName == null) throw new IllegalArgumentException("boneName cannot be null.");
		Array<BoneData> bones = this.bones;
		for (int i = 0, n = bones.size; i < n; i++) {
			BoneData bone = bones.get(i);
			if (bone.name.equals(boneName)) return bone;
		}
		return null;
	}

	/** @return -1 if the bone was not found. */
	public int findBoneIndex (String boneName) {
		if (boneName == null) throw new IllegalArgumentException("boneName cannot be null.");
		Array<BoneData> bones = this.bones;
		for (int i = 0, n = bones.size; i < n; i++)
			if (bones.get(i).name.equals(boneName)) return i;
		return -1;
	}

	// --- Slots.

	public void addSlot (SlotData slot) {
		if (slot == null) throw new IllegalArgumentException("slot cannot be null.");
		slots.add(slot);
	}

	public Array<SlotData> getSlots () {
		return slots;
	}

	/** @return May be null. */
	public SlotData findSlot (String slotName) {
		if (slotName == null) throw new IllegalArgumentException("slotName cannot be null.");
		Array<SlotData> slots = this.slots;
		for (int i = 0, n = slots.size; i < n; i++) {
			SlotData slot = slots.get(i);
			if (slot.name.equals(slotName)) return slot;
		}
		return null;
	}

	/** @return -1 if the bone was not found. */
	public int findSlotIndex (String slotName) {
		if (slotName == null) throw new IllegalArgumentException("slotName cannot be null.");
		Array<SlotData> slots = this.slots;
		for (int i = 0, n = slots.size; i < n; i++)
			if (slots.get(i).name.equals(slotName)) return i;
		return -1;
	}

	// --- Skins.

	/** @return May be null. */
	public Skin getDefaultSkin () {
		return defaultSkin;
	}

	/** @param defaultSkin May be null. */
	public void setDefaultSkin (Skin defaultSkin) {
		this.defaultSkin = defaultSkin;
	}

	public void addSkin (Skin skin) {
		if (skin == null) throw new IllegalArgumentException("skin cannot be null.");
		skins.add(skin);
	}

	/** @return May be null. */
	public Skin findSkin (String skinName) {
		if (skinName == null) throw new IllegalArgumentException("skinName cannot be null.");
		for (Skin skin : skins)
			if (skin.name.equals(skinName)) return skin;
		return null;
	}

	/** Returns all skins, including the default skin. */
	public Array<Skin> getSkins () {
		return skins;
	}

	// --- Animations.

	public void addAnimation (Animation animation) {
		if (animation == null) throw new IllegalArgumentException("animation cannot be null.");
		animations.add(animation);
	}

	public Array<Animation> getAnimations () {
		return animations;
	}

	/** @return May be null. */
	public Animation findAnimation (String animationName) {
		if (animationName == null) throw new IllegalArgumentException("animationName cannot be null.");
		Array<Animation> animations = this.animations;
		for (int i = 0, n = animations.size; i < n; i++) {
			Animation animation = animations.get(i);
			if (animation.name.equals(animationName)) return animation;
		}
		return null;
	}

	// ---

	/** @return May be null. */
	public String getName () {
		return name;
	}

	/** @param name May be null. */
	public void setName (String name) {
		this.name = name;
	}

	public String toString () {
		return name != null ? name : super.toString();
	}
}
