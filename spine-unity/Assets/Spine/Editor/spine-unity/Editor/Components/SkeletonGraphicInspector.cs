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

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

#if UNITY_2018_2_OR_NEWER
#define HAS_CULL_TRANSPARENT_MESH
#endif

#if UNITY_2017_2_OR_NEWER
#define NEWPLAYMODECALLBACKS
#endif

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	using Icons = SpineEditorUtilities.Icons;

	[CustomEditor(typeof(SkeletonGraphic))]
	[CanEditMultipleObjects]
	public class SkeletonGraphicInspector : UnityEditor.Editor {

		const string SeparatorSlotNamesFieldName = "separatorSlotNames";
		const string ReloadButtonString = "Reload";
		protected GUIContent SkeletonDataAssetLabel, UpdateTimingLabel;
		static GUILayoutOption reloadButtonWidth;
		static GUILayoutOption ReloadButtonWidth { get { return reloadButtonWidth = reloadButtonWidth ?? GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(ReloadButtonString)).x + 20); } }
		static GUIStyle ReloadButtonStyle { get { return EditorStyles.miniButton; } }

		SerializedProperty material, color;
		SerializedProperty additiveMaterial, multiplyMaterial, screenMaterial;
		SerializedProperty skeletonDataAsset, initialSkinName;
		SerializedProperty startingAnimation, startingLoop, timeScale, freeze,
			updateTiming, updateWhenInvisible, unscaledTime, tintBlack, layoutScaleMode, editReferenceRect;
		SerializedProperty initialFlipX, initialFlipY;
		SerializedProperty meshGeneratorSettings;
		SerializedProperty allowMultipleCanvasRenderers, separatorSlotNames, enableSeparatorSlots,
			updateSeparatorPartLocation, updateSeparatorPartScale;
		SerializedProperty raycastTarget, maskable;

		readonly GUIContent UnscaledTimeLabel = new GUIContent("Unscaled Time",
			"If enabled, AnimationState uses unscaled game time (Time.unscaledDeltaTime), " +
				"running animations independent of e.g. game pause (Time.timeScale). " +
				"Instance SkeletonAnimation.timeScale will still be applied.");

		SkeletonGraphic thisSkeletonGraphic;
		protected bool isInspectingPrefab;
		protected bool slotsReapplyRequired = false;
		protected bool forceReloadQueued = false;

		protected bool TargetIsValid {
			get {
				if (serializedObject.isEditingMultipleObjects) {
					foreach (UnityEngine.Object c in targets) {
						SkeletonGraphic component = (SkeletonGraphic)c;
						if (!component.IsValid)
							return false;
					}
					return true;
				} else {
					SkeletonGraphic component = (SkeletonGraphic)target;
					return component.IsValid;
				}
			}
		}

		void OnEnable () {
#if NEW_PREFAB_SYSTEM
			isInspectingPrefab = false;
#else
			isInspectingPrefab = (PrefabUtility.GetPrefabType(target) == PrefabType.Prefab);
#endif
			SpineEditorUtilities.ConfirmInitialization();

			// Labels
			SkeletonDataAssetLabel = new GUIContent("SkeletonData Asset", Icons.spine);
			UpdateTimingLabel = new GUIContent("Animation Update", "Whether to update the animation in normal Update (the default), physics step FixedUpdate, or manually via a user call.");

			SerializedObject so = this.serializedObject;
			thisSkeletonGraphic = target as SkeletonGraphic;

			// MaskableGraphic
			material = so.FindProperty("m_Material");
			color = so.FindProperty("m_Color");
			raycastTarget = so.FindProperty("m_RaycastTarget");
			maskable = so.FindProperty("m_Maskable");

			// SkeletonRenderer
			additiveMaterial = so.FindProperty("additiveMaterial");
			multiplyMaterial = so.FindProperty("multiplyMaterial");
			screenMaterial = so.FindProperty("screenMaterial");

			skeletonDataAsset = so.FindProperty("skeletonDataAsset");
			initialSkinName = so.FindProperty("initialSkinName");

			initialFlipX = so.FindProperty("initialFlipX");
			initialFlipY = so.FindProperty("initialFlipY");

			// SkeletonAnimation
			startingAnimation = so.FindProperty("startingAnimation");
			startingLoop = so.FindProperty("startingLoop");
			timeScale = so.FindProperty("timeScale");
			unscaledTime = so.FindProperty("unscaledTime");
			freeze = so.FindProperty("freeze");
			updateTiming = so.FindProperty("updateTiming");
			updateWhenInvisible = so.FindProperty("updateWhenInvisible");
			layoutScaleMode = so.FindProperty("layoutScaleMode");
			editReferenceRect = so.FindProperty("editReferenceRect");

			meshGeneratorSettings = so.FindProperty("meshGenerator").FindPropertyRelative("settings");
			meshGeneratorSettings.isExpanded = SkeletonRendererInspector.advancedFoldout;

			allowMultipleCanvasRenderers = so.FindProperty("allowMultipleCanvasRenderers");
			updateSeparatorPartLocation = so.FindProperty("updateSeparatorPartLocation");
			updateSeparatorPartScale = so.FindProperty("updateSeparatorPartScale");
			enableSeparatorSlots = so.FindProperty("enableSeparatorSlots");

			separatorSlotNames = so.FindProperty("separatorSlotNames");
			separatorSlotNames.isExpanded = true;

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
				SkeletonGraphic component = (SkeletonGraphic)c;
				component.EditReferenceRect = false;
			}
		}

		public override void OnInspectorGUI () {

			if (UnityEngine.Event.current.type == EventType.Layout) {
				if (forceReloadQueued) {
					forceReloadQueued = false;
					foreach (UnityEngine.Object c in targets) {
						SpineEditorUtilities.ReloadSkeletonDataAssetAndComponent(c as SkeletonGraphic);
					}
				} else {
					foreach (UnityEngine.Object c in targets) {
						SkeletonGraphic component = c as SkeletonGraphic;
						if (!component.IsValid) {
							SpineEditorUtilities.ReinitializeComponent(component);
							if (!component.IsValid) continue;
						}
					}
				}
			}

			bool wasChanged = false;
			EditorGUI.BeginChangeCheck();

			using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
				SpineInspectorUtility.PropertyFieldFitLabel(skeletonDataAsset, SkeletonDataAssetLabel);
				if (GUILayout.Button(ReloadButtonString, ReloadButtonStyle, ReloadButtonWidth))
					forceReloadQueued = true;
			}

			if (thisSkeletonGraphic.skeletonDataAsset == null) {
				EditorGUILayout.HelpBox("You need to assign a SkeletonData asset first.", MessageType.Info);
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
				return;
			}
			if (!SpineEditorUtilities.SkeletonDataAssetIsValid(thisSkeletonGraphic.skeletonDataAsset)) {
				EditorGUILayout.HelpBox("SkeletonData asset error. Please check SkeletonData asset.", MessageType.Error);
				return;
			}

			EditorGUILayout.PropertyField(material);
			EditorGUILayout.PropertyField(color);

			string errorMessage = null;
			if (SpineEditorUtilities.Preferences.componentMaterialWarning &&
				MaterialChecks.IsMaterialSetupProblematic(thisSkeletonGraphic, ref errorMessage)) {
				EditorGUILayout.HelpBox(errorMessage, MessageType.Error, true);
			}

			bool isSingleRendererOnly = (!allowMultipleCanvasRenderers.hasMultipleDifferentValues && allowMultipleCanvasRenderers.boolValue == false);
			bool isSeparationEnabledButNotMultipleRenderers =
				 isSingleRendererOnly && (!enableSeparatorSlots.hasMultipleDifferentValues && enableSeparatorSlots.boolValue == true);
			bool meshRendersIncorrectlyWithSingleRenderer =
				isSingleRendererOnly && SkeletonHasMultipleSubmeshes();

			if (isSeparationEnabledButNotMultipleRenderers || meshRendersIncorrectlyWithSingleRenderer)
				meshGeneratorSettings.isExpanded = true;

			using (new SpineInspectorUtility.BoxScope()) {

				EditorGUILayout.PropertyField(meshGeneratorSettings, SpineInspectorUtility.TempContent("Advanced..."), includeChildren: true);
				SkeletonRendererInspector.advancedFoldout = meshGeneratorSettings.isExpanded;

				if (meshGeneratorSettings.isExpanded) {
					EditorGUILayout.Space();
					using (new SpineInspectorUtility.IndentScope()) {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.PropertyField(allowMultipleCanvasRenderers, SpineInspectorUtility.TempContent("Multiple CanvasRenderers"));

						if (GUILayout.Button(new GUIContent("Trim Renderers", "Remove currently unused CanvasRenderer GameObjects. These will be regenerated whenever needed."),
							EditorStyles.miniButton, GUILayout.Width(100f))) {

							foreach (UnityEngine.Object skeletonGraphic in targets) {
								((SkeletonGraphic)skeletonGraphic).TrimRenderers();
							}
						}
						EditorGUILayout.EndHorizontal();

						BlendModeMaterials blendModeMaterials = thisSkeletonGraphic.skeletonDataAsset.blendModeMaterials;
						if (allowMultipleCanvasRenderers.boolValue == true && blendModeMaterials.RequiresBlendModeMaterials) {
							using (new SpineInspectorUtility.IndentScope()) {
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField("Blend Mode Materials", EditorStyles.boldLabel);

								if (GUILayout.Button(new GUIContent("Assign Default", "Assign default Blend Mode Materials."),
									EditorStyles.miniButton, GUILayout.Width(100f))) {
									AssignDefaultBlendModeMaterials();
								}
								EditorGUILayout.EndHorizontal();

								bool usesAdditiveMaterial = blendModeMaterials.applyAdditiveMaterial;
								bool pmaVertexColors = thisSkeletonGraphic.MeshGenerator.settings.pmaVertexColors;
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

						EditorGUILayout.PropertyField(updateTiming, UpdateTimingLabel);
						EditorGUILayout.PropertyField(updateWhenInvisible);

						// warning box
						if (isSeparationEnabledButNotMultipleRenderers) {
							using (new SpineInspectorUtility.BoxScope()) {
								meshGeneratorSettings.isExpanded = true;
								EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("'Multiple Canvas Renderers' must be enabled\nwhen 'Enable Separation' is enabled.", Icons.warning), GUILayout.Height(42), GUILayout.Width(340));
							}
						} else if (meshRendersIncorrectlyWithSingleRenderer) {
							using (new SpineInspectorUtility.BoxScope()) {
								meshGeneratorSettings.isExpanded = true;
								EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("This mesh uses multiple atlas pages or blend modes.\n" +
																							"You need to enable 'Multiple Canvas Renderers'\n" +
																							"for correct rendering. Consider packing\n" +
																							"attachments to a single atlas page if possible.", Icons.warning), GUILayout.Height(60), GUILayout.Width(380));
							}
						}
					}

					EditorGUILayout.Space();
					SeparatorsField(separatorSlotNames, enableSeparatorSlots, updateSeparatorPartLocation, updateSeparatorPartScale);
				}
			}

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(initialSkinName);
			{
				Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);
				EditorGUI.PrefixLabel(rect, SpineInspectorUtility.TempContent("Initial Flip"));
				rect.x += EditorGUIUtility.labelWidth;
				rect.width = 30f;
				SpineInspectorUtility.ToggleLeft(rect, initialFlipX, SpineInspectorUtility.TempContent("X", tooltip: "initialFlipX"));
				rect.x += 35f;
				SpineInspectorUtility.ToggleLeft(rect, initialFlipY, SpineInspectorUtility.TempContent("Y", tooltip: "initialFlipY"));
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(startingAnimation);
			EditorGUILayout.PropertyField(startingLoop);
			EditorGUILayout.PropertyField(timeScale);
			EditorGUILayout.PropertyField(unscaledTime, UnscaledTimeLabel);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(freeze);
			EditorGUILayout.Space();
			SkeletonRendererInspector.SkeletonRootMotionParameter(targets);
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
					foreach (UnityEngine.Object skeletonGraphic in targets) {
						MatchRectTransformWithBounds((SkeletonGraphic)skeletonGraphic);
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			if (TargetIsValid && !isInspectingPrefab) {
				EditorGUILayout.Space();
				if (SpineInspectorUtility.CenteredButton(new GUIContent("Add Skeleton Utility", Icons.skeletonUtility), 21, true, 200f))
					foreach (UnityEngine.Object t in targets) {
						Component component = t as Component;
						if (component.GetComponent<SkeletonUtility>() == null) {
							component.gameObject.AddComponent<SkeletonUtility>();
						}
					}
			}

			wasChanged |= EditorGUI.EndChangeCheck();
			if (wasChanged) {
				serializedObject.ApplyModifiedProperties();
				slotsReapplyRequired = true;
			}

			if (slotsReapplyRequired && UnityEngine.Event.current.type == EventType.Repaint) {
				foreach (UnityEngine.Object target in targets) {
					SkeletonGraphic skeletonGraphic = (SkeletonGraphic)target;
					skeletonGraphic.ReapplySeparatorSlotNames();
					skeletonGraphic.LateUpdate();
					SceneView.RepaintAll();
				}
				slotsReapplyRequired = false;
			}
		}

		protected bool SkeletonHasMultipleSubmeshes () {
			foreach (UnityEngine.Object target in targets) {
				SkeletonGraphic skeletonGraphic = (SkeletonGraphic)target;
				if (skeletonGraphic.HasMultipleSubmeshInstructions())
					return true;
			}
			return false;
		}

		protected void OnSceneGUI () {
			SkeletonGraphic skeletonGraphic = (SkeletonGraphic)target;
			if (skeletonGraphic.EditReferenceRect) {
				SpineHandles.DrawRectTransformRect(skeletonGraphic, Color.gray);
				SpineHandles.DrawReferenceRect(skeletonGraphic, Color.green);
			} else {
				SpineHandles.DrawReferenceRect(skeletonGraphic, Color.blue);
			}


		}

		protected void AssignDefaultBlendModeMaterials () {
			foreach (UnityEngine.Object target in targets) {
				SkeletonGraphic skeletonGraphic = (SkeletonGraphic)target;
				skeletonGraphic.additiveMaterial = DefaultSkeletonGraphicAdditiveMaterial;
				skeletonGraphic.multiplyMaterial = DefaultSkeletonGraphicMultiplyMaterial;
				skeletonGraphic.screenMaterial = DefaultSkeletonGraphicScreenMaterial;
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

		public static void SeparatorsField (SerializedProperty separatorSlotNames, SerializedProperty enableSeparatorSlots,
			SerializedProperty updateSeparatorPartLocation, SerializedProperty updateSeparatorPartScale) {

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
				} else
					EditorGUILayout.PropertyField(separatorSlotNames, new GUIContent(separatorSlotNames.displayName + string.Format("{0} [{1}]", terminalSlotWarning, separatorSlotNames.arraySize), SeparatorsDescription), true);

				EditorGUILayout.PropertyField(enableSeparatorSlots, SpineInspectorUtility.TempContent("Enable Separation", tooltip: "Whether to enable separation at the above separator slots."));
				EditorGUILayout.PropertyField(updateSeparatorPartLocation, SpineInspectorUtility.TempContent("Update Part Location", tooltip: "Update separator part GameObject location to match the position of the SkeletonGraphic. This can be helpful when re-parenting parts to a different GameObject."));
				EditorGUILayout.PropertyField(updateSeparatorPartScale, SpineInspectorUtility.TempContent("Update Part Scale", tooltip: "Update separator part GameObject scale to match the scale (lossyScale) of the SkeletonGraphic. This can be helpful when re-parenting parts to a different GameObject."));
			}
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

		[MenuItem("GameObject/Spine/SkeletonGraphic (UnityUI)", false, 15)]
		static public void SkeletonGraphicCreateMenuItem () {
			GameObject parentGameObject = Selection.activeObject as GameObject;
			RectTransform parentTransform = parentGameObject == null ? null : parentGameObject.GetComponent<RectTransform>();

			if (parentTransform == null)
				Debug.LogWarning("Your new SkeletonGraphic will not be visible until it is placed under a Canvas");

			GameObject gameObject = NewSkeletonGraphicGameObject("New SkeletonGraphic");
			gameObject.transform.SetParent(parentTransform, false);
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = gameObject;
			EditorGUIUtility.PingObject(Selection.activeObject);
		}

		// SpineEditorUtilities.InstantiateDelegate. Used by drag and drop.
		public static Component SpawnSkeletonGraphicFromDrop (SkeletonDataAsset data) {
			return InstantiateSkeletonGraphic(data);
		}

		public static SkeletonGraphic InstantiateSkeletonGraphic (SkeletonDataAsset skeletonDataAsset, string skinName) {
			return InstantiateSkeletonGraphic(skeletonDataAsset, skeletonDataAsset.GetSkeletonData(true).FindSkin(skinName));
		}

		public static SkeletonGraphic InstantiateSkeletonGraphic (SkeletonDataAsset skeletonDataAsset, Skin skin = null) {
			string spineGameObjectName = string.Format("SkeletonGraphic ({0})", skeletonDataAsset.name.Replace("_SkeletonData", ""));
			GameObject go = NewSkeletonGraphicGameObject(spineGameObjectName);
			SkeletonGraphic graphic = go.GetComponent<SkeletonGraphic>();
			graphic.skeletonDataAsset = skeletonDataAsset;

			SkeletonData data = skeletonDataAsset.GetSkeletonData(true);

			if (data == null) {
				for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++) {
					string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
					skeletonDataAsset.atlasAssets[i] = (AtlasAssetBase)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAssetBase));
				}

				data = skeletonDataAsset.GetSkeletonData(true);
			}

			skin = skin ?? data.DefaultSkin ?? data.Skins.Items[0];
			graphic.MeshGenerator.settings.zSpacing = SpineEditorUtilities.Preferences.defaultZSpacing;

			graphic.startingLoop = SpineEditorUtilities.Preferences.defaultInstantiateLoop;
			graphic.Initialize(false);
			if (skin != null) graphic.Skeleton.SetSkin(skin);
			graphic.initialSkinName = skin.Name;
			graphic.Skeleton.UpdateWorldTransform();
			graphic.UpdateMesh();
			return graphic;
		}

		static GameObject NewSkeletonGraphicGameObject (string gameObjectName) {
			GameObject go = EditorInstantiation.NewGameObject(gameObjectName, true, typeof(RectTransform), typeof(CanvasRenderer), typeof(SkeletonGraphic));
			SkeletonGraphic graphic = go.GetComponent<SkeletonGraphic>();
			graphic.material = SkeletonGraphicInspector.DefaultSkeletonGraphicMaterial;
			graphic.additiveMaterial = SkeletonGraphicInspector.DefaultSkeletonGraphicAdditiveMaterial;
			graphic.multiplyMaterial = SkeletonGraphicInspector.DefaultSkeletonGraphicMultiplyMaterial;
			graphic.screenMaterial = SkeletonGraphicInspector.DefaultSkeletonGraphicScreenMaterial;

#if HAS_CULL_TRANSPARENT_MESH
			CanvasRenderer canvasRenderer = go.GetComponent<CanvasRenderer>();
			canvasRenderer.cullTransparentMesh = false;
#endif
			return go;
		}

		public static Material DefaultSkeletonGraphicMaterial {
			get { return FirstMaterialWithName("SkeletonGraphicDefault"); }
		}

		public static Material DefaultSkeletonGraphicAdditiveMaterial {
			get { return FirstMaterialWithName("SkeletonGraphicAdditive"); }
		}

		public static Material DefaultSkeletonGraphicMultiplyMaterial {
			get { return FirstMaterialWithName("SkeletonGraphicMultiply"); }
		}

		public static Material DefaultSkeletonGraphicScreenMaterial {
			get { return FirstMaterialWithName("SkeletonGraphicScreen"); }
		}

		protected static Material FirstMaterialWithName (string name) {
			string[] guids = AssetDatabase.FindAssets(name + " t:material");
			if (guids.Length <= 0) return null;

			string firstAssetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
			if (string.IsNullOrEmpty(firstAssetPath)) return null;

			Material firstMaterial = AssetDatabase.LoadAssetAtPath<Material>(firstAssetPath);
			return firstMaterial;
		}

		#endregion
	}
}
