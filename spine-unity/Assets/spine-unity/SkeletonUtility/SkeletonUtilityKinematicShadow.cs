using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkeletonUtilityKinematicShadow : MonoBehaviour {
	public bool hideShadow = true;
	public Transform parent;
	Dictionary<Transform, Transform> shadowTable;
	GameObject shadowRoot;

	void Start () {
		shadowRoot = (GameObject)Instantiate(gameObject);
		if (hideShadow)
			shadowRoot.hideFlags = HideFlags.HideInHierarchy;

		if(parent == null)
			shadowRoot.transform.parent = transform.root;
		else
			shadowRoot.transform.parent = parent;

		shadowTable = new Dictionary<Transform, Transform>();

		Destroy(shadowRoot.GetComponent<SkeletonUtilityKinematicShadow>());

		shadowRoot.transform.position = transform.position;
		shadowRoot.transform.rotation = transform.rotation;

		Vector3 scaleRef = transform.TransformPoint(Vector3.right);
		float scale = Vector3.Distance(transform.position, scaleRef);
		shadowRoot.transform.localScale = Vector3.one;

		var shadowJoints = shadowRoot.GetComponentsInChildren<Joint>();
		foreach (Joint j in shadowJoints) {
			j.connectedAnchor *= scale;
		}

		var joints = GetComponentsInChildren<Joint>();
		foreach (var j in joints)
			Destroy(j);

		var rbs = GetComponentsInChildren<Rigidbody>();
		foreach (var rb in rbs)
			Destroy(rb);

		var colliders = GetComponentsInChildren<Collider>();
		foreach (var c in colliders)
			Destroy(c);


		//match by bone name
		var shadowBones = shadowRoot.GetComponentsInChildren<SkeletonUtilityBone>();
		var bones = GetComponentsInChildren<SkeletonUtilityBone>();

		//build bone lookup
		foreach (var b in bones) {
			if (b.gameObject == gameObject)
				continue;

			foreach (var sb in shadowBones) {
				if (sb.GetComponent<Rigidbody>() == null)
					continue;

				if (sb.boneName == b.boneName) {
					shadowTable.Add(sb.transform, b.transform);
					break;
				}
			}
		}

		foreach (var b in shadowBones)
			Destroy(b);
	}

	void FixedUpdate () {
		shadowRoot.GetComponent<Rigidbody>().MovePosition(transform.position);
		shadowRoot.GetComponent<Rigidbody>().MoveRotation(transform.rotation);

		foreach (var pair in shadowTable) {
			pair.Value.localPosition = pair.Key.localPosition;
			pair.Value.localRotation = pair.Key.localRotation;
		}
	}
}