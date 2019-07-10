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
			public static Texture2D point;
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
			public static Texture2D skeletonDataAssetIcon;
			public static Texture2D info;
			public static Texture2D unity;

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
				point = LoadIcon("icon-point.png");
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

				skeletonDataAssetIcon = LoadIcon("SkeletonDataAsset Icon.png");

				info = EditorGUIUtility.FindTexture("console.infoicon.sml");
				unity = EditorGUIUtility.FindTexture("SceneAsset Icon");
			}

			public static Texture2D GetAttachmentIcon (Attachment attachment) {
				// Analysis disable once CanBeReplacedWithTryCastAndCheckForNull
				if (attachment is RegionAttachment)
					return Icons.image;
				else if (attachment is MeshAttachment)
					return ((MeshAttachment)attachment).IsWeighted() ? Icons.weights : Icons.mesh;
				else if (attachment is BoundingBoxAttachment)
					return Icons.boundingBox;
				else if (attachment is PointAttachment)
					return Icons.point;
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

		static int STRAIGHT_ALPHA_PARAM_ID = Shader.PropertyToID("_StraightAlphaInput");

	#if NEW_PREFERENCES_SETTINGS_PROVIDER
		static class SpineSettingsProviderRegistration
		{
			[SettingsProvider]
			public static SettingsProvider CreateSpineSettingsProvider()
			{
				var provider = new SettingsProvider("Spine", SettingsScope.User)
				{
					label = "Spine",
					guiHandler = (searchContext) =>
					{
						Preferences.HandlePreferencesGUI(); // This line shall NOT be merged to 3.8 branch. Version to provide a non-behavior-changing implementation for 3.7 branch.
					},

					// Populate the search keywords to enable smart search filtering and label highlighting:
					keywords = new HashSet<string>(new[] { "Spine", "Preferences", "Skeleton", "Default", "Mix", "Duration" })
				};
				return provider;
			}
		}
	#else
		// Preferences entry point
		[PreferenceItem("Spine")]
		static void PreferencesGUI () {
			Preferences.HandlePreferencesGUI();
		}
	#endif

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
			
			Preferences.Load();

			string[] assets = AssetDatabase.FindAssets("t:script SpineEditorUtilities");
			string assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
			editorPath = Path.GetDirectoryName(assetPath).Replace("\\", "/");
			editorGUIPath = editorPath + "/GUI";

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
			if (material == null || !material.HasProperty(STRAIGHT_ALPHA_PARAM_ID)) {
				return true; // non-Spine shader used on material
			}
			
			// 'sRGBTexture = true' generates incorrectly weighted mipmaps at PMA textures,
			// causing white borders due to undesired custom weighting.
			if (texImporter.sRGBTexture && texImporter.mipmapEnabled && PlayerSettings.colorSpace == ColorSpace.Gamma) {
				Debug.LogWarningFormat("`{0}` : Problematic Texture Settings found: When enabling `Generate Mip Maps` in Gamma color space, it is recommended to disable `sRGB (Color Texture)`. Otherwise you will receive white border artifacts on an atlas exported with default `Premultiply alpha` settings.\n(You can disable this warning in `Edit - Preferences - Spine`)", texturePath);
			}
			if (texImporter.alphaIsTransparency) {
				int straightAlphaValue = material.GetInt(STRAIGHT_ALPHA_PARAM_ID);
				if (straightAlphaValue == 0) {
					string materialName = System.IO.Path.GetFileName(materialPath);
					Debug.LogWarningFormat("`{0}` and material `{1}` : Incorrect Texture / Material Settings found: It is strongly recommended to disable `Alpha Is Transparency` on `Premultiply alpha` textures.\nAssuming `Premultiply alpha` texture because `Straight Alpha Texture` is disabled at material). (You can disable this warning in `Edit - Preferences - Spine`)", texturePath, materialName);
				}
			}
			return true;
		}
#endregion

		public static class Preferences {
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

			const bool DEFAULT_DEFAULT_INSTANTIATE_LOOP = true;
			const string DEFAULT_INSTANTIATE_LOOP_KEY = "SPINE_DEFAULT_INSTANTIATE_LOOP";
			public static bool defaultInstantiateLoop = DEFAULT_DEFAULT_INSTANTIATE_LOOP;

			const bool DEFAULT_SHOW_HIERARCHY_ICONS = true;
			const string SHOW_HIERARCHY_ICONS_KEY = "SPINE_SHOW_HIERARCHY_ICONS";
			public static bool showHierarchyIcons = DEFAULT_SHOW_HIERARCHY_ICONS;

			const bool DEFAULT_SET_TEXTUREIMPORTER_SETTINGS = true;
			const string SET_TEXTUREIMPORTER_SETTINGS_KEY = "SPINE_SET_TEXTUREIMPORTER_SETTINGS";
			public static bool setTextureImporterSettings = DEFAULT_SET_TEXTUREIMPORTER_SETTINGS;

			const bool DEFAULT_ATLASTXT_WARNING = true;
			const string ATLASTXT_WARNING_KEY = "SPINE_ATLASTXT_WARNING";
			public static bool atlasTxtImportWarning = DEFAULT_ATLASTXT_WARNING;

			const bool DEFAULT_TEXTUREIMPORTER_WARNING = true;
			const string TEXTUREIMPORTER_WARNING_KEY = "SPINE_TEXTUREIMPORTER_WARNING";
			public static bool textureImporterWarning = DEFAULT_TEXTUREIMPORTER_WARNING;

			internal const float DEFAULT_MIPMAPBIAS = -0.5f;

			public const float DEFAULT_SCENE_ICONS_SCALE = 1f;
			public const string SCENE_ICONS_SCALE_KEY = "SPINE_SCENE_ICONS_SCALE";

			const bool DEFAULT_AUTO_RELOAD_SCENESKELETONS = true;
			const string AUTO_RELOAD_SCENESKELETONS_KEY = "SPINE_AUTO_RELOAD_SCENESKELETONS";
			public static bool autoReloadSceneSkeletons = DEFAULT_AUTO_RELOAD_SCENESKELETONS;

			static bool preferencesLoaded = false;

			public static void Load () {
				if (preferencesLoaded)
					return;

				defaultMix = EditorPrefs.GetFloat(DEFAULT_MIX_KEY, DEFAULT_DEFAULT_MIX);
				defaultScale = EditorPrefs.GetFloat(DEFAULT_SCALE_KEY, DEFAULT_DEFAULT_SCALE);
				defaultZSpacing = EditorPrefs.GetFloat(DEFAULT_ZSPACING_KEY, DEFAULT_DEFAULT_ZSPACING);
				defaultShader = EditorPrefs.GetString(DEFAULT_SHADER_KEY, DEFAULT_DEFAULT_SHADER);
				showHierarchyIcons = EditorPrefs.GetBool(SHOW_HIERARCHY_ICONS_KEY, DEFAULT_SHOW_HIERARCHY_ICONS);
				setTextureImporterSettings = EditorPrefs.GetBool(SET_TEXTUREIMPORTER_SETTINGS_KEY, DEFAULT_SET_TEXTUREIMPORTER_SETTINGS);
				autoReloadSceneSkeletons = EditorPrefs.GetBool(AUTO_RELOAD_SCENESKELETONS_KEY, DEFAULT_AUTO_RELOAD_SCENESKELETONS);
				atlasTxtImportWarning = EditorPrefs.GetBool(ATLASTXT_WARNING_KEY, DEFAULT_ATLASTXT_WARNING);
				textureImporterWarning = EditorPrefs.GetBool(TEXTUREIMPORTER_WARNING_KEY, DEFAULT_TEXTUREIMPORTER_WARNING);

				SpineHandles.handleScale = EditorPrefs.GetFloat(SCENE_ICONS_SCALE_KEY, DEFAULT_SCENE_ICONS_SCALE);
				preferencesLoaded = true;
			}

			public static void HandlePreferencesGUI () {
				if (!preferencesLoaded)
					Load();

				EditorGUI.BeginChangeCheck();
				showHierarchyIcons = EditorGUILayout.Toggle(new GUIContent("Show Hierarchy Icons", "Show relevant icons on GameObjects with Spine Components on them. Disable this if you have large, complex scenes."), showHierarchyIcons);
				if (EditorGUI.EndChangeCheck()) {
					EditorPrefs.SetBool(SHOW_HIERARCHY_ICONS_KEY, showHierarchyIcons);
#if NEWPLAYMODECALLBACKS
					HierarchyHandler.IconsOnPlaymodeStateChanged(PlayModeStateChange.EnteredEditMode);
#else
					HierarchyHandler.IconsOnPlaymodeStateChanged();
#endif
				}

				BoolPrefsField(ref autoReloadSceneSkeletons, AUTO_RELOAD_SCENESKELETONS_KEY, new GUIContent("Auto-reload scene components", "Reloads Skeleton components in the scene whenever their SkeletonDataAsset is modified. This makes it so changes in the SkeletonDataAsset inspector are immediately reflected. This may be slow when your scenes have large numbers of SkeletonRenderers or SkeletonGraphic."));

				EditorGUILayout.Separator();
				EditorGUILayout.LabelField("Auto-Import Settings", EditorStyles.boldLabel);
				{
					SpineEditorUtilities.FloatPrefsField(ref defaultMix, DEFAULT_MIX_KEY, new GUIContent("Default Mix", "The Default Mix Duration for newly imported SkeletonDataAssets."), min: 0);
					SpineEditorUtilities.FloatPrefsField(ref defaultScale, DEFAULT_SCALE_KEY, new GUIContent("Default SkeletonData Scale", "The Default skeleton import scale for newly imported SkeletonDataAssets."), min: 0.0000001f);

					EditorGUI.BeginChangeCheck();
					var shader = (EditorGUILayout.ObjectField("Default Shader", Shader.Find(defaultShader), typeof(Shader), false) as Shader);
					defaultShader = shader != null ? shader.name : DEFAULT_DEFAULT_SHADER;
					if (EditorGUI.EndChangeCheck())
						EditorPrefs.SetString(DEFAULT_SHADER_KEY, defaultShader);

					SpineEditorUtilities.BoolPrefsField(ref setTextureImporterSettings, SET_TEXTUREIMPORTER_SETTINGS_KEY, new GUIContent("Apply Atlas Texture Settings", "Apply the recommended settings for Texture Importers."));
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Warnings", EditorStyles.boldLabel);
				{
					SpineEditorUtilities.BoolPrefsField(ref atlasTxtImportWarning, ATLASTXT_WARNING_KEY, new GUIContent("Atlas Extension Warning", "Log a warning and recommendation whenever a `.atlas` file is found."));
					SpineEditorUtilities.BoolPrefsField(ref textureImporterWarning, TEXTUREIMPORTER_WARNING_KEY, new GUIContent("Texture Settings Warning", "Log a warning and recommendation whenever Texture Import Settings are detected that could lead to undesired effects, e.g. white border artifacts."));
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Editor Instantiation", EditorStyles.boldLabel);
				{
					EditorGUI.BeginChangeCheck();
					defaultZSpacing = EditorGUILayout.Slider("Default Slot Z-Spacing", defaultZSpacing, -0.1f, 0f);
					if (EditorGUI.EndChangeCheck())
						EditorPrefs.SetFloat(DEFAULT_ZSPACING_KEY, defaultZSpacing);

					SpineEditorUtilities.BoolPrefsField(ref defaultInstantiateLoop, DEFAULT_INSTANTIATE_LOOP_KEY, new GUIContent("Default Loop", "Spawn Spine GameObjects with loop enabled."));
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Handles and Gizmos", EditorStyles.boldLabel);
				{
					EditorGUI.BeginChangeCheck();
					SpineHandles.handleScale = EditorGUILayout.Slider("Editor Bone Scale", SpineHandles.handleScale, 0.01f, 2f);
					SpineHandles.handleScale = Mathf.Max(0.01f, SpineHandles.handleScale);
					if (EditorGUI.EndChangeCheck()) {
						EditorPrefs.SetFloat(SCENE_ICONS_SCALE_KEY, SpineHandles.handleScale);
						SceneView.RepaintAll();
					}
				}
				
#if NEW_TIMELINE_AS_PACKAGE
				GUILayout.Space(20);
				EditorGUILayout.LabelField("Timeline Support", EditorStyles.boldLabel);
				using (new GUILayout.HorizontalScope()) {
					EditorGUILayout.PrefixLabel("Timeline Package Support");

					var requestState = SpineEditorUtilities.SpinePackageDependencyUtility.HandlePendingAsyncTimelineRequest();
					using (new EditorGUI.DisabledGroupScope(requestState != SpineEditorUtilities.SpinePackageDependencyUtility.RequestState.NoRequestIssued)) {
						if (GUILayout.Button("Enable", GUILayout.Width(64)))
							SpineEditorUtilities.SpinePackageDependencyUtility.EnableTimelineSupport();
						if (GUILayout.Button("Disable", GUILayout.Width(64)))
							SpineEditorUtilities.SpinePackageDependencyUtility.DisableTimelineSupport();
					}
				}
#endif

				GUILayout.Space(20);
				EditorGUILayout.LabelField("3rd Party Settings", EditorStyles.boldLabel);
				using (new GUILayout.HorizontalScope()) {
					EditorGUILayout.PrefixLabel("Define TK2D");
					if (GUILayout.Button("Enable", GUILayout.Width(64)))
						SpineTK2DEditorUtility.EnableTK2D();
					if (GUILayout.Button("Disable", GUILayout.Width(64)))
						SpineTK2DEditorUtility.DisableTK2D();
				}
			}
		}

		static void BoolPrefsField (ref bool currentValue, string editorPrefsKey, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			currentValue = EditorGUILayout.Toggle(label, currentValue);
			if (EditorGUI.EndChangeCheck())
				EditorPrefs.SetBool(editorPrefsKey, currentValue);
		}

		static void FloatPrefsField (ref float currentValue, string editorPrefsKey, GUIContent label, float min = float.NegativeInfinity, float max = float.PositiveInfinity) {
			EditorGUI.BeginChangeCheck();
			currentValue = EditorGUILayout.DelayedFloatField(label, currentValue);
			if (EditorGUI.EndChangeCheck()) {
				currentValue = Mathf.Clamp(currentValue, min, max);
				EditorPrefs.SetFloat(editorPrefsKey, currentValue);
			}
		}



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
				foreach (var sg in activeSkeletonGraphics) {
					var skeletonDataAsset = sg.skeletonDataAsset;
					if (skeletonDataAsset != null) {
						var assetPath = AssetDatabase.GetAssetPath(skeletonDataAsset);
						var sgID = sg.GetInstanceID();
						savedSkeletonDataAssetAtSKeletonGraphicID[sgID] = assetPath;
						skeletonDataAssetsToReload.Add(skeletonDataAsset);
					}
				}

				foreach (var sda in skeletonDataAssetsToReload) {
					sda.Clear();
					sda.GetSkeletonData(true);
				}

				foreach (var sr in activeSkeletonRenderers) {
					var meshRenderer = sr.GetComponent<MeshRenderer>();
					var sharedMaterials = meshRenderer.sharedMaterials;
					foreach (var m in sharedMaterials) {
						if (m == null) {
							sr.Initialize(true);
							break;
						}
					}
				}

				foreach (var sg in activeSkeletonGraphics) {
					if (sg.mainTexture == null)
						sg.Initialize(true);
				}
			}

			public static void ReloadSceneSkeletonComponents (SkeletonDataAsset skeletonDataAsset) {
				if (EditorApplication.isPaused) return;
				if (EditorApplication.isPlaying) return;
				if (EditorApplication.isCompiling) return;
				if (EditorApplication.isPlayingOrWillChangePlaymode) return;

				var activeSkeletonRenderers = GameObject.FindObjectsOfType<SkeletonRenderer>();
				foreach (var sr in activeSkeletonRenderers) {
					if (sr.isActiveAndEnabled && sr.skeletonDataAsset == skeletonDataAsset) sr.Initialize(true);
				}

				var activeSkeletonGraphics = GameObject.FindObjectsOfType<SkeletonGraphic>();
				foreach (var sg in activeSkeletonGraphics) {
					if (sg.isActiveAndEnabled && sg.skeletonDataAsset == skeletonDataAsset) sg.Initialize(true);
				}
			}
		}

		public static class AssetUtility {
			public const string SkeletonDataSuffix = "_SkeletonData";
			public const string AtlasSuffix = "_Atlas";

			static readonly int[][] compatibleBinaryVersions = { new[] { 3, 7, 0 } };
			static readonly int[][] compatibleJsonVersions = { new[] { 3, 7, 0 }, new[] { 3, 6, 0 }, new[] { 3, 5, 0 } };
			//static bool isFixVersionRequired = false;

			/// HACK: This list keeps the asset reference temporarily during importing.
			///
			/// In cases of very large projects/sufficient RAM pressure, when AssetDatabase.SaveAssets is called,
			/// Unity can mistakenly unload assets whose references are only on the stack.
			/// This leads to MissingReferenceException and other errors.
			public static readonly List<ScriptableObject> protectFromStackGarbageCollection = new List<ScriptableObject>();
			public static HashSet<string> assetsImportedInWrongState = new HashSet<string>();

			public static void HandleOnPostprocessAllAssets (string[] imported) {
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
				AssetUtility.assetsImportedInWrongState.UnionWith(imported);
				if (AssetDatabaseAvailabilityDetector.IsAssetDatabaseAvailable()) {
					string[] combinedAssets = AssetUtility.assetsImportedInWrongState.ToArray();
					AssetUtility.assetsImportedInWrongState.Clear();
					AssetUtility.ImportSpineContent(combinedAssets);
				}
			}

#region Match SkeletonData with Atlases
			static readonly AttachmentType[] AtlasTypes = { AttachmentType.Region, AttachmentType.Linkedmesh, AttachmentType.Mesh };

			public static List<string> GetRequiredAtlasRegions (string skeletonDataPath) {
				List<string> requiredPaths = new List<string>();

				if (skeletonDataPath.Contains(".skel")) {
					AddRequiredAtlasRegionsFromBinary(skeletonDataPath, requiredPaths);
					return requiredPaths;
				}

				TextReader reader = null;
				TextAsset spineJson = AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonDataPath);
				Dictionary<string, object> root = null;
				try {
					if (spineJson != null) {
						reader = new StringReader(spineJson.text);
					}
					else {
						// On a "Reimport All" the order of imports can be wrong, thus LoadAssetAtPath() above could return null.
						// as a workaround, we provide a fallback reader.
						reader = new StreamReader(skeletonDataPath);
					}
					root = Json.Deserialize(reader) as Dictionary<string, object>;
				}
				finally {
					if (reader != null)
						reader.Dispose();
				}

				if (root == null || !root.ContainsKey("skins"))
					return requiredPaths;

				foreach (var skin in (Dictionary<string, object>)root["skins"]) {
					foreach (var slot in (Dictionary<string, object>)skin.Value) {

						foreach (var attachment in ((Dictionary<string, object>)slot.Value)) {
							var data = ((Dictionary<string, object>)attachment.Value);

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
								requiredPaths.Add(attachment.Key);
						}
					}
				}

				return requiredPaths;
			}

			internal static void AddRequiredAtlasRegionsFromBinary (string skeletonDataPath, List<string> requiredPaths) {
				SkeletonBinary binary = new SkeletonBinary(new AtlasRequirementLoader(requiredPaths));
				Stream input = null;
				TextAsset data = AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonDataPath);
				try {
					if (data != null) {
						input = new MemoryStream(data.bytes);
					}
					else {
						// On a "Reimport All" the order of imports can be wrong, thus LoadAssetAtPath() above could return null.
						// as a workaround, we provide a fallback reader.
						input = File.Open(skeletonDataPath, FileMode.Open, FileAccess.Read);
					}
					binary.ReadSkeletonData(input);
				}
				finally {
					if (input != null)
						input.Dispose();
				}
				binary = null;
			}

			internal static AtlasAssetBase GetMatchingAtlas (List<string> requiredPaths, List<AtlasAssetBase> atlasAssets) {
				AtlasAssetBase atlasAssetMatch = null;

				foreach (AtlasAssetBase a in atlasAssets) {
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

			public static void ImportSpineContent (string[] imported, bool reimport = false) {
				var atlasPaths = new List<string>();
				var imagePaths = new List<string>();
				var skeletonPaths = new List<string>();

				foreach (string str in imported) {
					string extension = Path.GetExtension(str).ToLower();
					switch (extension) {
						case ".atlas":
							if (SpineEditorUtilities.Preferences.atlasTxtImportWarning) {
								Debug.LogWarningFormat("`{0}` : If this file is a Spine atlas, please change its extension to `.atlas.txt`. This is to allow Unity to recognize it and avoid filename collisions. You can also set this file extension when exporting from the Spine editor.", str);
							}
							break;
						case ".txt":
							if (str.EndsWith(".atlas.txt", System.StringComparison.Ordinal))
								atlasPaths.Add(str);
							break;
						case ".png":
						case ".jpg":
							imagePaths.Add(str);
							break;
						case ".json":
							var jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(str);
							if (jsonAsset != null && IsSpineData(jsonAsset))
								skeletonPaths.Add(str);
							break;
						case ".bytes":
							if (str.ToLower().EndsWith(".skel.bytes", System.StringComparison.Ordinal)) {
								if (IsSpineData(AssetDatabase.LoadAssetAtPath<TextAsset>(str)))
									skeletonPaths.Add(str);
							}
							break;
					}
				}

				// Import atlases first.
				var atlases = new List<AtlasAssetBase>();
				foreach (string ap in atlasPaths) {
					TextAsset atlasText = AssetDatabase.LoadAssetAtPath<TextAsset>(ap);
					AtlasAssetBase atlas = IngestSpineAtlas(atlasText);
					atlases.Add(atlas);
				}

				// Import skeletons and match them with atlases.
				bool abortSkeletonImport = false;
				foreach (string skeletonPath in skeletonPaths) {
					if (!reimport && CheckForValidSkeletonData(skeletonPath)) {
						ReloadSkeletonData(skeletonPath);
						continue;
					}

					string dir = Path.GetDirectoryName(skeletonPath);

#if SPINE_TK2D
					IngestSpineProject(AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonPath), null);
#else
					var localAtlases = FindAtlasesAtPath(dir);
					var requiredPaths = GetRequiredAtlasRegions(skeletonPath);
					var atlasMatch = GetMatchingAtlas(requiredPaths, localAtlases);
					if (atlasMatch != null || requiredPaths.Count == 0) {
						IngestSpineProject(AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonPath), atlasMatch);
					} else {
						SkeletonImportDialog(skeletonPath, localAtlases, requiredPaths, ref abortSkeletonImport);
					}

					if (abortSkeletonImport)
						break;
#endif
				}

				SkeletonDataAssetInspector[] skeletonDataInspectors = Resources.FindObjectsOfTypeAll<SkeletonDataAssetInspector>();
				foreach (var inspector in skeletonDataInspectors) {
					inspector.UpdateSkeletonData();
				}
				
				// Any post processing of images

				// Under some circumstances (e.g. on first import) SkeletonGraphic objects 
				// have their skeletonGraphic.skeletonDataAsset reference corrupted
				// by the instance of the ScriptableObject being destroyed but still assigned.
				// Here we restore broken skeletonGraphic.skeletonDataAsset references.
				var skeletonGraphicObjects = Resources.FindObjectsOfTypeAll(typeof(SkeletonGraphic)) as SkeletonGraphic[];
				foreach (var skeletonGraphic in skeletonGraphicObjects) {
					
					if (skeletonGraphic.skeletonDataAsset == null) {
						var skeletonGraphicID = skeletonGraphic.GetInstanceID();
						if (DataReloadHandler.savedSkeletonDataAssetAtSKeletonGraphicID.ContainsKey(skeletonGraphicID)) {
							string assetPath = DataReloadHandler.savedSkeletonDataAssetAtSKeletonGraphicID[skeletonGraphicID];
							skeletonGraphic.skeletonDataAsset = (SkeletonDataAsset)AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(assetPath);
						}
					}
				}
			}
			
			static void ReloadSkeletonData (string skeletonJSONPath) {
				string dir = Path.GetDirectoryName(skeletonJSONPath);
				TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonJSONPath);
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
							AtlasAssetBase[] skeletonDataAtlasAssets = skeletonDataAsset.atlasAssets;
							if (skeletonDataAtlasAssets != null) {
								for (int i = 0; i < skeletonDataAtlasAssets.Length; i++) {
									if (!ReferenceEquals(null, skeletonDataAtlasAssets[i]) &&
										skeletonDataAtlasAssets[i].Equals(null) &&
										skeletonDataAtlasAssets[i].GetInstanceID() != 0
									) {
										skeletonDataAtlasAssets[i] = EditorUtility.InstanceIDToObject(skeletonDataAtlasAssets[i].GetInstanceID()) as AtlasAssetBase;
									}
								}
							}

							SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(true);
							string currentHash = skeletonData != null ? skeletonData.Hash : null;

#if SPINE_SKELETONMECANIM
							if (currentHash == null || lastHash != currentHash)
								SkeletonBaker.UpdateMecanimClips(skeletonDataAsset);
#endif

							// if (currentHash == null || lastHash != currentHash)
							// Do any upkeep on synchronized assets

							if (currentHash != null)
								EditorPrefs.SetString(guid + "_hash", currentHash);
						}
						DataReloadHandler.ReloadSceneSkeletonComponents(skeletonDataAsset);
					}
				}
			}

#region Import Atlases
			static List<AtlasAssetBase> FindAtlasesAtPath (string path) {
				List<AtlasAssetBase> arr = new List<AtlasAssetBase>();
				DirectoryInfo dir = new DirectoryInfo(path);
				FileInfo[] assetInfoArr = dir.GetFiles("*.asset");

				int subLen = Application.dataPath.Length - 6;
				foreach (var f in assetInfoArr) {
					string assetRelativePath = f.FullName.Substring(subLen, f.FullName.Length - subLen).Replace("\\", "/");
					Object obj = AssetDatabase.LoadAssetAtPath(assetRelativePath, typeof(AtlasAssetBase));
					if (obj != null)
						arr.Add(obj as AtlasAssetBase);
				}

				return arr;
			}

			static AtlasAssetBase IngestSpineAtlas (TextAsset atlasText) {
				if (atlasText == null) {
					Debug.LogWarning("Atlas source cannot be null!");
					return null;
				}

				string primaryName = Path.GetFileNameWithoutExtension(atlasText.name).Replace(".atlas", "");
				string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(atlasText));

				string atlasPath = assetPath + "/" + primaryName + AtlasSuffix + ".asset";

				SpineAtlasAsset atlasAsset = (SpineAtlasAsset)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(SpineAtlasAsset));

				List<Material> vestigialMaterials = new List<Material>();

				if (atlasAsset == null)
					atlasAsset = SpineAtlasAsset.CreateInstance<SpineAtlasAsset>();
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

				var populatingMaterials = new List<Material>(pageFiles.Count);//atlasAsset.materials = new Material[pageFiles.Count];

				for (int i = 0; i < pageFiles.Count; i++) {
					string texturePath = assetPath + "/" + pageFiles[i];
					Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));

					if (SpineEditorUtilities.Preferences.setTextureImporterSettings) {
						TextureImporter texImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
						if (texImporter == null) {
							Debug.LogWarning(string.Format("{0} ::: Texture asset \"{1}\" not found. Skipping. Please check your atlas file for renamed files.", atlasAsset.name, texturePath));
							continue;
						}

						texImporter.textureCompression = TextureImporterCompression.Uncompressed;
						texImporter.alphaSource = TextureImporterAlphaSource.FromInput;
						texImporter.mipmapEnabled = false;
						texImporter.alphaIsTransparency = false; // Prevent the texture importer from applying bleed to the transparent parts for PMA.
						texImporter.spriteImportMode = SpriteImportMode.None;
						texImporter.maxTextureSize = 2048;

						EditorUtility.SetDirty(texImporter);
						AssetDatabase.ImportAsset(texturePath);
						AssetDatabase.SaveAssets();
					}

					string pageName = Path.GetFileNameWithoutExtension(pageFiles[i]);

					//because this looks silly
					if (pageName == primaryName && pageFiles.Count == 1)
						pageName = "Material";

					string materialPath = assetPath + "/" + primaryName + "_" + pageName + ".mat";
					Material mat = (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));

					if (mat == null) {
						mat = new Material(Shader.Find(SpineEditorUtilities.Preferences.defaultShader));
						AssetDatabase.CreateAsset(mat, materialPath);
					} else {
						vestigialMaterials.Remove(mat);
					}

					mat.mainTexture = texture;
					EditorUtility.SetDirty(mat);
					AssetDatabase.SaveAssets();

					populatingMaterials.Add(mat); //atlasAsset.materials[i] = mat;
				}

				atlasAsset.materials = populatingMaterials.ToArray();

				for (int i = 0; i < vestigialMaterials.Count; i++)
					AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(vestigialMaterials[i]));

				if (AssetDatabase.GetAssetPath(atlasAsset) == "")
					AssetDatabase.CreateAsset(atlasAsset, atlasPath);
				else
					atlasAsset.Clear();

				EditorUtility.SetDirty(atlasAsset);
				AssetDatabase.SaveAssets();

				if (pageFiles.Count != atlasAsset.materials.Length)
					Debug.LogWarning(string.Format("{0} :: Not all atlas pages were imported. If you rename your image files, please make sure you also edit the filenames specified in the atlas file.", atlasAsset.name));
				else
					Debug.Log(string.Format("{0} :: Imported with {1} material", atlasAsset.name, atlasAsset.materials.Length));

				// Iterate regions and bake marked.
				Atlas atlas = atlasAsset.GetAtlas();
				if (atlas != null) {
					FieldInfo field = typeof(Atlas).GetField("regions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic);
					var regions = (List<AtlasRegion>)field.GetValue(atlas);
					string atlasAssetPath = AssetDatabase.GetAssetPath(atlasAsset);
					string atlasAssetDirPath = Path.GetDirectoryName(atlasAssetPath);
					string bakedDirPath = Path.Combine(atlasAssetDirPath, atlasAsset.name);

					bool hasBakedRegions = false;
					for (int i = 0; i < regions.Count; i++) {
						AtlasRegion region = regions[i];
						string bakedPrefabPath = Path.Combine(bakedDirPath, SpineEditorUtilities.AssetUtility.GetPathSafeName(region.name) + ".prefab").Replace("\\", "/");
						GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(bakedPrefabPath, typeof(GameObject));
						if (prefab != null) {
							SkeletonBaker.BakeRegion(atlasAsset, region, false);
							hasBakedRegions = true;
						}
					}

					if (hasBakedRegions) {
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
					}
				}

				protectFromStackGarbageCollection.Remove(atlasAsset);
				return (AtlasAssetBase)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(AtlasAssetBase));
			}
#endregion

#region Import SkeletonData (json or binary)
			internal static SkeletonDataAsset IngestSpineProject (TextAsset spineJson, params AtlasAssetBase[] atlasAssets) {
				string primaryName = Path.GetFileNameWithoutExtension(spineJson.name);
				string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(spineJson));
				string filePath = assetPath + "/" + primaryName + SkeletonDataSuffix + ".asset";

#if SPINE_TK2D
				if (spineJson != null) {
					SkeletonDataAsset skeletonDataAsset = (SkeletonDataAsset)AssetDatabase.LoadAssetAtPath(filePath, typeof(SkeletonDataAsset));
					if (skeletonDataAsset == null) {
						skeletonDataAsset = SkeletonDataAsset.CreateInstance<SkeletonDataAsset>();
						skeletonDataAsset.skeletonJSON = spineJson;
						skeletonDataAsset.fromAnimation = new string[0];
						skeletonDataAsset.toAnimation = new string[0];
						skeletonDataAsset.duration = new float[0];
						skeletonDataAsset.defaultMix = SpineEditorUtilities.Preferences.defaultMix;
						skeletonDataAsset.scale = SpineEditorUtilities.Preferences.defaultScale;

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
						skeletonDataAsset = ScriptableObject.CreateInstance<SkeletonDataAsset>();
						{
							skeletonDataAsset.atlasAssets = atlasAssets;
							skeletonDataAsset.skeletonJSON = spineJson;
							skeletonDataAsset.defaultMix = SpineEditorUtilities.Preferences.defaultMix;
							skeletonDataAsset.scale = SpineEditorUtilities.Preferences.defaultScale;
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

#region Spine Skeleton Data File Validation
			public static bool CheckForValidSkeletonData (string skeletonJSONPath) {
				string dir = Path.GetDirectoryName(skeletonJSONPath);
				TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonJSONPath);
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
				if (asset == null)
					return false;

				bool isSpineData = false;
				string rawVersion = null;

				int[][] compatibleVersions;
				if (asset.name.Contains(".skel")) {
					try {
						using (var memStream = new MemoryStream(asset.bytes)) {
							rawVersion = SkeletonBinary.GetVersionString(memStream);
						}
						isSpineData = !(string.IsNullOrEmpty(rawVersion));
						compatibleVersions = compatibleBinaryVersions;
					} catch (System.Exception e) {
						Debug.LogErrorFormat("Failed to read '{0}'. It is likely not a binary Spine SkeletonData file.\n{1}", asset.name, e);
						return false;
					}
				} else {
					object obj = Json.Deserialize(new StringReader(asset.text));
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

					compatibleVersions = compatibleJsonVersions;
				}

				// Version warning
				if (isSpineData) {
					string primaryRuntimeVersionDebugString = compatibleVersions[0][0] + "." + compatibleVersions[0][1];

					if (string.IsNullOrEmpty(rawVersion)) {
						Debug.LogWarningFormat("Skeleton '{0}' has no version information. It may be incompatible with your runtime version: spine-unity v{1}", asset.name, primaryRuntimeVersionDebugString);
					} else {
						string[] versionSplit = rawVersion.Split('.');
						bool match = false;
						foreach (var version in compatibleVersions) {
							bool primaryMatch = version[0] == int.Parse(versionSplit[0], CultureInfo.InvariantCulture);
							bool secondaryMatch = version[1] == int.Parse(versionSplit[1], CultureInfo.InvariantCulture);

							// if (isFixVersionRequired) secondaryMatch &= version[2] <= int.Parse(jsonVersionSplit[2], CultureInfo.InvariantCulture);

							if (primaryMatch && secondaryMatch) {
								match = true;
								break;
							}
						}

						if (!match)
							Debug.LogWarningFormat("Skeleton '{0}' (exported with Spine {1}) may be incompatible with your runtime version: spine-csharp v{2}", asset.name, rawVersion, primaryRuntimeVersionDebugString);
					}
				}

				return isSpineData;
			}
#endregion

#region Dialogs
			public static void SkeletonImportDialog (string skeletonPath, List<AtlasAssetBase> localAtlases, List<string> requiredPaths, ref bool abortSkeletonImport) {
				bool resolved = false;
				while (!resolved) {

					string filename = Path.GetFileNameWithoutExtension(skeletonPath);
					int result = EditorUtility.DisplayDialogComplex(
						string.Format("AtlasAsset for \"{0}\"", filename),
						string.Format("Could not automatically set the AtlasAsset for \"{0}\".\n\n (You may resolve this manually later.)", filename),
						"Resolve atlases...", "Import without atlases", "Stop importing"
					);

					switch (result) {
						case -1:
							//Debug.Log("Select Atlas");
							AtlasAssetBase selectedAtlas = BrowseAtlasDialog(Path.GetDirectoryName(skeletonPath));
							if (selectedAtlas != null) {
								localAtlases.Clear();
								localAtlases.Add(selectedAtlas);
								var atlasMatch = AssetUtility.GetMatchingAtlas(requiredPaths, localAtlases);
								if (atlasMatch != null) {
									resolved = true;
									AssetUtility.IngestSpineProject(AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonPath), atlasMatch);
								}
							}
							break;
						case 0: // Resolve AtlasAssets...
							var atlasList = MultiAtlasDialog(requiredPaths, Path.GetDirectoryName(skeletonPath), Path.GetFileNameWithoutExtension(skeletonPath));
							if (atlasList != null)
								AssetUtility.IngestSpineProject(AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonPath), atlasList.ToArray());

							resolved = true;
							break;
						case 1: // Import without atlas
							Debug.LogWarning("Imported with missing atlases. Skeleton will not render: " + Path.GetFileName(skeletonPath));
							AssetUtility.IngestSpineProject(AssetDatabase.LoadAssetAtPath<TextAsset>(skeletonPath), new AtlasAssetBase[] { });
							resolved = true;
							break;
						case 2: // Stop importing all
							abortSkeletonImport = true;
							resolved = true;
							break;
					}
				}
			}

			public static List<AtlasAssetBase> MultiAtlasDialog (List<string> requiredPaths, string initialDirectory, string filename = "") {
				List<AtlasAssetBase> atlasAssets = new List<AtlasAssetBase>();
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
						if (n == 0)
							break;

						const int MaxListLength = 15;
						for (int i = 0; (i < n && i < MaxListLength); i++)
							dialogText.AppendLine(string.Format("\t {0}", missingRegions[i]));

						if (n > MaxListLength)
							dialogText.AppendLine(string.Format("\t... {0} more...", n - MaxListLength));
					}

					// Show dialog box.
					int result = EditorUtility.DisplayDialogComplex(
						"SkeletonDataAsset has missing Atlas.",
						dialogText.ToString(),
						"Browse Atlas...", "Import anyway", "Cancel import"
					);

					switch (result) {
						case 0: // Browse...
							AtlasAssetBase selectedAtlasAsset = BrowseAtlasDialog(lastAtlasPath);
							if (selectedAtlasAsset != null) {
								if (!atlasAssets.Contains(selectedAtlasAsset)) {
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

			public static AtlasAssetBase BrowseAtlasDialog (string dirPath) {
				string path = EditorUtility.OpenFilePanel("Select AtlasAsset...", dirPath, "asset");
				if (path == "")
					return null; // Canceled or closed by user.

				int subLen = Application.dataPath.Length - 6;
				string assetRelativePath = path.Substring(subLen, path.Length - subLen).Replace("\\", "/");

				var obj = AssetDatabase.LoadAssetAtPath(assetRelativePath, typeof(AtlasAssetBase));
				if (obj == null || !(obj is AtlasAssetBase)) {
					Debug.Log("Chosen asset was not of type AtlasAssetBase");
					return null;
				}

				return (AtlasAssetBase)obj;
			}
#endregion

			public static string GetPathSafeName (string name) {
				foreach (char c in System.IO.Path.GetInvalidFileNameChars()) { // Doesn't handle more obscure file name limitations.
					name = name.Replace(c, '_');
				}
				return name;
			}
		}

		public static class EditorInstantiation {
			public delegate Component InstantiateDelegate (SkeletonDataAsset skeletonDataAsset);

			public class SkeletonComponentSpawnType {
				public string menuLabel;
				public InstantiateDelegate instantiateDelegate;
				public bool isUI;
			}

			internal static readonly List<SkeletonComponentSpawnType> additionalSpawnTypes = new List<SkeletonComponentSpawnType>();

			public static void TryInitializeSkeletonRendererSettings (SkeletonRenderer skeletonRenderer, Skin skin = null) {
				const string PMAShaderQuery = "Spine/Skeleton";
				const string TintBlackShaderQuery = "Tint Black";

				if (skeletonRenderer == null) return;
				var skeletonDataAsset = skeletonRenderer.skeletonDataAsset;
				if (skeletonDataAsset == null) return;

				bool pmaVertexColors = false;
				bool tintBlack = false;
				foreach (AtlasAssetBase atlasAsset in skeletonDataAsset.atlasAssets) {
					if (!pmaVertexColors) {
						foreach (Material m in atlasAsset.Materials) {
							if (m.shader.name.Contains(PMAShaderQuery)) {
								pmaVertexColors = true;
								break;
							}
						}
					}

					if (!tintBlack) {
						foreach (Material m in atlasAsset.Materials) {
							if (m.shader.name.Contains(TintBlackShaderQuery)) {
								tintBlack = true;
								break;
							}
						}
					}
				}

				skeletonRenderer.pmaVertexColors = pmaVertexColors;
				skeletonRenderer.tintBlack = tintBlack;
				skeletonRenderer.zSpacing = SpineEditorUtilities.Preferences.defaultZSpacing;

				var data = skeletonDataAsset.GetSkeletonData(false);
				bool noSkins = data.DefaultSkin == null && (data.Skins == null || data.Skins.Count == 0); // Support attachmentless/skinless SkeletonData.
				skin = skin ?? data.DefaultSkin ?? (noSkins ? null : data.Skins.Items[0]);
				if (skin != null && skin != data.DefaultSkin) {
					skeletonRenderer.initialSkinName = skin.Name;
				}
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
						skeletonDataAsset.atlasAssets[i] = (AtlasAssetBase)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAssetBase));
					}
					data = skeletonDataAsset.GetSkeletonData(false);
				}

				if (data == null) {
					Debug.LogWarning("InstantiateSkeletonAnimation tried to instantiate a skeleton from an invalid SkeletonDataAsset.");
					return null;
				}

				string spineGameObjectName = string.Format("Spine GameObject ({0})", skeletonDataAsset.name.Replace("_SkeletonData", ""));
				GameObject go = EditorInstantiation.NewGameObject(spineGameObjectName, typeof(MeshFilter), typeof(MeshRenderer), typeof(SkeletonAnimation));
				SkeletonAnimation newSkeletonAnimation = go.GetComponent<SkeletonAnimation>();
				newSkeletonAnimation.skeletonDataAsset = skeletonDataAsset;
				TryInitializeSkeletonRendererSettings(newSkeletonAnimation, skin);

				// Initialize
				try {
					newSkeletonAnimation.Initialize(false);
				} catch (System.Exception e) {
					if (destroyInvalid) {
						Debug.LogWarning("Editor-instantiated SkeletonAnimation threw an Exception. Destroying GameObject to prevent orphaned GameObject.");
						GameObject.DestroyImmediate(go);
					}
					throw e;
				}

				newSkeletonAnimation.loop = SpineEditorUtilities.Preferences.defaultInstantiateLoop;
				newSkeletonAnimation.skeleton.Update(0);
				newSkeletonAnimation.state.Update(0);
				newSkeletonAnimation.state.Apply(newSkeletonAnimation.skeleton);
				newSkeletonAnimation.skeleton.UpdateWorldTransform();

				return newSkeletonAnimation;
			}
			
			/// <summary>Handles creating a new GameObject in the Unity Editor. This uses the new ObjectFactory API where applicable.</summary>
			public static GameObject NewGameObject (string name) {
#if NEW_PREFAB_SYSTEM
				return ObjectFactory.CreateGameObject(name);
#else
				return new GameObject(name);
#endif
			}

			/// <summary>Handles creating a new GameObject in the Unity Editor. This uses the new ObjectFactory API where applicable.</summary>
			public static GameObject NewGameObject (string name, params System.Type[] components) {
#if NEW_PREFAB_SYSTEM
				return ObjectFactory.CreateGameObject(name, components);
#else
				return new GameObject(name, components);
#endif
			}

			public static void InstantiateEmptySpineGameObject<T> (string name) where T : MonoBehaviour {
				var parentGameObject = Selection.activeObject as GameObject;
				var parentTransform = parentGameObject == null ? null : parentGameObject.transform;

				var gameObject = EditorInstantiation.NewGameObject(name, typeof(T));
				gameObject.transform.SetParent(parentTransform, false);
				EditorUtility.FocusProjectWindow();
				Selection.activeObject = gameObject;
				EditorGUIUtility.PingObject(Selection.activeObject);
			}

#region SkeletonMecanim
#if SPINE_SKELETONMECANIM
			public static SkeletonMecanim InstantiateSkeletonMecanim (SkeletonDataAsset skeletonDataAsset, string skinName) {
				return InstantiateSkeletonMecanim(skeletonDataAsset, skeletonDataAsset.GetSkeletonData(true).FindSkin(skinName));
			}

			public static SkeletonMecanim InstantiateSkeletonMecanim (SkeletonDataAsset skeletonDataAsset, Skin skin = null, bool destroyInvalid = true) {
				SkeletonData data = skeletonDataAsset.GetSkeletonData(true);

				if (data == null) {
					for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++) {
						string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
						skeletonDataAsset.atlasAssets[i] = (AtlasAssetBase)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAssetBase));
					}
					data = skeletonDataAsset.GetSkeletonData(false);
				}

				if (data == null) {
					Debug.LogWarning("InstantiateSkeletonMecanim tried to instantiate a skeleton from an invalid SkeletonDataAsset.");
					return null;
				}

				string spineGameObjectName = string.Format("Spine Mecanim GameObject ({0})", skeletonDataAsset.name.Replace("_SkeletonData", ""));
				GameObject go = EditorInstantiation.NewGameObject(spineGameObjectName, typeof(MeshFilter), typeof(MeshRenderer), typeof(Animator), typeof(SkeletonMecanim));

				if (skeletonDataAsset.controller == null) {
					SkeletonBaker.GenerateMecanimAnimationClips(skeletonDataAsset);
					Debug.Log(string.Format("Mecanim controller was automatically generated and assigned for {0}", skeletonDataAsset.name));
				}

				go.GetComponent<Animator>().runtimeAnimatorController = skeletonDataAsset.controller;

				SkeletonMecanim newSkeletonMecanim = go.GetComponent<SkeletonMecanim>();
				newSkeletonMecanim.skeletonDataAsset = skeletonDataAsset;
				TryInitializeSkeletonRendererSettings(newSkeletonMecanim, skin);

				// Initialize
				try {
					newSkeletonMecanim.Initialize(false);
				} catch (System.Exception e) {
					if (destroyInvalid) {
						Debug.LogWarning("Editor-instantiated SkeletonAnimation threw an Exception. Destroying GameObject to prevent orphaned GameObject.");
						GameObject.DestroyImmediate(go);
					}
					throw e;
				}

				newSkeletonMecanim.skeleton.Update(0);
				newSkeletonMecanim.skeleton.UpdateWorldTransform();
				newSkeletonMecanim.LateUpdate();

				return newSkeletonMecanim;
			}
#endif
#endregion
		}

		public static class DragAndDropInstantiation {
			public struct SpawnMenuData {
				public Vector3 spawnPoint;
				public SkeletonDataAsset skeletonDataAsset;
				public EditorInstantiation.InstantiateDelegate instantiateDelegate;
				public bool isUI;
			}

			public static void SceneViewDragAndDrop (SceneView sceneview) {
				var current = UnityEngine.Event.current;
				var references = DragAndDrop.objectReferences;
				if (current.type == EventType.Layout)
					return;

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
							GUI.Label(new Rect(mousePos + new Vector2(20f, 20f), new Vector2(400f, 20f)), new GUIContent(string.Format("Create Spine GameObject ({0})", skeletonDataAsset.skeletonJSON.name), SpineEditorUtilities.Icons.skeletonDataAssetIcon));
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

			public static void ShowInstantiateContextMenu (SkeletonDataAsset skeletonDataAsset, Vector3 spawnPoint) {
				var menu = new GenericMenu();

				// SkeletonAnimation
				menu.AddItem(new GUIContent("SkeletonAnimation"), false, HandleSkeletonComponentDrop, new SpawnMenuData {
					skeletonDataAsset = skeletonDataAsset,
					spawnPoint = spawnPoint,
					instantiateDelegate = (data) => EditorInstantiation.InstantiateSkeletonAnimation(data),
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
							instantiateDelegate = System.Delegate.CreateDelegate(typeof(EditorInstantiation.InstantiateDelegate), graphicInstantiateDelegate) as EditorInstantiation.InstantiateDelegate,
							isUI = true
						});
				}

#if SPINE_SKELETONMECANIM
				menu.AddSeparator("");
				// SkeletonMecanim
				menu.AddItem(new GUIContent("SkeletonMecanim"), false, HandleSkeletonComponentDrop, new SpawnMenuData {
					skeletonDataAsset = skeletonDataAsset,
					spawnPoint = spawnPoint,
					instantiateDelegate = (data) => EditorInstantiation.InstantiateSkeletonMecanim(data)
				});
#endif

				menu.ShowAsContext();
			}

			public static void HandleSkeletonComponentDrop (object spawnMenuData) {
				var data = (SpawnMenuData)spawnMenuData;

				if (data.skeletonDataAsset.GetSkeletonData(true) == null) {
					EditorUtility.DisplayDialog("Invalid SkeletonDataAsset", "Unable to create Spine GameObject.\n\nPlease check your SkeletonDataAsset.", "Ok");
					return;
				}

				bool isUI = data.isUI;

				Component newSkeletonComponent = data.instantiateDelegate.Invoke(data.skeletonDataAsset);
				GameObject newGameObject = newSkeletonComponent.gameObject;
				Transform newTransform = newGameObject.transform;

				var activeGameObject = Selection.activeGameObject;
				if (isUI && activeGameObject != null)
					newTransform.SetParent(activeGameObject.transform, false);

				newTransform.position = isUI ? data.spawnPoint : RoundVector(data.spawnPoint, 2);

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
		}

		static class HierarchyHandler {
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

		internal static class SpineTK2DEditorUtility {
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

	public static class SpineHandles {
		internal static float handleScale = 1f;
		public static Color BoneColor { get { return new Color(0.8f, 0.8f, 0.8f, 0.4f); } }
		public static Color PathColor { get { return new Color(254/255f, 127/255f, 0); } }
		public static Color TransformContraintColor { get { return new Color(170/255f, 226/255f, 35/255f); } }
		public static Color IkColor { get { return new Color(228/255f,90/255f,43/255f); } }
		public static Color PointColor { get { return new Color(1f, 1f, 0f, 1f);  } }

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
					_boneNameStyle = new GUIStyle(EditorStyles.whiteMiniLabel) {
						alignment = TextAnchor.MiddleCenter,
						stretchWidth = true,
						padding = new RectOffset(0, 0, 0, 0),
						contentOffset = new Vector2(-5f, 0f)
					};
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

		static GUIStyle _pointNameStyle;
		public static GUIStyle PointNameStyle {
			get {
				if (_pointNameStyle == null) {
					_pointNameStyle = new GUIStyle(SpineHandles.BoneNameStyle);
					_pointNameStyle.normal.textColor = SpineHandles.PointColor;
				}
				return _pointNameStyle;
			}
		}

		public static void DrawBoneNames (Transform transform, Skeleton skeleton, float positionScale = 1f) {
			GUIStyle style = BoneNameStyle;
			foreach (Bone b in skeleton.Bones) {
				var pos = new Vector3(b.WorldX * positionScale, b.WorldY * positionScale, 0) + (new Vector3(b.A, b.C) * (b.Data.Length * 0.5f));
				pos = transform.TransformPoint(pos);
				Handles.Label(pos, b.Data.Name, style);
			}
		}

		public static void DrawBones (Transform transform, Skeleton skeleton, float positionScale = 1f) {
			float boneScale = 1.8f; // Draw the root bone largest;
			DrawCrosshairs2D(skeleton.Bones.Items[0].GetWorldPosition(transform), 0.08f, positionScale);

			foreach (Bone b in skeleton.Bones) {
				DrawBone(transform, b, boneScale, positionScale);
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
		public static void DrawBoneWireframe (Transform transform, Bone b, Color color, float skeletonRenderScale = 1f) {
			Handles.color = color;
			var pos = new Vector3(b.WorldX * skeletonRenderScale, b.WorldY * skeletonRenderScale, 0);
			float length = b.Data.Length;

			if (length > 0) {
				Quaternion rot = Quaternion.Euler(0, 0, b.WorldRotationX);
				Vector3 scale = Vector3.one * length * b.WorldScaleX * skeletonRenderScale;
				const float my = 1.5f;
				scale.y *= (SpineHandles.handleScale + 1) * 0.5f;
				scale.y = Mathf.Clamp(scale.x, -my * skeletonRenderScale, my * skeletonRenderScale);
				Handles.DrawPolyLine(GetBoneWireBuffer(transform.localToWorldMatrix * Matrix4x4.TRS(pos, rot, scale)));
				var wp = transform.TransformPoint(pos);
				DrawBoneCircle(wp, color, transform.forward, skeletonRenderScale);
			} else {
				var wp = transform.TransformPoint(pos);
				DrawBoneCircle(wp, color, transform.forward, skeletonRenderScale);
			}
		}

		public static void DrawBone (Transform transform, Bone b, float boneScale, float skeletonRenderScale = 1f) {
			var pos = new Vector3(b.WorldX * skeletonRenderScale, b.WorldY * skeletonRenderScale, 0);
			float length = b.Data.Length;
			if (length > 0) {
				Quaternion rot = Quaternion.Euler(0, 0, b.WorldRotationX);
				Vector3 scale = Vector3.one * length * b.WorldScaleX * skeletonRenderScale;
				const float my = 1.5f;
				scale.y *= (SpineHandles.handleScale + 1f) * 0.5f;
				scale.y = Mathf.Clamp(scale.x, -my * skeletonRenderScale, my * skeletonRenderScale);
				SpineHandles.GetBoneMaterial().SetPass(0);
				Graphics.DrawMeshNow(SpineHandles.BoneMesh, transform.localToWorldMatrix * Matrix4x4.TRS(pos, rot, scale));
			} else {
				var wp = transform.TransformPoint(pos);
				DrawBoneCircle(wp, SpineHandles.BoneColor, transform.forward, boneScale * skeletonRenderScale);
			}
		}

		public static void DrawBone (Transform transform, Bone b, float boneScale, Color color, float skeletonRenderScale = 1f) {
			var pos = new Vector3(b.WorldX * skeletonRenderScale, b.WorldY * skeletonRenderScale, 0);
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
				DrawBoneCircle(wp, color, transform.forward, boneScale * skeletonRenderScale);
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
			Handles.DotHandleCap(0, position, Quaternion.identity, size * HandleUtility.GetHandleSize(position), EventType.Ignore); //Handles.DotCap(0, position, Quaternion.identity, size * HandleUtility.GetHandleSize(position));
		}

		public static void DrawBoundingBoxes (Transform transform, Skeleton skeleton) {
			foreach (var slot in skeleton.Slots) {
				var bba = slot.Attachment as BoundingBoxAttachment;
				if (bba != null) SpineHandles.DrawBoundingBox(slot, bba, transform);
			}
		}

		public static void DrawBoundingBox (Slot slot, BoundingBoxAttachment box, Transform t) {
			if (box.Vertices.Length <= 2) return; // Handle cases where user creates a BoundingBoxAttachment but doesn't actually define it.

			var worldVerts = new float[box.WorldVerticesLength];
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

		public static void DrawPointAttachment (Bone bone, PointAttachment pointAttachment, Transform skeletonTransform) {
			if (bone == null) return;
			if (pointAttachment == null) return;

			Vector2 localPos;
			pointAttachment.ComputeWorldPosition(bone, out localPos.x, out localPos.y);
			float localRotation = pointAttachment.ComputeWorldRotation(bone);
			Matrix4x4 m = Matrix4x4.TRS(localPos, Quaternion.Euler(0, 0, localRotation), Vector3.one) * Matrix4x4.TRS(Vector3.right * 0.25f, Quaternion.identity, Vector3.one);

			DrawBoneCircle(skeletonTransform.TransformPoint(localPos), SpineHandles.PointColor, Vector3.back, 1.3f);
			DrawArrowhead(skeletonTransform.localToWorldMatrix * m);
		}

		public static void DrawConstraints (Transform transform, Skeleton skeleton, float skeletonRenderScale = 1f) {
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
				targetPos = targetBone.GetWorldPosition(transform, skeletonRenderScale);

				if (tc.TranslateMix > 0) {
					if (tc.TranslateMix != 1f) {
						Handles.color = handleColor;
						foreach (var b in tc.Bones) {
							pos = b.GetWorldPosition(transform, skeletonRenderScale);
							Handles.DrawDottedLine(targetPos, pos, Thickness);
						}
					}
					SpineHandles.DrawBoneCircle(targetPos, handleColor, normal, 1.3f * skeletonRenderScale);
					Handles.color = handleColor;
					SpineHandles.DrawCrosshairs(targetPos, 0.2f, targetBone.A, targetBone.B, targetBone.C, targetBone.D, transform, skeletonRenderScale);
				}
			}

			// IK Constraints
			handleColor = SpineHandles.IkColor;
			foreach (var ikc in skeleton.IkConstraints) {
				Bone targetBone = ikc.Target;
				targetPos = targetBone.GetWorldPosition(transform, skeletonRenderScale);
				var bones = ikc.Bones;
				active = ikc.Mix > 0;
				if (active) {
					pos = bones.Items[0].GetWorldPosition(transform, skeletonRenderScale);
					switch (bones.Count) {
					case 1: {
							Handles.color = handleColor;
							Handles.DrawLine(targetPos, pos);
							SpineHandles.DrawBoneCircle(targetPos, handleColor, normal);
							var m = bones.Items[0].GetMatrix4x4();
							m.m03 = targetBone.WorldX * skeletonRenderScale;
							m.m13 = targetBone.WorldY * skeletonRenderScale;
							SpineHandles.DrawArrowhead(transform.localToWorldMatrix * m);
							break;
						}
					case 2: {
							Bone childBone = bones.Items[1];
							Vector3 child = childBone.GetWorldPosition(transform, skeletonRenderScale);
							Handles.color = handleColor;
							Handles.DrawLine(child, pos);
							Handles.DrawLine(targetPos, child);
							SpineHandles.DrawBoneCircle(pos, handleColor, normal, 0.5f);
							SpineHandles.DrawBoneCircle(child, handleColor, normal, 0.5f);
							SpineHandles.DrawBoneCircle(targetPos, handleColor, normal);
							var m = childBone.GetMatrix4x4();
							m.m03 = targetBone.WorldX * skeletonRenderScale;
							m.m13 = targetBone.WorldY * skeletonRenderScale;
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
						SpineHandles.DrawBoneCircle(b.GetWorldPosition(transform, skeletonRenderScale), handleColor, normal, 1f * skeletonRenderScale);
			}
		}

		static void DrawCrosshairs2D (Vector3 position, float scale, float skeletonRenderScale = 1f) {
			scale *= SpineHandles.handleScale * skeletonRenderScale;
			Handles.DrawLine(position + new Vector3(-scale, 0), position + new Vector3(scale, 0));
			Handles.DrawLine(position + new Vector3(0, -scale), position + new Vector3(0, scale));
		}

		static void DrawCrosshairs (Vector3 position, float scale, float a, float b, float c, float d, Transform transform, float skeletonRenderScale = 1f) {
			scale *= SpineHandles.handleScale * skeletonRenderScale;

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

		static void DrawArrowhead (Vector3 pos, Quaternion worldQuaternion) {
			Graphics.DrawMeshNow(SpineHandles.ArrowheadMesh, pos, worldQuaternion, 0);
		}

		static void DrawArrowhead (Matrix4x4 m) {
			float s = SpineHandles.handleScale;
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
			float secondScale = firstScale - (Thickness * SpineHandles.handleScale * scale);

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
