using UnityEngine;
using System.Collections;

public class Spineboy : MonoBehaviour {
	private SkeletonAnimation skeleton;
	
	void Start() {
		skeleton = GetComponent<SkeletonAnimation>();
	}
	
	void LateUpdate() {
		if (skeleton.loop) return;
		
		if (skeleton.state.Animation != null && skeleton.state.Time >= skeleton.state.Animation.Duration - 0.25) {
			skeleton.animationName = "walk";
			skeleton.loop = true;
		}
	}
	
	void OnMouseDown() {
		skeleton.animationName = "jump";
		skeleton.loop = false;
	}
}
