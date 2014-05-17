/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;

namespace Spine {
	public class Bone {
		static public bool yDown;

		internal BoneData data;
		internal Bone parent;
		internal float x, y, rotation, scaleX, scaleY;
		internal float m00, m01, m10, m11;
		internal float worldX, worldY, worldRotation, worldScaleX, worldScaleY;

		public BoneData Data { get { return data; } }
		public Bone Parent { get { return parent; } }
		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }
		public float Rotation { get { return rotation; } set { rotation = value; } }
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }
		public float ScaleY { get { return scaleY; } set { scaleY = value; } }

		public float M00 { get { return m00; } }
		public float M01 { get { return m01; } }
		public float M10 { get { return m10; } }
		public float M11 { get { return m11; } }
		public float WorldX { get { return worldX; } }
		public float WorldY { get { return worldY; } }
		public float WorldRotation { get { return worldRotation; } }
		public float WorldScaleX { get { return worldScaleX; } }
		public float WorldScaleY { get { return worldScaleY; } }

		/// <param name="parent">May be null.</param>
		public Bone (BoneData data, Bone parent) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			this.data = data;
			this.parent = parent;
			SetToSetupPose();
		}

		/// <summary>Computes the world SRT using the parent bone and the local SRT.</summary>
		public void UpdateWorldTransform (bool flipX, bool flipY) {
			Bone parent = this.parent;
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
				worldRotation = data.inheritRotation ? parent.worldRotation + rotation : rotation;
			} else {
				worldX = flipX ? -x : x;
				worldY = flipY != yDown ? -y : y;
				worldScaleX = scaleX;
				worldScaleY = scaleY;
				worldRotation = rotation;
			}
			float radians = worldRotation * (float)Math.PI / 180;
			float cos = (float)Math.Cos(radians);
			float sin = (float)Math.Sin(radians);
			m00 = cos * worldScaleX;
			m10 = sin * worldScaleX;
			m01 = -sin * worldScaleY;
			m11 = cos * worldScaleY;
			if (flipX) {
				m00 = -m00;
				m01 = -m01;
			}
			if (flipY != yDown) {
				m10 = -m10;
				m11 = -m11;
			}
		}

		public void SetToSetupPose () {
			BoneData data = this.data;
			x = data.x;
			y = data.y;
			rotation = data.rotation;
			scaleX = data.scaleX;
			scaleY = data.scaleY;
		}

		override public String ToString () {
			return data.name;
		}
	}
}
