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

/** Stores a pose for a transform constraint. */
public class TransformConstraintPose implements Pose<TransformConstraintPose> {
	float mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY;

	public void set (TransformConstraintPose pose) {
		mixRotate = pose.mixRotate;
		mixX = pose.mixX;
		mixY = pose.mixY;
		mixScaleX = pose.mixScaleX;
		mixScaleY = pose.mixScaleY;
		mixShearY = pose.mixShearY;
	}

	/** A percentage that controls the mix between the constrained and unconstrained rotation. */
	public float getMixRotate () {
		return mixRotate;
	}

	public void setMixRotate (float mixRotate) {
		this.mixRotate = mixRotate;
	}

	/** A percentage that controls the mix between the constrained and unconstrained translation X. */
	public float getMixX () {
		return mixX;
	}

	public void setMixX (float mixX) {
		this.mixX = mixX;
	}

	/** A percentage that controls the mix between the constrained and unconstrained translation Y. */
	public float getMixY () {
		return mixY;
	}

	public void setMixY (float mixY) {
		this.mixY = mixY;
	}

	/** A percentage that controls the mix between the constrained and unconstrained scale X. */
	public float getMixScaleX () {
		return mixScaleX;
	}

	public void setMixScaleX (float mixScaleX) {
		this.mixScaleX = mixScaleX;
	}

	/** A percentage that controls the mix between the constrained and unconstrained scale X. */
	public float getMixScaleY () {
		return mixScaleY;
	}

	public void setMixScaleY (float mixScaleY) {
		this.mixScaleY = mixScaleY;
	}

	/** A percentage that controls the mix between the constrained and unconstrained shear Y. */
	public float getMixShearY () {
		return mixShearY;
	}

	public void setMixShearY (float mixShearY) {
		this.mixShearY = mixShearY;
	}
}
