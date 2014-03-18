using System;
using Cocos2D;
using Microsoft.Xna.Framework;
using SpineCocosXna.Spine;
using Spine;

namespace SpineCocosXna
{
    public class IntroLayer : CCLayerColor
    {
        public IntroLayer()
        {
            String name = "goblins"; //"spineboy"; // 

            CCSkeletonAnimation skeleton = new CCSkeletonAnimation(name + ".json", name + ".atlas", 1.0f);
            skeleton.updateWorldTransform();
            skeleton.UpdateTransform();
        
            if (name == "goblins") skeleton.setSkin("goblin");
            skeleton.setSlotsToSetupPose(); // Without this the skin attachments won't be attached. See SetSkin.
            //AnimationStateData stateData = new AnimationStateData(skeleton.);
            //if (name == "spineboy")
            //{
            //    stateData.SetMix("walk", "jump", 0.2f);
            //    stateData.SetMix("jump", "walk", 0.4f);
            //}

         
            //skeletonNode.debugBones = true;
            //skeleton.Update(0);

            //skeletonNode->runAction(CCRepeatForever::create(CCSequence::create(CCFadeOut::create(1),
            //    CCFadeIn::create(1),
            //    CCDelayTime::create(5),
            //    NULL)));

            CCSize windowSize = CCDirector.SharedDirector.WinSize;
            skeleton.SetPosition(windowSize.Width / 2, 20);
            AddChild(skeleton);


            // add the label as a child to this Layer
            //AddChild(label);

            // setup our color for the background
            Color = new CCColor3B(Microsoft.Xna.Framework.Color.Blue);
            Opacity = 255;
            ScheduleUpdate();

        }

        public override void Update(float dt)
        {
            base.Update(dt);
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

