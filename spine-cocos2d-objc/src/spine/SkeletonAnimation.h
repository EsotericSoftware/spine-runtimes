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

#import <spine/spine.h>
#import <spine/SkeletonRenderer.h>
#import "cocos2d.h"

@class SkeletonAnimation;

typedef void(^spStartListener)(spTrackEntry* entry);
typedef void(^spInterruptListener)(spTrackEntry* entry);
typedef void(^spEndListener)(spTrackEntry* entry);
typedef void(^spDisposeListener)(spTrackEntry* entry);
typedef void(^spCompleteListener)(spTrackEntry* entry);
typedef void(^spEventListener)(spTrackEntry* entry, spEvent* event);

/** Draws an animated skeleton, providing an AnimationState for applying one or more animations and queuing animations to be
 * played later. */
@interface SkeletonAnimation : SkeletonRenderer {
	spAnimationState* _state;
	bool _ownsAnimationStateData;
	float _timeScale;

	spStartListener _startListener;
    spInterruptListener _interruptListener;
	spEndListener _endListener;
    spDisposeListener _disposeListener;
	spCompleteListener _completeListener;
	spEventListener _eventListener;
}

+ (id) skeletonWithData:(spSkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData;
+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlas:(spAtlas*)atlas scale:(float)scale;
+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale;

- (id) initWithData:(spSkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData;
- (id) initWithFile:(NSString*)skeletonDataFile atlas:(spAtlas*)atlas scale:(float)scale;
- (id) initWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale;

- (void) setAnimationStateData:(spAnimationStateData*)stateData;
- (void) setMixFrom:(NSString*)fromAnimation to:(NSString*)toAnimation duration:(float)duration;

- (spTrackEntry*) setAnimationForTrack:(int)trackIndex name:(NSString*)name loop:(bool)loop;
- (spTrackEntry*) addAnimationForTrack:(int)trackIndex name:(NSString*)name loop:(bool)loop afterDelay:(float)delay;
- (spTrackEntry*) getCurrentForTrack:(int)trackIndex;
- (void) clearTracks;
- (void) clearTrack:(int)trackIndex;

- (void) setListenerForEntry:(spTrackEntry*)entry onStart:(spStartListener)listener;
- (void) setListenerForEntry:(spTrackEntry*)entry onInterrupt:(spInterruptListener)listener;
- (void) setListenerForEntry:(spTrackEntry*)entry onEnd:(spEndListener)listener;
- (void) setListenerForEntry:(spTrackEntry*)entry onDispose:(spDisposeListener)listener;
- (void) setListenerForEntry:(spTrackEntry*)entry onComplete:(spCompleteListener)listener;
- (void) setListenerForEntry:(spTrackEntry*)entry onEvent:(spEventListener)listener;

- (void) onAnimationStateEvent:(spTrackEntry*)entry type:(spEventType)type event:(spEvent*)event;
- (void) onTrackEntryEvent:(spTrackEntry*)entry type:(spEventType)type event:(spEvent*)event;

@property (nonatomic, readonly) spAnimationState* state;
@property (nonatomic) float timeScale;
@property (nonatomic, copy) spStartListener startListener;
@property (nonatomic, copy) spInterruptListener interruptListener;
@property (nonatomic, copy) spEndListener endListener;
@property (nonatomic, copy) spDisposeListener disposeListener;
@property (nonatomic, copy) spCompleteListener completeListener;
@property (nonatomic, copy) spEventListener eventListener;

@end
