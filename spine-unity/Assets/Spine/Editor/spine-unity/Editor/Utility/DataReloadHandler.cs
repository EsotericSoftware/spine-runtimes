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

#if UNITY_2020_2_OR_NEWER
#define HAS_OBJECT_CHANGE_EVENTS
#endif

#if UNITY_2022_2_OR_NEWER
#define USE_FIND_OBJECTS_BY_TYPE
#endif

#if UNITY_6000_3_OR_NEWER
#define USES_ENTITY_ID
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

#if USES_ENTITY_ID
			internal static Dictionary<EntityId, string> savedSkeletonDataAssetAtSKeletonGraphicID = new Dictionary<EntityId, string>();
#else
			internal static Dictionary<int, string> savedSkeletonDataAssetAtSKeletonGraphicID = new Dictionary<int, string>();
#endif

#if NEWPLAYMODECALLBACKS
			internal static void OnPlaymodeStateChanged (PlayModeStateChange stateChange) {
#else
			internal static void OnPlaymodeStateChanged () {
#endif
				ReloadAllActiveSkeletonsEditMode();
			}

			public static void ReloadAllActiveSkeletonsEditMode () {
				if (!Preferences.reloadAfterPlayMode) return;
				if (EditorApplication.isPaused) return;
				if (EditorApplication.isPlaying) return;
				if (EditorApplication.isCompiling) return;
				if (EditorApplication.isPlayingOrWillChangePlaymode) return;

				HashSet<SkeletonDataAsset> skeletonDataAssetsToReload = new HashSet<SkeletonDataAsset>();

#if USE_FIND_OBJECTS_BY_TYPE
				SkeletonRenderer[] activeSkeletonRenderers = GameObject.FindObjectsByType<SkeletonRenderer>(FindObjectsSortMode.None);
#else
				SkeletonRenderer[] activeSkeletonRenderers = GameObject.FindObjectsOfType<SkeletonRenderer>();
#endif
				foreach (SkeletonRenderer sr in activeSkeletonRenderers) {
					SkeletonDataAsset skeletonDataAsset = sr.skeletonDataAsset;
					if (skeletonDataAsset != null) skeletonDataAssetsToReload.Add(skeletonDataAsset);
				}

				// Under some circumstances (e.g. on first import) SkeletonGraphic objects
				// have their skeletonGraphic.skeletonDataAsset reference corrupted
				// by the instance of the ScriptableObject being destroyed but still assigned.
				// Here we save the skeletonGraphic.skeletonDataAsset asset path in order
				// to restore it later.
#if USE_FIND_OBJECTS_BY_TYPE
				SkeletonGraphic[] activeSkeletonGraphics = GameObject.FindObjectsByType<SkeletonGraphic>(FindObjectsSortMode.None);
#else
				SkeletonGraphic[] activeSkeletonGraphics = GameObject.FindObjectsOfType<SkeletonGraphic>();
#endif
				foreach (SkeletonGraphic skeletonGraphic in activeSkeletonGraphics) {
					SkeletonDataAsset skeletonDataAsset = skeletonGraphic.skeletonDataAsset;
					if (skeletonDataAsset != null) {
						string assetPath = AssetDatabase.GetAssetPath(skeletonDataAsset);
#if USES_ENTITY_ID
						EntityId sgID = skeletonGraphic.GetEntityId();
#else
						int sgID = skeletonGraphic.GetInstanceID();
#endif
						savedSkeletonDataAssetAtSKeletonGraphicID[sgID] = assetPath;
						skeletonDataAssetsToReload.Add(skeletonDataAsset);
					}
				}

				foreach (SkeletonDataAsset skeletonDataAsset in skeletonDataAssetsToReload) {
					ReloadSkeletonDataAsset(skeletonDataAsset, false);
				}

				foreach (SkeletonRenderer skeletonRenderer in activeSkeletonRenderers)
					skeletonRenderer.Initialize(true);
				foreach (SkeletonGraphic skeletonGraphic in activeSkeletonGraphics)
					skeletonGraphic.Initialize(true);
			}

			public static void ReloadSceneSkeletonComponents (SkeletonDataAsset skeletonDataAsset) {
				if (EditorApplication.isPaused) return;
				if (EditorApplication.isPlaying) return;
				if (EditorApplication.isCompiling) return;
				if (EditorApplication.isPlayingOrWillChangePlaymode) return;

#if USE_FIND_OBJECTS_BY_TYPE
				SkeletonRenderer[] activeSkeletonRenderers = GameObject.FindObjectsByType<SkeletonRenderer>(FindObjectsSortMode.None);
#else
				SkeletonRenderer[] activeSkeletonRenderers = GameObject.FindObjectsOfType<SkeletonRenderer>();
#endif
				foreach (SkeletonRenderer renderer in activeSkeletonRenderers) {
					if (renderer.isActiveAndEnabled && renderer.skeletonDataAsset == skeletonDataAsset) renderer.Initialize(true);
				}

#if USE_FIND_OBJECTS_BY_TYPE
				SkeletonGraphic[] activeSkeletonGraphics = GameObject.FindObjectsByType<SkeletonGraphic>(FindObjectsSortMode.None);
#else
				SkeletonGraphic[] activeSkeletonGraphics = GameObject.FindObjectsOfType<SkeletonGraphic>();
#endif
				foreach (SkeletonGraphic graphic in activeSkeletonGraphics) {
					if (graphic.isActiveAndEnabled && graphic.skeletonDataAsset == skeletonDataAsset)
						graphic.Initialize(true);
				}
			}

			/// <summary>Reloads the SkeletonDataAsset if loaded, then its scene components if enabled in the
			/// Spine preferences, and refreshes Inspectors showing it.</summary>
			public static void ReloadSkeletonDataAssetAndSceneComponents (SkeletonDataAsset skeletonDataAsset) {
				if (!skeletonDataAsset.IsLoaded) return;
				ReloadSkeletonDataAsset(skeletonDataAsset, false);
				if (Preferences.autoReloadSceneSkeletons && skeletonDataAsset.IsLoaded)
					ReloadSceneSkeletonComponents(skeletonDataAsset);
				foreach (SkeletonDataAssetInspector inspector in Resources.FindObjectsOfTypeAll<SkeletonDataAssetInspector>()) {
					if (inspector.target == skeletonDataAsset)
						inspector.UpdateSkeletonData();
				}
			}

#if HAS_OBJECT_CHANGE_EVENTS
			// Only supported on 2020.2 and newer. Best solution as it's independent of whether the Inspector
			// is closed or inspecting something else while Undo / Redo is performed and
			// ObjectChangeEvents.changesPublished runs on the main thread.
			internal static void OnUndoRedoPerformed () {
				ObjectChangeEvents.changesPublished -= ReloadChangedSkeletonDataAssets;
				ObjectChangeEvents.changesPublished += ReloadChangedSkeletonDataAssets;
			}

			static void ReloadChangedSkeletonDataAssets (ref ObjectChangeEventStream stream) {
				ObjectChangeEvents.changesPublished -= ReloadChangedSkeletonDataAssets;
				// In play mode, SkeletonDataAsset.OnValidate updates mix settings in-place.
				if (EditorApplication.isPlayingOrWillChangePlaymode) return;

				HashSet<SkeletonDataAsset> skeletonDataAssets = new HashSet<SkeletonDataAsset>();
				for (int i = 0; i < stream.length; ++i) {
					if (stream.GetEventType(i) != ObjectChangeKind.ChangeAssetObjectProperties) continue;
					ChangeAssetObjectPropertiesEventArgs change;
					stream.GetChangeAssetObjectPropertiesEvent(i, out change);
#if USES_ENTITY_ID
					SkeletonDataAsset skeletonDataAsset = EditorUtility.EntityIdToObject(change.instanceId) as SkeletonDataAsset;
#else
					SkeletonDataAsset skeletonDataAsset = EditorUtility.InstanceIDToObject(change.instanceId) as SkeletonDataAsset;
#endif
					if (skeletonDataAsset != null) skeletonDataAssets.Add(skeletonDataAsset);
				}
				foreach (SkeletonDataAsset skeletonDataAsset in skeletonDataAssets)
					ReloadSkeletonDataAssetAndSceneComponents(skeletonDataAsset);
			}
#endif

			public static void ClearAnimationReferenceAssets (SkeletonDataAsset skeletonDataAsset) {
				ForEachAnimationReferenceAsset(skeletonDataAsset, (referenceAsset) => referenceAsset.Clear());
			}

			public static void ReloadAnimationReferenceAssets (SkeletonDataAsset skeletonDataAsset) {
				ForEachAnimationReferenceAsset(skeletonDataAsset, (referenceAsset) => referenceAsset.Initialize());
			}

			public static void ClearEventDataReferenceAssets (SkeletonDataAsset skeletonDataAsset) {
				// Only loaded reference assets hold cached EventData, others look it up once loaded.
				foreach (EventDataReferenceAsset referenceAsset in Resources.FindObjectsOfTypeAll<EventDataReferenceAsset>()) {
					if (referenceAsset.SkeletonDataAsset == skeletonDataAsset)
						referenceAsset.Clear();
				}
			}

			private static void ForEachAnimationReferenceAsset (SkeletonDataAsset skeletonDataAsset,
				System.Action<AnimationReferenceAsset> func) {

				string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AnimationReferenceAsset");
				foreach (string guid in guids) {
					string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
					if (string.IsNullOrEmpty(path)) continue;
					UnityEngine.Object[] allAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
					foreach (UnityEngine.Object obj in allAssets) {
						AnimationReferenceAsset referenceAsset = obj as AnimationReferenceAsset;
						if (referenceAsset != null && referenceAsset.SkeletonDataAsset == skeletonDataAsset)
							func(referenceAsset);
					}
				}
			}
		}
	}
}
