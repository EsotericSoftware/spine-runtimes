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

import type { SlotPose } from "../SlotPose.js";
import type { Slot } from "../Slot.js";
import type { TextureRegion } from "../Texture.js";
import { Color, MathUtils, type NumberArrayLike } from "../Utils.js";
import { Attachment } from "./Attachment.js";
import type { HasSequence } from "./HasSequence.js";
import type { Sequence } from "./Sequence.js";

/** An attachment that displays a textured quadrilateral.
 *
 * See [Region attachments](http://esotericsoftware.com/spine-regions) in the Spine User Guide. */
export class RegionAttachment extends Attachment implements HasSequence {
	readonly sequence: Sequence;

	/** The local x translation. */
	x = 0;

	/** The local y translation. */
	y = 0;

	/** The local scaleX. */
	scaleX = 1;

	/** The local scaleY. */
	scaleY = 1;

	/** The local rotation in degrees, counter clockwise. */
	rotation = 0;

	/** The width of the region attachment in Spine. */
	width = 0;

	/** The height of the region attachment in Spine. */
	height = 0;

	/** The name of the texture region for this attachment. */
	path?: string;

	/** The color to tint the region attachment. */
	color = new Color(1, 1, 1, 1);

	tempColor = new Color(1, 1, 1, 1);

	constructor (name: string, sequence: Sequence) {
		super(name);
		this.sequence = sequence;
	}

	copy (): Attachment {
		const copy = new RegionAttachment(this.name, this.sequence.copy());
		copy.path = this.path;
		copy.x = this.x;
		copy.y = this.y;
		copy.scaleX = this.scaleX;
		copy.scaleY = this.scaleY;
		copy.rotation = this.rotation;
		copy.width = this.width;
		copy.height = this.height;
		copy.color.setFromColor(this.color);
		return copy;
	}

	/** Transforms the attachment's four vertices to world coordinates.
	 *
	 * See <a href="http://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
	 * Runtimes Guide.
	 * @param worldVertices The output world vertices. Must have a length >= `offset` + 8.
	 * @param offset The `worldVertices` index to begin writing values.
	 * @param stride The number of `worldVertices` entries between the value pairs written. */
	computeWorldVertices (slot: Slot, vertexOffsets: NumberArrayLike, worldVertices: NumberArrayLike, offset: number, stride: number) {

		const bone = slot.bone.appliedPose;
		const x = bone.worldX, y = bone.worldY;
		const a = bone.a, b = bone.b, c = bone.c, d = bone.d;

		let offsetX = vertexOffsets[0];
		let offsetY = vertexOffsets[1];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // br
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = vertexOffsets[2];
		offsetY = vertexOffsets[3];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // bl
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = vertexOffsets[4];
		offsetY = vertexOffsets[5];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // ul
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = vertexOffsets[6];
		offsetY = vertexOffsets[7];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // ur
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
	}

	getOffsets (pose: SlotPose): number[] {
		// biome-ignore lint/style/noNonNullAssertion: offsets are always defined after updateSequence
		return this.sequence.offsets![this.sequence.resolveIndex(pose)];
	}

	updateSequence () {
		this.sequence.update(this);
	}

	/** Computes {@link Sequence.getUVs | UVs} and {@link Sequence.getOffsets | offsets} for a region attachment.
	 * @param uvs Output array for the computed UVs, length of 8.
	 * @param offset Output array for the computed vertex offsets, length of 8. */
	static computeUVs (region: TextureRegion | null, x: number, y: number, scaleX: number, scaleY: number, rotation: number, width: number,
		height: number, offset: number[], uvs: NumberArrayLike): void {

		if (!region) throw new Error("Region not set.");
		const regionScaleX = width / region.originalWidth * scaleX;
		const regionScaleY = height / region.originalHeight * scaleY;
		const localX = -width / 2 * scaleX + region.offsetX * regionScaleX;
		const localY = -height / 2 * scaleY + region.offsetY * regionScaleY;
		const localX2 = localX + region.width * regionScaleX;
		const localY2 = localY + region.height * regionScaleY;
		const radians = rotation * MathUtils.degRad;
		const cos = Math.cos(radians);
		const sin = Math.sin(radians);
		const localXCos = localX * cos + x;
		const localXSin = localX * sin;
		const localYCos = localY * cos + y;
		const localYSin = localY * sin;
		const localX2Cos = localX2 * cos + x;
		const localX2Sin = localX2 * sin;
		const localY2Cos = localY2 * cos + y;
		const localY2Sin = localY2 * sin;
		offset[0] = localXCos - localYSin;
		offset[1] = localYCos + localXSin;
		offset[2] = localXCos - localY2Sin;
		offset[3] = localY2Cos + localXSin;
		offset[4] = localX2Cos - localY2Sin;
		offset[5] = localY2Cos + localX2Sin;
		offset[6] = localX2Cos - localYSin;
		offset[7] = localYCos + localX2Sin;

		if (region == null) {
			uvs[0] = 0;
			uvs[1] = 0;
			uvs[2] = 0;
			uvs[3] = 1;
			uvs[4] = 1;
			uvs[5] = 1;
			uvs[6] = 1;
			uvs[7] = 0;
		} else {
			uvs[1] = region.v2;
			uvs[2] = region.u;
			uvs[5] = region.v;
			uvs[6] = region.u2;
			if (region.degrees === 90) {
				uvs[0] = region.u2;
				uvs[3] = region.v2;
				uvs[4] = region.u;
				uvs[7] = region.v;
			} else {
				uvs[0] = region.u;
				uvs[3] = region.v;
				uvs[4] = region.u2;
				uvs[7] = region.v2;
			}
		}
	}

	static X1 = 0;
	static Y1 = 1;
	static C1R = 2;
	static C1G = 3;
	static C1B = 4;
	static C1A = 5;
	static U1 = 6;
	static V1 = 7;

	static X2 = 8;
	static Y2 = 9;
	static C2R = 10;
	static C2G = 11;
	static C2B = 12;
	static C2A = 13;
	static U2 = 14;
	static V2 = 15;

	static X3 = 16;
	static Y3 = 17;
	static C3R = 18;
	static C3G = 19;
	static C3B = 20;
	static C3A = 21;
	static U3 = 22;
	static V3 = 23;

	static X4 = 24;
	static Y4 = 25;
	static C4R = 26;
	static C4G = 27;
	static C4B = 28;
	static C4A = 29;
	static U4 = 30;
	static V4 = 31;
}
