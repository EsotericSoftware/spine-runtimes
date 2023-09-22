/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#define AUTOINIT_SPINEREFERENCE

using UnityEngine;

namespace Spine.Unity {
	[CreateAssetMenu(menuName = "Spine/Animation Reference Asset", order = 100)]
	public class AnimationReferenceAsset : ScriptableObject, IHasSkeletonDataAsset {
		const bool QuietSkeletonData = true;

		[SerializeField] protected SkeletonDataAsset skeletonDataAsset;
		[SerializeField, SpineAnimation] protected string animationName;
		private Animation animation;

		public SkeletonDataAsset SkeletonDataAsset { get { return skeletonDataAsset; } }

		public Animation Animation {
			get {
#if AUTOINIT_SPINEREFERENCE
				if (animation == null)
					Initialize();
#endif
				return animation;
			}
		}

		/// <summary>Clears the cached animation corresponding to a loaded SkeletonData object.
		/// Use this to force a reload for the next time Animation is called.</summary>
		public void Clear () {
			animation = null;
		}

		public void Initialize () {
			if (skeletonDataAsset == null) return;
			SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(AnimationReferenceAsset.QuietSkeletonData);
			this.animation = skeletonData != null ? skeletonData.FindAnimation(animationName) : null;
			if (this.animation == null) Debug.LogWarningFormat("Animation '{0}' not found in SkeletonData : {1}.", animationName, skeletonDataAsset.name);
		}

		public static implicit operator Animation (AnimationReferenceAsset asset) {
			return asset.Animation;
		}
	}
}
