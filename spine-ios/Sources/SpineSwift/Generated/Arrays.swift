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

/// ArrayFloat wrapper
@objc(SpineArrayFloat)
@objcMembers
public class ArrayFloat: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_float, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_float_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_float_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_float_size(_ptr.assumingMemoryBound(to: spine_array_float_wrapper.self)))
    }

    public subscript(index: Int) -> Float {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_float_buffer(_ptr.assumingMemoryBound(to: spine_array_float_wrapper.self))!
            return buffer[Int(index)]
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: Float) {
        spine_array_float_add(_ptr.assumingMemoryBound(to: spine_array_float_wrapper.self), value)
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_float_clear(_ptr.assumingMemoryBound(to: spine_array_float_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> Float {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_float_remove_at(_ptr.assumingMemoryBound(to: spine_array_float_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_float_set_size(_ptr.assumingMemoryBound(to: spine_array_float_wrapper.self), newValue, 0.0)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_float_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_float_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_float_dispose(_ptr.assumingMemoryBound(to: spine_array_float_wrapper.self))
        }
    }
}

/// ArrayInt wrapper
@objc(SpineArrayInt)
@objcMembers
public class ArrayInt: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_int, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_int_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_int_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_int_size(_ptr.assumingMemoryBound(to: spine_array_int_wrapper.self)))
    }

    public subscript(index: Int) -> Int32 {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_int_buffer(_ptr.assumingMemoryBound(to: spine_array_int_wrapper.self))!
            return buffer[Int(index)]
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: Int32) {
        spine_array_int_add(_ptr.assumingMemoryBound(to: spine_array_int_wrapper.self), value)
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_int_clear(_ptr.assumingMemoryBound(to: spine_array_int_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> Int32 {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_int_remove_at(_ptr.assumingMemoryBound(to: spine_array_int_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_int_set_size(_ptr.assumingMemoryBound(to: spine_array_int_wrapper.self), newValue, 0)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_int_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_int_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_int_dispose(_ptr.assumingMemoryBound(to: spine_array_int_wrapper.self))
        }
    }
}

/// ArrayUnsignedShort wrapper
@objc(SpineArrayUnsignedShort)
@objcMembers
public class ArrayUnsignedShort: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_unsigned_short, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_unsigned_short_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_unsigned_short_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_unsigned_short_size(_ptr.assumingMemoryBound(to: spine_array_unsigned_short_wrapper.self)))
    }

    public subscript(index: Int) -> UInt16 {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_unsigned_short_buffer(_ptr.assumingMemoryBound(to: spine_array_unsigned_short_wrapper.self))!
            return buffer[Int(index)]
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: UInt16) {
        spine_array_unsigned_short_add(_ptr.assumingMemoryBound(to: spine_array_unsigned_short_wrapper.self), value)
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_unsigned_short_clear(_ptr.assumingMemoryBound(to: spine_array_unsigned_short_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> UInt16 {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_unsigned_short_remove_at(_ptr.assumingMemoryBound(to: spine_array_unsigned_short_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_unsigned_short_set_size(_ptr.assumingMemoryBound(to: spine_array_unsigned_short_wrapper.self), newValue, 0)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_unsigned_short_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_unsigned_short_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_unsigned_short_dispose(_ptr.assumingMemoryBound(to: spine_array_unsigned_short_wrapper.self))
        }
    }
}

/// ArrayPropertyId wrapper
@objc(SpineArrayPropertyId)
@objcMembers
public class ArrayPropertyId: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_property_id, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_property_id_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_property_id_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_property_id_size(_ptr.assumingMemoryBound(to: spine_array_property_id_wrapper.self)))
    }

    public subscript(index: Int) -> Int64 {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_property_id_buffer(_ptr.assumingMemoryBound(to: spine_array_property_id_wrapper.self))!
            return buffer[Int(index)]
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: Int64) {
        spine_array_property_id_add(_ptr.assumingMemoryBound(to: spine_array_property_id_wrapper.self), value)
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_property_id_clear(_ptr.assumingMemoryBound(to: spine_array_property_id_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> Int64 {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_property_id_remove_at(_ptr.assumingMemoryBound(to: spine_array_property_id_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_property_id_set_size(_ptr.assumingMemoryBound(to: spine_array_property_id_wrapper.self), newValue, 0)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_property_id_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_property_id_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_property_id_dispose(_ptr.assumingMemoryBound(to: spine_array_property_id_wrapper.self))
        }
    }
}

/// ArrayAnimation wrapper
@objc(SpineArrayAnimation)
@objcMembers
public class ArrayAnimation: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_animation, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_animation_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_animation_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_animation_size(_ptr.assumingMemoryBound(to: spine_array_animation_wrapper.self)))
    }

    public subscript(index: Int) -> Animation? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_animation_buffer(_ptr.assumingMemoryBound(to: spine_array_animation_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { Animation(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: Animation?) {
        spine_array_animation_add(_ptr.assumingMemoryBound(to: spine_array_animation_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_animation_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_animation_clear(_ptr.assumingMemoryBound(to: spine_array_animation_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> Animation? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_animation_remove_at(_ptr.assumingMemoryBound(to: spine_array_animation_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_animation_set_size(_ptr.assumingMemoryBound(to: spine_array_animation_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_animation_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_animation_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_animation_dispose(_ptr.assumingMemoryBound(to: spine_array_animation_wrapper.self))
        }
    }
}

/// ArrayAtlasPage wrapper
@objc(SpineArrayAtlasPage)
@objcMembers
public class ArrayAtlasPage: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_atlas_page, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_atlas_page_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_atlas_page_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_atlas_page_size(_ptr.assumingMemoryBound(to: spine_array_atlas_page_wrapper.self)))
    }

    public subscript(index: Int) -> AtlasPage? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_atlas_page_buffer(_ptr.assumingMemoryBound(to: spine_array_atlas_page_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { AtlasPage(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: AtlasPage?) {
        spine_array_atlas_page_add(_ptr.assumingMemoryBound(to: spine_array_atlas_page_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_atlas_page_clear(_ptr.assumingMemoryBound(to: spine_array_atlas_page_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> AtlasPage? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_atlas_page_remove_at(_ptr.assumingMemoryBound(to: spine_array_atlas_page_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_atlas_page_set_size(_ptr.assumingMemoryBound(to: spine_array_atlas_page_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_atlas_page_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_atlas_page_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_atlas_page_dispose(_ptr.assumingMemoryBound(to: spine_array_atlas_page_wrapper.self))
        }
    }
}

/// ArrayAtlasRegion wrapper
@objc(SpineArrayAtlasRegion)
@objcMembers
public class ArrayAtlasRegion: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_atlas_region, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_atlas_region_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_atlas_region_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_atlas_region_size(_ptr.assumingMemoryBound(to: spine_array_atlas_region_wrapper.self)))
    }

    public subscript(index: Int) -> AtlasRegion? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_atlas_region_buffer(_ptr.assumingMemoryBound(to: spine_array_atlas_region_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { AtlasRegion(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: AtlasRegion?) {
        spine_array_atlas_region_add(_ptr.assumingMemoryBound(to: spine_array_atlas_region_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_atlas_region_clear(_ptr.assumingMemoryBound(to: spine_array_atlas_region_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> AtlasRegion? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_atlas_region_remove_at(_ptr.assumingMemoryBound(to: spine_array_atlas_region_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_atlas_region_set_size(_ptr.assumingMemoryBound(to: spine_array_atlas_region_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_atlas_region_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_atlas_region_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_atlas_region_dispose(_ptr.assumingMemoryBound(to: spine_array_atlas_region_wrapper.self))
        }
    }
}

/// ArrayAttachment wrapper
@objc(SpineArrayAttachment)
@objcMembers
public class ArrayAttachment: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_attachment, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_attachment_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_attachment_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_attachment_size(_ptr.assumingMemoryBound(to: spine_array_attachment_wrapper.self)))
    }

    public subscript(index: Int) -> Attachment? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_attachment_buffer(_ptr.assumingMemoryBound(to: spine_array_attachment_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            guard let ptr = elementPtr else { return nil }
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
    }

    /// Adds a value to the end of this array
    public func add(_ value: Attachment?) {
        spine_array_attachment_add(_ptr.assumingMemoryBound(to: spine_array_attachment_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_attachment_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_attachment_clear(_ptr.assumingMemoryBound(to: spine_array_attachment_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> Attachment? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_attachment_remove_at(_ptr.assumingMemoryBound(to: spine_array_attachment_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_attachment_set_size(_ptr.assumingMemoryBound(to: spine_array_attachment_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_attachment_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_attachment_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_attachment_dispose(_ptr.assumingMemoryBound(to: spine_array_attachment_wrapper.self))
        }
    }
}

/// ArrayBone wrapper
@objc(SpineArrayBone)
@objcMembers
public class ArrayBone: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_bone, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_bone_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_bone_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_bone_size(_ptr.assumingMemoryBound(to: spine_array_bone_wrapper.self)))
    }

    public subscript(index: Int) -> Bone? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_bone_buffer(_ptr.assumingMemoryBound(to: spine_array_bone_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { Bone(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: Bone?) {
        spine_array_bone_add(_ptr.assumingMemoryBound(to: spine_array_bone_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_bone_clear(_ptr.assumingMemoryBound(to: spine_array_bone_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> Bone? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_bone_remove_at(_ptr.assumingMemoryBound(to: spine_array_bone_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_bone_set_size(_ptr.assumingMemoryBound(to: spine_array_bone_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_bone_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_bone_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_bone_dispose(_ptr.assumingMemoryBound(to: spine_array_bone_wrapper.self))
        }
    }
}

/// ArrayBoneData wrapper
@objc(SpineArrayBoneData)
@objcMembers
public class ArrayBoneData: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_bone_data, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_bone_data_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_bone_data_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_bone_data_size(_ptr.assumingMemoryBound(to: spine_array_bone_data_wrapper.self)))
    }

    public subscript(index: Int) -> BoneData? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_bone_data_buffer(_ptr.assumingMemoryBound(to: spine_array_bone_data_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { BoneData(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: BoneData?) {
        spine_array_bone_data_add(_ptr.assumingMemoryBound(to: spine_array_bone_data_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_bone_data_clear(_ptr.assumingMemoryBound(to: spine_array_bone_data_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> BoneData? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_bone_data_remove_at(_ptr.assumingMemoryBound(to: spine_array_bone_data_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_bone_data_set_size(_ptr.assumingMemoryBound(to: spine_array_bone_data_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_bone_data_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_bone_data_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_bone_data_dispose(_ptr.assumingMemoryBound(to: spine_array_bone_data_wrapper.self))
        }
    }
}

/// ArrayBonePose wrapper
@objc(SpineArrayBonePose)
@objcMembers
public class ArrayBonePose: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_bone_pose, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_bone_pose_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_bone_pose_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_bone_pose_size(_ptr.assumingMemoryBound(to: spine_array_bone_pose_wrapper.self)))
    }

    public subscript(index: Int) -> BonePose? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_bone_pose_buffer(_ptr.assumingMemoryBound(to: spine_array_bone_pose_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { BonePose(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: BonePose?) {
        spine_array_bone_pose_add(_ptr.assumingMemoryBound(to: spine_array_bone_pose_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_bone_pose_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_bone_pose_clear(_ptr.assumingMemoryBound(to: spine_array_bone_pose_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> BonePose? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_bone_pose_remove_at(_ptr.assumingMemoryBound(to: spine_array_bone_pose_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_bone_pose_set_size(_ptr.assumingMemoryBound(to: spine_array_bone_pose_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_bone_pose_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_bone_pose_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_bone_pose_dispose(_ptr.assumingMemoryBound(to: spine_array_bone_pose_wrapper.self))
        }
    }
}

/// ArrayBoundingBoxAttachment wrapper
@objc(SpineArrayBoundingBoxAttachment)
@objcMembers
public class ArrayBoundingBoxAttachment: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_bounding_box_attachment, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_bounding_box_attachment_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_bounding_box_attachment_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_bounding_box_attachment_size(_ptr.assumingMemoryBound(to: spine_array_bounding_box_attachment_wrapper.self)))
    }

    public subscript(index: Int) -> BoundingBoxAttachment? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_bounding_box_attachment_buffer(_ptr.assumingMemoryBound(to: spine_array_bounding_box_attachment_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { BoundingBoxAttachment(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: BoundingBoxAttachment?) {
        spine_array_bounding_box_attachment_add(_ptr.assumingMemoryBound(to: spine_array_bounding_box_attachment_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_bounding_box_attachment_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_bounding_box_attachment_clear(_ptr.assumingMemoryBound(to: spine_array_bounding_box_attachment_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> BoundingBoxAttachment? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_bounding_box_attachment_remove_at(_ptr.assumingMemoryBound(to: spine_array_bounding_box_attachment_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_bounding_box_attachment_set_size(_ptr.assumingMemoryBound(to: spine_array_bounding_box_attachment_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_bounding_box_attachment_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_bounding_box_attachment_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_bounding_box_attachment_dispose(_ptr.assumingMemoryBound(to: spine_array_bounding_box_attachment_wrapper.self))
        }
    }
}

/// ArrayConstraint wrapper
@objc(SpineArrayConstraint)
@objcMembers
public class ArrayConstraint: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_constraint, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_constraint_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_constraint_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_constraint_size(_ptr.assumingMemoryBound(to: spine_array_constraint_wrapper.self)))
    }

    public subscript(index: Int) -> Constraint? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_constraint_buffer(_ptr.assumingMemoryBound(to: spine_array_constraint_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            guard let ptr = elementPtr else { return nil }
            let rtti = spine_constraint_get_rtti(ptr)
        let rttiClassName = String(cString: spine_rtti_get_class_name(rtti)!)
        switch rttiClassName {
        case "IkConstraint":
            let castedPtr = spine_constraint_cast_to_ik_constraint(ptr)
            return IkConstraint(fromPointer: castedPtr!)
        case "PathConstraint":
            let castedPtr = spine_constraint_cast_to_path_constraint(ptr)
            return PathConstraint(fromPointer: castedPtr!)
        case "PhysicsConstraint":
            let castedPtr = spine_constraint_cast_to_physics_constraint(ptr)
            return PhysicsConstraint(fromPointer: castedPtr!)
        case "Slider":
            let castedPtr = spine_constraint_cast_to_slider(ptr)
            return Slider(fromPointer: castedPtr!)
        case "TransformConstraint":
            let castedPtr = spine_constraint_cast_to_transform_constraint(ptr)
            return TransformConstraint(fromPointer: castedPtr!)
        default:
            fatalError("Unknown concrete type: \(rttiClassName) for abstract class Constraint")
        }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: Constraint?) {
        spine_array_constraint_add(_ptr.assumingMemoryBound(to: spine_array_constraint_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_constraint_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_constraint_clear(_ptr.assumingMemoryBound(to: spine_array_constraint_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> Constraint? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_constraint_remove_at(_ptr.assumingMemoryBound(to: spine_array_constraint_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_constraint_set_size(_ptr.assumingMemoryBound(to: spine_array_constraint_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_constraint_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_constraint_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_constraint_dispose(_ptr.assumingMemoryBound(to: spine_array_constraint_wrapper.self))
        }
    }
}

/// ArrayConstraintData wrapper
@objc(SpineArrayConstraintData)
@objcMembers
public class ArrayConstraintData: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_constraint_data, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_constraint_data_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_constraint_data_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_constraint_data_size(_ptr.assumingMemoryBound(to: spine_array_constraint_data_wrapper.self)))
    }

    public subscript(index: Int) -> ConstraintData? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_constraint_data_buffer(_ptr.assumingMemoryBound(to: spine_array_constraint_data_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            guard let ptr = elementPtr else { return nil }
            let rtti = spine_constraint_data_get_rtti(ptr)
        let rttiClassName = String(cString: spine_rtti_get_class_name(rtti)!)
        switch rttiClassName {
        case "IkConstraintData":
            let castedPtr = spine_constraint_data_cast_to_ik_constraint_data(ptr)
            return IkConstraintData(fromPointer: castedPtr!)
        case "PathConstraintData":
            let castedPtr = spine_constraint_data_cast_to_path_constraint_data(ptr)
            return PathConstraintData(fromPointer: castedPtr!)
        case "PhysicsConstraintData":
            let castedPtr = spine_constraint_data_cast_to_physics_constraint_data(ptr)
            return PhysicsConstraintData(fromPointer: castedPtr!)
        case "SliderData":
            let castedPtr = spine_constraint_data_cast_to_slider_data(ptr)
            return SliderData(fromPointer: castedPtr!)
        case "TransformConstraintData":
            let castedPtr = spine_constraint_data_cast_to_transform_constraint_data(ptr)
            return TransformConstraintData(fromPointer: castedPtr!)
        default:
            fatalError("Unknown concrete type: \(rttiClassName) for abstract class ConstraintData")
        }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: ConstraintData?) {
        spine_array_constraint_data_add(_ptr.assumingMemoryBound(to: spine_array_constraint_data_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_constraint_data_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_constraint_data_clear(_ptr.assumingMemoryBound(to: spine_array_constraint_data_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> ConstraintData? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_constraint_data_remove_at(_ptr.assumingMemoryBound(to: spine_array_constraint_data_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_constraint_data_set_size(_ptr.assumingMemoryBound(to: spine_array_constraint_data_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_constraint_data_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_constraint_data_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_constraint_data_dispose(_ptr.assumingMemoryBound(to: spine_array_constraint_data_wrapper.self))
        }
    }
}

/// ArrayEvent wrapper
@objc(SpineArrayEvent)
@objcMembers
public class ArrayEvent: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_event, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_event_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_event_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_event_size(_ptr.assumingMemoryBound(to: spine_array_event_wrapper.self)))
    }

    public subscript(index: Int) -> Event? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_event_buffer(_ptr.assumingMemoryBound(to: spine_array_event_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { Event(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: Event?) {
        spine_array_event_add(_ptr.assumingMemoryBound(to: spine_array_event_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_event_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_event_clear(_ptr.assumingMemoryBound(to: spine_array_event_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> Event? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_event_remove_at(_ptr.assumingMemoryBound(to: spine_array_event_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_event_set_size(_ptr.assumingMemoryBound(to: spine_array_event_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_event_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_event_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_event_dispose(_ptr.assumingMemoryBound(to: spine_array_event_wrapper.self))
        }
    }
}

/// ArrayEventData wrapper
@objc(SpineArrayEventData)
@objcMembers
public class ArrayEventData: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_event_data, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_event_data_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_event_data_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_event_data_size(_ptr.assumingMemoryBound(to: spine_array_event_data_wrapper.self)))
    }

    public subscript(index: Int) -> EventData? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_event_data_buffer(_ptr.assumingMemoryBound(to: spine_array_event_data_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { EventData(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: EventData?) {
        spine_array_event_data_add(_ptr.assumingMemoryBound(to: spine_array_event_data_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_event_data_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_event_data_clear(_ptr.assumingMemoryBound(to: spine_array_event_data_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> EventData? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_event_data_remove_at(_ptr.assumingMemoryBound(to: spine_array_event_data_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_event_data_set_size(_ptr.assumingMemoryBound(to: spine_array_event_data_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_event_data_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_event_data_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_event_data_dispose(_ptr.assumingMemoryBound(to: spine_array_event_data_wrapper.self))
        }
    }
}

/// ArrayFromProperty wrapper
@objc(SpineArrayFromProperty)
@objcMembers
public class ArrayFromProperty: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_from_property, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_from_property_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_from_property_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_from_property_size(_ptr.assumingMemoryBound(to: spine_array_from_property_wrapper.self)))
    }

    public subscript(index: Int) -> FromProperty? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_from_property_buffer(_ptr.assumingMemoryBound(to: spine_array_from_property_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            guard let ptr = elementPtr else { return nil }
            let rtti = spine_from_property_get_rtti(ptr)
        let rttiClassName = String(cString: spine_rtti_get_class_name(rtti)!)
        switch rttiClassName {
        case "FromRotate":
            let castedPtr = spine_from_property_cast_to_from_rotate(ptr)
            return FromRotate(fromPointer: castedPtr!)
        case "FromScaleX":
            let castedPtr = spine_from_property_cast_to_from_scale_x(ptr)
            return FromScaleX(fromPointer: castedPtr!)
        case "FromScaleY":
            let castedPtr = spine_from_property_cast_to_from_scale_y(ptr)
            return FromScaleY(fromPointer: castedPtr!)
        case "FromShearY":
            let castedPtr = spine_from_property_cast_to_from_shear_y(ptr)
            return FromShearY(fromPointer: castedPtr!)
        case "FromX":
            let castedPtr = spine_from_property_cast_to_from_x(ptr)
            return FromX(fromPointer: castedPtr!)
        case "FromY":
            let castedPtr = spine_from_property_cast_to_from_y(ptr)
            return FromY(fromPointer: castedPtr!)
        default:
            fatalError("Unknown concrete type: \(rttiClassName) for abstract class FromProperty")
        }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: FromProperty?) {
        spine_array_from_property_add(_ptr.assumingMemoryBound(to: spine_array_from_property_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_from_property_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_from_property_clear(_ptr.assumingMemoryBound(to: spine_array_from_property_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> FromProperty? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_from_property_remove_at(_ptr.assumingMemoryBound(to: spine_array_from_property_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_from_property_set_size(_ptr.assumingMemoryBound(to: spine_array_from_property_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_from_property_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_from_property_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_from_property_dispose(_ptr.assumingMemoryBound(to: spine_array_from_property_wrapper.self))
        }
    }
}

/// ArrayPhysicsConstraint wrapper
@objc(SpineArrayPhysicsConstraint)
@objcMembers
public class ArrayPhysicsConstraint: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_physics_constraint, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_physics_constraint_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_physics_constraint_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_physics_constraint_size(_ptr.assumingMemoryBound(to: spine_array_physics_constraint_wrapper.self)))
    }

    public subscript(index: Int) -> PhysicsConstraint? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_physics_constraint_buffer(_ptr.assumingMemoryBound(to: spine_array_physics_constraint_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { PhysicsConstraint(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: PhysicsConstraint?) {
        spine_array_physics_constraint_add(_ptr.assumingMemoryBound(to: spine_array_physics_constraint_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_physics_constraint_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_physics_constraint_clear(_ptr.assumingMemoryBound(to: spine_array_physics_constraint_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> PhysicsConstraint? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_physics_constraint_remove_at(_ptr.assumingMemoryBound(to: spine_array_physics_constraint_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_physics_constraint_set_size(_ptr.assumingMemoryBound(to: spine_array_physics_constraint_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_physics_constraint_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_physics_constraint_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_physics_constraint_dispose(_ptr.assumingMemoryBound(to: spine_array_physics_constraint_wrapper.self))
        }
    }
}

/// ArrayPolygon wrapper
@objc(SpineArrayPolygon)
@objcMembers
public class ArrayPolygon: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_polygon, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_polygon_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_polygon_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_polygon_size(_ptr.assumingMemoryBound(to: spine_array_polygon_wrapper.self)))
    }

    public subscript(index: Int) -> Polygon? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_polygon_buffer(_ptr.assumingMemoryBound(to: spine_array_polygon_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { Polygon(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: Polygon?) {
        spine_array_polygon_add(_ptr.assumingMemoryBound(to: spine_array_polygon_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_polygon_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_polygon_clear(_ptr.assumingMemoryBound(to: spine_array_polygon_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> Polygon? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_polygon_remove_at(_ptr.assumingMemoryBound(to: spine_array_polygon_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_polygon_set_size(_ptr.assumingMemoryBound(to: spine_array_polygon_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_polygon_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_polygon_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_polygon_dispose(_ptr.assumingMemoryBound(to: spine_array_polygon_wrapper.self))
        }
    }
}

/// ArraySkin wrapper
@objc(SpineArraySkin)
@objcMembers
public class ArraySkin: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_skin, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_skin_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_skin_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_skin_size(_ptr.assumingMemoryBound(to: spine_array_skin_wrapper.self)))
    }

    public subscript(index: Int) -> Skin? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_skin_buffer(_ptr.assumingMemoryBound(to: spine_array_skin_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { Skin(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: Skin?) {
        spine_array_skin_add(_ptr.assumingMemoryBound(to: spine_array_skin_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_skin_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_skin_clear(_ptr.assumingMemoryBound(to: spine_array_skin_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> Skin? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_skin_remove_at(_ptr.assumingMemoryBound(to: spine_array_skin_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_skin_set_size(_ptr.assumingMemoryBound(to: spine_array_skin_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_skin_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_skin_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_skin_dispose(_ptr.assumingMemoryBound(to: spine_array_skin_wrapper.self))
        }
    }
}

/// ArraySlot wrapper
@objc(SpineArraySlot)
@objcMembers
public class ArraySlot: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_slot, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_slot_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_slot_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_slot_size(_ptr.assumingMemoryBound(to: spine_array_slot_wrapper.self)))
    }

    public subscript(index: Int) -> Slot? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_slot_buffer(_ptr.assumingMemoryBound(to: spine_array_slot_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { Slot(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: Slot?) {
        spine_array_slot_add(_ptr.assumingMemoryBound(to: spine_array_slot_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_slot_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_slot_clear(_ptr.assumingMemoryBound(to: spine_array_slot_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> Slot? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_slot_remove_at(_ptr.assumingMemoryBound(to: spine_array_slot_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_slot_set_size(_ptr.assumingMemoryBound(to: spine_array_slot_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_slot_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_slot_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_slot_dispose(_ptr.assumingMemoryBound(to: spine_array_slot_wrapper.self))
        }
    }
}

/// ArraySlotData wrapper
@objc(SpineArraySlotData)
@objcMembers
public class ArraySlotData: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_slot_data, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_slot_data_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_slot_data_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_slot_data_size(_ptr.assumingMemoryBound(to: spine_array_slot_data_wrapper.self)))
    }

    public subscript(index: Int) -> SlotData? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_slot_data_buffer(_ptr.assumingMemoryBound(to: spine_array_slot_data_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { SlotData(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: SlotData?) {
        spine_array_slot_data_add(_ptr.assumingMemoryBound(to: spine_array_slot_data_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_slot_data_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_slot_data_clear(_ptr.assumingMemoryBound(to: spine_array_slot_data_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> SlotData? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_slot_data_remove_at(_ptr.assumingMemoryBound(to: spine_array_slot_data_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_slot_data_set_size(_ptr.assumingMemoryBound(to: spine_array_slot_data_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_slot_data_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_slot_data_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_slot_data_dispose(_ptr.assumingMemoryBound(to: spine_array_slot_data_wrapper.self))
        }
    }
}

/// ArrayTextureRegion wrapper
@objc(SpineArrayTextureRegion)
@objcMembers
public class ArrayTextureRegion: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_texture_region, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_texture_region_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_texture_region_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_texture_region_size(_ptr.assumingMemoryBound(to: spine_array_texture_region_wrapper.self)))
    }

    public subscript(index: Int) -> TextureRegion? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_texture_region_buffer(_ptr.assumingMemoryBound(to: spine_array_texture_region_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { TextureRegion(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: TextureRegion?) {
        spine_array_texture_region_add(_ptr.assumingMemoryBound(to: spine_array_texture_region_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_texture_region_clear(_ptr.assumingMemoryBound(to: spine_array_texture_region_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> TextureRegion? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_texture_region_remove_at(_ptr.assumingMemoryBound(to: spine_array_texture_region_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_texture_region_set_size(_ptr.assumingMemoryBound(to: spine_array_texture_region_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_texture_region_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_texture_region_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_texture_region_dispose(_ptr.assumingMemoryBound(to: spine_array_texture_region_wrapper.self))
        }
    }
}

/// ArrayTimeline wrapper
@objc(SpineArrayTimeline)
@objcMembers
public class ArrayTimeline: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_timeline, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_timeline_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_timeline_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_timeline_size(_ptr.assumingMemoryBound(to: spine_array_timeline_wrapper.self)))
    }

    public subscript(index: Int) -> Timeline? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_timeline_buffer(_ptr.assumingMemoryBound(to: spine_array_timeline_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            guard let ptr = elementPtr else { return nil }
            let rtti = spine_timeline_get_rtti(ptr)
        let rttiClassName = String(cString: spine_rtti_get_class_name(rtti)!)
        switch rttiClassName {
        case "AlphaTimeline":
            let castedPtr = spine_timeline_cast_to_alpha_timeline(ptr)
            return AlphaTimeline(fromPointer: castedPtr!)
        case "AttachmentTimeline":
            let castedPtr = spine_timeline_cast_to_attachment_timeline(ptr)
            return AttachmentTimeline(fromPointer: castedPtr!)
        case "DeformTimeline":
            let castedPtr = spine_timeline_cast_to_deform_timeline(ptr)
            return DeformTimeline(fromPointer: castedPtr!)
        case "DrawOrderFolderTimeline":
            let castedPtr = spine_timeline_cast_to_draw_order_folder_timeline(ptr)
            return DrawOrderFolderTimeline(fromPointer: castedPtr!)
        case "DrawOrderTimeline":
            let castedPtr = spine_timeline_cast_to_draw_order_timeline(ptr)
            return DrawOrderTimeline(fromPointer: castedPtr!)
        case "EventTimeline":
            let castedPtr = spine_timeline_cast_to_event_timeline(ptr)
            return EventTimeline(fromPointer: castedPtr!)
        case "IkConstraintTimeline":
            let castedPtr = spine_timeline_cast_to_ik_constraint_timeline(ptr)
            return IkConstraintTimeline(fromPointer: castedPtr!)
        case "InheritTimeline":
            let castedPtr = spine_timeline_cast_to_inherit_timeline(ptr)
            return InheritTimeline(fromPointer: castedPtr!)
        case "PathConstraintMixTimeline":
            let castedPtr = spine_timeline_cast_to_path_constraint_mix_timeline(ptr)
            return PathConstraintMixTimeline(fromPointer: castedPtr!)
        case "PathConstraintPositionTimeline":
            let castedPtr = spine_timeline_cast_to_path_constraint_position_timeline(ptr)
            return PathConstraintPositionTimeline(fromPointer: castedPtr!)
        case "PathConstraintSpacingTimeline":
            let castedPtr = spine_timeline_cast_to_path_constraint_spacing_timeline(ptr)
            return PathConstraintSpacingTimeline(fromPointer: castedPtr!)
        case "PhysicsConstraintDampingTimeline":
            let castedPtr = spine_timeline_cast_to_physics_constraint_damping_timeline(ptr)
            return PhysicsConstraintDampingTimeline(fromPointer: castedPtr!)
        case "PhysicsConstraintGravityTimeline":
            let castedPtr = spine_timeline_cast_to_physics_constraint_gravity_timeline(ptr)
            return PhysicsConstraintGravityTimeline(fromPointer: castedPtr!)
        case "PhysicsConstraintInertiaTimeline":
            let castedPtr = spine_timeline_cast_to_physics_constraint_inertia_timeline(ptr)
            return PhysicsConstraintInertiaTimeline(fromPointer: castedPtr!)
        case "PhysicsConstraintMassTimeline":
            let castedPtr = spine_timeline_cast_to_physics_constraint_mass_timeline(ptr)
            return PhysicsConstraintMassTimeline(fromPointer: castedPtr!)
        case "PhysicsConstraintMixTimeline":
            let castedPtr = spine_timeline_cast_to_physics_constraint_mix_timeline(ptr)
            return PhysicsConstraintMixTimeline(fromPointer: castedPtr!)
        case "PhysicsConstraintResetTimeline":
            let castedPtr = spine_timeline_cast_to_physics_constraint_reset_timeline(ptr)
            return PhysicsConstraintResetTimeline(fromPointer: castedPtr!)
        case "PhysicsConstraintStrengthTimeline":
            let castedPtr = spine_timeline_cast_to_physics_constraint_strength_timeline(ptr)
            return PhysicsConstraintStrengthTimeline(fromPointer: castedPtr!)
        case "PhysicsConstraintWindTimeline":
            let castedPtr = spine_timeline_cast_to_physics_constraint_wind_timeline(ptr)
            return PhysicsConstraintWindTimeline(fromPointer: castedPtr!)
        case "Rgb2Timeline":
            let castedPtr = spine_timeline_cast_to_rgb2_timeline(ptr)
            return Rgb2Timeline(fromPointer: castedPtr!)
        case "Rgba2Timeline":
            let castedPtr = spine_timeline_cast_to_rgba2_timeline(ptr)
            return Rgba2Timeline(fromPointer: castedPtr!)
        case "RgbaTimeline":
            let castedPtr = spine_timeline_cast_to_rgba_timeline(ptr)
            return RgbaTimeline(fromPointer: castedPtr!)
        case "RgbTimeline":
            let castedPtr = spine_timeline_cast_to_rgb_timeline(ptr)
            return RgbTimeline(fromPointer: castedPtr!)
        case "RotateTimeline":
            let castedPtr = spine_timeline_cast_to_rotate_timeline(ptr)
            return RotateTimeline(fromPointer: castedPtr!)
        case "ScaleTimeline":
            let castedPtr = spine_timeline_cast_to_scale_timeline(ptr)
            return ScaleTimeline(fromPointer: castedPtr!)
        case "ScaleXTimeline":
            let castedPtr = spine_timeline_cast_to_scale_x_timeline(ptr)
            return ScaleXTimeline(fromPointer: castedPtr!)
        case "ScaleYTimeline":
            let castedPtr = spine_timeline_cast_to_scale_y_timeline(ptr)
            return ScaleYTimeline(fromPointer: castedPtr!)
        case "SequenceTimeline":
            let castedPtr = spine_timeline_cast_to_sequence_timeline(ptr)
            return SequenceTimeline(fromPointer: castedPtr!)
        case "ShearTimeline":
            let castedPtr = spine_timeline_cast_to_shear_timeline(ptr)
            return ShearTimeline(fromPointer: castedPtr!)
        case "ShearXTimeline":
            let castedPtr = spine_timeline_cast_to_shear_x_timeline(ptr)
            return ShearXTimeline(fromPointer: castedPtr!)
        case "ShearYTimeline":
            let castedPtr = spine_timeline_cast_to_shear_y_timeline(ptr)
            return ShearYTimeline(fromPointer: castedPtr!)
        case "SliderMixTimeline":
            let castedPtr = spine_timeline_cast_to_slider_mix_timeline(ptr)
            return SliderMixTimeline(fromPointer: castedPtr!)
        case "SliderTimeline":
            let castedPtr = spine_timeline_cast_to_slider_timeline(ptr)
            return SliderTimeline(fromPointer: castedPtr!)
        case "TransformConstraintTimeline":
            let castedPtr = spine_timeline_cast_to_transform_constraint_timeline(ptr)
            return TransformConstraintTimeline(fromPointer: castedPtr!)
        case "TranslateTimeline":
            let castedPtr = spine_timeline_cast_to_translate_timeline(ptr)
            return TranslateTimeline(fromPointer: castedPtr!)
        case "TranslateXTimeline":
            let castedPtr = spine_timeline_cast_to_translate_x_timeline(ptr)
            return TranslateXTimeline(fromPointer: castedPtr!)
        case "TranslateYTimeline":
            let castedPtr = spine_timeline_cast_to_translate_y_timeline(ptr)
            return TranslateYTimeline(fromPointer: castedPtr!)
        default:
            fatalError("Unknown concrete type: \(rttiClassName) for abstract class Timeline")
        }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: Timeline?) {
        spine_array_timeline_add(_ptr.assumingMemoryBound(to: spine_array_timeline_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_timeline_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_timeline_clear(_ptr.assumingMemoryBound(to: spine_array_timeline_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> Timeline? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_timeline_remove_at(_ptr.assumingMemoryBound(to: spine_array_timeline_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_timeline_set_size(_ptr.assumingMemoryBound(to: spine_array_timeline_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_timeline_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_timeline_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_timeline_dispose(_ptr.assumingMemoryBound(to: spine_array_timeline_wrapper.self))
        }
    }
}

/// ArrayToProperty wrapper
@objc(SpineArrayToProperty)
@objcMembers
public class ArrayToProperty: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_to_property, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_to_property_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_to_property_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_to_property_size(_ptr.assumingMemoryBound(to: spine_array_to_property_wrapper.self)))
    }

    public subscript(index: Int) -> ToProperty? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_to_property_buffer(_ptr.assumingMemoryBound(to: spine_array_to_property_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            guard let ptr = elementPtr else { return nil }
            let rtti = spine_to_property_get_rtti(ptr)
        let rttiClassName = String(cString: spine_rtti_get_class_name(rtti)!)
        switch rttiClassName {
        case "ToRotate":
            let castedPtr = spine_to_property_cast_to_to_rotate(ptr)
            return ToRotate(fromPointer: castedPtr!)
        case "ToScaleX":
            let castedPtr = spine_to_property_cast_to_to_scale_x(ptr)
            return ToScaleX(fromPointer: castedPtr!)
        case "ToScaleY":
            let castedPtr = spine_to_property_cast_to_to_scale_y(ptr)
            return ToScaleY(fromPointer: castedPtr!)
        case "ToShearY":
            let castedPtr = spine_to_property_cast_to_to_shear_y(ptr)
            return ToShearY(fromPointer: castedPtr!)
        case "ToX":
            let castedPtr = spine_to_property_cast_to_to_x(ptr)
            return ToX(fromPointer: castedPtr!)
        case "ToY":
            let castedPtr = spine_to_property_cast_to_to_y(ptr)
            return ToY(fromPointer: castedPtr!)
        default:
            fatalError("Unknown concrete type: \(rttiClassName) for abstract class ToProperty")
        }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: ToProperty?) {
        spine_array_to_property_add(_ptr.assumingMemoryBound(to: spine_array_to_property_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_to_property_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_to_property_clear(_ptr.assumingMemoryBound(to: spine_array_to_property_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> ToProperty? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_to_property_remove_at(_ptr.assumingMemoryBound(to: spine_array_to_property_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_to_property_set_size(_ptr.assumingMemoryBound(to: spine_array_to_property_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_to_property_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_to_property_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_to_property_dispose(_ptr.assumingMemoryBound(to: spine_array_to_property_wrapper.self))
        }
    }
}

/// ArrayTrackEntry wrapper
@objc(SpineArrayTrackEntry)
@objcMembers
public class ArrayTrackEntry: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_track_entry, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_track_entry_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_track_entry_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_track_entry_size(_ptr.assumingMemoryBound(to: spine_array_track_entry_wrapper.self)))
    }

    public subscript(index: Int) -> TrackEntry? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_track_entry_buffer(_ptr.assumingMemoryBound(to: spine_array_track_entry_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            return elementPtr.map { TrackEntry(fromPointer: $0) }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: TrackEntry?) {
        spine_array_track_entry_add(_ptr.assumingMemoryBound(to: spine_array_track_entry_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_track_entry_clear(_ptr.assumingMemoryBound(to: spine_array_track_entry_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> TrackEntry? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_track_entry_remove_at(_ptr.assumingMemoryBound(to: spine_array_track_entry_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_track_entry_set_size(_ptr.assumingMemoryBound(to: spine_array_track_entry_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_track_entry_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_track_entry_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_track_entry_dispose(_ptr.assumingMemoryBound(to: spine_array_track_entry_wrapper.self))
        }
    }
}

/// ArrayUpdate wrapper
@objc(SpineArrayUpdate)
@objcMembers
public class ArrayUpdate: NSObject {
    public let _ptr: UnsafeMutableRawPointer
    private let _ownsMemory: Bool

    public init(fromPointer ptr: spine_array_update, ownsMemory: Bool = false) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        self._ownsMemory = ownsMemory
        super.init()
    }


    /// Create a new empty array
    public override convenience init() {
        let ptr = spine_array_update_create()!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    /// Create a new array with the specified initial capacity
    public convenience init(capacity: Int) {
        let ptr = spine_array_update_create_with_capacity(capacity)!
        self.init(fromPointer: ptr, ownsMemory: true)
    }

    public var count: Int {
        return Int(spine_array_update_size(_ptr.assumingMemoryBound(to: spine_array_update_wrapper.self)))
    }

    public subscript(index: Int) -> Update? {
        get {
            precondition(index >= 0 && index < count, "Index out of bounds")
            let buffer = spine_array_update_buffer(_ptr.assumingMemoryBound(to: spine_array_update_wrapper.self))!
            let elementPtr = buffer[Int(index)]
            guard let ptr = elementPtr else { return nil }
            let rtti = spine_update_get_rtti(ptr)
        let rttiClassName = String(cString: spine_rtti_get_class_name(rtti)!)
        switch rttiClassName {
        case "Bone":
            let castedPtr = spine_update_cast_to_bone(ptr)
            return Bone(fromPointer: castedPtr!)
        case "BonePose":
            let castedPtr = spine_update_cast_to_bone_pose(ptr)
            return BonePose(fromPointer: castedPtr!)
        case "IkConstraint":
            let castedPtr = spine_update_cast_to_ik_constraint(ptr)
            return IkConstraint(fromPointer: castedPtr!)
        case "PathConstraint":
            let castedPtr = spine_update_cast_to_path_constraint(ptr)
            return PathConstraint(fromPointer: castedPtr!)
        case "PhysicsConstraint":
            let castedPtr = spine_update_cast_to_physics_constraint(ptr)
            return PhysicsConstraint(fromPointer: castedPtr!)
        case "Slider":
            let castedPtr = spine_update_cast_to_slider(ptr)
            return Slider(fromPointer: castedPtr!)
        case "TransformConstraint":
            let castedPtr = spine_update_cast_to_transform_constraint(ptr)
            return TransformConstraint(fromPointer: castedPtr!)
        default:
            fatalError("Unknown concrete type: \(rttiClassName) for abstract class Update")
        }
        }
    }

    /// Adds a value to the end of this array
    public func add(_ value: Update?) {
        spine_array_update_add(_ptr.assumingMemoryBound(to: spine_array_update_wrapper.self), value?._ptr.assumingMemoryBound(to: spine_update_wrapper.self))
    }

    /// Removes all elements from this array
    public func clear() {
        spine_array_update_clear(_ptr.assumingMemoryBound(to: spine_array_update_wrapper.self))
    }

    /// Removes the element at the given index
    @discardableResult
    public func removeAt(_ index: Int) -> Update? {
        precondition(index >= 0 && index < count, "Index out of bounds")
        let value = self[index]
        spine_array_update_remove_at(_ptr.assumingMemoryBound(to: spine_array_update_wrapper.self), index)
        return value
    }

    /// Sets the size of this array
    public var length: Int {
        get { count }
        set {
            spine_array_update_set_size(_ptr.assumingMemoryBound(to: spine_array_update_wrapper.self), newValue, nil)
        }
    }

    /// Ensures this array has at least the given capacity
    public func ensureCapacity(_ capacity: Int) {
        spine_array_update_ensure_capacity(_ptr.assumingMemoryBound(to: spine_array_update_wrapper.self), capacity)
    }

    deinit {
        if _ownsMemory {
            spine_array_update_dispose(_ptr.assumingMemoryBound(to: spine_array_update_wrapper.self))
        }
    }
}