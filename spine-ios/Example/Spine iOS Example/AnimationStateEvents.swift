//
//  AnimationStateEvents.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 15.05.24.
//

import SwiftUI
import Spine
import SpineCppLite

struct AnimationStateEvents: View {
    
    @StateObject
    var controller = SpineController(
        onInitialized: { controller in
            controller.skeleton.scaleX = 0.5
            controller.skeleton.scaleY = 0.5
            controller.skeleton.findSlot(slotName: "gun")?.setColor(r: 1, g: 0, b: 0, a: 1)
            controller.animationStateData.defaultMix = 0.2
            let walk = controller.animationState.setAnimationByName(trackIndex: 0, animationName: "walk", loop: true)
            controller.animationStateWrapper.setTrackEntryListener(entry: walk) { type, entry, event in
                print("Walk animation event \(type)");
            }
            controller.animationState.addAnimationByName(trackIndex: 0, animationName: "jump", loop: false, delay: 2)
            let run = controller.animationState.addAnimationByName(trackIndex: 0, animationName: "run", loop: true, delay: 0)
            controller.animationStateWrapper.setTrackEntryListener(entry: run) { type, entry, event in
                print("Run animation event \(type)");
            }
            controller.animationStateWrapper.setStateListener { type, entry, event in
                if type == SPINE_EVENT_TYPE_EVENT, let event {
                    print("User event: { name: \(event.data.name ?? "--"), intValue: \(event.intValue), floatValue: \(event.floatValue), stringValue: \(event.stringValue ?? "--") }")
                }
            }
            let current = controller.animationState.getCurrent(trackIndex: 0).animation.name ?? "--"
            print("Current: \(current)")
        }
    )
    
    var body: some View {
        VStack {
            Text("See output in console!")
            SpineView(
                atlasFile: "spineboy.atlas",
                skeletonFile: "spineboy-pro.skel",
                controller: controller
            )
        }
        .navigationTitle("Animation State Listener")
        .navigationBarTitleDisplayMode(.inline)
    }
}

#Preview {
    AnimationStateEvents()
}
