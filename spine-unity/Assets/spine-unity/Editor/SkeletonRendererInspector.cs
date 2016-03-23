/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	
	[CustomEditor(typeof(SkeletonRenderer))]
	public class SkeletonRendererInspector : UnityEditor.Editor {
		protected static bool advancedFoldout;

		protected SerializedProperty skeletonDataAsset, initialSkinName, normals, tangents, meshes, immutableTriangles, separatorSlotNames, front, zSpacing;

		protected SpineInspectorUtility.SerializedSortingProperties sortingProperties;

		protected virtual void OnEnable () {
			SpineEditorUtilities.ConfirmInitialization();
			skeletonDataAsset = serializedObject.FindProperty("skeletonDataAsset");
			initialSkinName = serializedObject.FindProperty("initialSkinName");
			normals = serializedObject.FindProperty("calculateNormals");
			tangents = serializedObject.FindProperty("calculateTangents");
			meshes = serializedObject.FindProperty("renderMeshes");
			immutableTriangles = serializedObject.FindProperty("immutableTriangles");
			separatorSlotNames = serializedObject.FindProperty("separatorSlotNames");
			separatorSlotNames.isExpanded = true;

			front = serializedObject.FindProperty("frontFacing");
			zSpacing = serializedObject.FindProperty("zSpacing");

			var renderer = ((SkeletonRenderer)target).GetComponent<Renderer>();
			sortingProperties = new SpineInspectorUtility.SerializedSortingProperties(renderer);
		}

		protected virtual void DrawInspectorGUI () {
			SkeletonRenderer component = (SkeletonRenderer)target;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(skeletonDataAsset);
			float reloadWidth = GUI.skin.label.CalcSize(new GUIContent("Reload")).x + 20;
			if (GUILayout.Button("Reload", GUILayout.Width(reloadWidth))) {
				if (component.skeletonDataAsset != null) {
					foreach (AtlasAsset aa in component.skeletonDataAsset.atlasAssets) {
						if (aa != null)
							aa.Reset();
					}

					component.skeletonDataAsset.Reset();
				}
				component.Initialize(true);
			}
			EditorGUILayout.EndHorizontal();

			if (!component.valid) {
				component.Initialize(true);
				component.LateUpdate();
				if (!component.valid)
					return;
			}

			// Initial skin name.
			{
				String[] skins = new String[component.skeleton.Data.Skins.Count];
				int skinIndex = 0;
				for (int i = 0; i < skins.Length; i++) {
					String skinNameString = component.skeleton.Data.Skins.Items[i].Name;
					skins[i] = skinNameString;
					if (skinNameString == initialSkinName.stringValue)
						skinIndex = i;
				}

				skinIndex = EditorGUILayout.Popup("Initial Skin", skinIndex, skins);			
				initialSkinName.stringValue = skins[skinIndex];
			}

			EditorGUILayout.Space();

			// Sorting Layers
			{
				SpineInspectorUtility.SortingPropertyFields(sortingProperties, applyModifiedProperties: true);
			}

			// More Render Options...
			{
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					EditorGUI.indentLevel++;
					advancedFoldout = EditorGUILayout.Foldout(advancedFoldout, "Advanced");
					if(advancedFoldout) {
						EditorGUI.indentLevel++;
						SeparatorsField(separatorSlotNames);
						EditorGUILayout.PropertyField(meshes,
							new GUIContent("Render Mesh Attachments", "Disable to optimize rendering for skeletons that don't use Mesh Attachments"));
						EditorGUILayout.PropertyField(immutableTriangles,
							new GUIContent("Immutable Triangles", "Enable to optimize rendering for skeletons that never change attachment visbility"));
						EditorGUILayout.Space();

						const float MinZSpacing = -0.1f;
						const float MaxZSpacing = 0f;
						EditorGUILayout.Slider(zSpacing, MinZSpacing, MaxZSpacing);

						if (normals != null) {
							EditorGUILayout.PropertyField(normals);
							EditorGUILayout.PropertyField(tangents);
						}

						if (front != null) {
							EditorGUILayout.PropertyField(front);
						}
						EditorGUI.indentLevel--;
					}
					EditorGUI.indentLevel--;

				}
			}
		}

		public static void SeparatorsField (SerializedProperty separatorSlotNames) {
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
				if (separatorSlotNames.isExpanded) {
					EditorGUILayout.PropertyField(separatorSlotNames, includeChildren: true);
				} else {
					EditorGUILayout.PropertyField(separatorSlotNames, new GUIContent(separatorSlotNames.displayName + string.Format(" [{0}]", separatorSlotNames.arraySize)), includeChildren: true);
				}

			}
		}

		override public void OnInspectorGUI () {
			serializedObject.Update();
			DrawInspectorGUI();
			if (serializedObject.ApplyModifiedProperties() ||
				(UnityEngine.Event.current.type == EventType.ValidateCommand && UnityEngine.Event.current.commandName == "UndoRedoPerformed")
			) {
				if (!Application.isPlaying)
					((SkeletonRenderer)target).Initialize(true);
			}
		}

	}
}