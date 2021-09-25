/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

#define SPINE_OPTIONAL_MATERIALOVERRIDE

// Contributed by: Lost Polygon

using Spine.Unity.Examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	// This script is not intended for use with code. See the readme.txt file in SkeletonRendererCustomMaterials folder to learn more.
	[CustomEditor(typeof(SkeletonRendererCustomMaterials))]
	public class SkeletonRendererCustomMaterialsInspector : UnityEditor.Editor {
		List<SkeletonRendererCustomMaterials.AtlasMaterialOverride> componentCustomMaterialOverrides, _customMaterialOverridesPrev;
		List<SkeletonRendererCustomMaterials.SlotMaterialOverride> componentCustomSlotMaterials, _customSlotMaterialsPrev;
		SkeletonRendererCustomMaterials component;

		const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
		MethodInfo RemoveCustomMaterialOverrides, RemoveCustomSlotMaterials, SetCustomMaterialOverrides, SetCustomSlotMaterials;

		#region SkeletonRenderer context menu
		[MenuItem("CONTEXT/SkeletonRenderer/Add Basic Serialized Custom Materials")]
		static void AddSkeletonRendererCustomMaterials (MenuCommand menuCommand) {
			var skeletonRenderer = (SkeletonRenderer)menuCommand.context;
			var newComponent = skeletonRenderer.gameObject.AddComponent<SkeletonRendererCustomMaterials>();
			Undo.RegisterCreatedObjectUndo(newComponent, "Add Basic Serialized Custom Materials");
		}

		[MenuItem("CONTEXT/SkeletonRenderer/Add Basic Serialized Custom Materials", true)]
		static bool AddSkeletonRendererCustomMaterials_Validate (MenuCommand menuCommand) {
			var skeletonRenderer = (SkeletonRenderer)menuCommand.context;
			return (skeletonRenderer.GetComponent<SkeletonRendererCustomMaterials>() == null);
		}
		#endregion

		void OnEnable () {
			Type cm = typeof(SkeletonRendererCustomMaterials);
			RemoveCustomMaterialOverrides = cm.GetMethod("RemoveCustomMaterialOverrides", PrivateInstance);
			RemoveCustomSlotMaterials = cm.GetMethod("RemoveCustomSlotMaterials", PrivateInstance);
			SetCustomMaterialOverrides = cm.GetMethod("SetCustomMaterialOverrides", PrivateInstance);
			SetCustomSlotMaterials = cm.GetMethod("SetCustomSlotMaterials", PrivateInstance);
		}

		public override void OnInspectorGUI () {
			component = (SkeletonRendererCustomMaterials)target;
			var skeletonRenderer = component.skeletonRenderer;

			// Draw the default inspector
			DrawDefaultInspector();

			if (serializedObject.isEditingMultipleObjects)
				return;

			if (componentCustomMaterialOverrides == null) {
				Type cm = typeof(SkeletonRendererCustomMaterials);
				componentCustomMaterialOverrides = cm.GetField("customMaterialOverrides", PrivateInstance).GetValue(component) as List<SkeletonRendererCustomMaterials.AtlasMaterialOverride>;
				componentCustomSlotMaterials = cm.GetField("customSlotMaterials", PrivateInstance).GetValue(component) as List<SkeletonRendererCustomMaterials.SlotMaterialOverride>;
				if (componentCustomMaterialOverrides == null) {
					Debug.Log("Reflection failed.");
					return;
				}
			}

			// Fill with current values at start
			if (_customMaterialOverridesPrev == null || _customSlotMaterialsPrev == null) {
				_customMaterialOverridesPrev = CopyList(componentCustomMaterialOverrides);
				_customSlotMaterialsPrev = CopyList(componentCustomSlotMaterials);
			}

			// Compare new values with saved. If change is detected:
			// store new values, restore old values, remove overrides, restore new values, restore overrides.

			// 1. Store new values
			var customMaterialOverridesNew = CopyList(componentCustomMaterialOverrides);
			var customSlotMaterialsNew = CopyList(componentCustomSlotMaterials);

			// Detect changes
			if (!_customMaterialOverridesPrev.SequenceEqual(customMaterialOverridesNew) ||
				!_customSlotMaterialsPrev.SequenceEqual(customSlotMaterialsNew)) {
				// 2. Restore old values
				componentCustomMaterialOverrides.Clear();
				componentCustomSlotMaterials.Clear();
				componentCustomMaterialOverrides.AddRange(_customMaterialOverridesPrev);
				componentCustomSlotMaterials.AddRange(_customSlotMaterialsPrev);

				// 3. Remove overrides
				RemoveCustomMaterials();

				// 4. Restore new values
				componentCustomMaterialOverrides.Clear();
				componentCustomSlotMaterials.Clear();
				componentCustomMaterialOverrides.AddRange(customMaterialOverridesNew);
				componentCustomSlotMaterials.AddRange(customSlotMaterialsNew);

				// 5. Restore overrides
				SetCustomMaterials();

				if (skeletonRenderer != null)
					skeletonRenderer.LateUpdate();
			}

			_customMaterialOverridesPrev = CopyList(componentCustomMaterialOverrides);
			_customSlotMaterialsPrev = CopyList(componentCustomSlotMaterials);

			if (SpineInspectorUtility.LargeCenteredButton(SpineInspectorUtility.TempContent("Clear and Reapply Changes", tooltip: "Removes all non-serialized overrides in the SkeletonRenderer and reapplies the overrides on this component."))) {
				if (skeletonRenderer != null) {
#if SPINE_OPTIONAL_MATERIALOVERRIDE
					skeletonRenderer.CustomMaterialOverride.Clear();
#endif
					skeletonRenderer.CustomSlotMaterials.Clear();
					RemoveCustomMaterials();
					SetCustomMaterials();
					skeletonRenderer.LateUpdate();
				}
			}
		}

		void RemoveCustomMaterials () {
			RemoveCustomMaterialOverrides.Invoke(component, null);
			RemoveCustomSlotMaterials.Invoke(component, null);
		}

		void SetCustomMaterials () {
			SetCustomMaterialOverrides.Invoke(component, null);
			SetCustomSlotMaterials.Invoke(component, null);
		}

		static List<T> CopyList<T> (List<T> list) {
			return list.GetRange(0, list.Count);
		}
	}
}
