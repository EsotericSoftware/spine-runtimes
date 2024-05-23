//
//  SpineUIView.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 16.05.24.
//

import UIKit

import MetalKit
import Spine

public final class SpineUIView: MTKView {
    
    let controller: SpineController
    let mode: Spine.ContentMode
    let alignment: Spine.Alignment
    let boundsProvider: BoundsProvider
    
    internal var computedBounds: CGRect = .zero
    internal var renderer: SpineRenderer?
    
    init(
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
        
        super.init(frame: .zero, device: MTLCreateSystemDefaultDevice())
        clearColor = MTLClearColor(backgroundColor)
        isOpaque = backgroundColor != .clear
    }
    
    convenience init(
        atlasFile: String,
        skeletonFile: String,
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .clear
    ) {
        self.init(controller: controller, mode: mode, alignment: alignment, boundsProvider: boundsProvider, backgroundColor: backgroundColor)
        Task.detached(priority: .high) {
            do {
                try await self.load(atlasFile: atlasFile, skeletonFile: skeletonFile)
            } catch {
                print(error)
            }
        }
    }
    
    convenience init(
        drawable: SkeletonDrawableWrapper,
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .clear
    ) {
        self.init(controller: controller, mode: mode, alignment: alignment, boundsProvider: boundsProvider, backgroundColor: backgroundColor)
        do {
            try load(drawable: drawable)
        } catch {
            print(error)
        }
    }
    
    public override init(frame frameRect: CGRect, device: MTLDevice?) {
        fatalError("init(frame: device:) has not been implemented. Use init() instead.")
    }
    
    required init(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented. Use init() instead.")
    }
}

extension SpineUIView {
    internal func load(atlasFile: String, skeletonFile: String) async throws {
        try await self.controller.load(
            atlasFile: atlasFile,
            skeletonFile: skeletonFile
        )
        try await MainActor.run {
            try self.load(drawable: self.controller.drawable)
        }
    }
    
    internal func load(drawable: SkeletonDrawableWrapper) throws {
        controller.drawable = drawable
        computedBounds = boundsProvider.computeBounds(for: drawable)
        try initRenderer(
            atlasPages: controller.drawable.atlasPages
        )
        controller.initialize()
    }
    
    private func initRenderer(atlasPages: [CGImage]) throws {
        renderer = try SpineRenderer(spineView: self, atlasPages: atlasPages)
        renderer?.delegate = controller
        renderer?.dataSource = controller
        renderer?.mtkView(self, drawableSizeWillChange: drawableSize)
        delegate = renderer
    }
}

extension SkeletonDrawableWrapper {
    
    func renderToImage(size: CGSize, backgroundColor: UIColor) throws -> CGImage? {
        let spineView = SpineUIView(
            controller: SpineController(disposeDrawableOnDeInit: false), // Doesn't own the drawable
            backgroundColor: backgroundColor
        )
        spineView.frame = CGRect(origin: .zero, size: size)
        spineView.isPaused = false
        spineView.enableSetNeedsDisplay = false
        spineView.framebufferOnly = false
        
        try spineView.load(drawable: self)
        spineView.delegate?.draw(in: spineView)
        
        guard let texture = spineView.currentDrawable?.texture else {
            throw "Could not read texture."
        }
        let width = texture.width
        let height = texture.height
        let rowBytes = width * 4
        let data = UnsafeMutableRawPointer.allocate(byteCount: rowBytes * height, alignment: MemoryLayout<UInt8>.alignment)
        defer {
            data.deallocate()
        }
        
        let region = MTLRegionMake2D(0, 0, width, height)
        texture.getBytes(data, bytesPerRow: rowBytes, from: region, mipmapLevel: 0)
        
        let bitmapInfo = CGBitmapInfo(
            rawValue: CGImageAlphaInfo.premultipliedFirst.rawValue
        ).union(.byteOrder32Little)
        
        let colorSpace = CGColorSpaceCreateDeviceRGB()
        guard let context = CGContext(data: data, width: width, height: height, bitsPerComponent: 8, bytesPerRow: rowBytes, space: colorSpace, bitmapInfo: bitmapInfo.rawValue),
              let cgImage = context.makeImage() else {
                throw "Could not create image."
        }
        return cgImage
    }
}
