var meshesDemo = function(pathPrefix) {
	var CIRCLE_INNER_COLOR = new spine.Color(0.8, 0, 0, 0.5);
	var CIRCLE_OUTER_COLOR = new spine.Color(0.8, 0, 0, 0.8);

	var canvas, gl, renderer, input, assetManager;
	var skeleton, state, bounds;		
	var lastFrameTime = Date.now() / 1000;	
	var playButton, timeLine, isPlaying = true, playTime = 0;		

	function init () {
		if (pathPrefix === undefined) pathPrefix = "";		

		canvas = document.getElementById("meshesdemo-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);		
		assetManager = new spine.webgl.AssetManager(gl, pathPrefix);		
		assetManager.loadTexture("assets/raptor.png");
		assetManager.loadText("assets/raptor.json");
		assetManager.loadText("assets/raptor.atlas");
		requestAnimationFrame(load);
	}

	function load () {
		if (assetManager.isLoadingComplete()) {
			var atlas = new spine.TextureAtlas(assetManager.get("assets/raptor.atlas"), function(path) {
				return assetManager.get("assets/" + path);		
			});
			var atlasLoader = new spine.TextureAtlasAttachmentLoader(atlas);
			var skeletonJson = new spine.SkeletonJson(atlasLoader);
			var skeletonData = skeletonJson.readSkeletonData(assetManager.get("assets/raptor.json"));
			skeleton = new spine.Skeleton(skeletonData);
			state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
			state.setAnimation(0, "walk", true);
			state.apply(skeleton);
			skeleton.updateWorldTransform();
			var offset = new spine.Vector2();
			bounds = new spine.Vector2();
			skeleton.getBounds(offset, bounds);

			renderer.camera.position.x = offset.x + bounds.x / 2;
			renderer.camera.position.y = offset.y + bounds.y / 2;

			setupUI();
			requestAnimationFrame(render);
		} else requestAnimationFrame(load);
	}

	function setupUI() {
		playButton = $("#meshesdemo-playbutton");
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

		timeLine = $("#meshesdemo-timeline");
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

		$("#meshesdemo-drawbonescheckbox").change(function() {
			renderer.skeletonDebugRenderer.drawBones = this.checked;
		});
		$("#meshesdemo-drawregionscheckbox").change(function() {
			renderer.skeletonDebugRenderer.drawRegionAttachments = this.checked;
		});
		$("#meshesdemo-drawmeshhullcheckbox").change(function() {
			renderer.skeletonDebugRenderer.drawMeshHull = this.checked;
		});
		$("#meshesdemo-drawmeshtrianglescheckbox").change(function() {
			renderer.skeletonDebugRenderer.drawMeshTriangles = this.checked;
		});
	}

	function render () {
		var now = Date.now() / 1000;
		var delta = now - lastFrameTime;
		lastFrameTime = now;
		if (delta > 0.032) delta = 0.032;

		renderer.camera.viewportWidth = bounds.x * 1.2;
		renderer.camera.viewportHeight = bounds.y * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(0.2, 0.2, 0.2, 1);
		gl.clear(gl.COLOR_BUFFER_BIT);
		
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

		renderer.begin();				
		renderer.drawSkeleton(skeleton);
		renderer.drawSkeletonDebug(skeleton);
		renderer.end();

		requestAnimationFrame(render);
	}

	init();
};