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

#if UNITY_2017 || UNITY_2018
 using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

using Spine.Unity;

namespace Spine.Unity.Playables {
	
	[TrackColor(0.855f, 0.8623f, 0.87f)]
	[TrackClipType(typeof(SpineSkeletonFlipClip))]
	[TrackBindingType(typeof(SpinePlayableHandleBase))]
	public class SpineSkeletonFlipTrack : TrackAsset {
		public override Playable CreateTrackMixer (PlayableGraph graph, GameObject go, int inputCount) {
			return ScriptPlayable<SpineSkeletonFlipMixerBehaviour>.Create(graph, inputCount);
		}

		public override void GatherProperties (PlayableDirector director, IPropertyCollector driver) {
#if UNITY_EDITOR
			SpinePlayableHandleBase trackBinding = director.GetGenericBinding(this) as SpinePlayableHandleBase;
			if (trackBinding == null)
				return;

			var serializedObject = new UnityEditor.SerializedObject(trackBinding);
			var iterator = serializedObject.GetIterator();
			while (iterator.NextVisible(true)) {
				if (iterator.hasVisibleChildren)
					continue;

				driver.AddFromName<SpinePlayableHandleBase>(trackBinding.gameObject, iterator.propertyPath);
			}
#endif
			base.GatherProperties(director, driver);
		}
	}
}
#endif