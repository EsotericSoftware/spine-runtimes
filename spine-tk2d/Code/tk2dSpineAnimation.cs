using UnityEngine;
using Spine;

// TODO: add events in animation component

[RequireComponent(typeof(tk2dSpineSkeleton))]
public class tk2dSpineAnimation : MonoBehaviour {
	
	public string animationName;
	public bool loop;
	public float animationSpeed = 1;
	public Spine.AnimationState state;
	
	private tk2dSpineSkeleton cachedSpineSkeleton;
	
	void Start () {
		cachedSpineSkeleton = GetComponent<tk2dSpineSkeleton>();
		state = new Spine.AnimationState(cachedSpineSkeleton.skeletonDataAsset.GetAnimationStateData());
	}
	
	void Update () {
		UpdateAnimation();
	}
	
	private void UpdateAnimation() {
		// Check if we need to stop current animation
		if (state.Animation != null && animationName == null) {
			state.ClearAnimation();
		}
		
		// Check for different animation name or animation end
		else if (state.Animation == null || animationName != state.Animation.Name) {
			Spine.Animation animation = cachedSpineSkeleton.skeleton.Data.FindAnimation(animationName);
			if (animation != null) state.SetAnimation(animation,loop);
		}
		
		state.Loop = loop;
		
		// Update animation
		cachedSpineSkeleton.skeleton.Update(Time.deltaTime * animationSpeed);
		state.Update(Time.deltaTime * animationSpeed);
		state.Apply(cachedSpineSkeleton.skeleton);
	}
}
