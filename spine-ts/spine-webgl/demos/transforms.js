var transformsDemo = function (canvas, bgColor) {
	var COLOR_INNER = new spine.Color(0.8, 0, 0, 0.5);
	var COLOR_OUTER = new spine.Color(0.8, 0, 0, 0.8);
	var COLOR_INNER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.5);
	var COLOR_OUTER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.8);

	var canvas, gl, renderer, input, assetManager;
	var skeleton, state, bounds;
	var timeKeeper;
	var rotateHandle;
	var target = null;
	var hoverTargets = [null, null, null];
	var controlBones = ["wheel2overlay", "wheel3overlay", "rotate-handle"];
	var coords = new spine.Vector3(), temp = new spine.Vector3(), temp2 = new spine.Vector2();
	var lastRotation = 0;
	var mix, lastOffset = 0, lastMix = 0.5;

	if (!bgColor) bgColor = new spine.Color(235 / 255, 239 / 255, 244 / 255, 1);

	function init() {
		gl = canvas.context.gl;
		renderer = new spine.SceneRenderer(canvas, gl);
		assetManager = new spine.AssetManager(gl, spineDemos.path, spineDemos.downloader);
		assetManager.loadTextureAtlas("atlas2.atlas");
		assetManager.loadJson("demos.json");
		input = new spine.Input(canvas);
		timeKeeper = new spine.TimeKeeper();
	}

	function loadingComplete() {
		var atlasLoader = new spine.AtlasAttachmentLoader(assetManager.get("atlas2.atlas"));
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get("demos.json").transforms);
		skeleton = new spine.Skeleton(skeletonData);
		skeleton.setToSetupPose();
		skeleton.updateWorldTransform();
		var offset = new spine.Vector2();
		bounds = new spine.Vector2();
		skeleton.getBounds(offset, bounds, []);
		state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
		skeleton.setToSetupPose();
		skeleton.updateWorldTransform();
		rotateHandle = skeleton.findBone("rotate-handle");

		renderer.camera.position.x = offset.x + bounds.x / 2;
		renderer.camera.position.y = offset.y + bounds.y / 2;

		renderer.skeletonDebugRenderer.drawRegionAttachments = false;
		renderer.skeletonDebugRenderer.drawMeshHull = false;
		renderer.skeletonDebugRenderer.drawMeshTriangles = false;

		setupUI();
		setupInput();
	}

	function setupUI() {
		var rotationOffset = $("#transforms-rotationoffset").data("slider");
		rotationOffset.changed = function (percent) {
			var val = percent * 360 - 180;
			var delta = val - lastOffset;
			lastOffset = val;
			skeleton.findTransformConstraint("wheel2").data.offsetRotation += delta;
			skeleton.findTransformConstraint("wheel3").data.offsetRotation += delta;
			$("#transforms-rotationoffset-label").text(Math.round(val) + "°");
		};
		$("#transforms-rotationoffset-label").text("0°");

		var translationMix = $("#transforms-translationmix").data("slider");
		translationMix.set(0.5);
		translationMix.changed = function (percent) {
			var val = percent;
			var delta = val - lastMix;
			lastMix = val;
			var constraint = skeleton.findTransformConstraint("wheel1");
			constraint.mixX += delta;
			constraint.mixY += delta;
			$("#transforms-translationmix-label").text(Math.round(val * 100) + "%");
		};
		$("#transforms-translationmix-label").text("50%");
	}

	function setupInput() {
		var getRotation = function (x, y) {
			renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.clientWidth, canvas.clientHeight);
			var wheel1 = skeleton.findBone("wheel1overlay");
			var v = coords.sub(new spine.Vector3(wheel1.worldX, wheel1.worldY, 0)).normalize();
			var angle = Math.acos(v.x) * spine.MathUtils.radiansToDegrees;
			if (v.y < 0) angle = 360 - angle;
			return angle;
		}
		input.addListener({
			down: function (x, y) {
				target = spineDemos.closest(canvas, renderer, skeleton, controlBones, hoverTargets, x, y);
				if (target === rotateHandle) lastRotation = getRotation(x, y);
			},
			up: function (x, y) {
				target = null;
			},
			dragged: function (x, y) {
				if (target === rotateHandle) {
					var rotation = getRotation(x, y);
					var delta = rotation - lastRotation;
					skeleton.findBone("wheel1").rotation += delta;
					lastRotation = rotation;
				} else
					spineDemos.dragged(canvas, renderer, target, x, y);
			},
			moved: function (x, y) {
				spineDemos.closest(canvas, renderer, skeleton, controlBones, hoverTargets, x, y);
			}
		})
	}

	function render() {
		timeKeeper.update();
		var delta = timeKeeper.delta;
		skeleton.updateWorldTransform();

		renderer.camera.viewportWidth = bounds.x * 1.6;
		renderer.camera.viewportHeight = bounds.y * 1.2;
		renderer.resize(spine.ResizeMode.Fit);

		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);

		renderer.begin();
		renderer.drawSkeleton(skeleton, true);
		renderer.drawSkeletonDebug(skeleton, false, ["root", "rotate-handle"]);
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
	}

	init();
	transformsDemo.assetManager = assetManager;
	transformsDemo.loadingComplete = loadingComplete;
	transformsDemo.render = render;
};