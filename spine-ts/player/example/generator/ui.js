var spineGenerator;

(function (spineGenerator) {
	var UI = (function () {
		function UI(dropZone, multiple, acceptedExtensions, callback) {
		}

		UI.createElement = function (parent, html) {
			parent.insertAdjacentHTML("beforeend", html);
			return parent.lastElementChild;
		}

		UI.clear = function(element) {
			element.innerHTML = "";
		}

		UI.hide = function(element) {
			element.__oldDisplay = element.style.display;
			element.style.display = "none";
		}

		UI.show = function(element, display) {
			if (display) element.style.display = display;
			else if (element.__oldDisplay) element.style.display = element.__oldDisplay;
			else element.style.display = "block";
		}

		return UI;
	}());
	spineGenerator.UI = UI;
}(spineGenerator || (spineGenerator = {})));

(function (spineGenerator) {
	var UI = spineGenerator.UI;
	var DropZone = (function () {
		function DropZone(dropZone, multiple, acceptedExtensions, callback) {
			var fileButton = this.fileButton = UI.createElement(dropZone,
				`<input style="display:none;" type="file" ${multiple?"multiple":""} accept="${acceptedExtensions}"/>`);

			dropZone.onclick = function() {
				fileButton.click();
			};
			dropZone.addEventListener("dragenter", function  (event) {
				event.stopPropagation();
				event.preventDefault();
			}, false);
			dropZone.addEventListener("dragover", function  (event) {
				event.stopPropagation();
				event.preventDefault();
			}, false);
			dropZone.addEventListener("drop", function  (event) {
				event.stopPropagation();
				event.preventDefault();

				loadFiles(event.dataTransfer.files);
			}, false);

			fileButton.onchange = function () {
				callback(fileButton.files);
				fileButton.value = "";
			};
		}
		return DropZone;
	}());
	spineGenerator.UI.DropZone = DropZone;
}(spineGenerator || (spineGenerator = {})));