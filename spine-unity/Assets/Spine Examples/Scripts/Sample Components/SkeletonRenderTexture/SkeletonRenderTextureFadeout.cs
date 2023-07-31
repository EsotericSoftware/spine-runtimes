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

#if UNITY_2019_3_OR_NEWER
#define HAS_FORCE_RENDER_OFF
#endif

#if UNITY_2017_2_OR_NEWER
#define HAS_VECTOR_INT
#endif

using UnityEngine;

namespace Spine.Unity.Examples {

	/// <summary>
	/// A simple fadeout component that uses a <see cref="SkeletonRenderTexture"/> for transparency fadeout.
	/// Attach a <see cref="SkeletonRenderTexture"/> and this component to a skeleton GameObject and disable both
	/// components initially and keep them disabled during normal gameplay. When you need to start fadeout,
	/// enable this component.
	/// At the end of the fadeout, the event delegate <c>OnFadeoutComplete</c> is called, to which you can bind e.g.
	/// a method that disables or destroys the entire GameObject.
	/// </summary>
	[RequireComponent(typeof(SkeletonRenderTextureBase))]
	public class SkeletonRenderTextureFadeout : MonoBehaviour {
		SkeletonRenderTextureBase skeletonRenderTexture;

		public float fadeoutSeconds = 2.0f;
		protected float fadeoutSecondsRemaining;

		public delegate void FadeoutCallback (SkeletonRenderTextureFadeout skeleton);
		public event FadeoutCallback OnFadeoutComplete;

		protected void Awake () {
			skeletonRenderTexture = this.GetComponent<SkeletonRenderTextureBase>();
		}

		protected void OnEnable () {
			fadeoutSecondsRemaining = fadeoutSeconds;
			skeletonRenderTexture.enabled = true;
		}

		protected void Update () {
			if (fadeoutSecondsRemaining == 0)
				return;

			fadeoutSecondsRemaining -= Time.deltaTime;
			if (fadeoutSecondsRemaining <= 0) {
				fadeoutSecondsRemaining = 0;
				if (OnFadeoutComplete != null)
					OnFadeoutComplete(this);
				return;
			}
			float fadeoutAlpha = fadeoutSecondsRemaining / fadeoutSeconds;
#if HAS_VECTOR_INT
			skeletonRenderTexture.color.a = fadeoutAlpha;
#else
			Debug.LogError("The SkeletonRenderTexture component requires Unity 2017.2 or newer.");
#endif
		}
	}
}
