var animationMixingDemo = function(pathPrefix) {
	var OUTLINE_COLOR = new spine.Color(0, 0.8, 0, 1);	

	var canvas, gl, renderer, input, assetManager;
	var skeleton, skeletonNoMix, state, stateNoMix, bounds;
	var timeSlider;
	var lastFrameTime = Date.now() / 1000

	function init () {
		timeSlider = document.getElementById("animationmixingdemo-timeslider");
		canvas = document.getElementById("animationmixingdemo-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = new spine.webgl.AssetManager(gl, pathPrefix);		
		assetManager.loadTexture("assets/spineboy.png");
		assetManager.loadText("assets/spineboy.json");
		assetManager.loadText("assets/spineboy.atlas");		
		requestAnimationFrame(load);

		input = new spine.webgl.Input(canvas);
		input.addListener({
			down: function(x, y) {
				state.setAnimation(1, "shoot", false);
				stateNoMix.setAnimation(1, "shoot", false);	
			},
			up: function(x, y) { },
			moved: function(x, y) {	}
		});
	}	

	function load () {
		if (assetManager.isLoadingComplete()) {
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
			requestAnimationFrame(render);			
		} else requestAnimationFrame(load);
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
		var atlas = new spine.TextureAtlas(assetManager.get("assets/" + name + ".atlas"), function(path) {
			return assetManager.get("assets/" + path);		
		});
		var atlasLoader = new spine.TextureAtlasAttachmentLoader(atlas);
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get("assets/" + name + ".json"));
		var skeleton = new spine.Skeleton(skeletonData);
		skeleton.setSkinByName("default");
		return skeleton;
	}

	function render () {
		var now = Date.now() / 1000;
		var delta = now - lastFrameTime;
		lastFrameTime = now;
		if (delta > 0.032) delta = 0.032;
		delta *= (timeSlider.value / 100);		

		var offset = bounds.offset;
		var size = bounds.size;

		renderer.camera.position.x = offset.x + size.x -  50;
		renderer.camera.position.y = offset.y + size.y / 2;
		renderer.camera.viewportWidth = size.x * 2.4;
		renderer.camera.viewportHeight = size.y * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(0.2, 0.2, 0.2, 1);
		gl.clear(gl.COLOR_BUFFER_BIT);		

		renderer.begin();
		state.update(delta);
		state.apply(skeleton);
		skeleton.updateWorldTransform();
		skeleton.x = 0;		
		renderer.drawSkeleton(skeleton);

		stateNoMix.update(delta);
		stateNoMix.apply(skeletonNoMix);
		skeletonNoMix.updateWorldTransform();
		skeletonNoMix.x = size.x;	
		renderer.drawSkeleton(skeletonNoMix);
		renderer.end();

		requestAnimationFrame(render);
	}

	init();
};