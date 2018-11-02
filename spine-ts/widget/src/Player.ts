/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

 module spine {
	export interface SpinePlayerConfig {
		jsonUrl: string;
		atlasUrl: string;
		animation: string;
		skin: string;
		scale: number;
		x: number;
		y: number;
		alpha: boolean;
		backgroundColor: string;
		premultipliedAlpha: boolean;
		success: (widget: SpineWidget) => void;
		error: (widget: SpineWidget, msg: string) => void;
	}

	class Slider {
		private slider: HTMLElement;
		private value: HTMLElement;
		public change: (percentage: number) => void;

		constructor(parent: HTMLElement) {
			parent.innerHTML = /*html*/`
				<div class="spine-player-slider">
					<div class="spine-player-slider-value"></div>
				</div>
			`;
			this.slider = findWithClass(parent, "spine-player-slider")[0];
			this.value = findWithClass(parent, "spine-player-slider-value")[0];
			this.setValue(0);

			let input = new spine.webgl.Input(this.slider);
			var dragging = false;
			input.addListener({
				down: (x, y) => {
					dragging = true;
				},
				up: (x, y) => {
					dragging = false;
					let percentage = x / this.slider.clientWidth;
					this.setValue(x / this.slider.clientWidth);
					if (this.change) this.change(percentage);
				},
				moved: (x, y) => {
					if (dragging) {
						let percentage = x / this.slider.clientWidth;
						this.setValue(x / this.slider.clientWidth);
						if (this.change) this.change(percentage);
					}
				},
				dragged: (x, y) => {
					let percentage = x / this.slider.clientWidth;
					this.setValue(x / this.slider.clientWidth);
					if (this.change) this.change(percentage);
				}
			});
		}

		setValue(percentage: number) {
			percentage = Math.max(0, Math.min(1, percentage));
			this.value.style.width = "" + (percentage * 100) + "%";
		}
	}

	export class SpinePlayer {
		private sceneRenderer: spine.webgl.SceneRenderer;
		private canvas: HTMLCanvasElement;
		private context: spine.webgl.ManagedWebGLRenderingContext;
		private loadingScreen: spine.webgl.LoadingScreen;
		private assetManager: spine.webgl.AssetManager;
		private timelineSlider: Slider;
		private loaded: boolean;
		private skeleton: Skeleton;
		private animationState: AnimationState;
		private time = new TimeKeeper();

		private paused = true;
		private currentAnimation: string;

		constructor(parent: HTMLElement, private config: SpinePlayerConfig) {
			this.validateConfig(config);
			this.render(parent, config);
		}

		validateConfig(config: SpinePlayerConfig): SpinePlayerConfig {
			if (!config) throw new Error("Please pass a configuration to new.spine.SpinePlayer().");
			if (!config.jsonUrl) throw new Error("Please specify the URL of the skeleton JSON file.");
			if (!config.atlasUrl) throw new Error("Please specify the URL of the atlas file.");
			if (!config.scale) config.scale = 1;
			if (!config.x) config.x = 0;
			if (!config.y) config.y = 0;
			if (!config.alpha) config.alpha = false;
			if (!config.backgroundColor) config.backgroundColor = "#000000";
			if (!config.premultipliedAlpha) config.premultipliedAlpha = false;
			if (!config.success) config.success = (widget) => {};
			if (!config.error) config.error = (widget, msg) => {};
			return config;
		}

		render(parent: HTMLElement, config: SpinePlayerConfig) {
			parent.innerHTML = /*html*/`
				<div class="spine-player">
					<canvas class="spine-player-canvas"></canvas>
					<div class="spine-player-controls">
						<div class="spine-player-timeline">
						</div>
						<div class="spine-player-buttons">
							<button id="spine-player-button-play-pause" class="spine-player-button spine-player-button-icon-play"></button>
							<div class="spine-player-button-spacer"></div>
							<button id="spine-player-button-speed" class="spine-player-button">
								Speed
							</button>
							<button id="spine-player-button-animation" class="spine-player-button">
								Animation
							</button>
							<button id="spine-player-button-skin" class="spine-player-button">
								Skin
							</button>
							<button id="spine-player-button-settings" class="spine-player-button">
								Settings
							</button>
							<button id="spine-player-button-fullscreen" class="spine-player-button">
								Fullscreen
							</button>
						</div>
					</div>
				</div>
			`;

			// Setup the scene renderer and OpenGL context
			this.canvas = findWithClass(parent, "spine-player-canvas")[0] as HTMLCanvasElement;
			var webglConfig = { alpha: config.alpha };
			this.context = new spine.webgl.ManagedWebGLRenderingContext(this.canvas, webglConfig);

			// Setup the scene renderer and loading screen
			this.sceneRenderer = new spine.webgl.SceneRenderer(this.canvas, this.context, true);
			this.loadingScreen = new spine.webgl.LoadingScreen(this.sceneRenderer);

			// Load the assets
			this.assetManager = new spine.webgl.AssetManager(this.context);
			this.assetManager.loadText(config.jsonUrl);
			this.assetManager.loadTextureAtlas(config.atlasUrl);

			// Setup rendering loop
			requestAnimationFrame(() => this.drawFrame());

			// Setup the event listeners for UI elements
			let timeline = findWithClass(parent, "spine-player-timeline")[0];
			this.timelineSlider = new Slider(timeline);
			let playButton = findWithId(parent, "spine-player-button-play-pause")[0];
			let animationButton = findWithId(parent, "spine-player-button-animation")[0];
			let skinButton = findWithId(parent, "spine-player-button-skin")[0];
			let settingsButton = findWithId(parent, "spine-player-button-settings")[0];
			let fullscreenButton = findWithId(parent, "spine-player-button-fullscreen")[0];
		}

		drawFrame () {
			requestAnimationFrame(() => this.drawFrame());
			let ctx = this.context;
			let gl = ctx.gl;
			this.time.update();

			// Clear the viewport
			let bg = new Color().setFromString(this.config.backgroundColor);
			gl.clearColor(bg.r, bg.g, bg.b, bg.a);
			gl.clear(gl.COLOR_BUFFER_BIT);

			// Display loading screen
			this.loadingScreen.draw(this.assetManager.isLoadingComplete());

			// Have we finished loading the asset? Then set things up
			if (this.assetManager.isLoadingComplete() && this.skeleton == null) this.loadSkeleton();

			// Resize the canvas
			this.sceneRenderer.resize(webgl.ResizeMode.Expand);

			// Update and draw the skeleton
			if (this.loaded) {
				this.skeleton.x = this.config.x;
				this.skeleton.y = this.config.y;
				this.skeleton.scaleX = this.skeleton.scaleY = this.config.scale;

				if (!this.paused && this.currentAnimation) {
					this.animationState.update(this.time.delta);
					this.animationState.apply(this.skeleton);
					this.skeleton.updateWorldTransform();
					let animation = this.skeleton.data.findAnimation(this.currentAnimation);
					let animationTime = this.animationState.getCurrent(0).trackTime % animation.duration;
					let percentage = animationTime / animation.duration;
					this.timelineSlider.setValue(percentage);
				}

				this.sceneRenderer.camera.position.x = 0;
				this.sceneRenderer.camera.position.y = 0;
				this.sceneRenderer.begin();
				this.sceneRenderer.line(0, 0, 200, 0, Color.RED);
				this.sceneRenderer.line(0, 0, 0, 200, Color.GREEN);
				this.sceneRenderer.drawSkeleton(this.skeleton, this.config.premultipliedAlpha);
				this.sceneRenderer.end();
			}
		}

		loadSkeleton () {
			if (this.loaded) return;

			let atlas = this.assetManager.get(this.config.atlasUrl);
			let jsonText = this.assetManager.get(this.config.jsonUrl);
			let json = new SkeletonJson(new AtlasAttachmentLoader(atlas));
			let skeletonData = json.readSkeletonData(jsonText);
			this.skeleton = new Skeleton(skeletonData);
			this.animationState = new AnimationState(new AnimationStateData(skeletonData));

			// Setup the first animation
			if (this.config.animation) {
				this.currentAnimation = this.config.animation;
			} else {
				if (skeletonData.animations.length > 0) {
					this.currentAnimation = skeletonData.animations[0].name;
				}
			}
			if(this.currentAnimation) {
				this.animationState.setAnimation(0, this.currentAnimation, true);
				this.paused = false;
				this.timelineSlider.change = (percentage) => {
					this.paused = true;
				}
			}

			this.loaded = true;
		}

		private resize () {
			let canvas = this.canvas;
			let w = canvas.clientWidth;
			let h = canvas.clientHeight;

			var devicePixelRatio = window.devicePixelRatio || 1;
			if (canvas.width != Math.floor(w * devicePixelRatio) || canvas.height != Math.floor(h * devicePixelRatio)) {
				canvas.width = Math.floor(w * devicePixelRatio);
				canvas.height = Math.floor(h * devicePixelRatio);
			}
			this.context.gl.viewport(0, 0, canvas.width, canvas.height);
			this.sceneRenderer.camera.setViewport(canvas.width, canvas.height);
		}
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
 }