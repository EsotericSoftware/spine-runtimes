#/****************************************************************************
# Copyright (c) 2013-2014 cocos2d-x.org
# Copyright (c) 2015 Chukong Technologies Inc.
#
# http://www.cocos2d-x.org
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:

# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.

# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.
# ****************************************************************************/
#cmake_policy(SET CMP0017 NEW)

cmake_minimum_required(VERSION 3.8)

set(APP_NAME spine-cocos2dx-example)
project (${APP_NAME})

set(COCOS2D_ROOT ${CMAKE_SOURCE_DIR}/cocos2d)

set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} "${COCOS2D_ROOT}/cmake/Modules/")

set(COCOS2DX_ROOT_PATH ${CMAKE_CURRENT_SOURCE_DIR}/cocos2d)
set(CMAKE_MODULE_PATH ${COCOS2DX_ROOT_PATH}/cmake/Modules/)

# libcocos2d
set(BUILD_CPP_EMPTY_TEST OFF CACHE BOOL "turn off build cpp-empty-test" FORCE)
set(BUILD_CPP_TESTS OFF CACHE BOOL "turn off build cpp-tests" FORCE)
set(BUILD_LUA_LIBS OFF CACHE BOOL "turn off build lua related targets" FORCE)
set(BUILD_JS_LIBS OFF CACHE BOOL "turn off build js related targets" FORCE)

set(BUILD_EDITOR_SPINE OFF CACHE BOOL "Build editor support for spine" FORCE)


set(BUILD_EDITOR_SPINE ON CACHE BOOL "enable spine")

include_directories(
  ${CMAKE_CURRENT_LIST_DIR}/../../spine-cpp/spine-cpp/include
  ${CMAKE_CURRENT_LIST_DIR}/../src
)

include(CocosBuildSet)
add_subdirectory(${COCOS2DX_ROOT_PATH}/cocos ${ENGINE_BINARY_PATH}/cocos/core)

get_target_property(COCOS2D_X_VERSION cocos2d VERSION)
set(COCOS2DX_ROOT_PATH ${COCOS2D_ROOT})

set(PLATFORM_SPECIFIC_SRC)
set(PLATFORM_SPECIFIC_HEADERS)
set(cc_common_res)


set(GAME_RES_FOLDER  ${CMAKE_CURRENT_SOURCE_DIR}/Resources)
if(APPLE OR VS)
  cocos_mark_multi_resources(cc_common_res RES_TO "Resources" FOLDERS ${GAME_RES_FOLDER})
endif()

if(MACOSX OR APPLE)
    set(APP_UI_RES
    proj.ios_mac/mac/Icon.icns
    proj.ios_mac/mac/Info.plist
    )
    set(PLATFORM_SPECIFIC_SRC
      proj.ios_mac/mac/main.cpp
    )
elseif(LINUX)
    set(PLATFORM_SPECIFIC_SRC
      proj.linux/main.cpp
    )
elseif ( WIN32 )
    set(PLATFORM_SPECIFIC_SRC
      proj.win32/main.cpp
    )
    set(PLATFORM_SPECIFIC_HEADERS
      proj.win32/main.h
      proj.win32/resource.h
    )
elseif(ANDROID)
    set(PLATFORM_SPECIFIC_SRC
      proj.android-studio/app/jni/hellocpp/main.cpp
    )
endif()


file(GLOB GAME_CLASSES_SOURCES "Classes/*.cpp")
file(GLOB GAME_CLASSES_HEADERS "Classes/*.h")

set(GAME_SRC
  ${GAME_CLASSES_SOURCES}
  ${PLATFORM_SPECIFIC_SRC}
  ${cc_common_res}
)


set(GAME_HEADERS
  ${GAME_CLASSES_HEADERS}
  ${PLATFORM_SPECIFIC_HEADERS}
)

if( ANDROID )
    add_library(${APP_NAME} SHARED ${GAME_SRC} ${GAME_HEADERS})
    IF(CMAKE_BUILD_TYPE MATCHES RELEASE)
        ADD_CUSTOM_COMMAND(TARGET ${APP_NAME} POST_BUILD COMMAND ${CMAKE_STRIP} lib${APP_NAME}.so)
    ENDIF()
else()
    add_executable(${APP_NAME} ${GAME_SRC} ${GAME_HEADERS})
endif()

target_include_directories(${APP_NAME} PRIVATE Classes)
target_link_libraries(${APP_NAME} cocos2d)

setup_cocos_app_config(${APP_NAME})

if(COMMAND cocos_get_resource_path)
  cocos_get_resource_path(APP_RES_DIR ${APP_NAME})
else()
  set(APP_RES_DIR "$<TARGET_FILE_DIR:${APP_NAME}>/Resources")
endif()

if(APPLE)
  set_target_properties(${APP_NAME} PROPERTIES RESOURCE "${API_UI_RES}")
  if(MACOSX)
    set_target_properties(${APP_NAME} PROPERTIES
                          MACOSX_BUNDLE_INFO_PLIST "${CMAKE_CURRENT_SOURCE_DIR}/proj.ios_mac/mac/Info.plist"
                          )
  elseif(IOS)
    cocos_pak_xcode(${APP_NAME} INFO_PLIST "${CMAKE_CURRENT_SOURCE_DIR}/proj.ios_mac/ios/Info.plist")
    set_xcode_property(${APP_NAME} ASSETCATALOG_COMPILER_APPICON_NAME "AppIcon-${APP_NAME}")
    set_xcode_property(${APP_NAME} CODE_SIGN_IDENTITY "iPhone Developer")
    set_xcode_property(${APP_NAME} DEVELOPMENT_TEAM "")
    set_xcode_property(${APP_NAME} CODE_SIGN_IDENTITY "iPhone Developer")
  endif()
elseif(WINDOWS)
  cocos_copy_target_dll(${APP_NAME} COPY_TO ${APP_RES_DIR}/..)
endif()

if(LINUX OR WINDOWS)
  if("${COCOS2D_X_VERSION}" VERSION_LESS "4.0")
    if(COMMAND cocos_copy_res)
      cocos_copy_res(COPY_TO ${APP_RES_DIR} FOLDERS ${GAME_RES_FOLDER})
    else()
      cocos_copy_target_res(${APP_NAME} COPY_TO ${APP_RES_DIR} FOLDERS ${GAME_RES_FOLDER})
    endif()
  else()
    cocos_copy_target_res(${APP_NAME} LINK_TO ${APP_RES_DIR} FOLDERS ${GAME_RES_FOLDER})
  endif()
endif()
