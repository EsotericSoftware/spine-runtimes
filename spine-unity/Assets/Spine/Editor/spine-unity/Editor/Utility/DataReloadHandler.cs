/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	public partial class SpineEditorUtilities {
		public static class DataReloadHandler {

			internal static Dictionary<int, string> savedSkeletonDataAssetAtSKeletonGraphicID = new Dictionary<int, string>();

#if NEWPLAYMODECALLBACKS
			internal static void OnPlaymodeStateChanged (PlayModeStateChange stateChange) {
#else
			internal static void OnPlaymodeStateChanged () {
#endif
				ReloadAllActiveSkeletonsEditMode();
			}

			public static void ReloadAllActiveSkeletonsEditMode () {

				if (EditorApplication.isPaused) return;
				if (EditorApplication.isPlaying) return;
				if (EditorApplication.isCompiling) return;
				if (EditorApplication.isPlayingOrWillChangePlaymode) return;

				var skeletonDataAssetsToReload = new HashSet<SkeletonDataAsset>();

				var activeSkeletonRenderers = GameObject.FindObjectsOfType<SkeletonRenderer>();
				foreach (var sr in activeSkeletonRenderers) {
					var skeletonDataAsset = sr.skeletonDataAsset;
					if (skeletonDataAsset != null) skeletonDataAssetsToReload.Add(skeletonDataAsset);
				}

				// Under some circumstances (e.g. on first import) SkeletonGraphic objects
				// have their skeletonGraphic.skeletonDataAsset reference corrupted
				// by the instance of the ScriptableObject being destroyed but still assigned.
				// Here we save the skeletonGraphic.skeletonDataAsset asset path in order
				// to restore it later.
				var activeSkeletonGraphics = GameObject.FindObjectsOfType<SkeletonGraphic>();
				foreach (var skeletonGraphic in activeSkeletonGraphics) {
					var skeletonDataAsset = skeletonGraphic.skeletonDataAsset;
					if (skeletonDataAsset != null) {
						var assetPath = AssetDatabase.GetAssetPath(skeletonDataAsset);
						var sgID = skeletonGraphic.GetInstanceID();
						savedSkeletonDataAssetAtSKeletonGraphicID[sgID] = assetPath;
						skeletonDataAssetsToReload.Add(skeletonDataAsset);
					}
				}

				foreach (var skeletonDataAsset in skeletonDataAssetsToReload) {
					ReloadSkeletonDataAsset(skeletonDataAsset, false);
				}

				foreach (var skeletonRenderer in activeSkeletonRenderers)
					skeletonRenderer.Initialize(true);
				foreach (var skeletonGraphic in activeSkeletonGraphics)
					skeletonGraphic.Initialize(true);
			}

			public static void ReloadSceneSkeletonComponents (SkeletonDataAsset skeletonDataAsset) {
				if (EditorApplication.isPaused) return;
				if (EditorApplication.isPlaying) return;
				if (EditorApplication.isCompiling) return;
				if (EditorApplication.isPlayingOrWillChangePlaymode) return;

				var activeSkeletonRenderers = GameObject.FindObjectsOfType<SkeletonRenderer>();
				foreach (var renderer in activeSkeletonRenderers) {
					if (renderer.isActiveAndEnabled && renderer.skeletonDataAsset == skeletonDataAsset) renderer.Initialize(true);
				}

				var activeSkeletonGraphics = GameObject.FindObjectsOfType<SkeletonGraphic>();
				foreach (var graphic in activeSkeletonGraphics) {
					if (graphic.isActiveAndEnabled && graphic.skeletonDataAsset == skeletonDataAsset)
						graphic.Initialize(true);
				}
			}

			public static void ClearAnimationReferenceAssets (SkeletonDataAsset skeletonDataAsset) {
				ForEachAnimationReferenceAsset(skeletonDataAsset, (referenceAsset) => referenceAsset.Clear());
			}

			public static void ReloadAnimationReferenceAssets (SkeletonDataAsset skeletonDataAsset) {
				ForEachAnimationReferenceAsset(skeletonDataAsset, (referenceAsset) => referenceAsset.Initialize());
			}

			private static void ForEachAnimationReferenceAsset (SkeletonDataAsset skeletonDataAsset,
				System.Action<AnimationReferenceAsset> func) {

				string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AnimationReferenceAsset");
				foreach (string guid in guids) {
					string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
					if (!string.IsNullOrEmpty(path)) {
						var referenceAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationReferenceAsset>(path);
						if (referenceAsset.SkeletonDataAsset == skeletonDataAsset)
							func(referenceAsset);
					}
				}
			}
		}
	}
}
