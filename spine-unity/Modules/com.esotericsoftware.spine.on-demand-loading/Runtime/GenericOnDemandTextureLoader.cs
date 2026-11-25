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
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Spine.Unity {
	using ReplacementMaterial = BlendModeMaterials.ReplacementMaterial;

	/// <summary>
	/// Interface to derive a concrete target reference struct from which holds
	/// an on-demand loading reference to the target texture to be loaded.
	/// </summary>
	public interface ITargetTextureReference {
#if UNITY_EDITOR
		Texture EditorTexture { get; }
#endif
	}

	/// <summary>
	/// Interface to derive a concrete request handler struct from which covers
	/// a single texture loading request.
	/// </summary>
	public interface IOnDemandRequest {
		bool WasRequested { get; }
		bool WasSuccessfullyLoaded { get; }
		bool IsTarget (Texture texture);
		void Release ();
	}

	/// <summary>Interface for on-demand texture loaders which support deferred cleanup of unused texture requests.</summary>
	public interface IDeferredCleanupOnDemandTextureLoader {
		UnityEngine.Object UnityObject { get; }
		bool HasUnreleasedRequests { get; }
		void UnloadAllTextures ();
		void RunCleanupIteration ();
	}

	/// <summary>
	/// Base class to derive your own OnDemandTextureLoader subclasses from which already provides
	/// the general loading and unloading framework.
	/// For reference, see the <see cref="AddressablesTextureLoader"/> class available
	/// in the com.esotericsoftware.spine.addressables UPM package.
	/// </summary>
	/// <typeparam name="TargetReference">The implementation struct which holds an on-demand loading reference
	/// to the target texture to be loaded, derived from ITargetTextureReference.</typeparam>
	/// <typeparam name="TextureRequest">The implementation struct covering a single texture loading request,
	/// derived from IOnDemandRequest</typeparam>
	[System.Serializable]
	public abstract class GenericOnDemandTextureLoader<TargetReference, TextureRequest> : OnDemandTextureLoader, IDeferredCleanupOnDemandTextureLoader
		where TargetReference : ITargetTextureReference
		where TextureRequest : IOnDemandRequest {

		[System.Serializable]
		public struct PlaceholderTextureMapping {
			public Texture placeholderTexture;
			public TargetReference targetTextureReference;
		}

		/// <summary>
		/// Unfortunately serialization of jagged arrays PlaceholderTextureMapping[][] is not supported,
		/// so we need to use this class with a 1D-array PlaceholderMaterialMapping[] as a workaround.
		/// </summary>
		[System.Serializable]
		public struct PlaceholderMaterialMapping {

			public PlaceholderTextureMapping[] textures;
		}

		// Note: not System.Serializabe on purpose. Would be unnecessary and causes problems otherwise.
		public struct MaterialOnDemandData {
			public int lastFrameRequested;
			public TextureRequest[] textureRequests;
			public List<Material>[] materialsUsingTexture;
		}

		struct ShaderMainTextureProperty {
			public Shader shader;
			public int propertyID;
		}

		static readonly int mainTexPropertyID = Shader.PropertyToID("_MainTex");
		/// <summary>Cache of main texture property IDs per shader, see <see cref="GetMainTexturePropertyID"/>.</summary>
		static readonly List<ShaderMainTextureProperty> mainTexturePropertyByShader = new List<ShaderMainTextureProperty>();

		void Reset () {
			Clear(clearAtlasAsset: true);
		}

		/// <summary>Unload all textures when the ScriptableObject is unloaded or destroyed.</summary>
		protected virtual void OnDisable () {
			OnDemandTextureLoaderCleanup.Unregister(this);
			UnloadAllTextures();
			lastCleanupFrame = -1;
		}

		UnityEngine.Object IDeferredCleanupOnDemandTextureLoader.UnityObject {
			get { return this; }
		}

		void IDeferredCleanupOnDemandTextureLoader.RunCleanupIteration () {
			BeginCustomTextureLoading();
			EndCustomTextureLoading();
		}

		public override void Clear (bool clearAtlasAsset = false) {
			OnDemandTextureLoaderCleanup.Unregister(this);
			UnloadAllTextures();
			if (clearAtlasAsset) atlasAsset = null;
			placeholderMap = null;
			loadedDataAtMaterial = null;
			additionalTexturePropertyIDs = null;
			lastCleanupFrame = -1;
		}

		public override string GetPlaceholderTextureName (string originalTextureName) {
			return originalTextureName + "_low";
		}

		/// <summary>Get the property ID of the material's main texture: either the property marked with the
		/// <c>[MainTexture]</c> attribute if any, or <c>_MainTex</c> otherwise.
		/// The result is cached per shader.</summary>
		/// <returns>The main texture property ID, or -1 if the shader has no main texture property.</returns>
		public static int GetMainTexturePropertyID (Material material) {
			if (!material) return -1;
			Shader shader = material.shader;
			if (!shader) return -1;

			for (int i = 0, count = mainTexturePropertyByShader.Count; i < count; ++i) {
				if (mainTexturePropertyByShader[i].shader == shader)
					return mainTexturePropertyByShader[i].propertyID;
			}

			int propertyID = -1;
#if UNITY_2019_3_OR_NEWER
			for (int propertyIndex = 0, count = shader.GetPropertyCount(); propertyIndex < count; ++propertyIndex) {
				int id = shader.GetPropertyNameId(propertyIndex);
				if ((shader.GetPropertyFlags(propertyIndex) & ShaderPropertyFlags.MainTexture) != 0) {
					propertyID = id;
					break;
				}
				if (id == mainTexPropertyID)
					propertyID = id; // used unless [MainTexture] property follows.
			}
#else
			if (material.HasProperty(mainTexPropertyID))
				propertyID = mainTexPropertyID;
#endif
			mainTexturePropertyByShader.Add(new ShaderMainTextureProperty { shader = shader, propertyID = propertyID });
			return propertyID;
		}

#if UNITY_EDITOR
		void OnValidate () {
			additionalTexturePropertyIDs = null; // re-created from additionalTextureProperties on next use.
		}
#endif

		/// <summary>Number of on-demand loaded textures per material.
		/// The main texture plus <see cref="additionalTextureProperties"/>.</summary>
		public int OnDemandTextureCount {
			get {
				EnsureTexturePropertyIDsSetup();
				return 1 + additionalTexturePropertyIDs.Length;
			}
		}

		void EnsureTexturePropertyIDsSetup () {
			if (additionalTexturePropertyIDs != null)
				return;

			int propertyCount = additionalTextureProperties != null ? additionalTextureProperties.Length : 0;
			additionalTexturePropertyIDs = new int[propertyCount];
			for (int i = 0; i < propertyCount; ++i) {
				additionalTexturePropertyIDs[i] = IsValidAdditionalTextureProperty(additionalTextureProperties, i) ?
					Shader.PropertyToID(additionalTextureProperties[i]) : -1;
			}
		}

		/// <summary>Returns whether the additional texture property at the given index is a valid entry.
		/// Entries which are empty or duplicates of an earlier entry are ignored.</summary>
		public static bool IsValidAdditionalTextureProperty (string[] additionalTextureProperties, int propertyIndex) {
			string propertyName = additionalTextureProperties[propertyIndex];
			if (string.IsNullOrEmpty(propertyName)) return false;
			return Array.IndexOf(additionalTextureProperties, propertyName, 0, propertyIndex) < 0;
		}

		/// <summary>Returns the texture assigned at the material's texture property with the given texture index.
		/// See <see cref="additionalTextureProperties"/> for the texture index mapping.</summary>
		/// <returns>The assigned texture, or null if the material has no such property or no texture assigned.</returns>
		protected Texture GetTexture (Material material, int textureIndex) {
			if (!material) return null;
			if (textureIndex == 0) {
				int mainTexturePropertyID = GetMainTexturePropertyID(material);
				if (mainTexturePropertyID < 0) return null;
				return material.GetTexture(mainTexturePropertyID);
			}

			EnsureTexturePropertyIDsSetup();
			int propertyIndex = textureIndex - 1;
			if (propertyIndex >= additionalTexturePropertyIDs.Length) return null;
			int propertyID = additionalTexturePropertyIDs[propertyIndex];
			if (propertyID < 0 || !material.HasProperty(propertyID)) return null;
			return material.GetTexture(propertyID);
		}

		/// <summary>Assigns the texture at the material's texture property with the given texture index.
		/// See <see cref="additionalTextureProperties"/> for the texture index mapping.</summary>
		protected void SetTexture (Material material, int textureIndex, Texture texture) {
			if (!material) return;
			if (textureIndex == 0) {
				int mainTexturePropertyID = GetMainTexturePropertyID(material);
				if (mainTexturePropertyID >= 0) material.SetTexture(mainTexturePropertyID, texture);
				return;
			}

			EnsureTexturePropertyIDsSetup();
			int propertyIndex = textureIndex - 1;
			if (propertyIndex >= additionalTexturePropertyIDs.Length) return;
			int propertyID = additionalTexturePropertyIDs[propertyIndex];
			if (propertyID >= 0 && material.HasProperty(propertyID)) material.SetTexture(propertyID, texture);
		}

		/// <summary>Searches the placeholder map for the given placeholder texture at the given texture index.</summary>
		/// <returns>The material index of the matching placeholder map entry, or -1 if not found.</returns>
		protected int FindMaterialIndexByPlaceholder (Texture placeholderTexture, int textureIndex) {
			if (placeholderMap == null || placeholderTexture == null) return -1;
			for (int materialIndex = 0, materialCount = placeholderMap.Length; materialIndex < materialCount; ++materialIndex) {
				PlaceholderTextureMapping[] textures = placeholderMap[materialIndex].textures;
				if (textures == null || textureIndex >= textures.Length) continue;
				if (textures[textureIndex].placeholderTexture == placeholderTexture)
					return materialIndex;
			}
			return -1;
		}

		/// <summary>Searches the placeholder map for a main texture placeholder with the given texture name.</summary>
		/// <returns>The material index of the matching placeholder map entry, or -1 if not found.</returns>
		protected int FindMaterialIndexByPlaceholderName (string placeholderTextureName) {
			if (placeholderMap == null) return -1;
			for (int materialIndex = 0, materialCount = placeholderMap.Length; materialIndex < materialCount; ++materialIndex) {
				PlaceholderTextureMapping[] textures = placeholderMap[materialIndex].textures;
				if (textures == null || textures.Length == 0) continue;
				Texture placeholderTexture = textures[0].placeholderTexture;
				if (placeholderTexture != null && placeholderTexture.name == placeholderTextureName)
					return materialIndex;
			}
			return -1;
		}

		/// <summary>Searches the loaded data for an active request at the given texture index which loads
		/// the given target texture.</summary>
		/// <returns>The material index of the matching loaded data entry, or -1 if not found.</returns>
		protected int FindLoadedMaterialIndexByTarget (Texture targetTexture, int textureIndex) {
			if (loadedDataAtMaterial == null || targetTexture == null) return -1;
			for (int materialIndex = 0, materialCount = loadedDataAtMaterial.Length; materialIndex < materialCount; ++materialIndex) {
				TextureRequest[] textureRequests = loadedDataAtMaterial[materialIndex].textureRequests;
				if (textureRequests == null || textureIndex >= textureRequests.Length) continue;
				TextureRequest textureRequest = textureRequests[textureIndex];
				if (textureRequest.WasRequested && textureRequest.IsTarget(targetTexture))
					return materialIndex;
			}
			return -1;
		}

#if UNITY_EDITOR
		/// <summary>Searches the placeholder map for the given target texture at the given texture index.</summary>
		/// <returns>The material index of the matching placeholder map entry, or -1 if not found.</returns>
		protected int FindMaterialIndexByTargetTexture (Texture targetTexture, int textureIndex) {
			if (placeholderMap == null || targetTexture == null) return -1;
			for (int materialIndex = 0, materialCount = placeholderMap.Length; materialIndex < materialCount; ++materialIndex) {
				PlaceholderTextureMapping[] textures = placeholderMap[materialIndex].textures;
				if (textures == null || textureIndex >= textures.Length) continue;
				if (textures[textureIndex].targetTextureReference.EditorTexture == targetTexture)
					return materialIndex;
			}
			return -1;
		}

		public override bool AssignPlaceholderTexture (Material material, out Texture targetTexture) {
			targetTexture = null;
			if (placeholderMap == null) return false;
			return AssignPlaceholderTexture(material, 0, out targetTexture);
		}

		public override bool AssignPlaceholderTextures (Material material, out Texture[] targetTextures) {
			targetTextures = null;
			if (placeholderMap == null) return false;

			int textureCount = OnDemandTextureCount;
			for (int textureIndex = 0; textureIndex < textureCount; ++textureIndex) {
				Texture targetTexture;
				bool assigned;
				if (textureIndex == 0) // main texture via the virtual method, which subclasses might override.
					assigned = AssignPlaceholderTexture(material, out targetTexture);
				else
					assigned = AssignPlaceholderTexture(material, textureIndex, out targetTexture);
				if (!assigned) continue;
				if (targetTextures == null) targetTextures = new Texture[textureCount];
				targetTextures[textureIndex] = targetTexture;
			}
			return targetTextures != null;
		}

		/// <summary>Replaces the material's texture at the given texture index with its placeholder texture
		/// if it is set to a target texture of this loader.</summary>
		/// <param name="targetTexture">The target texture which was replaced on success, null otherwise.</param>
		/// <returns>True if the texture was replaced.</returns>
		bool AssignPlaceholderTexture (Material material, int textureIndex, out Texture targetTexture) {
			targetTexture = null;
			Texture activeTexture = GetTexture(material, textureIndex);
			if (!activeTexture) return false;

			for (int materialIndex = 0; materialIndex < placeholderMap.Length; ++materialIndex) {
				PlaceholderTextureMapping[] textures = placeholderMap[materialIndex].textures;
				if (textures == null || textureIndex >= textures.Length) continue;

				Texture placeholderTexture = textures[textureIndex].placeholderTexture;
				if (!placeholderTexture || textures[textureIndex].targetTextureReference.EditorTexture != activeTexture)
					continue;

				targetTexture = activeTexture;
				SetTexture(material, textureIndex, placeholderTexture);
				return true;
			}
			return false;
		}

		public override void RestoreTargetTextures (Material material, Texture[] targetTextures) {
			if (!material || targetTextures == null) return;
			for (int textureIndex = 0; textureIndex < targetTextures.Length; ++textureIndex) {
				if (targetTextures[textureIndex])
					SetTexture(material, textureIndex, targetTextures[textureIndex]);
			}
		}

		/// <summary>Replaces each placeholder texture assigned at any texture property of the material with the
		/// target texture mapped to it in the placeholder map, regardless of the property name.</summary>
		/// <returns>True if any target texture was assigned.</returns>
		public override bool RestoreTargetTextures (Material material) {
			if (!material || placeholderMap == null) return false;

			bool anyRestored = false;
			string[] texturePropertyNames = GetTexturePropertyNames(material);
			foreach (string propertyName in texturePropertyNames) {
				Texture currentTexture = material.GetTexture(propertyName);
				if (currentTexture == null) continue;

				Texture targetTexture = FindTargetTextureByPlaceholder(currentTexture);
				if (targetTexture == null) continue;
				material.SetTexture(propertyName, targetTexture);
				anyRestored = true;
			}
			return anyRestored;
		}

		/// <summary>Calls <see cref="RestoreTargetTextures(Material)"/> for each AtlasAsset material and blend mode
		/// material. Additionally assigns the target texture at AtlasAsset materials where the main texture is missing.</summary>
		/// <param name="restoredMaterials">A newly created list of the modified materials, null if none was modified.</param>
		/// <returns>True if any material was modified.</returns>
		public override bool RestoreTargetTextures (out List<Material> restoredMaterials) {
			restoredMaterials = null;
			if (!atlasAsset || placeholderMap == null) return false;

			int atlasMaterialCount = Math.Min(atlasAsset.MaterialCount, placeholderMap.Length);
			int materialIndex = 0;
			foreach (Material material in GetInputMaterials()) {
				bool restored = RestoreTargetTextures(material);
				if (materialIndex < atlasMaterialCount && material && GetTexture(material, 0) == null) {
					// AtlasAsset material with missing main texture (e.g. after an interrupted build).
					PlaceholderTextureMapping[] textures = placeholderMap[materialIndex].textures;
					Texture targetTexture = null;
					if (textures != null && textures.Length > 0)
						targetTexture = textures[0].targetTextureReference.EditorTexture;
					if (targetTexture != null) {
						SetTexture(material, 0, targetTexture);
						restored = true;
					}
				}
				++materialIndex;
				if (!restored) continue;
				if (restoredMaterials == null) restoredMaterials = new List<Material>();
				restoredMaterials.Add(material);
			}
			return restoredMaterials != null;
		}

		/// <summary>Searches the placeholder map for the given placeholder texture at any texture index.</summary>
		/// <returns>The target texture of the matching placeholder map entry, or null if not found.</returns>
		Texture FindTargetTextureByPlaceholder (Texture placeholderTexture) {
			for (int materialIndex = 0, materialCount = placeholderMap.Length; materialIndex < materialCount; ++materialIndex) {
				PlaceholderTextureMapping[] textures = placeholderMap[materialIndex].textures;
				if (textures == null) continue;
				for (int textureIndex = 0, textureCount = textures.Length; textureIndex < textureCount; ++textureIndex) {
					if (textures[textureIndex].placeholderTexture == placeholderTexture)
						return textures[textureIndex].targetTextureReference.EditorTexture;
				}
			}
			return null;
		}

		public override bool IsPlaceholderTexture (Texture texture) {
			if (texture == null || placeholderMap == null) return false;
			foreach (PlaceholderMaterialMapping materialMap in placeholderMap) {
				PlaceholderTextureMapping[] textures = materialMap.textures;
				if (textures == null) continue;
				for (int textureIndex = 0; textureIndex < textures.Length; ++textureIndex) {
					if (textures[textureIndex].placeholderTexture == texture) return true;
				}
			}
			return false;
		}

		public override bool IsTargetTexture (Texture texture) {
			if (texture == null || placeholderMap == null) return false;
			for (int materialIndex = 0, materialCount = placeholderMap.Length; materialIndex < materialCount; ++materialIndex) {
				PlaceholderTextureMapping[] textures = placeholderMap[materialIndex].textures;
				if (textures == null) continue;
				for (int textureIndex = 0, textureCount = textures.Length; textureIndex < textureCount; ++textureIndex) {
					if (textures[textureIndex].placeholderTexture != null &&
						textures[textureIndex].targetTextureReference.EditorTexture == texture)
						return true;
				}
			}
			return false;
		}

		/// <summary>Adds the names of all texture properties which have a texture asset assigned at any AtlasAsset
		/// material to <see cref="additionalTextureProperties"/>. Omits the always-included main texture property
		/// and names already listed.
		/// When called from inspector code, call <c>GenericOnDemandTextureLoaderInspector.UpdatePlaceholderTextures</c>
		/// afterwards to regenerate the placeholder map.</summary>
		/// <returns>The number of added texture property names.</returns>
		public int AddAllTextureProperties () {
			if (!atlasAsset) return 0;

			List<string> propertyNames = new List<string>(additionalTextureProperties ?? new string[0]);
			int previousCount = propertyNames.Count;
			foreach (Material material in atlasAsset.Materials) {
				if (!material) continue;
				int mainTexturePropertyID = GetMainTexturePropertyID(material);
				string[] texturePropertyNames = GetTexturePropertyNames(material);
				foreach (string propertyName in texturePropertyNames) {
					if (Shader.PropertyToID(propertyName) == mainTexturePropertyID || propertyNames.Contains(propertyName)) continue;
					if (!IsTextureAsset(material.GetTexture(propertyName))) continue;
					propertyNames.Add(propertyName);
				}
			}
			int addedCount = propertyNames.Count - previousCount;
			if (addedCount > 0) {
				additionalTextureProperties = propertyNames.ToArray();
				additionalTexturePropertyIDs = null;
			}
			return addedCount;
		}

		/// <summary>Returns whether the texture is a <see cref="Texture2D"/> asset of the project imported via a
		/// <c>TextureImporter</c>, as opposed to a built-in, runtime-created or generated texture, which is required
		/// for creating placeholder textures and on-demand loading.</summary>
		public static bool IsTextureAsset (Texture texture) {
			if (!(texture is Texture2D)) return false;
			string assetPath = UnityEditor.AssetDatabase.GetAssetPath(texture);
			if (string.IsNullOrEmpty(assetPath) ||
				assetPath == "Resources/unity_builtin_extra" || assetPath == "Library/unity default resources")
				return false;
			return UnityEditor.AssetImporter.GetAtPath(assetPath) is UnityEditor.TextureImporter;
		}

		public override bool ValidateSetup () {
			return ValidateSetup(true);
		}

		/// <summary>Validates that the placeholder map matches the AtlasAsset materials and the configured texture
		/// properties, e.g. that no textures were changed at the materials after the loader was setup.</summary>
		/// <param name="logWarnings">Whether to log a warning for each problem found.</param>
		/// <returns>True if the setup is valid.</returns>
		public bool ValidateSetup (bool logWarnings) {
			if (!atlasAsset) {
				if (logWarnings) Debug.LogWarning(string.Format("{0}: no AtlasAsset assigned.", name), this);
				return false;
			}
			int materialCount = atlasAsset.MaterialCount;
			if (placeholderMap == null || placeholderMap.Length != materialCount) {
				if (logWarnings) Debug.LogWarning(string.Format("{0}: placeholder map has {1} material entries, but {2} has {3} materials. " +
					"Please hit 'Regenerate' at the loader.", name, placeholderMap != null ? placeholderMap.Length : 0, atlasAsset.name, materialCount), this);
				return false;
			}

			bool valid = true;
			int textureCount = OnDemandTextureCount;
			int materialIndex = 0;
			foreach (Material material in atlasAsset.Materials) {
				PlaceholderTextureMapping[] textures = placeholderMap[materialIndex].textures;
				++materialIndex;
				if (!material) {
					if (logWarnings) Debug.LogWarning(string.Format("{0}: material #{1} of {2} is null.", name, materialIndex, atlasAsset.name), this);
					valid = false;
					continue;
				}
				if (textures == null || textures.Length != textureCount) {
					if (logWarnings) Debug.LogWarning(string.Format("{0}: placeholder map entry of material '{1}' has {2} texture entries, but {3} are expected " +
						"(main texture and {4} additional texture properties). Please hit 'Regenerate' at the loader.",
						name, material.name, textures != null ? textures.Length : 0, textureCount, textureCount - 1), this);
					valid = false;
					continue;
				}
				int mainTexturePropertyID = GetMainTexturePropertyID(material);
				for (int textureIndex = 0; textureIndex < textureCount; ++textureIndex) {
					// the main texture is always covered by texture index 0, also when listed as additional property.
					if (textureIndex > 0 && additionalTexturePropertyIDs[textureIndex - 1] == mainTexturePropertyID) continue;
					Texture currentTexture = GetTexture(material, textureIndex);
					Texture placeholderTexture = textures[textureIndex].placeholderTexture;
					Texture targetTexture = textures[textureIndex].targetTextureReference.EditorTexture;
					string error = null;
					string advice = "Please hit 'Regenerate' at the loader.";
					if (placeholderTexture != null) {
						if (targetTexture == null) {
							error = string.Format("placeholder texture '{0}' for {1} of material '{2}' has no target texture reference",
								placeholderTexture.name, GetTexturePropertyDescription(textureIndex), material.name);
						} else if (currentTexture != null && currentTexture != targetTexture && currentTexture != placeholderTexture) {
							error = string.Format("material '{0}' has texture '{1}' assigned as {2}, but the loader maps target texture '{3}'",
								material.name, currentTexture.name, GetTexturePropertyDescription(textureIndex), targetTexture.name);
						} else if (validateTargetReference != null) {
							string referenceError = validateTargetReference(textures[textureIndex].targetTextureReference);
							if (referenceError != null) {
								error = string.Format("target texture '{0}' for {1} of material '{2}' {3}",
									targetTexture.name, GetTexturePropertyDescription(textureIndex), material.name, referenceError);
								advice = null;
							}
						}
					} else if (currentTexture != null && IsTextureAsset(currentTexture)) {
						error = string.Format("material '{0}' has texture '{1}' assigned as {2}, but the loader has no mapping for it",
							material.name, currentTexture.name, GetTexturePropertyDescription(textureIndex));
					}
					if (error == null) continue;
					valid = false;
					if (logWarnings) Debug.LogWarning(string.Format("{0}: {1}. {2}", name, error, advice), this);
				}
			}
			return valid;
		}

		/// <summary>Editor-only hook for extension packages to validate target texture references in
		/// <see cref="ValidateSetup()"/>, e.g. checking that a texture is marked as addressable.
		/// Returns null if the reference is valid, otherwise the problem description which is logged
		/// as "{loader}: target texture 'X' for {property} of material 'Y' {problem description}.",
		/// e.g. "is not marked as addressable. Mark the texture as addressable".</summary>
		public static Func<TargetReference, string> validateTargetReference;

		/// <returns>Description of the texture property at the given texture index for log messages.</returns>
		string GetTexturePropertyDescription (int textureIndex) {
			if (textureIndex == 0) return "main texture";
			int propertyIndex = textureIndex - 1;
			string propertyName = additionalTextureProperties != null && propertyIndex < additionalTextureProperties.Length ?
				additionalTextureProperties[propertyIndex] : "";
			return "texture property '" + propertyName + "'";
		}
#endif

		public override bool AssignPlaceholderTextures (out IEnumerable<Material> modifiedMaterials) {
			modifiedMaterials = null;
			if (!atlasAsset) return false;

			int normalMaterialCount = atlasAsset.Materials.Count();
			IEnumerable<Material> inputMaterials = GetInputMaterials();
			int materialIndex = 0;
			foreach (Material targetMaterial in inputMaterials) {
				if ((materialIndex < normalMaterialCount) && (materialIndex >= placeholderMap.Length)) {
					Debug.LogError(string.Format("Failed to assign placeholder textures at {0}, material #{1} {2}. " +
						"It seems like the GenericOnDemandTextureLoader asset was not setup accordingly for the AtlasAsset.",
						atlasAsset, materialIndex + 1, targetMaterial), this);
					return false;
				}
				int mapIndex = materialIndex;
#if UNITY_EDITOR
				if (!Application.isPlaying) {
					int foundMapIndex = FindMaterialIndexByTargetTexture(GetTexture(targetMaterial, 0), 0);
					if (foundMapIndex >= 0)
						mapIndex = foundMapIndex;
				}
#endif
				if (mapIndex < normalMaterialCount) {
					PlaceholderTextureMapping[] textures = placeholderMap[mapIndex].textures;
					if (textures == null) continue;

					bool isAtlasMaterial = materialIndex < normalMaterialCount;
					for (int textureIndex = 0, count = textures.Length; textureIndex < count; ++textureIndex) {
						Texture placeholderTexture = textures[textureIndex].placeholderTexture;
						if (placeholderTexture == null) {
							if (textureIndex == 0) {
								Debug.LogWarning(string.Format(
									"Placeholder texture set to null at {0}, for material #{1} {2}. " +
									"It seems like the GenericOnDemandTextureLoader asset was not setup accordingly " +
									"for the AtlasAsset.",
									atlasAsset, materialIndex + 1, targetMaterial), this);
							}
							continue; // additional texture property not used at this material.
						}
						if (textureIndex > 0) {
							Texture activeTexture = GetTexture(targetMaterial, textureIndex);
							// an additional texture cleared at the material is not replaced, it would be restored from the mapping otherwise.
							if (activeTexture == null) continue;
							if (!isAtlasMaterial) {
#if UNITY_EDITOR
								// Blend mode materials: only replace additional textures referencing the target texture.
								if (activeTexture != textures[textureIndex].targetTextureReference.EditorTexture)
									continue;
#else
								continue;
#endif
							}
						}
						SetTexture(targetMaterial, textureIndex, placeholderTexture);
					}
				}
				++materialIndex;
			}
			modifiedMaterials = inputMaterials;
			return true;
		}

		public override bool HasPlaceholderTexturesAssigned (out List<Material> placeholderMaterials) {
			placeholderMaterials = null;
			if (!atlasAsset) return false;

			bool anyPlaceholderAssigned = false;
			IEnumerable<Material> inputMaterials = GetInputMaterials();
			foreach (Material material in inputMaterials) {
				bool hasPlaceholderAssigned = HasPlaceholderAssigned(material);
				if (hasPlaceholderAssigned) {
					anyPlaceholderAssigned = true;
					if (placeholderMaterials == null) placeholderMaterials = new List<Material>();
					placeholderMaterials.Add(material);
				}
			}
			return anyPlaceholderAssigned;
		}

		public override bool AssignTargetTextures (out IEnumerable<Material> modifiedMaterials) {
			modifiedMaterials = null;
			if (!atlasAsset) return false;
			BeginCustomTextureLoading();

			int normalMaterialCount = atlasAsset.Materials.Count();
			IEnumerable<Material> inputMaterials = GetInputMaterials();

			// process normal materials
			int materialIndex = 0;
			foreach (Material targetMaterial in inputMaterials) {
				if (materialIndex > normalMaterialCount - 1) break;

				if (materialIndex >= placeholderMap.Length) {
					Debug.LogError(string.Format("Failed to assign target textures at {0}, material #{1} {2}. " +
						"It seems like the OnDemandTextureLoader asset was not setup accordingly for the AtlasAsset.",
						atlasAsset, materialIndex + 1, targetMaterial), this);
					return false;
				}

				AssignTargetTextures(targetMaterial, materialIndex);
				++materialIndex;
			}
			// process blend mode materials
			if (skeletonDataAsset != null) {
				foreach (ReplacementMaterial replacement in skeletonDataAsset.blendModeMaterials.additiveMaterials) {
					AssignBlendModeTargetTextures(replacement.material, replacement);
				}
				foreach (ReplacementMaterial replacement in skeletonDataAsset.blendModeMaterials.multiplyMaterials) {
					AssignBlendModeTargetTextures(replacement.material, replacement);
				}
				foreach (ReplacementMaterial replacement in skeletonDataAsset.blendModeMaterials.screenMaterials) {
					AssignBlendModeTargetTextures(replacement.material, replacement);
				}
			}
			modifiedMaterials = inputMaterials;
			EndCustomTextureLoading();
			return true;
		}

		protected IEnumerable<Material> GetInputMaterials () {
			int normalMaterialCount = atlasAsset.Materials.Count();
			IEnumerable<Material> inputMaterials = atlasAsset.Materials;
			if (skeletonDataAsset != null) {
				BlendModeMaterials blendModeMaterials = skeletonDataAsset.blendModeMaterials;
				int additiveCount = blendModeMaterials.additiveMaterials.Count;
				int multiplyCount = blendModeMaterials.multiplyMaterials.Count;
				int screenCount = blendModeMaterials.screenMaterials.Count;
				int totalBlendModeMaterialCount = additiveCount + multiplyCount + screenCount;
				if (totalBlendModeMaterialCount > 0) {
					List<Material> materialsList = new List<Material>(normalMaterialCount + totalBlendModeMaterialCount);
					materialsList.AddRange(atlasAsset.Materials);
					materialsList.AddRange(blendModeMaterials.additiveMaterials.Where(r => r.material != null).Select(r => r.material));
					materialsList.AddRange(blendModeMaterials.multiplyMaterials.Where(r => r.material != null).Select(r => r.material));
					materialsList.AddRange(blendModeMaterials.screenMaterials.Where(r => r.material != null).Select(r => r.material));
					inputMaterials = materialsList;
				}
			}
			return inputMaterials;
		}

		public override void BeginCustomTextureLoading () {
			if (placeholderMap == null) return;

			if (loadedDataAtMaterial == null || (loadedDataAtMaterial.Length == 0 && placeholderMap.Length > 0)) {
				loadedDataAtMaterial = new MaterialOnDemandData[placeholderMap.Length];
				for (int i = 0, count = loadedDataAtMaterial.Length; i < count; ++i) {
					loadedDataAtMaterial[i].lastFrameRequested = -1;

					PlaceholderTextureMapping[] textures = placeholderMap[i].textures;
					if (textures == null)
						continue;

					int texturesAtMaterial = textures.Length;
					loadedDataAtMaterial[i].textureRequests = new TextureRequest[texturesAtMaterial];
					loadedDataAtMaterial[i].materialsUsingTexture = new List<Material>[texturesAtMaterial];
				}
			}
		}

		public override void EndCustomTextureLoading () {
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			// Multiple renderers sharing this loader and the cleanup component may all call this in a single frame,
			// so unused textures are only checked once per frame. Explicit UnloadUnusedTextures() calls are unaffected.
			int currentFrame = Time.frameCount;
			if (lastCleanupFrame == currentFrame)
				return;
			lastCleanupFrame = currentFrame;
			UnloadUnusedTextures();
		}

		public override bool HasPlaceholderAssigned (Material material) {
			if (!material || placeholderMap == null) return false;
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				// Recovery must also detect placeholders at properties removed or renamed in the loader settings.
				foreach (string propertyName in GetTexturePropertyNames(material)) {
					if (IsPlaceholderTexture(material.GetTexture(propertyName))) return true;
				}
				return false;
			}
#endif

			int textureCount = OnDemandTextureCount;
			for (int textureIndex = 0; textureIndex < textureCount; ++textureIndex) {
				Texture currentTexture = GetTexture(material, textureIndex);
				if (currentTexture != null && FindMaterialIndexByPlaceholder(currentTexture, textureIndex) >= 0)
					return true;
			}
			return false;
		}

		public override void RequestLoadMaterialTextures (Material material, ref Material overrideMaterial) {
			if (!material || placeholderMap == null) return;

			int textureCount = OnDemandTextureCount;
			for (int textureIndex = 0; textureIndex < textureCount; ++textureIndex) {
				Texture currentTexture = GetTexture(material, textureIndex);
				if (currentTexture == null) continue;

				int foundMaterialIndex = FindMaterialIndexByPlaceholder(currentTexture, textureIndex);
				if (foundMaterialIndex >= 0)
					RequestLoadTexture(material, foundMaterialIndex, textureIndex, null);

				int loadedMaterialIndex = FindLoadedMaterialIndexByTarget(currentTexture, textureIndex);
				if (loadedMaterialIndex >= 0) {
					TrackMaterial(loadedMaterialIndex, textureIndex, material);
					loadedDataAtMaterial[loadedMaterialIndex].lastFrameRequested = Time.frameCount;
				}
			}
		}

		public override void RequestLoadTexture (Texture placeholderTexture, ref Texture replacementTexture,
			System.Action<Texture> onTextureLoaded = null) {

			if (placeholderTexture == null || placeholderMap == null) return;

			int textureCount = OnDemandTextureCount;
			for (int textureIndex = 0; textureIndex < textureCount; ++textureIndex) {
				int foundMaterialIndex = FindMaterialIndexByPlaceholder(placeholderTexture, textureIndex);
				if (foundMaterialIndex >= 0) {
					Material material = atlasAsset.Materials.ElementAt(foundMaterialIndex);
					Texture loadedTexture = RequestLoadTexture(material, foundMaterialIndex, textureIndex, onTextureLoaded);
					if (loadedTexture != null)
						replacementTexture = loadedTexture;
				}

				// Note: placeholderTexture might also be an already loaded target texture.
				int loadedMaterialIndex = FindLoadedMaterialIndexByTarget(placeholderTexture, textureIndex);
				if (loadedMaterialIndex >= 0)
					loadedDataAtMaterial[loadedMaterialIndex].lastFrameRequested = Time.frameCount;

				if (foundMaterialIndex >= 0 || loadedMaterialIndex >= 0)
					return;
			}
		}

		protected void AssignTargetTextures (Material material, int materialIndex) {
			if (materialIndex > placeholderMap.Length - 1) return;
			PlaceholderTextureMapping[] textures = placeholderMap[materialIndex].textures;
			if (textures == null) return;

			for (int textureIndex = 0, textureCount = textures.Length; textureIndex < textureCount; ++textureIndex)
				RequestLoadTexture(material, materialIndex, textureIndex, null);
		}

		protected void AssignBlendModeTargetTextures (Material blendModeMaterial, ReplacementMaterial replacementMaterial) {
			if (!blendModeMaterial) return;

			int mainMaterialIndex;
			Texture mainTexture = GetTexture(blendModeMaterial, 0);
			if (mainTexture != null) {
				mainMaterialIndex = FindMaterialIndexByPlaceholder(mainTexture, 0);
				if (mainMaterialIndex < 0)
					return;
			} else {
				string textureNameFull = Path.GetFileNameWithoutExtension(replacementMaterial.pageName);
				string placeholderTextureName = GetPlaceholderTextureName(textureNameFull);
				mainMaterialIndex = FindMaterialIndexByPlaceholderName(placeholderTextureName);
				if (mainMaterialIndex < 0)
					return;
			}
			RequestLoadTexture(blendModeMaterial, mainMaterialIndex, 0, null);

			// Additional textures: only where the blend mode material references the placeholder texture.
			PlaceholderTextureMapping[] textures = placeholderMap[mainMaterialIndex].textures;
			for (int textureIndex = 1, textureCount = textures != null ? textures.Length : 0; textureIndex < textureCount; ++textureIndex) {
				Texture placeholderTexture = textures[textureIndex].placeholderTexture;
				if (placeholderTexture == null || GetTexture(blendModeMaterial, textureIndex) != placeholderTexture)
					continue;
				RequestLoadTexture(blendModeMaterial, mainMaterialIndex, textureIndex, null);
			}
		}

		protected virtual Texture RequestLoadTexture (Material material, int materialIndex, int textureIndex,
			System.Action<Texture> onTextureLoaded) {

			PlaceholderTextureMapping[] placeholderTextures = placeholderMap[materialIndex].textures;
			if (placeholderTextures == null || textureIndex >= placeholderTextures.Length)
				return null;
			if (textureIndex > 0 && placeholderTextures[textureIndex].placeholderTexture == null)
				return null;

			TargetReference targetReference = placeholderTextures[textureIndex].targetTextureReference;
			loadedDataAtMaterial[materialIndex].lastFrameRequested = Time.frameCount;

#if UNITY_EDITOR
			if (!Application.isPlaying) {
				Texture editorTexture = targetReference.EditorTexture;
				if (editorTexture != null) {
					Texture currentTexture = GetTexture(material, textureIndex);
					// Do not revert textures changed or cleared by the user, replace only placeholders and a missing main texture.
					bool isPlaceholder = currentTexture == placeholderTextures[textureIndex].placeholderTexture;
					if (isPlaceholder || (currentTexture == null && textureIndex == 0))
						SetTexture(material, textureIndex, editorTexture);
					if (onTextureLoaded != null) onTextureLoaded(editorTexture);
				}
				return editorTexture;
			}
#endif
			MaterialOnDemandData materialData = loadedDataAtMaterial[materialIndex];
			if (materialData.textureRequests[textureIndex].WasRequested) {
				TrackMaterial(materialIndex, textureIndex, material);
				Texture loadedTexture = GetAlreadyLoadedTexture(materialIndex, textureIndex);
				if (loadedTexture != null) {
					SetTexture(material, textureIndex, loadedTexture);
					if (onTextureLoaded != null) onTextureLoaded(loadedTexture);
				}
				return loadedTexture;
			}

			TrackMaterial(materialIndex, textureIndex, material);
			try {
				CreateTextureRequest(targetReference, materialData, textureIndex, material, onTextureLoaded);
			} finally {
				if (materialData.textureRequests[textureIndex].WasRequested)
					OnDemandTextureLoaderCleanup.Register(this);
				else
					UntrackMaterial(materialIndex, textureIndex, material);
			}
			return null;
		}

		public abstract Texture GetAlreadyLoadedTexture (int materialIndex, int textureIndex);

		public abstract void CreateTextureRequest (TargetReference targetReference,
			MaterialOnDemandData materialData, int textureIndex, Material materialToUpdate,
			System.Action<Texture> onTextureLoaded);

		protected virtual bool HasRequestFailed (TextureRequest textureRequest) {
			return false;
		}

		public virtual bool HasUnreleasedRequests {
			get {
				if (loadedDataAtMaterial == null) return false;

				for (int materialIndex = 0, materialCount = loadedDataAtMaterial.Length; materialIndex < materialCount; ++materialIndex) {
					TextureRequest[] textureRequests = loadedDataAtMaterial[materialIndex].textureRequests;
					if (textureRequests == null) continue;

					for (int textureIndex = 0, textureCount = textureRequests.Length; textureIndex < textureCount; ++textureIndex) {
						if (textureRequests[textureIndex].WasRequested)
							return true;
					}
				}
				return false;
			}
		}

		public virtual void UnloadAllTextures () {
			if (loadedDataAtMaterial == null) return;

			for (int materialIndex = 0, materialCount = loadedDataAtMaterial.Length; materialIndex < materialCount; ++materialIndex) {
				TextureRequest[] textureRequests = loadedDataAtMaterial[materialIndex].textureRequests;
				if (textureRequests == null) continue;

				for (int textureIndex = 0, textureCount = textureRequests.Length; textureIndex < textureCount; ++textureIndex) {
					if (textureRequests[textureIndex].WasRequested)
						RequestUnloadTexture(materialIndex, textureIndex);
				}
			}
		}

		public virtual void UnloadUnusedTextures () {
			if (loadedDataAtMaterial == null) return;

			int currentFrameCount = Time.frameCount;
			float timePerFrame = Time.smoothDeltaTime;
			float deltaFramesToUnload = unloadAfterSecondsUnused / timePerFrame;

			for (int materialIndex = 0, materialCount = loadedDataAtMaterial.Length; materialIndex < materialCount; ++materialIndex) {
				MaterialOnDemandData materialData = loadedDataAtMaterial[materialIndex];
				RemoveDestroyedMaterials(materialData);
				if (materialData.textureRequests == null) continue;
				int textureCount = materialData.textureRequests.Length;

				for (int textureIndex = 0; textureIndex < textureCount; ++textureIndex) {
					TextureRequest textureRequest = materialData.textureRequests[textureIndex];
					if (!textureRequest.WasRequested) continue;

					bool failed = HasRequestFailed(textureRequest);
					bool unusedLongEnough = currentFrameCount - materialData.lastFrameRequested > deltaFramesToUnload;
					if (failed || unusedLongEnough)
						RequestUnloadTexture(materialIndex, textureIndex);
				}
			}
		}

		public virtual void RequestUnloadTexture (int materialIndex, int textureIndex) {
			if (loadedDataAtMaterial == null || materialIndex >= loadedDataAtMaterial.Length) return;

			MaterialOnDemandData materialData = loadedDataAtMaterial[materialIndex];
			if (materialData.textureRequests == null || textureIndex >= materialData.textureRequests.Length) return;
			TextureRequest textureRequest = materialData.textureRequests[textureIndex];
			bool hasActiveRequest = textureRequest.WasRequested;
			if (hasActiveRequest)
				materialData.textureRequests[textureIndex] = default(TextureRequest);

			List<Material> materialsUsingTexture = materialData.materialsUsingTexture != null &&
				textureIndex < materialData.materialsUsingTexture.Length ?
				materialData.materialsUsingTexture[textureIndex] : null;
			List<Material> restoredMaterials = null;
			try {
				if (placeholderMap == null || materialIndex >= placeholderMap.Length) return;
				PlaceholderTextureMapping[] placeholderTextures = placeholderMap[materialIndex].textures;
				if (placeholderTextures == null || textureIndex >= placeholderTextures.Length) return;

				Texture placeholderTexture = placeholderTextures[textureIndex].placeholderTexture;
				Material targetMaterial = atlasAsset ? atlasAsset.Materials.ElementAtOrDefault(materialIndex) : null;
				Texture targetMaterialTexture = GetTexture(targetMaterial, textureIndex);

				if (materialsUsingTexture != null) {
					foreach (Material material in materialsUsingTexture) {
						RestorePlaceholderIfTargetTexture(material, textureIndex, textureRequest, hasActiveRequest,
							targetMaterialTexture, placeholderTexture, ref restoredMaterials);
					}
				}

				RestorePlaceholderIfTargetTexture(targetMaterial, textureIndex, textureRequest, hasActiveRequest,
					targetMaterialTexture, placeholderTexture, ref restoredMaterials);

				// also reset material textures of blend mode materials
				if ((hasActiveRequest || targetMaterialTexture != null) && skeletonDataAsset != null) {
					BlendModeMaterials blendModeMaterials = skeletonDataAsset.blendModeMaterials;
					foreach (ReplacementMaterial replacementMaterial in blendModeMaterials.additiveMaterials) {
						Material replacement = replacementMaterial != null ? replacementMaterial.material : null;
						RestorePlaceholderIfTargetTexture(replacement, textureIndex, textureRequest, hasActiveRequest,
							targetMaterialTexture, placeholderTexture, ref restoredMaterials);
					}
					foreach (ReplacementMaterial replacementMaterial in blendModeMaterials.multiplyMaterials) {
						Material replacement = replacementMaterial != null ? replacementMaterial.material : null;
						RestorePlaceholderIfTargetTexture(replacement, textureIndex, textureRequest, hasActiveRequest,
							targetMaterialTexture, placeholderTexture, ref restoredMaterials);
					}
					foreach (ReplacementMaterial replacementMaterial in blendModeMaterials.screenMaterials) {
						Material replacement = replacementMaterial != null ? replacementMaterial.material : null;
						RestorePlaceholderIfTargetTexture(replacement, textureIndex, textureRequest, hasActiveRequest,
							targetMaterialTexture, placeholderTexture, ref restoredMaterials);
					}
				}
			} finally {
				if (materialsUsingTexture != null)
					materialsUsingTexture.Clear();
				if (hasActiveRequest)
					textureRequest.Release();
			}

			if (restoredMaterials == null) return;
			foreach (Material restoredMaterial in restoredMaterials)
				OnTextureUnloaded(restoredMaterial, textureIndex);
		}

		void TrackMaterial (int materialIndex, int textureIndex, Material material) {
			if (!material || loadedDataAtMaterial == null ||
				materialIndex < 0 || materialIndex >= loadedDataAtMaterial.Length)
				return;

			MaterialOnDemandData materialData = loadedDataAtMaterial[materialIndex];
			if (materialData.textureRequests == null ||
				textureIndex < 0 || textureIndex >= materialData.textureRequests.Length)
				return;

			if (materialData.materialsUsingTexture == null) {
				materialData.materialsUsingTexture = new List<Material>[materialData.textureRequests.Length];
				loadedDataAtMaterial[materialIndex] = materialData;
			}

			List<Material> materials = materialData.materialsUsingTexture[textureIndex];
			if (materials == null) {
				materials = new List<Material>();
				materialData.materialsUsingTexture[textureIndex] = materials;
			}
			if (!materials.Contains(material))
				materials.Add(material);
		}

		void UntrackMaterial (int materialIndex, int textureIndex, Material material) {
			if (loadedDataAtMaterial == null ||
				materialIndex < 0 || materialIndex >= loadedDataAtMaterial.Length)
				return;

			List<Material>[] materialsUsingTexture = loadedDataAtMaterial[materialIndex].materialsUsingTexture;
			if (materialsUsingTexture == null ||
				textureIndex < 0 || textureIndex >= materialsUsingTexture.Length)
				return;

			List<Material> materials = materialsUsingTexture[textureIndex];
			if (materials != null)
				materials.Remove(material);
		}

		void RemoveDestroyedMaterials (MaterialOnDemandData materialData) {
			List<Material>[] materialsUsingTexture = materialData.materialsUsingTexture;
			if (materialsUsingTexture == null) return;

			for (int textureIndex = 0; textureIndex < materialsUsingTexture.Length; ++textureIndex) {
				List<Material> materials = materialsUsingTexture[textureIndex];
				if (materials == null) continue;

				for (int materialIndex = materials.Count - 1; materialIndex >= 0; --materialIndex) {
					if (!materials[materialIndex])
						materials.RemoveAt(materialIndex);
				}
			}
		}

		void RestorePlaceholderIfTargetTexture (Material material, int textureIndex, TextureRequest textureRequest,
			bool hasActiveRequest, Texture targetMaterialTexture, Texture placeholderTexture,
			ref List<Material> restoredMaterials) {

			if (!material) return;
			Texture currentTexture = GetTexture(material, textureIndex);
			bool isTargetTexture = hasActiveRequest ?
				currentTexture != null && textureRequest.IsTarget(currentTexture) :
				currentTexture == targetMaterialTexture;
			if (!isTargetTexture) return;

			SetTexture(material, textureIndex, placeholderTexture);
			if (hasActiveRequest) {
				if (restoredMaterials == null) restoredMaterials = new List<Material>();
				restoredMaterials.Add(material);
			}
		}

		public int maxPlaceholderSize = 128;
		public float unloadAfterSecondsUnused = 60.0f;

		/// <summary>Names of additional texture properties (e.g. <c>"_BumpMap"</c> for normal maps) which shall be
		/// loaded on demand in addition to the main texture. Texture index 0 of each
		/// <see cref="PlaceholderMaterialMapping.textures"/> entry is always the main texture, texture index
		/// <c>i + 1</c> corresponds to <c>additionalTextureProperties[i]</c>.</summary>
		[Delayed] public string[] additionalTextureProperties = new string[0];
		int[] additionalTexturePropertyIDs;
		int lastCleanupFrame = -1;

		/// <summary>A map from placeholder to on-demand-loaded target textures.
		/// This array holds PlaceholderMaterialMapping for each Material,
		/// where each <c>PlaceholderMaterialMapping.textures</c> contains a Texture-to-TextureReference mapping
		/// for each Texture at the Material.</summary>
		public PlaceholderMaterialMapping[] placeholderMap;

		/// <summary>An array holding loaded data for each Material.</summary>
		protected MaterialOnDemandData[] loadedDataAtMaterial;
	}
}
#endif
