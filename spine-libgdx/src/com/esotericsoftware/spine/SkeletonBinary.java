
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
import com.esotericsoftware.spine.attachments.TextureAtlasAttachmentResolver;

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

	static public final int ATTACHMENT_REGION = 0;
	static public final int ATTACHMENT_REGION_SEQUENCE = 1;

	static public final int CURVE_LINEAR = 0;
	static public final int CURVE_STEPPED = 1;
	static public final int CURVE_BEZIER = 2;

	private final AttachmentResolver attachmentResolver;
	private float scale = 1;

	public SkeletonBinary (TextureAtlas atlas) {
		attachmentResolver = new TextureAtlasAttachmentResolver(atlas);
	}

	public SkeletonBinary (AttachmentResolver attachmentResolver) {
		this.attachmentResolver = attachmentResolver;
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

		SkeletonData skeletonData = new SkeletonData(attachmentResolver);
		DataInput input = new DataInput(file.read(512));
		try {
			// Bones.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				String name = input.readString();
				BoneData parent = null;
				String parentName = input.readString();
				if (parentName != null) {
					parent = skeletonData.findBone(parentName);
					if (parent == null) throw new SerializationException("Bone not found: " + parentName);
				}
				BoneData boneData = new BoneData(name, parent);
				boneData.x = input.readFloat() * scale;
				boneData.y = input.readFloat() * scale;
				boneData.scaleX = input.readFloat();
				boneData.scaleY = input.readFloat();
				boneData.rotation = input.readFloat();
				boneData.length = input.readFloat() * scale;
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

			input.close();
		} catch (IOException ex) {
			throw new SerializationException("Error reading skeleton file.", ex);
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
				skin.addAttachment(slotIndex, name, readAttachment(input, name));
			}
		}
		return skin;
	}

	private Attachment readAttachment (DataInput input, String attachmentName) throws IOException {
		String name = input.readString();
		if (name == null) name = attachmentName;

		Attachment attachment;
		int type = input.readByte();
		switch (type) {
		case ATTACHMENT_REGION:
			attachment = new RegionAttachment(name);
			break;
		case ATTACHMENT_REGION_SEQUENCE:
			float fps = input.readFloat();
			Mode mode = Mode.values()[input.readInt(true)];
			attachment = new RegionSequenceAttachment(name, 1 / fps, mode);
			break;
		default:
			throw new SerializationException("Unknown attachment type: " + type + " (" + name + ")");
		}

		attachment.setX(input.readFloat() * scale);
		attachment.setY(input.readFloat() * scale);
		attachment.setScaleX(input.readFloat());
		attachment.setScaleY(input.readFloat());
		attachment.setRotation(input.readFloat());
		attachment.setWidth(input.readFloat() * scale);
		attachment.setHeight(input.readFloat() * scale);
		return attachment;
	}

	public Animation readAnimation (FileHandle file, SkeletonData skeleton) {
		if (file == null) throw new IllegalArgumentException("file cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");

		Array<Timeline> timelines = new Array();
		float duration = 0;

		DataInput input = new DataInput(file.read(512));
		try {
			int boneCount = input.readInt(true);
			for (int i = 0; i < boneCount; i++) {
				String boneName = input.readString();
				int boneIndex = skeleton.findBoneIndex(boneName);
				if (boneIndex == -1) throw new SerializationException("Bone not found: " + boneName);
				int itemCount = input.readInt(true);
				for (int ii = 0; ii < itemCount; ii++) {
					int timelineType = input.readByte();
					int keyCount = input.readInt(true);
					switch (timelineType) {
					case TIMELINE_ROTATE: {
						RotateTimeline timeline = new RotateTimeline(keyCount);
						timeline.setBoneIndex(boneIndex);
						for (int keyframeIndex = 0; keyframeIndex < keyCount; keyframeIndex++) {
							timeline.setKeyframe(keyframeIndex, input.readFloat(), input.readFloat());
							if (keyframeIndex < keyCount - 1) readCurve(input, keyframeIndex, timeline);
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getDuration());
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
						for (int keyframeIndex = 0; keyframeIndex < keyCount; keyframeIndex++) {
							timeline.setKeyframe(keyframeIndex, input.readFloat(), input.readFloat() * timelineScale, input.readFloat()
								* timelineScale);
							if (keyframeIndex < keyCount - 1) readCurve(input, keyframeIndex, timeline);
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getDuration());
						break;
					default:
						throw new RuntimeException("Invalid timeline type for a bone: " + timelineType + " (" + boneName + ")");
					}
				}
			}

			int slotCount = input.readInt(true);
			for (int i = 0; i < slotCount; i++) {
				String slotName = input.readString();
				int slotIndex = skeleton.findSlotIndex(slotName);
				int itemCount = input.readInt(true);
				for (int ii = 0; ii < itemCount; ii++) {
					int timelineType = input.readByte();
					int keyCount = input.readInt(true);
					switch (timelineType) {
					case TIMELINE_COLOR: {
						ColorTimeline timeline = new ColorTimeline(keyCount);
						timeline.setSlotIndex(slotIndex);
						for (int keyframeIndex = 0; keyframeIndex < keyCount; keyframeIndex++) {
							float time = input.readFloat();
							Color.rgba8888ToColor(Color.tmp, input.readInt());
							timeline.setKeyframe(keyframeIndex, time, Color.tmp.r, Color.tmp.g, Color.tmp.b, Color.tmp.a);
							if (keyframeIndex < keyCount - 1) readCurve(input, keyframeIndex, timeline);
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getDuration());
						break;
					}
					case TIMELINE_ATTACHMENT:
						AttachmentTimeline timeline = new AttachmentTimeline(keyCount);
						timeline.setSlotIndex(slotIndex);
						for (int keyframeIndex = 0; keyframeIndex < keyCount; keyframeIndex++)
							timeline.setKeyframe(keyframeIndex, input.readFloat(), input.readString());
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getDuration());
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
		return new Animation(timelines, duration);
	}

	private void readCurve (DataInput input, int keyframeIndex, CurveTimeline timeline) throws IOException {
		switch (input.readByte()) {
		case CURVE_STEPPED:
			timeline.setStepped(keyframeIndex);
			break;
		case CURVE_BEZIER:
			timeline.setCurve(keyframeIndex, input.readFloat(), input.readFloat(), input.readFloat(), input.readFloat());
			break;
		}
	}
}
