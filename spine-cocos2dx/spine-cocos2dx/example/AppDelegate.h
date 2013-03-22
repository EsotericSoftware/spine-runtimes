#ifndef _APPDELEGATE_H_
#define _APPDELEGATE_H_

#include "cocos2d.h"

class  AppDelegate : private cocos2d::CCApplication {
public:
	AppDelegate();
	virtual ~AppDelegate();

	virtual bool applicationDidFinishLaunching();
	virtual void applicationDidEnterBackground();
	virtual void applicationWillEnterForeground();
};

#endif // _APPDELEGATE_H_
