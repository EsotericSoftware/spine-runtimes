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

import { SpineCanvas, SpineCanvasApp, AtlasAttachmentLoader, SkeletonBinary, SkeletonJson, Skeleton, Animation, AnimationState, AnimationStateData, Physics, Vector2, Vector3, ResizeMode, Color, MixBlend, MixDirection, SceneRenderer, SkeletonData } from "./index.js";

interface Rectangle {
	x: number,
	y: number,
	width: number,
	height: number,
}

interface OverlaySkeletonOptions {
	atlasPath: string,
	skeletonPath: string,
	scale: number,
	animation?: string,
	skeletonData?: SkeletonData,
}

interface OverlayHTMLOptions {
	element: HTMLElement,
	mode?: OverlayElementMode,
	showBounds?: boolean,
	offsetX?: number,
	offsetY?: number,
	xAxis?: number,
	yAxis?: number,
}

type OverlayElementMode = 'inside' | 'origin';

/** Manages the life-cycle and WebGL context of a {@link SpineCanvasOverlay}. */
export class SpineCanvasOverlay {

	private spineCanvas:SpineCanvas;
	private canvas:HTMLCanvasElement;

	private skeletonList = new Array<{
		skeleton: Skeleton,
		state: AnimationState,
		bounds: Rectangle,
		htmlOptionsList: Array<OverlayHTMLOptions>,
	}>();

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
		const blue = new Color(0, 0, 1, 1);
		const spineCanvasApp: SpineCanvasApp = {

			update: (canvas: SpineCanvas, delta: number) => {
				this.skeletonList.forEach(({ skeleton, state, htmlOptionsList }) => {
					if (htmlOptionsList.length === 0) return;
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
				const tempVector = new Vector3();
				this.skeletonList.forEach(({ skeleton, htmlOptionsList, bounds }) => {
					if (htmlOptionsList.length === 0) return;

					let { x: ax, y: ay, width: aw, height: ah } = bounds;

					htmlOptionsList.forEach(({ element, mode, showBounds, offsetX = 0, offsetY = 0, xAxis = 0, yAxis = 0 }) => {

						const divBounds = element.getBoundingClientRect();
						let x = 0, y = 0;
						if (mode === 'inside') {
							// scale ratio
							const scaleWidth = divBounds.width * devicePixelRatio / aw;
							const scaleHeight = divBounds.height * devicePixelRatio / ah;

							// attempt to use width ratio
							let ratio = scaleWidth;
							let scaledW = aw * ratio;
							let scaledH = ah * ratio;

							// if scaled height is bigger than div height, use height ratio instead
							if (scaledH > divBounds.height * devicePixelRatio) ratio = scaleHeight;

							const scaledX = (ax + aw / 2) * ratio;
							const scaledY = (ay + ah / 2) * ratio;

							const divX = divBounds.x + divBounds.width / 2 + window.scrollX;
							const divY = divBounds.y - 1 + divBounds.height / 2 + window.scrollY;

							tempVector.set(divX, divY, 0);
                			renderer.camera.screenToWorld(tempVector, canvas.htmlCanvas.clientWidth, canvas.htmlCanvas.clientHeight);

							x = tempVector.x - scaledX;
							y = tempVector.y - scaledY;

							skeleton.scaleX = ratio;
							skeleton.scaleY = ratio;

							if (showBounds) {
								renderer.circle(true, tempVector.x, tempVector.y, 10, blue);
								renderer.rect(false, ax * ratio + x + offsetX, ay * ratio + y + offsetY, aw * ratio, ah * ratio, blue);
							}

						} else {
							const divX = divBounds.x + divBounds.width * xAxis + window.scrollX;
							const divY = divBounds.y + divBounds.height * yAxis + window.scrollY;

							tempVector.set(divX, divY, 0);
                			renderer.camera.screenToWorld(tempVector, canvas.htmlCanvas.clientWidth, canvas.htmlCanvas.clientHeight);

							x = tempVector.x;
							y = tempVector.y;

							if (showBounds) {
								// show skeleton root
								const root = skeleton.getRootBone()!;
								renderer.circle(true, x + root.x + offsetX, y + root.y + offsetY, 10, red);
							}
						}

						renderer.drawSkeleton(skeleton, true, -1, -1, (vertices, size, vertexSize) => {
							for (let i = 0; i < size; i+=vertexSize) {
								vertices[i] = vertices[i] + x + offsetX;
								vertices[i+1] = vertices[i+1] + y + offsetY;
							}
						});

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

	// TODO: Reject error
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
		skeletonOptions: OverlaySkeletonOptions,
		htmlOptionsList: Array<OverlayHTMLOptions> | Array<HTMLElement> | HTMLElement | NodeList = [],
	) {
		const { atlasPath, skeletonPath, scale = 1, animation, skeletonData: skeletonDataInput } = skeletonOptions;
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
		const skeletonData = skeletonDataInput ?? skeletonLoader.readSkeletonData(skeletonFile);

		const skeleton = new Skeleton(skeletonData);
		const animationStateData = new AnimationStateData(skeletonData);
		const state = new AnimationState(animationStateData);

		let animationData;
		if (animation) {
			state.setAnimation(0, animation, true);
			animationData = animation ? skeleton.data.findAnimation(animation)! : undefined;
		}
		const bounds = this.calculateAnimationViewport(skeleton, animationData);

		let list: Array<OverlayHTMLOptions>;
		if (htmlOptionsList instanceof HTMLElement) htmlOptionsList = [htmlOptionsList] as Array<HTMLElement>;
		if (htmlOptionsList instanceof NodeList) htmlOptionsList = Array.from(htmlOptionsList) as Array<HTMLElement>;

		if (htmlOptionsList.length > 0 && htmlOptionsList[0] instanceof HTMLElement) {
			list = htmlOptionsList.map(element => ({ element: element } as OverlayHTMLOptions));
		} else {
			list = htmlOptionsList as Array<OverlayHTMLOptions>;
		}

		const mapList = list.map(({ element, mode: givenMode, showBounds = false, offsetX = 0, offsetY = 0, xAxis = 0, yAxis = 0 }, i) => {
			const mode = givenMode ?? 'inside';
			if (mode == 'inside' && i > 0) {
				console.warn("inside option works with multiple html elements only if the elements have the same dimension"
					+ "This is because the skeleton is scaled to stay into the div."
					+ "You can call addSkeleton several time (skeleton data can be reuse, if given).");
			}
			return {
				element,
				mode,
				showBounds,
				offsetX,
				offsetY,
				xAxis,
				yAxis,
			}
		});
		this.skeletonList.push({ skeleton, state, bounds, htmlOptionsList: mapList });

		return { skeleton, state }
	}

	public recalculateBounds(skeleton: Skeleton, state: AnimationState) {
		const track = state.getCurrent(0);
		const animation = track?.animation as (Animation | undefined);
		const bounds = this.calculateAnimationViewport(skeleton, animation);
		bounds.x /= skeleton.scaleX;
		bounds.y /= skeleton.scaleY;
		bounds.width /= skeleton.scaleX;
		bounds.height /= skeleton.scaleY;
		const element = this.skeletonList.find(element => element.skeleton === skeleton);
		if (element) {
			element.bounds = bounds;
		}
	}

	private calculateAnimationViewport (skeleton: Skeleton, animation?: Animation): Rectangle {
		skeleton.setToSetupPose();

		let offset = new Vector2(), size = new Vector2();
		const tempArray = new Array<number>(2);
		if (!animation) {
			skeleton.updateWorldTransform(Physics.update);
			skeleton.getBounds(offset, size, tempArray, this.spineCanvas.renderer.skeletonRenderer.getSkeletonClipping());
			return {
				x: offset.x,
				y: offset.y,
				width: size.x,
				height: size.y,
			}
		}

		let steps = 100, stepTime = animation.duration ? animation.duration / steps : 0, time = 0;
		let minX = 100000000, maxX = -100000000, minY = 100000000, maxY = -100000000;
		for (let i = 0; i < steps; i++, time += stepTime) {
			animation.apply(skeleton, time, time, false, [], 1, MixBlend.setup, MixDirection.mixIn);
			skeleton.updateWorldTransform(Physics.update);
			skeleton.getBounds(offset, size, tempArray, this.spineCanvas.renderer.skeletonRenderer.getSkeletonClipping());

			if (!isNaN(offset.x) && !isNaN(offset.y) && !isNaN(size.x) && !isNaN(size.y)) {
				minX = Math.min(offset.x, minX);
				maxX = Math.max(offset.x + size.x, maxX);
				minY = Math.min(offset.y, minY);
				maxY = Math.max(offset.y + size.y, maxY);
			} else
				console.error("Animation bounds are invalid: " + animation.name);
		}

		return {
			x: minX,
			y: minY,
			width: maxX - minX,
			height: maxY - minY,
		}
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

	// TODO
	dispose () {
		this.disposed = true;
	}
}
