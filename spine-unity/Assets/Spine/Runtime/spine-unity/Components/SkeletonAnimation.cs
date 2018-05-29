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
	
	[ExecuteInEditMode]
	[AddComponentMenu("Spine/SkeletonAnimation")]
	[HelpURL("http://esotericsoftware.com/spine-unity-documentation#Controlling-Animation")]
	public class SkeletonAnimation : SkeletonRenderer, ISkeletonAnimation, IAnimationStateComponent {

		#region IAnimationStateComponent
		/// <summary>
		/// This is the Spine.AnimationState object of this SkeletonAnimation. You can control animations through it. 
		/// Note that this object, like .skeleton, is not guaranteed to exist in Awake. Do all accesses and caching to it in Start</summary>
		public Spine.AnimationState state;
		/// <summary>
		/// This is the Spine.AnimationState object of this SkeletonAnimation. You can control animations through it. 
		/// Note that this object, like .skeleton, is not guaranteed to exist in Awake. Do all accesses and caching to it in Start</summary>
		public Spine.AnimationState AnimationState { get { return this.state; } }
		#endregion

		#region Bone Callbacks ISkeletonAnimation
		protected event UpdateBonesDelegate _UpdateLocal;
		protected event UpdateBonesDelegate _UpdateWorld;
		protected event UpdateBonesDelegate _UpdateComplete;

		/// <summary>
		/// Occurs after the animations are applied and before world space values are resolved.
		/// Use this callback when you want to set bone local values.
		/// </summary>
		public event UpdateBonesDelegate UpdateLocal { add { _UpdateLocal += value; } remove { _UpdateLocal -= value; } }

		/// <summary>
		/// Occurs after the Skeleton's bone world space values are resolved (including all constraints).
		/// Using this callback will cause the world space values to be solved an extra time.
		/// Use this callback if want to use bone world space values, and also set bone local values.</summary>
		public event UpdateBonesDelegate UpdateWorld { add { _UpdateWorld += value; } remove { _UpdateWorld -= value; } }

		/// <summary>
		/// Occurs after the Skeleton's bone world space values are resolved (including all constraints).
		/// Use this callback if you want to use bone world space values, but don't intend to modify bone local values.
		/// This callback can also be used when setting world position and the bone matrix.</summary>
		public event UpdateBonesDelegate UpdateComplete { add { _UpdateComplete += value; } remove { _UpdateComplete -= value; } }
		#endregion

		#region Serialized state and Beginner API
		[SerializeField]
		[SpineAnimation]
		private string _animationName;

		/// <summary>
		/// Setting this property sets the animation of the skeleton. If invalid, it will store the animation name for the next time the skeleton is properly initialized.
		/// Getting this property gets the name of the currently playing animation. If invalid, it will return the last stored animation name set through this property.</summary>
		public string AnimationName {
			get {
				if (!valid) {
					return _animationName;
				} else {
					TrackEntry entry = state.GetCurrent(0);
					return entry == null ? null : entry.Animation.Name;
				}
			}
			set {
				if (_animationName == value)
					return;
				_animationName = value;

				if (string.IsNullOrEmpty(value)) {
					state.ClearTrack(0);
				} else {
					TrySetAnimation(value, loop);
				}
			}
		}

		TrackEntry TrySetAnimation (string animationName, bool animationLoop) {
			var animationObject = skeletonDataAsset.GetSkeletonData(false).FindAnimation(animationName);
			if (animationObject != null)
				return state.SetAnimation(0, animationObject, animationLoop);

			return null;
		}

		/// <summary>Whether or not <see cref="AnimationName"/> should loop. This only applies to the initial animation specified in the inspector, or any subsequent Animations played through .AnimationName. Animations set through state.SetAnimation are unaffected.</summary>
		public bool loop;

		/// <summary>
		/// The rate at which animations progress over time. 1 means 100%. 0.5 means 50%.</summary>
		/// <remarks>AnimationState and TrackEntry also have their own timeScale. These are combined multiplicatively.</remarks>
		public float timeScale = 1;
		#endregion

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

		/// <summary>
		/// Clears the previously generated mesh, resets the skeleton's pose, and clears all previously active animations.</summary>
		public override void ClearState () {
			base.ClearState();
			if (state != null) state.ClearTracks();
		}

		/// <summary>
		/// Initialize this component. Attempts to load the SkeletonData and creates the internal Spine objects and buffers.</summary>
		/// <param name="overwrite">If set to <c>true</c>, force overwrite an already initialized object.</param>
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
					TrackEntry startingTrack = TrySetAnimation(_animationName, loop);
					if (startingTrack != null)
						Update(0);
				} else {
					// Assume SkeletonAnimation is valid for skeletonData and skeleton. Checked above.
					var animationObject = skeletonDataAsset.GetSkeletonData(false).FindAnimation(_animationName);
					if (animationObject != null)
						animationObject.PoseSkeleton(skeleton, 0f);
				}
			}
			#else
			if (!string.IsNullOrEmpty(_animationName)) {
				TrackEntry startingTrack = TrySetAnimation(_animationName, loop);
				if (startingTrack != null)
					Update(0);
			}
			#endif
		}

		void Update () {
			Update(Time.deltaTime);
		}

		/// <summary>Progresses the AnimationState according to the given deltaTime, and applies it to the Skeleton. Use Time.deltaTime to update manually. Use deltaTime 0 to update without progressing the time.</summary>
		public void Update (float deltaTime) {
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
