var tankDemo = function (canvas, bgColor) {
	var canvas, gl, renderer, input, assetManager;
	var skeleton, state, offset, bounds;
	var timeKeeper;
	var playButton, timeLine, isPlaying = true, playTime = 0;

	if (!bgColor) bgColor = new spine.Color(235 / 255, 239 / 255, 244 / 255, 1);

	function init() {
		gl = canvas.context.gl;
		renderer = new spine.SceneRenderer(canvas, gl);
		assetManager = new spine.AssetManager(gl, spineDemos.path, spineDemos.downloader);
		assetManager.loadTextureAtlas("atlas2.atlas");
		assetManager.loadJson("demos.json");
		timeKeeper = new spine.TimeKeeper();
	}

	function loadingComplete() {
		var atlasLoader = new spine.AtlasAttachmentLoader(assetManager.get("atlas2.atlas"));
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get("demos.json").tank);
		skeleton = new spine.Skeleton(skeletonData);
		state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
		state.setAnimation(0, "drive", true);
		state.apply(skeleton);
		skeleton.updateWorldTransform();
		offset = new spine.Vector2();
		bounds = new spine.Vector2();
		offset.x = -1204.22;
		bounds.x = 1914.52;
		bounds.y = 965.78;
		// skeleton.getBounds(offset, bounds);

		renderer.skeletonDebugRenderer.drawRegionAttachments = false;
		renderer.skeletonDebugRenderer.drawMeshHull = false;
		renderer.skeletonDebugRenderer.drawMeshTriangles = false;

		setupUI();
	}

	function setupUI() {
		playButton = $("#tank-playbutton");
		var playButtonUpdate = function () {
			isPlaying = !isPlaying;
			if (isPlaying)
				playButton.addClass("pause").removeClass("play");
			else
				playButton.addClass("play").removeClass("pause");
		}
		playButton.click(playButtonUpdate);
		playButton.addClass("pause");

		timeLine = $("#tank-timeline").data("slider");
		timeLine.changed = function (percent) {
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

		renderer.skeletonDebugRenderer.drawPaths = false;
		renderer.skeletonDebugRenderer.drawBones = false;
		$("#tank-drawbones").change(function () {
			renderer.skeletonDebugRenderer.drawPaths = this.checked;
			renderer.skeletonDebugRenderer.drawBones = this.checked;
		});
	}

	function render() {
		timeKeeper.update();
		var delta = timeKeeper.delta;

		if (isPlaying) {
			var animationDuration = state.getCurrent(0).animation.duration;
			playTime += delta;
			while (playTime >= animationDuration)
				playTime -= animationDuration;
			timeLine.set(playTime / animationDuration);

			state.update(delta);
			state.apply(skeleton);
			skeleton.updateWorldTransform();
		}

		offset.x = skeleton.findBone("tankRoot").worldX;
		offset.y = skeleton.findBone("tankRoot").worldY;

		renderer.camera.position.x = offset.x - 300;
		renderer.camera.position.y = bounds.y - 505;
		renderer.camera.viewportWidth = bounds.x * 1.2;
		renderer.camera.viewportHeight = bounds.y * 1.2;
		renderer.resize(spine.ResizeMode.Fit);

		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);

		renderer.begin();
		renderer.drawSkeleton(skeleton, true);
		renderer.drawSkeletonDebug(skeleton, true);
		renderer.end();
	}

	init();
	tankDemo.assetManager = assetManager;
	tankDemo.loadingComplete = loadingComplete;
	tankDemo.render = render;
};