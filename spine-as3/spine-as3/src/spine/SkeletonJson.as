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

	public class SkeletonJson {
		public var attachmentLoader : AttachmentLoader;
		public var scale : Number = 1;
		private var linkedMeshes : Vector.<LinkedMesh> = new Vector.<LinkedMesh>();

		public function SkeletonJson(attachmentLoader : AttachmentLoader = null) {
			this.attachmentLoader = attachmentLoader;
		}

		/** @param object A String or ByteArray. */
		public function readSkeletonData(object : *, name : String = null) : SkeletonData {
			if (object == null) throw new ArgumentError("object cannot be null.");

			var root : Object;
			if (object is String)
				root = JSON.parse(String(object));
			else if (object is ByteArray)
				root = JSON.parse(ByteArray(object).readUTFBytes(ByteArray(object).length));
			else if (object is Object)
				root = object;
			else
				throw new ArgumentError("object must be a String, ByteArray or Object.");

			var skeletonData : SkeletonData = new SkeletonData();
			skeletonData.name = name;

			// Skeleton.
			var skeletonMap : Object = root["skeleton"];
			if (skeletonMap) {
				skeletonData.hash = skeletonMap["hash"];
				skeletonData.version = skeletonMap["spine"];
				skeletonData.x = skeletonMap["x"] || 0;
				skeletonData.y = skeletonMap["y"] || 0;
				skeletonData.width = skeletonMap["width"] || 0;
				skeletonData.height = skeletonMap["height"] || 0;
				skeletonData.fps = skeletonMap["fps"] || 0;
				skeletonData.imagesPath = skeletonMap["images"];
			}

			// Bones.
			var boneData : BoneData;
			for each (var boneMap : Object in root["bones"]) {
				var parent : BoneData = null;
				var parentName : String = boneMap["parent"];
				if (parentName) {
					parent = skeletonData.findBone(parentName);
					if (!parent) throw new Error("Parent bone not found: " + parentName);
				}
				boneData = new BoneData(skeletonData.bones.length, boneMap["name"], parent);
				boneData.length = Number(boneMap["length"] || 0) * scale;
				boneData.x = Number(boneMap["x"] || 0) * scale;
				boneData.y = Number(boneMap["y"] || 0) * scale;
				boneData.rotation = (boneMap["rotation"] || 0);
				boneData.scaleX = getNumber(boneMap, "scaleX", 1);
				boneData.scaleY = getNumber(boneMap, "scaleY", 1);
				boneData.shearX = Number(boneMap["shearX"] || 0);
				boneData.shearY = Number(boneMap["shearY"] || 0);
				boneData.transformMode = TransformMode[boneMap["transform"] || "normal"];
				boneData.skinRequired = getValue(boneMap, "skin", false);

				color = boneMap["color"];
				if (color) boneData.color.setFromString(color);

				skeletonData.bones.push(boneData);
			}

			// Slots.
			for each (var slotMap : Object in root["slots"]) {
				var slotName : String = slotMap["name"];
				var boneName : String = slotMap["bone"];
				boneData = skeletonData.findBone(boneName);
				if (!boneData) throw new Error("Slot bone not found: " + boneName);
				var slotData : SlotData = new SlotData(skeletonData.slots.length, slotName, boneData);

				var color : String = slotMap["color"];
				if (color) slotData.color.setFromString(color);

				var dark : String = slotMap["dark"];
				if (dark) slotData.darkColor = Color.fromString(dark);

				slotData.attachmentName = slotMap["attachment"];
				slotData.blendMode = BlendMode[slotMap["blend"] || "normal"];
				skeletonData.slots.push(slotData);
			}

			// IK constraints.
			for each (var constraintMap : Object in root["ik"]) {
				var ikData : IkConstraintData = new IkConstraintData(constraintMap["name"]);
				ikData.order = constraintMap["order"] || 0;
				ikData.skinRequired = getValue(constraintMap, "skin", false);

				for each (boneName in constraintMap["bones"]) {
					var bone : BoneData = skeletonData.findBone(boneName);
					if (!bone) throw new Error("IK constraint bone not found: " + boneName);
					ikData.bones.push(bone);
				}

				ikData.target = skeletonData.findBone(constraintMap["target"]);
				if (!ikData.target) throw new Error("Target bone not found: " + constraintMap["target"]);

				ikData.bendDirection = (!constraintMap.hasOwnProperty("bendPositive") || constraintMap["bendPositive"]) ? 1 : -1;
				ikData.compress = getValue(constraintMap, "compress", false);
				ikData.stretch = getValue(constraintMap, "stretch", false);
				ikData.uniform = getValue(constraintMap, "uniform", false);
				ikData.softness = getNumber(constraintMap, "softness", 0) * scale;
				ikData.mix = getNumber(constraintMap, "mix", 1);

				skeletonData.ikConstraints.push(ikData);
			}

			// Transform constraints.
			for each (constraintMap in root["transform"]) {
				var transformData : TransformConstraintData = new TransformConstraintData(constraintMap["name"]);
				transformData.order = constraintMap["order"] || 0;
				transformData.skinRequired = getValue(constraintMap, "skin", false);

				for each (boneName in constraintMap["bones"]) {
					bone = skeletonData.findBone(boneName);
					if (!bone) throw new Error("Transform constraint bone not found: " + boneName);
					transformData.bones.push(bone);
				}

				transformData.target = skeletonData.findBone(constraintMap["target"]);
				if (!transformData.target) throw new Error("Target bone not found: " + constraintMap["target"]);

				transformData.local = getValue(constraintMap, "local", false);
				transformData.relative = getValue(constraintMap, "relative", false);

				transformData.offsetRotation = Number(constraintMap["rotation"] || 0);
				transformData.offsetX = Number(constraintMap["x"] || 0) * scale;
				transformData.offsetY = Number(constraintMap["y"] || 0) * scale;
				transformData.offsetScaleX = Number(constraintMap["scaleX"] || 0);
				transformData.offsetScaleY = Number(constraintMap["scaleY"] || 0);
				transformData.offsetShearY = Number(constraintMap["shearY"] || 0);

				transformData.mixRotate = getNumber(constraintMap, "mixRotate", 1);
				transformData.mixX = getNumber(constraintMap, "mixX", 1);
				transformData.mixY = getNumber(constraintMap, "mixY", transformData.mixX);
				transformData.mixScaleX = getNumber(constraintMap, "mixScaleX", 1);
				transformData.mixScaleY = getNumber(constraintMap, "mixScaleY", transformData.mixScaleX);
				transformData.mixShearY = getNumber(constraintMap, "mixShearY", 1);

				skeletonData.transformConstraints.push(transformData);
			}

			// Path constraints.
			for each (constraintMap in root["path"]) {
				var pathData : PathConstraintData = new PathConstraintData(constraintMap["name"]);
				pathData.order = constraintMap["order"] || 0;
				pathData.skinRequired = getValue(constraintMap, "skin", false);

				for each (boneName in constraintMap["bones"]) {
					bone = skeletonData.findBone(boneName);
					if (!bone) throw new Error("Path constraint bone not found: " + boneName);
					pathData.bones.push(bone);
				}

				pathData.target = skeletonData.findSlot(constraintMap["target"]);
				if (!pathData.target) throw new Error("Path target slot not found: " + constraintMap["target"]);

				pathData.positionMode = PositionMode[constraintMap["positionMode"] || "percent"];
				pathData.spacingMode = SpacingMode[constraintMap["spacingMode"] || "length"];
				pathData.rotateMode = RotateMode[constraintMap["rotateMode"] || "tangent"];
				pathData.offsetRotation = Number(constraintMap["rotation"] || 0);
				pathData.position = Number(constraintMap["position"] || 0);
				if (pathData.positionMode == PositionMode.fixed) pathData.position *= scale;
				pathData.spacing = Number(constraintMap["spacing"] || 0);
				if (pathData.spacingMode == SpacingMode.length || pathData.spacingMode == SpacingMode.fixed) pathData.spacing *= scale;
				pathData.mixRotate = getNumber(constraintMap, "mixRotate", 1);
				pathData.mixX = getNumber(constraintMap, "mixX", 1);
				pathData.mixY = getNumber(constraintMap, "mixY", pathData.mixX);

				skeletonData.pathConstraints.push(pathData);
			}

			// Skins.
			var skins : Object = root["skins"];
			for (var i : int = 0; i < skins.length; i++) {
				var ii : int;
				var skinMap : Object = skins[i];
				var skin : Skin = new Skin(skinMap["name"]);

				if (skinMap["bones"]) {
					for (ii = 0; ii < skinMap["bones"].length; ii++) {
						boneData = skeletonData.findBone(skinMap["bones"][ii]);
						if (boneData == null) throw new Error("Skin bone not found: " + skinMap["bones"][ii]);
						skin.bones.push(boneData);
					}
				}

				var constraint : ConstraintData;
				if (skinMap["ik"]) {
					for (ii = 0; ii < skinMap["ik"].length; ii++) {
						constraint = skeletonData.findIkConstraint(skinMap["ik"][ii]);
						if (constraint == null) throw new Error("Skin IK constraint not found: " + skinMap["ik"][ii]);
						skin.constraints.push(constraint);
					}
				}

				if (skinMap["transform"]) {
					for (ii = 0; ii < skinMap["transform"].length; ii++) {
						constraint = skeletonData.findTransformConstraint(skinMap["transform"][ii]);
						if (constraint == null) throw new Error("Skin transform constraint not found: " + skinMap["transform"][ii]);
						skin.constraints.push(constraint);
					}
				}

				if (skinMap["path"]) {
					for (ii = 0; ii < skinMap["path"].length; ii++) {
						constraint = skeletonData.findPathConstraint(skinMap["path"][ii]);
						if (constraint == null) throw new Error("Skin path constraint not found: " + skinMap["path"][ii]);
						skin.constraints.push(constraint);
					}
				}

				for (slotName in skinMap.attachments) {
					var slot : SlotData = skeletonData.findSlot(slotName);
					var slotEntry : Object = skinMap.attachments[slotName];
					for (var attachmentName : String in slotEntry) {
						var attachment : Attachment = readAttachment(slotEntry[attachmentName], skin, slot.index, attachmentName, skeletonData);
						if (attachment != null)
							skin.setAttachment(slot.index, attachmentName, attachment);
					}
				}
				skeletonData.skins.push(skin);
				if (skin.name == "default") skeletonData.defaultSkin = skin;
			}

			// Linked meshes.
			var linkedMeshes : Vector.<LinkedMesh> = this.linkedMeshes;
			for each (var linkedMesh : LinkedMesh in linkedMeshes) {
				var parentSkin : Skin = !linkedMesh.skin ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
				if (!parentSkin) throw new Error("Skin not found: " + linkedMesh.skin);
				var parentMesh : Attachment = parentSkin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (!parentMesh) throw new Error("Parent mesh not found: " + linkedMesh.parent);
				linkedMesh.mesh.deformAttachment = linkedMesh.inheritDeform ? VertexAttachment(parentMesh) : linkedMesh.mesh;
				linkedMesh.mesh.parentMesh = MeshAttachment(parentMesh);
				linkedMesh.mesh.updateUVs();
			}
			linkedMeshes.length = 0;

			// Events.
			var events : Object = root["events"];
			if (events) {
				for (var eventName : String in events) {
					var eventMap : Object = events[eventName];
					var eventData : EventData = new EventData(eventName);
					eventData.intValue = eventMap["int"] || 0;
					eventData.floatValue = eventMap["float"] || 0;
					eventData.stringValue = eventMap["string"] || "";
					eventData.audioPath = eventMap["audio"] || null;
					if (eventData.audioPath != null) {
						eventData.volume = eventMap["volume"] || 1;
						eventData.balance = eventMap["balance"] || 0;
					}
					skeletonData.events.push(eventData);
				}
			}

			// Animations.
			var animations : Object = root["animations"];
			for (var animationName : String in animations)
				readAnimation(animations[animationName], animationName, skeletonData);

			return skeletonData;
		}

		private function readAttachment(map : Object, skin : Skin, slotIndex : int, name : String, skeletonData: SkeletonData) : Attachment {
			name = map["name"] || name;

			var scale : Number = this.scale;
			var color : String;
			switch (AttachmentType[getValue(map, "type", "region")]) {
			case AttachmentType.region:
				var region : RegionAttachment = attachmentLoader.newRegionAttachment(skin, name, map["path"] || name);
				if (!region) return null;
				region.path = map["path"] || name;
				region.x = Number(map["x"] || 0) * scale;
				region.y = Number(map["y"] || 0) * scale;
				region.scaleX = getNumber(map, "scaleX", 1);
				region.scaleY = getNumber(map, "scaleY", 1);
				region.rotation = map["rotation"] || 0;
				region.width = Number(map["width"] || 0) * scale;
				region.height = Number(map["height"] || 0) * scale;

				color = map["color"];
				if (color) region.color.setFromString(color);

				region.updateOffset();
				return region;
			case AttachmentType.mesh:
			case AttachmentType.linkedmesh:
				var mesh : MeshAttachment = attachmentLoader.newMeshAttachment(skin, name, map["path"] || name);
				if (!mesh) return null;
				mesh.path = map["path"] || name;

				color = map["color"];
				if (color) mesh.color.setFromString(color);

				mesh.width = Number(map["width"] || 0) * scale;
				mesh.height = Number(map["height"] || 0) * scale;
				if (map["parent"]) {
					var inheritDeform : Boolean = getValue(map, "deform", true);
					linkedMeshes.push(new LinkedMesh(mesh, map["skin"], slotIndex, map["parent"], inheritDeform));
					return mesh;
				}
				var uvs : Vector.<Number> = getFloatArray(map, "uvs", 1);
				readVertices(map, mesh, uvs.length);
				mesh.triangles = getUintArray(map, "triangles");
				mesh.regionUVs = uvs;
				mesh.updateUVs();
				mesh.hullLength = int(map["hull"] || 0) * 2;
				if (map["edges"]) mesh.edges = getIntArray(map, "edges");
				return mesh;
			case AttachmentType.boundingbox:
				var box : BoundingBoxAttachment = attachmentLoader.newBoundingBoxAttachment(skin, name);
				if (!box) return null;
				readVertices(map, box, int(map["vertexCount"]) << 1);
				return box;
			case AttachmentType.path:
				var path : PathAttachment = attachmentLoader.newPathAttachment(skin, name);
				if (!path) return null;
				path.closed = getValue(map, "closed", false);
				path.constantSpeed = getValue(map, "constantSpeed", true);
				var vertexCount : int = int(map["vertexCount"]);
				readVertices(map, path, vertexCount << 1);
				var lengths : Vector.<Number> = new Vector.<Number>();
				for each (var curves : Object in map["lengths"]) {
					lengths.push(Number(curves) * scale);
				}
				path.lengths = lengths;
				return path;
			case AttachmentType.point:
				var point : PointAttachment = attachmentLoader.newPointAttachment(skin, name);
				if (!point) return null;
				point.x = getNumber(map, "x", 0) * scale;
				point.y = getNumber(map, "y", 0) * scale;
				point.rotation = getNumber(map, "rotation", 0);

				color = map["color"];
				if (color) point.color.setFromString(color);

				return point;
			case AttachmentType.clipping:
				var clip : ClippingAttachment = attachmentLoader.newClippingAttachment(skin, name);
				if (!clip) return null;
				var end : String = map["end"];
				if (end != null) {
					var slot : SlotData = skeletonData.findSlot(end);
					if (slot == null) throw new Error("Clipping end slot not found: " + end);
					clip.endSlot = slot;
				}

				vertexCount = int(map["vertexCount"]);
				readVertices(map, clip, vertexCount << 1);

				color = map["color"];
				if (color) clip.color.setFromString(color);

				return clip;
			}
			return null;
		}

		private function readVertices(map : Object, attachment : VertexAttachment, verticesLength : int) : void {
			attachment.worldVerticesLength = verticesLength;
			var vertices : Vector.<Number> = getFloatArray(map, "vertices", 1);
			if (verticesLength == vertices.length) {
				if (scale != 1) {
					for (var i : int = 0, n : int = vertices.length; i < n; i++) {
						vertices[i] *= scale;
					}
				}
				attachment.vertices = vertices;
				return;
			}

			var weights : Vector.<Number> = new Vector.<Number>(verticesLength * 3 * 3);
			weights.length = 0;
			var bones : Vector.<int> = new Vector.<int>(verticesLength * 3);
			bones.length = 0;
			for (i = 0, n = vertices.length; i < n;) {
				var boneCount : int = int(vertices[i++]);
				bones.push(boneCount);
				for (var nn : int = i + boneCount * 4; i < nn; i += 4) {
					bones.push(int(vertices[i]));
					weights.push(vertices[i + 1] * scale);
					weights.push(vertices[i + 2] * scale);
					weights.push(vertices[i + 3]);
				}
			}
			attachment.bones = bones;
			attachment.vertices = weights;
		}

		private function readAnimation(map : Object, name : String, skeletonData : SkeletonData) : void {
			var scale : Number = this.scale;
			var timelines : Vector.<Timeline> = new Vector.<Timeline>();

			var slotMap : Object, slotIndex : int, slotName : String;
			var timelineMap : Array, keyMap : Object, nextMap : Object;
			var frame : int, bezier : int;
			var time : Number, time2 : Number;
			var curve : Object;
			var timelineName : String;
			var i : int, n : int;

			// Slot timelines.
			var slots : Object = map["slots"];
			for (slotName in slots) {
				slotMap = slots[slotName];
				slotIndex = skeletonData.findSlotIndex(slotName);

				for (timelineName in slotMap) {
					timelineMap = slotMap[timelineName];
					if (!timelineMap) continue;
					if (timelineName == "attachment") {
						var attachmentTimeline : AttachmentTimeline = new AttachmentTimeline(timelineMap.length, slotIndex);
						for (frame = 0; frame < timelineMap.length; frame++) {
							keyMap = timelineMap[frame];
							attachmentTimeline.setFrame(frame, getNumber(keyMap, "time", 0), keyMap.name);
						}
						timelines.push(attachmentTimeline);

					} else if (timelineName == "rgba") {
						var rgbaTimeline : RGBATimeline = new RGBATimeline(timelineMap.length, timelineMap.length << 2, slotIndex);
						keyMap = timelineMap[0];
						time = getNumber(keyMap, "time", 0);
						var rgba : Color = Color.fromString(keyMap.color);

						for (frame = 0, bezier = 0;; frame++) {
							rgbaTimeline.setFrame(frame, time, rgba.r, rgba.g, rgba.b, rgba.a);
							nextMap = timelineMap[frame + 1];
							if (!nextMap) {
								timeline.shrink(bezier);
								break;
							}
							time2 = getNumber(nextMap, "time", 0);
							var newRgba : Color = Color.fromString(nextMap.color);
							curve = keyMap.curve;
							if (curve) {
								bezier = readCurve(curve, rgbaTimeline, bezier, frame, 0, time, time2, rgba.r, newRgba.r, 1);
								bezier = readCurve(curve, rgbaTimeline, bezier, frame, 1, time, time2, rgba.g, newRgba.g, 1);
								bezier = readCurve(curve, rgbaTimeline, bezier, frame, 2, time, time2, rgba.b, newRgba.b, 1);
								bezier = readCurve(curve, rgbaTimeline, bezier, frame, 3, time, time2, rgba.a, newRgba.a, 1);
							}
							time = time2;
							rgba = newRgba;
							keyMap = nextMap;
						}
						timelines.push(rgbaTimeline);

					} else if (timelineName == "rgb") {
						var rgbTimeline : RGBTimeline = new RGBTimeline(timelineMap.length, timelineMap.length * 3, slotIndex);
						keyMap = timelineMap[0];
						time = getNumber(keyMap, "time", 0);
						var rgb : Color = Color.fromString(keyMap.color);

						for (frame = 0, bezier = 0;; frame++) {
							rgbTimeline.setFrame(frame, time, rgb.r, rgb.g, rgb.b);
							nextMap = timelineMap[frame + 1];
							if (!nextMap) {
								timeline.shrink(bezier);
								break;
							}
							time2 = getNumber(nextMap, "time", 0);
							var newRgb : Color = Color.fromString(nextMap.color);
							curve = keyMap.curve;
							if (curve) {
								bezier = readCurve(curve, rgbTimeline, bezier, frame, 0, time, time2, rgb.r, newRgb.r, 1);
								bezier = readCurve(curve, rgbTimeline, bezier, frame, 1, time, time2, rgb.g, newRgb.g, 1);
								bezier = readCurve(curve, rgbTimeline, bezier, frame, 2, time, time2, rgb.b, newRgb.b, 1);
							}
							time = time2;
							rgb = newRgb;
							keyMap = nextMap;
						}
						timelines.push(rgbTimeline);

					} else if (timelineName == "alpha") {
						timelines.push(readTimeline(timelineMap, new AlphaTimeline(timelineMap.length, timelineMap.length, slotIndex), 0, 1));
					} else if (timelineName == "rgba2") {
						var rgba2Timeline : RGBA2Timeline = new RGBA2Timeline(timelineMap.length, timelineMap.length * 7, slotIndex);

						keyMap = timelineMap[0];
						time = getNumber(keyMap, "time", 0);
						var lighta : Color = Color.fromString(keyMap.light);
						var darka : Color = Color.fromString(keyMap.dark);

						for (frame = 0, bezier = 0;; frame++) {
							rgba2Timeline.setFrame(frame, time, lighta.r, lighta.g, lighta.b, lighta.a, darka.r, darka.g, darka.b);
							nextMap = timelineMap[frame + 1];
							if (!nextMap) {
								timeline.shrink(bezier);
								break;
							}
							time2 = getNumber(nextMap, "time", 0);
							var newLighta : Color = Color.fromString(nextMap.light);
							var newDarka : Color = Color.fromString(nextMap.dark);
							curve = keyMap.curve;
							if (curve) {
								bezier = readCurve(curve, rgba2Timeline, bezier, frame, 0, time, time2, lighta.r, newLighta.r, 1);
								bezier = readCurve(curve, rgba2Timeline, bezier, frame, 1, time, time2, lighta.g, newLighta.g, 1);
								bezier = readCurve(curve, rgba2Timeline, bezier, frame, 2, time, time2, lighta.b, newLighta.b, 1);
								bezier = readCurve(curve, rgba2Timeline, bezier, frame, 3, time, time2, lighta.a, newLighta.a, 1);
								bezier = readCurve(curve, rgba2Timeline, bezier, frame, 4, time, time2, darka.r, newDarka.r, 1);
								bezier = readCurve(curve, rgba2Timeline, bezier, frame, 5, time, time2, darka.g, newDarka.g, 1);
								bezier = readCurve(curve, rgba2Timeline, bezier, frame, 6, time, time2, darka.b, newDarka.b, 1);
							}
							time = time2;
							lighta = newLighta;
							darka = newDarka;
							keyMap = nextMap;
						}
						timelines.push(rgba2Timeline);

					} else if (timelineName == "rgb2") {
						var rgb2Timeline : RGB2Timeline = new RGB2Timeline(timelineMap.length, timelineMap.length * 6, slotIndex);

						keyMap = timelineMap[0];
						time = getNumber(keyMap, "time", 0);
						var light : Color = Color.fromString(keyMap.light);
						var dark : Color = Color.fromString(keyMap.dark);

						for (frame = 0, bezier = 0;; frame++) {
							rgb2Timeline.setFrame(frame, time, light.r, light.g, light.b, dark.r, dark.g, dark.b);
							nextMap = timelineMap[frame + 1];
							if (!nextMap) {
								timeline.shrink(bezier);
								break;
							}
							time2 = getNumber(nextMap, "time", 0);
							var newLight : Color = Color.fromString(nextMap.light);
							var newDark : Color = Color.fromString(nextMap.dark);
							curve = keyMap.curve;
							if (curve) {
								bezier = readCurve(curve, rgb2Timeline, bezier, frame, 0, time, time2, light.r, newLight.r, 1);
								bezier = readCurve(curve, rgb2Timeline, bezier, frame, 1, time, time2, light.g, newLight.g, 1);
								bezier = readCurve(curve, rgb2Timeline, bezier, frame, 2, time, time2, light.b, newLight.b, 1);
								bezier = readCurve(curve, rgb2Timeline, bezier, frame, 3, time, time2, dark.r, newDark.r, 1);
								bezier = readCurve(curve, rgb2Timeline, bezier, frame, 4, time, time2, dark.g, newDark.g, 1);
								bezier = readCurve(curve, rgb2Timeline, bezier, frame, 5, time, time2, dark.b, newDark.b, 1);
							}
							time = time2;
							light = newLight;
							dark = newDark;
							keyMap = nextMap;
						}
						timelines.push(rgb2Timeline);

					} else
						throw new Error("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
				}
			}

			// Bone timelines.
			var bones : Object = map["bones"];
			for (var boneName : String in bones) {
				var boneIndex : int = skeletonData.findBoneIndex(boneName);
				if (boneIndex == -1) throw new Error("Bone not found: " + boneName);
				var boneMap : Object = bones[boneName];

				for (timelineName in boneMap) {
					timelineMap = boneMap[timelineName];
					if (timelineMap.length == 0) continue;

					if (timelineName === "rotate") {
						timelines.push(readTimeline(timelineMap, new RotateTimeline(timelineMap.length, timelineMap.length, boneIndex), 0, 1));
					} else if (timelineName === "translate") {
						var translateTimeline : TranslateTimeline = new TranslateTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
						timelines.push(readTimeline2(timelineMap, translateTimeline, "x", "y", 0, scale));
					} else if (timelineName === "translatex") {
						var translateXTimeline : TranslateXTimeline = new TranslateXTimeline(timelineMap.length, timelineMap.length, boneIndex);
						timelines.push(readTimeline(timelineMap, translateXTimeline, 0, scale));
					} else if (timelineName === "translatey") {
						var translateYTimeline : TranslateYTimeline = new TranslateYTimeline(timelineMap.length, timelineMap.length, boneIndex);
						timelines.push(readTimeline(timelineMap, translateYTimeline, 0, scale));
					} else if (timelineName === "scale") {
						var scaleTimeline : ScaleTimeline = new ScaleTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
						timelines.push(readTimeline2(timelineMap, scaleTimeline, "x", "y", 1, 1));
					} else if (timelineName === "scalex") {
						var scaleXTimeline : ScaleXTimeline = new ScaleXTimeline(timelineMap.length, timelineMap.length, boneIndex);
						timelines.push(readTimeline(timelineMap, scaleXTimeline, 1, 1));
					} else if (timelineName === "scaley") {
						var scaleYTimeline : ScaleYTimeline = new ScaleYTimeline(timelineMap.length, timelineMap.length, boneIndex);
						timelines.push(readTimeline(timelineMap, scaleYTimeline, 1, 1));
					} else if (timelineName === "shear") {
						var shearTimeline : ShearTimeline = new ShearTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
						timelines.push(readTimeline2(timelineMap, shearTimeline, "x", "y", 0, 1));
					} else if (timelineName === "shearx") {
						var shearXTimeline : ShearXTimeline = new ShearXTimeline(timelineMap.length, timelineMap.length, boneIndex);
						timelines.push(readTimeline(timelineMap, shearXTimeline, 0, 1));
					} else if (timelineName === "sheary") {
						var shearYTimeline : ShearYTimeline = new ShearYTimeline(timelineMap.length, timelineMap.length, boneIndex);
						timelines.push(readTimeline(timelineMap, shearYTimeline, 0, 1));
					} else
						throw new Error("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
				}
			}

			// IK constraint timelines.
			var ikMap : Object = map["ik"];
			for (var ikConstraintName : String in ikMap) {
				timelineMap = map.ik[ikConstraintName];
				keyMap = timelineMap[0];
				if (!keyMap) continue;

				var ikIndex : int = skeletonData.ikConstraints.indexOf(skeletonData.findIkConstraint(ikConstraintName));
				var ikTimeline : IkConstraintTimeline = new IkConstraintTimeline(timelineMap.length, timelineMap.length << 1, ikIndex);

				time = getNumber(keyMap, "time", 0);
				var mix : Number = getNumber(keyMap, "mix", 1);
				var softness : Number = getNumber(keyMap, "softness", 0) * scale;

				for (frame = 0, bezier = 0;; frame++) {
					ikTimeline.setFrame(frame, time, mix, softness, getValue(keyMap, "bendPositive", true) ? 1 : -1, getValue(keyMap, "compress", false), getValue(keyMap, "stretch", false));
					nextMap = timelineMap[frame + 1];
					if (!nextMap) {
						timeline.shrink(bezier);
						break;
					}

					time2 = getNumber(nextMap, "time", 0);
					var mix2 : Number = getNumber(nextMap, "mix", 1);
					var softness2 : Number = getNumber(nextMap, "softness", 0) * scale;
					curve = keyMap.curve;
					if (curve) {
						bezier = readCurve(curve, ikTimeline, bezier, frame, 0, time, time2, mix, mix2, 1);
						bezier = readCurve(curve, ikTimeline, bezier, frame, 1, time, time2, softness, softness2, scale);
					}

					time = time2;
					mix = mix2;
					softness = softness2;
					keyMap = nextMap;
				}
				timelines.push(ikTimeline);
			}

			// Transform constraint timelines.
			var mixRotate : Number, mixRotate2 : Number;
			var mixX : Number, mixX2 : Number;
			var mixY : Number, mixY2 : Number;
			var transformMap : Object = map["transform"];
			for (var transformName : String in transformMap) {
				timelineMap = map.transform[transformName];
				keyMap = timelineMap[0];
				if (!keyMap) continue;

				var transformIndex : int = skeletonData.transformConstraints.indexOf(skeletonData.findTransformConstraint(transformName));
				var transformTimeline : TransformConstraintTimeline = new TransformConstraintTimeline(timelineMap.length, timelineMap.length << 2, transformIndex);

				time = getNumber(keyMap, "time", 0);
				mixRotate = getNumber(keyMap, "mixRotate", 1);
				var mixShearY : Number = getNumber(keyMap, "mixShearY", 1);
				mixX = getNumber(keyMap, "mixX", 1);
				mixY = getNumber(keyMap, "mixY", mixX);
				var mixScaleX : Number = getNumber(keyMap, "mixScaleX", 1);
				var mixScaleY : Number = getNumber(keyMap, "mixScaleY", mixScaleX);

				for (frame = 0, bezier = 0;; frame++) {
					transformTimeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
					nextMap = timelineMap[frame + 1];
					if (!nextMap) {
						timeline.shrink(bezier);
						break;
					}

					time2 = getNumber(nextMap, "time", 0);
					mixRotate2 = getNumber(nextMap, "mixRotate", 1);
					var mixShearY2 : Number = getNumber(nextMap, "mixShearY", 1);
					mixX2 = getNumber(nextMap, "mixX", 1);
					mixY2 = getNumber(nextMap, "mixY", mixX2);
					var mixScaleX2 : Number = getNumber(nextMap, "mixScaleX", 1);
					var mixScaleY2 : Number = getNumber(nextMap, "mixScaleY", mixScaleX2);
					curve = keyMap.curve;
					if (curve) {
						bezier = readCurve(curve, transformTimeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
						bezier = readCurve(curve, transformTimeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
						bezier = readCurve(curve, transformTimeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
						bezier = readCurve(curve, transformTimeline, bezier, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
						bezier = readCurve(curve, transformTimeline, bezier, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
						bezier = readCurve(curve, transformTimeline, bezier, frame, 5, time, time2, mixShearY, mixShearY2, 1);
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
				timelines.push(transformTimeline);
			}

			// Path constraint timelines.
			var paths : Object = map["path"];
			for (var pathName : String in paths) {
				var index : int = skeletonData.findPathConstraintIndex(pathName);
				if (index == -1) throw new Error("Path constraint not found: " + pathName);
				var pathData : PathConstraintData = skeletonData.pathConstraints[index];

				var pathMap : Object = paths[pathName];
				for (timelineName in pathMap) {
					timelineMap = pathMap[timelineName];
					keyMap = timelineMap[0];
					if (!keyMap) continue;

					if (timelineName === "position") {
						var positionTimeline : PathConstraintPositionTimeline = new PathConstraintPositionTimeline(timelineMap.length, timelineMap.length, index);
						timelines.push(readTimeline(timelineMap, positionTimeline, 0, pathData.positionMode == PositionMode.fixed ? scale : 1));
					} else if (timelineName === "spacing") {
						var spacingTimeline : PathConstraintSpacingTimeline = new PathConstraintSpacingTimeline(timelineMap.length, timelineMap.length, index);
						timelines.push(readTimeline(timelineMap, spacingTimeline, 0, pathData.spacingMode == SpacingMode.length || pathData.spacingMode == SpacingMode.fixed ? scale : 1));
					} else if (timelineName === "mix") {
						var mixTimeline : PathConstraintMixTimeline = new PathConstraintMixTimeline(timelineMap.size, timelineMap.size * 3, index);
						time = getNumber(keyMap, "time", 0);
						mixRotate = getNumber(keyMap, "mixRotate", 1);
						mixX = getNumber(keyMap, "mixX", 1);
						mixY = getNumber(keyMap, "mixY", mixX);
						for (frame = 0, bezier = 0;; frame++) {
							mixTimeline.setFrame(frame, time, mixRotate, mixX, mixY);
							nextMap = timelineMap[frame + 1];
							if (!nextMap) {
								timeline.shrink(bezier);
								break;
							}
							time2 = getNumber(nextMap, "time", 0);
							mixRotate2 = getNumber(nextMap, "mixRotate", 1);
							mixX2 = getNumber(nextMap, "mixX", 1);
							mixY2 = getNumber(nextMap, "mixY", mixX2);
							curve = keyMap.curve;
							if (curve != null) {
								bezier = readCurve(curve, mixTimeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
								bezier = readCurve(curve, mixTimeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
								bezier = readCurve(curve, mixTimeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
							}
							time = time2;
							mixRotate = mixRotate2;
							mixX = mixX2;
							mixY = mixY2;
							keyMap = nextMap;
						}
						timelines.push(mixTimeline);
					}
				}
			}

			// Deform timelines.
			var deforms : Object = map["deform"];
			for (var deformName : String in deforms) {
				var deformMap : Object = deforms[deformName];
				var skin : Skin = skeletonData.findSkin(deformName);
				if (skin == null) throw new Error("Skin not found: " + deformName);
				for (slotName in deformMap) {
					slotMap = deformMap[slotName];
					slotIndex = skeletonData.findSlotIndex(slotName);
					if (slotIndex == -1) throw new Error("Slot not found: " + slotMap.name);
					for (timelineName in slotMap) {
						timelineMap = slotMap[timelineName];
						keyMap = timelineMap[0];
						if (!keyMap) continue;

						var attachment : VertexAttachment = skin.getAttachment(slotIndex, timelineName) as VertexAttachment;
						if (attachment == null) throw new Error("Deform attachment not found: " + timelineName);
						var weighted : Boolean = attachment.bones != null;
						var vertices : Vector.<Number> = attachment.vertices;
						var deformLength : int = weighted ? vertices.length / 3 * 2 : vertices.length;

						var deformTimeline : DeformTimeline = new DeformTimeline(timelineMap.length, timelineMap.length, slotIndex, attachment);
						time = getNumber(keyMap, "time", 0);
						for (frame = 0, bezier = 0;; frame++) {
							var deform : Vector.<Number>;
							var verticesValue : Object = keyMap["vertices"];
							if (verticesValue == null)
								deform = weighted ? new Vector.<Number>(deformLength, true) : vertices;
							else {
								deform = new Vector.<Number>(deformLength, true);
								var start : int = Number(keyMap["offset"] || 0);
								var temp : Vector.<Number> = getFloatArray(keyMap, "vertices", 1);
								for (i = 0; i < temp.length; i++)
									deform[start + i] = temp[i];
								if (scale != 1) {
									for (i = start, n = i + temp.length; i < n; i++)
										deform[i] *= scale;
								}
								if (!weighted) {
									for (i = 0; i < deformLength; i++)
										deform[i] += vertices[i];
								}
							}

							deformTimeline.setFrame(frame, time, deform);
							nextMap = timelineMap[frame + 1];
							if (!nextMap) {
								timeline.shrink(bezier);
								break;
							}
							time2 = getNumber(nextMap, "time", 0);
							curve = keyMap.curve;
							if (curve) bezier = readCurve(curve, deformTimeline, bezier, frame, 0, time, time2, 0, 1, 1);
							time = time2;
							keyMap = nextMap;
						}
						timelines.push(deformTimeline);
					}
				}
			}

			// Draw order timelines.
			var drawOrdersp : Array = map["drawOrder"];
			if (drawOrders) {
				var drawOrderTimeline : DrawOrderTimeline = new DrawOrderTimeline(drawOrders.length);
				var slotCount : int = skeletonData.slots.length;
				frame = 0;
				for each (var drawOrderMap : Object in drawOrders) {
					var drawOrder : Vector.<int> = null;
					var offsets : Array = drawOrderMap["offsets"];
					if (offsets) {
						drawOrder = new Vector.<int>(slotCount, true);
						for (i = slotCount - 1; i >= 0; i--)
							drawOrder[i] = -1;
						var unchanged : Vector.<int> = new Vector.<int>(slotCount - offsets.length, true);
						var originalIndex : int = 0, unchangedIndex : int = 0;
						for each (var offsetMap : Object in offsets) {
							slotIndex = skeletonData.findSlotIndex(offsetMap["slot"]);
							if (slotIndex == -1) throw new Error("Slot not found: " + offsetMap["slot"]);
							// Collect unchanged items.
							while (originalIndex != slotIndex)
								unchanged[unchangedIndex++] = originalIndex++;
							// Set changed items.
							drawOrder[originalIndex + offsetMap["offset"]] = originalIndex++;
						}
						// Collect remaining unchanged items.
						while (originalIndex < slotCount)
							unchanged[unchangedIndex++] = originalIndex++;
						// Fill in unchanged items.
						for (i = slotCount - 1; i >= 0; i--)
							if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
					}
					drawOrderTimeline.setFrame(frame++, Number(drawOrderMap["time"] || 0), drawOrder);
				}
				timelines.push(drawOrderTimeline);
			}

			// Event timelines.
			var eventsMap : Array = map["events"];
			if (eventsMap) {
				var eventTimeline : EventTimeline = new EventTimeline(eventsMap.length);
				frame = 0;
				for each (var eventMap : Object in eventsMap) {
					var eventData : EventData = skeletonData.findEvent(eventMap["name"]);
					if (!eventData) throw new Error("Event not found: " + eventMap["name"]);
					var event : Event = new Event(Number(eventMap["time"] || 0), eventData);
					event.intValue = getNumber(eventMap, "int", eventData.intValue);
					event.floatValue = getNumber(eventMap, "float", eventData.floatValue);
					event.stringValue = String(getValue(eventMap, "string", eventData.stringValue));
					if (eventData.audioPath != null) {
						event.volume = getNumber(eventMap, "volume", 1);
						event.balance = getNumber(eventMap, "balance", 0);
					}
					eventTimeline.setFrame(frame++, event);
				}
				timelines.push(eventTimeline);
			}

			var duration : Number = 0;
			for (i = 0, n = timelines.length; i < n; i++)
				duration = Math.max(duration, timelines[i].getDuration());

			skeletonData.animations.push(new Animation(name, timelines, duration));
		}

		static private function readTimeline(keys : Array, timeline : CurveTimeline1, defaultValue : Number, scale : Number) : CurveTimeline1 {
			var keyMap : Object = keys[0];
			var time : Number = getNumber(keyMap, "time", 0);
			var value : Number = getNumber(keyMap, "value", defaultValue) * scale;
			var bezier : int = 0;
			for (var frame : int = 0;; frame++) {
				timeline.setFrame(frame, time, value);
				var nextMap : Object = keys[frame + 1];
				if (!nextMap) break;
				var time2 : Number = getNumber(nextMap, "time", 0);
				var value2 : Number = getNumber(nextMap, "value", defaultValue) * scale;
				var curve : Object = keyMap.curve;
				if (curve) bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value, value2, scale);
				time = time2;
				value = value2;
				keyMap = nextMap;
			}
			timeline.shrink(bezier);
			return timeline;
		}

		static private function readTimeline2(keys : Array, timeline : CurveTimeline2, name1 : String, name2 : String, defaultValue : Number, scale : Number) : CurveTimeline2 {
			var keyMap : Object = keys[0];
			var time : Number = getNumber(keyMap, "time", 0);
			var value1 : Number = getNumber(keyMap, name1, defaultValue) * scale;
			var value2 : Number = getNumber(keyMap, name2, defaultValue) * scale;
			var bezier : int = 0;
			for (var frame : int = 0;; frame++) {
				timeline.setFrame(frame, time, value1, value2);
				var nextMap : Object = keys[frame + 1];
				if (!nextMap) break;
				var time2 : Number = getNumber(nextMap, "time", 0);
				var nvalue1 : Number = getNumber(nextMap, name1, defaultValue) * scale;
				var nvalue2 : Number = getNumber(nextMap, name2, defaultValue) * scale;
				var curve : Object = keyMap.curve;
				if (curve != null) {
					bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value1, nvalue1, scale);
					bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, value2, nvalue2, scale);
				}
				time = time2;
				value1 = nvalue1;
				value2 = nvalue2;
				keyMap = nextMap;
			}
			timeline.shrink(bezier);
			return timeline;
		}

		static private function readCurve(curve : Object, timeline : CurveTimeline, bezier : int, frame : int, value : Number, time1 : Number, time2 : Number,
			value1 : Number, value2 : Number, scale : Number) : int {
			if (curve == "stepped") {
				if (value != 0) timeline.setStepped(frame);
				return bezier;
			}
			var i : int = value << 2;
			var cx1 : Number = curve[i];
			var cy1 : Number = curve[i + 1] * scale;
			var cx2 : Number = curve[i + 2];
			var cy2 : Number = curve[i + 3] * scale;
			timeline.setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
			return bezier + 1;
		}

		static private function getValue(map : Object, property : String, defaultValue : Object) : Object {
			return map.hasOwnProperty(property) ? map[property] : defaultValue;
		}

		static private function getNumber(map : Object, property : String, defaultValue : Number) : Number {
			return map.hasOwnProperty(property) ? Number(map[property]) : defaultValue;
		}

		static private function getFloatArray(map : Object, name : String, scale : Number) : Vector.<Number> {
			var list : Array = map[name];
			var values : Vector.<Number> = new Vector.<Number>(list.length, true);
			var i : int = 0, n : int = list.length;
			if (scale == 1) {
				for (; i < n; i++)
					values[i] = list[i];
			} else {
				for (; i < n; i++)
					values[i] = list[i] * scale;
			}
			return values;
		}

		static private function getIntArray(map : Object, name : String) : Vector.<int> {
			var list : Array = map[name];
			var values : Vector.<int> = new Vector.<int>(list.length, true);
			for (var i : int = 0, n : int = list.length; i < n; i++)
				values[i] = int(list[i]);
			return values;
		}

		static private function getUintArray(map : Object, name : String) : Vector.<uint> {
			var list : Array = map[name];
			var values : Vector.<uint> = new Vector.<uint>(list.length, true);
			for (var i : int = 0, n : int = list.length; i < n; i++)
				values[i] = int(list[i]);
			return values;
		}
	}
}

import spine.attachments.MeshAttachment;

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
