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

import { ClippingAttachment } from "./attachments/ClippingAttachment";
import { Slot } from "./Slot";
import { Triangulator } from "./Triangulator";
import { Utils, Color, NumberArrayLike } from "./Utils";

export class SkeletonClipping {
	private triangulator = new Triangulator();
	private clippingPolygon = new Array<number>();
	private clipOutput = new Array<number>();
	clippedVertices = new Array<number>();
	clippedTriangles = new Array<number>();
	private scratch = new Array<number>();

	private clipAttachment: ClippingAttachment | null = null;
	private clippingPolygons: Array<Array<number>> | null = null;

	clipStart (slot: Slot, clip: ClippingAttachment): number {
		if (this.clipAttachment) return 0;
		this.clipAttachment = clip;

		let n = clip.worldVerticesLength;
		let vertices = Utils.setArraySize(this.clippingPolygon, n);
		clip.computeWorldVertices(slot, 0, n, vertices, 0, 2);
		let clippingPolygon = this.clippingPolygon;
		SkeletonClipping.makeClockwise(clippingPolygon);
		let clippingPolygons = this.clippingPolygons = this.triangulator.decompose(clippingPolygon, this.triangulator.triangulate(clippingPolygon));
		for (let i = 0, n = clippingPolygons.length; i < n; i++) {
			let polygon = clippingPolygons[i];
			SkeletonClipping.makeClockwise(polygon);
			polygon.push(polygon[0]);
			polygon.push(polygon[1]);
		}

		return clippingPolygons.length;
	}

	clipEndWithSlot (slot: Slot) {
		if (this.clipAttachment && this.clipAttachment.endSlot == slot.data) this.clipEnd();
	}

	clipEnd () {
		if (!this.clipAttachment) return;
		this.clipAttachment = null;
		this.clippingPolygons = null;
		this.clippedVertices.length = 0;
		this.clippedTriangles.length = 0;
		this.clippingPolygon.length = 0;
	}

	isClipping (): boolean {
		return this.clipAttachment != null;
	}

	clipTriangles (vertices: NumberArrayLike, verticesLength: number, triangles: NumberArrayLike, trianglesLength: number, uvs: NumberArrayLike,
		light: Color, dark: Color, twoColor: boolean) {

		let clipOutput = this.clipOutput, clippedVertices = this.clippedVertices;
		let clippedTriangles = this.clippedTriangles;
		let polygons = this.clippingPolygons!;
		let polygonsCount = polygons.length;
		let vertexSize = twoColor ? 12 : 8;

		let index = 0;
		clippedVertices.length = 0;
		clippedTriangles.length = 0;
		outer:
		for (let i = 0; i < trianglesLength; i += 3) {
			let vertexOffset = triangles[i] << 1;
			let x1 = vertices[vertexOffset], y1 = vertices[vertexOffset + 1];
			let u1 = uvs[vertexOffset], v1 = uvs[vertexOffset + 1];

			vertexOffset = triangles[i + 1] << 1;
			let x2 = vertices[vertexOffset], y2 = vertices[vertexOffset + 1];
			let u2 = uvs[vertexOffset], v2 = uvs[vertexOffset + 1];

			vertexOffset = triangles[i + 2] << 1;
			let x3 = vertices[vertexOffset], y3 = vertices[vertexOffset + 1];
			let u3 = uvs[vertexOffset], v3 = uvs[vertexOffset + 1];

			for (let p = 0; p < polygonsCount; p++) {
				let s = clippedVertices.length;
				if (this.clip(x1, y1, x2, y2, x3, y3, polygons[p], clipOutput)) {
					let clipOutputLength = clipOutput.length;
					if (clipOutputLength == 0) continue;
					let d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1;
					let d = 1 / (d0 * d2 + d1 * (y1 - y3));

					let clipOutputCount = clipOutputLength >> 1;
					let clipOutputItems = this.clipOutput;
					let clippedVerticesItems = Utils.setArraySize(clippedVertices, s + clipOutputCount * vertexSize);
					for (let ii = 0; ii < clipOutputLength; ii += 2) {
						let x = clipOutputItems[ii], y = clipOutputItems[ii + 1];
						clippedVerticesItems[s] = x;
						clippedVerticesItems[s + 1] = y;
						clippedVerticesItems[s + 2] = light.r;
						clippedVerticesItems[s + 3] = light.g;
						clippedVerticesItems[s + 4] = light.b;
						clippedVerticesItems[s + 5] = light.a;
						let c0 = x - x3, c1 = y - y3;
						let a = (d0 * c0 + d1 * c1) * d;
						let b = (d4 * c0 + d2 * c1) * d;
						let c = 1 - a - b;
						clippedVerticesItems[s + 6] = u1 * a + u2 * b + u3 * c;
						clippedVerticesItems[s + 7] = v1 * a + v2 * b + v3 * c;
						if (twoColor) {
							clippedVerticesItems[s + 8] = dark.r;
							clippedVerticesItems[s + 9] = dark.g;
							clippedVerticesItems[s + 10] = dark.b;
							clippedVerticesItems[s + 11] = dark.a;
						}
						s += vertexSize;
					}

					s = clippedTriangles.length;
					let clippedTrianglesItems = Utils.setArraySize(clippedTriangles, s + 3 * (clipOutputCount - 2));
					clipOutputCount--;
					for (let ii = 1; ii < clipOutputCount; ii++) {
						clippedTrianglesItems[s] = index;
						clippedTrianglesItems[s + 1] = (index + ii);
						clippedTrianglesItems[s + 2] = (index + ii + 1);
						s += 3;
					}
					index += clipOutputCount + 1;

				} else {
					let clippedVerticesItems = Utils.setArraySize(clippedVertices, s + 3 * vertexSize);
					clippedVerticesItems[s] = x1;
					clippedVerticesItems[s + 1] = y1;
					clippedVerticesItems[s + 2] = light.r;
					clippedVerticesItems[s + 3] = light.g;
					clippedVerticesItems[s + 4] = light.b;
					clippedVerticesItems[s + 5] = light.a;
					if (!twoColor) {
						clippedVerticesItems[s + 6] = u1;
						clippedVerticesItems[s + 7] = v1;

						clippedVerticesItems[s + 8] = x2;
						clippedVerticesItems[s + 9] = y2;
						clippedVerticesItems[s + 10] = light.r;
						clippedVerticesItems[s + 11] = light.g;
						clippedVerticesItems[s + 12] = light.b;
						clippedVerticesItems[s + 13] = light.a;
						clippedVerticesItems[s + 14] = u2;
						clippedVerticesItems[s + 15] = v2;

						clippedVerticesItems[s + 16] = x3;
						clippedVerticesItems[s + 17] = y3;
						clippedVerticesItems[s + 18] = light.r;
						clippedVerticesItems[s + 19] = light.g;
						clippedVerticesItems[s + 20] = light.b;
						clippedVerticesItems[s + 21] = light.a;
						clippedVerticesItems[s + 22] = u3;
						clippedVerticesItems[s + 23] = v3;
					} else {
						clippedVerticesItems[s + 6] = u1;
						clippedVerticesItems[s + 7] = v1;
						clippedVerticesItems[s + 8] = dark.r;
						clippedVerticesItems[s + 9] = dark.g;
						clippedVerticesItems[s + 10] = dark.b;
						clippedVerticesItems[s + 11] = dark.a;

						clippedVerticesItems[s + 12] = x2;
						clippedVerticesItems[s + 13] = y2;
						clippedVerticesItems[s + 14] = light.r;
						clippedVerticesItems[s + 15] = light.g;
						clippedVerticesItems[s + 16] = light.b;
						clippedVerticesItems[s + 17] = light.a;
						clippedVerticesItems[s + 18] = u2;
						clippedVerticesItems[s + 19] = v2;
						clippedVerticesItems[s + 20] = dark.r;
						clippedVerticesItems[s + 21] = dark.g;
						clippedVerticesItems[s + 22] = dark.b;
						clippedVerticesItems[s + 23] = dark.a;

						clippedVerticesItems[s + 24] = x3;
						clippedVerticesItems[s + 25] = y3;
						clippedVerticesItems[s + 26] = light.r;
						clippedVerticesItems[s + 27] = light.g;
						clippedVerticesItems[s + 28] = light.b;
						clippedVerticesItems[s + 29] = light.a;
						clippedVerticesItems[s + 30] = u3;
						clippedVerticesItems[s + 31] = v3;
						clippedVerticesItems[s + 32] = dark.r;
						clippedVerticesItems[s + 33] = dark.g;
						clippedVerticesItems[s + 34] = dark.b;
						clippedVerticesItems[s + 35] = dark.a;
					}

					s = clippedTriangles.length;
					let clippedTrianglesItems = Utils.setArraySize(clippedTriangles, s + 3);
					clippedTrianglesItems[s] = index;
					clippedTrianglesItems[s + 1] = (index + 1);
					clippedTrianglesItems[s + 2] = (index + 2);
					index += 3;
					continue outer;
				}
			}
		}
	}

	/** Clips the input triangle against the convex, clockwise clipping area. If the triangle lies entirely within the clipping
	 * area, false is returned. The clipping area must duplicate the first vertex at the end of the vertices list. */
	clip (x1: number, y1: number, x2: number, y2: number, x3: number, y3: number, clippingArea: Array<number>, output: Array<number>) {
		let originalOutput = output;
		let clipped = false;

		// Avoid copy at the end.
		let input: Array<number>;
		if (clippingArea.length % 4 >= 2) {
			input = output;
			output = this.scratch;
		} else
			input = this.scratch;

		input.length = 0;
		input.push(x1);
		input.push(y1);
		input.push(x2);
		input.push(y2);
		input.push(x3);
		input.push(y3);
		input.push(x1);
		input.push(y1);
		output.length = 0;

		let clippingVertices = clippingArea;
		let clippingVerticesLast = clippingArea.length - 4;
		for (let i = 0; ; i += 2) {
			let edgeX = clippingVertices[i], edgeY = clippingVertices[i + 1];
			let edgeX2 = clippingVertices[i + 2], edgeY2 = clippingVertices[i + 3];
			let deltaX = edgeX - edgeX2, deltaY = edgeY - edgeY2;

			let inputVertices = input;
			let inputVerticesLength = input.length - 2, outputStart = output.length;
			for (let ii = 0; ii < inputVerticesLength; ii += 2) {
				let inputX = inputVertices[ii], inputY = inputVertices[ii + 1];
				let inputX2 = inputVertices[ii + 2], inputY2 = inputVertices[ii + 3];
				let side2 = deltaX * (inputY2 - edgeY2) - deltaY * (inputX2 - edgeX2) > 0;
				if (deltaX * (inputY - edgeY2) - deltaY * (inputX - edgeX2) > 0) {
					if (side2) { // v1 inside, v2 inside
						output.push(inputX2);
						output.push(inputY2);
						continue;
					}
					// v1 inside, v2 outside
					let c0 = inputY2 - inputY, c2 = inputX2 - inputX;
					let s = c0 * (edgeX2 - edgeX) - c2 * (edgeY2 - edgeY);
					if (Math.abs(s) > 0.000001) {
						let ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / s;
						output.push(edgeX + (edgeX2 - edgeX) * ua);
						output.push(edgeY + (edgeY2 - edgeY) * ua);
					} else {
						output.push(edgeX);
						output.push(edgeY);
					}
				} else if (side2) { // v1 outside, v2 inside
					let c0 = inputY2 - inputY, c2 = inputX2 - inputX;
					let s = c0 * (edgeX2 - edgeX) - c2 * (edgeY2 - edgeY);
					if (Math.abs(s) > 0.000001) {
						let ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / s;
						output.push(edgeX + (edgeX2 - edgeX) * ua);
						output.push(edgeY + (edgeY2 - edgeY) * ua);
					} else {
						output.push(edgeX);
						output.push(edgeY);
					}
					output.push(inputX2);
					output.push(inputY2);
				}
				clipped = true;
			}

			if (outputStart == output.length) { // All edges outside.
				originalOutput.length = 0;
				return true;
			}

			output.push(output[0]);
			output.push(output[1]);

			if (i == clippingVerticesLast) break;
			let temp = output;
			output = input;
			output.length = 0;
			input = temp;
		}

		if (originalOutput != output) {
			originalOutput.length = 0;
			for (let i = 0, n = output.length - 2; i < n; i++)
				originalOutput[i] = output[i];
		} else
			originalOutput.length = originalOutput.length - 2;

		return clipped;
	}

	public static makeClockwise (polygon: NumberArrayLike) {
		let vertices = polygon;
		let verticeslength = polygon.length;

		let area = vertices[verticeslength - 2] * vertices[1] - vertices[0] * vertices[verticeslength - 1], p1x = 0, p1y = 0, p2x = 0, p2y = 0;
		for (let i = 0, n = verticeslength - 3; i < n; i += 2) {
			p1x = vertices[i];
			p1y = vertices[i + 1];
			p2x = vertices[i + 2];
			p2y = vertices[i + 3];
			area += p1x * p2y - p2x * p1y;
		}
		if (area < 0) return;

		for (let i = 0, lastX = verticeslength - 2, n = verticeslength >> 1; i < n; i += 2) {
			let x = vertices[i], y = vertices[i + 1];
			let other = lastX - i;
			vertices[i] = vertices[other];
			vertices[i + 1] = vertices[other + 1];
			vertices[other] = x;
			vertices[other + 1] = y;
		}
	}
}
