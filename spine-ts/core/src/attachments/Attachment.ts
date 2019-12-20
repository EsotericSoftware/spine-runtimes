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
	/** The base class for all attachments. */
	export abstract class Attachment {
		name: string;

		constructor (name: string) {
			if (name == null) throw new Error("name cannot be null.");
			this.name = name;
		}

		abstract copy (): Attachment;
	}

	/** Base class for an attachment with vertices that are transformed by one or more bones and can be deformed by a slot's
	 * {@link Slot#deform}. */
	export abstract class VertexAttachment extends Attachment {
		private static nextID = 0;

		/** The unique ID for this attachment. */
		id = (VertexAttachment.nextID++ & 65535) << 11;

		/** The bones which affect the {@link #getVertices()}. The array entries are, for each vertex, the number of bones affecting
		 * the vertex followed by that many bone indices, which is the index of the bone in {@link Skeleton#bones}. Will be null
		 * if this attachment has no weights. */
		bones: Array<number>;

		/** The vertex positions in the bone's coordinate system. For a non-weighted attachment, the values are `x,y`
		 * entries for each vertex. For a weighted attachment, the values are `x,y,weight` entries for each bone affecting
		 * each vertex. */
		vertices: ArrayLike<number>;

		/** The maximum number of world vertex values that can be output by
		 * {@link #computeWorldVertices()} using the `count` parameter. */
		worldVerticesLength = 0;

		/** Deform keys for the deform attachment are also applied to this attachment. May be null if no deform keys should be applied. */
		deformAttachment: VertexAttachment = this;

		constructor (name: string) {
			super(name);
		}

		/** Transforms the attachment's local {@link vertices} to world coordinates. If the slot's {@link Slot#deform} is
		 * not empty, it is used to deform the vertices.
		 *
		 * See [World transforms](http://esotericsoftware.com/spine-runtime-skeletons#World-transforms) in the Spine
		 * Runtimes Guide.
		 * @param start The index of the first {@link #vertices} value to transform. Each vertex has 2 values, x and y.
		 * @param count The number of world vertex values to output. Must be <= {@link #worldVerticesLength} - `start`.
		 * @param worldVertices The output world vertices. Must have a length >= `offset` + `count` *
		 *           `stride` / 2.
		 * @param offset The `worldVertices` index to begin writing values.
		 * @param stride The number of `worldVertices` entries between the value pairs written. */
		computeWorldVertices (slot: Slot, start: number, count: number, worldVertices: ArrayLike<number>, offset: number, stride: number) {
			count = offset + (count >> 1) * stride;
			let skeleton = slot.bone.skeleton;
			let deformArray = slot.deform;
			let vertices = this.vertices;
			let bones = this.bones;
			if (bones == null) {
				if (deformArray.length > 0) vertices = deformArray;
				let bone = slot.bone;
				let x = bone.worldX;
				let y = bone.worldY;
				let a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				for (let v = start, w = offset; w < count; v += 2, w += stride) {
					let vx = vertices[v], vy = vertices[v + 1];
					worldVertices[w] = vx * a + vy * b + x;
					worldVertices[w + 1] = vx * c + vy * d + y;
				}
				return;
			}
			let v = 0, skip = 0;
			for (let i = 0; i < start; i += 2) {
				let n = bones[v];
				v += n + 1;
				skip += n;
			}
			let skeletonBones = skeleton.bones;
			if (deformArray.length == 0) {
				for (let w = offset, b = skip * 3; w < count; w += stride) {
					let wx = 0, wy = 0;
					let n = bones[v++];
					n += v;
					for (; v < n; v++, b += 3) {
						let bone = skeletonBones[bones[v]];
						let vx = vertices[b], vy = vertices[b + 1], weight = vertices[b + 2];
						wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
						wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					}
					worldVertices[w] = wx;
					worldVertices[w + 1] = wy;
				}
			} else {
				let deform = deformArray;
				for (let w = offset, b = skip * 3, f = skip << 1; w < count; w += stride) {
					let wx = 0, wy = 0;
					let n = bones[v++];
					n += v;
					for (; v < n; v++, b += 3, f += 2) {
						let bone = skeletonBones[bones[v]];
						let vx = vertices[b] + deform[f], vy = vertices[b + 1] + deform[f + 1], weight = vertices[b + 2];
						wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
						wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					}
					worldVertices[w] = wx;
					worldVertices[w + 1] = wy;
				}
			}
		}

		/** Does not copy id (generated) or name (set on construction). **/
		copyTo (attachment: VertexAttachment) {
			if (this.bones != null) {
				attachment.bones = new Array<number>(this.bones.length);
				Utils.arrayCopy(this.bones, 0, attachment.bones, 0, this.bones.length);
			} else
				attachment.bones = null;

			if (this.vertices != null) {
				attachment.vertices = Utils.newFloatArray(this.vertices.length);
				Utils.arrayCopy(this.vertices, 0, attachment.vertices, 0, this.vertices.length);
			} else
				attachment.vertices = null;

			attachment.worldVerticesLength = this.worldVerticesLength;
			attachment.deformAttachment = this.deformAttachment;
		}
	}
}
