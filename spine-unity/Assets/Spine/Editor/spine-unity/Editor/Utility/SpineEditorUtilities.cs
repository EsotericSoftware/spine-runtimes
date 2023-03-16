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

#if UNITY_2017_1_OR_NEWER
#define BUILT_IN_SPRITE_MASK_COMPONENT
#endif

#if UNITY_2020_2_OR_NEWER
#define HAS_ON_POSTPROCESS_PREFAB
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
	using EventType = UnityEngine.EventType;

	// Analysis disable once ConvertToStaticType
	[InitializeOnLoad]
	public partial class SpineEditorUtilities : AssetPostprocessor {

		public static string editorPath = "";
		public static string editorGUIPath = "";
		public static bool initialized;
		private static List<string> texturesWithoutMetaFile = new List<string>();

		// Auto-import entry point for textures
		void OnPreprocessTexture () {
#if UNITY_2018_1_OR_NEWER
			bool customTextureSettingsExist = !assetImporter.importSettingsMissing;
#else
			bool customTextureSettingsExist = System.IO.File.Exists(assetImporter.assetPath + ".meta");
#endif
			if (!customTextureSettingsExist) {
				texturesWithoutMetaFile.Add(assetImporter.assetPath);
			}
		}

		// Auto-import post process entry point for all assets
		static void OnPostprocessAllAssets (string[] imported, string[] deleted, string[] moved, string[] movedFromAssetPaths) {
			if (imported.Length == 0)
				return;

			// we copy the list here to prevent nested calls to OnPostprocessAllAssets() triggering a Clear() of the list
			// in the middle of execution.
			List<string> texturesWithoutMetaFileCopy = new List<string>(texturesWithoutMetaFile);
			AssetUtility.HandleOnPostprocessAllAssets(imported, texturesWithoutMetaFileCopy);
			texturesWithoutMetaFile.Clear();
		}

#if HAS_ON_POSTPROCESS_PREFAB
		// Post process prefabs for setting the MeshFilter to not cause constant Prefab override changes.
		void OnPostprocessPrefab (GameObject g) {
			if (SpineBuildProcessor.isBuilding)
				return;

			SetupSpinePrefabMesh(g, context);
		}

		public static bool SetupSpinePrefabMesh (GameObject g, UnityEditor.AssetImporters.AssetImportContext context) {
			Dictionary<string, int> nameUsageCount = new Dictionary<string, int>();
			bool wasModified = false;
			SkeletonRenderer[] skeletonRenderers = g.GetComponentsInChildren<SkeletonRenderer>(true);
			foreach (SkeletonRenderer renderer in skeletonRenderers) {
				wasModified = true;
				MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
				if (meshFilter == null)
					meshFilter = renderer.gameObject.AddComponent<MeshFilter>();

				renderer.EditorUpdateMeshFilterHideFlags();
				renderer.Initialize(true, true);
				renderer.LateUpdateMesh();
				Mesh mesh = meshFilter.sharedMesh;
				if (mesh == null) continue;

				string meshName = string.Format("Skeleton Prefab Mesh \"{0}\"", renderer.name);
				if (nameUsageCount.ContainsKey(meshName)) {
					nameUsageCount[meshName]++;
					meshName = string.Format("Skeleton Prefab Mesh \"{0} ({1})\"", renderer.name, nameUsageCount[meshName]);
				} else {
					nameUsageCount.Add(meshName, 0);
				}
				mesh.name = meshName;
				mesh.hideFlags = HideFlags.None;
				if (context != null)
					context.AddObjectToAsset(meshFilter.sharedMesh.name, meshFilter.sharedMesh);
			}
			return wasModified;
		}

		public static bool CleanupSpinePrefabMesh (GameObject g) {
			bool wasModified = false;
			SkeletonRenderer[] skeletonRenderers = g.GetComponentsInChildren<SkeletonRenderer>(true);
			foreach (SkeletonRenderer renderer in skeletonRenderers) {
				MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
				if (meshFilter != null) {
					if (meshFilter.sharedMesh) {
						wasModified = true;
						meshFilter.sharedMesh = null;
						meshFilter.hideFlags = HideFlags.None;
					}
				}
			}
			return wasModified;
		}
#endif

		#region Initialization
		static SpineEditorUtilities () {
			EditorApplication.delayCall += Initialize; // delayed so that AssetDatabase is ready.
		}

		static void Initialize () {
			// Note: Preferences need to be loaded when changing play mode
			// to initialize handle scale correctly.
#if !NEW_PREFERENCES_SETTINGS_PROVIDER
			Preferences.Load();
#else
			SpinePreferences.Load();
#endif

			if (EditorApplication.isPlayingOrWillChangePlaymode) return;

			string[] folders = { "Assets", "Packages" };
			string[] assets;
			string assetPath;
			assets = AssetDatabase.FindAssets("t:texture icon-subMeshRenderer", folders);
			if (assets.Length > 0) {
				assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
				editorGUIPath = Path.GetDirectoryName(assetPath).Replace('\\', '/');
			}
			assets = AssetDatabase.FindAssets("t:script SpineEditorUtilities", folders);
			if (assets.Length > 0) {
				assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
				editorPath = Path.GetDirectoryName(assetPath).Replace('\\', '/');
				if (string.IsNullOrEmpty(editorGUIPath))
					editorGUIPath = editorPath.Replace("/Utility", "/GUI");
			}
			if (string.IsNullOrEmpty(editorGUIPath))
				return;
			Icons.Initialize();

			// Drag and Drop
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= DragAndDropInstantiation.SceneViewDragAndDrop;
			SceneView.duringSceneGui += DragAndDropInstantiation.SceneViewDragAndDrop;
#else
			SceneView.onSceneGUIDelegate -= DragAndDropInstantiation.SceneViewDragAndDrop;
			SceneView.onSceneGUIDelegate += DragAndDropInstantiation.SceneViewDragAndDrop;
#endif

#if UNITY_2021_2_OR_NEWER
			DragAndDrop.RemoveDropHandler(HierarchyHandler.HandleDragAndDrop);
			DragAndDrop.AddDropHandler(HierarchyHandler.HandleDragAndDrop);
#else
			EditorApplication.hierarchyWindowItemOnGUI -= HierarchyHandler.HandleDragAndDrop;
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyHandler.HandleDragAndDrop;
#endif
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

		public static void IssueWarningsForUnrecommendedTextureSettings () {

			string[] atlasDescriptionGUIDs = AssetDatabase.FindAssets("t:textasset .atlas"); // Note: finds ".atlas.txt" but also ".atlas 1.txt" files.
			for (int i = 0; i < atlasDescriptionGUIDs.Length; ++i) {
				string atlasDescriptionPath = AssetDatabase.GUIDToAssetPath(atlasDescriptionGUIDs[i]);
				if (!atlasDescriptionPath.EndsWith(".atlas.txt"))
					continue;

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

		public static void ReloadSkeletonDataAssetAndComponent (SkeletonRenderer component) {
			if (component == null) return;
			ReloadSkeletonDataAsset(component.skeletonDataAsset);
			ReinitializeComponent(component);
		}

		public static void ReloadSkeletonDataAssetAndComponent (SkeletonGraphic component) {
			if (component == null) return;
			ReloadSkeletonDataAsset(component.skeletonDataAsset);
			// Reinitialize.
			ReinitializeComponent(component);
		}

		public static void ClearSkeletonDataAsset (SkeletonDataAsset skeletonDataAsset) {
			skeletonDataAsset.Clear();
			DataReloadHandler.ClearAnimationReferenceAssets(skeletonDataAsset);
		}

		public static void ReloadSkeletonDataAsset (SkeletonDataAsset skeletonDataAsset, bool clearAtlasAssets = true) {
			if (skeletonDataAsset == null)
				return;

			if (clearAtlasAssets) {
				foreach (AtlasAssetBase aa in skeletonDataAsset.atlasAssets) {
					if (aa != null) aa.Clear();
				}
			}
			ClearSkeletonDataAsset(skeletonDataAsset);
			skeletonDataAsset.GetSkeletonData(true);
			DataReloadHandler.ReloadAnimationReferenceAssets(skeletonDataAsset);
		}

		public static void ReinitializeComponent (SkeletonRenderer component) {
			if (component == null) return;
			if (!SkeletonDataAssetIsValid(component.SkeletonDataAsset)) return;

			IAnimationStateComponent stateComponent = component as IAnimationStateComponent;
			AnimationState oldAnimationState = null;
			if (stateComponent != null) {
				oldAnimationState = stateComponent.AnimationState;
			}

			component.Initialize(true); // implicitly clears any subscribers

			if (oldAnimationState != null) {
				stateComponent.AnimationState.AssignEventSubscribersFrom(oldAnimationState);
			}

#if BUILT_IN_SPRITE_MASK_COMPONENT
			SpineMaskUtilities.EditorAssignSpriteMaskMaterials(component);
#endif
			component.LateUpdate();
		}

		public static void ReinitializeComponent (SkeletonGraphic component) {
			if (component == null) return;
			if (!SkeletonDataAssetIsValid(component.SkeletonDataAsset)) return;
			component.Initialize(true);
			component.LateUpdate();
		}

		public static bool SkeletonDataAssetIsValid (SkeletonDataAsset asset) {
			return asset != null && asset.GetSkeletonData(quiet: true) != null;
		}

		public static bool IssueWarningsForUnrecommendedTextureSettings (string texturePath) {
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
				texImporter.sRGBTexture, texImporter.mipmapEnabled, texImporter.alphaIsTransparency,
				texturePath, materialPath, ref errorMessage)) {
				Debug.LogWarning(errorMessage, material);
			}
			return true;
		}
		#endregion

		public static class HierarchyHandler {
			static Dictionary<int, GameObject> skeletonRendererTable = new Dictionary<int, GameObject>();
			static Dictionary<int, SkeletonUtilityBone> skeletonUtilityBoneTable = new Dictionary<int, SkeletonUtilityBone>();
			static Dictionary<int, BoundingBoxFollower> boundingBoxFollowerTable = new Dictionary<int, BoundingBoxFollower>();
			static Dictionary<int, BoundingBoxFollowerGraphic> boundingBoxFollowerGraphicTable = new Dictionary<int, BoundingBoxFollowerGraphic>();

#if NEWPLAYMODECALLBACKS
			internal static void IconsOnPlaymodeStateChanged (PlayModeStateChange stateChange) {
#else
			internal static void IconsOnPlaymodeStateChanged () {
#endif
				skeletonRendererTable.Clear();
				skeletonUtilityBoneTable.Clear();
				boundingBoxFollowerTable.Clear();
				boundingBoxFollowerGraphicTable.Clear();

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
				boundingBoxFollowerGraphicTable.Clear();

				SkeletonRenderer[] arr = Object.FindObjectsOfType<SkeletonRenderer>();
				foreach (SkeletonRenderer r in arr)
					skeletonRendererTable[r.gameObject.GetInstanceID()] = r.gameObject;

				SkeletonUtilityBone[] boneArr = Object.FindObjectsOfType<SkeletonUtilityBone>();
				foreach (SkeletonUtilityBone b in boneArr)
					skeletonUtilityBoneTable[b.gameObject.GetInstanceID()] = b;

				BoundingBoxFollower[] bbfArr = Object.FindObjectsOfType<BoundingBoxFollower>();
				foreach (BoundingBoxFollower bbf in bbfArr)
					boundingBoxFollowerTable[bbf.gameObject.GetInstanceID()] = bbf;

				BoundingBoxFollowerGraphic[] bbfgArr = Object.FindObjectsOfType<BoundingBoxFollowerGraphic>();
				foreach (BoundingBoxFollowerGraphic bbf in bbfgArr)
					boundingBoxFollowerGraphicTable[bbf.gameObject.GetInstanceID()] = bbf;
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
				} else if (boundingBoxFollowerGraphicTable.ContainsKey(instanceId)) {
					r.x -= 26;
					if (boundingBoxFollowerGraphicTable[instanceId] != null) {
						if (boundingBoxFollowerGraphicTable[instanceId].transform.childCount == 0)
							r.x += 13;
						r.y += 2;
						r.width = 13;
						r.height = 13;
						GUI.DrawTexture(r, Icons.boundingBox);
					}
				}
			}

#if UNITY_2021_2_OR_NEWER
			internal static DragAndDropVisualMode HandleDragAndDrop (int dropTargetInstanceID, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform) {
				SkeletonDataAsset skeletonDataAsset = DragAndDrop.objectReferences.Length == 0 ? null :
					DragAndDrop.objectReferences[0] as SkeletonDataAsset;
				if (skeletonDataAsset == null)
					return DragAndDropVisualMode.None;
				if (!perform)
					return DragAndDropVisualMode.Copy;

				GameObject dropTargetObject = UnityEditor.EditorUtility.InstanceIDToObject(dropTargetInstanceID) as GameObject;
				Transform dropTarget = dropTargetObject != null ? dropTargetObject.transform : null;
				Transform parent = dropTarget;
				int siblingIndex = 0;
				if (parent != null) {
					if (dropMode == HierarchyDropFlags.DropBetween) {
						parent = dropTarget.parent;
						siblingIndex = dropTarget ? dropTarget.GetSiblingIndex() + 1 : 0;
					} else if (dropMode == HierarchyDropFlags.DropAbove) {
						parent = dropTarget.parent;
						siblingIndex = dropTarget ? dropTarget.GetSiblingIndex() : 0;
					}
				}
				DragAndDropInstantiation.ShowInstantiateContextMenu(skeletonDataAsset, Vector3.zero, parent, siblingIndex);
				return DragAndDropVisualMode.Copy;
			}
#else
			internal static void HandleDragAndDrop (int instanceId, Rect selectionRect) {
				// HACK: Uses EditorApplication.hierarchyWindowItemOnGUI.
				// Only works when there is at least one item in the scene.
				UnityEngine.Event current = UnityEngine.Event.current;
				EventType eventType = current.type;
				bool isDraggingEvent = eventType == EventType.DragUpdated;
				bool isDropEvent = eventType == EventType.DragPerform;
				UnityEditor.DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				if (isDraggingEvent || isDropEvent) {
					EditorWindow mouseOverWindow = EditorWindow.mouseOverWindow;
					if (mouseOverWindow != null) {

						// One, existing, valid SkeletonDataAsset
						Object[] references = UnityEditor.DragAndDrop.objectReferences;
						if (references.Length == 1) {
							SkeletonDataAsset skeletonDataAsset = references[0] as SkeletonDataAsset;
							if (skeletonDataAsset != null && skeletonDataAsset.GetSkeletonData(true) != null) {

								// Allow drag-and-dropping anywhere in the Hierarchy Window.
								// HACK: string-compare because we can't get its type via reflection.
								const string HierarchyWindow = "UnityEditor.SceneHierarchyWindow";
								const string GenericDataTargetID = "target";
								if (HierarchyWindow.Equals(mouseOverWindow.GetType().ToString(), System.StringComparison.Ordinal)) {
									if (isDraggingEvent) {
										UnityEngine.Object mouseOverTarget = UnityEditor.EditorUtility.InstanceIDToObject(instanceId);
										if (mouseOverTarget)
											DragAndDrop.SetGenericData(GenericDataTargetID, mouseOverTarget);
										// Note: do not call current.Use(), otherwise we get the wrong drop-target parent.
									} else if (isDropEvent) {
										GameObject parentGameObject = DragAndDrop.GetGenericData(GenericDataTargetID) as UnityEngine.GameObject;
										Transform parent = parentGameObject != null ? parentGameObject.transform : null;
										// when dragging into empty space in hierarchy below last node, last node would be parent.
										if (IsLastNodeInHierarchy(parent))
											parent = null;
										DragAndDropInstantiation.ShowInstantiateContextMenu(skeletonDataAsset, Vector3.zero, parent, 0);
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

			internal static bool IsLastNodeInHierarchy (Transform node) {
				if (node == null)
					return false;

				while (node.parent != null) {
					if (node.GetSiblingIndex() != node.parent.childCount - 1)
						return false;
					node = node.parent;
				}

				GameObject[] rootNodes = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
				bool isLastNode = (rootNodes.Length > 0 && rootNodes[rootNodes.Length - 1].transform == node);
				return isLastNode;
			}
#endif
		}
	}

	public class TextureModificationWarningProcessor : UnityEditor.AssetModificationProcessor {
		static string[] OnWillSaveAssets (string[] paths) {
			if (SpineEditorUtilities.Preferences.textureImporterWarning) {
				foreach (string path in paths) {
					if ((path != null) &&
						(path.EndsWith(".png.meta", System.StringComparison.Ordinal) ||
						 path.EndsWith(".jpg.meta", System.StringComparison.Ordinal))) {

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
