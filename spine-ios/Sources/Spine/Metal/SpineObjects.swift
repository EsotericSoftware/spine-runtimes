import Foundation
import MetalKit

/// Shared objects that live throughout applications lifecycle
///
/// Persistent Objects
/// https://developer.apple.com/library/archive/documentation/3DDrawing/Conceptual/MTLBestPracticesGuide/PersistentObjects.html#//apple_ref/doc/uid/TP40016642-CH3-SW1
internal final class SpineObjects {
    
    static let shared = SpineObjects()
    
    internal lazy var device: MTLDevice = {
        MTLCreateSystemDefaultDevice()!
    }()
    
    internal lazy var commandQueue: MTLCommandQueue = {
        device.makeCommandQueue()!
    }()
}
