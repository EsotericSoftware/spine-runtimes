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

		internal class MaterialWithRefcount {
			public Material materialClone;
			public int refcount = 1;

			public MaterialWithRefcount(Material mat) {
				this.materialClone = mat;
			}
		}
		static Dictionary<MaterialTexturePair, MaterialWithRefcount> materialTable;
		internal static Dictionary<MaterialTexturePair, MaterialWithRefcount> MaterialTable {
			get {
				if (materialTable == null) materialTable = new Dictionary<MaterialTexturePair, MaterialWithRefcount>();
				return materialTable;
			}
		}

		internal struct SlotMaterialTextureTuple {
			public Slot slot;
			public Texture2D texture2D;
			public Material material;

			public SlotMaterialTextureTuple(Slot slot, Material material, Texture2D texture) {
				this.slot = slot;
				this.material = material;
				this.texture2D = texture;
			}
		}
		
		internal static Material GetOrAddMaterialFor(Material materialSource, Texture2D texture) {
			if (materialSource == null || texture == null) return null;

			var mt = SlotBlendModes.MaterialTable;
			MaterialWithRefcount matWithRefcount;
			var key = new MaterialTexturePair {	material = materialSource, texture2D = texture };
			if (!mt.TryGetValue(key, out matWithRefcount)) {
				matWithRefcount = new MaterialWithRefcount(new Material(materialSource));
				var m = matWithRefcount.materialClone;
				m.name = "(Clone)" + texture.name + "-" + materialSource.name;
				m.mainTexture = texture;
				mt[key] = matWithRefcount;
			}
			else {
				matWithRefcount.refcount++;
			}
			return matWithRefcount.materialClone;
		}

		internal static MaterialWithRefcount GetExistingMaterialFor(Material materialSource, Texture2D texture)
		{
			if (materialSource == null || texture == null) return null;

			var mt = SlotBlendModes.MaterialTable;
			MaterialWithRefcount matWithRefcount;
			var key = new MaterialTexturePair { material = materialSource, texture2D = texture };
			if (!mt.TryGetValue(key, out matWithRefcount)) {
				return null;
			}
			return matWithRefcount;
		}

		internal static void RemoveMaterialFromTable(Material materialSource, Texture2D texture) {
			var mt = SlotBlendModes.MaterialTable;
			var key = new MaterialTexturePair { material = materialSource, texture2D = texture };
			mt.Remove(key);
		}
		#endregion

		#region Inspector
		public Material multiplyMaterialSource;
		public Material screenMaterialSource;

		Texture2D texture;
		#endregion

		SlotMaterialTextureTuple[] slotsWithCustomMaterial = new SlotMaterialTextureTuple[0];

		public bool Applied { get; private set; }

		void Start() {
			if (!Applied) Apply();
		}

		void OnDestroy() {
			if (Applied) Remove();
		}

		public void Apply() {
			GetTexture();
			if (texture == null) return;

			var skeletonRenderer = GetComponent<SkeletonRenderer>();
			if (skeletonRenderer == null) return;

			var slotMaterials = skeletonRenderer.CustomSlotMaterials;

			int numSlotsWithCustomMaterial = 0;
			foreach (var s in skeletonRenderer.Skeleton.Slots) {
				switch (s.data.blendMode) {
				case BlendMode.Multiply:
					if (multiplyMaterialSource != null) {
						slotMaterials[s] = GetOrAddMaterialFor(multiplyMaterialSource, texture);
						++numSlotsWithCustomMaterial;
					}
					break;
				case BlendMode.Screen:
					if (screenMaterialSource != null) {
						slotMaterials[s] = GetOrAddMaterialFor(screenMaterialSource, texture);
						++numSlotsWithCustomMaterial;
					}
					break;
				}
			}
			slotsWithCustomMaterial = new SlotMaterialTextureTuple[numSlotsWithCustomMaterial];
			int storedSlotIndex = 0;
			foreach (var s in skeletonRenderer.Skeleton.Slots) {
				switch (s.data.blendMode) {
				case BlendMode.Multiply:
					if (multiplyMaterialSource != null) {
						slotsWithCustomMaterial[storedSlotIndex++] = new SlotMaterialTextureTuple(s, multiplyMaterialSource, texture);
					}
					break;
				case BlendMode.Screen:
					if (screenMaterialSource != null) {
						slotsWithCustomMaterial[storedSlotIndex++] = new SlotMaterialTextureTuple(s, screenMaterialSource, texture);
					}
					break;
				}
			}

			Applied = true;
			skeletonRenderer.LateUpdate();
		}

		public void Remove() {
			GetTexture();
			if (texture == null) return;

			var skeletonRenderer = GetComponent<SkeletonRenderer>();
			if (skeletonRenderer == null) return;

			var slotMaterials = skeletonRenderer.CustomSlotMaterials;

			foreach (var slotWithCustomMat in slotsWithCustomMaterial) {

				Slot s = slotWithCustomMat.slot;
				Material storedMaterialSource = slotWithCustomMat.material;
				Texture2D storedTexture = slotWithCustomMat.texture2D;

				var matWithRefcount = GetExistingMaterialFor(storedMaterialSource, storedTexture);
				if (--matWithRefcount.refcount == 0) {
					RemoveMaterialFromTable(storedMaterialSource, storedTexture);
				}
				// we don't want to remove slotMaterials[s] if it has been changed in the meantime.
				Material m;
				if (slotMaterials.TryGetValue(s, out m)) {
					var existingMat = matWithRefcount == null ? null : matWithRefcount.materialClone;
					if (Material.ReferenceEquals(m, existingMat)) {
						slotMaterials.Remove(s);
					}
				}
			}
			slotsWithCustomMaterial = null;
			
			Applied = false;
			if (skeletonRenderer.valid) skeletonRenderer.LateUpdate();
		}

		public void GetTexture() {
			if (texture == null) {
				var sr = GetComponent<SkeletonRenderer>(); if (sr == null) return;
				var sda = sr.skeletonDataAsset; if (sda == null) return;
				var aa = sda.atlasAssets[0]; if (aa == null) return;
				var am = aa.PrimaryMaterial; if (am == null) return;
				texture = am.mainTexture as Texture2D;
			}
		}

	}
}

