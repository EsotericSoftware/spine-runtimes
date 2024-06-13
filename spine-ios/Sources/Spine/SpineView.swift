import SwiftUI

public struct SpineView: UIViewRepresentable {
    
    public typealias UIViewType = SpineUIView

    private let source: SpineViewSource
    private let controller: SpineController
    private let mode: Spine.ContentMode
    private let alignment: Spine.Alignment
    private let boundsProvider: BoundsProvider
    private let backgroundColor: UIColor
    
    @Binding
    private var isRendering: Bool?
    
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
