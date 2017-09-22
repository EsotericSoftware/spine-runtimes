var spineDemos = {
	HOVER_COLOR_INNER: new spine.Color(0.478, 0, 0, 0.25),
	HOVER_COLOR_OUTER: new spine.Color(1, 1, 1, 1),
	NON_HOVER_COLOR_INNER: new spine.Color(0.478, 0, 0, 0.5),
	NON_HOVER_COLOR_OUTER: new spine.Color(1, 0, 0, 0.8),
	assetManager: new spine.SharedAssetManager("assets/"),
	demos: [],
	loopRunning: false,
	canvases: []
};

window.onerror = function (msg, url, lineNo, columnNo, error) {
    var string = msg.toLowerCase();
    var substring = "script error";
    if (string.indexOf(substring) > -1){
        alert('Script Error: See Browser Console for Detail');
    } else {
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
	function loop () {
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
		var canvas = demo.canvas;
		if (!spineDemos.assetManager.isLoadingComplete(demo.DEMO_NAME)) {
			if (demo.visible) {
				if (canvas.parentElement != demo.placeholder) {
					$(canvas).detach();
					demo.placeholder.appendChild(canvas);
				}
				demo.loadingScreen.draw();
			}
		} else {
			if (!demo.loaded) {
				demo.loadingComplete();
				demo.loaded = true;
			}

			if (demo.visible) {
				if (canvas.parentElement != demo.placeholder) {
					$(canvas).detach();
					demo.placeholder.appendChild(canvas);
				}
				if (spineDemos.log) console.log("Rendering " + canvas.id);
				demo.render();
				demo.loadingScreen.draw(true);
			}
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

	function createCanvases (numCanvases) {
		for (var i = 0; i < numCanvases; i++) {
			var canvas = document.createElement("canvas");
			canvas.width = 1; canvas.height = 1;
			canvas.ctx = new spine.webgl.ManagedWebGLRenderingContext(canvas, { alpha: false });
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
		var renderer = new spine.webgl.SceneRenderer(canvas, canvas.ctx.gl);
		demo.loadingScreen = new spine.webgl.LoadingScreen(renderer);
		$(window).on('DOMContentLoaded load resize scroll', function() {
			checkElementVisible(demo);
			renderDemo(demo);
		});
		checkElementVisible(demo);
		spineDemos.demos.push(demo);
	}

	loadSliders = function () {
		$(window).resize(function() {
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
			function positionHandle (percent) {
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
			function mouseEvent (e) {
				var x = e.pageX;
				if (!x && e.originalEvent.touches) x = e.originalEvent.touches[0].pageX;
				var percent = Math.max(0, Math.min(1, (x - div.offset().left - hw / 2) / (div.width() - hw - 2)));
				positionHandle(percent);
				if (object.changed) object.changed(percent);
			}
			function clearEvents () {
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