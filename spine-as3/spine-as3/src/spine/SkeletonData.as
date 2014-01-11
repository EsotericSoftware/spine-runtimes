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
 * any copy thereof. Redistributions in binary or source code must include
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

package spine {
import spine.animation.Animation;

public class SkeletonData {
	public var name:String;
	public var bones:Vector.<BoneData> = new Vector.<BoneData>(); // Ordered parents first.
	public var slots:Vector.<SlotData> = new Vector.<SlotData>(); // Setup pose draw order.
	public var skins:Vector.<Skin> = new Vector.<Skin>();
	public var defaultSkin:Skin;
	public var events:Vector.<EventData> = new Vector.<EventData>();
	public var animations:Vector.<Animation> = new Vector.<Animation>();

	// --- Bones.

	public function addBone (bone:BoneData) : void {
		if (bone == null)
			throw new ArgumentError("bone cannot be null.");
		bones[bones.length] = bone;
	}

	/** @return May be null. */
	public function findBone (boneName:String) : BoneData {
		if (boneName == null)
			throw new ArgumentError("boneName cannot be null.");
		for (var i:int = 0, n:int = bones.length; i < n; i++) {
			var bone:BoneData = bones[i];
			if (bone._name == boneName)
				return bone;
		}
		return null;
	}

	/** @return -1 if the bone was not found. */
	public function findBoneIndex (boneName:String) : int {
		if (boneName == null)
			throw new ArgumentError("boneName cannot be null.");
		for (var i:int = 0, n:int = bones.length; i < n; i++)
			if (bones[i]._name == boneName)
				return i;
		return -1;
	}

	// --- Slots.

	public function addSlot (slot:SlotData) : void {
		if (slot == null)
			throw new ArgumentError("slot cannot be null.");
		slots[slots.length] = slot;
	}

	/** @return May be null. */
	public function findSlot (slotName:String) : SlotData {
		if (slotName == null)
			throw new ArgumentError("slotName cannot be null.");
		for (var i:int = 0, n:int = slots.length; i < n; i++) {
			var slot:SlotData = slots[i];
			if (slot._name == slotName)
				return slot;
		}
		return null;
	}

	/** @return -1 if the bone was not found. */
	public function findSlotIndex (slotName:String) : int {
		if (slotName == null)
			throw new ArgumentError("slotName cannot be null.");
		for (var i:int = 0, n:int = slots.length; i < n; i++)
			if (slots[i]._name == slotName)
				return i;
		return -1;
	}

	// --- Skins.

	public function addSkin (skin:Skin) : void {
		if (skin == null)
			throw new ArgumentError("skin cannot be null.");
		skins[skins.length] = skin;
	}

	/** @return May be null. */
	public function findSkin (skinName:String) : Skin {
		if (skinName == null)
			throw new ArgumentError("skinName cannot be null.");
		for each (var skin:Skin in skins)
			if (skin._name == skinName)
				return skin;
		return null;
	}
	
	// --- Events.
	
	public function addEvent (eventData:EventData) : void {
		if (eventData == null)
			throw new ArgumentError("eventData cannot be null.");
		events[events.length] = eventData;
	}
	
	/** @return May be null. */
	public function findEvent (eventName:String) : EventData {
		if (eventName == null)
			throw new ArgumentError("eventName cannot be null.");
		for (var i:int = 0, n:int = events.length; i < n; i++) {
			var eventData:EventData = events[i];
			if (eventData.name == eventName)
				return eventData;
		}
		return null;
	}
	
	// --- Animations.
	
	public function addAnimation (animation:Animation) : void {
		if (animation == null)
			throw new ArgumentError("animation cannot be null.");
		animations[animations.length] = animation;
	}
	
	/** @return May be null. */
	public function findAnimation (animationName:String) : Animation {
		if (animationName == null)
			throw new ArgumentError("animationName cannot be null.");
		for (var i:int = 0, n:int = animations.length; i < n; i++) {
			var animation:Animation = animations[i];
			if (animation.name == animationName)
				return animation;
		}
		return null;
	}

	// ---

	public function toString () : String {
		return name != null ? name : super.toString();
	}
}

}
