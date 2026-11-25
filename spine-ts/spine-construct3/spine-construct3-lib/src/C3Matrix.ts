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

const identityQuaternion = [0, 0, 0, 1] as const;

export class C3Matrix {

	public a = 0;
	public b = 0;
	public c = 0;
	public d = 0;
	public tx = 0;
	public ty = 0;
	public tz = 0;

	// separate 3D basis used only for rendering
	public renderXAxisX = 0;
	public renderXAxisY = 0;
	public renderXAxisZ = 0;
	public renderYAxisX = 0;
	public renderYAxisY = 0;
	public renderYAxisZ = 0;
	public renderZAxisX = 0;
	public renderZAxisY = 0;
	public renderZAxisZ = 1;
	public revision = 0;

	private prevScaleX = Infinity;
	private prevScaleY = Infinity;
	private prevObjectAngle = Infinity;
	private prevOffsetAngle = Infinity;
	private prevQuaternionX = Infinity;
	private prevQuaternionY = Infinity;
	private prevQuaternionZ = Infinity;
	private prevQuaternionW = Infinity;
	private tempPoint = new Vector2();

	public update (
		x: number,
		y: number,
		z: number,
		objectAngle: number,
		scaleX = 1,
		scaleY = 1,
		quaternion: ArrayLike<number> = identityQuaternion,
		offsetAngle = 0,
	) {
		const quaternionX = quaternion[0];
		const quaternionY = quaternion[1];
		const quaternionZ = quaternion[2];
		const quaternionW = quaternion[3];
		if (this.tx === x && this.ty === y && this.tz === z &&
			this.prevObjectAngle === objectAngle && this.prevOffsetAngle === offsetAngle &&
			this.prevScaleX === scaleX && this.prevScaleY === scaleY &&
			this.prevQuaternionX === quaternionX && this.prevQuaternionY === quaternionY &&
			this.prevQuaternionZ === quaternionZ && this.prevQuaternionW === quaternionW) return false;

		this.prevObjectAngle = objectAngle;
		this.prevOffsetAngle = offsetAngle;
		this.prevScaleX = scaleX;
		this.prevScaleY = scaleY;
		this.prevQuaternionX = quaternionX;
		this.prevQuaternionY = quaternionY;
		this.prevQuaternionZ = quaternionZ;
		this.prevQuaternionW = quaternionW;

		// keep pre-3D transform for all existing 2D APIs.
		const angle2D = objectAngle + offsetAngle;
		const cos2D = Math.cos(angle2D);
		const sin2D = Math.sin(angle2D);
		this.a = scaleX * cos2D;
		this.b = scaleX * sin2D;
		this.c = -scaleY * sin2D;
		this.d = scaleY * cos2D;
		this.tx = x;
		this.ty = y;
		this.tz = z;

		if (quaternionX === 0 && quaternionY === 0 && quaternionZ === 0 && quaternionW === 1) {
			this.renderXAxisX = this.a;
			this.renderXAxisY = this.b;
			this.renderXAxisZ = 0;
			this.renderYAxisX = this.c;
			this.renderYAxisY = this.d;
			this.renderYAxisZ = 0;
			this.renderZAxisX = 0;
			this.renderZAxisY = 0;
			this.renderZAxisZ = 1;
			this.revision++;
			return true;
		}

		// Rz(objectAngle) * quaternion3D * Rz(offsetAngle) * Scale.
		const xy = quaternionX * quaternionY;
		const wz = quaternionW * quaternionZ;
		const rotation00 = 1 - 2 * (quaternionY * quaternionY + quaternionZ * quaternionZ);
		const rotation01 = 2 * (xy - wz);
		const rotation10 = 2 * (xy + wz);
		const rotation11 = 1 - 2 * (quaternionX * quaternionX + quaternionZ * quaternionZ);

		const offsetCos = Math.cos(offsetAngle);
		const offsetSin = Math.sin(offsetAngle);

		const quaternionXAxisX = (rotation00 * offsetCos + rotation01 * offsetSin) * scaleX;
		const quaternionXAxisY = (rotation10 * offsetCos + rotation11 * offsetSin) * scaleX;
		const quaternionYAxisX = (-rotation00 * offsetSin + rotation01 * offsetCos) * scaleY;
		const quaternionYAxisY = (-rotation10 * offsetSin + rotation11 * offsetCos) * scaleY;

		const rotation02 = 2 * (quaternionX * quaternionZ + quaternionW * quaternionY);
		const rotation12 = 2 * (quaternionY * quaternionZ - quaternionW * quaternionX);
		const rotation20 = 2 * (quaternionX * quaternionZ - quaternionW * quaternionY);
		const rotation21 = 2 * (quaternionY * quaternionZ + quaternionW * quaternionX);

		const objectCos = Math.cos(objectAngle);
		const objectSin = Math.sin(objectAngle);

		this.renderXAxisX = objectCos * quaternionXAxisX - objectSin * quaternionXAxisY;
		this.renderXAxisY = objectSin * quaternionXAxisX + objectCos * quaternionXAxisY;
		this.renderXAxisZ = (rotation20 * offsetCos + rotation21 * offsetSin) * scaleX;
		this.renderYAxisX = objectCos * quaternionYAxisX - objectSin * quaternionYAxisY;
		this.renderYAxisY = objectSin * quaternionYAxisX + objectCos * quaternionYAxisY;
		this.renderYAxisZ = (-rotation20 * offsetSin + rotation21 * offsetCos) * scaleY;
		this.renderZAxisX = objectCos * rotation02 - objectSin * rotation12;
		this.renderZAxisY = objectSin * rotation02 + objectCos * rotation12;
		this.renderZAxisZ = 1 - 2 * (quaternionX * quaternionX + quaternionY * quaternionY);
		this.revision++;
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
		return bone.appliedPose.worldToParent(this.gameToSkeleton(x, y));
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
		const delta = a * d - b * c;
		const x = (d * cos - c * sin) / delta;
		const y = (a * sin - b * cos) / delta;
		return Math.atan2(y, x) * (180 / Math.PI);
	}

}
