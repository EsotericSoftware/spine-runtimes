var additiveBlendingDemo = function(canvas, bgColor) {
	var COLOR_INNER = new spine.Color(0.8, 0, 0, 0.5);
	var COLOR_OUTER = new spine.Color(0.8, 0, 0, 0.8);
	var COLOR_INNER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.5);
	var COLOR_OUTER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.8);
	var HANDLE_SIZE = 0.10;

	var gl, renderer, input, assetManager;
	var skeleton, state, bounds;
	var timeKeeper, loadingScreen;
	var target = null;
	var dragging = false;
	var handle = new spine.Vector2();
	var coords = new spine.webgl.Vector3(), temp = new spine.webgl.Vector3(), temp2 = new spine.Vector2(), temp3 = new spine.webgl.Vector3();
	var isPlaying = true;

	var left, right, up, down;
	var cursor;

	var clientMouseX = 0, clientMouseY = 0;

	var DEMO_NAME = "AdditiveBlendingDemo";

	if (!bgColor) bgColor = new spine.Color(235 / 255, 239 / 255, 244 / 255, 1);

	function isMobileDevice() {
		return (typeof window.orientation !== "undefined") || (navigator.userAgent.indexOf('IEMobile') !== -1);
	};

	function init () {
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.ctx.gl;

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = spineDemos.assetManager;
		var textureLoader = function(img) { return new spine.webgl.GLTexture(gl, img); };
		assetManager.loadTexture(DEMO_NAME, textureLoader, "atlas2.png");
		assetManager.loadText(DEMO_NAME, "atlas2.atlas");
		assetManager.loadJson(DEMO_NAME, "demos.json");
		timeKeeper = new spine.TimeKeeper();

		cursor = document.getElementById("cursor");
	}

	function loadingComplete () {
		var atlas = new spine.TextureAtlas(assetManager.get(DEMO_NAME, "atlas2.atlas"), function(path) {
			return assetManager.get(DEMO_NAME, path);
		});
		var atlasLoader = new spine.AtlasAttachmentLoader(atlas);
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get(DEMO_NAME, "demos.json")["owl"]);
		skeleton = new spine.Skeleton(skeletonData);
		state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));

		state.setAnimation(0, "idle", true);
		left = state.setAnimation(1, "blink", true);

		left = state.setAnimation(2, "left", true);
		right = state.setAnimation(3, "right", true);
		up = state.setAnimation(4, "up", true);
		down = state.setAnimation(5, "down", true);


		left.mixBlend = spine.MixBlend.add;
		left.alpha = 0;
		right.mixBlend = spine.MixBlend.add;
		right.alpha = 0;
		up.mixBlend = spine.MixBlend.add;
		up.alpha = 0;
		down.mixBlend = spine.MixBlend.add;
		down.alpha = 0;

		state.apply(skeleton);
		skeleton.updateWorldTransform();
		var offset = new spine.Vector2();
		bounds = new spine.Vector2();
		skeleton.getBounds(offset, bounds, []);

		renderer.camera.position.x = offset.x + bounds.x / 2;
		renderer.camera.position.y = offset.y + bounds.y / 2;

		renderer.skeletonDebugRenderer.drawMeshHull = false;
		renderer.skeletonDebugRenderer.drawMeshTriangles = false;

		setupInput();
	}

	function calculateBlend (x, y, isPageCoords) {
		if (isPageCoords) {
			var canvasBounds = canvas.getBoundingClientRect();
			x = Math.max(0, Math.min(canvasBounds.width, x - canvasBounds.x));
			y = Math.max(0, Math.min(canvasBounds.height, y - canvasBounds.y));
		}
		x = x / canvas.width;
		y = y / canvas.height;
		if (x > 1) x = 1;
		if (x < 0) x = 0;
		if (y > 1) y = 1;
		if (y < 0) y = 0;
		left.alpha = (Math.max(x, 0.5) - 0.5) * 2;
		right.alpha = (0.5 - Math.min(x, 0.5)) * 2;
		up.alpha = (0.5 - Math.min(y, 0.5)) * 2;
		down.alpha = (Math.max(y, 0.5) - 0.5) * 2;
	}

	function setupInput () {
		if (!isMobileDevice()) {
			document.addEventListener("mousemove", function (event) {
				clientMouseX = event.clientX;
				clientMouseY = event.clientY;
			}, false);
		} else {
			var input = new spine.webgl.Input(canvas);
			input.addListener({
				down: function(x, y) {
					renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
					if (temp.set(handle.x, handle.y, 0).distance(coords) < canvas.width * HANDLE_SIZE) {
						dragging = true;
					}
				},
				up: function(x, y) {
					dragging = false;
				},
				dragged: function(x, y) {
					if (dragging && x > 0 && x < canvas.width && y > 0 && y < canvas.height) {
						renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
						handle.x = coords.x;
						handle.y = coords.y;
						calculateBlend(x, y, false);
					}
				}
			});
		}
	}

	function render () {
		if (!isMobileDevice()) {
			calculateBlend(clientMouseX, clientMouseY, true);
		}

		timeKeeper.update();
		var delta = timeKeeper.delta;

		state.update(delta);
		state.apply(skeleton);
		skeleton.updateWorldTransform();

		renderer.camera.viewportWidth = bounds.x * 1.4;
		renderer.camera.viewportHeight = bounds.y * 1.4;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);

		renderer.begin();
		renderer.drawSkeleton(skeleton, true);

		if (isMobileDevice()) {
			gl.lineWidth(2);
			renderer.circle(true, handle.x, handle.y, canvas.width * HANDLE_SIZE, COLOR_INNER);
			renderer.circle(false, handle.x, handle.y, canvas.width * HANDLE_SIZE, COLOR_OUTER);
			gl.lineWidth(1);
		}

		renderer.end();
	}

	additiveBlendingDemo.loadingComplete = loadingComplete;
	additiveBlendingDemo.render = render;
	additiveBlendingDemo.DEMO_NAME = DEMO_NAME;
	init();
};