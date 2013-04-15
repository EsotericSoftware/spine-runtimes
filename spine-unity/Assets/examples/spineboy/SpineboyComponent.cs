using UnityEngine;
using System.Collections;

public class SpineboyComponent : MonoBehaviour {
	public void OnMouseDown () {
		SkeletonComponent skeletonComponent = GetComponent<SkeletonComponent>();
		skeletonComponent.animationName = "jump";
		skeletonComponent.loop = false;
	}

	public void Update () {
		SkeletonComponent skeletonComponent = GetComponent<SkeletonComponent>();
		if (!skeletonComponent.loop && skeletonComponent.state.Time >= skeletonComponent.state.Animation.Duration - 0.25) {
			skeletonComponent.animationName = "walk";
			skeletonComponent.loop = true;
		}
	}
}
