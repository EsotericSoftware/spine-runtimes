var spritesheetsDemo = function(canvas, bgColor) {
	var SKELETON_ATLAS_COLOR = new spine.Color(0, 0.8, 0, 0.8);
	var FRAME_ATLAS_COLOR = new spine.Color(0.8, 0, 0, 0.8);

	var canvas, gl, renderer, input, assetManager;
	var skeleton, animationState, offset, bounds;
	var skeletonSeq, walkAnim, walkLastTime = 0, walkLastTimePrecise = 0;
	var skeletonAtlas;
	var viewportWidth, viewportHeight;
	var frames = [], currFrame = 0, frameTime = 0, frameScale = 0, FPS = 30;
	var timeKeeper, loadingScreen, input;
	var playTime = 0, framePlaytime = 0, clickAnim = 0;

	var DEMO_NAME = "SpritesheetsDemo";

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
		input = new spine.webgl.Input(canvas);
	}

	function loadingComplete () {
		skeletonAtlas = new spine.TextureAtlas(assetManager.get(DEMO_NAME, "atlas1.atlas"), function(path) {
			return assetManager.get(DEMO_NAME, path);
		});
		var atlasLoader = new spine.AtlasAttachmentLoader(skeletonAtlas);
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get(DEMO_NAME, "demos.json").raptor);
		skeleton = new spine.Skeleton(skeletonData);
		var stateData = new spine.AnimationStateData(skeleton.data);
		stateData.defaultMix = 0.5;
		stateData.setMix("jump", "walk", 0.3);
		animationState = new spine.AnimationState(stateData);
		animationState.setAnimation(0, "walk", true);
		animationState.apply(skeleton);
		skeleton.updateWorldTransform();
		offset = new spine.Vector2();
		bounds = new spine.Vector2();
		skeleton.getBounds(offset, bounds, []);
		skeleton.x -= 60;

		skeletonSeq = new spine.Skeleton(skeletonData);
		walkAnim = skeletonSeq.data.findAnimation("walk");
		walkAnim.apply(skeletonSeq, 0, 0, true, null, 1, true, false);
		skeletonSeq.x += bounds.x + 150;

		viewportWidth = ((700 + bounds.x) - offset.x);
		viewportHeight = ((0 + bounds.y) - offset.y);
		resize();
		setupUI();
		setupInput();

		$("#spritesheets-overlay").removeClass("overlay-hide");
		$("#spritesheets-overlay").addClass("overlay");
	}

	function setupUI () {
		timeSlider = $("#spritesheets-timeslider").data("slider");
		timeSlider.set(0.5);
		timeSliderLabel = $("#spritesheets-timeslider-label")[0];
	}

	function setupInput () {
		input.addListener({
			down: function(x, y) {
				setAnimation((clickAnim++ % 2 == 0) ? "roar" : "jump");
			},
			up: function(x, y) { },
			moved: function(x, y) {	},
			dragged: function(x, y) { }
		});
		$("#spritesheets-roar").click(function () {
			setAnimation("roar");
		});
		$("#spritesheets-jump").click(function () {
			setAnimation("jump");
		});
	}

	function setAnimation (name) {
		animationState.setAnimation(0, name, false);
		animationState.addAnimation(0, "walk", true, 0);
	}

	function resize () {
		renderer.camera.position.x = offset.x + viewportWidth / 2 - 25;
		renderer.camera.position.y = offset.y + viewportHeight / 2 - 100;
		renderer.camera.viewportWidth = viewportWidth * 1.2;
		renderer.camera.viewportHeight = viewportHeight * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);
	}

	function render () {
		timeKeeper.update();
		var delta = timeKeeper.delta;

		resize();

		delta *= timeSlider.get();
		if (timeSliderLabel) {
			var oldValue = timeSliderLabel.textContent;
			var newValue = Math.round(timeSlider.get() * 100) + "%";
			if (oldValue !== newValue) timeSliderLabel.textContent = newValue;
		}

		var animationDuration = animationState.getCurrent(0).animation.duration;
		playTime += delta;
		while (playTime >= animationDuration) {
			playTime -= animationDuration;
		}

		walkLastTimePrecise += delta;
		while (walkLastTimePrecise - walkLastTime > 1 / FPS) {
			var newWalkTime = walkLastTime + 1 / FPS;
			walkAnim.apply(skeletonSeq, walkLastTime, newWalkTime, true, null, 1, spine.MixBlend.setup, spine.MixDirection.mixIn);
			walkLastTime = newWalkTime;
		}
		skeletonSeq.updateWorldTransform();

		animationState.update(delta);
		var current = animationState.getCurrent(0);
		if (current.animation.name == "walk") current.trackTime = walkLastTimePrecise;
		animationState.apply(skeleton);
		skeleton.updateWorldTransform();

		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);

		renderer.begin();
		var frame = frames[currFrame];
		renderer.drawSkeleton(skeleton, true);
		renderer.drawSkeleton(skeletonSeq, true);
		renderer.end();
	}

	spritesheetsDemo.loadingComplete = loadingComplete;
	spritesheetsDemo.render = render;
	spritesheetsDemo.DEMO_NAME = DEMO_NAME;
	init();
};