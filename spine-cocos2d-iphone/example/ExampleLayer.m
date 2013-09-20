

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
	[animationNode setAnimation:@"walk" loop:NO];
	[animationNode addAnimation:@"jump" loop:NO afterDelay:0];
	[animationNode addAnimation:@"walk" loop:YES afterDelay:0];
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