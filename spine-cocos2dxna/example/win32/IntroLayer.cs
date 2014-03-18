using System;
using Cocos2D;
using Microsoft.Xna.Framework;
using SpineCocosXna.Spine;
using Spine;

namespace spine_cocos_xna
{
    class IntroLayer : CCNode
    {

        CCSkeletonAnimation skeletonNode;
        AnimationState state;
        Slot slot;


        public IntroLayer()
        {

            CCSize windowSize = CCDirector.SharedDirector.WinSize;

            String name = @"Content\goblins";
            skeletonNode = new CCSkeletonAnimation(name + ".json", name + ".atlas", 1.0f);

            skeletonNode.setSkin("goblin");

            skeletonNode.NodeToWorldTransform();
            skeletonNode.setSlotsToSetupPose();
            skeletonNode.updateWorldTransform();

            skeletonNode.AddAnimation(0, "walk", true, 4);
            skeletonNode.SetAnimation(0, "walk", true);

            skeletonNode.Start += Start;
            skeletonNode.End += End;
            skeletonNode.Complete += Complete;
            skeletonNode.Event += Event;

            skeletonNode.findSlot("head");

            skeletonNode.RunAction(new CCRepeatForever(new CCSequence(
                    new CCFadeOut(1), new CCFadeIn(1)
                    )));

            skeletonNode.RunAction(new CCRepeatForever(new CCSequence(
                new CCMoveTo(5, new CCPoint(900, 10)), new CCMoveTo(5, new CCPoint(10, 10))
                )));


            skeletonNode.SetPosition(windowSize.Width / 2, windowSize.Height / 2);
            AddChild(skeletonNode);

        }

        public void Start(AnimationState state, int trackIndex)
        {
            Console.WriteLine(trackIndex + " " + state.GetCurrent(trackIndex) + ": start");
        }

        public void End(AnimationState state, int trackIndex)
        {
            Console.WriteLine(trackIndex + " " + state.GetCurrent(trackIndex) + ": end");
        }

        public void Complete(AnimationState state, int trackIndex, int loopCount)
        {
            Console.WriteLine(trackIndex + " " + state.GetCurrent(trackIndex) + ": complete " + loopCount);
        }

        public void Event(AnimationState state, int trackIndex, Event e)
        {
            Console.WriteLine(trackIndex + " " + state.GetCurrent(trackIndex) + ": event " + e);
        }

        public static CCScene Scene
        {
            get
            {
                // 'scene' is an autorelease object.
                var scene = new CCScene();

                // 'layer' is an autorelease object.
                var layer = new IntroLayer();

                // add layer as a child to scene
                scene.AddChild(layer);

                // return the scene
                return scene;

            }

        }

    }
}

