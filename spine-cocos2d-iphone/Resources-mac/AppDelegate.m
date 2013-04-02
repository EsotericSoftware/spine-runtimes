
#import "AppDelegate.h"
#import "ExampleLayer.h"

@implementation spine_cocos2d_iphoneAppDelegate
@synthesize window=window_, glView=glView_;

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {
	CCDirectorMac *director = (CCDirectorMac*)[CCDirector sharedDirector];

	[director setDisplayStats:YES];
	[director setView:glView_];
	[director setResizeMode:kCCDirectorResize_AutoScale];

	[window_ setAcceptsMouseMovedEvents:NO];
	[window_ center];

	[director runWithScene:[ExampleLayer scene]];
}

- (BOOL) applicationShouldTerminateAfterLastWindowClosed: (NSApplication *) theApplication {
	return YES;
}

- (void)dealloc {
	[[CCDirector sharedDirector] end];
	[window_ release];
	[super dealloc];
}

#pragma mark AppDelegate - IBActions

- (IBAction)toggleFullScreen: (id)sender {
	CCDirectorMac *director = (CCDirectorMac*) [CCDirector sharedDirector];
	[director setFullScreen:![director isFullScreen]];
}

@end
