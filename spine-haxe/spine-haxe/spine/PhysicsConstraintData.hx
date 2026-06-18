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

import spine.ConstraintData.ScaleYMode;

/** Stores the setup pose for a PhysicsConstraint.
 *
 * @see https://esotericsoftware.com/spine-physics-constraints Physics constraints in the Spine User Guide */
class PhysicsConstraintData extends ConstraintData<PhysicsConstraint, PhysicsConstraintPose> {
	/** The bone constrained by this physics constraint. */
	public var bone:BoneData;

	public var x = 0.;
	public var y = 0.;
	public var rotate = 0.;
	public var scaleX = 0.;
	public var shearX = 0.;
	public var limit = 0.;
	public var step = 0.;

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained poses. */
	public var mix = 0.;

	public var inertiaGlobal = false;
	public var strengthGlobal = false;
	public var dampingGlobal = false;
	public var massGlobal = false;
	public var windGlobal = false;
	public var gravityGlobal = false;
	public var mixGlobal = false;

	/** Determines how BonePose.scaleY changes when PhysicsConstraintData.scaleX sets BonePose.scaleX. */
	public var scaleYMode(default, set):ScaleYMode = ScaleYMode.none;

	public function new(name:String) {
		super(name, new PhysicsConstraintPose());
	}

	public function create(skeleton:Skeleton) {
		return new PhysicsConstraint(this, skeleton);
	}

	public function set_scaleYMode(scaleYMode:ScaleYMode) {
		if (scaleYMode == null)
			throw new SpineException("scaleYMode cannot be null.");
		this.scaleYMode = scaleYMode;
		return scaleYMode;
	}
}
