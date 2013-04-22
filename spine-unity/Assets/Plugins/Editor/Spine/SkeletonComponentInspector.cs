/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkeletonComponent))]
public class SkeletonComponentInspector : Editor {
	private SerializedProperty skeletonDataAsset, animationName, loop, timeScale;

	void OnEnable () {
		skeletonDataAsset = serializedObject.FindProperty("skeletonDataAsset");
		animationName = serializedObject.FindProperty("animationName");
		loop = serializedObject.FindProperty("loop");
		timeScale = serializedObject.FindProperty("timeScale");
	}

	override public void OnInspectorGUI () {
		serializedObject.Update();
		SkeletonComponent component = (SkeletonComponent)target;

		EditorGUIUtility.LookLikeInspector();
		EditorGUILayout.PropertyField(skeletonDataAsset);
		
		if (component.skeleton != null) {
			// Animation name.
			String[] animations = new String[component.skeleton.Data.Animations.Count + 1];
			animations[0] = "<None>";
			int animationIndex = 0;
			for (int i = 0; i < animations.Length - 1; i++) {
				String name = component.skeleton.Data.Animations[i].Name;
				animations[i + 1] = name;
				if (name == animationName.stringValue)
					animationIndex = i + 1;
			}
		
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Animation");
			EditorGUIUtility.LookLikeControls();
			animationIndex = EditorGUILayout.Popup(animationIndex, animations);
			EditorGUIUtility.LookLikeInspector();
			EditorGUILayout.EndHorizontal();
		
			animationName.stringValue = animationIndex == 0 ? null : animations[animationIndex];
		}

		// Animation loop.
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Loop");
		loop.boolValue = EditorGUILayout.Toggle(loop.boolValue);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.PropertyField(timeScale);
		
		if (!Application.isPlaying) {
			if (serializedObject.ApplyModifiedProperties() ||
				(Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")
			) {
				component.Clear();
				component.Update();
			}
		}
	}
}
