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

using Spine.Unity.Examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {

	// This script is not intended for use with code. See spine-unity documentation page for additional information.
	[CustomEditor(typeof(SkeletonGraphicCustomMaterials))]
	public class SkeletonGraphicCustomMaterialsInspector : UnityEditor.Editor {
		List<SkeletonGraphicCustomMaterials.AtlasMaterialOverride> componentCustomMaterialOverrides, _customMaterialOverridesPrev;
		List<SkeletonGraphicCustomMaterials.AtlasTextureOverride> componentCustomTextureOverrides, _customTextureOverridesPrev;
		SkeletonGraphicCustomMaterials component;

		const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
		MethodInfo RemoveCustomMaterialOverrides, RemoveCustomTextureOverrides, SetCustomMaterialOverrides, SetCustomTextureOverrides;

		#region SkeletonGraphic context menu
		[MenuItem("CONTEXT/SkeletonGraphic/Add Basic Serialized Custom Materials")]
		static void AddSkeletonGraphicCustomMaterials (MenuCommand menuCommand) {
			SkeletonGraphic skeletonGraphic = (SkeletonGraphic)menuCommand.context;
			SkeletonGraphicCustomMaterials newComponent = skeletonGraphic.gameObject.AddComponent<SkeletonGraphicCustomMaterials>();
			Undo.RegisterCreatedObjectUndo(newComponent, "Add Basic Serialized Custom Materials");
		}

		[MenuItem("CONTEXT/SkeletonGraphic/Add Basic Serialized Custom Materials", true)]
		static bool AddSkeletonGraphicCustomMaterials_Validate (MenuCommand menuCommand) {
			SkeletonGraphic skeletonGraphic = (SkeletonGraphic)menuCommand.context;
			return (skeletonGraphic.GetComponent<SkeletonGraphicCustomMaterials>() == null);
		}
		#endregion

		void OnEnable () {
			Type cm = typeof(SkeletonGraphicCustomMaterials);
			RemoveCustomMaterialOverrides = cm.GetMethod("RemoveCustomMaterialOverrides", PrivateInstance);
			RemoveCustomTextureOverrides = cm.GetMethod("RemoveCustomTextureOverrides", PrivateInstance);
			SetCustomMaterialOverrides = cm.GetMethod("SetCustomMaterialOverrides", PrivateInstance);
			SetCustomTextureOverrides = cm.GetMethod("SetCustomTextureOverrides", PrivateInstance);
		}

		public override void OnInspectorGUI () {
			component = (SkeletonGraphicCustomMaterials)target;
			SkeletonGraphic skeletonGraphic = component.skeletonGraphic;

			// Draw the default inspector
			DrawDefaultInspector();

			if (serializedObject.isEditingMultipleObjects)
				return;

			if (componentCustomMaterialOverrides == null) {
				Type cm = typeof(SkeletonGraphicCustomMaterials);
				componentCustomMaterialOverrides = cm.GetField("customMaterialOverrides", PrivateInstance).GetValue(component) as List<SkeletonGraphicCustomMaterials.AtlasMaterialOverride>;
				componentCustomTextureOverrides = cm.GetField("customTextureOverrides", PrivateInstance).GetValue(component) as List<SkeletonGraphicCustomMaterials.AtlasTextureOverride>;
				if (componentCustomMaterialOverrides == null) {
					Debug.Log("Reflection failed.");
					return;
				}
			}

			// Fill with current values at start
			if (_customMaterialOverridesPrev == null || _customTextureOverridesPrev == null) {
				_customMaterialOverridesPrev = CopyList(componentCustomMaterialOverrides);
				_customTextureOverridesPrev = CopyList(componentCustomTextureOverrides);
			}

			// Compare new values with saved. If change is detected:
			// store new values, restore old values, remove overrides, restore new values, restore overrides.

			// 1. Store new values
			var customMaterialOverridesNew = CopyList(componentCustomMaterialOverrides);
			var customTextureOverridesNew = CopyList(componentCustomTextureOverrides);

			// Detect changes
			if (!_customMaterialOverridesPrev.SequenceEqual(customMaterialOverridesNew) ||
				!_customTextureOverridesPrev.SequenceEqual(customTextureOverridesNew)) {
				// 2. Restore old values
				componentCustomMaterialOverrides.Clear();
				componentCustomTextureOverrides.Clear();
				componentCustomMaterialOverrides.AddRange(_customMaterialOverridesPrev);
				componentCustomTextureOverrides.AddRange(_customTextureOverridesPrev);

				// 3. Remove overrides
				RemoveCustomMaterials();

				// 4. Restore new values
				componentCustomMaterialOverrides.Clear();
				componentCustomTextureOverrides.Clear();
				componentCustomMaterialOverrides.AddRange(customMaterialOverridesNew);
				componentCustomTextureOverrides.AddRange(customTextureOverridesNew);

				// 5. Restore overrides
				SetCustomMaterials();

				if (skeletonGraphic != null)
					skeletonGraphic.LateUpdate();
			}

			_customMaterialOverridesPrev = CopyList(componentCustomMaterialOverrides);
			_customTextureOverridesPrev = CopyList(componentCustomTextureOverrides);

			if (SpineInspectorUtility.LargeCenteredButton(SpineInspectorUtility.TempContent("Clear and Reapply Changes", tooltip: "Removes all non-serialized overrides in the SkeletonGraphic and reapplies the overrides on this component."))) {
				if (skeletonGraphic != null) {
					skeletonGraphic.CustomMaterialOverride.Clear();
					skeletonGraphic.CustomTextureOverride.Clear();
					RemoveCustomMaterials();
					SetCustomMaterials();
					skeletonGraphic.LateUpdate();
				}
			}
		}

		void RemoveCustomMaterials () {
			RemoveCustomMaterialOverrides.Invoke(component, null);
			RemoveCustomTextureOverrides.Invoke(component, null);
		}

		void SetCustomMaterials () {
			SetCustomMaterialOverrides.Invoke(component, null);
			SetCustomTextureOverrides.Invoke(component, null);
		}

		static List<T> CopyList<T> (List<T> list) {
			return list.GetRange(0, list.Count);
		}
	}
}
