/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
#pragma warning disable 0219

/*****************************************************************************
 * Spine Editor Utilities created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
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
			public static Texture2D boundingBox;
			public static Texture2D mesh;
			public static Texture2D weights;
			public static Texture2D skin;
			public static Texture2D skinsRoot;
			public static Texture2D animation;
			public static Texture2D animationRoot;
			public static Texture2D spine;
			public static Texture2D _event;
			public static Texture2D constraintNib;
			public static Texture2D warning;
			public static Texture2D skeletonUtility;
			public static Texture2D hingeChain;
			public static Texture2D subMeshRenderer;
			public static Texture2D unityIcon;
			public static Texture2D controllerIcon;

			internal static Mesh _boneMesh;
			public static Mesh BoneMesh {
				get {
					if (_boneMesh == null) {
						_boneMesh = new Mesh();
						_boneMesh.vertices = new [] {
							new Vector3(0, 0, 0),
							new Vector3(-0.1f, 0.1f, 0),
							new Vector3(0, 1, 0),
							new Vector3(0.1f, 0.1f, 0)
						};
						_boneMesh.uv = new Vector2[4];
						_boneMesh.triangles = new [] { 0, 1, 2, 2, 3, 0 };
						_boneMesh.RecalculateBounds();
						_boneMesh.RecalculateNormals();
					}
					return _boneMesh;
				}
			}

			internal static Material _boneMaterial;
			public static Material BoneMaterial {
				get {
					if (_boneMaterial == null) {
						#if UNITY_4_3
						_boneMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
						_boneMaterial.SetColor("_TintColor", new Color(0.4f, 0.4f, 0.4f, 0.25f));
						#else
						_boneMaterial = new Material(Shader.Find("Hidden/Spine/Bones"));
						_boneMaterial.SetColor("_Color", new Color(0.4f, 0.4f, 0.4f, 0.25f));
						#endif
					}
					return _boneMaterial;
				}
			}

			public static void Initialize () {
				skeleton = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-skeleton.png");
				nullBone = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-null.png");
				bone = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-bone.png");
				poseBones = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-poseBones.png");
				boneNib = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-boneNib.png");
				slot = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-slot.png");
				slotRoot = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-slotRoot.png");
				skinPlaceholder = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-skinPlaceholder.png");
				image = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-image.png");
				boundingBox = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-boundingBox.png");
				mesh = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-mesh.png");
				weights = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-weights.png");
				skin = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-skinPlaceholder.png");
				skinsRoot = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-skinsRoot.png");
				animation = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-animation.png");
				animationRoot = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-animationRoot.png");
				spine = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-spine.png");
				_event = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-event.png");
				constraintNib = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-constraintNib.png");
				warning = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-warning.png");
				skeletonUtility = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-skeletonUtility.png");
				hingeChain = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-hingeChain.png");
				subMeshRenderer = (Texture2D)AssetDatabase.LoadMainAssetAtPath(SpineEditorUtilities.editorGUIPath + "/icon-subMeshRenderer.png");

				unityIcon = EditorGUIUtility.FindTexture("SceneAsset Icon");

				controllerIcon = EditorGUIUtility.FindTexture("AnimatorController Icon");
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

		const bool DEFAULT_SHOW_HIERARCHY_ICONS = true;
		const string SHOW_HIERARCHY_ICONS_KEY = "SPINE_SHOW_HIERARCHY_ICONS";
		public static bool showHierarchyIcons = DEFAULT_SHOW_HIERARCHY_ICONS;

		#region Initialization
		static SpineEditorUtilities () {
			Initialize();
		}

		static void Initialize () {
			{
				defaultMix = EditorPrefs.GetFloat(DEFAULT_MIX_KEY, DEFAULT_DEFAULT_MIX);
				defaultScale = EditorPrefs.GetFloat(DEFAULT_SCALE_KEY, DEFAULT_DEFAULT_SCALE);
				defaultShader = EditorPrefs.GetString(DEFAULT_SHADER_KEY, DEFAULT_DEFAULT_SHADER);	
				showHierarchyIcons = EditorPrefs.GetBool(SHOW_HIERARCHY_ICONS_KEY, DEFAULT_SHOW_HIERARCHY_ICONS);
			}

			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			SceneView.onSceneGUIDelegate += OnSceneGUI;

			DirectoryInfo rootDir = new DirectoryInfo(Application.dataPath);
			FileInfo[] files = rootDir.GetFiles("SpineEditorUtilities.cs", SearchOption.AllDirectories);
			editorPath = Path.GetDirectoryName(files[0].FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets"));
			editorGUIPath = editorPath + "/GUI";

			Icons.Initialize();

			assetsImportedInWrongState = new HashSet<string>();
			skeletonRendererTable = new Dictionary<int, GameObject>();
			skeletonUtilityBoneTable = new Dictionary<int, SkeletonUtilityBone>();
			boundingBoxFollowerTable = new Dictionary<int, BoundingBoxFollower>();

			EditorApplication.hierarchyWindowChanged -= HierarchyWindowChanged;
			EditorApplication.hierarchyWindowChanged += HierarchyWindowChanged;
			EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowItemOnGUI;
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;

			HierarchyWindowChanged();
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
			if (!preferencesLoaded) {
				preferencesLoaded = true;
				defaultMix = EditorPrefs.GetFloat(DEFAULT_MIX_KEY, DEFAULT_DEFAULT_MIX);
				defaultScale = EditorPrefs.GetFloat(DEFAULT_SCALE_KEY, DEFAULT_DEFAULT_SCALE);
				defaultShader = EditorPrefs.GetString(DEFAULT_SHADER_KEY, DEFAULT_DEFAULT_SHADER);
				showHierarchyIcons = EditorPrefs.GetBool(SHOW_HIERARCHY_ICONS_KEY, DEFAULT_SHOW_HIERARCHY_ICONS);
			}


			EditorGUI.BeginChangeCheck();
			showHierarchyIcons = EditorGUILayout.Toggle(new GUIContent("Show Hierarchy Icons", "Show relevant icons on GameObjects with Spine Components on them. Disable this if you have large, complex scenes."), showHierarchyIcons);
			if (EditorGUI.EndChangeCheck()) {
				EditorPrefs.SetBool(SHOW_HIERARCHY_ICONS_KEY, showHierarchyIcons);
				HierarchyWindowChanged();				
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
			#if UNITY_5_3_OR_NEWER
			defaultShader = EditorGUILayout.DelayedTextField(new GUIContent("Default shader", "Default shader for materials auto-generated on import."), defaultShader); 
			#else
			defaultShader = EditorGUILayout.TextField(new GUIContent("Default shader", "Default shader for materials auto-generated on import."), defaultShader); 
			#endif
			if (EditorGUI.EndChangeCheck())
				EditorPrefs.SetString(DEFAULT_SHADER_KEY, defaultShader);
			
			GUILayout.Space(20);
			EditorGUILayout.LabelField("3rd Party Settings", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("TK2D");

			if (GUILayout.Button("Enable", GUILayout.Width(64)))
				EnableTK2D();
			if (GUILayout.Button("Disable", GUILayout.Width(64)))
				DisableTK2D();
			GUILayout.EndHorizontal();
		}
		#endregion

		#region Drag and Drop to Scene View

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

		static void OnSceneGUI (SceneView sceneview) {
			var current = UnityEngine.Event.current;
			var references = DragAndDrop.objectReferences;

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
					}

				}
			}

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
			if (activeGameObject != null)
				transform.SetParent(activeGameObject.transform, false);

			newGameObject.transform.position = isUI ? data.spawnPoint : RoundVector(data.spawnPoint, 2);

			if (isUI && (activeGameObject == null || activeGameObject.GetComponent<RectTransform>() == null))
				Debug.Log("Created a UI Skeleton GameObject not under a RectTransform. It may not be visible until you parent it to a canvas.");

			if (!isUI && activeGameObject != null && activeGameObject.transform.localScale != Vector3.one)
				Debug.Log("New Spine GameObject was parented to a scaled Transform. It may not be the intended size.");

			Selection.activeGameObject = newGameObject;
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

		#region Hierarchy Icons
		static void HierarchyWindowChanged () {
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

		static void HierarchyWindowItemOnGUI (int instanceId, Rect selectionRect) {
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
					if (IsValidSpineData((TextAsset)AssetDatabase.LoadAssetAtPath(str, typeof(TextAsset))))
						skeletonPaths.Add(str);
					break;
				case ".bytes":
					if (str.ToLower().EndsWith(".skel.bytes", System.StringComparison.Ordinal)) {
						if (IsValidSpineData((TextAsset)AssetDatabase.LoadAssetAtPath(str, typeof(TextAsset))))
							skeletonPaths.Add(str);
					}
					break;
				}
			}
				
			// Import atlases first.
			var atlases = new List<AtlasAsset>();
			foreach (string ap in atlasPaths) {
				// MITCH: left note: Always import atlas data now.
				TextAsset atlasText = (TextAsset)AssetDatabase.LoadAssetAtPath(ap, typeof(TextAsset));
				AtlasAsset atlas = IngestSpineAtlas(atlasText);
				atlases.Add(atlas);
			}

			// Import skeletons and match them with atlases.
			bool abortSkeletonImport = false;
			foreach (string sp in skeletonPaths) {
				if (!reimport && CheckForValidSkeletonData(sp)) {
					ResetExistingSkeletonData(sp);
					continue;
				}

				string dir = Path.GetDirectoryName(sp);

				#if SPINE_TK2D
				IngestSpineProject(AssetDatabase.LoadAssetAtPath(sp, typeof(TextAsset)) as TextAsset, null);
				#else
				var localAtlases = FindAtlasesAtPath(dir);
				var requiredPaths = GetRequiredAtlasRegions(sp);
				var atlasMatch = GetMatchingAtlas(requiredPaths, localAtlases);
				if (atlasMatch != null) {
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
							Debug.Log("Select Atlas");
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
			// MITCH: left a todo: any post processing of images
		}

		static void ResetExistingSkeletonData (string skeletonJSONPath) {
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

						skeletonDataAsset.Reset();

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

						if (currentHash != null) {
							EditorPrefs.SetString(guid + "_hash", currentHash);
						}
					}
				}
			}
		}
		#endregion

		#region Match SkeletonData with Atlases
		static readonly AttachmentType[] NonAtlasTypes = { AttachmentType.Boundingbox, AttachmentType.Path };

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

							if (NonAtlasTypes.Contains(attachmentType))
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
				texImporter.textureType = TextureImporterType.Advanced;
				texImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				texImporter.mipmapEnabled = false;
				texImporter.alphaIsTransparency = false;
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
				atlasAsset.Reset();

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
					skeletonDataAsset.Reset();
					skeletonDataAsset.GetSkeletonData(true);
				}

				return skeletonDataAsset;
			} else {
				EditorUtility.DisplayDialog("Error!", "Tried to ingest null Spine data.", "OK");
				return null;
			}

			#else
			if (spineJson != null && atlasAssets != null) {
				SkeletonDataAsset skelDataAsset = (SkeletonDataAsset)AssetDatabase.LoadAssetAtPath(filePath, typeof(SkeletonDataAsset));
				if (skelDataAsset == null) {
					skelDataAsset = SkeletonDataAsset.CreateInstance<SkeletonDataAsset>();
					skelDataAsset.atlasAssets = atlasAssets;
					skelDataAsset.skeletonJSON = spineJson;
					skelDataAsset.fromAnimation = new string[0];
					skelDataAsset.toAnimation = new string[0];
					skelDataAsset.duration = new float[0];
					skelDataAsset.defaultMix = defaultMix;
					skelDataAsset.scale = defaultScale;

					AssetDatabase.CreateAsset(skelDataAsset, filePath);
					AssetDatabase.SaveAssets();
				} else {
					skelDataAsset.atlasAssets = atlasAssets;
					skelDataAsset.Reset();
					skelDataAsset.GetSkeletonData(true);
				}

				return skelDataAsset;
			} else {
				EditorUtility.DisplayDialog("Error!", "Must specify both Spine JSON and AtlasAsset array", "OK");
				return null;
			}
			#endif
		}
		#endregion

		#region Checking Methods
		static int[][] compatibleVersions = { new[] {3, 4, 0}, new[] {3, 3, 0} };
		static bool isFixVersionRequired = false;

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

		public static bool IsValidSpineData (TextAsset asset) {
			if (asset.name.Contains(".skel")) return true;

			object obj = null;
			obj = Json.Deserialize(new StringReader(asset.text));

			if (obj == null) {
				Debug.LogError("Is not valid JSON.");
				return false;
			}

			var root = obj as Dictionary<string, object>;
			if (root == null) {
				Debug.LogError("Parser returned an incorrect type.");
				return false;
			}

			// Version warning
			{
				var skeletonInfo = (Dictionary<string, object>)root["skeleton"];
				string jsonVersion = (string)skeletonInfo["spine"];
				if (!string.IsNullOrEmpty(jsonVersion)) {
					string[] jsonVersionSplit = jsonVersion.Split('.');
					bool match = false;
					foreach (var version in compatibleVersions) {
						bool primaryMatch = version[0] == int.Parse(jsonVersionSplit[0]);
						bool secondaryMatch = version[1] == int.Parse(jsonVersionSplit[1]);

						if (isFixVersionRequired)
							secondaryMatch &= version[2] <= int.Parse(jsonVersionSplit[2]);
						
						if (primaryMatch && secondaryMatch) {
							match = true;
							break;
						}
					}

					if (!match) {
						string runtimeVersion = compatibleVersions[0][0] + "." + compatibleVersions[0][1];
						Debug.LogWarning(string.Format("Skeleton '{0}' (exported with Spine {1}) may be incompatible with your runtime version: spine-unity v{2}", asset.name, jsonVersion, runtimeVersion));
					}
				}
			}


			return root.ContainsKey("skeleton");


		}
		#endregion

		#region SkeletonAnimation Menu
		[MenuItem("Assets/Spine/Instantiate (SkeletonAnimation)", false, 10)]
		static void InstantiateSkeletonAnimation () {
			Object[] arr = Selection.objects;
			foreach (Object o in arr) {
				string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
				string skinName = EditorPrefs.GetString(guid + "_lastSkin", "");

				InstantiateSkeletonAnimation((SkeletonDataAsset)o, skinName, false);
				SceneView.RepaintAll();
			}
		}

		[MenuItem("Assets/Spine/Instantiate (SkeletonAnimation)", true, 10)]
		static bool ValidateInstantiateSkeletonAnimation () {
			Object[] arr = Selection.objects;

			if (arr.Length == 0)
				return false;

			foreach (Object o in arr) {
				if (o.GetType() != typeof(SkeletonDataAsset))
					return false;
			}

			return true;
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

			if (skin == null) skin = data.DefaultSkin;
			if (skin == null) skin = data.Skins.Items[0];

			string spineGameObjectName = string.Format("Spine GameObject ({0})", skeletonDataAsset.name.Replace("_SkeletonData", ""));
			GameObject go = new GameObject(spineGameObjectName, typeof(MeshFilter), typeof(MeshRenderer), typeof(SkeletonAnimation));
			SkeletonAnimation newSkeletonAnimation = go.GetComponent<SkeletonAnimation>();
			newSkeletonAnimation.skeletonDataAsset = skeletonDataAsset;

			{
				bool requiresNormals = false;
				foreach (AtlasAsset atlasAsset in skeletonDataAsset.atlasAssets) {
					foreach (Material m in atlasAsset.materials) {
						if (m.shader.name.Contains("Lit")) {
							requiresNormals = true;
							break;
						}
					}
				}
				newSkeletonAnimation.calculateNormals = requiresNormals;
			}

			try {
				newSkeletonAnimation.Initialize(false);
			} catch (System.Exception e) {
				if (destroyInvalid) {
					Debug.LogWarning("Editor-instantiated SkeletonAnimation threw an Exception. Destroying GameObject to prevent orphaned GameObject.");
					GameObject.DestroyImmediate(go);
				}
				throw e;
			}

			newSkeletonAnimation.skeleton.SetSkin(skin);
			newSkeletonAnimation.initialSkinName = skin.Name;

			newSkeletonAnimation.skeleton.Update(1);
			newSkeletonAnimation.state.Update(1);
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

		[MenuItem("Assets/Spine/Instantiate (Mecanim)", false, 100)]
		static void InstantiateSkeletonAnimator () {
			Object[] arr = Selection.objects;
			foreach (Object o in arr) {
				string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
				string skinName = EditorPrefs.GetString(guid + "_lastSkin", "");

				InstantiateSkeletonAnimator((SkeletonDataAsset)o, skinName);
				SceneView.RepaintAll();
			}
		}

		[MenuItem("Assets/Spine/Instantiate (Mecanim)", true, 100)]
		static bool ValidateInstantiateSkeletonAnimator () {
			Object[] arr = Selection.objects;

			if (arr.Length == 0)
				return false;

			foreach (Object o in arr) {
				if (o.GetType() != typeof(SkeletonDataAsset))
					return false;
			}

			return true;
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

			bool requiresNormals = false;

			foreach (AtlasAsset atlasAsset in anim.skeletonDataAsset.atlasAssets) {
				foreach (Material m in atlasAsset.materials) {
					if (m.shader.name.Contains("Lit")) {
						requiresNormals = true;
						break;
					}
				}
			}

			anim.calculateNormals = requiresNormals;

			SkeletonData data = skeletonDataAsset.GetSkeletonData(true);

			if (data == null) {
				for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++) {
					string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
					skeletonDataAsset.atlasAssets[i] = (AtlasAsset)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAsset));
				}

				data = skeletonDataAsset.GetSkeletonData(true);
			}

			if (skin == null)
				skin = data.DefaultSkin;

			if (skin == null)
				skin = data.Skins.Items[0];

			anim.Initialize(false);

			anim.skeleton.SetSkin(skin);
			anim.initialSkinName = skin.Name;

			anim.skeleton.Update(1);
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
				if (group == BuildTargetGroup.Unknown)
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

}

