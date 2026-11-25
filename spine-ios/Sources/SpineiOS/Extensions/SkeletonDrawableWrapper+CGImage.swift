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
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import CoreGraphics
import Foundation
import Metal

extension SkeletonDrawableWrapper {

    /// Render the ``Skeleton`` to a `CGImage`
    ///
    /// Parameters:
    ///     - size: The size of the `CGImage` that should be rendered.
    ///     - boundsProvider: The skeleton bounds provider used to compute fitting and positioning.
    ///     - backgroundColor: the background color of the image
    ///     - scaleFactor: The scale factor. Set this to `UIScreen.main.scale` if you want to show the image in a view
    public func renderToImage(
        size: CGSize,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: SpineUIColor,
        scaleFactor: CGFloat = 1
    ) throws -> CGImage? {
        let controller = SpineController(disposeDrawableOnDeInit: false)  // Doesn't own the drawable
        let spineView = SpineUIView(
            controller: controller,
            boundsProvider: boundsProvider,
            backgroundColor: backgroundColor
        )
#if canImport(UIKit)
        spineView.frame = CGRect(origin: .zero, size: size)
#elseif canImport(AppKit)
        let scaledSize = CGSize(width: size.width * scaleFactor, height: size.height * scaleFactor)
        spineView.autoResizeDrawable = false
        spineView.frame = CGRect(origin: .zero, size: scaledSize)
        spineView.drawableSize = scaledSize
#endif
        spineView.isPaused = false
        spineView.enableSetNeedsDisplay = false
        spineView.framebufferOnly = false

#if canImport(UIKit)
        spineView.contentScaleFactor = scaleFactor
#endif

        defer {
            controller.drawable = nil
            spineView.delegate = nil
            spineView.renderer = nil
        }

        try spineView.load(drawable: self)
        spineView.renderer?.waitUntilCompleted = true

        spineView.delegate?.draw(in: spineView)

        guard let texture = spineView.currentDrawable?.texture else {
            throw SpineError("Could not read texture.")
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
        guard
            let context = CGContext(
                data: data, width: width, height: height, bitsPerComponent: 8, bytesPerRow: rowBytes, space: colorSpace,
                bitmapInfo: bitmapInfo.rawValue),
            let cgImage = context.makeImage()
        else {
            throw SpineError("Could not create image.")
        }
        return cgImage
    }
}
