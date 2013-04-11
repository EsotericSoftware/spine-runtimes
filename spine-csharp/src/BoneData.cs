using System;

namespace Spine {
	public class BoneData {
		/** May be null. */
		public BoneData Parent { get; private set; }
		public String Name { get; private set; }
		public float Length { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public float Rotation { get; set; }
		public float ScaleX { get; set; }
		public float ScaleY { get; set; }

		/** @param parent May be null. */
		public BoneData (String name, BoneData parent) {
			if (name == null) throw new ArgumentNullException("name cannot be null.");
			Name = name;
			Parent = parent;
			ScaleX = 1;
			ScaleY = 1;
		}

		override public String ToString () {
			return Name;
		}
	}
}
