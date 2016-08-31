var ikConstraintDemo = function(pathPrefix, loadingComplete) {
	var canvas, gl, renderer, input, assetManager;
	var skeleton, bounds;		
	var lastFrameTime = Date.now() / 1000;
	var target = null;
	var isHover = false;
	var boneName = "hip";
	var coords = new spine.webgl.Vector3(), temp = new spine.webgl.Vector3(), temp2 = new spine.Vector2();		

	function init () {

		canvas = document.getElementById("ikdemo-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = new spine.webgl.AssetManager(gl, pathPrefix);
		input = new spine.webgl.Input(canvas);		
		assetManager.loadTexture("spineboy.png");
		assetManager.loadText("spineboy-mesh.json");
		assetManager.loadText("spineboy.atlas");
		requestAnimationFrame(load);
	}

	function load () {
		if (assetManager.isLoadingComplete()) {
			var atlas = new spine.TextureAtlas(assetManager.get("spineboy.atlas"), function(path) {
				return assetManager.get(path);		
			});
			var atlasLoader = new spine.TextureAtlasAttachmentLoader(atlas);
			var skeletonJson = new spine.SkeletonJson(atlasLoader);
			var skeletonData = skeletonJson.readSkeletonData(assetManager.get("spineboy-mesh.json"));
			skeleton = new spine.Skeleton(skeletonData);
			skeleton.setToSetupPose();
			skeleton.updateWorldTransform();
			var offset = new spine.Vector2();
			bounds = new spine.Vector2();
			skeleton.getBounds(offset, bounds);

			renderer.camera.position.x = offset.x + bounds.x / 2;
			renderer.camera.position.y = offset.y + bounds.y / 2;

			setupInput();

			loadingComplete(canvas, render);
		} else requestAnimationFrame(load);
	}

	function setupInput() {
		input.addListener({
			down: function(x, y) {			
				var bone = skeleton.findBone(boneName);				
				renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);				
				if (temp.set(skeleton.x + bone.worldX, skeleton.y + bone.worldY, 0).distance(coords) < 20) {
					target = bone;
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
				var bone = skeleton.findBone(boneName);				
				renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.width, canvas.height);				
				isHover = temp.set(skeleton.x + bone.worldX, skeleton.y + bone.worldY, 0).distance(coords) < 20;					
			}
		});
	}

	function render () {
		var now = Date.now() / 1000;
		var delta = now - lastFrameTime;
		lastFrameTime = now;
		if (delta > 0.032) delta = 0.032;

		renderer.camera.viewportWidth = bounds.x * 1.2;
		renderer.camera.viewportHeight = bounds.y * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(0.2, 0.2, 0.2, 1);
		gl.clear(gl.COLOR_BUFFER_BIT);

		skeleton.updateWorldTransform();

		renderer.begin();				
		renderer.drawSkeleton(skeleton, true);
		var bone = skeleton.findBone(boneName);

		var colorInner = isHover ? spineDemos.HOVER_COLOR_INNER : spineDemos.NON_HOVER_COLOR_INNER;
		var colorOuter = isHover ? spineDemos.HOVER_COLOR_OUTER : spineDemos.NON_HOVER_COLOR_OUTER;

		renderer.circle(true, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorInner);
		gl.lineWidth(2);
		renderer.circle(false, skeleton.x + bone.worldX, skeleton.y + bone.worldY, 20, colorOuter);			
		renderer.end();
		gl.lineWidth(1);
	}
	init();
};