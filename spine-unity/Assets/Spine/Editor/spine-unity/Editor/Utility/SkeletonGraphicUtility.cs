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

#if UNITY_2017_2_OR_NEWER
#define NEWPLAYMODECALLBACKS
#endif

using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	public static class SkeletonGraphicUtility {
		#region Auto Detect Setting
		public static void DetectTintBlack (SkeletonGraphic skeletonGraphic) {
			bool requiresTintBlack = HasTintBlackSlot(skeletonGraphic);
			if (requiresTintBlack)
				Debug.Log(string.Format("Found Tint-Black slot at '{0}'", skeletonGraphic));
			else
				Debug.Log(string.Format("No Tint-Black slot found at '{0}'", skeletonGraphic));
			skeletonGraphic.MeshSettings.tintBlack = requiresTintBlack;
		}

		public static bool HasTintBlackSlot (SkeletonGraphic skeletonGraphic) {
			SlotData[] slotsItems = skeletonGraphic.SkeletonData.Slots.Items;
			for (int i = 0, count = skeletonGraphic.SkeletonData.Slots.Count; i < count; ++i) {
				SlotData slotData = slotsItems[i];
				if (slotData.GetSetupPose().GetDarkColor().HasValue)
					return true;
			}
			return false;
		}

		public static void DetectCanvasGroupCompatible (SkeletonGraphic skeletonGraphic) {
			bool requiresCanvasGroupCompatible = IsBelowCanvasGroup(skeletonGraphic);
			if (requiresCanvasGroupCompatible)
				Debug.Log(string.Format("Skeleton is a child of CanvasGroup: '{0}'", skeletonGraphic));
			else
				Debug.Log(string.Format("Skeleton is not a child of CanvasGroup: '{0}'", skeletonGraphic));
			skeletonGraphic.MeshSettings.canvasGroupCompatible = requiresCanvasGroupCompatible;
		}

		public static bool IsBelowCanvasGroup (SkeletonGraphic skeletonGraphic) {
			return skeletonGraphic.gameObject.GetComponentInParent<CanvasGroup>() != null;
		}

		public static void DetectPMAVertexColors (SkeletonGraphic skeletonGraphic) {
			MeshGenerator.Settings settings = skeletonGraphic.MeshSettings;
			bool usesSpineShader = MaterialChecks.UsesSpineShader(skeletonGraphic.material);
			if (!usesSpineShader) {
				Debug.Log(string.Format("Skeleton is not using a Spine shader, thus the shader is likely " +
					"not using PMA vertex color: '{0}'", skeletonGraphic));
				skeletonGraphic.MeshSettings.pmaVertexColors = false;
				return;
			}

			bool requiresPMAVertexColorsDisabled = settings.canvasGroupCompatible && !settings.tintBlack;
			if (requiresPMAVertexColorsDisabled) {
				Debug.Log(string.Format("Skeleton requires PMA Vertex Colors disabled: '{0}'", skeletonGraphic));
				skeletonGraphic.MeshSettings.pmaVertexColors = false;
			} else {
				Debug.Log(string.Format("Skeleton requires or permits PMA Vertex Colors enabled: '{0}'", skeletonGraphic));
				skeletonGraphic.MeshSettings.pmaVertexColors = true;
			}
		}

		public static bool IsSkeletonTexturePMA (SkeletonGraphic skeletonGraphic, out bool detectionSucceeded) {
			Texture texture = skeletonGraphic.mainTexture;
			string texturePath = AssetDatabase.GetAssetPath(texture.GetInstanceID());
			TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texturePath);
			if (importer.alphaIsTransparency != importer.sRGBTexture) {
				Debug.LogWarning(string.Format("Texture '{0}' at skeleton '{1}' is neither configured correctly for " +
					"PMA nor Straight Alpha.", texture, skeletonGraphic), texture);
				detectionSucceeded = false;
				return false;
			}
			detectionSucceeded = true;
			bool isPMATexture = !importer.alphaIsTransparency && !importer.sRGBTexture;
			return isPMATexture;
		}

		public static void DetectMaterial (SkeletonGraphic skeletonGraphic) {
			MeshGenerator.Settings settings = skeletonGraphic.MeshSettings;

			bool detectionSucceeded;
			bool usesPMATexture = IsSkeletonTexturePMA(skeletonGraphic, out detectionSucceeded);
			if (!detectionSucceeded) {
				Debug.LogWarning(string.Format("Unable to assign Material for skeleton '{0}'.", skeletonGraphic), skeletonGraphic);
				return;
			}

			Material newMaterial = null;
			if (usesPMATexture) {
				if (settings.tintBlack) {
					if (settings.canvasGroupCompatible)
						newMaterial = MaterialWithName("SkeletonGraphicTintBlack-CanvasGroup");
					else
						newMaterial = MaterialWithName("SkeletonGraphicTintBlack");
				} else { // not tintBlack
					if (settings.canvasGroupCompatible)
						newMaterial = MaterialWithName("SkeletonGraphicDefault-CanvasGroup");
					else
						newMaterial = MaterialWithName("SkeletonGraphicDefault");
				}
			} else { // straight alpha texture
				if (settings.tintBlack) {
					if (settings.canvasGroupCompatible)
						newMaterial = MaterialWithName("SkeletonGraphicTintBlack-CanvasGroupStraight");
					else
						newMaterial = MaterialWithName("SkeletonGraphicTintBlack-Straight");
				} else { // not tintBlack
					if (settings.canvasGroupCompatible)
						newMaterial = MaterialWithName("SkeletonGraphicDefault-CanvasGroupStraight");
					else
						newMaterial = MaterialWithName("SkeletonGraphicDefault-Straight");
				}
			}
			if (newMaterial != null) {
				Debug.Log(string.Format("Assigning material '{0}' at skeleton '{1}'",
					newMaterial, skeletonGraphic), newMaterial);
				skeletonGraphic.material = newMaterial;
			}
		}

		public static void DetectBlendModeMaterials (SkeletonGraphic skeletonGraphic) {
			bool detectionSucceeded;
			bool usesPMATexture = IsSkeletonTexturePMA(skeletonGraphic, out detectionSucceeded);
			if (!detectionSucceeded) {
				Debug.LogWarning(string.Format("Unable to assign Blend Mode materials for skeleton '{0}'.", skeletonGraphic), skeletonGraphic);
				return;
			}
			DetectBlendModeMaterial(skeletonGraphic, BlendMode.Additive, usesPMATexture);
			DetectBlendModeMaterial(skeletonGraphic, BlendMode.Multiply, usesPMATexture);
			DetectBlendModeMaterial(skeletonGraphic, BlendMode.Screen, usesPMATexture);
		}

		public static void DetectBlendModeMaterial (SkeletonGraphic skeletonGraphic, BlendMode blendMode, bool usesPMATexture) {
			MeshGenerator.Settings settings = skeletonGraphic.MeshSettings;

			string optionalTintBlack = settings.tintBlack ? "TintBlack" : "";
			string blendModeString = blendMode.ToString();
			string optionalDash = settings.canvasGroupCompatible || !usesPMATexture ? "-" : "";
			string optionalCanvasGroup = settings.canvasGroupCompatible ? "CanvasGroup" : "";
			string optionalStraight = !usesPMATexture ? "Straight" : "";

			string materialName = string.Format("SkeletonGraphic{0}{1}{2}{3}{4}",
				optionalTintBlack, blendModeString, optionalDash, optionalCanvasGroup, optionalStraight);
			Material newMaterial = MaterialWithName(materialName);

			if (newMaterial != null) {
				switch (blendMode) {
				case BlendMode.Additive:
					skeletonGraphic.additiveMaterial = newMaterial;
					break;
				case BlendMode.Multiply:
					skeletonGraphic.multiplyMaterial = newMaterial;
					break;
				case BlendMode.Screen:
					skeletonGraphic.screenMaterial = newMaterial;
					break;
				}
			}
		}
		#endregion Auto Detect Setting

		#region Material Defaults
		public static Material DefaultSkeletonGraphicMaterial {
			get {
				return MaterialWithName(SpineEditorUtilities.Preferences.UsesPMAWorkflow ?
					"SkeletonGraphicDefault" :
					"SkeletonGraphicDefault-Straight");
			}
		}

		public static Material DefaultSkeletonGraphicAdditiveMaterial {
			get {
				return MaterialWithName(SpineEditorUtilities.Preferences.UsesPMAWorkflow ?
					"SkeletonGraphicAdditive" :
					"SkeletonGraphicAdditive-Straight");
			}
		}

		public static Material DefaultSkeletonGraphicMultiplyMaterial {
			get {
				return MaterialWithName(SpineEditorUtilities.Preferences.UsesPMAWorkflow ?
					"SkeletonGraphicMultiply" :
					"SkeletonGraphicMultiply-Straight");
			}
		}

		public static Material DefaultSkeletonGraphicScreenMaterial {
			get {
				return MaterialWithName(SpineEditorUtilities.Preferences.UsesPMAWorkflow ?
					"SkeletonGraphicScreen" :
					"SkeletonGraphicScreen-Straight");
			}
		}

		public static Material MaterialWithName (string name) {
			string[] guids = AssetDatabase.FindAssets(name + " t:material");
			if (guids.Length <= 0) return null;

			int closestNameDistance = int.MaxValue;
			int closestNameIndex = 0;
			for (int i = 0; i < guids.Length; ++i) {
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
				int distance = string.CompareOrdinal(assetName, name);
				if (distance < closestNameDistance) {
					closestNameDistance = distance;
					closestNameIndex = i;
				}
			}

			string foundAssetPath = AssetDatabase.GUIDToAssetPath(guids[closestNameIndex]);
			if (string.IsNullOrEmpty(foundAssetPath)) return null;

			Material firstMaterial = AssetDatabase.LoadAssetAtPath<Material>(foundAssetPath);
			return firstMaterial;
		}
		#endregion Material Defaults
	}
}
