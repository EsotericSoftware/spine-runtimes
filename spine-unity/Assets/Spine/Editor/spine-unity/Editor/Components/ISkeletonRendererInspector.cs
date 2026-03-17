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

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#else
#define NO_PREFAB_MESH
#endif

#if UNITY_2018_1_OR_NEWER
#define PER_MATERIAL_PROPERTY_BLOCKS
#endif

using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	using Event = UnityEngine.Event;
	using Icons = SpineEditorUtilities.Icons;

	public class ISkeletonRendererInspector : UnityEditor.Editor {
		public static bool advancedFoldout;
		protected bool loadingFailed = false;

		const string SeparatorSlotNamesFieldName = "separatorSlotNames";

		protected SerializedProperty skeletonDataAsset, initialSkinName;
		protected SerializedProperty initialFlipX, initialFlipY;
		protected SerializedProperty updateWhenInvisible, separatorSlotNames, enableSeparatorSlots;
		protected SerializedProperty clearStateOnDisable, fixDrawOrder;
		protected SerializedProperty useClipping, zSpacing, immutableTriangles;
		protected SerializedProperty threadedMeshGeneration;
		// Vertex Data parameters
		protected SerializedProperty tintBlack, canvasGroupCompatible, pmaVertexColors, addNormals, calculateTangents;
		protected SerializedProperty physicsPositionInheritanceFactor, physicsRotationInheritanceFactor, physicsMovementRelativeTo;

		protected bool isInspectingPrefab;
		protected bool forceReloadQueued = false;

		private GUIContent SkeletonDataAssetLabel, SkeletonUtilityButtonContent;

		protected readonly GUIContent ClearStateOnDisableLabel = new GUIContent(
			"Clear State On Disable", "Use this if you are pooling or enabling/disabling your Spine GameObject.");

		protected readonly GUIContent UseClippingLabel = new GUIContent("Use Clipping",
			"When disabled, clipping attachments are ignored. This may be used to save performance.");
		protected readonly GUIContent ZSpacingLabel = new GUIContent("Z Spacing",
			"A value other than 0 adds a space between each rendered attachment to prevent Z Fighting when using shaders" +
			" that read or write to the depth buffer. Large values may cause unwanted parallax and spaces depending on " +
			"camera setup.");
		protected readonly GUIContent ThreadedMeshGenerationLabel = new GUIContent("Use Threading",
			"When enabled, mesh generation is performed on multiple threads in parallel.");

		protected readonly GUIContent TintBlackLabel = new GUIContent("Tint Black (!)",
			"Adds black tint vertex data to the mesh as UV2 and UV3. Black tinting requires that the shader interpret " +
			"UV2 and UV3 as black tint colors for this effect to work. You may then want to use the " +
			"[Spine/SkeletonGraphic Tint Black] shader.");
		protected readonly GUIContent CanvasGroupCompatibleLabel = new GUIContent("CanvasGroup Compatible",
			"Enable when using SkeletonGraphic under a CanvasGroup. " +
			"When enabled, PMA Vertex Color alpha value is stored at uv2.g instead of color.a to capture " +
			"CanvasGroup modifying color.a. Also helps to detect correct parameter setting combinations.");
		protected readonly GUIContent PMAVertexColorsLabel = new GUIContent("PMA Vertex Colors",
			"Use this if you are using the default Spine/Skeleton shader or any premultiply-alpha shader.");
		protected readonly GUIContent AddNormalsLabel = new GUIContent("Add Normals",
			"Use this if your shader requires vertex normals. A more efficient solution for 2D setups is to modify the " +
			"shader to assume a single normal value for the whole mesh.");
		protected readonly GUIContent CalculateTangentsLabel = new GUIContent("Solve Tangents",
			"Calculates the tangents per frame. Use this if you are using lit shaders (usually with normal maps) that " +
			"require vertex tangents.");

		protected readonly GUIContent ImmutableTrianglesLabel = new GUIContent("Immutable Triangles",
			"Enable to optimize rendering for skeletons that never change attachment visibility");


		private static GUIContent EnableSeparatorSlotsLabel;
		private GUIContent UpdateWhenInvisibleLabel, FixDrawOrderLabel;

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

		protected SerializedProperty meshSettings;

		const string ReloadButtonString = "Reload";
		static GUILayoutOption reloadButtonWidth;
		static GUILayoutOption ReloadButtonWidth { get { return reloadButtonWidth = reloadButtonWidth ?? GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(ReloadButtonString)).x + 20); } }
		static GUIStyle ReloadButtonStyle { get { return EditorStyles.miniButton; } }

		protected virtual bool TargetIsValid {
			get {
				foreach (var o in targets) {
					var component = (ISkeletonRenderer)o;
					if (!component.IsValid)
						return false;
				}
				return true;
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

			UpdateWhenInvisibleLabel = new GUIContent("Update When Invisible", "Update mode used when the MeshRenderer becomes invisible. Update mode is automatically reset to UpdateMode.FullUpdate when the mesh becomes visible again.");
			FixDrawOrderLabel = new GUIContent("Fix Draw Order", "Applies only when 3+ submeshes are used (2+ materials with alternating order, e.g. \"A B A\"). If true, GPU instancing will be disabled at all materials and MaterialPropertyBlocks are assigned at each material to prevent aggressive batching of submeshes by e.g. the LWRP renderer, leading to incorrect draw order (e.g. \"A1 B A2\" changed to \"A1A2 B\"). You can disable this parameter when everything is drawn correctly to save the additional performance cost. Note: the GPU instancing setting will remain disabled at affected material assets after exiting play mode, you have to enable it manually if you accidentally enabled this parameter.");

			skeletonDataAsset = serializedObject.FindProperty("skeletonDataAsset");
			initialSkinName = serializedObject.FindProperty("initialSkinName");
			initialFlipX = serializedObject.FindProperty("initialFlipX");
			initialFlipY = serializedObject.FindProperty("initialFlipY");

			clearStateOnDisable = serializedObject.FindProperty("clearStateOnDisable");
			updateWhenInvisible = serializedObject.FindProperty("updateWhenInvisible");
			fixDrawOrder = serializedObject.FindProperty("fixDrawOrder");

			meshSettings = serializedObject.FindProperty("meshSettings");
			meshSettings.isExpanded = SkeletonRendererInspector.advancedFoldout;

			useClipping = meshSettings.FindPropertyRelative("useClipping");
			zSpacing = meshSettings.FindPropertyRelative("zSpacing");
			tintBlack = meshSettings.FindPropertyRelative("tintBlack");
			canvasGroupCompatible = meshSettings.FindPropertyRelative("canvasGroupCompatible");
			pmaVertexColors = meshSettings.FindPropertyRelative("pmaVertexColors");
			addNormals = meshSettings.FindPropertyRelative("addNormals");
			calculateTangents = meshSettings.FindPropertyRelative("calculateTangents");
			immutableTriangles = meshSettings.FindPropertyRelative("immutableTriangles");

			threadedMeshGeneration = serializedObject.FindProperty("threadedMeshGeneration");
			separatorSlotNames = serializedObject.FindProperty("separatorSlotNames");
			separatorSlotNames.isExpanded = true;
			enableSeparatorSlots = serializedObject.FindProperty("enableSeparatorSlots");

			physicsPositionInheritanceFactor = serializedObject.FindProperty("physicsPositionInheritanceFactor");
			physicsRotationInheritanceFactor = serializedObject.FindProperty("physicsRotationInheritanceFactor");
			physicsMovementRelativeTo = serializedObject.FindProperty("physicsMovementRelativeTo");
		}

		public virtual void OnSceneGUI () {
			var skeletonRenderer = (ISkeletonRenderer)target;
			if (loadingFailed)
				return;

			var skeleton = skeletonRenderer.Skeleton;
			if (skeleton == null) {
				loadingFailed = true;
				return;
			}
			var transform = skeletonRenderer.Component.transform;
			if (skeleton == null) return;

			SpineHandles.DrawBones(transform, skeleton, skeletonRenderer.MeshScale);
		}

		override public void OnInspectorGUI () {
			bool multi = serializedObject.isEditingMultipleObjects;
			DrawInspectorGUI(multi);
			serializedObject.ApplyModifiedProperties();
		}

		protected virtual void InspectorDrawPreparation () { }
		protected virtual void FirstPropertyFields () { }
		protected virtual void MaterialWarningsBox () { }
		protected virtual void AdditionalSeparatorSlotProperties () { }
		protected virtual void VertexDataProperties () { }
		protected virtual void AfterAdvancedPropertyFields () { }

		protected virtual void RendererProperties () {
			using (new SpineInspectorUtility.LabelWidthScope()) {
				// Optimization options
				if (updateWhenInvisible != null) EditorGUILayout.PropertyField(updateWhenInvisible, UpdateWhenInvisibleLabel);

#if PER_MATERIAL_PROPERTY_BLOCKS
				if (fixDrawOrder != null) EditorGUILayout.PropertyField(fixDrawOrder, FixDrawOrderLabel);
#endif
				if (immutableTriangles != null) EditorGUILayout.PropertyField(immutableTriangles, ImmutableTrianglesLabel);
				EditorGUILayout.PropertyField(clearStateOnDisable, ClearStateOnDisableLabel);
				EditorGUILayout.Space();
			}

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
				SeparatorSlotProperties(separatorSlotNames, enableSeparatorSlots);
				AdditionalSeparatorSlotProperties();
			}

			EditorGUILayout.Space();

			// Render options
			EditorGUILayout.PropertyField(useClipping, UseClippingLabel);
			const float MinZSpacing = -0.1f;
			const float MaxZSpacing = 0f;
			EditorGUILayout.Slider(zSpacing, MinZSpacing, MaxZSpacing, ZSpacingLabel);
		}

		protected virtual void PhysicsProperties () {
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

		protected virtual void AdvancedPropertyFields () {
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Renderer Settings", EditorStyles.boldLabel);
			RendererProperties();
			EditorGUILayout.Space();

			if (threadedMeshGeneration != null) {
				EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Threaded Mesh Generation", SpineEditorUtilities.Icons.subMeshRenderer), EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(threadedMeshGeneration, ThreadedMeshGenerationLabel);
				EditorGUILayout.Space();
			}

			using (new SpineInspectorUtility.LabelWidthScope()) {
				EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Vertex Data", SpineInspectorUtility.UnityIcon<MeshFilter>()), EditorStyles.boldLabel);
				VertexDataProperties();
			}

			EditorGUILayout.Space();
			using (new SpineInspectorUtility.LabelWidthScope()) {
				EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Physics Inheritance", SpineEditorUtilities.Icons.constraintPhysics), EditorStyles.boldLabel);
				PhysicsProperties();

			}
		}

		protected virtual void DrawInspectorGUI (bool multi) {
			// Initialize.
			if (Event.current.type == EventType.Layout) {
				if (forceReloadQueued) {
					forceReloadQueued = false;
					foreach (var c in targets) {
						SpineEditorUtilities.ReloadSkeletonDataAssetAndComponent(c as ISkeletonRenderer);
					}
				} else {
					foreach (var c in targets) {
						var component = c as ISkeletonRenderer;
						if (!component.IsValid) {
							SpineEditorUtilities.ReinitializeComponent(component);
						}
					}
				}

				InspectorDrawPreparation();

#if NO_PREFAB_MESH
				if (isInspectingPrefab) {
					foreach (var c in targets) {
						var component = c as SkeletonRenderer;
						if (component != null) {
							MeshFilter meshFilter = component.GetComponent<MeshFilter>();
							if (meshFilter != null && meshFilter.sharedMesh != null)
								meshFilter.sharedMesh = null;
						}
					}
				}
#endif
			}

			bool valid = TargetIsValid;

			// Fields.
			bool skeletonAssetValid = CommonSkeletonAssetProperties(multi);
			if (!skeletonAssetValid || !valid)
				return;

			EditorGUILayout.PropertyField(initialSkinName, SpineInspectorUtility.TempContent("Initial Skin"));

			using (new EditorGUILayout.HorizontalScope()) {
				SpineInspectorUtility.ToggleLeftLayout(initialFlipX);
				SpineInspectorUtility.ToggleLeftLayout(initialFlipY);
				EditorGUILayout.Space();
			}

			FirstPropertyFields();

			MaterialWarningsBox();

			// More Render Options...
			using (new SpineInspectorUtility.BoxScope()) {
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

						AdvancedPropertyFields();

						EditorGUILayout.Space();

						if (valid && !isInspectingPrefab) {
							if (multi) {
								// Support multi-edit SkeletonUtility button.
								//	EditorGUILayout.Space();
								//	bool addSkeletonUtility = GUILayout.Button(buttonContent, GUILayout.Height(30));
								//	foreach (var t in targets) {
								//		var component = t as Component;
								//		if (addSkeletonUtility && component.GetComponent<SkeletonUtility>() == null)
								//			component.gameObject.AddComponent<SkeletonUtility>();
								//	}
							} else {
								var component = (Component)target;
								if (component.GetComponent<SkeletonUtility>() == null) {
									if (SpineInspectorUtility.CenteredButton(SkeletonUtilityButtonContent, 21, true, 200f))
										component.gameObject.AddComponent<SkeletonUtility>();
								}
							}
						}
						EditorGUILayout.Space();
					}
				}
			}

			AfterAdvancedPropertyFields();
		}

		/// <returns>True when the SkeletonDataAsset is valid, false otherwise.</returns>
		protected bool CommonSkeletonAssetProperties (bool multi) {
			if (multi) {
				using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
					SpineInspectorUtility.PropertyFieldFitLabel(skeletonDataAsset, SkeletonDataAssetLabel);
					if (GUILayout.Button(ReloadButtonString, ReloadButtonStyle, ReloadButtonWidth))
						forceReloadQueued = true;
				}
			} else {
				var component = (ISkeletonRenderer)target;

				using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
					SpineInspectorUtility.PropertyFieldFitLabel(skeletonDataAsset, SkeletonDataAssetLabel);
					if (component.IsValid) {
						if (GUILayout.Button(ReloadButtonString, ReloadButtonStyle, ReloadButtonWidth))
							forceReloadQueued = true;
					}
				}

				if (component.SkeletonDataAsset == null) {
					EditorGUILayout.HelpBox("Skeleton Data Asset required", MessageType.Warning);
					return false;
				}

				if (!SpineEditorUtilities.SkeletonDataAssetIsValid(component.SkeletonDataAsset)) {
					EditorGUILayout.HelpBox("Skeleton Data Asset error. Please check Skeleton Data Asset.", MessageType.Error);
					return false;
				}
			}
			return true;
		}

		public static void SetSeparatorSlotNames (SkeletonRenderer skeletonRenderer, string[] newSlotNames) {
			var field = SpineInspectorUtility.GetNonPublicField(typeof(SkeletonRenderer), SeparatorSlotNamesFieldName);
			field.SetValue(skeletonRenderer, newSlotNames);
		}

		public static string[] GetSeparatorSlotNames (SkeletonRenderer skeletonRenderer) {
			var field = SpineInspectorUtility.GetNonPublicField(typeof(SkeletonRenderer), SeparatorSlotNamesFieldName);
			return field.GetValue(skeletonRenderer) as string[];
		}

		public static string TerminalSlotWarningString (SerializedProperty separatorSlotNames) {
			bool multi = separatorSlotNames.serializedObject.isEditingMultipleObjects;
			bool hasTerminalSlot = false;
			if (!multi) {
				var sr = separatorSlotNames.serializedObject.targetObject as ISkeletonComponent;
				var skeleton = sr.Skeleton;
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

			return hasTerminalSlot ? " (!)" : "";
		}

		public static void SeparatorSlotProperties (SerializedProperty separatorSlotNames,
			SerializedProperty enableSeparatorSlots) {

			string terminalSlotWarning = TerminalSlotWarningString(separatorSlotNames);
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

			if (EnableSeparatorSlotsLabel == null)
				EnableSeparatorSlotsLabel = new GUIContent("Enable Separation", "Whether to enable separation at the above separator slots.");

			EditorGUILayout.PropertyField(enableSeparatorSlots, EnableSeparatorSlotsLabel);
		}
	}
}
