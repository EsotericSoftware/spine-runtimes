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

import { ClippingAttachment, MeshAttachment, RegionAttachment } from "./attachments";
import type { Skeleton } from "./Skeleton";
import { SkeletonClipping } from "./SkeletonClipping";
import { BlendMode } from "./SlotData";
import type { Color, NumberArrayLike } from "./Utils";

export class SkeletonRendererCore {
	private commandPool = new CommandPool();
	private worldVertices = new Float32Array(12 * 1024);
	private quadIndices = new Uint32Array([0, 1, 2, 2, 3, 0]);
	private clipping = new SkeletonClipping();
	private renderCommands: RenderCommand[] = [];

	render (skeleton: Skeleton): RenderCommand | undefined {
		this.commandPool.reset();
		this.renderCommands.length = 0;

		const clipper = this.clipping;

		for (let i = 0; i < skeleton.slots.length; i++) {
			const slot = skeleton.drawOrder[i];
			const attachment = slot.applied.attachment;

			if (!attachment) {
				clipper.clipEnd(slot);
				continue;
			}

			const slotApplied = slot.applied;
			const color = slotApplied.color;
			const alpha = color.a;
			if ((alpha === 0 || !slot.bone.active) && !(attachment instanceof ClippingAttachment)) {
				clipper.clipEnd(slot);
				continue;
			}

			let vertices: NumberArrayLike;
			let verticesCount: number;
			let uvs: NumberArrayLike;
			let indices: number[] | Uint32Array;
			let indicesCount: number;
			let attachmentColor: Color;
			// biome-ignore lint/suspicious/noExplicitAny: texture depends on the runtime
			let texture: any;

			if (attachment instanceof RegionAttachment) {
				attachmentColor = attachment.color;

				if (attachmentColor.a === 0) {
					clipper.clipEnd(slot);
					continue;
				}

				attachment.computeWorldVertices(slot, this.worldVertices, 0, 2);
				vertices = this.worldVertices;
				verticesCount = 4;
				uvs = attachment.uvs as Float32Array;
				indices = this.quadIndices;
				indicesCount = 6;
				texture = attachment.region?.texture;

			} else if (attachment instanceof MeshAttachment) {
				attachmentColor = attachment.color;

				if (attachmentColor.a === 0) {
					clipper.clipEnd(slot);
					continue;
				}

				if (this.worldVertices.length < attachment.worldVerticesLength)
					this.worldVertices = new Float32Array(attachment.worldVerticesLength);

				attachment.computeWorldVertices(skeleton, slot, 0, attachment.worldVerticesLength, this.worldVertices, 0, 2);
				vertices = this.worldVertices;
				verticesCount = attachment.worldVerticesLength >> 1;
				uvs = attachment.uvs as Float32Array;
				indices = attachment.triangles;
				indicesCount = indices.length;
				texture = attachment.region?.texture;

			} else if (attachment instanceof ClippingAttachment) {
				clipper.clipStart(skeleton, slot, attachment);
				continue;
			} else {
				continue;
			}

			const skelColor = skeleton.color;
			const r = Math.floor(skelColor.r * slotApplied.color.r * attachmentColor.r * 255);
			const g = Math.floor(skelColor.g * slotApplied.color.g * attachmentColor.g * 255);
			const b = Math.floor(skelColor.b * slotApplied.color.b * attachmentColor.b * 255);
			const a = Math.floor(skelColor.a * slotApplied.color.a * attachmentColor.a * 255);

			let darkColor = 0xff000000;
			if (slotApplied.darkColor) {
				const { r, g, b } = slotApplied.darkColor;
				darkColor = 0xff000000 |
					(Math.floor(r * 255) << 16) |
					(Math.floor(g * 255) << 8) |
					Math.floor(b * 255);
			}

			if (clipper.isClipping()) {
				clipper.clipTrianglesUnpacked(vertices, indices, indicesCount, uvs);
				vertices = clipper.clippedVerticesTyped;
				verticesCount = clipper.clippedVerticesLength >> 1;
				uvs = clipper.clippedUVsTyped;
				indices = clipper.clippedTrianglesTyped;
				indicesCount = clipper.clippedTrianglesLength;
			}

			const cmd = this.commandPool.getCommand(verticesCount, indicesCount);
			cmd.blendMode = slot.data.blendMode;
			cmd.texture = texture;

			cmd.positions.set(vertices.subarray(0, verticesCount << 1));
			cmd.uvs.set(uvs.subarray(0, verticesCount << 1));

			for (let j = 0; j < verticesCount; j++) {
				cmd.colors[j] = (a << 24) | (r << 16) | (g << 8) | b;
				cmd.darkColors[j] = darkColor;
			}

			if (indices instanceof Uint16Array) {
				cmd.indices.set(indices.subarray(0, indicesCount));
			} else {
				cmd.indices.set(indices.slice(0, indicesCount));
			}

			this.renderCommands.push(cmd);
			clipper.clipEnd(slot);
		}

		clipper.clipEnd();
		return this.batchCommands();
	}

	private batchSubCommands (commands: RenderCommand[], first: number, last: number,
		numVertices: number, numIndices: number): RenderCommand {

		const firstCmd = commands[first];
		const batched = this.commandPool.getCommand(numVertices, numIndices);

		batched.blendMode = firstCmd.blendMode;
		batched.texture = firstCmd.texture;

		let positionsOffset = 0;
		let uvsOffset = 0;
		let colorsOffset = 0;
		let indicesOffset = 0;
		let vertexOffset = 0;

		for (let i = first; i <= last; i++) {
			const cmd = commands[i];

			batched.positions.set(cmd.positions, positionsOffset);
			positionsOffset += cmd.numVertices << 1;

			batched.uvs.set(cmd.uvs, uvsOffset);
			uvsOffset += cmd.numVertices << 1;

			batched.colors.set(cmd.colors, colorsOffset);
			batched.darkColors.set(cmd.darkColors, colorsOffset);
			colorsOffset += cmd.numVertices;

			// cannot fast copy - indices need vertex offset adjustment
			for (let j = 0; j < cmd.numIndices; j++)
				batched.indices[indicesOffset + j] = cmd.indices[j] + vertexOffset;

			indicesOffset += cmd.numIndices;
			vertexOffset += cmd.numVertices;
		}

		return batched;
	}

	private batchCommands (): RenderCommand | undefined {
		if (this.renderCommands.length === 0) return undefined;

		let root: RenderCommand | undefined;
		let last: RenderCommand | undefined;

		let first = this.renderCommands[0];
		let startIndex = 0;
		let i = 1;
		let numVertices = first.numVertices;
		let numIndices = first.numIndices;

		while (i <= this.renderCommands.length) {
			const cmd = i < this.renderCommands.length ? this.renderCommands[i] : null;

			if (cmd && cmd.numVertices === 0 && cmd.numIndices === 0) {
				i++;
				continue;
			}

			const canBatch = cmd !== null &&
				cmd.texture === first.texture &&
				cmd.blendMode === first.blendMode &&
				cmd.colors[0] === first.colors[0] &&
				cmd.darkColors[0] === first.darkColors[0] &&
				numIndices + cmd.numIndices < 0xffff;
			if (canBatch) {
				numVertices += cmd.numVertices;
				numIndices += cmd.numIndices;
			} else {
				const batched = this.batchSubCommands(this.renderCommands, startIndex, i - 1,
					numVertices, numIndices);

				if (!last) {
					root = last = batched;
				} else {
					last.next = batched;
					last = batched;
				}

				if (i === this.renderCommands.length) break;

				first = this.renderCommands[i];
				startIndex = i;
				numVertices = first.numVertices;
				numIndices = first.numIndices;
			}
			i++;
		}

		return root;
	}
}

interface RenderCommand {
	positions: Float32Array;
	uvs: Float32Array;
	colors: Uint32Array;
	darkColors: Uint32Array;
	indices: Uint16Array;
	_positions: Float32Array;
	_uvs: Float32Array;
	_colors: Uint32Array;
	_darkColors: Uint32Array;
	_indices: Uint16Array;
	numVertices: number;
	numIndices: number;
	blendMode: BlendMode;
	// biome-ignore lint/suspicious/noExplicitAny: texture depends on the runtime
	texture: any;
	next?: RenderCommand;
}

class CommandPool {
	private pool: RenderCommand[] = [];
	private inUse: RenderCommand[] = [];

	getCommand (numVertices: number, numIndices: number): RenderCommand {
		let cmd: RenderCommand | undefined;
		for (const c of this.pool) {
			if (c._positions.length >= numVertices << 1 && c._indices.length >= numIndices) {
				cmd = c;
				break;
			}
		}

		if (!cmd) {
			const _positions = new Float32Array(numVertices << 1);
			const _uvs = new Float32Array(numVertices << 1);
			const _colors = new Uint32Array(numVertices);
			const _darkColors = new Uint32Array(numVertices);
			const _indices = new Uint16Array(numIndices);
			cmd = {
				positions: _positions,
				uvs: _uvs,
				colors: _colors,
				darkColors: _darkColors,
				indices: _indices,
				_positions,
				_uvs,
				_colors,
				_darkColors,
				_indices,
				numVertices,
				numIndices,
				blendMode: BlendMode.Normal,
				texture: null
			};
		} else {
			this.pool.splice(this.pool.indexOf(cmd), 1);
			cmd.next = undefined;
			cmd.numVertices = numVertices;
			cmd.numIndices = numIndices;

			cmd.positions = cmd._positions.subarray(0, numVertices << 1);
			cmd.uvs = cmd._uvs.subarray(0, numVertices * 2);
			cmd.colors = cmd._colors.subarray(0, numVertices);
			cmd.darkColors = cmd._darkColors.subarray(0, numVertices);
			cmd.indices = cmd._indices.subarray(0, numIndices);
		}

		this.inUse.push(cmd);
		return cmd;
	}

	reset (): void {
		this.pool.push(...this.inUse);
		this.inUse.length = 0;
	}
}