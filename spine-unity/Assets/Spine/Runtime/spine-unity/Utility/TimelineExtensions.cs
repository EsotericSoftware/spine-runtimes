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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.AnimationTools {
	public static class TimelineExtensions {

		/// <summary>Evaluates the resulting value of a TranslateTimeline at a given time.
		/// SkeletonData can be accessed from Skeleton.Data or from SkeletonDataAsset.GetSkeletonData.
		/// If no SkeletonData is given, values are returned as difference to setup pose
		/// instead of absolute values.</summary>
		public static Vector2 Evaluate (this TranslateTimeline timeline, float time, SkeletonData skeletonData = null) {
			if (time < timeline.Frames[0]) return Vector2.zero;

			float x, y;
			timeline.GetCurveValue(out x, out y, time);

			if (skeletonData == null) {
				return new Vector2(x, y);
			} else {
				BoneData boneData = skeletonData.Bones.Items[timeline.BoneIndex];
				return new Vector2(boneData.X + x, boneData.Y + y);
			}
		}

		/// <summary>Evaluates the resulting value of a pair of split translate timelines at a given time.
		/// SkeletonData can be accessed from Skeleton.Data or from SkeletonDataAsset.GetSkeletonData.
		/// If no SkeletonData is given, values are returned as difference to setup pose
		/// instead of absolute values.</summary>
		public static Vector2 Evaluate (TranslateXTimeline xTimeline, TranslateYTimeline yTimeline,
			float time, SkeletonData skeletonData = null) {

			float x = 0, y = 0;
			if (xTimeline != null && time > xTimeline.Frames[0]) x = xTimeline.GetCurveValue(time);
			if (yTimeline != null && time > yTimeline.Frames[0]) y = yTimeline.GetCurveValue(time);

			if (skeletonData == null) {
				return new Vector2(x, y);
			} else {
				var bonesItems = skeletonData.Bones.Items;
				BoneData boneDataX = bonesItems[xTimeline.BoneIndex];
				BoneData boneDataY = bonesItems[yTimeline.BoneIndex];
				return new Vector2(boneDataX.X + x, boneDataY.Y + y);
			}
		}

		/// <summary>Gets the translate timeline for a given boneIndex.
		/// You can get the boneIndex using SkeletonData.FindBoneIndex.
		/// The root bone is always boneIndex 0.
		/// This will return null if a TranslateTimeline is not found.</summary>
		public static TranslateTimeline FindTranslateTimelineForBone (this Animation a, int boneIndex) {
			foreach (var timeline in a.Timelines) {
				if (timeline.GetType().IsSubclassOf(typeof(TranslateTimeline)))
					continue;

				var translateTimeline = timeline as TranslateTimeline;
				if (translateTimeline != null && translateTimeline.BoneIndex == boneIndex)
					return translateTimeline;
			}
			return null;
		}

		/// <summary>Gets the IBoneTimeline timeline of a given type for a given boneIndex.
		/// You can get the boneIndex using SkeletonData.FindBoneIndex.
		/// The root bone is always boneIndex 0.
		/// This will return null if a timeline of the given type is not found.</summary>
		public static T FindTimelineForBone<T> (this Animation a, int boneIndex) where T : class, IBoneTimeline {
			foreach (var timeline in a.Timelines) {
				T translateTimeline = timeline as T;
				if (translateTimeline != null && translateTimeline.BoneIndex == boneIndex)
					return translateTimeline;
			}
			return null;
		}
	}
}
