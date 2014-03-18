using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cocos2D;
namespace SpineCocosXna.Spine
{
    public class ExampleLayer : CCLayerColor
    {

        public static CCScene Scene
        {

            get{
                   var scene = new CCScene();
            scene.AddChild(new ExampleLayer());
            return scene;
            }
         
        }


        public ExampleLayer()
        {

            

        }


        /*
         * static cocos2d::CCScene* scene ();

	virtual bool init ();
	virtual void update (float deltaTime);

	CREATE_FUNC (ExampleLayer);
         * */

        //        spine::CCSkeletonAnimation* skeletonNode;
	
    //void animationStateEvent (spine::CCSkeletonAnimation* node, int trackIndex, spEventType type, spEvent* event, int loopCount);



    }
}
