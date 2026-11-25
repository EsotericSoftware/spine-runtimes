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

import SpineiOS
import SwiftUI

struct TextureFiltering: View {
    @State private var textureFilter = SpineTextureFilter.atlas

    @StateObject
    private var controller = SpineController(
        onInitialized: { controller in
            controller.animationState.setAnimation(0, "walk", true)
        }
    )

    var body: some View {
        VStack {
            Picker("Texture filter", selection: $textureFilter) {
                Text("Atlas").tag(SpineTextureFilter.atlas)
                Text("Nearest").tag(SpineTextureFilter.nearest)
                Text("Linear").tag(SpineTextureFilter.linear)
            }
            .pickerStyle(.segmented)
            .padding()

            SpineView(
                from: .bundle(atlasFileName: "spineboy-pma.atlas", skeletonFileName: "spineboy-pro.skel"),
                controller: controller,
                mode: .fill,
                textureFilter: textureFilter
            )
            // Texture filtering is selected when the renderer is created.
            .id(textureFilter)

            Text(description)
                .font(.footnote)
                .foregroundColor(.secondary)
                .multilineTextAlignment(.center)
                .padding()
        }
        .onDisappear {
            controller.dispose()
        }
        .navigationTitle("Texture Filtering")
        .inlineNavigationTitle()
    }

    private var description: String {
        switch textureFilter {
        case .atlas:
            return "Uses the minification and magnification filters exported in each atlas page. This atlas uses linear filtering."
        case .nearest:
            return "Overrides all atlas pages with nearest-neighbor filtering."
        case .linear:
            return "Overrides all atlas pages with linear filtering."
        }
    }
}

#Preview {
    TextureFiltering()
}
