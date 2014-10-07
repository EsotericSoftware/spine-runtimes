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
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoneFollower))]
public class BoneFollowerInspector : Editor {
	private SerializedProperty boneName, skeletonRenderer, followZPosition, followBoneRotation;
	BoneFollower component;

	void OnEnable () {
		skeletonRenderer = serializedObject.FindProperty("skeletonRenderer");
		boneName = serializedObject.FindProperty("boneName");
		followBoneRotation = serializedObject.FindProperty("followBoneRotation");
		followZPosition = serializedObject.FindProperty("followZPosition");
		component = (BoneFollower)target;
		ForceReload();
	}

	void FindRenderer () {
		if (skeletonRenderer.objectReferenceValue == null) {
			SkeletonRenderer parentRenderer = SkeletonUtility.GetInParent<SkeletonRenderer>(component.transform);

			if (parentRenderer != null) {
				skeletonRenderer.objectReferenceValue = (UnityEngine.Object)parentRenderer;
			}

		}
	}

	void ForceReload () {
		if (component.skeletonRenderer != null) {
			if (component.skeletonRenderer.valid == false)
				component.skeletonRenderer.Reset();
		}
	}

	override public void OnInspectorGUI () {
		serializedObject.Update();

		FindRenderer();

		EditorGUILayout.PropertyField(skeletonRenderer);

		if (component.valid) {
			String[] bones = new String[1];
			try {
				bones = new String[component.skeletonRenderer.skeleton.Data.Bones.Count + 1];
			} catch {

			}

			bones[0] = "<None>";
			for (int i = 0; i < bones.Length - 1; i++)
				bones[i + 1] = component.skeletonRenderer.skeleton.Data.Bones[i].Name;
			Array.Sort<String>(bones);
			int boneIndex = Math.Max(0, Array.IndexOf(bones, boneName.stringValue));

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Bone");
			EditorGUIUtility.LookLikeControls();
			boneIndex = EditorGUILayout.Popup(boneIndex, bones);
			EditorGUILayout.EndHorizontal();

			boneName.stringValue = boneIndex == 0 ? null : bones[boneIndex];
			EditorGUILayout.PropertyField(followBoneRotation);
			EditorGUILayout.PropertyField(followZPosition);
		} else {
			GUILayout.Label("INVALID");
		}

		if (serializedObject.ApplyModifiedProperties() ||
			(Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")
	    ) {
			component.Reset();
		}
	}
}
