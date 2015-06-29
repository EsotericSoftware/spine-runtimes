/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.JsonReader;
import com.badlogic.gdx.utils.JsonValue;
import com.badlogic.gdx.utils.SerializationException;
import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.ColorTimeline;
import com.esotericsoftware.spine.Animation.CurveTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderTimeline;
import com.esotericsoftware.spine.Animation.EventTimeline;
import com.esotericsoftware.spine.Animation.FfdTimeline;
import com.esotericsoftware.spine.Animation.FlipXTimeline;
import com.esotericsoftware.spine.Animation.FlipYTimeline;
import com.esotericsoftware.spine.Animation.IkConstraintTimeline;
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
import com.esotericsoftware.spine.attachments.SkinnedMeshAttachment;

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

		// Skeleton.
		JsonValue skeletonMap = root.get("skeleton");
		if (skeletonMap != null) {
			skeletonData.hash = skeletonMap.getString("hash", null);
			skeletonData.version = skeletonMap.getString("spine", null);
			skeletonData.width = skeletonMap.getFloat("width", 0);
			skeletonData.height = skeletonMap.getFloat("height", 0);
			skeletonData.imagesPath = skeletonMap.getString("images", null);
		}

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
			boneData.flipX = boneMap.getBoolean("flipX", false);
			boneData.flipY = boneMap.getBoolean("flipY", false);
			boneData.inheritScale = boneMap.getBoolean("inheritScale", true);
			boneData.inheritRotation = boneMap.getBoolean("inheritRotation", true);

			String color = boneMap.getString("color", null);
			if (color != null) boneData.getColor().set(Color.valueOf(color));

			skeletonData.bones.add(boneData);
		}

		// IK constraints.
		for (JsonValue ikMap = root.getChild("ik"); ikMap != null; ikMap = ikMap.next) {
			IkConstraintData ikConstraintData = new IkConstraintData(ikMap.getString("name"));

			for (JsonValue boneMap = ikMap.getChild("bones"); boneMap != null; boneMap = boneMap.next) {
				String boneName = boneMap.asString();
				BoneData bone = skeletonData.findBone(boneName);
				if (bone == null) throw new SerializationException("IK bone not found: " + boneName);
				ikConstraintData.bones.add(bone);
			}

			String targetName = ikMap.getString("target");
			ikConstraintData.target = skeletonData.findBone(targetName);
			if (ikConstraintData.target == null) throw new SerializationException("Target bone not found: " + targetName);

			ikConstraintData.bendDirection = ikMap.getBoolean("bendPositive", true) ? 1 : -1;
			ikConstraintData.mix = ikMap.getFloat("mix", 1);

			skeletonData.ikConstraints.add(ikConstraintData);
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
			slotData.blendMode = BlendMode.valueOf(slotMap.getString("blend", BlendMode.normal.name()));
			skeletonData.slots.add(slotData);
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
			skeletonData.skins.add(skin);
			if (skin.name.equals("default")) skeletonData.defaultSkin = skin;
		}

		// Events.
		for (JsonValue eventMap = root.getChild("events"); eventMap != null; eventMap = eventMap.next) {
			EventData eventData = new EventData(eventMap.name);
			eventData.intValue = eventMap.getInt("int", 0);
			eventData.floatValue = eventMap.getFloat("float", 0f);
			eventData.stringValue = eventMap.getString("string", null);
			skeletonData.events.add(eventData);
		}

		// Animations.
		for (JsonValue animationMap = root.getChild("animations"); animationMap != null; animationMap = animationMap.next)
			readAnimation(animationMap.name, animationMap, skeletonData);

		skeletonData.bones.shrink();
		skeletonData.slots.shrink();
		skeletonData.skins.shrink();
		skeletonData.events.shrink();
		skeletonData.animations.shrink();
		skeletonData.ikConstraints.shrink();
		return skeletonData;
	}

	private Attachment readAttachment (Skin skin, String name, JsonValue map) {
		float scale = this.scale;
		name = map.getString("name", name);
		String path = map.getString("path", name);

		switch (AttachmentType.valueOf(map.getString("type", AttachmentType.region.name()))) {
		case region: {
			RegionAttachment region = attachmentLoader.newRegionAttachment(skin, name, path);
			if (region == null) return null;
			region.setPath(path);
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
		}
		case boundingbox: {
			BoundingBoxAttachment box = attachmentLoader.newBoundingBoxAttachment(skin, name);
			if (box == null) return null;
			float[] vertices = map.require("vertices").asFloatArray();
			if (scale != 1) {
				for (int i = 0, n = vertices.length; i < n; i++)
					vertices[i] *= scale;
			}
			box.setVertices(vertices);
			return box;
		}
		case mesh: {
			MeshAttachment mesh = attachmentLoader.newMeshAttachment(skin, name, path);
			if (mesh == null) return null;
			mesh.setPath(path);
			float[] vertices = map.require("vertices").asFloatArray();
			if (scale != 1) {
				for (int i = 0, n = vertices.length; i < n; i++)
					vertices[i] *= scale;
			}
			mesh.setVertices(vertices);
			mesh.setTriangles(map.require("triangles").asShortArray());
			mesh.setRegionUVs(map.require("uvs").asFloatArray());
			mesh.updateUVs();

			String color = map.getString("color", null);
			if (color != null) mesh.getColor().set(Color.valueOf(color));

			if (map.has("hull")) mesh.setHullLength(map.require("hull").asInt() * 2);
			if (map.has("edges")) mesh.setEdges(map.require("edges").asIntArray());
			mesh.setWidth(map.getFloat("width", 0) * scale);
			mesh.setHeight(map.getFloat("height", 0) * scale);
			return mesh;
		}
		case skinnedmesh: {
			SkinnedMeshAttachment mesh = attachmentLoader.newSkinnedMeshAttachment(skin, name, path);
			if (mesh == null) return null;
			mesh.setPath(path);
			float[] uvs = map.require("uvs").asFloatArray();
			float[] vertices = map.require("vertices").asFloatArray();
			FloatArray weights = new FloatArray(uvs.length * 3 * 3);
			IntArray bones = new IntArray(uvs.length * 3);
			for (int i = 0, n = vertices.length; i < n;) {
				int boneCount = (int)vertices[i++];
				bones.add(boneCount);
				for (int nn = i + boneCount * 4; i < nn;) {
					bones.add((int)vertices[i]);
					weights.add(vertices[i + 1] * scale);
					weights.add(vertices[i + 2] * scale);
					weights.add(vertices[i + 3]);
					i += 4;
				}
			}
			mesh.setBones(bones.toArray());
			mesh.setWeights(weights.toArray());
			mesh.setTriangles(map.require("triangles").asShortArray());
			mesh.setRegionUVs(uvs);
			mesh.updateUVs();

			String color = map.getString("color", null);
			if (color != null) mesh.getColor().set(Color.valueOf(color));

			if (map.has("hull")) mesh.setHullLength(map.require("hull").asInt() * 2);
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
		float scale = this.scale;
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

				} else if (timelineName.equals("flipX") || timelineName.equals("flipY")) {
					boolean x = timelineName.equals("flipX");
					FlipXTimeline timeline = x ? new FlipXTimeline(timelineMap.size) : new FlipYTimeline(timelineMap.size);
					timeline.boneIndex = boneIndex;

					String field = x ? "x" : "y";
					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child; valueMap != null; valueMap = valueMap.next) {
						timeline.setFrame(frameIndex, valueMap.getFloat("time"), valueMap.getBoolean(field, false));
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() * 2 - 2]);

				} else
					throw new RuntimeException("Invalid timeline type for a bone: " + timelineName + " (" + boneMap.name + ")");
			}
		}

		// IK timelines.
		for (JsonValue ikMap = map.getChild("ik"); ikMap != null; ikMap = ikMap.next) {
			IkConstraintData ikConstraint = skeletonData.findIkConstraint(ikMap.name);
			IkConstraintTimeline timeline = new IkConstraintTimeline(ikMap.size);
			timeline.ikConstraintIndex = skeletonData.getIkConstraints().indexOf(ikConstraint, true);
			int frameIndex = 0;
			for (JsonValue valueMap = ikMap.child; valueMap != null; valueMap = valueMap.next) {
				timeline.setFrame(frameIndex, valueMap.getFloat("time"), valueMap.getFloat("mix"),
					valueMap.getBoolean("bendPositive") ? 1 : -1);
				readCurve(timeline, frameIndex, valueMap);
				frameIndex++;
			}
			timelines.add(timeline);
			duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() * 3 - 3]);
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
					Attachment attachment = skin.getAttachment(slotIndex, meshMap.name);
					if (attachment == null) throw new SerializationException("FFD attachment not found: " + meshMap.name);
					timeline.slotIndex = slotIndex;
					timeline.attachment = attachment;

					int vertexCount;
					if (attachment instanceof MeshAttachment)
						vertexCount = ((MeshAttachment)attachment).getVertices().length;
					else
						vertexCount = ((SkinnedMeshAttachment)attachment).getWeights().length / 3 * 2;

					int frameIndex = 0;
					for (JsonValue valueMap = meshMap.child; valueMap != null; valueMap = valueMap.next) {
						float[] vertices;
						JsonValue verticesValue = valueMap.get("vertices");
						if (verticesValue == null) {
							if (attachment instanceof MeshAttachment)
								vertices = ((MeshAttachment)attachment).getVertices();
							else
								vertices = new float[vertexCount];
						} else {
							vertices = new float[vertexCount];
							int start = valueMap.getInt("offset", 0);
							System.arraycopy(verticesValue.asFloatArray(), 0, vertices, start, verticesValue.size);
							if (scale != 1) {
								for (int i = start, n = i + verticesValue.size; i < n; i++)
									vertices[i] *= scale;
							}
							if (attachment instanceof MeshAttachment) {
								float[] meshVertices = ((MeshAttachment)attachment).getVertices();
								for (int i = 0; i < vertexCount; i++)
									vertices[i] += meshVertices[i];
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
		JsonValue drawOrdersMap = map.get("drawOrder");
		if (drawOrdersMap == null) drawOrdersMap = map.get("draworder");
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
		skeletonData.animations.add(new Animation(name, timelines, duration));
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
