/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#define NO_PREFAB_MESH

using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Editor {
	using Event = UnityEngine.Event;

	[CustomEditor(typeof(SkeletonRenderer))]
	[CanEditMultipleObjects]
	public class SkeletonRendererInspector : UnityEditor.Editor {
		protected static bool advancedFoldout;
		protected static bool showBoneNames, showPaths, showShapes, showConstraints = true;

		protected SerializedProperty skeletonDataAsset, initialSkinName, normals, tangents, meshes, immutableTriangles, separatorSlotNames, frontFacing, zSpacing, pmaVertexColors, clearStateOnDisable;
		protected SpineInspectorUtility.SerializedSortingProperties sortingProperties;
		protected bool isInspectingPrefab;

		protected GUIContent SkeletonDataAssetLabel, SkeletonUtilityButtonContent;
		protected GUIContent PMAVertexColorsLabel, ClearStateOnDisableLabel, ZSpacingLabel, MeshesLabel, ImmubleTrianglesLabel;
		protected GUIContent NormalsLabel, TangentsLabel;
		const string ReloadButtonLabel = "Reload";

		protected bool TargetIsValid {
			get {
				if (serializedObject.isEditingMultipleObjects) {
					foreach (var o in targets) {
						var component = (SkeletonRenderer)o;
						if (!component.valid)
							return false;
					}
					return true;
				} else {
					var component = (SkeletonRenderer)target;
					return component.valid;
				}
			}
		}

		protected virtual void OnEnable () {
			isInspectingPrefab = (PrefabUtility.GetPrefabType(target) == PrefabType.Prefab);
			
			SpineEditorUtilities.ConfirmInitialization();

			// Labels
			SkeletonDataAssetLabel = new GUIContent("SkeletonData Asset", SpineEditorUtilities.Icons.spine);
			SkeletonUtilityButtonContent = new GUIContent("Add Skeleton Utility", SpineEditorUtilities.Icons.skeletonUtility);
			MeshesLabel = new GUIContent("Render MeshAttachments", "Disable to optimize rendering for skeletons that don't use Mesh Attachments");
			ImmubleTrianglesLabel = new GUIContent("Immutable Triangles", "Enable to optimize rendering for skeletons that never change attachment visbility");
			PMAVertexColorsLabel = new GUIContent("PMA Vertex Colors", "Use this if you are using the default Spine/Skeleton shader or any premultiply-alpha shader.");
			ClearStateOnDisableLabel = new GUIContent("Clear State On Disable", "Use this if you are pooling or enabling/disabling your Spine GameObject.");
			ZSpacingLabel = new GUIContent("Z Spacing", "A value other than 0 adds a space between each rendered attachment to prevent Z Fighting when using shaders that read or write to the depth buffer. Large values may cause unwanted parallax and spaces depending on camera setup.");
			NormalsLabel = new GUIContent("Add Normals", "Use this if your shader requires vertex normals. A more efficient solution for 2D setups is to modify the shader to assume a single normal value for the whole mesh.");
			TangentsLabel = new GUIContent("Solve Tangents", "Calculates the tangents per frame. Use this if you are using lit shaders (usually with normal maps) that require vertex tangents.");

			var so = this.serializedObject;
			skeletonDataAsset = so.FindProperty("skeletonDataAsset");
			initialSkinName = so.FindProperty("initialSkinName");
			normals = so.FindProperty("calculateNormals");
			tangents = so.FindProperty("calculateTangents");
			meshes = so.FindProperty("renderMeshes");
			immutableTriangles = so.FindProperty("immutableTriangles");
			pmaVertexColors = so.FindProperty("pmaVertexColors");
			clearStateOnDisable = so.FindProperty("clearStateOnDisable");

			separatorSlotNames = so.FindProperty("separatorSlotNames");
			separatorSlotNames.isExpanded = true;

			frontFacing = so.FindProperty("frontFacing");
			zSpacing = so.FindProperty("zSpacing");

			SerializedObject rso = SpineInspectorUtility.GetRenderersSerializedObject(serializedObject);
			sortingProperties = new SpineInspectorUtility.SerializedSortingProperties(rso);
		}

		public static void ReapplySeparatorSlotNames (SkeletonRenderer skeletonRenderer) {
			if (!skeletonRenderer.valid) return;

			var separatorSlots = skeletonRenderer.separatorSlots;
			var separatorSlotNames = skeletonRenderer.separatorSlotNames;
			var skeleton = skeletonRenderer.skeleton;

			separatorSlots.Clear();
			for (int i = 0, n = separatorSlotNames.Length; i < n; i++) {
				var slot = skeleton.FindSlot(separatorSlotNames[i]);
				if (slot != null) {
					separatorSlots.Add(slot);
				} else {
					Debug.LogWarning(separatorSlotNames[i] + " is not a slot in " + skeletonRenderer.skeletonDataAsset.skeletonJSON.name);				
				}
			}
		}

		protected virtual void DrawInspectorGUI (bool multi) {
			bool valid = TargetIsValid;
			var reloadWidth = GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(ReloadButtonLabel)).x + 20);
			var reloadButtonStyle = EditorStyles.miniButtonRight;

			if (multi) {
				using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
					SpineInspectorUtility.PropertyFieldFitLabel(skeletonDataAsset, SkeletonDataAssetLabel);
					if (GUILayout.Button(ReloadButtonLabel, reloadButtonStyle, reloadWidth)) {
						foreach (var c in targets) {
							var component = c as SkeletonRenderer;
							if (component.skeletonDataAsset != null) {
								foreach (AtlasAsset aa in component.skeletonDataAsset.atlasAssets) {
									if (aa != null)
										aa.Clear();
								}
								component.skeletonDataAsset.Clear();
							}
							component.Initialize(true);
						}
					}
				}

				foreach (var c in targets) {
					var component = c as SkeletonRenderer;
					if (!component.valid) {
						if (Event.current.type == EventType.Layout) {
							component.Initialize(true);
							component.LateUpdate();
						}
						if (!component.valid)
							continue;
					}

					#if NO_PREFAB_MESH
					if (isInspectingPrefab) {
						MeshFilter meshFilter = component.GetComponent<MeshFilter>();
						if (meshFilter != null)
							meshFilter.sharedMesh = null;
					}
					#endif
				}
					
				if (valid)
					EditorGUILayout.PropertyField(initialSkinName);
			} else {
				var component = (SkeletonRenderer)target;

				if (!component.valid && Event.current.type == EventType.Layout) {
					component.Initialize(true);
					component.LateUpdate();
				}

				using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
					SpineInspectorUtility.PropertyFieldFitLabel(skeletonDataAsset, SkeletonDataAssetLabel);
					if (component.valid) {
						if (GUILayout.Button(ReloadButtonLabel, reloadButtonStyle, reloadWidth)) {
							if (component.skeletonDataAsset != null) {
								foreach (AtlasAsset aa in component.skeletonDataAsset.atlasAssets) {
									if (aa != null)
										aa.Clear();
								}
								component.skeletonDataAsset.Clear();
							}
							component.Initialize(true);
						}
					}
				}

				if (component.skeletonDataAsset == null) {
					EditorGUILayout.HelpBox("Skeleton Data Asset required", MessageType.Warning);
					return;
				}

				#if NO_PREFAB_MESH
				if (isInspectingPrefab) {
					MeshFilter meshFilter = component.GetComponent<MeshFilter>();
					if (meshFilter != null)
						meshFilter.sharedMesh = null;
				}
				#endif

				// Initial skin name.
				if (component.valid) {
					string[] skins = new string[component.skeleton.Data.Skins.Count];
					int skinIndex = 0;
					for (int i = 0; i < skins.Length; i++) {
						string skinNameString = component.skeleton.Data.Skins.Items[i].Name;
						skins[i] = skinNameString;
						if (skinNameString == initialSkinName.stringValue)
							skinIndex = i;
					}
					skinIndex = EditorGUILayout.Popup("Initial Skin", skinIndex, skins);
					if (skins.Length > 0) // Support attachmentless/skinless SkeletonData.
						initialSkinName.stringValue = skins[skinIndex];
				}
			}

			EditorGUILayout.Space();

			// Sorting Layers
			SpineInspectorUtility.SortingPropertyFields(sortingProperties, applyModifiedProperties: true);

			if (!TargetIsValid) return;
			
			// More Render Options...
			using (new SpineInspectorUtility.BoxScope()) {
				EditorGUI.BeginChangeCheck();
				if (advancedFoldout = EditorGUILayout.Foldout(advancedFoldout, "Advanced")) {
					using (new SpineInspectorUtility.IndentScope()) {

						using (new SpineInspectorUtility.LabelWidthScope()) {
							// Optimization options
							EditorGUILayout.PropertyField(meshes, MeshesLabel);
							EditorGUILayout.PropertyField(immutableTriangles, ImmubleTrianglesLabel);
							EditorGUILayout.PropertyField(clearStateOnDisable, ClearStateOnDisableLabel);
							EditorGUILayout.Space();
						}

						SeparatorsField(separatorSlotNames);
						EditorGUILayout.Space();

						// Render options
						const float MinZSpacing = -0.1f;
						const float MaxZSpacing = 0f;
						EditorGUILayout.Slider(zSpacing, MinZSpacing, MaxZSpacing, ZSpacingLabel);
						EditorGUILayout.Space();

						using (new SpineInspectorUtility.LabelWidthScope()) {
							EditorGUILayout.LabelField("Vertex Data", EditorStyles.boldLabel);
							EditorGUILayout.PropertyField(pmaVertexColors, PMAVertexColorsLabel);

							// Optional fields. May be disabled in SkeletonRenderer.
							if (normals != null) EditorGUILayout.PropertyField(normals, NormalsLabel);
							if (tangents != null) EditorGUILayout.PropertyField(tangents, TangentsLabel);
							if (frontFacing != null) EditorGUILayout.PropertyField(frontFacing);

							EditorGUILayout.Space();

							EditorGUILayout.LabelField("Editor Preview", EditorStyles.boldLabel);
							showBoneNames = EditorGUILayout.Toggle("Show Bone Names", showBoneNames);
							showPaths = EditorGUILayout.Toggle("Show Paths", showPaths);
							showShapes = EditorGUILayout.Toggle("Show Shapes", showShapes);
							showConstraints = EditorGUILayout.Toggle("Show Constraints", showConstraints);
						}

						EditorGUILayout.Space();
					}
				}
				if (EditorGUI.EndChangeCheck())
					SceneView.RepaintAll();
			}
		}

		public static void SeparatorsField (SerializedProperty separatorSlotNames) {
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
					EditorGUILayout.PropertyField(separatorSlotNames, new GUIContent(separatorSlotNames.displayName + terminalSlotWarning, SeparatorsDescription), true);
					EditorGUILayout.Space();
				} else
					EditorGUILayout.PropertyField(separatorSlotNames, new GUIContent(separatorSlotNames.displayName + string.Format("{0} [{1}]", terminalSlotWarning, separatorSlotNames.arraySize), SeparatorsDescription), true);
			}
		}

		public void OnSceneGUI () {
			var skeletonRenderer = (SkeletonRenderer)target;
			var skeleton = skeletonRenderer.skeleton;
			var transform = skeletonRenderer.transform;

			if (skeleton == null) return;

			if (showPaths) SpineHandles.DrawPaths(transform, skeleton);
			SpineHandles.DrawBones(transform, skeleton);
			if (showConstraints) SpineHandles.DrawConstraints(transform, skeleton);
			if (showBoneNames) SpineHandles.DrawBoneNames(transform, skeleton);
			if (showShapes) SpineHandles.DrawBoundingBoxes(transform, skeleton);
		}

		public void DrawSkeletonUtilityButton (bool multi) {
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
				EditorGUILayout.Space();
				var component = (Component)target;
				if (component.GetComponent<SkeletonUtility>() == null) {						
					if (SpineInspectorUtility.LargeCenteredButton(SkeletonUtilityButtonContent))
						component.gameObject.AddComponent<SkeletonUtility>();
				}
			}
		}

		override public void OnInspectorGUI () {
			//serializedObject.Update();
			bool multi = serializedObject.isEditingMultipleObjects;
			DrawInspectorGUI(multi);
			if (serializedObject.ApplyModifiedProperties() || SpineInspectorUtility.UndoRedoPerformed(Event.current)) {
				if (!Application.isPlaying) {
					if (multi)
						foreach (var o in targets)
							((SkeletonRenderer)o).Initialize(true);
					else
						((SkeletonRenderer)target).Initialize(true);
				}
			}
		}

	}
}
