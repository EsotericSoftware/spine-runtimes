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

package spine;

import haxe.io.FPHelper;
import haxe.io.Bytes;

class BinaryInput {
	private var bytes:Bytes;
	private var index:Int = 0;

	public var strings:Array<String> = new Array<String>();

	public function new(bytes:Bytes) {
		this.bytes = bytes;
	}

	public function readByte():Int {
		var result = bytes.get(index++);
		if ((result & 0x80) != 0) {
			result |= 0xffffff00;
		}
		return result;
	}

	public function readUnsignedByte():Int {
		return bytes.get(index++);
	}

	public function readShort():Int {
		var ch1 = readUnsignedByte();
		var ch2 = readUnsignedByte();
		var result = ((ch1 << 8) | ch2);
		if ((result & 0x8000) != 0) {
			result |= 0xFFFF0000;
		}
		return result;
	}

	public function readInt32():Int {
		var ch1 = readUnsignedByte();
		var ch2 = readUnsignedByte();
		var ch3 = readUnsignedByte();
		var ch4 = readUnsignedByte();
		var result = (ch1 << 24) | (ch2 << 16) | (ch3 << 8) | ch4;
		return result;
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
		var idx:Int = readInt(true);
		return idx == 0 ? null : strings[idx - 1];
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
					chars += String.fromCharCode(b & 0xff);
					i++;
			}
		}
		return chars;
	}

	public function readFloat():Float {
		return FPHelper.i32ToFloat(readInt32());
	}

	public function readBoolean():Bool {
		return this.readByte() != 0;
	}
}
