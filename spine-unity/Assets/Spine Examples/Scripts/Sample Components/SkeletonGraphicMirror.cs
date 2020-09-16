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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Examples {
	public class SkeletonGraphicMirror : MonoBehaviour {

		public SkeletonRenderer source;
		public bool mirrorOnStart = true;
		public bool restoreOnDisable = true;
		SkeletonGraphic skeletonGraphic;

		Skeleton originalSkeleton;
		bool originalFreeze;
		Texture2D overrideTexture;

		private void Awake () {
			skeletonGraphic = GetComponent<SkeletonGraphic>();
		}

		void Start () {
			if (mirrorOnStart)
				StartMirroring();
		}

		void LateUpdate () {
			skeletonGraphic.UpdateMesh();
		}

		void OnDisable () {
			if (restoreOnDisable)
				RestoreIndependentSkeleton();
		}

		/// <summary>Freeze the SkeletonGraphic on this GameObject, and use the source as the Skeleton to be rendered by the SkeletonGraphic.</summary>
		public void StartMirroring () {
			if (source == null)
				return;
			if (skeletonGraphic == null)
				return;

			skeletonGraphic.startingAnimation = string.Empty;

			if (originalSkeleton == null) {
				originalSkeleton = skeletonGraphic.Skeleton;
				originalFreeze = skeletonGraphic.freeze;
			}

			skeletonGraphic.Skeleton = source.skeleton;
			skeletonGraphic.freeze = true;
			if (overrideTexture != null)
				skeletonGraphic.OverrideTexture = overrideTexture;
		}

		/// <summary>Use a new texture for the SkeletonGraphic. Use this if your source skeleton uses a repacked atlas. </summary>
		public void UpdateTexture (Texture2D newOverrideTexture) {
			overrideTexture = newOverrideTexture;
			if (newOverrideTexture != null)
				skeletonGraphic.OverrideTexture = overrideTexture;
		}

		/// <summary>Stops mirroring the source SkeletonRenderer and allows the SkeletonGraphic to become an independent Skeleton component again.</summary>
		public void RestoreIndependentSkeleton () {
			if (originalSkeleton == null)
				return;

			skeletonGraphic.Skeleton = originalSkeleton;
			skeletonGraphic.freeze = originalFreeze;
			skeletonGraphic.OverrideTexture = null;

			originalSkeleton = null;
		}
	}

}
