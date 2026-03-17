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

/** Changes a subset of a skeleton's spine.Skeleton.drawOrder. */
class DrawOrderFolderTimeline extends Timeline {
	private var slots:Array<Int>;
	private var inFolder:Array<Bool>;
	private var drawOrders:Array<Array<Int>>;

	/** @param slots spine.Skeleton.slots indices controlled by this timeline, in setup order.
	 * @param slotCount The maximum number of slots in the skeleton. */
	public function new(frameCount:Int, slots:Array<Int>, slotCount:Int) {
		super(frameCount, Property.drawOrder);
		this.slots = slots;
		drawOrders = new Array<Array<Int>>();
		drawOrders.resize(frameCount);
		inFolder = new Array<Bool>();
		inFolder.resize(slotCount);
		for (i in 0...slotCount)
			inFolder[i] = false;
		for (i in slots)
			inFolder[i] = true;
	}

	public var frameCount(get, never):Int;

	private function get_frameCount():Int {
		return frames.length;
	}

	/** The spine.Skeleton.slots indices that this timeline affects, in setup order. */
	public function getSlots():Array<Int> {
		return slots;
	}

	/** The draw order for each frame. See setFrame(). */
	public function getDrawOrders():Array<Array<Int>> {
		return drawOrders;
	}

	/** Sets the time and draw order for the specified frame.
	 * @param frame Between 0 and frameCount, inclusive.
	 * @param time The frame time in seconds.
	 * @param drawOrder Ordered getSlots() indices, or null to use setup pose order. */
	public function setFrame(frame:Int, time:Float, drawOrder:Array<Int>):Void {
		frames[frame] = time;
		drawOrders[frame] = drawOrder;
	}

	public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, blend:MixBlend, direction:MixDirection,
			appliedPose:Bool) {
		if (direction == MixDirection.mixOut) {
			if (blend == MixBlend.setup) setupApply(skeleton);
		} else if (time < frames[0]) {
			if (blend == MixBlend.setup || blend == MixBlend.first) setupApply(skeleton);
		} else {
			var order = drawOrders[Timeline.search1(frames, time)];
			if (order == null)
				setupApply(skeleton);
			else
				orderApply(skeleton, order);
		}
	}

	private function setupApply(skeleton:Skeleton):Void {
		var drawOrder = skeleton.drawOrder;
		var allSlots = skeleton.slots;
		var found = 0, done = slots.length;
		var i = 0;
		while (true) {
			if (inFolder[drawOrder[i].data.index]) {
				drawOrder[i] = allSlots[slots[found]];
				found++;
				if (found == done) break;
			}
			i++;
		}
	}

	private function orderApply(skeleton:Skeleton, order:Array<Int>):Void {
		var drawOrder = skeleton.drawOrder;
		var allSlots = skeleton.slots;
		var found = 0, done = slots.length;
		var i = 0;
		while (true) {
			if (inFolder[drawOrder[i].data.index]) {
				drawOrder[i] = allSlots[slots[order[found]]];
				found++;
				if (found == done) break;
			}
			i++;
		}
	}
}
