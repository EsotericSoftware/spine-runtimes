// swift-tools-version: 5.9
import PackageDescription

let package = Package(
    name: "spine_flutter",
    platforms: [
        .iOS("13.0"),
        .macOS("10.15")
    ],
    products: [
        .library(name: "spine-flutter", type: .dynamic, targets: ["spine_flutter"])
    ],
    targets: [
        .target(
            name: "spine_flutter",
            path: "Sources/spine_flutter",
            sources: [
                "src",
                "spine"
            ],
            publicHeadersPath: "include",
            cxxSettings: [
                .headerSearchPath("include"),
                .headerSearchPath("include/spine")
            ],
            linkerSettings: [
                .linkedLibrary("c++")
            ]
        )
    ],
    cxxLanguageStandard: .cxx11
)
