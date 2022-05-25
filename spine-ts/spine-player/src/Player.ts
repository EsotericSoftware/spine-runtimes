/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

import { Animation, AnimationState, AnimationStateData, AtlasAttachmentLoader, Bone, Color, Disposable, Downloader, MathUtils, MixBlend, MixDirection, Skeleton, SkeletonBinary, SkeletonData, SkeletonJson, StringMap, TextureAtlas, TextureFilter, TimeKeeper, TrackEntry, Vector2 } from "@esotericsoftware/spine-core"
import { AssetManager, GLTexture, Input, LoadingScreen, ManagedWebGLRenderingContext, ResizeMode, SceneRenderer, Vector3 } from "@esotericsoftware/spine-webgl"

export interface SpinePlayerConfig {
	/* The URL of the skeleton JSON file (.json). Undefined if binaryUrl is given. */
	jsonUrl?: string

	/* Optional: The name of a field in the JSON that holds the skeleton data. Default: none */
	jsonField?: string

	/* The URL of the skeleton binary file (.skel). Undefined if jsonUrl is given. */
	binaryUrl?: string

	/* The URL of the skeleton atlas file (.atlas). Atlas page images are automatically resolved. */
	atlasUrl?: string

	/* Raw data URIs, mapping a path to base64 encoded raw data. When player's asset manager resolves the jsonUrl, binaryUrl,
	   atlasUrl, or the image paths referenced in the atlas, it will first look for that path in the raw data URIs. This
	   allows embedding assets directly in HTML/JS. Default: none */
	rawDataURIs?: StringMap<string>

	/* Optional: The name of the animation to be played. Default: empty animation */
	animation?: string

	/* Optional: List of animation names from which the user can choose. Default: all animations */
	animations?: string[]

	/* Optional: The default mix time used to switch between two animations. Default: 0.25 */
	defaultMix?: number

	/* Optional: The name of the skin to be set. Default: the default skin */
	skin?: string

	/* Optional: List of skin names from which the user can choose. Default: all skins */
	skins?: string[]

	/* Optional: Whether the skeleton's atlas images use premultiplied alpha. Default: true */
	premultipliedAlpha?: boolean

	/* Optional: Whether to show the player controls. When false, no external CSS file is needed. Default: true */
	showControls?: boolean

	/* Optional: Whether to show the loading animation. Default: true */
	showLoading?: boolean

	/* Optional: Which debugging visualizations are shown. Default: none */
	debug?: {
		bones: boolean
		regions: boolean
		meshes: boolean
		bounds: boolean
		paths: boolean
		clipping: boolean
		points: boolean
		hulls: boolean
	}

	/* Optional: The position and size of the viewport in the skeleton's world coordinates. Default: the bounding box that fits
	  the current animation, 10% padding, 0.25 transition time */
	viewport?: {
		/* Optional: The position and size of the viewport in the skeleton's world coordinates. Default: the bounding box that
		   fits the current animation */
		x?: number
		y?: number
		width?: number
		height?: number

		/* Optional: Padding around the viewport size, given as a number or percentage (eg "25%"). Default: 10% */
		padLeft?: string | number
		padRight?: string | number
		padTop?: string | number
		padBottom?: string | number

		/* Optional: Whether to draw lines showing the viewport bounds. Default: false */
		debugRender?: boolean,

		/* Optional: When the current viewport changes, the time to animate to the new viewport. Default: 0.25 */
		transitionTime?: number

		/* Optional: Viewports for specific animations. Default: none */
		animations?: StringMap<Viewport>
	}

	/* Optional: Whether the canvas is transparent, allowing the web page behind the canvas to show through when
	   backgroundColor alpha is < ff. Default: false */
	alpha?: boolean

	/* Optional: Whether to preserve the drawing buffer. This is needed if you want to take a screenshot via canvas.getDataURL(), Default: false */
	preserveDrawingBuffer: boolean

	/* Optional: The canvas background color, given in the format #rrggbb or #rrggbbaa. Default: #000000ff (black) or when
	   alpha is true #00000000 (transparent) */
	backgroundColor?: string

	/* Optional: The background color used in fullscreen mode, given in the format #rrggbb or #rrggbbaa. Default: backgroundColor */
	fullScreenBackgroundColor?: string

	/* Optional: An image to draw behind the skeleton. Default: none */
	backgroundImage?: {
		url: string

		/* Optional: The position and size of the background image in the skeleton's world coordinates. Default: fills the viewport */
		x?: number
		y?: number
		width?: number
		height?: number
	}

	/* Optional: Whether mipmapping and anisotropic filtering are used for highest quality scaling when available, otherwise the
	   filter settings from the texture atlas are used. Default: true */
	mipmaps?: boolean

	/* Optional: List of bone names that the user can drag to position. Default: none */
	controlBones?: string[]

	/* Optional: Callback when the skeleton and its assets have been successfully loaded. If an animation is set on track 0,
	   the player won't set its own animation. Default: none */
	success?: (player: SpinePlayer) => void

	/* Optional: Callback when the skeleton could not be loaded or rendered. Default: none */
	error?: (player: SpinePlayer, msg: string) => void

	/* Optional: Callback at the start of each frame, before the skeleton is posed or drawn. Default: none */
	frame?: (player: SpinePlayer, delta: number) => void

	/* Optional: Callback after the skeleton is posed each frame, before it is drawn. Default: none */
	update?: (player: SpinePlayer, delta: number) => void

	/* Optional: Callback after the skeleton is drawn each frame. Default: none */
	draw?: (player: SpinePlayer, delta: number) => void

	/* Optional: Callback each frame before the skeleton is loaded. Default: none */
	loading?: (player: SpinePlayer, delta: number) => void

	/* Optional: The downloader used by the player's asset manager. Passing the same downloader to multiple players using the
	   same assets ensures the assets are only downloaded once. Default: new instance */
	downloader?: Downloader
}

export interface Viewport {
	/* Optional: The position and size of the viewport in the skeleton's world coordinates. Default: the bounding box that fits
	   the current animation */
	x: number,
	y: number,
	width: number,
	height: number,

	/* Optional: Padding around the viewport size, given as a number or percentage (eg "25%"). Default: 10% */
	padLeft: string | number
	padRight: string | number
	padTop: string | number
	padBottom: string | number
}

export class SpinePlayer implements Disposable {
	public parent: HTMLElement;
	public dom: HTMLElement;
	public canvas: HTMLCanvasElement | null = null;
	public context: ManagedWebGLRenderingContext | null = null;
	public sceneRenderer: SceneRenderer | null = null;
	public loadingScreen: LoadingScreen | null = null;
	public assetManager: AssetManager | null = null;
	public bg = new Color();
	public bgFullscreen = new Color();

	private playerControls: HTMLElement | null = null;
	private timelineSlider: Slider | null = null;
	private playButton: HTMLElement | null = null;
	private skinButton: HTMLElement | null = null;
	private animationButton: HTMLElement | null = null;

	private playTime = 0;
	private selectedBones: (Bone | null)[] = [];
	private cancelId = 0;
	popup: Popup | null = null;

	/* True if the player is unable to load or render the skeleton. */
	public error: boolean = false;
	/* The player's skeleton. Null until loading is complete (access after config.success). */
	public skeleton: Skeleton | null = null;
	/* The animation state controlling the skeleton. Null until loading is complete (access after config.success). */
	public animationState: AnimationState | null = null;

	public paused = true;
	public speed = 1;
	public time = new TimeKeeper();
	private stopRequestAnimationFrame = false;
	private disposed = false;

	private viewport: Viewport = {} as Viewport;
	private currentViewport: Viewport = {} as Viewport;
	private previousViewport: Viewport = {} as Viewport;
	private viewportTransitionStart = 0;
	private eventListeners: Array<{ target: any, event: any, func: any }> = [];

	constructor (parent: HTMLElement | string, private config: SpinePlayerConfig) {
		let parentDom = typeof parent === "string" ? document.getElementById(parent) : parent;
		if (parentDom == null) throw new Error("SpinePlayer parent not found: " + parent);
		this.parent = parentDom;

		if (config.showControls === void 0) config.showControls = true;
		let controls = config.showControls ? /*html*/`
<div class="spine-player-controls spine-player-popup-parent spine-player-controls-hidden">
<div class="spine-player-timeline"></div>
<div class="spine-player-buttons">
<button class="spine-player-button spine-player-button-icon-pause"></button>
<div class="spine-player-button-spacer"></div>
<button class="spine-player-button spine-player-button-icon-speed"></button>
<button class="spine-player-button spine-player-button-icon-animations"></button>
<button class="spine-player-button spine-player-button-icon-skins"></button>
<button class="spine-player-button spine-player-button-icon-settings"></button>
<button class="spine-player-button spine-player-button-icon-fullscreen"></button>
<img class="spine-player-button-icon-spine-logo" src="data:image/svg+xml,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20viewBox%3D%220%200%20104%2031.16%22%3E%3Cpath%20d%3D%22M104%2012.68a1.31%201.31%200%200%201-.37%201%201.28%201.28%200%200%201-.85.31H91.57a10.51%2010.51%200%200%200%20.29%202.55%204.92%204.92%200%200%200%201%202%204.27%204.27%200%200%200%201.64%201.26%206.89%206.89%200%200%200%202.6.44%2010.66%2010.66%200%200%200%202.17-.2%2012.81%2012.81%200%200%200%201.64-.44q.69-.25%201.14-.44a1.87%201.87%200%200%201%20.68-.2.44.44%200%200%201%20.27.04.43.43%200%200%201%20.16.2%201.38%201.38%200%200%201%20.09.37%204.89%204.89%200%200%201%200%20.58%204.14%204.14%200%200%201%200%20.43v.32a.83.83%200%200%201-.09.26%201.1%201.1%200%200%201-.17.22%202.77%202.77%200%200%201-.61.34%208.94%208.94%200%200%201-1.32.46%2018.54%2018.54%200%200%201-1.88.41%2013.78%2013.78%200%200%201-2.28.18%2010.55%2010.55%200%200%201-3.68-.59%206.82%206.82%200%200%201-2.66-1.74%207.44%207.44%200%200%201-1.63-2.89%2013.48%2013.48%200%200%201-.55-4%2012.76%2012.76%200%200%201%20.57-3.94%208.35%208.35%200%200%201%201.64-3%207.15%207.15%200%200%201%202.58-1.87%208.47%208.47%200%200%201%203.39-.65%208.19%208.19%200%200%201%203.41.64%206.46%206.46%200%200%201%202.32%201.73%207%207%200%200%201%201.3%202.54%2011.17%2011.17%200%200%201%20.43%203.13zm-3.14-.93a5.69%205.69%200%200%200-1.09-3.86%204.17%204.17%200%200%200-3.42-1.4%204.52%204.52%200%200%200-2%20.44%204.41%204.41%200%200%200-1.47%201.15A5.29%205.29%200%200%200%2092%209.75a7%207%200%200%200-.36%202zM80.68%2021.94a.42.42%200%200%201-.08.26.59.59%200%200%201-.25.18%201.74%201.74%200%200%201-.47.11%206.31%206.31%200%200%201-.76%200%206.5%206.5%200%200%201-.78%200%201.74%201.74%200%200%201-.47-.11.59.59%200%200%201-.25-.18.42.42%200%200%201-.08-.26V12a9.8%209.8%200%200%200-.23-2.35%204.86%204.86%200%200%200-.66-1.53%202.88%202.88%200%200%200-1.13-1%203.57%203.57%200%200%200-1.6-.34%204%204%200%200%200-2.35.83A12.71%2012.71%200%200%200%2069.11%2010v11.9a.42.42%200%200%201-.08.26.59.59%200%200%201-.25.18%201.74%201.74%200%200%201-.47.11%206.51%206.51%200%200%201-.78%200%206.31%206.31%200%200%201-.76%200%201.88%201.88%200%200%201-.48-.11.52.52%200%200%201-.25-.18.46.46%200%200%201-.07-.26v-17a.53.53%200%200%201%20.03-.21.5.5%200%200%201%20.23-.19%201.28%201.28%200%200%201%20.44-.11%208.53%208.53%200%200%201%201.39%200%201.12%201.12%200%200%201%20.43.11.6.6%200%200%201%20.22.19.47.47%200%200%201%20.07.26V7.2a10.46%2010.46%200%200%201%202.87-2.36%206.17%206.17%200%200%201%202.88-.75%206.41%206.41%200%200%201%202.87.58%205.16%205.16%200%200%201%201.88%201.54%206.15%206.15%200%200%201%201%202.26%2013.46%2013.46%200%200%201%20.31%203.11z%22%20fill%3D%22%23fff%22%2F%3E%3Cpath%20d%3D%22M43.35%202.86c.09%202.6%201.89%204%205.48%204.61%203%20.48%205.79.24%206.69-2.37%201.75-5.09-2.4-3.82-6-4.39s-6.31-2.03-6.17%202.15zm1.08%2010.69c.33%201.94%202.14%203.06%204.91%203s4.84-1.16%205.13-3.25c.53-3.88-2.53-2.38-5.3-2.3s-5.4-1.26-4.74%202.55zM48%2022.44c.55%201.45%202.06%202.06%204.1%201.63s3.45-1.11%203.33-2.76c-.21-3.06-2.22-2.1-4.26-1.66S47%2019.6%2048%2022.44zm1.78%206.78c.16%201.22%201.22%202%202.88%201.93s2.92-.67%203.13-2c.4-2.43-1.46-1.53-3.12-1.51s-3.17-.82-2.89%201.58z%22%20fill%3D%22%23ff4000%22%2F%3E%3Cpath%20d%3D%22M35.28%2013.16a15.33%2015.33%200%200%201-.48%204%208.75%208.75%200%200%201-1.42%203%206.35%206.35%200%200%201-2.32%201.91%207.14%207.14%200%200%201-3.16.67%206.1%206.1%200%200%201-1.4-.15%205.34%205.34%200%200%201-1.26-.47%207.29%207.29%200%200%201-1.24-.81q-.61-.49-1.29-1.15v8.51a.47.47%200%200%201-.08.26.56.56%200%200%201-.25.19%201.74%201.74%200%200%201-.47.11%206.47%206.47%200%200%201-.78%200%206.26%206.26%200%200%201-.76%200%201.89%201.89%200%200%201-.48-.11.49.49%200%200%201-.25-.19.51.51%200%200%201-.07-.26V4.91a.57.57%200%200%201%20.06-.27.46.46%200%200%201%20.23-.18%201.47%201.47%200%200%201%20.44-.1%207.41%207.41%200%200%201%201.3%200%201.45%201.45%200%200%201%20.43.1.52.52%200%200%201%20.24.18.51.51%200%200%201%20.07.27V7.2a18.06%2018.06%200%200%201%201.49-1.38%209%209%200%200%201%201.45-1%206.82%206.82%200%200%201%201.49-.59%207.09%207.09%200%200%201%204.78.52%206%206%200%200%201%202.13%202%208.79%208.79%200%200%201%201.2%202.9%2015.72%2015.72%200%200%201%20.4%203.51zm-3.28.36a15.64%2015.64%200%200%200-.2-2.53%207.32%207.32%200%200%200-.69-2.17%204.06%204.06%200%200%200-1.3-1.51%203.49%203.49%200%200%200-2-.57%204.1%204.1%200%200%200-1.2.18%204.92%204.92%200%200%200-1.2.57%208.54%208.54%200%200%200-1.28%201A15.77%2015.77%200%200%200%2022.76%2010v6.77a13.53%2013.53%200%200%200%202.46%202.4%204.12%204.12%200%200%200%202.44.83%203.56%203.56%200%200%200%202-.57A4.28%204.28%200%200%200%2031%2018a7.58%207.58%200%200%200%20.77-2.12%2011.43%2011.43%200%200%200%20.23-2.36zM12%2017.3a5.39%205.39%200%200%201-.48%202.33%204.73%204.73%200%200%201-1.37%201.72%206.19%206.19%200%200%201-2.12%201.06%209.62%209.62%200%200%201-2.71.36%2010.38%2010.38%200%200%201-3.21-.5A7.63%207.63%200%200%201%201%2021.82a3.25%203.25%200%200%201-.66-.43%201.09%201.09%200%200%201-.3-.53%203.59%203.59%200%200%201-.04-.93%204.06%204.06%200%200%201%200-.61%202%202%200%200%201%20.09-.4.42.42%200%200%201%20.16-.22.43.43%200%200%201%20.24-.07%201.35%201.35%200%200%201%20.61.26q.41.26%201%20.56a9.22%209.22%200%200%200%201.41.55%206.25%206.25%200%200%200%201.87.26%205.62%205.62%200%200%200%201.44-.17%203.48%203.48%200%200%200%201.12-.5%202.23%202.23%200%200%200%20.73-.84%202.68%202.68%200%200%200%20.26-1.21%202%202%200%200%200-.37-1.21%203.55%203.55%200%200%200-1-.87%208.09%208.09%200%200%200-1.36-.66l-1.56-.61a16%2016%200%200%201-1.57-.73%206%206%200%200%201-1.37-1%204.52%204.52%200%200%201-1-1.4%204.69%204.69%200%200%201-.37-2%204.88%204.88%200%200%201%20.39-1.87%204.46%204.46%200%200%201%201.16-1.61%205.83%205.83%200%200%201%201.94-1.11A8.06%208.06%200%200%201%206.53%204a8.28%208.28%200%200%201%201.36.11%209.36%209.36%200%200%201%201.23.28%205.92%205.92%200%200%201%20.94.37%204.09%204.09%200%200%201%20.59.35%201%201%200%200%201%20.26.26.83.83%200%200%201%20.09.26%201.32%201.32%200%200%200%20.06.35%203.87%203.87%200%200%201%200%20.51%204.76%204.76%200%200%201%200%20.56%201.39%201.39%200%200%201-.09.39.5.5%200%200%201-.16.22.35.35%200%200%201-.21.07%201%201%200%200%201-.49-.21%207%207%200%200%200-.83-.44%209.26%209.26%200%200%200-1.2-.44%205.49%205.49%200%200%200-1.58-.16%204.93%204.93%200%200%200-1.4.18%202.69%202.69%200%200%200-1%20.51%202.16%202.16%200%200%200-.59.83%202.43%202.43%200%200%200-.2%201%202%202%200%200%200%20.38%201.24%203.6%203.6%200%200%200%201%20.88%208.25%208.25%200%200%200%201.38.68l1.58.62q.8.32%201.59.72a6%206%200%200%201%201.39%201%204.37%204.37%200%200%201%201%201.36%204.46%204.46%200%200%201%20.37%201.8z%22%20fill%3D%22%23fff%22%2F%3E%3C%2Fsvg%3E">
</div></div>` : "";

		this.parent.appendChild(this.dom = createElement(
				/*html*/`<div class="spine-player" style="position:relative;height:100%"><canvas class="spine-player-canvas" style="display:block;width:100%;height:100%"></canvas>${controls}</div>`));

		try {
			this.validateConfig(config);
		} catch (e) {
			this.showError((e as any).message, e as any);
		}

		this.initialize();

		// Register a global resize handler to redraw, avoiding flicker.
		this.addEventListener(window, "resize", () => this.drawFrame(false));

		// Start the rendering loop.
		requestAnimationFrame(() => this.drawFrame());
	}

	dispose (): void {
		this.sceneRenderer?.dispose();
		this.loadingScreen?.dispose();
		this.assetManager?.dispose();
		for (var i = 0; i < this.eventListeners.length; i++) {
			var eventListener = this.eventListeners[i];
			eventListener.target.removeEventListener(eventListener.event, eventListener.func);
		}
		this.parent.removeChild(this.dom);
		this.disposed = true;
	}

	addEventListener (target: any, event: any, func: any) {
		this.eventListeners.push({ target: target, event: event, func: func });
		target.addEventListener(event, func);
	}

	private validateConfig (config: SpinePlayerConfig) {
		if (!config) throw new Error("A configuration object must be passed to to new SpinePlayer().");
		if ((config as any).skelUrl) config.binaryUrl = (config as any).skelUrl;
		if (!config.jsonUrl && !config.binaryUrl) throw new Error("A URL must be specified for the skeleton JSON or binary file.");
		if (!config.atlasUrl) throw new Error("A URL must be specified for the atlas file.");
		if (!config.backgroundColor) config.backgroundColor = config.alpha ? "00000000" : "000000";
		if (!config.fullScreenBackgroundColor) config.fullScreenBackgroundColor = config.backgroundColor;
		if (config.backgroundImage && !config.backgroundImage.url) config.backgroundImage = undefined;
		if (config.premultipliedAlpha === void 0) config.premultipliedAlpha = true;
		if (config.preserveDrawingBuffer === void 0) config.preserveDrawingBuffer = false;
		if (config.mipmaps === void 0) config.mipmaps = true;
		if (!config.debug) config.debug = {
			bones: false,
			clipping: false,
			bounds: false,
			hulls: false,
			meshes: false,
			paths: false,
			points: false,
			regions: false
		};
		if (config.animations && config.animation && config.animations.indexOf(config.animation) < 0)
			throw new Error("Animation '" + config.animation + "' is not in the config animation list: " + toString(config.animations));
		if (config.skins && config.skin && config.skins.indexOf(config.skin) < 0)
			throw new Error("Default skin '" + config.skin + "' is not in the config skins list: " + toString(config.skins));
		if (!config.viewport) config.viewport = {} as any;
		if (!config.viewport!.animations) config.viewport!.animations = {};
		if (config.viewport!.debugRender === void 0) config.viewport!.debugRender = false;
		if (config.viewport!.transitionTime === void 0) config.viewport!.transitionTime = 0.25;
		if (!config.controlBones) config.controlBones = [];
		if (config.showLoading === void 0) config.showLoading = true;
		if (config.defaultMix === void 0) config.defaultMix = 0.25;
	}

	private initialize (): HTMLElement | null {
		let config = this.config;
		let dom = this.dom;

		if (!config.alpha) { // Prevents a flash before the first frame is drawn.
			let hex = config.backgroundColor!;
			this.dom.style.backgroundColor = (hex.charAt(0) == '#' ? hex : "#" + hex).substr(0, 7);
		}

		try {
			// Setup the OpenGL context.
			this.canvas = findWithClass(dom, "spine-player-canvas") as HTMLCanvasElement;
			this.context = new ManagedWebGLRenderingContext(this.canvas, { alpha: config.alpha, preserveDrawingBuffer: config.preserveDrawingBuffer });

			// Setup the scene renderer and loading screen.
			this.sceneRenderer = new SceneRenderer(this.canvas, this.context, true);
			if (config.showLoading) this.loadingScreen = new LoadingScreen(this.sceneRenderer);
		} catch (e) {
			this.showError("Sorry, your browser does not support \nPlease use the latest version of Firefox, Chrome, Edge, or Safari.", e as any);
			return null;
		}

		// Load the assets.
		this.assetManager = new AssetManager(this.context, "", config.downloader);
		if (config.rawDataURIs) {
			for (let path in config.rawDataURIs)
				this.assetManager.setRawDataURI(path, config.rawDataURIs[path]);
		}
		if (config.jsonUrl)
			this.assetManager.loadJson(config.jsonUrl);
		else
			this.assetManager.loadBinary(config.binaryUrl!);
		this.assetManager.loadTextureAtlas(config.atlasUrl!);
		if (config.backgroundImage) this.assetManager.loadTexture(config.backgroundImage.url);

		// Setup the UI elements.
		this.bg.setFromString(config.backgroundColor!);
		this.bgFullscreen.setFromString(config.fullScreenBackgroundColor!);
		if (config.showControls) {
			this.playerControls = dom.children[1] as HTMLElement;
			let controls = this.playerControls.children;
			let timeline = controls[0] as HTMLElement;
			let buttons = controls[1].children;
			this.playButton = buttons[0] as HTMLElement;
			let speedButton = buttons[2] as HTMLElement;
			this.animationButton = buttons[3] as HTMLElement;
			this.skinButton = buttons[4] as HTMLElement;
			let settingsButton = buttons[5] as HTMLElement;
			let fullscreenButton = buttons[6] as HTMLElement;
			let logoButton = buttons[7] as HTMLElement;

			this.timelineSlider = new Slider();
			timeline.appendChild(this.timelineSlider.create());
			this.timelineSlider.change = (percentage) => {
				this.pause();
				let animationDuration = this.animationState!.getCurrent(0)!.animation!.duration;
				let time = animationDuration * percentage;
				this.animationState!.update(time - this.playTime);
				this.animationState!.apply(this.skeleton!);
				this.skeleton!.updateWorldTransform();
				this.playTime = time;
			};

			this.playButton.onclick = () => (this.paused ? this.play() : this.pause());
			speedButton.onclick = () => this.showSpeedDialog(speedButton);
			this.animationButton.onclick = () => this.showAnimationsDialog(this.animationButton!);
			this.skinButton.onclick = () => this.showSkinsDialog(this.skinButton!);
			settingsButton.onclick = () => this.showSettingsDialog(settingsButton);

			let oldWidth = this.canvas.clientWidth, oldHeight = this.canvas.clientHeight;
			let oldStyleWidth = this.canvas.style.width, oldStyleHeight = this.canvas.style.height;
			let isFullscreen = false;
			fullscreenButton.onclick = () => {
				let fullscreenChanged = () => {
					isFullscreen = !isFullscreen;
					if (!isFullscreen) {
						this.canvas!.style.width = oldWidth + "px";
						this.canvas!.style.height = oldHeight + "px";
						this.drawFrame(false);
						// Got to reset the style to whatever the user set after the next layouting.
						requestAnimationFrame(() => {
							this.canvas!.style.width = oldStyleWidth;
							this.canvas!.style.height = oldStyleHeight;
						});
					}
				};

				let player = dom as any;
				player.onfullscreenchange = fullscreenChanged;
				player.onwebkitfullscreenchange = fullscreenChanged;

				let doc = document as any;
				if (doc.fullscreenElement || doc.webkitFullscreenElement || doc.mozFullScreenElement || doc.msFullscreenElement) {
					if (doc.exitFullscreen) doc.exitFullscreen();
					else if (doc.mozCancelFullScreen) doc.mozCancelFullScreen();
					else if (doc.webkitExitFullscreen) doc.webkitExitFullscreen()
					else if (doc.msExitFullscreen) doc.msExitFullscreen();
				} else {
					oldWidth = this.canvas!.clientWidth;
					oldHeight = this.canvas!.clientHeight;
					oldStyleWidth = this.canvas!.style.width;
					oldStyleHeight = this.canvas!.style.height;
					if (player.requestFullscreen) player.requestFullscreen();
					else if (player.webkitRequestFullScreen) player.webkitRequestFullScreen();
					else if (player.mozRequestFullScreen) player.mozRequestFullScreen();
					else if (player.msRequestFullscreen) player.msRequestFullscreen();
				}
			};

			logoButton.onclick = () => window.open("http://esotericsoftware.com");
		}
		return dom;
	}

	private loadSkeleton () {
		if (this.error) return;

		if (this.assetManager!.hasErrors())
			this.showError("Error: Assets could not be loaded.\n" + toString(this.assetManager!.getErrors()));

		let config = this.config;

		// Configure filtering, don't use mipmaps in WebGL1 if the atlas page is non-POT
		let atlas = this.assetManager!.require(config.atlasUrl!) as TextureAtlas;
		let gl = this.context!.gl, anisotropic = gl.getExtension("EXT_texture_filter_anisotropic");
		let isWebGL1 = gl.getParameter(gl.VERSION).indexOf("WebGL 1.0") != -1;
		for (let page of atlas.pages) {
			let minFilter = page.minFilter;
			var useMipMaps: boolean = config.mipmaps!;
			var isPOT = MathUtils.isPowerOfTwo(page.width) && MathUtils.isPowerOfTwo(page.height);
			if (isWebGL1 && !isPOT) useMipMaps = false;

			if (useMipMaps) {
				if (anisotropic) {
					gl.texParameterf(gl.TEXTURE_2D, anisotropic.TEXTURE_MAX_ANISOTROPY_EXT, 8);
					minFilter = TextureFilter.MipMapLinearLinear;
				} else
					minFilter = TextureFilter.Linear; // Don't use mipmaps without anisotropic.
				page.texture!.setFilters(minFilter, TextureFilter.Nearest);
			}
			if (minFilter != TextureFilter.Nearest && minFilter != TextureFilter.Linear) (page.texture as GLTexture).update(true);
		}

		// Load skeleton data.
		let skeletonData: SkeletonData;
		if (config.jsonUrl) {
			try {
				let jsonData = this.assetManager!.remove(config.jsonUrl);
				if (!jsonData) throw new Error("Empty JSON data.");
				if (config.jsonField) {
					jsonData = jsonData[config.jsonField];
					if (!jsonData) throw new Error("JSON field does not exist: " + config.jsonField);
				}
				let json = new SkeletonJson(new AtlasAttachmentLoader(atlas));
				skeletonData = json.readSkeletonData(jsonData);
			} catch (e) {
				this.showError(`Error: Could not load skeleton JSON.\n${(e as any).message}`, e as any);
				return;
			}
		} else {
			let binaryData = this.assetManager!.remove(config.binaryUrl!);
			let binary = new SkeletonBinary(new AtlasAttachmentLoader(atlas));
			try {
				skeletonData = binary.readSkeletonData(binaryData);
			} catch (e) {
				this.showError(`Error: Could not load skeleton binary.\n${(e as any).message}`, e as any);
				return;
			}
		}
		this.skeleton = new Skeleton(skeletonData);
		let stateData = new AnimationStateData(skeletonData);
		stateData.defaultMix = config.defaultMix!;
		this.animationState = new AnimationState(stateData);

		// Check if all control bones are in the skeleton
		config.controlBones!.forEach(bone => {
			if (!skeletonData.findBone(bone)) this.showError(`Error: Control bone does not exist in skeleton: ${bone}`);
		})

		// Setup skin.
		if (!config.skin && skeletonData.skins.length) config.skin = skeletonData.skins[0].name;
		if (config.skins && config.skin!.length) {
			config.skins.forEach(skin => {
				if (!this.skeleton!.data.findSkin(skin))
					this.showError(`Error: Skin in config list does not exist in skeleton: ${skin}`);
			});
		}
		if (config.skin) {
			if (!this.skeleton.data.findSkin(config.skin))
				this.showError(`Error: Skin does not exist in skeleton: ${config.skin}`);
			this.skeleton.setSkinByName(config.skin);
			this.skeleton.setSlotsToSetupPose();
		}

		// Check if all animations given a viewport exist.
		Object.getOwnPropertyNames(config.viewport!.animations).forEach((animation: string) => {
			if (!skeletonData.findAnimation(animation))
				this.showError(`Error: Animation for which a viewport was specified does not exist in skeleton: ${animation}`);
		});

		// Setup the animations after the viewport, so default bounds don't get messed up.
		if (config.animations && config.animations.length) {
			config.animations.forEach(animation => {
				if (!this.skeleton!.data.findAnimation(animation))
					this.showError(`Error: Animation in config list does not exist in skeleton: ${animation}`);
			});
			if (!config.animation) config.animation = config.animations[0];
		}

		if (config.animation && !skeletonData.findAnimation(config.animation))
			this.showError(`Error: Animation does not exist in skeleton: ${config.animation}`);

		// Setup input processing and control bones.
		this.setupInput();

		if (config.showControls) {
			// Hide skin and animation if there's only the default skin / no animation
			if (skeletonData.skins.length == 1 || (config.skins && config.skins.length == 1)) this.skinButton!.classList.add("spine-player-hidden");
			if (skeletonData.animations.length == 1 || (config.animations && config.animations.length == 1)) this.animationButton!.classList.add("spine-player-hidden");
		}

		if (config.success) config.success(this);

		let entry = this.animationState.getCurrent(0);
		if (!entry) {
			if (config.animation) {
				entry = this.setAnimation(config.animation);
				this.play();
			} else {
				entry = this.animationState.setEmptyAnimation(0);
				entry.trackEnd = 100000000;
				this.setViewport(entry.animation!);
				this.pause();
			}
		} else if (!this.currentViewport) {
			this.setViewport(entry.animation!);
			this.play();
		}
	}

	private setupInput () {
		let config = this.config;
		let controlBones = config.controlBones!;
		if (!controlBones.length && !config.showControls) return;
		let selectedBones = this.selectedBones = new Array<Bone | null>(controlBones.length);
		let canvas = this.canvas!;
		let target: Bone | null = null;
		let offset = new Vector2();
		let coords = new Vector3();
		let mouse = new Vector3();
		let position = new Vector2();
		let skeleton = this.skeleton!;
		let renderer = this.sceneRenderer!;

		let closest = function (x: number, y: number): Bone | null {
			mouse.set(x, canvas.clientHeight - y, 0)
			offset.x = offset.y = 0;
			let bestDistance = 24, index = 0;
			let best: Bone | null = null;
			for (let i = 0; i < controlBones.length; i++) {
				selectedBones[i] = null;
				let bone = skeleton.findBone(controlBones[i]);
				if (!bone) continue;
				let distance = renderer.camera.worldToScreen(
					coords.set(bone.worldX, bone.worldY, 0),
					canvas.clientWidth, canvas.clientHeight).distance(mouse);
				if (distance < bestDistance) {
					bestDistance = distance;
					best = bone;
					index = i;
					offset.x = coords.x - mouse.x;
					offset.y = coords.y - mouse.y;
				}
			}
			if (best) selectedBones[index] = best;
			return best;
		};

		new Input(canvas).addListener({
			down: (x, y) => {
				target = closest(x, y);
			},
			up: () => {
				if (target)
					target = null;
				else if (config.showControls)
					(this.paused ? this.play() : this.pause());
			},
			dragged: (x, y) => {
				if (target) {
					x = MathUtils.clamp(x + offset.x, 0, canvas.clientWidth)
					y = MathUtils.clamp(y - offset.y, 0, canvas.clientHeight);
					renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.clientWidth, canvas.clientHeight);
					if (target.parent) {
						target.parent.worldToLocal(position.set(coords.x - skeleton.x, coords.y - skeleton.y));
						target.x = position.x;
						target.y = position.y;
					} else {
						target.x = coords.x - skeleton.x;
						target.y = coords.y - skeleton.y;
					}
				}
			},
			moved: (x, y) => closest(x, y)
		});

		if (config.showControls) {
			// For manual hover to work, we need to disable hidding controls if the mouse/touch entered the clickable area of a child of the controls.
			// For this we need to register a mouse handler on the document and see if we are within the canvas area.
			this.addEventListener(document, "mousemove", (ev: UIEvent) => {
				if (ev instanceof MouseEvent) handleHover(ev.clientX, ev.clientY);
			});
			this.addEventListener(document, "touchmove", (ev: UIEvent) => {
				if (ev instanceof TouchEvent) {
					let touches = ev.changedTouches;
					if (touches.length) {
						let touch = touches[0];
						handleHover(touch.clientX, touch.clientY);
					}

				}
			});

			let overlap = (mouseX: number, mouseY: number, rect: DOMRect | ClientRect): boolean => {
				let x = mouseX - rect.left, y = mouseY - rect.top;
				return x >= 0 && x <= rect.width && y >= 0 && y <= rect.height;
			}

			let mouseOverControls = true, mouseOverCanvas = false;
			let handleHover = (mouseX: number, mouseY: number) => {
				let popup = findWithClass(this.dom, "spine-player-popup");
				mouseOverControls = overlap(mouseX, mouseY, this.playerControls!.getBoundingClientRect());
				mouseOverCanvas = overlap(mouseX, mouseY, canvas.getBoundingClientRect());
				clearTimeout(this.cancelId);
				let hide = !popup && !mouseOverControls && !mouseOverCanvas && !this.paused;
				if (hide)
					this.playerControls!.classList.add("spine-player-controls-hidden");
				else
					this.playerControls!.classList.remove("spine-player-controls-hidden");
				if (!mouseOverControls && !popup && !this.paused) {
					this.cancelId = setTimeout(() => {
						if (!this.paused) this.playerControls!.classList.add("spine-player-controls-hidden");
					}, 1000);
				}
			}
		}
	}

	play () {
		this.paused = false;
		let config = this.config;
		if (config.showControls) {
			this.cancelId = setTimeout(() => {
				if (!this.paused) this.playerControls!.classList.add("spine-player-controls-hidden");
			}, 1000);
			this.playButton!.classList.remove("spine-player-button-icon-play");
			this.playButton!.classList.add("spine-player-button-icon-pause");

			// If no config animation, set one when first clicked.
			if (!config.animation) {
				if (config.animations && config.animations.length)
					config.animation = config.animations[0];
				else if (this.skeleton!.data.animations.length)
					config.animation = this.skeleton!.data.animations[0].name;
				if (config.animation) this.setAnimation(config.animation);
			}
		}
	}

	pause () {
		this.paused = true;
		if (this.config.showControls) {
			this.playerControls!.classList.remove("spine-player-controls-hidden");
			clearTimeout(this.cancelId);
			this.playButton!.classList.remove("spine-player-button-icon-pause");
			this.playButton!.classList.add("spine-player-button-icon-play");
		}
	}

	/* Sets a new animation and viewport on track 0. */
	setAnimation (animation: string | Animation, loop: boolean = true): TrackEntry {
		animation = this.setViewport(animation);
		return this.animationState!.setAnimationWith(0, animation, loop);
	}

	/* Adds a new animation and viewport on track 0. */
	addAnimation (animation: string | Animation, loop: boolean = true, delay: number = 0): TrackEntry {
		animation = this.setViewport(animation);
		return this.animationState!.addAnimationWith(0, animation, loop, delay);
	}

	/* Sets the viewport for the specified animation. */
	setViewport (animation: string | Animation): Animation {
		if (typeof animation == "string") {
			let foundAnimation = this.skeleton!.data.findAnimation(animation);
			if (!foundAnimation) throw new Error("Animation not found: " + animation);
			animation = foundAnimation;
		}

		this.previousViewport = this.currentViewport;

		// Determine the base viewport.
		let globalViewport = this.config.viewport!;
		let viewport = this.currentViewport = {
			padLeft: globalViewport.padLeft !== void 0 ? globalViewport.padLeft : "10%",
			padRight: globalViewport.padRight !== void 0 ? globalViewport.padRight : "10%",
			padTop: globalViewport.padTop !== void 0 ? globalViewport.padTop : "10%",
			padBottom: globalViewport.padBottom !== void 0 ? globalViewport.padBottom : "10%"
		} as Viewport;
		if (globalViewport.x !== void 0 && globalViewport.y !== void 0 && globalViewport.width && globalViewport.height) {
			viewport.x = globalViewport.x;
			viewport.y = globalViewport.y;
			viewport.width = globalViewport.width;
			viewport.height = globalViewport.height;
		} else
			this.calculateAnimationViewport(animation, viewport);

		// Override with the animation specific viewport for the final result.
		let userAnimViewport = this.config.viewport!.animations![animation.name];
		if (userAnimViewport) {
			if (userAnimViewport.x !== void 0 && userAnimViewport.y !== void 0 && userAnimViewport.width && userAnimViewport.height) {
				viewport.x = userAnimViewport.x;
				viewport.y = userAnimViewport.y;
				viewport.width = userAnimViewport.width;
				viewport.height = userAnimViewport.height;
			}
			if (userAnimViewport.padLeft !== void 0) viewport.padLeft = userAnimViewport.padLeft;
			if (userAnimViewport.padRight !== void 0) viewport.padRight = userAnimViewport.padRight;
			if (userAnimViewport.padTop !== void 0) viewport.padTop = userAnimViewport.padTop;
			if (userAnimViewport.padBottom !== void 0) viewport.padBottom = userAnimViewport.padBottom;
		}

		// Translate percentage padding to world units.
		viewport.padLeft = this.percentageToWorldUnit(viewport.width, viewport.padLeft);
		viewport.padRight = this.percentageToWorldUnit(viewport.width, viewport.padRight);
		viewport.padBottom = this.percentageToWorldUnit(viewport.height, viewport.padBottom);
		viewport.padTop = this.percentageToWorldUnit(viewport.height, viewport.padTop);

		this.viewportTransitionStart = performance.now();
		return animation;
	}

	private percentageToWorldUnit (size: number, percentageOrAbsolute: string | number): number {
		if (typeof percentageOrAbsolute === "string")
			return size * parseFloat(percentageOrAbsolute.substr(0, percentageOrAbsolute.length - 1)) / 100;
		return percentageOrAbsolute;
	}

	private calculateAnimationViewport (animation: Animation, viewport: Viewport) {
		this.skeleton!.setToSetupPose();

		let steps = 100, stepTime = animation.duration ? animation.duration / steps : 0, time = 0;
		let minX = 100000000, maxX = -100000000, minY = 100000000, maxY = -100000000;
		let offset = new Vector2(), size = new Vector2();

		for (let i = 0; i < steps; i++, time += stepTime) {
			animation.apply(this.skeleton!, time, time, false, [], 1, MixBlend.setup, MixDirection.mixIn);
			this.skeleton!.updateWorldTransform();
			this.skeleton!.getBounds(offset, size);

			if (!isNaN(offset.x) && !isNaN(offset.y) && !isNaN(size.x) && !isNaN(size.y)) {
				minX = Math.min(offset.x, minX);
				maxX = Math.max(offset.x + size.x, maxX);
				minY = Math.min(offset.y, minY);
				maxY = Math.max(offset.y + size.y, maxY);
			} else
				this.showError("Animation bounds are invalid: " + animation.name);
		}

		viewport.x = minX;
		viewport.y = minY;
		viewport.width = maxX - minX;
		viewport.height = maxY - minY;
	}

	private drawFrame (requestNextFrame = true) {
		try {
			if (this.error) return;
			if (this.disposed) return;
			if (requestNextFrame && !this.stopRequestAnimationFrame) requestAnimationFrame(() => this.drawFrame());

			let doc = document as any;
			let isFullscreen = doc.fullscreenElement || doc.webkitFullscreenElement || doc.mozFullScreenElement || doc.msFullscreenElement;
			let bg = isFullscreen ? this.bgFullscreen : this.bg;

			this.time.update();
			let delta = this.time.delta;

			// Load the skeleton if the assets are ready.
			let loading = this.assetManager!.isLoadingComplete();
			if (!this.skeleton && loading) this.loadSkeleton();
			let skeleton = this.skeleton!;
			let config = this.config!;
			if (skeleton) {
				// Resize the canvas.
				let renderer = this.sceneRenderer!;
				renderer.resize(ResizeMode.Expand);

				let playDelta = this.paused ? 0 : delta * this.speed;
				if (config.frame) config.frame(this, playDelta);

				// Update animation time and pose the skeleton.
				if (!this.paused) {
					this.animationState!.update(playDelta);
					this.animationState!.apply(skeleton);
					skeleton.updateWorldTransform();

					if (config.showControls) {
						this.playTime += playDelta;
						let entry = this.animationState!.getCurrent(0);
						if (entry) {
							let duration = entry.animation!.duration;
							while (this.playTime >= duration && duration != 0)
								this.playTime -= duration;
							this.playTime = Math.max(0, Math.min(this.playTime, duration));
							this.timelineSlider!.setValue(this.playTime / duration);
						}
					}
				}

				// Determine the viewport.
				let viewport = this.viewport;
				viewport.x = this.currentViewport.x - (this.currentViewport.padLeft as number),
					viewport.y = this.currentViewport.y - (this.currentViewport.padBottom as number),
					viewport.width = this.currentViewport.width + (this.currentViewport.padLeft as number) + (this.currentViewport.padRight as number),
					viewport.height = this.currentViewport.height + (this.currentViewport.padBottom as number) + (this.currentViewport.padTop as number)

				if (this.previousViewport) {
					let transitionAlpha = (performance.now() - this.viewportTransitionStart) / 1000 / config.viewport!.transitionTime!;
					if (transitionAlpha < 1) {
						let x = this.previousViewport.x - (this.previousViewport.padLeft as number);
						let y = this.previousViewport.y - (this.previousViewport.padBottom as number);
						let width = this.previousViewport.width + (this.previousViewport.padLeft as number) + (this.previousViewport.padRight as number);
						let height = this.previousViewport.height + (this.previousViewport.padBottom as number) + (this.previousViewport.padTop as number);
						viewport.x = x + (viewport.x - x) * transitionAlpha;
						viewport.y = y + (viewport.y - y) * transitionAlpha;
						viewport.width = width + (viewport.width - width) * transitionAlpha;
						viewport.height = height + (viewport.height - height) * transitionAlpha;
					}
				}

				renderer.camera.zoom = this.canvas!.height / this.canvas!.width > viewport.height / viewport.width
					? viewport.width / this.canvas!.width : viewport.height / this.canvas!.height;
				renderer.camera.position.x = viewport.x + viewport.width / 2;
				renderer.camera.position.y = viewport.y + viewport.height / 2;

				// Clear the screen.
				let gl = this.context!.gl;
				gl.clearColor(bg.r, bg.g, bg.b, bg.a);
				gl.clear(gl.COLOR_BUFFER_BIT);

				if (config.update) config.update(this, playDelta);

				renderer.begin();

				// Draw the background image.
				let bgImage = config.backgroundImage;
				if (bgImage) {
					let texture = this.assetManager!.require(bgImage.url);
					if (bgImage.x !== void 0 && bgImage.y !== void 0 && bgImage.width && bgImage.height)
						renderer.drawTexture(texture, bgImage.x, bgImage.y, bgImage.width, bgImage.height);
					else
						renderer.drawTexture(texture, viewport.x, viewport.y, viewport.width, viewport.height);
				}

				// Draw the skeleton and debug output.
				renderer.drawSkeleton(skeleton, config.premultipliedAlpha);
				if ((renderer.skeletonDebugRenderer.drawBones = config.debug!.bones!)
					|| (renderer.skeletonDebugRenderer.drawBoundingBoxes = config.debug!.bounds!)
					|| (renderer.skeletonDebugRenderer.drawClipping = config.debug!.clipping!)
					|| (renderer.skeletonDebugRenderer.drawMeshHull = config.debug!.hulls!)
					|| (renderer.skeletonDebugRenderer.drawPaths = config.debug!.paths!)
					|| (renderer.skeletonDebugRenderer.drawRegionAttachments = config.debug!.regions!)
					|| (renderer.skeletonDebugRenderer.drawMeshTriangles = config.debug!.meshes!)
				) {
					renderer.drawSkeletonDebug(skeleton, config.premultipliedAlpha);
				}

				// Draw the control bones.
				let controlBones = config.controlBones!;
				if (controlBones.length) {
					let selectedBones = this.selectedBones;
					gl.lineWidth(2);
					for (let i = 0; i < controlBones.length; i++) {
						let bone = skeleton.findBone(controlBones[i]);
						if (!bone) continue;
						let colorInner = selectedBones[i] ? BONE_INNER_OVER : BONE_INNER;
						let colorOuter = selectedBones[i] ? BONE_OUTER_OVER : BONE_OUTER;
						renderer.circle(true, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorInner);
						renderer.circle(false, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorOuter);
					}
				}

				// Draw the viewport bounds.
				if (config.viewport!.debugRender) {
					gl.lineWidth(1);
					renderer.rect(false, this.currentViewport.x, this.currentViewport.y, this.currentViewport.width, this.currentViewport.height, Color.GREEN);
					renderer.rect(false, viewport.x, viewport.y, viewport.width, viewport.height, Color.RED);
				}

				renderer.end();

				if (config.draw) config.draw(this, playDelta);
			}

			// Draw the loading screen.
			if (config.showLoading) {
				this.loadingScreen!.backgroundColor.setFromColor(bg);
				this.loadingScreen!.draw(loading);
			}
			if (loading && config.loading) config.loading(this, delta);
		} catch (e) {
			this.showError(`Error: Unable to render skeleton.\n${(e as any).message}`, e as any);
		}
	}

	stopRendering () {
		this.stopRequestAnimationFrame = true;
	}

	private hidePopup (id: string): boolean {
		return this.popup != null && this.popup.hide(id);
	}

	private showSpeedDialog (speedButton: HTMLElement) {
		let id = "speed";
		if (this.hidePopup(id)) return;

		let popup = new Popup(id, speedButton, this, this.playerControls!, /*html*/`
<div class="spine-player-popup-title">Speed</div>
<hr>
<div class="spine-player-row" style="align-items:center;padding:8px">
<div class="spine-player-column">
	<div class="spine-player-speed-slider" style="margin-bottom:4px"></div>
	<div class="spine-player-row" style="justify-content:space-between"><div>0.1x</div><div>1x</div><div>2x</div></div>
</div>
</div>`);
		let slider = new Slider(2, 0.1, true);
		findWithClass(popup.dom, "spine-player-speed-slider").appendChild(slider.create());
		slider.setValue(this.speed / 2);
		slider.change = (percentage) => this.speed = percentage * 2;
		popup.show();
	}

	private showAnimationsDialog (animationsButton: HTMLElement) {
		let id = "animations";
		if (this.hidePopup(id)) return;
		if (!this.skeleton || !this.skeleton.data.animations.length) return;

		let popup = new Popup(id, animationsButton, this, this.playerControls!,
				/*html*/`<div class="spine-player-popup-title">Animations</div><hr><ul class="spine-player-list"></ul>`);

		let rows = findWithClass(popup.dom, "spine-player-list");
		this.skeleton.data.animations.forEach((animation) => {
			// Skip animations not whitelisted if a whitelist was given.
			if (this.config.animations && this.config.animations.indexOf(animation.name) < 0) return;

			let row = createElement(
					/*html*/`<li class="spine-player-list-item selectable"><div class="selectable-circle"></div><div class="selectable-text"></div></li>`);
			if (animation.name == this.config.animation) row.classList.add("selected");
			findWithClass(row, "selectable-text").innerText = animation.name;
			rows.appendChild(row);
			row.onclick = () => {
				removeClass(rows.children, "selected");
				row.classList.add("selected");
				this.config.animation = animation.name;
				this.playTime = 0;
				this.setAnimation(animation.name);
				this.play();
			}
		});
		popup.show();
	}

	private showSkinsDialog (skinButton: HTMLElement) {
		let id = "skins";
		if (this.hidePopup(id)) return;
		if (!this.skeleton || !this.skeleton.data.animations.length) return;

		let popup = new Popup(id, skinButton, this, this.playerControls!,
				/*html*/`<div class="spine-player-popup-title">Skins</div><hr><ul class="spine-player-list"></ul>`);

		let rows = findWithClass(popup.dom, "spine-player-list");
		this.skeleton.data.skins.forEach((skin) => {
			// Skip skins not whitelisted if a whitelist was given.
			if (this.config.skins && this.config.skins.indexOf(skin.name) < 0) return;

			let row = createElement(/*html*/`<li class="spine-player-list-item selectable"><div class="selectable-circle"></div><div class="selectable-text"></div></li>`);
			if (skin.name == this.config.skin) row.classList.add("selected");
			findWithClass(row, "selectable-text").innerText = skin.name;
			rows.appendChild(row);
			row.onclick = () => {
				removeClass(rows.children, "selected");
				row.classList.add("selected");
				this.config.skin = skin.name;
				this.skeleton!.setSkinByName(this.config.skin);
				this.skeleton!.setSlotsToSetupPose();
			}
		});
		popup.show();
	}

	private showSettingsDialog (settingsButton: HTMLElement) {
		let id = "settings";
		if (this.hidePopup(id)) return;
		if (!this.skeleton || !this.skeleton.data.animations.length) return;

		let popup = new Popup(id, settingsButton, this, this.playerControls!, /*html*/`<div class="spine-player-popup-title">Debug</div><hr><ul class="spine-player-list"></li>`);

		let rows = findWithClass(popup.dom, "spine-player-list");
		let makeItem = (label: string, name: string) => {
			let row = createElement(/*html*/`<li class="spine-player-list-item"></li>`);
			let s = new Switch(label);
			row.appendChild(s.create());
			let debug = this.config.debug as any;
			s.setEnabled(debug[name]);
			s.change = (value) => debug[name] = value;
			rows.appendChild(row);
		};
		makeItem("Bones", "bones");
		makeItem("Regions", "regions");
		makeItem("Meshes", "meshes");
		makeItem("Bounds", "bounds");
		makeItem("Paths", "paths");
		makeItem("Clipping", "clipping");
		makeItem("Points", "points");
		makeItem("Hulls", "hulls");
		popup.show();
	}

	private showError (message: string, error?: Error) {
		if (this.error) {
			if (error) throw error; // Don't lose error if showError throws, is caught, and showError is called again.
		} else {
			this.error = true;
			this.dom.appendChild(createElement(
					/*html*/`<div class="spine-player-error" style="background:#000;color:#fff;position:absolute;top:0;width:100%;height:100%;display:flex;justify-content:center;align-items:center;overflow:auto;z-index:999">`
				+ message.replace("\n", "<br><br>") + `</div>`));
			if (this.config.error) this.config.error(this, message);
			throw (error ? error : new Error(message));
		}
	}
}

class Popup {
	public dom: HTMLElement;
	private className: string;
	private windowClickListener: any;

	constructor (private id: string, private button: HTMLElement, private player: SpinePlayer, parent: HTMLElement, htmlContent: string) {
		this.dom = createElement(/*html*/`<div class="spine-player-popup spine-player-hidden"></div>`);
		this.dom.innerHTML = htmlContent;
		parent.appendChild(this.dom);
		this.className = "spine-player-button-icon-" + id + "-selected";
	}

	dispose () {

	}

	hide (id: string): boolean {
		this.dom.remove();
		this.button.classList.remove(this.className);
		if (this.id == id) {
			this.player.popup = null;
			return true;
		}
		return false;
	}

	show () {
		this.player.popup = this;
		this.button.classList.add(this.className);
		this.dom.classList.remove("spine-player-hidden");

		// Make sure the popup isn't bigger than the player.
		let dismissed = false;
		let resize = () => {
			if (!dismissed) requestAnimationFrame(resize);
			let playerDom = this.player.dom;
			let bottomOffset = Math.abs(playerDom.getBoundingClientRect().bottom - playerDom.getBoundingClientRect().bottom);
			let rightOffset = Math.abs(playerDom.getBoundingClientRect().right - playerDom.getBoundingClientRect().right);
			this.dom.style.maxHeight = (playerDom.clientHeight - bottomOffset - rightOffset) + "px";
		}
		requestAnimationFrame(resize);

		// Dismiss when clicking somewhere outside the popup.
		let justClicked = true;
		let windowClickListener = (event: any) => {
			if (justClicked || this.player.popup != this) {
				justClicked = false;
				return;
			}
			if (!this.dom.contains(event.target)) {
				this.dom.remove();
				window.removeEventListener("click", windowClickListener);
				this.button.classList.remove(this.className);
				this.player.popup = null;
				dismissed = true;
			}
		};
		this.player.addEventListener(window, "click", windowClickListener);
	}
}

class Switch {
	private switch: HTMLElement | null = null;
	private enabled = false;
	public change: (value: boolean) => void = () => { };

	constructor (private text: string) { }


	create (): HTMLElement {
		this.switch = createElement(/*html*/`
<div class="spine-player-switch">
	<span class="spine-player-switch-text">${this.text}</span>
	<div class="spine-player-switch-knob-area">
		<div class="spine-player-switch-knob"></div>
	</div>
</div>`);
		this.switch.addEventListener("click", () => {
			this.setEnabled(!this.enabled);
			if (this.change) this.change(this.enabled);
		})
		return this.switch;
	}

	setEnabled (enabled: boolean) {
		if (enabled) this.switch?.classList.add("active");
		else this.switch?.classList.remove("active");
		this.enabled = enabled;
	}

	isEnabled (): boolean {
		return this.enabled;
	}
}

class Slider {
	private slider: HTMLElement | null = null;
	private value: HTMLElement | null = null;
	private knob: HTMLElement | null = null;
	public change: (percentage: number) => void = () => { };

	constructor (public snaps = 0, public snapPercentage = 0.1, public big = false) { }

	create (): HTMLElement {
		this.slider = createElement(/*html*/`
<div class="spine-player-slider ${this.big ? "big" : ""}">
	<div class="spine-player-slider-value"></div>
	<!--<div class="spine-player-slider-knob"></div>-->
</div>`);
		this.value = findWithClass(this.slider, "spine-player-slider-value");
		// this.knob = findWithClass(this.slider, "spine-player-slider-knob");
		this.setValue(0);

		let dragging = false;
		new Input(this.slider).addListener({
			down: (x, y) => {
				dragging = true;
				this.value?.classList.add("hovering");
			},
			up: (x, y) => {
				dragging = false;
				if (this.change) this.change(this.setValue(x / this.slider!.clientWidth));
				this.value?.classList.remove("hovering");
			},
			moved: (x, y) => {
				if (dragging && this.change) this.change(this.setValue(x / this.slider!.clientWidth));
			},
			dragged: (x, y) => {
				if (this.change) this.change(this.setValue(x / this.slider!.clientWidth));
			}
		});

		return this.slider;
	}

	setValue (percentage: number): number {
		percentage = Math.max(0, Math.min(1, percentage));
		if (this.snaps) {
			let snap = 1 / this.snaps;
			let modulo = percentage % snap;
			// floor
			if (modulo < snap * this.snapPercentage)
				percentage = percentage - modulo;
			else if (modulo > snap - snap * this.snapPercentage)
				percentage = percentage - modulo + snap;
			percentage = Math.max(0, Math.min(1, percentage));
		}
		this.value!.style.width = "" + (percentage * 100) + "%";
		// this.knob.style.left = "" + (-8 + percentage * this.slider.clientWidth) + "px";
		return percentage;
	}
}

function findWithClass (element: HTMLElement, className: string): HTMLElement {
	return element.getElementsByClassName(className)[0] as HTMLElement;
}

function createElement (html: string): HTMLElement {
	let div = document.createElement("div");
	div.innerHTML = html;
	return div.children[0] as HTMLElement;
}

function removeClass (elements: HTMLCollection, clazz: string) {
	for (let i = 0; i < elements.length; i++)
		elements[i].classList.remove(clazz);
}

function toString (object: any) {
	return JSON.stringify(object)
		.replace(/&/g, "&amp;")
		.replace(/</g, "&lt;")
		.replace(/>/g, "&gt;")
		.replace(/"/g, "&#34;")
		.replace(/'/g, "&#39;");
}

const BONE_INNER_OVER = new Color(0.478, 0, 0, 0.25);
const BONE_OUTER_OVER = new Color(1, 1, 1, 1);
const BONE_INNER = new Color(0.478, 0, 0, 0.5);
const BONE_OUTER = new Color(1, 0, 0, 0.8);
