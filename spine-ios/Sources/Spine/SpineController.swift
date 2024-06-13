import Foundation
import CoreGraphics
import QuartzCore
import UIKit

public typealias SpineControllerCallback = (_ controller: SpineController) -> Void

@objcMembers
public final class SpineController: NSObject, ObservableObject {
    
    public internal(set) var drawable: SkeletonDrawableWrapper!
    
    private let onInitialized: SpineControllerCallback?
    private let onBeforeUpdateWorldTransforms: SpineControllerCallback?
    private let onAfterUpdateWorldTransforms: SpineControllerCallback?
    private let onBeforePaint: SpineControllerCallback?
    private let onAfterPaint: SpineControllerCallback?
    private let disposeDrawableOnDeInit: Bool
    
    private var displayLink: CADisplayLink?
    
    private var scaleX: CGFloat = 1
    private var scaleY: CGFloat = 1
    private var offsetX: CGFloat = 0
    private var offsetY: CGFloat = 0
    
    @Published
    public private(set) var isPlaying: Bool = true
    
    @Published
    public private(set) var viewSize: CGSize = .zero
    
    public init(
        onInitialized: SpineControllerCallback? = nil,
        onBeforeUpdateWorldTransforms: SpineControllerCallback? = nil,
        onAfterUpdateWorldTransforms: SpineControllerCallback? = nil,
        onBeforePaint: SpineControllerCallback? = nil,
        onAfterPaint: SpineControllerCallback? = nil,
        disposeDrawableOnDeInit: Bool = true
    ) {
        self.onInitialized = onInitialized
        self.onBeforeUpdateWorldTransforms = onBeforeUpdateWorldTransforms
        self.onAfterUpdateWorldTransforms = onAfterUpdateWorldTransforms
        self.onBeforePaint = onBeforePaint
        self.onAfterPaint = onAfterPaint
        self.disposeDrawableOnDeInit = disposeDrawableOnDeInit
        
        super.init()
        
        addDisplayLinkIfNeeded()
    }
    
    deinit {
        if disposeDrawableOnDeInit {
            drawable?.dispose() // TODO move drawable out of view?
        }
        removeDisplayLink()
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
    
    public func toSkeletonCoordinates(position: CGPoint) -> CGPoint {
        let x = position.x;
        let y = position.y;
        return CGPoint(
            x: (x - viewSize.width / 2) / scaleX - offsetX,
            y: (y - viewSize.height / 2) / scaleY - offsetY
        )
    }
    
    public func fromSkeletonCoordinates(position: CGPoint) -> CGPoint {
        let x = position.x;
        let y = position.y;
        return CGPoint(
            x: (x + offsetX) * scaleX,
            y: (y + offsetY) * scaleY
        )
    }
    
    public func pause() {
        isPlaying = false
    }
    
    public func resume() {
        isPlaying = true
    }
    
    internal func load(atlasFile: String, skeletonFile: String, bundle: Bundle = .main) async throws {
        let atlasAndPages = try await Atlas.fromBundle(atlasFile, bundle: bundle)
        let skeletonData = try await SkeletonData.fromBundle(
            atlas: atlasAndPages.0,
            skeletonFileName: skeletonFile,
            bundle: bundle
        )
        try await MainActor.run {
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
    
    internal var lastUpdate: CFTimeInterval = 0
    
    @objc private func updateDrawable() {
        guard isPlaying else {
            lastUpdate = CACurrentMediaTime()
            return
        }
        
        if lastUpdate == 0 {
            lastUpdate = CACurrentMediaTime()
        }
        let delta = CACurrentMediaTime() - lastUpdate
        onBeforeUpdateWorldTransforms?(self)
        drawable?.update(delta: Float(delta))
        lastUpdate = CACurrentMediaTime()
        onAfterUpdateWorldTransforms?(self)
    }
    
    private func addDisplayLinkIfNeeded() {
        guard displayLink == nil else {
            return
        }
        displayLink = CADisplayLink(target: self, selector: #selector(updateDrawable))
        displayLink?.add(to: .current, forMode: .common)
    }

    private func removeDisplayLink() {
        displayLink?.remove(from: .current, forMode: .common)
        displayLink = nil
    }
}

extension SpineController: SpineRendererDelegate {
    
    func spineRendererWillDraw(_ spineRenderer: SpineRenderer) {
        onBeforePaint?(self)
    }
    
    func spineRendererDidDraw(_ spineRenderer: SpineRenderer) {
        onAfterPaint?(self)
    }
    
    func spineRendererDidUpdate(_ spineRenderer: SpineRenderer, scaleX: CGFloat, scaleY: CGFloat, offsetX: CGFloat, offsetY: CGFloat, size: CGSize) {
        self.scaleX = scaleX
        self.scaleY = scaleY
        self.offsetX = offsetX
        self.offsetY = offsetY
        self.viewSize = size
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
