
#import "cocos2d.h"

@interface spine_cocos2d_iphoneAppDelegate : NSObject <NSApplicationDelegate> {
	NSWindow *window_;
	CCGLView *glView_;
}

@property (assign) IBOutlet NSWindow *window;
@property (assign) IBOutlet CCGLView *glView;

- (IBAction)toggleFullScreen:(id)sender;

@end
