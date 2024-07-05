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
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Spine.Unity {

	[System.Serializable]
	public struct AddressableTextureReference : ITargetTextureReference {
		[SerializeField] public AssetReferenceTexture assetReference;

#if UNITY_EDITOR
		public Texture EditorTexture {
			get {
				return (Texture)assetReference.editorAsset;
			}
		}
#endif
	}

	public struct AddressableRequest : IOnDemandRequest {
		public AsyncOperationHandle<Texture> handle;

		public bool WasRequested {
			get { return handle.IsValid(); }
		}

		public bool WasSuccessfullyLoaded {
			get { return handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded; }
		}

		public bool IsTarget (Texture texture) {
			return handle.Result == texture;
		}

		public void Release () {
			Addressables.Release(handle);
		}
	}

	[System.Serializable]
	public class AddressablesTextureLoader : GenericOnDemandTextureLoader<AddressableTextureReference, AddressableRequest> {
		public override void CreateTextureRequest (AddressableTextureReference targetReference,
			MaterialOnDemandData materialData, int textureIndex, Material materialToUpdate,
			System.Action<Texture> onTextureLoaded) {

			OnTextureRequested(materialToUpdate, textureIndex);
			materialData.textureRequests[textureIndex].handle = targetReference.assetReference.LoadAssetAsync<Texture>();
			materialData.textureRequests[textureIndex].handle.Completed += (obj) => {
				if (obj.Status == AsyncOperationStatus.Succeeded) {
					Texture loadedTexture = (Texture)targetReference.assetReference.Asset;
					materialToUpdate.mainTexture = loadedTexture;
					OnTextureLoaded(materialToUpdate, textureIndex);
					if (onTextureLoaded != null) onTextureLoaded(loadedTexture);
				} else {
					OnTextureLoadFailed(materialToUpdate, textureIndex);
				}
			};
		}

		public override Texture GetAlreadyLoadedTexture (int materialIndex, int textureIndex) {
			AddressableTextureReference targetReference = placeholderMap[materialIndex].textures[textureIndex].targetTextureReference;
			return (Texture)targetReference.assetReference.Asset;
		}
	}
}
#endif
