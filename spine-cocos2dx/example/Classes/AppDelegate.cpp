#include "AppDelegate.h"

#include <vector>
#include <string>

#include "ExampleScene.h"

USING_NS_CC;
using namespace std;

AppDelegate::AppDelegate() {
}

AppDelegate::~AppDelegate() {
}

bool AppDelegate::applicationDidFinishLaunching() {
	CCDirector* director = CCDirector::sharedDirector();
	
	CCEGLView* view = CCEGLView::sharedOpenGLView();
	director->setOpenGLView(view);
	view->setViewName("Spine Example");
	view->setFrameSize(640, 480);
	view->setDesignResolutionSize(640, 480, kResolutionNoBorder);

	director->setDisplayStats(true);
	director->runWithScene(ExampleScene::scene());
	return true;
}

void AppDelegate::applicationDidEnterBackground() {
	CCDirector::sharedDirector()->stopAnimation();
	// SimpleAudioEngine::sharedEngine()->pauseBackgroundMusic();
}

void AppDelegate::applicationWillEnterForeground() {
	CCDirector::sharedDirector()->startAnimation();
	// SimpleAudioEngine::sharedEngine()->resumeBackgroundMusic();
}
