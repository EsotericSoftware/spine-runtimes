//
//  MainView.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 08.05.24.
//

import SwiftUI

struct MainView: View {
    var body: some View {
        
        List {
            NavigationLink("SimpleAnimation") {
                SimpleAnimation()
            }
            NavigationLink("Play/Pause") {
                PlayPauseAnimation()
            }
            NavigationLink("Animation State Listener") {
                AnimationStateEvents()
            }
            NavigationLink("Dress Up") {
                DressUp()
            }
            NavigationLink("IK Following") {
                IKFollowing()
            }
        }
        .navigationTitle("Spine Examples")
    }
}

#Preview {
    MainView()
}
