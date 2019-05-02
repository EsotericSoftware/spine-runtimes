/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine {
	public class MathUtils {
		static public var PI : Number = Math.PI;
		static public var PI2 : Number = Math.PI * 2;
		static public var radDeg : Number = 180 / Math.PI;
		static public var degRad : Number = Math.PI / 180;

		static public function cosDeg(degrees : Number) : Number {
			return Math.cos(degrees * degRad);
		}

		static public function sinDeg(degrees : Number) : Number {
			return Math.sin(degrees * degRad);
		}

		static public function clamp(value : Number, min : Number, max : Number) : Number {
			if (value < min) return min;
			if (value > max) return max;
			return value;
		}

		static public function signum(value : Number) : Number {
			return value > 0 ? 1 : value < 0 ? -1 : 0;
		}

		static public function randomTriangular(min : Number, max : Number) : Number {
			return randomTriangularWith(min, max, (min + max) * 0.5);
		}

		static public function randomTriangularWith(min : Number, max : Number, mode : Number) : Number {
			var u : Number = Math.random();
			var d : Number = max - min;
			if (u <= (mode - min) / d) return min + Math.sqrt(u * d * (mode - min));
			return max - Math.sqrt((1 - u) * d * (max - mode));
		}
	}
}
