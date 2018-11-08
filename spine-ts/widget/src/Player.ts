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
		/* the URL of the skeleton .json file */
		jsonUrl: string

		/* the URL of the skeleton .atlas file. Atlas page images are automatically resolved. */
		atlasUrl: string

		/* Optional: the name of the animation to be played. Default: first animation in the skeleton. */
		animation: string

		/* Optional: list of animation names from which the user can choose. */
		animations: string[]

		/* Optional: the name of the skin to be set. Default: the default skin. */
		skin: string

		/* Optional: list of skin names from which the user can choose. */
		skins: string[]

		/* Optional: list of bone names that the user can control by dragging. */
		controlBones: string[]

		/* Optional: whether the skeleton uses premultiplied alpha. Default: false. */
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

		/* Optional: callback when the widget and its assets have been successfully loaded. */
		success: (widget: SpinePlayer) => void

		/* Optional: callback when the widget could not be loaded. */
		error: (widget: SpinePlayer, msg: string) => void
	}

	class Popup {
		public dom: HTMLElement;

		constructor(parent: HTMLElement, htmlContent: string) {
			this.dom = createElement(/*html*/`
				<div class="spine-player-popup spine-player-hidden">
				</div>
			`);
			this.dom.innerHTML = htmlContent;
			parent.appendChild(this.dom);
		}

		show () {
			this.dom.classList.remove("spine-player-hidden");
			var justClicked = true;
			let windowClickListener = (event: any) => {
				if (justClicked) {
					justClicked = false;
					return;
				}
				if (!isContained(this.dom, event.target)) {
					this.dom.parentNode.removeChild(this.dom);
					window.removeEventListener("click", windowClickListener);
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
		public change: (percentage: number) => void;

		render(): HTMLElement {
			this.slider = createElement(/*html*/`
				<div class="spine-player-slider">
					<div class="spine-player-slider-value"></div>
				</div>
			`);
			this.value = findWithClass(this.slider, "spine-player-slider-value")[0];
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
			return this.slider;
		}

		setValue(percentage: number) {
			percentage = Math.max(0, Math.min(1, percentage));
			this.value.style.width = "" + (percentage * 100) + "%";
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

		private selectedBones: Bone[];

		constructor(parent: HTMLElement, private config: SpinePlayerConfig) {
			parent.appendChild(this.render());
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
				regions: false,
				meshes: false,
				bounds: false,
				clipping: false,
				paths: false,
				points: false,
				hulls: false
			}
			if (!config.debug.bones) config.debug.bones = false;
			if (!config.debug.bounds) config.debug.bounds = false;
			if (!config.debug.clipping) config.debug.clipping = false;
			if (!config.debug.hulls) config.debug.hulls = false;
			if (!config.debug.paths) config.debug.paths = false;
			if (!config.debug.points) config.debug.points = false;
			if (!config.debug.regions) config.debug.regions = false;
			if (!config.debug.meshes) config.debug.meshes = false;

			if (config.animations && config.animation) {
				if  (config.animations.indexOf(config.animation) < 0) throw new Error("Default animation '" +  config.animation + "' is not contained in the list of selectable animations " + escapeHtml(JSON.stringify(this.config.animations)) + ".");
			}

			if (config.skins && config.skin) {
				if  (config.skins.indexOf(config.skin) < 0) throw new Error("Default skin '" +  config.skin + "' is not contained in the list of selectable skins " + escapeHtml(JSON.stringify(this.config.skins)) + ".");
			}

			if (!config.controlBones) config.controlBones = [];

			if (!config.showControls) config.showControls = true;

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
					<div class="spine-player-controls spine-player-popup-parent">
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
					</div>
				</div>
			`)

			try {
				// Validate the configuration
				this.config = this.validateConfig(config);
			}  catch (e) {
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
			this.assetManager.loadText(config.jsonUrl);
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
			let animationButton = findWithId(dom, "spine-player-button-animation")[0];
			let skinButton = findWithId(dom, "spine-player-button-skin")[0];
			let settingsButton = findWithId(dom, "spine-player-button-settings")[0];
			let fullscreenButton = findWithId(dom, "spine-player-button-fullscreen")[0];

			this.playButton.onclick = () => {
				if (this.paused) this.play()
				else this.pause();
			}

			speedButton.onclick = () => {
				this.showSpeedDialog();
			}

			animationButton.onclick = () => {
				this.showAnimationsDialog();
			}

			skinButton.onclick = () => {
				this.showSkinsDialog();
			}

			settingsButton.onclick = () => {
				this.showSettingsDialog();
			}

			fullscreenButton.onclick = () => {
				let doc = document as any;
				if(doc.fullscreenElement || doc.webkitFullscreenElement || doc.mozFullScreenElement || doc.msFullscreenElement) {
					if (doc.exitFullscreen) doc.exitFullscreen();
					else if (doc.mozCancelFullScreen) doc.mozCancelFullScreen();
					else if (doc.webkitExitFullscreen) doc.webkitExitFullscreen()
					else if (doc.msExitFullscreen) doc.msExitFullscreen();
				} else {
					let player = dom as any;
					if (player.requestFullscreen) player.requestFullscreen();
					else if (player.webkitRequestFullScreen) player.webkitRequestFullScreen();
					else if (player.mozRequestFullScreen) player.mozRequestFullScreen();
					else if (player.msRequestFullscreen) player.msRequestFullscreen();
				}
			};

			// Register a global resize handler to redraw and avoid flicker
			window.onresize = () => {
				this.drawFrame(false);
			}

			if (!config.showControls) findWithClass(dom, "spine-player-controls ")[0].classList.add("spine-player-hidden");

			return dom;
		}

		showSpeedDialog () {
			let popup = new Popup(this.playerControls, /*html*/`
				<div class="spine-player-row" style="user-select: none; align-items: center; padding: 8px;">
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
			`);
			let sliderParent = findWithClass(popup.dom, "spine-player-speed-slider")[0];
			let slider = new Slider();
			sliderParent.appendChild(slider.render());
			slider.setValue(this.speed / 2);
			slider.change = (percentage) => {
				this.speed = percentage * 2;
			}
			popup.show();
		}

		showAnimationsDialog () {
			if (!this.skeleton || this.skeleton.data.animations.length == 0) return;

			let popup = new Popup(this.playerControls, /*html*/`
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
					this.animationState.setAnimation(0, this.config.animation, true);
				}
			});
			popup.show();
		}

		showSkinsDialog () {
			if (!this.skeleton || this.skeleton.data.animations.length == 0) return;

			let popup = new Popup(this.playerControls, /*html*/`
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

			popup.show();
		}

		showSettingsDialog () {
			if (!this.skeleton || this.skeleton.data.animations.length == 0) return;

			let popup = new Popup(this.playerControls, /*html*/`
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

			makeItem("Show bones", "bones");
			makeItem("Show regions", "regions");
			makeItem("Show meshes", "meshes");
			makeItem("Show bounds", "bounds");
			makeItem("Show paths", "paths");
			makeItem("Show clipping", "clipping");
			makeItem("Show points", "points");
			makeItem("Show hulls", "hulls");

			popup.show();
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
					this.skeleton.updateWorldTransform();
				}

				let viewportSize = this.scale(this.config.viewport.width, this.config.viewport.height, this.canvas.width, this.canvas.height);

				this.sceneRenderer.camera.zoom = this.config.viewport.width / viewportSize.x;
				this.sceneRenderer.camera.position.x = this.config.viewport.x + this.config.viewport.width / 2;
				this.sceneRenderer.camera.position.y = this.config.viewport.y + this.config.viewport.height / 2;

				this.sceneRenderer.begin();

				// Draw background image if given
				if (this.config.backgroundImage && this.config.backgroundImage.url) {
					let bgImage = this.assetManager.get(this.config.backgroundImage.url);
					if (!this.config.backgroundImage.x) {
						this.sceneRenderer.drawTexture(bgImage, this.config.viewport.x, this.config.viewport.y, this.config.viewport.width, this.config.viewport.height);
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
			let jsonText = this.assetManager.get(this.config.jsonUrl);
			let json = new SkeletonJson(new AtlasAttachmentLoader(atlas));
			let skeletonData: SkeletonData;
			try {
				skeletonData = json.readSkeletonData(jsonText);
			} catch (e) {
				this.showError("Error: could not load skeleton .json.<br><br>" + escapeHtml(JSON.stringify(e)));
				return;
			}
			this.skeleton = new Skeleton(skeletonData);
			let stateData = new AnimationStateData(skeletonData);
			stateData.defaultMix = 0.2;
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

			// Setup viewport after skin is set
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

			// Setup the animations after viewport, so default bounds don't get messed up.
			if (!this.config.animation) {
				if (skeletonData.animations.length > 0) {
					this.config.animation = skeletonData.animations[0].name;
				}
			}

			if (this.config.animations && this.config.animations.length > 0) {
				this.config.animations.forEach(animation => {
					if (!this.skeleton.data.findAnimation(animation)) {
						this.showError(`Error: animation '${animation}' in selectable animation list does not exist in skeleton.`);
						return;
					}
				});
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

			this.config.success(this);
			this.loaded = true;
		}

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
					target = null;
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