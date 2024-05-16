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
            wrappedValue: SpineController(onInitialized: { controller in
                controller.animationState.setAnimationByName(
                    trackIndex: 0,
                    animationName: "walk",
                    loop: true
                )
            })
        )
    }
    
    var body: some View {
        SpineView(
            atlasFile: "spineboy.atlas",
            skeletonFile: "spineboy-pro.skel",
            controller: controller,
            contentMode: .fit,
            alignment: .center
        )
        .navigationTitle("Simple Animation")
        .navigationBarTitleDisplayMode(.inline)
    }
}

#Preview {
    SimpleAnimation()
}
