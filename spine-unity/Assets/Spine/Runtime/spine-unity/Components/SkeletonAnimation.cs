/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

using UnityEngine;

namespace Spine.Unity {

	#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
	#else
	[ExecuteInEditMode]
	#endif
	[AddComponentMenu("Spine/SkeletonAnimation")]
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
				if (_animationName == value) {
					TrackEntry entry = state.GetCurrent(0);
					if (entry != null && entry.loop == loop)
						return;
				}
				_animationName = value;

				if (string.IsNullOrEmpty(value)) {
					state.ClearTrack(0);
				} else {
					var animationObject = skeletonDataAsset.GetSkeletonData(false).FindAnimation(value);
					if (animationObject != null)
						state.SetAnimation(0, animationObject, loop);
				}
			}
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

			if (!string.IsNullOrEmpty(_animationName)) {
				var animationObject = skeletonDataAsset.GetSkeletonData(false).FindAnimation(_animationName);
				if (animationObject != null) {
					state.SetAnimation(0, animationObject, loop);
					#if UNITY_EDITOR
					if (!Application.isPlaying)
						Update(0f);
					#endif
				}
			}
		}

		void Update () {
			#if UNITY_EDITOR
			if (!Application.isPlaying) {
				Update(0f);
				return;
			}
			#endif

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
