import SpineWrapper
import SwiftUI
import Foundation

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

/// Atlas data loaded from a `.atlas` file and its corresponding `.png` files. For each atlas image,
/// a corresponding [Image] and [Paint] is constructed, which are used when rendering a skeleton
/// that uses this atlas.
///
/// Use the static methods [fromAsset], [fromFile], and [fromHttp] to load an atlas. Call [dispose]
/// when the atlas is no longer in use to release its resources.
public extension Atlas {
    
    private static func load(atlasFileName: String, loadFile: (_ name: String) async throws -> Data) async throws -> (Atlas, [Image]) {
        let atlasBytes = try await loadFile(atlasFileName)
        guard let atlasData = String(data: atlasBytes, encoding: .utf8) as? NSString else {
            throw "Couldn't read atlas bytes"
        }
        
        let atlasDataNative = UnsafeMutablePointer<CChar>(mutating: atlasData.utf8String)
        guard let atlas = spine_atlas_load(atlasDataNative) else {
            throw "Couldn't load atlas data"
        }
        
        if let error = spine_atlas_get_error(atlas) {
            let message = String(cString: error)
            spine_atlas_dispose(atlas)
            throw "Couldn't load atlas: \(message)"
        }
        
        var atlasPages = [Image]()
        let numImagePaths = spine_atlas_get_num_image_paths(atlas);
        
        for i in 0..<numImagePaths {
            guard let atlasPageFilePointer = spine_atlas_get_image_path(atlas, i) else {
                continue
            }
            let atlasPageFile = String(cString: atlasPageFilePointer)
            let imageData = try await loadFile(atlasPageFile)
            guard let image = UIImage(data: imageData) else {
                continue
            }
            atlasPages.append(Image(uiImage: image))
        }
        
        // TODO: Paint in Swift?
        
        return (Atlas(atlas), atlasPages)
    }
    
    /// Loads an [Atlas] from the file [atlasFileName] in the main bundle or the optionally provided [bundle].
    ///
    /// Throws an [Exception] in case the atlas could not be loaded.
    public static func fromAsset(_ atlasFileName: String, bundle: Bundle = .main) async throws -> (Atlas, [Image]) {
        return try await Self.load(atlasFileName: atlasFileName) { name in
            return try bundle.loadAsData(fileName: atlasFileName)
        }
    }
    
    /// Loads an [Atlas] from the file [atlasFileName].
    ///
    /// Throws an [Exception] in case the atlas could not be loaded.
    public static func fromFile(_ atlasFileName: String) async throws -> (Atlas, [Image]) {
        throw "Not implemented"
    }
        
    /// Loads an [Atlas] from the URL [atlasURL].
    ///
    /// Throws an [Exception] in case the atlas could not be loaded.
    public static func fromHttp(_ atlasURL: String) async throws -> (Atlas, [Image]) {
        throw "Not implemented"
    }
}

public extension SkeletonData {
    
    /// Loads a [SkeletonData] from the [json] string, using the provided [atlas] to resolve attachment
    /// images.
    ///
    /// Throws an [Exception] in case the atlas could not be loaded.
    public static func fromJson(atlas: Atlas, json: String) throws -> SkeletonData {
        let jsonNative = UnsafeMutablePointer<CChar>(mutating: (json as NSString).utf8String)
        guard let result = spine_skeleton_data_load_json(atlas.wrappee, jsonNative) else {
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
        return SkeletonData(data)
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
            atlas.wrappee,
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
        return SkeletonData(data)
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
}

// Helper

extension Bundle {
    func loadFileUrl(fileName: String) throws -> URL {
        let components = fileName.split(separator: ".")
        guard components.count > 1, let ext = components.last else {
            throw "Provide both file name and file extension"
        }
        let name = components.dropLast(1).joined(separator: ".")
        
        guard let fileUrl = url(forResource: name, withExtension: String(ext)) else {
            throw "Could not load file with name \(name)"
        }
        return fileUrl
    }
    
    func loadAsData(fileName: String) throws -> Data {
        let fileUrl = try loadFileUrl(fileName: fileName)
        return try Data(contentsOf: fileUrl, options: [])
    }
}

extension String: Error {
    
}
