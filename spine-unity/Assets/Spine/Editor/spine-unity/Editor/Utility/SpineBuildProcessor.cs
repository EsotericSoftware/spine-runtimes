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


#if UNITY_2018_1_OR_NEWER
#define HAS_BUILD_PROCESS_WITH_REPORT
#endif

#if UNITY_2020_2_OR_NEWER
#define HAS_ON_POSTPROCESS_PREFAB
#endif

#if (UNITY_2020_3 && !(UNITY_2020_3_1 || UNITY_2020_3_2 || UNITY_2020_3_3 || UNITY_2020_3_4 || UNITY_2020_3_5 || UNITY_2020_3_6 || UNITY_2020_3_7 || UNITY_2020_3_8 || UNITY_2020_3_9 || UNITY_2020_3_10 || UNITY_2020_3_11 || UNITY_2020_3_12 || UNITY_2020_3_13 || UNITY_2020_3_14 || UNITY_2020_3_15))
#define UNITY_2020_3_16_OR_NEWER
#endif
#if (UNITY_2021_1 && !(UNITY_2021_1_1 || UNITY_2021_1_2 || UNITY_2021_1_3 || UNITY_2021_1_4 || UNITY_2021_1_5 || UNITY_2021_1_6 || UNITY_2021_1_7 || UNITY_2021_1_8 || UNITY_2021_1_9 || UNITY_2021_1_10 || UNITY_2021_1_11 || UNITY_2021_1_12 || UNITY_2021_1_13 || UNITY_2021_1_14 || UNITY_2021_1_15 || UNITY_2021_1_16))
#define UNITY_2021_1_17_OR_NEWER
#endif

#if UNITY_2020_3_16_OR_NEWER || UNITY_2021_1_17_OR_NEWER
#define HAS_SAVE_ASSET_IF_DIRTY
#endif

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
		static Dictionary<string, string> spriteAtlasTexturesToRestore = new Dictionary<string, string>();

		internal static void PreprocessBuild () {
			isBuilding = true;
#if HAS_ON_POSTPROCESS_PREFAB
			PreprocessSpinePrefabMeshes();
#endif
			PreprocessSpriteAtlases();
		}

		internal static void PostprocessBuild () {
			isBuilding = false;
#if HAS_ON_POSTPROCESS_PREFAB
			PostprocessSpinePrefabMeshes();
#endif
			PostprocessSpriteAtlases();
		}

#if HAS_ON_POSTPROCESS_PREFAB
		internal static void PreprocessSpinePrefabMeshes () {
			BuildUtilities.IsInSkeletonAssetBuildPreProcessing = true;
			try {
				AssetDatabase.StartAssetEditing();
				prefabsToRestore.Clear();
				var prefabAssets = AssetDatabase.FindAssets("t:Prefab");
				foreach (var asset in prefabAssets) {
					string assetPath = AssetDatabase.GUIDToAssetPath(asset);
					GameObject prefabGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
					if (SpineEditorUtilities.CleanupSpinePrefabMesh(prefabGameObject)) {
#if HAS_SAVE_ASSET_IF_DIRTY
						AssetDatabase.SaveAssetIfDirty(prefabGameObject);
#endif
						prefabsToRestore.Add(assetPath);
					}
					EditorUtility.UnloadUnusedAssetsImmediate();
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
		internal static void PreprocessSpriteAtlases () {
			BuildUtilities.IsInSpriteAtlasBuildPreProcessing = true;
			try {
				AssetDatabase.StartAssetEditing();
				spriteAtlasTexturesToRestore.Clear();
				var spriteAtlasAssets = AssetDatabase.FindAssets("t:SpineSpriteAtlasAsset");
				foreach (var asset in spriteAtlasAssets) {
					string assetPath = AssetDatabase.GUIDToAssetPath(asset);
					SpineSpriteAtlasAsset atlasAsset = AssetDatabase.LoadAssetAtPath<SpineSpriteAtlasAsset>(assetPath);
					if (atlasAsset && atlasAsset.materials.Length > 0) {
						spriteAtlasTexturesToRestore[assetPath] = AssetDatabase.GetAssetPath(atlasAsset.materials[0].mainTexture);
						atlasAsset.materials[0].mainTexture = null;
					}
#if HAS_SAVE_ASSET_IF_DIRTY
					AssetDatabase.SaveAssetIfDirty(atlasAsset);
#endif
					EditorUtility.UnloadUnusedAssetsImmediate();
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
				foreach (var pair in spriteAtlasTexturesToRestore) {
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

	public class SpineBuildPreprocessor :
#if HAS_BUILD_PROCESS_WITH_REPORT
		IPreprocessBuildWithReport
#else
		IPreprocessBuild
#endif
	{
		public int callbackOrder {
			get { return -2000; }
		}
#if HAS_BUILD_PROCESS_WITH_REPORT
		void IPreprocessBuildWithReport.OnPreprocessBuild (BuildReport report) {
			SpineBuildProcessor.PreprocessBuild();
		}
#else
		void IPreprocessBuild.OnPreprocessBuild (BuildTarget target, string path) {
			SpineBuildProcessor.PreprocessBuild();
		}
#endif
	}

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
