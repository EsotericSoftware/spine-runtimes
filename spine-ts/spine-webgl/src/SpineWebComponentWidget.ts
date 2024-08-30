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

import { SpineCanvas, SpineCanvasApp, AtlasAttachmentLoader, SkeletonBinary, SkeletonJson, Skeleton, Animation, AnimationState, AnimationStateData, Physics, Vector2, Vector3, ResizeMode, Color, MixBlend, MixDirection, SceneRenderer, SkeletonData, Input, LoadingScreenWidget, TextureAtlas, Texture } from "./index.js";

interface Rectangle {
    x: number,
    y: number,
    width: number,
    height: number,
}

type UpdateSpineWidgetFunction = (canvas: SpineCanvas, delta: number, skeleton: Skeleton, state: AnimationState) => void;

type OffScreenUpdateBehaviourType = "pause" | "update" | "pose";
function isOffScreenUpdateBehaviourType(value: string): value is OffScreenUpdateBehaviourType {
    return (
        value === "pause" ||
        value === "update" ||
        value === "pose"
    );
}

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

// TODO: add missing assets to main assets folder (chibi)

class SpineWebComponentWidget extends HTMLElement implements WidgetLayoutOptions, WidgetInternalState, Partial<WidgetPublicState> {

    // skeleton options
    public atlasPath: string;
    public skeletonPath: string;
    public scale = 1;
    public animation?: string;
    public skin?: string;
    skeletonData?: SkeletonData; // TODO
	update?: UpdateSpineWidgetFunction;
	beforeUpdateWorldTransforms: UpdateSpineWidgetFunction = () => {};
	afterUpdateWorldTransforms: UpdateSpineWidgetFunction= () => {};

    // layout options
    public fit: FitType = "contain";
    public mode: ModeType = "inside";
    public offsetX = 0;
    public offsetY = 0;
    public xAxis = 0;
    public yAxis = 0;
    public width = -1;
    public height = -1;
    public draggable = false;
    public debug = false;
    public identifier = "";
    public loadingSpinner = true;
    public manualStart = false;
    public pages?: Array<number>;
    public offScreenUpdateBehaviour: OffScreenUpdateBehaviourType = "pause";
    public clip = false;

    // state
    public skeleton?: Skeleton;
    public state?: AnimationState;
    public bounds?: Rectangle;
    public loadingPromise?: Promise<WidgetPublicState>;
    public loading = true;
    public started = false;
    public onScreenAtLeastOnce = false;

    public onScreenFunction: (widget: SpineWebComponentWidget) => void = async (widget) => {
        if (widget.loading && !widget.onScreenAtLeastOnce) {
            widget.onScreenAtLeastOnce = true;

            if (widget.manualStart) {
                widget.start();
            }
        }
    }

    // TODO: makes the interface exposes getter, make getter and make these private
    // internal state
    public currentScaleDpi = 1;
    public worldOffsetX = 0;
    public worldOffsetY = 0;
    public dragX = 0;
    public dragY = 0;
    public dragging = false;
    public intersectionObserver? : IntersectionObserver;
    public onScreen = false;
    public textureAtlas?: TextureAtlas;

    private root: ShadowRoot;
    private overlay: SpineWebComponentOverlay;

    private divLoader: HTMLDivElement;

    public loadingScreen: LoadingScreenWidget | null = null;

    static get observedAttributes(): string[] {
        return [
            "atlas",
            "skeleton",
            "scale",
            "animation",
            "skin",
            "fit",
            "width",
            "height",
            "draggable",
            "mode",
            "x-axis",
            "y-axis",
            "identifier",
            "offset-x",
            "offset-y",
            "debug",
            "manual-start",
            "spinner",
            "pages",
            "offscreen",
            "clip",
        ];
    }

    constructor() {
        super();
        this.root = this.attachShadow({ mode: "open" });
        this.overlay = this.initializeOverlay();
        this.atlasPath = "TODO";
        this.skeletonPath = "TODO";

        this.divLoader = document.createElement("div");
        this.divLoader.classList.add("container-loader");

        const loader = document.createElement("div");
        loader.classList.add("loader");
        this.divLoader.appendChild(loader);
    }

    connectedCallback() {
        if (!this.atlasPath) {
            throw new Error("Missing atlas attribute");
        }

        if (!this.skeletonPath) {
            throw new Error("Missing skeleton attribute");
        }

        this.overlay.addWidget(this);
        if (!this.manualStart) {
            this.start();
        }

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
        if (newValue !== null) {
            if (name === "identifier") {
                this.identifier = newValue;
            }

            if (name === "atlas") {
                this.atlasPath = newValue;
            }

            if (name === "skeleton") {
                this.skeletonPath = newValue;
            }

            if (name === "skin") {
                this.skin = newValue;
            }

            if (name === "fit") {
                this.fit = isFitType(newValue) ? newValue : "contain";
            }

            if (name === "mode") {
                this.mode = isModeType(newValue) ? newValue : "inside";
            }

            if (name === "offscreen") {
                this.offScreenUpdateBehaviour = isOffScreenUpdateBehaviourType(newValue) ? newValue : "pause";
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

            if (name === "spinner") {
                this.loadingSpinner = Boolean(newValue);
            }

            if (name === "manual-start") {
                this.manualStart = Boolean(newValue);
            }

            if (name === "clip") {
                this.clip = Boolean(newValue);
            }

            if (name === "pages") {
                this.pages = newValue.split(",").reduce((acc, pageIndex) => {
                    const index = parseInt(pageIndex);
                    if (!isNaN(index)) acc.push(index);
                    return acc;
                }, [] as Array<number>);
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

    public start() {
        // if you want to start again the widget, first reset it
        if (this.started) return;
        this.started = true;

        this.loadingPromise = this.loadSkeleton();
        this.loadingPromise.then(() => {
            this.loading = false;
        });
    }

    public async loadTexturesInPagesAttribute(atlas: TextureAtlas) {
        const pagesIndexToLoad = this.pages ?? atlas.pages.map((_, i) => i); // if no pages provided, loads all

        const atlasPath = this.atlasPath.includes('/') ? this.atlasPath.substring(0, this.atlasPath.lastIndexOf('/') + 1) : '';
        const promisePageList: Array<Promise<void>> = [];
        pagesIndexToLoad.forEach((index) => {
            const page = atlas.pages[index];
            const promiseTextureLoad = this.loadTexture(`${atlasPath}${page.name}`).then(texture => page.setTexture(texture));
            promisePageList.push(promiseTextureLoad);
        });

        return Promise.all(promisePageList)
    }

    // add a skeleton to the overlay and set the bounds to the given animation or to the setup pose
    private async loadSkeleton() {
        this.loading = true;
        // if (this.identifier !== "TODELETE") return Promise.reject();
		const { atlasPath, skeletonPath, scale = 1, animation, skeletonData: skeletonDataInput, skin } = this;
		const isBinary = skeletonPath.endsWith(".skel");

        // skeleton and atlas txt are loaded immeaditely
        // textures are loaeded depending on the 'pages' param:
        // - [0,2]: only pages at index 0 and 2 are loaded
        // - []: no page is loaded
        // - undefined: all pages are loaded (default)
		await Promise.all([
			isBinary ? this.loadBinary(skeletonPath) : this.loadJson(skeletonPath),
			this.loadTextureAtlasButNoTextures(atlasPath).then(atlas => this.loadTexturesInPagesAttribute(atlas)),
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

        if (skin) {
			skeleton.setSkinByName(skin);
		}

		let animationData;
		if (animation) {
			state.setAnimation(0, animation, true);
			animationData = animation ? skeleton.data.findAnimation(animation)! : undefined;
		}

        // ideally we would know the dpi and the zoom, however they are combined
        // to simplify we just assume that the user wants to load the skeleton at scale 1
        // at the current browser zoom level
        // this might be problematic for free-scale modes (origin and inside+none)
        this.currentScaleDpi = window.devicePixelRatio;
		// skeleton.scaleX = this.currentScaleDpi;
		// skeleton.scaleY = this.currentScaleDpi;

        this.skeleton = skeleton;
        this.state = state;
        this.textureAtlas = atlas;

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
        let width;
        let height;
        if (this.width === -1 || this.height === -1) {
            width = "0";
            height = "0";
        } else {
            width = `${this.width}px`
            height = `${this.height}px`
        }
        this.root.innerHTML = `
        <style>
            :host {
                position: relative;
                display: inline-block;
                width:  ${width};
                height: ${height};
                // background-color: red;
            }
        </style>
        `;
    }

    /*
	* Load assets utilities
	*/

	public async loadBinary(path: string) {
		return new Promise((resolve, reject) => {
			this.overlay.spineCanvas.assetManager.loadBinary(path,
				(_, binary) => resolve(binary),
				(_, message) => reject(message),
			);
		});
	}

	public async loadJson(path: string) {
		return new Promise((resolve, reject) => {
			this.overlay.spineCanvas.assetManager.loadJson(path,
				(_, object) => resolve(object),
				(_, message) => reject(message),
			);
		});
	}

    public async loadTexture(path: string) {
		return new Promise<Texture>((resolve, reject) => {
			this.overlay.spineCanvas.assetManager.loadTexture(path,
				(_, texture) => resolve(texture),
				(_, message) => reject(message),
			);
		});
	}

	public async loadTextureAtlas(path: string) {
		return new Promise((resolve, reject) => {
			this.overlay.spineCanvas.assetManager.loadTextureAtlas(path,
				(_, atlas) => resolve(atlas),
				(_, message) => reject(message),
			);
		});
	}

    public async loadTextureAtlasButNoTextures(path: string) {
		return new Promise<TextureAtlas>((resolve, reject) => {
			this.overlay.spineCanvas.assetManager.loadTextureAtlasButNoTextures(path,
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

    private intersectionObserver? : IntersectionObserver;
    private resizeObserver:ResizeObserver;
    private input: Input;

    // TODO: the canvas is now beg as the screen - maybe we can reduce the enhancement of the canvas and just translate
	// how many pixels to add to the edges to prevent "edge cuttin" on fast scrolling
    // be aware that the canvas is already big as the display size
    // making it bigger might reduce performance significantly
	private overflowTop = .2;
	private overflowBottom = .0;
	private overflowLeft = .0;
	private overflowRight = .0;
	private overflowLeftSize: number
	private overflowTopSize: number;

    private currentCanvasBaseWidth = 0;
    private currentCanvasBaseHeight = 0;

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

        // sono per l'ennesima volta fermo alla dimensione del canvas.
        // purtroppo la dimensione dello schermo non permette di determinare la posizione dei div container relativamente allo schermo
        // se trovassimo un modo per farlo, sarebbe fatta. provare prima a investigare un pochino lì.
        //
        // alternativamente giocare ancora con i lvh o qualcosa del genere
        // purtroppo ho già provato un po' e si hanno oggettivamente dei brevi flash allo scroll quando la dimensione del canvas
        // ovvero quando compare o scompare la barra durante lo scroll da mobile

		// this.canvas.style.width = "100lvw";
		// this.canvas.style.height = "120lvh";
		this.canvas.style.setProperty("pointer-events", "none");
		this.canvas.style.transform =`translate(0px,0px)`;
		// this.canvas.style.setProperty("will-change", "transform"); // performance seems to be even worse with this uncommented

        this.fps = document.createElement('span');
        this.fps.style.position = "fixed";
		this.fps.style.top = "0";
		this.fps.style.left = "0";
        this.root.appendChild(this.fps);

        this.spineCanvas = new SpineCanvas(this.canvas, { app: this.setupSpineCanvasApp() });

        this.updateCanvasSize();
        this.zoomHandler();

        // translateCanvas starts a requestAnimationFrame loop
        this.translateCanvas();

		this.overflowLeftSize = this.overflowLeft * document.documentElement.clientWidth;
		this.overflowTopSize = this.overflowTop * document.documentElement.clientHeight;

        // resize and zoom
		// TODO: should I use the resize event?
		this.resizeObserver = new ResizeObserver(() => {
            this.updateCanvasSize();
			this.zoomHandler();
        });
        this.resizeObserver.observe(document.body);

        const screen = window.screen;
        screen.orientation.onchange = () => {
            this.updateCanvasSize();
            // after an orientation change the scrolling changes, but the scroll event does not fire
            this.scrollHandler();
        }

		window.addEventListener('scroll', this.scrollHandler);
		this.scrollHandler();

        this.input = new Input(document.body, false);
		this.setupDragUtility();
    }

    addWidget(widget: SpineWebComponentWidget) {
        this.skeletonList.push(widget);
        this.intersectionObserver!.observe(widget.getHTMLElementReference());
    }

    private setupSpineCanvasApp(): SpineCanvasApp {
		const red = new Color(1, 0, 0, 1);
		const green = new Color(0, 1, 0, 1);
		const blue = new Color(0, 0, 1, 1);
		const transparentWhite = new Color(1, 1, 1, .3);
		return {
			update: (canvas: SpineCanvas, delta: number) => {
				this.skeletonList.forEach(({ skeleton, state, update, onScreen, offScreenUpdateBehaviour, skeletonPath, beforeUpdateWorldTransforms, afterUpdateWorldTransforms }) => {
                    if (!skeleton || !state) return;
                    if (!onScreen && offScreenUpdateBehaviour === "pause") return;
					if (update) update(canvas, delta, skeleton, state)
					else {
                        // delta = 0
						state.update(delta);
						skeleton.update(delta);

                        if (onScreen || (!onScreen && offScreenUpdateBehaviour === "pose") ) {
                            state.apply(skeleton);
                            beforeUpdateWorldTransforms(canvas, delta, skeleton, state);
                            skeleton.updateWorldTransform(Physics.update);
                            afterUpdateWorldTransforms(canvas, delta, skeleton, state);
                        }
					}
				});
				this.fps.innerText = canvas.time.framesPerSecond.toFixed(2) + " fps";
			},

			render: (canvas: SpineCanvas) => {
				canvas.clear(0, 0, 0, 0);
				let renderer = canvas.renderer;
				renderer.begin();

				const devicePixelRatio = window.devicePixelRatio;
				const tempVector = new Vector3();
				this.skeletonList.forEach((widget) => {
                    const { skeleton, bounds, mode, debug, offsetX, offsetY, xAxis, yAxis, dragX, dragY, fit, loadingSpinner, onScreen, loading, clip } = widget;

                    if ((!onScreen && dragX === 0 && dragY === 0)) return;
                    const divBounds = widget.getHTMLElementReference().getBoundingClientRect();
                    divBounds.x += this.overflowLeftSize;
                    divBounds.y += this.overflowTopSize;

                    // TODO: drag does not work while clip is on

                    // TODO: now that the clip logic works, remove the code duplication
                    if (clip) {
                        const clipToBoundStart = (canvas: SpineCanvas, divBounds: Rectangle) => {
                            const renderer = canvas.renderer;

                            // break current batch and start a new one
                            renderer.end();

                            // set the new viewport to the div bound
                            const viewportWidth = divBounds.width * window.devicePixelRatio;
                            const viewporthHeight = divBounds.height * window.devicePixelRatio;
                            canvas.gl.viewport(
                                divBounds.x * window.devicePixelRatio,
                                this.canvas.height - (divBounds.y + divBounds.height) * window.devicePixelRatio,
                                viewportWidth,
                                viewporthHeight
                            );
                            renderer.camera.setViewport(viewportWidth, viewporthHeight);
                            renderer.camera.update();

                            // start the new batch that will be filled with only this skeleton
                            renderer.begin();

                            // TODO: debug - to remove
                            if (false) {
                                renderer.circle(true, -viewportWidth / 2, -viewporthHeight / 2, 20, red);
                                renderer.circle(true, viewportWidth / 2, -viewporthHeight / 2, 20, red);
                                renderer.circle(true, -viewportWidth / 2, viewporthHeight / 2, 20, red);
                                renderer.circle(true, viewportWidth / 2, viewporthHeight / 2, 20, red);
                                renderer.circle(true, 0, 0, 10, red);

                                renderer.rect(true, 0, 0, -viewportWidth, -viewporthHeight, transparentWhite);
                                renderer.rect(true, 0, 0, viewportWidth, viewporthHeight, transparentWhite);
                            }
                        }

                        const clipToBoundEnd = () =>  {
                            // end clip batch
                            renderer.end();

                            canvas.gl.viewport(0, 0, this.canvas.width, this.canvas.height);
                            canvas.renderer.camera.setViewport(this.canvas.width, this.canvas.height);
                            canvas.renderer.camera.update();

                            // start new normal batch
                            renderer.begin();
                        }



                        clipToBoundStart(canvas, divBounds)



                        // get the desired point into the the div (center by default) in world coordinate
                        const divX = divBounds.x + divBounds.width * (xAxis + .5);
                        const divY = divBounds.y + divBounds.height * (-yAxis + .5);
                        this.screenToWorld(tempVector, divX, divY);

                        if (loading) {
                            if (loadingSpinner) {
                                if (!widget.loadingScreen) widget.loadingScreen = new LoadingScreenWidget(renderer);
                                widget.loadingScreen!.draw(true, tempVector.x, tempVector.y, divBounds.width * devicePixelRatio, divBounds.height * devicePixelRatio);
                            }
                            return;
                        }

                        if (skeleton) {
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
                                x = - boundsX;
                                y = - boundsY;

                                if (fit !== "none") {
                                    // scale the skeleton
                                    skeleton.scaleX = ratioW;
                                    skeleton.scaleY = ratioH;
                                    skeleton.updateWorldTransform(Physics.update);
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
                        }



                        clipToBoundEnd();

                    }





                    else {


                        // get the desired point into the the div (center by default) in world coordinate
                        const divX = divBounds.x + divBounds.width * (xAxis + .5);
                        const divY = divBounds.y + divBounds.height * (-yAxis + .5);
                        this.screenToWorld(tempVector, divX, divY);

                        if (loading) {
                            if (loadingSpinner) {
                                if (!widget.loadingScreen) widget.loadingScreen = new LoadingScreenWidget(renderer);
                                widget.loadingScreen!.draw(true, tempVector.x, tempVector.y, divBounds.width * devicePixelRatio, divBounds.height * devicePixelRatio);
                            }
                            return;
                        }

                        if (skeleton) {
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
                                    skeleton.updateWorldTransform(Physics.update);
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
                        }


                    }








				});

				renderer.end();
			},
		}

	}

    connectedCallback(): void {
        // TODO: move the intersectio observer to the canvas - so that we can instantiate a single one rather than one per widget

        this.intersectionObserver = new IntersectionObserver((widgets) => {
            widgets.forEach(({ isIntersecting, target }) => {

                const widget = this.skeletonList.find(w => w.getHTMLElementReference() == target);
                if (!widget) return;
                widget.onScreen = isIntersecting;
                if (isIntersecting) {
                    widget.onScreenFunction(widget);
                }
            })
        }, { rootMargin: "30px 20px 30px 20px" });
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
					if (!widget.draggable || (!widget.onScreen && widget.dragX === 0 && widget.dragY === 0)) return;

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
                    if (!widget.dragging || (!widget.onScreen && widget.dragX === 0 && widget.dragY === 0)) return;
                    const skeleton = widget.skeleton!;
                    skeleton.physicsTranslate(dragX, dragY);
                    widget.dragX += dragX;
                    widget.dragY += dragY;
                    ev?.preventDefault();
                    ev?.stopPropagation()
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
		// resize canvas, if necessary
		this.resizeCanvas();

		// temporarely remove the div to get the page size without considering the div
		// this is necessary otherwise if the bigger element in the page is remove and the div
		// was the second bigger element, now it would be the div to determine the page size
		this.div.remove();
		const { width, height } = this.getPageSize();
		this.root.appendChild(this.div);

		this.div.style.width = width + "px";
		this.div.style.height = height + "px";
	}

    private resizeCanvas() {
        const { width, height } = this.getScreenSize();

		if (this.currentCanvasBaseWidth !== width || this.currentCanvasBaseHeight !== height) {
            this.currentCanvasBaseWidth = width;
            this.currentCanvasBaseHeight = height;
            this.overflowLeftSize = this.overflowLeft * width;
		    this.overflowTopSize = this.overflowTop * height;

            const totalWidth = width * (1 + (this.overflowLeft + this.overflowRight));
            const totalHeight = height * (1 + (this.overflowTop + this.overflowBottom));

            this.canvas.style.width = totalWidth + "px";
		    this.canvas.style.height = totalHeight + "px";
            this.spineCanvas.renderer.resize3(totalWidth, totalHeight);
        }
	}

    // right now, we scroll the canvas each frame, that makes scrolling on mobile waaay more smoother
    // this is way scroll handler do nothing
    private scrollHandler = () => {
		// this.translateCanvas();
	}

    private translateCanvas() {
		const scrollPositionX = window.scrollX - this.overflowLeftSize;
		const scrollPositionY = window.scrollY - this.overflowTopSize;
		this.canvas.style.transform =`translate(${scrollPositionX}px,${scrollPositionY}px)`;
        requestAnimationFrame(() => this.translateCanvas());
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

            // TODO: improve this horrible thing
            this.spineCanvas.renderer.resize3(
                +this.canvas.style.width.substring(0, this.canvas.style.width.length - 2),
                +this.canvas.style.height.substring(0, this.canvas.style.height.length - 2),
            );
		})
	}

    // we need the bounding client rect otherwise decimals won't be returned
    // this means that during zoom it might occurs that the div would be resized
    // rounded 1px more making a scrollbar appear
    private getPageSize() {
		return document.body.getBoundingClientRect();
	}

    // screen size remain the same when it is rotated
    // we need to swap them based and the orientation angle
    private getScreenSize() {
        const { screen } = window;
        const { width, height } = window.screen;
        const angle = screen.orientation.angle;
        const rotated = angle === 90 || angle === 270;
        return rotated
            ? { width: height, height: width }
            : { width, height };
    }

    /*
    * Other utilities
    */
    private screenToWorld(vec: Vector3, x: number, y: number) {
        vec.set(x, y, 0);
        // pay attention that clientWidth/Height rounds the size - if we don't like it, we should use getBoundingClientRect as in getPagSize
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

export function createSpineWidget(parameters: { atlas: string, skeleton: string, animation: string, skin: string, manualStart?: boolean, pages: Array<string> }): SpineWebComponentWidget {
    const {
        atlas,
        skeleton,
        animation,
        skin,
        manualStart = false,
        pages = [],
    } = parameters;

    const widget = document.createElement("spine-widget") as SpineWebComponentWidget;

    widget.setAttribute("skeleton", skeleton);
    widget.setAttribute("atlas", atlas);
    widget.setAttribute("skin", skin);
    widget.setAttribute("animation", animation);
    widget.setAttribute("manual-start", `${manualStart}`);
    widget.setAttribute("pages", `${pages.join(",")}`);

    if (!manualStart) {
        widget.start();
    }

    return widget;
}