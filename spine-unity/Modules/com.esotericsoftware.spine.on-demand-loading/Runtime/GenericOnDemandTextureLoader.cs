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

#if UNITY_2021_1_OR_NEWER
		static readonly Dictionary<Shader, bool> hasMainTexturePropertyByShader = new Dictionary<Shader, bool>();
#endif

		void Reset () {
			Clear(clearAtlasAsset: true);
		}

		/// <summary>Unload all textures when the ScriptableObject is unloaded or destroyed.</summary>
		protected virtual void OnDisable () {
			OnDemandTextureLoaderCleanup.Unregister(this);
			UnloadAllTextures();
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
		}

		public override string GetPlaceholderTextureName (string originalTextureName) {
			return originalTextureName + "_low";
		}

		static bool HasMainTextureProperty (Material material) {
			if (!material || !material.shader) return false;
			if (material.HasProperty("_MainTex")) return true;

#if UNITY_2021_1_OR_NEWER
			Shader shader = material.shader;
			bool hasMainTextureProperty;
			if (hasMainTexturePropertyByShader.TryGetValue(shader, out hasMainTextureProperty))
				return hasMainTextureProperty;

			for (int propertyIndex = 0, propertyCount = shader.GetPropertyCount(); propertyIndex < propertyCount; ++propertyIndex) {
				if ((shader.GetPropertyFlags(propertyIndex) & UnityEngine.Rendering.ShaderPropertyFlags.MainTexture) != 0) {
					hasMainTextureProperty = true;
					break;
				}
			}
			hasMainTexturePropertyByShader.Add(shader, hasMainTextureProperty);
			return hasMainTextureProperty;
#else
			return false;
#endif
		}

#if UNITY_EDITOR
		public override bool AssignPlaceholderTexture (Material material, out Texture targetTexture) {
			targetTexture = null;
			if (!HasMainTextureProperty(material) || !material.mainTexture || placeholderMap == null) return false;

			Texture activeTexture = material.mainTexture;
			int textureIndex = 0; // Todo: currently only main texture is supported.
			for (int materialIndex = 0; materialIndex < placeholderMap.Length; ++materialIndex) {
				PlaceholderTextureMapping[] textures = placeholderMap[materialIndex].textures;
				if (textures == null || textureIndex >= textures.Length ||
					textures[textureIndex].targetTextureReference.EditorTexture != activeTexture)
					continue;

				Texture placeholderTexture = textures[textureIndex].placeholderTexture;
				if (!placeholderTexture) return false;
				targetTexture = activeTexture;
				material.mainTexture = placeholderTexture;
				return true;
			}
			return false;
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
				Texture activeTexture = targetMaterial.mainTexture;
				int textureIndex = 0; // Todo: currently only main texture is supported.

				int mapIndex = materialIndex;
#if UNITY_EDITOR
				if (!Application.isPlaying) {
					int foundMapIndex = Array.FindIndex(placeholderMap,
						entry => entry.textures[textureIndex].targetTextureReference.EditorTexture == activeTexture);
					if (foundMapIndex >= 0)
						mapIndex = foundMapIndex;
				}
#endif
				if (mapIndex < normalMaterialCount) {
					Texture placeholderTexture = placeholderMap[mapIndex].textures[textureIndex].placeholderTexture;
					if (placeholderTexture == null) {
						Debug.LogWarning(string.Format("Placeholder texture set to null at {0}, for material #{1} {2}. " +
							"It seems like the GenericOnDemandTextureLoader asset was not setup accordingly for the AtlasAsset.",
							atlasAsset, materialIndex + 1, targetMaterial), this);
					} else {
						targetMaterial.mainTexture = placeholderTexture;
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
			UnloadUnusedTextures();
		}

		public override bool HasPlaceholderAssigned (Material material) {
			if (!HasMainTextureProperty(material)) return false;
			Texture currentTexture = material.mainTexture;
			int textureIndex = 0; // Todo: currently only main texture is supported.
			int foundMaterialIndex = Array.FindIndex(placeholderMap, entry => entry.textures[textureIndex].placeholderTexture == currentTexture);
			return foundMaterialIndex >= 0;
		}

		public override void RequestLoadMaterialTextures (Material material, ref Material overrideMaterial) {
			if (!HasMainTextureProperty(material) || !material.mainTexture) return;

			Texture currentTexture = material.mainTexture;
			int textureIndex = 0; // Todo: currently only main texture is supported.

			int foundMaterialIndex = Array.FindIndex(placeholderMap, entry => entry.textures[textureIndex].placeholderTexture == currentTexture);
			if (foundMaterialIndex >= 0)
				RequestLoadTexture(material, foundMaterialIndex, textureIndex, null);

			int loadedMaterialIndex = Array.FindIndex(loadedDataAtMaterial, entry =>
				entry.textureRequests[textureIndex].WasRequested &&
				entry.textureRequests[textureIndex].IsTarget(currentTexture));
			if (loadedMaterialIndex >= 0) {
				TrackMaterial(loadedMaterialIndex, textureIndex, material);
				loadedDataAtMaterial[loadedMaterialIndex].lastFrameRequested = Time.frameCount;
			}
		}

		public override void RequestLoadTexture (Texture placeholderTexture, ref Texture replacementTexture,
			System.Action<Texture> onTextureLoaded = null) {

			if (placeholderTexture == null) return;

			Texture currentTexture = placeholderTexture;
			int textureIndex = 0; // Todo: currently only main texture is supported.

			int foundMaterialIndex = Array.FindIndex(placeholderMap, entry => entry.textures[textureIndex].placeholderTexture == currentTexture);
			if (foundMaterialIndex >= 0) {
				Material material = atlasAsset.Materials.ElementAt(foundMaterialIndex);
				Texture loadedTexture = RequestLoadTexture(material, foundMaterialIndex, textureIndex, onTextureLoaded);
				if (loadedTexture != null)
					replacementTexture = loadedTexture;
			}

			int loadedMaterialIndex = Array.FindIndex(loadedDataAtMaterial, entry =>
				entry.textureRequests[textureIndex].WasRequested &&
				entry.textureRequests[textureIndex].IsTarget(placeholderTexture));
			if (loadedMaterialIndex >= 0)
				loadedDataAtMaterial[loadedMaterialIndex].lastFrameRequested = Time.frameCount;
		}

		protected void AssignTargetTextures (Material material, int materialIndex) {
			int textureIndex = 0; // Todo: currently only main texture is supported.
			if (materialIndex > placeholderMap.Length - 1) return;
			RequestLoadTexture(material, materialIndex, textureIndex, null);
		}

		protected void AssignBlendModeTargetTextures (Material blendModeMaterial, ReplacementMaterial replacementMaterial) {
			int textureIndex = 0; // Todo: currently only main texture is supported.
			int mainMaterialIndex = 0;
			if (blendModeMaterial.mainTexture != null) {
				mainMaterialIndex = Array.FindIndex(placeholderMap,
					entry => entry.textures[textureIndex].placeholderTexture == blendModeMaterial.mainTexture);
				if (mainMaterialIndex < 0)
					return;
			} else {
				string textureNameFull = Path.GetFileNameWithoutExtension(replacementMaterial.pageName);
				string placeholderTextureName = GetPlaceholderTextureName(textureNameFull);
				mainMaterialIndex = Array.FindIndex(placeholderMap,
					entry => entry.textures[textureIndex].placeholderTexture.name == placeholderTextureName);
				if (mainMaterialIndex < 0)
					return;
			}
			RequestLoadTexture(blendModeMaterial, mainMaterialIndex, textureIndex, null);
		}

		protected virtual Texture RequestLoadTexture (Material material, int materialIndex, int textureIndex,
			System.Action<Texture> onTextureLoaded) {

			PlaceholderTextureMapping[] placeholderTextures = placeholderMap[materialIndex].textures;
			if (placeholderTextures == null || textureIndex >= placeholderTextures.Length)
				return null;

			TargetReference targetReference = placeholderTextures[textureIndex].targetTextureReference;
			loadedDataAtMaterial[materialIndex].lastFrameRequested = Time.frameCount;

#if UNITY_EDITOR
			if (!Application.isPlaying) {
				if (targetReference.EditorTexture != null) {
					material.mainTexture = targetReference.EditorTexture;
					if (onTextureLoaded != null) onTextureLoaded(targetReference.EditorTexture);
				}
				return targetReference.EditorTexture;
			}
#endif
			MaterialOnDemandData materialData = loadedDataAtMaterial[materialIndex];
			if (materialData.textureRequests[textureIndex].WasRequested) {
				TrackMaterial(materialIndex, textureIndex, material);
				Texture loadedTexture = GetAlreadyLoadedTexture(materialIndex, textureIndex);
				if (loadedTexture != null) {
					material.mainTexture = loadedTexture;
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
				Texture targetMaterialTexture = targetMaterial ? targetMaterial.mainTexture : null;

				if (materialsUsingTexture != null) {
					foreach (Material material in materialsUsingTexture) {
						RestorePlaceholderIfTargetTexture(material, textureRequest, hasActiveRequest,
							targetMaterialTexture, placeholderTexture, ref restoredMaterials);
					}
				}

				RestorePlaceholderIfTargetTexture(targetMaterial, textureRequest, hasActiveRequest,
					targetMaterialTexture, placeholderTexture, ref restoredMaterials);

				// also reset material textures of blend mode materials
				if ((hasActiveRequest || targetMaterialTexture != null) && skeletonDataAsset != null) {
					BlendModeMaterials blendModeMaterials = skeletonDataAsset.blendModeMaterials;
					foreach (ReplacementMaterial replacementMaterial in blendModeMaterials.additiveMaterials) {
						Material replacement = replacementMaterial != null ? replacementMaterial.material : null;
						RestorePlaceholderIfTargetTexture(replacement, textureRequest, hasActiveRequest,
							targetMaterialTexture, placeholderTexture, ref restoredMaterials);
					}
					foreach (ReplacementMaterial replacementMaterial in blendModeMaterials.multiplyMaterials) {
						Material replacement = replacementMaterial != null ? replacementMaterial.material : null;
						RestorePlaceholderIfTargetTexture(replacement, textureRequest, hasActiveRequest,
							targetMaterialTexture, placeholderTexture, ref restoredMaterials);
					}
					foreach (ReplacementMaterial replacementMaterial in blendModeMaterials.screenMaterials) {
						Material replacement = replacementMaterial != null ? replacementMaterial.material : null;
						RestorePlaceholderIfTargetTexture(replacement, textureRequest, hasActiveRequest,
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

		void RestorePlaceholderIfTargetTexture (Material material, TextureRequest textureRequest,
			bool hasActiveRequest, Texture targetMaterialTexture, Texture placeholderTexture,
			ref List<Material> restoredMaterials) {

			if (!material) return;
			Texture currentTexture = material.mainTexture;
			bool isTargetTexture = hasActiveRequest ?
				currentTexture != null && textureRequest.IsTarget(currentTexture) :
				currentTexture == targetMaterialTexture;
			if (!isTargetTexture) return;

			material.mainTexture = placeholderTexture;
			if (hasActiveRequest) {
				if (restoredMaterials == null) restoredMaterials = new List<Material>();
				restoredMaterials.Add(material);
			}
		}

		public int maxPlaceholderSize = 128;
		public float unloadAfterSecondsUnused = 60.0f;

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
