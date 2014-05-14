/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine {
import spine.attachments.Attachment;

/** Stores attachments by slot index and attachment name. */
public class Skin {
	internal var _name:String;
	private var attachments:Object = new Object();

	public function Skin (name:String) {
		if (name == null)
			throw new ArgumentError("name cannot be null.");
		_name = name;
	}

	public function addAttachment (slotIndex:int, name:String, attachment:Attachment) : void {
		if (attachment == null)
			throw new ArgumentError("attachment cannot be null.");
		attachments[slotIndex + ":" + name] = attachment;
	}

	/** @return May be null. */
	public function getAttachment (slotIndex:int, name:String) : Attachment {
		return attachments[slotIndex + ":" + name];
	}

	public function get name () : String {
		return _name;
	}

	public function toString () : String {
		return _name;
	}

	/** Attach each attachment in this skin if the corresponding attachment in the old skin is currently attached. */
	public function attachAll (skeleton:Skeleton, oldSkin:Skin) : void {
		for (var key:String in oldSkin.attachments) {
			var colon:int = key.indexOf(":");
			var slotIndex:int = parseInt(key.substring(0, colon));
			var name:String = key.substring(colon + 1);
			var slot:Slot = skeleton.slots[slotIndex];
			if (slot.attachment && slot.attachment.name == name) {
				var attachment:Attachment = getAttachment(slotIndex, name);
				if (attachment != null)
					slot.attachment = attachment;
			}
		}
	}
}

}
