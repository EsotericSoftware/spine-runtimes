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

import { AlphaTimeline, Animation, AttachmentTimeline, type BoneTimeline2, type CurveTimeline, CurveTimeline1, DeformTimeline, DrawOrderTimeline, EventTimeline, IkConstraintTimeline, InheritTimeline, PathConstraintMixTimeline, PathConstraintPositionTimeline, PathConstraintSpacingTimeline, PhysicsConstraintDampingTimeline, PhysicsConstraintGravityTimeline, PhysicsConstraintInertiaTimeline, PhysicsConstraintMassTimeline, PhysicsConstraintMixTimeline, PhysicsConstraintResetTimeline, PhysicsConstraintStrengthTimeline, PhysicsConstraintWindTimeline, RGB2Timeline, RGBA2Timeline, RGBATimeline, RGBTimeline, RotateTimeline, ScaleTimeline, ScaleXTimeline, ScaleYTimeline, SequenceTimeline, ShearTimeline, ShearXTimeline, ShearYTimeline, SliderMixTimeline, SliderTimeline, type Timeline, TransformConstraintTimeline, TranslateTimeline, TranslateXTimeline, TranslateYTimeline } from "./Animation.js";
import type { Attachment, VertexAttachment } from "./attachments/Attachment.js";
import type { AttachmentLoader } from "./attachments/AttachmentLoader.js";
import type { HasTextureRegion } from "./attachments/HasTextureRegion.js";
import type { MeshAttachment } from "./attachments/MeshAttachment.js";
import { Sequence, SequenceModeValues } from "./attachments/Sequence.js";
import { BoneData } from "./BoneData.js";
import { Event } from "./Event.js";
import { EventData } from "./EventData.js";
import { IkConstraintData } from "./IkConstraintData.js";
import { PathConstraintData, PositionMode, SpacingMode } from "./PathConstraintData.js";
import { PhysicsConstraintData } from "./PhysicsConstraintData.js";
import { SkeletonData } from "./SkeletonData.js";
import { Skin } from "./Skin.js";
import { SliderData } from "./SliderData.js";
import { SlotData } from "./SlotData.js";
import { type FromProperty, FromRotate, FromScaleX, FromScaleY, FromShearY, FromX, FromY, type ToProperty, ToRotate, ToScaleX, ToScaleY, ToShearY, ToX, ToY, TransformConstraintData } from "./TransformConstraintData.js";
import { Color, type NumberArrayLike, Utils } from "./Utils.js";

/** Loads skeleton data in the Spine binary format.
 *
 * See [Spine binary format](http://esotericsoftware.com/spine-binary-format) and
 * [JSON and binary data](http://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data) in the Spine
 * Runtimes Guide. */
export class SkeletonBinary {
	/** Scales bone positions, image sizes, and translations as they are loaded. This allows different size images to be used at
	 * runtime than were used in Spine.
	 *
	 * See [Scaling](http://esotericsoftware.com/spine-loading-skeleton-data#Scaling) in the Spine Runtimes Guide. */
	scale = 1;

	attachmentLoader: AttachmentLoader;
	private linkedMeshes = [] as LinkedMesh[];

	constructor (attachmentLoader: AttachmentLoader) {
		this.attachmentLoader = attachmentLoader;
	}

	readSkeletonData (binary: Uint8Array | ArrayBuffer): SkeletonData {
		const scale = this.scale;

		const skeletonData = new SkeletonData();
		skeletonData.name = ""; // BOZO

		const input = new BinaryInput(binary);

		const lowHash = input.readInt32();
		const highHash = input.readInt32();
		skeletonData.hash = highHash === 0 && lowHash === 0 ? null : highHash.toString(16) + lowHash.toString(16);
		skeletonData.version = input.readString();
		skeletonData.x = input.readFloat();
		skeletonData.y = input.readFloat();
		skeletonData.width = input.readFloat();
		skeletonData.height = input.readFloat();
		skeletonData.referenceScale = input.readFloat() * scale;

		const nonessential = input.readBoolean();
		if (nonessential) {
			skeletonData.fps = input.readFloat();
			skeletonData.imagesPath = input.readString();
			skeletonData.audioPath = input.readString();
		}

		let n = 0;
		// Strings.
		n = input.readInt(true)
		for (let i = 0; i < n; i++) {
			const str = input.readString();
			if (!str) throw new Error("String in string table must not be null.");
			input.strings.push(str);
		}

		// Bones.
		const bones = skeletonData.bones;
		n = input.readInt(true)
		for (let i = 0; i < n; i++) {
			const name = input.readString();
			if (!name) throw new Error("Bone name must not be null.");
			const parent = i === 0 ? null : bones[input.readInt(true)];
			const data = new BoneData(i, name, parent);
			const setup = data.setup;
			setup.rotation = input.readFloat();
			setup.x = input.readFloat() * scale;
			setup.y = input.readFloat() * scale;
			setup.scaleX = input.readFloat();
			setup.scaleY = input.readFloat();
			setup.shearX = input.readFloat();
			setup.shearY = input.readFloat();
			setup.inherit = input.readByte();
			data.length = input.readFloat() * scale;
			data.skinRequired = input.readBoolean();
			if (nonessential) {
				Color.rgba8888ToColor(data.color, input.readInt32());
				data.icon = input.readString() ?? undefined;
				data.visible = input.readBoolean();
			}
			bones.push(data);
		}

		// Slots.
		n = input.readInt(true);
		for (let i = 0; i < n; i++) {
			const slotName = input.readString();
			if (!slotName) throw new Error("Slot name must not be null.");
			const boneData = bones[input.readInt(true)];
			const data = new SlotData(i, slotName, boneData);
			Color.rgba8888ToColor(data.setup.color, input.readInt32());

			const darkColor = input.readInt32();
			if (darkColor !== -1) Color.rgb888ToColor(data.setup.darkColor = new Color(), darkColor);

			data.attachmentName = input.readStringRef();
			data.blendMode = input.readInt(true);
			if (nonessential) data.visible = input.readBoolean();
			skeletonData.slots.push(data);
		}

		// Constraints.
		const constraints = skeletonData.constraints;
		const constraintCount = input.readInt(true);
		for (let i = 0; i < constraintCount; i++) {
			const name = input.readString();
			if (!name) throw new Error("Constraint data name must not be null.");
			let nn: number;
			switch (input.readByte()) {
				case CONSTRAINT_IK: {
					const data = new IkConstraintData(name);
					nn = input.readInt(true);
					for (let ii = 0; ii < nn; ii++)
						data.bones.push(bones[input.readInt(true)]);
					data.target = bones[input.readInt(true)];
					const flags = input.readByte();
					data.skinRequired = (flags & 1) !== 0;
					data.uniform = (flags & 2) !== 0;
					const setup = data.setup;
					setup.bendDirection = (flags & 4) !== 0 ? -1 : 1;
					setup.compress = (flags & 8) !== 0;
					setup.stretch = (flags & 16) !== 0;
					if ((flags & 32) !== 0) setup.mix = (flags & 64) !== 0 ? input.readFloat() : 1;
					if ((flags & 128) !== 0) setup.softness = input.readFloat() * scale;
					constraints.push(data);
					break;
				}
				case CONSTRAINT_TRANSFORM: {
					const data = new TransformConstraintData(name);
					nn = input.readInt(true);
					for (let ii = 0; ii < nn; ii++)
						data.bones.push(bones[input.readInt(true)]);
					data.source = bones[input.readInt(true)];
					let flags = input.readUnsignedByte();
					data.skinRequired = (flags & 1) !== 0;
					data.localSource = (flags & 2) !== 0;
					data.localTarget = (flags & 4) !== 0;
					data.additive = (flags & 8) !== 0;
					data.clamp = (flags & 16) !== 0;

					nn = flags >> 5;
					for (let ii = 0, tn: number; ii < nn; ii++) {
						let fromScale = 1;
						let from: FromProperty | null;
						switch (input.readByte()) {
							case 0: from = new FromRotate(); break;
							case 1: {
								fromScale = scale;
								from = new FromX();
								break;
							}
							case 2: {
								fromScale = scale;
								from = new FromY();
								break;
							}
							case 3: from = new FromScaleX(); break;
							case 4: from = new FromScaleY(); break;
							case 5: from = new FromShearY(); break;
							default: from = null;
						}
						if (!from) continue;
						from.offset = input.readFloat() * fromScale;
						tn = input.readByte();
						for (let t = 0; t < tn; t++) {
							let toScale = 1;
							let to: ToProperty | null;
							switch (input.readByte()) {
								case 0: to = new ToRotate(); break;
								case 1: {
									toScale = scale;
									to = new ToX();
									break;
								}
								case 2: {
									toScale = scale;
									to = new ToY();
									break;
								}
								case 3: to = new ToScaleX(); break;
								case 4: to = new ToScaleY(); break;
								case 5: to = new ToShearY(); break;
								default: to = null;
							}
							if (!to) continue;
							to.offset = input.readFloat() * toScale;
							to.max = input.readFloat() * toScale;
							to.scale = input.readFloat() * toScale / fromScale;
							from.to[t] = to;
						}
						data.properties[ii] = from;
					}
					flags = input.readByte();
					if ((flags & 1) !== 0) data.offsets[TransformConstraintData.ROTATION] = input.readFloat();
					if ((flags & 2) !== 0) data.offsets[TransformConstraintData.X] = input.readFloat() * scale;
					if ((flags & 4) !== 0) data.offsets[TransformConstraintData.Y] = input.readFloat() * scale;
					if ((flags & 8) !== 0) data.offsets[TransformConstraintData.SCALEX] = input.readFloat();
					if ((flags & 16) !== 0) data.offsets[TransformConstraintData.SCALEY] = input.readFloat();
					if ((flags & 32) !== 0) data.offsets[TransformConstraintData.SHEARY] = input.readFloat();
					flags = input.readByte();
					const setup = data.setup;
					if ((flags & 1) !== 0) setup.mixRotate = input.readFloat();
					if ((flags & 2) !== 0) setup.mixX = input.readFloat();
					if ((flags & 4) !== 0) setup.mixY = input.readFloat();
					if ((flags & 8) !== 0) setup.mixScaleX = input.readFloat();
					if ((flags & 16) !== 0) setup.mixScaleY = input.readFloat();
					if ((flags & 32) !== 0) setup.mixShearY = input.readFloat();
					constraints.push(data);
					break;
				}
				case CONSTRAINT_PATH: {
					const data = new PathConstraintData(name);
					nn = input.readInt(true);
					for (let ii = 0; ii < nn; ii++)
						data.bones.push(bones[input.readInt(true)]);
					data.slot = skeletonData.slots[input.readInt(true)];
					const flags = input.readByte();
					data.skinRequired = (flags & 1) !== 0;
					data.positionMode = (flags >> 1) & 2;
					data.spacingMode = (flags >> 2) & 3;
					data.rotateMode = (flags >> 4) & 3;
					if ((flags & 128) !== 0) data.offsetRotation = input.readFloat();
					const setup = data.setup;
					setup.position = input.readFloat();
					if (data.positionMode === PositionMode.Fixed) setup.position *= scale;
					setup.spacing = input.readFloat();
					if (data.spacingMode === SpacingMode.Length || data.spacingMode === SpacingMode.Fixed) setup.spacing *= scale;
					setup.mixRotate = input.readFloat();
					setup.mixX = input.readFloat();
					setup.mixY = input.readFloat();
					constraints.push(data);
					break;
				}
				case CONSTRAINT_PHYSICS: {
					const data = new PhysicsConstraintData(name);
					data.bone = bones[input.readInt(true)];
					let flags = input.readByte();
					data.skinRequired = (flags & 1) !== 0;
					if ((flags & 2) !== 0) data.x = input.readFloat();
					if ((flags & 4) !== 0) data.y = input.readFloat();
					if ((flags & 8) !== 0) data.rotate = input.readFloat();
					if ((flags & 16) !== 0) data.scaleX = input.readFloat();
					if ((flags & 32) !== 0) data.shearX = input.readFloat();
					data.limit = ((flags & 64) !== 0 ? input.readFloat() : 5000) * scale;
					data.step = 1 / input.readUnsignedByte();
					const setup = data.setup;
					setup.inertia = input.readFloat();
					setup.strength = input.readFloat();
					setup.damping = input.readFloat();
					setup.massInverse = (flags & 128) !== 0 ? input.readFloat() : 1;
					setup.wind = input.readFloat();
					setup.gravity = input.readFloat();
					flags = input.readByte();
					if ((flags & 1) !== 0) data.inertiaGlobal = true;
					if ((flags & 2) !== 0) data.strengthGlobal = true;
					if ((flags & 4) !== 0) data.dampingGlobal = true;
					if ((flags & 8) !== 0) data.massGlobal = true;
					if ((flags & 16) !== 0) data.windGlobal = true;
					if ((flags & 32) !== 0) data.gravityGlobal = true;
					if ((flags & 64) !== 0) data.mixGlobal = true;
					setup.mix = (flags & 128) !== 0 ? input.readFloat() : 1;
					constraints.push(data);
					break;
				}
				case CONSTRAINT_SLIDER: {
					const data = new SliderData(name);
					const flags = input.readByte();
					data.skinRequired = (flags & 1) !== 0;
					data.loop = (flags & 2) !== 0;
					data.additive = (flags & 4) !== 0;
					if ((flags & 8) !== 0) data.setup.time = input.readFloat();
					if ((flags & 16) !== 0) data.setup.mix = (flags & 32) !== 0 ? input.readFloat() : 1;
					if ((flags & 64) !== 0) {
						data.local = (flags & 128) !== 0;
						data.bone = bones[input.readInt(true)];
						const offset = input.readFloat();
						let propertyScale = 1;
						switch (input.readByte()) {
							case 0: data.property = new FromRotate(); break;
							case 1: {
								propertyScale = scale;
								data.property = new FromX();
								break;
							}
							case 2: {
								propertyScale = scale;
								data.property = new FromY();
								break;
							}
							case 3: data.property = new FromScaleX(); break;
							case 4: data.property = new FromScaleY(); break;
							case 5: data.property = new FromShearY(); break;
							default: continue;
						};
						data.property.offset = offset * propertyScale;
						data.offset = input.readFloat();
						data.scale = input.readFloat() / propertyScale;
					}
					constraints.push(data);
					break;
				}
			}
		}

		// Default skin.
		const defaultSkin = this.readSkin(input, skeletonData, true, nonessential);
		if (defaultSkin) {
			skeletonData.defaultSkin = defaultSkin;
			skeletonData.skins.push(defaultSkin);
		}

		// Skins.
		{
			let i = skeletonData.skins.length;
			Utils.setArraySize(skeletonData.skins, n = i + input.readInt(true));
			for (; i < n; i++) {
				const skin = this.readSkin(input, skeletonData, false, nonessential);
				if (!skin) throw new Error("readSkin() should not have returned null.");
				skeletonData.skins[i] = skin;
			}
		}

		// Linked meshes.
		n = this.linkedMeshes.length;
		for (let i = 0; i < n; i++) {
			const linkedMesh = this.linkedMeshes[i];
			const skin = skeletonData.skins[linkedMesh.skinIndex];
			if (!linkedMesh.parent) throw new Error("Linked mesh parent must not be null");
			const parent = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
			if (!parent) throw new Error(`Parent mesh not found: ${linkedMesh.parent}`);
			linkedMesh.mesh.timelineAttachment = linkedMesh.inheritTimeline ? parent as VertexAttachment : linkedMesh.mesh;
			linkedMesh.mesh.setParentMesh(parent as MeshAttachment);
			if (linkedMesh.mesh.region != null) linkedMesh.mesh.updateRegion();
		}
		this.linkedMeshes.length = 0;

		// Events.
		n = input.readInt(true);
		for (let i = 0; i < n; i++) {
			const eventName = input.readString();
			if (!eventName) throw new Error("Event data name must not be null");
			const data = new EventData(eventName);
			data.intValue = input.readInt(false);
			data.floatValue = input.readFloat();
			data.stringValue = input.readString();
			data.audioPath = input.readString();
			if (data.audioPath) {
				data.volume = input.readFloat();
				data.balance = input.readFloat();
			}
			skeletonData.events.push(data);
		}

		// Animations.
		const animations = skeletonData.animations;
		n = input.readInt(true);
		for (let i = 0; i < n; i++) {
			const animationName = input.readString();
			if (!animationName) throw new Error("Animation name must not be null.");
			animations.push(this.readAnimation(input, animationName, skeletonData));
		}

		for (let i = 0; i < constraintCount; i++) {
			const constraint = constraints[i];
			if (constraint instanceof SliderData) constraint.animation = animations[input.readInt(true)];
		}

		return skeletonData;
	}

	private readSkin (input: BinaryInput, skeletonData: SkeletonData, defaultSkin: boolean, nonessential: boolean): Skin | null {
		let skin = null;
		let slotCount = 0;

		if (defaultSkin) {
			slotCount = input.readInt(true)
			if (slotCount === 0) return null;
			skin = new Skin("default");
		} else {
			const skinName = input.readString();
			if (!skinName) throw new Error("Skin name must not be null.");
			skin = new Skin(skinName);

			if (nonessential) Color.rgba8888ToColor(skin.color, input.readInt32());

			let n = input.readInt(true);
			let from: object[] = skeletonData.bones, to: object[] = skin.bones;
			for (let i = 0; i < n; i++)
				to[i] = from[input.readInt(true)];

			n = input.readInt(true);
			from = skeletonData.constraints;
			to = skin.constraints;
			for (let i = 0; i < n; i++)
				to[i] = from[input.readInt(true)];

			slotCount = input.readInt(true);
		}

		for (let i = 0; i < slotCount; i++) {
			const slotIndex = input.readInt(true);
			for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				const name = input.readStringRef();
				if (!name)
					throw new Error("Attachment name must not be null");
				const attachment = this.readAttachment(input, skeletonData, skin, slotIndex, name, nonessential);
				if (attachment) skin.setAttachment(slotIndex, name, attachment);
			}
		}
		return skin;
	}

	private readAttachment (input: BinaryInput, skeletonData: SkeletonData, skin: Skin, slotIndex: number, attachmentName: string | null | undefined, nonessential: boolean): Attachment | null {
		const scale = this.scale;

		const flags = input.readByte();
		const name = (flags & 8) !== 0 ? input.readStringRef() : attachmentName;
		if (!name) throw new Error("Attachment name must not be null");
		switch ((flags & 0b111) as AttachmentType) { // BUG?
			case AttachmentType.Region: {
				let path = (flags & 16) !== 0 ? input.readStringRef() : null;
				const color = (flags & 32) !== 0 ? input.readInt32() : 0xffffffff;
				const sequence = (flags & 64) !== 0 ? this.readSequence(input) : null;
				const rotation = (flags & 128) !== 0 ? input.readFloat() : 0;
				const x = input.readFloat();
				const y = input.readFloat();
				const scaleX = input.readFloat();
				const scaleY = input.readFloat();
				const width = input.readFloat();
				const height = input.readFloat();

				if (!path) path = name;
				const region = this.attachmentLoader.newRegionAttachment(skin, name, path, sequence);
				if (!region) return null;
				region.path = path;
				region.x = x * scale;
				region.y = y * scale;
				region.scaleX = scaleX;
				region.scaleY = scaleY;
				region.rotation = rotation;
				region.width = width * scale;
				region.height = height * scale;
				Color.rgba8888ToColor(region.color, color);
				region.sequence = sequence;
				if (region.region != null) region.updateRegion();
				return region;
			}
			case AttachmentType.BoundingBox: {
				const vertices = this.readVertices(input, (flags & 16) !== 0);
				const color = nonessential ? input.readInt32() : 0;

				const box = this.attachmentLoader.newBoundingBoxAttachment(skin, name);
				if (!box) return null;
				box.worldVerticesLength = vertices.length;
				box.vertices = vertices.vertices;
				box.bones = vertices.bones;
				if (nonessential) Color.rgba8888ToColor(box.color, color);
				return box;
			}
			case AttachmentType.Mesh: {
				let path = (flags & 16) !== 0 ? input.readStringRef() : name;
				const color = (flags & 32) !== 0 ? input.readInt32() : 0xffffffff;
				const sequence = (flags & 64) !== 0 ? this.readSequence(input) : null;
				const hullLength = input.readInt(true);
				const vertices = this.readVertices(input, (flags & 128) !== 0);
				const uvs = this.readFloatArray(input, vertices.length, 1);
				const triangles = this.readShortArray(input, (vertices.length - hullLength - 2) * 3);
				let edges: number[] = [];
				let width = 0, height = 0;
				if (nonessential) {
					edges = this.readShortArray(input, input.readInt(true));
					width = input.readFloat();
					height = input.readFloat();
				}

				if (!path) path = name;
				const mesh = this.attachmentLoader.newMeshAttachment(skin, name, path, sequence);
				if (!mesh) return null;
				mesh.path = path;
				Color.rgba8888ToColor(mesh.color, color);
				mesh.bones = vertices.bones;
				mesh.vertices = vertices.vertices;
				mesh.worldVerticesLength = vertices.length;
				mesh.triangles = triangles;
				mesh.regionUVs = uvs;
				if (mesh.region != null) mesh.updateRegion();
				mesh.hullLength = hullLength << 1;
				mesh.sequence = sequence;
				if (nonessential) {
					mesh.edges = edges;
					mesh.width = width * scale;
					mesh.height = height * scale;
				}
				return mesh;
			}
			case AttachmentType.LinkedMesh: {
				const path = (flags & 16) !== 0 ? input.readStringRef() : name;
				if (path == null) throw new Error("Path of linked mesh must not be null");
				const color = (flags & 32) !== 0 ? input.readInt32() : 0xffffffff;
				const sequence = (flags & 64) !== 0 ? this.readSequence(input) : null;
				const inheritTimelines = (flags & 128) !== 0;
				const skinIndex = input.readInt(true);
				const parent = input.readStringRef();
				let width = 0, height = 0;
				if (nonessential) {
					width = input.readFloat();
					height = input.readFloat();
				}

				const mesh = this.attachmentLoader.newMeshAttachment(skin, name, path, sequence);
				if (!mesh) return null;
				mesh.path = path;
				Color.rgba8888ToColor(mesh.color, color);
				mesh.sequence = sequence;
				if (nonessential) {
					mesh.width = width * scale;
					mesh.height = height * scale;
				}
				this.linkedMeshes.push(new LinkedMesh(mesh, skinIndex, slotIndex, parent, inheritTimelines));
				return mesh;
			}
			case AttachmentType.Path: {
				const closed = (flags & 16) !== 0;
				const constantSpeed = (flags & 32) !== 0;
				const vertices = this.readVertices(input, (flags & 64) !== 0);

				const lengths = Utils.newArray(vertices.length / 6, 0);
				for (let i = 0, n = lengths.length; i < n; i++)
					lengths[i] = input.readFloat() * scale;
				const color = nonessential ? input.readInt32() : 0;

				const path = this.attachmentLoader.newPathAttachment(skin, name);
				if (!path) return null;
				path.closed = closed;
				path.constantSpeed = constantSpeed;
				path.worldVerticesLength = vertices.length;
				path.vertices = vertices.vertices;
				path.bones = vertices.bones;
				path.lengths = lengths;
				if (nonessential) Color.rgba8888ToColor(path.color, color);
				return path;
			}
			case AttachmentType.Point: {
				const rotation = input.readFloat();
				const x = input.readFloat();
				const y = input.readFloat();
				const color = nonessential ? input.readInt32() : 0;

				const point = this.attachmentLoader.newPointAttachment(skin, name);
				if (!point) return null;
				point.x = x * scale;
				point.y = y * scale;
				point.rotation = rotation;
				if (nonessential) Color.rgba8888ToColor(point.color, color);
				return point;
			}
			case AttachmentType.Clipping: {
				const endSlotIndex = input.readInt(true);
				const vertices = this.readVertices(input, (flags & 16) !== 0);
				const color = nonessential ? input.readInt32() : 0;

				const clip = this.attachmentLoader.newClippingAttachment(skin, name);
				if (!clip) return null;
				clip.endSlot = skeletonData.slots[endSlotIndex];
				clip.worldVerticesLength = vertices.length;
				clip.vertices = vertices.vertices;
				clip.bones = vertices.bones;
				if (nonessential) Color.rgba8888ToColor(clip.color, color);
				return clip;
			}
		}
	}

	private readSequence (input: BinaryInput) {
		const sequence = new Sequence(input.readInt(true));
		sequence.start = input.readInt(true);
		sequence.digits = input.readInt(true);
		sequence.setupIndex = input.readInt(true);
		return sequence;
	}

	private readVertices (input: BinaryInput, weighted: boolean): Vertices {
		const scale = this.scale;
		const vertexCount = input.readInt(true);
		const length = vertexCount << 1;

		if (!weighted)
			return new Vertices(null, this.readFloatArray(input, length, scale), length);

		const weights: number[] = [];
		const bonesArray: number[] = [];
		for (let i = 0; i < vertexCount; i++) {
			const boneCount = input.readInt(true);
			bonesArray.push(boneCount);
			for (let ii = 0; ii < boneCount; ii++) {
				bonesArray.push(input.readInt(true));
				weights.push(input.readFloat() * scale);
				weights.push(input.readFloat() * scale);
				weights.push(input.readFloat());
			}
		}
		return new Vertices(bonesArray, Utils.toFloatArray(weights), length);
	}

	private readFloatArray (input: BinaryInput, n: number, scale: number): number[] {
		const array: number[] = [];
		if (scale === 1) {
			for (let i = 0; i < n; i++)
				array[i] = input.readFloat();
		} else {
			for (let i = 0; i < n; i++)
				array[i] = input.readFloat() * scale;
		}
		return array;
	}

	private readShortArray (input: BinaryInput, n: number): number[] {
		const array: number[] = [];
		for (let i = 0; i < n; i++)
			array[i] = input.readInt(true);
		return array;
	}

	private readAnimation (input: BinaryInput, name: string, skeletonData: SkeletonData): Animation {
		input.readInt(true); // Number of timelines.
		const timelines: Timeline[] = [];
		const scale = this.scale;

		// Slot timelines.
		for (let i = 0, n = input.readInt(true); i < n; i++) {
			const slotIndex = input.readInt(true);
			for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				const timelineType = input.readByte();
				const frameCount = input.readInt(true);
				const frameLast = frameCount - 1;
				switch (timelineType) {
					case SLOT_ATTACHMENT: {
						const timeline = new AttachmentTimeline(frameCount, slotIndex);
						for (let frame = 0; frame < frameCount; frame++)
							timeline.setFrame(frame, input.readFloat(), input.readStringRef());
						timelines.push(timeline);
						break;
					}
					case SLOT_RGBA: {
						const bezierCount = input.readInt(true);
						const timeline = new RGBATimeline(frameCount, bezierCount, slotIndex);

						let time = input.readFloat();
						let r = input.readUnsignedByte() / 255.0;
						let g = input.readUnsignedByte() / 255.0;
						let b = input.readUnsignedByte() / 255.0;
						let a = input.readUnsignedByte() / 255.0;

						for (let frame = 0, bezier = 0; ; frame++) {
							timeline.setFrame(frame, time, r, g, b, a);
							if (frame === frameLast) break;

							const time2 = input.readFloat();
							const r2 = input.readUnsignedByte() / 255.0;
							const g2 = input.readUnsignedByte() / 255.0;
							const b2 = input.readUnsignedByte() / 255.0;
							const a2 = input.readUnsignedByte() / 255.0;

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
						timelines.push(timeline);
						break;
					}
					case SLOT_RGB: {
						const bezierCount = input.readInt(true);
						const timeline = new RGBTimeline(frameCount, bezierCount, slotIndex);

						let time = input.readFloat();
						let r = input.readUnsignedByte() / 255.0;
						let g = input.readUnsignedByte() / 255.0;
						let b = input.readUnsignedByte() / 255.0;

						for (let frame = 0, bezier = 0; ; frame++) {
							timeline.setFrame(frame, time, r, g, b);
							if (frame === frameLast) break;

							const time2 = input.readFloat();
							const r2 = input.readUnsignedByte() / 255.0;
							const g2 = input.readUnsignedByte() / 255.0;
							const b2 = input.readUnsignedByte() / 255.0;

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
						timelines.push(timeline);
						break;
					}
					case SLOT_RGBA2: {
						const bezierCount = input.readInt(true);
						const timeline = new RGBA2Timeline(frameCount, bezierCount, slotIndex);

						let time = input.readFloat();
						let r = input.readUnsignedByte() / 255.0;
						let g = input.readUnsignedByte() / 255.0;
						let b = input.readUnsignedByte() / 255.0;
						let a = input.readUnsignedByte() / 255.0;
						let r2 = input.readUnsignedByte() / 255.0;
						let g2 = input.readUnsignedByte() / 255.0;
						let b2 = input.readUnsignedByte() / 255.0;

						for (let frame = 0, bezier = 0; ; frame++) {
							timeline.setFrame(frame, time, r, g, b, a, r2, g2, b2);
							if (frame === frameLast) break;
							const time2 = input.readFloat();
							const nr = input.readUnsignedByte() / 255.0;
							const ng = input.readUnsignedByte() / 255.0;
							const nb = input.readUnsignedByte() / 255.0;
							const na = input.readUnsignedByte() / 255.0;
							const nr2 = input.readUnsignedByte() / 255.0;
							const ng2 = input.readUnsignedByte() / 255.0;
							const nb2 = input.readUnsignedByte() / 255.0;

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
						timelines.push(timeline);
						break;
					}
					case SLOT_RGB2: {
						const bezierCount = input.readInt(true);
						const timeline = new RGB2Timeline(frameCount, bezierCount, slotIndex);

						let time = input.readFloat();
						let r = input.readUnsignedByte() / 255.0;
						let g = input.readUnsignedByte() / 255.0;
						let b = input.readUnsignedByte() / 255.0;
						let r2 = input.readUnsignedByte() / 255.0;
						let g2 = input.readUnsignedByte() / 255.0;
						let b2 = input.readUnsignedByte() / 255.0;

						for (let frame = 0, bezier = 0; ; frame++) {
							timeline.setFrame(frame, time, r, g, b, r2, g2, b2);
							if (frame === frameLast) break;
							const time2 = input.readFloat();
							const nr = input.readUnsignedByte() / 255.0;
							const ng = input.readUnsignedByte() / 255.0;
							const nb = input.readUnsignedByte() / 255.0;
							const nr2 = input.readUnsignedByte() / 255.0;
							const ng2 = input.readUnsignedByte() / 255.0;
							const nb2 = input.readUnsignedByte() / 255.0;

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
						timelines.push(timeline);
						break;
					}
					case SLOT_ALPHA: {
						const timeline = new AlphaTimeline(frameCount, input.readInt(true), slotIndex);
						let time = input.readFloat(), a = input.readUnsignedByte() / 255;
						for (let frame = 0, bezier = 0; ; frame++) {
							timeline.setFrame(frame, time, a);
							if (frame === frameLast) break;
							const time2 = input.readFloat();
							const a2 = input.readUnsignedByte() / 255;
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
						timelines.push(timeline);
					}
				}
			}
		}

		// Bone timelines.
		for (let i = 0, n = input.readInt(true); i < n; i++) {
			const boneIndex = input.readInt(true);
			for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				const type = input.readByte(), frameCount = input.readInt(true);
				if (type === BONE_INHERIT) {
					const timeline = new InheritTimeline(frameCount, boneIndex);
					for (let frame = 0; frame < frameCount; frame++) {
						timeline.setFrame(frame, input.readFloat(), input.readByte());
					}
					timelines.push(timeline);
					continue;
				}
				const bezierCount = input.readInt(true);
				switch (type) {
					case BONE_ROTATE: readTimeline(input, timelines, new RotateTimeline(frameCount, bezierCount, boneIndex), 1); break;
					case BONE_TRANSLATE: readTimeline(input, timelines, new TranslateTimeline(frameCount, bezierCount, boneIndex), scale); break;
					case BONE_TRANSLATEX: readTimeline(input, timelines, new TranslateXTimeline(frameCount, bezierCount, boneIndex), scale); break;
					case BONE_TRANSLATEY: readTimeline(input, timelines, new TranslateYTimeline(frameCount, bezierCount, boneIndex), scale); break;
					case BONE_SCALE: readTimeline(input, timelines, new ScaleTimeline(frameCount, bezierCount, boneIndex), 1); break;
					case BONE_SCALEX: readTimeline(input, timelines, new ScaleXTimeline(frameCount, bezierCount, boneIndex), 1); break;
					case BONE_SCALEY: readTimeline(input, timelines, new ScaleYTimeline(frameCount, bezierCount, boneIndex), 1); break;
					case BONE_SHEAR: readTimeline(input, timelines, new ShearTimeline(frameCount, bezierCount, boneIndex), 1); break;
					case BONE_SHEARX: readTimeline(input, timelines, new ShearXTimeline(frameCount, bezierCount, boneIndex), 1); break;
					case BONE_SHEARY: readTimeline(input, timelines, new ShearYTimeline(frameCount, bezierCount, boneIndex), 1); break;
				}
			}
		}

		// IK constraint timelines.
		for (let i = 0, n = input.readInt(true); i < n; i++) {
			const index = input.readInt(true), frameCount = input.readInt(true), frameLast = frameCount - 1;
			const timeline = new IkConstraintTimeline(frameCount, input.readInt(true), index);
			let flags = input.readByte();
			let time = input.readFloat(), mix = (flags & 1) !== 0 ? ((flags & 2) !== 0 ? input.readFloat() : 1) : 0;
			let softness = (flags & 4) !== 0 ? input.readFloat() * scale : 0;
			for (let frame = 0, bezier = 0; ; frame++) {
				timeline.setFrame(frame, time, mix, softness, (flags & 8) !== 0 ? 1 : -1, (flags & 16) !== 0, (flags & 32) !== 0);
				if (frame === frameLast) break;
				flags = input.readByte();
				const time2 = input.readFloat(), mix2 = (flags & 1) !== 0 ? ((flags & 2) !== 0 ? input.readFloat() : 1) : 0;
				const softness2 = (flags & 4) !== 0 ? input.readFloat() * scale : 0;
				if ((flags & 64) !== 0) {
					timeline.setStepped(frame);
				} else if ((flags & 128) !== 0) {
					setBezier(input, timeline, bezier++, frame, 0, time, time2, mix, mix2, 1);
					setBezier(input, timeline, bezier++, frame, 1, time, time2, softness, softness2, scale);
				}
				time = time2;
				mix = mix2;
				softness = softness2;
			}
			timelines.push(timeline);
		}

		// Transform constraint timelines.
		for (let i = 0, n = input.readInt(true); i < n; i++) {
			const index = input.readInt(true), frameCount = input.readInt(true), frameLast = frameCount - 1;
			const timeline = new TransformConstraintTimeline(frameCount, input.readInt(true), index);
			let time = input.readFloat(), mixRotate = input.readFloat(), mixX = input.readFloat(), mixY = input.readFloat(),
				mixScaleX = input.readFloat(), mixScaleY = input.readFloat(), mixShearY = input.readFloat();
			for (let frame = 0, bezier = 0; ; frame++) {
				timeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
				if (frame === frameLast) break;
				const time2 = input.readFloat(), mixRotate2 = input.readFloat(), mixX2 = input.readFloat(), mixY2 = input.readFloat(),
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
			timelines.push(timeline);
		}

		// Path constraint timelines.
		for (let i = 0, n = input.readInt(true); i < n; i++) {
			const index = input.readInt(true);
			const data = skeletonData.constraints[index] as PathConstraintData;
			for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				const type = input.readByte(), frameCount = input.readInt(true), bezierCount = input.readInt(true);
				switch (type) {
					case PATH_POSITION:
						readTimeline(input, timelines, new PathConstraintPositionTimeline(frameCount, bezierCount, index),
							data.positionMode === PositionMode.Fixed ? scale : 1);
						break;
					case PATH_SPACING:
						readTimeline(input, timelines, new PathConstraintSpacingTimeline(frameCount, bezierCount, index),
							data.spacingMode === SpacingMode.Length || data.spacingMode === SpacingMode.Fixed ? scale : 1);
						break;
					case PATH_MIX: {
						const timeline = new PathConstraintMixTimeline(frameCount, bezierCount, index);
						let time = input.readFloat(), mixRotate = input.readFloat(), mixX = input.readFloat(), mixY = input.readFloat();
						for (let frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1; ; frame++) {
							timeline.setFrame(frame, time, mixRotate, mixX, mixY);
							if (frame === frameLast) break;
							const time2 = input.readFloat(), mixRotate2 = input.readFloat(), mixX2 = input.readFloat(),
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
						timelines.push(timeline);
					}
				}
			}
		}

		// Physics timelines.
		for (let i = 0, n = input.readInt(true); i < n; i++) {
			const index = input.readInt(true) - 1;
			for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				const type = input.readByte(), frameCount = input.readInt(true);
				if (type === PHYSICS_RESET) {
					const timeline = new PhysicsConstraintResetTimeline(frameCount, index);
					for (let frame = 0; frame < frameCount; frame++)
						timeline.setFrame(frame, input.readFloat());
					timelines.push(timeline);
					continue;
				}
				const bezierCount = input.readInt(true);
				switch (type) {
					case PHYSICS_INERTIA: readTimeline(input, timelines, new PhysicsConstraintInertiaTimeline(frameCount, bezierCount, index), 1); break;
					case PHYSICS_STRENGTH: readTimeline(input, timelines, new PhysicsConstraintStrengthTimeline(frameCount, bezierCount, index), 1); break;
					case PHYSICS_DAMPING: readTimeline(input, timelines, new PhysicsConstraintDampingTimeline(frameCount, bezierCount, index), 1); break;
					case PHYSICS_MASS: readTimeline(input, timelines, new PhysicsConstraintMassTimeline(frameCount, bezierCount, index), 1); break;
					case PHYSICS_WIND: readTimeline(input, timelines, new PhysicsConstraintWindTimeline(frameCount, bezierCount, index), 1); break;
					case PHYSICS_GRAVITY: readTimeline(input, timelines, new PhysicsConstraintGravityTimeline(frameCount, bezierCount, index), 1); break;
					case PHYSICS_MIX: readTimeline(input, timelines, new PhysicsConstraintMixTimeline(frameCount, bezierCount, index), 1); break;
					default: throw new Error("Unknown physics timeline type.");
				}
			}
		}

		// Slider timelines.
		for (let i = 0, n = input.readInt(true); i < n; i++) {
			const index = input.readInt(true);
			for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				const type = input.readByte(), frameCount = input.readInt(true), bezierCount = input.readInt(true);
				switch (type) {
					case SLIDER_TIME: readTimeline(input, timelines, new SliderTimeline(frameCount, bezierCount, index), 1); break;
					case SLIDER_MIX: readTimeline(input, timelines, new SliderMixTimeline(frameCount, bezierCount, index), 1); break;
					default: throw new Error(`Uknown slider type: ${type}`);
				}

			}
		}

		// Attachment timelines.
		for (let i = 0, n = input.readInt(true); i < n; i++) {
			const skin = skeletonData.skins[input.readInt(true)];
			for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				const slotIndex = input.readInt(true);
				for (let iii = 0, nnn = input.readInt(true); iii < nnn; iii++) {
					const attachmentName = input.readStringRef();
					if (!attachmentName) throw new Error("attachmentName must not be null.");
					const attachment = skin.getAttachment(slotIndex, attachmentName);
					const timelineType = input.readByte();
					const frameCount = input.readInt(true);
					const frameLast = frameCount - 1;

					switch (timelineType) {
						case ATTACHMENT_DEFORM: {
							const vertexAttachment = attachment as VertexAttachment;
							const weighted = vertexAttachment.bones;
							const vertices = vertexAttachment.vertices;
							const deformLength = weighted ? vertices.length / 3 * 2 : vertices.length;


							const bezierCount = input.readInt(true);
							const timeline = new DeformTimeline(frameCount, bezierCount, slotIndex, vertexAttachment);

							let time = input.readFloat();
							for (let frame = 0, bezier = 0; ; frame++) {
								let deform: NumberArrayLike;
								let end = input.readInt(true);
								if (end === 0)
									deform = weighted ? Utils.newFloatArray(deformLength) : vertices;
								else {
									deform = Utils.newFloatArray(deformLength);
									const start = input.readInt(true);
									end += start;
									if (scale === 1) {
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

								timeline.setFrame(frame, time, deform);
								if (frame === frameLast) break;
								const time2 = input.readFloat();
								switch (input.readByte()) {
									case CURVE_STEPPED:
										timeline.setStepped(frame);
										break;
									case CURVE_BEZIER:
										setBezier(input, timeline, bezier++, frame, 0, time, time2, 0, 1, 1);
								}
								time = time2;
							}
							timelines.push(timeline);
							break;
						}
						case ATTACHMENT_SEQUENCE: {
							const timeline = new SequenceTimeline(frameCount, slotIndex, attachment as unknown as HasTextureRegion);
							for (let frame = 0; frame < frameCount; frame++) {
								const time = input.readFloat();
								const modeAndIndex = input.readInt32();
								timeline.setFrame(frame, time, SequenceModeValues[modeAndIndex & 0xf], modeAndIndex >> 4,
									input.readFloat());
							}
							timelines.push(timeline);
							break;
						}
					}
				}
			}
		}

		// Draw order timeline.
		const drawOrderCount = input.readInt(true);
		if (drawOrderCount > 0) {
			const timeline = new DrawOrderTimeline(drawOrderCount);
			const slotCount = skeletonData.slots.length;
			for (let i = 0; i < drawOrderCount; i++) {
				const time = input.readFloat();
				const offsetCount = input.readInt(true);
				const drawOrder = Utils.newArray(slotCount, 0);
				for (let ii = slotCount - 1; ii >= 0; ii--)
					drawOrder[ii] = -1;
				const unchanged = Utils.newArray(slotCount - offsetCount, 0);
				let originalIndex = 0, unchangedIndex = 0;
				for (let ii = 0; ii < offsetCount; ii++) {
					const slotIndex = input.readInt(true);
					// Collect unchanged items.
					while (originalIndex !== slotIndex)
						unchanged[unchangedIndex++] = originalIndex++;
					// Set changed items.
					drawOrder[originalIndex + input.readInt(true)] = originalIndex++;
				}
				// Collect remaining unchanged items.
				while (originalIndex < slotCount)
					unchanged[unchangedIndex++] = originalIndex++;
				// Fill in unchanged items.
				for (let ii = slotCount - 1; ii >= 0; ii--)
					if (drawOrder[ii] === -1) drawOrder[ii] = unchanged[--unchangedIndex];
				timeline.setFrame(i, time, drawOrder);
			}
			timelines.push(timeline);
		}

		// Event timeline.
		const eventCount = input.readInt(true);
		if (eventCount > 0) {
			const timeline = new EventTimeline(eventCount);
			for (let i = 0; i < eventCount; i++) {
				const time = input.readFloat();
				const eventData = skeletonData.events[input.readInt(true)];
				const event = new Event(time, eventData);
				event.intValue = input.readInt(false);
				event.floatValue = input.readFloat();
				event.stringValue = input.readString();
				if (event.stringValue == null) event.stringValue = eventData.stringValue;
				if (event.data.audioPath) {
					event.volume = input.readFloat();
					event.balance = input.readFloat();
				}
				timeline.setFrame(i, event);
			}
			timelines.push(timeline);
		}

		let duration = 0;
		for (let i = 0, n = timelines.length; i < n; i++)
			duration = Math.max(duration, timelines[i].getDuration());
		return new Animation(name, timelines, duration);
	}
}

export class BinaryInput {
	constructor (data: Uint8Array | ArrayBuffer, public strings = [] as string[], private index: number = 0, private buffer = new DataView(data instanceof ArrayBuffer ? data : data.buffer)) {
	}

	readByte (): number {
		return this.buffer.getInt8(this.index++);
	}

	readUnsignedByte (): number {
		return this.buffer.getUint8(this.index++);
	}

	readShort (): number {
		const value = this.buffer.getInt16(this.index);
		this.index += 2;
		return value;
	}

	readInt32 (): number {
		const value = this.buffer.getInt32(this.index)
		this.index += 4;
		return value;
	}

	readInt (optimizePositive: boolean) {
		let b = this.readByte();
		let result = b & 0x7F;
		if ((b & 0x80) !== 0) {
			b = this.readByte();
			result |= (b & 0x7F) << 7;
			if ((b & 0x80) !== 0) {
				b = this.readByte();
				result |= (b & 0x7F) << 14;
				if ((b & 0x80) !== 0) {
					b = this.readByte();
					result |= (b & 0x7F) << 21;
					if ((b & 0x80) !== 0) {
						b = this.readByte();
						result |= (b & 0x7F) << 28;
					}
				}
			}
		}
		return optimizePositive ? result : ((result >>> 1) ^ -(result & 1));
	}

	readStringRef (): string | null {
		const index = this.readInt(true);
		return index === 0 ? null : this.strings[index - 1];
	}

	readString (): string | null {
		let byteCount = this.readInt(true);
		switch (byteCount) {
			case 0:
				return null;
			case 1:
				return "";
		}
		byteCount--;
		let chars = "";
		for (let i = 0; i < byteCount;) {
			const b = this.readUnsignedByte();
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
		const value = this.buffer.getFloat32(this.index);
		this.index += 4;
		return value;
	}

	readBoolean (): boolean {
		return this.readByte() !== 0;
	}
}

class LinkedMesh {
	parent: string | null; skinIndex: number;
	slotIndex: number;
	mesh: MeshAttachment;
	inheritTimeline: boolean;

	constructor (mesh: MeshAttachment, skinIndex: number, slotIndex: number, parent: string | null, inheritDeform: boolean) {
		this.mesh = mesh;
		this.skinIndex = skinIndex;
		this.slotIndex = slotIndex;
		this.parent = parent;
		this.inheritTimeline = inheritDeform;
	}
}

class Vertices {
	constructor (public bones: Array<number> | null = null, public vertices: Array<number> | Float32Array, public length: number = 0) { }
}

enum AttachmentType { Region, BoundingBox, Mesh, LinkedMesh, Path, Point, Clipping }

function readTimeline (input: BinaryInput, timelines: Array<Timeline>, timeline: CurveTimeline1, scale: number): void;
function readTimeline (input: BinaryInput, timelines: Array<Timeline>, timeline: BoneTimeline2, scale: number): void;
function readTimeline (input: BinaryInput, timelines: Array<Timeline>, timeline: CurveTimeline1 | BoneTimeline2, scale: number): void {
	if (timeline instanceof CurveTimeline1)
		readTimeline1(input, timelines, timeline, scale);
	else
		readTimeline2(input, timelines, timeline, scale);
}

function readTimeline1 (input: BinaryInput, timelines: Array<Timeline>, timeline: CurveTimeline1, scale: number): void {
	let time = input.readFloat(), value = input.readFloat() * scale;
	for (let frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1; ; frame++) {
		timeline.setFrame(frame, time, value);
		if (frame === frameLast) break;
		const time2 = input.readFloat(), value2 = input.readFloat() * scale;
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
	timelines.push(timeline);
}

function readTimeline2 (input: BinaryInput, timelines: Array<Timeline>, timeline: BoneTimeline2, scale: number): void {
	let time = input.readFloat(), value1 = input.readFloat() * scale, value2 = input.readFloat() * scale;
	for (let frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1; ; frame++) {
		timeline.setFrame(frame, time, value1, value2);
		if (frame === frameLast) break;
		const time2 = input.readFloat(), nvalue1 = input.readFloat() * scale, nvalue2 = input.readFloat() * scale;
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
	timelines.push(timeline);
}

function setBezier (input: BinaryInput, timeline: CurveTimeline, bezier: number, frame: number, value: number,
	time1: number, time2: number, value1: number, value2: number, scale: number) {
	timeline.setBezier(bezier, frame, value, time1, value1, input.readFloat(), input.readFloat() * scale, input.readFloat(), input.readFloat() * scale, time2, value2);
}

const BONE_ROTATE = 0;
const BONE_TRANSLATE = 1;
const BONE_TRANSLATEX = 2;
const BONE_TRANSLATEY = 3;
const BONE_SCALE = 4;
const BONE_SCALEX = 5;
const BONE_SCALEY = 6;
const BONE_SHEAR = 7;
const BONE_SHEARX = 8;
const BONE_SHEARY = 9;
const BONE_INHERIT = 10;

const SLOT_ATTACHMENT = 0;
const SLOT_RGBA = 1;
const SLOT_RGB = 2;
const SLOT_RGBA2 = 3;
const SLOT_RGB2 = 4;
const SLOT_ALPHA = 5;

const CONSTRAINT_IK = 0;
const CONSTRAINT_PATH = 1;
const CONSTRAINT_TRANSFORM = 2;
const CONSTRAINT_PHYSICS = 3;
const CONSTRAINT_SLIDER = 4;

const ATTACHMENT_DEFORM = 0;
const ATTACHMENT_SEQUENCE = 1;

const PATH_POSITION = 0;
const PATH_SPACING = 1;
const PATH_MIX = 2;

const PHYSICS_INERTIA = 0;
const PHYSICS_STRENGTH = 1;
const PHYSICS_DAMPING = 2;
const PHYSICS_MASS = 4;
const PHYSICS_WIND = 5;
const PHYSICS_GRAVITY = 6;
const PHYSICS_MIX = 7;
const PHYSICS_RESET = 8;

const SLIDER_TIME = 0;
const SLIDER_MIX = 1;

// biome-ignore lint/correctness/noUnusedVariables: intentional
const CURVE_LINEAR = 0;
const CURVE_STEPPED = 1;
const CURVE_BEZIER = 2;
