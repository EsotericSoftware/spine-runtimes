

#import "cocos2d.h"
#import <spine/spine-cocos2d-iphone.h>

@interface ExampleLayer : CCLayer<CCSkeletonAnimationDelegate> {
	CCSkeletonAnimation* animationNode;
}

+(CCScene*) scene;

@end