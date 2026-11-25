/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import Foundation
import SpineSwift
import SwiftUI

#if canImport(UIKit)
import UIKit
#elseif canImport(AppKit)
import AppKit
#endif

// Re-export version info from SpineSwift
public var version: String {
    return SpineSwift.version
}

public var majorVersion: Int {
    return SpineSwift.majorVersion
}

public var minorVersion: Int {
    return SpineSwift.minorVersion
}

/// ``Atlas`` data loaded from a `.atlas` file and its corresponding `.png` files. For each atlas image,
/// a corresponding `UIImage` is constructed, which is used when rendering a skeleton
/// that uses this atlas.
///
/// Use the static methods ``Atlas/fromBundle(_:bundle:)``, ``Atlas/fromFile(_:)``, and ``Atlas/fromHttp(_:)`` to load an atlas. Call ``Atlas/dispose()``
/// when the atlas is no longer in use to release its resources.
extension Atlas {

    /// Loads an ``Atlas`` from the file with name `atlasFileName` in the `main` bundle or the optionally provided [bundle].
    ///
    /// Throws an `Error` in case the atlas could not be loaded.
    public static func fromBundle(_ atlasFileName: String, bundle: Bundle = .main) async throws -> (Atlas, [SpineUIImage]) {
        let data = try await FileSource.bundle(fileName: atlasFileName, bundle: bundle).load()
        return try await Self.fromData(data: data) { name in
            return try await FileSource.bundle(fileName: name, bundle: bundle).load()
        }
    }

    /// Loads an ``Atlas`` from the file URL `atlasFile`.
    ///
    /// Throws an `Error` in case the atlas could not be loaded.
    public static func fromFile(_ atlasFile: URL) async throws -> (Atlas, [SpineUIImage]) {
        let data = try await FileSource.file(atlasFile).load()
        return try await Self.fromData(data: data) { name in
            let dir = atlasFile.deletingLastPathComponent()
            let file = dir.appendingPathComponent(name)
            return try await FileSource.file(file).load()
        }
    }

    /// Loads an ``Atlas`` from the http URL `atlasURL`.
    ///
    /// Throws an `Error` in case the atlas could not be loaded.
    public static func fromHttp(_ atlasURL: URL) async throws -> (Atlas, [SpineUIImage]) {
        let data = try await FileSource.http(atlasURL).load()
        return try await Self.fromData(data: data) { name in
            let dir = atlasURL.deletingLastPathComponent()
            let file = dir.appendingPathComponent(name)
            return try await FileSource.http(file).load()
        }
    }

    private static func fromData(data: Data, loadFile: (_ name: String) async throws -> Data) async throws -> (Atlas, [SpineUIImage]) {
        guard let atlasData = String(data: data, encoding: .utf8) else {
            throw SpineError("Couldn't read atlas bytes as utf8 string")
        }

        // Use SpineSwift's loadAtlas function
        let atlas = try loadAtlas(atlasData)

        var atlasPages = [SpineUIImage]()

        // Load images for each atlas page
        let pages = atlas.pages
        for i in 0..<pages.count {
            guard let page = pages[i] else { continue }
            let imagePath = page.texturePath

            let imageData = try await loadFile(imagePath)
            guard let image = SpineUIImage(data: imageData) else {
                continue
            }
            atlasPages.append(image)
        }

        return (atlas, atlasPages)
    }
}

extension SkeletonData {

    /// Loads a ``SkeletonData`` from the file with name `skeletonFileName` in the main bundle or the optionally provided `bundle`.
    /// Uses the provided ``Atlas`` to resolve attachment images.
    ///
    /// Throws an `Error` in case the skeleton data could not be loaded.
    public static func fromBundle(atlas: Atlas, skeletonFileName: String, bundle: Bundle = .main) async throws -> SkeletonData {
        return try fromData(
            atlas: atlas,
            data: try await FileSource.bundle(fileName: skeletonFileName, bundle: bundle).load(),
            isJson: skeletonFileName.hasSuffix(".json"),
            path: skeletonFileName
        )
    }

    /// Loads a ``SkeletonData`` from the file URL `skeletonFile`. Uses the provided ``Atlas`` to resolve attachment images.
    ///
    /// Throws an `Error` in case the skeleton data could not be loaded.
    public static func fromFile(atlas: Atlas, skeletonFile: URL) async throws -> SkeletonData {
        return try fromData(
            atlas: atlas,
            data: try await FileSource.file(skeletonFile).load(),
            isJson: skeletonFile.absoluteString.hasSuffix(".json"),
            path: skeletonFile.absoluteString
        )
    }

    /// Loads a ``SkeletonData`` from the http URL `skeletonFile`. Uses the provided ``Atlas`` to resolve attachment images.
    ///
    /// Throws an `Error` in case the skeleton data could not be loaded.
    public static func fromHttp(atlas: Atlas, skeletonURL: URL) async throws -> SkeletonData {
        return try fromData(
            atlas: atlas,
            data: try await FileSource.http(skeletonURL).load(),
            isJson: skeletonURL.absoluteString.hasSuffix(".json"),
            path: skeletonURL.absoluteString
        )
    }

    private static func fromData(atlas: Atlas, data: Data, isJson: Bool, path: String) throws -> SkeletonData {
        if isJson {
            guard let json = String(data: data, encoding: .utf8) else {
                throw SpineError("Couldn't read skeleton data json string")
            }
            return try loadSkeletonDataJson(atlas: atlas, jsonData: json, path: path)
        } else {
            return try loadSkeletonDataBinary(atlas: atlas, binaryData: data, path: path)
        }
    }
}

// Helper

extension CGRect {

    /// Construct a `CGRect` from ``Bounds``
    public init(bounds: Bounds) {
        self = CGRect(
            x: CGFloat(bounds.x),
            y: CGFloat(bounds.y),
            width: CGFloat(bounds.width),
            height: CGFloat(bounds.height)
        )
    }
}

internal enum FileSource {
    case bundle(fileName: String, bundle: Bundle = .main)
    case file(URL)
    case http(URL)

    internal func load() async throws -> Data {
        switch self {
        case .bundle(let fileName, let bundle):
            let components = fileName.split(separator: ".")
            guard components.count > 1, let ext = components.last else {
                throw SpineError("Provide both file name and file extension")
            }
            let name = components.dropLast(1).joined(separator: ".")

            guard let fileUrl = bundle.url(forResource: name, withExtension: String(ext)) else {
                throw SpineError("Could not load file with name \(name) from bundle")
            }
            return try Data(contentsOf: fileUrl, options: [])
        case .file(let fileUrl):
            return try Data(contentsOf: fileUrl, options: [])
        case .http(let url):
            if #available(iOS 15.0, macOS 12.0, macCatalyst 15.0, tvOS 15.0, *) {
                let (temp, response) = try await URLSession.shared.download(from: url)
                guard let httpResponse = response as? HTTPURLResponse, httpResponse.statusCode == 200 else {
                    throw URLError(.badServerResponse)
                }
                return try Data(contentsOf: temp, options: [])
            } else {
                let lock = NSRecursiveLock()
                nonisolated(unsafe)
                    var isCancelled = false
                nonisolated(unsafe)
                    var taskHolder: URLSessionDownloadTask? = nil
                return try await withTaskCancellationHandler {
                    try await withCheckedThrowingContinuation { continuation in
                        let task = URLSession.shared.downloadTask(with: url) { temp, response, error in
                            if let error {
                                continuation.resume(throwing: error)
                            } else {
                                guard let httpResponse = response as? HTTPURLResponse, httpResponse.statusCode == 200 else {
                                    continuation.resume(throwing: URLError(.badServerResponse))
                                    return
                                }
                                guard let temp else {
                                    continuation.resume(throwing: SpineError("Could not download file."))
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
                        let shouldCancel = lock.withLock {
                            if !isCancelled {
                                taskHolder = task
                            }
                            return isCancelled
                        }
                        if shouldCancel {
                            task.cancel()
                        }
                    }
                } onCancel: {
                    lock.withLock {
                        isCancelled = true
                        let value = taskHolder
                        taskHolder = nil
                        return value
                    }?.cancel()
                }
            }
        }
    }
}

// Re-export SpineError from SpineSwift
public typealias SpineError = SpineSwift.SpineError
