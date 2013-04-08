LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := cocos2dcpp_shared

LOCAL_MODULE_FILENAME := libcocos2dcpp

# Sources
LOCAL_SRC_FILES := hellocpp/main.cpp \
                   ../../Classes/AppDelegate.cpp \
                   ../../Classes/ExampleLayer.cpp

SRC_LIST := $(wildcard $(LOCAL_PATH)/../../../../../spine-runtimes/spine-c/src/spine/*.c)
LOCAL_SRC_FILES += $(SRC_LIST:$(LOCAL_PATH)/%=%)

SRC_LIST := $(wildcard $(LOCAL_PATH)/../../../../../spine-runtimes/spine-cocos2dx/src/spine/*.cpp)
LOCAL_SRC_FILES += $(SRC_LIST:$(LOCAL_PATH)/%=%)

# Headers
LOCAL_C_INCLUDES := $(LOCAL_PATH)/../../Classes \
					../../../../spine-runtimes/spine-c/include \
					../../../../spine-runtimes/spine-cocos2dx/src

LOCAL_WHOLE_STATIC_LIBRARIES += cocos2dx_static
#LOCAL_WHOLE_STATIC_LIBRARIES += cocosdenshion_static
#LOCAL_WHOLE_STATIC_LIBRARIES += box2d_static
#LOCAL_WHOLE_STATIC_LIBRARIES += chipmunk_static
#LOCAL_WHOLE_STATIC_LIBRARIES += cocos_extension_static

include $(BUILD_SHARED_LIBRARY)

$(call import-module,cocos2dx)
#$(call import-module,cocos2dx/platform/third_party/android/prebuilt/libcurl)
#$(call import-module,CocosDenshion/android)
#$(call import-module,extensions)
#$(call import-module,external/Box2D)
#$(call import-module,external/chipmunk)