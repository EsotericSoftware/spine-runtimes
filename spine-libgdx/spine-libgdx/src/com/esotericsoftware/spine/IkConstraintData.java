/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;

/** Stores the setup pose for an {@link IkConstraint}.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-ik-constraints">IK constraints</a> in the Spine User Guide. */
public class IkConstraintData extends ConstraintData {
	final Array<BoneData> bones = new Array();
	BoneData target;
	int bendDirection = 1;
	boolean compress, stretch, uniform;
	float mix = 1, softness;

	public IkConstraintData (String name) {
		super(name);
	}

	/** The bones that are constrained by this IK constraint. */
	public Array<BoneData> getBones () {
		return bones;
	}

	/** The bone that is the IK target. */
	public BoneData getTarget () {
		return target;
	}

	public void setTarget (BoneData target) {
		if (target == null) throw new IllegalArgumentException("target cannot be null.");
		this.target = target;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained rotations. */
	public float getMix () {
		return mix;
	}

	public void setMix (float mix) {
		this.mix = mix;
	}

	/** For two bone IK, the distance from the maximum reach of the bones that rotation will slow. */
	public float getSoftness () {
		return softness;
	}

	public void setSoftness (float softness) {
		this.softness = softness;
	}

	/** Controls the bend direction of the IK bones, either 1 or -1. */
	public int getBendDirection () {
		return bendDirection;
	}

	public void setBendDirection (int bendDirection) {
		this.bendDirection = bendDirection;
	}

	/** When true and only a single bone is being constrained, if the target is too close, the bone is scaled to reach it. */
	public boolean getCompress () {
		return compress;
	}

	public void setCompress (boolean compress) {
		this.compress = compress;
	}

	/** When true, if the target is out of range, the parent bone is scaled to reach it. If more than one bone is being constrained
	 * and the parent bone has local nonuniform scale, stretch is not applied. */
	public boolean getStretch () {
		return stretch;
	}

	public void setStretch (boolean stretch) {
		this.stretch = stretch;
	}

	/** When true, only a single bone is being constrained, and {@link #getCompress()} or {@link #getStretch()} is used, the bone
	 * is scaled on both the X and Y axes. */
	public boolean getUniform () {
		return uniform;
	}

	public void setUniform (boolean uniform) {
		this.uniform = uniform;
	}
}
