
#import "SpineboyExample.h"
#import "GoblinsExample.h"

@implementation SpineboyExample

+ (CCScene*) scene {
	CCScene *scene = [CCScene node];
	[scene addChild:[SpineboyExample node]];
	return scene;
}

-(id) init {
	self = [super initWithColor:ccc4(128, 128, 128, 255)];
	if (!self) return nil;

	skeletonNode = [SkeletonAnimation skeletonWithFile:@"spineboy.json" atlasFile:@"spineboy.atlas" scale:0.6];
	[skeletonNode setMixFrom:@"walk" to:@"jump" duration:0.2f];
	[skeletonNode setMixFrom:@"jump" to:@"run" duration:0.2f];

	skeletonNode.startListener = ^(int trackIndex) {
		spTrackEntry* entry = spAnimationState_getCurrent(skeletonNode.state, trackIndex);
		const char* animationName = (entry && entry->animation) ? entry->animation->name : 0;
		CCLOG(@"%d start: %s", trackIndex, animationName);
	};
	skeletonNode.endListener = ^(int trackIndex) {
		CCLOG(@"%d end", trackIndex);
	};
	skeletonNode.completeListener = ^(int trackIndex, int loopCount) {
		CCLOG(@"%d complete: %d", trackIndex, loopCount);
	};
	skeletonNode.eventListener = ^(int trackIndex, spEvent* event) {
		CCLOG(@"%d event: %s, %d, %f, %s", trackIndex, event->data->name, event->intValue, event->floatValue, event->stringValue);
	};

	[skeletonNode setAnimationForTrack:0 name:@"walk" loop:YES];
	spTrackEntry* jumpEntry = [skeletonNode addAnimationForTrack:0 name:@"jump" loop:NO afterDelay:3];
	[skeletonNode addAnimationForTrack:0 name:@"run" loop:YES afterDelay:0];

	[skeletonNode setListenerForEntry:jumpEntry onStart:^(int trackIndex) {
		CCLOG(@"jumped!");
	}];

	// [skeletonNode setAnimationForTrack:1 name:@"test" loop:YES];

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
		[[CCDirector sharedDirector] replaceScene:[GoblinsExample scene]];
#if __CC_PLATFORM_MAC
	return YES;
#endif
}

@end
