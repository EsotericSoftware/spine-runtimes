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
#endif

#if UNITY_2018_2_OR_NEWER
#define HAS_CULL_TRANSPARENT_MESH
#endif

#if UNITY_2017_2_OR_NEWER
#define NEWPLAYMODECALLBACKS
#endif

using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	using Icons = SpineEditorUtilities.Icons;

	[CustomEditor(typeof(SkeletonGraphic))]
	[CanEditMultipleObjects]

	public class SkeletonGraphicInspector : ISkeletonRendererInspector {

		protected SerializedProperty material, color;
		protected SerializedProperty additiveMaterial, multiplyMaterial, screenMaterial;
		protected SerializedProperty freeze;
		protected SerializedProperty allowMultipleCanvasRenderers,
			updateSeparatorPartLocation, updateSeparatorPartScale;
		protected SerializedProperty raycastTarget, maskable;
		protected SerializedProperty layoutScaleMode, editReferenceRect;

		protected GUIContent allowMultipleCanvasRenderersLabel, updateSeparatorPartLocationLabel,
			updateSeparatorPartScaleLabel;

		protected SkeletonGraphic thisSkeletonGraphic;

		protected override void OnEnable () {
			base.OnEnable();

			// Labels
			allowMultipleCanvasRenderersLabel = new GUIContent("Multiple CanvasRenderers",
				"When set to true, SkeletonGraphic no longer uses a single CanvasRenderer" +
				"but automatically creates the required number of child CanvasRenderer" +
				"GameObjects for each required draw call (submesh).");
			updateSeparatorPartLocationLabel = new GUIContent("Update Part Location",
				"Update separator part GameObject location to match the position of the SkeletonGraphic. " +
				"This can be helpful when re-parenting parts to a different GameObject.");
			updateSeparatorPartScaleLabel = new GUIContent("Update Part Scale",
				"Update separator part GameObject scale to match the scale (lossyScale) of the SkeletonGraphic. " +
				"This can be helpful when re-parenting parts to a different GameObject.");

			// Properties
			thisSkeletonGraphic = target as SkeletonGraphic;

			// MaskableGraphic
			material = serializedObject.FindProperty("m_Material");
			color = serializedObject.FindProperty("m_SkeletonColor");
			raycastTarget = serializedObject.FindProperty("m_RaycastTarget");
			maskable = serializedObject.FindProperty("m_Maskable");

			// SkeletonGraphic
			additiveMaterial = serializedObject.FindProperty("additiveMaterial");
			multiplyMaterial = serializedObject.FindProperty("multiplyMaterial");
			screenMaterial = serializedObject.FindProperty("screenMaterial");
			freeze = serializedObject.FindProperty("freeze");
			allowMultipleCanvasRenderers = serializedObject.FindProperty("allowMultipleCanvasRenderers");
			updateSeparatorPartLocation = serializedObject.FindProperty("updateSeparatorPartLocation");
			updateSeparatorPartScale = serializedObject.FindProperty("updateSeparatorPartScale");
			layoutScaleMode = serializedObject.FindProperty("layoutScaleMode");
			editReferenceRect = serializedObject.FindProperty("editReferenceRect");

#if NEWPLAYMODECALLBACKS
			EditorApplication.playModeStateChanged += OnPlaymodeChanged;
#else
			EditorApplication.playmodeStateChanged += OnPlaymodeChanged;
#endif
		}

		void OnDisable () {
#if NEWPLAYMODECALLBACKS
			EditorApplication.playModeStateChanged -= OnPlaymodeChanged;
#else
			EditorApplication.playmodeStateChanged -= OnPlaymodeChanged;
#endif
			DisableEditReferenceRectMode();
		}

#if NEWPLAYMODECALLBACKS
		void OnPlaymodeChanged (PlayModeStateChange mode) {
#else
		void OnPlaymodeChanged () {
#endif
			DisableEditReferenceRectMode();
		}

		void DisableEditReferenceRectMode () {
			foreach (UnityEngine.Object c in targets) {
				SkeletonGraphic component = c as SkeletonGraphic;
				if (component == null) continue;
				component.EditReferenceRect = false;
			}
		}



		protected override void FirstPropertyFields () {
			using (new SpineInspectorUtility.LabelWidthScope(100)) {
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.PropertyField(material);
					if (GUILayout.Button("Detect", EditorStyles.miniButton, GUILayout.Width(67f))) {
						Undo.RecordObjects(targets, "Detect Material");
						foreach (UnityEngine.Object target in targets) {
							SkeletonGraphic skeletonGraphic = target as SkeletonGraphic;
							if (skeletonGraphic == null) continue;
							SkeletonGraphicUtility.DetectMaterial(skeletonGraphic);
						}
					}
				}
				EditorGUILayout.PropertyField(color);
			}
		}

		protected override void MaterialWarningsBox () {
			string errorMessage = null;
			if (SpineEditorUtilities.Preferences.componentMaterialWarning &&
				MaterialChecks.IsMaterialSetupProblematic(thisSkeletonGraphic, ref errorMessage)) {
				EditorGUILayout.HelpBox(errorMessage, MessageType.Error, true);
			}
		}

		protected override void VertexDataProperties () {
			using (new EditorGUILayout.HorizontalScope()) {
				EditorGUILayout.PropertyField(tintBlack, TintBlackLabel);
				if (GUILayout.Button("Detect", EditorStyles.miniButton, GUILayout.Width(65f))) {
					Undo.RecordObjects(targets, "Detect Tint Black");
					foreach (UnityEngine.Object target in targets) {
						SkeletonGraphic skeletonGraphic = target as SkeletonGraphic;
						if (skeletonGraphic == null) continue;
						SkeletonGraphicUtility.DetectTintBlack(skeletonGraphic);
					}
				}
			}
			using (new EditorGUILayout.HorizontalScope()) {
				EditorGUILayout.PropertyField(canvasGroupCompatible, CanvasGroupCompatibleLabel);
				if (GUILayout.Button("Detect", EditorStyles.miniButton, GUILayout.Width(65f))) {
					Undo.RecordObjects(targets, "Detect CanvasGroup Compatible");
					foreach (UnityEngine.Object target in targets) {
						SkeletonGraphic skeletonGraphic = target as SkeletonGraphic;
						if (skeletonGraphic == null) continue;
						SkeletonGraphicUtility.DetectCanvasGroupCompatible(skeletonGraphic);
					}
				}
			}
			using (new EditorGUILayout.HorizontalScope()) {
				EditorGUILayout.PropertyField(pmaVertexColors, PMAVertexColorsLabel);
				if (GUILayout.Button("Detect", EditorStyles.miniButton, GUILayout.Width(65f))) {
					Undo.RecordObjects(targets, "Detect PMA Vertex Colors");
					foreach (UnityEngine.Object target in targets) {
						SkeletonGraphic skeletonGraphic = target as SkeletonGraphic;
						if (skeletonGraphic == null) continue;
						SkeletonGraphicUtility.DetectPMAVertexColors(skeletonGraphic);
					}
				}
			}
			using (new EditorGUILayout.HorizontalScope()) {
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Detect Settings", EditorStyles.miniButton, GUILayout.Width(100f))) {
					Undo.RecordObjects(targets, "Detect Settings");
					foreach (UnityEngine.Object target in targets) {
						SkeletonGraphic skeletonGraphic = target as SkeletonGraphic;
						if (skeletonGraphic == null) continue;
						SkeletonGraphicUtility.DetectTintBlack(skeletonGraphic);
						SkeletonGraphicUtility.DetectCanvasGroupCompatible(skeletonGraphic);
						SkeletonGraphicUtility.DetectPMAVertexColors(skeletonGraphic);
					}
				}
				if (GUILayout.Button("Detect Material", EditorStyles.miniButton, GUILayout.Width(100f))) {
					Undo.RecordObjects(targets, "Detect Material");
					foreach (UnityEngine.Object target in targets) {
						SkeletonGraphic skeletonGraphic = target as SkeletonGraphic;
						if (skeletonGraphic == null) continue;
						SkeletonGraphicUtility.DetectMaterial(skeletonGraphic);
					}
				}
			}

			EditorGUILayout.PropertyField(addNormals, AddNormalsLabel);
			EditorGUILayout.PropertyField(calculateTangents, CalculateTangentsLabel);
			EditorGUILayout.PropertyField(immutableTriangles, ImmutableTrianglesLabel);
		}

		protected override void RendererProperties () {

			bool isSingleRendererOnly = (!allowMultipleCanvasRenderers.hasMultipleDifferentValues && allowMultipleCanvasRenderers.boolValue == false);
			bool isSeparationEnabledButNotMultipleRenderers =
				 isSingleRendererOnly && (!enableSeparatorSlots.hasMultipleDifferentValues && enableSeparatorSlots.boolValue == true);
			bool meshRendersIncorrectlyWithSingleRenderer =
				isSingleRendererOnly && SkeletonHasMultipleSubmeshes();

			if (isSeparationEnabledButNotMultipleRenderers || meshRendersIncorrectlyWithSingleRenderer)
				advancedFoldout = true;

			base.RendererProperties();

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(allowMultipleCanvasRenderers, allowMultipleCanvasRenderersLabel);

			if (GUILayout.Button(new GUIContent("Trim Renderers", "Remove currently unused CanvasRenderer GameObjects. These will be regenerated whenever needed."),
				EditorStyles.miniButton, GUILayout.Width(100f))) {

				Undo.RecordObjects(targets, "Trim Renderers");
				foreach (UnityEngine.Object target in targets) {
					SkeletonGraphic skeletonGraphic = target as SkeletonGraphic;
					if (skeletonGraphic == null) continue;
					skeletonGraphic.TrimRenderers();
				}
			}
			EditorGUILayout.EndHorizontal();

			BlendModeMaterials blendModeMaterials = thisSkeletonGraphic.skeletonDataAsset.blendModeMaterials;
			if (allowMultipleCanvasRenderers.boolValue == true && blendModeMaterials.RequiresBlendModeMaterials) {
				using (new SpineInspectorUtility.IndentScope()) {
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Blend Mode Materials", EditorStyles.boldLabel);

					if (GUILayout.Button(new GUIContent("Detect", "Auto-Assign Blend Mode Materials according to Vertex Data and Texture settings."),
						EditorStyles.miniButton, GUILayout.Width(100f))) {

						Undo.RecordObjects(targets, "Detect Blend Mode Materials");
						foreach (UnityEngine.Object target in targets) {
							SkeletonGraphic skeletonGraphic = target as SkeletonGraphic;
							if (skeletonGraphic == null) continue;
							SkeletonGraphicUtility.DetectBlendModeMaterials(skeletonGraphic);
						}
					}
					EditorGUILayout.EndHorizontal();

					bool usesAdditiveMaterial = blendModeMaterials.applyAdditiveMaterial;
					bool pmaVertexColors = thisSkeletonGraphic.MeshSettings.pmaVertexColors;
					if (pmaVertexColors)
						using (new EditorGUI.DisabledGroupScope(true)) {
							EditorGUILayout.LabelField("Additive Material - Unused with PMA Vertex Colors", EditorStyles.label);
						}
					else if (usesAdditiveMaterial)
						EditorGUILayout.PropertyField(additiveMaterial, SpineInspectorUtility.TempContent("Additive Material", null, "SkeletonGraphic Material for 'Additive' blend mode slots. Unused when 'PMA Vertex Colors' is enabled."));
					else
						using (new EditorGUI.DisabledGroupScope(true)) {
							EditorGUILayout.LabelField("No Additive Mat - 'Apply Additive Material' disabled at SkeletonDataAsset", EditorStyles.label);
						}
					EditorGUILayout.PropertyField(multiplyMaterial, SpineInspectorUtility.TempContent("Multiply Material", null, "SkeletonGraphic Material for 'Multiply' blend mode slots."));
					EditorGUILayout.PropertyField(screenMaterial, SpineInspectorUtility.TempContent("Screen Material", null, "SkeletonGraphic Material for 'Screen' blend mode slots."));
				}
			}

			// warning box
			if (isSeparationEnabledButNotMultipleRenderers) {
				using (new SpineInspectorUtility.BoxScope()) {
					meshSettings.isExpanded = true;
					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("'Multiple Canvas Renderers' must be enabled\nwhen 'Enable Separation' is enabled.", Icons.warning), GUILayout.Height(42), GUILayout.Width(340));
				}
			} else if (meshRendersIncorrectlyWithSingleRenderer) {
				using (new SpineInspectorUtility.BoxScope()) {
					meshSettings.isExpanded = true;
					EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("This mesh uses multiple atlas pages or blend modes.\n" +
																				"You need to enable 'Multiple Canvas Renderers'\n" +
																				"for correct rendering. Consider packing\n" +
																				"attachments to a single atlas page if possible.", Icons.warning), GUILayout.Height(60), GUILayout.Width(340));
				}
			}
		}

		protected override void AfterAdvancedPropertyFields () {

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(freeze);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(raycastTarget);
			if (maskable != null) EditorGUILayout.PropertyField(maskable);

			EditorGUILayout.PropertyField(layoutScaleMode);

			using (new EditorGUI.DisabledGroupScope(layoutScaleMode.intValue == 0)) {
				EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight + 5));
				EditorGUILayout.PrefixLabel("Edit Layout Bounds");
				editReferenceRect.boolValue = GUILayout.Toggle(editReferenceRect.boolValue,
					EditorGUIUtility.IconContent("EditCollider"), EditorStyles.miniButton, GUILayout.Width(40f));
				EditorGUILayout.EndHorizontal();
			}
			if (layoutScaleMode.intValue == 0) {
				editReferenceRect.boolValue = false;
			}

			using (new EditorGUI.DisabledGroupScope(editReferenceRect.boolValue == false && layoutScaleMode.intValue != 0)) {
				EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight + 5));
				EditorGUILayout.PrefixLabel("Match RectTransform with Mesh");
				if (GUILayout.Button("Match", EditorStyles.miniButton, GUILayout.Width(65f))) {
					foreach (UnityEngine.Object target in targets) {
						SkeletonGraphic skeletonGraphic = target as SkeletonGraphic;
						if (skeletonGraphic == null) continue;
						MatchRectTransformWithBounds(skeletonGraphic);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		protected bool SkeletonHasMultipleSubmeshes () {
			foreach (UnityEngine.Object target in targets) {
				SkeletonGraphic skeletonGraphic = target as SkeletonGraphic;
				if (skeletonGraphic == null) continue;
				if (skeletonGraphic.HasMultipleSubmeshInstructions())
					return true;
			}
			return false;
		}

		protected override void AdditionalSeparatorSlotProperties () {
			EditorGUILayout.PropertyField(updateSeparatorPartLocation, updateSeparatorPartLocationLabel);
			EditorGUILayout.PropertyField(updateSeparatorPartScale, updateSeparatorPartScaleLabel);
		}

		public override void OnSceneGUI () {
			base.OnSceneGUI();

			SkeletonGraphic skeletonGraphic = (SkeletonGraphic)target;

			if (skeletonGraphic.layoutScaleMode != SkeletonGraphic.LayoutMode.None) {
				if (skeletonGraphic.EditReferenceRect) {
					SpineHandles.DrawRectTransformRect(skeletonGraphic, Color.gray);
					SpineHandles.DrawReferenceRect(skeletonGraphic, Color.green);
				} else {
					SpineHandles.DrawReferenceRect(skeletonGraphic, Color.blue);
				}
			}
			SpineHandles.DrawPivotOffsetHandle(skeletonGraphic, Color.green);
		}

		#region Menus
		[MenuItem("CONTEXT/SkeletonGraphic/Match RectTransform with Mesh Bounds")]
		static void MatchRectTransformWithBounds (MenuCommand command) {
			SkeletonGraphic skeletonGraphic = (SkeletonGraphic)command.context;
			MatchRectTransformWithBounds(skeletonGraphic);
		}

		static void MatchRectTransformWithBounds (SkeletonGraphic skeletonGraphic) {
			if (!skeletonGraphic.MatchRectTransformWithBounds())
				Debug.Log("Mesh was not previously generated.");
		}
		#endregion
	}
}
