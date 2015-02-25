using UnityEngine;
using System.Collections;

public class DynamicSpineBone : MonoBehaviour {

	public Transform speedReference;

	[SpineBone]
	public string boneName;

	[Range(-90, 90)]
	public float minRotation = -45;
	[Range(-90, 90)]
	public float maxRotation = 45;

	[Range(-2000, 2000)]
	public float rotationFactor = 300;

	[Range(5, 30)]
	public float returnSpeed = 10;

	[Range(100, 1000)]
	public float boneSpeed = 300;

	public float returnThreshhold = 0.01f;

	public bool useAcceleration;


	SkeletonAnimation skeletonAnimation;
	float goalRotation;
	Spine.Bone bone;
	Vector3 velocity;
	Vector3 acceleration;
	Vector3 lastPosition;

	void Start() {
		if (speedReference == null)
			speedReference = transform;

		skeletonAnimation = GetComponent<SkeletonAnimation>();
		bone = SpineBone.GetBone(boneName, skeletonAnimation);
		skeletonAnimation.UpdateLocal += UpdateLocal;
		lastPosition = speedReference.position;
	}

	void FixedUpdate() {
		acceleration = (speedReference.position - lastPosition) - velocity;
		velocity = speedReference.position - lastPosition;
		lastPosition = speedReference.position;
	}

	void UpdateLocal(SkeletonRenderer renderer) {
		Vector3 vec = useAcceleration ? acceleration : velocity;

		if (Mathf.Abs(vec.x) < returnThreshhold)
			goalRotation = Mathf.Lerp(goalRotation, 0, returnSpeed * Time.deltaTime);
		else
			goalRotation += vec.x * rotationFactor * Time.deltaTime * (bone.WorldFlipX ? -1 : 1);

		goalRotation = Mathf.Clamp(goalRotation, minRotation, maxRotation);

		bone.Rotation = Mathf.Lerp(bone.Rotation, bone.Rotation + goalRotation, boneSpeed * Time.deltaTime);

	}
}
