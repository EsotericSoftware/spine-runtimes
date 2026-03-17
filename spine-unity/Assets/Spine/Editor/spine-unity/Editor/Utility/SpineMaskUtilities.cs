/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

#pragma warning disable 0219

#define SPINE_SKELETONMECANIM

#if UNITY_2017_2_OR_NEWER
#define NEWPLAYMODECALLBACKS
#endif

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

#if UNITY_2018 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEWHIERARCHYWINDOWCALLBACKS
#endif

#if UNITY_2017_1_OR_NEWER
#define BUILT_IN_SPRITE_MASK_COMPONENT
#endif

#if BUILT_IN_SPRITE_MASK_COMPONENT

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	public class SpineMaskUtilities {

		private const string MATERIAL_FILENAME_SUFFIX_INSIDE_MASK = "_InsideMask";
		private const string MATERIAL_FILENAME_SUFFIX_OUTSIDE_MASK = "_OutsideMask";

		public static void EditorGatherAtlasAssetsMaskMaterials () {
			string[] guids = AssetDatabase.FindAssets("t:AtlasAssetBase");
			foreach (string guid in guids) {
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (!string.IsNullOrEmpty(path)) {
					AtlasAssetBase atlasAsset = AssetDatabase.LoadAssetAtPath<AtlasAssetBase>(path);
					if (atlasAsset && !atlasAsset.HasMaterialOverrideSets)
						EditorGatherAtlasAssetMaskMaterials(atlasAsset);
				}
			}
		}

		public static void EditorGatherAtlasAssetMaskMaterials (AtlasAssetBase atlasAsset) {
			EditorGatherAtlasAssetMaskMaterials(atlasAsset,
				SkeletonRenderer.MATERIAL_OVERRIDE_SET_INSIDE_MASK_NAME,
				SkeletonRenderer.STENCIL_COMP_MASKINTERACTION_VISIBLE_INSIDE);
			EditorGatherAtlasAssetMaskMaterials(atlasAsset,
				SkeletonRenderer.MATERIAL_OVERRIDE_SET_OUTSIDE_MASK_NAME,
				SkeletonRenderer.STENCIL_COMP_MASKINTERACTION_VISIBLE_OUTSIDE);
		}

		public static void EditorGatherAtlasAssetMaskMaterials (AtlasAssetBase atlasAsset,
			string overrideSetName, UnityEngine.Rendering.CompareFunction maskFunction) {

			MaterialOverrideSet overrideSet = atlasAsset.GetMaterialOverrideSet(overrideSetName);
			foreach (Material originalMaterial in atlasAsset.Materials) {
				string originalMaterialPath = AssetDatabase.GetAssetPath(originalMaterial);
				if (string.IsNullOrEmpty(originalMaterialPath)) continue;

				string maskMaterialPath = MaskMaterialPath(originalMaterialPath, maskFunction);
				Material maskMaterial = AssetDatabase.LoadAssetAtPath<Material>(maskMaterialPath);
				if (maskMaterial != null) {
					if (overrideSet == null)
						overrideSet = atlasAsset.AddMaterialOverrideSet(overrideSetName);
					overrideSet.SetOverride(originalMaterial, maskMaterial);
				}
			}
		}

		public static void EditorSetupSpriteMaskMaterials (SkeletonRenderer skeleton) {
			SpriteMaskInteraction maskInteraction = skeleton.MaskInteraction;
			if (maskInteraction == SpriteMaskInteraction.VisibleInsideMask) {
				if (skeleton.insideMaskMaterials == null)
					EditorInitSpriteMaskMaterialsInsideMask(skeleton);
			} else if (maskInteraction == SpriteMaskInteraction.VisibleOutsideMask) {
				if (skeleton.outsideMaskMaterials == null)
					EditorInitSpriteMaskMaterialsOutsideMask(skeleton);
			}
		}

		private static void EditorInitSpriteMaskMaterialsInsideMask (SkeletonRenderer skeleton) {
			EditorInitSpriteMaskMaterialsMaskMode(ref skeleton.insideMaskMaterials,
				skeleton,
				SkeletonRenderer.MATERIAL_OVERRIDE_SET_INSIDE_MASK_NAME,
				SkeletonRenderer.STENCIL_COMP_MASKINTERACTION_VISIBLE_INSIDE);
		}

		private static void EditorInitSpriteMaskMaterialsOutsideMask (SkeletonRenderer skeleton) {
			EditorInitSpriteMaskMaterialsMaskMode(ref skeleton.outsideMaskMaterials,
				skeleton,
				SkeletonRenderer.MATERIAL_OVERRIDE_SET_OUTSIDE_MASK_NAME,
				SkeletonRenderer.STENCIL_COMP_MASKINTERACTION_VISIBLE_OUTSIDE);
		}

		private static void EditorInitSpriteMaskMaterialsMaskMode (ref MaterialOverrideSet[] maskMaterials,
			SkeletonRenderer skeleton,
			string overrideSetName, UnityEngine.Rendering.CompareFunction maskFunction) {
			AtlasAssetBase[] atlasAssets = skeleton.skeletonDataAsset.atlasAssets;
			int atlasAssetCount = atlasAssets.Length;
			if (maskMaterials == null || maskMaterials.Length != atlasAssetCount)
				maskMaterials = new MaterialOverrideSet[atlasAssetCount];

			for (int i = 0, n = atlasAssetCount; i < n; ++i) {
				AtlasAssetBase atlasAsset = atlasAssets[i];
				maskMaterials[i] = atlasAsset.GetMaterialOverrideSet(overrideSetName);
				if (maskMaterials[i] == null && !Application.isPlaying) {
					maskMaterials[i] = EditorInitSpriteMaskOverrideSet(
						atlasAsset, overrideSetName, maskFunction);
				}
			}
			skeleton.UpdateMaterials();
		}

		private static MaterialOverrideSet EditorInitSpriteMaskOverrideSet(
			AtlasAssetBase atlasAsset, string overrideSetName, UnityEngine.Rendering.CompareFunction maskFunction) {

			MaterialOverrideSet overrideSet = atlasAsset.AddMaterialOverrideSet(overrideSetName);
			foreach (Material originalMaterial in atlasAsset.Materials) {
				Material maskMaterial = EditorCreateOrLoadMaskMaterialAsset(maskFunction, originalMaterial);
				if (maskMaterial == null) {
					maskMaterial = new Material(originalMaterial);
					maskMaterial.name += overrideSetName;
					maskMaterial.SetFloat(SkeletonRenderer.STENCIL_COMP_PARAM_ID, (int)maskFunction);
				}
				overrideSet.AddOverride(originalMaterial, maskMaterial);
			}
			UnityEditor.EditorUtility.SetDirty(atlasAsset);
			UnityEditor.AssetDatabase.SaveAssets();
			return overrideSet;
		}

		public static Material EditorCreateOrLoadMaskMaterialAsset (UnityEngine.Rendering.CompareFunction maskFunction,
			Material originalMaterial) {

			string originalMaterialPath = UnityEditor.AssetDatabase.GetAssetPath(originalMaterial);
			string materialPath = MaskMaterialPath(originalMaterialPath, maskFunction);
			Material material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(materialPath);
			if (material != null) {
				return material;
			}

			material = new Material(originalMaterial);
			material.SetFloat(SkeletonRenderer.STENCIL_COMP_PARAM_ID, (int)maskFunction);

			UnityEditor.AssetDatabase.CreateAsset(material, materialPath);
			Debug.Log(string.Concat("Created material '", materialPath, "' for mask interaction based on '", originalMaterialPath, "'."));
			UnityEditor.EditorUtility.SetDirty(material);
			UnityEditor.AssetDatabase.SaveAssets();
			return material;
		}

		public static string InsideMaskMaterialPath (string originalMaterialPath) {
			return MaskMaterialPath(originalMaterialPath, SkeletonRenderer.STENCIL_COMP_MASKINTERACTION_VISIBLE_INSIDE);
		}

		public static string OutsideMaskMaterialPath (string originalMaterialPath) {
			return MaskMaterialPath(originalMaterialPath, SkeletonRenderer.STENCIL_COMP_MASKINTERACTION_VISIBLE_OUTSIDE);
		}

		public static string MaskMaterialPath (string originalMaterialPath,
			UnityEngine.Rendering.CompareFunction maskFunction) {

			int posOfExtensionDot = originalMaterialPath.LastIndexOf('.');
			return (maskFunction == SkeletonRenderer.STENCIL_COMP_MASKINTERACTION_VISIBLE_INSIDE) ?
				originalMaterialPath.Insert(posOfExtensionDot, MATERIAL_FILENAME_SUFFIX_INSIDE_MASK) :
				originalMaterialPath.Insert(posOfExtensionDot, MATERIAL_FILENAME_SUFFIX_OUTSIDE_MASK);
		}
	}
}
#endif // BUILT_IN_SPRITE_MASK_COMPONENT
