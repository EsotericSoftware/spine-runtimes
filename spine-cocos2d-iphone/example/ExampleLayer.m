

#import "ExampleLayer.h"

@implementation ExampleLayer

+ (CCScene*) scene {
	CCScene *scene = [CCScene node];
	[scene addChild:[ExampleLayer node]];
	return scene;
}

-(id) init {
	self = [super init];
	if (!self) return nil;

	animationNode = [CCSkeletonAnimation skeletonWithFile:@"spineboy.json" atlasFile:@"spineboy.atlas" scale:1];
	[animationNode setMixFrom:@"walk" to:@"jump" duration:0.2f];
	[animationNode setMixFrom:@"jump" to:@"walk" duration:0.4f];
	[animationNode setDelegate:self];
	[animationNode setAnimationForTrack:0 name:@"walk" loop:NO];
	[animationNode addAnimationForTrack:0 name:@"jump" loop:NO afterDelay:0];
	[animationNode addAnimationForTrack:0 name:@"walk" loop:YES afterDelay:0];
	[animationNode addAnimationForTrack:0 name:@"jump" loop:YES afterDelay:4];
	[animationNode setAnimationForTrack:1 name:@"drawOrder" loop:YES];
	animationNode.timeScale = 0.3f;
	animationNode.debugBones = true;

	CGSize windowSize = [[CCDirector sharedDirector] winSize];
	[animationNode setPosition:ccp(windowSize.width / 2, 20)];
	[self addChild:animationNode];

#if __CC_PLATFORM_MAC
	[self setMouseEnabled:YES];
#endif

	return self;
}

- (void) animationDidStart:(CCSkeletonAnimation*)animation track:(int)trackIndex {
	CCLOG(@"%d start: %s", trackIndex, AnimationState_getCurrent(animation.state, trackIndex)->animation->name);
}

- (void) animationWillEnd:(CCSkeletonAnimation*)animation track:(int)trackIndex {
	CCLOG(@"%d end: %s", trackIndex, AnimationState_getCurrent(animation.state, trackIndex)->animation->name);
}

- (void) animationDidTriggerEvent:(CCSkeletonAnimation*)animation track:(int)trackIndex event:(Event*)event {
	CCLOG(@"%d event: %s, %s: %d, %f, %s", trackIndex, AnimationState_getCurrent(animation.state, trackIndex)->animation->name,
			event->data->name, event->intValue, event->floatValue, event->stringValue);
}

- (void) animationDidComplete:(CCSkeletonAnimation*)animation track:(int)trackIndex loopCount:(int)loopCount {
	CCLOG(@"%d complete: %s, %d", trackIndex, AnimationState_getCurrent(animation.state, trackIndex)->animation->name, loopCount);
}

#if __CC_PLATFORM_MAC
- (BOOL) ccMouseDown:(NSEvent*)event {
	CCDirector* director = [CCDirector sharedDirector];
	NSPoint location =  [director convertEventToGL:event];
	location.x -= [[director runningScene]position].x;
	location.y -= [[director runningScene]position].y;
	location.x -= animationNode.position.x;
	location.y -= animationNode.position.y;
	if (CGRectContainsPoint(animationNode.boundingBox, location)) NSLog(@"Clicked!");
	return YES;
}
#endif

@end