// swift-tools-version: 5.9
// The swift-tools-version declares the minimum version of Swift required to build this package.

import PackageDescription

// spine-c and spine-cpp sources are copied into Sources/spine_flutter by
// spine-flutter/setup.sh.
let package = Package(
    name: "spine_flutter",
    platforms: [
        .iOS("13.0")
    ],
    products: [
        // Dynamic, so the Dart side can load it via
        // DynamicLibrary.open('spine_flutter.framework/spine_flutter').
        .library(name: "spine-flutter", type: .dynamic, targets: ["spine_flutter"])
    ],
    dependencies: [],
    targets: [
        .target(
            name: "spine_flutter",
            dependencies: [],
            exclude: [
                "spine-c/include/module.modulemap"
            ],
            publicHeadersPath: "include",
            cxxSettings: [
                .headerSearchPath("spine-cpp/include"),
                .headerSearchPath("spine-c/include"),
                .headerSearchPath("spine-c/src"),
                .headerSearchPath("spine-c/src/generated"),
            ]
        )
    ],
    cxxLanguageStandard: .cxx11
)
