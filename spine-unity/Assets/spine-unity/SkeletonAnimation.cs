/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using UnityEngine;

namespace Spine.Unity {
	
	[ExecuteInEditMode]
	[AddComponentMenu("Spine/SkeletonAnimation")]
	[HelpURL("http://esotericsoftware.com/spine-unity-documentation#Controlling-Animation")]
	public class SkeletonAnimation : SkeletonRenderer, ISkeletonAnimation, Spine.Unity.IAnimationStateComponent {

		/// <summary>
		/// This is the Spine.AnimationState object of this SkeletonAnimation. You can control animations through it. 
		/// Note that this object, like .skeleton, is not guaranteed to exist in Awake. Do all accesses and caching to it in Start</summary>
		public Spine.AnimationState state;
		public Spine.AnimationState AnimationState { get { return this.state; } }

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

		[SerializeField]
		[SpineAnimation]
		private String _animationName;
		public String AnimationName {
			get {
				if (!valid) {
					Debug.LogWarning("You tried access AnimationName but the SkeletonAnimation was not valid. Try checking your Skeleton Data for errors.");
					return null;
				}

				TrackEntry entry = state.GetCurrent(0);
				return entry == null ? null : entry.Animation.Name;
			}
			set {
				if (_animationName == value)
					return;
				_animationName = value;

				if (!valid) {
					Debug.LogWarning("You tried to change AnimationName but the SkeletonAnimation was not valid. Try checking your Skeleton Data for errors.");
					return;
				}

				if (string.IsNullOrEmpty(value))
					state.ClearTrack(0);
				else
					state.SetAnimation(0, value, loop);
			}
		}

		/// <summary>Whether or not an animation should loop. This only applies to the initial animation specified in the inspector, or any subsequent Animations played through .AnimationName. Animations set through state.SetAnimation are unaffected.</summary>
		[Tooltip("Whether or not an animation should loop. This only applies to the initial animation specified in the inspector, or any subsequent Animations played through .AnimationName. Animations set through state.SetAnimation are unaffected.")]
		public bool loop;

		/// <summary>
		/// The rate at which animations progress over time. 1 means 100%. 0.5 means 50%.</summary>
		/// <remarks>AnimationState and TrackEntry also have their own timeScale. These are combined multiplicatively.</remarks>
		[Tooltip("The rate at which animations progress over time. 1 means 100%. 0.5 means 50%.")]
		public float timeScale = 1;

		#region Runtime Instantiation
		/// <summary>Adds and prepares a SkeletonAnimation component to a GameObject at runtime.</summary>
		/// <returns>The newly instantiated SkeletonAnimation</returns>
		public static SkeletonAnimation AddToGameObject (GameObject gameObject, SkeletonDataAsset skeletonDataAsset) {
			return SkeletonRenderer.AddSpineComponent<SkeletonAnimation>(gameObject, skeletonDataAsset);
		}

		/// <summary>Instantiates a new UnityEngine.GameObject and adds a prepared SkeletonAnimation component to it.</summary>
		/// <returns>The newly instantiated SkeletonAnimation component.</returns>
		public static SkeletonAnimation NewSkeletonAnimationGameObject (SkeletonDataAsset skeletonDataAsset) {
			return SkeletonRenderer.NewSpineGameObject<SkeletonAnimation>(skeletonDataAsset);
		}
		#endregion

		public override void Initialize (bool overwrite) {
			if (valid && !overwrite)
				return;

			base.Initialize(overwrite);

			if (!valid)
				return;

			state = new Spine.AnimationState(skeletonDataAsset.GetAnimationStateData());

			#if UNITY_EDITOR
			if (!string.IsNullOrEmpty(_animationName)) {
				if (Application.isPlaying) {
					state.SetAnimation(0, _animationName, loop);
				} else {
					// Assume SkeletonAnimation is valid for skeletonData and skeleton. Checked above.
					var animationObject = skeletonDataAsset.GetSkeletonData(false).FindAnimation(_animationName);
					if (animationObject != null)
						animationObject.Apply(skeleton, 0f, 0f, false, null);
				}
				Update(0);
			}
			#else
			if (!string.IsNullOrEmpty(_animationName)) {
				state.SetAnimation(0, _animationName, loop);
				Update(0);
			}
			#endif
		}

		public virtual void Update () {
			Update(Time.deltaTime);
		}

		public virtual void Update (float deltaTime) {
			if (!valid)
				return;

			deltaTime *= timeScale;
			skeleton.Update(deltaTime);
			state.Update(deltaTime);
			state.Apply(skeleton);

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

}
