/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#import <spine/SkeletonAnimation.h>
#import <spine/spine-cocos2d-objc.h>
#import <spine/extension.h>

static void animationCallback (spAnimationState* state, int trackIndex, spEventType type, spEvent* event, int loopCount) {
	[(SkeletonAnimation*)state->rendererObject onAnimationStateEvent:trackIndex type:type event:event loopCount:loopCount];
}

void trackEntryCallback (spAnimationState* state, int trackIndex, spEventType type, spEvent* event, int loopCount) {
	[(SkeletonAnimation*)state->rendererObject onTrackEntryEvent:trackIndex type:type event:event loopCount:loopCount];
}

typedef struct _TrackEntryListeners {
	spStartListener startListener;
	spEndListener endListener;
	spCompleteListener completeListener;
	spEventListener eventListener;
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
@synthesize timeScale = _timeScale;
@synthesize startListener = _startListener;
@synthesize endListener = _endListener;
@synthesize completeListener = _completeListener;
@synthesize eventListener = _eventListener;

+ (id) skeletonWithData:(spSkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData {
	return [[[self alloc] initWithData:skeletonData ownsSkeletonData:ownsSkeletonData] autorelease];
}

+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlas:(spAtlas*)atlas scale:(float)scale {
	return [[[self alloc] initWithFile:skeletonDataFile atlas:atlas scale:scale] autorelease];
}

+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale {
	return [[[self alloc] initWithFile:skeletonDataFile atlasFile:atlasFile scale:scale] autorelease];
}

- (void) initialize {
	_ownsAnimationStateData = true;
    _timeScale = 1;

	_state = spAnimationState_create(spAnimationStateData_create(_skeleton->data));
	_state->rendererObject = self;
	_state->listener = animationCallback;

	_spAnimationState* stateInternal = (_spAnimationState*)_state;
	stateInternal->disposeTrackEntry = disposeTrackEntry;
}

- (id) initWithData:(spSkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData {
	self = [super initWithData:skeletonData ownsSkeletonData:ownsSkeletonData];
	if (!self) return nil;
	
	[self initialize];
	
	return self;
}

- (id) initWithFile:(NSString*)skeletonDataFile atlas:(spAtlas*)atlas scale:(float)scale {
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
	if (_ownsAnimationStateData) spAnimationStateData_dispose(_state->data);
	spAnimationState_dispose(_state);

	[_startListener release];
	[_endListener release];
	[_completeListener release];
	[_eventListener release];

	[super dealloc];
}

- (void) update:(CCTime)deltaTime {
	deltaTime *= _timeScale;
	spSkeleton_update(_skeleton, deltaTime);
	spAnimationState_update(_state, deltaTime);
	spAnimationState_apply(_state, _skeleton);
	spSkeleton_updateWorldTransform(_skeleton);
}

- (void) setAnimationStateData:(spAnimationStateData*)stateData {
	NSAssert(stateData, @"stateData cannot be null.");
	
	if (_ownsAnimationStateData) spAnimationStateData_dispose(_state->data);
	spAnimationState_dispose(_state);

	_ownsAnimationStateData = false;
	_state = spAnimationState_create(stateData);
	_state->rendererObject = self;
	_state->listener = animationCallback;
}

- (void) setMixFrom:(NSString*)fromAnimation to:(NSString*)toAnimation duration:(float)duration {
	spAnimationStateData_setMixByName(_state->data, [fromAnimation UTF8String], [toAnimation UTF8String], duration);
}

- (spTrackEntry*) setAnimationForTrack:(int)trackIndex name:(NSString*)name loop:(bool)loop {
	spAnimation* animation = spSkeletonData_findAnimation(_skeleton->data, [name UTF8String]);
	if (!animation) {
		CCLOG(@"Spine: Animation not found: %@", name);
		return 0;
	}
	return spAnimationState_setAnimation(_state, trackIndex, animation, loop);
}

- (spTrackEntry*) addAnimationForTrack:(int)trackIndex name:(NSString*)name loop:(bool)loop afterDelay:(int)delay {
	spAnimation* animation = spSkeletonData_findAnimation(_skeleton->data, [name UTF8String]);
	if (!animation) {
		CCLOG(@"Spine: Animation not found: %@", name);
		return 0;
	}
	return spAnimationState_addAnimation(_state, trackIndex, animation, loop, delay);
}

- (spTrackEntry*) getCurrentForTrack:(int)trackIndex {
	return spAnimationState_getCurrent(_state, trackIndex);
}

- (void) clearTracks {
	spAnimationState_clearTracks(_state);
}

- (void) clearTrack:(int)trackIndex {
	spAnimationState_clearTrack(_state, trackIndex);
}

- (void) onAnimationStateEvent:(int)trackIndex type:(spEventType)type event:(spEvent*)event loopCount:(int)loopCount {
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

- (void) onTrackEntryEvent:(int)trackIndex type:(spEventType)type event:(spEvent*)event loopCount:(int)loopCount {
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

- (void) setListenerForEntry:(spTrackEntry*)entry onStart:(spStartListener)listener {
	getListeners(entry)->startListener = [listener copy];
}

- (void) setListenerForEntry:(spTrackEntry*)entry onEnd:(spEndListener)listener {
	getListeners(entry)->endListener = [listener copy];
}

- (void) setListenerForEntry:(spTrackEntry*)entry onComplete:(spCompleteListener)listener {
	getListeners(entry)->completeListener = [listener copy];
}

- (void) setListenerForEntry:(spTrackEntry*)entry onEvent:(spEventListener)listener {
	getListeners(entry)->eventListener = [listener copy];
}

@end
