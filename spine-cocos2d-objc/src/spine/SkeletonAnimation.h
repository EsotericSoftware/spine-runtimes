/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
