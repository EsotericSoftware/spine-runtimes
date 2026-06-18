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

/** Changes an IK constraint's spine.IkConstraintPose.mix, spine.IkConstraintPose.softness,
 * spine.IkConstraintPose.bendDirection, spine.IkConstraintPose.stretch, and spine.IkConstraintPose.compress. */
class IkConstraintTimeline extends CurveTimeline implements ConstraintTimeline {
	private static inline var ENTRIES:Int = 6;
	private static inline var MIX:Int = 1;
	private static inline var SOFTNESS:Int = 2;
	private static inline var BEND_DIRECTION:Int = 3;
	private static inline var COMPRESS:Int = 4;
	private static inline var STRETCH:Int = 5;

	/** The index of the IK constraint in spine.Skeleton.ikConstraints that will be changed when this timeline is
	 * applied. */
	public var constraintIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, constraintIndex:Int) {
		super(frameCount, bezierCount, Property.ikConstraint + "|" + constraintIndex);
		this.constraintIndex = constraintIndex;
	}

	public override function getFrameEntries():Int {
		return ENTRIES;
	}

	public function getConstraintIndex() {
		return constraintIndex;
	}

	/** Sets the time, mix, softness, bend direction, compress, and stretch for the specified frame.
	 * @param frame Between 0 and frameCount, inclusive.
	 * @param time The frame time in seconds.
	 * @param bendDirection 1 or -1. */
	public function setFrame(frame:Int, time:Float, mix:Float, softness:Float, bendDirection:Int, compress:Bool, stretch:Bool):Void {
		frame *= ENTRIES;
		frames[frame] = time;
		frames[frame + MIX] = mix;
		frames[frame + SOFTNESS] = softness;
		frames[frame + BEND_DIRECTION] = bendDirection;
		frames[frame + COMPRESS] = compress ? 1 : 0;
		frames[frame + STRETCH] = stretch ? 1 : 0;
	}

	public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, from:MixFrom, add:Bool, out:Bool, appliedPose:Bool) {
		var constraint = cast(skeleton.constraints[constraintIndex], IkConstraint);
		if (!constraint.active)
			return;
		var pose = appliedPose ? constraint.appliedPose : constraint.pose;

		if (time < frames[0]) {
			var setup = constraint.data.setupPose;
			if (from == MixFrom.setup) {
				pose.mix = setup.mix;
				pose.softness = setup.softness;
				pose.bendDirection = setup.bendDirection;
				pose.compress = setup.compress;
				pose.stretch = setup.stretch;
			} else if (from == MixFrom.first) {
				pose.mix += (setup.mix - pose.mix) * alpha;
				pose.softness += (setup.softness - pose.softness) * alpha;
				pose.bendDirection = setup.bendDirection;
				pose.compress = setup.compress;
				pose.stretch = setup.stretch;
			}
			return;
		}

		var mix:Float = 0, softness:Float = 0;
		var i:Int = Timeline.search(frames, time, ENTRIES);
		var curveType:Int = Std.int(curves[Std.int(i / ENTRIES)]);
		switch (curveType) {
			case CurveTimeline.LINEAR:
				var before:Float = frames[i];
				mix = frames[i + MIX];
				softness = frames[i + SOFTNESS];
				var t:Float = (time - before) / (frames[i + ENTRIES] - before);
				mix += (frames[i + ENTRIES + MIX] - mix) * t;
				softness += (frames[i + ENTRIES + SOFTNESS] - softness) * t;
			case CurveTimeline.STEPPED:
				mix = frames[i + MIX];
				softness = frames[i + SOFTNESS];
			default:
				mix = getBezierValue(time, i, MIX, curveType - CurveTimeline.BEZIER);
				softness = getBezierValue(time, i, SOFTNESS, curveType + CurveTimeline.BEZIER_SIZE - CurveTimeline.BEZIER);
		}

		var base = from == MixFrom.setup ? constraint.data.setupPose : pose;
		pose.mix = base.mix + (mix - base.mix) * alpha;
		pose.softness = base.softness + (softness - base.softness) * alpha;
		if (out) {
			if (from == MixFrom.setup) {
				pose.bendDirection = base.bendDirection;
				pose.compress = base.compress;
				pose.stretch = base.stretch;
			}
		} else {
			pose.bendDirection = Std.int(frames[i + BEND_DIRECTION]);
			pose.compress = frames[i + COMPRESS] != 0;
			pose.stretch = frames[i + STRETCH] != 0;
		}
	}
}
