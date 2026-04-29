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

import type { ClippingAttachment } from "./attachments/ClippingAttachment.js";
import type { Skeleton } from "./Skeleton.js";
import type { Slot } from "./Slot.js";
import { Triangulator } from "./Triangulator.js";
import { type Color, type NumberArrayLike, Utils } from "./Utils.js";

export class SkeletonClipping {
	private triangulator: Triangulator | null = null;
	private clippingPolygon = [] as number[];
	private clippingPolygons: number[][] = [];
	private clipOutput = [] as number[];
	clippedVertices = [] as number[];

	/** An empty array unless {@link clipTrianglesUnpacked} was used. **/
	clippedUVs = [] as number[];

	clippedTriangles = [] as number[];
	inverseVertices = [] as number[];

	_clippedVerticesTyped = new Float32Array(1024);
	_clippedUVsTyped = new Float32Array(1024);
	_clippedTrianglesTyped = new Uint16Array(1024);
	clippedVerticesTyped = new Float32Array(0);
	clippedUVsTyped = new Float32Array(0);
	clippedTrianglesTyped = new Uint16Array(0);
	clippedVerticesLength = 0;
	clippedUVsLength = 0;
	clippedTrianglesLength = 0;

	private scratch = [] as number[];
	private inverse = false;

	private clipAttachment: ClippingAttachment | null = null;

	clipStart (skeleton: Skeleton, slot: Slot, clip: ClippingAttachment): void {
		if (this.clipAttachment) return;
		const n = clip.worldVerticesLength;
		this.clipAttachment = clip;
		this.inverse = clip.inverse;

		const vertices = Utils.setArraySize(this.clippingPolygon, n);
		clip.computeWorldVertices(skeleton, slot, 0, n, vertices, 0, 2);
		const clippingPolygon = this.clippingPolygon;
		const convex = this.makeClockwise(clippingPolygon);

		if (convex || this.inverse || clip.convex) {
			if (!convex) this.makeConvex(clippingPolygon);
			this.clippingPolygon.push(clippingPolygon[0], clippingPolygon[1]);
			this.clippingPolygons.push(clippingPolygon);
		} else {
			if (this.triangulator === null) this.triangulator = new Triangulator();
			this.clippingPolygons.push(...this.triangulator.decompose(clippingPolygon, this.triangulator.triangulate(clippingPolygon)));
		}
	}

	clipEnd (slot?: Slot) {
		if (!this.clipAttachment) return;
		if (slot && this.clipAttachment.endSlot !== slot.data) return;
		this.clipAttachment = null;
		this.clippingPolygons.length = 0;
	}

	isClipping (): boolean {
		return this.clipAttachment != null;
	}

	clipTriangles (vertices: NumberArrayLike, triangles: NumberArrayLike, trianglesLength: number): boolean;
	clipTriangles (vertices: NumberArrayLike, triangles: NumberArrayLike, trianglesLength: number,
		uvs: NumberArrayLike, light: Color, dark: Color, twoColor: boolean, stride: number): boolean;
	clipTriangles (vertices: NumberArrayLike, triangles: NumberArrayLike, trianglesLength: number,
		uvs?: NumberArrayLike, light?: Color, dark?: Color, twoColor?: boolean, stride?: number): boolean {

		return (uvs && light && dark && typeof twoColor === 'boolean' && typeof stride === 'number')
			? this.clipTrianglesRender(vertices, triangles, trianglesLength, uvs, light, dark, twoColor, stride)
			: this.clipTrianglesNoRender(vertices, triangles, trianglesLength);
	}

	private clipTrianglesNoRender (vertices: NumberArrayLike, triangles: NumberArrayLike, trianglesLength: number): boolean {
		const clippedVertices = this.clippedVertices;
		clippedVertices.length = 0;
		const clippedTriangles = this.clippedTriangles;
		clippedTriangles.length = 0;
		let index = 0;

		if (this.inverse) {
			const polygon = this.clippingPolygons[0];
			for (let i = 0; i < trianglesLength; i += 3) {
				let t = triangles[i] << 1;
				const x1 = vertices[t], y1 = vertices[t + 1];
				t = triangles[i + 1] << 1;
				const x2 = vertices[t], y2 = vertices[t + 1];
				t = triangles[i + 2] << 1;
				const x3 = vertices[t], y3 = vertices[t + 1];
				this.clipInverse(x1, y1, x2, y2, x3, y3, polygon);

				const iv = this.inverseVertices;
				for (let offset = 0, nn = this.inverseVertices.length; offset < nn;) {
					const polygonSize = iv[offset++];
					let vertexCount = polygonSize >> 1, s = clippedVertices.length;

					const cv = Utils.setArraySize(clippedVertices, s + polygonSize);
					Utils.arrayCopy(iv, offset, cv, s, polygonSize);

					s = clippedTriangles.length;
					const ct = Utils.setArraySize(clippedTriangles, s + 3 * (vertexCount - 2));
					for (let ii = 1; ii < vertexCount - 1; ii++, s += 3) {
						ct[s] = index;
						ct[s + 1] = index + ii;
						ct[s + 2] = index + ii + 1;
					}
					index += vertexCount;
					offset += polygonSize;
				}
			}
			return true;
		}

		const clipOutput = this.clipOutput;
		const polygons = this.clippingPolygons;
		const polygonsCount = polygons.length;
		let clipOutputItems = null;
		for (let i = 0; i < trianglesLength; i += 3) {
			let t = triangles[i] << 1;
			const x1 = vertices[t], y1 = vertices[t + 1];
			t = triangles[i + 1] << 1;
			const x2 = vertices[t], y2 = vertices[t + 1];
			t = triangles[i + 2] << 1;
			const x3 = vertices[t], y3 = vertices[t + 1];
			for (let p = 0; p < polygonsCount; p++) {
				let s = clippedVertices.length;
				if (this.clip(x1, y1, x2, y2, x3, y3, polygons[p])) {
					clipOutputItems = this.clipOutput;
					const clipOutputLength = clipOutput.length;
					if (clipOutputLength === 0) continue;
					let clipOutputCount = clipOutputLength >> 1;

					const cv = Utils.setArraySize(clippedVertices, s + clipOutputLength);
					Utils.arrayCopy(clipOutputItems, 0, cv, s, clipOutputLength);

					s = clippedTriangles.length;
					const ct = Utils.setArraySize(clippedTriangles, s + 3 * (clipOutputCount - 2));
					clipOutputCount--;
					for (let ii = 1; ii < clipOutputCount; ii++, s += 3) {
						ct[s] = index;
						ct[s + 1] = (index + ii);
						ct[s + 2] = (index + ii + 1);
					}
					index += clipOutputCount;
				} else {
					const cv = Utils.setArraySize(clippedVertices, s + 3 * 2);
					cv[s] = x1;
					cv[s + 1] = y1;
					cv[s + 2] = x2;
					cv[s + 3] = y2;
					cv[s + 4] = x3;
					cv[s + 5] = y3;

					s = clippedTriangles.length;
					const ct = Utils.setArraySize(clippedTriangles, s + 3);
					ct[s] = index;
					ct[s + 1] = (index + 1);
					ct[s + 2] = (index + 2);
					index += 3;
					break;
				}
			}
		}
		return clipOutputItems != null;
	}

	private clipTrianglesRender (vertices: NumberArrayLike, triangles: NumberArrayLike, trianglesLength: number, uvs: NumberArrayLike, light: Color, dark: Color,
		twoColor: boolean, stride: number): boolean {
		const clippedVertices = this.clippedVertices;
		clippedVertices.length = 0;
		const clippedTriangles = this.clippedTriangles;
		clippedTriangles.length = 0;
		let index = 0;

		if (this.inverse) {
			const polygon = this.clippingPolygons[0];
			for (let i = 0; i < trianglesLength; i += 3) {
				let t0 = triangles[i], t1 = triangles[i + 1], t2 = triangles[i + 2];
				const x1 = vertices[t0 * stride], y1 = vertices[t0 * stride + 1];
				const x2 = vertices[t1 * stride], y2 = vertices[t1 * stride + 1];
				const x3 = vertices[t2 * stride], y3 = vertices[t2 * stride + 1];
				this.clipInverse(x1, y1, x2, y2, x3, y3, polygon);
				const nn = this.inverseVertices.length;
				if (nn === 0) continue;

				const u1 = uvs[t0 <<= 1], v1 = uvs[t0 + 1];
				const u2 = uvs[t1 <<= 1], v2 = uvs[t1 + 1];
				const u3 = uvs[t2 <<= 1], v3 = uvs[t2 + 1];
				const d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1, d = 1 / (d0 * d2 + d1 * (y1 - y3));
				const iv = this.inverseVertices;
				for (let offset = 0; offset < nn;) {
					const polygonSize = iv[offset++];
					const vertexCount = polygonSize >> 1;

					let s = clippedVertices.length;
					const cv = Utils.setArraySize(clippedVertices, s + vertexCount * stride);
					for (let ii = 0; ii < polygonSize; ii += 2, s += stride) {
						const x = iv[offset + ii], y = iv[offset + ii + 1];
						cv[s] = x;
						cv[s + 1] = y;
						cv[s + 2] = light.r;
						cv[s + 3] = light.g;
						cv[s + 4] = light.b;
						cv[s + 5] = light.a;
						const c0 = x - x3, c1 = y - y3, a = (d0 * c0 + d1 * c1) * d, b = (d4 * c0 + d2 * c1) * d, c = 1 - a - b;
						cv[s + 6] = u1 * a + u2 * b + u3 * c;
						cv[s + 7] = v1 * a + v2 * b + v3 * c;
						if (twoColor) {
							cv[s + 8] = dark.r;
							cv[s + 9] = dark.g;
							cv[s + 10] = dark.b;
							cv[s + 11] = dark.a;
						}
					}

					s = clippedTriangles.length;
					const ct = Utils.setArraySize(clippedTriangles, s + 3 * (vertexCount - 2));
					for (let ii = 1; ii < vertexCount - 1; ii++, s += 3) {
						ct[s] = index;
						ct[s + 1] = index + ii;
						ct[s + 2] = index + ii + 1;
					}
					index += vertexCount;
					offset += polygonSize;
				}
			}
			return true;
		}


		const clipOutput = this.clipOutput;
		const polygons = this.clippingPolygons;
		const polygonsCount = this.clippingPolygons.length;
		let clipOutputItems = null;
		for (let i = 0; i < trianglesLength; i += 3) {
			let t = triangles[i];
			const x1 = vertices[t * stride], y1 = vertices[t * stride + 1];
			const u1 = uvs[t << 1], v1 = uvs[(t << 1) + 1];
			t = triangles[i + 1];
			const x2 = vertices[t * stride], y2 = vertices[t * stride + 1];
			const u2 = uvs[t << 1], v2 = uvs[(t << 1) + 1];
			t = triangles[i + 2];
			const x3 = vertices[t * stride], y3 = vertices[t * stride + 1];
			const u3 = uvs[t << 1], v3 = uvs[(t << 1) + 1];
			const d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1, d = 1 / (d0 * d2 + d1 * (y1 - y3));
			for (let p = 0; p < polygonsCount; p++) {
				let s = clippedVertices.length;
				if (this.clip(x1, y1, x2, y2, x3, y3, polygons[p])) {
					clipOutputItems = this.clipOutput;
					const clipOutputLength = clipOutput.length;
					if (clipOutputLength === 0) continue;
					let clipOutputCount = clipOutputLength >> 1;

					const cv = Utils.setArraySize(clippedVertices, s + clipOutputCount * stride);
					for (let ii = 0; ii < clipOutputLength; ii += 2, s += stride) {
						const x = clipOutputItems[ii], y = clipOutputItems[ii + 1];
						cv[s] = x;
						cv[s + 1] = y;
						cv[s + 2] = light.r;
						cv[s + 3] = light.g;
						cv[s + 4] = light.b;
						cv[s + 5] = light.a;
						const c0 = x - x3, c1 = y - y3, a = (d0 * c0 + d1 * c1) * d, b = (d4 * c0 + d2 * c1) * d, c = 1 - a - b;
						cv[s + 6] = u1 * a + u2 * b + u3 * c;
						cv[s + 7] = v1 * a + v2 * b + v3 * c;
						if (twoColor) {
							cv[s + 8] = dark.r;
							cv[s + 9] = dark.g;
							cv[s + 10] = dark.b;
							cv[s + 11] = dark.a;
						}
					}

					s = clippedTriangles.length;
					const ct = Utils.setArraySize(clippedTriangles, s + 3 * (clipOutputCount - 2));
					clipOutputCount--;
					for (let ii = 1; ii < clipOutputCount; ii++, s += 3) {
						ct[s] = index;
						ct[s + 1] = (index + ii);
						ct[s + 2] = (index + ii + 1);
					}
					index += clipOutputCount + 1;
				} else {
					const cv = Utils.setArraySize(clippedVertices, s + 3 * stride);
					cv[s] = x1;
					cv[s + 1] = y1;
					cv[s + 2] = light.r;
					cv[s + 3] = light.g;
					cv[s + 4] = light.b;
					cv[s + 5] = light.a;
					if (!twoColor) {
						cv[s + 6] = u1;
						cv[s + 7] = v1;

						cv[s + 8] = x2;
						cv[s + 9] = y2;
						cv[s + 10] = light.r;
						cv[s + 11] = light.g;
						cv[s + 12] = light.b;
						cv[s + 13] = light.a;
						cv[s + 14] = u2;
						cv[s + 15] = v2;

						cv[s + 16] = x3;
						cv[s + 17] = y3;
						cv[s + 18] = light.r;
						cv[s + 19] = light.g;
						cv[s + 20] = light.b;
						cv[s + 21] = light.a;
						cv[s + 22] = u3;
						cv[s + 23] = v3;
					} else {
						cv[s + 6] = u1;
						cv[s + 7] = v1;
						cv[s + 8] = dark.r;
						cv[s + 9] = dark.g;
						cv[s + 10] = dark.b;
						cv[s + 11] = dark.a;

						cv[s + 12] = x2;
						cv[s + 13] = y2;
						cv[s + 14] = light.r;
						cv[s + 15] = light.g;
						cv[s + 16] = light.b;
						cv[s + 17] = light.a;
						cv[s + 18] = u2;
						cv[s + 19] = v2;
						cv[s + 20] = dark.r;
						cv[s + 21] = dark.g;
						cv[s + 22] = dark.b;
						cv[s + 23] = dark.a;

						cv[s + 24] = x3;
						cv[s + 25] = y3;
						cv[s + 26] = light.r;
						cv[s + 27] = light.g;
						cv[s + 28] = light.b;
						cv[s + 29] = light.a;
						cv[s + 30] = u3;
						cv[s + 31] = v3;
						cv[s + 32] = dark.r;
						cv[s + 33] = dark.g;
						cv[s + 34] = dark.b;
						cv[s + 35] = dark.a;
					}

					s = clippedTriangles.length;
					const ct = Utils.setArraySize(clippedTriangles, s + 3);
					ct[s] = index;
					ct[s + 1] = (index + 1);
					ct[s + 2] = (index + 2);
					index += 3;
					break;
				}
			}
		}
		return clipOutputItems != null;
	}

	public clipTrianglesUnpacked (vertices: NumberArrayLike, vertexStart: number, triangles: NumberArrayLike | Uint16Array, trianglesLength: number, uvs: NumberArrayLike, stride = 2): boolean {
		let clippedVertices = this._clippedVerticesTyped;
		let clippedUVs = this._clippedUVsTyped;
		let clippedTriangles = this._clippedTrianglesTyped;
		let index = 0;
		this.clippedVerticesLength = 0;
		this.clippedUVsLength = 0;
		this.clippedTrianglesLength = 0;

		if (this.inverse) {
			const polygon = this.clippingPolygons[0];
			for (let i = 0; i < trianglesLength; i += 3) {
				let v = triangles[i] * stride;
				const x1 = vertices[vertexStart + v], y1 = vertices[vertexStart + v + 1];
				let uv = triangles[i] << 1;
				const u1 = uvs[uv], v1 = uvs[uv + 1];
				v = triangles[i + 1] * stride;
				const x2 = vertices[vertexStart + v], y2 = vertices[vertexStart + v + 1];
				uv = triangles[i + 1] << 1;
				const u2 = uvs[uv], v2 = uvs[uv + 1];
				v = triangles[i + 2] * stride;
				const x3 = vertices[vertexStart + v], y3 = vertices[vertexStart + v + 1];
				uv = triangles[i + 2] << 1;
				const u3 = uvs[uv], v3 = uvs[uv + 1];
				this.clipInverse(x1, y1, x2, y2, x3, y3, polygon);
				const nn = this.inverseVertices.length;
				if (nn === 0) continue;

				const d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1, d = 1 / (d0 * d2 + d1 * (y1 - y3));
				const iv = this.inverseVertices;
				for (let offset = 0; offset < nn;) {
					const polygonSize = iv[offset++];
					const vertexCount = polygonSize >> 1;

					let s = this.clippedVerticesLength;
					const newLength = s + vertexCount * stride;
					const newUVLength = this.clippedUVsLength + vertexCount * 2;
					if (clippedVertices.length < newLength) {
						this._clippedVerticesTyped = new Float32Array(newLength * 2);
						this._clippedVerticesTyped.set(clippedVertices.subarray(0, s));
						clippedVertices = this._clippedVerticesTyped;
					}
					if (clippedUVs.length < newUVLength) {
						this._clippedUVsTyped = new Float32Array(newUVLength * 2);
						this._clippedUVsTyped.set(clippedUVs.subarray(0, this.clippedUVsLength));
						clippedUVs = this._clippedUVsTyped;
					}
					this.clippedVerticesLength = newLength;
					this.clippedUVsLength = newUVLength;

					const cv = this._clippedVerticesTyped;
					const cu = this._clippedUVsTyped;
					let uvIndex = newUVLength - vertexCount * 2;
					for (let ii = 0; ii < polygonSize; ii += 2, s += stride, uvIndex += 2) {
						const x = iv[offset + ii], y = iv[offset + ii + 1];
						cv[s] = x;
						cv[s + 1] = y;
						const c0 = x - x3, c1 = y - y3, a = (d0 * c0 + d1 * c1) * d, b = (d4 * c0 + d2 * c1) * d, c = 1 - a - b;
						cu[uvIndex] = u1 * a + u2 * b + u3 * c;
						cu[uvIndex + 1] = v1 * a + v2 * b + v3 * c;
					}

					s = this.clippedTrianglesLength;
					const newLengthTriangles = s + 3 * (vertexCount - 2);
					if (clippedTriangles.length < newLengthTriangles) {
						this._clippedTrianglesTyped = new Uint16Array(newLengthTriangles * 2);
						this._clippedTrianglesTyped.set(clippedTriangles.subarray(0, s));
						clippedTriangles = this._clippedTrianglesTyped;
					}
					this.clippedTrianglesLength = newLengthTriangles;
					const ct = clippedTriangles;
					for (let ii = 1; ii < vertexCount - 1; ii++, s += 3) {
						ct[s] = index;
						ct[s + 1] = index + ii;
						ct[s + 2] = index + ii + 1;
					}
					index += vertexCount;
					offset += polygonSize;
				}
			}
			return true;
		}

		const clipOutput = this.clipOutput;
		const polygons = this.clippingPolygons;
		const polygonsCount = this.clippingPolygons.length;
		let clipOutputItems = null;
		for (let i = 0; i < trianglesLength; i += 3) {
			let t = triangles[i];
			let v = t * stride;
			const x1 = vertices[vertexStart + v], y1 = vertices[vertexStart + v + 1];
			let uv = t << 1;
			const u1 = uvs[uv], v1 = uvs[uv + 1];
			t = triangles[i + 1];
			v = t * stride;
			const x2 = vertices[vertexStart + v], y2 = vertices[vertexStart + v + 1];
			uv = t << 1;
			const u2 = uvs[uv], v2 = uvs[uv + 1];
			t = triangles[i + 2];
			v = t * stride;
			const x3 = vertices[vertexStart + v], y3 = vertices[vertexStart + v + 1];
			uv = t << 1;
			const u3 = uvs[uv], v3 = uvs[uv + 1];
			const d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1, d = 1 / (d0 * d2 + d1 * (y1 - y3));

			for (let p = 0; p < polygonsCount; p++) {
				let s = this.clippedVerticesLength;
				if (this.clip(x1, y1, x2, y2, x3, y3, polygons[p])) {
					clipOutputItems = clipOutput;
					const clipOutputLength = clipOutput.length;
					if (clipOutputLength === 0) continue;
					let clipOutputCount = clipOutputLength >> 1;

					const newLength = s + clipOutputCount * stride;
					if (clippedVertices.length < newLength) {
						this._clippedVerticesTyped = new Float32Array(newLength * 2);
						this._clippedVerticesTyped.set(clippedVertices.subarray(0, s));
						this._clippedUVsTyped = new Float32Array((this.clippedUVsLength + clipOutputCount * 2) * 2);
						this._clippedUVsTyped.set(clippedUVs.subarray(0, this.clippedUVsLength));
						clippedVertices = this._clippedVerticesTyped;
						clippedUVs = this._clippedUVsTyped;
					}
					const cv = clippedVertices;
					const cu = clippedUVs;
					this.clippedVerticesLength = newLength;

					let uvIndex = this.clippedUVsLength;
					this.clippedUVsLength = uvIndex + clipOutputCount * 2;
					for (let ii = 0; ii < clipOutputLength; ii += 2, s += stride, uvIndex += 2) {
						const x = clipOutputItems[ii], y = clipOutputItems[ii + 1];
						cv[s] = x;
						cv[s + 1] = y;

						const c0 = x - x3, c1 = y - y3, a = (d0 * c0 + d1 * c1) * d, b = (d4 * c0 + d2 * c1) * d, c = 1 - a - b;
						cu[uvIndex] = u1 * a + u2 * b + u3 * c;
						cu[uvIndex + 1] = v1 * a + v2 * b + v3 * c;
					}

					s = this.clippedTrianglesLength;
					const newLengthTriangles = s + 3 * (clipOutputCount - 2)
					if (clippedTriangles.length < newLengthTriangles) {
						this._clippedTrianglesTyped = new Uint16Array(newLengthTriangles * 2);
						this._clippedTrianglesTyped.set(clippedTriangles.subarray(0, s));
						clippedTriangles = this._clippedTrianglesTyped;
					}
					this.clippedTrianglesLength = newLengthTriangles;
					const ct = clippedTriangles;
					clipOutputCount--;
					for (let ii = 1; ii < clipOutputCount; ii++, s += 3) {
						ct[s] = index;
						ct[s + 1] = (index + ii);
						ct[s + 2] = (index + ii + 1);
					}
					index += clipOutputCount + 1;

				} else {

					let newLength = s + 3 * stride;
					if (clippedVertices.length < newLength) {
						this._clippedVerticesTyped = new Float32Array(newLength * 2);
						this._clippedVerticesTyped.set(clippedVertices.subarray(0, s));
						clippedVertices = this._clippedVerticesTyped;
					}
					clippedVertices[s] = x1;
					clippedVertices[s + 1] = y1;
					clippedVertices[s + stride] = x2;
					clippedVertices[s + stride + 1] = y2;
					clippedVertices[s + stride * 2] = x3;
					clippedVertices[s + stride * 2 + 1] = y3;

					const uvLength = this.clippedUVsLength + 3 * 2;
					if (clippedUVs.length < uvLength) {
						this._clippedUVsTyped = new Float32Array(uvLength * 2);
						this._clippedUVsTyped.set(clippedUVs.subarray(0, this.clippedUVsLength));
						clippedUVs = this._clippedUVsTyped;
					}
					const uvIndex = this.clippedUVsLength;
					clippedUVs[uvIndex] = u1;
					clippedUVs[uvIndex + 1] = v1;
					clippedUVs[uvIndex + 2] = u2;
					clippedUVs[uvIndex + 3] = v2;
					clippedUVs[uvIndex + 4] = u3;
					clippedUVs[uvIndex + 5] = v3;

					this.clippedVerticesLength = newLength;
					this.clippedUVsLength = uvLength;

					s = this.clippedTrianglesLength;
					newLength = s + 3;
					if (clippedTriangles.length < newLength) {
						this._clippedTrianglesTyped = new Uint16Array(newLength * 2);
						this._clippedTrianglesTyped.set(clippedTriangles.subarray(0, s));
						clippedTriangles = this._clippedTrianglesTyped;
					}

					const ct = clippedTriangles;
					ct[s] = index;
					ct[s + 1] = (index + 1);
					ct[s + 2] = (index + 2);
					index += 3;

					this.clippedTrianglesLength = newLength;
					break;
				}
			}
		}

		this.clippedVerticesTyped = this._clippedVerticesTyped.subarray(0, this.clippedVerticesLength)
		this.clippedUVsTyped = this._clippedUVsTyped.subarray(0, this.clippedUVsLength)
		this.clippedTrianglesTyped = this._clippedTrianglesTyped.subarray(0, this.clippedTrianglesLength)
		return clipOutputItems !== null;
	}

	private clip (x1: number, y1: number, x2: number, y2: number, x3: number, y3: number, polygon: number[]) {
		const originalOutput = this.clipOutput;
		let clipped = false;

		// Avoid copy at the end.
		let input: number[], output: number[];
		if (polygon.length % 4 >= 2) {
			input = this.clipOutput;
			output = this.scratch;
		} else {
			input = this.scratch;
			output = this.clipOutput;
		}

		const v = polygon;
		input.length = 8;
		const iv = input;
		iv[0] = x1;
		iv[1] = y1;
		iv[2] = x2;
		iv[3] = y2;
		iv[4] = x3;
		iv[5] = y3;
		iv[6] = x1;
		iv[7] = y1;
		output.length = 0;

		const last = polygon.length - 4;
		for (let i = 0; ; i += 2) {
			const edgeX = v[i], edgeY = v[i + 1], ex = edgeX - v[i + 2], ey = edgeY - v[i + 3];
			const outputStart = output.length;
			const iv = input;
			for (let ii = 0, nn = input.length - 2; ii < nn;) {
				x1 = iv[ii];
				y1 = iv[ii + 1];
				ii += 2;
				x2 = iv[ii];
				y2 = iv[ii + 1];
				const s2 = ey * (edgeX - x2) > ex * (edgeY - y2);
				const s1 = ey * (edgeX - x1) - ex * (edgeY - y1);
				if (s1 > 0) {
					if (s2) // v1 in, v2 in
						output.push(x2, y2);
					else { // v1 in, v2 out
						const ix = x2 - x1, iy = y2 - y1, t = s1 / (ix * ey - iy * ex);
						if (t >= 0 && t <= 1) {
							output.push(x1 + ix * t, y1 + iy * t);
							clipped = true;
						} else
							output.push(x2, y2);
					}
				} else if (s2) { // v1 out, v2 in
					const ix = x2 - x1, iy = y2 - y1, t = s1 / (ix * ey - iy * ex);
					if (t >= 0 && t <= 1) {
						output.push(x1 + ix * t, y1 + iy * t, x2, y2);
						clipped = true;
					} else
						output.push(x2, y2);
				} else // v1 out, v2 out
					clipped = true;
			}

			if (outputStart === output.length) { // All outside.
				originalOutput.length = 0;
				return true;
			}

			output.push(output[0], output[1]);

			if (i === last) break;
			const temp = output;
			output = input;
			output.length = 0;
			input = temp;
		}

		if (originalOutput !== output) {
			originalOutput.length = 0;
			for (let i = 0, n = output.length - 2; i < n; i++)
				originalOutput[i] = output[i];
		} else
			originalOutput.length = originalOutput.length - 2;

		return clipped;
	}

	private clipInverse (x1: number, y1: number, x2: number, y2: number, x3: number, y3: number, polygon: number[]): void {
		this.inverseVertices.length = 0;
		const vLast = polygon.length - 4;

		let input: number[], output: number[]; // Avoid copy at the end.
		if (polygon.length % 4 >= 2) {
			input = this.clipOutput;
			output = this.scratch;
		} else {
			input = this.scratch;
			output = this.clipOutput;
		}

		input.length = 8;
		let v = polygon, iv = input;
		iv[0] = x1;
		iv[1] = y1;
		iv[2] = x2;
		iv[3] = y2;
		iv[4] = x3;
		iv[5] = y3;
		iv[6] = x1;
		iv[7] = y1;
		output.length = 0;

		for (let i = 0; ; i += 2) {
			const edgeX = v[i], edgeY = v[i + 1], ex = edgeX - v[i + 2], ey = edgeY - v[i + 3];
			const outputStart = output.length, fragmentStart = this.inverseVertices.length;
			this.inverseVertices.push(0);
			iv = input;
			for (let ii = 0, nn = input.length - 2; ii < nn;) {
				x1 = iv[ii];
				y1 = iv[ii + 1];
				ii += 2;
				x2 = iv[ii];
				y2 = iv[ii + 1];
				const s2 = ey * (edgeX - x2) > ex * (edgeY - y2);
				const s1 = ey * (edgeX - x1) - ex * (edgeY - y1);
				if (s1 > 0) {
					if (s2) // v1 in, v2 in
						output.push(x2, y2);
					else {
						// v1 in, v2 out
						const ix = x2 - x1, iy = y2 - y1, t = s1 / (ix * ey - iy * ex);
						if (t >= 0 && t <= 1) {
							const cx = x1 + ix * t, cy = y1 + iy * t;
							output.push(cx, cy);
							this.inverseVertices.push(cx, cy, x2, y2);
						} else
							output.push(x2, y2);
					}
				} else if (s2) { // v1 out, v2 in
					const dx = x2 - x1, dy = y2 - y1, t = s1 / (dx * ey - dy * ex);
					if (t >= 0 && t <= 1) {
						const cx = x1 + dx * t, cy = y1 + dy * t;
						this.inverseVertices.push(cx, cy);
						output.push(cx, cy, x2, y2);
					} else
						output.push(x2, y2);
				} else // v1 out, v2 out
					this.inverseVertices.push(x2, y2);
			}

			const fragmentSize = this.inverseVertices.length - fragmentStart - 1;
			if (fragmentSize >= 6)
				this.inverseVertices[fragmentStart] = fragmentSize;
			else
				this.inverseVertices.length = fragmentStart; // Degenerate.

			if (outputStart === output.length) break; // All outside.

			output.push(output[0], output[1]);

			if (i === vLast) break;
			const temp = output;
			output = input;
			output.length = 0;
			input = temp;
		}
	}

	private makeClockwise (polygon: number[]): boolean {
		const v = polygon;
		const n = polygon.length;
		let noCW = true, noCCW = true;
		let area = 0, prevX = v[n - 2], prevY = v[n - 1], currX = v[0], currY = v[1];
		for (let i = 2; i < n; i += 2) {
			const nextX = v[i], nextY = v[i + 1];
			area += currX * nextY - nextX * currY;
			const cross = (currX - prevX) * (nextY - currY) - (currY - prevY) * (nextX - currX);
			noCCW = noCCW && cross <= 0;
			noCW = noCW && cross >= 0;
			prevX = currX;
			prevY = currY;
			currX = nextX;
			currY = nextY;
		}
		area += currX * v[1] - v[0] * currY;
		const cross = (currX - prevX) * (v[1] - currY) - (currY - prevY) * (v[0] - currX);
		noCCW = noCCW && cross <= 0;
		noCW = noCW && cross >= 0;
		if (area >= 0) {
			for (let i = 0, lastX = n - 2, half = n >> 1; i < half; i += 2) {
				const x = v[i], y = v[i + 1];
				const other = lastX - i;
				v[i] = v[other];
				v[i + 1] = v[other + 1];
				v[other] = x;
				v[other + 1] = y;
			}
			return noCW;
		}
		return noCCW;
	}

	private makeConvex (polygon: number[]): void {
		const n = polygon.length;
		const v = polygon;
		this.clipOutput.length = n;
		const sorted = this.clipOutput;
		sorted[0] = v[0];
		sorted[1] = v[1];
		for (let i = 2; i < n; i += 2) {
			const x = v[i], y = v[i + 1];
			let p = i - 2;
			for (; p >= 0 && (sorted[p] > x || (sorted[p] === x && sorted[p + 1] > y)); p -= 2) {
				sorted[p + 2] = sorted[p];
				sorted[p + 3] = sorted[p + 1];
			}
			sorted[p + 2] = x;
			sorted[p + 3] = y;
		}
		v[0] = sorted[0];
		v[1] = sorted[1];
		v[2] = sorted[2];
		v[3] = sorted[3];
		let s = 4;
		for (let i = 4; i < n; i += 2, s += 2) {
			const x = sorted[i], y = sorted[i + 1];
			while ((v[s - 2] - v[s - 4]) * (y - v[s - 3]) - (v[s - 1] - v[s - 3]) * (x - v[s - 4]) >= 0) {
				s -= 2;
				if (s === 2) break;
			}
			v[s] = x;
			v[s + 1] = y;
		}
		v[s] = sorted[n - 4];
		v[s + 1] = sorted[n - 3];
		const t = s;
		s += 2;
		for (let i = n - 6; i >= 0; i -= 2, s += 2) {
			const x = sorted[i], y = sorted[i + 1];
			while ((v[s - 2] - v[s - 4]) * (y - v[s - 3]) - (v[s - 1] - v[s - 3]) * (x - v[s - 4]) >= 0) {
				s -= 2;
				if (s === t) break;
			}
			v[s] = x;
			v[s + 1] = y;
		}
		polygon.length = s - 2;
	}
}
