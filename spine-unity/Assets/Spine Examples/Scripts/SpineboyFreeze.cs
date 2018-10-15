using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class SpineboyFreeze : MonoBehaviour {

		public SkeletonAnimation skeletonAnimation;
		public AnimationReferenceAsset freeze;
		public AnimationReferenceAsset idle;

		public Color freezeColor;
		public Color freezeBlackColor;
		public ParticleSystem particles;
		public float freezePoint = 0.5f;

		public string colorProperty = "_Color";
		public string blackTintProperty = "_Black";

		MaterialPropertyBlock block;
		MeshRenderer meshRenderer;

		IEnumerator Start () {
			block = new MaterialPropertyBlock();
			meshRenderer = GetComponent<MeshRenderer>();

			particles.Stop();
			particles.Clear();
			var main = particles.main;
			main.loop = false;

			var state = skeletonAnimation.AnimationState;
			while (true) {

				yield return new WaitForSeconds(1f);

				// Play freeze animation
				state.SetAnimation(0, freeze, false);
				yield return new WaitForSeconds(freezePoint);

				// Freeze effects
				particles.Play();
				block.SetColor(colorProperty, freezeColor);
				block.SetColor(blackTintProperty, freezeBlackColor);
				meshRenderer.SetPropertyBlock(block);


				yield return new WaitForSeconds(2f);

				// Return to Idle
				state.SetAnimation(0, idle, true);
				block.SetColor(colorProperty, Color.white);
				block.SetColor(blackTintProperty, Color.black);
				meshRenderer.SetPropertyBlock(block);


				yield return null;
			}
		}
	}

}
