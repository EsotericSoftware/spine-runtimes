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
import UIKit

public typealias SpineControllerCallback = (_ controller: SpineController) -> Void

public final class SpineController: ObservableObject {
    
    public private(set) var drawable: SkeletonDrawableWrapper!
    
    private let onInitialized: SpineControllerCallback?
    private let onBeforeUpdateWorldTransforms: SpineControllerCallback?
    private let onAfterUpdateWorldTransforms: SpineControllerCallback?
    private let onBeforePaint: SpineControllerCallback?
    private let onAfterPaint: SpineControllerCallback?
    
    private var scaleX: CGFloat = 1
    private var scaleY: CGFloat = 1
    private var offsetX: CGFloat = 0
    private var offsetY: CGFloat = 0
    
    @Published
    public private(set) var isPlaying: Bool = true
    
    public init(
        onInitialized: SpineControllerCallback? = nil,
        onBeforeUpdateWorldTransforms: SpineControllerCallback? = nil,
        onAfterUpdateWorldTransforms: SpineControllerCallback? = nil,
        onBeforePaint: SpineControllerCallback? = nil,
        onAfterPaint: SpineControllerCallback? = nil
    ) {
        self.onInitialized = onInitialized
        self.onBeforeUpdateWorldTransforms = onBeforeUpdateWorldTransforms
        self.onAfterUpdateWorldTransforms = onAfterUpdateWorldTransforms
        self.onBeforePaint = onBeforePaint
        self.onAfterPaint = onAfterPaint
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
    
    public var animationStateWrapper: AnimationStateWrapper {
        drawable.animationStateWrapper
    }
    
    /// Transforms the coordinates given in the [SpineWidget] coordinate system in [position] to
    /// the skeleton coordinate system. See the `IKFollowing.swift` example how to use this
    /// to move a bone based on user touch input.
    public func toSkeletonCoordinates(position: CGPoint) -> CGPoint {
        let x = position.x;
        let y = position.y;
        return CGPoint(x: x / scaleX - offsetX, y: y / scaleY - offsetY)
    }
    
    public func pause() {
        isPlaying = false
    }
    
    public func resume() {
        isPlaying = true
    }
    
    internal func load(atlasFile: String, skeletonFile: String) async throws {
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
        }
    }
    
    internal func initialize() {
        onInitialized?(self)
    }
}

extension SpineController: SpineRendererDelegate {
    
    func spineRendererWillUpdate(_ spineRenderer: SpineRenderer) {
        onBeforeUpdateWorldTransforms?(self)
    }
    
    func spineRenderer(_ spineRenderer: SpineRenderer, needsUpdate delta: TimeInterval) {
        drawable?.update(delta: Float(delta))
    }
    
    func spineRendererDidUpdate(_ spineRenderer: SpineRenderer) {
        onAfterUpdateWorldTransforms?(self)
    }
    
    func spineRendererWillDraw(_ spineRenderer: SpineRenderer) {
        onBeforePaint?(self)
    }
    
    func spineRendererDidDraw(_ spineRenderer: SpineRenderer) {
        onAfterPaint?(self)
    }
    
    func spineRendererDidUpdate(_ spineRenderer: SpineRenderer, scaleX: CGFloat, scaleY: CGFloat, offsetX: CGFloat, offsetY: CGFloat) {
        self.scaleX = scaleX
        self.scaleY = scaleY
        self.offsetX = offsetX
        self.offsetY = offsetY
    }
}

extension SpineController: SpineRendererDataSource {
    
    func isPlaying(_ spineRenderer: SpineRenderer) -> Bool {
        return isPlaying
    }
    
    func skeletonDrawable(_ spineRenderer: SpineRenderer) -> SkeletonDrawableWrapper {
        return drawable
    }
    
    func renderCommands(_ spineRenderer: SpineRenderer) -> [RenderCommand] {
        return drawable?.skeletonDrawable.render() ?? []
    }
}
