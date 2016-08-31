var tankDemo = function(pathPrefix, loadingComplete) {	
	var canvas, gl, renderer, input, assetManager;
	var skeleton, state, offset, bounds;		
	var lastFrameTime = Date.now() / 1000;	
	var playButton, timeLine, isPlaying = true, playTime = 0;		

	function init () {
		if (pathPrefix === undefined) pathPrefix = "";		

		canvas = document.getElementById("tankdemo-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);		
		assetManager = new spine.webgl.AssetManager(gl, pathPrefix);		
		assetManager.loadTexture("tank.png");
		assetManager.loadText("tank.json");
		assetManager.loadText("tank.atlas");
		requestAnimationFrame(load);
	}

	function load () {
		if (assetManager.isLoadingComplete()) {
			var atlas = new spine.TextureAtlas(assetManager.get("tank.atlas"), function(path) {
				return assetManager.get(path);		
			});
			var atlasLoader = new spine.TextureAtlasAttachmentLoader(atlas);
			var skeletonJson = new spine.SkeletonJson(atlasLoader);
			var skeletonData = skeletonJson.readSkeletonData(assetManager.get("tank.json"));
			skeleton = new spine.Skeleton(skeletonData);
			state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
			state.setAnimation(0, "drive", true);
			state.apply(skeleton);
			skeleton.updateWorldTransform();
			offset = new spine.Vector2();
			bounds = new spine.Vector2();
			skeleton.getBounds(offset, bounds);
			setupUI();
			loadingComplete(canvas, render);
		} else requestAnimationFrame(load);
	}

	function setupUI() {
		playButton = $("#tankdemo-playbutton");
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

		timeLine = $("#tankdemo-timeline");
		timeLine.slider({ range: "max", min: 0, max: 100, value: 0, slide: function () {
			if (isPlaying) playButton.click();
			if (!isPlaying) {				
				var time = timeLine.slider("value") / 100;
				var animationDuration = state.getCurrent(0).animation.duration;
				time = animationDuration * time;				
				state.update(time - playTime);
				state.apply(skeleton);
				skeleton.updateWorldTransform();
				playTime = time;												
			}
		}});		
	}

	function render () {
		var now = Date.now() / 1000;
		var delta = now - lastFrameTime;
		lastFrameTime = now;
		if (delta > 0.032) delta = 0.032;

		if (isPlaying) {
			var animationDuration = state.getCurrent(0).animation.duration;
			playTime += delta;			
			while (playTime >= animationDuration) {
				playTime -= animationDuration;
			}
			timeLine.slider("value", (playTime / animationDuration * 100));

			state.update(delta);
			state.apply(skeleton);
			skeleton.updateWorldTransform();			
		}	

		offset.x = skeleton.findBone("tankRoot").worldX;
		offset.y = skeleton.findBone("tankRoot").worldY;

		renderer.camera.position.x = offset.x - 300;
		renderer.camera.position.y = offset.y + 200;
		renderer.camera.viewportWidth = bounds.x * 1.2;
		renderer.camera.viewportHeight = bounds.y * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(0.2, 0.2, 0.2, 1);
		gl.clear(gl.COLOR_BUFFER_BIT);			

		renderer.begin();				
		renderer.drawSkeleton(skeleton, true);				
		renderer.end();
	}

	init();
};