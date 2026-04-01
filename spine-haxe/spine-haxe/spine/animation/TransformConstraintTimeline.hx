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

/** Changes a transform constraint's spine.TransformConstraint.mixRotate, spine.TransformConstraint.mixX,
 * spine.TransformConstraint.mixY, spine.TransformConstraint.mixScaleX,
 * spine.TransformConstraint.mixScaleY, and spine.TransformConstraint.mixShearY. */
class TransformConstraintTimeline extends CurveTimeline implements ConstraintTimeline {
	static public inline var ENTRIES:Int = 7;
	private static inline var ROTATE:Int = 1;
	private static inline var X:Int = 2;
	private static inline var Y:Int = 3;
	private static inline var SCALEX:Int = 4;
	private static inline var SCALEY:Int = 5;
	private static inline var SHEARY:Int = 6;

	/** The index of the transform constraint in spine.Skeleton.transformConstraints that will be changed when this
	 * timeline is applied. */
	public var constraintIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, constraintIndex:Int) {
		super(frameCount, bezierCount, Property.transformConstraint + "|" + constraintIndex);
		this.constraintIndex = constraintIndex;
	}

	public override function getFrameEntries():Int {
		return ENTRIES;
	}

	public function getConstraintIndex() {
		return constraintIndex;
	}

	/** Sets the time, rotate mix, translate mix, scale mix, and shear mix for the specified frame.
	 * @param frame Between 0 and frameCount, inclusive.
	 * @param time The frame time in seconds. */
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

	public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, fromSetup:Bool, add:Bool, out:Bool,
			appliedPose:Bool) {
		var constraint = cast(skeleton.constraints[constraintIndex], TransformConstraint);
		if (!constraint.active)
			return;
		var pose = appliedPose ? constraint.appliedPose : constraint.pose;

		if (time < frames[0]) {
			if (fromSetup) {
				var setup = constraint.data.setupPose;
				pose.mixRotate = setup.mixRotate;
				pose.mixX = setup.mixX;
				pose.mixY = setup.mixY;
				pose.mixScaleX = setup.mixScaleX;
				pose.mixScaleY = setup.mixScaleY;
				pose.mixShearY = setup.mixShearY;
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

		var base = fromSetup ? constraint.data.setupPose : pose;
		pose.mixRotate = base.mixRotate + (rotate - base.mixRotate) * alpha;
		pose.mixX = base.mixX + (x - base.mixX) * alpha;
		pose.mixY = base.mixY + (y - base.mixY) * alpha;
		pose.mixScaleX = base.mixScaleX + (scaleX - base.mixScaleX) * alpha;
		pose.mixScaleY = base.mixScaleY + (scaleY - base.mixScaleY) * alpha;
		pose.mixShearY = base.mixShearY + (shearY - base.mixShearY) * alpha;
	}
}
