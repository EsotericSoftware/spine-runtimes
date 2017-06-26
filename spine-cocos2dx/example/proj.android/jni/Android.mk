LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

$(call import-add-path,$(LOCAL_PATH)/../../cocos2d)
$(call import-add-path,$(LOCAL_PATH)/../../cocos2d/external)
$(call import-add-path,$(LOCAL_PATH)/../../cocos2d/cocos)
$(call import-add-path,$(LOCAL_PATH)/../../cocos2d/cocos/audio/include)

LOCAL_MODULE := MyGame_shared

LOCAL_MODULE_FILENAME := libMyGame

LOCAL_SRC_FILES := hellocpp/main.cpp \
                   ../../Classes/AppDelegate.cpp \
                   ../../Classes/BatchingExample.cpp \
                   ../../Classes/GoblinsExample.cpp \
                   ../../Classes/RaptorExample.cpp \
                   ../../Classes/SimpleCommand.cpp \
                   ../../Classes/SpineboyExample.cpp \
                   ../../Classes/TankExample.cpp \
                   ../../../src/spine/AttachmentVertices.cpp \
                   ../../../src/spine/Cocos2dAttachmentLoader.cpp \
                   ../../../src/spine/SkeletonAnimation.cpp \
                   ../../../src/spine/SkeletonBatch.cpp \
                   ../../../src/spine/SkeletonTwoColorBatch.cpp \
                   ../../../src/spine/SkeletonRenderer.cpp \
                   ../../../src/spine/spine-cocos2dx.cpp \
                   ../../../../spine-c/spine-c/src/spine/Animation.c \
				   ../../../../spine-c/spine-c/src/spine/AnimationState.c \
				   ../../../../spine-c/spine-c/src/spine/AnimationStateData.c \
				   ../../../../spine-c/spine-c/src/spine/Atlas.c \
				   ../../../../spine-c/spine-c/src/spine/AtlasAttachmentLoader.c \
				   ../../../../spine-c/spine-c/src/spine/Attachment.c \
				   ../../../../spine-c/spine-c/src/spine/AttachmentLoader.c \
				   ../../../../spine-c/spine-c/src/spine/Bone.c \
				   ../../../../spine-c/spine-c/src/spine/BoneData.c \
				   ../../../../spine-c/spine-c/src/spine/BoundingBoxAttachment.c \
				   ../../../../spine-c/spine-c/src/spine/Color.c \
				   ../../../../spine-c/spine-c/src/spine/Event.c \
				   ../../../../spine-c/spine-c/src/spine/EventData.c \
				   ../../../../spine-c/spine-c/src/spine/IkConstraint.c \
				   ../../../../spine-c/spine-c/src/spine/IkConstraintData.c \
				   ../../../../spine-c/spine-c/src/spine/Json.c \
				   ../../../../spine-c/spine-c/src/spine/MeshAttachment.c \
				   ../../../../spine-c/spine-c/src/spine/PathAttachment.c \
				   ../../../../spine-c/spine-c/src/spine/PointAttachment.c \
				   ../../../../spine-c/spine-c/src/spine/PathConstraint.c \
				   ../../../../spine-c/spine-c/src/spine/PathConstraintData.c \
				   ../../../../spine-c/spine-c/src/spine/RegionAttachment.c \
				   ../../../../spine-c/spine-c/src/spine/Skeleton.c \
				   ../../../../spine-c/spine-c/src/spine/SkeletonBinary.c \
				   ../../../../spine-c/spine-c/src/spine/SkeletonBounds.c \
				   ../../../../spine-c/spine-c/src/spine/SkeletonData.c \
				   ../../../../spine-c/spine-c/src/spine/SkeletonJson.c \
				   ../../../../spine-c/spine-c/src/spine/Skin.c \
				   ../../../../spine-c/spine-c/src/spine/Slot.c \
				   ../../../../spine-c/spine-c/src/spine/SlotData.c \
				   ../../../../spine-c/spine-c/src/spine/TransformConstraint.c \
				   ../../../../spine-c/spine-c/src/spine/TransformConstraintData.c \
				   ../../../../spine-c/spine-c/src/spine/VertexAttachment.c \
				   ../../../../spine-c/spine-c/src/spine/VertexEffect.c \
				   ../../../../spine-c/spine-c/src/spine/extension.c


LOCAL_C_INCLUDES := $(LOCAL_PATH)/../../Classes \
				   $(LOCAL_PATH)/../../../../spine-c/spine-c/include \
				   $(LOCAL_PATH)/../../../../spine-cocos2dx/src

# _COCOS_HEADER_ANDROID_BEGIN
# _COCOS_HEADER_ANDROID_END


LOCAL_STATIC_LIBRARIES := cocos2dx_static

# _COCOS_LIB_ANDROID_BEGIN
# _COCOS_LIB_ANDROID_END

include $(BUILD_SHARED_LIBRARY)

$(call import-module,.)

# _COCOS_LIB_IMPORT_ANDROID_BEGIN
# _COCOS_LIB_IMPORT_ANDROID_END
