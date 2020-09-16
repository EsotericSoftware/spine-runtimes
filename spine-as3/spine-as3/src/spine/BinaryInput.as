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

package spine {
	import flash.utils.ByteArray;
	
	class BinaryInput {
		private var bytes : ByteArray;	
		public var strings : Vector.<String> = new Vector.<String>();
		
		public function BinaryInput(bytes: ByteArray) {
			this.bytes = bytes;		
		}
		public function readByte() : int {
			return bytes.readByte();		
		}
	
		public function readShort() : int {
			return bytes.readShort();
		}
	
		public function readInt32(): int {
			 return bytes.readInt();
		}
	
		public function readInt(optimizePositive: Boolean) : int {
			var b : int = readByte();
			var result : int = b & 0x7F;
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
	
		public function readStringRef (): String {
			var index : int = readInt(true);
			return index == 0 ? null : strings[index - 1];
		}
	
		public function readString () : String {
			var byteCount : int = readInt(true);
			switch (byteCount) {
			case 0:
				return null;
			case 1:
				return "";
			}
			byteCount--;
			var chars : String = "";		
			for (var i : int = 0; i < byteCount;) {
				var b : int = readByte();
				switch (b >> 4) {
				case 12:
				case 13:
					chars += String.fromCharCode(((b & 0x1F) << 6 | readByte() & 0x3F));
					i += 2;
					break;
				case 14:
					chars += String.fromCharCode(((b & 0x0F) << 12 | (readByte() & 0x3F) << 6 | readByte() & 0x3F));
					i += 3;
					break;
				default:
					chars += String.fromCharCode(b);
					i++;
				}
			}
			return chars;
		}
	
		public function readFloat (): Number {
			return bytes.readFloat();		
		}
	
		public function readBoolean (): Boolean {
			return this.readByte() != 0;
		}
	}
}
