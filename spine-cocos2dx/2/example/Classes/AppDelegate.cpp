#include "AppDelegate.h"

#include <vector>
#include <string>

#include "SpineboyExample.h"
#include "AppMacros.h"

USING_NS_CC;
using namespace std;

AppDelegate::AppDelegate () {
}

AppDelegate::~AppDelegate () {
}

bool AppDelegate::applicationDidFinishLaunching () {
	// initialize director
	CCDirector* director = CCDirector::sharedDirector();
	CCEGLView* view = CCEGLView::sharedOpenGLView();
	director->setOpenGLView(view);
	view->setDesignResolutionSize(designResolutionSize.width, designResolutionSize.height, kResolutionNoBorder);

	CCSize frameSize = view->getFrameSize();
	
	vector<string> searchPath;

	// In this demo, we select resource according to the frame's height.
	// If the resource size is different from design resolution size, you need to set contentScaleFactor.
	// We use the ratio of resource's height to the height of design resolution,
	// this can make sure that the resource's height could fit for the height of design resolution.
	if (frameSize.height > mediumResource.size.height) {
		// if the frame's height is larger than the height of medium resource size, select large resource.
		searchPath.push_back(largeResource.directory);
		director->setContentScaleFactor(MIN(largeResource.size.height/designResolutionSize.height, largeResource.size.width/designResolutionSize.width));
	} else if (frameSize.height > smallResource.size.height) {
		// if the frame's height is larger than the height of small resource size, select medium resource.
		searchPath.push_back(mediumResource.directory);
		director->setContentScaleFactor(MIN(mediumResource.size.height/designResolutionSize.height, mediumResource.size.width/designResolutionSize.width));
	} else {
		// if the frame's height is smaller than the height of medium resource size, select small resource.
		searchPath.push_back(smallResource.directory);
		director->setContentScaleFactor(MIN(smallResource.size.height/designResolutionSize.height, smallResource.size.width/designResolutionSize.width));
	}
	
	searchPath.push_back("common");
	 
	// set searching path
	CCFileUtils::sharedFileUtils()->setSearchPaths(searchPath);
	
	// turn on display FPS
	director->setDisplayStats(true);

	// set FPS. the default value is 1.0/60 if you don't call this
	director->setAnimationInterval(1.0 / 60);

	// create a scene. it's an autorelease object
	auto scene = SpineboyExample::scene();

	// run
	director->runWithScene(scene);

	return true;
}

// This function will be called when the app is inactive. When comes a phone call,it's be invoked too
void AppDelegate::applicationDidEnterBackground () {
	CCDirector::sharedDirector()->stopAnimation();

	// if you use SimpleAudioEngine, it must be paused
	// SimpleAudioEngine::sharedEngine()->pauseBackgroundMusic();
}

// this function will be called when the app is active again
void AppDelegate::applicationWillEnterForeground () {
	CCDirector::sharedDirector()->startAnimation();

	// if you use SimpleAudioEngine, it must resume here
	// SimpleAudioEngine::sharedEngine()->resumeBackgroundMusic();
}
