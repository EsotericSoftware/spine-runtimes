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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {

	// This is an example of how you could store animation transitions for use in your animation system.
	// More ideally, this would be stored in a ScriptableObject in asset form rather than in a MonoBehaviour.
	public sealed class TransitionDictionaryExample : MonoBehaviour {

		[System.Serializable]
		public struct SerializedEntry {
			public AnimationReferenceAsset from;
			public AnimationReferenceAsset to;
			public AnimationReferenceAsset transition;
		}

		[SerializeField]
		List<SerializedEntry> transitions = new List<SerializedEntry>();
		readonly Dictionary<AnimationStateData.AnimationPair, Animation> dictionary = new Dictionary<AnimationStateData.AnimationPair, Animation>();

		void Start () {
			dictionary.Clear();
			foreach (SerializedEntry e in transitions) {
				dictionary.Add(new AnimationStateData.AnimationPair(e.from.Animation, e.to.Animation), e.transition.Animation);
			}
		}

		public Animation GetTransition (Animation from, Animation to) {
			Animation result;
			dictionary.TryGetValue(new AnimationStateData.AnimationPair(from, to), out result);
			return result;
		}
	}
}
