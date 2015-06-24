/*****************************************************************************
 * SkeletonRagdoll2D added by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine;

[RequireComponent(typeof(SkeletonRenderer))]
public class SkeletonRagdoll2D : MonoBehaviour {
	private static Transform helper;

	[Header("Hierarchy")]
	[SpineBone]
	public string startingBoneName = "";
	[SpineBone]
	public List<string> stopBoneNames = new List<string>();

	[Header("Parameters")]
	public bool applyOnStart;
	[Tooltip("Set RootRigidbody IsKinematic to true when Apply is called.")]
	public bool pinStartBone;
	public float gravityScale = 1;
	[Tooltip("Warning!  You will have to re-enable and tune mix values manually if attempting to remove the ragdoll system.")]
	public bool disableIK = true;
	[Tooltip("If no BoundingBox Attachment is attached to a bone, this becomes the default Width or Radius of a Bone's ragdoll Rigidbody")]
	public float thickness = 0.125f;
	[Tooltip("Default rotational limit value.  Min is negative this value, Max is this value.")]
	public float rotationLimit = 20;
	public float rootMass = 20;
	[Tooltip("If your ragdoll seems unstable or uneffected by limits, try lowering this value.")]
	[Range(0.01f, 1f)]
	public float massFalloffFactor = 0.4f;
	[Tooltip("The layer assigned to all of the rigidbody parts.")]
	public int colliderLayer = 0;
	[Range(0, 1)]
	public float mix = 1;

	public Rigidbody2D RootRigidbody {
		get {
			return this.rootRigidbody;
		}
	}

	public Vector3 RootOffset {
		get {
			return this.rootOffset;
		}
	}

	public Vector3 EstimatedSkeletonPosition {
		get {
			return this.rootRigidbody.position - rootOffset;
		}
	}

	public bool IsActive {
		get {
			return this.isActive;
		}
	}

	private Rigidbody2D rootRigidbody;
	private ISkeletonAnimation skeletonAnim;
	private Skeleton skeleton;
	private Dictionary<Bone, Transform> boneTable = new Dictionary<Bone, Transform>();
	private Bone startingBone;
	private Transform ragdollRoot;
	private Vector2 rootOffset;
	private bool isActive;


	IEnumerator Start () {
		skeletonAnim = (ISkeletonAnimation)GetComponent<SkeletonRenderer>();
		if (helper == null) {
			helper = (Transform)(new GameObject("Helper")).transform;
			helper.hideFlags = HideFlags.HideInHierarchy;
		}

		if (applyOnStart) {
			yield return null;
			Apply();
		}
	}

	public Coroutine SmoothMix (float target, float duration) {
		return StartCoroutine(SmoothMixCoroutine(target, duration));
	}

	IEnumerator SmoothMixCoroutine (float target, float duration) {
		float startTime = Time.time;
		float startMix = mix;
		while (mix > 0) {
			mix = Mathf.SmoothStep(startMix, target, (Time.time - startTime) / duration);
			yield return null;
		}
	}

	public void SetSkeletonPosition (Vector3 worldPosition) {
		if (!isActive) {
			Debug.LogWarning("Can't call SetSkeletonPosition while Ragdoll is not active!");
			return;
		}

		Vector3 offset = worldPosition - transform.position;
		transform.position = worldPosition;
		foreach (Transform t in boneTable.Values) {
			t.position -= offset;
		}

		UpdateWorld(null);
		skeleton.UpdateWorldTransform();
	}

	public Rigidbody2D[] GetRigidbodyArray () {
		if (!isActive)
			return new Rigidbody2D[0];

		Rigidbody2D[] arr = new Rigidbody2D[boneTable.Count];
		int i = 0;
		foreach (Transform t in boneTable.Values) {
			arr[i] = t.GetComponent<Rigidbody2D>();
			i++;
		}

		return arr;
	}

	public Rigidbody2D GetRigidbody (string boneName) {
		var bone = skeleton.FindBone(boneName);
		if (bone == null)
			return null;

		if (boneTable.ContainsKey(bone))
			return boneTable[bone].GetComponent<Rigidbody2D>();

		return null;
	}

	public void Remove () {
		isActive = false;
		foreach (var t in boneTable.Values) {
			Destroy(t.gameObject);
		}
		Destroy(ragdollRoot.gameObject);

		boneTable.Clear();
		skeletonAnim.UpdateWorld -= UpdateWorld;
	}

	public void Apply () {
		isActive = true;
		skeleton = skeletonAnim.Skeleton;
		mix = 1;

		var ragdollRootBone = skeleton.FindBone(startingBoneName);
		startingBone = ragdollRootBone;
		RecursivelyCreateBoneProxies(ragdollRootBone);

		rootRigidbody = boneTable[ragdollRootBone].GetComponent<Rigidbody2D>();
		rootRigidbody.isKinematic = pinStartBone;
		rootRigidbody.mass = rootMass;

		List<Collider2D> boneColliders = new List<Collider2D>();

		foreach (var pair in boneTable) {
			var b = pair.Key;
			var t = pair.Value;
			Bone parentBone = null;
			Transform parentTransform = transform;

			boneColliders.Add(t.GetComponent<Collider2D>());

			if (b != startingBone) {
				parentBone = b.Parent;
				parentTransform = boneTable[parentBone];
			} else {
				ragdollRoot = new GameObject("RagdollRoot").transform;
				ragdollRoot.parent = transform;

				if (b == skeleton.RootBone) {
					ragdollRoot.localPosition = new Vector3(b.WorldX, b.WorldY, 0);
					ragdollRoot.localRotation = Quaternion.Euler(0, 0, GetCompensatedRotationIK(b));
					parentTransform = ragdollRoot;
				} else {
					ragdollRoot.localPosition = new Vector3(b.Parent.WorldX, b.Parent.WorldY, 0);
					ragdollRoot.localRotation = Quaternion.Euler(0, 0, GetCompensatedRotationIK(b.Parent));
					parentTransform = ragdollRoot;
				}

				rootOffset = t.position - transform.position;
			}

			var rbParent = parentTransform.GetComponent<Rigidbody2D>();

			if (rbParent != null) {
				var joint = t.gameObject.AddComponent<HingeJoint2D>();
				joint.connectedBody = rbParent;
				Vector3 localPos = parentTransform.InverseTransformPoint(t.position);
				localPos.x *= 1;
				joint.connectedAnchor = localPos;
				joint.GetComponent<Rigidbody2D>().mass = joint.connectedBody.mass * massFalloffFactor;
				JointAngleLimits2D limits = new JointAngleLimits2D();
				limits.min = -rotationLimit;
				limits.max = rotationLimit;
				joint.limits = limits;
				joint.useLimits = true;
			}
		}

		for (int x = 0; x < boneColliders.Count; x++) {
			for (int y = 0; y < boneColliders.Count; y++) {
				if (x == y) continue;
				Physics2D.IgnoreCollision(boneColliders[x], boneColliders[y]);
			}
		}

		var utilityBones = GetComponentsInChildren<SkeletonUtilityBone>();
		if (utilityBones.Length > 0) {
			List<string> destroyedUtilityBoneNames = new List<string>();
			foreach (var ub in utilityBones) {
				if (ub.mode == SkeletonUtilityBone.Mode.Override) {
					destroyedUtilityBoneNames.Add(ub.gameObject.name);
					Destroy(ub.gameObject);
				}
			}

			if (destroyedUtilityBoneNames.Count > 0) {
				string msg = "Destroyed Utility Bones: ";
				for (int i = 0; i < destroyedUtilityBoneNames.Count; i++) {
					msg += destroyedUtilityBoneNames[i];
					if (i != destroyedUtilityBoneNames.Count - 1) {
						msg += ",";
					}
				}
				Debug.LogWarning(msg);
			}
		}

		if (disableIK) {
			foreach (IkConstraint ik in skeleton.IkConstraints) {
				ik.Mix = 0;
			}
		}

		skeletonAnim.UpdateWorld += UpdateWorld;
	}

	void RecursivelyCreateBoneProxies (Bone b) {
		if (stopBoneNames.Contains(b.Data.Name))
			return;

		GameObject go = new GameObject(b.Data.Name);
		go.layer = colliderLayer;
		Transform t = go.transform;
		boneTable.Add(b, t);

		t.parent = transform;

		t.localPosition = new Vector3(b.WorldX, b.WorldY, 0);
		//TODO: deal with WorldFlipY
		t.localRotation = Quaternion.Euler(0, 0, b.WorldFlipX ? -b.WorldRotation : b.WorldRotation);
		t.localScale = new Vector3(b.WorldScaleX, b.WorldScaleY, 0);

		float length = b.Data.Length;

		//TODO proper ragdoll branching
		var colliders = AttachBoundingBoxRagdollColliders(b);

		if (length == 0) {
			//physics
			if (colliders.Count == 0) {
				var circle = go.AddComponent<CircleCollider2D>();
				circle.radius = thickness / 2f;
			}
		} else {
			//physics
			if (colliders.Count == 0) {
				var box = go.AddComponent<BoxCollider2D>();
				box.size = new Vector2(length, thickness);
#if UNITY_5_0
				box.offset = new Vector2((b.WorldFlipX ? -length : length) / 2, 0);
#else
				box.center = new Vector2((b.WorldFlipX ? -length : length) / 2, 0);
#endif
			}
		}

		var rb = go.AddComponent<Rigidbody2D>();
		rb.gravityScale = gravityScale;

		foreach (Bone child in b.Children) {
			RecursivelyCreateBoneProxies(child);
		}
	}

	List<Collider2D> AttachBoundingBoxRagdollColliders (Bone b) {
		List<Collider2D> colliders = new List<Collider2D>();
		Transform t = boneTable[b];
		GameObject go = t.gameObject;
		var skin = skeleton.Skin;
		if (skin == null)
			skin = skeleton.Data.DefaultSkin;

		bool flipX = b.WorldFlipX;
		bool flipY = b.WorldFlipY;

		List<Attachment> attachments = new List<Attachment>();
		foreach (Slot s in skeleton.Slots) {
			if (s.Bone == b) {
				skin.FindAttachmentsForSlot(skeleton.Slots.IndexOf(s), attachments);
				foreach (var a in attachments) {
					if (a is BoundingBoxAttachment) {
						if (!a.Name.ToLower().Contains("ragdoll"))
							continue;

						var collider = SkeletonUtility.AddBoundingBoxAsComponent((BoundingBoxAttachment)a, go, false);

						if (flipX || flipY) {
							Vector2[] points = collider.points;

							for (int i = 0; i < points.Length; i++) {
								if (flipX)
									points[i].x *= -1;

								if (flipY)
									points[i].y *= -1;
							}

							collider.points = points;
						}

						colliders.Add(collider);
					}
				}
			}
		}

		return colliders;
	}

	void UpdateWorld (SkeletonRenderer skeletonRenderer) {
		foreach (var pair in boneTable) {
			var b = pair.Key;
			var t = pair.Value;
			bool flip = false;
			bool flipX = false;  //TODO:  deal with negative scale instead of Flip Key
			bool flipY = false;  //TODO:  deal with negative scale instead of Flip Key
			Bone parentBone = null;
			Transform parentTransform = transform;

			if (b != startingBone) {
				parentBone = b.Parent;
				parentTransform = boneTable[parentBone];
				flipX = parentBone.WorldFlipX;
				flipY = parentBone.WorldFlipY;

			} else {
				parentBone = b.Parent;
				parentTransform = ragdollRoot;
				if (b.Parent != null) {
					flipX = b.worldFlipX;
					flipY = b.WorldFlipY;
				} else {
					flipX = b.Skeleton.FlipX;
					flipY = b.Skeleton.FlipY;
				}
			}

			flip = flipX ^ flipY;

			helper.position = parentTransform.position;
			helper.rotation = parentTransform.rotation;
			helper.localScale = new Vector3(flipX ? -parentTransform.localScale.x : parentTransform.localScale.x, flipY ? -parentTransform.localScale.y : parentTransform.localScale.y, 1);


			Vector3 pos = t.position;
			pos = helper.InverseTransformPoint(pos);
			b.X = Mathf.Lerp(b.X, pos.x, mix);
			b.Y = Mathf.Lerp(b.Y, pos.y, mix);

			Vector3 right = helper.InverseTransformDirection(t.right);

			float a = Mathf.Atan2(right.y, right.x) * Mathf.Rad2Deg;

			if (b.WorldFlipX ^ b.WorldFlipY) {
				a *= -1;
			}

			if (parentBone != null) {
				if ((b.WorldFlipX ^ b.WorldFlipY) != flip) {
					a -= GetCompensatedRotationIK(parentBone) * 2;
				}
			}

			b.Rotation = Mathf.Lerp(b.Rotation, a, mix);
			b.RotationIK = Mathf.Lerp(b.rotationIK, a, mix);
		}
	}

	float GetCompensatedRotationIK (Bone b) {
		Bone parent = b.Parent;
		float a = b.RotationIK;
		while (parent != null) {
			a += parent.RotationIK;
			parent = parent.parent;
		}

		return a;
	}

	void OnDrawGizmosSelected () {
		if (isActive) {
			Gizmos.DrawWireSphere(transform.position, thickness * 1.2f);
			Vector3 newTransformPos = rootRigidbody.position - rootOffset;
			Gizmos.DrawLine(transform.position, newTransformPos);
			Gizmos.DrawWireSphere(newTransformPos, thickness * 1.2f);
		}
	}

}
