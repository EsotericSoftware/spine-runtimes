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

import spine.animation.Timeline;
import spine.Event;
import spine.Skeleton;

class PhysicsConstraintResetTimeline extends Timeline {
	/** The index of the physics constraint in {@link Skeleton#physicsConstraints} that will be reset when this timeline is
	* applied, or -1 if all physics constraints in the skeleton will be reset. */
	public var constraintIndex:Int = 0;

	public function new(frameCount:Int, physicsConstraintIndex:Int) {
		propertyIds = [Std.string(Property.physicsConstraintReset)];
		super(frameCount, propertyIds);
		constraintIndex = physicsConstraintIndex;
	}

	public override function getFrameCount():Int {
		return frames.length;
	}

	/** Sets the time in seconds and the event for the specified key frame. */
	public function setFrame(frame:Int, time:Float):Void {
		frames[frame] = time;
	}

	/** Resets the physics constraint when frames > lastTime and <= time. */
	public override function apply(skeleton:Skeleton, lastTime:Float, time:Float, firedEvents:Array<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		var constraint:PhysicsConstraint = null;
		if (this.constraintIndex != -1) {
			constraint = skeleton.physicsConstraints[constraintIndex];
			if (!constraint.active) return;
		}

		var frames:Array<Float> = this.frames;
		if (lastTime > time) // Apply events after lastTime for looped animations.
		{
			apply(skeleton, lastTime, 2147483647, [], alpha, blend, direction);
			lastTime = -1;
		} else if (lastTime >= frames[frames.length - 1]) // Last time is after last frame.
		{
			return;
		}
		if (time < frames[0]) return;

		if (lastTime < frames[0] || time >= frames[Timeline.search1(frames, lastTime) + 1]) {
			if (constraint != null)
				constraint.reset();
			else {
				for (constraint in skeleton.physicsConstraints) {
					if (constraint.active) constraint.reset();
				}
			}
		}
	}
}
