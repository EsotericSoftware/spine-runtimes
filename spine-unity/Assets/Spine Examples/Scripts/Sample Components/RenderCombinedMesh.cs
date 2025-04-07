/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

#if UNITY_2019_3_OR_NEWER
#define SET_VERTICES_HAS_LENGTH_PARAMETER
#endif

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spine.Unity.Examples {

#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	public class RenderCombinedMesh : MonoBehaviour {
		public SkeletonRenderer skeletonRenderer;
		public SkeletonRenderSeparator renderSeparator;
		public MeshRenderer[] referenceRenderers;

		bool updateViaSkeletonCallback = false;
		MeshFilter[] referenceMeshFilters;
		MeshRenderer ownRenderer;
		MeshFilter ownMeshFilter;

		protected DoubleBuffered<Mesh> doubleBufferedMesh;
		protected ExposedList<Vector3> positionBuffer;
		protected ExposedList<Color32> colorBuffer;
		protected ExposedList<Vector2> uvBuffer;
		protected ExposedList<int> indexBuffer;

#if UNITY_EDITOR
		private void Reset () {
			if (skeletonRenderer == null)
				skeletonRenderer = this.GetComponentInParent<SkeletonRenderer>();
			GatherRenderers();

			Awake();
			if (referenceRenderers.Length > 0)
				ownRenderer.sharedMaterial = referenceRenderers[0].sharedMaterial;

			LateUpdate();
		}
#endif
		protected void GatherRenderers () {
			referenceRenderers = this.GetComponentsInChildren<MeshRenderer>();
			if (referenceRenderers.Length == 0 ||
				(referenceRenderers.Length == 1 && referenceRenderers[0].gameObject == this.gameObject)) {
				Transform parent = this.transform.parent;
				if (parent)
					referenceRenderers = parent.GetComponentsInChildren<MeshRenderer>();
			}
			referenceRenderers = referenceRenderers.Where(
				(val, idx) => val.gameObject != this.gameObject && val.enabled).ToArray();
		}

		void Awake () {
			if (skeletonRenderer == null)
				skeletonRenderer = this.GetComponentInParent<SkeletonRenderer>();
			if (referenceRenderers == null || referenceRenderers.Length == 0) {
				GatherRenderers();
			}

			if (renderSeparator == null) {
				if (skeletonRenderer)
					renderSeparator = skeletonRenderer.GetComponent<SkeletonRenderSeparator>();
				else
					renderSeparator = this.GetComponentInParent<SkeletonRenderSeparator>();
			}

			int count = referenceRenderers.Length;
			referenceMeshFilters = new MeshFilter[count];
			for (int i = 0; i < count; ++i) {
				referenceMeshFilters[i] = referenceRenderers[i].GetComponent<MeshFilter>();
			}

			ownRenderer = this.GetComponent<MeshRenderer>();
			if (ownRenderer == null)
				ownRenderer = this.gameObject.AddComponent<MeshRenderer>();
			ownMeshFilter = this.GetComponent<MeshFilter>();
			if (ownMeshFilter == null)
				ownMeshFilter = this.gameObject.AddComponent<MeshFilter>();
		}

		void OnEnable () {
#if UNITY_EDITOR
			if (Application.isPlaying)
				Awake();
#endif
			if (skeletonRenderer) {
				skeletonRenderer.OnMeshAndMaterialsUpdated -= UpdateOnCallback;
				skeletonRenderer.OnMeshAndMaterialsUpdated += UpdateOnCallback;
				updateViaSkeletonCallback = true;
			}
			if (renderSeparator) {
				renderSeparator.OnMeshAndMaterialsUpdated -= UpdateOnCallback;
				renderSeparator.OnMeshAndMaterialsUpdated += UpdateOnCallback;
				updateViaSkeletonCallback = true;
			}
		}

		void OnDisable () {
			if (skeletonRenderer)
				skeletonRenderer.OnMeshAndMaterialsUpdated -= UpdateOnCallback;
			if (renderSeparator)
				renderSeparator.OnMeshAndMaterialsUpdated -= UpdateOnCallback;
		}

		void OnDestroy () {
			for (int i = 0; i < 2; ++i) {
				Mesh mesh = doubleBufferedMesh.GetNext();
#if UNITY_EDITOR
				if (Application.isEditor && !Application.isPlaying)
					UnityEngine.Object.DestroyImmediate(mesh);
				else
					UnityEngine.Object.Destroy(mesh);
#else
				UnityEngine.Object.Destroy(mesh);
#endif
			}
		}

		void LateUpdate () {
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				UpdateMesh();
				return;
			}
#endif

			if (updateViaSkeletonCallback)
				return;
			UpdateMesh();
		}

		void UpdateOnCallback (SkeletonRenderer r) {
			UpdateMesh();
		}

		protected void EnsureBufferSizes (int combinedVertexCount, int combinedIndexCount) {
			if (positionBuffer == null) {
				positionBuffer = new ExposedList<Vector3>(combinedVertexCount);
				uvBuffer = new ExposedList<Vector2>(combinedVertexCount);
				colorBuffer = new ExposedList<Color32>(combinedVertexCount);
				indexBuffer = new ExposedList<int>(combinedIndexCount);
			}

			if (positionBuffer.Count != combinedVertexCount) {
				positionBuffer.Resize(combinedVertexCount);
				uvBuffer.Resize(combinedVertexCount);
				colorBuffer.Resize(combinedVertexCount);
			}
			if (indexBuffer.Count != combinedIndexCount) {
				indexBuffer.Resize(combinedIndexCount);
			}
		}

		void InitMesh () {
			if (doubleBufferedMesh == null) {
				doubleBufferedMesh = new DoubleBuffered<Mesh>();
				for (int i = 0; i < 2; ++i) {
					Mesh combinedMesh = doubleBufferedMesh.GetNext();
					combinedMesh.MarkDynamic();
					combinedMesh.name = "RenderCombinedMesh" + i;
					combinedMesh.subMeshCount = 1;
				}
			}
		}

		void UpdateMesh () {
			InitMesh();
			int combinedVertexCount = 0;
			int combinedIndexCount = 0;
			GetCombinedMeshInfo(ref combinedVertexCount, ref combinedIndexCount);

			EnsureBufferSizes(combinedVertexCount, combinedIndexCount);

			int combinedV = 0;
			int combinedI = 0;
			for (int r = 0, rendererCount = referenceMeshFilters.Length; r < rendererCount; ++r) {
				MeshFilter meshFilter = referenceMeshFilters[r];
				Mesh mesh = meshFilter.sharedMesh;
				if (mesh == null) continue;

				int vertexCount = mesh.vertexCount;
				Vector3[] positions = mesh.vertices;
				Vector2[] uvs = mesh.uv;
				Color32[] colors = mesh.colors32;

				System.Array.Copy(positions, 0, this.positionBuffer.Items, combinedV, vertexCount);
				System.Array.Copy(uvs, 0, this.uvBuffer.Items, combinedV, vertexCount);
				System.Array.Copy(colors, 0, this.colorBuffer.Items, combinedV, vertexCount);

				for (int s = 0, submeshCount = mesh.subMeshCount; s < submeshCount; ++s) {
					int submeshIndexCount = (int)mesh.GetIndexCount(s);
					int[] submeshIndices = mesh.GetIndices(s);
					int[] dstIndices = this.indexBuffer.Items;
					for (int i = 0; i < submeshIndexCount; ++i)
						dstIndices[i + combinedI] = submeshIndices[i] + combinedV;
					combinedI += submeshIndexCount;
				}
				combinedV += vertexCount;
			}

			Mesh combinedMesh = doubleBufferedMesh.GetNext();
			combinedMesh.Clear();
#if SET_VERTICES_HAS_LENGTH_PARAMETER
			combinedMesh.SetVertices(this.positionBuffer.Items, 0, this.positionBuffer.Count);
			combinedMesh.SetUVs(0, this.uvBuffer.Items, 0, this.uvBuffer.Count);
			combinedMesh.SetColors(this.colorBuffer.Items, 0, this.colorBuffer.Count);
			combinedMesh.SetTriangles(this.indexBuffer.Items, 0, this.indexBuffer.Count, 0);
#else
			// Note: excess already contains zero positions and indices after ExposedList.Resize().
			combinedMesh.vertices = this.positionBuffer.Items;
			combinedMesh.uv = this.uvBuffer.Items;
			combinedMesh.colors32 = this.colorBuffer.Items;
			combinedMesh.triangles = this.indexBuffer.Items;
#endif
			ownMeshFilter.sharedMesh = combinedMesh;
		}

		void GetCombinedMeshInfo (ref int vertexCount, ref int indexCount) {
			for (int r = 0, rendererCount = referenceMeshFilters.Length; r < rendererCount; ++r) {
				MeshFilter meshFilter = referenceMeshFilters[r];
				Mesh mesh = meshFilter.sharedMesh;
				if (mesh == null) continue;

				vertexCount += mesh.vertexCount;
				for (int s = 0, submeshCount = mesh.subMeshCount; s < submeshCount; ++s) {
					indexCount += (int)mesh.GetIndexCount(s);
				}
			}
		}
	}
}
