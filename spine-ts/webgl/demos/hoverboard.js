var hoverboardDemo = function(canvas, bgColor) {
	var COLOR_INNER = new spine.Color(0.8, 0, 0, 0.5);
	var COLOR_OUTER = new spine.Color(0.8, 0, 0, 0.8);
	var COLOR_INNER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.5);
	var COLOR_OUTER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.8);

	var gl, renderer, input, assetManager;
	var skeleton, state, bounds;
	var timeKeeper, loadingScreen;
	var target = null;
	var hoverTargets = [];
	var controlBones = ["hoverboard controller", "hip controller", "board target", "crosshair"];
	var coords = new spine.webgl.Vector3(), temp = new spine.webgl.Vector3(), temp2 = new spine.Vector2(), temp3 = new spine.webgl.Vector3();
	var isPlaying = true;

	var DEMO_NAME = "HoverboardDemo";

	if (!bgColor) bgColor = new spine.Color(235 / 255, 239 / 255, 244 / 255, 1);

	function init () {
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.ctx.gl;

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = spineDemos.assetManager;
		var textureLoader = function(img) { return new spine.webgl.GLTexture(gl, img); };
		assetManager.loadTexture(DEMO_NAME, textureLoader, "atlas1.png");
		assetManager.loadTexture(DEMO_NAME, textureLoader, "atlas12.png");
		assetManager.loadText(DEMO_NAME, "atlas1.atlas");
		assetManager.loadJson(DEMO_NAME, "demos.json");
		input = new spine.webgl.Input(canvas);
		timeKeeper = new spine.TimeKeeper();
	}

	function loadingComplete () {
		var atlas = new spine.TextureAtlas(assetManager.get(DEMO_NAME, "atlas1.atlas"), function(path) {
			return assetManager.get(DEMO_NAME, path);
		});
		var atlasLoader = new spine.AtlasAttachmentLoader(atlas);
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get(DEMO_NAME, "demos.json")["spineboy"]);
		skeleton = new spine.Skeleton(skeletonData);
		state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
		state.setAnimation(0, "hoverboard", true);
		state.apply(skeleton);
		skeleton.updateWorldTransform();
		var offset = new spine.Vector2();
		bounds = new spine.Vector2();
		skeleton.getBounds(offset, bounds, []);
		for (var i = 0; i < controlBones.length; i++)
			hoverTargets.push(null);

		renderer.camera.position.x = offset.x + bounds.x / 2;
		renderer.camera.position.y = offset.y + bounds.y / 2;

		renderer.skeletonDebugRenderer.drawMeshHull = false;
		renderer.skeletonDebugRenderer.drawMeshTriangles = false;

		setupUI();
		setupInput();
	}

	function setupUI () {
		var checkbox = $("#hoverboard-drawbones");
		renderer.skeletonDebugRenderer.drawRegionAttachments = false;
		renderer.skeletonDebugRenderer.drawPaths = false;
		renderer.skeletonDebugRenderer.drawBones = false;
		checkbox.change(function() {
			renderer.skeletonDebugRenderer.drawPaths = this.checked;
			renderer.skeletonDebugRenderer.drawBones = this.checked;
		});

		var aimTrack = 1;
		var shootAimTrack = 2;
		var shootTrack = 3;

		$("#hoverboard-aim").change(function () {
			if (!this.checked)
				state.setEmptyAnimation(aimTrack, 0.2);
			else {
				state.setEmptyAnimation(aimTrack, 0);
				state.addAnimation(aimTrack, "aim", true, 0).mixDuration = 0.2;
			}
		});

		$("#hoverboard-shoot").click(function () {
			state.setAnimation(shootAimTrack, "aim", true);
			state.setAnimation(shootTrack, "shoot", false).listener = {
				complete: function (trackIndex) {
					state.setEmptyAnimation(shootAimTrack, 0.2);
					state.clearTrack(shootTrack);
				}
			};
		});

		$("#hoverboard-jump").click(function () {
			state.setAnimation(aimTrack, "jump", false);
			state.addEmptyAnimation(aimTrack, 0.5, 0);
			if ($("#hoverboard-aim").prop("checked"))
				state.addAnimation(aimTrack, "aim", true, 0.4).mixDuration = 0.2;
		});
	}

	function setupInput () {
		input.addListener({
			down: function(x, y) {
				isPlaying = false;
				for (var i = 0; i < controlBones.length; i++) {
					var bone = skeleton.findBone(controlBones[i]);
					renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
					if (temp.set(bone.worldX, bone.worldY, 0).distance(coords) < 30) {
						target = bone;
					}
				}
			},
			up: function(x, y) {
				if (target && target.data.name == "crosshair") $("#hoverboard-shoot").click();
				target = null;
			},
			dragged: function(x, y) {
				if (target != null && x > 0 && x < canvas.width && y > 0 && y < canvas.height) {
					renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
					if (target.parent !== null) {
						target.parent.worldToLocal(temp2.set(coords.x, coords.y));
						target.x = temp2.x;
						target.y = temp2.y;
					} else {
						target.x = coords.x;
						target.y = coords.y;
					}
				}
			},
			moved: function (x, y) {
				for (var i = 0; i < controlBones.length; i++) {
					var bone = skeleton.findBone(controlBones[i]);
					renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
					if (temp.set(bone.worldX, bone.worldY, 0).distance(coords) < 30) {
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

		state.update(delta);
		state.apply(skeleton);
		skeleton.updateWorldTransform();

		renderer.camera.viewportWidth = bounds.x * 1.2;
		renderer.camera.viewportHeight = bounds.y * 1.2;
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
	}

	hoverboardDemo.loadingComplete = loadingComplete;
	hoverboardDemo.render = render;
	hoverboardDemo.DEMO_NAME = DEMO_NAME;
	init();
};