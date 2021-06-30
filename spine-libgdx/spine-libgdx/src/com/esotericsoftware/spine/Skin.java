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
import com.badlogic.gdx.utils.OrderedSet;

import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;

/** Stores attachments by slot index and attachment name.
 * <p>
 * See SkeletonData {@link SkeletonData#defaultSkin}, Skeleton {@link Skeleton#skin}, and
 * <a href="http://esotericsoftware.com/spine-runtime-skins">Runtime skins</a> in the Spine Runtimes Guide. */
public class Skin {
	final String name;
	final OrderedSet<SkinEntry> attachments = new OrderedSet();
	final Array<BoneData> bones = new Array(0);
	final Array<ConstraintData> constraints = new Array(0);
	private final SkinEntry lookup = new SkinEntry(0, "", null);

	public Skin (String name) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
		attachments.orderedItems().ordered = false;
	}

	/** Adds an attachment to the skin for the specified slot index and name. */
	public void setAttachment (int slotIndex, String name, Attachment attachment) {
		if (attachment == null) throw new IllegalArgumentException("attachment cannot be null.");
		SkinEntry entry = new SkinEntry(slotIndex, name, attachment);
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
			setAttachment(entry.slotIndex, entry.name, entry.attachment);
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
			if (entry.attachment instanceof MeshAttachment)
				setAttachment(entry.slotIndex, entry.name, ((MeshAttachment)entry.attachment).newLinkedMesh());
			else
				setAttachment(entry.slotIndex, entry.name, entry.attachment != null ? entry.attachment.copy() : null);
		}
	}

	/** Returns the attachment for the specified slot index and name, or null. */
	public @Null Attachment getAttachment (int slotIndex, String name) {
		lookup.set(slotIndex, name);
		SkinEntry entry = attachments.get(lookup);
		return entry != null ? entry.attachment : null;
	}

	/** Removes the attachment in the skin for the specified slot index and name, if any. */
	public void removeAttachment (int slotIndex, String name) {
		lookup.set(slotIndex, name);
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

	/** The skin's name, which is unique across all skins in the skeleton. */
	public String getName () {
		return name;
	}

	public String toString () {
		return name;
	}

	/** Attach each attachment in this skin if the corresponding attachment in the old skin is currently attached. */
	void attachAll (Skeleton skeleton, Skin oldSkin) {
		Object[] slots = skeleton.slots.items;
		for (SkinEntry entry : oldSkin.attachments.orderedItems()) {
			int slotIndex = entry.slotIndex;
			Slot slot = (Slot)slots[slotIndex];
			if (slot.attachment == entry.attachment) {
				Attachment attachment = getAttachment(slotIndex, entry.name);
				if (attachment != null) slot.setAttachment(attachment);
			}
		}
	}

	/** Stores an entry in the skin consisting of the slot index and the attachment name. */
	static public class SkinEntry {
		int slotIndex;
		String name;
		@Null Attachment attachment;
		private int hashCode;

		SkinEntry (int slotIndex, String name, @Null Attachment attachment) {
			set(slotIndex, name);
			this.attachment = attachment;
		}

		void set (int slotIndex, String name) {
			if (slotIndex < 0) throw new IllegalArgumentException("slotIndex must be >= 0.");
			if (name == null) throw new IllegalArgumentException("name cannot be null.");
			this.slotIndex = slotIndex;
			this.name = name;
			hashCode = name.hashCode() + slotIndex * 37;
		}

		public int getSlotIndex () {
			return slotIndex;
		}

		/** The name the attachment is associated with, equivalent to the skin placeholder name in the Spine editor. */
		public String getName () {
			return name;
		}

		public Attachment getAttachment () {
			return attachment;
		}

		public int hashCode () {
			return hashCode;
		}

		public boolean equals (Object object) {
			if (object == null) return false;
			SkinEntry other = (SkinEntry)object;
			if (slotIndex != other.slotIndex) return false;
			return name.equals(other.name);
		}

		public String toString () {
			return slotIndex + ":" + name;
		}
	}
}
