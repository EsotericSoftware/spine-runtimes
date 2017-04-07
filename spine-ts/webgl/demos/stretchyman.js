var stretchymanDemo = function(loadingComplete, bgColor) {
	var COLOR_INNER = new spine.Color(0.8, 0, 0, 0.5);
	var COLOR_OUTER = new spine.Color(0.8, 0, 0, 0.8);
	var COLOR_INNER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.5);
	var COLOR_OUTER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.8);

	var canvas, gl, renderer, input, assetManager;
	var skeleton, bounds, state;
	var timeKeeper, loadingScreen;
	var target = null;
	var hoverTargets = [];
	var controlBones = [
		"back leg controller",
		"front leg controller",
		"back arm controller",
		"front arm controller",
		"head controller",
		"hip controller"
	];
	var coords = new spine.webgl.Vector3(), temp = new spine.webgl.Vector3(), temp2 = new spine.Vector2(), temp3 = new spine.webgl.Vector3();
	var kneePos = new spine.Vector2();
	var playButton, timeLine, spacing, isPlaying = true, playTime = 0;

	var DEMO_NAME = "StretchymanDemo";

	if (!bgColor) bgColor = new spine.Color(235 / 255, 239 / 255, 244 / 255, 1);

	function init () {
		canvas = document.getElementById("stretchyman-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });
		if (!gl) {
			alert('WebGL is unavailable.');
			return;
		}

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = spineDemos.assetManager;
		var textureLoader = function(img) { return new spine.webgl.GLTexture(gl, img); };
		input = new spine.webgl.Input(canvas);
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
			var skeletonData = skeletonJson.readSkeletonData(assetManager.get(DEMO_NAME, "demos.json").stretchyman);
			skeleton = new spine.Skeleton(skeletonData);
			skeleton.setToSetupPose();
			skeleton.updateWorldTransform();
			var offset = new spine.Vector2();
			bounds = new spine.Vector2();
			skeleton.getBounds(offset, bounds, []);
			for (var i = 0; i < controlBones.length; i++) hoverTargets.push(null);
			state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
			state.setAnimation(0, "idle", true);

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
		var checkbox = $("#stretchyman-drawbones");
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
					if (target.parent !== null)
						target.parent.worldToLocal(temp2.set(coords.x - skeleton.x, coords.y - skeleton.y));
					else
						temp2.set(coords.x - skeleton.x, coords.y - skeleton.y);
					target.x = temp2.x;
					target.y = temp2.y;
					if (target.data.name === "head controller") {
						var hipControl = skeleton.findBone("hip controller");
						target.x = spine.MathUtils.clamp(target.x, -65, 65);
						target.y = Math.max(260, target.y);
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

	function center (middleBone, hipBone, footBone, amount, dir) {
		temp.set(footBone.worldX + skeleton.x, footBone.worldY + skeleton.y, 0)
			.sub(temp3.set(hipBone.worldX + skeleton.x, hipBone.worldY + skeleton.y, 0));
		var dist = Math.sqrt(temp.x * temp.x + temp.y * temp.y);
		temp3.set(hipBone.worldX + skeleton.x, hipBone.worldY + skeleton.y, 0);
		temp.scale(0.5).add(temp3);
		middleBone.parent.worldToLocal(kneePos.set(temp.x, temp.y));
		middleBone.x = kneePos.x;
		middleBone.y = kneePos.y;
		middleBone.children[0].y = (22 + Math.max(0, amount - dist * 0.3)) * dir;
	}

	function rotate (handBone, elbowBone) {
		// can do all this in world space cause handBone is essentially in world space
		var v = coords.set(handBone.worldX, handBone.worldY, 0).sub(new spine.webgl.Vector3(elbowBone.worldX, elbowBone.worldY, 0)).normalize();
		var angle = Math.acos(v.x) * spine.MathUtils.radiansToDegrees + 180;
		if (v.y < 0) angle = 360 - angle;
		handBone.rotation = angle;
	}

	function render () {
		timeKeeper.update();
		var delta = timeKeeper.delta;

		state.update(delta);
		state.apply(skeleton);
		center(skeleton.findBone("back leg middle"), skeleton.findBone("back leg 1"), skeleton.findBone("back leg controller"), 65, 1);
		center(skeleton.findBone("front leg middle"), skeleton.findBone("front leg 1"), skeleton.findBone("front leg controller"), 65, 1);
		center(skeleton.findBone("front arm middle"), skeleton.findBone("front arm 1"), skeleton.findBone("front arm controller"), 90, -1);
		center(skeleton.findBone("back arm middle"), skeleton.findBone("back arm 1"), skeleton.findBone("back arm controller"), 90, -1);
		rotate(skeleton.findBone("front arm controller"), skeleton.findBone("front arm elbow"));
		rotate(skeleton.findBone("back arm controller"), skeleton.findBone("back arm elbow"));
		var headControl = skeleton.findBone("head controller"), hipControl = skeleton.findBone("hip controller")
		var head = skeleton.findBone("head");
		var angle = Math.atan2(headControl.worldY - hipControl.worldY, headControl.worldX - hipControl.worldX) * spine.MathUtils.radDeg;
		angle = (angle - 90) * 2.5;
		head.rotation = head.data.rotation + Math.min(90, Math.abs(angle)) * Math.sign(angle);
		skeleton.updateWorldTransform();

		renderer.camera.viewportWidth = bounds.x * 1.2;
		renderer.camera.viewportHeight = bounds.y * 1.5;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);

		renderer.begin();
		renderer.drawSkeleton(skeleton, true);
		renderer.drawSkeletonDebug(skeleton, false, ["root"]);
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

		loadingScreen.draw(true);
	}

	init();
};