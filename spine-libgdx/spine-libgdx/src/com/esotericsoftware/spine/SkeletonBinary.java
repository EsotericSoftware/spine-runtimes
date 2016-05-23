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

import java.io.IOException;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.DataInput;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.SerializationException;
import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.ColorTimeline;
import com.esotericsoftware.spine.Animation.CurveTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderTimeline;
import com.esotericsoftware.spine.Animation.EventTimeline;
import com.esotericsoftware.spine.Animation.FfdTimeline;
import com.esotericsoftware.spine.Animation.IkConstraintTimeline;
import com.esotericsoftware.spine.Animation.RotateTimeline;
import com.esotericsoftware.spine.Animation.ScaleTimeline;
import com.esotericsoftware.spine.Animation.ShearTimeline;
import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.Animation.TransformConstraintTimeline;
import com.esotericsoftware.spine.Animation.TranslateTimeline;
import com.esotericsoftware.spine.SkeletonJson.LinkedMesh;
import com.esotericsoftware.spine.attachments.AtlasAttachmentLoader;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.AttachmentLoader;
import com.esotericsoftware.spine.attachments.AttachmentType;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.WeightedMeshAttachment;

public class SkeletonBinary {
	static public final int TIMELINE_ROTATE = 0;
	static public final int TIMELINE_TRANSLATE = 1;
	static public final int TIMELINE_SCALE = 2;
	static public final int TIMELINE_SHEAR = 3;
	static public final int TIMELINE_ATTACHMENT = 4;
	static public final int TIMELINE_COLOR = 5;

	static public final int CURVE_LINEAR = 0;
	static public final int CURVE_STEPPED = 1;
	static public final int CURVE_BEZIER = 2;

	static private final Color tempColor = new Color();

	private final AttachmentLoader attachmentLoader;
	private float scale = 1;
	private Array<LinkedMesh> linkedMeshes = new Array();

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

		float scale = this.scale;

		SkeletonData skeletonData = new SkeletonData();
		skeletonData.name = file.nameWithoutExtension();

		DataInput input = new DataInput(file.read(512)) {
			private char[] chars = new char[32];

			public String readString () throws IOException {
				int byteCount = readInt(true);
				switch (byteCount) {
				case 0:
					return null;
				case 1:
					return "";
				}
				byteCount--;
				if (chars.length < byteCount) chars = new char[byteCount];
				char[] chars = this.chars;
				int charCount = 0;
				for (int i = 0; i < byteCount;) {
					int b = read();
					switch (b >> 4) {
					case 12:
					case 13:
						chars[charCount++] = (char)((b & 0x1F) << 6 | read() & 0x3F);
						i += 2;
						break;
					case 14:
						chars[charCount++] = (char)((b & 0x0F) << 12 | (read() & 0x3F) << 6 | read() & 0x3F);
						i += 3;
						break;
					default:
						chars[charCount++] = (char)b;
						i++;
					}
				}
				return new String(chars, 0, charCount);
			}
		};
		try {
			skeletonData.hash = input.readString();
			if (skeletonData.hash.isEmpty()) skeletonData.hash = null;
			skeletonData.version = input.readString();
			if (skeletonData.version.isEmpty()) skeletonData.version = null;
			skeletonData.width = input.readFloat();
			skeletonData.height = input.readFloat();

			boolean nonessential = input.readBoolean();

			if (nonessential) {
				skeletonData.imagesPath = input.readString();
				if (skeletonData.imagesPath.isEmpty()) skeletonData.imagesPath = null;
			}

			// Bones.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				String name = input.readString();
				BoneData parent = i == 0 ? null : skeletonData.bones.get(input.readInt(true));
				BoneData boneData = new BoneData(name, parent);
				boneData.rotation = input.readFloat();
				boneData.x = input.readFloat() * scale;
				boneData.y = input.readFloat() * scale;
				boneData.scaleX = input.readFloat();
				boneData.scaleY = input.readFloat();
				boneData.shearX = input.readFloat();
				boneData.shearY = input.readFloat();
				boneData.length = input.readFloat() * scale;
				boneData.inheritRotation = input.readBoolean();
				boneData.inheritScale = input.readBoolean();
				if (nonessential) Color.rgba8888ToColor(boneData.color, input.readInt());
				skeletonData.bones.add(boneData);
			}

			// IK constraints.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				IkConstraintData ikConstraintData = new IkConstraintData(input.readString());
				for (int ii = 0, nn = input.readInt(true); ii < nn; ii++)
					ikConstraintData.bones.add(skeletonData.bones.get(input.readInt(true)));
				ikConstraintData.target = skeletonData.bones.get(input.readInt(true));
				ikConstraintData.mix = input.readFloat();
				ikConstraintData.bendDirection = input.readByte();
				skeletonData.ikConstraints.add(ikConstraintData);
			}

			// Transform constraints.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				TransformConstraintData transformConstraintData = new TransformConstraintData(input.readString());
				transformConstraintData.bone = skeletonData.bones.get(input.readInt(true));
				transformConstraintData.target = skeletonData.bones.get(input.readInt(true));
				transformConstraintData.offsetRotation = input.readFloat();
				transformConstraintData.offsetX = input.readFloat() * scale;
				transformConstraintData.offsetY = input.readFloat() * scale;
				transformConstraintData.offsetScaleX = input.readFloat();
				transformConstraintData.offsetScaleY = input.readFloat();
				transformConstraintData.offsetShearY = input.readFloat();
				transformConstraintData.rotateMix = input.readFloat();
				transformConstraintData.translateMix = input.readFloat();
				transformConstraintData.scaleMix = input.readFloat();
				transformConstraintData.shearMix = input.readFloat();
				skeletonData.transformConstraints.add(transformConstraintData);
			}

			// Slots.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				String slotName = input.readString();
				BoneData boneData = skeletonData.bones.get(input.readInt(true));
				SlotData slotData = new SlotData(slotName, boneData);
				Color.rgba8888ToColor(slotData.color, input.readInt());
				slotData.attachmentName = input.readString();
				slotData.blendMode = BlendMode.values[input.readInt(true)];
				skeletonData.slots.add(slotData);
			}

			// Default skin.
			Skin defaultSkin = readSkin(input, "default", nonessential);
			if (defaultSkin != null) {
				skeletonData.defaultSkin = defaultSkin;
				skeletonData.skins.add(defaultSkin);
			}

			// Skins.
			for (int i = 0, n = input.readInt(true); i < n; i++)
				skeletonData.skins.add(readSkin(input, input.readString(), nonessential));

			// Linked meshes.
			for (int i = 0, n = linkedMeshes.size; i < n; i++) {
				LinkedMesh linkedMesh = linkedMeshes.get(i);
				Skin skin = linkedMesh.skin == null ? skeletonData.getDefaultSkin() : skeletonData.findSkin(linkedMesh.skin);
				if (skin == null) throw new SerializationException("Skin not found: " + linkedMesh.skin);
				Attachment parent = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (parent == null) throw new SerializationException("Parent mesh not found: " + linkedMesh.parent);
				if (linkedMesh.mesh instanceof MeshAttachment) {
					MeshAttachment mesh = (MeshAttachment)linkedMesh.mesh;
					mesh.setParentMesh((MeshAttachment)parent);
					mesh.updateUVs();
				} else {
					WeightedMeshAttachment mesh = (WeightedMeshAttachment)linkedMesh.mesh;
					mesh.setParentMesh((WeightedMeshAttachment)parent);
					mesh.updateUVs();
				}
			}
			linkedMeshes.clear();

			// Events.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				EventData eventData = new EventData(input.readString());
				eventData.intValue = input.readInt(false);
				eventData.floatValue = input.readFloat();
				eventData.stringValue = input.readString();
				skeletonData.events.add(eventData);
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
		skeletonData.events.shrink();
		skeletonData.animations.shrink();
		skeletonData.ikConstraints.shrink();
		return skeletonData;
	}

	/** @return May be null. */
	private Skin readSkin (DataInput input, String skinName, boolean nonessential) throws IOException {
		int slotCount = input.readInt(true);
		if (slotCount == 0) return null;
		Skin skin = new Skin(skinName);
		for (int i = 0; i < slotCount; i++) {
			int slotIndex = input.readInt(true);
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				String name = input.readString();
				skin.addAttachment(slotIndex, name, readAttachment(input, skin, slotIndex, name, nonessential));
			}
		}
		return skin;
	}

	private Attachment readAttachment (DataInput input, Skin skin, int slotIndex, String attachmentName, boolean nonessential)
		throws IOException {
		float scale = this.scale;

		String name = input.readString();
		if (name == null) name = attachmentName;

		AttachmentType type = AttachmentType.values[input.readByte()];
		switch (type) {
		case region: {
			String path = input.readString();
			float rotation = input.readFloat();
			float x = input.readFloat();
			float y = input.readFloat();
			float scaleX = input.readFloat();
			float scaleY = input.readFloat();
			float width = input.readFloat();
			float height = input.readFloat();
			int color = input.readInt();

			if (path == null) path = name;
			RegionAttachment region = attachmentLoader.newRegionAttachment(skin, name, path);
			if (region == null) return null;
			region.setPath(path);
			region.setX(x * scale);
			region.setY(y * scale);
			region.setScaleX(scaleX);
			region.setScaleY(scaleY);
			region.setRotation(rotation);
			region.setWidth(width * scale);
			region.setHeight(height * scale);
			Color.rgba8888ToColor(region.getColor(), color);
			region.updateOffset();
			return region;
		}
		case boundingbox: {
			float[] vertices = readFloatArray(input, input.readInt(true) * 2, scale);
			BoundingBoxAttachment box = attachmentLoader.newBoundingBoxAttachment(skin, name);
			if (box == null) return null;
			box.setVertices(vertices);
			return box;
		}
		case mesh: {
			String path = input.readString();
			int color = input.readInt();
			int hullLength = 0;
			int verticesLength = input.readInt(true) * 2;
			float[] uvs = readFloatArray(input, verticesLength, 1);
			short[] triangles = readShortArray(input);
			float[] vertices = readFloatArray(input, verticesLength, scale);
			hullLength = input.readInt(true);
			short[] edges = null;
			float width = 0, height = 0;
			if (nonessential) {
				edges = readShortArray(input);
				width = input.readFloat();
				height = input.readFloat();
			}

			if (path == null) path = name;
			MeshAttachment mesh = attachmentLoader.newMeshAttachment(skin, name, path);
			if (mesh == null) return null;
			mesh.setPath(path);
			Color.rgba8888ToColor(mesh.getColor(), color);
			mesh.setVertices(vertices);
			mesh.setTriangles(triangles);
			mesh.setRegionUVs(uvs);
			mesh.updateUVs();
			mesh.setHullLength(hullLength * 2);
			if (nonessential) {
				mesh.setEdges(edges);
				mesh.setWidth(width * scale);
				mesh.setHeight(height * scale);
			}
			return mesh;
		}
		case linkedmesh: {
			String path = input.readString();
			int color = input.readInt();
			String skinName = input.readString();
			String parent = input.readString();
			boolean inheritFFD = input.readBoolean();
			float width = 0, height = 0;
			if (nonessential) {
				width = input.readFloat();
				height = input.readFloat();
			}

			if (path == null) path = name;
			MeshAttachment mesh = attachmentLoader.newMeshAttachment(skin, name, path);
			if (mesh == null) return null;
			mesh.setPath(path);
			Color.rgba8888ToColor(mesh.getColor(), color);
			mesh.setInheritFFD(inheritFFD);
			if (nonessential) {
				mesh.setWidth(width * scale);
				mesh.setHeight(height * scale);
			}
			linkedMeshes.add(new LinkedMesh(mesh, skinName, slotIndex, parent));
			return mesh;
		}
		case weightedmesh: {
			String path = input.readString();
			int color = input.readInt();
			int vertexCount = input.readInt(true);
			float[] uvs = readFloatArray(input, vertexCount * 2, 1);
			short[] triangles = readShortArray(input);
			FloatArray weights = new FloatArray(uvs.length * 3 * 3);
			IntArray bones = new IntArray(uvs.length * 3);
			for (int i = 0; i < vertexCount; i++) {
				int boneCount = (int)input.readFloat();
				bones.add(boneCount);
				for (int ii = 0; ii < boneCount; ii++) {
					bones.add((int)input.readFloat());
					weights.add(input.readFloat() * scale);
					weights.add(input.readFloat() * scale);
					weights.add(input.readFloat());
				}
			}
			int hullLength = input.readInt(true);
			short[] edges = null;
			float width = 0, height = 0;
			if (nonessential) {
				edges = readShortArray(input);
				width = input.readFloat();
				height = input.readFloat();
			}

			if (path == null) path = name;
			WeightedMeshAttachment mesh = attachmentLoader.newWeightedMeshAttachment(skin, name, path);
			if (mesh == null) return null;
			mesh.setPath(path);
			Color.rgba8888ToColor(mesh.getColor(), color);
			mesh.setBones(bones.toArray());
			mesh.setWeights(weights.toArray());
			mesh.setTriangles(triangles);
			mesh.setRegionUVs(uvs);
			mesh.updateUVs();
			mesh.setHullLength(hullLength * 2);
			if (nonessential) {
				mesh.setEdges(edges);
				mesh.setWidth(width * scale);
				mesh.setHeight(height * scale);
			}
			return mesh;
		}
		case weightedlinkedmesh: {
			String path = input.readString();
			int color = input.readInt();
			String skinName = input.readString();
			String parent = input.readString();
			boolean inheritFFD = input.readBoolean();
			float width = 0, height = 0;
			if (nonessential) {
				width = input.readFloat();
				height = input.readFloat();
			}

			if (path == null) path = name;
			WeightedMeshAttachment mesh = attachmentLoader.newWeightedMeshAttachment(skin, name, path);
			if (mesh == null) return null;
			mesh.setPath(path);
			Color.rgba8888ToColor(mesh.getColor(), color);
			mesh.setInheritFFD(inheritFFD);
			if (nonessential) {
				mesh.setWidth(width * scale);
				mesh.setHeight(height * scale);
			}
			linkedMeshes.add(new LinkedMesh(mesh, skinName, slotIndex, parent));
			return mesh;
		}
		}
		return null;
	}

	private float[] readFloatArray (DataInput input, int n, float scale) throws IOException {
		float[] array = new float[n];
		if (scale == 1) {
			for (int i = 0; i < n; i++)
				array[i] = input.readFloat();
		} else {
			for (int i = 0; i < n; i++)
				array[i] = input.readFloat() * scale;
		}
		return array;
	}

	private short[] readShortArray (DataInput input) throws IOException {
		int n = input.readInt(true);
		short[] array = new short[n];
		for (int i = 0; i < n; i++)
			array[i] = input.readShort();
		return array;
	}

	private void readAnimation (String name, DataInput input, SkeletonData skeletonData) {
		Array<Timeline> timelines = new Array();
		float scale = this.scale;
		float duration = 0;

		try {
			// Slot timelines.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				int slotIndex = input.readInt(true);
				for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					int timelineType = input.readByte();
					int frameCount = input.readInt(true);
					switch (timelineType) {
					case TIMELINE_COLOR: {
						ColorTimeline timeline = new ColorTimeline(frameCount);
						timeline.slotIndex = slotIndex;
						for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							float time = input.readFloat();
							Color.rgba8888ToColor(tempColor, input.readInt());
							timeline.setFrame(frameIndex, time, tempColor.r, tempColor.g, tempColor.b, tempColor.a);
							if (frameIndex < frameCount - 1) readCurve(input, frameIndex, timeline);
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[frameCount * 5 - 5]);
						break;
					}
					case TIMELINE_ATTACHMENT:
						AttachmentTimeline timeline = new AttachmentTimeline(frameCount);
						timeline.slotIndex = slotIndex;
						for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
							timeline.setFrame(frameIndex, input.readFloat(), input.readString());
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[frameCount - 1]);
						break;
					}
				}
			}

			// Bone timelines.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				int boneIndex = input.readInt(true);
				for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					int timelineType = input.readByte();
					int frameCount = input.readInt(true);
					switch (timelineType) {
					case TIMELINE_ROTATE: {
						RotateTimeline timeline = new RotateTimeline(frameCount);
						timeline.boneIndex = boneIndex;
						for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							timeline.setFrame(frameIndex, input.readFloat(), input.readFloat());
							if (frameIndex < frameCount - 1) readCurve(input, frameIndex, timeline);
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[frameCount * 2 - 2]);
						break;
					}
					case TIMELINE_TRANSLATE:
					case TIMELINE_SCALE:
					case TIMELINE_SHEAR: {
						TranslateTimeline timeline;
						float timelineScale = 1;
						if (timelineType == TIMELINE_SCALE)
							timeline = new ScaleTimeline(frameCount);
						else if (timelineType == TIMELINE_SHEAR)
							timeline = new ShearTimeline(frameCount);
						else {
							timeline = new TranslateTimeline(frameCount);
							timelineScale = scale;
						}
						timeline.boneIndex = boneIndex;
						for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							timeline.setFrame(frameIndex, input.readFloat(), input.readFloat() * timelineScale,
								input.readFloat() * timelineScale);
							if (frameIndex < frameCount - 1) readCurve(input, frameIndex, timeline);
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[frameCount * 3 - 3]);
						break;
					}
					}
				}
			}

			// IK constraint timelines.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				IkConstraintData constraint = skeletonData.ikConstraints.get(input.readInt(true));
				int frameCount = input.readInt(true);
				IkConstraintTimeline timeline = new IkConstraintTimeline(frameCount);
				timeline.ikConstraintIndex = skeletonData.getIkConstraints().indexOf(constraint, true);
				for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
					timeline.setFrame(frameIndex, input.readFloat(), input.readFloat(), input.readByte());
					if (frameIndex < frameCount - 1) readCurve(input, frameIndex, timeline);
				}
				timelines.add(timeline);
				duration = Math.max(duration, timeline.getFrames()[frameCount * 3 - 3]);
			}

			// Transform constraint timelines.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				TransformConstraintData constraint = skeletonData.transformConstraints.get(input.readInt(true));
				int frameCount = input.readInt(true);
				TransformConstraintTimeline timeline = new TransformConstraintTimeline(frameCount);
				timeline.transformConstraintIndex = skeletonData.getTransformConstraints().indexOf(constraint, true);
				for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
					timeline.setFrame(frameIndex, input.readFloat(), input.readFloat(), input.readFloat(), input.readFloat(),
						input.readFloat());
					if (frameIndex < frameCount - 1) readCurve(input, frameIndex, timeline);
				}
				timelines.add(timeline);
				duration = Math.max(duration, timeline.getFrames()[frameCount * 5 - 5]);
			}

			// FFD timelines.
			for (int i = 0, n = input.readInt(true); i < n; i++) {
				Skin skin = skeletonData.skins.get(input.readInt(true));
				for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					int slotIndex = input.readInt(true);
					for (int iii = 0, nnn = input.readInt(true); iii < nnn; iii++) {
						Attachment attachment = skin.getAttachment(slotIndex, input.readString());
						int frameCount = input.readInt(true);
						FfdTimeline timeline = new FfdTimeline(frameCount);
						timeline.slotIndex = slotIndex;
						timeline.attachment = attachment;
						for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							float time = input.readFloat();

							float[] vertices;
							int vertexCount;
							if (attachment instanceof MeshAttachment)
								vertexCount = ((MeshAttachment)attachment).getVertices().length;
							else
								vertexCount = ((WeightedMeshAttachment)attachment).getWeights().length / 3 * 2;

							int end = input.readInt(true);
							if (end == 0) {
								if (attachment instanceof MeshAttachment)
									vertices = ((MeshAttachment)attachment).getVertices();
								else
									vertices = new float[vertexCount];
							} else {
								vertices = new float[vertexCount];
								int start = input.readInt(true);
								end += start;
								if (scale == 1) {
									for (int v = start; v < end; v++)
										vertices[v] = input.readFloat();
								} else {
									for (int v = start; v < end; v++)
										vertices[v] = input.readFloat() * scale;
								}
								if (attachment instanceof MeshAttachment) {
									float[] meshVertices = ((MeshAttachment)attachment).getVertices();
									for (int v = 0, vn = vertices.length; v < vn; v++)
										vertices[v] += meshVertices[v];
								}
							}

							timeline.setFrame(frameIndex, time, vertices);
							if (frameIndex < frameCount - 1) readCurve(input, frameIndex, timeline);
						}
						timelines.add(timeline);
						duration = Math.max(duration, timeline.getFrames()[frameCount - 1]);
					}
				}
			}

			// Draw order timeline.
			int drawOrderCount = input.readInt(true);
			if (drawOrderCount > 0) {
				DrawOrderTimeline timeline = new DrawOrderTimeline(drawOrderCount);
				int slotCount = skeletonData.slots.size;
				for (int i = 0; i < drawOrderCount; i++) {
					float time = input.readFloat();
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
					timeline.setFrame(i, time, drawOrder);
				}
				timelines.add(timeline);
				duration = Math.max(duration, timeline.getFrames()[drawOrderCount - 1]);
			}

			// Event timeline.
			int eventCount = input.readInt(true);
			if (eventCount > 0) {
				EventTimeline timeline = new EventTimeline(eventCount);
				for (int i = 0; i < eventCount; i++) {
					float time = input.readFloat();
					EventData eventData = skeletonData.events.get(input.readInt(true));
					Event event = new Event(time, eventData);
					event.intValue = input.readInt(false);
					event.floatValue = input.readFloat();
					event.stringValue = input.readBoolean() ? input.readString() : eventData.stringValue;
					timeline.setFrame(i, event);
				}
				timelines.add(timeline);
				duration = Math.max(duration, timeline.getFrames()[eventCount - 1]);
			}
		} catch (IOException ex) {
			throw new SerializationException("Error reading skeleton file.", ex);
		}

		timelines.shrink();
		skeletonData.animations.add(new Animation(name, timelines, duration));
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
