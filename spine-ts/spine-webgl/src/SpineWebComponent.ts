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

type UpdateSpineFunction = (canvas: SpineCanvas, delta: number, skeleton: Skeleton, state: AnimationState) => void;

type ModeType = 'inside' | 'origin';
function isModeType(value: string): value is ModeType {
    return (
        value === "inside" ||
        value === "origin"
    );
}

type FitType = "fill" | "fitWidth" | "fitHeight" | "contain" | "cover" | "none" | "scaleDown";
function isFitType(value: string): value is FitType {
    return (
        value === "fill" ||
        value === "fitWidth" ||
        value === "fitHeight" ||
        value === "contain" ||
        value === "cover" ||
        value === "none" ||
        value === "scaleDown"
    );
}

interface WidgetLayoutOptions {
    mode: ModeType
    debug: boolean
    offsetX: number
    offsetY: number
    xAxis: number
    yAxis: number
    draggable: boolean
    fit: FitType
    width: number
    height: number
    identifier: string
}

interface WidgetPublicState {
    skeleton: Skeleton
    state: AnimationState
    bounds: Rectangle
}

interface WidgetInternalState {
    currentScaleDpi: number
    worldOffsetX: number
    worldOffsetY: number
    dragging: boolean
    dragX: number
    dragY: number
}


class SpineWebComponentWidget extends HTMLElement implements WidgetLayoutOptions, WidgetInternalState, Partial<WidgetPublicState> {

    // skeleton options
    public atlasPath: string;
    public skeletonPath: string;
    public scale = 1;
    public animation?: string;
    skeletonData?: SkeletonData; // TODO
	update?: UpdateSpineFunction; // TODO

    // layout options
    public fit: FitType = "contain";
    public mode: ModeType = "inside";
    public offsetX = 0;
    public offsetY = 0;
    public xAxis = 0;
    public yAxis = 0;
    public width = 0;
    public height = 0;
    public draggable = false;
    public debug = false;
    public identifier = "";

    // state
    public skeleton?: Skeleton;
    public state?: AnimationState;
    public bounds?: Rectangle;
    public loadingPromise?: Promise<WidgetPublicState>;

    // TODO: makes the interface exposes getter, make getter and make these private
    // internal state
    public currentScaleDpi = 1;
    public worldOffsetX = 0;
    public worldOffsetY = 0;
    public dragX = 0;
    public dragY = 0;
    public dragging = false;

    private root: ShadowRoot;
    private overlay: SpineWebComponentOverlay;

    static get observedAttributes(): string[] {
      return ["atlas", "skeleton", "scale", "animation", "fit", "width", "height", "draggable", "mode", "x-axis", "y-axis", "identifier", "offset-x", "offset-y", "debug"];
    }

    constructor() {
        super();
        this.root = this.attachShadow({ mode: "open" });
        this.overlay = this.initializeOverlay();
        this.atlasPath = "TODO";
        this.skeletonPath = "TODO";
    }

    connectedCallback() {
        if (!this.atlasPath) {
            throw new Error("Missing atlas attribute");
        }

        if (!this.skeletonPath) {
            throw new Error("Missing skeleton attribute");
        }

        this.loadingPromise = this.loadSkeleton();
        this.loadingPromise.then(() => {
            this.overlay.skeletonList.push(this);
        }); // async

        this.render();
    }

    disconnectedCallback(): void {
        this.loadingPromise?.then(() => {
            const index = this.overlay.skeletonList.indexOf(this);
            if (index !== -1) {
                this.overlay.skeletonList.splice(index, 1);
            }
        });
    }

    attributeChangedCallback(name: string, oldValue: string | null, newValue: string | null): void {
        if (newValue) {
            if (name === "identifier") {
                this.identifier = newValue;
            }

            if (name === "atlas") {
                this.atlasPath = newValue;
            }

            if (name === "skeleton") {
                this.skeletonPath = newValue;
            }

            if (name === "fit") {
                this.fit = isFitType(newValue) ? newValue : "contain";
            }

            if (name === "mode") {
                this.mode = isModeType(newValue) ? newValue : "inside";
            }

            if (name === "x-axis") {
                let float = this.xAxis;
                float = parseFloat(newValue);
                this.xAxis = float;
            }
            if (name === "y-axis") {
                let float = this.yAxis;
                float = parseFloat(newValue);
                this.yAxis = float;
            }
            if (name === "offset-x") {
                let float = 0;
                float = parseInt(newValue);
                this.offsetX = float;
            }
            if (name === "offset-y") {
                let float = 0;
                float = parseInt(newValue);
                this.offsetY = float;
            }

            if (name === "scale") {
                let scaleFloat = 1;
                scaleFloat = parseFloat(newValue);
                this.scale = scaleFloat;
            }

            if (name === "width") {
                let widthFloat = 1;
                widthFloat = parseFloat(newValue);
                this.width = widthFloat;
            }

            if (name === "height") {
                let heightFloat = 1;
                heightFloat = parseFloat(newValue);
                this.height = heightFloat;
            }

            if (name === "animation") {
                this.animation = newValue;
            }

            if (name === "draggable") {
                this.draggable = Boolean(newValue);
            }

            if (name === "debug") {
                this.debug = Boolean(newValue);
            }
        }
    }

    // calculate bounds of the current animation on track 0, then set it
	public recalculateBounds() {
        const { skeleton, state } = this;
		if (!skeleton || !state) return;
		const track = state.getCurrent(0);
		const animation = track?.animation as (Animation | undefined);
		const bounds = this.calculateAnimationViewport(animation);
		this.setBounds(bounds);
	}

	// set the given bounds on the current skeleton
	// bounds is used to center the skeleton in inside mode and as a input area for click events
	public setBounds(bounds: Rectangle) {
        const { skeleton } = this;
		if (!skeleton) return;
		bounds.x /= skeleton.scaleX;
		bounds.y /= skeleton.scaleY;
		bounds.width /= skeleton.scaleX;
		bounds.height /= skeleton.scaleY;
        this.bounds = bounds;
	}

    private initializeOverlay(): SpineWebComponentOverlay {
        let overlay = document.querySelector("spine-overlay") as SpineWebComponentOverlay;
        if (!overlay) {
            overlay = document.createElement("spine-overlay") as SpineWebComponentOverlay;
            document.body.appendChild(overlay);
        }
        return overlay;
    }

    // add a skeleton to the overlay and set the bounds to the given animation or to the setup pose
    private async loadSkeleton() {
        // if (this.identifier !== "TODELETE") return Promise.reject();
		const { atlasPath, skeletonPath, scale = 1, animation, skeletonData: skeletonDataInput, update } = this;
		const isBinary = skeletonPath.endsWith(".skel");

        // TODO: when multiple component are loaded, they do no reuse the asset manager cache.
        // TODO: we need to reuse the same texture atlas to allow batching when skeletons use the same texture
		await Promise.all([
			isBinary ? this.loadBinary(skeletonPath) : this.loadJson(skeletonPath),
			this.loadTextureAtlas(atlasPath),
		]);

		const atlas = this.overlay.spineCanvas.assetManager.require(atlasPath);
		const atlasLoader = new AtlasAttachmentLoader(atlas);

		const skeletonLoader = isBinary ? new SkeletonBinary(atlasLoader) : new SkeletonJson(atlasLoader);
		skeletonLoader.scale = scale;

		const skeletonFile = this.overlay.spineCanvas.assetManager.require(skeletonPath);
		const skeletonData = skeletonDataInput ?? skeletonLoader.readSkeletonData(skeletonFile);

		const skeleton = new Skeleton(skeletonData);
		const animationStateData = new AnimationStateData(skeletonData);
		const state = new AnimationState(animationStateData);

		let animationData;
		if (animation) {
			state.setAnimation(0, animation, true);
			animationData = animation ? skeleton.data.findAnimation(animation)! : undefined;
		}

        // ideally we would know the dpi and the zoom, however they are combined
        // to simplify we just assume that the user wants to load the skeleton at scale 1
        // at the current browser zoom level
        this.currentScaleDpi = window.devicePixelRatio;
		// skeleton.scaleX = this.currentScaleDpi;
		// skeleton.scaleY = this.currentScaleDpi;

        this.skeleton = skeleton;
        this.state = state;

		const bounds = this.calculateAnimationViewport(animationData);
        this.bounds = bounds;
		return { skeleton, state, bounds };
	}

    public getHTMLElementReference(): HTMLElement {
        return this.width <= 0 || this.width <= 0
            ? this.parentElement!
            : this;
    }

    private render(): void {
      this.root.innerHTML = `
        <style>
          :host {
            display: inline-block;
            width: ${this.width}px;
            height: ${this.height}px;
            background-color: red;
          }
        </style>
      `;
    }

    /*
	* Load assets utilities
	*/

	private async loadBinary(path: string) {
		return new Promise((resolve, reject) => {
			this.overlay.spineCanvas.assetManager.loadBinary(path,
				(_, binary) => resolve(binary),
				(_, message) => reject(message),
			);
		});
	}

	private async loadJson(path: string) {
		return new Promise((resolve, reject) => {
			this.overlay.spineCanvas.assetManager.loadJson(path,
				(_, object) => resolve(object),
				(_, message) => reject(message),
			);
		});
	}

	private async loadTextureAtlas(path: string) {
		return new Promise((resolve, reject) => {
			this.overlay.spineCanvas.assetManager.loadTextureAtlas(path,
				(_, atlas) => resolve(atlas),
				(_, message) => reject(message),
			);
		});
	}

    /*
    * Other utilities
    */

    private calculateAnimationViewport (animation?: Animation): Rectangle {
        const renderer = this.overlay.spineCanvas.renderer;
        const { skeleton } = this;
        if (!skeleton) return { x: 0, y: 0, width: 0, height: 0 };
        skeleton.setToSetupPose();

        let offset = new Vector2(), size = new Vector2();
        const tempArray = new Array<number>(2);
        if (!animation) {
            skeleton.updateWorldTransform(Physics.update);
            skeleton.getBounds(offset, size, tempArray, renderer.skeletonRenderer.getSkeletonClipping());
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
            skeleton.getBounds(offset, size, tempArray, renderer.skeletonRenderer.getSkeletonClipping());

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
}

class SpineWebComponentOverlay extends HTMLElement {
    private root: ShadowRoot;

    public spineCanvas:SpineCanvas;
    private div: HTMLDivElement;
    private canvas:HTMLCanvasElement;
    private fps: HTMLSpanElement;

    public skeletonList = new Array<SpineWebComponentWidget>();

    private resizeObserver:ResizeObserver;
    private input: Input;

	// how many pixels to add to the
	private overflowTop = .1;
	private overflowBottom = .2;
	private overflowLeft = .1;
	private overflowRight = .1;
	private overflowLeftSize: number
	private overflowTopSize: number;


    constructor() {
        super();
        this.root = this.attachShadow({ mode: 'open' });

        this.div = document.createElement('div');
		this.div.style.position = "absolute";
		this.div.style.top = "0";
		this.div.style.left = "0";
		this.div.style.setProperty("pointer-events", "none");
		this.div.style.overflow = "hidden"
		// this.div.style.backgroundColor = "rgba(0, 255, 0, 0.3)";

        this.root.appendChild(this.div);

		this.canvas = document.createElement('canvas');
		this.div.appendChild(this.canvas);
		this.canvas.style.position = "absolute";
		this.canvas.style.top = "0";
		this.canvas.style.left = "0";
		this.canvas.style.setProperty("pointer-events", "none");
		this.canvas.style.transform =`translate(0px,0px)`;
		// this.canvas.style.setProperty("will-change", "transform"); // performance seems to be even worse with this uncommented

        this.fps = document.createElement('span');
        this.fps.style.position = "fixed";
		this.fps.style.top = "0";
		this.fps.style.left = "0";
        this.root.appendChild(this.fps);


        // resize and zoom
		// TODO: should I use the resize event?
		this.resizeObserver = new ResizeObserver(() => {
            this.updateCanvasSize();
			this.zoomHandler();
			this.spineCanvas.renderer.resize(ResizeMode.Expand);
        });
        this.resizeObserver.observe(document.body);

		this.updateCanvasSize();
		this.overflowLeftSize = this.overflowLeft * document.documentElement.clientWidth;
		this.overflowTopSize = this.overflowTop * document.documentElement.clientHeight;

		this.zoomHandler();

		// scroll
		window.addEventListener('scroll', this.scrollHandler);
		this.scrollHandler();

        this.spineCanvas = new SpineCanvas(this.canvas, { app: this.setupSpineCanvasApp() });
        this.input = new Input(document.body, false);
		this.setupDragUtility();
    }

    private setupSpineCanvasApp(): SpineCanvasApp {
		const red = new Color(1, 0, 0, 1);
		const green = new Color(0, 1, 0, 1);
		const blue = new Color(0, 0, 1, 1);

		return {
			update: (canvas: SpineCanvas, delta: number) => {
				this.skeletonList.forEach(({ skeleton, state, update }) => {
                    if (!skeleton || !state) return;
					if (update) update(canvas, delta, skeleton, state)
					else {
						state.update(delta);
						state.apply(skeleton);
						skeleton.update(delta);
						skeleton.updateWorldTransform(Physics.update);
					}
				});
				this.fps.innerText = canvas.time.framesPerSecond.toFixed(2) + " fps";
			},

			render: (canvas: SpineCanvas) => {
				// canvas.clear(0, 0 , 0, 0);
				let renderer = canvas.renderer;
				renderer.begin();

				const devicePixelRatio = window.devicePixelRatio;
				const tempVector = new Vector3();
				this.skeletonList.forEach((widget) => {
                    const { skeleton, bounds, mode, debug, offsetX, offsetY, xAxis, yAxis, dragX, dragY, fit } = widget;
                    if (!skeleton) return;

                    const divBounds = widget.getHTMLElementReference().getBoundingClientRect();
                    divBounds.x += this.overflowLeftSize;
                    divBounds.y += this.overflowTopSize;

                    // get the desired point into the the div (center by default) in world coordinate
                    const divX = divBounds.x + divBounds.width * (xAxis + .5);
                    const divY = divBounds.y + divBounds.height * (-yAxis + .5);
                    this.screenToWorld(tempVector, divX, divY);

                    let x = tempVector.x;
                    let y = tempVector.y;
                    if (mode === 'inside') {
                        let { x: ax, y: ay, width: aw, height: ah } = bounds!;

                        // scale ratio
                        const scaleWidth = divBounds.width * devicePixelRatio / aw;
                        const scaleHeight = divBounds.height * devicePixelRatio / ah;

                        let ratioW = skeleton.scaleX;
                        let ratioH = skeleton.scaleY;

                        if (fit === "fill") { // Fill the target box by distorting the source's aspect ratio.
                            ratioW = scaleWidth;
                            ratioH = scaleHeight;
                        } else if (fit === "fitWidth") {
                            ratioW = scaleWidth;
                            ratioH = scaleWidth;
                        } else if (fit === "fitHeight") {
                            ratioW = scaleHeight;
                            ratioH = scaleHeight;
                        } else if (fit === "contain") {
                            // if scaled height is bigger than div height, use height ratio instead
                            if (ah * scaleWidth > divBounds.height * devicePixelRatio){
                                ratioW = scaleHeight;
                                ratioH = scaleHeight;
                            } else {
                                ratioW = scaleWidth;
                                ratioH = scaleWidth;
                            }
                        } else if (fit === "cover") {
                            if (ah * scaleWidth < divBounds.height * devicePixelRatio){
                                ratioW = scaleHeight;
                                ratioH = scaleHeight;
                            } else {
                                ratioW = scaleWidth;
                                ratioH = scaleWidth;
                            }
                        } else if (fit === "scaleDown") {
                            if (aw > divBounds.width * devicePixelRatio || ah > divBounds.height * devicePixelRatio) {
                                if (ah * scaleWidth > divBounds.height * devicePixelRatio){
                                    ratioW = scaleHeight;
                                    ratioH = scaleHeight;
                                } else {
                                    ratioW = scaleWidth;
                                    ratioH = scaleWidth;
                                }
                            }
                        }

                        // get the center of the bounds
                        const boundsX = (ax + aw / 2) * ratioW;
                        const boundsY = (ay + ah / 2) * ratioH;

                        // get vertices offset: calculate the distance between div center and bounds center
                        x = tempVector.x - boundsX;
                        y = tempVector.y - boundsY;

                        if (fit !== "none") {
                            // scale the skeleton
                            skeleton.scaleX = ratioW;
                            skeleton.scaleY = ratioH;
                        }
                    }

                    widget.worldOffsetX = x + offsetX + dragX;
                    widget.worldOffsetY = y + offsetY + dragY;

                    renderer.drawSkeleton(skeleton, true, -1, -1, (vertices, size, vertexSize) => {
                        for (let i = 0; i < size; i+=vertexSize) {
                            vertices[i] = vertices[i] + widget.worldOffsetX;
                            vertices[i+1] = vertices[i+1] + widget.worldOffsetY;
                        }
                    });

                    // drawing debug stuff
                    if (debug) {
                    // if (true) {
                        let { x: ax, y: ay, width: aw, height: ah } = bounds!;

                        // show bounds and its center
                        renderer.rect(false,
                            ax * skeleton.scaleX + widget.worldOffsetX,
                            ay * skeleton.scaleY + widget.worldOffsetY,
                            aw * skeleton.scaleX,
                            ah * skeleton.scaleY,
                            blue);
                        const bbCenterX = (ax + aw / 2) * skeleton.scaleX + widget.worldOffsetX;
                        const bbCenterY = (ay + ah / 2) * skeleton.scaleY + widget.worldOffsetY;
                        renderer.circle(true, bbCenterX, bbCenterY, 10, blue);

                        // show skeleton root
                        const root = skeleton.getRootBone()!;
                        renderer.circle(true, root.x + widget.worldOffsetX, root.y + widget.worldOffsetY, 10, red);

                        // show shifted origin
                        const originX = widget.worldOffsetX - dragX - offsetX;
                        const originY = widget.worldOffsetY - dragY - offsetY;
                        renderer.circle(true, originX, originY, 10, green);

                        // show line from origin to bounds center
                        renderer.line(originX, originY, bbCenterX, bbCenterY, green);
                    }

				});

				renderer.end();
			},
		}

	}

    connectedCallback(): void {
    }

    disconnectedCallback(): void {
    }

    private setupDragUtility() {
		// TODO: we should use document - body might have some margin that offset the click events - Meanwhile I take event pageX/Y
		const tempVectorInput = new Vector3();

		let prevX = 0;
		let prevY = 0;
		this.input.addListener({
			down: (x, y, ev) => {
				const originalEvent = ev instanceof MouseEvent ? ev : ev!.changedTouches[0];
				tempVectorInput.set(originalEvent.pageX - window.scrollX + this.overflowLeftSize, originalEvent.pageY - window.scrollY + this.overflowTopSize, 0);
				this.spineCanvas.renderer.camera.screenToWorld(tempVectorInput, this.canvas.clientWidth, this.canvas.clientHeight);
				this.skeletonList.forEach(widget => {
					if (!widget.draggable) return;

                    const { worldOffsetX, worldOffsetY } = widget;
                    const bounds = widget.bounds!;
                    const skeleton = widget.skeleton!;
                    const newBounds: Rectangle = {
                        x: bounds.x * skeleton.scaleX + worldOffsetX,
                        y: bounds.y * skeleton.scaleY + worldOffsetY,
                        width: bounds.width * skeleton.scaleX,
                        height: bounds.height * skeleton.scaleY,
                    };

                    if (inside(tempVectorInput, newBounds)) {
                        widget.dragging = true;
                        ev?.preventDefault();
                    }
				});
				prevX = tempVectorInput.x;
				prevY = tempVectorInput.y;
			},
			dragged: (x, y, ev) => {
				const originalEvent = ev instanceof MouseEvent ? ev : ev!.changedTouches[0];
				tempVectorInput.set(originalEvent.pageX - window.scrollX + this.overflowLeftSize, originalEvent.pageY - window.scrollY + this.overflowTopSize, 0);
				this.spineCanvas.renderer.camera.screenToWorld(tempVectorInput, this.canvas.clientWidth, this.canvas.clientHeight);
				let dragX = tempVectorInput.x - prevX;
				let dragY = tempVectorInput.y - prevY;
				this.skeletonList.forEach(widget => {
                    if (widget.dragging) {
                        const skeleton = widget.skeleton!;
                        skeleton.physicsTranslate(dragX, dragY);
                        widget.dragX += dragX;
                        widget.dragY += dragY;
                        ev?.preventDefault();
                        ev?.stopPropagation()
                    }
				});
				prevX = tempVectorInput.x;
				prevY = tempVectorInput.y;
			},
			up: () => {
				this.skeletonList.forEach(widget => {
                    widget.dragging = false;
				});
			}
		})
	}

    /*
	* Resize/scroll utilities
	*/

	private updateCanvasSize() {
		// resize canvas
		this.resizeCanvas();

		// recalculate overflow left and size since canvas size changed
		// we could keep the initial values, avoid this and the translation below - even though we don't have a great gain
		this.translateCanvas();

		// temporarely remove the div to get the page size without considering the div
		// this is necessary otherwise if the bigger element in the page is remove and the div
		// was the second bigger element, now it would be the div to dtermine the page size
		this.div.remove();
		const { width, height } = this.getPageSize();
		this.root.appendChild(this.div);

		this.div.style.width = width + "px";
		this.div.style.height = height + "px";
	}

    private resizeCanvas() {
		const displayWidth = document.documentElement.clientWidth;
    	const displayHeight = document.documentElement.clientHeight;
		this.canvas.style.width = displayWidth * (1 + (this.overflowLeft + this.overflowRight)) + "px";
		this.canvas.style.height = displayHeight * (1 + (this.overflowTop + this.overflowBottom)) + "px";
		if (this.spineCanvas) this.spineCanvas.renderer.resize(ResizeMode.Expand);
	}

    private scrollHandler = () => {
		this.translateCanvas();
	}

    private translateCanvas() {
		const displayWidth = document.documentElement.clientWidth;
    	const displayHeight = document.documentElement.clientHeight;

		this.overflowLeftSize = this.overflowLeft * displayWidth;
		this.overflowTopSize = this.overflowTop * displayHeight;

		const scrollPositionX = window.scrollX - this.overflowLeftSize;
		const scrollPositionY = window.scrollY - this.overflowTopSize;
		this.canvas.style.transform =`translate(${scrollPositionX}px,${scrollPositionY}px)`;
	}

    private zoomHandler = () => {
		this.skeletonList.forEach((widget) => {
            // inside mode scale automatically to fit the skeleton within its parent
            if (widget.mode !== 'origin' && widget.fit !== 'none') return;

            const skeleton = widget.skeleton;
            if (!skeleton) return;
            const scale = window.devicePixelRatio;
            skeleton.scaleX = skeleton.scaleX / widget.currentScaleDpi * scale;
            skeleton.scaleY = skeleton.scaleY / widget.currentScaleDpi * scale;
            widget.currentScaleDpi = scale;
		})
	}

    private getPageSize() {
		// we need the bounding client rect otherwise decimals won't be returned
		// this means that during zoom it might occurs that the div would be resized
		// rounded 1px more making a scrollbar appear
		return document.body.getBoundingClientRect();
	}

    /*
    * Other utilities
    */
    private screenToWorld(vec: Vector3, x: number, y: number) {
        vec.set(x, y, 0);
        this.spineCanvas.renderer.camera.screenToWorld(vec, this.canvas.clientWidth, this.canvas.clientHeight);
    }
}

const inside = (point: { x: number; y: number }, rectangle: Rectangle): boolean => {
    return (
        point.x >= rectangle.x &&
        point.x <= rectangle.x + rectangle.width &&
        point.y >= rectangle.y &&
        point.y <= rectangle.y + rectangle.height
    );
}

customElements.define('spine-widget', SpineWebComponentWidget);
customElements.define('spine-overlay', SpineWebComponentOverlay);

export function getSpineWidget(identifier: string) {
    return document.querySelector(`spine-widget[identifier=${identifier}]`);
}