plugins {
    alias(libs.plugins.androidLibrary)
    `maven-publish`
    signing
}

android {
    namespace = "com.esotericsoftware.spine"
    compileSdk = 34

    defaultConfig {
        minSdk = 23

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
    api("com.esotericsoftware.spine:spine-libgdx:4.2.7")

    testImplementation(libs.junit)
    androidTestImplementation(libs.androidx.junit)
    androidTestImplementation(libs.androidx.espresso.core)
}

val libraryVersion = "4.2.7";

tasks.register<Jar>("sourceJar") {
    archiveClassifier.set("sources")
    from(android.sourceSets["main"].java.srcDirs)
}

afterEvaluate {
    publishing {
        publications {
            create<MavenPublication>("release") {
                artifact(tasks.getByName("bundleReleaseAar"))
                artifact(tasks.getByName("sourceJar"))

                groupId = "com.esotericsoftware.spine"
                artifactId = "spine-android"
                version = libraryVersion

                pom {
                    packaging = "aar"
                    name.set("spine-android")
                    description.set("Spine Runtime for Android")
                    url.set("https://github.com/esotericsoftware/spine-runtimes")
                    licenses {
                        license {
                            name.set("Spine Runtimes License")
                            url.set("http://esotericsoftware.com/spine-runtimes-license")
                        }
                    }
                    developers {
                        developer {
                            name.set("Esoteric Software")
                            email.set("contact@esotericsoftware.com")
                        }
                    }
                    scm {
                        url.set(pom.url.get())
                        connection.set("scm:git:${url.get()}.git")
                        developerConnection.set("scm:git:${url.get()}.git")
                    }

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

        repositories {
            maven {
                name = "SonaType"
                url = uri(if (libraryVersion.endsWith("-SNAPSHOT")) {
                    "https://oss.sonatype.org/content/repositories/snapshots"
                } else {
                    "https://oss.sonatype.org/service/local/staging/deploy/maven2"
                })

                credentials {
                    username = project.findProperty("ossrhUsername") as String?
                    password = project.findProperty("ossrhPassword") as String?
                }
            }
        }
    }

    signing {
        useGpgCmd()
        sign(publishing.publications["release"])
        sign(tasks.getByName("sourceJar"))
    }

    tasks.withType<Sign> {
        onlyIf { !libraryVersion.endsWith("-SNAPSHOT") }
    }
}
