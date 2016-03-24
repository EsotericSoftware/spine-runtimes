using UnityEngine;
using System.Collections;
using Spine.Unity;

public class SpineBlinkPlayer : MonoBehaviour {
	const int BlinkTrack = 1;

	[SpineAnimation]
	public string blinkAnimation;
	public float minimumDelay = 0.15f;
	public float maximumDelay = 3f;

	IEnumerator Start () {
		var skeletonAnimation = GetComponent<SkeletonAnimation>(); if (skeletonAnimation == null) yield break;
		while (true) {
			skeletonAnimation.state.SetAnimation(SpineBlinkPlayer.BlinkTrack, blinkAnimation, false);
			yield return new WaitForSeconds(Random.Range(minimumDelay, maximumDelay));
		}
	}

}
