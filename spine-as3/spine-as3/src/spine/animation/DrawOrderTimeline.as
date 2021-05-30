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

package spine.animation {
	import spine.Event;
	import spine.Skeleton;
	import spine.Slot;

	public class DrawOrderTimeline extends Timeline {
		public var drawOrders : Vector.<Vector.<int>>;

		public function DrawOrderTimeline(frameCount : int) {
			super(frameCount, [
				Property.drawOrder
			]);
			drawOrders = new Vector.<Vector.<int>>(frameCount, true);
		}

		public function get frameCount() : int {
			return frames.length;
		}

		/** Sets the time in seconds and the draw order for the specified key frame.
		 * @param drawOrder For each slot in {@link Skeleton#slots}, the index of the new draw order. May be null to use setup pose
		 *           draw order. */
		public function setFrame(frame : int, time : Number, drawOrder : Vector.<int>) : void {
			frames[frame] = time;
			drawOrders[frame] = drawOrder;
		}

		public override function apply (skeleton : Skeleton, lastTime : Number, time : Number, events : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var drawOrder: Vector.<Slot> = skeleton.drawOrder;
			var slots : Vector.<Slot> = skeleton.slots;
			var i : int = 0, n : int = slots.length;
		
			if (direction == MixDirection.mixOut) {
				if (blend == MixBlend.setup) {
					for (i = 0; i < n; i++)
						drawOrder[i] = slots[i];
				}
				return;
			}

			if (time < frames[0]) {
				if (blend == MixBlend.setup || blend == MixBlend.first) {
					for (i = 0; i < n; i++)
						drawOrder[i] = slots[i];
				}
				return;
			}

			var drawOrderToSetupIndex : Vector.<int> = drawOrders[search(frames, time)];
			if (drawOrderToSetupIndex == null) {
				for (i = 0; i < n; i++)
					drawOrder[i] = slots[i];
			} else {
				for (i = 0; i < n; i++)
					drawOrder[i] = slots[drawOrderToSetupIndex[i]];
			}
		}
	}
}
