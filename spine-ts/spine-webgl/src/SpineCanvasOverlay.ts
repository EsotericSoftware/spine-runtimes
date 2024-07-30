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

import { SpineCanvas, SpineCanvasApp, AtlasAttachmentLoader, SkeletonBinary, SkeletonJson, Skeleton, AnimationState, AnimationStateData, Physics, Vector3, ResizeMode, Color } from "./index.js";

/** Manages the life-cycle and WebGL context of a {@link SpineCanvasApp}. The app loads
 * assets and initializes itself, then updates and renders its state at the screen refresh rate. */
export class SpineCanvasOverlay {

	private spineCanvas:SpineCanvas;
	private canvas:HTMLCanvasElement;

	private skeletonList = new Array<{ skeleton: Skeleton, state: AnimationState, htmlElements: Array<HTMLElement>}>();

	private disposed = false;



	/** Constructs a new spine canvas, rendering to the provided HTML canvas. */
	constructor () {
		this.canvas = document.createElement('canvas');
		document.body.appendChild(this.canvas); // adds the canvas to the body element
		this.canvas.style.position = "absolute";
		this.canvas.style.top = "0";
		this.canvas.style.left = "0";
		this.canvas.style.display = "inline";
		this.canvas.style.setProperty("pointer-events", "none");
		this.updateCanvasSize();

		const resizeObserver = new ResizeObserver(() => {
            this.updateCanvasSize();
			this.spineCanvas.renderer.resize(ResizeMode.Expand);
        });
        resizeObserver.observe(document.body);

		const red = new Color(1, 0, 0, 1);
		const spineCanvasApp: SpineCanvasApp = {

			update: (canvas: SpineCanvas, delta: number) => {
				this.skeletonList.forEach(({ skeleton, state, htmlElements }) => {
					if (htmlElements.length === 0) return;
					state.update(delta);
                    state.apply(skeleton);
                    skeleton.update(delta);
                    skeleton.updateWorldTransform(Physics.update);
				});
			},

			render: (canvas: SpineCanvas) => {
				let renderer = canvas.renderer;
				renderer.begin();

                // webgl canvas center
                const vec3 = new Vector3(0, 0);
                renderer.camera.worldToScreen(vec3, canvas.htmlCanvas.clientWidth, canvas.htmlCanvas.clientHeight);

				const devicePixelRatio = window.devicePixelRatio;
				this.skeletonList.forEach(({ skeleton, htmlElements }) => {
					if (htmlElements.length === 0) return;

					htmlElements.forEach((div) => {

						const bounds = div.getBoundingClientRect();
						const x = (bounds.x + window.scrollX - vec3.x) * devicePixelRatio;
						const y = (bounds.y + window.scrollY - vec3.y) * devicePixelRatio;

						renderer.drawSkeleton(skeleton, true, -1, -1, (vertices, size, vertexSize) => {
							for (let i = 0; i < size; i+=vertexSize) {
								vertices[i] = vertices[i] + x;
								vertices[i+1] = vertices[i+1] - y;
							}
						});

						// show skeleton center (root)
						const root = skeleton.getRootBone()!;
						const vec3Root = new Vector3(root.x, root.y);
						renderer.camera.worldToScreen(vec3Root, canvas.htmlCanvas.clientWidth, canvas.htmlCanvas.clientHeight);
						const rootX = (vec3Root.x - vec3.x) * devicePixelRatio;
						const rootY = (vec3Root.y - vec3.y) * devicePixelRatio;
						renderer.circle(true, x + rootX, -y + rootY, 20, red);
					});

				});

				// Complete rendering.
				renderer.end();
			},
		}

		this.spineCanvas = new SpineCanvas(this.canvas, {
			app: spineCanvasApp,
		})
	}

	public async loadBinary(path: string) {
		return new Promise((resolve, reject) => {
			this.spineCanvas.assetManager.loadBinary(path, () => resolve(null));
		});
	}

	public async loadJson(path: string) {
		return new Promise((resolve, reject) => {
			this.spineCanvas.assetManager.loadJson(path, () => resolve(null));
		});
	}

	public async loadTextureAtlas(path: string) {
		return new Promise((resolve, reject) => {
			this.spineCanvas.assetManager.loadTextureAtlas(path, () => resolve(null));
		});
	}

	public async addSkeleton(
		skeletonOptions: { atlasPath: string, skeletonPath: string, scale: number },
		elements: Array<HTMLElement> = [],
	) {
		const { atlasPath, skeletonPath, scale } = skeletonOptions;
		const isBinary = skeletonPath.endsWith(".skel");
		await Promise.all([
			isBinary ? this.loadBinary(skeletonPath) : this.loadJson(skeletonPath),
			this.loadTextureAtlas(atlasPath),
		]);

		const atlas = this.spineCanvas.assetManager.require(atlasPath);
		const atlasLoader = new AtlasAttachmentLoader(atlas);

		const skeletonLoader = isBinary ? new SkeletonBinary(atlasLoader) : new SkeletonJson(atlasLoader);
		skeletonLoader.scale = scale;

		const skeletonFile = this.spineCanvas.assetManager.require(skeletonPath);
		const skeletonData = skeletonLoader.readSkeletonData(skeletonFile);

		const skeleton = new Skeleton(skeletonData);
		const animationStateData = new AnimationStateData(skeletonData);
		const state = new AnimationState(animationStateData);

		this.skeletonList.push({ skeleton, state, htmlElements: [...elements] });

		return { skeleton, state }
	}


	private updateCanvasSize() {
		const pageSize = this.getPageSize();
		this.canvas.style.width = pageSize.width + "px";
		this.canvas.style.height = pageSize.height + "px";
	}

	private getPageSize() {
		const width = Math.max(
			document.body.scrollWidth,
			document.documentElement.scrollWidth,
			document.body.offsetWidth,
			document.documentElement.offsetWidth,
			document.documentElement.clientWidth
		);

		const height = Math.max(
			document.body.scrollHeight,
			document.documentElement.scrollHeight,
			document.body.offsetHeight,
			document.documentElement.offsetHeight,
			document.documentElement.clientHeight
		);

		return { width, height };
	}

	/** Disposes the app, so the update() and render() functions are no longer called. Calls the dispose() callback.*/
	dispose () {
		this.disposed = true;
	}
}
