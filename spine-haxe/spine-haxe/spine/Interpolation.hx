/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

package spine;

/** Takes a linear value in the range of 0-1 and outputs a usually non-linear, interpolated value. */
class Interpolation {
	public static var linear(get, never):Interpolation;
	public static var smooth(get, never):Interpolation;
	public static var slowFast(get, never):Interpolation;
	public static var fastSlow(get, never):Interpolation;
	public static var circle(get, never):Interpolation;

	private static var _linear:Interpolation;
	private static var _smooth:Interpolation;
	private static var _slowFast:Interpolation;
	private static var _fastSlow:Interpolation;
	private static var _circle:Interpolation;

	static function get_linear():Interpolation {
		if (_linear == null)
			_linear = new Interpolation(function(a:Float):Float return a);
		return _linear;
	}

	/** Aka "smoothstep". */
	static function get_smooth():Interpolation {
		if (_smooth == null)
			_smooth = new Interpolation(function(a:Float):Float return a * a * (3 - 2 * a));
		return _smooth;
	}

	/** Slow, then fast. */
	static function get_slowFast():Interpolation {
		if (_slowFast == null)
			_slowFast = new PowIn(2);
		return _slowFast;
	}

	/** Fast, then slow. */
	static function get_fastSlow():Interpolation {
		if (_fastSlow == null)
			_fastSlow = new PowOut(2);
		return _fastSlow;
	}

	static function get_circle():Interpolation {
		if (_circle == null) {
			_circle = new Interpolation(function(a:Float):Float {
				if (a <= 0.5) {
					a *= 2;
					return (1 - Math.sqrt(1 - a * a)) / 2;
				}
				a--;
				a *= 2;
				return (Math.sqrt(1 - a * a) + 1) / 2;
			});
		}
		return _circle;
	}

	private var applyFunc:Float->Float;

	public function new(applyFunc:Float->Float) {
		this.applyFunc = applyFunc;
	}

	/** @param a Alpha value between 0 and 1. */
	public function apply(a:Float):Float {
		return applyFunc(a);
	}

	public function applyRange(start:Float, end:Float, a:Float):Float {
		return start + (end - start) * apply(a);
	}
}

class Pow extends Interpolation {
	var power:Int;

	public function new(power:Int) {
		super(null);
		this.power = power;
	}

	override public function apply(a:Float):Float {
		if (a <= 0.5)
			return Math.pow(a * 2, power) / 2;
		return Math.pow((a - 1) * 2, power) / (power % 2 == 0 ? -2 : 2) + 1;
	}
}

class PowIn extends Pow {
	public function new(power:Int) {
		super(power);
	}

	override public function apply(a:Float):Float {
		return Math.pow(a, power);
	}
}

class PowOut extends Pow {
	public function new(power:Int) {
		super(power);
	}

	override public function apply(a:Float):Float {
		return Math.pow(a - 1, power) * (power % 2 == 0 ? -1 : 1) + 1;
	}
}
