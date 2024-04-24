// The Swift Programming Language
// https://docs.swift.org/swift-book

import SpineSharedStructs
import SpineWrapper

public class Spine {
    
    public static var version: String {
        return "\(majorVersion).\(minorVersion)"
    }
    
    public static var majorVersion: Int32 {
        return spine_major_version()
    }
    
    public static var minorVersion: Int32 {
        return spine_minor_version()
    }
}
