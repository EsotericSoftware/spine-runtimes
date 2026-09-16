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

#if UNITY_2018_1_OR_NEWER
#define HAS_BUILD_PROCESS_WITH_REPORT
#endif

#if UNITY_2021_2_OR_NEWER
#define HAS_BUILD_PLAYER_PROCESSOR
#endif

#if UNITY_2020_2_OR_NEWER
#define HAS_ON_POSTPROCESS_PREFAB
#endif

// Note: major_minor_OR_NEWER is automatically defined, but not major_minor_patch_OR_NEWER
#if (UNITY_2020_3 && !(UNITY_2020_3_1 || UNITY_2020_3_2 || UNITY_2020_3_3 || UNITY_2020_3_4 || UNITY_2020_3_5 || UNITY_2020_3_6 || UNITY_2020_3_7 || UNITY_2020_3_8 || UNITY_2020_3_9 || UNITY_2020_3_10 || UNITY_2020_3_11 || UNITY_2020_3_12 || UNITY_2020_3_13 || UNITY_2020_3_14 || UNITY_2020_3_15))
#define UNITY_2020_3_16_OR_NEWER
#endif
#if (UNITY_2021_1 && !(UNITY_2021_1_1 || UNITY_2021_1_2 || UNITY_2021_1_3 || UNITY_2021_1_4 || UNITY_2021_1_5 || UNITY_2021_1_6 || UNITY_2021_1_7 || UNITY_2021_1_8 || UNITY_2021_1_9 || UNITY_2021_1_10 || UNITY_2021_1_11 || UNITY_2021_1_12 || UNITY_2021_1_13 || UNITY_2021_1_14 || UNITY_2021_1_15 || UNITY_2021_1_16))
#define UNITY_2021_1_17_OR_NEWER
#endif

#if UNITY_2020_3_16_OR_NEWER || UNITY_2021_1_17_OR_NEWER || UNITY_2021_2_OR_NEWER
#define HAS_SAVE_ASSET_IF_DIRTY
#endif
#define SPINE_OPTIONAL_ON_DEMAND_LOADING

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
#if HAS_BUILD_PROCESS_WITH_REPORT
using UnityEditor.Build.Reporting;
#endif

namespace Spine.Unity.Editor {
	public class SpineBuildProcessor {
		internal static bool isBuilding = false;

#if HAS_ON_POSTPROCESS_PREFAB
		static List<string> prefabsToRestore = new List<string>();
#endif
#if SPINE_OPTIONAL_ON_DEMAND_LOADING
		/// <summary>Target textures replaced by placeholders of one loader at a material. A material may have
		/// entries of multiple loaders.</summary>
		struct OnDemandTexturesToRestore {
			public Material material;
			/// <summary>Asset path instead of a reference, as the loader asset might be unloaded during the build.</summary>
			public string loaderPath;
			public Texture[] targetTextures;
		}
		static List<string> textureLoadersToRestore = new List<string>();
		static List<OnDemandTexturesToRestore> onDemandTexturesToRestoreAtMaterial = new List<OnDemandTexturesToRestore>();
#endif
		static Dictionary<string, string> spriteAtlasTexturesToRestore = new Dictionary<string, string>();

		internal static void PreprocessBuild () {
			isBuilding = true;
			try {
#if HAS_ON_POSTPROCESS_PREFAB
				if (SpineEditorUtilities.Preferences.removePrefabPreviewMeshes)
					PreprocessSpinePrefabMeshes();
#endif
#if SPINE_OPTIONAL_ON_DEMAND_LOADING
				PreprocessOnDemandTextureLoaders();
				PreprocessOnDemandMaterialAssets();
#endif
				PreprocessSpriteAtlases();
			} catch {
				// revert any already applied pre-processing steps when aborting the build.
				PostprocessBuild();
				throw;
			}
		}

		internal static void PostprocessBuild () {
			isBuilding = false;
#if HAS_ON_POSTPROCESS_PREFAB
			if (SpineEditorUtilities.Preferences.removePrefabPreviewMeshes)
				PostprocessSpinePrefabMeshes();
#endif
#if SPINE_OPTIONAL_ON_DEMAND_LOADING
			PostprocessOnDemandMaterialAssets();
			PostprocessOnDemandTextureLoaders();
#endif
			PostprocessSpriteAtlases();
		}

#if HAS_ON_POSTPROCESS_PREFAB
		internal static void PreprocessSpinePrefabMeshes () {
			BuildUtilities.IsInSkeletonAssetBuildPreProcessing = true;
			try {
				AssetDatabase.StartAssetEditing();
				prefabsToRestore.Clear();
				string[] prefabAssets = AssetDatabase.FindAssets("t:Prefab");
				foreach (string asset in prefabAssets) {
					string assetPath = AssetDatabase.GUIDToAssetPath(asset);
					if (!AssetUtility.AssetCanBeModified(assetPath)) continue;

					GameObject prefabGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
					if (SpineEditorUtilities.CleanupSpinePrefabMesh(prefabGameObject)) {
#if HAS_SAVE_ASSET_IF_DIRTY
						AssetDatabase.SaveAssetIfDirty(prefabGameObject);
#endif
						prefabsToRestore.Add(assetPath);
					}
				}
				AssetDatabase.StopAssetEditing();
#if !HAS_SAVE_ASSET_IF_DIRTY
				if (prefabAssets.Length > 0)
					AssetDatabase.SaveAssets();
#endif
			} finally {
				BuildUtilities.IsInSkeletonAssetBuildPreProcessing = false;
			}
		}

		internal static void PostprocessSpinePrefabMeshes () {
			BuildUtilities.IsInSkeletonAssetBuildPostProcessing = true;
			try {
				foreach (string assetPath in prefabsToRestore) {
					GameObject g = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
					SpineEditorUtilities.SetupSpinePrefabMesh(g, null);
#if HAS_SAVE_ASSET_IF_DIRTY
					AssetDatabase.SaveAssetIfDirty(g);
#endif
				}
#if !HAS_SAVE_ASSET_IF_DIRTY
				if (prefabsToRestore.Count > 0)
					AssetDatabase.SaveAssets();
#endif
				prefabsToRestore.Clear();

			} finally {
				BuildUtilities.IsInSkeletonAssetBuildPostProcessing = false;
			}
		}
#endif

#if SPINE_OPTIONAL_ON_DEMAND_LOADING
		internal static void PreprocessOnDemandTextureLoaders () {
			BuildUtilities.IsInSkeletonAssetBuildPreProcessing = true;
			try {
				textureLoadersToRestore.Clear();

				// validate all active loaders before modifying any materials. Invalid loaders are skipped, so their
				// materials keep the high-resolution textures instead of receiving wrong placeholder textures.
				List<string> usedLoaderPaths = new List<string>();
				List<OnDemandTextureLoader> skippedLoaders = new List<OnDemandTextureLoader>();
				string[] loaderAssets = AssetDatabase.FindAssets("t:OnDemandTextureLoader");
				foreach (string loaderAsset in loaderAssets) {
					string assetPath = AssetDatabase.GUIDToAssetPath(loaderAsset);
					OnDemandTextureLoader loader = AssetDatabase.LoadAssetAtPath<OnDemandTextureLoader>(assetPath);
					bool isLoaderUsed = loader && loader.atlasAsset && loader.atlasAsset.OnDemandTextureLoader == loader &&
						loader.atlasAsset.TextureLoadingMode == AtlasAssetBase.LoadingMode.OnDemand;
					if (!isLoaderUsed) continue;

					if (!loader.ValidateSetup()) {
						skippedLoaders.Add(loader);
						continue;
					}
					usedLoaderPaths.Add(assetPath);
				}

				RestoreTargetTexturesAtSkippedLoaders(skippedLoaders);
				foreach (OnDemandTextureLoader loader in skippedLoaders) {
					Debug.LogWarning(string.Format("On-demand texture loader '{0}' is skipped when building due to the invalid setup " +
						"reported above. Its textures are included in the build at full resolution and are not loaded on demand. " +
						"Please hit 'Regenerate' at the loader.", AssetDatabase.GetAssetPath(loader)), loader);
				}

				AssetDatabase.StartAssetEditing();
				try {
					foreach (string assetPath in usedLoaderPaths) {
						OnDemandTextureLoader loader = AssetDatabase.LoadAssetAtPath<OnDemandTextureLoader>(assetPath);
						IEnumerable<Material> modifiedMaterials;
						textureLoadersToRestore.Add(assetPath);
						loader.AssignPlaceholderTextures(out modifiedMaterials);

#if HAS_SAVE_ASSET_IF_DIRTY
						foreach (Material material in modifiedMaterials) {
							AssetDatabase.SaveAssetIfDirty(material);
						}
#endif
					}
				} finally {
					AssetDatabase.StopAssetEditing();
				}
#if !HAS_SAVE_ASSET_IF_DIRTY
				if (textureLoadersToRestore.Count > 0)
					AssetDatabase.SaveAssets();
#endif
			} finally {
				BuildUtilities.IsInSkeletonAssetBuildPreProcessing = false;
			}
		}

		/// <summary>Recovers atlas, blend mode and copied material assets of skipped loaders before assigning any
		/// build placeholders. Recovery scans once for all skipped loaders, even when the normal material scan is disabled.
		/// Aborts the build if any of their placeholders cannot be restored.</summary>
		static void RestoreTargetTexturesAtSkippedLoaders (List<OnDemandTextureLoader> loaders) {
			if (loaders.Count == 0) return;

			HashSet<Material> materialsToSave = new HashSet<Material>();
			AssetDatabase.StartAssetEditing();
			try {
				foreach (OnDemandTextureLoader loader in loaders) {
					List<Material> restoredMaterials;
					if (!loader.RestoreTargetTextures(out restoredMaterials)) continue;
					foreach (Material material in restoredMaterials)
						materialsToSave.Add(material);
				}

				HashSet<string> materialAssetPaths = new HashSet<string>();
				foreach (string guid in AssetDatabase.FindAssets("t:Material"))
					materialAssetPaths.Add(AssetDatabase.GUIDToAssetPath(guid));
				foreach (string assetPath in materialAssetPaths) {
					foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(assetPath)) {
						Material material = asset as Material;
						if (!material) continue;
						foreach (OnDemandTextureLoader loader in loaders) {
							if (loader.RestoreTargetTextures(material))
								materialsToSave.Add(material);
						}
						foreach (OnDemandTextureLoader loader in loaders) {
							if (loader.HasPlaceholderAssigned(material))
								throw UnrestoredPlaceholderException(loader, material);
						}
					}
				}

				// Also check the loader's own materials, which need not all be persistent assets.
				foreach (OnDemandTextureLoader loader in loaders) {
					List<Material> placeholderMaterials;
					if (loader.HasPlaceholderTexturesAssigned(out placeholderMaterials))
						throw UnrestoredPlaceholderException(loader, placeholderMaterials[0]);
				}
			} finally {
				AssetDatabase.StopAssetEditing();
				// Save successful recoveries even if another material caused the build to abort.
				foreach (Material material in materialsToSave) {
					if (!material) continue;
					EditorUtility.SetDirty(material);
#if HAS_SAVE_ASSET_IF_DIRTY
					AssetDatabase.SaveAssetIfDirty(material);
#endif
				}
#if !HAS_SAVE_ASSET_IF_DIRTY
				if (materialsToSave.Count > 0)
					AssetDatabase.SaveAssets();
#endif
			}
		}

		static BuildFailedException UnrestoredPlaceholderException (OnDemandTextureLoader loader, Material material) {
			return new BuildFailedException(string.Format("Cannot build: Material '{0}' ({1}) still references a placeholder of " +
				"skipped on-demand texture loader '{2}'. Restore its target texture references and regenerate the loader before building.",
				material.name, AssetDatabase.GetAssetPath(material), AssetDatabase.GetAssetPath(loader)));
		}

		internal static void PostprocessOnDemandTextureLoaders () {
			BuildUtilities.IsInSkeletonAssetBuildPostProcessing = true;
			try {
				foreach (string assetPath in textureLoadersToRestore) {
					OnDemandTextureLoader loader = AssetDatabase.LoadAssetAtPath<OnDemandTextureLoader>(assetPath);
					IEnumerable<Material> modifiedMaterials;
					loader.AssignTargetTextures(out modifiedMaterials);
#if HAS_SAVE_ASSET_IF_DIRTY
					foreach (Material material in modifiedMaterials) {
						AssetDatabase.SaveAssetIfDirty(material);
					}
#endif
				}
#if !HAS_SAVE_ASSET_IF_DIRTY
				if (textureLoadersToRestore.Count > 0)
					AssetDatabase.SaveAssets();
#endif
				textureLoadersToRestore.Clear();

			} finally {
				BuildUtilities.IsInSkeletonAssetBuildPostProcessing = false;
			}
		}

		internal static void PreprocessOnDemandMaterialAssets () {
			BuildUtilities.IsInSkeletonAssetBuildPreProcessing = true;
			try {
				onDemandTexturesToRestoreAtMaterial.Clear();
				if (!SpineEditorUtilities.Preferences.scanOnDemandMaterials || textureLoadersToRestore.Count == 0) return;

				List<OnDemandTextureLoader> loaders = new List<OnDemandTextureLoader>(textureLoadersToRestore.Count);
				List<string> loaderPaths = new List<string>(textureLoadersToRestore.Count);
				foreach (string assetPath in textureLoadersToRestore) {
					OnDemandTextureLoader loader = AssetDatabase.LoadAssetAtPath<OnDemandTextureLoader>(assetPath);
					if (loader) {
						loaders.Add(loader);
						loaderPaths.Add(assetPath);
					}
				}
				if (loaders.Count == 0) return;

				HashSet<string> materialAssetPaths = new HashSet<string>();
				string[] materialAssetGuids = AssetDatabase.FindAssets("t:Material");
				foreach (string materialAssetGuid in materialAssetGuids)
					materialAssetPaths.Add(AssetDatabase.GUIDToAssetPath(materialAssetGuid));

				AssetDatabase.StartAssetEditing();
				try {
					foreach (string assetPath in materialAssetPaths) {
						UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
						foreach (UnityEngine.Object asset in assets) {
							Material material = asset as Material;
							if (!material) continue;

							// every loader gets its turn, a material might reference target textures of multiple loaders.
							bool anyPlaceholderAssigned = false;
							for (int loaderIndex = 0; loaderIndex < loaders.Count; ++loaderIndex) {
								Texture[] targetTextures;
								if (!loaders[loaderIndex].AssignPlaceholderTextures(material, out targetTextures)) continue;

								onDemandTexturesToRestoreAtMaterial.Add(new OnDemandTexturesToRestore {
									material = material,
									loaderPath = loaderPaths[loaderIndex],
									targetTextures = targetTextures
								});
								anyPlaceholderAssigned = true;
							}
							if (anyPlaceholderAssigned) {
								EditorUtility.SetDirty(material);
#if HAS_SAVE_ASSET_IF_DIRTY
								AssetDatabase.SaveAssetIfDirty(material);
#endif
							}
							LogUncoveredTargetTextures(material, assetPath, loaders);
						}
					}
				} finally {
					AssetDatabase.StopAssetEditing();
				}
#if !HAS_SAVE_ASSET_IF_DIRTY
				if (onDemandTexturesToRestoreAtMaterial.Count > 0)
					AssetDatabase.SaveAssets();
#endif
			} finally {
				BuildUtilities.IsInSkeletonAssetBuildPreProcessing = false;
			}
		}

		/// <summary>Logs a warning for each texture property of the material which still references an on-demand
		/// target texture after placeholders have been assigned, e.g. a differently named normal map property of a
		/// custom shader. Such high-resolution textures are included in the build instead of being loaded on demand.</summary>
		internal static void LogUncoveredTargetTextures (Material material, string assetPath, List<OnDemandTextureLoader> loaders) {
			string[] texturePropertyNames = OnDemandTextureLoader.GetTexturePropertyNames(material);
			foreach (string propertyName in texturePropertyNames) {
				Texture texture = material.GetTexture(propertyName);
				if (!texture) continue;

				foreach (OnDemandTextureLoader loader in loaders) {
					if (!loader.IsTargetTexture(texture)) continue;
					Debug.LogWarning(string.Format("On-demand target texture '{0}' is still referenced at texture property '{1}' " +
						"of Material '{2}' after assigning placeholders, as the property is not covered by loader '{3}' " +
						"(neither main texture nor listed in 'Additional Texture Properties'). " +
						"The high-resolution texture will be included in the build instead of being loaded on demand.",
						texture.name, propertyName, assetPath, loader.name), material);
					break;
				}
			}
		}

		internal static void PostprocessOnDemandMaterialAssets () {
			BuildUtilities.IsInSkeletonAssetBuildPostProcessing = true;
			try {
				if (onDemandTexturesToRestoreAtMaterial.Count == 0) return;
				AssetDatabase.StartAssetEditing();
				try {
					foreach (OnDemandTexturesToRestore texturesToRestore in onDemandTexturesToRestoreAtMaterial) {
						Material material = texturesToRestore.material;
						if (!material) continue;
						OnDemandTextureLoader loader = AssetDatabase.LoadAssetAtPath<OnDemandTextureLoader>(texturesToRestore.loaderPath);
						if (!loader) continue;

						loader.RestoreTargetTextures(material, texturesToRestore.targetTextures);
						EditorUtility.SetDirty(material);
#if HAS_SAVE_ASSET_IF_DIRTY
						AssetDatabase.SaveAssetIfDirty(material);
#endif
					}
				} finally {
					AssetDatabase.StopAssetEditing();
				}
#if !HAS_SAVE_ASSET_IF_DIRTY
				if (onDemandTexturesToRestoreAtMaterial.Count > 0)
					AssetDatabase.SaveAssets();
#endif
				onDemandTexturesToRestoreAtMaterial.Clear();
			} finally {
				BuildUtilities.IsInSkeletonAssetBuildPostProcessing = false;
			}
		}
#endif
		internal static void PreprocessSpriteAtlases () {
			BuildUtilities.IsInSpriteAtlasBuildPreProcessing = true;
			try {
				AssetDatabase.StartAssetEditing();
				spriteAtlasTexturesToRestore.Clear();
				string[] spriteAtlasAssets = AssetDatabase.FindAssets("t:SpineSpriteAtlasAsset");
				foreach (string asset in spriteAtlasAssets) {
					string assetPath = AssetDatabase.GUIDToAssetPath(asset);
					if (!AssetUtility.AssetCanBeModified(assetPath)) continue;

					SpineSpriteAtlasAsset atlasAsset = AssetDatabase.LoadAssetAtPath<SpineSpriteAtlasAsset>(assetPath);
					if (atlasAsset && atlasAsset.materials.Length > 0) {
						spriteAtlasTexturesToRestore[assetPath] = AssetDatabase.GetAssetPath(atlasAsset.materials[0].mainTexture);
						atlasAsset.materials[0].mainTexture = null;
					}
#if HAS_SAVE_ASSET_IF_DIRTY
					AssetDatabase.SaveAssetIfDirty(atlasAsset);
#endif
				}
				AssetDatabase.StopAssetEditing();
#if !HAS_SAVE_ASSET_IF_DIRTY
				if (spriteAtlasAssets.Length > 0)
					AssetDatabase.SaveAssets();
#endif
			} finally {
				BuildUtilities.IsInSpriteAtlasBuildPreProcessing = false;
			}
		}

		internal static void PostprocessSpriteAtlases () {
			BuildUtilities.IsInSpriteAtlasBuildPostProcessing = true;
			try {
				foreach (KeyValuePair<string, string> pair in spriteAtlasTexturesToRestore) {
					string assetPath = pair.Key;
					SpineSpriteAtlasAsset atlasAsset = AssetDatabase.LoadAssetAtPath<SpineSpriteAtlasAsset>(assetPath);
					if (atlasAsset && atlasAsset.materials.Length > 0) {
						Texture atlasTexture = AssetDatabase.LoadAssetAtPath<Texture>(pair.Value);
						atlasAsset.materials[0].mainTexture = atlasTexture;
					}
#if HAS_SAVE_ASSET_IF_DIRTY
					AssetDatabase.SaveAssetIfDirty(atlasAsset);
#endif
				}
#if !HAS_SAVE_ASSET_IF_DIRTY
				if (spriteAtlasTexturesToRestore.Count > 0)
					AssetDatabase.SaveAssets();
#endif
				spriteAtlasTexturesToRestore.Clear();
			} finally {
				BuildUtilities.IsInSpriteAtlasBuildPostProcessing = false;
			}
		}
	}

#if HAS_BUILD_PLAYER_PROCESSOR
	/// <summary>
	/// Build Preprocessor for Unity 2021.2 and newer.
	/// Unfortunately BuildPlayerProcessors seem to be executed before IPreprocessBuildWithReport regardless of
	/// callbackOrder, thus requiring use of this base class to call pre-build hooks before Addressables or
	/// Asset Bundles are built.
	/// </summary>
	public class SpineBuildPreprocessor : UnityEditor.Build.BuildPlayerProcessor {
		public override int callbackOrder {
			get { return -2000; }
		}

		public override void PrepareForBuild (BuildPlayerContext buildPlayerContext) {
			SpineBuildProcessor.PreprocessBuild();
		}
	}
#elif HAS_BUILD_PROCESS_WITH_REPORT
	public class SpineBuildPreprocessor : IPreprocessBuildWithReport {
		public int callbackOrder {
			get { return -2000; }
		}

		void IPreprocessBuildWithReport.OnPreprocessBuild (BuildReport report) {
			SpineBuildProcessor.PreprocessBuild();
		}
	}
#else
	public class SpineBuildPreprocessor : IPreprocessBuild {
		public int callbackOrder {
			get { return -2000; }
		}

		void IPreprocessBuild.OnPreprocessBuild (BuildTarget target, string path) {
			SpineBuildProcessor.PreprocessBuild();
		}
	}
#endif

	public class SpineBuildPostprocessor :
#if HAS_BUILD_PROCESS_WITH_REPORT
		IPostprocessBuildWithReport
#else
		IPostprocessBuild
#endif
	{
		public int callbackOrder {
			get { return 2000; }
		}


#if HAS_BUILD_PROCESS_WITH_REPORT
		void IPostprocessBuildWithReport.OnPostprocessBuild (BuildReport report) {
			SpineBuildProcessor.PostprocessBuild();
		}
#else
		void IPostprocessBuild.OnPostprocessBuild (BuildTarget target, string path) {
			SpineBuildProcessor.PostprocessBuild();
		}
#endif
	}
}
