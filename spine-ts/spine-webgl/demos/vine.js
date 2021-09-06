var vineDemo = function (canvas, bgColor) {
	var COLOR_INNER = new spine.Color(0.8, 0, 0, 0.5);
	var COLOR_OUTER = new spine.Color(0.8, 0, 0, 0.8);
	var COLOR_INNER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.5);
	var COLOR_OUTER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.8);

	var canvas, gl, renderer, input, assetManager;
	var skeleton, state, bounds;
	var timeKeeper;
	var target = null;
	var hoverTargets = [null, null, null, null, null, null];
	var controlBones = ["base", "vine-control1", "vine-control2", "vine-control3", "vine-control4"];
	var coords = new spine.Vector3(), temp = new spine.Vector3(), temp2 = new spine.Vector2();
	var playButton, timeLine, isPlaying = true, playTime = 0;

	if (!bgColor) bgColor = new spine.Color(235 / 255, 239 / 255, 244 / 255, 1);

	function init() {
		gl = canvas.context.gl;
		renderer = new spine.SceneRenderer(canvas, gl);
		input = new spine.Input(canvas);
		assetManager = new spine.AssetManager(gl, spineDemos.path, spineDemos.downloader);
		assetManager.loadTextureAtlas("atlas2.atlas");
		assetManager.loadJson("demos.json");
		timeKeeper = new spine.TimeKeeper();
	}

	function loadingComplete() {
		var atlasLoader = new spine.AtlasAttachmentLoader(assetManager.get("atlas2.atlas"));
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get("demos.json").vine);
		skeleton = new spine.Skeleton(skeletonData);
		skeleton.setToSetupPose();
		skeleton.updateWorldTransform();
		var offset = new spine.Vector2();
		bounds = new spine.Vector2();
		skeleton.getBounds(offset, bounds, []);
		state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
		state.setAnimation(0, "animation", true);
		state.apply(skeleton);
		skeleton.updateWorldTransform();

		renderer.camera.position.x = offset.x + bounds.x / 2;
		renderer.camera.position.y = offset.y + bounds.y / 2;

		renderer.skeletonDebugRenderer.drawMeshHull = false;
		renderer.skeletonDebugRenderer.drawMeshTriangles = false;

		setupUI();
		setupInput();
	}

	function setupUI() {
		playButton = $("#vine-playbutton");
		var playButtonUpdate = function () {
			isPlaying = !isPlaying;
			if (isPlaying)
				playButton.addClass("pause").removeClass("play");
			else
				playButton.addClass("play").removeClass("pause");
		}
		playButton.click(playButtonUpdate);
		playButton.addClass("pause");

		timeLine = $("#vine-timeline").data("slider");
		timeLine.changed = function (percent) {
			if (isPlaying) playButton.click();
			if (!isPlaying) {
				var animationDuration = state.getCurrent(0).animation.duration;
				time = animationDuration * percent;
				state.update(time - playTime);
				state.apply(skeleton);
				skeleton.updateWorldTransform();
				playTime = time;
			}
		};

		renderer.skeletonDebugRenderer.drawPaths = false;
		renderer.skeletonDebugRenderer.drawBones = false;
		var checkbox = $("#vine-drawbones");
		checkbox.change(function () {
			renderer.skeletonDebugRenderer.drawPaths = this.checked;
			renderer.skeletonDebugRenderer.drawBones = this.checked;
		});
	}

	function setupInput() {
		input.addListener({
			down: function (x, y) {
				target = spineDemos.closest(canvas, renderer, skeleton, controlBones, hoverTargets, x, y);
			},
			up: function (x, y) {
				target = null;
			},
			dragged: function (x, y) {
				spineDemos.dragged(canvas, renderer, target, x, y);
			},
			moved: function (x, y) {
				spineDemos.closest(canvas, renderer, skeleton, controlBones, hoverTargets, x, y);
			}
		});
	}

	function render() {
		timeKeeper.update();
		var delta = timeKeeper.delta;

		if (isPlaying) {
			var animationDuration = state.getCurrent(0).animation.duration;
			playTime += delta;
			while (playTime >= animationDuration) {
				playTime -= animationDuration;
			}
			timeLine.set(playTime / animationDuration);

			state.update(delta);
			state.apply(skeleton);
		}

		skeleton.updateWorldTransform();

		renderer.camera.viewportWidth = bounds.x * 1.2;
		renderer.camera.viewportHeight = bounds.y * 1.2;
		renderer.resize(spine.ResizeMode.Fit);

		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);

		renderer.begin();
		renderer.drawSkeleton(skeleton, true);
		renderer.drawSkeletonDebug(skeleton);
		gl.lineWidth(2);
		for (var i = 0; i < controlBones.length; i++) {
			var bone = skeleton.findBone(controlBones[i]);
			var colorInner = hoverTargets[i] !== null ? spineDemos.HOVER_COLOR_INNER : spineDemos.NON_HOVER_COLOR_INNER;
			var colorOuter = hoverTargets[i] !== null ? spineDemos.HOVER_COLOR_OUTER : spineDemos.NON_HOVER_COLOR_OUTER;
			renderer.circle(true, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorInner);
			renderer.circle(false, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorOuter);
		}
		gl.lineWidth(1);
		renderer.end();
	}

	init();
	vineDemo.assetManager = assetManager;
	vineDemo.loadingComplete = loadingComplete;
	vineDemo.render = render;
};