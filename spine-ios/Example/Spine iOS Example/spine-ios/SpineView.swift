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
    
    public init(
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .white
    ) {
        self.atlasFile = nil
        self.skeletonFile = nil
        self.drawable = nil
        self.controller = controller
        self.mode = mode
        self.alignment = alignment
        self.boundsProvider = boundsProvider
    }
    
    public init(
        drawable: SkeletonDrawableWrapper,
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .white
    ) {
        self.atlasFile = nil
        self.skeletonFile = nil
        self.drawable = drawable
        self.controller = controller
        self.mode = mode
        self.alignment = alignment
        self.boundsProvider = boundsProvider
    }
    
    public init(
        atlasFile: String,
        skeletonFile: String,
        controller: SpineController = SpineController(),
        mode: Spine.ContentMode = .fit,
        alignment: Spine.Alignment = .center,
        boundsProvider: BoundsProvider = SetupPoseBounds(),
        backgroundColor: UIColor = .white
    ) {
        self.atlasFile = atlasFile
        self.skeletonFile = skeletonFile
        self.drawable = nil
        self.controller = controller
        self.mode = mode
        self.alignment = alignment
        self.boundsProvider = boundsProvider
    }
    
    public func makeUIView(context: Context) -> SpineUIView {
        if let atlasFile, let skeletonFile {
            return SpineUIView(
                atlasFile: atlasFile,
                skeletonFile: skeletonFile,
                controller: controller,
                mode: mode,
                alignment: alignment,
                boundsProvider: boundsProvider
            )
        } else if let drawable {
            return SpineUIView(
                drawable: drawable,
                controller: controller,
                mode: mode,
                alignment: alignment,
                boundsProvider: boundsProvider
            )
        } else {
            return SpineUIView(
                controller: controller,
                mode: mode,
                alignment: alignment,
                boundsProvider: boundsProvider
            )
        }
    }
    
    public func updateUIView(_ uiView: SpineUIView, context: Context) {
        // Stub
    }
}
