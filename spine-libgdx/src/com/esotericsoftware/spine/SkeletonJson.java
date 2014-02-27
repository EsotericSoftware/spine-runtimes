/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.ColorTimeline;
import com.esotericsoftware.spine.Animation.CurveTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderTimeline;
import com.esotericsoftware.spine.Animation.EventTimeline;
import com.esotericsoftware.spine.Animation.FfdTimeline;
import com.esotericsoftware.spine.Animation.RotateTimeline;
import com.esotericsoftware.spine.Animation.ScaleTimeline;
import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.Animation.TranslateTimeline;
import com.esotericsoftware.spine.attachments.AtlasAttachmentLoader;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.AttachmentLoader;
import com.esotericsoftware.spine.attachments.AttachmentType;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.JsonReader;
import com.badlogic.gdx.utils.JsonValue;
import com.badlogic.gdx.utils.SerializationException;

public class SkeletonJson {
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

		float scale = this.scale;

		SkeletonData skeletonData = new SkeletonData();
		skeletonData.name = file.nameWithoutExtension();

		JsonValue root = new JsonReader().parse(file);

		// Bones.
		for (JsonValue boneMap = root.getChild("bones"); boneMap != null; boneMap = boneMap.next) {
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

			String color = boneMap.getString("color", null);
			if (color != null) boneData.getColor().set(Color.valueOf(color));

			skeletonData.addBone(boneData);
		}

		// Slots.
		for (JsonValue slotMap = root.getChild("slots"); slotMap != null; slotMap = slotMap.next) {
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
		for (JsonValue skinMap = root.getChild("skins"); skinMap != null; skinMap = skinMap.next) {
			Skin skin = new Skin(skinMap.name);
			for (JsonValue slotEntry = skinMap.child; slotEntry != null; slotEntry = slotEntry.next) {
				int slotIndex = skeletonData.findSlotIndex(slotEntry.name);
				if (slotIndex == -1) throw new SerializationException("Slot not found: " + slotEntry.name);
				for (JsonValue entry = slotEntry.child; entry != null; entry = entry.next) {
					Attachment attachment = readAttachment(skin, entry.name, entry);
					if (attachment != null) skin.addAttachment(slotIndex, entry.name, attachment);
				}
			}
			skeletonData.addSkin(skin);
			if (skin.name.equals("default")) skeletonData.defaultSkin = skin;
		}

		// Events.
		for (JsonValue eventMap = root.getChild("events"); eventMap != null; eventMap = eventMap.next) {
			EventData eventData = new EventData(eventMap.name);
			eventData.intValue = eventMap.getInt("int", 0);
			eventData.floatValue = eventMap.getFloat("float", 0f);
			eventData.stringValue = eventMap.getString("string", null);
			skeletonData.addEvent(eventData);
		}

		// Animations.
		for (JsonValue animationMap = root.getChild("animations"); animationMap != null; animationMap = animationMap.next)
			readAnimation(animationMap.name, animationMap, skeletonData);

		skeletonData.bones.shrink();
		skeletonData.slots.shrink();
		skeletonData.skins.shrink();
		skeletonData.animations.shrink();
		return skeletonData;
	}

	private Attachment readAttachment (Skin skin, String name, JsonValue map) {
		float scale = this.scale;
		name = map.getString("name", name);

		switch (AttachmentType.valueOf(map.getString("type", AttachmentType.region.name()))) {
		case region:
			RegionAttachment region = attachmentLoader.newRegionAttachment(skin, name, map.getString("path", name));
			region.setX(map.getFloat("x", 0) * scale);
			region.setY(map.getFloat("y", 0) * scale);
			region.setScaleX(map.getFloat("scaleX", 1));
			region.setScaleY(map.getFloat("scaleY", 1));
			region.setRotation(map.getFloat("rotation", 0));
			region.setWidth(map.getFloat("width") * scale);
			region.setHeight(map.getFloat("height") * scale);

			String color = map.getString("color", null);
			if (color != null) region.getColor().set(Color.valueOf(color));

			region.updateOffset();
			return region;
		case boundingbox: {
			BoundingBoxAttachment box = attachmentLoader.newBoundingBoxAttachment(skin, name);
			float[] vertices = map.require("vertices").asFloatArray();
			if (scale != 1) {
				for (int i = 0, n = vertices.length; i < n; i++)
					vertices[i] *= scale;
			}
			box.setVertices(vertices);
			return box;
		}
		case mesh: {
			MeshAttachment mesh = attachmentLoader.newMeshAttachment(skin, name, map.getString("path", name));
			float[] vertices = map.require("vertices").asFloatArray();
			if (scale != 1) {
				for (int i = 0, n = vertices.length; i < n; i++)
					vertices[i] *= scale;
			}
			short[] triangles = map.require("triangles").asShortArray();
			float[] uvs = map.require("uvs").asFloatArray();
			mesh.setMesh(vertices, triangles, uvs);
			if (map.has("hull")) mesh.setHullLength(map.require("hull").asInt());
			if (map.has("edges")) mesh.setEdges(map.require("edges").asIntArray());
			mesh.setWidth(map.getFloat("width", 0) * scale);
			mesh.setHeight(map.getFloat("height", 0) * scale);
			return mesh;
		}
		}

		// RegionSequenceAttachment regionSequenceAttachment = (RegionSequenceAttachment)attachment;
		//
		// float fps = map.getFloat("fps");
		// regionSequenceAttachment.setFrameTime(fps);
		//
		// String modeString = map.getString("mode");
		// regionSequenceAttachment.setMode(modeString == null ? Mode.forward : Mode.valueOf(modeString));

		return null;
	}

	private void readAnimation (String name, JsonValue map, SkeletonData skeletonData) {
		Array<Timeline> timelines = new Array();
		float duration = 0;

		// Slot timelines.
		for (JsonValue slotMap = map.getChild("slots"); slotMap != null; slotMap = slotMap.next) {
			int slotIndex = skeletonData.findSlotIndex(slotMap.name);
			if (slotIndex == -1) throw new SerializationException("Slot not found: " + slotMap.name);

			for (JsonValue timelineMap = slotMap.child; timelineMap != null; timelineMap = timelineMap.next) {
				String timelineName = timelineMap.name;
				if (timelineName.equals("color")) {
					ColorTimeline timeline = new ColorTimeline(timelineMap.size);
					timeline.slotIndex = slotIndex;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child; valueMap != null; valueMap = valueMap.next) {
						Color color = Color.valueOf(valueMap.getString("color"));
						timeline.setFrame(frameIndex, valueMap.getFloat("time"), color.r, color.g, color.b, color.a);
						readCurve(timeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() * 5 - 5]);

				} else if (timelineName.equals("attachment")) {
					AttachmentTimeline timeline = new AttachmentTimeline(timelineMap.size);
					timeline.slotIndex = slotIndex;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child; valueMap != null; valueMap = valueMap.next)
						timeline.setFrame(frameIndex++, valueMap.getFloat("time"), valueMap.getString("name"));
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() - 1]);
				} else
					throw new RuntimeException("Invalid timeline type for a slot: " + timelineName + " (" + slotMap.name + ")");
			}
		}

		// Bone timelines.
		for (JsonValue boneMap = map.getChild("bones"); boneMap != null; boneMap = boneMap.next) {
			int boneIndex = skeletonData.findBoneIndex(boneMap.name);
			if (boneIndex == -1) throw new SerializationException("Bone not found: " + boneMap.name);

			for (JsonValue timelineMap = boneMap.child; timelineMap != null; timelineMap = timelineMap.next) {
				String timelineName = timelineMap.name;
				if (timelineName.equals("rotate")) {
					RotateTimeline timeline = new RotateTimeline(timelineMap.size);
					timeline.boneIndex = boneIndex;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child; valueMap != null; valueMap = valueMap.next) {
						timeline.setFrame(frameIndex, valueMap.getFloat("time"), valueMap.getFloat("angle"));
						readCurve(timeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() * 2 - 2]);

				} else if (timelineName.equals("translate") || timelineName.equals("scale")) {
					TranslateTimeline timeline;
					float timelineScale = 1;
					if (timelineName.equals("scale"))
						timeline = new ScaleTimeline(timelineMap.size);
					else {
						timeline = new TranslateTimeline(timelineMap.size);
						timelineScale = scale;
					}
					timeline.boneIndex = boneIndex;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child; valueMap != null; valueMap = valueMap.next) {
						float x = valueMap.getFloat("x", 0), y = valueMap.getFloat("y", 0);
						timeline.setFrame(frameIndex, valueMap.getFloat("time"), x * timelineScale, y * timelineScale);
						readCurve(timeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() * 3 - 3]);

				} else
					throw new RuntimeException("Invalid timeline type for a bone: " + timelineName + " (" + boneMap.name + ")");
			}
		}

		// FFD timelines.
		for (JsonValue ffdMap = map.getChild("ffd"); ffdMap != null; ffdMap = ffdMap.next) {
			Skin skin = skeletonData.findSkin(ffdMap.name);
			if (skin == null) throw new SerializationException("Skin not found: " + ffdMap.name);
			for (JsonValue slotMap = ffdMap.child; slotMap != null; slotMap = slotMap.next) {
				int slotIndex = skeletonData.findSlotIndex(slotMap.name);
				if (slotIndex == -1) throw new SerializationException("Slot not found: " + slotMap.name);
				for (JsonValue meshMap = slotMap.child; meshMap != null; meshMap = meshMap.next) {
					FfdTimeline timeline = new FfdTimeline(meshMap.size);
					MeshAttachment mesh = (MeshAttachment)skin.getAttachment(slotIndex, meshMap.name);
					if (mesh == null) throw new SerializationException("Mesh attachment not found: " + meshMap.name);
					timeline.slotIndex = slotIndex;
					timeline.meshAttachment = mesh;

					int frameIndex = 0;
					for (JsonValue valueMap = meshMap.child; valueMap != null; valueMap = valueMap.next) {
						float[] vertices;
						JsonValue verticesValue = valueMap.get("vertices");
						if (verticesValue == null)
							vertices = mesh.getVertices();
						else {
							vertices = verticesValue.asFloatArray();
							if (scale != 1) {
								for (int i = 0, n = vertices.length; i < n; i++)
									vertices[i] *= scale;
							}
						}
						timeline.setFrame(frameIndex, valueMap.getFloat("time"), vertices);
						readCurve(timeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() - 1]);
				}
			}
		}

		// Draw order timeline.
		JsonValue drawOrdersMap = map.get("draworder");
		if (drawOrdersMap != null) {
			DrawOrderTimeline timeline = new DrawOrderTimeline(drawOrdersMap.size);
			int slotCount = skeletonData.slots.size;
			int frameIndex = 0;
			for (JsonValue drawOrderMap = drawOrdersMap.child; drawOrderMap != null; drawOrderMap = drawOrderMap.next) {
				int[] drawOrder = null;
				JsonValue offsets = drawOrderMap.get("offsets");
				if (offsets != null) {
					drawOrder = new int[slotCount];
					for (int i = slotCount - 1; i >= 0; i--)
						drawOrder[i] = -1;
					int[] unchanged = new int[slotCount - offsets.size];
					int originalIndex = 0, unchangedIndex = 0;
					for (JsonValue offsetMap = offsets.child; offsetMap != null; offsetMap = offsetMap.next) {
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
				}
				timeline.setFrame(frameIndex++, drawOrderMap.getFloat("time"), drawOrder);
			}
			timelines.add(timeline);
			duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() - 1]);
		}

		// Event timeline.
		JsonValue eventsMap = map.get("events");
		if (eventsMap != null) {
			EventTimeline timeline = new EventTimeline(eventsMap.size);
			int frameIndex = 0;
			for (JsonValue eventMap = eventsMap.child; eventMap != null; eventMap = eventMap.next) {
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
