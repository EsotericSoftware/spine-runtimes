import SwiftUI
import Spine

struct IKFollowing: View {
    
    @StateObject
    var model = IKFollowingModel()
        
    var body: some View {
        SpineView(
            from: .bundle(atlasFileName: "spineboy.atlas", skeletonFileName: "spineboy-pro.skel"),
            controller: model.controller,
            alignment: .centerLeft
        )
        .gesture(
            DragGesture(minimumDistance: 0)
                .onChanged { gesture in
                    model.crossHairPosition = model.controller.toSkeletonCoordinates(
                        position: gesture.location
                    )
                }
        )
        .navigationTitle("IK Following")
        .navigationBarTitleDisplayMode(.inline)
    }
}

#Preview {
    if #available(iOS 15.0, *) {
        IKFollowing()
            .previewInterfaceOrientation(.landscapeLeft)
    } else {
        IKFollowing()
    }
}

final class IKFollowingModel: ObservableObject {
    
    @Published
    var controller: SpineController!
    
    @Published
    var crossHairPosition: CGPoint?
    
    init() {
        controller = SpineController(
            onInitialized: { controller in
                controller.animationState.setAnimationByName(
                    trackIndex: 0,
                    animationName: "walk",
                    loop: true
                )
                controller.animationState.setAnimationByName(
                    trackIndex: 1,
                    animationName: "aim",
                    loop: true
                )
            },
            onAfterUpdateWorldTransforms: { 
                [weak self] controller in guard let self else { return }
                guard let worldPosition = self.crossHairPosition else {
                    return
                }
                let bone = controller.skeleton.findBone(boneName: "crosshair")!
                if let parent = bone.parent {
                    let position = parent.worldToLocal(worldX: Float(worldPosition.x), worldY:  Float(worldPosition.y))
                    bone.x = position.x
                    bone.y = position.y
                }
            }
        )
    }
}
