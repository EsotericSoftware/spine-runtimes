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

import { Animation, Timeline, InheritTimeline, AttachmentTimeline, RGBATimeline, RGBTimeline, RGBA2Timeline, RGB2Timeline, AlphaTimeline, RotateTimeline, TranslateTimeline, TranslateXTimeline, TranslateYTimeline, ScaleTimeline, ScaleXTimeline, ScaleYTimeline, ShearTimeline, ShearXTimeline, ShearYTimeline, IkConstraintTimeline, TransformConstraintTimeline, PathConstraintPositionTimeline, PathConstraintSpacingTimeline, PathConstraintMixTimeline, DeformTimeline, DrawOrderTimeline, EventTimeline, CurveTimeline1, CurveTimeline2, CurveTimeline, SequenceTimeline, PhysicsConstraintResetTimeline, PhysicsConstraintInertiaTimeline, PhysicsConstraintStrengthTimeline, PhysicsConstraintDampingTimeline, PhysicsConstraintMassTimeline, PhysicsConstraintWindTimeline, PhysicsConstraintGravityTimeline, PhysicsConstraintMixTimeline } from "./Animation.js";
import { VertexAttachment, Attachment } from "./attachments/Attachment.js";
import { AttachmentLoader } from "./attachments/AttachmentLoader.js";
import { HasTextureRegion } from "./attachments/HasTextureRegion.js";
import { MeshAttachment } from "./attachments/MeshAttachment.js";
import { Sequence, SequenceModeValues } from "./attachments/Sequence.js";
import { BoneData } from "./BoneData.js";
import { Event } from "./Event.js";
import { EventData } from "./EventData.js";
import { IkConstraintData } from "./IkConstraintData.js";
import { PathConstraintData, PositionMode, SpacingMode } from "./PathConstraintData.js";
import { PhysicsConstraintData } from "./PhysicsConstraintData.js";
import { SkeletonData } from "./SkeletonData.js";
import { Skin } from "./Skin.js";
import { SlotData } from "./SlotData.js";
import { TransformConstraintData } from "./TransformConstraintData.js";
import { Color, Utils } from "./Utils.js";

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
	private linkedMeshes = new Array<LinkedMesh>();

	constructor (attachmentLoader: AttachmentLoader) {
		this.attachmentLoader = attachmentLoader;
	}

	readSkeletonData (binary: Uint8Array): SkeletonData {
		let scale = this.scale;

		let skeletonData = new SkeletonData();
		skeletonData.name = ""; // BOZO

		let input = new BinaryInput(binary);

		let lowHash = input.readInt32();
		let highHash = input.readInt32();
		skeletonData.hash = highHash == 0 && lowHash == 0 ? null : highHash.toString(16) + lowHash.toString(16);
		skeletonData.version = input.readString();
		skeletonData.x = input.readFloat();
		skeletonData.y = input.readFloat();
		skeletonData.width = input.readFloat();
		skeletonData.height = input.readFloat();
		skeletonData.referenceScale = input.readFloat() * scale;

		let nonessential = input.readBoolean();
		if (nonessential) {
			skeletonData.fps = input.readFloat();
			skeletonData.imagesPath = input.readString();
			skeletonData.audioPath = input.readString();
		}

		let n = 0;
		// Strings.
		n = input.readInt(true)
		for (let i = 0; i < n; i++) {
			let str = input.readString();
			if (!str) throw new Error("String in string table must not be null.");
			input.strings.push(str);
		}

		// Bones.
		n = input.readInt(true)
		for (let i = 0; i < n; i++) {
			let name = input.readString();
			if (!name) throw new Error("Bone name must not be null.");
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
			data.inherit = input.readByte();
			data.skinRequired = input.readBoolean();
			if (nonessential) {
				Color.rgba8888ToColor(data.color, input.readInt32());
				data.icon = input.readString() ?? undefined;
				data.visible = input.readBoolean();
			}
			skeletonData.bones.push(data);
		}

		// Slots.
		n = input.readInt(true);
		for (let i = 0; i < n; i++) {
			let slotName = input.readString();
			if (!slotName) throw new Error("Slot name must not be null.");
			let path: string | null = null;
			if (nonessential) {
				const slash = slotName!.lastIndexOf('/');
				if (slash != -1) {
					path = slotName.substring(0, slash);
					slotName = slotName.substring(slash + 1);
				}
			}
			let boneData = skeletonData.bones[input.readInt(true)];
			let data = new SlotData(i, slotName, boneData);
			Color.rgba8888ToColor(data.color, input.readInt32());

			let darkColor = input.readInt32();
			if (darkColor != -1) Color.rgb888ToColor(data.darkColor = new Color(), darkColor);

			data.attachmentName = input.readStringRef();
			data.blendMode = input.readInt(true);
			if (nonessential) {
				data.visible = input.readBoolean();
				data.path = path;
			}
			skeletonData.slots.push(data);
		}

		// IK constraints.
		n = input.readInt(true);
		for (let i = 0, nn; i < n; i++) {
			let name = input.readString();
			if (!name) throw new Error("IK constraint data name must not be null.");
			let data = new IkConstraintData(name);
			data.order = input.readInt(true);
			nn = input.readInt(true);
			for (let ii = 0; ii < nn; ii++)
				data.bones.push(skeletonData.bones[input.readInt(true)]);
			data.target = skeletonData.bones[input.readInt(true)];
			let flags = input.readByte();
			data.skinRequired = (flags & 1) != 0;
			data.bendDirection = (flags & 2) != 0 ? 1 : -1;
			data.compress = (flags & 4) != 0;
			data.stretch = (flags & 8) != 0;
			data.uniform = (flags & 16) != 0;
			if ((flags & 32) != 0) data.mix = (flags & 64) != 0 ? input.readFloat() : 1;
			if ((flags & 128) != 0) data.softness = input.readFloat() * scale;
			skeletonData.ikConstraints.push(data);
		}

		// Transform constraints.
		n = input.readInt(true);
		for (let i = 0, nn; i < n; i++) {
			let name = input.readString();
			if (!name) throw new Error("Transform constraint data name must not be null.");
			let data = new TransformConstraintData(name);
			data.order = input.readInt(true);
			nn = input.readInt(true);
			for (let ii = 0; ii < nn; ii++)
				data.bones.push(skeletonData.bones[input.readInt(true)]);
			data.target = skeletonData.bones[input.readInt(true)];
			let flags = input.readByte();
			data.skinRequired = (flags & 1) != 0;
			data.local = (flags & 2) != 0;
			data.relative = (flags & 4) != 0;
			if ((flags & 8) != 0) data.offsetRotation = input.readFloat();
			if ((flags & 16) != 0) data.offsetX = input.readFloat() * scale;
			if ((flags & 32) != 0) data.offsetY = input.readFloat() * scale;
			if ((flags & 64) != 0) data.offsetScaleX = input.readFloat();
			if ((flags & 128) != 0) data.offsetScaleY = input.readFloat();
			flags = input.readByte();
			if ((flags & 1) != 0) data.offsetShearY = input.readFloat();
			if ((flags & 2) != 0) data.mixRotate = input.readFloat();
			if ((flags & 4) != 0) data.mixX = input.readFloat();
			if ((flags & 8) != 0) data.mixY = input.readFloat();
			if ((flags & 16) != 0) data.mixScaleX = input.readFloat();
			if ((flags & 32) != 0) data.mixScaleY = input.readFloat();
			if ((flags & 64) != 0) data.mixShearY = input.readFloat();
			skeletonData.transformConstraints.push(data);
		}

		// Path constraints.
		n = input.readInt(true);
		for (let i = 0, nn; i < n; i++) {
			let name = input.readString();
			if (!name) throw new Error("Path constraint data name must not be null.");
			let data = new PathConstraintData(name);
			data.order = input.readInt(true);
			data.skinRequired = input.readBoolean();
			nn = input.readInt(true);
			for (let ii = 0; ii < nn; ii++)
				data.bones.push(skeletonData.bones[input.readInt(true)]);
			data.target = skeletonData.slots[input.readInt(true)];
			const flags = input.readByte();
			data.positionMode = flags & 1;
			data.spacingMode = (flags >> 1) & 3;
			data.rotateMode = (flags >> 3) & 3;
			if ((flags & 128) != 0) data.offsetRotation = input.readFloat();
			data.position = input.readFloat();
			if (data.positionMode == PositionMode.Fixed) data.position *= scale;
			data.spacing = input.readFloat();
			if (data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed) data.spacing *= scale;
			data.mixRotate = input.readFloat();
			data.mixX = input.readFloat();
			data.mixY = input.readFloat();
			skeletonData.pathConstraints.push(data);
		}

		// Physics constraints.
		n = input.readInt(true);
		for (let i = 0, nn; i < n; i++) {
			const name = input.readString();
			if (!name) throw new Error("Physics constraint data name must not be null.");
			const data = new PhysicsConstraintData(name);
			data.order = input.readInt(true);
			data.bone = skeletonData.bones[input.readInt(true)];
			let flags = input.readByte();
			data.skinRequired = (flags & 1) != 0;
			if ((flags & 2) != 0) data.x = input.readFloat();
			if ((flags & 4) != 0) data.y = input.readFloat();
			if ((flags & 8) != 0) data.rotate = input.readFloat();
			if ((flags & 16) != 0) data.scaleX = input.readFloat();
			if ((flags & 32) != 0) data.shearX = input.readFloat();
			data.limit = ((flags & 64) != 0 ? input.readFloat() : 5000) * scale;
			data.step = 1 / input.readUnsignedByte();
			data.inertia = input.readFloat();
			data.strength = input.readFloat();
			data.damping = input.readFloat();
			data.massInverse = (flags & 128) != 0 ? input.readFloat() : 1;
			data.wind = input.readFloat();
			data.gravity = input.readFloat();
			flags = input.readByte();
			if ((flags & 1) != 0) data.inertiaGlobal = true;
			if ((flags & 2) != 0) data.strengthGlobal = true;
			if ((flags & 4) != 0) data.dampingGlobal = true;
			if ((flags & 8) != 0) data.massGlobal = true;
			if ((flags & 16) != 0) data.windGlobal = true;
			if ((flags & 32) != 0) data.gravityGlobal = true;
			if ((flags & 64) != 0) data.mixGlobal = true;
			data.mix = (flags & 128) != 0 ? input.readFloat() : 1;
			skeletonData.physicsConstraints.push(data);
		}

		// Default skin.
		let defaultSkin = this.readSkin(input, skeletonData, true, nonessential);
		if (defaultSkin) {
			skeletonData.defaultSkin = defaultSkin;
			skeletonData.skins.push(defaultSkin);
		}

		// Skins.
		{
			let i = skeletonData.skins.length;
			Utils.setArraySize(skeletonData.skins, n = i + input.readInt(true));
			for (; i < n; i++) {
				let skin = this.readSkin(input, skeletonData, false, nonessential);
				if (!skin) throw new Error("readSkin() should not have returned null.");
				skeletonData.skins[i] = skin;
			}
		}

		// Linked meshes.
		n = this.linkedMeshes.length;
		for (let i = 0; i < n; i++) {
			let linkedMesh = this.linkedMeshes[i];
			const skin = skeletonData.skins[linkedMesh.skinIndex];
			if (!linkedMesh.parent) throw new Error("Linked mesh parent must not be null");
			let parent = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
			if (!parent) throw new Error(`Parent mesh not found: ${linkedMesh.parent}`);
			linkedMesh.mesh.timelineAttachment = linkedMesh.inheritTimeline ? parent as VertexAttachment : linkedMesh.mesh;
			linkedMesh.mesh.setParentMesh(parent as MeshAttachment);
			if (linkedMesh.mesh.region != null) linkedMesh.mesh.updateRegion();
		}
		this.linkedMeshes.length = 0;

		// Events.
		n = input.readInt(true);
		for (let i = 0; i < n; i++) {
			let eventName = input.readString();
			if (!eventName) throw new Error("Event data name must not be null");
			let data = new EventData(eventName);
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
		n = input.readInt(true);
		for (let i = 0; i < n; i++) {
			let animationName = input.readString();
			if (!animationName) throw new Error("Animatio name must not be null.");
			skeletonData.animations.push(this.readAnimation(input, animationName, skeletonData));
		}
		return skeletonData;
	}

	private readSkin (input: BinaryInput, skeletonData: SkeletonData, defaultSkin: boolean, nonessential: boolean): Skin | null {
		let skin = null;
		let slotCount = 0;

		if (defaultSkin) {
			slotCount = input.readInt(true)
			if (slotCount == 0) return null;
			skin = new Skin("default");
		} else {
			let skinName = input.readString();
			if (!skinName) throw new Error("Skin name must not be null.");
			skin = new Skin(skinName);
			if (nonessential) Color.rgba8888ToColor(skin.color, input.readInt32());
			skin.bones.length = input.readInt(true);
			for (let i = 0, n = skin.bones.length; i < n; i++)
				skin.bones[i] = skeletonData.bones[input.readInt(true)];

			for (let i = 0, n = input.readInt(true); i < n; i++)
				skin.constraints.push(skeletonData.ikConstraints[input.readInt(true)]);
			for (let i = 0, n = input.readInt(true); i < n; i++)
				skin.constraints.push(skeletonData.transformConstraints[input.readInt(true)]);
			for (let i = 0, n = input.readInt(true); i < n; i++)
				skin.constraints.push(skeletonData.pathConstraints[input.readInt(true)]);
			for (let i = 0, n = input.readInt(true); i < n; i++)
				skin.constraints.push(skeletonData.physicsConstraints[input.readInt(true)]);

			slotCount = input.readInt(true);
		}

		for (let i = 0; i < slotCount; i++) {
			let slotIndex = input.readInt(true);
			for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				let name = input.readStringRef();
				if (!name)
					throw new Error("Attachment name must not be null");
				let attachment = this.readAttachment(input, skeletonData, skin, slotIndex, name, nonessential);
				if (attachment) skin.setAttachment(slotIndex, name, attachment);
			}
		}
		return skin;
	}

	private readAttachment (input: BinaryInput, skeletonData: SkeletonData, skin: Skin, slotIndex: number, attachmentName: string | null | undefined, nonessential: boolean): Attachment | null {
		let scale = this.scale;

		let flags = input.readByte();
		const name = (flags & 8) != 0 ? input.readStringRef() : attachmentName;
		if (!name) throw new Error("Attachment name must not be null");
		switch ((flags & 0b111) as AttachmentType) { // BUG?
			case AttachmentType.Region: {
				let path = (flags & 16) != 0 ? input.readStringRef() : null;
				const color = (flags & 32) != 0 ? input.readInt32() : 0xffffffff;
				const sequence = (flags & 64) != 0 ? this.readSequence(input) : null;
				let rotation = (flags & 128) != 0 ? input.readFloat() : 0;
				let x = input.readFloat();
				let y = input.readFloat();
				let scaleX = input.readFloat();
				let scaleY = input.readFloat();
				let width = input.readFloat();
				let height = input.readFloat();

				if (!path) path = name;
				let region = this.attachmentLoader.newRegionAttachment(skin, name, path, sequence);
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
				if (sequence == null) region.updateRegion();
				return region;
			}
			case AttachmentType.BoundingBox: {
				let vertices = this.readVertices(input, (flags & 16) != 0);
				let color = nonessential ? input.readInt32() : 0;

				let box = this.attachmentLoader.newBoundingBoxAttachment(skin, name);
				if (!box) return null;
				box.worldVerticesLength = vertices.length;
				box.vertices = vertices.vertices!;
				box.bones = vertices.bones;
				if (nonessential) Color.rgba8888ToColor(box.color, color);
				return box;
			}
			case AttachmentType.Mesh: {
				let path = (flags & 16) != 0 ? input.readStringRef() : name;
				const color = (flags & 32) != 0 ? input.readInt32() : 0xffffffff;
				const sequence = (flags & 64) != 0 ? this.readSequence(input) : null;
				const hullLength = input.readInt(true);
				const vertices = this.readVertices(input, (flags & 128) != 0);
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
				let mesh = this.attachmentLoader.newMeshAttachment(skin, name, path, sequence);
				if (!mesh) return null;
				mesh.path = path;
				Color.rgba8888ToColor(mesh.color, color);
				mesh.bones = vertices.bones;
				mesh.vertices = vertices.vertices!;
				mesh.worldVerticesLength = vertices.length;
				mesh.triangles = triangles;
				mesh.regionUVs = uvs;
				if (sequence == null) mesh.updateRegion();
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
				const path = (flags & 16) != 0 ? input.readStringRef() : name;
				if (path == null) throw new Error("Path of linked mesh must not be null");
				const color = (flags & 32) != 0 ? input.readInt32() : 0xffffffff;
				const sequence = (flags & 64) != 0 ? this.readSequence(input) : null;
				const inheritTimelines = (flags & 128) != 0;
				const skinIndex = input.readInt(true);
				const parent = input.readStringRef();
				let width = 0, height = 0;
				if (nonessential) {
					width = input.readFloat();
					height = input.readFloat();
				}

				let mesh = this.attachmentLoader.newMeshAttachment(skin, name, path, sequence);
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
				const closed = (flags & 16) != 0;
				const constantSpeed = (flags & 32) != 0;
				const vertices = this.readVertices(input, (flags & 64) != 0);

				const lengths = Utils.newArray(vertices.length / 6, 0);
				for (let i = 0, n = lengths.length; i < n; i++)
					lengths[i] = input.readFloat() * scale;
				const color = nonessential ? input.readInt32() : 0;

				const path = this.attachmentLoader.newPathAttachment(skin, name);
				if (!path) return null;
				path.closed = closed;
				path.constantSpeed = constantSpeed;
				path.worldVerticesLength = vertices.length;
				path.vertices = vertices.vertices!;
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
				const vertices = this.readVertices(input, (flags & 16) != 0);
				let color = nonessential ? input.readInt32() : 0;

				let clip = this.attachmentLoader.newClippingAttachment(skin, name);
				if (!clip) return null;
				clip.endSlot = skeletonData.slots[endSlotIndex];
				clip.worldVerticesLength = vertices.length;
				clip.vertices = vertices.vertices!;
				clip.bones = vertices.bones;
				if (nonessential) Color.rgba8888ToColor(clip.color, color);
				return clip;
			}
		}
		return null;
	}

	private readSequence (input: BinaryInput) {
		let sequence = new Sequence(input.readInt(true));
		sequence.start = input.readInt(true);
		sequence.digits = input.readInt(true);
		sequence.setupIndex = input.readInt(true);
		return sequence;
	}

	private readVertices (input: BinaryInput, weighted: boolean): Vertices {
		const scale = this.scale;
		const vertexCount = input.readInt(true);
		const vertices = new Vertices();
		vertices.length = vertexCount << 1;
		if (!weighted) {
			vertices.vertices = this.readFloatArray(input, vertices.length, scale);
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

	private readShortArray (input: BinaryInput, n: number): number[] {
		let array = new Array<number>(n);
		for (let i = 0; i < n; i++)
			array[i] = input.readInt(true);
		return array;
	}

	private readAnimation (input: BinaryInput, name: string, skeletonData: SkeletonData): Animation {
		input.readInt(true); // Number of timelines.
		let timelines = new Array<Timeline>();
		let scale = this.scale;

		// Slot timelines.
		for (let i = 0, n = input.readInt(true); i < n; i++) {
			let slotIndex = input.readInt(true);
			for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				let timelineType = input.readByte();
				let frameCount = input.readInt(true);
				let frameLast = frameCount - 1;
				switch (timelineType) {
					case SLOT_ATTACHMENT: {
						let timeline = new AttachmentTimeline(frameCount, slotIndex);
						for (let frame = 0; frame < frameCount; frame++)
							timeline.setFrame(frame, input.readFloat(), input.readStringRef());
						timelines.push(timeline);
						break;
					}
					case SLOT_RGBA: {
						let bezierCount = input.readInt(true);
						let timeline = new RGBATimeline(frameCount, bezierCount, slotIndex);

						let time = input.readFloat();
						let r = input.readUnsignedByte() / 255.0;
						let g = input.readUnsignedByte() / 255.0;
						let b = input.readUnsignedByte() / 255.0;
						let a = input.readUnsignedByte() / 255.0;

						for (let frame = 0, bezier = 0; ; frame++) {
							timeline.setFrame(frame, time, r, g, b, a);
							if (frame == frameLast) break;

							let time2 = input.readFloat();
							let r2 = input.readUnsignedByte() / 255.0;
							let g2 = input.readUnsignedByte() / 255.0;
							let b2 = input.readUnsignedByte() / 255.0;
							let a2 = input.readUnsignedByte() / 255.0;

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
						let bezierCount = input.readInt(true);
						let timeline = new RGBTimeline(frameCount, bezierCount, slotIndex);

						let time = input.readFloat();
						let r = input.readUnsignedByte() / 255.0;
						let g = input.readUnsignedByte() / 255.0;
						let b = input.readUnsignedByte() / 255.0;

						for (let frame = 0, bezier = 0; ; frame++) {
							timeline.setFrame(frame, time, r, g, b);
							if (frame == frameLast) break;

							let time2 = input.readFloat();
							let r2 = input.readUnsignedByte() / 255.0;
							let g2 = input.readUnsignedByte() / 255.0;
							let b2 = input.readUnsignedByte() / 255.0;

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
						let bezierCount = input.readInt(true);
						let timeline = new RGBA2Timeline(frameCount, bezierCount, slotIndex);

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
							if (frame == frameLast) break;
							let time2 = input.readFloat();
							let nr = input.readUnsignedByte() / 255.0;
							let ng = input.readUnsignedByte() / 255.0;
							let nb = input.readUnsignedByte() / 255.0;
							let na = input.readUnsignedByte() / 255.0;
							let nr2 = input.readUnsignedByte() / 255.0;
							let ng2 = input.readUnsignedByte() / 255.0;
							let nb2 = input.readUnsignedByte() / 255.0;

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
						let bezierCount = input.readInt(true);
						let timeline = new RGB2Timeline(frameCount, bezierCount, slotIndex);

						let time = input.readFloat();
						let r = input.readUnsignedByte() / 255.0;
						let g = input.readUnsignedByte() / 255.0;
						let b = input.readUnsignedByte() / 255.0;
						let r2 = input.readUnsignedByte() / 255.0;
						let g2 = input.readUnsignedByte() / 255.0;
						let b2 = input.readUnsignedByte() / 255.0;

						for (let frame = 0, bezier = 0; ; frame++) {
							timeline.setFrame(frame, time, r, g, b, r2, g2, b2);
							if (frame == frameLast) break;
							let time2 = input.readFloat();
							let nr = input.readUnsignedByte() / 255.0;
							let ng = input.readUnsignedByte() / 255.0;
							let nb = input.readUnsignedByte() / 255.0;
							let nr2 = input.readUnsignedByte() / 255.0;
							let ng2 = input.readUnsignedByte() / 255.0;
							let nb2 = input.readUnsignedByte() / 255.0;

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
						let timeline = new AlphaTimeline(frameCount, input.readInt(true), slotIndex);
						let time = input.readFloat(), a = input.readUnsignedByte() / 255;
						for (let frame = 0, bezier = 0; ; frame++) {
							timeline.setFrame(frame, time, a);
							if (frame == frameLast) break;
							let time2 = input.readFloat();
							let a2 = input.readUnsignedByte() / 255;
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
			let boneIndex = input.readInt(true);
			for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				let type = input.readByte(), frameCount = input.readInt(true);
				if (type == BONE_INHERIT) {
					let timeline = new InheritTimeline(frameCount, boneIndex);
					for (let frame = 0; frame < frameCount; frame++) {
						timeline.setFrame(frame, input.readFloat(), input.readByte());
					}
					timelines.push(timeline);
					continue;
				}
				let bezierCount = input.readInt(true);
				switch (type) {
					case BONE_ROTATE:
						timelines.push(readTimeline1(input, new RotateTimeline(frameCount, bezierCount, boneIndex), 1));
						break;
					case BONE_TRANSLATE:
						timelines.push(readTimeline2(input, new TranslateTimeline(frameCount, bezierCount, boneIndex), scale));
						break;
					case BONE_TRANSLATEX:
						timelines.push(readTimeline1(input, new TranslateXTimeline(frameCount, bezierCount, boneIndex), scale));
						break;
					case BONE_TRANSLATEY:
						timelines.push(readTimeline1(input, new TranslateYTimeline(frameCount, bezierCount, boneIndex), scale));
						break;
					case BONE_SCALE:
						timelines.push(readTimeline2(input, new ScaleTimeline(frameCount, bezierCount, boneIndex), 1));
						break;
					case BONE_SCALEX:
						timelines.push(readTimeline1(input, new ScaleXTimeline(frameCount, bezierCount, boneIndex), 1));
						break;
					case BONE_SCALEY:
						timelines.push(readTimeline1(input, new ScaleYTimeline(frameCount, bezierCount, boneIndex), 1));
						break;
					case BONE_SHEAR:
						timelines.push(readTimeline2(input, new ShearTimeline(frameCount, bezierCount, boneIndex), 1));
						break;
					case BONE_SHEARX:
						timelines.push(readTimeline1(input, new ShearXTimeline(frameCount, bezierCount, boneIndex), 1));
						break;
					case BONE_SHEARY:
						timelines.push(readTimeline1(input, new ShearYTimeline(frameCount, bezierCount, boneIndex), 1));
				}
			}
		}

		// IK constraint timelines.
		for (let i = 0, n = input.readInt(true); i < n; i++) {
			let index = input.readInt(true), frameCount = input.readInt(true), frameLast = frameCount - 1;
			let timeline = new IkConstraintTimeline(frameCount, input.readInt(true), index);
			let flags = input.readByte();
			let time = input.readFloat(), mix = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.readFloat() : 1) : 0;
			let softness = (flags & 4) != 0 ? input.readFloat() * scale : 0;
			for (let frame = 0, bezier = 0; ; frame++) {
				timeline.setFrame(frame, time, mix, softness, (flags & 8) != 0 ? 1 : -1, (flags & 16) != 0, (flags & 32) != 0);
				if (frame == frameLast) break;
				flags = input.readByte();
				const time2 = input.readFloat(), mix2 = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.readFloat() : 1) : 0;
				const softness2 = (flags & 4) != 0 ? input.readFloat() * scale : 0;
				if ((flags & 64) != 0) {
					timeline.setStepped(frame);
				} else if ((flags & 128) != 0) {
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
			let index = input.readInt(true), frameCount = input.readInt(true), frameLast = frameCount - 1;
			let timeline = new TransformConstraintTimeline(frameCount, input.readInt(true), index);
			let time = input.readFloat(), mixRotate = input.readFloat(), mixX = input.readFloat(), mixY = input.readFloat(),
				mixScaleX = input.readFloat(), mixScaleY = input.readFloat(), mixShearY = input.readFloat();
			for (let frame = 0, bezier = 0; ; frame++) {
				timeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
				if (frame == frameLast) break;
				let time2 = input.readFloat(), mixRotate2 = input.readFloat(), mixX2 = input.readFloat(), mixY2 = input.readFloat(),
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
			let index = input.readInt(true);
			let data = skeletonData.pathConstraints[index];
			for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				const type = input.readByte(), frameCount = input.readInt(true), bezierCount = input.readInt(true);
				switch (type) {
					case PATH_POSITION:
						timelines
							.push(readTimeline1(input, new PathConstraintPositionTimeline(frameCount, bezierCount, index),
								data.positionMode == PositionMode.Fixed ? scale : 1));
						break;
					case PATH_SPACING:
						timelines
							.push(readTimeline1(input, new PathConstraintSpacingTimeline(frameCount, bezierCount, index),
								data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed ? scale : 1));
						break;
					case PATH_MIX:
						let timeline = new PathConstraintMixTimeline(frameCount, bezierCount, index);
						let time = input.readFloat(), mixRotate = input.readFloat(), mixX = input.readFloat(), mixY = input.readFloat();
						for (let frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1; ; frame++) {
							timeline.setFrame(frame, time, mixRotate, mixX, mixY);
							if (frame == frameLast) break;
							let time2 = input.readFloat(), mixRotate2 = input.readFloat(), mixX2 = input.readFloat(),
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

		// Physics timelines.
		for (let i = 0, n = input.readInt(true); i < n; i++) {
			const index = input.readInt(true) - 1;
			for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				const type = input.readByte(), frameCount = input.readInt(true);
				if (type == PHYSICS_RESET) {
					const timeline = new PhysicsConstraintResetTimeline(frameCount, index);
					for (let frame = 0; frame < frameCount; frame++)
						timeline.setFrame(frame, input.readFloat());
					timelines.push(timeline);
					continue;
				}
				const bezierCount = input.readInt(true);
				switch (type) {
					case PHYSICS_INERTIA:
						timelines.push(readTimeline1(input, new PhysicsConstraintInertiaTimeline(frameCount, bezierCount, index), 1));
						break;
					case PHYSICS_STRENGTH:
						timelines.push(readTimeline1(input, new PhysicsConstraintStrengthTimeline(frameCount, bezierCount, index), 1));
						break;
					case PHYSICS_DAMPING:
						timelines.push(readTimeline1(input, new PhysicsConstraintDampingTimeline(frameCount, bezierCount, index), 1));
						break;
					case PHYSICS_MASS:
						timelines.push(readTimeline1(input, new PhysicsConstraintMassTimeline(frameCount, bezierCount, index), 1));
						break;
					case PHYSICS_WIND:
						timelines.push(readTimeline1(input, new PhysicsConstraintWindTimeline(frameCount, bezierCount, index), 1));
						break;
					case PHYSICS_GRAVITY:
						timelines.push(readTimeline1(input, new PhysicsConstraintGravityTimeline(frameCount, bezierCount, index), 1));
						break;
					case PHYSICS_MIX:
						timelines.push(readTimeline1(input, new PhysicsConstraintMixTimeline(frameCount, bezierCount, index), 1));
				}
			}
		}

		// Deform timelines.
		for (let i = 0, n = input.readInt(true); i < n; i++) {
			let skin = skeletonData.skins[input.readInt(true)];
			for (let ii = 0, nn = input.readInt(true); ii < nn; ii++) {
				let slotIndex = input.readInt(true);
				for (let iii = 0, nnn = input.readInt(true); iii < nnn; iii++) {
					let attachmentName = input.readStringRef();
					if (!attachmentName) throw new Error("attachmentName must not be null.");
					let attachment = skin.getAttachment(slotIndex, attachmentName);
					let timelineType = input.readByte();
					let frameCount = input.readInt(true);
					let frameLast = frameCount - 1;

					switch (timelineType) {
						case ATTACHMENT_DEFORM: {
							let vertexAttachment = attachment as VertexAttachment;
							let weighted = vertexAttachment.bones;
							let vertices = vertexAttachment.vertices;
							let deformLength = weighted ? vertices.length / 3 * 2 : vertices.length;


							let bezierCount = input.readInt(true);
							let timeline = new DeformTimeline(frameCount, bezierCount, slotIndex, vertexAttachment);

							let time = input.readFloat();
							for (let frame = 0, bezier = 0; ; frame++) {
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

								timeline.setFrame(frame, time, deform);
								if (frame == frameLast) break;
								let time2 = input.readFloat();
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
							let timeline = new SequenceTimeline(frameCount, slotIndex, attachment as unknown as HasTextureRegion);
							for (let frame = 0; frame < frameCount; frame++) {
								let time = input.readFloat();
								let modeAndIndex = input.readInt32();
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
	constructor (data: Uint8Array, public strings = new Array<string>(), private index: number = 0, private buffer = new DataView(data.buffer)) {
	}

	readByte (): number {
		return this.buffer.getInt8(this.index++);
	}

	readUnsignedByte (): number {
		return this.buffer.getUint8(this.index++);
	}

	readShort (): number {
		let value = this.buffer.getInt16(this.index);
		this.index += 2;
		return value;
	}

	readInt32 (): number {
		let value = this.buffer.getInt32(this.index)
		this.index += 4;
		return value;
	}

	readInt (optimizePositive: boolean) {
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

	readStringRef (): string | null {
		let index = this.readInt(true);
		return index == 0 ? null : this.strings[index - 1];
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
		let charCount = 0;
		for (let i = 0; i < byteCount;) {
			let b = this.readUnsignedByte();
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
	constructor (public bones: Array<number> | null = null, public vertices: Array<number> | Float32Array | null = null, public length: number = 0) { }
}

enum AttachmentType { Region, BoundingBox, Mesh, LinkedMesh, Path, Point, Clipping }

function readTimeline1 (input: BinaryInput, timeline: CurveTimeline1, scale: number): CurveTimeline1 {
	let time = input.readFloat(), value = input.readFloat() * scale;
	for (let frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1; ; frame++) {
		timeline.setFrame(frame, time, value);
		if (frame == frameLast) break;
		let time2 = input.readFloat(), value2 = input.readFloat() * scale;
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

function readTimeline2 (input: BinaryInput, timeline: CurveTimeline2, scale: number): CurveTimeline2 {
	let time = input.readFloat(), value1 = input.readFloat() * scale, value2 = input.readFloat() * scale;
	for (let frame = 0, bezier = 0, frameLast = timeline.getFrameCount() - 1; ; frame++) {
		timeline.setFrame(frame, time, value1, value2);
		if (frame == frameLast) break;
		let time2 = input.readFloat(), nvalue1 = input.readFloat() * scale, nvalue2 = input.readFloat() * scale;
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

const CURVE_LINEAR = 0;
const CURVE_STEPPED = 1;
const CURVE_BEZIER = 2;
