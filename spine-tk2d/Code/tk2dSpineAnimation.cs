using UnityEngine;
using Spine;

// TODO: add events in animation component

[RequireComponent(typeof(tk2dSpineSkeleton))]
public class tk2dSpineAnimation : MonoBehaviour {
	
	public string animationName;
	public bool loop;
	public float animationSpeed = 1;
	public Spine.AnimationState state;
	
	private tk2dSpineSkeleton spineSkeleton;
	
	void Start () {
		spineSkeleton = GetComponent<tk2dSpineSkeleton>();
		state = new Spine.AnimationState(spineSkeleton.skeletonDataAsset.GetAnimationStateData());
	}
	
	void Update () {
		UpdateAnimation();
	}
	
	private void UpdateAnimation() {
		// Check if we need to stop current animation
		if (state.Animation != null && animationName == null) {
			state.ClearAnimation();
		} else if (state.Animation == null || animationName != state.Animation.Name) {
			// Check for different animation name or animation end
			Spine.Animation animation = spineSkeleton.skeleton.Data.FindAnimation(animationName);
			if (animation != null) state.SetAnimation(animation,loop);
		}
		
		state.Loop = loop;
		
		// Update animation
		spineSkeleton.skeleton.Update(Time.deltaTime * animationSpeed);
		state.Update(Time.deltaTime * animationSpeed);
		state.Apply(spineSkeleton.skeleton);
	}
}
