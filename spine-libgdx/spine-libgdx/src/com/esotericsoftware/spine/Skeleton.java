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

package com.esotericsoftware.spine;

import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.Null;

import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.utils.SkeletonClipping;

/** Stores bones and slots to be posed by animations and application code. Multiple skeleton instances can share the same
 * {@link SkeletonData}, including animations, attachments, and skins.
 * <p>
 * After posing, call {@link #updateWorldTransform(Physics)} to apply constraints and compute world transforms for rendering.
 * <p>
 * See <a href="https://esotericsoftware.com/spine-runtime-architecture#Instance-objects">Instance objects</a> in the Spine
 * Runtimes Guide. */
public class Skeleton {
	static private final short[] quadTriangles = {0, 1, 2, 2, 3, 0};

	final SkeletonData data;
	final Array<Bone> bones;
	final Array<Slot> slots;
	final DrawOrder drawOrder;
	final Array<Constraint> constraints;
	final Array<PhysicsConstraint> physics;
	final Array updateCache = new Array();
	final Array<Posed> resetCache = new Array(true, 16, Posed[]::new);
	@Null Skin skin;
	final Color color;
	float x, y, scaleX = 1, scaleY = 1, time, windX = 1, windY = 0, gravityX = 0, gravityY = 1;
	int update;

	public Skeleton (SkeletonData data) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.data = data;

		bones = new Array(true, data.bones.size, Bone[]::new);
		Bone[] bones = this.bones.items;
		for (BoneData boneData : data.bones) {
			Bone bone;
			if (boneData.parent == null)
				bone = new Bone(boneData, null);
			else {
				Bone parent = bones[boneData.parent.index];
				bone = new Bone(boneData, parent);
				parent.children.add(bone);
			}
			this.bones.add(bone);
		}

		slots = new Array(true, data.slots.size, Slot[]::new);
		for (SlotData slotData : data.slots)
			slots.add(new Slot(slotData, this));
		drawOrder = new DrawOrder(slots);

		physics = new Array(true, 8, PhysicsConstraint[]::new);
		constraints = new Array(true, data.constraints.size, Constraint[]::new);
		for (ConstraintData constraintData : data.constraints) {
			Constraint constraint = constraintData.create(this);
			if (constraint instanceof PhysicsConstraint physicsConstraint) physics.add(physicsConstraint);
			constraints.add(constraint);
		}
		physics.shrink();

		color = new Color(1, 1, 1, 1);

		updateCache();
	}

	/** Copy constructor. */
	public Skeleton (Skeleton skeleton) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		data = skeleton.data;

		bones = new Array(true, skeleton.bones.size, Bone[]::new);
		for (Bone bone : skeleton.bones) {
			Bone newBone;
			if (bone.parent == null)
				newBone = new Bone(bone, null);
			else {
				Bone parent = bones.items[bone.parent.data.index];
				newBone = new Bone(bone, parent);
				parent.children.add(newBone);
			}
			bones.add(newBone);
		}

		slots = new Array(true, skeleton.slots.size, Slot[]::new);
		for (Slot slot : skeleton.slots)
			slots.add(new Slot(slot, bones.items[slot.bone.data.index], this));

		drawOrder = new DrawOrder(slots);
		drawOrder.pose.clear();
		for (Slot slot : skeleton.drawOrder.pose)
			drawOrder.pose.add(slots.items[slot.data.index]);

		physics = new Array(true, skeleton.physics.size, PhysicsConstraint[]::new);
		constraints = new Array(true, skeleton.constraints.size, Constraint[]::new);
		for (Constraint other : skeleton.constraints) {
			Constraint constraint = other.copy(this);
			if (constraint instanceof PhysicsConstraint physicsConstraint) physics.add(physicsConstraint);
			constraints.add(constraint);
		}

		skin = skeleton.skin;
		color = new Color(skeleton.color);
		x = skeleton.x;
		y = skeleton.y;
		scaleX = skeleton.scaleX;
		scaleY = skeleton.scaleY;
		time = skeleton.time;

		updateCache();
	}

	/** Caches information about bones and constraints. Must be called if the {@link #skin} is modified or if bones, constraints,
	 * or weighted path attachments are added or removed. */
	public void updateCache () {
		updateCache.clear();
		resetCache.clear();

		drawOrder.unconstrained();
		Slot[] slots = this.slots.items;
		for (int i = 0, n = this.slots.size; i < n; i++)
			slots[i].unconstrained();

		int boneCount = bones.size;
		Bone[] bones = this.bones.items;
		for (int i = 0; i < boneCount; i++) {
			Bone bone = bones[i];
			bone.sorted = bone.data.skinRequired;
			bone.active = !bone.sorted;
			bone.unconstrained();
		}
		if (skin != null) {
			BoneData[] skinBones = skin.bones.items;
			for (int i = 0, n = skin.bones.size; i < n; i++) {
				var bone = bones[skinBones[i].index];
				do {
					bone.sorted = false;
					bone.active = true;
					bone = bone.parent;
				} while (bone != null);
			}
		}

		Constraint[] constraints = this.constraints.items;
		int n = this.constraints.size;
		for (int i = 0; i < n; i++)
			constraints[i].unconstrained();
		for (int i = 0; i < n; i++) {
			Constraint<?, ?, ?> constraint = constraints[i];
			constraint.active = constraint.isSourceActive()
				&& (!constraint.data.skinRequired || (skin != null && skin.constraints.contains(constraint.data, true)));
			if (constraint.active) constraint.sort(this);
		}

		for (int i = 0; i < boneCount; i++)
			sortBone(bones[i]);

		Object[] updateCache = this.updateCache.items;
		n = this.updateCache.size;
		for (int i = 0; i < n; i++)
			if (updateCache[i] instanceof Bone bone) updateCache[i] = bone.appliedPose;
	}

	void constrained (Posed object) {
		if (object.pose == object.appliedPose) {
			object.constrained();
			resetCache.add(object);
		}
	}

	void sortBone (Bone bone) {
		if (bone.sorted || !bone.active) return;
		Bone parent = bone.parent;
		if (parent != null) sortBone(parent);
		bone.sorted = true;
		updateCache.add(bone);
	}

	void sortReset (Array<Bone> bones) {
		Bone[] items = bones.items;
		for (int i = 0, n = bones.size; i < n; i++) {
			Bone bone = items[i];
			if (bone.active) {
				if (bone.sorted) sortReset(bone.children);
				bone.sorted = false;
			}
		}
	}

	/** Updates the world transform for each bone and applies all constraints.
	 * <p>
	 * See <a href="https://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
	 * Runtimes Guide. */
	public void updateWorldTransform (Physics physics) {
		update++;

		if (drawOrder.appliedPose == drawOrder.constrainedPose) drawOrder.reset();
		Posed[] resetCache = this.resetCache.items;
		for (int i = 0, n = this.resetCache.size; i < n; i++)
			resetCache[i].reset();

		Object[] updateCache = this.updateCache.items;
		for (int i = 0, n = this.updateCache.size; i < n; i++)
			((Update)updateCache[i]).update(this, physics);
	}

	/** Temporarily sets the root bone as a child of the specified bone, then updates the world transform for each bone and applies
	 * all constraints.
	 * <p>
	 * See <a href="https://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
	 * Runtimes Guide. */
	public void updateWorldTransform (Physics physics, BonePose parent) { // Do not port.
		if (parent == null) throw new IllegalArgumentException("parent cannot be null.");

		update++;

		if (drawOrder.appliedPose == drawOrder.constrainedPose) drawOrder.reset();
		Posed[] resetCache = this.resetCache.items;
		for (int i = 0, n = this.resetCache.size; i < n; i++)
			resetCache[i].reset();

		// Apply the parent bone transform to the root bone. The root bone always inherits scale, rotation and reflection.
		BonePose rootBone = getRootBone().appliedPose;
		float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
		rootBone.worldX = pa * x + pb * y + parent.worldX;
		rootBone.worldY = pc * x + pd * y + parent.worldY;

		float rx = (rootBone.rotation + rootBone.shearX) * degRad;
		float ry = (rootBone.rotation + 90 + rootBone.shearY) * degRad;
		float la = cos(rx) * rootBone.scaleX;
		float lb = cos(ry) * rootBone.scaleY;
		float lc = sin(rx) * rootBone.scaleX;
		float ld = sin(ry) * rootBone.scaleY;
		rootBone.a = (pa * la + pb * lc) * scaleX;
		rootBone.b = (pa * lb + pb * ld) * scaleX;
		rootBone.c = (pc * la + pd * lc) * scaleY;
		rootBone.d = (pc * lb + pd * ld) * scaleY;

		// Update everything except root bone.
		Object[] updateCache = this.updateCache.items;
		for (int i = 0, n = this.updateCache.size; i < n; i++) {
			var updatable = (Update)updateCache[i];
			if (updatable != rootBone) updatable.update(this, physics);
		}
	}

	/** Sets the bones, constraints, slots, and draw order to their setup pose values. */
	public void setupPose () {
		setupPoseBones();
		setupPoseSlots();
	}

	/** Sets the bones and constraints to their setup pose values. */
	public void setupPoseBones () {
		Bone[] bones = this.bones.items;
		for (int i = 0, n = this.bones.size; i < n; i++)
			bones[i].setupPose();

		Constraint[] constraints = this.constraints.items;
		for (int i = 0, n = this.constraints.size; i < n; i++)
			constraints[i].setupPose();
	}

	/** Sets the slots and draw order to their setup pose values. */
	public void setupPoseSlots () {
		drawOrder.setupPose();
		Slot[] slots = this.slots.items;
		for (int i = 0, n = this.slots.size; i < n; i++)
			slots[i].setupPose();
	}

	/** The skeleton's setup pose data. */
	public SkeletonData getData () {
		return data;
	}

	/** The skeleton's bones, sorted parent first. The root bone is always the first bone. */
	public Array<Bone> getBones () {
		return bones;
	}

	/** The list of bones and constraints, sorted in the order they should be updated, as computed by {@link #updateCache()}. */
	public Array<Update> getUpdateCache () {
		return updateCache;
	}

	/** Returns the root bone, or null if the skeleton has no bones. */
	public Bone getRootBone () {
		return bones.size == 0 ? null : bones.first();
	}

	/** Finds a bone by comparing each bone's name. It is more efficient to cache the results of this method than to call it
	 * repeatedly. */
	public @Null Bone findBone (String boneName) {
		if (boneName == null) throw new IllegalArgumentException("boneName cannot be null.");
		Bone[] bones = this.bones.items;
		for (int i = 0, n = this.bones.size; i < n; i++)
			if (bones[i].data.name.equals(boneName)) return bones[i];
		return null;
	}

	/** The skeleton's slots in setup pose order. To change the order use {@link DrawOrder#getPose()}. For rendering use
	 * {@link DrawOrder#getAppliedPose()}. */
	public Array<Slot> getSlots () {
		return slots;
	}

	/** Finds a slot by comparing each slot's name. It is more efficient to cache the results of this method than to call it
	 * repeatedly. */
	public @Null Slot findSlot (String slotName) {
		if (slotName == null) throw new IllegalArgumentException("slotName cannot be null.");
		Slot[] slots = this.slots.items;
		for (int i = 0, n = this.slots.size; i < n; i++)
			if (slots[i].data.name.equals(slotName)) return slots[i];
		return null;
	}

	/** The skeleton's draw order. Use {@link DrawOrder#appliedPose} for rendering and {@link DrawOrder#pose} for changing the draw
	 * order. */
	public DrawOrder getDrawOrder () {
		return drawOrder;
	}

	/** The skeleton's current skin. */
	public @Null Skin getSkin () {
		return skin;
	}

	/** Sets a skin by name.
	 * <p>
	 * See {@link #setSkin(Skin)}. */
	public void setSkin (String skinName) {
		Skin skin = data.findSkin(skinName);
		if (skin == null) throw new IllegalArgumentException("Skin not found: " + skinName);
		setSkin(skin);
	}

	/** Sets the skin used to look up attachments before looking in {@link SkeletonData#defaultSkin}. If the skin is changed,
	 * {@link #updateCache()} is called.
	 * <p>
	 * Attachments from the new skin are attached if the corresponding attachment from the old skin was attached. If there was no
	 * old skin, each slot's setup mode attachment is attached from the new skin.
	 * <p>
	 * After changing the skin, the visible attachments can be reset to those attached in the setup pose by calling
	 * {@link #setupPoseSlots()}. Also, often {@link AnimationState#apply(Skeleton)} is called before the next time the skeleton is
	 * rendered to allow any attachment keys in the current animation(s) to hide or show attachments from the new skin. */
	public void setSkin (@Null Skin newSkin) {
		if (newSkin == skin) return;
		if (newSkin != null) {
			if (skin != null)
				newSkin.attachAll(this, skin);
			else {
				Slot[] slots = this.slots.items;
				for (int i = 0, n = this.slots.size; i < n; i++) {
					Slot slot = slots[i];
					String name = slot.data.attachmentName;
					if (name != null) {
						Attachment attachment = newSkin.getAttachment(i, name);
						if (attachment != null) slot.pose.setAttachment(attachment);
					}
				}
			}
		}
		skin = newSkin;
		updateCache();
	}

	/** Finds an attachment by looking in the {@link #skin} and {@link SkeletonData#defaultSkin} using the slot name and attachment
	 * name.
	 * <p>
	 * See {@link #getAttachment(int, String)}. */
	public @Null Attachment getAttachment (String slotName, String placeholder) {
		SlotData slot = data.findSlot(slotName);
		if (slot == null) throw new IllegalArgumentException("Slot not found: " + slotName);
		return getAttachment(slot.getIndex(), placeholder);
	}

	/** Finds an attachment by looking in the {@link #skin} and {@link SkeletonData#defaultSkin} using the slot index and skin
	 * placeholder name. First the skin is checked and if the attachment was not found, the default skin is checked.
	 * <p>
	 * See <a href="https://esotericsoftware.com/spine-runtime-skins">Runtime skins</a> in the Spine Runtimes Guide. */
	public @Null Attachment getAttachment (int slotIndex, String placeholder) {
		if (placeholder == null) throw new IllegalArgumentException("placeholder cannot be null.");
		if (skin != null) {
			Attachment attachment = skin.getAttachment(slotIndex, placeholder);
			if (attachment != null) return attachment;
		}
		if (data.defaultSkin != null) return data.defaultSkin.getAttachment(slotIndex, placeholder);
		return null;
	}

	/** A convenience method to set an attachment by finding the slot with {@link #findSlot(String)}, finding the attachment with
	 * {@link #getAttachment(int, String)}, then setting the slot's {@link SlotPose#attachment}.
	 * @param placeholder May be null to clear the slot's attachment. */
	public void setAttachment (String slotName, @Null String placeholder) {
		if (slotName == null) throw new IllegalArgumentException("slotName cannot be null.");
		Slot slot = findSlot(slotName);
		if (slot == null) throw new IllegalArgumentException("Slot not found: " + slotName);
		Attachment attachment = null;
		if (placeholder != null) {
			attachment = getAttachment(slot.data.index, placeholder);
			if (attachment == null)
				throw new IllegalArgumentException("Attachment not found: " + placeholder + ", for slot: " + slotName);
		}
		slot.pose.setAttachment(attachment);
	}

	/** The skeleton's constraints. */
	public Array<Constraint> getConstraints () {
		return constraints;
	}

	/** The skeleton's physics constraints. */
	public Array<PhysicsConstraint> getPhysicsConstraints () {
		return physics;
	}

	/** Finds a constraint of the specified type by comparing each constraints's name. It is more efficient to cache the results of
	 * this method than to call it multiple times. */
	public @Null <T extends Constraint> T findConstraint (String constraintName, Class<T> type) {
		if (constraintName == null) throw new IllegalArgumentException("constraintName cannot be null.");
		if (type == null) throw new IllegalArgumentException("type cannot be null.");
		Constraint[] constraints = this.constraints.items;
		for (int i = 0, n = this.constraints.size; i < n; i++) {
			Constraint constraint = constraints[i];
			if (type.isInstance(constraint) && constraint.data.name.equals(constraintName)) return (T)constraint;
		}
		return null;
	}

	/** Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the applied pose.
	 * @param offset An output value, the distance from the skeleton origin to the bottom left corner of the AABB.
	 * @param size An output value, the width and height of the AABB.
	 * @param temp Working memory to temporarily store attachments' computed world vertices. */
	public void getBounds (Vector2 offset, Vector2 size, FloatArray temp) {
		getBounds(offset, size, temp, null);
	}

	/** Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the applied pose. Optionally applies
	 * clipping.
	 * @param offset An output value, the distance from the skeleton origin to the bottom left corner of the AABB.
	 * @param size An output value, the width and height of the AABB.
	 * @param temp Working memory to temporarily store attachments' computed world vertices.
	 * @param clipper {@link SkeletonClipping} to use. If <code>null</code>, no clipping is applied. */
	public void getBounds (Vector2 offset, Vector2 size, FloatArray temp, SkeletonClipping clipper) {
		if (offset == null) throw new IllegalArgumentException("offset cannot be null.");
		if (size == null) throw new IllegalArgumentException("size cannot be null.");
		if (temp == null) throw new IllegalArgumentException("temp cannot be null.");
		Array<Slot> drawOrder = this.drawOrder.appliedPose;
		Slot[] slots = drawOrder.items;
		float minX = Integer.MAX_VALUE, minY = Integer.MAX_VALUE, maxX = Integer.MIN_VALUE, maxY = Integer.MIN_VALUE;
		for (int i = 0, n = drawOrder.size; i < n; i++) {
			Slot slot = slots[i];
			if (!slot.bone.active) continue;
			int verticesLength = 0;
			float[] vertices = null;
			short[] triangles = null;
			Attachment attachment = slot.appliedPose.attachment;
			if (attachment != null) {
				if (attachment instanceof RegionAttachment region) {
					verticesLength = 8;
					vertices = temp.setSize(8);
					region.computeWorldVertices(slot, region.getOffsets(slot.appliedPose), vertices, 0, 2);
					triangles = quadTriangles;
				} else if (attachment instanceof MeshAttachment mesh) {
					verticesLength = mesh.getWorldVerticesLength();
					vertices = temp.setSize(verticesLength);
					mesh.computeWorldVertices(this, slot, 0, verticesLength, vertices, 0, 2);
					triangles = mesh.getTriangles();
				} else if (attachment instanceof ClippingAttachment clip && clipper != null) {
					clipper.clipEnd(slot);
					clipper.clipStart(this, slot, clip);
					continue;
				}
				if (vertices != null) {
					if (clipper != null && clipper.isClipping() && clipper.clipTriangles(vertices, triangles, triangles.length)) {
						vertices = clipper.getClippedVertices().items;
						verticesLength = clipper.getClippedVertices().size;
					}
					for (int ii = 0; ii < verticesLength; ii += 2) {
						float x = vertices[ii], y = vertices[ii + 1];
						minX = Math.min(minX, x);
						minY = Math.min(minY, y);
						maxX = Math.max(maxX, x);
						maxY = Math.max(maxY, y);
					}
				}
			}
			if (clipper != null) clipper.clipEnd(slot);
		}
		if (clipper != null) clipper.clipEnd();
		offset.set(minX, minY);
		size.set(maxX - minX, maxY - minY);
	}

	/** The color to tint all the skeleton's attachments. */
	public Color getColor () {
		return color;
	}

	/** A convenience method for setting the skeleton color. The color can also be set by modifying {@link #color}. */
	public void setColor (Color color) {
		if (color == null) throw new IllegalArgumentException("color cannot be null.");
		this.color.set(color);
	}

	/** A convenience method for setting the skeleton color. The color can also be set by modifying {@link #color}. */
	public void setColor (float r, float g, float b, float a) {
		color.set(r, g, b, a);
	}

	/** Scales the entire skeleton on the X axis.
	 * <p>
	 * Bones that do not inherit scale are still affected by this property. */
	public float getScaleX () {
		return scaleX;
	}

	public void setScaleX (float scaleX) {
		this.scaleX = scaleX;
	}

	/** Scales the entire skeleton on the Y axis.
	 * <p>
	 * Bones that do not inherit scale are still affected by this property. */
	public float getScaleY () {
		return scaleY;
	}

	public void setScaleY (float scaleY) {
		this.scaleY = scaleY;
	}

	/** Scales the entire skeleton on the X and Y axes.
	 * <p>
	 * Bones that do not inherit scale are still affected by this property. */
	public void setScale (float scaleX, float scaleY) {
		this.scaleX = scaleX;
		this.scaleY = scaleY;
	}

	/** Sets the skeleton X position, which is added to the root bone worldX position.
	 * <p>
	 * Bones that do not inherit translation are still affected by this property. */
	public float getX () {
		return x;
	}

	public void setX (float x) {
		this.x = x;
	}

	/** Sets the skeleton Y position, which is added to the root bone worldY position.
	 * <p>
	 * Bones that do not inherit translation are still affected by this property. */
	public float getY () {
		return y;
	}

	public void setY (float y) {
		this.y = y;
	}

	/** Sets the skeleton X and Y position, which is added to the root bone worldX and worldY position.
	 * <p>
	 * Bones that do not inherit translation are still affected by this property. */
	public void setPosition (float x, float y) {
		this.x = x;
		this.y = y;
	}

	/** The x component of a vector that defines the direction {@link PhysicsConstraintPose#wind} is applied. */
	public float getWindX () {
		return windX;
	}

	public void setWindX (float windX) {
		this.windX = windX;
	}

	/** The y component of a vector that defines the direction {@link PhysicsConstraintPose#wind} is applied. */
	public float getWindY () {
		return windY;
	}

	public void setWindY (float windY) {
		this.windY = windY;
	}

	/** The x component of a vector that defines the direction {@link PhysicsConstraintPose#gravity} is applied. */
	public float getGravityX () {
		return gravityX;
	}

	public void setGravityX (float gravityX) {
		this.gravityX = gravityX;
	}

	/** The y component of a vector that defines the direction {@link PhysicsConstraintPose#gravity} is applied. */
	public float getGravityY () {
		return gravityY;
	}

	public void setGravityY (float gravityY) {
		this.gravityY = gravityY;
	}

	/** Calls {@link PhysicsConstraint#translate(float, float)} for each physics constraint. */
	public void physicsTranslate (float x, float y) {
		PhysicsConstraint[] constraints = this.physics.items;
		for (int i = 0, n = this.physics.size; i < n; i++)
			constraints[i].translate(x, y);
	}

	/** Calls {@link PhysicsConstraint#rotate(float, float, float)} for each physics constraint. */
	public void physicsRotate (float x, float y, float degrees) {
		PhysicsConstraint[] constraints = this.physics.items;
		for (int i = 0, n = this.physics.size; i < n; i++)
			constraints[i].rotate(x, y, degrees);
	}

	/** Returns the skeleton's time, used for time-based manipulations, such as {@link PhysicsConstraint}.
	 * <p>
	 * See {@link #update(float)}. */
	public float getTime () {
		return time;
	}

	public void setTime (float time) {
		this.time = time;
	}

	/** Increments the skeleton's {@link #time}. */
	public void update (float delta) {
		time += delta;
	}

	public String toString () {
		return data.name != null ? data.name : super.toString();
	}
}
