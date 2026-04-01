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

#if UNITY_2017_2_OR_NEWER
#define NEWPLAYMODECALLBACKS
#endif

#if UNITY_2018_3_OR_NEWER
#define NEW_PREFERENCES_SETTINGS_PROVIDER
#endif

#if UNITY_2020_2_OR_NEWER
#define HAS_ON_POSTPROCESS_PREFAB
#endif

#if UNITY_2021_2_OR_NEWER
#define TEXT_ASSET_HAS_GET_DATA_BYTES
#endif

#if TEXT_ASSET_HAS_GET_DATA_BYTES
#define HAS_ANY_UNSAFE_OPTIONS
#endif

#if UNITY_2017_3_OR_NEWER
#define ALLOWS_CUSTOM_PROFILING
#endif

#if !SPINE_AUTO_UPGRADE_COMPONENTS_OFF
#define AUTO_UPGRADE_TO_43_COMPONENTS
#endif

using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	public class SpinePreferences : ScriptableObject {

		protected struct TextureWorkflowProperties {
			public SerializedProperty textureSettingsReference;
			public SerializedProperty blendModeMaterialAdditive;
			public SerializedProperty blendModeMaterialMultiply;
			public SerializedProperty blendModeMaterialScreen;

			public TextureWorkflowProperties (SerializedObject settings) {
				textureSettingsReference = settings.FindProperty("textureSettingsReference");
				blendModeMaterialAdditive = settings.FindProperty("blendModeMaterialAdditive");
				blendModeMaterialMultiply = settings.FindProperty("blendModeMaterialMultiply");
				blendModeMaterialScreen = settings.FindProperty("blendModeMaterialScreen");
			}
		}

#if NEW_PREFERENCES_SETTINGS_PROVIDER
		static int wasPreferencesDirCreated = 0;
		static int wasPreferencesAssetCreated = 0;
#endif

		public const string SPINE_SETTINGS_ASSET_PATH = "Assets/Editor/SpineSettings.asset";

		internal const float DEFAULT_DEFAULT_SCALE = 0.01f;
		public float defaultScale = DEFAULT_DEFAULT_SCALE;

		internal const float DEFAULT_DEFAULT_MIX = 0.2f;
		public float defaultMix = DEFAULT_DEFAULT_MIX;

		internal const string DEFAULT_DEFAULT_SHADER = "Spine/Skeleton";
		public string defaultShader = DEFAULT_DEFAULT_SHADER;
		public string DefaultShader {
			get { return !string.IsNullOrEmpty(defaultShader) ? defaultShader : DEFAULT_DEFAULT_SHADER; }
			set { defaultShader = value; }
		}

		internal const float DEFAULT_DEFAULT_ZSPACING = 0f;
		public float defaultZSpacing = DEFAULT_DEFAULT_ZSPACING;

		internal const bool DEFAULT_DEFAULT_INSTANTIATE_LOOP = true;
		public bool defaultInstantiateLoop = DEFAULT_DEFAULT_INSTANTIATE_LOOP;

		internal static readonly Vector2 DEFAULT_DEFAULT_PHYSICS_POSITION_INHERITANCE = Vector2.one;
		public Vector2 defaultPhysicsPositionInheritance = DEFAULT_DEFAULT_PHYSICS_POSITION_INHERITANCE;

		internal const float DEFAULT_DEFAULT_PHYSICS_ROTATION_INHERITANCE = 1f;
		public float defaultPhysicsRotationInheritance = DEFAULT_DEFAULT_PHYSICS_ROTATION_INHERITANCE;

		internal const bool DEFAULT_SHOW_HIERARCHY_ICONS = true;
		public bool showHierarchyIcons = DEFAULT_SHOW_HIERARCHY_ICONS;

		internal const bool DEFAULT_RELOAD_AFTER_PLAYMODE = true;
		public bool reloadAfterPlayMode = DEFAULT_RELOAD_AFTER_PLAYMODE;

		internal const bool DEFAULT_SET_TEXTUREIMPORTER_SETTINGS = true;
		public bool setTextureImporterSettings = DEFAULT_SET_TEXTUREIMPORTER_SETTINGS;

		internal const string DEFAULT_TEXTURE_SETTINGS_REFERENCE = "";
		public string textureSettingsReference = DEFAULT_TEXTURE_SETTINGS_REFERENCE;

#if HAS_ON_POSTPROCESS_PREFAB
		internal const bool DEFAULT_FIX_PREFAB_OVERRIDE_VIA_MESH_FILTER = false;
		public bool fixPrefabOverrideViaMeshFilter = DEFAULT_FIX_PREFAB_OVERRIDE_VIA_MESH_FILTER;

		internal const bool DEFAULT_REMOVE_PREFAB_PREVIEW_MESHES = false;
		public bool removePrefabPreviewMeshes = DEFAULT_REMOVE_PREFAB_PREVIEW_MESHES;
#endif

		public bool UsesPMAWorkflow {
			get {
				return IsPMAWorkflow(textureSettingsReference);
			}
		}
		public static bool IsPMAWorkflow (string textureSettingsReference) {
			if (textureSettingsReference == null)
				return true;
			string settingsReference = textureSettingsReference.ToLower();
			if (settingsReference.Contains("straight") || !settingsReference.Contains("pma"))
				return false;
			return true;
		}

		public bool ShowWorkflowMismatchDialog {
			get { return workflowMismatchDialog; }
			set { workflowMismatchDialog = value; }
		}

		public bool ShowSplitComponentChangeWarning {
			get { return splitComponentChangeWarning; }
			set {
				if (splitComponentChangeWarning == value) return;

				SerializedObject serializedSettings = new SerializedObject(this);
				SerializedProperty splitComponentChangeProperty = serializedSettings.FindProperty("splitComponentChangeWarning");
				splitComponentChangeProperty.boolValue = value;
				serializedSettings.ApplyModifiedProperties();
			}
		}

		internal const bool DEFAULT_APPLY_ADDITIVE_MATERIAL = false;
		public bool applyAdditiveMaterial = DEFAULT_APPLY_ADDITIVE_MATERIAL;

		public const string DEFAULT_TEXTURE_PRESET_STRAIGHT = "StraightAlphaPreset";
		public const string DEFAULT_TEXTURE_PRESET_PMA = "PMATexturePreset";
		public const string DEFAULT_TEXTURE_PRESET = DEFAULT_TEXTURE_PRESET_STRAIGHT;

		public const string DEFAULT_BLEND_MODE_MULTIPLY_MATERIAL_STRAIGHT = "SkeletonStraightMultiply";
		public const string DEFAULT_BLEND_MODE_SCREEN_MATERIAL_STRAIGHT = "SkeletonStraightScreen";
		public const string DEFAULT_BLEND_MODE_ADDITIVE_MATERIAL_STRAIGHT = "SkeletonStraightAdditive";

		public const string DEFAULT_BLEND_MODE_MULTIPLY_MATERIAL_PMA = "SkeletonPMAMultiply";
		public const string DEFAULT_BLEND_MODE_SCREEN_MATERIAL_PMA = "SkeletonPMAScreen";
		public const string DEFAULT_BLEND_MODE_ADDITIVE_MATERIAL_PMA = "SkeletonPMAAdditive";

		public const string DEFAULT_BLEND_MODE_MULTIPLY_MATERIAL = DEFAULT_BLEND_MODE_MULTIPLY_MATERIAL_STRAIGHT;
		public const string DEFAULT_BLEND_MODE_SCREEN_MATERIAL = DEFAULT_BLEND_MODE_SCREEN_MATERIAL_STRAIGHT;
		public const string DEFAULT_BLEND_MODE_ADDITIVE_MATERIAL = DEFAULT_BLEND_MODE_ADDITIVE_MATERIAL_STRAIGHT;

		public Material blendModeMaterialMultiply = null;
		public Material blendModeMaterialScreen = null;
		public Material blendModeMaterialAdditive = null;

		public string FindPathOfAsset (string assetName) {
			string typeSearchString = assetName;
			string[] guids = AssetDatabase.FindAssets(typeSearchString);
			if (guids.Length > 0) {
				return AssetDatabase.GUIDToAssetPath(guids[0]);
			}
			return null;
		}

		public Material BlendModeMaterialMultiply {
			get {
				if (blendModeMaterialMultiply == null) {
					string path = FindPathOfAsset(DEFAULT_BLEND_MODE_MULTIPLY_MATERIAL);
					blendModeMaterialMultiply = AssetDatabase.LoadAssetAtPath<Material>(path);
				}
				return blendModeMaterialMultiply;
			}
		}
		public Material BlendModeMaterialScreen {
			get {
				if (blendModeMaterialScreen == null) {
					string path = FindPathOfAsset(DEFAULT_BLEND_MODE_SCREEN_MATERIAL);
					blendModeMaterialScreen = AssetDatabase.LoadAssetAtPath<Material>(path);
				}
				return blendModeMaterialScreen;
			}
		}
		public Material BlendModeMaterialAdditive {
			get {
				if (blendModeMaterialAdditive == null) {
					string path = FindPathOfAsset(DEFAULT_BLEND_MODE_ADDITIVE_MATERIAL);
					blendModeMaterialAdditive = AssetDatabase.LoadAssetAtPath<Material>(path);
				}
				return blendModeMaterialAdditive;
			}
		}

		internal const bool DEFAULT_ATLASTXT_WARNING = true;
		public bool atlasTxtImportWarning = DEFAULT_ATLASTXT_WARNING;

		internal const bool DEFAULT_TEXTUREIMPORTER_WARNING = true;
		public bool textureImporterWarning = DEFAULT_TEXTUREIMPORTER_WARNING;

		internal const bool DEFAULT_COMPONENTMATERIAL_WARNING = true;
		public bool componentMaterialWarning = DEFAULT_COMPONENTMATERIAL_WARNING;

		internal const bool DEFAULT_SKELETONDATA_ASSET_NO_FILE_ERROR = true;
		public bool skeletonDataAssetNoFileError = DEFAULT_SKELETONDATA_ASSET_NO_FILE_ERROR;

		internal const bool DEFAULT_WORKFLOW_MISMATCH_DIALOG = true;
		public bool workflowMismatchDialog = DEFAULT_WORKFLOW_MISMATCH_DIALOG;

		internal const bool DEFAULT_SKELETONDATA_ASSET_MISMATCH_WARNING = true;
		public bool skeletonDataAssetMismatchWarning = DEFAULT_SKELETONDATA_ASSET_MISMATCH_WARNING;

		internal const bool DEFAULT_SPLIT_COMPONENT_CHANGE_WARNING = true;
		public bool splitComponentChangeWarning = DEFAULT_SPLIT_COMPONENT_CHANGE_WARNING;

		public const float DEFAULT_MIPMAPBIAS = -0.5f;

		public const bool DEFAULT_AUTO_RELOAD_SCENESKELETONS = true;
		public bool autoReloadSceneSkeletons = DEFAULT_AUTO_RELOAD_SCENESKELETONS;

		public const string SCENE_ICONS_SCALE_KEY = "SPINE_SCENE_ICONS_SCALE";
		internal const float DEFAULT_SCENE_ICONS_SCALE = 1f;
		[Range(0.01f, 2f)]
		public float handleScale = DEFAULT_SCENE_ICONS_SCALE;

		public const bool DEFAULT_MECANIM_EVENT_INCLUDE_FOLDERNAME = true;
		public bool mecanimEventIncludeFolderName = DEFAULT_MECANIM_EVENT_INCLUDE_FOLDERNAME;

		// Timeline extension module
		public const bool DEFAULT_TIMELINE_DEFAULT_MIX_DURATION = false;
		public bool timelineDefaultMixDuration = DEFAULT_TIMELINE_DEFAULT_MIX_DURATION;

		public const bool DEFAULT_TIMELINE_USE_BLEND_DURATION = true;
		public bool timelineUseBlendDuration = DEFAULT_TIMELINE_USE_BLEND_DURATION;

#if NEW_PREFERENCES_SETTINGS_PROVIDER
		public static void Load () {
			GetOrCreateSettings();
		}

		static SpinePreferences settings = null;

		internal static SpinePreferences GetOrCreateSettings () {
			if (settings != null)
				return settings;

			settings = AssetDatabase.LoadAssetAtPath<SpinePreferences>(SPINE_SETTINGS_ASSET_PATH);
			if (settings == null)
				settings = FindSpinePreferences();
			if (settings == null) {
				settings = ScriptableObject.CreateInstance<SpinePreferences>();
				SpineEditorUtilities.OldPreferences.CopyOldToNewPreferences(ref settings);
				// Multiple threads may be calling this method during import, creating the folder
				// multiple times with ascending number suffix. Atomic wasPreferencesDirCreated int
				// variable is used to prevent any redundant create operations.
				if (!AssetDatabase.IsValidFolder("Assets/Editor") && Interlocked.Exchange(ref wasPreferencesDirCreated, 1) == 0)
					AssetDatabase.CreateFolder("Assets", "Editor");
				if (Interlocked.Exchange(ref wasPreferencesAssetCreated, 1) == 0)
					AssetDatabase.CreateAsset(settings, SPINE_SETTINGS_ASSET_PATH);
			}

#if HAS_ON_POSTPROCESS_PREFAB
			SkeletonRenderer.fixPrefabOverrideViaMeshFilterGlobal = settings.fixPrefabOverrideViaMeshFilter;
#endif
			SkeletonDataAsset.errorIfSkeletonFileNullGlobal = settings.skeletonDataAssetNoFileError;
			return settings;
		}

		static SpinePreferences FindSpinePreferences () {
			string typeSearchString = " t:SpinePreferences";
			string[] guids = AssetDatabase.FindAssets(typeSearchString);
			foreach (string guid in guids) {
				string path = AssetDatabase.GUIDToAssetPath(guid);
				SpinePreferences preferences = AssetDatabase.LoadAssetAtPath<SpinePreferences>(path);
				if (preferences != null)
					return preferences;
			}
			return null;
		}

		private static void ShowBlendModeMaterialProperty (SerializedProperty blendModeMaterialProperty,
			string blendType, bool isTexturePresetPMA) {

			EditorGUILayout.PropertyField(blendModeMaterialProperty, new GUIContent(blendType + " Material", blendType + " blend mode Material template."));
			Material material = blendModeMaterialProperty.objectReferenceValue as Material;
			if (material == null)
				return;

			bool isMaterialPMA = MaterialChecks.IsPMATextureMaterial(material);
			if (!isTexturePresetPMA && isMaterialPMA) {
				EditorGUILayout.HelpBox(string.Format("'{0} Material' uses PMA but 'Atlas Texture Settings' uses Straight Alpha. " +
					"You might want to assign 'SkeletonStraight{0}' instead.", blendType), MessageType.Warning);
			} else if (isTexturePresetPMA && !isMaterialPMA) {
				EditorGUILayout.HelpBox(string.Format("'{0} Material' uses Straight Alpha but 'Atlas Texture Settings' uses PMA. " +
					"You might want to assign 'SkeletonPMA{0}' instead.", blendType), MessageType.Warning);
			}
		}

		public static void HandlePreferencesGUI (SerializedObject settings) {

			float prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 250;

			using (new EditorGUI.IndentLevelScope()) {
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(settings.FindProperty("showHierarchyIcons"), new GUIContent("Show Hierarchy Icons", "Show relevant icons on GameObjects with Spine Components on them. Disable this if you have large, complex scenes."));
				if (EditorGUI.EndChangeCheck()) {
#if NEWPLAYMODECALLBACKS
					SpineEditorUtilities.HierarchyHandler.IconsOnPlaymodeStateChanged(PlayModeStateChange.EnteredEditMode);
#else
					SpineEditorUtilities.HierarchyHandler.IconsOnPlaymodeStateChanged();
#endif
				}

				EditorGUILayout.PropertyField(settings.FindProperty("autoReloadSceneSkeletons"), new GUIContent("Auto-reload scene components", "Reloads Skeleton components in the scene whenever their SkeletonDataAsset is modified. This makes it so changes in the SkeletonData asset inspector are immediately reflected. This may be slow when your scenes have large numbers of SkeletonRenderers or SkeletonGraphic."));
				EditorGUILayout.PropertyField(settings.FindProperty("reloadAfterPlayMode"), new GUIContent("Reload SkeletonData after Play", "When enabled, the shared SkeletonData of all skeletons in the active scene is reloaded (from the .json or .skel.bytes file) after exiting play-mode. This may add undesired delays, but prevents (accidental) modifications to the shared SkeletonData during play-mode carrying over its effect into subsequent plays."));

				EditorGUILayout.Separator();
				EditorGUILayout.LabelField("Auto-Import Settings", EditorStyles.boldLabel);
				{
					SpineEditorUtilities.FloatPropertyField(settings.FindProperty("defaultMix"), new GUIContent("Default Mix", "The Default Mix Duration for newly imported SkeletonDataAssets."), min: 0f);
					SpineEditorUtilities.FloatPropertyField(settings.FindProperty("defaultScale"), new GUIContent("Default SkeletonData Scale", "The Default skeleton import scale for newly imported SkeletonDataAssets."), min: 0.0000001f);

					SpineEditorUtilities.ShaderPropertyField(settings.FindProperty("defaultShader"), new GUIContent("Default Shader"), SpinePreferences.DEFAULT_DEFAULT_SHADER);

					TextureWorkflowProperties textureProperties = new TextureWorkflowProperties(settings);

					EditorGUILayout.Space();
					using (new GUILayout.HorizontalScope()) {
						EditorGUILayout.PrefixLabel("Switch Texture Workflow");
						if (GUILayout.Button(new GUIContent("Straight Alpha", "Assign straight-alpha atlas texture workflow templates."), GUILayout.Width(96)))
							SwitchToStraightAlphaDefaults(textureProperties);
						bool isLinearColorSpace = QualitySettings.activeColorSpace == ColorSpace.Linear;
						using (new EditorGUI.DisabledScope(isLinearColorSpace)) {
							if (GUILayout.Button(new GUIContent("PMA", isLinearColorSpace ?
								"[Only supported with Gamma color space]" : "Assign PMA atlas texture workflow templates."), GUILayout.Width(64))) {
								SwitchToPMADefaults(textureProperties);
							}
						}
					}
					EditorGUILayout.PropertyField(settings.FindProperty("setTextureImporterSettings"), new GUIContent("Apply Atlas Texture Settings", "Apply reference settings for Texture Importers."));

					var textureSettingsRef = textureProperties.textureSettingsReference;
					SpineEditorUtilities.PresetAssetPropertyField(textureSettingsRef, new GUIContent("Atlas Texture Settings",
						string.Format("Apply the selected texture import settings at newly imported atlas textures.\n\n" +
						"When exporting atlas textures from Spine with \"Premultiply alpha\" enabled (the default), assign \"{0}\". If you have disabled \"Premultiply alpha\", leave it at \"{1}\".\n\n" +
						"You can also create your own TextureImporter Preset asset and assign it here.",
						DEFAULT_TEXTURE_PRESET_PMA, DEFAULT_TEXTURE_PRESET_STRAIGHT)));
					if (string.IsNullOrEmpty(textureSettingsRef.stringValue)) {
						string[] pmaTextureSettingsReferenceGUIDS = AssetDatabase.FindAssets(DEFAULT_TEXTURE_PRESET);
						if (pmaTextureSettingsReferenceGUIDS.Length > 0) {
							string assetPath = AssetDatabase.GUIDToAssetPath(pmaTextureSettingsReferenceGUIDS[0]);
							if (!string.IsNullOrEmpty(assetPath))
								textureSettingsRef.stringValue = assetPath;
						}
					}
					bool isTexturePresetPMA = IsPMAWorkflow(textureSettingsRef.stringValue);
					EditorGUILayout.PropertyField(settings.FindProperty("applyAdditiveMaterial"),
						new GUIContent("Apply Additive Material", "The Default Apply Additive Material setting for newly imported SkeletonDataAssets."));
					ShowBlendModeMaterialProperty(textureProperties.blendModeMaterialAdditive, "Additive", isTexturePresetPMA);
					ShowBlendModeMaterialProperty(textureProperties.blendModeMaterialMultiply, "Multiply", isTexturePresetPMA);
					ShowBlendModeMaterialProperty(textureProperties.blendModeMaterialScreen, "Screen", isTexturePresetPMA);
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Warnings", EditorStyles.boldLabel);
				{
					EditorGUILayout.PropertyField(settings.FindProperty("atlasTxtImportWarning"), new GUIContent("Atlas & Skel Extension Warning", "Log a warning and recommendation whenever a `.atlas` or `.skel` file is found."));
					EditorGUILayout.PropertyField(settings.FindProperty("textureImporterWarning"), new GUIContent("Texture Settings Warning", "Log a warning and recommendation whenever Texture Import Settings are detected that could lead to undesired effects, e.g. white border artifacts."));
					EditorGUILayout.PropertyField(settings.FindProperty("componentMaterialWarning"), new GUIContent("Component & Material Warning", "Log a warning and recommendation whenever Component and Material settings are not compatible."));
					EditorGUILayout.PropertyField(settings.FindProperty("skeletonDataAssetNoFileError"), new GUIContent("SkeletonDataAsset no file Error", "Log an error when querying SkeletonData from SkeletonDataAsset with no json or binary file assigned."));
					EditorGUILayout.PropertyField(settings.FindProperty("skeletonDataAssetMismatchWarning"), new GUIContent("SkeletonDataAsset Mismatch Warning", "Highlight AnimationReferenceAsset dropdown in red when the reference asset's SkeletonDataAsset does not match the one at the skeleton component."));
					EditorGUILayout.PropertyField(settings.FindProperty("workflowMismatchDialog"), new GUIContent("Workflow Mismatch Dialog", "Show warning dialog when PMA atlas is detected but not supported with current project settings."));
					SkeletonDataAsset.errorIfSkeletonFileNullGlobal = settings.FindProperty("skeletonDataAssetNoFileError").boolValue;
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Editor Instantiation", EditorStyles.boldLabel);
				{
					EditorGUILayout.Slider(settings.FindProperty("defaultZSpacing"), -0.1f, 0f, new GUIContent("Default Slot Z-Spacing"));
					EditorGUILayout.PropertyField(settings.FindProperty("defaultInstantiateLoop"), new GUIContent("Default Loop", "Spawn Spine GameObjects with loop enabled."));
					EditorGUILayout.LabelField("Physics Inheritance");
					using (new SpineInspectorUtility.IndentScope()) {
						EditorGUILayout.PropertyField(settings.FindProperty("defaultPhysicsPositionInheritance"), new GUIContent("Default Position", "The Default Physics Inheritance - Position factor."));
						EditorGUILayout.PropertyField(settings.FindProperty("defaultPhysicsRotationInheritance"), new GUIContent("Default Rotation", "The Default Physics Inheritance - Rotation factor."));
					}
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Mecanim Bake Settings", EditorStyles.boldLabel);
				{
					EditorGUILayout.PropertyField(settings.FindProperty("mecanimEventIncludeFolderName"), new GUIContent("Include Folder Name in Event", "When enabled, Mecanim events will call methods named 'FolderNameEventName', when disabled it will call 'EventName'."));
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Handles and Gizmos", EditorStyles.boldLabel);
				{
					EditorGUI.BeginChangeCheck();
					SerializedProperty scaleProperty = settings.FindProperty("handleScale");
					EditorGUILayout.PropertyField(scaleProperty, new GUIContent("Editor Bone Scale"));
					if (EditorGUI.EndChangeCheck()) {
						EditorPrefs.SetFloat(SpinePreferences.SCENE_ICONS_SCALE_KEY, scaleProperty.floatValue);
						SceneView.RepaintAll();
					}
				}

#if HAS_ON_POSTPROCESS_PREFAB
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);
				{
					EditorGUILayout.PropertyField(settings.FindProperty("fixPrefabOverrideViaMeshFilter"), new GUIContent("Fix Prefab Overr. MeshFilter", "Fixes the prefab always being marked as changed (sets the MeshFilter's hide flags to DontSaveInEditor), but at the cost of references to the MeshFilter by other components being lost. This is a global setting that can be overwritten on each SkeletonRenderer"));
					SkeletonRenderer.fixPrefabOverrideViaMeshFilterGlobal = settings.FindProperty("fixPrefabOverrideViaMeshFilter").boolValue;

					EditorGUILayout.PropertyField(settings.FindProperty("removePrefabPreviewMeshes"), new GUIContent("Optimize Preview Meshes", "When enabled, Spine prefab preview meshes will be removed in a pre-build step to reduce build size. This increases build time as all prefabs in the project will be processed."));
				}
#endif

#if HAS_ANY_UNSAFE_OPTIONS
				GUILayout.Space(20);
				EditorGUILayout.LabelField("Unsafe Build Defines", EditorStyles.boldLabel);
				using (new GUILayout.HorizontalScope()) {
					EditorGUILayout.PrefixLabel(new GUIContent("Direct data access", "Allow unsafe direct data access. Currently affects reading .skel.bytes files, reading with fewer allocations."));
					if (GUILayout.Button("Disable", GUILayout.Width(64)))
						SpineBuildEnvUtility.DisableBuildDefine(SpineBuildEnvUtility.SPINE_ALLOW_UNSAFE_CODE);
					if (GUILayout.Button("Enable", GUILayout.Width(64)))
						SpineBuildEnvUtility.EnableBuildDefine(SpineBuildEnvUtility.SPINE_ALLOW_UNSAFE_CODE);
				}
#endif
				GUILayout.Space(20);
				EditorGUILayout.LabelField("Automatic Component Upgrade", EditorStyles.boldLabel);
#if SPINE_AUTO_UPGRADE_COMPONENTS_OFF
				bool upgradeComponentsEnabled = false;
#else
				bool upgradeComponentsEnabled = true;
#endif
				using (new GUILayout.HorizontalScope()) {
					EditorGUILayout.PrefixLabel(new GUIContent("Split Component Upgrade",
						"Allow automatic upgrade of skeleton components to split components new in version 4.3. " +
						"Disable once all scenes and prefabs are migrated to avoid unnecessary editor checks."));
					SpineEditorUtilities.EnableDisableDefineButtons(SpineBuildEnvUtility.SPINE_AUTO_UPGRADE_COMPONENTS_OFF, upgradeComponentsEnabled, invert: true);
				}
				using (new EditorGUI.DisabledScope(!upgradeComponentsEnabled)) {
					using (new GUILayout.HorizontalScope()) {
						EditorGUILayout.PrefixLabel(new GUIContent("Upgrade Scenes & Prefabs", "Upgrades all scenes and " +
						"prefabs in the project to split animation components new in version 4.3."));
						if (GUILayout.Button("Upgrade All", GUILayout.Width(132))) {
							if (EditorUtility.DisplayDialog("Upgrade All",
								"This will open and process all scenes and prefabs in your project to upgrade Spine components to version 4.3.\n\n" +
								"This process may take a while for large projects.\n\n" +
								"Make sure to backup your project before proceeding.\n\n" +
								"Continue?", "Yes, Upgrade", "Cancel")) {
#if AUTO_UPGRADE_TO_43_COMPONENTS
							SpineEditorUtilities.UpgradeAllScenesAndPrefabsTo43();
#endif
							}
						}
					}
				}

				GUILayout.Space(20);
				EditorGUILayout.LabelField("Threading Defaults", EditorStyles.boldLabel);
				{
					bool useThreadedMeshGeneration = RuntimeSettings.UseThreadedMeshGeneration;
					bool useThreadedAnimation = RuntimeSettings.UseThreadedAnimation;
					SpineEditorUtilities.BoolRuntimePropertiesField(
						() => RuntimeSettings.UseThreadedMeshGeneration,
						value => RuntimeSettings.UseThreadedMeshGeneration = value,
						new GUIContent("Threaded MeshGeneration", "Global setting for the equally named SkeletonRenderer and SkeletonGraphic Inspector parameter."));
					SpineEditorUtilities.BoolRuntimePropertiesField(
						() => RuntimeSettings.UseThreadedAnimation, value => RuntimeSettings.UseThreadedAnimation = value,
						new GUIContent("Threaded Animation", "Global setting for the equally named SkeletonAnimation and SkeletonGraphic Inspector parameter."));

#if SPINE_DISABLE_LOAD_BALANCING
					bool loadBalancingEnabled = false;
#else
					bool loadBalancingEnabled = true;
#endif
					using (new GUILayout.HorizontalScope()) {
						EditorGUILayout.PrefixLabel(new GUIContent("Load Balancing",
							"Enable load balancing to better utilize threads." +
							"Only has an effect when using threaded animation or threaded mesh generation."));
						SpineEditorUtilities.EnableDisableDefineButtons(SpineBuildEnvUtility.SPINE_DISABLE_LOAD_BALANCING,
							loadBalancingEnabled, invert: true);
					}

#if ALLOWS_CUSTOM_PROFILING
#if SPINE_ENABLE_THREAD_PROFILING
					bool threadProfilingEnabled = true;
#else
					bool threadProfilingEnabled = false;
#endif
					using (new GUILayout.HorizontalScope()) {
						EditorGUILayout.PrefixLabel(new GUIContent("Thread Profiling",
							"Enable profiling of Spine worker threads in the Unity Profiler. " +
							"Enable only when needed, as it adds some overhead."));
						SpineEditorUtilities.EnableDisableDefineButtons(SpineBuildEnvUtility.SPINE_ENABLE_THREAD_PROFILING, threadProfilingEnabled);
					}
#endif
				}

				GUILayout.Space(20);
				EditorGUILayout.LabelField("Timeline Extension", EditorStyles.boldLabel);
				{
					EditorGUILayout.PropertyField(settings.FindProperty("timelineDefaultMixDuration"), new GUIContent("Default Mix Duration", "When enabled, the clip uses the default mix duration by default, as specified at the SkeletonDataAsset."));
					EditorGUILayout.PropertyField(settings.FindProperty("timelineUseBlendDuration"), new GUIContent("Use Blend Duration", "When enabled, MixDuration will be synced with timeline clip transition duration 'Ease In Duration'."));
				}
			}
			EditorGUIUtility.labelWidth = prevLabelWidth;
		}

		static void AssignAssetString (SerializedProperty stringProperty, string assetName) {
			string[] guids = AssetDatabase.FindAssets(assetName);
			if (guids.Length > 0) {
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
				if (!string.IsNullOrEmpty(assetPath))
					stringProperty.stringValue = assetPath;
			}
		}

		static void AssignAssetReference (SerializedProperty stringProperty, string assetName) {
			string[] guids = AssetDatabase.FindAssets(assetName);
			if (guids.Length > 0) {
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
				if (!string.IsNullOrEmpty(assetPath))
					stringProperty.objectReferenceValue = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
			}
		}

		public void SwitchToStraightAlphaDefaults () {
			SerializedObject serializedSettings = new SerializedObject(this);
			TextureWorkflowProperties properties = new TextureWorkflowProperties(serializedSettings);
			SwitchToStraightAlphaDefaults(properties);
			serializedSettings.ApplyModifiedProperties();
		}

		public void SwitchToPMADefaults () {
			SerializedObject serializedSettings = new SerializedObject(this);
			TextureWorkflowProperties properties = new TextureWorkflowProperties(serializedSettings);
			SwitchToPMADefaults(properties);
			serializedSettings.ApplyModifiedProperties();
		}

		static void SwitchToStraightAlphaDefaults (TextureWorkflowProperties properties) {
			AssignAssetString(properties.textureSettingsReference, DEFAULT_TEXTURE_PRESET_STRAIGHT);
			AssignAssetReference(properties.blendModeMaterialAdditive, DEFAULT_BLEND_MODE_ADDITIVE_MATERIAL_STRAIGHT);
			AssignAssetReference(properties.blendModeMaterialMultiply, DEFAULT_BLEND_MODE_MULTIPLY_MATERIAL_STRAIGHT);
			AssignAssetReference(properties.blendModeMaterialScreen, DEFAULT_BLEND_MODE_SCREEN_MATERIAL_STRAIGHT);
		}

		static void SwitchToPMADefaults (TextureWorkflowProperties properties) {
			AssignAssetString(properties.textureSettingsReference, DEFAULT_TEXTURE_PRESET_PMA);
			AssignAssetReference(properties.blendModeMaterialAdditive, DEFAULT_BLEND_MODE_ADDITIVE_MATERIAL_PMA);
			AssignAssetReference(properties.blendModeMaterialMultiply, DEFAULT_BLEND_MODE_MULTIPLY_MATERIAL_PMA);
			AssignAssetReference(properties.blendModeMaterialScreen, DEFAULT_BLEND_MODE_SCREEN_MATERIAL_PMA);
		}
#endif // NEW_PREFERENCES_SETTINGS_PROVIDER
	}
}
