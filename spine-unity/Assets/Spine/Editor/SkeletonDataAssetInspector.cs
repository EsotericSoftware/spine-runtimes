/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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

		EditorGUILayout.PropertyField(atlasAsset);
		EditorGUILayout.PropertyField(skeletonJSON);
		EditorGUILayout.PropertyField(scale);
		
		SkeletonData skeletonData = asset.GetSkeletonData(asset.atlasAsset == null || asset.skeletonJSON == null);
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
