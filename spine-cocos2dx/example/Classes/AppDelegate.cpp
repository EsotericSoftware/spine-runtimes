/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

#include "AppDelegate.h"

#include <vector>
#include <string>

#include "ExampleLayer.h"
#include "AppMacros.h"

USING_NS_CC;
using namespace std;

AppDelegate::AppDelegate () {
}

AppDelegate::~AppDelegate () {
}

bool AppDelegate::applicationDidFinishLaunching () {
	CCDirector* director = CCDirector::sharedDirector();

	CCEGLView* view = CCEGLView::sharedOpenGLView();
	director->setOpenGLView(view);
	view->setDesignResolutionSize(designResolutionSize.width, designResolutionSize.height, kResolutionNoBorder);

	// In this demo, we select resource according to the frame's height.
	// If the resource size is different from design resolution size, you need to set contentScaleFactor.
	// We use the ratio of resource's height to the height of design resolution,
	// this can make sure that the resource's height could fit for the height of design resolution.

	vector<string> searchPath;
	CCSize frameSize = view->getFrameSize();
	if (frameSize.height > mediumResource.size.height) {
		// if the frame's height is larger than the height of medium resource size, select large resource.
		searchPath.push_back(largeResource.directory);

		director->setContentScaleFactor( //
				MIN(largeResource.size.height / designResolutionSize.height, //
				largeResource.size.width / designResolutionSize.width));
	} else if (frameSize.height > smallResource.size.height) {
		// if the frame's height is larger than the height of small resource size, select medium resource.
		searchPath.push_back(mediumResource.directory);

		director->setContentScaleFactor( //
				MIN(mediumResource.size.height / designResolutionSize.height, //
				mediumResource.size.width / designResolutionSize.width));
	} else {
		// if the frame's height is smaller than the height of medium resource size, select small resource.
		searchPath.push_back(smallResource.directory);

		director->setContentScaleFactor( //
				MIN(smallResource.size.height / designResolutionSize.height, //
				smallResource.size.width / designResolutionSize.width));
	}

	searchPath.push_back("common");
	CCFileUtils::sharedFileUtils()->setSearchPaths(searchPath);

	director->setDisplayStats(true);
	director->setAnimationInterval(1.0 / 60);
	director->runWithScene(ExampleLayer::scene());

	return true;
}

void AppDelegate::applicationDidEnterBackground () {
	CCDirector::sharedDirector()->stopAnimation();
	// SimpleAudioEngine::sharedEngine()->pauseBackgroundMusic();
}

void AppDelegate::applicationWillEnterForeground () {
	CCDirector::sharedDirector()->startAnimation();
	// SimpleAudioEngine::sharedEngine()->resumeBackgroundMusic();
}
