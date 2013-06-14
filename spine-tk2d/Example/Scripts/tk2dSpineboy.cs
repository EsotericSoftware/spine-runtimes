using UnityEngine;
using System.Collections;

/*
 */
public class tk2dSpineboy : MonoBehaviour {
	private tk2dSpineAnimation skeleton;
	
	void Start() {
		skeleton = GetComponent<tk2dSpineAnimation>();
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
