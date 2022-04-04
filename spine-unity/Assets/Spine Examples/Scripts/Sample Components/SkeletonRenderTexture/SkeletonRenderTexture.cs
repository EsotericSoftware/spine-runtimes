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
	public class SkeletonRenderTexture : MonoBehaviour {
#if HAS_GET_SHARED_MATERIALS
		public Color color = Color.white;
		public Material quadMaterial;
		public Camera targetCamera;
		public int maxRenderTextureSize = 1024;
		protected SkeletonRenderer skeletonRenderer;
		protected MeshRenderer meshRenderer;
		protected MeshFilter meshFilter;
		public GameObject quad;
		protected MeshRenderer quadMeshRenderer;
		protected MeshFilter quadMeshFilter;
		protected Mesh quadMesh;
		public RenderTexture renderTexture;

		private CommandBuffer commandBuffer;
		private MaterialPropertyBlock propertyBlock;
		private readonly List<Material> materials = new List<Material>();

		protected Vector2Int requiredRenderTextureSize;
		protected Vector2Int allocatedRenderTextureSize;

		void Awake () {
			meshRenderer = this.GetComponent<MeshRenderer>();
			meshFilter = this.GetComponent<MeshFilter>();
			skeletonRenderer = this.GetComponent<SkeletonRenderer>();
			if (targetCamera == null)
				targetCamera = Camera.main;

			commandBuffer = new CommandBuffer();
			propertyBlock = new MaterialPropertyBlock();

			CreateQuadChild();
		}

		void OnDestroy () {
			if (renderTexture)
				RenderTexture.ReleaseTemporary(renderTexture);
		}

		void CreateQuadChild () {
			quad = new GameObject(this.name + " RenderTexture", typeof(MeshRenderer), typeof(MeshFilter));
			quad.transform.SetParent(this.transform.parent, false);
			quadMeshRenderer = quad.GetComponent<MeshRenderer>();
			quadMeshFilter = quad.GetComponent<MeshFilter>();

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
			Bounds boundsLocalSpace = meshFilter.sharedMesh.bounds;
			Vector3 meshMinWorldSpace = transform.TransformPoint(boundsLocalSpace.min);
			Vector3 meshMaxWorldSpace = transform.TransformPoint(boundsLocalSpace.max);
			Vector3 meshMinXMaxYWorldSpace = new Vector3(meshMinWorldSpace.x, meshMaxWorldSpace.y);
			Vector3 meshMaxXMinYWorldSpace = new Vector3(meshMaxWorldSpace.x, meshMinWorldSpace.y);

			// We need to get the min/max of all four corners, close position and rotation of the skeleton
			// in combination with perspective projection otherwise might lead to incorrect screen space min/max.
			Vector3 meshMinProjected = targetCamera.WorldToScreenPoint(meshMinWorldSpace);
			Vector3 meshMaxProjected = targetCamera.WorldToScreenPoint(meshMaxWorldSpace);
			Vector3 meshMinXMaxYProjected = targetCamera.WorldToScreenPoint(meshMinXMaxYWorldSpace);
			Vector3 meshMaxXMinYProjected = targetCamera.WorldToScreenPoint(meshMaxXMinYWorldSpace);
			// To handle 180 degree rotation and thus min/max inversion, we get min/max of all four corners
			Vector3 meshMinScreenSpace =
				Vector3.Min(meshMinProjected, Vector3.Min(meshMaxProjected,
				Vector3.Min(meshMinXMaxYProjected, meshMaxXMinYProjected)));
			Vector3 meshMaxScreenSpace =
				Vector3.Max(meshMinProjected, Vector3.Max(meshMaxProjected,
				Vector3.Max(meshMinXMaxYProjected, meshMaxXMinYProjected)));

			requiredRenderTextureSize = new Vector2Int(
				Mathf.Min(maxRenderTextureSize, Mathf.CeilToInt(Mathf.Abs(meshMaxScreenSpace.x - meshMinScreenSpace.x))),
				Mathf.Min(maxRenderTextureSize, Mathf.CeilToInt(Mathf.Abs(meshMaxScreenSpace.y - meshMinScreenSpace.y))));

			PrepareRenderTexture();
			PrepareCommandBuffer(meshMinWorldSpace, meshMaxWorldSpace);
		}

		protected void PrepareCommandBuffer (Vector3 meshMinWorldSpace, Vector3 meshMaxWorldSpace) {
			commandBuffer.Clear();
			commandBuffer.SetRenderTarget(renderTexture);
			commandBuffer.ClearRenderTarget(true, true, Color.clear);

			Matrix4x4 projectionMatrix = Matrix4x4.Ortho(
				meshMinWorldSpace.x, meshMaxWorldSpace.x,
				meshMinWorldSpace.y, meshMaxWorldSpace.y,
				float.MinValue, float.MaxValue);

			commandBuffer.SetProjectionMatrix(projectionMatrix);
			commandBuffer.SetViewport(new Rect(Vector2.zero, requiredRenderTextureSize));
		}

		protected void RenderToRenderTexture () {
			meshRenderer.GetPropertyBlock(propertyBlock);
			meshRenderer.GetSharedMaterials(materials);

			for (int i = 0; i < materials.Count; i++)
				commandBuffer.DrawMesh(meshFilter.sharedMesh, transform.localToWorldMatrix,
					materials[i], meshRenderer.subMeshStartIndex + i, -1, propertyBlock);
			Graphics.ExecuteCommandBuffer(commandBuffer);
		}

		protected void AssignAtQuad () {
			Vector2 min = meshFilter.sharedMesh.bounds.min;
			Vector2 max = meshFilter.sharedMesh.bounds.max;

			Vector3[] vertices = new Vector3[4] {
				new Vector3(min.x, min.y, 0),
				new Vector3(max.x, min.y, 0),
				new Vector3(min.x, max.y, 0),
				new Vector3(max.x, max.y, 0)
			};
			quadMesh.vertices = vertices;

			int[] indices = new int[6] { 0, 2, 1, 2, 3, 1 };
			quadMesh.triangles = indices;

			Vector3[] normals = new Vector3[4] {
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward
			};
			quadMesh.normals = normals;

			float maxU = (float)(requiredRenderTextureSize.x) / allocatedRenderTextureSize.x;
			float maxV = (float)(requiredRenderTextureSize.y) / allocatedRenderTextureSize.y;
			Vector2[] uv = new Vector2[4] {
				new Vector2(0, 0),
				new Vector2(maxU, 0),
				new Vector2(0, maxV),
				new Vector2(maxU, maxV)
			};
			quadMesh.uv = uv;
			quadMeshFilter.mesh = quadMesh;
			quadMeshRenderer.sharedMaterial.mainTexture = this.renderTexture;
			quadMeshRenderer.sharedMaterial.color = color;

			quadMeshRenderer.transform.position = this.transform.position;
			quadMeshRenderer.transform.rotation = this.transform.rotation;
			quadMeshRenderer.transform.localScale = this.transform.localScale;
		}

		protected void PrepareRenderTexture () {
			Vector2Int textureSize = new Vector2Int(
				Mathf.NextPowerOfTwo(requiredRenderTextureSize.x),
				Mathf.NextPowerOfTwo(requiredRenderTextureSize.y));

			if (textureSize != allocatedRenderTextureSize) {
				if (renderTexture)
					RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = RenderTexture.GetTemporary(textureSize.x, textureSize.y);
				allocatedRenderTextureSize = textureSize;
			}
		}
#endif
	}
}
