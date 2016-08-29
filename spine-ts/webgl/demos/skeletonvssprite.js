var skeletonVsSpriteDemo = function(pathPrefix) {
	var SKELETON_ATLAS_COLOR = new spine.Color(0, 0.8, 0, 0.8);
	var FRAME_ATLAS_COLOR = new spine.Color(0.8, 0, 0, 0.8);

	var canvas, gl, renderer, input, assetManager;
	var skeleton, animationState, offset, bounds;
	var skeletonAtlas;
	var frameAtlas;
	var viewportWidth, viewportHeight;
	var frames = [], currFrame = 0, frameTime = 0, frameScale = 0, FPS = 30;
	var lastFrameTime = Date.now() / 1000;
	var timeSlider, timeSliderLabel, atlasCheckbox;
	var playButton, timeLine, isPlaying = true, playTime = 0;

	function init () {
		if (pathPrefix === undefined) pathPrefix = "";		

		canvas = document.getElementById("skeletonvsspritedemo-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;	
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = new spine.webgl.AssetManager(gl, pathPrefix);		
		assetManager.loadTexture("assets/raptor.png");
		assetManager.loadText("assets/raptor.json");
		assetManager.loadText("assets/raptor.atlas");
		assetManager.loadText("assets/raptor-walk.atlas");
		assetManager.loadTexture("assets/raptor-walk.png");
		assetManager.loadTexture("assets/raptor-walk2.png");
		assetManager.loadTexture("assets/raptor-walk3.png");
		assetManager.loadTexture("assets/raptor-walk4.png");
		requestAnimationFrame(load);
	}

	function load () {
		if (assetManager.isLoadingComplete()) {
			skeletonAtlas = new spine.TextureAtlas(assetManager.get("assets/raptor.atlas"), function(path) {
				return assetManager.get("assets/" + path);		
			});
			var atlasLoader = new spine.TextureAtlasAttachmentLoader(skeletonAtlas);
			var skeletonJson = new spine.SkeletonJson(atlasLoader);
			var skeletonData = skeletonJson.readSkeletonData(assetManager.get("assets/raptor.json"));
			skeleton = new spine.Skeleton(skeletonData);
			animationState = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
			animationState.setAnimation(0, "walk", true);
			animationState.apply(skeleton);
			skeleton.updateWorldTransform();
			offset = new spine.Vector2();
			bounds = new spine.Vector2();
			skeleton.getBounds(offset, bounds);

			frameAtlas = new spine.TextureAtlas(assetManager.get("assets/raptor-walk.atlas"), function(path) {
				return assetManager.get("assets/" + path);		
			});
			for (var i = 0; i < frameAtlas.regions.length - 1; i++) {
				frames.push(frameAtlas.findRegion("raptor-walk_" + i));
			}
			frameScale = bounds.x / frames[0].width * 1.1;
			viewportWidth = ((700 + bounds.x) - offset.x);
			viewportHeight = ((0 + bounds.y) - offset.y);						

			setupUI();
			requestAnimationFrame(render);
		} else requestAnimationFrame(load);
	}

	function setupUI() {
		playButton = $("#skeletonvsspritedemo-playbutton");
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

		timeLine = $("#skeletonvsspritedemo-timeline");
		timeLine.slider({ range: "max", min: 0, max: 100, value: 0, slide: function () {
			if (isPlaying) playButton.click();		
			if (!isPlaying) {				
				var time = timeLine.slider("value") / 100;
				var animationDuration = animationState.getCurrent(0).animation.duration;
				time = animationDuration * time;				
				animationState.update(time - playTime);
				animationState.apply(skeleton);
				skeleton.updateWorldTransform();
				playTime = time;
				frameTime = time;
				while (frameTime > animationDuration) frameTime -= animationDuration;				
				currFrame = Math.min(frames.length - 1, (frameTime / (1 / FPS)) | 0);								
			}
		}});		

		timeSlider = $("#skeletonvsspritedemo-timeslider");
		timeSlider.slider({ range: "max", min: 0, max: 200, value: 50 });
		timeSliderLabel = $("#skeletonvsspritedemo-timeslider-label");
		atlasCheckbox = document.getElementById("skeletonvsspritedemo-atlascheckbox");
	}

	function render () {
		var now = Date.now() / 1000;
		var delta = now - lastFrameTime;
		lastFrameTime = now;
		if (delta > 0.032) delta = 0.032;

		delta *= (timeSlider.slider("value") / 100);
		if (timeSliderLabel) timeSliderLabel.text(timeSlider.slider("value") + "%");	

		if (!atlasCheckbox.checked) {
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

				frameTime += delta;
				while (frameTime > animationDuration) frameTime -= animationDuration;				
				currFrame = Math.min(frames.length - 1, (frameTime / (1 / FPS)) | 0);
			}
		}	

		renderer.camera.position.x = offset.x + viewportWidth / 2 + 100;
		renderer.camera.position.y = offset.y + viewportHeight / 2;	
		renderer.camera.viewportWidth = viewportWidth * 1.2;
		renderer.camera.viewportHeight = viewportHeight * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);
		gl.clearColor(0.2, 0.2, 0.2, 1);
		gl.clear(gl.COLOR_BUFFER_BIT);	

		renderer.begin();
		if (!atlasCheckbox.checked) {
			var frame = frames[currFrame];
			renderer.drawRegion(frame, 700, offset.y - 40, frame.width * frameScale, frame.height * frameScale);	
			renderer.drawSkeleton(skeleton);
		} else {		
			var skeletonAtlasSize = skeletonAtlas.pages[0].texture.getImage().width;
			var frameAtlasSize = frameAtlas.pages[0].texture.getImage().width;
			var halfSpaceWidth = viewportWidth / 2;
			var halfSpaceHeight = viewportHeight;
			var pageSize = halfSpaceWidth / 2;															

			// we only have one page for skeleton
			var skeletonPageSize = pageSize * skeletonAtlasSize / frameAtlasSize;
			renderer.rect(true, offset.x + halfSpaceWidth / 2 - skeletonPageSize / 2,
						  offset.y + halfSpaceHeight / 2 - skeletonPageSize / 2, skeletonPageSize, skeletonPageSize, spine.Color.WHITE);
			renderer.drawTexture(skeletonAtlas.pages[0].texture, offset.x + halfSpaceWidth / 2 - skeletonPageSize / 2,
								 offset.y + halfSpaceHeight / 2 - skeletonPageSize / 2, skeletonPageSize, skeletonPageSize);
			renderer.rect(false, offset.x + halfSpaceWidth / 2 - skeletonPageSize / 2,
						  offset.y + halfSpaceHeight / 2 - skeletonPageSize / 2, skeletonPageSize, skeletonPageSize, SKELETON_ATLAS_COLOR);

			var x = offset.x + halfSpaceWidth  + 150;
			var y = offset.y + halfSpaceHeight / 2;
			var i = 0;
			for (var row = 0; row < frameAtlas.pages.length / 2; row++) {
				for (var col = 0; col < 2; col++) {
					var page = frameAtlas.pages[i++];
					renderer.rect(true, x + col * pageSize, y - row * pageSize, pageSize, pageSize, spine.Color.WHITE);
					renderer.drawTexture(page.texture, x + col * pageSize, y - row * pageSize, pageSize, pageSize);
					renderer.rect(false, x + col * pageSize, y - row * pageSize, pageSize, pageSize, FRAME_ATLAS_COLOR);
				}
			}			
		}
		renderer.end();

		requestAnimationFrame(render);
	}

	init();
};