import Foundation
import SpineC

func runSkeletonDrawableTest() {
    print("Testing SkeletonDrawable and event listeners...")

    // Enable debug extension for leak detection
    spine_enable_debug_extension(true)

    // Load atlas and skeleton data
    let atlasPath = "../../spine-ts/assets/spineboy.atlas"
    let jsonPath = "../../spine-ts/assets/spineboy-pro.json"

    // Read atlas file
    guard let atlasData = try? String(contentsOfFile: atlasPath, encoding: .utf8) else {
        print("❌ Failed to read atlas file: \(atlasPath)")
        return
    }

    // Load atlas
    let atlasResult = spine_atlas_load(atlasData)
    guard let atlas = spine_atlas_result_get_atlas(atlasResult) else {
        if let error = spine_atlas_result_get_error(atlasResult) {
            print("❌ Failed to load atlas: \(String(cString: error))")
        } else {
            print("❌ Failed to load atlas: unknown error")
        }
        spine_atlas_result_dispose(atlasResult)
        return
    }
    spine_atlas_result_dispose(atlasResult)
    print("✓ Atlas loaded successfully")

    // Read skeleton JSON
    guard let skeletonJson = try? String(contentsOfFile: jsonPath, encoding: .utf8) else {
        print("❌ Failed to read skeleton JSON file: \(jsonPath)")
        spine_atlas_dispose(atlas)
        return
    }

    // Load skeleton data
    let skeletonDataResult = spine_skeleton_data_load_json(atlas, skeletonJson, jsonPath)
    guard let skeletonData = spine_skeleton_data_result_get_data(skeletonDataResult) else {
        if let error = spine_skeleton_data_result_get_error(skeletonDataResult) {
            print("❌ Failed to load skeleton data: \(String(cString: error))")
        } else {
            print("❌ Failed to load skeleton data: unknown error")
        }
        spine_skeleton_data_result_dispose(skeletonDataResult)
        spine_atlas_dispose(atlas)
        return
    }
    spine_skeleton_data_result_dispose(skeletonDataResult)
    print("✓ Skeleton data loaded successfully")

    // Create skeleton drawable
    let drawable = spine_skeleton_drawable_create(skeletonData)
    print("✓ SkeletonDrawable created successfully")

    // Get skeleton and animation state
    let skeleton = spine_skeleton_drawable_get_skeleton(drawable)
    let animationState = spine_skeleton_drawable_get_animation_state(drawable)

    // Test skeleton bounds
    print("\nTesting skeleton bounds:")
    let boundsArray = spine_array_float_create_with_capacity(4)
    spine_skeleton_get_bounds(skeleton, boundsArray)
    if let buffer = spine_array_float_buffer(boundsArray), spine_array_float_size(boundsArray) == 4 {
        let x = buffer[0]
        let y = buffer[1]
        let width = buffer[2]
        let height = buffer[3]
        print("  Initial bounds: x=\(x), y=\(y), width=\(width), height=\(height)")
    }

    // Set skeleton to pose and update bounds
    spine_skeleton_setup_pose(skeleton)
    spine_skeleton_update_world_transform(skeleton, SPINE_PHYSICS_NONE)

    spine_skeleton_get_bounds(skeleton, boundsArray)
    if let buffer = spine_array_float_buffer(boundsArray), spine_array_float_size(boundsArray) == 4 {
        let x = buffer[0]
        let y = buffer[1]
        let width = buffer[2]
        let height = buffer[3]
        print("  Bounds after setupPose: x=\(x), y=\(y), width=\(width), height=\(height)")
    }

    // Set an animation and setup track entry listener
    let trackEntry = spine_animation_state_set_animation_1(animationState, 0, "walk", true)
    print("✓ Set animation: walk")

    // Test track entry properties
    if let trackEntry = trackEntry {
        let trackIndex = spine_track_entry_get_track_index(trackEntry)
        let loop = spine_track_entry_get_loop(trackEntry)
        print("  Track entry: index=\(trackIndex), loop=\(loop)")
    }

    // Update several times to trigger events
    print("\nUpdating animation state...")
    for i in 0..<5 {
        spine_animation_state_update(animationState, 0.016)  // ~60fps
        spine_animation_state_apply(animationState, skeleton)

        // Check for events
        let events = spine_skeleton_drawable_get_animation_state_events(drawable)
        let numEvents = spine_animation_state_events_get_num_events(events)
        if numEvents > 0 {
            print("  Frame \(i): \(numEvents) event(s)")
            for j in 0..<numEvents {
                let eventType = spine_animation_state_events_get_event_type(events, j)
                if let event = spine_animation_state_events_get_event(events, j) {
                    let eventData = spine_event_get_data(event)
                    let eventName = spine_event_data_get_name(eventData)
                    print("    Event type: \(eventType), name: \(String(cString: eventName!))")
                }
            }
        }
        spine_animation_state_events_reset(events)
    }

    // Test switching animations
    print("\nSwitching to run animation...")
    spine_animation_state_set_animation_1(animationState, 0, "run", true)

    // Update a few more times
    for i in 0..<3 {
        spine_animation_state_update(animationState, 0.016)
        spine_animation_state_apply(animationState, skeleton)

        let events = spine_skeleton_drawable_get_animation_state_events(drawable)
        let numEvents = spine_animation_state_events_get_num_events(events)
        if numEvents > 0 {
            print("  Frame \(i): \(numEvents) event(s) after switching")
        }
        spine_animation_state_events_reset(events)
    }

    // Test bounds after animation updates
    print("\nTesting bounds after animation:")
    spine_skeleton_update_world_transform(skeleton, SPINE_PHYSICS_NONE)
    spine_skeleton_get_bounds(skeleton, boundsArray)
    if let buffer = spine_array_float_buffer(boundsArray), spine_array_float_size(boundsArray) == 4 {
        let x = buffer[0]
        let y = buffer[1]
        let width = buffer[2]
        let height = buffer[3]
        print("  Bounds after animation: x=\(x), y=\(y), width=\(width), height=\(height)")
    }

    // Test with different animations that might have different bounds
    print("\nTesting bounds with jump animation:")
    spine_animation_state_set_animation_1(animationState, 0, "jump", false)
    spine_animation_state_update(animationState, 0.5)  // Update to middle of jump
    spine_animation_state_apply(animationState, skeleton)
    spine_skeleton_update_world_transform(skeleton, SPINE_PHYSICS_NONE)

    spine_skeleton_get_bounds(skeleton, boundsArray)
    if let buffer = spine_array_float_buffer(boundsArray), spine_array_float_size(boundsArray) == 4 {
        let x = buffer[0]
        let y = buffer[1]
        let width = buffer[2]
        let height = buffer[3]
        print("  Bounds during jump: x=\(x), y=\(y), width=\(width), height=\(height)")
    }

    // Cleanup
    spine_array_float_dispose(boundsArray)
    spine_skeleton_drawable_dispose(drawable)
    spine_skeleton_data_dispose(skeletonData)
    spine_atlas_dispose(atlas)

    // Report memory leaks if debug extension is enabled
    spine_report_leaks()

    print("\n✓ SpineC API Test complete")
}

// Test function is called from main.swift
