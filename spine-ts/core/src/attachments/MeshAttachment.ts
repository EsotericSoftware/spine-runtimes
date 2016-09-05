/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.5
 * 
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine {
	export class MeshAttachment extends VertexAttachment {
		region: TextureRegion;
		path: string;
		regionUVs: ArrayLike<number>; worldVertices: ArrayLike<number>;
		triangles: Array<number>;
		color = new Color(1, 1, 1, 1);
		hullLength: number;
		private parentMesh: MeshAttachment;
		inheritDeform = false;
		tempColor = new Color(0, 0, 0, 0);

		constructor (name: string) {
			super(name);
		}

		updateUVs () {
			let regionUVs = this.regionUVs;
			let verticesLength = regionUVs.length;
			let worldVerticesLength = (verticesLength >> 1) * 8;
			if (this.worldVertices == null || this.worldVertices.length != worldVerticesLength)
				this.worldVertices = Utils.newFloatArray(worldVerticesLength);

			let u = 0, v = 0, width = 0, height = 0;
			if (this.region == null) {
				u = v = 0;
				width = height = 1;
			} else {
				u = this.region.u;
				v = this.region.v;
				width = this.region.u2 - u;
				height = this.region.v2 - v;
			}
			if (this.region.rotate) {
				for (let i = 0, w = 6; i < verticesLength; i += 2, w += 8) {
					this.worldVertices[w] = u + regionUVs[i + 1] * width;
					this.worldVertices[w + 1] = v + height - regionUVs[i] * height;
				}
			} else {
				for (let i = 0, w = 6; i < verticesLength; i += 2, w += 8) {
					this.worldVertices[w] = u + regionUVs[i] * width;
					this.worldVertices[w + 1] = v + regionUVs[i + 1] * height;
				}
			}
		}

		/** @return The updated world vertices. */
		updateWorldVertices (slot: Slot, premultipliedAlpha: boolean) {
			let skeleton = slot.bone.skeleton;
			let skeletonColor = skeleton.color, slotColor = slot.color, meshColor = this.color;
			let alpha = skeletonColor.a * slotColor.a * meshColor.a;
			let multiplier = premultipliedAlpha ? alpha : 1;
			let color = this.tempColor;
			color.set(skeletonColor.r * slotColor.r * meshColor.r * multiplier,
				skeletonColor.g * slotColor.g * meshColor.g * multiplier,
				skeletonColor.b * slotColor.b * meshColor.b * multiplier,
				alpha);

			let x = skeleton.x, y = skeleton.y;
			let deformArray = slot.attachmentVertices;
			let vertices = this.vertices, worldVertices = this.worldVertices;
			let bones = this.bones;
			if (bones == null) {
				let verticesLength = vertices.length;
				if (deformArray.length > 0) vertices = deformArray;
				let bone = slot.bone;
				x += bone.worldX;
				y += bone.worldY;
				let a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				for (let v = 0, w = 0; v < verticesLength; v += 2, w += 8) {
					let vx = vertices[v], vy = vertices[v + 1];
					worldVertices[w] = vx * a + vy * b + x;
					worldVertices[w + 1] = vx * c + vy * d + y;
					worldVertices[w + 2] = color.r;
					worldVertices[w + 3] = color.g;
					worldVertices[w + 4] = color.b;
					worldVertices[w + 5] = color.a;
				}
				return worldVertices;
			}
			let skeletonBones = skeleton.bones;
			if (deformArray.length == 0) {
				for (let w = 0, v = 0, b = 0, n = bones.length; v < n; w += 8) {
					let wx = x, wy = y;
					let nn = bones[v++] + v;
					for (; v < nn; v++, b += 3) {
						let bone = skeletonBones[bones[v]];
						let vx = vertices[b], vy = vertices[b + 1], weight = vertices[b + 2];
						wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
						wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					}
					worldVertices[w] = wx;
					worldVertices[w + 1] = wy;
					worldVertices[w + 2] = color.r;
					worldVertices[w + 3] = color.g;
					worldVertices[w + 4] = color.b;
					worldVertices[w + 5] = color.a;
				}
			} else {
				let deform = deformArray;
				for (let w = 0, v = 0, b = 0, f = 0, n = bones.length; v < n; w += 8) {
					let wx = x, wy = y;
					let nn = bones[v++] + v;
					for (; v < nn; v++, b += 3, f += 2) {
						let bone = skeletonBones[bones[v]];
						let vx = vertices[b] + deform[f], vy = vertices[b + 1] + deform[f + 1], weight = vertices[b + 2];
						wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
						wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
					}
					worldVertices[w] = wx;
					worldVertices[w + 1] = wy;
					worldVertices[w + 2] = color.r;
					worldVertices[w + 3] = color.g;
					worldVertices[w + 4] = color.b;
					worldVertices[w + 5] = color.a;
				}
			}
			return worldVertices;
		}

		applyDeform (sourceAttachment: VertexAttachment): boolean {
			return this == sourceAttachment || (this.inheritDeform && this.parentMesh == sourceAttachment);
		}

		getParentMesh () {
			return this.parentMesh;
		}

		/** @param parentMesh May be null. */
		setParentMesh (parentMesh: MeshAttachment) {
			this.parentMesh = parentMesh;
			if (parentMesh != null) {
				this.bones = parentMesh.bones;
				this.vertices = parentMesh.vertices;
				this.regionUVs = parentMesh.regionUVs;
				this.triangles = parentMesh.triangles;
				this.hullLength = parentMesh.hullLength;
			}
		}
	}

}
