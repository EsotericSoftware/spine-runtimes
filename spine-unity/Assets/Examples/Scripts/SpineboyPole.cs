using UnityEngine;
using System.Collections;
using Spine.Unity;

using Spine.Unity.Modules;

public class SpineboyPole : MonoBehaviour {
	public SkeletonAnimation skeletonAnimation;
	public SkeletonRenderSeparator separator;

	[Space]
	[SpineAnimation]
	public string run;
	[SpineAnimation]
	public string pole;
	public float startX;
	public float endX;

	const float Speed = 18f;
	const float RunTimeScale = 1.5f;

	IEnumerator Start () {
		var state = skeletonAnimation.state;

		while (true) {
			// Run phase
			SetXPosition(startX);
			separator.enabled = false; // Disable Separator during run.
			state.SetAnimation(0, run, true);
			state.TimeScale = RunTimeScale;

			while (transform.localPosition.x < endX) {
				transform.Translate(Vector3.right * Speed * Time.deltaTime);
				yield return null;
			}

			// Hit phase
			SetXPosition(endX);
			separator.enabled = true; // Enable Separator when hit
			var poleTrack = state.SetAnimation(0, pole, false);
			yield return new WaitForSpineAnimationComplete(poleTrack);
			yield return new WaitForSeconds(1f);
		}
	}

	void SetXPosition (float x) {
		var tp = transform.localPosition;
		tp.x = x;
		transform.localPosition = tp;
	}
}

