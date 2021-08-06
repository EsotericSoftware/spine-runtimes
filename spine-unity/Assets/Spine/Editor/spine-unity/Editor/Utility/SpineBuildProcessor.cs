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

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Build;
#if HAS_BUILD_PROCESS_WITH_REPORT
using UnityEditor.Build.Reporting;
#endif

namespace Spine.Unity.Editor {
	public class SpineBuildProcessor
	{
		internal static bool isBuilding = false;

#if HAS_ON_POSTPROCESS_PREFAB
		static List<string> prefabsToRestore = new List<string>();
#endif

		internal static void PreprocessBuild()
		{
			isBuilding = true;
#if HAS_ON_POSTPROCESS_PREFAB
			var assets = AssetDatabase.FindAssets("t:Prefab");
			foreach(var asset in assets) {
				string assetPath = AssetDatabase.GUIDToAssetPath(asset);
				GameObject g = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
				if (SpineEditorUtilities.CleanupSpinePrefabMesh(g)) {
					prefabsToRestore.Add(assetPath);
				}
			}
			AssetDatabase.SaveAssets();
#endif
		}

		internal static void PostprocessBuild()
		{
			isBuilding = false;
#if HAS_ON_POSTPROCESS_PREFAB
			foreach (string assetPath in prefabsToRestore) {
				GameObject g = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
				SpineEditorUtilities.SetupSpinePrefabMesh(g, null);
			}
			AssetDatabase.SaveAssets();
#endif
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
		void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
		{
			SpineBuildProcessor.PreprocessBuild();
		}
#else
		void IPreprocessBuild.OnPreprocessBuild(BuildTarget target, string path)
		{
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
		void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report)
		{
			SpineBuildProcessor.PostprocessBuild();
		}
#else
		void IPostprocessBuild.OnPostprocessBuild(BuildTarget target, string path)
		{
			SpineBuildProcessor.PostprocessBuild();
		}
#endif
	}
}
