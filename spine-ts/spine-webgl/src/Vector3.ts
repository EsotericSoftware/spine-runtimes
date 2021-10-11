/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

import { Matrix4, M00, M01, M02, M03, M10, M11, M12, M13, M20, M21, M22, M23, M30, M31, M32, M33 } from "./Matrix4";

export class Vector3 {
	x = 0;
	y = 0;
	z = 0;

	constructor (x: number = 0, y: number = 0, z: number = 0) {
		this.x = x;
		this.y = y;
		this.z = z;
	}

	setFrom (v: Vector3): Vector3 {
		this.x = v.x;
		this.y = v.y;
		this.z = v.z;
		return this;
	}

	set (x: number, y: number, z: number): Vector3 {
		this.x = x;
		this.y = y;
		this.z = z;
		return this;
	}

	add (v: Vector3): Vector3 {
		this.x += v.x;
		this.y += v.y;
		this.z += v.z;
		return this;
	}

	sub (v: Vector3): Vector3 {
		this.x -= v.x;
		this.y -= v.y;
		this.z -= v.z;
		return this;
	}

	scale (s: number): Vector3 {
		this.x *= s;
		this.y *= s;
		this.z *= s;
		return this;
	}

	normalize (): Vector3 {
		let len = this.length();
		if (len == 0) return this;
		len = 1 / len;
		this.x *= len;
		this.y *= len;
		this.z *= len;
		return this;
	}

	cross (v: Vector3): Vector3 {
		return this.set(this.y * v.z - this.z * v.y, this.z * v.x - this.x * v.z, this.x * v.y - this.y * v.x)
	}

	multiply (matrix: Matrix4): Vector3 {
		let l_mat = matrix.values;
		return this.set(this.x * l_mat[M00] + this.y * l_mat[M01] + this.z * l_mat[M02] + l_mat[M03],
			this.x * l_mat[M10] + this.y * l_mat[M11] + this.z * l_mat[M12] + l_mat[M13],
			this.x * l_mat[M20] + this.y * l_mat[M21] + this.z * l_mat[M22] + l_mat[M23]);
	}

	project (matrix: Matrix4): Vector3 {
		let l_mat = matrix.values;
		let l_w = 1 / (this.x * l_mat[M30] + this.y * l_mat[M31] + this.z * l_mat[M32] + l_mat[M33]);
		return this.set((this.x * l_mat[M00] + this.y * l_mat[M01] + this.z * l_mat[M02] + l_mat[M03]) * l_w,
			(this.x * l_mat[M10] + this.y * l_mat[M11] + this.z * l_mat[M12] + l_mat[M13]) * l_w,
			(this.x * l_mat[M20] + this.y * l_mat[M21] + this.z * l_mat[M22] + l_mat[M23]) * l_w);
	}

	dot (v: Vector3): number {
		return this.x * v.x + this.y * v.y + this.z * v.z;
	}

	length (): number {
		return Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
	}

	distance (v: Vector3): number {
		let a = v.x - this.x;
		let b = v.y - this.y;
		let c = v.z - this.z;
		return Math.sqrt(a * a + b * b + c * c);
	}
}
