/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using Spine;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	using Event = UnityEngine.Event;
	using Icons = SpineEditorUtilities.Icons;

	[CustomEditor(typeof(SkeletonAnimation))]
	[CanEditMultipleObjects]
	public class SkeletonAnimationInspector : UnityEditor.Editor {

		protected SerializedProperty updateTiming, animationName, loop, timeScale, unscaledTime, autoReset, threadedAnimation;
		readonly GUIContent UpdateTimingLabel = new GUIContent("Animation Update",
			"Whether to update the animation in normal Update (the default), " +
			"physics step FixedUpdate, or manually via a user call.");
		readonly GUIContent LoopLabel = new GUIContent("Loop",
			"Whether or not .AnimationName should loop. This only applies to the initial " +
			"animation specified in the inspector, or any subsequent Animations played through .AnimationName. " +
			"Animations set through state.SetAnimation are unaffected.");
		readonly GUIContent TimeScaleLabel = new GUIContent("Time Scale",
			"The rate at which animations progress over time. 1 means normal speed. 0.5 means 50% speed.");
		readonly GUIContent UnscaledTimeLabel = new GUIContent("Unscaled Time",
			"When enabled, AnimationState uses unscaled game time (Time.unscaledDeltaTime), " +
				"running animations independent of e.g. game pause (Time.timeScale). " +
				"Instance SkeletonAnimation.timeScale will still be applied.");
		readonly GUIContent ThreadedAnimationLabel = new GUIContent("Use Threading",
			"When enabled, animations are processed on multiple threads in parallel.");

		protected bool TargetIsValid {
			get {
				foreach (UnityEngine.Object o in targets) {
					ISkeletonAnimation component = (ISkeletonAnimation)o;
					if (!component.IsValid)
						return false;
				}
				return true;
			}
		}

		protected void OnEnable () {
			animationName = serializedObject.FindProperty("animationName");
			loop = serializedObject.FindProperty("loop");
			timeScale = serializedObject.FindProperty("timeScale");
			unscaledTime = serializedObject.FindProperty("unscaledTime");
			updateTiming = serializedObject.FindProperty("updateTiming");
			threadedAnimation = serializedObject.FindProperty("threadedAnimation");
		}

		override public void OnInspectorGUI () {
			DrawInspectorGUI();
			serializedObject.ApplyModifiedProperties();
		}

		protected virtual void DrawInspectorGUI () {
			foreach (UnityEngine.Object c in targets) {
				ISkeletonAnimation component = c as ISkeletonAnimation;
				if (!component.IsValid) {
					SpineEditorUtilities.ReinitializeComponent(component);
				}
			}

			bool sameData = SpineInspectorUtility.TargetsUseSameData(serializedObject);
			EditorGUILayout.Space();
			if (!sameData) {
				EditorGUILayout.DelayedTextField(animationName);
			} else {
				EditorGUILayout.PropertyField(animationName);
			}

			EditorGUILayout.PropertyField(loop, LoopLabel);
			EditorGUILayout.PropertyField(timeScale, TimeScaleLabel);
			foreach (UnityEngine.Object o in targets) {
				SkeletonAnimation component = o as SkeletonAnimation;
				component.timeScale = Mathf.Max(component.timeScale, 0);
			}
			EditorGUILayout.PropertyField(unscaledTime, UnscaledTimeLabel);
			EditorGUILayout.PropertyField(updateTiming, UpdateTimingLabel);
			EditorGUILayout.Space();

			if (threadedAnimation != null) {
				EditorGUILayout.LabelField(SpineInspectorUtility.TempContent("Threaded Animation", SpineEditorUtilities.Icons.subMeshRenderer), EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(threadedAnimation, ThreadedAnimationLabel);
				EditorGUILayout.Space();
			}

			EditorGUILayout.Space();
			SkeletonRootMotionParameter();
		}

		protected void SkeletonRootMotionParameter () {
			SkeletonRootMotionParameter(targets);
		}

		public static void SkeletonRootMotionParameter (Object[] targets) {
			int rootMotionComponentCount = 0;
			foreach (UnityEngine.Object t in targets) {
				Component component = t as Component;
				if (component.GetComponent<SkeletonRootMotion>() != null) {
					++rootMotionComponentCount;
				}
			}
			bool allHaveRootMotion = rootMotionComponentCount == targets.Length;
			bool anyHaveRootMotion = rootMotionComponentCount > 0;

			using (new GUILayout.HorizontalScope()) {
				EditorGUILayout.PrefixLabel("Root Motion");
				if (!allHaveRootMotion) {
					if (GUILayout.Button(SpineInspectorUtility.TempContent("Add Component", Icons.constraintTransform), GUILayout.MaxWidth(130), GUILayout.Height(18))) {
						foreach (UnityEngine.Object t in targets) {
							Component component = t as Component;
							if (component.GetComponent<SkeletonRootMotion>() == null) {
								component.gameObject.AddComponent<SkeletonRootMotion>();
							}
						}
					}
				}
				if (anyHaveRootMotion) {
					if (GUILayout.Button(SpineInspectorUtility.TempContent("Remove Component", Icons.constraintTransform), GUILayout.MaxWidth(140), GUILayout.Height(18))) {
						foreach (UnityEngine.Object t in targets) {
							Component component = t as Component;
							SkeletonRootMotion rootMotionComponent = component.GetComponent<SkeletonRootMotion>();
							if (rootMotionComponent != null) {
								DestroyImmediate(rootMotionComponent);
							}
						}
					}
				}
			}
		}
	}
}
