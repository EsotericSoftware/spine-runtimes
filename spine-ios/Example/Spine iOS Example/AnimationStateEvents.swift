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
            let current = controller.animationState.getCurrent(trackIndex: 0)?.animation.name ?? "--"
            print("Current: \(current)")
        }
    )
    
    var body: some View {
        VStack {
            Text("See output in console!")
            SpineView(
                from: .bundle(atlasFileName: "spineboy-pma.atlas", skeletonFileName: "spineboy-pro.skel"),
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
