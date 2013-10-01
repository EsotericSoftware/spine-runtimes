/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
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
 *****************************************************************************/

using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkeletonAnimation))]
public class SkeletonAnimationInspector : Editor {
	private SerializedProperty skeletonDataAsset, initialSkinName, timeScale, normals, tangents;
	private SerializedProperty animationName, loop, useAnimationName;

	void OnEnable () {
		skeletonDataAsset = serializedObject.FindProperty("skeletonDataAsset");
		animationName = serializedObject.FindProperty("_animationName");
		loop = serializedObject.FindProperty("loop");
		useAnimationName = serializedObject.FindProperty("useAnimationName");
		initialSkinName = serializedObject.FindProperty("initialSkinName");
		timeScale = serializedObject.FindProperty("timeScale");
		normals = serializedObject.FindProperty("calculateNormals");
		tangents = serializedObject.FindProperty("calculateTangents");
	}

	override public void OnInspectorGUI () {
		serializedObject.Update();
		SkeletonAnimation component = (SkeletonAnimation)target;

		EditorGUIUtility.LookLikeInspector();
		EditorGUILayout.PropertyField(skeletonDataAsset);
		
		if (component.skeleton != null) {
			// Initial skin name.
			String[] skins = new String[component.skeleton.Data.Skins.Count];
			int skinIndex = 0;
			for (int i = 0; i < skins.Length; i++) {
				String name = component.skeleton.Data.Skins[i].Name;
				skins[i] = name;
				if (name == initialSkinName.stringValue)
					skinIndex = i;
			}
		
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Initial Skin");
			EditorGUIUtility.LookLikeControls();
			skinIndex = EditorGUILayout.Popup(skinIndex, skins);
			EditorGUIUtility.LookLikeInspector();
			EditorGUILayout.EndHorizontal();
		
			initialSkinName.stringValue = skins[skinIndex];

			// Animation name.
			String[] animations = new String[component.skeleton.Data.Animations.Count + 2];
			animations[0] = "<No Change>";
			animations[1] = "<None>";
			int animationIndex = useAnimationName.boolValue ? 1 : 0;
			for (int i = 0; i < animations.Length - 2; i++) {
				String name = component.skeleton.Data.Animations[i].Name;
				animations[i + 2] = name;
				if (name == animationName.stringValue)
					animationIndex = i + 2;
			}
		
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Animation");
			EditorGUIUtility.LookLikeControls();
			animationIndex = EditorGUILayout.Popup(animationIndex, animations);
			EditorGUIUtility.LookLikeInspector();
			EditorGUILayout.EndHorizontal();

			if (animationIndex == 0) {
				component.animationName = null;
				useAnimationName.boolValue = false;
			} else if (animationIndex == 1) {
				component.animationName = null;
				useAnimationName.boolValue = true;
			} else {
				component.animationName = animations[animationIndex];
				useAnimationName.boolValue = true;
			}
		}

		// Animation loop.
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Loop");
		loop.boolValue = EditorGUILayout.Toggle(loop.boolValue);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.PropertyField(timeScale);
		EditorGUILayout.PropertyField(normals);
		EditorGUILayout.PropertyField(tangents);
		
		if (serializedObject.ApplyModifiedProperties() ||
			(Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")
		) {
			if (!Application.isPlaying) {
				component.Clear();
				component.Update();
			}
		}
	}
}
