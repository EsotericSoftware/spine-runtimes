/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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
