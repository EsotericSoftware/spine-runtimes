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

import { BlendMode, ClippingAttachment, Color, MeshAttachment, type NumberArrayLike, RegionAttachment, type Skeleton, SkeletonClipping, type Slot, type TextureRegion, Utils } from "@esotericsoftware/spine-core";
import type { CanvasTexture } from "./CanvasTexture.js";

const worldVertices = Utils.newFloatArray(8);

/** Map a Spine `BlendMode` to the matching Canvas2D `globalCompositeOperation`.
 * The four Spine blend modes have direct canvas equivalents — "lighter" reproduces
 * additive (`GL_ONE, GL_ONE`) blending on canvas2d.
 */
function blendModeToComposite (blendMode: BlendMode): GlobalCompositeOperation {
	switch (blendMode) {
		case BlendMode.Additive: return "lighter";
		case BlendMode.Multiply: return "multiply";
		case BlendMode.Screen:   return "screen";
		default:                 return "source-over"; // Normal
	}
}

export class SkeletonRenderer {
	static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];
	// Positions-only world-vertex buffer (stride 2). Per-vertex color and
	// UVs were previously interleaved here at VERTEX_SIZE = 2 + 2 + 4, but
	// canvas2d has no per-pixel vertex-attribute interpolation: the four
	// color floats per vertex were computed and never read by drawTriangle,
	// and UVs come straight from `sequence.getUVs()` (a spine-cached array
	// keyed by sequence frame, stable per-frame). Dropping both halves the
	// vertex-buffer traffic and cuts the helper code to a single
	// `computeWorldVertices(..., stride: 2)` call per attachment.
	static VERTEX_SIZE = 2;

	private ctx: CanvasRenderingContext2D;

	public triangleRendering = false;
	public debugRendering = false;
	private vertices = Utils.newFloatArray(2 * 1024);
	private tempColor = new Color();

	// Clipping state. `clipper` tracks the active clip range (which `endSlot`
	// it ends at) per spine-core's polygon-clip helper; `clippingVerts` is
	// the world-space polygon buffer; `clipped` tells us whether we've
	// `ctx.save()`d to install a canvas clip so we know to `restore()` when
	// the range closes.
	private clipper = new SkeletonClipping();
	private clippingVerts: NumberArrayLike = Utils.newFloatArray(8);
	private clipped = false;

	constructor (context: CanvasRenderingContext2D) {
		this.ctx = context;
	}

	draw (skeleton: Skeleton) {
		if (this.triangleRendering) this.drawTriangles(skeleton);
		else this.drawImages(skeleton);
	}

	/** Push a canvas clip for the polygon defined by `attachment`. Uses
	 * `ctx.save()` so an `endClipRange` call can `restore()` cleanly even
	 * if the per-slot draws push their own save/restore in between.
	 */
	private startClipRange (skeleton: Skeleton, slot: Slot, attachment: ClippingAttachment) {
		this.clipper.clipStart(skeleton, slot, attachment);

		const verticesLength = attachment.worldVerticesLength;
		if (verticesLength > this.clippingVerts.length) {
			this.clippingVerts = Utils.newFloatArray(verticesLength);
		}
		attachment.computeWorldVertices(skeleton, slot, 0, verticesLength, this.clippingVerts, 0, 2);

		// nested clips: pop the outer one first so its save/restore pairs
		// stay balanced. The new clip becomes the active range.
		if (this.clipped) this.ctx.restore();

		const ctx = this.ctx;
		ctx.save();
		ctx.beginPath();
		ctx.moveTo(this.clippingVerts[0], this.clippingVerts[1]);
		for (let v = 2; v < verticesLength; v += 2) {
			ctx.lineTo(this.clippingVerts[v], this.clippingVerts[v + 1]);
		}
		ctx.closePath();
		ctx.clip();
		this.clipped = true;
	}

	/** Step the clipper state (which may close the active range if `slot`
	 * is the range's `endSlot`) and pop the canvas clip when the range ends.
	 * Safe to call once per slot regardless of whether a clip is active.
	 */
	private endClipRange (slot?: Slot) {
		this.clipper.clipEnd(slot);
		if (this.clipped && !this.clipper.isClipping()) {
			this.ctx.restore();
			this.clipped = false;
		}
	}

	private drawImages (skeleton: Skeleton) {
		const ctx = this.ctx;
		const color = this.tempColor;
		const skeletonColor = skeleton.color;
		const drawOrder = skeleton.drawOrder.appliedPose;

		let blendMode: BlendMode | null = null;

		if (this.debugRendering) ctx.strokeStyle = "green";

		for (let i = 0, n = drawOrder.length; i < n; i++) {
			const slot = drawOrder[i];
			const bone = slot.bone;
			const pose = slot.appliedPose;
			const attachment = pose.attachment;

			if (attachment instanceof ClippingAttachment) {
				this.startClipRange(skeleton, slot, attachment);
				continue;
			}
			if (!bone.active) { this.endClipRange(slot); continue; }
			if (!(attachment instanceof RegionAttachment)) { this.endClipRange(slot); continue; }

			const sequence = attachment.sequence;
			const sequenceIndex = sequence.resolveIndex(pose);
			attachment.computeWorldVertices(slot, attachment.getOffsets(pose), worldVertices, 0, 2);

			const region = sequence.regions[sequenceIndex] as TextureRegion;

			const image: HTMLImageElement = region.texture.getImage() as HTMLImageElement;

			const slotColor = pose.color;
			const regionColor = attachment.color;
			color.set(skeletonColor.r * slotColor.r * regionColor.r,
				skeletonColor.g * slotColor.g * regionColor.g,
				skeletonColor.b * slotColor.b * regionColor.b,
				skeletonColor.a * slotColor.a * regionColor.a);

			if (slot.data.blendMode !== blendMode) {
				blendMode = slot.data.blendMode;
				ctx.globalCompositeOperation = blendModeToComposite(blendMode);
			}

			ctx.save();
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
			this.endClipRange(slot);
		}

		this.endClipRange();
		ctx.globalCompositeOperation = "source-over";
	}

	private drawTriangles (skeleton: Skeleton) {
		const ctx = this.ctx;
		const color = this.tempColor;
		const skeletonColor = skeleton.color;
		const drawOrder = skeleton.drawOrder.appliedPose;

		let blendMode: BlendMode | null = null;
		let vertices: NumberArrayLike = this.vertices;
		let uvs: NumberArrayLike = this.vertices;
		let triangles: Array<number> | null = null;

		for (let i = 0, n = drawOrder.length; i < n; i++) {
			const slot = drawOrder[i];
			const pose = slot.appliedPose;
			const attachment = pose.attachment;

			if (attachment instanceof ClippingAttachment) {
				this.startClipRange(skeleton, slot, attachment);
				continue;
			}

			let texture: HTMLImageElement;
			if (attachment instanceof RegionAttachment) {
				const sequence = attachment.sequence;
				const sequenceIndex = sequence.resolveIndex(pose);

				attachment.computeWorldVertices(slot, attachment.getOffsets(pose), this.vertices, 0, 2);
				vertices = this.vertices;
				uvs = sequence.getUVs(sequenceIndex);
				triangles = SkeletonRenderer.QUAD_TRIANGLES;

				texture = (sequence.regions[sequenceIndex]?.texture as CanvasTexture).getImage();
			} else if (attachment instanceof MeshAttachment) {
				const sequence = attachment.sequence;
				const sequenceIndex = sequence.resolveIndex(pose);

				if (this.vertices.length < attachment.worldVerticesLength) {
					this.vertices = Utils.newFloatArray(attachment.worldVerticesLength);
				}
				attachment.computeWorldVertices(skeleton, slot, 0, attachment.worldVerticesLength, this.vertices, 0, 2);
				vertices = this.vertices;
				uvs = sequence.getUVs(sequenceIndex);
				triangles = attachment.triangles;

				texture = (sequence.regions[sequenceIndex]?.texture as CanvasTexture).getImage();
			} else {
				this.endClipRange(slot);
				continue;
			}

			if (texture) {
				if (slot.data.blendMode !== blendMode) {
					blendMode = slot.data.blendMode;
					ctx.globalCompositeOperation = blendModeToComposite(blendMode);
				}

				const slotColor = pose.color;
				const attachmentColor = attachment.color;
				color.set(skeletonColor.r * slotColor.r * attachmentColor.r,
					skeletonColor.g * slotColor.g * attachmentColor.g,
					skeletonColor.b * slotColor.b * attachmentColor.b,
					skeletonColor.a * slotColor.a * attachmentColor.a);

				ctx.globalAlpha = color.a;

				for (let j = 0; j < triangles.length; j += 3) {
					// triangle index N → position at vertices[N*2 .. N*2+1],
					// UV at uvs[N*2 .. N*2+1]. Positions are recomputed per
					// frame; UVs come from spine's sequence cache.
					const a = triangles[j] * 2, b = triangles[j + 1] * 2, c = triangles[j + 2] * 2;

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
			this.endClipRange(slot);
		}

		this.endClipRange();
		this.ctx.globalAlpha = 1;
		this.ctx.globalCompositeOperation = "source-over";
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
