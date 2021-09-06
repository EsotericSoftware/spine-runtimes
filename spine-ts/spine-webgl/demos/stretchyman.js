var stretchymanDemo = function (canvas, bgColor) {
	var COLOR_INNER = new spine.Color(0.8, 0, 0, 0.5);
	var COLOR_OUTER = new spine.Color(0.8, 0, 0, 0.8);
	var COLOR_INNER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.5);
	var COLOR_OUTER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.8);

	var canvas, gl, renderer, input, assetManager;
	var skeleton, bounds, state;
	var timeKeeper;
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
	var coords = new spine.Vector3(), temp = new spine.Vector3(), temp2 = new spine.Vector2(), temp3 = new spine.Vector3();
	var kneePos = new spine.Vector2();
	var playButton, timeLine, spacing, isPlaying = true, playTime = 0;

	if (!bgColor) bgColor = new spine.Color(235 / 255, 239 / 255, 244 / 255, 1);

	function init() {
		gl = canvas.context.gl;
		renderer = new spine.SceneRenderer(canvas, gl);
		assetManager = new spine.AssetManager(gl, spineDemos.path, spineDemos.downloader);
		assetManager.loadTextureAtlas("atlas2.atlas");
		assetManager.loadJson("demos.json");
		timeKeeper = new spine.TimeKeeper();
		input = new spine.Input(canvas);
	}

	function loadingComplete() {
		var atlasLoader = new spine.AtlasAttachmentLoader(assetManager.get("atlas2.atlas"));
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get("demos.json").stretchyman);
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
	}

	function setupUI() {
		var checkbox = $("#stretchyman-drawbones");
		renderer.skeletonDebugRenderer.drawPaths = false;
		renderer.skeletonDebugRenderer.drawBones = false;
		checkbox.change(function () {
			renderer.skeletonDebugRenderer.drawPaths = this.checked;
			renderer.skeletonDebugRenderer.drawBones = this.checked;
		});
	}

	function setupInput() {
		input.addListener({
			down: function (x, y) {
				target = spineDemos.closest(canvas, renderer, skeleton, controlBones, hoverTargets, x, y);
			},
			up: function (x, y) {
				target = null;
			},
			dragged: function (x, y) {
				spineDemos.dragged(canvas, renderer, target, x, y);
				if (target && target.data.name === "head controller") {
					var hipControl = skeleton.findBone("hip controller");
					target.x = spine.MathUtils.clamp(target.x, -65, 65);
					target.y = Math.max(260, target.y);
				}
			},
			moved: function (x, y) {
				spineDemos.closest(canvas, renderer, skeleton, controlBones, hoverTargets, x, y);
			}
		});
	}

	function center(middleBone, hipBone, footBone, amount, dir) {
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

	function rotate(handBone, elbowBone) {
		// can do all this in world space cause handBone is essentially in world space
		var v = coords.set(handBone.worldX, handBone.worldY, 0).sub(new spine.Vector3(elbowBone.worldX, elbowBone.worldY, 0)).normalize();
		var angle = Math.acos(v.x) * spine.MathUtils.radiansToDegrees + 180;
		if (v.y < 0) angle = 360 - angle;
		handBone.rotation = angle;
	}

	function render() {
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
		renderer.resize(spine.ResizeMode.Fit);

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
	}

	init();
	stretchymanDemo.assetManager = assetManager;
	stretchymanDemo.loadingComplete = loadingComplete;
	stretchymanDemo.render = render;
};