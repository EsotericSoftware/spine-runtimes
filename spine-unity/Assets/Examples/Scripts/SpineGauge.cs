using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(SkeletonRenderer))]
public class SpineGauge : MonoBehaviour{

	[Range(0,1)]
	public float fill = 0;

	[SpineAnimation]
	public string fillAnimationName;
	Spine.Animation fillAnimation;

	SkeletonRenderer skeletonRenderer;

	void Start () {
		skeletonRenderer = GetComponent<SkeletonRenderer>();
	}

	void Update () {
		
		var skeleton = skeletonRenderer.skeleton;

		if (skeleton == null)
			return;

		if (fillAnimation == null) {
			fillAnimation = skeleton.Data.FindAnimation(fillAnimationName);
			if (fillAnimation == null)
				return;
		}

		fillAnimation.Apply(skeleton, 0, fill, false, null);
		skeleton.Update(Time.deltaTime);
		skeleton.UpdateWorldTransform();
	}
}
