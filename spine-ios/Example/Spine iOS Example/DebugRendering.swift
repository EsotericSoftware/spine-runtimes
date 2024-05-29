import SwiftUI
import Spine

struct DebugRendering: View {
    
    @StateObject
    var model = DebugRenderingModel()
    
    var body: some View {
        ZStack {
            Color.red.ignoresSafeArea()
            SpineView(
                from: .bundle(atlasFileName: "spineboy.atlas", skeletonFileName: "spineboy-pro.skel"),
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
