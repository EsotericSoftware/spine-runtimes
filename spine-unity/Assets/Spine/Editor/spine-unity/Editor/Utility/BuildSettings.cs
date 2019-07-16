/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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

#if UNITY_2018_3_OR_NEWER
#define NEW_PREFERENCES_SETTINGS_PROVIDER
#endif

#if UNITY_2019_1_OR_NEWER
#define NEW_TIMELINE_AS_PACKAGE
#endif

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Globalization;

namespace Spine.Unity.Editor {
	public partial class SpineEditorUtilities {
		public static class SpineTK2DEditorUtility {
			const string SPINE_TK2D_DEFINE = "SPINE_TK2D";

			internal static void EnableTK2D () {
				SpineBuildEnvUtility.DisableSpineAsmdefFiles();
				SpineBuildEnvUtility.EnableBuildDefine(SPINE_TK2D_DEFINE);
			}

			internal static void DisableTK2D () {
				SpineBuildEnvUtility.EnableSpineAsmdefFiles();
				SpineBuildEnvUtility.DisableBuildDefine(SPINE_TK2D_DEFINE);
			}
		}

		public static class SpinePackageDependencyUtility
		{
			public enum RequestState {
				NoRequestIssued = 0,
				InProgress,
				Success,
				Failure
			}

			#if NEW_TIMELINE_AS_PACKAGE
			const string SPINE_TIMELINE_PACKAGE_DOWNLOADED_DEFINE = "SPINE_TIMELINE_PACKAGE_DOWNLOADED";
			const string TIMELINE_PACKAGE_NAME = "com.unity.timeline";
			const string TIMELINE_ASMDEF_DEPENDENCY_STRING = "\"Unity.Timeline\"";
			static UnityEditor.PackageManager.Requests.AddRequest timelineRequest = null;
			
			/// <summary>
			/// Enables Spine's Timeline components by downloading the Timeline Package in Unity 2019 and newer
			/// and setting respective compile definitions once downloaded.
			/// </summary>
			internal static void EnableTimelineSupport () {
				Debug.Log("Downloading Timeline package " + TIMELINE_PACKAGE_NAME + ".");
				timelineRequest = UnityEditor.PackageManager.Client.Add(TIMELINE_PACKAGE_NAME);
				// Note: unfortunately there is no callback provided, only polling support.
				// So polling HandlePendingAsyncTimelineRequest() is necessary.

				EditorApplication.update -= UpdateAsyncTimelineRequest;
				EditorApplication.update += UpdateAsyncTimelineRequest;
			}

			public static void UpdateAsyncTimelineRequest () {
				HandlePendingAsyncTimelineRequest();
			}

			public static RequestState HandlePendingAsyncTimelineRequest () {
				if (timelineRequest == null)
					return RequestState.NoRequestIssued;

				var status = timelineRequest.Status;
				if (status == UnityEditor.PackageManager.StatusCode.InProgress) {
					return RequestState.InProgress;
				}
				else {
					EditorApplication.update -= UpdateAsyncTimelineRequest;
					timelineRequest = null;
					if (status == UnityEditor.PackageManager.StatusCode.Failure) {
						Debug.LogError("Download of package " + TIMELINE_PACKAGE_NAME + " failed!");
						return RequestState.Failure;
					}
					else { // status == UnityEditor.PackageManager.StatusCode.Success
						HandleSuccessfulTimelinePackageDownload();
						return RequestState.Success;
					}
				}
			}

			internal static void DisableTimelineSupport () {
				SpineBuildEnvUtility.DisableBuildDefine(SPINE_TIMELINE_PACKAGE_DOWNLOADED_DEFINE);
				SpineBuildEnvUtility.RemoveDependencyFromAsmdefFile(TIMELINE_ASMDEF_DEPENDENCY_STRING);
			}

			internal static void HandleSuccessfulTimelinePackageDownload () {

				#if !SPINE_TK2D
				SpineBuildEnvUtility.EnableSpineAsmdefFiles();
				#endif
				SpineBuildEnvUtility.AddDependencyToAsmdefFile(TIMELINE_ASMDEF_DEPENDENCY_STRING);
				SpineBuildEnvUtility.EnableBuildDefine(SPINE_TIMELINE_PACKAGE_DOWNLOADED_DEFINE);

				ReimportTimelineScripts();
			}

			internal static void ReimportTimelineScripts () {
				// Note: unfortunately AssetDatabase::Refresh is not enough and
				// ImportAsset on a dir does not have the desired effect.
				List<string> searchStrings = new List<string>();
				searchStrings.Add("SpineAnimationStateBehaviour t:script");
				searchStrings.Add("SpineAnimationStateClip t:script");
				searchStrings.Add("SpineAnimationStateMixerBehaviour t:script");
				searchStrings.Add("SpineAnimationStateTrack t:script");

				searchStrings.Add("SpineSkeletonFlipBehaviour t:script");
				searchStrings.Add("SpineSkeletonFlipClip t:script");
				searchStrings.Add("SpineSkeletonFlipMixerBehaviour t:script");
				searchStrings.Add("SpineSkeletonFlipTrack t:script");

				searchStrings.Add("SkeletonAnimationPlayableHandle t:script");
				searchStrings.Add("SpinePlayableHandleBase t:script");

				foreach (string searchString in searchStrings) {
					string[] guids = AssetDatabase.FindAssets(searchString);
					foreach (string guid in guids) {
						string currentPath = AssetDatabase.GUIDToAssetPath(guid);
						AssetDatabase.ImportAsset(currentPath, ImportAssetOptions.ForceUpdate);
					}
				}
			}
			#endif
		}
	}

	public static class SpineBuildEnvUtility
	{
		static bool IsInvalidGroup (BuildTargetGroup group) {
			int gi = (int)group;
			return
				gi == 15 || gi == 16
				||
				group == BuildTargetGroup.Unknown;
		}

		public static bool EnableBuildDefine (string define) {

			bool wasDefineAdded = false;
			Debug.LogWarning("Please ignore errors \"PlayerSettings Validation: Requested build target group doesn't exist\" below");
			foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup))) {
				if (IsInvalidGroup(group))
					continue;

				string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
				if (!defines.Contains(define)) {
					wasDefineAdded = true;
					if (defines.EndsWith(";", System.StringComparison.Ordinal))
						defines += define;
					else
						defines += ";" + define;

					PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
				}
			}
			Debug.LogWarning("Please ignore errors \"PlayerSettings Validation: Requested build target group doesn't exist\" above");

			if (wasDefineAdded) {
				Debug.LogWarning("Setting Scripting Define Symbol " + define);
			}
			else {
				Debug.LogWarning("Already Set Scripting Define Symbol " + define);
			}
			return wasDefineAdded;
		}

		public static bool DisableBuildDefine (string define) {

			bool wasDefineRemoved = false;
			foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup))) {
				if (IsInvalidGroup(group))
					continue;

				string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
				if (defines.Contains(define)) {
					wasDefineRemoved = true;
					if (defines.Contains(define + ";"))
						defines = defines.Replace(define + ";", "");
					else
						defines = defines.Replace(define, "");

					PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
				}
			}

			if (wasDefineRemoved) {
				Debug.LogWarning("Removing Scripting Define Symbol " + define);
			}
			else {
				Debug.LogWarning("Already Removed Scripting Define Symbol " + define);
			}
			return wasDefineRemoved;
		}

		public static void DisableSpineAsmdefFiles () {
			SetAsmdefFileActive("spine-unity-editor", false);
			SetAsmdefFileActive("spine-unity", false);
		}

		public static void EnableSpineAsmdefFiles () {
			SetAsmdefFileActive("spine-unity-editor", true);
			SetAsmdefFileActive("spine-unity", true);
		}

		public static void AddDependencyToAsmdefFile (string dependencyName) {
			string asmdefName = "spine-unity";
			string filePath = FindAsmdefFile(asmdefName);
			if (string.IsNullOrEmpty(filePath))
				return;

			if (System.IO.File.Exists(filePath)) {
				string fileContent = File.ReadAllText(filePath);

				if (!fileContent.Contains("references")) {
					string nameLine = string.Concat("\"name\": \"", asmdefName, "\"");
					fileContent = fileContent.Replace(nameLine,
													nameLine +
													@",\n""references"": []");
				}

				if (!fileContent.Contains(dependencyName)) {
					fileContent = fileContent.Replace(@"""references"": [",
													@"""references"": [" + dependencyName);
					File.WriteAllText(filePath, fileContent);
				}
			}
		}

		public static void RemoveDependencyFromAsmdefFile (string dependencyName) {
			string asmdefName = "spine-unity";
			string filePath = FindAsmdefFile(asmdefName);
			if (string.IsNullOrEmpty(filePath))
				return;

			if (System.IO.File.Exists(filePath)) {
				string fileContent = File.ReadAllText(filePath);
				// this simple implementation shall suffice for now.
				if (fileContent.Contains(dependencyName)) {
					fileContent = fileContent.Replace(dependencyName, "");
					File.WriteAllText(filePath, fileContent);
				}
			}
		}

		internal static string FindAsmdefFile (string filename) {
			string filePath = FindAsmdefFile(filename, isDisabledFile: false);
			if (string.IsNullOrEmpty(filePath))
				filePath = FindAsmdefFile(filename, isDisabledFile: true);
			return filePath;
		}

		internal static string FindAsmdefFile (string filename, bool isDisabledFile) {

			string typeSearchString = isDisabledFile ? " t:TextAsset" : " t:AssemblyDefinitionAsset";
			string extension = isDisabledFile ? ".txt" : ".asmdef";
			string filenameWithExtension = filename + (isDisabledFile ? ".txt" : ".asmdef");
			string[] guids = AssetDatabase.FindAssets(filename + typeSearchString);
			foreach (string guid in guids) {
				string currentPath = AssetDatabase.GUIDToAssetPath(guid);
				if (!string.IsNullOrEmpty(currentPath)) {
					if (System.IO.Path.GetFileName(currentPath) == filenameWithExtension)
						return currentPath;
				}
			}
			return null;
		}

		internal static void SetAsmdefFileActive (string filename, bool setActive) {

			string typeSearchString = setActive ? " t:TextAsset" : " t:AssemblyDefinitionAsset";
			string extensionBeforeChange = setActive ? "txt" : "asmdef";
			string[] guids = AssetDatabase.FindAssets(filename + typeSearchString);
			foreach (string guid in guids) {
				string currentPath = AssetDatabase.GUIDToAssetPath(guid);
				if (!System.IO.Path.HasExtension(extensionBeforeChange)) // asmdef is also found as t:TextAsset, so check
					continue;

				string targetPath = System.IO.Path.ChangeExtension(currentPath, setActive ? "asmdef" : "txt");
				if (System.IO.File.Exists(currentPath) && !System.IO.File.Exists(targetPath)) {
					System.IO.File.Copy(currentPath, targetPath);
					System.IO.File.Copy(currentPath + ".meta", targetPath + ".meta");
				}
				AssetDatabase.DeleteAsset(currentPath);
			}
		}
	}
}
