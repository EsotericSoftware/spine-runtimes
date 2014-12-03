#include "main.h"
#include "../Classes/AppDelegate.h"

USING_NS_CC;

int APIENTRY _tWinMain(HINSTANCE hInstance,
					   HINSTANCE hPrevInstance,
					   LPTSTR lpCmdLine,
					   int nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	// create the application instance
	AppDelegate app;

	CCEGLView* eglView = CCEGLView::sharedOpenGLView();
	eglView->setViewName("Spine Example");
	eglView->setFrameSize(960, 640);
	return CCApplication::sharedApplication()->run();
}
