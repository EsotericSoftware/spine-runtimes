/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {
#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[HelpURL("http://esotericsoftware.com/spine-unity#SkeletonGraphicCustomMaterials")]
	public class SkeletonGraphicCustomMaterials : MonoBehaviour {

		#region Inspector
		public SkeletonGraphic skeletonGraphic;
		[SerializeField] protected List<AtlasMaterialOverride> customMaterialOverrides = new List<AtlasMaterialOverride>();
		[SerializeField] protected List<AtlasTextureOverride> customTextureOverrides = new List<AtlasTextureOverride>();

#if UNITY_EDITOR
		void Reset () {
			skeletonGraphic = GetComponent<SkeletonGraphic>();

			// Populate material list
			if (skeletonGraphic != null && skeletonGraphic.skeletonDataAsset != null) {
				var atlasAssets = skeletonGraphic.skeletonDataAsset.atlasAssets;

				var initialAtlasMaterialOverrides = new List<AtlasMaterialOverride>();
				foreach (AtlasAssetBase atlasAsset in atlasAssets) {
					foreach (Material atlasMaterial in atlasAsset.Materials) {
						var atlasMaterialOverride = new AtlasMaterialOverride {
							overrideEnabled = false,
							originalTexture = atlasMaterial.mainTexture
						};

						initialAtlasMaterialOverrides.Add(atlasMaterialOverride);
					}
				}
				customMaterialOverrides = initialAtlasMaterialOverrides;
			}

			// Populate texture list
			if (skeletonGraphic != null && skeletonGraphic.skeletonDataAsset != null) {
				var atlasAssets = skeletonGraphic.skeletonDataAsset.atlasAssets;

				var initialAtlasTextureOverrides = new List<AtlasTextureOverride>();
				foreach (AtlasAssetBase atlasAsset in atlasAssets) {
					foreach (Material atlasMaterial in atlasAsset.Materials) {
						var atlasTextureOverride = new AtlasTextureOverride {
							overrideEnabled = false,
							originalTexture = atlasMaterial.mainTexture
						};

						initialAtlasTextureOverrides.Add(atlasTextureOverride);
					}
				}
				customTextureOverrides = initialAtlasTextureOverrides;
			}
		}
#endif
		#endregion

		void SetCustomMaterialOverrides () {
			if (skeletonGraphic == null) {
				Debug.LogError("skeletonGraphic == null");
				return;
			}

			for (int i = 0; i < customMaterialOverrides.Count; i++) {
				AtlasMaterialOverride atlasMaterialOverride = customMaterialOverrides[i];
				if (atlasMaterialOverride.overrideEnabled)
					skeletonGraphic.CustomMaterialOverride[atlasMaterialOverride.originalTexture] = atlasMaterialOverride.replacementMaterial;
			}
		}

		void RemoveCustomMaterialOverrides () {
			if (skeletonGraphic == null) {
				Debug.LogError("skeletonGraphic == null");
				return;
			}

			for (int i = 0; i < customMaterialOverrides.Count; i++) {
				AtlasMaterialOverride atlasMaterialOverride = customMaterialOverrides[i];
				Material currentMaterial;

				if (!skeletonGraphic.CustomMaterialOverride.TryGetValue(atlasMaterialOverride.originalTexture, out currentMaterial))
					continue;

				// Do not revert the material if it was changed by something else
				if (currentMaterial != atlasMaterialOverride.replacementMaterial)
					continue;

				skeletonGraphic.CustomMaterialOverride.Remove(atlasMaterialOverride.originalTexture);
			}
		}

		void SetCustomTextureOverrides () {
			if (skeletonGraphic == null) {
				Debug.LogError("skeletonGraphic == null");
				return;
			}

			for (int i = 0; i < customTextureOverrides.Count; i++) {
				AtlasTextureOverride atlasTextureOverride = customTextureOverrides[i];
				if (atlasTextureOverride.overrideEnabled)
					skeletonGraphic.CustomTextureOverride[atlasTextureOverride.originalTexture] = atlasTextureOverride.replacementTexture;
			}
		}

		void RemoveCustomTextureOverrides () {
			if (skeletonGraphic == null) {
				Debug.LogError("skeletonGraphic == null");
				return;
			}

			for (int i = 0; i < customTextureOverrides.Count; i++) {
				AtlasTextureOverride atlasTextureOverride = customTextureOverrides[i];
				Texture currentTexture;

				if (!skeletonGraphic.CustomTextureOverride.TryGetValue(atlasTextureOverride.originalTexture, out currentTexture))
					continue;

				// Do not revert the material if it was changed by something else
				if (currentTexture != atlasTextureOverride.replacementTexture)
					continue;

				skeletonGraphic.CustomTextureOverride.Remove(atlasTextureOverride.originalTexture);
			}
		}

		// OnEnable applies the overrides at runtime, and when the editor loads.
		void OnEnable () {
			if (skeletonGraphic == null)
				skeletonGraphic = GetComponent<SkeletonGraphic>();

			if (skeletonGraphic == null) {
				Debug.LogError("skeletonGraphic == null");
				return;
			}

			skeletonGraphic.Initialize(false);
			SetCustomMaterialOverrides();
			SetCustomTextureOverrides();
		}

		// OnDisable removes the overrides at runtime, and in the editor when the component is disabled or destroyed.
		void OnDisable () {
			if (skeletonGraphic == null) {
				Debug.LogError("skeletonGraphic == null");
				return;
			}

			RemoveCustomMaterialOverrides();
			RemoveCustomTextureOverrides();
		}

		[Serializable]
		public struct AtlasMaterialOverride : IEquatable<AtlasMaterialOverride> {
			public bool overrideEnabled;
			public Texture originalTexture;
			public Material replacementMaterial;

			public bool Equals (AtlasMaterialOverride other) {
				return overrideEnabled == other.overrideEnabled && originalTexture == other.originalTexture && replacementMaterial == other.replacementMaterial;
			}
		}

		[Serializable]
		public struct AtlasTextureOverride : IEquatable<AtlasTextureOverride> {
			public bool overrideEnabled;
			public Texture originalTexture;
			public Texture replacementTexture;

			public bool Equals (AtlasTextureOverride other) {
				return overrideEnabled == other.overrideEnabled && originalTexture == other.originalTexture && replacementTexture == other.replacementTexture;
			}
		}
	}
}
