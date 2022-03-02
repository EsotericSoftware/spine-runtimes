/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2022, Esoteric Software LLC
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

#if UNITY_2017_2_OR_NEWER
#define HAS_VECTOR_INT
#endif

using UnityEngine;
using UnityEngine.Events;

namespace Spine.Unity.Examples {
	public class RenderTextureFadeoutExample : MonoBehaviour {

		public SkeletonRenderTexture skeletonRenderTexture;
		public SkeletonRenderer normalSkeletonRenderer;

		public void Update () {
			float fadeoutAlpha = 1.0f - ((0.5f * Time.time) % 1.0f);

			// changing transpacency at a MeshRenderer does not yield the desired effect
			// due to overlapping attachment meshes.
			normalSkeletonRenderer.Skeleton.SetColor(new Color(1, 1, 1, fadeoutAlpha));

#if HAS_VECTOR_INT
			// Thus we render the whole skeleton to a RenderTexture first using the 
			// SkeletonRenderTexture component.
			// Changing transparency at a single quad with a RenderTexture works as desired.
			skeletonRenderTexture.color.a = fadeoutAlpha;
#else
			Debug.LogError("The SkeletonRenderTexture component requires Unity 2017.2 or newer.");
#endif
		}
	}
}
