using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine;

[RequireComponent(typeof(Animator))]
public class SkeletonAnimator : SkeletonRenderer, ISkeletonAnimation {

	public event UpdateBonesDelegate UpdateLocal {
		add { _UpdateLocal += value; }
		remove { _UpdateLocal -= value; }
	}

	public event UpdateBonesDelegate UpdateWorld {
		add { _UpdateWorld += value; }
		remove { _UpdateWorld -= value; }
	}

	public event UpdateBonesDelegate UpdateComplete {
		add { _UpdateComplete += value; }
		remove { _UpdateComplete -= value; }
	}

	protected event UpdateBonesDelegate _UpdateLocal;
	protected event UpdateBonesDelegate _UpdateWorld;
	protected event UpdateBonesDelegate _UpdateComplete;

	Dictionary<string, Spine.Animation> animationTable = new Dictionary<string, Spine.Animation>();
	Animator animator;

	public override void Reset () {
		base.Reset();
		if (!valid)
			return;

		animationTable.Clear();

		var data = skeletonDataAsset.GetSkeletonData(true);

		foreach (var a in data.Animations) {
			animationTable.Add(a.Name, a);
		}

		animator = GetComponent<Animator>();
	}

	void Update () {
		if (skeleton == null)
			return;

		skeleton.Update(Time.deltaTime);

		//apply
		int layerCount = animator.layerCount;
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < layerCount; i++) {

			float layerWeight = animator.GetLayerWeight(i);
			if (i == 0)
				layerWeight = 1;

			var stateInfo = animator.GetCurrentAnimatorStateInfo(i);
			var clipInfo = animator.GetCurrentAnimationClipState(i);
			var nextStateInfo = animator.GetNextAnimatorStateInfo(i);
			var nextClipInfo = animator.GetNextAnimationClipState(i);

			foreach (var info in clipInfo) {
				float weight = info.weight * layerWeight;
				if (weight == 0)
					continue;

				float time = stateInfo.normalizedTime * info.clip.length;
				animationTable[info.clip.name].Mix(skeleton, Mathf.Max(0, time - deltaTime), time, stateInfo.loop, null, weight);
			}

			foreach (var info in nextClipInfo) {
				float weight = info.weight * layerWeight;
				if (weight == 0)
					continue;

				float time = nextStateInfo.normalizedTime * info.clip.length;
				animationTable[info.clip.name].Mix(skeleton, Mathf.Max(0, time - deltaTime), time, nextStateInfo.loop, null, weight);
			}
		}

		if (_UpdateLocal != null)
			_UpdateLocal(this);

		skeleton.UpdateWorldTransform();

		if (_UpdateWorld != null) {
			_UpdateWorld(this);
			skeleton.UpdateWorldTransform();
		}

		if (_UpdateComplete != null) {
			_UpdateComplete(this);
		}
	}
}
