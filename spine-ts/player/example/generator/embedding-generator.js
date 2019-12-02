window.addEventListener("load", function(event) {
	setupDropZone();
});

if (!String.prototype.endsWith) {
	String.prototype.endsWith = function(search, this_len) {
		if (this_len === undefined || this_len > this.length) {
			this_len = this.length;
		}
		return this.substring(this_len - search.length, this_len) === search;
	};
}

var appState = {
	dataUrls: null,
	jsonFile: null,
	skelFile: null,
	atlasFile: null,
	minorVersion: null,
	majorVersion: null,
	player: null
}

function loadFiles(files) {
	var skels = 0;
	var skelFile = null;
	var jsons = 0;
	var jsonFile = null;
	var atlases = 0;
	var atlasFile = null;
	var pngs = 0;

	for (var i = 0; i < files.length; i++) {
		var file = files[i].name.toLowerCase();
		if (file.endsWith(".skel")) {
			skels++;
			skelFile = file;
		}
		if (file.endsWith(".json")) {
			jsons++;
			jsonFile = file;
		}
		if (file.endsWith(".atlas")) {
			atlases++;
			atlasFile = file;
		}
		if (file.endsWith(".png")) pngs++;
	}

	if ((skels == 0 && jsons == 0) || (skels != 0 && jsons != 0) || skels > 1 || jsons > 1) {
		showError("Please specify a single .skel or .json file.");
		return;
	}

	if (atlases != 1) {
		showError("Please specify a single .atlas file.");
		return;
	}

	var filesToLoad = files.length;
	var dataUrls = {};
	for (var i = 0; i < files.length; i++) {
		var file = files[i];
		var reader = new FileReader();
		reader.onload = function(file) {
			return function(dataUrl) {
				console.log("Loaded " + file.name);
				dataUrls[file.name] = dataUrl.target.result;
				filesToLoad--;
				if (filesToLoad == 0) {
					setupPlayer(dataUrls, jsonFile, skelFile, atlasFile);
				}
			};
		}(file);
		reader.onerror = function () {
			showError("Sorry, couldn't load all files.");
		}
		reader.readAsDataURL(file);
	}
}

function setupPlayer(dataUrls, jsonFile, skelFile, atlasFile) {
	var version = getSkeletonVersion(dataUrls, jsonFile, skelFile);
	var major = parseInt(version.split("\.")[0]);
	var minor = parseInt(version.split("\.")[1]);

	appState.dataUrls = dataUrls;
	appState.jsonFile = jsonFile;
	appState.skelFile = skelFile;
	appState.atlasFile = atlasFile;
	appState.majorVersion = major;
	appState.minorVersion = minor;

	if (major == 3 && minor < 8) {
		showError("Couldn't load script for Spine version " + version + ". Only skeletons with version >= 3.8 are supported.");
		return;
	}

	var cssUrl = "https://esotericsoftware.com/files/spine-player/" + major + "." + minor + "/spine-player.css";
	spine = null;
	loadCSS(cssUrl, function () {
		var playerUrl = "https://esotericsoftware.com/files/spine-player/" + major + "." + minor + "/spine-player.js";
		loadJavaScript(playerUrl, function() {
			document.getElementById("sp_generator_editor").classList.remove("sp_generator_hidden");
			document.getElementById("sp_generator_drop_zone").classList.add("sp_generator_hidden");
			var player = document.getElementById("sp_generator_player");
			player.innerHTML = "";

			var config = {
				jsonUrl: jsonFile,
				skelUrl: skelFile,
				atlasUrl: atlasFile,
				rawDataURIs: dataUrls,
				success: setupConfigUI,
				alpha: true, // needed so we can emulate shizzle
				viewport: { // needed so we can see viewport bounds
					debugRender: true
				}
			};

			appState.player = new spine.SpinePlayer(player, config);

		}, function() {
			showError("Couldn't load script for Spine version " + version + ". Only skeletons with version 3.8+ are supported.");
		});
	}, function () {
		showError("Couldn't load CSS for Spine version " + version + ". Only skeletons with version 3.8+ are supported.");
	});
}

function setupConfigUI() {
	// Setup tabs
	var tabs = document.getElementsByClassName("sp_generator_tabs")[0];
	var children = tabs.getElementsByTagName("span");
	for (var i = 0; i < children.length; i++) {
		(function (tab) {
			tab.onclick = function () {
				var panelId = tab.getAttribute("data-tab");
				var panels = document.getElementById("sp_generator_config").getElementsByClassName("sp_generator_panel");
				for (var i = 0; i < panels.length; i++) {
					var panel = panels[i];
					if (panelId == panel.getAttribute("id")) {
						tab.classList.add("sp_generator_selected_tab");
						panel.classList.remove("sp_generator_hidden");
					} else {
						tab.classList.remove("sp_generator_selected_tab");
						panel.classList.add("sp_generator_hidden");
					}
				}
			}
		})(children[i]);
	}

	// Fill general tab
	var showControls = document.getElementById("sp_generator_show_controls");
	showControls.onchange = function () {
		appState.player.config.showControls = showControls.checked;
	};
	var canvasAlpha = document.getElementById("sp_generator_canvas_alpha");
	canvasAlpha.onchange = function () {
		var re = /[0-9A-Fa-f]{2}/g;
		if (canvasAlpha.value.length > 2 || !re.test(canvasAlpha.value))
			canvasAlpha.value = "FF";
		else
			canvasAlpha.value = canvasAlpha.value.toUpperCase();
		var alpha = Number.parseInt(canvasAlpha.value, 16);
		appState.player.config.alpha = alpha != 0xff;
		appState.player.config.backgroundColor = document.getElementById("sp_generator_background").value + canvasAlpha.value;
	}
	var premultipliedAlpha = document.getElementById("sp_generator_premultiplied_alpha");
	var premultipliedAlpha = document.getElementById("sp_generator_premultiplied_alpha");
	premultipliedAlpha.onchange = function () {
		appState.player.config.premultipliedAlpha = premultipliedAlpha.checked;
	}
	var backgroundImage = document.getElementById("sp_generator_background_image");
	backgroundImage.innerHTML = "";
	var noneImage = document.createElement("option");
	noneImage.value = "none";
	noneImage.innerText = "None";
	noneImage.selected = true;
	backgroundImage.append(noneImage);
	for(var data in appState.dataUrls) {
		if (data.toLowerCase().endsWith(".png")) {
			var image = document.createElement("option");
			image.value = data;
			image.innerText = data;
			backgroundImage.append(image);
		}
	}
	backgroundImage.onchange = function() {
		var imageUrl = backgroundImage.value;
		if (imageUrl != "none" && !appState.player.assetManager.get(imageUrl)) {
			appState.player.assetManager.loadTexture(imageUrl);
		}

		var boundsTable = document.getElementById("sp_generator_background_bounds");
		if (imageUrl == "none")
			boundsTable.classList.add("sp_generator_hidden");
		else
			boundsTable.classList.remove("sp_generator_hidden");

		if (appState.player.config.backgroundImage) {
			appState.player.config.backgroundImage.url = imageUrl != "none" ? imageUrl: null;
		} else {
			appState.player.config.backgroundImage = {
				url: imageUrl != "none" ? imageUrl : null
			}
		}
	}
	var backgroundX = document.getElementById("sp_generator_background_x");
	backgroundX.onkeyup = backgroundX.onchange = function () {
		var value = Number.parseFloat(backgroundX.value);
		if (Number.isNaN(value)) return;
		appState.player.config.backgroundImage.x = value;
	};

	var backgroundY = document.getElementById("sp_generator_background_y");
	backgroundY.onkeyup = backgroundY.onchange = function () {
		var value = Number.parseFloat(backgroundY.value);
		if (Number.isNaN(value)) return;
		appState.player.config.backgroundImage.y = value;
	};
	var backgroundWidth = document.getElementById("sp_generator_background_width");
	backgroundWidth.onkeyup = backgroundWidth.onchange = function () {
		var value = Number.parseFloat(backgroundWidth.value);
		if (Number.isNaN(value)) return;
		appState.player.config.backgroundImage.width = value;
	};
	var backgroundHeight = document.getElementById("sp_generator_background_height");
	backgroundHeight.onkeyup = backgroundHeight.onchange = function () {
		var value = Number.parseFloat(backgroundHeight.value);
		if (Number.isNaN(value)) return;
		appState.player.config.backgroundImage.height = value;
	};


	// Fill animations tab

	// Fill viewports tab

	// Fill skins tab

	// Fill debug tab
}

function changeBackgroundColor(background) {
	appState.player.config.backgroundColor = background.valueElement.value + document.getElementById("sp_generator_canvas_alpha").value;
}

function changeFullscreenBackgroundColor(background) {
	appState.player.config.fullScreenBackgroundColor = background.valueElement.value;
}

function getSkeletonVersion(dataUrls, jsonFile, skelFile) {
	if (jsonFile) {
		var json = JSON.parse(atob(dataUrls[jsonFile].split(',')[1]));
		return json.skeleton.spine;
	} else {
		var bytes = atob(dataUrls[skelFile].split(',')[1]);
		var array = new Uint8Array(new ArrayBuffer(bytes.length));
		for (var i = 0; i < bytes.length; i++) {
			array[i] = bytes.charCodeAt(i);
		}

		var input = new BinaryInput(array);
		input.readString();
		var version = input.readString();
		return version;x
	}
}

function loadJavaScript(url, success, error) {
  var script = document.createElement('script');
  script.setAttribute('src', url);
  script.setAttribute('type', 'text/javascript');
  script.onload = success;
  script.onerror = error;
  document.getElementsByTagName("head")[0].appendChild(script);
};

function loadCSS(url, success, error) {
	var script = document.createElement('link');
	script.setAttribute('href', url);
	script.setAttribute('rel', 'stylesheet');
	script.onload = success;
	script.onerror = error;
	document.getElementsByTagName("head")[0].appendChild(script);
  };

function showError(error) {
	alert(error);
}

function setupDropZone() {
	var fileButton = document.getElementById("sp_generator_file_button");
	var dropZone = document.getElementById("sp_generator_drop_zone");
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
		loadFiles(fileButton.files);
		fileButton.value = "";
	};
}

function generateScript(jsonFile, skelFile, atlasFile, dataUrls) {
	var shortVersion = major + "." + minor;
	var scriptCode =
	'<script src="https://esotericsoftware.com/files/spine-player/' + shortVersion + '/spine-player.js"><' + '/script>\n' +
	'<link rel="stylesheet" href="https://esotericsoftware.com/files/spine-player/' + shortVersion + '/spine-player.css">\n\n' +
	'<div id="player-container" style="width: 100%; height: 100vh;"></div>\n\n' +
	'<script>\n' +
	'new spine.SpinePlayer("player-container", {\n';
	if (jsonFile) scriptCode +=
	'   jsonUrl: "' + jsonFile + '",\n';
	else scriptCode +=
	'   skelUrl: "' + skelFile + '",\n';

	scriptCode +=
	'   atlasUrl: "' + atlasFile + '",\n' +
	'   rawDataURIs: {\n'

	for (var file in dataUrls) {
		scriptCode +=
	'       "' + file + '": "' + dataUrls[file] + '",\n';
	}

	scriptCode +=
	'   }\n' +
	'});\n' +
	'<' + '/script>';
}

var BinaryInput = (function () {
	function BinaryInput(data, strings, index, buffer) {
		if (strings === void 0) { strings = new Array(); }
		if (index === void 0) { index = 0; }
		if (buffer === void 0) { buffer = new DataView(data.buffer); }
		this.strings = strings;
		this.index = index;
		this.buffer = buffer;
	}
	BinaryInput.prototype.readByte = function () {
		return this.buffer.getInt8(this.index++);
	};
	BinaryInput.prototype.readShort = function () {
		var value = this.buffer.getInt16(this.index);
		this.index += 2;
		return value;
	};
	BinaryInput.prototype.readInt32 = function () {
		var value = this.buffer.getInt32(this.index);
		this.index += 4;
		return value;
	};
	BinaryInput.prototype.readInt = function (optimizePositive) {
		var b = this.readByte();
		var result = b & 0x7F;
		if ((b & 0x80) != 0) {
			b = this.readByte();
			result |= (b & 0x7F) << 7;
			if ((b & 0x80) != 0) {
				b = this.readByte();
				result |= (b & 0x7F) << 14;
				if ((b & 0x80) != 0) {
					b = this.readByte();
					result |= (b & 0x7F) << 21;
					if ((b & 0x80) != 0) {
						b = this.readByte();
						result |= (b & 0x7F) << 28;
					}
				}
			}
		}
		return optimizePositive ? result : ((result >>> 1) ^ -(result & 1));
	};
	BinaryInput.prototype.readStringRef = function () {
		var index = this.readInt(true);
		return index == 0 ? null : this.strings[index - 1];
	};
	BinaryInput.prototype.readString = function () {
		var byteCount = this.readInt(true);
		switch (byteCount) {
			case 0:
				return null;
			case 1:
				return "";
		}
		byteCount--;
		var chars = "";
		var charCount = 0;
		for (var i = 0; i < byteCount;) {
			var b = this.readByte();
			switch (b >> 4) {
				case 12:
				case 13:
					chars += String.fromCharCode(((b & 0x1F) << 6 | this.readByte() & 0x3F));
					i += 2;
					break;
				case 14:
					chars += String.fromCharCode(((b & 0x0F) << 12 | (this.readByte() & 0x3F) << 6 | this.readByte() & 0x3F));
					i += 3;
					break;
				default:
					chars += String.fromCharCode(b);
					i++;
			}
		}
		return chars;
	};
	BinaryInput.prototype.readFloat = function () {
		var value = this.buffer.getFloat32(this.index);
		this.index += 4;
		return value;
	};
	BinaryInput.prototype.readBoolean = function () {
		return this.readByte() != 0;
	};
	return BinaryInput;
}());