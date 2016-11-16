var vineDemo = function(loadingComplete, bgColor) {
	var COLOR_INNER = new spine.Color(0.8, 0, 0, 0.5);
	var COLOR_OUTER = new spine.Color(0.8, 0, 0, 0.8);
	var COLOR_INNER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.5);
	var COLOR_OUTER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.8);

	var canvas, gl, renderer, input, assetManager;
	var skeleton, state, bounds;		
	var timeKeeper, loadingScreen;
	var target = null;
	var hoverTargets = [null, null, null, null, null, null];
	var controlBones = ["base", "vine-control1", "vine-control2", "vine-control3", "vine-control4"];
	var coords = new spine.webgl.Vector3(), temp = new spine.webgl.Vector3(), temp2 = new spine.Vector2();
	var playButton, timeLine, isPlaying = true, playTime = 0;

	var DEMO_NAME = "VineDemo";

	if (!bgColor) bgColor = new spine.Color(235 / 255, 239 / 255, 244 / 255, 1);

	function init () {
		canvas = document.getElementById("vine-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);		
		input = new spine.webgl.Input(canvas);
		assetManager = spineDemos.assetManager;
		var textureLoader = function(img) { return new spine.webgl.GLTexture(gl, img); };
		assetManager.loadTexture(DEMO_NAME, textureLoader, "atlas2.png");
		assetManager.loadText(DEMO_NAME, "atlas2.atlas");
		assetManager.loadJson(DEMO_NAME, "demos.json");		
		timeKeeper = new spine.TimeKeeper();		
		loadingScreen = new spine.webgl.LoadingScreen(renderer);
		requestAnimationFrame(load);
	}

	function load () {
		timeKeeper.update();
		if (assetManager.isLoadingComplete(DEMO_NAME)) {
			var atlas = new spine.TextureAtlas(assetManager.get(DEMO_NAME, "atlas2.atlas"), function(path) {
				return assetManager.get(DEMO_NAME, path);		
			});
			var atlasLoader = new spine.AtlasAttachmentLoader(atlas);
			var skeletonJson = new spine.SkeletonJson(atlasLoader);
			var skeletonData = skeletonJson.readSkeletonData(assetManager.get(DEMO_NAME, "demos.json").vine);
			skeleton = new spine.Skeleton(skeletonData);
			skeleton.setToSetupPose();
			skeleton.updateWorldTransform();
			var offset = new spine.Vector2();
			bounds = new spine.Vector2();
			skeleton.getBounds(offset, bounds);
			state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
			state.setAnimation(0, "animation", true);
			state.apply(skeleton);
			skeleton.updateWorldTransform();

			renderer.camera.position.x = offset.x + bounds.x / 2;
			renderer.camera.position.y = offset.y + bounds.y / 2;

			renderer.skeletonDebugRenderer.drawMeshHull = false;
			renderer.skeletonDebugRenderer.drawMeshTriangles = false;

			setupUI();
			setupInput();

			loadingComplete(canvas, render);
		} else {
			loadingScreen.draw(); 
			requestAnimationFrame(load);
		}
	}

	function setupUI() {
		playButton = $("#vine-playbutton");
		var playButtonUpdate = function () {			
			isPlaying = !isPlaying;
			if (isPlaying)
				playButton.addClass("pause").removeClass("play");
			else
				playButton.addClass("play").removeClass("pause");
		}
		playButton.click(playButtonUpdate);
		playButton.addClass("pause");

		timeLine = $("#vine-timeline").data("slider");
		timeLine.changed = function (percent) {
			if (isPlaying) playButton.click();
			if (!isPlaying) {
				var animationDuration = state.getCurrent(0).animation.duration;
				time = animationDuration * percent;	
				state.update(time - playTime);
				state.apply(skeleton);
				skeleton.updateWorldTransform();
				playTime = time;		
			}
		};

		renderer.skeletonDebugRenderer.drawPaths = false;
		renderer.skeletonDebugRenderer.drawBones = false;
		var checkbox = $("#vine-drawbones");
		checkbox.change(function() {
			renderer.skeletonDebugRenderer.drawPaths = this.checked;
			renderer.skeletonDebugRenderer.drawBones = this.checked;			
		});
	}

	function setupInput() {
		input.addListener({
			down: function(x, y) {
				for (var i = 0; i < controlBones.length; i++) {	
					var bone = skeleton.findBone(controlBones[i]);				
					renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);				
					if (temp.set(skeleton.x + bone.worldX, skeleton.y + bone.worldY, 0).distance(coords) < 30) {
						target = bone;
					}				
				}
			},
			up: function(x, y) {
				target = null;
			},
			dragged: function(x, y) {
				if (target != null) {
					renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
					if (target.parent !== null) {
						target.parent.worldToLocal(temp2.set(coords.x - skeleton.x, coords.y - skeleton.y));
						target.x = temp2.x;
						target.y = temp2.y;
					} else {
						target.x = coords.x - skeleton.x;
						target.y = coords.y - skeleton.y;
					}
				}
			},
			moved: function (x, y) {
				for (var i = 0; i < controlBones.length; i++) {	
					var bone = skeleton.findBone(controlBones[i]);				
					renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);				
					if (temp.set(skeleton.x + bone.worldX, skeleton.y + bone.worldY, 0).distance(coords) < 30) {
						hoverTargets[i] = bone;
					} else {
						hoverTargets[i] = null;
					}
				}				
			}
		});
	}

	function render () {
		timeKeeper.update();
		var delta = timeKeeper.delta;

		if (isPlaying) {
			var animationDuration = state.getCurrent(0).animation.duration;
			playTime += delta;			
			while (playTime >= animationDuration) {
				playTime -= animationDuration;
			}
			timeLine.set(playTime / animationDuration);

			state.update(delta);
			state.apply(skeleton);						
		}

		skeleton.updateWorldTransform();

		renderer.camera.viewportWidth = bounds.x * 1.2;
		renderer.camera.viewportHeight = bounds.y * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);			

		renderer.begin();				
		renderer.drawSkeleton(skeleton, true);
		renderer.drawSkeletonDebug(skeleton);
		gl.lineWidth(2);
		for (var i = 0; i < controlBones.length; i++) {		
			var bone = skeleton.findBone(controlBones[i]);
			var colorInner = hoverTargets[i] !== null ? spineDemos.HOVER_COLOR_INNER : spineDemos.NON_HOVER_COLOR_INNER;
			var colorOuter = hoverTargets[i] !== null ? spineDemos.HOVER_COLOR_OUTER : spineDemos.NON_HOVER_COLOR_OUTER;
			renderer.circle(true, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorInner);			
			renderer.circle(false, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorOuter);			
		}
		gl.lineWidth(1);
		renderer.end();

		loadingScreen.draw(true);
	}

	init();
};