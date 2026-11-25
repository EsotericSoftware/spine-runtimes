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

import { type BlendMode, type Bone, ClippingAttachment, MathUtils, MeshAttachment, PathAttachment, RegionAttachment, type RenderCommand, type Skeleton, SkeletonRendererCore, Utils, Vector2 } from "@esotericsoftware/spine-core";
import type { C3Matrix } from "./C3Matrix";
import { BlendingModeSpineToC3, type C3TextureEditor, type C3TextureRuntime } from "./C3Texture";

type C3Renderer = IRenderer | SDK.Gfx.IWebGLRenderer;
type C3Texture = C3TextureRuntime | C3TextureEditor;
type C3Quad = DOMQuad | SDK.Quad;

abstract class C3SkeletonRenderer<
	Renderer extends C3Renderer,
	Texture extends C3Texture,
> extends SkeletonRendererCore {

	private command?: RenderCommand;
	private transformedPositions: { buffer: Float32Array; positions: Float32Array }[] = [];
	private transformedMatrixRevision = -1;
	private inputColor: [number, number, number, number] = [1, 1, 1, 1];
	private tempVertices = new Float32Array(4096);
	private tempColors = new Float32Array(4096);
	private tempPoint = new Vector2();
	private tempArray = [] as number[];
	private inv255 = 1 / 255;

	private prevRed = -1;
	private prevGreen = -1;
	private prevBlue = -1;
	private prevAlpha = -1;
	private prevSlotZOffset = Number.NaN;

	constructor (
		protected renderer: Renderer,
		protected matrix: C3Matrix,
	) {
		super();
	}

	draw (skeleton: Skeleton, inColors: [number, number, number], opacity = 1, requestRedraw = true, slotZOffset = 0) {
		const { inv255 } = this;
		const newCommand = requestRedraw ||
			this.prevRed !== inColors[0] || this.prevGreen !== inColors[1] || this.prevBlue !== inColors[2] ||
			this.prevAlpha !== opacity || this.prevSlotZOffset !== slotZOffset || !this.command;
		this.prevRed = inColors[0];
		this.prevGreen = inColors[1];
		this.prevBlue = inColors[2];
		this.prevAlpha = opacity;
		this.prevSlotZOffset = slotZOffset;

		if (newCommand) {
			const inputColor = this.inputColor;
			inputColor[0] = inColors[0];
			inputColor[1] = inColors[1];
			inputColor[2] = inColors[2];
			inputColor[3] = opacity;
			this.command = this.render(skeleton, true, inputColor, 3, slotZOffset);
		}
		const transformPositions = newCommand || this.transformedMatrixRevision !== this.matrix.revision;
		let command = this.command;
		let commandIndex = 0;

		while (command) {
			const { numVertices, uvs, colors, indices, blendMode } = command;
			const transformedPositions = this.getTransformedPositions(command, commandIndex++, transformPositions);
			const c3colors = this.tempColors.length < numVertices * 4
				? (this.tempColors = new Float32Array(numVertices * 4))
				: this.tempColors;

			for (let i = 0; i < numVertices; i++) {
				const color = colors[i];
				const colorDst = i * 4;
				c3colors[colorDst] = (color >>> 16 & 0xFF) * inv255;
				c3colors[colorDst + 1] = (color >>> 8 & 0xFF) * inv255;
				c3colors[colorDst + 2] = (color & 0xFF) * inv255;
				c3colors[colorDst + 3] = (color >>> 24) * inv255;
			}

			this.renderSkeleton(
				transformedPositions,
				uvs,
				indices,
				c3colors.subarray(0, numVertices * 4),
				command.texture,
				blendMode)
			command = command.next;
		}
		this.transformedMatrixRevision = this.matrix.revision;
	}

	drawDebug (skeleton: Skeleton, x: number, y: number, quad: C3Quad) {
		const { matrix } = this;

		this.setColorFillMode();
		this.setBlendMode();

		// bones
		const bones = skeleton.bones;
		for (let i = 0, n = bones.length; i < n; i++) {
			const bone = bones[i];
			// if (!bone.parent) continue;
			const boneApplied = bone.appliedPose;
			const { x: x1, y: y1 } = matrix.skeletonToGame(boneApplied.worldX, boneApplied.worldY);
			const endX = boneApplied.worldX + bone.data.length * boneApplied.a;
			const endY = boneApplied.worldY + bone.data.length * boneApplied.c;
			const { x: x2, y: y2 } = matrix.skeletonToGame(endX, endY);

			this.setColor(1, 0, 0, 1);
			this.setColorFillMode();

			const t = this.tempPoint.set(y2 - y1, x1 - x2);
			t.normalize();
			const width = 1 * 0.5;
			const tx = t.x * width;
			const ty = t.y * width;
			this.poly([
				x1 + tx, y1 + ty,
				x1 - tx, y1 - ty,
				x2 + tx, y2 + ty,
				x2 - tx, y2 - ty,
				x2 + tx, y2 + ty,
				x1 - tx, y1 - ty,
			]);

			this.setColor(0, 1, 0, 1);
			this.poly(this.circle(x1, y1, 2));
		}

		// regions
		this.setColor(0, 0, 1, 0.5);
		const slots = skeleton.slots;
		for (let i = 0, n = slots.length; i < n; i++) {
			const slot = slots[i];
			if (!slot.bone.active) continue;
			const attachment = slot.appliedPose.attachment;
			if (attachment instanceof RegionAttachment) {
				const vertices = this.tempVertices;

				attachment.computeWorldVertices(slot, attachment.getOffsets(slot.appliedPose), vertices, 0, 2);
				let p = matrix.skeletonToGame(vertices[0], vertices[1]);
				const x1 = p.x, y1 = p.y;
				p = matrix.skeletonToGame(vertices[2], vertices[3]);
				const x2 = p.x, y2 = p.y;
				p = matrix.skeletonToGame(vertices[4], vertices[5]);
				const x3 = p.x, y3 = p.y;
				p = matrix.skeletonToGame(vertices[6], vertices[7]);
				const x4 = p.x, y4 = p.y;
				this.line(x1, y1, x2, y2);
				this.line(x2, y2, x3, y3);
				this.line(x3, y3, x4, y4);
				this.line(x4, y4, x1, y1);
			}
		}

		// meshes
		for (let i = 0, n = slots.length; i < n; i++) {
			const slot = slots[i];
			if (!slot.bone.active) continue;
			const attachment = slot.appliedPose.attachment;
			if (!(attachment instanceof MeshAttachment)) continue;
			if (this.tempVertices.length < attachment.worldVerticesLength)
				this.tempVertices = new Float32Array(attachment.worldVerticesLength);
			const vertices = this.tempVertices;
			attachment.computeWorldVertices(skeleton, slot, 0, attachment.worldVerticesLength, vertices, 0, 2);
			const triangles = attachment.triangles;
			let hullLength = attachment.hullLength;

			// mesh triangles
			this.setColor(1, 0.64, 0, 0.5);
			for (let ii = 0, nn = triangles.length; ii < nn; ii += 3) {
				const v1 = triangles[ii] * 2, v2 = triangles[ii + 1] * 2, v3 = triangles[ii + 2] * 2;
				let p = matrix.skeletonToGame(vertices[v1], vertices[v1 + 1]);
				const x1 = p.x, y1 = p.y;
				p = matrix.skeletonToGame(vertices[v2], vertices[v2 + 1]);
				const x2 = p.x, y2 = p.y;
				p = matrix.skeletonToGame(vertices[v3], vertices[v3 + 1]);
				const x3 = p.x, y3 = p.y;
				this.triangle(x1, y1, x2, y2, x3, y3);
			}

			// mesh hulls
			if (hullLength > 0) {
				this.setColor(0, 0, 1, 0.5);
				hullLength = (hullLength >> 1) * 2;
				let p = matrix.skeletonToGame(vertices[hullLength - 2], vertices[hullLength - 1]);
				let lastX = p.x, lastY = p.y;
				for (let ii = 0, nn = hullLength; ii < nn; ii += 2) {
					p = matrix.skeletonToGame(vertices[ii], vertices[ii + 1]);
					const x1 = p.x, y1 = p.y;
					this.line(x1, y1, lastX, lastY);
					lastX = x1;
					lastY = y1;
				}
			}
		}

		// paths
		for (let i = 0, n = slots.length; i < n; i++) {
			const slot = slots[i];
			if (!slot.bone.active) continue;
			const attachment = slot.appliedPose.attachment;
			if (!(attachment instanceof PathAttachment)) continue;
			let nn = attachment.worldVerticesLength;
			const world = this.tempArray = Utils.setArraySize(this.tempArray, nn, 0);
			attachment.computeWorldVertices(skeleton, slot, 0, nn, world, 0, 2);
			let p = matrix.skeletonToGame(world[2], world[3]);
			let x1 = p.x, y1 = p.y, x2 = 0, y2 = 0;
			if (attachment.closed) {
				this.setColor(1, 0.5, 0, 1);
				p = matrix.skeletonToGame(world[0], world[1]);
				const cx1 = p.x, cy1 = p.y;
				p = matrix.skeletonToGame(world[nn - 2], world[nn - 1]);
				const cx2 = p.x, cy2 = p.y;
				p = matrix.skeletonToGame(world[nn - 4], world[nn - 3]);
				x2 = p.x;
				y2 = p.y;
				this.curve(x1, y1, cx1, cy1, cx2, cy2, x2, y2, 32);
				this.setColor(.75, .75, .75, 1);
				this.line(x1, y1, cx1, cy1);
				this.line(x2, y2, cx2, cy2);
			}
			nn -= 4;
			for (let ii = 4; ii < nn; ii += 6) {
				p = matrix.skeletonToGame(world[ii], world[ii + 1]);
				const cx1 = p.x, cy1 = p.y;
				p = matrix.skeletonToGame(world[ii + 2], world[ii + 3]);
				const cx2 = p.x, cy2 = p.y;
				p = matrix.skeletonToGame(world[ii + 4], world[ii + 5]);
				x2 = p.x;
				y2 = p.y;
				this.setColor(1, 0.5, 0, 1);
				this.curve(x1, y1, cx1, cy1, cx2, cy2, x2, y2, 32);
				this.setColor(.75, .75, .75, 1);
				this.line(x1, y1, cx1, cy1);
				this.line(x2, y2, cx2, cy2);
				x1 = x2;
				y1 = y2;
			}
		}

		// clipping
		this.setColor(0.8, 0, 0, 1);
		for (let i = 0, n = slots.length; i < n; i++) {
			const slot = slots[i];
			if (!slot.bone.active) continue;
			const attachment = slot.appliedPose.attachment;
			if (!(attachment instanceof ClippingAttachment)) continue;
			const nn = attachment.worldVerticesLength;
			const world = this.tempArray = Utils.setArraySize(this.tempArray, nn, 0);
			attachment.computeWorldVertices(skeleton, slot, 0, nn, world, 0, 2);
			for (let i = 0, n = world.length; i < n; i += 2) {
				let p = matrix.skeletonToGame(world[i], world[i + 1]);
				const x1 = p.x, y1 = p.y;
				p = matrix.skeletonToGame(world[(i + 2) % world.length], world[(i + 3) % world.length]);
				const x2 = p.x, y2 = p.y;
				this.line(x1, y1, x2, y2);
			}
		}

		this.renderGameObjectBounds(x, y, quad, false);
	}

	private getTransformedPositions (command: RenderCommand, commandIndex: number, transformPositions: boolean) {
		const length = command.numVertices * 3;
		let cache = this.transformedPositions[commandIndex];
		if (!cache || cache.buffer.length < length) {
			const buffer = new Float32Array(length);
			cache = { buffer, positions: buffer };
			this.transformedPositions[commandIndex] = cache;
			transformPositions = true;
		}

		if (cache.positions.length !== length) {
			cache.positions = cache.buffer.length === length
				? cache.buffer
				: cache.buffer.subarray(0, length);
			transformPositions = true;
		}

		if (transformPositions) {
			const {
				renderXAxisX, renderXAxisY, renderXAxisZ,
				renderYAxisX, renderYAxisY, renderYAxisZ,
				renderZAxisX, renderZAxisY, renderZAxisZ,
				tx, ty, tz,
			} = this.matrix;
			const source = command.positions;
			const destination = cache.positions;
			for (let i = 0; i < length; i += 3) {
				const x = source[i];
				const y = source[i + 1];
				const z = source[i + 2];
				destination[i] = renderXAxisX * x + renderYAxisX * y + renderZAxisX * z + tx;
				destination[i + 1] = renderXAxisY * x + renderYAxisY * y + renderZAxisY * z + ty;
				destination[i + 2] = renderXAxisZ * x + renderYAxisZ * y + renderZAxisZ * z + tz;
			}
		}
		return cache.positions;
	}

	protected abstract setColor (r: number, g: number, b: number, a: number): void;
	protected abstract setColorFillMode (): void;
	protected abstract setBlendMode (): void;
	protected abstract poly (points: number[]): void;
	protected abstract lineInternal (x: number, y: number, x2: number, y2: number): void;
	protected line (x: number, y: number, x2: number, y2: number, offsetX = 0, offsetY = 0) {
		this.lineInternal(x + offsetX, y + offsetY, x2 + offsetX, y2 + offsetY);
	};

	protected abstract renderSkeleton (vertices: Float32Array, uvs: Float32Array, indices: Uint16Array, colors: Float32Array, texture: Texture, blendMode: BlendMode): void;
	public abstract renderGameObjectBounds (x: number, y: number, quad: DOMQuad | SDK.Quad, spriteBody: boolean): void;

	protected circle (x: number, y: number, radius: number) {
		let segments = Math.max(1, (6 * MathUtils.cbrt(radius)) | 0);
		if (segments <= 0) throw new Error("segments must be > 0.");
		const angle = 2 * MathUtils.PI / segments;
		const cos = Math.cos(angle);
		const sin = Math.sin(angle);
		let cx = radius, cy = 0;
		segments--;
		const poly = [];
		for (let i = 0; i < segments; i++) {
			poly.push(x, y);
			poly.push(x + cx, y + cy);
			const temp = cx;
			cx = cos * cx - sin * cy;
			cy = sin * temp + cos * cy;
			poly.push(x + cx, y + cy);
		}
		poly.push(x, y, x + cx, y + cy);
		cx = radius;
		cy = 0;
		poly.push(x + cx, y + cy);
		return poly;
	}

	protected triangle (x: number, y: number, x2: number, y2: number, x3: number, y3: number, offsetX = 0, offsetY = 0) {
		this.line(x, y, x2, y2, offsetX, offsetY);
		this.line(x2, y2, x3, y3, offsetX, offsetY);
		this.line(x3, y3, x, y, offsetX, offsetY);
	}

	protected curve (x1: number, y1: number, cx1: number, cy1: number, cx2: number, cy2: number, x2: number, y2: number, segments: number, offsetX = 0, offsetY = 0) {
		x1 += offsetX;
		y1 += offsetY;
		cx1 += offsetX;
		cy1 += offsetY;
		x2 += offsetX;
		y2 += offsetY;
		cx2 += offsetX;
		cy2 += offsetY;

		// Algorithm from: http://www.antigrain.com/research/bezier_interpolation/index.html#PAGE_BEZIER_INTERPOLATION
		const subdiv_step = 1 / segments;
		const subdiv_step2 = subdiv_step * subdiv_step;
		const subdiv_step3 = subdiv_step * subdiv_step * subdiv_step;

		const pre1 = 3 * subdiv_step;
		const pre2 = 3 * subdiv_step2;
		const pre4 = 6 * subdiv_step2;
		const pre5 = 6 * subdiv_step3;

		const tmp1x = x1 - cx1 * 2 + cx2;
		const tmp1y = y1 - cy1 * 2 + cy2;

		const tmp2x = (cx1 - cx2) * 3 - x1 + x2;
		const tmp2y = (cy1 - cy2) * 3 - y1 + y2;

		let fx = x1;
		let fy = y1;

		let dfx = (cx1 - x1) * pre1 + tmp1x * pre2 + tmp2x * subdiv_step3;
		let dfy = (cy1 - y1) * pre1 + tmp1y * pre2 + tmp2y * subdiv_step3;

		let ddfx = tmp1x * pre4 + tmp2x * pre5;
		let ddfy = tmp1y * pre4 + tmp2y * pre5;

		const dddfx = tmp2x * pre5;
		const dddfy = tmp2y * pre5;

		while (segments-- > 0) {
			this.line(fx, fy, fx + dfx, fy + dfy);
			fx += dfx;
			fy += dfy;
			dfx += ddfx;
			dfy += ddfy;
			ddfx += dddfx;
			ddfy += dddfy;
		}
		this.line(fx, fy, x2, y2);
	}

}

export class C3RendererRuntime extends C3SkeletonRenderer<IRenderer, C3TextureRuntime> {
	protected setColor (r: number, g: number, b: number, a: number): void {
		this.renderer.setColor([r, g, b, a]);
	}

	protected setColorFillMode (): void {
		this.renderer.setColorFillMode();
	}

	protected setBlendMode (blendMode: BlendModeParameter = "normal"): void {
		this.renderer.setBlendMode(blendMode);
	}

	protected poly (points: number[]): void {
		this.renderer.convexPoly(points);
	}

	protected lineInternal (x: number, y: number, x2: number, y2: number): void {
		this.renderer.line(x, y, x2, y2);
	}

	protected renderSkeleton (vertices: Float32Array, uvs: Float32Array, indices: Uint16Array, colors: Float32Array, texture: C3TextureRuntime, blendMode: BlendMode) {
		this.renderer.setBlendMode(BlendingModeSpineToC3[blendMode]);
		this.renderer.setTextureFillMode();
		this.renderer.setTexture(texture.texture);
		this.renderer.drawMesh(vertices, uvs, indices, colors);
	};

	public renderDragHandles (bone: Bone, radius: number) {
		const boneApplied = bone.appliedPose;
		const { x: x1, y: y1 } = this.matrix.skeletonToGame(boneApplied.worldX, boneApplied.worldY);
		this.renderer.setColorFillMode();
		this.renderer.setColor([1, 0, 0, .2]);
		this.renderer.convexPoly(this.circle(x1, y1, radius));
	}

	public renderGameObjectBounds (x: number, y: number, quad: DOMQuad) {
		const { renderer, matrix } = this;
		renderer.setAlphaBlendMode();
		renderer.setColorFillMode();
		renderer.setColorRgba(0.25, 0, 0, 0.25);
		renderer.lineQuad(quad);
		renderer.line(x, y, matrix.tx, matrix.ty);
	};
}

export class C3RendererEditor extends C3SkeletonRenderer<SDK.Gfx.IWebGLRenderer, C3TextureEditor> {
	protected setColor (r: number, g: number, b: number, a: number): void {
		this.renderer.SetColorRgba(r, g, b, a);
	}

	protected setColorFillMode (): void {
		this.renderer.SetColorFillMode();
	}

	protected setBlendMode (blendMode: BlendModeParameter = "normal"): void {
		this.renderer.SetBlendMode(blendMode);
	}

	protected poly (points: number[]): void {
		this.renderer.ConvexPoly(points);
	}

	protected lineInternal (x: number, y: number, x2: number, y2: number): void {
		this.renderer.Line(x, y, x2, y2);
	}

	protected renderSkeleton (vertices: Float32Array, uvs: Float32Array, indices: Uint16Array, colors: Float32Array, texture: C3TextureEditor, blendMode: BlendMode) {
		this.renderer.SetBlendMode(BlendingModeSpineToC3[blendMode]);
		this.renderer.SetTextureFillMode();
		this.renderer.SetTexture(texture.texture);
		this.renderer.DrawMesh(vertices, uvs, indices, colors);
	};

	public renderGameObjectBounds (x: number, y: number, quad: SDK.Quad, spriteBody: boolean): void {
		const { renderer, matrix } = this;
		renderer.SetAlphaBlend();
		renderer.SetColorFillMode();
		if (spriteBody)
			renderer.SetColorRgba(0, 0, 0.25, 0.25);
		else
			renderer.SetColorRgba(0.25, 0, 0, 0.25);
		renderer.LineQuad(quad);
		renderer.Line(x, y, matrix.tx, matrix.ty);
	}
}