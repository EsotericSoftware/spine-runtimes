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

/** Stores the setup pose for a spine.IkConstraint.
 *
 * @see https://esotericsoftware.com/spine-ik-constraints IK constraints in the Spine User Guide */
class IkConstraintData extends ConstraintData<IkConstraint, IkConstraintPose> {
	/** The bones that are constrained by this IK constraint. */
	public final bones:Array<BoneData> = new Array<BoneData>();

	/** The bone that is the IK target. */
	public var target(default, set):BoneData;

	/** Determines how BonePose.scaleY changes when IkConstraintPose.compress or IkConstraintPose.stretch set BonePose.scaleX. */
	public var scaleYMode:ScaleYMode = ScaleYMode.none;

	public function new(name:String) {
		super(name, new IkConstraintPose());
	}

	public function create(skeleton:Skeleton):IkConstraint {
		return new IkConstraint(this, skeleton);
	}

	public function set_target(target:BoneData) {
		if (target == null)
			throw new SpineException("target cannot be null.");
		this.target = target;
		return target;
	}
}
