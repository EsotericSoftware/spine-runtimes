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

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Spine;

namespace Spine.Unity.Editor {
	using Event = UnityEngine.Event;

	[CustomEditor(typeof(SpineSpriteAtlasAsset)), CanEditMultipleObjects]
	public class SpineSpriteAtlasAssetInspector : UnityEditor.Editor {
		SerializedProperty atlasFile, materials;
		SpineSpriteAtlasAsset atlasAsset;

		static List<AtlasRegion> GetRegions (Atlas atlas) {
			FieldInfo regionsField = SpineInspectorUtility.GetNonPublicField(typeof(Atlas), "regions");
			return (List<AtlasRegion>)regionsField.GetValue(atlas);
		}

		void OnEnable () {
			SpineEditorUtilities.ConfirmInitialization();
			atlasFile = serializedObject.FindProperty("spriteAtlasFile");
			materials = serializedObject.FindProperty("materials");
			materials.isExpanded = true;
			atlasAsset = (SpineSpriteAtlasAsset)target;

			if (!SpineSpriteAtlasAsset.AnySpriteAtlasNeedsRegionsLoaded())
				return;
			EditorApplication.update -= SpineSpriteAtlasAsset.UpdateWhenEditorPlayModeStarted;
			EditorApplication.update += SpineSpriteAtlasAsset.UpdateWhenEditorPlayModeStarted;
		}

		void OnDisable () {
			EditorApplication.update -= SpineSpriteAtlasAsset.UpdateWhenEditorPlayModeStarted;
		}

		override public void OnInspectorGUI () {
			if (serializedObject.isEditingMultipleObjects) {
				DrawDefaultInspector();
				return;
			}

			serializedObject.Update();
			atlasAsset = (atlasAsset == null) ? (SpineSpriteAtlasAsset)target : atlasAsset;

			if (atlasAsset.RegionsNeedLoading) {
				if (GUILayout.Button(SpineInspectorUtility.TempContent("Load regions by entering Play mode"), GUILayout.Height(20))) {
					EditorApplication.isPlaying = true;
				}
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(atlasFile);
			EditorGUILayout.PropertyField(materials, true);
			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
				atlasAsset.Clear();
				atlasAsset.GetAtlas();
				atlasAsset.updateRegionsInPlayMode = true;
			}

			if (materials.arraySize == 0) {
				EditorGUILayout.HelpBox("No materials", MessageType.Error);
				return;
			}

			for (int i = 0; i < materials.arraySize; i++) {
				SerializedProperty prop = materials.GetArrayElementAtIndex(i);
				var material = (Material)prop.objectReferenceValue;
				if (material == null) {
					EditorGUILayout.HelpBox("Materials cannot be null.", MessageType.Error);
					return;
				}
			}

			if (atlasFile.objectReferenceValue != null) {
				int baseIndent = EditorGUI.indentLevel;

				var regions = SpineSpriteAtlasAssetInspector.GetRegions(atlasAsset.GetAtlas());
				int regionsCount = regions.Count;
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.LabelField("Atlas Regions", EditorStyles.boldLabel);
					EditorGUILayout.LabelField(string.Format("{0} regions total", regionsCount));
				}
				AtlasPage lastPage = null;
				for (int i = 0; i < regionsCount; i++) {
					if (lastPage != regions[i].page) {
						if (lastPage != null) {
							EditorGUILayout.Separator();
							EditorGUILayout.Separator();
						}
						lastPage = regions[i].page;
						Material mat = ((Material)lastPage.rendererObject);
						if (mat != null) {
							EditorGUI.indentLevel = baseIndent;
							using (new GUILayout.HorizontalScope())
							using (new EditorGUI.DisabledGroupScope(true))
								EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.Width(250));
							EditorGUI.indentLevel = baseIndent + 1;
						} else {
							EditorGUILayout.HelpBox("Page missing material!", MessageType.Warning);
						}
					}

					string regionName = regions[i].name;
					Texture2D icon = SpineEditorUtilities.Icons.image;
					if (regionName.EndsWith(" ")) {
						regionName = string.Format("'{0}'", regions[i].name);
						icon = SpineEditorUtilities.Icons.warning;
						EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(regionName, icon, "Region name ends with whitespace. This may cause errors. Please check your source image filenames."));
					} else {
						EditorGUILayout.LabelField(SpineInspectorUtility.TempContent(regionName, icon));
					}
				}
				EditorGUI.indentLevel = baseIndent;
			}

			if (serializedObject.ApplyModifiedProperties() || SpineInspectorUtility.UndoRedoPerformed(Event.current))
				atlasAsset.Clear();
		}
	}

}
