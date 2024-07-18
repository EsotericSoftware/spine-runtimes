plugins {
    alias(libs.plugins.androidLibrary)
    `maven-publish`
}

android {
    namespace = "com.esotericsoftware.spine"
    compileSdk = 34

    defaultConfig {
        minSdk = 24

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        consumerProguardFiles("consumer-rules.pro")
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_1_8
        targetCompatibility = JavaVersion.VERSION_1_8
    }
}

dependencies {
    implementation(libs.androidx.appcompat)
    api("com.badlogicgames.gdx:gdx:1.12.2-SNAPSHOT")
    api("com.esotericsoftware.spine:spine-libgdx:4.2.0")

    testImplementation(libs.junit)
    androidTestImplementation(libs.androidx.junit)
    androidTestImplementation(libs.androidx.espresso.core)
}

afterEvaluate {
    publishing {
        publications {
            create<MavenPublication>("mavenLocal") {
                groupId = "com.esotericsoftware"
                artifactId = "spine-android"
                version = "4.2"
                artifact(tasks.getByName("bundleReleaseAar"))

                pom {
                    withXml {
                        val dependenciesNode = asNode().appendNode("dependencies")
                        configurations.api.get().dependencies.forEach { dependency ->
                            dependenciesNode.appendNode("dependency").apply {
                                appendNode("groupId", dependency.group)
                                appendNode("artifactId", dependency.name)
                                appendNode("version", dependency.version)
                                appendNode("scope", "compile")
                            }
                        }
                        configurations.implementation.get().dependencies.forEach { dependency ->
                            dependenciesNode.appendNode("dependency").apply {
                                appendNode("groupId", dependency.group)
                                appendNode("artifactId", dependency.name)
                                appendNode("version", dependency.version)
                                appendNode("scope", "runtime")
                            }
                        }
                    }
                }
            }
        }
    }
}
