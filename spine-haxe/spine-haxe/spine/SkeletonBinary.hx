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

package spine;

import haxe.io.Bytes;
import StringTools;
import spine.animation.AlphaTimeline;
import spine.animation.Animation;
import spine.animation.AttachmentTimeline;
import spine.animation.CurveTimeline1;
import spine.animation.CurveTimeline2;
import spine.animation.CurveTimeline;
import spine.animation.DeformTimeline;
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
import spine.animation.PhysicsConstraintTimeline;
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
		n = input.readInt(true);
		for (i in 0...n) {
			var boneName:String = input.readString();
			var boneParent:BoneData = i == 0 ? null : skeletonData.bones[input.readInt(true)];
			var boneData:BoneData = new BoneData(i, boneName, boneParent);
			boneData.rotation = input.readFloat();
			boneData.x = input.readFloat() * scale;
			boneData.y = input.readFloat() * scale;
			boneData.scaleX = input.readFloat();
			boneData.scaleY = input.readFloat();
			boneData.shearX = input.readFloat();
			boneData.shearY = input.readFloat();
			boneData.length = input.readFloat() * scale;
			boneData.inherit = Inherit.values[input.readInt(true)];
			boneData.skinRequired = input.readBoolean();
			if (nonessential) {
				boneData.color.setFromRgba8888(input.readInt32());
				boneData.icon = input.readString();
				boneData.visible = input.readBoolean();
			}
			skeletonData.bones.push(boneData);
		}

		// Slots.
		n = input.readInt(true);
		for (i in 0...n) {
			var slotName:String = input.readString();

			var slotBoneData:BoneData = skeletonData.bones[input.readInt(true)];
			var slotData:SlotData = new SlotData(i, slotName, slotBoneData);
			slotData.color.setFromRgba8888(input.readInt32());

			var darkColor:Int = input.readInt32();
			if (darkColor != -1) {
				slotData.darkColor = new Color(0, 0, 0);
				slotData.darkColor.setFromRgb888(darkColor);
			}

			slotData.attachmentName = input.readStringRef();
			slotData.blendMode = BlendMode.values[input.readInt(true)];
			if (nonessential) slotData.visible = input.readBoolean();
			skeletonData.slots.push(slotData);
		}

		// IK constraints.
		n = input.readInt(true);
		for (i in 0...n) {
			var ikData:IkConstraintData = new IkConstraintData(input.readString());
			ikData.order = input.readInt(true);
			nn = input.readInt(true);
			for (ii in 0...nn) {
				ikData.bones.push(skeletonData.bones[input.readInt(true)]);
			}
			ikData.target = skeletonData.bones[input.readInt(true)];
			var flags = input.readByte();
			ikData.skinRequired = (flags & 1) != 0;
			ikData.bendDirection = (flags & 2) != 0 ? 1 : -1;
			ikData.compress = (flags & 4) != 0;
			ikData.stretch = (flags & 8) != 0;
			ikData.uniform = (flags & 16) != 0;
			if ((flags & 32) != 0) ikData.mix = (flags & 64) != 0 ? input.readFloat() : 1;
			if ((flags & 128) != 0) ikData.softness = input.readFloat() * scale;
			skeletonData.ikConstraints.push(ikData);
		}

		// Transform constraints.
		n = input.readInt(true);
		for (i in 0...n) {
			var transformData:TransformConstraintData = new TransformConstraintData(input.readString());
			transformData.order = input.readInt(true);
			nn = input.readInt(true);
			for (ii in 0...nn) {
				transformData.bones.push(skeletonData.bones[input.readInt(true)]);
			}
			transformData.target = skeletonData.bones[input.readInt(true)];
			var flags = input.readByte();
			transformData.skinRequired = (flags & 1) != 0;
			transformData.local = (flags & 2) != 0;
			transformData.relative = (flags & 4) != 0;
			if ((flags & 8) != 0) transformData.offsetRotation = input.readFloat();
			if ((flags & 16) != 0) transformData.offsetX = input.readFloat() * scale;
			if ((flags & 32) != 0) transformData.offsetY = input.readFloat() * scale;
			if ((flags & 64) != 0) transformData.offsetScaleX = input.readFloat();
			if ((flags & 128) != 0) transformData.offsetScaleY = input.readFloat();
			flags = input.readByte();
			if ((flags & 1) != 0) transformData.offsetShearY = input.readFloat();
			if ((flags & 2) != 0) transformData.mixRotate = input.readFloat();
			if ((flags & 4) != 0) transformData.mixX = input.readFloat();
			if ((flags & 8) != 0) transformData.mixY = input.readFloat();
			if ((flags & 16) != 0) transformData.mixScaleX = input.readFloat();
			if ((flags & 32) != 0) transformData.mixScaleY = input.readFloat();
			if ((flags & 64) != 0) transformData.mixShearY = input.readFloat();
			skeletonData.transformConstraints.push(transformData);
		}

		// Path constraints.
		n = input.readInt(true);
		for (i in 0...n) {
			var pathData:PathConstraintData = new PathConstraintData(input.readString());
			pathData.order = input.readInt(true);
			pathData.skinRequired = input.readBoolean();
			nn = input.readInt(true);
			for (ii in 0...nn) {
				pathData.bones.push(skeletonData.bones[input.readInt(true)]);
			}
			pathData.target = skeletonData.slots[input.readInt(true)];
			var flags = input.readByte();
			pathData.positionMode = PositionMode.values[flags & 1];
			pathData.spacingMode = SpacingMode.values[(flags >> 1) & 3];
			pathData.rotateMode = RotateMode.values[(flags >> 3) & 3];
			if ((flags & 128) != 0) pathData.offsetRotation = input.readFloat();
			pathData.position = input.readFloat();
			if (pathData.positionMode == PositionMode.fixed)
				pathData.position *= scale;
			pathData.spacing = input.readFloat();
			if (pathData.spacingMode == SpacingMode.length || pathData.spacingMode == SpacingMode.fixed)
				pathData.spacing *= scale;
			pathData.mixRotate = input.readFloat();
			pathData.mixX = input.readFloat();
			pathData.mixY = input.readFloat();
			skeletonData.pathConstraints.push(pathData);
		}

		// Physics constraints.
		n = input.readInt(true);
		for (i in 0...n) {
			var physicsData:PhysicsConstraintData = new PhysicsConstraintData(input.readString());
			physicsData.order = input.readInt(true);
			physicsData.bone = skeletonData.bones[input.readInt(true)];
			var flags = input.readByte();
			physicsData.skinRequired = (flags & 1) != 0;
			if ((flags & 2) != 0) physicsData.x = input.readFloat();
			if ((flags & 4) != 0) physicsData.y = input.readFloat();
			if ((flags & 8) != 0) physicsData.rotate = input.readFloat();
			if ((flags & 16) != 0) physicsData.scaleX = input.readFloat();
			if ((flags & 32) != 0) physicsData.shearX = input.readFloat();
			physicsData.limit = ((flags & 64) != 0 ? input.readFloat() : 5000) * scale;
			physicsData.step = 1 / input.readUnsignedByte();
			physicsData.inertia = input.readFloat();
			physicsData.strength = input.readFloat();
			physicsData.damping = input.readFloat();
			physicsData.massInverse = (flags & 128) != 0 ? input.readFloat() : 1;
			physicsData.wind = input.readFloat();
			physicsData.gravity = input.readFloat();
			flags = input.readByte();
			if ((flags & 1) != 0) physicsData.inertiaGlobal = true;
			if ((flags & 2) != 0) physicsData.strengthGlobal = true;
			if ((flags & 4) != 0) physicsData.dampingGlobal = true;
			if ((flags & 8) != 0) physicsData.massGlobal = true;
			if ((flags & 16) != 0) physicsData.windGlobal = true;
			if ((flags & 32) != 0) physicsData.gravityGlobal = true;
			if ((flags & 64) != 0) physicsData.mixGlobal = true;
			physicsData.mix = (flags & 128) != 0 ? input.readFloat() : 1;
			skeletonData.physicsConstraints.push(physicsData);
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
			var parent:Attachment = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
			if (parent == null)
				throw new SpineException("Parent mesh not found: " + linkedMesh.parent);
			linkedMesh.mesh.timelineAttachment = linkedMesh.inheritTimeline ? cast(parent, VertexAttachment) : linkedMesh.mesh;
			linkedMesh.mesh.parentMesh = cast(parent, MeshAttachment);
			if (linkedMesh.mesh.region != null)
				linkedMesh.mesh.updateRegion();
		}
		linkedMeshes.resize(0);

		// Events.
		n = input.readInt(true);
		for (i in 0...n) {
			var data:EventData = new EventData(input.readString());
			data.intValue = input.readInt(false);
			data.floatValue = input.readFloat();
			data.stringValue = input.readString();
			data.audioPath = input.readString();
			if (data.audioPath != null) {
				data.volume = input.readFloat();
				data.balance = input.readFloat();
			}
			skeletonData.events.push(data);
		}

		// Animations.
		n = input.readInt(true);
		for (i in 0...n) {
			skeletonData.animations.push(readAnimation(input, input.readString(), skeletonData));
		}
		return skeletonData;
	}

	private function readSkin(input:BinaryInput, skeletonData:SkeletonData, defaultSkin:Bool, nonessential:Bool):Skin {
		var skin:Skin = null;
		var slotCount:Int = 0;

		if (defaultSkin) {
			slotCount = input.readInt(true);
			if (slotCount == 0)
				return null;
			skin = new Skin("default");
		} else {
			skin = new Skin(input.readString());
			if (nonessential) skin.color.setFromRgba8888(input.readInt32());
			skin.bones.resize(input.readInt(true));
			for (i in 0...skin.bones.length) {
				skin.bones[i] = skeletonData.bones[input.readInt(true)];
			}

			for (i in 0...input.readInt(true)) {
				skin.constraints.push(skeletonData.ikConstraints[input.readInt(true)]);
			}
			for (i in 0...input.readInt(true)) {
				skin.constraints.push(skeletonData.transformConstraints[input.readInt(true)]);
			}
			for (i in 0...input.readInt(true)) {
				skin.constraints.push(skeletonData.pathConstraints[input.readInt(true)]);
			}
			for (i in 0...input.readInt(true)) {
				skin.constraints.push(skeletonData.physicsConstraints[input.readInt(true)]);
			}

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

	private function readSequence(input:BinaryInput):Sequence {
		var sequence = new Sequence(input.readInt(true));
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
		var name:String = (flags & 8) != 0 ? input.readStringRef() : attachmentName;
		if (name == null)
			throw new SpineException("Attachment name must not be null");
		switch (AttachmentType.values[flags & 7]) {
			case AttachmentType.region:
				path = (flags & 16) != 0 ? input.readStringRef() : null;
				color = (flags & 32) != 0 ? input.readInt32() : 0xffffffff;
				var sequence = (flags & 64) != 0 ? readSequence(input) : null;
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
				region.sequence = sequence;
				if (sequence == null)
					region.updateRegion();
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
				var sequence = (flags & 64) != 0 ? readSequence(input) : null;
				var hullLength = input.readInt(true);
				vertices = readVertices(input, (flags & 128) != 0);
				var uvs:Array<Float> = readFloatArray(input, vertices.length, 1);
				var triangles:Array<Int> = readShortArray(input, (vertices.length - hullLength - 2) * 3);
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
				if (vertices.bones.length > 0)
					mesh.bones = vertices.bones;
				mesh.vertices = vertices.vertices;
				mesh.worldVerticesLength = vertices.length;
				mesh.triangles = triangles;
				mesh.regionUVs = uvs;
				if (sequence == null)
					mesh.updateRegion();
				mesh.hullLength = hullLength << 1;
				mesh.sequence = sequence;
				if (nonessential) {
					mesh.edges = edges;
					mesh.width = width * scale;
					mesh.height = height * scale;
				}
				return mesh;
			case AttachmentType.linkedmesh:
				path = (flags & 16) != 0 ? input.readStringRef() : name;
				if (path == null) throw new SpineException("Path of linked mesh must not be null");
				color = (flags & 32) != 0 ? input.readInt32() : 0xffffffff;
				var sequence = (flags & 64) != 0 ? this.readSequence(input) : null;
				var inheritTimelines:Bool = (flags & 128) != 0;
				var skinIndex = input.readInt(true);
				var parent:String = input.readStringRef();
				if (nonessential) {
					width = input.readFloat();
					height = input.readFloat();
				}

				mesh = attachmentLoader.newMeshAttachment(skin, name, path, sequence);
				if (mesh == null)
					return null;
				mesh.path = path;
				mesh.color.setFromRgba8888(color);
				mesh.sequence = sequence;
				if (nonessential) {
					mesh.width = width * scale;
					mesh.height = height * scale;
				}
				this.linkedMeshes.push(new LinkedMeshBinary(mesh, skinIndex, slotIndex, parent, inheritTimelines));
				return mesh;
			case AttachmentType.path:
				var closed:Bool = (flags & 16) != 0;
				var constantSpeed:Bool = (flags & 32) != 0;
				vertices = readVertices(input, (flags & 64) != 0);
				var lengths:Array<Float> = new Array<Float>();
				lengths.resize(Std.int(vertices.length / 6));
				for (i in 0...lengths.length) {
					lengths[i] = input.readFloat() * scale;
				}
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
		var weights:Array<Float> = new Array<Float>();
		var bonesArray:Array<Int> = new Array<Int>();
		for (i in 0...vertexCount) {
			var boneCount:Int = input.readInt(true);
			bonesArray.push(boneCount);
			for (ii in 0...boneCount) {
				bonesArray.push(input.readInt(true));
				weights.push(input.readFloat() * scale);
				weights.push(input.readFloat() * scale);
				weights.push(input.readFloat());
			}
		}
		vertices.vertices = weights;
		vertices.bones = bonesArray;
		return vertices;
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

	private function readAnimation(input:BinaryInput, name:String, skeletonData:SkeletonData):Animation {
		input.readInt(true); // Count of timelines.
		var timelines:Array<Timeline> = new Array<Timeline>();
		var i:Int = 0, n:Int = 0, ii:Int = 0, nn:Int = 0;

		var index:Int, slotIndex:Int, timelineType:Int, timelineScale:Float;
		var frameCount:Int,
			frameLast:Int,
			frame:Int,
			bezierCount:Int,
			bezier:Int;
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
						timelines.push(readTimeline(input, new RotateTimeline(frameCount, bezierCount, boneIndex), 1));
					case BONE_TRANSLATE:
						timelines.push(readTimeline2(input, new TranslateTimeline(frameCount, bezierCount, boneIndex), scale));
					case BONE_TRANSLATEX:
						timelines.push(readTimeline(input, new TranslateXTimeline(frameCount, bezierCount, boneIndex), scale));
					case BONE_TRANSLATEY:
						timelines.push(readTimeline(input, new TranslateYTimeline(frameCount, bezierCount, boneIndex), scale));
					case BONE_SCALE:
						timelines.push(readTimeline2(input, new ScaleTimeline(frameCount, bezierCount, boneIndex), 1));
					case BONE_SCALEX:
						timelines.push(readTimeline(input, new ScaleXTimeline(frameCount, bezierCount, boneIndex), 1));
					case BONE_SCALEY:
						timelines.push(readTimeline(input, new ScaleYTimeline(frameCount, bezierCount, boneIndex), 1));
					case BONE_SHEAR:
						timelines.push(readTimeline2(input, new ShearTimeline(frameCount, bezierCount, boneIndex), 1));
					case BONE_SHEARX:
						timelines.push(readTimeline(input, new ShearXTimeline(frameCount, bezierCount, boneIndex), 1));
					case BONE_SHEARY:
						timelines.push(readTimeline(input, new ShearYTimeline(frameCount, bezierCount, boneIndex), 1));
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
			var	softness:Float = (flags & 4) != 0 ? input.readFloat() * scale : 0;

			frame = 0;
			bezier = 0;
			while (true) {
				ikTimeline.setFrame(frame, time, mix, softness, (flags & 8) != 0 ? 1 : -1, (flags & 16) != 0, (flags & 32) != 0);
				if (frame == frameLast)
					break;
				flags = input.readByte();
				time2 = input.readFloat();
				var mix2:Float = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.readFloat() : 1) : 0;
				var	softness2:Float = (flags & 4) != 0 ? input.readFloat() * scale : 0;
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
			var data:PathConstraintData = skeletonData.pathConstraints[index];
			for (ii in 0...input.readInt(true)) {
				var type:Int = input.readByte(), frameCount:Int = input.readInt(true), bezierCount:Int = input.readInt(true);
				switch (type) {
					case PATH_POSITION:
						timelines.push(readTimeline(input, new PathConstraintPositionTimeline(frameCount, bezierCount, index),
							data.positionMode == PositionMode.fixed ? scale : 1));
					case PATH_SPACING:
						timelines.push(readTimeline(input, new PathConstraintSpacingTimeline(frameCount, bezierCount, index),
							data.spacingMode == SpacingMode.length
							|| data.spacingMode == SpacingMode.fixed ? scale : 1));
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
								case CURVE_STEPPED:
									mixTimeline.setStepped(frame);
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
				var type:Int = input.readByte(), frameCount:Int = input.readInt(true);
				if (type == PHYSICS_RESET) {
					var timeline:PhysicsConstraintResetTimeline = new PhysicsConstraintResetTimeline(frameCount, constraintIndex);
					for (frame in 0...frameCount)
						timeline.setFrame(frame, input.readFloat());
					timelines.push(timeline);
					continue;
				}
				var bezierCount = input.readInt(true);
				switch (type) {
					case PHYSICS_INERTIA:
						timelines.push(readTimeline(input, new PhysicsConstraintInertiaTimeline(frameCount, bezierCount, constraintIndex), 1));
						break;
					case PHYSICS_STRENGTH:
						timelines.push(readTimeline(input, new PhysicsConstraintStrengthTimeline(frameCount, bezierCount, constraintIndex), 1));
						break;
					case PHYSICS_DAMPING:
						timelines.push(readTimeline(input, new PhysicsConstraintDampingTimeline(frameCount, bezierCount, constraintIndex), 1));
						break;
					case PHYSICS_MASS:
						timelines.push(readTimeline(input, new PhysicsConstraintMassTimeline(frameCount, bezierCount, constraintIndex), 1));
						break;
					case PHYSICS_WIND:
						timelines.push(readTimeline(input, new PhysicsConstraintWindTimeline(frameCount, bezierCount, constraintIndex), 1));
						break;
					case PHYSICS_GRAVITY:
						timelines.push(readTimeline(input, new PhysicsConstraintGravityTimeline(frameCount, bezierCount, constraintIndex), 1));
						break;
					case PHYSICS_MIX:
						timelines.push(readTimeline(input, new PhysicsConstraintMixTimeline(frameCount, bezierCount, constraintIndex), 1));
				}
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
									case CURVE_STEPPED:
										deformTimeline.setStepped(frame);
									case CURVE_BEZIER:
										SkeletonBinary.setBezier(input, deformTimeline, bezier++, frame, 0, time, time2, 0, 1, 1);
								}
								time = time2;

								frame++;
							}
							timelines.push(deformTimeline);
						case ATTACHMENT_SEQUENCE:
							var timeline = new SequenceTimeline(frameCount, slotIndex, cast(attachment, HasTextureRegion));
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

		// Draw order timelines.
		var drawOrderCount:Int = input.readInt(true);
		if (drawOrderCount > 0) {
			var drawOrderTimeline:DrawOrderTimeline = new DrawOrderTimeline(drawOrderCount);
			var slotCount:Int = skeletonData.slots.length;
			for (i in 0...drawOrderCount) {
				time = input.readFloat();
				var offsetCount:Int = input.readInt(true);
				var drawOrder:Array<Int> = new Array<Int>();
				drawOrder.resize(slotCount);
				var ii:Int = slotCount - 1;
				while (ii >= 0) {
					drawOrder[ii--] = -1;
				}
				var unchanged:Array<Int> = new Array<Int>();
				unchanged.resize(slotCount - offsetCount);
				var originalIndex:Int = 0, unchangedIndex:Int = 0;
				for (ii in 0...offsetCount) {
					slotIndex = input.readInt(true);
					// Collect unchanged items.
					while (originalIndex != slotIndex) {
						unchanged[unchangedIndex++] = originalIndex++;
					}
					// Set changed items.
					drawOrder[originalIndex + input.readInt(true)] = originalIndex++;
				}
				// Collect remaining unchanged items.
				while (originalIndex < slotCount) {
					unchanged[unchangedIndex++] = originalIndex++;
				}
				// Fill in unchanged items.
				ii = slotCount - 1;
				while (ii >= 0) {
					if (drawOrder[ii] == -1)
						drawOrder[ii] = unchanged[--unchangedIndex];
					ii--;
				}
				drawOrderTimeline.setFrame(i, time, drawOrder);
			}
			timelines.push(drawOrderTimeline);
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
				if (event.stringValue == null) event.stringValue = eventData.stringValue;
				if (event.data.audioPath != null) {
					event.volume = input.readFloat();
					event.balance = input.readFloat();
				}
				eventTimeline.setFrame(i, event);
			}
			timelines.push(eventTimeline);
		}

		var duration:Float = 0;
		for (i in 0...timelines.length) {
			duration = Math.max(duration, timelines[i].getDuration());
		}
		return new Animation(name, timelines, duration);
	}

	static private function readTimeline(input:BinaryInput, timeline:CurveTimeline1, scale:Float):CurveTimeline1 {
		var time:Float = input.readFloat(),
			value:Float = input.readFloat() * scale;

		var frame:Int = 0,
			bezier:Int = 0,
			frameLast:Int = timeline.getFrameCount() - 1;
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
		return timeline;
	}

	static private function readTimeline2(input:BinaryInput, timeline:CurveTimeline2, scale:Float):CurveTimeline2 {
		var time:Float = input.readFloat(),
			value1:Float = input.readFloat() * scale,
			value2:Float = input.readFloat() * scale;

		var frame:Int = 0,
			bezier:Int = 0,
			frameLast:Int = timeline.getFrameCount() - 1;
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
		return timeline;
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
	public var parent(default, null):String;
	public var skinIndex(default, null):Int;
	public var slotIndex(default, null):Int;
	public var mesh(default, null):MeshAttachment;
	public var inheritTimeline(default, null):Bool;

	public function new(mesh:MeshAttachment, skinIndex:Int, slotIndex:Int, parent:String, inheritTimeline:Bool) {
		this.mesh = mesh;
		this.skinIndex = skinIndex;
		this.slotIndex = slotIndex;
		this.parent = parent;
		this.inheritTimeline = inheritTimeline;
	}
}
