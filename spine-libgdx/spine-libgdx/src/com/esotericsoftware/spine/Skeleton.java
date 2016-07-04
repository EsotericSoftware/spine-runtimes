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
import com.badlogic.gdx.utils.ObjectMap.Entry;
import com.esotericsoftware.spine.Skin.Key;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.PathAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;

public class Skeleton {
	final SkeletonData data;
	final Array<Bone> bones;
	final Array<Slot> slots;
	Array<Slot> drawOrder;
	final Array<IkConstraint> ikConstraints, ikConstraintsSorted;
	final Array<TransformConstraint> transformConstraints;
	final Array<PathConstraint> pathConstraints;
	final Array<Updatable> updateCache = new Array();
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
			Bone bone;
			if (boneData.parent == null)
				bone = new Bone(boneData, this, null);
			else {
				Bone parent = bones.get(boneData.parent.index);
				bone = new Bone(boneData, this, parent);
				parent.children.add(bone);
			}
			bones.add(bone);
		}

		slots = new Array(data.slots.size);
		drawOrder = new Array(data.slots.size);
		for (SlotData slotData : data.slots) {
			Bone bone = bones.get(slotData.boneData.index);
			Slot slot = new Slot(slotData, bone);
			slots.add(slot);
			drawOrder.add(slot);
		}

		ikConstraints = new Array(data.ikConstraints.size);
		ikConstraintsSorted = new Array(ikConstraints.size);
		for (IkConstraintData ikConstraintData : data.ikConstraints)
			ikConstraints.add(new IkConstraint(ikConstraintData, this));

		transformConstraints = new Array(data.transformConstraints.size);
		for (TransformConstraintData transformConstraintData : data.transformConstraints)
			transformConstraints.add(new TransformConstraint(transformConstraintData, this));

		pathConstraints = new Array(data.pathConstraints.size);
		for (PathConstraintData pathConstraintData : data.pathConstraints)
			pathConstraints.add(new PathConstraint(pathConstraintData, this));

		color = new Color(1, 1, 1, 1);

		updateCache();
	}

	/** Copy constructor. */
	public Skeleton (Skeleton skeleton) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		data = skeleton.data;

		bones = new Array(skeleton.bones.size);
		for (Bone bone : skeleton.bones) {
			Bone copy;
			if (bone.parent == null)
				copy = new Bone(bone, this, null);
			else {
				Bone parent = bones.get(bone.parent.data.index);
				copy = new Bone(bone, this, parent);
				parent.children.add(copy);
			}
			bones.add(copy);
		}

		slots = new Array(skeleton.slots.size);
		for (Slot slot : skeleton.slots) {
			Bone bone = bones.get(slot.bone.data.index);
			slots.add(new Slot(slot, bone));
		}

		drawOrder = new Array(slots.size);
		for (Slot slot : skeleton.drawOrder)
			drawOrder.add(slots.get(slot.data.index));

		ikConstraints = new Array(skeleton.ikConstraints.size);
		ikConstraintsSorted = new Array(ikConstraints.size);
		for (IkConstraint ikConstraint : skeleton.ikConstraints)
			ikConstraints.add(new IkConstraint(ikConstraint, this));

		transformConstraints = new Array(skeleton.transformConstraints.size);
		for (TransformConstraint transformConstraint : skeleton.transformConstraints)
			transformConstraints.add(new TransformConstraint(transformConstraint, this));

		pathConstraints = new Array(skeleton.pathConstraints.size);
		for (PathConstraint pathConstraint : skeleton.pathConstraints)
			pathConstraints.add(new PathConstraint(pathConstraint, this));

		skin = skeleton.skin;
		color = new Color(skeleton.color);
		time = skeleton.time;
		flipX = skeleton.flipX;
		flipY = skeleton.flipY;

		updateCache();
	}

	/** Caches information about bones and constraints. Must be called if bones, constraints, or weighted path attachments are
	 * added or removed. */
	public void updateCache () {
		Array<Updatable> updateCache = this.updateCache;
		updateCache.clear();

		Array<Bone> bones = this.bones;
		for (int i = 0, n = bones.size; i < n; i++)
			bones.get(i).sorted = false;

		// IK first, lowest hierarchy depth first.
		Array<IkConstraint> ikConstraints = this.ikConstraintsSorted;
		ikConstraints.clear();
		ikConstraints.addAll(this.ikConstraints);
		int ikCount = ikConstraints.size;
		for (int i = 0, level, n = ikCount; i < n; i++) {
			IkConstraint ik = ikConstraints.get(i);
			Bone bone = ik.bones.first().parent;
			for (level = 0; bone != null; level++)
				bone = bone.parent;
			ik.level = level;
		}
		for (int i = 1, ii; i < ikCount; i++) {
			IkConstraint ik = ikConstraints.get(i);
			int level = ik.level;
			for (ii = i - 1; ii >= 0; ii--) {
				IkConstraint other = ikConstraints.get(ii);
				if (other.level < level) break;
				ikConstraints.set(ii + 1, other);
			}
			ikConstraints.set(ii + 1, ik);
		}
		for (int i = 0, n = ikConstraints.size; i < n; i++) {
			IkConstraint constraint = ikConstraints.get(i);
			Bone target = constraint.target;
			sortBone(target);

			Array<Bone> constrained = constraint.bones;
			Bone parent = constrained.first();
			sortBone(parent);

			updateCache.add(constraint);

			sortReset(parent.children);
			constrained.peek().sorted = true;
		}

		Array<PathConstraint> pathConstraints = this.pathConstraints;
		for (int i = 0, n = pathConstraints.size; i < n; i++) {
			PathConstraint constraint = pathConstraints.get(i);

			Slot slot = constraint.target;
			int slotIndex = slot.getData().index;
			Bone slotBone = slot.bone;
			if (skin != null) sortPathConstraintAttachment(skin, slotIndex, slotBone);
			if (data.defaultSkin != null && data.defaultSkin != skin)
				sortPathConstraintAttachment(data.defaultSkin, slotIndex, slotBone);
			for (int ii = 0, nn = data.skins.size; ii < nn; ii++)
				sortPathConstraintAttachment(data.skins.get(ii), slotIndex, slotBone);

			Attachment attachment = slot.attachment;
			if (attachment instanceof PathAttachment) sortPathConstraintAttachment(attachment, slotBone);

			Array<Bone> constrained = constraint.bones;
			int boneCount = constrained.size;
			for (int ii = 0; ii < boneCount; ii++)
				sortBone(constrained.get(ii));

			updateCache.add(constraint);

			for (int ii = 0; ii < boneCount; ii++)
				sortReset(constrained.get(ii).children);
			for (int ii = 0; ii < boneCount; ii++)
				constrained.get(ii).sorted = true;
		}

		Array<TransformConstraint> transformConstraints = this.transformConstraints;
		for (int i = 0, n = transformConstraints.size; i < n; i++) {
			TransformConstraint constraint = transformConstraints.get(i);

			sortBone(constraint.target);

			Array<Bone> constrained = constraint.bones;
			int boneCount = constrained.size;
			for (int ii = 0; ii < boneCount; ii++)
				sortBone(constrained.get(ii));

			updateCache.add(constraint);

			for (int ii = 0; ii < boneCount; ii++)
				sortReset(constrained.get(ii).children);
			for (int ii = 0; ii < boneCount; ii++)
				constrained.get(ii).sorted = true;
		}

		for (int i = 0, n = bones.size; i < n; i++)
			sortBone(bones.get(i));
	}

	private void sortPathConstraintAttachment (Skin skin, int slotIndex, Bone slotBone) {
		for (Entry<Key, Attachment> entry : skin.attachments.entries())
			if (entry.key.slotIndex == slotIndex) sortPathConstraintAttachment(entry.value, slotBone);
	}

	private void sortPathConstraintAttachment (Attachment attachment, Bone slotBone) {
		if (!(attachment instanceof PathAttachment)) return;
		int[] pathBones = ((PathAttachment)attachment).getBones();
		if (pathBones == null)
			sortBone(slotBone);
		else {
			Array<Bone> bones = this.bones;
			for (int boneIndex : pathBones)
				sortBone(bones.get(boneIndex));
		}
	}

	private void sortBone (Bone bone) {
		if (bone.sorted) return;
		Bone parent = bone.parent;
		if (parent != null) sortBone(parent);
		bone.sorted = true;
		updateCache.add(bone);
	}

	private void sortReset (Array<Bone> bones) {
		for (int i = 0, n = bones.size; i < n; i++) {
			Bone bone = bones.get(i);
			if (bone.sorted) sortReset(bone.children);
			bone.sorted = false;
		}
	}

	/** Updates the world transform for each bone and applies constraints. */
	public void updateWorldTransform () {
		Array<Updatable> updateCache = this.updateCache;
		for (int i = 0, n = updateCache.size; i < n; i++)
			updateCache.get(i).update();
	}

	/** Sets the bones, constraints, and slots to their setup pose values. */
	public void setToSetupPose () {
		setBonesToSetupPose();
		setSlotsToSetupPose();
	}

	/** Sets the bones and constraints to their setup pose values. */
	public void setBonesToSetupPose () {
		Array<Bone> bones = this.bones;
		for (int i = 0, n = bones.size; i < n; i++)
			bones.get(i).setToSetupPose();

		Array<IkConstraint> ikConstraints = this.ikConstraints;
		for (int i = 0, n = ikConstraints.size; i < n; i++) {
			IkConstraint constraint = ikConstraints.get(i);
			constraint.bendDirection = constraint.data.bendDirection;
			constraint.mix = constraint.data.mix;
		}

		Array<TransformConstraint> transformConstraints = this.transformConstraints;
		for (int i = 0, n = transformConstraints.size; i < n; i++) {
			TransformConstraint constraint = transformConstraints.get(i);
			TransformConstraintData data = constraint.data;
			constraint.rotateMix = data.rotateMix;
			constraint.translateMix = data.translateMix;
			constraint.scaleMix = data.scaleMix;
			constraint.shearMix = data.shearMix;
		}

		Array<PathConstraint> pathConstraints = this.pathConstraints;
		for (int i = 0, n = pathConstraints.size; i < n; i++) {
			PathConstraint constraint = pathConstraints.get(i);
			PathConstraintData data = constraint.data;
			constraint.position = data.position;
			constraint.spacing = data.spacing;
			constraint.rotateMix = data.rotateMix;
			constraint.translateMix = data.translateMix;
		}
	}

	public void setSlotsToSetupPose () {
		Array<Slot> slots = this.slots;
		System.arraycopy(slots.items, 0, drawOrder.items, 0, slots.size);
		for (int i = 0, n = slots.size; i < n; i++)
			slots.get(i).setToSetupPose();
	}

	public SkeletonData getData () {
		return data;
	}

	public Array<Bone> getBones () {
		return bones;
	}

	public Array<Updatable> getUpdateCache () {
		return updateCache;
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
		if (drawOrder == null) throw new IllegalArgumentException("drawOrder cannot be null.");
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
	public IkConstraint findIkConstraint (String constraintName) {
		if (constraintName == null) throw new IllegalArgumentException("constraintName cannot be null.");
		Array<IkConstraint> ikConstraints = this.ikConstraints;
		for (int i = 0, n = ikConstraints.size; i < n; i++) {
			IkConstraint ikConstraint = ikConstraints.get(i);
			if (ikConstraint.data.name.equals(constraintName)) return ikConstraint;
		}
		return null;
	}

	public Array<TransformConstraint> getTransformConstraints () {
		return transformConstraints;
	}

	/** @return May be null. */
	public TransformConstraint findTransformConstraint (String constraintName) {
		if (constraintName == null) throw new IllegalArgumentException("constraintName cannot be null.");
		Array<TransformConstraint> transformConstraints = this.transformConstraints;
		for (int i = 0, n = transformConstraints.size; i < n; i++) {
			TransformConstraint constraint = transformConstraints.get(i);
			if (constraint.data.name.equals(constraintName)) return constraint;
		}
		return null;
	}

	public Array<PathConstraint> getPathConstraints () {
		return pathConstraints;
	}

	/** @return May be null. */
	public PathConstraint findPathConstraint (String constraintName) {
		if (constraintName == null) throw new IllegalArgumentException("constraintName cannot be null.");
		Array<PathConstraint> pathConstraints = this.pathConstraints;
		for (int i = 0, n = pathConstraints.size; i < n; i++) {
			PathConstraint constraint = pathConstraints.get(i);
			if (constraint.data.name.equals(constraintName)) return constraint;
		}
		return null;
	}

	/** Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose.
	 * @param offset The distance from the skeleton origin to the bottom left corner of the AABB.
	 * @param size The width and height of the AABB. */
	public void getBounds (Vector2 offset, Vector2 size) {
		if (offset == null) throw new IllegalArgumentException("offset cannot be null.");
		if (size == null) throw new IllegalArgumentException("size cannot be null.");
		Array<Slot> drawOrder = this.drawOrder;
		float minX = Integer.MAX_VALUE, minY = Integer.MAX_VALUE, maxX = Integer.MIN_VALUE, maxY = Integer.MIN_VALUE;
		for (int i = 0, n = drawOrder.size; i < n; i++) {
			Slot slot = drawOrder.get(i);
			float[] vertices = null;
			Attachment attachment = slot.attachment;
			if (attachment instanceof RegionAttachment)
				vertices = ((RegionAttachment)attachment).updateWorldVertices(slot, false);
			else if (attachment instanceof MeshAttachment) //
				vertices = ((MeshAttachment)attachment).updateWorldVertices(slot, true);
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
		if (color == null) throw new IllegalArgumentException("color cannot be null.");
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
