package spine;

import openfl.Vector;

class Sequence {
	private static var _nextID = 0;

	public var id = _nextID++;
	public var regions:Vector<TextureRegion>;
	public var start = 0;
	public var digits = 0;

	/** The index of the region to show for the setup pose. */
	public var setupIndex = 0;

	public function new(count:Int) {
		this.regions = new Vector<TextureRegion>(count);
	}

	public function copy():Sequence {
		var copy = new Sequence(this.regions.length);
		for (i in 0...this.regions.length) {
			copy.regions[i] = this.regions[i];
		}
		copy.start = this.start;
		copy.digits = this.digits;
		copy.setupIndex = this.setupIndex;
		return copy;
	}

	public function apply(slot:Slot, attachment:HasTextureRegion) {
		var index:Int = slot.sequenceIndex;
		if (index == -1)
			index = this.setupIndex;
		if (index >= this.regions.length)
			index = this.regions.length - 1;
		var region = this.regions[index];
		if (attachment.region != region) {
			attachment.region = region;
			attachment.updateRegion();
		}
	}

	public function getPath(basePath:String, index:Int):String {
		var result = basePath;
		var frame = Std.string(this.start + index);

		for (i in 0...(this.digits - frame.length)) {
			result += "0";
		}
		result += frame;
		return result;
	}
}
