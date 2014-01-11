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

[CustomEditor(typeof(SkeletonAnimation))]
public class SkeletonAnimationInspector : Editor {
	private SerializedProperty skeletonDataAsset, initialSkinName, timeScale, normals, tangents;
	private SerializedProperty animationName, loop;

	void OnEnable () {
		skeletonDataAsset = serializedObject.FindProperty("skeletonDataAsset");
		animationName = serializedObject.FindProperty("_animationName");
		loop = serializedObject.FindProperty("loop");
		initialSkinName = serializedObject.FindProperty("initialSkinName");
		timeScale = serializedObject.FindProperty("timeScale");
		normals = serializedObject.FindProperty("calculateNormals");
		tangents = serializedObject.FindProperty("calculateTangents");
	}

	override public void OnInspectorGUI () {
		serializedObject.Update();
		SkeletonAnimation component = (SkeletonAnimation)target;

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
			EditorGUILayout.EndHorizontal();
		
			initialSkinName.stringValue = skins[skinIndex];

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
			EditorGUILayout.EndHorizontal();

			if (animationIndex == 0)
				component.animationName = null;
			else
				component.animationName = animations[animationIndex];
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
