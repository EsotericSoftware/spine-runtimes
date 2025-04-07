/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace Spine.Unity.Examples {
	public class AttackSpineboy : MonoBehaviour {

		public SkeletonAnimation spineboy;
		public SkeletonAnimation attackerSpineboy;
		public SpineGauge gauge;
		public Text healthText;

		int currentHealth = 100;
		const int maxHealth = 100;

		public AnimationReferenceAsset shoot, hit, idle, death;

		public UnityEngine.Events.UnityEvent onAttack;

		void Update () {
			if (Input.GetKeyDown(KeyCode.Space)) {
				currentHealth -= 10;
				healthText.text = currentHealth + "/" + maxHealth;

				attackerSpineboy.AnimationState.SetAnimation(1, shoot, false);
				attackerSpineboy.AnimationState.AddEmptyAnimation(1, 0.5f, 2f);

				if (currentHealth > 0) {
					spineboy.AnimationState.SetAnimation(0, hit, false);
					spineboy.AnimationState.AddAnimation(0, idle, true, 0);
					gauge.fillPercent = (float)currentHealth / (float)maxHealth;
					onAttack.Invoke();
				} else {
					if (currentHealth >= 0) {
						gauge.fillPercent = 0;
						spineboy.AnimationState.SetAnimation(0, death, false).TrackEnd = float.PositiveInfinity;
					}
				}
			}
		}
	}

}
