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

// from spine-unity 4.0 onward BlendModeMaterialAssets are obsolete and shall be upgraded.
#define UPGRADE_ALL_BLEND_MODE_MATERIALS

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	public class BlendModeMaterialsUtility {

		public const string MATERIAL_SUFFIX_MULTIPLY = "-Multiply";
		public const string MATERIAL_SUFFIX_SCREEN = "-Screen";
		public const string MATERIAL_SUFFIX_ADDITIVE = "-Additive";

#if UPGRADE_ALL_BLEND_MODE_MATERIALS
		public const bool ShallUpgradeBlendModeMaterials = true;
#else
		public const bool ShallUpgradeBlendModeMaterials = false;
#endif

		protected class TemplateMaterials {
			public Material multiplyTemplate;
			public Material screenTemplate;
			public Material additiveTemplate;
		};

		public static void UpgradeBlendModeMaterials (SkeletonDataAsset skeletonDataAsset) {
			var skeletonData = skeletonDataAsset.GetSkeletonData(true);
			if (skeletonData == null)
				return;
			UpdateBlendModeMaterials(skeletonDataAsset, ref skeletonData, true);
		}

		public static void UpdateBlendModeMaterials (SkeletonDataAsset skeletonDataAsset) {
			var skeletonData = skeletonDataAsset.GetSkeletonData(true);
			if (skeletonData == null)
				return;
			UpdateBlendModeMaterials(skeletonDataAsset, ref skeletonData, false);
		}

		public static void UpdateBlendModeMaterials (SkeletonDataAsset skeletonDataAsset, ref SkeletonData skeletonData,
			bool upgradeFromModifierAssets = ShallUpgradeBlendModeMaterials) {

			TemplateMaterials templateMaterials = new TemplateMaterials();
			bool anyMaterialsChanged = ClearUndesiredMaterialEntries(skeletonDataAsset);

			var blendModesModifierAsset = FindBlendModeMaterialsModifierAsset(skeletonDataAsset);
			if (blendModesModifierAsset) {
				if (upgradeFromModifierAssets) {
					TransferSettingsFromModifierAsset(blendModesModifierAsset,
					skeletonDataAsset, templateMaterials);
					UpdateBlendmodeMaterialsRequiredState(skeletonDataAsset, skeletonData);
				} else
					return;
			} else {
				if (!UpdateBlendmodeMaterialsRequiredState(skeletonDataAsset, skeletonData))
					return;
				AssignPreferencesTemplateMaterials(templateMaterials);
			}
			bool success = CreateAndAssignMaterials(skeletonDataAsset, templateMaterials, ref anyMaterialsChanged);
			if (success) {
				if (blendModesModifierAsset != null) {
					RemoveObsoleteModifierAsset(blendModesModifierAsset, skeletonDataAsset);
				}
			}

			SpineEditorUtilities.ClearSkeletonDataAsset(skeletonDataAsset);
			skeletonData = skeletonDataAsset.GetSkeletonData(true);
			if (anyMaterialsChanged)
				ReloadSceneSkeletons(skeletonDataAsset);
			AssetDatabase.SaveAssets();
		}

		protected static bool ClearUndesiredMaterialEntries (SkeletonDataAsset skeletonDataAsset) {
			Predicate<BlendModeMaterials.ReplacementMaterial> ifMaterialMissing = r => r.material == null;

			bool anyMaterialsChanged = false;
			if (!skeletonDataAsset.blendModeMaterials.applyAdditiveMaterial) {
				anyMaterialsChanged |= skeletonDataAsset.blendModeMaterials.additiveMaterials.Count > 0;
				skeletonDataAsset.blendModeMaterials.additiveMaterials.Clear();
			} else
				anyMaterialsChanged |= skeletonDataAsset.blendModeMaterials.additiveMaterials.RemoveAll(ifMaterialMissing) != 0;
			anyMaterialsChanged |= skeletonDataAsset.blendModeMaterials.multiplyMaterials.RemoveAll(ifMaterialMissing) != 0;
			anyMaterialsChanged |= skeletonDataAsset.blendModeMaterials.screenMaterials.RemoveAll(ifMaterialMissing) != 0;
			return anyMaterialsChanged;
		}

		protected static BlendModeMaterialsAsset FindBlendModeMaterialsModifierAsset (SkeletonDataAsset skeletonDataAsset) {
			foreach (var modifierAsset in skeletonDataAsset.skeletonDataModifiers) {
				if (modifierAsset is BlendModeMaterialsAsset)
					return (BlendModeMaterialsAsset)modifierAsset;
			}
			return null;
		}

		protected static bool UpdateBlendmodeMaterialsRequiredState (SkeletonDataAsset skeletonDataAsset, SkeletonData skeletonData) {
			return skeletonDataAsset.blendModeMaterials.UpdateBlendmodeMaterialsRequiredState(skeletonData);
		}

		protected static void TransferSettingsFromModifierAsset (BlendModeMaterialsAsset modifierAsset,
			SkeletonDataAsset skeletonDataAsset, TemplateMaterials templateMaterials) {

			skeletonDataAsset.blendModeMaterials.TransferSettingsFrom(modifierAsset);

			templateMaterials.multiplyTemplate = modifierAsset.multiplyMaterialTemplate;
			templateMaterials.screenTemplate = modifierAsset.screenMaterialTemplate;
			templateMaterials.additiveTemplate = modifierAsset.additiveMaterialTemplate;
		}

		protected static void RemoveObsoleteModifierAsset (BlendModeMaterialsAsset modifierAsset,
			SkeletonDataAsset skeletonDataAsset) {

			skeletonDataAsset.skeletonDataModifiers.Remove(modifierAsset);
			Debug.Log(string.Format("BlendModeMaterialsAsset upgraded to built-in BlendModeMaterials at SkeletonData asset '{0}'.",
				skeletonDataAsset.name), skeletonDataAsset);
			EditorUtility.SetDirty(skeletonDataAsset);
		}

		protected static void AssignPreferencesTemplateMaterials (TemplateMaterials templateMaterials) {

			templateMaterials.multiplyTemplate = SpineEditorUtilities.Preferences.BlendModeMaterialMultiply;
			templateMaterials.screenTemplate = SpineEditorUtilities.Preferences.BlendModeMaterialScreen;
			templateMaterials.additiveTemplate = SpineEditorUtilities.Preferences.BlendModeMaterialAdditive;
		}

		protected static bool CreateAndAssignMaterials (SkeletonDataAsset skeletonDataAsset,
			TemplateMaterials templateMaterials, ref bool anyReplacementMaterialsChanged) {

			bool anyCreationFailed = false;
			var blendModeMaterials = skeletonDataAsset.blendModeMaterials;
			bool applyAdditiveMaterial = blendModeMaterials.applyAdditiveMaterial;

			var skinEntries = new List<Skin.SkinEntry>();

			SpineEditorUtilities.ClearSkeletonDataAsset(skeletonDataAsset);
			skeletonDataAsset.isUpgradingBlendModeMaterials = true;
			SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(true);

			var slotsItems = skeletonData.Slots.Items;
			for (int slotIndex = 0, slotCount = skeletonData.Slots.Count; slotIndex < slotCount; slotIndex++) {
				var slot = slotsItems[slotIndex];
				if (slot.BlendMode == BlendMode.Normal) continue;
				if (!applyAdditiveMaterial && slot.BlendMode == BlendMode.Additive) continue;

				List<BlendModeMaterials.ReplacementMaterial> replacementMaterials = null;
				Material materialTemplate = null;
				string materialSuffix = null;
				switch (slot.BlendMode) {
				case BlendMode.Multiply:
					replacementMaterials = blendModeMaterials.multiplyMaterials;
					materialTemplate = templateMaterials.multiplyTemplate;
					materialSuffix = MATERIAL_SUFFIX_MULTIPLY;
					break;
				case BlendMode.Screen:
					replacementMaterials = blendModeMaterials.screenMaterials;
					materialTemplate = templateMaterials.screenTemplate;
					materialSuffix = MATERIAL_SUFFIX_SCREEN;
					break;
				case BlendMode.Additive:
					replacementMaterials = blendModeMaterials.additiveMaterials;
					materialTemplate = templateMaterials.additiveTemplate;
					materialSuffix = MATERIAL_SUFFIX_ADDITIVE;
					break;
				}

				skinEntries.Clear();
				foreach (var skin in skeletonData.Skins)
					skin.GetAttachments(slotIndex, skinEntries);

				foreach (var entry in skinEntries) {
					var renderableAttachment = entry.Attachment as IHasRendererObject;
					if (renderableAttachment != null) {
						var originalRegion = (AtlasRegion)renderableAttachment.RendererObject;
						bool replacementExists = replacementMaterials.Exists(
							replacement => replacement.pageName == originalRegion.page.name);
						if (!replacementExists) {
							bool createdNewMaterial;
							var replacement = CreateOrLoadReplacementMaterial(originalRegion, materialTemplate, materialSuffix, out createdNewMaterial);
							if (replacement != null) {
								replacementMaterials.Add(replacement);
								anyReplacementMaterialsChanged = true;
								if (createdNewMaterial) {
									Debug.Log(string.Format("Created blend mode Material '{0}' for SkeletonData asset '{1}'.",
										replacement.material.name, skeletonDataAsset), replacement.material);
								}
							} else {
								Debug.LogError(string.Format("Failed creating blend mode Material for SkeletonData asset '{0}'," +
									" atlas page '{1}', template '{2}'.",
									skeletonDataAsset.name, originalRegion.page.name, materialTemplate.name),
									skeletonDataAsset);
								anyCreationFailed = true;
							}
						}
					}
				}
			}

			skeletonDataAsset.isUpgradingBlendModeMaterials = false;
			EditorUtility.SetDirty(skeletonDataAsset);
			return !anyCreationFailed;
		}

		protected static string GetBlendModeMaterialPath (AtlasPage originalPage, string materialSuffix) {
			var originalMaterial = originalPage.rendererObject as Material;
			var originalPath = AssetDatabase.GetAssetPath(originalMaterial);
			return originalPath.Replace(".mat", materialSuffix + ".mat");
		}

		protected static BlendModeMaterials.ReplacementMaterial CreateOrLoadReplacementMaterial (
			AtlasRegion originalRegion, Material materialTemplate, string materialSuffix, out bool createdNewMaterial) {

			createdNewMaterial = false;
			var newReplacement = new BlendModeMaterials.ReplacementMaterial();
			var originalPage = originalRegion.page;
			var originalMaterial = originalPage.rendererObject as Material;
			var blendMaterialPath = GetBlendModeMaterialPath(originalPage, materialSuffix);

			newReplacement.pageName = originalPage.name;
			if (File.Exists(blendMaterialPath)) {
				newReplacement.material = AssetDatabase.LoadAssetAtPath<Material>(blendMaterialPath);
			} else {
				var blendModeMaterial = new Material(materialTemplate) {
					name = originalMaterial.name + " " + materialTemplate.name,
					mainTexture = originalMaterial.mainTexture
				};
				newReplacement.material = blendModeMaterial;

				AssetDatabase.CreateAsset(blendModeMaterial, blendMaterialPath);
				EditorUtility.SetDirty(blendModeMaterial);
				createdNewMaterial = true;
			}

			if (newReplacement.material)
				return newReplacement;
			else
				return null;
		}

		protected static void ReloadSceneSkeletons (SkeletonDataAsset skeletonDataAsset) {
			if (SpineEditorUtilities.Preferences.autoReloadSceneSkeletons)
				SpineEditorUtilities.DataReloadHandler.ReloadSceneSkeletonComponents(skeletonDataAsset);
		}
	}
}
