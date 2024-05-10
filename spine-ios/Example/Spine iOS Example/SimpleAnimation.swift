//
//  ContentView.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 17.04.24.
//

import SwiftUI
import Spine

struct SimpleAnimation: View {
    
    @StateObject
    var controller: SpineController
    
    init() {
        _controller = StateObject(
            wrappedValue: SpineController { controller in
                controller.skeleton.scaleX = 0.2
                controller.skeleton.scaleY = 0.2
                _ = controller.animationState.setAnimationByName(
                    trackIndex: 0,
                    animationName: "walk",
                    loop: true
                )
            }
        )
    }
    
    var body: some View {
        SpineView(
            atlasFile: "spineboy.atlas",
            skeletonFile: "spineboy-pro.skel",
            controller: controller
        )
        .navigationTitle("Simple Animation")
        .navigationBarTitleDisplayMode(.inline)
    }
}

#Preview {
    SimpleAnimation()
}
