package spine.utils;

import haxe.ds.ObjectMap;
import spine.*;
import spine.animation.*;
import spine.attachments.*;

class SkeletonSerializer {
	private var visitedObjects:ObjectMap<Dynamic, String> = new ObjectMap();
	private var nextId:Int = 1;
	private var json:JsonWriter;

	public function new() {}

	public function serializeSkeletonData(data:SkeletonData):String {
		visitedObjects = new ObjectMap();
		nextId = 1;
		json = new JsonWriter();
		writeSkeletonData(data);
		return json.getString();
	}

	public function serializeSkeleton(skeleton:Skeleton):String {
		visitedObjects = new ObjectMap();
		nextId = 1;
		json = new JsonWriter();
		writeSkeleton(skeleton);
		return json.getString();
	}

	public function serializeAnimationState(state:AnimationState):String {
		visitedObjects = new ObjectMap();
		nextId = 1;
		json = new JsonWriter();
		writeAnimationState(state);
		return json.getString();
	}

	// Core reflection helpers
	private function extractPropertyName(javaGetter:String):String {
		var getter = javaGetter;
		if (getter.indexOf("()") != -1)
			getter = getter.substr(0, getter.length - 2);

		if (getter.substr(0, 3) == "get") {
			var prop = getter.substr(3);
			return prop.charAt(0).toLowerCase() + prop.substr(1);
		}
		if (getter.substr(0, 2) == "is") {
			var prop = getter.substr(2);
			return prop.charAt(0).toLowerCase() + prop.substr(1);
		}
		return getter;
	}

	private function getSpecialFieldNames(javaGetter:String):Array<String> {
		return switch (javaGetter) {
			case "getInt()": ["intValue"];
			case "getFloat()": ["floatValue"];
			case "getString()": ["stringValue"];
			case "getPhysicsConstraints()": ["physics"];
			case "getUpdateCache()": ["_updateCache"];
			case "getSetupPose()": ["setup"];
			case "getAppliedPose()": ["applied"];
			default: [];
		}
	}

	private function getPropertyValue(obj:Dynamic, javaGetter:String):Dynamic {
		var propName = extractPropertyName(javaGetter);

		// Try direct field access first
		if (Reflect.hasField(obj, propName)) {
			return Reflect.field(obj, propName);
		}

		// Try special field variations
		var specialNames = getSpecialFieldNames(javaGetter);
		for (name in specialNames) {
			if (Reflect.hasField(obj, name)) {
				return Reflect.field(obj, name);
			}
		}

		// Try method access (remove parentheses)
		var methodName = javaGetter;
		if (methodName.indexOf("()") != -1) {
			methodName = methodName.substr(0, methodName.length - 2);
		}
		if (Reflect.hasField(obj, methodName)) {
			var method = Reflect.field(obj, methodName);
			if (Reflect.isFunction(method)) {
				return Reflect.callMethod(obj, method, []);
			}
		}

		// Last resort: return null and let the caller handle it
		return null;
	}

	private function writeAnimation(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<Animation-" + nameValue + ">" : "<Animation-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Animation");

		json.writeName("timelines");
		writeProperty(obj, "getTimelines()", {
			"kind": "array",
			"name": "timelines",
			"getter": "getTimelines()",
			"elementType": "Timeline",
			"elementKind": "object",
			"writeMethodCall": "writeTimeline",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "object",
			"name": "bones",
			"getter": "getBones()",
			"valueType": "IntArray",
			"writeMethodCall": "writeIntArray",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeAlphaTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<AlphaTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("AlphaTimeline");

		json.writeName("slotIndex");
		writeProperty(obj, "getSlotIndex()", {
			"kind": "primitive",
			"name": "slotIndex",
			"getter": "getSlotIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeAttachmentTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<AttachmentTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("AttachmentTimeline");

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("slotIndex");
		writeProperty(obj, "getSlotIndex()", {
			"kind": "primitive",
			"name": "slotIndex",
			"getter": "getSlotIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("attachmentNames");
		writeProperty(obj, "getAttachmentNames()", {
			"kind": "array",
			"name": "attachmentNames",
			"getter": "getAttachmentNames()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeDeformTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<DeformTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("DeformTimeline");

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("attachment");
		writeProperty(obj, "getAttachment()", {
			"kind": "object",
			"name": "attachment",
			"getter": "getAttachment()",
			"valueType": "VertexAttachment",
			"writeMethodCall": "writeVertexAttachment",
			"isNullable": false
		});

		json.writeName("vertices");
		writeProperty(obj, "getVertices()", {
			"kind": "nestedArray",
			"name": "vertices",
			"getter": "getVertices()",
			"elementType": "float",
			"isNullable": false
		});

		json.writeName("slotIndex");
		writeProperty(obj, "getSlotIndex()", {
			"kind": "primitive",
			"name": "slotIndex",
			"getter": "getSlotIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeDrawOrderTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<DrawOrderTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("DrawOrderTimeline");

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("drawOrders");
		writeProperty(obj, "getDrawOrders()", {
			"kind": "nestedArray",
			"name": "drawOrders",
			"getter": "getDrawOrders()",
			"elementType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeEventTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<EventTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("EventTimeline");

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("events");
		writeProperty(obj, "getEvents()", {
			"kind": "array",
			"name": "events",
			"getter": "getEvents()",
			"elementType": "Event",
			"elementKind": "object",
			"writeMethodCall": "writeEvent",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeIkConstraintTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<IkConstraintTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("IkConstraintTimeline");

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeInheritTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<InheritTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("InheritTimeline");

		json.writeName("boneIndex");
		writeProperty(obj, "getBoneIndex()", {
			"kind": "primitive",
			"name": "boneIndex",
			"getter": "getBoneIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePathConstraintMixTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PathConstraintMixTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathConstraintMixTimeline");

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePathConstraintPositionTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PathConstraintPositionTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathConstraintPositionTimeline");

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePathConstraintSpacingTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PathConstraintSpacingTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathConstraintSpacingTimeline");

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePhysicsConstraintDampingTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PhysicsConstraintDampingTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintDampingTimeline");

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePhysicsConstraintGravityTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PhysicsConstraintGravityTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintGravityTimeline");

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePhysicsConstraintInertiaTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PhysicsConstraintInertiaTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintInertiaTimeline");

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePhysicsConstraintMassTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PhysicsConstraintMassTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintMassTimeline");

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePhysicsConstraintMixTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PhysicsConstraintMixTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintMixTimeline");

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePhysicsConstraintResetTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PhysicsConstraintResetTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintResetTimeline");

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePhysicsConstraintStrengthTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PhysicsConstraintStrengthTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintStrengthTimeline");

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePhysicsConstraintWindTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PhysicsConstraintWindTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintWindTimeline");

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeRGB2Timeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<RGB2Timeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("RGB2Timeline");

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("slotIndex");
		writeProperty(obj, "getSlotIndex()", {
			"kind": "primitive",
			"name": "slotIndex",
			"getter": "getSlotIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeRGBA2Timeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<RGBA2Timeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("RGBA2Timeline");

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("slotIndex");
		writeProperty(obj, "getSlotIndex()", {
			"kind": "primitive",
			"name": "slotIndex",
			"getter": "getSlotIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeRGBATimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<RGBATimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("RGBATimeline");

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("slotIndex");
		writeProperty(obj, "getSlotIndex()", {
			"kind": "primitive",
			"name": "slotIndex",
			"getter": "getSlotIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeRGBTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<RGBTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("RGBTimeline");

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("slotIndex");
		writeProperty(obj, "getSlotIndex()", {
			"kind": "primitive",
			"name": "slotIndex",
			"getter": "getSlotIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeRotateTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<RotateTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("RotateTimeline");

		json.writeName("boneIndex");
		writeProperty(obj, "getBoneIndex()", {
			"kind": "primitive",
			"name": "boneIndex",
			"getter": "getBoneIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeScaleTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<ScaleTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ScaleTimeline");

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("boneIndex");
		writeProperty(obj, "getBoneIndex()", {
			"kind": "primitive",
			"name": "boneIndex",
			"getter": "getBoneIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeScaleXTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<ScaleXTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ScaleXTimeline");

		json.writeName("boneIndex");
		writeProperty(obj, "getBoneIndex()", {
			"kind": "primitive",
			"name": "boneIndex",
			"getter": "getBoneIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeScaleYTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<ScaleYTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ScaleYTimeline");

		json.writeName("boneIndex");
		writeProperty(obj, "getBoneIndex()", {
			"kind": "primitive",
			"name": "boneIndex",
			"getter": "getBoneIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSequenceTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<SequenceTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SequenceTimeline");

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("slotIndex");
		writeProperty(obj, "getSlotIndex()", {
			"kind": "primitive",
			"name": "slotIndex",
			"getter": "getSlotIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("attachment");
		writeProperty(obj, "getAttachment()", {
			"kind": "object",
			"name": "attachment",
			"getter": "getAttachment()",
			"valueType": "Attachment",
			"writeMethodCall": "writeAttachment",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeShearTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<ShearTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ShearTimeline");

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("boneIndex");
		writeProperty(obj, "getBoneIndex()", {
			"kind": "primitive",
			"name": "boneIndex",
			"getter": "getBoneIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeShearXTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<ShearXTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ShearXTimeline");

		json.writeName("boneIndex");
		writeProperty(obj, "getBoneIndex()", {
			"kind": "primitive",
			"name": "boneIndex",
			"getter": "getBoneIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeShearYTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<ShearYTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ShearYTimeline");

		json.writeName("boneIndex");
		writeProperty(obj, "getBoneIndex()", {
			"kind": "primitive",
			"name": "boneIndex",
			"getter": "getBoneIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSliderMixTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<SliderMixTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SliderMixTimeline");

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSliderTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<SliderTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SliderTimeline");

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeTimeline(obj:Dynamic):Void {
		if (Std.isOfType(obj, AlphaTimeline)) {
			writeAlphaTimeline(obj);
		} else if (Std.isOfType(obj, AttachmentTimeline)) {
			writeAttachmentTimeline(obj);
		} else if (Std.isOfType(obj, DeformTimeline)) {
			writeDeformTimeline(obj);
		} else if (Std.isOfType(obj, DrawOrderTimeline)) {
			writeDrawOrderTimeline(obj);
		} else if (Std.isOfType(obj, EventTimeline)) {
			writeEventTimeline(obj);
		} else if (Std.isOfType(obj, IkConstraintTimeline)) {
			writeIkConstraintTimeline(obj);
		} else if (Std.isOfType(obj, InheritTimeline)) {
			writeInheritTimeline(obj);
		} else if (Std.isOfType(obj, PathConstraintMixTimeline)) {
			writePathConstraintMixTimeline(obj);
		} else if (Std.isOfType(obj, PathConstraintPositionTimeline)) {
			writePathConstraintPositionTimeline(obj);
		} else if (Std.isOfType(obj, PathConstraintSpacingTimeline)) {
			writePathConstraintSpacingTimeline(obj);
		} else if (Std.isOfType(obj, PhysicsConstraintDampingTimeline)) {
			writePhysicsConstraintDampingTimeline(obj);
		} else if (Std.isOfType(obj, PhysicsConstraintGravityTimeline)) {
			writePhysicsConstraintGravityTimeline(obj);
		} else if (Std.isOfType(obj, PhysicsConstraintInertiaTimeline)) {
			writePhysicsConstraintInertiaTimeline(obj);
		} else if (Std.isOfType(obj, PhysicsConstraintMassTimeline)) {
			writePhysicsConstraintMassTimeline(obj);
		} else if (Std.isOfType(obj, PhysicsConstraintMixTimeline)) {
			writePhysicsConstraintMixTimeline(obj);
		} else if (Std.isOfType(obj, PhysicsConstraintResetTimeline)) {
			writePhysicsConstraintResetTimeline(obj);
		} else if (Std.isOfType(obj, PhysicsConstraintStrengthTimeline)) {
			writePhysicsConstraintStrengthTimeline(obj);
		} else if (Std.isOfType(obj, PhysicsConstraintWindTimeline)) {
			writePhysicsConstraintWindTimeline(obj);
		} else if (Std.isOfType(obj, RGB2Timeline)) {
			writeRGB2Timeline(obj);
		} else if (Std.isOfType(obj, RGBA2Timeline)) {
			writeRGBA2Timeline(obj);
		} else if (Std.isOfType(obj, RGBATimeline)) {
			writeRGBATimeline(obj);
		} else if (Std.isOfType(obj, RGBTimeline)) {
			writeRGBTimeline(obj);
		} else if (Std.isOfType(obj, RotateTimeline)) {
			writeRotateTimeline(obj);
		} else if (Std.isOfType(obj, ScaleTimeline)) {
			writeScaleTimeline(obj);
		} else if (Std.isOfType(obj, ScaleXTimeline)) {
			writeScaleXTimeline(obj);
		} else if (Std.isOfType(obj, ScaleYTimeline)) {
			writeScaleYTimeline(obj);
		} else if (Std.isOfType(obj, SequenceTimeline)) {
			writeSequenceTimeline(obj);
		} else if (Std.isOfType(obj, ShearTimeline)) {
			writeShearTimeline(obj);
		} else if (Std.isOfType(obj, ShearXTimeline)) {
			writeShearXTimeline(obj);
		} else if (Std.isOfType(obj, ShearYTimeline)) {
			writeShearYTimeline(obj);
		} else if (Std.isOfType(obj, SliderMixTimeline)) {
			writeSliderMixTimeline(obj);
		} else if (Std.isOfType(obj, SliderTimeline)) {
			writeSliderTimeline(obj);
		} else if (Std.isOfType(obj, TransformConstraintTimeline)) {
			writeTransformConstraintTimeline(obj);
		} else if (Std.isOfType(obj, TranslateTimeline)) {
			writeTranslateTimeline(obj);
		} else if (Std.isOfType(obj, TranslateXTimeline)) {
			writeTranslateXTimeline(obj);
		} else if (Std.isOfType(obj, TranslateYTimeline)) {
			writeTranslateYTimeline(obj);
		} else {
			throw new spine.SpineException("Unknown Timeline type");
		}
	}

	private function writeTransformConstraintTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<TransformConstraintTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TransformConstraintTimeline");

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("constraintIndex");
		writeProperty(obj, "getConstraintIndex()", {
			"kind": "primitive",
			"name": "constraintIndex",
			"getter": "getConstraintIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeTranslateTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<TranslateTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TranslateTimeline");

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("boneIndex");
		writeProperty(obj, "getBoneIndex()", {
			"kind": "primitive",
			"name": "boneIndex",
			"getter": "getBoneIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeTranslateXTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<TranslateXTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TranslateXTimeline");

		json.writeName("boneIndex");
		writeProperty(obj, "getBoneIndex()", {
			"kind": "primitive",
			"name": "boneIndex",
			"getter": "getBoneIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeTranslateYTimeline(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<TranslateYTimeline-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TranslateYTimeline");

		json.writeName("boneIndex");
		writeProperty(obj, "getBoneIndex()", {
			"kind": "primitive",
			"name": "boneIndex",
			"getter": "getBoneIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("frameEntries");
		writeProperty(obj, "getFrameEntries()", {
			"kind": "primitive",
			"name": "frameEntries",
			"getter": "getFrameEntries()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("propertyIds");
		writeProperty(obj, "getPropertyIds()", {
			"kind": "array",
			"name": "propertyIds",
			"getter": "getPropertyIds()",
			"elementType": "String",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frames");
		writeProperty(obj, "getFrames()", {
			"kind": "array",
			"name": "frames",
			"getter": "getFrames()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("frameCount");
		writeProperty(obj, "getFrameCount()", {
			"kind": "primitive",
			"name": "frameCount",
			"getter": "getFrameCount()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("duration");
		writeProperty(obj, "getDuration()", {
			"kind": "primitive",
			"name": "duration",
			"getter": "getDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeAnimationState(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<AnimationState-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("AnimationState");

		json.writeName("timeScale");
		writeProperty(obj, "getTimeScale()", {
			"kind": "primitive",
			"name": "timeScale",
			"getter": "getTimeScale()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("data");
		writeProperty(obj, "getData()", {
			"kind": "object",
			"name": "data",
			"getter": "getData()",
			"valueType": "AnimationStateData",
			"writeMethodCall": "writeAnimationStateData",
			"isNullable": false
		});

		json.writeName("tracks");
		writeProperty(obj, "getTracks()", {
			"kind": "array",
			"name": "tracks",
			"getter": "getTracks()",
			"elementType": "TrackEntry",
			"elementKind": "object",
			"writeMethodCall": "writeTrackEntry",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeTrackEntry(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<TrackEntry-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TrackEntry");

		json.writeName("trackIndex");
		writeProperty(obj, "getTrackIndex()", {
			"kind": "primitive",
			"name": "trackIndex",
			"getter": "getTrackIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("animation");
		writeProperty(obj, "getAnimation()", {
			"kind": "object",
			"name": "animation",
			"getter": "getAnimation()",
			"valueType": "Animation",
			"writeMethodCall": "writeAnimation",
			"isNullable": false
		});

		json.writeName("loop");
		writeProperty(obj, "getLoop()", {
			"kind": "primitive",
			"name": "loop",
			"getter": "getLoop()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("delay");
		writeProperty(obj, "getDelay()", {
			"kind": "primitive",
			"name": "delay",
			"getter": "getDelay()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("trackTime");
		writeProperty(obj, "getTrackTime()", {
			"kind": "primitive",
			"name": "trackTime",
			"getter": "getTrackTime()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("trackEnd");
		writeProperty(obj, "getTrackEnd()", {
			"kind": "primitive",
			"name": "trackEnd",
			"getter": "getTrackEnd()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("trackComplete");
		writeProperty(obj, "getTrackComplete()", {
			"kind": "primitive",
			"name": "trackComplete",
			"getter": "getTrackComplete()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("animationStart");
		writeProperty(obj, "getAnimationStart()", {
			"kind": "primitive",
			"name": "animationStart",
			"getter": "getAnimationStart()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("animationEnd");
		writeProperty(obj, "getAnimationEnd()", {
			"kind": "primitive",
			"name": "animationEnd",
			"getter": "getAnimationEnd()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("animationLast");
		writeProperty(obj, "getAnimationLast()", {
			"kind": "primitive",
			"name": "animationLast",
			"getter": "getAnimationLast()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("animationTime");
		writeProperty(obj, "getAnimationTime()", {
			"kind": "primitive",
			"name": "animationTime",
			"getter": "getAnimationTime()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("timeScale");
		writeProperty(obj, "getTimeScale()", {
			"kind": "primitive",
			"name": "timeScale",
			"getter": "getTimeScale()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("alpha");
		writeProperty(obj, "getAlpha()", {
			"kind": "primitive",
			"name": "alpha",
			"getter": "getAlpha()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("eventThreshold");
		writeProperty(obj, "getEventThreshold()", {
			"kind": "primitive",
			"name": "eventThreshold",
			"getter": "getEventThreshold()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("alphaAttachmentThreshold");
		writeProperty(obj, "getAlphaAttachmentThreshold()", {
			"kind": "primitive",
			"name": "alphaAttachmentThreshold",
			"getter": "getAlphaAttachmentThreshold()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("mixAttachmentThreshold");
		writeProperty(obj, "getMixAttachmentThreshold()", {
			"kind": "primitive",
			"name": "mixAttachmentThreshold",
			"getter": "getMixAttachmentThreshold()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("mixDrawOrderThreshold");
		writeProperty(obj, "getMixDrawOrderThreshold()", {
			"kind": "primitive",
			"name": "mixDrawOrderThreshold",
			"getter": "getMixDrawOrderThreshold()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("next");
		writeProperty(obj, "getNext()", {
			"kind": "object",
			"name": "next",
			"getter": "getNext()",
			"valueType": "AnimationState.TrackEntry",
			"writeMethodCall": "writeTrackEntry",
			"isNullable": true
		});

		json.writeName("previous");
		writeProperty(obj, "getPrevious()", {
			"kind": "object",
			"name": "previous",
			"getter": "getPrevious()",
			"valueType": "AnimationState.TrackEntry",
			"writeMethodCall": "writeTrackEntry",
			"isNullable": true
		});

		json.writeName("mixTime");
		writeProperty(obj, "getMixTime()", {
			"kind": "primitive",
			"name": "mixTime",
			"getter": "getMixTime()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("mixDuration");
		writeProperty(obj, "getMixDuration()", {
			"kind": "primitive",
			"name": "mixDuration",
			"getter": "getMixDuration()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("additive");
		writeProperty(obj, "additive", {
			"kind": "bool",
			"name": "additive",
			"getter": "additive",
			"isNullable": false
		});

		json.writeName("mixingFrom");
		writeProperty(obj, "getMixingFrom()", {
			"kind": "object",
			"name": "mixingFrom",
			"getter": "getMixingFrom()",
			"valueType": "AnimationState.TrackEntry",
			"writeMethodCall": "writeTrackEntry",
			"isNullable": true
		});

		json.writeName("mixingTo");
		writeProperty(obj, "getMixingTo()", {
			"kind": "object",
			"name": "mixingTo",
			"getter": "getMixingTo()",
			"valueType": "AnimationState.TrackEntry",
			"writeMethodCall": "writeTrackEntry",
			"isNullable": true
		});

		json.writeName("holdPrevious");
		writeProperty(obj, "getHoldPrevious()", {
			"kind": "primitive",
			"name": "holdPrevious",
			"getter": "getHoldPrevious()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("shortestRotation");
		writeProperty(obj, "getShortestRotation()", {
			"kind": "primitive",
			"name": "shortestRotation",
			"getter": "getShortestRotation()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("reverse");
		writeProperty(obj, "getReverse()", {
			"kind": "primitive",
			"name": "reverse",
			"getter": "getReverse()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeAnimationStateData(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<AnimationStateData-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("AnimationStateData");

		json.writeName("skeletonData");
		writeProperty(obj, "getSkeletonData()", {
			"kind": "object",
			"name": "skeletonData",
			"getter": "getSkeletonData()",
			"valueType": "SkeletonData",
			"writeMethodCall": "writeSkeletonData",
			"isNullable": false
		});

		json.writeName("defaultMix");
		writeProperty(obj, "getDefaultMix()", {
			"kind": "primitive",
			"name": "defaultMix",
			"getter": "getDefaultMix()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeAttachment(obj:Dynamic):Void {
		if (Std.isOfType(obj, BoundingBoxAttachment)) {
			writeBoundingBoxAttachment(obj);
		} else if (Std.isOfType(obj, ClippingAttachment)) {
			writeClippingAttachment(obj);
		} else if (Std.isOfType(obj, MeshAttachment)) {
			writeMeshAttachment(obj);
		} else if (Std.isOfType(obj, PathAttachment)) {
			writePathAttachment(obj);
		} else if (Std.isOfType(obj, PointAttachment)) {
			writePointAttachment(obj);
		} else if (Std.isOfType(obj, RegionAttachment)) {
			writeRegionAttachment(obj);
		} else {
			throw new spine.SpineException("Unknown Attachment type");
		}
	}

	private function writeBone(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<Bone-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Bone");

		json.writeName("parent");
		writeProperty(obj, "getParent()", {
			"kind": "object",
			"name": "parent",
			"getter": "getParent()",
			"valueType": "Bone",
			"writeMethodCall": "writeBone",
			"isNullable": true
		});

		json.writeName("children");
		writeProperty(obj, "getChildren()", {
			"kind": "array",
			"name": "children",
			"getter": "getChildren()",
			"elementType": "Bone",
			"elementKind": "object",
			"writeMethodCall": "writeBone",
			"isNullable": false
		});

		json.writeName("data");
		writeProperty(obj, "getData()", {
			"kind": "object",
			"name": "data",
			"getter": "getData()",
			"valueType": "BoneData",
			"writeMethodCall": "writeBoneData",
			"isNullable": false
		});

		json.writeName("pose");
		writeProperty(obj, "getPose()", {
			"kind": "object",
			"name": "pose",
			"getter": "getPose()",
			"valueType": "BoneLocal",
			"writeMethodCall": "writeBoneLocal",
			"isNullable": false
		});

		json.writeName("appliedPose");
		writeProperty(obj, "getAppliedPose()", {
			"kind": "object",
			"name": "appliedPose",
			"getter": "getAppliedPose()",
			"valueType": "BonePose",
			"writeMethodCall": "writeBonePose",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeBoneData(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<BoneData-" + nameValue + ">" : "<BoneData-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("BoneData");

		json.writeName("index");
		writeProperty(obj, "getIndex()", {
			"kind": "primitive",
			"name": "index",
			"getter": "getIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("parent");
		writeProperty(obj, "getParent()", {
			"kind": "object",
			"name": "parent",
			"getter": "getParent()",
			"valueType": "BoneData",
			"writeMethodCall": "writeBoneData",
			"isNullable": true
		});

		json.writeName("length");
		writeProperty(obj, "getLength()", {
			"kind": "primitive",
			"name": "length",
			"getter": "getLength()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("color");
		writeProperty(obj, "getColor()", {
			"kind": "object",
			"name": "color",
			"getter": "getColor()",
			"valueType": "Color",
			"writeMethodCall": "writeColor",
			"isNullable": false
		});

		json.writeName("icon");
		writeProperty(obj, "getIcon()", {
			"kind": "primitive",
			"name": "icon",
			"getter": "getIcon()",
			"valueType": "String",
			"isNullable": true
		});

		json.writeName("visible");
		writeProperty(obj, "getVisible()", {
			"kind": "primitive",
			"name": "visible",
			"getter": "getVisible()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("setupPose");
		writeProperty(obj, "getSetupPose()", {
			"kind": "object",
			"name": "setupPose",
			"getter": "getSetupPose()",
			"valueType": "BoneLocal",
			"writeMethodCall": "writeBoneLocal",
			"isNullable": false
		});

		json.writeName("skinRequired");
		writeProperty(obj, "getSkinRequired()", {
			"kind": "primitive",
			"name": "skinRequired",
			"getter": "getSkinRequired()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeBoneLocal(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<BoneLocal-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("BoneLocal");

		json.writeName("x");
		writeProperty(obj, "getX()", {
			"kind": "primitive",
			"name": "x",
			"getter": "getX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("y");
		writeProperty(obj, "getY()", {
			"kind": "primitive",
			"name": "y",
			"getter": "getY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("rotation");
		writeProperty(obj, "getRotation()", {
			"kind": "primitive",
			"name": "rotation",
			"getter": "getRotation()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scaleX");
		writeProperty(obj, "getScaleX()", {
			"kind": "primitive",
			"name": "scaleX",
			"getter": "getScaleX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scaleY");
		writeProperty(obj, "getScaleY()", {
			"kind": "primitive",
			"name": "scaleY",
			"getter": "getScaleY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("shearX");
		writeProperty(obj, "getShearX()", {
			"kind": "primitive",
			"name": "shearX",
			"getter": "getShearX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("shearY");
		writeProperty(obj, "getShearY()", {
			"kind": "primitive",
			"name": "shearY",
			"getter": "getShearY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("inherit");
		writeProperty(obj, "getInherit()", {
			"kind": "enum",
			"name": "inherit",
			"getter": "getInherit()",
			"enumName": "Inherit",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeBonePose(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<BonePose-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("BonePose");

		json.writeName("a");
		writeProperty(obj, "getA()", {
			"kind": "primitive",
			"name": "a",
			"getter": "getA()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("b");
		writeProperty(obj, "getB()", {
			"kind": "primitive",
			"name": "b",
			"getter": "getB()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("c");
		writeProperty(obj, "getC()", {
			"kind": "primitive",
			"name": "c",
			"getter": "getC()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("d");
		writeProperty(obj, "getD()", {
			"kind": "primitive",
			"name": "d",
			"getter": "getD()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("worldX");
		writeProperty(obj, "getWorldX()", {
			"kind": "primitive",
			"name": "worldX",
			"getter": "getWorldX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("worldY");
		writeProperty(obj, "getWorldY()", {
			"kind": "primitive",
			"name": "worldY",
			"getter": "getWorldY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("worldRotationX");
		writeProperty(obj, "getWorldRotationX()", {
			"kind": "primitive",
			"name": "worldRotationX",
			"getter": "getWorldRotationX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("worldRotationY");
		writeProperty(obj, "getWorldRotationY()", {
			"kind": "primitive",
			"name": "worldRotationY",
			"getter": "getWorldRotationY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("worldScaleX");
		writeProperty(obj, "getWorldScaleX()", {
			"kind": "primitive",
			"name": "worldScaleX",
			"getter": "getWorldScaleX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("worldScaleY");
		writeProperty(obj, "getWorldScaleY()", {
			"kind": "primitive",
			"name": "worldScaleY",
			"getter": "getWorldScaleY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("x");
		writeProperty(obj, "getX()", {
			"kind": "primitive",
			"name": "x",
			"getter": "getX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("y");
		writeProperty(obj, "getY()", {
			"kind": "primitive",
			"name": "y",
			"getter": "getY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("rotation");
		writeProperty(obj, "getRotation()", {
			"kind": "primitive",
			"name": "rotation",
			"getter": "getRotation()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scaleX");
		writeProperty(obj, "getScaleX()", {
			"kind": "primitive",
			"name": "scaleX",
			"getter": "getScaleX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scaleY");
		writeProperty(obj, "getScaleY()", {
			"kind": "primitive",
			"name": "scaleY",
			"getter": "getScaleY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("shearX");
		writeProperty(obj, "getShearX()", {
			"kind": "primitive",
			"name": "shearX",
			"getter": "getShearX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("shearY");
		writeProperty(obj, "getShearY()", {
			"kind": "primitive",
			"name": "shearY",
			"getter": "getShearY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("inherit");
		writeProperty(obj, "getInherit()", {
			"kind": "enum",
			"name": "inherit",
			"getter": "getInherit()",
			"enumName": "Inherit",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeBoundingBoxAttachment(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<BoundingBoxAttachment-" + nameValue + ">" : "<BoundingBoxAttachment-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("BoundingBoxAttachment");

		json.writeName("color");
		writeProperty(obj, "getColor()", {
			"kind": "object",
			"name": "color",
			"getter": "getColor()",
			"valueType": "Color",
			"writeMethodCall": "writeColor",
			"isNullable": false
		});

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "array",
			"name": "bones",
			"getter": "getBones()",
			"elementType": "int",
			"elementKind": "primitive",
			"isNullable": true
		});

		json.writeName("vertices");
		writeProperty(obj, "getVertices()", {
			"kind": "array",
			"name": "vertices",
			"getter": "getVertices()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("worldVerticesLength");
		writeProperty(obj, "getWorldVerticesLength()", {
			"kind": "primitive",
			"name": "worldVerticesLength",
			"getter": "getWorldVerticesLength()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("timelineAttachment");
		writeProperty(obj, "getTimelineAttachment()", {
			"kind": "object",
			"name": "timelineAttachment",
			"getter": "getTimelineAttachment()",
			"valueType": "Attachment",
			"writeMethodCall": "writeAttachment",
			"isNullable": true
		});

		json.writeName("id");
		writeProperty(obj, "getId()", {
			"kind": "primitive",
			"name": "id",
			"getter": "getId()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeClippingAttachment(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<ClippingAttachment-" + nameValue + ">" : "<ClippingAttachment-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ClippingAttachment");

		json.writeName("endSlot");
		writeProperty(obj, "getEndSlot()", {
			"kind": "object",
			"name": "endSlot",
			"getter": "getEndSlot()",
			"valueType": "SlotData",
			"writeMethodCall": "writeSlotData",
			"isNullable": true
		});

		json.writeName("color");
		writeProperty(obj, "getColor()", {
			"kind": "object",
			"name": "color",
			"getter": "getColor()",
			"valueType": "Color",
			"writeMethodCall": "writeColor",
			"isNullable": false
		});

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "array",
			"name": "bones",
			"getter": "getBones()",
			"elementType": "int",
			"elementKind": "primitive",
			"isNullable": true
		});

		json.writeName("vertices");
		writeProperty(obj, "getVertices()", {
			"kind": "array",
			"name": "vertices",
			"getter": "getVertices()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("worldVerticesLength");
		writeProperty(obj, "getWorldVerticesLength()", {
			"kind": "primitive",
			"name": "worldVerticesLength",
			"getter": "getWorldVerticesLength()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("timelineAttachment");
		writeProperty(obj, "getTimelineAttachment()", {
			"kind": "object",
			"name": "timelineAttachment",
			"getter": "getTimelineAttachment()",
			"valueType": "Attachment",
			"writeMethodCall": "writeAttachment",
			"isNullable": true
		});

		json.writeName("id");
		writeProperty(obj, "getId()", {
			"kind": "primitive",
			"name": "id",
			"getter": "getId()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeConstraint(obj:Dynamic):Void {
		if (Std.isOfType(obj, IkConstraint)) {
			writeIkConstraint(obj);
		} else if (Std.isOfType(obj, PathConstraint)) {
			writePathConstraint(obj);
		} else if (Std.isOfType(obj, PhysicsConstraint)) {
			writePhysicsConstraint(obj);
		} else if (Std.isOfType(obj, Slider)) {
			writeSlider(obj);
		} else if (Std.isOfType(obj, TransformConstraint)) {
			writeTransformConstraint(obj);
		} else {
			throw new spine.SpineException("Unknown Constraint type");
		}
	}

	private function writeConstraintData(obj:Dynamic):Void {
		if (Std.isOfType(obj, IkConstraintData)) {
			writeIkConstraintData(obj);
		} else if (Std.isOfType(obj, PathConstraintData)) {
			writePathConstraintData(obj);
		} else if (Std.isOfType(obj, PhysicsConstraintData)) {
			writePhysicsConstraintData(obj);
		} else if (Std.isOfType(obj, SliderData)) {
			writeSliderData(obj);
		} else if (Std.isOfType(obj, TransformConstraintData)) {
			writeTransformConstraintData(obj);
		} else {
			throw new spine.SpineException("Unknown ConstraintData type");
		}
	}

	private function writeEvent(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<Event-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Event");

		json.writeName("int");
		writeProperty(obj, "getInt()", {
			"kind": "primitive",
			"name": "int",
			"getter": "getInt()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("float");
		writeProperty(obj, "getFloat()", {
			"kind": "primitive",
			"name": "float",
			"getter": "getFloat()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("string");
		writeProperty(obj, "getString()", {
			"kind": "primitive",
			"name": "string",
			"getter": "getString()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("volume");
		writeProperty(obj, "getVolume()", {
			"kind": "primitive",
			"name": "volume",
			"getter": "getVolume()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("balance");
		writeProperty(obj, "getBalance()", {
			"kind": "primitive",
			"name": "balance",
			"getter": "getBalance()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("time");
		writeProperty(obj, "getTime()", {
			"kind": "primitive",
			"name": "time",
			"getter": "getTime()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("data");
		writeProperty(obj, "getData()", {
			"kind": "object",
			"name": "data",
			"getter": "getData()",
			"valueType": "EventData",
			"writeMethodCall": "writeEventData",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeEventData(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<EventData-" + nameValue + ">" : "<EventData-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("EventData");

		json.writeName("int");
		writeProperty(obj, "getInt()", {
			"kind": "primitive",
			"name": "int",
			"getter": "getInt()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("float");
		writeProperty(obj, "getFloat()", {
			"kind": "primitive",
			"name": "float",
			"getter": "getFloat()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("string");
		writeProperty(obj, "getString()", {
			"kind": "primitive",
			"name": "string",
			"getter": "getString()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("audioPath");
		writeProperty(obj, "getAudioPath()", {
			"kind": "primitive",
			"name": "audioPath",
			"getter": "getAudioPath()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("volume");
		writeProperty(obj, "getVolume()", {
			"kind": "primitive",
			"name": "volume",
			"getter": "getVolume()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("balance");
		writeProperty(obj, "getBalance()", {
			"kind": "primitive",
			"name": "balance",
			"getter": "getBalance()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeIkConstraint(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<IkConstraint-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("IkConstraint");

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "array",
			"name": "bones",
			"getter": "getBones()",
			"elementType": "BonePose",
			"elementKind": "object",
			"writeMethodCall": "writeBonePose",
			"isNullable": false
		});

		json.writeName("target");
		writeProperty(obj, "getTarget()", {
			"kind": "object",
			"name": "target",
			"getter": "getTarget()",
			"valueType": "Bone",
			"writeMethodCall": "writeBone",
			"isNullable": false
		});

		json.writeName("data");
		writeProperty(obj, "getData()", {
			"kind": "object",
			"name": "data",
			"getter": "getData()",
			"valueType": "IkConstraintData",
			"writeMethodCall": "writeIkConstraintData",
			"isNullable": false
		});

		json.writeName("pose");
		writeProperty(obj, "getPose()", {
			"kind": "object",
			"name": "pose",
			"getter": "getPose()",
			"valueType": "IkConstraintPose",
			"writeMethodCall": "writeIkConstraintPose",
			"isNullable": false
		});

		json.writeName("appliedPose");
		writeProperty(obj, "getAppliedPose()", {
			"kind": "object",
			"name": "appliedPose",
			"getter": "getAppliedPose()",
			"valueType": "IkConstraintPose",
			"writeMethodCall": "writeIkConstraintPose",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeIkConstraintData(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<IkConstraintData-" + nameValue + ">" : "<IkConstraintData-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("IkConstraintData");

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "array",
			"name": "bones",
			"getter": "getBones()",
			"elementType": "BoneData",
			"elementKind": "object",
			"writeMethodCall": "writeBoneData",
			"isNullable": false
		});

		json.writeName("target");
		writeProperty(obj, "getTarget()", {
			"kind": "object",
			"name": "target",
			"getter": "getTarget()",
			"valueType": "BoneData",
			"writeMethodCall": "writeBoneData",
			"isNullable": false
		});

		json.writeName("uniform");
		writeProperty(obj, "getUniform()", {
			"kind": "primitive",
			"name": "uniform",
			"getter": "getUniform()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("setupPose");
		writeProperty(obj, "getSetupPose()", {
			"kind": "object",
			"name": "setupPose",
			"getter": "getSetupPose()",
			"valueType": "IkConstraintPose",
			"writeMethodCall": "writeIkConstraintPose",
			"isNullable": false
		});

		json.writeName("skinRequired");
		writeProperty(obj, "getSkinRequired()", {
			"kind": "primitive",
			"name": "skinRequired",
			"getter": "getSkinRequired()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeIkConstraintPose(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<IkConstraintPose-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("IkConstraintPose");

		json.writeName("mix");
		writeProperty(obj, "getMix()", {
			"kind": "primitive",
			"name": "mix",
			"getter": "getMix()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("softness");
		writeProperty(obj, "getSoftness()", {
			"kind": "primitive",
			"name": "softness",
			"getter": "getSoftness()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("bendDirection");
		writeProperty(obj, "getBendDirection()", {
			"kind": "primitive",
			"name": "bendDirection",
			"getter": "getBendDirection()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("compress");
		writeProperty(obj, "getCompress()", {
			"kind": "primitive",
			"name": "compress",
			"getter": "getCompress()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("stretch");
		writeProperty(obj, "getStretch()", {
			"kind": "primitive",
			"name": "stretch",
			"getter": "getStretch()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeMeshAttachment(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<MeshAttachment-" + nameValue + ">" : "<MeshAttachment-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("MeshAttachment");

		json.writeName("region");
		writeProperty(obj, "getRegion()", {
			"kind": "object",
			"name": "region",
			"getter": "getRegion()",
			"valueType": "TextureRegion",
			"writeMethodCall": "writeTextureRegion",
			"isNullable": true
		});

		json.writeName("triangles");
		writeProperty(obj, "getTriangles()", {
			"kind": "array",
			"name": "triangles",
			"getter": "getTriangles()",
			"elementType": "short",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("regionUVs");
		writeProperty(obj, "getRegionUVs()", {
			"kind": "array",
			"name": "regionUVs",
			"getter": "getRegionUVs()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("uVs");
		writeProperty(obj, "getUVs()", {
			"kind": "array",
			"name": "uVs",
			"getter": "getUVs()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("color");
		writeProperty(obj, "getColor()", {
			"kind": "object",
			"name": "color",
			"getter": "getColor()",
			"valueType": "Color",
			"writeMethodCall": "writeColor",
			"isNullable": false
		});

		json.writeName("path");
		writeProperty(obj, "getPath()", {
			"kind": "primitive",
			"name": "path",
			"getter": "getPath()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("hullLength");
		writeProperty(obj, "getHullLength()", {
			"kind": "primitive",
			"name": "hullLength",
			"getter": "getHullLength()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("edges");
		writeProperty(obj, "getEdges()", {
			"kind": "array",
			"name": "edges",
			"getter": "getEdges()",
			"elementType": "short",
			"elementKind": "primitive",
			"isNullable": true
		});

		json.writeName("width");
		writeProperty(obj, "getWidth()", {
			"kind": "primitive",
			"name": "width",
			"getter": "getWidth()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("height");
		writeProperty(obj, "getHeight()", {
			"kind": "primitive",
			"name": "height",
			"getter": "getHeight()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("sequence");
		writeProperty(obj, "getSequence()", {
			"kind": "object",
			"name": "sequence",
			"getter": "getSequence()",
			"valueType": "Sequence",
			"writeMethodCall": "writeSequence",
			"isNullable": true
		});

		json.writeName("parentMesh");
		writeProperty(obj, "getParentMesh()", {
			"kind": "object",
			"name": "parentMesh",
			"getter": "getParentMesh()",
			"valueType": "MeshAttachment",
			"writeMethodCall": "writeMeshAttachment",
			"isNullable": true
		});

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "array",
			"name": "bones",
			"getter": "getBones()",
			"elementType": "int",
			"elementKind": "primitive",
			"isNullable": true
		});

		json.writeName("vertices");
		writeProperty(obj, "getVertices()", {
			"kind": "array",
			"name": "vertices",
			"getter": "getVertices()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("worldVerticesLength");
		writeProperty(obj, "getWorldVerticesLength()", {
			"kind": "primitive",
			"name": "worldVerticesLength",
			"getter": "getWorldVerticesLength()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("timelineAttachment");
		writeProperty(obj, "getTimelineAttachment()", {
			"kind": "object",
			"name": "timelineAttachment",
			"getter": "getTimelineAttachment()",
			"valueType": "Attachment",
			"writeMethodCall": "writeAttachment",
			"isNullable": true
		});

		json.writeName("id");
		writeProperty(obj, "getId()", {
			"kind": "primitive",
			"name": "id",
			"getter": "getId()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePathAttachment(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<PathAttachment-" + nameValue + ">" : "<PathAttachment-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathAttachment");

		json.writeName("closed");
		writeProperty(obj, "getClosed()", {
			"kind": "primitive",
			"name": "closed",
			"getter": "getClosed()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("constantSpeed");
		writeProperty(obj, "getConstantSpeed()", {
			"kind": "primitive",
			"name": "constantSpeed",
			"getter": "getConstantSpeed()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("lengths");
		writeProperty(obj, "getLengths()", {
			"kind": "array",
			"name": "lengths",
			"getter": "getLengths()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("color");
		writeProperty(obj, "getColor()", {
			"kind": "object",
			"name": "color",
			"getter": "getColor()",
			"valueType": "Color",
			"writeMethodCall": "writeColor",
			"isNullable": false
		});

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "array",
			"name": "bones",
			"getter": "getBones()",
			"elementType": "int",
			"elementKind": "primitive",
			"isNullable": true
		});

		json.writeName("vertices");
		writeProperty(obj, "getVertices()", {
			"kind": "array",
			"name": "vertices",
			"getter": "getVertices()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("worldVerticesLength");
		writeProperty(obj, "getWorldVerticesLength()", {
			"kind": "primitive",
			"name": "worldVerticesLength",
			"getter": "getWorldVerticesLength()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("timelineAttachment");
		writeProperty(obj, "getTimelineAttachment()", {
			"kind": "object",
			"name": "timelineAttachment",
			"getter": "getTimelineAttachment()",
			"valueType": "Attachment",
			"writeMethodCall": "writeAttachment",
			"isNullable": true
		});

		json.writeName("id");
		writeProperty(obj, "getId()", {
			"kind": "primitive",
			"name": "id",
			"getter": "getId()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePathConstraint(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PathConstraint-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathConstraint");

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "array",
			"name": "bones",
			"getter": "getBones()",
			"elementType": "BonePose",
			"elementKind": "object",
			"writeMethodCall": "writeBonePose",
			"isNullable": false
		});

		json.writeName("slot");
		writeProperty(obj, "getSlot()", {
			"kind": "object",
			"name": "slot",
			"getter": "getSlot()",
			"valueType": "Slot",
			"writeMethodCall": "writeSlot",
			"isNullable": false
		});

		json.writeName("data");
		writeProperty(obj, "getData()", {
			"kind": "object",
			"name": "data",
			"getter": "getData()",
			"valueType": "PathConstraintData",
			"writeMethodCall": "writePathConstraintData",
			"isNullable": false
		});

		json.writeName("pose");
		writeProperty(obj, "getPose()", {
			"kind": "object",
			"name": "pose",
			"getter": "getPose()",
			"valueType": "PathConstraintPose",
			"writeMethodCall": "writePathConstraintPose",
			"isNullable": false
		});

		json.writeName("appliedPose");
		writeProperty(obj, "getAppliedPose()", {
			"kind": "object",
			"name": "appliedPose",
			"getter": "getAppliedPose()",
			"valueType": "PathConstraintPose",
			"writeMethodCall": "writePathConstraintPose",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePathConstraintData(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<PathConstraintData-" + nameValue + ">" : "<PathConstraintData-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathConstraintData");

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "array",
			"name": "bones",
			"getter": "getBones()",
			"elementType": "BoneData",
			"elementKind": "object",
			"writeMethodCall": "writeBoneData",
			"isNullable": false
		});

		json.writeName("slot");
		writeProperty(obj, "getSlot()", {
			"kind": "object",
			"name": "slot",
			"getter": "getSlot()",
			"valueType": "SlotData",
			"writeMethodCall": "writeSlotData",
			"isNullable": false
		});

		json.writeName("positionMode");
		writeProperty(obj, "getPositionMode()", {
			"kind": "enum",
			"name": "positionMode",
			"getter": "getPositionMode()",
			"enumName": "PositionMode",
			"isNullable": false
		});

		json.writeName("spacingMode");
		writeProperty(obj, "getSpacingMode()", {
			"kind": "enum",
			"name": "spacingMode",
			"getter": "getSpacingMode()",
			"enumName": "SpacingMode",
			"isNullable": false
		});

		json.writeName("rotateMode");
		writeProperty(obj, "getRotateMode()", {
			"kind": "enum",
			"name": "rotateMode",
			"getter": "getRotateMode()",
			"enumName": "RotateMode",
			"isNullable": false
		});

		json.writeName("offsetRotation");
		writeProperty(obj, "getOffsetRotation()", {
			"kind": "primitive",
			"name": "offsetRotation",
			"getter": "getOffsetRotation()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("setupPose");
		writeProperty(obj, "getSetupPose()", {
			"kind": "object",
			"name": "setupPose",
			"getter": "getSetupPose()",
			"valueType": "PathConstraintPose",
			"writeMethodCall": "writePathConstraintPose",
			"isNullable": false
		});

		json.writeName("skinRequired");
		writeProperty(obj, "getSkinRequired()", {
			"kind": "primitive",
			"name": "skinRequired",
			"getter": "getSkinRequired()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePathConstraintPose(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PathConstraintPose-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PathConstraintPose");

		json.writeName("position");
		writeProperty(obj, "getPosition()", {
			"kind": "primitive",
			"name": "position",
			"getter": "getPosition()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("spacing");
		writeProperty(obj, "getSpacing()", {
			"kind": "primitive",
			"name": "spacing",
			"getter": "getSpacing()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("mixRotate");
		writeProperty(obj, "getMixRotate()", {
			"kind": "primitive",
			"name": "mixRotate",
			"getter": "getMixRotate()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("mixX");
		writeProperty(obj, "getMixX()", {
			"kind": "primitive",
			"name": "mixX",
			"getter": "getMixX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("mixY");
		writeProperty(obj, "getMixY()", {
			"kind": "primitive",
			"name": "mixY",
			"getter": "getMixY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePhysicsConstraint(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PhysicsConstraint-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraint");

		json.writeName("bone");
		writeProperty(obj, "getBone()", {
			"kind": "object",
			"name": "bone",
			"getter": "getBone()",
			"valueType": "BonePose",
			"writeMethodCall": "writeBonePose",
			"isNullable": false
		});

		json.writeName("data");
		writeProperty(obj, "getData()", {
			"kind": "object",
			"name": "data",
			"getter": "getData()",
			"valueType": "PhysicsConstraintData",
			"writeMethodCall": "writePhysicsConstraintData",
			"isNullable": false
		});

		json.writeName("pose");
		writeProperty(obj, "getPose()", {
			"kind": "object",
			"name": "pose",
			"getter": "getPose()",
			"valueType": "PhysicsConstraintPose",
			"writeMethodCall": "writePhysicsConstraintPose",
			"isNullable": false
		});

		json.writeName("appliedPose");
		writeProperty(obj, "getAppliedPose()", {
			"kind": "object",
			"name": "appliedPose",
			"getter": "getAppliedPose()",
			"valueType": "PhysicsConstraintPose",
			"writeMethodCall": "writePhysicsConstraintPose",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePhysicsConstraintData(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<PhysicsConstraintData-" + nameValue + ">" : "<PhysicsConstraintData-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintData");

		json.writeName("bone");
		writeProperty(obj, "getBone()", {
			"kind": "object",
			"name": "bone",
			"getter": "getBone()",
			"valueType": "BoneData",
			"writeMethodCall": "writeBoneData",
			"isNullable": false
		});

		json.writeName("step");
		writeProperty(obj, "getStep()", {
			"kind": "primitive",
			"name": "step",
			"getter": "getStep()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("x");
		writeProperty(obj, "getX()", {
			"kind": "primitive",
			"name": "x",
			"getter": "getX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("y");
		writeProperty(obj, "getY()", {
			"kind": "primitive",
			"name": "y",
			"getter": "getY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("rotate");
		writeProperty(obj, "getRotate()", {
			"kind": "primitive",
			"name": "rotate",
			"getter": "getRotate()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scaleX");
		writeProperty(obj, "getScaleX()", {
			"kind": "primitive",
			"name": "scaleX",
			"getter": "getScaleX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("shearX");
		writeProperty(obj, "getShearX()", {
			"kind": "primitive",
			"name": "shearX",
			"getter": "getShearX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("limit");
		writeProperty(obj, "getLimit()", {
			"kind": "primitive",
			"name": "limit",
			"getter": "getLimit()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("inertiaGlobal");
		writeProperty(obj, "getInertiaGlobal()", {
			"kind": "primitive",
			"name": "inertiaGlobal",
			"getter": "getInertiaGlobal()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("strengthGlobal");
		writeProperty(obj, "getStrengthGlobal()", {
			"kind": "primitive",
			"name": "strengthGlobal",
			"getter": "getStrengthGlobal()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("dampingGlobal");
		writeProperty(obj, "getDampingGlobal()", {
			"kind": "primitive",
			"name": "dampingGlobal",
			"getter": "getDampingGlobal()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("massGlobal");
		writeProperty(obj, "getMassGlobal()", {
			"kind": "primitive",
			"name": "massGlobal",
			"getter": "getMassGlobal()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("windGlobal");
		writeProperty(obj, "getWindGlobal()", {
			"kind": "primitive",
			"name": "windGlobal",
			"getter": "getWindGlobal()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("gravityGlobal");
		writeProperty(obj, "getGravityGlobal()", {
			"kind": "primitive",
			"name": "gravityGlobal",
			"getter": "getGravityGlobal()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("mixGlobal");
		writeProperty(obj, "getMixGlobal()", {
			"kind": "primitive",
			"name": "mixGlobal",
			"getter": "getMixGlobal()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("setupPose");
		writeProperty(obj, "getSetupPose()", {
			"kind": "object",
			"name": "setupPose",
			"getter": "getSetupPose()",
			"valueType": "PhysicsConstraintPose",
			"writeMethodCall": "writePhysicsConstraintPose",
			"isNullable": false
		});

		json.writeName("skinRequired");
		writeProperty(obj, "getSkinRequired()", {
			"kind": "primitive",
			"name": "skinRequired",
			"getter": "getSkinRequired()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePhysicsConstraintPose(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<PhysicsConstraintPose-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PhysicsConstraintPose");

		json.writeName("inertia");
		writeProperty(obj, "getInertia()", {
			"kind": "primitive",
			"name": "inertia",
			"getter": "getInertia()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("strength");
		writeProperty(obj, "getStrength()", {
			"kind": "primitive",
			"name": "strength",
			"getter": "getStrength()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("damping");
		writeProperty(obj, "getDamping()", {
			"kind": "primitive",
			"name": "damping",
			"getter": "getDamping()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("massInverse");
		writeProperty(obj, "getMassInverse()", {
			"kind": "primitive",
			"name": "massInverse",
			"getter": "getMassInverse()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("wind");
		writeProperty(obj, "getWind()", {
			"kind": "primitive",
			"name": "wind",
			"getter": "getWind()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("gravity");
		writeProperty(obj, "getGravity()", {
			"kind": "primitive",
			"name": "gravity",
			"getter": "getGravity()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("mix");
		writeProperty(obj, "getMix()", {
			"kind": "primitive",
			"name": "mix",
			"getter": "getMix()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writePointAttachment(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<PointAttachment-" + nameValue + ">" : "<PointAttachment-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("PointAttachment");

		json.writeName("x");
		writeProperty(obj, "getX()", {
			"kind": "primitive",
			"name": "x",
			"getter": "getX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("y");
		writeProperty(obj, "getY()", {
			"kind": "primitive",
			"name": "y",
			"getter": "getY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("rotation");
		writeProperty(obj, "getRotation()", {
			"kind": "primitive",
			"name": "rotation",
			"getter": "getRotation()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("color");
		writeProperty(obj, "getColor()", {
			"kind": "object",
			"name": "color",
			"getter": "getColor()",
			"valueType": "Color",
			"writeMethodCall": "writeColor",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeRegionAttachment(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<RegionAttachment-" + nameValue + ">" : "<RegionAttachment-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("RegionAttachment");

		json.writeName("region");
		writeProperty(obj, "getRegion()", {
			"kind": "object",
			"name": "region",
			"getter": "getRegion()",
			"valueType": "TextureRegion",
			"writeMethodCall": "writeTextureRegion",
			"isNullable": true
		});

		json.writeName("offset");
		writeProperty(obj, "getOffset()", {
			"kind": "array",
			"name": "offset",
			"getter": "getOffset()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("uVs");
		writeProperty(obj, "getUVs()", {
			"kind": "array",
			"name": "uVs",
			"getter": "getUVs()",
			"elementType": "float",
			"elementKind": "primitive",
			"isNullable": false
		});

		json.writeName("x");
		writeProperty(obj, "getX()", {
			"kind": "primitive",
			"name": "x",
			"getter": "getX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("y");
		writeProperty(obj, "getY()", {
			"kind": "primitive",
			"name": "y",
			"getter": "getY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scaleX");
		writeProperty(obj, "getScaleX()", {
			"kind": "primitive",
			"name": "scaleX",
			"getter": "getScaleX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scaleY");
		writeProperty(obj, "getScaleY()", {
			"kind": "primitive",
			"name": "scaleY",
			"getter": "getScaleY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("rotation");
		writeProperty(obj, "getRotation()", {
			"kind": "primitive",
			"name": "rotation",
			"getter": "getRotation()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("width");
		writeProperty(obj, "getWidth()", {
			"kind": "primitive",
			"name": "width",
			"getter": "getWidth()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("height");
		writeProperty(obj, "getHeight()", {
			"kind": "primitive",
			"name": "height",
			"getter": "getHeight()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("color");
		writeProperty(obj, "getColor()", {
			"kind": "object",
			"name": "color",
			"getter": "getColor()",
			"valueType": "Color",
			"writeMethodCall": "writeColor",
			"isNullable": false
		});

		json.writeName("path");
		writeProperty(obj, "getPath()", {
			"kind": "primitive",
			"name": "path",
			"getter": "getPath()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("sequence");
		writeProperty(obj, "getSequence()", {
			"kind": "object",
			"name": "sequence",
			"getter": "getSequence()",
			"valueType": "Sequence",
			"writeMethodCall": "writeSequence",
			"isNullable": true
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSequence(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<Sequence-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Sequence");

		json.writeName("start");
		writeProperty(obj, "getStart()", {
			"kind": "primitive",
			"name": "start",
			"getter": "getStart()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("digits");
		writeProperty(obj, "getDigits()", {
			"kind": "primitive",
			"name": "digits",
			"getter": "getDigits()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("setupIndex");
		writeProperty(obj, "getSetupIndex()", {
			"kind": "primitive",
			"name": "setupIndex",
			"getter": "getSetupIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("regions");
		writeProperty(obj, "getRegions()", {
			"kind": "array",
			"name": "regions",
			"getter": "getRegions()",
			"elementType": "TextureRegion",
			"elementKind": "object",
			"writeMethodCall": "writeTextureRegion",
			"isNullable": false
		});

		json.writeName("id");
		writeProperty(obj, "getId()", {
			"kind": "primitive",
			"name": "id",
			"getter": "getId()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSkeleton(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<Skeleton-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Skeleton");

		json.writeName("data");
		writeProperty(obj, "getData()", {
			"kind": "object",
			"name": "data",
			"getter": "getData()",
			"valueType": "SkeletonData",
			"writeMethodCall": "writeSkeletonData",
			"isNullable": false
		});

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "array",
			"name": "bones",
			"getter": "getBones()",
			"elementType": "Bone",
			"elementKind": "object",
			"writeMethodCall": "writeBone",
			"isNullable": false
		});

		json.writeName("updateCache");
		writeProperty(obj, "getUpdateCache()", {
			"kind": "array",
			"name": "updateCache",
			"getter": "getUpdateCache()",
			"elementType": "Update",
			"elementKind": "object",
			"writeMethodCall": "writeUpdate",
			"isNullable": false
		});

		json.writeName("rootBone");
		writeProperty(obj, "getRootBone()", {
			"kind": "object",
			"name": "rootBone",
			"getter": "getRootBone()",
			"valueType": "Bone",
			"writeMethodCall": "writeBone",
			"isNullable": false
		});

		json.writeName("slots");
		writeProperty(obj, "getSlots()", {
			"kind": "array",
			"name": "slots",
			"getter": "getSlots()",
			"elementType": "Slot",
			"elementKind": "object",
			"writeMethodCall": "writeSlot",
			"isNullable": false
		});

		json.writeName("drawOrder");
		writeProperty(obj, "getDrawOrder()", {
			"kind": "array",
			"name": "drawOrder",
			"getter": "getDrawOrder()",
			"elementType": "Slot",
			"elementKind": "object",
			"writeMethodCall": "writeSlot",
			"isNullable": false
		});

		json.writeName("skin");
		writeProperty(obj, "getSkin()", {
			"kind": "object",
			"name": "skin",
			"getter": "getSkin()",
			"valueType": "Skin",
			"writeMethodCall": "writeSkin",
			"isNullable": true
		});

		json.writeName("constraints");
		writeProperty(obj, "getConstraints()", {
			"kind": "array",
			"name": "constraints",
			"getter": "getConstraints()",
			"elementType": "Constraint",
			"elementKind": "object",
			"writeMethodCall": "writeConstraint",
			"isNullable": false
		});

		json.writeName("physicsConstraints");
		writeProperty(obj, "getPhysicsConstraints()", {
			"kind": "array",
			"name": "physicsConstraints",
			"getter": "getPhysicsConstraints()",
			"elementType": "PhysicsConstraint",
			"elementKind": "object",
			"writeMethodCall": "writePhysicsConstraint",
			"isNullable": false
		});

		json.writeName("color");
		writeProperty(obj, "getColor()", {
			"kind": "object",
			"name": "color",
			"getter": "getColor()",
			"valueType": "Color",
			"writeMethodCall": "writeColor",
			"isNullable": false
		});

		json.writeName("scaleX");
		writeProperty(obj, "getScaleX()", {
			"kind": "primitive",
			"name": "scaleX",
			"getter": "getScaleX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scaleY");
		writeProperty(obj, "getScaleY()", {
			"kind": "primitive",
			"name": "scaleY",
			"getter": "getScaleY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("x");
		writeProperty(obj, "getX()", {
			"kind": "primitive",
			"name": "x",
			"getter": "getX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("y");
		writeProperty(obj, "getY()", {
			"kind": "primitive",
			"name": "y",
			"getter": "getY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("windX");
		writeProperty(obj, "getWindX()", {
			"kind": "primitive",
			"name": "windX",
			"getter": "getWindX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("windY");
		writeProperty(obj, "getWindY()", {
			"kind": "primitive",
			"name": "windY",
			"getter": "getWindY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("gravityX");
		writeProperty(obj, "getGravityX()", {
			"kind": "primitive",
			"name": "gravityX",
			"getter": "getGravityX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("gravityY");
		writeProperty(obj, "getGravityY()", {
			"kind": "primitive",
			"name": "gravityY",
			"getter": "getGravityY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("time");
		writeProperty(obj, "getTime()", {
			"kind": "primitive",
			"name": "time",
			"getter": "getTime()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSkeletonData(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<SkeletonData-" + nameValue + ">" : "<SkeletonData-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SkeletonData");

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "array",
			"name": "bones",
			"getter": "getBones()",
			"elementType": "BoneData",
			"elementKind": "object",
			"writeMethodCall": "writeBoneData",
			"isNullable": false
		});

		json.writeName("slots");
		writeProperty(obj, "getSlots()", {
			"kind": "array",
			"name": "slots",
			"getter": "getSlots()",
			"elementType": "SlotData",
			"elementKind": "object",
			"writeMethodCall": "writeSlotData",
			"isNullable": false
		});

		json.writeName("defaultSkin");
		writeProperty(obj, "getDefaultSkin()", {
			"kind": "object",
			"name": "defaultSkin",
			"getter": "getDefaultSkin()",
			"valueType": "Skin",
			"writeMethodCall": "writeSkin",
			"isNullable": true
		});

		json.writeName("skins");
		writeProperty(obj, "getSkins()", {
			"kind": "array",
			"name": "skins",
			"getter": "getSkins()",
			"elementType": "Skin",
			"elementKind": "object",
			"writeMethodCall": "writeSkin",
			"isNullable": false
		});

		json.writeName("events");
		writeProperty(obj, "getEvents()", {
			"kind": "array",
			"name": "events",
			"getter": "getEvents()",
			"elementType": "EventData",
			"elementKind": "object",
			"writeMethodCall": "writeEventData",
			"isNullable": false
		});

		json.writeName("animations");
		writeProperty(obj, "getAnimations()", {
			"kind": "array",
			"name": "animations",
			"getter": "getAnimations()",
			"elementType": "Animation",
			"elementKind": "object",
			"writeMethodCall": "writeAnimation",
			"isNullable": false
		});

		json.writeName("constraints");
		writeProperty(obj, "getConstraints()", {
			"kind": "array",
			"name": "constraints",
			"getter": "getConstraints()",
			"elementType": "ConstraintData",
			"elementKind": "object",
			"writeMethodCall": "writeConstraintData",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": true
		});

		json.writeName("x");
		writeProperty(obj, "getX()", {
			"kind": "primitive",
			"name": "x",
			"getter": "getX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("y");
		writeProperty(obj, "getY()", {
			"kind": "primitive",
			"name": "y",
			"getter": "getY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("width");
		writeProperty(obj, "getWidth()", {
			"kind": "primitive",
			"name": "width",
			"getter": "getWidth()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("height");
		writeProperty(obj, "getHeight()", {
			"kind": "primitive",
			"name": "height",
			"getter": "getHeight()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("referenceScale");
		writeProperty(obj, "getReferenceScale()", {
			"kind": "primitive",
			"name": "referenceScale",
			"getter": "getReferenceScale()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("version");
		writeProperty(obj, "getVersion()", {
			"kind": "primitive",
			"name": "version",
			"getter": "getVersion()",
			"valueType": "String",
			"isNullable": true
		});

		json.writeName("hash");
		writeProperty(obj, "getHash()", {
			"kind": "primitive",
			"name": "hash",
			"getter": "getHash()",
			"valueType": "String",
			"isNullable": true
		});

		json.writeName("imagesPath");
		writeProperty(obj, "getImagesPath()", {
			"kind": "primitive",
			"name": "imagesPath",
			"getter": "getImagesPath()",
			"valueType": "String",
			"isNullable": true
		});

		json.writeName("audioPath");
		writeProperty(obj, "getAudioPath()", {
			"kind": "primitive",
			"name": "audioPath",
			"getter": "getAudioPath()",
			"valueType": "String",
			"isNullable": true
		});

		json.writeName("fps");
		writeProperty(obj, "getFps()", {
			"kind": "primitive",
			"name": "fps",
			"getter": "getFps()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSkin(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<Skin-" + nameValue + ">" : "<Skin-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Skin");

		json.writeName("attachments");
		writeProperty(obj, "getAttachments()", {
			"kind": "array",
			"name": "attachments",
			"getter": "getAttachments()",
			"elementType": "SkinEntry",
			"elementKind": "object",
			"writeMethodCall": "writeSkinEntry",
			"isNullable": false
		});

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "array",
			"name": "bones",
			"getter": "getBones()",
			"elementType": "BoneData",
			"elementKind": "object",
			"writeMethodCall": "writeBoneData",
			"isNullable": false
		});

		json.writeName("constraints");
		writeProperty(obj, "getConstraints()", {
			"kind": "array",
			"name": "constraints",
			"getter": "getConstraints()",
			"elementType": "ConstraintData",
			"elementKind": "object",
			"writeMethodCall": "writeConstraintData",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("color");
		writeProperty(obj, "getColor()", {
			"kind": "object",
			"name": "color",
			"getter": "getColor()",
			"valueType": "Color",
			"writeMethodCall": "writeColor",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSkinEntry(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<SkinEntry-" + nameValue + ">" : "<SkinEntry-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SkinEntry");

		json.writeName("slotIndex");
		writeProperty(obj, "getSlotIndex()", {
			"kind": "primitive",
			"name": "slotIndex",
			"getter": "getSlotIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("attachment");
		writeProperty(obj, "getAttachment()", {
			"kind": "object",
			"name": "attachment",
			"getter": "getAttachment()",
			"valueType": "Attachment",
			"writeMethodCall": "writeAttachment",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSlider(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<Slider-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Slider");

		json.writeName("bone");
		writeProperty(obj, "getBone()", {
			"kind": "object",
			"name": "bone",
			"getter": "getBone()",
			"valueType": "Bone",
			"writeMethodCall": "writeBone",
			"isNullable": false
		});

		json.writeName("data");
		writeProperty(obj, "getData()", {
			"kind": "object",
			"name": "data",
			"getter": "getData()",
			"valueType": "SliderData",
			"writeMethodCall": "writeSliderData",
			"isNullable": false
		});

		json.writeName("pose");
		writeProperty(obj, "getPose()", {
			"kind": "object",
			"name": "pose",
			"getter": "getPose()",
			"valueType": "SliderPose",
			"writeMethodCall": "writeSliderPose",
			"isNullable": false
		});

		json.writeName("appliedPose");
		writeProperty(obj, "getAppliedPose()", {
			"kind": "object",
			"name": "appliedPose",
			"getter": "getAppliedPose()",
			"valueType": "SliderPose",
			"writeMethodCall": "writeSliderPose",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSliderData(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<SliderData-" + nameValue + ">" : "<SliderData-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SliderData");

		json.writeName("animation");
		writeProperty(obj, "getAnimation()", {
			"kind": "object",
			"name": "animation",
			"getter": "getAnimation()",
			"valueType": "Animation",
			"writeMethodCall": "writeAnimation",
			"isNullable": false
		});

		json.writeName("additive");
		writeProperty(obj, "getAdditive()", {
			"kind": "primitive",
			"name": "additive",
			"getter": "getAdditive()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("loop");
		writeProperty(obj, "getLoop()", {
			"kind": "primitive",
			"name": "loop",
			"getter": "getLoop()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("bone");
		writeProperty(obj, "getBone()", {
			"kind": "object",
			"name": "bone",
			"getter": "getBone()",
			"valueType": "BoneData",
			"writeMethodCall": "writeBoneData",
			"isNullable": true
		});

		json.writeName("property");
		writeProperty(obj, "getProperty()", {
			"kind": "object",
			"name": "property",
			"getter": "getProperty()",
			"valueType": "TransformConstraintData.FromProperty",
			"writeMethodCall": "writeFromProperty",
			"isNullable": true
		});

		json.writeName("offset");
		writeProperty(obj, "getOffset()", {
			"kind": "primitive",
			"name": "offset",
			"getter": "getOffset()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scale");
		writeProperty(obj, "getScale()", {
			"kind": "primitive",
			"name": "scale",
			"getter": "getScale()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("local");
		writeProperty(obj, "getLocal()", {
			"kind": "primitive",
			"name": "local",
			"getter": "getLocal()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("setupPose");
		writeProperty(obj, "getSetupPose()", {
			"kind": "object",
			"name": "setupPose",
			"getter": "getSetupPose()",
			"valueType": "SliderPose",
			"writeMethodCall": "writeSliderPose",
			"isNullable": false
		});

		json.writeName("skinRequired");
		writeProperty(obj, "getSkinRequired()", {
			"kind": "primitive",
			"name": "skinRequired",
			"getter": "getSkinRequired()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSliderPose(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<SliderPose-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SliderPose");

		json.writeName("time");
		writeProperty(obj, "getTime()", {
			"kind": "primitive",
			"name": "time",
			"getter": "getTime()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("mix");
		writeProperty(obj, "getMix()", {
			"kind": "primitive",
			"name": "mix",
			"getter": "getMix()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSlot(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<Slot-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("Slot");

		json.writeName("bone");
		writeProperty(obj, "getBone()", {
			"kind": "object",
			"name": "bone",
			"getter": "getBone()",
			"valueType": "Bone",
			"writeMethodCall": "writeBone",
			"isNullable": false
		});

		json.writeName("data");
		writeProperty(obj, "getData()", {
			"kind": "object",
			"name": "data",
			"getter": "getData()",
			"valueType": "SlotData",
			"writeMethodCall": "writeSlotData",
			"isNullable": false
		});

		json.writeName("pose");
		writeProperty(obj, "getPose()", {
			"kind": "object",
			"name": "pose",
			"getter": "getPose()",
			"valueType": "SlotPose",
			"writeMethodCall": "writeSlotPose",
			"isNullable": false
		});

		json.writeName("appliedPose");
		writeProperty(obj, "getAppliedPose()", {
			"kind": "object",
			"name": "appliedPose",
			"getter": "getAppliedPose()",
			"valueType": "SlotPose",
			"writeMethodCall": "writeSlotPose",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSlotData(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<SlotData-" + nameValue + ">" : "<SlotData-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SlotData");

		json.writeName("index");
		writeProperty(obj, "getIndex()", {
			"kind": "primitive",
			"name": "index",
			"getter": "getIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("boneData");
		writeProperty(obj, "getBoneData()", {
			"kind": "object",
			"name": "boneData",
			"getter": "getBoneData()",
			"valueType": "BoneData",
			"writeMethodCall": "writeBoneData",
			"isNullable": false
		});

		json.writeName("attachmentName");
		writeProperty(obj, "getAttachmentName()", {
			"kind": "primitive",
			"name": "attachmentName",
			"getter": "getAttachmentName()",
			"valueType": "String",
			"isNullable": true
		});

		json.writeName("blendMode");
		writeProperty(obj, "getBlendMode()", {
			"kind": "enum",
			"name": "blendMode",
			"getter": "getBlendMode()",
			"enumName": "BlendMode",
			"isNullable": false
		});

		json.writeName("visible");
		writeProperty(obj, "getVisible()", {
			"kind": "primitive",
			"name": "visible",
			"getter": "getVisible()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("setupPose");
		writeProperty(obj, "getSetupPose()", {
			"kind": "object",
			"name": "setupPose",
			"getter": "getSetupPose()",
			"valueType": "SlotPose",
			"writeMethodCall": "writeSlotPose",
			"isNullable": false
		});

		json.writeName("skinRequired");
		writeProperty(obj, "getSkinRequired()", {
			"kind": "primitive",
			"name": "skinRequired",
			"getter": "getSkinRequired()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeSlotPose(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<SlotPose-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("SlotPose");

		json.writeName("color");
		writeProperty(obj, "getColor()", {
			"kind": "object",
			"name": "color",
			"getter": "getColor()",
			"valueType": "Color",
			"writeMethodCall": "writeColor",
			"isNullable": false
		});

		json.writeName("darkColor");
		writeProperty(obj, "getDarkColor()", {
			"kind": "object",
			"name": "darkColor",
			"getter": "getDarkColor()",
			"valueType": "Color",
			"writeMethodCall": "writeColor",
			"isNullable": true
		});

		json.writeName("attachment");
		writeProperty(obj, "getAttachment()", {
			"kind": "object",
			"name": "attachment",
			"getter": "getAttachment()",
			"valueType": "Attachment",
			"writeMethodCall": "writeAttachment",
			"isNullable": true
		});

		json.writeName("sequenceIndex");
		writeProperty(obj, "getSequenceIndex()", {
			"kind": "primitive",
			"name": "sequenceIndex",
			"getter": "getSequenceIndex()",
			"valueType": "int",
			"isNullable": false
		});

		json.writeName("deform");
		writeProperty(obj, "getDeform()", {
			"kind": "object",
			"name": "deform",
			"getter": "getDeform()",
			"valueType": "FloatArray",
			"writeMethodCall": "writeFloatArray",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeTransformConstraint(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<TransformConstraint-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TransformConstraint");

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "array",
			"name": "bones",
			"getter": "getBones()",
			"elementType": "BonePose",
			"elementKind": "object",
			"writeMethodCall": "writeBonePose",
			"isNullable": false
		});

		json.writeName("source");
		writeProperty(obj, "getSource()", {
			"kind": "object",
			"name": "source",
			"getter": "getSource()",
			"valueType": "Bone",
			"writeMethodCall": "writeBone",
			"isNullable": false
		});

		json.writeName("data");
		writeProperty(obj, "getData()", {
			"kind": "object",
			"name": "data",
			"getter": "getData()",
			"valueType": "TransformConstraintData",
			"writeMethodCall": "writeTransformConstraintData",
			"isNullable": false
		});

		json.writeName("pose");
		writeProperty(obj, "getPose()", {
			"kind": "object",
			"name": "pose",
			"getter": "getPose()",
			"valueType": "TransformConstraintPose",
			"writeMethodCall": "writeTransformConstraintPose",
			"isNullable": false
		});

		json.writeName("appliedPose");
		writeProperty(obj, "getAppliedPose()", {
			"kind": "object",
			"name": "appliedPose",
			"getter": "getAppliedPose()",
			"valueType": "TransformConstraintPose",
			"writeMethodCall": "writeTransformConstraintPose",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeTransformConstraintData(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var nameValue = getPropertyValue(obj, "getName()");
		var refString = nameValue != null ? "<TransformConstraintData-" + nameValue + ">" : "<TransformConstraintData-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TransformConstraintData");

		json.writeName("bones");
		writeProperty(obj, "getBones()", {
			"kind": "array",
			"name": "bones",
			"getter": "getBones()",
			"elementType": "BoneData",
			"elementKind": "object",
			"writeMethodCall": "writeBoneData",
			"isNullable": false
		});

		json.writeName("source");
		writeProperty(obj, "getSource()", {
			"kind": "object",
			"name": "source",
			"getter": "getSource()",
			"valueType": "BoneData",
			"writeMethodCall": "writeBoneData",
			"isNullable": false
		});

		json.writeName("offsetRotation");
		writeProperty(obj, "getOffsetRotation()", {
			"kind": "primitive",
			"name": "offsetRotation",
			"getter": "getOffsetRotation()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("offsetX");
		writeProperty(obj, "getOffsetX()", {
			"kind": "primitive",
			"name": "offsetX",
			"getter": "getOffsetX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("offsetY");
		writeProperty(obj, "getOffsetY()", {
			"kind": "primitive",
			"name": "offsetY",
			"getter": "getOffsetY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("offsetScaleX");
		writeProperty(obj, "getOffsetScaleX()", {
			"kind": "primitive",
			"name": "offsetScaleX",
			"getter": "getOffsetScaleX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("offsetScaleY");
		writeProperty(obj, "getOffsetScaleY()", {
			"kind": "primitive",
			"name": "offsetScaleY",
			"getter": "getOffsetScaleY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("offsetShearY");
		writeProperty(obj, "getOffsetShearY()", {
			"kind": "primitive",
			"name": "offsetShearY",
			"getter": "getOffsetShearY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("localSource");
		writeProperty(obj, "getLocalSource()", {
			"kind": "primitive",
			"name": "localSource",
			"getter": "getLocalSource()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("localTarget");
		writeProperty(obj, "getLocalTarget()", {
			"kind": "primitive",
			"name": "localTarget",
			"getter": "getLocalTarget()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("additive");
		writeProperty(obj, "getAdditive()", {
			"kind": "primitive",
			"name": "additive",
			"getter": "getAdditive()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("clamp");
		writeProperty(obj, "getClamp()", {
			"kind": "primitive",
			"name": "clamp",
			"getter": "getClamp()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeName("properties");
		writeProperty(obj, "getProperties()", {
			"kind": "array",
			"name": "properties",
			"getter": "getProperties()",
			"elementType": "FromProperty",
			"elementKind": "object",
			"writeMethodCall": "writeFromProperty",
			"isNullable": false
		});

		json.writeName("name");
		writeProperty(obj, "getName()", {
			"kind": "primitive",
			"name": "name",
			"getter": "getName()",
			"valueType": "String",
			"isNullable": false
		});

		json.writeName("setupPose");
		writeProperty(obj, "getSetupPose()", {
			"kind": "object",
			"name": "setupPose",
			"getter": "getSetupPose()",
			"valueType": "TransformConstraintPose",
			"writeMethodCall": "writeTransformConstraintPose",
			"isNullable": false
		});

		json.writeName("skinRequired");
		writeProperty(obj, "getSkinRequired()", {
			"kind": "primitive",
			"name": "skinRequired",
			"getter": "getSkinRequired()",
			"valueType": "boolean",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeFromProperty(obj:Dynamic):Void {
		if (Std.isOfType(obj, spine.TransformConstraintData.FromRotate)) {
			writeFromRotate(obj);
		} else if (Std.isOfType(obj, spine.TransformConstraintData.FromScaleX)) {
			writeFromScaleX(obj);
		} else if (Std.isOfType(obj, spine.TransformConstraintData.FromScaleY)) {
			writeFromScaleY(obj);
		} else if (Std.isOfType(obj, spine.TransformConstraintData.FromShearY)) {
			writeFromShearY(obj);
		} else if (Std.isOfType(obj, spine.TransformConstraintData.FromX)) {
			writeFromX(obj);
		} else if (Std.isOfType(obj, spine.TransformConstraintData.FromY)) {
			writeFromY(obj);
		} else {
			throw new spine.SpineException("Unknown FromProperty type");
		}
	}

	private function writeFromRotate(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<FromRotate-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("FromRotate");

		json.writeName("offset");
		writeProperty(obj, "offset", {
			"kind": "primitive",
			"name": "offset",
			"getter": "offset",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("to");
		writeProperty(obj, "to", {
			"kind": "array",
			"name": "to",
			"getter": "to",
			"elementType": "ToProperty",
			"elementKind": "object",
			"writeMethodCall": "writeToProperty",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeFromScaleX(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<FromScaleX-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("FromScaleX");

		json.writeName("offset");
		writeProperty(obj, "offset", {
			"kind": "primitive",
			"name": "offset",
			"getter": "offset",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("to");
		writeProperty(obj, "to", {
			"kind": "array",
			"name": "to",
			"getter": "to",
			"elementType": "ToProperty",
			"elementKind": "object",
			"writeMethodCall": "writeToProperty",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeFromScaleY(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<FromScaleY-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("FromScaleY");

		json.writeName("offset");
		writeProperty(obj, "offset", {
			"kind": "primitive",
			"name": "offset",
			"getter": "offset",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("to");
		writeProperty(obj, "to", {
			"kind": "array",
			"name": "to",
			"getter": "to",
			"elementType": "ToProperty",
			"elementKind": "object",
			"writeMethodCall": "writeToProperty",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeFromShearY(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<FromShearY-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("FromShearY");

		json.writeName("offset");
		writeProperty(obj, "offset", {
			"kind": "primitive",
			"name": "offset",
			"getter": "offset",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("to");
		writeProperty(obj, "to", {
			"kind": "array",
			"name": "to",
			"getter": "to",
			"elementType": "ToProperty",
			"elementKind": "object",
			"writeMethodCall": "writeToProperty",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeFromX(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<FromX-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("FromX");

		json.writeName("offset");
		writeProperty(obj, "offset", {
			"kind": "primitive",
			"name": "offset",
			"getter": "offset",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("to");
		writeProperty(obj, "to", {
			"kind": "array",
			"name": "to",
			"getter": "to",
			"elementType": "ToProperty",
			"elementKind": "object",
			"writeMethodCall": "writeToProperty",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeFromY(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<FromY-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("FromY");

		json.writeName("offset");
		writeProperty(obj, "offset", {
			"kind": "primitive",
			"name": "offset",
			"getter": "offset",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("to");
		writeProperty(obj, "to", {
			"kind": "array",
			"name": "to",
			"getter": "to",
			"elementType": "ToProperty",
			"elementKind": "object",
			"writeMethodCall": "writeToProperty",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeToProperty(obj:Dynamic):Void {
		if (Std.isOfType(obj, spine.TransformConstraintData.ToRotate)) {
			writeToRotate(obj);
		} else if (Std.isOfType(obj, spine.TransformConstraintData.ToScaleX)) {
			writeToScaleX(obj);
		} else if (Std.isOfType(obj, spine.TransformConstraintData.ToScaleY)) {
			writeToScaleY(obj);
		} else if (Std.isOfType(obj, spine.TransformConstraintData.ToShearY)) {
			writeToShearY(obj);
		} else if (Std.isOfType(obj, spine.TransformConstraintData.ToX)) {
			writeToX(obj);
		} else if (Std.isOfType(obj, spine.TransformConstraintData.ToY)) {
			writeToY(obj);
		} else {
			throw new spine.SpineException("Unknown ToProperty type");
		}
	}

	private function writeToRotate(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<ToRotate-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ToRotate");

		json.writeName("offset");
		writeProperty(obj, "offset", {
			"kind": "primitive",
			"name": "offset",
			"getter": "offset",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("max");
		writeProperty(obj, "max", {
			"kind": "primitive",
			"name": "max",
			"getter": "max",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scale");
		writeProperty(obj, "scale", {
			"kind": "primitive",
			"name": "scale",
			"getter": "scale",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeToScaleX(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<ToScaleX-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ToScaleX");

		json.writeName("offset");
		writeProperty(obj, "offset", {
			"kind": "primitive",
			"name": "offset",
			"getter": "offset",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("max");
		writeProperty(obj, "max", {
			"kind": "primitive",
			"name": "max",
			"getter": "max",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scale");
		writeProperty(obj, "scale", {
			"kind": "primitive",
			"name": "scale",
			"getter": "scale",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeToScaleY(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<ToScaleY-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ToScaleY");

		json.writeName("offset");
		writeProperty(obj, "offset", {
			"kind": "primitive",
			"name": "offset",
			"getter": "offset",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("max");
		writeProperty(obj, "max", {
			"kind": "primitive",
			"name": "max",
			"getter": "max",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scale");
		writeProperty(obj, "scale", {
			"kind": "primitive",
			"name": "scale",
			"getter": "scale",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeToShearY(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<ToShearY-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ToShearY");

		json.writeName("offset");
		writeProperty(obj, "offset", {
			"kind": "primitive",
			"name": "offset",
			"getter": "offset",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("max");
		writeProperty(obj, "max", {
			"kind": "primitive",
			"name": "max",
			"getter": "max",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scale");
		writeProperty(obj, "scale", {
			"kind": "primitive",
			"name": "scale",
			"getter": "scale",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeToX(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<ToX-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ToX");

		json.writeName("offset");
		writeProperty(obj, "offset", {
			"kind": "primitive",
			"name": "offset",
			"getter": "offset",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("max");
		writeProperty(obj, "max", {
			"kind": "primitive",
			"name": "max",
			"getter": "max",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scale");
		writeProperty(obj, "scale", {
			"kind": "primitive",
			"name": "scale",
			"getter": "scale",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeToY(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<ToY-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("ToY");

		json.writeName("offset");
		writeProperty(obj, "offset", {
			"kind": "primitive",
			"name": "offset",
			"getter": "offset",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("max");
		writeProperty(obj, "max", {
			"kind": "primitive",
			"name": "max",
			"getter": "max",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("scale");
		writeProperty(obj, "scale", {
			"kind": "primitive",
			"name": "scale",
			"getter": "scale",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeTransformConstraintPose(obj:Dynamic):Void {
		if (visitedObjects.exists(obj)) {
			json.writeValue(visitedObjects.get(obj));
			return;
		}

		var refString = "<TransformConstraintPose-" + nextId++ + ">";
		visitedObjects.set(obj, refString);

		json.writeObjectStart();
		json.writeName("refString");
		json.writeValue(refString);
		json.writeName("type");
		json.writeValue("TransformConstraintPose");

		json.writeName("mixRotate");
		writeProperty(obj, "getMixRotate()", {
			"kind": "primitive",
			"name": "mixRotate",
			"getter": "getMixRotate()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("mixX");
		writeProperty(obj, "getMixX()", {
			"kind": "primitive",
			"name": "mixX",
			"getter": "getMixX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("mixY");
		writeProperty(obj, "getMixY()", {
			"kind": "primitive",
			"name": "mixY",
			"getter": "getMixY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("mixScaleX");
		writeProperty(obj, "getMixScaleX()", {
			"kind": "primitive",
			"name": "mixScaleX",
			"getter": "getMixScaleX()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("mixScaleY");
		writeProperty(obj, "getMixScaleY()", {
			"kind": "primitive",
			"name": "mixScaleY",
			"getter": "getMixScaleY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeName("mixShearY");
		writeProperty(obj, "getMixShearY()", {
			"kind": "primitive",
			"name": "mixShearY",
			"getter": "getMixShearY()",
			"valueType": "float",
			"isNullable": false
		});

		json.writeObjectEnd();
	}

	private function writeUpdate(obj:Dynamic):Void {
		if (Std.isOfType(obj, BonePose)) {
			writeBonePose(obj);
		} else if (Std.isOfType(obj, IkConstraint)) {
			writeIkConstraint(obj);
		} else if (Std.isOfType(obj, PathConstraint)) {
			writePathConstraint(obj);
		} else if (Std.isOfType(obj, PhysicsConstraint)) {
			writePhysicsConstraint(obj);
		} else if (Std.isOfType(obj, Slider)) {
			writeSlider(obj);
		} else if (Std.isOfType(obj, TransformConstraint)) {
			writeTransformConstraint(obj);
		} else {
			throw new spine.SpineException("Unknown Update type");
		}
	}

	private function writeVertexAttachment(obj:Dynamic):Void {
		if (Std.isOfType(obj, BoundingBoxAttachment)) {
			writeBoundingBoxAttachment(obj);
		} else if (Std.isOfType(obj, ClippingAttachment)) {
			writeClippingAttachment(obj);
		} else if (Std.isOfType(obj, MeshAttachment)) {
			writeMeshAttachment(obj);
		} else if (Std.isOfType(obj, PathAttachment)) {
			writePathAttachment(obj);
		} else {
			throw new spine.SpineException("Unknown VertexAttachment type");
		}
	}

	private function writeProperty(obj:Dynamic, javaGetter:String, propertyInfo:Dynamic):Void {
		var value = getPropertyValue(obj, javaGetter);

		if (value == null) {
			json.writeNull();
			return;
		}

		switch (propertyInfo.kind) {
			case "primitive":
				json.writeValue(value);

			case "object":
				var writeMethod = Reflect.field(this, propertyInfo.writeMethodCall);
				if (writeMethod != null) {
					Reflect.callMethod(this, writeMethod, [value]);
				} else {
					json.writeValue("<" + propertyInfo.valueType + ">");
				}

			case "enum":
				// Handle enum-like classes with ordinal property (all spine enums except Property)
				if (Reflect.hasField(value, "ordinal")) {
					// For enum-like classes, we need to find the static field name
					var className = Type.getClassName(Type.getClass(value));
					var clazz = Type.resolveClass(className);
					if (clazz != null) {
						// Find the static field that matches this instance
						for (fieldName in Type.getClassFields(clazz)) {
							var fieldValue = Reflect.field(clazz, fieldName);
							if (fieldValue == value) {
								json.writeValue(fieldName);
								return;
							}
						}
					}
					// Fallback: use ordinal value
					json.writeValue("ordinal_" + Reflect.field(value, "ordinal"));
				} else {
					// Handle actual Haxe enums (like Property)
					var enumValue = Type.enumConstructor(value);
					json.writeValue(enumValue != null ? enumValue : "unknown");
				}

			case "array":
				writeArray(value, propertyInfo);

			case "nestedArray":
				writeNestedArray(value);
		}
	}

	private function writeArray(arr:Dynamic, propertyInfo:Dynamic):Void {
		if (arr == null) {
			json.writeNull();
			return;
		}

		// Check if it's actually an array
		if (!Std.isOfType(arr, Array)) {
			json.writeValue("<Not an array: " + Type.getClassName(Type.getClass(arr)) + ">");
			return;
		}

		json.writeArrayStart();
		for (item in cast(arr, Array<Dynamic>)) {
			if (propertyInfo.elementKind == "primitive") {
				json.writeValue(item);
			} else if (propertyInfo.writeMethodCall != null) {
				var writeMethod = Reflect.field(this, propertyInfo.writeMethodCall);
				if (writeMethod != null) {
					Reflect.callMethod(this, writeMethod, [item]);
				} else {
					json.writeValue(item);
				}
			} else {
				json.writeValue(item);
			}
		}
		json.writeArrayEnd();
	}

	private function writeNestedArray(arr:Dynamic):Void {
		if (arr == null) {
			json.writeNull();
			return;
		}

		// Check if it's actually an array
		if (!Std.isOfType(arr, Array)) {
			json.writeValue("<Not an array: " + Type.getClassName(Type.getClass(arr)) + ">");
			return;
		}

		json.writeArrayStart();
		for (nestedArray in cast(arr, Array<Dynamic>)) {
			if (nestedArray == null) {
				json.writeNull();
			} else {
				json.writeArrayStart();
				for (elem in cast(nestedArray, Array<Dynamic>)) {
					json.writeValue(elem);
				}
				json.writeArrayEnd();
			}
		}
		json.writeArrayEnd();
	}

	// Helper methods for special types
	private function writeColor(obj:Dynamic):Void {
		if (obj == null) {
			json.writeNull();
			return;
		}
		json.writeObjectStart();
		json.writeName("r");
		json.writeValue(Reflect.field(obj, "r"));
		json.writeName("g");
		json.writeValue(Reflect.field(obj, "g"));
		json.writeName("b");
		json.writeValue(Reflect.field(obj, "b"));
		json.writeName("a");
		json.writeValue(Reflect.field(obj, "a"));
		json.writeObjectEnd();
	}

	private function writeTextureRegion(obj:Dynamic):Void {
		json.writeValue("<TextureRegion>");
	}

	private function writeIntArray(obj:Dynamic):Void {
		if (obj == null) {
			json.writeNull();
			return;
		}
		// IntArray in Java might be a custom type, try to get its array data
		var items = getPropertyValue(obj, "getItems()");
		if (items != null) {
			writeArray(items, {elementKind: "primitive"});
		} else {
			// Fallback: assume it's already an array
			writeArray(obj, {elementKind: "primitive"});
		}
	}

	private function writeFloatArray(obj:Dynamic):Void {
		if (obj == null) {
			json.writeNull();
			return;
		}
		// FloatArray in Java might be a custom type, try to get its array data
		var items = getPropertyValue(obj, "getItems()");
		if (items != null) {
			writeArray(items, {elementKind: "primitive"});
		} else {
			// Fallback: assume it's already an array
			writeArray(obj, {elementKind: "primitive"});
		}
	}
}
