var stretchyDemo = function(loadingComplete, bgColor) {
	var COLOR_INNER = new spine.Color(0.8, 0, 0, 0.5);
	var COLOR_OUTER = new spine.Color(0.8, 0, 0, 0.8);
	var COLOR_INNER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.5);
	var COLOR_OUTER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.8);

	var canvas, gl, renderer, input, assetManager;
	var skeleton, bounds;		
	var timeKeeper, loadingScreen;
	var target = null;
	var kneeFront, kneeBack;
	var hoverTargets = [];
	var controlBones = ["front leg controller", "back leg controller", "hip", "back hand controller", "front hand controller", "spine control"];
	var coords = new spine.webgl.Vector3(), temp = new spine.webgl.Vector3(), temp2 = new spine.Vector2(), temp3 = new spine.webgl.Vector3();
	var kneePos = new spine.Vector2();
	var playButton, timeLine, spacing, isPlaying = true, playTime = 0;

	var DEMO_NAME = "StretchyDemo";

	if (!bgColor) bgColor = new spine.Color(1, 1, 1, 1);	

	function init () {
		canvas = document.getElementById("stretchydemo-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = spineDemos.assetManager;
		var textureLoader = function(img) { return new spine.webgl.GLTexture(gl, img); };
		input = new spine.webgl.Input(canvas);		
		assetManager.loadTexture(DEMO_NAME, textureLoader, "atlas2.png");
		assetManager.loadText(DEMO_NAME, "atlas2.atlas");
		assetManager.loadJson(DEMO_NAME, "demos.json");	
		timeKeeper = new spine.TimeKeeper();		
		loadingScreen = new spine.webgl.LoadingScreen(renderer);
		loadingScreen.backgroundColor = bgColor;
		requestAnimationFrame(load);
	}

	function load () {
		timeKeeper.update();
		if (assetManager.isLoadingComplete(DEMO_NAME)) {
			var atlas = new spine.TextureAtlas(assetManager.get(DEMO_NAME, "atlas2.atlas"), function(path) {
				return assetManager.get(DEMO_NAME, path);		
			});
			var atlasLoader = new spine.TextureAtlasAttachmentLoader(atlas);
			var skeletonJson = new spine.SkeletonJson(atlasLoader);
			var skeletonData = skeletonJson.readSkeletonData(assetManager.get(DEMO_NAME, "demos.json").stretchyman);
			skeleton = new spine.Skeleton(skeletonData);
			skeleton.setToSetupPose();
			skeleton.updateWorldTransform();
			var offset = new spine.Vector2();
			bounds = new spine.Vector2();
			skeleton.getBounds(offset, bounds);
			for (var i = 0; i < controlBones.length; i++) hoverTargets.push(null);
			kneeFront = skeleton.findBone("front leg middle");
			kneeBack = skeleton.findBone("back leg middle");			

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
		var checkbox = $("#stretchydemo-drawbones");
		renderer.skeletonDebugRenderer.drawPaths = false;
		renderer.skeletonDebugRenderer.drawBones = false;
		checkbox.change(function() {
			renderer.skeletonDebugRenderer.drawPaths = this.checked;
			renderer.skeletonDebugRenderer.drawBones = this.checked;			
		});
	}

	function setupInput (){
		input.addListener({
			down: function(x, y) {
				for (var i = 0; i < controlBones.length; i++) {	
					var bone = skeleton.findBone(controlBones[i]);				
					renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);				
					if (temp.set(skeleton.x + bone.worldX, skeleton.y + bone.worldY, 0).distance(coords) < 20) {
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
					if (temp.set(skeleton.x + bone.worldX, skeleton.y + bone.worldY, 0).distance(coords) < 20) {
						hoverTargets[i] = bone;
					} else {
						hoverTargets[i] = null;
					}
				}	
			}
		});
	}

	function centerKnee(kneeBone, hipBone, footBone) {
		temp.set(footBone.worldX + skeleton.x, footBone.worldY + skeleton.y, 0)
			.sub(temp3.set(hipBone.worldX + skeleton.x, hipBone.worldY + skeleton.y, 0));
		temp3.set(hipBone.worldX + skeleton.x, hipBone.worldY + skeleton.y, 0);
		temp.scale(0.5).add(temp3);
		kneeBone.parent.worldToLocal(kneePos.set(temp.x, temp.y));
		kneeBone.x = kneePos.x;
		kneeBone.y = kneePos.y;		
	}

	function render () {
		timeKeeper.update();
		var delta = timeKeeper.delta;	

		skeleton.updateWorldTransform();
		centerKnee(kneeBack, skeleton.findBone("back leg root"), skeleton.findBone("back leg controller"));
		centerKnee(kneeFront, skeleton.findBone("front leg root"), skeleton.findBone("front leg controller"));
		skeleton.updateWorldTransform();

		renderer.camera.viewportWidth = bounds.x * 1.2;
		renderer.camera.viewportHeight = bounds.y * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);			

		renderer.begin();				
		renderer.drawSkeleton(skeleton, true);
		renderer.drawSkeletonDebug(skeleton, false, ["root", "front leg middle", "back leg middle"]);
		gl.lineWidth(2);
		for (var i = 0; i < controlBones.length; i++) {		
			var bone = skeleton.findBone(controlBones[i]);
			var colorInner = hoverTargets[i] !== null ? spineDemos.HOVER_COLOR_INNER : spineDemos.NON_HOVER_COLOR_INNER;
			var colorOuter = hoverTargets[i] !== null ? spineDemos.HOVER_COLOR_OUTER : spineDemos.NON_HOVER_COLOR_OUTER;
			renderer.circle(true, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorInner);			
			renderer.circle(false, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorOuter);			
		}
		renderer.end();
		gl.lineWidth(1);
	}

	init();
};