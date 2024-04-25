import Foundation
import SpineWrapper

/// Skeleton data loaded from a skeleton `.json` or `.skel` file. Contains bones, slots, constraints,
/// skins, animations, and so on making up a skeleton. Also contains meta data such as the skeletons
/// setup pose bounding box, the Spine editor version it was exported from, and so on.
///
/// Skeleton data is stateless. Stateful [Skeleton] instances can be constructed from a [SkeletonData] instance.
/// A single [SkeletonData] instance can be shared by multiple [Skeleton] instances.
///
/// Use the static methods [fromJson], [fromBinary], [fromAsset], [fromFile], and [fromURL] to load
/// skeleton data. Call [dispose] when the skeleton data is no longer in use to free its resources.
///
/// See [Data objects](http://esotericsoftware.com/spine-runtime-architecture#Data-objects) in the Spine
/// Runtimes Guide.
public final class SkeletonData {
    internal let data: spine_skeleton_data
    private var disposed = false
    
    private init(data: spine_skeleton_data) {
        self.data = data
    }
    
    /// Loads a [SkeletonData] from the [json] string, using the provided [atlas] to resolve attachment
    /// images.
    ///
    /// Throws an [Exception] in case the atlas could not be loaded.
    public static func fromJson(atlas: Atlas, json: String) throws -> SkeletonData {
        let jsonNative = UnsafeMutablePointer<CChar>(mutating: (json as NSString).utf8String)
        guard let result = spine_skeleton_data_load_json(atlas.atlas, jsonNative) else {
            throw "Couldn't load skeleton data json"
        }
        if let error = spine_skeleton_data_result_get_error(result) {
            let message = String(cString: error)
            spine_skeleton_data_result_dispose(result)
            throw "Couldn't load skeleton data: \(message)"
        }
        guard let data = spine_skeleton_data_result_get_data(result) else {
            throw "Couldn't load skeleton data from result"
        }
        spine_skeleton_data_result_dispose(result)
        return SkeletonData(data: data)
    }
    
    /// Loads a [SkeletonData] from the [binary] skeleton data, using the provided [atlas] to resolve attachment
    /// images.
    ///
    /// Throws an [Exception] in case the skeleton data could not be loaded.
    public static func fromBinary(atlas: Atlas, binary: Data) throws -> SkeletonData {
        let binaryNative = try binary.withUnsafeBytes { unsafeBytes in
            guard let bytes = unsafeBytes.bindMemory(to: UInt8.self).baseAddress else {
                throw "Couldn't read atlas binary"
            }
            return (data: bytes, length: Int32(unsafeBytes.count))
        }
        let result = spine_skeleton_data_load_binary(
            atlas.atlas,
            binaryNative.data,
            binaryNative.length
        )
        if let error = spine_skeleton_data_result_get_error(result) {
            let message = String(cString: error)
            spine_skeleton_data_result_dispose(result)
            throw "Couldn't load skeleton data: \(message)"
        }
        guard let data = spine_skeleton_data_result_get_data(result) else {
            throw "Couldn't load skeleton data from result"
        }
        spine_skeleton_data_result_dispose(result)
        return SkeletonData(data: data)
    }
    
    /// Loads a [SkeletonData] from the file [skeletonFile] in the main bundle or the optionally provided [bundle].
    /// Uses the provided [atlas] to resolve attachment images.
    ///
    /// Throws an [Exception] in case the skeleton data could not be loaded.
    public static func fromAsset(atlas: Atlas, skeletonFile: String, bundle: Bundle = .main) throws -> SkeletonData {
        let data = try bundle.loadAsData(fileName: skeletonFile)
        if skeletonFile.hasSuffix(".json") {
            guard let json = String(data: data, encoding: .utf8) else {
                throw "Couldn't read skeleton data json string"
            }
            return try fromJson(atlas: atlas, json: json)
        } else {
            return try fromBinary(atlas: atlas, binary: data)
        }
    }
    
    /// Loads a [SkeletonData] from the file [skeletonFile]. Uses the provided [atlas] to resolve attachment images.
    ///
    /// Throws an [Exception] in case the skeleton data could not be loaded.
    public static func fromFile(atlas: Atlas, skeletonFile: String, bundle: Bundle = .main) throws -> SkeletonData {
        throw "Not implemented"
    }
    
    /// Loads a [SkeletonData] from the URL [skeletonURL]. Uses the provided [atlas] to resolve attachment images.
    ///
    /// Throws an [Exception] in case the skeleton data could not be loaded.
    public static func fromHttp(atlas: Atlas, skeletonURL: URL) throws -> SkeletonData {
        throw "Not implemented"
    }
    
    /// Disposes the (native) resources of this skeleton data. The skeleton data can no longer be
    /// used after calling this function. Only the first call to this method will
    /// have an effect. Subsequent calls are ignored.
    public func dispose() {
        if disposed { return }
        disposed = true;
        spine_skeleton_data_dispose(data);
    }
}
