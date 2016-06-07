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
using UnityEditor;
using UnityEngine;
using Spine;

namespace Spine.Unity.Editor {
	
	[CustomEditor(typeof(SkeletonAnimation))]
	public class SkeletonAnimationInspector : SkeletonRendererInspector {
		protected SerializedProperty animationName, loop, timeScale, autoReset;
		protected bool wasAnimationNameChanged;

		protected override void OnEnable () {
			base.OnEnable();
			animationName = serializedObject.FindProperty("_animationName");
			loop = serializedObject.FindProperty("loop");
			timeScale = serializedObject.FindProperty("timeScale");
		}

		protected override void DrawInspectorGUI () {
			base.DrawInspectorGUI();

			SkeletonAnimation component = (SkeletonAnimation)target;
			if (!component.valid)
				return;

			if (!isInspectingPrefab) {
				if (wasAnimationNameChanged) {
					if (!Application.isPlaying) {
						if (component.state != null) component.state.ClearTrack(0);
						component.skeleton.SetToSetupPose();
					}

					Spine.Animation animationToUse = component.skeleton.Data.FindAnimation(animationName.stringValue);

					if (!Application.isPlaying) {
						if (animationToUse != null) animationToUse.Apply(component.skeleton, 0f, 0f, false, null);
						component.Update();
						component.LateUpdate();
						SceneView.RepaintAll();
					} else {
						if (animationToUse != null)
							component.state.SetAnimation(0, animationToUse, loop.boolValue);
						else
							component.state.ClearTrack(0);
					}

					wasAnimationNameChanged = false;
				}

				// Reflect animationName serialized property in the inspector even if SetAnimation API was used.
				if (Application.isPlaying) {
					TrackEntry current = component.state.GetCurrent(0);
					if (current != null) {
						if (component.AnimationName != animationName.stringValue)
							animationName.stringValue = current.Animation.Name;
					}
				}
			}
				
			EditorGUILayout.Space();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(animationName);
			wasAnimationNameChanged |= EditorGUI.EndChangeCheck(); // Value used in the next update.

			EditorGUILayout.PropertyField(loop);
			EditorGUILayout.PropertyField(timeScale);
			component.timeScale = Mathf.Max(component.timeScale, 0);

			EditorGUILayout.Space();

			if (!isInspectingPrefab) {
				if (component.GetComponent<SkeletonUtility>() == null) {
					if (GUILayout.Button(new GUIContent("Add Skeleton Utility", SpineEditorUtilities.Icons.skeletonUtility), GUILayout.Height(30)))
						component.gameObject.AddComponent<SkeletonUtility>();
				}
			}
		}
	}
}
