/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Collections.Generic;

namespace Spine {
	public class Bone{
		static public bool yDown;

		internal BoneData data;
		internal Skeleton skeleton;
		internal Bone parent;
		internal List<Bone> children = new List<Bone>();
		internal float x, y, rotation, rotationIK, scaleX, scaleY;
		internal bool flipX, flipY;
		internal float m00, m01, m10, m11;
		internal float worldX, worldY, worldRotation, worldScaleX, worldScaleY;
		internal bool worldFlipX, worldFlipY;

		public BoneData Data { get { return data; } }
		public Skeleton Skeleton { get { return skeleton; } }
		public Bone Parent { get { return parent; } }
		public List<Bone> Children { get { return children; } }
		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }
		/// <summary>The forward kinetics rotation.</summary>
		public float Rotation { get { return rotation; } set { rotation = value; } }
		/// <summary>The inverse kinetics rotation, as calculated by any IK constraints.</summary>
		public float RotationIK { get { return rotationIK; } set { rotationIK = value; } }
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }
		public float ScaleY { get { return scaleY; } set { scaleY = value; } }
		public bool FlipX { get { return flipX; } set { flipX = value; } }
		public bool FlipY { get { return flipY; } set { flipY = value; } }

		public float M00 { get { return m00; } }
		public float M01 { get { return m01; } }
		public float M10 { get { return m10; } }
		public float M11 { get { return m11; } }
		public float WorldX { get { return worldX; } }
		public float WorldY { get { return worldY; } }
		public float WorldRotation { get { return worldRotation; } }
		public float WorldScaleX { get { return worldScaleX; } }
		public float WorldScaleY { get { return worldScaleY; } }
		public bool WorldFlipX { get { return worldFlipX; } set { worldFlipX = value; } }
		public bool WorldFlipY { get { return worldFlipY; } set { worldFlipY = value; } }

		/// <param name="parent">May be null.</param>
		public Bone (BoneData data, Skeleton skeleton, Bone parent) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");
			this.data = data;
			this.skeleton = skeleton;
			this.parent = parent;
			SetToSetupPose();
		}

		/// <summary>Computes the world SRT using the parent bone and the local SRT.</summary>
		public void UpdateWorldTransform () {
			Bone parent = this.parent;
			float x = this.x, y = this.y;
			if (parent != null) {
				worldX = x * parent.m00 + y * parent.m01 + parent.worldX;
				worldY = x * parent.m10 + y * parent.m11 + parent.worldY;
				if (data.inheritScale) {
					worldScaleX = parent.worldScaleX * scaleX;
					worldScaleY = parent.worldScaleY * scaleY;
				} else {
					worldScaleX = scaleX;
					worldScaleY = scaleY;
				}
				worldRotation = data.inheritRotation ? parent.worldRotation + rotationIK : rotationIK;
				worldFlipX = parent.worldFlipX != flipX;
				worldFlipY = parent.worldFlipY != flipY;
			} else {
				Skeleton skeleton = this.skeleton;
				bool skeletonFlipX = skeleton.flipX, skeletonFlipY = skeleton.flipY;
				worldX = skeletonFlipX ? -x : x;
				worldY = skeletonFlipY != yDown ? -y : y;
				worldScaleX = scaleX;
				worldScaleY = scaleY;
				worldRotation = rotationIK;
				worldFlipX = skeletonFlipX != flipX;
				worldFlipY = skeletonFlipY != flipY;
			}
			float radians = worldRotation * (float)Math.PI / 180;
			float cos = (float)Math.Cos(radians);
			float sin = (float)Math.Sin(radians);
			if (worldFlipX) {
				m00 = -cos * worldScaleX;
				m01 = sin * worldScaleY;
			} else {
				m00 = cos * worldScaleX;
				m01 = -sin * worldScaleY;
			}
			if (worldFlipY != yDown) {
				m10 = -sin * worldScaleX;
				m11 = -cos * worldScaleY;
			} else {
				m10 = sin * worldScaleX;
				m11 = cos * worldScaleY;
			}
		}

		public void SetToSetupPose () {
			BoneData data = this.data;
			x = data.x;
			y = data.y;
			rotation = data.rotation;
			rotationIK = rotation;
			scaleX = data.scaleX;
			scaleY = data.scaleY;
			flipX = data.flipX;
			flipY = data.flipY;
		}

		public void worldToLocal (float worldX, float worldY, out float localX, out float localY) {
			float dx = worldX - this.worldX, dy = worldY - this.worldY;
			float m00 = this.m00, m10 = this.m10, m01 = this.m01, m11 = this.m11;
			if (worldFlipX != (worldFlipY != yDown)) {
				m00 = -m00;
				m11 = -m11;
			}
			float invDet = 1 / (m00 * m11 - m01 * m10);
			localX = (dx * m00 * invDet - dy * m01 * invDet);
			localY = (dy * m11 * invDet - dx * m10 * invDet);
		}

		public void localToWorld (float localX, float localY, out float worldX, out float worldY) {
			worldX = localX * m00 + localY * m01 + this.worldX;
			worldY = localX * m10 + localY * m11 + this.worldY;
		}

		override public String ToString () {
			return data.name;
		}
	}
}
