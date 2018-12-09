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
			foreach (var e in transitions) {
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
