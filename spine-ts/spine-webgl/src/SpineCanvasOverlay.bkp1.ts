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

import { SpineCanvas, SpineCanvasApp, AtlasAttachmentLoader, SkeletonBinary, SkeletonJson, Skeleton, Animation, AnimationState, AnimationStateData, Physics, Vector2, Vector3, ResizeMode, Color, MixBlend, MixDirection, SceneRenderer, SkeletonData, Input } from "./index.js";

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
	update?: UpdateSpineFunction;
}

type UpdateSpineFunction = (canvas: SpineCanvas, delta: number, skeleton: Skeleton, state: AnimationState) => void;

interface OverlayHTMLOptions {
	element: HTMLElement,
	mode?: OverlayElementMode,
	debug?: boolean,
	offsetX?: number,
	offsetY?: number,
	xAxis?: number,
	yAxis?: number,
	draggable?: boolean,
}

type OverlayHTMLElement = Required<OverlayHTMLOptions> & { element: HTMLElement, worldOffsetX: number, worldOffsetY: number, dragging: boolean, dragX: number, dragY: number };

type OverlayElementMode = 'inside' | 'origin';

/** Manages the life-cycle and WebGL context of a {@link SpineCanvasOverlay}. */
export class SpineCanvasOverlay {

	private spineCanvas:SpineCanvas;
	private canvas:HTMLCanvasElement;
	private input:Input;

	private skeletonList = new Array<{
		skeleton: Skeleton,
		state: AnimationState,
		bounds: Rectangle,
		htmlOptionsList: Array<OverlayHTMLElement>,
		update?: UpdateSpineFunction,
	}>();

	private resizeObserver:ResizeObserver;
	private disposed = false;

	private currentTranslateX = 0;
	private currentTranslateY = 0;
	private additionalPixelsBottom = 200;
	private offsetHeight = 50;
	private offsetHeightDraw: number;
	/** Constructs a new spine canvas, rendering to the provided HTML canvas. */
	constructor () {
		this.canvas = document.createElement('canvas');
		document.body.appendChild(this.canvas);
		this.canvas.style.position = "absolute";
		this.canvas.style.top = "0";
		this.canvas.style.left = "0";
		this.canvas.style.setProperty("pointer-events", "none");
		this.offsetHeightDraw = this.offsetHeight;
		this.canvas.style.transform =`translate(${this.currentTranslateX}px,${this.currentTranslateY}px)`;
		// this.canvas.style.display = "inline";
		// this.canvas.style.overflow = "hidden"; // useless
		// this.canvas.style.setProperty("will-change", "transform"); // performance seems to be even worse with this uncommented
		this.updateCanvasSize();

		this.resizeObserver = new ResizeObserver(() => {
            this.updateCanvasSize();
			this.spineCanvas.renderer.resize(ResizeMode.Expand);
        });
        this.resizeObserver.observe(document.body);

		window.addEventListener('scroll', this.scrollHandler);


		this.spineCanvas = new SpineCanvas(this.canvas, { app: this.setupSpineCanvasApp() });

		this.input = new Input(document.body, false);
		this.setupDragUtility();
	}

	// add a skeleton to the overlay and set the bounds to the given animation or to the setup pose
	public async addSkeleton(
		skeletonOptions: OverlaySkeletonOptions,
		htmlOptionsList: Array<OverlayHTMLOptions> | OverlayHTMLOptions | Array<HTMLElement> | HTMLElement | NodeList = [],
	) {
		const { atlasPath, skeletonPath, scale = 1, animation, skeletonData: skeletonDataInput, update } = skeletonOptions;
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
		if ('element' in htmlOptionsList) htmlOptionsList = [htmlOptionsList] as Array<OverlayHTMLOptions>;

		if (htmlOptionsList.length > 0 && htmlOptionsList[0] instanceof HTMLElement) {
			list = htmlOptionsList.map(element => ({ element } as OverlayHTMLOptions));
		} else {
			list = htmlOptionsList as Array<OverlayHTMLOptions>;
		}

		const mapList = list.map(({ element, mode: givenMode, debug = false, offsetX = 0, offsetY = 0, xAxis = 0, yAxis = 0, draggable = false, }, i) => {
			const mode = givenMode ?? 'inside';
			if (mode == 'inside' && i > 0) {
				console.warn("inside option works with multiple html elements only if the elements have the same dimension"
					+ "This is because the skeleton is scaled to stay into the div."
					+ "You can call addSkeleton several time (skeleton data can be reuse, if given).");
			}
			return {
				element: element as HTMLElement,
				mode,
				debug,
				offsetX,
				offsetY,
				xAxis,
				yAxis,
				draggable,
				dragX: 0,
				dragY: 0,
				worldOffsetX: 0,
				worldOffsetY: 0,
				dragging: false,
			}
		});
		this.skeletonList.push({ skeleton, state, update, bounds, htmlOptionsList: mapList });

		return { skeleton, state };
	}

	// calculate bounds of the current animation on track 0, then set it
	public recalculateBounds(skeleton: Skeleton) {
		const element = this.skeletonList.find(element => element.skeleton === skeleton);
		if (!element) return;
		const track = element.state.getCurrent(0);
		const animation = track?.animation as (Animation | undefined);
		const bounds = this.calculateAnimationViewport(skeleton, animation);
		this.setBounds(skeleton, bounds);
	}

	// set the given bounds on the current skeleton
	// bounds is used to center the skeleton in inside mode and as a input area for click events
	public setBounds(skeleton: Skeleton, bounds: Rectangle) {
		bounds.x /= skeleton.scaleX;
		bounds.y /= skeleton.scaleY;
		bounds.width /= skeleton.scaleX;
		bounds.height /= skeleton.scaleY;
		const element = this.skeletonList.find(element => element.skeleton === skeleton);
		if (element) {
			element.bounds = bounds;
		}
	}

	/*
	* Load assets utilities
	*/

	public async loadBinary(path: string) {
		return new Promise((resolve, reject) => {
			this.spineCanvas.assetManager.loadBinary(path,
				(_, binary) => resolve(binary),
				(_, message) => reject(message),
			);
		});
	}

	public async loadJson(path: string) {
		return new Promise((resolve, reject) => {
			this.spineCanvas.assetManager.loadJson(path,
				(_, object) => resolve(object),
				(_, message) => reject(message),
			);
		});
	}

	public async loadTextureAtlas(path: string) {
		return new Promise((resolve, reject) => {
			this.spineCanvas.assetManager.loadTextureAtlas(path,
				(_, atlas) => resolve(atlas),
				(_, message) => reject(message),
			);
		});
	}

	/*
	* Init utilities
	*/

	private setupSpineCanvasApp(): SpineCanvasApp {
		const red = new Color(1, 0, 0, 1);
		const green = new Color(0, 1, 0, 1);
		const blue = new Color(0, 0, 1, 1);

		return {
			update: (canvas: SpineCanvas, delta: number) => {
				this.skeletonList.forEach(({ skeleton, state, update, htmlOptionsList }) => {
					if (htmlOptionsList.length === 0) return;
					if (update) update(canvas, delta, skeleton, state)
					else {
						state.update(delta);
						state.apply(skeleton);
						skeleton.update(delta);
						skeleton.updateWorldTransform(Physics.update);
					}
				});
				(document.body.querySelector("#fps")! as HTMLElement).innerText = canvas.time.framesPerSecond.toFixed(2) + " fps";
			},

			render: (canvas: SpineCanvas) => {
				// canvas.clear(1, 0, 0, .1);
				let renderer = canvas.renderer;
				renderer.begin();

				const devicePixelRatio = window.devicePixelRatio;
				const tempVector = new Vector3();
				this.skeletonList.forEach(({ skeleton, htmlOptionsList, bounds }) => {
					if (htmlOptionsList.length === 0) return;

					let { x: ax, y: ay, width: aw, height: ah } = bounds;

					htmlOptionsList.forEach((list) => {
						const { element, mode, debug, offsetX, offsetY, xAxis, yAxis, dragX, dragY } = list;
						const divBounds = element.getBoundingClientRect();
						// divBounds.y += this.offsetHeightDraw;

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

							// get the center of the bounds
							const boundsX = (ax + aw / 2) * ratio;
							const boundsY = (ay + ah / 2) * ratio;

							// get the center of the div in world coordinate
							// const divX = divBounds.x + divBounds.width / 2 + window.scrollX;
							// const divY = divBounds.y - 1 + divBounds.height / 2 + window.scrollY;
							const divX = divBounds.x + divBounds.width / 2;
							const divY = divBounds.y - 1 + divBounds.height / 2;
							this.screenToWorld(tempVector, divX, divY);

							// get vertices offset: calculate the distance between div center and bounds center
							x = tempVector.x - boundsX;
							y = tempVector.y - boundsY;

							// scale the skeleton
							skeleton.scaleX = ratio;
							skeleton.scaleY = ratio;
						} else {

							// TODO: window.devicePixelRatio to manage browser zoom

							// get the center of the div in world coordinate
							// const divX = divBounds.x + divBounds.width * xAxis + window.scrollX;
							// const divY = divBounds.y + divBounds.height * yAxis + window.scrollY;
							const divX = divBounds.x + divBounds.width * xAxis;
							const divY = divBounds.y + divBounds.height * yAxis;
							this.screenToWorld(tempVector, divX, divY);
							// console.log(tempVector.x, tempVector.y)
							// console.log(window.devicePixelRatio)

							// get vertices offset
							x = tempVector.x;
							y = tempVector.y;
						}


						list.worldOffsetX = x + offsetX + dragX;
						list.worldOffsetY = y + offsetY + dragY;

						renderer.drawSkeleton(skeleton, true, -1, -1, (vertices, size, vertexSize) => {
							for (let i = 0; i < size; i+=vertexSize) {
								vertices[i] = vertices[i] + list.worldOffsetX;
								vertices[i+1] = vertices[i+1] + list.worldOffsetY;
							}
						});

						// drawing debug stuff
						if (debug) {
						// if (true) {
							// show bounds and its center
							renderer.rect(false,
								ax * skeleton.scaleX + list.worldOffsetX,
								ay * skeleton.scaleY + list.worldOffsetY,
								aw * skeleton.scaleX,
								ah * skeleton.scaleY,
								blue);
							const bbCenterX = (ax + aw / 2) * skeleton.scaleX + list.worldOffsetX;
							const bbCenterY = (ay + ah / 2) * skeleton.scaleY + list.worldOffsetY;
							renderer.circle(true, bbCenterX, bbCenterY, 10, blue);

							// show skeleton root
							const root = skeleton.getRootBone()!;
							renderer.circle(true, root.x + list.worldOffsetX, root.y + list.worldOffsetY, 10, red);

							// show shifted origin
							const originX = list.worldOffsetX - dragX - offsetX;
							const originY = list.worldOffsetY - dragY - offsetY;
							renderer.circle(true, originX, originY, 10, green);

							// show line from origin to bounds center
							renderer.line(originX, originY, bbCenterX, bbCenterY, green);
						}

					});

				});

				renderer.end();
			},
		}

	}

	private setupDragUtility() {
		// TODO: we should use document - body might have some margin that offset the click events - Meanwhile I take event pageX/Y
		const tempVectorInput = new Vector3();

		let prevX = 0;
		let prevY = 0;
		this.input.addListener({
			down: (x, y, ev) => {
				const originalEvent = ev instanceof MouseEvent ? ev : ev!.changedTouches[0];
				tempVectorInput.set(originalEvent.pageX - window.scrollX, originalEvent.pageY - window.scrollY, 0);
				this.spineCanvas.renderer.camera.screenToWorld(tempVectorInput, this.canvas.clientWidth, this.canvas.clientHeight);
				this.skeletonList.forEach(({ htmlOptionsList, bounds, skeleton }) => {
					htmlOptionsList.forEach((element) => {
						if (!element.draggable) return;

						const { worldOffsetX, worldOffsetY } = element;
						const newBounds: Rectangle = {
							x: bounds.x * skeleton.scaleX + worldOffsetX,
							y: bounds.y * skeleton.scaleY + worldOffsetY,
							width: bounds.width * skeleton.scaleX,
							height: bounds.height * skeleton.scaleY,
						};

						if (this.inside(tempVectorInput, newBounds)) {
							element.dragging = true;
							ev?.preventDefault();
						}

					});
				});
				prevX = tempVectorInput.x;
				prevY = tempVectorInput.y;
			},
			dragged: (x, y, ev) => {
				const originalEvent = ev instanceof MouseEvent ? ev : ev!.changedTouches[0];
				tempVectorInput.set(originalEvent.pageX - window.scrollX, originalEvent.pageY - window.scrollY, 0);
				this.spineCanvas.renderer.camera.screenToWorld(tempVectorInput, this.canvas.clientWidth, this.canvas.clientHeight);
				let dragX = tempVectorInput.x - prevX;
				let dragY = tempVectorInput.y - prevY;
				this.skeletonList.forEach(({ htmlOptionsList, bounds, skeleton }) => {
					htmlOptionsList.forEach((element) => {
						const { dragging } = element;

						if (dragging) {
							skeleton.physicsTranslate(dragX, dragY);
							element.dragX += dragX;
							element.dragY += dragY;
							ev?.preventDefault();
							ev?.stopPropagation()
						}

					});
				});
				prevX = tempVectorInput.x;
				prevY = tempVectorInput.y;
			},
			up: () => {
				this.skeletonList.forEach(({ htmlOptionsList }) => {
					htmlOptionsList.forEach((element) => {
						element.dragging = false;
					});
				});
			}
		})
	}

	/*
	* Resize/scroll utilities
	*/

	private updateCanvasSize() {
		// const pageSize = this.getPageSize();
		// this.canvas.style.width = pageSize.width + "px";
		// this.canvas.style.height = pageSize.height + "px";

		// const displayWidth = window.innerWidth;
    	// const displayHeight = window.innerHeight;


		const displayWidth = document.documentElement.clientWidth;
    	const displayHeight = document.documentElement.clientHeight;
		this.canvas.style.width = displayWidth + "px";
		this.canvas.style.height = displayHeight + "px";
		// this.canvas.style.height = displayHeight + this.additionalPixelsBottom + "px";
	}

	// private scrollHandler = () => {
	// 	const { width, height } = this.getPageSize()

	// 	// const viewportHeightWithScrollbar = window.innerHeight;
	// 	// const viewportHeightNoScrollbar = document.documentElement.clientHeight;
	// 	// const scrollbarHeight = viewportHeightWithScrollbar - viewportHeightNoScrollbar;
	// 	// const bottomY = viewportHeightNoScrollbar + window.scrollY;

	// 	// const viewportWidthWithScrollbar = window.innerWidth;
	// 	// const viewportWidthNoScrollbar = document.documentElement.clientWidth;
	// 	// const scrollbarWidth = viewportWidthWithScrollbar - viewportWidthNoScrollbar;
	// 	// const bottomX = viewportWidthNoScrollbar + window.scrollY;

	// 	// if (bottomX + scrollbarWidth <= width) {
	// 	// 	this.currentTranslateX = window.scrollX;
	// 	// }

	// 	// if (bottomY + scrollbarHeight <= height) {
	// 	// 	this.currentTranslateY = window.scrollY;
	// 	// }

	// 	const viewportHeightNoScrollbar = document.documentElement.clientHeight;
	// 	const bottomY = viewportHeightNoScrollbar + window.scrollY;

	// 	const viewportWidthNoScrollbar = document.documentElement.clientWidth;
	// 	const bottomX = viewportWidthNoScrollbar + window.scrollX;

	// 	if (bottomX <= width) {
	// 		this.currentTranslateX = window.scrollX;
	// 	}

	// 	if (window.scrollY <= this.offsetHeight) {
	// 		console.log(window.scrollY);
	// 		console.log("aaa")
	// 		this.currentTranslateY = 0;
	// 		this.offsetHeightDraw = this.offsetHeight;
	// 	} else if (bottomY + this.additionalPixelsBottom - this.offsetHeight <= height) {
	// 		console.log("bbb")
	// 		this.currentTranslateY = window.scrollY - this.offsetHeight;
	// 		this.offsetHeightDraw = this.offsetHeight;
	// 	} else {
	// 		console.log("ccc")
	// 		this.currentTranslateY = window.scrollY - this.additionalPixelsBottom;
	// 		this.offsetHeightDraw = this.additionalPixelsBottom;
	// 	}

	// 	// translate should be faster
	// 	this.canvas.style.transform =`translate(${this.currentTranslateX}px,${this.currentTranslateY}px)`;
	// 	console.log(`translate(${this.currentTranslateX}px,${this.currentTranslateY}px)`)
	// 	// this.canvas.style.top = `${this.currentTranslateY}px`;
	// 	// this.canvas.style.left = `${this.currentTranslateX}px`;
	// }

	private scrollHandler = () => {
		const { width, height } = this.getPageSize()

		const viewportHeightNoScrollbar = document.documentElement.clientHeight;
		const bottomY = viewportHeightNoScrollbar + window.scrollY;

		const viewportWidthNoScrollbar = document.documentElement.clientWidth;
		const bottomX = viewportWidthNoScrollbar + window.scrollY;

		if (bottomX <= width) {
			this.currentTranslateX = window.scrollX;
		}

		if (bottomY <= height) {
			this.currentTranslateY = window.scrollY;
		}

		this.canvas.style.transform =`translate(${this.currentTranslateX}px,${this.currentTranslateY}px)`;

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

	/*
	* Other utilities
	*/

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

	private screenToWorld(vec: Vector3, x: number, y: number) {
		vec.set(x, y, 0);
		this.spineCanvas.renderer.camera.screenToWorld(vec, this.canvas.clientWidth, this.canvas.clientHeight);
	}

	private inside(point: { x: number; y: number }, rectangle: Rectangle): boolean {
		return (
			point.x >= rectangle.x &&
			point.x <= rectangle.x + rectangle.width &&
			point.y >= rectangle.y &&
			point.y <= rectangle.y + rectangle.height
		);
	}

	// TODO
	dispose () {
		this.spineCanvas.dispose();
		this.canvas.remove();
		this.disposed = true;
		this.resizeObserver.disconnect();
	}
}
