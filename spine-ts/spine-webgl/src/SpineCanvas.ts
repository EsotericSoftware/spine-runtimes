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

import { TimeKeeper, AssetManager, ManagedWebGLRenderingContext, SceneRenderer, Input, StringMap } from "./";

/** An app running inside a {@link SpineCanvas}. The app life-cycle
 * is as follows:
 *
 * 1. `loadAssets()` is called. The app can queue assets for loading via {@link SpineCanvas#assetManager}.
 * 2. `initialize()` is called when all assets are loaded. The app can setup anything it needs to enter the main application logic.
 * 3. `update()` is called periodically at screen refresh rate. The app can update its state.
 * 4. `render()` is called periodically at screen refresh rate. The app can render its state via {@link SpineCanvas#renderer} or directly via the WebGL context in {@link SpineCanvas.gl}`
 *
 * The `error()` method is called in case the assets could not be loaded.
 */
export interface SpineCanvasApp {
	loadAssets?(canvas: SpineCanvas): void;
	initialize?(canvas: SpineCanvas): void;
	update?(canvas: SpineCanvas, delta: number): void;
	render?(canvas: SpineCanvas): void;
	error?(canvas: SpineCanvas, errors: StringMap<string>): void;
}

/** Configuration passed to the {@link SpineCanvas} constructor */
export interface SpineCanvasConfig {
	/* The {@link SpineCanvasApp} to be run in the canvas. */
	app: SpineCanvasApp;
	/* The path prefix to be used by the {@link AssetManager}. */
	pathPrefix?: string;
	/* The WebGL context configuration */
	webglConfig?: any;
}

/** Manages the life-cycle and WebGL context of a {@link SpineCanvasApp}. The app loads
 * assets and initializes itself, then updates and renders its state at the screen refresh rate. */
export class SpineCanvas {
	readonly context: ManagedWebGLRenderingContext;

	/** Tracks the current time, delta, and other time related statistics. */
	readonly time = new TimeKeeper();
	/** The HTML canvas to render to. */
	readonly htmlCanvas: HTMLCanvasElement;
	/** The WebGL rendering context. */
	readonly gl: WebGLRenderingContext;
	/** The scene renderer for easy drawing of skeletons, shapes, and images. */
	readonly renderer: SceneRenderer;
	/** The asset manager to load assets with. */
	readonly assetManager: AssetManager;
	/** The input processor used to listen to mouse, touch, and keyboard events. */
	readonly input: Input;

	/** Constructs a new spine canvas, rendering to the provided HTML canvas. */
	constructor (canvas: HTMLCanvasElement, config: SpineCanvasConfig) {
		if (!config.pathPrefix) config.pathPrefix = "";
		if (!config.app) config.app = {
			loadAssets: () => { },
			initialize: () => { },
			update: () => { },
			render: () => { },
			error: () => { },
		}
		if (config.webglConfig) config.webglConfig = { alpha: true };

		this.htmlCanvas = canvas;
		this.context = new ManagedWebGLRenderingContext(canvas, config.webglConfig);
		this.renderer = new SceneRenderer(canvas, this.context);
		this.gl = this.context.gl;
		this.assetManager = new AssetManager(this.context, config.pathPrefix);
		this.input = new Input(canvas);

		if (config.app.loadAssets) config.app.loadAssets(this);

		let loop = () => {
			requestAnimationFrame(loop);
			this.time.update();
			if (config.app.update) config.app.update(this, this.time.delta);
			if (config.app.render) config.app.render(this);
		}

		let waitForAssets = () => {
			if (this.assetManager.isLoadingComplete()) {
				if (this.assetManager.hasErrors()) {
					if (config.app.error) config.app.error(this, this.assetManager.getErrors());
				} else {
					if (config.app.initialize) config.app.initialize(this);
					loop();
				}
				return;
			}
			requestAnimationFrame(waitForAssets);
		}
		requestAnimationFrame(waitForAssets);
	}

	/** Clears the canvas with the given color. The color values are given in the range [0,1]. */
	clear (r: number, g: number, b: number, a: number) {
		this.gl.clearColor(r, g, b, a);
		this.gl.clear(this.gl.COLOR_BUFFER_BIT);
	}
}
