using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SkeletonUtilityBone)), ExecuteInEditMode]
public class SkeletonUtilityGroundConstraint : SkeletonUtilityConstraint {

	public LayerMask groundMask;
	public bool use2D = true;
	public float castDistance = 5f;
	Vector3 rayOrigin;
	Vector3 rayDir = new Vector3(0,-1,0);
	float hitY;

	protected override void OnEnable ()
	{
		base.OnEnable ();
	}

	protected override void OnDisable ()
	{
		base.OnDisable ();
	}

	public override void DoUpdate()
	{
		rayOrigin = transform.position + new Vector3(0,castDistance,0);

		hitY = float.MinValue;
		if(use2D){
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin , rayDir, castDistance, groundMask);
			if(hit.collider != null){
				hitY = hit.point.y;
			}
		}
		else{
			RaycastHit hit;
			if(Physics.Raycast( rayOrigin, rayDir, out hit, castDistance, groundMask)){
				hitY = hit.point.y;
			}
		}

		Vector3 v = transform.position;
		v.y = Mathf.Clamp(v.y, hitY, float.MaxValue);
		transform.position = v;
		
		utilBone.bone.X = transform.localPosition.x;
		utilBone.bone.Y = transform.localPosition.y;

	}

	void OnDrawGizmos(){
		Vector3 hitEnd = rayOrigin + (rayDir * Mathf.Min(castDistance, rayOrigin.y - hitY));
		Vector3 clearEnd = rayOrigin + (rayDir * castDistance);
		Gizmos.DrawLine(rayOrigin, hitEnd);
		Gizmos.color = Color.red;
		Gizmos.DrawLine(hitEnd, clearEnd);
	}
}
