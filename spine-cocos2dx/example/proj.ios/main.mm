
#import <UIKit/UIKit.h>
#import <stdexcept>
#import <iostream>

int main(int argc, char *argv[]) {
    NSAutoreleasePool * pool = [[NSAutoreleasePool alloc] init];
    int retVal = 0;
    try {
        retVal = UIApplicationMain(argc, argv, nil, @"AppController");
    } catch (const std::exception &ex) {
        std::cout << "Unhandled exception:";
        std::cout << ex.what();
    }
    [pool release];
    return retVal;
}
