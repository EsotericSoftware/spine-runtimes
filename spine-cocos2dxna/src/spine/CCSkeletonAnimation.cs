using Spine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpineCocosXna.Spine
{
    class CCSkeletonAnimation : CCSkeleton
    {

        public float UpdateDeltaTime = 1;

        AnimationState _state;
        bool _ownsAnimationStateData;

        public delegate void StartEndDelegate(AnimationState state, int trackIndex);
        public event StartEndDelegate Start;
        public event StartEndDelegate End;

        public delegate void EventDelegate(AnimationState state, int trackIndex, Event e);
        public event EventDelegate Event;

        public delegate void CompleteDelegate(AnimationState state, int trackIndex, int loopCount);
        public event CompleteDelegate Complete;

        public CCSkeletonAnimation()
        {
            this.initializer();

        }

        public CCSkeletonAnimation(SkeletonData skeletonData) : 
            base(skeletonData,false)
        {
            this.initializer();
        }

        public CCSkeletonAnimation(string skeletonDataFile, Atlas atlas, float scale)
            :base(skeletonDataFile,atlas,scale)
        {
            this.initializer();
        }

        public CCSkeletonAnimation(string skeletonDataFile, string atlasFile, float scale) 
            : base(skeletonDataFile,atlasFile,scale)
        {
           
           initializer();
        }

        
        public void setAnimationStateData (AnimationStateData stateData ) {
	         //NSAssert(stateData, @"stateData cannot be null.");
	        if (stateData!=null)

	        _ownsAnimationStateData = false;
	        _state = new AnimationState(stateData);
            _state.Event += state_Event;
            _state.Start += state_Start;
            _state.Complete += state_Complete;
            _state.End += state_End;

        }

        public override void Update(float dt)
        {
            base.Update(dt);

            dt *= UpdateDeltaTime;
            _state.Update(dt);
            _state.Apply(skeleton);
            updateWorldTransform();
        }

        public void setMix(string fromAnimation, string toAnimation, float duration)
        {
            _state.Data.SetMix(fromAnimation, toAnimation, duration);
        }

        ~CCSkeletonAnimation()
        {
        }

        void initializer()
        {
            _ownsAnimationStateData = true;

            _state = new AnimationState(new AnimationStateData(skeleton.Data)); //spAnimationState_create(spAnimationStateData_create(skeleton->data));
            _state.Event += state_Event;
            _state.Start += state_Start;
            _state.Complete += state_Complete;
            _state.End += state_End;
        }

        void state_End(AnimationState state, int trackIndex)
        {
            if (End != null)
                End(state, trackIndex);

        }

        private void state_Complete(AnimationState state, int trackIndex, int loopCount)
        {
            //spAnimationState_clearTrack

            if (Complete != null)
                Complete(state, trackIndex, loopCount);
        }

        void state_Start(AnimationState state, int trackIndex)
        {
            if (Start != null)
                Start(state, trackIndex);
        }

        void state_Event(AnimationState state, int trackIndex, Event e)
        {
            if (Event != null)
                Event(state, trackIndex, e);

        }

       public TrackEntry SetAnimation(int trackIndex, string name, bool loop)
        {

            Animation animation = skeleton.Data.FindAnimation(name);
            if (animation == null)
            {
                Console.WriteLine(string.Format("Spine: Animation not found: %s", name)); //CCLog("Spine: Animation not found: %s", name);
                return null;
            }
            return _state.SetAnimation(trackIndex, animation, loop);
        }

       public TrackEntry AddAnimation(int trackIndex, string name, bool loop, float delay)
        {
            Animation animation = skeleton.Data.FindAnimation(name);
            if (animation == null)
            {
                //CCLog("Spine: Animation not found: %s", name);
                return null;
            }
            return _state.AddAnimation(trackIndex, animation, loop, delay);
        }


    }
}
