/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

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

	skeletonNode = [CCSkeleton skeletonWithFile:@"spineboy.json" atlasFile:@"spineboy.atlas"];
	AnimationStateData_setMixByName(skeletonNode->state->data, "walk", "jump", 0.4f);
	AnimationStateData_setMixByName(skeletonNode->state->data, "jump", "walk", 0.4f);
	AnimationState_setAnimationByName(skeletonNode->state, "walk", true);
	skeletonNode->timeScale = 0.3f;
	skeletonNode->debugBones = true;

	CGSize windowSize = [[CCDirector sharedDirector] winSize];
	[skeletonNode setPosition:ccp(windowSize.width / 2, 20)];
	[self addChild:skeletonNode];

	[self scheduleUpdate];

#if __CC_PLATFORM_MAC
	[self setMouseEnabled:YES];
#endif

	return self;
}

- (void) update:(ccTime)delta {
    if (skeletonNode->state->loop) {
        if (skeletonNode->state->time > 2) AnimationState_setAnimationByName(skeletonNode->state, "jump", false);
    } else {
        if (skeletonNode->state->time > 1) AnimationState_setAnimationByName(skeletonNode->state, "walk", true);
    }
}

#if __CC_PLATFORM_MAC
- (BOOL) ccMouseDown:(NSEvent*)event {
	CCDirector* director = [CCDirector sharedDirector];
	NSPoint location =  [director convertEventToGL:event];
	location.x -= [[director runningScene]position].x;
	location.y -= [[director runningScene]position].y;
	location.x -= skeletonNode.position.x;
	location.y -= skeletonNode.position.y;
	if (CGRectContainsPoint(skeletonNode.boundingBox, location)) NSLog(@"Clicked!");
	return YES;
}
#endif

@end
