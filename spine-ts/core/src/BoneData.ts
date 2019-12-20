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

	/** Stores the setup pose for a {@link Bone}. */
	export class BoneData {
		/** The index of the bone in {@link Skeleton#getBones()}. */
		index: number;

		/** The name of the bone, which is unique across all bones in the skeleton. */
		name: string;

		/** @returns May be null. */
		parent: BoneData;

		/** The bone's length. */
		length: number;

		/** The local x translation. */
		x = 0;

		/** The local y translation. */
		y = 0;

		/** The local rotation. */
		rotation = 0;

		/** The local scaleX. */
		scaleX = 1;

		/** The local scaleY. */
		scaleY = 1;

		/** The local shearX. */
		shearX = 0;

		/** The local shearX. */
		shearY = 0;

		/** The transform mode for how parent world transforms affect this bone. */
		transformMode = TransformMode.Normal;

		/** When true, {@link Skeleton#updateWorldTransform()} only updates this bone if the {@link Skeleton#skin} contains this
	 	* bone.
	 	* @see Skin#bones */
		skinRequired = false;

		/** The color of the bone as it was in Spine. Available only when nonessential data was exported. Bones are not usually
		 * rendered at runtime. */
		color = new Color();

		constructor (index: number, name: string, parent: BoneData) {
			if (index < 0) throw new Error("index must be >= 0.");
			if (name == null) throw new Error("name cannot be null.");
			this.index = index;
			this.name = name;
			this.parent = parent;
		}
	}

	/** Determines how a bone inherits world transforms from parent bones. */
	export enum TransformMode {
		Normal, OnlyTranslation, NoRotationOrReflection, NoScale, NoScaleOrReflection
	}
}
