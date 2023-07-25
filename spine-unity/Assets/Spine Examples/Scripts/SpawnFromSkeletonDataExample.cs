/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class SpawnFromSkeletonDataExample : MonoBehaviour {

		public SkeletonDataAsset skeletonDataAsset;
		[Range(0, 100)]
		public int count = 20;

		[SpineAnimation(dataField: "skeletonDataAsset")]
		public string startingAnimation;

		IEnumerator Start () {
			if (skeletonDataAsset == null) yield break;
			skeletonDataAsset.GetSkeletonData(false); // Preload SkeletonDataAsset.
			yield return new WaitForSeconds(1f); // Pretend stuff is happening.

			Animation spineAnimation = skeletonDataAsset.GetSkeletonData(false).FindAnimation(startingAnimation);
			for (int i = 0; i < count; i++) {
				SkeletonAnimation sa = SkeletonAnimation.NewSkeletonAnimationGameObject(skeletonDataAsset); // Spawn a new SkeletonAnimation GameObject.
				DoExtraStuff(sa, spineAnimation); // optional stuff for fun.
				sa.gameObject.name = i.ToString();
				yield return new WaitForSeconds(1f / 8f);
			}

		}

		void DoExtraStuff (SkeletonAnimation sa, Spine.Animation spineAnimation) {
			sa.transform.localPosition = Random.insideUnitCircle * 6f;
			sa.transform.SetParent(this.transform, false);

			if (spineAnimation != null) {
				sa.Initialize(false);
				sa.AnimationState.SetAnimation(0, spineAnimation, true);
			}
		}

	}

}
