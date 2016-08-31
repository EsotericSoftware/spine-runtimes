var transformConstraintDemo = function(pathPrefix, loadingComplete) {
	var COLOR_INNER = new spine.Color(0.8, 0, 0, 0.5);
	var COLOR_OUTER = new spine.Color(0.8, 0, 0, 0.8);
	var COLOR_INNER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.5);
	var COLOR_OUTER_SELECTED = new spine.Color(0.0, 0, 0.8, 0.8);

	var canvas, gl, renderer, input, assetManager;
	var skeleton, state, bounds;		
	var lastFrameTime = Date.now() / 1000;
	var target = null;	
	var wheel1;
	var coords = new spine.webgl.Vector3(), temp = new spine.webgl.Vector3(), temp2 = new spine.Vector2();
	var lastRotation = 0;
	var rotationOffset, mix, lastOffset = 0;

	function init () {

		canvas = document.getElementById("transformdemo-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = new spine.webgl.AssetManager(gl, pathPrefix);
		input = new spine.webgl.Input(canvas);
		var getRotation = function(x, y) {
			renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);
			var v = coords.sub(new spine.webgl.Vector3(wheel1.worldX, wheel1.worldY, 0)).normalize();
			var angle = Math.acos(v.x) * spine.MathUtils.radiansToDegrees;
			if (v.y < 0) angle = 360 - angle;			
			return angle;
		}
		input.addListener({
			down: function(x, y) { 
				lastRotation = getRotation(x, y);
			},
			up: function(x, y) { },
			dragged: function(x, y) {
				var rotation = getRotation(x, y);
				var delta = rotation - lastRotation;
				wheel1.rotation += delta;
				lastRotation = rotation;
			 },
			moved: function (x, y) { }
		})
		assetManager.loadTexture("tank.png");
		assetManager.loadText("transformConstraint.json");
		assetManager.loadText("tank.atlas");
		requestAnimationFrame(load);
	}

	function load () {
		if (assetManager.isLoadingComplete()) {
			var atlas = new spine.TextureAtlas(assetManager.get("tank.atlas"), function(path) {
				return assetManager.get(path);		
			});
			var atlasLoader = new spine.TextureAtlasAttachmentLoader(atlas);
			var skeletonJson = new spine.SkeletonJson(atlasLoader);
			var skeletonData = skeletonJson.readSkeletonData(assetManager.get("transformConstraint.json"));
			skeleton = new spine.Skeleton(skeletonData);
			skeleton.setToSetupPose();
			skeleton.updateWorldTransform();
			var offset = new spine.Vector2();
			bounds = new spine.Vector2();
			skeleton.getBounds(offset, bounds);
			state = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
			skeleton.setToSetupPose();			
			skeleton.updateWorldTransform();
			wheel1 = skeleton.findBone("wheel1");		

			renderer.camera.position.x = offset.x + bounds.x / 2;
			renderer.camera.position.y = offset.y + bounds.y / 2;

			renderer.skeletonDebugRenderer.drawRegionAttachments = false;
			renderer.skeletonDebugRenderer.drawMeshHull = false;
			renderer.skeletonDebugRenderer.drawMeshTriangles = false;

			setupUI();

			loadingComplete(canvas, render);
		} else requestAnimationFrame(load);
	}

	function setupUI() {
		rotationOffset = $("#transformdemo-rotationoffset");
		rotationOffset.slider({ range: "max", min: -180, max: 180, value: 0, slide: function () {
			var val = rotationOffset.slider("value");
			var delta = val - lastOffset;
			lastOffset = val;
			skeleton.findTransformConstraint("wheel2").data.offsetRotation += delta;			
			skeleton.findTransformConstraint("wheel3").data.offsetRotation += delta;
			$("#transformdemo-rotationoffset-label").text(val + "°");
		}});	
	}

	function render () {
		var now = Date.now() / 1000;
		var delta = now - lastFrameTime;
		lastFrameTime = now;
		if (delta > 0.032) delta = 0.032;

		skeleton.updateWorldTransform();

		renderer.camera.viewportWidth = bounds.x * 1.2;
		renderer.camera.viewportHeight = bounds.y * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(0.2, 0.2, 0.2, 1);
		gl.clear(gl.COLOR_BUFFER_BIT);

		renderer.begin();				
		renderer.drawSkeleton(skeleton, true);
		renderer.drawSkeletonDebug(skeleton, false, ["root"]);				
		var bone = wheel1;
		var colorInner = bone === target ? COLOR_INNER_SELECTED : COLOR_INNER;
		var colorOuter = bone === target ? COLOR_OUTER_SELECTED : COLOR_OUTER;		
		renderer.circle(false, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorOuter);		
		renderer.end();		
	}

	init();
};