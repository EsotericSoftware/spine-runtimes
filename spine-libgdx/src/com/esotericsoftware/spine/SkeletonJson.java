/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
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

package com.esotericsoftware.spine;

// BOZO - Event values don't seem to key properly.

import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.ColorTimeline;
import com.esotericsoftware.spine.Animation.CurveTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderTimeline;
import com.esotericsoftware.spine.Animation.EventTimeline;
import com.esotericsoftware.spine.Animation.RotateTimeline;
import com.esotericsoftware.spine.Animation.ScaleTimeline;
import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.Animation.TranslateTimeline;
import com.esotericsoftware.spine.attachments.AtlasAttachmentLoader;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.AttachmentLoader;
import com.esotericsoftware.spine.attachments.AttachmentType;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.RegionSequenceAttachment;
import com.esotericsoftware.spine.attachments.RegionSequenceAttachment.Mode;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.JsonReader;
import com.badlogic.gdx.utils.JsonValue;
import com.badlogic.gdx.utils.SerializationException;

public class SkeletonJson {
	static public final String TIMELINE_SCALE = "scale";
	static public final String TIMELINE_ROTATE = "rotate";
	static public final String TIMELINE_TRANSLATE = "translate";
	static public final String TIMELINE_ATTACHMENT = "attachment";
	static public final String TIMELINE_COLOR = "color";

	private final AttachmentLoader attachmentLoader;
	private float scale = 1;

	public SkeletonJson (TextureAtlas atlas) {
		attachmentLoader = new AtlasAttachmentLoader(atlas);
	}

	public SkeletonJson (AttachmentLoader attachmentLoader) {
		this.attachmentLoader = attachmentLoader;
	}

	public float getScale () {
		return scale;
	}

	/** Scales the bones, images, and animations as they are loaded. */
	public void setScale (float scale) {
		this.scale = scale;
	}

	public SkeletonData readSkeletonData (FileHandle file) {
		if (file == null) throw new IllegalArgumentException("file cannot be null.");

		SkeletonData skeletonData = new SkeletonData();
		skeletonData.name = file.nameWithoutExtension();

		JsonValue root = new JsonReader().parse(file);

		// Bones.
		for (JsonValue boneMap = root.getChild("bones"); boneMap != null; boneMap = boneMap.next()) {
			BoneData parent = null;
			String parentName = boneMap.getString("parent", null);
			if (parentName != null) {
				parent = skeletonData.findBone(parentName);
				if (parent == null) throw new SerializationException("Parent bone not found: " + parentName);
			}
			BoneData boneData = new BoneData(boneMap.getString("name"), parent);
			boneData.length = boneMap.getFloat("length", 0) * scale;
			boneData.x = boneMap.getFloat("x", 0) * scale;
			boneData.y = boneMap.getFloat("y", 0) * scale;
			boneData.rotation = boneMap.getFloat("rotation", 0);
			boneData.scaleX = boneMap.getFloat("scaleX", 1);
			boneData.scaleY = boneMap.getFloat("scaleY", 1);
			boneData.inheritScale = boneMap.getBoolean("inheritScale", true);
			boneData.inheritRotation = boneMap.getBoolean("inheritRotation", true);
			skeletonData.addBone(boneData);
		}

		// Slots.
		for (JsonValue slotMap = root.getChild("slots"); slotMap != null; slotMap = slotMap.next()) {
			String slotName = slotMap.getString("name");
			String boneName = slotMap.getString("bone");
			BoneData boneData = skeletonData.findBone(boneName);
			if (boneData == null) throw new SerializationException("Slot bone not found: " + boneName);
			SlotData slotData = new SlotData(slotName, boneData);

			String color = slotMap.getString("color", null);
			if (color != null) slotData.getColor().set(Color.valueOf(color));

			slotData.attachmentName = slotMap.getString("attachment", null);

			slotData.additiveBlending = slotMap.getBoolean("additive", false);

			skeletonData.addSlot(slotData);
		}

		// Skins.
		for (JsonValue skinMap = root.getChild("skins"); skinMap != null; skinMap = skinMap.next()) {
			Skin skin = new Skin(skinMap.name());
			for (JsonValue slotEntry = skinMap.child(); slotEntry != null; slotEntry = slotEntry.next()) {
				int slotIndex = skeletonData.findSlotIndex(slotEntry.name());
				if (slotIndex == -1) throw new SerializationException("Slot not found: " + slotEntry.name());
				for (JsonValue entry = slotEntry.child(); entry != null; entry = entry.next()) {
					Attachment attachment = readAttachment(skin, entry.name(), entry);
					if (attachment != null) skin.addAttachment(slotIndex, entry.name(), attachment);
				}
			}
			skeletonData.addSkin(skin);
			if (skin.name.equals("default")) skeletonData.defaultSkin = skin;
		}

		// Events.
		for (JsonValue eventMap = root.getChild("events"); eventMap != null; eventMap = eventMap.next()) {
			EventData eventData = new EventData(eventMap.name());
			eventData.intValue = eventMap.getInt("int", 0);
			eventData.floatValue = eventMap.getFloat("float", 0f);
			eventData.stringValue = eventMap.getString("string", null);
			skeletonData.addEvent(eventData);
		}

		// Animations.
		for (JsonValue animationMap = root.getChild("animations"); animationMap != null; animationMap = animationMap.next())
			readAnimation(animationMap.name(), animationMap, skeletonData);

		skeletonData.bones.shrink();
		skeletonData.slots.shrink();
		skeletonData.skins.shrink();
		skeletonData.animations.shrink();
		return skeletonData;
	}

	private Attachment readAttachment (Skin skin, String name, JsonValue map) {
		name = map.getString("name", name);

		AttachmentType type = AttachmentType.valueOf(map.getString("type", AttachmentType.region.name()));
		Attachment attachment = attachmentLoader.newAttachment(skin, type, name);

		if (attachment instanceof RegionSequenceAttachment) {
			RegionSequenceAttachment regionSequenceAttachment = (RegionSequenceAttachment)attachment;

			float fps = map.getFloat("fps");
			regionSequenceAttachment.setFrameTime(fps);

			String modeString = map.getString("mode");
			regionSequenceAttachment.setMode(modeString == null ? Mode.forward : Mode.valueOf(modeString));

		} else if (attachment instanceof RegionAttachment) {
			RegionAttachment regionAttachment = (RegionAttachment)attachment;
			regionAttachment.setX(map.getFloat("x", 0) * scale);
			regionAttachment.setY(map.getFloat("y", 0) * scale);
			regionAttachment.setScaleX(map.getFloat("scaleX", 1));
			regionAttachment.setScaleY(map.getFloat("scaleY", 1));
			regionAttachment.setRotation(map.getFloat("rotation", 0));
			regionAttachment.setWidth(map.getFloat("width", 32) * scale);
			regionAttachment.setHeight(map.getFloat("height", 32) * scale);
			regionAttachment.updateOffset();

		} else if (attachment instanceof BoundingBoxAttachment) {
			BoundingBoxAttachment box = (BoundingBoxAttachment)attachment;
			JsonValue pointsArray = map.require("vertices");
			float[] points = new float[pointsArray.size];
			int i = 0;
			for (JsonValue point = pointsArray.child; point != null; point = point.next())
				points[i++] = point.asFloat();
			box.setVertices(points);
		}

		return attachment;
	}

	private void readAnimation (String name, JsonValue map, SkeletonData skeletonData) {
		Array<Timeline> timelines = new Array();
		float duration = 0;

		for (JsonValue boneMap = map.getChild("bones"); boneMap != null; boneMap = boneMap.next()) {
			int boneIndex = skeletonData.findBoneIndex(boneMap.name());
			if (boneIndex == -1) throw new SerializationException("Bone not found: " + boneMap.name());

			for (JsonValue timelineMap = boneMap.child(); timelineMap != null; timelineMap = timelineMap.next()) {
				String timelineName = timelineMap.name();
				if (timelineName.equals(TIMELINE_ROTATE)) {
					RotateTimeline timeline = new RotateTimeline(timelineMap.size);
					timeline.boneIndex = boneIndex;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child(); valueMap != null; valueMap = valueMap.next()) {
						float time = valueMap.getFloat("time");
						timeline.setFrame(frameIndex, time, valueMap.getFloat("angle"));
						readCurve(timeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() * 2 - 2]);

				} else if (timelineName.equals(TIMELINE_TRANSLATE) || timelineName.equals(TIMELINE_SCALE)) {
					TranslateTimeline timeline;
					float timelineScale = 1;
					if (timelineName.equals(TIMELINE_SCALE))
						timeline = new ScaleTimeline(timelineMap.size);
					else {
						timeline = new TranslateTimeline(timelineMap.size);
						timelineScale = scale;
					}
					timeline.boneIndex = boneIndex;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child(); valueMap != null; valueMap = valueMap.next()) {
						float time = valueMap.getFloat("time");
						float x = valueMap.getFloat("x", 0), y = valueMap.getFloat("y", 0);
						timeline.setFrame(frameIndex, time, x * timelineScale, y * timelineScale);
						readCurve(timeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() * 3 - 3]);

				} else
					throw new RuntimeException("Invalid timeline type for a bone: " + timelineName + " (" + boneMap.name() + ")");
			}
		}

		for (JsonValue slotMap = map.getChild("slots"); slotMap != null; slotMap = slotMap.next()) {
			int slotIndex = skeletonData.findSlotIndex(slotMap.name());
			if (slotIndex == -1) throw new SerializationException("Slot not found: " + slotMap.name());

			for (JsonValue timelineMap = slotMap.child(); timelineMap != null; timelineMap = timelineMap.next()) {
				String timelineName = timelineMap.name();
				if (timelineName.equals(TIMELINE_COLOR)) {
					ColorTimeline timeline = new ColorTimeline(timelineMap.size);
					timeline.slotIndex = slotIndex;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child(); valueMap != null; valueMap = valueMap.next()) {
						float time = valueMap.getFloat("time");
						Color color = Color.valueOf(valueMap.getString("color"));
						timeline.setFrame(frameIndex, time, color.r, color.g, color.b, color.a);
						readCurve(timeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() * 5 - 5]);

				} else if (timelineName.equals(TIMELINE_ATTACHMENT)) {
					AttachmentTimeline timeline = new AttachmentTimeline(timelineMap.size);
					timeline.slotIndex = slotIndex;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child(); valueMap != null; valueMap = valueMap.next()) {
						float time = valueMap.getFloat("time");
						timeline.setFrame(frameIndex++, time, valueMap.getString("name"));
					}
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() - 1]);

				} else
					throw new RuntimeException("Invalid timeline type for a slot: " + timelineName + " (" + slotMap.name() + ")");
			}
		}

		JsonValue eventsMap = map.get("events");
		if (eventsMap != null) {
			EventTimeline timeline = new EventTimeline(eventsMap.size);
			int frameIndex = 0;
			for (JsonValue eventMap = eventsMap.child; eventMap != null; eventMap = eventMap.next()) {
				EventData eventData = skeletonData.findEvent(eventMap.getString("name"));
				if (eventData == null) throw new SerializationException("Event not found: " + eventMap.getString("name"));
				Event event = new Event(eventData);
				event.intValue = eventMap.getInt("int", eventData.getInt());
				event.floatValue = eventMap.getFloat("float", eventData.getFloat());
				event.stringValue = eventMap.getString("string", eventData.getString());
				timeline.setFrame(frameIndex++, eventMap.getFloat("time"), event);
			}
			timelines.add(timeline);
			duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() - 1]);
		}

		JsonValue drawOrdersMap = map.get("draworder");
		if (drawOrdersMap != null) {
			DrawOrderTimeline timeline = new DrawOrderTimeline(drawOrdersMap.size);
			int slotCount = skeletonData.slots.size;
			int frameIndex = 0;
			for (JsonValue drawOrderMap = drawOrdersMap.child; drawOrderMap != null; drawOrderMap = drawOrderMap.next()) {
				int[] drawOrder = new int[slotCount];
				for (int i = slotCount - 1; i >= 0; i--)
					drawOrder[i] = -1;
				int[] unchanged = new int[slotCount - drawOrderMap.get("offsets").size];
				int originalIndex = 0, unchangedIndex = 0;
				for (JsonValue offsetMap = drawOrderMap.getChild("offsets"); offsetMap != null; offsetMap = offsetMap.next()) {
					int slotIndex = skeletonData.findSlotIndex(offsetMap.getString("slot"));
					if (slotIndex == -1) throw new SerializationException("Slot not found: " + offsetMap.getString("slot"));
					// Collect unchanged items.
					while (originalIndex != slotIndex)
						unchanged[unchangedIndex++] = originalIndex++;
					// Set changed items.
					drawOrder[originalIndex + offsetMap.getInt("offset")] = originalIndex++;
				}
				// Collect remaining unchanged items.
				while (originalIndex < slotCount)
					unchanged[unchangedIndex++] = originalIndex++;
				// Fill in unchanged items.
				for (int i = slotCount - 1; i >= 0; i--)
					if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
				timeline.setFrame(frameIndex++, drawOrderMap.getFloat("time"), drawOrder);
			}
			timelines.add(timeline);
			duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() - 1]);
		}

		timelines.shrink();
		skeletonData.addAnimation(new Animation(name, timelines, duration));
	}

	void readCurve (CurveTimeline timeline, int frameIndex, JsonValue valueMap) {
		JsonValue curve = valueMap.get("curve");
		if (curve == null) return;
		if (curve.isString() && curve.asString().equals("stepped"))
			timeline.setStepped(frameIndex);
		else if (curve.isArray()) {
			timeline.setCurve(frameIndex, curve.getFloat(0), curve.getFloat(1), curve.getFloat(2), curve.getFloat(3));
		}
	}
}
