/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License, Professional License, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
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
using Spine;

[CustomEditor(typeof(SkeletonDataAsset))]
public class SkeletonDataAssetInspector : Editor {
	private SerializedProperty atlasAsset, skeletonJSON, scale, fromAnimation, toAnimation, duration;
	private bool showAnimationStateData = true;

	void OnEnable () {
		atlasAsset = serializedObject.FindProperty("atlasAsset");
		skeletonJSON = serializedObject.FindProperty("skeletonJSON");
		scale = serializedObject.FindProperty("scale");
		fromAnimation = serializedObject.FindProperty("fromAnimation");
		toAnimation = serializedObject.FindProperty("toAnimation");
		duration = serializedObject.FindProperty("duration");
	}

	override public void OnInspectorGUI () {
		serializedObject.Update();
		SkeletonDataAsset asset = (SkeletonDataAsset)target;

		EditorGUIUtility.LookLikeInspector();
		EditorGUILayout.PropertyField(atlasAsset);
		EditorGUILayout.PropertyField(skeletonJSON);
		EditorGUILayout.PropertyField(scale);
		
		SkeletonData skeletonData = asset.GetSkeletonData(true);
		if (skeletonData != null) {
			showAnimationStateData = EditorGUILayout.Foldout(showAnimationStateData, "Animation State Data");
			if (showAnimationStateData) {
				// Animation names.
				String[] animations = new String[skeletonData.Animations.Count];
				for (int i = 0; i < animations.Length; i++)
					animations[i] = skeletonData.Animations[i].Name;
			
				for (int i = 0; i < fromAnimation.arraySize; i++) {
					SerializedProperty from = fromAnimation.GetArrayElementAtIndex(i);
					SerializedProperty to = toAnimation.GetArrayElementAtIndex(i);
					SerializedProperty durationProp = duration.GetArrayElementAtIndex(i);
					EditorGUILayout.BeginHorizontal();
					from.stringValue = animations[EditorGUILayout.Popup(Math.Max(Array.IndexOf(animations, from.stringValue), 0), animations)];
					to.stringValue = animations[EditorGUILayout.Popup(Math.Max(Array.IndexOf(animations, to.stringValue), 0), animations)];
					durationProp.floatValue = EditorGUILayout.FloatField(durationProp.floatValue);
					if (GUILayout.Button("Delete")) {
						duration.DeleteArrayElementAtIndex(i);
						toAnimation.DeleteArrayElementAtIndex(i);
						fromAnimation.DeleteArrayElementAtIndex(i);
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				if (GUILayout.Button("Add Mix")) {
					duration.arraySize++;
					toAnimation.arraySize++;
					fromAnimation.arraySize++;
				}
				EditorGUILayout.Space();
				EditorGUILayout.EndHorizontal();
			}
		}
		
		if (!Application.isPlaying) {
			if (serializedObject.ApplyModifiedProperties() ||
				(UnityEngine.Event.current.type == EventType.ValidateCommand && UnityEngine.Event.current.commandName == "UndoRedoPerformed")
			) {
				asset.Clear();
			}
		}
	}
}
