/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Modules {

	[DisallowMultipleComponent]
	public class SlotBlendModes : MonoBehaviour {

		#region Internal Material Dictionary
		public struct MaterialTexturePair {
			public Texture2D texture2D;
			public Material material;
		}

		static Dictionary<MaterialTexturePair, Material> materialTable;
		internal static Dictionary<MaterialTexturePair, Material> MaterialTable {
			get {
				if (materialTable == null) materialTable = new Dictionary<MaterialTexturePair, Material>();
				return materialTable;
			}
		}

		internal static Material GetMaterialFor (Material materialSource, Texture2D texture) {
			if (materialSource == null || texture == null) return null;

			var mt = SlotBlendModes.MaterialTable;
			Material m;
			var key = new MaterialTexturePair {	material = materialSource, texture2D = texture };
			if (!mt.TryGetValue(key, out m)) {
				m = new Material(materialSource);
				m.name = "(Clone)" + texture.name + "-" + materialSource.name;
				m.mainTexture = texture;
				mt[key] = m;
			}

			return m;
		}
		#endregion

		#region Inspector
		public Material multiplyMaterialSource;
		public Material screenMaterialSource;

		Texture2D texture;
		#endregion

		public bool Applied { get; private set; }

		void Start () {
			if (!Applied) Apply();
		}

		void OnDestroy () {
			if (Applied) Remove();
		}

		public void Apply () {
			GetTexture();
			if (texture == null) return;

			var skeletonRenderer = GetComponent<SkeletonRenderer>();
			if (skeletonRenderer == null) return;

			var slotMaterials = skeletonRenderer.CustomSlotMaterials;

			foreach (var s in skeletonRenderer.Skeleton.Slots) {
				switch (s.data.blendMode) {
				case BlendMode.Multiply:
					if (multiplyMaterialSource != null) slotMaterials[s] = GetMaterialFor(multiplyMaterialSource, texture);
					break;
				case BlendMode.Screen:
					if (screenMaterialSource != null) slotMaterials[s] = GetMaterialFor(screenMaterialSource, texture);
					break;
				}
			}

			Applied = true;
			skeletonRenderer.LateUpdate();
		}

		public void Remove () {
			GetTexture();
			if (texture == null) return;

			var skeletonRenderer = GetComponent<SkeletonRenderer>();
			if (skeletonRenderer == null) return;

			var slotMaterials = skeletonRenderer.CustomSlotMaterials;

			foreach (var s in skeletonRenderer.Skeleton.Slots) {
				Material m = null;

				switch (s.data.blendMode) {
				case BlendMode.Multiply:
					if (slotMaterials.TryGetValue(s, out m) && Material.ReferenceEquals(m, GetMaterialFor(multiplyMaterialSource, texture)))
						slotMaterials.Remove(s);
					break;
				case BlendMode.Screen:
					if (slotMaterials.TryGetValue(s, out m) && Material.ReferenceEquals(m, GetMaterialFor(screenMaterialSource, texture)))
						slotMaterials.Remove(s);
					break;
				}
			}

			Applied = false;
			skeletonRenderer.LateUpdate();
		}

		public void GetTexture () {
			if (texture == null) {
				var sr = GetComponent<SkeletonRenderer>(); if (sr == null) return;
				var sda = sr.skeletonDataAsset; if (sda == null) return;
				var aa = sda.atlasAssets[0]; if (aa == null) return;
				var am = aa.materials[0]; if (am == null) return;
				texture = am.mainTexture as Texture2D;
			}
		}


	}
}

