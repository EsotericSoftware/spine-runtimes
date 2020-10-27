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

using UnityEditor;
using UnityEngine;
using Spine;
using Spine.Unity;
using Spine.Unity.Playables;
using Spine.Unity.Editor;

[CustomPropertyDrawer(typeof(SpineAnimationStateBehaviour))]
public class SpineAnimationStateDrawer : PropertyDrawer {

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		const int fieldCount = 11;
		return fieldCount * EditorGUIUtility.singleLineHeight;
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		SerializedProperty animationReferenceProp = property.FindPropertyRelative("animationReference");
		SerializedProperty loopProp = property.FindPropertyRelative("loop");

		SerializedProperty customDurationProp = property.FindPropertyRelative("customDuration");
		SerializedProperty useBlendDurationProp = property.FindPropertyRelative("useBlendDuration");
		SerializedProperty mixDurationProp = property.FindPropertyRelative("mixDuration");
		SerializedProperty holdPreviousProp = property.FindPropertyRelative("holdPrevious");
		SerializedProperty eventProp = property.FindPropertyRelative("eventThreshold");
		SerializedProperty attachmentProp = property.FindPropertyRelative("attachmentThreshold");
		SerializedProperty drawOrderProp = property.FindPropertyRelative("drawOrderThreshold");

		// initialize useBlendDuration parameter according to preferences
		SerializedProperty isInitializedProp = property.FindPropertyRelative("isInitialized");
		if (!isInitializedProp.hasMultipleDifferentValues && isInitializedProp.boolValue == false) {
			useBlendDurationProp.boolValue = SpineEditorUtilities.Preferences.timelineUseBlendDuration;
			isInitializedProp.boolValue = true;
		}

		Rect singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

		float lineHeightWithSpacing = EditorGUIUtility.singleLineHeight + 2f;

		EditorGUI.PropertyField(singleFieldRect, animationReferenceProp);

		singleFieldRect.y += lineHeightWithSpacing;
		EditorGUI.PropertyField(singleFieldRect, loopProp);

		singleFieldRect.y += lineHeightWithSpacing * 0.5f;

		singleFieldRect.y += lineHeightWithSpacing;
		EditorGUI.LabelField(singleFieldRect, "Mixing Settings", EditorStyles.boldLabel);

		singleFieldRect.y += lineHeightWithSpacing;
		EditorGUI.PropertyField(singleFieldRect, customDurationProp);

		bool greyOutCustomDurations = (!customDurationProp.hasMultipleDifferentValues &&
										customDurationProp.boolValue == false);
		using (new EditorGUI.DisabledGroupScope(greyOutCustomDurations)) {
			singleFieldRect.y += lineHeightWithSpacing;
			EditorGUI.PropertyField(singleFieldRect, useBlendDurationProp);

			singleFieldRect.y += lineHeightWithSpacing;
			EditorGUI.PropertyField(singleFieldRect, mixDurationProp);
		}

		singleFieldRect.y += lineHeightWithSpacing;
		EditorGUI.PropertyField(singleFieldRect, holdPreviousProp);

		singleFieldRect.y += lineHeightWithSpacing;
		EditorGUI.PropertyField(singleFieldRect, eventProp);

		singleFieldRect.y += lineHeightWithSpacing;
		EditorGUI.PropertyField(singleFieldRect, attachmentProp);

		singleFieldRect.y += lineHeightWithSpacing;
		EditorGUI.PropertyField(singleFieldRect, drawOrderProp);
	}
}
