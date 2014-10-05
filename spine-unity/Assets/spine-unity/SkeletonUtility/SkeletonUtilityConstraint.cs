using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SkeletonUtilityBone)), ExecuteInEditMode]

public abstract class SkeletonUtilityConstraint : MonoBehaviour {

	protected SkeletonUtilityBone utilBone;
	protected SkeletonUtility skeletonUtility;

	protected virtual void OnEnable () {
		utilBone = GetComponent<SkeletonUtilityBone>();
		skeletonUtility = SkeletonUtility.GetInParent<SkeletonUtility>(transform);
		skeletonUtility.RegisterConstraint(this);
	}

	protected virtual void OnDisable () {
		skeletonUtility.UnregisterConstraint(this);
	}

	public abstract void DoUpdate ();
}
