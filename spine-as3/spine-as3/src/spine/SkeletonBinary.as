/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

package spine {
	import spine.attachments.ClippingAttachment;
	import spine.animation.TwoColorTimeline;
	import spine.attachments.PointAttachment;
	import spine.animation.PathConstraintMixTimeline;
	import spine.animation.PathConstraintSpacingTimeline;
	import spine.animation.PathConstraintPositionTimeline;
	import spine.animation.TransformConstraintTimeline;
	import spine.animation.ShearTimeline;
	import spine.attachments.PathAttachment;
	import spine.attachments.VertexAttachment;

	import flash.utils.ByteArray;

	import spine.animation.Animation;
	import spine.animation.AttachmentTimeline;
	import spine.animation.ColorTimeline;
	import spine.animation.CurveTimeline;
	import spine.animation.DrawOrderTimeline;
	import spine.animation.EventTimeline;
	import spine.animation.DeformTimeline;
	import spine.animation.IkConstraintTimeline;
	import spine.animation.RotateTimeline;
	import spine.animation.ScaleTimeline;
	import spine.animation.Timeline;
	import spine.animation.TranslateTimeline;
	import spine.attachments.Attachment;
	import spine.attachments.AttachmentLoader;
	import spine.attachments.AttachmentType;
	import spine.attachments.BoundingBoxAttachment;
	import spine.attachments.MeshAttachment;
	import spine.attachments.RegionAttachment;

	public class SkeletonBinary {
		public var attachmentLoader : AttachmentLoader;
		public var scale : Number = 1;
		private var linkedMeshes : Vector.<LinkedMesh> = new Vector.<LinkedMesh>();

		private static const BONE_ROTATE : int = 0;
		private static const BONE_TRANSLATE : int = 1;
		private static const BONE_SCALE : int = 2;
		private static const BONE_SHEAR : int = 3;

		private static const SLOT_ATTACHMENT : int = 0;
		private static const SLOT_COLOR : int = 1;
		private static const SLOT_TWO_COLOR : int = 2;

		private static const PATH_POSITION : int = 0;
		private static const PATH_SPACING : int = 1;
		private static const PATH_MIX : int = 2;

		private static const CURVE_LINEAR : int = 0;
		private static const CURVE_STEPPED : int = 1;
		private static const CURVE_BEZIER : int = 2;

		public function SkeletonBinary(attachmentLoader : AttachmentLoader = null) {
			this.attachmentLoader = attachmentLoader;
		}

		/** @param object A String or ByteArray. */
		public function readSkeletonData(object : *) : SkeletonData {
			if (object == null) throw new ArgumentError("object cannot be null.");
			if (!(object is ByteArray)) throw new ArgumentError("Object must be a ByteArray");

			var scale : Number = this.scale;

			var skeletonData : SkeletonData = new SkeletonData();
			skeletonData.name = ""; // BOZO

			var input : BinaryInput = new BinaryInput(object);

			skeletonData.hash = input.readString();
			skeletonData.version = input.readString();
			if ("3.8.75" == skeletonData.version)
					throw new Error("Unsupported skeleton data, please export with a newer version of Spine.");
			skeletonData.x = input.readFloat();
			skeletonData.y = input.readFloat();
			skeletonData.width = input.readFloat();
			skeletonData.height = input.readFloat();

			var nonessential : Boolean = input.readBoolean();
			if (nonessential) {
				skeletonData.fps = input.readFloat();

				skeletonData.imagesPath = input.readString();
				skeletonData.audioPath = input.readString();
			}

			var n : int = 0;
			var i : int = 0;
			// Strings.
			n = input.readInt(true);
			for (i = 0; i < n; i++)
				input.strings.push(input.readString());

			// Bones.
			n = input.readInt(true);
			for (i = 0; i < n; i++) {
				var boneName : String = input.readString();
				var boneParent : BoneData = i == 0 ? null : skeletonData.bones[input.readInt(true)];
				var boneData : BoneData = new BoneData(i, boneName, boneParent);
				boneData.rotation = input.readFloat();
				boneData.x = input.readFloat() * scale;
				boneData.y = input.readFloat() * scale;
				boneData.scaleX = input.readFloat();
				boneData.scaleY = input.readFloat();
				boneData.shearX = input.readFloat();
				boneData.shearY = input.readFloat();
				boneData.length = input.readFloat() * scale;
				boneData.transformMode = TransformMode.values[input.readInt(true)];
				boneData.skinRequired = input.readBoolean();
				if (nonessential) boneData.color.setFromRgba8888(input.readInt32());
				skeletonData.bones.push(boneData);
			}

			// Slots.
			n = input.readInt(true);
			for (i = 0; i < n; i++) {
				var slotName : String = input.readString();
				var slotBoneData : BoneData = skeletonData.bones[input.readInt(true)];
				var slotData : SlotData = new SlotData(i, slotName, slotBoneData);
				slotData.color.setFromRgba8888(input.readInt32());

				var darkColor : int = input.readInt32();
				if (darkColor != -1) slotData.darkColor.setFromRgb888(darkColor);

				slotData.attachmentName = input.readStringRef();
				slotData.blendMode = BlendMode.values[input.readInt(true)];
				skeletonData.slots.push(slotData);
			}

			// IK constraints.
			n = input.readInt(true);
			var nn : int = 0;
			var ii : int = 0;
			for (i = 0; i < n; i++) {
				var ikData : IkConstraintData = new IkConstraintData(input.readString());
				ikData.order = input.readInt(true);
				ikData.skinRequired = input.readBoolean();
				nn = input.readInt(true);
				for (ii = 0; ii < nn; ii++)
					ikData.bones.push(skeletonData.bones[input.readInt(true)]);
				ikData.target = skeletonData.bones[input.readInt(true)];
				ikData.mix = input.readFloat();
				ikData.softness = input.readFloat() * scale;
				ikData.bendDirection = input.readByte();
				ikData.compress = input.readBoolean();
				ikData.stretch = input.readBoolean();
				ikData.uniform = input.readBoolean();
				skeletonData.ikConstraints.push(ikData);
			}

			// Transform constraints.
			n = input.readInt(true);
			for (i = 0, nn; i < n; i++) {
				var transData : TransformConstraintData = new TransformConstraintData(input.readString());
				transData.order = input.readInt(true);
				transData.skinRequired = input.readBoolean();
				nn = input.readInt(true);
				for (ii = 0; ii < nn; ii++)
					transData.bones.push(skeletonData.bones[input.readInt(true)]);
				transData.target = skeletonData.bones[input.readInt(true)];
				transData.local = input.readBoolean();
				transData.relative = input.readBoolean();
				transData.offsetRotation = input.readFloat();
				transData.offsetX = input.readFloat() * scale;
				transData.offsetY = input.readFloat() * scale;
				transData.offsetScaleX = input.readFloat();
				transData.offsetScaleY = input.readFloat();
				transData.offsetShearY = input.readFloat();
				transData.rotateMix = input.readFloat();
				transData.translateMix = input.readFloat();
				transData.scaleMix = input.readFloat();
				transData.shearMix = input.readFloat();
				skeletonData.transformConstraints.push(transData);
			}

			// Path constraints.
			n = input.readInt(true);
			for (i = 0, nn; i < n; i++) {
				var pathData : PathConstraintData = new PathConstraintData(input.readString());
				pathData.order = input.readInt(true);
				pathData.skinRequired = input.readBoolean();
				nn = input.readInt(true);
				for (ii = 0; ii < nn; ii++)
					pathData.bones.push(skeletonData.bones[input.readInt(true)]);
				pathData.target = skeletonData.slots[input.readInt(true)];
				pathData.positionMode = PositionMode.values[input.readInt(true)];
				pathData.spacingMode = SpacingMode.values[input.readInt(true)];
				pathData.rotateMode = RotateMode.values[input.readInt(true)];
				pathData.offsetRotation = input.readFloat();
				pathData.position = input.readFloat();
				if (pathData.positionMode == PositionMode.fixed) pathData.position *= scale;
				pathData.spacing = input.readFloat();
				if (pathData.spacingMode == SpacingMode.length || pathData.spacingMode == SpacingMode.fixed) pathData.spacing *= scale;
				pathData.rotateMix = input.readFloat();
				pathData.translateMix = input.readFloat();
				skeletonData.pathConstraints.push(pathData);
			}

			// Default skin.
			var defaultSkin : Skin = readSkin(input, skeletonData, true, nonessential);
			if (defaultSkin != null) {
				skeletonData.defaultSkin = defaultSkin;
				skeletonData.skins.push(defaultSkin);
			}

			// Skins.
			{
				i = skeletonData.skins.length;
				skeletonData.skins.length = n = i + input.readInt(true);
				for (; i < n; i++)
					skeletonData.skins[i] = readSkin(input, skeletonData, false, nonessential);
			}

			// Linked meshes.
			n = this.linkedMeshes.length;
			for (i = 0; i < n; i++) {
				var linkedMesh : LinkedMesh = this.linkedMeshes[i];
				var skin : Skin = linkedMesh.skin == null ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
				if (skin == null) throw new Error("Skin not found: " + linkedMesh.skin);
				var parent : Attachment = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (parent == null) throw new Error("Parent mesh not found: " + linkedMesh.parent);
				linkedMesh.mesh.deformAttachment = linkedMesh.inheritDeform ? parent as VertexAttachment : linkedMesh.mesh;
				linkedMesh.mesh.parentMesh = parent as MeshAttachment;
				linkedMesh.mesh.updateUVs();
			}
			this.linkedMeshes.length = 0;

			// Events.
			n = input.readInt(true);
			for (i = 0; i < n; i++) {
				var data : EventData = new EventData(input.readStringRef());
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
			for (i = 0; i < n; i++)
				skeletonData.animations.push(readAnimation(input, input.readString(), skeletonData));
			return skeletonData;
		}

		private function readSkin (input: BinaryInput, skeletonData: SkeletonData, defaultSkin: Boolean, nonessential: Boolean): Skin {
			var skin : Skin = null;
			var i : int = 0;
			var n : int = 0;
			var ii : int;
			var nn: int;
			var slotCount: int;

			if (defaultSkin) {
				slotCount = input.readInt(true);
				if (slotCount == 0) return null;
				skin = new Skin("default");
			} else {
				skin = new Skin(input.readStringRef());
				skin.bones.length = input.readInt(true);
				for (i = 0, n = skin.bones.length; i < n; i++)
					skin.bones[i] = skeletonData.bones[input.readInt(true)];

				for (i = 0, n = input.readInt(true); i < n; i++)
					skin.constraints.push(skeletonData.ikConstraints[input.readInt(true)]);
				for (i = 0, n = input.readInt(true); i < n; i++)
					skin.constraints.push(skeletonData.transformConstraints[input.readInt(true)]);
				for (i = 0, n = input.readInt(true); i < n; i++)
					skin.constraints.push(skeletonData.pathConstraints[input.readInt(true)]);

				slotCount = input.readInt(true);
			}

			for (i = 0; i < slotCount; i++) {
				var slotIndex : int = input.readInt(true);
				for (ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					var name : String = input.readStringRef();
					var attachment : Attachment = readAttachment(input, skeletonData, skin, slotIndex, name, nonessential);
					if (attachment != null) skin.setAttachment(slotIndex, name, attachment);
				}
			}
			return skin;
		}

		private function readAttachment(input: BinaryInput, skeletonData: SkeletonData, skin: Skin, slotIndex: Number, attachmentName: String, nonessential: Boolean): Attachment {
			var scale : Number = this.scale;
			var i : int = 0;
			var n : int = 0;

			var vertexCount : int;
			var vertices : Vertices;
			var path: String;
			var rotation : Number;
			var x : Number;
			var y: Number;
			var scaleX : Number;
			var scaleY : Number;
			var width : Number;
			var height : Number;
			var color : int;
			var mesh : MeshAttachment;

			var name : String = input.readStringRef();
			if (name == null) name = attachmentName;

			var typeIndex : int = input.readByte();
			var type : AttachmentType = AttachmentType.values[typeIndex];
			switch (type) {
			case AttachmentType.region: {
				path = input.readStringRef();
				rotation = input.readFloat();
				x = input.readFloat();
				y = input.readFloat();
				scaleX = input.readFloat();
				scaleY = input.readFloat();
				width = input.readFloat();
				height = input.readFloat();
				color = input.readInt32();

				if (path == null) path = name;
				var region : RegionAttachment = this.attachmentLoader.newRegionAttachment(skin, name, path);
				if (region == null) return null;
				region.path = path;
				region.x = x * scale;
				region.y = y * scale;
				region.scaleX = scaleX;
				region.scaleY = scaleY;
				region.rotation = rotation;
				region.width = width * scale;
				region.height = height * scale;
				region.color.setFromRgba8888(color);
				region.updateOffset();
				return region;
			}
			case AttachmentType.boundingbox: {
				vertexCount = input.readInt(true);
				vertices = readVertices(input, vertexCount);
				color = nonessential ? input.readInt32() : 0;

				var box : BoundingBoxAttachment = this.attachmentLoader.newBoundingBoxAttachment(skin, name);
				if (box == null) return null;
				box.worldVerticesLength = vertexCount << 1;
				box.vertices = vertices.vertices;
				box.bones = vertices.bones;
				if (nonessential) box.color.setFromRgba8888(color);
				return box;
			}
			case AttachmentType.mesh: {
				path = input.readStringRef();
				color = input.readInt32();
				vertexCount = input.readInt(true);
				var uvs : Vector.<Number> = readFloatArray(input, vertexCount << 1, 1);
				var triangles : Vector.<uint> = readUnsignedShortArray(input);
				vertices = readVertices(input, vertexCount);
				var hullLength : int = input.readInt(true);
				var edges : Vector.<int> = null;
				if (nonessential) {
					edges = readShortArray(input);
					width = input.readFloat();
					height = input.readFloat();
				}

				if (path == null) path = name;
				mesh = this.attachmentLoader.newMeshAttachment(skin, name, path);
				if (mesh == null) return null;
				mesh.path = path;
				mesh.color.setFromRgba8888(color);
				mesh.bones = vertices.bones;
				mesh.vertices = vertices.vertices;
				mesh.worldVerticesLength = vertexCount << 1;
				mesh.triangles = triangles;
				mesh.regionUVs = uvs;
				mesh.updateUVs();
				mesh.hullLength = hullLength << 1;
				if (nonessential) {
					mesh.edges = edges;
					mesh.width = width * scale;
					mesh.height = height * scale;
				}
				return mesh;
			}
			case AttachmentType.linkedmesh: {
				path = input.readStringRef();
				color = input.readInt32();
				var skinName : String = input.readStringRef();
				var parent : String = input.readStringRef();
				var inheritDeform : Boolean = input.readBoolean();
				if (nonessential) {
					width = input.readFloat();
					height = input.readFloat();
				}

				if (path == null) path = name;
				mesh = this.attachmentLoader.newMeshAttachment(skin, name, path);
				if (mesh == null) return null;
				mesh.path = path;
				mesh.color.setFromRgba8888(color);
				if (nonessential) {
					mesh.width = width * scale;
					mesh.height = height * scale;
				}
				this.linkedMeshes.push(new LinkedMesh(mesh, skinName, slotIndex, parent, inheritDeform));
				return mesh;
			}
			case AttachmentType.path: {
				var closed : Boolean = input.readBoolean();
				var constantSpeed : Boolean = input.readBoolean();
				vertexCount = input.readInt(true);
				vertices = this.readVertices(input, vertexCount);
				var lengths : Vector.<Number> = new Vector.<Number>();
				lengths.length = vertexCount / 3;
				for (i = 0, n = lengths.length; i < n; i++)
					lengths[i] = input.readFloat() * scale;
				color = nonessential ? input.readInt32() : 0;

				var pathAttachment : PathAttachment = this.attachmentLoader.newPathAttachment(skin, name);
				if (pathAttachment == null) return null;
				pathAttachment.closed = closed;
				pathAttachment.constantSpeed = constantSpeed;
				pathAttachment.worldVerticesLength = vertexCount << 1;
				pathAttachment.vertices = vertices.vertices;
				pathAttachment.bones = vertices.bones;
				pathAttachment.lengths = lengths;
				if (nonessential) pathAttachment.color.setFromRgba8888(color);
				return pathAttachment;
			}
			case AttachmentType.point: {
				rotation = input.readFloat();
				x = input.readFloat();
				y = input.readFloat();
				color = nonessential ? input.readInt32() : 0;

				var point : PointAttachment = this.attachmentLoader.newPointAttachment(skin, name);
				if (point == null) return null;
				point.x = x * scale;
				point.y = y * scale;
				point.rotation = rotation;
				if (nonessential) point.color.setFromRgba8888(color);
				return point;
			}
			case AttachmentType.clipping: {
				var endSlotIndex : int = input.readInt(true);
				vertexCount = input.readInt(true);
				vertices = this.readVertices(input, vertexCount);
				color = nonessential ? input.readInt32() : 0;

				var clip : ClippingAttachment = this.attachmentLoader.newClippingAttachment(skin, name);
				if (clip == null) return null;
				clip.endSlot = skeletonData.slots[endSlotIndex];
				clip.worldVerticesLength = vertexCount << 1;
				clip.vertices = vertices.vertices;
				clip.bones = vertices.bones;
				if (nonessential) clip.color.setFromRgba8888(color);
				return clip;
			}
			}
			return null;
		}

		private function readVertices (input: BinaryInput, vertexCount: int): Vertices {
			var verticesLength : int = vertexCount << 1;
			var vertices : Vertices = new Vertices();
			var scale : Number = this.scale;
			if (!input.readBoolean()) {
				vertices.vertices = readFloatArray(input, verticesLength, scale);
				return vertices;
			}
			var weights : Vector.<Number> = new Vector.<Number>();
			var bonesArray : Vector.<int> = new Vector.<int>();
			for (var i : int = 0; i < vertexCount; i++) {
				var boneCount : int = input.readInt(true);
				bonesArray.push(boneCount);
				for (var ii : int = 0; ii < boneCount; ii++) {
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

		private function readFloatArray (input: BinaryInput, n: Number, scale: Number): Vector.<Number> {
			var i : int = 0;
			var array : Vector.<Number> = new Vector.<Number>();
			array.length = n;
			if (scale == 1) {
				for (i = 0; i < n; i++)
					array[i] = input.readFloat();
			} else {
				for (i = 0; i < n; i++)
					array[i] = input.readFloat() * scale;
			}
			return array;
		}

		private function readShortArray (input: BinaryInput): Vector.<int> {
			var n : int = input.readInt(true);
			var array : Vector.<int> = new Vector.<int>();
			array.length = n;
			for (var i : int = 0; i < n; i++)
				array[i] = input.readShort();
			return array;
		}

		private function readUnsignedShortArray (input: BinaryInput): Vector.<uint> {
			var n : int = input.readInt(true);
			var array : Vector.<uint> = new Vector.<uint>();
			array.length = n;
			for (var i : int = 0; i < n; i++)
				array[i] = input.readShort();
			return array;
		}

		private function readAnimation (input: BinaryInput, name: String, skeletonData: SkeletonData): Animation {
			var timelines : Vector.<Timeline> = new Vector.<Timeline>();
			var scale : Number = this.scale;
			var duration : Number = 0;
			var tempColor1 : Color = new Color(0, 0, 0, 0);
			var tempColor2 : Color = new Color(0, 0, 0, 0);
			var i : int = 0, n : int = 0, ii : int = 0, nn : int = 0;
			var slotIndex : int;
			var timelineType : int;
			var frameCount : int;
			var frameIndex : int;
			var timelineScale : Number;
			var index : int;
			var time : Number;

			// Slot timelines.
			for (i = 0, n = input.readInt(true); i < n; i++) {
				slotIndex = input.readInt(true);
				for (ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					timelineType = input.readByte();
					frameCount = input.readInt(true);
					frameIndex = 0;
					switch (timelineType) {
					case SkeletonBinary.SLOT_ATTACHMENT: {
						var attachmentTimeline : AttachmentTimeline = new AttachmentTimeline(frameCount);
						attachmentTimeline.slotIndex = slotIndex;
						for (frameIndex = 0; frameIndex < frameCount; frameIndex++)
							attachmentTimeline.setFrame(frameIndex, input.readFloat(), input.readStringRef());
						timelines.push(attachmentTimeline);
						duration = Math.max(duration, attachmentTimeline.frames[frameCount - 1]);
						break;
					}
					case SkeletonBinary.SLOT_COLOR: {
						var colorTimeline : ColorTimeline = new ColorTimeline(frameCount);
						colorTimeline.slotIndex = slotIndex;
						for (frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							time = input.readFloat();
							tempColor1.setFromRgba8888(input.readInt32());
							colorTimeline.setFrame(frameIndex, time, tempColor1.r, tempColor1.g, tempColor1.b, tempColor1.a);
							if (frameIndex < frameCount - 1) readCurve(input, frameIndex, colorTimeline);
						}
						timelines.push(colorTimeline);
						duration = Math.max(duration, colorTimeline.frames[(frameCount - 1) * ColorTimeline.ENTRIES]);
						break;
					}
					case SkeletonBinary.SLOT_TWO_COLOR: {
						var twoColorTimeline : TwoColorTimeline = new TwoColorTimeline(frameCount);
						twoColorTimeline.slotIndex = slotIndex;
						for (frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							time = input.readFloat();
							tempColor1.setFromRgba8888(input.readInt32());
							tempColor2.setFromRgb888(input.readInt32());
							twoColorTimeline.setFrame(frameIndex, time, tempColor1.r, tempColor1.g, tempColor1.b, tempColor1.a, tempColor2.r,
								tempColor2.g, tempColor2.b);
							if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, twoColorTimeline);
						}
						timelines.push(twoColorTimeline);
						duration = Math.max(duration, twoColorTimeline.frames[(frameCount - 1) * TwoColorTimeline.ENTRIES]);
						break;
					}
					}
				}
			}

			// Bone timelines.
			for (i = 0, n = input.readInt(true); i < n; i++) {
				var boneIndex : int = input.readInt(true);
				for (ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					timelineType = input.readByte();
					frameCount = input.readInt(true);
					frameIndex = 0;
					switch (timelineType) {
					case SkeletonBinary.BONE_ROTATE: {
						var rotateTimeline : RotateTimeline = new RotateTimeline(frameCount);
						rotateTimeline.boneIndex = boneIndex;
						for (frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							rotateTimeline.setFrame(frameIndex, input.readFloat(), input.readFloat());
							if (frameIndex < frameCount - 1) readCurve(input, frameIndex, rotateTimeline);
						}
						timelines.push(rotateTimeline);
						duration = Math.max(duration, rotateTimeline.frames[(frameCount - 1) * RotateTimeline.ENTRIES]);
						break;
					}
					case SkeletonBinary.BONE_TRANSLATE:
					case SkeletonBinary.BONE_SCALE:
					case SkeletonBinary.BONE_SHEAR: {
						var translateTimeline : TranslateTimeline;
						timelineScale = 1;
						if (timelineType == SkeletonBinary.BONE_SCALE)
							translateTimeline = new ScaleTimeline(frameCount);
						else if (timelineType == SkeletonBinary.BONE_SHEAR)
							translateTimeline = new ShearTimeline(frameCount);
						else {
							translateTimeline = new TranslateTimeline(frameCount);
							timelineScale = scale;
						}
						translateTimeline.boneIndex = boneIndex;
						for (frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							translateTimeline.setFrame(frameIndex, input.readFloat(), input.readFloat() * timelineScale,
								input.readFloat() * timelineScale);
							if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, translateTimeline);
						}
						timelines.push(translateTimeline);
						duration = Math.max(duration, translateTimeline.frames[(frameCount - 1) * TranslateTimeline.ENTRIES]);
						break;
					}
					}
				}
			}

			// IK constraint timelines.
			for (i = 0, n = input.readInt(true); i < n; i++) {
				index = input.readInt(true);
				frameCount = input.readInt(true);
				var ikConstraintTimeline : IkConstraintTimeline = new IkConstraintTimeline(frameCount);
				frameIndex = 0;
				ikConstraintTimeline.ikConstraintIndex = index;
				for (frameIndex = 0; frameIndex < frameCount; frameIndex++) {
					ikConstraintTimeline.setFrame(frameIndex, input.readFloat(), input.readFloat(), input.readFloat() * scale, input.readByte(), input.readBoolean(),
						input.readBoolean());
					if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, ikConstraintTimeline);
				}
				timelines.push(ikConstraintTimeline);
				duration = Math.max(duration, ikConstraintTimeline.frames[(frameCount - 1) * IkConstraintTimeline.ENTRIES]);
			}

			// Transform constraint timelines.
			for (i = 0, n = input.readInt(true); i < n; i++) {
				index = input.readInt(true);
				frameCount = input.readInt(true);
				var transformConstraintTimeline : TransformConstraintTimeline = new TransformConstraintTimeline(frameCount);
				transformConstraintTimeline.transformConstraintIndex = index;
				for (frameIndex = 0; frameIndex < frameCount; frameIndex++) {
					transformConstraintTimeline.setFrame(frameIndex, input.readFloat(), input.readFloat(), input.readFloat(), input.readFloat(),
						input.readFloat());
					if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, transformConstraintTimeline);
				}
				timelines.push(transformConstraintTimeline);
				duration = Math.max(duration, transformConstraintTimeline.frames[(frameCount - 1) * TransformConstraintTimeline.ENTRIES]);
			}

			// Path constraint timelines.
			for (i = 0, n = input.readInt(true); i < n; i++) {
				index = input.readInt(true);
				var data : PathConstraintData = skeletonData.pathConstraints[index];
				for (ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					timelineType = input.readByte();
					frameCount = input.readInt(true);
					switch (timelineType) {
					case SkeletonBinary.PATH_POSITION:
					case SkeletonBinary.PATH_SPACING: {
						var pathConstraintPositionTimeline : PathConstraintPositionTimeline;
						timelineScale = 1;
						if (timelineType == SkeletonBinary.PATH_SPACING) {
							pathConstraintPositionTimeline = new PathConstraintSpacingTimeline(frameCount);
							if (data.spacingMode == SpacingMode.length || data.spacingMode == SpacingMode.fixed) timelineScale = scale;
						} else {
							pathConstraintPositionTimeline = new PathConstraintPositionTimeline(frameCount);
							if (data.positionMode == PositionMode.fixed) timelineScale = scale;
						}
						pathConstraintPositionTimeline.pathConstraintIndex = index;
						for (frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							pathConstraintPositionTimeline.setFrame(frameIndex, input.readFloat(), input.readFloat() * timelineScale);
							if (frameIndex < frameCount - 1) readCurve(input, frameIndex, pathConstraintPositionTimeline);
						}
						timelines.push(pathConstraintPositionTimeline);
						duration = Math.max(duration, pathConstraintPositionTimeline.frames[(frameCount - 1) * PathConstraintPositionTimeline.ENTRIES]);
						break;
					}
					case SkeletonBinary.PATH_MIX: {
						var pathConstraintMixTimeline : PathConstraintMixTimeline = new PathConstraintMixTimeline(frameCount);
						pathConstraintMixTimeline.pathConstraintIndex = index;
						for (frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							pathConstraintMixTimeline.setFrame(frameIndex, input.readFloat(), input.readFloat(), input.readFloat());
							if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, pathConstraintMixTimeline);
						}
						timelines.push(pathConstraintMixTimeline);
						duration = Math.max(duration, pathConstraintMixTimeline.frames[(frameCount - 1) * PathConstraintMixTimeline.ENTRIES]);
						break;
					}
					}
				}
			}

			// Deform timelines.
			for (i = 0, n = input.readInt(true); i < n; i++) {
				var skin : Skin = skeletonData.skins[input.readInt(true)];
				for (ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					slotIndex = input.readInt(true);
					for (var iii : int = 0, nnn : int = input.readInt(true); iii < nnn; iii++) {
						var attachment : VertexAttachment = skin.getAttachment(slotIndex, input.readStringRef()) as VertexAttachment;
						var weighted : Boolean = attachment.bones != null;
						var vertices : Vector.<Number> = attachment.vertices;
						var deformLength : int = weighted ? vertices.length / 3 * 2 : vertices.length;

						frameCount = input.readInt(true);
						var deformTimeline : DeformTimeline= new DeformTimeline(frameCount);
						deformTimeline.slotIndex = slotIndex;
						deformTimeline.attachment = attachment;

						for (frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							time = input.readFloat();
							var deform : Vector.<Number>;
							var end : int = input.readInt(true);
							if (end == 0) {
								if (weighted) {
									deform = new Vector.<Number>();
									deform.length = deformLength;
								} else
									deform = vertices;
							} else {
								var v : int, vn: int;
								deform = new Vector.<Number>();
								deform.length = deformLength;
								var start : int = input.readInt(true);
								end += start;
								if (scale == 1) {
									for (v = start; v < end; v++)
										deform[v] = input.readFloat();
								} else {
									for (v = start; v < end; v++)
										deform[v] = input.readFloat() * scale;
								}
								if (!weighted) {
									for (v = 0, vn = deform.length; v < vn; v++)
										deform[v] += vertices[v];
								}
							}

							deformTimeline.setFrame(frameIndex, time, deform);
							if (frameIndex < frameCount - 1) readCurve(input, frameIndex, deformTimeline);
						}
						timelines.push(deformTimeline);
						duration = Math.max(duration, deformTimeline.frames[frameCount - 1]);
					}
				}
			}

			// Draw order timeline.
			var drawOrderCount : int = input.readInt(true);
			if (drawOrderCount > 0) {
				var drawOrderTimeline : DrawOrderTimeline = new DrawOrderTimeline(drawOrderCount);
				var slotCount : int = skeletonData.slots.length;
				for (i = 0; i < drawOrderCount; i++) {
					time = input.readFloat();
					var offsetCount : int = input.readInt(true);
					var drawOrder : Vector.<int> = new Vector.<int>();
					drawOrder.length = slotCount;
					for (ii = slotCount - 1; ii >= 0; ii--)
						drawOrder[ii] = -1;
					var unchanged : Vector.<int> = new Vector.<int>();
					unchanged.length = slotCount - offsetCount;
					var originalIndex : int = 0, unchangedIndex : int = 0;
					for (ii = 0; ii < offsetCount; ii++) {
						slotIndex = input.readInt(true);
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
					for (ii = slotCount - 1; ii >= 0; ii--)
						if (drawOrder[ii] == -1) drawOrder[ii] = unchanged[--unchangedIndex];
					drawOrderTimeline.setFrame(i, time, drawOrder);
				}
				timelines.push(drawOrderTimeline);
				duration = Math.max(duration, drawOrderTimeline.frames[drawOrderCount - 1]);
			}

			// Event timeline.
			var eventCount : int = input.readInt(true);
			if (eventCount > 0) {
				var eventTimeline : EventTimeline = new EventTimeline(eventCount);
				for (i = 0; i < eventCount; i++) {
					time = input.readFloat();
					var eventData : EventData = skeletonData.events[input.readInt(true)];
					var event : Event = new Event(time, eventData);
					event.intValue = input.readInt(false);
					event.floatValue = input.readFloat();
					event.stringValue = input.readBoolean() ? input.readString() : eventData.stringValue;
					if (event.data.audioPath != null) {
						event.volume = input.readFloat();
						event.balance = input.readFloat();
					}
					eventTimeline.setFrame(i, event);
				}
				timelines.push(eventTimeline);
				duration = Math.max(duration, eventTimeline.frames[eventCount - 1]);
			}

			return new Animation(name, timelines, duration);
		}

		private function readCurve (input: BinaryInput, frameIndex: Number, timeline: CurveTimeline) : void {
			switch (input.readByte()) {
			case SkeletonBinary.CURVE_STEPPED:
				timeline.setStepped(frameIndex);
				break;
			case SkeletonBinary.CURVE_BEZIER:
				setCurve(timeline, frameIndex, input.readFloat(), input.readFloat(), input.readFloat(), input.readFloat());
				break;
			}
		}

		public function setCurve (timeline: CurveTimeline, frameIndex: Number, cx1: Number, cy1: Number, cx2: Number, cy2: Number) : void {
			timeline.setCurve(frameIndex, cx1, cy1, cx2, cy2);
		}

	}
}

import spine.attachments.MeshAttachment;

class Vertices {
	public var vertices : Vector.<Number> = new Vector.<Number>();
	public var bones : Vector.<int> = new Vector.<int>();
}

class LinkedMesh {
	internal var parent : String, skin : String;
	internal var slotIndex : int;
	internal var mesh : MeshAttachment;
	internal var inheritDeform : Boolean;

	public function LinkedMesh(mesh : MeshAttachment, skin : String, slotIndex : int, parent : String, inheritDeform : Boolean) {
		this.mesh = mesh;
		this.skin = skin;
		this.slotIndex = slotIndex;
		this.parent = parent;
		this.inheritDeform = inheritDeform;
	}
}
