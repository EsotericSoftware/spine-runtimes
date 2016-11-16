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

module spine.webgl {
	export class SkeletonDebugRenderer implements Disposable {
		boneLineColor = new Color(1, 0, 0, 1);
		boneOriginColor = new Color(0, 1, 0, 1);
		attachmentLineColor = new Color(0, 0, 1, 0.5);
		triangleLineColor = new Color(1, 0.64, 0, 0.5);
		pathColor = new Color().setFromString("FF7F00");
		aabbColor = new Color(0, 1, 0, 0.5);
		drawBones = true;
		drawRegionAttachments = true;
		drawBoundingBoxes = true;
		drawMeshHull = true;
		drawMeshTriangles = true;
		drawPaths = true;
		drawSkeletonXY = false;
		premultipliedAlpha = false;
		scale = 1;
		boneWidth = 2;

		private gl: WebGLRenderingContext;
		private bounds = new SkeletonBounds();
		private temp = new Array<number>();
		private static LIGHT_GRAY = new Color(192 / 255, 192 / 255, 192 / 255, 1);
		private static GREEN = new Color(0, 1, 0, 1);

		constructor (gl: WebGLRenderingContext) {
			this.gl = gl;
		}

		draw (shapes: ShapeRenderer, skeleton: Skeleton, ignoredBones: Array<string> = null) {
			let skeletonX = skeleton.x;
			let skeletonY = skeleton.y;
			let gl = this.gl;
			let srcFunc = this.premultipliedAlpha ? gl.ONE : gl.SRC_ALPHA;
			shapes.setBlendMode(srcFunc, gl.ONE_MINUS_SRC_ALPHA);

			let bones = skeleton.bones;
			if (this.drawBones) {
				shapes.setColor(this.boneLineColor);
				for (let i = 0, n = bones.length; i < n; i++) {
					let bone = bones[i];
					if (ignoredBones && ignoredBones.indexOf(bone.data.name) > -1) continue;
					if (bone.parent == null) continue;
					let x = skeletonX + bone.data.length * bone.a + bone.worldX;
					let y = skeletonY + bone.data.length * bone.c + bone.worldY;
					shapes.rectLine(true, skeletonX + bone.worldX, skeletonY + bone.worldY, x, y, this.boneWidth * this.scale);
				}
				if (this.drawSkeletonXY) shapes.x(skeletonX, skeletonY, 4 * this.scale);
			}

			if (this.drawRegionAttachments) {
				shapes.setColor(this.attachmentLineColor);
				let slots = skeleton.slots;
				for (let i = 0, n = slots.length; i < n; i++) {
					let slot = slots[i];
					let attachment = slot.getAttachment();
					if (attachment instanceof RegionAttachment) {
						let regionAttachment = <RegionAttachment>attachment;
						let vertices = regionAttachment.updateWorldVertices(slot, false);
						shapes.line(vertices[RegionAttachment.X1], vertices[RegionAttachment.Y1], vertices[RegionAttachment.X2], vertices[RegionAttachment.Y2]);
						shapes.line(vertices[RegionAttachment.X2], vertices[RegionAttachment.Y2], vertices[RegionAttachment.X3], vertices[RegionAttachment.Y3]);
						shapes.line(vertices[RegionAttachment.X3], vertices[RegionAttachment.Y3], vertices[RegionAttachment.X4], vertices[RegionAttachment.Y4]);
						shapes.line(vertices[RegionAttachment.X4], vertices[RegionAttachment.Y4], vertices[RegionAttachment.X1], vertices[RegionAttachment.Y1]);
					}
				}
			}

			if (this.drawMeshHull || this.drawMeshTriangles) {
				let slots = skeleton.slots;
				for (let i = 0, n = slots.length; i < n; i++) {
					let slot = slots[i];
					let attachment = slot.getAttachment();
					if (!(attachment instanceof MeshAttachment)) continue;
					let mesh = <MeshAttachment>attachment;
					mesh.updateWorldVertices(slot, false);
					let vertices = mesh.worldVertices;
					let triangles = mesh.triangles;
					let hullLength = mesh.hullLength;
					if (this.drawMeshTriangles) {
						shapes.setColor(this.triangleLineColor);
						for (let ii = 0, nn = triangles.length; ii < nn; ii += 3) {
							let v1 = triangles[ii] * 8, v2 = triangles[ii + 1] * 8, v3 = triangles[ii + 2] * 8;
							shapes.triangle(false, vertices[v1], vertices[v1 + 1], //
								vertices[v2], vertices[v2 + 1], //
								vertices[v3], vertices[v3 + 1] //
							);
						}
					}
					if (this.drawMeshHull && hullLength > 0) {
						shapes.setColor(this.attachmentLineColor);
						hullLength = (hullLength >> 1) * 8;
						let lastX = vertices[hullLength - 8], lastY = vertices[hullLength - 7];
						for (let ii = 0, nn = hullLength; ii < nn; ii += 8) {
							let x = vertices[ii], y = vertices[ii + 1];
							shapes.line(x, y, lastX, lastY);
							lastX = x;
							lastY = y;
						}
					}
				}
			}

			if (this.drawBoundingBoxes) {
				let bounds = this.bounds;
				bounds.update(skeleton, true);
				shapes.setColor(this.aabbColor);
				shapes.rect(false, bounds.minX, bounds.minY, bounds.getWidth(), bounds.getHeight());
				let polygons = bounds.polygons;
				let boxes = bounds.boundingBoxes;
				for (let i = 0, n = polygons.length; i < n; i++) {
					let polygon = polygons[i];
					shapes.setColor(boxes[i].color);
					shapes.polygon(polygon, 0, polygon.length);
				}
			}

			if (this.drawPaths) {
				let slots = skeleton.slots;
				for (let i = 0, n = slots.length; i < n; i++) {
					let slot = slots[i];
					let attachment = slot.getAttachment();
					if (!(attachment instanceof PathAttachment)) continue;
					let path = <PathAttachment>attachment;
					let nn = path.worldVerticesLength;
					let world = this.temp = Utils.setArraySize(this.temp, nn, 0);
					path.computeWorldVertices(slot, world);
					let color = this.pathColor;
					let x1 = world[2], y1 = world[3], x2 = 0, y2 = 0;
					if (path.closed) {
						shapes.setColor(color);
						let cx1 = world[0], cy1 = world[1], cx2 = world[nn - 2], cy2 = world[nn - 1];
						x2 = world[nn - 4];
						y2 = world[nn - 3];
						shapes.curve(x1, y1, cx1, cy1, cx2, cy2, x2, y2, 32);
						shapes.setColor(SkeletonDebugRenderer.LIGHT_GRAY);
						shapes.line(x1, y1, cx1, cy1);
						shapes.line(x2, y2, cx2, cy2);
					}
					nn -= 4;
					for (let ii = 4; ii < nn; ii += 6) {
						let cx1 = world[ii], cy1 = world[ii + 1], cx2 = world[ii + 2], cy2 = world[ii + 3];
						x2 = world[ii + 4];
						y2 = world[ii + 5];
						shapes.setColor(color);
						shapes.curve(x1, y1, cx1, cy1, cx2, cy2, x2, y2, 32);
						shapes.setColor(SkeletonDebugRenderer.LIGHT_GRAY);
						shapes.line(x1, y1, cx1, cy1);
						shapes.line(x2, y2, cx2, cy2);
						x1 = x2;
						y1 = y2;
					}
				}
			}

			if (this.drawBones) {
				shapes.setColor(this.boneOriginColor);
				for (let i = 0, n = bones.length; i < n; i++) {
					let bone = bones[i];
					if (ignoredBones && ignoredBones.indexOf(bone.data.name) > -1) continue;
					shapes.circle(true, skeletonX + bone.worldX, skeletonY + bone.worldY, 3 * this.scale, SkeletonDebugRenderer.GREEN, 8);
				}
			}
		}

		dispose () {
		}
	}
}
