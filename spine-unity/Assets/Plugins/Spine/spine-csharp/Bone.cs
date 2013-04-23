/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

using System;

namespace Spine {
	public class Bone {
		static public bool yDown;

		public BoneData Data { get; private set; }
		public Bone Parent { get; private set; }
		public float X { get; set; }
		public float Y { get; set; }
		public float Rotation { get; set; }
		public float ScaleX { get; set; }
		public float ScaleY { get; set; }

		public float M00 { get; private set; }
		public float M01 { get; private set; }
		public float M10 { get; private set; }
		public float M11 { get; private set; }
		public float WorldX { get; private set; }
		public float WorldY { get; private set; }
		public float WorldRotation { get; private set; }
		public float WorldScaleX { get; private set; }
		public float WorldScaleY { get; private set; }

		/** @param parent May be null. */
		public Bone (BoneData data, Bone parent) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			Data = data;
			Parent = parent;
			SetToBindPose();
		}

		/** Computes the world SRT using the parent bone and the local SRT. */
		public void UpdateWorldTransform (bool flipX, bool flipY) {
			Bone parent = Parent;
			if (parent != null) {
				WorldX = X * parent.M00 + Y * parent.M01 + parent.WorldX;
				WorldY = X * parent.M10 + Y * parent.M11 + parent.WorldY;
				WorldScaleX = parent.WorldScaleX * ScaleX;
				WorldScaleY = parent.WorldScaleY * ScaleY;
				WorldRotation = parent.WorldRotation + Rotation;
			} else {
				WorldX = X;
				WorldY = Y;
				WorldScaleX = ScaleX;
				WorldScaleY = ScaleY;
				WorldRotation = Rotation;
			}
			float radians = WorldRotation * (float)Math.PI / 180;
			float cos = (float)Math.Cos(radians);
			float sin = (float)Math.Sin(radians);
			M00 = cos * WorldScaleX;
			M10 = sin * WorldScaleX;
			M01 = -sin * WorldScaleY;
			M11 = cos * WorldScaleY;
			if (flipX) {
				M00 = -M00;
				M01 = -M01;
			}
			if (flipY) {
				M10 = -M10;
				M11 = -M11;
			}
			if (yDown) {
				M10 = -M10;
				M11 = -M11;
			}
		}

		public void SetToBindPose () {
			BoneData data = Data;
			X = data.X;
			Y = data.Y;
			Rotation = data.Rotation;
			ScaleX = data.ScaleX;
			ScaleY = data.ScaleY;
		}

		override public String ToString () {
			return Data.Name;
		}
	}
}
