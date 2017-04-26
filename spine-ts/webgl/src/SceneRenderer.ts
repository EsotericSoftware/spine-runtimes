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
	export class SceneRenderer implements Disposable {
		context: ManagedWebGLRenderingContext;
		canvas: HTMLCanvasElement;
		camera: OrthoCamera;
		batcher: PolygonBatcher;
		private twoColorTint = false;
		private batcherShader: Shader;
		private shapes: ShapeRenderer;
		private shapesShader: Shader;
		private activeRenderer: PolygonBatcher | ShapeRenderer | SkeletonDebugRenderer = null;
		private skeletonRenderer: SkeletonRenderer;
		private skeletonDebugRenderer: SkeletonDebugRenderer;
		private QUAD = [
			0, 0, 1, 1, 1, 1, 0, 0,
			0, 0, 1, 1, 1, 1, 0, 0,
			0, 0, 1, 1, 1, 1, 0, 0,
			0, 0, 1, 1, 1, 1, 0, 0,
		];
		private QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];
		private WHITE = new Color(1, 1, 1, 1);

		constructor (canvas: HTMLCanvasElement, context: ManagedWebGLRenderingContext | WebGLRenderingContext, twoColorTint: boolean = true) {
			this.canvas = canvas;
			this.context = context instanceof ManagedWebGLRenderingContext? context : new ManagedWebGLRenderingContext(context);
			this.twoColorTint = twoColorTint;
			this.camera = new OrthoCamera(canvas.width, canvas.height);
			this.batcherShader = twoColorTint ? Shader.newTwoColoredTextured(this.context) : Shader.newColoredTextured(this.context);
			this.batcher = new PolygonBatcher(this.context, twoColorTint);
			this.shapesShader = Shader.newColored(this.context);
			this.shapes = new ShapeRenderer(this.context);
			this.skeletonRenderer = new SkeletonRenderer(this.context, twoColorTint);
			this.skeletonDebugRenderer = new SkeletonDebugRenderer(this.context);
		}

		begin () {
			this.camera.update();
			this.enableRenderer(this.batcher);
		}

		drawSkeleton (skeleton: Skeleton, premultipliedAlpha = false) {
			this.enableRenderer(this.batcher);
			this.skeletonRenderer.premultipliedAlpha = premultipliedAlpha;
			this.skeletonRenderer.draw(this.batcher, skeleton);
		}

		drawSkeletonDebug(skeleton: Skeleton, premultipliedAlpha = false, ignoredBones: Array<string> = null) {
			this.enableRenderer(this.shapes);
			this.skeletonDebugRenderer.premultipliedAlpha = premultipliedAlpha;
			this.skeletonDebugRenderer.draw(this.shapes, skeleton, ignoredBones);
		}

		drawTexture (texture: GLTexture, x: number, y: number, width: number, height: number, color: Color = null) {
			this.enableRenderer(this.batcher);
			if (color === null) color = this.WHITE;
			let quad = this.QUAD;
			var i = 0;
			quad[i++] = x;
			quad[i++] = y;
			quad[i++] = color.r;
			quad[i++] = color.g;
			quad[i++] = color.b;
			quad[i++] = color.a;
			quad[i++] = 0;
			quad[i++] = 1;
			if (this.twoColorTint) {
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
			}
			quad[i++] = x + width;
			quad[i++] = y;
			quad[i++] = color.r;
			quad[i++] = color.g;
			quad[i++] = color.b;
			quad[i++] = color.a;
			quad[i++] = 1;
			quad[i++] = 1;
			if (this.twoColorTint) {
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
			}
			quad[i++] = x + width;
			quad[i++] = y + height;
			quad[i++] = color.r;
			quad[i++] = color.g;
			quad[i++] = color.b;
			quad[i++] = color.a;
			quad[i++] = 1;
			quad[i++] = 0;
			if (this.twoColorTint) {
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
			}
			quad[i++] = x;
			quad[i++] = y + height;
			quad[i++] = color.r;
			quad[i++] = color.g;
			quad[i++] = color.b;
			quad[i++] = color.a;
			quad[i++] = 0;
			quad[i++] = 0;
			if (this.twoColorTint) {
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
			}
			this.batcher.draw(texture, quad, this.QUAD_TRIANGLES);
		}

		drawTextureRotated (texture: GLTexture, x: number, y: number, width: number, height: number, pivotX: number, pivotY: number, angle: number, color: Color = null, premultipliedAlpha: boolean = false) {
			this.enableRenderer(this.batcher);
			if (color === null) color = this.WHITE;
			let quad = this.QUAD;

			// bottom left and top right corner points relative to origin
			let worldOriginX = x + pivotX;
			let worldOriginY = y + pivotY;
			let fx = -pivotX;
			let fy = -pivotY;
			let fx2 = width - pivotX;
			let fy2 = height - pivotY;

			// construct corner points, start from top left and go counter clockwise
			let p1x = fx;
			let p1y = fy;
			let p2x = fx;
			let p2y = fy2;
			let p3x = fx2;
			let p3y = fy2;
			let p4x = fx2;
			let p4y = fy;

			let x1 = 0;
			let y1 = 0;
			let x2 = 0;
			let y2 = 0;
			let x3 = 0;
			let y3 = 0;
			let x4 = 0;
			let y4 = 0;

			// rotate
			if (angle != 0) {
				let cos = MathUtils.cosDeg(angle);
				let sin = MathUtils.sinDeg(angle);

				x1 = cos * p1x - sin * p1y;
				y1 = sin * p1x + cos * p1y;

				x4 = cos * p2x - sin * p2y;
				y4 = sin * p2x + cos * p2y;

				x3 = cos * p3x - sin * p3y;
				y3 = sin * p3x + cos * p3y;

				x2 = x3 + (x1 - x4);
				y2 = y3 + (y1 - y4);
			} else {
				x1 = p1x;
				y1 = p1y;

				x4 = p2x;
				y4 = p2y;

				x3 = p3x;
				y3 = p3y;

				x2 = p4x;
				y2 = p4y;
			}

			x1 += worldOriginX;
			y1 += worldOriginY;
			x2 += worldOriginX;
			y2 += worldOriginY;
			x3 += worldOriginX;
			y3 += worldOriginY;
			x4 += worldOriginX;
			y4 += worldOriginY;

			var i = 0;
			quad[i++] = x1;
			quad[i++] = y1;
			quad[i++] = color.r;
			quad[i++] = color.g;
			quad[i++] = color.b;
			quad[i++] = color.a;
			quad[i++] = 0;
			quad[i++] = 1;
			if (this.twoColorTint) {
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
			}
			quad[i++] = x2;
			quad[i++] = y2;
			quad[i++] = color.r;
			quad[i++] = color.g;
			quad[i++] = color.b;
			quad[i++] = color.a;
			quad[i++] = 1;
			quad[i++] = 1;
			if (this.twoColorTint) {
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
			}
			quad[i++] = x3;
			quad[i++] = y3;
			quad[i++] = color.r;
			quad[i++] = color.g;
			quad[i++] = color.b;
			quad[i++] = color.a;
			quad[i++] = 1;
			quad[i++] = 0;
			if (this.twoColorTint) {
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
			}
			quad[i++] = x4;
			quad[i++] = y4;
			quad[i++] = color.r;
			quad[i++] = color.g;
			quad[i++] = color.b;
			quad[i++] = color.a;
			quad[i++] = 0;
			quad[i++] = 0;
			if (this.twoColorTint) {
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
			}
			this.batcher.draw(texture, quad, this.QUAD_TRIANGLES);
		}

		drawRegion (region: TextureAtlasRegion, x: number, y: number, width: number, height: number, color: Color = null, premultipliedAlpha: boolean = false) {
			this.enableRenderer(this.batcher);
			if (color === null) color = this.WHITE;
			let quad = this.QUAD;
			var i = 0;
			quad[i++] = x;
			quad[i++] = y;
			quad[i++] = color.r;
			quad[i++] = color.g;
			quad[i++] = color.b;
			quad[i++] = color.a;
			quad[i++] = region.u;
			quad[i++] = region.v2;
			if (this.twoColorTint) {
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
			}
			quad[i++] = x + width;
			quad[i++] = y;
			quad[i++] = color.r;
			quad[i++] = color.g;
			quad[i++] = color.b;
			quad[i++] = color.a;
			quad[i++] = region.u2;
			quad[i++] = region.v2;
			if (this.twoColorTint) {
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
			}
			quad[i++] = x + width;
			quad[i++] = y + height;
			quad[i++] = color.r;
			quad[i++] = color.g;
			quad[i++] = color.b;
			quad[i++] = color.a;
			quad[i++] = region.u2;
			quad[i++] = region.v;
			if (this.twoColorTint) {
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
			}
			quad[i++] = x;
			quad[i++] = y + height;
			quad[i++] = color.r;
			quad[i++] = color.g;
			quad[i++] = color.b;
			quad[i++] = color.a;
			quad[i++] = region.u;
			quad[i++] = region.v;
			if (this.twoColorTint) {
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
				quad[i++] = 0;
			}
			this.batcher.draw(<GLTexture>region.texture, quad, this.QUAD_TRIANGLES);
		}

		line (x: number, y: number, x2: number, y2: number, color: Color = null, color2: Color = null) {
			this.enableRenderer(this.shapes);
			this.shapes.line(x, y, x2, y2, color);
		}

		triangle (filled: boolean, x: number, y: number, x2: number, y2: number, x3: number, y3: number, color: Color = null, color2: Color = null, color3: Color = null) {
			this.enableRenderer(this.shapes);
			this.shapes.triangle(filled, x, y, x2, y2, x3, y3, color, color2, color3);
		}

		quad (filled: boolean, x: number, y: number, x2: number, y2: number, x3: number, y3: number, x4: number, y4: number, color: Color = null, color2: Color = null, color3: Color = null, color4: Color = null) {
			this.enableRenderer(this.shapes);
			this.shapes.quad(filled, x, y, x2, y2, x3, y3, x4, y4, color, color2, color3, color4);
		}

		rect (filled: boolean, x: number, y: number, width: number, height: number, color: Color = null) {
			this.enableRenderer(this.shapes);
			this.shapes.rect(filled, x, y, width, height, color);
		}

		rectLine (filled: boolean, x1: number, y1: number, x2: number, y2: number, width: number, color: Color = null) {
			this.enableRenderer(this.shapes);
			this.shapes.rectLine(filled, x1, y1, x2, y2, width, color);
		}

		polygon (polygonVertices: ArrayLike<number>, offset: number, count: number, color: Color = null) {
			this.enableRenderer(this.shapes);
			this.shapes.polygon(polygonVertices, offset, count, color);
		}

		circle (filled: boolean, x: number, y: number, radius: number, color: Color = null, segments: number = 0) {
			this.enableRenderer(this.shapes);
			this.shapes.circle(filled, x, y, radius, color, segments);
		}

		curve (x1: number, y1: number, cx1: number, cy1: number, cx2: number, cy2: number, x2: number, y2: number, segments: number, color: Color = null) {
			this.enableRenderer(this.shapes);
			this.shapes.curve(x1, y1, cx1, cy1, cx2, cy2, x2, y2, segments, color);
		}

		end () {
			if (this.activeRenderer === this.batcher) this.batcher.end();
			else if (this.activeRenderer === this.shapes) this.shapes.end();
			this.activeRenderer = null;
		}

		resize (resizeMode: ResizeMode) {
			let canvas = this.canvas;
			var w = canvas.clientWidth;
			var h = canvas.clientHeight;
			if (canvas.width != w || canvas.height != h) {
				canvas.width = w;
				canvas.height = h;
			}
			this.context.gl.viewport(0, 0, canvas.width, canvas.height);

			if (resizeMode === ResizeMode.Stretch) {
				// nothing to do, we simply apply the viewport size of the camera
			} else if (resizeMode === ResizeMode.Expand) {
				this.camera.setViewport(w, h);
			} else if (resizeMode === ResizeMode.Fit) {
				let sourceWidth = canvas.width, sourceHeight = canvas.height;
				let targetWidth = this.camera.viewportWidth, targetHeight = this.camera.viewportHeight;
				let targetRatio = targetHeight / targetWidth;
				let sourceRatio = sourceHeight / sourceWidth;
				let scale = targetRatio < sourceRatio ? targetWidth / sourceWidth : targetHeight / sourceHeight;
				this.camera.viewportWidth = sourceWidth * scale;
				this.camera.viewportHeight = sourceHeight * scale;
			}
			this.camera.update();
		}

		private enableRenderer(renderer: PolygonBatcher | ShapeRenderer | SkeletonDebugRenderer) {
			if (this.activeRenderer === renderer) return;
			this.end();
			if (renderer instanceof PolygonBatcher) {
				this.batcherShader.bind();
				this.batcherShader.setUniform4x4f(Shader.MVP_MATRIX, this.camera.projectionView.values);
				this.batcherShader.setUniformi("u_texture", 0);
				this.batcher.begin(this.batcherShader);
				this.activeRenderer = this.batcher;
			} else if (renderer instanceof ShapeRenderer) {
				this.shapesShader.bind();
				this.shapesShader.setUniform4x4f(Shader.MVP_MATRIX, this.camera.projectionView.values);
				this.shapes.begin(this.shapesShader);
				this.activeRenderer = this.shapes;
			} else {
				this.activeRenderer = this.skeletonDebugRenderer;
			}
		}

		dispose () {
			this.batcher.dispose();
			this.batcherShader.dispose();
			this.shapes.dispose();
			this.shapesShader.dispose();
			this.skeletonDebugRenderer.dispose();
		}
	}

	export enum ResizeMode {
		Stretch,
		Expand,
		Fit
	}
}
