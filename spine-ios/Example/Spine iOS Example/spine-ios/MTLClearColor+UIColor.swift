//
//  MTLClearColor+Color.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 16.05.24.
//

import UIKit
import MetalKit

extension MTLClearColor {
    init(_ color: UIColor) {
        // Variables to hold the RGBA components
        var red: CGFloat = 0
        var green: CGFloat = 0
        var blue: CGFloat = 0
        var alpha: CGFloat = 0
        
        // Get the RGBA components from UIColor
        color.getRed(&red, green: &green, blue: &blue, alpha: &alpha)
        
        // Initialize MTLClearColor with the RGBA values
        self.init(red: Double(red), green: Double(green), blue: Double(blue), alpha: Double(alpha))
    }
}
