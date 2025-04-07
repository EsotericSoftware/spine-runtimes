/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	using Event = UnityEngine.Event;
	using Icons = SpineEditorUtilities.Icons;

	[CustomEditor(typeof(SkeletonRenderer))]
	[CanEditMultipleObjects]
	public class SkeletonRendererInspector : UnityEditor.Editor {
		public static bool advancedFoldout;
		protected bool loadingFailed = false;

		const string SeparatorSlotNamesFieldName = "separatorSlotNames";

		protected SerializedProperty skeletonDataAsset, initialSkinName;
		protected SerializedProperty initialFlipX, initialFlipY;
		protected SerializedProperty updateTiming, updateWhenInvisible, singleSubmesh, separatorSlotNames,
			clearStateOnDisable, immutableTriangles, fixDrawOrder, fixPrefabOverrideViaMeshFilter;
		protected SerializedProperty normals, tangents, zSpacing, pmaVertexColors, tintBlack; // MeshGenerator settings
		protected SerializedProperty maskInteraction;
		protected SerializedProperty maskMaterialsNone, maskMaterialsInside, maskMaterialsOutside;
		protected SerializedProperty physicsPositionInheritanceFactor, physicsRotationInheritanceFactor, physicsMovementRelativeTo;
		protected SpineInspectorUtility.SerializedSortingProperties sortingProperties;
		protected bool wasInitParameterChanged = false;
		protected bool requireRepaint = false;

		protected bool isInspectingPrefab;
		protected bool forceReloadQueued = false;
		protected bool setMaskNoneMaterialsQueued = false;
		protected bool setInsideMaskMaterialsQueued = false;
		protected bool setOutsideMaskMaterialsQueued = false;
		protected bool deleteInsideMaskMaterialsQueued = false;
		protected bool deleteOutsideMaskMaterialsQueued = false;

		protected GUIContent SkeletonDataAssetLabel, SkeletonUtilityButtonContent;
		protected GUIContent PMAVertexColorsLabel, ClearStateOnDisableLabel, ZSpacingLabel, ImmubleTrianglesLabel,
			TintBlackLabel, UpdateTimingLabel, UpdateWhenInvisibleLabel, SingleSubmeshLabel, FixDrawOrderLabel, FixPrefabOverrideViaMeshFilterLabel;
		protected GUIContent NormalsLabel, TangentsLabel, MaskInteractionLabel;
		protected GUIContent MaskMaterialsHeadingLabel, MaskMaterialsNoneLabel, MaskMaterialsInsideLabel, MaskMaterialsOutsideLabel;
		protected GUIContent SetMaterialButtonLabel, ClearMaterialButtonLabel, DeleteMaterialButtonLabel;

		readonly GUIContent PhysicsPositionInheritanceFactorLabel = new GUIContent("Position",
			"When set to non-zero, Transform position movement in X and Y direction is applied to skeleton " +
			"PhysicsConstraints, multiplied by these " +
			"\nX and Y scale factors to the right. Typical (X,Y) values are " +
			"\n(1,1) to apply XY movement normally, " +
			"\n(2,2) to apply movement with double intensity, " +
			"\n(1,0) to apply only horizontal movement, or" +
			"\n(0,0) to not apply any Transform position movement at all.");
		readonly GUIContent PhysicsRotationInheritanceFactorLabel = new GUIContent("Rotation",
			"When set to non-zero, Transform rotation movement is applied to skeleton PhysicsConstraints, " +
			"multiplied by this scale factor to the right. Typical values are " +
			"\n1 to apply movement normally, " +
			"\n2 to apply movement with double intensity, or " +
			"\n0 to not apply any Transform rotation movement at all.");
		readonly GUIContent PhysicsMovementRelativeToLabel = new GUIContent("Movement relative to",
			"Reference transform relative to which physics movement will be calculated, or null to use world location.");

		const string ReloadButtonString = "Reload";
		static GUILayoutOption reloadButtonWidth;
		static GUILayoutOption ReloadButtonWidth { get { return reloadButtonWidth = reloadButtonWidth ?? GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(ReloadButtonString)).x + 20); } }
		static GUIStyle ReloadButtonStyle { get { return EditorStyles.miniButton; } }

		protected bool TargetIsValid {
			get {
				if (serializedObject.isEditingMultipleObjects) {
					foreach (UnityEngine.Object o in targets) {
						SkeletonRenderer component = (SkeletonRenderer)o;
						if (!component.valid)
							return false;
					}
					return true;
				} else {
					SkeletonRenderer component = (SkeletonRenderer)target;
					return component.valid;
				}
			}
		}

		protected virtual void OnEnable () {
#if NEW_PREFAB_SYSTEM
			isInspectingPrefab = false;
#else
			isInspectingPrefab = (PrefabUtility.GetPrefabType(target) == PrefabType.Prefab);
#endif
			SpineEditorUtilities.ConfirmInitialization();
			loadingFailed = false;

			// Labels
			SkeletonDataAssetLabel = new GUIContent("SkeletonData Asset", Icons.spine);
			SkeletonUtilityButtonContent = new GUIContent("Add Skeleton Utility", Icons.skeletonUtility);
			ImmubleTrianglesLabel = new GUIContent("Immutable Triangles", "Enable to optimize rendering for skeletons that never change attachment visbility");
			PMAVertexColorsLabel = new GUIContent("PMA Vertex Colors", "Use this if you are using the default Spine/Skeleton shader or any premultiply-alpha shader.");
			ClearStateOnDisableLabel = new GUIContent("Clear State On Disable", "Use this if you are pooling or enabling/disabling your Spine GameObject.");
			ZSpacingLabel = new GUIContent("Z Spacing", "A value other than 0 adds a space between each rendered attachment to prevent Z Fighting when using shaders that read or write to the depth buffer. Large values may cause unwanted parallax and spaces depending on camera setup.");
			NormalsLabel = new GUIContent("Add Normals", "Use this if your shader requires vertex normals. A more efficient solution for 2D setups is to modify the shader to assume a single normal value for the whole mesh.");
			TangentsLabel = new GUIContent("Solve Tangents", "Calculates the tangents per frame. Use this if you are using lit shaders (usually with normal maps) that require vertex tangents.");
			TintBlackLabel = new GUIContent("Tint Black (!)", "Adds black tint vertex data to the mesh as UV2 and UV3. Black tinting requires that the shader interpret UV2 and UV3 as black tint colors for this effect to work. You may also use the default [Spine/Skeleton Tint Black] shader.\n\nIf you only need to tint the whole skeleton and not individual parts, the [Spine/Skeleton Tint] shader is recommended for better efficiency and changing/animating the _Black material property via MaterialPropertyBlock.");
			SingleSubmeshLabel = new GUIContent("Use Single Submesh", "Simplifies submesh generation by assuming you are only using one Material and need only one submesh. This is will disable multiple materials, render separation, and custom slot materials.");
			UpdateTimingLabel = new GUIContent("Animation Update", "Whether to update the animation in normal Update (the default), physics step FixedUpdate, or manually via a user call.");
			UpdateWhenInvisibleLabel = new GUIContent("Update When Invisible", "Update mode used when the MeshRenderer becomes invisible. Update mode is automatically reset to UpdateMode.FullUpdate when the mesh becomes visible again.");
			FixDrawOrderLabel = new GUIContent("Fix Draw Order", "Applies only when 3+ submeshes are used (2+ materials with alternating order, e.g. \"A B A\"). If true, GPU instancing will be disabled at all materials and MaterialPropertyBlocks are assigned at each material to prevent aggressive batching of submeshes by e.g. the LWRP renderer, leading to incorrect draw order (e.g. \"A1 B A2\" changed to \"A1A2 B\"). You can disable this parameter when everything is drawn correctly to save the additional performance cost. Note: the GPU instancing setting will remain disabled at affected material assets after exiting play mode, you have to enable it manually if you accidentally enabled this parameter.");
			FixPrefabOverrideViaMeshFilterLabel = new GUIContent("Fix Prefab Overr. MeshFilter", "Fixes the prefab always being marked as changed (sets the MeshFilter's hide flags to DontSaveInEditor), but at the cost of references to the MeshFilter by other components being lost. For global settings see Edit - Preferences - Spine.");
			MaskInteractionLabel = new GUIContent("Mask Interaction", "SkeletonRenderer's interaction with a Sprite Mask.");
			MaskMaterialsHeadingLabel = new GUIContent("Mask Interaction Materials", "Materials used for different interaction with sprite masks.");
			MaskMaterialsNoneLabel = new GUIContent("Normal Materials", "Normal materials used when Mask Interaction is set to None.");
			MaskMaterialsInsideLabel = new GUIContent("Inside Mask", "Materials used when Mask Interaction is set to Inside Mask.");
			MaskMaterialsOutsideLabel = new GUIContent("Outside Mask", "Materials used when Mask Interaction is set to Outside Mask.");
			SetMaterialButtonLabel = new GUIContent("Set", "Prepares material references for switching to the corresponding Mask Interaction mode at runtime. Creates the required materials if they do not exist.");
			ClearMaterialButtonLabel = new GUIContent("Clear", "Clears unused material references. Note: when switching to the corresponding Mask Interaction mode at runtime, a new material is generated on the fly.");
			DeleteMaterialButtonLabel = new GUIContent("Delete", "Clears unused material references and deletes the corresponding assets. Note: when switching to the corresponding Mask Interaction mode at runtime, a new material is generated on the fly.");

			SerializedObject so = this.serializedObject;
			skeletonDataAsset = so.FindProperty("skeletonDataAsset");
			initialSkinName = so.FindProperty("initialSkinName");
			initialFlipX = so.FindProperty("initialFlipX");
			initialFlipY = so.FindProperty("initialFlipY");
			normals = so.FindProperty("addNormals");
			tangents = so.FindProperty("calculateTangents");
			immutableTriangles = so.FindProperty("immutableTriangles");
			pmaVertexColors = so.FindProperty("pmaVertexColors");
			clearStateOnDisable = so.FindProperty("clearStateOnDisable");
			tintBlack = so.FindProperty("tintBlack");
			updateTiming = so.FindProperty("updateTiming");
			updateWhenInvisible = so.FindProperty("updateWhenInvisible");
			singleSubmesh = so.FindProperty("singleSubmesh");
			fixDrawOrder = so.FindProperty("fixDrawOrder");
			fixPrefabOverrideViaMeshFilter = so.FindProperty("fixPrefabOverrideViaMeshFilter");
			maskInteraction = so.FindProperty("maskInteraction");
			maskMaterialsNone = so.FindProperty("maskMaterials.materialsMaskDisabled");
			maskMaterialsInside = so.FindProperty("maskMaterials.materialsInsideMask");
			maskMaterialsOutside = so.FindProperty("maskMaterials.materialsOutsideMask");
			physicsPositionInheritanceFactor = so.FindProperty("physicsPositionInheritanceFactor");
			physicsRotationInheritanceFactor = so.FindProperty("physicsRotationInheritanceFactor");
			physicsMovementRelativeTo = so.FindProperty("physicsMovementRelativeTo");

			separatorSlotNames = so.FindProperty("separatorSlotNames");
			separatorSlotNames.isExpanded = true;

			zSpacing = so.FindProperty("zSpacing");

			SerializedObject renderersSerializedObject = SpineInspectorUtility.GetRenderersSerializedObject(serializedObject); // Allows proper multi-edit behavior.
			sortingProperties = new SpineInspectorUtility.SerializedSortingProperties(renderersSerializedObject);
		}

		public void OnSceneGUI () {
			SkeletonRenderer skeletonRenderer = (SkeletonRenderer)target;
			if (loadingFailed)
				return;

			Skeleton skeleton = skeletonRenderer.Skeleton;
			if (skeleton == null) {
				loadingFailed = true;
				return;
			}
			Transform transform = skeletonRenderer.transform;
			if (skeleton == null) return;

			SpineHandles.DrawBones(transform, skeleton);
		}

		override public void OnInspectorGUI () {
			bool multi = serializedObject.isEditingMultipleObjects;
			DrawInspectorGUI(multi);
			HandleSkinChange();
			if (serializedObject.ApplyModifiedProperties() || SpineInspectorUtility.UndoRedoPerformed(Event.current) ||
				AreAnyMaskMaterialsMissing()) {
				if (!Application.isPlaying) {
					foreach (UnityEngine.Object o in targets)
						SpineEditorUtilities.ReinitializeComponent((SkeletonRenderer)o);
					SceneView.RepaintAll();
				}
			}

			if (!isInspectingPrefab) {
				if (requireRepaint) {
					SceneView.RepaintAll();
					requireRepaint = false;
				}
			}
		}

		protected virtual void DrawInspectorGUI (bool multi) {
			// Initialize.
			if (Event.current.type == EventType.Layout) {
				if (forceReloadQueued) {
					forceReloadQueued = false;
					foreach (UnityEngine.Object c in targets) {
						SpineEditorUtilities.ReloadSkeletonDataAssetAndComponent(c as SkeletonRenderer);
					}
				} else {
					foreach (UnityEngine.Object c in targets) {
						SkeletonRenderer component = c as SkeletonRenderer;
						if (!component.valid) {
							SpineEditorUtilities.ReinitializeComponent(component);
							if (!component.valid) continue;
						}
					}
				}

#if BUILT_IN_SPRITE_MASK_COMPONENT
				if (setMaskNoneMaterialsQueued) {
					setMaskNoneMaterialsQueued = false;
					foreach (UnityEngine.Object c in targets)
						EditorSetMaskMaterials(c as SkeletonRenderer, SpriteMaskInteraction.None);
				}
				if (setInsideMaskMaterialsQueued) {
					setInsideMaskMaterialsQueued = false;
					foreach (UnityEngine.Object c in targets)
						EditorSetMaskMaterials(c as SkeletonRenderer, SpriteMaskInteraction.VisibleInsideMask);
				}
				if (setOutsideMaskMaterialsQueued) {
					setOutsideMaskMaterialsQueued = false;
					foreach (UnityEngine.Object c in targets)
						EditorSetMaskMaterials(c as SkeletonRenderer, SpriteMaskInteraction.VisibleOutsideMask);
				}

				if (deleteInsideMaskMaterialsQueued) {
					deleteInsideMaskMaterialsQueued = false;
					foreach (UnityEngine.Object c in targets)
						EditorDeleteMaskMaterials(c as SkeletonRenderer, SpriteMaskInteraction.VisibleInsideMask);
				}
				if (deleteOutsideMaskMaterialsQueued) {
					deleteOutsideMaskMaterialsQueued = false;
					foreach (UnityEngine.Object c in targets)
						EditorDeleteMaskMaterials(c as SkeletonRenderer, SpriteMaskInteraction.VisibleOutsideMask);
				}
#endif

#if NO_PREFAB_MESH
				if (isInspectingPrefab) {
					foreach (UnityEngine.Object c in targets) {
						SkeletonRenderer component = (SkeletonRenderer)c;
						MeshFilter meshFilter = component.GetComponent<MeshFilter>();
						if (meshFilter != null && meshFilter.sharedMesh != null)
							meshFilter.sharedMesh = null;
					}
				}
#endif
			}

			bool valid = TargetIsValid;

			foreach (UnityEngine.Object o in targets)
				ApplyModifiedMeshParameters(o as SkeletonRenderer);

			// Fields.
			if (multi) {
				using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
					SpineInspectorUtility.PropertyFieldFitLabel(skeletonDataAsset, SkeletonDataAssetLabel);
					if (GUILayout.Button(ReloadButtonString, ReloadButtonStyle, ReloadButtonWidth))
						forceReloadQueued = true;
				}

				if (valid) EditorGUILayout.PropertyField(initialSkinName, SpineInspectorUtility.TempContent("Initial Skin"));

			} else {
				SkeletonRenderer component = (SkeletonRenderer)target;

				using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
					SpineInspectorUtility.PropertyFieldFitLabel(skeletonDataAsset, SkeletonDataAssetLabel);
					if (component.valid) {
						if (GUILayout.Button(ReloadButtonString, ReloadButtonStyle, ReloadButtonWidth))
							forceReloadQueued = true;
					}
				}

				if (component.skeletonDataAsset == null) {
					EditorGUILayout.HelpBox("SkeletonData asset required", MessageType.Warning);
					return;
				}

				if (!SpineEditorUtilities.SkeletonDataAssetIsValid(component.skeletonDataAsset)) {
					EditorGUILayout.HelpBox("SkeletonData asset error. Please check SkeletonData asset.", MessageType.Error);
					return;
				}

				if (valid)
					EditorGUILayout.PropertyField(initialSkinName, SpineInspectorUtility.TempContent("Initial Skin"));

			}

			EditorGUILayout.Space();

			// Sorting Layers
			SpineInspectorUtility.SortingPropertyFields(sortingProperties, applyModifiedProperties: true);

			if (maskInteraction != null) EditorGUILayout.PropertyField(maskInteraction, MaskInteractionLabel);

			if (!valid)
				return;

			string errorMessage = null;
			if (SpineEditorUtilities.Preferences.componentMaterialWarning &&
				MaterialChecks.IsMaterialSetupProblematic((SkeletonRenderer)this.target, ref errorMessage)) {
				EditorGUILayout.HelpBox(errorMessage, MessageType.Error, true);
			}

			// More Render Options...
			using (new SpineInspectorUtility.BoxScope()) {
				EditorGUI.BeginChangeCheck();

				EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight + 5));
				advancedFoldout = EditorGUILayout.Foldout(advancedFoldout, "Advanced");
				if (advancedFoldout) {
					EditorGUILayout.Space();
					if (GUILayout.Button("Debug", EditorStyles.miniButton, GUILayout.Width(65f)))
						SkeletonDebugWindow.Init();
				} else {
					EditorGUILayout.Space();
				}
				EditorGUILayout.EndHorizontal();

				if (advancedFoldout) {

					using (new SpineInspectorUtility.IndentScope()) {
						using (new EditorGUILayout.HorizontalScope()) {
							EditorGUI.BeginChangeCheck();
							SpineInspectorUtility.ToggleLeftLayout(initialFlipX);
							SpineInspectorUtility.ToggleLeftLayout(initialFlipY);
							wasInitParameterChanged |= EditorGUI.EndChangeCheck(); // Value used in the next update.
							EditorGUILayout.Space();
						}
						EditorGUILayout.Space();

						EditorGUILayout.LabelField("Renderer and Update Settings", EditorStyles.boldLabel);
						using (new SpineInspectorUtility.LabelWidthScope()) {
							// Optimization options
							if (updateTiming != null) EditorGUILayout.PropertyField(updateTiming, UpdateTimingLabel);
							if (updateWhenInvisible != null) EditorGUILayout.PropertyField(updateWhenInvisible, UpdateWhenInvisibleLabel);

							if (singleSubmesh != null) EditorGUILayout.PropertyField(singleSubmesh, SingleSubmeshLabel);
#if PER_MATERIAL_PROPERTY_BLOCKS
							if (fixDrawOrder != null) EditorGUILayout.PropertyField(fixDrawOrder, FixDrawOrderLabel);
#endif
							if (immutableTriangles != null) EditorGUILayout.PropertyField(immutableTriangles, ImmubleTrianglesLabel);
							EditorGUILayout.PropertyField(clearStateOnDisable, ClearStateOnDisableLabel);
							EditorGUILayout.Space();

#if HAS_ON_POSTPROCESS_PREFAB
							if (fixPrefabOverrideViaMeshFilter != null) EditorGUILayout.PropertyField(fixPrefabOverrideViaMeshFilter, FixPrefabOverrideViaMeshFilterLabel);
							EditorGUILayout.Space();
#endif
						}

						SeparatorsField(separatorSlotNames);
						EditorGUILayout.Space();

						// Render options
						const float MinZSpacing = -0.1f;
						const float MaxZSpacing = 0f;
						EditorGUILayout.Slider(zSpacing, MinZSpacing, MaxZSpacing, ZSpacingLabel);
						EditorGUILayout.Space();

						using (new SpineInspectorUtility.LabelWidthScope()) {
							EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Vertex Data", SpineInspectorUtility.UnityIcon<MeshFilter>()), EditorStyles.boldLabel);
							if (pmaVertexColors != null) EditorGUILayout.PropertyField(pmaVertexColors, PMAVertexColorsLabel);
							EditorGUILayout.PropertyField(tintBlack, TintBlackLabel);

							// Optional fields. May be disabled in SkeletonRenderer.
							if (normals != null) EditorGUILayout.PropertyField(normals, NormalsLabel);
							if (tangents != null) EditorGUILayout.PropertyField(tangents, TangentsLabel);
						}

#if BUILT_IN_SPRITE_MASK_COMPONENT
						EditorGUILayout.Space();
						if (maskMaterialsNone.arraySize > 0 || maskMaterialsInside.arraySize > 0 || maskMaterialsOutside.arraySize > 0) {
							EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Mask Interaction Materials", SpineInspectorUtility.UnityIcon<SpriteMask>()), EditorStyles.boldLabel);
							bool differentMaskModesSelected = maskInteraction.hasMultipleDifferentValues;
							int activeMaskInteractionValue = differentMaskModesSelected ? -1 : maskInteraction.intValue;

							bool ignoredParam = true;
							MaskMaterialsEditingField(ref setMaskNoneMaterialsQueued, ref ignoredParam, maskMaterialsNone, MaskMaterialsNoneLabel,
														differentMaskModesSelected, allowDelete: false, isActiveMaterial: activeMaskInteractionValue == (int)SpriteMaskInteraction.None);
							MaskMaterialsEditingField(ref setInsideMaskMaterialsQueued, ref deleteInsideMaskMaterialsQueued, maskMaterialsInside, MaskMaterialsInsideLabel,
														differentMaskModesSelected, allowDelete: true, isActiveMaterial: activeMaskInteractionValue == (int)SpriteMaskInteraction.VisibleInsideMask);
							MaskMaterialsEditingField(ref setOutsideMaskMaterialsQueued, ref deleteOutsideMaskMaterialsQueued, maskMaterialsOutside, MaskMaterialsOutsideLabel,
														differentMaskModesSelected, allowDelete: true, isActiveMaterial: activeMaskInteractionValue == (int)SpriteMaskInteraction.VisibleOutsideMask);
						}
#endif
						using (new SpineInspectorUtility.LabelWidthScope()) {
							EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Physics Inheritance", SpineEditorUtilities.Icons.constraintPhysics), EditorStyles.boldLabel);

							using (new GUILayout.HorizontalScope()) {
								EditorGUILayout.LabelField(PhysicsPositionInheritanceFactorLabel, GUILayout.Width(EditorGUIUtility.labelWidth));
								int savedIndentLevel = EditorGUI.indentLevel;
								EditorGUI.indentLevel = 0;
								EditorGUILayout.PropertyField(physicsPositionInheritanceFactor, GUIContent.none, GUILayout.MinWidth(60));
								EditorGUI.indentLevel = savedIndentLevel;
							}
							EditorGUILayout.PropertyField(physicsRotationInheritanceFactor, PhysicsRotationInheritanceFactorLabel);
							EditorGUILayout.PropertyField(physicsMovementRelativeTo, PhysicsMovementRelativeToLabel);
						}

						EditorGUILayout.Space();

						if (valid && !isInspectingPrefab) {
							if (multi) {
								// Support multi-edit SkeletonUtility button.
								//	EditorGUILayout.Space();
								//	bool addSkeletonUtility = GUILayout.Button(buttonContent, GUILayout.Height(30));
								//	foreach (UnityEngine.Object t in targets) {
								//		Component component = t as Component;
								//		if (addSkeletonUtility && component.GetComponent<SkeletonUtility>() == null)
								//			component.gameObject.AddComponent<SkeletonUtility>();
								//	}
							} else {
								Component component = (Component)target;
								if (component.GetComponent<SkeletonUtility>() == null) {
									if (SpineInspectorUtility.CenteredButton(SkeletonUtilityButtonContent, 21, true, 200f))
										component.gameObject.AddComponent<SkeletonUtility>();
								}
							}
						}

						EditorGUILayout.Space();
					}
				}

				if (EditorGUI.EndChangeCheck())
					SceneView.RepaintAll();
			}
		}

		protected void ApplyModifiedMeshParameters (SkeletonRenderer skeletonRenderer) {
			if (skeletonRenderer == null) return;
			if (!skeletonRenderer.valid)
				return;

			if (!isInspectingPrefab) {
				if (wasInitParameterChanged) {
					wasInitParameterChanged = false;
					if (!Application.isPlaying) {
						skeletonRenderer.Initialize(true);
						skeletonRenderer.LateUpdate();
						requireRepaint = true;
					}
				}
			}
		}

		protected void SkeletonRootMotionParameter () {
			SkeletonRootMotionParameter(targets);
		}

		public static void SkeletonRootMotionParameter (Object[] targets) {
			int rootMotionComponentCount = 0;
			foreach (UnityEngine.Object t in targets) {
				Component component = t as Component;
				if (component.GetComponent<SkeletonRootMotion>() != null) {
					++rootMotionComponentCount;
				}
			}
			bool allHaveRootMotion = rootMotionComponentCount == targets.Length;
			bool anyHaveRootMotion = rootMotionComponentCount > 0;

			using (new GUILayout.HorizontalScope()) {
				EditorGUILayout.PrefixLabel("Root Motion");

				if (!allHaveRootMotion) {
					if (GUILayout.Button(SpineInspectorUtility.TempContent("Add Component", Icons.constraintTransform), GUILayout.MaxWidth(130), GUILayout.Height(18))) {
						foreach (UnityEngine.Object t in targets) {
							Component component = t as Component;
							if (component.GetComponent<SkeletonRootMotion>() == null) {
								component.gameObject.AddComponent<SkeletonRootMotion>();
							}
						}
					}
				}
				if (anyHaveRootMotion) {
					if (GUILayout.Button(SpineInspectorUtility.TempContent("Remove Component", Icons.constraintTransform), GUILayout.MaxWidth(140), GUILayout.Height(18))) {
						foreach (UnityEngine.Object t in targets) {
							Component component = t as Component;
							SkeletonRootMotion rootMotionComponent = component.GetComponent<SkeletonRootMotion>();
							if (rootMotionComponent != null) {
								DestroyImmediate(rootMotionComponent);
							}
						}
					}
				}
			}
		}

		public static void SetSeparatorSlotNames (SkeletonRenderer skeletonRenderer, string[] newSlotNames) {
			FieldInfo field = SpineInspectorUtility.GetNonPublicField(typeof(SkeletonRenderer), SeparatorSlotNamesFieldName);
			field.SetValue(skeletonRenderer, newSlotNames);
		}

		public static string[] GetSeparatorSlotNames (SkeletonRenderer skeletonRenderer) {
			FieldInfo field = SpineInspectorUtility.GetNonPublicField(typeof(SkeletonRenderer), SeparatorSlotNamesFieldName);
			return field.GetValue(skeletonRenderer) as string[];
		}

		public static void SeparatorsField (SerializedProperty separatorSlotNames) {
			bool multi = separatorSlotNames.serializedObject.isEditingMultipleObjects;
			bool hasTerminalSlot = false;
			if (!multi) {
				ISkeletonComponent sr = separatorSlotNames.serializedObject.targetObject as ISkeletonComponent;
				Skeleton skeleton = sr.Skeleton;
				int lastSlot = skeleton.Slots.Count - 1;
				if (skeleton != null) {
					for (int i = 0, n = separatorSlotNames.arraySize; i < n; i++) {
						string slotName = separatorSlotNames.GetArrayElementAtIndex(i).stringValue;
						SlotData slot = skeleton.Data.FindSlot(slotName);
						int index = slot != null ? slot.Index : -1;
						if (index == 0 || index == lastSlot) {
							hasTerminalSlot = true;
							break;
						}
					}
				}
			}

			string terminalSlotWarning = hasTerminalSlot ? " (!)" : "";

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
				const string SeparatorsDescription = "Stored names of slots where the Skeleton's render will be split into different batches. This is used by separate components that split the render into different MeshRenderers or GameObjects.";
				if (separatorSlotNames.isExpanded) {
					EditorGUILayout.PropertyField(separatorSlotNames, SpineInspectorUtility.TempContent(separatorSlotNames.displayName + terminalSlotWarning, Icons.slotRoot, SeparatorsDescription), true);
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("+", GUILayout.MaxWidth(28f), GUILayout.MaxHeight(15f))) {
						separatorSlotNames.arraySize++;
					}
					GUILayout.EndHorizontal();

					EditorGUILayout.Space();
				} else
					EditorGUILayout.PropertyField(separatorSlotNames, new GUIContent(separatorSlotNames.displayName + string.Format("{0} [{1}]", terminalSlotWarning, separatorSlotNames.arraySize), SeparatorsDescription), true);
			}
		}

		public void MaskMaterialsEditingField (ref bool wasSetRequested, ref bool wasDeleteRequested,
													SerializedProperty maskMaterials, GUIContent label,
													bool differentMaskModesSelected, bool allowDelete, bool isActiveMaterial) {
			using (new EditorGUILayout.HorizontalScope()) {

				EditorGUILayout.LabelField(label, isActiveMaterial ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.MinWidth(80f), GUILayout.MaxWidth(140));
				EditorGUILayout.LabelField(maskMaterials.hasMultipleDifferentValues ? "-" : maskMaterials.arraySize.ToString(), EditorStyles.miniLabel, GUILayout.Width(42f));

				bool enableSetButton = differentMaskModesSelected || maskMaterials.arraySize == 0;
				bool enableClearButtons = differentMaskModesSelected || (maskMaterials.arraySize != 0 && !isActiveMaterial);

				EditorGUI.BeginDisabledGroup(!enableSetButton);
				if (GUILayout.Button(SetMaterialButtonLabel, EditorStyles.miniButtonLeft, GUILayout.Width(46f))) {
					wasSetRequested = true;
				}
				EditorGUI.EndDisabledGroup();

				EditorGUI.BeginDisabledGroup(!enableClearButtons);
				{
					if (GUILayout.Button(ClearMaterialButtonLabel, allowDelete ? EditorStyles.miniButtonMid : EditorStyles.miniButtonRight, GUILayout.Width(46f))) {
						maskMaterials.ClearArray();
					} else if (allowDelete && GUILayout.Button(DeleteMaterialButtonLabel, EditorStyles.miniButtonRight, GUILayout.Width(46f))) {
						wasDeleteRequested = true;
					}
					if (!allowDelete)
						GUILayout.Space(46f);
				}
				EditorGUI.EndDisabledGroup();
			}
		}

		void HandleSkinChange () {
			if (!Application.isPlaying && Event.current.type == EventType.Layout && !initialSkinName.hasMultipleDifferentValues) {
				bool mismatchDetected = false;
				string newSkinName = initialSkinName.stringValue;
				foreach (UnityEngine.Object o in targets) {
					mismatchDetected |= UpdateIfSkinMismatch((SkeletonRenderer)o, newSkinName);
				}

				if (mismatchDetected) {
					mismatchDetected = false;
					SceneView.RepaintAll();
				}
			}
		}

		static bool UpdateIfSkinMismatch (SkeletonRenderer skeletonRenderer, string componentSkinName) {
			if (!skeletonRenderer.valid || skeletonRenderer.EditorSkipSkinSync) return false;

			Skin skin = skeletonRenderer.Skeleton.Skin;
			string skeletonSkinName = skin != null ? skin.Name : null;
			bool defaultCase = skin == null && string.IsNullOrEmpty(componentSkinName);
			bool fieldMatchesSkin = defaultCase || string.Equals(componentSkinName, skeletonSkinName, System.StringComparison.Ordinal);

			if (!fieldMatchesSkin) {
				Skin skinToSet = string.IsNullOrEmpty(componentSkinName) ? null : skeletonRenderer.Skeleton.Data.FindSkin(componentSkinName);
				skeletonRenderer.Skeleton.SetSkin(skinToSet);
				skeletonRenderer.Skeleton.SetSlotsToSetupPose();

				// Note: the UpdateIfSkinMismatch concept shall be replaced with e.g. an OnValidate based
				// solution or in a separate commit. The current solution does not repaint the Game view because
				// it is first applying values and in the next editor pass is calling this skin-changing method.
				if (skeletonRenderer is SkeletonAnimation)
					((SkeletonAnimation)skeletonRenderer).Update(0f);
				else if (skeletonRenderer is SkeletonMecanim)
					((SkeletonMecanim)skeletonRenderer).Update();

				skeletonRenderer.LateUpdate();
				return true;
			}
			return false;
		}

		bool AreAnyMaskMaterialsMissing () {
#if BUILT_IN_SPRITE_MASK_COMPONENT
			foreach (UnityEngine.Object o in targets) {
				SkeletonRenderer component = (SkeletonRenderer)o;
				if (!component.valid)
					continue;
				if (SpineMaskUtilities.AreMaskMaterialsMissing(component))
					return true;
			}
#endif
			return false;
		}

#if BUILT_IN_SPRITE_MASK_COMPONENT
		static void EditorSetMaskMaterials (SkeletonRenderer component, SpriteMaskInteraction maskType) {
			if (component == null) return;
			if (!SpineEditorUtilities.SkeletonDataAssetIsValid(component.SkeletonDataAsset)) return;
			SpineMaskUtilities.EditorInitMaskMaterials(component, component.maskMaterials, maskType);
		}

		static void EditorDeleteMaskMaterials (SkeletonRenderer component, SpriteMaskInteraction maskType) {
			if (component == null) return;
			if (!SpineEditorUtilities.SkeletonDataAssetIsValid(component.SkeletonDataAsset)) return;
			SpineMaskUtilities.EditorDeleteMaskMaterials(component.maskMaterials, maskType);
		}
#endif
	}
}
