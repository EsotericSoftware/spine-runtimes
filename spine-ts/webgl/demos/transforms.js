var transformsDemo = function(canvas, bgColor) {
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
	var coords = new spine.webgl.Vector3(), temp = new spine.webgl.Vector3(), temp2 = new spine.Vector2();
	var lastRotation = 0;
	var mix, lastOffset = 0, lastMix = 0.5;

	var DEMO_NAME = "TransformsDemo";

	if (!bgColor) bgColor = new spine.Color(235 / 255, 239 / 255, 244 / 255, 1);

	function init () {
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.ctx.gl;

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = spineDemos.assetManager;
		var textureLoader = function(img) { return new spine.webgl.GLTexture(gl, img); };
		assetManager.loadTexture(DEMO_NAME, textureLoader, "atlas2.png");
		assetManager.loadText(DEMO_NAME, "atlas2.atlas");
		assetManager.loadJson(DEMO_NAME, "demos.json");
		input = new spine.webgl.Input(canvas);
		timeKeeper = new spine.TimeKeeper();
	}

	function loadingComplete () {
		var atlas = new spine.TextureAtlas(assetManager.get(DEMO_NAME, "atlas2.atlas"), function(path) {
			return assetManager.get(DEMO_NAME, path);
		});
		var atlasLoader = new spine.AtlasAttachmentLoader(atlas);
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get(DEMO_NAME, "demos.json").transforms);
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
			skeleton.findTransformConstraint("wheel1").translateMix += delta;
			$("#transforms-translationmix-label").text(Math.round(val * 100) + "%");
		};
		$("#transforms-translationmix-label").text("50%");
	}

	function setupInput() {
		var getRotation = function(x, y) {
			renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
			var wheel1 = skeleton.findBone("wheel1overlay");
			var v = coords.sub(new spine.webgl.Vector3(wheel1.worldX, wheel1.worldY, 0)).normalize();
			var angle = Math.acos(v.x) * spine.MathUtils.radiansToDegrees;
			if (v.y < 0) angle = 360 - angle;
			return angle;
		}
		input.addListener({
			down: function(x, y) {
				renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
				for (var i = 0; i < controlBones.length; i++) {
					var bone = skeleton.findBone(controlBones[i]);
					if (temp.set(skeleton.x + bone.worldX, skeleton.y + bone.worldY, 0).distance(coords) < 30) {
						target = bone;
						if (target === rotateHandle) lastRotation = getRotation(x, y);
					}
				}
			},
			up: function(x, y) {
				target = null;
			},
			dragged: function(x, y) {
				if (target != null && x > 0 && x < canvas.width && y > 0 && y < canvas.height) {
					if (target === rotateHandle) {
						var rotation = getRotation(x, y);
						var delta = rotation - lastRotation;
						skeleton.findBone("wheel1").rotation += delta;
						lastRotation = rotation;
					} else {
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
				}
			 },
			moved: function (x, y) {
				renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
				for (var i = 0; i < controlBones.length; i++) {
					var bone = skeleton.findBone(controlBones[i]);
					if (temp.set(skeleton.x + bone.worldX, skeleton.y + bone.worldY, 0).distance(coords) < 30) {
						hoverTargets[i] = bone;
					} else {
						hoverTargets[i] = null;
					}
				}
			}
		})
	}

	function render () {
		timeKeeper.update();
		var delta = timeKeeper.delta;
		skeleton.updateWorldTransform();

		renderer.camera.viewportWidth = bounds.x * 1.6;
		renderer.camera.viewportHeight = bounds.y * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);

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

	transformsDemo.loadingComplete = loadingComplete;
	transformsDemo.render = render;
	transformsDemo.DEMO_NAME = DEMO_NAME;
	init();
};