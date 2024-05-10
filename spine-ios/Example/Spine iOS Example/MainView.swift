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
        }
        .navigationTitle("Spine iOS Example")
    }
}

#Preview {
    MainView()
}
