var spineDemos = {
	HOVER_COLOR_INNER: new spine.Color(0.478, 0, 0, 0.25),
	HOVER_COLOR_OUTER: new spine.Color(1, 1, 1, 1),
	NON_HOVER_COLOR_INNER: new spine.Color(0.478, 0, 0, 0.5),
	NON_HOVER_COLOR_OUTER: new spine.Color(1, 0, 0, 0.8),
	demos: [],
	loopRunning: false,
	canvases: [],
	downloader: new spine.Downloader(),
	path: "assets/"
};

window.onerror = function (msg, url, lineNo, columnNo, error) {
	var string = msg.toLowerCase();
	var substring = "script error";
	if (string.indexOf(substring) > -1)
		alert('Script Error: See Browser Console for Detail');
	else {
		var message = [
			'Message: ' + msg,
			'URL: ' + url,
			'Line: ' + lineNo,
			'Column: ' + columnNo,
			'Error object: ' + JSON.stringify(error)
		].join(' - ');

		alert(message);
	}
	return false;
};

(function () {
	var timeKeeper = new spine.TimeKeeper();
	function loop() {
		timeKeeper.update();
		if (spineDemos.log) console.log(timeKeeper.delta + ", " + timeKeeper.framesPerSecond);
		requestAnimationFrame(loop);
		var demos = spineDemos.demos;
		for (var i = 0; i < demos.length; i++) {
			var demo = demos[i];
			checkElementVisible(demo);
			renderDemo(demo);
		}
	}

	function renderDemo(demo) {
		if (demo.visible) {
			var canvas = demo.canvas;
			if (canvas.parentElement != demo.placeholder) {
				$(canvas).detach();
				demo.placeholder.appendChild(canvas);
			}
			let complete = demo.assetManager.isLoadingComplete();
			if (complete) {
				if (!demo.loaded) {
					demo.loaded = true;
					demo.loadingComplete();
				}
				if (spineDemos.log) console.log("Rendering: " + canvas.id);
				demo.render();
			}
			demo.loadingScreen.draw(complete);
		}
	}

	function checkElementVisible(demo) {
		const rect = demo.placeholder.getBoundingClientRect();
		const windowHeight = (window.innerHeight || document.documentElement.clientHeight);
		const windowWidth = (window.innerWidth || document.documentElement.clientWidth);
		const vertInView = (rect.top <= windowHeight * 1.1) && ((rect.top + rect.height) >= windowHeight * -0.1);
		const horInView = (rect.left <= windowWidth * 1.1) && ((rect.left + rect.width) >= windowWidth * -0.1);

		demo.visible = (vertInView && horInView);
	}

	function createCanvases(numCanvases) {
		for (var i = 0; i < numCanvases; i++) {
			var canvas = document.createElement("canvas");
			canvas.width = 1; canvas.height = 1;
			canvas.context = new spine.ManagedWebGLRenderingContext(canvas, { alpha: false });
			canvas.id = "canvas-" + i;
			spineDemos.canvases.push(canvas);
		}
	}

	spineDemos.init = function () {
		var numCanvases = 5;
		var isFirefox = navigator.userAgent.toLowerCase().indexOf('firefox') > -1;
		var isAndroid = navigator.userAgent.toLowerCase().indexOf("android") > -1;
		if (isFirefox && isAndroid) numCanvases = 2;
		createCanvases(numCanvases);
		loadSliders();
		requestAnimationFrame(loop);
	}

	spineDemos.addDemo = function (demo, placeholder) {
		var canvas = spineDemos.canvases[spineDemos.demos.length % spineDemos.canvases.length];
		demo(canvas);
		demo.placeholder = placeholder;
		demo.canvas = canvas;
		demo.visible = false;
		var renderer = new spine.SceneRenderer(canvas, canvas.context.gl);
		demo.loadingScreen = new spine.LoadingScreen(renderer);
		$(window).on('DOMContentLoaded load resize scroll', function () {
			checkElementVisible(demo);
			renderDemo(demo);
		});
		checkElementVisible(demo);
		spineDemos.demos.push(demo);
	}

	var coords = new spine.Vector3();
	var mouse = new spine.Vector3();
	spineDemos.closest = function (canvas, renderer, skeleton, controlBones, hoverTargets, x, y) {
		mouse.set(x, canvas.clientHeight - y, 0)
		var bestDistance = 24, index = 0;
		var best;
		for (var i = 0; i < controlBones.length; i++) {
			hoverTargets[i] = null;
			let bone = skeleton.findBone(controlBones[i]);
			let distance = renderer.camera.worldToScreen(
				coords.set(bone.worldX, bone.worldY, 0),
				canvas.clientWidth, canvas.clientHeight).distance(mouse);
			if (distance < bestDistance) {
				bestDistance = distance;
				best = bone;
				index = i;
			}
		}
		if (best) hoverTargets[index] = best;
		return best;
	};

	var position = new spine.Vector3();
	spineDemos.dragged = function (canvas, renderer, target, x, y) {
		if (target) {
			x = spine.MathUtils.clamp(x, 0, canvas.clientWidth)
			y = spine.MathUtils.clamp(y, 0, canvas.clientHeight);
			renderer.camera.screenToWorld(coords.set(x, y, 0), canvas.clientWidth, canvas.clientHeight);
			if (target.parent !== null) {
				target.parent.worldToLocal(position.set(coords.x, coords.y));
				target.x = position.x;
				target.y = position.y;
			} else {
				target.x = coords.x;
				target.y = coords.y;
			}
		}
	};

	loadSliders = function () {
		$(window).resize(function () {
			$(".slider").each(function () {
				$(this).data("slider").resized();
			});
		});
		$(".slider").each(function () {
			var div = $(this), handle = $("<div/>").appendTo(div);
			var bg1, bg2;
			if (div.hasClass("filled")) {
				bg1 = $("<span/>").appendTo(div)[0].style;
				bg2 = $("<span/>").appendTo(div)[0].style;
			}
			var hw = handle.width(), value = 0, object, lastX;
			handle = handle[0].style;
			positionHandle(0);
			function positionHandle(percent) {
				var w = div.width();
				var x = Math.round((w - hw - 3) * percent + 1);
				if (x != lastX) {
					lastX = x;
					handle.transform = "translateX(" + x + "px)";
					if (bg1) {
						var w1 = x + hw / 2;
						bg1.width = w1 + "px";
						bg2.width = (w - w1) + "px";
						bg2.left = w1 + "px";
					}
				}
				value = percent;
			}
			function mouseEvent(e) {
				var x = e.pageX;
				if (!x && e.originalEvent.touches) x = e.originalEvent.touches[0].pageX;
				var percent = Math.max(0, Math.min(1, (x - div.offset().left - hw / 2) / (div.width() - hw - 2)));
				positionHandle(percent);
				if (object.changed) object.changed(percent);
			}
			function clearEvents() {
				$(document).off("mouseup.slider mousemove.slider touchmove.slider touchend.slider");
			}
			div.on("mousedown touchstart", function (e) {
				mouseEvent(e);
				e.preventDefault(); // Disable text selection.
				$(document).on("mousemove.slider touchmove.slider", mouseEvent).on("mouseup.slider touchend.slider", clearEvents);
			});
			div.data("slider", object = {
				set: positionHandle,
				get: function () { return value; },
				resized: function () {
					lastX = null;
					positionHandle(value);
				}
			});
		});
	}
})();