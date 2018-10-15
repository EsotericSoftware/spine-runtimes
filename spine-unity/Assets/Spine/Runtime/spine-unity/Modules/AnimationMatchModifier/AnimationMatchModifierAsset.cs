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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine;
using Spine.Unity;

namespace Spine.Unity.Modules {

	//[CreateAssetMenu(menuName = "Spine/SkeletonData Modifiers/Animation Match", order = 200)]
	public class AnimationMatchModifierAsset : SkeletonDataModifierAsset {

		public bool matchAllAnimations = true;

		public override void Apply (SkeletonData skeletonData) {
			if (matchAllAnimations)
				AnimationTools.MatchAnimationTimelines(skeletonData.animations, skeletonData);
		}

		public static class AnimationTools {

			#region Filler Timelines
			/// <summary>
			/// Matches the animation timelines across the given set of animations.
			/// This allows unkeyed properties to assume setup pose when animations are naively mixed using Animation.Apply.
			/// </summary>
			/// <param name="animations">An enumerable collection animations whose timelines will be matched.</param>
			/// <param name="skeletonData">The SkeletonData where the animations belong.</param>
			public static void MatchAnimationTimelines (IEnumerable<Spine.Animation> animations, SkeletonData skeletonData) {
				if (animations == null) return;
				if (skeletonData == null) throw new System.ArgumentNullException("skeletonData", "Timelines can't be matched without a SkeletonData source.");

				// Build a reference collection of timelines to match
				// and a collection of dummy timelines that can be used to fill-in missing items.
				var timelineDictionary = new Dictionary<int, Spine.Timeline>();
				foreach (var animation in animations) {
					foreach (var timeline in animation.timelines) {
						if (timeline is EventTimeline) continue;

						int propertyID = timeline.PropertyId;
						if (!timelineDictionary.ContainsKey(propertyID)) {
							timelineDictionary.Add(propertyID, GetFillerTimeline(timeline, skeletonData));
						}
					}
				}
				var idsToMatch = new List<int>(timelineDictionary.Keys);

				// For each animation in the list, check for and add missing timelines.
				var currentAnimationIDs = new HashSet<int>();
				foreach (var animation in animations) {
					currentAnimationIDs.Clear();
					foreach (var timeline in animation.timelines) {
						if (timeline is EventTimeline) continue;
						currentAnimationIDs.Add(timeline.PropertyId);
					}

					var animationTimelines = animation.timelines;
					foreach (int propertyID in idsToMatch) {
						if (!currentAnimationIDs.Contains(propertyID))
							animationTimelines.Add(timelineDictionary[propertyID]);
					}
				}

				// These are locals, but sometimes Unity's GC does weird stuff. So let's clean up.
				timelineDictionary.Clear();
				timelineDictionary = null;
				idsToMatch.Clear();
				idsToMatch = null;
				currentAnimationIDs.Clear();
				currentAnimationIDs = null;
			}

			static Timeline GetFillerTimeline (Timeline timeline, SkeletonData skeletonData) {
				int propertyID = timeline.PropertyId;
				int tt = propertyID >> 24;
				var timelineType = (TimelineType)tt;

				switch (timelineType) {
					// Bone
					case TimelineType.Rotate:
						return GetFillerTimeline((RotateTimeline)timeline, skeletonData);
					case TimelineType.Translate:
						return GetFillerTimeline((TranslateTimeline)timeline, skeletonData);
					case TimelineType.Scale:
						return GetFillerTimeline((ScaleTimeline)timeline, skeletonData);
					case TimelineType.Shear:
						return GetFillerTimeline((ShearTimeline)timeline, skeletonData);

					// Slot
					case TimelineType.Attachment:
						return GetFillerTimeline((AttachmentTimeline)timeline, skeletonData);
					case TimelineType.Color:
						return GetFillerTimeline((ColorTimeline)timeline, skeletonData);
					case TimelineType.TwoColor:
						return GetFillerTimeline((TwoColorTimeline)timeline, skeletonData);
					case TimelineType.Deform:
						return GetFillerTimeline((DeformTimeline)timeline, skeletonData);

					// Skeleton
					case TimelineType.DrawOrder:
						return GetFillerTimeline((DrawOrderTimeline)timeline, skeletonData);

					// IK Constraint
					case TimelineType.IkConstraint:
						return GetFillerTimeline((IkConstraintTimeline)timeline, skeletonData);

					// TransformConstraint
					case TimelineType.TransformConstraint:
						return GetFillerTimeline((TransformConstraintTimeline)timeline, skeletonData);

					// Path Constraint
					case TimelineType.PathConstraintPosition:
						return GetFillerTimeline((PathConstraintPositionTimeline)timeline, skeletonData);
					case TimelineType.PathConstraintSpacing:
						return GetFillerTimeline((PathConstraintSpacingTimeline)timeline, skeletonData);
					case TimelineType.PathConstraintMix:
						return GetFillerTimeline((PathConstraintMixTimeline)timeline, skeletonData);
				}

				return null;
			}

			static RotateTimeline GetFillerTimeline (RotateTimeline timeline, SkeletonData skeletonData) {
				var t = new RotateTimeline(1);
				t.boneIndex = timeline.boneIndex;
				t.SetFrame(0, 0, 0);
				return t;
			}

			static TranslateTimeline GetFillerTimeline (TranslateTimeline timeline, SkeletonData skeletonData) {
				var t = new TranslateTimeline(1);
				t.boneIndex = timeline.boneIndex;
				t.SetFrame(0, 0, 0, 0);
				return t;
			}

			static ScaleTimeline GetFillerTimeline (ScaleTimeline timeline, SkeletonData skeletonData) {
				var t = new ScaleTimeline(1);
				t.boneIndex = timeline.boneIndex;
				t.SetFrame(0, 0, 0, 0);
				return t;
			}

			static ShearTimeline GetFillerTimeline (ShearTimeline timeline, SkeletonData skeletonData) {
				var t = new ShearTimeline(1);
				t.boneIndex = timeline.boneIndex;
				t.SetFrame(0, 0, 0, 0);
				return t;
			}

			static AttachmentTimeline GetFillerTimeline (AttachmentTimeline timeline, SkeletonData skeletonData) {
				var t = new AttachmentTimeline(1);
				t.slotIndex = timeline.slotIndex;
				var slotData = skeletonData.slots.Items[t.slotIndex];
				t.SetFrame(0, 0, slotData.attachmentName);
				return t;
			}

			static ColorTimeline GetFillerTimeline (ColorTimeline timeline, SkeletonData skeletonData) {
				var t = new ColorTimeline(1);
				t.slotIndex = timeline.slotIndex;
				var slotData = skeletonData.slots.Items[t.slotIndex];
				t.SetFrame(0, 0, slotData.r, slotData.g, slotData.b, slotData.a);
				return t;
			}

			static TwoColorTimeline GetFillerTimeline (TwoColorTimeline timeline, SkeletonData skeletonData) {
				var t = new TwoColorTimeline(1);
				t.slotIndex = timeline.slotIndex;
				var slotData = skeletonData.slots.Items[t.slotIndex];
				t.SetFrame(0, 0, slotData.r, slotData.g, slotData.b, slotData.a, slotData.r2, slotData.g2, slotData.b2);
				return t;
			}

			static DeformTimeline GetFillerTimeline (DeformTimeline timeline, SkeletonData skeletonData) {
				var t = new DeformTimeline(1);
				t.slotIndex = timeline.slotIndex;
				t.attachment = timeline.attachment;

				if (t.attachment.IsWeighted()) {
					t.SetFrame(0, 0, new float[t.attachment.vertices.Length]);
				} else {
					t.SetFrame(0, 0, t.attachment.vertices.Clone() as float[]);
				}

				return t;
			}

			static DrawOrderTimeline GetFillerTimeline (DrawOrderTimeline timeline, SkeletonData skeletonData) {
				var t = new DrawOrderTimeline(1);
				t.SetFrame(0, 0, null); // null means use setup pose in DrawOrderTimeline.Apply.
				return t;
			}

			static IkConstraintTimeline GetFillerTimeline (IkConstraintTimeline timeline, SkeletonData skeletonData) {
				var t = new IkConstraintTimeline(1);
				var ikConstraintData = skeletonData.ikConstraints.Items[timeline.ikConstraintIndex];
				t.SetFrame(0, 0, ikConstraintData.mix, ikConstraintData.bendDirection, ikConstraintData.compress, ikConstraintData.stretch);
				return t;
			}

			static TransformConstraintTimeline GetFillerTimeline (TransformConstraintTimeline timeline, SkeletonData skeletonData) {
				var t = new TransformConstraintTimeline(1);
				var data = skeletonData.transformConstraints.Items[timeline.transformConstraintIndex];
				t.SetFrame(0, 0, data.rotateMix, data.translateMix, data.scaleMix, data.shearMix);
				return t;
			}

			static PathConstraintPositionTimeline GetFillerTimeline (PathConstraintPositionTimeline timeline, SkeletonData skeletonData) {
				var t = new PathConstraintPositionTimeline(1);
				var data = skeletonData.pathConstraints.Items[timeline.pathConstraintIndex];
				t.SetFrame(0, 0, data.position);
				return t;
			}

			static PathConstraintSpacingTimeline GetFillerTimeline (PathConstraintSpacingTimeline timeline, SkeletonData skeletonData) {
				var t = new PathConstraintSpacingTimeline(1);
				var data = skeletonData.pathConstraints.Items[timeline.pathConstraintIndex];
				t.SetFrame(0, 0, data.spacing);
				return t;
			}

			static PathConstraintMixTimeline GetFillerTimeline (PathConstraintMixTimeline timeline, SkeletonData skeletonData) {
				var t = new PathConstraintMixTimeline(1);
				var data = skeletonData.pathConstraints.Items[timeline.pathConstraintIndex];
				t.SetFrame(0, 0, data.rotateMix, data.translateMix);
				return t;
			}
			#endregion
		}

	}

}
