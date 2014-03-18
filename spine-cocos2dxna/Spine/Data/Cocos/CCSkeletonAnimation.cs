using SharpDX.Direct3D9;
using Spine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpineCocosXna.Spine
{
    class CCSkeletonAnimation : CCSkeleton
    {

        AnimationState state;

        CCSkeleton super;
        object listenerInstance;
        //SEL_AnimationStateEvent listenerMethod;
        bool ownsAnimationStateData;

        public CCSkeletonAnimation()
            : base()
        {
            this.initializer();
        }

        public CCSkeletonAnimation(SkeletonData skeletonData)
            : base(skeletonData)
        {
            this.initializer();
        }

        public CCSkeletonAnimation(string skeletonDataFile, Atlas atlas, float scale)
            : base(skeletonDataFile, atlas, scale)
        {
            this.initializer();
        }

        public CCSkeletonAnimation(string skeletonDataFile, string atlasFile, float scale)
            : base(skeletonDataFile, atlasFile, scale)
        {
            initializer();
        }



        public void setMix(string fromAnimation, string toAnimation, float duration)
        {
            state.Data.SetMix(fromAnimation, toAnimation, duration);
            //spAnimationStateData_setMixByName(state.Data, fromAnimation, toAnimation, duration);
        }


        ~CCSkeletonAnimation()
        {
            //if (ownsAnimationStateData) spAnimationStateData_dispose(state->data);
            //spAnimationState_dispose(state);
        }

        //public CCSkeletonAnimation createWithData(SkeletonData skeletonData)
        //{

        //}

        void initializer()
        {
            listenerInstance = null;
            //listenerMethod = 0;

            ownsAnimationStateData = true;

            state = new AnimationState(new AnimationStateData(skeleton.Data)); //spAnimationState_create(spAnimationStateData_create(skeleton->data));
            //state. = this; CONTEXTO
            state.Event += state_Event;
            state.Start += state_Start;
            state.Complete += state_Complete;
            state.End += state_End;
            //state. listener = callback;

        }

        void state_End(AnimationState state, int trackIndex)
        {

        }

        private void state_Complete(AnimationState state, int trackIndex, int loopCount)
        {
            //spAnimationState_clearTrack
        }

        void state_Start(AnimationState state, int trackIndex)
        {

        }

        void state_Event(AnimationState state, int trackIndex, Event e)
        {

        }


       public    TrackEntry SetAnimation(int trackIndex, string name, bool loop)
        {

            Animation animation = skeleton.data.FindAnimation(name);
            if (animation == null)
            {
                Console.WriteLine(string.Format("Spine: Animation not found: %s", name)); //CCLog("Spine: Animation not found: %s", name);
                return null;
            }
            return state.SetAnimation(trackIndex, animation, loop);
        }

       public TrackEntry AddAnimation(int trackIndex, string name, bool loop, float delay)
        {
            Animation animation = skeleton.data.FindAnimation(name);
            if (animation != null)
            {
                //CCLog("Spine: Animation not found: %s", name);
                return null;
            }
            return state.AddAnimation(trackIndex, animation, loop, delay);
        }


    }
}
