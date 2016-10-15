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

		private ctx: CanvasRenderingContext2D;

		public triangleRendering = false;
		public debugRendering = false;

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

			for (let i = 0, n = drawOrder.length; i < n; i++) {
				let slot = drawOrder[i];
				let attachment = slot.getAttachment();
				let region: TextureAtlasRegion = null;
				let image: HTMLImageElement = null;
				let vertices: ArrayLike<number> = null;
				if (attachment instanceof RegionAttachment) {
					let regionAttachment = <RegionAttachment>attachment;
					vertices = regionAttachment.updateWorldVertices(slot, false);
					region = <TextureAtlasRegion>regionAttachment.region;
					image = (<CanvasTexture>(region).texture).getImage();

				} else continue;

				let att = <RegionAttachment>attachment;
				let bone = slot.bone;
				let x = vertices[0];
				let y = vertices[1];
				let rotation = (bone.getWorldRotationX() - att.rotation) * Math.PI / 180;
				let xx = vertices[24] - vertices[0];
				let xy = vertices[25] - vertices[1];
				let yx = vertices[8] - vertices[0];
				let yy = vertices[9] - vertices[1];
				let w = Math.sqrt(xx * xx + xy * xy), h = -Math.sqrt(yx * yx + yy * yy);
				ctx.translate(x, y);
				ctx.rotate(rotation);
				if (region.rotate) {
					ctx.rotate(Math.PI / 2);
					ctx.drawImage(image, region.x, region.y, region.height, region.width, 0, 0, h, -w);
					ctx.rotate(-Math.PI / 2);
				} else {
					ctx.drawImage(image, region.x, region.y, region.width, region.height, 0, 0, w, h);
				}
				if (this.debugRendering) ctx.strokeRect(0, 0, w, h);
				ctx.rotate(-rotation);
				ctx.translate(-x, -y);
			}
		}

		private drawTriangles (skeleton: Skeleton) {
			let blendMode: BlendMode = null;

			let vertices: ArrayLike<number> = null;
			let triangles: Array<number> = null;
			let drawOrder = skeleton.drawOrder;

			for (let i = 0, n = drawOrder.length; i < n; i++) {
				let slot = drawOrder[i];
				let attachment = slot.getAttachment();
				let texture: HTMLImageElement = null;
				let region: TextureAtlasRegion = null;
				if (attachment instanceof RegionAttachment) {
					let regionAttachment = <RegionAttachment>attachment;
					vertices = regionAttachment.updateWorldVertices(slot, false);
					triangles = SkeletonRenderer.QUAD_TRIANGLES;
					region = <TextureAtlasRegion>regionAttachment.region;
					texture = (<CanvasTexture>region.texture).getImage();

				} else if (attachment instanceof MeshAttachment) {
					let mesh = <MeshAttachment>attachment;
					vertices = mesh.updateWorldVertices(slot, false);
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
	}
}
