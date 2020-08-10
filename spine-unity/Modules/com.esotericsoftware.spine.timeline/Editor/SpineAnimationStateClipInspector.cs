/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

using UnityEditor;
using Spine.Unity.Playables;
using UnityEngine.Timeline;

namespace Spine.Unity.Editor {

	[CustomEditor(typeof(SpineAnimationStateClip))]
	[CanEditMultipleObjects]
	public class SpineAnimationStateClipInspector : UnityEditor.Editor {

		protected SerializedProperty templateProp = null;

		protected class ClipInfo {
			public TimelineClip timelineClip;
			public float previousBlendInDuration = -1.0f;
			public float unblendedMixDuration = 0.2f;
		}

		protected ClipInfo[] clipInfo = null;

		public void OnEnable () {
			templateProp = serializedObject.FindProperty("template");
			System.Array.Resize(ref clipInfo, targets.Length);
			for (int i = 0; i < targets.Length; ++i) {
				var clip = (SpineAnimationStateClip)targets[i];
				clipInfo[i] = new ClipInfo();
				clipInfo[i].timelineClip = FindTimelineClip(clip);
			}
		}

		public override void OnInspectorGUI () {
			serializedObject.Update();
			EditorGUILayout.PropertyField(templateProp);

			for (int i = 0; i < targets.Length; ++i) {
				var targetClip = (SpineAnimationStateClip)targets[i];
				if (targetClip.template.useBlendDuration)
					AdjustMixDuration(targetClip, clipInfo[i]);
			}

			serializedObject.ApplyModifiedProperties();
		}

		protected void AdjustMixDuration(SpineAnimationStateClip targetClip, ClipInfo timelineClipInfo) {

			if (timelineClipInfo == null)
				return;

			var timelineClip = timelineClipInfo.timelineClip;
			if (timelineClip == null)
				return;

			float blendInDur = (float)timelineClip.blendInDuration;
			bool isBlendingNow = blendInDur > 0;
			bool wasBlendingBefore = timelineClipInfo.previousBlendInDuration > 0;

			if (isBlendingNow) {
				if (!wasBlendingBefore) {
					timelineClipInfo.unblendedMixDuration = targetClip.template.mixDuration;
				}
				targetClip.template.mixDuration = blendInDur;
				EditorUtility.SetDirty(targetClip);
			}
			else if (wasBlendingBefore) {
				targetClip.template.mixDuration = timelineClipInfo.unblendedMixDuration;
				EditorUtility.SetDirty(targetClip);
			}
			timelineClipInfo.previousBlendInDuration = blendInDur;
		}

		protected TimelineClip FindTimelineClip(SpineAnimationStateClip targetClip) {
			string[] guids = AssetDatabase.FindAssets("t:TimelineAsset");
			foreach (string guid in guids) {
				TimelineAsset timeline = (TimelineAsset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(TimelineAsset));
				foreach (var track in timeline.GetOutputTracks()) {
					foreach (var clip in track.GetClips()) {
						if (clip.asset.GetType() == typeof(SpineAnimationStateClip) && object.ReferenceEquals(clip.asset, targetClip)) {
							return clip;
						}
					}
				}
			}
			return null;
		}

	}
}
