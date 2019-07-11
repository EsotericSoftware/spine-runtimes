var skinsDemo = function(canvas, bgColor) {
	var canvas, gl, renderer, input, assetManager;
	var skeleton, state, offset, bounds;
	var timeKeeper, loadingScreen;
	var playButton, timeLine, isPlaying = true, playTime = 0;
	var randomizeSkins, lastSkinChange = Date.now() / 1000, clickAnim = 0;

	var DEMO_NAME = "SkinsDemo";

	if (!bgColor) bgColor = new spine.Color(235 / 255, 239 / 255, 244 / 255, 1);

	function init () {
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.ctx.gl;

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = spineDemos.assetManager;
		var textureLoader = function(img) { return new spine.webgl.GLTexture(gl, img); };
		assetManager.loadTexture(DEMO_NAME, textureLoader, "heroes.png");
		assetManager.loadTexture(DEMO_NAME, textureLoader, "heroes2.png");
		assetManager.loadText(DEMO_NAME, "heroes.atlas");
		assetManager.loadJson(DEMO_NAME, "demos.json");
		input = new spine.webgl.Input(canvas);
		timeKeeper = new spine.TimeKeeper();
	}

	function loadingComplete () {
		var atlas = new spine.TextureAtlas(assetManager.get(DEMO_NAME, "heroes.atlas"), function(path) {
			return assetManager.get(DEMO_NAME, path);
		});
		var atlasLoader = new spine.AtlasAttachmentLoader(atlas);
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get(DEMO_NAME, "demos.json").heroes);
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
		skeleton.getBounds(offset, bounds, []);
		setupUI();
		setupInput();
	}

	function setupInput (){
		input.addListener({
			down: function(x, y) {
				swingSword();
			},
			up: function(x, y) { },
			dragged: function(x, y) { },
			moved: function (x, y) { }
		});
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
		state.addAnimation(0, "crouchIdle", true, 2.5).listener = {
			start: function (trackIndex) {
				setupAnimations(state);
			}
		};

		state.setAnimation(1, "empty", false, 0);
		state.setAnimation(1, "hideSword", false, 2);
	}

	function setupUI() {
		var list = $("#skins-skin");
		for (var skin in skeleton.data.skins) {
			skin = skeleton.data.skins[skin];
			if (skin.name == "default") continue;
			var option = $("<option></option>");
			option.attr("value", skin.name).text(skin.name);
			if (skin.name === "Assassin") {
				option.attr("selected", "selected");
				skeleton.setSkinByName("Assassin");
			}
			list.append(option);
		}
		list.change(function() {
			activeSkin = $("#skins-skin option:selected").text();
			skeleton.setSkinByName(activeSkin);
			skeleton.setSlotsToSetupPose();
			randomizeSkins.checked = false;
		});

		$("#skins-randomizeattachments").click(randomizeAttachments);
		$("#skins-swingsword").click(swingSword);
		randomizeSkins = document.getElementById("skins-randomizeskins");
	}

	function setSkin (skin) {
		var slot = skeleton.findSlot("item_near");
		var weapon = slot.getAttachment();
		skeleton.setSkin(skin);
		skeleton.setSlotsToSetupPose();
		slot.setAttachment(weapon);
	}

	function swingSword () {
		state.setAnimation(5, (clickAnim++ % 2 == 0) ? "meleeSwing2" : "meleeSwing1", false, 0);
	}

	function randomizeSkin () {
		var result;
		var count = 0;
		for (var skin in skeleton.data.skins) {
			if (skeleton.data.skins[skin].name === "default") continue;
			if (Math.random() < 1/++count) {
				result = skeleton.data.skins[skin];
			}
		}
		setSkin(result);
		$("#skins-skin option").filter(function() {
			return ($(this).text() == result.name);
		}).prop("selected", true);
	}

	function randomizeAttachments () {
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
				newSkin.setAttachment(slot, attachmentName, attachments[attachmentName]);
			}
		}
		setSkin(newSkin);
		randomizeSkins.checked = false;
	}

	function render () {
		timeKeeper.update();
		var delta = timeKeeper.delta;

		if (randomizeSkins.checked) {
			var now = Date.now() / 1000;
			if (now - lastSkinChange > 2) {
				randomizeSkin();
				lastSkinChange = now;
			}
		}

		renderer.camera.position.x = offset.x + bounds.x * 1.5 - 125;
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
		var texture = assetManager.get(DEMO_NAME, "heroes.png");
		var width = bounds.x * 1.25;
		var scale = width / texture.getImage().width;
		var height = scale * texture.getImage().height;
		renderer.drawTexture(texture, offset.x + bounds.x + 190, offset.y + bounds.y / 2 - height / 2 - 5, width, height);
		renderer.end();
	}

	skinsDemo.loadingComplete = loadingComplete;
	skinsDemo.render = render;
	skinsDemo.DEMO_NAME = DEMO_NAME;
	init();
};