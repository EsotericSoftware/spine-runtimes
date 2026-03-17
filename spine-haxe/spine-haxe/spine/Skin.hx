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

package spine;

import haxe.ds.StringMap;
import spine.attachments.Attachment;
import spine.attachments.MeshAttachment;

/** Stores attachments by slot index and attachment name.
 *
 * See spine.SkeletonData.defaultSkin, spine.Skeleton.skin, and
 * Runtime skins at https://esotericsoftware.com/spine-runtime-skins in the Spine Runtimes Guide. */
class Skin {
	/** The skin's name, which is unique across all skins in the skeleton. */
	public final name:String;

	/** Returns the attachment for the specified slot index and name, or null. */
	public final attachments:Array<StringMap<Attachment>> = new Array<StringMap<Attachment>>();

	public final bones:Array<BoneData> = new Array<BoneData>();
	public final constraints = new Array<ConstraintData<Dynamic, Dynamic>>();

	/** The color of the skin as it was in Spine, or a default color if nonessential data was not exported. */
	public final color:Color = new Color(0.99607843, 0.61960787, 0.30980393, 1); // fe9e4fff

	public function new(name:String) {
		if (name == null)
			throw new SpineException("name cannot be null.");
		this.name = name;
	}

	/** Adds an attachment to the skin for the specified slot index and name. */
	public function setAttachment(slotIndex:Int, name:String, attachment:Attachment):Void {
		if (attachment == null)
			throw new SpineException("attachment cannot be null.");
		if (slotIndex >= attachments.length)
			attachments.resize(slotIndex + 1);
		if (attachments[slotIndex] == null)
			attachments[slotIndex] = new StringMap<Attachment>();
		attachments[slotIndex].set(name, attachment);
	}

	/** Adds all attachments, bones, and constraints from the specified skin to this skin. */
	public function addSkin(skin:Skin):Void {
		var contained:Bool = false;
		for (i in 0...skin.bones.length) {
			var bone:BoneData = skin.bones[i];
			contained = false;
			for (j in 0...bones.length) {
				if (bones[j] == bone) {
					contained = true;
					break;
				}
			}
			if (!contained)
				bones.push(bone);
		}

		for (i in 0...skin.constraints.length) {
			var constraint = skin.constraints[i];
			contained = false;
			for (j in 0...constraints.length) {
				if (constraints[j] == constraint) {
					contained = true;
					break;
				}
			}
			if (!contained)
				constraints.push(constraint);
		}

		var attachments:Array<SkinEntry> = skin.getAttachments();
		for (i in 0...attachments.length) {
			var attachment:SkinEntry = attachments[i];
			setAttachment(attachment.slotIndex, attachment.name, attachment.attachment);
		}
	}

	/** Adds all bones and constraints and copies of all attachments from the specified skin to this skin. Mesh attachments are not
	 * copied, instead a new linked mesh is created. The attachment copies can be modified without affecting the originals. */
	public function copySkin(skin:Skin):Void {
		var contained:Bool = false;
		var attachment:SkinEntry;

		for (i in 0...skin.bones.length) {
			var bone:BoneData = skin.bones[i];
			contained = false;
			for (j in 0...bones.length) {
				if (bones[j] == bone) {
					contained = true;
					break;
				}
			}
			if (!contained)
				bones.push(bone);
		}

		for (i in 0...skin.constraints.length) {
			var constraint = skin.constraints[i];
			contained = false;
			for (j in 0...constraints.length) {
				if (constraints[j] == constraint) {
					contained = true;
					break;
				}
			}
			if (!contained)
				constraints.push(constraint);
		}

		var attachments:Array<SkinEntry> = skin.getAttachments();
		for (i in 0...attachments.length) {
			attachment = attachments[i];
			if (attachment.attachment == null)
				continue;
			if (Std.isOfType(attachment.attachment, MeshAttachment)) {
				var mesh = cast(attachment.attachment, MeshAttachment);
				attachment.attachment = mesh.newLinkedMesh();
				setAttachment(attachment.slotIndex, attachment.name, attachment.attachment);
			} else {
				attachment.attachment = attachment.attachment.copy();
				setAttachment(attachment.slotIndex, attachment.name, attachment.attachment);
			}
		}
	}

	/** Returns the attachment for the specified slot index and name, or null. */
	public function getAttachment(slotIndex:Int, name:String):Attachment {
		if (slotIndex >= attachments.length)
			return null;
		var dictionary:StringMap<Attachment> = attachments[slotIndex];
		return dictionary != null ? dictionary.get(name) : null;
	}

	/** Removes the attachment in the skin for the specified slot index and name, if any. */
	public function removeAttachment(slotIndex:Int, name:String):Void {
		var dictionary:StringMap<Attachment> = attachments[slotIndex];
		if (dictionary != null)
			dictionary.remove(name);
	}

	/** Returns all attachments in this skin. */
	public function getAttachments():Array<SkinEntry> {
		var entries:Array<SkinEntry> = new Array<SkinEntry>();
		for (slotIndex in 0...attachments.length) {
			var attachments:StringMap<Attachment> = attachments[slotIndex];
			if (attachments != null) {
				for (name in attachments.keys()) {
					var attachment:Attachment = attachments.get(name);
					if (attachment != null)
						entries.push(new SkinEntry(slotIndex, name, attachment));
				}
			}
		}
		return entries;
	}

	/** Returns all attachments in this skin for the specified slot index. */
	public function getAttachmentsForSlot(slotIndex:Int):Array<SkinEntry> {
		var entries:Array<SkinEntry> = new Array<SkinEntry>();
		var attachments:StringMap<Attachment> = attachments[slotIndex];
		if (attachments != null) {
			for (name in attachments.keys()) {
				var attachment:Attachment = attachments.get(name);
				if (attachment != null)
					entries.push(new SkinEntry(slotIndex, name, attachment));
			}
		}
		return entries;
	}

	/** Clears all attachments, bones, and constraints. */
	public function clear():Void {
		attachments.resize(0);
		bones.resize(0);
		constraints.resize(0);
	}

	public function toString():String {
		return name;
	}

	/** Attach each attachment in this skin if the corresponding attachment in the old skin is currently attached. */
	public function attachAll(skeleton:Skeleton, oldSkin:Skin):Void {
		var slotIndex:Int = 0;
		for (element in skeleton.slots) {
			var slot = element.pose;
			var slotAttachment = slot.attachment;
			if (slotAttachment != null && slotIndex < oldSkin.attachments.length) {
				var dictionary:StringMap<Attachment> = oldSkin.attachments[slotIndex];
				if (null != dictionary) {
					for (name in dictionary.keys()) {
						var skinAttachment:Attachment = dictionary.get(name);
						if (slotAttachment == skinAttachment) {
							var attachment:Attachment = getAttachment(slotIndex, name);
							if (attachment != null)
								slot.attachment = attachment;
							break;
						}
					}
				}
			}
			slotIndex++;
		}
	}
}
