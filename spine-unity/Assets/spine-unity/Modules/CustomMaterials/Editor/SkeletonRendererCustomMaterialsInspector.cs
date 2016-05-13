/*****************************************************************************
 * SkeletonRendererCustomMaterialsInspector created by Lost Polygon
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Spine.Unity.Modules;

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

			if (GUILayout.Button(new GUIContent("Clear and Reapply Changes", "Removes all non-serialized overrides in the SkeletonRenderer and reapplies the overrides on this component."))) {
				if (skeletonRenderer != null) {
					skeletonRenderer.CustomMaterialOverride.Clear();
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