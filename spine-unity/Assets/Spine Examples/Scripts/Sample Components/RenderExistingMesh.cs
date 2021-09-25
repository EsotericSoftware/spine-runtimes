/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {

#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
	public class RenderExistingMesh : MonoBehaviour {
		public MeshRenderer referenceRenderer;

		bool updateViaSkeletonCallback = false;
		MeshFilter referenceMeshFilter;
		MeshRenderer ownRenderer;
		MeshFilter ownMeshFilter;

		[System.Serializable]
		public struct MaterialReplacement {
			public Material originalMaterial;
			public Material replacementMaterial;
		}
		public MaterialReplacement[] replacementMaterials = new MaterialReplacement[0];

		private Dictionary<Material, Material> replacementMaterialDict = new Dictionary<Material, Material>();
		private Material[] sharedMaterials = new Material[0];

#if UNITY_EDITOR
		private void Reset () {
			if (referenceRenderer == null) {
				referenceRenderer = this.transform.parent.GetComponentInParent<MeshRenderer>();
				if (!referenceRenderer)
					return;
			}

			var parentMaterials = referenceRenderer.sharedMaterials;
			if (replacementMaterials.Length != parentMaterials.Length) {
				replacementMaterials = new MaterialReplacement[parentMaterials.Length];
			}
			for (int i = 0; i < parentMaterials.Length; ++i) {
				replacementMaterials[i].originalMaterial = parentMaterials[i];
				replacementMaterials[i].replacementMaterial = parentMaterials[i];
			}
			Awake();
			LateUpdate();
		}
#endif

		void Awake () {
			if (referenceRenderer == null) {
				referenceRenderer = this.transform.parent.GetComponentInParent<MeshRenderer>();
			}

			// subscribe to OnMeshAndMaterialsUpdated
			var skeletonRenderer = referenceRenderer.GetComponent<SkeletonAnimation>();
			if (skeletonRenderer) {
				skeletonRenderer.OnMeshAndMaterialsUpdated -= UpdateOnCallback;
				skeletonRenderer.OnMeshAndMaterialsUpdated += UpdateOnCallback;
				updateViaSkeletonCallback = true;
			}
			referenceMeshFilter = referenceRenderer.GetComponent<MeshFilter>();
			ownRenderer = this.GetComponent<MeshRenderer>();
			ownMeshFilter = this.GetComponent<MeshFilter>();

			InitializeDict();
		}

#if UNITY_EDITOR
		private void Update () {
			if (!Application.isPlaying) {
				InitializeDict();
			}
		}
#endif

		void LateUpdate () {
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				UpdateMaterials();
				return;
			}
#endif

			if (updateViaSkeletonCallback)
				return;
			UpdateMaterials();
		}

		void UpdateOnCallback (SkeletonRenderer r) {
			UpdateMaterials();
		}

		void UpdateMaterials () {
			ownMeshFilter.sharedMesh = referenceMeshFilter.sharedMesh;

			var parentMaterials = referenceRenderer.sharedMaterials;
			if (sharedMaterials.Length != parentMaterials.Length) {
				sharedMaterials = new Material[parentMaterials.Length];
			}
			for (int i = 0; i < parentMaterials.Length; ++i) {
				var parentMaterial = parentMaterials[i];
				if (replacementMaterialDict.ContainsKey(parentMaterial)) {
					sharedMaterials[i] = replacementMaterialDict[parentMaterial];
				}
			}
			ownRenderer.sharedMaterials = sharedMaterials;
		}

		void InitializeDict () {
			for (int i = 0; i < replacementMaterials.Length; ++i) {
				var entry = replacementMaterials[i];
				replacementMaterialDict[entry.originalMaterial] = entry.replacementMaterial;
			}
		}
	}
}
