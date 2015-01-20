using UnityEngine;
using System.Collections;

public class FootSoldierExample : MonoBehaviour {
	[SpineAnimation("Idle")]
	public string idleAnimation;

	[SpineAnimation]
	public string attackAnimation;

	[SpineSlot]
	public string eyesSlot;

	[SpineAttachment(currentSkinOnly: true, slot: "eyesSlot")]
	public string eyesOpenAttachment;

	[SpineAttachment(currentSkinOnly: true, slot: "eyesSlot")]
	public string blinkAttachment;

	[Range(0, 0.2f)]
	public float blinkDuration = 0.05f;

	private SkeletonAnimation skeletonAnimation;

	void Awake() {
		skeletonAnimation = GetComponent<SkeletonAnimation>();
	}

	void Start() {
		skeletonAnimation.state.SetAnimation(0, idleAnimation, true);
		StartCoroutine("Blink");
	}

	void Update() {
		if (Input.GetKey(KeyCode.Space)) {
			if (skeletonAnimation.state.GetCurrent(0).Animation.Name != attackAnimation) {
				skeletonAnimation.state.SetAnimation(0, attackAnimation, false);
				skeletonAnimation.state.AddAnimation(0, idleAnimation, true, 0);
			}
		}
	}

	IEnumerator Blink() {
		while (true) {
			yield return new WaitForSeconds(Random.Range(0.25f, 3f));
			skeletonAnimation.skeleton.SetAttachment(eyesSlot, blinkAttachment);
			yield return new WaitForSeconds(blinkDuration);
			skeletonAnimation.skeleton.SetAttachment(eyesSlot, eyesOpenAttachment);
		}
	}
}
