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

struct Physics: View {
    
    @StateObject
    var model = PhysicsModel()
    
    var body: some View {
		ZStack {
			Color(red: 51 / 255, green: 51 / 255, blue: 51 / 255).ignoresSafeArea()
			SpineView(
				from: .bundle(atlasFileName: "celestial-circus-pma.atlas", skeletonFileName: "celestial-circus-pro.skel"),
				controller: model.controller
			)
			.gesture(
				DragGesture(minimumDistance: 0)
					.onChanged { gesture in
						model.updateBonePosition(position: gesture.location)
					}
			)
		}
        .navigationTitle("Physics (drag anywhere)")
        .navigationBarTitleDisplayMode(.inline)
    }
}

#Preview {
    Physics()
}

final class PhysicsModel: ObservableObject {
    
    @Published
    var controller: SpineController!
    
    @Published
    var mousePosition: CGPoint?
    
    @Published
    var lastMousePosition: CGPoint?
    
    init() {
        controller = SpineController(
            onInitialized: { controller in
                controller.animationState.setAnimationByName(
                    trackIndex: 0,
                    animationName: "eyeblink",
                    loop: true
                )
                controller.animationState.setAnimationByName(
                    trackIndex: 1,
                    animationName: "wings-and-feet",
                    loop: true
                )
            },
            onAfterUpdateWorldTransforms: {
                [weak self] controller in guard let self else { return }
                
                guard let lastMousePosition else {
                    self.lastMousePosition = mousePosition
                    return
                }
                guard let mousePosition else {
                    return
                }
                let dx = mousePosition.x - lastMousePosition.x
                let dy = mousePosition.y - lastMousePosition.y
                let positionX = controller.skeleton.x + Float(dx)
                let positionY = controller.skeleton.y + Float(dy)
                controller.skeleton.setPosition(x: positionX, y: positionY)
                self.lastMousePosition = mousePosition
            }
        )
    }
    
    func updateBonePosition(position: CGPoint) {
        mousePosition = controller.toSkeletonCoordinates(position: position)
    }
}
