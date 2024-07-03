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
    private let mode: Spine.ContentMode
    private let alignment: Spine.Alignment
    private let boundsProvider: BoundsProvider
    private let backgroundColor: UIColor // Not using `SwiftUI.Color`, as briging to `UIColor` prior iOS 14 might not always work.
    
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
    ///     - backgroundColor: The background color of the view. Per defaut, `UIColor.clear` is used
    ///     - isRendering: Bindgin to disable or enable rendering. Disable it when the spine view is out of bounds and you want to preserve CPU/GPU resources.
    ///
    /// - Returns: A new instance of ``SpineView``.
    public init(
        from source: SpineViewSource,
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .clear,
        isRendering: Binding<Bool?> = .constant(nil)
    ) {
        self.source = source
        self.controller = controller
        self.mode = mode
        self.alignment = alignment
        self.boundsProvider = boundsProvider
        self.backgroundColor = backgroundColor
        _isRendering = isRendering
    }
    
    public func makeUIView(context: Context) -> SpineUIView {
        return SpineUIView(
            from: source,
            controller: controller,
            mode: mode,
            alignment: alignment,
            boundsProvider: boundsProvider,
            backgroundColor: backgroundColor
        )
    }
    
    public func updateUIView(_ uiView: SpineUIView, context: Context) {
        if let isRendering {
            uiView.isRendering = isRendering
        }
    }
}
