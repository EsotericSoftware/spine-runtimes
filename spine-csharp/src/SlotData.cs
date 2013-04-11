using System;

namespace Spine {
	public class SlotData {
		public String Name { get; private set; }
		public BoneData BoneData { get; private set; }
		public float R { get; set; }
		public float G { get; set; }
		public float B { get; set; }
		public float A { get; set; }
		/** @param attachmentName May be null. */
		public String AttachmentName { get; set; }

		public SlotData (String name, BoneData boneData) {
			if (name == null) throw new ArgumentNullException("name cannot be null.");
			if (boneData == null) throw new ArgumentNullException("boneData cannot be null.");
			Name = name;
			BoneData = boneData;
			R = 1;
			G = 1;
			B = 1;
			A = 1;
		}

		override public String ToString () {
			return Name;
		}
	}
}
