(function() {	
	var canvas, gl, renderer, input, assetManager;
	var skeleton, animationState, offset, bounds;
	var skeletonAtlas;
	var frameAtlas;
	var viewportWidth, viewportHeight;
	var frames = [], currFrame = 0, frameTime = 0, frameScale = 0, FPS = 30;
	var lastFrameTime = Date.now() / 1000;
	var timeSlider, atlasCheckbox;

	var SKELETON_ATLAS_COLOR = new spine.Color(0, 0.8, 0, 0.8);
	var FRAME_ATLAS_COLOR = new spine.Color(0.8, 0, 0, 0.8);

	function init () {
		timeSlider = document.getElementById("skeletonvsspritedemo-timeslider");
		atlasCheckbox = document.getElementById("skeletonvsspritedemo-atlascheckbox");

		canvas = document.getElementById("skeletonvsspritedemo-canvas");
		canvas.width = window.innerWidth; canvas.height = window.innerHeight;	
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = new spine.webgl.AssetManager(gl);		
		assetManager.loadTexture("assets/raptor.png");
		assetManager.loadText("assets/raptor.json");
		assetManager.loadText("assets/raptor.atlas");
		assetManager.loadText("assets/raptor-walk.atlas");
		assetManager.loadTexture("assets/raptor-walk.png");
		assetManager.loadTexture("assets/raptor-walk2.png");
		assetManager.loadTexture("assets/raptor-walk3.png");
		assetManager.loadTexture("assets/raptor-walk4.png");
		requestAnimationFrame(load);
	}

	function load () {
		if (assetManager.isLoadingComplete()) {
			skeletonAtlas = new spine.TextureAtlas(assetManager.get("assets/raptor.atlas"), function(path) {
				return assetManager.get("assets/" + path);		
			});
			var atlasLoader = new spine.TextureAtlasAttachmentLoader(skeletonAtlas);
			var skeletonJson = new spine.SkeletonJson(atlasLoader);
			var skeletonData = skeletonJson.readSkeletonData(assetManager.get("assets/raptor.json"));
			skeleton = new spine.Skeleton(skeletonData);
			animationState = new spine.AnimationState(new spine.AnimationStateData(skeleton.data));
			animationState.setAnimation(0, "walk", true);
			animationState.apply(skeleton);
			skeleton.updateWorldTransform();
			offset = new spine.Vector2();
			bounds = new spine.Vector2();
			skeleton.getBounds(offset, bounds);

			frameAtlas = new spine.TextureAtlas(assetManager.get("assets/raptor-walk.atlas"), function(path) {
				return assetManager.get("assets/" + path);		
			});
			for (var i = 0; i < frameAtlas.regions.length - 1; i++) {
				frames.push(frameAtlas.findRegion("raptor-walk_" + i));
			}
			frameScale = bounds.x / frames[0].width * 1.1;

			viewportWidth = ((700 + bounds.x) - offset.x);
			viewportHeight = ((0 + bounds.y) - offset.y);

			renderer.camera.position.x = offset.x + viewportWidth / 2;
			renderer.camera.position.y = offset.y + viewportHeight / 2;			

			requestAnimationFrame(render);
		} else requestAnimationFrame(load);
	}

	function render () {
		var now = Date.now() / 1000;
		var delta = now - lastFrameTime;
		lastFrameTime = now;
		if (delta > 0.032) delta = 0.032;

		delta *= (timeSlider.value / 100);	

		if (!atlasCheckbox.checked) {			
			animationState.update(delta);
			animationState.apply(skeleton);
			skeleton.updateWorldTransform();

			frameTime += delta;
			if (frameTime > 1 / FPS) {
				frameTime -= 1 / FPS;
				currFrame++;
				if (currFrame >= frames.length) currFrame = 0;
			}
		}	

		renderer.camera.viewportWidth = viewportWidth * 1.2;
		renderer.camera.viewportHeight = viewportHeight * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);
		gl.clearColor(0.2, 0.2, 0.2, 1);
		gl.clear(gl.COLOR_BUFFER_BIT);	

		renderer.begin();
		if (!atlasCheckbox.checked) {
			var frame = frames[currFrame];
			renderer.drawRegion(frame, 700, offset.y - 40, frame.width * frameScale, frame.height * frameScale);	
			renderer.drawSkeleton(skeleton);
		} else {		
			var skeletonAtlasSize = skeletonAtlas.pages[0].texture.getImage().width;
			var frameAtlasSize = frameAtlas.pages[0].texture.getImage().width;
			var halfSpaceWidth = viewportWidth / 2;
			var halfSpaceHeight = viewportHeight;
			var pageSize = halfSpaceWidth / 2;															

			// we only have one page for skeleton
			var skeletonPageSize = pageSize * frameAtlasSize / skeletonAtlasSize;
			renderer.drawTexture(skeletonAtlas.pages[0].texture, offset.x + halfSpaceWidth - skeletonPageSize / 2,
								 offset.y + halfSpaceWidth - skeletonPageSize / 2, skeletonPageSize, skeletonPageSize);
			renderer.rect(skeletonAtlas.pages[0].texture, offset.x + halfSpaceWidth - skeletonPageSize / 2,
						  offset.y + halfSpaceWidth - skeletonPageSize / 2, skeletonPageSize, skeletonPageSize, SKELETON_ATLAS_COLOR);

			var x = offset.x + halfSpaceWidth;
			var y = offset.y + halfSpaceHeight / 2;
			var i = 0;
			for (var row = 0; row < frameAtlas.pages.length / 2; row++) {
				for (var col = 0; col < 2; col++) {
					var page = frameAtlas.pages[i++];
					renderer.drawTexture(page.texture, x + col * pageSize, y - row * pageSize, pageSize, pageSize);
					renderer.rect(false, x + col * pageSize, y - row * pageSize, pageSize, pageSize, FRAME_ATLAS_COLOR);
				}
			}			
		}
		renderer.end();

		requestAnimationFrame(render);
	}

	init();
})();