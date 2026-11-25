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

#if canImport(UIKit)
import SwiftUI

/// A `SwiftUI` `View` to display a Spine skeleton. The skeleton can be loaded from a bundle, local files, http, or a pre-loaded ``SkeletonDrawableWrapper``.
///
/// The skeleton displayed by a ``SpineUIView`` can be controlled via a ``SpineController``.
///
/// The size of the widget can be derived from the bounds provided by a ``BoundsProvider``. If the view is not sized by the bounds
/// computed by the ``BoundsProvider``, the widget will use the computed bounds to fit the skeleton inside the view's dimensions.
///
/// This is a ``UIViewRepresentable`` of `SpineUIView`.
public struct SpineView: UIViewRepresentable {
    public typealias UIViewType = SpineUIView

    private let source: SpineViewSource
    private let controller: SpineController
    private let mode: SpineContentMode
    private let alignment: SpineAlignment
    private let boundsProvider: BoundsProvider
    private let backgroundColor: SpineUIColor  // Not using `SwiftUI.Color`, as bridging to `UIColor` prior to iOS 14 might not always work.
    private let textureFilter: SpineTextureFilter

    @Binding
    private var isRendering: Bool?

    /// An initializer that constructs a new ``SpineView`` from a ``SpineViewSource``.
    ///
    /// After initialization is complete, the provided `controller` is invoked as per the ``SpineController`` semantics, to allow
    /// modifying how the skeleton inside the widget is animated and rendered.
    ///
    /// - Parameters:
    ///     - from: Specifies the ``SpineViewSource`` from which to load `atlas` and `skeleton` data.
    ///     - controller: The ``SpineController`` used to modify how the skeleton inside the view is animated and rendered.
    ///     - skeletonFileName: Specifies either a Skeleton `.json` or `.skel` file containing the skeleton data
    ///     - bundle: Specifies from which bundle to load the files. Per default, it is `Bundle.main`
    ///     - mode: How the skeleton is fitted inside ``SpineUIView``. Per default, it is `.fit`
    ///     - alignment: How the skeleton is alignment inside ``SpineUIView``. Per default, it is `.center`
    ///     - boundsProvider: The skeleton bounds must be computed via a ``BoundsProvider``. Per default, ``SetupPoseBounds`` is used.
    ///     - backgroundColor: The background color of the view. Per default, `UIColor.clear` is used
    ///     - textureFilter: The texture filtering mode. Per default, atlas page settings are used.
    ///     - isRendering: Binding to disable or enable rendering. Disable it when the spine view is out of bounds and you want to preserve CPU/GPU resources.
    ///
    /// - Returns: A new instance of ``SpineView``.
    public init(
        from source: SpineViewSource,
        controller: SpineController = SpineController(),
        mode: SpineContentMode = .fit,
        alignment: SpineAlignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: SpineUIColor = .clear,
        textureFilter: SpineTextureFilter = .atlas,
        isRendering: Binding<Bool?> = .constant(nil)
    ) {
        self.source = source
        self.controller = controller
        self.mode = mode
        self.alignment = alignment
        self.boundsProvider = boundsProvider
        self.backgroundColor = backgroundColor
        self.textureFilter = textureFilter
        _isRendering = isRendering
    }

    public func makeUIView(context: Context) -> SpineUIView {
        return SpineUIView(
            from: source,
            controller: controller,
            mode: mode,
            alignment: alignment,
            boundsProvider: boundsProvider,
            backgroundColor: backgroundColor,
            textureFilter: textureFilter
        )
    }

    public func updateUIView(_ uiView: SpineUIView, context: Context) {
        if let isRendering {
            uiView.isRendering = isRendering
        }
    }
}
#endif
