/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import java.io.EOFException;
import java.io.IOException;
import java.io.InputStream;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.DataInput;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.Null;
import com.badlogic.gdx.utils.SerializationException;

import com.esotericsoftware.spine.Animation.AlphaTimeline;
import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.CurveTimeline;
import com.esotericsoftware.spine.Animation.CurveTimeline1;
import com.esotericsoftware.spine.Animation.CurveTimeline2;
import com.esotericsoftware.spine.Animation.DeformTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderTimeline;
import com.esotericsoftware.spine.Animation.EventTimeline;
import com.esotericsoftware.spine.Animation.IkConstraintTimeline;
import com.esotericsoftware.spine.Animation.InheritTimeline;
import com.esotericsoftware.spine.Animation.PathConstraintMixTimeline;
import com.esotericsoftware.spine.Animation.PathConstraintPositionTimeline;
import com.esotericsoftware.spine.Animation.PathConstraintSpacingTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintDampingTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintGravityTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintInertiaTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintMassTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintMixTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintResetTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintStrengthTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintWindTimeline;
import com.esotericsoftware.spine.Animation.RGB2Timeline;
import com.esotericsoftware.spine.Animation.RGBA2Timeline;
import com.esotericsoftware.spine.Animation.RGBATimeline;
import com.esotericsoftware.spine.Animation.RGBTimeline;
import com.esotericsoftware.spine.Animation.RotateTimeline;
import com.esotericsoftware.spine.Animation.ScaleTimeline;
import com.esotericsoftware.spine.Animation.ScaleXTimeline;
import com.esotericsoftware.spine.Animation.ScaleYTimeline;
import com.esotericsoftware.spine.Animation.SequenceTimeline;
import com.esotericsoftware.spine.Animation.ShearTimeline;
import com.esotericsoftware.spine.Animation.ShearXTimeline;
import com.esotericsoftware.spine.Animation.ShearYTimeline;
import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.Animation.TransformConstraintTimeline;
import com.esotericsoftware.spine.Animation.TranslateTimeline;
import com.esotericsoftware.spine.Animation.TranslateXTimeline;
import com.esotericsoftware.spine.Animation.TranslateYTimeline;
import com.esotericsoftware.spine.BoneData.Inherit;
import com.esotericsoftware.spine.PathConstraintData.PositionMode;
import com.esotericsoftware.spine.PathConstraintData.RotateMode;
import com.esotericsoftware.spine.PathConstraintData.SpacingMode;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.AttachmentLoader;
import com.esotericsoftware.spine.attachments.AttachmentType;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.PathAttachment;
import com.esotericsoftware.spine.attachments.PointAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.Sequence;
import com.esotericsoftware.spine.attachments.Sequence.SequenceMode;
import com.esotericsoftware.spine.attachments.VertexAttachment;

/** Loads skeleton data in the Spine binary format.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-binary-format">Spine binary format</a> and
 * <a href="http://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data">JSON and binary data</a> in the Spine
 * Runtimes Guide. */
public class SkeletonBinary extends SkeletonLoader {
	static public final int BONE_ROTATE = 0;
	static public final int BONE_TRANSLATE = 1;
	static public final int BONE_TRANSLATEX = 2;
	static public final int BONE_TRANSLATEY = 3;
	static public final int BONE_SCALE = 4;
	static public final int BONE_SCALEX = 5;
	static public final int BONE_SCALEY = 6;
	static public final int BONE_SHEAR = 7;
	static public final int BONE_SHEARX = 8;
	static public final int BONE_SHEARY = 9;
	static public final int BONE_INHERIT = 10;

	static public final int SLOT_ATTACHMENT = 0;
	static public final int SLOT_RGBA = 1;
	static public final int SLOT_RGB = 2;
	static public final int SLOT_RGBA2 = 3;
	static public final int SLOT_RGB2 = 4;
	static public final int SLOT_ALPHA = 5;

	static public final int ATTACHMENT_DEFORM = 0;
	static public final int ATTACHMENT_SEQUENCE = 1;

	static public final int PATH_POSITION = 0;
	static public final int PATH_SPACING = 1;
	static public final int PATH_MIX = 2;

	static public final int PHYSICS_INERTIA = 0;
	static public final int PHYSICS_STRENGTH = 1;
	static public final int PHYSICS_DAMPING = 2;
	static public final int PHYSICS_MASS = 4;
	static public final int PHYSICS_WIND = 5;
	static public final int PHYSICS_GRAVITY = 6;
	static public final int PHYSICS_MIX = 7;
	static public final int PHYSICS_RESET = 8;

	static public final int CURVE_LINEAR = 0;
	static public final int CURVE_STEPPED = 1;
	static public final int CURVE_BEZIER = 2;

	private final Array<LinkedMesh> linkedMeshes = new Array();

	public SkeletonBinary (AttachmentLoader attachmentLoader) {
		super(attachmentLoader);
	}

	public SkeletonBinary (TextureAtlas atlas) {
		super(atlas);
	}

	public SkeletonData readSkeletonData (FileHandle file) {
		if (file == null) throw new IllegalArgumentException("file cannot be null.");
		SkeletonData skeletonData = readSkeletonData(file.read());
		skeletonData.name = file.nameWithoutExtension();
		return skeletonData;
	}

	public SkeletonData readSkeletonData (InputStream dataInput) {
		if (dataInput == null) throw new IllegalArgumentException("dataInput cannot be null.");

		float scale = this.scale;

		SkeletonInput input = new SkeletonInput(dataInput);
		SkeletonData skeletonData = new SkeletonData();
		try {
			long hash = input.readLong();
			skeletonData.hash = hash == 0 ? null : Long.toString(hash);
			skeletonData.version = input.readString();
			if (skeletonData.version.isEmpty()) skeletonData.version = null;
			skeletonData.x = input.readFloat();
			skeletonData.y = input.readFloat();
			skeletonData.width = input.readFloat();
			skeletonData.height = input.readFloat();
			skeletonData.referenceScale = input.readFloat() * scale;

			boolean nonessential = input.readBoolean();
			if (nonessential) {
				skeletonData.fps = input.readFloat();

				skeletonData.imagesPath = input.readString();
				if (skeletonData.imagesPath.isEmpty()) skeletonData.imagesPath = null;

				skeletonData.audioPath = input.readString();
				if (skeletonData.audioPath.isEmpty()) skeletonData.audioPath = null;
			}

			int n;
			Object[] o;

			// Strings.
			o = input.strings = new String[n = input.readInt(true)];
			for (int i = 0; i < n; i++)
				o[i] = input.readString();

			// Bones.
			Object[] bones = skeletonData.bones.setSize(n = input.readInt(true));
			for (int i = 0; i < n; i++) {
				String name = input.readString();
				BoneData parent = i == 0 ? null : (BoneData)bones[input.readInt(true)];
				BoneData data = new BoneData(i, name, parent);
				data.rotation = input.readFloat();
				data.x = input.readFloat() * scale;
				data.y = input.readFloat() * scale;
				data.scaleX = input.readFloat();
				data.scaleY = input.readFloat();
				data.shearX = input.readFloat();
				data.shearY = input.readFloat();
				data.length = input.readFloat() * scale;
				data.inherit = Inherit.values[input.readByte()];
				data.skinRequired = input.readBoolean();
				if (nonessential) {
					Color.rgba8888ToColor(data.color, input.readInt());
					data.icon = input.readString();
					data.visible = input.readBoolean();
				}
				bones[i] = data;
			}

			// Slots.
			Object[] slots = skeletonData.slots.setSize(n = input.readInt(true));
			for (int i = 0; i < n; i++) {
				String slotName = input.readString();
				BoneData boneData = (BoneData)bones[input.readInt(true)];
				SlotData data = new SlotData(i, slotName, boneData);
				Color.rgba8888ToColor(data.color, input.readInt());

				int darkColor = input.readInt();
				if (darkColor != -1) Color.rgb888ToColor(data.darkColor = new Color(), darkColor);

				data.attachmentName = input.readStringRef();
				data.blendMode = BlendMode.values[input.readInt(true)];
				if (nonessential) data.visible = input.readBoolean();
				slots[i] = data;
			}

			// IK constraints.
			o = skeletonData.ikConstraints.setSize(n = input.readInt(true));
			for (int i = 0, nn; i < n; i++) {
				IkConstraintData data = new IkConstraintData(input.readString());
				data.order = input.readInt(true);
				Object[] constraintBones = data.bones.setSize(nn = input.readInt(true));
				for (int ii = 0; ii < nn; ii++)
					constraintBones[ii] = bones[input.readInt(true)];
				data.target = (BoneData)bones[input.readInt(true)];
				int flags = input.read();
				data.skinRequired = (flags & 1) != 0;
				data.bendDirection = (flags & 2) != 0 ? 1 : -1;
				data.compress = (flags & 4) != 0;
				data.stretch = (flags & 8) != 0;
				data.uniform = (flags & 16) != 0;
				if ((flags & 32) != 0) data.mix = (flags & 64) != 0 ? input.readFloat() : 1;
				if ((flags & 128) != 0) data.softness = input.readFloat() * scale;
				o[i] = data;
			}

			// Transform constraints.
			o = skeletonData.transformConstraints.setSize(n = input.readInt(true));
			for (int i = 0, nn; i < n; i++) {
				TransformConstraintData data = new TransformConstraintData(input.readString());
				data.order = input.readInt(true);
				Object[] constraintBones = data.bones.setSize(nn = input.readInt(true));
				for (int ii = 0; ii < nn; ii++)
					constraintBones[ii] = bones[input.readInt(true)];
				data.target = (BoneData)bones[input.readInt(true)];
				int flags = input.read();
				data.skinRequired = (flags & 1) != 0;
				data.local = (flags & 2) != 0;
				data.relative = (flags & 4) != 0;
				if ((flags & 8) != 0) data.offsetRotation = input.readFloat();
				if ((flags & 16) != 0) data.offsetX = input.readFloat() * scale;
				if ((flags & 32) != 0) data.offsetY = input.readFloat() * scale;
				if ((flags & 64) != 0) data.offsetScaleX = input.readFloat();
				if ((flags & 128) != 0) data.offsetScaleY = input.readFloat();
				flags = input.read();
				if ((flags & 1) != 0) data.offsetShearY = input.readFloat();
				if ((flags & 2) != 0) data.mixRotate = input.readFloat();
				if ((flags & 4) != 0) data.mixX = input.readFloat();
				if ((flags & 8) != 0) data.mixY = input.readFloat();
				if ((flags & 16) != 0) data.mixScaleX = input.readFloat();
				if ((flags & 32) != 0) data.mixScaleY = input.readFloat();
				if ((flags & 64) != 0) data.mixShearY = input.readFloat();
				o[i] = data;
			}

			// Path constraints.
			o = skeletonData.pathConstraints.setSize(n = input.readInt(true));
			for (int i = 0, nn; i < n; i++) {
				PathConstraintData data = new PathConstraintData(input.readString());
				data.order = input.readInt(true);
				data.skinRequired = input.readBoolean();
				Object[] constraintBones = data.bones.setSize(nn = input.readInt(true));
				for (int ii = 0; ii < nn; ii++)
					constraintBones[ii] = bones[input.readInt(true)];
				data.target = (SlotData)slots[input.readInt(true)];
				int flags = input.read();
				data.positionMode = PositionMode.values[flags & 1];
				data.spacingMode = SpacingMode.values[(flags >> 1) & 3];
				data.rotateMode = RotateMode.values[(flags >> 3) & 3];
				if ((flags & 128) != 0) data.offsetRotation = input.readFloat();
				data.position = input.readFloat();
				if (data.positionMode == PositionMode.fixed) data.position *= scale;
				data.spacing = input.readFloat();
				if (data.spacingMode == SpacingMode.length || data.spacingMode == SpacingMode.fixed) data.spacing *= scale;
				data.mixRotate = input.readFloat();
				data.mixX = input.readFloat();
				data.mixY = input.readFloat();
				o[i] = data;
			}

			// Physics constraints.
			o = skeletonData.physicsConstraints.setSize(n = input.readInt(true));
			for (int i = 0; i < n; i++) {
				PhysicsConstraintData data = new PhysicsConstraintData(input.readString());
				data.order = input.readInt(true);
				data.bone = (BoneData)bones[input.readInt(true)];
				int flags = input.read();
				data.skinRequired = (flags & 1) != 0;
				if ((flags & 2) != 0) data.x = input.readFloat();
				if ((flags & 4) != 0) data.y = input.readFloat();
				if ((flags & 8) != 0) data.rotate = input.readFloat();
				if ((flags & 16) != 0) data.scaleX = input.readFloat();
				if ((flags & 32) != 0) data.shearX = input.readFloat();
				data.limit = ((flags & 64) != 0 ? input.readFloat() : 5000) * scale;
				data.step = 1f / input.readUnsignedByte();
				data.inertia = input.readFloat();
				data.strength = input.readFloat();
				data.damping = input.readFloat();
				data.massInverse = (flags & 128) != 0 ? input.readFloat() : 1;
				data.wind = input.readFloat();
				data.gravity = input.readFloat();
				flags = input.read();
				if ((flags & 1) != 0) data.inertiaGlobal = true;
				if ((flags & 2) != 0) data.strengthGlobal = true;
				if ((flags & 4) != 0) data.dampingGlobal = true;
				if ((flags & 8) != 0) data.massGlobal = true;
				if ((flags & 16) != 0) data.windGlobal = true;
				if ((flags & 32) != 0) data.gravityGlobal = true;
				if ((flags & 64) != 0) data.mixGlobal = true;
				data.mix = (flags & 128) != 0 ? input.readFloat() : 1;
				o[i] = data;
			}

			// Default skin.
			Skin defaultSkin = readSkin(input, skeletonData, true, nonessential);
			if (defaultSkin != null) {
				skeletonData.defaultSkin = defaultSkin;
				skeletonData.skins.add(defaultSkin);
			}

			// Skins.
			{
				int i = skeletonData.skins.size;
				o = skeletonData.skins.setSize(n = i + input.readInt(true));
				for (; i < n; i++)
					o[i] = readSkin(input, skeletonData, false, nonessential);
			}

			// Linked meshes.
			n = linkedMeshes.size;
			Object[] items = linkedMeshes.items;
			for (int i = 0; i < n; i++) {
				LinkedMesh linkedMesh = (LinkedMesh)items[i];
				Skin skin = skeletonData.skins.get(linkedMesh.skinIndex);
				Attachment parent = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (parent == null) throw new SerializationException("Parent mesh not found: " + linkedMesh.parent);
				linkedMesh.mesh.setTimelineAttachment(linkedMesh.inheritTimelines ? (VertexAttachment)parent : linkedMesh.mesh);
				linkedMesh.mesh.setParentMesh((MeshAttachment)parent);
				if (linkedMesh.mesh.getSequence() == null) linkedMesh.mesh.updateRegion();
			}
			linkedMeshes.clear();

			// Events.
			o = skeletonData.events.setSize(n = input.readInt(true));
			for (int i = 0; i < n; i++) {
				EventData data = new EventData(input.readString());
				data.intValue = input.readInt(false);
				data.floatValue = input.readFloat();
				data.stringValue = input.readString();
				data.audioPath = input.readString();
				if (data.audioPath != null) {
					data.volume = input.readFloat();
					data.balance = input.readFloat();
				}
				o[i] = data;
			}

			// Animations.
			o = skeletonData.animations.setSize(n = input.readInt(true));
			for (int i = 0; i < n; i++)
				o[i] = readAnimation(input, input.readString(), skeletonData);

		} catch (IOException ex) {
			throw new SerializationException("Error reading skeleton file.", ex);
		} finally {
			try {
				input.close();
			} catch (IOException ignored) {
			}
		}
		return skeletonData;
	}

	private @Null Skin readSkin (SkeletonInput input, SkeletonData skeletonData, boolean defaultSkin, boolean nonessential)
		throws IOException {

		Skin skin;
		int slotCount;
		if (defaultSkin) {
			slotCount = input.readInt(true);
			if (slotCount == 0) return null;
			skin = new Skin("default");
		} else {
			skin = new Skin(input.readString());

			if (nonessential) Color.rgba8888ToColor(skin.color, input.readInt());

			Object[] bones = skin.bones.setSize(input.readInt(true)), items = skeletonData.bones.items;
			for (int i = 0, n = skin.bones.size; i < n; i++)
				bones[i] = items[input.readInt(true)];

			items = skeletonData.ikConstraints.items;
			for (int i = 0, n = input.readInt(true); i < n; i++)
				skin.constraints.add((ConstraintData)items[input.readInt(true)]);
			items = skeletonData.transformConstraints.items;
			for (int i = 0, n = input.readInt(true); i < n; i++)
				skin.constraints.add((ConstraintData)items[input.readInt(true)]);
			items = skeletonData.pathConstraints.items;
			for (int i = 0, n = input.readInt(true); i < n; i++)
				skin.constraints.add((ConstraintData)items[input.readInt(true)]);
			items = skeletonData.physicsConstraints.items;
			for (int i = 0, n = input.readInt(true); i < n; i++)
				skin.constraints.add((ConstraintData)items[input.readInt(true)]);
			skin.constraints.shrink();

			slotCount = input.readInt(true);
		}

		for (int i = 0; i < slotCount; i++) {
			int slotIndex = input.readInt(true);
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				String name = input.readStringRef();
				Attachment attachment = readAttachment(input, skeletonData, skin, slotIndex, name, nonessential);
				if (attachment != null) skin.setAttachment(slotIndex, name, attachment);
			}
		}
		return skin;
	}

	private Attachment readAttachment (SkeletonInput input, SkeletonData skeletonData, Skin skin, int slotIndex,
		String attachmentName, boolean nonessential) throws IOException {
		float scale = this.scale;

		int flags = input.readByte();
		String name = (flags & 8) != 0 ? input.readStringRef() : attachmentName;
		switch (AttachmentType.values[flags & 0b111]) {
		case region: {
			String path = (flags & 16) != 0 ? input.readStringRef() : null;
			int color = (flags & 32) != 0 ? input.readInt() : 0xffffffff;
			Sequence sequence = (flags & 64) != 0 ? readSequence(input) : null;
			float rotation = (flags & 128) != 0 ? input.readFloat() : 0;
			float x = input.readFloat();
			float y = input.readFloat();
			float scaleX = input.readFloat();
			float scaleY = input.readFloat();
			float width = input.readFloat();
			float height = input.readFloat();

			if (path == null) path = name;
			RegionAttachment region = attachmentLoader.newRegionAttachment(skin, name, path, sequence);
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
			region.setSequence(sequence);
			if (sequence == null) region.updateRegion();
			return region;
		}
		case boundingbox: {
			Vertices vertices = readVertices(input, (flags & 16) != 0);
			int color = nonessential ? input.readInt() : 0;

			BoundingBoxAttachment box = attachmentLoader.newBoundingBoxAttachment(skin, name);
			if (box == null) return null;
			box.setWorldVerticesLength(vertices.length);
			box.setVertices(vertices.vertices);
			box.setBones(vertices.bones);
			if (nonessential) Color.rgba8888ToColor(box.getColor(), color);
			return box;
		}
		case mesh: {
			String path = (flags & 16) != 0 ? input.readStringRef() : name;
			int color = (flags & 32) != 0 ? input.readInt() : 0xffffffff;
			Sequence sequence = (flags & 64) != 0 ? readSequence(input) : null;
			int hullLength = input.readInt(true);
			Vertices vertices = readVertices(input, (flags & 128) != 0);
			float[] uvs = readFloatArray(input, vertices.length, 1);
			short[] triangles = readShortArray(input, (vertices.length - hullLength - 2) * 3);

			short[] edges = null;
			float width = 0, height = 0;
			if (nonessential) {
				edges = readShortArray(input, input.readInt(true));
				width = input.readFloat();
				height = input.readFloat();
			}

			MeshAttachment mesh = attachmentLoader.newMeshAttachment(skin, name, path, sequence);
			if (mesh == null) return null;
			mesh.setPath(path);
			Color.rgba8888ToColor(mesh.getColor(), color);
			mesh.setBones(vertices.bones);
			mesh.setVertices(vertices.vertices);
			mesh.setWorldVerticesLength(vertices.length);
			mesh.setTriangles(triangles);
			mesh.setRegionUVs(uvs);
			if (sequence == null) mesh.updateRegion();
			mesh.setHullLength(hullLength << 1);
			mesh.setSequence(sequence);
			if (nonessential) {
				mesh.setEdges(edges);
				mesh.setWidth(width * scale);
				mesh.setHeight(height * scale);
			}
			return mesh;
		}
		case linkedmesh: {
			String path = (flags & 16) != 0 ? input.readStringRef() : name;
			int color = (flags & 32) != 0 ? input.readInt() : 0xffffffff;
			Sequence sequence = (flags & 64) != 0 ? readSequence(input) : null;
			boolean inheritTimelines = (flags & 128) != 0;
			int skinIndex = input.readInt(true);
			String parent = input.readStringRef();
			float width = 0, height = 0;
			if (nonessential) {
				width = input.readFloat();
				height = input.readFloat();
			}

			MeshAttachment mesh = attachmentLoader.newMeshAttachment(skin, name, path, sequence);
			if (mesh == null) return null;
			mesh.setPath(path);
			Color.rgba8888ToColor(mesh.getColor(), color);
			mesh.setSequence(sequence);
			if (nonessential) {
				mesh.setWidth(width * scale);
				mesh.setHeight(height * scale);
			}
			linkedMeshes.add(new LinkedMesh(mesh, skinIndex, slotIndex, parent, inheritTimelines));
			return mesh;
		}
		case path: {
			boolean closed = (flags & 16) != 0;
			boolean constantSpeed = (flags & 32) != 0;
			Vertices vertices = readVertices(input, (flags & 64) != 0);
			float[] lengths = new float[vertices.length / 6];
			for (int i = 0, n = lengths.length; i < n; i++)
				lengths[i] = input.readFloat() * scale;
			int color = nonessential ? input.readInt() : 0;

			PathAttachment path = attachmentLoader.newPathAttachment(skin, name);
			if (path == null) return null;
			path.setClosed(closed);
			path.setConstantSpeed(constantSpeed);
			path.setWorldVerticesLength(vertices.length);
			path.setVertices(vertices.vertices);
			path.setBones(vertices.bones);
			path.setLengths(lengths);
			if (nonessential) Color.rgba8888ToColor(path.getColor(), color);
			return path;
		}
		case point: {
			float rotation = input.readFloat();
			float x = input.readFloat();
			float y = input.readFloat();
			int color = nonessential ? input.readInt() : 0;

			PointAttachment point = attachmentLoader.newPointAttachment(skin, name);
			if (point == null) return null;
			point.setX(x * scale);
			point.setY(y * scale);
			point.setRotation(rotation);
			if (nonessential) Color.rgba8888ToColor(point.getColor(), color);
			return point;
		}
		case clipping:
			int endSlotIndex = input.readInt(true);
			Vertices vertices = readVertices(input, (flags & 16) != 0);
			int color = nonessential ? input.readInt() : 0;

			ClippingAttachment clip = attachmentLoader.newClippingAttachment(skin, name);
			if (clip == null) return null;
			clip.setEndSlot(skeletonData.slots.get(endSlotIndex));
			clip.setWorldVerticesLength(vertices.length);
			clip.setVertices(vertices.vertices);
			clip.setBones(vertices.bones);
			if (nonessential) Color.rgba8888ToColor(clip.getColor(), color);
			return clip;
		}
		return null;
	}

	private Sequence readSequence (SkeletonInput input) throws IOException {
		Sequence sequence = new Sequence(input.readInt(true));
		sequence.setStart(input.readInt(true));
		sequence.setDigits(input.readInt(true));
		sequence.setSetupIndex(input.readInt(true));
		return sequence;
	}

	private Vertices readVertices (SkeletonInput input, boolean weighted) throws IOException {
		float scale = this.scale;
		int vertexCount = input.readInt(true);
		Vertices vertices = new Vertices();
		vertices.length = vertexCount << 1;
		if (!weighted) {
			vertices.vertices = readFloatArray(input, vertices.length, scale);
			return vertices;
		}
		FloatArray weights = new FloatArray(vertices.length * 3 * 3);
		IntArray bonesArray = new IntArray(vertices.length * 3);
		for (int i = 0; i < vertexCount; i++) {
			int boneCount = input.readInt(true);
			bonesArray.add(boneCount);
			for (int ii = 0; ii < boneCount; ii++) {
				bonesArray.add(input.readInt(true));
				weights.add(input.readFloat() * scale);
				weights.add(input.readFloat() * scale);
				weights.add(input.readFloat());
			}
		}
		vertices.vertices = weights.toArray();
		vertices.bones = bonesArray.toArray();
		return vertices;
	}

	private float[] readFloatArray (SkeletonInput input, int n, float scale) throws IOException {
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

	private short[] readShortArray (SkeletonInput input, int n) throws IOException {
		short[] array = new short[n];
		for (int i = 0; i < n; i++)
			array[i] = (short)input.readInt(true);
		return array;
	}

	private Animation readAnimation (SkeletonInput input, String name, SkeletonData skeletonData) throws IOException {
		Array<Timeline> timelines = new Array(input.readInt(true));
		float scale = this.scale;

		// Slot timelines.
		for (int i = 0, n = input.readInt(true); i < n; i++) {
			int slotIndex = input.readInt(true);
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				int timelineType = input.readByte(), frameCount = input.readInt(true), frameLast = frameCount - 1;
				switch (timelineType) {
				case SLOT_ATTACHMENT: {
					AttachmentTimeline timeline = new AttachmentTimeline(frameCount, slotIndex);
					for (int frame = 0; frame < frameCount; frame++)
						timeline.setFrame(frame, input.readFloat(), input.readStringRef());
					timelines.add(timeline);
					break;
				}
				case SLOT_RGBA: {
					RGBATimeline timeline = new RGBATimeline(frameCount, input.readInt(true), slotIndex);
					float time = input.readFloat();
					float r = input.read() / 255f, g = input.read() / 255f;
					float b = input.read() / 255f, a = input.read() / 255f;
					for (int frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, r, g, b, a);
						if (frame == frameLast) break;
						float time2 = input.readFloat();
						float r2 = input.read() / 255f, g2 = input.read() / 255f;
						float b2 = input.read() / 255f, a2 = input.read() / 255f;
						switch (input.readByte()) {
						case CURVE_STEPPED:
							timeline.setStepped(frame);
							break;
						case CURVE_BEZIER:
							setBezier(input, timeline, bezier++, frame, 0, time, time2, r, r2, 1);
							setBezier(input, timeline, bezier++, frame, 1, time, time2, g, g2, 1);
							setBezier(input, timeline, bezier++, frame, 2, time, time2, b, b2, 1);
							setBezier(input, timeline, bezier++, frame, 3, time, time2, a, a2, 1);
						}
						time = time2;
						r = r2;
						g = g2;
						b = b2;
						a = a2;
					}
					timelines.add(timeline);
					break;
				}
				case SLOT_RGB: {
					RGBTimeline timeline = new RGBTimeline(frameCount, input.readInt(true), slotIndex);
					float time = input.readFloat();
					float r = input.read() / 255f, g = input.read() / 255f, b = input.read() / 255f;
					for (int frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, r, g, b);
						if (frame == frameLast) break;
						float time2 = input.readFloat();
						float r2 = input.read() / 255f, g2 = input.read() / 255f, b2 = input.read() / 255f;
						switch (input.readByte()) {
						case CURVE_STEPPED:
							timeline.setStepped(frame);
							break;
						case CURVE_BEZIER:
							setBezier(input, timeline, bezier++, frame, 0, time, time2, r, r2, 1);
							setBezier(input, timeline, bezier++, frame, 1, time, time2, g, g2, 1);
							setBezier(input, timeline, bezier++, frame, 2, time, time2, b, b2, 1);
						}
						time = time2;
						r = r2;
						g = g2;
						b = b2;
					}
					timelines.add(timeline);
					break;
				}
				case SLOT_RGBA2: {
					RGBA2Timeline timeline = new RGBA2Timeline(frameCount, input.readInt(true), slotIndex);
					float time = input.readFloat();
					float r = input.read() / 255f, g = input.read() / 255f;
					float b = input.read() / 255f, a = input.read() / 255f;
					float r2 = input.read() / 255f, g2 = input.read() / 255f, b2 = input.read() / 255f;
					for (int frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, r, g, b, a, r2, g2, b2);
						if (frame == frameLast) break;
						float time2 = input.readFloat();
						float nr = input.read() / 255f, ng = input.read() / 255f;
						float nb = input.read() / 255f, na = input.read() / 255f;
						float nr2 = input.read() / 255f, ng2 = input.read() / 255f, nb2 = input.read() / 255f;
						switch (input.readByte()) {
						case CURVE_STEPPED:
							timeline.setStepped(frame);
							break;
						case CURVE_BEZIER:
							setBezier(input, timeline, bezier++, frame, 0, time, time2, r, nr, 1);
							setBezier(input, timeline, bezier++, frame, 1, time, time2, g, ng, 1);
							setBezier(input, timeline, bezier++, frame, 2, time, time2, b, nb, 1);
							setBezier(input, timeline, bezier++, frame, 3, time, time2, a, na, 1);
							setBezier(input, timeline, bezier++, frame, 4, time, time2, r2, nr2, 1);
							setBezier(input, timeline, bezier++, frame, 5, time, time2, g2, ng2, 1);
							setBezier(input, timeline, bezier++, frame, 6, time, time2, b2, nb2, 1);
						}
						time = time2;
						r = nr;
						g = ng;
						b = nb;
						a = na;
						r2 = nr2;
						g2 = ng2;
						b2 = nb2;
					}
					timelines.add(timeline);
					break;
				}
				case SLOT_RGB2: {
					RGB2Timeline timeline = new RGB2Timeline(frameCount, input.readInt(true), slotIndex);
					float time = input.readFloat();
					float r = input.read() / 255f, g = input.read() / 255f, b = input.read() / 255f;
					float r2 = input.read() / 255f, g2 = input.read() / 255f, b2 = input.read() / 255f;
					for (int frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, r, g, b, r2, g2, b2);
						if (frame == frameLast) break;
						float time2 = input.readFloat();
						float nr = input.read() / 255f, ng = input.read() / 255f, nb = input.read() / 255f;
						float nr2 = input.read() / 255f, ng2 = input.read() / 255f, nb2 = input.read() / 255f;
						switch (input.readByte()) {
						case CURVE_STEPPED:
							timeline.setStepped(frame);
							break;
						case CURVE_BEZIER:
							setBezier(input, timeline, bezier++, frame, 0, time, time2, r, nr, 1);
							setBezier(input, timeline, bezier++, frame, 1, time, time2, g, ng, 1);
							setBezier(input, timeline, bezier++, frame, 2, time, time2, b, nb, 1);
							setBezier(input, timeline, bezier++, frame, 3, time, time2, r2, nr2, 1);
							setBezier(input, timeline, bezier++, frame, 4, time, time2, g2, ng2, 1);
							setBezier(input, timeline, bezier++, frame, 5, time, time2, b2, nb2, 1);
						}
						time = time2;
						r = nr;
						g = ng;
						b = nb;
						r2 = nr2;
						g2 = ng2;
						b2 = nb2;
					}
					timelines.add(timeline);
					break;
				}
				case SLOT_ALPHA:
					AlphaTimeline timeline = new AlphaTimeline(frameCount, input.readInt(true), slotIndex);
					float time = input.readFloat(), a = input.read() / 255f;
					for (int frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, a);
						if (frame == frameLast) break;
						float time2 = input.readFloat();
						float a2 = input.read() / 255f;
						switch (input.readByte()) {
						case CURVE_STEPPED:
							timeline.setStepped(frame);
							break;
						case CURVE_BEZIER:
							setBezier(input, timeline, bezier++, frame, 0, time, time2, a, a2, 1);
						}
						time = time2;
						a = a2;
					}
					timelines.add(timeline);
				}
			}
		}

		// Bone timelines.
		for (int i = 0, n = input.readInt(true); i < n; i++) {
			int boneIndex = input.readInt(true);
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				int type = input.readByte(), frameCount = input.readInt(true);
				if (type == BONE_INHERIT) {
					InheritTimeline timeline = new InheritTimeline(frameCount, boneIndex);
					for (int frame = 0; frame < frameCount; frame++)
						timeline.setFrame(frame, input.readFloat(), Inherit.values[input.readByte()]);
					timelines.add(timeline);
					continue;
				}
				int bezierCount = input.readInt(true);
				switch (type) {
				case BONE_ROTATE:
					readTimeline(input, timelines, new RotateTimeline(frameCount, bezierCount, boneIndex), 1);
					break;
				case BONE_TRANSLATE:
					readTimeline(input, timelines, new TranslateTimeline(frameCount, bezierCount, boneIndex), scale);
					break;
				case BONE_TRANSLATEX:
					readTimeline(input, timelines, new TranslateXTimeline(frameCount, bezierCount, boneIndex), scale);
					break;
				case BONE_TRANSLATEY:
					readTimeline(input, timelines, new TranslateYTimeline(frameCount, bezierCount, boneIndex), scale);
					break;
				case BONE_SCALE:
					readTimeline(input, timelines, new ScaleTimeline(frameCount, bezierCount, boneIndex), 1);
					break;
				case BONE_SCALEX:
					readTimeline(input, timelines, new ScaleXTimeline(frameCount, bezierCount, boneIndex), 1);
					break;
				case BONE_SCALEY:
					readTimeline(input, timelines, new ScaleYTimeline(frameCount, bezierCount, boneIndex), 1);
					break;
				case BONE_SHEAR:
					readTimeline(input, timelines, new ShearTimeline(frameCount, bezierCount, boneIndex), 1);
					break;
				case BONE_SHEARX:
					readTimeline(input, timelines, new ShearXTimeline(frameCount, bezierCount, boneIndex), 1);
					break;
				case BONE_SHEARY:
					readTimeline(input, timelines, new ShearYTimeline(frameCount, bezierCount, boneIndex), 1);
				}
			}
		}

		// IK constraint timelines.
		for (int i = 0, n = input.readInt(true); i < n; i++) {
			int index = input.readInt(true), frameCount = input.readInt(true), frameLast = frameCount - 1;
			IkConstraintTimeline timeline = new IkConstraintTimeline(frameCount, input.readInt(true), index);
			int flags = input.read();
			float time = input.readFloat(), mix = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.readFloat() : 1) : 0;
			float softness = (flags & 4) != 0 ? input.readFloat() * scale : 0;
			for (int frame = 0, bezier = 0;; frame++) {
				timeline.setFrame(frame, time, mix, softness, (flags & 8) != 0 ? 1 : -1, (flags & 16) != 0, (flags & 32) != 0);
				if (frame == frameLast) break;
				flags = input.read();
				float time2 = input.readFloat(), mix2 = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.readFloat() : 1) : 0;
				float softness2 = (flags & 4) != 0 ? input.readFloat() * scale : 0;
				if ((flags & 64) != 0)
					timeline.setStepped(frame);
				else if ((flags & 128) != 0) {
					setBezier(input, timeline, bezier++, frame, 0, time, time2, mix, mix2, 1);
					setBezier(input, timeline, bezier++, frame, 1, time, time2, softness, softness2, scale);
				}
				time = time2;
				mix = mix2;
				softness = softness2;
			}
			timelines.add(timeline);
		}

		// Transform constraint timelines.
		for (int i = 0, n = input.readInt(true); i < n; i++) {
			int index = input.readInt(true), frameCount = input.readInt(true), frameLast = frameCount - 1;
			TransformConstraintTimeline timeline = new TransformConstraintTimeline(frameCount, input.readInt(true), index);
			float time = input.readFloat(), mixRotate = input.readFloat(), mixX = input.readFloat(), mixY = input.readFloat(),
				mixScaleX = input.readFloat(), mixScaleY = input.readFloat(), mixShearY = input.readFloat();
			for (int frame = 0, bezier = 0;; frame++) {
				timeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
				if (frame == frameLast) break;
				float time2 = input.readFloat(), mixRotate2 = input.readFloat(), mixX2 = input.readFloat(), mixY2 = input.readFloat(),
					mixScaleX2 = input.readFloat(), mixScaleY2 = input.readFloat(), mixShearY2 = input.readFloat();
				switch (input.readByte()) {
				case CURVE_STEPPED:
					timeline.setStepped(frame);
					break;
				case CURVE_BEZIER:
					setBezier(input, timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
					setBezier(input, timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
					setBezier(input, timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
					setBezier(input, timeline, bezier++, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
					setBezier(input, timeline, bezier++, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
					setBezier(input, timeline, bezier++, frame, 5, time, time2, mixShearY, mixShearY2, 1);
				}
				time = time2;
				mixRotate = mixRotate2;
				mixX = mixX2;
				mixY = mixY2;
				mixScaleX = mixScaleX2;
				mixScaleY = mixScaleY2;
				mixShearY = mixShearY2;
			}
			timelines.add(timeline);
		}

		// Path constraint timelines.
		for (int i = 0, n = input.readInt(true); i < n; i++) {
			int index = input.readInt(true);
			PathConstraintData data = skeletonData.pathConstraints.get(index);
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				int type = input.readByte(), frameCount = input.readInt(true), bezierCount = input.readInt(true);
				switch (type) {
				case PATH_POSITION:
					readTimeline(input, timelines, new PathConstraintPositionTimeline(frameCount, bezierCount, index),
						data.positionMode == PositionMode.fixed ? scale : 1);
					break;
				case PATH_SPACING:
					readTimeline(input, timelines, new PathConstraintSpacingTimeline(frameCount, bezierCount, index),
						data.spacingMode == SpacingMode.length || data.spacingMode == SpacingMode.fixed ? scale : 1);
					break;
				case PATH_MIX:
					PathConstraintMixTimeline timeline = new PathConstraintMixTimeline(frameCount, bezierCount, index);
					float time = input.readFloat(), mixRotate = input.readFloat(), mixX = input.readFloat(), mixY = input.readFloat();
					for (int frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1;; frame++) {
						timeline.setFrame(frame, time, mixRotate, mixX, mixY);
						if (frame == frameLast) break;
						float time2 = input.readFloat(), mixRotate2 = input.readFloat(), mixX2 = input.readFloat(),
							mixY2 = input.readFloat();
						switch (input.readByte()) {
						case CURVE_STEPPED:
							timeline.setStepped(frame);
							break;
						case CURVE_BEZIER:
							setBezier(input, timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
							setBezier(input, timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
							setBezier(input, timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
						}
						time = time2;
						mixRotate = mixRotate2;
						mixX = mixX2;
						mixY = mixY2;
					}
					timelines.add(timeline);
				}
			}
		}

		// Physics timelines.
		for (int i = 0, n = input.readInt(true); i < n; i++) {
			int index = input.readInt(true) - 1;
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				int type = input.readByte(), frameCount = input.readInt(true);
				if (type == PHYSICS_RESET) {
					PhysicsConstraintResetTimeline timeline = new PhysicsConstraintResetTimeline(frameCount, index);
					for (int frame = 0; frame < frameCount; frame++)
						timeline.setFrame(frame, input.readFloat());
					timelines.add(timeline);
					continue;
				}
				int bezierCount = input.readInt(true);
				switch (type) {
				case PHYSICS_INERTIA:
					readTimeline(input, timelines, new PhysicsConstraintInertiaTimeline(frameCount, bezierCount, index), 1);
					break;
				case PHYSICS_STRENGTH:
					readTimeline(input, timelines, new PhysicsConstraintStrengthTimeline(frameCount, bezierCount, index), 1);
					break;
				case PHYSICS_DAMPING:
					readTimeline(input, timelines, new PhysicsConstraintDampingTimeline(frameCount, bezierCount, index), 1);
					break;
				case PHYSICS_MASS:
					readTimeline(input, timelines, new PhysicsConstraintMassTimeline(frameCount, bezierCount, index), 1);
					break;
				case PHYSICS_WIND:
					readTimeline(input, timelines, new PhysicsConstraintWindTimeline(frameCount, bezierCount, index), 1);
					break;
				case PHYSICS_GRAVITY:
					readTimeline(input, timelines, new PhysicsConstraintGravityTimeline(frameCount, bezierCount, index), 1);
					break;
				case PHYSICS_MIX:
					readTimeline(input, timelines, new PhysicsConstraintMixTimeline(frameCount, bezierCount, index), 1);
				}
			}
		}

		// Attachment timelines.
		for (int i = 0, n = input.readInt(true); i < n; i++) {
			Skin skin = skeletonData.skins.get(input.readInt(true));
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				int slotIndex = input.readInt(true);
				for (int iii = 0, nnn = input.readInt(true); iii < nnn; iii++) {
					String attachmentName = input.readStringRef();
					Attachment attachment = skin.getAttachment(slotIndex, attachmentName);
					if (attachment == null) throw new SerializationException("Timeline attachment not found: " + attachmentName);

					int timelineType = input.readByte(), frameCount = input.readInt(true), frameLast = frameCount - 1;
					switch (timelineType) {
					case ATTACHMENT_DEFORM: {
						VertexAttachment vertexAttachment = (VertexAttachment)attachment;
						boolean weighted = vertexAttachment.getBones() != null;
						float[] vertices = vertexAttachment.getVertices();
						int deformLength = weighted ? (vertices.length / 3) << 1 : vertices.length;

						DeformTimeline timeline = new DeformTimeline(frameCount, input.readInt(true), slotIndex, vertexAttachment);

						float time = input.readFloat();
						for (int frame = 0, bezier = 0;; frame++) {
							float[] deform;
							int end = input.readInt(true);
							if (end == 0)
								deform = weighted ? new float[deformLength] : vertices;
							else {
								deform = new float[deformLength];
								int start = input.readInt(true);
								end += start;
								if (scale == 1) {
									for (int v = start; v < end; v++)
										deform[v] = input.readFloat();
								} else {
									for (int v = start; v < end; v++)
										deform[v] = input.readFloat() * scale;
								}
								if (!weighted) {
									for (int v = 0, vn = deform.length; v < vn; v++)
										deform[v] += vertices[v];
								}
							}
							timeline.setFrame(frame, time, deform);
							if (frame == frameLast) break;
							float time2 = input.readFloat();
							switch (input.readByte()) {
							case CURVE_STEPPED:
								timeline.setStepped(frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, timeline, bezier++, frame, 0, time, time2, 0, 1, 1);
							}
							time = time2;
						}
						timelines.add(timeline);
						break;
					}
					case ATTACHMENT_SEQUENCE:
						SequenceTimeline timeline = new SequenceTimeline(frameCount, slotIndex, attachment);
						for (int frame = 0; frame < frameCount; frame++) {
							float time = input.readFloat();
							int modeAndIndex = input.readInt();
							timeline.setFrame(frame, time, SequenceMode.values[modeAndIndex & 0xf], modeAndIndex >> 4,
								input.readFloat());
						}
						timelines.add(timeline);
					}
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
				event.stringValue = input.readString();
				if (event.stringValue == null) event.stringValue = eventData.stringValue;
				if (event.getData().audioPath != null) {
					event.volume = input.readFloat();
					event.balance = input.readFloat();
				}
				timeline.setFrame(i, event);
			}
			timelines.add(timeline);
		}

		float duration = 0;
		Object[] items = timelines.items;
		for (int i = 0, n = timelines.size; i < n; i++)
			duration = Math.max(duration, ((Timeline)items[i]).getDuration());
		return new Animation(name, timelines, duration);
	}

	private void readTimeline (SkeletonInput input, Array<Timeline> timelines, CurveTimeline1 timeline, float scale)
		throws IOException {
		float time = input.readFloat(), value = input.readFloat() * scale;
		for (int frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1;; frame++) {
			timeline.setFrame(frame, time, value);
			if (frame == frameLast) break;
			float time2 = input.readFloat(), value2 = input.readFloat() * scale;
			switch (input.readByte()) {
			case CURVE_STEPPED:
				timeline.setStepped(frame);
				break;
			case CURVE_BEZIER:
				setBezier(input, timeline, bezier++, frame, 0, time, time2, value, value2, scale);
			}
			time = time2;
			value = value2;
		}
		timelines.add(timeline);
	}

	private void readTimeline (SkeletonInput input, Array<Timeline> timelines, CurveTimeline2 timeline, float scale)
		throws IOException {
		float time = input.readFloat(), value1 = input.readFloat() * scale, value2 = input.readFloat() * scale;
		for (int frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1;; frame++) {
			timeline.setFrame(frame, time, value1, value2);
			if (frame == frameLast) break;
			float time2 = input.readFloat(), nvalue1 = input.readFloat() * scale, nvalue2 = input.readFloat() * scale;
			switch (input.readByte()) {
			case CURVE_STEPPED:
				timeline.setStepped(frame);
				break;
			case CURVE_BEZIER:
				setBezier(input, timeline, bezier++, frame, 0, time, time2, value1, nvalue1, scale);
				setBezier(input, timeline, bezier++, frame, 1, time, time2, value2, nvalue2, scale);
			}
			time = time2;
			value1 = nvalue1;
			value2 = nvalue2;
		}
		timelines.add(timeline);
	}

	void setBezier (SkeletonInput input, CurveTimeline timeline, int bezier, int frame, int value, float time1, float time2,
		float value1, float value2, float scale) throws IOException {
		timeline.setBezier(bezier, frame, value, time1, value1, input.readFloat(), input.readFloat() * scale, input.readFloat(),
			input.readFloat() * scale, time2, value2);
	}

	static class Vertices {
		int length;
		int[] bones;
		float[] vertices;
	}

	static class SkeletonInput extends DataInput {
		private char[] chars = new char[32];
		String[] strings;

		public SkeletonInput (InputStream input) {
			super(input);
		}

		public SkeletonInput (FileHandle file) {
			super(file.read(512));
		}

		public @Null String readStringRef () throws IOException {
			int index = readInt(true);
			return index == 0 ? null : strings[index - 1];
		}

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
				case -1:
					throw new EOFException();
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
	}

	static class LinkedMesh {
		String parent;
		int skinIndex, slotIndex;
		MeshAttachment mesh;
		boolean inheritTimelines;

		public LinkedMesh (MeshAttachment mesh, int skinIndex, int slotIndex, String parent, boolean inheritTimelines) {
			this.mesh = mesh;
			this.skinIndex = skinIndex;
			this.slotIndex = slotIndex;
			this.parent = parent;
			this.inheritTimelines = inheritTimelines;
		}
	}
}
