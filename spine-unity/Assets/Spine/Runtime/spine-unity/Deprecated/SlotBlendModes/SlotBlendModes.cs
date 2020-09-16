/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

using System.Collections.Generic;
using UnityEngine;
using System;

namespace Spine.Unity.Deprecated {

	/// <summary>
	/// Deprecated. The spine-unity 3.7 runtime introduced SkeletonDataModifierAssets BlendModeMaterials which replaced SlotBlendModes. See the
	/// <see href="http://esotericsoftware.com/spine-unity-skeletondatamodifierassets#BlendModeMaterials">SkeletonDataModifierAssets BlendModeMaterials documentation page</see> and
	/// <see href="http://esotericsoftware.com/forum/Slot-blending-not-work-11281">this forum thread</see> for further information.
	/// This class will be removed in the spine-unity 3.9 runtime.
	/// </summary>
	[Obsolete("The spine-unity 3.7 runtime introduced SkeletonDataModifierAssets BlendModeMaterials which replaced SlotBlendModes. Will be removed in spine-unity 3.9.", false)]
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
