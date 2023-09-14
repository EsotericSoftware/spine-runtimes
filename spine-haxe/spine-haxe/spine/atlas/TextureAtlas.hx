/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine.atlas;

import haxe.ds.StringMap;
import openfl.utils.Assets;

class TextureAtlas {
	private var pages = new Array<TextureAtlasPage>();
	private var regions = new Array<TextureAtlasRegion>();
	private var textureLoader:TextureLoader;

	/** @param object A String or ByteArray. */
	public function new(atlasText:String, textureLoader:TextureLoader) {
		if (atlasText == null) {
			throw new SpineException("atlasText must not be null");
		}
		if (textureLoader == null) {
			throw new SpineException("textureLoader must not be null");
		}
		load(atlasText, textureLoader);
	}

	private function load(atlasText:String, textureLoader:TextureLoader):Void {
		if (textureLoader == null) {
			throw new SpineException("textureLoader cannot be null.");
		}
		this.textureLoader = textureLoader;

		var reader:Reader = new Reader(atlasText);
		var entry:Array<String> = new Array<String>();
		entry.resize(5);
		var page:TextureAtlasPage = null;
		var region:TextureAtlasRegion = null;

		var pageFields:StringMap<Void->Void> = new StringMap<Void->Void>();
		pageFields.set("size", function():Void {
			page.width = Std.parseInt(entry[1]);
			page.height = Std.parseInt(entry[2]);
		});
		pageFields.set("format", function():Void {
			page.format = Format.fromName(entry[0]);
		});
		pageFields.set("filter", function():Void {
			page.minFilter = TextureFilter.fromName(entry[1]);
			page.magFilter = TextureFilter.fromName(entry[2]);
		});
		pageFields.set("repeat", function():Void {
			if (entry[1].indexOf('x') != -1)
				page.uWrap = TextureWrap.repeat;
			if (entry[1].indexOf('y') != -1)
				page.vWrap = TextureWrap.repeat;
		});
		pageFields.set("pma", function():Void {
			page.pma = entry[1] == "true";
		});

		var regionFields:StringMap<Void->Void> = new StringMap<Void->Void>();
		regionFields.set("xy", function():Void {
			region.x = Std.parseInt(entry[1]);
			region.y = Std.parseInt(entry[2]);
		});
		regionFields.set("size", function():Void {
			region.width = Std.parseInt(entry[1]);
			region.height = Std.parseInt(entry[2]);
		});
		regionFields.set("bounds", function():Void {
			region.x = Std.parseInt(entry[1]);
			region.y = Std.parseInt(entry[2]);
			region.width = Std.parseInt(entry[3]);
			region.height = Std.parseInt(entry[4]);
		});
		regionFields.set("offset", function():Void {
			region.offsetX = Std.parseInt(entry[1]);
			region.offsetY = Std.parseInt(entry[2]);
		});
		regionFields.set("orig", function():Void {
			region.originalWidth = Std.parseInt(entry[1]);
			region.originalHeight = Std.parseInt(entry[2]);
		});
		regionFields.set("offsets", function():Void {
			region.offsetX = Std.parseInt(entry[1]);
			region.offsetY = Std.parseInt(entry[2]);
			region.originalWidth = Std.parseInt(entry[3]);
			region.originalHeight = Std.parseInt(entry[4]);
		});
		regionFields.set("rotate", function():Void {
			var value:String = entry[1];
			if (value == "true")
				region.degrees = 90;
			else if (value != "false")
				region.degrees = Std.parseInt(value);
		});
		regionFields.set("index", function():Void {
			region.index = Std.parseInt(entry[1]);
		});

		var line:String = reader.readLine();
		// Ignore empty lines before first entry.
		while (line != null && line.length == 0) {
			line = reader.readLine();
		}

		// Header entries.
		while (true) {
			if (line == null || line.length == 0)
				break;
			if (reader.readEntry(entry, line) == 0)
				break; // Silently ignore all header fields.
			line = reader.readLine();
		}

		// Page and region entries.
		var names:Array<String> = null;
		var values:Array<Array<Float>> = null;
		var field:Void->Void;
		while (true) {
			if (line == null)
				break;
			if (line.length == 0) {
				page = null;
				line = reader.readLine();
			} else if (page == null) {
				page = new TextureAtlasPage(line);
				while (true) {
					if (reader.readEntry(entry, line = reader.readLine()) == 0)
						break;
					field = pageFields.get(entry[0]);
					if (field != null) {
						field();
					}
				}
				textureLoader.loadPage(page, page.name);
				pages.push(page);
			} else {
				region = new TextureAtlasRegion(page, line);
				while (true) {
					var count:Int = reader.readEntry(entry, line = reader.readLine());
					if (count == 0)
						break;
					field = regionFields.get(entry[0]);
					if (field != null) {
						field();
					} else {
						if (names == null) {
							names = new Array<String>();
							values = new Array<Array<Float>>();
						}
						names.push(entry[0]);
						var entryValues:Array<Float> = new Array<Float>();
						entryValues.resize(count);
						for (i in 0...count) {
							entryValues[i] = Std.parseInt(entry[i + 1]);
						}
						values.push(entryValues);
					}
				}

				if (region.originalWidth == 0 && region.originalHeight == 0) {
					region.originalWidth = region.width;
					region.originalHeight = region.height;
				}

				if (names != null && names.length > 0 && values != null && values.length > 0) {
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
	public function findRegion(name:String):TextureAtlasRegion {
		for (region in regions) {
			if (region.name == name) {
				return region;
			}
		}
		return null;
	}

	public function dispose():Void {
		for (page in pages) {
			textureLoader.unloadPage(page);
		}
	}
}

class Reader {
	private static var trimRegex:EReg = new EReg("^\\s+|\\s+$", "g");

	private var lines:Array<String>;
	private var index:Int = 0;

	public function new(text:String) {
		var regex:EReg = new EReg("[ \t]*(?:\r\n|\r|\n)[ \t]*", "g");
		lines = regex.split(text);
	}

	private function trim(value:String):String {
		return trimRegex.replace(value, "");
	}

	public function readLine():String {
		return index >= lines.length ? null : lines[index++];
	}

	public function readEntry(entry:Array<String>, line:String):Int {
		if (line == null)
			return 0;
		if (line.length == 0)
			return 0;
		var colon:Int = line.indexOf(':');
		if (colon == -1)
			return 0;
		entry[0] = trim(line.substr(0, colon));
		var i:Int = 1;
		var lastMatch:Int = colon + 1;
		while (true) {
			var comma:Int = line.indexOf(',', lastMatch);
			if (comma == -1) {
				entry[i] = trim(line.substr(lastMatch));
				return i;
			}
			entry[i] = trim(line.substr(lastMatch, comma - lastMatch));
			lastMatch = comma + 1;
			if (i == 4)
				return 4;

			i++;
		}
	}
}
