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
	public class SpineboyFreeze : MonoBehaviour {

		public SkeletonAnimation skeletonAnimation;
		public AnimationReferenceAsset freeze;
		public AnimationReferenceAsset idle;

		public Color freezeColor;
		public Color freezeBlackColor;
		public ParticleSystem particles;
		public float freezePoint = 0.5f;

		public string colorProperty = "_Color";
		public string blackTintProperty = "_Black";

		MaterialPropertyBlock block;
		MeshRenderer meshRenderer;

		IEnumerator Start () {
			block = new MaterialPropertyBlock();
			meshRenderer = GetComponent<MeshRenderer>();

			particles.Stop();
			particles.Clear();
			ParticleSystem.MainModule main = particles.main;
			main.loop = false;

			AnimationState state = skeletonAnimation.AnimationState;
			while (true) {

				yield return new WaitForSeconds(1f);

				// Play freeze animation
				state.SetAnimation(0, freeze, false);
				yield return new WaitForSeconds(freezePoint);

				// Freeze effects
				particles.Play();
				block.SetColor(colorProperty, freezeColor);
				block.SetColor(blackTintProperty, freezeBlackColor);
				meshRenderer.SetPropertyBlock(block);


				yield return new WaitForSeconds(2f);

				// Return to Idle
				state.SetAnimation(0, idle, true);
				block.SetColor(colorProperty, Color.white);
				block.SetColor(blackTintProperty, Color.black);
				meshRenderer.SetPropertyBlock(block);


				yield return null;
			}
		}
	}

}
