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

package spine.animation;

import spine.Event;
import spine.Skeleton;
import spine.Slot;

/** Changes the alpha for a slot's spine.Slot.color. */
class AlphaTimeline extends CurveTimeline1 implements SlotTimeline {
	private static inline var ENTRIES:Int = 4;
	private static inline var R:Float = 1;
	private static inline var G:Float = 2;
	private static inline var B:Float = 3;

	private var slotIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, slotIndex:Int) {
		super(frameCount, bezierCount, Property.alpha + "|" + slotIndex);
		this.slotIndex = slotIndex;
	}

	public override function getFrameEntries():Int {
		return ENTRIES;
	}

	public function getSlotIndex():Int {
		return slotIndex;
	}

	public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, fromSetup:Bool, add:Bool, out:Bool,
			appliedPose:Bool) {
		var slot = skeleton.slots[slotIndex];
		if (!slot.bone.active)
			return;

		var color = (appliedPose ? slot.appliedPose : slot.pose).color;
		var a:Float = 0;
		if (time < frames[0]) {
			if (fromSetup)
				color.a = slot.data.setupPose.color.a;
			return;
		}

		a = getCurveValue(time);
		if (alpha != 1) {
			if (fromSetup) {
				var setup = slot.data.setupPose.color;
				a = setup.a + (a - setup.a) * alpha;
			} else
				a = color.a + (a - color.a) * alpha;
		}
		color.a = a < 0 ? 0 : (a > 1 ? 1 : a);
	}
}
