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

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#else
#define NO_PREFAB_MESH
#endif

#if UNITY_2018_1_OR_NEWER
#define PER_MATERIAL_PROPERTY_BLOCKS
#endif

#if UNITY_2017_1_OR_NEWER
#define BUILT_IN_SPRITE_MASK_COMPONENT
#endif

#if UNITY_2020_2_OR_NEWER
#define HAS_ON_POSTPROCESS_PREFAB
#endif

using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	[CustomEditor(typeof(SkeletonRenderer))]
	[CanEditMultipleObjects]
	public class SkeletonRendererInspector : ISkeletonRendererInspector {

		protected SerializedProperty singleSubmesh;
		protected SerializedProperty fixPrefabOverrideViaMeshFilter;
		protected SerializedProperty maskInteraction;
		protected SpineInspectorUtility.SerializedSortingProperties sortingProperties;


		protected GUIContent SingleSubmeshLabel;
		protected GUIContent FixPrefabOverrideViaMeshFilterLabel;
		protected GUIContent MaskInteractionLabel;

		const string ReloadButtonString = "Reload";
		static GUILayoutOption reloadButtonWidth;
		static GUILayoutOption ReloadButtonWidth { get { return reloadButtonWidth = reloadButtonWidth ?? GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(ReloadButtonString)).x + 20); } }
		static GUIStyle ReloadButtonStyle { get { return EditorStyles.miniButton; } }


		protected override void OnEnable () {
			base.OnEnable();

			// Labels
			SingleSubmeshLabel = new GUIContent("Use Single Submesh", "Simplifies submesh generation by assuming you are only using one Material and need only one submesh. This is will disable multiple materials, render separation, and custom slot materials.");
			FixPrefabOverrideViaMeshFilterLabel = new GUIContent("Fix Prefab Overr. MeshFilter", "Fixes the prefab always being marked as changed (sets the MeshFilter's hide flags to DontSaveInEditor), but at the cost of references to the MeshFilter by other components being lost. For global settings see Edit - Preferences - Spine.");
			MaskInteractionLabel = new GUIContent("Mask Interaction", "SkeletonRenderer's interaction with a Sprite Mask.");

			// Properties
			singleSubmesh = serializedObject.FindProperty("singleSubmesh");
			fixPrefabOverrideViaMeshFilter = serializedObject.FindProperty("fixPrefabOverrideViaMeshFilter");
			maskInteraction = serializedObject.FindProperty("maskInteraction");

			SerializedObject renderersSerializedObject = SpineInspectorUtility.GetRenderersSerializedObject(serializedObject); // Allows proper multi-edit behavior.
			sortingProperties = new SpineInspectorUtility.SerializedSortingProperties(renderersSerializedObject);
		}

		protected override void InspectorDrawPreparation () {
#if BUILT_IN_SPRITE_MASK_COMPONENT
			foreach (UnityEngine.Object t in targets)
				SpineMaskUtilities.EditorSetupSpriteMaskMaterials((SkeletonRenderer)t);
#endif
		}

		protected override void FirstPropertyFields () {
			EditorGUILayout.Space();

			SpineInspectorUtility.SortingPropertyFields(sortingProperties, applyModifiedProperties: true);
			if (maskInteraction != null) EditorGUILayout.PropertyField(maskInteraction, MaskInteractionLabel);
		}

		protected override void VertexDataProperties () {
			EditorGUILayout.PropertyField(pmaVertexColors, PMAVertexColorsLabel);
			EditorGUILayout.PropertyField(tintBlack, TintBlackLabel);
			EditorGUILayout.PropertyField(addNormals, AddNormalsLabel);
			EditorGUILayout.PropertyField(calculateTangents, CalculateTangentsLabel);
		}

		protected override void RendererProperties () {
			base.RendererProperties();

			if (singleSubmesh != null) EditorGUILayout.PropertyField(singleSubmesh, SingleSubmeshLabel);

#if HAS_ON_POSTPROCESS_PREFAB
			if (fixPrefabOverrideViaMeshFilter != null) EditorGUILayout.PropertyField(fixPrefabOverrideViaMeshFilter, FixPrefabOverrideViaMeshFilterLabel);
			EditorGUILayout.Space();
#endif
		}

		protected override void MaterialWarningsBox () {
			string errorMessage = null;
			if (SpineEditorUtilities.Preferences.componentMaterialWarning &&
				MaterialChecks.IsMaterialSetupProblematic((SkeletonRenderer)this.target, ref errorMessage)) {
				EditorGUILayout.HelpBox(errorMessage, MessageType.Error, true);
			}
		}
	}
}
