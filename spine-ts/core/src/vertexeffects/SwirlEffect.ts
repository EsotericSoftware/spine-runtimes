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

module spine {
	export class SwirlEffect implements VertexEffect {
		static interpolation = new PowOut(2);
		centerX = 0;
		centerY = 0;
		radius = 0;
		angle = 0;
		private worldX = 0;
		private worldY = 0;

		constructor (radius: number) {
			this.radius = radius;
		}

		begin(skeleton: Skeleton): void {
			this.worldX = skeleton.x + this.centerX;
			this.worldY = skeleton.y + this.centerY;
		}

		transform(position: Vector2, uv: Vector2, light: Color, dark: Color): void {
			let radAngle = this.angle * MathUtils.degreesToRadians;
			let x = position.x - this.worldX;
			let y = position.y - this.worldY;
			let dist = Math.sqrt(x * x + y * y);
			if (dist < this.radius) {
				let theta = SwirlEffect.interpolation.apply(0, radAngle, (this.radius - dist) / this.radius);
				let cos = Math.cos(theta);
				let sin = Math.sin(theta);
				position.x = cos * x - sin * y + this.worldX;
				position.y = sin * x + cos * y + this.worldY;
			}
		}

		end(): void {
		}
	}
}
