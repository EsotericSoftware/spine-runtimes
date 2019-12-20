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

	/** Stores the setup pose for a {@link PathConstraint}.
	 *
	 * See [Path constraints](http://esotericsoftware.com/spine-path-constraints) in the Spine User Guide. */
	export class PathConstraintData extends ConstraintData {

		/** The bones that will be modified by this path constraint. */
		bones = new Array<BoneData>();

		/** The slot whose path attachment will be used to constrained the bones. */
		target: SlotData;

		/** The mode for positioning the first bone on the path. */
		positionMode: PositionMode;

		/** The mode for positioning the bones after the first bone on the path. */
		spacingMode: SpacingMode;

		/** The mode for adjusting the rotation of the bones. */
		rotateMode: RotateMode;

		/** An offset added to the constrained bone rotation. */
		offsetRotation: number;

		/** The position along the path. */
		position: number;

		/** The spacing between bones. */
		spacing: number;

		/** A percentage (0-1) that controls the mix between the constrained and unconstrained rotations. */
		rotateMix: number;

		/** A percentage (0-1) that controls the mix between the constrained and unconstrained translations. */
		translateMix: number;

		constructor (name: string) {
			super(name, 0, false);
		}
	}

	/** Controls how the first bone is positioned along the path.
	 *
	 * See [Position mode](http://esotericsoftware.com/spine-path-constraints#Position-mode) in the Spine User Guide. */
	export enum PositionMode {
		Fixed, Percent
	}

	/** Controls how bones after the first bone are positioned along the path.
	 *
	 * [Spacing mode](http://esotericsoftware.com/spine-path-constraints#Spacing-mode) in the Spine User Guide. */
	export enum SpacingMode {
		Length, Fixed, Percent
	}

	/** Controls how bones are rotated, translated, and scaled to match the path.
	 *
	 * [Rotate mode](http://esotericsoftware.com/spine-path-constraints#Rotate-mod) in the Spine User Guide. */
	export enum RotateMode {
		Tangent, Chain, ChainScale
	}
}
