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

import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.utils.Array;

/** Stores the skeleton's draw order, which is the order that each slot's attachment is rendered. */
public class DrawOrder {
	final Array<Slot> setupPose, pose, constrainedPose;
	Array<Slot> appliedPose;

	DrawOrder (Array<Slot> setupPose) {
		this.setupPose = setupPose;
		pose = new Array(setupPose);
		constrainedPose = new Array(true, 0, Slot[]::new);
		appliedPose = pose;
	}

	/** Sets the unconstrained draw order to the setup pose order. */
	public void setupPose () {
		arraycopy(setupPose.items, 0, pose.setSize(setupPose.size), 0, setupPose.size);
	}

	/** The unconstrained draw order, set by animations and application code. */
	public Array<Slot> getPose () {
		return pose;
	}

	/** The constrained draw order for rendering. If no constraints modify the draw order, this is the same as {@link #pose}.
	 * Otherwise it is a copy of {@link #pose} modified by constraints. */
	public Array<Slot> getAppliedPose () {
		return appliedPose;
	}

	/** Sets the applied pose to the unconstrained pose, for when no constraints will modify the draw order. */
	void unconstrained () {
		appliedPose = pose;
	}

	/** Sets the applied pose to the constrained pose, in anticipation of the applied pose being modified by constraints. */
	void constrained () {
		appliedPose = constrainedPose;
	}

	/** Copies the unconstrained pose to the constrained pose, as a starting point for constraints to be applied. */
	void reset () { // Port: resetConstrained
		arraycopy(pose.items, 0, constrainedPose.setSize(pose.size), 0, pose.size);
	}
}
