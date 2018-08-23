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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity.Modules {
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
