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
	import spine.animation.*;
	import spine.attachments.*;
	import flash.utils.ByteArray;

	public class SkeletonBinary {
		public var attachmentLoader : AttachmentLoader;
		public var scale : Number = 1;
		private var linkedMeshes : Vector.<LinkedMesh> = new Vector.<LinkedMesh>();

		private static const BONE_ROTATE : int = 0;
		private static const BONE_TRANSLATE : int = 1;
		private static const BONE_TRANSLATEX : int = 2;
		private static const BONE_TRANSLATEY : int = 3;
		private static const BONE_SCALE : int = 4;
		private static const BONE_SCALEX : int = 5;
		private static const BONE_SCALEY : int = 6;
		private static const BONE_SHEAR : int = 7;
		private static const BONE_SHEARX : int = 8;
		private static const BONE_SHEARY : int = 9;

		private static const SLOT_ATTACHMENT : int = 0;
		private static const SLOT_RGBA : int = 1;
		private static const SLOT_RGB : int = 2;
		private static const SLOT_RGBA2 : int = 3;
		private static const SLOT_RGB2 : int = 4;
		private static const SLOT_ALPHA : int = 5;

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

			var lowHash : int = input.readInt32();
			var highHash : int = input.readInt32();
			skeletonData.hash = highHash == 0 && lowHash == 0 ? null : highHash.toString(16) + lowHash.toString(16);
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
				var transformData : TransformConstraintData = new TransformConstraintData(input.readString());
				transformData.order = input.readInt(true);
				transformData.skinRequired = input.readBoolean();
				nn = input.readInt(true);
				for (ii = 0; ii < nn; ii++)
					transformData.bones.push(skeletonData.bones[input.readInt(true)]);
				transformData.target = skeletonData.bones[input.readInt(true)];
				transformData.local = input.readBoolean();
				transformData.relative = input.readBoolean();
				transformData.offsetRotation = input.readFloat();
				transformData.offsetX = input.readFloat() * scale;
				transformData.offsetY = input.readFloat() * scale;
				transformData.offsetScaleX = input.readFloat();
				transformData.offsetScaleY = input.readFloat();
				transformData.offsetShearY = input.readFloat();
				transformData.mixRotate = input.readFloat();
				transformData.mixX = input.readFloat();
				transformData.mixY = input.readFloat();
				transformData.mixScaleX = input.readFloat();
				transformData.mixScaleY = input.readFloat();
				transformData.mixShearY = input.readFloat();
				skeletonData.transformConstraints.push(transformData);
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
				pathData.mixRotate = input.readFloat();
				pathData.mixX = input.readFloat();
				pathData.mixY = input.readFloat();
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

		private function readAttachment(input: BinaryInput, skeletonData: SkeletonData, skin: Skin, slotIndex: int, attachmentName: String, nonessential: Boolean): Attachment {
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

			switch (AttachmentType.values[input.readByte()]) {
			case AttachmentType.region:
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
			case AttachmentType.boundingbox:
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
			case AttachmentType.mesh:
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
			case AttachmentType.linkedmesh:
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
			case AttachmentType.path:
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
			case AttachmentType.point:
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
			case AttachmentType.clipping:
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
			return null;
		}

		private function readVertices (input: BinaryInput, vertexCount: int): Vertices {
			var scale : Number = this.scale;
			var verticesLength : int = vertexCount << 1;
			var vertices : Vertices = new Vertices();
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

		private function readFloatArray (input: BinaryInput, n: int, scale: Number): Vector.<Number> {
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
			input.readInt(true); // Number of timelines.
			var timelines : Vector.<Timeline> = new Vector.<Timeline>();
			var scale : Number = this.scale;
			var i : int = 0, n : int = 0, ii : int = 0, nn : int = 0;
			var index : int, slotIndex : int, timelineType : int, timelineScale : Number;
			var frameCount : int, frameLast : int, frame : int, bezierCount : int, bezier : int;
			var time : Number, time2 : Number;

			// Slot timelines.
			var r : Number, g : Number, b : Number, a : Number;
			var r2 : Number, g2 : Number, b2 : Number, a2 : Number;
			var nr : Number, ng : Number, nb : Number, na : Number;
			var nr2 : Number, ng2 : Number, nb2 : Number, na2 : Number;
			for (i = 0, n = input.readInt(true); i < n; i++) {
				slotIndex = input.readInt(true);
				for (ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					timelineType = input.readByte();
					frameCount = input.readInt(true);
					frameLast = frameCount - 1;
					switch (timelineType) {
					case SLOT_ATTACHMENT:
						var attachmentTimeline : AttachmentTimeline = new AttachmentTimeline(frameCount, slotIndex);
						for (frame = 0; frame < frameCount; frame++)
							attachmentTimeline.setFrame(frame, input.readFloat(), input.readStringRef());
						timelines.push(attachmentTimeline);
						break;
					case SLOT_RGBA:
						bezierCount = input.readInt(true);
						var rgbaTimeline : RGBATimeline = new RGBATimeline(frameCount, bezierCount, slotIndex);

						time = input.readFloat();
						r = input.readUnsignedByte() / 255.0;
						g = input.readUnsignedByte() / 255.0;
						b = input.readUnsignedByte() / 255.0;
						a = input.readUnsignedByte() / 255.0;

						for (frame = 0, bezier = 0;; frame++) {
							rgbaTimeline.setFrame(frame, time, r, g, b, a);
							if (frame == frameLast) break;

							time2 = input.readFloat();
							r2 = input.readUnsignedByte() / 255.0;
							g2 = input.readUnsignedByte() / 255.0;
							b2 = input.readUnsignedByte() / 255.0;
							a2 = input.readUnsignedByte() / 255.0;

							switch (input.readByte()) {
							case CURVE_STEPPED:
								rgbaTimeline.setStepped(frame);
								break;
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
						}
						timelines.push(rgbaTimeline);
						break;
					case SLOT_RGB:
						bezierCount = input.readInt(true);
						var rgbTimeline : RGBTimeline = new RGBTimeline(frameCount, bezierCount, slotIndex);

						time = input.readFloat();
						r = input.readUnsignedByte() / 255.0;
						g = input.readUnsignedByte() / 255.0;
						b = input.readUnsignedByte() / 255.0;

						for (frame = 0, bezier = 0;; frame++) {
							rgbTimeline.setFrame(frame, time, r, g, b);
							if (frame == frameLast) break;

							time2 = input.readFloat();
							r2 = input.readUnsignedByte() / 255.0;
							g2 = input.readUnsignedByte() / 255.0;
							b2 = input.readUnsignedByte() / 255.0;

							switch (input.readByte()) {
							case CURVE_STEPPED:
								rgbTimeline.setStepped(frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, rgbTimeline, bezier++, frame, 0, time, time2, r, r2, 1);
								setBezier(input, rgbTimeline, bezier++, frame, 1, time, time2, g, g2, 1);
								setBezier(input, rgbTimeline, bezier++, frame, 2, time, time2, b, b2, 1);
							}
							time = time2;
							r = r2;
							g = g2;
							b = b2;
						}
						timelines.push(rgbTimeline);
						break;
					case SLOT_RGBA2:
						bezierCount = input.readInt(true);
						var rgba2Timeline : RGBA2Timeline = new RGBA2Timeline(frameCount, bezierCount, slotIndex);

						time = input.readFloat();
						r = input.readUnsignedByte() / 255.0;
						g = input.readUnsignedByte() / 255.0;
						b = input.readUnsignedByte() / 255.0;
						a = input.readUnsignedByte() / 255.0;
						r2 = input.readUnsignedByte() / 255.0;
						g2 = input.readUnsignedByte() / 255.0;
						b2 = input.readUnsignedByte() / 255.0;

						for (frame = 0, bezier = 0;; frame++) {
							rgba2Timeline.setFrame(frame, time, r, g, b, a, r2, g2, b2);
							if (frame == frameLast) break;
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
								break;
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
						}
						timelines.push(rgba2Timeline);
						break;
					case SLOT_RGB2:
						bezierCount = input.readInt(true);
						var rgb2Timeline : RGB2Timeline = new RGB2Timeline(frameCount, bezierCount, slotIndex);

						time = input.readFloat();
						r = input.readUnsignedByte() / 255.0;
						g = input.readUnsignedByte() / 255.0;
						b = input.readUnsignedByte() / 255.0;
						r2 = input.readUnsignedByte() / 255.0;
						g2 = input.readUnsignedByte() / 255.0;
						b2 = input.readUnsignedByte() / 255.0;

						for (frame = 0, bezier = 0;; frame++) {
							rgb2Timeline.setFrame(frame, time, r, g, b, r2, g2, b2);
							if (frame == frameLast) break;
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
								break;
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
						}
						timelines.push(rgb2Timeline);
						break;
					case SLOT_ALPHA:
						var alphaTimeline : AlphaTimeline = new AlphaTimeline(frameCount, input.readInt(true), slotIndex);
						time = input.readFloat();
						a = input.readUnsignedByte() / 255;
						for (frame = 0, bezier = 0;; frame++) {
							alphaTimeline.setFrame(frame, time, a);
							if (frame == frameLast) break;
							time2 = input.readFloat();
							a2 = input.readUnsignedByte() / 255;
							switch (input.readByte()) {
							case CURVE_STEPPED:
								alphaTimeline.setStepped(frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, alphaTimeline, bezier++, frame, 0, time, time2, a, a2, 1);
							}
							time = time2;
							a = a2;
						}
						timelines.push(alphaTimeline);
					}
				}
			}

			// Bone timelines.
			for (i = 0, n = input.readInt(true); i < n; i++) {
				var boneIndex : int = input.readInt(true);
				for (ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					timelineType = input.readByte();
					frameCount = input.readInt(true);
					bezierCount = input.readInt(true);
					switch (timelineType) {
					case BONE_ROTATE:
						timelines.push(readTimeline(input, new RotateTimeline(frameCount, bezierCount, boneIndex), 1));
						break;
					case BONE_TRANSLATE:
						timelines.push(readTimeline2(input, new TranslateTimeline(frameCount, bezierCount, boneIndex), scale));
						break;
					case BONE_TRANSLATEX:
						timelines.push(readTimeline(input, new TranslateXTimeline(frameCount, bezierCount, boneIndex), scale));
						break;
					case BONE_TRANSLATEY:
						timelines.push(readTimeline(input, new TranslateYTimeline(frameCount, bezierCount, boneIndex), scale));
						break;
					case BONE_SCALE:
						timelines.push(readTimeline2(input, new ScaleTimeline(frameCount, bezierCount, boneIndex), 1));
						break;
					case BONE_SCALEX:
						timelines.push(readTimeline(input, new ScaleXTimeline(frameCount, bezierCount, boneIndex), 1));
						break;
					case BONE_SCALEY:
						timelines.push(readTimeline(input, new ScaleYTimeline(frameCount, bezierCount, boneIndex), 1));
						break;
					case BONE_SHEAR:
						timelines.push(readTimeline2(input, new ShearTimeline(frameCount, bezierCount, boneIndex), 1));
						break;
					case BONE_SHEARX:
						timelines.push(readTimeline(input, new ShearXTimeline(frameCount, bezierCount, boneIndex), 1));
						break;
					case BONE_SHEARY:
						timelines.push(readTimeline(input, new ShearYTimeline(frameCount, bezierCount, boneIndex), 1));
					}
				}
			}

			// IK constraint timelines.
			for (i = 0, n = input.readInt(true); i < n; i++) {
				index = input.readInt(true);
				frameCount = input.readInt(true);
				frameLast = frameCount - 1;
				var ikTimeline : IkConstraintTimeline = new IkConstraintTimeline(frameCount, input.readInt(true), index);
				time = input.readFloat();
				var mix : Number = input.readFloat(), softness : Number = input.readFloat() * scale;
				for (frame = 0, bezier = 0;; frame++) {
					ikTimeline.setFrame(frame, time, mix, softness, input.readByte(), input.readBoolean(), input.readBoolean());
					if (frame == frameLast) break;
					time2 = input.readFloat();
					var mix2 : Number = input.readFloat(), softness2 : Number = input.readFloat() * scale;
					switch (input.readByte()) {
					case CURVE_STEPPED:
						ikTimeline.setStepped(frame);
						break;
					case CURVE_BEZIER:
						setBezier(input, ikTimeline, bezier++, frame, 0, time, time2, mix, mix2, 1);
						setBezier(input, ikTimeline, bezier++, frame, 1, time, time2, softness, softness2, scale);
					}
					time = time2;
					mix = mix2;
					softness = softness2;
				}
				timelines.push(ikTimeline);
			}

			// Transform constraint timelines.
			var mixRotate : Number, mixRotate2 : Number;
			var mixX : Number, mixX2 : Number;
			var mixY : Number, mixY2 : Number;
			for (i = 0, n = input.readInt(true); i < n; i++) {
				index = input.readInt(true);
				frameCount = input.readInt(true);
				frameLast = frameCount - 1;
				var transformTimeline : TransformConstraintTimeline = new TransformConstraintTimeline(frameCount, input.readInt(true), index);
				time = input.readFloat();
				mixRotate = input.readFloat();
				mixX = input.readFloat();
				mixY = input.readFloat();
				var mixScaleX : Number = input.readFloat(), mixScaleY : Number = input.readFloat(), mixShearY : Number = input.readFloat();
				for (frame = 0, bezier = 0;; frame++) {
					transformTimeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
					if (frame == frameLast) break;
					time2 = input.readFloat()
					mixRotate2 = input.readFloat();
					mixX2 = input.readFloat();
					mixY2 = input.readFloat();
					var mixScaleX2 : Number = input.readFloat(), mixScaleY2 : Number = input.readFloat(), mixShearY2 : Number = input.readFloat();
					switch (input.readByte()) {
					case CURVE_STEPPED:
						transformTimeline.setStepped(frame);
						break;
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
				}
				timelines.push(transformTimeline);
			}

			// Path constraint timelines.
			for (i = 0, n = input.readInt(true); i < n; i++) {
				index = input.readInt(true);
				var data : PathConstraintData = skeletonData.pathConstraints[index];
				for (ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					switch (input.readByte()) {
					case PATH_POSITION:
						timelines
							.push(readTimeline(input, new PathConstraintPositionTimeline(input.readInt(true), input.readInt(true), index),
								data.positionMode == PositionMode.fixed ? scale : 1));
						break;
					case PATH_SPACING:
						timelines
							.push(readTimeline(input, new PathConstraintSpacingTimeline(input.readInt(true), input.readInt(true), index),
								data.spacingMode == SpacingMode.length || data.spacingMode == SpacingMode.fixed ? scale : 1));
						break;
					case PATH_MIX:
						var mixTimeline : PathConstraintMixTimeline = new PathConstraintMixTimeline(input.readInt(true), input.readInt(true), index);
						time = input.readFloat();
						mixRotate = input.readFloat();
						mixX = input.readFloat();
						mixY = input.readFloat();
						for (frame = 0, bezier = 0, frameLast = mixTimeline.getFrameCount() - 1;; frame++) {
							mixTimeline.setFrame(frame, time, mixRotate, mixX, mixY);
							if (frame == frameLast) break;
							time2 = input.readFloat();
							mixRotate2 = input.readFloat();
							mixX2 = input.readFloat();
							mixY2 = input.readFloat();
							switch (input.readByte()) {
							case CURVE_STEPPED:
								mixTimeline.setStepped(frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, mixTimeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
								setBezier(input, mixTimeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
								setBezier(input, mixTimeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
							}
							time = time2;
							mixRotate = mixRotate2;
							mixX = mixX2;
							mixY = mixY2;
						}
						timelines.push(mixTimeline);
					}
				}
			}

			// Deform timelines.
			for (i = 0, n = input.readInt(true); i < n; i++) {
				var skin : Skin = skeletonData.skins[input.readInt(true)];
				for (ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					slotIndex = input.readInt(true);
					for (var iii : int = 0, nnn : int = input.readInt(true); iii < nnn; iii++) {
						var attachmentName : String = input.readStringRef();
						var attachment : VertexAttachment = skin.getAttachment(slotIndex, attachmentName) as VertexAttachment;
						if (attachment == null) throw Error("Vertex attachment not found: " + attachmentName);
						var weighted : Boolean = attachment.bones != null;
						var vertices : Vector.<Number> = attachment.vertices;
						var deformLength : int = weighted ? vertices.length / 3 * 2 : vertices.length;

						frameCount = input.readInt(true);
						frameLast = frameCount - 1;
						bezierCount = input.readInt(true);
						var deformTimeline : DeformTimeline = new DeformTimeline(frameCount, bezierCount, slotIndex, attachment);

						time = input.readFloat();
						for (frame = 0, bezier = 0;; frame++) {
							var deform : Vector.<Number>;
							var end : int = input.readInt(true);
							if (end == 0) {
								if (weighted) {
									deform = new Vector.<Number>();
									deform.length = deformLength;
								} else
									deform = vertices;
							} else {
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

							deformTimeline.setFrame(frame, time, deform);
							if (frame == frameLast) break;
							time2 = input.readFloat();
							switch(input.readByte()) {
							case CURVE_STEPPED:
								deformTimeline.setStepped(frame);
								break;
							case CURVE_BEZIER:
								SkeletonBinary.setBezier(input, deformTimeline, bezier++, frame, 0, time, time2, 0, 1, 1);
							}
							time = time2;
						}
						timelines.push(deformTimeline);
					}
				}
			}

			// Draw order timelines.
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
			}

			// Event timelines.
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
			}

			var duration : Number = 0;
			for (i = 0, n = timelines.length; i < n; i++)
				duration = Math.max(duration, timelines[i].getDuration());
			return new Animation(name, timelines, duration);
		}

		static private function readTimeline (input: BinaryInput, timeline: CurveTimeline1, scale: Number) : CurveTimeline1 {
			var time : Number = input.readFloat(), value : Number = input.readFloat() * scale;
			for (var frame : int = 0, bezier : int = 0, frameLast : int = timeline.getFrameCount() - 1;; frame++) {
				timeline.setFrame(frame, time, value);
				if (frame == frameLast) break;
				var time2 : Number = input.readFloat(), value2 : Number = input.readFloat() * scale;
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
			return timeline;
		}

		static private function readTimeline2 (input: BinaryInput, timeline: CurveTimeline2, scale: Number) : CurveTimeline2 {
			var time : Number = input.readFloat(), value1 : Number = input.readFloat() * scale, value2 : Number = input.readFloat() * scale;
			for (var frame : int = 0, bezier : int = 0, frameLast : int = timeline.getFrameCount() - 1;; frame++) {
				timeline.setFrame(frame, time, value1, value2);
				if (frame == frameLast) break;
				var time2 : Number = input.readFloat(), nvalue1 : Number = input.readFloat() * scale, nvalue2 : Number = input.readFloat() * scale;
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
			return timeline;
		}

		static private function setBezier (input: BinaryInput, timeline: CurveTimeline, bezier: Number, frame: Number, value: Number,
			time1: Number, time2: Number, value1: Number, value2: Number, scale: Number) : void {
			timeline.setBezier(bezier, frame, value, time1, value1, input.readFloat(), input.readFloat() * scale, input.readFloat(), input.readFloat() * scale, time2, value2);
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
