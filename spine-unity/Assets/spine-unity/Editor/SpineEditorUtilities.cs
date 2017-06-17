/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#pragma warning disable 0219

// Original contribution by: Mitch Thompson

#define SPINE_SKELETONANIMATOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using Spine;

namespace Spine.Unity.Editor {
	using EventType = UnityEngine.EventType;
	
	// Analysis disable once ConvertToStaticType
	[InitializeOnLoad]
	public class SpineEditorUtilities : AssetPostprocessor {

		public static class Icons {
			public static Texture2D skeleton;
			public static Texture2D nullBone;
			public static Texture2D bone;
			public static Texture2D poseBones;
			public static Texture2D boneNib;
			public static Texture2D slot;
			public static Texture2D slotRoot;
			public static Texture2D skinPlaceholder;
			public static Texture2D image;
			public static Texture2D genericAttachment;
			public static Texture2D boundingBox;
			public static Texture2D mesh;
			public static Texture2D weights;
			public static Texture2D path;
			public static Texture2D clipping;
			public static Texture2D skin;
			public static Texture2D skinsRoot;
			public static Texture2D animation;
			public static Texture2D animationRoot;
			public static Texture2D spine;
			public static Texture2D userEvent;
			public static Texture2D constraintNib;
			public static Texture2D constraintRoot;
			public static Texture2D constraintTransform;
			public static Texture2D constraintPath;
			public static Texture2D constraintIK;
			public static Texture2D warning;
			public static Texture2D skeletonUtility;
			public static Texture2D hingeChain;
			public static Texture2D subMeshRenderer;

			public static Texture2D info;

			public static Texture2D unity;
//			public static Texture2D controllerIcon;

			static Texture2D LoadIcon (string filename) {
				return (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/" + filename);
			}

			public static void Initialize () {
				skeleton = LoadIcon("icon-skeleton.png");
				nullBone = LoadIcon("icon-null.png");
				bone = LoadIcon("icon-bone.png");
				poseBones = LoadIcon("icon-poseBones.png");
				boneNib = LoadIcon("icon-boneNib.png");
				slot = LoadIcon("icon-slot.png");
				slotRoot = LoadIcon("icon-slotRoot.png");
				skinPlaceholder = LoadIcon("icon-skinPlaceholder.png");

				genericAttachment = LoadIcon("icon-attachment.png");
				image = LoadIcon("icon-image.png");
				boundingBox = LoadIcon("icon-boundingBox.png");
				mesh = LoadIcon("icon-mesh.png");
				weights = LoadIcon("icon-weights.png");
				path = LoadIcon("icon-path.png");
				clipping = LoadIcon("icon-clipping.png");

				skin = LoadIcon("icon-skin.png");
				skinsRoot = LoadIcon("icon-skinsRoot.png");
				animation = LoadIcon("icon-animation.png");
				animationRoot = LoadIcon("icon-animationRoot.png");
				spine = LoadIcon("icon-spine.png");
				userEvent = LoadIcon("icon-event.png");
				constraintNib = LoadIcon("icon-constraintNib.png");

				constraintRoot = LoadIcon("icon-constraints.png");
				constraintTransform = LoadIcon("icon-constraintTransform.png");
				constraintPath = LoadIcon("icon-constraintPath.png");
				constraintIK = LoadIcon("icon-constraintIK.png");

				warning = LoadIcon("icon-warning.png");
				skeletonUtility = LoadIcon("icon-skeletonUtility.png");
				hingeChain = LoadIcon("icon-hingeChain.png");
				subMeshRenderer = LoadIcon("icon-subMeshRenderer.png");


				info = EditorGUIUtility.FindTexture("console.infoicon.sml");
				unity = EditorGUIUtility.FindTexture("SceneAsset Icon");
//				controllerIcon = EditorGUIUtility.FindTexture("AnimatorController Icon");
			}

			public static Texture2D GetAttachmentIcon (Attachment attachment) {
				// Analysis disable once CanBeReplacedWithTryCastAndCheckForNull
				if (attachment is RegionAttachment)
					return Icons.image;
				else if (attachment is MeshAttachment)
					return ((MeshAttachment)attachment).IsWeighted() ? Icons.weights : Icons.mesh;
				else if (attachment is BoundingBoxAttachment)
					return Icons.boundingBox;
				else if (attachment is PathAttachment)
					return Icons.path;
				else if (attachment is ClippingAttachment)
					return Icons.clipping;
				else
					return Icons.warning;
			}
		}

		public static string editorPath = "";
		public static string editorGUIPath = "";
		public static bool initialized;

		/// This list keeps the asset reference temporarily during importing.
		/// 
		/// In cases of very large projects/sufficient RAM pressure, when AssetDatabase.SaveAssets is called,
		/// Unity can mistakenly unload assets whose references are only on the stack.
		/// This leads to MissingReferenceException and other errors.
		static readonly List<ScriptableObject> protectFromStackGarbageCollection = new List<ScriptableObject>();
		static HashSet<string> assetsImportedInWrongState;
		static Dictionary<int, GameObject> skeletonRendererTable;
		static Dictionary<int, SkeletonUtilityBone> skeletonUtilityBoneTable;
		static Dictionary<int, BoundingBoxFollower> boundingBoxFollowerTable;

		#if SPINE_TK2D
		const float DEFAULT_DEFAULT_SCALE = 1f;
		#else
		const float DEFAULT_DEFAULT_SCALE = 0.01f;
		#endif
		const string DEFAULT_SCALE_KEY = "SPINE_DEFAULT_SCALE";
		public static float defaultScale = DEFAULT_DEFAULT_SCALE;

		const float DEFAULT_DEFAULT_MIX = 0.2f;
		const string DEFAULT_MIX_KEY = "SPINE_DEFAULT_MIX";
		public static float defaultMix = DEFAULT_DEFAULT_MIX;

		const string DEFAULT_DEFAULT_SHADER = "Spine/Skeleton";
		const string DEFAULT_SHADER_KEY = "SPINE_DEFAULT_SHADER";
		public static string defaultShader = DEFAULT_DEFAULT_SHADER;

		const float DEFAULT_DEFAULT_ZSPACING = 0f;
		const string DEFAULT_ZSPACING_KEY = "SPINE_DEFAULT_ZSPACING";
		public static float defaultZSpacing = DEFAULT_DEFAULT_ZSPACING;

		const bool DEFAULT_SHOW_HIERARCHY_ICONS = true;
		const string SHOW_HIERARCHY_ICONS_KEY = "SPINE_SHOW_HIERARCHY_ICONS";
		public static bool showHierarchyIcons = DEFAULT_SHOW_HIERARCHY_ICONS;

		public const float DEFAULT_SCENE_ICONS_SCALE = 1f;
		public const string SCENE_ICONS_SCALE_KEY = "SPINE_SCENE_ICONS_SCALE";

		#region Initialization
		static SpineEditorUtilities () {
			Initialize();
		}

		static void LoadPreferences () {
			defaultMix = EditorPrefs.GetFloat(DEFAULT_MIX_KEY, DEFAULT_DEFAULT_MIX);
			defaultScale = EditorPrefs.GetFloat(DEFAULT_SCALE_KEY, DEFAULT_DEFAULT_SCALE);
			defaultZSpacing = EditorPrefs.GetFloat(DEFAULT_ZSPACING_KEY, DEFAULT_DEFAULT_ZSPACING);
			defaultShader = EditorPrefs.GetString(DEFAULT_SHADER_KEY, DEFAULT_DEFAULT_SHADER);	
			showHierarchyIcons = EditorPrefs.GetBool(SHOW_HIERARCHY_ICONS_KEY, DEFAULT_SHOW_HIERARCHY_ICONS);
			SpineHandles.handleScale = EditorPrefs.GetFloat(SCENE_ICONS_SCALE_KEY, DEFAULT_SCENE_ICONS_SCALE);
			preferencesLoaded = true;
		}

		static void Initialize () {
			LoadPreferences();

			DirectoryInfo rootDir = new DirectoryInfo(Application.dataPath);
			FileInfo[] files = rootDir.GetFiles("SpineEditorUtilities.cs", SearchOption.AllDirectories);
			editorPath = Path.GetDirectoryName(files[0].FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets"));
			editorGUIPath = editorPath + "/GUI";

			Icons.Initialize();

			assetsImportedInWrongState = new HashSet<string>();
			skeletonRendererTable = new Dictionary<int, GameObject>();
			skeletonUtilityBoneTable = new Dictionary<int, SkeletonUtilityBone>();
			boundingBoxFollowerTable = new Dictionary<int, BoundingBoxFollower>();

			// Drag and Drop
			SceneView.onSceneGUIDelegate -= SceneViewDragAndDrop;
			SceneView.onSceneGUIDelegate += SceneViewDragAndDrop;
			EditorApplication.hierarchyWindowItemOnGUI -= HierarchyDragAndDrop;
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyDragAndDrop;

			// Hierarchy Icons
			EditorApplication.hierarchyWindowChanged -= HierarchyIconsOnChanged;
			EditorApplication.hierarchyWindowChanged += HierarchyIconsOnChanged;
			EditorApplication.hierarchyWindowItemOnGUI -= HierarchyIconsOnGUI;
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyIconsOnGUI;

			HierarchyIconsOnChanged();
			initialized = true;
		}

		public static void ConfirmInitialization () {
			if (!initialized || Icons.skeleton == null)
				Initialize();
		}
		#endregion

		#region Spine Preferences and Defaults
		static bool preferencesLoaded = false;

		[PreferenceItem("Spine")]
		static void PreferencesGUI () {
			if (!preferencesLoaded)
				LoadPreferences();

			EditorGUI.BeginChangeCheck();
			showHierarchyIcons = EditorGUILayout.Toggle(new GUIContent("Show Hierarchy Icons", "Show relevant icons on GameObjects with Spine Components on them. Disable this if you have large, complex scenes."), showHierarchyIcons);
			if (EditorGUI.EndChangeCheck()) {
				EditorPrefs.SetBool(SHOW_HIERARCHY_ICONS_KEY, showHierarchyIcons);
				HierarchyIconsOnChanged();
			}

			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Auto-Import Settings", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();
			defaultMix = EditorGUILayout.FloatField("Default Mix", defaultMix);
			if (EditorGUI.EndChangeCheck()) 
				EditorPrefs.SetFloat(DEFAULT_MIX_KEY, defaultMix);

			EditorGUI.BeginChangeCheck();
			defaultScale = EditorGUILayout.FloatField("Default SkeletonData Scale", defaultScale);
			if (EditorGUI.EndChangeCheck())
				EditorPrefs.SetFloat(DEFAULT_SCALE_KEY, defaultScale);

			EditorGUI.BeginChangeCheck();
			var shader = (EditorGUILayout.ObjectField("Default Shader", Shader.Find(defaultShader), typeof(Shader), false) as Shader);
			defaultShader = shader != null ? shader.name : DEFAULT_DEFAULT_SHADER;
			if (EditorGUI.EndChangeCheck())
				EditorPrefs.SetString(DEFAULT_SHADER_KEY, defaultShader);
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Editor Instantiation", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			defaultZSpacing = EditorGUILayout.Slider("Default Slot Z-Spacing", defaultZSpacing, -0.1f, 0f);
			if (EditorGUI.EndChangeCheck())
				EditorPrefs.SetFloat(DEFAULT_ZSPACING_KEY, defaultZSpacing);
			

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Handles and Gizmos", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			SpineHandles.handleScale = EditorGUILayout.Slider("Editor Bone Scale", SpineHandles.handleScale, 0.01f, 2f);
			SpineHandles.handleScale = Mathf.Max(0.01f, SpineHandles.handleScale);
			if (EditorGUI.EndChangeCheck()) {
				EditorPrefs.SetFloat(SCENE_ICONS_SCALE_KEY, SpineHandles.handleScale);
				SceneView.RepaintAll();
			}
				
			
			GUILayout.Space(20);
			EditorGUILayout.LabelField("3rd Party Settings", EditorStyles.boldLabel);
			using (new GUILayout.HorizontalScope()) {
				EditorGUILayout.PrefixLabel("Define TK2D");
				if (GUILayout.Button("Enable", GUILayout.Width(64)))
					EnableTK2D();
				if (GUILayout.Button("Disable", GUILayout.Width(64)))
					DisableTK2D();
			}
		}
		#endregion

		#region Drag and Drop Instantiation

		public delegate Component InstantiateDelegate (SkeletonDataAsset skeletonDataAsset);

		struct SpawnMenuData {
			public Vector3 spawnPoint;
			public SkeletonDataAsset skeletonDataAsset;
			public InstantiateDelegate instantiateDelegate;
			public bool isUI;
		}

		public class SkeletonComponentSpawnType {
			public string menuLabel;
			public InstantiateDelegate instantiateDelegate;
			public bool isUI;
		}

		public static readonly List<SkeletonComponentSpawnType> additionalSpawnTypes = new List<SkeletonComponentSpawnType>();

		static void SceneViewDragAndDrop (SceneView sceneview) {
			var current = UnityEngine.Event.current;
			var references = DragAndDrop.objectReferences;
			if (current.type == EventType.Repaint || current.type == EventType.Layout) return;

			// Allow drag and drop of one SkeletonDataAsset.
			if (references.Length == 1) {
				var skeletonDataAsset = references[0] as SkeletonDataAsset;
				if (skeletonDataAsset != null) {
					var mousePos = current.mousePosition;

					bool invalidSkeletonData = skeletonDataAsset.GetSkeletonData(true) == null;
					if (invalidSkeletonData) {
						DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
						Handles.BeginGUI();
						GUI.Label(new Rect(mousePos + new Vector2(20f, 20f), new Vector2(400f, 40f)), new GUIContent(string.Format("{0} is invalid.\nCannot create new Spine GameObject.", skeletonDataAsset.name), SpineEditorUtilities.Icons.warning));
						Handles.EndGUI();
						return;
					} else {
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						Handles.BeginGUI();
						GUI.Label(new Rect(mousePos + new Vector2(20f, 20f), new Vector2(400f, 20f)), new GUIContent(string.Format("Create Spine GameObject ({0})", skeletonDataAsset.skeletonJSON.name), SpineEditorUtilities.Icons.spine));
						Handles.EndGUI();

						if (current.type == EventType.DragPerform) {
							RectTransform rectTransform = (Selection.activeGameObject == null) ? null : Selection.activeGameObject.GetComponent<RectTransform>();
							Plane plane = (rectTransform == null) ? new Plane(Vector3.back, Vector3.zero) : new Plane(-rectTransform.forward, rectTransform.position);
							Vector3 spawnPoint = MousePointToWorldPoint2D(mousePos, sceneview.camera, plane);
							ShowInstantiateContextMenu(skeletonDataAsset, spawnPoint);
							DragAndDrop.AcceptDrag();
							current.Use();
						}
					}
				}
			}
		}

		static void HierarchyDragAndDrop (int instanceId, Rect selectionRect) {
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
					var references = DragAndDrop.objectReferences;
					if (references.Length == 1) {
						var skeletonDataAsset = references[0] as SkeletonDataAsset;
						if (skeletonDataAsset != null && skeletonDataAsset.GetSkeletonData(true) != null) {
							
							// Allow drag-and-dropping anywhere in the Hierarchy Window.
							// HACK: string-compare because we can't get its type via reflection.
							const string HierarchyWindow = "UnityEditor.SceneHierarchyWindow";
							if (HierarchyWindow.Equals(mouseOverWindow.GetType().ToString(), System.StringComparison.Ordinal)) {
								if (isDraggingEvent) {
									DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
									current.Use();
								} else if (isDropEvent) {
									ShowInstantiateContextMenu(skeletonDataAsset, Vector3.zero);
									DragAndDrop.AcceptDrag();
									current.Use();
									return;
								}
							}
								
						}
					}
				}
			}

		}

		public static void ShowInstantiateContextMenu (SkeletonDataAsset skeletonDataAsset, Vector3 spawnPoint) {
			var menu = new GenericMenu();

			// SkeletonAnimation
			menu.AddItem(new GUIContent("SkeletonAnimation"), false, HandleSkeletonComponentDrop, new SpawnMenuData {
				skeletonDataAsset = skeletonDataAsset,
				spawnPoint = spawnPoint,
				instantiateDelegate = (data) => InstantiateSkeletonAnimation(data),
				isUI = false
			});

			// SkeletonGraphic
			var skeletonGraphicInspectorType = System.Type.GetType("Spine.Unity.Editor.SkeletonGraphicInspector");
			if (skeletonGraphicInspectorType != null) {
				var graphicInstantiateDelegate = skeletonGraphicInspectorType.GetMethod("SpawnSkeletonGraphicFromDrop", BindingFlags.Static | BindingFlags.Public);
				if (graphicInstantiateDelegate != null)
					menu.AddItem(new GUIContent("SkeletonGraphic (UI)"), false, HandleSkeletonComponentDrop, new SpawnMenuData {
						skeletonDataAsset = skeletonDataAsset,
						spawnPoint = spawnPoint,
						instantiateDelegate = System.Delegate.CreateDelegate(typeof(InstantiateDelegate), graphicInstantiateDelegate) as InstantiateDelegate,
						isUI = true
					});
			}

			#if SPINE_SKELETONANIMATOR
			menu.AddSeparator("");
			// SkeletonAnimator
			menu.AddItem(new GUIContent("SkeletonAnimator"), false, HandleSkeletonComponentDrop, new SpawnMenuData {
				skeletonDataAsset = skeletonDataAsset,
				spawnPoint = spawnPoint,
				instantiateDelegate = (data) => InstantiateSkeletonAnimator(data)
			});
			#endif

			menu.ShowAsContext();
		}

		public static void HandleSkeletonComponentDrop (object menuData) {
			var data = (SpawnMenuData)menuData;

			if (data.skeletonDataAsset.GetSkeletonData(true) == null) {
				EditorUtility.DisplayDialog("Invalid SkeletonDataAsset", "Unable to create Spine GameObject.\n\nPlease check your SkeletonDataAsset.", "Ok");
				return;
			}

			bool isUI = data.isUI;

			GameObject newGameObject = null;
			Component newSkeletonComponent = data.instantiateDelegate.Invoke(data.skeletonDataAsset);
			newGameObject = newSkeletonComponent.gameObject;
			var transform = newGameObject.transform;

			var activeGameObject = Selection.activeGameObject;
			if (isUI && activeGameObject != null)
				transform.SetParent(activeGameObject.transform, false);

			newGameObject.transform.position = isUI ? data.spawnPoint : RoundVector(data.spawnPoint, 2);

			if (isUI && (activeGameObject == null || activeGameObject.GetComponent<RectTransform>() == null))
				Debug.Log("Created a UI Skeleton GameObject not under a RectTransform. It may not be visible until you parent it to a canvas.");

			if (!isUI && activeGameObject != null && activeGameObject.transform.localScale != Vector3.one)
				Debug.Log("New Spine GameObject was parented to a scaled Transform. It may not be the intended size.");

			Selection.activeGameObject = newGameObject;
			//EditorGUIUtility.PingObject(newGameObject); // Doesn't work when setting activeGameObject.
			Undo.RegisterCreatedObjectUndo(newGameObject, "Create Spine GameObject");
		}

		/// <summary>
		/// Rounds off vector components to a number of decimal digits.
		/// </summary>
		public static Vector3 RoundVector (Vector3 vector, int digits) {
			vector.x = (float)System.Math.Round(vector.x, digits);
			vector.y = (float)System.Math.Round(vector.y, digits);
			vector.z = (float)System.Math.Round(vector.z, digits);
			return vector;
		}

		/// <summary>
		/// Converts a mouse point to a world point on a plane.
		/// </summary>
		static Vector3 MousePointToWorldPoint2D (Vector2 mousePosition, Camera camera, Plane plane) {
			var screenPos = new Vector3(mousePosition.x, camera.pixelHeight - mousePosition.y, 0f);
			var ray = camera.ScreenPointToRay(screenPos);
			float distance;
			bool hit = plane.Raycast(ray, out distance);
			return ray.GetPoint(distance);
		}
		#endregion

		#region Hierarchy
		static void HierarchyIconsOnChanged () {
			if (showHierarchyIcons) {
				skeletonRendererTable.Clear();
				skeletonUtilityBoneTable.Clear();
				boundingBoxFollowerTable.Clear();

				SkeletonRenderer[] arr = Object.FindObjectsOfType<SkeletonRenderer>();
				foreach (SkeletonRenderer r in arr)
					skeletonRendererTable.Add(r.gameObject.GetInstanceID(), r.gameObject);

				SkeletonUtilityBone[] boneArr = Object.FindObjectsOfType<SkeletonUtilityBone>();
				foreach (SkeletonUtilityBone b in boneArr)
					skeletonUtilityBoneTable.Add(b.gameObject.GetInstanceID(), b);

				BoundingBoxFollower[] bbfArr = Object.FindObjectsOfType<BoundingBoxFollower>();
				foreach (BoundingBoxFollower bbf in bbfArr)
					boundingBoxFollowerTable.Add(bbf.gameObject.GetInstanceID(), bbf);
			}
		}

		static void HierarchyIconsOnGUI (int instanceId, Rect selectionRect) {
			if (showHierarchyIcons) {
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
		}
		#endregion

		#region Auto-Import Entry Point
		static void OnPostprocessAllAssets (string[] imported, string[] deleted, string[] moved, string[] movedFromAssetPaths) {
			if (imported.Length == 0)
				return;

			// In case user used "Assets -> Reimport All", during the import process,
			// asset database is not initialized until some point. During that period,
			// all attempts to load any assets using API (i.e. AssetDatabase.LoadAssetAtPath)
			// will return null, and as result, assets won't be loaded even if they actually exists,
			// which may lead to numerous importing errors.
			// This situation also happens if Library folder is deleted from the project, which is a pretty
			// common case, since when using version control systems, the Library folder must be excluded.
			// 
			// So to avoid this, in case asset database is not available, we delay loading the assets
			// until next time.
			//
			// Unity *always* reimports some internal assets after the process is done, so this method
			// is always called once again in a state when asset database is available.
			//
			// Checking whether AssetDatabase is initialized is done by attempting to load
			// a known "marker" asset that should always be available. Failing to load this asset
			// means that AssetDatabase is not initialized.
			assetsImportedInWrongState.UnionWith(imported);
			if (AssetDatabaseAvailabilityDetector.IsAssetDatabaseAvailable()) {
				string[] combinedAssets = assetsImportedInWrongState.ToArray();
				assetsImportedInWrongState.Clear();
				ImportSpineContent(combinedAssets);
			}
		}

		public static void ImportSpineContent (string[] imported, bool reimport = false) {
			var atlasPaths = new List<string>();
			var imagePaths = new List<string>();
			var skeletonPaths = new List<string>();

			foreach (string str in imported) {
				string extension = Path.GetExtension(str).ToLower();
				switch (extension) {
				case ".txt":
					if (str.EndsWith(".atlas.txt", System.StringComparison.Ordinal))
						atlasPaths.Add(str);
					break;
				case ".png":
				case ".jpg":
					imagePaths.Add(str);
					break;
				case ".json":
					var jsonAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(str, typeof(TextAsset));
					if (jsonAsset != null && IsSpineData(jsonAsset))
						skeletonPaths.Add(str);
					break;
				case ".bytes":
					if (str.ToLower().EndsWith(".skel.bytes", System.StringComparison.Ordinal)) {
						if (IsSpineData((TextAsset)AssetDatabase.LoadAssetAtPath(str, typeof(TextAsset))))
							skeletonPaths.Add(str);
					}
					break;
				}
			}
				
			// Import atlases first.
			var atlases = new List<AtlasAsset>();
			foreach (string ap in atlasPaths) {
				TextAsset atlasText = (TextAsset)AssetDatabase.LoadAssetAtPath(ap, typeof(TextAsset));
				AtlasAsset atlas = IngestSpineAtlas(atlasText);
				atlases.Add(atlas);
			}

			// Import skeletons and match them with atlases.
			bool abortSkeletonImport = false;
			foreach (string sp in skeletonPaths) {
				if (!reimport && CheckForValidSkeletonData(sp)) {
					ReloadSkeletonData(sp);
					continue;
				}

				string dir = Path.GetDirectoryName(sp);

				#if SPINE_TK2D
				IngestSpineProject(AssetDatabase.LoadAssetAtPath(sp, typeof(TextAsset)) as TextAsset, null);
				#else
				var localAtlases = FindAtlasesAtPath(dir);
				var requiredPaths = GetRequiredAtlasRegions(sp);
				var atlasMatch = GetMatchingAtlas(requiredPaths, localAtlases);
				if (atlasMatch != null || requiredPaths.Count == 0) {
					IngestSpineProject(AssetDatabase.LoadAssetAtPath(sp, typeof(TextAsset)) as TextAsset, atlasMatch);
				} else {
					bool resolved = false;
					while (!resolved) {

						var filename = Path.GetFileNameWithoutExtension(sp);
						int result = EditorUtility.DisplayDialogComplex(
							string.Format("AtlasAsset for \"{0}\"", filename),
							string.Format("Could not automatically set the AtlasAsset for \"{0}\". You may set it manually.", filename),
							"Choose AtlasAssets...", "Skip this", "Stop importing all"
						);

						switch (result) {
						case -1:
							//Debug.Log("Select Atlas");
							AtlasAsset selectedAtlas = GetAtlasDialog(Path.GetDirectoryName(sp));
							if (selectedAtlas != null) {
								localAtlases.Clear();
								localAtlases.Add(selectedAtlas);
								atlasMatch = GetMatchingAtlas(requiredPaths, localAtlases);
								if (atlasMatch != null) {
									resolved = true;
									IngestSpineProject(AssetDatabase.LoadAssetAtPath(sp, typeof(TextAsset)) as TextAsset, atlasMatch);
								}
							}
							break;
						case 0: // Choose AtlasAssets...
							var atlasList = MultiAtlasDialog(requiredPaths, Path.GetDirectoryName(sp), Path.GetFileNameWithoutExtension(sp));
							if (atlasList != null)
								IngestSpineProject(AssetDatabase.LoadAssetAtPath(sp, typeof(TextAsset)) as TextAsset, atlasList.ToArray());

							resolved = true;
							break;
						case 1: // Skip
							Debug.Log("Skipped importing: " + Path.GetFileName(sp));
							resolved = true;
							break;
						case 2: // Stop importing all
							abortSkeletonImport = true;
							resolved = true;
							break;
						}
					}
				}

				if (abortSkeletonImport)
					break;
				#endif
			}
			// Any post processing of images
		}

		static void ReloadSkeletonData (string skeletonJSONPath) {
			string dir = Path.GetDirectoryName(skeletonJSONPath);
			TextAsset textAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(skeletonJSONPath, typeof(TextAsset));
			DirectoryInfo dirInfo = new DirectoryInfo(dir);
			FileInfo[] files = dirInfo.GetFiles("*.asset");

			foreach (var f in files) {
				string localPath = dir + "/" + f.Name;
				var obj = AssetDatabase.LoadAssetAtPath(localPath, typeof(Object));
				var skeletonDataAsset = obj as SkeletonDataAsset;
				if (skeletonDataAsset != null) {
					if (skeletonDataAsset.skeletonJSON == textAsset) {
						if (Selection.activeObject == skeletonDataAsset)
							Selection.activeObject = null;

						Debug.LogFormat("Changes to '{0}' detected. Clearing SkeletonDataAsset: {1}", skeletonJSONPath, localPath);
						skeletonDataAsset.Clear();

						string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(skeletonDataAsset));
						string lastHash = EditorPrefs.GetString(guid + "_hash");

						// For some weird reason sometimes Unity loses the internal Object pointer,
						// and as a result, all comparisons with null returns true.
						// But the C# wrapper is still alive, so we can "restore" the object
						// by reloading it from its Instance ID.
						AtlasAsset[] skeletonDataAtlasAssets = skeletonDataAsset.atlasAssets;
						if (skeletonDataAtlasAssets != null) {
							for (int i = 0; i < skeletonDataAtlasAssets.Length; i++) {
								if (!ReferenceEquals(null, skeletonDataAtlasAssets[i]) &&
									skeletonDataAtlasAssets[i].Equals(null) &&
									skeletonDataAtlasAssets[i].GetInstanceID() != 0
								) {
									skeletonDataAtlasAssets[i] = EditorUtility.InstanceIDToObject(skeletonDataAtlasAssets[i].GetInstanceID()) as AtlasAsset;
								}
							}
						}

						SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(true);
						string currentHash = skeletonData != null ? skeletonData.Hash : null;

						#if SPINE_SKELETONANIMATOR
						if (currentHash == null || lastHash != currentHash)
							UpdateMecanimClips(skeletonDataAsset);
						#endif

						// if (currentHash == null || lastHash != currentHash)
						// Do any upkeep on synchronized assets

						if (currentHash != null)
							EditorPrefs.SetString(guid + "_hash", currentHash);
					}
				}
			}
		}
		#endregion

		#region Match SkeletonData with Atlases
		//static readonly AttachmentType[] NonAtlasTypes = { AttachmentType.Boundingbox, AttachmentType.Path };
		static readonly AttachmentType[] AtlasTypes = { AttachmentType.Region, AttachmentType.Linkedmesh, AttachmentType.Mesh };

		static List<AtlasAsset> MultiAtlasDialog (List<string> requiredPaths, string initialDirectory, string filename = "") {
			List<AtlasAsset> atlasAssets = new List<AtlasAsset>();
			bool resolved = false;
			string lastAtlasPath = initialDirectory;
			while (!resolved) {

				// Build dialog box message.
				var missingRegions = new List<string>(requiredPaths);
				var dialogText = new StringBuilder();
				{
					dialogText.AppendLine(string.Format("SkeletonDataAsset for \"{0}\"", filename));
					dialogText.AppendLine("has missing regions.");
					dialogText.AppendLine();
					dialogText.AppendLine("Current Atlases:");

					if (atlasAssets.Count == 0)
						dialogText.AppendLine("\t--none--");

					for (int i = 0; i < atlasAssets.Count; i++)
						dialogText.AppendLine("\t" + atlasAssets[i].name);

					dialogText.AppendLine();
					dialogText.AppendLine("Missing Regions:");

					foreach (var atlasAsset in atlasAssets) {
						var atlas = atlasAsset.GetAtlas();
						for (int i = 0; i < missingRegions.Count; i++) {
							if (atlas.FindRegion(missingRegions[i]) != null) {
								missingRegions.RemoveAt(i);
								i--;
							}
						}
					}
						
					int n = missingRegions.Count;
					if (n == 0) break;

					const int MaxListLength = 15;
					for (int i = 0; (i < n && i < MaxListLength); i++)
						dialogText.AppendLine("\t" + missingRegions[i]);

					if (n > MaxListLength) dialogText.AppendLine(string.Format("\t... {0} more...", n - MaxListLength));
				}

				// Show dialog box.
				int result = EditorUtility.DisplayDialogComplex(
					"SkeletonDataAsset has missing Atlas.",
					dialogText.ToString(),
					"Browse...", "Import anyway", "Cancel"
				);

				switch (result) {
				case 0: // Browse...
					AtlasAsset selectedAtlasAsset = GetAtlasDialog(lastAtlasPath);
					if (selectedAtlasAsset != null) {
						var atlas = selectedAtlasAsset.GetAtlas();
						bool hasValidRegion = false;
						foreach (string str in missingRegions) {
							if (atlas.FindRegion(str) != null) {
								hasValidRegion = true;
								break;
							}
						}
						atlasAssets.Add(selectedAtlasAsset);
					}
					break;
				case 1: // Import anyway
					resolved = true;
					break;
				case 2: // Cancel
					atlasAssets = null;
					resolved = true;
					break;
				}
			}

			return atlasAssets;
		}

		static AtlasAsset GetAtlasDialog (string dirPath) {
			string path = EditorUtility.OpenFilePanel("Select AtlasAsset...", dirPath, "asset");
			if (path == "") return null; // Canceled or closed by user.

			int subLen = Application.dataPath.Length - 6;
			string assetRelativePath = path.Substring(subLen, path.Length - subLen).Replace("\\", "/");

			Object obj = AssetDatabase.LoadAssetAtPath(assetRelativePath, typeof(AtlasAsset));

			if (obj == null || obj.GetType() != typeof(AtlasAsset))
				return null;

			return (AtlasAsset)obj;
		}

		static void AddRequiredAtlasRegionsFromBinary (string skeletonDataPath, List<string> requiredPaths) {
			SkeletonBinary binary = new SkeletonBinary(new AtlasRequirementLoader(requiredPaths));
			TextAsset data = (TextAsset)AssetDatabase.LoadAssetAtPath(skeletonDataPath, typeof(TextAsset));
			MemoryStream input = new MemoryStream(data.bytes);
			binary.ReadSkeletonData(input);
			binary = null;
		}

		public static List<string> GetRequiredAtlasRegions (string skeletonDataPath) {
			List<string> requiredPaths = new List<string>();

			if (skeletonDataPath.Contains(".skel")) {
				AddRequiredAtlasRegionsFromBinary(skeletonDataPath, requiredPaths);
				return requiredPaths;
			}

			TextAsset spineJson = (TextAsset)AssetDatabase.LoadAssetAtPath(skeletonDataPath, typeof(TextAsset));

			StringReader reader = new StringReader(spineJson.text);
			var root = Json.Deserialize(reader) as Dictionary<string, object>;

			if (!root.ContainsKey("skins"))
				return requiredPaths;			

			foreach (KeyValuePair<string, object> entry in (Dictionary<string, object>)root["skins"]) {
				foreach (KeyValuePair<string, object> slotEntry in (Dictionary<string, object>)entry.Value) {

					foreach (KeyValuePair<string, object> attachmentEntry in ((Dictionary<string, object>)slotEntry.Value)) {
						var data = ((Dictionary<string, object>)attachmentEntry.Value);

						// Ignore non-atlas-requiring types.
						if (data.ContainsKey("type")) {
							AttachmentType attachmentType;
							string typeString = (string)data["type"];
							try {
								attachmentType = (AttachmentType)System.Enum.Parse(typeof(AttachmentType), typeString, true);
							} catch (System.ArgumentException e) {
								// For more info, visit: http://esotericsoftware.com/forum/Spine-editor-and-runtime-version-management-6534
								Debug.LogWarning(string.Format("Unidentified Attachment type: \"{0}\". Skeleton may have been exported from an incompatible Spine version.", typeString));
								throw e;
							}
								
							if (!AtlasTypes.Contains(attachmentType))
								continue;
						}

						if (data.ContainsKey("path"))
							requiredPaths.Add((string)data["path"]);
						else if (data.ContainsKey("name"))
							requiredPaths.Add((string)data["name"]);
						else
							requiredPaths.Add(attachmentEntry.Key);
					}
				}
			}

			return requiredPaths;
		}

		static AtlasAsset GetMatchingAtlas (List<string> requiredPaths, List<AtlasAsset> atlasAssets) {
			AtlasAsset atlasAssetMatch = null;

			foreach (AtlasAsset a in atlasAssets) {
				Atlas atlas = a.GetAtlas();
				bool failed = false;
				foreach (string regionPath in requiredPaths) {
					if (atlas.FindRegion(regionPath) == null) {
						failed = true;
						break;
					}
				}

				if (!failed) {
					atlasAssetMatch = a;
					break;
				}
			}

			return atlasAssetMatch;
		}

		public class AtlasRequirementLoader : AttachmentLoader {
			List<string> requirementList;

			public AtlasRequirementLoader (List<string> requirementList) {
				this.requirementList = requirementList;
			}

			public RegionAttachment NewRegionAttachment (Skin skin, string name, string path) {
				requirementList.Add(path);
				return new RegionAttachment(name);
			}

			public MeshAttachment NewMeshAttachment (Skin skin, string name, string path) {
				requirementList.Add(path);
				return new MeshAttachment(name);
			}

			public BoundingBoxAttachment NewBoundingBoxAttachment (Skin skin, string name) {
				return new BoundingBoxAttachment(name);
			}

			public PathAttachment NewPathAttachment (Skin skin, string name) {
				return new PathAttachment(name);
			}

			public PointAttachment NewPointAttachment (Skin skin, string name) {
				return new PointAttachment(name);
			}

			public ClippingAttachment NewClippingAttachment (Skin skin, string name) {
				return new ClippingAttachment(name);
			}
		}
		#endregion

		#region Import Atlases
		static List<AtlasAsset> FindAtlasesAtPath (string path) {
			List<AtlasAsset> arr = new List<AtlasAsset>();
			DirectoryInfo dir = new DirectoryInfo(path);
			FileInfo[] assetInfoArr = dir.GetFiles("*.asset");

			int subLen = Application.dataPath.Length - 6;
			foreach (var f in assetInfoArr) {
				string assetRelativePath = f.FullName.Substring(subLen, f.FullName.Length - subLen).Replace("\\", "/");
				Object obj = AssetDatabase.LoadAssetAtPath(assetRelativePath, typeof(AtlasAsset));
				if (obj != null)
					arr.Add(obj as AtlasAsset);
			}

			return arr;
		}

		static AtlasAsset IngestSpineAtlas (TextAsset atlasText) {
			if (atlasText == null) {
				Debug.LogWarning("Atlas source cannot be null!");
				return null;
			}

			string primaryName = Path.GetFileNameWithoutExtension(atlasText.name).Replace(".atlas", "");
			string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(atlasText));

			string atlasPath = assetPath + "/" + primaryName + "_Atlas.asset";

			AtlasAsset atlasAsset = (AtlasAsset)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(AtlasAsset));

			List<Material> vestigialMaterials = new List<Material>();

			if (atlasAsset == null)
				atlasAsset = AtlasAsset.CreateInstance<AtlasAsset>();
			else {
				foreach (Material m in atlasAsset.materials)
					vestigialMaterials.Add(m);
			}

			protectFromStackGarbageCollection.Add(atlasAsset);
			atlasAsset.atlasFile = atlasText;

			//strip CR
			string atlasStr = atlasText.text;
			atlasStr = atlasStr.Replace("\r", "");

			string[] atlasLines = atlasStr.Split('\n');
			List<string> pageFiles = new List<string>();
			for (int i = 0; i < atlasLines.Length - 1; i++) {
				if (atlasLines[i].Trim().Length == 0)
					pageFiles.Add(atlasLines[i + 1].Trim());
			}

			atlasAsset.materials = new Material[pageFiles.Count];

			for (int i = 0; i < pageFiles.Count; i++) {
				string texturePath = assetPath + "/" + pageFiles[i];
				Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));

				TextureImporter texImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
				#if UNITY_5_5_OR_NEWER
				texImporter.textureCompression = TextureImporterCompression.Uncompressed;
				texImporter.alphaSource = TextureImporterAlphaSource.FromInput;
				#else
				texImporter.textureType = TextureImporterType.Advanced;
				texImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				#endif
				texImporter.mipmapEnabled = false;
				texImporter.alphaIsTransparency = false; // Prevent the texture importer from applying bleed to the transparent parts.
				texImporter.spriteImportMode = SpriteImportMode.None;
				texImporter.maxTextureSize = 2048;


				EditorUtility.SetDirty(texImporter);
				AssetDatabase.ImportAsset(texturePath);
				AssetDatabase.SaveAssets();

				string pageName = Path.GetFileNameWithoutExtension(pageFiles[i]);

				//because this looks silly
				if (pageName == primaryName && pageFiles.Count == 1)
					pageName = "Material";

				string materialPath = assetPath + "/" + primaryName + "_" + pageName + ".mat";
				Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));

				if (mat == null) {
					mat = new Material(Shader.Find(defaultShader));
					AssetDatabase.CreateAsset(mat, materialPath);
				} else {
					vestigialMaterials.Remove(mat);
				}

				mat.mainTexture = texture;
				EditorUtility.SetDirty(mat);
				AssetDatabase.SaveAssets();

				atlasAsset.materials[i] = mat;
			}

			for (int i = 0; i < vestigialMaterials.Count; i++)
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(vestigialMaterials[i]));

			if (AssetDatabase.GetAssetPath(atlasAsset) == "")
				AssetDatabase.CreateAsset(atlasAsset, atlasPath);
			else
				atlasAsset.Clear();

			EditorUtility.SetDirty(atlasAsset);
			AssetDatabase.SaveAssets();

			// Iterate regions and bake marked.
			Atlas atlas = atlasAsset.GetAtlas();
			FieldInfo field = typeof(Atlas).GetField("regions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic);
			List<AtlasRegion> regions = (List<AtlasRegion>)field.GetValue(atlas);
			string atlasAssetPath = AssetDatabase.GetAssetPath(atlasAsset);
			string atlasAssetDirPath = Path.GetDirectoryName(atlasAssetPath);
			string bakedDirPath = Path.Combine(atlasAssetDirPath, atlasAsset.name);

			bool hasBakedRegions = false;
			for (int i = 0; i < regions.Count; i++) {
				AtlasRegion region = regions[i];
				string bakedPrefabPath = Path.Combine(bakedDirPath, SpineEditorUtilities.GetPathSafeRegionName(region) + ".prefab").Replace("\\", "/");
				GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(bakedPrefabPath, typeof(GameObject));
				if (prefab != null) {
					BakeRegion(atlasAsset, region, false);
					hasBakedRegions = true;
				}
			}

			if (hasBakedRegions) {
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			protectFromStackGarbageCollection.Remove(atlasAsset);
			return (AtlasAsset)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(AtlasAsset));
		}
		#endregion

		#region Bake Atlas Region
		public static GameObject BakeRegion (AtlasAsset atlasAsset, AtlasRegion region, bool autoSave = true) {
			Atlas atlas = atlasAsset.GetAtlas();
			string atlasAssetPath = AssetDatabase.GetAssetPath(atlasAsset);
			string atlasAssetDirPath = Path.GetDirectoryName(atlasAssetPath);
			string bakedDirPath = Path.Combine(atlasAssetDirPath, atlasAsset.name);
			string bakedPrefabPath = Path.Combine(bakedDirPath, GetPathSafeRegionName(region) + ".prefab").Replace("\\", "/");

			GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(bakedPrefabPath, typeof(GameObject));
			GameObject root;
			Mesh mesh;
			bool isNewPrefab = false;

			if (!Directory.Exists(bakedDirPath))
				Directory.CreateDirectory(bakedDirPath);

			if (prefab == null) {
				root = new GameObject("temp", typeof(MeshFilter), typeof(MeshRenderer));
				prefab = PrefabUtility.CreatePrefab(bakedPrefabPath, root);
				isNewPrefab = true;
				Object.DestroyImmediate(root);
			}

			mesh = (Mesh)AssetDatabase.LoadAssetAtPath(bakedPrefabPath, typeof(Mesh));

			Material mat = null;
			mesh = atlasAsset.GenerateMesh(region.name, mesh, out mat);
			if (isNewPrefab) {
				AssetDatabase.AddObjectToAsset(mesh, prefab);
				prefab.GetComponent<MeshFilter>().sharedMesh = mesh;
			}

			EditorUtility.SetDirty(mesh);
			EditorUtility.SetDirty(prefab);

			if (autoSave) {
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			prefab.GetComponent<MeshRenderer>().sharedMaterial = mat;

			return prefab;
		}
		#endregion

		#region Import SkeletonData (json or binary)
		static SkeletonDataAsset IngestSpineProject (TextAsset spineJson, params AtlasAsset[] atlasAssets) {
			string primaryName = Path.GetFileNameWithoutExtension(spineJson.name);
			string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(spineJson));
			string filePath = assetPath + "/" + primaryName + "_SkeletonData.asset";

			#if SPINE_TK2D
			if (spineJson != null) {
				SkeletonDataAsset skeletonDataAsset = (SkeletonDataAsset)AssetDatabase.LoadAssetAtPath(filePath, typeof(SkeletonDataAsset));
				if (skeletonDataAsset == null) {
					skeletonDataAsset = SkeletonDataAsset.CreateInstance<SkeletonDataAsset>();
					skeletonDataAsset.skeletonJSON = spineJson;
					skeletonDataAsset.fromAnimation = new string[0];
					skeletonDataAsset.toAnimation = new string[0];
					skeletonDataAsset.duration = new float[0];
					skeletonDataAsset.defaultMix = defaultMix;
					skeletonDataAsset.scale = defaultScale;

					AssetDatabase.CreateAsset(skeletonDataAsset, filePath);
					AssetDatabase.SaveAssets();
				} else {
					skeletonDataAsset.Clear();
					skeletonDataAsset.GetSkeletonData(true);
				}

				return skeletonDataAsset;
			} else {
				EditorUtility.DisplayDialog("Error!", "Tried to ingest null Spine data.", "OK");
				return null;
			}

			#else
			if (spineJson != null && atlasAssets != null) {
				SkeletonDataAsset skeletonDataAsset = (SkeletonDataAsset)AssetDatabase.LoadAssetAtPath(filePath, typeof(SkeletonDataAsset));
				if (skeletonDataAsset == null) {
					skeletonDataAsset = ScriptableObject.CreateInstance<SkeletonDataAsset>(); {
						skeletonDataAsset.atlasAssets = atlasAssets;
						skeletonDataAsset.skeletonJSON = spineJson;
						skeletonDataAsset.defaultMix = defaultMix;
						skeletonDataAsset.scale = defaultScale;
					}

					AssetDatabase.CreateAsset(skeletonDataAsset, filePath);
					AssetDatabase.SaveAssets();
				} else {
					skeletonDataAsset.atlasAssets = atlasAssets;
					skeletonDataAsset.Clear();
					skeletonDataAsset.GetSkeletonData(true);
				}

				return skeletonDataAsset;
			} else {
				EditorUtility.DisplayDialog("Error!", "Must specify both Spine JSON and AtlasAsset array", "OK");
				return null;
			}
			#endif
		}
		#endregion

		#region Checking Methods
		static int[][] compatibleVersions = { new[] {3, 6, 0} };
		//static bool isFixVersionRequired = false;

		static bool CheckForValidSkeletonData (string skeletonJSONPath) {
			string dir = Path.GetDirectoryName(skeletonJSONPath);
			TextAsset textAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(skeletonJSONPath, typeof(TextAsset));
			DirectoryInfo dirInfo = new DirectoryInfo(dir);
			FileInfo[] files = dirInfo.GetFiles("*.asset");

			foreach (var path in files) {
				string localPath = dir + "/" + path.Name;
				var obj = AssetDatabase.LoadAssetAtPath(localPath, typeof(Object));
				var skeletonDataAsset = obj as SkeletonDataAsset;
				if (skeletonDataAsset != null && skeletonDataAsset.skeletonJSON == textAsset)
					return true;
			}

			return false;
		}

		public static bool IsSpineData (TextAsset asset) {
			if (asset == null) return false;

			bool isSpineData = false;
			string rawVersion = null;

			if (asset.name.Contains(".skel")) {
				try {
					rawVersion = SkeletonBinary.GetVersionString(new MemoryStream(asset.bytes));
					//Debug.Log(rawVersion);
					isSpineData = !(string.IsNullOrEmpty(rawVersion));
				} catch (System.Exception e) {
					Debug.LogErrorFormat("Failed to read '{0}'. It is likely not a binary Spine SkeletonData file.\n{1}", asset.name, e);
					return false;
				}
			} else {
				var obj = Json.Deserialize(new StringReader(asset.text));
				if (obj == null) {
					Debug.LogErrorFormat("'{0}' is not valid JSON.", asset.name);
					return false;
				}

				var root = obj as Dictionary<string, object>;
				if (root == null) {
					Debug.LogError("Parser returned an incorrect type.");
					return false;
				}

				isSpineData = root.ContainsKey("skeleton");
				if (isSpineData) {
					var skeletonInfo = (Dictionary<string, object>)root["skeleton"];
					object jv;
					skeletonInfo.TryGetValue("spine", out jv);
					rawVersion = jv as string;
				}
			}

			// Version warning
			if (isSpineData) {
				string runtimeVersion = compatibleVersions[0][0] + "." + compatibleVersions[0][1];

				if (string.IsNullOrEmpty(rawVersion)) {
					Debug.LogWarningFormat("Skeleton '{0}' has no version information. It may be incompatible with your runtime version: spine-unity v{1}", asset.name, runtimeVersion);
				} else {
					string[] versionSplit = rawVersion.Split('.');
					bool match = false;
					foreach (var version in compatibleVersions) {
						bool primaryMatch = version[0] == int.Parse(versionSplit[0]);
						bool secondaryMatch = version[1] == int.Parse(versionSplit[1]);

						// if (isFixVersionRequired) secondaryMatch &= version[2] <= int.Parse(jsonVersionSplit[2]);

						if (primaryMatch && secondaryMatch) {
							match = true;
							break;
						}
					}

					if (!match)
						Debug.LogWarningFormat("Skeleton '{0}' (exported with Spine {1}) may be incompatible with your runtime version: spine-unity v{2}", asset.name, rawVersion, runtimeVersion);
				}
			}

			return isSpineData;
		}
		#endregion

		#region SkeletonAnimation Menu
//		[MenuItem("Assets/Spine/Instantiate (SkeletonAnimation)", false, 10)]
//		static void InstantiateSkeletonAnimation () {
//			Object[] arr = Selection.objects;
//			foreach (Object o in arr) {
//				string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
//				string skinName = EditorPrefs.GetString(guid + "_lastSkin", "");
//
//				InstantiateSkeletonAnimation((SkeletonDataAsset)o, skinName, false);
//				SceneView.RepaintAll();
//			}
//		}
//
//		[MenuItem("Assets/Spine/Instantiate (SkeletonAnimation)", true, 10)]
//		static bool ValidateInstantiateSkeletonAnimation () {
//			Object[] arr = Selection.objects;
//
//			if (arr.Length == 0)
//				return false;
//
//			foreach (Object o in arr) {
//				if (o.GetType() != typeof(SkeletonDataAsset))
//					return false;
//			}
//
//			return true;
//		}

		public static void IngestAdvancedRenderSettings (SkeletonRenderer skeletonRenderer) {
			const string PMAShaderQuery = "Spine/Skeleton";
			const string TintBlackShaderQuery = "Tint Black";

			if (skeletonRenderer == null) return;
			var skeletonDataAsset = skeletonRenderer.skeletonDataAsset;
			if (skeletonDataAsset == null) return;

			bool pmaVertexColors = false;
			bool tintBlack = false;
			foreach (AtlasAsset atlasAsset in skeletonDataAsset.atlasAssets) {
				if (!pmaVertexColors) {
					foreach (Material m in atlasAsset.materials) {
						if (m.shader.name.Contains(PMAShaderQuery)) {
							pmaVertexColors = true;
							break;
						}
					}
				}

				if (!tintBlack) {
					foreach (Material m in atlasAsset.materials) {
						if (m.shader.name.Contains(TintBlackShaderQuery)) {
							tintBlack = true;
							break;
						}
					}
				}
			}

			skeletonRenderer.pmaVertexColors = pmaVertexColors;
			skeletonRenderer.tintBlack = tintBlack;
		}

		public static SkeletonAnimation InstantiateSkeletonAnimation (SkeletonDataAsset skeletonDataAsset, string skinName, bool destroyInvalid = true) {
			var skeletonData = skeletonDataAsset.GetSkeletonData(true);
			var skin = skeletonData != null ? skeletonData.FindSkin(skinName) : null;
			return InstantiateSkeletonAnimation(skeletonDataAsset, skin, destroyInvalid);
		}

		public static SkeletonAnimation InstantiateSkeletonAnimation (SkeletonDataAsset skeletonDataAsset, Skin skin = null, bool destroyInvalid = true) {
			SkeletonData data = skeletonDataAsset.GetSkeletonData(true);

			if (data == null) {
				for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++) {
					string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
					skeletonDataAsset.atlasAssets[i] = (AtlasAsset)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAsset));
				}
				data = skeletonDataAsset.GetSkeletonData(false);
			}

			if (data == null) {
				Debug.LogWarning("InstantiateSkeletonAnimation tried to instantiate a skeleton from an invalid SkeletonDataAsset.");
				return null;
			}

			string spineGameObjectName = string.Format("Spine GameObject ({0})", skeletonDataAsset.name.Replace("_SkeletonData", ""));
			GameObject go = new GameObject(spineGameObjectName, typeof(MeshFilter), typeof(MeshRenderer), typeof(SkeletonAnimation));
			SkeletonAnimation newSkeletonAnimation = go.GetComponent<SkeletonAnimation>();
			newSkeletonAnimation.skeletonDataAsset = skeletonDataAsset;
			IngestAdvancedRenderSettings(newSkeletonAnimation);

			try {
				newSkeletonAnimation.Initialize(false);
			} catch (System.Exception e) {
				if (destroyInvalid) {
					Debug.LogWarning("Editor-instantiated SkeletonAnimation threw an Exception. Destroying GameObject to prevent orphaned GameObject.");
					GameObject.DestroyImmediate(go);
				}
				throw e;
			}

			// Set Defaults
			bool noSkins = data.DefaultSkin == null && (data.Skins == null || data.Skins.Count == 0); // Support attachmentless/skinless SkeletonData.
			skin = skin ?? data.DefaultSkin ?? (noSkins ? null : data.Skins.Items[0]);
			if (skin != null) {
				newSkeletonAnimation.initialSkinName = skin.Name;
				newSkeletonAnimation.skeleton.SetSkin(skin);
			}

			newSkeletonAnimation.zSpacing = defaultZSpacing;

			newSkeletonAnimation.skeleton.Update(0);
			newSkeletonAnimation.state.Update(0);
			newSkeletonAnimation.state.Apply(newSkeletonAnimation.skeleton);
			newSkeletonAnimation.skeleton.UpdateWorldTransform();

			return newSkeletonAnimation;
		}
		#endregion

		#region SkeletonAnimator
		#if SPINE_SKELETONANIMATOR
		static void UpdateMecanimClips (SkeletonDataAsset skeletonDataAsset) {
			if (skeletonDataAsset.controller == null)
				return;

			SkeletonBaker.GenerateMecanimAnimationClips(skeletonDataAsset);
		}

		public static SkeletonAnimator InstantiateSkeletonAnimator (SkeletonDataAsset skeletonDataAsset, string skinName) {
			return InstantiateSkeletonAnimator(skeletonDataAsset, skeletonDataAsset.GetSkeletonData(true).FindSkin(skinName));
		}

		public static SkeletonAnimator InstantiateSkeletonAnimator (SkeletonDataAsset skeletonDataAsset, Skin skin = null) {
			string spineGameObjectName = string.Format("Spine Mecanim GameObject ({0})", skeletonDataAsset.name.Replace("_SkeletonData", ""));
			GameObject go = new GameObject(spineGameObjectName, typeof(MeshFilter), typeof(MeshRenderer), typeof(Animator), typeof(SkeletonAnimator));

			if (skeletonDataAsset.controller == null) {
				SkeletonBaker.GenerateMecanimAnimationClips(skeletonDataAsset);
				Debug.Log(string.Format("Mecanim controller was automatically generated and assigned for {0}", skeletonDataAsset.name));
			}

			go.GetComponent<Animator>().runtimeAnimatorController = skeletonDataAsset.controller;

			SkeletonAnimator anim = go.GetComponent<SkeletonAnimator>();
			anim.skeletonDataAsset = skeletonDataAsset;
			IngestAdvancedRenderSettings(anim);

			SkeletonData data = skeletonDataAsset.GetSkeletonData(true);
			if (data == null) {
				for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++) {
					string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
					skeletonDataAsset.atlasAssets[i] = (AtlasAsset)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAsset));
				}
				data = skeletonDataAsset.GetSkeletonData(true);
			}

			// Set defaults
			skin = skin ?? data.DefaultSkin ?? data.Skins.Items[0];
			anim.zSpacing = defaultZSpacing;

			anim.Initialize(false);
			anim.skeleton.SetSkin(skin);
			anim.initialSkinName = skin.Name;

			anim.skeleton.Update(0);
			anim.skeleton.UpdateWorldTransform();
			anim.LateUpdate();

			return anim;
		}
		#endif
		#endregion

		#region TK2D Support
		const string SPINE_TK2D_DEFINE = "SPINE_TK2D";

		static void EnableTK2D () {
			bool added = false;
			foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup))) {				
				int gi = (int)group;
				if (gi == 15 || gi == 16 || group == BuildTargetGroup.Unknown)
					continue;

				string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
				if (!defines.Contains(SPINE_TK2D_DEFINE)) {
					added = true;
					if (defines.EndsWith(";", System.StringComparison.Ordinal))
						defines = defines + SPINE_TK2D_DEFINE;
					else
						defines = defines + ";" + SPINE_TK2D_DEFINE;

					PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
				}
			}

			if (added) {
				Debug.LogWarning("Setting Scripting Define Symbol " + SPINE_TK2D_DEFINE);
			} else {
				Debug.LogWarning("Already Set Scripting Define Symbol " + SPINE_TK2D_DEFINE);
			}
		}


		static void DisableTK2D () {
			bool removed = false;
			foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup))) {
				int gi = (int)group;
				if (gi == 15 || gi == 16 || group == BuildTargetGroup.Unknown)
					continue;
				
				string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
				if (defines.Contains(SPINE_TK2D_DEFINE)) {
					removed = true;
					if (defines.Contains(SPINE_TK2D_DEFINE + ";"))
						defines = defines.Replace(SPINE_TK2D_DEFINE + ";", "");
					else
						defines = defines.Replace(SPINE_TK2D_DEFINE, "");

					PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
				}
			}

			if (removed) {
				Debug.LogWarning("Removing Scripting Define Symbol " + SPINE_TK2D_DEFINE);
			} else {
				Debug.LogWarning("Already Removed Scripting Define Symbol " + SPINE_TK2D_DEFINE);
			}
		}
		#endregion

		public static string GetPathSafeRegionName (AtlasRegion region) {
			return region.name.Replace("/", "_");
		}
	}

	public static class SpineHandles {
		internal static float handleScale = 1f;
		public static Color BoneColor { get { return new Color(0.8f, 0.8f, 0.8f, 0.4f); } }
		public static Color PathColor { get { return new Color(254/255f, 127/255f, 0); } }
		public static Color TransformContraintColor { get { return new Color(170/255f, 226/255f, 35/255f); } }
		public static Color IkColor { get { return new Color(228/255f,90/255f,43/255f); } }

		static Vector3[] _boneMeshVerts = {
			new Vector3(0, 0, 0),
			new Vector3(0.1f, 0.1f, 0),
			new Vector3(1, 0, 0),
			new Vector3(0.1f, -0.1f, 0)
		};
		static Mesh _boneMesh;
		public static Mesh BoneMesh {
			get {
				if (_boneMesh == null) {
					_boneMesh = new Mesh {
						vertices = _boneMeshVerts,
						uv = new Vector2[4],
						triangles = new [] { 0, 1, 2, 2, 3, 0 }
					};
					_boneMesh.RecalculateBounds();
					_boneMesh.RecalculateNormals();
				}
				return _boneMesh;
			}
		}

		static Mesh _arrowheadMesh;
		public static Mesh ArrowheadMesh {
			get {
				if (_arrowheadMesh == null) {
					_arrowheadMesh = new Mesh {
						vertices = new [] {
							new Vector3(0, 0),
							new Vector3(-0.1f, 0.05f),
							new Vector3(-0.1f, -0.05f)
						},
						uv = new Vector2[3],
						triangles = new [] { 0, 1, 2 }
					};
					_arrowheadMesh.RecalculateBounds();
					_arrowheadMesh.RecalculateNormals();
				}
				return _arrowheadMesh;
			}
		}

		static Material _boneMaterial;
		static Material BoneMaterial {
			get {
				if (_boneMaterial == null) {
					_boneMaterial = new Material(Shader.Find("Hidden/Spine/Bones"));
					_boneMaterial.SetColor("_Color", SpineHandles.BoneColor);
				}

				return _boneMaterial;
			}
		}
		public static Material GetBoneMaterial () {
			BoneMaterial.SetColor("_Color", SpineHandles.BoneColor);
			return BoneMaterial;
		}

		public static Material GetBoneMaterial (Color color) {
			BoneMaterial.SetColor("_Color", color);
			return BoneMaterial;
		}

		static Material _ikMaterial;
		public static Material IKMaterial {
			get {
				if (_ikMaterial == null) {
					_ikMaterial = new Material(Shader.Find("Hidden/Spine/Bones"));
					_ikMaterial.SetColor("_Color", SpineHandles.IkColor);
				}
				return _ikMaterial;
			}
		}

		static GUIStyle _boneNameStyle;
		public static GUIStyle BoneNameStyle {
			get {
				if (_boneNameStyle == null) {
					_boneNameStyle = new GUIStyle(EditorStyles.whiteMiniLabel);
					_boneNameStyle.alignment = TextAnchor.MiddleCenter;
					_boneNameStyle.stretchWidth = true;
					_boneNameStyle.padding = new RectOffset(0, 0, 0, 0);
					_boneNameStyle.contentOffset = new Vector2(-5f, 0f);
				}
				return _boneNameStyle;
			}
		}

		static GUIStyle _pathNameStyle;
		public static GUIStyle PathNameStyle {
			get {
				if (_pathNameStyle == null) {
					_pathNameStyle = new GUIStyle(SpineHandles.BoneNameStyle);
					_pathNameStyle.normal.textColor = SpineHandles.PathColor;
				}
				return _pathNameStyle;
			}
		}

		public static void DrawBoneNames (Transform transform, Skeleton skeleton) {
			GUIStyle style = BoneNameStyle;
			foreach (Bone b in skeleton.Bones) {
				var pos = new Vector3(b.WorldX, b.WorldY, 0) + (new Vector3(b.A, b.C) * (b.Data.Length * 0.5f));
				pos = transform.TransformPoint(pos);
				Handles.Label(pos, b.Data.Name, style);
			}
		}

		public static void DrawBones (Transform transform, Skeleton skeleton) {
			float boneScale = 1.8f; // Draw the root bone largest;
			DrawCrosshairs2D(skeleton.Bones.Items[0].GetWorldPosition(transform), 0.08f);

			foreach (Bone b in skeleton.Bones) {
				DrawBone(transform, b, boneScale);
				boneScale = 1f;
			}
		}

		static Vector3[] _boneWireBuffer = new Vector3[5];
		static Vector3[] GetBoneWireBuffer (Matrix4x4 m) {
			for (int i = 0, n = _boneMeshVerts.Length; i < n; i++)
				_boneWireBuffer[i] = m.MultiplyPoint(_boneMeshVerts[i]);

			_boneWireBuffer[4] = _boneWireBuffer[0]; // closed polygon.
			return _boneWireBuffer;
		}
		public static void DrawBoneWireframe (Transform transform, Bone b, Color color) {
			Handles.color = color;
			var pos = new Vector3(b.WorldX, b.WorldY, 0);
			float length = b.Data.Length;

			if (length > 0) {
				Quaternion rot = Quaternion.Euler(0, 0, b.WorldRotationX);
				Vector3 scale = Vector3.one * length * b.WorldScaleX;
				const float my = 1.5f;
				scale.y *= (SpineHandles.handleScale + 1f) * 0.5f;
				scale.y = Mathf.Clamp(scale.x, -my, my);
				Handles.DrawPolyLine(GetBoneWireBuffer(transform.localToWorldMatrix * Matrix4x4.TRS(pos, rot, scale)));
				var wp = transform.TransformPoint(pos);
				DrawBoneCircle(wp, color, transform.forward);
			} else {
				var wp = transform.TransformPoint(pos);
				DrawBoneCircle(wp, color, transform.forward);
			}
		}

		public static void DrawBone (Transform transform, Bone b, float boneScale) {
			var pos = new Vector3(b.WorldX, b.WorldY, 0);
			float length = b.Data.Length;
			if (length > 0) {
				Quaternion rot = Quaternion.Euler(0, 0, b.WorldRotationX);
				Vector3 scale = Vector3.one * length * b.WorldScaleX;
				const float my = 1.5f;
				scale.y *= (SpineHandles.handleScale + 1f) * 0.5f;
				scale.y = Mathf.Clamp(scale.x, -my, my);
				SpineHandles.GetBoneMaterial().SetPass(0);
				Graphics.DrawMeshNow(SpineHandles.BoneMesh, transform.localToWorldMatrix * Matrix4x4.TRS(pos, rot, scale));
			} else {
				var wp = transform.TransformPoint(pos);
				DrawBoneCircle(wp, SpineHandles.BoneColor, transform.forward, boneScale);
			}
		}

		public static void DrawBone (Transform transform, Bone b, float boneScale, Color color) {
			var pos = new Vector3(b.WorldX, b.WorldY, 0);
			float length = b.Data.Length;
			if (length > 0) {
				Quaternion rot = Quaternion.Euler(0, 0, b.WorldRotationX);
				Vector3 scale = Vector3.one * length * b.WorldScaleX;
				const float my = 1.5f;
				scale.y *= (SpineHandles.handleScale + 1f) * 0.5f;
				scale.y = Mathf.Clamp(scale.x, -my, my);
				SpineHandles.GetBoneMaterial(color).SetPass(0);
				Graphics.DrawMeshNow(SpineHandles.BoneMesh, transform.localToWorldMatrix * Matrix4x4.TRS(pos, rot, scale));
			} else {
				var wp = transform.TransformPoint(pos);
				DrawBoneCircle(wp, color, transform.forward, boneScale);
			}
		}

		public static void DrawPaths (Transform transform, Skeleton skeleton) {
			foreach (Slot s in skeleton.DrawOrder) {
				var p = s.Attachment as PathAttachment;
				if (p != null) SpineHandles.DrawPath(s, p, transform, true);
			}
		}

		static float[] pathVertexBuffer;
		public static void DrawPath (Slot s, PathAttachment p, Transform t, bool includeName) {
			int worldVerticesLength = p.WorldVerticesLength;

			if (pathVertexBuffer == null || pathVertexBuffer.Length < worldVerticesLength)
				pathVertexBuffer = new float[worldVerticesLength];

			float[] pv = pathVertexBuffer;
			p.ComputeWorldVertices(s, pv);

			var ocolor = Handles.color;
			Handles.color = SpineHandles.PathColor;

			Matrix4x4 m = t.localToWorldMatrix;
			const int step = 6;
			int n = worldVerticesLength - step;
			Vector3 p0, p1, p2, p3;
			for (int i = 2; i < n; i += step) {
				p0 = m.MultiplyPoint(new Vector3(pv[i], pv[i+1]));
				p1 = m.MultiplyPoint(new Vector3(pv[i+2], pv[i+3]));
				p2 = m.MultiplyPoint(new Vector3(pv[i+4], pv[i+5]));
				p3 = m.MultiplyPoint(new Vector3(pv[i+6], pv[i+7]));
				DrawCubicBezier(p0, p1, p2, p3);
			}

			n += step;
			if (p.Closed) {
				p0 = m.MultiplyPoint(new Vector3(pv[n - 4], pv[n - 3]));
				p1 = m.MultiplyPoint(new Vector3(pv[n - 2], pv[n - 1]));
				p2 = m.MultiplyPoint(new Vector3(pv[0], pv[1]));
				p3 = m.MultiplyPoint(new Vector3(pv[2], pv[3]));
				DrawCubicBezier(p0, p1, p2, p3);
			}

			const float endCapSize = 0.05f;
			Vector3 firstPoint = m.MultiplyPoint(new Vector3(pv[2], pv[3]));
			SpineHandles.DrawDot(firstPoint, endCapSize);

			//if (!p.Closed) SpineHandles.DrawDot(m.MultiplyPoint(new Vector3(pv[n - 4], pv[n - 3])), endCapSize);
			if (includeName) Handles.Label(firstPoint + new Vector3(0,0.1f), p.Name, PathNameStyle);

			Handles.color = ocolor;
		}

		public static void DrawDot (Vector3 position, float size) {
			#if UNITY_5_6_OR_NEWER
			Handles.DotHandleCap(0, position, Quaternion.identity, size * HandleUtility.GetHandleSize(position), EventType.Ignore);
			#else
			Handles.DotCap(0, position, Quaternion.identity, size * HandleUtility.GetHandleSize(position));
			#endif
		}

		public static void DrawBoundingBoxes (Transform transform, Skeleton skeleton) {
			foreach (var slot in skeleton.Slots) {
				var bba = slot.Attachment as BoundingBoxAttachment;
				if (bba != null) SpineHandles.DrawBoundingBox(slot, bba, transform);
			}
		}

		public static void DrawBoundingBox (Slot slot, BoundingBoxAttachment box, Transform t) {
			if (box.Vertices.Length <= 0) return; // Handle cases where user creates a BoundingBoxAttachment but doesn't actually define it.

			var worldVerts = new float[box.Vertices.Length];
			box.ComputeWorldVertices(slot, worldVerts);

			Handles.color = Color.green;
			Vector3 lastVert = Vector3.zero;
			Vector3 vert = Vector3.zero;
			Vector3 firstVert = t.TransformPoint(new Vector3(worldVerts[0], worldVerts[1], 0));
			for (int i = 0; i < worldVerts.Length; i += 2) {
				vert.x = worldVerts[i];
				vert.y = worldVerts[i + 1];
				vert.z = 0;

				vert = t.TransformPoint(vert);

				if (i > 0)
					Handles.DrawLine(lastVert, vert);

				lastVert = vert;
			}

			Handles.DrawLine(lastVert, firstVert);
		}

		public static void DrawConstraints (Transform transform, Skeleton skeleton) {
			Vector3 targetPos;
			Vector3 pos;
			bool active;
			Color handleColor;
			const float Thickness = 4f;
			Vector3 normal = transform.forward;

			// Transform Constraints
			handleColor = SpineHandles.TransformContraintColor;
			foreach (var tc in skeleton.TransformConstraints) {
				var targetBone = tc.Target;
				targetPos = targetBone.GetWorldPosition(transform);

				if (tc.TranslateMix > 0) {
					if (tc.TranslateMix != 1f) {
						Handles.color = handleColor;
						foreach (var b in tc.Bones) {
							pos = b.GetWorldPosition(transform);
							Handles.DrawDottedLine(targetPos, pos, Thickness);
						}
					}
					SpineHandles.DrawBoneCircle(targetPos, handleColor, normal, 1.3f);
					Handles.color = handleColor;
					SpineHandles.DrawCrosshairs(targetPos, 0.2f, targetBone.A, targetBone.B, targetBone.C, targetBone.D, transform);
				}
			}

			// IK Constraints
			handleColor = SpineHandles.IkColor;
			foreach (var ikc in skeleton.IkConstraints) {
				Bone targetBone = ikc.Target;
				targetPos = targetBone.GetWorldPosition(transform);
				var bones = ikc.Bones;
				active = ikc.Mix > 0;
				if (active) {
					pos = bones.Items[0].GetWorldPosition(transform);
					switch (bones.Count) {
					case 1: {
							Handles.color = handleColor;
							Handles.DrawLine(targetPos, pos);
							SpineHandles.DrawBoneCircle(targetPos, handleColor, normal);
							var m = bones.Items[0].GetMatrix4x4();
							m.m03 = targetBone.WorldX;
							m.m13 = targetBone.WorldY;
							SpineHandles.DrawArrowhead(transform.localToWorldMatrix * m);
							break;	
						}
					case 2: {
							Bone childBone = bones.Items[1];
							Vector3 child = childBone.GetWorldPosition(transform);
							Handles.color = handleColor;
							Handles.DrawLine(child, pos);
							Handles.DrawLine(targetPos, child);
							SpineHandles.DrawBoneCircle(pos, handleColor, normal, 0.5f);
							SpineHandles.DrawBoneCircle(child, handleColor, normal, 0.5f);
							SpineHandles.DrawBoneCircle(targetPos, handleColor, normal);
							var m = childBone.GetMatrix4x4();
							m.m03 = targetBone.WorldX;
							m.m13 = targetBone.WorldY;
							SpineHandles.DrawArrowhead(transform.localToWorldMatrix * m);
							break;
						}
					}
				}
				//Handles.Label(targetPos, ikc.Data.Name, SpineHandles.BoneNameStyle);
			}

			// Path Constraints
			handleColor = SpineHandles.PathColor;
			foreach (var pc in skeleton.PathConstraints) {
				active = pc.TranslateMix > 0;
				if (active)
					foreach (var b in pc.Bones)
						SpineHandles.DrawBoneCircle(b.GetWorldPosition(transform), handleColor, normal, 1f);
			}
		}

		static void DrawCrosshairs2D (Vector3 position, float scale) {
			scale *= SpineHandles.handleScale;
			Handles.DrawLine(position + new Vector3(-scale, 0), position + new Vector3(scale, 0));
			Handles.DrawLine(position + new Vector3(0, -scale), position + new Vector3(0, scale));
		}

		static void DrawCrosshairs (Vector3 position, float scale, float a, float b, float c, float d, Transform transform) {
			scale *= SpineHandles.handleScale;

			var xOffset = (Vector3)(new Vector2(a, c).normalized * scale);
			var yOffset = (Vector3)(new Vector2(b, d).normalized * scale);
			xOffset = transform.TransformDirection(xOffset);
			yOffset = transform.TransformDirection(yOffset);

			Handles.DrawLine(position + xOffset, position - xOffset);
			Handles.DrawLine(position + yOffset, position - yOffset);
		}

		static void DrawArrowhead2D (Vector3 pos, float localRotation, float scale = 1f) {
			scale *= SpineHandles.handleScale;

			SpineHandles.IKMaterial.SetPass(0);
			Graphics.DrawMeshNow(SpineHandles.ArrowheadMesh, Matrix4x4.TRS(pos, Quaternion.Euler(0, 0, localRotation), new Vector3(scale, scale, scale)));
		}

		static void DrawArrowhead (Matrix4x4 m) {
			var s = SpineHandles.handleScale;
			m.m00 *= s;
			m.m01 *= s;
			m.m02 *= s;
			m.m10 *= s;
			m.m11 *= s;
			m.m12 *= s;
			m.m20 *= s;
			m.m21 *= s;
			m.m22 *= s;

			SpineHandles.IKMaterial.SetPass(0);
			Graphics.DrawMeshNow(SpineHandles.ArrowheadMesh, m);
		}

		static void DrawBoneCircle (Vector3 pos, Color outlineColor, Vector3 normal, float scale = 1f) {
			scale *= SpineHandles.handleScale;

			Color o = Handles.color;
			Handles.color = outlineColor;
			float firstScale = 0.08f * scale;
			Handles.DrawSolidDisc(pos, normal, firstScale);
			const float Thickness = 0.03f;
			float secondScale = firstScale - (Thickness  * SpineHandles.handleScale);

			if (secondScale > 0f) {
				Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
				Handles.DrawSolidDisc(pos, normal, secondScale);
			}

			Handles.color = o;
		}

		internal static void DrawCubicBezier (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
			Handles.DrawBezier(p0, p3, p1, p2, Handles.color, Texture2D.whiteTexture, 2f);
			//			const float dotSize = 0.01f;
			//			Quaternion q = Quaternion.identity;
			//			Handles.DotCap(0, p0, q, dotSize);
			//			Handles.DotCap(0, p1, q, dotSize);
			//			Handles.DotCap(0, p2, q, dotSize);
			//			Handles.DotCap(0, p3, q, dotSize);
			//			Handles.DrawLine(p0, p1);
			//			Handles.DrawLine(p3, p2);
		}
	}

}
