var spritesheetDemo = function(pathPrefix, loadingComplete) {
	var SKELETON_ATLAS_COLOR = new spine.Color(0, 0.8, 0, 0.8);
	var FRAME_ATLAS_COLOR = new spine.Color(0.8, 0, 0, 0.8);

	var canvas, gl, renderer, input, assetManager;
	var skeleton, animationState, offset, bounds;
	var skeletonSeq, walkAnim, walkLastTime = 0, walkLastTimePrecise = 0;
	var skeletonAtlas;	
	var viewportWidth, viewportHeight;
	var frames = [], currFrame = 0, frameTime = 0, frameScale = 0, FPS = 30;
	var lastFrameTime = Date.now() / 1000;
	var timeSlider, timeSliderLabel;
	var playButton, timeLine, isPlaying = true, playTime = 0, framePlaytime = 0;

	function init () {
		if (pathPrefix === undefined) pathPrefix = "";		

		canvas = document.getElementById("spritesheetdemo-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;	
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = new spine.webgl.AssetManager(gl, pathPrefix);		
		assetManager.loadTexture("raptor.png");
		assetManager.loadText("raptor.json");
		assetManager.loadText("raptor.atlas");		
		requestAnimationFrame(load);
	}

	function load () {
		if (assetManager.isLoadingComplete()) {
			skeletonAtlas = new spine.TextureAtlas(assetManager.get("raptor.atlas"), function(path) {
				return assetManager.get("" + path);		
			});			
			var atlasLoader = new spine.TextureAtlasAttachmentLoader(skeletonAtlas);
			var skeletonJson = new spine.SkeletonJson(atlasLoader);
			var skeletonData = skeletonJson.readSkeletonData(assetManager.get("raptor.json"));
			skeleton = new spine.Skeleton(skeletonData);
			animationState = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
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

			setupUI();
			loadingComplete(canvas, render);
		} else requestAnimationFrame(load);
	}

	function setupUI() {
		playButton = $("#spritesheetdemo-playbutton");
		var playButtonUpdate = function () {	
			isPlaying = !isPlaying;
			if (isPlaying) {
				playButton.val("Pause");
				playButton.addClass("pause").removeClass("play");		
			} else {
				playButton.val("Play");
				playButton.addClass("play").removeClass("pause");
			}		
		}
		playButton.click(playButtonUpdate);

		timeLine = $("#spritesheetdemo-timeline");
		timeLine.slider({ range: "max", min: 0, max: 100, value: 0, slide: function () {
			if (isPlaying) playButton.click();		
			if (!isPlaying) {				
				var time = timeLine.slider("value") / 100;
				var animationDuration = animationState.getCurrent(0).animation.duration;
				time = animationDuration * time;
				var playDelta = time - playTime;				
				animationState.update(playDelta);
				animationState.apply(skeleton);
				skeleton.updateWorldTransform();
				playTime = time;
								
				walkLastTimePrecise += playDelta;
				var sign = playDelta < 0 ? -1 : 1;	
				while (Math.abs(walkLastTimePrecise - walkLastTime) > 1 / FPS) {
					var newWalkTime = walkLastTime + sign * 1 / FPS;
					walkAnim.apply(skeletonSeq, walkLastTime, newWalkTime, true, null);
					walkLastTime = newWalkTime;
				}
				skeletonSeq.updateWorldTransform();											
			}
		}});		

		timeSlider = $("#spritesheetdemo-timeslider");
		timeSlider.slider({ range: "max", min: 0, max: 200, value: 50 });
		timeSliderLabel = $("#spritesheetdemo-timeslider-label");		
	}

	function render () {
		var now = Date.now() / 1000;
		var delta = now - lastFrameTime;
		lastFrameTime = now;
		if (delta > 0.032) delta = 0.032;

		delta *= (timeSlider.slider("value") / 100);
		if (timeSliderLabel) timeSliderLabel.text(timeSlider.slider("value") + "%");	
		
		if (isPlaying) {
			var animationDuration = animationState.getCurrent(0).animation.duration;
			playTime += delta;			
			while (playTime >= animationDuration) {
				playTime -= animationDuration;
			}
			timeLine.slider("value", (playTime / animationDuration * 100));
							
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
		}			

		renderer.camera.position.x = offset.x + viewportWidth / 2 + 100;
		renderer.camera.position.y = offset.y + viewportHeight / 2;	
		renderer.camera.viewportWidth = viewportWidth * 1.2;
		renderer.camera.viewportHeight = viewportHeight * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);
		gl.clearColor(0.2, 0.2, 0.2, 1);
		gl.clear(gl.COLOR_BUFFER_BIT);	

		renderer.begin();		
		var frame = frames[currFrame];				
		renderer.drawSkeleton(skeleton, true);
		renderer.drawSkeleton(skeletonSeq, true);		
		renderer.end();		
	}

	init();
};