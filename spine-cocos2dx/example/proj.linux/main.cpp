#include "../Classes/AppDelegate.h"

#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <unistd.h>

USING_NS_CC;

int main(int argc, char **argv) {
	// create the application instance
	AppDelegate app;
	return Application::getInstance()->run();
}
