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
import com.esotericsoftware.spine.attachments.AtlasAttachmentLoader;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.AttachmentLoader;
import com.esotericsoftware.spine.attachments.AttachmentType;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.RegionSequenceAttachment;
import com.esotericsoftware.spine.attachments.RegionSequenceAttachment.Mode;

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
		skeletonData.setName(file.nameWithoutExtension());

		DataInput input = new DataInput(file.read(512));
		try {
			// Bones.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				String name = input.readString();
				BoneData parent = null;
				String parentName = input.readString();
				if (parentName != null) {
					parent = skeletonData.findBone(parentName);
					if (parent == null) throw new SerializationException("Parent bone not found: " + parentName);
				}
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
				String boneName = input.readString();
				BoneData boneData = skeletonData.findBone(boneName);
				if (boneData == null) throw new SerializationException("Bone not found: " + boneName);
				SlotData slotData = new SlotData(slotName, boneData);
				Color.rgba8888ToColor(slotData.getColor(), input.readInt());
				slotData.setAttachmentName(input.readString());
				slotData.additiveBlending = input.readByte() == 1;
				skeletonData.addSlot(slotData);
			}

			// Default skin.
			Skin defaultSkin = readSkin(input, "default");
			if (defaultSkin != null) {
				skeletonData.setDefaultSkin(defaultSkin);
				skeletonData.addSkin(defaultSkin);
			}

			// Skins.
			for (int i = 0, n = input.readInt(true); i < n; i++)
				skeletonData.addSkin(readSkin(input, input.readString()));

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
			int attachmentCount = input.readInt(true);
			for (int ii = 0; ii < attachmentCount; ii++) {
				String name = input.readString();
				skin.addAttachment(slotIndex, name, readAttachment(input, skin, name));
			}
		}
		return skin;
	}

	private Attachment readAttachment (DataInput input, Skin skin, String attachmentName) throws IOException {
		String name = input.readString();
		if (name == null) name = attachmentName;

		AttachmentType type = AttachmentType.values()[input.readByte()];
		Attachment attachment = attachmentLoader.newAttachment(skin, type, name);

		if (attachment instanceof RegionSequenceAttachment) {
			RegionSequenceAttachment regionSequenceAttachment = (RegionSequenceAttachment)attachment;
			regionSequenceAttachment.setFrameTime(1 / input.readFloat());
			regionSequenceAttachment.setMode(Mode.values()[input.readInt(true)]);
		}

		if (attachment instanceof RegionAttachment) {
			RegionAttachment regionAttachment = (RegionAttachment)attachment;
			regionAttachment.setX(input.readFloat() * scale);
			regionAttachment.setY(input.readFloat() * scale);
			regionAttachment.setScaleX(input.readFloat());
			regionAttachment.setScaleY(input.readFloat());
			regionAttachment.setRotation(input.readFloat());
			regionAttachment.setWidth(input.readFloat() * scale);
			regionAttachment.setHeight(input.readFloat() * scale);
			regionAttachment.updateOffset();
		}

		return attachment;
	}

	private void readAnimation (String name, DataInput input, SkeletonData skeletonData) {
		Array<Timeline> timelines = new Array();
		float duration = 0;

		try {
			int boneCount = input.readInt(true);
			for (int i = 0; i < boneCount; i++) {
				String boneName = input.readString();
				int boneIndex = skeletonData.findBoneIndex(boneName);
				if (boneIndex == -1) throw new SerializationException("Bone not found: " + boneName);
				int itemCount = input.readInt(true);
				for (int ii = 0; ii < itemCount; ii++) {
					int timelineType = input.readByte();
					int keyCount = input.readInt(true);
					switch (timelineType) {
					case TIMELINE_ROTATE: {
						RotateTimeline timeline = new RotateTimeline(keyCount);
						timeline.setBoneIndex(boneIndex);
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
						timeline.setBoneIndex(boneIndex);
						for (int frameIndex = 0; frameIndex < keyCount; frameIndex++) {
							timeline.setFrame(frameIndex, input.readFloat(), input.readFloat() * timelineScale, input.readFloat()
								* timelineScale);
							if (frameIndex < keyCount - 1) readCurve(input, frameIndex, timeline);
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[keyCount * 3 - 3]);
						break;
					default:
						throw new RuntimeException("Invalid timeline type for a bone: " + timelineType + " (" + boneName + ")");
					}
				}
			}

			int slotCount = input.readInt(true);
			for (int i = 0; i < slotCount; i++) {
				String slotName = input.readString();
				int slotIndex = skeletonData.findSlotIndex(slotName);
				int itemCount = input.readInt(true);
				for (int ii = 0; ii < itemCount; ii++) {
					int timelineType = input.readByte();
					int keyCount = input.readInt(true);
					switch (timelineType) {
					case TIMELINE_COLOR: {
						ColorTimeline timeline = new ColorTimeline(keyCount);
						timeline.setSlotIndex(slotIndex);
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
						timeline.setSlotIndex(slotIndex);
						for (int frameIndex = 0; frameIndex < keyCount; frameIndex++)
							timeline.setFrame(frameIndex, input.readFloat(), input.readString());
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[keyCount - 1]);
						break;
					default:
						throw new RuntimeException("Invalid timeline type for a slot: " + timelineType + " (" + slotName + ")");
					}
				}
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
