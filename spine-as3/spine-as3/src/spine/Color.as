/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
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
	}
}