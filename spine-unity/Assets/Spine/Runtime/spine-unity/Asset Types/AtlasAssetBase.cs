using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {
	public abstract class AtlasAssetBase : ScriptableObject {
		public abstract Material PrimaryMaterial { get; }
		public abstract IEnumerable<Material> Materials { get; }
		public abstract int MaterialCount { get; }

		public abstract bool IsLoaded { get; }
		public abstract void Clear ();
		public abstract Atlas GetAtlas ();
	}
}
