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

#if UNITY_2018_2_OR_NEWER
#define HAS_CULL_TRANSPARENT_MESH
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Spine.Unity.Examples {
	using MaterialReplacement = RenderExistingMesh.MaterialReplacement;

#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	public class RenderExistingMeshGraphic : MonoBehaviour {
		public SkeletonGraphic referenceSkeletonGraphic;
		public Material replacementMaterial;

		public MaterialReplacement[] replacementMaterials = new MaterialReplacement[0];

		SkeletonSubmeshGraphic ownGraphic;
		public List<SkeletonSubmeshGraphic> ownSubmeshGraphics;

#if UNITY_EDITOR
		private void Reset () {
			Awake();
			LateUpdate();
		}
#endif

		void Awake () {
			// subscribe to OnMeshAndMaterialsUpdated
			if (referenceSkeletonGraphic) {
				referenceSkeletonGraphic.OnMeshAndMaterialsUpdated -= UpdateOnCallback;
				referenceSkeletonGraphic.OnMeshAndMaterialsUpdated += UpdateOnCallback;
			}

			ownGraphic = this.GetComponent<SkeletonSubmeshGraphic>();
			if (referenceSkeletonGraphic) {
				if (referenceSkeletonGraphic.allowMultipleCanvasRenderers)
					EnsureCanvasRendererCount(referenceSkeletonGraphic.canvasRenderers.Count);
				else
					SetupSubmeshGraphic();
			}
		}

		protected void OnDisable () {
			if (referenceSkeletonGraphic) {
				referenceSkeletonGraphic.OnMeshAndMaterialsUpdated -= UpdateOnCallback;
			}
		}

		protected void OnEnable () {
#if UNITY_EDITOR
			// handle disabled scene reload
			if (Application.isPlaying) {
				Awake();
				return;
			}
#endif
			if (referenceSkeletonGraphic) {
				referenceSkeletonGraphic.OnMeshAndMaterialsUpdated -= UpdateOnCallback;
				referenceSkeletonGraphic.OnMeshAndMaterialsUpdated += UpdateOnCallback;
			}
		}

		void SetupSubmeshGraphic () {
			if (ownGraphic == null)
				ownGraphic = this.gameObject.AddComponent<SkeletonSubmeshGraphic>();

			ownGraphic.maskable = referenceSkeletonGraphic.maskable;
#if HAS_CULL_TRANSPARENT_MESH
			ownGraphic.canvasRenderer.cullTransparentMesh = referenceSkeletonGraphic.canvasRenderer.cullTransparentMesh;
#endif
			ownGraphic.canvasRenderer.SetMaterial(replacementMaterial, referenceSkeletonGraphic.mainTexture);
		}

		protected void EnsureCanvasRendererCount (int targetCount) {
			if (ownSubmeshGraphics == null)
				ownSubmeshGraphics = new List<SkeletonSubmeshGraphic>();

#if HAS_CULL_TRANSPARENT_MESH
			bool cullTransparentMesh = referenceSkeletonGraphic.canvasRenderer.cullTransparentMesh;
#endif
			Vector2 pivot = referenceSkeletonGraphic.rectTransform.pivot;

			int currentCount = ownSubmeshGraphics.Count;
			for (int i = currentCount; i < targetCount; ++i) {
				GameObject go = new GameObject(string.Format("Renderer{0}", i), typeof(RectTransform));
				go.transform.SetParent(this.transform, false);
				go.transform.localPosition = Vector3.zero;
				CanvasRenderer canvasRenderer = go.AddComponent<CanvasRenderer>();
#if HAS_CULL_TRANSPARENT_MESH
				canvasRenderer.cullTransparentMesh = cullTransparentMesh;
#endif
				SkeletonSubmeshGraphic submeshGraphic = go.AddComponent<SkeletonSubmeshGraphic>();
				ownSubmeshGraphics.Add(submeshGraphic);
				submeshGraphic.maskable = referenceSkeletonGraphic.maskable;
				submeshGraphic.raycastTarget = false;
				submeshGraphic.rectTransform.pivot = pivot;
				submeshGraphic.rectTransform.anchorMin = Vector2.zero;
				submeshGraphic.rectTransform.anchorMax = Vector2.one;
				submeshGraphic.rectTransform.sizeDelta = Vector2.zero;
			}
		}

		protected void UpdateCanvasRenderers () {
			Mesh[] referenceMeshes = referenceSkeletonGraphic.MeshesMultipleCanvasRenderers.Items;
			Material[] referenceMaterials = referenceSkeletonGraphic.MaterialsMultipleCanvasRenderers.Items;
			Texture[] referenceTextures = referenceSkeletonGraphic.TexturesMultipleCanvasRenderers.Items;

			int end = Math.Min(ownSubmeshGraphics.Count, referenceSkeletonGraphic.TexturesMultipleCanvasRenderers.Count);

			for (int i = 0; i < end; i++) {
				SkeletonSubmeshGraphic submeshGraphic = ownSubmeshGraphics[i];
				CanvasRenderer reference = referenceSkeletonGraphic.canvasRenderers[i];

				if (reference.gameObject.activeInHierarchy) {
					Material usedMaterial = replacementMaterial != null ?
						replacementMaterial : GetReplacementMaterialFor(referenceMaterials[i]);
					if (usedMaterial == null)
						usedMaterial = referenceMaterials[i];
					usedMaterial = referenceSkeletonGraphic.GetModifiedMaterial(usedMaterial);
					submeshGraphic.canvasRenderer.SetMaterial(usedMaterial, referenceTextures[i]);
					submeshGraphic.canvasRenderer.SetMesh(referenceMeshes[i]);
					submeshGraphic.gameObject.SetActive(true);
				} else {
					submeshGraphic.canvasRenderer.Clear();
					submeshGraphic.gameObject.SetActive(false);
				}
			}
		}

		protected void DisableCanvasRenderers () {
			for (int i = 0; i < ownSubmeshGraphics.Count; i++) {
				SkeletonSubmeshGraphic submeshGraphic = ownSubmeshGraphics[i];
				submeshGraphic.canvasRenderer.Clear();
				submeshGraphic.gameObject.SetActive(false);
			}
		}

		protected Material GetReplacementMaterialFor (Material originalMaterial) {
			for (int i = 0; i < replacementMaterials.Length; ++i) {
				MaterialReplacement entry = replacementMaterials[i];
				if (entry.originalMaterial != null && entry.originalMaterial.shader == originalMaterial.shader)
					return entry.replacementMaterial;
			}
			return null;
		}

#if UNITY_EDITOR
		void LateUpdate () {
			if (!Application.isPlaying) {
				UpdateMesh();
			}
		}
#endif

		void UpdateOnCallback (SkeletonGraphic g) {
			UpdateMesh();
		}

		void UpdateMesh () {
			if (!referenceSkeletonGraphic) return;

			if (referenceSkeletonGraphic.allowMultipleCanvasRenderers) {
				EnsureCanvasRendererCount(referenceSkeletonGraphic.canvasRenderers.Count);
				UpdateCanvasRenderers();
				if (ownGraphic)
					ownGraphic.canvasRenderer.Clear();
			} else {
				if (ownGraphic == null)
					ownGraphic = this.gameObject.AddComponent<SkeletonSubmeshGraphic>();

				DisableCanvasRenderers();

				Material referenceMaterial = referenceSkeletonGraphic.materialForRendering;
				Material usedMaterial = replacementMaterial != null ? replacementMaterial : GetReplacementMaterialFor(referenceMaterial);
				if (usedMaterial == null)
					usedMaterial = referenceMaterial;
				usedMaterial = referenceSkeletonGraphic.GetModifiedMaterial(usedMaterial);
				ownGraphic.canvasRenderer.SetMaterial(usedMaterial, referenceSkeletonGraphic.mainTexture);
				Mesh mesh = referenceSkeletonGraphic.GetLastMesh();
				ownGraphic.canvasRenderer.SetMesh(mesh);
			}
		}
	}
}
