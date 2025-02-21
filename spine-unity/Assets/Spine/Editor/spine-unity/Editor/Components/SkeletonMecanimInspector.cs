/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

// Contributed by: Mitch Thompson

using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	[CustomEditor(typeof(SkeletonMecanim))]
	[CanEditMultipleObjects]
	public class SkeletonMecanimInspector : SkeletonRendererInspector {
		public static bool mecanimSettingsFoldout;
		public static bool enableScenePreview;

		protected SerializedProperty autoReset;
		protected SerializedProperty useCustomMixMode;
		protected SerializedProperty layerMixModes;
		protected SerializedProperty layerBlendModes;

		protected override void OnEnable () {
			base.OnEnable();
			SerializedProperty mecanimTranslator = serializedObject.FindProperty("translator");
			autoReset = mecanimTranslator.FindPropertyRelative("autoReset");
			useCustomMixMode = mecanimTranslator.FindPropertyRelative("useCustomMixMode");
			layerMixModes = mecanimTranslator.FindPropertyRelative("layerMixModes");
			layerBlendModes = mecanimTranslator.FindPropertyRelative("layerBlendModes");
		}

		protected override void DrawInspectorGUI (bool multi) {
			AddRootMotionComponentIfEnabled();

			base.DrawInspectorGUI(multi);

			using (new SpineInspectorUtility.BoxScope()) {
				mecanimSettingsFoldout = EditorGUILayout.Foldout(mecanimSettingsFoldout, "Mecanim Translator");
				if (mecanimSettingsFoldout) {
					EditorGUILayout.PropertyField(autoReset, new GUIContent("Auto Reset",
						"When set to true, the skeleton state is mixed out to setup-" +
						"pose when an animation finishes, according to the " +
						"animation's keyed items."));

					EditorGUILayout.PropertyField(useCustomMixMode, new GUIContent("Custom MixMode",
						"When disabled, the recommended MixMode is used according to the layer blend mode. Enable to specify a custom MixMode for each Mecanim layer."));

					if (useCustomMixMode.hasMultipleDifferentValues || useCustomMixMode.boolValue == true) {
						DrawLayerSettings();
						EditorGUILayout.Space();
					}
				}
			}

			EditorGUI.BeginChangeCheck();
			enableScenePreview = EditorGUILayout.Toggle(new GUIContent("Scene Preview",
				"Preview the Animation Clip selected in the Animation window. Lock this SkeletonMecanim Inspector " +
				"window, open the Animation window and select the Animation Clip. Then in the Animation window " +
				"scrub through the timeline."),
				enableScenePreview, GUILayout.MaxWidth(150f));
			bool wasScenePreviewChanged = EditorGUI.EndChangeCheck();
			if (enableScenePreview)
				HandleAnimationPreview();
			else if (wasScenePreviewChanged) // just disabled, back to setup pose
				PreviewAnimationInScene(null, 0.0f);
		}

		protected void AddRootMotionComponentIfEnabled () {
			foreach (UnityEngine.Object t in targets) {
				Component component = t as Component;
				Animator animator = component.GetComponent<Animator>();
				if (animator != null && animator.applyRootMotion) {
					if (component.GetComponent<SkeletonMecanimRootMotion>() == null) {
						component.gameObject.AddComponent<SkeletonMecanimRootMotion>();
					}
				}
			}
		}

		protected void HandleAnimationPreview () {
			UnityEngine.Object animationWindow = AnimationWindowPreview.GetOpenAnimationWindow();

			AnimationClip selectedClip = null;
			if (animationWindow != null) {
				selectedClip = AnimationWindowPreview.GetAnimationClip(animationWindow);
			}

			if (selectedClip != null) {
				float time = AnimationWindowPreview.GetAnimationTime(animationWindow);
				PreviewAnimationInScene(selectedClip, time);
			} else // back to setup pose
				PreviewAnimationInScene(null, 0.0f);
		}

		protected void PreviewAnimationInScene (AnimationClip clip, float time) {
			foreach (UnityEngine.Object c in targets) {
				SkeletonRenderer skeletonRenderer = c as SkeletonRenderer;
				if (skeletonRenderer == null) continue;
				Skeleton skeleton = skeletonRenderer.Skeleton;
				SkeletonData skeletonData = skeleton.Data;

				skeleton.SetToSetupPose();
				if (clip != null) {
					Spine.Animation animation = skeletonData.FindAnimation(clip.name);
					animation.Apply(skeleton, 0, time, false, null, 1.0f, MixBlend.First, MixDirection.In);
				}
				skeletonRenderer.LateUpdate();
			}
			SceneView.RepaintAll();
		}

		protected void DrawLayerSettings () {
			string[] layerNames = GetLayerNames();
			float widthLayerColumn = 140;
			float widthMixColumn = 84;

			using (new GUILayout.HorizontalScope()) {
				Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);
				rect.width = widthLayerColumn;
				EditorGUI.LabelField(rect, SpineInspectorUtility.TempContent("Mecanim Layer"), EditorStyles.boldLabel);

				int savedIndent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				rect.position += new Vector2(rect.width, 0);
				rect.width = widthMixColumn;
				EditorGUI.LabelField(rect, SpineInspectorUtility.TempContent("Mix Mode"), EditorStyles.boldLabel);

				EditorGUI.indentLevel = savedIndent;
			}

			using (new SpineInspectorUtility.IndentScope()) {
				int layerCount = layerMixModes.arraySize;
				for (int i = 0; i < layerCount; ++i) {
					using (new GUILayout.HorizontalScope()) {
						string layerName = i < layerNames.Length ? layerNames[i] : ("Layer " + i);

						Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);
						rect.width = widthLayerColumn;
						EditorGUI.PrefixLabel(rect, SpineInspectorUtility.TempContent(layerName));

						int savedIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel = 0;

						SerializedProperty mixMode = layerMixModes.GetArrayElementAtIndex(i);
						rect.position += new Vector2(rect.width, 0);
						rect.width = widthMixColumn;
						EditorGUI.PropertyField(rect, mixMode, GUIContent.none);

						EditorGUI.indentLevel = savedIndent;
					}
				}
			}
		}

		protected string[] GetLayerNames () {
			int maxLayerCount = 0;
			int maxIndex = 0;
			for (int i = 0; i < targets.Length; ++i) {
				SkeletonMecanim skeletonMecanim = ((SkeletonMecanim)targets[i]);
				int count = skeletonMecanim.Translator.MecanimLayerCount;
				if (count > maxLayerCount) {
					maxLayerCount = count;
					maxIndex = i;
				}
			}
			if (maxLayerCount == 0)
				return new string[0];
			SkeletonMecanim skeletonMecanimMaxLayers = ((SkeletonMecanim)targets[maxIndex]);
			return skeletonMecanimMaxLayers.Translator.MecanimLayerNames;
		}
	}
}
