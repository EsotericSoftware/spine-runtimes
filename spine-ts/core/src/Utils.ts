/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.5
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

module spine {
	export interface Map<T> {
		[key: string]: T;
	}

	export interface Disposable {
		dispose (): void;
	}

	export class Color {
		public static WHITE = new Color(1, 1, 1, 1);
		public static RED = new Color(1, 0, 0, 1);
		public static GREEN = new Color(0, 1, 0, 1);
		public static BLUE = new Color(0, 0, 1, 1);
		public static MAGENTA = new Color(1, 0, 1, 1);

		constructor (public r: number = 0, public g: number = 0, public b: number = 0, public a: number = 0) {
		}

		set (r: number, g: number, b: number, a: number) {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
			this.clamp();
			return this;
		}

		setFromColor (c: Color) {
			this.r = c.r;
			this.g = c.g;
			this.b = c.b;
			this.a = c.a;
			return this;
		}

		setFromString (hex: string) {
			hex = hex.charAt(0) == '#' ? hex.substr(1) : hex;
			this.r = parseInt(hex.substr(0, 2), 16) / 255.0;
			this.g = parseInt(hex.substr(2, 2), 16) / 255.0;
			this.b = parseInt(hex.substr(4, 2), 16) / 255.0;
			this.a = (hex.length != 8 ? 255 : parseInt(hex.substr(6, 2), 16)) / 255.0;
			return this;
		}

		add (r: number, g: number, b: number, a: number) {
			this.r += r;
			this.g += g;
			this.b += b;
			this.a += a;
			this.clamp();
			return this;
		}

		clamp () {
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

	export class MathUtils {
		static PI = 3.1415927;
		static PI2 = MathUtils.PI * 2;
		static radiansToDegrees = 180 / MathUtils.PI;
		static radDeg = MathUtils.radiansToDegrees;
		static degreesToRadians = MathUtils.PI / 180;
		static degRad = MathUtils.degreesToRadians;

		static clamp (value: number, min: number, max: number) {
			if (value < min) return min;
			if (value > max) return max;
			return value;
		}

		static cosDeg (degrees: number) {
			return Math.cos(degrees * MathUtils.degRad);
		}

		static sinDeg (degrees: number) {
			return Math.sin(degrees * MathUtils.degRad);
		}

		static signum (value: number): number {
			return value >= 0 ? 1 : -1;
		}

		static toInt (x: number) {
			return x > 0 ? Math.floor(x) : Math.ceil(x);
		}

		static cbrt (x: number) {
			var y = Math.pow(Math.abs(x), 1/3);
  			return x < 0 ? -y : y;
		}
	}

	export class Utils {
		static SUPPORTS_TYPED_ARRAYS = typeof(Float32Array) !== "undefined";

		static arrayCopy<T> (source: ArrayLike<T>, sourceStart: number, dest: ArrayLike<T>, destStart: number, numElements: number) {
			for (let i = sourceStart, j = destStart; i < sourceStart + numElements; i++, j++) {
				dest[j] = source[i];
			}
		}

		static setArraySize<T> (array: Array<T>, size: number, value: any = 0): Array<T> {
			let oldSize = array.length;
			if (oldSize == size) return array;
			array.length = size;
			if (oldSize < size) {
				for (let i = oldSize; i < size; i++) array[i] = value;
			}
			return array;
		}

		static newArray<T> (size: number, defaultValue: T): Array<T> {
			let array = new Array<T>(size);
			for (let i = 0; i < size; i++) array[i] = defaultValue;
			return array;
		}

		static newFloatArray (size: number): ArrayLike<number> {
			if (Utils.SUPPORTS_TYPED_ARRAYS) {
				return new Float32Array(size)
			} else {
				 let array = new Array<number>(size);
				 for (let i = 0; i < array.length; i++) array[i] = 0;
				 return array;
			}
		}

		static toFloatArray (array: Array<number>) {
			return Utils.SUPPORTS_TYPED_ARRAYS ? new Float32Array(array) : array;
		}
	}

	export class DebugUtils {
		static logBones(skeleton: Skeleton) {
			for (let i = 0; i < skeleton.bones.length; i++) {
				let bone = skeleton.bones[i];
				console.log(bone.data.name + ", " + bone.a + ", " + bone.b + ", " + bone.c + ", " + bone.d + ", " + bone.worldX + ", " + bone.worldY);
			}
		}
	}

	export class Pool<T> {
		private items = new Array<T>();
		private instantiator: () => T;

		constructor (instantiator: () => T) {
			this.instantiator = instantiator;
		}

		obtain () {
			return this.items.length > 0 ? this.items.pop() : this.instantiator();
		}

		free (item: T) {
			this.items.push(item);
		}

		freeAll (items: ArrayLike<T>) {
			for (let i = 0; i < items.length; i++) this.items[i] = items[i];
		}

		clear () {
			this.items.length = 0;
		}
	}

	export class Vector2 {
		constructor (public x = 0, public y = 0) {
		}

		set (x: number, y: number): Vector2 {
			this.x = x;
			this.y = y;
			return this;
		}

		length () {
			let x = this.x;
			let y = this.y;
			return Math.sqrt(x * x + y * y);
		}

		normalize () {
			let len = this.length();
			if (len != 0) {
				this.x /= len;
				this.y /= len;
			}
			return this;
		}
	}

	export class TimeKeeper {
		maxDelta = 0.064;		
		framesPerSecond = 0;
		delta = 0;
		totalTime = 0;

		private lastTime = Date.now() / 1000;
		private frameCount = 0;
		private frameTime = 0;

		update () {
			var now = Date.now() / 1000;
			this.delta = now - this.lastTime;
			this.frameTime += this.delta;
			this.totalTime += this.delta;
			if (this.delta > this.maxDelta) this.delta = this.maxDelta;
			this.lastTime = now;
			
			this.frameCount++;
			if (this.frameTime > 1) {
				this.framesPerSecond = this.frameCount / this.frameTime;
				this.frameTime = 0;
				this.frameCount = 0;
			}
		}
	}
}
