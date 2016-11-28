cmake_minimum_required(VERSION 2.8.9)
project(spine)

set(CMAKE_INSTALL_PREFIX "./")
set(CMAKE_VERBOSE_MAKEFILE ON)
set(SPINE_SFML FALSE CACHE BOOL FALSE)
set(SPINE_COCOS2D_OBJC FALSE CACHE BOOL FALSE)
set(SPINE_COCOS2D_X FALSE CACHE BOOL FALSE)

if((${SPINE_SFML}) OR (${CMAKE_CURRENT_BINARY_DIR} MATCHES "spine-sfml"))
	add_subdirectory(spine-c)
	add_subdirectory(spine-sfml)
endif()

if((${SPINE_COCOS2D_OBJC}) OR (${CMAKE_CURRENT_BINARY_DIR} MATCHES "spine-cocos2d-objc"))
	if (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
		add_subdirectory(spine-cocos2d-objc)
	endif()
endif()

if((${SPINE_COCOS2D_X}) OR (${CMAKE_CURRENT_BINARY_DIR} MATCHES "spine-cocos2dx"))
	add_subdirectory(spine-cocos2dx)
endif()

add_subdirectory(spine-c/spine-c-unit-tests)