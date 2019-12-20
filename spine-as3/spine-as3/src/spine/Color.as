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
	public class Color {
		public static var WHITE : Color = new Color(1, 1, 1, 1);
		public static var RED : Color = new Color(1, 0, 0, 1);
		public static var GREEN : Color = new Color(0, 1, 0, 1);
		public static var BLUE : Color = new Color(0, 0, 1, 1);
		public static var MAGENTA : Color = new Color(1, 0, 1, 1);
		public var r : Number = 0;
		public var g : Number = 0;
		public var b : Number = 0;
		public var a : Number = 0;

		public function Color(r : Number, g : Number, b : Number, a : Number = 0) {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public function setFrom(r : Number, g : Number, b : Number, a : Number) : Color {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
			this.clamp();
			return this;
		}

		public function setFromColor(c : Color) : Color {
			this.r = c.r;
			this.g = c.g;
			this.b = c.b;
			this.a = c.a;
			return this;
		}

		public function setFromString(hex : String) : Color {
			hex = hex.charAt(0) == '#' ? hex.substr(1) : hex;
			this.r = parseInt(hex.substr(0, 2), 16) / 255.0;
			this.g = parseInt(hex.substr(2, 2), 16) / 255.0;
			this.b = parseInt(hex.substr(4, 2), 16) / 255.0;
			this.a = (hex.length != 8 ? 255 : parseInt(hex.substr(6, 2), 16)) / 255.0;
			return this;
		}

		public function add(r : Number, g : Number, b : Number, a : Number) : Color {
			this.r += r;
			this.g += g;
			this.b += b;
			this.a += a;
			this.clamp();
			return this;
		}

		public function clamp() : Color {
			if (this.r < 0) this.r = 0;
			else if (this.r > 1) this.r = 1;

			if (this.g < 0) this.g = 0;
			else if (this.g > 1) this.g = 1;

			if (this.b < 0) this.b = 0;
			else if (this.b > 1) this.b = 1;

			if (this.a < 0) this.a = 0;
			else if (this.a > 1) this.a = 1;
			return this;
		}
		
		public function setFromRgba8888(value: int) : void {
			r = ((value & 0xff000000) >>> 24) / 255;
			g = ((value & 0x00ff0000) >>> 16) / 255;
			b = ((value & 0x0000ff00) >>> 8) / 255;
			a = ((value & 0x000000ff)) / 255;
		}

		public function setFromRgb888(value: int) : void {
			r = ((value & 0x00ff0000) >>> 16) / 255;
			g = ((value & 0x0000ff00) >>> 8) / 255;
			b = ((value & 0x000000ff)) / 255;
		}
	}
}
