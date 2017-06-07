#include "HelloWorldScene.h"
#include "SimpleAudioEngine.h"
#include "CCSpineNode.h"

using namespace cocos2d;
using namespace CocosDenshion;

CCScene* HelloWorld::scene()
{
    // 'scene' is an autorelease object
    CCScene *scene = CCScene::create();
    
    // 'layer' is an autorelease object
    HelloWorld *layer = HelloWorld::create();

    // add layer as a child to scene
    scene->addChild(layer);

    // return the scene
    return scene;
}

// on "init" you need to initialize your instance
bool HelloWorld::init()
{
    //////////////////////////////
    // 1. super init first
    if ( !CCLayer::init() )
    {
        return false;
    }
    
    CCSpineNode* spineNode = CCSpineNode::createWithFileNames("./data/spineboy-skeleton.json", "./data/spineboy.atlas");
    
    spineNode->playAnimation("./data/spineboy-walk.json");
    
    CCSize winSize = CCDirector::sharedDirector()->getWinSize();
    spineNode->setPosition(ccp(winSize.width/2, 0));
    
    
    addChild(spineNode);

    
    return true;
}
