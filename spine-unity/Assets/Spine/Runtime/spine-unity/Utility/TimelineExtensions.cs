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

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Spine.Unity.AnimationTools {
	public static class TimelineExtensions {

		/// <summary>Evaluates the resulting value of a TranslateTimeline at a given time.
		/// SkeletonData can be accessed from Skeleton.Data or from SkeletonDataAsset.GetSkeletonData.
		/// If no SkeletonData is given, values are computed relative to setup pose instead of local-absolute.</summary>
		public static Vector2 Evaluate (this TranslateTimeline timeline, float time, SkeletonData skeletonData = null) {
			const int PREV_TIME = -3, PREV_X = -2, PREV_Y = -1;
			const int X = 1, Y = 2;

			var frames = timeline.frames;
			if (time < frames[0]) return Vector2.zero;

			float x, y;
			if (time >= frames[frames.Length - TranslateTimeline.ENTRIES]) { // Time is after last frame.
				x = frames[frames.Length + PREV_X];
				y = frames[frames.Length + PREV_Y];
			}
			else {
				// Interpolate between the previous frame and the current frame.
				int frame = Animation.BinarySearch(frames, time, TranslateTimeline.ENTRIES);
				x = frames[frame + PREV_X];
				y = frames[frame + PREV_Y];
				float frameTime = frames[frame];
				float percent = timeline.GetCurvePercent(frame / TranslateTimeline.ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				x += (frames[frame + X] - x) * percent;
				y += (frames[frame + Y] - y) * percent;
			}

			Vector2 xy = new Vector2(x, y);
			if (skeletonData == null) {
				return xy;
			}
			else {
				var boneData = skeletonData.bones.Items[timeline.boneIndex];
				return xy + new Vector2(boneData.x, boneData.y);
			}
		}

		/// <summary>Gets the translate timeline for a given boneIndex.
		/// You can get the boneIndex using SkeletonData.FindBoneIndex.
		/// The root bone is always boneIndex 0.
		/// This will return null if a TranslateTimeline is not found.</summary>
		public static TranslateTimeline FindTranslateTimelineForBone (this Animation a, int boneIndex) {
			foreach (var timeline in a.timelines) {
				if (timeline.GetType().IsSubclassOf(typeof(TranslateTimeline)))
					continue;

				var translateTimeline = timeline as TranslateTimeline;
				if (translateTimeline != null && translateTimeline.boneIndex == boneIndex)
					return translateTimeline;
			}
			return null;
		}
	}
}
