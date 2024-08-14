/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import { NumberArrayLike, Color, SkeletonClipping, Vector2, Utils, Skeleton, BlendMode, RegionAttachment, TextureAtlasRegion, MeshAttachment, ClippingAttachment } from "@esotericsoftware/spine-core";
import { GLTexture } from "./GLTexture.js";
import { PolygonBatcher } from "./PolygonBatcher.js";
import { ManagedWebGLRenderingContext } from "./WebGL.js";


class Renderable {
	constructor (public vertices: NumberArrayLike, public numVertices: number, public numFloats: number) { }
};

export type VertexTransformer = (vertices: NumberArrayLike, numVertices: number, stride: number) => void;

export class SkeletonRenderer {
	static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];

	premultipliedAlpha = false;
	private tempColor = new Color();
	private tempColor2 = new Color();
	private vertices: NumberArrayLike;
	private vertexSize = 2 + 2 + 4;
	private twoColorTint = false;
	private renderable: Renderable = new Renderable([], 0, 0);
	private clipper: SkeletonClipping = new SkeletonClipping();
	private temp = new Vector2();
	private temp2 = new Vector2();
	private temp3 = new Color();
	private temp4 = new Color();

	constructor (context: ManagedWebGLRenderingContext, twoColorTint: boolean = true) {
		this.twoColorTint = twoColorTint;
		if (twoColorTint)
			this.vertexSize += 4;
		this.vertices = Utils.newFloatArray(this.vertexSize * 1024);
	}

	draw (batcher: PolygonBatcher, skeleton: Skeleton, slotRangeStart: number = -1, slotRangeEnd: number = -1, transformer: VertexTransformer | null = null) {
		let clipper = this.clipper;
		let premultipliedAlpha = this.premultipliedAlpha;
		let twoColorTint = this.twoColorTint;
		let blendMode: BlendMode | null = null;

		let renderable: Renderable = this.renderable;
		let uvs: NumberArrayLike;
		let triangles: Array<number>;
		let drawOrder = skeleton.drawOrder;
		let attachmentColor: Color;
		let skeletonColor = skeleton.color;
		let vertexSize = twoColorTint ? 12 : 8;
		let inRange = false;
		if (slotRangeStart == -1) inRange = true;
		for (let i = 0, n = drawOrder.length; i < n; i++) {
			let clippedVertexSize = clipper.isClipping() ? 2 : vertexSize;
			let slot = drawOrder[i];
			if (!slot.bone.active) {
				clipper.clipEndWithSlot(slot);
				continue;
			}

			if (slotRangeStart >= 0 && slotRangeStart == slot.data.index) {
				inRange = true;
			}

			if (!inRange) {
				clipper.clipEndWithSlot(slot);
				continue;
			}

			if (slotRangeEnd >= 0 && slotRangeEnd == slot.data.index) {
				inRange = false;
			}

			let attachment = slot.getAttachment();
			let texture: GLTexture;
			if (attachment instanceof RegionAttachment) {
				let region = <RegionAttachment>attachment;
				renderable.vertices = this.vertices;
				renderable.numVertices = 4;
				renderable.numFloats = clippedVertexSize << 2;
				region.computeWorldVertices(slot, renderable.vertices, 0, clippedVertexSize);
				triangles = SkeletonRenderer.QUAD_TRIANGLES;
				uvs = region.uvs;
				texture = <GLTexture>region.region!.texture;
				attachmentColor = region.color;
			} else if (attachment instanceof MeshAttachment) {
				let mesh = <MeshAttachment>attachment;
				renderable.vertices = this.vertices;
				renderable.numVertices = (mesh.worldVerticesLength >> 1);
				renderable.numFloats = renderable.numVertices * clippedVertexSize;
				if (renderable.numFloats > renderable.vertices.length) {
					renderable.vertices = this.vertices = Utils.newFloatArray(renderable.numFloats);
				}
				mesh.computeWorldVertices(slot, 0, mesh.worldVerticesLength, renderable.vertices, 0, clippedVertexSize);
				triangles = mesh.triangles;
				texture = <GLTexture>mesh.region!.texture;
				uvs = mesh.uvs;
				attachmentColor = mesh.color;
			} else if (attachment instanceof ClippingAttachment) {
				let clip = <ClippingAttachment>(attachment);
				clipper.clipStart(slot, clip);
				continue;
			} else {
				clipper.clipEndWithSlot(slot);
				continue;
			}

			if (texture) {
				let slotColor = slot.color;
				let finalColor = this.tempColor;
				finalColor.r = skeletonColor.r * slotColor.r * attachmentColor.r;
				finalColor.g = skeletonColor.g * slotColor.g * attachmentColor.g;
				finalColor.b = skeletonColor.b * slotColor.b * attachmentColor.b;
				finalColor.a = skeletonColor.a * slotColor.a * attachmentColor.a;
				if (premultipliedAlpha) {
					finalColor.r *= finalColor.a;
					finalColor.g *= finalColor.a;
					finalColor.b *= finalColor.a;
				}
				let darkColor = this.tempColor2;
				if (!slot.darkColor)
					darkColor.set(0, 0, 0, 1.0);
				else {
					if (premultipliedAlpha) {
						darkColor.r = slot.darkColor.r * finalColor.a;
						darkColor.g = slot.darkColor.g * finalColor.a;
						darkColor.b = slot.darkColor.b * finalColor.a;
					} else {
						darkColor.setFromColor(slot.darkColor);
					}
					darkColor.a = premultipliedAlpha ? 1.0 : 0.0;
				}

				let slotBlendMode = slot.data.blendMode;
				if (slotBlendMode != blendMode) {
					blendMode = slotBlendMode;
					batcher.setBlendMode(blendMode, premultipliedAlpha);
				}

				if (clipper.isClipping()) {
					clipper.clipTriangles(renderable.vertices, triangles, triangles.length, uvs, finalColor, darkColor, twoColorTint);
					let clippedVertices = new Float32Array(clipper.clippedVertices);
					let clippedTriangles = clipper.clippedTriangles;
					if (transformer) transformer(clippedVertices, clippedVertices.length, vertexSize);
					batcher.draw(texture, clippedVertices, clippedTriangles);
				} else {
					let verts = renderable.vertices;
					if (!twoColorTint) {
						for (let v = 2, u = 0, n = renderable.numFloats; v < n; v += vertexSize, u += 2) {
							verts[v] = finalColor.r;
							verts[v + 1] = finalColor.g;
							verts[v + 2] = finalColor.b;
							verts[v + 3] = finalColor.a;
							verts[v + 4] = uvs[u];
							verts[v + 5] = uvs[u + 1];
						}
					} else {
						for (let v = 2, u = 0, n = renderable.numFloats; v < n; v += vertexSize, u += 2) {
							verts[v] = finalColor.r;
							verts[v + 1] = finalColor.g;
							verts[v + 2] = finalColor.b;
							verts[v + 3] = finalColor.a;
							verts[v + 4] = uvs[u];
							verts[v + 5] = uvs[u + 1];
							verts[v + 6] = darkColor.r;
							verts[v + 7] = darkColor.g;
							verts[v + 8] = darkColor.b;
							verts[v + 9] = darkColor.a;
						}
					}
					let view = (renderable.vertices as Float32Array).subarray(0, renderable.numFloats);
					if (transformer) transformer(renderable.vertices, renderable.numFloats, vertexSize);
					batcher.draw(texture, view, triangles);
				}
			}

			clipper.clipEndWithSlot(slot);
		}

		// console.log(renderable.vertices[1])
		clipper.clipEnd();
	}

	/** Returns the {@link SkeletonClipping} used by this renderer for use with e.g. {@link Skeleton.getBounds} **/
	public getSkeletonClipping (): SkeletonClipping {
		return this.clipper;
	}
}
