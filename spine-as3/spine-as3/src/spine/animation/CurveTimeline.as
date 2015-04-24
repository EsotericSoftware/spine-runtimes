/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.animation {
import spine.Event;
import spine.Skeleton;

/** Base class for frames that use an interpolation bezier curve. */
public class CurveTimeline implements Timeline {
	static private const LINEAR:Number = 0;
	static private const STEPPED:Number = 1;
	static private const BEZIER:Number = 2;
	static private const BEZIER_SEGMENTS:int = 10;
	static private const BEZIER_SIZE:int = BEZIER_SEGMENTS * 2 - 1;

	private var curves:Vector.<Number>; // type, x, y, ...

	public function CurveTimeline (frameCount:int) {
		curves = new Vector.<Number>((frameCount - 1) * BEZIER_SIZE, true)
	}

	public function apply (skeleton:Skeleton, lastTime:Number, time:Number, firedEvents:Vector.<Event>, alpha:Number) : void {
	}

	public function get frameCount () : int {
		return curves.length / BEZIER_SIZE + 1;
	}

	public function setLinear (frameIndex:int) : void {
		curves[int(frameIndex * BEZIER_SIZE)] = LINEAR;
	}

	public function setStepped (frameIndex:int) : void {
		curves[int(frameIndex * BEZIER_SIZE)] = STEPPED;
	}

	/** Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
	 * cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
	 * the difference between the keyframe's values. */
	public function setCurve (frameIndex:int, cx1:Number, cy1:Number, cx2:Number, cy2:Number) : void {
		var subdiv1:Number = 1 / BEZIER_SEGMENTS, subdiv2:Number = subdiv1 * subdiv1, subdiv3:Number = subdiv2 * subdiv1;
		var pre1:Number = 3 * subdiv1, pre2:Number = 3 * subdiv2, pre4:Number = 6 * subdiv2, pre5:Number = 6 * subdiv3;
		var tmp1x:Number = -cx1 * 2 + cx2, tmp1y:Number = -cy1 * 2 + cy2, tmp2x:Number = (cx1 - cx2) * 3 + 1, tmp2y:Number = (cy1 - cy2) * 3 + 1;
		var dfx:Number = cx1 * pre1 + tmp1x * pre2 + tmp2x * subdiv3, dfy:Number = cy1 * pre1 + tmp1y * pre2 + tmp2y * subdiv3;
		var ddfx:Number = tmp1x * pre4 + tmp2x * pre5, ddfy:Number = tmp1y * pre4 + tmp2y * pre5;
		var dddfx:Number = tmp2x * pre5, dddfy:Number = tmp2y * pre5;

		var i:int = frameIndex * BEZIER_SIZE;
		var curves:Vector.<Number> = this.curves;
		curves[int(i++)] = BEZIER;

		var x:Number = dfx, y:Number = dfy;
		for (var n:int = i + BEZIER_SIZE - 1; i < n; i += 2) {
			curves[i] = x;
			curves[int(i + 1)] = y;
			dfx += ddfx;
			dfy += ddfy;
			ddfx += dddfx;
			ddfy += dddfy;
			x += dfx;
			y += dfy;
		}
	}

	public function getCurvePercent (frameIndex:int, percent:Number) : Number {
		var curves:Vector.<Number> = this.curves;
		var i:int = frameIndex * BEZIER_SIZE;
		var type:Number = curves[i];
		if (type == LINEAR) return percent;
		if (type == STEPPED) return 0;
		i++;
		var x:Number = 0;
		for (var start:int = i, n:int = i + BEZIER_SIZE - 1; i < n; i += 2) {
			x = curves[i];
			if (x >= percent) {
				var prevX:Number, prevY:Number;
				if (i == start) {
					prevX = 0;
					prevY = 0;
				} else {
					prevX = curves[int(i - 2)];
					prevY = curves[int(i - 1)];
				}
				return prevY + (curves[int(i + 1)] - prevY) * (percent - prevX) / (x - prevX);
			}
		}
		var y:Number = curves[int(i - 1)];
		return y + (1 - y) * (percent - x) / (1 - x); // Last point is 1,1.
	}
}

}
