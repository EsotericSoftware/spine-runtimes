var spritesheetDemo = function(loadingComplete, bgColor) {
	var SKELETON_ATLAS_COLOR = new spine.Color(0, 0.8, 0, 0.8);
	var FRAME_ATLAS_COLOR = new spine.Color(0.8, 0, 0, 0.8);

	var canvas, gl, renderer, input, assetManager;
	var skeleton, animationState, offset, bounds;
	var skeletonSeq, walkAnim, walkLastTime = 0, walkLastTimePrecise = 0;
	var skeletonAtlas;	
	var viewportWidth, viewportHeight;
	var frames = [], currFrame = 0, frameTime = 0, frameScale = 0, FPS = 30;
	var timeKeeper, loadingScreen, input;
	var playTime = 0, framePlaytime = 0;

	var DEMO_NAME = "SpritesheetDemo";

	if (!bgColor) bgColor = new spine.Color(0, 0, 0, 1);

	function init () {
		canvas = document.getElementById("spritesheetdemo-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;	
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = spineDemos.assetManager;
		var textureLoader = function(img) { return new spine.webgl.GLTexture(gl, img); };		
		assetManager.loadTexture(DEMO_NAME, textureLoader, "atlas1.png");
		assetManager.loadText(DEMO_NAME, "atlas1.atlas");
		assetManager.loadJson(DEMO_NAME, "demos.json");	
		timeKeeper = new spine.TimeKeeper();
		input = new spine.webgl.Input(canvas);	
		loadingScreen = new spine.webgl.LoadingScreen(renderer);
		loadingScreen.backgroundColor = bgColor;
		requestAnimationFrame(load);
	}

	function load () {
		timeKeeper.update();
		if (assetManager.isLoadingComplete(DEMO_NAME)) {
			skeletonAtlas = new spine.TextureAtlas(assetManager.get(DEMO_NAME, "atlas1.atlas"), function(path) {
				return assetManager.get(DEMO_NAME, path);		
			});			
			var atlasLoader = new spine.TextureAtlasAttachmentLoader(skeletonAtlas);
			var skeletonJson = new spine.SkeletonJson(atlasLoader);
			var skeletonData = skeletonJson.readSkeletonData(assetManager.get(DEMO_NAME, "demos.json").raptor);
			skeleton = new spine.Skeleton(skeletonData);
			var stateData = new spine.AnimationStateData(skeleton.data);
			stateData.setMix("walk", "Jump", 0.5);
			stateData.setMix("Jump", "walk", 0.5);
			animationState = new spine.AnimationState(stateData);
			animationState.setAnimation(0, "walk", true);
			animationState.apply(skeleton);
			skeleton.updateWorldTransform();
			offset = new spine.Vector2();
			bounds = new spine.Vector2();
			skeleton.getBounds(offset, bounds);

			skeletonSeq = new spine.Skeleton(skeletonData);
			walkAnim = skeletonSeq.data.findAnimation("walk");
			walkAnim.apply(skeletonSeq, 0, 0, true, null);
			skeletonSeq.x += bounds.x + 150;
			
			viewportWidth = ((700 + bounds.x) - offset.x);
			viewportHeight = ((0 + bounds.y) - offset.y);
			resize();					
			setupUI();
			setupInput();

			$("#spritesheetdemo-overlay").removeClass("overlay-hide");
			$("#spritesheetdemo-overlay").addClass("overlay");			
			loadingComplete(canvas, render);
		} else {
			loadingScreen.draw();
			requestAnimationFrame(load);
		}
	}	

	function setupUI() {
		timeSlider = $("#spritesheetdemo-timeslider").data("slider");
		timeSlider.set(0.5);
		timeSliderLabel = $("#spritesheetdemo-timeslider-label");		
	}

	function setupInput() {
		input.addListener({
			down: function(x, y) { 
				animationState.setAnimation(0, "Jump", false).listener = {
					event: function (trackIndex, event) {
					},
					complete: function (trackIndex, loopCount) {
						animationState.setAnimation(0, "walk", true);
					},
					start: function (trackIndex) {
					},
					end: function (trackIndex) {
					}
				}
			},
			up: function(x, y) { },
			moved: function(x, y) {	},
			dragged: function(x, y) { }
		});
	}

	function resize () {
		renderer.camera.position.x = offset.x + viewportWidth / 2 + 100;
		renderer.camera.position.y = offset.y + viewportHeight / 2  - 160;	
		renderer.camera.viewportWidth = viewportWidth * 1.2;
		renderer.camera.viewportHeight = viewportHeight * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);
	}

	function render () {
		timeKeeper.update();
		var delta = timeKeeper.delta;

		delta *= timeSlider.get();
		if (timeSliderLabel) timeSliderLabel.text(Math.round(timeSlider.get() * 100) + "%");	
				
		var animationDuration = animationState.getCurrent(0).animation.duration;
		playTime += delta;			
		while (playTime >= animationDuration) {
			playTime -= animationDuration;
		}			
						
		animationState.update(delta);
		animationState.apply(skeleton);
		skeleton.updateWorldTransform();

		walkLastTimePrecise += delta;				
		while (walkLastTimePrecise - walkLastTime > 1 / FPS) {
			var newWalkTime = walkLastTime + 1 / FPS;
			walkAnim.apply(skeletonSeq, walkLastTime, newWalkTime, true, null);
			walkLastTime = newWalkTime;
		}								
		skeletonSeq.updateWorldTransform();					

		
		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);	

		renderer.begin();		
		var frame = frames[currFrame];				
		renderer.drawSkeleton(skeleton, true);
		renderer.drawSkeleton(skeletonSeq, true);		
		renderer.end();		
	}

	init();
};