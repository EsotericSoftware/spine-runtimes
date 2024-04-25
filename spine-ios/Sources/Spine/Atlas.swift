import SpineWrapper
import SwiftUI
import Foundation

/// Atlas data loaded from a `.atlas` file and its corresponding `.png` files. For each atlas image,
/// a corresponding [Image] and [Paint] is constructed, which are used when rendering a skeleton
/// that uses this atlas.
///
/// Use the static methods [fromAsset], [fromFile], and [fromHttp] to load an atlas. Call [dispose]
/// when the atlas is no longer in use to release its resources.
public final class Atlas {
    internal let atlas: spine_atlas
    private let atlasPages: [Image]
    private var disposed = false
    
    private init(atlas: spine_atlas, atlasPages: [Image]) {
        self.atlas = atlas
        self.atlasPages = atlasPages
    }
    
    private static func load(atlasFileName: String, loadFile: (_ name: String) async throws -> Data) async throws -> Atlas {
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
        
        return Atlas(atlas: atlas, atlasPages: atlasPages)
    }
    
    /// Loads an [Atlas] from the file [atlasFileName] in the main bundle or the optionally provided [bundle].
    ///
    /// Throws an [Exception] in case the atlas could not be loaded.
    public static func fromAsset(_ atlasFileName: String, bundle: Bundle = .main) async throws -> Atlas {
        return try await Self.load(atlasFileName: atlasFileName) { name in
            return try bundle.loadAsData(fileName: atlasFileName)
        }
    }
    
    /// Loads an [Atlas] from the file [atlasFileName].
    ///
    /// Throws an [Exception] in case the atlas could not be loaded.
    public static func fromFile(_ atlasFileName: String) async throws -> Atlas {
        throw "Not implemented"
    }
        
    /// Loads an [Atlas] from the URL [atlasURL].
    ///
    /// Throws an [Exception] in case the atlas could not be loaded.
    public static func fromHttp(_ atlasURL: String) async throws -> Atlas {
        throw "Not implemented"
    }
    
    /// Disposes the (native) resources of this atlas. The atlas can no longer be
    /// used after calling this function. Only the first call to this method will
    /// have an effect. Subsequent calls are ignored.
    public func dispose() {
        if disposed { return }
        disposed = true
        spine_atlas_dispose(atlas)
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
