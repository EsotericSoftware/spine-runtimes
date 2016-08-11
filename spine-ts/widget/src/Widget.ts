module spine {	

	export class SpineWidget {		
		skeleton: Skeleton;
		state: AnimationState;
		gl: WebGLRenderingContext;
		canvas: HTMLCanvasElement;

		private _config: SpineWidgetConfig;
		private _assetManager: spine.webgl.AssetManager;
		private _shader: spine.webgl.Shader;
		private _batcher: spine.webgl.PolygonBatcher;
		private _mvp = new spine.webgl.Matrix4();
		private _skeletonRenderer: spine.webgl.SkeletonRenderer;		
		private _paused = false;
		private _lastFrameTime = Date.now() / 1000.0;
		private _backgroundColor = new Color();
		private _loaded = false;

		constructor (element: HTMLElement, config: SpineWidgetConfig) {
			if (!element) throw new Error ("Please provide a DOM element, e.g. document.getElementById('myelement')");
			if (!config) throw new Error ("Please provide a configuration, specifying at least the json file, atlas file and animation name");

			this.validateConfig(config);

			let canvas = this.canvas = document.createElement("canvas");
			element.appendChild(canvas);
			canvas.width = 640;
			canvas.height = 480;
			var webglConfig = { alpha: false };
			let gl = this.gl = <WebGLRenderingContext> (canvas.getContext("webgl", webglConfig) || canvas.getContext("experimental-webgl", webglConfig));	

			this._shader = spine.webgl.Shader.newColoredTextured(gl);
			this._batcher = new spine.webgl.PolygonBatcher(gl);
			this._mvp.ortho2d(0, 0, 639, 479);
			this._skeletonRenderer = new spine.webgl.SkeletonRenderer(gl);

			let assets = this._assetManager = new spine.webgl.AssetManager(gl);
			assets.loadText(config.atlas);
			assets.loadText(config.json);
			assets.loadTexture(config.atlas.replace(".atlas", ".png"));
			requestAnimationFrame(() => { this.load(); });
		}

		private validateConfig (config: SpineWidgetConfig) {
			if (!config.atlas) throw new Error("Please specify config.atlas");
			if (!config.json) throw new Error("Please specify config.json");
			if (!config.animationName) throw new Error("Please specify config.animationName");

			if (!config.scale) config.scale = 1.0;
			if (!config.skin) config.skin = "default";
			if (config.loop === undefined) config.loop = true;			
			if (!config.y) config.y = 20;
			if (!config.width) config.width = 640;
			if (!config.height) config.height = 480;
			if (config.fitToCanvas === undefined) config.fitToCanvas = false;
			if (!config.x) config.x = config.width / 2;
			if (!config.backgroundColor) config.backgroundColor = "#555555";
			if (!config.imagesPath) {
				let index = config.atlas.lastIndexOf("/");
				if (index != -1) {
					config.imagesPath = config.atlas.substr(0, index) + "/";
				} else {
					config.imagesPath = "";
				}
			}
			if (!config.premultipliedAlpha === undefined) config.premultipliedAlpha = false;
			this._backgroundColor.setFromString(config.backgroundColor);
			this._config = config;		
		}

		private load () {
			let assetManager = this._assetManager;
			let imagesPath = this._config.imagesPath;
			let config = this._config;
			if (assetManager.isLoadingComplete()) {
				if (assetManager.hasErrors()) {
					if (config.error) config.error(this, "Failed to load assets: " + JSON.stringify(assetManager.errors));
					else throw new Error("Failed to load assets: " + JSON.stringify(assetManager.errors));
				}

				let atlas = new spine.webgl.TextureAtlas(this._assetManager.get(this._config.atlas) as string, (path) => {
					return assetManager.get(imagesPath + path) as spine.webgl.Texture;
				});
				
				let atlasLoader = new spine.webgl.TextureAtlasAttachmentLoader(atlas);				
				var skeletonJson = new spine.SkeletonJson(atlasLoader);
				
				// Set the scale to apply during parsing, parse the file, and create a new skeleton.
				skeletonJson.scale = config.scale;
				var skeletonData = skeletonJson.readSkeletonData(assetManager.get(config.json) as string);
				var skeleton = this.skeleton = new spine.Skeleton(skeletonData);
				skeleton.x = config.x;
				skeleton.y = config.y;
				skeleton.setSkinByName(config.skin);

				var animationState = this.state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
				animationState.setAnimation(0, config.animationName, true);

				if (config.success) config.success(this);
				this._loaded = true;
				requestAnimationFrame(() => { this.render(); });
			} else
				requestAnimationFrame(() => { this.load(); });
		}

		public pause () {
			this._paused = true;
		}

		public play () {
			this._paused = false;
			requestAnimationFrame(() => { this.render(); });
		}

		public isPlaying () {
			return !this._paused;
		}

		public setAnimation (animationName: string) {
			if (!this._loaded) throw new Error("Widget isn't loaded yet");
			this.skeleton.setToSetupPose();
			this.state.setAnimation(0, animationName, this._config.loop);
		}

		private render () {			
			var now = Date.now() / 1000;
			var delta = now - this._lastFrameTime;
			if (delta > 0.1) delta = 0;
			this._lastFrameTime = now;

			let gl = this.gl;
			let color = this._backgroundColor;
			gl.clearColor(color.r, color.g, color.b, color.a);
			gl.clear(gl.COLOR_BUFFER_BIT);

			// Apply the animation state based on the delta time.
			var state = this.state;
			var skeleton = this.skeleton;
			var premultipliedAlpha = this._config.premultipliedAlpha;
			state.update(delta);
			state.apply(skeleton);
			skeleton.updateWorldTransform();
			
			// Bind the shader and set the texture and model-view-projection matrix.
			let shader = this._shader;
			shader.bind();
			shader.setUniformi(spine.webgl.Shader.SAMPLER, 0);
			shader.setUniform4x4f(spine.webgl.Shader.MVP_MATRIX, this._mvp.values);

			// Start the batch and tell the SkeletonRenderer to render the active skeleton.
			let batcher = this._batcher;
			let skeletonRenderer = this._skeletonRenderer;
			batcher.begin(shader);
			skeletonRenderer.premultipliedAlpha = premultipliedAlpha;
			skeletonRenderer.draw(batcher, skeleton);
			batcher.end();
				
			shader.unbind();

			if (!this._paused) requestAnimationFrame(() => { this.render(); });
		}
	}

	export class SpineWidgetConfig {
		json: string;
		atlas: string;
		animationName: string;
		imagesPath: string;
		scale = 1.0;
		skin = "default";		
		loop = true;
		x = 0;
		y = 0;
		width = 640;
		height = 480;
		fitToCanvas = false;
		backgroundColor = "#555555";
		premultipliedAlpha = false;		
		success: (widget: SpineWidget) => void;
		error: (widget: SpineWidget, msg: string) => void;		
	}
}