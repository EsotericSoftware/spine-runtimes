package spine;

import openfl.utils.ByteArray;
import openfl.Vector;

class BinaryInput {
	private var bytes:ByteArray;

	public var strings:Vector<String> = new Vector<String>();

	public function new(bytes:ByteArray) {
		this.bytes = bytes;
	}

	public function readByte():Int {
		return bytes.readByte();
	}

	public function readUnsignedByte():Int {
		return bytes.readUnsignedByte();
	}

	public function readShort():Int {
		return bytes.readShort();
	}

	public function readInt32():Int {
		return bytes.readInt();
	}

	public function readInt(optimizePositive:Bool):Int {
		var b:Int = readByte();
		var result:Int = b & 0x7F;
		if ((b & 0x80) != 0) {
			b = readByte();
			result |= (b & 0x7F) << 7;
			if ((b & 0x80) != 0) {
				b = readByte();
				result |= (b & 0x7F) << 14;
				if ((b & 0x80) != 0) {
					b = readByte();
					result |= (b & 0x7F) << 21;
					if ((b & 0x80) != 0) {
						b = readByte();
						result |= (b & 0x7F) << 28;
					}
				}
			}
		}
		return optimizePositive ? result : ((result >>> 1) ^ -(result & 1));
	}

	public function readStringRef():String {
		var index:Int = readInt(true);
		return index == 0 ? null : strings[index - 1];
	}

	public function readString():String {
		var byteCount:Int = readInt(true);
		switch (byteCount) {
			case 0:
				return null;
			case 1:
				return "";
		}
		byteCount--;
		var chars:String = "";
		var i:Int = 0;
		while (i < byteCount) {
			var b:Int = readByte();
			switch (b >> 4) {
				case 12, 13:
					chars += String.fromCharCode(((b & 0x1F) << 6 | readByte() & 0x3F));
					i += 2;
				case 14:
					chars += String.fromCharCode(((b & 0x0F) << 12 | (readByte() & 0x3F) << 6 | readByte() & 0x3F));
					i += 3;
				default:
					chars += String.fromCharCode(b);
					i++;
			}
		}
		return chars;
	}

	public function readFloat():Float {
		return bytes.readFloat();
	}

	public function readBoolean():Bool {
		return this.readByte() != 0;
	}
}
