/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.ObjectMap;
import com.badlogic.gdx.utils.ObjectMap.Entry;
import com.badlogic.gdx.utils.Pool;
import com.esotericsoftware.spine.attachments.Attachment;

/** Stores attachments by slot index and attachment name.
 * <p>
 * See SkeletonData {@link SkeletonData#defaultSkin}, Skeleton {@link Skeleton#skin}, and
 * <a href="http://esotericsoftware.com/spine-runtime-skins">Runtime skins</a> in the Spine Runtimes Guide. */
public class Skin {
	final String name;
	final ObjectMap<Key, Attachment> attachments = new ObjectMap();
	private final Key lookup = new Key();
	final Pool<Key> keyPool = new Pool(64) {
		protected Object newObject () {
			return new Key();
		}
	};

	public Skin (String name) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
	}

	/** Adds an attachment to the skin for the specified slot index and name. */
	public void addAttachment (int slotIndex, String name, Attachment attachment) {
		if (attachment == null) throw new IllegalArgumentException("attachment cannot be null.");
		if (slotIndex < 0) throw new IllegalArgumentException("slotIndex must be >= 0.");
		Key key = keyPool.obtain();
		key.set(slotIndex, name);
		attachments.put(key, attachment);
	}

	/** Adds all attachments from the specified skin to this skin. */
	public void addAttachments (Skin skin) {
		for (Entry<Key, Attachment> entry : skin.attachments.entries())
			addAttachment(entry.key.slotIndex, entry.key.name, entry.value);
	}

	/** Returns the attachment for the specified slot index and name, or null. */
	public Attachment getAttachment (int slotIndex, String name) {
		if (slotIndex < 0) throw new IllegalArgumentException("slotIndex must be >= 0.");
		lookup.set(slotIndex, name);
		return attachments.get(lookup);
	}

	public void findNamesForSlot (int slotIndex, Array<String> names) {
		if (names == null) throw new IllegalArgumentException("names cannot be null.");
		if (slotIndex < 0) throw new IllegalArgumentException("slotIndex must be >= 0.");
		for (Key key : attachments.keys())
			if (key.slotIndex == slotIndex) names.add(key.name);
	}

	public void findAttachmentsForSlot (int slotIndex, Array<Attachment> attachments) {
		if (attachments == null) throw new IllegalArgumentException("attachments cannot be null.");
		if (slotIndex < 0) throw new IllegalArgumentException("slotIndex must be >= 0.");
		for (Entry<Key, Attachment> entry : this.attachments.entries())
			if (entry.key.slotIndex == slotIndex) attachments.add(entry.value);
	}

	public void clear () {
		for (Key key : attachments.keys())
			keyPool.free(key);
		attachments.clear();
	}

	/** The skin's name, which is unique within the skeleton. */
	public String getName () {
		return name;
	}

	public String toString () {
		return name;
	}

	/** Attach each attachment in this skin if the corresponding attachment in the old skin is currently attached. */
	void attachAll (Skeleton skeleton, Skin oldSkin) {
		for (Entry<Key, Attachment> entry : oldSkin.attachments.entries()) {
			int slotIndex = entry.key.slotIndex;
			Slot slot = skeleton.slots.get(slotIndex);
			if (slot.attachment == entry.value) {
				Attachment attachment = getAttachment(slotIndex, entry.key.name);
				if (attachment != null) slot.setAttachment(attachment);
			}
		}
	}

	static class Key {
		int slotIndex;
		String name;
		int hashCode;

		public void set (int slotIndex, String name) {
			if (name == null) throw new IllegalArgumentException("name cannot be null.");
			this.slotIndex = slotIndex;
			this.name = name;
			hashCode = 31 * (31 + name.hashCode()) + slotIndex;
		}

		public int hashCode () {
			return hashCode;
		}

		public boolean equals (Object object) {
			if (object == null) return false;
			Key other = (Key)object;
			if (slotIndex != other.slotIndex) return false;
			if (!name.equals(other.name)) return false;
			return true;
		}

		public String toString () {
			return slotIndex + ":" + name;
		}
	}
}
