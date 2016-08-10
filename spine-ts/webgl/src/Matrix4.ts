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

module spine.webgl {
	export let M00 = 0;
	export let M01 = 4;
	export let M02 = 8;
	export let M03 = 12;
	export let M10 = 1;
	export let M11 = 5;
	export let M12 = 9;
	export let M13 = 13;
	export let M20 = 2;
	export let M21 = 6;
	export let M22 = 10;
	export let M23 = 14;
	export let M30 = 3;
	export let M31 = 7;
	export let M32 = 11;
	export let M33 = 15;

	export class Matrix4 {
		temp: Float32Array = new Float32Array(16);
		values: Float32Array = new Float32Array(16);

		constructor() {
			this.values[M00] = 1;
			this.values[M11] = 1;
			this.values[M22] = 1;
			this.values[M33] = 1;
		}

		set(values: Float32Array | Array<number>): Matrix4 {
			this.values.set(values);
			return this;
		}

		transpose(): Matrix4 {
			this.temp[M00] = this.values[M00];
			this.temp[M01] = this.values[M10];
			this.temp[M02] = this.values[M20];
			this.temp[M03] = this.values[M30];
			this.temp[M10] = this.values[M01];
			this.temp[M11] = this.values[M11];
			this.temp[M12] = this.values[M21];
			this.temp[M13] = this.values[M31];
			this.temp[M20] = this.values[M02];
			this.temp[M21] = this.values[M12];
			this.temp[M22] = this.values[M22];
			this.temp[M23] = this.values[M32];
			this.temp[M30] = this.values[M03];
			this.temp[M31] = this.values[M13];
			this.temp[M32] = this.values[M23];
			this.temp[M33] = this.values[M33];
			return this.set(this.temp);
		}

		identity(): Matrix4 {            
			this.values[M00] = 1;
			this.values[M01] = 0;
			this.values[M02] = 0;
			this.values[M03] = 0;
			this.values[M10] = 0;
			this.values[M11] = 1;
			this.values[M12] = 0;
			this.values[M13] = 0;
			this.values[M20] = 0;
			this.values[M21] = 0;
			this.values[M22] = 1;
			this.values[M23] = 0;
			this.values[M30] = 0;
			this.values[M31] = 0;
			this.values[M32] = 0;
			this.values[M33] = 1;
			return this;
		}

		invert(): Matrix4 {
			let l_det = this.values[M30] * this.values[M21] * this.values[M12] * this.values[M03] - this.values[M20] * this.values[M31] * this.values[M12] * this.values[M03] - this.values[M30] * this.values[M11]
				* this.values[M22] * this.values[M03] + this.values[M10] * this.values[M31] * this.values[M22] * this.values[M03] + this.values[M20] * this.values[M11] * this.values[M32] * this.values[M03] - this.values[M10]
				* this.values[M21] * this.values[M32] * this.values[M03] - this.values[M30] * this.values[M21] * this.values[M02] * this.values[M13] + this.values[M20] * this.values[M31] * this.values[M02] * this.values[M13]
				+ this.values[M30] * this.values[M01] * this.values[M22] * this.values[M13] - this.values[M00] * this.values[M31] * this.values[M22] * this.values[M13] - this.values[M20] * this.values[M01] * this.values[M32]
				* this.values[M13] + this.values[M00] * this.values[M21] * this.values[M32] * this.values[M13] + this.values[M30] * this.values[M11] * this.values[M02] * this.values[M23] - this.values[M10] * this.values[M31]
				* this.values[M02] * this.values[M23] - this.values[M30] * this.values[M01] * this.values[M12] * this.values[M23] + this.values[M00] * this.values[M31] * this.values[M12] * this.values[M23] + this.values[M10]
				* this.values[M01] * this.values[M32] * this.values[M23] - this.values[M00] * this.values[M11] * this.values[M32] * this.values[M23] - this.values[M20] * this.values[M11] * this.values[M02] * this.values[M33]
				+ this.values[M10] * this.values[M21] * this.values[M02] * this.values[M33] + this.values[M20] * this.values[M01] * this.values[M12] * this.values[M33] - this.values[M00] * this.values[M21] * this.values[M12]
				* this.values[M33] - this.values[M10] * this.values[M01] * this.values[M22] * this.values[M33] + this.values[M00] * this.values[M11] * this.values[M22] * this.values[M33];
			if (l_det == 0) throw new Error("non-invertible matrix");
			let inv_det = 1.0 / l_det;
			this.temp[M00] = this.values[M12] * this.values[M23] * this.values[M31] - this.values[M13] * this.values[M22] * this.values[M31] + this.values[M13] * this.values[M21] * this.values[M32] - this.values[M11]
				* this.values[M23] * this.values[M32] - this.values[M12] * this.values[M21] * this.values[M33] + this.values[M11] * this.values[M22] * this.values[M33];
			this.temp[M01] = this.values[M03] * this.values[M22] * this.values[M31] - this.values[M02] * this.values[M23] * this.values[M31] - this.values[M03] * this.values[M21] * this.values[M32] + this.values[M01]
				* this.values[M23] * this.values[M32] + this.values[M02] * this.values[M21] * this.values[M33] - this.values[M01] * this.values[M22] * this.values[M33];
			this.temp[M02] = this.values[M02] * this.values[M13] * this.values[M31] - this.values[M03] * this.values[M12] * this.values[M31] + this.values[M03] * this.values[M11] * this.values[M32] - this.values[M01]
				* this.values[M13] * this.values[M32] - this.values[M02] * this.values[M11] * this.values[M33] + this.values[M01] * this.values[M12] * this.values[M33];
			this.temp[M03] = this.values[M03] * this.values[M12] * this.values[M21] - this.values[M02] * this.values[M13] * this.values[M21] - this.values[M03] * this.values[M11] * this.values[M22] + this.values[M01]
				* this.values[M13] * this.values[M22] + this.values[M02] * this.values[M11] * this.values[M23] - this.values[M01] * this.values[M12] * this.values[M23];
			this.temp[M10] = this.values[M13] * this.values[M22] * this.values[M30] - this.values[M12] * this.values[M23] * this.values[M30] - this.values[M13] * this.values[M20] * this.values[M32] + this.values[M10]
				* this.values[M23] * this.values[M32] + this.values[M12] * this.values[M20] * this.values[M33] - this.values[M10] * this.values[M22] * this.values[M33];
			this.temp[M11] = this.values[M02] * this.values[M23] * this.values[M30] - this.values[M03] * this.values[M22] * this.values[M30] + this.values[M03] * this.values[M20] * this.values[M32] - this.values[M00]
				* this.values[M23] * this.values[M32] - this.values[M02] * this.values[M20] * this.values[M33] + this.values[M00] * this.values[M22] * this.values[M33];
			this.temp[M12] = this.values[M03] * this.values[M12] * this.values[M30] - this.values[M02] * this.values[M13] * this.values[M30] - this.values[M03] * this.values[M10] * this.values[M32] + this.values[M00]
				* this.values[M13] * this.values[M32] + this.values[M02] * this.values[M10] * this.values[M33] - this.values[M00] * this.values[M12] * this.values[M33];
			this.temp[M13] = this.values[M02] * this.values[M13] * this.values[M20] - this.values[M03] * this.values[M12] * this.values[M20] + this.values[M03] * this.values[M10] * this.values[M22] - this.values[M00]
				* this.values[M13] * this.values[M22] - this.values[M02] * this.values[M10] * this.values[M23] + this.values[M00] * this.values[M12] * this.values[M23];
			this.temp[M20] = this.values[M11] * this.values[M23] * this.values[M30] - this.values[M13] * this.values[M21] * this.values[M30] + this.values[M13] * this.values[M20] * this.values[M31] - this.values[M10]
				* this.values[M23] * this.values[M31] - this.values[M11] * this.values[M20] * this.values[M33] + this.values[M10] * this.values[M21] * this.values[M33];
			this.temp[M21] = this.values[M03] * this.values[M21] * this.values[M30] - this.values[M01] * this.values[M23] * this.values[M30] - this.values[M03] * this.values[M20] * this.values[M31] + this.values[M00]
				* this.values[M23] * this.values[M31] + this.values[M01] * this.values[M20] * this.values[M33] - this.values[M00] * this.values[M21] * this.values[M33];
			this.temp[M22] = this.values[M01] * this.values[M13] * this.values[M30] - this.values[M03] * this.values[M11] * this.values[M30] + this.values[M03] * this.values[M10] * this.values[M31] - this.values[M00]
				* this.values[M13] * this.values[M31] - this.values[M01] * this.values[M10] * this.values[M33] + this.values[M00] * this.values[M11] * this.values[M33];
			this.temp[M23] = this.values[M03] * this.values[M11] * this.values[M20] - this.values[M01] * this.values[M13] * this.values[M20] - this.values[M03] * this.values[M10] * this.values[M21] + this.values[M00]
				* this.values[M13] * this.values[M21] + this.values[M01] * this.values[M10] * this.values[M23] - this.values[M00] * this.values[M11] * this.values[M23];
			this.temp[M30] = this.values[M12] * this.values[M21] * this.values[M30] - this.values[M11] * this.values[M22] * this.values[M30] - this.values[M12] * this.values[M20] * this.values[M31] + this.values[M10]
				* this.values[M22] * this.values[M31] + this.values[M11] * this.values[M20] * this.values[M32] - this.values[M10] * this.values[M21] * this.values[M32];
			this.temp[M31] = this.values[M01] * this.values[M22] * this.values[M30] - this.values[M02] * this.values[M21] * this.values[M30] + this.values[M02] * this.values[M20] * this.values[M31] - this.values[M00]
				* this.values[M22] * this.values[M31] - this.values[M01] * this.values[M20] * this.values[M32] + this.values[M00] * this.values[M21] * this.values[M32];
			this.temp[M32] = this.values[M02] * this.values[M11] * this.values[M30] - this.values[M01] * this.values[M12] * this.values[M30] - this.values[M02] * this.values[M10] * this.values[M31] + this.values[M00]
				* this.values[M12] * this.values[M31] + this.values[M01] * this.values[M10] * this.values[M32] - this.values[M00] * this.values[M11] * this.values[M32];
			this.temp[M33] = this.values[M01] * this.values[M12] * this.values[M20] - this.values[M02] * this.values[M11] * this.values[M20] + this.values[M02] * this.values[M10] * this.values[M21] - this.values[M00]
				* this.values[M12] * this.values[M21] - this.values[M01] * this.values[M10] * this.values[M22] + this.values[M00] * this.values[M11] * this.values[M22];
			this.values[M00] = this.temp[M00] * inv_det;
			this.values[M01] = this.temp[M01] * inv_det;
			this.values[M02] = this.temp[M02] * inv_det;
			this.values[M03] = this.temp[M03] * inv_det;
			this.values[M10] = this.temp[M10] * inv_det;
			this.values[M11] = this.temp[M11] * inv_det;
			this.values[M12] = this.temp[M12] * inv_det;
			this.values[M13] = this.temp[M13] * inv_det;
			this.values[M20] = this.temp[M20] * inv_det;
			this.values[M21] = this.temp[M21] * inv_det;
			this.values[M22] = this.temp[M22] * inv_det;
			this.values[M23] = this.temp[M23] * inv_det;
			this.values[M30] = this.temp[M30] * inv_det;
			this.values[M31] = this.temp[M31] * inv_det;
			this.values[M32] = this.temp[M32] * inv_det;
			this.values[M33] = this.temp[M33] * inv_det;
			return this;
		}

		determinant(): number {	
			return this.values[M30] * this.values[M21] * this.values[M12] * this.values[M03] - this.values[M20] * this.values[M31] * this.values[M12] * this.values[M03] - this.values[M30] * this.values[M11]
				* this.values[M22] * this.values[M03] + this.values[M10] * this.values[M31] * this.values[M22] * this.values[M03] + this.values[M20] * this.values[M11] * this.values[M32] * this.values[M03] - this.values[M10]
				* this.values[M21] * this.values[M32] * this.values[M03] - this.values[M30] * this.values[M21] * this.values[M02] * this.values[M13] + this.values[M20] * this.values[M31] * this.values[M02] * this.values[M13]
				+ this.values[M30] * this.values[M01] * this.values[M22] * this.values[M13] - this.values[M00] * this.values[M31] * this.values[M22] * this.values[M13] - this.values[M20] * this.values[M01] * this.values[M32]
				* this.values[M13] + this.values[M00] * this.values[M21] * this.values[M32] * this.values[M13] + this.values[M30] * this.values[M11] * this.values[M02] * this.values[M23] - this.values[M10] * this.values[M31]
				* this.values[M02] * this.values[M23] - this.values[M30] * this.values[M01] * this.values[M12] * this.values[M23] + this.values[M00] * this.values[M31] * this.values[M12] * this.values[M23] + this.values[M10]
				* this.values[M01] * this.values[M32] * this.values[M23] - this.values[M00] * this.values[M11] * this.values[M32] * this.values[M23] - this.values[M20] * this.values[M11] * this.values[M02] * this.values[M33]
				+ this.values[M10] * this.values[M21] * this.values[M02] * this.values[M33] + this.values[M20] * this.values[M01] * this.values[M12] * this.values[M33] - this.values[M00] * this.values[M21] * this.values[M12]
				* this.values[M33] - this.values[M10] * this.values[M01] * this.values[M22] * this.values[M33] + this.values[M00] * this.values[M11] * this.values[M22] * this.values[M33];	
		}

		translate(x: number, y: number, z: number): Matrix4 {
			this.values[M03] += x;
			this.values[M13] += y;
			this.values[M23] += z;
			return this;
	    }

		copy(): Matrix4 {
			return new Matrix4().set(this.values);
		}

		projection(near: number, far: number, fovy: number, aspectRatio: number): Matrix4 {
			this.identity();
			let l_fd = (1.0 / Math.tan((fovy * (Math.PI / 180)) / 2.0));
			let l_a1 = (far + near) / (near - far);
			let l_a2 = (2 * far * near) / (near - far);
			this.values[M00] = l_fd / aspectRatio;
			this.values[M10] = 0;
			this.values[M20] = 0;
			this.values[M30] = 0;
			this.values[M01] = 0;
			this.values[M11] = l_fd;
			this.values[M21] = 0;
			this.values[M31] = 0;
			this.values[M02] = 0;
			this.values[M12] = 0;
			this.values[M22] = l_a1;
			this.values[M32] = -1;
			this.values[M03] = 0;
			this.values[M13] = 0;
			this.values[M23] = l_a2;
			this.values[M33] = 0;

			return this;
		}

		ortho2d(x: number, y: number, width: number, height: number): Matrix4 {
			return this.ortho(x, x + width, y, y + height, 0, 1);
		}

		ortho(left: number, right: number, bottom: number, top: number, near: number, far: number): Matrix4 {
			this.identity();
			let x_orth = 2 / (right - left);
			let y_orth = 2 / (top - bottom);
			let z_orth = -2 / (far - near);

			let tx = -(right + left) / (right - left);
			let ty = -(top + bottom) / (top - bottom);
			let tz = -(far + near) / (far - near);

			this.values[M00] = x_orth;
			this.values[M10] = 0;
			this.values[M20] = 0;
			this.values[M30] = 0;
			this.values[M01] = 0;
			this.values[M11] = y_orth;
			this.values[M21] = 0;
			this.values[M31] = 0;
			this.values[M02] = 0;
			this.values[M12] = 0;
			this.values[M22] = z_orth;
			this.values[M32] = 0;
			this.values[M03] = tx;
			this.values[M13] = ty;
			this.values[M23] = tz;
			this.values[M33] = 1;
		
			return this;
		}

		multiply(matrix: Matrix4): Matrix4 {
			this.temp[M00] = this.values[M00] * matrix.values[M00] + this.values[M01] * matrix.values[M10] + this.values[M02] * matrix.values[M20] + this.values[M03]
				* matrix.values[M30];
			this.temp[M01] = this.values[M00] * matrix.values[M01] + this.values[M01] * matrix.values[M11] + this.values[M02] * matrix.values[M21] + this.values[M03]
				* matrix.values[M31];
			this.temp[M02] = this.values[M00] * matrix.values[M02] + this.values[M01] * matrix.values[M12] + this.values[M02] * matrix.values[M22] + this.values[M03]
				* matrix.values[M32];
			this.temp[M03] = this.values[M00] * matrix.values[M03] + this.values[M01] * matrix.values[M13] + this.values[M02] * matrix.values[M23] + this.values[M03]
				* matrix.values[M33];
			this.temp[M10] = this.values[M10] * matrix.values[M00] + this.values[M11] * matrix.values[M10] + this.values[M12] * matrix.values[M20] + this.values[M13]
				* matrix.values[M30];
			this.temp[M11] = this.values[M10] * matrix.values[M01] + this.values[M11] * matrix.values[M11] + this.values[M12] * matrix.values[M21] + this.values[M13]
				* matrix.values[M31];
			this.temp[M12] = this.values[M10] * matrix.values[M02] + this.values[M11] * matrix.values[M12] + this.values[M12] * matrix.values[M22] + this.values[M13]
				* matrix.values[M32];
			this.temp[M13] = this.values[M10] * matrix.values[M03] + this.values[M11] * matrix.values[M13] + this.values[M12] * matrix.values[M23] + this.values[M13]
				* matrix.values[M33];
			this.temp[M20] = this.values[M20] * matrix.values[M00] + this.values[M21] * matrix.values[M10] + this.values[M22] * matrix.values[M20] + this.values[M23]
				* matrix.values[M30];
			this.temp[M21] = this.values[M20] * matrix.values[M01] + this.values[M21] * matrix.values[M11] + this.values[M22] * matrix.values[M21] + this.values[M23]
				* matrix.values[M31];
			this.temp[M22] = this.values[M20] * matrix.values[M02] + this.values[M21] * matrix.values[M12] + this.values[M22] * matrix.values[M22] + this.values[M23]
				* matrix.values[M32];
			this.temp[M23] = this.values[M20] * matrix.values[M03] + this.values[M21] * matrix.values[M13] + this.values[M22] * matrix.values[M23] + this.values[M23]
				* matrix.values[M33];
			this.temp[M30] = this.values[M30] * matrix.values[M00] + this.values[M31] * matrix.values[M10] + this.values[M32] * matrix.values[M20] + this.values[M33]
				* matrix.values[M30];
			this.temp[M31] = this.values[M30] * matrix.values[M01] + this.values[M31] * matrix.values[M11] + this.values[M32] * matrix.values[M21] + this.values[M33]
				* matrix.values[M31];
			this.temp[M32] = this.values[M30] * matrix.values[M02] + this.values[M31] * matrix.values[M12] + this.values[M32] * matrix.values[M22] + this.values[M33]
				* matrix.values[M32];
			this.temp[M33] = this.values[M30] * matrix.values[M03] + this.values[M31] * matrix.values[M13] + this.values[M32] * matrix.values[M23] + this.values[M33]
				* matrix.values[M33];
			return this.set(this.temp);
		}

		multiplyLeft(matrix: Matrix4): Matrix4 {
			this.temp[M00] = matrix.values[M00] * this.values[M00] + matrix.values[M01] * this.values[M10] + matrix.values[M02] * this.values[M20] + matrix.values[M03]
				* this.values[M30];
			this.temp[M01] = matrix.values[M00] * this.values[M01] + matrix.values[M01] * this.values[M11] + matrix.values[M02] * this.values[M21] + matrix.values[M03]
				* this.values[M31];
			this.temp[M02] = matrix.values[M00] * this.values[M02] + matrix.values[M01] * this.values[M12] + matrix.values[M02] * this.values[M22] + matrix.values[M03]
				* this.values[M32];
			this.temp[M03] = matrix.values[M00] * this.values[M03] + matrix.values[M01] * this.values[M13] + matrix.values[M02] * this.values[M23] + matrix.values[M03]
				* this.values[M33];
			this.temp[M10] = matrix.values[M10] * this.values[M00] + matrix.values[M11] * this.values[M10] + matrix.values[M12] * this.values[M20] + matrix.values[M13]
				* this.values[M30];
			this.temp[M11] = matrix.values[M10] * this.values[M01] + matrix.values[M11] * this.values[M11] + matrix.values[M12] * this.values[M21] + matrix.values[M13]
				* this.values[M31];
			this.temp[M12] = matrix.values[M10] * this.values[M02] + matrix.values[M11] * this.values[M12] + matrix.values[M12] * this.values[M22] + matrix.values[M13]
				* this.values[M32];
			this.temp[M13] = matrix.values[M10] * this.values[M03] + matrix.values[M11] * this.values[M13] + matrix.values[M12] * this.values[M23] + matrix.values[M13]
				* this.values[M33];
			this.temp[M20] = matrix.values[M20] * this.values[M00] + matrix.values[M21] * this.values[M10] + matrix.values[M22] * this.values[M20] + matrix.values[M23]
				* this.values[M30];
			this.temp[M21] = matrix.values[M20] * this.values[M01] + matrix.values[M21] * this.values[M11] + matrix.values[M22] * this.values[M21] + matrix.values[M23]
				* this.values[M31];
			this.temp[M22] = matrix.values[M20] * this.values[M02] + matrix.values[M21] * this.values[M12] + matrix.values[M22] * this.values[M22] + matrix.values[M23]
				* this.values[M32];
			this.temp[M23] = matrix.values[M20] * this.values[M03] + matrix.values[M21] * this.values[M13] + matrix.values[M22] * this.values[M23] + matrix.values[M23]
				* this.values[M33];
			this.temp[M30] = matrix.values[M30] * this.values[M00] + matrix.values[M31] * this.values[M10] + matrix.values[M32] * this.values[M20] + matrix.values[M33]
				* this.values[M30];
			this.temp[M31] = matrix.values[M30] * this.values[M01] + matrix.values[M31] * this.values[M11] + matrix.values[M32] * this.values[M21] + matrix.values[M33]
				* this.values[M31];
			this.temp[M32] = matrix.values[M30] * this.values[M02] + matrix.values[M31] * this.values[M12] + matrix.values[M32] * this.values[M22] + matrix.values[M33]
				* this.values[M32];
			this.temp[M33] = matrix.values[M30] * this.values[M03] + matrix.values[M31] * this.values[M13] + matrix.values[M32] * this.values[M23] + matrix.values[M33]
				* this.values[M33];
			return this.set(this.temp);
		}
	}
}
