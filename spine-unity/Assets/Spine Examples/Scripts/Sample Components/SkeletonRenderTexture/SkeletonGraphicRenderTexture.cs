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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Spine.Unity.Examples {

	/// <summary>
	/// When enabled, this component renders a skeleton to a RenderTexture and
	/// then draws this RenderTexture at a UI RawImage quad of the same size.
	/// This allows changing transparency at a single quad, which produces a more
	/// natural fadeout effect.
	/// Note: It is recommended to keep this component disabled as much as possible
	/// because of the additional rendering overhead. Only enable it when alpha blending is required.
	/// </summary>
	[RequireComponent(typeof(SkeletonGraphic))]
	public class SkeletonGraphicRenderTexture : SkeletonRenderTextureBase {
#if HAS_VECTOR2INT
		[System.Serializable]
		public struct TextureMaterialPair {
			public Texture texture;
			public Material material;

			public TextureMaterialPair (Texture texture, Material material) {
				this.texture = texture;
				this.material = material;
			}
		}

		public RectTransform customRenderRect;
		protected SkeletonGraphic skeletonGraphic;
		public List<TextureMaterialPair> meshRendererMaterialForTexture = new List<TextureMaterialPair>();
		protected CanvasRenderer quadCanvasRenderer;
		protected RawImage quadRawImage;
		protected readonly Vector3[] worldCorners = new Vector3[4];

		protected override void Awake () {
			base.Awake();
			skeletonGraphic = this.GetComponent<SkeletonGraphic>();
			if (targetCamera == null) {
				targetCamera = skeletonGraphic.canvas.worldCamera;
				if (targetCamera == null)
					targetCamera = Camera.main;
			}
			CreateQuadChild();
		}

		void CreateQuadChild () {
			quad = new GameObject(this.name + " RenderTexture", typeof(CanvasRenderer), typeof(RawImage));
			quad.transform.SetParent(this.transform.parent, false);
			quadCanvasRenderer = quad.GetComponent<CanvasRenderer>();
			quadRawImage = quad.GetComponent<RawImage>();

			quadMesh = new Mesh();
			quadMesh.MarkDynamic();
			quadMesh.name = "RenderTexture Quad";
			quadMesh.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
		}

		void Reset () {
			skeletonGraphic = this.GetComponent<SkeletonGraphic>();
			AtlasAssetBase[] atlasAssets = skeletonGraphic.SkeletonDataAsset.atlasAssets;
			for (int i = 0; i < atlasAssets.Length; ++i) {
				foreach (Material material in atlasAssets[i].Materials) {
					if (material.mainTexture != null) {
						meshRendererMaterialForTexture.Add(
							new TextureMaterialPair(material.mainTexture, material));
					}
				}
			}
		}

		void OnEnable () {
			skeletonGraphic.OnInstructionsPrepared += PrepareQuad;
			skeletonGraphic.AssignMeshOverrideSingleRenderer += RenderSingleMeshToRenderTexture;
			skeletonGraphic.AssignMeshOverrideMultipleRenderers += RenderMultipleMeshesToRenderTexture;
			skeletonGraphic.disableMeshAssignmentOnOverride = true;
			skeletonGraphic.OnMeshAndMaterialsUpdated += RenderOntoQuad;
			List<CanvasRenderer> canvasRenderers = skeletonGraphic.canvasRenderers;
			for (int i = 0; i < canvasRenderers.Count; ++i)
				canvasRenderers[i].cull = true;

			if (quadCanvasRenderer)
				quadCanvasRenderer.gameObject.SetActive(true);
		}

		void OnDisable () {
			skeletonGraphic.OnInstructionsPrepared -= PrepareQuad;
			skeletonGraphic.AssignMeshOverrideSingleRenderer -= RenderSingleMeshToRenderTexture;
			skeletonGraphic.AssignMeshOverrideMultipleRenderers -= RenderMultipleMeshesToRenderTexture;
			skeletonGraphic.disableMeshAssignmentOnOverride = false;
			skeletonGraphic.OnMeshAndMaterialsUpdated -= RenderOntoQuad;
			List<CanvasRenderer> canvasRenderers = skeletonGraphic.canvasRenderers;
			for (int i = 0; i < canvasRenderers.Count; ++i)
				canvasRenderers[i].cull = false;

			if (quadCanvasRenderer)
				quadCanvasRenderer.gameObject.SetActive(false);
			if (renderTexture)
				RenderTexture.ReleaseTemporary(renderTexture);
			allocatedRenderTextureSize = Vector2Int.zero;
		}

		void PrepareQuad (SkeletonRendererInstruction instruction) {
			PrepareForMesh();
			SetupQuad();
		}

		void RenderOntoQuad (SkeletonGraphic skeletonRenderer) {
			AssignAtQuad();
		}

		protected void PrepareForMesh () {
			// We need to get the min/max of all four corners, rotation of the skeleton
			// in combination with perspective projection otherwise might lead to incorrect
			// screen space min/max.
			RectTransform rectTransform = customRenderRect ? customRenderRect : skeletonGraphic.rectTransform;
			rectTransform.GetWorldCorners(worldCorners);

			RenderMode canvasRenderMode = skeletonGraphic.canvas.renderMode;
			Vector3 screenCorner0, screenCorner1, screenCorner2, screenCorner3;
			// note: world corners are ordered bottom left, top left, top right, bottom right.
			// This corresponds to 0, 3, 1, 2 in our desired order.
			if (canvasRenderMode == RenderMode.ScreenSpaceOverlay) {
				screenCorner0 = worldCorners[0];
				screenCorner1 = worldCorners[3];
				screenCorner2 = worldCorners[1];
				screenCorner3 = worldCorners[2];
			} else {
				screenCorner0 = targetCamera.WorldToScreenPoint(worldCorners[0]);
				screenCorner1 = targetCamera.WorldToScreenPoint(worldCorners[3]);
				screenCorner2 = targetCamera.WorldToScreenPoint(worldCorners[1]);
				screenCorner3 = targetCamera.WorldToScreenPoint(worldCorners[2]);
			}

			// To avoid perspective distortion when rotated, we project all vertices
			// onto a plane parallel to the view frustum near plane.
			// Avoids the requirement of 'noperspective' vertex attribute interpolation modifier in shaders.
			float averageScreenDepth = (screenCorner0.z + screenCorner1.z + screenCorner2.z + screenCorner3.z) / 4.0f;
			screenCorner0.z = screenCorner1.z = screenCorner2.z = screenCorner3.z = averageScreenDepth;

			if (canvasRenderMode == RenderMode.ScreenSpaceOverlay) {
				worldCornerNoDistortion0 = screenCorner0;
				worldCornerNoDistortion1 = screenCorner1;
				worldCornerNoDistortion2 = screenCorner2;
				worldCornerNoDistortion3 = screenCorner3;
			} else {
				worldCornerNoDistortion0 = targetCamera.ScreenToWorldPoint(screenCorner0);
				worldCornerNoDistortion1 = targetCamera.ScreenToWorldPoint(screenCorner1);
				worldCornerNoDistortion2 = targetCamera.ScreenToWorldPoint(screenCorner2);
				worldCornerNoDistortion3 = targetCamera.ScreenToWorldPoint(screenCorner3);
			}
			Vector3 screenSpaceMin, screenSpaceMax;
			PrepareTextureMapping(out screenSpaceMin, out screenSpaceMax,
				screenCorner0, screenCorner1, screenCorner2, screenCorner3);
			PrepareCommandBuffer(targetCamera, screenSpaceMin, screenSpaceMax);
		}

		protected Material MeshRendererMaterialForTexture (Texture texture) {
			return meshRendererMaterialForTexture.Find(x => x.texture == texture).material;
		}

		protected void RenderSingleMeshToRenderTexture (Mesh mesh, Material graphicMaterial, Texture texture) {
			Material meshRendererMaterial = MeshRendererMaterialForTexture(texture);
			commandBuffer.DrawMesh(mesh, transform.localToWorldMatrix, meshRendererMaterial, 0, -1);
			Graphics.ExecuteCommandBuffer(commandBuffer);
		}

		protected void RenderMultipleMeshesToRenderTexture (int meshCount,
			Mesh[] meshes, Material[] graphicMaterials, Texture[] textures) {

			for (int i = 0; i < meshCount; ++i) {
				Material meshRendererMaterial = MeshRendererMaterialForTexture(textures[i]);
				commandBuffer.DrawMesh(meshes[i], transform.localToWorldMatrix, meshRendererMaterial, 0, -1);
			}
			Graphics.ExecuteCommandBuffer(commandBuffer);
		}

		protected void SetupQuad () {
			quadRawImage.texture = this.renderTexture;
			quadRawImage.color = color;
			quadCanvasRenderer.SetColor(color);

			RectTransform srcRectTransform = skeletonGraphic.rectTransform;
			RectTransform dstRectTransform = quadRawImage.rectTransform;

			dstRectTransform.anchorMin = srcRectTransform.anchorMin;
			dstRectTransform.anchorMax = srcRectTransform.anchorMax;
			dstRectTransform.anchoredPosition = srcRectTransform.anchoredPosition;
			dstRectTransform.pivot = srcRectTransform.pivot;
			dstRectTransform.localScale = srcRectTransform.localScale;
			dstRectTransform.sizeDelta = srcRectTransform.sizeDelta;
			dstRectTransform.rotation = srcRectTransform.rotation;
		}

		protected void PrepareCommandBuffer (Camera targetCamera, Vector3 screenSpaceMin, Vector3 screenSpaceMax) {
			commandBuffer.Clear();
			commandBuffer.SetRenderTarget(renderTexture);
			commandBuffer.ClearRenderTarget(true, true, Color.clear);

			Rect canvasRect = skeletonGraphic.canvas.pixelRect;

			Matrix4x4 projectionMatrix = Matrix4x4.Ortho(
				canvasRect.x, canvasRect.x + canvasRect.width,
				canvasRect.y, canvasRect.y + canvasRect.height,
				float.MinValue, float.MaxValue);

			RenderMode canvasRenderMode = skeletonGraphic.canvas.renderMode;
			if (canvasRenderMode == RenderMode.ScreenSpaceOverlay) {
				commandBuffer.SetViewMatrix(Matrix4x4.identity);
				commandBuffer.SetProjectionMatrix(projectionMatrix);
			} else {
				commandBuffer.SetViewMatrix(targetCamera.worldToCameraMatrix);
				commandBuffer.SetProjectionMatrix(targetCamera.projectionMatrix);
			}

			Vector2 targetCameraViewportSize = targetCamera.pixelRect.size;
			Rect viewportRect = new Rect(-screenSpaceMin * downScaleFactor, targetCameraViewportSize * downScaleFactor);
			commandBuffer.SetViewport(viewportRect);
		}

		protected override void AssignMeshAtRenderer () {
			quadCanvasRenderer.SetMesh(quadMesh);
		}
#endif // HAS_VECTOR2INT
	}
}
