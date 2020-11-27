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
		private linkedMeshes = new Array<LinkedMesh>();

		constructor (attachmentLoader: AttachmentLoader) {
			this.attachmentLoader = attachmentLoader;
		}

		readSkeletonData (json: string | any): SkeletonData {
			let scale = this.scale;
			let skeletonData = new SkeletonData();
			let root = typeof(json) === "string" ? JSON.parse(json) : json;

			// Skeleton
			let skeletonMap = root.skeleton;
			if (skeletonMap != null) {
				skeletonData.hash = skeletonMap.hash;
				skeletonData.version = skeletonMap.spine;
				skeletonData.x = skeletonMap.x;
				skeletonData.y = skeletonMap.y;
				skeletonData.width = skeletonMap.width;
				skeletonData.height = skeletonMap.height;
				skeletonData.fps = skeletonMap.fps;
				skeletonData.imagesPath = skeletonMap.images;
			}

			// Bones
			if (root.bones) {
				for (let i = 0; i < root.bones.length; i++) {
					let boneMap = root.bones[i];

					let parent: BoneData = null;
					let parentName: string = this.getValue(boneMap, "parent", null);
					if (parentName != null) {
						parent = skeletonData.findBone(parentName);
						if (parent == null) throw new Error("Parent bone not found: " + parentName);
					}
					let data = new BoneData(skeletonData.bones.length, boneMap.name, parent);
					data.length = this.getValue(boneMap, "length", 0) * scale;
					data.x = this.getValue(boneMap, "x", 0) * scale;
					data.y = this.getValue(boneMap, "y", 0) * scale;
					data.rotation = this.getValue(boneMap, "rotation", 0);
					data.scaleX = this.getValue(boneMap, "scaleX", 1);
					data.scaleY = this.getValue(boneMap, "scaleY", 1);
					data.shearX = this.getValue(boneMap, "shearX", 0);
					data.shearY = this.getValue(boneMap, "shearY", 0);
					data.transformMode = SkeletonJson.transformModeFromString(this.getValue(boneMap, "transform", "normal"));
					data.skinRequired = this.getValue(boneMap, "skin", false);

					let color = this.getValue(boneMap, "color", null);
					if (color) data.color.setFromString(color);

					skeletonData.bones.push(data);
				}
			}

			// Slots.
			if (root.slots) {
				for (let i = 0; i < root.slots.length; i++) {
					let slotMap = root.slots[i];
					let slotName: string = slotMap.name;
					let boneName: string = slotMap.bone;
					let boneData = skeletonData.findBone(boneName);
					if (boneData == null) throw new Error("Slot bone not found: " + boneName);
					let data = new SlotData(skeletonData.slots.length, slotName, boneData);

					let color: string = this.getValue(slotMap, "color", null);
					if (color != null) data.color.setFromString(color);

					let dark: string = this.getValue(slotMap, "dark", null);
					if (dark != null) {
						data.darkColor = new Color(1, 1, 1, 1);
						data.darkColor.setFromString(dark);
					}

					data.attachmentName = this.getValue(slotMap, "attachment", null);
					data.blendMode = SkeletonJson.blendModeFromString(this.getValue(slotMap, "blend", "normal"));
					skeletonData.slots.push(data);
				}
			}

			// IK constraints
			if (root.ik) {
				for (let i = 0; i < root.ik.length; i++) {
					let constraintMap = root.ik[i];
					let data = new IkConstraintData(constraintMap.name);
					data.order = this.getValue(constraintMap, "order", 0);
					data.skinRequired = this.getValue(constraintMap, "skin", false);

					for (let j = 0; j < constraintMap.bones.length; j++) {
						let boneName = constraintMap.bones[j];
						let bone = skeletonData.findBone(boneName);
						if (bone == null) throw new Error("IK bone not found: " + boneName);
						data.bones.push(bone);
					}

					let targetName: string = constraintMap.target;
					data.target = skeletonData.findBone(targetName);
					if (data.target == null) throw new Error("IK target bone not found: " + targetName);

					data.mix = this.getValue(constraintMap, "mix", 1);
					data.softness = this.getValue(constraintMap, "softness", 0) * scale;
					data.bendDirection = this.getValue(constraintMap, "bendPositive", true) ? 1 : -1;
					data.compress = this.getValue(constraintMap, "compress", false);
					data.stretch = this.getValue(constraintMap, "stretch", false);
					data.uniform = this.getValue(constraintMap, "uniform", false);

					skeletonData.ikConstraints.push(data);
				}
			}

			// Transform constraints.
			if (root.transform) {
				for (let i = 0; i < root.transform.length; i++) {
					let constraintMap = root.transform[i];
					let data = new TransformConstraintData(constraintMap.name);
					data.order = this.getValue(constraintMap, "order", 0);
					data.skinRequired = this.getValue(constraintMap, "skin", false);

					for (let j = 0; j < constraintMap.bones.length; j++) {
						let boneName = constraintMap.bones[j];
						let bone = skeletonData.findBone(boneName);
						if (bone == null) throw new Error("Transform constraint bone not found: " + boneName);
						data.bones.push(bone);
					}

					let targetName: string = constraintMap.target;
					data.target = skeletonData.findBone(targetName);
					if (data.target == null) throw new Error("Transform constraint target bone not found: " + targetName);

					data.local = this.getValue(constraintMap, "local", false);
					data.relative = this.getValue(constraintMap, "relative", false);
					data.offsetRotation = this.getValue(constraintMap, "rotation", 0);
					data.offsetX = this.getValue(constraintMap, "x", 0) * scale;
					data.offsetY = this.getValue(constraintMap, "y", 0) * scale;
					data.offsetScaleX = this.getValue(constraintMap, "scaleX", 0);
					data.offsetScaleY = this.getValue(constraintMap, "scaleY", 0);
					data.offsetShearY = this.getValue(constraintMap, "shearY", 0);

					data.mixRotate = this.getValue(constraintMap, "mixRotate", 1);
					data.mixX = this.getValue(constraintMap, "mixX", 1);
					data.mixY = this.getValue(constraintMap, "mixY", data.mixX);
					data.mixScaleX = this.getValue(constraintMap, "mixScaleX", 1);
					data.mixScaleY = this.getValue(constraintMap, "mixScaleY", data.mixScaleX);
					data.mixShearY = this.getValue(constraintMap, "mixShearY", 1);

					skeletonData.transformConstraints.push(data);
				}
			}

			// Path constraints.
			if (root.path) {
				for (let i = 0; i < root.path.length; i++) {
					let constraintMap = root.path[i];
					let data = new PathConstraintData(constraintMap.name);
					data.order = this.getValue(constraintMap, "order", 0);
					data.skinRequired = this.getValue(constraintMap, "skin", false);

					for (let j = 0; j < constraintMap.bones.length; j++) {
						let boneName = constraintMap.bones[j];
						let bone = skeletonData.findBone(boneName);
						if (bone == null) throw new Error("Transform constraint bone not found: " + boneName);
						data.bones.push(bone);
					}

					let targetName: string = constraintMap.target;
					data.target = skeletonData.findSlot(targetName);
					if (data.target == null) throw new Error("Path target slot not found: " + targetName);

					data.positionMode = SkeletonJson.positionModeFromString(this.getValue(constraintMap, "positionMode", "percent"));
					data.spacingMode = SkeletonJson.spacingModeFromString(this.getValue(constraintMap, "spacingMode", "length"));
					data.rotateMode = SkeletonJson.rotateModeFromString(this.getValue(constraintMap, "rotateMode", "tangent"));
					data.offsetRotation = this.getValue(constraintMap, "rotation", 0);
					data.position = this.getValue(constraintMap, "position", 0);
					if (data.positionMode == PositionMode.Fixed) data.position *= scale;
					data.spacing = this.getValue(constraintMap, "spacing", 0);
					if (data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed) data.spacing *= scale;
					data.mixRotate = this.getValue(constraintMap, "mixRotate", 1);
					data.mixX = this.getValue(constraintMap, "mixX", 1);
					data.mixY = this.getValue(constraintMap, "mixY", 1);

					skeletonData.pathConstraints.push(data);
				}
			}

			// Skins.
			if (root.skins) {
				for (let i = 0; i < root.skins.length; i++) {
					let skinMap = root.skins[i]
					let skin = new Skin(skinMap.name);

					if (skinMap.bones) {
						for (let ii = 0; ii < skinMap.bones.length; ii++) {
							let bone = skeletonData.findBone(skinMap.bones[ii]);
							if (bone == null) throw new Error("Skin bone not found: " + skinMap.bones[i]);
							skin.bones.push(bone);
						}
					}

					if (skinMap.ik) {
						for (let ii = 0; ii < skinMap.ik.length; ii++) {
							let constraint = skeletonData.findIkConstraint(skinMap.ik[ii]);
							if (constraint == null) throw new Error("Skin IK constraint not found: " + skinMap.ik[i]);
							skin.constraints.push(constraint);
						}
					}

					if (skinMap.transform) {
						for (let ii = 0; ii < skinMap.transform.length; ii++) {
							let constraint = skeletonData.findTransformConstraint(skinMap.transform[ii]);
							if (constraint == null) throw new Error("Skin transform constraint not found: " + skinMap.transform[i]);
							skin.constraints.push(constraint);
						}
					}

					if (skinMap.path) {
						for (let ii = 0; ii < skinMap.path.length; ii++) {
							let constraint = skeletonData.findPathConstraint(skinMap.path[ii]);
							if (constraint == null) throw new Error("Skin path constraint not found: " + skinMap.path[i]);
							skin.constraints.push(constraint);
						}
					}

					for (let slotName in skinMap.attachments) {
						let slot = skeletonData.findSlot(slotName);
						if (slot == null) throw new Error("Slot not found: " + slotName);
						let slotMap = skinMap.attachments[slotName];
						for (let entryName in slotMap) {
							let attachment = this.readAttachment(slotMap[entryName], skin, slot.index, entryName, skeletonData);
							if (attachment != null) skin.setAttachment(slot.index, entryName, attachment);
						}
					}
					skeletonData.skins.push(skin);
					if (skin.name == "default") skeletonData.defaultSkin = skin;
				}
			}

			// Linked meshes.
			for (let i = 0, n = this.linkedMeshes.length; i < n; i++) {
				let linkedMesh = this.linkedMeshes[i];
				let skin = linkedMesh.skin == null ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
				if (skin == null) throw new Error("Skin not found: " + linkedMesh.skin);
				let parent = skin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (parent == null) throw new Error("Parent mesh not found: " + linkedMesh.parent);
				linkedMesh.mesh.deformAttachment = linkedMesh.inheritDeform ? <VertexAttachment>parent : <VertexAttachment>linkedMesh.mesh;
				linkedMesh.mesh.setParentMesh(<MeshAttachment> parent);
				linkedMesh.mesh.updateUVs();
			}
			this.linkedMeshes.length = 0;

			// Events.
			if (root.events) {
				for (let eventName in root.events) {
					let eventMap = root.events[eventName];
					let data = new EventData(eventName);
					data.intValue = this.getValue(eventMap, "int", 0);
					data.floatValue = this.getValue(eventMap, "float", 0);
					data.stringValue = this.getValue(eventMap, "string", "");
					data.audioPath = this.getValue(eventMap, "audio", null);
					if (data.audioPath != null) {
						data.volume = this.getValue(eventMap, "volume", 1);
						data.balance = this.getValue(eventMap, "balance", 0);
					}
					skeletonData.events.push(data);
				}
			}

			// Animations.
			if (root.animations) {
				for (let animationName in root.animations) {
					let animationMap = root.animations[animationName];
					this.readAnimation(animationMap, animationName, skeletonData);
				}
			}

			return skeletonData;
		}

		readAttachment (map: any, skin: Skin, slotIndex: number, name: string, skeletonData: SkeletonData): Attachment {
			let scale = this.scale;
			name = this.getValue(map, "name", name);

			switch (this.getValue(map, "type", "region")) {
				case "region": {
					let path = this.getValue(map, "path", name);
					let region = this.attachmentLoader.newRegionAttachment(skin, name, path);
					if (region == null) return null;
					region.path = path;
					region.x = this.getValue(map, "x", 0) * scale;
					region.y = this.getValue(map, "y", 0) * scale;
					region.scaleX = this.getValue(map, "scaleX", 1);
					region.scaleY = this.getValue(map, "scaleY", 1);
					region.rotation = this.getValue(map, "rotation", 0);
					region.width = map.width * scale;
					region.height = map.height * scale;

					let color: string = this.getValue(map, "color", null);
					if (color != null) region.color.setFromString(color);

					region.updateOffset();
					return region;
				}
				case "boundingbox": {
					let box = this.attachmentLoader.newBoundingBoxAttachment(skin, name);
					if (box == null) return null;
					this.readVertices(map, box, map.vertexCount << 1);
					let color: string = this.getValue(map, "color", null);
					if (color != null) box.color.setFromString(color);
					return box;
				}
				case "mesh":
				case "linkedmesh": {
					let path = this.getValue(map, "path", name);
					let mesh = this.attachmentLoader.newMeshAttachment(skin, name, path);
					if (mesh == null) return null;
					mesh.path = path;

					let color = this.getValue(map, "color", null);
					if (color != null) mesh.color.setFromString(color);

					mesh.width = this.getValue(map, "width", 0) * scale;
					mesh.height = this.getValue(map, "height", 0) * scale;

					let parent: string = this.getValue(map, "parent", null);
					if (parent != null) {
						this.linkedMeshes.push(new LinkedMesh(mesh, <string> this.getValue(map, "skin", null), slotIndex, parent, this.getValue(map, "deform", true)));
						return mesh;
					}

					let uvs: Array<number> = map.uvs;
					this.readVertices(map, mesh, uvs.length);
					mesh.triangles = map.triangles;
					mesh.regionUVs = uvs;
					mesh.updateUVs();

					mesh.edges = this.getValue(map, "edges", null);
					mesh.hullLength = this.getValue(map, "hull", 0) * 2;
					return mesh;
				}
				case "path": {
					let path = this.attachmentLoader.newPathAttachment(skin, name);
					if (path == null) return null;
					path.closed = this.getValue(map, "closed", false);
					path.constantSpeed = this.getValue(map, "constantSpeed", true);

					let vertexCount = map.vertexCount;
					this.readVertices(map, path, vertexCount << 1);

					let lengths: Array<number> = Utils.newArray(vertexCount / 3, 0);
					for (let i = 0; i < map.lengths.length; i++)
						lengths[i] = map.lengths[i] * scale;
					path.lengths = lengths;

					let color: string = this.getValue(map, "color", null);
					if (color != null) path.color.setFromString(color);
					return path;
				}
				case "point": {
					let point = this.attachmentLoader.newPointAttachment(skin, name);
					if (point == null) return null;
					point.x = this.getValue(map, "x", 0) * scale;
					point.y = this.getValue(map, "y", 0) * scale;
					point.rotation = this.getValue(map, "rotation", 0);

					let color = this.getValue(map, "color", null);
					if (color != null) point.color.setFromString(color);
					return point;
				}
				case "clipping": {
					let clip = this.attachmentLoader.newClippingAttachment(skin, name);
					if (clip == null) return null;

					let end = this.getValue(map, "end", null);
					if (end != null) {
						let slot = skeletonData.findSlot(end);
						if (slot == null) throw new Error("Clipping end slot not found: " + end);
						clip.endSlot = slot;
					}

					let vertexCount = map.vertexCount;
					this.readVertices(map, clip, vertexCount << 1);

					let color: string = this.getValue(map, "color", null);
					if (color != null) clip.color.setFromString(color);
					return clip;
				}
			}
			return null;
		}

		readVertices (map: any, attachment: VertexAttachment, verticesLength: number) {
			let scale = this.scale;
			attachment.worldVerticesLength = verticesLength;
			let vertices: Array<number> = map.vertices;
			if (verticesLength == vertices.length) {
				let scaledVertices = Utils.toFloatArray(vertices);
				if (scale != 1) {
					for (let i = 0, n = vertices.length; i < n; i++)
						scaledVertices[i] *= scale;
				}
				attachment.vertices = scaledVertices;
				return;
			}
			let weights = new Array<number>();
			let bones = new Array<number>();
			for (let i = 0, n = vertices.length; i < n;) {
				let boneCount = vertices[i++];
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

		readAnimation (map: any, name: string, skeletonData: SkeletonData) {
			let scale = this.scale;
			let timelines = new Array<Timeline>();

			// Slot timelines.
			if (map.slots) {
				for (let slotName in map.slots) {
					let slotMap = map.slots[slotName];
					let slotIndex = skeletonData.findSlotIndex(slotName);
					if (slotIndex == -1) throw new Error("Slot not found: " + slotName);
					for (let timelineName in slotMap) {
						let timelineMap = slotMap[timelineName];
						if (!timelineMap) continue;
						if (timelineName == "attachment") {
							let timeline = new AttachmentTimeline(timelineMap.length, slotIndex);
							for (let frame = 0; frame < timelineMap.length; frame++) {
								let keyMap = timelineMap[frame];
								timeline.setFrame(frame, this.getValue(keyMap, "time", 0), keyMap.name);
							}
							timelines.push(timeline);

						} else if (timelineName == "rgba") {
							let timeline = new RGBATimeline(timelineMap.length, timelineMap.length << 2, slotIndex);
							let keyMap = timelineMap[0];
							let time = this.getValue(keyMap, "time", 0);
							let color = new Color().setFromString(keyMap.color);

							for (let frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, color.r, color.g, color.b, color.a);
								if (timelineMap.length == frame + 1) {
									break;
								}
								let nextMap = timelineMap[frame + 1];
								let time2 = this.getValue(nextMap, "time", 0);
								let newColor = new Color().setFromString(nextMap.color);
								let curve = keyMap.curve;
								if (curve) {
									bezier = this.readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 3, time, time2, color.a, newColor.a, 1);
								}
								time = time2;
								color = newColor;
								keyMap = nextMap;
							}

							timelines.push(timeline);

						} else if (timelineName == "rgb") {
							let timeline = new RGBTimeline(timelineMap.length, timelineMap.length * 3, slotIndex);
							let keyMap = timelineMap[0];
							let time = this.getValue(keyMap, "time", 0);
							let color = new Color().setFromString(keyMap.color);

							for (let frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, color.r, color.g, color.b);
								if (timelineMap.length == frame + 1) {
									break;
								}
								let nextMap = timelineMap[frame + 1];
								let time2 = this.getValue(nextMap, "time", 0);
								let newColor = new Color().setFromString(nextMap.color);
								let curve = keyMap.curve;
								if (curve) {
									bezier = this.readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
								}
								time = time2;
								color = newColor;
								keyMap = nextMap;
							}

							timelines.push(timeline);

						} else if (timelineName == "alpha") {
							timelines.push(this.readTimeline(timelineMap, new AlphaTimeline(timelineMap.length, timelineMap.length, slotIndex), 0, 1));
						} else if (timelineName == "rgba2") {
							let timeline = new RGBA2Timeline(timelineMap.length, timelineMap.length * 7, slotIndex);

							let keyMap = timelineMap[0];
							let time = this.getValue(keyMap, "time", 0);
							let color = new Color().setFromString(keyMap.light);
							let color2 = new Color().setFromString(keyMap.dark);

							for (let frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, color.r, color.g, color.b, color.a, color2.r, color2.g, color2.b);
								if (timelineMap.length == frame + 1) {
									break;
								}
								let nextMap = timelineMap[frame + 1];
								let time2 = this.getValue(nextMap, "time", 0);
								let newColor = new Color().setFromString(nextMap.light);
								let newColor2 = new Color().setFromString(nextMap.dark);
								let curve = keyMap.curve;
								if (curve) {
									bezier = this.readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 3, time, time2, color.a, newColor.a, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 4, time, time2, color2.r, newColor2.r, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 5, time, time2, color2.g, newColor2.g, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 6, time, time2, color2.b, newColor2.b, 1);
								}
								time = time2;
								color = newColor;
								color2 = newColor2;
								keyMap = nextMap;
							}

							timelines.push(timeline);

						} else if (timelineName == "rgb2") {
							let timeline = new RGB2Timeline(timelineMap.length, timelineMap.length * 6, slotIndex);

							let keyMap = timelineMap[0];
							let time = this.getValue(keyMap, "time", 0);
							let color = new Color().setFromString(keyMap.light);
							let color2 = new Color().setFromString(keyMap.dark);

							for (let frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, color.r, color.g, color.b, color2.r, color2.g, color2.b);
								if (timelineMap.length == frame + 1) {
									break;
								}
								let nextMap = timelineMap[frame + 1];
								let time2 = this.getValue(nextMap, "time", 0);
								let newColor = new Color().setFromString(nextMap.light);
								let newColor2 = new Color().setFromString(nextMap.dark);
								let curve = keyMap.curve;
								if (curve) {
									bezier = this.readCurve(curve, timeline, bezier, frame, 0, time, time2, color.r, newColor.r, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 1, time, time2, color.g, newColor.g, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 2, time, time2, color.b, newColor.b, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 3, time, time2, color2.r, newColor2.r, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 4, time, time2, color2.g, newColor2.g, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 5, time, time2, color2.b, newColor2.b, 1);
								}
								time = time2;
								color = newColor;
								color2 = newColor2;
								keyMap = nextMap;
							}

							timelines.push(timeline);

						} else
							throw new Error("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
					}
				}
			}

			// Bone timelines.
			if (map.bones) {
				for (let boneName in map.bones) {
					let boneMap = map.bones[boneName];
					let boneIndex = skeletonData.findBoneIndex(boneName);
					if (boneIndex == -1) throw new Error("Bone not found: " + boneName);
					for (let timelineName in boneMap) {
						let timelineMap = boneMap[timelineName];
						if (timelineMap.length == 0) continue;

						if (timelineName === "rotate") {
							timelines.push(this.readTimeline(timelineMap, new RotateTimeline(timelineMap.length, timelineMap.length, boneIndex), 0, 1));
						} else if (timelineName === "translate") {
							let timeline = new TranslateTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
							timelines.push(this.readTimeline2(timelineMap, timeline, "x", "y", 0, scale));
						} else if (timelineName === "translatex") {
							let timeline = new TranslateXTimeline(timelineMap.length, timelineMap.length, boneIndex);
							timelines.push(this.readTimeline(timelineMap, timeline, 0, scale));
						} else if (timelineName === "translatey") {
							let timeline = new TranslateYTimeline(timelineMap.length, timelineMap.length, boneIndex);
							timelines.push(this.readTimeline(timelineMap, timeline, 0, scale));
						} else if (timelineName === "scale") {
							let timeline = new ScaleTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
							timelines.push(this.readTimeline2(timelineMap, timeline, "x", "y", 1, 1));
						} else if (timelineName === "scalex") {
							let timeline = new ScaleXTimeline(timelineMap.length, timelineMap.length, boneIndex);
							timelines.push(this.readTimeline(timelineMap, timeline, 1, 1));
						} else if (timelineName === "scaley") {
							let timeline = new ScaleYTimeline(timelineMap.length, timelineMap.length, boneIndex);
							timelines.push(this.readTimeline(timelineMap, timeline, 1, 1));
						} else if (timelineName === "shear") {
							let timeline = new ShearTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
							timelines.push(this.readTimeline2(timelineMap, timeline, "x", "y", 0, 1));
						} else if (timelineName === "shearx") {
							let timeline = new ShearXTimeline(timelineMap.length, timelineMap.length, boneIndex);
							timelines.push(this.readTimeline(timelineMap, timeline, 0, 1));
						} else if (timelineName === "sheary") {
							let timeline = new ShearYTimeline(timelineMap.length, timelineMap.length, boneIndex);
							timelines.push(this.readTimeline(timelineMap, timeline, 0, 1));
						} else {
							throw new Error("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
						}
					}
				}
			}

			// IK constraint timelines.
			if (map.ik) {
				for (let constraintName in map.ik) {
					let constraintMap = map.ik[constraintName];
					let keyMap = constraintMap[0];
					if (!keyMap) continue;

					let constraint = skeletonData.findIkConstraint(constraintName);
					let constraintIndex = skeletonData.ikConstraints.indexOf(constraint);
					let timeline = new IkConstraintTimeline(constraintMap.length, constraintMap.length << 1, constraintIndex);

					let time = this.getValue(keyMap, "time", 0);
					let mix = this.getValue(keyMap, "mix", 1);
					let softness = this.getValue(keyMap, "softness", 0) * scale;

					for (let frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, mix, softness, this.getValue(keyMap, "bendPositive", true) ? 1 : -1, this.getValue(keyMap, "compress", false), this.getValue(keyMap, "stretch", false))
						let nextMap = constraintMap[frame + 1];
						if (!nextMap) {
							break;
						}

						let time2 = this.getValue(nextMap, "time", 0);
						let mix2 = this.getValue(nextMap, "mix", 1);
						let softness2 = this.getValue(nextMap, "softness", 0) * scale;
						let curve = keyMap.curve;
						if (curve) {
							bezier = this.readCurve(curve, timeline, bezier, frame, 0, time, time2, mix, mix2, 1);
							bezier = this.readCurve(curve, timeline, bezier, frame, 1, time, time2, softness, softness2, scale);
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
				for (let constraintName in map.transform) {
					let timelineMap = map.transform[constraintName];
					let keyMap = timelineMap[0];
					if (!keyMap) continue;

					let constraint = skeletonData.findTransformConstraint(constraintName);
					let constraintIndex = skeletonData.transformConstraints.indexOf(constraint);
					let timeline = new TransformConstraintTimeline(timelineMap.length, timelineMap.length << 2, constraintIndex);

					let time = this.getValue(keyMap, "time", 0);
					let mixRotate = this.getValue(keyMap, "mixRotate", 1);
					let mixShearY = this.getValue(keyMap, "mixShearY", 1);
					let mixX = this.getValue(keyMap, "mixX", 1);
					let mixY = this.getValue(keyMap, "mixY", mixX);
					let mixScaleX = this.getValue(keyMap, "mixScaleX", 1);
					let mixScaleY = this.getValue(keyMap, "mixScaleY", mixScaleX);

					for (let frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
						let nextMap = timelineMap[frame + 1];
						if (!nextMap) {
							break;
						}

						let time2 = this.getValue(nextMap, "time", 0);
						let mixRotate2 = this.getValue(nextMap, "mixRotate", 1);
						let mixShearY2 = this.getValue(nextMap, "mixShearY", 1);
						let mixX2 = this.getValue(nextMap, "mixX", 1);
						let mixY2 = this.getValue(nextMap, "mixY", mixX2);
						let mixScaleX2 = this.getValue(nextMap, "mixScaleX", 1);
						let mixScaleY2 = this.getValue(nextMap, "mixScaleY", mixScaleX2);
						let curve = keyMap.curve;
						if (curve) {
							bezier = this.readCurve(curve, timeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
							bezier = this.readCurve(curve, timeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
							bezier = this.readCurve(curve, timeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
							bezier = this.readCurve(curve, timeline, bezier, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
							bezier = this.readCurve(curve, timeline, bezier, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
							bezier = this.readCurve(curve, timeline, bezier, frame, 5, time, time2, mixShearY, mixShearY2, 1);
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
				for (let constraintName in map.path) {
					let constraintMap = map.path[constraintName];
					let index = skeletonData.findPathConstraintIndex(constraintName);
					if (index == -1) throw new Error("Path constraint not found: " + constraintName);
					let data = skeletonData.pathConstraints[index];
					for (let timelineName in constraintMap) {
						let timelineMap = constraintMap[timelineName];
						let keyMap = timelineMap[0];
						if (!keyMap) continue;

						if (timelineName === "position") {
							let timeline = new PathConstraintPositionTimeline(timelineMap.length, timelineMap.length, index);
							timelines.push(this.readTimeline(timelineMap, timeline, 0, data.positionMode == PositionMode.Fixed ? scale : 1));
						} else if (timelineName === "spacing") {
							let timeline = new PathConstraintSpacingTimeline(timelineMap.length, timelineMap.length, index);
							timelines.push(this.readTimeline(timelineMap, timeline, 0, data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed ? scale : 1));
						} else if (timelineName === "mix") {
							let timeline = new PathConstraintMixTimeline(timelineMap.size, timelineMap.size * 3, index);
							let time = this.getValue(keyMap, "time", 0);
							let mixRotate = this.getValue(keyMap, "mixRotate", 1);
							let mixX = this.getValue(keyMap, "mixX", 1);
							let mixY = this.getValue(keyMap, "mixY", mixX);
							for (let frame = 0, bezier = 0;; frame++) {
								timeline.setFrame(frame, time, mixRotate, mixX, mixY);
								let nextMap = timelineMap[frame + 1];
								if (!nextMap) {
									break;
								}
								let time2 = this.getValue(nextMap, "time", 0);
								let mixRotate2 = this.getValue(nextMap, "mixRotate", 1);
								let mixX2 = this.getValue(nextMap, "mixX", 1);
								let mixY2 = this.getValue(nextMap, "mixY", mixX2);
								let curve = keyMap.curve;
								if (curve != null) {
									bezier = this.readCurve(curve, timeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
									bezier = this.readCurve(curve, timeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
								}
								time = time2;
								mixRotate = mixRotate2;
								mixX = mixX2;
								mixY = mixY2;
								keyMap = nextMap;
							}
							timelines.push(timeline);
						}
					}
				}
			}

			// Deform timelines.
			if (map.deform) {
				for (let deformName in map.deform) {
					let deformMap = map.deform[deformName];
					let skin = skeletonData.findSkin(deformName);
					if (skin == null) throw new Error("Skin not found: " + deformName);
					for (let slotName in deformMap) {
						let slotMap = deformMap[slotName];
						let slotIndex = skeletonData.findSlotIndex(slotName);
						if (slotIndex == -1) throw new Error("Slot not found: " + slotMap.name);
						for (let timelineName in slotMap) {
							let timelineMap = slotMap[timelineName];
							let keyMap = timelineMap[0];
							if (!keyMap) continue;

							let attachment = <VertexAttachment>skin.getAttachment(slotIndex, timelineName);
							if (attachment == null) throw new Error("Deform attachment not found: " + timelineMap.name);
							let weighted = attachment.bones != null;
							let vertices = attachment.vertices;
							let deformLength = weighted ? vertices.length / 3 * 2 : vertices.length;

							let timeline = new DeformTimeline(timelineMap.length, timelineMap.length, slotIndex, attachment);
							let time = this.getValue(keyMap, "time", 0);
							for (let frame = 0, bezier = 0;; frame++) {
								let deform: ArrayLike<number>;
								let verticesValue: Array<Number> = this.getValue(keyMap, "vertices", null);
								if (verticesValue == null)
									deform = weighted ? Utils.newFloatArray(deformLength) : vertices;
								else {
									deform = Utils.newFloatArray(deformLength);
									let start = <number>this.getValue(keyMap, "offset", 0);
									Utils.arrayCopy(verticesValue, 0, deform, start, verticesValue.length);
									if (scale != 1) {
										for (let i = start, n = i + verticesValue.length; i < n; i++)
											deform[i] *= scale;
									}
									if (!weighted) {
										for (let i = 0; i < deformLength; i++)
											deform[i] += vertices[i];
									}
								}

								timeline.setFrame(frame, time, deform);
								let nextMap = timelineMap[frame + 1];
								if (!nextMap) {
									break;
								}
								let time2 = this.getValue(nextMap, "time", 0);
								let curve = keyMap.curve;
								if (curve) {
									bezier = this.readCurve(curve, timeline, bezier, frame, 0, time, time2, 0, 1, 1);
								}
								time = time2;
								keyMap = nextMap;
							}
							timelines.push(timeline);
						}
					}
				}
			}

			// Draw order timeline.
			let drawOrderNode = map.drawOrder;
			if (drawOrderNode == null) drawOrderNode = map.draworder;
			if (drawOrderNode != null) {
				let timeline = new DrawOrderTimeline(drawOrderNode.length);
				let slotCount = skeletonData.slots.length;
				let frame = 0;
				for (let j = 0; j < drawOrderNode.length; j++, frame++) {
					let drawOrderMap = drawOrderNode[j];
					let drawOrder: Array<number> = null;
					let offsets = this.getValue(drawOrderMap, "offsets", null);
					if (offsets != null) {
						drawOrder = Utils.newArray<number>(slotCount, -1);
						let unchanged = Utils.newArray<number>(slotCount - offsets.length, 0);
						let originalIndex = 0, unchangedIndex = 0;
						for (let i = 0; i < offsets.length; i++) {
							let offsetMap = offsets[i];
							let slotIndex = skeletonData.findSlotIndex(offsetMap.slot);
							if (slotIndex == -1) throw new Error("Slot not found: " + offsetMap.slot);
							// Collect unchanged items.
							while (originalIndex != slotIndex)
								unchanged[unchangedIndex++] = originalIndex++;
							// Set changed items.
							drawOrder[originalIndex + offsetMap.offset] = originalIndex++;
						}
						// Collect remaining unchanged items.
						while (originalIndex < slotCount)
							unchanged[unchangedIndex++] = originalIndex++;
						// Fill in unchanged items.
						for (let i = slotCount - 1; i >= 0; i--)
							if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
					}
					timeline.setFrame(frame, this.getValue(drawOrderMap, "time", 0), drawOrder);
				}
				timelines.push(timeline);
			}

			// Event timeline.
			if (map.events) {
				let timeline = new EventTimeline(map.events.length);
				let frame = 0;
				for (let i = 0; i < map.events.length; i++, frame++) {
					let eventMap = map.events[i];
					let eventData = skeletonData.findEvent(eventMap.name);
					if (eventData == null) throw new Error("Event not found: " + eventMap.name);
					let event = new Event(Utils.toSinglePrecision(this.getValue(eventMap, "time", 0)), eventData);
					event.intValue = this.getValue(eventMap, "int", eventData.intValue);
					event.floatValue = this.getValue(eventMap, "float", eventData.floatValue);
					event.stringValue = this.getValue(eventMap, "string", eventData.stringValue);
					if (event.data.audioPath != null) {
						event.volume = this.getValue(eventMap, "volume", 1);
						event.balance = this.getValue(eventMap, "balance", 0);
					}
					timeline.setFrame(frame, event);
				}
				timelines.push(timeline);
			}

			let duration = 0;
			for (let i = 0, n = timelines.length; i < n; i++)
				duration = Math.max(duration, (timelines[i]).getDuration());

			if (isNaN(duration)) {
				throw new Error("Error while parsing animation, duration is NaN");
			}

			skeletonData.animations.push(new Animation(name, timelines, duration));
		}

		private readTimeline (keys: any[], timeline: CurveTimeline1, defaultValue: number, scale: number) {
			let keyMap = keys[0];
			let time = this.getValue(keyMap, "time", 0);
			let value = this.getValue(keyMap, "value", defaultValue) * scale;
			let bezier = 0;
			for (let frame = 0;; frame++) {
				timeline.setFrame(frame, time, value);
				let nextMap = keys[frame + 1];
				if (!nextMap) break;
				let time2 = this.getValue(nextMap, "time", 0);
				let value2 = this.getValue(nextMap, "value", defaultValue) * scale;
				let curve = keyMap.curve;
				if (curve) bezier = this.readCurve(curve, timeline, bezier, frame, 0, time, time2, value, value2, scale);
				time = time2;
				value = value2;
				keyMap = nextMap;
			}
			return timeline;
		}

		private readTimeline2 (keys: any[], timeline: CurveTimeline2, name1: string, name2: string, defaultValue: number, scale: number) {
			let keyMap = keys[0];
			let time = this.getValue(keyMap, "time", 0);
			let value1 = this.getValue(keyMap, name1, defaultValue) * scale;
			let value2 = this.getValue(keyMap, name2, defaultValue) * scale;
			let bezier = 0;
			for (let frame = 0;; frame++) {
				timeline.setFrame(frame, time, value1, value2);
				let nextMap = keys[frame + 1];
				if (!nextMap) break;
				let time2 = this.getValue(nextMap, "time", 0);
				let nvalue1 = this.getValue(nextMap, name1, defaultValue) * scale;
				let nvalue2 = this.getValue(nextMap, name2, defaultValue) * scale;
				let curve = keyMap.curve;
				if (curve != null) {
					bezier = this.readCurve(curve, timeline, bezier, frame, 0, time, time2, value1, nvalue1, scale);
					bezier = this.readCurve(curve, timeline, bezier, frame, 1, time, time2, value2, nvalue2, scale);
				}
				time = time2;
				value1 = nvalue1;
				value2 = nvalue2;
				keyMap = nextMap;
			}
			timeline.shrink(bezier);
			return timeline;
		}

		private readCurve (curve: any, timeline: CurveTimeline, bezier: number, frame: number, value: number, time1: number, time2: number,
			value1: number, value2: number, scale: number) {
			if (curve == "stepped") {
				if (value != 0) timeline.setStepped(frame);
			} else {
				let i = value << 2;
				let cx1 = curve[i++];
				let cy1 = curve[i++] * scale;
				let cx2 = curve[i++];
				let cy2 = curve[i++] * scale;
				this.setBezier(timeline, frame, value, bezier++, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
			}
			return bezier;
		}

		setBezier (timeline: CurveTimeline, frame: number, value: number, bezier: number, time1: number, value1: number, cx1: number, cy1: number,
			cx2: number, cy2: number, time2: number, value2: number) {
			timeline.setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
		}

		getValue (map: any, prop: string, defaultValue: any) {
			return map[prop] !== undefined ? map[prop] : defaultValue;
		}

		static blendModeFromString (str: string) {
			str = str.toLowerCase();
			if (str == "normal") return BlendMode.Normal;
			if (str == "additive") return BlendMode.Additive;
			if (str == "multiply") return BlendMode.Multiply;
			if (str == "screen") return BlendMode.Screen;
			throw new Error(`Unknown blend mode: ${str}`);
		}

		static positionModeFromString (str: string) {
			str = str.toLowerCase();
			if (str == "fixed") return PositionMode.Fixed;
			if (str == "percent") return PositionMode.Percent;
			throw new Error(`Unknown position mode: ${str}`);
		}

		static spacingModeFromString (str: string) {
			str = str.toLowerCase();
			if (str == "length") return SpacingMode.Length;
			if (str == "fixed") return SpacingMode.Fixed;
			if (str == "percent") return SpacingMode.Percent;
			throw new Error(`Unknown position mode: ${str}`);
		}

		static rotateModeFromString (str: string) {
			str = str.toLowerCase();
			if (str == "tangent") return RotateMode.Tangent;
			if (str == "chain") return RotateMode.Chain;
			if (str == "chainscale") return RotateMode.ChainScale;
			throw new Error(`Unknown rotate mode: ${str}`);
		}

		static transformModeFromString(str: string) {
			str = str.toLowerCase();
			if (str == "normal") return TransformMode.Normal;
			if (str == "onlytranslation") return TransformMode.OnlyTranslation;
			if (str == "norotationorreflection") return TransformMode.NoRotationOrReflection;
			if (str == "noscale") return TransformMode.NoScale;
			if (str == "noscaleorreflection") return TransformMode.NoScaleOrReflection;
			throw new Error(`Unknown transform mode: ${str}`);
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
}
