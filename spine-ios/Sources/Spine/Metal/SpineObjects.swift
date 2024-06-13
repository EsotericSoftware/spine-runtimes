import Foundation
import MetalKit

/// Shared objects that live throughout applications lifecycle
///
/// Persistent Objects
/// https://developer.apple.com/library/archive/documentation/3DDrawing/Conceptual/MTLBestPracticesGuide/PersistentObjects.html#//apple_ref/doc/uid/TP40016642-CH3-SW1
final class SpineObjects {
    
    static let shared = SpineObjects()
    
    lazy var device: MTLDevice = {
        MTLCreateSystemDefaultDevice()!
    }()
    
    lazy var commandQueue: MTLCommandQueue = {
        device.makeCommandQueue()!
    }()
}
