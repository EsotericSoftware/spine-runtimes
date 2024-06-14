import UIKit
import MetalKit

/// A ``UIView`` to display a Spine skeleton. The skeleton can be loaded from a bundle, local files, http, or a pre-loaded ``SkeletonDrawableWrapper``.
///
/// The skeleton displayed by a ``SpineUIView`` can be controlled via a ``SpineController``.
///
/// The size of the widget can be derived from the bounds provided by a ``BoundsProvider``. If the view is not sized by the bounds
/// computed by the ``BoundsProvider``, the widget will use the computed bounds to fit the skeleton inside the view's dimensions.
///
/// This is a direct subclass of ``MTKView`` and is using `Metal` to render the skeleton.
@objc
public final class SpineUIView: MTKView {
    
    let controller: SpineController
    let mode: Spine.ContentMode
    let alignment: Spine.Alignment
    let boundsProvider: BoundsProvider
    
    internal var computedBounds: CGRect = .zero
    internal var renderer: SpineRenderer?
    
    @objc internal init(
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .clear
    ) {
        self.controller = controller
        self.mode = mode
        self.alignment = alignment
        self.boundsProvider = boundsProvider
        
        super.init(frame: .zero, device: SpineObjects.shared.device)
        clearColor = MTLClearColor(backgroundColor)
        isOpaque = backgroundColor != .clear
    }
    
    /// An initializer that constructs a new ``SpineUIView`` from a ``SpineViewSource``.
    ///
    /// After initialization is complete, the provided `controller` is invoked as per the ``SpineController`` semantics, to allow
    /// modifying how the skeleton inside the widget is animated and rendered.
    ///
    /// - Parameters:
    ///     - from: Specifies the ``SpineViewSource`` from which to load `atlas` and `skeleton` data.
    ///     - controller: The ``SpineController`` used to modify how the skeleton inside the view is animated and rendered.
    ///     - mode: How the skeleton is fitted inside ``SpineUIView``. Per default, it is `.fit`
    ///     - alignment: How the skeleton is alignment inside ``SpineUIView``. Per default, it is `.center`
    ///     - boundsProvider: The skeleton bounds must be computed via a ``BoundsProvider``. Per default, ``SetupPoseBounds`` is used.
    ///     - backgroundColor: The background color of the view. Per defaut, `UIColor.clear` is used
    ///
    /// - Returns: A new instance of ``SpineUIView``.
    public convenience init(
        from source: SpineViewSource,
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .clear
    ) {
        self.init(controller: controller, mode: mode, alignment: alignment, boundsProvider: boundsProvider, backgroundColor: backgroundColor)
        Task.detached(priority: .high) {
            do {
                let drawable = try await source.loadDrawable()
                try await self.load(drawable: drawable)
            } catch {
                print(error)
            }
        }
    }
    
    /// A convenience initializer that constructs a new ``SpineUIView`` from bundled files.
    ///
    /// After initialization is complete, the provided `controller` is invoked as per the ``SpineController`` semantics, to allow
    /// modifying how the skeleton inside the widget is animated and rendered.
    ///
    /// - Parameters:
    ///     - atlasFileName: Specifies the `.atlas` file to be loaded for the images used to render the skeleton
    ///     - skeletonFileName: Specifies either a Skeleton `.json` or `.skel` file containing the skeleton data
    ///     - bundle: Specifies from which bundle to load the files. Per default, it is `Bundle.main`
    ///     - controller: The ``SpineController`` used to modify how the skeleton inside the view is animated and rendered.
    ///     - mode: How the skeleton is fitted inside ``SpineUIView``. Per default, it is `.fit`
    ///     - alignment: How the skeleton is alignment inside ``SpineUIView``. Per default, it is `.center`
    ///     - boundsProvider: The skeleton bounds must be computed via a ``BoundsProvider``. Per default, ``SetupPoseBounds`` is used.
    ///     - backgroundColor: The background color of the view. Per defaut, `UIColor.clear` is used
    ///
    /// - Returns: A new instance of ``SpineUIView``.
    @objc public convenience init(
        atlasFileName: String,
        skeletonFileName: String,
        bundle: Bundle = .main,
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .clear
    ) {
        self.init(from: .bundle(atlasFileName: atlasFileName, skeletonFileName: skeletonFileName, bundle: bundle), controller: controller, mode: mode, alignment: alignment, boundsProvider: boundsProvider, backgroundColor: backgroundColor)
    }
    
    /// A convenience initializer that constructs a new ``SpineUIView`` from file URLs.
    ///
    /// After initialization is complete, the provided `controller` is invoked as per the ``SpineController`` semantics, to allow
    /// modifying how the skeleton inside the widget is animated and rendered.
    ///
    /// - Parameters:
    ///     - atlasFile: Specifies the `.atlas` file to be loaded for the images used to render the skeleton
    ///     - skeletonFile: Specifies either a Skeleton `.json` or `.skel` file containing the skeleton data
    ///     - controller: The ``SpineController`` used to modify how the skeleton inside the view is animated and rendered.
    ///     - mode: How the skeleton is fitted inside ``SpineUIView``. Per default, it is `.fit`
    ///     - alignment: How the skeleton is alignment inside ``SpineUIView``. Per default, it is `.center`
    ///     - boundsProvider: The skeleton bounds must be computed via a ``BoundsProvider``. Per default, ``SetupPoseBounds`` is used.
    ///     - backgroundColor: The background color of the view. Per defaut, `UIColor.clear` is used
    ///
    /// - Returns: A new instance of ``SpineUIView``.
    @objc public convenience init(
        atlasFile: URL,
        skeletonFile: URL,
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .clear
    ) {
        self.init(from: .file(atlasFile: atlasFile, skeletonFile: skeletonFile), controller: controller, mode: mode, alignment: alignment, boundsProvider: boundsProvider, backgroundColor: backgroundColor)
    }
    
    /// A convenience initializer that constructs a new ``SpineUIView`` from HTTP.
    ///
    /// After initialization is complete, the provided `controller` is invoked as per the ``SpineController`` semantics, to allow
    /// modifying how the skeleton inside the widget is animated and rendered.
    ///
    /// - Parameters:
    ///     - atlasURL: Specifies the `.atlas` file http URL to be loaded for the images used to render the skeleton
    ///     - skeletonURL: Specifies either a Skeleton `.json` or `.skel` file http URL containing the skeleton data
    ///     - controller: The ``SpineController`` used to modify how the skeleton inside the view is animated and rendered.
    ///     - mode: How the skeleton is fitted inside ``SpineUIView``. Per default, it is `.fit`
    ///     - alignment: How the skeleton is alignment inside ``SpineUIView``. Per default, it is `.center`
    ///     - boundsProvider: The skeleton bounds must be computed via a ``BoundsProvider``. Per default, ``SetupPoseBounds`` is used.
    ///     - backgroundColor: The background color of the view. Per defaut, `UIColor.clear` is used
    ///
    /// - Returns: A new instance of ``SpineUIView``.
    @objc public convenience init(
        atlasURL: URL,
        skeletonURL: URL,
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .clear
    ) {
        self.init(from: .http(atlasURL: atlasURL, skeletonURL: skeletonURL), controller: controller, mode: mode, alignment: alignment, boundsProvider: boundsProvider, backgroundColor: backgroundColor)
    }
    
    /// A convenience initializer that constructs a new ``SpineUIView`` with a ``SkeletonDrawableWrapper``.
    ///
    /// After initialization is complete, the provided `controller` is invoked as per the ``SpineController`` semantics, to allow
    /// modifying how the skeleton inside the widget is animated and rendered.
    ///
    /// - Parameters:
    ///     - drawable: The ``SkeletonDrawableWrapper`` provided directly to the ``SpineController``
    ///     - controller: The ``SpineController`` used to modify how the skeleton inside the view is animated and rendered.
    ///     - mode: How the skeleton is fitted inside ``SpineUIView``. Per default, it is `.fit`
    ///     - alignment: How the skeleton is alignment inside ``SpineUIView``. Per default, it is `.center`
    ///     - boundsProvider: The skeleton bounds must be computed via a ``BoundsProvider``. Per default, ``SetupPoseBounds`` is used.
    ///
    /// - Returns: A new instance of ``SpineUIView``.
    @objc public convenience init(
        drawable: SkeletonDrawableWrapper,
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .clear
    ) {
        self.init(from: .drawable(drawable), controller: controller, mode: mode, alignment: alignment, boundsProvider: boundsProvider, backgroundColor: backgroundColor)
    }
    
    internal override init(frame frameRect: CGRect, device: MTLDevice?) {
        fatalError("init(frame: device:) has not been implemented. Use init() instead.")
    }
    
    internal required init(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented. Use init() instead.")
    }
    
    /// Disable or enable rendering. Disable it when the spine view is out of bounds and you want to preserve CPU/GPU resources.
    public var isRendering: Bool {
        get { !super.isPaused }
        set { super.isPaused = !newValue }
    }
}

extension SpineUIView {
    
    internal func load(drawable: SkeletonDrawableWrapper) throws {
        controller.drawable = drawable
        computedBounds = boundsProvider.computeBounds(for: drawable)
        try initRenderer(
            atlasPages: controller.drawable.atlasPages
        )
        controller.initialize()
    }
    
    private func initRenderer(atlasPages: [UIImage]) throws {
        renderer = try SpineRenderer(
            device: SpineObjects.shared.device,
            commandQueue: SpineObjects.shared.commandQueue,
            pixelFormat: colorPixelFormat,
            atlasPages: atlasPages,
            pma: controller.drawable.atlas.isPma
        )
        renderer?.delegate = controller
        renderer?.dataSource = controller
        renderer?.mtkView(self, drawableSizeWillChange: drawableSize)
        delegate = renderer
    }
}

/// Defines from which source the ``SkeletonDrawableWrapper`` holding `atlas` and `skeleton` data is loaded.
///
/// The following sources are supported:
///     - bundle: Provide file names of your `atlas` and `skeleton` files, including the file extension, to load them from a ``Bundle``. Per defailt, ``Bundle.main`` is used.
///     - file: Provide file URLs to the `atlas` and `skeleton` files.
///     - http: Provide http URLs to the `atlas` and `skeleton` files.
///     - drawable: Directly provide a ``SkeletonDrawableWrapper``
///
public enum SpineViewSource {
    case bundle(atlasFileName: String, skeletonFileName: String, bundle: Bundle = .main)
    case file(atlasFile: URL, skeletonFile: URL)
    case http(atlasURL: URL, skeletonURL: URL)
    case drawable(SkeletonDrawableWrapper)
    
    internal func loadDrawable() async throws -> SkeletonDrawableWrapper {
        switch self {
        case .bundle(let atlasFileName, let skeletonFileName, let bundle):
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
        case .file(let atlasFile, let skeletonFile):
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
        case .http(let atlasURL, let skeletonURL):
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
        case .drawable(let skeletonDrawableWrapper):
            return skeletonDrawableWrapper
        }
    }
}
