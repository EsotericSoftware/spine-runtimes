/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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
