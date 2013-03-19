//
//  CCSpineNode.h
//  SpineExample
//
//  Created by wu shengjin on 13-3-18.
//
//

#ifndef __CCSpineNode__
#define __CCSpineNode__

#include "cocos2d.h"
#include <spine-cc/spine.h>

using namespace spine;


USING_NS_CC;

class CCSpineNode : public CCNode {
public:
    CCSpineNode();
    virtual ~CCSpineNode();
    
    static CCSpineNode* createWithFileNames(const char* skeletonFileName, const char* atlasFileName);
    bool initWithFiles(const char* skeletonFileName, const char* atlasFileName);

    void playAnimation(const char* fileName, bool loop = true);

    void update(float dt);
    void draw();
    
    void setFlipX(bool flip);
    void setFlipY(bool flip);
    
    bool isCurrAnimOver() const;
private:
    SkeletonJson* m_skeletonJson;
    Skeleton* m_skeleton;
    Animation* m_animation;
    
    float m_animTimer;
    bool m_loop;
    std::string m_animName;
};

#endif /* defined(__SpineExample__CCSpineNode__) */
