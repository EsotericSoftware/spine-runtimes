//
//  ContentView.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 17.04.24.
//

import SwiftUI
import BaseViews
import Spine

struct ContentView: View {
    var body: some View {
        VStack {
            Image(systemName: "globe")
                .imageScale(.large)
                .foregroundStyle(.tint)
            Text("Hello, \(Spine.name)!")
        }
        .padding()
    }
}

#Preview {
    ContentView()
}
