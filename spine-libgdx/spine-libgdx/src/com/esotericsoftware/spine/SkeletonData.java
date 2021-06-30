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

package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Null;

/** Stores the setup pose and all of the stateless data for a skeleton.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-runtime-architecture#Data-objects">Data objects</a> in the Spine Runtimes
 * Guide. */
public class SkeletonData {
	@Null String name;
	final Array<BoneData> bones = new Array(); // Ordered parents first.
	final Array<SlotData> slots = new Array(); // Setup pose draw order.
	final Array<Skin> skins = new Array();
	@Null Skin defaultSkin;
	final Array<EventData> events = new Array();
	final Array<Animation> animations = new Array();
	final Array<IkConstraintData> ikConstraints = new Array();
	final Array<TransformConstraintData> transformConstraints = new Array();
	final Array<PathConstraintData> pathConstraints = new Array();
	float x, y, width, height;
	@Null String version, hash;

	// Nonessential.
	float fps = 30;
	@Null String imagesPath, audioPath;

	// --- Bones.

	public SkeletonData () {
		super();
	}

	/** The skeleton's bones, sorted parent first. The root bone is always the first bone. */
	public Array<BoneData> getBones () {
		return bones;
	}

	/** Finds a bone by comparing each bone's name. It is more efficient to cache the results of this method than to call it
	 * multiple times. */
	public @Null BoneData findBone (String boneName) {
		if (boneName == null) throw new IllegalArgumentException("boneName cannot be null.");
		Object[] bones = this.bones.items;
		for (int i = 0, n = this.bones.size; i < n; i++) {
			BoneData bone = (BoneData)bones[i];
			if (bone.name.equals(boneName)) return bone;
		}
		return null;
	}

	// --- Slots.

	/** The skeleton's slots. */
	public Array<SlotData> getSlots () {
		return slots;
	}

	/** Finds a slot by comparing each slot's name. It is more efficient to cache the results of this method than to call it
	 * multiple times. */
	public @Null SlotData findSlot (String slotName) {
		if (slotName == null) throw new IllegalArgumentException("slotName cannot be null.");
		Object[] slots = this.slots.items;
		for (int i = 0, n = this.slots.size; i < n; i++) {
			SlotData slot = (SlotData)slots[i];
			if (slot.name.equals(slotName)) return slot;
		}
		return null;
	}

	// --- Skins.

	/** The skeleton's default skin. By default this skin contains all attachments that were not in a skin in Spine.
	 * <p>
	 * See {@link Skeleton#getAttachment(int, String)}. */
	public @Null Skin getDefaultSkin () {
		return defaultSkin;
	}

	public void setDefaultSkin (@Null Skin defaultSkin) {
		this.defaultSkin = defaultSkin;
	}

	/** Finds a skin by comparing each skin's name. It is more efficient to cache the results of this method than to call it
	 * multiple times. */
	public @Null Skin findSkin (String skinName) {
		if (skinName == null) throw new IllegalArgumentException("skinName cannot be null.");
		for (Skin skin : skins)
			if (skin.name.equals(skinName)) return skin;
		return null;
	}

	/** All skins, including the default skin. */
	public Array<Skin> getSkins () {
		return skins;
	}

	// --- Events.

	/** Finds an event by comparing each events's name. It is more efficient to cache the results of this method than to call it
	 * multiple times. */
	public @Null EventData findEvent (String eventDataName) {
		if (eventDataName == null) throw new IllegalArgumentException("eventDataName cannot be null.");
		for (EventData eventData : events)
			if (eventData.name.equals(eventDataName)) return eventData;
		return null;
	}

	/** The skeleton's events. */
	public Array<EventData> getEvents () {
		return events;
	}

	// --- Animations.

	/** The skeleton's animations. */
	public Array<Animation> getAnimations () {
		return animations;
	}

	/** Finds an animation by comparing each animation's name. It is more efficient to cache the results of this method than to
	 * call it multiple times. */
	public @Null Animation findAnimation (String animationName) {
		if (animationName == null) throw new IllegalArgumentException("animationName cannot be null.");
		Object[] animations = this.animations.items;
		for (int i = 0, n = this.animations.size; i < n; i++) {
			Animation animation = (Animation)animations[i];
			if (animation.name.equals(animationName)) return animation;
		}
		return null;
	}

	// --- IK constraints

	/** The skeleton's IK constraints. */
	public Array<IkConstraintData> getIkConstraints () {
		return ikConstraints;
	}

	/** Finds an IK constraint by comparing each IK constraint's name. It is more efficient to cache the results of this method
	 * than to call it multiple times. */
	public @Null IkConstraintData findIkConstraint (String constraintName) {
		if (constraintName == null) throw new IllegalArgumentException("constraintName cannot be null.");
		Object[] ikConstraints = this.ikConstraints.items;
		for (int i = 0, n = this.ikConstraints.size; i < n; i++) {
			IkConstraintData constraint = (IkConstraintData)ikConstraints[i];
			if (constraint.name.equals(constraintName)) return constraint;
		}
		return null;
	}

	// --- Transform constraints

	/** The skeleton's transform constraints. */
	public Array<TransformConstraintData> getTransformConstraints () {
		return transformConstraints;
	}

	/** Finds a transform constraint by comparing each transform constraint's name. It is more efficient to cache the results of
	 * this method than to call it multiple times. */
	public @Null TransformConstraintData findTransformConstraint (String constraintName) {
		if (constraintName == null) throw new IllegalArgumentException("constraintName cannot be null.");
		Object[] transformConstraints = this.transformConstraints.items;
		for (int i = 0, n = this.transformConstraints.size; i < n; i++) {
			TransformConstraintData constraint = (TransformConstraintData)transformConstraints[i];
			if (constraint.name.equals(constraintName)) return constraint;
		}
		return null;
	}

	// --- Path constraints

	/** The skeleton's path constraints. */
	public Array<PathConstraintData> getPathConstraints () {
		return pathConstraints;
	}

	/** Finds a path constraint by comparing each path constraint's name. It is more efficient to cache the results of this method
	 * than to call it multiple times. */
	public @Null PathConstraintData findPathConstraint (String constraintName) {
		if (constraintName == null) throw new IllegalArgumentException("constraintName cannot be null.");
		Object[] pathConstraints = this.pathConstraints.items;
		for (int i = 0, n = this.pathConstraints.size; i < n; i++) {
			PathConstraintData constraint = (PathConstraintData)pathConstraints[i];
			if (constraint.name.equals(constraintName)) return constraint;
		}
		return null;
	}

	// ---

	/** The skeleton's name, which by default is the name of the skeleton data file when possible, or null when a name hasn't been
	 * set. */
	public @Null String getName () {
		return name;
	}

	public void setName (@Null String name) {
		this.name = name;
	}

	/** The X coordinate of the skeleton's axis aligned bounding box in the setup pose. */
	public float getX () {
		return x;
	}

	public void setX (float x) {
		this.x = x;
	}

	/** The Y coordinate of the skeleton's axis aligned bounding box in the setup pose. */
	public float getY () {
		return y;
	}

	public void setY (float y) {
		this.y = y;
	}

	/** The width of the skeleton's axis aligned bounding box in the setup pose. */
	public float getWidth () {
		return width;
	}

	public void setWidth (float width) {
		this.width = width;
	}

	/** The height of the skeleton's axis aligned bounding box in the setup pose. */
	public float getHeight () {
		return height;
	}

	public void setHeight (float height) {
		this.height = height;
	}

	/** The Spine version used to export the skeleton data, or null. */
	public @Null String getVersion () {
		return version;
	}

	public void setVersion (@Null String version) {
		this.version = version;
	}

	/** The skeleton data hash. This value will change if any of the skeleton data has changed. */
	public @Null String getHash () {
		return hash;
	}

	public void setHash (@Null String hash) {
		this.hash = hash;
	}

	/** The path to the images directory as defined in Spine, or null if nonessential data was not exported. */
	public @Null String getImagesPath () {
		return imagesPath;
	}

	public void setImagesPath (@Null String imagesPath) {
		this.imagesPath = imagesPath;
	}

	/** The path to the audio directory as defined in Spine, or null if nonessential data was not exported. */
	public @Null String getAudioPath () {
		return audioPath;
	}

	public void setAudioPath (@Null String audioPath) {
		this.audioPath = audioPath;
	}

	/** The dopesheet FPS in Spine, or zero if nonessential data was not exported. */
	public float getFps () {
		return fps;
	}

	public void setFps (float fps) {
		this.fps = fps;
	}

	public String toString () {
		return name != null ? name : super.toString();
	}
}
