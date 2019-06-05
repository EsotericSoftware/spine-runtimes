var transitionsDemo = function(canvas, loadingComplete, bgColor) {
	var OUTLINE_COLOR = new spine.Color(0, 0.8, 0, 1);

	var canvas, gl, renderer, input, assetManager;
	var skeleton, skeletonNoMix, state, stateNoMix, bounds;
	var timeSlider, timeSliderLabel;
	var timeKeeper;

	var DEMO_NAME = "TransitionsDemo";

	if (!bgColor) bgColor = new spine.Color(235 / 255, 239 / 255, 244 / 255, 1);

	function init () {
		timeSlider = $("#transitions-timeslider").data("slider");
		timeSlider.set(0.5);
		timeSliderLabel = $("#transitions-timeslider-label")[0];

		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.ctx.gl;

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = spineDemos.assetManager;
		var textureLoader = function(img) { return new spine.webgl.GLTexture(gl, img); };

		assetManager.loadTexture(DEMO_NAME, textureLoader, "atlas1.png");
		assetManager.loadTexture(DEMO_NAME, textureLoader, "atlas12.png");
		assetManager.loadText(DEMO_NAME, "atlas1.atlas");
		assetManager.loadJson(DEMO_NAME, "demos.json");

		input = new spine.webgl.Input(canvas);
		timeKeeper = new spine.TimeKeeper();
	}

	function loadingComplete () {
		skeleton = loadSkeleton("spineboy");
		skeletonNoMix = new spine.Skeleton(skeleton.data);
		state = createState(0.25);
		state.multipleMixing = true;
		setAnimations(state, 0, 0);
		stateNoMix = createState(0);
		setAnimations(stateNoMix, -0.25, 0);

		state.apply(skeleton);
		skeleton.updateWorldTransform();
		bounds = { offset: new spine.Vector2(), size: new spine.Vector2() };
		skeleton.getBounds(bounds.offset, bounds.size, []);
		setupInput();
		$("#transitions-overlay").removeClass("overlay-hide");
		$("#transitions-overlay").addClass("overlay");
	}

	function setupInput() {
		$("#transitions-die").click(function () {
			var entry = state.setAnimation(0, "death", false);
			setAnimations(state, 0, true, 0);
			entry.next.mixDuration = 0.1;

			var entry = stateNoMix.setAnimation(0, "death", false);
			setAnimations(stateNoMix, -0.25, -0.25 + -0.1);
		});
	}

	function createState (mix) {
		var stateData = new spine.AnimationStateData(skeleton.data);
		stateData.defaultMix = mix;
		var state = new spine.AnimationState(stateData);
		return state;
	}

	function setAnimations (state, delay, first) {
		state.addAnimation(0, "idle", true, first);
		state.addAnimation(0, "walk", true, 0.6);
		state.addAnimation(0, "jump", false, 1);
		state.addAnimation(0, "run", true, delay);
		state.addAnimation(0, "walk", true, 1.2);
		state.addAnimation(0, "run", true, 0.5);
		state.addAnimation(0, "jump", false, 1);
		state.addAnimation(0, "run", true, delay);
		state.addAnimation(0, "jump", true, 0.5);
		state.addAnimation(0, "walk", true, delay).listener = {
			start: function (trackIndex) {
				setAnimations(state, delay, 0.6);
			}
		};
	}

	function loadSkeleton(name) {
		var atlas = new spine.TextureAtlas(assetManager.get(DEMO_NAME, "atlas1.atlas"), function(path) {
			return assetManager.get(DEMO_NAME, path);
		});
		var atlasLoader = new spine.AtlasAttachmentLoader(atlas);
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get(DEMO_NAME, "demos.json")[name]);
		var skeleton = new spine.Skeleton(skeletonData);
		skeleton.setSkinByName("default");
		return skeleton;
	}

	function render () {
		timeKeeper.update();
		var delta = timeKeeper.delta * timeSlider.get();
		if (timeSliderLabel) {
			var oldValue = timeSliderLabel.textContent;
			var newValue = Math.round(timeSlider.get() * 100) + "%";
			if (oldValue !== newValue) timeSliderLabel.textContent = newValue;
		}

		var offset = bounds.offset;
		var size = bounds.size;

		renderer.camera.position.x = offset.x + size.x - 50;
		renderer.camera.position.y = offset.y + size.y / 2 - 40;
		renderer.camera.viewportWidth = size.x * 2;
		renderer.camera.viewportHeight = size.y * 2;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);

		renderer.begin();
		state.update(delta);
		state.apply(skeleton);
		skeleton.updateWorldTransform();
		skeleton.x = -200;
		skeleton.y = -100;
		renderer.drawSkeleton(skeleton, true);

		stateNoMix.update(delta);
		stateNoMix.apply(skeletonNoMix);
		skeletonNoMix.updateWorldTransform();
		skeletonNoMix.x = size.x + 45;
		skeletonNoMix.y = -100;
		renderer.drawSkeleton(skeletonNoMix, true);
		renderer.end();
	}

	transitionsDemo.loadingComplete = loadingComplete;
	transitionsDemo.render = render;
	transitionsDemo.DEMO_NAME = DEMO_NAME;
	init();
};