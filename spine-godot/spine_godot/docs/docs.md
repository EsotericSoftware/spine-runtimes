# Spine Godot Documentation Update Guide

## Overview
This document serves as a reusable prompt for updating the Spine Godot documentation. The documentation consists of XML files in `spine_godot/docs/` that document the user-facing API classes exposed to GDScript.

## Key Rule
**Any `SpineXXX.h` class that contains a `_bind_methods()` declaration must be documented**, as this indicates the class is exposed to GDScript users.

## Instructions for Documentation Update

**IMPORTANT: DO NOT USE TASK AGENTS FOR THIS WORK!** You must work on documentation sequentially, one class at a time, checking off completed items as you go.

### Step 1: Analyze Current State
Before updating documentation, you must:
1. Run: `grep -l "_bind_methods" spine_godot/Spine*.h | sort` to find all classes that need documentation
2. List all existing `.xml` files in `spine_godot/docs/`
3. Compare the two lists to identify missing documentation
4. Update the checklist below based on current state

### Step 2: Documentation Sources
For each class that needs documentation:
1. **Primary source**: The Godot wrapper header in `spine_godot/SpineXXX.h` - check the `_bind_methods()` implementation in the corresponding `.cpp` file to see what's exposed
2. **CRITICAL source for documentation text**: The corresponding spine-cpp header in `spine_godot/spine-cpp/include/spine/*.h` - **COPY AND ADAPT THE DOCUMENTATION FROM HERE**
3. **Implementation details**: The corresponding `.cpp` file if needed for understanding behavior

### Step 3: Update Process (WORK SEQUENTIALLY)
For each class in the checklist, working one at a time:
1. Read the wrapper's `_bind_methods()` to identify all exposed methods, properties, signals, and constants
2. **READ THE SPINE-CPP HEADER FILE AND COPY THE DOCUMENTATION COMMENTS** - Look for:
   - Class-level documentation (usually after `class ClassName`)
   - Method documentation (usually comments above or near methods)
   - Member variable documentation (inline comments or above declarations)
3. **ADAPT the spine-cpp documentation to GDScript**:
   - Convert C++ terminology to GDScript terminology
   - Adjust examples to use GDScript syntax
   - Keep the technical accuracy and detail from the original documentation
4. For Godot-specific additions (signals, properties not in spine-cpp), document based on implementation
5. Create or update the XML documentation following Godot's documentation format
6. Ensure all bound methods, properties, signals, and enum constants are documented
7. **Check off the item in the checklist after completing its documentation**

### CRITICAL RULE FOR DOCUMENTATION
**DO NOT WRITE GENERIC OR VAGUE DOCUMENTATION!** You must:
- Copy the actual documentation from spine-cpp headers
- Preserve technical details about what each method/property does
- Explain the purpose and behavior as documented in spine-cpp
- Only write your own documentation for Godot-specific features not in spine-cpp

## Documentation Checklist

**Instructions**: Update this checklist every time before processing documentation updates. Check the box [x] when documentation is complete and up-to-date.

### Classes with Existing Documentation
Based on files in `spine_godot/docs/`:
- [ ] **SpineAnimation** - `spine_godot/SpineAnimation.h` → `spine-cpp/include/spine/Animation.h`
- [ ] **SpineAnimationState** - `spine_godot/SpineAnimationState.h` → `spine-cpp/include/spine/AnimationState.h`
- [ ] **SpineAnimationTrack** - `spine_godot/SpineAnimationTrack.h` → (Godot wrapper specific)
- [ ] **SpineAtlasResource** - `spine_godot/SpineAtlasResource.h` → `spine-cpp/include/spine/Atlas.h`
- [ ] **SpineAttachment** - `spine_godot/SpineAttachment.h` → `spine-cpp/include/spine/Attachment.h`
- [ ] **SpineBone** - `spine_godot/SpineBone.h` → `spine-cpp/include/spine/Bone.h`
- [ ] **SpineBoneData** - `spine_godot/SpineBoneData.h` → `spine-cpp/include/spine/BoneData.h`
- [ ] **SpineBoneNode2D** - `spine_godot/SpineBoneNode2D.h` → (Godot node wrapper)
- [ ] **SpineConstraintData** - `spine_godot/SpineConstraintData.h` → `spine-cpp/include/spine/ConstraintData.h`
- [ ] **SpineEvent** - `spine_godot/SpineEvent.h` → `spine-cpp/include/spine/Event.h`
- [ ] **SpineIkConstraint** - `spine_godot/SpineIkConstraint.h` → `spine-cpp/include/spine/IkConstraint.h`
- [ ] **SpineIkConstraintData** - `spine_godot/SpineIkConstraintData.h` → `spine-cpp/include/spine/IkConstraintData.h`
- [ ] **SpinePathConstraint** - `spine_godot/SpinePathConstraint.h` → `spine-cpp/include/spine/PathConstraint.h`
- [ ] **SpinePathConstraintData** - `spine_godot/SpinePathConstraintData.h` → `spine-cpp/include/spine/PathConstraintData.h`
- [ ] **SpineSkeleton** - `spine_godot/SpineSkeleton.h` → `spine-cpp/include/spine/Skeleton.h`
- [ ] **SpineSkeletonDataResource** - `spine_godot/SpineSkeletonDataResource.h` → `spine-cpp/include/spine/SkeletonData.h`
- [ ] **SpineSkeletonFileResource** - `spine_godot/SpineSkeletonFileResource.h` → (Godot resource loader)
- [ ] **SpineSkin** - `spine_godot/SpineSkin.h` → `spine-cpp/include/spine/Skin.h`
- [ ] **SpineSlot** - `spine_godot/SpineSlot.h` → `spine-cpp/include/spine/Slot.h`
- [ ] **SpineSlotData** - `spine_godot/SpineSlotData.h` → `spine-cpp/include/spine/SlotData.h`
- [ ] **SpineSlotNode2D** - `spine_godot/SpineSlotNode2D.h` → (Godot node wrapper)
- [ ] **SpineSprite2D** - `spine_godot/SpineSprite2D.h` → (Main Godot node for Spine animations)
- [ ] **SpineTimeline** - `spine_godot/SpineTimeline.h` → `spine-cpp/include/spine/Timeline.h`
- [ ] **SpineTrackEntry** - `spine_godot/SpineTrackEntry.h` → (Part of AnimationState)
- [ ] **SpineTransformConstraint** - `spine_godot/SpineTransformConstraint.h` → `spine-cpp/include/spine/TransformConstraint.h`
- [ ] **SpineTransformConstraintData** - `spine_godot/SpineTransformConstraintData.h` → `spine-cpp/include/spine/TransformConstraintData.h`

### Classes Missing Documentation (Have _bind_methods but no .xml file)
These classes have `_bind_methods()` in their headers but lack documentation:

#### Data Classes
- [ ] **SpineEventData** - `spine_godot/SpineEventData.h` → `spine-cpp/include/spine/EventData.h`
  - Setup pose data for events

#### Physics Classes (New in 4.x)
- [ ] **SpinePhysicsConstraint** - `spine_godot/SpinePhysicsConstraint.h` → `spine-cpp/include/spine/PhysicsConstraint.h`
  - Runtime physics constraint
- [ ] **SpinePhysicsConstraintData** - `spine_godot/SpinePhysicsConstraintData.h` → `spine-cpp/include/spine/PhysicsConstraintData.h`
  - Setup data for physics constraints
- [ ] **SpinePhysicsConstraintPose** - `spine_godot/SpinePhysicsConstraintPose.h` → `spine-cpp/include/spine/PhysicsConstraintPose.h`
  - Physics constraint pose state

#### Slider Classes (New in 4.x)
- [ ] **SpineSlider** - `spine_godot/SpineSlider.h` → `spine-cpp/include/spine/Slider.h`
  - Runtime slider for property animation
- [ ] **SpineSliderData** - `spine_godot/SpineSliderData.h` → `spine-cpp/include/spine/SliderData.h`
  - Setup data for sliders
- [ ] **SpineSliderPose** - `spine_godot/SpineSliderPose.h` → `spine-cpp/include/spine/SliderPose.h`
  - Slider pose state

#### Pose Classes
- [x] **SpineBoneLocal** - `spine_godot/SpineBoneLocal.h` → `spine-cpp/include/spine/BoneLocal.h`
  - Local bone transform data
- [x] **SpineBonePose** - `spine_godot/SpineBonePose.h` → `spine-cpp/include/spine/BonePose.h`
  - Bone pose state
- [ ] **SpineIkConstraintPose** - `spine_godot/SpineIkConstraintPose.h` → `spine-cpp/include/spine/IkConstraintPose.h`
  - IK constraint pose state
- [ ] **SpinePathConstraintPose** - `spine_godot/SpinePathConstraintPose.h` → `spine-cpp/include/spine/PathConstraintPose.h`
  - Path constraint pose state
- [ ] **SpineSlotPose** - `spine_godot/SpineSlotPose.h` → `spine-cpp/include/spine/SlotPose.h`
  - Slot pose state
- [ ] **SpineTransformConstraintPose** - `spine_godot/SpineTransformConstraintPose.h` → `spine-cpp/include/spine/TransformConstraintPose.h`
  - Transform constraint pose state

#### Constants/Enums Class
- [ ] **SpineConstant** - `spine_godot/SpineConstant.h` → (Godot-specific constants)
  - Exposes Spine enums and constants to GDScript (MixBlend, MixDirection, Property, etc.)

### Classes to Skip (Internal/Editor Only)
These have `_bind_methods()` but should NOT have user-facing documentation:
- **SpineCommon** - Internal base classes and utilities
- **SpineEditorPlugin** - Editor-only functionality

## Documentation Template

When creating new documentation files, use this template:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<class name="SpineClassName" inherits="RefCounted" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="../../../doc/class.xsd">
	<brief_description>
		One-line description of the class purpose.
	</brief_description>
	<description>
		Detailed description of the class.
		Include usage context and relationship to other Spine classes.
		For wrapper classes, explain what Spine runtime feature this exposes.
	</description>
	<methods>
		<method name="method_name">
			<return type="return_type" />
			<param index="0" name="param_name" type="param_type" />
			<description>
				Method description. Check _bind_methods() for exact signature.
			</description>
		</method>
	</methods>
	<members>
		<member name="property_name" type="property_type" setter="set_property" getter="get_property">
			Property description. Must match ADD_PROPERTY in _bind_methods().
		</member>
	</members>
	<signals>
		<signal name="signal_name">
			<param index="0" name="param_name" type="param_type" />
			<description>
				Signal description. Must match ADD_SIGNAL in _bind_methods().
			</description>
		</signal>
	</signals>
	<constants>
		<constant name="CONSTANT_NAME" value="0" enum="EnumName">
			Constant description. Must match BIND_ENUM_CONSTANT in _bind_methods().
		</constant>
	</constants>
</class>
```

## Validation Steps

After updating documentation:
1. Verify every method in `_bind_methods()` has corresponding documentation
2. Check that all `ADD_PROPERTY` calls have corresponding member documentation
3. Verify all `ADD_SIGNAL` calls have corresponding signal documentation
4. Ensure all `BIND_ENUM_CONSTANT` calls have corresponding constant documentation
5. Test that property setters/getters match the actual implementation
6. Validate that examples (if provided) are valid GDScript code

## Important Notes

### Identifying What to Document
- Run `grep -l "_bind_methods" spine_godot/Spine*.h` to get the definitive list
- The presence of `_bind_methods()` is the key indicator that a class needs documentation
- Even if a class seems internal, if it has `_bind_methods()` it's exposed to users

### Class Categories
1. **Core Runtime Classes**: SpineSkeleton, SpineAnimation, SpineBone, etc.
2. **Data Classes**: Classes ending in "Data" that hold setup/configuration
3. **Pose Classes**: Classes ending in "Pose" that represent runtime state
4. **Constraint Classes**: IK, Path, Transform, Physics constraints
5. **Resource Classes**: Atlas, SkeletonData, SkeletonFile resources
6. **Node Classes**: SpineSprite2D, SpineBoneNode2D, SpineSlotNode2D (Godot scene nodes)
7. **Constants Class**: SpineConstant (enum definitions)

### Version Considerations
- Physics constraints and sliders are new in Spine 4.x
- Some features may not be available in all Spine versions
- Documentation should note version requirements where applicable

## Running the Documentation Update

To use this guide:
1. Run `grep -l "_bind_methods" spine_godot/Spine*.h | sort` to get current list
2. Compare with existing .xml files in `spine_godot/docs/`
3. Update this checklist based on findings
4. For each unchecked item, create or update the XML documentation
5. Focus on the `_bind_methods()` implementation to ensure accuracy
6. Mark items as complete when documentation is finished
7. Commit with message indicating which classes were documented

Remember: The presence of `_bind_methods()` is the definitive indicator that a class needs documentation.