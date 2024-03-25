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

#define SPINE_OPTIONAL_ON_DEMAND_LOADING

#if SPINE_OPTIONAL_ON_DEMAND_LOADING

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spine.Unity {

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
	public abstract class GenericOnDemandTextureLoader<TargetReference, TextureRequest> : OnDemandTextureLoader
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
		}

		void Reset () {
			Clear(clearAtlasAsset: true);
		}

		public override void Clear (bool clearAtlasAsset = false) {
			if (clearAtlasAsset) atlasAsset = null;
			placeholderMap = null;
			loadedDataAtMaterial = null;
		}

		public override string GetPlaceholderTextureName (string originalTextureName) {
			return originalTextureName + "_low";
		}

		public override bool AssignPlaceholderTextures (out IEnumerable<Material> modifiedMaterials) {
			modifiedMaterials = null;
			if (!atlasAsset) return false;

			int materialIndex = 0;
			foreach (Material targetMaterial in atlasAsset.Materials) {
				if (materialIndex >= placeholderMap.Length) {
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
				Texture placeholderTexture = placeholderMap[mapIndex].textures[textureIndex].placeholderTexture;
				if (placeholderTexture == null) {
					Debug.LogWarning(string.Format("Placeholder texture set to null at {0}, for material #{1} {2}. " +
						"It seems like the GenericOnDemandTextureLoader asset was not setup accordingly for the AtlasAsset.",
						atlasAsset, materialIndex + 1, targetMaterial), this);
				} else {
					targetMaterial.mainTexture = placeholderTexture;
				}
				++materialIndex;
			}
			modifiedMaterials = atlasAsset.Materials;
			return true;
		}

		public override bool HasPlaceholderTexturesAssigned (out List<Material> placeholderMaterials) {
			placeholderMaterials = null;
			if (!atlasAsset) return false;

			bool anyPlaceholderAssigned = false;

			int materialIndex = 0;
			foreach (Material material in atlasAsset.Materials) {
				if (materialIndex >= placeholderMap.Length)
					return false;
				bool hasPlaceholderAssigned = HasPlaceholderAssigned(material);
				if (hasPlaceholderAssigned) {
					anyPlaceholderAssigned = true;
					if (placeholderMaterials == null) placeholderMaterials = new List<Material>();
					placeholderMaterials.Add(material);
				}
				materialIndex++;
			}
			return anyPlaceholderAssigned;
		}

		public override bool AssignTargetTextures (out IEnumerable<Material> modifiedMaterials) {
			modifiedMaterials = null;
			if (!atlasAsset) return false;
			BeginCustomTextureLoading();
			int i = 0;
			foreach (Material targetMaterial in atlasAsset.Materials) {
				if (i >= placeholderMap.Length) {
					Debug.LogError(string.Format("Failed to assign target textures at {0}, material #{1} {2}. " +
						"It seems like the OnDemandTextureLoader asset was not setup accordingly for the AtlasAsset.",
						atlasAsset, i + 1, targetMaterial), this);
					return false;
				}
				AssignTargetTextures(targetMaterial, i);
				++i;
			}
			modifiedMaterials = atlasAsset.Materials;
			EndCustomTextureLoading();
			return true;
		}

		public override void BeginCustomTextureLoading () {
			if (loadedDataAtMaterial == null || (loadedDataAtMaterial.Length == 0 && placeholderMap.Length > 0)) {
				loadedDataAtMaterial = new MaterialOnDemandData[placeholderMap.Length];
				for (int i = 0, count = loadedDataAtMaterial.Length; i < count; ++i) {
					loadedDataAtMaterial[i].lastFrameRequested = -1;

					PlaceholderTextureMapping[] textures = placeholderMap[i].textures;
					if (textures == null)
						continue;

					int texturesAtMaterial = textures.Length;
					loadedDataAtMaterial[i].textureRequests = new TextureRequest[texturesAtMaterial];
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
			Texture currentTexture = material.mainTexture;
			int textureIndex = 0; // Todo: currently only main texture is supported.
			int foundMaterialIndex = Array.FindIndex(placeholderMap, entry => entry.textures[textureIndex].placeholderTexture == currentTexture);
			return foundMaterialIndex >= 0;
		}

		public override void RequestLoadMaterialTextures (Material material, ref Material overrideMaterial) {
			if (!material || !material.mainTexture) return;

			Texture currentTexture = material.mainTexture;
			int textureIndex = 0; // Todo: currently only main texture is supported.

			int foundMaterialIndex = Array.FindIndex(placeholderMap, entry => entry.textures[textureIndex].placeholderTexture == currentTexture);
			if (foundMaterialIndex >= 0)
				RequestLoadTexture(material, foundMaterialIndex, textureIndex, null);

			int loadedMaterialIndex = Array.FindIndex(loadedDataAtMaterial, entry =>
				entry.textureRequests[textureIndex].WasRequested &&
				entry.textureRequests[textureIndex].IsTarget(currentTexture));
			if (loadedMaterialIndex >= 0)
				loadedDataAtMaterial[loadedMaterialIndex].lastFrameRequested = Time.frameCount;
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
			RequestLoadTexture(material, materialIndex, textureIndex, null);
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
				Texture loadedTexture = GetAlreadyLoadedTexture(materialIndex, textureIndex);
				if (loadedTexture != null) {
					material.mainTexture = loadedTexture;
					if (onTextureLoaded != null) onTextureLoaded(loadedTexture);
				}
				return loadedTexture;
			}

			CreateTextureRequest(targetReference, materialData, textureIndex, material, onTextureLoaded);
			return null;
		}

		public abstract Texture GetAlreadyLoadedTexture (int materialIndex, int textureIndex);

		public abstract void CreateTextureRequest (TargetReference targetReference,
			MaterialOnDemandData materialData, int textureIndex, Material materialToUpdate,
			System.Action<Texture> onTextureLoaded);

		public virtual void UnloadUnusedTextures () {
			int currentFrameCount = Time.frameCount;
			float timePerFrame = Time.smoothDeltaTime;
			float deltaFramesToUnload = unloadAfterSecondsUnused / timePerFrame;

			for (int materialIndex = 0, materialCount = loadedDataAtMaterial.Length; materialIndex < materialCount; ++materialIndex) {
				MaterialOnDemandData materialData = loadedDataAtMaterial[materialIndex];
				int textureCount = materialData.textureRequests.Length;

				for (int textureIndex = 0; textureIndex < textureCount; ++textureIndex) {
					TextureRequest textureRequest = materialData.textureRequests[textureIndex];
					if (textureRequest.WasSuccessfullyLoaded &&
						currentFrameCount - materialData.lastFrameRequested > deltaFramesToUnload) {
						RequestUnloadTexture(materialIndex, textureIndex);
					}
				}
			}
		}

		public virtual void RequestUnloadTexture (int materialIndex, int textureIndex) {
			if (materialIndex >= loadedDataAtMaterial.Length) return;

			bool wasReleased = false;
			PlaceholderTextureMapping[] placeholderTextures = placeholderMap[materialIndex].textures;
			MaterialOnDemandData materialData = loadedDataAtMaterial[materialIndex];
			if (materialData.textureRequests[textureIndex].WasRequested) {
				materialData.textureRequests[textureIndex].Release();
				wasReleased = true;
			}

			// reset material textures to placeholder textures.
			Material targetMaterial = atlasAsset.Materials.ElementAt(materialIndex);
			if (targetMaterial) {
				targetMaterial.mainTexture = placeholderTextures[textureIndex].placeholderTexture;
				if (wasReleased)
					OnTextureUnloaded(targetMaterial, textureIndex);
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
