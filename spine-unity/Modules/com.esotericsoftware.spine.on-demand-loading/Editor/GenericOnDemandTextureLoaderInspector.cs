/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if UNITY_2017_2_OR_NEWER
#define NEWPLAYMODECALLBACKS
#endif

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
		protected SerializedProperty maxPlaceholderSize;
		protected SerializedProperty placeholderMap;
		protected SerializedProperty unloadAfterSecondsUnused;
		static protected bool placeholdersFoldout = true;
		protected SerializedProperty loadedDataAtMaterial;
		protected GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader;
		protected GUIContent placeholderTexturesLabel;

		/// <summary>
		/// Called via InitializeOnLoad attribute upon Editor startup or compilation.
		/// </summary>
		static GenericOnDemandTextureLoaderInspector () {
			RegisterPlayModeChangedCallbacks();
		}

		public static void RegisterPlayModeChangedCallbacks () {
#if NEWPLAYMODECALLBACKS
			EditorApplication.playModeStateChanged -= OnPlaymodeChanged;
			EditorApplication.playModeStateChanged += OnPlaymodeChanged;
#else
			EditorApplication.playmodeStateChanged -= OnPlaymodeChanged;
			EditorApplication.playmodeStateChanged += OnPlaymodeChanged;
#endif
		}

		/// <summary>
		/// Derive your implementation subclass of this class and implement the respective abstract methods.
		/// Note: Unfortunately the Unity menu entries are created via static methods, so this is a workaround
		/// to provide virtual static functions in old C# versions.
		/// </summary>
		public abstract class StaticMethodImplementations {

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

				int i = 0;
				foreach (Material targetMaterial in atlasAsset.Materials) {
					Texture targetTexture = targetMaterial.mainTexture;
					materialMap[i].textures = new GenericOnDemandTextureLoader<TargetReference, TextureRequest>.PlaceholderTextureMapping[1]; // Todo: currently only main texture is supported.
					int textureIndex = 0;

					GenericOnDemandTextureLoader<TargetReference, TextureRequest>.PlaceholderTextureMapping[] texturesMap = materialMap[i].textures;
					if (texturesMap[textureIndex].placeholderTexture != targetTexture) { // otherwise already set to placeholder
						SetupOnDemandLoadingReference(ref texturesMap[textureIndex].targetTextureReference, targetTexture);
						texturesMap[textureIndex].placeholderTexture = CreatePlaceholderTextureFor(targetTexture, maxPlaceholderSize, loader);
					}
					++i;
				}
				// assign late since CreatePlaceholderTextureFor(texture) method above might save assets and clear these values.
				loader.placeholderMap = materialMap;
				loader.atlasAsset = atlasAsset;
			}

			public virtual Texture CreatePlaceholderTextureFor (Texture originalTexture, int maxPlaceholderSize,
			GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader) {

				const string AssetFolderName = "LoadingPlaceholderAssets";
				string originalPath = AssetDatabase.GetAssetPath(originalTexture);
				string parentFolder = System.IO.Path.GetDirectoryName(originalPath);
				string dataPath = parentFolder + "/" + AssetFolderName;
				if (!AssetDatabase.IsValidFolder(dataPath)) {
					AssetDatabase.CreateFolder(parentFolder, AssetFolderName);
				}

				string originalTextureName = System.IO.Path.GetFileNameWithoutExtension(originalPath);
				string texturePath = string.Format("{0}/{1}.png",
					dataPath, loader.GetPlaceholderTextureName(originalTextureName));
				Texture placeholderTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
				if (placeholderTexture == null) {
					AssetDatabase.CopyAsset(originalPath, texturePath);

					const bool resizePhysically = true;

					TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texturePath);
					const string defaultPlatform = "Default";
					TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(defaultPlatform);
					settings.maxTextureSize = maxPlaceholderSize;
					importer.SetPlatformTextureSettings(settings);
					importer.maxTextureSize = maxPlaceholderSize;
					importer.isReadable = resizePhysically;
					importer.SaveAndReimport();

					if (resizePhysically) {
						bool hasOverrides = TextureImporterUtility.DisableOverrides(importer, out List<string> disabledPlatforms);

						Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
						if (texture2D) {
							Color[] maxTextureSizePixels = texture2D.GetPixels();

							// SetPixels supports only uncompressed textures using certain formats.
							Texture2D uncompressedTexture =
								new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBA32, false);
							uncompressedTexture.SetPixels(maxTextureSizePixels);

							byte[] bytes = uncompressedTexture.EncodeToPNG();
							string targetPath = Application.dataPath + "/../" + texturePath;
							System.IO.File.WriteAllBytes(targetPath, bytes);

							importer.isReadable = false;
							importer.SaveAndReimport();

							EditorUtility.SetDirty(uncompressedTexture);
							AssetDatabase.SaveAssets();
						}

						if (hasOverrides)
							TextureImporterUtility.EnableOverrides(importer, disabledPlatforms);
					}
					placeholderTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
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
			maxPlaceholderSize = serializedObject.FindProperty("maxPlaceholderSize");
			placeholderMap = serializedObject.FindProperty("placeholderMap");
			unloadAfterSecondsUnused = serializedObject.FindProperty("unloadAfterSecondsUnused");
			loadedDataAtMaterial = serializedObject.FindProperty("loadedDataAtMaterial");
			placeholderTexturesLabel = new GUIContent("Placeholder Textures");
			loader = (GenericOnDemandTextureLoader<TargetReference, TextureRequest>)target;

			if (staticMethods == null)
				staticMethods = CreateStaticMethodImplementations();
		}

#if NEWPLAYMODECALLBACKS
		static void OnPlaymodeChanged (PlayModeStateChange mode) {
			bool assignTargetTextures = mode == PlayModeStateChange.EnteredEditMode;
#else
		static void OnPlaymodeChanged () {
			bool assignTargetTextures = !Application.isPlaying;
#endif
			if (assignTargetTextures) {
				AssignTargetTexturesAtAllLoaders();
			}
		}

		public static void AssignTargetTexturesAtAllLoaders () {

			string[] loaderAssets = AssetDatabase.FindAssets("t:OnDemandTextureLoader");
			foreach (string loaderAsset in loaderAssets) {
				string assetPath = AssetDatabase.GUIDToAssetPath(loaderAsset);
				OnDemandTextureLoader loader = AssetDatabase.LoadAssetAtPath<OnDemandTextureLoader>(assetPath);
				AssignTargetTexturesAtLoader(loader);
			}
		}

		public static void AssignTargetTexturesAtLoader (OnDemandTextureLoader loader) {
			List<Material> placeholderMaterials;
			List<Material> nullTextureMaterials;
			bool anyPlaceholdersAssigned = loader.HasPlaceholderTexturesAssigned(out placeholderMaterials);
			bool anyMaterialNull = loader.HasNullMainTexturesAssigned(out nullTextureMaterials);
			if (anyPlaceholdersAssigned || anyMaterialNull) {
				Debug.Log("OnDemandTextureLoader detected placeholders assigned or null main textures at one or more materials. Resetting to target textures.", loader);
				AssetDatabase.StartAssetEditing();
				IEnumerable<Material> modifiedMaterials;
				loader.AssignTargetTextures(out modifiedMaterials);
				if (placeholderMaterials != null) {
					foreach (Material placeholderMaterial in placeholderMaterials) {
						EditorUtility.SetDirty(placeholderMaterial);
					}
				}
				if (nullTextureMaterials != null) {
					foreach (Material nullTextureMaterial in nullTextureMaterials) {
						EditorUtility.SetDirty(nullTextureMaterial);
					}
				}
				AssetDatabase.StopAssetEditing();
				AssetDatabase.SaveAssets();
			}
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
			EditorGUILayout.PropertyField(maxPlaceholderSize);
			EditorGUILayout.PropertyField(unloadAfterSecondsUnused);

			placeholdersFoldout = EditorGUILayout.Foldout(placeholdersFoldout, placeholderTexturesLabel, true);
			if (placeholdersFoldout) {
				for (int m = 0, materialCount = placeholderMap.arraySize; m < materialCount; ++m) {
					// line below equals: PlaceholderTextureMapping[] materialTextures = placeholderMap[m].textures;
					SerializedProperty materialTextures = placeholderMap.GetArrayElementAtIndex(m).FindPropertyRelative("textures");

					for (int t = 0, textureCount = materialTextures.arraySize; t < textureCount; ++t) {
						// line below equals: PlaceholderTextureMapping textureMapping = materialTextures[t];
						SerializedProperty textureMapping = materialTextures.GetArrayElementAtIndex(t);
						DrawPlaceholderMapping(textureMapping);
					}
				}
			}

			if (GUILayout.Button(new GUIContent("Regenerate", "Re-initialize the placeholder texture maps."), EditorStyles.miniButton, GUILayout.Width(160f)))
				ReinitPlaceholderTextures(loader);

			GUILayout.Space(16f);
			EditorGUILayout.LabelField("Testing", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight + 5));
			if (GUILayout.Button(new GUIContent("Assign Placeholders", "Assign placeholder textures (for testing)."), EditorStyles.miniButton, GUILayout.Width(160f)))
				AssignPlaceholderTextures(loader);
			if (GUILayout.Button(new GUIContent("Assign Normal Textures", "Re-assign target textures."), EditorStyles.miniButton, GUILayout.Width(160f)))
				AssignTargetTextures(loader);
			EditorGUILayout.EndHorizontal();

			if (!Application.isPlaying)
				serializedObject.ApplyModifiedProperties();
		}

		public void DeletePlaceholderTextures (GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader) {
			foreach (var materialMap in loader.placeholderMap) {
				var textures = materialMap.textures;
				if (textures == null || textures.Length == 0)
					continue;

				Texture texture = textures[0].placeholderTexture;
				if (texture)
					AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(texture));
			}
			loader.Clear(clearAtlasAsset: false);
			AssetDatabase.SaveAssets();
		}

		public void ReinitPlaceholderTextures (GenericOnDemandTextureLoader<TargetReference, TextureRequest> loader) {
			AssignTargetTextures(loader);
			DeletePlaceholderTextures(loader);
			staticMethods.SetupForAtlasAsset(loader, loader.atlasAsset);
			EditorUtility.SetDirty(loader);
			AssetDatabase.SaveAssets();
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
