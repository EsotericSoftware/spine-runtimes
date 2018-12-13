using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class MaterialReplacementExample : MonoBehaviour {

		public Material originalMaterial;
		public Material replacementMaterial;
		public bool replacementEnabled = true;
		public SkeletonAnimation skeletonAnimation;

		[Space]
		public string phasePropertyName = "_FillPhase";
		[Range(0f, 1f)] public float phase = 1f;

		bool previousEnabled;
		MaterialPropertyBlock mpb;

		void Start () {
			previousEnabled = replacementEnabled;
			SetReplacementEnabled(replacementEnabled);
			mpb = new MaterialPropertyBlock();
		}

		void Update () {
			mpb.SetFloat(phasePropertyName, phase);
			GetComponent<MeshRenderer>().SetPropertyBlock(mpb);

			if (previousEnabled != replacementEnabled)
				SetReplacementEnabled(replacementEnabled);

			previousEnabled = replacementEnabled;

		}

		void SetReplacementEnabled (bool active) {
			if (replacementEnabled) {
				skeletonAnimation.CustomMaterialOverride[originalMaterial] = replacementMaterial;
			} else {
				skeletonAnimation.CustomMaterialOverride.Remove(originalMaterial);
			}
		}

	}
}

