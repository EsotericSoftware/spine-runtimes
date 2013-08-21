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

#import <spine/CCSkeletonAnimation.h>
#import <spine/spine-cocos2d-iphone.h>

@interface CCSkeletonAnimation (Private)
- (void) initialize;
@end

@implementation CCSkeletonAnimation

@synthesize states = _states;

+ (id) skeletonWithData:(SkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData {
	return [[[CCSkeletonAnimation alloc] initWithData:skeletonData ownsSkeletonData:ownsSkeletonData] autorelease];
}

+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlas:(Atlas*)atlas scale:(float)scale {
	return [[[CCSkeletonAnimation alloc] initWithFile:skeletonDataFile atlas:atlas scale:scale] autorelease];
}

+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale {
	return [[[CCSkeletonAnimation alloc] initWithFile:skeletonDataFile atlasFile:atlasFile scale:scale] autorelease];
}

- (void) initialize {
	_states = [[NSMutableArray arrayWithCapacity:2] retain];
	_stateDatas = [[NSMutableArray arrayWithCapacity:2] retain];
	[self addAnimationState];
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
	for (NSValue* value in _stateDatas)
		AnimationStateData_dispose([value pointerValue]);
	[_stateDatas release];
	
	for (NSValue* value in _states)
		AnimationState_dispose([value pointerValue]);
	[_states release];

	[super dealloc];
}

- (void) update:(ccTime)deltaTime {
	[super update:deltaTime];

	deltaTime *= _timeScale;
	for (NSValue* value in _states) {
		AnimationState* state = [value pointerValue];
		AnimationState_update(state, deltaTime);
		AnimationState_apply(state, _skeleton);
	}
	Skeleton_updateWorldTransform(_skeleton);
}

- (void) addAnimationState {
	AnimationStateData* stateData = AnimationStateData_create(_skeleton->data);
	[_stateDatas addObject:[NSValue valueWithPointer:stateData]];
	[self addAnimationState:stateData];
}

- (void) addAnimationState:(AnimationStateData*)stateData {
	NSAssert(stateData, @"stateData cannot be null.");
	AnimationState* state = AnimationState_create(stateData);
	[_states addObject:[NSValue valueWithPointer:state]];
}

- (AnimationState*) getAnimationState:(int)stateIndex {
	NSAssert(stateIndex >= 0 && stateIndex < (int)_states.count, @"stateIndex out of range.");
	return [[_states objectAtIndex:stateIndex] pointerValue];
}

- (void) setAnimationStateData:(AnimationStateData*)stateData forState:(int)stateIndex {
	NSAssert(stateData, @"stateData cannot be null.");
	NSAssert(stateIndex >= 0 && stateIndex < (int)_states.count, @"stateIndex out of range.");
	
	AnimationState* state = [[_states objectAtIndex:stateIndex] pointerValue];
	for (NSValue* value in _stateDatas) {
		if (state->data == [value pointerValue]) {
			AnimationStateData_dispose(state->data);
			[_stateDatas removeObject:value];
			break;
		}
	}
	[_states removeObject:[NSValue valueWithPointer:state]];
	AnimationState_dispose(state);

	state = AnimationState_create(stateData);
	[_states setObject:[NSValue valueWithPointer:state] atIndexedSubscript:stateIndex];
}

- (void) setMixFrom:(NSString*)fromAnimation to:(NSString*)toAnimation duration:(float)duration {
	[self setMixFrom:fromAnimation to:toAnimation duration:duration forState:0];
}

- (void) setMixFrom:(NSString*)fromAnimation to:(NSString*)toAnimation duration:(float)duration forState:(int)stateIndex {
	NSAssert(stateIndex >= 0 && stateIndex < (int)_states.count, @"stateIndex out of range.");
	AnimationState* state = [[_states objectAtIndex:stateIndex] pointerValue];
	AnimationStateData_setMixByName(state->data, [fromAnimation UTF8String], [toAnimation UTF8String], duration);
}

- (void) setAnimation:(NSString*)name loop:(bool)loop {
	[self setAnimation:name loop:loop forState:0];
}

- (void) setAnimation:(NSString*)name loop:(bool)loop forState:(int)stateIndex {
	NSAssert(stateIndex >= 0 && stateIndex < (int)_states.count, @"stateIndex out of range.");
	AnimationState* state = [[_states objectAtIndex:stateIndex] pointerValue];
	AnimationState_setAnimationByName(state, [name UTF8String], loop);
}

- (void) addAnimation:(NSString*)name loop:(bool)loop afterDelay:(float)delay {
	[self addAnimation:name loop:loop afterDelay:delay forState:0];
}

- (void) addAnimation:(NSString*)name loop:(bool)loop afterDelay:(float)delay forState:(int)stateIndex {
	NSAssert(stateIndex >= 0 && stateIndex < (int)_states.count, @"stateIndex out of range.");
	AnimationState* state = [[_states objectAtIndex:stateIndex] pointerValue];
	AnimationState_addAnimationByName(state, [name UTF8String], loop, delay);
}

- (void) clearAnimation {
	[self clearAnimationForState:0];
}

- (void) clearAnimationForState:(int)stateIndex {
	NSAssert(stateIndex >= 0 && stateIndex < (int)_states.count, @"stateIndex out of range.");
	AnimationState* state = [[_states objectAtIndex:stateIndex] pointerValue];
	AnimationState_clearAnimation(state);
}

@end
