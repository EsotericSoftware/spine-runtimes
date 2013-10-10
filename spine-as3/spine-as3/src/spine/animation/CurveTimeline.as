/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
		curves[frameIndex * 6] = LINEAR;
	}

	public function setStepped (frameIndex:int) : void {
		curves[frameIndex * 6] = STEPPED;
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
		curves[i + 1] = cy1 * pre1 + tmp1y * pre2 + tmp2y * subdiv_step3;
		curves[i + 2] = tmp1x * pre4 + tmp2x * pre5;
		curves[i + 3] = tmp1y * pre4 + tmp2y * pre5;
		curves[i + 4] = tmp2x * pre5;
		curves[i + 5] = tmp2y * pre5;
	}

	public function getCurvePercent (frameIndex:int, percent:Number) : Number {
		var curveIndex:int = frameIndex * 6;
		var dfx:Number = curves[curveIndex];
		if (dfx == LINEAR)
			return percent;
		if (dfx == STEPPED)
			return 0;
		var dfy:Number = curves[curveIndex + 1];
		var ddfx:Number = curves[curveIndex + 2];
		var ddfy:Number = curves[curveIndex + 3];
		var dddfx:Number = curves[curveIndex + 4];
		var dddfy:Number = curves[curveIndex + 5];
		var x:Number = dfx;
		var y:Number = dfy;
		var i:int = BEZIER_SEGMENTS - 2;
		while (true) {
			if (x >= percent) {
				var lastX:Number = x - dfx;
				var lastY:Number = y - dfy;
				return lastY + (y - lastY) * (percent - lastX) / (x - lastX);
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
