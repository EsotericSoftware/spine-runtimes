/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEditor;
using UnityEngine;
using Spine;

namespace Spine.Unity.Editor {
	
	[CustomEditor(typeof(SkeletonAnimation))]
	[CanEditMultipleObjects]
	public class SkeletonAnimationInspector : SkeletonRendererInspector {
		protected SerializedProperty animationName, loop, timeScale, autoReset;
		protected bool wasAnimationNameChanged;
		protected bool requireRepaint;
		readonly GUIContent LoopLabel = new GUIContent("Loop", "Whether or not .AnimationName should loop. This only applies to the initial animation specified in the inspector, or any subsequent Animations played through .AnimationName. Animations set through state.SetAnimation are unaffected.");
		readonly GUIContent TimeScaleLabel = new GUIContent("Time Scale", "The rate at which animations progress over time. 1 means normal speed. 0.5 means 50% speed.");

		protected override void OnEnable () {
			base.OnEnable();
			animationName = serializedObject.FindProperty("_animationName");
			loop = serializedObject.FindProperty("loop");
			timeScale = serializedObject.FindProperty("timeScale");
		}

		protected override void DrawInspectorGUI (bool multi) {
			base.DrawInspectorGUI(multi);
			if (!TargetIsValid) return;
			bool sameData = SpineInspectorUtility.TargetsUseSameData(serializedObject);

			if (multi) {
				foreach (var o in targets)		
					TrySetAnimation(o, multi);
				
				EditorGUILayout.Space();
				if (!sameData) {
					#if UNITY_5_3_OR_NEWER
					EditorGUILayout.DelayedTextField(animationName);
					#else
					animationName.stringValue = EditorGUILayout.TextField(animationName.displayName, animationName.stringValue);
					#endif
				} else {
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(animationName);
					wasAnimationNameChanged |= EditorGUI.EndChangeCheck(); // Value used in the next update.
				}
				EditorGUILayout.PropertyField(loop);
				EditorGUILayout.PropertyField(timeScale);
				foreach (var o in targets) {
					var component = o as SkeletonAnimation;
					component.timeScale = Mathf.Max(component.timeScale, 0);
				}
			} else {
				TrySetAnimation(target, multi);

				EditorGUILayout.Space();
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(animationName);
				wasAnimationNameChanged |= EditorGUI.EndChangeCheck(); // Value used in the next update.
				EditorGUILayout.PropertyField(loop, LoopLabel);
				EditorGUILayout.PropertyField(timeScale, TimeScaleLabel);
				var component = (SkeletonAnimation)target;
				component.timeScale = Mathf.Max(component.timeScale, 0);
			}

			if (!isInspectingPrefab) {
				if (requireRepaint) {
					SceneView.RepaintAll();
					requireRepaint = false;
				}

				DrawSkeletonUtilityButton(multi);
			}
		}

		protected void TrySetAnimation (Object o, bool multi) {
			var skeletonAnimation = o as SkeletonAnimation;
			if (skeletonAnimation == null) return;
			if (!skeletonAnimation.valid)
				return;

			if (!isInspectingPrefab) {
				if (wasAnimationNameChanged) {
					if (!Application.isPlaying) {
						if (skeletonAnimation.state != null) skeletonAnimation.state.ClearTrack(0);
						skeletonAnimation.skeleton.SetToSetupPose();
					}

					Spine.Animation animationToUse = skeletonAnimation.skeleton.Data.FindAnimation(animationName.stringValue);

					if (!Application.isPlaying) {
						if (animationToUse != null) animationToUse.Apply(skeletonAnimation.skeleton, 0f, 0f, false, null, 1f, true, false);
						skeletonAnimation.Update();
						skeletonAnimation.LateUpdate();
						requireRepaint = true;
					} else {
						if (animationToUse != null)
							skeletonAnimation.state.SetAnimation(0, animationToUse, loop.boolValue);
						else
							skeletonAnimation.state.ClearTrack(0);
					}

					wasAnimationNameChanged = false;
				}

				// Reflect animationName serialized property in the inspector even if SetAnimation API was used.
				if (!multi && Application.isPlaying) {
					TrackEntry current = skeletonAnimation.state.GetCurrent(0);
					if (current != null) {
						if (skeletonAnimation.AnimationName != animationName.stringValue)
							animationName.stringValue = current.Animation.Name;
					}
				}
			}
		}
	}
}
