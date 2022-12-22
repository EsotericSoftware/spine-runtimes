/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {

	//[CreateAssetMenu(menuName = "Spine/SkeletonData Modifiers/Animation Match", order = 200)]
	public class AnimationMatchModifierAsset : SkeletonDataModifierAsset {

		public bool matchAllAnimations = true;

		public override void Apply (SkeletonData skeletonData) {
			if (matchAllAnimations)
				AnimationTools.MatchAnimationTimelines(skeletonData.Animations, skeletonData);
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
				Dictionary<string, Timeline> timelineDictionary = new Dictionary<string, Spine.Timeline>();
				foreach (Animation animation in animations) {
					foreach (Timeline timeline in animation.Timelines) {
						if (timeline is EventTimeline) continue;

						foreach (string propertyId in timeline.PropertyIds) {
							if (!timelineDictionary.ContainsKey(propertyId)) {
								timelineDictionary.Add(propertyId, GetFillerTimeline(timeline, skeletonData));
							}
						}
					}
				}
				List<string> idsToMatch = new List<string>(timelineDictionary.Keys);

				// For each animation in the list, check for and add missing timelines.
				HashSet<string> currentAnimationIDs = new HashSet<string>();
				foreach (Animation animation in animations) {
					currentAnimationIDs.Clear();
					foreach (Timeline timeline in animation.Timelines) {
						if (timeline is EventTimeline) continue;
						foreach (string propertyId in timeline.PropertyIds) {
							currentAnimationIDs.Add(propertyId);
						}
					}

					ExposedList<Timeline> animationTimelines = animation.Timelines;
					foreach (string propertyId in idsToMatch) {
						if (!currentAnimationIDs.Contains(propertyId))
							animationTimelines.Add(timelineDictionary[propertyId]);
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
				if (timeline is RotateTimeline)
					return GetFillerTimeline((RotateTimeline)timeline, skeletonData);
				if (timeline is TranslateTimeline)
					return GetFillerTimeline((TranslateTimeline)timeline, skeletonData);
				if (timeline is ScaleTimeline)
					return GetFillerTimeline((ScaleTimeline)timeline, skeletonData);
				if (timeline is ShearTimeline)
					return GetFillerTimeline((ShearTimeline)timeline, skeletonData);
				if (timeline is AttachmentTimeline)
					return GetFillerTimeline((AttachmentTimeline)timeline, skeletonData);
				if (timeline is RGBATimeline)
					return GetFillerTimeline((RGBATimeline)timeline, skeletonData);
				if (timeline is RGBA2Timeline)
					return GetFillerTimeline((RGBA2Timeline)timeline, skeletonData);
				if (timeline is DeformTimeline)
					return GetFillerTimeline((DeformTimeline)timeline, skeletonData);
				if (timeline is DrawOrderTimeline)
					return GetFillerTimeline((DrawOrderTimeline)timeline, skeletonData);
				if (timeline is IkConstraintTimeline)
					return GetFillerTimeline((IkConstraintTimeline)timeline, skeletonData);
				if (timeline is TransformConstraintTimeline)
					return GetFillerTimeline((TransformConstraintTimeline)timeline, skeletonData);
				if (timeline is PathConstraintPositionTimeline)
					return GetFillerTimeline((PathConstraintPositionTimeline)timeline, skeletonData);
				if (timeline is PathConstraintSpacingTimeline)
					return GetFillerTimeline((PathConstraintSpacingTimeline)timeline, skeletonData);
				if (timeline is PathConstraintMixTimeline)
					return GetFillerTimeline((PathConstraintMixTimeline)timeline, skeletonData);
				return null;
			}

			static RotateTimeline GetFillerTimeline (RotateTimeline timeline, SkeletonData skeletonData) {
				RotateTimeline t = new RotateTimeline(1, 0, timeline.BoneIndex);
				t.SetFrame(0, 0, 0);
				return t;
			}

			static TranslateTimeline GetFillerTimeline (TranslateTimeline timeline, SkeletonData skeletonData) {
				TranslateTimeline t = new TranslateTimeline(1, 0, timeline.BoneIndex);
				t.SetFrame(0, 0, 0, 0);
				return t;
			}

			static ScaleTimeline GetFillerTimeline (ScaleTimeline timeline, SkeletonData skeletonData) {
				ScaleTimeline t = new ScaleTimeline(1, 0, timeline.BoneIndex);
				t.SetFrame(0, 0, 0, 0);
				return t;
			}

			static ShearTimeline GetFillerTimeline (ShearTimeline timeline, SkeletonData skeletonData) {
				ShearTimeline t = new ShearTimeline(1, 0, timeline.BoneIndex);
				t.SetFrame(0, 0, 0, 0);
				return t;
			}

			static AttachmentTimeline GetFillerTimeline (AttachmentTimeline timeline, SkeletonData skeletonData) {
				AttachmentTimeline t = new AttachmentTimeline(1, timeline.SlotIndex);
				SlotData slotData = skeletonData.Slots.Items[t.SlotIndex];
				t.SetFrame(0, 0, slotData.AttachmentName);
				return t;
			}

			static RGBATimeline GetFillerTimeline (RGBATimeline timeline, SkeletonData skeletonData) {
				RGBATimeline t = new RGBATimeline(1, 0, timeline.SlotIndex);
				SlotData slotData = skeletonData.Slots.Items[t.SlotIndex];
				t.SetFrame(0, 0, slotData.R, slotData.G, slotData.B, slotData.A);
				return t;
			}

			static RGBA2Timeline GetFillerTimeline (RGBA2Timeline timeline, SkeletonData skeletonData) {
				RGBA2Timeline t = new RGBA2Timeline(1, 0, timeline.SlotIndex);
				SlotData slotData = skeletonData.Slots.Items[t.SlotIndex];
				t.SetFrame(0, 0, slotData.R, slotData.G, slotData.B, slotData.A, slotData.R2, slotData.G2, slotData.B2);
				return t;
			}

			static DeformTimeline GetFillerTimeline (DeformTimeline timeline, SkeletonData skeletonData) {
				DeformTimeline t = new DeformTimeline(1, 0, timeline.SlotIndex, timeline.Attachment);
				if (t.Attachment.IsWeighted()) {
					t.SetFrame(0, 0, new float[t.Attachment.Vertices.Length]);
				} else {
					t.SetFrame(0, 0, t.Attachment.Vertices.Clone() as float[]);
				}

				return t;
			}

			static DrawOrderTimeline GetFillerTimeline (DrawOrderTimeline timeline, SkeletonData skeletonData) {
				DrawOrderTimeline t = new DrawOrderTimeline(1);
				t.SetFrame(0, 0, null); // null means use setup pose in DrawOrderTimeline.Apply.
				return t;
			}

			static IkConstraintTimeline GetFillerTimeline (IkConstraintTimeline timeline, SkeletonData skeletonData) {
				IkConstraintTimeline t = new IkConstraintTimeline(1, 0, timeline.IkConstraintIndex);
				IkConstraintData ikConstraintData = skeletonData.IkConstraints.Items[timeline.IkConstraintIndex];
				t.SetFrame(0, 0, ikConstraintData.Mix, ikConstraintData.Softness, ikConstraintData.BendDirection, ikConstraintData.Compress, ikConstraintData.Stretch);
				return t;
			}

			static TransformConstraintTimeline GetFillerTimeline (TransformConstraintTimeline timeline, SkeletonData skeletonData) {
				TransformConstraintTimeline t = new TransformConstraintTimeline(1, 0, timeline.TransformConstraintIndex);
				TransformConstraintData data = skeletonData.TransformConstraints.Items[timeline.TransformConstraintIndex];
				t.SetFrame(0, 0, data.MixRotate, data.MixX, data.MixY, data.MixScaleX, data.MixScaleY, data.MixShearY);
				return t;
			}

			static PathConstraintPositionTimeline GetFillerTimeline (PathConstraintPositionTimeline timeline, SkeletonData skeletonData) {
				PathConstraintPositionTimeline t = new PathConstraintPositionTimeline(1, 0, timeline.PathConstraintIndex);
				PathConstraintData data = skeletonData.PathConstraints.Items[timeline.PathConstraintIndex];
				t.SetFrame(0, 0, data.Position);
				return t;
			}

			static PathConstraintSpacingTimeline GetFillerTimeline (PathConstraintSpacingTimeline timeline, SkeletonData skeletonData) {
				PathConstraintSpacingTimeline t = new PathConstraintSpacingTimeline(1, 0, timeline.PathConstraintIndex);
				PathConstraintData data = skeletonData.PathConstraints.Items[timeline.PathConstraintIndex];
				t.SetFrame(0, 0, data.Spacing);
				return t;
			}

			static PathConstraintMixTimeline GetFillerTimeline (PathConstraintMixTimeline timeline, SkeletonData skeletonData) {
				PathConstraintMixTimeline t = new PathConstraintMixTimeline(1, 0, timeline.PathConstraintIndex);
				PathConstraintData data = skeletonData.PathConstraints.Items[timeline.PathConstraintIndex];
				t.SetFrame(0, 0, data.RotateMix, data.MixX, data.MixY);
				return t;
			}
			#endregion
		}

	}

}
