/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

using Spine;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	using Event = UnityEngine.Event;

	[CustomPropertyDrawer(typeof(MaterialOverrideSet))]
	public class MaterialOverrideSetDrawer : PropertyDrawer {
		private const float Padding = 5f;
		private const float ButtonWidth = 20f;

		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
			SerializedProperty dictionaryKeysProp = property.FindPropertyRelative("dictionaryKeys");
			return EditorGUIUtility.singleLineHeight * (dictionaryKeysProp.arraySize + 2);
		}

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			SerializedProperty nameProperty = property.FindPropertyRelative("name");
			SerializedProperty dictionaryKeysProperty = property.FindPropertyRelative("dictionaryKeys");
			SerializedProperty dictionaryValuesProperty = property.FindPropertyRelative("dictionaryValues");

			Rect labelPosition = new Rect(position.x, position.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
			Rect namePosition = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);

			EditorGUI.LabelField(labelPosition, label);
			nameProperty.stringValue = EditorGUI.TextField(namePosition, GUIContent.none, nameProperty.stringValue);

			Rect contentPosition = EditorGUI.IndentedRect(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, position.height - EditorGUIUtility.singleLineHeight));
			float lineHeight = EditorGUIUtility.singleLineHeight;

			for (int i = 0; i < dictionaryKeysProperty.arraySize; i++) {
				Rect keyRect = new Rect(contentPosition.x, contentPosition.y + i * lineHeight, contentPosition.width * 0.5f, lineHeight);
				Rect valueRect = new Rect(contentPosition.x + contentPosition.width * 0.5f, contentPosition.y + i * lineHeight, contentPosition.width * 0.5f - ButtonWidth, lineHeight);
				Rect removeButtonRect = new Rect(contentPosition.xMax - ButtonWidth, contentPosition.y + i * lineHeight, ButtonWidth, lineHeight);

				EditorGUI.PropertyField(keyRect, dictionaryKeysProperty.GetArrayElementAtIndex(i), GUIContent.none);
				EditorGUI.PropertyField(valueRect, dictionaryValuesProperty.GetArrayElementAtIndex(i), GUIContent.none);

				if (GUI.Button(removeButtonRect, "-")) {
					dictionaryKeysProperty.DeleteArrayElementAtIndex(i);
					dictionaryValuesProperty.DeleteArrayElementAtIndex(i);
					break;
				}
			}

			int indent = 15;
			Rect addButtonRect = new Rect(contentPosition.x + indent, contentPosition.y + dictionaryKeysProperty.arraySize * lineHeight, contentPosition.width - indent, lineHeight);
			if (GUI.Button(addButtonRect, "+ Add Entry")) {
				dictionaryKeysProperty.InsertArrayElementAtIndex(dictionaryKeysProperty.arraySize);
				dictionaryValuesProperty.InsertArrayElementAtIndex(dictionaryValuesProperty.arraySize);
			}

			EditorGUI.EndProperty();
		}
	}
}
