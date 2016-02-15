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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEditor;
using UnityEngine;
using Spine;


[CustomEditor(typeof(AtlasAsset))]
public class AtlasAssetInspector : Editor {
	private SerializedProperty atlasFile, materials;
	private AtlasAsset atlasAsset;
	private List<bool> baked;
	private List<GameObject> bakedObjects;

	void OnEnable () {
		SpineEditorUtilities.ConfirmInitialization();
		atlasFile = serializedObject.FindProperty("atlasFile");
		materials = serializedObject.FindProperty("materials");
		atlasAsset = (AtlasAsset)target;
		UpdateBakedList();
	}

	void UpdateBakedList () {
		AtlasAsset asset = (AtlasAsset)target;
		baked = new List<bool>();
		bakedObjects = new List<GameObject>();
		if (atlasFile.objectReferenceValue != null) {
			Atlas atlas = asset.GetAtlas();
			FieldInfo field = typeof(Atlas).GetField("regions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic);
			List<AtlasRegion> regions = (List<AtlasRegion>)field.GetValue(atlas);
			string atlasAssetPath = AssetDatabase.GetAssetPath(atlasAsset);
			string atlasAssetDirPath = Path.GetDirectoryName(atlasAssetPath);
			string bakedDirPath = Path.Combine(atlasAssetDirPath, atlasAsset.name);


			for (int i = 0; i < regions.Count; i++) {
				AtlasRegion region = regions[i];
				string bakedPrefabPath = Path.Combine(bakedDirPath, SpineEditorUtilities.GetPathSafeRegionName(region) + ".prefab").Replace("\\", "/");
				GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(bakedPrefabPath, typeof(GameObject));
				baked.Add(prefab != null);
				bakedObjects.Add(prefab);
			}
		}
	}



	override public void OnInspectorGUI () {
		serializedObject.Update();
		AtlasAsset asset = (AtlasAsset)target;

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(atlasFile);
		EditorGUILayout.PropertyField(materials, true);
		if (EditorGUI.EndChangeCheck())
			serializedObject.ApplyModifiedProperties();

		if (materials.arraySize == 0) {
			EditorGUILayout.LabelField(new GUIContent("Error:  Missing materials", SpineEditorUtilities.Icons.warning));
			return;
		}

		for (int i = 0; i < materials.arraySize; i++) {
			SerializedProperty prop = materials.GetArrayElementAtIndex(i);
			Material mat = (Material)prop.objectReferenceValue;
			if (mat == null) {
				EditorGUILayout.LabelField(new GUIContent("Error:  Materials cannot be null", SpineEditorUtilities.Icons.warning));
				return;
			}
		}

		if (atlasFile.objectReferenceValue != null) {
			Atlas atlas = asset.GetAtlas();
			FieldInfo field = typeof(Atlas).GetField("regions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic);
			List<AtlasRegion> regions = (List<AtlasRegion>)field.GetValue(atlas);
			EditorGUILayout.LabelField(new GUIContent("Region Baking", SpineEditorUtilities.Icons.unityIcon));
			EditorGUI.indentLevel++;
			AtlasPage lastPage = null;
			for (int i = 0; i < regions.Count; i++) {
				if (lastPage != regions[i].page) {
					if (lastPage != null) {
						EditorGUILayout.Separator();
						EditorGUILayout.Separator();
					}
					lastPage = regions[i].page;
					Material mat = ((Material)lastPage.rendererObject);
					if (mat != null) {
						GUILayout.BeginHorizontal();
						{
							EditorGUI.BeginDisabledGroup(true);
							EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.Width(250));
							EditorGUI.EndDisabledGroup();
						}
						GUILayout.EndHorizontal();

					} else {
						EditorGUILayout.LabelField(new GUIContent("Page missing material!", SpineEditorUtilities.Icons.warning));
					}
				}
				GUILayout.BeginHorizontal();
				{
					 //EditorGUILayout.ToggleLeft(baked[i] ? "" : regions[i].name, baked[i]);
					bool result = baked[i] ? EditorGUILayout.ToggleLeft("", baked[i], GUILayout.Width(24)) : EditorGUILayout.ToggleLeft("    " + regions[i].name, baked[i]);
					if(baked[i]){
						EditorGUILayout.ObjectField(bakedObjects[i], typeof(GameObject), false, GUILayout.Width(250));
					}
					if (result && !baked[i]) {
						//bake
						baked[i] = true;
						bakedObjects[i] = SpineEditorUtilities.BakeRegion(atlasAsset, regions[i]);
						EditorGUIUtility.PingObject(bakedObjects[i]);
					} else if (!result && baked[i]) {
						//unbake
						bool unbakeResult = EditorUtility.DisplayDialog("Delete Baked Region", "Do you want to delete the prefab for " + regions[i].name, "Yes", "Cancel");
						switch (unbakeResult) {
							case true:
								//delete
								string atlasAssetPath = AssetDatabase.GetAssetPath(atlasAsset);
								string atlasAssetDirPath = Path.GetDirectoryName(atlasAssetPath);
								string bakedDirPath = Path.Combine(atlasAssetDirPath, atlasAsset.name);
								string bakedPrefabPath = Path.Combine(bakedDirPath, SpineEditorUtilities.GetPathSafeRegionName(regions[i]) + ".prefab").Replace("\\", "/");
								AssetDatabase.DeleteAsset(bakedPrefabPath);
								baked[i] = false;
								break;
							case false:
								//do nothing
								break;
						}
					}
				}
				GUILayout.EndHorizontal();
			}
			EditorGUI.indentLevel--;
		}

		if (serializedObject.ApplyModifiedProperties() ||
			(UnityEngine.Event.current.type == EventType.ValidateCommand && UnityEngine.Event.current.commandName == "UndoRedoPerformed")
		) {
			asset.Reset();
		}
	}
}
