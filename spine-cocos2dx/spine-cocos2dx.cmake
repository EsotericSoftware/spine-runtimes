set(CMAKE_CXX_STANDARD 11)

if(EXISTS ${CMAKE_CURRENT_LIST_DIR}/../spine-cpp AND EXISTS ${CMAKE_CURRENT_LIST_DIR}/spine-cocos2dx)
	set(SPINE_RUNTIMES_PATH ${CMAKE_CURRENT_LIST_DIR}/..)
endif()

if (NOT DEFINED SPINE_RUNTIMES_PATH)
	message(FATAL_ERROR "Please set SPINE_RUNTIMES_PATH to the directory you cloned https://github.com/esotericsoftware/spine-runtimes to. E.g. cmake .. -DSPINE_RUNTIMES_PATH=/path/to/spine-runtimes")
endif()

message("-- SPINE_RUNTIMES_PATH:${SPINE_RUNTIMES_PATH}")

# Disable the built-in cocos2dx Spine support
set(BUILD_EDITOR_SPINE OFF CACHE BOOL "Build editor support for spine" FORCE)

# Add spine-cpp library
file(GLOB SPINE_CPP_HEADERS "${SPINE_RUNTIMES_PATH}/spine-cpp/spine-cpp/include/**/*.h")
file(GLOB SPINE_CPP_SOURCES "${SPINE_RUNTIMES_PATH}/spine-cpp/spine-cpp/src/**/*.cpp")
add_library(spine-cpp STATIC ${SPINE_CPP_SOURCES} ${SPINE_CPP_HEADERS})
target_include_directories(spine-cpp PUBLIC "${SPINE_RUNTIMES_PATH}/spine-cpp/spine-cpp/include/")

# Add spine-cocos2dx library
file(GLOB_RECURSE SPINE_COCOS2DX_HEADERS "${SPINE_RUNTIMES_PATH}/spine-cocos2dx/spine-cocos2dx/src/**/*.h")
file(GLOB_RECURSE SPINE_COCOS2DX_SOURCES "${SPINE_RUNTIMES_PATH}/spine-cocos2dx/spine-cocos2dx/src/**/*.cpp")
add_library(spine-cocos2dx STATIC ${SPINE_COCOS2DX_SOURCES} ${SPINE_COCOS2DX_HEADERS})
target_include_directories(spine-cocos2dx PUBLIC "${SPINE_RUNTIMES_PATH}/spine-cpp/spine-cpp/include/")
target_include_directories(spine-cocos2dx PUBLIC "${SPINE_RUNTIMES_PATH}/spine-cocos2dx/spine-cocos2dx/src/")
target_link_libraries(spine-cocos2dx PRIVATE cocos2d)