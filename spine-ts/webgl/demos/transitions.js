var transitions = function(loadingComplete, bgColor) {
	var OUTLINE_COLOR = new spine.Color(0, 0.8, 0, 1);	

	var canvas, gl, renderer, input, assetManager;
	var skeleton, skeletonNoMix, state, stateNoMix, bounds;
	var timeSlider, timeSliderLabel;
	var timeKeeper;
	var loadingScreen;

	var DEMO_NAME = "TransitionsDemo";

	if (!bgColor) bgColor = new spine.Color(1, 1, 1, 1);

	function init () {
		timeSlider = $("#transitions-timeslider").data("slider");
		timeSlider.set(0.5);
		timeSliderLabel = $("#transitions-timeslider-label")[0];
		canvas = document.getElementById("transitions-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = spineDemos.assetManager;
		var textureLoader = function(img) { return new spine.webgl.GLTexture(gl, img); };
		
		assetManager.loadTexture(DEMO_NAME, textureLoader, "atlas1.png");		
		assetManager.loadText(DEMO_NAME, "atlas1.atlas");
		assetManager.loadJson(DEMO_NAME, "demos.json");
				
		input = new spine.webgl.Input(canvas);
		timeKeeper = new spine.TimeKeeper();		
		loadingScreen = new spine.webgl.LoadingScreen(renderer);
		loadingScreen.backgroundColor = bgColor;

		requestAnimationFrame(load);	
	}

	function load () {
		timeKeeper.update();
		if (assetManager.isLoadingComplete(DEMO_NAME)) {
			skeleton = loadSkeleton("spineboy");
			skeletonNoMix = new spine.Skeleton(skeleton.data);					
			state = createState(0.2);
			setAnimations(state, 0);
			stateNoMix = createState(0.0);
			setAnimations(stateNoMix, -0.2);
			
			state.apply(skeleton);
			skeleton.updateWorldTransform();
			bounds = { offset: new spine.Vector2(), size: new spine.Vector2() };
			skeleton.getBounds(bounds.offset, bounds.size);
			setupInput();
			$("#transitions-overlay").removeClass("overlay-hide");
			$("#transitions-overlay").addClass("overlay");	
			loadingComplete(canvas, render);						
		} else {
			loadingScreen.draw();			
			requestAnimationFrame(load);
		}
	}

	function setupInput() {
		input.addListener({
			down: function(x, y) { },
			up: function(x, y) { },
			moved: function(x, y) {	},
			dragged: function(x, y) { }
		});
	}

	function createState(mix) {
		var stateData = new spine.AnimationStateData(skeleton.data);
		stateData.setMix("walk", "jump", mix);
		stateData.setMix("jump", "walk", mix);
		stateData.setMix("walk", "idle", mix);
		stateData.setMix("idle", "walk", mix);			
		var state = new spine.AnimationState(stateData);		
		return state;
	}

	function setAnimations(state, delay) {
		state.addAnimation(0, "idle", false, delay);
		state.addAnimation(0, "walk", false, delay);		
		state.addAnimation(0, "idle", false, delay);
		state.addAnimation(0, "walk", false, delay);
		state.addAnimation(0, "walk", false, 0);
		state.addAnimation(0, "jump", false, delay);
		state.addAnimation(0, "walk", false, delay).listener = {
			event: function (trackIndex, event) {},
			complete: function (trackIndex, loopCount) {},
			start: function (trackIndex) { 
				setAnimations(state, delay);
			},
			end: function (trackIndex) {}
		};
	}

	function loadSkeleton(name) {
		var atlas = new spine.TextureAtlas(assetManager.get(DEMO_NAME, "atlas1.atlas"), function(path) {
			return assetManager.get(DEMO_NAME, path);		
		});
		var atlasLoader = new spine.TextureAtlasAttachmentLoader(atlas);
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

		renderer.camera.position.x = offset.x + size.x -  50;
		renderer.camera.position.y = offset.y + size.y / 2 - 50;
		renderer.camera.viewportWidth = size.x * 2.4;
		renderer.camera.viewportHeight = size.y * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);		

		renderer.begin();
		state.update(delta);
		state.apply(skeleton);
		skeleton.updateWorldTransform();
		skeleton.x = 0;		
		renderer.drawSkeleton(skeleton, true);

		stateNoMix.update(delta);
		stateNoMix.apply(skeletonNoMix);
		skeletonNoMix.updateWorldTransform();
		skeletonNoMix.x = size.x;	
		renderer.drawSkeleton(skeletonNoMix, true);
		renderer.end();		
	}
	init();
	return render;
};