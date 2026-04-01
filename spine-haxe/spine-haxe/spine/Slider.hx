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

package spine;

import spine.animation.ConstraintTimeline;
import spine.animation.DrawOrderFolderTimeline;
import spine.animation.DrawOrderTimeline;
import spine.animation.PhysicsConstraintTimeline;
import spine.animation.SlotTimeline;

/** Stores the setup pose for a {@link PhysicsConstraint}.
 * <p>
 * See <a href="https://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide. */
class Slider extends Constraint<Slider, SliderData, SliderPose> {
	static private final offsets:Array<Float> = [for (i in 0...6) .0];

	public var bone:Bone;

	public function new(data:SliderData, skeleton:Skeleton) {
		super(data, new SliderPose(), new SliderPose());
		if (skeleton == null)
			throw new SpineException("skeleton cannot be null.");

		if (data.bone != null)
			bone = skeleton.bones[data.bone.index];
	}

	public function copy(skeleton:Skeleton) {
		var copy = new Slider(data, skeleton);
		copy.pose.set(pose);
		return copy;
	}

	public function update(skeleton:Skeleton, physics:Physics) {
		var p = appliedPose;
		if (p.mix == 0)
			return;

		var animation = data.animation;
		if (bone != null) {
			if (!bone.active)
				return;
			if (data.local)
				bone.appliedPose.validateLocalTransform(skeleton);
			p.time = data.offset + (data.property.value(skeleton, bone.appliedPose, data.local, offsets) - data.property.offset) * data.scale;
			if (data.loop)
				p.time = animation.duration + (p.time % animation.duration);
			else
				p.time = Math.max(0, p.time);
		}

		var bones = skeleton.bones;
		var indices = animation.bones;
		var i = 0, n = animation.bones.length;
		while (i < n)
			bones[indices[i++]].appliedPose.modifyLocal(skeleton);

		animation.apply(skeleton, p.time, p.time, data.loop, null, p.mix, false, data.additive, false, true);
	}

	function sort(skeleton:Skeleton) {
		if (bone != null && !data.local)
			skeleton.sortBone(bone);
		skeleton._updateCache.push(this);

		var bones = skeleton.bones;
		var indices = data.animation.bones;
		var i = 0, n = data.animation.bones.length;
		while (i < n) {
			var bone = bones[indices[i++]];
			bone.sorted = false;
			skeleton.sortReset(bone.children);
			skeleton.constrained(bone);
		}

		var timelines = data.animation.timelines;
		var slots = skeleton.slots;
		var constraints = skeleton.constraints;
		var physics = skeleton.physics;
		var physicsCount = skeleton.physics.length;
		var i = 0, n = data.animation.timelines.length;
		while (i < n) {
			var t = timelines[i++];
			if (Std.isOfType(t, SlotTimeline))
				skeleton.constrained(slots[cast(t, SlotTimeline).getSlotIndex()]);
			else if (Std.isOfType(t, DrawOrderTimeline) || Std.isOfType(t, DrawOrderFolderTimeline))
				skeleton.drawOrder.constrained();
			else if (Std.isOfType(t, PhysicsConstraintTimeline)) {
				var timeline = cast(t, PhysicsConstraintTimeline);
				if (timeline.constraintIndex == -1) {
					for (ii in 0...physicsCount)
						skeleton.constrained(physics[ii]);
				} else
					skeleton.constrained(constraints[timeline.constraintIndex]);
			} else if (Std.isOfType(t, ConstraintTimeline)) {
				var constraintIndex = cast(t, ConstraintTimeline).getConstraintIndex();
				if (constraintIndex != -1)
					skeleton.constrained(constraints[constraintIndex]);
			}
		}
	}
}
