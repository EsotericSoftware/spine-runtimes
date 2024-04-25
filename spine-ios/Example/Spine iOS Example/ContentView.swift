//
//  ContentView.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 17.04.24.
//

import SwiftUI
import Spine

struct ContentView: View {
    var body: some View {
        SpineView(mesh: "spineboy-mesh")
    }
}

#Preview {
    ContentView()
}
