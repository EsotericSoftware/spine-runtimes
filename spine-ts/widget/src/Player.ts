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
		debug: {
			bones: boolean;
			regions: boolean;
			bounds: boolean;
			paths: boolean;
			points: boolean;
			clipping: boolean;
			meshHull: boolean;
			triangles: boolean;
		},
		viewport: {
			x: number,
			y: number,
			width: number,
			height: number
		}
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
					percentage = Math.max(0, Math.min(percentage, 1));
					this.setValue(x / this.slider.clientWidth);
					if (this.change) this.change(percentage);
				},
				moved: (x, y) => {
					if (dragging) {
						let percentage = x / this.slider.clientWidth;
						percentage = Math.max(0, Math.min(percentage, 1));
						this.setValue(x / this.slider.clientWidth);
						if (this.change) this.change(percentage);
					}
				},
				dragged: (x, y) => {
					let percentage = x / this.slider.clientWidth;
					percentage = Math.max(0, Math.min(percentage, 1));
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
		private playButton: HTMLElement;
		private loaded: boolean;
		private skeleton: Skeleton;
		private animationState: AnimationState;
		private time = new TimeKeeper();

		private paused = true;
		private playTime = 0;
		private speed = 1;

		constructor(parent: HTMLElement, private config: SpinePlayerConfig) {
			this.validateConfig(config);
			this.render(parent, config);
		}

		validateConfig(config: SpinePlayerConfig): SpinePlayerConfig {
			if (!config) throw new Error("Please pass a configuration to new.spine.SpinePlayer().");
			if (!config.jsonUrl) throw new Error("Please specify the URL of the skeleton JSON file.");
			if (!config.atlasUrl) throw new Error("Please specify the URL of the atlas file.");
			if (!config.alpha) config.alpha = false;
			if (!config.backgroundColor) config.backgroundColor = "#000000";
			if (!config.premultipliedAlpha) config.premultipliedAlpha = false;
			if (!config.success) config.success = (widget) => {};
			if (!config.error) config.error = (widget, msg) => {};
			if (!config.debug) config.debug = {
				bones: false,
				bounds: false,
				clipping: false,
				meshHull: false,
				paths: false,
				points: false,
				regions: false,
				triangles: false
			}
			if (!config.debug.bones) config.debug.bones = false;
			if (!config.debug.bounds) config.debug.bounds = false;
			if (!config.debug.clipping) config.debug.clipping = false;
			if (!config.debug.meshHull) config.debug.meshHull = false;
			if (!config.debug.paths) config.debug.paths = false;
			if (!config.debug.points) config.debug.points = false;
			if (!config.debug.regions) config.debug.regions = false;
			if (!config.debug.triangles) config.debug.triangles = false;
			return config;
		}

		render(parent: HTMLElement, config: SpinePlayerConfig) {
			parent.innerHTML = /*html*/`
				<div class="spine-player">
					<canvas class="spine-player-canvas"></canvas>
					<div class="spine-player-controls spine-player-dropdown">
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
						</div>

						<div class="spine-player-dropdown-content spine-player-hidden">
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
			this.playButton = findWithId(parent, "spine-player-button-play-pause")[0];
			let speedButton = findWithId(parent, "spine-player-button-speed")[0];
			let animationButton = findWithId(parent, "spine-player-button-animation")[0];
			let skinButton = findWithId(parent, "spine-player-button-skin")[0];
			let settingsButton = findWithId(parent, "spine-player-button-settings")[0];
			let fullscreenButton = findWithId(parent, "spine-player-button-fullscreen")[0];

			let dropdown = findWithClass(parent, "spine-player-dropdown-content")[0];

			var justClicked = false;
			let dismissDropdown = function (event: any) {
				if (justClicked) {
					justClicked = false;
					return;
				}
				if (!isContained(dropdown, event.target)) {
					dropdown.classList.add("spine-player-hidden");
					window.onclick = null;
				}
			}

			this.playButton.onclick = () => {
				if (this.paused) this.play()
				else this.pause();
			}

			speedButton.onclick = () => {
				dropdown.classList.remove("spine-player-hidden");
				dropdown.innerHTML = /*html*/`
					<div class="spine-player-row" style="user-select: none; align-items: center;">
						<div style="margin-right: 16px;">Speed</div>
						<div class="spine-player-column">
							<div class="spine-player-speed-slider" style="margin-bottom: 4px;"></div>
							<div class="spine-player-row" style="justify-content: space-between;">
								<div>0.1x</div>
								<div>1x</div>
								<div>2x</div>
							</div>
						</div>
					</div>
				`;
				let sliderParent = findWithClass(dropdown, "spine-player-speed-slider")[0];
				let slider = new Slider(sliderParent);
				slider.setValue(this.speed / 2);
				slider.change = (percentage) => {
					this.speed = percentage * 2;
				}
				justClicked = true;
				window.onclick = dismissDropdown;
			}

			animationButton.onclick = () => {
				if (!this.skeleton || this.skeleton.data.animations.length == 0) return;
				dropdown.classList.remove("spine-player-hidden");
				dropdown.innerHTML = /*html*/`
					<div>Animations</div>
					<hr>
					<div class="spine-player-list" style="user-select: none; align-items: center; max-height: 90px; overflow: auto;">
					</div>
				`;

				let rows = findWithClass(dropdown, "spine-player-list")[0];
				this.skeleton.data.animations.forEach((animation) => {
					let row = document.createElement("div");
					row.classList.add("spine-player-list-item");
					if (animation.name == this.config.animation) row.classList.add("spine-player-list-item-selected");
					row.innerText = animation.name;
					rows.appendChild(row);
					row.onclick = () => {
						removeClass(rows.children, "spine-player-list-item-selected");
						row.classList.add("spine-player-list-item-selected");
						this.config.animation = animation.name;
						this.playTime = 0;
						this.animationState.setAnimation(0, this.config.animation, true);
					}
				});

				justClicked = true;
				window.onclick = dismissDropdown;
			}

			skinButton.onclick = () => {
				if (!this.skeleton || this.skeleton.data.animations.length == 0) return;
				dropdown.classList.remove("spine-player-hidden");
				dropdown.innerHTML = /*html*/`
					<div>Skins</div>
					<hr>
					<div class="spine-player-list" style="user-select: none; align-items: center; max-height: 90px; overflow: auto;">
					</div>
				`;

				let rows = findWithClass(dropdown, "spine-player-list")[0];
				this.skeleton.data.skins.forEach((skin) => {
					let row = document.createElement("div");
					row.classList.add("spine-player-list-item");
					if (skin.name == this.config.skin) row.classList.add("spine-player-list-item-selected");
					row.innerText = skin.name;
					rows.appendChild(row);
					row.onclick = () => {
						removeClass(rows.children, "spine-player-list-item-selected");
						row.classList.add("spine-player-list-item-selected");
						this.config.skin = skin.name;
						this.skeleton.setSkinByName(this.config.skin);
						this.skeleton.setSlotsToSetupPose();
					}
				});

				justClicked = true;
				window.onclick = dismissDropdown;
			}

			settingsButton.onclick = () => {
				if (!this.skeleton || this.skeleton.data.animations.length == 0) return;
				dropdown.classList.remove("spine-player-hidden");
				dropdown.innerHTML = /*html*/`
					<div>Debug</div>
					<hr>
					<div class="spine-player-list" style="user-select: none; align-items: center; max-height: 90px; overflow: auto;">
					</div>
				`;

				let rows = findWithClass(dropdown, "spine-player-list")[0];
				let makeItem = (name: string) => {
					let row = document.createElement("div");
					row.classList.add("spine-player-list-item");
					if ((this.config.debug as any)[name] == true) row.classList.add("spine-player-list-item-selected");
					row.innerText = name
					rows.appendChild(row);
					row.onclick = () => {
						if ((this.config.debug as any)[name]) {
							(this.config.debug as any)[name] = false;
							row.classList.remove("spine-player-list-item-selected");
						} else {
							(this.config.debug as any)[name] = true;
							row.classList.add("spine-player-list-item-selected");
						}
					}
				};

				Object.keys(this.config.debug).forEach((name) => {
					makeItem(name);
				});

				justClicked = true;
				window.onclick = dismissDropdown;
			}

			// Register a global resize handler to redraw and avoid flicker
			window.onresize = () => {
				this.drawFrame(false);
			}
		}

		drawFrame (requestNextFrame = true) {
			if (requestNextFrame) requestAnimationFrame(() => this.drawFrame());
			let ctx = this.context;
			let gl = ctx.gl;

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
				if (!this.paused && this.config.animation) {
					this.time.update();
					let delta = this.time.delta * this.speed;

					let animationDuration = this.animationState.getCurrent(0).animation.duration;
					this.playTime += delta;
					while (this.playTime >= animationDuration) {
						this.playTime -= animationDuration;
					}
					this.playTime = Math.max(0, Math.min(this.playTime, animationDuration));
					this.timelineSlider.setValue(this.playTime / animationDuration);

					this.animationState.update(delta);
					this.animationState.apply(this.skeleton);
					this.skeleton.updateWorldTransform();
				}

				let viewportSize = this.scale(this.config.viewport.width, this.config.viewport.height, this.canvas.width, this.canvas.height);

				this.sceneRenderer.camera.zoom = this.config.viewport.width / viewportSize.x;
				this.sceneRenderer.camera.position.x = this.config.viewport.x + this.config.viewport.width / 2;
				this.sceneRenderer.camera.position.y = this.config.viewport.y + this.config.viewport.height / 2;

				this.sceneRenderer.begin();
				this.sceneRenderer.drawSkeleton(this.skeleton, this.config.premultipliedAlpha);
				this.sceneRenderer.skeletonDebugRenderer.drawBones = this.config.debug.bones;
				this.sceneRenderer.skeletonDebugRenderer.drawBoundingBoxes = this.config.debug.bounds;
				this.sceneRenderer.skeletonDebugRenderer.drawClipping = this.config.debug.clipping;
				this.sceneRenderer.skeletonDebugRenderer.drawMeshHull = this.config.debug.meshHull;
				this.sceneRenderer.skeletonDebugRenderer.drawPaths = this.config.debug.paths;
				this.sceneRenderer.skeletonDebugRenderer.drawRegionAttachments = this.config.debug.regions;
				this.sceneRenderer.skeletonDebugRenderer.drawMeshTriangles = this.config.debug.triangles;
				this.sceneRenderer.drawSkeletonDebug(this.skeleton, this.config.premultipliedAlpha);
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

			let atlas = this.assetManager.get(this.config.atlasUrl);
			let jsonText = this.assetManager.get(this.config.jsonUrl);
			let json = new SkeletonJson(new AtlasAttachmentLoader(atlas));
			let skeletonData = json.readSkeletonData(jsonText);
			this.skeleton = new Skeleton(skeletonData);
			let stateData = new AnimationStateData(skeletonData);
			stateData.defaultMix = 0.2;
			this.animationState = new AnimationState(stateData);

			// Setup skin
			if (!this.config.skin) {
				if (skeletonData.skins.length > 0) {
					this.config.skin = skeletonData.skins[0].name;
				}
			}
			if (this.config.skin) {
				this.skeleton.setSkinByName(this.config.skin);
				this.skeleton.setSlotsToSetupPose();
			}

			// Setup viewport
			if (!this.config.viewport || !this.config.viewport.x || !this.config.viewport.y || !this.config.viewport.width || !this.config.viewport.height) {
				this.config.viewport = {
					x: 0,
					y: 0,
					width: 0,
					height: 0
				}

				this.skeleton.updateWorldTransform();
				let offset = new spine.Vector2();
				let size = new spine.Vector2();
				this.skeleton.getBounds(offset, size);
				this.config.viewport.x = offset.x + size.x / 2 - size.x / 2 * 1.2;
				this.config.viewport.y = offset.y + size.y / 2 - size.y / 2 * 1.2;
				this.config.viewport.width = size.x * 1.2;
				this.config.viewport.height = size.y * 1.2;
			}

			// Setup the first animation
			if (!this.config.animation) {
				if (skeletonData.animations.length > 0) {
					this.config.animation = skeletonData.animations[0].name;
				}
			}
			if(this.config.animation) {
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

			this.loaded = true;
		}

		private play () {
			this.paused = false;
			this.playButton.classList.remove("spine-player-button-icon-play");
			this.playButton.classList.add("spine-player-button-icon-pause");

			if (this.config.animation) {
				if (!this.animationState.getCurrent(0)) {
					this.animationState.setAnimation(0, this.config.animation, true);
				}
			}
		}

		private pause () {
			this.paused = true;
			this.playButton.classList.remove("spine-player-button-icon-pause");
			this.playButton.classList.add("spine-player-button-icon-play");
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

	function removeClass(elements: HTMLCollection, clazz: string) {
		for (var i = 0; i < elements.length; i++) {
			elements[i].classList.remove(clazz);
		}
	}
 }