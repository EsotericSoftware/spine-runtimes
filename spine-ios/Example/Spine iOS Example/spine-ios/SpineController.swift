//
//  ConentViewModel.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 08.05.24.
//

import Foundation
import Spine
import CoreGraphics
import QuartzCore

public final class SpineController: ObservableObject {
    
    public private(set) var drawable: SkeletonDrawableWrapper!
    
    private let onInitialized: (_ controller: SpineController) -> Void
    
    @Published
    public private(set) var isPlaying: Bool = true
    
    public init(onInitialized: @escaping (_ controller: SpineController) -> Void) {
        self.onInitialized = onInitialized
    }
    
    deinit {
        drawable?.dispose()
    }
    
    public var atlas: Atlas {
        drawable.atlas
    }
    
    public var skeletonData: Skeleton {
        drawable.skeleton
    }
    
    public var skeleton: Skeleton {
        drawable.skeleton
    }
    
    public var animationStateData: AnimationStateData {
        drawable.animationStateData
    }
    
    public var animationState: AnimationState {
        drawable.animationState
    }
    
    public func pause() {
        isPlaying = false
    }
    
    public func resume() {
        // TODO: Resume at correct time
        isPlaying = true
    }
    
    internal func initialize(atlasFile: String, skeletonFile: String) async throws {
        let atlasAndPages = try await Atlas.fromAsset(atlasFile)
        try await MainActor.run {
            let skeletonData = try SkeletonData.fromAsset(
                atlas: atlasAndPages.0,
                skeletonFile: skeletonFile
            )
            let skeletonDrawableWrapper = try SkeletonDrawableWrapper(
                atlas: atlasAndPages.0,
                atlasPages: atlasAndPages.1,
                skeletonData: skeletonData
            )
            self.drawable = skeletonDrawableWrapper
            
            onInitialized(self)
        }
    }
}

extension SpineController: SpineRendererDelegate {
    func spineRenderer(_ spineRenderer: SpineRenderer, needsUpdate delta: TimeInterval) {
        drawable?.update(delta: Float(delta))
    }
}

extension SpineController: SpineRendererDataSource {
    
    func isPlaying(_ spineRenderer: SpineRenderer) -> Bool {
        return isPlaying
    }
    
    func renderCommands(_ spineRenderer: SpineRenderer) -> [RenderCommand] {
        return drawable?.skeletonDrawable.render() ?? []
    }
}
