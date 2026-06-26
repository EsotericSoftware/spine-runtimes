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

import { BlendMode, Color, RegionAttachment, type Skeleton, SkeletonRendererCore, type TextureRegion } from "@esotericsoftware/spine-core";
import type { CanvasTexture } from "./CanvasTexture.js";

// Use Canvas2D's closest native compositing equivalents. These may not be pixel-identical to WebGL on transparent destinations.
function blendModeToCompositeOperation (blendMode: BlendMode): GlobalCompositeOperation {
	switch (blendMode) {
		case BlendMode.Additive:
			return "lighter";
		case BlendMode.Multiply:
			return "multiply";
		case BlendMode.Screen:
			return "screen";
	}
	return "source-over";
}

export class SkeletonRenderer {
	// Positions-only stride for triangle rendering and clipping.
	static VERTEX_SIZE = 2;

	private ctx: CanvasRenderingContext2D;

	public triangleRendering = false;
	public debugRendering = false;
	private tempColor = new Color();
	private rendererCore = new SkeletonRendererCore();

	constructor (context: CanvasRenderingContext2D) {
		this.ctx = context;
	}

	draw (skeleton: Skeleton) {
		if (this.triangleRendering) this.drawTriangles(skeleton);
		else this.drawImages(skeleton);
	}

	private drawImages (skeleton: Skeleton) {
		const ctx = this.ctx;
		const color = this.tempColor;
		const skeletonColor = skeleton.color;
		const drawOrder = skeleton.drawOrder.appliedPose;
		const oldAlpha = ctx.globalAlpha;
		const oldCompositeOperation = ctx.globalCompositeOperation;

		if (this.debugRendering) ctx.strokeStyle = "green";

		for (let i = 0, n = drawOrder.length; i < n; i++) {
			const slot = drawOrder[i];
			const bone = slot.bone;
			if (!bone.active) continue;

			const pose = slot.appliedPose;
			const attachment = pose.attachment;
			if (!(attachment instanceof RegionAttachment)) continue;

			const sequence = attachment.sequence;
			const sequenceIndex = sequence.resolveIndex(pose);

			const region = sequence.regions[sequenceIndex] as TextureRegion;

			const image: HTMLImageElement = region.texture.getImage() as HTMLImageElement;

			const slotColor = pose.color;
			const regionColor = attachment.color;
			color.set(skeletonColor.r * slotColor.r * regionColor.r,
				skeletonColor.g * slotColor.g * regionColor.g,
				skeletonColor.b * slotColor.b * regionColor.b,
				skeletonColor.a * slotColor.a * regionColor.a);

			ctx.save();
			ctx.globalCompositeOperation = blendModeToCompositeOperation(slot.data.blendMode);

			const boneApplied = bone.appliedPose;
			ctx.transform(boneApplied.a, boneApplied.c, boneApplied.b, boneApplied.d, boneApplied.worldX, boneApplied.worldY);
			const offsets = attachment.getOffsets(pose);
			ctx.translate(offsets[0], offsets[1]);
			ctx.rotate(attachment.rotation * Math.PI / 180);

			const atlasScale = attachment.width / region.originalWidth;
			ctx.scale(atlasScale * attachment.scaleX, atlasScale * attachment.scaleY);

			let w = region.width, h = region.height;
			ctx.translate(w / 2, h / 2);
			if (region.degrees === 90) {
				const t = w;
				w = h;
				h = t;
				ctx.rotate(-Math.PI / 2);
			}
			ctx.scale(1, -1);
			ctx.translate(-w / 2, -h / 2);

			ctx.globalAlpha = color.a;
			ctx.drawImage(image, image.width * region.u, image.height * region.v, w, h, 0, 0, w, h);
			if (this.debugRendering) ctx.strokeRect(0, 0, w, h);
			ctx.restore();
		}

		ctx.globalAlpha = oldAlpha;
		ctx.globalCompositeOperation = oldCompositeOperation;
	}

	private drawTriangles (skeleton: Skeleton) {
		const ctx = this.ctx;
		const oldAlpha = ctx.globalAlpha;
		const oldCompositeOperation = ctx.globalCompositeOperation;

		for (let command = this.rendererCore.render(skeleton, false, undefined, SkeletonRenderer.VERTEX_SIZE); command; command = command.next) {
			if (command.numVertices === 0 || command.numIndices === 0) continue;

			const texture = (command.texture as CanvasTexture).getImage();
			ctx.globalCompositeOperation = blendModeToCompositeOperation(command.blendMode);
			ctx.globalAlpha = (command.colors[0] >>> 24) / 255;
			this.drawTriangleList(texture, command.positions.subarray(0, command.numVertices * SkeletonRenderer.VERTEX_SIZE),
				command.uvs.subarray(0, command.numVertices << 1), command.indices.subarray(0, command.numIndices));
		}

		ctx.globalAlpha = oldAlpha;
		ctx.globalCompositeOperation = oldCompositeOperation;
	}

	private drawTriangleList (texture: HTMLImageElement, vertices: ArrayLike<number>, uvs: ArrayLike<number>, triangles: ArrayLike<number>) {
		const ctx = this.ctx;
		for (let j = 0; j < triangles.length; j += 3) {
			const a = triangles[j] * SkeletonRenderer.VERTEX_SIZE, b = triangles[j + 1] * SkeletonRenderer.VERTEX_SIZE, c = triangles[j + 2] * SkeletonRenderer.VERTEX_SIZE;

			const x0 = vertices[a], y0 = vertices[a + 1], u0 = uvs[a], v0 = uvs[a + 1];
			const x1 = vertices[b], y1 = vertices[b + 1], u1 = uvs[b], v1 = uvs[b + 1];
			const x2 = vertices[c], y2 = vertices[c + 1], u2 = uvs[c], v2 = uvs[c + 1];

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

	// Adapted from http://extremelysatisfactorytotalitarianism.com/blog/?p=2120
	// Apache 2 licensed
	private drawTriangle (img: HTMLImageElement, x0: number, y0: number, u0: number, v0: number,
		x1: number, y1: number, u1: number, v1: number,
		x2: number, y2: number, u2: number, v2: number) {
		const ctx = this.ctx;

		const width = img.width - 1;
		const height = img.height - 1;
		u0 *= width;
		v0 *= height;
		u1 *= width;
		v1 *= height;
		u2 *= width;
		v2 *= height;

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

		let det = u1 * v2 - u2 * v1;
		if (det === 0) return;
		det = 1 / det;

		// linear transformation
		const a = (v2 * x1 - v1 * x2) * det;
		const b = (v2 * y1 - v1 * y2) * det;
		const c = (u1 * x2 - u2 * x1) * det;
		const d = (u1 * y2 - u2 * y1) * det;

		// translation
		const e = x0 - a * u0 - c * v0;
		const f = y0 - b * u0 - d * v0;

		ctx.save();
		ctx.transform(a, b, c, d, e, f);
		ctx.clip();
		ctx.drawImage(img, 0, 0);
		ctx.restore();
	}
}
