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
	
	[CustomEditor(typeof(SkeletonRenderer))]
	[CanEditMultipleObjects]
	public class SkeletonRendererInspector : UnityEditor.Editor {
		protected static bool advancedFoldout;
		protected SerializedProperty skeletonDataAsset, initialSkinName, normals, tangents, meshes, immutableTriangles, separatorSlotNames, frontFacing, zSpacing, pmaVertexColors;
		protected SpineInspectorUtility.SerializedSortingProperties sortingProperties;
		protected bool isInspectingPrefab;

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
			skeletonDataAsset = serializedObject.FindProperty("skeletonDataAsset");
			initialSkinName = serializedObject.FindProperty("initialSkinName");
			normals = serializedObject.FindProperty("calculateNormals");
			tangents = serializedObject.FindProperty("calculateTangents");
			meshes = serializedObject.FindProperty("renderMeshes");
			immutableTriangles = serializedObject.FindProperty("immutableTriangles");
			pmaVertexColors = serializedObject.FindProperty("pmaVertexColors");
			separatorSlotNames = serializedObject.FindProperty("separatorSlotNames");
			separatorSlotNames.isExpanded = true;

			frontFacing = serializedObject.FindProperty("frontFacing");
			zSpacing = serializedObject.FindProperty("zSpacing");

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
			if (multi) {
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.PropertyField(skeletonDataAsset);
					const string ReloadButtonLabel = "Reload";
					float reloadWidth = GUI.skin.label.CalcSize(new GUIContent(ReloadButtonLabel)).x + 20;
					if (GUILayout.Button(ReloadButtonLabel, GUILayout.Width(reloadWidth))) {
						foreach (var c in targets) {
							var component = c as SkeletonRenderer;
							if (component.skeletonDataAsset != null) {
								foreach (AtlasAsset aa in component.skeletonDataAsset.atlasAssets) {
									if (aa != null)
										aa.Reset();
								}
								component.skeletonDataAsset.Reset();
							}
							component.Initialize(true);
						}
					}
				}

				foreach (var c in targets) {
					var component = c as SkeletonRenderer;
					if (!component.valid) {
						component.Initialize(true);
						component.LateUpdate();
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

				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.PropertyField(skeletonDataAsset);
					if (valid) {
						const string ReloadButtonLabel = "Reload";
						float reloadWidth = GUI.skin.label.CalcSize(new GUIContent(ReloadButtonLabel)).x + 20;
						if (GUILayout.Button(ReloadButtonLabel, GUILayout.Width(reloadWidth))) {
							if (component.skeletonDataAsset != null) {
								foreach (AtlasAsset aa in component.skeletonDataAsset.atlasAssets) {
									if (aa != null)
										aa.Reset();
								}
								component.skeletonDataAsset.Reset();
							}
							component.Initialize(true);
						}
					}
				}

				if (!component.valid) {
					component.Initialize(true);
					component.LateUpdate();
					if (!component.valid) {
						EditorGUILayout.HelpBox("Skeleton Data Asset required", MessageType.Warning);
						return;
					}
				}

				#if NO_PREFAB_MESH
				if (isInspectingPrefab) {
					MeshFilter meshFilter = component.GetComponent<MeshFilter>();
					if (meshFilter != null)
						meshFilter.sharedMesh = null;
				}
				#endif

				// Initial skin name.
				if (valid) {
					string[] skins = new string[component.skeleton.Data.Skins.Count];
					int skinIndex = 0;
					for (int i = 0; i < skins.Length; i++) {
						string skinNameString = component.skeleton.Data.Skins.Items[i].Name;
						skins[i] = skinNameString;
						if (skinNameString == initialSkinName.stringValue)
							skinIndex = i;
					}
					skinIndex = EditorGUILayout.Popup("Initial Skin", skinIndex, skins);			
					initialSkinName.stringValue = skins[skinIndex];
				}
			}

			EditorGUILayout.Space();

			// Sorting Layers
			SpineInspectorUtility.SortingPropertyFields(sortingProperties, applyModifiedProperties: true);

			if (!valid) return;
			
			// More Render Options...
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
				EditorGUI.indentLevel++;
				advancedFoldout = EditorGUILayout.Foldout(advancedFoldout, "Advanced");
				if (advancedFoldout) {
					EditorGUI.indentLevel++;
					SeparatorsField(separatorSlotNames);
					EditorGUILayout.Space();

					// Optimization options
					SpineInspectorUtility.PropertyFieldWideLabel(meshes,
						new GUIContent("Render MeshAttachments", "Disable to optimize rendering for skeletons that don't use Mesh Attachments"));
					SpineInspectorUtility.PropertyFieldWideLabel(immutableTriangles,
						new GUIContent("Immutable Triangles", "Enable to optimize rendering for skeletons that never change attachment visbility"));
					EditorGUILayout.Space();

					// Render options
					const float MinZSpacing = -0.1f;
					const float MaxZSpacing = 0f;
					EditorGUILayout.Slider(zSpacing, MinZSpacing, MaxZSpacing);
					EditorGUILayout.Space();
					SpineInspectorUtility.PropertyFieldWideLabel(pmaVertexColors,
						new GUIContent("PMA Vertex Colors", "Use this if you are using the default Spine/Skeleton shader or any premultiply-alpha shader."));

					// Optional fields. May be disabled in SkeletonRenderer.
					if (normals != null) SpineInspectorUtility.PropertyFieldWideLabel(normals, new GUIContent("Add Normals"));
					if (tangents != null) SpineInspectorUtility.PropertyFieldWideLabel(tangents, new GUIContent("Solve Tangents"));
					if (frontFacing != null) SpineInspectorUtility.PropertyFieldWideLabel(frontFacing);

					EditorGUI.indentLevel--;
				}
				EditorGUI.indentLevel--;
			}
		}

		public static void SeparatorsField (SerializedProperty separatorSlotNames) {
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
				if (separatorSlotNames.isExpanded)
					EditorGUILayout.PropertyField(separatorSlotNames, includeChildren: true);
				else
					EditorGUILayout.PropertyField(separatorSlotNames, new GUIContent(separatorSlotNames.displayName + string.Format(" [{0}]", separatorSlotNames.arraySize)), includeChildren: true);
			}
		}

		public void DrawSkeletonUtilityButton (bool multi) {
			var buttonContent = new GUIContent("Add Skeleton Utility", SpineEditorUtilities.Icons.skeletonUtility);
			if (multi) {
				// Support multi-edit SkeletonUtility button.
				//	EditorGUILayout.Space();
				//	bool addSkeletonUtility = GUILayout.Button(buttonContent, GUILayout.Height(30));
				//	foreach (var t in targets) {
				//		var component = t as SkeletonAnimation;
				//		if (addSkeletonUtility && component.GetComponent<SkeletonUtility>() == null)
				//			component.gameObject.AddComponent<SkeletonUtility>();
				//	}
			} else {
				EditorGUILayout.Space();
				var component = (SkeletonAnimation)target;
				if (component.GetComponent<SkeletonUtility>() == null) {						
					if (GUILayout.Button(buttonContent, GUILayout.Height(30)))
						component.gameObject.AddComponent<SkeletonUtility>();
				}
			}
		}

		override public void OnInspectorGUI () {
			//serializedObject.Update();
			bool multi = serializedObject.isEditingMultipleObjects;
			DrawInspectorGUI(multi);
			if (serializedObject.ApplyModifiedProperties() ||
				(UnityEngine.Event.current.type == EventType.ValidateCommand && UnityEngine.Event.current.commandName == "UndoRedoPerformed")
			) {
				if (!Application.isPlaying) {
					if (multi) {
						foreach (var o in targets) {
							var sr = o as SkeletonRenderer;
							sr.Initialize(true);
						}
					} else {
						((SkeletonRenderer)target).Initialize(true);
					}

				}
					
					
			}
		}

	}
}
