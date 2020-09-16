/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

module spine {
	export interface Viewport {
		x: number,
		y: number,
		width: number,
		height: number,
		padLeft: string | number
		padRight: string | number
		padTop: string | number
		padBottom: string | number
	}

	export interface SpinePlayerConfig {
		/* the URL of the skeleton .json file */
		jsonUrl: string

		/* the URL of the skeleton .skel file */
		skelUrl: string

		/* the URL of the skeleton .atlas file. Atlas page images are automatically resolved. */
		atlasUrl: string

		/* Raw data URIs, mapping from a path to base 64 encoded raw data. When the player
		   resolves a path of the `jsonUrl`, `skelUrl`, `atlasUrl`, or the image paths
		   referenced in the atlas, it will first look for that path in this array of
		   raw data URIs. This allows embedding of resources directly in HTML/JS. */
		rawDataURIs: Map<string>

		/* Optional: the name of the animation to be played. Default: first animation in the skeleton. */
		animation: string

		/* Optional: list of animation names from which the user can choose. */
		animations: string[]

		/* Optional: the default mix time used to switch between two animations. */
		defaultMix: number

		/* Optional: the name of the skin to be set. Default: the default skin. */
		skin: string

		/* Optional: list of skin names from which the user can choose. */
		skins: string[]

		/* Optional: whether the skeleton uses premultiplied alpha. Default: true. */
		premultipliedAlpha: boolean

		/* Optional: whether to show the player controls. Default: true. */
		showControls: boolean

		/* Optional: which debugging visualizations should be one. Default: none. */
		debug: {
			bones: boolean
			regions: boolean
			meshes: boolean
			bounds: boolean
			paths: boolean
			clipping: boolean
			points: boolean
			hulls: boolean;
		},

		/* Optional: the position and size of the viewport in world coordinates of the skeleton. Default: the setup pose bounding box. */
		viewport: {
			x: number
			y: number
			width: number
			height: number
			padLeft: string | number
			padRight: string | number
			padTop: string | number
			padBottom: string | number
			animations: Map<Viewport>
			debugRender: boolean,
			transitionTime: number
		}

		/* Optional: whether the canvas should be transparent. Default: false. */
		alpha: boolean

		/* Optional: the background color. Must be given in the format #rrggbbaa. Default: #000000ff. */
		backgroundColor: string

		/* Optional: the background image. Default: none. */
		backgroundImage: {
			/* The URL of the background image */
			url: string

			/* Optional: the position and size of the background image in world coordinates. Default: viewport. */
			x: number
			y: number
			width: number
			height: number
		}

		/* Optional: the background color used in fullscreen mode. Must be given in the format #rrggbbaa. Default: backgroundColor. */
		fullScreenBackgroundColor: string

		/* Optional: list of bone names that the user can control by dragging. */
		controlBones: string[]

		/* Optional: callback when the widget and its assets have been successfully loaded. */
		success: (widget: SpinePlayer) => void

		/* Optional: callback when the widget could not be loaded. */
		error: (widget: SpinePlayer, msg: string) => void
	}

	class Popup {
		public dom: HTMLElement;

		constructor(private player: HTMLElement, parent: HTMLElement, htmlContent: string) {
			this.dom = createElement(/*html*/`
				<div class="spine-player-popup spine-player-hidden">
				</div>
			`);
			this.dom.innerHTML = htmlContent;
			parent.appendChild(this.dom);
		}

		show (dismissedListener: () => void) {
			this.dom.classList.remove("spine-player-hidden");

			// Make sure the popup isn't bigger than the player.
			var dismissed = false;
			let resize = () => {
				if (!dismissed) requestAnimationFrame(resize);
				let bottomOffset = Math.abs(this.dom.getBoundingClientRect().bottom - this.player.getBoundingClientRect().bottom);
				let rightOffset = Math.abs(this.dom.getBoundingClientRect().right - this.player.getBoundingClientRect().right);
				let maxHeight = this.player.clientHeight - bottomOffset - rightOffset;
				this.dom.style.maxHeight = maxHeight + "px";
			}
			requestAnimationFrame(resize);

			// Dismiss when clicking somewhere else outside
			// the popup
			var justClicked = true;
			let windowClickListener = (event: any) => {
				if (justClicked) {
					justClicked = false;
					return;
				}
				if (!isContained(this.dom, event.target)) {
					this.dom.remove();
					window.removeEventListener("click", windowClickListener);
					dismissedListener();
					dismissed = true;
				}
			}
			window.addEventListener("click", windowClickListener);
		}
	}

	class Switch {
		private switch: HTMLElement;
		private enabled = false;
		public change: (value: boolean) => void;

		constructor(private text: string) {}

		render(): HTMLElement {
			this.switch = createElement(/*html*/`
				<div class="spine-player-switch">
					<span class="spine-player-switch-text">${this.text}</span>
					<div class="spine-player-switch-knob-area">
						<div class="spine-player-switch-knob"></div>
					</div>
				</div>
			`);
			this.switch.addEventListener("click", () => {
				this.setEnabled(!this.enabled);
				if (this.change) this.change(this.enabled);
			})
			return this.switch;
		}

		setEnabled(enabled: boolean) {
			if (enabled) this.switch.classList.add("active");
			else this.switch.classList.remove("active");
			this.enabled = enabled;
		}

		isEnabled(): boolean {
			return this.enabled;
		}
	}

	class Slider {
		private slider: HTMLElement;
		private value: HTMLElement;
		private knob: HTMLElement;
		public change: (percentage: number) => void;

		constructor(public snaps = 0, public snapPercentage = 0.1, public big = false) { }

		render(): HTMLElement {
			this.slider = createElement(/*html*/`
				<div class="spine-player-slider ${this.big ? "big": ""}">
					<div class="spine-player-slider-value"></div>
					<!--<div class="spine-player-slider-knob"></div>-->
				</div>
			`);
			this.value = findWithClass(this.slider, "spine-player-slider-value")[0];
			// this.knob = findWithClass(this.slider, "spine-player-slider-knob")[0];
			this.setValue(0);

			let input = new spine.webgl.Input(this.slider);
			var dragging = false;
			input.addListener({
				down: (x, y) => {
					dragging = true;
					this.value.classList.add("hovering");
				},
				up: (x, y) => {
					dragging = false;
					let percentage = x / this.slider.clientWidth;
					percentage = percentage = Math.max(0, Math.min(percentage, 1));
					this.setValue(x / this.slider.clientWidth);
					if (this.change) this.change(percentage);
					this.value.classList.remove("hovering");
				},
				moved: (x, y) => {
					if (dragging) {
						let percentage = x / this.slider.clientWidth;
						percentage = Math.max(0, Math.min(percentage, 1));
						percentage = this.setValue(x / this.slider.clientWidth);
						if (this.change) this.change(percentage);
					}
				},
				dragged: (x, y) => {
					let percentage = x / this.slider.clientWidth;
					percentage = Math.max(0, Math.min(percentage, 1));
					percentage = this.setValue(x / this.slider.clientWidth);
					if (this.change) this.change(percentage);
				}
			});


			return this.slider;
		}

		setValue(percentage: number): number {
			percentage = Math.max(0, Math.min(1, percentage));
			if (this.snaps > 0) {
				let modulo = percentage % (1 / this.snaps);
				// floor
				if (modulo < (1 / this.snaps) * this.snapPercentage) {
					percentage = percentage - modulo;
				} else if (modulo > (1 / this.snaps) - (1 / this.snaps) * this.snapPercentage) {
					percentage = percentage - modulo + (1 / this.snaps);
				}
				percentage = Math.max(0, Math.min(1, percentage));
			}
			this.value.style.width = "" + (percentage * 100) + "%";
			// this.knob.style.left = "" + (-8 + percentage * this.slider.clientWidth) + "px";
			return percentage;
		}
	}

	export class SpinePlayer {
		static HOVER_COLOR_INNER = new spine.Color(0.478, 0, 0, 0.25);
		static HOVER_COLOR_OUTER = new spine.Color(1, 1, 1, 1);
		static NON_HOVER_COLOR_INNER = new spine.Color(0.478, 0, 0, 0.5);
		static NON_HOVER_COLOR_OUTER = new spine.Color(1, 0, 0, 0.8);

		private sceneRenderer: spine.webgl.SceneRenderer;
		private dom: HTMLElement;
		private playerControls: HTMLElement;
		private canvas: HTMLCanvasElement;
		private timelineSlider: Slider;
		private playButton: HTMLElement;
		private skinButton: HTMLElement;
		private animationButton: HTMLElement;

		private context: spine.webgl.ManagedWebGLRenderingContext;
		private loadingScreen: spine.webgl.LoadingScreen;
		private assetManager: spine.webgl.AssetManager;

		private loaded: boolean;
		private skeleton: Skeleton;
		private animationState: AnimationState;
		private time = new TimeKeeper();
		private paused = true;
		private playTime = 0;
		private speed = 1;

		private animationViewports: Map<Viewport> = {}
		private currentViewport: Viewport = null;
		private previousViewport: Viewport = null;
		private viewportTransitionStart = 0;

		private selectedBones: Bone[];
		private parent: HTMLElement;

		private stopRequestAnimationFrame = false;

		constructor(parent: HTMLElement | string, private config: SpinePlayerConfig) {
			if (typeof parent === "string") this.parent = document.getElementById(parent);
			else this.parent = parent;
			this.parent.appendChild(this.render());
		}

		validateConfig(config: SpinePlayerConfig): SpinePlayerConfig {
			if (!config) throw new Error("Please pass a configuration to new.spine.SpinePlayer().");
			if (!config.jsonUrl && !config.skelUrl) throw new Error("Please specify the URL of the skeleton JSON or .skel file.");
			if (!config.atlasUrl) throw new Error("Please specify the URL of the atlas file.");
			if (!config.alpha) config.alpha = false;
			if (!config.backgroundColor) config.backgroundColor = "#000000";
			if (!config.fullScreenBackgroundColor) config.fullScreenBackgroundColor = config.backgroundColor;
			if (typeof config.premultipliedAlpha === "undefined") config.premultipliedAlpha = true;
			if (!config.success) config.success = (widget) => {};
			if (!config.error) config.error = (widget, msg) => {};
			if (!config.debug) config.debug = {
				bones: false,
				regions: false,
				meshes: false,
				bounds: false,
				clipping: false,
				paths: false,
				points: false,
				hulls: false
			}
			if (typeof config.debug.bones === "undefined") config.debug.bones = false;
			if (typeof config.debug.bounds === "undefined") config.debug.bounds = false;
			if (typeof config.debug.clipping === "undefined") config.debug.clipping = false;
			if (typeof config.debug.hulls === "undefined") config.debug.hulls = false;
			if (typeof config.debug.paths === "undefined") config.debug.paths = false;
			if (typeof config.debug.points === "undefined") config.debug.points = false;
			if (typeof config.debug.regions === "undefined") config.debug.regions = false;
			if (typeof config.debug.meshes === "undefined") config.debug.meshes = false;

			if (config.animations && config.animation) {
				if (config.animations.indexOf(config.animation) < 0) throw new Error("Default animation '" + config.animation + "' is not contained in the list of selectable animations " + escapeHtml(JSON.stringify(this.config.animations)) + ".");
			}

			if (config.skins && config.skin) {
				if (config.skins.indexOf(config.skin) < 0) throw new Error("Default skin '" + config.skin + "' is not contained in the list of selectable skins " + escapeHtml(JSON.stringify(this.config.skins)) + ".");
			}

			if (!config.controlBones) config.controlBones = [];

			if (typeof config.showControls === "undefined")
				config.showControls = true;

			if (typeof config.defaultMix === "undefined")
				config.defaultMix = 0.25;

			return config;
		}

		showError(error: string) {
			let errorDom = findWithClass(this.dom, "spine-player-error")[0];
			errorDom.classList.remove("spine-player-hidden");
			errorDom.innerHTML = `<p style="text-align: center; align-self: center;">${error}</p>`;
			this.config.error(this, error);
		}

		render(): HTMLElement {
			let config = this.config;
			let dom = this.dom = createElement(/*html*/`
				<div class="spine-player">
					<canvas class="spine-player-canvas"></canvas>
					<div class="spine-player-error spine-player-hidden"></div>
					<div class="spine-player-controls spine-player-popup-parent spine-player-controls-hidden">
						<div class="spine-player-timeline">
						</div>
						<div class="spine-player-buttons">
							<button id="spine-player-button-play-pause" class="spine-player-button spine-player-button-icon-pause"></button>
							<div class="spine-player-button-spacer"></div>
							<button id="spine-player-button-speed" class="spine-player-button spine-player-button-icon-speed"></button>
							<button id="spine-player-button-animation" class="spine-player-button spine-player-button-icon-animations"></button>
							<button id="spine-player-button-skin" class="spine-player-button spine-player-button-icon-skins"></button>
							<button id="spine-player-button-settings" class="spine-player-button spine-player-button-icon-settings"></button>
							<button id="spine-player-button-fullscreen" class="spine-player-button spine-player-button-icon-fullscreen"></button>
							<img id="spine-player-button-logo" class="spine-player-button-icon-spine-logo" src="data:image/svg+xml,%3Csvg%20id%3D%22Spine_Logo%22%20data-name%3D%22Spine%20Logo%22%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20viewBox%3D%220%200%20104%2031.16%22%3E%3Cdefs%3E%3Cstyle%3E.cls-1%7Bfill%3A%23fff%3B%7D.cls-2%7Bfill%3A%23ff4000%3B%7D%3C%2Fstyle%3E%3C%2Fdefs%3E%3Ctitle%3Espine-logo-white%3C%2Ftitle%3E%3Cpath%20id%3D%22e%22%20class%3D%22cls-1%22%20d%3D%22M104%2C12.68a1.31%2C1.31%2C0%2C0%2C1-.37%2C1%2C1.28%2C1.28%2C0%2C0%2C1-.85.31H91.57a10.51%2C10.51%2C0%2C0%2C0%2C.29%2C2.55%2C4.92%2C4.92%2C0%2C0%2C0%2C1%2C2A4.27%2C4.27%2C0%2C0%2C0%2C94.5%2C19.8a6.89%2C6.89%2C0%2C0%2C0%2C2.6.44%2C10.66%2C10.66%2C0%2C0%2C0%2C2.17-.2%2C12.81%2C12.81%2C0%2C0%2C0%2C1.64-.44q.69-.25%2C1.14-.44a1.87%2C1.87%2C0%2C0%2C1%2C.68-.2A.44.44%2C0%2C0%2C1%2C103%2C19a.43.43%2C0%2C0%2C1%2C.16.2%2C1.38%2C1.38%2C0%2C0%2C1%2C.09.37%2C4.89%2C4.89%2C0%2C0%2C1%2C0%2C.58%2C4.14%2C4.14%2C0%2C0%2C1%2C0%2C.43v.32a.83.83%2C0%2C0%2C1-.09.26%2C1.1%2C1.1%2C0%2C0%2C1-.17.22%2C2.77%2C2.77%2C0%2C0%2C1-.61.34%2C8.94%2C8.94%2C0%2C0%2C1-1.32.46%2C18.54%2C18.54%2C0%2C0%2C1-1.88.41%2C13.78%2C13.78%2C0%2C0%2C1-2.28.18%2C10.55%2C10.55%2C0%2C0%2C1-3.68-.59%2C6.82%2C6.82%2C0%2C0%2C1-2.66-1.74%2C7.44%2C7.44%2C0%2C0%2C1-1.63-2.89%2C13.48%2C13.48%2C0%2C0%2C1-.55-4%2C12.76%2C12.76%2C0%2C0%2C1%2C.57-3.94%2C8.35%2C8.35%2C0%2C0%2C1%2C1.64-3%2C7.15%2C7.15%2C0%2C0%2C1%2C2.58-1.87%2C8.47%2C8.47%2C0%2C0%2C1%2C3.39-.65%2C8.19%2C8.19%2C0%2C0%2C1%2C3.41.64%2C6.46%2C6.46%2C0%2C0%2C1%2C2.32%2C1.73A7%2C7%2C0%2C0%2C1%2C103.59%2C9a11.17%2C11.17%2C0%2C0%2C1%2C.43%2C3.13Zm-3.14-.93a5.69%2C5.69%2C0%2C0%2C0-1.09-3.86%2C4.17%2C4.17%2C0%2C0%2C0-3.42-1.4%2C4.52%2C4.52%2C0%2C0%2C0-2%2C.44%2C4.41%2C4.41%2C0%2C0%2C0-1.47%2C1.15A5.29%2C5.29%2C0%2C0%2C0%2C92%2C9.75a7%2C7%2C0%2C0%2C0-.36%2C2Z%22%2F%3E%3Cpath%20id%3D%22n%22%20class%3D%22cls-1%22%20d%3D%22M80.68%2C21.94a.42.42%2C0%2C0%2C1-.08.26.59.59%2C0%2C0%2C1-.25.18%2C1.74%2C1.74%2C0%2C0%2C1-.47.11%2C6.31%2C6.31%2C0%2C0%2C1-.76%2C0%2C6.5%2C6.5%2C0%2C0%2C1-.78%2C0%2C1.74%2C1.74%2C0%2C0%2C1-.47-.11.59.59%2C0%2C0%2C1-.25-.18.42.42%2C0%2C0%2C1-.08-.26V12a9.8%2C9.8%2C0%2C0%2C0-.23-2.35%2C4.86%2C4.86%2C0%2C0%2C0-.66-1.53%2C2.88%2C2.88%2C0%2C0%2C0-1.13-1%2C3.57%2C3.57%2C0%2C0%2C0-1.6-.34%2C4%2C4%2C0%2C0%2C0-2.35.83A12.71%2C12.71%2C0%2C0%2C0%2C69.11%2C10v11.9a.42.42%2C0%2C0%2C1-.08.26.59.59%2C0%2C0%2C1-.25.18%2C1.74%2C1.74%2C0%2C0%2C1-.47.11%2C6.51%2C6.51%2C0%2C0%2C1-.78%2C0%2C6.31%2C6.31%2C0%2C0%2C1-.76%2C0%2C1.88%2C1.88%2C0%2C0%2C1-.48-.11.52.52%2C0%2C0%2C1-.25-.18.46.46%2C0%2C0%2C1-.07-.26v-17A.53.53%2C0%2C0%2C1%2C66%2C4.69a.5.5%2C0%2C0%2C1%2C.23-.19%2C1.28%2C1.28%2C0%2C0%2C1%2C.44-.11%2C8.53%2C8.53%2C0%2C0%2C1%2C1.39%2C0%2C1.12%2C1.12%2C0%2C0%2C1%2C.43.11.6.6%2C0%2C0%2C1%2C.22.19.47.47%2C0%2C0%2C1%2C.07.26V7.2a10.46%2C10.46%2C0%2C0%2C1%2C2.87-2.36%2C6.17%2C6.17%2C0%2C0%2C1%2C2.88-.75%2C6.41%2C6.41%2C0%2C0%2C1%2C2.87.58%2C5.16%2C5.16%2C0%2C0%2C1%2C1.88%2C1.54%2C6.15%2C6.15%2C0%2C0%2C1%2C1%2C2.26%2C13.46%2C13.46%2C0%2C0%2C1%2C.31%2C3.11Z%22%2F%3E%3Cg%20id%3D%22i%22%3E%3Cpath%20class%3D%22cls-2%22%20d%3D%22M43.35%2C2.86c.09%2C2.6%2C1.89%2C4%2C5.48%2C4.61%2C3%2C.48%2C5.79.24%2C6.69-2.37%2C1.75-5.09-2.4-3.82-6-4.39S43.21-1.32%2C43.35%2C2.86Z%22%2F%3E%3Cpath%20class%3D%22cls-2%22%20d%3D%22M44.43%2C13.55c.33%2C1.94%2C2.14%2C3.06%2C4.91%2C3s4.84-1.16%2C5.13-3.25c.53-3.88-2.53-2.38-5.3-2.3S43.77%2C9.74%2C44.43%2C13.55Z%22%2F%3E%3Cpath%20class%3D%22cls-2%22%20d%3D%22M48%2C22.44c.55%2C1.45%2C2.06%2C2.06%2C4.1%2C1.63s3.45-1.11%2C3.33-2.76c-.21-3.06-2.22-2.1-4.26-1.66S47%2C19.6%2C48%2C22.44Z%22%2F%3E%3Cpath%20class%3D%22cls-2%22%20d%3D%22M49.78%2C29.22c.16%2C1.22%2C1.22%2C2%2C2.88%2C1.93s2.92-.67%2C3.13-2c.4-2.43-1.46-1.53-3.12-1.51S49.5%2C26.82%2C49.78%2C29.22Z%22%2F%3E%3C%2Fg%3E%3Cpath%20id%3D%22p%22%20class%3D%22cls-1%22%20d%3D%22M35.28%2C13.16a15.33%2C15.33%2C0%2C0%2C1-.48%2C4%2C8.75%2C8.75%2C0%2C0%2C1-1.42%2C3%2C6.35%2C6.35%2C0%2C0%2C1-2.32%2C1.91%2C7.14%2C7.14%2C0%2C0%2C1-3.16.67%2C6.1%2C6.1%2C0%2C0%2C1-1.4-.15%2C5.34%2C5.34%2C0%2C0%2C1-1.26-.47A7.29%2C7.29%2C0%2C0%2C1%2C24%2C21.31q-.61-.49-1.29-1.15v8.51a.47.47%2C0%2C0%2C1-.08.26.56.56%2C0%2C0%2C1-.25.19%2C1.74%2C1.74%2C0%2C0%2C1-.47.11%2C6.47%2C6.47%2C0%2C0%2C1-.78%2C0%2C6.26%2C6.26%2C0%2C0%2C1-.76%2C0%2C1.89%2C1.89%2C0%2C0%2C1-.48-.11.49.49%2C0%2C0%2C1-.25-.19.51.51%2C0%2C0%2C1-.07-.26V4.91a.57.57%2C0%2C0%2C1%2C.06-.27.46.46%2C0%2C0%2C1%2C.23-.18%2C1.47%2C1.47%2C0%2C0%2C1%2C.44-.1%2C7.41%2C7.41%2C0%2C0%2C1%2C1.3%2C0%2C1.45%2C1.45%2C0%2C0%2C1%2C.43.1.52.52%2C0%2C0%2C1%2C.24.18.51.51%2C0%2C0%2C1%2C.07.27V7.2a18.06%2C18.06%2C0%2C0%2C1%2C1.49-1.38%2C9%2C9%2C0%2C0%2C1%2C1.45-1%2C6.82%2C6.82%2C0%2C0%2C1%2C1.49-.59%2C7.09%2C7.09%2C0%2C0%2C1%2C4.78.52%2C6%2C6%2C0%2C0%2C1%2C2.13%2C2%2C8.79%2C8.79%2C0%2C0%2C1%2C1.2%2C2.9A15.72%2C15.72%2C0%2C0%2C1%2C35.28%2C13.16ZM32%2C13.52a15.64%2C15.64%2C0%2C0%2C0-.2-2.53%2C7.32%2C7.32%2C0%2C0%2C0-.69-2.17%2C4.06%2C4.06%2C0%2C0%2C0-1.3-1.51%2C3.49%2C3.49%2C0%2C0%2C0-2-.57%2C4.1%2C4.1%2C0%2C0%2C0-1.2.18%2C4.92%2C4.92%2C0%2C0%2C0-1.2.57%2C8.54%2C8.54%2C0%2C0%2C0-1.28%2C1A15.77%2C15.77%2C0%2C0%2C0%2C22.76%2C10v6.77a13.53%2C13.53%2C0%2C0%2C0%2C2.46%2C2.4%2C4.12%2C4.12%2C0%2C0%2C0%2C2.44.83%2C3.56%2C3.56%2C0%2C0%2C0%2C2-.57A4.28%2C4.28%2C0%2C0%2C0%2C31%2C18a7.58%2C7.58%2C0%2C0%2C0%2C.77-2.12A11.43%2C11.43%2C0%2C0%2C0%2C32%2C13.52Z%22%2F%3E%3Cpath%20id%3D%22s%22%20class%3D%22cls-1%22%20d%3D%22M12%2C17.3a5.39%2C5.39%2C0%2C0%2C1-.48%2C2.33%2C4.73%2C4.73%2C0%2C0%2C1-1.37%2C1.72%2C6.19%2C6.19%2C0%2C0%2C1-2.12%2C1.06%2C9.62%2C9.62%2C0%2C0%2C1-2.71.36%2C10.38%2C10.38%2C0%2C0%2C1-3.21-.5%2C7.63%2C7.63%2C0%2C0%2C1-1.11-.45%2C3.25%2C3.25%2C0%2C0%2C1-.66-.43%2C1.09%2C1.09%2C0%2C0%2C1-.3-.53A3.59%2C3.59%2C0%2C0%2C1%2C0%2C19.93a4.06%2C4.06%2C0%2C0%2C1%2C0-.61%2C2%2C2%2C0%2C0%2C1%2C.09-.4.42.42%2C0%2C0%2C1%2C.16-.22.43.43%2C0%2C0%2C1%2C.24-.07%2C1.35%2C1.35%2C0%2C0%2C1%2C.61.26q.41.26%2C1%2C.56A9.22%2C9.22%2C0%2C0%2C0%2C3.51%2C20a6.25%2C6.25%2C0%2C0%2C0%2C1.87.26%2C5.62%2C5.62%2C0%2C0%2C0%2C1.44-.17%2C3.48%2C3.48%2C0%2C0%2C0%2C1.12-.5%2C2.23%2C2.23%2C0%2C0%2C0%2C.73-.84%2C2.68%2C2.68%2C0%2C0%2C0%2C.26-1.21%2C2%2C2%2C0%2C0%2C0-.37-1.21%2C3.55%2C3.55%2C0%2C0%2C0-1-.87A8.09%2C8.09%2C0%2C0%2C0%2C6.2%2C14.8l-1.56-.61a16%2C16%2C0%2C0%2C1-1.57-.73%2C6%2C6%2C0%2C0%2C1-1.37-1%2C4.52%2C4.52%2C0%2C0%2C1-1-1.4%2C4.69%2C4.69%2C0%2C0%2C1-.37-2A4.88%2C4.88%2C0%2C0%2C1%2C.72%2C7.19%2C4.46%2C4.46%2C0%2C0%2C1%2C1.88%2C5.58%2C5.83%2C5.83%2C0%2C0%2C1%2C3.82%2C4.47%2C8.06%2C8.06%2C0%2C0%2C1%2C6.53%2C4a8.28%2C8.28%2C0%2C0%2C1%2C1.36.11%2C9.36%2C9.36%2C0%2C0%2C1%2C1.23.28%2C5.92%2C5.92%2C0%2C0%2C1%2C.94.37%2C4.09%2C4.09%2C0%2C0%2C1%2C.59.35%2C1%2C1%2C0%2C0%2C1%2C.26.26.83.83%2C0%2C0%2C1%2C.09.26%2C1.32%2C1.32%2C0%2C0%2C0%2C.06.35%2C3.87%2C3.87%2C0%2C0%2C1%2C0%2C.51%2C4.76%2C4.76%2C0%2C0%2C1%2C0%2C.56%2C1.39%2C1.39%2C0%2C0%2C1-.09.39.5.5%2C0%2C0%2C1-.16.22.35.35%2C0%2C0%2C1-.21.07%2C1%2C1%2C0%2C0%2C1-.49-.21%2C7%2C7%2C0%2C0%2C0-.83-.44%2C9.26%2C9.26%2C0%2C0%2C0-1.2-.44A5.49%2C5.49%2C0%2C0%2C0%2C6.5%2C6.48a4.93%2C4.93%2C0%2C0%2C0-1.4.18%2C2.69%2C2.69%2C0%2C0%2C0-1%2C.51A2.16%2C2.16%2C0%2C0%2C0%2C3.51%2C8a2.43%2C2.43%2C0%2C0%2C0-.2%2C1%2C2%2C2%2C0%2C0%2C0%2C.38%2C1.24%2C3.6%2C3.6%2C0%2C0%2C0%2C1%2C.88%2C8.25%2C8.25%2C0%2C0%2C0%2C1.38.68l1.58.62q.8.32%2C1.59.72a6%2C6%2C0%2C0%2C1%2C1.39%2C1%2C4.37%2C4.37%2C0%2C0%2C1%2C1%2C1.36A4.46%2C4.46%2C0%2C0%2C1%2C12%2C17.3Z%22%2F%3E%3C%2Fsvg%3E"/>
						</div>
					</div>
				</div>
			`)

			try {
				// Validate the configuration
				this.config = this.validateConfig(config);
			} catch (e) {
				this.showError(e);
				return dom
			}

			try {
				// Setup the scene renderer and OpenGL context
				this.canvas = findWithClass(dom, "spine-player-canvas")[0] as HTMLCanvasElement;
				var webglConfig = { alpha: config.alpha };
				this.context = new spine.webgl.ManagedWebGLRenderingContext(this.canvas, webglConfig);
				// Setup the scene renderer and loading screen
				this.sceneRenderer = new spine.webgl.SceneRenderer(this.canvas, this.context, true);
				this.loadingScreen = new spine.webgl.LoadingScreen(this.sceneRenderer);
			} catch (e) {
				this.showError("Sorry, your browser does not support WebGL.<br><br>Please use the latest version of Firefox, Chrome, Edge, or Safari.");
				return dom;
			}

			// Load the assets
			this.assetManager = new spine.webgl.AssetManager(this.context);
			if (config.rawDataURIs) {
				for (let path in config.rawDataURIs) {
					let data = config.rawDataURIs[path];
					this.assetManager.setRawDataURI(path, data);
				}
			}
			if (config.jsonUrl) this.assetManager.loadText(config.jsonUrl);
			else this.assetManager.loadBinary(config.skelUrl);
			this.assetManager.loadTextureAtlas(config.atlasUrl);
			if (config.backgroundImage && config.backgroundImage.url)
				this.assetManager.loadTexture(config.backgroundImage.url);

			// Setup rendering loop
			requestAnimationFrame(() => this.drawFrame());

			// Setup the event listeners for UI elements
			this.playerControls = findWithClass(dom, "spine-player-controls")[0];
			let timeline = findWithClass(dom, "spine-player-timeline")[0];
			this.timelineSlider = new Slider();
			timeline.appendChild(this.timelineSlider.render());
			this.playButton = findWithId(dom, "spine-player-button-play-pause")[0];
			let speedButton = findWithId(dom, "spine-player-button-speed")[0];
			this.animationButton = findWithId(dom, "spine-player-button-animation")[0];
			this.skinButton = findWithId(dom, "spine-player-button-skin")[0];
			let settingsButton = findWithId(dom, "spine-player-button-settings")[0];
			let fullscreenButton = findWithId(dom, "spine-player-button-fullscreen")[0];
			let logoButton = findWithId(dom, "spine-player-button-logo")[0];

			this.playButton.onclick = () => {
				if (this.paused) this.play()
				else this.pause();
			}

			speedButton.onclick = () => {
				this.showSpeedDialog(speedButton);
			}

			this.animationButton.onclick = () => {
				this.showAnimationsDialog(this.animationButton);
			}

			this.skinButton.onclick = () => {
				this.showSkinsDialog(this.skinButton);
			}

			settingsButton.onclick = () => {
				this.showSettingsDialog(settingsButton);
			}

			let oldWidth = this.canvas.clientWidth;
			let oldHeight = this.canvas.clientHeight;
			let oldStyleWidth = this.canvas.style.width;
			let oldStyleHeight = this.canvas.style.height;
			var isFullscreen = false;
			fullscreenButton.onclick = () => {
				let fullscreenChanged = () => {
					isFullscreen = !isFullscreen;
					if (!isFullscreen) {
						this.canvas.style.width = "" + oldWidth + "px";
						this.canvas.style.height = "" + oldHeight + "px";
						this.drawFrame(false);
						// Got to reset the style to whatever the user set
						// after the next layouting.
						requestAnimationFrame(() => {
							this.canvas.style.width = oldStyleWidth;
							this.canvas.style.height = oldStyleHeight;
						});
					}
				};

				let doc = document as any;
				(dom as any).onfullscreenchange = fullscreenChanged;
				(dom as any).onwebkitfullscreenchange = fullscreenChanged;

				if(doc.fullscreenElement || doc.webkitFullscreenElement || doc.mozFullScreenElement || doc.msFullscreenElement) {
					if (doc.exitFullscreen) doc.exitFullscreen();
					else if (doc.mozCancelFullScreen) doc.mozCancelFullScreen();
					else if (doc.webkitExitFullscreen) doc.webkitExitFullscreen()
					else if (doc.msExitFullscreen) doc.msExitFullscreen();
				} else {
					oldWidth = this.canvas.clientWidth;
					oldHeight = this.canvas.clientHeight;
					oldStyleWidth = this.canvas.style.width;
					oldStyleHeight = this.canvas.style.height;
					let player = dom as any;
					if (player.requestFullscreen) player.requestFullscreen();
					else if (player.webkitRequestFullScreen) player.webkitRequestFullScreen();
					else if (player.mozRequestFullScreen) player.mozRequestFullScreen();
					else if (player.msRequestFullscreen) player.msRequestFullscreen();
				}
			};

			logoButton.onclick = () => {
				window.open("http://esotericsoftware.com");
			};

			// Register a global resize handler to redraw and avoid flicker
			window.onresize = () => {
				this.drawFrame(false);
			}

			return dom;
		}

		private lastPopup: Popup;
		showSpeedDialog (speedButton: HTMLElement) {
			if (this.lastPopup) this.lastPopup.dom.remove()
			if (this.lastPopup && findWithClass(this.lastPopup.dom, "spine-player-popup-title")[0].textContent == "Speed") {
				this.lastPopup = null;
				speedButton.classList.remove("spine-player-button-icon-speed-selected")
				return;
			}
			let popup = new Popup(this.dom, this.playerControls, /*html*/`
				<div class="spine-player-popup-title">Speed</div>
				<hr>
				<div class="spine-player-row" style="user-select: none; align-items: center; padding: 8px;">
					<div class="spine-player-column">
						<div class="spine-player-speed-slider" style="margin-bottom: 4px;"></div>
						<div class="spine-player-row" style="justify-content: space-between;">
							<div>0.1x</div>
							<div>1x</div>
							<div>2x</div>
						</div>
					</div>
				</div>
			`);
			let sliderParent = findWithClass(popup.dom, "spine-player-speed-slider")[0];
			let slider = new Slider(2, 0.1, true);
			sliderParent.appendChild(slider.render());
			slider.setValue(this.speed / 2);
			slider.change = (percentage) => {
				this.speed = percentage * 2;
			}
			speedButton.classList.add("spine-player-button-icon-speed-selected")
			popup.show(() => {
				speedButton.classList.remove("spine-player-button-icon-speed-selected")
				popup.dom.remove();
				this.lastPopup = null;
			});
			this.lastPopup = popup;
		}

		showAnimationsDialog (animationsButton: HTMLElement) {
			if (this.lastPopup) this.lastPopup.dom.remove()
			if (this.lastPopup && findWithClass(this.lastPopup.dom, "spine-player-popup-title")[0].textContent == "Animations") {
				this.lastPopup = null;
				animationsButton.classList.remove("spine-player-button-icon-animations-selected")
				return;
			}
			if (!this.skeleton || this.skeleton.data.animations.length == 0) return;

			let popup = new Popup(this.dom, this.playerControls, /*html*/`
				<div class="spine-player-popup-title">Animations</div>
				<hr>
				<ul class="spine-player-list"></ul>
			`);

			let rows = findWithClass(popup.dom, "spine-player-list")[0];
			this.skeleton.data.animations.forEach((animation) => {
				// skip animations not whitelisted if a whitelist is given
				if (this.config.animations && this.config.animations.indexOf(animation.name) < 0) {
					return;
				}

				let row = createElement(/*html*/`
					<li class="spine-player-list-item selectable">
						<div class="selectable-circle">
						</div>
						<div class="selectable-text">
						</div>
					</li>
				`);
				if (animation.name == this.config.animation) row.classList.add("selected");
				findWithClass(row, "selectable-text")[0].innerText = animation.name;
				rows.appendChild(row);
				row.onclick = () => {
					removeClass(rows.children, "selected");
					row.classList.add("selected");
					this.config.animation = animation.name;
					this.playTime = 0;
					this.setAnimation(animation.name);
				}
			});
			animationsButton.classList.add("spine-player-button-icon-animations-selected")
			popup.show(() => {
				animationsButton.classList.remove("spine-player-button-icon-animations-selected")
				popup.dom.remove();
				this.lastPopup = null;
			});
			this.lastPopup = popup;
		}

		showSkinsDialog (skinButton: HTMLElement) {
			if (this.lastPopup) this.lastPopup.dom.remove()
			if (this.lastPopup && findWithClass(this.lastPopup.dom, "spine-player-popup-title")[0].textContent == "Skins") {
				this.lastPopup = null;
				skinButton.classList.remove("spine-player-button-icon-skins-selected")
				return;
			}
			if (!this.skeleton || this.skeleton.data.animations.length == 0) return;

			let popup = new Popup(this.dom, this.playerControls, /*html*/`
				<div class="spine-player-popup-title">Skins</div>
				<hr>
				<ul class="spine-player-list"></ul>
			`);

			let rows = findWithClass(popup.dom, "spine-player-list")[0];
			this.skeleton.data.skins.forEach((skin) => {
				// skip skins not whitelisted if a whitelist is given
				if (this.config.skins && this.config.skins.indexOf(skin.name) < 0) {
					return;
				}

				let row = createElement(/*html*/`
					<li class="spine-player-list-item selectable">
						<div class="selectable-circle">
						</div>
						<div class="selectable-text">
						</div>
					</li>
				`);
				if (skin.name == this.config.skin) row.classList.add("selected");
				findWithClass(row, "selectable-text")[0].innerText = skin.name;
				rows.appendChild(row);
				row.onclick = () => {
					removeClass(rows.children, "selected");
					row.classList.add("selected");
					this.config.skin = skin.name;
					this.skeleton.setSkinByName(this.config.skin);
					this.skeleton.setSlotsToSetupPose();
				}
			});

			skinButton.classList.add("spine-player-button-icon-skins-selected")
			popup.show(() => {
				skinButton.classList.remove("spine-player-button-icon-skins-selected")
				popup.dom.remove();
				this.lastPopup = null;
			});
			this.lastPopup = popup;
		}

		showSettingsDialog (settingsButton: HTMLElement) {
			if (this.lastPopup) this.lastPopup.dom.remove()
			if (this.lastPopup && findWithClass(this.lastPopup.dom, "spine-player-popup-title")[0].textContent == "Debug") {
				this.lastPopup = null;
				settingsButton.classList.remove("spine-player-button-icon-settings-selected")
				return;
			}
			if (!this.skeleton || this.skeleton.data.animations.length == 0) return;

			let popup = new Popup(this.dom, this.playerControls, /*html*/`
				<div class="spine-player-popup-title">Debug</div>
				<hr>
				<ul class="spine-player-list">
				</li>
			`);

			let rows = findWithClass(popup.dom, "spine-player-list")[0];
			let makeItem = (label: string, name: string) => {
				let row = createElement(/*html*/`<li class="spine-player-list-item"></li>`);
				let s = new Switch(label);
				row.appendChild(s.render());
				s.setEnabled((this.config.debug as any)[name]);
				s.change = (value) => {
					(this.config.debug as any)[name] = value;
				}
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

			settingsButton.classList.add("spine-player-button-icon-settings-selected")
			popup.show(() => {
				settingsButton.classList.remove("spine-player-button-icon-settings-selected")
				popup.dom.remove();
				this.lastPopup = null;
			});
			this.lastPopup = popup;
		}

		drawFrame (requestNextFrame = true) {
			if (requestNextFrame && !this.stopRequestAnimationFrame) requestAnimationFrame(() => this.drawFrame());
			let ctx = this.context;
			let gl = ctx.gl;

			// Clear the viewport
			var doc = document as any;
			var isFullscreen = doc.fullscreenElement || doc.webkitFullscreenElement || doc.mozFullScreenElement || doc.msFullscreenElement;
			let bg = new Color().setFromString(isFullscreen ? this.config.fullScreenBackgroundColor : this.config.backgroundColor);
			gl.clearColor(bg.r, bg.g, bg.b, bg.a);
			gl.clear(gl.COLOR_BUFFER_BIT);

			// Display loading screen
			this.loadingScreen.backgroundColor.setFromColor(bg);
			this.loadingScreen.draw(this.assetManager.isLoadingComplete());

			// Have we finished loading the asset? Then set things up
			if (this.assetManager.isLoadingComplete() && this.skeleton == null) this.loadSkeleton();

			// Resize the canvas
			this.sceneRenderer.resize(webgl.ResizeMode.Expand);

			// Update and draw the skeleton
			if (this.loaded) {
				// Update animation and skeleton based on user selections
				if (!this.paused && this.config.animation) {
					this.time.update();
					let delta = this.time.delta * this.speed;

					let animationDuration = this.animationState.getCurrent(0).animation.duration;
					this.playTime += delta;
					while (this.playTime >= animationDuration && animationDuration != 0) {
						this.playTime -= animationDuration;
					}
					this.playTime = Math.max(0, Math.min(this.playTime, animationDuration));
					this.timelineSlider.setValue(this.playTime / animationDuration);

					this.animationState.update(delta);
					this.animationState.apply(this.skeleton);
				}

				this.skeleton.updateWorldTransform();

				let viewport = {
					x: this.currentViewport.x - (this.currentViewport.padLeft as number),
					y: this.currentViewport.y - (this.currentViewport.padBottom as number),
					width: this.currentViewport.width + (this.currentViewport.padLeft as number) + (this.currentViewport.padRight as number),
					height: this.currentViewport.height + (this.currentViewport.padBottom as number) + (this.currentViewport.padTop as number)
				}

				let transitionAlpha = ((performance.now() - this.viewportTransitionStart) / 1000) / this.config.viewport.transitionTime;
				if (this.previousViewport && transitionAlpha < 1) {
					let oldViewport = {
						x: this.previousViewport.x - (this.previousViewport.padLeft as number),
						y: this.previousViewport.y - (this.previousViewport.padBottom as number),
						width: this.previousViewport.width + (this.previousViewport.padLeft as number) + (this.previousViewport.padRight as number),
						height: this.previousViewport.height + (this.previousViewport.padBottom as number) + (this.previousViewport.padTop as number)
					}

					viewport = {
						x: oldViewport.x + (viewport.x - oldViewport.x) * transitionAlpha,
						y: oldViewport.y + (viewport.y - oldViewport.y) * transitionAlpha,
						width: oldViewport.width + (viewport.width - oldViewport.width) * transitionAlpha,
						height: oldViewport.height + (viewport.height - oldViewport.height) * transitionAlpha
					}
				}

				let viewportSize = this.scale(viewport.width, viewport.height, this.canvas.width, this.canvas.height);

				this.sceneRenderer.camera.zoom = viewport.width / viewportSize.x;
				this.sceneRenderer.camera.position.x = viewport.x + viewport.width / 2;
				this.sceneRenderer.camera.position.y = viewport.y + viewport.height / 2;

				this.sceneRenderer.begin();

				// Draw background image if given
				if (this.config.backgroundImage && this.config.backgroundImage.url) {
					let bgImage = this.assetManager.get(this.config.backgroundImage.url);
					if (!(this.config.backgroundImage.hasOwnProperty("x") && this.config.backgroundImage.hasOwnProperty("y") && this.config.backgroundImage.hasOwnProperty("width") && this.config.backgroundImage.hasOwnProperty("height"))) {
						this.sceneRenderer.drawTexture(bgImage, viewport.x, viewport.y, viewport.width, viewport.height);
					} else {
						this.sceneRenderer.drawTexture(bgImage, this.config.backgroundImage.x, this.config.backgroundImage.y, this.config.backgroundImage.width, this.config.backgroundImage.height);
					}
				}

				// Draw skeleton and debug output
				this.sceneRenderer.drawSkeleton(this.skeleton, this.config.premultipliedAlpha);
				this.sceneRenderer.skeletonDebugRenderer.drawBones = this.config.debug.bones;
				this.sceneRenderer.skeletonDebugRenderer.drawBoundingBoxes = this.config.debug.bounds;
				this.sceneRenderer.skeletonDebugRenderer.drawClipping = this.config.debug.clipping;
				this.sceneRenderer.skeletonDebugRenderer.drawMeshHull = this.config.debug.hulls;
				this.sceneRenderer.skeletonDebugRenderer.drawPaths = this.config.debug.paths;
				this.sceneRenderer.skeletonDebugRenderer.drawRegionAttachments = this.config.debug.regions;
				this.sceneRenderer.skeletonDebugRenderer.drawMeshTriangles = this.config.debug.meshes;
				this.sceneRenderer.drawSkeletonDebug(this.skeleton, this.config.premultipliedAlpha);

				// Render the selected bones
				let controlBones = this.config.controlBones;
				let selectedBones = this.selectedBones;
				let skeleton = this.skeleton;
				gl.lineWidth(2);
				for (var i = 0; i < controlBones.length; i++) {
					var bone = skeleton.findBone(controlBones[i]);
					if (!bone) continue;
					var colorInner = selectedBones[i] !== null ? SpinePlayer.HOVER_COLOR_INNER : SpinePlayer.NON_HOVER_COLOR_INNER;
					var colorOuter = selectedBones[i] !== null ? SpinePlayer.HOVER_COLOR_OUTER : SpinePlayer.NON_HOVER_COLOR_OUTER;
					this.sceneRenderer.circle(true, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorInner);
					this.sceneRenderer.circle(false, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorOuter);
				}
				gl.lineWidth(1);

				// Render the viewport bounds
				if (this.config.viewport.debugRender) {
					this.sceneRenderer.rect(false, this.currentViewport.x, this.currentViewport.y, this.currentViewport.width, this.currentViewport.height, Color.GREEN);
					this.sceneRenderer.rect(false, viewport.x, viewport.y, viewport.width, viewport.height, Color.RED);
				}

				this.sceneRenderer.end();

				this.sceneRenderer.camera.zoom = 0;
			}
		}

		scale(sourceWidth: number, sourceHeight: number, targetWidth: number, targetHeight: number): Vector2 {
			let targetRatio = targetHeight / targetWidth;
			let sourceRatio = sourceHeight / sourceWidth;
			let scale = targetRatio > sourceRatio ? targetWidth / sourceWidth : targetHeight / sourceHeight;
			let temp = new spine.Vector2();
			temp.x = sourceWidth * scale;
			temp.y = sourceHeight * scale;
			return temp;
		}

		loadSkeleton () {
			if (this.loaded) return;

			if (this.assetManager.hasErrors()) {
				this.showError("Error: assets could not be loaded.<br><br>" + escapeHtml(JSON.stringify(this.assetManager.getErrors())));
				return;
			}

			let atlas = this.assetManager.get(this.config.atlasUrl);
			let skeletonData: SkeletonData;
			if (this.config.jsonUrl) {
				let jsonText = this.assetManager.get(this.config.jsonUrl);
				let json = new SkeletonJson(new AtlasAttachmentLoader(atlas));
				try {
					skeletonData = json.readSkeletonData(jsonText);
				} catch (e) {
					this.showError("Error: could not load skeleton .json.<br><br>" + escapeHtml(JSON.stringify(e)));
					return;
				}
			} else {
				let binaryData = this.assetManager.get(this.config.skelUrl);
				let binary = new SkeletonBinary(new AtlasAttachmentLoader(atlas));
				try {
					skeletonData = binary.readSkeletonData(binaryData);
				} catch (e) {
					this.showError("Error: could not load skeleton .skel.<br><br>" + escapeHtml(JSON.stringify(e)));
					return;
				}
			}
			this.skeleton = new Skeleton(skeletonData);
			let stateData = new AnimationStateData(skeletonData);
			stateData.defaultMix = this.config.defaultMix;
			this.animationState = new AnimationState(stateData);

			// Check if all controllable bones are in the skeleton
			if (this.config.controlBones) {
				this.config.controlBones.forEach(bone => {
					if (!skeletonData.findBone(bone)) {
						this.showError(`Error: control bone '${bone}' does not exist in skeleton.`);
					}
				})
			}

			// Setup skin
			if (!this.config.skin) {
				if (skeletonData.skins.length > 0) {
					this.config.skin = skeletonData.skins[0].name;
				}
			}

			if (this.config.skins && this.config.skin.length > 0) {
				this.config.skins.forEach(skin => {
					if (!this.skeleton.data.findSkin(skin)) {
						this.showError(`Error: skin '${skin}' in selectable skin list does not exist in skeleton.`);
						return;
					}
				});
			}

			if (this.config.skin) {
				if (!this.skeleton.data.findSkin(this.config.skin)) {
					this.showError(`Error: skin '${this.config.skin}' does not exist in skeleton.`);
					return;
				}
				this.skeleton.setSkinByName(this.config.skin);
				this.skeleton.setSlotsToSetupPose();
			}

			// Setup empty viewport if none is given and check
			// if all animations for which viewports where given
			// exist.
			if (!this.config.viewport) {
				(this.config.viewport as any) = {
					animations: {},
					debugRender: false,
					transitionTime: 0.2
				}
			}
			if (typeof this.config.viewport.debugRender === "undefined") this.config.viewport.debugRender = false;
			if (typeof this.config.viewport.transitionTime === "undefined") this.config.viewport.transitionTime = 0.2;
			if (!this.config.viewport.animations) {
				this.config.viewport.animations = {};
			} else {
				Object.getOwnPropertyNames(this.config.viewport.animations).forEach((animation: string) => {
					if (!skeletonData.findAnimation(animation)) {
						this.showError(`Error: animation '${animation}' for which a viewport was specified does not exist in skeleton.`);
						return;
					}
				});
			}

			// Setup the animations after viewport, so default bounds don't get messed up.
			if (this.config.animations && this.config.animations.length > 0) {
				this.config.animations.forEach(animation => {
					if (!this.skeleton.data.findAnimation(animation)) {
						this.showError(`Error: animation '${animation}' in selectable animation list does not exist in skeleton.`);
						return;
					}
				});

				if (!this.config.animation) {
					this.config.animation = this.config.animations[0];
				}
			}

			if (!this.config.animation) {
				if (skeletonData.animations.length > 0) {
					this.config.animation = skeletonData.animations[0].name;
				}
			}

			if(this.config.animation) {
				if (!skeletonData.findAnimation(this.config.animation)) {
					this.showError(`Error: animation '${this.config.animation}' does not exist in skeleton.`);
					return;
				}
				this.play()
				this.timelineSlider.change = (percentage) => {
					this.pause();
					var animationDuration = this.animationState.getCurrent(0).animation.duration;
					var time = animationDuration * percentage;
					this.animationState.update(time - this.playTime);
					this.animationState.apply(this.skeleton);
					this.skeleton.updateWorldTransform();
					this.playTime = time;
				}
			}

			// Setup the input processor and controllable bones
			this.setupInput();

			// Hide skin and animation if there's only the default skin / no animation
			if (skeletonData.skins.length == 1 || (this.config.skins && this.config.skins.length == 1)) this.skinButton.classList.add("spine-player-hidden");
			if (skeletonData.animations.length == 1 || (this.config.animations && this.config.animations.length == 1)) this.animationButton.classList.add("spine-player-hidden");

			this.config.success(this);
			this.loaded = true;
		}

		private cancelId = 0;
		setupInput () {
			let controlBones = this.config.controlBones;
			let selectedBones = this.selectedBones = new Array<Bone>(this.config.controlBones.length);
			let canvas = this.canvas;
			let input = new spine.webgl.Input(canvas);
			var target:Bone = null;
			let coords = new spine.webgl.Vector3();
			let temp = new spine.webgl.Vector3();
			let temp2 = new spine.Vector2();
			let skeleton = this.skeleton
			let renderer = this.sceneRenderer;
			input.addListener({
				down: (x, y) => {
					for (var i = 0; i < controlBones.length; i++) {
						var bone = skeleton.findBone(controlBones[i]);
						if (!bone) continue;
						renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
						if (temp.set(skeleton.x + bone.worldX, skeleton.y + bone.worldY, 0).distance(coords) < 30) {
							target = bone;
						}
					}
				},
				up: (x, y) => {
					if (target) {
						target = null;
					} else {
						if (!this.config.showControls) return;
						if (this.paused)
							this.play()
						else
							this.pause();
					}
				},
				dragged: (x, y) => {
					if (target != null) {
						renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
						if (target.parent !== null) {
							target.parent.worldToLocal(temp2.set(coords.x - skeleton.x, coords.y - skeleton.y));
							target.x = temp2.x;
							target.y = temp2.y;
						} else {
							target.x = coords.x - skeleton.x;
							target.y = coords.y - skeleton.y;
						}
					}
				},
				moved: (x, y) => {
					for (var i = 0; i < controlBones.length; i++) {
						var bone = skeleton.findBone(controlBones[i]);
						if (!bone) continue;
						renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
						if (temp.set(skeleton.x + bone.worldX, skeleton.y + bone.worldY, 0).distance(coords) < 30) {
							selectedBones[i] = bone;
						} else {
							selectedBones[i] = null;
						}
					}
				}
			});

			// For the manual hover to work, we need to disable
			// hidding the controls if the mouse/touch entered
			// the clickable area of a child of the controls.
			// For this we need to register a mouse handler on
			// the document and see if we are within the canvas
			// area :/
			var mouseOverControls = true;
			var mouseOverCanvas = false;
			document.addEventListener("mousemove", (ev: UIEvent) => {
				if (ev instanceof MouseEvent) {
					handleHover(ev.clientX, ev.clientY);
				}
			});
			document.addEventListener("touchmove", (ev: UIEvent) => {
				if (ev instanceof TouchEvent) {
					var touches = ev.changedTouches;
					if (touches.length > 0) {
						var touch = touches[0];
						handleHover(touch.clientX, touch.clientY);
					}
				}
			});

			let handleHover = (mouseX: number, mouseY: number) => {
				if (!this.config.showControls) return;

				let popup = findWithClass(this.dom, "spine-player-popup");
				mouseOverControls = overlap(mouseX, mouseY, this.playerControls.getBoundingClientRect());
				mouseOverCanvas = overlap(mouseX, mouseY, this.canvas.getBoundingClientRect());
				clearTimeout(this.cancelId);
				let hide = popup.length == 0 && !mouseOverControls && !mouseOverCanvas && !this.paused;
				if (hide) {
					this.playerControls.classList.add("spine-player-controls-hidden");
				} else {
					this.playerControls.classList.remove("spine-player-controls-hidden");
				}
				if (!mouseOverControls && popup.length == 0 && !this.paused) {
					let remove = () => {
						if (!this.paused) this.playerControls.classList.add("spine-player-controls-hidden");
					};
					this.cancelId = setTimeout(remove, 1000);
				}
			}

			let overlap = (mouseX: number, mouseY: number, rect: DOMRect | ClientRect): boolean => {
					let x = mouseX - rect.left;
					let y = mouseY - rect.top;
					return x >= 0 && x <= rect.width && y >= 0 && y <= rect.height;
			}
		}

		private play () {
			this.paused = false;
			let remove = () => {
				if (!this.paused) this.playerControls.classList.add("spine-player-controls-hidden");
			};
			this.cancelId = setTimeout(remove, 1000);
			this.playButton.classList.remove("spine-player-button-icon-play");
			this.playButton.classList.add("spine-player-button-icon-pause");

			if (this.config.animation) {
				if (!this.animationState.getCurrent(0)) {
					this.setAnimation(this.config.animation);
				}
			}
		}

		private pause () {
			this.paused = true;
			this.playerControls.classList.remove("spine-player-controls-hidden");
			clearTimeout(this.cancelId);

			this.playButton.classList.remove("spine-player-button-icon-pause");
			this.playButton.classList.add("spine-player-button-icon-play");
		}

		public setAnimation (animation: string) {
			// Determine viewport
			this.previousViewport = this.currentViewport;
			let animViewport = this.calculateAnimationViewport(animation);

			// The calculated animation viewport is the base
			let viewport: Viewport = {
				x: animViewport.x,
				y: animViewport.y,
				width: animViewport.width,
				height: animViewport.height,
				padLeft: "10%",
				padRight: "10%",
				padTop: "10%",
				padBottom: "10%"
			}

			// Override with global viewport settings if they exist
			let globalViewport = this.config.viewport;
			if (typeof globalViewport.x !== "undefined" && typeof globalViewport.y !== "undefined" && typeof globalViewport.width !== "undefined" && typeof globalViewport.height !== "undefined") {
				viewport.x = globalViewport.x;
				viewport.y = globalViewport.y;
				viewport.width = globalViewport.width;
				viewport.height = globalViewport.height;
			}
			if (typeof globalViewport.padLeft !== "undefined") viewport.padLeft = globalViewport.padLeft;
			if (typeof globalViewport.padRight !== "undefined") viewport.padRight = globalViewport.padRight;
			if (typeof globalViewport.padTop !== "undefined") viewport.padTop = globalViewport.padTop;
			if (typeof globalViewport.padBottom !== "undefined") viewport.padBottom = globalViewport.padBottom;

			// Override with animation viewport settings given by user for final result.
			let userAnimViewport = this.config.viewport.animations[animation];
			if (userAnimViewport) {
				if (typeof userAnimViewport.x !== "undefined" && typeof userAnimViewport.y !== "undefined" && typeof userAnimViewport.width !== "undefined" && typeof userAnimViewport.height !== "undefined") {
					viewport.x = userAnimViewport.x;
					viewport.y = userAnimViewport.y;
					viewport.width = userAnimViewport.width;
					viewport.height = userAnimViewport.height;
				}
				if (typeof userAnimViewport.padLeft !== "undefined") viewport.padLeft = userAnimViewport.padLeft;
				if (typeof userAnimViewport.padRight !== "undefined") viewport.padRight = userAnimViewport.padRight;
				if (typeof userAnimViewport.padTop !== "undefined") viewport.padTop = userAnimViewport.padTop;
				if (typeof userAnimViewport.padBottom !== "undefined") viewport.padBottom = userAnimViewport.padBottom;
			}

			// Translate percentage paddings to world units
			viewport.padLeft = this.percentageToWorldUnit(viewport.width, viewport.padLeft);
			viewport.padRight = this.percentageToWorldUnit(viewport.width, viewport.padRight);
			viewport.padBottom = this.percentageToWorldUnit(viewport.height, viewport.padBottom);
			viewport.padTop = this.percentageToWorldUnit(viewport.height, viewport.padTop);

			// Adjust x, y, width, and height by padding.
			this.currentViewport = viewport;
			this.viewportTransitionStart = performance.now();

			this.animationState.clearTracks();
			this.skeleton.setToSetupPose();
			this.animationState.setAnimation(0, animation, true);
		}

		private percentageToWorldUnit(size: number, percentageOrAbsolute: string | number): number {
			if (typeof percentageOrAbsolute === "string") {
				return size * parseFloat(percentageOrAbsolute.substr(0, percentageOrAbsolute.length - 1)) / 100;
			} else {
				return percentageOrAbsolute;
			}
		}

		private calculateAnimationViewport (animationName: string) {
			let animation = this.skeleton.data.findAnimation(animationName);
			this.animationState.clearTracks();
			this.skeleton.setToSetupPose()
			this.animationState.setAnimationWith(0, animation, true);

			let steps = 100;
			let stepTime = animation.duration > 0 ? animation.duration / steps : 0;
			let minX = 100000000;
			let maxX = -100000000;
			let minY = 100000000;
			let maxY = -100000000;
			let offset = new spine.Vector2();
			let size = new spine.Vector2();

			for (var i = 0; i < steps; i++) {
				this.animationState.update(stepTime);
				this.animationState.apply(this.skeleton);
				this.skeleton.updateWorldTransform();
				this.skeleton.getBounds(offset, size);

				minX = Math.min(offset.x, minX);
				maxX = Math.max(offset.x + size.x, maxX);
				minY = Math.min(offset.y, minY);
				maxY = Math.max(offset.y + size.y, maxY);
			}

			offset.x = minX;
			offset.y = minY;
			size.x = maxX - minX;
			size.y = maxY - minY;

			return {
				x: offset.x,
				y: offset.y,
				width: size.x,
				height: size.y
			};
		}

		public stopRendering() {
			this.stopRequestAnimationFrame = true;
		}
	}

	function isContained(dom: HTMLElement, needle: HTMLElement): boolean {
		if (dom === needle) return true;
		let findRecursive = (dom: HTMLElement, needle: HTMLElement) => {
			for(var i = 0; i < dom.children.length; i++) {
				let child = dom.children[i] as HTMLElement;
				if (child === needle) return true;
				if (findRecursive(child, needle)) return true;
			}
			return false;
		};
		return findRecursive(dom, needle);
	}

	function findWithId(dom: HTMLElement, id: string): HTMLElement[] {
		let found = new Array<HTMLElement>()
		let findRecursive = (dom: HTMLElement, id: string, found: HTMLElement[]) => {
			for(var i = 0; i < dom.children.length; i++) {
				let child = dom.children[i] as HTMLElement;
				if (child.id === id) found.push(child);
				findRecursive(child, id, found);
			}
		};
		findRecursive(dom, id, found);
		return found;
	}

	function findWithClass(dom: HTMLElement, className: string): HTMLElement[] {
		let found = new Array<HTMLElement>()
		let findRecursive = (dom: HTMLElement, className: string, found: HTMLElement[]) => {
			for(var i = 0; i < dom.children.length; i++) {
				let child = dom.children[i] as HTMLElement;
				if (child.classList.contains(className)) found.push(child);
				findRecursive(child, className, found);
			}
		};
		findRecursive(dom, className, found);
		return found;
	}

	function createElement(html: string): HTMLElement {
		let dom = document.createElement("div");
		dom.innerHTML = html;
		return dom.children[0] as HTMLElement;
	}

	function removeClass(elements: HTMLCollection, clazz: string) {
		for (var i = 0; i < elements.length; i++) {
			elements[i].classList.remove(clazz);
		}
	}

	function escapeHtml(str: string) {
		if (!str) return "";
		return str
			 .replace(/&/g, "&amp;")
			 .replace(/</g, "&lt;")
			 .replace(/>/g, "&gt;")
			 .replace(/"/g, "&#34;")
			 .replace(/'/g, "&#39;");
	 }
 }
