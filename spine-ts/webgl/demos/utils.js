var spineDemos;
(function(spineDemos) {
	spineDemos.setupRendering = function (canvas, renderFunc) {
		function render() {
			renderFunc();
			requestAnimationFrame(render);
		};
		render();
	}
})(spineDemos || (spineDemos = { }));