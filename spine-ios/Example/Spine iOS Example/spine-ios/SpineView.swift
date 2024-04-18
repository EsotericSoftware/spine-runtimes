//
//  File.swift
//  
//
//  Created by Denis Andrašec on 17.04.24.
//

import SwiftUI

public struct SpineView: UIViewControllerRepresentable {
    public typealias UIViewControllerType = SpineViewController
    
    public let mesh: String
    public let bundle: Bundle
    
    public init(mesh: String, bundle: Bundle = .main) {
        self.mesh = mesh
        self.bundle = bundle
    }
    
    public func makeUIViewController(context: Context) -> SpineViewController {
        return SpineViewController(mesh: mesh, bundle: bundle)
    }
    
    public func updateUIViewController(_ uiViewController: SpineViewController, context: Context) {
        //
    }
}
