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
#pragma warning disable 0618 // for 3.7 branch only. Avoids "PreferenceItem' is obsolete: '[PreferenceItem] is deprecated. Use [SettingsProvider] instead."

// Original contribution by: Mitch Thompson

#define SPINE_SKELETONMECANIM

#if UNITY_2017_2_OR_NEWER
#define NEWPLAYMODECALLBACKS
#endif

#if UNITY_2018 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEWHIERARCHYWINDOWCALLBACKS
#endif

#if UNITY_2018_3_OR_NEWER
#define NEW_PREFERENCES_SETTINGS_PROVIDER
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
	using EventType = UnityEngine.EventType;

	// Analysis disable once ConvertToStaticType
	[InitializeOnLoad]
	public partial class SpineEditorUtilities : AssetPostprocessor {

		public static string editorPath = "";
		public static string editorGUIPath = "";
		public static bool initialized;
	
		// Auto-import entry point
		static void OnPostprocessAllAssets (string[] imported, string[] deleted, string[] moved, string[] movedFromAssetPaths) {
			if (imported.Length == 0)
				return;

			AssetUtility.HandleOnPostprocessAllAssets(imported);
		}

#region Initialization
		static SpineEditorUtilities () {
			Initialize();
		}

		static void Initialize () {
			if (EditorApplication.isPlayingOrWillChangePlaymode) return;
			
			#if !NEW_PREFERENCES_SETTINGS_PROVIDER
			Preferences.Load();
			#else
			SpinePreferences.Load();
			#endif

			string[] assets = AssetDatabase.FindAssets("t:script SpineEditorUtilities");
			string assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
			editorPath = Path.GetDirectoryName(assetPath).Replace("\\", "/");

			assets = AssetDatabase.FindAssets("t:texture icon-subMeshRenderer");
			if (assets.Length > 0) {
				assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
				editorGUIPath = Path.GetDirectoryName(assetPath).Replace("\\", "/");
			}
			else {
				editorGUIPath = editorPath.Replace("/Utility", "/GUI");
			}
			Icons.Initialize();

			// Drag and Drop
		#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= DragAndDropInstantiation.SceneViewDragAndDrop;
			SceneView.duringSceneGui += DragAndDropInstantiation.SceneViewDragAndDrop;
		#else
			SceneView.onSceneGUIDelegate -= DragAndDropInstantiation.SceneViewDragAndDrop;
			SceneView.onSceneGUIDelegate += DragAndDropInstantiation.SceneViewDragAndDrop;
		#endif

			EditorApplication.hierarchyWindowItemOnGUI -= HierarchyHandler.HandleDragAndDrop;
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyHandler.HandleDragAndDrop;

			// Hierarchy Icons
			#if NEWPLAYMODECALLBACKS
			EditorApplication.playModeStateChanged -= HierarchyHandler.IconsOnPlaymodeStateChanged;
			EditorApplication.playModeStateChanged += HierarchyHandler.IconsOnPlaymodeStateChanged;
			HierarchyHandler.IconsOnPlaymodeStateChanged(PlayModeStateChange.EnteredEditMode);
			#else
			EditorApplication.playmodeStateChanged -= HierarchyHandler.IconsOnPlaymodeStateChanged;
			EditorApplication.playmodeStateChanged += HierarchyHandler.IconsOnPlaymodeStateChanged;
			HierarchyHandler.IconsOnPlaymodeStateChanged();
			#endif

			// Data Refresh Edit Mode.
			// This prevents deserialized SkeletonData from persisting from play mode to edit mode.
			#if NEWPLAYMODECALLBACKS
			EditorApplication.playModeStateChanged -= DataReloadHandler.OnPlaymodeStateChanged;
			EditorApplication.playModeStateChanged += DataReloadHandler.OnPlaymodeStateChanged;
			DataReloadHandler.OnPlaymodeStateChanged(PlayModeStateChange.EnteredEditMode);
			#else
			EditorApplication.playmodeStateChanged -= DataReloadHandler.OnPlaymodeStateChanged;
			EditorApplication.playmodeStateChanged += DataReloadHandler.OnPlaymodeStateChanged;
			DataReloadHandler.OnPlaymodeStateChanged();
			#endif

			if (SpineEditorUtilities.Preferences.textureImporterWarning) {
				IssueWarningsForUnrecommendedTextureSettings();
			}

			initialized = true;
		}

		public static void ConfirmInitialization () {
			if (!initialized || Icons.skeleton == null)
				Initialize();
		}

		public static void IssueWarningsForUnrecommendedTextureSettings() {

			string[] atlasDescriptionGUIDs = AssetDatabase.FindAssets("t:textasset .atlas"); // Note: finds .atlas.txt files
			for (int i = 0; i < atlasDescriptionGUIDs.Length; ++i) {
				string atlasDescriptionPath = AssetDatabase.GUIDToAssetPath(atlasDescriptionGUIDs[i]);
				string texturePath = atlasDescriptionPath.Replace(".atlas.txt", ".png");

				bool textureExists = IssueWarningsForUnrecommendedTextureSettings(texturePath);
				if (!textureExists) {
					texturePath = texturePath.Replace(".png", ".jpg");
					textureExists = IssueWarningsForUnrecommendedTextureSettings(texturePath);
				}
				if (!textureExists) {
					continue;
				}
			}
		}

		public static bool IssueWarningsForUnrecommendedTextureSettings(string texturePath)
		{
			TextureImporter texImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
			if (texImporter == null) {
				return false;
			}

			int extensionPos = texturePath.LastIndexOf('.');
			string materialPath = texturePath.Substring(0, extensionPos) + "_Material.mat";
			Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

			if (material == null)
				return true;

			string errorMessage = null;
			if (MaterialChecks.IsTextureSetupProblematic(material, PlayerSettings.colorSpace,
				texImporter. sRGBTexture, texImporter. mipmapEnabled, texImporter. alphaIsTransparency,
				texturePath, materialPath, ref errorMessage)) {
				Debug.LogWarning(errorMessage);
			}
			return true;
		}
		#endregion

		public static class HierarchyHandler {
			static Dictionary<int, GameObject> skeletonRendererTable = new Dictionary<int, GameObject>();
			static Dictionary<int, SkeletonUtilityBone> skeletonUtilityBoneTable = new Dictionary<int, SkeletonUtilityBone>();
			static Dictionary<int, BoundingBoxFollower> boundingBoxFollowerTable = new Dictionary<int, BoundingBoxFollower>();

#if NEWPLAYMODECALLBACKS
			internal static void IconsOnPlaymodeStateChanged (PlayModeStateChange stateChange) {
#else
			internal static void IconsOnPlaymodeStateChanged () {
#endif
				skeletonRendererTable.Clear();
				skeletonUtilityBoneTable.Clear();
				boundingBoxFollowerTable.Clear();

#if NEWHIERARCHYWINDOWCALLBACKS
				EditorApplication.hierarchyChanged -= IconsOnChanged;
#else
				EditorApplication.hierarchyWindowChanged -= IconsOnChanged;
#endif
				EditorApplication.hierarchyWindowItemOnGUI -= IconsOnGUI;

				if (!Application.isPlaying && Preferences.showHierarchyIcons) {
#if NEWHIERARCHYWINDOWCALLBACKS
					EditorApplication.hierarchyChanged += IconsOnChanged;
#else
					EditorApplication.hierarchyWindowChanged += IconsOnChanged;
#endif
					EditorApplication.hierarchyWindowItemOnGUI += IconsOnGUI;
					IconsOnChanged();
				}
			}

			internal static void IconsOnChanged () {
				skeletonRendererTable.Clear();
				skeletonUtilityBoneTable.Clear();
				boundingBoxFollowerTable.Clear();

				SkeletonRenderer[] arr = Object.FindObjectsOfType<SkeletonRenderer>();
				foreach (SkeletonRenderer r in arr)
					skeletonRendererTable[r.gameObject.GetInstanceID()] = r.gameObject;

				SkeletonUtilityBone[] boneArr = Object.FindObjectsOfType<SkeletonUtilityBone>();
				foreach (SkeletonUtilityBone b in boneArr)
					skeletonUtilityBoneTable[b.gameObject.GetInstanceID()] = b;

				BoundingBoxFollower[] bbfArr = Object.FindObjectsOfType<BoundingBoxFollower>();
				foreach (BoundingBoxFollower bbf in bbfArr)
					boundingBoxFollowerTable[bbf.gameObject.GetInstanceID()] = bbf;
			}

			internal static void IconsOnGUI (int instanceId, Rect selectionRect) {
				Rect r = new Rect(selectionRect);
				if (skeletonRendererTable.ContainsKey(instanceId)) {
					r.x = r.width - 15;
					r.width = 15;
					GUI.Label(r, Icons.spine);
				} else if (skeletonUtilityBoneTable.ContainsKey(instanceId)) {
					r.x -= 26;
					if (skeletonUtilityBoneTable[instanceId] != null) {
						if (skeletonUtilityBoneTable[instanceId].transform.childCount == 0)
							r.x += 13;
						r.y += 2;
						r.width = 13;
						r.height = 13;
						if (skeletonUtilityBoneTable[instanceId].mode == SkeletonUtilityBone.Mode.Follow)
							GUI.DrawTexture(r, Icons.bone);
						else
							GUI.DrawTexture(r, Icons.poseBones);
					}
				} else if (boundingBoxFollowerTable.ContainsKey(instanceId)) {
					r.x -= 26;
					if (boundingBoxFollowerTable[instanceId] != null) {
						if (boundingBoxFollowerTable[instanceId].transform.childCount == 0)
							r.x += 13;
						r.y += 2;
						r.width = 13;
						r.height = 13;
						GUI.DrawTexture(r, Icons.boundingBox);
					}
				}
			}

			internal static void HandleDragAndDrop (int instanceId, Rect selectionRect) {
				// HACK: Uses EditorApplication.hierarchyWindowItemOnGUI.
				// Only works when there is at least one item in the scene.
				var current = UnityEngine.Event.current;
				var eventType = current.type;
				bool isDraggingEvent = eventType == EventType.DragUpdated;
				bool isDropEvent = eventType == EventType.DragPerform;
				if (isDraggingEvent || isDropEvent) {
					var mouseOverWindow = EditorWindow.mouseOverWindow;
					if (mouseOverWindow != null) {

						// One, existing, valid SkeletonDataAsset
						var references = UnityEditor.DragAndDrop.objectReferences;
						if (references.Length == 1) {
							var skeletonDataAsset = references[0] as SkeletonDataAsset;
							if (skeletonDataAsset != null && skeletonDataAsset.GetSkeletonData(true) != null) {

								// Allow drag-and-dropping anywhere in the Hierarchy Window.
								// HACK: string-compare because we can't get its type via reflection.
								const string HierarchyWindow = "UnityEditor.SceneHierarchyWindow";
								if (HierarchyWindow.Equals(mouseOverWindow.GetType().ToString(), System.StringComparison.Ordinal)) {
									if (isDraggingEvent) {
										UnityEditor.DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
										current.Use();
									} else if (isDropEvent) {
										DragAndDropInstantiation.ShowInstantiateContextMenu(skeletonDataAsset, Vector3.zero);
										UnityEditor.DragAndDrop.AcceptDrag();
										current.Use();
										return;
									}
								}

							}
						}
					}
				}

			}
		}
	}

	public class TextureModificationWarningProcessor : UnityEditor.AssetModificationProcessor
	{
		static string[] OnWillSaveAssets(string[] paths)
		{
			if (SpineEditorUtilities.Preferences.textureImporterWarning) {
				foreach (string path in paths) {
					if (path.EndsWith(".png.meta", System.StringComparison.Ordinal) ||
						path.EndsWith(".jpg.meta", System.StringComparison.Ordinal)) {

						string texturePath = System.IO.Path.ChangeExtension(path, null); // .meta removed
						string atlasPath = System.IO.Path.ChangeExtension(texturePath, "atlas.txt");
						if (System.IO.File.Exists(atlasPath))
							SpineEditorUtilities.IssueWarningsForUnrecommendedTextureSettings(texturePath);
					}
				}
			}
			return paths;
		}
	}
}
