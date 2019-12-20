/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#import "IKExample.h"
#import "SpineboyExample.h"

// This example demonstrates how to set the position
// of a bone based on the touch position, which in
// turn will make an IK chain follow that bone
// smoothly.
@implementation IKExample

+ (CCScene*) scene {
	CCScene *scene = [CCScene node];
	[scene addChild:[IKExample node]];
	return scene;
}

-(id) init {
	self = [super init];
	if (!self) return nil;

	// Load the Spineboy skeleton and create a SkeletonAnimation node from it
	// centered on the screen.

	skeletonNode = [SkeletonAnimation skeletonWithFile:@"spineboy-pro.json" atlasFile:@"spineboy.atlas" scale:0.4];
	CGSize windowSize = [[CCDirector sharedDirector] viewSize];
	[skeletonNode setPosition:ccp(windowSize.width / 2, 20)];
	[self addChild:skeletonNode];
	self.userInteractionEnabled = YES;
    self.contentSize = windowSize;
	
	// Queue the "walk" animation on the first track.
	[skeletonNode setAnimationForTrack:0 name:@"walk" loop:YES];
	
	// Queue the "aim" animation on a higher track.
	// It consists of a single frame that positions
	// the back arm and gun such that they point at
	// the "crosshair" bone. By setting this
	// animation on a higher track, it overrides
	// any changes to the back arm and gun made
	// by the walk animation, allowing us to
	// mix the two. The mouse position following
	// is performed in the lambda below.
	[skeletonNode setAnimationForTrack:1 name:@"aim" loop:YES];

	// Position the "crosshair" bone at the mouse
	// location.
	//
	// When setting the crosshair bone position
	// to the mouse position, we need to translate
	// from "skeleton space" to "local bone space".
	// Note that the local bone space is calculated
	// using the bone's parent worldToLocal() function!
	//
	// After updating the bone position based on the
	// converted mouse location, we call updateWorldTransforms()
	// again so the change of the IK target position is
	// applied to the rest of the skeleton.
	__weak IKExample* scene = self;
	skeletonNode.postUpdateWorldTransformsListener = ^(SkeletonAnimation* node) {
		if (scene != NULL) {
			__strong IKExample* sceneStrong = scene;
			spBone* crosshair = [node findBone:@"crosshair"]; // The bone should be cached
			float localX = 0, localY = 0;
			spBone_worldToLocal(crosshair->parent, sceneStrong->position.x, sceneStrong->position.y, &localX, &localY);
			crosshair->x = localX;
			crosshair->y = localY;
			crosshair->appliedValid = FALSE;
			spBone_updateWorldTransform(crosshair);
		}
    };
	return self;
}

#if ( TARGET_OS_IPHONE || TARGET_IPHONE_SIMULATOR )
- (void)touchBegan:(UITouch *)touch withEvent:(UIEvent *)event {
	position = [skeletonNode convertToNodeSpace:touch.locationInWorld];
	printf("%f %f\n", position.x, position.y);
}

- (void)touchMoved:(UITouch *)touch withEvent:(UIEvent *)event {
	position = [skeletonNode convertToNodeSpace:touch.locationInWorld];
	printf("%f %f\n", position.x, position.y);
}

- (void)touchEnded:(UITouch *)touch withEvent:(UIEvent *)event {
	[[CCDirector sharedDirector] replaceScene:[SpineboyExample scene]];
}
#endif

@end
