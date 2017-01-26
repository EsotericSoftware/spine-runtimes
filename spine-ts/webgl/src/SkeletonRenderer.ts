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
	export class SkeletonRenderer {
		static VERTEX_SIZE = 2 + 2 + 4;
		static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];

		premultipliedAlpha = false;
		private gl: WebGLRenderingContext;
		private tempColor = new Color();
		private vertices = Utils.newFloatArray(SkeletonRenderer.VERTEX_SIZE * 1024);

		constructor (gl: WebGLRenderingContext) {
			this.gl = gl;
		}

		draw (batcher: PolygonBatcher, skeleton: Skeleton) {
			let premultipliedAlpha = this.premultipliedAlpha;
			let blendMode: BlendMode = null;

			let vertices: ArrayLike<number> = null;
			let triangles: Array<number> = null;
			let drawOrder = skeleton.drawOrder;
			for (let i = 0, n = drawOrder.length; i < n; i++) {
				let slot = drawOrder[i];
				let attachment = slot.getAttachment();
				let texture: GLTexture = null;
				if (attachment instanceof RegionAttachment) {
					let region = <RegionAttachment>attachment;
					vertices = this.computeRegionVertices(slot, region, premultipliedAlpha);
					triangles = SkeletonRenderer.QUAD_TRIANGLES;
					texture = <GLTexture>(<TextureAtlasRegion>region.region.renderObject).texture;

				} else if (attachment instanceof MeshAttachment) {
					let mesh = <MeshAttachment>attachment;
					vertices = this.computeMeshVertices(slot, mesh, premultipliedAlpha);
					triangles = mesh.triangles;
					texture = <GLTexture>(<TextureAtlasRegion>mesh.region.renderObject).texture;
				} else continue;

				if (texture != null) {
					let slotBlendMode = slot.data.blendMode;
					if (slotBlendMode != blendMode) {
						blendMode = slotBlendMode;
						batcher.setBlendMode(getSourceGLBlendMode(this.gl, blendMode, premultipliedAlpha), getDestGLBlendMode(this.gl, blendMode));
					}
					batcher.draw(texture, vertices, triangles);
				}
			}
		}

		computeRegionVertices(slot: Slot, region: RegionAttachment, pma: boolean) {
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

			region.computeWorldVertices(slot.bone, this.vertices, 0, SkeletonRenderer.VERTEX_SIZE);

			let vertices = this.vertices;
			let uvs = region.uvs;

			vertices[RegionAttachment.C1R] = color.r;
			vertices[RegionAttachment.C1G] = color.g;
			vertices[RegionAttachment.C1B] = color.b;
			vertices[RegionAttachment.C1A] = color.a;
			vertices[RegionAttachment.U1] = uvs[0];
			vertices[RegionAttachment.V1] = uvs[1];

			vertices[RegionAttachment.C2R] = color.r;
			vertices[RegionAttachment.C2G] = color.g;
			vertices[RegionAttachment.C2B] = color.b;
			vertices[RegionAttachment.C2A] = color.a;
			vertices[RegionAttachment.U2] = uvs[2];
			vertices[RegionAttachment.V2] = uvs[3];

			vertices[RegionAttachment.C3R] = color.r;
			vertices[RegionAttachment.C3G] = color.g;
			vertices[RegionAttachment.C3B] = color.b;
			vertices[RegionAttachment.C3A] = color.a;
			vertices[RegionAttachment.U3] = uvs[4];
			vertices[RegionAttachment.V3] = uvs[5];

			vertices[RegionAttachment.C4R] = color.r;
			vertices[RegionAttachment.C4G] = color.g;
			vertices[RegionAttachment.C4B] = color.b;
			vertices[RegionAttachment.C4A] = color.a;
			vertices[RegionAttachment.U4] = uvs[6];
			vertices[RegionAttachment.V4] = uvs[7];

			return vertices;
		}

		computeMeshVertices(slot: Slot, mesh: MeshAttachment, pma: boolean) {
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

			let numVertices = mesh.worldVerticesLength / 2;
			if (this.vertices.length < mesh.worldVerticesLength) {
				this.vertices = Utils.newFloatArray(mesh.worldVerticesLength);
			}
			let vertices = this.vertices;
			mesh.computeWorldVertices(slot, 0, mesh.worldVerticesLength, vertices, 0, SkeletonRenderer.VERTEX_SIZE);

			let uvs = mesh.uvs;
			for (let i = 0, n = numVertices, u = 0, v = 2; i < n; i++) {
				vertices[v++] = color.r;
				vertices[v++] = color.g;
				vertices[v++] = color.b;
				vertices[v++] = color.a;
				vertices[v++] = uvs[u++];
				vertices[v++] = uvs[u++];
				v += 2;
			}

			return vertices;
		}
	}
}
