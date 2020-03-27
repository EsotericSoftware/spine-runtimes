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

			internal static bool IsTK2DInstalled () {
				return (Shader.Find("tk2d/SolidVertexColor") != null ||
					Shader.Find("tk2d/AdditiveVertexColor") != null);
			}

			internal static bool IsTK2DAllowed {
				get {
					return false; // replace with "return true;" to allow TK2D support
				}
			}

			internal static void EnableTK2D () {
				if (!IsTK2DAllowed)
					return;
				SpineBuildEnvUtility.DisableSpineAsmdefFiles();
				SpineBuildEnvUtility.EnableBuildDefine(SPINE_TK2D_DEFINE);
			}

			internal static void DisableTK2D () {
				SpineBuildEnvUtility.EnableSpineAsmdefFiles();
				SpineBuildEnvUtility.DisableBuildDefine(SPINE_TK2D_DEFINE);
			}
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

		internal static void SetAsmdefFileActive (string filename, bool setActive) {

			string typeSearchString = setActive ? " t:TextAsset" : " t:AssemblyDefinitionAsset";
			string extensionBeforeChange = setActive ? ".txt" : ".asmdef";
			string[] guids = AssetDatabase.FindAssets(filename + typeSearchString);
			foreach (string guid in guids) {
				string currentPath = AssetDatabase.GUIDToAssetPath(guid);
				if (System.IO.Path.GetExtension(currentPath) != extensionBeforeChange) // asmdef is also found as t:TextAsset, so check
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
