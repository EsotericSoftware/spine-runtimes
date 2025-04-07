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

		public static void EditorAssignSpriteMaskMaterials (SkeletonRenderer skeleton) {
			SkeletonRenderer.SpriteMaskInteractionMaterials maskMaterials = skeleton.maskMaterials;
			SpriteMaskInteraction maskInteraction = skeleton.maskInteraction;
			MeshRenderer meshRenderer = skeleton.GetComponent<MeshRenderer>();

			if (maskMaterials.materialsMaskDisabled.Length > 0 && maskMaterials.materialsMaskDisabled[0] != null &&
				maskInteraction == SpriteMaskInteraction.None) {
				meshRenderer.materials = maskMaterials.materialsMaskDisabled;
			} else if (maskInteraction == SpriteMaskInteraction.VisibleInsideMask) {
				if (maskMaterials.materialsInsideMask.Length == 0 || maskMaterials.materialsInsideMask[0] == null)
					EditorInitSpriteMaskMaterialsInsideMask(skeleton);
				meshRenderer.materials = maskMaterials.materialsInsideMask;
			} else if (maskInteraction == SpriteMaskInteraction.VisibleOutsideMask) {
				if (maskMaterials.materialsOutsideMask.Length == 0 || maskMaterials.materialsOutsideMask[0] == null)
					EditorInitSpriteMaskMaterialsOutsideMask(skeleton);
				meshRenderer.materials = maskMaterials.materialsOutsideMask;
			}
		}

		public static bool AreMaskMaterialsMissing (SkeletonRenderer skeleton) {
			SkeletonRenderer.SpriteMaskInteractionMaterials maskMaterials = skeleton.maskMaterials;
			SpriteMaskInteraction maskInteraction = skeleton.maskInteraction;

			if (maskInteraction == SpriteMaskInteraction.VisibleInsideMask) {
				return (maskMaterials.materialsInsideMask.Length == 0 || maskMaterials.materialsInsideMask[0] == null);
			} else if (maskInteraction == SpriteMaskInteraction.VisibleOutsideMask) {
				return (maskMaterials.materialsOutsideMask.Length == 0 || maskMaterials.materialsOutsideMask[0] == null);
			}
			return false;
		}

		public static void EditorInitMaskMaterials (SkeletonRenderer skeleton, SkeletonRenderer.SpriteMaskInteractionMaterials maskMaterials, SpriteMaskInteraction maskType) {
			if (maskType == SpriteMaskInteraction.None) {
				EditorConfirmDisabledMaskMaterialsInit(skeleton);
			} else if (maskType == SpriteMaskInteraction.VisibleInsideMask) {
				EditorInitSpriteMaskMaterialsInsideMask(skeleton);
			} else if (maskType == SpriteMaskInteraction.VisibleOutsideMask) {
				EditorInitSpriteMaskMaterialsOutsideMask(skeleton);
			}
		}

		public static void EditorDeleteMaskMaterials (SkeletonRenderer.SpriteMaskInteractionMaterials maskMaterials, SpriteMaskInteraction maskType) {
			Material[] targetMaterials;
			if (maskType == SpriteMaskInteraction.VisibleInsideMask) {
				targetMaterials = maskMaterials.materialsInsideMask;
			} else if (maskType == SpriteMaskInteraction.VisibleOutsideMask) {
				targetMaterials = maskMaterials.materialsOutsideMask;
			} else {
				Debug.LogWarning("EditorDeleteMaskMaterials: Normal materials are kept as a reference and shall never be deleted.");
				return;
			}

			for (int i = 0; i < targetMaterials.Length; ++i) {
				Material material = targetMaterials[i];
				if (material != null) {
					string materialPath = UnityEditor.AssetDatabase.GetAssetPath(material);
					UnityEditor.AssetDatabase.DeleteAsset(materialPath);
					Debug.Log(string.Concat("Deleted material '", materialPath, "'"));
				}
			}

			if (maskType == SpriteMaskInteraction.VisibleInsideMask) {
				maskMaterials.materialsInsideMask = new Material[0];
			} else if (maskType == SpriteMaskInteraction.VisibleOutsideMask) {
				maskMaterials.materialsOutsideMask = new Material[0];
			}
		}

		private static void EditorInitSpriteMaskMaterialsInsideMask (SkeletonRenderer skeleton) {
			SkeletonRenderer.SpriteMaskInteractionMaterials maskMaterials = skeleton.maskMaterials;
			EditorInitSpriteMaskMaterialsForMaskType(skeleton, SkeletonRenderer.STENCIL_COMP_MASKINTERACTION_VISIBLE_INSIDE,
													ref maskMaterials.materialsInsideMask);
		}

		private static void EditorInitSpriteMaskMaterialsOutsideMask (SkeletonRenderer skeleton) {
			SkeletonRenderer.SpriteMaskInteractionMaterials maskMaterials = skeleton.maskMaterials;
			EditorInitSpriteMaskMaterialsForMaskType(skeleton, SkeletonRenderer.STENCIL_COMP_MASKINTERACTION_VISIBLE_OUTSIDE,
													ref maskMaterials.materialsOutsideMask);
		}

		private static void EditorInitSpriteMaskMaterialsForMaskType (SkeletonRenderer skeleton, UnityEngine.Rendering.CompareFunction maskFunction,
																ref Material[] materialsToFill) {
			if (!EditorConfirmDisabledMaskMaterialsInit(skeleton))
				return;

			SkeletonRenderer.SpriteMaskInteractionMaterials maskMaterials = skeleton.maskMaterials;
			Material[] originalMaterials = maskMaterials.materialsMaskDisabled;
			materialsToFill = new Material[originalMaterials.Length];
			for (int i = 0; i < originalMaterials.Length; i++) {
				Material newMaterial = null;

				if (!Application.isPlaying) {
					newMaterial = EditorCreateOrLoadMaskMaterialAsset(maskMaterials, maskFunction, originalMaterials[i]);
				}
				if (newMaterial == null) {
					newMaterial = new Material(originalMaterials[i]);
					newMaterial.SetFloat(SkeletonRenderer.STENCIL_COMP_PARAM_ID, (int)maskFunction);
				}
				materialsToFill[i] = newMaterial;
			}
		}

		private static bool EditorConfirmDisabledMaskMaterialsInit (SkeletonRenderer skeleton) {
			SkeletonRenderer.SpriteMaskInteractionMaterials maskMaterials = skeleton.maskMaterials;
			if (maskMaterials.materialsMaskDisabled.Length > 0 && maskMaterials.materialsMaskDisabled[0] != null) {
				return true;
			}

			MeshRenderer meshRenderer = skeleton.GetComponent<MeshRenderer>();
			Material[] currentMaterials = meshRenderer.sharedMaterials;

			if (currentMaterials.Length == 0 || currentMaterials[0] == null) {
				// Note: if no attachments are visible, no materials are set. This is a valid state.
				return false;
			}

			// We have to be sure that there has not been a recompilation or similar events that led to
			// inside- or outside-mask materials being assigned to meshRenderer.sharedMaterials.
			string firstMaterialPath = UnityEditor.AssetDatabase.GetAssetPath(currentMaterials[0]);
			if (firstMaterialPath.Contains(MATERIAL_FILENAME_SUFFIX_INSIDE_MASK) ||
				firstMaterialPath.Contains(MATERIAL_FILENAME_SUFFIX_OUTSIDE_MASK)) {

				maskMaterials.materialsMaskDisabled = new Material[currentMaterials.Length];
				for (int i = 0; i < currentMaterials.Length; ++i) {
					string path = UnityEditor.AssetDatabase.GetAssetPath(currentMaterials[i]);
					string correctPath = null;
					if (path.Contains(MATERIAL_FILENAME_SUFFIX_INSIDE_MASK)) {
						correctPath = path.Replace(MATERIAL_FILENAME_SUFFIX_INSIDE_MASK, "");
					} else if (path.Contains(MATERIAL_FILENAME_SUFFIX_OUTSIDE_MASK)) {
						correctPath = path.Replace(MATERIAL_FILENAME_SUFFIX_OUTSIDE_MASK, "");
					}

					if (correctPath != null) {
						Material material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(correctPath);
						if (material == null)
							Debug.LogWarning("No original ignore-mask material found for path " + correctPath);
						maskMaterials.materialsMaskDisabled[i] = material;
					}
				}
			} else {
				maskMaterials.materialsMaskDisabled = currentMaterials;
			}
			return true;
		}

		public static Material EditorCreateOrLoadMaskMaterialAsset (SkeletonRenderer.SpriteMaskInteractionMaterials maskMaterials,
																UnityEngine.Rendering.CompareFunction maskFunction, Material originalMaterial) {
			string originalMaterialPath = UnityEditor.AssetDatabase.GetAssetPath(originalMaterial);
			int posOfExtensionDot = originalMaterialPath.LastIndexOf('.');
			string materialPath = (maskFunction == SkeletonRenderer.STENCIL_COMP_MASKINTERACTION_VISIBLE_INSIDE) ?
													originalMaterialPath.Insert(posOfExtensionDot, MATERIAL_FILENAME_SUFFIX_INSIDE_MASK) :
													originalMaterialPath.Insert(posOfExtensionDot, MATERIAL_FILENAME_SUFFIX_OUTSIDE_MASK);

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
	}
}
#endif // BUILT_IN_SPRITE_MASK_COMPONENT
