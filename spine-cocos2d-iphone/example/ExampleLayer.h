
#import "cocos2d.h"
#import <spine/spine-cocos2d-iphone.h>

@interface ExampleLayer : CCLayer {
	Atlas *atlas;
	SkeletonData *skeletonData;
	Animation *walkAnimation;
	Animation *jumpAnimation;
	CCSkeleton* skeletonNode;
}

+(CCScene*) scene;

@end
