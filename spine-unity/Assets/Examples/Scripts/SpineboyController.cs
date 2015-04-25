

/*****************************************************************************
 * SpineboyController created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SkeletonAnimation), typeof(Rigidbody2D))]
public class SpineboyController : MonoBehaviour {

	SkeletonAnimation skeletonAnimation;
	public string idleAnimation = "idle";
	public string walkAnimation = "walk";
	public string runAnimation = "run";
	public string hitAnimation = "hit";
	public string deathAnimation = "death";
	public float walkVelocity = 1;
	public float runVelocity = 3;
	public int hp = 10;
	string currentAnimation = "";
	bool hit = false;
	bool dead = false;

	void Start () {
		skeletonAnimation = GetComponent<SkeletonAnimation>();
	}

	void Update () {
		if (!dead) {
			float x = Input.GetAxis("Horizontal");
			float absX = Mathf.Abs(x);

			if (!hit) {
				if (x > 0)
					skeletonAnimation.skeleton.FlipX = false;
				else if (x < 0)
						skeletonAnimation.skeleton.FlipX = true;

				if (absX > 0.7f) {
					SetAnimation(runAnimation, true);
					GetComponent<Rigidbody2D>().velocity = new Vector2(runVelocity * Mathf.Sign(x), GetComponent<Rigidbody2D>().velocity.y);
				} else if (absX > 0) {
						SetAnimation(walkAnimation, true);
						GetComponent<Rigidbody2D>().velocity = new Vector2(walkVelocity * Mathf.Sign(x), GetComponent<Rigidbody2D>().velocity.y);
					} else {
						SetAnimation(idleAnimation, true);
						GetComponent<Rigidbody2D>().velocity = new Vector2(0, GetComponent<Rigidbody2D>().velocity.y);
					}
			} else {
				if (skeletonAnimation.state.GetCurrent(0).Animation.Name != hitAnimation)
					hit = false;
			}
		}
	}

	void SetAnimation (string anim, bool loop) {
		if (currentAnimation != anim) {
			skeletonAnimation.state.SetAnimation(0, anim, loop);
			currentAnimation = anim;
		}
	}

	void OnMouseUp () {

		if (hp > 0) {
			hp--;

			if (hp == 0) {
				SetAnimation(deathAnimation, false);
				dead = true;
			} else {
				skeletonAnimation.state.SetAnimation(0, hitAnimation, false);
				skeletonAnimation.state.AddAnimation(0, currentAnimation, true, 0);
				GetComponent<Rigidbody2D>().velocity = new Vector2(0, GetComponent<Rigidbody2D>().velocity.y);
				hit = true;
			}

		}
	}
}