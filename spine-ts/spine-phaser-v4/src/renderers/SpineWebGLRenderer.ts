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

import { SceneRenderer } from "@esotericsoftware/spine-webgl";
import * as Phaser from "phaser";
import type { SpineGameObject } from "../SpineGameObject.js";
import type { SpineGameObjectRenderer } from "./SpineGameObjectRenderer.js";

export class SpineWebGLRenderer implements SpineGameObjectRenderer {
	// Scene plugins are created per scene, so reuse one SceneRenderer per Phaser WebGL renderer.
	private static readonly sceneRenderers = new WeakMap<Phaser.Renderer.WebGL.WebGLRenderer, SceneRenderer>();

	static getSceneRenderer (renderer: Phaser.Renderer.WebGL.WebGLRenderer): SceneRenderer {
		let sceneRenderer = SpineWebGLRenderer.sceneRenderers.get(renderer);
		if (!sceneRenderer) {
			sceneRenderer = new SceneRenderer(renderer.canvas, renderer.gl, true);
			SpineWebGLRenderer.sceneRenderers.set(renderer, sceneRenderer);
		}
		return sceneRenderer;
	}

	static getExistingSceneRenderer (renderer: Phaser.Renderer.WebGL.WebGLRenderer): SceneRenderer | null {
		return SpineWebGLRenderer.sceneRenderers.get(renderer) ?? null;
	}

	static disposeSceneRenderer (renderer: Phaser.Renderer.WebGL.WebGLRenderer): void {
		SpineWebGLRenderer.sceneRenderers.get(renderer)?.dispose();
		SpineWebGLRenderer.sceneRenderers.delete(renderer);
	}

	readonly type = "spine-webgl";

	renderWebGL (
		renderer: Phaser.Renderer.WebGL.WebGLRenderer,
		src: SpineGameObject,
		drawingContext: Phaser.Renderer.WebGL.DrawingContext,
		parentMatrix?: Phaser.GameObjects.Components.TransformMatrix,
		_renderStep?: number,
		displayList?: Phaser.GameObjects.GameObject[],
		displayListIndex?: number
	) {
		const camera = drawingContext.camera;
		if (!camera) return;
		const sceneRenderer = SpineWebGLRenderer.getSceneRenderer(renderer);

		const hasDisplayListPosition = displayList !== undefined && displayListIndex !== undefined;
		const previousIsSpineWebGL = hasDisplayListPosition && this.isAdjacentSpineWebGLGameObject(src, camera, displayList, displayListIndex, -1);
		const nextIsSpineWebGL = hasDisplayListPosition && this.isAdjacentSpineWebGLGameObject(src, camera, displayList, displayListIndex, 1);
		const drawingContextChanged = src.plugin.currentWebGLDrawingContext !== drawingContext;
		const shouldBeginSpineRenderer = !previousIsSpineWebGL || drawingContextChanged || !sceneRenderer.batcher.isDrawing;

		if (shouldBeginSpineRenderer) {
			if (sceneRenderer.batcher.isDrawing) {
				sceneRenderer.end();
				src.plugin.currentWebGLDrawingContext = null;
			}

			renderer.renderNodes.finishBatch();
			drawingContext.beginDraw();
			this.syncRendererCameraToDrawingContext(sceneRenderer, drawingContext);

			renderer.renderNodes.getNode("YieldContext")?.run(drawingContext);
			sceneRenderer.begin();
			src.plugin.currentWebGLDrawingContext = drawingContext;
		}

		camera.addToRenderList(src);
		const transform = Phaser.GameObjects.GetCalcMatrix(src, camera, parentMatrix, !drawingContext.useCanvas).calc;
		const { a, b, c, d, tx, ty } = transform;
		const { renderOffsetX, renderOffsetY, alpha } = src;

		sceneRenderer.drawSkeleton(
			src.skeleton,
			-1,
			-1,
			(vertices, numVertices, stride) => {
				for (let i = 0; i < numVertices; i += stride) {
					const vx = vertices[i] + renderOffsetX;
					const vy = vertices[i + 1] + renderOffsetY;
					vertices[i] = vx * a + vy * c + tx;
					vertices[i + 1] = vx * b + vy * d + ty;
					vertices[i + 2] *= alpha;
					vertices[i + 3] *= alpha;
					vertices[i + 4] *= alpha;
					vertices[i + 5] *= alpha;
					if (stride > 8) {
						vertices[i + 8] *= alpha;
						vertices[i + 9] *= alpha;
						vertices[i + 10] *= alpha;
					}
					// Phaser uploads TextureSources with flipY=true. Mesh2D compensates with
					// setFlipV(true); the spine-webgl path uses the same Phaser texture, so flip V here.
					vertices[i + 7] = 1 - vertices[i + 7];
				}
			}
		);

		if (!nextIsSpineWebGL) {
			sceneRenderer.end();
			src.plugin.currentWebGLDrawingContext = null;
			renderer.renderNodes.getNode("RebindContext")?.run(drawingContext);
			drawingContext.beginDraw();
		}
	}

	// disposed by SpinePlugin.
	preDestroy () {
	}

	private syncRendererCameraToDrawingContext (sceneRenderer: SceneRenderer, drawingContext: Phaser.Renderer.WebGL.DrawingContext) {
		const viewportWidth = drawingContext.width;
		const viewportHeight = drawingContext.height;
		sceneRenderer.camera.position.x = viewportWidth / 2;
		sceneRenderer.camera.position.y = viewportHeight / 2;
		sceneRenderer.camera.up.y = -1;
		sceneRenderer.camera.direction.z = 1;
		sceneRenderer.camera.setViewport(viewportWidth, viewportHeight);
		sceneRenderer.camera.update();
	}

	private isAdjacentSpineWebGLGameObject (
		src: SpineGameObject,
		camera: Phaser.Cameras.Scene2D.Camera,
		displayList: Phaser.GameObjects.GameObject[],
		displayListIndex: number,
		direction: -1 | 1
	): boolean {
		for (let i = displayListIndex + direction; i >= 0 && i < displayList.length; i += direction) {
			const gameObject = displayList[i];
			if (!this.isRenderableInDisplayList(gameObject, camera)) continue;
			return gameObject.type === src.type && (gameObject as SpineGameObject).rendererType === "spine-webgl";
		}

		return false;
	}

	private isRenderableInDisplayList (gameObject: Phaser.GameObjects.GameObject | undefined, camera: Phaser.Cameras.Scene2D.Camera): boolean {
		if (!gameObject) return false;

		const GameObjectRenderMask = 0xf;
		return GameObjectRenderMask === gameObject.renderFlags &&
			(gameObject.cameraFilter === 0 || !(gameObject.cameraFilter & camera.id)) &&
			(gameObject as Phaser.GameObjects.GameObject & { visible?: boolean }).visible !== false;
	}
}
