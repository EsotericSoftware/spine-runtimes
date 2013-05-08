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
