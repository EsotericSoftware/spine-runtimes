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

#if UNITY_2019_3_OR_NEWER
#define HAS_FORCE_RENDER_OFF
#endif

#if UNITY_2018_2_OR_NEWER
#define HAS_GET_SHARED_MATERIALS
#endif

using UnityEngine;
using UnityEngine.Rendering;

namespace Spine.Unity.Examples {

	public abstract class SkeletonRenderTextureBase : MonoBehaviour {
#if HAS_GET_SHARED_MATERIALS
		public Color color = Color.white;
		public int maxRenderTextureSize = 1024;
		public GameObject quad;
		protected Mesh quadMesh;
		public RenderTexture renderTexture;

		protected CommandBuffer commandBuffer;
		protected Vector2Int requiredRenderTextureSize;
		protected Vector2Int allocatedRenderTextureSize;

		protected virtual void Awake () {
			commandBuffer = new CommandBuffer();
		}

		void OnDestroy () {
			if (renderTexture)
				RenderTexture.ReleaseTemporary(renderTexture);
		}

		protected void PrepareRenderTexture () {
			Vector2Int textureSize = new Vector2Int(
				Mathf.NextPowerOfTwo(requiredRenderTextureSize.x),
				Mathf.NextPowerOfTwo(requiredRenderTextureSize.y));

			if (textureSize != allocatedRenderTextureSize) {
				if (renderTexture)
					RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = RenderTexture.GetTemporary(textureSize.x, textureSize.y);
				renderTexture.filterMode = FilterMode.Point;
				allocatedRenderTextureSize = textureSize;
			}
		}
#endif
	}
}
