/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License, Professional License, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine {
import flash.utils.ByteArray;

import spine.animation.Animation;
import spine.animation.AttachmentTimeline;
import spine.animation.ColorTimeline;
import spine.animation.CurveTimeline;
import spine.animation.RotateTimeline;
import spine.animation.ScaleTimeline;
import spine.animation.Timeline;
import spine.animation.TranslateTimeline;
import spine.attachments.Attachment;
import spine.attachments.AttachmentLoader;
import spine.attachments.AttachmentType;
import spine.attachments.RegionAttachment;

public class SkeletonJson {
	static public const TIMELINE_SCALE:String = "scale";
	static public const TIMELINE_ROTATE:String = "rotate";
	static public const TIMELINE_TRANSLATE:String = "translate";
	static public const TIMELINE_ATTACHMENT:String = "attachment";
	static public const TIMELINE_COLOR:String = "color";

	public var attachmentLoader:AttachmentLoader;
	public var scale:Number = 1;

	public function SkeletonJson (attachmentLoader:AttachmentLoader = null) {
		this.attachmentLoader = attachmentLoader;
	}

	/** @param object A String or ByteArray. */
	public function readSkeletonData (object:*, name:String = null) : SkeletonData {
		if (object == null)
			throw new ArgumentError("object cannot be null.");

		var json:String;
		if (object is String)
			json = String(object);
		else if (object is ByteArray)
			json = object.readUTFBytes(object.length);
		else
			throw new ArgumentError("object must be a String or ByteArray.");

		var skeletonData:SkeletonData = new SkeletonData();
		skeletonData.name = name;

		var root:Object = JSON.parse(json);

		// Bones.
		var boneData:BoneData;
		for each (var boneMap:Object in root["bones"]) {
			var parent:BoneData = null;
			var parentName:String = boneMap["parent"];
			if (parentName) {
				parent = skeletonData.findBone(parentName);
				if (!parent)
					throw new Error("Parent bone not found: " + parentName);
			}
			boneData = new BoneData(boneMap["name"], parent);
			boneData.length = (boneMap["length"] || 0) * scale;
			boneData.x = (boneMap["x"] || 0) * scale;
			boneData.y = (boneMap["y"] || 0) * scale;
			boneData.rotation = (boneMap["rotation"] || 0);
			boneData.scaleX = boneMap["scaleX"] || 1;
			boneData.scaleY = boneMap["scaleY"] || 1;
			boneData.inheritScale = boneMap["inheritScale"] || true;
			boneData.inheritRotation = boneMap["inheritRotation"] || true;
			skeletonData.addBone(boneData);
		}

		// Slots.
		for each (var slotMap:Object in root["slots"]) {
			var boneName:String = slotMap["bone"];
			boneData = skeletonData.findBone(boneName);
			if (!boneData)
				throw new Error("Slot bone not found: " + boneName);
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

			skeletonData.addSlot(slotData);
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
			skeletonData.addSkin(skin);
			if (skin.name == "default")
				skeletonData.defaultSkin = skin;
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
		var attachment:Attachment = attachmentLoader.newAttachment(skin, type, name);

		if (attachment is RegionAttachment) {
			var regionAttachment:RegionAttachment = attachment as RegionAttachment;
			regionAttachment.x = (map["x"] || 0) * scale;
			regionAttachment.y = (map["y"] || 0) * scale;
			regionAttachment.scaleX = map["scaleX"] || 1;
			regionAttachment.scaleY = map["scaleY"] || 1;
			regionAttachment.rotation = map["rotation"] || 0;
			regionAttachment.width = (map["width"] || 32) * scale;
			regionAttachment.height = (map["height"] || 32) * scale;
			regionAttachment.updateOffset();
		}

		return attachment;
	}

	private function readAnimation (name:String, map:Object, skeletonData:SkeletonData) : void {
		var timelines:Vector.<Timeline> = new Vector.<Timeline>();
		var duration:Number = 0;

		var bones:Object = map["bones"];
		for (var boneName:String in bones) {
			var boneIndex:int = skeletonData.findBoneIndex(boneName);
			if (boneIndex == -1)
				throw new Error("Bone not found: " + boneName);
			var boneMap:Object = bones[boneName];

			for (var timelineName:Object in boneMap) {
				var values:Array = boneMap[timelineName];
				if (timelineName == TIMELINE_ROTATE) {
					var timeline:RotateTimeline = new RotateTimeline(values.length);
					timeline.boneIndex = boneIndex;

					var frameIndex:int = 0;
					for each (var valueMap:Object in values) {
						timeline.setFrame(frameIndex, valueMap["time"], valueMap["angle"]);
						readCurve(timeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines.push(timeline);
					duration = Math.max(duration, timeline.frames[timeline.frameCount * 2 - 2]);

				} else if (timelineName == TIMELINE_TRANSLATE || timelineName == TIMELINE_SCALE) {
					var timeline1:TranslateTimeline;
					var timelineScale:Number = 1;
					if (timelineName == TIMELINE_SCALE)
						timeline1 = new ScaleTimeline(values.length);
					else {
						timeline1 = new TranslateTimeline(values.length);
						timelineScale = scale;
					}
					timeline1.boneIndex = boneIndex;

					var frameIndex1:int = 0;
					for each (var valueMap1:Object in values) {
						var x:Number = (valueMap1["x"] || 0) * timelineScale;
						var y:Number = (valueMap1["y"] || 0) * timelineScale;
						timeline1.setFrame(frameIndex1, valueMap1["time"], x, y);
						readCurve(timeline1, frameIndex1, valueMap1);
						frameIndex1++;
					}
					timelines.push(timeline1);
					duration = Math.max(duration, timeline1.frames[timeline1.frameCount * 3 - 3]);

				} else
					throw new Error("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
			}
		}

		var slots:Object = map["slots"];
		for (var slotName:String in slots) {
			var slotMap:Object = slots[slotName];
			var slotIndex:int = skeletonData.findSlotIndex(slotName);

			for (var timelineName2:Object in slotMap) {
				var values2:Object = slotMap[timelineName2];
				if (timelineName2 == TIMELINE_COLOR) {
					var timeline2:ColorTimeline = new ColorTimeline(values2.length);
					timeline2.slotIndex = slotIndex;

					var frameIndex2:int = 0;
					for each (var valueMap2:Object in values2) {
						var color:String = valueMap2["color"];
						var r:Number = toColor(color, 0);
						var g:Number = toColor(color, 1);
						var b:Number = toColor(color, 2);
						var a:Number = toColor(color, 3);
						timeline2.setFrame(frameIndex2, valueMap2["time"], r, g, b, a);
						readCurve(timeline2, frameIndex2, valueMap2);
						frameIndex2++;
					}
					timelines.push(timeline2);
					duration = Math.max(duration, timeline2.frames[timeline2.frameCount * 5 - 5]);

				} else if (timelineName2 == TIMELINE_ATTACHMENT) {
					var timeline3:AttachmentTimeline = new AttachmentTimeline(values2.length);
					timeline3.slotIndex = slotIndex;

					var frameIndex3:int = 0;
					for each (var valueMap3:Object in values2) {
						timeline3.setFrame(frameIndex3++, valueMap3["time"], valueMap3["name"]);
					}
					timelines.push(timeline3);
					duration = Math.max(duration, timeline3.frames[timeline3.frameCount - 1]);

				} else
					throw new Error("Invalid timeline type for a slot: " + timelineName2 + " (" + slotName + ")");
			}
		}

		skeletonData.addAnimation(new Animation(name, timelines, duration));
	}

	private function readCurve (timeline:CurveTimeline, frameIndex:int, valueMap:Object) : void {
		var curve:Object = valueMap["curve"];
		if (curve == null)
			return;
		if (curve == "stepped")
			timeline.setStepped(frameIndex);
		else if (curve is Array) {
			timeline.setCurve(frameIndex, curve[0], curve[1], curve[2], curve[3]);
		}
	}

	static private function toColor (hexString:String, colorIndex:int) : Number {
		if (hexString.length != 8)
			throw new ArgumentError("Color hexidecimal length must be 8, recieved: " + hexString);
		return parseInt(hexString.substring(colorIndex * 2, colorIndex * 2 + 2), 16) / 255;
	}
}

}
