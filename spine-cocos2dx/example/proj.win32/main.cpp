#include "main.h"
#include "../Classes/AppDelegate.h"
#include "CCEGLView.h"

USING_NS_CC;

int APIENTRY _tWinMain(HINSTANCE hInstance,
                       HINSTANCE hPrevInstance,
                       LPTSTR    lpCmdLine,
                       int       nCmdShow)
{
    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);

    // create the application instance
    AppDelegate app;
    CCEGLView* eglView = CCEGLView::sharedOpenGLView();
    eglView->setViewName("ExampleSpine");
    eglView->setFrameSize(480, 320);
    // So we need to invoke 'setFrameZoomFactor' (only valid on desktop(win32, mac, linux)) to make the window smaller.
   // eglView->setFrameZoomFactor(0.4f);
    return CCApplication::sharedApplication()->run();
}
