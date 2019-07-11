var clippingDemo = function(canvas, bgColor) {
	var gl, renderer, assetManager;
	var skeleton, state, bounds;
	var timeKeeper;
	var playButton, timeline, isPlaying = true, playTime = 0;

	var DEMO_NAME = "ClippingDemo";

	if (!bgColor) bgColor = new spine.Color(235 / 255, 239 / 255, 244 / 255, 1);

	function init () {
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.ctx.gl;
		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = spineDemos.assetManager;
		var textureLoader = function(img) { return new spine.webgl.GLTexture(gl, img); };
		assetManager.loadTexture(DEMO_NAME, textureLoader, "atlas1.png");
		assetManager.loadTexture(DEMO_NAME, textureLoader, "atlas12.png");
		assetManager.loadText(DEMO_NAME, "atlas1.atlas");
		assetManager.loadJson(DEMO_NAME, "demos.json");
		timeKeeper = new spine.TimeKeeper();
	}

	function loadingComplete () {
		var atlas = new spine.TextureAtlas(assetManager.get(DEMO_NAME, "atlas1.atlas"), function(path) {
			return assetManager.get(DEMO_NAME, path);
		});
		var atlasLoader = new spine.AtlasAttachmentLoader(atlas);
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get(DEMO_NAME, "demos.json")["spineboy"]);
		skeleton = new spine.Skeleton(skeletonData);
		state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
		state.setAnimation(0, "portal", true);
		state.apply(skeleton);
		skeleton.updateWorldTransform();
		var offset = new spine.Vector2();
		bounds = new spine.Vector2();
		skeleton.getBounds(offset, bounds, []);

		renderer.camera.position.x = offset.x + bounds.x + 200;
		renderer.camera.position.y = offset.y + bounds.y / 2 + 100;

		renderer.skeletonDebugRenderer.drawMeshHull = false;
		renderer.skeletonDebugRenderer.drawMeshTriangles = false;

		setupUI();
	}

	function setupUI() {
		playButton = $("#clipping-playbutton");
		var playButtonUpdate = function () {
			isPlaying = !isPlaying;
			if (isPlaying)
				playButton.addClass("pause").removeClass("play");
			else
				playButton.addClass("play").removeClass("pause");
		}
		playButton.click(playButtonUpdate);
		playButton.addClass("pause");

		timeline = $("#clipping-timeline").data("slider");
		timeline.changed = function (percent) {
			if (isPlaying) playButton.click();
			if (!isPlaying) {
				var animationDuration = state.getCurrent(0).animation.duration;
				var time = animationDuration * percent;
				state.update(time - playTime);
				state.apply(skeleton);
				skeleton.updateWorldTransform();
				playTime = time;
			}
		};

		renderer.skeletonDebugRenderer.drawRegionAttachments = false;
		renderer.skeletonDebugRenderer.drawClipping = false;
		renderer.skeletonDebugRenderer.drawBones = false;
		renderer.skeletonDebugRenderer.drawMeshHull = false;
		renderer.skeletonDebugRenderer.drawMeshTriangles = false;
		$("#clipping-drawtriangles").click(function() {
			renderer.skeletonDebugRenderer.drawMeshHull = this.checked;
			renderer.skeletonDebugRenderer.drawMeshTriangles = this.checked;
			renderer.skeletonDebugRenderer.drawClipping = this.checked;
			renderer.skeletonDebugRenderer.drawRegionAttachments = this.checked;
		})
	}

	function render () {
		timeKeeper.update();
		var delta = timeKeeper.delta;

		if (isPlaying) {
			var animationDuration = state.getCurrent(0).animation.duration;
			playTime += delta;
			while (playTime >= animationDuration)
				playTime -= animationDuration;
			timeline.set(playTime / animationDuration);

			state.update(delta);
			state.apply(skeleton);
			skeleton.updateWorldTransform();
		}

		renderer.camera.viewportWidth = bounds.x * 1.6;
		renderer.camera.viewportHeight = bounds.y * 1.6;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);

		renderer.begin();
		renderer.drawSkeleton(skeleton, true);
		renderer.drawSkeletonDebug(skeleton, false, ["root"]);
		renderer.end();
	}

	clippingDemo.loadingComplete = loadingComplete;
	clippingDemo.render = render;
	clippingDemo.DEMO_NAME = DEMO_NAME;
	init();
};