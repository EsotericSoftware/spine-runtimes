//
// Spine Runtimes License Agreement
// Last updated April 5, 2025. Replaces all prior versions.
//
// Copyright (c) 2013-2025, Esoteric Software LLC
//
// Integration of the Spine Runtimes into software or otherwise creating
// derivative works of the Spine Runtimes is permitted under the terms and
// conditions of Section 2 of the Spine Editor License Agreement:
// http://esotericsoftware.com/spine-editor-license
//
// Otherwise, it is permitted to integrate the Spine Runtimes into software
// or otherwise create derivative works of the Spine Runtimes (collectively,
// "Products"), provided that each user of the Products must obtain their own
// Spine Editor license and redistribution of the Products in any form must
// include this license and copyright notice.
//
// THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
// BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

// AUTO GENERATED FILE, DO NOT EDIT.

import Foundation
import SpineC

/// Stores bones and slots to be posed by animations and application code. Multiple skeleton
/// instances can share the same SkeletonData, including animations, attachments, and skins.
///
/// After posing, call updateWorldTransform(Physics) to apply constraints and compute world
/// transforms for rendering.
@objc(SpineSkeleton)
@objcMembers
public class Skeleton: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_skeleton) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public convenience init(_ skeletonData: SkeletonData) {
        let ptr = spine_skeleton_create(skeletonData._ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    public var data: SkeletonData {
        let result = spine_skeleton_get_data(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        return SkeletonData(fromPointer: result!)
    }

    public var bones: ArrayBone {
        let result = spine_skeleton_get_bones(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        return ArrayBone(fromPointer: result!)
    }

    public var updateCacheList: ArrayUpdate {
        let result = spine_skeleton_get_update_cache(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        return ArrayUpdate(fromPointer: result!)
    }

    public var rootBone: Bone? {
        let result = spine_skeleton_get_root_bone(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        return result.map { Bone(fromPointer: $0) }
    }

    /// The skeleton's slots in setup pose order. To change the order use DrawOrder::getPose(). For
    /// rendering use DrawOrder::getAppliedPose().
    public var slots: ArraySlot {
        let result = spine_skeleton_get_slots(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        return ArraySlot(fromPointer: result!)
    }

    /// The skeleton's draw order. Use DrawOrder::getAppliedPose() for rendering and
    /// DrawOrder::getPose() for changing the draw order.
    public var drawOrder: DrawOrder {
        let result = spine_skeleton_get_draw_order(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        return DrawOrder(fromPointer: result!)
    }

    public var skin: Skin? {
        let result = spine_skeleton_get_skin(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        return result.map { Skin(fromPointer: $0) }
    }

    public var constraints: ArrayConstraint {
        let result = spine_skeleton_get_constraints(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        return ArrayConstraint(fromPointer: result!)
    }

    /// The skeleton's physics constraints.
    public var physicsConstraints: ArrayPhysicsConstraint {
        let result = spine_skeleton_get_physics_constraints(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        return ArrayPhysicsConstraint(fromPointer: result!)
    }

    public var color: Color {
        let result = spine_skeleton_get_color(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        return Color(fromPointer: result!)
    }

    public var scaleX: Float {
        get {
            let result = spine_skeleton_get_scale_x(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
            return result
        }
        set {
            spine_skeleton_set_scale_x(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), newValue)
        }
    }

    public var scaleY: Float {
        get {
            let result = spine_skeleton_get_scale_y(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
            return result
        }
        set {
            spine_skeleton_set_scale_y(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), newValue)
        }
    }

    public var x: Float {
        get {
            let result = spine_skeleton_get_x(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
            return result
        }
        set {
            spine_skeleton_set_x(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), newValue)
        }
    }

    public var y: Float {
        get {
            let result = spine_skeleton_get_y(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
            return result
        }
        set {
            spine_skeleton_set_y(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), newValue)
        }
    }

    /// The x component of a vector that defines the direction PhysicsConstraintPose::getWind() is
    /// applied.
    public var windX: Float {
        get {
            let result = spine_skeleton_get_wind_x(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
            return result
        }
        set {
            spine_skeleton_set_wind_x(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), newValue)
        }
    }

    /// The y component of a vector that defines the direction PhysicsConstraintPose::getWind() is
    /// applied.
    public var windY: Float {
        get {
            let result = spine_skeleton_get_wind_y(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
            return result
        }
        set {
            spine_skeleton_set_wind_y(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), newValue)
        }
    }

    /// The x component of a vector that defines the direction PhysicsConstraintPose::getGravity()
    /// is applied.
    public var gravityX: Float {
        get {
            let result = spine_skeleton_get_gravity_x(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
            return result
        }
        set {
            spine_skeleton_set_gravity_x(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), newValue)
        }
    }

    /// The y component of a vector that defines the direction PhysicsConstraintPose::getGravity()
    /// is applied.
    public var gravityY: Float {
        get {
            let result = spine_skeleton_get_gravity_y(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
            return result
        }
        set {
            spine_skeleton_set_gravity_y(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), newValue)
        }
    }

    /// Returns the skeleton's time, used for time-based manipulations, such as PhysicsConstraint.
    ///
    /// See update().
    public var time: Float {
        get {
            let result = spine_skeleton_get_time(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
            return result
        }
        set {
            spine_skeleton_set_time(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), newValue)
        }
    }

    public var setColor: Color {
        get { fatalError("Setter-only property") }
        set(newValue) {
            spine_skeleton_set_color_1(
                _ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), newValue._ptr.assumingMemoryBound(to: spine_color_wrapper.self))
        }
    }

    /// Caches information about bones and constraints. Must be called if the active skin is
    /// modified or if bones, constraints, or weighted path attachments are added or removed.
    public func updateCache() {
        spine_skeleton_update_cache(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
    }

    public func printUpdateCache() {
        spine_skeleton_print_update_cache(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
    }

    public func constrained(_ object: Posed) {
        spine_skeleton_constrained(
            _ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), object._ptr.assumingMemoryBound(to: spine_posed_wrapper.self))
    }

    public func sortBone(_ bone: Bone?) {
        spine_skeleton_sort_bone(
            _ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), bone?._ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
    }

    public static func sortReset(_ bones: ArrayBone) {
        spine_skeleton_sort_reset(bones._ptr.assumingMemoryBound(to: spine_array_bone_wrapper.self))
    }

    /// Updates the world transform for each bone and applies all constraints.
    ///
    /// See [World transforms](http://esotericsoftware.com/spine-runtime-skeletons#World-transforms)
    /// in the Spine Runtimes Guide.
    public func updateWorldTransform(_ physics: Physics) {
        spine_skeleton_update_world_transform(
            _ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), spine_physics(rawValue: UInt32(physics.rawValue)))
    }

    /// Sets the bones, constraints, and slots to their setup pose values.
    public func setupPose() {
        spine_skeleton_setup_pose(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
    }

    /// Sets the bones and constraints to their setup pose values.
    public func setupPoseBones() {
        spine_skeleton_setup_pose_bones(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
    }

    public func setupPoseSlots() {
        spine_skeleton_setup_pose_slots(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
    }

    /// - Returns: May be NULL.
    public func findBone(_ boneName: String) -> Bone? {
        let result = spine_skeleton_find_bone(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), boneName)
        return result.map { Bone(fromPointer: $0) }
    }

    /// - Returns: May be NULL.
    public func findSlot(_ slotName: String) -> Slot? {
        let result = spine_skeleton_find_slot(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), slotName)
        return result.map { Slot(fromPointer: $0) }
    }

    /// A convenience method to set an attachment by finding the slot with findSlot(String), finding
    /// the attachment with getAttachment(int, String), then setting the slot's
    /// SlotPose::getAttachment().
    ///
    /// - Parameter placeholderName: May be empty.
    public func setAttachment(_ slotName: String, _ placeholderName: String) {
        spine_skeleton_set_attachment(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), slotName, placeholderName)
    }

    public func setScale(_ scaleX: Float, _ scaleY: Float) {
        spine_skeleton_set_scale(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), scaleX, scaleY)
    }

    public func setPosition(_ x: Float, _ y: Float) {
        spine_skeleton_set_position(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), x, y)
    }

    /// Rotates the physics constraint so next {
    public func physicsTranslate(_ x: Float, _ y: Float) {
        spine_skeleton_physics_translate(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), x, y)
    }

    /// Calls {
    public func physicsRotate(_ x: Float, _ y: Float, _ degrees: Float) {
        spine_skeleton_physics_rotate(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), x, y, degrees)
    }

    public func update(_ delta: Float) {
        spine_skeleton_update(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), delta)
    }

    /// Sets a skin by name (see setSkin).
    public func setSkin(_ skinName: String) {
        spine_skeleton_set_skin_1(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), skinName)
    }

    /// Sets the skin used to look up attachments before looking in SkeletonData::getDefaultSkin().
    /// If the skin is changed, updateCache() is called.
    ///
    /// Attachments from the new skin are attached if the corresponding attachment from the old skin
    /// was attached. If there was no old skin, each slot's setup pose placeholder attachment is
    /// attached from the new skin.
    ///
    /// After changing the skin, the visible attachments can be reset to those attached in the setup
    /// pose by calling setupPoseSlots(). Also, AnimationState::apply(Skeleton & ) is often called
    /// before the next time the skeleton is rendered so attachment keys in the current animation(s)
    /// can hide or show attachments from the new skin.
    ///
    /// - Parameter newSkin: May be NULL.
    public func setSkin2(_ newSkin: Skin?) {
        spine_skeleton_set_skin_2(
            _ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), newSkin?._ptr.assumingMemoryBound(to: spine_skin_wrapper.self))
    }

    /// Finds an attachment by looking in getSkin() and SkeletonData::getDefaultSkin() using the
    /// slot name and skin placeholder name. First the skin is checked and if the attachment was not
    /// found, the default skin is checked.
    ///
    /// - Returns: May be NULL.
    public func getAttachment(_ slotName: String, _ placeholderName: String) -> Attachment? {
        let result = spine_skeleton_get_attachment_1(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), slotName, placeholderName)
        guard let ptr = result else { return nil }
        let rtti = spine_attachment_get_rtti(ptr)
        let rttiClassName = String(cString: spine_rtti_get_class_name(rtti)!)
        switch rttiClassName {
        case "BoundingBoxAttachment":
            let castedPtr = spine_attachment_cast_to_bounding_box_attachment(ptr)
            return BoundingBoxAttachment(fromPointer: castedPtr!)
        case "ClippingAttachment":
            let castedPtr = spine_attachment_cast_to_clipping_attachment(ptr)
            return ClippingAttachment(fromPointer: castedPtr!)
        case "MeshAttachment":
            let castedPtr = spine_attachment_cast_to_mesh_attachment(ptr)
            return MeshAttachment(fromPointer: castedPtr!)
        case "PathAttachment":
            let castedPtr = spine_attachment_cast_to_path_attachment(ptr)
            return PathAttachment(fromPointer: castedPtr!)
        case "PointAttachment":
            let castedPtr = spine_attachment_cast_to_point_attachment(ptr)
            return PointAttachment(fromPointer: castedPtr!)
        case "RegionAttachment":
            let castedPtr = spine_attachment_cast_to_region_attachment(ptr)
            return RegionAttachment(fromPointer: castedPtr!)
        default:
            fatalError("Unknown concrete type: \(rttiClassName) for abstract class Attachment")
        }
    }

    /// Finds an attachment by looking in getSkin() and SkeletonData::getDefaultSkin() using the
    /// slot index and skin placeholder name. First the skin is checked and if the attachment was
    /// not found, the default skin is checked.
    ///
    /// - Returns: May be NULL.
    public func getAttachment2(_ slotIndex: Int32, _ placeholderName: String) -> Attachment? {
        let result = spine_skeleton_get_attachment_2(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), slotIndex, placeholderName)
        guard let ptr = result else { return nil }
        let rtti = spine_attachment_get_rtti(ptr)
        let rttiClassName = String(cString: spine_rtti_get_class_name(rtti)!)
        switch rttiClassName {
        case "BoundingBoxAttachment":
            let castedPtr = spine_attachment_cast_to_bounding_box_attachment(ptr)
            return BoundingBoxAttachment(fromPointer: castedPtr!)
        case "ClippingAttachment":
            let castedPtr = spine_attachment_cast_to_clipping_attachment(ptr)
            return ClippingAttachment(fromPointer: castedPtr!)
        case "MeshAttachment":
            let castedPtr = spine_attachment_cast_to_mesh_attachment(ptr)
            return MeshAttachment(fromPointer: castedPtr!)
        case "PathAttachment":
            let castedPtr = spine_attachment_cast_to_path_attachment(ptr)
            return PathAttachment(fromPointer: castedPtr!)
        case "PointAttachment":
            let castedPtr = spine_attachment_cast_to_point_attachment(ptr)
            return PointAttachment(fromPointer: castedPtr!)
        case "RegionAttachment":
            let castedPtr = spine_attachment_cast_to_region_attachment(ptr)
            return RegionAttachment(fromPointer: castedPtr!)
        default:
            fatalError("Unknown concrete type: \(rttiClassName) for abstract class Attachment")
        }
    }

    public func setColor2(_ r: Float, _ g: Float, _ b: Float, _ a: Float) {
        spine_skeleton_set_color_2(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), r, g, b, a)
    }

    public func dispose() {
        spine_skeleton_dispose(_ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
    }
}
