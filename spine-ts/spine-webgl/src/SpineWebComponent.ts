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

const loadingSpinner = "data:image/svg+xml,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20viewBox%3D%220%200%20104%2031.16%22%3E%3Cpath%20d%3D%22M104%2012.68a1.31%201.31%200%200%201-.37%201%201.28%201.28%200%200%201-.85.31H91.57a10.51%2010.51%200%200%200%20.29%202.55%204.92%204.92%200%200%200%201%202%204.27%204.27%200%200%200%201.64%201.26%206.89%206.89%200%200%200%202.6.44%2010.66%2010.66%200%200%200%202.17-.2%2012.81%2012.81%200%200%200%201.64-.44q.69-.25%201.14-.44a1.87%201.87%200%200%201%20.68-.2.44.44%200%200%201%20.27.04.43.43%200%200%201%20.16.2%201.38%201.38%200%200%201%20.09.37%204.89%204.89%200%200%201%200%20.58%204.14%204.14%200%200%201%200%20.43v.32a.83.83%200%200%201-.09.26%201.1%201.1%200%200%201-.17.22%202.77%202.77%200%200%201-.61.34%208.94%208.94%200%200%201-1.32.46%2018.54%2018.54%200%200%201-1.88.41%2013.78%2013.78%200%200%201-2.28.18%2010.55%2010.55%200%200%201-3.68-.59%206.82%206.82%200%200%201-2.66-1.74%207.44%207.44%200%200%201-1.63-2.89%2013.48%2013.48%200%200%201-.55-4%2012.76%2012.76%200%200%201%20.57-3.94%208.35%208.35%200%200%201%201.64-3%207.15%207.15%200%200%201%202.58-1.87%208.47%208.47%200%200%201%203.39-.65%208.19%208.19%200%200%201%203.41.64%206.46%206.46%200%200%201%202.32%201.73%207%207%200%200%201%201.3%202.54%2011.17%2011.17%200%200%201%20.43%203.13zm-3.14-.93a5.69%205.69%200%200%200-1.09-3.86%204.17%204.17%200%200%200-3.42-1.4%204.52%204.52%200%200%200-2%20.44%204.41%204.41%200%200%200-1.47%201.15A5.29%205.29%200%200%200%2092%209.75a7%207%200%200%200-.36%202zM80.68%2021.94a.42.42%200%200%201-.08.26.59.59%200%200%201-.25.18%201.74%201.74%200%200%201-.47.11%206.31%206.31%200%200%201-.76%200%206.5%206.5%200%200%201-.78%200%201.74%201.74%200%200%201-.47-.11.59.59%200%200%201-.25-.18.42.42%200%200%201-.08-.26V12a9.8%209.8%200%200%200-.23-2.35%204.86%204.86%200%200%200-.66-1.53%202.88%202.88%200%200%200-1.13-1%203.57%203.57%200%200%200-1.6-.34%204%204%200%200%200-2.35.83A12.71%2012.71%200%200%200%2069.11%2010v11.9a.42.42%200%200%201-.08.26.59.59%200%200%201-.25.18%201.74%201.74%200%200%201-.47.11%206.51%206.51%200%200%201-.78%200%206.31%206.31%200%200%201-.76%200%201.88%201.88%200%200%201-.48-.11.52.52%200%200%201-.25-.18.46.46%200%200%201-.07-.26v-17a.53.53%200%200%201%20.03-.21.5.5%200%200%201%20.23-.19%201.28%201.28%200%200%201%20.44-.11%208.53%208.53%200%200%201%201.39%200%201.12%201.12%200%200%201%20.43.11.6.6%200%200%201%20.22.19.47.47%200%200%201%20.07.26V7.2a10.46%2010.46%200%200%201%202.87-2.36%206.17%206.17%200%200%201%202.88-.75%206.41%206.41%200%200%201%202.87.58%205.16%205.16%200%200%201%201.88%201.54%206.15%206.15%200%200%201%201%202.26%2013.46%2013.46%200%200%201%20.31%203.11z%22%20fill%3D%22%23fff%22%2F%3E%3Cpath%20d%3D%22M43.35%202.86c.09%202.6%201.89%204%205.48%204.61%203%20.48%205.79.24%206.69-2.37%201.75-5.09-2.4-3.82-6-4.39s-6.31-2.03-6.17%202.15zm1.08%2010.69c.33%201.94%202.14%203.06%204.91%203s4.84-1.16%205.13-3.25c.53-3.88-2.53-2.38-5.3-2.3s-5.4-1.26-4.74%202.55zM48%2022.44c.55%201.45%202.06%202.06%204.1%201.63s3.45-1.11%203.33-2.76c-.21-3.06-2.22-2.1-4.26-1.66S47%2019.6%2048%2022.44zm1.78%206.78c.16%201.22%201.22%202%202.88%201.93s2.92-.67%203.13-2c.4-2.43-1.46-1.53-3.12-1.51s-3.17-.82-2.89%201.58z%22%20fill%3D%22%23ff4000%22%2F%3E%3Cpath%20d%3D%22M35.28%2013.16a15.33%2015.33%200%200%201-.48%204%208.75%208.75%200%200%201-1.42%203%206.35%206.35%200%200%201-2.32%201.91%207.14%207.14%200%200%201-3.16.67%206.1%206.1%200%200%201-1.4-.15%205.34%205.34%200%200%201-1.26-.47%207.29%207.29%200%200%201-1.24-.81q-.61-.49-1.29-1.15v8.51a.47.47%200%200%201-.08.26.56.56%200%200%201-.25.19%201.74%201.74%200%200%201-.47.11%206.47%206.47%200%200%201-.78%200%206.26%206.26%200%200%201-.76%200%201.89%201.89%200%200%201-.48-.11.49.49%200%200%201-.25-.19.51.51%200%200%201-.07-.26V4.91a.57.57%200%200%201%20.06-.27.46.46%200%200%201%20.23-.18%201.47%201.47%200%200%201%20.44-.1%207.41%207.41%200%200%201%201.3%200%201.45%201.45%200%200%201%20.43.1.52.52%200%200%201%20.24.18.51.51%200%200%201%20.07.27V7.2a18.06%2018.06%200%200%201%201.49-1.38%209%209%200%200%201%201.45-1%206.82%206.82%200%200%201%201.49-.59%207.09%207.09%200%200%201%204.78.52%206%206%200%200%201%202.13%202%208.79%208.79%200%200%201%201.2%202.9%2015.72%2015.72%200%200%201%20.4%203.51zm-3.28.36a15.64%2015.64%200%200%200-.2-2.53%207.32%207.32%200%200%200-.69-2.17%204.06%204.06%200%200%200-1.3-1.51%203.49%203.49%200%200%200-2-.57%204.1%204.1%200%200%200-1.2.18%204.92%204.92%200%200%200-1.2.57%208.54%208.54%200%200%200-1.28%201A15.77%2015.77%200%200%200%2022.76%2010v6.77a13.53%2013.53%200%200%200%202.46%202.4%204.12%204.12%200%200%200%202.44.83%203.56%203.56%200%200%200%202-.57A4.28%204.28%200%200%200%2031%2018a7.58%207.58%200%200%200%20.77-2.12%2011.43%2011.43%200%200%200%20.23-2.36zM12%2017.3a5.39%205.39%200%200%201-.48%202.33%204.73%204.73%200%200%201-1.37%201.72%206.19%206.19%200%200%201-2.12%201.06%209.62%209.62%200%200%201-2.71.36%2010.38%2010.38%200%200%201-3.21-.5A7.63%207.63%200%200%201%201%2021.82a3.25%203.25%200%200%201-.66-.43%201.09%201.09%200%200%201-.3-.53%203.59%203.59%200%200%201-.04-.93%204.06%204.06%200%200%201%200-.61%202%202%200%200%201%20.09-.4.42.42%200%200%201%20.16-.22.43.43%200%200%201%20.24-.07%201.35%201.35%200%200%201%20.61.26q.41.26%201%20.56a9.22%209.22%200%200%200%201.41.55%206.25%206.25%200%200%200%201.87.26%205.62%205.62%200%200%200%201.44-.17%203.48%203.48%200%200%200%201.12-.5%202.23%202.23%200%200%200%20.73-.84%202.68%202.68%200%200%200%20.26-1.21%202%202%200%200%200-.37-1.21%203.55%203.55%200%200%200-1-.87%208.09%208.09%200%200%200-1.36-.66l-1.56-.61a16%2016%200%200%201-1.57-.73%206%206%200%200%201-1.37-1%204.52%204.52%200%200%201-1-1.4%204.69%204.69%200%200%201-.37-2%204.88%204.88%200%200%201%20.39-1.87%204.46%204.46%200%200%201%201.16-1.61%205.83%205.83%200%200%201%201.94-1.11A8.06%208.06%200%200%201%206.53%204a8.28%208.28%200%200%201%201.36.11%209.36%209.36%200%200%201%201.23.28%205.92%205.92%200%200%201%20.94.37%204.09%204.09%200%200%201%20.59.35%201%201%200%200%201%20.26.26.83.83%200%200%201%20.09.26%201.32%201.32%200%200%200%20.06.35%203.87%203.87%200%200%201%200%20.51%204.76%204.76%200%200%201%200%20.56%201.39%201.39%200%200%201-.09.39.5.5%200%200%201-.16.22.35.35%200%200%201-.21.07%201%201%200%200%201-.49-.21%207%207%200%200%200-.83-.44%209.26%209.26%200%200%200-1.2-.44%205.49%205.49%200%200%200-1.58-.16%204.93%204.93%200%200%200-1.4.18%202.69%202.69%200%200%200-1%20.51%202.16%202.16%200%200%200-.59.83%202.43%202.43%200%200%200-.2%201%202%202%200%200%200%20.38%201.24%203.6%203.6%200%200%200%201%20.88%208.25%208.25%200%200%200%201.38.68l1.58.62q.8.32%201.59.72a6%206%200%200%201%201.39%201%204.37%204.37%200%200%201%201%201.36%204.46%204.46%200%200%201%20.37%201.8z%22%20fill%3D%22%23fff%22%2F%3E%3C%2Fsvg%3E";

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
    public skin?: string;
    skeletonData?: SkeletonData; // TODO
	update?: UpdateSpineFunction; // TODO

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

    // state
    public skeleton?: Skeleton;
    public state?: AnimationState;
    public bounds?: Rectangle;
    public loadingPromise?: Promise<WidgetPublicState>;
    public loading = true;

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

    private divLoader: HTMLDivElement;

    static get observedAttributes(): string[] {
      return ["atlas", "skeleton", "scale", "animation", "skin", "fit", "width", "height", "draggable", "mode", "x-axis", "y-axis", "identifier", "offset-x", "offset-y", "debug"];
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

        this.overlay.skeletonList.push(this);
        this.loadingPromise = this.loadSkeleton();
        this.loadingPromise.then(() => {
            this.loading = false;
            this.hideLoader();
        }); // async

        this.render();
        this.root.appendChild(this.divLoader);
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

            if (name === "skin") {
                this.skin = newValue;
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

    private showLoader() {
        this.divLoader.classList.remove("hide-loader");
    }

    private hideLoader() {
        this.divLoader.classList.add("hide-loader");
    }

    // add a skeleton to the overlay and set the bounds to the given animation or to the setup pose
    private async loadSkeleton() {
        this.showLoader();
        // if (this.identifier !== "TODELETE") return Promise.reject();
		const { atlasPath, skeletonPath, scale = 1, animation, skeletonData: skeletonDataInput, update, skin } = this;
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
        const width = this.width === -1 ? "100%" : `${this.width}px`
        const height = this.height === -1 ? "100%" : `${this.height}px`
        this.root.innerHTML = `
        <style>
            :host {
            display: inline-block;
            width:  ${width};
            height: ${height};
            // background-color: red;
            }

            .container-loader {
                width: 100%;
                height: 100%;
                display: flex;
                justify-content: center;
                align-items: center;
            }

            .hide-loader {
                display: none;
            }

            .loader {
                animation: rotate 1s infinite;
                height: 50px;
                width: 50px;
            }

            .loader:before,
            .loader:after {
                border-radius: 50%;
                content: "";
                display: block;
                height: 20px;
                width: 20px;
            }
            .loader:before {
                animation: ball1 1s infinite;
                background-color: #fff;
                box-shadow: 30px 0 0 #ff3d00;
                margin-bottom: 10px;
            }
            .loader:after {
                animation: ball2 1s infinite;
                background-color: #ff3d00;
                box-shadow: 30px 0 0 #fff;
            }

            @keyframes rotate {
                0% { transform: rotate(0deg) scale(0.8) }
                50% { transform: rotate(360deg) scale(1.2) }
                100% { transform: rotate(720deg) scale(0.8) }
            }

            @keyframes ball1 {
                0% {
                box-shadow: 30px 0 0 #ff3d00;
                }
                50% {
                box-shadow: 0 0 0 #ff3d00;
                margin-bottom: 0;
                transform: translate(15px, 15px);
                }
                100% {
                box-shadow: 30px 0 0 #ff3d00;
                margin-bottom: 10px;
                }
            }

            @keyframes ball2 {
                0% {
                box-shadow: 30px 0 0 #fff;
                }
                50% {
                box-shadow: 0 0 0 #fff;
                margin-top: -20px;
                transform: translate(15px, 15px);
                }
                100% {
                box-shadow: 30px 0 0 #fff;
                margin-top: 0;
                }
            }
        </style>
        `;

        // <div>
        //     <div class="loader"></div>
        // </div>
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

                    if (!skeleton) {
                        console.log("aaa")
                        // widget.style.backgroundImage = `url("${loadingSpinner}")`;
                        // widget.style.backgroundColor = `pink`;
                        // widget.classList.add("loading");
                        // widget.classList.add("spine-player-button-icon-spine-logo");
                        // widget.classList.add("lds-hourglass");
                        // widget.shadowRoot?.host.classList.add("spine-player-button-icon-spine-logo");
                        // widget.style.backgroundImage = loadingSpinner;
                        return;
                    }

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