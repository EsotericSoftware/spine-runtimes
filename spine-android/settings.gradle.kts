pluginManagement {
    repositories {
        google {
            content {
                includeGroupByRegex("com\\.android.*")
                includeGroupByRegex("com\\.google.*")
                includeGroupByRegex("androidx.*")
            }
        }
        mavenCentral()
        gradlePluginPortal()
    }
}
dependencyResolutionManagement {
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        mavenCentral()
        maven {
            url = uri("https://oss.sonatype.org/content/repositories/snapshots")
        }
        mavenLocal()
    }
}

rootProject.name = "Spine Android Examples"
includeBuild("../spine-libgdx") {
    dependencySubstitution {
        substitute(module("com.esotericsoftware.spine:spine-libgdx")).using(project(":spine-libgdx"))
    }
}
//includeBuild("../../libgdx") {
//    dependencySubstitution {
//        substitute(module("com.badlogicgames.gdx:gdx")).using(project(":gdx"))
//    }
//}
include(":app")
include(":spine-android")
