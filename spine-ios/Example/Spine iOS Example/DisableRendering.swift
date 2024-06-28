import SwiftUI
import Spine

struct DisableRendering: View {
    
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
    
    @State
    var isRendering: Bool?
    
    var body: some View {
        VStack {
            List {
                VStack(alignment: .leading) {
                    Text("Scroll spine boy out of the viewport")
                    Text("Rendering is disabled when the spine view moves out of the viewport, preserving CPU/GPU resources.")
                        .foregroundColor(.secondary)
                }
                
                SpineView(
                    from: .bundle(atlasFileName: "spineboy-pma.atlas", skeletonFileName: "spineboy-pro.skel"),
                    controller: controller,
                    isRendering: $isRendering
                )
                .frame(minHeight: 200)
                .onAppear {
                    isRendering = true
                    print("rendering enabled")
                }
                .onDisappear {
                    isRendering = false
                    print("rendering disabled")
                }
                
                Text("Foo")
                    .frame(minHeight: 400)
                
                Text("Bar")
                    .frame(minHeight: 400)
                
                Text("Baz")
                    .frame(minHeight: 400)
            }
        }
        .navigationTitle("Disable Rendering")
        .navigationBarTitleDisplayMode(.inline)
    }
}

#Preview {
    DisableRendering()
}
