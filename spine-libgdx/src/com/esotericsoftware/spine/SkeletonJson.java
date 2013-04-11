/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
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
 ******************************************************************************/

package com.esotericsoftware.spine;

import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.ColorTimeline;
import com.esotericsoftware.spine.Animation.CurveTimeline;
import com.esotericsoftware.spine.Animation.RotateTimeline;
import com.esotericsoftware.spine.Animation.ScaleTimeline;
import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.Animation.TranslateTimeline;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.RegionSequenceAttachment;
import com.esotericsoftware.spine.attachments.RegionSequenceAttachment.Mode;
import com.esotericsoftware.spine.attachments.AtlasAttachmentLoader;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Json;
import com.badlogic.gdx.utils.ObjectMap.Entry;
import com.badlogic.gdx.utils.OrderedMap;
import com.badlogic.gdx.utils.SerializationException;

import java.io.StringWriter;

public class SkeletonJson {
	static public final String TIMELINE_SCALE = "scale";
	static public final String TIMELINE_ROTATE = "rotate";
	static public final String TIMELINE_TRANSLATE = "translate";
	static public final String TIMELINE_ATTACHMENT = "attachment";
	static public final String TIMELINE_COLOR = "color";

	private final Json json = new Json();
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
		skeletonData.setName(file.nameWithoutExtension());

		OrderedMap<String, ?> root = json.fromJson(OrderedMap.class, file);

		// Bones.
		for (OrderedMap boneMap : (Array<OrderedMap>)root.get("bones")) {
			BoneData parent = null;
			String parentName = (String)boneMap.get("parent");
			if (parentName != null) {
				parent = skeletonData.findBone(parentName);
				if (parent == null) throw new SerializationException("Parent bone not found: " + parentName);
			}
			BoneData boneData = new BoneData((String)boneMap.get("name"), parent);
			boneData.length = getFloat(boneMap, "length", 0) * scale;
			boneData.x = getFloat(boneMap, "x", 0) * scale;
			boneData.y = getFloat(boneMap, "y", 0) * scale;
			boneData.rotation = getFloat(boneMap, "rotation", 0);
			boneData.scaleX = getFloat(boneMap, "scaleX", 1);
			boneData.scaleY = getFloat(boneMap, "scaleY", 1);
			skeletonData.addBone(boneData);
		}

		// Slots.
		Array<OrderedMap> slots = (Array<OrderedMap>)root.get("slots");
		if (slots != null) {
			for (OrderedMap slotMap : slots) {
				String slotName = (String)slotMap.get("name");
				String boneName = (String)slotMap.get("bone");
				BoneData boneData = skeletonData.findBone(boneName);
				if (boneData == null) throw new SerializationException("Slot bone not found: " + boneName);
				SlotData slotData = new SlotData(slotName, boneData);

				String color = (String)slotMap.get("color");
				if (color != null) slotData.getColor().set(Color.valueOf(color));

				slotData.setAttachmentName((String)slotMap.get("attachment"));

				skeletonData.addSlot(slotData);
			}
		}

		// Skins.
		OrderedMap<String, OrderedMap> skinsMap = (OrderedMap)root.get("skins");
		if (skinsMap != null) {
			for (Entry<String, OrderedMap> entry : skinsMap.entries()) {
				Skin skin = new Skin(entry.key);
				for (Entry<String, OrderedMap> slotEntry : ((OrderedMap<String, OrderedMap>)entry.value).entries()) {
					int slotIndex = skeletonData.findSlotIndex(slotEntry.key);
					for (Entry<String, OrderedMap> attachmentEntry : ((OrderedMap<String, OrderedMap>)slotEntry.value).entries()) {
						Attachment attachment = readAttachment(skin, attachmentEntry.key, attachmentEntry.value);
						if (attachment != null) skin.addAttachment(slotIndex, attachmentEntry.key, attachment);
					}
				}
				skeletonData.addSkin(skin);
				if (skin.name.equals("default")) skeletonData.setDefaultSkin(skin);
			}
		}

		// Animations.
		OrderedMap<String, OrderedMap> animationMap = (OrderedMap)root.get("animations");
		if (animationMap != null) {
			for (Entry<String, OrderedMap> entry : animationMap.entries())
				readAnimation(entry.key, entry.value, skeletonData);
		}

		skeletonData.bones.shrink();
		skeletonData.slots.shrink();
		skeletonData.skins.shrink();
		skeletonData.animations.shrink();
		return skeletonData;
	}

	private Attachment readAttachment (Skin skin, String name, OrderedMap map) {
		name = (String)map.get("name", name);

		AttachmentType type = AttachmentType.valueOf((String)map.get("type", AttachmentType.region.name()));
		Attachment attachment = attachmentLoader.newAttachment(skin, type, name);

		if (attachment instanceof RegionSequenceAttachment) {
			RegionSequenceAttachment regionSequenceAttachment = (RegionSequenceAttachment)attachment;

			Float fps = (Float)map.get("fps");
			if (fps == null) throw new SerializationException("Region sequence attachment missing fps: " + name);
			regionSequenceAttachment.setFrameTime(fps);

			String modeString = (String)map.get("mode");
			regionSequenceAttachment.setMode(modeString == null ? Mode.forward : Mode.valueOf(modeString));
		}

		if (attachment instanceof RegionAttachment) {
			RegionAttachment regionAttachment = (RegionAttachment)attachment;
			regionAttachment.setX(getFloat(map, "x", 0) * scale);
			regionAttachment.setY(getFloat(map, "y", 0) * scale);
			regionAttachment.setScaleX(getFloat(map, "scaleX", 1));
			regionAttachment.setScaleY(getFloat(map, "scaleY", 1));
			regionAttachment.setRotation(getFloat(map, "rotation", 0));
			regionAttachment.setWidth(getFloat(map, "width", 32) * scale);
			regionAttachment.setHeight(getFloat(map, "height", 32) * scale);
			regionAttachment.updateOffset();
		}

		return attachment;
	}

	private float getFloat (OrderedMap map, String name, float defaultValue) {
		Object value = map.get(name);
		if (value == null) return defaultValue;
		return (Float)value;
	}

	private void readAnimation (String name, OrderedMap<String, ?> map, SkeletonData skeletonData) {
		Array<Timeline> timelines = new Array();
		float duration = 0;

		OrderedMap<String, ?> bonesMap = (OrderedMap)map.get("bones");
		if (bonesMap != null) {
			for (Entry<String, ?> entry : bonesMap.entries()) {
				String boneName = entry.key;
				int boneIndex = skeletonData.findBoneIndex(boneName);
				if (boneIndex == -1) throw new SerializationException("Bone not found: " + boneName);

				OrderedMap<?, ?> timelineMap = (OrderedMap)entry.value;
				for (Entry timelineEntry : timelineMap.entries()) {
					Array<OrderedMap> values = (Array)timelineEntry.value;
					String timelineName = (String)timelineEntry.key;
					if (timelineName.equals(TIMELINE_ROTATE)) {
						RotateTimeline timeline = new RotateTimeline(values.size);
						timeline.setBoneIndex(boneIndex);

						int frameIndex = 0;
						for (OrderedMap valueMap : values) {
							float time = (Float)valueMap.get("time");
							timeline.setFrame(frameIndex, time, (Float)valueMap.get("angle"));
							readCurve(timeline, frameIndex, valueMap);
							frameIndex++;
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() * 2 - 2]);

					} else if (timelineName.equals(TIMELINE_TRANSLATE) || timelineName.equals(TIMELINE_SCALE)) {
						TranslateTimeline timeline;
						float timelineScale = 1;
						if (timelineName.equals(TIMELINE_SCALE))
							timeline = new ScaleTimeline(values.size);
						else {
							timeline = new TranslateTimeline(values.size);
							timelineScale = scale;
						}
						timeline.setBoneIndex(boneIndex);

						int frameIndex = 0;
						for (OrderedMap valueMap : values) {
							float time = (Float)valueMap.get("time");
							Float x = (Float)valueMap.get("x"), y = (Float)valueMap.get("y");
							timeline
								.setFrame(frameIndex, time, x == null ? 0 : (x * timelineScale), y == null ? 0 : (y * timelineScale));
							readCurve(timeline, frameIndex, valueMap);
							frameIndex++;
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() * 3 - 3]);

					} else
						throw new RuntimeException("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
				}
			}
		}

		OrderedMap<String, ?> slotsMap = (OrderedMap)map.get("slots");
		if (slotsMap != null) {
			for (Entry<String, ?> entry : slotsMap.entries()) {
				String slotName = entry.key;
				int slotIndex = skeletonData.findSlotIndex(slotName);
				OrderedMap<?, ?> timelineMap = (OrderedMap)entry.value;

				for (Entry timelineEntry : timelineMap.entries()) {
					Array<OrderedMap> values = (Array)timelineEntry.value;
					String timelineName = (String)timelineEntry.key;
					if (timelineName.equals(TIMELINE_COLOR)) {
						ColorTimeline timeline = new ColorTimeline(values.size);
						timeline.setSlotIndex(slotIndex);

						int frameIndex = 0;
						for (OrderedMap valueMap : values) {
							float time = (Float)valueMap.get("time");
							Color color = Color.valueOf((String)valueMap.get("color"));
							timeline.setFrame(frameIndex, time, color.r, color.g, color.b, color.a);
							readCurve(timeline, frameIndex, valueMap);
							frameIndex++;
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() * 5 - 5]);

					} else if (timelineName.equals(TIMELINE_ATTACHMENT)) {
						AttachmentTimeline timeline = new AttachmentTimeline(values.size);
						timeline.setSlotIndex(slotIndex);

						int frameIndex = 0;
						for (OrderedMap valueMap : values) {
							float time = (Float)valueMap.get("time");
							timeline.setFrame(frameIndex++, time, (String)valueMap.get("name"));
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() - 1]);

					} else
						throw new RuntimeException("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
				}
			}
		}

		timelines.shrink();
		skeletonData.addAnimation(new Animation(name, timelines, duration));
	}

	private void readCurve (CurveTimeline timeline, int frameIndex, OrderedMap valueMap) {
		Object curveObject = valueMap.get("curve");
		if (curveObject == null) return;
		if (curveObject.equals("stepped"))
			timeline.setStepped(frameIndex);
		else if (curveObject instanceof Array) {
			Array curve = (Array)curveObject;
			timeline.setCurve(frameIndex, (Float)curve.get(0), (Float)curve.get(1), (Float)curve.get(2), (Float)curve.get(3));
		}
	}
}
