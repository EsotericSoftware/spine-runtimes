
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

	skeletonNode = [CCSkeleton create:@"spineboy.json" atlasFile:@"spineboy.atlas"];
	[skeletonNode setMix:@"walk" to:@"jump" duration:0.4f];
	[skeletonNode setMix:@"jump" to:@"walk" duration:0.4f];
	[skeletonNode setAnimation:@"walk" loop:true];
	skeletonNode->timeScale = 0.3f;
	skeletonNode->debugBones = true;

	CGSize windowSize = [[CCDirector sharedDirector] winSize];
	[skeletonNode setPosition:ccp(windowSize.width / 2, 20)];
	[self addChild:skeletonNode];

	[self scheduleUpdate];

	return self;
}

- (void) update:(ccTime)delta {
    if (strcmp(skeletonNode->state->animation->name, "walk") == 0) {
        if (skeletonNode->state->time > 2) [skeletonNode setAnimation:@"jump" loop:false];
    } else {
        if (skeletonNode->state->time > 1) [skeletonNode setAnimation:@"walk" loop:true];
    }
}

@end
