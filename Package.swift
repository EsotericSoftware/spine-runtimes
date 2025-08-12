// swift-tools-version: 5.9
// The swift-tools-version declares the minimum version of Swift required to build this package.

import PackageDescription

let package = Package(
    name: "spine-ios",
    platforms: [
        .iOS(.v13),
        .tvOS(.v13),
        .macCatalyst(.v13),
        .visionOS(.v1),
        .macOS(.v10_15),
        .watchOS(.v6),
    ],
    products: [
        // Products define the executables and libraries a package produces, making them visible to other packages.
        .library(
            name: "SpineC",
            targets: ["SpineC"]
        ),
        .library(
            name: "SpineSwift",
            targets: ["SpineSwift"]
        ),
        .library(
            name: "SpineiOS",
            targets: ["SpineiOS"]
        ),
    ],
    targets: [
        .target(
            name: "SpineiOS",
            dependencies: [
                "SpineSwift",
                "SpineShadersStructs",
            ],
            path: "spine-ios/Sources/SpineiOS"
        ),
        .target(
            name: "SpineC",
            path: "spine-ios/Sources/SpineC",
            sources: [
                "src",
                "spine"
            ],
            linkerSettings: [
                .linkedLibrary("c++")
            ]
        ),
        .target(
            name: "SpineSwift",
            dependencies: ["SpineC"],
            path: "spine-ios/Sources/SpineSwift",
            sources: [
                "Generated",
                "Extensions",
            ]
        ),
        .systemLibrary(
            name: "SpineShadersStructs",
            path: "spine-ios/Sources/SpineShadersStructs"
        ),
    ],
    cxxLanguageStandard: .cxx11
)
