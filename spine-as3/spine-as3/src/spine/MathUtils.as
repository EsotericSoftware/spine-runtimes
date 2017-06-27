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