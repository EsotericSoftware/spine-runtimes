/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.SkinnedMeshAttachment;

public class Skeleton {
	final SkeletonData data;
	final Array<Bone> bones;
	final Array<Slot> slots;
	Array<Slot> drawOrder;
	final Array<IkConstraint> ikConstraints;
	private final Array<Array<Bone>> boneCache = new Array();
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
			bones.add(new Bone(boneData, this, parent));
		}

		slots = new Array(data.slots.size);
		drawOrder = new Array(data.slots.size);
		for (SlotData slotData : data.slots) {
			Bone bone = bones.get(data.bones.indexOf(slotData.boneData, true));
			Slot slot = new Slot(slotData, bone);
			slots.add(slot);
			drawOrder.add(slot);
		}

		ikConstraints = new Array(data.ikConstraints.size);
		for (IkConstraintData ikConstraintData : data.ikConstraints)
			ikConstraints.add(new IkConstraint(ikConstraintData, this));

		color = new Color(1, 1, 1, 1);

		updateCache();
	}

	/** Copy constructor. */
	public Skeleton (Skeleton skeleton) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		data = skeleton.data;

		bones = new Array(skeleton.bones.size);
		for (Bone bone : skeleton.bones) {
			Bone parent = bone.parent == null ? null : bones.get(skeleton.bones.indexOf(bone.parent, true));
			bones.add(new Bone(bone, this, parent));
		}

		slots = new Array(skeleton.slots.size);
		for (Slot slot : skeleton.slots) {
			Bone bone = bones.get(skeleton.bones.indexOf(slot.bone, true));
			slots.add(new Slot(slot, bone));
		}

		drawOrder = new Array(slots.size);
		for (Slot slot : skeleton.drawOrder)
			drawOrder.add(slots.get(skeleton.slots.indexOf(slot, true)));

		ikConstraints = new Array(skeleton.ikConstraints.size);
		for (IkConstraint ikConstraint : skeleton.ikConstraints) {
			Bone target = bones.get(skeleton.bones.indexOf(ikConstraint.target, true));
			Array<Bone> ikBones = new Array(ikConstraint.bones.size);
			for (Bone bone : ikConstraint.bones)
				ikBones.add(bones.get(skeleton.bones.indexOf(bone, true)));
			ikConstraints.add(new IkConstraint(ikConstraint, ikBones, target));
		}

		skin = skeleton.skin;
		color = new Color(skeleton.color);
		time = skeleton.time;
		flipX = skeleton.flipX;
		flipY = skeleton.flipY;

		updateCache();
	}

	/** Caches information about bones and IK constraints. Must be called if bones or IK constraints are added or removed. */
	public void updateCache () {
		Array<Bone> bones = this.bones;
		Array<Array<Bone>> boneCache = this.boneCache;
		Array<IkConstraint> ikConstraints = this.ikConstraints;
		int ikConstraintsCount = ikConstraints.size;

		int arrayCount = ikConstraintsCount + 1;
		while (boneCache.size < arrayCount)
			boneCache.add(new Array());
		for (int i = 0; i < arrayCount; i++)
			boneCache.get(i).clear();

		Array<Bone> nonIkBones = boneCache.first();

		outer:
		for (int i = 0, n = bones.size; i < n; i++) {
			Bone bone = bones.get(i);
			Bone current = bone;
			do {
				for (int ii = 0; ii < ikConstraintsCount; ii++) {
					IkConstraint ikConstraint = ikConstraints.get(ii);
					Bone parent = ikConstraint.bones.first();
					Bone child = ikConstraint.bones.peek();
					while (true) {
						if (current == child) {
							boneCache.get(ii).add(bone);
							boneCache.get(ii + 1).add(bone);
							continue outer;
						}
						if (child == parent) break;
						child = child.parent;
					}
				}
				current = current.parent;
			} while (current != null);
			nonIkBones.add(bone);
		}
	}

	/** Updates the world transform for each bone and applies IK constraints. */
	public void updateWorldTransform () {
		Array<Bone> bones = this.bones;
		for (int i = 0, nn = bones.size; i < nn; i++) {
			Bone bone = bones.get(i);
			bone.rotationIK = bone.rotation;
		}
		Array<Array<Bone>> boneCache = this.boneCache;
		Array<IkConstraint> ikConstraints = this.ikConstraints;
		int i = 0, last = ikConstraints.size;
		while (true) {
			Array<Bone> updateBones = boneCache.get(i);
			for (int ii = 0, nn = updateBones.size; ii < nn; ii++)
				updateBones.get(ii).updateWorldTransform();
			if (i == last) break;
			ikConstraints.get(i).apply();
			i++;
		}
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

		Array<IkConstraint> ikConstraints = this.ikConstraints;
		for (int i = 0, n = ikConstraints.size; i < n; i++) {
			IkConstraint ikConstraint = ikConstraints.get(i);
			ikConstraint.bendDirection = ikConstraint.data.bendDirection;
			ikConstraint.mix = ikConstraint.data.mix;
		}
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

	/** Sets the skin used to look up attachments before looking in the {@link SkeletonData#getDefaultSkin() default skin}.
	 * Attachments from the new skin are attached if the corresponding attachment from the old skin was attached. If there was no
	 * old skin, each slot's setup mode attachment is attached from the new skin.
	 * @param newSkin May be null. */
	public void setSkin (Skin newSkin) {
		if (newSkin != null) {
			if (skin != null)
				newSkin.attachAll(this, skin);
			else {
				Array<Slot> slots = this.slots;
				for (int i = 0, n = slots.size; i < n; i++) {
					Slot slot = slots.get(i);
					String name = slot.data.attachmentName;
					if (name != null) {
						Attachment attachment = newSkin.getAttachment(i, name);
						if (attachment != null) slot.setAttachment(attachment);
					}
				}
			}
		}
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

	public Array<IkConstraint> getIkConstraints () {
		return ikConstraints;
	}

	/** @return May be null. */
	public IkConstraint findIkConstraint (String ikConstraintName) {
		if (ikConstraintName == null) throw new IllegalArgumentException("ikConstraintName cannot be null.");
		Array<IkConstraint> ikConstraints = this.ikConstraints;
		for (int i = 0, n = ikConstraints.size; i < n; i++) {
			IkConstraint ikConstraint = ikConstraints.get(i);
			if (ikConstraint.data.name.equals(ikConstraintName)) return ikConstraint;
		}
		return null;
	}

	/** Returns the axis aligned bounding box (AABB) of the region, mesh, and skinned mesh attachments for the current pose.
	 * @param offset The distance from the skeleton origin to the bottom left corner of the AABB.
	 * @param size The width and height of the AABB. */
	public void getBounds (Vector2 offset, Vector2 size) {
		Array<Slot> drawOrder = this.drawOrder;
		float minX = Integer.MAX_VALUE, minY = Integer.MAX_VALUE, maxX = Integer.MIN_VALUE, maxY = Integer.MIN_VALUE;
		for (int i = 0, n = drawOrder.size; i < n; i++) {
			Slot slot = drawOrder.get(i);
			float[] vertices = null;
			Attachment attachment = slot.attachment;
			if (attachment instanceof RegionAttachment) {
				RegionAttachment region = (RegionAttachment)attachment;
				region.updateWorldVertices(slot, false);
				vertices = region.getWorldVertices();

			} else if (attachment instanceof MeshAttachment) {
				MeshAttachment mesh = (MeshAttachment)attachment;
				mesh.updateWorldVertices(slot, true);
				vertices = mesh.getWorldVertices();

			} else if (attachment instanceof SkinnedMeshAttachment) {
				SkinnedMeshAttachment mesh = (SkinnedMeshAttachment)attachment;
				mesh.updateWorldVertices(slot, true);
				vertices = mesh.getWorldVertices();
			}
			if (vertices != null) {
				for (int ii = 0, nn = vertices.length; ii < nn; ii += 5) {
					float x = vertices[ii], y = vertices[ii + 1];
					minX = Math.min(minX, x);
					minY = Math.min(minY, y);
					maxX = Math.max(maxX, x);
					maxY = Math.max(maxY, y);
				}
			}
		}
		offset.set(minX, minY);
		size.set(maxX - minX, maxY - minY);
	}

	public Color getColor () {
		return color;
	}

	/** A convenience method for setting the skeleton color. The color can also be set by modifying {@link #getColor()}. */
	public void setColor (Color color) {
		this.color.set(color);
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

	public void setFlip (boolean flipX, boolean flipY) {
		this.flipX = flipX;
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

	public void setPosition (float x, float y) {
		this.x = x;
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
