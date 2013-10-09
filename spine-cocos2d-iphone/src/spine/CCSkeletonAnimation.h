/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
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
 *****************************************************************************/

#define SPINE_SHORT_NAMES
#import <spine/spine.h>
#import <spine/CCSkeleton.h>
#import "cocos2d.h"

@class CCSkeletonAnimation;

@protocol CCSkeletonAnimationDelegate <NSObject>
@optional
- (void) animationDidStart:(CCSkeletonAnimation*)animation track:(int)trackIndex;
- (void) animationWillEnd:(CCSkeletonAnimation*)animation track:(int)trackIndex;
- (void) animationDidTriggerEvent:(CCSkeletonAnimation*)animation track:(int)trackIndex event:(Event*)event;
- (void) animationDidComplete:(CCSkeletonAnimation*)animation track:(int)trackIndex loopCount:(int)loopCount;
@end

/** Draws an animated skeleton, providing an AnimationState for applying one or more animations and queuing animations to be
 * played later. */
@interface CCSkeletonAnimation : CCSkeleton {
	AnimationState* _state;
	bool _ownsAnimationStateData;

	id<CCSkeletonAnimationDelegate> _delegate;
	bool _delegateStart, _delegateEnd, _delegateEvent, _delegateComplete;
}

+ (id) skeletonWithData:(SkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData;
+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlas:(Atlas*)atlas scale:(float)scale;
+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale;

- (id) initWithData:(SkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData;
- (id) initWithFile:(NSString*)skeletonDataFile atlas:(Atlas*)atlas scale:(float)scale;
- (id) initWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale;

- (void) setAnimationStateData:(AnimationStateData*)stateData;
- (void) setMixFrom:(NSString*)fromAnimation to:(NSString*)toAnimation duration:(float)duration;

- (void) setDelegate:(id<CCSkeletonAnimationDelegate>)delegate;
- (TrackEntry*) setAnimationForTrack:(int)trackIndex name:(NSString*)name loop:(bool)loop;
- (TrackEntry*) addAnimationForTrack:(int)trackIndex name:(NSString*)name loop:(bool)loop afterDelay:(int)delay;
- (TrackEntry*) getCurrentForTrack:(int)trackIndex;
- (void) clearTracks;
- (void) clearTrack:(int)trackIndex;

- (void) onAnimationStateEvent:(int)trackIndex type:(EventType)type event:(Event*)event loopCount:(int)loopCount;

@property (nonatomic, readonly) AnimationState* state;

@end
