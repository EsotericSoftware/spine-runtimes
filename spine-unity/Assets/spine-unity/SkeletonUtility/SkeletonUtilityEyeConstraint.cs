using UnityEngine;
using System.Collections;

public class SkeletonUtilityEyeConstraint : SkeletonUtilityConstraint {

	public Transform[] eyes;
	public float radius = 0.5f;
	public Transform target;
	public Vector3 targetPosition;
	public float speed = 10;
	Vector3[] origins;
	Vector3 centerPoint;
	
	protected override void OnEnable () {
		if (!Application.isPlaying)
			return;

		base.OnEnable();

		Bounds centerBounds = new Bounds(eyes[0].localPosition, Vector3.zero);
		origins = new Vector3[eyes.Length];
		for (int i = 0; i < eyes.Length; i++) {
			origins[i] = eyes[i].localPosition;
			centerBounds.Encapsulate(origins[i]);
		}

		centerPoint = centerBounds.center;
	}
	
	protected override void OnDisable () {
		if (!Application.isPlaying)
			return;

		base.OnDisable();
	}
	
	public override void DoUpdate () {

		if (target != null)
			targetPosition = target.position;

		Vector3 goal = targetPosition;

		Vector3 center = transform.TransformPoint(centerPoint);
		Vector3 dir = goal - center;

		if (dir.magnitude > 1) 
			dir.Normalize();

		for (int i = 0; i < eyes.Length; i++) {
			center = transform.TransformPoint(origins[i]);
			eyes[i].position = Vector3.MoveTowards(eyes[i].position, center + (dir * radius), speed * Time.deltaTime);
		}
		
	}	
}