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

/// Stores the setup pose and all of the stateless data for a skeleton.
///
/// See Data objects in the Spine Runtimes Guide.
@objc(SpineSkeletonData)
@objcMembers
public class SkeletonData: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_skeleton_data) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public override convenience init() {
        let ptr = spine_skeleton_data_create()
        self.init(fromPointer: ptr!)
    }

    /// The skeleton's name, which by default is the name of the skeleton data file when possible,
    /// or null when a name hasn't been set.
    public var name: String {
        get {
            let result = spine_skeleton_data_get_name(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return String(cString: result!)
        }
        set {
            spine_skeleton_data_set_name(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), newValue)
        }
    }

    /// The skeleton's bones, sorted parent first. The root bone is always the first bone.
    public var bones: ArrayBoneData {
        let result = spine_skeleton_data_get_bones(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return ArrayBoneData(fromPointer: result!)
    }

    /// The skeleton's slots in the setup pose draw order.
    public var slots: ArraySlotData {
        let result = spine_skeleton_data_get_slots(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return ArraySlotData(fromPointer: result!)
    }

    /// All skins, including the default skin.
    public var skins: ArraySkin {
        let result = spine_skeleton_data_get_skins(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return ArraySkin(fromPointer: result!)
    }

    /// The skeleton's default skin. By default this skin contains all attachments that were not in
    /// a skin in Spine.
    ///
    /// - Returns: May be NULL.
    public var defaultSkin: Skin? {
        get {
            let result = spine_skeleton_data_get_default_skin(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return result.map { Skin(fromPointer: $0) }
        }
        set {
            spine_skeleton_data_set_default_skin(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), newValue?._ptr.assumingMemoryBound(to: spine_skin_wrapper.self))
        }
    }

    /// The skeleton's events.
    public var events: ArrayEventData {
        let result = spine_skeleton_data_get_events(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return ArrayEventData(fromPointer: result!)
    }

    /// The skeleton's animations.
    public var animations: ArrayAnimation {
        let result = spine_skeleton_data_get_animations(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return ArrayAnimation(fromPointer: result!)
    }

    /// The skeleton's constraints.
    public var constraints: ArrayConstraintData {
        let result = spine_skeleton_data_get_constraints(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return ArrayConstraintData(fromPointer: result!)
    }

    /// The X coordinate of the skeleton's axis aligned bounding box in the setup pose.
    public var x: Float {
        get {
            let result = spine_skeleton_data_get_x(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return result
        }
        set {
            spine_skeleton_data_set_x(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), newValue)
        }
    }

    /// The Y coordinate of the skeleton's axis aligned bounding box in the setup pose.
    public var y: Float {
        get {
            let result = spine_skeleton_data_get_y(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return result
        }
        set {
            spine_skeleton_data_set_y(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), newValue)
        }
    }

    /// The width of the skeleton's axis aligned bounding box in the setup pose.
    public var width: Float {
        get {
            let result = spine_skeleton_data_get_width(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return result
        }
        set {
            spine_skeleton_data_set_width(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), newValue)
        }
    }

    /// The height of the skeleton's axis aligned bounding box in the setup pose.
    public var height: Float {
        get {
            let result = spine_skeleton_data_get_height(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return result
        }
        set {
            spine_skeleton_data_set_height(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), newValue)
        }
    }

    /// Baseline scale factor for applying physics and other effects based on distance to
    /// non-scalable properties, such as angle or scale. Default is 100.
    public var referenceScale: Float {
        get {
            let result = spine_skeleton_data_get_reference_scale(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return result
        }
        set {
            spine_skeleton_data_set_reference_scale(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), newValue)
        }
    }

    /// The Spine version used to export this data, or NULL.
    public var version: String {
        get {
            let result = spine_skeleton_data_get_version(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return String(cString: result!)
        }
        set {
            spine_skeleton_data_set_version(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), newValue)
        }
    }

    /// The skeleton data hash. This value will change if any of the skeleton data has changed.
    public var hashString: String {
        get {
            let result = spine_skeleton_data_get_hash(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return String(cString: result!)
        }
        set {
            spine_skeleton_data_set_hash(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), newValue)
        }
    }

    /// The path to the images folder as defined in Spine, or null if nonessential data was not
    /// exported.
    public var imagesPath: String {
        get {
            let result = spine_skeleton_data_get_images_path(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return String(cString: result!)
        }
        set {
            spine_skeleton_data_set_images_path(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), newValue)
        }
    }

    /// The path to the audio folder as defined in Spine, or null if nonessential data was not
    /// exported.
    public var audioPath: String {
        get {
            let result = spine_skeleton_data_get_audio_path(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return String(cString: result!)
        }
        set {
            spine_skeleton_data_set_audio_path(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), newValue)
        }
    }

    /// The dopesheet FPS in Spine. Available only when nonessential data was exported.
    public var fps: Float {
        get {
            let result = spine_skeleton_data_get_fps(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        return result
        }
        set {
            spine_skeleton_data_set_fps(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), newValue)
        }
    }

    /// Finds a bone by comparing each bone's name. It is more efficient to cache the results of
    /// this method than to call it multiple times.
    ///
    /// - Returns: May be NULL.
    public func findBone(_ boneName: String) -> BoneData? {
        let result = spine_skeleton_data_find_bone(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), boneName)
        return result.map { BoneData(fromPointer: $0) }
    }

    /// - Returns: May be NULL.
    public func findSlot(_ slotName: String) -> SlotData? {
        let result = spine_skeleton_data_find_slot(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), slotName)
        return result.map { SlotData(fromPointer: $0) }
    }

    /// - Returns: May be NULL.
    public func findSkin(_ skinName: String) -> Skin? {
        let result = spine_skeleton_data_find_skin(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), skinName)
        return result.map { Skin(fromPointer: $0) }
    }

    /// - Returns: May be NULL.
    public func findEvent(_ eventDataName: String) -> EventData? {
        let result = spine_skeleton_data_find_event(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), eventDataName)
        return result.map { EventData(fromPointer: $0) }
    }

    /// - Returns: May be NULL.
    public func findAnimation(_ animationName: String) -> Animation? {
        let result = spine_skeleton_data_find_animation(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), animationName)
        return result.map { Animation(fromPointer: $0) }
    }

    /// Collects animations used by slider constraints.
    public func findSliderAnimations(_ animations: ArrayAnimation) -> ArrayAnimation {
        let result = spine_skeleton_data_find_slider_animations(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self), animations._ptr.assumingMemoryBound(to: spine_array_animation_wrapper.self))
        return ArrayAnimation(fromPointer: result!)
    }

    public func dispose() {
        spine_skeleton_data_dispose(_ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
    }
}