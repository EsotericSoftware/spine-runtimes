/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

module spine {

	/** Stores the setup pose for an {@link IkConstraint}.
	 * <p>
	 * See [IK constraints](http://esotericsoftware.com/spine-ik-constraints) in the Spine User Guide. */
	export class IkConstraintData extends ConstraintData {
		/** The bones that are constrained by this IK constraint. */
		bones = new Array<BoneData>();

		/** The bone that is the IK target. */
		target: BoneData;

		/** Controls the bend direction of the IK bones, either 1 or -1. */
		bendDirection = 1;

		/** When true and only a single bone is being constrained, if the target is too close, the bone is scaled to reach it. */
		compress = false;

		/** When true, if the target is out of range, the parent bone is scaled to reach it. If more than one bone is being constrained
		 * and the parent bone has local nonuniform scale, stretch is not applied. */
		stretch = false;

		/** When true, only a single bone is being constrained, and {@link #getCompress()} or {@link #getStretch()} is used, the bone
		 * is scaled on both the X and Y axes. */
		uniform = false;

		/** A percentage (0-1) that controls the mix between the constrained and unconstrained rotations. */
		mix = 1;

		/** For two bone IK, the distance from the maximum reach of the bones that rotation will slow. */
		softness = 0;

		constructor (name: string) {
			super(name, 0, false);
		}
	}
}
