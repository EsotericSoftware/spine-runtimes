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

	auto director = Director::getInstance();
	auto glview = GLViewImpl::create("Spine Example");
	glview->setFrameSize(960, 640);
	director->setOpenGLView(glview);
	return Application::getInstance()->run();
}
