
#import "GoblinsExample.h"
#import "SpineboyExample.h"

@implementation GoblinsExample

+ (CCScene*) scene {
	CCScene *scene = [CCScene node];
	[scene addChild:[GoblinsExample node]];
	return scene;
}

-(id) init {
	self = [super initWithColor:ccc4(128, 128, 128, 255)];
	if (!self) return nil;

	skeletonNode = [SkeletonAnimation skeletonWithFile:@"goblins-mesh.json" atlasFile:@"goblins-mesh.atlas" scale:1];
	[skeletonNode setSkin:@"goblin"];
	[skeletonNode setAnimationForTrack:0 name:@"walk" loop:YES];

	CGSize windowSize = [[CCDirector sharedDirector] winSize];
	[skeletonNode setPosition:ccp(windowSize.width / 2, 20)];
	[self addChild:skeletonNode];

#if __CC_PLATFORM_MAC
	[self setMouseEnabled:YES];
#else
	[self setTouchEnabled:YES];
#endif

	return self;
}

#if __CC_PLATFORM_MAC
- (BOOL) ccMouseDown:(NSEvent*)event {
#else
- (void) ccTouchesBegan:(NSSet*)touches withEvent:(UIEvent*)event {
#endif
	if (!skeletonNode.debugBones)
		skeletonNode.debugBones = true;
	else if (skeletonNode.timeScale == 1)
		skeletonNode.timeScale = 0.3f;
	else
		[[CCDirector sharedDirector] replaceScene:[SpineboyExample scene]];
#if __CC_PLATFORM_MAC
	return YES;
#endif
}

@end
