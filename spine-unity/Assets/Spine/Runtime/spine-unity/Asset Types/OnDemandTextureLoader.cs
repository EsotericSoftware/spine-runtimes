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

using System.Collections.Generic;
using UnityEngine;

#if SPINE_OPTIONAL_ON_DEMAND_LOADING
namespace Spine.Unity {
	public abstract class OnDemandTextureLoader : ScriptableObject {
		public AtlasAssetBase atlasAsset;
		/// <summary>
		/// Additional <see cref="SkeletonDataAsset"/> reference, currently only used to cover blend mode materials
		/// which are not stored at <c>atlasAsset</c>.
		/// </summary>
		public SkeletonDataAsset skeletonDataAsset;

		/// <param name="originalTextureName">Original texture name without extension.</param>
		/// <returns>The placeholder texture's name for a given original target texture name.</returns>
		public abstract string GetPlaceholderTextureName (string originalTextureName);
		/// <summary>
		/// Assigns previously setup placeholder textures at each Material of the associated AtlasAssetBase.</summary>
		/// <returns>True on success, false if the placeholder texture could not be assigned at any of the
		/// AtlasAssetBase's materials.</returns>
		public abstract bool AssignPlaceholderTextures (out IEnumerable<Material> modifiedMaterials);
#if UNITY_EDITOR
		/// <summary>Assigns a placeholder when the material's main texture matches a target texture of this loader.</summary>
		/// <param name="targetTexture">The target texture which was replaced on success, otherwise null.</param>
		public virtual bool AssignPlaceholderTexture (Material material, out Texture targetTexture) {
			targetTexture = null;
			return false;
		}

		/// <summary>Assigns placeholders at every texture property of the material (main texture and any
		/// additional texture properties such as normal maps) whose texture matches a target texture of this loader.
		/// The default implementation calls <see cref="AssignPlaceholderTexture"/> to maintain existing behaviour
		/// of loaders only supporting the main texture, for backwards compatibility.</summary>
		/// <param name="targetTextures">The replaced target textures indexed by texture index (0 is the main texture),
		/// with null entries where nothing was replaced. Null if nothing was replaced at all.
		/// Pass to <see cref="RestoreTargetTextures"/> to undo the replacement.</param>
		/// <returns>True if any placeholder texture was assigned.</returns>
		public virtual bool AssignPlaceholderTextures (Material material, out Texture[] targetTextures) {
			Texture targetTexture;
			bool anyAssigned = AssignPlaceholderTexture(material, out targetTexture);
			targetTextures = anyAssigned ? new Texture[] { targetTexture } : null;
			return anyAssigned;
		}

		/// <summary>Restores target textures previously replaced via
		/// <see cref="AssignPlaceholderTextures(Material, out Texture[])"/> at the material.</summary>
		/// <param name="targetTextures">The target textures indexed by texture index (0 is the main texture),
		/// null entries are skipped.</param>
		public virtual void RestoreTargetTextures (Material material, Texture[] targetTextures) {
			if (!material || targetTextures == null || targetTextures.Length == 0 || !targetTextures[0]) return;
			material.mainTexture = targetTextures[0];
		}

		/// <summary>Replaces each placeholder texture assigned at any texture property of the material with its target
		/// texture. Placeholders are identified by texture instead of by property name. Used to recover materials
		/// after an interrupted build and before placeholder textures are deleted.</summary>
		/// <returns>True if any target texture was assigned.</returns>
		public virtual bool RestoreTargetTextures (Material material) {
			if (!material || !HasPlaceholderAssigned(material)) return false;
			Material overrideMaterial = null;
			BeginCustomTextureLoading();
			try {
				RequestLoadMaterialTextures(material, ref overrideMaterial);
			} finally {
				EndCustomTextureLoading();
			}
			return !HasPlaceholderAssigned(material);
		}

		/// <summary>Calls <see cref="RestoreTargetTextures(Material)"/> for each material of the associated AtlasAssetBase.</summary>
		/// <param name="restoredMaterials">A newly created list of the modified materials, null if none was modified.</param>
		/// <returns>True if any material was modified.</returns>
		public virtual bool RestoreTargetTextures (out List<Material> restoredMaterials) {
			restoredMaterials = null;
			if (!atlasAsset) return false;
			foreach (Material material in atlasAsset.Materials) {
				if (!RestoreTargetTextures(material)) continue;
				if (restoredMaterials == null) restoredMaterials = new List<Material>();
				restoredMaterials.Add(material);
			}
			return restoredMaterials != null;
		}

		/// <summary>Returns whether the texture is a placeholder referenced by this loader, including mappings whose
		/// target reference is missing. Used to protect shared placeholder assets across different loading backends.
		/// Loaders not derived from GenericOnDemandTextureLoader should override this to expose their placeholders.</summary>
		public virtual bool IsPlaceholderTexture (Texture texture) {
			return false;
		}

		/// <summary>Returns whether the texture is a target texture of this loader, which is replaced by a placeholder
		/// texture when building and loaded on demand at runtime.</summary>
		public virtual bool IsTargetTexture (Texture texture) {
			return false;
		}

		/// <summary>Validates the loader setup, logging a warning for each problem found. Called before placeholder
		/// textures are assigned for a build.</summary>
		/// <returns>True if the setup is valid.</returns>
		public virtual bool ValidateSetup () {
			return true;
		}

		/// <summary>Returns the names of all texture properties of the material's shader. Unused texture properties
		/// remaining at the material from a previously assigned shader are not included.</summary>
		public static string[] GetTexturePropertyNames (Material material) {
			if (!material || !material.shader) return new string[0];
			List<string> propertyNames = new List<string>();
#if UNITY_2018_1_OR_NEWER
			// Material.GetTexturePropertyNames also returns unused properties of previously assigned shaders.
			string[] materialPropertyNames = material.GetTexturePropertyNames();
			foreach (string propertyName in materialPropertyNames) {
				if (material.HasProperty(propertyName))
					propertyNames.Add(propertyName);
			}
#else
			// ShaderUtil property access is obsolete on newer Unity versions, only used for the spine-unity core minimum version.
			Shader shader = material.shader;
			for (int i = 0, count = UnityEditor.ShaderUtil.GetPropertyCount(shader); i < count; ++i) {
				if (UnityEditor.ShaderUtil.GetPropertyType(shader, i) == UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv)
					propertyNames.Add(UnityEditor.ShaderUtil.GetPropertyName(shader, i));
			}
#endif
			return propertyNames.ToArray();
		}
#endif
		/// <summary>
		/// Returns whether any placeholder textures are assigned at the Material of the associated AtlasAssetBase.
		/// </summary>
		/// <param name="placeholderMaterials">A newly created list of materials which has a placeholder texture assigned.</param>
		/// <returns>True, if any placeholder texture is assigned at a Material of the associated AtlasAssetBase.</returns>
		public abstract bool HasPlaceholderTexturesAssigned (out List<Material> placeholderMaterials);

		/// <summary>
		/// Returns whether any main texture is null at a Material of the associated AtlasAssetBase.
		/// </summary>
		/// <param name="nullTextureMaterials">A newly created list of materials which has a null main texture assigned.</param>
		/// <returns>True, if any null main texture is assigned at a Material of the associated AtlasAssetBase.</returns>
		public virtual bool HasNullMainTexturesAssigned (out List<Material> nullTextureMaterials) {
			nullTextureMaterials = null;
			if (!atlasAsset) return false;

			bool anyNullTexture = false;
			foreach (Material material in atlasAsset.Materials) {
				if (material.mainTexture == null) {
					anyNullTexture = true;
					if (nullTextureMaterials == null) nullTextureMaterials = new List<Material>();
					nullTextureMaterials.Add(material);
				}
			}
			return anyNullTexture;
		}

		/// <summary>
		/// Assigns previously setup target textures at each Material where placeholder textures are setup.</summary>
		/// <returns>True on success, false if the target texture could not be assigned at any of the
		/// AtlasAssetBase's materials.</returns>
		public abstract bool AssignTargetTextures (out IEnumerable<Material> modifiedMaterials);
		public abstract void BeginCustomTextureLoading ();
		public abstract void EndCustomTextureLoading ();
		public abstract bool HasPlaceholderAssigned (Material material);
		public abstract void RequestLoadMaterialTextures (Material material, ref Material overrideMaterial);
		public abstract void RequestLoadTexture (Texture placeholderTexture, ref Texture replacementTexture,
			System.Action<Texture> onTextureLoaded = null);
		public abstract void Clear (bool clearAtlasAsset = false);

		#region Event delegates
		public delegate void TextureLoadDelegate (OnDemandTextureLoader loader, Material material, int textureIndex);
		protected event TextureLoadDelegate onTextureRequested;
		protected event TextureLoadDelegate onTextureLoaded;
		protected event TextureLoadDelegate onTextureLoadFailed;
		protected event TextureLoadDelegate onTextureUnloaded;

		public event TextureLoadDelegate TextureRequested {
			add { onTextureRequested += value; }
			remove { onTextureRequested -= value; }
		}
		public event TextureLoadDelegate TextureLoaded {
			add { onTextureLoaded += value; }
			remove { onTextureLoaded -= value; }
		}
		public event TextureLoadDelegate TextureLoadFailed {
			add { onTextureLoadFailed += value; }
			remove { onTextureLoadFailed -= value; }
		}
		public event TextureLoadDelegate TextureUnloaded {
			add { onTextureUnloaded += value; }
			remove { onTextureUnloaded -= value; }
		}

		protected void OnTextureRequested (Material material, int textureIndex) {
			if (onTextureRequested != null)
				onTextureRequested(this, material, textureIndex);
		}
		protected void OnTextureLoaded (Material material, int textureIndex) {
			if (onTextureLoaded != null)
				onTextureLoaded(this, material, textureIndex);
		}
		protected void OnTextureLoadFailed (Material material, int textureIndex) {
			if (onTextureLoadFailed != null)
				onTextureLoadFailed(this, material, textureIndex);
		}
		protected void OnTextureUnloaded (Material material, int textureIndex) {
			if (onTextureUnloaded != null)
				onTextureUnloaded(this, material, textureIndex);
		}
		#endregion
	}
}
#endif
