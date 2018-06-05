LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

$(call import-add-path,$(LOCAL_PATH)/../../cocos2d)
$(call import-add-path,$(LOCAL_PATH)/../../cocos2d/external)
$(call import-add-path,$(LOCAL_PATH)/../../cocos2d/cocos)
$(call import-add-path,$(LOCAL_PATH)/../../cocos2d/cocos/audio/include)

LOCAL_MODULE := MyGame_shared

LOCAL_MODULE_FILENAME := libMyGame

LOCAL_SRC_FILES := hellocpp/main.cpp \
../../Classes//AppDelegate.cpp \
../../Classes//BatchingExample.cpp \
../../Classes//CoinExample.cpp \
../../Classes//GoblinsExample.cpp \
../../Classes//RaptorExample.cpp \
../../Classes//SkeletonRendererSeparatorExample.cpp \
../../Classes//SpineboyExample.cpp \
../../Classes//TankExample.cpp \
../../../src/spine/AttachmentVertices.cpp \
../../../src/spine/SkeletonAnimation.cpp \
../../../src/spine/SkeletonBatch.cpp \
../../../src/spine/SkeletonRenderer.cpp \
../../../src/spine/SkeletonTwoColorBatch.cpp \
../../../src/spine/spine-cocos2dx.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Animation.cpp \
../../../../spine-cpp/spine-cpp//src/spine/AnimationState.cpp \
../../../../spine-cpp/spine-cpp//src/spine/AnimationStateData.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Atlas.cpp \
../../../../spine-cpp/spine-cpp//src/spine/AtlasAttachmentLoader.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Attachment.cpp \
../../../../spine-cpp/spine-cpp//src/spine/AttachmentLoader.cpp \
../../../../spine-cpp/spine-cpp//src/spine/AttachmentTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Bone.cpp \
../../../../spine-cpp/spine-cpp//src/spine/BoneData.cpp \
../../../../spine-cpp/spine-cpp//src/spine/BoundingBoxAttachment.cpp \
../../../../spine-cpp/spine-cpp//src/spine/ClippingAttachment.cpp \
../../../../spine-cpp/spine-cpp//src/spine/ColorTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Constraint.cpp \
../../../../spine-cpp/spine-cpp//src/spine/CurveTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/DeformTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/DrawOrderTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Event.cpp \
../../../../spine-cpp/spine-cpp//src/spine/EventData.cpp \
../../../../spine-cpp/spine-cpp//src/spine/EventTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Extension.cpp \
../../../../spine-cpp/spine-cpp//src/spine/IkConstraint.cpp \
../../../../spine-cpp/spine-cpp//src/spine/IkConstraintData.cpp \
../../../../spine-cpp/spine-cpp//src/spine/IkConstraintTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Json.cpp \
../../../../spine-cpp/spine-cpp//src/spine/LinkedMesh.cpp \
../../../../spine-cpp/spine-cpp//src/spine/MathUtil.cpp \
../../../../spine-cpp/spine-cpp//src/spine/MeshAttachment.cpp \
../../../../spine-cpp/spine-cpp//src/spine/PathAttachment.cpp \
../../../../spine-cpp/spine-cpp//src/spine/PathConstraint.cpp \
../../../../spine-cpp/spine-cpp//src/spine/PathConstraintData.cpp \
../../../../spine-cpp/spine-cpp//src/spine/PathConstraintMixTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/PathConstraintPositionTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/PathConstraintSpacingTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/PointAttachment.cpp \
../../../../spine-cpp/spine-cpp//src/spine/RegionAttachment.cpp \
../../../../spine-cpp/spine-cpp//src/spine/RotateTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/RTTI.cpp \
../../../../spine-cpp/spine-cpp//src/spine/ScaleTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/ShearTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Skeleton.cpp \
../../../../spine-cpp/spine-cpp//src/spine/SkeletonBinary.cpp \
../../../../spine-cpp/spine-cpp//src/spine/SkeletonBounds.cpp \
../../../../spine-cpp/spine-cpp//src/spine/SkeletonClipping.cpp \
../../../../spine-cpp/spine-cpp//src/spine/SkeletonData.cpp \
../../../../spine-cpp/spine-cpp//src/spine/SkeletonJson.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Skin.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Slot.cpp \
../../../../spine-cpp/spine-cpp//src/spine/SlotData.cpp \
../../../../spine-cpp/spine-cpp//src/spine/SpineObject.cpp \
../../../../spine-cpp/spine-cpp//src/spine/TextureLoader.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Timeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/TransformConstraint.cpp \
../../../../spine-cpp/spine-cpp//src/spine/TransformConstraintData.cpp \
../../../../spine-cpp/spine-cpp//src/spine/TransformConstraintTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/TranslateTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Triangulator.cpp \
../../../../spine-cpp/spine-cpp//src/spine/TwoColorTimeline.cpp \
../../../../spine-cpp/spine-cpp//src/spine/Updatable.cpp \
../../../../spine-cpp/spine-cpp//src/spine/VertexAttachment.cpp \
../../../../spine-cpp/spine-cpp//src/spine/VertexEffect.cpp \

LOCAL_C_INCLUDES := $(LOCAL_PATH)/../../Classes \
				   $(LOCAL_PATH)/../../../../spine-cpp/spine-cpp/include \
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
