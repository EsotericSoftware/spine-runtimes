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

#import "RaptorExample.h"
#import "TankExample.h"

spJitterVertexEffect* effect = 0;

@implementation RaptorExample

+ (CCScene*) scene {
    CCScene *scene = [CCScene node];
    [scene addChild:[RaptorExample node]];
    return scene;
}

-(id) init {
    self = [super init];
    if (!self) return nil;
	
	if (!effect) effect = spJitterVertexEffect_create(10, 10);
    
    skeletonNode = [SkeletonAnimation skeletonWithFile:@"raptor-pro.json" atlasFile:@"raptor.atlas" scale:0.3f];
    [skeletonNode setAnimationForTrack:0 name:@"walk" loop:YES];
	[skeletonNode setEffect:&effect->super];
    
    CGSize windowSize = [[CCDirector sharedDirector] viewSize];
    [skeletonNode setPosition:ccp(windowSize.width / 2, 20)];
    [self addChild:skeletonNode];
    
    self.userInteractionEnabled = YES;
    self.contentSize = windowSize;
    
    return self;
}

#if ( TARGET_OS_IPHONE || TARGET_IPHONE_SIMULATOR )
- (void)touchBegan:(UITouch *)touch withEvent:(UIEvent *)event {
    if (!skeletonNode.debugBones)
        skeletonNode.debugBones = true;
    else if (skeletonNode.timeScale == 1)
        skeletonNode.timeScale = 0.3f;
    else
        [[CCDirector sharedDirector] replaceScene:[TankExample scene]];
}
#endif

@end
