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
	/** The base class for timelines that interpolate between frame values using stepped, linear, or a Bezier curve. */
	public class CurveTimeline extends Timeline {
		static internal const LINEAR : Number = 0;
		static internal const STEPPED : Number = 1;
		static internal const BEZIER : Number = 2;
		static internal const BEZIER_SIZE : int = 18;

		internal var curves : Vector.<Number>; // type, x, y, ...

		public function CurveTimeline(frameCount : int, bezierCount : int, propertyIds : Array) {
			super(frameCount, propertyIds);
			curves = new Vector.<Number>(frameCount + bezierCount * BEZIER_SIZE, true);
			curves[frameCount - 1] = STEPPED;
		}

		/** Sets the specified key frame to linear interpolation. */
		public function setLinear(frame : int) : void {
			curves[frame] = LINEAR;
		}

		/** Sets the specified key frame to stepped interpolation. */
		public function setStepped(frame : int) : void{
			curves[frame] = STEPPED;
		}

		/** Shrinks the storage for Bezier curves, for use when <code>bezierCount</code> (specified in the constructor) was larger
		 * than the actual number of Bezier curves. */
		public function shrink(bezierCount : int) : void {
			var size : int = getFrameCount() + bezierCount * BEZIER_SIZE;
			var curves : Vector.<Number> = this.curves;
			if (curves.length > size) {
				var newCurves : Vector.<Number> = new Vector.<Number>(size, true);
				for (var i : int = 0; i < size; i++)
					newCurves[i] = curves[i];
				curves = newCurves;
			}
		}

		/** Stores the segments for the specified Bezier curve. For timelines that modify multiple values, there may be more than
		 * one curve per frame.
		 * @param bezier The ordinal of this Bezier curve for this timeline, between 0 and <code>bezierCount - 1</code> (specified
		 *           in the constructor), inclusive.
		 * @param frame Between 0 and <code>frameCount - 1</code>, inclusive.
		 * @param value The index of the value for this frame that this curve is used for.
		 * @param time1 The time for the first key.
		 * @param value1 The value for the first key.
		 * @param cx1 The time for the first Bezier handle.
		 * @param cy1 The value for the first Bezier handle.
		 * @param cx2 The time of the second Bezier handle.
		 * @param cy2 The value for the second Bezier handle.
		 * @param time2 The time for the second key.
		 * @param value2 The value for the second key. */
		public function setBezier(bezier : int, frame : int, value : Number, time1 : Number, value1 : Number, cx1 : Number, cy1 : Number, cx2 : Number,
			cy2 : Number, time2 : Number, value2 : Number) : void {
			var curves : Vector.<Number> = this.curves;
			var i : int = getFrameCount() + bezier * BEZIER_SIZE;
			if (value == 0) curves[frame] = BEZIER + i;
			var tmpx : Number = (time1 - cx1 * 2 + cx2) * 0.03, tmpy : Number = (value1 - cy1 * 2 + cy2) * 0.03;
			var dddx : Number = ((cx1 - cx2) * 3 - time1 + time2) * 0.006, dddy : Number = ((cy1 - cy2) * 3 - value1 + value2) * 0.006;
			var ddx : Number = tmpx * 2 + dddx, ddy : Number = tmpy * 2 + dddy;
			var dx : Number = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667, dy : Number = (cy1 - value1) * 0.3 + tmpy + dddy * 0.16666667;
			var x : Number = time1 + dx, y : Number = value1 + dy;
			for (var n : int = i + BEZIER_SIZE; i < n; i += 2) {
				curves[i] = x;
				curves[i + 1] = y;
				dx += ddx;
				dy += ddy;
				ddx += dddx;
				ddy += dddy;
				x += dx;
				y += dy;
			}
		}

		/** Returns the Bezier interpolated value for the specified time.
		 * @param frameIndex The index into {@link #getFrames()} for the values of the frame before <code>time</code>.
		 * @param valueOffset The offset from <code>frameIndex</code> to the value this curve is used for.
		 * @param i The index of the Bezier segments. See {@link #getCurveType(int)}. */
		public function getBezierValue(time : Number, frameIndex : int, valueOffset : int, i : int) : Number {
			var curves : Vector.<Number> = this.curves;
			var x : Number, y : Number;
			if (curves[i] > time) {
				x = frames[frameIndex];
				y = frames[frameIndex + valueOffset];
				return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
			}
			var n : int = i + BEZIER_SIZE;
			for (i += 2; i < n; i += 2) {
				if (curves[i] >= time) {
					x = curves[i - 2];
					y = curves[i - 1];
					return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
				}
			}
			frameIndex += getFrameEntries();
			x = curves[n - 2];
			y = curves[n - 1];
			return y + (time - x) / (frames[frameIndex] - x) * (frames[frameIndex + valueOffset] - y);
		}
	}
}
