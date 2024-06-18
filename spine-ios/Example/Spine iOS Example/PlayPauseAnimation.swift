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
            from: .bundle(atlasFileName: "dragon.atlas", skeletonFileName: "dragon-ess.skel"),
//            from: .http(
//                atlasURL: URL(string: "https://github.com/esotericsoftware/spine-runtimes/raw/spine-ios/spine-ios/Example/Spine%20iOS%20Example/Assets/dragon/dragon.atlas")!,
//                skeletonURL:  URL(string: "https://github.com/esotericsoftware/spine-runtimes/raw/spine-ios/spine-ios/Example/Spine%20iOS%20Example/Assets/dragon/dragon-ess.skel")!
//            ),
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
