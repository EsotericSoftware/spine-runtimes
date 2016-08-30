var meshesDemo = function(pathPrefix) {
	var canvas, gl, renderer, input, assetManager;
	var skeleton, bounds;		
	var lastFrameTime = Date.now() / 1000;
	var skeletons = {};
	var activeSkeleton = "girl";

	var playButton, timeLine, isPlaying = true;

	function init () {
		canvas = document.getElementById("meshesdemo-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = new spine.webgl.AssetManager(gl, pathPrefix);		
		assetManager.loadTexture("assets/girl.png");
		assetManager.loadText("assets/girl.json");
		assetManager.loadText("assets/girl.atlas");
		assetManager.loadTexture("assets/gree_girl.png");		
		assetManager.loadText("assets/gree_girl.json");
		assetManager.loadText("assets/gree_girl.atlas");
		assetManager.loadTexture("assets/fanart_cut.png");		
		assetManager.loadText("assets/fanart_cut.json");
		assetManager.loadText("assets/fanart_cut.atlas");
		requestAnimationFrame(load);
	}	

	function load () {
		if (assetManager.isLoadingComplete()) {
			skeletons["girl"] = loadSkeleton("girl", "animation");
			skeletons["green_girl"] = loadSkeleton("gree_girl", "animation");
			skeletons["fanart"] = loadSkeleton("fanart_cut", "animation");
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
				var active = skeletons[activeSkeleton];
				var time = timeLine.slider("value") / 100;
				var animationDuration = active.state.getCurrent(0).animation.duration;
				time = animationDuration * time;
				active.state.update(time - active.playTime);
				active.state.apply(active.skeleton);
				active.skeleton.updateWorldTransform();
				active.playTime = time;				
			}
		}});

		var list = $("#meshesdemo-active-skeleton");	
		for (var skeletonName in skeletons) {
			var option = $("<option></option>");
			option.attr("value", skeletonName).text(skeletonName);
			if (skeletonName === activeSkeleton) option.attr("selected", "selected");
			list.append(option);
		}
		list.change(function() {
			activeSkeleton = $("#meshesdemo-active-skeleton option:selected").text();
			var active = skeletons[activeSkeleton];
			var animationDuration = active.state.getCurrent(0).animation.duration;
			timeLine.slider("value", (active.playTime / animationDuration * 100));
		})

		$("#meshesdemo-drawbonescheckbox").click(function() {
			renderer.skeletonDebugRenderer.drawBones = this.checked;
		})
		$("#meshesdemo-drawregionscheckbox").click(function() {
			renderer.skeletonDebugRenderer.drawRegionAttachments = this.checked;
		})
		$("#meshesdemo-drawmeshhullcheckbox").click(function() {
			renderer.skeletonDebugRenderer.drawMeshHull = this.checked;
		})
		$("#meshesdemo-drawmeshtrianglescheckbox").click(function() {
			renderer.skeletonDebugRenderer.drawMeshTriangles = this.checked;
		})
	}

	function loadSkeleton(name, animation, sequenceSlots) {
		var atlas = new spine.TextureAtlas(assetManager.get("assets/" + name + ".atlas"), function(path) {
			return assetManager.get("assets/" + path);		
		});
		var atlasLoader = new spine.TextureAtlasAttachmentLoader(atlas);
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get("assets/" + name + ".json"));
		var skeleton = new spine.Skeleton(skeletonData);
		skeleton.setSkinByName("default");

		var state = new spine.AnimationState(new spine.AnimationStateData(skeletonData));
		state.setAnimation(0, animation, true);
		state.apply(skeleton);
		skeleton.updateWorldTransform();			
		var offset = new spine.Vector2();
		var size = new spine.Vector2();
		skeleton.getBounds(offset, size);

		return {
			atlas: atlas,
			skeleton: skeleton, 
			state: state, 
			playTime: 0,
			bounds: {
				offset: offset,
				size: size
			}			
		};
	}

	function render () {
		var now = Date.now() / 1000;
		var delta = now - lastFrameTime;
		lastFrameTime = now;	
		if (delta > 0.032) delta = 0.032;	

		var active = skeletons[activeSkeleton];
		var skeleton = active.skeleton;
		var state = active.state;
		var offset = active.bounds.offset;
		var size = active.bounds.size;

		renderer.camera.position.x = offset.x + size.x / 2;
		renderer.camera.position.y = offset.y + size.y / 2;
		renderer.camera.viewportWidth = size.x * 1.2;
		renderer.camera.viewportHeight = size.y * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(0.2, 0.2, 0.2, 1);
		gl.clear(gl.COLOR_BUFFER_BIT);

		if (isPlaying) {
			var animationDuration = state.getCurrent(0).animation.duration;
			active.playTime += delta;			
			while (active.playTime >= animationDuration) {
				active.playTime -= animationDuration;
			}
			timeLine.slider("value", (active.playTime / animationDuration * 100));

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