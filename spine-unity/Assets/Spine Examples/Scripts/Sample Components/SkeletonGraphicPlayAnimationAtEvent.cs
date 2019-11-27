using UnityEngine;
using Spine.Unity;

public class SkeletonGraphicPlayAnimationAtEvent : MonoBehaviour {

	public SkeletonGraphic skeletonGraphic;
	public int trackIndex = 0;
	public float playbackSpeed = 1.0f;

	public void PlayAnimationLooping (string animation) {
		var entry = skeletonGraphic.AnimationState.SetAnimation(trackIndex, animation, true);
		entry.TimeScale = playbackSpeed;
	}

	public void PlayAnimationOnce (string animation) {
		var entry = skeletonGraphic.AnimationState.SetAnimation(trackIndex, animation, false);
		entry.TimeScale = playbackSpeed;
	}

	public void ClearTrack () {
		skeletonGraphic.AnimationState.ClearTrack(trackIndex);
	}
}
