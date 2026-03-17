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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {

	/// <summary>
	/// A set of Material overrides to replace one material with another (e.g. an inside-mask variant).
	/// Used by <see cref="AtlasAssetBase"/> to hold all mask material overrides in a common location
	/// to avoid duplicate identical materials, and thus unnecessary draw calls.
	/// </summary>
	[System.Serializable]
	public class MaterialOverrideSet {
		public string name;

		[SerializeField] protected List<Material> dictionaryKeys = new List<Material>();
		[SerializeField] protected List<Material> dictionaryValues = new List<Material>();

		public MaterialOverrideSet (string name) {
			this.name = name;
		}

		public MaterialOverrideSet (MaterialOverrideSet src) {
			this.name = src.name;
			this.dictionaryKeys = new List<Material>(src.dictionaryKeys);
			this.dictionaryValues = new List<Material>(src.dictionaryValues);
		}

		/// <summary>Adds an override from <c>originalMaterial</c> to an <c>overrideMaterial</c>.
		/// The caller is responsible to ensure that an entry mapping from <c>originalMaterial</c>
		/// is not already present.</summary>
		public void AddOverride (Material originalMaterial, Material overrideMaterial) {
			dictionaryKeys.Add(originalMaterial);
			dictionaryValues.Add(overrideMaterial);
		}

		/// <summary>Removes a previously added override from <c>originalMaterial</c>.</summary>
		public void RemoveOverride (Material originalMaterial) {
			int existingIndex = dictionaryKeys.IndexOf(originalMaterial);
			if (existingIndex >= 0) {
				dictionaryKeys.RemoveAt(existingIndex);
				dictionaryValues.RemoveAt(existingIndex);
			}
		}

		/// <summary>Sets an existing override from <c>originalMaterial</c> to an <c>overrideMaterial</c>
		/// if an entry for <c>originalMaterial</c> already exists. Otherwise it adds the respective entry.
		/// </summary>
		public void SetOverride (Material originalMaterial, Material overrideMaterial) {
			int existingIndex = dictionaryKeys.IndexOf(originalMaterial);
			if (existingIndex < 0) {
				dictionaryKeys.Add(originalMaterial);
				dictionaryValues.Add(overrideMaterial);
			} else {
				dictionaryValues[existingIndex] = overrideMaterial;
			}
		}

		/// <summary>Applies previously set overrides in-place to the given <c>materials</c> array,
		/// replacing the respective reference to an <c>originalMaterial</c> with the corresponding
		/// <c>overrideMaterial</c>.</summary>
		public void ApplyOverrideTo (Material[] materials) {
			if (dictionaryKeys.Count == 0) return;

			for (int i = 0, count = materials.Length; i < count; ++i) {
				int dictIndex = dictionaryKeys.IndexOf(materials[i]);
				if (dictIndex >= 0)
					materials[i] = dictionaryValues[dictIndex];
			}
		}
	}
}
