/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#import <spine/CCSkeletonAnimation.h>
#import <spine/spine-cocos2d-iphone.h>

static void callback (AnimationState* state, int trackIndex, EventType type, Event* event, int loopCount) {
	[(CCSkeletonAnimation*)state->context onAnimationStateEvent:trackIndex type:type event:event loopCount:loopCount];
}

@interface CCSkeletonAnimation (Private)
- (void) initialize;
@end

@implementation CCSkeletonAnimation

@synthesize state = _state;

+ (id) skeletonWithData:(SkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData {
	return [[[self alloc] initWithData:skeletonData ownsSkeletonData:ownsSkeletonData] autorelease];
}

+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlas:(Atlas*)atlas scale:(float)scale {
	return [[[self alloc] initWithFile:skeletonDataFile atlas:atlas scale:scale] autorelease];
}

+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale {
	return [[[self alloc] initWithFile:skeletonDataFile atlasFile:atlasFile scale:scale] autorelease];
}

- (void) initialize {
	_ownsAnimationStateData = true;
	_state = AnimationState_create(AnimationStateData_create(_skeleton->data));
	_state->context = self;
	_state->listener = callback;
}

- (id) initWithData:(SkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData {
	self = [super initWithData:skeletonData ownsSkeletonData:ownsSkeletonData];
	if (!self) return nil;
	
	[self initialize];
	
	return self;
}

- (id) initWithFile:(NSString*)skeletonDataFile atlas:(Atlas*)atlas scale:(float)scale {
	self = [super initWithFile:skeletonDataFile atlas:atlas scale:scale];
	if (!self) return nil;
	
	[self initialize];
	
	return self;
}

- (id) initWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale {
	self = [super initWithFile:skeletonDataFile atlasFile:atlasFile scale:scale];
	if (!self) return nil;
	
	[self initialize];
	
	return self;
}

- (void) dealloc {
	if (_ownsAnimationStateData) AnimationStateData_dispose(_state->data);
	AnimationState_dispose(_state);

	[super dealloc];
}

- (void) update:(ccTime)deltaTime {
	[super update:deltaTime];

	deltaTime *= _timeScale;
	AnimationState_update(_state, deltaTime);
	AnimationState_apply(_state, _skeleton);
	Skeleton_updateWorldTransform(_skeleton);
}

- (void) setAnimationStateData:(AnimationStateData*)stateData {
	NSAssert(stateData, @"stateData cannot be null.");
	
	if (_ownsAnimationStateData) AnimationStateData_dispose(_state->data);
	AnimationState_dispose(_state);

	_ownsAnimationStateData = false;
	_state = AnimationState_create(stateData);
	_state->context = self;
	_state->listener = callback;
}

- (void) setMixFrom:(NSString*)fromAnimation to:(NSString*)toAnimation duration:(float)duration {
	AnimationStateData_setMixByName(_state->data, [fromAnimation UTF8String], [toAnimation UTF8String], duration);
}

- (void) setDelegate:(id<CCSkeletonAnimationDelegate>)delegate {
	_delegate = delegate;
	_delegateStart = [delegate respondsToSelector:@selector(animationDidStart:track:)];
	_delegateEnd = [delegate respondsToSelector:@selector(animationWillEnd:track:)];
	_delegateEvent = [delegate respondsToSelector:@selector(animationDidTriggerEvent:track:event:)];
	_delegateComplete = [delegate respondsToSelector:@selector(animationDidComplete:track:loopCount:)];
}

- (TrackEntry*) setAnimationForTrack:(int)trackIndex name:(NSString*)name loop:(bool)loop {
	Animation* animation = SkeletonData_findAnimation(_skeleton->data, [name UTF8String]);
	if (!animation) {
		CCLOG(@"Spine: Animation not found: %@", name);
		return 0;
	}
	return AnimationState_setAnimation(_state, trackIndex, animation, loop);
}

- (TrackEntry*) addAnimationForTrack:(int)trackIndex name:(NSString*)name loop:(bool)loop afterDelay:(int)delay {
	Animation* animation = SkeletonData_findAnimation(_skeleton->data, [name UTF8String]);
	if (!animation) {
		CCLOG(@"Spine: Animation not found: %@", name);
		return 0;
	}
	return AnimationState_addAnimation(_state, trackIndex, animation, loop, delay);
}

- (TrackEntry*) getCurrentForTrack:(int)trackIndex {
	return AnimationState_getCurrent(_state, trackIndex);
}

- (void) clearTracks {
	AnimationState_clearTracks(_state);
}

- (void) clearTrack:(int)trackIndex {
	AnimationState_clearTrack(_state, trackIndex);
}

- (void) onAnimationStateEvent:(int)trackIndex type:(EventType)type event:(Event*)event loopCount:(int)loopCount {
	if (!_delegate) return;
	switch (type) {
		case ANIMATION_START:
			if (_delegateStart) [_delegate animationDidStart:self track:trackIndex];
			break;
		case ANIMATION_END:
			if (_delegateEnd) [_delegate animationWillEnd:self track:trackIndex];
			break;
		case ANIMATION_COMPLETE:
			if (_delegateComplete) [_delegate animationDidComplete:self track:trackIndex loopCount:loopCount];
			break;
		case ANIMATION_EVENT:
			if (_delegateEvent) [_delegate animationDidTriggerEvent:self track:trackIndex event:event];
			break;
	}
}

@end
