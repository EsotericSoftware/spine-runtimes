/*****************************************************************************
 * SkeletonRendererCustomMaterialsInspector created by Lost Polygon
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

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
		List<SkeletonRendererCustomMaterials.AtlasMaterialOverride> _customMaterialOverridesPrev;
		List<SkeletonRendererCustomMaterials.SlotMaterialOverride> _customSlotMaterialsPrev;
		SkeletonRendererCustomMaterials component;
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
			BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
			System.Type cm = typeof(SkeletonRendererCustomMaterials);
			RemoveCustomMaterialOverrides = cm.GetMethod("RemoveCustomMaterialOverrides", flags);
			RemoveCustomSlotMaterials = cm.GetMethod("RemoveCustomSlotMaterials", flags);
			SetCustomMaterialOverrides = cm.GetMethod("SetCustomMaterialOverrides", flags);
			SetCustomSlotMaterials = cm.GetMethod("SetCustomSlotMaterials", flags);
		}

		public override void OnInspectorGUI () {
			component = (SkeletonRendererCustomMaterials)target;
			var skeletonRenderer = component.skeletonRenderer;

			// Draw the default inspector
			DrawDefaultInspector();

			// Fill with current values at start
			if (_customMaterialOverridesPrev == null || _customSlotMaterialsPrev == null) {
				_customMaterialOverridesPrev = CopyList(component.CustomMaterialOverrides);
				_customSlotMaterialsPrev = CopyList(component.CustomSlotMaterials);
			}

			// Compare new values with saved. If change is detected: 
			// store new values, restore old values, remove overrides, restore new values, restore overrides.

			// 1. Store new values
			var customMaterialOverridesNew = CopyList(component.CustomMaterialOverrides);
			var customSlotMaterialsNew = CopyList(component.CustomSlotMaterials);
			
			// Detect changes
			if (!_customMaterialOverridesPrev.SequenceEqual(customMaterialOverridesNew) ||
				!_customSlotMaterialsPrev.SequenceEqual(customSlotMaterialsNew)) {
				// 2. Restore old values
				component.CustomMaterialOverrides.Clear();
				component.CustomSlotMaterials.Clear();
				component.CustomMaterialOverrides.AddRange(_customMaterialOverridesPrev);
				component.CustomSlotMaterials.AddRange(_customSlotMaterialsPrev);

				// 3. Remove overrides
				RemoveCustomMaterials();

				// 4. Restore new values
				component.CustomMaterialOverrides.Clear();
				component.CustomSlotMaterials.Clear();
				component.CustomMaterialOverrides.AddRange(customMaterialOverridesNew);
				component.CustomSlotMaterials.AddRange(customSlotMaterialsNew);

				// 5. Restore overrides
				SetCustomMaterials();

				if (skeletonRenderer != null)
					skeletonRenderer.LateUpdate();
			}

			_customMaterialOverridesPrev = CopyList(component.CustomMaterialOverrides);
			_customSlotMaterialsPrev = CopyList(component.CustomSlotMaterials);

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