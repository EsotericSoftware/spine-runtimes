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

import spine.Event;
import spine.PathConstraint;
import spine.Skeleton;

/** The base class for most spine.PhysicsConstraint timelines. */
abstract class PhysicsConstraintTimeline extends ConstraintTimeline1 {
	/**
	 * @param constraintIndex -1 for all physics constraints in the skeleton.
	 */
	public function new(frameCount:Int, bezierCount:Int, constraintIndex:Int, property:Property) {
		super(frameCount, bezierCount, constraintIndex, property);
	}

	public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, fromSetup:Bool, add:Bool, out:Bool,
			appliedPose:Bool) {
		if (add && !this.additive)
			add = false;
		if (constraintIndex == -1) {
			var value:Float = time >= frames[0] ? getCurveValue(time) : 0;
			for (constraint in skeleton.physics) {
				if (constraint.active && global(constraint.data)) {
					var pose = appliedPose ? constraint.appliedPose : constraint.pose;
					set(pose, getAbsoluteValue2(time, alpha, fromSetup, add, get(pose), get(constraint.data.setupPose), value));
				}
			}
		} else {
			var constraint = cast(skeleton.constraints[constraintIndex], PhysicsConstraint);
			if (constraint.active) {
				var pose = appliedPose ? constraint.appliedPose : constraint.pose;
				set(pose, getAbsoluteValue(time, alpha, fromSetup, add, get(pose), get(constraint.data.setupPose)));
			}
		}
	}

	abstract public function get(pose:PhysicsConstraintPose):Float;

	abstract public function set(pose:PhysicsConstraintPose, value:Float):Void;

	abstract public function global(constraint:PhysicsConstraintData):Bool;
}
