/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine {
import flash.utils.ByteArray;

import spine.animation.Animation;
import spine.animation.AttachmentTimeline;
import spine.animation.ColorTimeline;
import spine.animation.CurveTimeline;
import spine.animation.DrawOrderTimeline;
import spine.animation.EventTimeline;
import spine.animation.FfdTimeline;
import spine.animation.FlipXTimeline;
import spine.animation.FlipYTimeline;
import spine.animation.IkConstraintTimeline;
import spine.animation.RotateTimeline;
import spine.animation.ScaleTimeline;
import spine.animation.Timeline;
import spine.animation.TranslateTimeline;
import spine.attachments.Attachment;
import spine.attachments.AttachmentLoader;
import spine.attachments.AttachmentType;
import spine.attachments.BoundingBoxAttachment;
import spine.attachments.MeshAttachment;
import spine.attachments.RegionAttachment;
import spine.attachments.SkinnedMeshAttachment;

public class SkeletonJson {
	public var attachmentLoader:AttachmentLoader;
	public var scale:Number = 1;

	public function SkeletonJson (attachmentLoader:AttachmentLoader = null) {
		this.attachmentLoader = attachmentLoader;
	}

	/** @param object A String or ByteArray. */
	public function readSkeletonData (object:*, name:String = null) : SkeletonData {
		if (object == null) throw new ArgumentError("object cannot be null.");

		var root:Object;
		if (object is String)
			root = JSON.parse(String(object));
		else if (object is ByteArray)
			root = JSON.parse(object.readUTFBytes(object.length));
		else if (object is Object)
			root = object;
		else
			throw new ArgumentError("object must be a String, ByteArray or Object.");

		var skeletonData:SkeletonData = new SkeletonData();
		skeletonData.name = name;

		// Skeleton.
		var skeletonMap:Object = root["skeleton"];
		if (skeletonMap) {
			skeletonData.hash = skeletonMap["hash"];
			skeletonData.version = skeletonMap["spine"];
			skeletonData.width = skeletonMap["width"] || 0;
			skeletonData.height = skeletonMap["height"] || 0;
		}

		// Bones.
		var boneData:BoneData;
		for each (var boneMap:Object in root["bones"]) {
			var parent:BoneData = null;
			var parentName:String = boneMap["parent"];
			if (parentName) {
				parent = skeletonData.findBone(parentName);
				if (!parent) throw new Error("Parent bone not found: " + parentName);
			}
			boneData = new BoneData(boneMap["name"], parent);
			boneData.length = (boneMap["length"] || 0) * scale;
			boneData.x = (boneMap["x"] || 0) * scale;
			boneData.y = (boneMap["y"] || 0) * scale;
			boneData.rotation = (boneMap["rotation"] || 0);
			boneData.scaleX = boneMap.hasOwnProperty("scaleX") ? boneMap["scaleX"] : 1;
			boneData.scaleY = boneMap.hasOwnProperty("scaleY") ? boneMap["scaleY"] : 1;
			boneData.flipX = boneMap["flipX"] || false;
			boneData.flipY = boneMap["flipY"] || false;
			boneData.inheritScale = boneMap.hasOwnProperty("inheritScale") ? boneMap["inheritScale"] : true;
			boneData.inheritRotation = boneMap.hasOwnProperty("inheritRotation") ? boneMap["inheritRotation"] : true;
			skeletonData.bones[skeletonData.bones.length] = boneData;
		}

		// IK constraints.
		for each (var ikMap:Object in root["ik"]) {
			var ikConstraintData:IkConstraintData = new IkConstraintData(ikMap["name"]);

			for each (var boneName:String in ikMap["bones"]) {
				var bone:BoneData = skeletonData.findBone(boneName);
				if (!bone) throw new Error("IK bone not found: " + boneName);
				ikConstraintData.bones[ikConstraintData.bones.length] = bone;
			}

			ikConstraintData.target = skeletonData.findBone(ikMap["target"]);
			if (!ikConstraintData.target) throw new Error("Target bone not found: " + ikMap["target"]);

			ikConstraintData.bendDirection = (!ikMap.hasOwnProperty("bendPositive") || ikMap["bendPositive"]) ? 1 : -1;
			ikConstraintData.mix = ikMap.hasOwnProperty("mix") ? ikMap["mix"] : 1;

			skeletonData.ikConstraints[skeletonData.ikConstraints.length] = ikConstraintData;
		}

		// Slots.
		for each (var slotMap:Object in root["slots"]) {
			boneName = slotMap["bone"];
			boneData = skeletonData.findBone(boneName);
			if (!boneData) throw new Error("Slot bone not found: " + boneName);
			var slotData:SlotData = new SlotData(slotMap["name"], boneData);

			var color:String = slotMap["color"];
			if (color) {
				slotData.r = toColor(color, 0);
				slotData.g = toColor(color, 1);
				slotData.b = toColor(color, 2);
				slotData.a = toColor(color, 3);
			}

			slotData.attachmentName = slotMap["attachment"];
			slotData.additiveBlending = slotMap["additive"];

			skeletonData.slots[skeletonData.slots.length] = slotData;
		}

		// Skins.
		var skins:Object = root["skins"];
		for (var skinName:String in skins) {
			var skinMap:Object = skins[skinName];
			var skin:Skin = new Skin(skinName);
			for (var slotName:String in skinMap) {
				var slotIndex:int = skeletonData.findSlotIndex(slotName);
				var slotEntry:Object = skinMap[slotName];
				for (var attachmentName:String in slotEntry) {
					var attachment:Attachment = readAttachment(skin, attachmentName, slotEntry[attachmentName]);
					if (attachment != null)
						skin.addAttachment(slotIndex, attachmentName, attachment);
				}
			}
			skeletonData.skins[skeletonData.skins.length] = skin;
			if (skin.name == "default")
				skeletonData.defaultSkin = skin;
		}

		// Events.
		var events:Object = root["events"];
		if (events) {
			for (var eventName:String in events) {
				var eventMap:Object = events[eventName];
				var eventData:EventData = new EventData(eventName);
				eventData.intValue = eventMap["int"] || 0;
				eventData.floatValue = eventMap["float"] || 0;
				eventData.stringValue = eventMap["string"] || null;
				skeletonData.events[skeletonData.events.length] = eventData;
			}
		}

		// Animations.
		var animations:Object = root["animations"];
		for (var animationName:String in animations)
			readAnimation(animationName, animations[animationName], skeletonData);

		return skeletonData;
	}

	private function readAttachment (skin:Skin, name:String, map:Object) : Attachment {
		name = map["name"] || name;

		var type:AttachmentType = AttachmentType[map["type"] || "region"];
		var path:String = map["path"] || name;

		var scale:Number = this.scale;
		var color:String, vertices:Vector.<Number>;
		switch (type) {
		case AttachmentType.region:
			var region:RegionAttachment = attachmentLoader.newRegionAttachment(skin, name, path);
			if (!region) return null;
			region.path = path;
			region.x = (map["x"] || 0) * scale;
			region.y = (map["y"] || 0) * scale;
			region.scaleX = map.hasOwnProperty("scaleX") ? map["scaleX"] : 1;
			region.scaleY = map.hasOwnProperty("scaleY") ? map["scaleY"] : 1;
			region.rotation = map["rotation"] || 0;
			region.width = (map["width"] || 0) * scale;
			region.height = (map["height"] || 0) * scale;
			
			color = map["color"];
			if (color) {
				region.r = toColor(color, 0);
				region.g = toColor(color, 1);
				region.b = toColor(color, 2);
				region.a = toColor(color, 3);
			}
			
			region.updateOffset();
			return region;

		case AttachmentType.mesh:
			var mesh:MeshAttachment = attachmentLoader.newMeshAttachment(skin, name, path);
			if (!mesh) return null;
			mesh.path = path; 
			mesh.vertices = getFloatArray(map, "vertices", scale);
			mesh.triangles = getUintArray(map, "triangles");
			mesh.regionUVs = getFloatArray(map, "uvs", 1);
			mesh.updateUVs();

			color = map["color"];
			if (color) {
				mesh.r = toColor(color, 0);
				mesh.g = toColor(color, 1);
				mesh.b = toColor(color, 2);
				mesh.a = toColor(color, 3);
			}

			mesh.hullLength = (map["hull"] || 0) * 2;
			if (map["edges"]) mesh.edges = getIntArray(map, "edges");
			mesh.width = (map["width"] || 0) * scale;
			mesh.height = (map["height"] || 0) * scale;
			return mesh;
		case AttachmentType.skinnedmesh:
			var skinnedMesh:SkinnedMeshAttachment = attachmentLoader.newSkinnedMeshAttachment(skin, name, path);
			if (!skinnedMesh) return null;
			skinnedMesh.path = path;

			var uvs:Vector.<Number> = getFloatArray(map, "uvs", 1);
			vertices = getFloatArray(map, "vertices", 1);
			var weights:Vector.<Number> = new Vector.<Number>();
			var bones:Vector.<int> = new Vector.<int>();
			for (var i:int = 0, n:int = vertices.length; i < n; ) {
				var boneCount:int = int(vertices[i++]);
				bones[bones.length] = boneCount;
				for (var nn:int = i + boneCount * 4; i < nn; ) {
					bones[bones.length] = vertices[i];
					weights[weights.length] = vertices[i + 1] * scale;
					weights[weights.length] = vertices[i + 2] * scale;
					weights[weights.length] = vertices[i + 3];
					i += 4;
				}
			}
			skinnedMesh.bones = bones;
			skinnedMesh.weights = weights;
			skinnedMesh.triangles = getUintArray(map, "triangles");
			skinnedMesh.regionUVs = uvs;
			skinnedMesh.updateUVs();
			
			color = map["color"];
			if (color) {
				skinnedMesh.r = toColor(color, 0);
				skinnedMesh.g = toColor(color, 1);
				skinnedMesh.b = toColor(color, 2);
				skinnedMesh.a = toColor(color, 3);
			}
			
			skinnedMesh.hullLength = (map["hull"] || 0) * 2;
			if (map["edges"]) skinnedMesh.edges = getIntArray(map, "edges");
			skinnedMesh.width = (map["width"] || 0) * scale;
			skinnedMesh.height = (map["height"] || 0) * scale;
			return skinnedMesh;
		case AttachmentType.boundingbox:
			var box:BoundingBoxAttachment = attachmentLoader.newBoundingBoxAttachment(skin, name);
			vertices = box.vertices;
			for each (var point:Number in map["vertices"])
				vertices[vertices.length] = point * scale;
			return box;
		}

		return null;
	}

	private function readAnimation (name:String, map:Object, skeletonData:SkeletonData) : void {
		var timelines:Vector.<Timeline> = new Vector.<Timeline>();
		var duration:Number = 0;

		var slotMap:Object, slotIndex:int, slotName:String;
		var values:Array, valueMap:Object, frameIndex:int;
		var i:int;
		var timelineName:String;

		var slots:Object = map["slots"];
		for (slotName in slots) {
			slotMap = slots[slotName];
			slotIndex = skeletonData.findSlotIndex(slotName);

			for (timelineName in slotMap) {
				values = slotMap[timelineName];
				if (timelineName == "color") {
					var colorTimeline:ColorTimeline = new ColorTimeline(values.length);
					colorTimeline.slotIndex = slotIndex;
					
					frameIndex = 0;
					for each (valueMap in values) {
						var color:String = valueMap["color"];
						var r:Number = toColor(color, 0);
						var g:Number = toColor(color, 1);
						var b:Number = toColor(color, 2);
						var a:Number = toColor(color, 3);
						colorTimeline.setFrame(frameIndex, valueMap["time"], r, g, b, a);
						readCurve(colorTimeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines[timelines.length] = colorTimeline;
					duration = Math.max(duration, colorTimeline.frames[colorTimeline.frameCount * 5 - 5]);
					
				} else if (timelineName == "attachment") {
					var attachmentTimeline:AttachmentTimeline = new AttachmentTimeline(values.length);
					attachmentTimeline.slotIndex = slotIndex;
					
					frameIndex = 0;
					for each (valueMap in values)
						attachmentTimeline.setFrame(frameIndex++, valueMap["time"], valueMap["name"]);
					timelines[timelines.length] = attachmentTimeline;
					duration = Math.max(duration, attachmentTimeline.frames[attachmentTimeline.frameCount - 1]);

				} else
					throw new Error("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
			}
		}

		var bones:Object = map["bones"];
		for (var boneName:String in bones) {
			var boneIndex:int = skeletonData.findBoneIndex(boneName);
			if (boneIndex == -1) throw new Error("Bone not found: " + boneName);
			var boneMap:Object = bones[boneName];

			for (timelineName in boneMap) {
				values = boneMap[timelineName];
				if (timelineName == "rotate") {
					var rotateTimeline:RotateTimeline = new RotateTimeline(values.length);
					rotateTimeline.boneIndex = boneIndex;

					frameIndex = 0;
					for each (valueMap in values) {
						rotateTimeline.setFrame(frameIndex, valueMap["time"], valueMap["angle"]);
						readCurve(rotateTimeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines[timelines.length] = rotateTimeline;
					duration = Math.max(duration, rotateTimeline.frames[rotateTimeline.frameCount * 2 - 2]);

				} else if (timelineName == "translate" || timelineName == "scale") {
					var timeline:TranslateTimeline;
					var timelineScale:Number = 1;
					if (timelineName == "scale")
						timeline = new ScaleTimeline(values.length);
					else {
						timeline = new TranslateTimeline(values.length);
						timelineScale = scale;
					}
					timeline.boneIndex = boneIndex;

					frameIndex = 0;
					for each (valueMap in values) {
						var x:Number = (valueMap["x"] || 0) * timelineScale;
						var y:Number = (valueMap["y"] || 0) * timelineScale;
						timeline.setFrame(frameIndex, valueMap["time"], x, y);
						readCurve(timeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines[timelines.length] = timeline;
					duration = Math.max(duration, timeline.frames[timeline.frameCount * 3 - 3]);

				} else if (timelineName == "flipX" || timelineName == "flipY") {
					var flipX:Boolean = timelineName == "flipX";
					var flipTimeline:FlipXTimeline = flipX ? new FlipXTimeline(values.length) : new FlipYTimeline(values.length);
					flipTimeline.boneIndex = boneIndex;
					
					var field:String = flipX ? "x" : "y";
					frameIndex = 0;
					for each (valueMap in values) {
						flipTimeline.setFrame(frameIndex, valueMap["time"], valueMap[field] || false);
						frameIndex++;
					}
					timelines[timelines.length] = flipTimeline;
					duration = Math.max(duration, flipTimeline.frames[flipTimeline.frameCount * 3 - 3]);

				} else
					throw new Error("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
			}
		}

		var ikMap:Object = map["ik"];
		for (var ikConstraintName:String in ikMap) {
			var ikConstraint:IkConstraintData = skeletonData.findIkConstraint(ikConstraintName);
			values = ikMap[ikConstraintName];
			var ikTimeline:IkConstraintTimeline = new IkConstraintTimeline(values.length);
			ikTimeline.ikConstraintIndex = skeletonData.ikConstraints.indexOf(ikConstraint);
			frameIndex = 0;
			for each (valueMap in values) {
				var mix:Number = valueMap.hasOwnProperty("mix") ? valueMap["mix"] : 1;
				var bendDirection:int = (!valueMap.hasOwnProperty("bendPositive") || valueMap["bendPositive"]) ? 1 : -1;
				ikTimeline.setFrame(frameIndex, valueMap["time"], mix, bendDirection);
				readCurve(ikTimeline, frameIndex, valueMap);
				frameIndex++;
			}
			timelines[timelines.length] = ikTimeline;
			duration = Math.max(duration, ikTimeline.frames[ikTimeline.frameCount * 3 - 3]);
		}

		var ffd:Object = map["ffd"];
		for (var skinName:String in ffd) {
			var skin:Skin = skeletonData.findSkin(skinName);
			slotMap = ffd[skinName];
			for (slotName in slotMap) {
				slotIndex = skeletonData.findSlotIndex(slotName);
				var meshMap:Object = slotMap[slotName];
				for (var meshName:String in meshMap) {
					values = meshMap[meshName];
					var ffdTimeline:FfdTimeline = new FfdTimeline(values.length);
					var attachment:Attachment = skin.getAttachment(slotIndex, meshName);
					if (!attachment) throw new Error("FFD attachment not found: " + meshName);
					ffdTimeline.slotIndex = slotIndex;
					ffdTimeline.attachment = attachment;

					var vertexCount:int;
					if (attachment is MeshAttachment)
						vertexCount = (attachment as MeshAttachment).vertices.length;
					else
						vertexCount = (attachment as SkinnedMeshAttachment).weights.length / 3 * 2;

					frameIndex = 0;
					for each (valueMap in values) {
						var vertices:Vector.<Number>;
						if (!valueMap["vertices"]) {
							if (attachment is MeshAttachment)
								vertices = (attachment as MeshAttachment).vertices;
							else
								vertices = new Vector.<Number>(vertexCount, true);
						} else {
							var verticesValue:Array = valueMap["vertices"];
							vertices = new Vector.<Number>(vertexCount, true);
							var start:int = valueMap["offset"] || 0;
							var n:int = verticesValue.length;
							if (scale == 1) {
								for (i = 0; i < n; i++)
									vertices[i + start] = verticesValue[i];
							} else {
								for (i = 0; i < n; i++)
									vertices[i + start] = verticesValue[i] * scale;
							}
							if (attachment is MeshAttachment) {
								var meshVertices:Vector.<Number> = (attachment as MeshAttachment).vertices;
								for (i = 0; i < vertexCount; i++)
									vertices[i] += meshVertices[i];
							}
						}
						
						ffdTimeline.setFrame(frameIndex, valueMap["time"], vertices);
						readCurve(ffdTimeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines[timelines.length] = ffdTimeline;
					duration = Math.max(duration, ffdTimeline.frames[ffdTimeline.frameCount - 1]);
				}
			}
		}

		var drawOrderValues:Object = map["drawOrder"];
		if (!drawOrderValues) drawOrderValues = map["draworder"];
		if (drawOrderValues) {
			var drawOrderTimeline:DrawOrderTimeline = new DrawOrderTimeline(drawOrderValues.length);
			var slotCount:int = skeletonData.slots.length;
			frameIndex = 0;
			for each (var drawOrderMap:Object in drawOrderValues) {
				var drawOrder:Vector.<int> = null;
				if (drawOrderMap["offsets"]) {
					drawOrder = new Vector.<int>(slotCount);
					for (i = slotCount - 1; i >= 0; i--)
						drawOrder[i] = -1;
					var offsets:Object = drawOrderMap["offsets"];
					var unchanged:Vector.<int> = new Vector.<int>(slotCount - offsets.length);
					var originalIndex:int = 0, unchangedIndex:int = 0;
					for each (var offsetMap:Object in offsets) {
						slotIndex = skeletonData.findSlotIndex(offsetMap["slot"]);
						if (slotIndex == -1) throw new Error("Slot not found: " + offsetMap["slot"]);
						// Collect unchanged items.
						while (originalIndex != slotIndex)
							unchanged[unchangedIndex++] = originalIndex++;
						// Set changed items.
						drawOrder[originalIndex + offsetMap["offset"]] = originalIndex++;
					}
					// Collect remaining unchanged items.
					while (originalIndex < slotCount)
						unchanged[unchangedIndex++] = originalIndex++;
					// Fill in unchanged items.
					for (i = slotCount - 1; i >= 0; i--)
						if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
				}
				drawOrderTimeline.setFrame(frameIndex++, drawOrderMap["time"], drawOrder);
			}
			timelines[timelines.length] = drawOrderTimeline;
			duration = Math.max(duration, drawOrderTimeline.frames[drawOrderTimeline.frameCount - 1]);
		}

		var eventsMap:Object = map["events"];
		if (eventsMap) {
			var eventTimeline:EventTimeline = new EventTimeline(eventsMap.length);
			frameIndex = 0;
			for each (var eventMap:Object in eventsMap) {
				var eventData:EventData = skeletonData.findEvent(eventMap["name"]);
				if (!eventData) throw new Error("Event not found: " + eventMap["name"]);
				var event:Event = new Event(eventData);
				event.intValue = eventMap.hasOwnProperty("int") ? eventMap["int"] : eventData.intValue;
				event.floatValue = eventMap.hasOwnProperty("float") ? eventMap["float"] : eventData.floatValue;
				event.stringValue = eventMap.hasOwnProperty("string") ? eventMap["string"] : eventData.stringValue;
				eventTimeline.setFrame(frameIndex++, eventMap["time"], event);
			}
			timelines[timelines.length] = eventTimeline;
			duration = Math.max(duration, eventTimeline.frames[eventTimeline.frameCount - 1]);
		}

		skeletonData.animations[skeletonData.animations.length] = new Animation(name, timelines, duration);
	}

	static private function readCurve (timeline:CurveTimeline, frameIndex:int, valueMap:Object) : void {
		var curve:Object = valueMap["curve"];
		if (!curve) return;
		if (curve == "stepped")
			timeline.setStepped(frameIndex);
		else if (curve is Array)
			timeline.setCurve(frameIndex, curve[0], curve[1], curve[2], curve[3]);
	}

	static private function toColor (hexString:String, colorIndex:int) : Number {
		if (hexString.length != 8) throw new ArgumentError("Color hexidecimal length must be 8, recieved: " + hexString);
		return parseInt(hexString.substring(colorIndex * 2, colorIndex * 2 + 2), 16) / 255;
	}

	static private function getFloatArray (map:Object, name:String, scale:Number) : Vector.<Number> {
		var list:Array = map[name];
		var values:Vector.<Number> = new Vector.<Number>(list.length, true);
		var i:int = 0, n:int = list.length;
		if (scale == 1) {
			for (; i < n; i++)
				values[i] = list[i];
		} else {
			for (; i < n; i++)
				values[i] = list[i] * scale;
		}
		return values;
	}
	
	static private function getIntArray (map:Object, name:String) : Vector.<int> {
		var list:Array = map[name];
		var values:Vector.<int> = new Vector.<int>(list.length, true);
		for (var i:int = 0, n:int = list.length; i < n; i++)
			values[i] = int(list[i]);
		return values;
	}
	
	static private function getUintArray (map:Object, name:String) : Vector.<uint> {
		var list:Array = map[name];
		var values:Vector.<uint> = new Vector.<uint>(list.length, true);
		for (var i:int = 0, n:int = list.length; i < n; i++)
			values[i] = int(list[i]);
		return values;
	}
}

}
