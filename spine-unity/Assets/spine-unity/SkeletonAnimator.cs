

/*****************************************************************************
 * SkeletonAnimator created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine;

[RequireComponent(typeof(Animator))]
public class SkeletonAnimator : SkeletonRenderer, ISkeletonAnimation {

	public enum MixMode { AlwaysMix, MixNext, SpineStyle }
	public MixMode[] layerMixModes = new MixMode[0];

	public Skeleton GetSkeleton () {
		return this.skeleton;

	}
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

	public Skeleton Skeleton {
		get {
			return this.skeleton;
		}
	}

	Dictionary<string, Spine.Animation> animationTable = new Dictionary<string, Spine.Animation>();
	Animator animator;
	float lastTime;

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

		lastTime = Time.time;
	}

	void Update () {
		if (!valid)
			return;

		if (layerMixModes.Length != animator.layerCount) {
			System.Array.Resize<MixMode>(ref layerMixModes, animator.layerCount);
		}
		float deltaTime = Time.time - lastTime;

		skeleton.Update(Time.deltaTime);

		//apply
		int layerCount = animator.layerCount;
		
		for (int i = 0; i < layerCount; i++) {

			float layerWeight = animator.GetLayerWeight(i);
			if (i == 0)
				layerWeight = 1;

			var stateInfo = animator.GetCurrentAnimatorStateInfo(i);
			var nextStateInfo = animator.GetNextAnimatorStateInfo(i);

#if UNITY_5
			var clipInfo = animator.GetCurrentAnimatorClipInfo(i);
			var nextClipInfo = animator.GetNextAnimatorClipInfo(i);
#else
			var clipInfo = animator.GetCurrentAnimationClipState(i);
			var nextClipInfo = animator.GetNextAnimationClipState(i);
#endif
			MixMode mode = layerMixModes[i];

			if (mode == MixMode.AlwaysMix) {
				//always use Mix instead of Applying the first non-zero weighted clip
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
			} else if (mode >= MixMode.MixNext) {
				//apply first non-zero weighted clip
				int c = 0;

				for (; c < clipInfo.Length; c++) {
					var info = clipInfo[c];
					float weight = info.weight * layerWeight;
					if (weight == 0)
						continue;

					float time = stateInfo.normalizedTime * info.clip.length;
					animationTable[info.clip.name].Apply(skeleton, Mathf.Max(0, time - deltaTime), time, stateInfo.loop, null);
					break;
				}

				//mix the rest
				for (; c < clipInfo.Length; c++) {
					var info = clipInfo[c];
					float weight = info.weight * layerWeight;
					if (weight == 0)
						continue;

					float time = stateInfo.normalizedTime * info.clip.length;
					animationTable[info.clip.name].Mix(skeleton, Mathf.Max(0, time - deltaTime), time, stateInfo.loop, null, weight);
				}

				c = 0;

				//apply next clip directly instead of mixing (ie:  no crossfade, ignores mecanim transition weights)
				if (mode == MixMode.SpineStyle) {
					for (; c < nextClipInfo.Length; c++) {
						var info = nextClipInfo[c];
						float weight = info.weight * layerWeight;
						if (weight == 0)
							continue;

						float time = nextStateInfo.normalizedTime * info.clip.length;
						animationTable[info.clip.name].Apply(skeleton, Mathf.Max(0, time - deltaTime), time, nextStateInfo.loop, null);
						break;
					}
				}

				//mix the rest
				for (; c < nextClipInfo.Length; c++) {
					var info = nextClipInfo[c];
					float weight = info.weight * layerWeight;
					if (weight == 0)
						continue;

					float time = nextStateInfo.normalizedTime * info.clip.length;
					animationTable[info.clip.name].Mix(skeleton, Mathf.Max(0, time - deltaTime), time, nextStateInfo.loop, null, weight);
				}
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

		lastTime = Time.time;
	}
}