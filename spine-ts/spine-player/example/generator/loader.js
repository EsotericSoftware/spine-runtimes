var spineGenerator;

(function (spineGenerator) {
	var Loader = (function () {
		function Loader() {
		}

		Loader.loadSkeletonFiles = function(files, success, error) {
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
				error("Please specify a single .skel or .json file.");
				return;
			}

			if (atlases != 1) {
				error("Please specify a single .atlas file.");
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
							var data = {
								dataUrls: dataUrls,
								jsonFile: jsonFile,
								skelFile: skelFile,
								atlasFile: atlasFile
							};
							var version = data.version = Loader.getSkeletonVersion(data);
							data.majorVersion = parseInt(version.split("\.")[0]);
							data.minorVersion = parseInt(version.split("\.")[1]);
							data.patchVersion = parseInt(version.split("\.")[2]);
							success(data);
						}
					};
				}(file);
				reader.onerror = function () {
					error("Sorry, couldn't load all files.");
				}
				reader.readAsDataURL(file);
			}
		}

		Loader.getSkeletonVersion = function (data) {
			var jsonFile = data.jsonFile;
			var skelFile = data.skelFile;
			var dataUrls = data.dataUrls;
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
				return version;
			}
		}

		Loader.loadJavaScript = function (url, success, error) {
			var script = document.createElement('script');
			script.setAttribute('src', url);
			script.setAttribute('type', 'text/javascript');
			script.onload = success;
			script.onerror = error;
			document.getElementsByTagName("head")[0].appendChild(script);
		};

		Loader.loadStyle = function(url, success, error) {
			var style = document.createElement('link');
			style.setAttribute('href', url);
			style.setAttribute('rel', 'stylesheet');
			style.onload = success;
			style.onerror = error;
			document.getElementsByTagName("head")[0].appendChild(style);
		};

		var BinaryInput = (function () {
			function BinaryInput(data, strings, index, buffer) {
				if (strings === void 0) { strings = new Array(); }
				if (index === void 0) { index = 0; }
				if (buffer === void 0) { buffer = new DataView(data.buffer); }
				this.index = index;
				this.buffer = buffer;
			}

			BinaryInput.prototype.readByte = function () {
				return this.buffer.getInt8(this.index++);
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
			return BinaryInput;
		}());

		return Loader;
	}());
	spineGenerator.Loader = Loader;
}(spineGenerator || (spineGenerator = {})));