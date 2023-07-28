/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

import com.badlogic.gdx.utils.Array;

/** Stores the current pose for a physics constraint. A physics constraint applies physics to bones.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide. */
public class PhysicsConstraint implements Updatable {
	final PhysicsConstraintData data;
	final Array<Bone> bones;
	float mix;

	boolean active;

	final Skeleton skeleton;
	float remaining, lastTime;

	public PhysicsConstraint (PhysicsConstraintData data, Skeleton skeleton) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.data = data;
		this.skeleton = skeleton;

		bones = new Array(data.bones.size);
		for (BoneData boneData : data.bones)
			bones.add(skeleton.bones.get(boneData.index));

		mix = data.mix;
	}

	/** Copy constructor. */
	public PhysicsConstraint (PhysicsConstraint constraint) {
		if (constraint == null) throw new IllegalArgumentException("constraint cannot be null.");
		data = constraint.data;
		skeleton = constraint.skeleton;
		bones = new Array(constraint.bones);
		mix = constraint.mix;
	}

	public void setToSetupPose () {
		remaining = 0;
		lastTime = skeleton.time;

		PhysicsConstraintData data = this.data;
		mix = data.mix;
	}

	/** Applies the constraint to the constrained bones. */
	public void update () {
		if (mix == 0) return;

		// BOZO
	}

	public boolean isActive () {
		return active;
	}

	/** The physics constraint's setup pose data. */
	public PhysicsConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
