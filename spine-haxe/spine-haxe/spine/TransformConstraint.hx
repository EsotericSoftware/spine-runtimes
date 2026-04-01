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

/** Stores the current pose for a transform constraint. A transform constraint adjusts the world transform of the constrained
 * bones to match that of the target bone.
 *
 * @see https://esotericsoftware.com/spine-transform-constraints Transform constraints in the Spine User Guide */
class TransformConstraint extends Constraint<TransformConstraint, TransformConstraintData, TransformConstraintPose> {
	/** The bones that will be modified by this transform constraint. */
	public final bones:Array<BonePose>;

	/** The target bone whose world transform will be copied to the constrained bones. */
	public var source:Bone;

	public function new(data:TransformConstraintData, skeleton:Skeleton) {
		super(data, new TransformConstraintPose(), new TransformConstraintPose());
		if (skeleton == null)
			throw new SpineException("skeleton cannot be null.");

		bones = new Array<BonePose>();
		for (boneData in data.bones)
			bones.push(skeleton.bones[boneData.index].constrainedPose);
		source = skeleton.bones[data.source.index];
	}

	public function copy(skeleton:Skeleton) {
		var copy = new TransformConstraint(data, skeleton);
		copy.pose.set(pose);
		return copy;
	}

	/** Applies the constraint to the constrained bones. */
	public function update(skeleton:Skeleton, physics:Physics):Void {
		var p = appliedPose;
		if (p.mixRotate == 0 && p.mixX == 0 && p.mixY == 0 && p.mixScaleX == 0 && p.mixScaleY == 0 && p.mixShearY == 0)
			return;

		var localSource = data.localSource,
			localTarget = data.localTarget,
			additive = data.additive,
			clamp = data.clamp;
		var offsets = data.offsets;
		var source = this.source.appliedPose;
		if (localSource)
			source.validateLocalTransform(skeleton);
		var fromItems = data.properties;
		var fn = data.properties.length, update = skeleton.update;
		var bones = this.bones;
		var i = 0, n = this.bones.length;
		var update = skeleton._update;
		while (i < n) {
			var bone = bones[i];
			if (localTarget)
				bone.modifyLocal(skeleton);
			else
				bone.modifyWorld(update);
			var f = 0;
			while (f < fn) {
				var from = fromItems[f];
				var value = from.value(skeleton, source, localSource, offsets) - from.offset;
				var toItems = from.to;
				var t = 0, tn = from.to.length;
				while (t < tn) {
					var to = toItems[t];
					if (to.mix(p) != 0) {
						var clamped = to.offset + value * to.scale;
						if (clamp) {
							if (to.offset < to.max)
								clamped = MathUtils.clamp(clamped, to.offset, to.max);
							else
								clamped = MathUtils.clamp(clamped, to.max, to.offset);
						}
						to.apply(skeleton, p, bone, clamped, localTarget, additive);
					}
					t++;
				}
				f++;
			}
			i++;
		}
	}

	public function sort(skeleton:Skeleton) {
		if (!data.localSource)
			skeleton.sortBone(source);
		var bones = this.bones;
		var boneCount = this.bones.length;
		var worldTarget = !data.localTarget;
		if (worldTarget) {
			for (i in 0...boneCount)
				skeleton.sortBone(bones[i].bone);
		}
		skeleton._updateCache.push(this);
		for (i in 0...boneCount) {
			var bone = bones[i].bone;
			skeleton.sortReset(bone.children);
			skeleton.constrained(bone);
		}
		for (i in 0...boneCount)
			bones[i].bone.sorted = worldTarget;
	}

	override public function isSourceActive() {
		return source.active;
	}
}
