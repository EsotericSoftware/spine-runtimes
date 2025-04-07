/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

namespace Spine.Unity {
	public abstract class AtlasAssetBase : ScriptableObject {
		public abstract Material PrimaryMaterial { get; }
		public abstract IEnumerable<Material> Materials { get; }
		public abstract int MaterialCount { get; }

		public abstract bool IsLoaded { get; }
		public abstract void Clear ();
		public abstract Atlas GetAtlas (bool onlyMetaData = false);

#if SPINE_OPTIONAL_ON_DEMAND_LOADING
		public enum LoadingMode {
			Normal = 0,
			OnDemand
		}
		public virtual LoadingMode TextureLoadingMode {
			get { return textureLoadingMode; }
			set { textureLoadingMode = value; }
		}
		public OnDemandTextureLoader OnDemandTextureLoader {
			get { return onDemandTextureLoader; }
			set { onDemandTextureLoader = value; }
		}

		public virtual void BeginCustomTextureLoading () {
			if (onDemandTextureLoader)
				onDemandTextureLoader.BeginCustomTextureLoading();
		}

		public virtual void EndCustomTextureLoading () {
			if (onDemandTextureLoader)
				onDemandTextureLoader.EndCustomTextureLoading();
		}

		public virtual void RequireTexturesLoaded (Material material, ref Material overrideMaterial) {
			if (onDemandTextureLoader)
				onDemandTextureLoader.RequestLoadMaterialTextures(material, ref overrideMaterial);
		}

		public virtual void RequireTextureLoaded (Texture placeholderTexture, ref Texture replacementTexture, System.Action<Texture> onTextureLoaded) {
			if (onDemandTextureLoader)
				onDemandTextureLoader.RequestLoadTexture(placeholderTexture, ref replacementTexture, onTextureLoaded);
		}

		[SerializeField] protected LoadingMode textureLoadingMode = LoadingMode.Normal;
		[SerializeField] protected OnDemandTextureLoader onDemandTextureLoader = null;
#endif
	}
}
