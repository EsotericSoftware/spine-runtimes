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

/** Changes a skeleton's Skeleton#drawOrder. */
class DrawOrderTimeline extends Timeline {
	public static final propertyID = Property.drawOrder;

	/** The draw order for each frame. See setFrame(). */
	public var drawOrders:Array<Array<Int>>;

	public function new(frameCount:Int) {
		super(frameCount, Property.drawOrder);
		drawOrders = new Array<Array<Int>>();
		drawOrders.resize(frameCount);
		this.instant = true;
	}

	public var frameCount(get, never):Int;

	private function get_frameCount():Int {
		return frames.length;
	}

	/** Sets the time and draw order for the specified frame.
	 * @param frame Between 0 and frameCount, inclusive.
	 * @param time The frame time in seconds.
	 * @param drawOrder Ordered spine.Skeleton.slots indices, or null to use setup pose order. */
	public function setFrame(frame:Int, time:Float, drawOrder:Array<Int>):Void {
		frames[frame] = time;
		drawOrders[frame] = drawOrder;
	}

	public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, fromSetup:Bool, add:Bool, out:Bool,
			appliedPose:Bool) {
		var drawOrder = appliedPose ? skeleton.drawOrder.appliedPose : skeleton.drawOrder.pose;
		var slots:Array<Slot> = skeleton.slots;
		var i:Int = 0, n:Int = slots.length;

		if (out) {
			if (fromSetup) {
				for (i in 0...n)
					drawOrder[i] = slots[i];
			}
			return;
		}

		if (time < frames[0]) {
			if (fromSetup) {
				for (i in 0...n)
					drawOrder[i] = slots[i];
			}
			return;
		}

		var drawOrderToSetupIndex:Array<Int> = drawOrders[Timeline.search1(frames, time)];
		if (drawOrderToSetupIndex == null) {
			for (i in 0...n)
				drawOrder[i] = slots[i];
		} else {
			for (i in 0...n)
				drawOrder[i] = slots[drawOrderToSetupIndex[i]];
		}
	}
}
