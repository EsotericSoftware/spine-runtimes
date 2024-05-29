import Foundation
import Spine
import SpineCppLite
import CoreGraphics
import UIKit

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
    
    /// Updates the [AnimationState] using the [delta] time given in seconds, applies the
    /// animation state to the [Skeleton] and updates the world transforms of the skeleton
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
