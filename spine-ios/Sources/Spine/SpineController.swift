import Foundation
import CoreGraphics
import QuartzCore
import UIKit

public typealias SpineControllerCallback = (_ controller: SpineController) -> Void

/// Controls how the skeleton of a ``SpineUIView`` is animated and rendered.
///
/// Upon initialization of a ``SpineUIView`` the provided `onInitialized` callback method is called once. This method can be used
/// to setup the initial animation(s) of the skeleton, among other things.
///
/// After initialization is complete, the ``SpineUIView`` is rendered at the screen refresh rate. In each frame,
/// the ``AnimationState`` is updated and applied to the ``Skeleton``.
///
/// Next the optionally provided method `onBeforeUpdateWorldTransforms` is called, which can modify the
/// skeleton before its current pose is calculated using ``Skeleton/updateWorldTransform(physics:)``. After
/// ``Skeleton.updateWorldTransforms`` has completed, the optional `onAfterUpdateWorldTransforms` method is
/// called, which can modify the current pose before rendering the skeleton.
///
/// Before the skeleton's current pose is rendered by the ``SpineUIView`` the optional `onBeforePaint` is called,
/// which allows rendering backgrounds or other objects that should go behind the skeleton in your view hierarchy. The
/// ``SpineUIView`` then renderes the skeleton's current pose, and finally calls the optional `onAfterPaint`, after which you
/// can render additional objects on top of the skeleton in your view hierarchy.
///
/// The underlying ``Atlas``, ``SkeletonData``, ``Skeleton``, ``AnimationStateData``, ``AnimationState``, and ``SkeletonDrawable``
/// can be accessed through their respective getters to inspect and/or modify the skeleton and its associated data. Accessing
/// this data is only allowed if the ``SpineUIView`` and its data have been initialized and have not been disposed yet.
///
/// By default, the view updates and renders the skeleton every frame. The `pause` method can be used to pause updating
/// and rendering the skeleton. The `resume` method resumes updating and rendering the skeleton. The `isPlaying` property
/// reports the current state.
///
/// Per default, ``SkeletonDrawableWrapper`` is disposed when ``SpineController`` is deinitialized. You can disable this behaviour with the ``disposeDrawableOnDeInit`` contructor parameter.
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
    
    /// Constructs a new ``SpineUIview`` controller. See the class documentation of ``SpineWidgetController`` for information on
    /// the optional arguments.
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
    
    /// The ``Atlas`` from which images to render the skeleton are sourced.
    public var atlas: Atlas {
        drawable.atlas
    }
    
    /// The setup-pose data used by the skeleton.
    public var skeletonData: SkeletonData {
        drawable.skeletonData
    }
    
    /// The ``Skeleton``
    public var skeleton: Skeleton {
        drawable.skeleton
    }
    
    /// The mixing information used by the ``AnimationState``
    public var animationStateData: AnimationStateData {
        drawable.animationStateData
    }
    
    /// The ``AnimationState`` used to manage animations that are being applied to the
    /// skeleton.
    public var animationState: AnimationState {
        drawable.animationState
    }
    
    /// The ``AnimationStateWrapper`` used to hold ``AnimationState``, register ``AnimationStateListener`` and call ``AnimationStateWrapper/update(delta:)``
    public var animationStateWrapper: AnimationStateWrapper {
        drawable.animationStateWrapper
    }
    
    /// Transforms the coordinates given in the ``SpineUIView`` coordinate system in `position` to
    /// the skeleton coordinate system. See the `IKFollowing.swift` example how to use this
    /// to move a bone based on user touch input.
    public func toSkeletonCoordinates(position: CGPoint) -> CGPoint {
        let x = position.x;
        let y = position.y;
        return CGPoint(
            x: (x - viewSize.width / 2) / scaleX - offsetX,
            y: (y - viewSize.height / 2) / scaleY - offsetY
        )
    }
    
    /// Transforms the coordinates given in skeleton coordinate system to
    /// the the ``SpineUIView`` coordinates. See the `DebugRendering.swift` example hot to use this to draw rectangles over skeleton bones for debugging purposes.
    public func fromSkeletonCoordinates(position: CGPoint) -> CGPoint {
        let x = position.x;
        let y = position.y;
        return CGPoint(
            x: (x + offsetX) * scaleX,
            y: (y + offsetY) * scaleY
        )
    }
    
    /// Pauses updating and rendering the skeleton.
    public func pause() {
        isPlaying = false
    }
    
    /// Resumes updating and rendering the skeleton.
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
