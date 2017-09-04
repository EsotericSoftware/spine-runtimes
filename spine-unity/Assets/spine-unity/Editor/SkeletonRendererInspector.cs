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
	using Icons = SpineEditorUtilities.Icons;

	[CustomEditor(typeof(SkeletonRenderer))]
	[CanEditMultipleObjects]
	public class SkeletonRendererInspector : UnityEditor.Editor {
		public static bool advancedFoldout;

		protected SerializedProperty skeletonDataAsset, initialSkinName;
		protected SerializedProperty initialFlipX, initialFlipY;
		protected SerializedProperty singleSubmesh, separatorSlotNames, clearStateOnDisable, immutableTriangles;
		protected SerializedProperty normals, tangents, meshes, zSpacing, pmaVertexColors, tintBlack; // MeshGenerator settings
		protected SpineInspectorUtility.SerializedSortingProperties sortingProperties;
		protected bool isInspectingPrefab;

		protected GUIContent SkeletonDataAssetLabel, SkeletonUtilityButtonContent;
		protected GUIContent PMAVertexColorsLabel, ClearStateOnDisableLabel, ZSpacingLabel, MeshesLabel, ImmubleTrianglesLabel, TintBlackLabel, SingleSubmeshLabel;
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
			SkeletonDataAssetLabel = new GUIContent("SkeletonData Asset", Icons.spine);
			SkeletonUtilityButtonContent = new GUIContent("Add Skeleton Utility", Icons.skeletonUtility);
			MeshesLabel = new GUIContent("Render MeshAttachments", "Disable to optimize rendering for skeletons that don't use Mesh Attachments");
			ImmubleTrianglesLabel = new GUIContent("Immutable Triangles", "Enable to optimize rendering for skeletons that never change attachment visbility");
			PMAVertexColorsLabel = new GUIContent("PMA Vertex Colors", "Use this if you are using the default Spine/Skeleton shader or any premultiply-alpha shader.");
			ClearStateOnDisableLabel = new GUIContent("Clear State On Disable", "Use this if you are pooling or enabling/disabling your Spine GameObject.");
			ZSpacingLabel = new GUIContent("Z Spacing", "A value other than 0 adds a space between each rendered attachment to prevent Z Fighting when using shaders that read or write to the depth buffer. Large values may cause unwanted parallax and spaces depending on camera setup.");
			NormalsLabel = new GUIContent("Add Normals", "Use this if your shader requires vertex normals. A more efficient solution for 2D setups is to modify the shader to assume a single normal value for the whole mesh.");
			TangentsLabel = new GUIContent("Solve Tangents", "Calculates the tangents per frame. Use this if you are using lit shaders (usually with normal maps) that require vertex tangents.");
			TintBlackLabel = new GUIContent("Tint Black (!)", "Adds black tint vertex data to the mesh as UV2 and UV3. Black tinting requires that the shader interpret UV2 and UV3 as black tint colors for this effect to work. You may also use the default [Spine/Skeleton Tint Black] shader.\n\nIf you only need to tint the whole skeleton and not individual parts, the [Spine/Skeleton Tint] shader is recommended for better efficiency and changing/animating the _Black material property via MaterialPropertyBlock.");
			SingleSubmeshLabel = new GUIContent("Use Single Submesh", "Simplifies submesh determination by assuming you are only using one Material and need only one submesh. This is will disable render separation and custom slot materials.");

			var so = this.serializedObject;
			skeletonDataAsset = so.FindProperty("skeletonDataAsset");
			initialSkinName = so.FindProperty("initialSkinName");
			initialFlipX = so.FindProperty("initialFlipX");
			initialFlipY = so.FindProperty("initialFlipY");
			normals = so.FindProperty("addNormals");
			tangents = so.FindProperty("calculateTangents");
			meshes = so.FindProperty("renderMeshes");
			immutableTriangles = so.FindProperty("immutableTriangles");
			pmaVertexColors = so.FindProperty("pmaVertexColors");
			clearStateOnDisable = so.FindProperty("clearStateOnDisable");
			tintBlack = so.FindProperty("tintBlack");
			singleSubmesh = so.FindProperty("singleSubmesh");

			separatorSlotNames = so.FindProperty("separatorSlotNames");
			separatorSlotNames.isExpanded = true;

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

		GUIContent[] skins;
		ExposedList<Skin> loadedSkinList;

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
						if (meshFilter != null && meshFilter.sharedMesh != null)
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
					if (meshFilter != null && meshFilter.sharedMesh != null)
						meshFilter.sharedMesh = null;
				}
				#endif

				// Initial skin name.
				if (component.valid) {
					var skeletonDataSkins = component.skeleton.Data.Skins;
					int skinCount = skeletonDataSkins.Count;
					if (loadedSkinList != skeletonDataSkins) {
						skins = new GUIContent[skinCount];
						loadedSkinList = skeletonDataSkins;
						for (int i = 0; i < skins.Length; i++) {
							string skinNameString = skeletonDataSkins.Items[i].Name;
							skins[i] = new GUIContent(skinNameString, Icons.skin);
						}
					}
					
					int skinIndex = 0;
					for (int i = 0; i < skins.Length; i++) {
						string skinNameString = skeletonDataSkins.Items[i].Name;
						if (skinNameString == initialSkinName.stringValue)
							skinIndex = i;
					}

					skinIndex = EditorGUILayout.Popup(SpineInspectorUtility.TempContent("Initial Skin"), skinIndex, skins);
					if (skins.Length > 0) // Support attachmentless/skinless SkeletonData.
						initialSkinName.stringValue = skins[skinIndex].text;
				}
			}

			EditorGUILayout.Space();

			// Sorting Layers
			SpineInspectorUtility.SortingPropertyFields(sortingProperties, applyModifiedProperties: true);

			if (!TargetIsValid) return;
			
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
							initialFlipX.boolValue = EditorGUILayout.ToggleLeft(initialFlipX.displayName, initialFlipX.boolValue, GUILayout.Width(120f));
							initialFlipY.boolValue = EditorGUILayout.ToggleLeft(initialFlipY.displayName, initialFlipY.boolValue, GUILayout.Width(120f));
							EditorGUILayout.Space();
						}

						EditorGUILayout.Space();
						EditorGUILayout.LabelField("Renderer Settings", EditorStyles.boldLabel);
						using (new SpineInspectorUtility.LabelWidthScope()) {
							// Optimization options
							if (singleSubmesh != null) EditorGUILayout.PropertyField(singleSubmesh, SingleSubmeshLabel);
							//if (meshes != null) EditorGUILayout.PropertyField(meshes, MeshesLabel);
							if (immutableTriangles != null) EditorGUILayout.PropertyField(immutableTriangles, ImmubleTrianglesLabel);
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
							EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Vertex Data", SpineInspectorUtility.UnityIcon<MeshFilter>()), EditorStyles.boldLabel);
							if (pmaVertexColors != null) EditorGUILayout.PropertyField(pmaVertexColors, PMAVertexColorsLabel);
							EditorGUILayout.PropertyField(tintBlack, TintBlackLabel);

							// Optional fields. May be disabled in SkeletonRenderer.
							if (normals != null) EditorGUILayout.PropertyField(normals, NormalsLabel);
							if (tangents != null) EditorGUILayout.PropertyField(tangents, TangentsLabel);
						}

						EditorGUILayout.Space();

						if (TargetIsValid && !isInspectingPrefab) {
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

		public void OnSceneGUI () {
			var skeletonRenderer = (SkeletonRenderer)target;
			var skeleton = skeletonRenderer.skeleton;
			var transform = skeletonRenderer.transform;
			if (skeleton == null) return;

			SpineHandles.DrawBones(transform, skeleton);
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
