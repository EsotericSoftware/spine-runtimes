var spineDemos = {
	HOVER_COLOR_INNER: new spine.Color(0.478, 0, 0, 0.25),
	HOVER_COLOR_OUTER: new spine.Color(1, 1, 1, 1),
	NON_HOVER_COLOR_INNER: new spine.Color(0.478, 0, 0, 0.5),
	NON_HOVER_COLOR_OUTER: new spine.Color(1, 0, 0, 0.8),
	assetManager: new spine.SharedAssetManager("http://esotericsoftware.com/demos/exports/"),
	demos: [],
	loopRunning: false
};
(function() {
	var timeKeeper = new spine.TimeKeeper();
	function loop () {
		timeKeeper.update();
		if (spineDemos.log) console.log(timeKeeper.delta + ", " + timeKeeper.framesPerSecond);
		spineDemos.requestAnimationFrame(loop);
		var demos = spineDemos.demos;		
		for (var i = 0; i < demos.length; i++) {
			var demo = demos[i];
			var canvas = demo.canvas;
			var renderFunc = demo.renderFunc;
			if (demo.visible) {
				if (spineDemos.log) console.log("Rendering " + canvas.id);
				renderFunc();
			}
		}
	}

	function setupLoop () {
		if (!spineDemos.loopRunning) {			
			loop();
			spineDemos.loopRunning = true;
		}
	}

	spineDemos.setupRendering = function (canvas, renderFunc) {
		var demo = {canvas: canvas, renderFunc: renderFunc, visible: false};
		$(window).on('DOMContentLoaded load resize scroll', function() {
			spineDemos.checkElementVisible(demo);
		});
		spineDemos.checkElementVisible(demo);
		setupLoop();
		spineDemos.demos.push(demo);
	};

	spineDemos.requestAnimationFrame = function(func) {
		requestAnimationFrame(func);
	};

	spineDemos.checkElementVisible = function (demo) {
		var rect = demo.canvas.getBoundingClientRect();
		var x = 0, y = 0;
		var width = (window.innerHeight || document.documentElement.clientHeight);
		var height = (window.innerWidth || document.documentElement.clientWidth);
		demo.visible = rect.left < x + width && rect.right > x && rect.top < y + height && rect.bottom > y;		 
	};

	spineDemos.setupWebGLContext = function (canvas) {
		config = {
			alpha: false,
			depth: false,
			stencil: false
		}
		return gl = canvas.getContext("webgl", config) || canvas.getContext("experimental-webgl", config);
	};

	spineDemos.loadSliders = function () {
		$(".slider").each(function () {
			var div = $(this), handle = $("<div/>").appendTo(div);
			var bg = div.hasClass("before") ? $("<span/>").appendTo(div) : null;
			var hw = handle.width(), value = 0, object;
			function positionHandle (percent) {
				var x = (div.width() - hw - 2) * percent;
				handle[0].style.left = x + "px";
				if (bg) bg.css("width", x + hw / 2);
				value = percent;
			}
			function mouseEvent (e) {
				var x = e.pageX || e.originalEvent.touches[0].pageX;
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
				get: function () { return value; }
			});
			div[0].handle = handle;
			div[0].positionHandle = positionHandle;
		});
	}
})();