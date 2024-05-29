import SwiftUI
import Spine

struct SimpleAnimation: View {
    
    @StateObject
    var controller = SpineController(
        onInitialized: { controller in
            controller.animationState.setAnimationByName(
                trackIndex: 0,
                animationName: "walk",
                loop: true
            )
        }
    )
    
    var body: some View {
        SpineView(
            from: .bundle(atlasFileName: "spineboy.atlas", skeletonFileName: "spineboy-pro.skel"),
//            from: .http(
//                atlasURL: URL(string: "https://github.com/denrase/spine-runtimes/raw/spine-ios/spine-ios/Example/Spine%20iOS%20Example/Assets/spineboy/spineboy.atlas")!,
//                skeletonURL:  URL(string: "https://github.com/denrase/spine-runtimes/raw/spine-ios/spine-ios/Example/Spine%20iOS%20Example/Assets/spineboy/spineboy-pro.skel")!
//            ),
            controller: controller,
            mode: .fit,
            alignment: .center
        )
        .navigationTitle("Simple Animation")
        .navigationBarTitleDisplayMode(.inline)
    }
}

#Preview {
    SimpleAnimation()
}
