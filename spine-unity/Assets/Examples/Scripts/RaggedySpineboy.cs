using UnityEngine;
using System.Collections;
using Spine.Unity;

public class RaggedySpineboy : MonoBehaviour {

	public LayerMask groundMask;
	public float restoreDuration = 0.5f;
	public Vector2 launchVelocity = new Vector2(50,100);

	Spine.Unity.Modules.SkeletonRagdoll2D ragdoll;
	Collider2D naturalCollider;

	void Start () {
		
		ragdoll = GetComponent<Spine.Unity.Modules.SkeletonRagdoll2D>();
		naturalCollider = GetComponent<Collider2D>();
	}

	void AddRigidbody () {
		var rb = gameObject.AddComponent<Rigidbody2D>();
		#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
        rb.freezeRotation = true;
		#else
		rb.fixedAngle = true;
		#endif
		naturalCollider.enabled = true;
	}

	void RemoveRigidbody () {
		Destroy(GetComponent<Rigidbody2D>());
		naturalCollider.enabled = false;
	}

	void Update () {
		
	}

	void OnMouseUp () {
		if (naturalCollider.enabled) {
			Launch();
		}
	}

	void Launch () {
		RemoveRigidbody();
		ragdoll.Apply();
		ragdoll.RootRigidbody.velocity = new Vector2(Random.Range(-launchVelocity.x, launchVelocity.x), launchVelocity.y);
		StartCoroutine(WaitUntilStopped());
	}

	IEnumerator Restore () {
		Vector3 estimatedPos = ragdoll.EstimatedSkeletonPosition;
		Vector3 rbPosition = ragdoll.RootRigidbody.position;

		Vector3 skeletonPoint = estimatedPos;
		RaycastHit2D hit = Physics2D.Raycast((Vector2)rbPosition, (Vector2)(estimatedPos - rbPosition), Vector3.Distance(estimatedPos, rbPosition), groundMask);
		if (hit.collider != null)
			skeletonPoint = hit.point;
		

		ragdoll.RootRigidbody.isKinematic = true;
		ragdoll.SetSkeletonPosition(skeletonPoint);

		yield return ragdoll.SmoothMix(0, restoreDuration);
		ragdoll.Remove();

		AddRigidbody();
	}

	IEnumerator WaitUntilStopped () {
		yield return new WaitForSeconds(0.5f);

		float t = 0;
		while (t < 0.5f) {
			if (ragdoll.RootRigidbody.velocity.magnitude > 0.09f)
				t = 0;
			else
				t += Time.deltaTime;

			yield return null;
		}

		StartCoroutine(Restore());
	}
}
