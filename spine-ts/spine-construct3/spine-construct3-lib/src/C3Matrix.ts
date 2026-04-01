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

import { type Bone, Vector2 } from "@esotericsoftware/spine-core";

export class C3Matrix {

	public a = 0;
	public b = 0;
	public c = 0;
	public d = 0;
	public tx = 0;
	public ty = 0;
	public tz = 0;

	public prevX = Infinity;
	public prevY = Infinity;
	public prevZ = Infinity;
	public prevAngle = Infinity;
	public prevScaleX = Infinity;
	public prevScaleY = Infinity;

	private tempPoint = new Vector2();

	public update (x: number, y: number, z: number, angle: number, scaleX = 1, scaleY = 1) {
		if (this.prevX === x && this.prevY === y && this.prevZ === z &&
			this.prevAngle === angle &&
			this.prevScaleX === scaleX && this.prevScaleY === scaleY) return false;
		this.prevX = x;
		this.prevY = y;
		this.prevZ = z;
		this.prevAngle = angle;
		this.prevScaleX = scaleX;
		this.prevScaleY = scaleY;
		const cos = Math.cos(angle);
		const sin = Math.sin(angle);
		this.a = scaleX * cos;
		this.b = scaleX * sin;
		this.c = -scaleY * sin;
		this.d = scaleY * cos;
		this.tx = x;
		this.ty = y;
		this.tz = z;
		return true;
	}

	public gameToSkeleton (x: number, y: number) {
		const tx = x - this.tx;
		const ty = y - this.ty;
		const { a, b, c, d, tempPoint } = this;
		const delta = a * d - b * c;
		tempPoint.x = (d * tx - c * ty) / delta;
		tempPoint.y = (a * ty - b * tx) / delta;
		return tempPoint;
	}

	public gameToBone (x: number, y: number, bone: Bone) {
		const point = this.gameToSkeleton(x, y);
		if (bone.parent)
			return bone.parent.appliedPose.worldToLocal(point);
		return bone.appliedPose.worldToLocal(point);
	}

	public skeletonToGame = (skeletonX: number, skeletonY: number) => {
		const { a, b, c, d, tempPoint } = this;
		tempPoint.x = a * skeletonX + c * skeletonY + this.tx;
		tempPoint.y = b * skeletonX + d * skeletonY + this.ty;
		return tempPoint;
	}

	public boneToGame (bone: Bone) {
		const { appliedPose } = bone;
		return this.skeletonToGame(appliedPose.worldX, appliedPose.worldY);
	}

	public gameToBoneRotation (gameAngleDeg: number, bone: Bone) {
		return bone.appliedPose.worldToLocalRotation(this.gameToSkeletonRotation(gameAngleDeg)) - 180;
	}

	public gameToSkeletonRotation (gameAngleDeg: number) {
		const rad = gameAngleDeg * Math.PI / 180;
		const sin = Math.sin(rad), cos = Math.cos(rad);
		const { a, b, c, d } = this;
		return Math.atan2(a * sin - b * cos, d * cos - c * sin) * (180 / Math.PI);
	}

}
