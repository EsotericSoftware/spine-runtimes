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

import { AlphaTimeline, Animation, AttachmentTimeline, type BoneTimeline2, type CurveTimeline, type CurveTimeline1, DeformTimeline, DrawOrderTimeline, EventTimeline, IkConstraintTimeline, InheritTimeline, PathConstraintMixTimeline, PathConstraintPositionTimeline, PathConstraintSpacingTimeline, PhysicsConstraintDampingTimeline, PhysicsConstraintGravityTimeline, PhysicsConstraintInertiaTimeline, PhysicsConstraintMassTimeline, PhysicsConstraintMixTimeline, PhysicsConstraintResetTimeline, PhysicsConstraintStrengthTimeline, PhysicsConstraintWindTimeline, RGB2Timeline, RGBA2Timeline, RGBATimeline, RGBTimeline, RotateTimeline, ScaleTimeline, ScaleXTimeline, ScaleYTimeline, SequenceTimeline, ShearTimeline, ShearXTimeline, ShearYTimeline, SliderMixTimeline, SliderTimeline, type Timeline, TransformConstraintTimeline, TranslateTimeline, TranslateXTimeline, TranslateYTimeline } from "./Animation.js";
import type { Attachment, VertexAttachment } from "./attachments/Attachment.js";
import type { AttachmentLoader } from "./attachments/AttachmentLoader.js";
import type { HasTextureRegion } from "./attachments/HasTextureRegion.js";
import type { MeshAttachment } from "./attachments/MeshAttachment.js";
import { Sequence, SequenceMode } from "./attachments/Sequence.js";
import { BoneData, Inherit } from "./BoneData.js";
import { Event } from "./Event.js";
import { EventData } from "./EventData.js";
import { IkConstraintData } from "./IkConstraintData.js";
import { PathConstraintData, PositionMode, RotateMode, SpacingMode } from "./PathConstraintData.js";
import { PhysicsConstraintData } from "./PhysicsConstraintData.js";
import { SkeletonData } from "./SkeletonData.js";
import { Skin } from "./Skin.js";
import { SliderData } from "./SliderData.js";
import { BlendMode, SlotData } from "./SlotData.js";
import { type FromProperty, FromRotate, FromScaleX, FromScaleY, FromShearY, FromX, FromY, type ToProperty, ToRotate, ToScaleX, ToScaleY, ToShearY, ToX, ToY, TransformConstraintData } from "./TransformConstraintData.js";
import { Color, type NumberArrayLike, Utils } from "./Utils.js";

/** Loads skeleton data in the Spine JSON format.
 *
 * See [Spine JSON format](http://esotericsoftware.com/spine-json-format) and
 * [JSON and binary data](http://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data) in the Spine
 * Runtimes Guide. */
export class SkeletonJson {
	attachmentLoader: AttachmentLoader;

	/** Scales bone positions, image sizes, and translations as they are loaded. This allows different size images to be used at
	 * runtime than were used in Spine.
	 *
	 * See [Scaling](http://esotericsoftware.com/spine-loading-skeleton-data#Scaling) in the Spine Runtimes Guide. */
	scale = 1;
	private readonly linkedMeshes = [] as LinkedMesh[];

	constructor (attachmentLoader: AttachmentLoader) {
		this.attachmentLoader = attachmentLoader;
	}

	// biome-ignore lint/suspicious/noExplicitAny: it is any until we define a schema
	readSkeletonData (json: string | any): SkeletonData {
		const scale = this.scale;
		const skeletonData = new SkeletonData();
		const root = typeof (json) === "string" ? JSON.parse(json) : json;

		// Skeleton
		const skeletonMap = root.skeleton;
		if (skeletonMap) {
			skeletonData.hash = skeletonMap.hash;
			skeletonData.version = skeletonMap.spine;
			skeletonData.x = skeletonMap.x;
			skeletonData.y = skeletonMap.y;
			skeletonData.width = skeletonMap.width;
			skeletonData.height = skeletonMap.height;
			skeletonData.referenceScale = getValue(skeletonMap, "referenceScale", 100) * scale;
			skeletonData.fps = skeletonMap.fps;
			skeletonData.imagesPath = skeletonMap.images ?? null;
			skeletonData.audioPath = skeletonMap.audio ?? null;
		}

		// Bones
		if (root.bones) {
			for (let i = 0; i < root.bones.length; i++) {
				const boneMap = root.bones[i];

				let parent: BoneData | null = null;
				const parentName: string = getValue(boneMap, "parent", null);
				if (parentName) parent = skeletonData.findBone(parentName);
				const data = new BoneData(skeletonData.bones.length, boneMap.name, parent);
				data.length = getValue(boneMap, "length", 0) * scale;
				const setup = data.setup;
				setup.x = getValue(boneMap, "x", 0) * scale;
				setup.y = getValue(boneMap, "y", 0) * scale;
				setup.rotation = getValue(boneMap, "rotation", 0);
				setup.scaleX = getValue(boneMap, "scaleX", 1);
				setup.scaleY = getValue(boneMap, "scaleY", 1);
				setup.shearX = getValue(boneMap, "shearX", 0);
				setup.shearY = getValue(boneMap, "shearY", 0);
				setup.inherit = Utils.enumValue(Inherit, getValue(boneMap, "inherit", "Normal"));
				data.skinRequired = getValue(boneMap, "skin", false);

				const color = getValue(boneMap, "color", null);
				if (color) data.color.setFromString(color);

				skeletonData.bones.push(data);
			}
		}

		// Slots.
		if (root.slots) {
			for (let i = 0; i < root.slots.length; i++) {
				const slotMap = root.slots[i];
				const slotName = slotMap.name;

				const boneData = skeletonData.findBone(slotMap.bone);
				if (!boneData) throw new Error(`Couldn't find bone ${slotMap.bone} for slot ${slotName}`);
				const data = new SlotData(skeletonData.slots.length, slotName, boneData);

				const color: string = getValue(slotMap, "color", null);
				if (color) data.setup.color.setFromString(color);

				const dark: string = getValue(slotMap, "dark", null);
				if (dark) data.setup.darkColor = Color.fromString(dark);

				data.attachmentName = getValue(slotMap, "attachment", null);
				data.blendMode = Utils.enumValue(BlendMode, getValue(slotMap, "blend", "normal"));
				data.visible = getValue(slotMap, "visible", true);
				skeletonData.slots.push(data);
			}
		}

		// Constraints.
		if (root.constraints) {
			for (const constraintMap of root.constraints) {
				const name = constraintMap.name;
				const skinRequired = getValue(constraintMap, "skin", false);
				switch (getValue(constraintMap, "type", false)) {
					case "ik": {
						const data = new IkConstraintData(name);
						data.skinRequired = skinRequired;

						for (let ii = 0; ii < constraintMap.bones.length; ii++) {
							const bone = skeletonData.findBone(constraintMap.bones[ii]);
							if (!bone) throw new Error(`Couldn't find bone ${constraintMap.bones[ii]} for IK constraint ${name}.`);
							data.bones.push(bone);
						}

						const targetName = constraintMap.target;
						const target = skeletonData.findBone(targetName);
						if (!target) throw new Error(`Couldn't find target bone ${targetName} for IK constraint ${name}.`);
						data.target = target;

						data.uniform = getValue(constraintMap, "uniform", false);
						const setup = data.setup;
						setup.mix = getValue(constraintMap, "mix", 1);
						setup.softness = getValue(constraintMap, "softness", 0) * scale;
						setup.bendDirection = getValue(constraintMap, "bendPositive", true) ? 1 : -1;
						setup.compress = getValue(constraintMap, "compress", false);
						setup.stretch = getValue(constraintMap, "stretch", false);

						skeletonData.constraints.push(data);
						break;
					}
					case "transform": {
						const data = new TransformConstraintData(name);
						data.skinRequired = skinRequired;

						for (let ii = 0; ii < constraintMap.bones.length; ii++) {
							const boneName = constraintMap.bones[ii];
							const bone = skeletonData.findBone(boneName);
							if (!bone) throw new Error(`Couldn't find bone ${boneName} for transform constraint ${constraintMap.name}.`);
							data.bones.push(bone);
						}

						const sourceName: string = constraintMap.source;
						const source = skeletonData.findBone(sourceName);
						if (!source) throw new Error(`Couldn't find source bone ${sourceName} for transform constraint ${constraintMap.name}.`);
						data.source = source;

						data.localSource = getValue(constraintMap, "localSource", false);
						data.localTarget = getValue(constraintMap, "localTarget", false);
						data.additive = getValue(constraintMap, "additive", false);
						data.clamp = getValue(constraintMap, "clamp", false);

						let rotate = false, x = false, y = false, scaleX = false, scaleY = false, shearY = false;
						const fromEntries = Object.entries(getValue(constraintMap, "properties", {})) as [string, object][];
						for (const [name, fromEntry] of fromEntries) {
							const from = this.fromProperty(name);
							const fromScale = this.propertyScale(name, scale);
							from.offset = getValue(fromEntry, "offset", 0) * fromScale;
							const toEntries = Object.entries(getValue(fromEntry, "to", {})) as [string, object][];
							for (const [name, toEntry] of toEntries) {
								let toScale = 1;
								let to: ToProperty;
								switch (name) {
									case "rotate": {
										rotate = true;
										to = new ToRotate();
										break;
									}
									case "x": {
										x = true;
										to = new ToX();
										toScale = scale;
										break;
									}
									case "y": {
										y = true;
										to = new ToY();
										toScale = scale;
										break;
									}
									case "scaleX": {
										scaleX = true;
										to = new ToScaleX();
										break;
									}
									case "scaleY": {
										scaleY = true;
										to = new ToScaleY();
										break;
									}
									case "shearY": {
										shearY = true;
										to = new ToShearY();
										break;
									}
									default: throw new Error(`Invalid transform constraint to property: ${name}`);
								}
								to.offset = getValue(toEntry, "offset", 0) * toScale;
								to.max = getValue(toEntry, "max", 1) * toScale;
								to.scale = getValue(toEntry, "scale", 1) * toScale / fromScale;
								from.to.push(to);
							}
							if (from.to.length > 0) data.properties.push(from);
						}

						data.offsets[TransformConstraintData.ROTATION] = getValue(constraintMap, "rotation", 0);
						data.offsets[TransformConstraintData.X] = getValue(constraintMap, "x", 0) * scale;
						data.offsets[TransformConstraintData.Y] = getValue(constraintMap, "y", 0) * scale;
						data.offsets[TransformConstraintData.SCALEX] = getValue(constraintMap, "scaleX", 0);
						data.offsets[TransformConstraintData.SCALEY] = getValue(constraintMap, "scaleY", 0);
						data.offsets[TransformConstraintData.SHEARY] = getValue(constraintMap, "shearY", 0);

						const setup = data.setup;
						if (rotate) setup.mixRotate = getValue(constraintMap, "mixRotate", 1);
						if (x) setup.mixX = getValue(constraintMap, "mixX", 1);
						if (y) setup.mixY = getValue(constraintMap, "mixY", setup.mixX);
						if (scaleX) setup.mixScaleX = getValue(constraintMap, "mixScaleX", 1);
						if (scaleY) setup.mixScaleY = getValue(constraintMap, "mixScaleY", setup.mixScaleX);
						if (shearY) setup.mixShearY = getValue(constraintMap, "mixShearY", 1);

						skeletonData.constraints.push(data);
						break;
					}
					case "path": {
						const data = new PathConstraintData(name);
						data.skinRequired = skinRequired;

						for (let ii = 0; ii < constraintMap.bones.length; ii++) {
							const boneName = constraintMap.bones[ii];
							const bone = skeletonData.findBone(boneName);
							if (!bone) throw new Error(`Couldn't find bone ${boneName} for path constraint ${constraintMap.name}.`);
							data.bones.push(bone);
						}

						const slotName: string = constraintMap.slot;
						const slot = skeletonData.findSlot(slotName);
						if (!slot) throw new Error(`Couldn't find slot ${slotName} for path constraint ${constraintMap.name}.`);
						data.slot = slot;

						data.positionMode = Utils.enumValue(PositionMode, getValue(constraintMap, "positionMode", "Percent"));
						data.spacingMode = Utils.enumValue(SpacingMode, getValue(constraintMap, "spacingMode", "Length"));
						data.rotateMode = Utils.enumValue(RotateMode, getValue(constraintMap, "rotateMode", "Tangent"));
						data.offsetRotation = getValue(constraintMap, "rotation", 0);
						const setup = data.setup;
						setup.position = getValue(constraintMap, "position", 0);
						if (data.positionMode === PositionMode.Fixed) setup.position *= scale;
						setup.spacing = getValue(constraintMap, "spacing", 0);
						if (data.spacingMode === SpacingMode.Length || data.spacingMode === SpacingMode.Fixed) setup.spacing *= scale;
						setup.mixRotate = getValue(constraintMap, "mixRotate", 1);
						setup.mixX = getValue(constraintMap, "mixX", 1);
						setup.mixY = getValue(constraintMap, "mixY", setup.mixX);

						skeletonData.constraints.push(data);
						break;
					}
					case "physics": {
						const data = new PhysicsConstraintData(name);
						data.skinRequired = skinRequired;

						const boneName: string = constraintMap.bone;
						const bone = skeletonData.findBone(boneName);
						if (bone == null) throw new Error(`Physics bone not found: ${boneName}`);
						data.bone = bone;

						data.x = getValue(constraintMap, "x", 0);
						data.y = getValue(constraintMap, "y", 0);
						data.rotate = getValue(constraintMap, "rotate", 0);
						data.scaleX = getValue(constraintMap, "scaleX", 0);
						data.shearX = getValue(constraintMap, "shearX", 0);
						data.limit = getValue(constraintMap, "limit", 5000) * scale;
						data.step = 1 / getValue(constraintMap, "fps", 60);
						const setup = data.setup;
						setup.inertia = getValue(constraintMap, "inertia", 0.5);
						setup.strength = getValue(constraintMap, "strength", 100);
						setup.damping = getValue(constraintMap, "damping", 0.85);
						setup.massInverse = 1 / getValue(constraintMap, "mass", 1);
						setup.wind = getValue(constraintMap, "wind", 0);
						setup.gravity = getValue(constraintMap, "gravity", 0);
						setup.mix = getValue(constraintMap, "mix", 1);
						data.inertiaGlobal = getValue(constraintMap, "inertiaGlobal", false);
						data.strengthGlobal = getValue(constraintMap, "strengthGlobal", false);
						data.dampingGlobal = getValue(constraintMap, "dampingGlobal", false);
						data.massGlobal = getValue(constraintMap, "massGlobal", false);
						data.windGlobal = getValue(constraintMap, "windGlobal", false);
						data.gravityGlobal = getValue(constraintMap, "gravityGlobal", false);
						data.mixGlobal = getValue(constraintMap, "mixGlobal", false);

						skeletonData.constraints.push(data);
						break;
					}
					case "slider": {
						const data = new SliderData(name);
						data.skinRequired = skinRequired;

						data.additive = getValue(constraintMap, "additive", false);
						data.loop = getValue(constraintMap, "loop", false);
						data.setup.time = getValue(constraintMap, "time", 0);
						data.setup.mix = getValue(constraintMap, "mix", 1);

						const boneName: string = constraintMap.bone;
						if (boneName) {
							data.bone = skeletonData.findBone(boneName);
							if (!data.bone) throw new Error(`Slider bone not found: ${boneName}`);
							const property = constraintMap.property;
							data.property = this.fromProperty(property);
							const propertyScale = this.propertyScale(property, scale);
							data.property.offset = getValue(constraintMap, "from", 0) * propertyScale;
							data.offset = getValue(constraintMap, "to", 0);
							data.scale = getValue(constraintMap, "scale", 1) / propertyScale;
							data.local = getValue(constraintMap, "local", false);
						}

						skeletonData.constraints.push(data);
						break;
					}
				}
			}
		}

		// Skins.
		if (root.skins) {
			for (let i = 0; i < root.skins.length; i++) {
				const skinMap = root.skins[i]
				const skin = new Skin(skinMap.name);

				if (skinMap.bones) {
					for (let ii = 0; ii < skinMap.bones.length; ii++) {
						const boneName = skinMap.bones[ii];
						const bone = skeletonData.findBone(boneName);
						if (!bone) throw new Error(`Couldn't find bone ${boneName} for skin ${skinMap.name}.`);
						skin.bones.push(bone);
					}
				}

				if (skinMap.ik) {
					for (let ii = 0; ii < skinMap.ik.length; ii++) {
						const constraintName = skinMap.ik[ii];
						const constraint = skeletonData.findConstraint(constraintName, IkConstraintData);
						if (!constraint) throw new Error(`Couldn't find IK constraint ${constraintName} for skin ${skinMap.name}.`);
						skin.constraints.push(constraint);
					}
				}

				if (skinMap.transform) {
					for (let ii = 0; ii < skinMap.transform.length; ii++) {
						const constraintName = skinMap.transform[ii];
						const constraint = skeletonData.findConstraint(constraintName, TransformConstraintData);
						if (!constraint) throw new Error(`Couldn't find transform constraint ${constraintName} for skin ${skinMap.name}.`);
						skin.constraints.push(constraint);
					}
				}

				if (skinMap.path) {
					for (let ii = 0; ii < skinMap.path.length; ii++) {
						const constraintName = skinMap.path[ii];
						const constraint = skeletonData.findConstraint(constraintName, PathConstraintData);
						if (!constraint) throw new Error(`Couldn't find path constraint ${constraintName} for skin ${skinMap.name}.`);
						skin.constraints.push(constraint);
					}
				}

				if (skinMap.physics) {
					for (let ii = 0; ii < skinMap.physics.length; ii++) {
						const constraintName = skinMap.physics[ii];
						const constraint = skeletonData.findConstraint(constraintName, PhysicsConstraintData);
						if (!constraint) throw new Error(`Couldn't find physics constraint ${constraintName} for skin ${skinMap.name}.`);
						skin.constraints.push(constraint);
					}
				}

				if (skinMap.slider) {
					for (let ii = 0; ii < skinMap.slider.length; ii++) {
						const constraintName = skinMap.slider[ii];
						const constraint = skeletonData.findConstraint(constraintName, SliderData);
						if (!constraint) throw new Error(`Couldn't find slider constraint ${constraintName} for skin ${skinMap.name}.`);
						skin.constraints.push(constraint);
					}
				}

				for (const slotName in skinMap.attachments) {
					const slot = skeletonData.findSlot(slotName);
					if (!slot) throw new Error(`Couldn't find slot ${slotName} for skin ${skinMap.name}.`);
					const slotMap = skinMap.attachments[slotName];
					for (const entryName in slotMap) {
						const attachment = this.readAttachment(slotMap[entryName], skin, slot.index, entryName, skeletonData);
						if (attachment) skin.setAttachment(slot.index, entryName, attachment);
					}
				}
				skeletonData.skins.push(skin);
				if (skin.name === "default") skeletonData.defaultSkin = skin;
			}
		}

		// Linked meshes.
		for (let i = 0, n = this.linkedMeshes.length; i < n; i++) {
			const linkedMesh = this.linkedMeshes[i];
			const skin = !linkedMesh.skin ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
			if (!skin) throw new Error(`Skin not found: ${linkedMesh.skin}`);
			const parent = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
			if (!parent) throw new Error(`Parent mesh not found: ${linkedMesh.parent}`);
			linkedMesh.mesh.timelineAttachment = linkedMesh.inheritTimeline ? <VertexAttachment>parent : <VertexAttachment>linkedMesh.mesh;
			linkedMesh.mesh.setParentMesh(<MeshAttachment>parent);
			if (linkedMesh.mesh.region != null) linkedMesh.mesh.updateRegion();
		}
		this.linkedMeshes.length = 0;

		// Events.
		if (root.events) {
			for (const eventName in root.events) {
				const eventMap = root.events[eventName];
				const data = new EventData(eventName);
				data.intValue = getValue(eventMap, "int", 0);
				data.floatValue = getValue(eventMap, "float", 0);
				data.stringValue = getValue(eventMap, "string", "");
				data.audioPath = getValue(eventMap, "audio", null);
				if (data.audioPath) {
					data.volume = getValue(eventMap, "volume", 1);
					data.balance = getValue(eventMap, "balance", 0);
				}
				skeletonData.events.push(data);
			}
		}

		// Animations.
		if (root.animations) {
			for (const animationName in root.animations) {
				const animationMap = root.animations[animationName];
				this.readAnimation(animationMap, animationName, skeletonData);
			}
		}

		// Slider animations.
		if (root.constraints) {
			for (const animationName in root.constraints) {
				const animationMap = root.constraints[animationName];
				if (animationMap.type === "slider") {
					const data = skeletonData.findConstraint(animationMap.name, SliderData)
					const animationName = animationMap.animation;
					const animation = skeletonData.findAnimation(animationName);
					if (!animation) throw new Error(`Slider animation not found: ${animationName}`);
					// biome-ignore lint/style/noNonNullAssertion: reference runtime
					data!.animation = animation;
				}
			}
		}

		return skeletonData;
	}

	private fromProperty (type: string): FromProperty {
		let from: FromProperty;
		switch (type) {
			case "rotate": from = new FromRotate(); break;
			case "x": from = new FromX(); break;
			case "y": from = new FromY(); break;
			case "scaleX": from = new FromScaleX(); break;
			case "scaleY": from = new FromScaleY(); break;
			case "shearY": from = new FromShearY(); break;
			default: throw new Error(`Invalid transform constraint from property: ${type}`);
		}
		return from;
	}

	private propertyScale (type: string, scale: number) {
		switch (type) {
			case "x":
			case "y": return scale;
			default: return 1;
		}
	}

	// biome-ignore lint/suspicious/noExplicitAny: it is any until we define a schema
	readAttachment (map: any, skin: Skin, slotIndex: number, name: string, skeletonData: SkeletonData): Attachment | null {
		const scale = this.scale;
		name = getValue(map, "name", name);

		switch (getValue(map, "type", "region")) {
			case "region": {
				const path = getValue(map, "path", name);
				const sequence = this.readSequence(getValue(map, "sequence", null));
				const region = this.attachmentLoader.newRegionAttachment(skin, name, path, sequence);
				if (!region) return null;
				region.path = path;
				region.x = getValue(map, "x", 0) * scale;
				region.y = getValue(map, "y", 0) * scale;
				region.scaleX = getValue(map, "scaleX", 1);
				region.scaleY = getValue(map, "scaleY", 1);
				region.rotation = getValue(map, "rotation", 0);
				region.width = map.width * scale;
				region.height = map.height * scale;
				region.sequence = sequence;

				const color: string = getValue(map, "color", null);
				if (color) region.color.setFromString(color);

				if (region.region != null) region.updateRegion();
				return region;
			}
			case "boundingbox": {
				const box = this.attachmentLoader.newBoundingBoxAttachment(skin, name);
				if (!box) return null;
				this.readVertices(map, box, map.vertexCount << 1);
				const color: string = getValue(map, "color", null);
				if (color) box.color.setFromString(color);
				return box;
			}
			case "mesh":
			case "linkedmesh": {
				const path = getValue(map, "path", name);
				const sequence = this.readSequence(getValue(map, "sequence", null));
				const mesh = this.attachmentLoader.newMeshAttachment(skin, name, path, sequence);
				if (!mesh) return null;
				mesh.path = path;

				const color = getValue(map, "color", null);
				if (color) mesh.color.setFromString(color);

				mesh.width = getValue(map, "width", 0) * scale;
				mesh.height = getValue(map, "height", 0) * scale;
				mesh.sequence = sequence;

				const parent: string = getValue(map, "parent", null);
				if (parent) {
					this.linkedMeshes.push(new LinkedMesh(mesh, <string>getValue(map, "skin", null), slotIndex, parent, getValue(map, "timelines", true)));
					return mesh;
				}

				const uvs: Array<number> = map.uvs;
				this.readVertices(map, mesh, uvs.length);
				mesh.triangles = map.triangles;
				mesh.regionUVs = uvs;
				if (mesh.region != null) mesh.updateRegion();

				mesh.edges = getValue(map, "edges", null);
				mesh.hullLength = getValue(map, "hull", 0) * 2;
				return mesh;
			}
			case "path": {
				const path = this.attachmentLoader.newPathAttachment(skin, name);
				if (!path) return null;
				path.closed = getValue(map, "closed", false);
				path.constantSpeed = getValue(map, "constantSpeed", true);

				const vertexCount = map.vertexCount;
				this.readVertices(map, path, vertexCount << 1);

				const lengths: Array<number> = Utils.newArray(vertexCount / 3, 0);
				for (let i = 0; i < map.lengths.length; i++)
					lengths[i] = map.lengths[i] * scale;
				path.lengths = lengths;

				const color: string = getValue(map, "color", null);
				if (color) path.color.setFromString(color);
				return path;
			}
			case "point": {
				const point = this.attachmentLoader.newPointAttachment(skin, name);
				if (!point) return null;
				point.x = getValue(map, "x", 0) * scale;
				point.y = getValue(map, "y", 0) * scale;
				point.rotation = getValue(map, "rotation", 0);

				const color = getValue(map, "color", null);
				if (color) point.color.setFromString(color);
				return point;
			}
			case "clipping": {
				const clip = this.attachmentLoader.newClippingAttachment(skin, name);
				if (!clip) return null;

				const end = getValue(map, "end", null);
				if (end) clip.endSlot = skeletonData.findSlot(end);

				const vertexCount = map.vertexCount;
				this.readVertices(map, clip, vertexCount << 1);

				const color: string = getValue(map, "color", null);
				if (color) clip.color.setFromString(color);
				return clip;
			}
		}
		return null;
	}

	readSequence (map: object) {
		if (map == null) return null;
		const sequence = new Sequence(getValue(map, "count", 0));
		sequence.start = getValue(map, "start", 1);
		sequence.digits = getValue(map, "digits", 0);
		sequence.setupIndex = getValue(map, "setup", 0);
		return sequence;
	}

	// biome-ignore lint/suspicious/noExplicitAny: it is any until we define a schema
	readVertices (map: any, attachment: VertexAttachment, verticesLength: number) {
		const scale = this.scale;
		attachment.worldVerticesLength = verticesLength;
		const vertices: Array<number> = map.vertices;
		if (verticesLength === vertices.length) {
			const scaledVertices = Utils.toFloatArray(vertices);
			if (scale !== 1) {
				for (let i = 0, n = vertices.length; i < n; i++)
					scaledVertices[i] *= scale;
			}
			attachment.vertices = scaledVertices;
			return;
		}
		const weights: number[] = [];
		const bones: number[] = [];
		for (let i = 0, n = vertices.length; i < n;) {
			const boneCount = vertices[i++];
			bones.push(boneCount);
			for (let nn = i + boneCount * 4; i < nn; i += 4) {
				bones.push(vertices[i]);
				weights.push(vertices[i + 1] * scale);
				weights.push(vertices[i + 2] * scale);
				weights.push(vertices[i + 3]);
			}
		}
		attachment.bones = bones;
		attachment.vertices = Utils.toFloatArray(weights);
	}

	// biome-ignore lint/suspicious/noExplicitAny: it is any untile we define a schema
	readAnimation (map: any, name: string, skeletonData: SkeletonData) {
		const scale = this.scale;
		const timelines: Timeline[] = [];

		// Slot timelines.
		if (map.slots) {
			for (const slotName in map.slots) {
				const slotMap = map.slots[slotName];
				const slot = skeletonData.findSlot(slotName);
				if (!slot) throw new Error(`Slot not found: ${slotName}`);
				const slotIndex = slot.index;
				for (const timelineName in slotMap) {
					const timelineMap = slotMap[timelineName];
					if (!timelineMap) continue;
					const frames = timelineMap.length;

					switch (timelineName) {
						case "attachment": {
							const timeline = new AttachmentTimeline(frames, slotIndex);
							for (let frame = 0; frame < frames; frame++) {
								const keyMap = timelineMap[frame];
								timeline.setFrame(frame, getValue(keyMap, "time", 0), getValue(keyMap, "name", null));
							}
							timelines.push(timeline);
							break;
						}
						case "rgba": {
							const timeline = new RGBATimeline(frames, frames << 2, slotIndex);
							let keyMap = timelineMap[0];
							let time = getValue(keyMap, "time", 0);
							let color = Color.fromString(keyMap.color);

							for (let frame = 0, bezier = 0; ; frame++) {
								timeline.setFrame(frame, time, color.r, color.g, color.b, color.a);
								const nextMap = timelineMap[frame + 1];
								if (!nextMap) {
									timeline.shrink(bezier);
									break;
								}
								const time2 = getValue(nextMap, "time", 0);
								const newColor = Color.fromString(nextMap.color);
								const curve = keyMap.curve;
								if (curve) {
									bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, color.a, newColor.a, 1);
								}
								time = time2;
								color = newColor;
								keyMap = nextMap;
							}

							timelines.push(timeline);
							break;
						}
						case "rgb": {
							const timeline = new RGBTimeline(frames, frames * 3, slotIndex);
							let keyMap = timelineMap[0];
							let time = getValue(keyMap, "time", 0);
							let color = Color.fromString(keyMap.color);

							for (let frame = 0, bezier = 0; ; frame++) {
								timeline.setFrame(frame, time, color.r, color.g, color.b);
								const nextMap = timelineMap[frame + 1];
								if (!nextMap) {
									timeline.shrink(bezier);
									break;
								}
								const time2 = getValue(nextMap, "time", 0);
								const newColor = Color.fromString(nextMap.color);
								const curve = keyMap.curve;
								if (curve) {
									bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
								}
								time = time2;
								color = newColor;
								keyMap = nextMap;
							}

							timelines.push(timeline);
							break;
						}
						case "alpha": {
							readTimeline1(timelines, timelineMap, new AlphaTimeline(frames, frames, slotIndex), 0, 1);
							break;
						}
						case "rgba2": {
							const timeline = new RGBA2Timeline(frames, frames * 7, slotIndex);

							let keyMap = timelineMap[0];
							let time = getValue(keyMap, "time", 0);
							let color = Color.fromString(keyMap.light);
							let color2 = Color.fromString(keyMap.dark);

							for (let frame = 0, bezier = 0; ; frame++) {
								timeline.setFrame(frame, time, color.r, color.g, color.b, color.a, color2.r, color2.g, color2.b);
								const nextMap = timelineMap[frame + 1];
								if (!nextMap) {
									timeline.shrink(bezier);
									break;
								}
								const time2 = getValue(nextMap, "time", 0);
								const newColor = Color.fromString(nextMap.light);
								const newColor2 = Color.fromString(nextMap.dark);
								const curve = keyMap.curve;
								if (curve) {
									bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, color.a, newColor.a, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, color2.r, newColor2.r, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, color2.g, newColor2.g, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 6, time, time2, color2.b, newColor2.b, 1);
								}
								time = time2;
								color = newColor;
								color2 = newColor2;
								keyMap = nextMap;
							}

							timelines.push(timeline);
							break;
						}
						case "rgb2": {
							const timeline = new RGB2Timeline(frames, frames * 6, slotIndex);

							let keyMap = timelineMap[0];
							let time = getValue(keyMap, "time", 0);
							let color = Color.fromString(keyMap.light);
							let color2 = Color.fromString(keyMap.dark);

							for (let frame = 0, bezier = 0; ; frame++) {
								timeline.setFrame(frame, time, color.r, color.g, color.b, color2.r, color2.g, color2.b);
								const nextMap = timelineMap[frame + 1];
								if (!nextMap) {
									timeline.shrink(bezier);
									break;
								}
								const time2 = getValue(nextMap, "time", 0);
								const newColor = Color.fromString(nextMap.light);
								const newColor2 = Color.fromString(nextMap.dark);
								const curve = keyMap.curve;
								if (curve) {
									bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, color2.r, newColor2.r, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, color2.g, newColor2.g, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, color2.b, newColor2.b, 1);
								}
								time = time2;
								color = newColor;
								color2 = newColor2;
								keyMap = nextMap;
							}

							timelines.push(timeline);
							break;
						}
						default:
							throw new Error(`Invalid timeline type for a slot: ${timelineMap.name} (${slotMap.name})`);
					}
				}
			}
		}

		// Bone timelines.
		if (map.bones) {
			for (const boneName in map.bones) {
				const boneMap = map.bones[boneName];
				const bone = skeletonData.findBone(boneName);
				if (!bone) throw new Error(`Bone not found: ${boneName}`);
				const boneIndex = bone.index;
				for (const timelineName in boneMap) {
					const timelineMap = boneMap[timelineName];
					const frames = timelineMap.length;
					if (frames === 0) continue;

					switch (timelineName) {
						case "rotate": readTimeline1(timelines, timelineMap, new RotateTimeline(frames, frames, boneIndex), 0, 1); break;
						case "translate": readTimeline2(timelines, timelineMap, new TranslateTimeline(frames, frames << 1, boneIndex), "x", "y", 0, scale); break;
						case "translatex": readTimeline1(timelines, timelineMap, new TranslateXTimeline(frames, frames, boneIndex), 0, scale); break;
						case "translatey": readTimeline1(timelines, timelineMap, new TranslateYTimeline(frames, frames, boneIndex), 0, scale); break;
						case "scale": readTimeline2(timelines, timelineMap, new ScaleTimeline(frames, frames << 1, boneIndex), "x", "y", 1, 1); break;
						case "scalex": readTimeline1(timelines, timelineMap, new ScaleXTimeline(frames, frames, boneIndex), 1, 1); break;
						case "scaley": readTimeline1(timelines, timelineMap, new ScaleYTimeline(frames, frames, boneIndex), 1, 1); break;
						case "shear": readTimeline2(timelines, timelineMap, new ShearTimeline(frames, frames << 1, boneIndex), "x", "y", 0, 1); break;
						case "shearx": readTimeline1(timelines, timelineMap, new ShearXTimeline(frames, frames, boneIndex), 0, 1); break;
						case "sheary": readTimeline1(timelines, timelineMap, new ShearYTimeline(frames, frames, boneIndex), 0, 1); break;
						case "inherit": {
							const timeline = new InheritTimeline(frames, bone.index);
							for (let frame = 0; frame < timelineMap.length; frame++) {
								const aFrame = timelineMap[frame];
								timeline.setFrame(frame, getValue(aFrame, "time", 0), Utils.enumValue(Inherit, getValue(aFrame, "inherit", "Normal")));
							}
							timelines.push(timeline);
							break;
						}
						default:
							throw new Error(`Invalid timeline type for a bone: ${timelineMap.name} (${boneMap.name})`);
					}

				}
			}
		}

		// IK constraint timelines.
		if (map.ik) {
			for (const constraintName in map.ik) {
				const constraintMap = map.ik[constraintName];
				let keyMap = constraintMap[0];
				if (!keyMap) continue;

				const constraint = skeletonData.findConstraint(constraintName, IkConstraintData);
				if (!constraint) throw new Error(`IK Constraint not found: ${constraintName}`);
				const timeline = new IkConstraintTimeline(constraintMap.length, constraintMap.length << 1,
					skeletonData.constraints.indexOf(constraint));

				let time = getValue(keyMap, "time", 0);
				let mix = getValue(keyMap, "mix", 1);
				let softness = getValue(keyMap, "softness", 0) * scale;

				for (let frame = 0, bezier = 0; ; frame++) {
					timeline.setFrame(frame, time, mix, softness, getValue(keyMap, "bendPositive", true) ? 1 : -1, getValue(keyMap, "compress", false), getValue(keyMap, "stretch", false));
					const nextMap = constraintMap[frame + 1];
					if (!nextMap) {
						timeline.shrink(bezier);
						break;
					}

					const time2 = getValue(nextMap, "time", 0);
					const mix2 = getValue(nextMap, "mix", 1);
					const softness2 = getValue(nextMap, "softness", 0) * scale;
					const curve = keyMap.curve;
					if (curve) {
						bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mix, mix2, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, softness, softness2, scale);
					}

					time = time2;
					mix = mix2;
					softness = softness2;
					keyMap = nextMap;
				}
				timelines.push(timeline);
			}
		}

		// Transform constraint timelines.
		if (map.transform) {
			for (const constraintName in map.transform) {
				const timelineMap = map.transform[constraintName];
				let keyMap = timelineMap[0];
				if (!keyMap) continue;

				const constraint = skeletonData.findConstraint(constraintName, TransformConstraintData);
				if (!constraint) throw new Error(`Transform constraint not found: ${constraintName}`);
				const timeline = new TransformConstraintTimeline(timelineMap.length, timelineMap.length * 6,
					skeletonData.constraints.indexOf(constraint));

				let time = getValue(keyMap, "time", 0);
				let mixRotate = getValue(keyMap, "mixRotate", 1);
				let mixX = getValue(keyMap, "mixX", 1), mixY = getValue(keyMap, "mixY", mixX);
				let mixScaleX = getValue(keyMap, "mixScaleX", 1), mixScaleY = getValue(keyMap, "mixScaleY", 1);
				const mixShearY = getValue(keyMap, "mixShearY", 1);

				for (let frame = 0, bezier = 0; ; frame++) {
					timeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
					const nextMap = timelineMap[frame + 1];
					if (!nextMap) {
						timeline.shrink(bezier);
						break;
					}

					const time2 = getValue(nextMap, "time", 0);
					const mixRotate2 = getValue(nextMap, "mixRotate", 1);
					const mixX2 = getValue(nextMap, "mixX", 1), mixY2 = getValue(nextMap, "mixY", mixX2);
					const mixScaleX2 = getValue(nextMap, "mixScaleX", 1), mixScaleY2 = getValue(nextMap, "mixScaleY", 1);
					const mixShearY2 = getValue(nextMap, "mixShearY", 1);
					const curve = keyMap.curve;
					if (curve) {
						bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
						bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, mixShearY, mixShearY2, 1);
					}

					time = time2;
					mixRotate = mixRotate2;
					mixX = mixX2;
					mixY = mixY2;
					mixScaleX = mixScaleX2;
					mixScaleY = mixScaleY2;
					mixScaleX = mixScaleX2;
					keyMap = nextMap;
				}
				timelines.push(timeline);
			}
		}

		// Path constraint timelines.
		if (map.path) {
			for (const constraintName in map.path) {
				const constraintMap = map.path[constraintName];
				const constraint = skeletonData.findConstraint(constraintName, PathConstraintData);
				if (!constraint) throw new Error(`Path constraint not found: ${constraintName}`);
				const index = skeletonData.constraints.indexOf(constraint);
				for (const timelineName in constraintMap) {
					const timelineMap = constraintMap[timelineName];
					let keyMap = timelineMap[0];
					if (!keyMap) continue;

					const frames = timelineMap.length;
					switch (timelineName) {
						case "position": {
							const timeline = new PathConstraintPositionTimeline(frames, frames, index);
							readTimeline1(timelines, timelineMap, timeline, 0, constraint.positionMode === PositionMode.Fixed ? scale : 1);
							break;
						}
						case "spacing": {
							const timeline = new PathConstraintSpacingTimeline(frames, frames, index);
							readTimeline1(timelines, timelineMap, timeline, 0, constraint.spacingMode === SpacingMode.Length || constraint.spacingMode === SpacingMode.Fixed ? scale : 1);
							break;
						}
						case "mix": {
							const timeline = new PathConstraintMixTimeline(frames, frames * 3, index);
							let time = getValue(keyMap, "time", 0);
							let mixRotate = getValue(keyMap, "mixRotate", 1);
							let mixX = getValue(keyMap, "mixX", 1);
							let mixY = getValue(keyMap, "mixY", mixX);
							for (let frame = 0, bezier = 0; ; frame++) {
								timeline.setFrame(frame, time, mixRotate, mixX, mixY);
								const nextMap = timelineMap[frame + 1];
								if (!nextMap) {
									timeline.shrink(bezier);
									break;
								}
								const time2 = getValue(nextMap, "time", 0);
								const mixRotate2 = getValue(nextMap, "mixRotate", 1);
								const mixX2 = getValue(nextMap, "mixX", 1);
								const mixY2 = getValue(nextMap, "mixY", mixX2);
								const curve = keyMap.curve;
								if (curve) {
									bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
									bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
								}
								time = time2;
								mixRotate = mixRotate2;
								mixX = mixX2;
								mixY = mixY2;
								keyMap = nextMap;
							}
							timelines.push(timeline);
							break;
						}
					}
				}
			}
		}

		// Physics constraint timelines.
		if (map.physics) {
			for (const constraintName in map.physics) {
				const constraintMap = map.physics[constraintName];
				let index = -1;
				if (constraintName.length > 0) {
					const constraint = skeletonData.findConstraint(constraintName, PhysicsConstraintData);
					if (!constraint) throw new Error(`Physics constraint not found: ${constraintName}`);
					index = skeletonData.constraints.indexOf(constraint);
				}
				for (const timelineName in constraintMap) {
					const timelineMap = constraintMap[timelineName];
					let keyMap = timelineMap[0];
					if (!keyMap) continue;

					const frames = timelineMap.length;
					let timeline: CurveTimeline1;
					let defaultValue = 0;
					if (timelineName === "reset") {
						const resetTimeline = new PhysicsConstraintResetTimeline(frames, index);
						for (let frame = 0; keyMap != null; keyMap = timelineMap[frame + 1], frame++)
							resetTimeline.setFrame(frame, getValue(keyMap, "time", 0));
						timelines.push(resetTimeline);
						continue;
					}
					switch (timelineName) {
						case "inertia": timeline = new PhysicsConstraintInertiaTimeline(frames, frames, index); break;
						case "strength": timeline = new PhysicsConstraintStrengthTimeline(frames, frames, index); break;
						case "damping": timeline = new PhysicsConstraintDampingTimeline(frames, frames, index); break;
						case "mass": timeline = new PhysicsConstraintMassTimeline(frames, frames, index); break;
						case "wind": timeline = new PhysicsConstraintWindTimeline(frames, frames, index); break;
						case "gravity": timeline = new PhysicsConstraintGravityTimeline(frames, frames, index); break;
						case "mix": {
							defaultValue = 1;
							timeline = new PhysicsConstraintMixTimeline(frames, frames, index);
							break;
						}
						default: continue;
					}
					readTimeline1(timelines, timelineMap, timeline, defaultValue, 1);
				}
			}
		}

		// Slider timelines.
		if (map.slider) {
			for (const constraintName in map.slider) {
				const constraintMap = map.slider[constraintName];
				const constraint = skeletonData.findConstraint(constraintName, SliderData);
				if (!constraint) throw new Error(`Slider not found: ${constraintName}`);
				const index = skeletonData.constraints.indexOf(constraint);

				for (const timelineName in constraintMap) {
					const timelineMap = constraintMap[timelineName];
					const keyMap = timelineMap[0];
					if (!keyMap) continue;

					const frames = timelineMap.length;
					switch (timelineName) {
						case "time": readTimeline1(timelines, timelineMap, new SliderTimeline(frames, frames, index), 1, 1); break;
						case "mix": readTimeline1(timelines, timelineMap, new SliderMixTimeline(frames, frames, index), 1, 1); break;
					}
				}
			}
		}

		// Attachment timelines.
		if (map.attachments) {
			for (const attachmentsName in map.attachments) {
				const attachmentsMap = map.attachments[attachmentsName];
				const skin = skeletonData.findSkin(attachmentsName);
				if (!skin) throw new Error(`Skin not found: ${attachmentsName}`);
				for (const slotMapName in attachmentsMap) {
					const slotMap = attachmentsMap[slotMapName];
					const slot = skeletonData.findSlot(slotMapName);
					if (!slot) throw new Error(`Slot not found: ${slotMapName}`);
					const slotIndex = slot.index;
					for (const attachmentMapName in slotMap) {
						const attachmentMap = slotMap[attachmentMapName];
						const attachment = <VertexAttachment>skin.getAttachment(slotIndex, attachmentMapName);
						if (!attachment) throw new Error(`Timeline attachment not found: ${attachmentMapName}`);

						for (const timelineMapName in attachmentMap) {
							const timelineMap = attachmentMap[timelineMapName];
							let keyMap = timelineMap[0];
							if (!keyMap) continue;

							if (timelineMapName === "deform") {
								const weighted = attachment.bones;
								const vertices = attachment.vertices;
								const deformLength = weighted ? vertices.length / 3 * 2 : vertices.length;

								const timeline = new DeformTimeline(timelineMap.length, timelineMap.length, slotIndex, attachment);
								let time = getValue(keyMap, "time", 0);
								for (let frame = 0, bezier = 0; ; frame++) {
									let deform: NumberArrayLike;
									const verticesValue: Array<number> = getValue(keyMap, "vertices", null);
									if (!verticesValue)
										deform = weighted ? Utils.newFloatArray(deformLength) : vertices;
									else {
										deform = Utils.newFloatArray(deformLength);
										const start = <number>getValue(keyMap, "offset", 0);
										Utils.arrayCopy(verticesValue, 0, deform, start, verticesValue.length);
										if (scale !== 1) {
											for (let i = start, n = i + verticesValue.length; i < n; i++)
												deform[i] *= scale;
										}
										if (!weighted) {
											for (let i = 0; i < deformLength; i++)
												deform[i] += vertices[i];
										}
									}

									timeline.setFrame(frame, time, deform);
									const nextMap = timelineMap[frame + 1];
									if (!nextMap) {
										timeline.shrink(bezier);
										break;
									}
									const time2 = getValue(nextMap, "time", 0);
									const curve = keyMap.curve;
									if (curve) bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, 0, 1, 1);
									time = time2;
									keyMap = nextMap;
								}
								timelines.push(timeline);
							} else if (timelineMapName === "sequence") {
								const timeline = new SequenceTimeline(timelineMap.length, slotIndex, attachment as unknown as HasTextureRegion);
								let lastDelay = 0;
								for (let frame = 0; frame < timelineMap.length; frame++) {
									const delay = getValue(keyMap, "delay", lastDelay);
									const time = getValue(keyMap, "time", 0);
									const mode = SequenceMode[getValue(keyMap, "mode", "hold")] as unknown as number;
									const index = getValue(keyMap, "index", 0);
									timeline.setFrame(frame, time, mode, index, delay);
									lastDelay = delay;
									keyMap = timelineMap[frame + 1];
								}
								timelines.push(timeline);
							}
						}
					}
				}
			}
		}

		// Draw order timelines.
		if (map.drawOrder) {
			const timeline = new DrawOrderTimeline(map.drawOrder.length);
			const slotCount = skeletonData.slots.length;
			let frame = 0;
			for (let i = 0; i < map.drawOrder.length; i++, frame++) {
				const drawOrderMap = map.drawOrder[i];
				let drawOrder: Array<number> | null = null;
				const offsets = getValue(drawOrderMap, "offsets", null);
				if (offsets) {
					drawOrder = Utils.newArray<number>(slotCount, -1);
					const unchanged = Utils.newArray<number>(slotCount - offsets.length, 0);
					let originalIndex = 0, unchangedIndex = 0;
					for (let ii = 0; ii < offsets.length; ii++) {
						const offsetMap = offsets[ii];
						const slot = skeletonData.findSlot(offsetMap.slot);
						if (!slot) throw new Error(`Slot not found: ${slot}`);
						const slotIndex = slot.index;
						// Collect unchanged items.
						while (originalIndex !== slotIndex)
							unchanged[unchangedIndex++] = originalIndex++;
						// Set changed items.
						drawOrder[originalIndex + offsetMap.offset] = originalIndex++;
					}
					// Collect remaining unchanged items.
					while (originalIndex < slotCount)
						unchanged[unchangedIndex++] = originalIndex++;
					// Fill in unchanged items.
					for (let ii = slotCount - 1; ii >= 0; ii--)
						if (drawOrder[ii] === -1) drawOrder[ii] = unchanged[--unchangedIndex];
				}
				timeline.setFrame(frame, getValue(drawOrderMap, "time", 0), drawOrder);
			}
			timelines.push(timeline);
		}

		// Event timelines.
		if (map.events) {
			const timeline = new EventTimeline(map.events.length);
			let frame = 0;
			for (let i = 0; i < map.events.length; i++, frame++) {
				const eventMap = map.events[i];
				const eventData = skeletonData.findEvent(eventMap.name);
				if (!eventData) throw new Error(`Event not found: ${eventMap.name}`);
				const event = new Event(Utils.toSinglePrecision(getValue(eventMap, "time", 0)), eventData);
				event.intValue = getValue(eventMap, "int", eventData.intValue);
				event.floatValue = getValue(eventMap, "float", eventData.floatValue);
				event.stringValue = getValue(eventMap, "string", eventData.stringValue);
				if (event.data.audioPath) {
					event.volume = getValue(eventMap, "volume", 1);
					event.balance = getValue(eventMap, "balance", 0);
				}
				timeline.setFrame(frame, event);
			}
			timelines.push(timeline);
		}

		let duration = 0;
		for (let i = 0, n = timelines.length; i < n; i++)
			duration = Math.max(duration, timelines[i].getDuration());
		skeletonData.animations.push(new Animation(name, timelines, duration));
	}
}

class LinkedMesh {
	parent: string; skin: string;
	slotIndex: number;
	mesh: MeshAttachment;
	inheritTimeline: boolean;

	constructor (mesh: MeshAttachment, skin: string, slotIndex: number, parent: string, inheritDeform: boolean) {
		this.mesh = mesh;
		this.skin = skin;
		this.slotIndex = slotIndex;
		this.parent = parent;
		this.inheritTimeline = inheritDeform;
	}
}

type CurveType = [number, number, number, number] | "stepped";
type Timeline1KeysType = { value: number, time?: number, curve?: CurveType };
type Timeline2KeysType = Timeline1KeysType & { x?: number, y?: number };

function readTimeline1 (timelines: Array<Timeline>, keys: Timeline1KeysType[], timeline: CurveTimeline1, defaultValue: number, scale: number) {
	let keyMap = keys[0];
	let time = keyMap.time ?? 0;
	let value = (keyMap.value ?? defaultValue) * scale;
	let bezier = 0;

	for (let frame = 0; ; frame++) {
		timeline.setFrame(frame, time, value);
		const nextMap = keys[frame + 1];
		if (!nextMap) {
			timeline.shrink(bezier);
			timelines.push(timeline);
			return;
		}
		const time2 = nextMap.time ?? 0;
		const value2 = (nextMap.value ?? defaultValue) * scale;
		if (keyMap.curve) bezier = readCurve(keyMap.curve, timeline, bezier, frame, 0, time, time2, value, value2, scale);
		time = time2;
		value = value2;
		keyMap = nextMap;
	}
}

function readTimeline2 (timelines: Array<Timeline>, keys: Timeline2KeysType[], timeline: BoneTimeline2, name1: "x", name2: "y", defaultValue: number, scale: number) {
	let keyMap = keys[0];
	let time = keyMap.time ?? 0;
	let value1 = (keyMap[name1] ?? defaultValue) * scale;
	let value2 = (keyMap[name2] ?? defaultValue) * scale;
	let bezier = 0;
	for (let frame = 0; ; frame++) {
		timeline.setFrame(frame, time, value1, value2);
		const nextMap = keys[frame + 1];
		if (!nextMap) {
			timeline.shrink(bezier);
			timelines.push(timeline);
			return;
		}
		const time2 = nextMap.time ?? 0;
		const nvalue1 = (nextMap[name1] ?? defaultValue) * scale;
		const nvalue2 = (nextMap[name2] ?? defaultValue) * scale;
		const curve = keyMap.curve;
		if (curve) {
			bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value1, nvalue1, scale);
			bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, value2, nvalue2, scale);
		}
		time = time2;
		value1 = nvalue1;
		value2 = nvalue2;
		keyMap = nextMap;
	}
}

function readCurve (curve: [number, number, number, number] | "stepped", timeline: CurveTimeline, bezier: number, frame: number, value: number, time1: number, time2: number,
	value1: number, value2: number, scale: number) {
	if (curve === "stepped") {
		timeline.setStepped(frame);
		return bezier;
	}
	const i = value << 2;
	const cx1 = curve[i];
	const cy1 = curve[i + 1] * scale;
	const cx2 = curve[i + 2];
	const cy2 = curve[i + 3] * scale;
	timeline.setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
	return bezier + 1;
}

// biome-ignore lint/suspicious/noExplicitAny: it is any until we define a schema
function getValue (map: any, property: string, defaultValue: any) {
	return map[property] !== undefined ? map[property] : defaultValue;
}
