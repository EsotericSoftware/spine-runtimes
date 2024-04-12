/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine.animation;

import spine.Event;
import spine.Skeleton;
import spine.TransformConstraint;
import spine.TransformConstraintData;

class TransformConstraintTimeline extends CurveTimeline {
	static public inline var ENTRIES:Int = 7;
	private static inline var ROTATE:Int = 1;
	private static inline var X:Int = 2;
	private static inline var Y:Int = 3;
	private static inline var SCALEX:Int = 4;
	private static inline var SCALEY:Int = 5;
	private static inline var SHEARY:Int = 6;

	/** The index of the transform constraint slot in {@link Skeleton#transformConstraints} that will be changed. */
	public var constraintIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, transformConstraintIndex:Int) {
		super(frameCount, bezierCount, [Property.transformConstraint + "|" + transformConstraintIndex]);
		this.constraintIndex = transformConstraintIndex;
	}

	public override function getFrameEntries():Int {
		return ENTRIES;
	}

	/** The time in seconds, rotate mix, translate mix, scale mix, and shear mix for the specified key frame. */
	public function setFrame(frame:Int, time:Float, mixRotate:Float, mixX:Float, mixY:Float, mixScaleX:Float, mixScaleY:Float, mixShearY:Float):Void {
		frame *= ENTRIES;
		frames[frame] = time;
		frames[frame + ROTATE] = mixRotate;
		frames[frame + X] = mixX;
		frames[frame + Y] = mixY;
		frames[frame + SCALEX] = mixScaleX;
		frames[frame + SCALEY] = mixScaleY;
		frames[frame + SHEARY] = mixShearY;
	}

	override public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		var constraint:TransformConstraint = skeleton.transformConstraints[constraintIndex];
		if (!constraint.active)
			return;

		var data:TransformConstraintData;
		if (time < frames[0]) {
			data = constraint.data;
			switch (blend) {
				case MixBlend.setup:
					constraint.mixRotate = data.mixRotate;
					constraint.mixX = data.mixX;
					constraint.mixY = data.mixY;
					constraint.mixScaleX = data.mixScaleX;
					constraint.mixScaleY = data.mixScaleY;
					constraint.mixShearY = data.mixShearY;
				case MixBlend.first:
					constraint.mixRotate += (data.mixRotate - constraint.mixRotate) * alpha;
					constraint.mixX += (data.mixX - constraint.mixX) * alpha;
					constraint.mixY += (data.mixY - constraint.mixY) * alpha;
					constraint.mixScaleX += (data.mixScaleX - constraint.mixScaleX) * alpha;
					constraint.mixScaleY += (data.mixScaleY - constraint.mixScaleY) * alpha;
					constraint.mixShearY += (data.mixShearY - constraint.mixShearY) * alpha;
			}
			return;
		}

		var rotate:Float, x:Float, y:Float, scaleX:Float, scaleY:Float, shearY:Float;
		var i:Int = Timeline.search(frames, time, ENTRIES);
		var curveType:Int = Std.int(curves[Std.int(i / ENTRIES)]);
		switch (curveType) {
			case CurveTimeline.LINEAR:
				var before:Float = frames[i];
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				scaleX = frames[i + SCALEX];
				scaleY = frames[i + SCALEY];
				shearY = frames[i + SHEARY];
				var t:Float = (time - before) / (frames[i + ENTRIES] - before);
				rotate += (frames[i + ENTRIES + ROTATE] - rotate) * t;
				x += (frames[i + ENTRIES + X] - x) * t;
				y += (frames[i + ENTRIES + Y] - y) * t;
				scaleX += (frames[i + ENTRIES + SCALEX] - scaleX) * t;
				scaleY += (frames[i + ENTRIES + SCALEY] - scaleY) * t;
				shearY += (frames[i + ENTRIES + SHEARY] - shearY) * t;
			case CurveTimeline.STEPPED:
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				scaleX = frames[i + SCALEX];
				scaleY = frames[i + SCALEY];
				shearY = frames[i + SHEARY];
			default:
				rotate = getBezierValue(time, i, ROTATE, curveType - CurveTimeline.BEZIER);
				x = getBezierValue(time, i, X, curveType + CurveTimeline.BEZIER_SIZE - CurveTimeline.BEZIER);
				y = getBezierValue(time, i, Y, curveType + CurveTimeline.BEZIER_SIZE * 2 - CurveTimeline.BEZIER);
				scaleX = getBezierValue(time, i, SCALEX, curveType + CurveTimeline.BEZIER_SIZE * 3 - CurveTimeline.BEZIER);
				scaleY = getBezierValue(time, i, SCALEY, curveType + CurveTimeline.BEZIER_SIZE * 4 - CurveTimeline.BEZIER);
				shearY = getBezierValue(time, i, SHEARY, curveType + CurveTimeline.BEZIER_SIZE * 5 - CurveTimeline.BEZIER);
		}

		if (blend == MixBlend.setup) {
			data = constraint.data;
			constraint.mixRotate = data.mixRotate + (rotate - data.mixRotate) * alpha;
			constraint.mixX = data.mixX + (x - data.mixX) * alpha;
			constraint.mixY = data.mixY + (y - data.mixY) * alpha;
			constraint.mixScaleX = data.mixScaleX + (scaleX - data.mixScaleX) * alpha;
			constraint.mixScaleY = data.mixScaleY + (scaleY - data.mixScaleY) * alpha;
			constraint.mixShearY = data.mixShearY + (shearY - data.mixShearY) * alpha;
		} else {
			constraint.mixRotate += (rotate - constraint.mixRotate) * alpha;
			constraint.mixX += (x - constraint.mixX) * alpha;
			constraint.mixY += (y - constraint.mixY) * alpha;
			constraint.mixScaleX += (scaleX - constraint.mixScaleX) * alpha;
			constraint.mixScaleY += (scaleY - constraint.mixScaleY) * alpha;
			constraint.mixShearY += (shearY - constraint.mixShearY) * alpha;
		}
	}
}
