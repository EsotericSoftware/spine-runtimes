/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
					TrySetAnimation(o as SkeletonAnimation, multi);
				
				EditorGUILayout.Space();
				if (!sameData) {
					EditorGUILayout.DelayedTextField(animationName);
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
				TrySetAnimation(target as SkeletonAnimation, multi);

				EditorGUILayout.Space();
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(animationName);
				wasAnimationNameChanged |= EditorGUI.EndChangeCheck(); // Value used in the next update.
				EditorGUILayout.PropertyField(loop, LoopLabel);
				EditorGUILayout.PropertyField(timeScale, TimeScaleLabel);
				var component = (SkeletonAnimation)target;
				component.timeScale = Mathf.Max(component.timeScale, 0);
				EditorGUILayout.Space();
			}

			if (!isInspectingPrefab) {
				if (requireRepaint) {
					SceneView.RepaintAll();
					requireRepaint = false;
				}
			}
		}

		protected void TrySetAnimation (SkeletonAnimation skeletonAnimation, bool multi) {
			if (skeletonAnimation == null) return;
			if (!skeletonAnimation.valid)
				return;

			if (!isInspectingPrefab) {
				if (wasAnimationNameChanged) {
					var skeleton = skeletonAnimation.Skeleton;
					var state = skeletonAnimation.AnimationState;

					if (!Application.isPlaying) {
						if (state != null) state.ClearTrack(0);
						skeleton.SetToSetupPose();
					}

					Spine.Animation animationToUse = skeleton.Data.FindAnimation(animationName.stringValue);

					if (!Application.isPlaying) {
						if (animationToUse != null) animationToUse.PoseSkeleton(skeleton, 0f);
						skeleton.UpdateWorldTransform();
						skeletonAnimation.LateUpdate();
						requireRepaint = true;
					} else {
						if (animationToUse != null)
							state.SetAnimation(0, animationToUse, loop.boolValue);
						else
							state.ClearTrack(0);
					}

					wasAnimationNameChanged = false;
				}

				// Reflect animationName serialized property in the inspector even if SetAnimation API was used.
				if (!multi && Application.isPlaying) {
					TrackEntry current = skeletonAnimation.AnimationState.GetCurrent(0);
					if (current != null) {
						if (skeletonAnimation.AnimationName != animationName.stringValue)
							animationName.stringValue = current.Animation.Name;
					}
				}
			}
		}
	}
}
