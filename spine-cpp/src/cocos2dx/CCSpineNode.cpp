//
//  CCSpineNode.cpp
//  SpineExample
//
//  Created by wu shengjin on 13-3-18.
//
//

#include "CCSpineNode.h"
#include <iostream>
#include <fstream>

#define FULL_PATH(a) CCFileUtils::sharedFileUtils()->fullPathFromRelativePath(a)


CCSpineNode* CCSpineNode::createWithFileNames(const char* skeletonFileName, const char* atlasFileName)
{
    CCSpineNode* node = new CCSpineNode();
    if (node && node->initWithFiles(skeletonFileName, atlasFileName)) {
        node->autorelease();
        return node;
    }
    
    CC_SAFE_DELETE(node);
    return NULL;
}

bool CCSpineNode::initWithFiles(const char* skeletonFileName, const char* atlasFileName)
{
    std::ifstream atlasFile(FULL_PATH(atlasFileName));
    Atlas *atlas = new Atlas(atlasFile);
    
    m_skeletonJson = new SkeletonJson(atlas);
    
    std::ifstream skeletonFile(FULL_PATH(skeletonFileName));
    SkeletonData *skeletonData = m_skeletonJson->readSkeletonData(skeletonFile);
    
    
    m_skeleton = new Skeleton(skeletonData);
    m_skeleton->setToBindPose();
    m_skeleton->updateWorldTransform();
    
    m_animTimer = 0.0f;
    m_animation = 0;
    m_loop = true;
    
    setShaderProgram(CCShaderCache::sharedShaderCache()->programForKey(kCCShader_PositionTextureColor));
    
    scheduleUpdate();
    
    return true;
}

void CCSpineNode::playAnimation(const char* fileName, bool loop)
{
    CC_SAFE_DELETE(m_animation);
    
    std::ifstream animationFile(FULL_PATH(fileName));
    m_animation = m_skeletonJson->readAnimation(animationFile, m_skeleton->data);
    m_skeleton->setToBindPose();

    m_animTimer = 0.0f;
    m_loop = loop;
}

void CCSpineNode::update(float dt)
{
    if (m_animation && m_skeleton)
    {
        m_animTimer += dt;

        m_animation->apply(m_skeleton, m_animTimer, m_loop);
        m_skeleton->updateWorldTransform();        
    }
}

void CCSpineNode::draw()
{
    CC_NODE_DRAW_SETUP();
   
    if (m_skeleton)
        m_skeleton->draw();
    
    CCNode::draw();
}
