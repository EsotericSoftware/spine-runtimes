var spineDemos;
(function(spineDemos) {
	spineDemos.HOVER_COLOR_INNER = new spine.Color(0.478, 0, 0, 0.25);
	spineDemos.HOVER_COLOR_OUTER = new spine.Color(1, 1, 1, 1);
	spineDemos.NON_HOVER_COLOR_INNER = new spine.Color(0.478, 0, 0, 0.5);
	spineDemos.NON_HOVER_COLOR_OUTER = new spine.Color(1, 0, 0, 0.8);
	spineDemos.assetManager = new spine.SharedAssetManager("http://esotericsoftware.com/demos/exports/");
	spineDemos.demos = [];
	spineDemos.loopRunning = false;

	var timeKeeper = new spine.TimeKeeper();
	var loop = function() {
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
			};
		}
	}

	var setupLoop = function() {
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
	}

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
	}
})(spineDemos || (spineDemos = { }));