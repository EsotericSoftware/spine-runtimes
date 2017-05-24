using UnityEngine;
using Spine.Unity;

namespace Spine.Unity.Examples {

	/// <summary>
	/// Add this component to a Spine GameObject to apply a specific slot's Colors as MaterialProperties.
	/// This allows you to apply the two color tint to the whole skeleton and not require the overhead of an extra vertex stream on the mesh.
	/// </summary>
	public class SlotTintBlackFollower : MonoBehaviour {
		#region Inspector
		/// <summary>
		/// Serialized name of the slot loaded at runtime. Change the slot field instead of this if you want to change the followed slot at runtime.</summary>
		[SpineSlot]
		[SerializeField]
		protected string slotName;

		[SerializeField]
		protected string colorPropertyName = "_Color";
		[SerializeField]
		protected string blackPropertyName = "_Black";
		#endregion

		public Slot slot;
		MeshRenderer mr;
		MaterialPropertyBlock mb;
		int colorPropertyId, blackPropertyId;

		void Start () {
			Initialize(false);
		}

		public void Initialize (bool overwrite) {
			if (overwrite || mb == null) {
				mb = new MaterialPropertyBlock();
				mr = GetComponent<MeshRenderer>();
				slot = GetComponent<ISkeletonComponent>().Skeleton.FindSlot(slotName);

				colorPropertyId = Shader.PropertyToID(colorPropertyName);
				blackPropertyId = Shader.PropertyToID(blackPropertyName);
			}
		}

		public void Update () {
			Slot s = slot;
			if (s == null) return;

			mb.SetColor(colorPropertyId, new Color(s.r, s.g, s.b, s.a));
			mb.SetColor(blackPropertyId, new Color(s.r2, s.g2, s.b2, 1f));

			mr.SetPropertyBlock(mb);
		}

		void OnDisable () {
			mb.Clear();
			mr.SetPropertyBlock(mb);
		}
	}
}
