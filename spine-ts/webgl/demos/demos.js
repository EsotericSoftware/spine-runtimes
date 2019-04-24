
$(function () {
	window.onerror = function(message, url, lineNo) {
		alert("Error: " + message + "\n" + "URL:" + url + "\nLine: " + lineNo);
	}


	spineDemos.init();
	spineDemos.assetManager = new spine.SharedAssetManager("assets/");

	var demos = [
		spritesheetsDemo,
		imageChangesDemo,
		transitionsDemo,
		meshesDemo,
		skinsDemo,
		hoverboardDemo,
		vineDemo,
		clippingDemo,
		stretchymanDemo,
		tankDemo,
		transformsDemo,
		additiveBlendingDemo
	];

	var placeholders = document.getElementsByClassName("aspect");

	for (var i = 0; i < demos.length; i++)
		spineDemos.addDemo(demos[i], placeholders[i]);

	function resizeSliders () {
		$(".slider").each(function () {
			$(this).data("slider").resized();
		});
	}

	function windowResized () {
		// Keep canvas from taking up whole screen.
		$(".aspect").each(function () {
			$(this).css("padding-bottom", Math.min(70.14, $(window).height() * 0.75 / $(this).width() * 100) + "%");
		});

		// Swap controls when media query puts text below canvas.
		var below = $("#below").is(':visible');
		$(".demo .description").each(function () {
			var description = $(this);
			var controls = description.children(".controls");
			if (below || description.hasClass("fullsize"))
				description.prepend(controls);
			else
				description.append(controls);
		});

		resizeSliders();
	}
	windowResized();
	$(window).resize(windowResized);

	$(".resize").click(function () {
		var resizeButton = $(this);
		var container = resizeButton.parent();
		var parent = container.parent();
		var overlayLabels = parent.find(".overlay-label");
		var description = parent.children(".description");
		var controls = description.children(".controls");

		container.toggleClass("fullsize");
		resizeButton.toggleClass("checked");

		var offset = parseFloat(overlayLabels.css("bottom"));
		description.toggleClass("fullsize");
		if (description.hasClass("fullsize")) {
			overlayLabels.css("bottom", offset * 1.666);
		} else {
			resizeSliders();
			overlayLabels.css("bottom", offset / 1.666);
		}
		setTimeout(function() {
			windowResized();
		}, 500);
	});

	$(".checkbox-overlay").change(function () {
		$(this).closest(".demo").find(".overlay").toggleClass("overlay-hide");
	});
});
