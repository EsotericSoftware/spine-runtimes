/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
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
import com.esotericsoftware.spine.Animation.DeformTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderTimeline;
import com.esotericsoftware.spine.Animation.EventTimeline;
import com.esotericsoftware.spine.Animation.IkConstraintTimeline;
import com.esotericsoftware.spine.Animation.PathConstraintMixTimeline;
import com.esotericsoftware.spine.Animation.PathConstraintPositionTimeline;
import com.esotericsoftware.spine.Animation.PathConstraintSpacingTimeline;
import com.esotericsoftware.spine.Animation.RotateTimeline;
import com.esotericsoftware.spine.Animation.ScaleTimeline;
import com.esotericsoftware.spine.Animation.ShearTimeline;
import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.Animation.TransformConstraintTimeline;
import com.esotericsoftware.spine.Animation.TranslateTimeline;
import com.esotericsoftware.spine.Animation.TwoColorTimeline;
import com.esotericsoftware.spine.BoneData.TransformMode;
import com.esotericsoftware.spine.PathConstraintData.PositionMode;
import com.esotericsoftware.spine.PathConstraintData.RotateMode;
import com.esotericsoftware.spine.PathConstraintData.SpacingMode;
import com.esotericsoftware.spine.attachments.AtlasAttachmentLoader;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.AttachmentLoader;
import com.esotericsoftware.spine.attachments.AttachmentType;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.PathAttachment;
import com.esotericsoftware.spine.attachments.PointAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.VertexAttachment;

/** Loads skeleton data in the Spine JSON format.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-json-format">Spine JSON format</a> and
 * <a href="http://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data">JSON and binary data</a> in the Spine
 * Runtimes Guide. */
public class SkeletonJson {
	private final AttachmentLoader attachmentLoader;
	private float scale = 1;
	private Array<LinkedMesh> linkedMeshes = new Array();

	public SkeletonJson (TextureAtlas atlas) {
		attachmentLoader = new AtlasAttachmentLoader(atlas);
	}

	public SkeletonJson (AttachmentLoader attachmentLoader) {
		if (attachmentLoader == null) throw new IllegalArgumentException("attachmentLoader cannot be null.");
		this.attachmentLoader = attachmentLoader;
	}

	/** Scales bone positions, image sizes, and translations as they are loaded. This allows different size images to be used at
	 * runtime than were used in Spine.
	 * <p>
	 * See <a href="http://esotericsoftware.com/spine-loading-skeleton-data#Scaling">Scaling</a> in the Spine Runtimes Guide. */
	public float getScale () {
		return scale;
	}

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
			skeletonData.fps = skeletonMap.getFloat("fps", 30);
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
			BoneData data = new BoneData(skeletonData.bones.size, boneMap.getString("name"), parent);
			data.length = boneMap.getFloat("length", 0) * scale;
			data.x = boneMap.getFloat("x", 0) * scale;
			data.y = boneMap.getFloat("y", 0) * scale;
			data.rotation = boneMap.getFloat("rotation", 0);
			data.scaleX = boneMap.getFloat("scaleX", 1);
			data.scaleY = boneMap.getFloat("scaleY", 1);
			data.shearX = boneMap.getFloat("shearX", 0);
			data.shearY = boneMap.getFloat("shearY", 0);
			data.transformMode = TransformMode.valueOf(boneMap.getString("transform", TransformMode.normal.name()));

			String color = boneMap.getString("color", null);
			if (color != null) data.getColor().set(Color.valueOf(color));

			skeletonData.bones.add(data);
		}

		// Slots.
		for (JsonValue slotMap = root.getChild("slots"); slotMap != null; slotMap = slotMap.next) {
			String slotName = slotMap.getString("name");
			String boneName = slotMap.getString("bone");
			BoneData boneData = skeletonData.findBone(boneName);
			if (boneData == null) throw new SerializationException("Slot bone not found: " + boneName);
			SlotData data = new SlotData(skeletonData.slots.size, slotName, boneData);

			String color = slotMap.getString("color", null);
			if (color != null) data.getColor().set(Color.valueOf(color));

			String dark = slotMap.getString("dark", null);
			if (dark != null) data.setDarkColor(Color.valueOf(dark));

			data.attachmentName = slotMap.getString("attachment", null);
			data.blendMode = BlendMode.valueOf(slotMap.getString("blend", BlendMode.normal.name()));
			skeletonData.slots.add(data);
		}

		// IK constraints.
		for (JsonValue constraintMap = root.getChild("ik"); constraintMap != null; constraintMap = constraintMap.next) {
			IkConstraintData data = new IkConstraintData(constraintMap.getString("name"));
			data.order = constraintMap.getInt("order", 0);

			for (JsonValue boneMap = constraintMap.getChild("bones"); boneMap != null; boneMap = boneMap.next) {
				String boneName = boneMap.asString();
				BoneData bone = skeletonData.findBone(boneName);
				if (bone == null) throw new SerializationException("IK bone not found: " + boneName);
				data.bones.add(bone);
			}

			String targetName = constraintMap.getString("target");
			data.target = skeletonData.findBone(targetName);
			if (data.target == null) throw new SerializationException("IK target bone not found: " + targetName);

			data.bendDirection = constraintMap.getBoolean("bendPositive", true) ? 1 : -1;
			data.mix = constraintMap.getFloat("mix", 1);

			skeletonData.ikConstraints.add(data);
		}

		// Transform constraints.
		for (JsonValue constraintMap = root.getChild("transform"); constraintMap != null; constraintMap = constraintMap.next) {
			TransformConstraintData data = new TransformConstraintData(constraintMap.getString("name"));
			data.order = constraintMap.getInt("order", 0);

			for (JsonValue boneMap = constraintMap.getChild("bones"); boneMap != null; boneMap = boneMap.next) {
				String boneName = boneMap.asString();
				BoneData bone = skeletonData.findBone(boneName);
				if (bone == null) throw new SerializationException("Transform constraint bone not found: " + boneName);
				data.bones.add(bone);
			}

			String targetName = constraintMap.getString("target");
			data.target = skeletonData.findBone(targetName);
			if (data.target == null) throw new SerializationException("Transform constraint target bone not found: " + targetName);

			data.local = constraintMap.getBoolean("local", false);
			data.relative = constraintMap.getBoolean("relative", false);

			data.offsetRotation = constraintMap.getFloat("rotation", 0);
			data.offsetX = constraintMap.getFloat("x", 0) * scale;
			data.offsetY = constraintMap.getFloat("y", 0) * scale;
			data.offsetScaleX = constraintMap.getFloat("scaleX", 0);
			data.offsetScaleY = constraintMap.getFloat("scaleY", 0);
			data.offsetShearY = constraintMap.getFloat("shearY", 0);

			data.rotateMix = constraintMap.getFloat("rotateMix", 1);
			data.translateMix = constraintMap.getFloat("translateMix", 1);
			data.scaleMix = constraintMap.getFloat("scaleMix", 1);
			data.shearMix = constraintMap.getFloat("shearMix", 1);

			skeletonData.transformConstraints.add(data);
		}

		// Path constraints.
		for (JsonValue constraintMap = root.getChild("path"); constraintMap != null; constraintMap = constraintMap.next) {
			PathConstraintData data = new PathConstraintData(constraintMap.getString("name"));
			data.order = constraintMap.getInt("order", 0);

			for (JsonValue boneMap = constraintMap.getChild("bones"); boneMap != null; boneMap = boneMap.next) {
				String boneName = boneMap.asString();
				BoneData bone = skeletonData.findBone(boneName);
				if (bone == null) throw new SerializationException("Path bone not found: " + boneName);
				data.bones.add(bone);
			}

			String targetName = constraintMap.getString("target");
			data.target = skeletonData.findSlot(targetName);
			if (data.target == null) throw new SerializationException("Path target slot not found: " + targetName);

			data.positionMode = PositionMode.valueOf(constraintMap.getString("positionMode", "percent"));
			data.spacingMode = SpacingMode.valueOf(constraintMap.getString("spacingMode", "length"));
			data.rotateMode = RotateMode.valueOf(constraintMap.getString("rotateMode", "tangent"));
			data.offsetRotation = constraintMap.getFloat("rotation", 0);
			data.position = constraintMap.getFloat("position", 0);
			if (data.positionMode == PositionMode.fixed) data.position *= scale;
			data.spacing = constraintMap.getFloat("spacing", 0);
			if (data.spacingMode == SpacingMode.length || data.spacingMode == SpacingMode.fixed) data.spacing *= scale;
			data.rotateMix = constraintMap.getFloat("rotateMix", 1);
			data.translateMix = constraintMap.getFloat("translateMix", 1);

			skeletonData.pathConstraints.add(data);
		}

		// Skins.
		for (JsonValue skinMap = root.getChild("skins"); skinMap != null; skinMap = skinMap.next) {
			Skin skin = new Skin(skinMap.name);
			for (JsonValue slotEntry = skinMap.child; slotEntry != null; slotEntry = slotEntry.next) {
				SlotData slot = skeletonData.findSlot(slotEntry.name);
				if (slot == null) throw new SerializationException("Slot not found: " + slotEntry.name);
				for (JsonValue entry = slotEntry.child; entry != null; entry = entry.next) {
					try {
						Attachment attachment = readAttachment(entry, skin, slot.index, entry.name, skeletonData);
						if (attachment != null) skin.addAttachment(slot.index, entry.name, attachment);
					} catch (Exception ex) {
						throw new SerializationException("Error reading attachment: " + entry.name + ", skin: " + skin, ex);
					}
				}
			}
			skeletonData.skins.add(skin);
			if (skin.name.equals("default")) skeletonData.defaultSkin = skin;
		}

		// Linked meshes.
		for (int i = 0, n = linkedMeshes.size; i < n; i++) {
			LinkedMesh linkedMesh = linkedMeshes.get(i);
			Skin skin = linkedMesh.skin == null ? skeletonData.getDefaultSkin() : skeletonData.findSkin(linkedMesh.skin);
			if (skin == null) throw new SerializationException("Skin not found: " + linkedMesh.skin);
			Attachment parent = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
			if (parent == null) throw new SerializationException("Parent mesh not found: " + linkedMesh.parent);
			linkedMesh.mesh.setParentMesh((MeshAttachment)parent);
			linkedMesh.mesh.updateUVs();
		}
		linkedMeshes.clear();

		// Events.
		for (JsonValue eventMap = root.getChild("events"); eventMap != null; eventMap = eventMap.next) {
			EventData data = new EventData(eventMap.name);
			data.intValue = eventMap.getInt("int", 0);
			data.floatValue = eventMap.getFloat("float", 0f);
			data.stringValue = eventMap.getString("string", "");
			skeletonData.events.add(data);
		}

		// Animations.
		for (JsonValue animationMap = root.getChild("animations"); animationMap != null; animationMap = animationMap.next) {
			try {
				readAnimation(animationMap, animationMap.name, skeletonData);
			} catch (Exception ex) {
				throw new SerializationException("Error reading animation: " + animationMap.name, ex);
			}
		}

		skeletonData.bones.shrink();
		skeletonData.slots.shrink();
		skeletonData.skins.shrink();
		skeletonData.events.shrink();
		skeletonData.animations.shrink();
		skeletonData.ikConstraints.shrink();
		return skeletonData;
	}

	private Attachment readAttachment (JsonValue map, Skin skin, int slotIndex, String name, SkeletonData skeletonData) {
		float scale = this.scale;
		name = map.getString("name", name);

		String type = map.getString("type", AttachmentType.region.name());

		switch (AttachmentType.valueOf(type)) {
		case region: {
			String path = map.getString("path", name);
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
			readVertices(map, box, map.getInt("vertexCount") << 1);

			String color = map.getString("color", null);
			if (color != null) box.getColor().set(Color.valueOf(color));
			return box;
		}
		case mesh:
		case linkedmesh: {
			String path = map.getString("path", name);
			MeshAttachment mesh = attachmentLoader.newMeshAttachment(skin, name, path);
			if (mesh == null) return null;
			mesh.setPath(path);

			String color = map.getString("color", null);
			if (color != null) mesh.getColor().set(Color.valueOf(color));

			mesh.setWidth(map.getFloat("width", 0) * scale);
			mesh.setHeight(map.getFloat("height", 0) * scale);

			String parent = map.getString("parent", null);
			if (parent != null) {
				mesh.setInheritDeform(map.getBoolean("deform", true));
				linkedMeshes.add(new LinkedMesh(mesh, map.getString("skin", null), slotIndex, parent));
				return mesh;
			}

			float[] uvs = map.require("uvs").asFloatArray();
			readVertices(map, mesh, uvs.length);
			mesh.setTriangles(map.require("triangles").asShortArray());
			mesh.setRegionUVs(uvs);
			mesh.updateUVs();

			if (map.has("hull")) mesh.setHullLength(map.require("hull").asInt() * 2);
			if (map.has("edges")) mesh.setEdges(map.require("edges").asShortArray());
			return mesh;
		}
		case path: {
			PathAttachment path = attachmentLoader.newPathAttachment(skin, name);
			if (path == null) return null;
			path.setClosed(map.getBoolean("closed", false));
			path.setConstantSpeed(map.getBoolean("constantSpeed", true));

			int vertexCount = map.getInt("vertexCount");
			readVertices(map, path, vertexCount << 1);

			float[] lengths = new float[vertexCount / 3];
			int i = 0;
			for (JsonValue curves = map.require("lengths").child; curves != null; curves = curves.next)
				lengths[i++] = curves.asFloat() * scale;
			path.setLengths(lengths);

			String color = map.getString("color", null);
			if (color != null) path.getColor().set(Color.valueOf(color));
			return path;
		}
		case point: {
			PointAttachment point = attachmentLoader.newPointAttachment(skin, name);
			if (point == null) return null;
			point.setX(map.getFloat("x", 0) * scale);
			point.setY(map.getFloat("y", 0) * scale);
			point.setRotation(map.getFloat("rotation", 0));

			String color = map.getString("color", null);
			if (color != null) point.getColor().set(Color.valueOf(color));
			return point;
		}
		case clipping: {
			ClippingAttachment clip = attachmentLoader.newClippingAttachment(skin, name);
			if (clip == null) return null;

			String end = map.getString("end", null);
			if (end != null) {
				SlotData slot = skeletonData.findSlot(end);
				if (slot == null) throw new SerializationException("Clipping end slot not found: " + end);
				clip.setEndSlot(slot);
			}

			readVertices(map, clip, map.getInt("vertexCount") << 1);

			String color = map.getString("color", null);
			if (color != null) clip.getColor().set(Color.valueOf(color));
			return clip;
		}
		}
		return null;
	}

	private void readVertices (JsonValue map, VertexAttachment attachment, int verticesLength) {
		attachment.setWorldVerticesLength(verticesLength);
		float[] vertices = map.require("vertices").asFloatArray();
		if (verticesLength == vertices.length) {
			if (scale != 1) {
				for (int i = 0, n = vertices.length; i < n; i++)
					vertices[i] *= scale;
			}
			attachment.setVertices(vertices);
			return;
		}
		FloatArray weights = new FloatArray(verticesLength * 3 * 3);
		IntArray bones = new IntArray(verticesLength * 3);
		for (int i = 0, n = vertices.length; i < n;) {
			int boneCount = (int)vertices[i++];
			bones.add(boneCount);
			for (int nn = i + boneCount * 4; i < nn; i += 4) {
				bones.add((int)vertices[i]);
				weights.add(vertices[i + 1] * scale);
				weights.add(vertices[i + 2] * scale);
				weights.add(vertices[i + 3]);
			}
		}
		attachment.setBones(bones.toArray());
		attachment.setVertices(weights.toArray());
	}

	private void readAnimation (JsonValue map, String name, SkeletonData skeletonData) {
		float scale = this.scale;
		Array<Timeline> timelines = new Array();
		float duration = 0;

		// Slot timelines.
		for (JsonValue slotMap = map.getChild("slots"); slotMap != null; slotMap = slotMap.next) {
			SlotData slot = skeletonData.findSlot(slotMap.name);
			if (slot == null) throw new SerializationException("Slot not found: " + slotMap.name);
			for (JsonValue timelineMap = slotMap.child; timelineMap != null; timelineMap = timelineMap.next) {
				String timelineName = timelineMap.name;
				if (timelineName.equals("attachment")) {
					AttachmentTimeline timeline = new AttachmentTimeline(timelineMap.size);
					timeline.slotIndex = slot.index;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child; valueMap != null; valueMap = valueMap.next)
						timeline.setFrame(frameIndex++, valueMap.getFloat("time"), valueMap.getString("name"));
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() - 1]);

				} else if (timelineName.equals("color")) {
					ColorTimeline timeline = new ColorTimeline(timelineMap.size);
					timeline.slotIndex = slot.index;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child; valueMap != null; valueMap = valueMap.next) {
						Color color = Color.valueOf(valueMap.getString("color"));
						timeline.setFrame(frameIndex, valueMap.getFloat("time"), color.r, color.g, color.b, color.a);
						readCurve(valueMap, timeline, frameIndex);
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[(timeline.getFrameCount() - 1) * ColorTimeline.ENTRIES]);

				} else if (timelineName.equals("twoColor")) {
					TwoColorTimeline timeline = new TwoColorTimeline(timelineMap.size);
					timeline.slotIndex = slot.index;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child; valueMap != null; valueMap = valueMap.next) {
						Color light = Color.valueOf(valueMap.getString("light"));
						Color dark = Color.valueOf(valueMap.getString("dark"));
						timeline.setFrame(frameIndex, valueMap.getFloat("time"), light.r, light.g, light.b, light.a, dark.r, dark.g,
							dark.b);
						readCurve(valueMap, timeline, frameIndex);
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[(timeline.getFrameCount() - 1) * TwoColorTimeline.ENTRIES]);

				} else
					throw new RuntimeException("Invalid timeline type for a slot: " + timelineName + " (" + slotMap.name + ")");
			}
		}

		// Bone timelines.
		for (JsonValue boneMap = map.getChild("bones"); boneMap != null; boneMap = boneMap.next) {
			BoneData bone = skeletonData.findBone(boneMap.name);
			if (bone == null) throw new SerializationException("Bone not found: " + boneMap.name);
			for (JsonValue timelineMap = boneMap.child; timelineMap != null; timelineMap = timelineMap.next) {
				String timelineName = timelineMap.name;
				if (timelineName.equals("rotate")) {
					RotateTimeline timeline = new RotateTimeline(timelineMap.size);
					timeline.boneIndex = bone.index;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child; valueMap != null; valueMap = valueMap.next) {
						timeline.setFrame(frameIndex, valueMap.getFloat("time"), valueMap.getFloat("angle"));
						readCurve(valueMap, timeline, frameIndex);
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[(timeline.getFrameCount() - 1) * RotateTimeline.ENTRIES]);

				} else if (timelineName.equals("translate") || timelineName.equals("scale") || timelineName.equals("shear")) {
					TranslateTimeline timeline;
					float timelineScale = 1;
					if (timelineName.equals("scale"))
						timeline = new ScaleTimeline(timelineMap.size);
					else if (timelineName.equals("shear"))
						timeline = new ShearTimeline(timelineMap.size);
					else {
						timeline = new TranslateTimeline(timelineMap.size);
						timelineScale = scale;
					}
					timeline.boneIndex = bone.index;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child; valueMap != null; valueMap = valueMap.next) {
						float x = valueMap.getFloat("x", 0), y = valueMap.getFloat("y", 0);
						timeline.setFrame(frameIndex, valueMap.getFloat("time"), x * timelineScale, y * timelineScale);
						readCurve(valueMap, timeline, frameIndex);
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration, timeline.getFrames()[(timeline.getFrameCount() - 1) * TranslateTimeline.ENTRIES]);

				} else
					throw new RuntimeException("Invalid timeline type for a bone: " + timelineName + " (" + boneMap.name + ")");
			}
		}

		// IK constraint timelines.
		for (JsonValue constraintMap = map.getChild("ik"); constraintMap != null; constraintMap = constraintMap.next) {
			IkConstraintData constraint = skeletonData.findIkConstraint(constraintMap.name);
			IkConstraintTimeline timeline = new IkConstraintTimeline(constraintMap.size);
			timeline.ikConstraintIndex = skeletonData.getIkConstraints().indexOf(constraint, true);
			int frameIndex = 0;
			for (JsonValue valueMap = constraintMap.child; valueMap != null; valueMap = valueMap.next) {
				timeline.setFrame(frameIndex, valueMap.getFloat("time"), valueMap.getFloat("mix", 1),
					valueMap.getBoolean("bendPositive", true) ? 1 : -1);
				readCurve(valueMap, timeline, frameIndex);
				frameIndex++;
			}
			timelines.add(timeline);
			duration = Math.max(duration, timeline.getFrames()[(timeline.getFrameCount() - 1) * IkConstraintTimeline.ENTRIES]);
		}

		// Transform constraint timelines.
		for (JsonValue constraintMap = map.getChild("transform"); constraintMap != null; constraintMap = constraintMap.next) {
			TransformConstraintData constraint = skeletonData.findTransformConstraint(constraintMap.name);
			TransformConstraintTimeline timeline = new TransformConstraintTimeline(constraintMap.size);
			timeline.transformConstraintIndex = skeletonData.getTransformConstraints().indexOf(constraint, true);
			int frameIndex = 0;
			for (JsonValue valueMap = constraintMap.child; valueMap != null; valueMap = valueMap.next) {
				timeline.setFrame(frameIndex, valueMap.getFloat("time"), valueMap.getFloat("rotateMix", 1),
					valueMap.getFloat("translateMix", 1), valueMap.getFloat("scaleMix", 1), valueMap.getFloat("shearMix", 1));
				readCurve(valueMap, timeline, frameIndex);
				frameIndex++;
			}
			timelines.add(timeline);
			duration = Math.max(duration,
				timeline.getFrames()[(timeline.getFrameCount() - 1) * TransformConstraintTimeline.ENTRIES]);
		}

		// Path constraint timelines.
		for (JsonValue constraintMap = map.getChild("paths"); constraintMap != null; constraintMap = constraintMap.next) {
			PathConstraintData data = skeletonData.findPathConstraint(constraintMap.name);
			if (data == null) throw new SerializationException("Path constraint not found: " + constraintMap.name);
			int index = skeletonData.pathConstraints.indexOf(data, true);
			for (JsonValue timelineMap = constraintMap.child; timelineMap != null; timelineMap = timelineMap.next) {
				String timelineName = timelineMap.name;
				if (timelineName.equals("position") || timelineName.equals("spacing")) {
					PathConstraintPositionTimeline timeline;
					float timelineScale = 1;
					if (timelineName.equals("spacing")) {
						timeline = new PathConstraintSpacingTimeline(timelineMap.size);
						if (data.spacingMode == SpacingMode.length || data.spacingMode == SpacingMode.fixed) timelineScale = scale;
					} else {
						timeline = new PathConstraintPositionTimeline(timelineMap.size);
						if (data.positionMode == PositionMode.fixed) timelineScale = scale;
					}
					timeline.pathConstraintIndex = index;
					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child; valueMap != null; valueMap = valueMap.next) {
						timeline.setFrame(frameIndex, valueMap.getFloat("time"), valueMap.getFloat(timelineName, 0) * timelineScale);
						readCurve(valueMap, timeline, frameIndex);
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration,
						timeline.getFrames()[(timeline.getFrameCount() - 1) * PathConstraintPositionTimeline.ENTRIES]);
				} else if (timelineName.equals("mix")) {
					PathConstraintMixTimeline timeline = new PathConstraintMixTimeline(timelineMap.size);
					timeline.pathConstraintIndex = index;
					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child; valueMap != null; valueMap = valueMap.next) {
						timeline.setFrame(frameIndex, valueMap.getFloat("time"), valueMap.getFloat("rotateMix", 1),
							valueMap.getFloat("translateMix", 1));
						readCurve(valueMap, timeline, frameIndex);
						frameIndex++;
					}
					timelines.add(timeline);
					duration = Math.max(duration,
						timeline.getFrames()[(timeline.getFrameCount() - 1) * PathConstraintMixTimeline.ENTRIES]);
				}
			}
		}

		// Deform timelines.
		for (JsonValue deformMap = map.getChild("deform"); deformMap != null; deformMap = deformMap.next) {
			Skin skin = skeletonData.findSkin(deformMap.name);
			if (skin == null) throw new SerializationException("Skin not found: " + deformMap.name);
			for (JsonValue slotMap = deformMap.child; slotMap != null; slotMap = slotMap.next) {
				SlotData slot = skeletonData.findSlot(slotMap.name);
				if (slot == null) throw new SerializationException("Slot not found: " + slotMap.name);
				for (JsonValue timelineMap = slotMap.child; timelineMap != null; timelineMap = timelineMap.next) {
					VertexAttachment attachment = (VertexAttachment)skin.getAttachment(slot.index, timelineMap.name);
					if (attachment == null) throw new SerializationException("Deform attachment not found: " + timelineMap.name);
					boolean weighted = attachment.getBones() != null;
					float[] vertices = attachment.getVertices();
					int deformLength = weighted ? vertices.length / 3 * 2 : vertices.length;

					DeformTimeline timeline = new DeformTimeline(timelineMap.size);
					timeline.slotIndex = slot.index;
					timeline.attachment = attachment;

					int frameIndex = 0;
					for (JsonValue valueMap = timelineMap.child; valueMap != null; valueMap = valueMap.next) {
						float[] deform;
						JsonValue verticesValue = valueMap.get("vertices");
						if (verticesValue == null)
							deform = weighted ? new float[deformLength] : vertices;
						else {
							deform = new float[deformLength];
							int start = valueMap.getInt("offset", 0);
							System.arraycopy(verticesValue.asFloatArray(), 0, deform, start, verticesValue.size);
							if (scale != 1) {
								for (int i = start, n = i + verticesValue.size; i < n; i++)
									deform[i] *= scale;
							}
							if (!weighted) {
								for (int i = 0; i < deformLength; i++)
									deform[i] += vertices[i];
							}
						}

						timeline.setFrame(frameIndex, valueMap.getFloat("time"), deform);
						readCurve(valueMap, timeline, frameIndex);
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
						SlotData slot = skeletonData.findSlot(offsetMap.getString("slot"));
						if (slot == null) throw new SerializationException("Slot not found: " + offsetMap.getString("slot"));
						// Collect unchanged items.
						while (originalIndex != slot.index)
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
				Event event = new Event(eventMap.getFloat("time"), eventData);
				event.intValue = eventMap.getInt("int", eventData.getInt());
				event.floatValue = eventMap.getFloat("float", eventData.getFloat());
				event.stringValue = eventMap.getString("string", eventData.getString());
				timeline.setFrame(frameIndex++, event);
			}
			timelines.add(timeline);
			duration = Math.max(duration, timeline.getFrames()[timeline.getFrameCount() - 1]);
		}

		timelines.shrink();
		skeletonData.animations.add(new Animation(name, timelines, duration));
	}

	void readCurve (JsonValue map, CurveTimeline timeline, int frameIndex) {
		JsonValue curve = map.get("curve");
		if (curve == null) return;
		if (curve.isString() && curve.asString().equals("stepped"))
			timeline.setStepped(frameIndex);
		else if (curve.isArray()) {
			timeline.setCurve(frameIndex, curve.getFloat(0), curve.getFloat(1), curve.getFloat(2), curve.getFloat(3));
		}
	}

	static class LinkedMesh {
		String parent, skin;
		int slotIndex;
		MeshAttachment mesh;

		public LinkedMesh (MeshAttachment mesh, String skin, int slotIndex, String parent) {
			this.mesh = mesh;
			this.skin = skin;
			this.slotIndex = slotIndex;
			this.parent = parent;
		}
	}
}
