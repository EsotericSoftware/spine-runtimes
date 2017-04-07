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

module spine.canvas {
	export class SkeletonRenderer {
		static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];
		static VERTEX_SIZE = 2 + 2 + 4;

		private ctx: CanvasRenderingContext2D;

		public triangleRendering = false;
		public debugRendering = false;
		private vertices = Utils.newFloatArray(8 * 1024);
		private tempColor = new Color();

		constructor (context: CanvasRenderingContext2D) {
			this.ctx = context;
		}

		draw (skeleton: Skeleton) {
			if (this.triangleRendering) this.drawTriangles(skeleton);
			else this.drawImages(skeleton);
		}

		private drawImages (skeleton: Skeleton) {
			let ctx = this.ctx;
			let drawOrder = skeleton.drawOrder;

			if (this.debugRendering) ctx.strokeStyle = "green";

			ctx.save();
			for (let i = 0, n = drawOrder.length; i < n; i++) {
				let slot = drawOrder[i];
				let attachment = slot.getAttachment();
				let regionAttachment: RegionAttachment = null;
				let region: TextureAtlasRegion = null;
				let image: HTMLImageElement = null;

				if (attachment instanceof RegionAttachment) {
					regionAttachment = <RegionAttachment>attachment;
					region = <TextureAtlasRegion>regionAttachment.region;
					image = (<CanvasTexture>region.texture).getImage();
				} else continue;

				let skeleton = slot.bone.skeleton;
				let skeletonColor = skeleton.color;
				let slotColor = slot.color;
				let regionColor = regionAttachment.color;
				let alpha = skeletonColor.a * slotColor.a * regionColor.a;
				let color = this.tempColor;
				color.set(skeletonColor.r * slotColor.r * regionColor.r,
					skeletonColor.g * slotColor.g * regionColor.g,
					skeletonColor.b * slotColor.b * regionColor.b,
					alpha);

				let att = <RegionAttachment>attachment;
				let bone = slot.bone;
				let w = region.width;
				let h = region.height;
				ctx.save();
				ctx.transform(bone.a, bone.c, bone.b, bone.d, bone.worldX, bone.worldY);
				ctx.translate(attachment.offset[0], attachment.offset[1]);
				ctx.rotate(attachment.rotation * Math.PI / 180);
				ctx.scale(attachment.scaleX, attachment.scaleY);
				ctx.translate(w / 2, h / 2);
				if (attachment.region.rotate) {
					let t = w;
					w = h;
					h = t;
					ctx.rotate(-Math.PI / 2);
				}
				ctx.scale(1, -1);
				ctx.translate(-w / 2, -h / 2);
				if (color.r != 1 || color.g != 1 || color.b != 1 || color.a != 1) {
					ctx.globalAlpha = color.a;
					// experimental tinting via compositing, doesn't work
					// ctx.globalCompositeOperation = "source-atop";
					// ctx.fillStyle = "rgba(" + (color.r * 255 | 0) + ", " + (color.g * 255 | 0)  + ", " + (color.b * 255 | 0) + ", " + color.a + ")";
					// ctx.fillRect(0, 0, w, h);
				}
				ctx.drawImage(image, region.x, region.y, w, h, 0, 0, w, h);
				if (this.debugRendering) ctx.strokeRect(0, 0, w, h);
				ctx.restore();
			}

			ctx.restore();
		}

		private drawTriangles (skeleton: Skeleton) {
			let blendMode: BlendMode = null;

			let vertices: ArrayLike<number> = this.vertices;
			let triangles: Array<number> = null;
			let drawOrder = skeleton.drawOrder;

			for (let i = 0, n = drawOrder.length; i < n; i++) {
				let slot = drawOrder[i];
				let attachment = slot.getAttachment();
				let texture: HTMLImageElement = null;
				let region: TextureAtlasRegion = null;
				if (attachment instanceof RegionAttachment) {
					let regionAttachment = <RegionAttachment>attachment;
					vertices = this.computeRegionVertices(slot, regionAttachment, false);
					triangles = SkeletonRenderer.QUAD_TRIANGLES;
					region = <TextureAtlasRegion>regionAttachment.region;
					texture = (<CanvasTexture>region.texture).getImage();

				} else if (attachment instanceof MeshAttachment) {
					let mesh = <MeshAttachment>attachment;
					vertices = this.computeMeshVertices(slot, mesh, false);
					triangles = mesh.triangles;
					texture = (<TextureAtlasRegion>mesh.region.renderObject).texture.getImage();
				} else continue;

				if (texture != null) {
					let slotBlendMode = slot.data.blendMode;
					if (slotBlendMode != blendMode) {
						blendMode = slotBlendMode;
					}

					let ctx = this.ctx;

					for (var j = 0; j < triangles.length; j+=3) {
						let t1 = triangles[j] * 8, t2 = triangles[j+1] * 8, t3 = triangles[j+2] * 8;

						let x0 = vertices[t1], y0 = vertices[t1 + 1], u0 = vertices[t1 + 6], v0 = vertices[t1 + 7];
						let x1 = vertices[t2], y1 = vertices[t2 + 1], u1 = vertices[t2 + 6], v1 = vertices[t2 + 7];
						let x2 = vertices[t3], y2 = vertices[t3 + 1], u2 = vertices[t3 + 6], v2 = vertices[t3 + 7];

						this.drawTriangle(texture, x0, y0, u0, v0, x1, y1, u1, v1, x2, y2, u2, v2);

						if (this.debugRendering) {
							ctx.strokeStyle = "green";
							ctx.beginPath();
							ctx.moveTo(x0, y0);
							ctx.lineTo(x1, y1);
							ctx.lineTo(x2, y2);
							ctx.lineTo(x0, y0);
							ctx.stroke();
						}
					}
				}
			}
		}

		// Adapted from http://extremelysatisfactorytotalitarianism.com/blog/?p=2120
		// Apache 2 licensed
		private drawTriangle(img: HTMLImageElement, x0: number, y0: number, u0: number, v0: number,
						x1: number, y1: number, u1: number, v1: number,
						x2: number, y2: number, u2: number, v2: number) {
			let ctx = this.ctx;

			u0 *= img.width;
			v0 *= img.height;
			u1 *= img.width;
			v1 *= img.height;
			u2 *= img.width;
			v2 *= img.height;

			ctx.beginPath();
			ctx.moveTo(x0, y0);
			ctx.lineTo(x1, y1);
			ctx.lineTo(x2, y2);
			ctx.closePath();

			x1 -= x0;
			y1 -= y0;
			x2 -= x0;
			y2 -= y0;

			u1 -= u0;
			v1 -= v0;
			u2 -= u0;
			v2 -= v0;

			var det = 1 / (u1*v2 - u2*v1),

			// linear transformation
			a = (v2*x1 - v1*x2) * det,
			b = (v2*y1 - v1*y2) * det,
			c = (u1*x2 - u2*x1) * det,
			d = (u1*y2 - u2*y1) * det,

			// translation
			e = x0 - a*u0 - c*v0,
			f = y0 - b*u0 - d*v0;

			ctx.save();
			ctx.transform(a, b, c, d, e, f);
			ctx.clip();
			ctx.drawImage(img, 0, 0);
			ctx.restore();
		}

		private computeRegionVertices(slot: Slot, region: RegionAttachment, pma: boolean) {
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

		private computeMeshVertices(slot: Slot, mesh: MeshAttachment, pma: boolean) {
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
