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
@import SpineiOS;

@interface SimpleAnimationViewController ()

@property (nonatomic, strong) SpineController *spineController;
@property (nonatomic, strong) SpineUIView *spineView;

@end

@implementation SimpleAnimationViewController

- (instancetype)init {
    self = [super init];
    if (self) {
        self.spineController = [[SpineController alloc] initOnInitialized:^(SpineController *controller) {
            [controller.animationState setAnimation:0 :@"walk" :YES];
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

    self.spineView = [[SpineUIView alloc] initWithAtlasFileName:@"spineboy-pma.atlas"
                                               skeletonFileName:@"spineboy-pro.skel"
                                                         bundle:[NSBundle mainBundle]
                                                     controller:self.spineController
                                                           mode:SpineContentModeFit
                                                      alignment:SpineAlignmentCenter
                                                 boundsProvider:[[SpineSetupPoseBounds alloc] init]
                                                backgroundColor:[UIColor clearColor]];
    self.spineView.frame = self.view.bounds;
    self.spineView.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;

    [self.view addSubview:self.spineView];
}

- (void)viewDidDisappear:(BOOL)animated {
    [super viewDidDisappear:animated];

    // UIKit will eventually release the view controller and controller. We dispose
    // explicitly here so leak reporting runs after native teardown when navigating back.
    [self.spineController dispose];
    [self.spineView removeFromSuperview];
    self.spineView = nil;
}

@end
