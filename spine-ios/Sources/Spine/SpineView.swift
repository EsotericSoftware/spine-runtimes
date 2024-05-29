import SwiftUI

public struct SpineView: UIViewRepresentable {
    
    public typealias UIViewType = SpineUIView

    private let source: SpineViewSource
    private let controller: SpineController
    private let mode: Spine.ContentMode
    private let alignment: Spine.Alignment
    private let boundsProvider: BoundsProvider
    private let backgroundColor: UIColor
    
    public init(
        from source: SpineViewSource,
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .clear
    ) {
        self.source = source
        self.controller = controller
        self.mode = mode
        self.alignment = alignment
        self.boundsProvider = boundsProvider
        self.backgroundColor = backgroundColor
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
        // Stub
    }
}
