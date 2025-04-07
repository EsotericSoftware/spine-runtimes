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

struct DebugRendering: View {
    
    @StateObject
    var model = DebugRenderingModel()
    
    var body: some View {
        ZStack {
            Color.red.ignoresSafeArea()
            SpineView(
                from: .bundle(atlasFileName: "spineboy-pma.atlas", skeletonFileName: "spineboy-pro.skel"),
                controller: model.controller,
                mode: .fit,
                alignment: .center
            )
            ForEach(model.boneRects, id: \.id) { boneLocation in
                Rectangle()
                    .fill(.blue)
                    .offset(x: boneLocation.x, y: boneLocation.y)
                    .frame(width: boneLocation.width, height: boneLocation.height)
            }
        }
        .navigationTitle("Debug Rendering")
        .navigationBarTitleDisplayMode(.inline)
    }
}

#Preview {
    DebugRendering()
}

final class DebugRenderingModel: ObservableObject {
    
    @Published
    var controller: SpineController!
    
    @Published
    var boneRects = [BoneRect]()
    
    init() {
        controller = SpineController(
            onInitialized: { controller in
                controller.animationState.setAnimationByName(
                    trackIndex: 0,
                    animationName: "walk",
                    loop: true
                )
            },
            onAfterPaint: { 
                [weak self] controller in guard let self else { return }
                boneRects = controller.drawable.skeleton.bones.map { bone in
                    let position = controller.fromSkeletonCoordinates(
                        position: CGPointMake(CGFloat(bone.worldX), CGFloat(bone.worldY))
                    )
                    return BoneRect(
                        id: UUID(),
                        x: position.x,
                        y: position.y,
                        width: 5,
                        height: 5
                    )
                }
            }
        )
    }
}

struct BoneRect: Hashable {
    let id: UUID
    let x: CGFloat
    let y: CGFloat
    let width: CGFloat
    let height: CGFloat
}
