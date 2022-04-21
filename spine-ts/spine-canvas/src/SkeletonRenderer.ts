/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

import { Utils, Color, Skeleton, RegionAttachment, TextureAtlasRegion, BlendMode, MeshAttachment, Slot } from "@esotericsoftware/spine-core";
import { CanvasTexture } from "./CanvasTexture";

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
		let color = this.tempColor;
		let skeletonColor = skeleton.color;
		let drawOrder = skeleton.drawOrder;

		if (this.debugRendering) ctx.strokeStyle = "green";

		for (let i = 0, n = drawOrder.length; i < n; i++) {
			let slot = drawOrder[i];
			let bone = slot.bone;
			if (!bone.active) continue;

			let attachment = slot.getAttachment();
			if (!(attachment instanceof RegionAttachment)) continue;
			let region: TextureAtlasRegion = <TextureAtlasRegion>attachment.region;
			let image: HTMLImageElement = (<CanvasTexture>region.page.texture).getImage() as HTMLImageElement;

			let slotColor = slot.color;
			let regionColor = attachment.color;
			color.set(skeletonColor.r * slotColor.r * regionColor.r,
				skeletonColor.g * slotColor.g * regionColor.g,
				skeletonColor.b * slotColor.b * regionColor.b,
				skeletonColor.a * slotColor.a * regionColor.a);

			ctx.save();
			ctx.transform(bone.a, bone.c, bone.b, bone.d, bone.worldX, bone.worldY);
			ctx.translate(attachment.offset[0], attachment.offset[1]);
			ctx.rotate(attachment.rotation * Math.PI / 180);

			let atlasScale = attachment.width / region.originalWidth;
			ctx.scale(atlasScale * attachment.scaleX, atlasScale * attachment.scaleY);

			let w = region.width, h = region.height;
			ctx.translate(w / 2, h / 2);
			if (attachment.region!.degrees == 90) {
				let t = w;
				w = h;
				h = t;
				ctx.rotate(-Math.PI / 2);
			}
			ctx.scale(1, -1);
			ctx.translate(-w / 2, -h / 2);

			ctx.globalAlpha = color.a;
			ctx.drawImage(image, region.x, region.y, w, h, 0, 0, w, h);
			if (this.debugRendering) ctx.strokeRect(0, 0, w, h);
			ctx.restore();
		}
	}

	private drawTriangles (skeleton: Skeleton) {
		let ctx = this.ctx;
		let color = this.tempColor;
		let skeletonColor = skeleton.color;
		let drawOrder = skeleton.drawOrder;

		let blendMode: BlendMode | null = null;
		let vertices: ArrayLike<number> = this.vertices;
		let triangles: Array<number> | null = null;

		for (let i = 0, n = drawOrder.length; i < n; i++) {
			let slot = drawOrder[i];
			let attachment = slot.getAttachment();

			let texture: HTMLImageElement;
			let region: TextureAtlasRegion;
			if (attachment instanceof RegionAttachment) {
				let regionAttachment = <RegionAttachment>attachment;
				vertices = this.computeRegionVertices(slot, regionAttachment, false);
				triangles = SkeletonRenderer.QUAD_TRIANGLES;
				region = <TextureAtlasRegion>regionAttachment.region;
				texture = (<CanvasTexture>region.page.texture).getImage() as HTMLImageElement;
			} else if (attachment instanceof MeshAttachment) {
				let mesh = <MeshAttachment>attachment;
				vertices = this.computeMeshVertices(slot, mesh, false);
				triangles = mesh.triangles;
				let region = (<TextureAtlasRegion>mesh.region!.renderObject);
				texture = region.page.texture!.getImage() as HTMLImageElement;
			} else
				continue;

			if (texture) {
				if (slot.data.blendMode != blendMode) blendMode = slot.data.blendMode;

				let slotColor = slot.color;
				let attachmentColor = attachment.color;
				color.set(skeletonColor.r * slotColor.r * attachmentColor.r,
					skeletonColor.g * slotColor.g * attachmentColor.g,
					skeletonColor.b * slotColor.b * attachmentColor.b,
					skeletonColor.a * slotColor.a * attachmentColor.a);

				ctx.globalAlpha = color.a;

				for (var j = 0; j < triangles.length; j += 3) {
					let t1 = triangles[j] * 8, t2 = triangles[j + 1] * 8, t3 = triangles[j + 2] * 8;

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

		this.ctx.globalAlpha = 1;
	}

	// Adapted from http://extremelysatisfactorytotalitarianism.com/blog/?p=2120
	// Apache 2 licensed
	private drawTriangle (img: HTMLImageElement, x0: number, y0: number, u0: number, v0: number,
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

		var det = 1 / (u1 * v2 - u2 * v1),

			// linear transformation
			a = (v2 * x1 - v1 * x2) * det,
			b = (v2 * y1 - v1 * y2) * det,
			c = (u1 * x2 - u2 * x1) * det,
			d = (u1 * y2 - u2 * y1) * det,

			// translation
			e = x0 - a * u0 - c * v0,
			f = y0 - b * u0 - d * v0;

		ctx.save();
		ctx.transform(a, b, c, d, e, f);
		ctx.clip();
		ctx.drawImage(img, 0, 0);
		ctx.restore();
	}

	private computeRegionVertices (slot: Slot, region: RegionAttachment, pma: boolean) {
		let skeletonColor = slot.bone.skeleton.color;
		let slotColor = slot.color;
		let regionColor = region.color;
		let alpha = skeletonColor.a * slotColor.a * regionColor.a;
		let multiplier = pma ? alpha : 1;
		let color = this.tempColor;
		color.set(skeletonColor.r * slotColor.r * regionColor.r * multiplier,
			skeletonColor.g * slotColor.g * regionColor.g * multiplier,
			skeletonColor.b * slotColor.b * regionColor.b * multiplier,
			alpha);

		region.computeWorldVertices(slot, this.vertices, 0, SkeletonRenderer.VERTEX_SIZE);

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

	private computeMeshVertices (slot: Slot, mesh: MeshAttachment, pma: boolean) {
		let skeletonColor = slot.bone.skeleton.color;
		let slotColor = slot.color;
		let regionColor = mesh.color;
		let alpha = skeletonColor.a * slotColor.a * regionColor.a;
		let multiplier = pma ? alpha : 1;
		let color = this.tempColor;
		color.set(skeletonColor.r * slotColor.r * regionColor.r * multiplier,
			skeletonColor.g * slotColor.g * regionColor.g * multiplier,
			skeletonColor.b * slotColor.b * regionColor.b * multiplier,
			alpha);

		let vertexCount = mesh.worldVerticesLength / 2;
		let vertices = this.vertices;
		if (vertices.length < mesh.worldVerticesLength) this.vertices = vertices = Utils.newFloatArray(mesh.worldVerticesLength);
		mesh.computeWorldVertices(slot, 0, mesh.worldVerticesLength, vertices, 0, SkeletonRenderer.VERTEX_SIZE);

		let uvs = mesh.uvs;
		for (let i = 0, u = 0, v = 2; i < vertexCount; i++) {
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
