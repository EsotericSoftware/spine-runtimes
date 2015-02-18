/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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
		if (!valid)
			return;

		if (layerMixModes.Length != animator.layerCount) {
			System.Array.Resize<MixMode>(ref layerMixModes, animator.layerCount);
		}

		skeleton.Update(Time.deltaTime);

		//apply
		int layerCount = animator.layerCount;
		float deltaTime = Time.deltaTime;
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
	}
}
