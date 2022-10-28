/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
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
#define HAS_VECTOR2INT
#endif

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Spine.Unity.Examples {

	public abstract class SkeletonRenderTextureBase : MonoBehaviour {
#if HAS_VECTOR2INT
		public Color color = Color.white;
		public int maxRenderTextureSize = 1024;
		public GameObject quad;
		protected Mesh quadMesh;
		public RenderTexture renderTexture;
		public Camera targetCamera;

		protected CommandBuffer commandBuffer;
		protected Vector2Int screenSize;
		protected Vector2Int usedRenderTextureSize;
		protected Vector2Int allocatedRenderTextureSize;
		protected Vector2 downScaleFactor = Vector2.one;

		protected Vector3 worldCornerNoDistortion0;
		protected Vector3 worldCornerNoDistortion1;
		protected Vector3 worldCornerNoDistortion2;
		protected Vector3 worldCornerNoDistortion3;
		protected Vector2 uvCorner0;
		protected Vector2 uvCorner1;
		protected Vector2 uvCorner2;
		protected Vector2 uvCorner3;

		protected virtual void Awake () {
			commandBuffer = new CommandBuffer();
		}

		void OnDestroy () {
			if (renderTexture)
				RenderTexture.ReleaseTemporary(renderTexture);
		}

		protected void PrepareTextureMapping (out Vector3 screenSpaceMin, out Vector3 screenSpaceMax,
			Vector3 screenCorner0, Vector3 screenCorner1, Vector3 screenCorner2, Vector3 screenCorner3) {

			screenSpaceMin =
				Vector3.Min(screenCorner0, Vector3.Min(screenCorner1,
				Vector3.Min(screenCorner2, screenCorner3)));
			screenSpaceMax =
				Vector3.Max(screenCorner0, Vector3.Max(screenCorner1,
				Vector3.Max(screenCorner2, screenCorner3)));
			// ensure we are on whole pixel borders
			screenSpaceMin.x = Mathf.Floor(screenSpaceMin.x);
			screenSpaceMin.y = Mathf.Floor(screenSpaceMin.y);
			screenSpaceMax.x = Mathf.Ceil(screenSpaceMax.x);
			screenSpaceMax.y = Mathf.Ceil(screenSpaceMax.y);

			// inverse-map screenCornerN to screenSpaceMin/screenSpaceMax area to get UV coordinates
			uvCorner0 = MathUtilities.InverseLerp(screenSpaceMin, screenSpaceMax, screenCorner0);
			uvCorner1 = MathUtilities.InverseLerp(screenSpaceMin, screenSpaceMax, screenCorner1);
			uvCorner2 = MathUtilities.InverseLerp(screenSpaceMin, screenSpaceMax, screenCorner2);
			uvCorner3 = MathUtilities.InverseLerp(screenSpaceMin, screenSpaceMax, screenCorner3);

			screenSize = new Vector2Int(Math.Abs((int)screenSpaceMax.x - (int)screenSpaceMin.x),
										Math.Abs((int)screenSpaceMax.y - (int)screenSpaceMin.y));
			usedRenderTextureSize = new Vector2Int(
				Math.Min(maxRenderTextureSize, screenSize.x),
				Math.Min(maxRenderTextureSize, screenSize.y));
			downScaleFactor = new Vector2(
				(float)usedRenderTextureSize.x / (float)screenSize.x,
				(float)usedRenderTextureSize.y / (float)screenSize.y);

			PrepareRenderTexture();
		}

		protected void PrepareRenderTexture () {
			Vector2Int textureSize = new Vector2Int(
				Mathf.NextPowerOfTwo(usedRenderTextureSize.x),
				Mathf.NextPowerOfTwo(usedRenderTextureSize.y));

			if (textureSize != allocatedRenderTextureSize) {
				if (renderTexture)
					RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = RenderTexture.GetTemporary(textureSize.x, textureSize.y);
				renderTexture.filterMode = FilterMode.Point;
				allocatedRenderTextureSize = textureSize;
			}
		}

		protected void AssignAtQuad () {
			Transform quadTransform = quad.transform;
			quadTransform.position = this.transform.position;
			quadTransform.rotation = this.transform.rotation;
			quadTransform.localScale = this.transform.localScale;

			Vector3 v0 = quadTransform.InverseTransformPoint(worldCornerNoDistortion0);
			Vector3 v1 = quadTransform.InverseTransformPoint(worldCornerNoDistortion1);
			Vector3 v2 = quadTransform.InverseTransformPoint(worldCornerNoDistortion2);
			Vector3 v3 = quadTransform.InverseTransformPoint(worldCornerNoDistortion3);
			Vector3[] vertices = new Vector3[4] { v0, v1, v2, v3 };

			quadMesh.vertices = vertices;

			int[] indices = new int[6] { 0, 1, 2, 2, 1, 3 };
			quadMesh.triangles = indices;

			Vector3[] normals = new Vector3[4] {
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward
			};
			quadMesh.normals = normals;

			float maxU = (float)usedRenderTextureSize.x / (float)allocatedRenderTextureSize.x;
			float maxV = (float)usedRenderTextureSize.y / (float)allocatedRenderTextureSize.y;
			if (downScaleFactor.x < 1 || downScaleFactor.y < 1) {
				maxU = downScaleFactor.x * (float)screenSize.x / (float)allocatedRenderTextureSize.x;
				maxV = downScaleFactor.y * (float)screenSize.y / (float)allocatedRenderTextureSize.y;
			}
			Vector2[] uv = new Vector2[4] {
				new Vector2(uvCorner0.x * maxU, uvCorner0.y * maxV),
				new Vector2(uvCorner1.x * maxU, uvCorner1.y * maxV),
				new Vector2(uvCorner2.x * maxU, uvCorner2.y * maxV),
				new Vector2(uvCorner3.x * maxU, uvCorner3.y * maxV),
			};
			quadMesh.uv = uv;
			AssignMeshAtRenderer();
		}

		protected abstract void AssignMeshAtRenderer ();
#endif // HAS_VECTOR2INT
	}
}
