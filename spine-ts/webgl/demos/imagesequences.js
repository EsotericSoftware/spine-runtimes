var imageSequencesDemo = function(loadingComplete, bgColor) {
	var OUTLINE_COLOR = new spine.Color(0, 0.8, 0, 1);	

	var canvas, gl, renderer, input, assetManager;
	var skeleton, bounds;		
	var timeKeeper, loadingScreen;
	var skeletons = {};
	var activeSkeleton = "Alien";
	var playButton, timeLine, isPlaying = true;

	var DEMO_NAME = "ImageSequenceDemo";

	if (!bgColor) bgColor = new spine.Color(0, 0, 0, 1);

	function init () {
		canvas = document.getElementById("imagesequencesdemo-canvas");
		canvas.width = canvas.clientWidth; canvas.height = canvas.clientHeight;
		gl = canvas.getContext("webgl", { alpha: false }) || canvas.getContext("experimental-webgl", { alpha: false });	

		renderer = new spine.webgl.SceneRenderer(canvas, gl);
		assetManager = spineDemos.assetManager;
		var textureLoader = function(img) { return new spine.webgl.GLTexture(gl, img); };	
		assetManager.loadTexture(DEMO_NAME, textureLoader, "atlas1.png");
		assetManager.loadText(DEMO_NAME, "atlas1.atlas");
		assetManager.loadJson(DEMO_NAME, "demos.json");		
		timeKeeper = new spine.TimeKeeper();		
		loadingScreen = new spine.webgl.LoadingScreen(renderer);
		loadingScreen.backgroundColor = bgColor;
		requestAnimationFrame(load);
	}	

	function load () {
		timeKeeper.update();
		if (assetManager.isLoadingComplete(DEMO_NAME)) {
			skeletons["Alien"] = loadSkeleton("alien", "death", ["head", "splat01"]);
			skeletons["Dragon"] = loadSkeleton("dragon", "flying", ["R_wing"])
			setupUI();
			loadingComplete(canvas, render);			
		} else {
			loadingScreen.draw();
			requestAnimationFrame(load);
		}
	}

	function setupUI() {
		playButton = $("#imagesequencesdemo-playbutton");
		var playButtonUpdate = function () {			
			isPlaying = !isPlaying;
			if (isPlaying) {
				playButton.val("Pause");
				playButton.addClass("pause").removeClass("play");		
			} else {
				playButton.val("Play");
				playButton.addClass("play").removeClass("pause");
			}			
		}
		playButton.click(playButtonUpdate);

		timeLine = $("#imagesequencesdemo-timeline");
		timeLine.slider({ range: "max", min: 0, max: 100, value: 0, slide: function () {
			if (isPlaying) playButton.click();		
			if (!isPlaying) {
				var active = skeletons[activeSkeleton];
				var time = timeLine.slider("value") / 100;
				var animationDuration = active.state.getCurrent(0).animation.duration;
				time = animationDuration * time;
				active.state.update(time - active.playTime);
				active.state.apply(active.skeleton);
				active.skeleton.updateWorldTransform();
				active.playTime = time;				
			}
		}});

		var list = $("#imagesequencesdemo-active-skeleton");	
		for (var skeletonName in skeletons) {
			var option = $("<option></option>");
			option.attr("value", skeletonName).text(skeletonName);
			if (skeletonName === activeSkeleton) option.attr("selected", "selected");
			list.append(option);
		}
		list.change(function() {
			activeSkeleton = $("#imagesequencesdemo-active-skeleton option:selected").text();
			var active = skeletons[activeSkeleton];
			var animationDuration = active.state.getCurrent(0).animation.duration;
			timeLine.slider("value", (active.playTime / animationDuration * 100));
		})
	}

	function loadSkeleton(name, animation, sequenceSlots) {
		var atlas = new spine.TextureAtlas(assetManager.get(DEMO_NAME, "atlas1.atlas"), function(path) {
			return assetManager.get(DEMO_NAME, path);		
		});
		var atlasLoader = new spine.TextureAtlasAttachmentLoader(atlas);
		var skeletonJson = new spine.SkeletonJson(atlasLoader);
		var skeletonData = skeletonJson.readSkeletonData(assetManager.get(DEMO_NAME, "demos.json")[name]);
		var skeleton = new spine.Skeleton(skeletonData);
		skeleton.setSkinByName("default");

		var state = new spine.AnimationState(new spine.AnimationStateData(skeletonData));
		var anim = skeletonData.findAnimation(animation);		
		state.setAnimation(0, animation, true);
		if (name === "alien") {
			state.update(anim.duration / 2.5);			
		}
		state.apply(skeleton);
		skeleton.updateWorldTransform();			
		var offset = new spine.Vector2();
		var size = new spine.Vector2();
		skeleton.getBounds(offset, size);
		if (name === "alien") {
			state.update(-anim.duration / 2.5);
		}

		var regions = [];
		for(var i = 0; i < sequenceSlots.length; i++) {
			var slot = sequenceSlots[i];
			var index = skeleton.findSlotIndex(slot);			
			for (var name in skeleton.skin.attachments[index]) {
				regions.push(skeleton.skin.attachments[index][name]);	
			}
		}

		return {
			atlas: atlas,
			skeleton: skeleton, 
			state: state, 
			playTime: 0,
			bounds: {
				offset: offset,
				size: size
			},
			slots: sequenceSlots,
			regions: regions
		};
	}

	function render () {
		timeKeeper.update();
		var delta = timeKeeper.delta;	

		var active = skeletons[activeSkeleton];
		var skeleton = active.skeleton;
		var state = active.state;
		var offset = active.bounds.offset;
		var size = active.bounds.size;

		renderer.camera.position.x = offset.x + size.x;
		renderer.camera.position.y = offset.y + size.y / 2;
		renderer.camera.viewportWidth = size.x * 2.4;
		renderer.camera.viewportHeight = size.y * 1.4;
		renderer.resize(spine.webgl.ResizeMode.Fit);

		gl.clearColor(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
		gl.clear(gl.COLOR_BUFFER_BIT);

		if (isPlaying) {
			var animationDuration = state.getCurrent(0).animation.duration;
			active.playTime += delta;			
			while (active.playTime >= animationDuration) {
				active.playTime -= animationDuration;
			}
			timeLine.slider("value", (active.playTime / animationDuration * 100));

			state.update(delta);
			state.apply(skeleton);
			skeleton.updateWorldTransform();
		}

		renderer.begin();				
		renderer.drawSkeleton(skeleton, true);

		var x = offset.x + size.x + 100;
		var y = offset.y;
		var slotsWidth = 0, slotsHeight = 0;
		var slotSize = size.y / 3;
		var maxSlotWidth = 0;	
		var j = 0;
		for (var i = 0; i < active.regions.length; i++) {			
			var region = active.regions[i].region;
			var scale = Math.min(slotSize / region.height, slotSize / region.width);
			renderer.drawRegion(region, x,  y, region.width * scale, region.height * scale);

			var isVisible = false;
			for (var ii = 0; ii < active.slots.length; ii++) {
				var slotName = active.slots[ii];
				var slotIndex = skeleton.findSlotIndex(slotName);
				
				for (var iii = 0; iii < skeleton.drawOrder.length; iii++) {
					var slot = skeleton.drawOrder[iii];
					if (slot.data.index == slotIndex) {
						if (slot.attachment != null) {
							if (slot.attachment.name === active.regions[i].name) {
								isVisible = true;
								break;
							}
						}
					}
				}
				if (isVisible) break;
			}

			if (isVisible) renderer.rect(false, x, y, region.width * scale, region.height * scale, OUTLINE_COLOR);

			maxSlotWidth = Math.max(maxSlotWidth, region.width * scale);
			y += slotSize;
			j++;
			if (j == 3) {
				x += maxSlotWidth + 10;
				maxSlotWidth = 0;
				y = offset.y;
				j = 0;
			}			
		}

		renderer.end();		
	}

	init();
};