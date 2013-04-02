
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

	atlas = Atlas_readAtlasFile("spineboy.atlas");
	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = 0.75;
	skeletonData = SkeletonJson_readSkeletonDataFile(json, "spineboy-skeleton.json");
	animation = SkeletonJson_readAnimationFile(json, "spineboy-walk.json", skeletonData);
	SkeletonJson_dispose(json);
    
	CCSkeleton* skeletonNode = [CCSkeleton create:skeletonData];
	Skeleton_setToBindPose(skeletonNode->skeleton);
	AnimationState_setAnimation(skeletonNode->state, animation, true);
	skeletonNode->debugBones = true;
    
	CGSize windowSize = [[CCDirector sharedDirector] winSize];
	[skeletonNode setPosition:ccp(windowSize.width / 2, 20)];
    [self addChild:skeletonNode];

	return self;
}

@end
