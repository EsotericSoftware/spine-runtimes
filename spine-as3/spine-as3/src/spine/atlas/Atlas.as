/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.atlas {
	import flash.utils.ByteArray;
	import flash.utils.Dictionary;

	public class Atlas {
		private var pages : Vector.<AtlasPage> = new Vector.<AtlasPage>();
		private var regions : Vector.<AtlasRegion> = new Vector.<AtlasRegion>();
		private var textureLoader : TextureLoader;

		/** @param object A String or ByteArray. */
		public function Atlas(object : *, textureLoader : TextureLoader) {
			if (!object) return;
			if (object is String)
				load(String(object), textureLoader);
			else if (object is ByteArray)
				load(ByteArray(object).readUTFBytes(ByteArray(object).length), textureLoader);
			else
				throw new ArgumentError("object must be a string or ByteArray containing .atlas data.");
		}

		protected function load(atlasText : String, textureLoader : TextureLoader) : void {
			if (textureLoader == null) throw new ArgumentError("textureLoader cannot be null.");
			this.textureLoader = textureLoader;

			var reader : Reader = new Reader(atlasText);
			var entry : Vector.<String> = new Vector.<String>(5, true);
			var page : AtlasPage;
			var region : AtlasRegion;

			var pageFields : Dictionary = new Dictionary();
			pageFields["size"] = function() : void {
				page.width = parseInt(entry[1]);
				page.height = parseInt(entry[2]);
			};
			pageFields["format"] = function() : void {
				page.format = Format[entry[0]];
			};
			pageFields["filter"] = function() : void {
				page.minFilter = TextureFilter[entry[1]];
				page.magFilter = TextureFilter[entry[2]];
			};
			pageFields["repeat"] = function() : void {
				if (entry[1].indexOf('x') != -1) page.uWrap = TextureWrap.repeat;
				if (entry[1].indexOf('y') != -1) page.vWrap = TextureWrap.repeat;
			};
			pageFields["pma"] = function() : void {
				page.pma = entry[1] == "true";
			};

			var regionFields : Dictionary = new Dictionary();
			regionFields["xy"] = function() : void { // Deprecated, use bounds.
				region.x = parseInt(entry[1]);
				region.y = parseInt(entry[2]);
			};
			regionFields["size"] = function() : void { // Deprecated, use bounds.
				region.width = parseInt(entry[1]);
				region.height = parseInt(entry[2]);
			};
			regionFields["bounds"] = function() : void {
				region.x = parseInt(entry[1]);
				region.y = parseInt(entry[2]);
				region.width = parseInt(entry[3]);
				region.height = parseInt(entry[4]);
			};
			regionFields["offset"] = function() : void { // Deprecated, use offsets.
				region.offsetX = parseInt(entry[1]);
				region.offsetY = parseInt(entry[2]);
			};
			regionFields["orig"] = function() : void { // Deprecated, use offsets.
				region.originalWidth = parseInt(entry[1]);
				region.originalHeight = parseInt(entry[2]);
			};
			regionFields["offsets"] = function() : void {
				region.offsetX = parseInt(entry[1]);
				region.offsetY = parseInt(entry[2]);
				region.originalWidth = parseInt(entry[3]);
				region.originalHeight = parseInt(entry[4]);
			};
			regionFields["rotate"] = function() : void {
				var value : String = entry[1];
				if (value == "true")
					region.degrees = 90;
				else if (value != "false")
					region.degrees = parseInt(value);
			};
			regionFields["index"] = function() : void {
				region.index = parseInt(entry[1]);
			};

			var line : String = reader.readLine();
			// Ignore empty lines before first entry.
			while (line != null && line.length == 0)
				line = reader.readLine();
			// Header entries.
			while (true) {
				if (line == null || line.length == 0) break;
				if (reader.readEntry(entry, line) == 0) break; // Silently ignore all header fields.
				line = reader.readLine();
			}

			// Page and region entries.
			var names : Vector.<String>;
			var values : Vector.<Vector.<Number>>;
			var field : Function;
			while (true) {
				if (line == null) break;
				if (line.length == 0) {
					page = null;
					line = reader.readLine();
				} else if (page == null) {
					page = new AtlasPage();
					page.name = line;
					while (true) {
						if (reader.readEntry(entry, line = reader.readLine()) == 0) break;
						field = pageFields[entry[0]];
						if (field) field();
					}
					textureLoader.loadPage(page, line);
					pages.push(page);
				} else {
					region = new AtlasRegion();
					region.page = page;
					region.name = line;
					while (true) {
						var count : int = reader.readEntry(entry, line = reader.readLine());
						if (count == 0) break;
						field = regionFields[entry[0]];
						if (field)
							field();
						else {
							if (names == null) {
								names = new Vector.<String>();
								values = new Vector.<Vector.<Number>>();
							}
							names.push(entry[0]);
							var entryValues : Vector.<Number> = new Vector.<Number>(count, true);
							for (var i : int = 0; i < count; i++)
								entryValues[i] = parseInt(entry[i + 1]);
							values.push(entryValues);
						}
					}
					if (region.originalWidth == 0 && region.originalHeight == 0) {
						region.originalWidth = region.width;
						region.originalHeight = region.height;
					}
					if (names != null && names.length > 0) {
						region.names = names;
						region.values = values;
						names = null;
						values = null;
					}
					region.u = region.x / page.width;
					region.v = region.y / page.height;
					if (region.degrees == 90) {
						region.u2 = (region.x + region.height) / page.width;
						region.v2 = (region.y + region.width) / page.height;
					} else {
						region.u2 = (region.x + region.width) / page.width;
						region.v2 = (region.y + region.height) / page.height;
					}
					textureLoader.loadRegion(region);
					regions.push(region);
				}
			}
		}

		/** Returns the first region found with the specified name. This method uses string comparison to find the region, so the result
		 * should be cached rather than calling this method multiple times.
		 * @return The region, or null. */
		public function findRegion(name : String) : AtlasRegion {
			for (var i : int = 0, n : int = regions.length; i < n; i++)
				if (regions[i].name == name)
					return regions[i];
			return null;
		}

		public function dispose() : void {
			for (var i : int = 0, n : int = pages.length; i < n; i++)
				textureLoader.unloadPage(pages[i]);
		}
	}
}

class Reader {
	static private const trimRegex : RegExp = /^\s+|\s+$/gs;

	private var lines : Array;
	private var index : int;

	function Reader(text : String) {
		lines = trim(text).split(/[ \t]*(?:\r\n|\r|\n)[ \t]*/);
	}

	function trim (value : String) : String {
		return value.replace(trimRegex, "");
	}

	function readLine() : String {
		return index >= lines.length ? null : lines[index++];
	}

	function readEntry(entry : Vector.<String>, line : String) : int {
		if (line == null) return 0;
		if (line.length == 0) return 0;

		var colon : int = line.indexOf(':');
		if (colon == -1) return 0;
		entry[0] = trim(line.substr(0, colon));
		for (var i : int = 1, lastMatch : int = colon + 1;; i++) {
			var comma : int = line.indexOf(',', lastMatch);
			if (comma == -1) {
				entry[i] = trim(line.substr(lastMatch));
				return i;
			}
			entry[i] = trim(line.substr(lastMatch, comma - lastMatch));
			lastMatch = comma + 1;
			if (i == 4) return 4;
		}
	}
}
