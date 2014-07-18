#region File Description
//-----------------------------------------------------------------------------
// SkeletalAnimation
//
// Copyright © 2014 Wave Corporation
// Use is subject to license terms.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Spine;
using System;
using WaveEngine.Common.Helpers;
using WaveEngine.Framework;
#endregion

namespace WaveEngine.Spine
{
    /// <summary>
    /// Behavior to control skeletal 2D animations
    /// </summary>
    public class SkeletalAnimation : Behavior
    {
        /// <summary>
        /// Event raised when an animation has finalized.
        /// </summary>
        public event AnimationState.StartEndDelegate EndAnimation;

        /// <summary>
        /// The skeletal data
        /// </summary>
        [RequiredComponent]
        public SkeletalData SkeletalData;

        /// <summary>
        /// The skeleton
        /// </summary>
        public Skeleton Skeleton;

        /// <summary>
        /// The state
        /// </summary>
        private AnimationState state;

        /// <summary>
        /// The animation path
        /// </summary>
        private string animationPath;

        /// <summary>
        /// The current skin
        /// </summary>
        private string currentSkin;

        #region Properties

        /// <summary>
        /// Gets or sets the speed.
        /// </summary>
        /// <value>
        /// The speed.
        /// </value>
        public float Speed { get; set; }

        /// <summary>
        /// Gets or sets the current animation.
        /// </summary>
        /// <value>
        /// The current animation.
        /// </value>
        public string CurrentAnimation { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public AnimationState State
        {
            get 
            { 
                return this.state; 
            }

            set 
            {
                if (this.state != null)
                {
                    this.state.End -= this.OnEndAnimation;
                }

                this.state = value;

                if (this.state != null)
                {
                    this.state.End -= this.OnEndAnimation;
                    this.state.End += this.OnEndAnimation;
                }
            }
        }

        /// <summary>
        /// Gets or sets the skin.
        /// </summary>
        /// <value>
        /// The skin.
        /// </value>
        public string Skin
        {
            set 
            {
                this.currentSkin = value;

                if (this.isInitialized)
                {
                    this.Skeleton.SetSkin(value);
                }
            }

            get
            {
                return this.currentSkin;
            }
        }

        #endregion

        #region Initialize
        /// <summary>
        /// Initializes a new instance of the <see cref="SkeletalAnimation" /> class.
        /// </summary>
        /// <param name="animationPath">The animation path.</param>
        public SkeletalAnimation(string animationPath)
            : base("SkeletalAnimation")
        {
            this.animationPath = animationPath;
            this.Speed = 1;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Plays this instance.
        /// </summary>
        public void Play()
        {
            this.state.SetAnimation(0, this.CurrentAnimation, false);
        }

        /// <summary>
        /// Plays this instance.
        /// </summary>
        /// <param name="mixDuration">Mix duration.</param>
        public void Play(float mixDuration)
        {
            // TODO: Use the mixDuration parameter
            this.state.SetAnimation(0, this.CurrentAnimation, false);
        }

        /// <summary>
        /// Plays the specified loop.
        /// </summary>
        /// <param name="loop">if set to <c>true</c> [loop].</param>
        /// <param name="mixDuration">Mix duration.</param>
        public void Play(bool loop, float mixDuration)
        {
            // TODO: Use the mixDuration parameter
            this.state.SetAnimation(0, this.CurrentAnimation, loop);
        }

        /// <summary>
        /// Plays the specified loop.
        /// </summary>
        /// <param name="loop">if set to <c>true</c> [loop].</param>
        public void Play(bool loop)
        {
            this.state.SetAnimation(0, this.CurrentAnimation, loop);
        }

        /// <summary>
        /// Search if the skeletal animation contains 
        /// </summary>
        /// <param name="animation">Animation name</param>
        /// <returns>Returns true if the skeletal animation contains the animation. False otherwise.</returns>
        public bool ContainsAnimation(string animation)
        {
            return this.state.Data.SkeletonData.FindAnimation(animation) != null;
        }

        /// <summary>
        /// Stops the current animation.
        /// </summary>
        public void Stop()
        {
            this.state.ClearTracks();
            this.Skeleton.Update(0);
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Resolves the dependencies needed for this instance to work.
        /// </summary>
        protected override void ResolveDependencies()
        {
            base.ResolveDependencies();
        }

        /// <summary>
        /// Performs further custom initialization for this instance.
        /// </summary>
        /// <remarks>
        /// By default this method does nothing.
        /// </remarks>
        protected override void Initialize()
        {
            base.Initialize();

            SkeletonJson json = new SkeletonJson(this.SkeletalData.Atlas);
            this.Skeleton = new Skeleton(json.ReadSkeletonData(this.animationPath));

            if (string.IsNullOrEmpty(this.currentSkin))
            {
                this.Skeleton.SetSkin(this.Skeleton.Data.DefaultSkin);
            }
            else
            {
                this.Skeleton.SetSkin(this.currentSkin);
            }

            AnimationStateData stateData = new AnimationStateData(this.Skeleton.Data);
            this.state = new AnimationState(stateData);
            this.state.End += this.OnEndAnimation;
        }

        /// <summary>
        /// Event handler of the end animation event.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="trackIndex">Index of the track.</param>
        private void OnEndAnimation(AnimationState state, int trackIndex)
        {
            if (this.EndAnimation != null)
            {
                this.EndAnimation(state, trackIndex);
            }
        }

        /// <summary>
        /// Allows this instance to execute custom logic during its <c>Update</c>.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        /// <remarks>
        /// This method will not be executed if the <see cref="T:WaveEngine.Framework.Component" />, or the <see cref="T:WaveEngine.Framework.Entity" />
        /// owning it are not <c>Active</c>.
        /// </remarks>
        protected override void Update(TimeSpan gameTime)
        {
            this.state.Update((float)gameTime.TotalSeconds * this.Speed);
            this.state.Apply(this.Skeleton);
            this.Skeleton.UpdateWorldTransform();
        }

        #endregion
    }
}
