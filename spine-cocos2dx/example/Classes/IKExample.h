//
//  IKExample.hpp
//  spine-cocos2d-x
//
//  Created by Mario Zechner on 28.10.19.
//

#ifndef _IKEXAMPLE_H_
#define _IKEXAMPLE_H_

#include "cocos2d.h"
#include <spine/spine-cocos2dx.h>

class IKExample : public cocos2d::LayerColor {
public:
    static cocos2d::Scene* scene ();

    CREATE_FUNC (IKExample);

    virtual bool init ();

    virtual void update (float deltaTime);

private:
    spine::SkeletonAnimation* skeletonNode;
	cocos2d::Vec2 position;
};

#endif // _IKEXAMPLE_H_
