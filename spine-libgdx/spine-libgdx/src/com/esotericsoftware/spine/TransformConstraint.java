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

package com.esotericsoftware.spine;

import static com.badlogic.gdx.math.MathUtils.*;

import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;

import com.esotericsoftware.spine.Skeleton.Physics;
import com.esotericsoftware.spine.TransformConstraintData.FromProperty;
import com.esotericsoftware.spine.TransformConstraintData.ToProperty;

/** Stores the current pose for a transform constraint. A transform constraint adjusts the world transform of the constrained
 * bones to match that of the source bone.
 * <p>
 * See <a href="https://esotericsoftware.com/spine-transform-constraints">Transform constraints</a> in the Spine User Guide. */
public class TransformConstraint implements Updatable {
	final TransformConstraintData data;
	final Array<Bone> bones;
	Bone source;
	float mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY;

	boolean active;
	final Vector2 temp = new Vector2();

	public TransformConstraint (TransformConstraintData data, Skeleton skeleton) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.data = data;

		bones = new Array(data.bones.size);
		for (BoneData boneData : data.bones)
			bones.add(skeleton.bones.get(boneData.index));

		source = skeleton.bones.get(data.source.index);

		setToSetupPose();
	}

	/** Copy constructor. */
	public TransformConstraint (TransformConstraint constraint, Skeleton skeleton) {
		this(constraint.data, skeleton);
		setToSetupPose();
	}

	public void setToSetupPose () {
		TransformConstraintData data = this.data;
		mixRotate = data.mixRotate;
		mixX = data.mixX;
		mixY = data.mixY;
		mixScaleX = data.mixScaleX;
		mixScaleY = data.mixScaleY;
		mixShearY = data.mixShearY;
	}

	/** Applies the constraint to the constrained bones. */
	public void update (Physics physics) {
		if (mixRotate == 0 && mixX == 0 && mixY == 0 && mixScaleX == 0 && mixScaleY == 0 && mixShearY == 0) return;

		TransformConstraintData data = this.data;
		boolean localFrom = data.localSource, localTarget = data.localTarget, additive = data.additive, clamp = data.clamp;
		Bone source = this.source;
		Object[] fromItems = data.properties.items;
		int fn = data.properties.size;
		Object[] bones = this.bones.items;
		for (int i = 0, n = this.bones.size; i < n; i++) {
			var bone = (Bone)bones[i];
			for (int f = 0; f < fn; f++) {
				var from = (FromProperty)fromItems[f];
				float value = from.value(data, source, localFrom) - from.offset;
				Object[] toItems = from.to.items;
				for (int t = 0, tn = from.to.size; t < tn; t++) {
					var to = (ToProperty)toItems[t];
					if (to.mix(this) != 0) {
						float clamped = to.offset + value * to.scale;
						if (clamp) {
							if (to.offset < to.max)
								clamped = clamp(clamped, to.offset, to.max);
							else
								clamped = clamp(clamped, to.max, to.offset);
						}
						to.apply(this, bone, clamped, localTarget, additive);
					}
				}
			}
			if (localTarget)
				bone.update(null);
			else
				bone.updateAppliedTransform();
		}
	}

	/** The bones that will be modified by this transform constraint. */
	public Array<Bone> getBones () {
		return bones;
	}

	/** The bone whose world transform will be copied to the constrained bones. */
	public Bone getSource () {
		return source;
	}

	public void setSource (Bone source) {
		if (source == null) throw new IllegalArgumentException("source cannot be null.");
		this.source = source;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained rotation. */
	public float getMixRotate () {
		return mixRotate;
	}

	public void setMixRotate (float mixRotate) {
		this.mixRotate = mixRotate;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained translation X. */
	public float getMixX () {
		return mixX;
	}

	public void setMixX (float mixX) {
		this.mixX = mixX;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained translation Y. */
	public float getMixY () {
		return mixY;
	}

	public void setMixY (float mixY) {
		this.mixY = mixY;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained scale X. */
	public float getMixScaleX () {
		return mixScaleX;
	}

	public void setMixScaleX (float mixScaleX) {
		this.mixScaleX = mixScaleX;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained scale X. */
	public float getMixScaleY () {
		return mixScaleY;
	}

	public void setMixScaleY (float mixScaleY) {
		this.mixScaleY = mixScaleY;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained shear Y. */
	public float getMixShearY () {
		return mixShearY;
	}

	public void setMixShearY (float mixShearY) {
		this.mixShearY = mixShearY;
	}

	public boolean isActive () {
		return active;
	}

	/** The transform constraint's setup pose data. */
	public TransformConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
