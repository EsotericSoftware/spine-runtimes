/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.esotericsoftware.spine.attachments.Attachment;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.utils.Array;

public class Skeleton {
	final SkeletonData data;
	final Array<Bone> bones;
	final Array<Slot> slots;
	Array<Slot> drawOrder;
	Skin skin;
	final Color color;
	float time;
	boolean flipX, flipY;
	float x, y;

	public Skeleton (SkeletonData data) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.data = data;

		bones = new Array(data.bones.size);
		for (BoneData boneData : data.bones) {
			Bone parent = boneData.parent == null ? null : bones.get(data.bones.indexOf(boneData.parent, true));
			bones.add(new Bone(boneData, parent));
		}

		slots = new Array(data.slots.size);
		drawOrder = new Array(data.slots.size);
		for (SlotData slotData : data.slots) {
			Bone bone = bones.get(data.bones.indexOf(slotData.boneData, true));
			Slot slot = new Slot(slotData, this, bone);
			slots.add(slot);
			drawOrder.add(slot);
		}

		color = new Color(1, 1, 1, 1);
	}

	/** Copy constructor. */
	public Skeleton (Skeleton skeleton) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		data = skeleton.data;

		bones = new Array(skeleton.bones.size);
		for (Bone bone : skeleton.bones) {
			Bone parent = bone.parent == null ? null : bones.get(skeleton.bones.indexOf(bone.parent, true));
			bones.add(new Bone(bone, parent));
		}

		slots = new Array(skeleton.slots.size);
		for (Slot slot : skeleton.slots) {
			Bone bone = bones.get(skeleton.bones.indexOf(slot.bone, true));
			Slot newSlot = new Slot(slot, this, bone);
			slots.add(newSlot);
		}

		drawOrder = new Array(slots.size);
		for (Slot slot : skeleton.drawOrder)
			drawOrder.add(slots.get(skeleton.slots.indexOf(slot, true)));

		skin = skeleton.skin;
		color = new Color(skeleton.color);
		time = skeleton.time;
	}

	/** Updates the world transform for each bone. */
	public void updateWorldTransform () {
		boolean flipX = this.flipX;
		boolean flipY = this.flipY;
		Array<Bone> bones = this.bones;
		for (int i = 0, n = bones.size; i < n; i++)
			bones.get(i).updateWorldTransform(flipX, flipY);
	}

	/** Sets the bones and slots to their setup pose values. */
	public void setToSetupPose () {
		setBonesToSetupPose();
		setSlotsToSetupPose();
	}

	public void setBonesToSetupPose () {
		Array<Bone> bones = this.bones;
		for (int i = 0, n = bones.size; i < n; i++)
			bones.get(i).setToSetupPose();
	}

	public void setSlotsToSetupPose () {
		Array<Slot> slots = this.slots;
		System.arraycopy(slots.items, 0, drawOrder.items, 0, slots.size);		
		for (int i = 0, n = slots.size; i < n; i++)
			slots.get(i).setToSetupPose(i);
	}

	public SkeletonData getData () {
		return data;
	}

	public Array<Bone> getBones () {
		return bones;
	}

	/** @return May return null. */
	public Bone getRootBone () {
		if (bones.size == 0) return null;
		return bones.first();
	}

	/** @return May be null. */
	public Bone findBone (String boneName) {
		if (boneName == null) throw new IllegalArgumentException("boneName cannot be null.");
		Array<Bone> bones = this.bones;
		for (int i = 0, n = bones.size; i < n; i++) {
			Bone bone = bones.get(i);
			if (bone.data.name.equals(boneName)) return bone;
		}
		return null;
	}

	/** @return -1 if the bone was not found. */
	public int findBoneIndex (String boneName) {
		if (boneName == null) throw new IllegalArgumentException("boneName cannot be null.");
		Array<Bone> bones = this.bones;
		for (int i = 0, n = bones.size; i < n; i++)
			if (bones.get(i).data.name.equals(boneName)) return i;
		return -1;
	}

	public Array<Slot> getSlots () {
		return slots;
	}

	/** @return May be null. */
	public Slot findSlot (String slotName) {
		if (slotName == null) throw new IllegalArgumentException("slotName cannot be null.");
		Array<Slot> slots = this.slots;
		for (int i = 0, n = slots.size; i < n; i++) {
			Slot slot = slots.get(i);
			if (slot.data.name.equals(slotName)) return slot;
		}
		return null;
	}

	/** @return -1 if the bone was not found. */
	public int findSlotIndex (String slotName) {
		if (slotName == null) throw new IllegalArgumentException("slotName cannot be null.");
		Array<Slot> slots = this.slots;
		for (int i = 0, n = slots.size; i < n; i++)
			if (slots.get(i).data.name.equals(slotName)) return i;
		return -1;
	}

	/** Returns the slots in the order they will be drawn. The returned array may be modified to change the draw order. */
	public Array<Slot> getDrawOrder () {
		return drawOrder;
	}

	/** Sets the slots and the order they will be drawn. */
	public void setDrawOrder (Array<Slot> drawOrder) {
		this.drawOrder = drawOrder;
	}

	/** @return May be null. */
	public Skin getSkin () {
		return skin;
	}

	/** Sets a skin by name.
	 * @see #setSkin(Skin) */
	public void setSkin (String skinName) {
		Skin skin = data.findSkin(skinName);
		if (skin == null) throw new IllegalArgumentException("Skin not found: " + skinName);
		setSkin(skin);
	}

	/** Sets the skin used to look up attachments not found in the {@link SkeletonData#getDefaultSkin() default skin}. Attachments
	 * from the new skin are attached if the corresponding attachment from the old skin was attached.
	 * @param newSkin May be null. */
	public void setSkin (Skin newSkin) {
		if (skin != null && newSkin != null) newSkin.attachAll(this, skin);
		skin = newSkin;
	}

	/** @return May be null. */
	public Attachment getAttachment (String slotName, String attachmentName) {
		return getAttachment(data.findSlotIndex(slotName), attachmentName);
	}

	/** @return May be null. */
	public Attachment getAttachment (int slotIndex, String attachmentName) {
		if (attachmentName == null) throw new IllegalArgumentException("attachmentName cannot be null.");
		if (skin != null) {
			Attachment attachment = skin.getAttachment(slotIndex, attachmentName);
			if (attachment != null) return attachment;
		}
		if (data.defaultSkin != null) return data.defaultSkin.getAttachment(slotIndex, attachmentName);
		return null;
	}

	/** @param attachmentName May be null. */
	public void setAttachment (String slotName, String attachmentName) {
		if (slotName == null) throw new IllegalArgumentException("slotName cannot be null.");
		Array<Slot> slots = this.slots;
		for (int i = 0, n = slots.size; i < n; i++) {
			Slot slot = slots.get(i);
			if (slot.data.name.equals(slotName)) {
				Attachment attachment = null;
				if (attachmentName != null) {
					attachment = getAttachment(i, attachmentName);
					if (attachment == null)
						throw new IllegalArgumentException("Attachment not found: " + attachmentName + ", for slot: " + slotName);
				}
				slot.setAttachment(attachment);
				return;
			}
		}
		throw new IllegalArgumentException("Slot not found: " + slotName);
	}

	public Color getColor () {
		return color;
	}

	public boolean getFlipX () {
		return flipX;
	}

	public void setFlipX (boolean flipX) {
		this.flipX = flipX;
	}

	public boolean getFlipY () {
		return flipY;
	}

	public void setFlipY (boolean flipY) {
		this.flipY = flipY;
	}

	public float getX () {
		return x;
	}

	public void setX (float x) {
		this.x = x;
	}

	public float getY () {
		return y;
	}

	public void setY (float y) {
		this.y = y;
	}

	public float getTime () {
		return time;
	}

	public void setTime (float time) {
		this.time = time;
	}

	public void update (float delta) {
		time += delta;
	}

	public String toString () {
		return data.name != null ? data.name : super.toString();
	}
}
