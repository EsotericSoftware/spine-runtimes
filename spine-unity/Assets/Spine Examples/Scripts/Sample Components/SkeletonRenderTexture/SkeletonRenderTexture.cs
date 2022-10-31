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

#if UNITY_2019_3_OR_NEWER
#define HAS_FORCE_RENDER_OFF
#endif

#if UNITY_2018_2_OR_NEWER
#define HAS_GET_SHARED_MATERIALS
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Spine.Unity.Examples {

	/// <summary>
	/// When enabled, this component renders a skeleton to a RenderTexture and
	/// then draws this RenderTexture at a quad of the same size.
	/// This allows changing transparency at a single quad, which produces a more
	/// natural fadeout effect.
	/// Note: It is recommended to keep this component disabled as much as possible
	/// because of the additional rendering overhead. Only enable it when alpha blending is required.
	/// </summary>
	[RequireComponent(typeof(SkeletonRenderer))]
	public class SkeletonRenderTexture : SkeletonRenderTextureBase {
#if HAS_GET_SHARED_MATERIALS
		public Material quadMaterial;
		protected SkeletonRenderer skeletonRenderer;
		protected MeshRenderer meshRenderer;
		protected MeshFilter meshFilter;
		protected MeshRenderer quadMeshRenderer;
		protected MeshFilter quadMeshFilter;

		private MaterialPropertyBlock propertyBlock;
		private readonly List<Material> materials = new List<Material>();
		protected override void Awake () {
			base.Awake();
			meshRenderer = this.GetComponent<MeshRenderer>();
			meshFilter = this.GetComponent<MeshFilter>();
			skeletonRenderer = this.GetComponent<SkeletonRenderer>();
			if (targetCamera == null)
				targetCamera = Camera.main;

			propertyBlock = new MaterialPropertyBlock();
			CreateQuadChild();
		}

		void CreateQuadChild () {
			quad = new GameObject(this.name + " RenderTexture", typeof(MeshRenderer), typeof(MeshFilter));
			quad.transform.SetParent(this.transform.parent, false);
			quadMeshRenderer = quad.GetComponent<MeshRenderer>();
			quadMeshFilter = quad.GetComponent<MeshFilter>();

			quadMeshRenderer.sortingOrder = meshRenderer.sortingOrder;
			quadMeshRenderer.sortingLayerID = meshRenderer.sortingLayerID;

			quadMesh = new Mesh();
			quadMesh.MarkDynamic();
			quadMesh.name = "RenderTexture Quad";
			quadMesh.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;

			if (quadMaterial != null)
				quadMeshRenderer.material = new Material(quadMaterial);
			else
				quadMeshRenderer.material = new Material(Shader.Find("Spine/RenderQuad"));
		}

		void OnEnable () {
			skeletonRenderer.OnMeshAndMaterialsUpdated += RenderOntoQuad;
#if HAS_FORCE_RENDER_OFF
			meshRenderer.forceRenderingOff = true;
#else
			Debug.LogError("This component requires Unity 2019.3 or newer for meshRenderer.forceRenderingOff. " +
				"Otherwise you will see the mesh rendered twice.");
#endif
			if (quadMeshRenderer)
				quadMeshRenderer.gameObject.SetActive(true);
		}

		void OnDisable () {
			skeletonRenderer.OnMeshAndMaterialsUpdated -= RenderOntoQuad;
#if HAS_FORCE_RENDER_OFF
			meshRenderer.forceRenderingOff = false;
#endif
			if (quadMeshRenderer)
				quadMeshRenderer.gameObject.SetActive(false);
			if (renderTexture)
				RenderTexture.ReleaseTemporary(renderTexture);
			allocatedRenderTextureSize = Vector2Int.zero;
		}

		void RenderOntoQuad (SkeletonRenderer skeletonRenderer) {
			PrepareForMesh();
			RenderToRenderTexture();
			AssignAtQuad();
		}

		protected void PrepareForMesh () {
			// We need to get the min/max of all four corners, rotation of the skeleton
			// in combination with perspective projection otherwise might lead to incorrect
			// screen space min/max.
			Bounds boundsLocalSpace = meshFilter.sharedMesh.bounds;
			Vector3 localCorner0 = boundsLocalSpace.min;
			Vector3 localCorner3 = boundsLocalSpace.max;
			Vector3 localCorner1 = new Vector3(localCorner0.x, localCorner3.y, localCorner0.z);
			Vector3 localCorner2 = new Vector3(localCorner3.x, localCorner0.y, localCorner3.z);

			Vector3 worldCorner0 = transform.TransformPoint(localCorner0);
			Vector3 worldCorner1 = transform.TransformPoint(localCorner1);
			Vector3 worldCorner2 = transform.TransformPoint(localCorner2);
			Vector3 worldCorner3 = transform.TransformPoint(localCorner3);

			Vector3 screenCorner0 = targetCamera.WorldToScreenPoint(worldCorner0);
			Vector3 screenCorner1 = targetCamera.WorldToScreenPoint(worldCorner1);
			Vector3 screenCorner2 = targetCamera.WorldToScreenPoint(worldCorner2);
			Vector3 screenCorner3 = targetCamera.WorldToScreenPoint(worldCorner3);

			// To avoid perspective distortion when rotated, we project all vertices
			// onto a plane parallel to the view frustum near plane.
			// Avoids the requirement of 'noperspective' vertex attribute interpolation modifier in shaders.
			float averageScreenDepth = (screenCorner0.z + screenCorner1.z + screenCorner2.z + screenCorner3.z) / 4.0f;
			screenCorner0.z = screenCorner1.z = screenCorner2.z = screenCorner3.z = averageScreenDepth;
			worldCornerNoDistortion0 = targetCamera.ScreenToWorldPoint(screenCorner0);
			worldCornerNoDistortion1 = targetCamera.ScreenToWorldPoint(screenCorner1);
			worldCornerNoDistortion2 = targetCamera.ScreenToWorldPoint(screenCorner2);
			worldCornerNoDistortion3 = targetCamera.ScreenToWorldPoint(screenCorner3);

			Vector3 screenSpaceMin, screenSpaceMax;
			PrepareTextureMapping(out screenSpaceMin, out screenSpaceMax,
				screenCorner0, screenCorner1, screenCorner2, screenCorner3);
			PrepareCommandBuffer(targetCamera, screenSpaceMin, screenSpaceMax);
		}

		protected void PrepareCommandBuffer (Camera targetCamera, Vector3 screenSpaceMin, Vector3 screenSpaceMax) {
			commandBuffer.Clear();
			commandBuffer.SetRenderTarget(renderTexture);
			commandBuffer.ClearRenderTarget(true, true, Color.clear);

			commandBuffer.SetProjectionMatrix(targetCamera.projectionMatrix);
			commandBuffer.SetViewMatrix(targetCamera.worldToCameraMatrix);
			Vector2 targetCameraViewportSize = targetCamera.pixelRect.size;
			Rect viewportRect = new Rect(-screenSpaceMin * downScaleFactor, targetCameraViewportSize * downScaleFactor);
			commandBuffer.SetViewport(viewportRect);
		}

		protected void RenderToRenderTexture () {
			meshRenderer.GetPropertyBlock(propertyBlock);
			meshRenderer.GetSharedMaterials(materials);

			for (int i = 0; i < materials.Count; i++)
				commandBuffer.DrawMesh(meshFilter.sharedMesh, transform.localToWorldMatrix,
					materials[i], meshRenderer.subMeshStartIndex + i, -1, propertyBlock);
			Graphics.ExecuteCommandBuffer(commandBuffer);
		}

		protected override void AssignMeshAtRenderer () {
			quadMeshFilter.mesh = quadMesh;
			quadMeshRenderer.sharedMaterial.mainTexture = this.renderTexture;
			quadMeshRenderer.sharedMaterial.color = color;
		}
#endif
	}
}
