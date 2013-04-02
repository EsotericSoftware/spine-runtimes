
#import "cocos2d.h"
#import <spine/spine-cocos2d-iphone.h>

@interface ExampleLayer : CCLayer {
	Atlas *atlas;
	SkeletonData *skeletonData;
	Animation *animation;
}

+(CCScene*) scene;

@end
