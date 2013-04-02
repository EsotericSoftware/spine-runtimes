
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

    // Load atlas, skeleton, and animations.
	atlas = Atlas_readAtlasFile("spineboy.atlas");
	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = 0.75;
	skeletonData = SkeletonJson_readSkeletonDataFile(json, "spineboy-skeleton.json");
	walkAnimation = SkeletonJson_readAnimationFile(json, "spineboy-walk.json", skeletonData);
	jumpAnimation = SkeletonJson_readAnimationFile(json, "spineboy-jump.json", skeletonData);
	SkeletonJson_dispose(json);
    
    // Configure mixing.
    AnimationStateData* stateData = AnimationStateData_create();
    AnimationStateData_setMix(stateData, walkAnimation, jumpAnimation, 0.4f);
    AnimationStateData_setMix(stateData, jumpAnimation, walkAnimation, 0.4f);

	skeletonNode = [CCSkeleton create:skeletonData stateData:stateData];
	Skeleton_setToBindPose(skeletonNode->skeleton);
	AnimationState_setAnimation(skeletonNode->state, walkAnimation, true);
    skeletonNode->timeScale = 0.15f;
	skeletonNode->debugBones = true;
    
	CGSize windowSize = [[CCDirector sharedDirector] winSize];
	[skeletonNode setPosition:ccp(windowSize.width / 2, 20)];
    [self addChild:skeletonNode];

    [self scheduleUpdate];
    
	return self;
}

- (void) update:(ccTime)delta {
    if (skeletonNode->state->animation == walkAnimation) {
        if (skeletonNode->state->time > 2) AnimationState_setAnimation(skeletonNode->state, jumpAnimation, false);
    } else {
        if (skeletonNode->state->time > 1) AnimationState_setAnimation(skeletonNode->state, walkAnimation, true);
    }
}

- (void) dealloc {
    Animation_dispose(walkAnimation);
    Animation_dispose(jumpAnimation);
	SkeletonData_dispose(skeletonData);
	Atlas_dispose(atlas);

    [super dealloc];
}

@end
