var spineDemos;
(function(spineDemos) {
	spineDemos.HOVER_COLOR_INNER = new spine.Color(0.478, 0, 0, 0.25);
	spineDemos.HOVER_COLOR_OUTER = new spine.Color(1, 1, 1, 1);
	spineDemos.NON_HOVER_COLOR_INNER = new spine.Color(0.478, 0, 0, 0.5);
	spineDemos.NON_HOVER_COLOR_OUTER = new spine.Color(1, 0, 0, 0.8);

	spineDemos.setupRendering = function (canvas, renderFunc) {
		var isVisible = false;

		function render () {
			renderFunc();
			if (isVisible) requestAnimationFrame(render);
		};

		function viewportCheck () {
			var old = isVisible	
			isVisible = spineDemos.isElementInViewport(canvas);			
			if (isVisible && old != isVisible) requestAnimationFrame(render);		
		}

		window.addEventListener("DOMContentLoaded", viewportCheck, false);
		window.addEventListener("load", viewportCheck, false);
		window.addEventListener("resize", viewportCheck, false);
		window.addEventListener("scroll", viewportCheck, false);

		viewportCheck();
		requestAnimationFrame(render);
	};

	spineDemos.isElementInViewport = function (canvas) {
		var rect = canvas.getBoundingClientRect();
		var x = 0, y = 0;
		var width = (window.innerHeight || document.documentElement.clientHeight);
		var height = (window.innerWidth || document.documentElement.clientWidth);
		return rect.left < x + width && rect.right > x && rect.top < y + height && rect.bottom > y;		 
	};
})(spineDemos || (spineDemos = { }));