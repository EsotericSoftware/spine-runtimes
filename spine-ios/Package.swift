// swift-tools-version: 5.9
// The swift-tools-version declares the minimum version of Swift required to build this package.

import PackageDescription

let package = Package(
    name: "spine-ios",
    platforms: [
        .iOS(.v16)
    ],
    products: [
        // Products define the executables and libraries a package produces, making them visible to other packages.
        .library(
            name: "Spine",
            targets: ["Spine"]
        ),
        .library(
            name: "SpineCppLite",
            targets: ["SpineCppLite"]
        )
    ],
    targets: [
        .target(
            name: "Spine",
            dependencies: [
                "SpineCppLite", "SpineShadersStructs"
            ],
            path: "Sources/Spine",
            swiftSettings: [
                .interoperabilityMode(.Cxx)
            ]
        ),
        .target(
            name: "SpineCppLite",
            path: "Sources/SpineCppLite"
        ),
        .systemLibrary(
            name: "SpineShadersStructs",
            path: "Sources/SpineShadersStructs"
        )
    ],
    cxxLanguageStandard: .cxx11
)
