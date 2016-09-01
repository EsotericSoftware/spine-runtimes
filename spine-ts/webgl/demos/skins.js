var skinsDemo = function(pathPrefix, loadingComplete, bgColor) {	
	var canvas, gl, renderer, input, assetManager;
	var skeleton, state, offset, bounds;		
	var timeKeeper, loadingScreen;
	var playButton, timeLine, isPlaying = true, playTime = 0;

	if (!bgColor) bgColor = new spine.Color(0, 0, 0, 1);		

	function init () {
		if (pathPrefix === undefined) pathPrefix = "";		

		canvas = document.getElementById("skinsdemo-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);		
		assetManager = new spine.webgl.AssetManager(gl, pathPrefix);		
		assetManager.loadTexture("heroes.png");
		assetManager.loadText("heroes.json");
		assetManager.loadText("heroes.atlas");
		timeKeeper = new spine.TimeKeeper();		
		loadingScreen = new spine.webgl.LoadingScreen(renderer);
		loadingScreen.backgroundColor = bgColor;
		requestAnimationFrame(load);
	}

	function load () {
		timeKeeper.update();
		if (assetManager.isLoadingComplete()) {
			var atlas = new spine.TextureAtlas(assetManager.get("heroes.atlas"), function(path) {
				return assetManager.get(path);		
			});
			var atlasLoader = new spine.TextureAtlasAttachmentLoader(atlas);
			var skeletonJson = new spine.SkeletonJson(atlasLoader);
			var skeletonData = skeletonJson.readSkeletonData(assetManager.get("heroes.json"));
			skeleton = new spine.Skeleton(skeletonData);
			skeleton.setSkinByName("Assassin");
			var stateData = new spine.AnimationStateData(skeleton.data);
			stateData.defaultMix = 0.2;
			stateData.setMix("roll", "run", 0);
			stateData.setMix("jump", "run2", 0);						
			state = new spine.AnimationState(stateData);			
			setupAnimations(state);			
			state.apply(skeleton);
			skeleton.updateWorldTransform();
			offset = new spine.Vector2();
			bounds = new spine.Vector2();
			skeleton.getBounds(offset, bounds);
			setupUI();
			loadingComplete(canvas, render);
		} else {
			loadingScreen.draw();
			requestAnimationFrame(load);
		}
	}

	function setupAnimations(state) {
		state.addAnimation(0, "idle", true, 1);
		state.addAnimation(0, "walk", true, 2);
		state.addAnimation(0, "run", true, 4);
		state.addAnimation(0, "roll", false, 3);
		state.addAnimation(0, "run", true, 0);
		state.addAnimation(0, "run2", true, 1.5);
		state.addAnimation(0, "jump", false, 3);
		state.addAnimation(0, "run2", true, 0);
		state.addAnimation(0, "run", true, 1);
		state.addAnimation(0, "idle", true, 3);
		state.addAnimation(0, "idleTired", true, 0.5);
		state.addAnimation(0, "idle", true, 2);
		state.addAnimation(0, "walk2", true, 1);
		state.addAnimation(0, "block", true, 3);
		state.addAnimation(0, "punch1", false, 1.5);
		state.addAnimation(0, "block", true, 0);
		state.addAnimation(0, "punch1", false, 1.5);
		state.addAnimation(0, "punch2", false, 0);
		state.addAnimation(0, "block", true, 0);
		state.addAnimation(0, "hitBig", false, 1.5);
		state.addAnimation(0, "floorIdle", true, 0);
		state.addAnimation(0, "floorGetUp", false, 1.5);
		state.addAnimation(0, "idle", true, 0);
		state.addAnimation(0, "meleeSwing1-fullBody", false, 1.5);
		state.addAnimation(0, "idle", true, 0);
		state.addAnimation(0, "meleeSwing2-fullBody", false, 1.5);
		state.addAnimation(0, "idle", true, 0);
		state.addAnimation(0, "idleTired", true, 0.5);
		state.addAnimation(0, "crouchIdle", true, 1.5);
		state.addAnimation(0, "crouchWalk", true, 2);
		state.addAnimation(0, "crouchIdle", true, 2.5);

		state.addAnimation(1, "meleeSwing1", false, 4);

		state.addAnimation(2, "meleeSwing1", false, 7.5);

		state.addAnimation(3, "meleeSwing2", false, 10.5);
		state.addAnimation(3, "meleeSwing1", false, 0);
		state.addAnimation(3, "meleeSwing2", false, 0);

		state.addAnimation(4, "hideSword", false, 19.15).listener = {
			event: function (trackIndex, event) {},
			complete: function (trackIndex, loopCount) {},
			start: function (trackIndex) { 
				setAnimations(state);
			},
			end: function (trackIndex) {}
		};
	}

	function setupUI() {
		var list = $("#skinsdemo-active-skin");	
		for (var skin in skeleton.data.skins) {
			skin = skeleton.data.skins[skin];
			var option = $("<option></option>");
			option.attr("value", skin.name).text(skin.name);
			if (skin.name === "Assassin") {
				option.attr("selected", "selected");
				skeleton.setSkinByName("Assassin");
			}
			list.append(option);
		}
		list.change(function() {
			activeSkin = $("#skinsdemo-active-skin option:selected").text();
			skeleton.setSkinByName(activeSkin);
			skeleton.setSlotsToSetupPose();
		});

		var randomSkin = $("#skinsdemo-randomizeskin");
		randomSkin.click(function() {
			var result;
			var count = 0;
			for (var skin in skeleton.data.skins) {
				if (skeleton.data.skins[skin].name === "default") continue;
				if (Math.random() < 1/++count) {
					result = skeleton.data.skins[skin];
				}
			}
			skeleton.setSkin(result);
			skeleton.setSlotsToSetupPose();
			$("#skinsdemo-active-skin select").val(result.name);
		});

		var randomizeAttachments = $("#skinsdemo-randomizeattachments");
		randomizeAttachments.click(function() {
			var skins = [];
			for (var skin in skeleton.data.skins) {
				skin = skeleton.data.skins[skin];
				if (skin.name === "default") continue;
				skins.push(skin);
			}

			var newSkin = new spine.Skin("random-skin");
			for (var slot = 0; slot < skeleton.slots.length; slot++) {
				var skin = skins[(Math.random() * skins.length - 1) | 0];
				var attachments = skin.attachments[slot];
				for (var attachmentName in attachments) {
					newSkin.addAttachment(slot, attachmentName, attachments[attachmentName]);
				}
			}
			skeleton.setSkin(newSkin);
			skeleton.setSlotsToSetupPose();			
		});
	}

	function render () {
		timeKeeper.update();
		var delta = timeKeeper.delta;

		renderer.camera.position.x = offset.x + bounds.x * 1.5 - 150;
		renderer.camera.position.y = offset.y + bounds.y / 2;
		renderer.camera.viewportWidth = bounds.x * 3;
		renderer.camera.viewportHeight = bounds.y * 1.2;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);			

		state.update(delta);
		state.apply(skeleton);
		skeleton.updateWorldTransform();		

		renderer.begin();				
		renderer.drawSkeleton(skeleton, true);
		var texture = assetManager.get("heroes.png");
		var width = bounds.x;
		var scale = width / texture.getImage().width;
		var height = scale * texture.getImage().height;
		renderer.drawTexture(texture, offset.x + bounds.x + 300, offset.y + bounds.y / 2 - height / 2 - 50, width, height);		
		renderer.end();		
	}

	init();
};