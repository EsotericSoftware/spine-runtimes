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

#import "TankExample.h"
#import "CoinExample.h"

@implementation TankExample

+ (CCScene*) scene {
    CCScene *scene = [CCScene node];
    [scene addChild:[TankExample node]];
    return scene;
}

-(id) init {
    self = [super init];
    if (!self) return nil;
    
    skeletonNode = [SkeletonAnimation skeletonWithFile:@"tank-pro.json" atlasFile:@"tank.atlas" scale:0.2f];
    [skeletonNode setAnimationForTrack:0 name:@"drive" loop:YES];
    
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
        [[CCDirector sharedDirector] replaceScene:[CoinExample scene]];
}
#endif

@end
