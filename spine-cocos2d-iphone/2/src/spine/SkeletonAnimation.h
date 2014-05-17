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

#define SPINE_SHORT_NAMES
#import <spine/spine.h>
#import <spine/SkeletonRenderer.h>
#import "cocos2d.h"

@class SkeletonAnimation;

typedef void(^StartListener)(int trackIndex);
typedef void(^EndListener)(int trackIndex);
typedef void(^CompleteListener)(int trackIndex, int loopCount);
typedef void(^EventListener)(int trackIndex, spEvent* event);

/** Draws an animated skeleton, providing an AnimationState for applying one or more animations and queuing animations to be
 * played later. */
@interface SkeletonAnimation : SkeletonRenderer {
	AnimationState* _state;
	bool _ownsAnimationStateData;

	StartListener _startListener;
	EndListener _endListener;
	CompleteListener _completeListener;
	EventListener _eventListener;
}

+ (id) skeletonWithData:(SkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData;
+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlas:(Atlas*)atlas scale:(float)scale;
+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale;

- (id) initWithData:(SkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData;
- (id) initWithFile:(NSString*)skeletonDataFile atlas:(Atlas*)atlas scale:(float)scale;
- (id) initWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale;

- (void) setAnimationStateData:(AnimationStateData*)stateData;
- (void) setMixFrom:(NSString*)fromAnimation to:(NSString*)toAnimation duration:(float)duration;

- (TrackEntry*) setAnimationForTrack:(int)trackIndex name:(NSString*)name loop:(bool)loop;
- (TrackEntry*) addAnimationForTrack:(int)trackIndex name:(NSString*)name loop:(bool)loop afterDelay:(int)delay;
- (TrackEntry*) getCurrentForTrack:(int)trackIndex;
- (void) clearTracks;
- (void) clearTrack:(int)trackIndex;

- (void) setListenerForEntry:(spTrackEntry*)entry onStart:(StartListener)listener;
- (void) setListenerForEntry:(spTrackEntry*)entry onEnd:(EndListener)listener;
- (void) setListenerForEntry:(spTrackEntry*)entry onComplete:(CompleteListener)listener;
- (void) setListenerForEntry:(spTrackEntry*)entry onEvent:(EventListener)listener;

- (void) onAnimationStateEvent:(int)trackIndex type:(EventType)type event:(Event*)event loopCount:(int)loopCount;
- (void) onTrackEntryEvent:(int)trackIndex type:(EventType)type event:(Event*)event loopCount:(int)loopCount;

@property (nonatomic, readonly) AnimationState* state;
@property (nonatomic, copy) StartListener startListener;
@property (nonatomic, copy) EndListener endListener;
@property (nonatomic, copy) CompleteListener completeListener;
@property (nonatomic, copy) EventListener eventListener;

@end
