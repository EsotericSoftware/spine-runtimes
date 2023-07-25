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
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Prototyping {
	/// <summary>
	/// Stores and serializes initial settings for a Spine Skeleton component. The settings only get applied on Start at runtime.</summary>
	public class SkeletonColorInitialize : MonoBehaviour {
		public Color skeletonColor = Color.white;
		public List<SlotSettings> slotSettings = new List<SlotSettings>();

		[System.Serializable]
		public class SlotSettings {
			[SpineSlot]
			public string slot = string.Empty;
			public Color color = Color.white;
		}

#if UNITY_EDITOR
		void OnValidate () {
			ISkeletonComponent skeletonComponent = GetComponent<ISkeletonComponent>();
			if (skeletonComponent != null) {
				skeletonComponent.Skeleton.SetSlotsToSetupPose();
				IAnimationStateComponent animationStateComponent = GetComponent<IAnimationStateComponent>();
				if (animationStateComponent != null && animationStateComponent.AnimationState != null) {
					animationStateComponent.AnimationState.Apply(skeletonComponent.Skeleton);
				}
			}
			ApplySettings();
		}
#endif

		void Start () {
			ApplySettings();
		}

		void ApplySettings () {
			ISkeletonComponent skeletonComponent = GetComponent<ISkeletonComponent>();
			if (skeletonComponent != null) {
				Skeleton skeleton = skeletonComponent.Skeleton;
				skeleton.SetColor(skeletonColor);

				foreach (SlotSettings s in slotSettings) {
					Slot slot = skeleton.FindSlot(s.slot);
					if (slot != null) slot.SetColor(s.color);
				}

			}
		}

	}

}
