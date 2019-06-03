/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine {
	export abstract class Attachment {
		name: string;

		constructor (name: string) {
			if (name == null) throw new Error("name cannot be null.");
			this.name = name;
		}

		abstract copy (): Attachment;
	}

	export abstract class VertexAttachment extends Attachment {
		private static nextID = 0;

		id = (VertexAttachment.nextID++ & 65535) << 11;
		bones: Array<number>;
		vertices: ArrayLike<number>;
		worldVerticesLength = 0;
		deformAttachment: VertexAttachment = this;

		constructor (name: string) {
			super(name);
		}

		/** Transforms local vertices to world coordinates.
		 * @param start The index of the first local vertex value to transform. Each vertex has 2 values, x and y.
		 * @param count The number of world vertex values to output. Must be <= {@link #getWorldVerticesLength()} - start.
		 * @param worldVertices The output world vertices. Must have a length >= offset + count.
		 * @param offset The worldVertices index to begin writing values. */
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
