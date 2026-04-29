/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import java.io.EOFException;
import java.io.IOException;
import java.io.InputStream;
import java.util.Arrays;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.DataInput;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.Null;
import com.badlogic.gdx.utils.SerializationException;

import com.esotericsoftware.spine.Animation.AlphaTimeline;
import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.BoneTimeline2;
import com.esotericsoftware.spine.Animation.CurveTimeline;
import com.esotericsoftware.spine.Animation.CurveTimeline1;
import com.esotericsoftware.spine.Animation.DeformTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderFolderTimeline;
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
import com.esotericsoftware.spine.Animation.SliderMixTimeline;
import com.esotericsoftware.spine.Animation.SliderTimeline;
import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.Animation.TransformConstraintTimeline;
import com.esotericsoftware.spine.Animation.TranslateTimeline;
import com.esotericsoftware.spine.Animation.TranslateXTimeline;
import com.esotericsoftware.spine.Animation.TranslateYTimeline;
import com.esotericsoftware.spine.BoneData.Inherit;
import com.esotericsoftware.spine.PathConstraintData.PositionMode;
import com.esotericsoftware.spine.PathConstraintData.RotateMode;
import com.esotericsoftware.spine.PathConstraintData.SpacingMode;
import com.esotericsoftware.spine.TransformConstraintData.FromProperty;
import com.esotericsoftware.spine.TransformConstraintData.FromRotate;
import com.esotericsoftware.spine.TransformConstraintData.FromScaleX;
import com.esotericsoftware.spine.TransformConstraintData.FromScaleY;
import com.esotericsoftware.spine.TransformConstraintData.FromShearY;
import com.esotericsoftware.spine.TransformConstraintData.FromX;
import com.esotericsoftware.spine.TransformConstraintData.FromY;
import com.esotericsoftware.spine.TransformConstraintData.ToProperty;
import com.esotericsoftware.spine.TransformConstraintData.ToRotate;
import com.esotericsoftware.spine.TransformConstraintData.ToScaleX;
import com.esotericsoftware.spine.TransformConstraintData.ToScaleY;
import com.esotericsoftware.spine.TransformConstraintData.ToShearY;
import com.esotericsoftware.spine.TransformConstraintData.ToX;
import com.esotericsoftware.spine.TransformConstraintData.ToY;
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
 * See <a href="https://esotericsoftware.com/spine-binary-format">Spine binary format</a> and
 * <a href="https://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data">JSON and binary data</a> in the Spine
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

	static public final int CONSTRAINT_IK = 0;
	static public final int CONSTRAINT_PATH = 1;
	static public final int CONSTRAINT_TRANSFORM = 2;
	static public final int CONSTRAINT_PHYSICS = 3;
	static public final int CONSTRAINT_SLIDER = 4;

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

	static public final int SLIDER_TIME = 0;
	static public final int SLIDER_MIX = 1;

	static public final int CURVE_LINEAR = 0;
	static public final int CURVE_STEPPED = 1;
	static public final int CURVE_BEZIER = 2;

	private final Array<LinkedMesh> linkedMeshes = new Array(true, 8, LinkedMesh[]::new);

	public SkeletonBinary (AttachmentLoader attachmentLoader) {
		super(attachmentLoader);
	}

	public SkeletonBinary (TextureAtlas atlas) {
		super(atlas);
	}

	public SkeletonData readSkeletonData (FileHandle file) {
		if (file == null) throw new IllegalArgumentException("file cannot be null.");
		try {
			SkeletonData skeletonData = readSkeletonData(file.read());
			skeletonData.name = file.nameWithoutExtension();
			return skeletonData;
		} catch (Throwable ex) {
			throw new SerializationException("Error reading binary skeleton file: " + file, ex);
		}
	}

	public SkeletonData readSkeletonData (InputStream dataInput) {
		if (dataInput == null) throw new IllegalArgumentException("dataInput cannot be null.");

		float scale = this.scale;

		var input = new SkeletonInput(dataInput);
		var skeletonData = new SkeletonData();
		String version = null;
		try {
			long hash = input.readLong();
			skeletonData.hash = hash == 0 ? null : Long.toString(hash);
			skeletonData.version = input.readString();
			if (skeletonData.version.isEmpty()) skeletonData.version = null;
			version = skeletonData.version;
			skeletonData.x = input.readFloat();
			skeletonData.y = input.readFloat();
			skeletonData.width = input.readFloat();
			skeletonData.height = input.readFloat();
			skeletonData.referenceScale = input.readFloat() * scale;

			boolean nonessential = input.readBoolean();
			if (nonessential) {
				skeletonData.fps = input.readFloat();
				skeletonData.imagesPath = input.readString();
				skeletonData.audioPath = input.readString();
			}

			int n;
			Object[] o;

			// Strings.
			o = input.strings = new String[n = input.readInt(true)];
			for (int i = 0; i < n; i++)
				o[i] = input.readString();

			// Bones.
			BoneData[] bones = skeletonData.bones.setSize(n = input.readInt(true));
			for (int i = 0; i < n; i++) {
				String name = input.readString();
				BoneData parent = i == 0 ? null : bones[input.readInt(true)];
				var data = new BoneData(i, name, parent);
				BonePose setup = data.setupPose;
				setup.rotation = input.readFloat();
				setup.x = input.readFloat() * scale;
				setup.y = input.readFloat() * scale;
				setup.scaleX = input.readFloat();
				setup.scaleY = input.readFloat();
				setup.shearX = input.readFloat();
				setup.shearY = input.readFloat();
				setup.inherit = Inherit.values[input.readByte()];
				data.length = input.readFloat() * scale;
				data.skinRequired = input.readBoolean();
				if (nonessential) {
					Color.rgba8888ToColor(data.color, input.readInt());
					data.icon = input.readString();
					data.iconSize = input.readFloat();
					data.iconRotation = input.readFloat();
					data.visible = input.readBoolean();
				}
				bones[i] = data;
			}

			// Slots.
			SlotData[] slots = skeletonData.slots.setSize(n = input.readInt(true));
			for (int i = 0; i < n; i++) {
				String slotName = input.readString();
				var boneData = bones[input.readInt(true)];
				var data = new SlotData(i, slotName, boneData);
				Color.rgba8888ToColor(data.setupPose.color, input.readInt());

				int darkColor = input.readInt();
				if (darkColor != -1) Color.rgb888ToColor(data.setupPose.darkColor = new Color(), darkColor);

				data.attachmentName = input.readStringRef();
				data.blendMode = BlendMode.values[input.readInt(true)];
				if (nonessential) data.visible = input.readBoolean();
				slots[i] = data;
			}

			// Constraints.
			int constraintCount = input.readInt(true);
			ConstraintData[] constraints = skeletonData.constraints.setSize(constraintCount);
			for (int i = 0; i < constraintCount; i++) {
				String name = input.readString();
				int nn;
				switch (input.readByte()) {
				case CONSTRAINT_IK -> {
					var data = new IkConstraintData(name);
					BoneData[] constraintBones = data.bones.setSize(nn = input.readInt(true));
					for (int ii = 0; ii < nn; ii++)
						constraintBones[ii] = bones[input.readInt(true)];
					data.target = bones[input.readInt(true)];
					int flags = input.read();
					data.skinRequired = (flags & 1) != 0;
					if ((flags & 2) != 0) data.scaleY = IkConstraintData.ScaleY.values[input.read()];
					IkConstraintPose setup = data.setupPose;
					setup.bendDirection = (flags & 4) != 0 ? -1 : 1;
					setup.compress = (flags & 8) != 0;
					setup.stretch = (flags & 16) != 0;
					if ((flags & 32) != 0) setup.mix = (flags & 64) != 0 ? input.readFloat() : 1;
					if ((flags & 128) != 0) setup.softness = input.readFloat() * scale;
					constraints[i] = data;
				}
				case CONSTRAINT_TRANSFORM -> {
					var data = new TransformConstraintData(name);
					BoneData[] constraintBones = data.bones.setSize(nn = input.readInt(true));
					for (int ii = 0; ii < nn; ii++)
						constraintBones[ii] = bones[input.readInt(true)];
					data.source = bones[input.readInt(true)];
					int flags = input.read();
					data.skinRequired = (flags & 1) != 0;
					data.localSource = (flags & 2) != 0;
					data.localTarget = (flags & 4) != 0;
					data.additive = (flags & 8) != 0;
					data.clamp = (flags & 16) != 0;
					FromProperty[] froms = data.properties.setSize(nn = flags >> 5);
					for (int ii = 0, tn; ii < nn; ii++) {
						float fromScale = 1;
						FromProperty from = switch (input.readByte()) {
						case 0 -> new FromRotate();
						case 1 -> {
							fromScale = scale;
							yield new FromX();
						}
						case 2 -> {
							fromScale = scale;
							yield new FromY();
						}
						case 3 -> new FromScaleX();
						case 4 -> new FromScaleY();
						case 5 -> new FromShearY();
						default -> null;
						};
						from.offset = input.readFloat() * fromScale;
						ToProperty[] tos = from.to.setSize(tn = input.readByte());
						for (int t = 0; t < tn; t++) {
							float toScale = 1;
							ToProperty to = switch (input.readByte()) {
							case 0 -> new ToRotate();
							case 1 -> {
								toScale = scale;
								yield new ToX();
							}
							case 2 -> {
								toScale = scale;
								yield new ToY();
							}
							case 3 -> new ToScaleX();
							case 4 -> new ToScaleY();
							case 5 -> new ToShearY();
							default -> null;
							};
							to.offset = input.readFloat() * toScale;
							to.max = input.readFloat() * toScale;
							to.scale = input.readFloat() * toScale / fromScale;
							tos[t] = to;
						}
						froms[ii] = from;
					}
					flags = input.read();
					if ((flags & 1) != 0) data.offsets[TransformConstraintData.ROTATION] = input.readFloat();
					if ((flags & 2) != 0) data.offsets[TransformConstraintData.X] = input.readFloat() * scale;
					if ((flags & 4) != 0) data.offsets[TransformConstraintData.Y] = input.readFloat() * scale;
					if ((flags & 8) != 0) data.offsets[TransformConstraintData.SCALEX] = input.readFloat();
					if ((flags & 16) != 0) data.offsets[TransformConstraintData.SCALEY] = input.readFloat();
					if ((flags & 32) != 0) data.offsets[TransformConstraintData.SHEARY] = input.readFloat();
					flags = input.read();
					TransformConstraintPose setup = data.setupPose;
					if ((flags & 1) != 0) setup.mixRotate = input.readFloat();
					if ((flags & 2) != 0) setup.mixX = input.readFloat();
					if ((flags & 4) != 0) setup.mixY = input.readFloat();
					if ((flags & 8) != 0) setup.mixScaleX = input.readFloat();
					if ((flags & 16) != 0) setup.mixScaleY = input.readFloat();
					if ((flags & 32) != 0) setup.mixShearY = input.readFloat();
					constraints[i] = data;
				}
				case CONSTRAINT_PATH -> {
					var data = new PathConstraintData(name);
					BoneData[] constraintBones = data.bones.setSize(nn = input.readInt(true));
					for (int ii = 0; ii < nn; ii++)
						constraintBones[ii] = bones[input.readInt(true)];
					data.slot = slots[input.readInt(true)];
					int flags = input.read();
					data.skinRequired = (flags & 1) != 0;
					data.positionMode = PositionMode.values[(flags >> 1) & 0b1];
					data.spacingMode = SpacingMode.values[(flags >> 2) & 0b11];
					data.rotateMode = RotateMode.values[(flags >> 4) & 0b11];
					if ((flags & 128) != 0) data.offsetRotation = input.readFloat();
					PathConstraintPose setup = data.setupPose;
					setup.position = input.readFloat();
					if (data.positionMode == PositionMode.fixed) setup.position *= scale;
					setup.spacing = input.readFloat();
					if (data.spacingMode == SpacingMode.length || data.spacingMode == SpacingMode.fixed) setup.spacing *= scale;
					setup.mixRotate = input.readFloat();
					setup.mixX = input.readFloat();
					setup.mixY = input.readFloat();
					constraints[i] = data;
				}
				case CONSTRAINT_PHYSICS -> {
					var data = new PhysicsConstraintData(name);
					data.bone = bones[input.readInt(true)];
					int flags = input.read();
					data.skinRequired = (flags & 1) != 0;
					if ((flags & 2) != 0) data.x = input.readFloat();
					if ((flags & 4) != 0) data.y = input.readFloat();
					if ((flags & 8) != 0) data.rotate = input.readFloat();
					if ((flags & 16) != 0) data.scaleX = input.readFloat();
					if ((flags & 32) != 0) data.shearX = input.readFloat();
					data.limit = ((flags & 64) != 0 ? input.readFloat() : 5000) * scale;
					data.step = 1f / input.readUnsignedByte();
					PhysicsConstraintPose setup = data.setupPose;
					setup.inertia = input.readFloat();
					setup.strength = input.readFloat();
					setup.damping = input.readFloat();
					setup.massInverse = (flags & 128) != 0 ? input.readFloat() : 1;
					setup.wind = input.readFloat();
					setup.gravity = input.readFloat();
					flags = input.read();
					if ((flags & 1) != 0) data.inertiaGlobal = true;
					if ((flags & 2) != 0) data.strengthGlobal = true;
					if ((flags & 4) != 0) data.dampingGlobal = true;
					if ((flags & 8) != 0) data.massGlobal = true;
					if ((flags & 16) != 0) data.windGlobal = true;
					if ((flags & 32) != 0) data.gravityGlobal = true;
					if ((flags & 64) != 0) data.mixGlobal = true;
					setup.mix = (flags & 128) != 0 ? input.readFloat() : 1;
					constraints[i] = data;
				}
				case CONSTRAINT_SLIDER -> {
					var data = new SliderData(name);
					int flags = input.read();
					data.skinRequired = (flags & 1) != 0;
					data.loop = (flags & 2) != 0;
					data.additive = (flags & 4) != 0;
					if ((flags & 8) != 0) data.setupPose.time = input.readFloat();
					if ((flags & 16) != 0) data.setupPose.mix = (flags & 32) != 0 ? input.readFloat() : 1;
					if ((flags & 64) != 0) {
						data.local = (flags & 128) != 0;
						data.bone = bones[input.readInt(true)];
						float offset = input.readFloat();
						float propertyScale = 1;
						data.property = switch (input.readByte()) {
						case 0 -> new FromRotate();
						case 1 -> {
							propertyScale = scale;
							yield new FromX();
						}
						case 2 -> {
							propertyScale = scale;
							yield new FromY();
						}
						case 3 -> new FromScaleX();
						case 4 -> new FromScaleY();
						case 5 -> new FromShearY();
						default -> null;
						};
						data.property.offset = offset * propertyScale;
						data.offset = input.readFloat();
						data.scale = input.readFloat() / propertyScale;
					}
					constraints[i] = data;
				}
				}
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
			LinkedMesh[] items = linkedMeshes.items;
			for (int i = 0; i < n; i++) {
				LinkedMesh linkedMesh = items[i];
				Skin skin = skeletonData.skins.items[linkedMesh.skinIndex];
				Attachment source = skin.getAttachment(linkedMesh.sourceIndex, linkedMesh.source);
				if (source == null) throw new SerializationException("Source mesh not found: " + linkedMesh.source);
				linkedMesh.mesh.setTimelineAttachment(linkedMesh.inheritTimelines ? source : linkedMesh.mesh);
				linkedMesh.mesh.setSourceMesh((MeshAttachment)source);
				linkedMesh.mesh.updateSequence();
			}
			linkedMeshes.clear();

			// Events.
			o = skeletonData.events.setSize(n = input.readInt(true));
			for (int i = 0; i < n; i++) {
				var data = new EventData(input.readString());
				Event setup = data.setupPose;
				setup.intValue = input.readInt(false);
				setup.floatValue = input.readFloat();
				setup.stringValue = input.readString();
				data.audioPath = input.readString();
				if (data.audioPath != null) {
					setup.volume = input.readFloat();
					setup.balance = input.readFloat();
				}
				o[i] = data;
			}

			// Animations.
			Animation[] animations = skeletonData.animations.setSize(n = input.readInt(true));
			for (int i = 0; i < n; i++)
				animations[i] = readAnimation(input, input.readString(), skeletonData, nonessential);

			for (int i = 0; i < constraintCount; i++)
				if (constraints[i] instanceof SliderData data) data.animation = animations[input.readInt(true)];
		} catch (Throwable ex) {
			if (version != null) throw new SerializationException("Error reading binary skeleton data, version: " + version, ex);
			throw new SerializationException("Error binary skeleton data.", ex);
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

			int n;
			Object[] from = skeletonData.bones.items, to = skin.bones.setSize(n = input.readInt(true));
			for (int i = 0; i < n; i++)
				to[i] = from[input.readInt(true)];

			from = skeletonData.constraints.items;
			to = skin.constraints.setSize(n = input.readInt(true));
			for (int i = 0; i < n; i++)
				to[i] = from[input.readInt(true)];

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
		return switch (AttachmentType.values[flags & 0b111]) {
		case region -> {
			String path = (flags & 16) != 0 ? input.readStringRef() : null;
			int color = (flags & 32) != 0 ? input.readInt() : 0xffffffff;
			Sequence sequence = readSequence(input, (flags & 64) != 0);
			float rotation = (flags & 128) != 0 ? input.readFloat() : 0;
			float x = input.readFloat();
			float y = input.readFloat();
			float scaleX = input.readFloat();
			float scaleY = input.readFloat();
			float width = input.readFloat();
			float height = input.readFloat();

			if (path == null) path = name;
			RegionAttachment region = attachmentLoader.newRegionAttachment(skin, name, path, sequence);
			if (region == null) yield null;
			region.setPath(path);
			region.setX(x * scale);
			region.setY(y * scale);
			region.setScaleX(scaleX);
			region.setScaleY(scaleY);
			region.setRotation(rotation);
			region.setWidth(width * scale);
			region.setHeight(height * scale);
			Color.rgba8888ToColor(region.getColor(), color);
			region.updateSequence();
			yield region;
		}
		case boundingbox -> {
			Vertices vertices = readVertices(input, (flags & 16) != 0);
			int color = nonessential ? input.readInt() : 0;

			BoundingBoxAttachment box = attachmentLoader.newBoundingBoxAttachment(skin, name);
			if (box == null) yield null;
			box.setWorldVerticesLength(vertices.length);
			box.setVertices(vertices.vertices);
			box.setBones(vertices.bones);
			if (nonessential) Color.rgba8888ToColor(box.getColor(), color);
			yield box;
		}
		case mesh -> {
			String path = (flags & 16) != 0 ? input.readStringRef() : name;
			int color = (flags & 32) != 0 ? input.readInt() : 0xffffffff;
			Sequence sequence = readSequence(input, (flags & 64) != 0);
			int hullLength = input.readInt(true);
			Vertices vertices = readVertices(input, (flags & 128) != 0);
			float[] uvs = readFloatArray(input, vertices.length, 1);
			short[] triangles = readShortArray(input, (vertices.length - hullLength - 2) * 3);

			int slotCount = input.readInt(true);
			int[] timelineSlots = null;
			if (slotCount > 0) {
				timelineSlots = new int[slotCount];
				for (int i = 0; i < slotCount; i++)
					timelineSlots[i] = input.readInt(true);
			}

			short[] edges = null;
			float width = 0, height = 0;
			if (nonessential) {
				edges = readShortArray(input, input.readInt(true));
				width = input.readFloat();
				height = input.readFloat();
			}

			MeshAttachment mesh = attachmentLoader.newMeshAttachment(skin, name, path, sequence);
			if (mesh == null) yield null;
			mesh.setPath(path);
			Color.rgba8888ToColor(mesh.getColor(), color);
			mesh.setHullLength(hullLength << 1);
			mesh.setBones(vertices.bones);
			mesh.setVertices(vertices.vertices);
			mesh.setWorldVerticesLength(vertices.length);
			mesh.setRegionUVs(uvs);
			mesh.setTriangles(triangles);
			if (timelineSlots != null) mesh.setTimelineSlots(timelineSlots);
			if (nonessential) {
				mesh.setEdges(edges);
				mesh.setWidth(width * scale);
				mesh.setHeight(height * scale);
			}
			mesh.updateSequence();
			yield mesh;
		}
		case linkedmesh -> {
			String path = (flags & 16) != 0 ? input.readStringRef() : name;
			int color = (flags & 32) != 0 ? input.readInt() : 0xffffffff;
			Sequence sequence = readSequence(input, (flags & 64) != 0);
			boolean inheritTimelines = (flags & 128) != 0;
			int sourceIndex = input.readInt(true);
			int skinIndex = input.readInt(true);
			String source = input.readStringRef();

			float width = 0, height = 0;
			if (nonessential) {
				width = input.readFloat();
				height = input.readFloat();
			}

			MeshAttachment mesh = attachmentLoader.newMeshAttachment(skin, name, path, sequence);
			if (mesh == null) yield null;
			mesh.setPath(path);
			Color.rgba8888ToColor(mesh.getColor(), color);
			if (nonessential) {
				mesh.setWidth(width * scale);
				mesh.setHeight(height * scale);
			}
			linkedMeshes.add(new LinkedMesh(mesh, skinIndex, slotIndex, sourceIndex, source, inheritTimelines));
			yield mesh;
		}
		case path -> {
			boolean closed = (flags & 16) != 0;
			boolean constantSpeed = (flags & 32) != 0;
			Vertices vertices = readVertices(input, (flags & 64) != 0);
			float[] lengths = readFloatArray(input, vertices.length / 6, scale);
			int color = nonessential ? input.readInt() : 0;

			PathAttachment path = attachmentLoader.newPathAttachment(skin, name);
			if (path == null) yield null;
			path.setClosed(closed);
			path.setConstantSpeed(constantSpeed);
			path.setWorldVerticesLength(vertices.length);
			path.setVertices(vertices.vertices);
			path.setBones(vertices.bones);
			path.setLengths(lengths);
			if (nonessential) Color.rgba8888ToColor(path.getColor(), color);
			yield path;
		}
		case point -> {
			float rotation = input.readFloat();
			float x = input.readFloat();
			float y = input.readFloat();
			int color = nonessential ? input.readInt() : 0;

			PointAttachment point = attachmentLoader.newPointAttachment(skin, name);
			if (point == null) yield null;
			point.setX(x * scale);
			point.setY(y * scale);
			point.setRotation(rotation);
			if (nonessential) Color.rgba8888ToColor(point.getColor(), color);
			yield point;
		}
		case clipping -> {
			int endSlotIndex = input.readInt(true);
			Vertices vertices = readVertices(input, (flags & 16) != 0);
			int color = nonessential ? input.readInt() : 0;

			ClippingAttachment clip = attachmentLoader.newClippingAttachment(skin, name);
			if (clip == null) yield null;
			clip.setEndSlot(skeletonData.slots.items[endSlotIndex]);
			clip.setConvex((flags & 32) != 0);
			clip.setInverse((flags & 64) != 0);
			clip.setWorldVerticesLength(vertices.length);
			clip.setVertices(vertices.vertices);
			clip.setBones(vertices.bones);
			if (nonessential) Color.rgba8888ToColor(clip.getColor(), color);
			yield clip;
		}
		default -> null;
		};
	}

	private Sequence readSequence (SkeletonInput input, boolean hasPathSuffix) throws IOException {
		if (!hasPathSuffix) return new Sequence(1, false);
		var sequence = new Sequence(input.readInt(true), true);
		sequence.setStart(input.readInt(true));
		sequence.setDigits(input.readInt(true));
		sequence.setSetupIndex(input.readInt(true));
		return sequence;
	}

	private Vertices readVertices (SkeletonInput input, boolean weighted) throws IOException {
		float scale = this.scale;
		int vertexCount = input.readInt(true);
		var vertices = new Vertices();
		vertices.length = vertexCount << 1;
		if (!weighted) {
			vertices.vertices = readFloatArray(input, vertices.length, scale);
			return vertices;
		}
		int n = input.readInt(true);
		var bones = new int[n];
		var weights = new float[(n - vertexCount) * 3];
		for (int b = 0, w = 0; b < n;) {
			int boneCount = input.readInt(true);
			bones[b++] = boneCount;
			for (int ii = 0; ii < boneCount; ii++, w += 3) {
				bones[b++] = input.readInt(true);
				weights[w] = input.readFloat() * scale;
				weights[w + 1] = input.readFloat() * scale;
				weights[w + 2] = input.readFloat();
			}
		}
		vertices.vertices = weights;
		vertices.bones = bones;
		return vertices;
	}

	private float[] readFloatArray (SkeletonInput input, int n, float scale) throws IOException {
		var array = new float[n];
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
		var array = new short[n];
		for (int i = 0; i < n; i++)
			array[i] = (short)input.readInt(true);
		return array;
	}

	private Animation readAnimation (SkeletonInput input, String name, SkeletonData skeletonData, boolean nonessential)
		throws IOException {
		var timelines = new Array<Timeline>(true, input.readInt(true), Timeline[]::new);
		float scale = this.scale;

		// Slot timelines.
		for (int i = 0, n = input.readInt(true); i < n; i++) {
			int slotIndex = input.readInt(true);
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				int timelineType = input.readByte(), frameCount = input.readInt(true), frameLast = frameCount - 1;
				switch (timelineType) {
				case SLOT_ATTACHMENT -> {
					var timeline = new AttachmentTimeline(frameCount, slotIndex);
					for (int frame = 0; frame < frameCount; frame++)
						timeline.setFrame(frame, input.readFloat(), input.readStringRef());
					timelines.add(timeline);
				}
				case SLOT_RGBA -> {
					var timeline = new RGBATimeline(frameCount, input.readInt(true), slotIndex);
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
						case CURVE_STEPPED -> timeline.setStepped(frame);
						case CURVE_BEZIER -> {
							setBezier(input, timeline, bezier++, frame, 0, time, time2, r, r2, 1);
							setBezier(input, timeline, bezier++, frame, 1, time, time2, g, g2, 1);
							setBezier(input, timeline, bezier++, frame, 2, time, time2, b, b2, 1);
							setBezier(input, timeline, bezier++, frame, 3, time, time2, a, a2, 1);
						}
						}
						time = time2;
						r = r2;
						g = g2;
						b = b2;
						a = a2;
					}
					timelines.add(timeline);
				}
				case SLOT_RGB -> {
					var timeline = new RGBTimeline(frameCount, input.readInt(true), slotIndex);
					float time = input.readFloat();
					float r = input.read() / 255f, g = input.read() / 255f, b = input.read() / 255f;
					for (int frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, r, g, b);
						if (frame == frameLast) break;
						float time2 = input.readFloat();
						float r2 = input.read() / 255f, g2 = input.read() / 255f, b2 = input.read() / 255f;
						switch (input.readByte()) {
						case CURVE_STEPPED -> timeline.setStepped(frame);
						case CURVE_BEZIER -> {
							setBezier(input, timeline, bezier++, frame, 0, time, time2, r, r2, 1);
							setBezier(input, timeline, bezier++, frame, 1, time, time2, g, g2, 1);
							setBezier(input, timeline, bezier++, frame, 2, time, time2, b, b2, 1);
						}
						}
						time = time2;
						r = r2;
						g = g2;
						b = b2;
					}
					timelines.add(timeline);
				}
				case SLOT_RGBA2 -> {
					var timeline = new RGBA2Timeline(frameCount, input.readInt(true), slotIndex);
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
						case CURVE_STEPPED -> timeline.setStepped(frame);
						case CURVE_BEZIER -> {
							setBezier(input, timeline, bezier++, frame, 0, time, time2, r, nr, 1);
							setBezier(input, timeline, bezier++, frame, 1, time, time2, g, ng, 1);
							setBezier(input, timeline, bezier++, frame, 2, time, time2, b, nb, 1);
							setBezier(input, timeline, bezier++, frame, 3, time, time2, a, na, 1);
							setBezier(input, timeline, bezier++, frame, 4, time, time2, r2, nr2, 1);
							setBezier(input, timeline, bezier++, frame, 5, time, time2, g2, ng2, 1);
							setBezier(input, timeline, bezier++, frame, 6, time, time2, b2, nb2, 1);
						}
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
				}
				case SLOT_RGB2 -> {
					var timeline = new RGB2Timeline(frameCount, input.readInt(true), slotIndex);
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
						case CURVE_STEPPED -> timeline.setStepped(frame);
						case CURVE_BEZIER -> {
							setBezier(input, timeline, bezier++, frame, 0, time, time2, r, nr, 1);
							setBezier(input, timeline, bezier++, frame, 1, time, time2, g, ng, 1);
							setBezier(input, timeline, bezier++, frame, 2, time, time2, b, nb, 1);
							setBezier(input, timeline, bezier++, frame, 3, time, time2, r2, nr2, 1);
							setBezier(input, timeline, bezier++, frame, 4, time, time2, g2, ng2, 1);
							setBezier(input, timeline, bezier++, frame, 5, time, time2, b2, nb2, 1);
						}
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
				}
				case SLOT_ALPHA -> {
					var timeline = new AlphaTimeline(frameCount, input.readInt(true), slotIndex);
					float time = input.readFloat(), a = input.read() / 255f;
					for (int frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, a);
						if (frame == frameLast) break;
						float time2 = input.readFloat();
						float a2 = input.read() / 255f;
						switch (input.readByte()) {
						case CURVE_STEPPED -> timeline.setStepped(frame);
						case CURVE_BEZIER -> setBezier(input, timeline, bezier++, frame, 0, time, time2, a, a2, 1);
						}
						time = time2;
						a = a2;
					}
					timelines.add(timeline);
				}
				}
			}
		}

		// Bone timelines.
		int boneCount = input.readInt(true);
		var bones = new IntArray(boneCount);
		for (int i = 0; i < boneCount; i++) {
			int boneIndex = input.readInt(true);
			bones.add(boneIndex);
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				int type = input.readByte(), frameCount = input.readInt(true);
				if (type == BONE_INHERIT) {
					var timeline = new InheritTimeline(frameCount, boneIndex);
					for (int frame = 0; frame < frameCount; frame++)
						timeline.setFrame(frame, input.readFloat(), Inherit.values[input.readByte()]);
					timelines.add(timeline);
					continue;
				}
				int bezierCount = input.readInt(true);
				switch (type) {
				case BONE_ROTATE -> readTimeline(input, timelines, new RotateTimeline(frameCount, bezierCount, boneIndex), 1);
				case BONE_TRANSLATE -> //
					readTimeline(input, timelines, new TranslateTimeline(frameCount, bezierCount, boneIndex), scale);
				case BONE_TRANSLATEX -> //
					readTimeline(input, timelines, new TranslateXTimeline(frameCount, bezierCount, boneIndex), scale);
				case BONE_TRANSLATEY -> //
					readTimeline(input, timelines, new TranslateYTimeline(frameCount, bezierCount, boneIndex), scale);
				case BONE_SCALE -> readTimeline(input, timelines, new ScaleTimeline(frameCount, bezierCount, boneIndex), 1);
				case BONE_SCALEX -> readTimeline(input, timelines, new ScaleXTimeline(frameCount, bezierCount, boneIndex), 1);
				case BONE_SCALEY -> readTimeline(input, timelines, new ScaleYTimeline(frameCount, bezierCount, boneIndex), 1);
				case BONE_SHEAR -> readTimeline(input, timelines, new ShearTimeline(frameCount, bezierCount, boneIndex), 1);
				case BONE_SHEARX -> readTimeline(input, timelines, new ShearXTimeline(frameCount, bezierCount, boneIndex), 1);
				case BONE_SHEARY -> readTimeline(input, timelines, new ShearYTimeline(frameCount, bezierCount, boneIndex), 1);
				}
			}
		}

		// IK constraint timelines.
		for (int i = 0, n = input.readInt(true); i < n; i++) {
			int index = input.readInt(true), frameCount = input.readInt(true), frameLast = frameCount - 1;
			var timeline = new IkConstraintTimeline(frameCount, input.readInt(true), index);
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
			var timeline = new TransformConstraintTimeline(frameCount, input.readInt(true), index);
			float time = input.readFloat(), mixRotate = input.readFloat(), mixX = input.readFloat(), mixY = input.readFloat(),
				mixScaleX = input.readFloat(), mixScaleY = input.readFloat(), mixShearY = input.readFloat();
			for (int frame = 0, bezier = 0;; frame++) {
				timeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
				if (frame == frameLast) break;
				float time2 = input.readFloat(), mixRotate2 = input.readFloat(), mixX2 = input.readFloat(), mixY2 = input.readFloat(),
					mixScaleX2 = input.readFloat(), mixScaleY2 = input.readFloat(), mixShearY2 = input.readFloat();
				switch (input.readByte()) {
				case CURVE_STEPPED -> timeline.setStepped(frame);
				case CURVE_BEZIER -> {
					setBezier(input, timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
					setBezier(input, timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
					setBezier(input, timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
					setBezier(input, timeline, bezier++, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
					setBezier(input, timeline, bezier++, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
					setBezier(input, timeline, bezier++, frame, 5, time, time2, mixShearY, mixShearY2, 1);
				}
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
			var data = (PathConstraintData)skeletonData.constraints.items[index];
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				int type = input.readByte(), frameCount = input.readInt(true), bezierCount = input.readInt(true);
				switch (type) {
				case PATH_POSITION -> {
					readTimeline(input, timelines, new PathConstraintPositionTimeline(frameCount, bezierCount, index),
						data.positionMode == PositionMode.fixed ? scale : 1);
				}
				case PATH_SPACING -> {
					readTimeline(input, timelines, new PathConstraintSpacingTimeline(frameCount, bezierCount, index),
						data.spacingMode == SpacingMode.length || data.spacingMode == SpacingMode.fixed ? scale : 1);
				}
				case PATH_MIX -> {
					var timeline = new PathConstraintMixTimeline(frameCount, bezierCount, index);
					float time = input.readFloat(), mixRotate = input.readFloat(), mixX = input.readFloat(), mixY = input.readFloat();
					for (int frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1;; frame++) {
						timeline.setFrame(frame, time, mixRotate, mixX, mixY);
						if (frame == frameLast) break;
						float time2 = input.readFloat(), mixRotate2 = input.readFloat(), mixX2 = input.readFloat(),
							mixY2 = input.readFloat();
						switch (input.readByte()) {
						case CURVE_STEPPED -> timeline.setStepped(frame);
						case CURVE_BEZIER -> {
							setBezier(input, timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
							setBezier(input, timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
							setBezier(input, timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
						}
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
		}

		// Physics timelines.
		for (int i = 0, n = input.readInt(true); i < n; i++) {
			int index = input.readInt(true) - 1;
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				int type = input.readByte(), frameCount = input.readInt(true);
				if (type == PHYSICS_RESET) {
					var timeline = new PhysicsConstraintResetTimeline(frameCount, index);
					for (int frame = 0; frame < frameCount; frame++)
						timeline.setFrame(frame, input.readFloat());
					timelines.add(timeline);
					continue;
				}
				int bezierCount = input.readInt(true);
				readTimeline(input, timelines, switch (type) {
				case PHYSICS_INERTIA -> new PhysicsConstraintInertiaTimeline(frameCount, bezierCount, index);
				case PHYSICS_STRENGTH -> new PhysicsConstraintStrengthTimeline(frameCount, bezierCount, index);
				case PHYSICS_DAMPING -> new PhysicsConstraintDampingTimeline(frameCount, bezierCount, index);
				case PHYSICS_MASS -> new PhysicsConstraintMassTimeline(frameCount, bezierCount, index);
				case PHYSICS_WIND -> new PhysicsConstraintWindTimeline(frameCount, bezierCount, index);
				case PHYSICS_GRAVITY -> new PhysicsConstraintGravityTimeline(frameCount, bezierCount, index);
				case PHYSICS_MIX -> new PhysicsConstraintMixTimeline(frameCount, bezierCount, index);
				default -> throw new SerializationException();
				}, 1);
			}
		}

		// Slider timelines.
		for (int i = 0, n = input.readInt(true); i < n; i++) {
			int index = input.readInt(true);
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				int type = input.readByte(), frameCount = input.readInt(true), bezierCount = input.readInt(true);
				readTimeline(input, timelines, switch (type) {
				case SLIDER_TIME -> new SliderTimeline(frameCount, bezierCount, index);
				case SLIDER_MIX -> new SliderMixTimeline(frameCount, bezierCount, index);
				default -> throw new SerializationException();
				}, 1);
			}
		}

		// Attachment timelines.
		for (int i = 0, n = input.readInt(true); i < n; i++) {
			Skin skin = skeletonData.skins.items[input.readInt(true)];
			for (int ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				int slotIndex = input.readInt(true);
				for (int iii = 0, nnn = input.readInt(true); iii < nnn; iii++) {
					String attachmentName = input.readStringRef();
					Attachment attachment = skin.getAttachment(slotIndex, attachmentName);
					if (attachment == null) throw new SerializationException("Timeline attachment not found: " + attachmentName);

					int timelineType = input.readByte(), frameCount = input.readInt(true), frameLast = frameCount - 1;
					switch (timelineType) {
					case ATTACHMENT_DEFORM -> {
						var vertexAttachment = (VertexAttachment)attachment;
						boolean weighted = vertexAttachment.getBones() != null;
						float[] vertices = vertexAttachment.getVertices();
						int deformLength = weighted ? (vertices.length / 3) << 1 : vertices.length;

						var timeline = new DeformTimeline(frameCount, input.readInt(true), slotIndex, vertexAttachment);

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
							case CURVE_STEPPED -> timeline.setStepped(frame);
							case CURVE_BEZIER -> setBezier(input, timeline, bezier++, frame, 0, time, time2, 0, 1, 1);
							}
							time = time2;
						}
						timelines.add(timeline);
					}
					case ATTACHMENT_SEQUENCE -> {
						var timeline = new SequenceTimeline(frameCount, slotIndex, attachment);
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
		}

		// Draw order timeline.
		int slotCount = skeletonData.slots.size;
		int drawOrderCount = input.readInt(true);
		if (drawOrderCount > 0) {
			var timeline = new DrawOrderTimeline(drawOrderCount);
			for (int i = 0; i < drawOrderCount; i++)
				timeline.setFrame(i, input.readFloat(), readDrawOrder(input, slotCount));
			timelines.add(timeline);
		}

		// Draw order folder timelines.
		int folderCount = input.readInt(true);
		for (int i = 0; i < folderCount; i++) {
			int folderSlotCount = input.readInt(true);
			var folderSlots = new int[folderSlotCount];
			for (int ii = 0; ii < folderSlotCount; ii++)
				folderSlots[ii] = input.readInt(true);
			int keyCount = input.readInt(true);
			var timeline = new DrawOrderFolderTimeline(keyCount, folderSlots, slotCount);
			for (int ii = 0; ii < keyCount; ii++)
				timeline.setFrame(ii, input.readFloat(), readDrawOrder(input, folderSlotCount));
			timelines.add(timeline);
		}

		// Event timeline.
		int eventCount = input.readInt(true);
		if (eventCount > 0) {
			var timeline = new EventTimeline(eventCount);
			for (int i = 0; i < eventCount; i++) {
				float time = input.readFloat();
				EventData eventData = skeletonData.events.items[input.readInt(true)];
				var event = new Event(time, eventData);
				event.intValue = input.readInt(false);
				event.floatValue = input.readFloat();
				event.stringValue = input.readString();
				if (event.stringValue == null) event.stringValue = eventData.setupPose.stringValue;
				if (event.data.audioPath != null) {
					event.volume = input.readFloat();
					event.balance = input.readFloat();
				}
				timeline.setFrame(i, event);
			}
			timelines.add(timeline);
		}

		float duration = 0;
		Timeline[] items = timelines.items;
		for (int i = 0, n = timelines.size; i < n; i++)
			duration = Math.max(duration, items[i].getDuration());

		Animation animation = new Animation(name);
		animation.setTimelines(timelines, bones);
		animation.setDuration(duration);
		if (nonessential) Color.rgba8888ToColor(animation.color, input.readInt());
		return animation;
	}

	private void readTimeline (SkeletonInput input, Array<Timeline> timelines, CurveTimeline1 timeline, float scale)
		throws IOException {
		float time = input.readFloat(), value = input.readFloat() * scale;
		for (int frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1;; frame++) {
			timeline.setFrame(frame, time, value);
			if (frame == frameLast) break;
			float time2 = input.readFloat(), value2 = input.readFloat() * scale;
			switch (input.readByte()) {
			case CURVE_STEPPED -> timeline.setStepped(frame);
			case CURVE_BEZIER -> setBezier(input, timeline, bezier++, frame, 0, time, time2, value, value2, scale);
			}
			time = time2;
			value = value2;
		}
		timelines.add(timeline);
	}

	private void readTimeline (SkeletonInput input, Array<Timeline> timelines, BoneTimeline2 timeline, float scale)
		throws IOException {
		float time = input.readFloat(), value1 = input.readFloat() * scale, value2 = input.readFloat() * scale;
		for (int frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1;; frame++) {
			timeline.setFrame(frame, time, value1, value2);
			if (frame == frameLast) break;
			float time2 = input.readFloat(), nvalue1 = input.readFloat() * scale, nvalue2 = input.readFloat() * scale;
			switch (input.readByte()) {
			case CURVE_STEPPED -> timeline.setStepped(frame);
			case CURVE_BEZIER -> {
				setBezier(input, timeline, bezier++, frame, 0, time, time2, value1, nvalue1, scale);
				setBezier(input, timeline, bezier++, frame, 1, time, time2, value2, nvalue2, scale);
			}
			}
			time = time2;
			value1 = nvalue1;
			value2 = nvalue2;
		}
		timelines.add(timeline);
	}

	private @Null int[] readDrawOrder (SkeletonInput input, int slotCount) throws IOException {
		int changeCount = input.readInt(true);
		if (changeCount == 0) return null;
		var drawOrder = new int[slotCount];
		Arrays.fill(drawOrder, -1);
		var unchanged = new int[slotCount - changeCount];
		int originalIndex = 0, unchangedIndex = 0;
		for (int i = 0; i < changeCount; i++) {
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
		for (int i = slotCount - 1; i >= 0; i--)
			if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
		return drawOrder;
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

		public @Null String readString () throws IOException {
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
				case -1 -> throw new EOFException();
				case 12, 13 -> {
					chars[charCount++] = (char)((b & 0x1F) << 6 | read() & 0x3F);
					i += 2;
				}
				case 14 -> {
					chars[charCount++] = (char)((b & 0x0F) << 12 | (read() & 0x3F) << 6 | read() & 0x3F);
					i += 3;
				}
				default -> {
					chars[charCount++] = (char)b;
					i++;
				}
				}
			}
			return new String(chars, 0, charCount);
		}
	}

	static class LinkedMesh {
		String source;
		int skinIndex, slotIndex, sourceIndex;
		MeshAttachment mesh;
		boolean inheritTimelines;

		public LinkedMesh (MeshAttachment mesh, int skinIndex, int slotIndex, int sourceIndex, String source,
			boolean inheritTimelines) {
			this.mesh = mesh;
			this.skinIndex = skinIndex;
			this.slotIndex = slotIndex;
			this.sourceIndex = sourceIndex;
			this.source = source;
			this.inheritTimelines = inheritTimelines;
		}
	}
}
