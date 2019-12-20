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

module spine {
	/** Loads skeleton data in the Spine binary format.
	 *
	 * See [Spine binary format](http://esotericsoftware.com/spine-binary-format) and
	 * [JSON and binary data](http://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data) in the Spine
	 * Runtimes Guide. */
	export class SkeletonBinary {
		static AttachmentTypeValues = [ 0 /*AttachmentType.Region*/, 1/*AttachmentType.BoundingBox*/, 2/*AttachmentType.Mesh*/, 3/*AttachmentType.LinkedMesh*/, 4/*AttachmentType.Path*/, 5/*AttachmentType.Point*/, 6/*AttachmentType.Clipping*/ ];
		static TransformModeValues = [TransformMode.Normal, TransformMode.OnlyTranslation, TransformMode.NoRotationOrReflection, TransformMode.NoScale, TransformMode.NoScaleOrReflection];
		static PositionModeValues = [ PositionMode.Fixed, PositionMode.Percent ];
		static SpacingModeValues = [ SpacingMode.Length, SpacingMode.Fixed, SpacingMode.Percent];
		static RotateModeValues = [ RotateMode.Tangent, RotateMode.Chain, RotateMode.ChainScale ];
		static BlendModeValues = [ BlendMode.Normal, BlendMode.Additive, BlendMode.Multiply, BlendMode.Screen];

		static BONE_ROTATE = 0;
		static BONE_TRANSLATE = 1;
		static BONE_SCALE = 2;
		static BONE_SHEAR = 3;

		static SLOT_ATTACHMENT = 0;
		static SLOT_COLOR = 1;
		static SLOT_TWO_COLOR = 2;

		static PATH_POSITION = 0;
		static PATH_SPACING = 1;
		static PATH_MIX = 2;

		static CURVE_LINEAR = 0;
		static CURVE_STEPPED = 1;
		static CURVE_BEZIER = 2;

		/** Scales bone positions, image sizes, and translations as they are loaded. This allows different size images to be used at
		 * runtime than were used in Spine.
		 *
		 * See [Scaling](http://esotericsoftware.com/spine-loading-skeleton-data#Scaling) in the Spine Runtimes Guide. */
		scale = 1;

		attachmentLoader: AttachmentLoader;
		private linkedMeshes = new Array<LinkedMesh>();

		constructor (attachmentLoader: AttachmentLoader) {
			this.attachmentLoader = attachmentLoader;
		}

		readSkeletonData (binary: Uint8Array): SkeletonData {
			let scale = this.scale;

			let skeletonData = new SkeletonData();
			skeletonData.name = ""; // BOZO

			let input = new BinaryInput(binary);

			skeletonData.hash = input.readString();
			skeletonData.version = input.readString();
			if ("3.8.75" == skeletonData.version)
					throw new Error("Unsupported skeleton data, please export with a newer version of Spine.");
			skeletonData.x = input.readFloat();
			skeletonData.y = input.readFloat();
			skeletonData.width = input.readFloat();
			skeletonData.height = input.readFloat();

			let nonessential = input.readBoolean();
			if (nonessential) {
				skeletonData.fps = input.readFloat();

				skeletonData.imagesPath = input.readString();
				skeletonData.audioPath = input.readString();
			}

			let n = 0;
			// Strings.
			n = input.readInt(true)
			for (let i = 0; i < n; i++)
				input.strings.push(input.readString());

			// Bones.
			n = input.readInt(true)
			for (let i = 0; i < n; i++) {
				let name = input.readString();
				let parent = i == 0 ? null : skeletonData.bones[input.readInt(true)];
				let data = new BoneData(i, name, parent);
				data.rotation = input.readFloat();
				data.x = input.readFloat() * scale;
				data.y = input.readFloat() * scale;
				data.scaleX = input.readFloat();
				data.scaleY = input.readFloat();
				data.shearX = input.readFloat();
				data.shearY = input.readFloat();
				data.length = input.readFloat() * scale;
				data.transformMode = SkeletonBinary.TransformModeValues[input.readInt(true)];
				data.skinRequired = input.readBoolean();
				if (nonessential) Color.rgba8888ToColor(data.color, input.readInt32());
				skeletonData.bones.push(data);
			}

			// Slots.
			n = input.readInt(true);
			for (let i = 0; i < n; i++) {
				let slotName = input.readString();
				let boneData = skeletonData.bones[input.readInt(true)];
				let data = new SlotData(i, slotName, boneData);
				Color.rgba8888ToColor(data.color, input.readInt32());

				let darkColor = input.readInt32();
				if (darkColor != -1) Color.rgb888ToColor(data.darkColor = new Color(), darkColor);

				data.attachmentName = input.readStringRef();
				data.blendMode = SkeletonBinary.BlendModeValues[input.readInt(true)];
				skeletonData.slots.push(data);
			}

			// IK constraints.
			n = input.readInt(true);
			for (let i = 0, nn; i < n; i++) {
				let data = new IkConstraintData(input.readString());
				data.order = input.readInt(true);
				data.skinRequired = input.readBoolean();
				nn = input.readInt(true);
				for (let ii = 0; ii < nn; ii++)
					data.bones.push(skeletonData.bones[input.readInt(true)]);
				data.target = skeletonData.bones[input.readInt(true)];
				data.mix = input.readFloat();
				data.softness = input.readFloat() * scale;
				data.bendDirection = input.readByte();
				data.compress = input.readBoolean();
				data.stretch = input.readBoolean();
				data.uniform = input.readBoolean();
				skeletonData.ikConstraints.push(data);
			}

			// Transform constraints.
			n = input.readInt(true);
			for (let i = 0, nn; i < n; i++) {
				let data = new TransformConstraintData(input.readString());
				data.order = input.readInt(true);
				data.skinRequired = input.readBoolean();
				nn = input.readInt(true);
				for (let ii = 0; ii < nn; ii++)
					data.bones.push(skeletonData.bones[input.readInt(true)]);
				data.target = skeletonData.bones[input.readInt(true)];
				data.local = input.readBoolean();
				data.relative = input.readBoolean();
				data.offsetRotation = input.readFloat();
				data.offsetX = input.readFloat() * scale;
				data.offsetY = input.readFloat() * scale;
				data.offsetScaleX = input.readFloat();
				data.offsetScaleY = input.readFloat();
				data.offsetShearY = input.readFloat();
				data.rotateMix = input.readFloat();
				data.translateMix = input.readFloat();
				data.scaleMix = input.readFloat();
				data.shearMix = input.readFloat();
				skeletonData.transformConstraints.push(data);
			}

			// Path constraints.
			n = input.readInt(true);
			for (let i = 0, nn; i < n; i++) {
				let data = new PathConstraintData(input.readString());
				data.order = input.readInt(true);
				data.skinRequired = input.readBoolean();
				nn = input.readInt(true);
				for (let ii = 0; ii < nn; ii++)
					data.bones.push(skeletonData.bones[input.readInt(true)]);
				data.target = skeletonData.slots[input.readInt(true)];
				data.positionMode = SkeletonBinary.PositionModeValues[input.readInt(true)];
				data.spacingMode = SkeletonBinary.SpacingModeValues[input.readInt(true)];
				data.rotateMode = SkeletonBinary.RotateModeValues[input.readInt(true)];
				data.offsetRotation = input.readFloat();
				data.position = input.readFloat();
				if (data.positionMode == PositionMode.Fixed) data.position *= scale;
				data.spacing = input.readFloat();
				if (data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed) data.spacing *= scale;
				data.rotateMix = input.readFloat();
				data.translateMix = input.readFloat();
				skeletonData.pathConstraints.push(data);
			}

			// Default skin.
			let defaultSkin = this.readSkin(input, skeletonData, true, nonessential);
			if (defaultSkin != null) {
				skeletonData.defaultSkin = defaultSkin;
				skeletonData.skins.push(defaultSkin);
			}

			// Skins.
			{
				let i = skeletonData.skins.length;
				Utils.setArraySize(skeletonData.skins, n = i + input.readInt(true));
				for (; i < n; i++)
					skeletonData.skins[i] = this.readSkin(input, skeletonData, false, nonessential);
			}

			// Linked meshes.
			n = this.linkedMeshes.length;
			for (let i = 0; i < n; i++) {
				let linkedMesh = this.linkedMeshes[i];
				let skin = linkedMesh.skin == null ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
				if (skin == null) throw new Error("Skin not found: " + linkedMesh.skin);
				let parent = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (parent == null) throw new Error("Parent mesh not found: " + linkedMesh.parent);
				linkedMesh.mesh.deformAttachment = linkedMesh.inheritDeform ? parent as VertexAttachment : linkedMesh.mesh;
				linkedMesh.mesh.setParentMesh(parent as MeshAttachment);
				linkedMesh.mesh.updateUVs();
			}
			this.linkedMeshes.length = 0;

			// Events.
			n = input.readInt(true);
			for (let i = 0; i < n; i++) {
				let data = new EventData(input.readStringRef());
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
			for (let i = 0; i < n; i++)
				skeletonData.animations.push(this.readAnimation(input, input.readString(), skeletonData));
			return skeletonData;
		}

		private readSkin (input: BinaryInput, skeletonData: SkeletonData, defaultSkin: boolean, nonessential: boolean): Skin {
			let skin = null;
			let slotCount = 0;

			if (defaultSkin) {
				slotCount = input.readInt(true)
				if (slotCount == 0) return null;
				skin = new Skin("default");
			} else {
				skin = new Skin(input.readStringRef());
				skin.bones.length = input.readInt(true);
				for (let i = 0, n = skin.bones.length; i < n; i++)
					skin.bones[i] = skeletonData.bones[input.readInt(true)];

				for (let i = 0, n = input.readInt(true); i < n; i++)
					skin.constraints.push(skeletonData.ikConstraints[input.readInt(true)]);
				for (let i = 0, n = input.readInt(true); i < n; i++)
					skin.constraints.push(skeletonData.transformConstraints[input.readInt(true)]);
				for (let i = 0, n = input.readInt(true); i < n; i++)
					skin.constraints.push(skeletonData.pathConstraints[input.readInt(true)]);

				slotCount = input.readInt(true);
			}

			for (let i = 0; i < slotCount; i++) {
				let slotIndex = input.readInt(true);
				for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					let name = input.readStringRef();
					let attachment = this.readAttachment(input, skeletonData, skin, slotIndex, name, nonessential);
					if (attachment != null) skin.setAttachment(slotIndex, name, attachment);
				}
			}
			return skin;
		}

		private readAttachment(input: BinaryInput, skeletonData: SkeletonData, skin: Skin, slotIndex: number, attachmentName: string, nonessential: boolean): Attachment {
			let scale = this.scale;

			let name = input.readStringRef();
			if (name == null) name = attachmentName;

			let typeIndex = input.readByte();
			let type = SkeletonBinary.AttachmentTypeValues[typeIndex];
			switch (type) {
			case AttachmentType.Region: {
				let path = input.readStringRef();
				let rotation = input.readFloat();
				let x = input.readFloat();
				let y = input.readFloat();
				let scaleX = input.readFloat();
				let scaleY = input.readFloat();
				let width = input.readFloat();
				let height = input.readFloat();
				let color = input.readInt32();

				if (path == null) path = name;
				let region = this.attachmentLoader.newRegionAttachment(skin, name, path);
				if (region == null) return null;
				region.path = path;
				region.x = x * scale;
				region.y = y * scale;
				region.scaleX = scaleX;
				region.scaleY = scaleY;
				region.rotation = rotation;
				region.width = width * scale;
				region.height = height * scale;
				Color.rgba8888ToColor(region.color, color);
				region.updateOffset();
				return region;
			}
			case AttachmentType.BoundingBox: {
				let vertexCount = input.readInt(true);
				let vertices = this.readVertices(input, vertexCount);
				let color = nonessential ? input.readInt32() : 0;

				let box = this.attachmentLoader.newBoundingBoxAttachment(skin, name);
				if (box == null) return null;
				box.worldVerticesLength = vertexCount << 1;
				box.vertices = vertices.vertices;
				box.bones = vertices.bones;
				if (nonessential) Color.rgba8888ToColor(box.color, color);
				return box;
			}
			case AttachmentType.Mesh: {
				let path = input.readStringRef();
				let color = input.readInt32();
				let vertexCount = input.readInt(true);
				let uvs = this.readFloatArray(input, vertexCount << 1, 1);
				let triangles = this.readShortArray(input);
				let vertices = this.readVertices(input, vertexCount);
				let hullLength = input.readInt(true);
				let edges = null;
				let width = 0, height = 0;
				if (nonessential) {
					edges = this.readShortArray(input);
					width = input.readFloat();
					height = input.readFloat();
				}

				if (path == null) path = name;
				let mesh = this.attachmentLoader.newMeshAttachment(skin, name, path);
				if (mesh == null) return null;
				mesh.path = path;
				Color.rgba8888ToColor(mesh.color, color);
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
			case AttachmentType.LinkedMesh: {
				let path = input.readStringRef();
				let color = input.readInt32();
				let skinName = input.readStringRef();
				let parent = input.readStringRef();
				let inheritDeform = input.readBoolean();
				let width = 0, height = 0;
				if (nonessential) {
					width = input.readFloat();
					height = input.readFloat();
				}

				if (path == null) path = name;
				let mesh = this.attachmentLoader.newMeshAttachment(skin, name, path);
				if (mesh == null) return null;
				mesh.path = path;
				Color.rgba8888ToColor(mesh.color, color);
				if (nonessential) {
					mesh.width = width * scale;
					mesh.height = height * scale;
				}
				this.linkedMeshes.push(new LinkedMesh(mesh, skinName, slotIndex, parent, inheritDeform));
				return mesh;
			}
			case AttachmentType.Path: {
				let closed = input.readBoolean();
				let constantSpeed = input.readBoolean();
				let vertexCount = input.readInt(true);
				let vertices = this.readVertices(input, vertexCount);
				let lengths = Utils.newArray(vertexCount / 3, 0);
				for (let i = 0, n = lengths.length; i < n; i++)
					lengths[i] = input.readFloat() * scale;
				let color = nonessential ? input.readInt32() : 0;

				let path = this.attachmentLoader.newPathAttachment(skin, name);
				if (path == null) return null;
				path.closed = closed;
				path.constantSpeed = constantSpeed;
				path.worldVerticesLength = vertexCount << 1;
				path.vertices = vertices.vertices;
				path.bones = vertices.bones;
				path.lengths = lengths;
				if (nonessential) Color.rgba8888ToColor(path.color, color);
				return path;
			}
			case AttachmentType.Point: {
				let rotation = input.readFloat();
				let x = input.readFloat();
				let y = input.readFloat();
				let color = nonessential ? input.readInt32() : 0;

				let point = this.attachmentLoader.newPointAttachment(skin, name);
				if (point == null) return null;
				point.x = x * scale;
				point.y = y * scale;
				point.rotation = rotation;
				if (nonessential) Color.rgba8888ToColor(point.color, color);
				return point;
			}
			case AttachmentType.Clipping: {
				let endSlotIndex = input.readInt(true);
				let vertexCount = input.readInt(true);
				let vertices = this.readVertices(input, vertexCount);
				let color = nonessential ? input.readInt32() : 0;

				let clip = this.attachmentLoader.newClippingAttachment(skin, name);
				if (clip == null) return null;
				clip.endSlot = skeletonData.slots[endSlotIndex];
				clip.worldVerticesLength = vertexCount << 1;
				clip.vertices = vertices.vertices;
				clip.bones = vertices.bones;
				if (nonessential) Color.rgba8888ToColor(clip.color, color);
				return clip;
			}
			}
			return null;
		}

		private readVertices (input: BinaryInput, vertexCount: number): Vertices {
			let verticesLength = vertexCount << 1;
			let vertices = new Vertices();
			let scale = this.scale;
			if (!input.readBoolean()) {
				vertices.vertices = this.readFloatArray(input, verticesLength, scale);
				return vertices;
			}
			let weights = new Array<number>();
			let bonesArray = new Array<number>();
			for (let i = 0; i < vertexCount; i++) {
				let boneCount = input.readInt(true);
				bonesArray.push(boneCount);
				for (let ii = 0; ii < boneCount; ii++) {
					bonesArray.push(input.readInt(true));
					weights.push(input.readFloat() * scale);
					weights.push(input.readFloat() * scale);
					weights.push(input.readFloat());
				}
			}
			vertices.vertices = Utils.toFloatArray(weights);
			vertices.bones = bonesArray;
			return vertices;
		}

		private readFloatArray (input: BinaryInput, n: number, scale: number): number[] {
			let array = new Array<number>(n);
			if (scale == 1) {
				for (let i = 0; i < n; i++)
					array[i] = input.readFloat();
			} else {
				for (let i = 0; i < n; i++)
					array[i] = input.readFloat() * scale;
			}
			return array;
		}

		private readShortArray (input: BinaryInput): number[] {
			let n = input.readInt(true);
			let array = new Array<number>(n);
			for (let i = 0; i < n; i++)
				array[i] = input.readShort();
			return array;
		}

		private readAnimation (input: BinaryInput, name: string, skeletonData: SkeletonData): Animation {
			let timelines = new Array<Timeline>();
			let scale = this.scale;
			let duration = 0;
			let tempColor1 = new Color();
			let tempColor2 = new Color();

			// Slot timelines.
			for (let i = 0, n = input.readInt(true); i < n; i++) {
				let slotIndex = input.readInt(true);
				for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					let timelineType = input.readByte();
					let frameCount = input.readInt(true);
					switch (timelineType) {
					case SkeletonBinary.SLOT_ATTACHMENT: {
						let timeline = new AttachmentTimeline(frameCount);
						timeline.slotIndex = slotIndex;
						for (let frameIndex = 0; frameIndex < frameCount; frameIndex++)
							timeline.setFrame(frameIndex, input.readFloat(), input.readStringRef());
						timelines.push(timeline);
						duration = Math.max(duration, timeline.frames[frameCount - 1]);
						break;
					}
					case SkeletonBinary.SLOT_COLOR: {
						let timeline = new ColorTimeline(frameCount);
						timeline.slotIndex = slotIndex;
						for (let frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							let time = input.readFloat();
							Color.rgba8888ToColor(tempColor1, input.readInt32());
							timeline.setFrame(frameIndex, time, tempColor1.r, tempColor1.g, tempColor1.b, tempColor1.a);
							if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, timeline);
						}
						timelines.push(timeline);
						duration = Math.max(duration, timeline.frames[(frameCount - 1) * ColorTimeline.ENTRIES]);
						break;
					}
					case SkeletonBinary.SLOT_TWO_COLOR: {
						let timeline = new TwoColorTimeline(frameCount);
						timeline.slotIndex = slotIndex;
						for (let frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							let time = input.readFloat();
							Color.rgba8888ToColor(tempColor1, input.readInt32());
							Color.rgb888ToColor(tempColor2, input.readInt32());
							timeline.setFrame(frameIndex, time, tempColor1.r, tempColor1.g, tempColor1.b, tempColor1.a, tempColor2.r,
								tempColor2.g, tempColor2.b);
							if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, timeline);
						}
						timelines.push(timeline);
						duration = Math.max(duration, timeline.frames[(frameCount - 1) * TwoColorTimeline.ENTRIES]);
						break;
					}
					}
				}
			}

			// Bone timelines.
			for (let i = 0, n = input.readInt(true); i < n; i++) {
				let boneIndex = input.readInt(true);
				for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					let timelineType = input.readByte();
					let frameCount = input.readInt(true);
					switch (timelineType) {
					case SkeletonBinary.BONE_ROTATE: {
						let timeline = new RotateTimeline(frameCount);
						timeline.boneIndex = boneIndex;
						for (let frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							timeline.setFrame(frameIndex, input.readFloat(), input.readFloat());
							if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, timeline);
						}
						timelines.push(timeline);
						duration = Math.max(duration, timeline.frames[(frameCount - 1) * RotateTimeline.ENTRIES]);
						break;
					}
					case SkeletonBinary.BONE_TRANSLATE:
					case SkeletonBinary.BONE_SCALE:
					case SkeletonBinary.BONE_SHEAR: {
						let timeline;
						let timelineScale = 1;
						if (timelineType == SkeletonBinary.BONE_SCALE)
							timeline = new ScaleTimeline(frameCount);
						else if (timelineType == SkeletonBinary.BONE_SHEAR)
							timeline = new ShearTimeline(frameCount);
						else {
							timeline = new TranslateTimeline(frameCount);
							timelineScale = scale;
						}
						timeline.boneIndex = boneIndex;
						for (let frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							timeline.setFrame(frameIndex, input.readFloat(), input.readFloat() * timelineScale,
								input.readFloat() * timelineScale);
							if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, timeline);
						}
						timelines.push(timeline);
						duration = Math.max(duration, timeline.frames[(frameCount - 1) * TranslateTimeline.ENTRIES]);
						break;
					}
					}
				}
			}

			// IK constraint timelines.
			for (let i = 0, n = input.readInt(true); i < n; i++) {
				let index = input.readInt(true);
				let frameCount = input.readInt(true);
				let timeline = new IkConstraintTimeline(frameCount);
				timeline.ikConstraintIndex = index;
				for (let frameIndex = 0; frameIndex < frameCount; frameIndex++) {
					timeline.setFrame(frameIndex, input.readFloat(), input.readFloat(), input.readFloat() * scale, input.readByte(), input.readBoolean(),
						input.readBoolean());
					if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, timeline);
				}
				timelines.push(timeline);
				duration = Math.max(duration, timeline.frames[(frameCount - 1) * IkConstraintTimeline.ENTRIES]);
			}

			// Transform constraint timelines.
			for (let i = 0, n = input.readInt(true); i < n; i++) {
				let index = input.readInt(true);
				let frameCount = input.readInt(true);
				let timeline = new TransformConstraintTimeline(frameCount);
				timeline.transformConstraintIndex = index;
				for (let frameIndex = 0; frameIndex < frameCount; frameIndex++) {
					timeline.setFrame(frameIndex, input.readFloat(), input.readFloat(), input.readFloat(), input.readFloat(),
						input.readFloat());
					if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, timeline);
				}
				timelines.push(timeline);
				duration = Math.max(duration, timeline.frames[(frameCount - 1) * TransformConstraintTimeline.ENTRIES]);
			}

			// Path constraint timelines.
			for (let i = 0, n = input.readInt(true); i < n; i++) {
				let index = input.readInt(true);
				let data = skeletonData.pathConstraints[index];
				for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					let timelineType = input.readByte();
					let frameCount = input.readInt(true);
					switch (timelineType) {
					case SkeletonBinary.PATH_POSITION:
					case SkeletonBinary.PATH_SPACING: {
						let timeline;
						let timelineScale = 1;
						if (timelineType == SkeletonBinary.PATH_SPACING) {
							timeline = new PathConstraintSpacingTimeline(frameCount);
							if (data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed) timelineScale = scale;
						} else {
							timeline = new PathConstraintPositionTimeline(frameCount);
							if (data.positionMode == PositionMode.Fixed) timelineScale = scale;
						}
						timeline.pathConstraintIndex = index;
						for (let frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							timeline.setFrame(frameIndex, input.readFloat(), input.readFloat() * timelineScale);
							if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, timeline);
						}
						timelines.push(timeline);
						duration = Math.max(duration, timeline.frames[(frameCount - 1) * PathConstraintPositionTimeline.ENTRIES]);
						break;
					}
					case SkeletonBinary.PATH_MIX: {
						let timeline = new PathConstraintMixTimeline(frameCount);
						timeline.pathConstraintIndex = index;
						for (let frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							timeline.setFrame(frameIndex, input.readFloat(), input.readFloat(), input.readFloat());
							if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, timeline);
						}
						timelines.push(timeline);
						duration = Math.max(duration, timeline.frames[(frameCount - 1) * PathConstraintMixTimeline.ENTRIES]);
						break;
					}
					}
				}
			}

			// Deform timelines.
			for (let i = 0, n = input.readInt(true); i < n; i++) {
				let skin = skeletonData.skins[input.readInt(true)];
				for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
					let slotIndex = input.readInt(true);
					for (let iii = 0, nnn = input.readInt(true); iii < nnn; iii++) {
						let attachment = skin.getAttachment(slotIndex, input.readStringRef()) as VertexAttachment;
						let weighted = attachment.bones != null;
						let vertices = attachment.vertices;
						let deformLength = weighted ? vertices.length / 3 * 2 : vertices.length;

						let frameCount = input.readInt(true);
						let timeline = new DeformTimeline(frameCount);
						timeline.slotIndex = slotIndex;
						timeline.attachment = attachment;

						for (let frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							let time = input.readFloat();
							let deform;
							let end = input.readInt(true);
							if (end == 0)
								deform = weighted ? Utils.newFloatArray(deformLength) : vertices;
							else {
								deform = Utils.newFloatArray(deformLength);
								let start = input.readInt(true);
								end += start;
								if (scale == 1) {
									for (let v = start; v < end; v++)
										deform[v] = input.readFloat();
								} else {
									for (let v = start; v < end; v++)
										deform[v] = input.readFloat() * scale;
								}
								if (!weighted) {
									for (let v = 0, vn = deform.length; v < vn; v++)
										deform[v] += vertices[v];
								}
							}

							timeline.setFrame(frameIndex, time, deform);
							if (frameIndex < frameCount - 1) this.readCurve(input, frameIndex, timeline);
						}
						timelines.push(timeline);
						duration = Math.max(duration, timeline.frames[frameCount - 1]);
					}
				}
			}

			// Draw order timeline.
			let drawOrderCount = input.readInt(true);
			if (drawOrderCount > 0) {
				let timeline = new DrawOrderTimeline(drawOrderCount);
				let slotCount = skeletonData.slots.length;
				for (let i = 0; i < drawOrderCount; i++) {
					let time = input.readFloat();
					let offsetCount = input.readInt(true);
					let drawOrder = Utils.newArray(slotCount, 0);
					for (let ii = slotCount - 1; ii >= 0; ii--)
						drawOrder[ii] = -1;
					let unchanged = Utils.newArray(slotCount - offsetCount, 0);
					let originalIndex = 0, unchangedIndex = 0;
					for (let ii = 0; ii < offsetCount; ii++) {
						let slotIndex = input.readInt(true);
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
					for (let ii = slotCount - 1; ii >= 0; ii--)
						if (drawOrder[ii] == -1) drawOrder[ii] = unchanged[--unchangedIndex];
					timeline.setFrame(i, time, drawOrder);
				}
				timelines.push(timeline);
				duration = Math.max(duration, timeline.frames[drawOrderCount - 1]);
			}

			// Event timeline.
			let eventCount = input.readInt(true);
			if (eventCount > 0) {
				let timeline = new EventTimeline(eventCount);
				for (let i = 0; i < eventCount; i++) {
					let time = input.readFloat();
					let eventData = skeletonData.events[input.readInt(true)];
					let event = new Event(time, eventData);
					event.intValue = input.readInt(false);
					event.floatValue = input.readFloat();
					event.stringValue = input.readBoolean() ? input.readString() : eventData.stringValue;
					if (event.data.audioPath != null) {
						event.volume = input.readFloat();
						event.balance = input.readFloat();
					}
					timeline.setFrame(i, event);
				}
				timelines.push(timeline);
				duration = Math.max(duration, timeline.frames[eventCount - 1]);
			}

			return new Animation(name, timelines, duration);
		}

		private readCurve (input: BinaryInput, frameIndex: number, timeline: CurveTimeline) {
			switch (input.readByte()) {
			case SkeletonBinary.CURVE_STEPPED:
				timeline.setStepped(frameIndex);
				break;
			case SkeletonBinary.CURVE_BEZIER:
				this.setCurve(timeline, frameIndex, input.readFloat(), input.readFloat(), input.readFloat(), input.readFloat());
				break;
			}
		}

		setCurve (timeline: CurveTimeline, frameIndex: number, cx1: number, cy1: number, cx2: number, cy2: number) {
			timeline.setCurve(frameIndex, cx1, cy1, cx2, cy2);
		}
	}

	class BinaryInput {
		constructor(data: Uint8Array, public strings = new Array<string>(), private index: number = 0, private buffer = new DataView(data.buffer)) { 

		}

		readByte(): number {
			return this.buffer.getInt8(this.index++);
		}

		readShort(): number {
			let value = this.buffer.getInt16(this.index);
			this.index += 2;
			return value;
		}

		readInt32(): number {
			 let value = this.buffer.getInt32(this.index)
			 this.index += 4;
			 return value;
		}

		readInt(optimizePositive: boolean) {
			let b = this.readByte();
			let result = b & 0x7F;
			if ((b & 0x80) != 0) {
				b = this.readByte();
				result |= (b & 0x7F) << 7;
				if ((b & 0x80) != 0) {
					b = this.readByte();
					result |= (b & 0x7F) << 14;
					if ((b & 0x80) != 0) {
						b = this.readByte();
						result |= (b & 0x7F) << 21;
						if ((b & 0x80) != 0) {
							b = this.readByte();
							result |= (b & 0x7F) << 28;
						}
					}
				}
			}
			return optimizePositive ? result : ((result >>> 1) ^ -(result & 1));
		}

		readStringRef (): string {
			let index = this.readInt(true);
			return index == 0 ? null : this.strings[index - 1];
		}

		readString (): string {
			let byteCount = this.readInt(true);
			switch (byteCount) {
			case 0:
				return null;
			case 1:
				return "";
			}
			byteCount--;
			let chars = "";
			let charCount = 0;
			for (let i = 0; i < byteCount;) {
				let b = this.readByte();
				switch (b >> 4) {
				case 12:
				case 13:
					chars += String.fromCharCode(((b & 0x1F) << 6 | this.readByte() & 0x3F));
					i += 2;
					break;
				case 14:
					chars += String.fromCharCode(((b & 0x0F) << 12 | (this.readByte() & 0x3F) << 6 | this.readByte() & 0x3F));
					i += 3;
					break;
				default:
					chars += String.fromCharCode(b);
					i++;
				}
			}
			return chars;
		}

		readFloat (): number {
			let value = this.buffer.getFloat32(this.index);
			this.index += 4;
			return value;
		}

		readBoolean (): boolean {
			return this.readByte() != 0;
		}
	}

	class LinkedMesh {
		parent: string; skin: string;
		slotIndex: number;
		mesh: MeshAttachment;
		inheritDeform: boolean;

		constructor (mesh: MeshAttachment, skin: string, slotIndex: number, parent: string, inheritDeform: boolean) {
			this.mesh = mesh;
			this.skin = skin;
			this.slotIndex = slotIndex;
			this.parent = parent;
			this.inheritDeform = inheritDeform;
		}
	}

	class Vertices {
		constructor(public bones: Array<number> = null, public vertices: Array<number> | Float32Array = null) { }
	}
}
