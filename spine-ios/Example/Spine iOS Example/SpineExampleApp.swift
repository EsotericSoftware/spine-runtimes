//
//  Spine_iOS_ExampleApp.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 17.04.24.
//

import SwiftUI
import Spine

@main
struct SpineExampleApp: App {
    
    init() {
        let version = Spine.version
        print("Spine \(version)")
        
        Task {
            do {
                // Load
                
                let atlas = try await Atlas.fromAsset("spineboy.atlas")
                print(atlas)
                
                let skeletonDataFromJson = try SkeletonData.fromAsset(
                    atlas: atlas.0,
                    skeletonFile: "spineboy-pro.json"
                )
                print(skeletonDataFromJson)
                
                let skeletonDataFromBinary = try SkeletonData.fromAsset(
                    atlas: atlas.0,
                    skeletonFile: "spineboy-pro.skel"
                )
                print(skeletonDataFromBinary)
                
                // Dispose
                
                atlas.0.dispose()
                skeletonDataFromJson.dispose()
                skeletonDataFromBinary.dispose()
            } catch {
                print(error)
            }
        }
    }
    
    var body: some Scene {
        WindowGroup {
            ContentView()
        }
    }
}
