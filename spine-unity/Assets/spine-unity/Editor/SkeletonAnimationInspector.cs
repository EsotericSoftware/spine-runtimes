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

using System;
using UnityEditor;
using UnityEngine;
using Spine;

[CustomEditor(typeof(SkeletonAnimation))]
public class SkeletonAnimationInspector : SkeletonRendererInspector {
	protected SerializedProperty animationName, loop, timeScale;
	protected bool isPrefab;

	protected override void OnEnable () {
		base.OnEnable();
		animationName = serializedObject.FindProperty("_animationName");
		loop = serializedObject.FindProperty("loop");
		timeScale = serializedObject.FindProperty("timeScale");

		if (PrefabUtility.GetPrefabType(this.target) == PrefabType.Prefab)
			isPrefab = true;


	}

	protected override void gui () {
		base.gui();

		SkeletonAnimation component = (SkeletonAnimation)target;
		if (!component.valid)
			return;

		//catch case where SetAnimation was used to set track 0 without using AnimationName
		if (Application.isPlaying) {
			TrackEntry currentState = component.state.GetCurrent(0);
			if (currentState != null) {
				if (component.AnimationName != animationName.stringValue) {
					animationName.stringValue = currentState.Animation.Name;
				}
			}
		}

		EditorGUILayout.Space();

		//TODO:  Refactor this to use GenericMenu and callbacks to avoid interfering with control by other behaviours.
		// Animation name.
		{
			String[] animations = new String[component.skeleton.Data.Animations.Count + 1];
			animations[0] = "<None>";
			int animationIndex = 0;
			for (int i = 0; i < animations.Length - 1; i++) {
				String name = component.skeleton.Data.Animations[i].Name;
				animations[i + 1] = name;
				if (name == animationName.stringValue)
					animationIndex = i + 1;
			}
		
			animationIndex = EditorGUILayout.Popup("Animation", animationIndex, animations);

			String selectedAnimationName = animationIndex == 0 ? null : animations[animationIndex];
			if (component.AnimationName != selectedAnimationName) {
				component.AnimationName = selectedAnimationName;
				animationName.stringValue = selectedAnimationName;
			}


		}

		EditorGUILayout.PropertyField(loop);
		EditorGUILayout.PropertyField(timeScale);
		component.timeScale = Math.Max(component.timeScale, 0);

		EditorGUILayout.Space();

		if (!isPrefab) {
			if (component.GetComponent<SkeletonUtility>() == null) {
				if (GUILayout.Button(new GUIContent("Add Skeleton Utility", SpineEditorUtilities.Icons.skeletonUtility), GUILayout.Height(30))) {
					component.gameObject.AddComponent<SkeletonUtility>();
				}
			}
		}
	}
}
