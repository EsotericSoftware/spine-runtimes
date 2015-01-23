/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
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
using UnityEditor;
using UnityEngine;
using Spine;


[CustomEditor(typeof(AtlasAsset))]
public class AtlasAssetInspector : Editor {
	private SerializedProperty atlasFile, materials;

	void OnEnable () {
		SpineEditorUtilities.ConfirmInitialization();
		atlasFile = serializedObject.FindProperty("atlasFile");
		materials = serializedObject.FindProperty("materials");
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
			EditorGUILayout.LabelField("Regions");
			EditorGUI.indentLevel++;
			for (int i = 0; i < regions.Count; i++) {
				EditorGUILayout.LabelField(regions[i].name);
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
