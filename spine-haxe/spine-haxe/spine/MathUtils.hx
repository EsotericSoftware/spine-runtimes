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

class MathUtils {
	static public var PI:Float = Math.PI;
	static public var PI2:Float = Math.PI * 2;
	static public var invPI2 = 1 / MathUtils.PI2;
	static public var radDeg:Float = 180 / Math.PI;
	static public var degRad:Float = Math.PI / 180;

	static public function cosDeg(degrees:Float):Float {
		return Math.cos(degrees * degRad);
	}

	static public function sinDeg(degrees:Float):Float {
		return Math.sin(degrees * degRad);
	}

	static public function atan2Deg (y:Float, x:Float):Float {
		return Math.atan2(y, x) * MathUtils.degRad;
	}

	static public function clamp(value:Float, min:Float, max:Float):Float {
		if (value < min)
			return min;
		if (value > max)
			return max;
		return value;
	}

	static public function signum(value:Float):Float {
		return value > 0 ? 1 : value < 0 ? -1 : 0;
	}

	static public function randomTriangular(min:Float, max:Float):Float {
		return randomTriangularWith(min, max, (min + max) * 0.5);
	}

	static public function randomTriangularWith(min:Float, max:Float, mode:Float):Float {
		var u:Float = Math.random();
		var d:Float = max - min;
		if (u <= (mode - min) / d)
			return min + Math.sqrt(u * d * (mode - min));
		return max - Math.sqrt((1 - u) * d * (max - mode));
	}
}
