//
//  File.swift
//  
//
//  Created by Denis Andrašec on 17.04.24.
//

import SwiftUI
import Spine

public struct SpineView: UIViewRepresentable {
    
    public typealias UIViewType = SpineUIView

    private let atlasFile: String?
    private let skeletonFile: String?
    private let drawable: SkeletonDrawableWrapper?
    
    private let controller: SpineController
    private let mode: Spine.ContentMode
    private let alignment: Spine.Alignment
    private let boundsProvider: BoundsProvider
    private let backgroundColor: UIColor
    
    public init(
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .clear
    ) {
        self.atlasFile = nil
        self.skeletonFile = nil
        self.drawable = nil
        self.controller = controller
        self.mode = mode
        self.alignment = alignment
        self.boundsProvider = boundsProvider
        self.backgroundColor = backgroundColor
    }
    
    public init(
        drawable: SkeletonDrawableWrapper,
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .clear
    ) {
        self.atlasFile = nil
        self.skeletonFile = nil
        self.drawable = drawable
        self.controller = controller
        self.mode = mode
        self.alignment = alignment
        self.boundsProvider = boundsProvider
        self.backgroundColor = backgroundColor
    }
    
    public init(
        atlasFile: String,
        skeletonFile: String,
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .clear
    ) {
        self.atlasFile = atlasFile
        self.skeletonFile = skeletonFile
        self.drawable = nil
        self.controller = controller
        self.mode = mode
        self.alignment = alignment
        self.boundsProvider = boundsProvider
        self.backgroundColor = backgroundColor
    }
    
    public func makeUIView(context: Context) -> SpineUIView {
        if let atlasFile, let skeletonFile {
            return SpineUIView(
                atlasFile: atlasFile,
                skeletonFile: skeletonFile,
                controller: controller,
                mode: mode,
                alignment: alignment,
                boundsProvider: boundsProvider,
                backgroundColor: backgroundColor
            )
        } else if let drawable {
            return SpineUIView(
                drawable: drawable,
                controller: controller,
                mode: mode,
                alignment: alignment,
                boundsProvider: boundsProvider,
                backgroundColor: backgroundColor
            )
        } else {
            return SpineUIView(
                controller: controller,
                mode: mode,
                alignment: alignment,
                boundsProvider: boundsProvider,
                backgroundColor: backgroundColor
            )
        }
    }
    
    public func updateUIView(_ uiView: SpineUIView, context: Context) {
        // Stub
    }
}
