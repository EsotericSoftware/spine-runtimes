
#import "cocos2d.h"
#import "AppDelegate.h"
#import "SpineboyExample.h"

@implementation AppController

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    CCFileUtils* sharedFileUtils = [CCFileUtils sharedFileUtils];
    
    sharedFileUtils.searchPath =
    [NSArray arrayWithObjects:
     [[[NSBundle mainBundle] resourcePath] stringByAppendingPathComponent:@"Images"],
     [[[NSBundle mainBundle] resourcePath] stringByAppendingPathComponent:@"Fonts"],
     [[[NSBundle mainBundle] resourcePath] stringByAppendingPathComponent:@"Resources-shared"],
     [[NSBundle mainBundle] resourcePath],
     nil];
    
    [self setupCocos2dWithOptions:@{
            CCSetupDepthFormat: @GL_DEPTH24_STENCIL8,
//          CCSetupTabletScale2X: @YES,
//			CCSetupScreenMode: CCScreenModeFixed,
//			CCSetupScreenOrientation: CCScreenOrientationPortrait,
  			CCSetupShowDebugStats: @YES,
		}];
    
    [[CCDirector sharedDirector] runWithScene:[SpineboyExample scene]];
    
    return YES;
}

//- (CCScene*) startScene {
//    return [SpineboyExample scene];
//}

@end
