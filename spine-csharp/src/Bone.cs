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
