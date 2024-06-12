import Foundation
import SwiftUI
import SpineCppLite

public var version: String {
    return "\(majorVersion).\(minorVersion)"
}

public var majorVersion: Int {
    return Int(spine_major_version())
}

public var minorVersion: Int {
    return Int(spine_minor_version())
}

/// Atlas data loaded from a `.atlas` file and its corresponding `.png` files. For each atlas image,
/// a corresponding [Image] and [Paint] is constructed, which are used when rendering a skeleton
/// that uses this atlas.
///
/// Use the static methods [fromAsset], [fromFile], and [fromHttp] to load an atlas. Call [dispose]
/// when the atlas is no longer in use to release its resources.
public extension Atlas {
    
    static func fromData(data: Data, loadFile: (_ name: String) async throws -> Data) async throws -> (Atlas, [UIImage]) {
        guard let atlasData = String(data: data, encoding: .utf8) as? NSString else {
            throw "Couldn't read atlas bytes as utf8 string"
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
        
        var atlasPages = [UIImage]()
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
            atlasPages.append(image)
        }
        
        return (Atlas(atlas), atlasPages)
    }
    
    /// Loads an [Atlas] from the file [atlasFileName] in the main bundle or the optionally provided [bundle].
    ///
    /// Throws an [Exception] in case the atlas could not be loaded.
    static func fromBundle(_ atlasFileName: String, bundle: Bundle = .main) async throws -> (Atlas, [UIImage]) {
        let data = try await FileSource.bundle(fileName: atlasFileName, bundle: bundle).load()
        return try await Self.fromData(data: data) { name in
            return try await FileSource.bundle(fileName: name, bundle: bundle).load()
        }
    }
    
    /// Loads an [Atlas] from the file [atlasFileName].
    ///
    /// Throws an [Exception] in case the atlas could not be loaded.
    static func fromFile(_ atlasFile: URL) async throws -> (Atlas, [UIImage]) {
        let data = try await FileSource.file(atlasFile).load()
        return try await Self.fromData(data: data) { name in
            let dir = atlasFile.deletingLastPathComponent()
            let file = dir.appendingPathComponent(name)
            return try await FileSource.file(file).load()
        }
    }
        
    /// Loads an [Atlas] from the URL [atlasURL].
    ///
    /// Throws an [Exception] in case the atlas could not be loaded.
    static func fromHttp(_ atlasURL: URL) async throws -> (Atlas, [UIImage]) {
        let data = try await FileSource.http(atlasURL).load()
        return try await Self.fromData(data: data) { name in
            let dir = atlasURL.deletingLastPathComponent()
            let file = dir.appendingPathComponent(name)
            return try await FileSource.http(file).load()
        }
    }
}

public extension SkeletonData {
    
    /// Loads a [SkeletonData] from the [json] string, using the provided [atlas] to resolve attachment
    /// images.
    ///
    /// Throws an [Exception] in case the atlas could not be loaded.
    static func fromJson(atlas: Atlas, json: String) throws -> SkeletonData {
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
    static func fromBinary(atlas: Atlas, binary: Data) throws -> SkeletonData {
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
    static func fromBundle(atlas: Atlas, skeletonFileName: String, bundle: Bundle = .main) async throws -> SkeletonData {
        return try fromBinary(
            atlas: atlas,
            binary: try await FileSource.bundle(fileName: skeletonFileName, bundle: bundle).load(),
            isJson: skeletonFileName.hasSuffix(".json")
        )
    }
    
    /// Loads a [SkeletonData] from the file [skeletonFile]. Uses the provided [atlas] to resolve attachment images.
    ///
    /// Throws an [Exception] in case the skeleton data could not be loaded.
    static func fromFile(atlas: Atlas, skeletonFile: URL) async throws -> SkeletonData {
        return try fromBinary(
            atlas: atlas,
            binary: try await FileSource.file(skeletonFile).load(),
            isJson: skeletonFile.absoluteString.hasSuffix(".json")
        )
    }
    
    /// Loads a [SkeletonData] from the URL [skeletonURL]. Uses the provided [atlas] to resolve attachment images.
    ///
    /// Throws an [Exception] in case the skeleton data could not be loaded.
    static func fromHttp(atlas: Atlas, skeletonURL: URL) async throws -> SkeletonData {
        return try fromBinary(
            atlas: atlas,
            binary: try await FileSource.http(skeletonURL).load(),
            isJson: skeletonURL.absoluteString.hasSuffix(".json")
        )
    }
    
    private static func fromBinary(atlas: Atlas, binary: Data, isJson: Bool) throws -> SkeletonData {
        if isJson {
            guard let json = String(data: binary, encoding: .utf8) else {
                throw "Couldn't read skeleton data json string"
            }
            return try fromJson(atlas: atlas, json: json)
        } else {
            return try fromBinary(atlas: atlas, binary: binary)
        }
    }
}

public extension SkeletonDrawable {
    
    func render() -> [RenderCommand] {
        var commands = [RenderCommand]()
        if disposed { return commands }
        
        var nativeCmd = spine_skeleton_drawable_render(wrappee)
        repeat {
            if let ncmd = nativeCmd {
                commands.append(RenderCommand(ncmd))
                nativeCmd = spine_render_command_get_next(ncmd)
            } else {
                nativeCmd = nil
            }
        } while (nativeCmd != nil)
        
        return commands
    }
}

public extension RenderCommand {
    
    var numVertices: Int {
        Int(spine_render_command_get_num_vertices(wrappee))
    }
    
    var positions: [Float] {
        let num = numVertices * 2
        let ptr = spine_render_command_get_positions(wrappee)
        return (0..<num).compactMap { ptr?[$0] }
    }
    
    var uvs: [Float] {
        let num = numVertices * 2
        let ptr = spine_render_command_get_uvs(wrappee)
        return (0..<num).compactMap { ptr?[$0] }
    }
    
    var colors: [Int32] {
        let num = numVertices
        let ptr = spine_render_command_get_colors(wrappee)
        return (0..<num).compactMap { ptr?[$0] }
    }
}

public extension Skin {
    static func create(name: String) -> Skin {
        return Skin(spine_skin_create(name))
    }
}

// Helper

extension CGRect {
    init(bounds: Bounds) {
        self = CGRect(
            x: CGFloat(bounds.x),
            y: CGFloat(bounds.y),
            width: CGFloat(bounds.width),
            height: CGFloat(bounds.height)
        )
    }
}

enum FileSource {
    case bundle(fileName: String, bundle: Bundle = .main)
    case file(URL)
    case http(URL)
    
    internal func load() async throws -> Data {
        switch self {
        case .bundle(let fileName, let bundle):
            let components = fileName.split(separator: ".")
            guard components.count > 1, let ext = components.last else {
                throw "Provide both file name and file extension"
            }
            let name = components.dropLast(1).joined(separator: ".")
            
            guard let fileUrl = bundle.url(forResource: name, withExtension: String(ext)) else {
                throw "Could not load file with name \(name) from bundle"
            }
            return try Data(contentsOf: fileUrl, options: [])
        case .file(let fileUrl):
            return try Data(contentsOf: fileUrl, options: [])
        case .http(let url):
            if #available(iOS 15.0, *) {
                let (temp, response) = try await URLSession.shared.download(from: url)
                guard let httpResponse = response as? HTTPURLResponse, httpResponse.statusCode == 200 else {
                    throw URLError(.badServerResponse)
                }
                return try Data(contentsOf: temp, options: [])
            } else {
                return try await withCheckedThrowingContinuation { continuation in
                    let task = URLSession.shared.downloadTask(with: url) { temp, response, error in
                        if let error {
                            continuation.resume(throwing: error)
                        } else {
                            guard let httpResponse = response as? HTTPURLResponse, httpResponse.statusCode == 200 else {
                                continuation.resume(throwing: URLError(.badServerResponse))
                                return
                            }
                            guard let temp else {
                                continuation.resume(throwing: "Could not download file.")
                                return
                            }
                            do {
                                continuation.resume(returning: try Data(contentsOf: temp, options: []))
                            } catch {
                                continuation.resume(throwing: error)
                            }
                        }
                    }
                    task.resume()
                }
            }
        }
    }
}

extension String: Error {
    
}
