import Foundation
import Spine
import SpineCppLite
import CoreGraphics
import UIKit

/// A ``SkeletonDrawableWrapper`` with ``SkeletonDrawable`` bundle loading, updating, and rendering an ``Atlas``, ``Skeleton``, and ``AnimationState``
/// into a single easy to use class.
///
/// Use the ``SkeletonDrawableWrapper/fromBundle(atlasFileName:skeletonFileName:bundle:)``, ``SkeletonDrawableWrapper/fromFile(atlasFile:skeletonFile:)``, or ``SkeletonDrawableWrapper/fromHttp(atlasURL:skeletonURL:)`` methods to construct a ``SkeletonDrawableWrapper``. To have
/// multiple skeleton drawable wrapper instances share the same ``Atlas`` and ``SkeletonData``, use the constructor.
///
/// You can then directly access the `skeletonDrawable` and with it `atlas`, `skeletonData`, `skeleton`, `animationStateData`, `animationState` and `animationStateWrapper`.
/// to query and animate the skeleton. Use the ``AnimationStateWrapper`` to queue animations on one or more tracks
/// via ``AnimationState/setAnimation(trackIndex:animation:loop:)`` or ``AnimationState/addAnimation(trackIndex:animation:loop:delay:)``.
///
/// To update the ``AnimationState`` and apply it to the ``Skeleton`` call the ``AnimationStateWrapper/update`` function, providing it
/// a delta time in seconds to advance the animations.
///
/// To render the current pose of the ``Skeleton`` as a `CGImage`, use ``SkeletonDrawableWrapper/renderToImage(size:backgroundColor:scaleFactor:)``.
///
/// When the skeleton drawable is no longer needed, call the ``SkeletonDrawableWrapper/dispose()`` method to release its resources. If
/// the skeleton drawable was constructed from a shared ``Atlas`` and ``SkeletonData``, make sure to dispose the
/// atlas and skeleton data as well, if no skeleton drawable references them anymore.
@objc(SpineSkeletonDrawableWrapper)
@objcMembers
public final class SkeletonDrawableWrapper: NSObject {
    
    public let atlas: Atlas
    public let atlasPages: [UIImage]
    public let skeletonData: SkeletonData
    
    public let skeletonDrawable: SkeletonDrawable
    public let skeleton: Skeleton
    public let animationStateData: AnimationStateData
    public let animationState: AnimationState
    public let animationStateWrapper: AnimationStateWrapper
    
    internal var disposed = false
    
    /// Constructs a new skeleton drawable from the `atlasFileName` and `skeletonFileName` from the `main` bundle
    /// or the optionally provided `bundle`.
    ///
    /// Throws an `Error` in case the data could not be loaded.
    public static func fromBundle(atlasFileName: String, skeletonFileName: String, bundle: Bundle = .main) async throws -> SkeletonDrawableWrapper {
        let atlasAndPages = try await Atlas.fromBundle(atlasFileName, bundle: bundle)
        let skeletonData = try await SkeletonData.fromBundle(
            atlas: atlasAndPages.0,
            skeletonFileName: skeletonFileName,
            bundle: bundle
        )
        return try SkeletonDrawableWrapper(
            atlas: atlasAndPages.0,
            atlasPages: atlasAndPages.1,
            skeletonData: skeletonData
        )
    }
    
    /// Constructs a new skeleton drawable from the `atlasFile` and `skeletonFile`.
    ///
    /// Throws an `Error` in case the data could not be loaded.
    public static func fromFile(atlasFile: URL, skeletonFile: URL) async throws -> SkeletonDrawableWrapper {
        let atlasAndPages = try await Atlas.fromFile(atlasFile)
        let skeletonData = try await SkeletonData.fromFile(
            atlas: atlasAndPages.0,
            skeletonFile: skeletonFile
        )
        return try SkeletonDrawableWrapper(
            atlas: atlasAndPages.0,
            atlasPages: atlasAndPages.1,
            skeletonData: skeletonData
        )
    }
    
    /// Constructs a new skeleton drawable wrapper from the http `atlasUrl` and `skeletonUrl`.
    ///
    /// Throws an `Error` in case the data could not be loaded.
    public static func fromHttp(atlasURL: URL, skeletonURL: URL) async throws -> SkeletonDrawableWrapper {
        let atlasAndPages = try await Atlas.fromHttp(atlasURL)
        let skeletonData = try await SkeletonData.fromHttp(
            atlas: atlasAndPages.0,
            skeletonURL: skeletonURL
        )
        return try SkeletonDrawableWrapper(
            atlas: atlasAndPages.0,
            atlasPages: atlasAndPages.1,
            skeletonData: skeletonData
        )
    }
    
    public init(atlas: Atlas, atlasPages: [UIImage], skeletonData: SkeletonData) throws {
        self.atlas = atlas
        self.atlasPages = atlasPages
        self.skeletonData = skeletonData
            
        guard let nativeSkeletonDrawable = spine_skeleton_drawable_create(skeletonData.wrappee) else {
            throw "Could not load native skeleton drawable"
        }
        skeletonDrawable = SkeletonDrawable(nativeSkeletonDrawable)
        
        guard let nativeSkeleton = spine_skeleton_drawable_get_skeleton(skeletonDrawable.wrappee) else {
            throw "Could not load native skeleton"
        }
        skeleton = Skeleton(nativeSkeleton)
        
        guard let nativeAnimationStateData = spine_skeleton_drawable_get_animation_state_data(skeletonDrawable.wrappee) else {
            throw "Could not load native animation state data"
        }
        animationStateData = AnimationStateData(nativeAnimationStateData)
        
        guard let nativeAnimationState = spine_skeleton_drawable_get_animation_state(skeletonDrawable.wrappee) else {
            throw "Could not load native animation state"
        }
        animationState = AnimationState(nativeAnimationState)
        animationStateWrapper = AnimationStateWrapper(
            animationState: animationState,
            aninationStateEvents: skeletonDrawable.animationStateEvents
        )
        skeleton.updateWorldTransform(physics: SPINE_PHYSICS_NONE)
        super.init()
    }
    
    /// Updates the ``AnimationState`` using the `delta` time given in seconds, applies the
    /// animation state to the ``Skeleton`` and updates the world transforms of the skeleton
    /// to calculate its current pose.
    public func update(delta: Float) {
        if disposed { return }
        
        animationStateWrapper.update(delta: delta)
        animationState.apply(skeleton: skeleton)
        
        skeleton.update(delta: delta)
        skeleton.updateWorldTransform(physics: SPINE_PHYSICS_UPDATE)
    }
    
    public func dispose() {
        if disposed { return }
        disposed = true
        
        atlas.dispose()
        skeletonData.dispose()
        
        skeletonDrawable.dispose()
    }
}
