import UIKit
import MetalKit

@objc
public final class SpineUIView: MTKView {
    
    let controller: SpineController
    let mode: Spine.ContentMode
    let alignment: Spine.Alignment
    let boundsProvider: BoundsProvider
    
    internal var computedBounds: CGRect = .zero
    internal var renderer: SpineRenderer?
    
    @objc public init(
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
    
    public override init(frame frameRect: CGRect, device: MTLDevice?) {
        fatalError("init(frame: device:) has not been implemented. Use init() instead.")
    }
    
    required init(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented. Use init() instead.")
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

public enum SpineViewSource {
    case bundle(atlasFileName: String, skeletonFileName: String, bundle: Bundle = .main)
    case file(atlasFile: URL, skeletonFile: URL)
    case http(atlasURL: URL, skeletonURL: URL)
    case drawable(SkeletonDrawableWrapper)
    
    func loadDrawable() async throws -> SkeletonDrawableWrapper {
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
