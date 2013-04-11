using System;

namespace Spine {
	abstract public class Attachment {
		public String Name { get; private set; }

		public Attachment (String name) {
			if (name == null) throw new ArgumentNullException("name cannot be null.");
			Name = name;
		}

		override public String ToString () {
			return Name;
		}
	}
}
