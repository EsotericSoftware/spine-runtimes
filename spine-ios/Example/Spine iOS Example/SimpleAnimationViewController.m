/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#import "SimpleAnimationViewController.h"
@import Spine;

@interface SimpleAnimationViewController ()

@property (nonatomic, strong) SpineController *spineController;

@end

@implementation SimpleAnimationViewController

- (instancetype)init {
    self = [super init];
    if (self) {
        self.spineController = [[SpineController alloc] initOnInitialized:^(SpineController *controller) {
            [controller.animationState setAnimationByNameWithTrackIndex:0 animationName:@"walk" loop:YES];
        }
                                            onBeforeUpdateWorldTransforms:nil
                                             onAfterUpdateWorldTransforms:nil
                                                            onBeforePaint:nil
                                                             onAfterPaint:nil
                                                  disposeDrawableOnDeInit:YES];
    }
    return self;
}

- (void)viewDidLoad {
    [super viewDidLoad];
    
    SpineUIView *spineView = [[SpineUIView alloc] initWithAtlasFileName:@"spineboy-pma.atlas"
                                                       skeletonFileName:@"spineboy-pro.skel"
                                                                 bundle:[NSBundle mainBundle]
                                                             controller:self.spineController
                                                                   mode:ContentModeFit
                                                              alignment:AlignmentCenter
                                                         boundsProvider:[[SpineSetupPoseBounds alloc] init]
                                                        backgroundColor:[UIColor clearColor]];
    spineView.frame = self.view.bounds;
    spineView.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
    
    [self.view addSubview:spineView];
}

@end
