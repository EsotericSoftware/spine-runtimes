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

#if UNITY_2017_2_OR_NEWER
#define HAS_VECTOR_INT
#endif

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Spine.Unity.Examples {
	public class RenderTextureFadeoutExample : MonoBehaviour {

		public SkeletonRenderTextureFadeout renderTextureFadeout;
		public SkeletonRenderTextureFadeout renderTextureFadeoutCanvas;
		public SkeletonRenderer normalSkeletonRenderer;

		float fadeoutSeconds = 2.0f;
		float fadeoutSecondsRemaining;

		IEnumerator Start () {
			while (true) {
				StartFadeoutBad();
				StartFadeoutGood(renderTextureFadeout);
				StartFadeoutGood(renderTextureFadeoutCanvas);
				yield return new WaitForSeconds(fadeoutSeconds + 1.0f);
			}
		}
		void Update () {
			UpdateBadFadeOutAlpha();
		}

		void UpdateBadFadeOutAlpha () {
			if (fadeoutSecondsRemaining == 0)
				return;

			fadeoutSecondsRemaining -= Time.deltaTime;
			if (fadeoutSecondsRemaining <= 0) {
				fadeoutSecondsRemaining = 0;
				return;
			}
			float fadeoutAlpha = fadeoutSecondsRemaining / fadeoutSeconds;

			// changing transparency at a MeshRenderer does not yield the desired effect
			// due to overlapping attachment meshes.
			normalSkeletonRenderer.Skeleton.SetColor(new Color(1, 1, 1, fadeoutAlpha));
		}

		void StartFadeoutBad () {
			fadeoutSecondsRemaining = fadeoutSeconds;
		}

		void StartFadeoutGood (SkeletonRenderTextureFadeout fadeoutComponent) {
			fadeoutComponent.gameObject.SetActive(true);
			// enabling the SkeletonRenderTextureFadeout component starts the fadeout.
			fadeoutComponent.enabled = true;
			fadeoutComponent.OnFadeoutComplete -= DisableGameObject;
			fadeoutComponent.OnFadeoutComplete += DisableGameObject;
		}

		void DisableGameObject (SkeletonRenderTextureFadeout target) {
			target.gameObject.SetActive(false);
		}
	}
}
