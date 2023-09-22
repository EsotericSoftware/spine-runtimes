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

class Color {
	public static var WHITE:Color = new Color(1, 1, 1, 1);
	public static var RED:Color = new Color(1, 0, 0, 1);
	public static var GREEN:Color = new Color(0, 1, 0, 1);
	public static var BLUE:Color = new Color(0, 0, 1, 1);
	public static var MAGENTA:Color = new Color(1, 0, 1, 1);

	public var r:Float = 0;
	public var g:Float = 0;
	public var b:Float = 0;
	public var a:Float = 0;

	public function new(r:Float, g:Float, b:Float, a:Float = 0) {
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
		clamp();
	}

	public function setFromColor(c:Color):Color {
		r = c.r;
		g = c.g;
		b = c.b;
		a = c.a;
		clamp();
		return this;
	}

	public function setFromString(hex:String):Color {
		hex = hex.charAt(0) == '#' ? hex.substr(1) : hex;
		r = Std.parseInt("0x" + hex.substr(0, 2)) / 255.0;
		g = Std.parseInt("0x" + hex.substr(2, 2)) / 255.0;
		b = Std.parseInt("0x" + hex.substr(4, 2)) / 255.0;
		a = (hex.length != 8 ? 255 : Std.parseInt("0x" + hex.substr(6, 2))) / 255.0;
		clamp();
		return this;
	}

	public function set(r:Float, g:Float, b:Float, a:Float):Color {
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
		clamp();
		return this;
	}

	public function add(r:Float, g:Float, b:Float, a:Float):Color {
		this.r += r;
		this.g += g;
		this.b += b;
		this.a += a;
		clamp();
		return this;
	}

	public function setFromRgba8888(value:Int):Void {
		r = ((value & 0xff000000) >>> 24) / 255;
		g = ((value & 0x00ff0000) >>> 16) / 255;
		b = ((value & 0x0000ff00) >>> 8) / 255;
		a = ((value & 0x000000ff)) / 255;
		clamp();
	}

	public function setFromRgb888(value:Int):Void {
		r = ((value & 0x00ff0000) >>> 16) / 255;
		g = ((value & 0x0000ff00) >>> 8) / 255;
		b = ((value & 0x000000ff)) / 255;
		clamp();
	}

	private function clamp():Color {
		if (r < 0)
			r = 0;
		else if (r > 1)
			r = 1;

		if (g < 0)
			g = 0;
		else if (g > 1)
			g = 1;

		if (b < 0)
			b = 0;
		else if (b > 1)
			b = 1;

		if (a < 0)
			a = 0;
		else if (a > 1)
			a = 1;
		return this;
	}

	static public function fromString(hex:String):Color {
		return new Color(0, 0, 0, 0).setFromString(hex);
	}
}
