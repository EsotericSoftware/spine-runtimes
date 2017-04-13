/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
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
				boneData.scaleX = boneMap.hasOwnProperty("scaleX") ? boneMap["scaleX"] : 1;
				boneData.scaleY = boneMap.hasOwnProperty("scaleY") ? boneMap["scaleY"] : 1;
				boneData.shearX = Number(boneMap["shearX"] || 0);
				boneData.shearY = Number(boneMap["shearY"] || 0);
				boneData.transformMode = TransformMode[boneMap["transform"] || "normal"];
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
				if (color) {
					slotData.color.setFrom(toColor(color, 0), toColor(color, 1), toColor(color, 2), toColor(color, 3));
				}

				var dark : String = slotMap["dark"];
				if (dark) {
					slotData.darkColor = new Color(toColor(dark, 0), toColor(dark, 1), toColor(dark, 2), 0);					
				}

				slotData.attachmentName = slotMap["attachment"];
				slotData.blendMode = BlendMode[slotMap["blend"] || "normal"];
				skeletonData.slots.push(slotData);
			}

			// IK constraints.
			for each (var constraintMap : Object in root["ik"]) {
				var ikConstraintData : IkConstraintData = new IkConstraintData(constraintMap["name"]);
				ikConstraintData.order = constraintMap["order"] || 0;

				for each (boneName in constraintMap["bones"]) {
					var bone : BoneData = skeletonData.findBone(boneName);
					if (!bone) throw new Error("IK constraint bone not found: " + boneName);
					ikConstraintData.bones.push(bone);
				}

				ikConstraintData.target = skeletonData.findBone(constraintMap["target"]);
				if (!ikConstraintData.target) throw new Error("Target bone not found: " + constraintMap["target"]);

				ikConstraintData.bendDirection = (!constraintMap.hasOwnProperty("bendPositive") || constraintMap["bendPositive"]) ? 1 : -1;
				ikConstraintData.mix = constraintMap.hasOwnProperty("mix") ? constraintMap["mix"] : 1;

				skeletonData.ikConstraints.push(ikConstraintData);
			}

			// Transform constraints.
			for each (constraintMap in root["transform"]) {
				var transformConstraintData : TransformConstraintData = new TransformConstraintData(constraintMap["name"]);
				transformConstraintData.order = constraintMap["order"] || 0;

				for each (boneName in constraintMap["bones"]) {
					bone = skeletonData.findBone(boneName);
					if (!bone) throw new Error("Transform constraint bone not found: " + boneName);
					transformConstraintData.bones.push(bone);
				}

				transformConstraintData.target = skeletonData.findBone(constraintMap["target"]);
				if (!transformConstraintData.target) throw new Error("Target bone not found: " + constraintMap["target"]);

				transformConstraintData.local = constraintMap.hasOwnProperty("local") ? Boolean(constraintMap["local"]) : false;
				transformConstraintData.relative = constraintMap.hasOwnProperty("relative") ? Boolean(constraintMap["relative"]) : false;

				transformConstraintData.offsetRotation = Number(constraintMap["rotation"] || 0);
				transformConstraintData.offsetX = Number(constraintMap["x"] || 0) * scale;
				transformConstraintData.offsetY = Number(constraintMap["y"] || 0) * scale;
				transformConstraintData.offsetScaleX = Number(constraintMap["scaleX"] || 0);
				transformConstraintData.offsetScaleY = Number(constraintMap["scaleY"] || 0);
				transformConstraintData.offsetShearY = Number(constraintMap["shearY"] || 0);

				transformConstraintData.rotateMix = constraintMap.hasOwnProperty("rotateMix") ? constraintMap["rotateMix"] : 1;
				transformConstraintData.translateMix = constraintMap.hasOwnProperty("translateMix") ? constraintMap["translateMix"] : 1;
				transformConstraintData.scaleMix = constraintMap.hasOwnProperty("scaleMix") ? constraintMap["scaleMix"] : 1;
				transformConstraintData.shearMix = constraintMap.hasOwnProperty("shearMix") ? constraintMap["shearMix"] : 1;

				skeletonData.transformConstraints.push(transformConstraintData);
			}

			// Path constraints.
			for each (constraintMap in root["path"]) {
				var pathConstraintData : PathConstraintData = new PathConstraintData(constraintMap["name"]);
				pathConstraintData.order = constraintMap["order"] || 0;

				for each (boneName in constraintMap["bones"]) {
					bone = skeletonData.findBone(boneName);
					if (!bone) throw new Error("Path constraint bone not found: " + boneName);
					pathConstraintData.bones.push(bone);
				}

				pathConstraintData.target = skeletonData.findSlot(constraintMap["target"]);
				if (!pathConstraintData.target) throw new Error("Path target slot not found: " + constraintMap["target"]);

				pathConstraintData.positionMode = PositionMode[constraintMap["positionMode"] || "percent"];
				pathConstraintData.spacingMode = SpacingMode[constraintMap["spacingMode"] || "length"];
				pathConstraintData.rotateMode = RotateMode[constraintMap["rotateMode"] || "tangent"];
				pathConstraintData.offsetRotation = Number(constraintMap["rotation"] || 0);
				pathConstraintData.position = Number(constraintMap["position"] || 0);
				if (pathConstraintData.positionMode == PositionMode.fixed) pathConstraintData.position *= scale;
				pathConstraintData.spacing = Number(constraintMap["spacing"] || 0);
				if (pathConstraintData.spacingMode == SpacingMode.length || pathConstraintData.spacingMode == SpacingMode.fixed) pathConstraintData.spacing *= scale;
				pathConstraintData.rotateMix = constraintMap.hasOwnProperty("rotateMix") ? constraintMap["rotateMix"] : 1;
				pathConstraintData.translateMix = constraintMap.hasOwnProperty("translateMix") ? constraintMap["translateMix"] : 1;

				skeletonData.pathConstraints.push(pathConstraintData);
			}

			// Skins.
			var skins : Object = root["skins"];
			for (var skinName : String in skins) {
				var skinMap : Object = skins[skinName];
				var skin : Skin = new Skin(skinName);
				for (slotName in skinMap) {
					var slotIndex : int = skeletonData.findSlotIndex(slotName);
					var slotEntry : Object = skinMap[slotName];
					for (var attachmentName : String in slotEntry) {
						var attachment : Attachment = readAttachment(slotEntry[attachmentName], skin, slotIndex, attachmentName, skeletonData);
						if (attachment != null)
							skin.addAttachment(slotIndex, attachmentName, attachment);
					}
				}
				skeletonData.skins[skeletonData.skins.length] = skin;
				if (skin.name == "default")
					skeletonData.defaultSkin = skin;
			}

			// Linked meshes.
			var linkedMeshes : Vector.<LinkedMesh> = this.linkedMeshes;
			for each (var linkedMesh : LinkedMesh in linkedMeshes) {
				var parentSkin : Skin = !linkedMesh.skin ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
				if (!parentSkin) throw new Error("Skin not found: " + linkedMesh.skin);
				var parentMesh : Attachment = parentSkin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (!parentMesh) throw new Error("Parent mesh not found: " + linkedMesh.parent);
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

			var typeName : String = map["type"] || "region";
			var type : AttachmentType = AttachmentType[typeName];

			var scale : Number = this.scale;
			var color : String;
			switch (type) {
				case AttachmentType.region:
					var region : RegionAttachment = attachmentLoader.newRegionAttachment(skin, name, map["path"] || name);
					if (!region) return null;
					region.path = map["path"] || name;
					region.x = Number(map["x"] || 0) * scale;
					region.y = Number(map["y"] || 0) * scale;
					region.scaleX = map.hasOwnProperty("scaleX") ? map["scaleX"] : 1;
					region.scaleY = map.hasOwnProperty("scaleY") ? map["scaleY"] : 1;
					region.rotation = map["rotation"] || 0;
					region.width = Number(map["width"] || 0) * scale;
					region.height = Number(map["height"] || 0) * scale;
					color = map["color"];
					if (color) {
						region.color.setFrom(toColor(color, 0), toColor(color, 1), toColor(color, 2), toColor(color, 3));
					}
					region.updateOffset();
					return region;
				case AttachmentType.mesh:
				case AttachmentType.linkedmesh:
					var mesh : MeshAttachment = attachmentLoader.newMeshAttachment(skin, name, map["path"] || name);
					if (!mesh) return null;
					mesh.path = map["path"] || name;
					color = map["color"];
					if (color) {
						mesh.color.setFrom(toColor(color, 0), toColor(color, 1), toColor(color, 2), toColor(color, 3));
					}
					mesh.width = Number(map["width"] || 0) * scale;
					mesh.height = Number(map["height"] || 0) * scale;
					if (map["parent"]) {
						mesh.inheritDeform = map.hasOwnProperty("deform") ? Boolean(map["deform"]) : true;
						linkedMeshes.push(new LinkedMesh(mesh, map["skin"], slotIndex, map["parent"]));
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
					path.closed = map.hasOwnProperty("closed") ? Boolean(map["closed"]) : false;
					path.constantSpeed = map.hasOwnProperty("constantSpeed") ? Boolean(map["constantSpeed"]) : true;
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
					point.x = map.hasOwnProperty("x") ? Number(map["x"]) * scale : 0;
					point.y = map.hasOwnProperty("y") ? Number(map["y"]) * scale : 0;
					point.rotation = map.hasOwnProperty("rotation") ? Number(map["rotation"]) : 0;
					color = map["color"];
					if (color) {
						point.color.setFrom(toColor(color, 0), toColor(color, 1), toColor(color, 2), toColor(color, 3));
					}
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
					if (color) {
						clip.color.setFrom(toColor(color, 0), toColor(color, 1), toColor(color, 2), toColor(color, 3));
					}
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
			var duration : Number = 0;

			var slotMap : Object, slotIndex : int, slotName : String;
			var values : Array, valueMap : Object, frameIndex : int;
			var i : int;
			var timelineName : String;

			var slots : Object = map["slots"];
			for (slotName in slots) {
				slotMap = slots[slotName];
				slotIndex = skeletonData.findSlotIndex(slotName);

				for (timelineName in slotMap) {
					values = slotMap[timelineName];
					if (timelineName == "attachment") {
						var attachmentTimeline : AttachmentTimeline = new AttachmentTimeline(values.length);
						attachmentTimeline.slotIndex = slotIndex;

						frameIndex = 0;
						for each (valueMap in values)
							attachmentTimeline.setFrame(frameIndex++, valueMap["time"], valueMap["name"]);
						timelines[timelines.length] = attachmentTimeline;
						duration = Math.max(duration, attachmentTimeline.frames[attachmentTimeline.frameCount - 1]);
					} else if (timelineName == "color") {
						var colorTimeline : ColorTimeline = new ColorTimeline(values.length);
						colorTimeline.slotIndex = slotIndex;

						frameIndex = 0;
						for each (valueMap in values) {
							var color : String = valueMap["color"];
							var r : Number = toColor(color, 0);
							var g : Number = toColor(color, 1);
							var b : Number = toColor(color, 2);
							var a : Number = toColor(color, 3);
							colorTimeline.setFrame(frameIndex, valueMap["time"], r, g, b, a);
							readCurve(valueMap, colorTimeline, frameIndex);
							frameIndex++;
						}
						timelines[timelines.length] = colorTimeline;
						duration = Math.max(duration, colorTimeline.frames[(colorTimeline.frameCount - 1) * ColorTimeline.ENTRIES]);
					} else if (timelineName == "twoColor") {
						var twoColorTimeline : TwoColorTimeline = new TwoColorTimeline(values.length);
						twoColorTimeline.slotIndex = slotIndex;

						frameIndex = 0;
						for each (valueMap in values) {
							color = valueMap["light"];
							var darkColor : String = valueMap["dark"];
							var light : Color = new Color(0, 0, 0, 0);
							var dark : Color = new Color(0, 0, 0, 0);
							light.setFrom(toColor(color, 0), toColor(color, 1), toColor(color, 2), toColor(color, 3));
							dark.setFrom(toColor(darkColor, 0), toColor(darkColor, 1), toColor(darkColor, 2), toColor(darkColor, 3));
							twoColorTimeline.setFrame(frameIndex, valueMap["time"], light.r, light.g, light.b, light.a, dark.r, dark.g, dark.b);
							readCurve(valueMap, twoColorTimeline, frameIndex);
							frameIndex++;
						}
						timelines[timelines.length] = twoColorTimeline;
						duration = Math.max(duration, twoColorTimeline.frames[(twoColorTimeline.frameCount - 1) * TwoColorTimeline.ENTRIES]);
					} else
						throw new Error("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
				}
			}

			var bones : Object = map["bones"];
			for (var boneName : String in bones) {
				var boneIndex : int = skeletonData.findBoneIndex(boneName);
				if (boneIndex == -1) throw new Error("Bone not found: " + boneName);
				var boneMap : Object = bones[boneName];

				for (timelineName in boneMap) {
					values = boneMap[timelineName];
					if (timelineName == "rotate") {
						var rotateTimeline : RotateTimeline = new RotateTimeline(values.length);
						rotateTimeline.boneIndex = boneIndex;

						frameIndex = 0;
						for each (valueMap in values) {
							rotateTimeline.setFrame(frameIndex, valueMap["time"], valueMap["angle"]);
							readCurve(valueMap, rotateTimeline, frameIndex);
							frameIndex++;
						}
						timelines[timelines.length] = rotateTimeline;
						duration = Math.max(duration, rotateTimeline.frames[(rotateTimeline.frameCount - 1) * RotateTimeline.ENTRIES]);
					} else if (timelineName == "translate" || timelineName == "scale" || timelineName == "shear") {
						var translateTimeline : TranslateTimeline;
						var timelineScale : Number = 1;
						if (timelineName == "scale")
							translateTimeline = new ScaleTimeline(values.length);
						else if (timelineName == "shear")
							translateTimeline = new ShearTimeline(values.length);
						else {
							translateTimeline = new TranslateTimeline(values.length);
							timelineScale = scale;
						}
						translateTimeline.boneIndex = boneIndex;

						frameIndex = 0;
						for each (valueMap in values) {
							var x : Number = Number(valueMap["x"] || 0) * timelineScale;
							var y : Number = Number(valueMap["y"] || 0) * timelineScale;
							translateTimeline.setFrame(frameIndex, valueMap["time"], x, y);
							readCurve(valueMap, translateTimeline, frameIndex);
							frameIndex++;
						}
						timelines[timelines.length] = translateTimeline;
						duration = Math.max(duration, translateTimeline.frames[(translateTimeline.frameCount - 1) * TranslateTimeline.ENTRIES]);
					} else
						throw new Error("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
				}
			}

			var ikMap : Object = map["ik"];
			for (var ikConstraintName : String in ikMap) {
				var ikConstraint : IkConstraintData = skeletonData.findIkConstraint(ikConstraintName);
				values = ikMap[ikConstraintName];
				var ikTimeline : IkConstraintTimeline = new IkConstraintTimeline(values.length);
				ikTimeline.ikConstraintIndex = skeletonData.ikConstraints.indexOf(ikConstraint);
				frameIndex = 0;
				for each (valueMap in values) {
					var mix : Number = valueMap.hasOwnProperty("mix") ? valueMap["mix"] : 1;
					var bendDirection : int = (!valueMap.hasOwnProperty("bendPositive") || valueMap["bendPositive"]) ? 1 : -1;
					ikTimeline.setFrame(frameIndex, valueMap["time"], mix, bendDirection);
					readCurve(valueMap, ikTimeline, frameIndex);
					frameIndex++;
				}
				timelines[timelines.length] = ikTimeline;
				duration = Math.max(duration, ikTimeline.frames[(ikTimeline.frameCount - 1) * IkConstraintTimeline.ENTRIES]);
			}

			var transformMap : Object = map["transform"];
			for (var transformName : String in transformMap) {
				var transformConstraint : TransformConstraintData = skeletonData.findTransformConstraint(transformName);
				values = transformMap[transformName];
				var transformTimeline : TransformConstraintTimeline = new TransformConstraintTimeline(values.length);
				transformTimeline.transformConstraintIndex = skeletonData.transformConstraints.indexOf(transformConstraint);
				frameIndex = 0;
				for each (valueMap in values) {
					var rotateMix : Number = valueMap.hasOwnProperty("rotateMix") ? valueMap["rotateMix"] : 1;
					var translateMix : Number = valueMap.hasOwnProperty("translateMix") ? valueMap["translateMix"] : 1;
					var scaleMix : Number = valueMap.hasOwnProperty("scaleMix") ? valueMap["scaleMix"] : 1;
					var shearMix : Number = valueMap.hasOwnProperty("shearMix") ? valueMap["shearMix"] : 1;
					transformTimeline.setFrame(frameIndex, valueMap["time"], rotateMix, translateMix, scaleMix, shearMix);
					readCurve(valueMap, transformTimeline, frameIndex);
					frameIndex++;
				}
				timelines.push(transformTimeline);
				duration = Math.max(duration, transformTimeline.frames[(transformTimeline.frameCount - 1) * TransformConstraintTimeline.ENTRIES]);
			}

			// Path constraint timelines.
			var paths : Object = map["paths"];
			for (var pathName : String in paths) {
				var index : int = skeletonData.findPathConstraintIndex(pathName);
				if (index == -1) throw new Error("Path constraint not found: " + pathName);
				var data : PathConstraintData = skeletonData.pathConstraints[index];

				var pathMap : Object = paths[pathName];
				for (timelineName in pathMap) {
					values = pathMap[timelineName];

					if (timelineName == "position" || timelineName == "spacing") {
						var pathTimeline : PathConstraintPositionTimeline;
						timelineScale = 1;
						if (timelineName == "spacing") {
							pathTimeline = new PathConstraintSpacingTimeline(values.length);
							if (data.spacingMode == SpacingMode.length || data.spacingMode == SpacingMode.fixed) timelineScale = scale;
						} else {
							pathTimeline = new PathConstraintPositionTimeline(values.length);
							if (data.positionMode == PositionMode.fixed) timelineScale = scale;
						}
						pathTimeline.pathConstraintIndex = index;
						frameIndex = 0;
						for each (valueMap in values) {
							var value : Number = valueMap[timelineName] || 0;
							pathTimeline.setFrame(frameIndex, valueMap["time"], value * timelineScale);
							readCurve(valueMap, pathTimeline, frameIndex);
							frameIndex++;
						}
						timelines.push(pathTimeline);
						duration = Math.max(duration, pathTimeline.frames[(pathTimeline.frameCount - 1) * PathConstraintPositionTimeline.ENTRIES]);
					} else if (timelineName == "mix") {
						var pathMixTimeline : PathConstraintMixTimeline = new PathConstraintMixTimeline(values.length);
						pathMixTimeline.pathConstraintIndex = index;
						frameIndex = 0;
						for each (valueMap in values) {
							rotateMix = valueMap.hasOwnProperty("rotateMix") ? valueMap["rotateMix"] : 1;
							translateMix = valueMap.hasOwnProperty("translateMix") ? valueMap["translateMix"] : 1;
							pathMixTimeline.setFrame(frameIndex, valueMap["time"], rotateMix, translateMix);
							readCurve(valueMap, pathMixTimeline, frameIndex);
							frameIndex++;
						}
						timelines.push(pathMixTimeline);
						duration = Math.max(duration, pathMixTimeline.frames[(pathMixTimeline.frameCount - 1) * PathConstraintMixTimeline.ENTRIES]);
					}
				}
			}

			var deformMap : Object = map["deform"];
			for (var skinName : String in deformMap) {
				var skin : Skin = skeletonData.findSkin(skinName);
				slotMap = deformMap[skinName];
				for (slotName in slotMap) {
					slotIndex = skeletonData.findSlotIndex(slotName);
					var timelineMap : Object = slotMap[slotName];
					for (timelineName in timelineMap) {
						values = timelineMap[timelineName];

						var attachment : VertexAttachment = skin.getAttachment(slotIndex, timelineName) as VertexAttachment;
						if (attachment == null) throw new Error("Deform attachment not found: " + timelineName);
						var weighted : Boolean = attachment.bones != null;
						var vertices : Vector.<Number> = attachment.vertices;
						var deformLength : int = weighted ? vertices.length / 3 * 2 : vertices.length;

						var deformTimeline : DeformTimeline = new DeformTimeline(values.length);
						deformTimeline.slotIndex = slotIndex;
						deformTimeline.attachment = attachment;

						frameIndex = 0;
						for each (valueMap in values) {
							var deform : Vector.<Number>;
							var verticesValue : Object = valueMap["vertices"];
							if (verticesValue == null)
								deform = weighted ? new Vector.<Number>(deformLength, true) : vertices;
							else {
								deform = new Vector.<Number>(deformLength, true);
								var start : int = Number(valueMap["offset"] || 0);
								var temp : Vector.<Number> = getFloatArray(valueMap, "vertices", 1);
								for (i = 0; i < temp.length; i++) {
									deform[start + i] = temp[i];
								}
								if (scale != 1) {
									var n : int;
									for (i = start, n = i + temp.length; i < n; i++)
										deform[i] *= scale;
								}
								if (!weighted) {
									for (i = 0; i < deformLength; i++)
										deform[i] += vertices[i];
								}
							}

							deformTimeline.setFrame(frameIndex, valueMap["time"], deform);
							readCurve(valueMap, deformTimeline, frameIndex);
							frameIndex++;
						}
						timelines[timelines.length] = deformTimeline;
						duration = Math.max(duration, deformTimeline.frames[deformTimeline.frameCount - 1]);
					}
				}
			}

			var drawOrderValues : Array = map["drawOrder"];
			if (!drawOrderValues) drawOrderValues = map["draworder"];
			if (drawOrderValues) {
				var drawOrderTimeline : DrawOrderTimeline = new DrawOrderTimeline(drawOrderValues.length);
				var slotCount : int = skeletonData.slots.length;
				frameIndex = 0;
				for each (var drawOrderMap : Object in drawOrderValues) {
					var drawOrder : Vector.<int> = null;
					if (drawOrderMap["offsets"]) {
						drawOrder = new Vector.<int>(slotCount);
						for (i = slotCount - 1; i >= 0; i--)
							drawOrder[i] = -1;
						var offsets : Array = drawOrderMap["offsets"];
						var unchanged : Vector.<int> = new Vector.<int>(slotCount - offsets.length);
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
					drawOrderTimeline.setFrame(frameIndex++, drawOrderMap["time"], drawOrder);
				}
				timelines[timelines.length] = drawOrderTimeline;
				duration = Math.max(duration, drawOrderTimeline.frames[drawOrderTimeline.frameCount - 1]);
			}

			var eventsMap : Array = map["events"];
			if (eventsMap) {
				var eventTimeline : EventTimeline = new EventTimeline(eventsMap.length);
				frameIndex = 0;
				for each (var eventMap : Object in eventsMap) {
					var eventData : EventData = skeletonData.findEvent(eventMap["name"]);
					if (!eventData) throw new Error("Event not found: " + eventMap["name"]);
					var event : Event = new Event(eventMap["time"], eventData);
					event.intValue = eventMap.hasOwnProperty("int") ? eventMap["int"] : eventData.intValue;
					event.floatValue = eventMap.hasOwnProperty("float") ? eventMap["float"] : eventData.floatValue;
					event.stringValue = eventMap.hasOwnProperty("string") ? eventMap["string"] : eventData.stringValue;
					eventTimeline.setFrame(frameIndex++, event);
				}
				timelines[timelines.length] = eventTimeline;
				duration = Math.max(duration, eventTimeline.frames[eventTimeline.frameCount - 1]);
			}

			skeletonData.animations[skeletonData.animations.length] = new Animation(name, timelines, duration);
		}

		static private function readCurve(map : Object, timeline : CurveTimeline, frameIndex : int) : void {
			var curve : Object = map["curve"];
			if (!curve) return;
			if (curve == "stepped")
				timeline.setStepped(frameIndex);
			else if (curve is Array)
				timeline.setCurve(frameIndex, curve[0], curve[1], curve[2], curve[3]);
		}

		static private function toColor(hexString : String, colorIndex : int) : Number {
			if (hexString.length != 8 && hexString.length != 6) throw new ArgumentError("Color hexidecimal length must be 6 or 8, received: " + hexString);
			return parseInt(hexString.substring(colorIndex * 2, colorIndex * 2 + 2), 16) / 255;			
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

	public function LinkedMesh(mesh : MeshAttachment, skin : String, slotIndex : int, parent : String) {
		this.mesh = mesh;
		this.skin = skin;
		this.slotIndex = slotIndex;
		this.parent = parent;
	}
}