/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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
