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

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(BoundingBoxFollower))]
public class BoundingBoxFollowerInspector : Editor {
	SerializedProperty skeletonRenderer, slotName;
	BoundingBoxFollower follower;
	bool needToReset = false;

	void OnEnable () {
		skeletonRenderer = serializedObject.FindProperty("skeletonRenderer");
		slotName = serializedObject.FindProperty("slotName");
		follower = (BoundingBoxFollower)target;
	}

	public override void OnInspectorGUI () {
		if (needToReset) {
			follower.HandleReset(null);
			needToReset = false;
		}
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(skeletonRenderer);
		EditorGUILayout.PropertyField(slotName, new GUIContent("Slot"));

		if (EditorGUI.EndChangeCheck()){
			serializedObject.ApplyModifiedProperties();
			needToReset = true;
		}

		bool hasBone = follower.GetComponent<BoneFollower>() != null;

		EditorGUI.BeginDisabledGroup(hasBone || follower.Slot == null);
		{
			if (GUILayout.Button(new GUIContent("Add Bone Follower", SpineEditorUtilities.Icons.bone))) {
				var boneFollower = follower.gameObject.AddComponent<BoneFollower>();
				boneFollower.boneName = follower.Slot.Data.BoneData.Name;
			}
		}
		EditorGUI.EndDisabledGroup();
		
		

		//GUILayout.Space(20);
		GUILayout.Label("Attachment Names", EditorStyles.boldLabel);
		foreach (var kp in follower.attachmentNameTable) {
			string name = kp.Value;
			var collider = follower.colliderTable[kp.Key];
			bool isPlaceholder = name != kp.Key.Name;
			collider.enabled = EditorGUILayout.ToggleLeft(new GUIContent(!isPlaceholder ? name : name + " [" + kp.Key.Name + "]", isPlaceholder ? SpineEditorUtilities.Icons.skinPlaceholder : SpineEditorUtilities.Icons.boundingBox), collider.enabled);
		}
	}
}
