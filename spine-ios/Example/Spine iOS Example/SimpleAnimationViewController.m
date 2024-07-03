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
