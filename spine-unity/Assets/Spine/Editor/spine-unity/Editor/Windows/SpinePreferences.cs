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

#if UNITY_2017_2_OR_NEWER
#define NEWPLAYMODECALLBACKS
#endif

#if UNITY_2018_3_OR_NEWER
#define NEW_PREFERENCES_SETTINGS_PROVIDER
#endif

#if UNITY_2020_2_OR_NEWER
#define HAS_ON_POSTPROCESS_PREFAB
#endif

using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	public class SpinePreferences : ScriptableObject {

#if NEW_PREFERENCES_SETTINGS_PROVIDER
		static int wasPreferencesDirCreated = 0;
		static int wasPreferencesAssetCreated = 0;
#endif

		public const string SPINE_SETTINGS_ASSET_PATH = "Assets/Editor/SpineSettings.asset";

#if SPINE_TK2D
		internal const float DEFAULT_DEFAULT_SCALE = 1f;
#else
		internal const float DEFAULT_DEFAULT_SCALE = 0.01f;
#endif
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

		internal const bool DEFAULT_SHOW_HIERARCHY_ICONS = true;
		public bool showHierarchyIcons = DEFAULT_SHOW_HIERARCHY_ICONS;

		internal const bool DEFAULT_SET_TEXTUREIMPORTER_SETTINGS = true;
		public bool setTextureImporterSettings = DEFAULT_SET_TEXTUREIMPORTER_SETTINGS;

		internal const string DEFAULT_TEXTURE_SETTINGS_REFERENCE = "";
		public string textureSettingsReference = DEFAULT_TEXTURE_SETTINGS_REFERENCE;

#if HAS_ON_POSTPROCESS_PREFAB
		internal const bool DEFAULT_FIX_PREFAB_OVERRIDE_VIA_MESH_FILTER = false;
		public bool fixPrefabOverrideViaMeshFilter = DEFAULT_FIX_PREFAB_OVERRIDE_VIA_MESH_FILTER;
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

		public const string DEFAULT_BLEND_MODE_MULTIPLY_MATERIAL = "SkeletonPMAMultiply";
		public const string DEFAULT_BLEND_MODE_SCREEN_MATERIAL = "SkeletonPMAScreen";
		public const string DEFAULT_BLEND_MODE_ADDITIVE_MATERIAL = "SkeletonPMAAdditive";

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
			return settings;
		}

		static SpinePreferences FindSpinePreferences () {
			string typeSearchString = " t:SpinePreferences";
			string[] guids = AssetDatabase.FindAssets(typeSearchString);
			foreach (string guid in guids) {
				string path = AssetDatabase.GUIDToAssetPath(guid);
				var preferences = AssetDatabase.LoadAssetAtPath<SpinePreferences>(path);
				if (preferences != null)
					return preferences;
			}
			return null;
		}

		private static void ShowBlendModeMaterialProperty (SerializedProperty blendModeMaterialProperty,
			string blendType, bool isTexturePresetPMA) {

			EditorGUILayout.PropertyField(blendModeMaterialProperty, new GUIContent(blendType + " Material", blendType + " blend mode Material template."));
			var material = blendModeMaterialProperty.objectReferenceValue as Material;
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

				EditorGUILayout.Separator();
				EditorGUILayout.LabelField("Auto-Import Settings", EditorStyles.boldLabel);
				{
					SpineEditorUtilities.FloatPropertyField(settings.FindProperty("defaultMix"), new GUIContent("Default Mix", "The Default Mix Duration for newly imported SkeletonDataAssets."), min: 0f);
					SpineEditorUtilities.FloatPropertyField(settings.FindProperty("defaultScale"), new GUIContent("Default SkeletonData Scale", "The Default skeleton import scale for newly imported SkeletonDataAssets."), min: 0.0000001f);

					SpineEditorUtilities.ShaderPropertyField(settings.FindProperty("defaultShader"), new GUIContent("Default Shader"), SpinePreferences.DEFAULT_DEFAULT_SHADER);

					EditorGUILayout.PropertyField(settings.FindProperty("setTextureImporterSettings"), new GUIContent("Apply Atlas Texture Settings", "Apply reference settings for Texture Importers."));
					var textureSettingsRef = settings.FindProperty("textureSettingsReference");
					SpineEditorUtilities.PresetAssetPropertyField(textureSettingsRef, new GUIContent("Atlas Texture Settings", "Apply the selected texture import settings at newly imported atlas textures. When exporting atlas textures from Spine with \"Premultiply alpha\" enabled (the default), you can leave it at \"PMATexturePreset\". If you have disabled \"Premultiply alpha\", set it to \"StraightAlphaTexturePreset\". You can also create your own TextureImporter Preset asset and assign it here."));
					if (string.IsNullOrEmpty(textureSettingsRef.stringValue)) {
						var pmaTextureSettingsReferenceGUIDS = AssetDatabase.FindAssets("PMATexturePreset");
						if (pmaTextureSettingsReferenceGUIDS.Length > 0) {
							var assetPath = AssetDatabase.GUIDToAssetPath(pmaTextureSettingsReferenceGUIDS[0]);
							if (!string.IsNullOrEmpty(assetPath))
								textureSettingsRef.stringValue = assetPath;
						}
					}

					SerializedProperty blendModeMaterialAdditive = settings.FindProperty("blendModeMaterialAdditive");
					SerializedProperty blendModeMaterialMultiply = settings.FindProperty("blendModeMaterialMultiply");
					SerializedProperty blendModeMaterialScreen = settings.FindProperty("blendModeMaterialScreen");
					bool isTexturePresetPMA = IsPMAWorkflow(textureSettingsRef.stringValue);
					ShowBlendModeMaterialProperty(blendModeMaterialAdditive, "Additive", isTexturePresetPMA);
					ShowBlendModeMaterialProperty(blendModeMaterialMultiply, "Multiply", isTexturePresetPMA);
					ShowBlendModeMaterialProperty(blendModeMaterialScreen, "Screen", isTexturePresetPMA);
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Warnings", EditorStyles.boldLabel);
				{
					EditorGUILayout.PropertyField(settings.FindProperty("atlasTxtImportWarning"), new GUIContent("Atlas Extension Warning", "Log a warning and recommendation whenever a `.atlas` file is found."));
					EditorGUILayout.PropertyField(settings.FindProperty("textureImporterWarning"), new GUIContent("Texture Settings Warning", "Log a warning and recommendation whenever Texture Import Settings are detected that could lead to undesired effects, e.g. white border artifacts."));
					EditorGUILayout.PropertyField(settings.FindProperty("componentMaterialWarning"), new GUIContent("Component & Material Warning", "Log a warning and recommendation whenever Component and Material settings are not compatible."));
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Editor Instantiation", EditorStyles.boldLabel);
				{
					EditorGUILayout.Slider(settings.FindProperty("defaultZSpacing"), -0.1f, 0f, new GUIContent("Default Slot Z-Spacing"));
					EditorGUILayout.PropertyField(settings.FindProperty("defaultInstantiateLoop"), new GUIContent("Default Loop", "Spawn Spine GameObjects with loop enabled."));
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
					var scaleProperty = settings.FindProperty("handleScale");
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
				}
#endif

#if SPINE_TK2D_DEFINE
				bool isTK2DDefineSet = true;
#else
				bool isTK2DDefineSet = false;
#endif
				bool isTK2DAllowed = SpineEditorUtilities.SpineTK2DEditorUtility.IsTK2DAllowed;
				if (SpineEditorUtilities.SpineTK2DEditorUtility.IsTK2DInstalled() || isTK2DDefineSet) {
					GUILayout.Space(20);
					EditorGUILayout.LabelField("3rd Party Settings", EditorStyles.boldLabel);
					using (new GUILayout.HorizontalScope()) {
						EditorGUILayout.PrefixLabel("Define TK2D");
						if (isTK2DAllowed && GUILayout.Button("Enable", GUILayout.Width(64)))
							SpineEditorUtilities.SpineTK2DEditorUtility.EnableTK2D();
						if (GUILayout.Button("Disable", GUILayout.Width(64)))
							SpineEditorUtilities.SpineTK2DEditorUtility.DisableTK2D();
					}
#if !SPINE_TK2D_DEFINE
					if (!isTK2DAllowed) {
						EditorGUILayout.LabelField("To allow TK2D support, please modify line 67 in", EditorStyles.boldLabel);
						EditorGUILayout.LabelField("Spine/Editor/spine-unity/Editor/Util./BuildSettings.cs", EditorStyles.boldLabel);
					}
#endif
				}

				GUILayout.Space(20);
				EditorGUILayout.LabelField("Timeline Extension", EditorStyles.boldLabel);
				{
					EditorGUILayout.PropertyField(settings.FindProperty("timelineUseBlendDuration"), new GUIContent("Use Blend Duration", "When enabled, MixDuration will be synced with timeline clip transition duration 'Ease In Duration'."));
				}
			}
			EditorGUIUtility.labelWidth = prevLabelWidth;
		}
#endif // NEW_PREFERENCES_SETTINGS_PROVIDER
	}
}
