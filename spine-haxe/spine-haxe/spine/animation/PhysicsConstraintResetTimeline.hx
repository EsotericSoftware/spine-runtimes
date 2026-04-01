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

import spine.animation.Timeline;
import spine.Event;
import spine.Skeleton;

/** Resets a physics constraint when specific animation times are reached. */
class PhysicsConstraintResetTimeline extends Timeline implements ConstraintTimeline {
	public var constraintIndex:Int;

	/** @param constraintIndex -1 for all physics constraints in the skeleton. */
	public function new(frameCount:Int, constraintIndex:Int) {
		super(frameCount, Property.physicsConstraintReset);
		this.constraintIndex = constraintIndex;
	}

	public function getConstraintIndex() {
		return constraintIndex;
	}

	public override function getFrameCount():Int {
		return frames.length;
	}

	/** Sets the time for the specified frame.
	 * @param frame Between 0 and frameCount, inclusive. */
	public function setFrame(frame:Int, time:Float):Void {
		frames[frame] = time;
	}

	/** Resets the physics constraint when frames > lastTime and <= time. */
	public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, fromSetup:Bool, add:Bool, out:Bool,
			appliedPose:Bool) {
		var constraint:PhysicsConstraint = null;
		if (constraintIndex != -1) {
			constraint = cast(skeleton.constraints[constraintIndex], PhysicsConstraint);
			if (!constraint.active)
				return;
		}

		var frames:Array<Float> = this.frames;

		if (lastTime > time) { // Apply events after lastTime for looped animations.
			apply(skeleton, lastTime, 2147483647, [], alpha, false, false, false, false);
			lastTime = -1;
		} else if (lastTime >= frames[frames.length - 1]) // Last time is after last frame.
			return;
		if (time < frames[0])
			return;

		if (lastTime < frames[0] || time >= frames[Timeline.search1(frames, lastTime) + 1]) {
			if (constraint != null)
				constraint.reset(skeleton);
			else {
				for (constraint in skeleton.physics) {
					if (constraint.active)
						constraint.reset(skeleton);
				}
			}
		}
	}
}
