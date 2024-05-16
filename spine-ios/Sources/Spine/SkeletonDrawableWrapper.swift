//
//  File.swift
//  
//
//  Created by Denis Andrašec on 15.05.24.
//

import Foundation
import Spine
import SpineWrapper
import CoreGraphics

public final class SkeletonDrawableWrapper {
    
    public let atlas: Atlas
    public let atlasPages: [CGImage]
    public let skeletonData: SkeletonData
    
    public let skeletonDrawable: SkeletonDrawable
    public let skeleton: Skeleton
    public let animationStateData: AnimationStateData
    public let animationState: AnimationState
    public let animationStateWrapper: AnimationStateWrapper
    
    internal var disposed = false
    
    public init(atlas: Atlas, atlasPages: [CGImage], skeletonData: SkeletonData) throws {
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
