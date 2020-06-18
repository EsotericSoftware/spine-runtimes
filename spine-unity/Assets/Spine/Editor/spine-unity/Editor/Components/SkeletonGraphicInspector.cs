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
#endif

using UnityEngine;
using UnityEditor;

namespace Spine.Unity.Editor {
	using Icons = SpineEditorUtilities.Icons;

	[InitializeOnLoad]
	[CustomEditor(typeof(SkeletonGraphic))]
	[CanEditMultipleObjects]
	public class SkeletonGraphicInspector : UnityEditor.Editor {

		const string SeparatorSlotNamesFieldName = "separatorSlotNames";
		const string ReloadButtonString = "Reload";
		protected GUIContent SkeletonDataAssetLabel;
		static GUILayoutOption reloadButtonWidth;
		static GUILayoutOption ReloadButtonWidth { get { return reloadButtonWidth = reloadButtonWidth ?? GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(ReloadButtonString)).x + 20); } }
		static GUIStyle ReloadButtonStyle { get { return EditorStyles.miniButton; } }

		SerializedProperty material, color;
		SerializedProperty skeletonDataAsset, initialSkinName;
		SerializedProperty startingAnimation, startingLoop, timeScale, freeze, unscaledTime, tintBlack;
		SerializedProperty initialFlipX, initialFlipY;
		SerializedProperty meshGeneratorSettings;
		SerializedProperty allowMultipleCanvasRenderers, separatorSlotNames, enableSeparatorSlots, updateSeparatorPartLocation;
		SerializedProperty raycastTarget;

		SkeletonGraphic thisSkeletonGraphic;
		protected bool isInspectingPrefab;
		protected bool slotsReapplyRequired = false;
		protected bool forceReloadQueued = false;

		protected bool TargetIsValid {
			get {
				if (serializedObject.isEditingMultipleObjects) {
					foreach (var o in targets) {
						var component = (SkeletonGraphic)o;
						if (!component.IsValid)
							return false;
					}
					return true;
				}
				else {
					var component = (SkeletonGraphic)target;
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

			var so = this.serializedObject;
			thisSkeletonGraphic = target as SkeletonGraphic;

			// MaskableGraphic
			material = so.FindProperty("m_Material");
			color = so.FindProperty("m_Color");
			raycastTarget = so.FindProperty("m_RaycastTarget");

			// SkeletonRenderer
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

			meshGeneratorSettings = so.FindProperty("meshGenerator").FindPropertyRelative("settings");
			meshGeneratorSettings.isExpanded = SkeletonRendererInspector.advancedFoldout;

			allowMultipleCanvasRenderers = so.FindProperty("allowMultipleCanvasRenderers");
			updateSeparatorPartLocation = so.FindProperty("updateSeparatorPartLocation");
			enableSeparatorSlots = so.FindProperty("enableSeparatorSlots");

			separatorSlotNames = so.FindProperty("separatorSlotNames");
			separatorSlotNames.isExpanded = true;
		}

		public override void OnInspectorGUI () {

			if (UnityEngine.Event.current.type == EventType.Layout) {
				if (forceReloadQueued) {
					forceReloadQueued = false;
					foreach (var c in targets) {
						SpineEditorUtilities.ReloadSkeletonDataAssetAndComponent(c as SkeletonGraphic);
					}
				}
				else {
					foreach (var c in targets) {
						var component = c as SkeletonGraphic;
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

			EditorGUILayout.PropertyField(material);
			EditorGUILayout.PropertyField(color);

			if (thisSkeletonGraphic.skeletonDataAsset == null) {
				EditorGUILayout.HelpBox("You need to assign a SkeletonDataAsset first.", MessageType.Info);
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
				return;
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

							foreach (var skeletonGraphic in targets) {
								((SkeletonGraphic)skeletonGraphic).TrimRenderers();
							}
						}
						EditorGUILayout.EndHorizontal();

						// warning box
						if (isSeparationEnabledButNotMultipleRenderers) {
							using (new SpineInspectorUtility.BoxScope()) {
								meshGeneratorSettings.isExpanded = true;
								EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("'Multiple Canvas Renderers' must be enabled\nwhen 'Enable Separation' is enabled.", Icons.warning), GUILayout.Height(42), GUILayout.Width(340));
							}
						}
						else if (meshRendersIncorrectlyWithSingleRenderer) {
							using (new SpineInspectorUtility.BoxScope()) {
								meshGeneratorSettings.isExpanded = true;
								EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("This mesh uses multiple atlas pages. You\n" +
																							"need to enable 'Multiple Canvas Renderers'\n" +
																							"for correct rendering. Consider packing\n" +
																							"attachments to a single atlas page if possible.", Icons.warning), GUILayout.Height(60), GUILayout.Width(340));
							}
						}
					}

					EditorGUILayout.Space();
					SeparatorsField(separatorSlotNames, enableSeparatorSlots, updateSeparatorPartLocation);
				}
			}

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(initialSkinName);
			{
				var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);
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
			EditorGUILayout.PropertyField(unscaledTime, SpineInspectorUtility.TempContent(unscaledTime.displayName, tooltip: "If checked, this will use Time.unscaledDeltaTime to make this update independent of game Time.timeScale. Instance SkeletonGraphic.timeScale will still be applied."));
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(freeze);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(raycastTarget);

			EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight + 5));
			EditorGUILayout.PrefixLabel("Match RectTransform with Mesh");
			if (GUILayout.Button("Match", EditorStyles.miniButton, GUILayout.Width(65f))) {
				foreach (var skeletonGraphic in targets) {
					MatchRectTransformWithBounds((SkeletonGraphic)skeletonGraphic);
				}
			}
			EditorGUILayout.EndHorizontal();

			if (TargetIsValid && !isInspectingPrefab) {
				EditorGUILayout.Space();
				if (SpineInspectorUtility.CenteredButton(new GUIContent("Add Skeleton Utility", Icons.skeletonUtility), 21, true, 200f))
				foreach (var t in targets) {
					var component = t as Component;
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
				foreach (var target in targets) {
					var skeletonGraphic = (SkeletonGraphic)target;
					skeletonGraphic.ReapplySeparatorSlotNames();
					skeletonGraphic.LateUpdate();
					SceneView.RepaintAll();
				}
				slotsReapplyRequired = false;
			}
		}

		protected bool SkeletonHasMultipleSubmeshes () {
			foreach (var target in targets) {
				var skeletonGraphic = (SkeletonGraphic)target;
				if (skeletonGraphic.HasMultipleSubmeshInstructions())
					return true;
			}
			return false;
		}

		public static void SetSeparatorSlotNames (SkeletonRenderer skeletonRenderer, string[] newSlotNames) {
			var field = SpineInspectorUtility.GetNonPublicField(typeof(SkeletonRenderer), SeparatorSlotNamesFieldName);
			field.SetValue(skeletonRenderer, newSlotNames);
		}

		public static string[] GetSeparatorSlotNames (SkeletonRenderer skeletonRenderer) {
			var field = SpineInspectorUtility.GetNonPublicField(typeof(SkeletonRenderer), SeparatorSlotNamesFieldName);
			return field.GetValue(skeletonRenderer) as string[];
		}

		public static void SeparatorsField (SerializedProperty separatorSlotNames, SerializedProperty enableSeparatorSlots,
			SerializedProperty updateSeparatorPartLocation) {

			bool multi = separatorSlotNames.serializedObject.isEditingMultipleObjects;
			bool hasTerminalSlot = false;
			if (!multi) {
				var sr = separatorSlotNames.serializedObject.targetObject as ISkeletonComponent;
				var skeleton = sr.Skeleton;
				int lastSlot = skeleton.Slots.Count - 1;
				if (skeleton != null) {
					for (int i = 0, n = separatorSlotNames.arraySize; i < n; i++) {
						int index = skeleton.FindSlotIndex(separatorSlotNames.GetArrayElementAtIndex(i).stringValue);
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
				}
				else
					EditorGUILayout.PropertyField(separatorSlotNames, new GUIContent(separatorSlotNames.displayName + string.Format("{0} [{1}]", terminalSlotWarning, separatorSlotNames.arraySize), SeparatorsDescription), true);

				EditorGUILayout.PropertyField(enableSeparatorSlots, SpineInspectorUtility.TempContent("Enable Separation", tooltip: "Whether to enable separation at the above separator slots."));
				EditorGUILayout.PropertyField(updateSeparatorPartLocation, SpineInspectorUtility.TempContent("Update Part Location", tooltip:"Update separator part GameObject location to match the position of the SkeletonGraphic. This can be helpful when re-parenting parts to a different GameObject."));
			}
		}

		#region Menus
		[MenuItem("CONTEXT/SkeletonGraphic/Match RectTransform with Mesh Bounds")]
		static void MatchRectTransformWithBounds (MenuCommand command) {
			var skeletonGraphic = (SkeletonGraphic)command.context;
			MatchRectTransformWithBounds(skeletonGraphic);
		}

		static void MatchRectTransformWithBounds (SkeletonGraphic skeletonGraphic) {
			if (!skeletonGraphic.MatchRectTransformWithBounds())
				Debug.Log("Mesh was not previously generated.");
		}

		[MenuItem("GameObject/Spine/SkeletonGraphic (UnityUI)", false, 15)]
		static public void SkeletonGraphicCreateMenuItem () {
			var parentGameObject = Selection.activeObject as GameObject;
			var parentTransform = parentGameObject == null ? null : parentGameObject.GetComponent<RectTransform>();

			if (parentTransform == null)
				Debug.LogWarning("Your new SkeletonGraphic will not be visible until it is placed under a Canvas");

			var gameObject = NewSkeletonGraphicGameObject("New SkeletonGraphic");
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
			var go = NewSkeletonGraphicGameObject(spineGameObjectName);
			var graphic = go.GetComponent<SkeletonGraphic>();
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
			var go = EditorInstantiation.NewGameObject(gameObjectName, true, typeof(RectTransform), typeof(CanvasRenderer), typeof(SkeletonGraphic));
			var graphic = go.GetComponent<SkeletonGraphic>();
			graphic.material = SkeletonGraphicInspector.DefaultSkeletonGraphicMaterial;
			return go;
		}

		public static Material DefaultSkeletonGraphicMaterial {
			get {
				var guids = AssetDatabase.FindAssets("SkeletonGraphicDefault t:material");
				if (guids.Length <= 0) return null;

				var firstAssetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
				if (string.IsNullOrEmpty(firstAssetPath)) return null;

				var firstMaterial = AssetDatabase.LoadAssetAtPath<Material>(firstAssetPath);
				return firstMaterial;
			}
		}

		#endregion
	}
}
