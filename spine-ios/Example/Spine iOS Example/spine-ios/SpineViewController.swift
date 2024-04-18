//
//  SpineViewController.swift
//
//
//  Created by Denis Andrašec on 17.04.24.
//

import UIKit
import MetalKit

public final class SpineViewController: UIViewController {
    
    private let meshURL: URL
    private let imageURL: URL
    
    private var renderer: SpineRenderer?
    private var mtkView: MTKView {
        return view as! MTKView
    }
    
    public init(mesh name: String, bundle: Bundle = .main) {
        meshURL = bundle.url(forResource: name, withExtension: "mesh")!
        imageURL = bundle.url(forResource: name, withExtension: "png")!
        
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
        
        do {
            let renderCommand = RenderCommand(
                mesh: try String(contentsOf: meshURL, encoding: .utf8),
                blendMode: .normal,
                premultipliedAlpha: true
            )
            renderer = try SpineRenderer(mtkView: mtkView, renderCommand: renderCommand, imageURL: imageURL)
            renderer?.mtkView(mtkView, drawableSizeWillChange: mtkView.drawableSize)
            mtkView.delegate = renderer;
        } catch {
            print(error)
        }
    }
}
