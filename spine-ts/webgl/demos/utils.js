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
			var canvas = demos[i].canvas;
			var renderFunc = demos[i].renderFunc;
			if (spineDemos.isElementInViewport(canvas)) {
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
		setupLoop();
		spineDemos.demos.push({canvas: canvas, renderFunc: renderFunc});		
	};

	spineDemos.requestAnimationFrame = function(func) {
		requestAnimationFrame(func);
	}

	spineDemos.isElementInViewport = function (canvas) {
		var rect = canvas.getBoundingClientRect();
		var x = 0, y = 0;
		var width = (window.innerHeight || document.documentElement.clientHeight);
		var height = (window.innerWidth || document.documentElement.clientWidth);
		return rect.left < x + width && rect.right > x && rect.top < y + height && rect.bottom > y;		 
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