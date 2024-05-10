//
//  PlayPause.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 08.05.24.
//

import SwiftUI

struct PlayPauseAnimation: View {
    @StateObject
    var controller: SpineController
    
    init() {
        _controller = StateObject(
            wrappedValue: SpineController { controller in
                controller.skeleton.scaleX = 0.2
                controller.skeleton.scaleY = 0.2
                
                _ = controller.animationState.setAnimationByName(
                    trackIndex: 0,
                    animationName: "flying",
                    loop: true
                )
            }
        )
    }
    
    var body: some View {
        SpineView(
            atlasFile: "dragon.atlas",
            skeletonFile: "dragon-ess.skel",
            controller: controller
        )
        .navigationTitle("Play/Pause")
        .navigationBarTitleDisplayMode(.inline)
        .toolbar {
            Button(action: {
                if controller.isPlaying {
                    controller.pause()
                } else {
                    controller.resume()
                }
            }) {
                Image(systemName: controller.isPlaying ? "pause.fill" : "play.fill")
            }
        }
    }
}

#Preview {
    PlayPauseAnimation()
}
