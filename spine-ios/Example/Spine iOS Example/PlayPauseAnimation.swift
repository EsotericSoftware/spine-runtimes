//
//  PlayPause.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 08.05.24.
//

import SwiftUI
import Spine

struct PlayPauseAnimation: View {
    
    @StateObject
    var controller = SpineController(
        onInitialized: { controller in
            controller.animationState.setAnimationByName(
                trackIndex: 0,
                animationName: "flying",
                loop: true
            )
        }
    )
    
    var body: some View {
        SpineView(
            atlasFile: "dragon.atlas",
            skeletonFile: "dragon-ess.skel",
            controller: controller,
            boundsProvider: SkinAndAnimationBounds(animation: "flying")
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
