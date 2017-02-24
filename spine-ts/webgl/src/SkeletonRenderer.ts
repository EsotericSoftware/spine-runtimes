/******************************************************************************
 * Spine Runtimes Software License v2.5
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

module spine.webgl {
	class Renderable {
		constructor(public vertices: ArrayLike<number>, public numFloats: number) {}
	};

	export class SkeletonRenderer {
		static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];

		premultipliedAlpha = false;
		private gl: WebGLRenderingContext;
		private tempColor = new Color();
		private tempColor2 = new Color();
		private vertices:ArrayLike<number>;
		private vertexSize = 2 + 2 + 4;
		private twoColorTint = false;
		private renderable: Renderable = new Renderable(null, 0);

		constructor (gl: WebGLRenderingContext, twoColorTint: boolean = true) {
			this.gl = gl;
			this.twoColorTint = twoColorTint;
			if (twoColorTint)
				this.vertexSize += 4;
			this.vertices = Utils.newFloatArray(this.vertexSize * 1024);
		}

		draw (batcher: PolygonBatcher, skeleton: Skeleton) {
			let premultipliedAlpha = this.premultipliedAlpha;
			let blendMode: BlendMode = null;

			let vertices: Renderable = null;
			let triangles: Array<number> = null;
			let drawOrder = skeleton.drawOrder;
			for (let i = 0, n = drawOrder.length; i < n; i++) {
				let slot = drawOrder[i];
				let attachment = slot.getAttachment();
				let texture: GLTexture = null;
				if (attachment instanceof RegionAttachment) {
					let region = <RegionAttachment>attachment;
					vertices = this.computeRegionVertices(slot, region, premultipliedAlpha, this.twoColorTint);
					triangles = SkeletonRenderer.QUAD_TRIANGLES;
					texture = <GLTexture>(<TextureAtlasRegion>region.region.renderObject).texture;

				} else if (attachment instanceof MeshAttachment) {
					let mesh = <MeshAttachment>attachment;
					vertices = this.computeMeshVertices(slot, mesh, premultipliedAlpha, this.twoColorTint);
					triangles = mesh.triangles;
					texture = <GLTexture>(<TextureAtlasRegion>mesh.region.renderObject).texture;
				} else continue;

				if (texture != null) {
					let slotBlendMode = slot.data.blendMode;
					if (slotBlendMode != blendMode) {
						blendMode = slotBlendMode;
						batcher.setBlendMode(getSourceGLBlendMode(this.gl, blendMode, premultipliedAlpha), getDestGLBlendMode(this.gl, blendMode));
					}

					let view = (vertices.vertices as Float32Array).subarray(0, vertices.numFloats);
					batcher.draw(texture, view, triangles);
				}
			}
		}

		private computeRegionVertices(slot: Slot, region: RegionAttachment, pma: boolean, twoColorTint: boolean = false) {
			let skeleton = slot.bone.skeleton;
			let skeletonColor = skeleton.color;
			let slotColor = slot.color;
			let regionColor = region.color;
			let alpha = skeletonColor.a * slotColor.a * regionColor.a;
			let multiplier = pma ? alpha : 1;
			let color = this.tempColor;
			color.set(skeletonColor.r * slotColor.r * regionColor.r * multiplier,
					  skeletonColor.g * slotColor.g * regionColor.g * multiplier,
					  skeletonColor.b * slotColor.b * regionColor.b * multiplier,
					  alpha);
			let dark = this.tempColor2;
			if (slot.darkColor == null) dark.set(0, 0, 0, 1);
			else dark.setFromColor(slot.darkColor);

			region.computeWorldVertices(slot.bone, this.vertices, 0, this.vertexSize);

			let vertices = this.vertices;
			let uvs = region.uvs;

			let i = 2;
			vertices[i++] = color.r;
			vertices[i++] = color.g;
			vertices[i++] = color.b;
			vertices[i++] = color.a;
			vertices[i++] = uvs[0];
			vertices[i++] = uvs[1];
			if (twoColorTint) {
				vertices[i++] = dark.r;
				vertices[i++] = dark.g;
				vertices[i++] = dark.b;
				vertices[i++] = 1;
			}
			i+=2;

			vertices[i++] = color.r;
			vertices[i++] = color.g;
			vertices[i++] = color.b;
			vertices[i++] = color.a;
			vertices[i++] = uvs[2];
			vertices[i++] = uvs[3];
			if (twoColorTint) {
				vertices[i++] = dark.r;
				vertices[i++] = dark.g;
				vertices[i++] = dark.b;
				vertices[i++] = 1;
			}
			i+=2;

			vertices[i++] = color.r;
			vertices[i++] = color.g;
			vertices[i++] = color.b;
			vertices[i++] = color.a;
			vertices[i++] = uvs[4];
			vertices[i++] = uvs[5];
			if (twoColorTint) {
				vertices[i++] = dark.r;
				vertices[i++] = dark.g;
				vertices[i++] = dark.b;
				vertices[i++] = 1;
			}
			i+=2;

			vertices[i++] = color.r;
			vertices[i++] = color.g;
			vertices[i++] = color.b;
			vertices[i++] = color.a;
			vertices[i++] = uvs[6];
			vertices[i++] = uvs[7];
			if (twoColorTint) {
				vertices[i++] = dark.r;
				vertices[i++] = dark.g;
				vertices[i++] = dark.b;
				vertices[i++] = 1;
			}

			this.renderable.vertices = vertices;
			this.renderable.numFloats = 4 * (twoColorTint ? 12 :  8);
			return this.renderable;
		}

		private computeMeshVertices(slot: Slot, mesh: MeshAttachment, pma: boolean, twoColorTint: boolean = false) {
			let skeleton = slot.bone.skeleton;
			let skeletonColor = skeleton.color;
			let slotColor = slot.color;
			let regionColor = mesh.color;
			let alpha = skeletonColor.a * slotColor.a * regionColor.a;
			let multiplier = pma ? alpha : 1;
			let color = this.tempColor;
			color.set(skeletonColor.r * slotColor.r * regionColor.r * multiplier,
					  skeletonColor.g * slotColor.g * regionColor.g * multiplier,
					  skeletonColor.b * slotColor.b * regionColor.b * multiplier,
					  alpha);
			let dark = this.tempColor2;
			if (slot.darkColor == null) dark.set(0, 0, 0, 1);
			else dark.setFromColor(slot.darkColor);

			let numVertices = mesh.worldVerticesLength / 2;
			if (this.vertices.length < mesh.worldVerticesLength) {
				this.vertices = Utils.newFloatArray(mesh.worldVerticesLength);
			}
			let vertices = this.vertices;
			mesh.computeWorldVertices(slot, 0, mesh.worldVerticesLength, vertices, 0, this.vertexSize);

			let uvs = mesh.uvs;
			if (!twoColorTint) {
				for (let i = 0, n = numVertices, u = 0, v = 2; i < n; i++) {
					vertices[v++] = color.r;
					vertices[v++] = color.g;
					vertices[v++] = color.b;
					vertices[v++] = color.a;
					vertices[v++] = uvs[u++];
					vertices[v++] = uvs[u++];
					v += 2;
				}
			} else {
				for (let i = 0, n = numVertices, u = 0, v = 2; i < n; i++) {
					vertices[v++] = color.r;
					vertices[v++] = color.g;
					vertices[v++] = color.b;
					vertices[v++] = color.a;
					vertices[v++] = uvs[u++];
					vertices[v++] = uvs[u++];
					vertices[v++] = dark.r;
					vertices[v++] = dark.g;
					vertices[v++] = dark.b;
					vertices[v++] = 1;
					v += 2;
				}
			}

			this.renderable.vertices = vertices;
			this.renderable.numFloats = numVertices * (twoColorTint ? 12 :  8);
			return this.renderable;
		}
	}
}
