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

using Spine;
using Spine.Unity;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class Goblins : MonoBehaviour {
		SkeletonAnimation skeletonAnimation;
		Bone headBone;
		bool girlSkin;

		[Range(-360, 360)]
		public float extraRotation;

		public void Start () {
			skeletonAnimation = GetComponent<SkeletonAnimation>();
			headBone = skeletonAnimation.Skeleton.FindBone("head");
			skeletonAnimation.UpdateLocal += UpdateLocal;
		}

		// This is called after the animation is applied to the skeleton and can be used to adjust the bones dynamically.
		public void UpdateLocal (ISkeletonAnimation skeletonRenderer) {
			headBone.Rotation += extraRotation;
		}

		public void OnMouseDown () {
			skeletonAnimation.Skeleton.SetSkin(girlSkin ? "goblin" : "goblingirl");
			skeletonAnimation.Skeleton.SetSlotsToSetupPose();

			girlSkin = !girlSkin;

			if (girlSkin) {
				skeletonAnimation.Skeleton.SetAttachment("right-hand-item", null);
				skeletonAnimation.Skeleton.SetAttachment("left-hand-item", "spear");
			} else
				skeletonAnimation.Skeleton.SetAttachment("left-hand-item", "dagger");
		}
	}
}
