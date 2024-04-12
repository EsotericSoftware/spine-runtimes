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
import spine.PathConstraint;
import spine.Skeleton;

/** The base class for most {@link PhysicsConstraint} timelines. */
abstract class PhysicsConstraintTimeline extends CurveTimeline1 {
	/** The index of the physics constraint in {@link Skeleton#getPhysicsConstraints()} that will be changed when this timeline
	 * is applied, or -1 if all physics constraints in the skeleton will be changed. */
	public var constraintIndex:Int = 0;

	/** @param physicsConstraintIndex -1 for all physics constraints in the skeleton. */
	public function new(frameCount:Int, bezierCount:Int, physicsConstraintIndex:Int, property:Int) {
		super(frameCount, bezierCount, [property + "|" + physicsConstraintIndex]);
		constraintIndex = physicsConstraintIndex;
	}

	public override function apply (skeleton:Skeleton, lastTime:Float, time:Float, firedEvents:Array<Event>, alpha:Float, blend:MixBlend, direction:MixDirection):Void {
		var constraint:PhysicsConstraint;
		if (constraintIndex == -1) {
			var value:Float = time >= frames[0] ? getCurveValue(time) : 0;

			for (constraint in skeleton.physicsConstraints) {
				if (constraint.active && global(constraint.data))
					set(constraint, getAbsoluteValue2(time, alpha, blend, get(constraint), setup(constraint), value));
			}
		} else {
			constraint = skeleton.physicsConstraints[constraintIndex];
			if (constraint.active) set(constraint, getAbsoluteValue(time, alpha, blend, get(constraint), setup(constraint)));
		}
	}

	abstract public function setup (constraint: PhysicsConstraint):Float;

	abstract public function get (constraint: PhysicsConstraint):Float;

	abstract public function set (constraint: PhysicsConstraint, value:Float):Void;

	abstract public function global (constraint: PhysicsConstraintData):Bool;
}
