import Foundation
import SpineCppLite

public typealias BlendMode = spine_blend_mode
public typealias MixBlend = spine_mix_blend
public typealias EventType = spine_event_type
public typealias AttachmentType = spine_attachment_type
public typealias ConstraintType = spine_constraint_type
public typealias Inherit = spine_inherit
public typealias PositionMode = spine_position_mode
public typealias SpacingMode = spine_spacing_mode
public typealias RotateMode = spine_rotate_mode
public typealias Physics = spine_physics

@objc(SpineTransformConstraintData)
@objcMembers
public final class TransformConstraintData: NSObject {

    internal let wrappee: spine_transform_constraint_data

    internal init(_ wrappee: spine_transform_constraint_data) {
        self.wrappee = wrappee
        super.init()
    }

    public var bones: [BoneData] {
        let num = Int(spine_transform_constraint_data_get_num_bones(wrappee))
        let ptr = spine_transform_constraint_data_get_bones(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var target: BoneData {
        get {
            return .init(spine_transform_constraint_data_get_target(wrappee))
        }
        set {
            spine_transform_constraint_data_set_target(wrappee, newValue.wrappee)
        }
    }

    public var mixRotate: Float {
        get {
            return spine_transform_constraint_data_get_mix_rotate(wrappee)
        }
        set {
            spine_transform_constraint_data_set_mix_rotate(wrappee, newValue)
        }
    }

    public var mixX: Float {
        get {
            return spine_transform_constraint_data_get_mix_x(wrappee)
        }
        set {
            spine_transform_constraint_data_set_mix_x(wrappee, newValue)
        }
    }

    public var mixY: Float {
        get {
            return spine_transform_constraint_data_get_mix_y(wrappee)
        }
        set {
            spine_transform_constraint_data_set_mix_y(wrappee, newValue)
        }
    }

    public var mixScaleX: Float {
        get {
            return spine_transform_constraint_data_get_mix_scale_x(wrappee)
        }
        set {
            spine_transform_constraint_data_set_mix_scale_x(wrappee, newValue)
        }
    }

    public var mixScaleY: Float {
        get {
            return spine_transform_constraint_data_get_mix_scale_y(wrappee)
        }
        set {
            spine_transform_constraint_data_set_mix_scale_y(wrappee, newValue)
        }
    }

    public var mixShearY: Float {
        get {
            return spine_transform_constraint_data_get_mix_shear_y(wrappee)
        }
        set {
            spine_transform_constraint_data_set_mix_shear_y(wrappee, newValue)
        }
    }

    public var offsetRotation: Float {
        get {
            return spine_transform_constraint_data_get_offset_rotation(wrappee)
        }
        set {
            spine_transform_constraint_data_set_offset_rotation(wrappee, newValue)
        }
    }

    public var offsetX: Float {
        get {
            return spine_transform_constraint_data_get_offset_x(wrappee)
        }
        set {
            spine_transform_constraint_data_set_offset_x(wrappee, newValue)
        }
    }

    public var offsetY: Float {
        get {
            return spine_transform_constraint_data_get_offset_y(wrappee)
        }
        set {
            spine_transform_constraint_data_set_offset_y(wrappee, newValue)
        }
    }

    public var offsetScaleX: Float {
        get {
            return spine_transform_constraint_data_get_offset_scale_x(wrappee)
        }
        set {
            spine_transform_constraint_data_set_offset_scale_x(wrappee, newValue)
        }
    }

    public var offsetScaleY: Float {
        get {
            return spine_transform_constraint_data_get_offset_scale_y(wrappee)
        }
        set {
            spine_transform_constraint_data_set_offset_scale_y(wrappee, newValue)
        }
    }

    public var offsetShearY: Float {
        get {
            return spine_transform_constraint_data_get_offset_shear_y(wrappee)
        }
        set {
            spine_transform_constraint_data_set_offset_shear_y(wrappee, newValue)
        }
    }

    public var isRelative: Bool {
        get {
            return spine_transform_constraint_data_get_is_relative(wrappee) != 0
        }
        set {
            spine_transform_constraint_data_set_is_relative(wrappee, newValue ? -1 : 0)
        }
    }

    public var isLocal: Bool {
        get {
            return spine_transform_constraint_data_get_is_local(wrappee) != 0
        }
        set {
            spine_transform_constraint_data_set_is_local(wrappee, newValue ? -1 : 0)
        }
    }

}

@objc(SpineBoundingBoxAttachment)
@objcMembers
public final class BoundingBoxAttachment: NSObject {

    internal let wrappee: spine_bounding_box_attachment

    internal init(_ wrappee: spine_bounding_box_attachment) {
        self.wrappee = wrappee
        super.init()
    }

    public var color: Color {
        return .init(spine_bounding_box_attachment_get_color(wrappee))
    }

    public func setColor(r: Float, g: Float, b: Float, a: Float) {
        spine_bounding_box_attachment_set_color(wrappee, r, g, b, a)
    }

}

@objc(SpinePhysicsConstraintData)
@objcMembers
public final class PhysicsConstraintData: NSObject {

    internal let wrappee: spine_physics_constraint_data

    internal init(_ wrappee: spine_physics_constraint_data) {
        self.wrappee = wrappee
        super.init()
    }

    public var bone: BoneData {
        get {
            return .init(spine_physics_constraint_data_get_bone(wrappee))
        }
        set {
            spine_physics_constraint_data_set_bone(wrappee, newValue.wrappee)
        }
    }

    public var x: Float {
        get {
            return spine_physics_constraint_data_get_x(wrappee)
        }
        set {
            spine_physics_constraint_data_set_x(wrappee, newValue)
        }
    }

    public var y: Float {
        get {
            return spine_physics_constraint_data_get_y(wrappee)
        }
        set {
            spine_physics_constraint_data_set_y(wrappee, newValue)
        }
    }

    public var rotate: Float {
        get {
            return spine_physics_constraint_data_get_rotate(wrappee)
        }
        set {
            spine_physics_constraint_data_set_rotate(wrappee, newValue)
        }
    }

    public var scaleX: Float {
        get {
            return spine_physics_constraint_data_get_scale_x(wrappee)
        }
        set {
            spine_physics_constraint_data_set_scale_x(wrappee, newValue)
        }
    }

    public var shearX: Float {
        get {
            return spine_physics_constraint_data_get_shear_x(wrappee)
        }
        set {
            spine_physics_constraint_data_set_shear_x(wrappee, newValue)
        }
    }

    public var limit: Float {
        get {
            return spine_physics_constraint_data_get_limit(wrappee)
        }
        set {
            spine_physics_constraint_data_set_limit(wrappee, newValue)
        }
    }

    public var step: Float {
        get {
            return spine_physics_constraint_data_get_step(wrappee)
        }
        set {
            spine_physics_constraint_data_set_step(wrappee, newValue)
        }
    }

    public var inertia: Float {
        get {
            return spine_physics_constraint_data_get_inertia(wrappee)
        }
        set {
            spine_physics_constraint_data_set_inertia(wrappee, newValue)
        }
    }

    public var strength: Float {
        get {
            return spine_physics_constraint_data_get_strength(wrappee)
        }
        set {
            spine_physics_constraint_data_set_strength(wrappee, newValue)
        }
    }

    public var damping: Float {
        get {
            return spine_physics_constraint_data_get_damping(wrappee)
        }
        set {
            spine_physics_constraint_data_set_damping(wrappee, newValue)
        }
    }

    public var massInverse: Float {
        get {
            return spine_physics_constraint_data_get_mass_inverse(wrappee)
        }
        set {
            spine_physics_constraint_data_set_mass_inverse(wrappee, newValue)
        }
    }

    public var wind: Float {
        get {
            return spine_physics_constraint_data_get_wind(wrappee)
        }
        set {
            spine_physics_constraint_data_set_wind(wrappee, newValue)
        }
    }

    public var gravity: Float {
        get {
            return spine_physics_constraint_data_get_gravity(wrappee)
        }
        set {
            spine_physics_constraint_data_set_gravity(wrappee, newValue)
        }
    }

    public var mix: Float {
        get {
            return spine_physics_constraint_data_get_mix(wrappee)
        }
        set {
            spine_physics_constraint_data_set_mix(wrappee, newValue)
        }
    }

    public var isInertiaGlobal: Bool {
        get {
            return spine_physics_constraint_data_is_inertia_global(wrappee) != 0
        }
        set {
            spine_physics_constraint_data_set_inertia_global(wrappee, newValue ? -1 : 0)
        }
    }

    public var isStrengthGlobal: Bool {
        get {
            return spine_physics_constraint_data_is_strength_global(wrappee) != 0
        }
        set {
            spine_physics_constraint_data_set_strength_global(wrappee, newValue ? -1 : 0)
        }
    }

    public var isDampingGlobal: Bool {
        get {
            return spine_physics_constraint_data_is_damping_global(wrappee) != 0
        }
        set {
            spine_physics_constraint_data_set_damping_global(wrappee, newValue ? -1 : 0)
        }
    }

    public var isMassGlobal: Bool {
        get {
            return spine_physics_constraint_data_is_mass_global(wrappee) != 0
        }
        set {
            spine_physics_constraint_data_set_mass_global(wrappee, newValue ? -1 : 0)
        }
    }

    public var isWindGlobal: Bool {
        get {
            return spine_physics_constraint_data_is_wind_global(wrappee) != 0
        }
        set {
            spine_physics_constraint_data_set_wind_global(wrappee, newValue ? -1 : 0)
        }
    }

    public var isGravityGlobal: Bool {
        get {
            return spine_physics_constraint_data_is_gravity_global(wrappee) != 0
        }
        set {
            spine_physics_constraint_data_set_gravity_global(wrappee, newValue ? -1 : 0)
        }
    }

    public var isMixGlobal: Bool {
        get {
            return spine_physics_constraint_data_is_mix_global(wrappee) != 0
        }
        set {
            spine_physics_constraint_data_set_mix_global(wrappee, newValue ? -1 : 0)
        }
    }

}

@objc(SpineAnimationStateEvents)
@objcMembers
public final class AnimationStateEvents: NSObject {

    internal let wrappee: spine_animation_state_events

    internal init(_ wrappee: spine_animation_state_events) {
        self.wrappee = wrappee
        super.init()
    }

    @discardableResult
    public func getEventType(index: Int32) -> EventType {
        return spine_animation_state_events_get_event_type(wrappee, index)
    }

    @discardableResult
    public func getTrackEntry(index: Int32) -> TrackEntry {
        return .init(spine_animation_state_events_get_track_entry(wrappee, index))
    }

    @discardableResult
    public func getEvent(index: Int32) -> Event? {
        return spine_animation_state_events_get_event(wrappee, index).flatMap { .init($0) }
    }

    public func reset() {
        spine_animation_state_events_reset(wrappee)
    }

}

@objc(SpineTransformConstraint)
@objcMembers
public final class TransformConstraint: NSObject {

    internal let wrappee: spine_transform_constraint

    internal init(_ wrappee: spine_transform_constraint) {
        self.wrappee = wrappee
        super.init()
    }

    public var order: Int32 {
        return spine_transform_constraint_get_order(wrappee)
    }

    public var data: TransformConstraintData {
        return .init(spine_transform_constraint_get_data(wrappee))
    }

    public var bones: [Bone] {
        let num = Int(spine_transform_constraint_get_num_bones(wrappee))
        let ptr = spine_transform_constraint_get_bones(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var target: Bone {
        get {
            return .init(spine_transform_constraint_get_target(wrappee))
        }
        set {
            spine_transform_constraint_set_target(wrappee, newValue.wrappee)
        }
    }

    public var mixRotate: Float {
        get {
            return spine_transform_constraint_get_mix_rotate(wrappee)
        }
        set {
            spine_transform_constraint_set_mix_rotate(wrappee, newValue)
        }
    }

    public var mixX: Float {
        get {
            return spine_transform_constraint_get_mix_x(wrappee)
        }
        set {
            spine_transform_constraint_set_mix_x(wrappee, newValue)
        }
    }

    public var mixY: Float {
        get {
            return spine_transform_constraint_get_mix_y(wrappee)
        }
        set {
            spine_transform_constraint_set_mix_y(wrappee, newValue)
        }
    }

    public var mixScaleX: Float {
        get {
            return spine_transform_constraint_get_mix_scale_x(wrappee)
        }
        set {
            spine_transform_constraint_set_mix_scale_x(wrappee, newValue)
        }
    }

    public var mixScaleY: Float {
        get {
            return spine_transform_constraint_get_mix_scale_y(wrappee)
        }
        set {
            spine_transform_constraint_set_mix_scale_y(wrappee, newValue)
        }
    }

    public var mixShearY: Float {
        get {
            return spine_transform_constraint_get_mix_shear_y(wrappee)
        }
        set {
            spine_transform_constraint_set_mix_shear_y(wrappee, newValue)
        }
    }

    public var isActive: Bool {
        get {
            return spine_transform_constraint_get_is_active(wrappee) != 0
        }
        set {
            spine_transform_constraint_set_is_active(wrappee, newValue ? -1 : 0)
        }
    }

    public func update() {
        spine_transform_constraint_update(wrappee)
    }

}

@objc(SpinePathConstraintData)
@objcMembers
public final class PathConstraintData: NSObject {

    internal let wrappee: spine_path_constraint_data

    internal init(_ wrappee: spine_path_constraint_data) {
        self.wrappee = wrappee
        super.init()
    }

    public var bones: [BoneData] {
        let num = Int(spine_path_constraint_data_get_num_bones(wrappee))
        let ptr = spine_path_constraint_data_get_bones(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var target: SlotData {
        get {
            return .init(spine_path_constraint_data_get_target(wrappee))
        }
        set {
            spine_path_constraint_data_set_target(wrappee, newValue.wrappee)
        }
    }

    public var positionMode: PositionMode {
        get {
            return spine_path_constraint_data_get_position_mode(wrappee)
        }
        set {
            spine_path_constraint_data_set_position_mode(wrappee, newValue)
        }
    }

    public var spacingMode: SpacingMode {
        get {
            return spine_path_constraint_data_get_spacing_mode(wrappee)
        }
        set {
            spine_path_constraint_data_set_spacing_mode(wrappee, newValue)
        }
    }

    public var rotateMode: RotateMode {
        get {
            return spine_path_constraint_data_get_rotate_mode(wrappee)
        }
        set {
            spine_path_constraint_data_set_rotate_mode(wrappee, newValue)
        }
    }

    public var offsetRotation: Float {
        get {
            return spine_path_constraint_data_get_offset_rotation(wrappee)
        }
        set {
            spine_path_constraint_data_set_offset_rotation(wrappee, newValue)
        }
    }

    public var position: Float {
        get {
            return spine_path_constraint_data_get_position(wrappee)
        }
        set {
            spine_path_constraint_data_set_position(wrappee, newValue)
        }
    }

    public var spacing: Float {
        get {
            return spine_path_constraint_data_get_spacing(wrappee)
        }
        set {
            spine_path_constraint_data_set_spacing(wrappee, newValue)
        }
    }

    public var mixRotate: Float {
        get {
            return spine_path_constraint_data_get_mix_rotate(wrappee)
        }
        set {
            spine_path_constraint_data_set_mix_rotate(wrappee, newValue)
        }
    }

    public var mixX: Float {
        get {
            return spine_path_constraint_data_get_mix_x(wrappee)
        }
        set {
            spine_path_constraint_data_set_mix_x(wrappee, newValue)
        }
    }

    public var mixY: Float {
        get {
            return spine_path_constraint_data_get_mix_y(wrappee)
        }
        set {
            spine_path_constraint_data_set_mix_y(wrappee, newValue)
        }
    }

}

@objc(SpineAnimationStateData)
@objcMembers
public final class AnimationStateData: NSObject {

    internal let wrappee: spine_animation_state_data

    internal init(_ wrappee: spine_animation_state_data) {
        self.wrappee = wrappee
        super.init()
    }

    public var skeletonData: SkeletonData {
        return .init(spine_animation_state_data_get_skeleton_data(wrappee))
    }

    public var defaultMix: Float {
        get {
            return spine_animation_state_data_get_default_mix(wrappee)
        }
        set {
            spine_animation_state_data_set_default_mix(wrappee, newValue)
        }
    }

    public func setMix(from: Animation, to: Animation, duration: Float) {
        spine_animation_state_data_set_mix(wrappee, from.wrappee, to.wrappee, duration)
    }

    @discardableResult
    public func getMix(from: Animation, to: Animation) -> Float {
        return spine_animation_state_data_get_mix(wrappee, from.wrappee, to.wrappee)
    }

    public func setMixByName(fromName: String?, toName: String?, duration: Float) {
        spine_animation_state_data_set_mix_by_name(wrappee, fromName, toName, duration)
    }

    @discardableResult
    public func getMixByName(fromName: String?, toName: String?) -> Float {
        return spine_animation_state_data_get_mix_by_name(wrappee, fromName, toName)
    }

    public func clear() {
        spine_animation_state_data_clear(wrappee)
    }

}

@objc(SpineSkeletonDataResult)
@objcMembers
public final class SkeletonDataResult: NSObject {

    internal let wrappee: spine_skeleton_data_result
    internal var disposed = false

    internal init(_ wrappee: spine_skeleton_data_result) {
        self.wrappee = wrappee
        super.init()
    }

    public var error: String? {
        return spine_skeleton_data_result_get_error(wrappee).flatMap { String(cString: $0) }
    }

    public var data: SkeletonData {
        return .init(spine_skeleton_data_result_get_data(wrappee))
    }

    public func dispose() {
        if disposed { return }
        disposed = true
        spine_skeleton_data_result_dispose(wrappee)
    }

}

@objc(SpineClippingAttachment)
@objcMembers
public final class ClippingAttachment: NSObject {

    internal let wrappee: spine_clipping_attachment

    internal init(_ wrappee: spine_clipping_attachment) {
        self.wrappee = wrappee
        super.init()
    }

    public var color: Color {
        return .init(spine_clipping_attachment_get_color(wrappee))
    }

    public var endSlot: SlotData? {
        get {
            return spine_clipping_attachment_get_end_slot(wrappee).flatMap { .init($0) }
        }
        set {
            spine_clipping_attachment_set_end_slot(wrappee, newValue?.wrappee)
        }
    }

    public func setColor(r: Float, g: Float, b: Float, a: Float) {
        spine_clipping_attachment_set_color(wrappee, r, g, b, a)
    }

}

@objc(SpineIkConstraintData)
@objcMembers
public final class IkConstraintData: NSObject {

    internal let wrappee: spine_ik_constraint_data

    internal init(_ wrappee: spine_ik_constraint_data) {
        self.wrappee = wrappee
        super.init()
    }

    public var bones: [BoneData] {
        let num = Int(spine_ik_constraint_data_get_num_bones(wrappee))
        let ptr = spine_ik_constraint_data_get_bones(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var target: BoneData {
        get {
            return .init(spine_ik_constraint_data_get_target(wrappee))
        }
        set {
            spine_ik_constraint_data_set_target(wrappee, newValue.wrappee)
        }
    }

    public var bendDirection: Int32 {
        get {
            return spine_ik_constraint_data_get_bend_direction(wrappee)
        }
        set {
            spine_ik_constraint_data_set_bend_direction(wrappee, newValue)
        }
    }

    public var compress: Bool {
        get {
            return spine_ik_constraint_data_get_compress(wrappee) != 0
        }
        set {
            spine_ik_constraint_data_set_compress(wrappee, newValue ? -1 : 0)
        }
    }

    public var stretch: Bool {
        get {
            return spine_ik_constraint_data_get_stretch(wrappee) != 0
        }
        set {
            spine_ik_constraint_data_set_stretch(wrappee, newValue ? -1 : 0)
        }
    }

    public var uniform: Bool {
        get {
            return spine_ik_constraint_data_get_uniform(wrappee) != 0
        }
        set {
            spine_ik_constraint_data_set_uniform(wrappee, newValue ? -1 : 0)
        }
    }

    public var mix: Float {
        get {
            return spine_ik_constraint_data_get_mix(wrappee)
        }
        set {
            spine_ik_constraint_data_set_mix(wrappee, newValue)
        }
    }

    public var softness: Float {
        get {
            return spine_ik_constraint_data_get_softness(wrappee)
        }
        set {
            spine_ik_constraint_data_set_softness(wrappee, newValue)
        }
    }

}

@objc(SpinePhysicsConstraint)
@objcMembers
public final class PhysicsConstraint: NSObject {

    internal let wrappee: spine_physics_constraint

    internal init(_ wrappee: spine_physics_constraint) {
        self.wrappee = wrappee
        super.init()
    }

    public var bone: Bone {
        get {
            return .init(spine_physics_constraint_get_bone(wrappee))
        }
        set {
            spine_physics_constraint_set_bone(wrappee, newValue.wrappee)
        }
    }

    public var inertia: Float {
        get {
            return spine_physics_constraint_get_inertia(wrappee)
        }
        set {
            spine_physics_constraint_set_inertia(wrappee, newValue)
        }
    }

    public var strength: Float {
        get {
            return spine_physics_constraint_get_strength(wrappee)
        }
        set {
            spine_physics_constraint_set_strength(wrappee, newValue)
        }
    }

    public var damping: Float {
        get {
            return spine_physics_constraint_get_damping(wrappee)
        }
        set {
            spine_physics_constraint_set_damping(wrappee, newValue)
        }
    }

    public var massInverse: Float {
        get {
            return spine_physics_constraint_get_mass_inverse(wrappee)
        }
        set {
            spine_physics_constraint_set_mass_inverse(wrappee, newValue)
        }
    }

    public var wind: Float {
        get {
            return spine_physics_constraint_get_wind(wrappee)
        }
        set {
            spine_physics_constraint_set_wind(wrappee, newValue)
        }
    }

    public var gravity: Float {
        get {
            return spine_physics_constraint_get_gravity(wrappee)
        }
        set {
            spine_physics_constraint_set_gravity(wrappee, newValue)
        }
    }

    public var mix: Float {
        get {
            return spine_physics_constraint_get_mix(wrappee)
        }
        set {
            spine_physics_constraint_set_mix(wrappee, newValue)
        }
    }

    public var reset: Bool {
        get {
            return spine_physics_constraint_get_reset(wrappee) != 0
        }
        set {
            spine_physics_constraint_set_reset(wrappee, newValue ? -1 : 0)
        }
    }

    public var ux: Float {
        get {
            return spine_physics_constraint_get_ux(wrappee)
        }
        set {
            spine_physics_constraint_set_ux(wrappee, newValue)
        }
    }

    public var uy: Float {
        get {
            return spine_physics_constraint_get_uy(wrappee)
        }
        set {
            spine_physics_constraint_set_uy(wrappee, newValue)
        }
    }

    public var cx: Float {
        get {
            return spine_physics_constraint_get_cx(wrappee)
        }
        set {
            spine_physics_constraint_set_cx(wrappee, newValue)
        }
    }

    public var cy: Float {
        get {
            return spine_physics_constraint_get_cy(wrappee)
        }
        set {
            spine_physics_constraint_set_cy(wrappee, newValue)
        }
    }

    public var tx: Float {
        get {
            return spine_physics_constraint_get_tx(wrappee)
        }
        set {
            spine_physics_constraint_set_tx(wrappee, newValue)
        }
    }

    public var ty: Float {
        get {
            return spine_physics_constraint_get_ty(wrappee)
        }
        set {
            spine_physics_constraint_set_ty(wrappee, newValue)
        }
    }

    public var xOffset: Float {
        get {
            return spine_physics_constraint_get_x_offset(wrappee)
        }
        set {
            spine_physics_constraint_set_x_offset(wrappee, newValue)
        }
    }

    public var xVelocity: Float {
        get {
            return spine_physics_constraint_get_x_velocity(wrappee)
        }
        set {
            spine_physics_constraint_set_x_velocity(wrappee, newValue)
        }
    }

    public var yOffset: Float {
        get {
            return spine_physics_constraint_get_y_offset(wrappee)
        }
        set {
            spine_physics_constraint_set_y_offset(wrappee, newValue)
        }
    }

    public var yVelocity: Float {
        get {
            return spine_physics_constraint_get_y_velocity(wrappee)
        }
        set {
            spine_physics_constraint_set_y_velocity(wrappee, newValue)
        }
    }

    public var rotateOffset: Float {
        get {
            return spine_physics_constraint_get_rotate_offset(wrappee)
        }
        set {
            spine_physics_constraint_set_rotate_offset(wrappee, newValue)
        }
    }

    public var rotateVelocity: Float {
        get {
            return spine_physics_constraint_get_rotate_velocity(wrappee)
        }
        set {
            spine_physics_constraint_set_rotate_velocity(wrappee, newValue)
        }
    }

    public var scaleOffset: Float {
        get {
            return spine_physics_constraint_get_scale_offset(wrappee)
        }
        set {
            spine_physics_constraint_set_scale_offset(wrappee, newValue)
        }
    }

    public var scaleVelocity: Float {
        get {
            return spine_physics_constraint_get_scale_velocity(wrappee)
        }
        set {
            spine_physics_constraint_set_scale_velocity(wrappee, newValue)
        }
    }

    public var isActive: Bool {
        get {
            return spine_physics_constraint_is_active(wrappee) != 0
        }
        set {
            spine_physics_constraint_set_active(wrappee, newValue ? -1 : 0)
        }
    }

    public var remaining: Float {
        get {
            return spine_physics_constraint_get_remaining(wrappee)
        }
        set {
            spine_physics_constraint_set_remaining(wrappee, newValue)
        }
    }

    public var lastTime: Float {
        get {
            return spine_physics_constraint_get_last_time(wrappee)
        }
        set {
            spine_physics_constraint_set_last_time(wrappee, newValue)
        }
    }

    public func resetFully() {
        spine_physics_constraint_reset_fully(wrappee)
    }

    public func update(physics: Physics) {
        spine_physics_constraint_update(wrappee, physics)
    }

    public func translate(x: Float, y: Float) {
        spine_physics_constraint_translate(wrappee, x, y)
    }

    public func rotate(x: Float, y: Float, degrees: Float) {
        spine_physics_constraint_rotate(wrappee, x, y, degrees)
    }

}

@objc(SpineRegionAttachment)
@objcMembers
public final class RegionAttachment: NSObject {

    internal let wrappee: spine_region_attachment

    internal init(_ wrappee: spine_region_attachment) {
        self.wrappee = wrappee
        super.init()
    }

    public var color: Color {
        return .init(spine_region_attachment_get_color(wrappee))
    }

    public var path: String? {
        return spine_region_attachment_get_path(wrappee).flatMap { String(cString: $0) }
    }

    public var region: TextureRegion? {
        return spine_region_attachment_get_region(wrappee).flatMap { .init($0) }
    }

    public var sequence: Sequence? {
        return spine_region_attachment_get_sequence(wrappee).flatMap { .init($0) }
    }

    public var offset: [Float?] {
        let num = Int(spine_region_attachment_get_num_offset(wrappee))
        let ptr = spine_region_attachment_get_offset(wrappee)
        return (0..<num).compactMap {
            ptr?[$0]
        }
    }

    public var uvs: [Float?] {
        let num = Int(spine_region_attachment_get_num_uvs(wrappee))
        let ptr = spine_region_attachment_get_uvs(wrappee)
        return (0..<num).compactMap {
            ptr?[$0]
        }
    }

    public var x: Float {
        get {
            return spine_region_attachment_get_x(wrappee)
        }
        set {
            spine_region_attachment_set_x(wrappee, newValue)
        }
    }

    public var y: Float {
        get {
            return spine_region_attachment_get_y(wrappee)
        }
        set {
            spine_region_attachment_set_y(wrappee, newValue)
        }
    }

    public var rotation: Float {
        get {
            return spine_region_attachment_get_rotation(wrappee)
        }
        set {
            spine_region_attachment_set_rotation(wrappee, newValue)
        }
    }

    public var scaleX: Float {
        get {
            return spine_region_attachment_get_scale_x(wrappee)
        }
        set {
            spine_region_attachment_set_scale_x(wrappee, newValue)
        }
    }

    public var scaleY: Float {
        get {
            return spine_region_attachment_get_scale_y(wrappee)
        }
        set {
            spine_region_attachment_set_scale_y(wrappee, newValue)
        }
    }

    public var width: Float {
        get {
            return spine_region_attachment_get_width(wrappee)
        }
        set {
            spine_region_attachment_set_width(wrappee, newValue)
        }
    }

    public var height: Float {
        get {
            return spine_region_attachment_get_height(wrappee)
        }
        set {
            spine_region_attachment_set_height(wrappee, newValue)
        }
    }

    public func updateRegion() {
        spine_region_attachment_update_region(wrappee)
    }

    public func setColor(r: Float, g: Float, b: Float, a: Float) {
        spine_region_attachment_set_color(wrappee, r, g, b, a)
    }

}

@objc(SpineVertexAttachment)
@objcMembers
public final class VertexAttachment: NSObject {

    internal let wrappee: spine_vertex_attachment

    internal init(_ wrappee: spine_vertex_attachment) {
        self.wrappee = wrappee
        super.init()
    }

    public var worldVerticesLength: Int32 {
        return spine_vertex_attachment_get_world_vertices_length(wrappee)
    }

    public var bones: [Int32?] {
        let num = Int(spine_vertex_attachment_get_num_bones(wrappee))
        let ptr = spine_vertex_attachment_get_bones(wrappee)
        return (0..<num).compactMap {
            ptr?[$0]
        }
    }

    public var vertices: [Float?] {
        let num = Int(spine_vertex_attachment_get_num_vertices(wrappee))
        let ptr = spine_vertex_attachment_get_vertices(wrappee)
        return (0..<num).compactMap {
            ptr?[$0]
        }
    }

    public var timelineAttachment: Attachment? {
        get {
            return spine_vertex_attachment_get_timeline_attachment(wrappee).flatMap { .init($0) }
        }
        set {
            spine_vertex_attachment_set_timeline_attachment(wrappee, newValue?.wrappee)
        }
    }

}

@objc(SpineSkeletonDrawable)
@objcMembers
public final class SkeletonDrawable: NSObject {

    internal let wrappee: spine_skeleton_drawable
    internal var disposed = false

    internal init(_ wrappee: spine_skeleton_drawable) {
        self.wrappee = wrappee
        super.init()
    }

    public var skeleton: Skeleton {
        return .init(spine_skeleton_drawable_get_skeleton(wrappee))
    }

    public var animationState: AnimationState {
        return .init(spine_skeleton_drawable_get_animation_state(wrappee))
    }

    public var animationStateData: AnimationStateData {
        return .init(spine_skeleton_drawable_get_animation_state_data(wrappee))
    }

    public var animationStateEvents: AnimationStateEvents {
        return .init(spine_skeleton_drawable_get_animation_state_events(wrappee))
    }

    public func dispose() {
        if disposed { return }
        disposed = true
        spine_skeleton_drawable_dispose(wrappee)
    }

}

@objc(SpinePointAttachment)
@objcMembers
public final class PointAttachment: NSObject {

    internal let wrappee: spine_point_attachment

    internal init(_ wrappee: spine_point_attachment) {
        self.wrappee = wrappee
        super.init()
    }

    public var color: Color {
        return .init(spine_point_attachment_get_color(wrappee))
    }

    public var x: Float {
        get {
            return spine_point_attachment_get_x(wrappee)
        }
        set {
            spine_point_attachment_set_x(wrappee, newValue)
        }
    }

    public var y: Float {
        get {
            return spine_point_attachment_get_y(wrappee)
        }
        set {
            spine_point_attachment_set_y(wrappee, newValue)
        }
    }

    public var rotation: Float {
        get {
            return spine_point_attachment_get_rotation(wrappee)
        }
        set {
            spine_point_attachment_set_rotation(wrappee, newValue)
        }
    }

    @discardableResult
    public func computeWorldPosition(bone: Bone) -> Vector {
        return .init(spine_point_attachment_compute_world_position(wrappee, bone.wrappee))
    }

    @discardableResult
    public func computeWorldRotation(bone: Bone) -> Float {
        return spine_point_attachment_compute_world_rotation(wrappee, bone.wrappee)
    }

    public func setColor(r: Float, g: Float, b: Float, a: Float) {
        spine_point_attachment_set_color(wrappee, r, g, b, a)
    }

}

@objc(SpineMeshAttachment)
@objcMembers
public final class MeshAttachment: NSObject {

    internal let wrappee: spine_mesh_attachment

    internal init(_ wrappee: spine_mesh_attachment) {
        self.wrappee = wrappee
        super.init()
    }

    public var regionUvs: [Float?] {
        let num = Int(spine_mesh_attachment_get_num_region_uvs(wrappee))
        let ptr = spine_mesh_attachment_get_region_uvs(wrappee)
        return (0..<num).compactMap {
            ptr?[$0]
        }
    }

    public var uvs: [Float?] {
        let num = Int(spine_mesh_attachment_get_num_uvs(wrappee))
        let ptr = spine_mesh_attachment_get_uvs(wrappee)
        return (0..<num).compactMap {
            ptr?[$0]
        }
    }

    public var triangles: [UInt16] {
        let num = Int(spine_mesh_attachment_get_num_triangles(wrappee))
        let ptr = spine_mesh_attachment_get_triangles(wrappee)
        return (0..<num).compactMap {
            ptr?[$0]
        }
    }

    public var color: Color {
        return .init(spine_mesh_attachment_get_color(wrappee))
    }

    public var path: String? {
        return spine_mesh_attachment_get_path(wrappee).flatMap { String(cString: $0) }
    }

    public var region: TextureRegion {
        return .init(spine_mesh_attachment_get_region(wrappee))
    }

    public var sequence: Sequence? {
        return spine_mesh_attachment_get_sequence(wrappee).flatMap { .init($0) }
    }

    public var edges: [UInt16] {
        let num = Int(spine_mesh_attachment_get_num_edges(wrappee))
        let ptr = spine_mesh_attachment_get_edges(wrappee)
        return (0..<num).compactMap {
            ptr?[$0]
        }
    }

    public var hullLength: Int32 {
        get {
            return spine_mesh_attachment_get_hull_length(wrappee)
        }
        set {
            spine_mesh_attachment_set_hull_length(wrappee, newValue)
        }
    }

    public var parentMesh: MeshAttachment? {
        get {
            return spine_mesh_attachment_get_parent_mesh(wrappee).flatMap { .init($0) }
        }
        set {
            spine_mesh_attachment_set_parent_mesh(wrappee, newValue?.wrappee)
        }
    }

    public var width: Float {
        get {
            return spine_mesh_attachment_get_width(wrappee)
        }
        set {
            spine_mesh_attachment_set_width(wrappee, newValue)
        }
    }

    public var height: Float {
        get {
            return spine_mesh_attachment_get_height(wrappee)
        }
        set {
            spine_mesh_attachment_set_height(wrappee, newValue)
        }
    }

    public func updateRegion() {
        spine_mesh_attachment_update_region(wrappee)
    }

    public func setColor(r: Float, g: Float, b: Float, a: Float) {
        spine_mesh_attachment_set_color(wrappee, r, g, b, a)
    }

}

@objc(SpinePathAttachment)
@objcMembers
public final class PathAttachment: NSObject {

    internal let wrappee: spine_path_attachment

    internal init(_ wrappee: spine_path_attachment) {
        self.wrappee = wrappee
        super.init()
    }

    public var lengths: [Float?] {
        let num = Int(spine_path_attachment_get_num_lengths(wrappee))
        let ptr = spine_path_attachment_get_lengths(wrappee)
        return (0..<num).compactMap {
            ptr?[$0]
        }
    }

    public var color: Color {
        return .init(spine_path_attachment_get_color(wrappee))
    }

    public var isClosed: Bool {
        get {
            return spine_path_attachment_get_is_closed(wrappee) != 0
        }
        set {
            spine_path_attachment_set_is_closed(wrappee, newValue ? -1 : 0)
        }
    }

    public var isConstantSpeed: Bool {
        get {
            return spine_path_attachment_get_is_constant_speed(wrappee) != 0
        }
        set {
            spine_path_attachment_set_is_constant_speed(wrappee, newValue ? -1 : 0)
        }
    }

    public func setColor(r: Float, g: Float, b: Float, a: Float) {
        spine_path_attachment_set_color(wrappee, r, g, b, a)
    }

}

@objc(SpineConstraintData)
@objcMembers
public final class ConstraintData: NSObject {

    internal let wrappee: spine_constraint_data

    internal init(_ wrappee: spine_constraint_data) {
        self.wrappee = wrappee
        super.init()
    }

    public var type: ConstraintType {
        return spine_constraint_data_get_type(wrappee)
    }

    public var name: String? {
        return spine_constraint_data_get_name(wrappee).flatMap { String(cString: $0) }
    }

    public var order: UInt64 {
        get {
            return spine_constraint_data_get_order(wrappee)
        }
        set {
            spine_constraint_data_set_order(wrappee, newValue)
        }
    }

    public var isSkinRequired: Bool {
        get {
            return spine_constraint_data_get_is_skin_required(wrappee) != 0
        }
        set {
            spine_constraint_data_set_is_skin_required(wrappee, newValue ? -1 : 0)
        }
    }

}

@objc(SpinePathConstraint)
@objcMembers
public final class PathConstraint: NSObject {

    internal let wrappee: spine_path_constraint

    internal init(_ wrappee: spine_path_constraint) {
        self.wrappee = wrappee
        super.init()
    }

    public var order: Int32 {
        return spine_path_constraint_get_order(wrappee)
    }

    public var data: PathConstraintData {
        return .init(spine_path_constraint_get_data(wrappee))
    }

    public var bones: [Bone] {
        let num = Int(spine_path_constraint_get_num_bones(wrappee))
        let ptr = spine_path_constraint_get_bones(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var target: Slot {
        get {
            return .init(spine_path_constraint_get_target(wrappee))
        }
        set {
            spine_path_constraint_set_target(wrappee, newValue.wrappee)
        }
    }

    public var position: Float {
        get {
            return spine_path_constraint_get_position(wrappee)
        }
        set {
            spine_path_constraint_set_position(wrappee, newValue)
        }
    }

    public var spacing: Float {
        get {
            return spine_path_constraint_get_spacing(wrappee)
        }
        set {
            spine_path_constraint_set_spacing(wrappee, newValue)
        }
    }

    public var mixRotate: Float {
        get {
            return spine_path_constraint_get_mix_rotate(wrappee)
        }
        set {
            spine_path_constraint_set_mix_rotate(wrappee, newValue)
        }
    }

    public var mixX: Float {
        get {
            return spine_path_constraint_get_mix_x(wrappee)
        }
        set {
            spine_path_constraint_set_mix_x(wrappee, newValue)
        }
    }

    public var mixY: Float {
        get {
            return spine_path_constraint_get_mix_y(wrappee)
        }
        set {
            spine_path_constraint_set_mix_y(wrappee, newValue)
        }
    }

    public var isActive: Bool {
        get {
            return spine_path_constraint_get_is_active(wrappee) != 0
        }
        set {
            spine_path_constraint_set_is_active(wrappee, newValue ? -1 : 0)
        }
    }

    public func update() {
        spine_path_constraint_update(wrappee)
    }

}

@objc(SpineAnimationState)
@objcMembers
public final class AnimationState: NSObject {

    internal let wrappee: spine_animation_state

    internal init(_ wrappee: spine_animation_state) {
        self.wrappee = wrappee
        super.init()
    }

    public var data: AnimationStateData {
        return .init(spine_animation_state_get_data(wrappee))
    }

    public var timeScale: Float {
        get {
            return spine_animation_state_get_time_scale(wrappee)
        }
        set {
            spine_animation_state_set_time_scale(wrappee, newValue)
        }
    }

    public func update(delta: Float) {
        spine_animation_state_update(wrappee, delta)
    }

    public func apply(skeleton: Skeleton) {
        spine_animation_state_apply(wrappee, skeleton.wrappee)
    }

    public func clearTracks() {
        spine_animation_state_clear_tracks(wrappee)
    }

    public func clearTrack(trackIndex: Int32) {
        spine_animation_state_clear_track(wrappee, trackIndex)
    }

    @discardableResult
    public func setAnimationByName(trackIndex: Int32, animationName: String?, loop: Bool) -> TrackEntry {
        return .init(spine_animation_state_set_animation_by_name(wrappee, trackIndex, animationName, loop ? -1 : 0))
    }

    @discardableResult
    public func setAnimation(trackIndex: Int32, animation: Animation, loop: Bool) -> TrackEntry {
        return .init(spine_animation_state_set_animation(wrappee, trackIndex, animation.wrappee, loop ? -1 : 0))
    }

    @discardableResult
    public func addAnimationByName(trackIndex: Int32, animationName: String?, loop: Bool, delay: Float) -> TrackEntry {
        return .init(spine_animation_state_add_animation_by_name(wrappee, trackIndex, animationName, loop ? -1 : 0, delay))
    }

    @discardableResult
    public func addAnimation(trackIndex: Int32, animation: Animation, loop: Bool, delay: Float) -> TrackEntry {
        return .init(spine_animation_state_add_animation(wrappee, trackIndex, animation.wrappee, loop ? -1 : 0, delay))
    }

    @discardableResult
    public func setEmptyAnimation(trackIndex: Int32, mixDuration: Float) -> TrackEntry {
        return .init(spine_animation_state_set_empty_animation(wrappee, trackIndex, mixDuration))
    }

    @discardableResult
    public func addEmptyAnimation(trackIndex: Int32, mixDuration: Float, delay: Float) -> TrackEntry {
        return .init(spine_animation_state_add_empty_animation(wrappee, trackIndex, mixDuration, delay))
    }

    @discardableResult
    public func getCurrent(trackIndex: Int32) -> TrackEntry? {
        return spine_animation_state_get_current(wrappee, trackIndex).flatMap { .init($0) }
    }

    public func setEmptyAnimations(mixDuration: Float) {
        spine_animation_state_set_empty_animations(wrappee, mixDuration)
    }

}

@objc(SpineTextureRegion)
@objcMembers
public final class TextureRegion: NSObject {

    internal let wrappee: spine_texture_region

    internal init(_ wrappee: spine_texture_region) {
        self.wrappee = wrappee
        super.init()
    }

    public var texture: UnsafeMutableRawPointer {
        get {
            return spine_texture_region_get_texture(wrappee)
        }
        set {
            spine_texture_region_set_texture(wrappee, newValue)
        }
    }

    public var u: Float {
        get {
            return spine_texture_region_get_u(wrappee)
        }
        set {
            spine_texture_region_set_u(wrappee, newValue)
        }
    }

    public var v: Float {
        get {
            return spine_texture_region_get_v(wrappee)
        }
        set {
            spine_texture_region_set_v(wrappee, newValue)
        }
    }

    public var u2: Float {
        get {
            return spine_texture_region_get_u2(wrappee)
        }
        set {
            spine_texture_region_set_u2(wrappee, newValue)
        }
    }

    public var v2: Float {
        get {
            return spine_texture_region_get_v2(wrappee)
        }
        set {
            spine_texture_region_set_v2(wrappee, newValue)
        }
    }

    public var degrees: Int32 {
        get {
            return spine_texture_region_get_degrees(wrappee)
        }
        set {
            spine_texture_region_set_degrees(wrappee, newValue)
        }
    }

    public var offsetX: Float {
        get {
            return spine_texture_region_get_offset_x(wrappee)
        }
        set {
            spine_texture_region_set_offset_x(wrappee, newValue)
        }
    }

    public var offsetY: Float {
        get {
            return spine_texture_region_get_offset_y(wrappee)
        }
        set {
            spine_texture_region_set_offset_y(wrappee, newValue)
        }
    }

    public var width: Int32 {
        get {
            return spine_texture_region_get_width(wrappee)
        }
        set {
            spine_texture_region_set_width(wrappee, newValue)
        }
    }

    public var height: Int32 {
        get {
            return spine_texture_region_get_height(wrappee)
        }
        set {
            spine_texture_region_set_height(wrappee, newValue)
        }
    }

    public var originalWidth: Int32 {
        get {
            return spine_texture_region_get_original_width(wrappee)
        }
        set {
            spine_texture_region_set_original_width(wrappee, newValue)
        }
    }

    public var originalHeight: Int32 {
        get {
            return spine_texture_region_get_original_height(wrappee)
        }
        set {
            spine_texture_region_set_original_height(wrappee, newValue)
        }
    }

}

@objc(SpineRenderCommand)
@objcMembers
public final class RenderCommand: NSObject {

    internal let wrappee: spine_render_command

    internal init(_ wrappee: spine_render_command) {
        self.wrappee = wrappee
        super.init()
    }

    public var indices: [UInt16] {
        let num = Int(spine_render_command_get_num_indices(wrappee))
        let ptr = spine_render_command_get_indices(wrappee)
        return (0..<num).compactMap {
            ptr?[$0]
        }
    }

    public var atlasPage: Int32 {
        return spine_render_command_get_atlas_page(wrappee)
    }

    public var blendMode: BlendMode {
        return spine_render_command_get_blend_mode(wrappee)
    }

    public var next: RenderCommand {
        return .init(spine_render_command_get_next(wrappee))
    }

}

@objc(SpineSkeletonData)
@objcMembers
public final class SkeletonData: NSObject {

    internal let wrappee: spine_skeleton_data
    internal var disposed = false

    internal init(_ wrappee: spine_skeleton_data) {
        self.wrappee = wrappee
        super.init()
    }

    public var name: String? {
        return spine_skeleton_data_get_name(wrappee).flatMap { String(cString: $0) }
    }

    public var bones: [BoneData] {
        let num = Int(spine_skeleton_data_get_num_bones(wrappee))
        let ptr = spine_skeleton_data_get_bones(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var slots: [SlotData] {
        let num = Int(spine_skeleton_data_get_num_slots(wrappee))
        let ptr = spine_skeleton_data_get_slots(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var skins: [Skin] {
        let num = Int(spine_skeleton_data_get_num_skins(wrappee))
        let ptr = spine_skeleton_data_get_skins(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var events: [EventData] {
        let num = Int(spine_skeleton_data_get_num_events(wrappee))
        let ptr = spine_skeleton_data_get_events(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var animations: [Animation] {
        let num = Int(spine_skeleton_data_get_num_animations(wrappee))
        let ptr = spine_skeleton_data_get_animations(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var ikConstraints: [IkConstraintData] {
        let num = Int(spine_skeleton_data_get_num_ik_constraints(wrappee))
        let ptr = spine_skeleton_data_get_ik_constraints(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var transformConstraints: [TransformConstraintData] {
        let num = Int(spine_skeleton_data_get_num_transform_constraints(wrappee))
        let ptr = spine_skeleton_data_get_transform_constraints(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var pathConstraints: [PathConstraintData] {
        let num = Int(spine_skeleton_data_get_num_path_constraints(wrappee))
        let ptr = spine_skeleton_data_get_path_constraints(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var physicsConstraints: [PhysicsConstraintData] {
        let num = Int(spine_skeleton_data_get_num_physics_constraints(wrappee))
        let ptr = spine_skeleton_data_get_physics_constraints(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var version: String? {
        return spine_skeleton_data_get_version(wrappee).flatMap { String(cString: $0) }
    }

    public var imagesPath: String? {
        return spine_skeleton_data_get_images_path(wrappee).flatMap { String(cString: $0) }
    }

    public var audioPath: String? {
        return spine_skeleton_data_get_audio_path(wrappee).flatMap { String(cString: $0) }
    }

    public var fps: Float {
        return spine_skeleton_data_get_fps(wrappee)
    }

    public var referenceScale: Float {
        return spine_skeleton_data_get_reference_scale(wrappee)
    }

    public var defaultSkin: Skin? {
        get {
            return spine_skeleton_data_get_default_skin(wrappee).flatMap { .init($0) }
        }
        set {
            spine_skeleton_data_set_default_skin(wrappee, newValue?.wrappee)
        }
    }

    public var x: Float {
        get {
            return spine_skeleton_data_get_x(wrappee)
        }
        set {
            spine_skeleton_data_set_x(wrappee, newValue)
        }
    }

    public var y: Float {
        get {
            return spine_skeleton_data_get_y(wrappee)
        }
        set {
            spine_skeleton_data_set_y(wrappee, newValue)
        }
    }

    public var width: Float {
        get {
            return spine_skeleton_data_get_width(wrappee)
        }
        set {
            spine_skeleton_data_set_width(wrappee, newValue)
        }
    }

    public var height: Float {
        get {
            return spine_skeleton_data_get_height(wrappee)
        }
        set {
            spine_skeleton_data_set_height(wrappee, newValue)
        }
    }

    @discardableResult
    public func findBone(name: String?) -> BoneData? {
        return spine_skeleton_data_find_bone(wrappee, name).flatMap { .init($0) }
    }

    @discardableResult
    public func findSlot(name: String?) -> SlotData? {
        return spine_skeleton_data_find_slot(wrappee, name).flatMap { .init($0) }
    }

    @discardableResult
    public func findSkin(name: String?) -> Skin? {
        return spine_skeleton_data_find_skin(wrappee, name).flatMap { .init($0) }
    }

    @discardableResult
    public func findEvent(name: String?) -> EventData? {
        return spine_skeleton_data_find_event(wrappee, name).flatMap { .init($0) }
    }

    @discardableResult
    public func findAnimation(name: String?) -> Animation? {
        return spine_skeleton_data_find_animation(wrappee, name).flatMap { .init($0) }
    }

    @discardableResult
    public func findIkConstraint(name: String?) -> IkConstraintData? {
        return spine_skeleton_data_find_ik_constraint(wrappee, name).flatMap { .init($0) }
    }

    @discardableResult
    public func findTransformConstraint(name: String?) -> TransformConstraintData? {
        return spine_skeleton_data_find_transform_constraint(wrappee, name).flatMap { .init($0) }
    }

    @discardableResult
    public func findPathConstraint(name: String?) -> PathConstraintData? {
        return spine_skeleton_data_find_path_constraint(wrappee, name).flatMap { .init($0) }
    }

    @discardableResult
    public func findPhysicsConstraint(name: String?) -> PhysicsConstraintData? {
        return spine_skeleton_data_find_physics_constraint(wrappee, name).flatMap { .init($0) }
    }

    public func dispose() {
        if disposed { return }
        disposed = true
        spine_skeleton_data_dispose(wrappee)
    }

}

@objc(SpineIkConstraint)
@objcMembers
public final class IkConstraint: NSObject {

    internal let wrappee: spine_ik_constraint

    internal init(_ wrappee: spine_ik_constraint) {
        self.wrappee = wrappee
        super.init()
    }

    public var order: Int32 {
        return spine_ik_constraint_get_order(wrappee)
    }

    public var data: IkConstraintData {
        return .init(spine_ik_constraint_get_data(wrappee))
    }

    public var bones: [Bone] {
        let num = Int(spine_ik_constraint_get_num_bones(wrappee))
        let ptr = spine_ik_constraint_get_bones(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var target: Bone {
        get {
            return .init(spine_ik_constraint_get_target(wrappee))
        }
        set {
            spine_ik_constraint_set_target(wrappee, newValue.wrappee)
        }
    }

    public var bendDirection: Int32 {
        get {
            return spine_ik_constraint_get_bend_direction(wrappee)
        }
        set {
            spine_ik_constraint_set_bend_direction(wrappee, newValue)
        }
    }

    public var compress: Bool {
        get {
            return spine_ik_constraint_get_compress(wrappee) != 0
        }
        set {
            spine_ik_constraint_set_compress(wrappee, newValue ? -1 : 0)
        }
    }

    public var stretch: Bool {
        get {
            return spine_ik_constraint_get_stretch(wrappee) != 0
        }
        set {
            spine_ik_constraint_set_stretch(wrappee, newValue ? -1 : 0)
        }
    }

    public var mix: Float {
        get {
            return spine_ik_constraint_get_mix(wrappee)
        }
        set {
            spine_ik_constraint_set_mix(wrappee, newValue)
        }
    }

    public var softness: Float {
        get {
            return spine_ik_constraint_get_softness(wrappee)
        }
        set {
            spine_ik_constraint_set_softness(wrappee, newValue)
        }
    }

    public var isActive: Bool {
        get {
            return spine_ik_constraint_get_is_active(wrappee) != 0
        }
        set {
            spine_ik_constraint_set_is_active(wrappee, newValue ? -1 : 0)
        }
    }

    public func update() {
        spine_ik_constraint_update(wrappee)
    }

}

@objc(SpineSkinEntries)
@objcMembers
public final class SkinEntries: NSObject {

    internal let wrappee: spine_skin_entries
    internal var disposed = false

    internal init(_ wrappee: spine_skin_entries) {
        self.wrappee = wrappee
        super.init()
    }

    @discardableResult
    public func getEntry(index: Int32) -> SkinEntry {
        return .init(spine_skin_entries_get_entry(wrappee, index))
    }

    public func dispose() {
        if disposed { return }
        disposed = true
        spine_skin_entries_dispose(wrappee)
    }

}

@objc(SpineTrackEntry)
@objcMembers
public final class TrackEntry: NSObject {

    internal let wrappee: spine_track_entry

    internal init(_ wrappee: spine_track_entry) {
        self.wrappee = wrappee
        super.init()
    }

    public var trackIndex: Int32 {
        return spine_track_entry_get_track_index(wrappee)
    }

    public var animation: Animation {
        return .init(spine_track_entry_get_animation(wrappee))
    }

    public var previous: TrackEntry {
        return .init(spine_track_entry_get_previous(wrappee))
    }

    public var animationTime: Float {
        return spine_track_entry_get_animation_time(wrappee)
    }

    public var next: TrackEntry? {
        return spine_track_entry_get_next(wrappee).flatMap { .init($0) }
    }

    public var isComplete: Bool {
        return spine_track_entry_is_complete(wrappee) != 0
    }

    public var mixingFrom: TrackEntry? {
        return spine_track_entry_get_mixing_from(wrappee).flatMap { .init($0) }
    }

    public var mixingTo: TrackEntry? {
        return spine_track_entry_get_mixing_to(wrappee).flatMap { .init($0) }
    }

    public var trackComplete: Float {
        return spine_track_entry_get_track_complete(wrappee)
    }

    public var isNextReady: Bool {
        return spine_track_entry_is_next_ready(wrappee) != 0
    }

    public var loop: Bool {
        get {
            return spine_track_entry_get_loop(wrappee) != 0
        }
        set {
            spine_track_entry_set_loop(wrappee, newValue ? -1 : 0)
        }
    }

    public var holdPrevious: Bool {
        get {
            return spine_track_entry_get_hold_previous(wrappee) != 0
        }
        set {
            spine_track_entry_set_hold_previous(wrappee, newValue ? -1 : 0)
        }
    }

    public var reverse: Bool {
        get {
            return spine_track_entry_get_reverse(wrappee) != 0
        }
        set {
            spine_track_entry_set_reverse(wrappee, newValue ? -1 : 0)
        }
    }

    public var shortestRotation: Bool {
        get {
            return spine_track_entry_get_shortest_rotation(wrappee) != 0
        }
        set {
            spine_track_entry_set_shortest_rotation(wrappee, newValue ? -1 : 0)
        }
    }

    public var delay: Float {
        get {
            return spine_track_entry_get_delay(wrappee)
        }
        set {
            spine_track_entry_set_delay(wrappee, newValue)
        }
    }

    public var trackTime: Float {
        get {
            return spine_track_entry_get_track_time(wrappee)
        }
        set {
            spine_track_entry_set_track_time(wrappee, newValue)
        }
    }

    public var trackEnd: Float {
        get {
            return spine_track_entry_get_track_end(wrappee)
        }
        set {
            spine_track_entry_set_track_end(wrappee, newValue)
        }
    }

    public var animationStart: Float {
        get {
            return spine_track_entry_get_animation_start(wrappee)
        }
        set {
            spine_track_entry_set_animation_start(wrappee, newValue)
        }
    }

    public var animationEnd: Float {
        get {
            return spine_track_entry_get_animation_end(wrappee)
        }
        set {
            spine_track_entry_set_animation_end(wrappee, newValue)
        }
    }

    public var animationLast: Float {
        get {
            return spine_track_entry_get_animation_last(wrappee)
        }
        set {
            spine_track_entry_set_animation_last(wrappee, newValue)
        }
    }

    public var timeScale: Float {
        get {
            return spine_track_entry_get_time_scale(wrappee)
        }
        set {
            spine_track_entry_set_time_scale(wrappee, newValue)
        }
    }

    public var alpha: Float {
        get {
            return spine_track_entry_get_alpha(wrappee)
        }
        set {
            spine_track_entry_set_alpha(wrappee, newValue)
        }
    }

    public var eventThreshold: Float {
        get {
            return spine_track_entry_get_event_threshold(wrappee)
        }
        set {
            spine_track_entry_set_event_threshold(wrappee, newValue)
        }
    }

    public var alphaAttachmentThreshold: Float {
        get {
            return spine_track_entry_get_alpha_attachment_threshold(wrappee)
        }
        set {
            spine_track_entry_set_alpha_attachment_threshold(wrappee, newValue)
        }
    }

    public var mixAttachmentThreshold: Float {
        get {
            return spine_track_entry_get_mix_attachment_threshold(wrappee)
        }
        set {
            spine_track_entry_set_mix_attachment_threshold(wrappee, newValue)
        }
    }

    public var mixDrawOrderThreshold: Float {
        get {
            return spine_track_entry_get_mix_draw_order_threshold(wrappee)
        }
        set {
            spine_track_entry_set_mix_draw_order_threshold(wrappee, newValue)
        }
    }

    public var mixTime: Float {
        get {
            return spine_track_entry_get_mix_time(wrappee)
        }
        set {
            spine_track_entry_set_mix_time(wrappee, newValue)
        }
    }

    public var mixDuration: Float {
        get {
            return spine_track_entry_get_mix_duration(wrappee)
        }
        set {
            spine_track_entry_set_mix_duration(wrappee, newValue)
        }
    }

    public var mixBlend: MixBlend {
        get {
            return spine_track_entry_get_mix_blend(wrappee)
        }
        set {
            spine_track_entry_set_mix_blend(wrappee, newValue)
        }
    }

    public func resetRotationDirections() {
        spine_track_entry_reset_rotation_directions(wrappee)
    }

    @discardableResult
    public func wasApplied() -> Bool {
        return spine_track_entry_was_applied(wrappee) != 0
    }

}

@objc(SpineAttachment)
@objcMembers
public final class Attachment: NSObject {

    internal let wrappee: spine_attachment
    internal var disposed = false

    internal init(_ wrappee: spine_attachment) {
        self.wrappee = wrappee
        super.init()
    }

    public var name: String? {
        return spine_attachment_get_name(wrappee).flatMap { String(cString: $0) }
    }

    public var type: AttachmentType {
        return spine_attachment_get_type(wrappee)
    }

    public func dispose() {
        if disposed { return }
        disposed = true
        spine_attachment_dispose(wrappee)
    }

}

@objc(SpineConstraint)
@objcMembers
public final class Constraint: NSObject {

    internal let wrappee: spine_constraint

    internal init(_ wrappee: spine_constraint) {
        self.wrappee = wrappee
        super.init()
    }

}

@objc(SpineEventData)
@objcMembers
public final class EventData: NSObject {

    internal let wrappee: spine_event_data

    internal init(_ wrappee: spine_event_data) {
        self.wrappee = wrappee
        super.init()
    }

    public var name: String? {
        return spine_event_data_get_name(wrappee).flatMap { String(cString: $0) }
    }

    public var audioPath: String? {
        return spine_event_data_get_audio_path(wrappee).flatMap { String(cString: $0) }
    }

    public var intValue: Int32 {
        get {
            return spine_event_data_get_int_value(wrappee)
        }
        set {
            spine_event_data_set_int_value(wrappee, newValue)
        }
    }

    public var floatValue: Float {
        get {
            return spine_event_data_get_float_value(wrappee)
        }
        set {
            spine_event_data_set_float_value(wrappee, newValue)
        }
    }

    public var stringValue: String? {
        get {
            return spine_event_data_get_string_value(wrappee).flatMap { String(cString: $0) }
        }
        set {
            spine_event_data_set_string_value(wrappee, newValue)
        }
    }

    public var volume: Float {
        get {
            return spine_event_data_get_volume(wrappee)
        }
        set {
            spine_event_data_set_volume(wrappee, newValue)
        }
    }

    public var balance: Float {
        get {
            return spine_event_data_get_balance(wrappee)
        }
        set {
            spine_event_data_set_balance(wrappee, newValue)
        }
    }

}

@objc(SpineSkinEntry)
@objcMembers
public final class SkinEntry: NSObject {

    internal let wrappee: spine_skin_entry

    internal init(_ wrappee: spine_skin_entry) {
        self.wrappee = wrappee
        super.init()
    }

    public var slotIndex: Int32 {
        return spine_skin_entry_get_slot_index(wrappee)
    }

    public var name: String? {
        return spine_skin_entry_get_name(wrappee).flatMap { String(cString: $0) }
    }

    public var attachment: Attachment {
        return .init(spine_skin_entry_get_attachment(wrappee))
    }

}

@objc(SpineBoneData)
@objcMembers
public final class BoneData: NSObject {

    internal let wrappee: spine_bone_data

    internal init(_ wrappee: spine_bone_data) {
        self.wrappee = wrappee
        super.init()
    }

    public var index: Int32 {
        return spine_bone_data_get_index(wrappee)
    }

    public var name: String? {
        return spine_bone_data_get_name(wrappee).flatMap { String(cString: $0) }
    }

    public var parent: BoneData? {
        return spine_bone_data_get_parent(wrappee).flatMap { .init($0) }
    }

    public var color: Color {
        return .init(spine_bone_data_get_color(wrappee))
    }

    public var length: Float {
        get {
            return spine_bone_data_get_length(wrappee)
        }
        set {
            spine_bone_data_set_length(wrappee, newValue)
        }
    }

    public var x: Float {
        get {
            return spine_bone_data_get_x(wrappee)
        }
        set {
            spine_bone_data_set_x(wrappee, newValue)
        }
    }

    public var y: Float {
        get {
            return spine_bone_data_get_y(wrappee)
        }
        set {
            spine_bone_data_set_y(wrappee, newValue)
        }
    }

    public var rotation: Float {
        get {
            return spine_bone_data_get_rotation(wrappee)
        }
        set {
            spine_bone_data_set_rotation(wrappee, newValue)
        }
    }

    public var scaleX: Float {
        get {
            return spine_bone_data_get_scale_x(wrappee)
        }
        set {
            spine_bone_data_set_scale_x(wrappee, newValue)
        }
    }

    public var scaleY: Float {
        get {
            return spine_bone_data_get_scale_y(wrappee)
        }
        set {
            spine_bone_data_set_scale_y(wrappee, newValue)
        }
    }

    public var shearX: Float {
        get {
            return spine_bone_data_get_shear_x(wrappee)
        }
        set {
            spine_bone_data_set_shear_x(wrappee, newValue)
        }
    }

    public var shearY: Float {
        get {
            return spine_bone_data_get_shear_y(wrappee)
        }
        set {
            spine_bone_data_set_shear_y(wrappee, newValue)
        }
    }

    public var inherit: Inherit {
        get {
            return spine_bone_data_get_inherit(wrappee)
        }
        set {
            spine_bone_data_set_inherit(wrappee, newValue)
        }
    }

    public var isSkinRequired: Bool {
        get {
            return spine_bone_data_get_is_skin_required(wrappee) != 0
        }
        set {
            spine_bone_data_set_is_skin_required(wrappee, newValue ? -1 : 0)
        }
    }

    public var isVisible: Bool {
        get {
            return spine_bone_data_is_visible(wrappee) != 0
        }
        set {
            spine_bone_data_set_visible(wrappee, newValue ? -1 : 0)
        }
    }

    public func setColor(r: Float, g: Float, b: Float, a: Float) {
        spine_bone_data_set_color(wrappee, r, g, b, a)
    }

}

@objc(SpineSlotData)
@objcMembers
public final class SlotData: NSObject {

    internal let wrappee: spine_slot_data

    internal init(_ wrappee: spine_slot_data) {
        self.wrappee = wrappee
        super.init()
    }

    public var index: Int32 {
        return spine_slot_data_get_index(wrappee)
    }

    public var name: String? {
        return spine_slot_data_get_name(wrappee).flatMap { String(cString: $0) }
    }

    public var boneData: BoneData {
        return .init(spine_slot_data_get_bone_data(wrappee))
    }

    public var color: Color {
        return .init(spine_slot_data_get_color(wrappee))
    }

    public var darkColor: Color {
        return .init(spine_slot_data_get_dark_color(wrappee))
    }

    public var hasDarkColor: Bool {
        get {
            return spine_slot_data_get_has_dark_color(wrappee) != 0
        }
        set {
            spine_slot_data_set_has_dark_color(wrappee, newValue ? -1 : 0)
        }
    }

    public var attachmentName: String? {
        get {
            return spine_slot_data_get_attachment_name(wrappee).flatMap { String(cString: $0) }
        }
        set {
            spine_slot_data_set_attachment_name(wrappee, newValue)
        }
    }

    public var blendMode: BlendMode {
        get {
            return spine_slot_data_get_blend_mode(wrappee)
        }
        set {
            spine_slot_data_set_blend_mode(wrappee, newValue)
        }
    }

    public var isVisible: Bool {
        get {
            return spine_slot_data_is_visible(wrappee) != 0
        }
        set {
            spine_slot_data_set_visible(wrappee, newValue ? -1 : 0)
        }
    }

    public func setColor(r: Float, g: Float, b: Float, a: Float) {
        spine_slot_data_set_color(wrappee, r, g, b, a)
    }

    public func setDarkColor(r: Float, g: Float, b: Float, a: Float) {
        spine_slot_data_set_dark_color(wrappee, r, g, b, a)
    }

}

@objc(SpineAnimation)
@objcMembers
public final class Animation: NSObject {

    internal let wrappee: spine_animation

    internal init(_ wrappee: spine_animation) {
        self.wrappee = wrappee
        super.init()
    }

    public var name: String? {
        return spine_animation_get_name(wrappee).flatMap { String(cString: $0) }
    }

    public var duration: Float {
        return spine_animation_get_duration(wrappee)
    }

}

@objc(SpineSkeleton)
@objcMembers
public final class Skeleton: NSObject {

    internal let wrappee: spine_skeleton

    internal init(_ wrappee: spine_skeleton) {
        self.wrappee = wrappee
        super.init()
    }

    public var bounds: Bounds {
        return .init(spine_skeleton_get_bounds(wrappee))
    }

    public var rootBone: Bone? {
        return spine_skeleton_get_root_bone(wrappee).flatMap { .init($0) }
    }

    public var data: SkeletonData? {
        return spine_skeleton_get_data(wrappee).flatMap { .init($0) }
    }

    public var bones: [Bone] {
        let num = Int(spine_skeleton_get_num_bones(wrappee))
        let ptr = spine_skeleton_get_bones(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var slots: [Slot] {
        let num = Int(spine_skeleton_get_num_slots(wrappee))
        let ptr = spine_skeleton_get_slots(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var drawOrder: [Slot] {
        let num = Int(spine_skeleton_get_num_draw_order(wrappee))
        let ptr = spine_skeleton_get_draw_order(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var ikConstraints: [IkConstraint] {
        let num = Int(spine_skeleton_get_num_ik_constraints(wrappee))
        let ptr = spine_skeleton_get_ik_constraints(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var transformConstraints: [TransformConstraint] {
        let num = Int(spine_skeleton_get_num_transform_constraints(wrappee))
        let ptr = spine_skeleton_get_transform_constraints(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var pathConstraints: [PathConstraint] {
        let num = Int(spine_skeleton_get_num_path_constraints(wrappee))
        let ptr = spine_skeleton_get_path_constraints(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var physicsConstraints: [PhysicsConstraint] {
        let num = Int(spine_skeleton_get_num_physics_constraints(wrappee))
        let ptr = spine_skeleton_get_physics_constraints(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var color: Color {
        return .init(spine_skeleton_get_color(wrappee))
    }

    public var skin: Skin? {
        get {
            return spine_skeleton_get_skin(wrappee).flatMap { .init($0) }
        }
        set {
            spine_skeleton_set_skin(wrappee, newValue?.wrappee)
        }
    }

    public var x: Float {
        get {
            return spine_skeleton_get_x(wrappee)
        }
        set {
            spine_skeleton_set_x(wrappee, newValue)
        }
    }

    public var y: Float {
        get {
            return spine_skeleton_get_y(wrappee)
        }
        set {
            spine_skeleton_set_y(wrappee, newValue)
        }
    }

    public var scaleX: Float {
        get {
            return spine_skeleton_get_scale_x(wrappee)
        }
        set {
            spine_skeleton_set_scale_x(wrappee, newValue)
        }
    }

    public var scaleY: Float {
        get {
            return spine_skeleton_get_scale_y(wrappee)
        }
        set {
            spine_skeleton_set_scale_y(wrappee, newValue)
        }
    }

    public var time: Float {
        get {
            return spine_skeleton_get_time(wrappee)
        }
        set {
            spine_skeleton_set_time(wrappee, newValue)
        }
    }

    public func updateCache() {
        spine_skeleton_update_cache(wrappee)
    }

    public func updateWorldTransform(physics: Physics) {
        spine_skeleton_update_world_transform(wrappee, physics)
    }

    public func updateWorldTransformBone(physics: Physics, parent: Bone) {
        spine_skeleton_update_world_transform_bone(wrappee, physics, parent.wrappee)
    }

    public func setToSetupPose() {
        spine_skeleton_set_to_setup_pose(wrappee)
    }

    public func setBonesToSetupPose() {
        spine_skeleton_set_bones_to_setup_pose(wrappee)
    }

    public func setSlotsToSetupPose() {
        spine_skeleton_set_slots_to_setup_pose(wrappee)
    }

    @discardableResult
    public func findBone(boneName: String?) -> Bone? {
        return spine_skeleton_find_bone(wrappee, boneName).flatMap { .init($0) }
    }

    @discardableResult
    public func findSlot(slotName: String?) -> Slot? {
        return spine_skeleton_find_slot(wrappee, slotName).flatMap { .init($0) }
    }

    @discardableResult
    public func getAttachmentByName(slotName: String?, attachmentName: String?) -> Attachment? {
        return spine_skeleton_get_attachment_by_name(wrappee, slotName, attachmentName).flatMap { .init($0) }
    }

    @discardableResult
    public func getAttachment(slotIndex: Int32, attachmentName: String?) -> Attachment? {
        return spine_skeleton_get_attachment(wrappee, slotIndex, attachmentName).flatMap { .init($0) }
    }

    public func setAttachment(slotName: String?, attachmentName: String?) {
        spine_skeleton_set_attachment(wrappee, slotName, attachmentName)
    }

    @discardableResult
    public func findIkConstraint(constraintName: String?) -> IkConstraint? {
        return spine_skeleton_find_ik_constraint(wrappee, constraintName).flatMap { .init($0) }
    }

    @discardableResult
    public func findTransformConstraint(constraintName: String?) -> TransformConstraint? {
        return spine_skeleton_find_transform_constraint(wrappee, constraintName).flatMap { .init($0) }
    }

    @discardableResult
    public func findPathConstraint(constraintName: String?) -> PathConstraint? {
        return spine_skeleton_find_path_constraint(wrappee, constraintName).flatMap { .init($0) }
    }

    @discardableResult
    public func findPhysicsConstraint(constraintName: String?) -> PhysicsConstraint? {
        return spine_skeleton_find_physics_constraint(wrappee, constraintName).flatMap { .init($0) }
    }

    public func setColor(r: Float, g: Float, b: Float, a: Float) {
        spine_skeleton_set_color(wrappee, r, g, b, a)
    }

    public func setPosition(x: Float, y: Float) {
        spine_skeleton_set_position(wrappee, x, y)
    }

    public func setScale(scaleX: Float, scaleY: Float) {
        spine_skeleton_set_scale(wrappee, scaleX, scaleY)
    }

    public func update(delta: Float) {
        spine_skeleton_update(wrappee, delta)
    }

    public func setSkinByName(skinName: String?) {
        spine_skeleton_set_skin_by_name(wrappee, skinName)
    }

}

@objc(SpineSequence)
@objcMembers
public final class Sequence: NSObject {

    internal let wrappee: spine_sequence

    internal init(_ wrappee: spine_sequence) {
        self.wrappee = wrappee
        super.init()
    }

    public var regions: [TextureRegion] {
        let num = Int(spine_sequence_get_num_regions(wrappee))
        let ptr = spine_sequence_get_regions(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var id: Int32 {
        get {
            return spine_sequence_get_id(wrappee)
        }
        set {
            spine_sequence_set_id(wrappee, newValue)
        }
    }

    public var start: Int32 {
        get {
            return spine_sequence_get_start(wrappee)
        }
        set {
            spine_sequence_set_start(wrappee, newValue)
        }
    }

    public var digits: Int32 {
        get {
            return spine_sequence_get_digits(wrappee)
        }
        set {
            spine_sequence_set_digits(wrappee, newValue)
        }
    }

    public var setupIndex: Int32 {
        get {
            return spine_sequence_get_setup_index(wrappee)
        }
        set {
            spine_sequence_set_setup_index(wrappee, newValue)
        }
    }

    public func apply(slot: Slot, attachment: Attachment) {
        spine_sequence_apply(wrappee, slot.wrappee, attachment.wrappee)
    }

    @discardableResult
    public func getPath(basePath: String?, index: Int32) -> String? {
        return spine_sequence_get_path(wrappee, basePath, index).flatMap { String(cString: $0) }
    }

}

@objc(SpineBounds)
@objcMembers
public final class Bounds: NSObject {

    internal let wrappee: spine_bounds

    internal init(_ wrappee: spine_bounds) {
        self.wrappee = wrappee
        super.init()
    }

    public var x: Float {
        return spine_bounds_get_x(wrappee)
    }

    public var y: Float {
        return spine_bounds_get_y(wrappee)
    }

    public var width: Float {
        return spine_bounds_get_width(wrappee)
    }

    public var height: Float {
        return spine_bounds_get_height(wrappee)
    }

}

@objc(SpineVector)
@objcMembers
public final class Vector: NSObject {

    internal let wrappee: spine_vector

    internal init(_ wrappee: spine_vector) {
        self.wrappee = wrappee
        super.init()
    }

    public var x: Float {
        return spine_vector_get_x(wrappee)
    }

    public var y: Float {
        return spine_vector_get_y(wrappee)
    }

}

@objc(SpineEvent)
@objcMembers
public final class Event: NSObject {

    internal let wrappee: spine_event

    internal init(_ wrappee: spine_event) {
        self.wrappee = wrappee
        super.init()
    }

    public var data: EventData {
        return .init(spine_event_get_data(wrappee))
    }

    public var time: Float {
        return spine_event_get_time(wrappee)
    }

    public var intValue: Int32 {
        get {
            return spine_event_get_int_value(wrappee)
        }
        set {
            spine_event_set_int_value(wrappee, newValue)
        }
    }

    public var floatValue: Float {
        get {
            return spine_event_get_float_value(wrappee)
        }
        set {
            spine_event_set_float_value(wrappee, newValue)
        }
    }

    public var stringValue: String? {
        get {
            return spine_event_get_string_value(wrappee).flatMap { String(cString: $0) }
        }
        set {
            spine_event_set_string_value(wrappee, newValue)
        }
    }

    public var volume: Float {
        get {
            return spine_event_get_volume(wrappee)
        }
        set {
            spine_event_set_volume(wrappee, newValue)
        }
    }

    public var balance: Float {
        get {
            return spine_event_get_balance(wrappee)
        }
        set {
            spine_event_set_balance(wrappee, newValue)
        }
    }

}

@objc(SpineAtlas)
@objcMembers
public final class Atlas: NSObject {

    internal let wrappee: spine_atlas
    internal var disposed = false

    internal init(_ wrappee: spine_atlas) {
        self.wrappee = wrappee
        super.init()
    }

    public var isPma: Bool {
        return spine_atlas_is_pma(wrappee) != 0
    }

    public var error: String? {
        return spine_atlas_get_error(wrappee).flatMap { String(cString: $0) }
    }

    @discardableResult
    public func load(atlasData: String?) -> Atlas {
        return .init(spine_atlas_load(atlasData))
    }

    @discardableResult
    public func getImagePath(index: Int32) -> String? {
        return spine_atlas_get_image_path(wrappee, index).flatMap { String(cString: $0) }
    }

    public func dispose() {
        if disposed { return }
        disposed = true
        spine_atlas_dispose(wrappee)
    }

}

@objc(SpineColor)
@objcMembers
public final class Color: NSObject {

    internal let wrappee: spine_color

    internal init(_ wrappee: spine_color) {
        self.wrappee = wrappee
        super.init()
    }

    public var r: Float {
        return spine_color_get_r(wrappee)
    }

    public var g: Float {
        return spine_color_get_g(wrappee)
    }

    public var b: Float {
        return spine_color_get_b(wrappee)
    }

    public var a: Float {
        return spine_color_get_a(wrappee)
    }

}

@objc(SpineBone)
@objcMembers
public final class Bone: NSObject {

    internal let wrappee: spine_bone

    internal init(_ wrappee: spine_bone) {
        self.wrappee = wrappee
        super.init()
    }

    public func setIsYDown(yDown: Bool) {
        spine_bone_set_is_y_down(yDown ? -1 : 0)
    }

    public var worldToLocalRotationX: Float {
        return spine_bone_get_world_to_local_rotation_x(wrappee)
    }

    public var worldToLocalRotationY: Float {
        return spine_bone_get_world_to_local_rotation_y(wrappee)
    }

    public var data: BoneData {
        return .init(spine_bone_get_data(wrappee))
    }

    public var skeleton: Skeleton {
        return .init(spine_bone_get_skeleton(wrappee))
    }

    public var parent: Bone? {
        return spine_bone_get_parent(wrappee).flatMap { .init($0) }
    }

    public var children: [Bone] {
        let num = Int(spine_bone_get_num_children(wrappee))
        let ptr = spine_bone_get_children(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var worldRotationX: Float {
        return spine_bone_get_world_rotation_x(wrappee)
    }

    public var worldRotationY: Float {
        return spine_bone_get_world_rotation_y(wrappee)
    }

    public var worldScaleX: Float {
        return spine_bone_get_world_scale_x(wrappee)
    }

    public var worldScaleY: Float {
        return spine_bone_get_world_scale_y(wrappee)
    }

    public var x: Float {
        get {
            return spine_bone_get_x(wrappee)
        }
        set {
            spine_bone_set_x(wrappee, newValue)
        }
    }

    public var y: Float {
        get {
            return spine_bone_get_y(wrappee)
        }
        set {
            spine_bone_set_y(wrappee, newValue)
        }
    }

    public var rotation: Float {
        get {
            return spine_bone_get_rotation(wrappee)
        }
        set {
            spine_bone_set_rotation(wrappee, newValue)
        }
    }

    public var scaleX: Float {
        get {
            return spine_bone_get_scale_x(wrappee)
        }
        set {
            spine_bone_set_scale_x(wrappee, newValue)
        }
    }

    public var scaleY: Float {
        get {
            return spine_bone_get_scale_y(wrappee)
        }
        set {
            spine_bone_set_scale_y(wrappee, newValue)
        }
    }

    public var shearX: Float {
        get {
            return spine_bone_get_shear_x(wrappee)
        }
        set {
            spine_bone_set_shear_x(wrappee, newValue)
        }
    }

    public var shearY: Float {
        get {
            return spine_bone_get_shear_y(wrappee)
        }
        set {
            spine_bone_set_shear_y(wrappee, newValue)
        }
    }

    public var appliedRotation: Float {
        get {
            return spine_bone_get_applied_rotation(wrappee)
        }
        set {
            spine_bone_set_applied_rotation(wrappee, newValue)
        }
    }

    public var aX: Float {
        get {
            return spine_bone_get_a_x(wrappee)
        }
        set {
            spine_bone_set_a_x(wrappee, newValue)
        }
    }

    public var aY: Float {
        get {
            return spine_bone_get_a_y(wrappee)
        }
        set {
            spine_bone_set_a_y(wrappee, newValue)
        }
    }

    public var aScaleX: Float {
        get {
            return spine_bone_get_a_scale_x(wrappee)
        }
        set {
            spine_bone_set_a_scale_x(wrappee, newValue)
        }
    }

    public var aScaleY: Float {
        get {
            return spine_bone_get_a_scale_y(wrappee)
        }
        set {
            spine_bone_set_a_scale_y(wrappee, newValue)
        }
    }

    public var aShearX: Float {
        get {
            return spine_bone_get_a_shear_x(wrappee)
        }
        set {
            spine_bone_set_a_shear_x(wrappee, newValue)
        }
    }

    public var aShearY: Float {
        get {
            return spine_bone_get_a_shear_y(wrappee)
        }
        set {
            spine_bone_set_a_shear_y(wrappee, newValue)
        }
    }

    public var a: Float {
        get {
            return spine_bone_get_a(wrappee)
        }
        set {
            spine_bone_set_a(wrappee, newValue)
        }
    }

    public var b: Float {
        get {
            return spine_bone_get_b(wrappee)
        }
        set {
            spine_bone_set_b(wrappee, newValue)
        }
    }

    public var c: Float {
        get {
            return spine_bone_get_c(wrappee)
        }
        set {
            spine_bone_set_c(wrappee, newValue)
        }
    }

    public var d: Float {
        get {
            return spine_bone_get_d(wrappee)
        }
        set {
            spine_bone_set_d(wrappee, newValue)
        }
    }

    public var worldX: Float {
        get {
            return spine_bone_get_world_x(wrappee)
        }
        set {
            spine_bone_set_world_x(wrappee, newValue)
        }
    }

    public var worldY: Float {
        get {
            return spine_bone_get_world_y(wrappee)
        }
        set {
            spine_bone_set_world_y(wrappee, newValue)
        }
    }

    public var isActive: Bool {
        get {
            return spine_bone_get_is_active(wrappee) != 0
        }
        set {
            spine_bone_set_is_active(wrappee, newValue ? -1 : 0)
        }
    }

    public var inherit: Inherit {
        get {
            return spine_bone_get_inherit(wrappee)
        }
        set {
            spine_bone_set_inherit(wrappee, newValue)
        }
    }

    public var isYDown: Bool {
        return spine_bone_get_is_y_down() != 0
    }

    public func update() {
        spine_bone_update(wrappee)
    }

    public func updateWorldTransform() {
        spine_bone_update_world_transform(wrappee)
    }

    public func updateWorldTransformWith(x: Float, y: Float, rotation: Float, scaleX: Float, scaleY: Float, shearX: Float, shearY: Float) {
        spine_bone_update_world_transform_with(wrappee, x, y, rotation, scaleX, scaleY, shearX, shearY)
    }

    public func updateAppliedTransform() {
        spine_bone_update_applied_transform(wrappee)
    }

    public func setToSetupPose() {
        spine_bone_set_to_setup_pose(wrappee)
    }

    @discardableResult
    public func worldToLocal(worldX: Float, worldY: Float) -> Vector {
        return .init(spine_bone_world_to_local(wrappee, worldX, worldY))
    }

    @discardableResult
    public func worldToParent(worldX: Float, worldY: Float) -> Vector {
        return .init(spine_bone_world_to_parent(wrappee, worldX, worldY))
    }

    @discardableResult
    public func localToWorld(localX: Float, localY: Float) -> Vector {
        return .init(spine_bone_local_to_world(wrappee, localX, localY))
    }

    @discardableResult
    public func parentToWorld(localX: Float, localY: Float) -> Vector {
        return .init(spine_bone_parent_to_world(wrappee, localX, localY))
    }

    @discardableResult
    public func worldToLocalRotation(worldRotation: Float) -> Float {
        return spine_bone_world_to_local_rotation(wrappee, worldRotation)
    }

    @discardableResult
    public func localToWorldRotation(localRotation: Float) -> Float {
        return spine_bone_local_to_world_rotation(wrappee, localRotation)
    }

    public func rotateWorld(degrees: Float) {
        spine_bone_rotate_world(wrappee, degrees)
    }

}

@objc(SpineSlot)
@objcMembers
public final class Slot: NSObject {

    internal let wrappee: spine_slot

    internal init(_ wrappee: spine_slot) {
        self.wrappee = wrappee
        super.init()
    }

    public var data: SlotData {
        return .init(spine_slot_get_data(wrappee))
    }

    public var bone: Bone {
        return .init(spine_slot_get_bone(wrappee))
    }

    public var skeleton: Skeleton {
        return .init(spine_slot_get_skeleton(wrappee))
    }

    public var color: Color {
        return .init(spine_slot_get_color(wrappee))
    }

    public var darkColor: Color {
        return .init(spine_slot_get_dark_color(wrappee))
    }

    public var attachment: Attachment? {
        get {
            return spine_slot_get_attachment(wrappee).flatMap { .init($0) }
        }
        set {
            spine_slot_set_attachment(wrappee, newValue?.wrappee)
        }
    }

    public var sequenceIndex: Int32 {
        get {
            return spine_slot_get_sequence_index(wrappee)
        }
        set {
            spine_slot_set_sequence_index(wrappee, newValue)
        }
    }

    public func setToSetupPose() {
        spine_slot_set_to_setup_pose(wrappee)
    }

    public func setColor(r: Float, g: Float, b: Float, a: Float) {
        spine_slot_set_color(wrappee, r, g, b, a)
    }

    public func setDarkColor(r: Float, g: Float, b: Float, a: Float) {
        spine_slot_set_dark_color(wrappee, r, g, b, a)
    }

    @discardableResult
    public func hasDarkColor() -> Bool {
        return spine_slot_has_dark_color(wrappee) != 0
    }

}

@objc(SpineSkin)
@objcMembers
public final class Skin: NSObject {

    internal let wrappee: spine_skin
    internal var disposed = false

    internal init(_ wrappee: spine_skin) {
        self.wrappee = wrappee
        super.init()
    }

    public var name: String? {
        return spine_skin_get_name(wrappee).flatMap { String(cString: $0) }
    }

    public var entries: SkinEntries {
        return .init(spine_skin_get_entries(wrappee))
    }

    public var bones: [BoneData] {
        let num = Int(spine_skin_get_num_bones(wrappee))
        let ptr = spine_skin_get_bones(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public var constraints: [ConstraintData] {
        let num = Int(spine_skin_get_num_constraints(wrappee))
        let ptr = spine_skin_get_constraints(wrappee)
        return (0..<num).compactMap {
            ptr?[$0].flatMap { .init($0) }
        }
    }

    public func setAttachment(slotIndex: Int32, name: String?, attachment: Attachment) {
        spine_skin_set_attachment(wrappee, slotIndex, name, attachment.wrappee)
    }

    @discardableResult
    public func getAttachment(slotIndex: Int32, name: String?) -> Attachment? {
        return spine_skin_get_attachment(wrappee, slotIndex, name).flatMap { .init($0) }
    }

    public func removeAttachment(slotIndex: Int32, name: String?) {
        spine_skin_remove_attachment(wrappee, slotIndex, name)
    }

    public func addSkin(other: Skin) {
        spine_skin_add_skin(wrappee, other.wrappee)
    }

    public func copySkin(other: Skin) {
        spine_skin_copy_skin(wrappee, other.wrappee)
    }

    public func dispose() {
        if disposed { return }
        disposed = true
        spine_skin_dispose(wrappee)
    }

}

