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

#define SPINE_OPTIONAL_ON_DEMAND_LOADING

#if SPINE_OPTIONAL_ON_DEMAND_LOADING

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	/// <summary>
	/// Base class for GenericOnDemandTextureLoader Inspector subclasses.
	/// For reference, see the <see cref="AddressablesTextureLoaderInspector"/> class available
	/// in the com.esotericsoftware.spine.addressables UPM package.
	/// </summary>
	/// <typeparam name="TargetReference">The implementation struct which holds an on-demand loading reference
	/// to the target texture to be loaded, derived from ITargetTextureReference.</typeparam>
	/// <typeparam name="TextureRequest">The implementation struct covering a single texture loading request,
	/// derived from IOnDemandRequest</typeparam>
	[InitializeOnLoad]
	[CustomEditor(typeof(GenericOnDemandTextureLoader<,>)), CanEditMultipleObjects]
	public abstract class GenericOnDemandTextureLoaderInspector<TargetReference, TextureRequest> : UnityEditor.Editor
		where TargetReference : Spine.Unity.ITargetTextureReference
		where TextureRequest : Spine.Unity.IOnDemandRequest {

		protected SerializedProperty atlasAsset;
		protected SerializedProperty skeletonDataAsset;
		protected SerializedProperty maxPlaceholderSize;
		protected SerializedProperty placeholderMap;
		protected SerializedProperty unloadAfterSecondsUnused;
		protected SerializedProperty additionalTextureProperties;
		static protected bool placeholdersFoldout = true;
		static GUIStyle boldFoldoutStyle;
		static GUIStyle BoldFoldoutStyle {
			get {
				if (boldFoldoutStyle == null) {
					boldFoldoutStyle = new GUIStyle(EditorStyles.foldout);
					boldFoldoutStyle.fontStyle = FontStyle.Bold;
				}
				return boldFoldoutStyle;
			}
		}
		protected SerializedProperty loadedDataAtMaterial;
		protected GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader;
		protected GUIContent placeholderTexturesLabel;
		protected GUIContent additionalTexturePropertiesLabel;

		const string STARTUP_RECOVERY_DONE_KEY = "SPINE_ON_DEMAND_STARTUP_RECOVERY_DONE";
		protected const string PlaceholderAssetFolderName = "LoadingPlaceholderAssets";

		/// <summary>
		/// Called via InitializeOnLoad attribute upon Editor startup or compilation.
		/// </summary>
		static GenericOnDemandTextureLoaderInspector () {
			RegisterPlayModeChangedCallbacks();
		}

		public static void RegisterPlayModeChangedCallbacks () {
			EditorApplication.playModeStateChanged -= OnPlaymodeChanged;
			EditorApplication.playModeStateChanged += OnPlaymodeChanged;
			// SessionState must not be accessed here: the static ctor may run from the Editor's ScriptableObject
			// constructor, e.g. when the loader is shown in the Inspector during a domain reload.
			EditorApplication.delayCall -= RestoreTargetTexturesAfterEditorLoad;
			EditorApplication.delayCall += RestoreTargetTexturesAfterEditorLoad;
		}

		static void RestoreTargetTexturesAfterEditorLoad () {
			if (SessionState.GetBool(STARTUP_RECOVERY_DONE_KEY, false)) return;
			if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return;

			RecoverTargetTexturesAfterInterruptedBuild();
			SessionState.SetBool(STARTUP_RECOVERY_DONE_KEY, true);
		}

		/// <summary>
		/// Derive your implementation subclass of this class and implement the respective abstract methods.
		/// Note: Unfortunately the Unity menu entries are created via static methods, so this is a workaround
		/// to provide virtual static functions in old C# versions.
		/// </summary>
		public abstract class StaticMethodImplementations {

			List<string> regeneratedPlaceholderPaths;

			public abstract GenericOnDemandTextureLoader<TargetReference, TextureRequest> GetOrCreateLoader (string loaderPath);

			/// <summary>
			/// Returns the on-demand loader asset's filename suffix. The filename
			/// is determined by the AtlasAsset, while this suffix replaces the "_Atlas" suffix.
			/// When set to e.g. "_Addressable", the loader asset created for
			/// the "Skeleton_Atlas" asset is named "Skeleton_Addressable".
			/// </summary>
			public virtual string LoaderSuffix { get { return "_Loader"; } }

			public abstract bool SetupOnDemandLoadingReference (
				ref TargetReference targetTextureReference, Texture targetTexture);

			/// <summary>
			/// Create a context menu wrapper in the main class for this generic implementation using the code below.
			/// <code>
			/// [MenuItem("CONTEXT/AtlasAssetBase/Add YourSubclass Loader")]
			///	static void AddYourSubclassLoader (MenuCommand cmd) {
			///		if (staticMethods == null)
			///			staticMethods = new YourSubclassMethodImplementations ();
			///		staticMethods.AddOnDemandLoader(cmd);
			///	}
			/// </code>
			/// </summary>
			public virtual void AddOnDemandLoader (MenuCommand cmd) {
				AtlasAssetBase atlasAsset = cmd.context as AtlasAssetBase;
				Debug.Log("Adding On-Demand Loader for " + atlasAsset.name, atlasAsset);

				if (atlasAsset.OnDemandTextureLoader != null) {
					Debug.LogWarning("AtlasAsset On-Demand TextureLoader is already set. " +
						"Please clear it if you want to assign a different one.");
					return;
				}

				atlasAsset.TextureLoadingMode = AtlasAssetBase.LoadingMode.OnDemand;
				EditorUtility.SetDirty(atlasAsset);

				string atlasAssetPath = AssetDatabase.GetAssetPath(atlasAsset);
				string loaderPath = atlasAssetPath.Replace(AssetUtility.AtlasSuffix, LoaderSuffix);

				GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader = staticMethods.GetOrCreateLoader(loaderPath);
				staticMethods.SetupForAtlasAsset(loader, atlasAsset);

				EditorUtility.SetDirty(loader);
				AssetDatabase.SaveAssets();
			}

			/// <summary>Rebuilds the map and refreshes each placeholder in place, preserving its GUID and references
			/// from other loaders. Shared textures use the settings of the loader most recently regenerated.</summary>
			public virtual void RegenerateForAtlasAsset (GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader, AtlasAssetBase atlasAsset) {
				List<string> previousPaths = regeneratedPlaceholderPaths;
				regeneratedPlaceholderPaths = new List<string>();
				try {
					SetupForAtlasAsset(loader, atlasAsset);
				} finally {
					regeneratedPlaceholderPaths = previousPaths;
				}
			}

			public virtual void SetupForAtlasAsset (GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader, AtlasAssetBase atlasAsset) {
				if (loader.placeholderMap != null && loader.placeholderMap.Length > 0) {
					IEnumerable<Material> modifiedMaterials;
					loader.AssignTargetTextures(out modifiedMaterials); // start from normal textures
				}

				if (atlasAsset == null) {
					Debug.LogError("AddressableTextureLoader.SetupForAtlasAsset: atlasAsset was null, aborting setup.", atlasAsset);
					return;
				}

				int materialCount = atlasAsset.MaterialCount;
				loader.placeholderMap = new GenericOnDemandTextureLoader<TargetReference, TextureRequest>.PlaceholderMaterialMapping[materialCount];
				GenericOnDemandTextureLoader<TargetReference, TextureRequest>.PlaceholderMaterialMapping[] materialMap = loader.placeholderMap;

				atlasAsset.OnDemandTextureLoader = loader;
				int maxPlaceholderSize = loader.maxPlaceholderSize;

				// Texture index 0 is always the main texture, index i + 1 is additionalTextureProperties[i].
				string[] additionalProperties = loader.additionalTextureProperties ?? new string[0];
				int textureCount = 1 + additionalProperties.Length;
				bool[] additionalPropertyFound = new bool[additionalProperties.Length];

				int i = 0;
				foreach (Material targetMaterial in atlasAsset.Materials) {
					materialMap[i].textures = new GenericOnDemandTextureLoader<TargetReference, TextureRequest>.PlaceholderTextureMapping[textureCount];
					GenericOnDemandTextureLoader<TargetReference, TextureRequest>.PlaceholderTextureMapping[] texturesMap = materialMap[i].textures;
					int mainTexturePropertyID = GenericOnDemandTextureLoader<TargetReference, TextureRequest>.GetMainTexturePropertyID(targetMaterial);

					for (int textureIndex = 0; textureIndex < textureCount; ++textureIndex) {
						Texture targetTexture;
						if (textureIndex == 0) {
							targetTexture = targetMaterial.mainTexture;
						} else {
							int propertyIndex = textureIndex - 1;
							if (!GenericOnDemandTextureLoader<TargetReference, TextureRequest>.IsValidAdditionalTextureProperty(additionalProperties, propertyIndex)) continue;
							string propertyName = additionalProperties[propertyIndex];
							if (!targetMaterial.HasProperty(propertyName)) continue;
							additionalPropertyFound[propertyIndex] = true;
							// the main texture is always covered by texture index 0.
							if (Shader.PropertyToID(propertyName) == mainTexturePropertyID) continue;
							targetTexture = targetMaterial.GetTexture(propertyName);
						}
						// null or built-in textures leave an empty mapping entry.
						if (!GenericOnDemandTextureLoader<TargetReference, TextureRequest>.IsTextureAsset(targetTexture)) continue;

						SetupOnDemandLoadingReference(ref texturesMap[textureIndex].targetTextureReference, targetTexture);
						texturesMap[textureIndex].placeholderTexture = CreatePlaceholderTextureFor(targetTexture, maxPlaceholderSize, loader);
					}
					++i;
				}
				for (int propertyIndex = 0; propertyIndex < additionalProperties.Length; ++propertyIndex) {
					if (GenericOnDemandTextureLoader<TargetReference, TextureRequest>.IsValidAdditionalTextureProperty(additionalProperties, propertyIndex) &&
						!additionalPropertyFound[propertyIndex]) {
						Debug.LogWarning(string.Format("Additional texture property '{0}' not found at any material of {1}.",
							additionalProperties[propertyIndex], atlasAsset.name), loader);
					}
				}
				// assign late since CreatePlaceholderTextureFor(texture) method above might save assets and clear these values.
				loader.placeholderMap = materialMap;
				loader.atlasAsset = atlasAsset;
				if (loader.skeletonDataAsset == null)
					AssignSkeletonDataAsset(loader, atlasAsset);
			}

			protected void AssignSkeletonDataAsset (GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader, AtlasAssetBase atlasAsset) {
				string atlasAssetPath = AssetDatabase.GetAssetPath(atlasAsset);
				string parentFolder = System.IO.Path.GetDirectoryName(atlasAssetPath);

				SkeletonDataAsset skeletonDataAsset = FindSkeletonDataAsset(parentFolder, atlasAsset);
				if (skeletonDataAsset) {
					loader.skeletonDataAsset = skeletonDataAsset;
					return;
				}
				string nextParentFolder = System.IO.Path.GetDirectoryName(parentFolder);
				skeletonDataAsset = FindSkeletonDataAsset(nextParentFolder, atlasAsset);
				if (skeletonDataAsset) {
					loader.skeletonDataAsset = skeletonDataAsset;
					return;
				}
			}

			protected SkeletonDataAsset FindSkeletonDataAsset (string searchFolder, AtlasAssetBase atlasAsset) {
				string[] guids = AssetDatabase.FindAssets("t:SkeletonDataAsset", new[] { searchFolder });
				foreach (string guid in guids) {
					string assetPath = AssetDatabase.GUIDToAssetPath(guid);
					SkeletonDataAsset skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(assetPath);
					if (skeletonDataAsset != null) {
						if (skeletonDataAsset.atlasAssets.Contains(atlasAsset)) {
							return skeletonDataAsset;
						}
					}
				}
				return null;
			}

			public virtual Texture CreatePlaceholderTextureFor (Texture originalTexture, int maxPlaceholderSize,
			GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader) {

				string originalPath = AssetDatabase.GetAssetPath(originalTexture);
				string parentFolder = System.IO.Path.GetDirectoryName(originalPath);
				string dataPath = parentFolder + "/" + PlaceholderAssetFolderName;
				if (!AssetDatabase.IsValidFolder(dataPath)) {
					AssetDatabase.CreateFolder(parentFolder, PlaceholderAssetFolderName);
				}

				string originalTextureName = System.IO.Path.GetFileNameWithoutExtension(originalPath);
				string texturePath = string.Format("{0}/{1}.png",
					dataPath, loader.GetPlaceholderTextureName(originalTextureName));
				Texture placeholderTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
				bool regenerate = regeneratedPlaceholderPaths != null && !regeneratedPlaceholderPaths.Contains(texturePath);
				if (placeholderTexture == null || regenerate) {
					string targetPath = Application.dataPath + "/../" + texturePath;
					if (placeholderTexture == null) {
						if (!AssetDatabase.CopyAsset(originalPath, texturePath))
							throw new InvalidOperationException("Failed to create placeholder texture: " + texturePath);
					} else {
						// Copy only the source image, not its .meta file: existing placeholder GUIDs must remain intact.
						System.IO.File.Copy(Application.dataPath + "/../" + originalPath, targetPath, true);
						TextureImporter sourceImporter = (TextureImporter)TextureImporter.GetAtPath(originalPath);
						TextureImporter targetImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
						EditorUtility.CopySerialized(sourceImporter, targetImporter);
					}

					TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texturePath);
					TextureImporterType originalTextureType = importer.textureType;
					List<string> disabledPlatforms = null;
					try {
						const string defaultPlatform = "Default";
						TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(defaultPlatform);
						settings.maxTextureSize = maxPlaceholderSize;
						importer.SetPlatformTextureSettings(settings);
						importer.maxTextureSize = maxPlaceholderSize;
						importer.isReadable = true;
						// Read source pixels, not the encoded representation of a normal map.
						if (originalTextureType != TextureImporterType.Default)
							importer.textureType = TextureImporterType.Default;
						importer.SaveAndReimport();
						TextureImporterUtility.DisableOverrides(importer, out disabledPlatforms);

						Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
						if (!texture2D)
							throw new InvalidOperationException("Failed to import placeholder texture: " + texturePath);

						// SetPixels supports only uncompressed textures using certain formats.
						Texture2D uncompressedTexture = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBA32, false);
						try {
							uncompressedTexture.SetPixels(texture2D.GetPixels());
							System.IO.File.WriteAllBytes(targetPath, uncompressedTexture.EncodeToPNG());
						} finally {
							UnityEngine.Object.DestroyImmediate(uncompressedTexture);
						}
					} finally {
						importer.isReadable = false;
						importer.textureType = originalTextureType;
						if (disabledPlatforms != null && disabledPlatforms.Count > 0)
							TextureImporterUtility.EnableOverrides(importer, disabledPlatforms);
						else
							importer.SaveAndReimport();
					}
					AssetDatabase.SaveAssets();
					placeholderTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
					if (regeneratedPlaceholderPaths != null)
						regeneratedPlaceholderPaths.Add(texturePath);
				}

				UnityEngine.Object folderObject = AssetDatabase.LoadAssetAtPath(dataPath, typeof(UnityEngine.Object));
				if (folderObject != null) {
					EditorGUIUtility.PingObject(folderObject);
				}

				return placeholderTexture;
			}
		}
		public static StaticMethodImplementations staticMethods;

		void OnEnable () {
			atlasAsset = serializedObject.FindProperty("atlasAsset");
			skeletonDataAsset = serializedObject.FindProperty("skeletonDataAsset");
			maxPlaceholderSize = serializedObject.FindProperty("maxPlaceholderSize");
			placeholderMap = serializedObject.FindProperty("placeholderMap");
			unloadAfterSecondsUnused = serializedObject.FindProperty("unloadAfterSecondsUnused");
			additionalTextureProperties = serializedObject.FindProperty("additionalTextureProperties");
			loadedDataAtMaterial = serializedObject.FindProperty("loadedDataAtMaterial");
			placeholderTexturesLabel = new GUIContent("Placeholder Textures");
			additionalTexturePropertiesLabel = new GUIContent("Additional Texture Properties",
				"Shader texture property names (e.g. '_BumpMap' for normal maps) to load on demand in addition to " +
				"the main texture. Placeholder textures are setup automatically for matching textures at the AtlasAsset materials.");
			loader = (GenericOnDemandTextureLoader<TargetReference, TextureRequest>)target;

			if (staticMethods == null)
				staticMethods = CreateStaticMethodImplementations();
		}

		static void OnPlaymodeChanged (PlayModeStateChange mode) {
			if (mode == PlayModeStateChange.EnteredEditMode) {
				AssignTargetTexturesAtAllLoaders();
			}
		}

		public static void AssignTargetTexturesAtAllLoaders () {
			if (BuildPipeline.isBuildingPlayer) return;

			string[] loaderAssets = AssetDatabase.FindAssets("t:OnDemandTextureLoader");
			foreach (string loaderAsset in loaderAssets) {
				string assetPath = AssetDatabase.GUIDToAssetPath(loaderAsset);
				OnDemandTextureLoader loader = AssetDatabase.LoadAssetAtPath<OnDemandTextureLoader>(assetPath);
				AssignTargetTexturesAtLoader(loader);
			}
		}

		static void RecoverTargetTexturesAfterInterruptedBuild () {
			List<OnDemandTextureLoader> loadersToRestore = new List<OnDemandTextureLoader>();
			// only consider active loaders which have had their textures replaced to placeholders.
			List<OnDemandTextureLoader> loadersWithLeftoverPlaceholders = new List<OnDemandTextureLoader>();
			string[] loaderAssets = AssetDatabase.FindAssets("t:OnDemandTextureLoader");
			foreach (string loaderAsset in loaderAssets) {
				string assetPath = AssetDatabase.GUIDToAssetPath(loaderAsset);
				OnDemandTextureLoader loader = AssetDatabase.LoadAssetAtPath<OnDemandTextureLoader>(assetPath);
				bool hadPlaceholders;
				if (!HasTargetTexturesToRestore(loader, out hadPlaceholders)) continue;

				loadersToRestore.Add(loader);
				if (hadPlaceholders)
					loadersWithLeftoverPlaceholders.Add(loader);
			}

			AssignTargetTexturesAtAdditionalMaterials(loadersWithLeftoverPlaceholders);
			foreach (OnDemandTextureLoader loader in loadersToRestore)
				RestoreTargetTexturesAtLoader(loader);
		}

		public static void AssignTargetTexturesAtLoader (OnDemandTextureLoader loader) {
			bool hadPlaceholders;
			if (HasTargetTexturesToRestore(loader, out hadPlaceholders))
				RestoreTargetTexturesAtLoader(loader);
		}

		static bool HasTargetTexturesToRestore (OnDemandTextureLoader loader, out bool hadPlaceholders) {
			hadPlaceholders = false;
			if (!loader) return false;

			List<Material> unusedMaterials;
			hadPlaceholders = loader.HasPlaceholderTexturesAssigned(out unusedMaterials);
			bool anyMaterialNull = loader.HasNullMainTexturesAssigned(out unusedMaterials);
			return hadPlaceholders || anyMaterialNull;
		}

		static void RestoreTargetTexturesAtLoader (OnDemandTextureLoader loader) {
			Debug.Log("OnDemandTextureLoader detected placeholders assigned or null main textures at one or more materials. Resetting to target textures.", loader);
			AssetDatabase.StartAssetEditing();
			try {
				IEnumerable<Material> modifiedMaterials;
				loader.AssignTargetTextures(out modifiedMaterials);
				foreach (Material material in modifiedMaterials)
					EditorUtility.SetDirty(material);
			} finally {
				AssetDatabase.StopAssetEditing();
			}
			AssetDatabase.SaveAssets();
		}

		static void AssignTargetTexturesAtAdditionalMaterials (List<OnDemandTextureLoader> loaders) {
			if (loaders.Count == 0) return;

			HashSet<string> materialAssetPaths = new HashSet<string>();
			string[] materialAssetGuids = AssetDatabase.FindAssets("t:Material");
			foreach (string materialAssetGuid in materialAssetGuids)
				materialAssetPaths.Add(AssetDatabase.GUIDToAssetPath(materialAssetGuid));

			bool anyMaterialModified = false;
			AssetDatabase.StartAssetEditing();
			try {
				foreach (string assetPath in materialAssetPaths) {
					UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
					foreach (UnityEngine.Object asset in assets) {
						Material material = asset as Material;
						if (!material) continue;

						bool anyRestored = false;
						foreach (OnDemandTextureLoader loader in loaders) {
							if (loader.RestoreTargetTextures(material))
								anyRestored = true;
						}
						if (!anyRestored) continue;

						Debug.Log(string.Format("OnDemandTextureLoader recovered target textures from placeholders at Material '{0}'.",
							assetPath), material);
						EditorUtility.SetDirty(material);
						anyMaterialModified = true;
					}
				}
			} finally {
				AssetDatabase.StopAssetEditing();
			}
			if (anyMaterialModified)
				AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// Override this method in your implementation subclass as follows.
		/// <code>
		/// protected override StaticMethodImplementations CreateStaticMethodImplementations () {
		///		return new YourStaticMethodImplementationsSubclass();
		/// }
		/// </code>
		/// </summary>
		protected abstract StaticMethodImplementations CreateStaticMethodImplementations ();

		/// <summary>Draws a single texture mapping entry in the Inspector.
		/// Can be overridden in subclasses where needed. Note that DrawSingleLineTargetTextureProperty
		/// can be overridden as well instead of overriding this method.
		/// Note that for the sake of space it should be drawn as a single line if possible.
		/// </summary>
		/// <param name="textureMapping">SerializedProperty pointing to a
		/// PlaceholderTextureMapping object of the placeholderMap array.</param>
		protected virtual void DrawPlaceholderMapping (SerializedProperty textureMapping) {
			EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight + 5));
			var placeholderTextureProp = textureMapping.FindPropertyRelative("placeholderTexture");
			var targetTextureProp = textureMapping.FindPropertyRelative("targetTextureReference");
			GUILayout.Space(16f);
			EditorGUILayout.PropertyField(placeholderTextureProp, GUIContent.none);
			EditorGUIUtility.labelWidth = 1; // workaround since GUIContent.none below seems to be ignored

			DrawSingleLineTargetTextureProperty(targetTextureProp);
			EditorGUIUtility.labelWidth = 0; // change back to default
			EditorGUILayout.EndHorizontal();
		}

		/// <summary>Draws the texture property name as label, followed by the texture mapping entry via
		/// <see cref="DrawPlaceholderMapping(SerializedProperty)"/>.</summary>
		/// <param name="textureMapping">SerializedProperty pointing to a
		/// PlaceholderTextureMapping object of the placeholderMap array.</param>
		/// <param name="texturePropertyName">Name of the texture property of the mapping entry, used as label.</param>
		protected virtual void DrawPlaceholderMapping (SerializedProperty textureMapping, string texturePropertyName) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(texturePropertyName, EditorStyles.miniLabel, GUILayout.Width(90f));
			DrawPlaceholderMapping(textureMapping);
			EditorGUILayout.EndHorizontal();
		}

		/// <returns>The name of the texture property at the given texture index, for display purposes.</returns>
		protected string GetTexturePropertyName (int textureIndex) {
			if (textureIndex == 0) return "Main Texture";
			string[] additionalProperties = loader.additionalTextureProperties;
			int propertyIndex = textureIndex - 1;
			return additionalProperties != null && propertyIndex < additionalProperties.Length ?
				additionalProperties[propertyIndex] : "";
		}

		/// <summary>Draws a single texture mapping TargetReference in the Inspector.
		/// Can be overridden in subclasses where needed. Note that this method is
		/// called inside a horizontal Inspector line of a BeginHorizontal() / EndHorizontal()
		/// pair, so it is limited to approximately half Inspector width.
		/// </summary>
		/// <param name="property">SerializedProperty pointing to a
		/// TargetReference object of the PlaceholderTextureMapping entry.</param>
		protected virtual void DrawSingleLineTargetTextureProperty (SerializedProperty property) {
			EditorGUILayout.PropertyField(property, GUIContent.none, true);
		}

		public override void OnInspectorGUI () {
			if (serializedObject.isEditingMultipleObjects) {
				DrawDefaultInspector();
				return;
			}

			serializedObject.Update();

			EditorGUILayout.PropertyField(atlasAsset);
			EditorGUILayout.PropertyField(skeletonDataAsset);
			EditorGUILayout.PropertyField(maxPlaceholderSize);
			EditorGUILayout.PropertyField(unloadAfterSecondsUnused);

			bool additionalTexturePropertiesChanged;
			bool previousHierarchyMode = EditorGUIUtility.hierarchyMode;
			EditorGUIUtility.hierarchyMode = false; // draws foldout arrows inside the boxes below instead of left of them.
			try {
				EditorGUILayout.Space();
				using (new SpineInspectorUtility.BoxScope(false)) {
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(additionalTextureProperties, additionalTexturePropertiesLabel, true);
					additionalTexturePropertiesChanged = EditorGUI.EndChangeCheck();

					EditorGUI.BeginDisabledGroup(Application.isPlaying || loader.atlasAsset == null);
					EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight + 5));
					if (GUILayout.Button(new GUIContent("Add All Textures", "Adds all texture properties which have a texture assigned " +
						"at any AtlasAsset material to the Additional Texture Properties list."), EditorStyles.miniButton, GUILayout.Width(160f))) {
						serializedObject.ApplyModifiedProperties(); // apply pending list edits before modifying the list directly.
						if (loader.AddAllTextureProperties() > 0) {
							serializedObject.Update();
							additionalTexturePropertiesChanged = true;
						}
					}
					EditorGUILayout.EndHorizontal();
					EditorGUI.EndDisabledGroup();
				}

				if (!Application.isPlaying && loader.atlasAsset != null && !loader.ValidateSetup(false)) {
					EditorGUILayout.HelpBox("The loader setup is invalid, e.g. the placeholder texture map does not match the AtlasAsset " +
						"materials after textures were changed, or a target texture can't be loaded on demand. Details are logged as warnings " +
						"when building. The loader is skipped when building until fixed. Hit 'Regenerate' to update the placeholder map.", MessageType.Warning);
				}

				EditorGUILayout.Space();
				using (new SpineInspectorUtility.BoxScope(false)) {
					placeholdersFoldout = EditorGUILayout.Foldout(placeholdersFoldout, placeholderTexturesLabel, true, BoldFoldoutStyle);
					if (placeholdersFoldout) {
						for (int m = 0, materialCount = placeholderMap.arraySize; m < materialCount; ++m) {
							// line below equals: PlaceholderTextureMapping[] materialTextures = placeholderMap[m].textures;
							SerializedProperty materialTextures = placeholderMap.GetArrayElementAtIndex(m).FindPropertyRelative("textures");

							for (int t = 0, textureCount = materialTextures.arraySize; t < textureCount; ++t) {
								// line below equals: PlaceholderTextureMapping textureMapping = materialTextures[t];
								SerializedProperty textureMapping = materialTextures.GetArrayElementAtIndex(t);
								// skip empty entries of additional texture properties not used at this material.
								if (t > 0 && textureMapping.FindPropertyRelative("placeholderTexture").objectReferenceValue == null)
									continue;
								DrawPlaceholderMapping(textureMapping, GetTexturePropertyName(t));
							}
						}
					}

					EditorGUI.BeginDisabledGroup(Application.isPlaying);
					if (GUILayout.Button(new GUIContent("Regenerate", "Re-initialize the placeholder texture maps."), EditorStyles.miniButton, GUILayout.Width(160f)))
						ReinitPlaceholderTextures(loader);
					EditorGUI.EndDisabledGroup();

					GUILayout.Space(16f);
					EditorGUILayout.LabelField("Testing", EditorStyles.boldLabel);
					EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight + 5));
					if (GUILayout.Button(new GUIContent("Assign Placeholders", "Assign placeholder textures for testing (only atlas materials)."), EditorStyles.miniButton, GUILayout.Width(160f)))
						AssignPlaceholderTextures(loader);
					if (GUILayout.Button(new GUIContent("Assign Normal Textures", "Re-assign target textures."), EditorStyles.miniButton, GUILayout.Width(160f)))
						AssignTargetTextures(loader);
					EditorGUILayout.EndHorizontal();
				}
			} finally {
				EditorGUIUtility.hierarchyMode = previousHierarchyMode;
			}

			if (!Application.isPlaying) {
				serializedObject.ApplyModifiedProperties();
				if (additionalTexturePropertiesChanged)
					UpdatePlaceholderTextures(loader);
			}
		}

		public void DeletePlaceholderTextures (GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader) {
			List<OnDemandTextureLoader> otherLoaders = GetOtherLoaders(loader);
			foreach (var materialMap in loader.placeholderMap) {
				var textures = materialMap.textures;
				if (textures == null || textures.Length == 0)
					continue;

				for (int t = 0; t < textures.Length; ++t) {
					Texture texture = textures[t].placeholderTexture;
					if (texture && !IsPlaceholderReferenced(otherLoaders, texture))
						AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(texture));
				}
			}
			loader.Clear(clearAtlasAsset: false);
			AssetDatabase.SaveAssets();
		}

		/// <summary>Returns all other loader assets, regardless of their loading backend. Loaders using the same
		/// target texture share its placeholder asset, which must not be deleted while another loader references it.</summary>
		protected static List<OnDemandTextureLoader> GetOtherLoaders (OnDemandTextureLoader loader) {
			List<OnDemandTextureLoader> otherLoaders = new List<OnDemandTextureLoader>();
			string[] loaderAssets = AssetDatabase.FindAssets("t:OnDemandTextureLoader");
			foreach (string loaderAsset in loaderAssets) {
				string assetPath = AssetDatabase.GUIDToAssetPath(loaderAsset);
				OnDemandTextureLoader otherLoader = AssetDatabase.LoadAssetAtPath<OnDemandTextureLoader>(assetPath);
				if (otherLoader != null && otherLoader != loader)
					otherLoaders.Add(otherLoader);
			}
			return otherLoaders;
		}

		static bool IsPlaceholderReferenced (List<OnDemandTextureLoader> loaders, Texture texture) {
			foreach (OnDemandTextureLoader loader in loaders) {
				if (loader.IsPlaceholderTexture(texture)) return true;
			}
			return false;
		}

		public void ReinitPlaceholderTextures (GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader) {
			if (Application.isPlaying || loader.atlasAsset == null) return;
			GenericOnDemandTextureLoader<TargetReference, TextureRequest>.PlaceholderMaterialMapping[] previousMap = loader.placeholderMap;
			RestoreTargetTexturesBeforeDeletingPlaceholders(loader);
			loader.Clear(clearAtlasAsset: false);
			staticMethods.RegenerateForAtlasAsset(loader, loader.atlasAsset);
			DeleteUnusedPlaceholderTextures(loader, previousMap);
			EditorUtility.SetDirty(loader);
			AssetDatabase.SaveAssets();
		}

		/// <summary>Replaces all assigned placeholder textures with their target textures before the placeholder
		/// textures are deleted, so that no texture references are lost. Placeholders are identified by texture,
		/// so this also covers texture properties no longer listed at the loader.</summary>
		protected static void RestoreTargetTexturesBeforeDeletingPlaceholders (GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader) {
			List<Material> restoredMaterials;
			if (!loader || !loader.RestoreTargetTextures(out restoredMaterials)) return;

			foreach (Material material in restoredMaterials)
				EditorUtility.SetDirty(material);
			// Only scan the project when the loader's own materials had placeholders assigned (e.g. after an interrupted
			// build), as additional material assets only receive placeholders together with them in the pre-build step.
			AssignTargetTexturesAtAdditionalMaterials(new List<OnDemandTextureLoader> { loader });
		}

		/// <summary>Updates the placeholder map after <c>additionalTextureProperties</c> changed. Existing placeholder
		/// textures are kept, missing ones are created and placeholder textures no longer referenced are deleted.</summary>
		public void UpdatePlaceholderTextures (GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader) {
			if (loader.atlasAsset == null) return;

			GenericOnDemandTextureLoader<TargetReference, TextureRequest>.PlaceholderMaterialMapping[] previousMap = loader.placeholderMap;
			RestoreTargetTexturesBeforeDeletingPlaceholders(loader);
			loader.Clear(clearAtlasAsset: false); // ensures cached property IDs and loaded data are recreated for the new list.
			staticMethods.SetupForAtlasAsset(loader, loader.atlasAsset);
			DeleteUnusedPlaceholderTextures(loader, previousMap);
			EditorUtility.SetDirty(loader);
			AssetDatabase.SaveAssets();
		}

		/// <summary>Deletes generated placeholder texture assets of the previous map which are no longer referenced
		/// by the loader's current map or by any other loader. Only textures inside a generated placeholder folder are deleted.</summary>
		protected static void DeleteUnusedPlaceholderTextures (GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader,
			GenericOnDemandTextureLoader<TargetReference, TextureRequest>.PlaceholderMaterialMapping[] previousMap) {

			if (previousMap == null) return;
			List<OnDemandTextureLoader> otherLoaders = GetOtherLoaders(loader);
			foreach (var materialMap in previousMap) {
				var textures = materialMap.textures;
				if (textures == null) continue;

				for (int t = 0; t < textures.Length; ++t) {
					Texture texture = textures[t].placeholderTexture;
					if (!texture || loader.IsPlaceholderTexture(texture) || IsPlaceholderReferenced(otherLoaders, texture)) continue;

					string assetPath = AssetDatabase.GetAssetPath(texture);
					if (System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(assetPath)) != PlaceholderAssetFolderName)
						continue;
					AssetDatabase.DeleteAsset(assetPath);
				}
			}
		}

		public bool AssignPlaceholderTextures (GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader) {
			// re-setup placeholders to ensure the mapping is up to date.
			staticMethods.SetupForAtlasAsset(loader, loader.atlasAsset);
			IEnumerable<Material> modifiedMaterials;
			return loader.AssignPlaceholderTextures(out modifiedMaterials);
		}

		public bool AssignTargetTextures (GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader) {
			IEnumerable<Material> modifiedMaterials;
			return loader.AssignTargetTextures(out modifiedMaterials);
		}
	}
}
#endif
