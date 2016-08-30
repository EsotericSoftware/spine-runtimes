var skinsDemo = function(pathPrefix, loadingComplete) {	
	var canvas, gl, renderer, input, assetManager;
	var skeleton, state, offset, bounds;		
	var lastFrameTime = Date.now() / 1000;	
	var playButton, timeLine, isPlaying = true, playTime = 0;		

	function init () {
		if (pathPrefix === undefined) pathPrefix = "";		

		canvas = document.getElementById("skinsdemo-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);		
		assetManager = new spine.webgl.AssetManager(gl, pathPrefix);		
		assetManager.loadTexture("assets/goblins-pma.png");
		assetManager.loadText("assets/goblins-mesh.json");
		assetManager.loadText("assets/goblins-pma.atlas");
		requestAnimationFrame(load);
	}

	function load () {
		if (assetManager.isLoadingComplete()) {
			var atlas = new spine.TextureAtlas(assetManager.get("assets/goblins-pma.atlas"), function(path) {
				return assetManager.get("assets/" + path);		
			});
			var atlasLoader = new spine.TextureAtlasAttachmentLoader(atlas);
			var skeletonJson = new spine.SkeletonJson(atlasLoader);
			var skeletonData = skeletonJson.readSkeletonData(assetManager.get("assets/goblins-mesh.json"));
			skeleton = new spine.Skeleton(skeletonData);
			state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
			state.setAnimation(0, "walk", true);
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
		playButton = $("#skinsdemo-playbutton");
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

		timeLine = $("#skinsdemo-timeline");
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

		var list = $("#skinsdemo-active-skin");	
		for (var skin in skeleton.data.skins) {
			skin = skeleton.data.skins[skin];
			var option = $("<option></option>");
			option.attr("value", skin.name).text(skin.name);
			if (skin.name === "goblin") {
				option.attr("selected", "selected");
				skeleton.setSkinByName("goblin");
			}
			list.append(option);
		}
		list.change(function() {
			activeSkin = $("#skinsdemo-active-skin option:selected").text();
			skeleton.setSkinByName(activeSkin);
			skeleton.setSlotsToSetupPose();
		});
	}

	function render () {
		var now = Date.now() / 1000;
		var delta = now - lastFrameTime;
		lastFrameTime = now;
		if (delta > 0.032) delta = 0.032;

		renderer.camera.position.x = offset.x + bounds.x;
		renderer.camera.position.y = offset.y + bounds.y / 2;
		renderer.camera.viewportWidth = bounds.x * 2.2;
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
		renderer.drawSkeleton(skeleton, true);
		var texture = assetManager.get("assets/goblins-pma.png");
		var width = bounds.x * 1.3;
		var scale = width / texture.getImage().width;
		var height = scale * texture.getImage().height;
		renderer.drawTexture(texture, offset.x + bounds.x, offset.y + bounds.y / 2 - height / 2, width, height);		
		renderer.end();		
	}

	init();
};