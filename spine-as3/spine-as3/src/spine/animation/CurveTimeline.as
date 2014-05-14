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

package spine.animation {
import spine.Event;
import spine.Skeleton;

/** Base class for frames that use an interpolation bezier curve. */
public class CurveTimeline implements Timeline {
	static private const LINEAR:Number = 0;
	static private const STEPPED:Number = -1;
	static private const BEZIER_SEGMENTS:int = 10;

	private var curves:Vector.<Number> = new Vector.<Number>(); // dfx, dfy, ddfx, ddfy, dddfx, dddfy, ...

	public function CurveTimeline (frameCount:int) {
		curves.length = frameCount * 6;
	}

	public function apply (skeleton:Skeleton, lastTime:Number, time:Number, firedEvents:Vector.<Event>, alpha:Number) : void {
	}

	public function get frameCount () : int {
		return curves.length / 6;
	}

	public function setLinear (frameIndex:int) : void {
		curves[int(frameIndex * 6)] = LINEAR;
	}

	public function setStepped (frameIndex:int) : void {
		curves[int(frameIndex * 6)] = STEPPED;
	}

	/** Sets the control handle positions for an interpolation bezier curve used to transition from this keyframe to the next.
	 * cx1 and cx2 are from 0 to 1, representing the percent of time between the two keyframes. cy1 and cy2 are the percent of
	 * the difference between the keyframe's values. */
	public function setCurve (frameIndex:int, cx1:Number, cy1:Number, cx2:Number, cy2:Number) : void {
		var subdiv_step:Number = 1 / BEZIER_SEGMENTS;
		var subdiv_step2:Number = subdiv_step * subdiv_step;
		var subdiv_step3:Number = subdiv_step2 * subdiv_step;
		var pre1:Number = 3 * subdiv_step;
		var pre2:Number = 3 * subdiv_step2;
		var pre4:Number = 6 * subdiv_step2;
		var pre5:Number = 6 * subdiv_step3;
		var tmp1x:Number = -cx1 * 2 + cx2;
		var tmp1y:Number = -cy1 * 2 + cy2;
		var tmp2x:Number = (cx1 - cx2) * 3 + 1;
		var tmp2y:Number = (cy1 - cy2) * 3 + 1;
		var i:int = frameIndex * 6;
		curves[i] = cx1 * pre1 + tmp1x * pre2 + tmp2x * subdiv_step3;
		curves[int(i + 1)] = cy1 * pre1 + tmp1y * pre2 + tmp2y * subdiv_step3;
		curves[int(i + 2)] = tmp1x * pre4 + tmp2x * pre5;
		curves[int(i + 3)] = tmp1y * pre4 + tmp2y * pre5;
		curves[int(i + 4)] = tmp2x * pre5;
		curves[int(i + 5)] = tmp2y * pre5;
	}

	public function getCurvePercent (frameIndex:int, percent:Number) : Number {
		var curveIndex:int = frameIndex * 6;
		var dfx:Number = curves[curveIndex];
		if (dfx == LINEAR)
			return percent;
		if (dfx == STEPPED)
			return 0;
		var dfy:Number = curves[int(curveIndex + 1)];
		var ddfx:Number = curves[int(curveIndex + 2)];
		var ddfy:Number = curves[int(curveIndex + 3)];
		var dddfx:Number = curves[int(curveIndex + 4)];
		var dddfy:Number = curves[int(curveIndex + 5)];
		var x:Number = dfx;
		var y:Number = dfy;
		var i:int = BEZIER_SEGMENTS - 2;
		while (true) {
			if (x >= percent) {
				var prevX:Number = x - dfx;
				var prevY:Number = y - dfy;
				return prevY + (y - prevY) * (percent - prevX) / (x - prevX);
			}
			if (i == 0)
				break;
			i--;
			dfx += ddfx;
			dfy += ddfy;
			ddfx += dddfx;
			ddfy += dddfy;
			x += dfx;
			y += dfy;
		}
		return y + (1 - y) * (percent - x) / (1 - x); // Last point is 1,1.
	}
}

}
