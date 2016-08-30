/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.5
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
		gl: WebGLRenderingContext;
		canvas: HTMLCanvasElement;
		camera: OrthoCamera;		
		private batcherShader: Shader;
		private batcher: PolygonBatcher;
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

		constructor (canvas: HTMLCanvasElement, gl: WebGLRenderingContext) {
			this.canvas = canvas;
			this.gl = gl;			
			this.camera = new OrthoCamera(canvas.width, canvas.height);
			this.batcherShader = Shader.newColoredTextured(gl);
			this.batcher = new PolygonBatcher(gl);
			this.shapesShader = Shader.newColored(gl);
			this.shapes = new ShapeRenderer(gl); 
			this.skeletonRenderer = new SkeletonRenderer(gl);
			this.skeletonDebugRenderer = new SkeletonDebugRenderer(gl);
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
			quad[0] = x;
			quad[1] = y;
			quad[2] = color.r;
			quad[3] = color.g;
			quad[4] = color.b;
			quad[5] = color.a;
			quad[6] = 0;
			quad[7] = 1;
			quad[8] = x + width;
			quad[9] = y;
			quad[10] = color.r;
			quad[11] = color.g;
			quad[12] = color.b;
			quad[13] = color.a;
			quad[14] = 1;
			quad[15] = 1;
			quad[16] = x + width;
			quad[17] = y + height;
			quad[18] = color.r;
			quad[19] = color.g;
			quad[20] = color.b;
			quad[21] = color.a;
			quad[22] = 1;
			quad[23] = 0;
			quad[24] = x;
			quad[25] = y + height;
			quad[26] = color.r;
			quad[27] = color.g;
			quad[28] = color.b;
			quad[29] = color.a;
			quad[30] = 0;
			quad[31] = 0;
			this.batcher.draw(texture, quad, this.QUAD_TRIANGLES);
		}

		drawRegion (region: TextureAtlasRegion, x: number, y: number, width: number, height: number, color: Color = null) {
			this.enableRenderer(this.batcher);
			if (color === null) color = this.WHITE;
			let quad = this.QUAD;
			quad[0] = x;
			quad[1] = y;
			quad[2] = color.r;
			quad[3] = color.g;
			quad[4] = color.b;
			quad[5] = color.a;
			quad[6] = region.u;
			quad[7] = region.v2;
			quad[8] = x + width;
			quad[9] = y;
			quad[10] = color.r;
			quad[11] = color.g;
			quad[12] = color.b;
			quad[13] = color.a;
			quad[14] = region.u2;
			quad[15] = region.v2;
			quad[16] = x + width;
			quad[17] = y + height;
			quad[18] = color.r;
			quad[19] = color.g;
			quad[20] = color.b;
			quad[21] = color.a;
			quad[22] = region.u2;
			quad[23] = region.v;
			quad[24] = x;
			quad[25] = y + height;
			quad[26] = color.r;
			quad[27] = color.g;
			quad[28] = color.b;
			quad[29] = color.a;
			quad[30] = region.u;
			quad[31] = region.v;
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
			this.gl.viewport(0, 0, canvas.width, canvas.height);

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