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

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Null;
import com.badlogic.gdx.utils.OrderedSet;

import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;

/** Stores attachments by slot index and placeholder name. Multiple {@link Skeleton} instances can use the same skins.
 * <p>
 * See {@link SkeletonData#defaultSkin}, {@link Skeleton#skin}, and
 * <a href="https://esotericsoftware.com/spine-runtime-skins">Runtime skins</a> in the Spine Runtimes Guide. */
public class Skin {
	final String name;
	final OrderedSet<SkinEntry> attachments = new OrderedSet();
	final Array<BoneData> bones = new Array(true, 0, BoneData[]::new);
	final Array<ConstraintData> constraints = new Array(true, 0, ConstraintData[]::new);
	private final SkinEntry lookup = new SkinEntry(0, "", null);

	// Nonessential.
	final Color color = new Color(0.99607843f, 0.61960787f, 0.30980393f, 1); // fe9e4fff

	public Skin (String name) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
		attachments.orderedItems().ordered = false;
	}

	/** Adds an attachment to the skin for the specified slot index and placeholder name. */
	public void setAttachment (int slotIndex, String placeholder, Attachment attachment) {
		if (attachment == null) throw new IllegalArgumentException("attachment cannot be null.");
		var entry = new SkinEntry(slotIndex, placeholder, attachment);
		if (!attachments.add(entry)) attachments.get(entry).attachment = attachment;
	}

	/** Adds all attachments, bones, and constraints from the specified skin to this skin. */
	public void addSkin (Skin skin) {
		if (skin == null) throw new IllegalArgumentException("skin cannot be null.");

		for (BoneData data : skin.bones)
			if (!bones.contains(data, true)) bones.add(data);

		for (ConstraintData data : skin.constraints)
			if (!constraints.contains(data, true)) constraints.add(data);

		for (SkinEntry entry : skin.attachments.orderedItems())
			setAttachment(entry.slotIndex, entry.placeholder, entry.attachment);
	}

	/** Adds all bones and constraints and copies of all attachments from the specified skin to this skin. Mesh attachments are not
	 * copied, instead a new linked mesh is created. The attachment copies can be modified without affecting the originals. */
	public void copySkin (Skin skin) {
		if (skin == null) throw new IllegalArgumentException("skin cannot be null.");

		for (BoneData data : skin.bones)
			if (!bones.contains(data, true)) bones.add(data);

		for (ConstraintData data : skin.constraints)
			if (!constraints.contains(data, true)) constraints.add(data);

		for (SkinEntry entry : skin.attachments.orderedItems()) {
			if (entry.attachment instanceof MeshAttachment mesh)
				setAttachment(entry.slotIndex, entry.placeholder, mesh.newLinkedMesh());
			else
				setAttachment(entry.slotIndex, entry.placeholder, entry.attachment != null ? entry.attachment.copy() : null);
		}
	}

	/** Returns the attachment for the specified slot index and placeholder name, or null. */
	public @Null Attachment getAttachment (int slotIndex, String placeholder) {
		lookup.set(slotIndex, placeholder);
		SkinEntry entry = attachments.get(lookup);
		return entry != null ? entry.attachment : null;
	}

	/** Removes the attachment in the skin for the specified slot index and placeholder name, if any. */
	public void removeAttachment (int slotIndex, String placeholder) {
		lookup.set(slotIndex, placeholder);
		attachments.remove(lookup);
	}

	/** Returns all attachments in this skin. */
	public Array<SkinEntry> getAttachments () {
		return attachments.orderedItems();
	}

	/** Returns all attachments in this skin for the specified slot index. */
	public void getAttachments (int slotIndex, Array<SkinEntry> attachments) {
		if (slotIndex < 0) throw new IllegalArgumentException("slotIndex must be >= 0.");
		if (attachments == null) throw new IllegalArgumentException("attachments cannot be null.");
		for (SkinEntry entry : this.attachments.orderedItems())
			if (entry.slotIndex == slotIndex) attachments.add(entry);
	}

	/** Clears all attachments, bones, and constraints. */
	public void clear () {
		attachments.clear(1024);
		bones.clear();
		constraints.clear();
	}

	public Array<BoneData> getBones () {
		return bones;
	}

	public Array<ConstraintData> getConstraints () {
		return constraints;
	}

	/** The skin's name, unique across all skins in the skeleton.
	 * <p>
	 * See {@link SkeletonData#findSkin(String)}. */
	public String getName () {
		return name;
	}

	/** The color of the skin as it was in Spine, or a default color if nonessential data was not exported. */
	public Color getColor () {
		return color;
	}

	public String toString () {
		return name;
	}

	/** Attach each attachment in this skin if the corresponding attachment in the old skin is currently attached. */
	void attachAll (Skeleton skeleton, Skin oldSkin) {
		Slot[] slots = skeleton.slots.items;
		for (SkinEntry entry : oldSkin.attachments.orderedItems()) {
			SlotPose slot = slots[entry.slotIndex].pose;
			if (slot.attachment == entry.attachment) {
				Attachment attachment = getAttachment(entry.slotIndex, entry.placeholder);
				if (attachment != null) slot.setAttachment(attachment);
			}
		}
	}

	/** Stores an entry in the skin consisting of the slot index and placeholder name. */
	static public class SkinEntry {
		int slotIndex;
		String placeholder;
		@Null Attachment attachment;
		private int hashCode;

		SkinEntry (int slotIndex, String placeholder, @Null Attachment attachment) {
			set(slotIndex, placeholder);
			this.attachment = attachment;
		}

		void set (int slotIndex, String placeholder) {
			if (slotIndex < 0) throw new IllegalArgumentException("slotIndex must be >= 0.");
			if (placeholder == null) throw new IllegalArgumentException("placeholder cannot be null.");
			this.slotIndex = slotIndex;
			this.placeholder = placeholder;
			hashCode = placeholder.hashCode() + slotIndex * 37;
		}

		/** The {@link Skeleton#slots} index. */
		public int getSlotIndex () {
			return slotIndex;
		}

		/** The placeholder name that the attachment is associated with. */
		public String getPlaceholder () {
			return placeholder;
		}

		/** The attachment for this skin entry. */
		public Attachment getAttachment () {
			return attachment;
		}

		public int hashCode () {
			return hashCode;
		}

		public boolean equals (Object object) {
			if (object == null) return false;
			var other = (SkinEntry)object;
			if (slotIndex != other.slotIndex) return false;
			return placeholder.equals(other.placeholder);
		}

		public String toString () {
			return slotIndex + ":" + placeholder;
		}
	}
}
