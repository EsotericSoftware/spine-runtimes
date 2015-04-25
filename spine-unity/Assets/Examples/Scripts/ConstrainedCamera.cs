

/*****************************************************************************
 * Constrained Camera created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using UnityEngine;
using System.Collections;

public class ConstrainedCamera : MonoBehaviour {
	public Transform target;
	public Vector3 offset;
	public Vector3 min;
	public Vector3 max;
	public float smoothing = 5f;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
		Vector3 goalPoint = target.position + offset;
		goalPoint.x = Mathf.Clamp(goalPoint.x, min.x, max.x);
		goalPoint.y = Mathf.Clamp(goalPoint.y, min.y, max.y);
		goalPoint.z = Mathf.Clamp(goalPoint.z, min.z, max.z);

		transform.position = Vector3.Lerp(transform.position, goalPoint, smoothing * Time.deltaTime);
	}
}