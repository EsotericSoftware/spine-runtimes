//
//  SpineViewController.swift
//
//
//  Created by Denis Andrašec on 17.04.24.
//

import UIKit
import MetalKit
import Spine

public final class SpineViewController: UIViewController {
    
    private var renderer: SpineRenderer?
    
    private var mtkView: MTKView {
        return view as! MTKView
    }
    
    private let atlasFile: String
    private let skeletonFile: String
    private let controller: SpineController
    
    private let contentMode: Spine.ContentMode
    private let alignment: Spine.Alignment
    private let boundsProvider: BoundsProvider
    
    public init(
        atlasFile: String,
        skeletonFile: String,
        controller: SpineController,
        contentMode: Spine.ContentMode? = nil,
        alignment: Spine.Alignment? = nil,
        boundsProvider: BoundsProvider? = nil
    ) {
        self.atlasFile = atlasFile
        self.skeletonFile = skeletonFile
        self.controller = controller
        
        self.contentMode = contentMode ?? .fit
        self.alignment = alignment ?? .center
        self.boundsProvider = boundsProvider ?? SetupPoseBounds()
        
        super.init(nibName: nil, bundle: nil)
    }
    
    public required init?(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }
    
    public override func loadView() {
        view = MTKView()
    }
    
    public override func viewDidLoad() {
        super.viewDidLoad()
        
        mtkView.device = MTLCreateSystemDefaultDevice()
        mtkView.clearColor = MTLClearColor(red: 0.2, green: 0.2, blue: 0.2, alpha: 1.0)
        
        load()
    }
    
    private func load() {
        Task.detached(priority: .high) {
            try await self.controller.load(
                atlasFile: self.atlasFile,
                skeletonFile: self.skeletonFile
            )
            await MainActor.run {
                self.initRenderer(
                    atlasPages: self.controller.drawable.atlasPages
                )
                self.controller.initialize()
            }
        }
    }
    
    private func initRenderer(atlasPages: [CGImage]) {
        do {
            renderer = try SpineRenderer(
                mtkView: mtkView,
                atlasPages: atlasPages,
                contentMode: contentMode,
                alignment: alignment,
                boundsProvider: boundsProvider
            )
            renderer?.delegate = controller
            renderer?.dataSource = controller
            
            renderer?.mtkView(mtkView, drawableSizeWillChange: mtkView.drawableSize)
            
            mtkView.delegate = renderer
        } catch {
            print(error)
        }
    }
}
