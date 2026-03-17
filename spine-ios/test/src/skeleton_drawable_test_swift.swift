import Foundation
import SpineSwift

func runSkeletonDrawableTestSwift() {
    print("Testing SkeletonDrawable with SpineSwift API...")

    print("Step 1: Enable debug extension")
    // Enable debug extension for leak detection
    enableDebugExtension(true)
    print("  Debug extension configured")

    do {
        print("Step 2: Load file paths")
        // Load atlas and skeleton data
        let atlasPath = "../../spine-ts/assets/spineboy.atlas"
        let jsonPath = "../../spine-ts/assets/spineboy-pro.json"

        print("Step 3: Read atlas file")
        // Read atlas file
        guard let atlasData = try? String(contentsOfFile: atlasPath, encoding: .utf8) else {
            print("❌ Failed to read atlas file: \(atlasPath)")
            return
        }
        print("  Atlas file read successfully")

        print("Step 4: Load atlas with SpineSwift API")
        // Load atlas
        let atlas: Atlas
        do {
            atlas = try loadAtlas(atlasData)
            print("✓ Atlas loaded successfully")
        } catch {
            print("❌ Failed to load atlas: \(error)")
            return
        }

        // Read skeleton JSON
        guard let skeletonJson = try? String(contentsOfFile: jsonPath, encoding: .utf8) else {
            print("❌ Failed to read skeleton JSON file: \(jsonPath)")
            // atlas will be freed when out of scope
            return
        }

        // Load skeleton data
        let skeletonData: SkeletonData
        do {
            skeletonData = try loadSkeletonDataJson(atlas: atlas, jsonData: skeletonJson, path: jsonPath)
            print("✓ Skeleton data loaded successfully")
        } catch {
            print("❌ Failed to load skeleton data: \(error)")
            // atlas will be freed when out of scope
            return
        }

        do {
            // Create skeleton drawable
            let drawable = SkeletonDrawable(skeletonData: skeletonData)
            print("✓ SkeletonDrawable created successfully")

            // Test skeleton bounds
            print("\nTesting skeleton bounds:")
            let initialBounds = drawable.skeleton.bounds
            print("  Initial bounds: x=\(initialBounds.x), y=\(initialBounds.y), width=\(initialBounds.width), height=\(initialBounds.height)")

            // Set skeleton to pose and update bounds
            drawable.skeleton.setupPose()
            drawable.skeleton.updateWorldTransform(Physics.none)

            let boundsAfterPose = drawable.skeleton.bounds
            print(
                "  Bounds after setupPose: x=\(boundsAfterPose.x), y=\(boundsAfterPose.y), width=\(boundsAfterPose.width), height=\(boundsAfterPose.height)"
            )

            // Test position
            let position = drawable.skeleton.getPosition()
            print("  Skeleton position: x=\(position.x), y=\(position.y)")

            // Set up animation state listener
            var eventCount = 0
            drawable.animationState.setListener { type, trackEntry, event in
                eventCount += 1
                print("  AnimationState event #\(eventCount): type=\(type), track=\(trackEntry.trackIndex)")
                if let event = event {
                    print("    Event name: \(event.data.name)")
                }
            }

            // Set an animation
            let trackEntry = drawable.animationState.setAnimation(0, "walk", true)
            print("✓ Set animation: walk")

            // Set track entry listener
            trackEntry.setListener { type, entry, event in
                print("  TrackEntry event: type=\(type)")
                if let event = event {
                    print("    Event data: \(event.data.name)")
                }
            }

            // Update several times to trigger events
            print("\nUpdating animation state...")
            for i in 0..<5 {
                drawable.update(0.016)  // ~60fps
                print("  Frame \(i): updated")
            }

            // Test switching animations
            print("\nSwitching to run animation...")
            _ = drawable.animationState.setAnimation(0, "run", true)

            // Update a few more times
            for i in 0..<3 {
                drawable.update(0.016)
                print("  Frame \(i): updated after switching")
            }

            // Test bounds after animation updates
            print("\nTesting bounds after animation:")
            drawable.skeleton.updateWorldTransform(Physics.none)
            let boundsAfterAnimation = drawable.skeleton.bounds
            print(
                "  Bounds after animation: x=\(boundsAfterAnimation.x), y=\(boundsAfterAnimation.y), width=\(boundsAfterAnimation.width), height=\(boundsAfterAnimation.height)"
            )

            // Test with different animations that might have different bounds
            print("\nTesting bounds with jump animation:")
            _ = drawable.animationState.setAnimation(0, "jump", false)
            drawable.update(0.5)  // Update to middle of jump

            let boundsAfterJump = drawable.skeleton.bounds
            print(
                "  Bounds during jump: x=\(boundsAfterJump.x), y=\(boundsAfterJump.y), width=\(boundsAfterJump.width), height=\(boundsAfterJump.height)"
            )

            // Test skin entries
            print("\nTesting skin entries:")

            // First, check available skins
            let skins = skeletonData.skins
            print("  Available skins: \(skins.count)")
            if skins.count > 0 {
                // List all skin names
                for i in 0..<skins.count {
                    if let skin = skins[i] {
                        print("    Skin \(i): \(skin.name)")
                    }
                }

                // Set and test the first skin (or default skin)
                if let defaultSkin = skins[0] {
                    // First get entries from the skin directly
                    let entriesFromData = defaultSkin.getEntries()
                    print("  Skin '\(defaultSkin.name)' from skeletonData has \(entriesFromData.count) entries")

                    // Count entries with attachments
                    let withAttachments = entriesFromData.filter { $0.attachment != nil }.count
                    print("    Entries with attachments: \(withAttachments)")

                    // Now set it on the skeleton
                    drawable.skeleton.setSkin2(defaultSkin)
                    drawable.skeleton.setupPoseSlots()  // Update slots after setting skin
                    print("  Set skin on skeleton: \(defaultSkin.name)")
                }
            }

            // Now test the skeleton's current skin
            if let skin = drawable.skeleton.skin {
                let entries = skin.getEntries()
                print("  Skeleton's current skin has \(entries.count) entries")

                // Show first few entries (with or without attachments for debugging)
                let entriesToShow = min(5, entries.count)
                for i in 0..<entriesToShow {
                    let entry = entries[i]
                    if let attachment = entry.attachment {
                        let attachmentType = attachment.rtti.rttiClassName ?? "unknown"
                        let attachmentName = attachment.name
                        print("    Entry \(i): slot=\(entry.slotIndex), name=\(entry.name)")
                        print("      Attachment: name=\(attachmentName), type=\(attachmentType)")
                    } else {
                        print("    Entry \(i): slot=\(entry.slotIndex), name=\(entry.name), attachment=nil")
                    }
                }

                // Count total entries with attachments
                let entriesWithAttachments = entries.filter { $0.attachment != nil }.count
                print("  Total entries with attachments: \(entriesWithAttachments) out of \(entries.count)")

                if entries.count > 3 {
                    print("    ... and \(entries.count - 3) more entries")
                }
            } else {
                print("  No skin is currently set")
            }

            // Test bone pose transformations
            print("\nTesting bone pose transformations:")
            if let rootBone = drawable.skeleton.rootBone {
                // Test bone properties
                let bonePose = rootBone.appliedPose
                print("  Root bone: name=\(rootBone.data.name), x=\(bonePose.x), y=\(bonePose.y)")

                // Get the bone's transform matrix from applied pose
                let a = bonePose.a
                let b = bonePose.b
                let c = bonePose.c
                let d = bonePose.d
                let worldX = bonePose.worldX
                let worldY = bonePose.worldY
                print("  Root bone transform: a=\(a), b=\(b), c=\(c), d=\(d), worldX=\(worldX), worldY=\(worldY)")

                // Use BonePose's built-in transformation methods
                let worldPoint = Vector(x: 100, y: 100)
                let localPoint = bonePose.worldToLocal(worldX: worldPoint.x, worldY: worldPoint.y)
                print("  World point (100, 100) -> Local: (\(localPoint.x), \(localPoint.y))")

                // Test local to world transformation
                let backToWorld = bonePose.localToWorld(localX: localPoint.x, localY: localPoint.y)
                print("  Local back to world: (\(backToWorld.x), \(backToWorld.y))")

                // Test rotation and scale from the pose
                let rotation = bonePose.rotation
                let scaleX = bonePose.scaleX
                let scaleY = bonePose.scaleY
                print("  Bone rotation: \(rotation), scale: (\(scaleX), \(scaleY))")

                // Test parent transformation if parent exists
                if let parentBone = rootBone.parent {
                    let parentPose = parentBone.appliedPose
                    print("  Parent bone: name=\(parentBone.data.name), worldX=\(parentPose.worldX), worldY=\(parentPose.worldY)")

                    // Transform world to parent coordinates
                    let parentPoint = bonePose.worldToParent(worldX: worldPoint.x, worldY: worldPoint.y)
                    print("  World point (100, 100) -> Parent: (\(parentPoint.x), \(parentPoint.y))")
                }
            }

            // Test render command
            print("\nTesting render command:")
            if let renderCommand = drawable.render() {
                print("  Got render command with blend mode: \(renderCommand.blendMode)")
                // Note: atlasPage and vertices are accessed via getters, not properties
                print("  Render command received successfully")
            }

            // Test constraint RTTI (similar to Dart test)
            print("\nTesting constraint RTTI:")
            let constraints = drawable.skeleton.constraints
            print("  Total constraints: \(constraints.count)")
            for i in 0..<constraints.count {
                if let constraint = constraints[i] {
                    let rttiClassName = constraint.rtti.rttiClassName ?? "unknown"
                    print("  Constraint \(i) type: \(rttiClassName)")

                    let data = constraint.data
                    let dataRttiClassName = data.rtti.rttiClassName ?? "unknown"
                    print("    Data type: \(dataRttiClassName)")
                    print("    Name: \(data.name)")
                }
            }

            // Test calling PosedActive methods on Bone instance
            print("\nTesting PosedActive methods on Bone:")
            if let bone = drawable.skeleton.findBone("rear-shin") {
                print("  Found bone: \(bone.data.name)")

                // Cast Bone to PosedActive and try to call PosedActive methods
                let posedActive = bone as PosedActive
                print("  Successfully cast Bone to PosedActive")

                // Try to access PosedActive properties/methods
                // This should work if the pointer is correct, or crash if there's an offset issue

                // Get the isActive property (this is a PosedActive method)
                let isActiveFromPosed = posedActive.isActive
                print("  isActive from PosedActive: \(isActiveFromPosed)")

                // Set active property through PosedActive
                print("  Setting active to false through PosedActive...")
                posedActive.active = false
                print("  ✓ Set active property succeeded")

                // Check if it's actually false
                let isStillActive = posedActive.isActive
                print("  isActive after setting to false: \(isStillActive)")

                // Restore it
                posedActive.active = true
                print("  Restored active to true")

                // Now test the Update protocol
                let updateProtocol = bone as Update
                print("  Successfully cast Bone to Update protocol")

                // Call update with required parameters
                print("  Calling update() through Update protocol...")
                updateProtocol.update(drawable.skeleton, Physics.none)
                print("  ✓ update() call succeeded through Update protocol")

                // Test that we can still use Bone methods after casting
                let appliedPose = bone.appliedPose
                print("  Bone appliedPose still accessible: x=\(appliedPose.x), y=\(appliedPose.y)")

                // The fact that this all works suggests the pointer is being handled correctly
                // even though we're not using cast functions in the constructor chain
                print("  ✓ All PosedActive and Update methods work correctly on Bone instance")
            } else {
                print("  Could not find bone 'rear-shin'")
            }

            // Clear listener before cleanup
            // drawable.animationState.setListener(nil)
        }

        skeletonData.dispose()
        atlas.dispose()

        print("\n✓ SpineSwift API test complete")
    }

    // Report memory leaks after Swift objects have gone out of scope
    reportLeaks()
}

// Test function is called from main.swift
