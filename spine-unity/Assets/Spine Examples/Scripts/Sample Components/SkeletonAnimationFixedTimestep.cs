/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
using UnityEngine;

namespace Spine.Unity {

	// To use this example component, add it to your SkeletonAnimation Spine GameObject.
	// This component will disable that SkeletonAnimation component to prevent it from calling its own Update and LateUpdate methods.

	[DisallowMultipleComponent]
	public sealed class SkeletonAnimationFixedTimestep : MonoBehaviour {
		#region Inspector
		public SkeletonAnimation skeletonAnimation;

		[Tooltip("The duration of each frame in seconds. For 12 fps: enter '1/12' in the Unity inspector.")]
		public float frameDeltaTime = 1 / 15f;

		[Header("Advanced")]
		[Tooltip("The maximum number of fixed timesteps. If the game framerate drops below the If the framerate is consistently faster than the limited frames, this does nothing.")]
		public int maxFrameSkip = 4;

		[Tooltip("If enabled, the Skeleton mesh will be updated only on the same frame when the animation and skeleton are updated. Disable this or call SkeletonAnimation.LateUpdate yourself if you are modifying the Skeleton using other components that don't run in the same fixed timestep.")]
		public bool frameskipMeshUpdate = true;

		[Tooltip("This is the amount the internal accumulator starts with. Set it to some fraction of your frame delta time if you want to stagger updates between multiple skeletons.")]
		public float timeOffset;
		#endregion

		float accumulatedTime = 0;
		bool requiresNewMesh;

		void OnValidate () {
			skeletonAnimation = GetComponent<SkeletonAnimation>();
			if (frameDeltaTime <= 0) frameDeltaTime = 1 / 60f;
			if (maxFrameSkip < 1) maxFrameSkip = 1;
		}

		void Awake () {
			requiresNewMesh = true;
			accumulatedTime = timeOffset;
		}

		void Update () {
			if (skeletonAnimation.enabled)
				skeletonAnimation.enabled = false;

			accumulatedTime += Time.deltaTime;

			float frames = 0;
			while (accumulatedTime >= frameDeltaTime) {
				frames++;
				if (frames > maxFrameSkip) break;
				accumulatedTime -= frameDeltaTime;
			}

			if (frames > 0) {
				skeletonAnimation.Update(frames * frameDeltaTime);
				requiresNewMesh = true;
			}	
		}

		void LateUpdate () {
			if (frameskipMeshUpdate && !requiresNewMesh) return;

			skeletonAnimation.LateUpdate();
			requiresNewMesh = false;
		}
	}
}

