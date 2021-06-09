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
	/** The base class for a {@link CurveTimeline} that sets one property. */
	public class CurveTimeline1 extends CurveTimeline {
		static private const ENTRIES : Number = 2;
		static private const VALUE : Number = 1;

		/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(int)}.
		 * @param propertyIds Unique identifiers for the properties the timeline modifies. */
		public function CurveTimeline1 (frameCount : int, bezierCount : int, propertyId : String) {
			super(frameCount, bezierCount, [ propertyId ]);
		}

		public override function getFrameEntries() : int {
			return ENTRIES;
		}

		/** Sets the time and values for the specified frame.
		 * @param frame Between 0 and <code>frameCount</code>, inclusive.
		 * @param time The frame time in seconds. */
		public function setFrame(frame : int, time : Number, value1 : Number) : void {
			frame <<= 1;
			frames[frame] = time;
			frames[frame + VALUE] = value1;
		}

		/** Returns the interpolated value for the specified time. */
		public function getCurveValue(time : Number) : Number {
			var frames : Vector.<Number> = this.frames;
			var i : int = frames.length - 2;
			for (var ii : int = 2; ii <= i; ii += 2) {
				if (frames[ii] > time) {
					i = ii - 2;
					break;
				}
			}

			var curveType : Number = curves[i >> 1];
			switch (curveType) {
			case LINEAR:
				var before : Number = frames[i], value : Number = frames[i + VALUE];
				return value + (time - before) / (frames[i + ENTRIES] - before) * (frames[i + ENTRIES + VALUE] - value);
			case STEPPED:
				return frames[i + VALUE];
			}
			return getBezierValue(time, i, VALUE, curveType - BEZIER);
		}
	}
}
