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

#import <spine/SkeletonAnimation.h>
#import <spine/spine-cocos2d-iphone.h>
#import <spine/extension.h>

static void animationCallback (AnimationState* state, int trackIndex, EventType type, Event* event, int loopCount) {
	[(SkeletonAnimation*)state->rendererObject onAnimationStateEvent:trackIndex type:type event:event loopCount:loopCount];
}

void trackEntryCallback (spAnimationState* state, int trackIndex, spEventType type, spEvent* event, int loopCount) {
	[(SkeletonAnimation*)state->rendererObject onTrackEntryEvent:trackIndex type:type event:event loopCount:loopCount];
}

typedef struct _TrackEntryListeners {
	StartListener startListener;
	EndListener endListener;
	CompleteListener completeListener;
	EventListener eventListener;
} _TrackEntryListeners;

static _TrackEntryListeners* getListeners (spTrackEntry* entry) {
	if (!entry->rendererObject) {
		entry->rendererObject = NEW(_TrackEntryListeners);
		entry->listener = trackEntryCallback;
	}
	return (_TrackEntryListeners*)entry->rendererObject;
}

void disposeTrackEntry (spTrackEntry* entry) {
	if (entry->rendererObject) {
		_TrackEntryListeners* listeners = (_TrackEntryListeners*)entry->rendererObject;
		[listeners->startListener release];
		[listeners->endListener release];
		[listeners->completeListener release];
		[listeners->eventListener release];
		FREE(listeners);
	}
	_spTrackEntry_dispose(entry);
}

//

@interface SkeletonAnimation (Private)
- (void) initialize;
@end

@implementation SkeletonAnimation

@synthesize state = _state;
@synthesize startListener = _startListener;
@synthesize endListener = _endListener;
@synthesize completeListener = _completeListener;
@synthesize eventListener = _eventListener;

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
	_state->rendererObject = self;
	_state->listener = animationCallback;

	_spAnimationState* stateInternal = (_spAnimationState*)_state;
	stateInternal->disposeTrackEntry = disposeTrackEntry;
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

	[_startListener release];
	[_endListener release];
	[_completeListener release];
	[_eventListener release];

	[super dealloc];
}

- (void) update:(CCTime)deltaTime {
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
	_state->rendererObject = self;
	_state->listener = animationCallback;
}

- (void) setMixFrom:(NSString*)fromAnimation to:(NSString*)toAnimation duration:(float)duration {
	AnimationStateData_setMixByName(_state->data, [fromAnimation UTF8String], [toAnimation UTF8String], duration);
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
	switch (type) {
	case SP_ANIMATION_START:
		if (_startListener) _startListener(trackIndex);
		break;
	case SP_ANIMATION_END:
		if (_endListener) _endListener(trackIndex);
		break;
	case SP_ANIMATION_COMPLETE:
		if (_completeListener) _completeListener(trackIndex, loopCount);
		break;
	case SP_ANIMATION_EVENT:
		if (_eventListener) _eventListener(trackIndex, event);
		break;
	}
}

- (void) onTrackEntryEvent:(int)trackIndex type:(EventType)type event:(Event*)event loopCount:(int)loopCount {
	spTrackEntry* entry = spAnimationState_getCurrent(_state, trackIndex);
	if (!entry->rendererObject) return;
	_TrackEntryListeners* listeners = (_TrackEntryListeners*)entry->rendererObject;
	switch (type) {
	case SP_ANIMATION_START:
		if (listeners->startListener) listeners->startListener(trackIndex);
		break;
	case SP_ANIMATION_END:
		if (listeners->endListener) listeners->endListener(trackIndex);
		break;
	case SP_ANIMATION_COMPLETE:
		if (listeners->completeListener) listeners->completeListener(trackIndex, loopCount);
		break;
	case SP_ANIMATION_EVENT:
		if (listeners->eventListener) listeners->eventListener(trackIndex, event);
		break;
	}
}

- (void) setListenerForEntry:(spTrackEntry*)entry onStart:(StartListener)listener {
	getListeners(entry)->startListener = [listener copy];
}

- (void) setListenerForEntry:(spTrackEntry*)entry onEnd:(EndListener)listener {
	getListeners(entry)->endListener = [listener copy];
}

- (void) setListenerForEntry:(spTrackEntry*)entry onComplete:(CompleteListener)listener {
	getListeners(entry)->completeListener = [listener copy];
}

- (void) setListenerForEntry:(spTrackEntry*)entry onEvent:(EventListener)listener {
	getListeners(entry)->eventListener = [listener copy];
}

@end
