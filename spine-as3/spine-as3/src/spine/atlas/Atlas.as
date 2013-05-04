package spine.atlas {
import flash.utils.ByteArray;

public class Atlas {
	private var pages:Vector.<AtlasPage> = new Vector.<AtlasPage>();
	private var regions:Vector.<AtlasRegion> = new Vector.<AtlasRegion>();
	private var textureLoader:TextureLoader;

	/** @param object A String or ByteArray. */
	public function Atlas (object:*, textureLoader:TextureLoader) {
		if (!object)
			return;
		if (object is String)
			load(String(object), textureLoader);
		else if (object is ByteArray)
			load(object.readUTFBytes(object.length), textureLoader);
		else
			throw new ArgumentError("object must be a TextureAtlas or AttachmentLoader.");
	}

	protected function load (atlasText:String, textureLoader:TextureLoader) : void {
		if (textureLoader == null)
			throw new ArgumentError("textureLoader cannot be null.");
		this.textureLoader = textureLoader;

		var reader:Reader = new Reader(atlasText);
		var tuple:Array = new Array();
		tuple.length = 4;
		var page:AtlasPage = null;
		while (true) {
			var line:String = reader.readLine();
			if (line == null)
				break;
			line = reader.trim(line);
			if (line.length == 0)
				page = null;
			else if (!page) {
				page = new AtlasPage();
				page.name = line;

				page.format = Format[reader.readValue()];

				reader.readTuple(tuple);
				page.minFilter = TextureFilter[tuple[0]];
				page.magFilter = TextureFilter[tuple[1]];

				var direction:String = reader.readValue();
				page.uWrap = TextureWrap.clampToEdge;
				page.vWrap = TextureWrap.clampToEdge;
				if (direction == "x")
					page.uWrap = TextureWrap.repeat;
				else if (direction == "y")
					page.vWrap = TextureWrap.repeat;
				else if (direction == "xy")
					page.uWrap = page.vWrap = TextureWrap.repeat;

				textureLoader.load(page, line);

				pages.push(page);

			} else {
				var region:AtlasRegion = new AtlasRegion();
				region.name = line;
				region.page = page;

				region.rotate = reader.readValue() == "true";

				reader.readTuple(tuple);
				var x:int = parseInt(tuple[0]);
				var y:int = parseInt(tuple[1]);

				reader.readTuple(tuple);
				var width:int = parseInt(tuple[0]);
				var height:int = parseInt(tuple[1]);

				region.u = x / page.width;
				region.v = y / page.height;
				if (region.rotate) {
					region.u2 = (x + height) / page.width;
					region.v2 = (y + width) / page.height;
				} else {
					region.u2 = (x + width) / page.width;
					region.v2 = (y + height) / page.height;
				}
				region.x = x;
				region.y = y;
				region.width = Math.abs(width);
				region.height = Math.abs(height);

				if (reader.readTuple(tuple) == 4) { // split is optional
					region.splits = new Vector.<int>(parseInt(tuple[0]), parseInt(tuple[1]), parseInt(tuple[2]), parseInt(tuple[3]));

					if (reader.readTuple(tuple) == 4) { // pad is optional, but only present with splits
						region.pads = Vector.<int>(parseInt(tuple[0]), parseInt(tuple[1]), parseInt(tuple[2]), parseInt(tuple[3]));

						reader.readTuple(tuple);
					}
				}

				region.originalWidth = parseInt(tuple[0]);
				region.originalHeight = parseInt(tuple[1]);

				reader.readTuple(tuple);
				region.offsetX = parseInt(tuple[0]);
				region.offsetY = parseInt(tuple[1]);

				region.index = parseInt(reader.readValue());

				regions.push(region);
			}
		}
	}

	/** Returns the first region found with the specified name. This method uses string comparison to find the region, so the result
	 * should be cached rather than calling this method multiple times.
	 * @return The region, or null. */
	public function findRegion (name:String) : AtlasRegion {
		for (var i:int = 0, n:int = regions.length; i < n; i++)
			if (regions[i].name == name)
				return regions[i];
		return null;
	}

	public function dispose () : void {
		for (var i:int = 0, n:int = pages.length; i < n; i++)
			textureLoader.unload(pages[i].rendererObject);
	}
}

}

class Reader {
	private var lines:Array;
	private var index:int;

	public function Reader (text:String) {
		lines = text.split(/\r\n|\r|\n/);
	}

	public function trim (value:String) : String {
		return value.replace(/^\s+|\s+$/gs, "");
	}

	public function readLine () : String {
		if (index >= lines.length)
			return null;
		return lines[index++];
	}

	public function readValue () : String {
		var line:String = readLine();
		var colon:int = line.indexOf(":");
		if (colon == -1)
			throw new Error("Invalid line: " + line);
		return trim(line.substring(colon + 1));
	}

	/** Returns the number of tuple values read (2 or 4). */
	public function readTuple (tuple:Array) : int {
		var line:String = readLine();
		var colon:int = line.indexOf(":");
		if (colon == -1)
			throw new Error("Invalid line: " + line);
		var i:int = 0, lastMatch:int = colon + 1;
		for (; i < 3; i++) {
			var comma:int = line.indexOf(",", lastMatch);
			if (comma == -1) {
				if (i == 0)
					throw new Error("Invalid line: " + line);
				break;
			}
			tuple[i] = trim(line.substr(lastMatch, comma - lastMatch));
			lastMatch = comma + 1;
		}
		tuple[i] = trim(line.substring(lastMatch));
		return i + 1;
	}
}
