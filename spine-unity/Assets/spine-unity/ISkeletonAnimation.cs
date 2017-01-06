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

namespace Spine.Unity {
	public delegate void UpdateBonesDelegate (ISkeletonAnimation animated);

	/// <summary>A Spine-Unity Component that animates a Skeleton but not necessarily with a Spine.AnimationState.</summary>
	public interface ISkeletonAnimation {
		event UpdateBonesDelegate UpdateLocal;
		event UpdateBonesDelegate UpdateWorld;
		event UpdateBonesDelegate UpdateComplete;

		void LateUpdate ();
		Skeleton Skeleton { get; }
	}

	/// <summary>A Spine-Unity Component that manages a Spine.Skeleton instance, instantiated from a SkeletonDataAsset.</summary>
	public interface ISkeletonComponent {
		/// <summary>Gets the SkeletonDataAsset of the Spine Component.</summary>
		SkeletonDataAsset SkeletonDataAsset { get; }

		/// <summary>Gets the Spine.Skeleton instance of the Spine Component. This is equivalent to SkeletonRenderer's .skeleton.</summary>
		Skeleton Skeleton { get; }
	}

	/// <summary>A Spine-Unity Component that uses a Spine.AnimationState to animate its skeleton.</summary>
	public interface IAnimationStateComponent {
		/// <summary>Gets the Spine.AnimationState of the animated Spine Component. This is equivalent to SkeletonAnimation.state.</summary>
		AnimationState AnimationState { get; }
	}
}
