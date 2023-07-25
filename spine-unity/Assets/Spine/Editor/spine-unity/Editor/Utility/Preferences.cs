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

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

#if UNITY_2018_3_OR_NEWER
#define NEW_PREFERENCES_SETTINGS_PROVIDER
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

#if NEW_PREFERENCES_SETTINGS_PROVIDER
		static class SpineSettingsProviderRegistration {
			[SettingsProvider]
			public static SettingsProvider CreateSpineSettingsProvider () {
				SettingsProvider provider = new SettingsProvider("Spine", SettingsScope.User) {
					label = "Spine",
					guiHandler = (searchContext) => {
						SpinePreferences settings = SpinePreferences.GetOrCreateSettings();
						SerializedObject serializedSettings = new SerializedObject(settings);
						SpinePreferences.HandlePreferencesGUI(serializedSettings);
						if (serializedSettings.ApplyModifiedProperties())
							OldPreferences.SaveToEditorPrefs(settings);
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

#if NEW_PREFERENCES_SETTINGS_PROVIDER
		public static SpinePreferences Preferences {
			get {
				return SpinePreferences.GetOrCreateSettings();
			}
		}
#endif

#if NEW_PREFERENCES_SETTINGS_PROVIDER
		public static class OldPreferences {
#else
		public static class Preferences {
#endif
			const string DEFAULT_SCALE_KEY = "SPINE_DEFAULT_SCALE";
			public static float defaultScale = SpinePreferences.DEFAULT_DEFAULT_SCALE;

			const string DEFAULT_MIX_KEY = "SPINE_DEFAULT_MIX";
			public static float defaultMix = SpinePreferences.DEFAULT_DEFAULT_MIX;

			const string DEFAULT_SHADER_KEY = "SPINE_DEFAULT_SHADER";
			public static string defaultShader = SpinePreferences.DEFAULT_DEFAULT_SHADER;
			public static string DefaultShader {
				get { return !string.IsNullOrEmpty(defaultShader) ? defaultShader : SpinePreferences.DEFAULT_DEFAULT_SHADER; }
				set { defaultShader = value; }
			}

			const string DEFAULT_ZSPACING_KEY = "SPINE_DEFAULT_ZSPACING";
			public static float defaultZSpacing = SpinePreferences.DEFAULT_DEFAULT_ZSPACING;

			const string DEFAULT_INSTANTIATE_LOOP_KEY = "SPINE_DEFAULT_INSTANTIATE_LOOP";
			public static bool defaultInstantiateLoop = SpinePreferences.DEFAULT_DEFAULT_INSTANTIATE_LOOP;

			const string SHOW_HIERARCHY_ICONS_KEY = "SPINE_SHOW_HIERARCHY_ICONS";
			public static bool showHierarchyIcons = SpinePreferences.DEFAULT_SHOW_HIERARCHY_ICONS;

			const string RELOAD_AFTER_PLAYMODE_KEY = "SPINE_RELOAD_AFTER_PLAYMODE";
			public static bool reloadAfterPlayMode = SpinePreferences.DEFAULT_RELOAD_AFTER_PLAYMODE;

			const string SET_TEXTUREIMPORTER_SETTINGS_KEY = "SPINE_SET_TEXTUREIMPORTER_SETTINGS";
			public static bool setTextureImporterSettings = SpinePreferences.DEFAULT_SET_TEXTUREIMPORTER_SETTINGS;

			const string TEXTURE_SETTINGS_REFERENCE_KEY = "SPINE_TEXTURE_SETTINGS_REFERENCE";
			public static string textureSettingsReference = SpinePreferences.DEFAULT_TEXTURE_SETTINGS_REFERENCE;

			public static bool UsesPMAWorkflow {
				get {
					return SpinePreferences.IsPMAWorkflow(textureSettingsReference);
				}
			}

			const string BLEND_MODE_MATERIAL_MULTIPLY_KEY = "SPINE_BLENDMODE_MATERIAL_MULTIPLY";
			const string BLEND_MODE_MATERIAL_SCREEN_KEY = "SPINE_BLENDMODE_MATERIAL_SCREEN";
			const string BLEND_MODE_MATERIAL_ADDITIVE_KEY = "SPINE_BLENDMODE_MATERIAL_ADDITIVE";
			public static string blendModeMaterialMultiply = "";
			public static string blendModeMaterialScreen = "";
			public static string blendModeMaterialAdditive = "";
			public const string DEFAULT_BLEND_MODE_MULTIPLY_MATERIAL = SpinePreferences.DEFAULT_BLEND_MODE_MULTIPLY_MATERIAL;
			public const string DEFAULT_BLEND_MODE_SCREEN_MATERIAL = SpinePreferences.DEFAULT_BLEND_MODE_SCREEN_MATERIAL;
			public const string DEFAULT_BLEND_MODE_ADDITIVE_MATERIAL = SpinePreferences.DEFAULT_BLEND_MODE_ADDITIVE_MATERIAL;

			public static Material BlendModeMaterialMultiply {
				get { return AssetDatabase.LoadAssetAtPath<Material>(blendModeMaterialMultiply); }
			}
			public static Material BlendModeMaterialScreen {
				get { return AssetDatabase.LoadAssetAtPath<Material>(blendModeMaterialScreen); }
			}
			public static Material BlendModeMaterialAdditive {
				get { return AssetDatabase.LoadAssetAtPath<Material>(blendModeMaterialAdditive); }
			}

			const string ATLASTXT_WARNING_KEY = "SPINE_ATLASTXT_WARNING";
			public static bool atlasTxtImportWarning = SpinePreferences.DEFAULT_ATLASTXT_WARNING;

			const string TEXTUREIMPORTER_WARNING_KEY = "SPINE_TEXTUREIMPORTER_WARNING";
			public static bool textureImporterWarning = SpinePreferences.DEFAULT_TEXTUREIMPORTER_WARNING;

			const string COMPONENTMATERIAL_WARNING_KEY = "SPINE_COMPONENTMATERIAL_WARNING";
			public static bool componentMaterialWarning = SpinePreferences.DEFAULT_COMPONENTMATERIAL_WARNING;

			const string SKELETONDATA_ASSET_NO_FILE_ERROR_KEY = "SPINE_SKELETONDATA_ASSET_NO_FILE_ERROR";
			public static bool skeletonDataAssetNoFileError = SpinePreferences.DEFAULT_SKELETONDATA_ASSET_NO_FILE_ERROR;

			public const float DEFAULT_MIPMAPBIAS = SpinePreferences.DEFAULT_MIPMAPBIAS;

			public const string SCENE_ICONS_SCALE_KEY = "SPINE_SCENE_ICONS_SCALE";
			public static float handleScale = SpinePreferences.DEFAULT_SCENE_ICONS_SCALE;

			const string AUTO_RELOAD_SCENESKELETONS_KEY = "SPINE_AUTO_RELOAD_SCENESKELETONS";
			public static bool autoReloadSceneSkeletons = SpinePreferences.DEFAULT_AUTO_RELOAD_SCENESKELETONS;

			const string MECANIM_EVENT_INCLUDE_FOLDERNAME_KEY = "SPINE_MECANIM_EVENT_INCLUDE_FOLDERNAME";
			public static bool mecanimEventIncludeFolderName = SpinePreferences.DEFAULT_MECANIM_EVENT_INCLUDE_FOLDERNAME;

			const string TIMELINE_USE_BLEND_DURATION_KEY = "SPINE_TIMELINE_USE_BLEND_DURATION_KEY";
			public static bool timelineUseBlendDuration = SpinePreferences.DEFAULT_TIMELINE_USE_BLEND_DURATION;

			const string TIMELINE_DEFAULT_MIX_DURATION_KEY = "SPINE_TIMELINE_DEFAULT_MIX_DURATION_KEY";
			public static bool timelineDefaultMixDuration = SpinePreferences.DEFAULT_TIMELINE_DEFAULT_MIX_DURATION;

			static bool preferencesLoaded = false;

			public static void Load () {
				if (preferencesLoaded)
					return;

				defaultMix = EditorPrefs.GetFloat(DEFAULT_MIX_KEY, SpinePreferences.DEFAULT_DEFAULT_MIX);
				defaultScale = EditorPrefs.GetFloat(DEFAULT_SCALE_KEY, SpinePreferences.DEFAULT_DEFAULT_SCALE);
				defaultZSpacing = EditorPrefs.GetFloat(DEFAULT_ZSPACING_KEY, SpinePreferences.DEFAULT_DEFAULT_ZSPACING);
				defaultShader = EditorPrefs.GetString(DEFAULT_SHADER_KEY, SpinePreferences.DEFAULT_DEFAULT_SHADER);
				showHierarchyIcons = EditorPrefs.GetBool(SHOW_HIERARCHY_ICONS_KEY, SpinePreferences.DEFAULT_SHOW_HIERARCHY_ICONS);
				reloadAfterPlayMode = EditorPrefs.GetBool(RELOAD_AFTER_PLAYMODE_KEY, SpinePreferences.DEFAULT_RELOAD_AFTER_PLAYMODE);
				setTextureImporterSettings = EditorPrefs.GetBool(SET_TEXTUREIMPORTER_SETTINGS_KEY, SpinePreferences.DEFAULT_SET_TEXTUREIMPORTER_SETTINGS);
				textureSettingsReference = EditorPrefs.GetString(TEXTURE_SETTINGS_REFERENCE_KEY, SpinePreferences.DEFAULT_TEXTURE_SETTINGS_REFERENCE);
				blendModeMaterialMultiply = EditorPrefs.GetString(BLEND_MODE_MATERIAL_MULTIPLY_KEY, "");
				blendModeMaterialScreen = EditorPrefs.GetString(BLEND_MODE_MATERIAL_SCREEN_KEY, "");
				blendModeMaterialAdditive = EditorPrefs.GetString(BLEND_MODE_MATERIAL_ADDITIVE_KEY, "");
				autoReloadSceneSkeletons = EditorPrefs.GetBool(AUTO_RELOAD_SCENESKELETONS_KEY, SpinePreferences.DEFAULT_AUTO_RELOAD_SCENESKELETONS);
				mecanimEventIncludeFolderName = EditorPrefs.GetBool(MECANIM_EVENT_INCLUDE_FOLDERNAME_KEY, SpinePreferences.DEFAULT_MECANIM_EVENT_INCLUDE_FOLDERNAME);
				atlasTxtImportWarning = EditorPrefs.GetBool(ATLASTXT_WARNING_KEY, SpinePreferences.DEFAULT_ATLASTXT_WARNING);
				textureImporterWarning = EditorPrefs.GetBool(TEXTUREIMPORTER_WARNING_KEY, SpinePreferences.DEFAULT_TEXTUREIMPORTER_WARNING);
				componentMaterialWarning = EditorPrefs.GetBool(COMPONENTMATERIAL_WARNING_KEY, SpinePreferences.DEFAULT_COMPONENTMATERIAL_WARNING);
				skeletonDataAssetNoFileError = EditorPrefs.GetBool(SKELETONDATA_ASSET_NO_FILE_ERROR_KEY, SpinePreferences.DEFAULT_SKELETONDATA_ASSET_NO_FILE_ERROR);
				timelineDefaultMixDuration = EditorPrefs.GetBool(TIMELINE_DEFAULT_MIX_DURATION_KEY, SpinePreferences.DEFAULT_TIMELINE_DEFAULT_MIX_DURATION);
				timelineUseBlendDuration = EditorPrefs.GetBool(TIMELINE_USE_BLEND_DURATION_KEY, SpinePreferences.DEFAULT_TIMELINE_USE_BLEND_DURATION);
				handleScale = EditorPrefs.GetFloat(SCENE_ICONS_SCALE_KEY, SpinePreferences.DEFAULT_SCENE_ICONS_SCALE);
				preferencesLoaded = true;
			}

#if NEW_PREFERENCES_SETTINGS_PROVIDER
			public static void CopyOldToNewPreferences (ref SpinePreferences newPreferences) {
				newPreferences.defaultMix = EditorPrefs.GetFloat(DEFAULT_MIX_KEY, SpinePreferences.DEFAULT_DEFAULT_MIX);
				newPreferences.defaultScale = EditorPrefs.GetFloat(DEFAULT_SCALE_KEY, SpinePreferences.DEFAULT_DEFAULT_SCALE);
				newPreferences.defaultZSpacing = EditorPrefs.GetFloat(DEFAULT_ZSPACING_KEY, SpinePreferences.DEFAULT_DEFAULT_ZSPACING);
				newPreferences.defaultShader = EditorPrefs.GetString(DEFAULT_SHADER_KEY, SpinePreferences.DEFAULT_DEFAULT_SHADER);
				newPreferences.showHierarchyIcons = EditorPrefs.GetBool(SHOW_HIERARCHY_ICONS_KEY, SpinePreferences.DEFAULT_SHOW_HIERARCHY_ICONS);
				newPreferences.reloadAfterPlayMode = EditorPrefs.GetBool(RELOAD_AFTER_PLAYMODE_KEY, SpinePreferences.DEFAULT_RELOAD_AFTER_PLAYMODE);
				newPreferences.setTextureImporterSettings = EditorPrefs.GetBool(SET_TEXTUREIMPORTER_SETTINGS_KEY, SpinePreferences.DEFAULT_SET_TEXTUREIMPORTER_SETTINGS);
				newPreferences.textureSettingsReference = EditorPrefs.GetString(TEXTURE_SETTINGS_REFERENCE_KEY, SpinePreferences.DEFAULT_TEXTURE_SETTINGS_REFERENCE);
				newPreferences.autoReloadSceneSkeletons = EditorPrefs.GetBool(AUTO_RELOAD_SCENESKELETONS_KEY, SpinePreferences.DEFAULT_AUTO_RELOAD_SCENESKELETONS);
				newPreferences.mecanimEventIncludeFolderName = EditorPrefs.GetBool(MECANIM_EVENT_INCLUDE_FOLDERNAME_KEY, SpinePreferences.DEFAULT_MECANIM_EVENT_INCLUDE_FOLDERNAME);
				newPreferences.atlasTxtImportWarning = EditorPrefs.GetBool(ATLASTXT_WARNING_KEY, SpinePreferences.DEFAULT_ATLASTXT_WARNING);
				newPreferences.textureImporterWarning = EditorPrefs.GetBool(TEXTUREIMPORTER_WARNING_KEY, SpinePreferences.DEFAULT_TEXTUREIMPORTER_WARNING);
				newPreferences.componentMaterialWarning = EditorPrefs.GetBool(COMPONENTMATERIAL_WARNING_KEY, SpinePreferences.DEFAULT_COMPONENTMATERIAL_WARNING);
				newPreferences.skeletonDataAssetNoFileError = EditorPrefs.GetBool(SKELETONDATA_ASSET_NO_FILE_ERROR_KEY, SpinePreferences.DEFAULT_SKELETONDATA_ASSET_NO_FILE_ERROR);
				newPreferences.timelineDefaultMixDuration = EditorPrefs.GetBool(TIMELINE_DEFAULT_MIX_DURATION_KEY, SpinePreferences.DEFAULT_TIMELINE_DEFAULT_MIX_DURATION);
				newPreferences.timelineUseBlendDuration = EditorPrefs.GetBool(TIMELINE_USE_BLEND_DURATION_KEY, SpinePreferences.DEFAULT_TIMELINE_USE_BLEND_DURATION);
				newPreferences.handleScale = EditorPrefs.GetFloat(SCENE_ICONS_SCALE_KEY, SpinePreferences.DEFAULT_SCENE_ICONS_SCALE);
			}

			public static void SaveToEditorPrefs (SpinePreferences preferences) {
				EditorPrefs.SetFloat(DEFAULT_MIX_KEY, preferences.defaultMix);
				EditorPrefs.SetFloat(DEFAULT_SCALE_KEY, preferences.defaultScale);
				EditorPrefs.SetFloat(DEFAULT_ZSPACING_KEY, preferences.defaultZSpacing);
				EditorPrefs.SetString(DEFAULT_SHADER_KEY, preferences.defaultShader);
				EditorPrefs.SetBool(SHOW_HIERARCHY_ICONS_KEY, preferences.showHierarchyIcons);
				EditorPrefs.SetBool(RELOAD_AFTER_PLAYMODE_KEY, preferences.reloadAfterPlayMode);
				EditorPrefs.SetBool(SET_TEXTUREIMPORTER_SETTINGS_KEY, preferences.setTextureImporterSettings);
				EditorPrefs.SetString(TEXTURE_SETTINGS_REFERENCE_KEY, preferences.textureSettingsReference);
				EditorPrefs.SetBool(AUTO_RELOAD_SCENESKELETONS_KEY, preferences.autoReloadSceneSkeletons);
				EditorPrefs.SetBool(MECANIM_EVENT_INCLUDE_FOLDERNAME_KEY, preferences.mecanimEventIncludeFolderName);
				EditorPrefs.SetBool(ATLASTXT_WARNING_KEY, preferences.atlasTxtImportWarning);
				EditorPrefs.SetBool(TEXTUREIMPORTER_WARNING_KEY, preferences.textureImporterWarning);
				EditorPrefs.SetBool(COMPONENTMATERIAL_WARNING_KEY, preferences.componentMaterialWarning);
				EditorPrefs.SetBool(SKELETONDATA_ASSET_NO_FILE_ERROR_KEY, preferences.skeletonDataAssetNoFileError);
				EditorPrefs.SetBool(TIMELINE_DEFAULT_MIX_DURATION_KEY, preferences.timelineDefaultMixDuration);
				EditorPrefs.SetBool(TIMELINE_USE_BLEND_DURATION_KEY, preferences.timelineUseBlendDuration);
				EditorPrefs.SetFloat(SCENE_ICONS_SCALE_KEY, preferences.handleScale);
			}
#endif

#if !NEW_PREFERENCES_SETTINGS_PROVIDER
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
				BoolPrefsField(ref reloadAfterPlayMode, RELOAD_AFTER_PLAYMODE_KEY, new GUIContent("Reload SkeletonData after Play", "When enabled, the shared SkeletonData of all skeletons in the active scene is reloaded (from the .json or .skel.bytes file) after exiting play-mode. This may add undesired delays, but prevents (accidental) modifications to the shared SkeletonData during play-mode carrying over its effect into subsequent plays."));

				EditorGUILayout.Separator();
				EditorGUILayout.LabelField("Auto-Import Settings", EditorStyles.boldLabel);
				{
					SpineEditorUtilities.FloatPrefsField(ref defaultMix, DEFAULT_MIX_KEY, new GUIContent("Default Mix", "The Default Mix Duration for newly imported SkeletonDataAssets."), min: 0);
					SpineEditorUtilities.FloatPrefsField(ref defaultScale, DEFAULT_SCALE_KEY, new GUIContent("Default SkeletonData Scale", "The Default skeleton import scale for newly imported SkeletonDataAssets."), min: 0.0000001f);

					EditorGUI.BeginChangeCheck();
					Shader shader = (EditorGUILayout.ObjectField("Default Shader", Shader.Find(defaultShader), typeof(Shader), false) as Shader);
					defaultShader = shader != null ? shader.name : SpinePreferences.DEFAULT_DEFAULT_SHADER;
					if (EditorGUI.EndChangeCheck())
						EditorPrefs.SetString(DEFAULT_SHADER_KEY, defaultShader);

					SpineEditorUtilities.BoolPrefsField(ref setTextureImporterSettings, SET_TEXTUREIMPORTER_SETTINGS_KEY, new GUIContent("Apply Atlas Texture Settings", "Apply the recommended settings for Texture Importers."));
					SpineEditorUtilities.Texture2DPrefsField(ref textureSettingsReference, TEXTURE_SETTINGS_REFERENCE_KEY, new GUIContent("Atlas Texture Reference Settings", "Apply the selected reference texture import settings at newly imported atlas textures. When exporting atlas textures from Spine with \"Premultiply alpha\" enabled (the default), you can leave it at \"PMAPresetTemplate\". If you have disabled \"Premultiply alpha\", set it to \"StraightAlphaPresetTemplate\". You can also create your own reference texture asset and assign it here."));
					if (string.IsNullOrEmpty(textureSettingsReference)) {
						string[] pmaTextureSettingsReferenceGUIDS = AssetDatabase.FindAssets("PMAPresetTemplate");
						if (pmaTextureSettingsReferenceGUIDS.Length > 0) {
							textureSettingsReference = AssetDatabase.GUIDToAssetPath(pmaTextureSettingsReferenceGUIDS[0]);
							EditorPrefs.SetString(TEXTURE_SETTINGS_REFERENCE_KEY, textureSettingsReference);
						}
					}

					SpineEditorUtilities.MaterialPrefsField(ref blendModeMaterialAdditive, BLEND_MODE_MATERIAL_ADDITIVE_KEY, new GUIContent("Additive Material", "Additive blend mode Material template."));
					if (string.IsNullOrEmpty(blendModeMaterialAdditive)) {
						string[] blendModeMaterialAdditiveGUIDS = AssetDatabase.FindAssets(DEFAULT_BLEND_MODE_ADDITIVE_MATERIAL);
						if (blendModeMaterialAdditiveGUIDS.Length > 0) {
							blendModeMaterialAdditive = AssetDatabase.GUIDToAssetPath(blendModeMaterialAdditiveGUIDS[0]);
						}
					}
					SpineEditorUtilities.MaterialPrefsField(ref blendModeMaterialMultiply, BLEND_MODE_MATERIAL_MULTIPLY_KEY, new GUIContent("Multiply Material", "Multiply blend mode Material template."));
					if (string.IsNullOrEmpty(blendModeMaterialMultiply)) {
						string[] blendModeMaterialMultiplyGUIDS = AssetDatabase.FindAssets(DEFAULT_BLEND_MODE_MULTIPLY_MATERIAL);
						if (blendModeMaterialMultiplyGUIDS.Length > 0) {
							blendModeMaterialMultiply = AssetDatabase.GUIDToAssetPath(blendModeMaterialMultiplyGUIDS[0]);
						}
					}
					SpineEditorUtilities.MaterialPrefsField(ref blendModeMaterialScreen, BLEND_MODE_MATERIAL_SCREEN_KEY, new GUIContent("Screen Material", "Screen blend mode Material template."));
					if (string.IsNullOrEmpty(blendModeMaterialScreen)) {
						string[] blendModeMaterialScreenGUIDS = AssetDatabase.FindAssets(DEFAULT_BLEND_MODE_SCREEN_MATERIAL);
						if (blendModeMaterialScreenGUIDS.Length > 0) {
							blendModeMaterialScreen = AssetDatabase.GUIDToAssetPath(blendModeMaterialScreenGUIDS[0]);
						}
					}
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Warnings", EditorStyles.boldLabel);
				{
					SpineEditorUtilities.BoolPrefsField(ref atlasTxtImportWarning, ATLASTXT_WARNING_KEY, new GUIContent("Atlas Extension Warning", "Log a warning and recommendation whenever a `.atlas` file is found."));
					SpineEditorUtilities.BoolPrefsField(ref textureImporterWarning, TEXTUREIMPORTER_WARNING_KEY, new GUIContent("Texture Settings Warning", "Log a warning and recommendation whenever Texture Import Settings are detected that could lead to undesired effects, e.g. white border artifacts."));
					SpineEditorUtilities.BoolPrefsField(ref componentMaterialWarning, COMPONENTMATERIAL_WARNING_KEY, new GUIContent("Component & Material Warning", "Log a warning and recommendation whenever Component and Material settings are not compatible."));
					SpineEditorUtilities.BoolPrefsField(ref skeletonDataAssetNoFileError, SKELETONDATA_ASSET_NO_FILE_ERROR_KEY, new GUIContent("SkeletonDataAsset no file Error", "Log an error when querying SkeletonData from SkeletonDataAsset with no json or binary file assigned."));
					SkeletonDataAsset.errorIfSkeletonFileNullGlobal = skeletonDataAssetNoFileError;
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
				EditorGUILayout.LabelField("Mecanim Bake Settings", EditorStyles.boldLabel);
				{
					SpineEditorUtilities.BoolPrefsField(ref mecanimEventIncludeFolderName, MECANIM_EVENT_INCLUDE_FOLDERNAME_KEY, new GUIContent("Include Folder Name in Event", "When enabled, Mecanim events will call methods named 'FolderNameEventName', when disabled it will call 'EventName'."));
				}

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Handles and Gizmos", EditorStyles.boldLabel);
				{
					EditorGUI.BeginChangeCheck();
					handleScale = EditorGUILayout.Slider("Editor Bone Scale", handleScale, 0.01f, 2f);
					handleScale = Mathf.Max(0.01f, handleScale);
					if (EditorGUI.EndChangeCheck()) {
						EditorPrefs.SetFloat(SCENE_ICONS_SCALE_KEY, handleScale);
						SceneView.RepaintAll();
					}
				}

#if SPINE_TK2D_DEFINE
				bool isTK2DDefineSet = true;
#else
				bool isTK2DDefineSet = false;
#endif
				bool isTK2DAllowed = SpineTK2DEditorUtility.IsTK2DAllowed;
				if (SpineTK2DEditorUtility.IsTK2DInstalled() || isTK2DDefineSet) {
					GUILayout.Space(20);
					EditorGUILayout.LabelField("3rd Party Settings", EditorStyles.boldLabel);
					using (new GUILayout.HorizontalScope()) {
						EditorGUILayout.PrefixLabel("Define TK2D");
						if (isTK2DAllowed && GUILayout.Button("Enable", GUILayout.Width(64)))
							SpineTK2DEditorUtility.EnableTK2D();
						if (GUILayout.Button("Disable", GUILayout.Width(64)))
							SpineTK2DEditorUtility.DisableTK2D();
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
					SpineEditorUtilities.BoolPrefsField(ref timelineDefaultMixDuration, TIMELINE_DEFAULT_MIX_DURATION_KEY, new GUIContent("Default Mix Duration", "When enabled, the clip uses the default mix duration by default, as specified at the SkeletonDataAsset."));
					SpineEditorUtilities.BoolPrefsField(ref timelineUseBlendDuration, TIMELINE_USE_BLEND_DURATION_KEY, new GUIContent("Use Blend Duration", "When enabled, MixDuration will be synced with timeline clip transition duration 'Ease In Duration'."));
				}
			}
#endif // !NEW_PREFERENCES_SETTINGS_PROVIDER
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

		static void Texture2DPrefsField (ref string currentValue, string editorPrefsKey, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			EditorGUIUtility.wideMode = true;
			Texture2D texture = (EditorGUILayout.ObjectField(label, AssetDatabase.LoadAssetAtPath<Texture2D>(currentValue), typeof(Object), false) as Texture2D);
			currentValue = texture != null ? AssetDatabase.GetAssetPath(texture) : "";
			if (EditorGUI.EndChangeCheck()) {
				EditorPrefs.SetString(editorPrefsKey, currentValue);
			}
		}

		static void MaterialPrefsField (ref string currentValue, string editorPrefsKey, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			EditorGUIUtility.wideMode = true;
			Material material = (EditorGUILayout.ObjectField(label, AssetDatabase.LoadAssetAtPath<Material>(currentValue), typeof(Object), false) as Material);
			currentValue = material != null ? AssetDatabase.GetAssetPath(material) : "";
			if (EditorGUI.EndChangeCheck()) {
				EditorPrefs.SetString(editorPrefsKey, currentValue);
			}
		}

		public static void FloatPropertyField (SerializedProperty property, GUIContent label, float min = float.NegativeInfinity, float max = float.PositiveInfinity) {
			EditorGUI.BeginChangeCheck();
			property.floatValue = EditorGUILayout.DelayedFloatField(label, property.floatValue);
			if (EditorGUI.EndChangeCheck()) {
				property.floatValue = Mathf.Clamp(property.floatValue, min, max);
			}
		}

		public static void ShaderPropertyField (SerializedProperty property, GUIContent label, string fallbackShaderName) {
			Shader shader = (EditorGUILayout.ObjectField(label, Shader.Find(property.stringValue), typeof(Shader), false) as Shader);
			property.stringValue = shader != null ? shader.name : fallbackShaderName;
		}

		public static void MaterialPropertyField (SerializedProperty property, GUIContent label) {
			Material material = (EditorGUILayout.ObjectField(label, AssetDatabase.LoadAssetAtPath<Material>(property.stringValue), typeof(Material), false) as Material);
			property.stringValue = material ? AssetDatabase.GetAssetPath(material) : "";
		}

#if NEW_PREFERENCES_SETTINGS_PROVIDER
		public static void PresetAssetPropertyField (SerializedProperty property, GUIContent label) {
			UnityEditor.Presets.Preset texturePreset = (EditorGUILayout.ObjectField(label, AssetDatabase.LoadAssetAtPath<UnityEditor.Presets.Preset>(property.stringValue), typeof(UnityEditor.Presets.Preset), false) as UnityEditor.Presets.Preset);
			bool isTexturePreset = texturePreset != null && texturePreset.GetTargetTypeName() == "TextureImporter";
			property.stringValue = isTexturePreset ? AssetDatabase.GetAssetPath(texturePreset) : "";
		}
#endif
	}
}
