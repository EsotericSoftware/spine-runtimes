/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import CoreGraphics
import Foundation
import Metal
import SpineSwift

/// Renders a Spine skeleton into caller-owned Metal textures without creating
/// an ``MTKView`` or participating in a view display loop.
///
/// The renderer uses the same animation updates, clipping, mesh deformation,
/// slot order, blend modes, atlas textures, and controller callbacks as
/// ``SpineUIView``. The caller owns presentation and may either let this class
/// create and commit a command buffer or provide one for shared synchronization.
@objcMembers
public final class SpineMetalRenderer: NSObject {
    public let controller: SpineController

    private let renderer: SpineRenderer
    private let bounds: CGRect
    private let mode: SpineContentMode
    private let alignment: SpineAlignment
    private let pixelFormat: MTLPixelFormat

    public init(
        drawable: SkeletonDrawableWrapper,
        controller: SpineController = SpineController(),
        mode: SpineContentMode = .fit,
        alignment: SpineAlignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        pixelFormat: MTLPixelFormat = .bgra8Unorm,
        textureFilter: SpineTextureFilter = .atlas
    ) throws {
        self.controller = controller
        self.bounds = boundsProvider.computeBounds(for: drawable)
        self.mode = mode
        self.alignment = alignment
        self.pixelFormat = pixelFormat

        controller.drawable = drawable
        let pma = controller.atlas.pages.count > 0
            ? (controller.atlas.pages[0]?.pma ?? false)
            : false
        renderer = try SpineRenderer(
            device: SpineObjects.shared.device,
            commandQueue: SpineObjects.shared.commandQueue,
            pixelFormat: pixelFormat,
            atlas: controller.atlas,
            atlasPages: drawable.atlasPages,
            pma: pma,
            textureFilter: textureFilter
        )

        super.init()

        renderer.delegate = controller
        renderer.dataSource = controller
        controller.initialize()
    }

    /// Renders into a texture and commits a command buffer owned by the Spine
    /// renderer. `sizeInPoints` controls content fitting and defaults to the
    /// texture's pixel dimensions, producing a backing scale of one.
    @discardableResult
    public func render(
        to texture: MTLTexture,
        sizeInPoints: CGSize? = nil,
        clearColor: MTLClearColor = MTLClearColorMake(0, 0, 0, 0),
        completion: ((MTLCommandBuffer) -> Void)? = nil
    ) -> Bool {
        updateViewport(for: texture, sizeInPoints: sizeInPoints)
        return renderer.draw(
            to: texture,
            pixelFormat: pixelFormat,
            clearColor: clearColor,
            completion: completion
        )
    }

    /// Encodes into a caller-owned command buffer. The caller must commit the
    /// command buffer after this method returns `true`.
    @discardableResult
    public func render(
        to texture: MTLTexture,
        commandBuffer: MTLCommandBuffer,
        sizeInPoints: CGSize? = nil,
        clearColor: MTLClearColor = MTLClearColorMake(0, 0, 0, 0),
        completion: ((MTLCommandBuffer) -> Void)? = nil
    ) -> Bool {
        updateViewport(for: texture, sizeInPoints: sizeInPoints)
        return renderer.draw(
            to: texture,
            pixelFormat: pixelFormat,
            commandBuffer: commandBuffer,
            clearColor: clearColor,
            completion: completion
        )
    }

    private func updateViewport(for texture: MTLTexture, sizeInPoints: CGSize?) {
        let drawableSize = CGSize(width: texture.width, height: texture.height)
        renderer.updateViewport(
            sizeInPoints: sizeInPoints ?? drawableSize,
            drawableSize: drawableSize,
            bounds: bounds,
            mode: mode,
            alignment: alignment
        )
    }
}
