//
//  SpinePlatformTypes+AppKit.swift
//  spine-ios
//
//  Created by Gyuhwan Park on 5/22/26.
//

#if canImport(AppKit)
import AppKit
import SwiftUI

public typealias SpineUIImage = NSImage
public typealias SpineUIColor = NSColor
public typealias SpineUIScreen = NSScreen

extension SpineUIScreen {
    // UIKit-compatible accessor
    var scale: CGFloat {
        backingScaleFactor
    }
}
#endif
