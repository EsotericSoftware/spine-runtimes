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
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
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

package com.esotericsoftware.spine;

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
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.DataInput;
import com.badlogic.gdx.utils.SerializationException;

import java.io.IOException;

public class SkeletonBinary {
	static public final int TIMELINE_SCALE = 0;
	static public final int TIMELINE_ROTATE = 1;
	static public final int TIMELINE_TRANSLATE = 2;
	static public final int TIMELINE_ATTACHMENT = 3;
	static public final int TIMELINE_COLOR = 4;
	static public final int TIMELINE_EVENT = 5;
	static public final int TIMELINE_DRAWORDER = 6;

	static public final int CURVE_LINEAR = 0;
	static public final int CURVE_STEPPED = 1;
	static public final int CURVE_BEZIER = 2;

	static private final Color tempColor = new Color();

	private final AttachmentLoader attachmentLoader;
	private float scale = 1;

	public SkeletonBinary (TextureAtlas atlas) {
		attachmentLoader = new AtlasAttachmentLoader(atlas);
	}

	public SkeletonBinary (AttachmentLoader attachmentLoader) {
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

		DataInput input = new DataInput(file.read(512));
		try {
			// Bones.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				String name = input.readString();
				BoneData parent = null;
				int parentIndex = input.readInt(true) - 1;
				if (parentIndex != -1) parent = skeletonData.bones.get(parentIndex);
				BoneData boneData = new BoneData(name, parent);
				boneData.x = input.readFloat() * scale;
				boneData.y = input.readFloat() * scale;
				boneData.scaleX = input.readFloat();
				boneData.scaleY = input.readFloat();
				boneData.rotation = input.readFloat();
				boneData.length = input.readFloat() * scale;
				boneData.inheritScale = input.readByte() == 1;
				boneData.inheritRotation = input.readByte() == 1;
				skeletonData.addBone(boneData);
			}

			// Slots.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				String slotName = input.readString();
				BoneData boneData = skeletonData.bones.get(input.readInt(true));
				SlotData slotData = new SlotData(slotName, boneData);
				Color.rgba8888ToColor(slotData.getColor(), input.readInt());
				slotData.attachmentName = input.readString();
				slotData.additiveBlending = input.readByte() == 1;
				skeletonData.addSlot(slotData);
			}

			// Default skin.
			Skin defaultSkin = readSkin(input, "default");
			if (defaultSkin != null) {
				skeletonData.defaultSkin = defaultSkin;
				skeletonData.addSkin(defaultSkin);
			}

			// Skins.
			for (int i = 0, n = input.readInt(true); i < n; i++)
				skeletonData.addSkin(readSkin(input, input.readString()));

			// Events.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				EventData eventData = new EventData(input.readString());
				eventData.intValue = input.readInt(false);
				eventData.floatValue = input.readFloat();
				eventData.stringValue = input.readString();
				skeletonData.addEvent(eventData);
			}

			// Animations.
			for (int i = 0, n = input.readInt(true); i < n; i++)
				readAnimation(input.readString(), input, skeletonData);

		} catch (IOException ex) {
			throw new SerializationException("Error reading skeleton file.", ex);
		} finally {
			try {
				input.close();
			} catch (IOException ignored) {
			}
		}

		skeletonData.bones.shrink();
		skeletonData.slots.shrink();
		skeletonData.skins.shrink();
		return skeletonData;
	}

	private Skin readSkin (DataInput input, String skinName) throws IOException {
		int slotCount = input.readInt(true);
		if (slotCount == 0) return null;
		Skin skin = new Skin(skinName);
		for (int i = 0; i < slotCount; i++) {
			int slotIndex = input.readInt(true);
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				String name = input.readString();
				skin.addAttachment(slotIndex, name, readAttachment(input, skin, name));
			}
		}
		return skin;
	}

	private Attachment readAttachment (DataInput input, Skin skin, String attachmentName) throws IOException {
		String name = input.readString();
		if (name == null) name = attachmentName;

		switch (AttachmentType.values()[input.readByte()]) {
		case region: {
			String path = input.readString();
			if (path == null) path = name;
			RegionAttachment region = attachmentLoader.newRegionAttachment(skin, name, path);
			if (region == null) return null;
			region.setX(input.readFloat() * scale);
			region.setY(input.readFloat() * scale);
			region.setScaleX(input.readFloat());
			region.setScaleY(input.readFloat());
			region.setRotation(input.readFloat());
			region.setWidth(input.readFloat() * scale);
			region.setHeight(input.readFloat() * scale);
			Color.rgba8888ToColor(region.getColor(), input.readInt());
			region.updateOffset();
			return region;
		}
		case boundingbox: {
			BoundingBoxAttachment box = attachmentLoader.newBoundingBoxAttachment(skin, name);
			if (box == null) return null;
			box.setVertices(readFloatArray(input, scale));
			return box;
		}
		case mesh: {
			String path = input.readString();
			if (path == null) path = name;
			MeshAttachment mesh = attachmentLoader.newMeshAttachment(skin, name, path);
			float[] vertices = readFloatArray(input, scale);
			short[] triangles = readShortArray(input);
			float[] uvs = readFloatArray(input, 1);
			Color.rgba8888ToColor(mesh.getColor(), input.readInt());
			mesh.setEdges(readIntArray(input));
			if (mesh.getEdges().length > 0) {
				mesh.setHullLength(input.readInt(true));
				mesh.setWidth(input.readFloat() * scale);
				mesh.setHeight(input.readFloat() * scale);
			}
			mesh.setMesh(vertices, triangles, uvs);
			return mesh;
		}
		}
		return null;
	}

	private float[] readFloatArray (DataInput input, float scale) throws IOException {
		int n = input.readInt(true);
		float[] array = new float[n];
		for (int i = 0; i < n; i++)
			array[i] = input.readFloat() * scale;
		return array;
	}

	private short[] readShortArray (DataInput input) throws IOException {
		int n = input.readInt(true);
		short[] array = new short[n];
		for (int i = 0; i < n; i++)
			array[i] = input.readShort();
		return array;
	}

	private int[] readIntArray (DataInput input) throws IOException {
		int n = input.readInt(true);
		int[] array = new int[n];
		for (int i = 0; i < n; i++)
			array[i] = input.readInt(true);
		return array;
	}

	private void readAnimation (String name, DataInput input, SkeletonData skeletonData) {
		Array<Timeline> timelines = new Array();
		float duration = 0;

		try {
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				int boneIndex = input.readInt(true);
				int itemCount = input.readInt(true);
				for (int ii = 0; ii < itemCount; ii++) {
					int timelineType = input.readByte();
					int keyCount = input.readInt(true);
					switch (timelineType) {
					case TIMELINE_ROTATE: {
						RotateTimeline timeline = new RotateTimeline(keyCount);
						timeline.boneIndex = boneIndex;
						for (int frameIndex = 0; frameIndex < keyCount; frameIndex++) {
							timeline.setFrame(frameIndex, input.readFloat(), input.readFloat());
							if (frameIndex < keyCount - 1) readCurve(input, frameIndex, timeline);
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[keyCount * 2 - 2]);
						break;
					}
					case TIMELINE_TRANSLATE:
					case TIMELINE_SCALE:
						TranslateTimeline timeline;
						float timelineScale = 1;
						if (timelineType == TIMELINE_SCALE)
							timeline = new ScaleTimeline(keyCount);
						else {
							timeline = new TranslateTimeline(keyCount);
							timelineScale = scale;
						}
						timeline.boneIndex = boneIndex;
						for (int frameIndex = 0; frameIndex < keyCount; frameIndex++) {
							timeline.setFrame(frameIndex, input.readFloat(), input.readFloat() * timelineScale, input.readFloat()
								* timelineScale);
							if (frameIndex < keyCount - 1) readCurve(input, frameIndex, timeline);
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[keyCount * 3 - 3]);
						break;
					}
				}
			}

			for (int i = 0, n = input.readInt(true); i < n; i++) {
				int slotIndex = input.readInt(true);
				int itemCount = input.readInt(true);
				for (int ii = 0; ii < itemCount; ii++) {
					int timelineType = input.readByte();
					int keyCount = input.readInt(true);
					switch (timelineType) {
					case TIMELINE_COLOR: {
						ColorTimeline timeline = new ColorTimeline(keyCount);
						timeline.slotIndex = slotIndex;
						for (int frameIndex = 0; frameIndex < keyCount; frameIndex++) {
							float time = input.readFloat();
							Color.rgba8888ToColor(tempColor, input.readInt());
							timeline.setFrame(frameIndex, time, tempColor.r, tempColor.g, tempColor.b, tempColor.a);
							if (frameIndex < keyCount - 1) readCurve(input, frameIndex, timeline);
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[keyCount * 5 - 5]);
						break;
					}
					case TIMELINE_ATTACHMENT:
						AttachmentTimeline timeline = new AttachmentTimeline(keyCount);
						timeline.slotIndex = slotIndex;
						for (int frameIndex = 0; frameIndex < keyCount; frameIndex++)
							timeline.setFrame(frameIndex, input.readFloat(), input.readString());
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[keyCount - 1]);
						break;
					}
				}
			}

			int eventCount = input.readInt(true);
			if (eventCount > 0) {
				EventTimeline timeline = new EventTimeline(eventCount);
				for (int i = 0; i < eventCount; i++) {
					float time = input.readFloat();
					EventData eventData = skeletonData.events.get(input.readInt(true));
					Event event = new Event(eventData);
					event.intValue = input.readInt(false);
					event.floatValue = input.readFloat();
					event.stringValue = input.readBoolean() ? input.readString() : eventData.stringValue;
					timeline.setFrame(i, time, event);
				}
				timelines.add(timeline);
				duration = Math.max(duration, timeline.getFrames()[eventCount - 1]);
			}

			int drawOrderCount = input.readInt(true);
			if (drawOrderCount > 0) {
				DrawOrderTimeline timeline = new DrawOrderTimeline(drawOrderCount);
				int slotCount = skeletonData.slots.size;
				for (int i = 0; i < drawOrderCount; i++) {
					int offsetCount = input.readInt(true);
					int[] drawOrder = new int[slotCount];
					for (int ii = slotCount - 1; ii >= 0; ii--)
						drawOrder[ii] = -1;
					int[] unchanged = new int[slotCount - offsetCount];
					int originalIndex = 0, unchangedIndex = 0;
					for (int ii = 0; ii < offsetCount; ii++) {
						int slotIndex = input.readInt(true);
						// Collect unchanged items.
						while (originalIndex != slotIndex)
							unchanged[unchangedIndex++] = originalIndex++;
						// Set changed items.
						drawOrder[originalIndex + input.readInt(true)] = originalIndex++;
					}
					// Collect remaining unchanged items.
					while (originalIndex < slotCount)
						unchanged[unchangedIndex++] = originalIndex++;
					// Fill in unchanged items.
					for (int ii = slotCount - 1; ii >= 0; ii--)
						if (drawOrder[ii] == -1) drawOrder[ii] = unchanged[--unchangedIndex];
					timeline.setFrame(i, input.readFloat(), drawOrder);
				}
				timelines.add(timeline);
				duration = Math.max(duration, timeline.getFrames()[drawOrderCount - 1]);
			}
		} catch (IOException ex) {
			throw new SerializationException("Error reading skeleton file.", ex);
		}

		timelines.shrink();
		skeletonData.addAnimation(new Animation(name, timelines, duration));
	}

	private void readCurve (DataInput input, int frameIndex, CurveTimeline timeline) throws IOException {
		switch (input.readByte()) {
		case CURVE_STEPPED:
			timeline.setStepped(frameIndex);
			break;
		case CURVE_BEZIER:
			setCurve(timeline, frameIndex, input.readFloat(), input.readFloat(), input.readFloat(), input.readFloat());
			break;
		}
	}

	void setCurve (CurveTimeline timeline, int frameIndex, float cx1, float cy1, float cx2, float cy2) {
		timeline.setCurve(frameIndex, cx1, cy1, cx2, cy2);
	}
}
