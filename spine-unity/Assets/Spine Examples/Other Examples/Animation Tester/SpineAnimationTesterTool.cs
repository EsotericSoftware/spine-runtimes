using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine;
using Spine.Unity;

using System.Text;

namespace Spine.Unity.Examples {
	public class SpineAnimationTesterTool : MonoBehaviour, IHasSkeletonDataAsset, IHasSkeletonComponent {

		public SkeletonAnimation skeletonAnimation;
		public SkeletonDataAsset SkeletonDataAsset { get { return skeletonAnimation.SkeletonDataAsset; } }
		public ISkeletonComponent SkeletonComponent { get { return skeletonAnimation; } }

		public bool useOverrideMixDuration;
		public float overrideMixDuration = 0.2f;

		public bool useOverrideAttachmentThreshold = true;

		[Range(0f,1f)]
		public float attachmentThreshold = 0.5f;

		public bool useOverrideDrawOrderThreshold;
		[Range(0f, 1f)]
		public float drawOrderThreshold = 0.5f;

		[System.Serializable]
		public struct AnimationControl {
			[SpineAnimation]
			public string animationName;
			public bool loop;
			public KeyCode key;

			[Space]			
			public bool useCustomMixDuration;
			public float mixDuration;
			//public bool useChainToControl;
			//public int chainToControl;
		}
		[System.Serializable]
		public class ControlledTrack {
			public List<AnimationControl> controls = new List<AnimationControl>();
		}

		[Space]
		public List<ControlledTrack> trackControls = new List<ControlledTrack>();

		[Header("UI")]
		public UnityEngine.UI.Text boundAnimationsText;
		public UnityEngine.UI.Text skeletonNameText;

		void OnValidate () {
			// Fill in the SkeletonData asset name
			if (skeletonNameText != null) {
				if (skeletonAnimation != null && skeletonAnimation.skeletonDataAsset != null) {
					skeletonNameText.text = SkeletonDataAsset.name.Replace("_SkeletonData", "");
				}
			}

			// Fill in the control list.
			if (boundAnimationsText != null) {
				var boundAnimationsStringBuilder = new StringBuilder();
				boundAnimationsStringBuilder.AppendLine("Animation Controls:");

				for (int trackIndex = 0; trackIndex < trackControls.Count; trackIndex++) {

					if (trackIndex > 0)
						boundAnimationsStringBuilder.AppendLine();

					boundAnimationsStringBuilder.AppendFormat("---- Track {0} ---- \n", trackIndex);
					foreach (var ba in trackControls[trackIndex].controls) {
						string animationName = ba.animationName;
						if (string.IsNullOrEmpty(animationName))
							animationName = "SetEmptyAnimation";

						boundAnimationsStringBuilder.AppendFormat("[{0}]  {1}\n", ba.key.ToString(), animationName);
					}

				}	

				boundAnimationsText.text = boundAnimationsStringBuilder.ToString();

			}
				
		}

		void Start () {
			if (useOverrideMixDuration) {
				skeletonAnimation.AnimationState.Data.DefaultMix = overrideMixDuration;
			}
		}

		void Update () {
			var animationState = skeletonAnimation.AnimationState;

			// For each track
			for (int trackIndex = 0; trackIndex < trackControls.Count; trackIndex++) {

				// For each control in the track
				foreach (var control in trackControls[trackIndex].controls) {

					// Check each control, and play the appropriate animation.
					if (Input.GetKeyDown(control.key)) {
						TrackEntry trackEntry;
						if (!string.IsNullOrEmpty(control.animationName)) {
							trackEntry = animationState.SetAnimation(trackIndex, control.animationName, control.loop);
							
						} else {
							float mix = control.useCustomMixDuration ? control.mixDuration : animationState.Data.DefaultMix;
							trackEntry = animationState.SetEmptyAnimation(trackIndex, mix);
						}

						if (trackEntry != null) {
							if (control.useCustomMixDuration)
								trackEntry.MixDuration = control.mixDuration;

							if (useOverrideAttachmentThreshold)
								trackEntry.AttachmentThreshold = attachmentThreshold;

							if (useOverrideDrawOrderThreshold)
								trackEntry.DrawOrderThreshold = drawOrderThreshold;
						}

						// Don't parse more than one animation per track.
						break; 
					}
				}
			}

		}

	}
}

