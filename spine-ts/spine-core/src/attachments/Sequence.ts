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

import type { SlotPose } from "src/SlotPose.js";
import type { TextureRegion } from "../Texture.js";
import { type NumberArrayLike, Utils } from "../Utils.js";
import type { HasSequence } from "./HasSequence.js";
import { MeshAttachment } from "./MeshAttachment.js";
import { RegionAttachment } from "./RegionAttachment.js";

/** Holds texture regions, UVs, and vertex offsets for rendering a region or mesh attachment. {@link #getRegions() Regions} must
 * be populated and {@link #update(HasSequence)} called before use. */
export class Sequence {
	private static _nextID = 0;

	id = Sequence.nextID();
	regions: Array<TextureRegion | null>;
	readonly pathSuffix: boolean;
	uvs?: NumberArrayLike[];

	/** Returns vertex offsets from the center of a {@link RegionAttachment}. Invalid to call for a {@link MeshAttachment}. */
	offsets?: number[][];

	start = 0;
	digits = 0;
	/** The index of the region to show for the setup pose. */
	setupIndex = 0;

	constructor (count: number, pathSuffix: boolean) {
		this.regions = new Array<TextureRegion>(count);
		this.pathSuffix = pathSuffix;
	}

	copy (): Sequence {
		const regionCount = this.regions.length;
		const copy = new Sequence(regionCount, this.pathSuffix);
		Utils.arrayCopy(this.regions, 0, copy.regions, 0, regionCount);
		copy.start = this.start;
		copy.digits = this.digits;
		copy.setupIndex = this.setupIndex;

		if (this.uvs != null) {
			const length = this.uvs[0].length;
			copy.uvs = [];
			for (let i = 0; i < regionCount; i++) {
				copy.uvs[i] = Utils.newFloatArray(length);
				Utils.arrayCopy(this.uvs[i], 0, copy.uvs[i], 0, length);
			}
		}
		if (this.offsets != null) {
			copy.offsets = [];
			for (let i = 0; i < regionCount; i++) {
				copy.offsets[i] = [];
				Utils.arrayCopy(this.offsets[i], 0, copy.offsets[i], 0, 8);
			}
		}

		return copy;
	}

	/** Computes UVs and offsets for the specified attachment. Must be called if the regions or attachment properties are
	  * changed. */
	public update (attachment: HasSequence) {
		const regionCount = this.regions.length;
		if (attachment instanceof RegionAttachment) {
			this.uvs = [];
			this.offsets = [];
			for (let i = 0; i < regionCount; i++) {
				this.uvs[i] = Utils.newFloatArray(8);
				this.offsets[i] = [];
				RegionAttachment.computeUVs(this.regions[i], attachment.x, attachment.y, attachment.scaleX, attachment.scaleY, attachment.rotation,
					attachment.width, attachment.height, this.offsets[i], this.uvs[i]);
			}
		} else if (attachment instanceof MeshAttachment) {
			const regionUVs = attachment.regionUVs;
			this.uvs = [];
			this.offsets = undefined;
			for (let i = 0; i < regionCount; i++) {
				this.uvs[i] = Utils.newFloatArray(regionUVs.length);
				MeshAttachment.computeUVs(this.regions[i], regionUVs, this.uvs[i]);
			}
		}
	}

	resolveIndex (pose: SlotPose): number {
		let index = pose.sequenceIndex;
		if (index === -1) index = this.setupIndex;
		if (index >= this.regions.length) index = this.regions.length - 1;
		return index;
	}

	getUVs (index: number): Float32Array {
		// biome-ignore lint/style/noNonNullAssertion: uvs are always defined after updateSequence
		return this.uvs![index] as Float32Array;
	}

	public hasPathSuffix (): boolean {
		return this.pathSuffix;
	}

	getPath (basePath: string, index: number): string {
		if (!this.pathSuffix) return basePath;
		let result = basePath;
		const frame = (this.start + index).toString();
		for (let i = this.digits - frame.length; i > 0; i--)
			result += "0";
		result += frame;
		return result;
	}

	private static nextID (): number {
		return Sequence._nextID++;
	}
}

export enum SequenceMode {
	hold = 0,
	once = 1,
	loop = 2,
	pingpong = 3,
	onceReverse = 4,
	loopReverse = 5,
	pingpongReverse = 6
}

export const SequenceModeValues = [
	SequenceMode.hold,
	SequenceMode.once,
	SequenceMode.loop,
	SequenceMode.pingpong,
	SequenceMode.onceReverse,
	SequenceMode.loopReverse,
	SequenceMode.pingpongReverse
];
