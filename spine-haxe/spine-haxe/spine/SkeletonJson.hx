package spine;

import haxe.Json;
import openfl.errors.ArgumentError;
import openfl.errors.Error;
import openfl.utils.ByteArray;
import openfl.utils.Object;
import openfl.Vector;
import Reflect;
import spine.animation.AlphaTimeline;
import spine.animation.Animation;
import spine.animation.AttachmentTimeline;
import spine.animation.CurveTimeline1;
import spine.animation.CurveTimeline2;
import spine.animation.CurveTimeline;
import spine.animation.DeformTimeline;
import spine.animation.DrawOrderTimeline;
import spine.animation.EventTimeline;
import spine.animation.IkConstraintTimeline;
import spine.animation.PathConstraintMixTimeline;
import spine.animation.PathConstraintPositionTimeline;
import spine.animation.PathConstraintSpacingTimeline;
import spine.animation.RGB2Timeline;
import spine.animation.RGBA2Timeline;
import spine.animation.RGBATimeline;
import spine.animation.RGBTimeline;
import spine.animation.RotateTimeline;
import spine.animation.ScaleTimeline;
import spine.animation.ScaleXTimeline;
import spine.animation.ScaleYTimeline;
import spine.animation.ShearTimeline;
import spine.animation.ShearXTimeline;
import spine.animation.ShearYTimeline;
import spine.animation.Timeline;
import spine.animation.TransformConstraintTimeline;
import spine.animation.TranslateTimeline;
import spine.animation.TranslateXTimeline;
import spine.animation.TranslateYTimeline;
import spine.attachments.Attachment;
import spine.attachments.AttachmentLoader;
import spine.attachments.AttachmentType;
import spine.attachments.BoundingBoxAttachment;
import spine.attachments.ClippingAttachment;
import spine.attachments.MeshAttachment;
import spine.attachments.PathAttachment;
import spine.attachments.PointAttachment;
import spine.attachments.RegionAttachment;
import spine.attachments.VertexAttachment;

class SkeletonJson {
	public var attachmentLoader:AttachmentLoader;
	public var scale:Float = 1;

	private var linkedMeshes:Vector<LinkedMesh> = new Vector<LinkedMesh>();

	public function new(attachmentLoader:AttachmentLoader = null) {
		this.attachmentLoader = attachmentLoader;
	}

	/** @param object A String or ByteArray. */
	public function readSkeletonData(object:Object, name:String = null):SkeletonData {
		if (object == null)
			throw new ArgumentError("object cannot be null.");

		var root:Object;
		if (Std.isOfType(object, String)) {
			root = Json.parse(cast(object, String));
		} else if (Std.isOfType(object, ByteArrayData)) {
			root = Json.parse(cast(object, ByteArray).readUTFBytes(cast(object, ByteArray).length));
		} else if (Std.isOfType(object, Dynamic)) {
			root = object;
		} else {
			throw new ArgumentError("object must be a String, ByteArray or Object.");
		}

		var skeletonData:SkeletonData = new SkeletonData();
		skeletonData.name = name;

		// Skeleton.
		var skeletonMap:Object = Reflect.getProperty(root, "skeleton");
		if (skeletonMap != null) {
			skeletonData.hash = Reflect.getProperty(skeletonMap, "hash");
			skeletonData.version = Reflect.getProperty(skeletonMap, "spine");
			skeletonData.x = getFloat(Reflect.getProperty(skeletonMap, "x"));
			skeletonData.y = getFloat(Reflect.getProperty(skeletonMap, "y"));
			skeletonData.width = getFloat(Reflect.getProperty(skeletonMap, "width"));
			skeletonData.height = getFloat(Reflect.getProperty(skeletonMap, "height"));
			skeletonData.fps = getFloat(Reflect.getProperty(skeletonMap, "fps"));
			skeletonData.imagesPath = Reflect.getProperty(skeletonMap, "images");
		}

		// Bones.
		var boneData:BoneData;
		for (boneMap in cast(Reflect.getProperty(root, "bones"), Array<Dynamic>)) {
			var parent:BoneData = null;
			var parentName:String = Reflect.getProperty(boneMap, "parent");
			if (parentName != null) {
				parent = skeletonData.findBone(parentName);
				if (parent == null)
					throw new Error("Parent bone not found: " + parentName);
			}
			boneData = new BoneData(skeletonData.bones.length, Reflect.getProperty(boneMap, "name"), parent);
			boneData.length = getFloat(Reflect.getProperty(boneMap, "length")) * scale;
			boneData.x = getFloat(Reflect.getProperty(boneMap, "x")) * scale;
			boneData.y = getFloat(Reflect.getProperty(boneMap, "y")) * scale;
			boneData.rotation = getFloat(Reflect.getProperty(boneMap, "rotation"));
			boneData.scaleX = getFloat(Reflect.getProperty(boneMap, "scaleX"), 1);
			boneData.scaleY = getFloat(Reflect.getProperty(boneMap, "scaleY"), 1);
			boneData.shearX = getFloat(Reflect.getProperty(boneMap, "shearX"));
			boneData.shearY = getFloat(Reflect.getProperty(boneMap, "shearY"));
			boneData.transformMode = Reflect.hasField(boneMap,
				"transform") ? TransformMode.fromName(Reflect.getProperty(boneMap, "transform")) : TransformMode.normal;
			boneData.skinRequired = Reflect.hasField(boneMap, "skin") ? cast(Reflect.getProperty(boneMap, "skin"), Bool) : false;

			var color:String = Reflect.getProperty(boneMap, "color");
			if (color != null) {
				boneData.color.setFromString(color);
			}

			skeletonData.bones.push(boneData);
		}

		// Slots.
		for (slotMap in cast(Reflect.getProperty(root, "slots"), Array<Dynamic>)) {
			var slotName:String = Reflect.getProperty(slotMap, "name");
			var boneName:String = Reflect.getProperty(slotMap, "bone");
			boneData = skeletonData.findBone(boneName);
			if (boneData == null)
				throw new Error("Slot bone not found: " + boneName);
			var slotData:SlotData = new SlotData(skeletonData.slots.length, slotName, boneData);

			var color:String = Reflect.getProperty(slotMap, "color");
			if (color != null) {
				slotData.color.setFromString(color);
			}

			var dark:String = Reflect.getProperty(slotMap, "dark");
			if (dark != null) {
				slotData.darkColor = new Color(0, 0, 0);
				slotData.darkColor.setFromString(dark);
			}

			slotData.attachmentName = Reflect.getProperty(slotMap, "attachment");
			slotData.blendMode = Reflect.hasField(slotMap, "blend") ? BlendMode.fromName(Reflect.getProperty(slotMap, "blend")) : BlendMode.normal;
			skeletonData.slots.push(slotData);
		}

		// IK constraints.
		if (Reflect.hasField(root, "ik")) {
			for (constraintMap in cast(Reflect.getProperty(root, "ik"), Array<Dynamic>)) {
				var ikData:IkConstraintData = new IkConstraintData(Reflect.getProperty(constraintMap, "name"));
				ikData.order = getInt(Reflect.getProperty(constraintMap, "order"));
				ikData.skinRequired = Reflect.hasField(constraintMap, "skin") ? cast(Reflect.getProperty(constraintMap, "skin"), Bool) : false;

				for (boneName in cast(Reflect.getProperty(constraintMap, "bones"), Array<Dynamic>)) {
					var bone:BoneData = skeletonData.findBone(boneName);
					if (bone == null)
						throw new Error("IK constraint bone not found: " + boneName);
					ikData.bones.push(bone);
				}

				ikData.target = skeletonData.findBone(Reflect.getProperty(constraintMap, "target"));
				if (ikData.target == null)
					throw new Error("Target bone not found: " + Reflect.getProperty(constraintMap, "target"));

				ikData.bendDirection = (!Reflect.hasField(constraintMap, "bendPositive")
					|| cast(Reflect.getProperty(constraintMap, "bendPositive"), Bool)) ? 1 : -1;
				ikData.compress = (Reflect.hasField(constraintMap, "compress")
					&& cast(Reflect.getProperty(constraintMap, "compress"), Bool));
				ikData.stretch = (Reflect.hasField(constraintMap, "stretch") && cast(Reflect.getProperty(constraintMap, "stretch"), Bool));
				ikData.uniform = (Reflect.hasField(constraintMap, "uniform") && cast(Reflect.getProperty(constraintMap, "uniform"), Bool));
				ikData.softness = getFloat(Reflect.getProperty(constraintMap, "softness")) * scale;
				ikData.mix = getFloat(Reflect.getProperty(constraintMap, "mix"), 1);

				skeletonData.ikConstraints.push(ikData);
			}
		}

		// Transform constraints.
		if (Reflect.hasField(root, "transform")) {
			for (constraintMap in cast(Reflect.getProperty(root, "transform"), Array<Dynamic>)) {
				var transformData:TransformConstraintData = new TransformConstraintData(Reflect.getProperty(constraintMap, "name"));
				transformData.order = getInt(Reflect.getProperty(constraintMap, "order"));
				transformData.skinRequired = Reflect.hasField(constraintMap, "skin") ? cast(Reflect.getProperty(constraintMap, "skin"), Bool) : false;

				for (boneName in cast(Reflect.getProperty(constraintMap, "bones"), Array<Dynamic>)) {
					var bone = skeletonData.findBone(boneName);
					if (bone == null)
						throw new Error("Transform constraint bone not found: " + boneName);
					transformData.bones.push(bone);
				}

				transformData.target = skeletonData.findBone(Reflect.getProperty(constraintMap, "target"));
				if (transformData.target == null)
					throw new Error("Target bone not found: " + Reflect.getProperty(constraintMap, "target"));

				transformData.local = Reflect.hasField(constraintMap, "local") ? cast(Reflect.getProperty(constraintMap, "local"), Bool) : false;
				transformData.relative = Reflect.hasField(constraintMap, "relative") ? cast(Reflect.getProperty(constraintMap, "relative"), Bool) : false;

				transformData.offsetRotation = getFloat(Reflect.getProperty(constraintMap, "rotation"));
				transformData.offsetX = getFloat(Reflect.getProperty(constraintMap, "x")) * scale;
				transformData.offsetY = getFloat(Reflect.getProperty(constraintMap, "y")) * scale;
				transformData.offsetScaleX = getFloat(Reflect.getProperty(constraintMap, "scaleX"));
				transformData.offsetScaleY = getFloat(Reflect.getProperty(constraintMap, "scaleY"));
				transformData.offsetShearY = getFloat(Reflect.getProperty(constraintMap, "shearY"));

				transformData.mixRotate = getFloat(Reflect.getProperty(constraintMap, "mixRotate"), 1);
				transformData.mixX = getFloat(Reflect.getProperty(constraintMap, "mixX"), 1);
				transformData.mixY = getFloat(Reflect.getProperty(constraintMap, "mixY"), transformData.mixX);
				transformData.mixScaleX = getFloat(Reflect.getProperty(constraintMap, "mixScaleX"), 1);
				transformData.mixScaleY = getFloat(Reflect.getProperty(constraintMap, "mixScaleY"), transformData.mixScaleX);
				transformData.mixShearY = getFloat(Reflect.getProperty(constraintMap, "mixShearY"), 1);

				skeletonData.transformConstraints.push(transformData);
			}
		}

		// Path constraints.
		if (Reflect.hasField(root, "path")) {
			for (constraintMap in cast(Reflect.getProperty(root, "path"), Array<Dynamic>)) {
				var pathData:PathConstraintData = new PathConstraintData(Reflect.getProperty(constraintMap, "name"));
				pathData.order = getInt(Reflect.getProperty(constraintMap, "order"));
				pathData.skinRequired = Reflect.hasField(constraintMap, "skin") ? cast(Reflect.getProperty(constraintMap, "skin"), Bool) : false;

				for (boneName in cast(Reflect.getProperty(constraintMap, "bones"), Array<Dynamic>)) {
					var bone = skeletonData.findBone(boneName);
					if (bone == null)
						throw new Error("Path constraint bone not found: " + boneName);
					pathData.bones.push(bone);
				}

				pathData.target = skeletonData.findSlot(Reflect.getProperty(constraintMap, "target"));
				if (pathData.target == null)
					throw new Error("Path target slot not found: " + Reflect.getProperty(constraintMap, "target"));

				pathData.positionMode = Reflect.hasField(constraintMap,
					"positionMode") ? PositionMode.fromName(Reflect.getProperty(constraintMap, "positionMode")) : PositionMode.percent;
				pathData.spacingMode = Reflect.hasField(constraintMap,
					"spacingMode") ? SpacingMode.fromName(Reflect.getProperty(constraintMap, "spacingMode")) : SpacingMode.length;
				pathData.rotateMode = Reflect.hasField(constraintMap,
					"rotateMode") ? RotateMode.fromName(Reflect.getProperty(constraintMap, "rotateMode")) : RotateMode.tangent;
				pathData.offsetRotation = getFloat(Reflect.getProperty(constraintMap, "rotation"));
				pathData.position = getFloat(Reflect.getProperty(constraintMap, "position"));
				if (pathData.positionMode == PositionMode.fixed)
					pathData.position *= scale;
				pathData.spacing = getFloat(Reflect.getProperty(constraintMap, "spacing"));
				if (pathData.spacingMode == SpacingMode.length || pathData.spacingMode == SpacingMode.fixed)
					pathData.spacing *= scale;
				pathData.mixRotate = getFloat(Reflect.getProperty(constraintMap, "mixRotate"), 1);
				pathData.mixX = getFloat(Reflect.getProperty(constraintMap, "mixX"), 1);
				pathData.mixY = getFloat(Reflect.getProperty(constraintMap, "mixY"), 1);

				skeletonData.pathConstraints.push(pathData);
			}
		}

		// Skins.
		if (Reflect.hasField(root, "skins")) {
			for (skinMap in cast(Reflect.getProperty(root, "skins"), Array<Dynamic>)) {
				var skin:Skin = new Skin(Reflect.getProperty(skinMap, "name"));

				if (Reflect.hasField(skinMap, "bones")) {
					var bones:Array<Dynamic> = cast(Reflect.getProperty(skinMap, "bones"), Array<Dynamic>);
					for (ii in 0...bones.length) {
						var boneData:BoneData = skeletonData.findBone(bones[ii]);
						if (boneData == null)
							throw new Error("Skin bone not found: " + bones[ii]);
						skin.bones.push(boneData);
					}
				}

				if (Reflect.hasField(skinMap, "ik")) {
					var ik:Array<Dynamic> = cast(Reflect.getProperty(skinMap, "ik"), Array<Dynamic>);
					for (ii in 0...ik.length) {
						var constraint:ConstraintData = skeletonData.findIkConstraint(ik[ii]);
						if (constraint == null)
							throw new Error("Skin IK constraint not found: " + ik[ii]);
						skin.constraints.push(constraint);
					}
				}

				if (Reflect.hasField(skinMap, "transform")) {
					var transform:Array<Dynamic> = cast(Reflect.getProperty(skinMap, "transform"), Array<Dynamic>);
					for (ii in 0...transform.length) {
						var constraint:ConstraintData = skeletonData.findTransformConstraint(transform[ii]);
						if (constraint == null)
							throw new Error("Skin transform constraint not found: " + transform[ii]);
						skin.constraints.push(constraint);
					}
				}

				if (Reflect.hasField(skinMap, "path")) {
					var path:Array<Dynamic> = cast(Reflect.getProperty(skinMap, "path"), Array<Dynamic>);
					for (ii in 0...path.length) {
						var constraint:ConstraintData = skeletonData.findPathConstraint(path[ii]);
						if (constraint == null)
							throw new Error("Skin path constraint not found: " + path[ii]);
						skin.constraints.push(constraint);
					}
				}

				if (Reflect.hasField(skinMap, "attachments")) {
					var attachments:Object = Reflect.getProperty(skinMap, "attachments");
					for (slotName in attachments) {
						var slot:SlotData = skeletonData.findSlot(slotName);
						var slotEntry:Object = Reflect.getProperty(attachments, slotName);
						for (attachmentName in slotEntry) {
							var attachment:Attachment = readAttachment(Reflect.getProperty(slotEntry, attachmentName), skin, slot.index, attachmentName,
								skeletonData);
							if (attachment != null) {
								skin.setAttachment(slot.index, attachmentName, attachment);
							}
						}
					}
				}

				skeletonData.skins.push(skin);
				if (skin.name == "default") {
					skeletonData.defaultSkin = skin;
				}
			}
		}

		// Linked meshes.
		for (linkedMesh in linkedMeshes) {
			var parentSkin:Skin = linkedMesh.skin == null ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
			if (parentSkin == null)
				throw new Error("Skin not found: " + linkedMesh.skin);
			var parentMesh:Attachment = parentSkin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
			if (parentMesh == null)
				throw new Error("Parent mesh not found: " + linkedMesh.parent);
			linkedMesh.mesh.deformAttachment = linkedMesh.inheritDeform ? cast(parentMesh, VertexAttachment) : linkedMesh.mesh;
			linkedMesh.mesh.parentMesh = cast(parentMesh, MeshAttachment);
			linkedMesh.mesh.updateUVs();
		}
		linkedMeshes.length = 0;

		// Events.
		var events:Object = Reflect.getProperty(root, "events");
		for (eventName in events) {
			var eventMap:Map<String, Dynamic> = events[eventName];
			var eventData:EventData = new EventData(eventName);
			eventData.intValue = getInt(eventMap["int"]);
			eventData.floatValue = getFloat(eventMap["float"]);
			eventData.stringValue = eventMap["string"] != null ? eventMap["string"] : "";
			eventData.audioPath = eventMap["audio"];
			if (eventData.audioPath != null) {
				eventData.volume = getFloat(eventMap["volume"], 1);
				eventData.balance = getFloat(eventMap["balance"]);
			}
			skeletonData.events.push(eventData);
		}

		// Animations.
		var animations:Object = Reflect.getProperty(root, "animations");
		for (animationName in animations) {
			readAnimation(animations[animationName], animationName, skeletonData);
		}
		return skeletonData;
	}

	private function readAttachment(map:Object, skin:Skin, slotIndex:Int, name:String, skeletonData:SkeletonData):Attachment {
		if (map["name"] != null)
			name = map["name"];

		var color:String;
		switch (AttachmentType.fromName(Reflect.hasField(map, "type") ? Reflect.getProperty(map, "type") : "region")) {
			case AttachmentType.region:
				var region:RegionAttachment = attachmentLoader.newRegionAttachment(skin, name, map["path"] != null ? map["path"] : name);
				if (region == null)
					return null;
				region.path = map["path"] != null ? map["path"] : name;
				region.x = getFloat(map["x"]) * scale;
				region.y = getFloat(map["y"]) * scale;
				region.scaleX = getFloat(map["scaleX"], 1);
				region.scaleY = getFloat(map["scaleY"], 1);
				region.rotation = getFloat(map["rotation"]);
				region.width = getFloat(map["width"]) * scale;
				region.height = getFloat(map["height"]) * scale;
				color = Reflect.getProperty(map, "color");
				if (color != null) {
					region.color.setFromString(color);
				}
				region.updateOffset();
				return region;
			case AttachmentType.mesh, AttachmentType.linkedmesh:
				var mesh:MeshAttachment = attachmentLoader.newMeshAttachment(skin, name, map["path"] != null ? map["path"] : name);
				if (mesh == null)
					return null;
				mesh.path = map["path"] != null ? map["path"] : name;
				color = Reflect.getProperty(map, "color");
				if (color != null) {
					mesh.color.setFromString(color);
				}
				mesh.width = getFloat(map["width"]) * scale;
				mesh.height = getFloat(map["height"]) * scale;
				if (map["parent"] != null) {
					var inheritDeform:Bool = map.hasOwnProperty("deform") ? cast(map["deform"], Bool) : true;
					linkedMeshes.push(new LinkedMesh(mesh, map["skin"], slotIndex, map["parent"], inheritDeform));
					return mesh;
				}
				var uvs:Vector<Float> = getFloatArray(map, "uvs");
				readVertices(map, mesh, uvs.length);
				mesh.triangles = getIntArray(map, "triangles");
				mesh.regionUVs = uvs;
				mesh.updateUVs();
				mesh.hullLength = (getInt(map["hull"])) * 2;
				if (map["edges"] != null)
					mesh.edges = getIntArray(map, "edges");
				return mesh;
			case AttachmentType.boundingbox:
				var box:BoundingBoxAttachment = attachmentLoader.newBoundingBoxAttachment(skin, name);
				if (box == null)
					return null;
				readVertices(map, box, Std.parseInt(map["vertexCount"]) << 1);
				return box;
			case AttachmentType.path:
				var path:PathAttachment = attachmentLoader.newPathAttachment(skin, name);
				if (path == null)
					return null;
				path.closed = map.hasOwnProperty("closed") ? cast(map["closed"], Bool) : false;
				path.constantSpeed = map.hasOwnProperty("constantSpeed") ? cast(map["constantSpeed"], Bool) : true;
				var vertexCount:Int = Std.parseInt(map["vertexCount"]);
				readVertices(map, path, vertexCount << 1);
				var lengths:Vector<Float> = new Vector<Float>();
				for (curves in cast(map["lengths"], Array<Dynamic>)) {
					lengths.push(Std.parseFloat(curves) * scale);
				}
				path.lengths = lengths;
				return path;
			case AttachmentType.point:
				var point:PointAttachment = attachmentLoader.newPointAttachment(skin, name);
				if (point == null)
					return null;
				point.x = map.hasOwnProperty("x") ? Std.parseFloat(map["x"]) * scale : 0;
				point.y = map.hasOwnProperty("y") ? Std.parseFloat(map["y"]) * scale : 0;
				point.rotation = map.hasOwnProperty("rotation") ? Std.parseFloat(map["rotation"]) : 0;
				color = Reflect.getProperty(map, "color");
				if (color != null) {
					point.color.setFromString(color);
				}
				return point;
			case AttachmentType.clipping:
				var clip:ClippingAttachment = attachmentLoader.newClippingAttachment(skin, name);
				if (clip == null)
					return null;
				var end:String = map["end"];
				if (end != null) {
					var slot:SlotData = skeletonData.findSlot(end);
					if (slot == null)
						throw new Error("Clipping end slot not found: " + end);
					clip.endSlot = slot;
				}
				var vertexCount:Int = Std.parseInt(map["vertexCount"]);
				readVertices(map, clip, vertexCount << 1);
				color = Reflect.getProperty(map, "color");
				if (color != null) {
					clip.color.setFromString(color);
				}
				return clip;
		}
		return null;
	}

	private function readVertices(map:Object, attachment:VertexAttachment, verticesLength:Int):Void {
		attachment.worldVerticesLength = verticesLength;
		var vertices:Vector<Float> = getFloatArray(map, "vertices");
		if (verticesLength == vertices.length) {
			if (scale != 1) {
				for (i in 0...vertices.length) {
					vertices[i] *= scale;
				}
			}
			attachment.vertices = vertices;
			return;
		}

		var weights:Vector<Float> = new Vector<Float>();
		var bones:Vector<Int> = new Vector<Int>();
		var i:Int = 0;
		var n:Int = vertices.length;
		while (i < n) {
			var boneCount:Int = Std.int(vertices[i++]);
			bones.push(boneCount);
			var nn:Int = i + boneCount * 4;
			while (i < nn) {
				bones.push(Std.int(vertices[i]));
				weights.push(vertices[i + 1] * scale);
				weights.push(vertices[i + 2] * scale);
				weights.push(vertices[i + 3]);

				i += 4;
			}
		}
		attachment.bones = bones;
		attachment.vertices = weights;
	}

	private function readAnimation(map:Object, name:String, skeletonData:SkeletonData):Void {
		var timelines:Vector<Timeline> = new Vector<Timeline>();

		var slotMap:Object;
		var slotIndex:Int;
		var slotName:String;

		var timelineMap:Array<Object>;
		var keyMap:Object;
		var nextMap:Object;
		var frame:Int, bezier:Int;
		var time:Float, time2:Float;
		var curve:Object;
		var timelineName:String;

		// Slot timelines.
		var slots:Object = Reflect.getProperty(map, "slots");
		for (slotName in slots) {
			slotMap = slots[slotName];
			slotIndex = skeletonData.findSlot(slotName).index;
			for (timelineName in slotMap) {
				timelineMap = slotMap[timelineName];
				if (timelineMap == null)
					continue;
				if (timelineName == "attachment") {
					var attachmentTimeline:AttachmentTimeline = new AttachmentTimeline(timelineMap.length, slotIndex);
					for (frame in 0...timelineMap.length) {
						keyMap = timelineMap[frame];
						attachmentTimeline.setFrame(frame, getFloat(Reflect.getProperty(keyMap, "time")), keyMap.name);
					}
					timelines.push(attachmentTimeline);
				} else if (timelineName == "rgba") {
					var rgbaTimeline:RGBATimeline = new RGBATimeline(timelineMap.length, timelineMap.length << 2, slotIndex);
					keyMap = timelineMap[0];
					time = getFloat(Reflect.getProperty(keyMap, "time"));
					var rgba:Color = Color.fromString(keyMap.color);

					frame = 0;
					bezier = 0;
					while (true) {
						rgbaTimeline.setFrame(frame, time, rgba.r, rgba.g, rgba.b, rgba.a);
						if (timelineMap.length == frame + 1)
							break;

						nextMap = timelineMap[frame + 1];
						time2 = getFloat(Reflect.getProperty(nextMap, "time"));
						var newRgba:Color = Color.fromString(nextMap.color);
						curve = keyMap.curve;
						if (curve != null) {
							bezier = readCurve(curve, rgbaTimeline, bezier, frame, 0, time, time2, rgba.r, newRgba.r, 1);
							bezier = readCurve(curve, rgbaTimeline, bezier, frame, 1, time, time2, rgba.g, newRgba.g, 1);
							bezier = readCurve(curve, rgbaTimeline, bezier, frame, 2, time, time2, rgba.b, newRgba.b, 1);
							bezier = readCurve(curve, rgbaTimeline, bezier, frame, 3, time, time2, rgba.a, newRgba.a, 1);
						}
						time = time2;
						rgba = newRgba;
						keyMap = nextMap;

						frame++;
					}

					timelines.push(rgbaTimeline);
				} else if (timelineName == "rgb") {
					var rgbTimeline:RGBTimeline = new RGBTimeline(timelineMap.length, timelineMap.length * 3, slotIndex);
					keyMap = timelineMap[0];
					time = getFloat(Reflect.getProperty(keyMap, "time"));
					var rgb:Color = Color.fromString(keyMap.color);

					frame = 0;
					bezier = 0;
					while (true) {
						rgbTimeline.setFrame(frame, time, rgb.r, rgb.g, rgb.b);
						nextMap = timelineMap[frame + 1];
						if (nextMap == null) {
							rgbTimeline.shrink(bezier);
							break;
						}

						time2 = getFloat(Reflect.getProperty(nextMap, "time"));
						var newRgb:Color = Color.fromString(nextMap.color);
						curve = keyMap.curve;
						if (curve != null) {
							bezier = readCurve(curve, rgbTimeline, bezier, frame, 0, time, time2, rgb.r, newRgb.r, 1);
							bezier = readCurve(curve, rgbTimeline, bezier, frame, 1, time, time2, rgb.g, newRgb.g, 1);
							bezier = readCurve(curve, rgbTimeline, bezier, frame, 2, time, time2, rgb.b, newRgb.b, 1);
						}
						time = time2;
						rgb = newRgb;
						keyMap = nextMap;

						frame++;
					}

					timelines.push(rgbTimeline);
				} else if (timelineName == "alpha") {
					timelines.push(readTimeline(timelineMap, new AlphaTimeline(timelineMap.length, timelineMap.length, slotIndex), 0, 1));
				} else if (timelineName == "rgba2") {
					var rgba2Timeline:RGBA2Timeline = new RGBA2Timeline(timelineMap.length, timelineMap.length * 7, slotIndex);

					keyMap = timelineMap[0];
					time = getFloat(Reflect.getProperty(keyMap, "time"));
					var lighta:Color = Color.fromString(keyMap.light);
					var darka:Color = Color.fromString(keyMap.dark);

					frame = 0;
					bezier = 0;
					while (true) {
						rgba2Timeline.setFrame(frame, time, lighta.r, lighta.g, lighta.b, lighta.a, darka.r, darka.g, darka.b);
						nextMap = timelineMap[frame + 1];
						if (nextMap == null) {
							rgba2Timeline.shrink(bezier);
							break;
						}

						time2 = getFloat(Reflect.getProperty(nextMap, "time"));
						var newLighta:Color = Color.fromString(nextMap.light);
						var newDarka:Color = Color.fromString(nextMap.dark);
						curve = keyMap.curve;
						if (curve != null) {
							bezier = readCurve(curve, rgba2Timeline, bezier, frame, 0, time, time2, lighta.r, newLighta.r, 1);
							bezier = readCurve(curve, rgba2Timeline, bezier, frame, 1, time, time2, lighta.g, newLighta.g, 1);
							bezier = readCurve(curve, rgba2Timeline, bezier, frame, 2, time, time2, lighta.b, newLighta.b, 1);
							bezier = readCurve(curve, rgba2Timeline, bezier, frame, 3, time, time2, lighta.a, newLighta.a, 1);
							bezier = readCurve(curve, rgba2Timeline, bezier, frame, 4, time, time2, darka.r, newDarka.r, 1);
							bezier = readCurve(curve, rgba2Timeline, bezier, frame, 5, time, time2, darka.g, newDarka.g, 1);
							bezier = readCurve(curve, rgba2Timeline, bezier, frame, 6, time, time2, darka.b, newDarka.b, 1);
						}
						time = time2;
						lighta = newLighta;
						darka = newDarka;
						keyMap = nextMap;

						frame++;
					}

					timelines.push(rgba2Timeline);
				} else if (timelineName == "rgb2") {
					var rgb2Timeline:RGB2Timeline = new RGB2Timeline(timelineMap.length, timelineMap.length * 6, slotIndex);

					keyMap = timelineMap[0];
					time = getFloat(Reflect.getProperty(keyMap, "time"));
					var light:Color = Color.fromString(keyMap.light);
					var dark:Color = Color.fromString(keyMap.dark);

					frame = 0;
					bezier = 0;
					while (true) {
						rgb2Timeline.setFrame(frame, time, light.r, light.g, light.b, dark.r, dark.g, dark.b);
						nextMap = timelineMap[frame + 1];
						if (nextMap == null) {
							rgb2Timeline.shrink(bezier);
							break;
						}

						time2 = getFloat(Reflect.getProperty(nextMap, "time"));
						var newLight:Color = Color.fromString(nextMap.light);
						var newDark:Color = Color.fromString(nextMap.dark);
						curve = keyMap.curve;
						if (curve != null) {
							bezier = readCurve(curve, rgb2Timeline, bezier, frame, 0, time, time2, light.r, newLight.r, 1);
							bezier = readCurve(curve, rgb2Timeline, bezier, frame, 1, time, time2, light.g, newLight.g, 1);
							bezier = readCurve(curve, rgb2Timeline, bezier, frame, 2, time, time2, light.b, newLight.b, 1);
							bezier = readCurve(curve, rgb2Timeline, bezier, frame, 3, time, time2, dark.r, newDark.r, 1);
							bezier = readCurve(curve, rgb2Timeline, bezier, frame, 4, time, time2, dark.g, newDark.g, 1);
							bezier = readCurve(curve, rgb2Timeline, bezier, frame, 5, time, time2, dark.b, newDark.b, 1);
						}
						time = time2;
						light = newLight;
						dark = newDark;
						keyMap = nextMap;

						frame++;
					}

					timelines.push(rgb2Timeline);
				} else {
					throw new Error("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
				}
			}
		}

		// Bone timelines.
		var bones:Object = Reflect.getProperty(map, "bones");
		for (boneName in bones) {
			var boneIndex:Int = skeletonData.findBoneIndex(boneName);
			if (boneIndex == -1)
				throw new Error("Bone not found: " + boneName);
			var boneMap:Object = bones[boneName];
			for (timelineName in boneMap) {
				timelineMap = boneMap[timelineName];
				if (timelineMap.length == 0)
					continue;

				if (timelineName == "rotate") {
					timelines.push(readTimeline(timelineMap, new RotateTimeline(timelineMap.length, timelineMap.length, boneIndex), 0, 1));
				} else if (timelineName == "translate") {
					var translateTimeline:TranslateTimeline = new TranslateTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
					timelines.push(readTimeline2(timelineMap, translateTimeline, "x", "y", 0, scale));
				} else if (timelineName == "translatex") {
					var translateXTimeline:TranslateXTimeline = new TranslateXTimeline(timelineMap.length, timelineMap.length, boneIndex);
					timelines.push(readTimeline(timelineMap, translateXTimeline, 0, scale));
				} else if (timelineName == "translatey") {
					var translateYTimeline:TranslateYTimeline = new TranslateYTimeline(timelineMap.length, timelineMap.length, boneIndex);
					timelines.push(readTimeline(timelineMap, translateYTimeline, 0, scale));
				} else if (timelineName == "scale") {
					var scaleTimeline:ScaleTimeline = new ScaleTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
					timelines.push(readTimeline2(timelineMap, scaleTimeline, "x", "y", 1, 1));
				} else if (timelineName == "scalex") {
					var scaleXTimeline:ScaleXTimeline = new ScaleXTimeline(timelineMap.length, timelineMap.length, boneIndex);
					timelines.push(readTimeline(timelineMap, scaleXTimeline, 1, 1));
				} else if (timelineName == "scaley") {
					var scaleYTimeline:ScaleYTimeline = new ScaleYTimeline(timelineMap.length, timelineMap.length, boneIndex);
					timelines.push(readTimeline(timelineMap, scaleYTimeline, 1, 1));
				} else if (timelineName == "shear") {
					var shearTimeline:ShearTimeline = new ShearTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
					timelines.push(readTimeline2(timelineMap, shearTimeline, "x", "y", 0, 1));
				} else if (timelineName == "shearx") {
					var shearXTimeline:ShearXTimeline = new ShearXTimeline(timelineMap.length, timelineMap.length, boneIndex);
					timelines.push(readTimeline(timelineMap, shearXTimeline, 0, 1));
				} else if (timelineName == "sheary") {
					var shearYTimeline:ShearYTimeline = new ShearYTimeline(timelineMap.length, timelineMap.length, boneIndex);
					timelines.push(readTimeline(timelineMap, shearYTimeline, 0, 1));
				} else {
					throw new Error("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
				}
			}
		}

		// IK constraint timelines.
		var iks:Object = Reflect.getProperty(map, "ik");
		for (ikConstraintName in iks) {
			timelineMap = iks[ikConstraintName];
			keyMap = timelineMap[0];
			if (keyMap == null)
				continue;

			var ikIndex:Int = skeletonData.ikConstraints.indexOf(skeletonData.findIkConstraint(ikConstraintName));
			var ikTimeline:IkConstraintTimeline = new IkConstraintTimeline(timelineMap.length, timelineMap.length << 1, ikIndex);

			time = getFloat(Reflect.getProperty(keyMap, "time"));
			var mix:Float = getFloat(Reflect.getProperty(keyMap, "mix"), 1);
			var softness:Float = getFloat(Reflect.getProperty(keyMap, "softness")) * scale;

			frame = 0;
			bezier = 0;
			while (true) {
				ikTimeline.setFrame(frame, time, mix, softness,
					Reflect.hasField(keyMap, "bendPositive") ? (cast(Reflect.getProperty(keyMap, "bendPositive"), Bool) ? 1 : -1) : 1,
					Reflect.hasField(keyMap, "compress") ? cast(Reflect.getProperty(keyMap, "compress"), Bool) : false,
					Reflect.hasField(keyMap, "stretch") ? cast(Reflect.getProperty(keyMap, "stretch"), Bool) : false);

				nextMap = timelineMap[frame + 1];
				if (nextMap == null) {
					ikTimeline.shrink(bezier);
					break;
				}

				time2 = getFloat(Reflect.getProperty(nextMap, "time"));
				var mix2:Float = getFloat(Reflect.getProperty(nextMap, "mix"), 1);
				var softness2:Float = getFloat(Reflect.getProperty(nextMap, "softness")) * scale;
				curve = keyMap.curve;
				if (curve != null) {
					bezier = readCurve(curve, ikTimeline, bezier, frame, 0, time, time2, mix, mix2, 1);
					bezier = readCurve(curve, ikTimeline, bezier, frame, 1, time, time2, softness, softness2, scale);
				}
				time = time2;
				mix = mix2;
				softness = softness2;
				keyMap = nextMap;

				frame++;
			}
			timelines.push(ikTimeline);
		}

		// Transform constraint timelines.
		var mixRotate:Float, mixRotate2:Float;
		var mixX:Float, mixX2:Float;
		var mixY:Float, mixY2:Float;
		var transforms:Object = Reflect.getProperty(map, "transform");
		for (transformName in transforms) {
			timelineMap = transforms[transformName];
			keyMap = timelineMap[0];
			if (keyMap == null)
				continue;

			var transformIndex:Int = skeletonData.transformConstraints.indexOf(skeletonData.findTransformConstraint(transformName));
			var transformTimeline:TransformConstraintTimeline = new TransformConstraintTimeline(timelineMap.length, timelineMap.length << 2, transformIndex);

			time = getFloat(Reflect.getProperty(keyMap, "time"));
			mixRotate = getFloat(Reflect.getProperty(keyMap, "mixRotate"), 1);
			var mixShearY:Float = getFloat(Reflect.getProperty(keyMap, "mixShearY"), 1);
			mixX = getFloat(Reflect.getProperty(keyMap, "mixX"), 1);
			mixY = getFloat(Reflect.getProperty(keyMap, "mixY"), mixX);
			var mixScaleX:Float = getFloat(Reflect.getProperty(keyMap, "mixScaleX"), 1);
			var mixScaleY:Float = getFloat(Reflect.getProperty(keyMap, "mixScaleY"), mixScaleX);

			frame = 0;
			bezier = 0;
			while (true) {
				transformTimeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
				nextMap = timelineMap[frame + 1];
				if (nextMap == null) {
					transformTimeline.shrink(bezier);
					break;
				}

				time2 = getFloat(Reflect.getProperty(nextMap, "time"));
				mixRotate2 = getFloat(Reflect.getProperty(nextMap, "mixRotate"), 1);
				var mixShearY2:Float = getFloat(Reflect.getProperty(nextMap, "mixShearY"), 1);
				mixX2 = getFloat(Reflect.getProperty(nextMap, "mixX"), 1);
				mixY2 = getFloat(Reflect.getProperty(nextMap, "mixY"), mixX2);
				var mixScaleX2:Float = getFloat(Reflect.getProperty(nextMap, "mixScaleX"), 1);
				var mixScaleY2:Float = getFloat(Reflect.getProperty(nextMap, "mixScaleY"), mixScaleX2);
				curve = keyMap.curve;
				if (curve != null) {
					bezier = readCurve(curve, transformTimeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
					bezier = readCurve(curve, transformTimeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
					bezier = readCurve(curve, transformTimeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
					bezier = readCurve(curve, transformTimeline, bezier, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
					bezier = readCurve(curve, transformTimeline, bezier, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
					bezier = readCurve(curve, transformTimeline, bezier, frame, 5, time, time2, mixShearY, mixShearY2, 1);
				}
				time = time2;
				mixRotate = mixRotate2;
				mixX = mixX2;
				mixY = mixY2;
				mixScaleX = mixScaleX2;
				mixScaleY = mixScaleY2;
				mixScaleX = mixScaleX2;
				keyMap = nextMap;

				frame++;
			}

			timelines.push(transformTimeline);
		}

		// Path constraint timelines.
		var paths:Object = Reflect.getProperty(map, "path");
		for (pathName in paths) {
			var index:Int = skeletonData.findPathConstraintIndex(pathName);
			if (index == -1)
				throw new Error("Path constraint not found: " + pathName);
			var pathData:PathConstraintData = skeletonData.pathConstraints[index];

			var pathMap:Object = paths[pathName];
			for (timelineName in pathMap) {
				timelineMap = pathMap[timelineName];
				keyMap = timelineMap[0];
				if (keyMap == null)
					continue;

				if (timelineName == "position") {
					var positionTimeline:PathConstraintPositionTimeline = new PathConstraintPositionTimeline(timelineMap.length, timelineMap.length, index);
					timelines.push(readTimeline(timelineMap, positionTimeline, 0, pathData.positionMode == PositionMode.fixed ? scale : 1));
				} else if (timelineName == "spacing") {
					var spacingTimeline:PathConstraintSpacingTimeline = new PathConstraintSpacingTimeline(timelineMap.length, timelineMap.length, index);
					timelines.push(readTimeline(timelineMap, spacingTimeline,
						0, pathData.spacingMode == SpacingMode.length || pathData.spacingMode == SpacingMode.fixed ? scale : 1));
				} else if (timelineName == "mix") {
					var mixTimeline:PathConstraintMixTimeline = new PathConstraintMixTimeline(timelineMap.length, timelineMap.length * 3, index);
					time = getFloat(Reflect.getProperty(keyMap, "time"));
					mixRotate = getFloat(Reflect.getProperty(keyMap, "mixRotate"), 1);
					mixX = getFloat(Reflect.getProperty(keyMap, "mixX"), 1);
					mixY = getFloat(Reflect.getProperty(keyMap, "mixY"), mixX);

					frame = 0;
					bezier = 0;
					while (true) {
						mixTimeline.setFrame(frame, time, mixRotate, mixX, mixY);
						nextMap = timelineMap[frame + 1];
						if (nextMap == null) {
							mixTimeline.shrink(bezier);
							break;
						}
						time2 = getFloat(Reflect.getProperty(nextMap, "time"));
						mixRotate2 = getFloat(Reflect.getProperty(nextMap, "mixRotate"), 1);
						mixX2 = getFloat(Reflect.getProperty(nextMap, "mixX"), 1);
						mixY2 = getFloat(Reflect.getProperty(nextMap, "mixY"), mixX2);
						curve = keyMap.curve;
						if (curve != null) {
							bezier = readCurve(curve, mixTimeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
							bezier = readCurve(curve, mixTimeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
							bezier = readCurve(curve, mixTimeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
						}
						time = time2;
						mixRotate = mixRotate2;
						mixX = mixX2;
						mixY = mixY2;
						keyMap = nextMap;

						frame++;
					}

					timelines.push(mixTimeline);
				}
			}
		}

		// Deform timelines.
		var deforms:Object = Reflect.getProperty(map, "deform");
		for (deformName in deforms) {
			var deformMap:Object = deforms[deformName];
			var skin:Skin = skeletonData.findSkin(deformName);
			if (skin == null)
				throw new Error("Skin not found: " + deformName);

			for (slotName in deformMap) {
				slotMap = deformMap[slotName];
				slotIndex = skeletonData.findSlot(slotName).index;
				if (slotIndex == -1)
					throw new Error("Slot not found: " + slotName);
				for (timelineName in slotMap) {
					timelineMap = slotMap[timelineName];
					keyMap = timelineMap[0];
					if (keyMap == null)
						continue;

					var attachment:VertexAttachment = cast(skin.getAttachment(slotIndex, timelineName), VertexAttachment);
					if (attachment == null)
						throw new Error("Deform attachment not found: " + timelineName);
					var weighted:Bool = attachment.bones != null;
					var vertices:Vector<Float> = attachment.vertices;
					var deformLength:Int = weighted ? Std.int(vertices.length / 3 * 2) : vertices.length;

					var deformTimeline:DeformTimeline = new DeformTimeline(timelineMap.length, timelineMap.length, slotIndex, attachment);
					time = getFloat(Reflect.getProperty(keyMap, "time"));
					frame = 0;
					bezier = 0;
					while (true) {
						var deform:Vector<Float>;
						var verticesValue:Vector<Float> = Reflect.getProperty(keyMap, "vertices");
						if (verticesValue == null) {
							deform = weighted ? new Vector<Float>(deformLength, true) : vertices;
						} else {
							deform = new Vector<Float>(deformLength, true);
							var start:Int = getInt(Reflect.getProperty(keyMap, "offset"));
							var temp:Vector<Float> = getFloatArray(keyMap, "vertices");
							for (i in 0...temp.length) {
								deform[start + i] = temp[i];
							}
							if (scale != 1) {
								for (i in start...start + temp.length) {
									deform[i] *= scale;
								}
							}
							if (!weighted) {
								for (i in 0...deformLength) {
									deform[i] += vertices[i];
								}
							}
						}

						deformTimeline.setFrame(frame, time, deform);
						nextMap = timelineMap[frame + 1];
						if (nextMap == null) {
							deformTimeline.shrink(bezier);
							break;
						}
						time2 = getFloat(Reflect.getProperty(nextMap, "time"));
						curve = keyMap.curve;
						if (curve != null) {
							bezier = readCurve(curve, deformTimeline, bezier, frame, 0, time, time2, 0, 1, 1);
						}
						time = time2;
						keyMap = nextMap;

						frame++;
					}

					timelines.push(deformTimeline);
				}
			}
		}

		// Draw order timelines.
		if (Reflect.hasField(map, "drawOrder")) {
			var drawOrders:Array<Dynamic> = cast(map["drawOrder"], Array<Dynamic>);
			if (drawOrders != null) {
				var drawOrderTimeline:DrawOrderTimeline = new DrawOrderTimeline(drawOrders.length);
				var slotCount:Int = skeletonData.slots.length;
				frame = 0;
				for (drawOrderMap in drawOrders) {
					var drawOrder:Vector<Int> = null;
					var offsets:Array<Dynamic> = Reflect.getProperty(drawOrderMap, "offsets");
					if (offsets != null) {
						drawOrder = new Vector<Int>(slotCount, true);
						var i = slotCount - 1;
						while (i >= 0) {
							drawOrder[i--] = -1;
						}
						var unchanged:Vector<Int> = new Vector<Int>(slotCount - offsets.length, true);
						var originalIndex:Int = 0, unchangedIndex:Int = 0;
						for (offsetMap in offsets) {
							slotIndex = skeletonData.findSlot(Reflect.getProperty(offsetMap, "slot")).index;
							if (slotIndex == -1)
								throw new Error("Slot not found: " + Reflect.getProperty(offsetMap, "slot"));
							// Collect unchanged items.
							while (originalIndex != slotIndex) {
								unchanged[unchangedIndex++] = originalIndex++;
							}
							// Set changed items.
							drawOrder[originalIndex + Reflect.getProperty(offsetMap, "offset")] = originalIndex++;
						}
						// Collect remaining unchanged items.
						while (originalIndex < slotCount) {
							unchanged[unchangedIndex++] = originalIndex++;
						}
						// Fill in unchanged items.
						i = slotCount - 1;
						while (i >= 0) {
							if (drawOrder[i] == -1)
								drawOrder[i] = unchanged[--unchangedIndex];
							i--;
						}
					}
					drawOrderTimeline.setFrame(frame++, getFloat(Reflect.getProperty(drawOrderMap, "time")), drawOrder);
				}
				timelines.push(drawOrderTimeline);
			}
		}

		// Event timelines.
		if (Reflect.hasField(map, "events")) {
			var eventsMap:Array<Dynamic> = cast(map["events"], Array<Dynamic>);
			if (eventsMap != null) {
				var eventTimeline:EventTimeline = new EventTimeline(eventsMap.length);
				frame = 0;
				for (eventMap in eventsMap) {
					var eventData:EventData = skeletonData.findEvent(Reflect.getProperty(eventMap, "name"));
					if (eventData == null)
						throw new Error("Event not found: " + Reflect.getProperty(eventMap, "name"));
					var event:Event = new Event(getFloat(Reflect.getProperty(eventMap, "time")), eventData);
					event.intValue = Reflect.hasField(eventMap, "int") ? getInt(Reflect.getProperty(eventMap, "int")) : eventData.intValue;
					event.floatValue = Reflect.hasField(eventMap, "float") ? getFloat(Reflect.getProperty(eventMap, "float")) : eventData.floatValue;
					event.stringValue = Reflect.hasField(eventMap, "string") ? Reflect.getProperty(eventMap, "string") : eventData.stringValue;
					if (eventData.audioPath != null) {
						event.volume = getFloat(Reflect.getProperty(eventMap, "volume"), 1);
						event.balance = getFloat(Reflect.getProperty(eventMap, "balance"));
					}
					eventTimeline.setFrame(frame++, event);
				}
				timelines.push(eventTimeline);
			}
		}

		var duration:Float = 0;
		for (i in 0...timelines.length) {
			duration = Math.max(duration, timelines[i].getDuration());
		}

		skeletonData.animations.push(new Animation(name, timelines, duration));
	}

	static private function readTimeline(keys:Array<Dynamic>, timeline:CurveTimeline1, defaultValue:Float, scale:Float):CurveTimeline1 {
		var keyMap:Object = keys[0];
		var time:Float = getFloat(Reflect.getProperty(keyMap, "time"));
		var value:Float = getFloat(Reflect.getProperty(keyMap, "value"), defaultValue) * scale;
		var bezier:Int = 0;
		var frame:Int = 0;
		while (true) {
			timeline.setFrame(frame, time, value);
			var nextMap:Object = keys[frame + 1];
			if (nextMap == null) {
				timeline.shrink(bezier);
				break;
			}
			var time2:Float = getFloat(Reflect.getProperty(nextMap, "time"));
			var value2:Float = getFloat(Reflect.getProperty(nextMap, "value"), defaultValue) * scale;
			var curve:Object = keyMap.curve;
			if (curve != null) {
				bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value, value2, scale);
			}
			time = time2;
			value = value2;
			keyMap = nextMap;

			frame++;
		}
		return timeline;
	}

	static private function readTimeline2(keys:Array<Dynamic>, timeline:CurveTimeline2, name1:String, name2:String, defaultValue:Float,
			scale:Float):CurveTimeline2 {
		var keyMap:Object = keys[0];
		var time:Float = getFloat(Reflect.getProperty(keyMap, "time"));
		var value1:Float = getFloat(Reflect.getProperty(keyMap, name1), defaultValue) * scale;
		var value2:Float = getFloat(Reflect.getProperty(keyMap, name2), defaultValue) * scale;
		var bezier:Int = 0;
		var frame:Int = 0;
		while (true) {
			timeline.setFrame(frame, time, value1, value2);
			var nextMap:Object = keys[frame + 1];
			if (nextMap == null) {
				timeline.shrink(bezier);
				break;
			}
			var time2:Float = getFloat(Reflect.getProperty(nextMap, "time"));
			var nvalue1:Float = getFloat(Reflect.getProperty(nextMap, name1), defaultValue) * scale;
			var nvalue2:Float = getFloat(Reflect.getProperty(nextMap, name2), defaultValue) * scale;
			var curve:Object = keyMap.curve;
			if (curve != null) {
				bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value1, nvalue1, scale);
				bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, value2, nvalue2, scale);
			}
			time = time2;
			value1 = nvalue1;
			value2 = nvalue2;
			keyMap = nextMap;

			frame++;
		}
		return timeline;
	}

	static private function readCurve(curve:Object, timeline:CurveTimeline, bezier:Int, frame:Int, value:Int, time1:Float, time2:Float, value1:Float,
			value2:Float, scale:Float):Int {
		if (curve == "stepped") {
			timeline.setStepped(frame);
			return bezier;
		}

		var i:Int = value << 2;
		var cx1:Float = curve[Std.string(i)];
		var cy1:Float = curve[Std.string(i + 1)] * scale;
		var cx2:Float = curve[Std.string(i + 2)];
		var cy2:Float = curve[Std.string(i + 3)] * scale;
		timeline.setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
		return bezier + 1;
	}

	static private function getFloat(value:Object, defaultValue:Float = 0):Float {
		if (Std.isOfType(value, Float))
			return cast(value, Float);
		var floatValue:Float = Std.parseFloat(value);
		if (Math.isNaN(floatValue))
			floatValue = defaultValue;
		return floatValue;
	}

	static private function getFloatArray(map:Object, name:String):Vector<Float> {
		var list:Array<Dynamic> = cast(map[name], Array<Dynamic>);
		var values:Vector<Float> = new Vector<Float>(list.length, true);
		for (i in 0...list.length) {
			values[i] = getFloat(list[i]);
		}
		return values;
	}

	static private function getInt(value:Object):Int {
		if (Std.isOfType(value, Int))
			return cast(value, Int);
		var intValue:Null<Int> = Std.parseInt(value);
		if (intValue == null)
			intValue = 0;
		return intValue;
	}

	static private function getIntArray(map:Object, name:String):Vector<Int> {
		var list:Array<Dynamic> = cast(map[name], Array<Dynamic>);
		var values:Vector<Int> = new Vector<Int>(list.length, true);
		for (i in 0...list.length) {
			values[i] = getInt(list[i]);
		}
		return values;
	}
}

class LinkedMesh {
	public var parent(default, null):String;
	public var skin(default, null):String;
	public var slotIndex(default, null):Int;
	public var mesh(default, null):MeshAttachment;
	public var inheritDeform(default, null):Bool;

	public function new(mesh:MeshAttachment, skin:String, slotIndex:Int, parent:String, inheritDeform:Bool) {
		this.mesh = mesh;
		this.skin = skin;
		this.slotIndex = slotIndex;
		this.parent = parent;
		this.inheritDeform = inheritDeform;
	}
}
