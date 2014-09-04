using UnityEngine;
using System.Collections;

public class Raptor : MonoBehaviour {
	public void Start () {
		// Get the SkeletonAnimation component for the GameObject this script is attached to.
		SkeletonAnimation skeletonAnimation = GetComponent<SkeletonAnimation>();
		// Set an animation on track 1 that does nothing to be played first.
		skeletonAnimation.state.SetAnimation(1, "empty", false);
		// Queue gun grab to be played on track 1 two seconds later.
		skeletonAnimation.state.AddAnimation(1, "gungrab", false, 2);
	}
}
