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
