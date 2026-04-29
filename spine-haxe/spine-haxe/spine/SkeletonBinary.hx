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

package spine;

import spine.animation.SliderMixTimeline;
import spine.animation.SliderTimeline;
import spine.TransformConstraintData;
import spine.IkConstraintData.ScaleY;
import haxe.io.Bytes;
import StringTools;
import spine.animation.AlphaTimeline;
import spine.animation.Animation;
import spine.animation.AttachmentTimeline;
import spine.animation.BoneTimeline2;
import spine.animation.CurveTimeline1;
import spine.animation.CurveTimeline;
import spine.animation.DeformTimeline;
import spine.animation.DrawOrderFolderTimeline;
import spine.animation.DrawOrderTimeline;
import spine.animation.EventTimeline;
import spine.animation.IkConstraintTimeline;
import spine.animation.InheritTimeline;
import spine.animation.PathConstraintMixTimeline;
import spine.animation.PathConstraintPositionTimeline;
import spine.animation.PathConstraintSpacingTimeline;
import spine.animation.PhysicsConstraintDampingTimeline;
import spine.animation.PhysicsConstraintGravityTimeline;
import spine.animation.PhysicsConstraintInertiaTimeline;
import spine.animation.PhysicsConstraintMassTimeline;
import spine.animation.PhysicsConstraintMixTimeline;
import spine.animation.PhysicsConstraintResetTimeline;
import spine.animation.PhysicsConstraintStrengthTimeline;
import spine.animation.PhysicsConstraintWindTimeline;
import spine.animation.RGB2Timeline;
import spine.animation.RGBA2Timeline;
import spine.animation.RGBATimeline;
import spine.animation.RGBTimeline;
import spine.animation.RotateTimeline;
import spine.animation.ScaleTimeline;
import spine.animation.ScaleXTimeline;
import spine.animation.ScaleYTimeline;
import spine.animation.SequenceTimeline;
import spine.animation.ShearTimeline;
import spine.animation.ShearXTimeline;
import spine.animation.ShearYTimeline;
import spine.animation.Timeline;
import spine.animation.TransformConstraintTimeline;
import spine.animation.TranslateTimeline;
import spine.animation.TranslateXTimeline;
import spine.animation.TranslateYTimeline;
import spine.attachments.Attachment;
import spine.attachments.AttachmentLoader;
import spine.attachments.AttachmentType;
import spine.attachments.BoundingBoxAttachment;
import spine.attachments.ClippingAttachment;
import spine.attachments.MeshAttachment;
import spine.attachments.PathAttachment;
import spine.attachments.PointAttachment;
import spine.attachments.RegionAttachment;
import spine.attachments.VertexAttachment;

/**
 * Loads skeleton data in the Spine binary format.
 *
 * @see https://esotericsoftware.com/spine-binary-format Spine binary format
 * @see https://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data JSON and binary data in the Spine Runtimes Guide
 */
class SkeletonBinary {
	public var attachmentLoader:AttachmentLoader;
	public var scale:Float = 1;

	private var linkedMeshes:Array<LinkedMeshBinary> = new Array<LinkedMeshBinary>();

	private static inline var BONE_ROTATE:Int = 0;
	private static inline var BONE_TRANSLATE:Int = 1;
	private static inline var BONE_TRANSLATEX:Int = 2;
	private static inline var BONE_TRANSLATEY:Int = 3;
	private static inline var BONE_SCALE:Int = 4;
	private static inline var BONE_SCALEX:Int = 5;
	private static inline var BONE_SCALEY:Int = 6;
	private static inline var BONE_SHEAR:Int = 7;
	private static inline var BONE_SHEARX:Int = 8;
	private static inline var BONE_SHEARY:Int = 9;
	private static inline var BONE_INHERIT:Int = 10;

	private static inline var SLOT_ATTACHMENT:Int = 0;
	private static inline var SLOT_RGBA:Int = 1;
	private static inline var SLOT_RGB:Int = 2;
	private static inline var SLOT_RGBA2:Int = 3;
	private static inline var SLOT_RGB2:Int = 4;
	private static inline var SLOT_ALPHA:Int = 5;

	private static inline var CONSTRAINT_IK = 0;
	private static inline var CONSTRAINT_PATH = 1;
	private static inline var CONSTRAINT_TRANSFORM = 2;
	private static inline var CONSTRAINT_PHYSICS = 3;
	private static inline var CONSTRAINT_SLIDER = 4;

	private static inline var ATTACHMENT_DEFORM = 0;
	private static inline var ATTACHMENT_SEQUENCE = 1;

	private static inline var PATH_POSITION:Int = 0;
	private static inline var PATH_SPACING:Int = 1;
	private static inline var PATH_MIX:Int = 2;

	private static inline var PHYSICS_INERTIA:Int = 0;
	private static inline var PHYSICS_STRENGTH:Int = 1;
	private static inline var PHYSICS_DAMPING:Int = 2;
	private static inline var PHYSICS_MASS:Int = 4;
	private static inline var PHYSICS_WIND:Int = 5;
	private static inline var PHYSICS_GRAVITY:Int = 6;
	private static inline var PHYSICS_MIX:Int = 7;
	private static inline var PHYSICS_RESET:Int = 8;

	private static inline var SLIDER_TIME = 0;
	private static inline var SLIDER_MIX = 1;

	private static inline var CURVE_LINEAR:Int = 0;
	private static inline var CURVE_STEPPED:Int = 1;
	private static inline var CURVE_BEZIER:Int = 2;

	public function new(attachmentLoader:AttachmentLoader) {
		this.attachmentLoader = attachmentLoader;
	}

	public function readSkeletonData(bytes:Bytes):SkeletonData {
		// bytes.getData(). = Endian.BIG_ENDIAN;

		var skeletonData:SkeletonData = new SkeletonData();
		skeletonData.name = null;

		var input:BinaryInput = new BinaryInput(bytes);

		var lowHash:Int = input.readInt32();
		var highHash:Int = input.readInt32();
		skeletonData.hash = highHash == 0 && lowHash == 0 ? null : StringTools.hex(highHash) + StringTools.hex(lowHash);
		skeletonData.version = input.readString();
		skeletonData.x = input.readFloat();
		skeletonData.y = input.readFloat();
		skeletonData.width = input.readFloat();
		skeletonData.height = input.readFloat();
		skeletonData.referenceScale = input.readFloat() * scale;

		var nonessential:Bool = input.readBoolean();
		if (nonessential) {
			skeletonData.fps = input.readFloat();
			skeletonData.imagesPath = input.readString();
			skeletonData.audioPath = input.readString();
		}

		var n:Int = 0;
		var nn:Int = 0;

		// Strings.
		n = input.readInt(true);
		for (i in 0...n) {
			input.strings.push(input.readString());
		}

		// Bones.
		var bones = skeletonData.bones;
		n = input.readInt(true);
		for (i in 0...n) {
			var boneName = input.readString();
			var boneParent:BoneData = i == 0 ? null : bones[input.readInt(true)];
			var data = new BoneData(i, boneName, boneParent);
			var setup = data.setupPose;
			setup.rotation = input.readFloat();
			setup.x = input.readFloat() * scale;
			setup.y = input.readFloat() * scale;
			setup.scaleX = input.readFloat();
			setup.scaleY = input.readFloat();
			setup.shearX = input.readFloat();
			setup.shearY = input.readFloat();
			setup.inherit = Inherit.values[input.readInt(true)];
			data.length = input.readFloat() * scale;
			data.skinRequired = input.readBoolean();
			if (nonessential) {
				data.color.setFromRgba8888(input.readInt32());
				data.icon = input.readString();
				data.iconSize = input.readFloat();
				data.iconRotation = input.readFloat();
				data.visible = input.readBoolean();
			}
			bones.push(data);
		}

		// Slots.
		var slots = skeletonData.slots;
		n = input.readInt(true);
		for (i in 0...n) {
			var slotName = input.readString();

			var boneData = bones[input.readInt(true)];
			var data = new SlotData(i, slotName, boneData);
			data.setupPose.color.setFromRgba8888(input.readInt32());

			var darkColor = input.readInt32();
			if (darkColor != -1)
				data.setupPose.darkColor = new Color(0, 0, 0).setFromRgb888(darkColor);

			data.attachmentName = input.readStringRef();
			data.blendMode = BlendMode.values[input.readInt(true)];
			if (nonessential)
				data.visible = input.readBoolean();
			slots.push(data);
		}

		// Constraints.
		var constraintCount = input.readInt(true);
		var constraints = skeletonData.constraints;
		for (i in 0...constraintCount) {
			var name = input.readString();
			var nn;
			switch (input.readByte()) {
				case CONSTRAINT_IK:
					var data = new IkConstraintData(name);
					nn = input.readInt(true);
					var constraintBones = data.bones;
					for (ii in 0...nn)
						constraintBones.push(bones[input.readInt(true)]);
					data.target = bones[input.readInt(true)];
					var flags = input.readByte();
					data.skinRequired = (flags & 1) != 0;
					if ((flags & 2) != 0)
						data.scaleY = ScaleY.values[input.readUnsignedByte()];
					var setup = data.setupPose;
					setup.bendDirection = (flags & 4) != 0 ? -1 : 1;
					setup.compress = (flags & 8) != 0;
					setup.stretch = (flags & 16) != 0;
					if ((flags & 32) != 0)
						setup.mix = (flags & 64) != 0 ? input.readFloat() : 1;
					if ((flags & 128) != 0)
						setup.softness = input.readFloat() * scale;
					constraints[i] = data;
				case CONSTRAINT_TRANSFORM:
					var data = new TransformConstraintData(name);
					nn = input.readInt(true);
					var constraintBones = data.bones;
					for (ii in 0...nn)
						constraintBones.push(bones[input.readInt(true)]);
					data.source = bones[input.readInt(true)];
					var flags = input.readUnsignedByte();
					data.skinRequired = (flags & 1) != 0;
					data.localSource = (flags & 2) != 0;
					data.localTarget = (flags & 4) != 0;
					data.additive = (flags & 8) != 0;
					data.clamp = (flags & 16) != 0;
					nn = flags >> 5;
					var froms = data.properties;
					for (ii in 0...nn) {
						var fromScale = 1.;
						var from:FromProperty;
						switch (input.readByte()) {
							case 0: from = new FromRotate();
							case 1:
								fromScale = scale;
								from = new FromX();
							case 2:
								fromScale = scale;
								from = new FromY();
							case 3: from = new FromScaleX();
							case 4: from = new FromScaleY();
							case 5: from = new FromShearY();
							default: from = null;
						}
						from.offset = input.readFloat() * fromScale;
						var tn = input.readByte();
						var tos = from.to;
						for (t in 0...tn) {
							var toScale = 1.;
							var to:ToProperty;
							switch (input.readByte()) {
								case 0: to = new ToRotate();
								case 1:
									toScale = scale;
									to = new ToX();
								case 2:
									toScale = scale;
									to = new ToY();
								case 3: to = new ToScaleX();
								case 4: to = new ToScaleY();
								case 5: to = new ToShearY();
								default: to = null;
							}
							to.offset = input.readFloat() * toScale;
							to.max = input.readFloat() * toScale;
							to.scale = input.readFloat() * toScale / fromScale;
							tos[t] = to;
						}
						froms[ii] = from;
					}
					flags = input.readByte();
					if ((flags & 1) != 0)
						data.offsets[TransformConstraintData.ROTATION] = input.readFloat();
					if ((flags & 2) != 0)
						data.offsets[TransformConstraintData.X] = input.readFloat() * scale;
					if ((flags & 4) != 0)
						data.offsets[TransformConstraintData.Y] = input.readFloat() * scale;
					if ((flags & 8) != 0)
						data.offsets[TransformConstraintData.SCALEX] = input.readFloat();
					if ((flags & 16) != 0)
						data.offsets[TransformConstraintData.SCALEY] = input.readFloat();
					if ((flags & 32) != 0)
						data.offsets[TransformConstraintData.SHEARY] = input.readFloat();
					flags = input.readByte();
					var setup = data.setupPose;
					if ((flags & 1) != 0)
						setup.mixRotate = input.readFloat();
					if ((flags & 2) != 0)
						setup.mixX = input.readFloat();
					if ((flags & 4) != 0)
						setup.mixY = input.readFloat();
					if ((flags & 8) != 0)
						setup.mixScaleX = input.readFloat();
					if ((flags & 16) != 0)
						setup.mixScaleY = input.readFloat();
					if ((flags & 32) != 0)
						setup.mixShearY = input.readFloat();
					constraints[i] = data;
				case CONSTRAINT_PATH:
					var data = new PathConstraintData(name);
					nn = input.readInt(true);
					var constraintBones = data.bones;
					for (ii in 0...nn)
						constraintBones[ii] = bones[input.readInt(true)];
					data.slot = slots[input.readInt(true)];
					var flags = input.readByte();
					data.skinRequired = (flags & 1) != 0;
					data.positionMode = PositionMode.values[(flags >> 1) & 1];
					data.spacingMode = SpacingMode.values[(flags >> 2) & 3];
					data.rotateMode = RotateMode.values[(flags >> 4) & 3];
					if ((flags & 128) != 0)
						data.offsetRotation = input.readFloat();
					var setup = data.setupPose;
					setup.position = input.readFloat();
					if (data.positionMode == PositionMode.fixed)
						setup.position *= scale;
					setup.spacing = input.readFloat();
					if (data.spacingMode == SpacingMode.length || data.spacingMode == SpacingMode.fixed)
						setup.spacing *= scale;
					setup.mixRotate = input.readFloat();
					setup.mixX = input.readFloat();
					setup.mixY = input.readFloat();
					constraints[i] = data;
				case CONSTRAINT_PHYSICS:
					var data = new PhysicsConstraintData(name);
					data.bone = bones[input.readInt(true)];
					var flags = input.readByte();
					data.skinRequired = (flags & 1) != 0;
					if ((flags & 2) != 0)
						data.x = input.readFloat();
					if ((flags & 4) != 0)
						data.y = input.readFloat();
					if ((flags & 8) != 0)
						data.rotate = input.readFloat();
					if ((flags & 16) != 0)
						data.scaleX = input.readFloat();
					if ((flags & 32) != 0)
						data.shearX = input.readFloat();
					data.limit = ((flags & 64) != 0 ? input.readFloat() : 5000) * scale;
					data.step = .1 / input.readUnsignedByte();
					var setup = data.setupPose;
					setup.inertia = input.readFloat();
					setup.strength = input.readFloat();
					setup.damping = input.readFloat();
					setup.massInverse = (flags & 128) != 0 ? input.readFloat() : 1;
					setup.wind = input.readFloat();
					setup.gravity = input.readFloat();
					flags = input.readByte();
					if ((flags & 1) != 0)
						data.inertiaGlobal = true;
					if ((flags & 2) != 0)
						data.strengthGlobal = true;
					if ((flags & 4) != 0)
						data.dampingGlobal = true;
					if ((flags & 8) != 0)
						data.massGlobal = true;
					if ((flags & 16) != 0)
						data.windGlobal = true;
					if ((flags & 32) != 0)
						data.gravityGlobal = true;
					if ((flags & 64) != 0)
						data.mixGlobal = true;
					setup.mix = (flags & 128) != 0 ? input.readFloat() : 1;
					constraints[i] = data;
				case CONSTRAINT_SLIDER:
					var data = new SliderData(name);
					var flags = input.readByte();
					data.skinRequired = (flags & 1) != 0;
					data.loop = (flags & 2) != 0;
					data.additive = (flags & 4) != 0;
					if ((flags & 8) != 0)
						data.setupPose.time = input.readFloat();
					if ((flags & 16) != 0)
						data.setupPose.mix = (flags & 32) != 0 ? input.readFloat() : 1;
					if ((flags & 64) != 0) {
						data.local = (flags & 128) != 0;
						data.bone = bones[input.readInt(true)];
						var offset = input.readFloat();
						var propertyScale = 1.;
						switch (input.readByte()) {
							case 0: data.property = new FromRotate();
							case 1:
								propertyScale = scale;
								data.property = new FromX();
							case 2:
								propertyScale = scale;
								data.property = new FromY();
							case 3: data.property = new FromScaleX();
							case 4: data.property = new FromScaleY();
							case 5: data.property = new FromShearY();
							default: data.property = null;
						}
						data.property.offset = offset * propertyScale;
						data.offset = input.readFloat();
						data.scale = input.readFloat() / propertyScale;
					}
					constraints[i] = data;
			}
		}

		// Default skin.
		var defaultSkin:Skin = readSkin(input, skeletonData, true, nonessential);
		if (defaultSkin != null) {
			skeletonData.defaultSkin = defaultSkin;
			skeletonData.skins.push(defaultSkin);
		}

		// Skins.
		{
			var i:Int = skeletonData.skins.length;
			n = i + input.readInt(true);
			while (i < n) {
				skeletonData.skins.push(readSkin(input, skeletonData, false, nonessential));
				i++;
			}
		}

		// Linked meshes.
		for (linkedMesh in linkedMeshes) {
			var skin:Skin = skeletonData.skins[linkedMesh.skinIndex];
			var source:Attachment = skin.getAttachment(linkedMesh.sourceIndex, linkedMesh.source);
			if (source == null)
				throw new SpineException("Source mesh not found: " + linkedMesh.source);
			linkedMesh.mesh.timelineAttachment = linkedMesh.inheritTimelines ? source : linkedMesh.mesh;
			linkedMesh.mesh.sourceMesh = cast(source, MeshAttachment);
			linkedMesh.mesh.updateSequence();
		}
		linkedMeshes.resize(0);

		// Events.
		n = input.readInt(true);
		for (i in 0...n) {
			var data:EventData = new EventData(input.readString());
			var setup = data.setupPose;
			setup.intValue = input.readInt(false);
			setup.floatValue = input.readFloat();
			setup.stringValue = input.readString();
			data.audioPath = input.readString();
			if (data.audioPath != null) {
				setup.volume = input.readFloat();
				setup.balance = input.readFloat();
			}
			skeletonData.events.push(data);
		}

		// Animations.
		var animations = skeletonData.animations;
		n = input.readInt(true);
		for (i in 0...n)
			animations[i] = readAnimation(input, input.readString(), skeletonData, nonessential);

		for (i in 0...constraintCount)
			if (Std.isOfType(constraints[i], SliderData)) {
				var data = cast(constraints[i], SliderData);
				data.animation = animations[input.readInt(true)];
			};
		return skeletonData;
	}

	private function readSkin(input:BinaryInput, skeletonData:SkeletonData, defaultSkin:Bool, nonessential:Bool):Skin {
		var skin:Skin = null;
		var slotCount = 0;

		if (defaultSkin) {
			slotCount = input.readInt(true);
			if (slotCount == 0)
				return null;
			skin = new Skin("default");
		} else {
			skin = new Skin(input.readString());

			if (nonessential)
				skin.color.setFromRgba8888(input.readInt32());

			var n:Int;
			var from1 = skeletonData.bones;
			var to1 = skin.bones;
			to1.resize(n = input.readInt(true));
			for (i in 0...n)
				to1[i] = from1[input.readInt(true)];

			var from2 = skeletonData.constraints;
			var to2 = skin.constraints;
			to2.resize(n = input.readInt(true));
			for (i in 0...n)
				to2[i] = from2[input.readInt(true)];

			slotCount = input.readInt(true);
		}

		for (i in 0...slotCount) {
			var slotIndex:Int = input.readInt(true);
			for (ii in 0...input.readInt(true)) {
				var name:String = input.readStringRef();
				if (name == null)
					throw new SpineException("Attachment name must not be null");
				var attachment:Attachment = readAttachment(input, skeletonData, skin, slotIndex, name, nonessential);
				if (attachment != null)
					skin.setAttachment(slotIndex, name, attachment);
			}
		}
		return skin;
	}

	private function readSequence(input:BinaryInput, hasPathSuffix:Bool):Sequence {
		if (!hasPathSuffix)
			return new Sequence(1, false);
		var sequence = new Sequence(input.readInt(true), true);
		sequence.start = input.readInt(true);
		sequence.digits = input.readInt(true);
		sequence.setupIndex = input.readInt(true);
		return sequence;
	}

	private function readAttachment(input:BinaryInput, skeletonData:SkeletonData, skin:Skin, slotIndex:Int, attachmentName:String,
			nonessential:Bool):Attachment {
		var vertices:Vertices;
		var path:String;
		var rotation:Float;
		var x:Float;
		var y:Float;
		var scaleX:Float;
		var scaleY:Float;
		var width:Float = 0;
		var height:Float = 0;
		var color:Int;
		var mesh:MeshAttachment;

		var flags = input.readByte();
		var name = (flags & 8) != 0 ? input.readStringRef() : attachmentName;
		switch (AttachmentType.values[flags & 7]) {
			case AttachmentType.region:
				path = (flags & 16) != 0 ? input.readStringRef() : null;
				color = (flags & 32) != 0 ? input.readInt32() : 0xffffffff;
				var sequence = readSequence(input, (flags & 64) != 0);
				rotation = (flags & 128) != 0 ? input.readFloat() : 0;
				x = input.readFloat();
				y = input.readFloat();
				scaleX = input.readFloat();
				scaleY = input.readFloat();
				width = input.readFloat();
				height = input.readFloat();

				if (path == null)
					path = name;
				var region:RegionAttachment = attachmentLoader.newRegionAttachment(skin, name, path, sequence);
				if (region == null)
					return null;
				region.path = path;
				region.x = x * scale;
				region.y = y * scale;
				region.scaleX = scaleX;
				region.scaleY = scaleY;
				region.rotation = rotation;
				region.width = width * scale;
				region.height = height * scale;
				region.color.setFromRgba8888(color);
				region.updateSequence();
				return region;
			case AttachmentType.boundingbox:
				vertices = readVertices(input, (flags & 16) != 0);
				color = nonessential ? input.readInt32() : 0;

				var box:BoundingBoxAttachment = attachmentLoader.newBoundingBoxAttachment(skin, name);
				if (box == null)
					return null;
				box.worldVerticesLength = vertices.length;
				box.vertices = vertices.vertices;
				if (vertices.bones.length > 0)
					box.bones = vertices.bones;
				if (nonessential)
					box.color.setFromRgba8888(color);
				return box;
			case AttachmentType.mesh:
				path = (flags & 16) != 0 ? input.readStringRef() : name;
				color = (flags & 32) != 0 ? input.readInt32() : 0xffffffff;
				var sequence = readSequence(input, (flags & 64) != 0);
				var hullLength = input.readInt(true);
				vertices = readVertices(input, (flags & 128) != 0);
				var uvs:Array<Float> = readFloatArray(input, vertices.length, 1);
				var triangles:Array<Int> = readShortArray(input, (vertices.length - hullLength - 2) * 3);

				var slotCount:Int = input.readInt(true);
				var timelineSlots:Array<Int> = null;
				if (slotCount > 0) {
					timelineSlots = new Array<Int>();
					timelineSlots.resize(slotCount);
					for (i in 0...slotCount)
						timelineSlots[i] = input.readInt(true);
				}

				var edges:Array<Int> = null;
				if (nonessential) {
					edges = readShortArray(input, input.readInt(true));
					width = input.readFloat();
					height = input.readFloat();
				}

				if (path == null)
					path = name;
				mesh = attachmentLoader.newMeshAttachment(skin, name, path, sequence);
				if (mesh == null)
					return null;
				mesh.path = path;
				mesh.color.setFromRgba8888(color);
				mesh.hullLength = hullLength << 1;
				if (vertices.bones.length > 0)
					mesh.bones = vertices.bones;
				mesh.vertices = vertices.vertices;
				mesh.worldVerticesLength = vertices.length;
				mesh.regionUVs = uvs;
				mesh.triangles = triangles;
				if (timelineSlots != null)
					mesh.timelineSlots = timelineSlots;
				if (nonessential) {
					mesh.edges = edges;
					mesh.width = width * scale;
					mesh.height = height * scale;
				}
				mesh.updateSequence();
				return mesh;
			case AttachmentType.linkedmesh:
				path = (flags & 16) != 0 ? input.readStringRef() : name;
				if (path == null)
					throw new SpineException("Path of linked mesh must not be null");
				color = (flags & 32) != 0 ? input.readInt32() : 0xffffffff;
				var sequence = readSequence(input, (flags & 64) != 0);
				var inheritTimelines:Bool = (flags & 128) != 0;
				var sourceIndex = input.readInt(true);
				var skinIndex = input.readInt(true);
				var source:String = input.readStringRef();
				if (nonessential) {
					width = input.readFloat();
					height = input.readFloat();
				}

				mesh = attachmentLoader.newMeshAttachment(skin, name, path, sequence);
				if (mesh == null)
					return null;
				mesh.path = path;
				mesh.color.setFromRgba8888(color);
				if (nonessential) {
					mesh.width = width * scale;
					mesh.height = height * scale;
				}
				this.linkedMeshes.push(new LinkedMeshBinary(mesh, skinIndex, slotIndex, sourceIndex, source, inheritTimelines));
				return mesh;
			case AttachmentType.path:
				var closed:Bool = (flags & 16) != 0;
				var constantSpeed:Bool = (flags & 32) != 0;
				vertices = readVertices(input, (flags & 64) != 0);
				var lengths:Array<Float> = readFloatArray(input, Std.int(vertices.length / 6), scale);
				color = nonessential ? input.readInt32() : 0;

				var pathAttachment:PathAttachment = attachmentLoader.newPathAttachment(skin, name);
				if (pathAttachment == null)
					return null;
				pathAttachment.closed = closed;
				pathAttachment.constantSpeed = constantSpeed;
				pathAttachment.worldVerticesLength = vertices.length;
				pathAttachment.vertices = vertices.vertices;
				if (vertices.bones.length > 0)
					pathAttachment.bones = vertices.bones;
				pathAttachment.lengths = lengths;
				if (nonessential)
					pathAttachment.color.setFromRgba8888(color);
				return pathAttachment;
			case AttachmentType.point:
				rotation = input.readFloat();
				x = input.readFloat();
				y = input.readFloat();
				color = nonessential ? input.readInt32() : 0;

				var point:PointAttachment = attachmentLoader.newPointAttachment(skin, name);
				if (point == null)
					return null;
				point.x = x * scale;
				point.y = y * scale;
				point.rotation = rotation;
				if (nonessential)
					point.color.setFromRgba8888(color);
				return point;
			case AttachmentType.clipping:
				var endSlotIndex:Int = input.readInt(true);
				vertices = readVertices(input, (flags & 16) != 0);
				color = nonessential ? input.readInt32() : 0;

				var clip:ClippingAttachment = attachmentLoader.newClippingAttachment(skin, name);
				if (clip == null)
					return null;
				clip.endSlot = skeletonData.slots[endSlotIndex];
				clip.convex = (flags & 32) != 0;
				clip.inverse = (flags & 64) != 0;
				clip.worldVerticesLength = vertices.length;
				clip.vertices = vertices.vertices;
				if (vertices.bones.length > 0)
					clip.bones = vertices.bones;
				if (nonessential)
					clip.color.setFromRgba8888(color);
				return clip;
		}
		return null;
	}

	private function readVertices(input:BinaryInput, weighted:Bool):Vertices {
		var vertexCount:Int = input.readInt(true);
		var vertices:Vertices = new Vertices();
		vertices.length = vertexCount << 1;
		if (!weighted) {
			vertices.vertices = readFloatArray(input, vertices.length, scale);
			return vertices;
		}
		var n:Int = input.readInt(true);
		var bones:Array<Int> = new Array<Int>();
		var weights:Array<Float> = new Array<Float>();
		var b:Int = 0, w:Int = 0;
		while (b < n) {
			var boneCount:Int = input.readInt(true);
			bones[b++] = boneCount;
			for (ii in 0...boneCount) {
				bones[b++] = input.readInt(true);
				weights[w] = input.readFloat() * scale;
				weights[w + 1] = input.readFloat() * scale;
				weights[w + 2] = input.readFloat();
				w += 3;
			}
		}
		vertices.vertices = weights;
		vertices.bones = bones;
		return vertices;
	}

	private function readDrawOrder(input:BinaryInput, slotCount:Int):Array<Int> {
		var changeCount:Int = input.readInt(true);
		if (changeCount == 0)
			return null;
		var drawOrder:Array<Int> = new Array<Int>();
		drawOrder.resize(slotCount);
		for (i in 0...slotCount)
			drawOrder[i] = -1;
		var unchanged:Array<Int> = new Array<Int>();
		unchanged.resize(slotCount - changeCount);
		var originalIndex:Int = 0, unchangedIndex:Int = 0;
		for (i in 0...changeCount) {
			var slotIndex:Int = input.readInt(true);
			while (originalIndex != slotIndex)
				unchanged[unchangedIndex++] = originalIndex++;
			drawOrder[originalIndex + input.readInt(true)] = originalIndex++;
		}
		while (originalIndex < slotCount)
			unchanged[unchangedIndex++] = originalIndex++;
		var i:Int = slotCount - 1;
		while (i >= 0) {
			if (drawOrder[i] == -1)
				drawOrder[i] = unchanged[--unchangedIndex];
			i--;
		}
		return drawOrder;
	}

	private function readFloatArray(input:BinaryInput, n:Int, scale:Float):Array<Float> {
		var array:Array<Float> = new Array<Float>();
		if (scale == 1) {
			for (i in 0...n) {
				array.push(input.readFloat());
			}
		} else {
			for (i in 0...n) {
				array.push(input.readFloat() * scale);
			}
		}
		return array;
	}

	private function readShortArray(input:BinaryInput, n:Int):Array<Int> {
		var array:Array<Int> = new Array<Int>();
		for (i in 0...n) {
			array.push(input.readInt(true));
		}
		return array;
	}

	private function readAnimation(input:BinaryInput, name:String, skeletonData:SkeletonData, nonessential:Bool):Animation {
		input.readInt(true); // Count of timelines.
		var timelines:Array<Timeline> = new Array<Timeline>();
		var i:Int = 0, n:Int = 0, ii:Int = 0, nn:Int = 0;

		var index:Int, slotIndex:Int, timelineType:Int, timelineScale:Float;
		var frameCount:Int, frameLast:Int, frame:Int, bezierCount:Int, bezier:Int;
		var time:Float, time2:Float;

		// Slot timelines.
		var r:Float, g:Float, b:Float, a:Float;
		var r2:Float, g2:Float, b2:Float, a2:Float;
		var nr:Float, ng:Float, nb:Float, na:Float;
		var nr2:Float, ng2:Float, nb2:Float, na2:Float;
		for (i in 0...input.readInt(true)) {
			slotIndex = input.readInt(true);
			for (ii in 0...input.readInt(true)) {
				timelineType = input.readByte();
				frameCount = input.readInt(true);
				frameLast = frameCount - 1;
				switch (timelineType) {
					case SkeletonBinary.SLOT_ATTACHMENT:
						var attachmentTimeline:AttachmentTimeline = new AttachmentTimeline(frameCount, slotIndex);
						attachmentTimeline.slotIndex = slotIndex;
						for (frame in 0...frameCount) {
							attachmentTimeline.setFrame(frame, input.readFloat(), input.readStringRef());
						}
						timelines.push(attachmentTimeline);
					case SLOT_RGBA:
						bezierCount = input.readInt(true);
						var rgbaTimeline:RGBATimeline = new RGBATimeline(frameCount, bezierCount, slotIndex);

						time = input.readFloat();
						r = input.readUnsignedByte() / 255.0;
						g = input.readUnsignedByte() / 255.0;
						b = input.readUnsignedByte() / 255.0;
						a = input.readUnsignedByte() / 255.0;

						frame = 0;
						bezier = 0;
						while (true) {
							rgbaTimeline.setFrame(frame, time, r, g, b, a);
							if (frame == frameLast)
								break;

							time2 = input.readFloat();
							r2 = input.readUnsignedByte() / 255.0;
							g2 = input.readUnsignedByte() / 255.0;
							b2 = input.readUnsignedByte() / 255.0;
							a2 = input.readUnsignedByte() / 255.0;

							switch (input.readByte()) {
								case CURVE_STEPPED:
									rgbaTimeline.setStepped(frame);
								case CURVE_BEZIER:
									setBezier(input, rgbaTimeline, bezier++, frame, 0, time, time2, r, r2, 1);
									setBezier(input, rgbaTimeline, bezier++, frame, 1, time, time2, g, g2, 1);
									setBezier(input, rgbaTimeline, bezier++, frame, 2, time, time2, b, b2, 1);
									setBezier(input, rgbaTimeline, bezier++, frame, 3, time, time2, a, a2, 1);
							}
							time = time2;
							r = r2;
							g = g2;
							b = b2;
							a = a2;

							frame++;
						}
						timelines.push(rgbaTimeline);
					case SLOT_RGB:
						bezierCount = input.readInt(true);
						var rgbTimeline:RGBTimeline = new RGBTimeline(frameCount, bezierCount, slotIndex);

						time = input.readFloat();
						r = input.readUnsignedByte() / 255.0;
						g = input.readUnsignedByte() / 255.0;
						b = input.readUnsignedByte() / 255.0;

						frame = 0;
						bezier = 0;
						while (true) {
							rgbTimeline.setFrame(frame, time, r, g, b);
							if (frame == frameLast)
								break;

							time2 = input.readFloat();
							r2 = input.readUnsignedByte() / 255.0;
							g2 = input.readUnsignedByte() / 255.0;
							b2 = input.readUnsignedByte() / 255.0;

							switch (input.readByte()) {
								case CURVE_STEPPED:
									rgbTimeline.setStepped(frame);
								case CURVE_BEZIER:
									setBezier(input, rgbTimeline, bezier++, frame, 0, time, time2, r, r2, 1);
									setBezier(input, rgbTimeline, bezier++, frame, 1, time, time2, g, g2, 1);
									setBezier(input, rgbTimeline, bezier++, frame, 2, time, time2, b, b2, 1);
							}
							time = time2;
							r = r2;
							g = g2;
							b = b2;

							frame++;
						}
						timelines.push(rgbTimeline);
					case SLOT_RGBA2:
						bezierCount = input.readInt(true);
						var rgba2Timeline:RGBA2Timeline = new RGBA2Timeline(frameCount, bezierCount, slotIndex);

						time = input.readFloat();
						r = input.readUnsignedByte() / 255.0;
						g = input.readUnsignedByte() / 255.0;
						b = input.readUnsignedByte() / 255.0;
						a = input.readUnsignedByte() / 255.0;
						r2 = input.readUnsignedByte() / 255.0;
						g2 = input.readUnsignedByte() / 255.0;
						b2 = input.readUnsignedByte() / 255.0;

						frame = 0;
						bezier = 0;
						while (true) {
							rgba2Timeline.setFrame(frame, time, r, g, b, a, r2, g2, b2);
							if (frame == frameLast)
								break;

							time2 = input.readFloat();
							nr = input.readUnsignedByte() / 255.0;
							ng = input.readUnsignedByte() / 255.0;
							nb = input.readUnsignedByte() / 255.0;
							na = input.readUnsignedByte() / 255.0;
							nr2 = input.readUnsignedByte() / 255.0;
							ng2 = input.readUnsignedByte() / 255.0;
							nb2 = input.readUnsignedByte() / 255.0;

							switch (input.readByte()) {
								case CURVE_STEPPED:
									rgba2Timeline.setStepped(frame);
								case CURVE_BEZIER:
									setBezier(input, rgba2Timeline, bezier++, frame, 0, time, time2, r, nr, 1);
									setBezier(input, rgba2Timeline, bezier++, frame, 1, time, time2, g, ng, 1);
									setBezier(input, rgba2Timeline, bezier++, frame, 2, time, time2, b, nb, 1);
									setBezier(input, rgba2Timeline, bezier++, frame, 3, time, time2, a, na, 1);
									setBezier(input, rgba2Timeline, bezier++, frame, 4, time, time2, r2, nr2, 1);
									setBezier(input, rgba2Timeline, bezier++, frame, 5, time, time2, g2, ng2, 1);
									setBezier(input, rgba2Timeline, bezier++, frame, 6, time, time2, b2, nb2, 1);
							}
							time = time2;
							r = nr;
							g = ng;
							b = nb;
							a = na;
							r2 = nr2;
							g2 = ng2;
							b2 = nb2;

							frame++;
						}
						timelines.push(rgba2Timeline);
					case SLOT_RGB2:
						bezierCount = input.readInt(true);
						var rgb2Timeline:RGB2Timeline = new RGB2Timeline(frameCount, bezierCount, slotIndex);

						time = input.readFloat();
						r = input.readUnsignedByte() / 255.0;
						g = input.readUnsignedByte() / 255.0;
						b = input.readUnsignedByte() / 255.0;
						r2 = input.readUnsignedByte() / 255.0;
						g2 = input.readUnsignedByte() / 255.0;
						b2 = input.readUnsignedByte() / 255.0;

						frame = 0;
						bezier = 0;
						while (true) {
							rgb2Timeline.setFrame(frame, time, r, g, b, r2, g2, b2);
							if (frame == frameLast)
								break;

							time2 = input.readFloat();
							nr = input.readUnsignedByte() / 255.0;
							ng = input.readUnsignedByte() / 255.0;
							nb = input.readUnsignedByte() / 255.0;
							nr2 = input.readUnsignedByte() / 255.0;
							ng2 = input.readUnsignedByte() / 255.0;
							nb2 = input.readUnsignedByte() / 255.0;

							switch (input.readByte()) {
								case CURVE_STEPPED:
									rgb2Timeline.setStepped(frame);
								case CURVE_BEZIER:
									setBezier(input, rgb2Timeline, bezier++, frame, 0, time, time2, r, nr, 1);
									setBezier(input, rgb2Timeline, bezier++, frame, 1, time, time2, g, ng, 1);
									setBezier(input, rgb2Timeline, bezier++, frame, 2, time, time2, b, nb, 1);
									setBezier(input, rgb2Timeline, bezier++, frame, 3, time, time2, r2, nr2, 1);
									setBezier(input, rgb2Timeline, bezier++, frame, 4, time, time2, g2, ng2, 1);
									setBezier(input, rgb2Timeline, bezier++, frame, 5, time, time2, b2, nb2, 1);
							}
							time = time2;
							r = nr;
							g = ng;
							b = nb;
							r2 = nr2;
							g2 = ng2;
							b2 = nb2;

							frame++;
						}
						timelines.push(rgb2Timeline);
					case SLOT_ALPHA:
						var alphaTimeline:AlphaTimeline = new AlphaTimeline(frameCount, input.readInt(true), slotIndex);
						time = input.readFloat();
						a = input.readUnsignedByte() / 255;

						frame = 0;
						bezier = 0;
						while (true) {
							alphaTimeline.setFrame(frame, time, a);
							if (frame == frameLast)
								break;

							time2 = input.readFloat();
							a2 = input.readUnsignedByte() / 255;
							switch (input.readByte()) {
								case CURVE_STEPPED:
									alphaTimeline.setStepped(frame);
								case CURVE_BEZIER:
									setBezier(input, alphaTimeline, bezier++, frame, 0, time, time2, a, a2, 1);
							}
							time = time2;
							a = a2;

							frame++;
						}
						timelines.push(alphaTimeline);
				}
			}
		}

		// Bone timelines.
		for (i in 0...input.readInt(true)) {
			var boneIndex:Int = input.readInt(true);
			for (ii in 0...input.readInt(true)) {
				timelineType = input.readByte();
				frameCount = input.readInt(true);
				if (timelineType == BONE_INHERIT) {
					var timeline = new InheritTimeline(frameCount, boneIndex);
					for (frame in 0...frameCount) {
						timeline.setFrame(frame, input.readFloat(), Inherit.values[input.readByte()]);
					}
					timelines.push(timeline);
					continue;
				}
				bezierCount = input.readInt(true);
				switch (timelineType) {
					case BONE_ROTATE:
						readTimeline(input, timelines, new RotateTimeline(frameCount, bezierCount, boneIndex), 1);
					case BONE_TRANSLATE: //
						readTimeline2(input, timelines, new TranslateTimeline(frameCount, bezierCount, boneIndex), scale);
					case BONE_TRANSLATEX: //
						readTimeline(input, timelines, new TranslateXTimeline(frameCount, bezierCount, boneIndex), scale);
					case BONE_TRANSLATEY: //
						readTimeline(input, timelines, new TranslateYTimeline(frameCount, bezierCount, boneIndex), scale);
					case BONE_SCALE:
						readTimeline2(input, timelines, new ScaleTimeline(frameCount, bezierCount, boneIndex), 1);
					case BONE_SCALEX:
						readTimeline(input, timelines, new ScaleXTimeline(frameCount, bezierCount, boneIndex), 1);
					case BONE_SCALEY:
						readTimeline(input, timelines, new ScaleYTimeline(frameCount, bezierCount, boneIndex), 1);
					case BONE_SHEAR:
						readTimeline2(input, timelines, new ShearTimeline(frameCount, bezierCount, boneIndex), 1);
					case BONE_SHEARX:
						readTimeline(input, timelines, new ShearXTimeline(frameCount, bezierCount, boneIndex), 1);
					case BONE_SHEARY:
						readTimeline(input, timelines, new ShearYTimeline(frameCount, bezierCount, boneIndex), 1);
				}
			}
		}

		// IK constraint timelines.
		for (i in 0...input.readInt(true)) {
			index = input.readInt(true);
			frameCount = input.readInt(true);
			frameLast = frameCount - 1;
			var ikTimeline:IkConstraintTimeline = new IkConstraintTimeline(frameCount, input.readInt(true), index);
			var flags = input.readByte();
			time = input.readFloat();
			var mix:Float = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.readFloat() : 1) : 0;
			var softness:Float = (flags & 4) != 0 ? input.readFloat() * scale : 0;

			frame = 0;
			bezier = 0;
			while (true) {
				ikTimeline.setFrame(frame, time, mix, softness, (flags & 8) != 0 ? 1 : -1, (flags & 16) != 0, (flags & 32) != 0);
				if (frame == frameLast)
					break;
				flags = input.readByte();
				time2 = input.readFloat();
				var mix2:Float = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.readFloat() : 1) : 0;
				var softness2:Float = (flags & 4) != 0 ? input.readFloat() * scale : 0;
				if ((flags & 64) != 0) {
					ikTimeline.setStepped(frame);
				} else if ((flags & 128) != 0) {
					setBezier(input, ikTimeline, bezier++, frame, 0, time, time2, mix, mix2, 1);
					setBezier(input, ikTimeline, bezier++, frame, 1, time, time2, softness, softness2, scale);
				}
				time = time2;
				mix = mix2;
				softness = softness2;

				frame++;
			}
			timelines.push(ikTimeline);
		}

		// Transform constraint timelines.
		var mixRotate:Float, mixRotate2:Float;
		var mixX:Float, mixX2:Float;
		var mixY:Float, mixY2:Float;
		for (i in 0...input.readInt(true)) {
			index = input.readInt(true);
			frameCount = input.readInt(true);

			frameLast = frameCount - 1;
			var transformTimeline:TransformConstraintTimeline = new TransformConstraintTimeline(frameCount, input.readInt(true), index);
			time = input.readFloat();
			mixRotate = input.readFloat();
			mixX = input.readFloat();
			mixY = input.readFloat();
			var mixScaleX:Float = input.readFloat(),
				mixScaleY:Float = input.readFloat(),
				mixShearY:Float = input.readFloat();
			frame = 0;
			bezier = 0;
			while (true) {
				transformTimeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
				if (frame == frameLast)
					break;

				time2 = input.readFloat();
				mixRotate2 = input.readFloat();
				mixX2 = input.readFloat();
				mixY2 = input.readFloat();
				var mixScaleX2:Float = input.readFloat(),
					mixScaleY2:Float = input.readFloat(),
					mixShearY2:Float = input.readFloat();
				switch (input.readByte()) {
					case CURVE_STEPPED:
						transformTimeline.setStepped(frame);
					case CURVE_BEZIER:
						setBezier(input, transformTimeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
						setBezier(input, transformTimeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
						setBezier(input, transformTimeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
						setBezier(input, transformTimeline, bezier++, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
						setBezier(input, transformTimeline, bezier++, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
						setBezier(input, transformTimeline, bezier++, frame, 5, time, time2, mixShearY, mixShearY2, 1);
				}
				time = time2;
				mixRotate = mixRotate2;
				mixX = mixX2;
				mixY = mixY2;
				mixScaleX = mixScaleX2;
				mixScaleY = mixScaleY2;
				mixShearY = mixShearY2;

				frame++;
			}

			timelines.push(transformTimeline);
		}

		// Path constraint timelines.
		for (i in 0...input.readInt(true)) {
			index = input.readInt(true);
			var data = cast(skeletonData.constraints[index], PathConstraintData);
			for (ii in 0...input.readInt(true)) {
				var type:Int = input.readByte(),
					frameCount:Int = input.readInt(true),
					bezierCount:Int = input.readInt(true);
				switch (type) {
					case PATH_POSITION:
						readTimeline(input, timelines, new PathConstraintPositionTimeline(frameCount, bezierCount, index),
							data.positionMode == PositionMode.fixed ? scale : 1);
					case PATH_SPACING:
						readTimeline(input, timelines,
							new PathConstraintSpacingTimeline(frameCount, bezierCount, index), data.spacingMode == SpacingMode.length || data.spacingMode == SpacingMode.fixed ? scale : 1);
					case PATH_MIX:
						var mixTimeline:PathConstraintMixTimeline = new PathConstraintMixTimeline(frameCount, bezierCount, index);
						time = input.readFloat();
						mixRotate = input.readFloat();
						mixX = input.readFloat();
						mixY = input.readFloat();

						frame = 0;
						bezier = 0;
						frameLast = mixTimeline.getFrameCount() - 1;
						while (true) {
							mixTimeline.setFrame(frame, time, mixRotate, mixX, mixY);
							if (frame == frameLast)
								break;
							time2 = input.readFloat();
							mixRotate2 = input.readFloat();
							mixX2 = input.readFloat();
							mixY2 = input.readFloat();
							switch (input.readByte()) {
								case CURVE_STEPPED: mixTimeline.setStepped(frame);
								case CURVE_BEZIER:
									setBezier(input, mixTimeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
									setBezier(input, mixTimeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
									setBezier(input, mixTimeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
							}
							time = time2;
							mixRotate = mixRotate2;
							mixX = mixX2;
							mixY = mixY2;

							frame++;
						}
						timelines.push(mixTimeline);
				}
			}
		}

		// Physics timelines.
		for (i in 0...input.readInt(true)) {
			var constraintIndex:Int = input.readInt(true) - 1;
			for (ii in 0...input.readInt(true)) {
				var type:Int = input.readByte(),
					frameCount:Int = input.readInt(true);
				if (type == PHYSICS_RESET) {
					var timeline:PhysicsConstraintResetTimeline = new PhysicsConstraintResetTimeline(frameCount, constraintIndex);
					for (frame in 0...frameCount)
						timeline.setFrame(frame, input.readFloat());
					timelines.push(timeline);
					continue;
				}
				var bezierCount = input.readInt(true);
				var timeline:CurveTimeline1;
				switch (type) {
					case PHYSICS_INERTIA:
						timeline = new PhysicsConstraintInertiaTimeline(frameCount, bezierCount, constraintIndex);
					case PHYSICS_STRENGTH:
						timeline = new PhysicsConstraintStrengthTimeline(frameCount, bezierCount, constraintIndex);
					case PHYSICS_DAMPING:
						timeline = new PhysicsConstraintDampingTimeline(frameCount, bezierCount, constraintIndex);
					case PHYSICS_MASS:
						timeline = new PhysicsConstraintMassTimeline(frameCount, bezierCount, constraintIndex);
					case PHYSICS_WIND:
						timeline = new PhysicsConstraintWindTimeline(frameCount, bezierCount, constraintIndex);
					case PHYSICS_GRAVITY:
						timeline = new PhysicsConstraintGravityTimeline(frameCount, bezierCount, constraintIndex);
					case PHYSICS_MIX:
						timeline = new PhysicsConstraintMixTimeline(frameCount, bezierCount, constraintIndex);
					default:
						throw new SpineException("Unknown physics timeline type: " + type);
				}
				readTimeline(input, timelines, timeline, 1);
			}
		}

		// Slider timelines.
		for (i in 0...input.readInt(true)) {
			var index = input.readInt(true);
			for (ii in 0...input.readInt(true)) {
				var type = input.readByte(),
					frameCount = input.readInt(true),
					bezierCount = input.readInt(true);
				var timeline:CurveTimeline1;
				switch (type) {
					case SLIDER_TIME:
						timeline = new SliderTimeline(frameCount, bezierCount, index);
					case SLIDER_MIX:
						timeline = new SliderMixTimeline(frameCount, bezierCount, index);
					default:
						throw new SpineException("Unknown slider timeline type: " + type);
				}
				readTimeline(input, timelines, timeline, 1);
			}
		}

		// Deform timelines.
		for (i in 0...input.readInt(true)) {
			var skin:Skin = skeletonData.skins[input.readInt(true)];
			for (ii in 0...input.readInt(true)) {
				slotIndex = input.readInt(true);
				for (iii in 0...input.readInt(true)) {
					var attachmentName:String = input.readStringRef();
					var attachment = skin.getAttachment(slotIndex, attachmentName);
					if (attachment == null)
						throw new SpineException("Vertex attachment not found: " + attachmentName);
					var timelineType = input.readByte();
					frameCount = input.readInt(true);
					frameLast = frameCount - 1;

					switch (timelineType) {
						case ATTACHMENT_DEFORM:
							var vertexAttachment = cast(attachment, VertexAttachment);
							var weighted:Bool = vertexAttachment.bones != null;
							var vertices:Array<Float> = vertexAttachment.vertices;
							var deformLength:Int = weighted ? Std.int(vertices.length / 3 * 2) : vertices.length;

							bezierCount = input.readInt(true);
							var deformTimeline:DeformTimeline = new DeformTimeline(frameCount, bezierCount, slotIndex, vertexAttachment);

							time = input.readFloat();
							frame = 0;
							bezier = 0;
							while (true) {
								var deform:Array<Float>;
								var end:Int = input.readInt(true);
								if (end == 0) {
									if (weighted) {
										deform = new Array<Float>();
										ArrayUtils.resize(deform, deformLength, 0);
									} else {
										deform = vertices;
									}
								} else {
									var v:Int, vn:Int;
									deform = new Array<Float>();
									ArrayUtils.resize(deform, deformLength, 0);
									var start:Int = input.readInt(true);
									end += start;
									if (scale == 1) {
										for (v in start...end) {
											deform[v] = input.readFloat();
										}
									} else {
										for (v in start...end) {
											deform[v] = input.readFloat() * scale;
										}
									}
									if (!weighted) {
										for (v in 0...deform.length) {
											deform[v] += vertices[v];
										}
									}
								}

								deformTimeline.setFrame(frame, time, deform);
								if (frame == frameLast)
									break;
								time2 = input.readFloat();
								switch (input.readByte()) {
									case CURVE_STEPPED: deformTimeline.setStepped(frame);
									case CURVE_BEZIER: SkeletonBinary.setBezier(input, deformTimeline, bezier++, frame, 0, time, time2, 0, 1, 1);
								}
								time = time2;

								frame++;
							}
							timelines.push(deformTimeline);
						case ATTACHMENT_SEQUENCE:
							var timeline = new SequenceTimeline(frameCount, slotIndex, attachment);
							for (frame in 0...frameCount) {
								var time = input.readFloat();
								var modeAndIndex = input.readInt32();
								timeline.setFrame(frame, time, SequenceMode.values[modeAndIndex & 0xf], modeAndIndex >> 4, input.readFloat());
							}
							timelines.push(timeline);
							break;
					}
				}
			}
		}

		// Draw order timeline.
		var slotCount:Int = skeletonData.slots.length;
		var drawOrderCount:Int = input.readInt(true);
		if (drawOrderCount > 0) {
			var drawOrderTimeline:DrawOrderTimeline = new DrawOrderTimeline(drawOrderCount);
			for (i in 0...drawOrderCount)
				drawOrderTimeline.setFrame(i, input.readFloat(), readDrawOrder(input, slotCount));
			timelines.push(drawOrderTimeline);
		}

		// Draw order folder timelines.
		var folderCount:Int = input.readInt(true);
		for (i in 0...folderCount) {
			var folderSlotCount:Int = input.readInt(true);
			var folderSlots:Array<Int> = new Array<Int>();
			folderSlots.resize(folderSlotCount);
			for (ii in 0...folderSlotCount)
				folderSlots[ii] = input.readInt(true);
			var keyCount:Int = input.readInt(true);
			var folderTimeline = new DrawOrderFolderTimeline(keyCount, folderSlots, slotCount);
			for (ii in 0...keyCount)
				folderTimeline.setFrame(ii, input.readFloat(), readDrawOrder(input, folderSlotCount));
			timelines.push(folderTimeline);
		}

		// Event timelines.
		var eventCount:Int = input.readInt(true);
		if (eventCount > 0) {
			var eventTimeline:EventTimeline = new EventTimeline(eventCount);
			for (i in 0...eventCount) {
				time = input.readFloat();
				var eventData:EventData = skeletonData.events[input.readInt(true)];
				var event:Event = new Event(time, eventData);
				event.intValue = input.readInt(false);
				event.floatValue = input.readFloat();
				event.stringValue = input.readString();
				if (event.stringValue == null)
					event.stringValue = eventData.setupPose.stringValue;
				if (event.data.audioPath != null) {
					event.volume = input.readFloat();
					event.balance = input.readFloat();
				}
				eventTimeline.setFrame(i, event);
			}
			timelines.push(eventTimeline);
		}

		var duration:Float = 0;
		for (i in 0...timelines.length)
			duration = Math.max(duration, timelines[i].getDuration());
		var animation = new Animation(name, timelines, duration);
		if (nonessential)
			animation.color.setFromRgba8888(input.readInt32());
		return animation;
	}

	static private function readTimeline(input:BinaryInput, timelines:Array<Timeline>, timeline:CurveTimeline1, scale:Float) {
		var time:Float = input.readFloat(),
			value:Float = input.readFloat() * scale;

		var frame:Int = 0, bezier:Int = 0, frameLast:Int = timeline.getFrameCount() - 1;
		while (true) {
			timeline.setFrame(frame, time, value);
			if (frame == frameLast)
				break;

			var time2:Float = input.readFloat(),
				value2:Float = input.readFloat() * scale;
			switch (input.readByte()) {
				case CURVE_STEPPED:
					timeline.setStepped(frame);
				case CURVE_BEZIER:
					setBezier(input, timeline, bezier++, frame, 0, time, time2, value, value2, scale);
			}
			time = time2;
			value = value2;

			frame++;
		}
		timelines.push(timeline);
	}

	static private function readTimeline2(input:BinaryInput, timelines:Array<Timeline>, timeline:BoneTimeline2, scale:Float) {
		var time:Float = input.readFloat(),
			value1:Float = input.readFloat() * scale,
			value2:Float = input.readFloat() * scale;

		var frame:Int = 0, bezier:Int = 0, frameLast:Int = timeline.getFrameCount() - 1;
		while (true) {
			timeline.setFrame(frame, time, value1, value2);
			if (frame == frameLast)
				break;

			var time2:Float = input.readFloat(),
				nvalue1:Float = input.readFloat() * scale,
				nvalue2:Float = input.readFloat() * scale;
			switch (input.readByte()) {
				case CURVE_STEPPED:
					timeline.setStepped(frame);
				case CURVE_BEZIER:
					setBezier(input, timeline, bezier++, frame, 0, time, time2, value1, nvalue1, scale);
					setBezier(input, timeline, bezier++, frame, 1, time, time2, value2, nvalue2, scale);
			}
			time = time2;
			value1 = nvalue1;
			value2 = nvalue2;

			frame++;
		}
		timelines.push(timeline);
	}

	static private function setBezier(input:BinaryInput, timeline:CurveTimeline, bezier:Int, frame:Int, value:Float, time1:Float, time2:Float, value1:Float,
			value2:Float, scale:Float):Void {
		timeline.setBezier(bezier, frame, value, time1, value1, input.readFloat(), input.readFloat() * scale, input.readFloat(), input.readFloat() * scale,
			time2, value2);
	}
}

class Vertices {
	public var vertices:Array<Float> = new Array<Float>();
	public var bones:Array<Int> = new Array<Int>();
	public var length:Int = 0;

	public function new() {}
}

class LinkedMeshBinary {
	public var source(default, null):String;
	public var skinIndex(default, null):Int;
	public var slotIndex(default, null):Int;
	public var sourceIndex(default, null):Int;
	public var mesh(default, null):MeshAttachment;
	public var inheritTimelines(default, null):Bool;

	public function new(mesh:MeshAttachment, skinIndex:Int, slotIndex:Int, sourceIndex:Int, source:String, inheritTimelines:Bool) {
		this.mesh = mesh;
		this.skinIndex = skinIndex;
		this.slotIndex = slotIndex;
		this.sourceIndex = sourceIndex;
		this.source = source;
		this.inheritTimelines = inheritTimelines;
	}
}
