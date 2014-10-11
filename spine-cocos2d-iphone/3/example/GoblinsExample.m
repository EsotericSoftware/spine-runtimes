
#import "GoblinsExample.h"
#import "SpineboyExample.h"

@implementation GoblinsExample

+ (CCScene*) scene {
	CCScene *scene = [CCScene node];
	[scene addChild:[GoblinsExample node]];
	return scene;
}

-(id) init {
	self = [super init];
	if (!self) return nil;

	skeletonNode = [SkeletonAnimation skeletonWithFile:@"goblins-mesh.json" atlasFile:@"goblins-mesh.atlas" scale:1];
	[skeletonNode setSkin:@"goblin"];
	[skeletonNode setAnimationForTrack:0 name:@"walk" loop:YES];

	CGSize windowSize = [[CCDirector sharedDirector] viewSize];
	[skeletonNode setPosition:ccp(windowSize.width / 2, 20)];
	[self addChild:skeletonNode];

	self.userInteractionEnabled = YES;
    self.contentSize = windowSize;
    
	return self;
}

#if ( TARGET_OS_IPHONE || TARGET_IPHONE_SIMULATOR )
- (void)touchBegan:(UITouch *)touch withEvent:(UIEvent *)event {
	if (!skeletonNode.debugBones)
		skeletonNode.debugBones = true;
	else if (skeletonNode.timeScale == 1)
		skeletonNode.timeScale = 0.3f;
	else
		[[CCDirector sharedDirector] replaceScene:[SpineboyExample scene]];
}
#endif

@end
