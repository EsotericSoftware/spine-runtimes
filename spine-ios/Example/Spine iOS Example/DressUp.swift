import SwiftUI
import Spine
import SpineCppLite

struct DressUp: View {
    
    @StateObject
    var model = DressUpModel()
    
    var body: some View {
        HStack(spacing: 0) {
            List {
                ForEach(model.skinImages.keys.sorted(), id: \.self) { skinName in
                    let rawImageData = model.skinImages[skinName]!
                    Button(action: { model.toggleSkin(skinName: skinName) }) {
                        Image(uiImage: UIImage(cgImage: rawImageData))
                            .resizable()
                            .scaledToFit()
                            .frame(width: model.thumbnailSize.width, height: model.thumbnailSize.height)
                            .grayscale(model.selectedSkins[skinName] == true ? 0.0 : 1.0)
                    }
                    .listRowSeparator(.hidden)
                }
            }
            .listStyle(.plain)
            
            Divider()
            
            if let drawable = model.drawable {
                SpineView(
                    from: .drawable(drawable),
                    controller: model.controller,
                    boundsProvider: SkinAndAnimationBounds(skins: ["full-skins/girl"])
                )
            } else {
                Spacer()
            }
        }
        .navigationTitle("Dress Up")
        .navigationBarTitleDisplayMode(.inline)
    }
}

#Preview {
    DressUp()
}

final class DressUpModel: ObservableObject {
    
    let thumbnailSize = CGSize(width: 200, height: 200)
    
    @Published
    var controller: SpineController
    
    @Published
    var drawable: SkeletonDrawableWrapper?
    
    @Published
    var skinImages = [String: CGImage]()
    
    @Published
    var selectedSkins = [String: Bool]()
    
    private var customSkin: Skin?
    
    init() {
        controller = SpineController(
            onInitialized: { controller in
                controller.animationState.setAnimationByName(
                    trackIndex: 0,
                    animationName: "dance",
                    loop: true
                )
            },
            disposeDrawableOnDeInit: false
        )
        Task.detached(priority: .high) {
            let drawable = try await SkeletonDrawableWrapper.fromBundle(
                atlasFileName: "mix-and-match.atlas",
                skeletonFileName: "mix-and-match-pro.skel"
            )
            try await MainActor.run {
                for skin in drawable.skeletonData.skins {
                    if skin.name == "default" { continue }
                    let skeleton = drawable.skeleton
                    skeleton.skin = skin
                    skeleton.setToSetupPose()
                    skeleton.update(delta: 0)
                    skeleton.updateWorldTransform(physics: SPINE_PHYSICS_UPDATE)
                    try skin.name.flatMap { skinName in
                        self.skinImages[skinName] = try drawable.renderToImage(
                            size: self.thumbnailSize,
                            backgroundColor: .white
                        )
                        self.selectedSkins[skinName] = false
                    }
                }
                self.toggleSkin(skinName: "full-skins/girl", drawable: drawable)
                self.drawable = drawable
            }
        }
    }
    
    deinit {
        drawable?.dispose()
        customSkin?.dispose()
    }
    
    func toggleSkin(skinName: String) {
        if let drawable {
            toggleSkin(skinName: skinName, drawable: drawable)
        }
    }
    
    func toggleSkin(skinName: String, drawable: SkeletonDrawableWrapper) {
        selectedSkins[skinName] = !(selectedSkins[skinName] ?? false)
        customSkin?.dispose()
        customSkin = Skin.create(name: "custom-skin")
        for skinName in selectedSkins.keys {
          if selectedSkins[skinName] == true {
              if let skin = drawable.skeletonData.findSkin(name: skinName) {
                  customSkin?.addSkin(other: skin)
              }
          }
        }
        drawable.skeleton.skin = customSkin
        drawable.skeleton.setToSetupPose()
    }
}
