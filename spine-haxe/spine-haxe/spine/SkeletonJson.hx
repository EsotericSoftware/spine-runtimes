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

import Reflect;
import haxe.Json;
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

class SkeletonJson {
	public var attachmentLoader:AttachmentLoader;
	public var scale:Float = 1;

	private var linkedMeshes:Array<LinkedMesh> = new Array<LinkedMesh>();

	public function new(attachmentLoader:AttachmentLoader) {
		this.attachmentLoader = attachmentLoader;
	}

	public function readSkeletonData(json:String):SkeletonData {
		if (json == null)
			throw new SpineException("object cannot be null.");

		var root = Json.parse(json);

		var skeletonData:SkeletonData = new SkeletonData();

		// Skeleton.
		var skeletonMap = getString(root, "skeleton", "");
		if (skeletonMap != null) {
			skeletonData.hash = getString(skeletonMap, "hash", "");
			skeletonData.version = getString(skeletonMap, "spine", "");
			skeletonData.x = getFloat(skeletonMap, "x");
			skeletonData.y = getFloat(skeletonMap, "y");
			skeletonData.width = getFloat(skeletonMap, "width");
			skeletonData.height = getFloat(skeletonMap, "height");
			skeletonData.referenceScale = getFloat(skeletonMap, "referenceScale", 100);
			skeletonData.fps = getFloat(skeletonMap, "fps");
			skeletonData.imagesPath = getString(skeletonMap, "images", "");
			skeletonData.audioPath = getString(skeletonMap, "audio", "");
		}

		// Bones.
		var boneData:BoneData;
		for (boneMap in cast(Reflect.getProperty(root, "bones"), Array<Dynamic>)) {
			var parent:BoneData = null;
			var parentName:String = Reflect.getProperty(boneMap, "parent");
			if (parentName != null) {
				parent = skeletonData.findBone(parentName);
				if (parent == null)
					throw new SpineException("Parent bone not found: " + parentName);
			}
			boneData = new BoneData(skeletonData.bones.length, Reflect.getProperty(boneMap, "name"), parent);
			boneData.length = getFloat(boneMap, "length") * scale;
			boneData.x = getFloat(boneMap, "x") * scale;
			boneData.y = getFloat(boneMap, "y") * scale;
			boneData.rotation = getFloat(boneMap, "rotation");
			boneData.scaleX = getFloat(boneMap, "scaleX", 1);
			boneData.scaleY = getFloat(boneMap, "scaleY", 1);
			boneData.shearX = getFloat(boneMap, "shearX");
			boneData.shearY = getFloat(boneMap, "shearY");
			boneData.inherit = Reflect.hasField(boneMap,
				"inherit") ? Inherit.fromName(Reflect.getProperty(boneMap, "inherit")) : Inherit.normal;
			boneData.skinRequired = Reflect.hasField(boneMap, "skin") ? cast(Reflect.getProperty(boneMap, "skin"), Bool) : false;

			var color:String = Reflect.getProperty(boneMap, "color");
			if (color != null) {
				boneData.color.setFromString(color);
			}

			skeletonData.bones.push(boneData);
		}

		// Slots.
		for (slotMap in cast(Reflect.getProperty(root, "slots"), Array<Dynamic>)) {
			var path:String = null;
			var slotName:String = Reflect.getProperty(slotMap, "name");

			var boneName:String = Reflect.getProperty(slotMap, "bone");
			boneData = skeletonData.findBone(boneName);
			if (boneData == null)
				throw new SpineException("Slot bone not found: " + boneName);
			var slotData:SlotData = new SlotData(skeletonData.slots.length, slotName, boneData);

			var color:String = Reflect.getProperty(slotMap, "color");
			if (color != null) {
				slotData.color.setFromString(color);
			}

			var dark:String = Reflect.getProperty(slotMap, "dark");
			if (dark != null) {
				slotData.darkColor = new Color(0, 0, 0);
				slotData.darkColor.setFromString(dark);
			}

			slotData.attachmentName = Reflect.getProperty(slotMap, "attachment");
			slotData.blendMode = Reflect.hasField(slotMap, "blend") ? BlendMode.fromName(Reflect.getProperty(slotMap, "blend")) : BlendMode.normal;
			slotData.visible = getValue(slotMap, "visible", true);
			skeletonData.slots.push(slotData);
		}

		// IK constraints.
		if (Reflect.hasField(root, "ik")) {
			for (constraintMap in cast(Reflect.getProperty(root, "ik"), Array<Dynamic>)) {
				var ikData:IkConstraintData = new IkConstraintData(Reflect.getProperty(constraintMap, "name"));
				ikData.order = getInt(constraintMap, "order");
				ikData.skinRequired = Reflect.hasField(constraintMap, "skin") ? cast(Reflect.getProperty(constraintMap, "skin"), Bool) : false;

				for (boneName in cast(Reflect.getProperty(constraintMap, "bones"), Array<Dynamic>)) {
					var bone:BoneData = skeletonData.findBone(boneName);
					if (bone == null)
						throw new SpineException("IK constraint bone not found: " + boneName);
					ikData.bones.push(bone);
				}

				ikData.target = skeletonData.findBone(Reflect.getProperty(constraintMap, "target"));
				if (ikData.target == null)
					throw new SpineException("Target bone not found: " + Reflect.getProperty(constraintMap, "target"));

				ikData.mix = getFloat(constraintMap, "mix", 1);
				ikData.softness = getFloat(constraintMap, "softness", 0) * scale;
				ikData.bendDirection = (!Reflect.hasField(constraintMap, "bendPositive")
					|| cast(Reflect.getProperty(constraintMap, "bendPositive"), Bool)) ? 1 : -1;
				ikData.compress = (Reflect.hasField(constraintMap, "compress")
					&& cast(Reflect.getProperty(constraintMap, "compress"), Bool));
				ikData.stretch = (Reflect.hasField(constraintMap, "stretch") && cast(Reflect.getProperty(constraintMap, "stretch"), Bool));
				ikData.uniform = (Reflect.hasField(constraintMap, "uniform") && cast(Reflect.getProperty(constraintMap, "uniform"), Bool));

				skeletonData.ikConstraints.push(ikData);
			}
		}

		// Transform constraints.
		if (Reflect.hasField(root, "transform")) {
			for (constraintMap in cast(Reflect.getProperty(root, "transform"), Array<Dynamic>)) {
				var transformData:TransformConstraintData = new TransformConstraintData(Reflect.getProperty(constraintMap, "name"));
				transformData.order = getInt(constraintMap, "order");
				transformData.skinRequired = Reflect.hasField(constraintMap, "skin") ? cast(Reflect.getProperty(constraintMap, "skin"), Bool) : false;

				for (boneName in cast(Reflect.getProperty(constraintMap, "bones"), Array<Dynamic>)) {
					var bone = skeletonData.findBone(boneName);
					if (bone == null)
						throw new SpineException("Transform constraint bone not found: " + boneName);
					transformData.bones.push(bone);
				}

				transformData.target = skeletonData.findBone(Reflect.getProperty(constraintMap, "target"));
				if (transformData.target == null)
					throw new SpineException("Target bone not found: " + Reflect.getProperty(constraintMap, "target"));

				transformData.local = Reflect.hasField(constraintMap, "local") ? cast(Reflect.getProperty(constraintMap, "local"), Bool) : false;
				transformData.relative = Reflect.hasField(constraintMap, "relative") ? cast(Reflect.getProperty(constraintMap, "relative"), Bool) : false;

				transformData.offsetRotation = getFloat(constraintMap, "rotation");
				transformData.offsetX = getFloat(constraintMap, "x") * scale;

				transformData.offsetY = getFloat(constraintMap, "y") * scale;

				transformData.offsetScaleX = getFloat(constraintMap, "scaleX");
				transformData.offsetScaleY = getFloat(constraintMap, "scaleY");
				transformData.offsetShearY = getFloat(constraintMap, "shearY");

				transformData.mixRotate = getFloat(constraintMap, "mixRotate", 1);
				transformData.mixX = getFloat(constraintMap, "mixX", 1);
				transformData.mixY = getFloat(constraintMap, "mixY", transformData.mixX);
				transformData.mixScaleX = getFloat(constraintMap, "mixScaleX", 1);
				transformData.mixScaleY = getFloat(constraintMap, "mixScaleY", transformData.mixScaleX);
				transformData.mixShearY = getFloat(constraintMap, "mixShearY", 1);

				skeletonData.transformConstraints.push(transformData);
			}
		}

		// Path constraints.
		if (Reflect.hasField(root, "path")) {
			for (constraintMap in cast(Reflect.getProperty(root, "path"), Array<Dynamic>)) {
				var pathData:PathConstraintData = new PathConstraintData(Reflect.getProperty(constraintMap, "name"));
				pathData.order = getInt(constraintMap, "order");
				pathData.skinRequired = Reflect.hasField(constraintMap, "skin") ? cast(Reflect.getProperty(constraintMap, "skin"), Bool) : false;

				for (boneName in cast(Reflect.getProperty(constraintMap, "bones"), Array<Dynamic>)) {
					var bone = skeletonData.findBone(boneName);
					if (bone == null)
						throw new SpineException("Path constraint bone not found: " + boneName);
					pathData.bones.push(bone);
				}

				pathData.target = skeletonData.findSlot(Reflect.getProperty(constraintMap, "target"));
				if (pathData.target == null)
					throw new SpineException("Path target slot not found: " + Reflect.getProperty(constraintMap, "target"));

				pathData.positionMode = Reflect.hasField(constraintMap,
					"positionMode") ? PositionMode.fromName(Reflect.getProperty(constraintMap, "positionMode")) : PositionMode.percent;
				pathData.spacingMode = Reflect.hasField(constraintMap,
					"spacingMode") ? SpacingMode.fromName(Reflect.getProperty(constraintMap, "spacingMode")) : SpacingMode.length;
				pathData.rotateMode = Reflect.hasField(constraintMap,
					"rotateMode") ? RotateMode.fromName(Reflect.getProperty(constraintMap, "rotateMode")) : RotateMode.tangent;
				pathData.offsetRotation = getFloat(constraintMap, "rotation");
				pathData.position = getFloat(constraintMap, "position");
				if (pathData.positionMode == PositionMode.fixed)
					pathData.position *= scale;
				pathData.spacing = getFloat(constraintMap, "spacing");
				if (pathData.spacingMode == SpacingMode.length || pathData.spacingMode == SpacingMode.fixed)
					pathData.spacing *= scale;
				pathData.mixRotate = getFloat(constraintMap, "mixRotate", 1);
				pathData.mixX = getFloat(constraintMap, "mixX", 1);
				pathData.mixY = getFloat(constraintMap, "mixY", 1);

				skeletonData.pathConstraints.push(pathData);
			}
		}

		// Physics constraints.
		if (Reflect.hasField(root, "physics")) {
			for (constraintMap in cast(Reflect.getProperty(root, "physics"), Array<Dynamic>)) {
				var physicsData:PhysicsConstraintData = new PhysicsConstraintData(Reflect.getProperty(constraintMap, "name"));
				physicsData.order = getInt(constraintMap, "order");
				physicsData.skinRequired = Reflect.hasField(constraintMap, "skin") ? cast(Reflect.getProperty(constraintMap, "skin"), Bool) : false;

				var boneName:String = Reflect.getProperty(constraintMap, "bone");
				var bone = skeletonData.findBone(boneName);
				if (bone == null)
					throw new SpineException("Physics constraint bone not found: " + boneName);
				physicsData.bone = bone;

				physicsData.x = getFloat(constraintMap, "x");
				physicsData.y = getFloat(constraintMap, "y");
				physicsData.rotate = getFloat(constraintMap, "rotate");
				physicsData.scaleX = getFloat(constraintMap, "scaleX");
				physicsData.shearX = getFloat(constraintMap, "shearX");
				physicsData.limit = getFloat(constraintMap, "limit", 5000) * scale;
				physicsData.step = 1 / getFloat(constraintMap, "fps", 60);
				physicsData.inertia = getFloat(constraintMap, "inertia", 1);
				physicsData.strength = getFloat(constraintMap, "strength", 100);
				physicsData.damping = getFloat(constraintMap, "damping", 1);
				physicsData.massInverse = 1 / getFloat(constraintMap, "mass", 1);
				physicsData.wind = getFloat(constraintMap, "wind");
				physicsData.gravity = getFloat(constraintMap, "gravity");
				physicsData.mix = getValue(constraintMap, "mix", 1);
				physicsData.inertiaGlobal = Reflect.hasField(constraintMap, "inertiaGlobal") ? cast(Reflect.getProperty(constraintMap, "inertiaGlobal"), Bool) : false;
				physicsData.strengthGlobal = Reflect.hasField(constraintMap, "strengthGlobal") ? cast(Reflect.getProperty(constraintMap, "strengthGlobal"), Bool) : false;
				physicsData.dampingGlobal = Reflect.hasField(constraintMap, "dampingGlobal") ? cast(Reflect.getProperty(constraintMap, "dampingGlobal"), Bool) : false;
				physicsData.dampingGlobal = Reflect.hasField(constraintMap, "dampingGlobal") ? cast(Reflect.getProperty(constraintMap, "dampingGlobal"), Bool) : false;
				physicsData.windGlobal = Reflect.hasField(constraintMap, "windGlobal") ? cast(Reflect.getProperty(constraintMap, "windGlobal"), Bool) : false;
				physicsData.gravityGlobal = Reflect.hasField(constraintMap, "gravityGlobal") ? cast(Reflect.getProperty(constraintMap, "gravityGlobal"), Bool) : false;
				physicsData.mixGlobal = Reflect.hasField(constraintMap, "mixGlobal") ? cast(Reflect.getProperty(constraintMap, "mixGlobal"), Bool) : false;

				skeletonData.physicsConstraints.push(physicsData);
			}
		}

		// Skins.
		if (Reflect.hasField(root, "skins")) {
			for (skinMap in cast(Reflect.getProperty(root, "skins"), Array<Dynamic>)) {
				var skin:Skin = new Skin(Reflect.getProperty(skinMap, "name"));

				if (Reflect.hasField(skinMap, "bones")) {
					var bones:Array<Dynamic> = cast(Reflect.getProperty(skinMap, "bones"), Array<Dynamic>);
					for (ii in 0...bones.length) {
						var boneData:BoneData = skeletonData.findBone(bones[ii]);
						if (boneData == null)
							throw new SpineException("Skin bone not found: " + bones[ii]);
						skin.bones.push(boneData);
					}
				}

				if (Reflect.hasField(skinMap, "ik")) {
					var ik:Array<Dynamic> = cast(Reflect.getProperty(skinMap, "ik"), Array<Dynamic>);
					for (ii in 0...ik.length) {
						var constraint:ConstraintData = skeletonData.findIkConstraint(ik[ii]);
						if (constraint == null)
							throw new SpineException("Skin IK constraint not found: " + ik[ii]);
						skin.constraints.push(constraint);
					}
				}

				if (Reflect.hasField(skinMap, "transform")) {
					var transform:Array<Dynamic> = cast(Reflect.getProperty(skinMap, "transform"), Array<Dynamic>);
					for (ii in 0...transform.length) {
						var constraint:ConstraintData = skeletonData.findTransformConstraint(transform[ii]);
						if (constraint == null)
							throw new SpineException("Skin transform constraint not found: " + transform[ii]);
						skin.constraints.push(constraint);
					}
				}

				if (Reflect.hasField(skinMap, "path")) {
					var path:Array<Dynamic> = cast(Reflect.getProperty(skinMap, "path"), Array<Dynamic>);
					for (ii in 0...path.length) {
						var constraint:ConstraintData = skeletonData.findPathConstraint(path[ii]);
						if (constraint == null)
							throw new SpineException("Skin path constraint not found: " + path[ii]);
						skin.constraints.push(constraint);
					}
				}

				if (Reflect.hasField(skinMap, "physics")) {
					var physics:Array<Dynamic> = cast(Reflect.getProperty(skinMap, "physics"), Array<Dynamic>);
					for (ii in 0...physics.length) {
						var constraint:ConstraintData = skeletonData.findPhysicsConstraint(physics[ii]);
						if (constraint == null)
							throw new SpineException("Skin physics constraint not found: " + physics[ii]);
						skin.constraints.push(constraint);
					}
				}

				if (Reflect.hasField(skinMap, "attachments")) {
					var attachments:Dynamic = Reflect.getProperty(skinMap, "attachments");
					for (slotName in Reflect.fields(attachments)) {
						var slot:SlotData = skeletonData.findSlot(slotName);
						var slotEntry:Dynamic = Reflect.getProperty(attachments, slotName);
						for (attachmentName in Reflect.fields(slotEntry)) {
							var attachment:Attachment = readAttachment(Reflect.getProperty(slotEntry, attachmentName), skin, slot.index, attachmentName,
								skeletonData);
							if (attachment != null) {
								skin.setAttachment(slot.index, attachmentName, attachment);
							}
						}
					}
				}

				skeletonData.skins.push(skin);
				if (skin.name == "default") {
					skeletonData.defaultSkin = skin;
				}
			}
		}

		// Linked meshes.
		for (linkedMesh in linkedMeshes) {
			var parentSkin:Skin = linkedMesh.skin == null ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
			if (parentSkin == null)
				throw new SpineException("Skin not found: " + linkedMesh.skin);
			var parentMesh:Attachment = parentSkin.getAttachment(linkedMesh.slotIndex, linkedMesh.parent);
			if (parentMesh == null)
				throw new SpineException("Parent mesh not found: " + linkedMesh.parent);
			linkedMesh.mesh.timelineAttachment = linkedMesh.inheritTimeline ? cast(parentMesh, VertexAttachment) : linkedMesh.mesh;
			linkedMesh.mesh.parentMesh = cast(parentMesh, MeshAttachment);
			if (linkedMesh.mesh.region != null)
				linkedMesh.mesh.updateRegion();
		}
		linkedMeshes.resize(0);

		// Events.
		var events:Dynamic = Reflect.getProperty(root, "events");
		for (eventName in Reflect.fields(events)) {
			var eventMap:Map<String, Dynamic> = Reflect.field(events, eventName);
			var eventData:EventData = new EventData(eventName);
			eventData.intValue = getInt(eventMap, "int");
			eventData.floatValue = getFloat(eventMap, "float");
			eventData.stringValue = getString(eventMap, "string", "");
			eventData.audioPath = getString(eventMap, "audio", "");
			if (eventData.audioPath != null) {
				eventData.volume = getFloat(eventMap, "volume", 1);
				eventData.balance = getFloat(eventMap, "balance");
			}
			skeletonData.events.push(eventData);
		}

		// Animations.
		var animations:Dynamic = Reflect.getProperty(root, "animations");
		for (animationName in Reflect.fields(animations)) {
			readAnimation(Reflect.field(animations, animationName), animationName, skeletonData);
		}
		return skeletonData;
	}

	private function readSequence(map:Dynamic) {
		if (map == null)
			return null;
		var sequence = new Sequence(getInt(map, "count", 0));
		sequence.start = getInt(map, "start", 1);
		sequence.digits = getInt(map, "digits", 0);
		sequence.setupIndex = getInt(map, "setup", 0);
		return sequence;
	}

	private function readAttachment(map:Dynamic, skin:Skin, slotIndex:Int, name:String, skeletonData:SkeletonData):Attachment {
		if (Reflect.field(map, "name") != null)
			name = Reflect.field(map, "name");

		var color:String;
		switch (AttachmentType.fromName(Reflect.hasField(map, "type") ? Reflect.getProperty(map, "type") : "region")) {
			case AttachmentType.region:
				var path = getString(map, "path", name);
				var sequence = readSequence(Reflect.field(map, "sequence"));
				var region:RegionAttachment = attachmentLoader.newRegionAttachment(skin, name, path, sequence);
				if (region == null)
					return null;
				region.path = path;
				region.x = getFloat(map, "x") * scale;
				region.y = getFloat(map, "y") * scale;
				region.scaleX = getFloat(map, "scaleX", 1);
				region.scaleY = getFloat(map, "scaleY", 1);
				region.rotation = getFloat(map, "rotation");
				region.width = getFloat(map, "width") * scale;
				region.height = getFloat(map, "height") * scale;
				region.sequence = sequence;

				color = Reflect.getProperty(map, "color");
				if (color != null) {
					region.color.setFromString(color);
				}
				if (region.region != null)
					region.updateRegion();
				return region;
			case AttachmentType.mesh, AttachmentType.linkedmesh:
				var path = getString(map, "path", name);
				var sequence = readSequence(Reflect.field(map, "sequence"));
				var mesh:MeshAttachment = attachmentLoader.newMeshAttachment(skin, name, path, sequence);
				if (mesh == null)
					return null;
				mesh.path = path;

				color = Reflect.getProperty(map, "color");
				if (color != null) {
					mesh.color.setFromString(color);
				}

				mesh.width = getFloat(map, "width") * scale;
				mesh.height = getFloat(map, "height") * scale;
				mesh.sequence = sequence;

				if (Reflect.field(map, "parent") != null) {
					var inheritTimelines:Bool = map.hasOwnProperty("timelines") ? cast(Reflect.field(map, "timelines"), Bool) : true;
					linkedMeshes.push(new LinkedMesh(mesh, Reflect.field(map, "skin"), slotIndex, Reflect.field(map, "parent"), inheritTimelines));
					return mesh;
				}

				var uvs:Array<Float> = getFloatArray(map, "uvs");
				readVertices(map, mesh, uvs.length);
				mesh.triangles = getIntArray(map, "triangles");
				mesh.regionUVs = uvs;
				if (mesh.region != null)
					mesh.updateRegion();

				if (Reflect.field(map, "edges") != null)
					mesh.edges = getIntArray(map, "edges");
				mesh.hullLength = getInt(map, "hull") * 2;
				return mesh;
			case AttachmentType.boundingbox:
				var box:BoundingBoxAttachment = attachmentLoader.newBoundingBoxAttachment(skin, name);
				if (box == null)
					return null;
				readVertices(map, box, Std.parseInt(Reflect.field(map, "vertexCount")) << 1);
				return box;
			case AttachmentType.path:
				var path:PathAttachment = attachmentLoader.newPathAttachment(skin, name);
				if (path == null)
					return null;
				path.closed = map.hasOwnProperty("closed") ? cast(Reflect.field(map, "closed"), Bool) : false;
				path.constantSpeed = map.hasOwnProperty("constantSpeed") ? cast(Reflect.field(map, "constantSpeed"), Bool) : true;
				var vertexCount:Int = Std.parseInt(Reflect.field(map, "vertexCount"));
				readVertices(map, path, vertexCount << 1);
				var lengths:Array<Float> = new Array<Float>();
				for (curves in cast(Reflect.field(map, "lengths"), Array<Dynamic>)) {
					lengths.push(Std.parseFloat(curves) * scale);
				}
				path.lengths = lengths;
				return path;
			case AttachmentType.point:
				var point:PointAttachment = attachmentLoader.newPointAttachment(skin, name);
				if (point == null)
					return null;
				point.x = getFloat(map, "x", 0) * scale;
				point.y = getFloat(map, "y", 0) * scale;
				point.rotation = getFloat(map, "rotation", 0);
				color = Reflect.getProperty(map, "color");
				if (color != null) {
					point.color.setFromString(color);
				}
				return point;
			case AttachmentType.clipping:
				var clip:ClippingAttachment = attachmentLoader.newClippingAttachment(skin, name);
				if (clip == null)
					return null;
				var end:String = getString(map, "end", null);
				if (end != null) {
					var slot:SlotData = skeletonData.findSlot(end);
					if (slot == null)
						throw new SpineException("Clipping end slot not found: " + end);
					clip.endSlot = slot;
				}
				var vertexCount:Int = getInt(map, "vertexCount", 0);
				readVertices(map, clip, vertexCount << 1);
				color = Reflect.getProperty(map, "color");
				if (color != null) {
					clip.color.setFromString(color);
				}
				return clip;
		}
		return null;
	}

	private function readVertices(map:Dynamic, attachment:VertexAttachment, verticesLength:Int):Void {
		attachment.worldVerticesLength = verticesLength;
		var vertices:Array<Float> = getFloatArray(map, "vertices");
		if (verticesLength == vertices.length) {
			if (scale != 1) {
				for (i in 0...vertices.length) {
					vertices[i] *= scale;
				}
			}
			attachment.vertices = vertices;
			return;
		}

		var weights:Array<Float> = new Array<Float>();
		var bones:Array<Int> = new Array<Int>();
		var i:Int = 0;
		var n:Int = vertices.length;
		while (i < n) {
			var boneCount:Int = Std.int(vertices[i++]);
			bones.push(boneCount);
			var nn:Int = i + boneCount * 4;
			while (i < nn) {
				bones.push(Std.int(vertices[i]));
				weights.push(vertices[i + 1] * scale);
				weights.push(vertices[i + 2] * scale);
				weights.push(vertices[i + 3]);

				i += 4;
			}
		}
		attachment.bones = bones;
		attachment.vertices = weights;
	}

	private function readAnimation(map:Dynamic, name:String, skeletonData:SkeletonData):Void {
		var timelines:Array<Timeline> = new Array<Timeline>();

		var slotMap:Dynamic;
		var slotIndex:Int;
		var slotName:String;

		var timelineMap:Array<Dynamic>;
		var keyMap:Dynamic;
		var nextMap:Dynamic;
		var frame:Int, bezier:Int;
		var time:Float, time2:Float;
		var curve:Dynamic;
		var timelineName:String;

		// Slot timelines.
		var slots:Dynamic = Reflect.getProperty(map, "slots");
		for (slotName in Reflect.fields(slots)) {
			slotMap = Reflect.field(slots, slotName);
			slotIndex = skeletonData.findSlot(slotName).index;
			for (timelineName in Reflect.fields(slotMap)) {
				timelineMap = Reflect.field(slotMap, timelineName);
				if (timelineMap == null)
					continue;
				if (timelineName == "attachment") {
					var attachmentTimeline:AttachmentTimeline = new AttachmentTimeline(timelineMap.length, slotIndex);
					for (frame in 0...timelineMap.length) {
						keyMap = timelineMap[frame];
						attachmentTimeline.setFrame(frame, getFloat(keyMap, "time"), getString(keyMap, "name", null));
					}
					timelines.push(attachmentTimeline);
				} else if (timelineName == "rgba") {
					var rgbaTimeline:RGBATimeline = new RGBATimeline(timelineMap.length, timelineMap.length << 2, slotIndex);
					keyMap = timelineMap[0];
					time = getFloat(keyMap, "time");
					var rgba:Color = Color.fromString(keyMap.color);

					frame = 0;
					bezier = 0;
					while (true) {
						rgbaTimeline.setFrame(frame, time, rgba.r, rgba.g, rgba.b, rgba.a);
						if (timelineMap.length == frame + 1)
							break;

						nextMap = timelineMap[frame + 1];
						time2 = getFloat(nextMap, "time");
						var newRgba:Color = Color.fromString(nextMap.color);
						curve = keyMap.curve;
						if (curve != null) {
							bezier = readCurve(curve, rgbaTimeline, bezier, frame, 0, time, time2, rgba.r, newRgba.r, 1);
							bezier = readCurve(curve, rgbaTimeline, bezier, frame, 1, time, time2, rgba.g, newRgba.g, 1);
							bezier = readCurve(curve, rgbaTimeline, bezier, frame, 2, time, time2, rgba.b, newRgba.b, 1);
							bezier = readCurve(curve, rgbaTimeline, bezier, frame, 3, time, time2, rgba.a, newRgba.a, 1);
						}
						time = time2;
						rgba = newRgba;
						keyMap = nextMap;

						frame++;
					}

					timelines.push(rgbaTimeline);
				} else if (timelineName == "rgb") {
					var rgbTimeline:RGBTimeline = new RGBTimeline(timelineMap.length, timelineMap.length * 3, slotIndex);
					keyMap = timelineMap[0];
					time = getFloat(keyMap, "time");
					var rgb:Color = Color.fromString(keyMap.color);

					frame = 0;
					bezier = 0;
					while (true) {
						rgbTimeline.setFrame(frame, time, rgb.r, rgb.g, rgb.b);
						nextMap = timelineMap[frame + 1];
						if (nextMap == null) {
							rgbTimeline.shrink(bezier);
							break;
						}

						time2 = getFloat(nextMap, "time");
						var newRgb:Color = Color.fromString(nextMap.color);
						curve = keyMap.curve;
						if (curve != null) {
							bezier = readCurve(curve, rgbTimeline, bezier, frame, 0, time, time2, rgb.r, newRgb.r, 1);
							bezier = readCurve(curve, rgbTimeline, bezier, frame, 1, time, time2, rgb.g, newRgb.g, 1);
							bezier = readCurve(curve, rgbTimeline, bezier, frame, 2, time, time2, rgb.b, newRgb.b, 1);
						}
						time = time2;
						rgb = newRgb;
						keyMap = nextMap;

						frame++;
					}

					timelines.push(rgbTimeline);
				} else if (timelineName == "alpha") {
					timelines.push(readTimeline(timelineMap, new AlphaTimeline(timelineMap.length, timelineMap.length, slotIndex), 0, 1));
				} else if (timelineName == "rgba2") {
					var rgba2Timeline:RGBA2Timeline = new RGBA2Timeline(timelineMap.length, timelineMap.length * 7, slotIndex);

					keyMap = timelineMap[0];
					time = getFloat(keyMap, "time");
					var lighta:Color = Color.fromString(keyMap.light);
					var darka:Color = Color.fromString(keyMap.dark);

					frame = 0;
					bezier = 0;
					while (true) {
						rgba2Timeline.setFrame(frame, time, lighta.r, lighta.g, lighta.b, lighta.a, darka.r, darka.g, darka.b);
						nextMap = timelineMap[frame + 1];
						if (nextMap == null) {
							rgba2Timeline.shrink(bezier);
							break;
						}

						time2 = getFloat(nextMap, "time");
						var newLighta:Color = Color.fromString(nextMap.light);
						var newDarka:Color = Color.fromString(nextMap.dark);
						curve = keyMap.curve;
						if (curve != null) {
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

						frame++;
					}

					timelines.push(rgba2Timeline);
				} else if (timelineName == "rgb2") {
					var rgb2Timeline:RGB2Timeline = new RGB2Timeline(timelineMap.length, timelineMap.length * 6, slotIndex);

					keyMap = timelineMap[0];
					time = getFloat(keyMap, "time");
					var light:Color = Color.fromString(keyMap.light);
					var dark:Color = Color.fromString(keyMap.dark);

					frame = 0;
					bezier = 0;
					while (true) {
						rgb2Timeline.setFrame(frame, time, light.r, light.g, light.b, dark.r, dark.g, dark.b);
						nextMap = timelineMap[frame + 1];
						if (nextMap == null) {
							rgb2Timeline.shrink(bezier);
							break;
						}

						time2 = getFloat(nextMap, "time");
						var newLight:Color = Color.fromString(nextMap.light);
						var newDark:Color = Color.fromString(nextMap.dark);
						curve = keyMap.curve;
						if (curve != null) {
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

						frame++;
					}

					timelines.push(rgb2Timeline);
				} else {
					throw new SpineException("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
				}
			}
		}

		// Bone timelines.
		var bones:Dynamic = Reflect.getProperty(map, "bones");
		for (boneName in Reflect.fields(bones)) {
			var boneIndex:Int = skeletonData.findBoneIndex(boneName);
			if (boneIndex == -1)
				throw new SpineException("Bone not found: " + boneName);
			var boneMap:Dynamic = Reflect.field(bones, boneName);
			for (timelineName in Reflect.fields(boneMap)) {
				timelineMap = Reflect.field(boneMap, timelineName);
				if (timelineMap.length == 0)
					continue;

				if (timelineName == "rotate") {
					timelines.push(readTimeline(timelineMap, new RotateTimeline(timelineMap.length, timelineMap.length, boneIndex), 0, 1));
				} else if (timelineName == "translate") {
					var translateTimeline:TranslateTimeline = new TranslateTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
					timelines.push(readTimeline2(timelineMap, translateTimeline, "x", "y", 0, scale));
				} else if (timelineName == "translatex") {
					var translateXTimeline:TranslateXTimeline = new TranslateXTimeline(timelineMap.length, timelineMap.length, boneIndex);
					timelines.push(readTimeline(timelineMap, translateXTimeline, 0, scale));
				} else if (timelineName == "translatey") {
					var translateYTimeline:TranslateYTimeline = new TranslateYTimeline(timelineMap.length, timelineMap.length, boneIndex);
					timelines.push(readTimeline(timelineMap, translateYTimeline, 0, scale));
				} else if (timelineName == "scale") {
					var scaleTimeline:ScaleTimeline = new ScaleTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
					timelines.push(readTimeline2(timelineMap, scaleTimeline, "x", "y", 1, 1));
				} else if (timelineName == "scalex") {
					var scaleXTimeline:ScaleXTimeline = new ScaleXTimeline(timelineMap.length, timelineMap.length, boneIndex);
					timelines.push(readTimeline(timelineMap, scaleXTimeline, 1, 1));
				} else if (timelineName == "scaley") {
					var scaleYTimeline:ScaleYTimeline = new ScaleYTimeline(timelineMap.length, timelineMap.length, boneIndex);
					timelines.push(readTimeline(timelineMap, scaleYTimeline, 1, 1));
				} else if (timelineName == "shear") {
					var shearTimeline:ShearTimeline = new ShearTimeline(timelineMap.length, timelineMap.length << 1, boneIndex);
					timelines.push(readTimeline2(timelineMap, shearTimeline, "x", "y", 0, 1));
				} else if (timelineName == "shearx") {
					var shearXTimeline:ShearXTimeline = new ShearXTimeline(timelineMap.length, timelineMap.length, boneIndex);
					timelines.push(readTimeline(timelineMap, shearXTimeline, 0, 1));
				} else if (timelineName == "sheary") {
					var shearYTimeline:ShearYTimeline = new ShearYTimeline(timelineMap.length, timelineMap.length, boneIndex);
					timelines.push(readTimeline(timelineMap, shearYTimeline, 0, 1));
				} else if (timelineName == "inherit") {
					var inheritTimeline:InheritTimeline = new InheritTimeline(timelineMap.length, boneIndex);
					for (frame in 0...timelineMap.length) {
						var aFrame:Dynamic = timelineMap[frame];
						inheritTimeline.setFrame(frame, getFloat(aFrame, "time"), Inherit.fromName(getValue(aFrame, "inherit", "Normal")));
					}
					timelines.push(inheritTimeline);
				} else {
					throw new SpineException("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
				}
			}
		}

		// IK constraint timelines.
		var iks:Dynamic = Reflect.getProperty(map, "ik");
		for (ikConstraintName in Reflect.fields(iks)) {
			timelineMap = Reflect.field(iks, ikConstraintName);
			keyMap = timelineMap[0];
			if (keyMap == null)
				continue;

			var ikIndex:Int = skeletonData.ikConstraints.indexOf(skeletonData.findIkConstraint(ikConstraintName));
			var ikTimeline:IkConstraintTimeline = new IkConstraintTimeline(timelineMap.length, timelineMap.length << 1, ikIndex);

			time = getFloat(keyMap, "time");
			var mix:Float = getFloat(keyMap, "mix", 1);
			var softness:Float = getFloat(keyMap, "softness") * scale;

			frame = 0;
			bezier = 0;
			while (true) {
				ikTimeline.setFrame(frame, time, mix, softness,
					Reflect.hasField(keyMap, "bendPositive") ? (cast(Reflect.getProperty(keyMap, "bendPositive"), Bool) ? 1 : -1) : 1,
					Reflect.hasField(keyMap, "compress") ? cast(Reflect.getProperty(keyMap, "compress"), Bool) : false,
					Reflect.hasField(keyMap, "stretch") ? cast(Reflect.getProperty(keyMap, "stretch"), Bool) : false);

				nextMap = timelineMap[frame + 1];
				if (nextMap == null) {
					ikTimeline.shrink(bezier);
					break;
				}

				time2 = getFloat(nextMap, "time");
				var mix2:Float = getFloat(nextMap, "mix", 1);
				var softness2:Float = getFloat(nextMap, "softness") * scale;

				curve = keyMap.curve;
				if (curve != null) {
					bezier = readCurve(curve, ikTimeline, bezier, frame, 0, time, time2, mix, mix2, 1);
					bezier = readCurve(curve, ikTimeline, bezier, frame, 1, time, time2, softness, softness2, scale);
				}
				time = time2;
				mix = mix2;
				softness = softness2;
				keyMap = nextMap;

				frame++;
			}
			timelines.push(ikTimeline);
		}

		// Transform constraint timelines.
		var mixRotate:Float, mixRotate2:Float;
		var mixX:Float, mixX2:Float;
		var mixY:Float, mixY2:Float;
		var transforms:Dynamic = Reflect.getProperty(map, "transform");
		for (transformName in Reflect.fields(transforms)) {
			timelineMap = Reflect.field(transforms, transformName);
			keyMap = timelineMap[0];
			if (keyMap == null)
				continue;

			var transformIndex:Int = skeletonData.transformConstraints.indexOf(skeletonData.findTransformConstraint(transformName));
			var transformTimeline:TransformConstraintTimeline = new TransformConstraintTimeline(timelineMap.length, timelineMap.length << 2, transformIndex);

			time = getFloat(keyMap, "time");
			mixRotate = getFloat(keyMap, "mixRotate", 1);
			var mixShearY:Float = getFloat(keyMap, "mixShearY", 1);
			mixX = getFloat(keyMap, "mixX", 1);
			mixY = getFloat(keyMap, "mixY", mixX);
			var mixScaleX:Float = getFloat(keyMap, "mixScaleX", 1);
			var mixScaleY:Float = getFloat(keyMap, "mixScaleY", mixScaleX);

			frame = 0;
			bezier = 0;
			while (true) {
				transformTimeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
				nextMap = timelineMap[frame + 1];
				if (nextMap == null) {
					transformTimeline.shrink(bezier);
					break;
				}

				time2 = getFloat(nextMap, "time");
				mixRotate2 = getFloat(nextMap, "mixRotate", 1);
				var mixShearY2:Float = getFloat(nextMap, "mixShearY", 1);
				mixX2 = getFloat(nextMap, "mixX", 1);
				mixY2 = getFloat(nextMap, "mixY", mixX2);
				var mixScaleX2:Float = getFloat(nextMap, "mixScaleX", 1);
				var mixScaleY2:Float = getFloat(nextMap, "mixScaleY", mixScaleX2);
				curve = keyMap.curve;
				if (curve != null) {
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

				frame++;
			}

			timelines.push(transformTimeline);
		}

		// Path constraint timelines.
		var paths:Dynamic = Reflect.getProperty(map, "path");
		for (pathName in Reflect.fields(paths)) {
			var index:Int = skeletonData.findPathConstraintIndex(pathName);
			if (index == -1)
				throw new SpineException("Path constraint not found: " + pathName);
			var pathData:PathConstraintData = skeletonData.pathConstraints[index];

			var pathMap:Dynamic = Reflect.field(paths, pathName);
			for (timelineName in Reflect.fields(pathMap)) {
				timelineMap = Reflect.field(pathMap, timelineName);
				keyMap = timelineMap[0];
				if (keyMap == null)
					continue;

				if (timelineName == "position") {
					var positionTimeline:PathConstraintPositionTimeline = new PathConstraintPositionTimeline(timelineMap.length, timelineMap.length, index);
					timelines.push(readTimeline(timelineMap, positionTimeline, 0, pathData.positionMode == PositionMode.fixed ? scale : 1));
				} else if (timelineName == "spacing") {
					var spacingTimeline:PathConstraintSpacingTimeline = new PathConstraintSpacingTimeline(timelineMap.length, timelineMap.length, index);
					timelines.push(readTimeline(timelineMap, spacingTimeline,
						0, pathData.spacingMode == SpacingMode.length || pathData.spacingMode == SpacingMode.fixed ? scale : 1));
				} else if (timelineName == "mix") {
					var mixTimeline:PathConstraintMixTimeline = new PathConstraintMixTimeline(timelineMap.length, timelineMap.length * 3, index);
					time = getFloat(keyMap, "time");
					mixRotate = getFloat(keyMap, "mixRotate", 1);
					mixX = getFloat(keyMap, "mixX", 1);
					mixY = getFloat(keyMap, "mixY", mixX);

					frame = 0;
					bezier = 0;
					while (true) {
						mixTimeline.setFrame(frame, time, mixRotate, mixX, mixY);
						nextMap = timelineMap[frame + 1];
						if (nextMap == null) {
							mixTimeline.shrink(bezier);
							break;
						}
						time2 = getFloat(nextMap, "time");
						mixRotate2 = getFloat(nextMap, "mixRotate", 1);
						mixX2 = getFloat(nextMap, "mixX", 1);
						mixY2 = getFloat(nextMap, "mixY", mixX2);
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

						frame++;
					}

					timelines.push(mixTimeline);
				}
			}
		}

		// Physics constraint timelines.
		var physics:Dynamic = Reflect.getProperty(map, "physics");
		for (physicsName in Reflect.fields(physics)) {
			var constraintIndex:Int = -1;
			if (physicsName.length > 0) {
				constraintIndex = skeletonData.findPhysicsConstraintIndex(physicsName);
				if (constraintIndex == -1)
					throw new SpineException("Physics constraint not found: " + physicsName);
			}
			var physicsMap:Dynamic = Reflect.field(physics, physicsName);
			for (timelineName in Reflect.fields(physicsMap)) {
				timelineMap = Reflect.field(physicsMap, timelineName);
				keyMap = timelineMap[0];
				if (keyMap == null)
					continue;

				var frames:Int = timelineMap.length;
				if (timelineName == "reset") {
					var timeline:PhysicsConstraintResetTimeline = new PhysicsConstraintResetTimeline(frames, constraintIndex);
					for (frame => keyMap in timelineMap)
						timeline.setFrame(frame, getFloat(keyMap, "time"));
					timelines.push(timeline);
					continue;
				}

				var timeline:PhysicsConstraintTimeline;
					if (timelineName == "inertia")
						timeline = new PhysicsConstraintInertiaTimeline(frames, frames, constraintIndex);
					else if (timelineName == "strength")
						timeline = new PhysicsConstraintStrengthTimeline(frames, frames, constraintIndex);
					else if (timelineName == "damping")
						timeline = new PhysicsConstraintDampingTimeline(frames, frames, constraintIndex);
					else if (timelineName == "mass")
						timeline = new PhysicsConstraintMassTimeline(frames, frames, constraintIndex);
					else if (timelineName == "wind")
						timeline = new PhysicsConstraintWindTimeline(frames, frames, constraintIndex);
					else if (timelineName == "gravity")
						timeline = new PhysicsConstraintGravityTimeline(frames, frames, constraintIndex);
					else if (timelineName == "mix") //
						timeline = new PhysicsConstraintMixTimeline(frames, frames, constraintIndex);
					else
						continue;
				timelines.push(readTimeline(timelineMap, timeline, 0, 1));
			}
		}

		// Attachment timelines.
		var attachments:Dynamic = Reflect.getProperty(map, "attachments");
		for (attachmentsName in Reflect.fields(attachments)) {
			var attachmentsMap:Dynamic = Reflect.field(attachments, attachmentsName);
			var skin:Skin = skeletonData.findSkin(attachmentsName);
			if (skin == null)
				throw new SpineException("Skin not found: " + attachmentsName);

			for (slotMapName in Reflect.fields(attachmentsMap)) {
				slotMap = Reflect.field(attachmentsMap, slotMapName);
				slotIndex = skeletonData.findSlot(slotMapName).index;
				if (slotIndex == -1)
					throw new SpineException("Slot not found: " + slotMapName);
				for (attachmentMapName in Reflect.fields(slotMap)) {
					var attachmentMap = Reflect.field(slotMap, attachmentMapName);
					var attachment:Attachment = skin.getAttachment(slotIndex, attachmentMapName);
					if (attachment == null)
						throw new SpineException("Timeline attachment not found: " + attachmentMapName);

					for (timelineMapName in Reflect.fields(attachmentMap)) {
						var timelineMap = Reflect.field(attachmentMap, timelineMapName);
						var keyMap = timelineMap[0];
						if (keyMap == null)
							continue;

						if (timelineMapName == "deform") {
							var vertexAttachment = cast(attachment, VertexAttachment);
							var weighted:Bool = vertexAttachment.bones != null;
							var vertices:Array<Float> = vertexAttachment.vertices;
							var deformLength:Int = weighted ? Std.int(vertices.length / 3 * 2) : vertices.length;

							var deformTimeline:DeformTimeline = new DeformTimeline(timelineMap.length, timelineMap.length, slotIndex, vertexAttachment);
							time = getFloat(keyMap, "time");
							frame = 0;
							bezier = 0;
							while (true) {
								var deform:Array<Float>;
								var verticesValue:Array<Float> = Reflect.getProperty(keyMap, "vertices");
								if (verticesValue == null) {
									if (weighted) {
										deform = new Array<Float>();
										ArrayUtils.resize(deform, deformLength, 0);
									} else {
										deform = vertices;
									}
								} else {
									deform = new Array<Float>();
									ArrayUtils.resize(deform, deformLength, 0);
									var start:Int = getInt(keyMap, "offset");
									var temp:Array<Float> = getFloatArray(keyMap, "vertices");
									for (i in 0...temp.length) {
										deform[start + i] = temp[i];
									}
									if (scale != 1) {
										for (i in start...start + temp.length) {
											deform[i] *= scale;
										}
									}
									if (!weighted) {
										for (i in 0...deformLength) {
											deform[i] += vertices[i];
										}
									}
								}

								deformTimeline.setFrame(frame, time, deform);
								nextMap = timelineMap[frame + 1];
								if (nextMap == null) {
									deformTimeline.shrink(bezier);
									break;
								}
								time2 = getFloat(nextMap, "time");
								curve = keyMap.curve;
								if (curve != null) {
									bezier = readCurve(curve, deformTimeline, bezier, frame, 0, time, time2, 0, 1, 1);
								}
								time = time2;
								keyMap = nextMap;

								frame++;
							}

							timelines.push(deformTimeline);
						} else if (timelineMapName == "sequence") {
							var timeline = new SequenceTimeline(timelineMap.length, slotIndex, cast(attachment, HasTextureRegion));
							var lastDelay:Float = 0;
							var frame:Int = 0;
							while (frame < timelineMap.length) {
								var delay = getFloat(keyMap, "delay", lastDelay);
								var time = getFloat(keyMap, "time", 0);
								var mode = SequenceMode.fromName(getString(keyMap, "mode", "hold"));
								var index = getInt(keyMap, "index", 0);
								timeline.setFrame(frame, time, mode, index, delay);
								lastDelay = delay;
								keyMap = timelineMap[frame + 1];
								frame++;
							}
							timelines.push(timeline);
						}
					}
				}
			}
		}

		// Draw order timelines.
		if (Reflect.hasField(map, "drawOrder")) {
			var drawOrders:Array<Dynamic> = cast(Reflect.field(map, "drawOrder"), Array<Dynamic>);
			if (drawOrders != null) {
				var drawOrderTimeline:DrawOrderTimeline = new DrawOrderTimeline(drawOrders.length);
				var slotCount:Int = skeletonData.slots.length;
				frame = 0;
				for (drawOrderMap in drawOrders) {
					var drawOrder:Array<Int> = null;
					var offsets:Array<Dynamic> = Reflect.getProperty(drawOrderMap, "offsets");
					if (offsets != null) {
						drawOrder = new Array<Int>();
						drawOrder.resize(slotCount);
						var i = slotCount - 1;
						while (i >= 0) {
							drawOrder[i--] = -1;
						}
						var unchanged:Array<Int> = new Array<Int>();
						unchanged.resize(slotCount - offsets.length);
						var originalIndex:Int = 0, unchangedIndex:Int = 0;
						for (offsetMap in offsets) {
							slotIndex = skeletonData.findSlot(Reflect.getProperty(offsetMap, "slot")).index;
							if (slotIndex == -1)
								throw new SpineException("Slot not found: " + Reflect.getProperty(offsetMap, "slot"));
							// Collect unchanged items.
							while (originalIndex != slotIndex) {
								unchanged[unchangedIndex++] = originalIndex++;
							}
							// Set changed items.
							drawOrder[originalIndex + Reflect.getProperty(offsetMap, "offset")] = originalIndex++;
						}
						// Collect remaining unchanged items.
						while (originalIndex < slotCount) {
							unchanged[unchangedIndex++] = originalIndex++;
						}
						// Fill in unchanged items.
						i = slotCount - 1;
						while (i >= 0) {
							if (drawOrder[i] == -1)
								drawOrder[i] = unchanged[--unchangedIndex];
							i--;
						}
					}
					drawOrderTimeline.setFrame(frame++, getFloat(drawOrderMap, "time"), drawOrder);
				}
				timelines.push(drawOrderTimeline);
			}
		}

		// Event timelines.
		if (Reflect.hasField(map, "events")) {
			var eventsMap:Array<Dynamic> = cast(Reflect.field(map, "events"), Array<Dynamic>);
			if (eventsMap != null) {
				var eventTimeline:EventTimeline = new EventTimeline(eventsMap.length);
				frame = 0;
				for (eventMap in eventsMap) {
					var eventData:EventData = skeletonData.findEvent(Reflect.getProperty(eventMap, "name"));
					if (eventData == null)
						throw new SpineException("Event not found: " + Reflect.getProperty(eventMap, "name"));
					var event:Event = new Event(getFloat(eventMap, "time"), eventData);
					event.intValue = Reflect.hasField(eventMap, "int") ? getInt(eventMap, "int") : eventData.intValue;

					event.floatValue = Reflect.hasField(eventMap, "float") ? getFloat(eventMap, "float") : eventData.floatValue;

					event.stringValue = Reflect.hasField(eventMap, "string") ? Reflect.getProperty(eventMap, "string") : eventData.stringValue;
					if (eventData.audioPath != null) {
						event.volume = getFloat(eventMap, "volume", 1);
						event.balance = getFloat(eventMap, "balance");
					}
					eventTimeline.setFrame(frame++, event);
				}
				timelines.push(eventTimeline);
			}
		}

		var duration:Float = 0;
		for (i in 0...timelines.length) {
			duration = Math.max(duration, timelines[i].getDuration());
		}

		skeletonData.animations.push(new Animation(name, timelines, duration));
	}

	static private function readTimeline(keys:Array<Dynamic>, timeline:CurveTimeline1, defaultValue:Float, scale:Float):CurveTimeline1 {
		var keyMap:Dynamic = keys[0];
		var time:Float = getFloat(keyMap, "time");
		var value:Float = getFloat(keyMap, "value", defaultValue) * scale;
		var bezier:Int = 0;
		var frame:Int = 0;
		while (true) {
			timeline.setFrame(frame, time, value);
			var nextMap:Dynamic = keys[frame + 1];
			if (nextMap == null) {
				timeline.shrink(bezier);
				break;
			}
			var time2:Float = getFloat(nextMap, "time");
			var value2:Float = getFloat(nextMap, "value", defaultValue) * scale;
			var curve:Dynamic = keyMap.curve;
			if (curve != null) {
				bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value, value2, scale);
			}
			time = time2;
			value = value2;
			keyMap = nextMap;

			frame++;
		}
		return timeline;
	}

	static private function readTimeline2(keys:Array<Dynamic>, timeline:CurveTimeline2, name1:String, name2:String, defaultValue:Float,
			scale:Float):CurveTimeline2 {
		var keyMap:Dynamic = keys[0];
		var time:Float = getFloat(keyMap, "time");
		var value1:Float = getFloat(keyMap, name1, defaultValue) * scale;
		var value2:Float = getFloat(keyMap, name2, defaultValue) * scale;
		var bezier:Int = 0;
		var frame:Int = 0;
		while (true) {
			timeline.setFrame(frame, time, value1, value2);
			var nextMap:Dynamic = keys[frame + 1];
			if (nextMap == null) {
				timeline.shrink(bezier);
				break;
			}
			var time2:Float = getFloat(nextMap, "time");
			var nvalue1:Float = getFloat(nextMap, name1, defaultValue) * scale;
			var nvalue2:Float = getFloat(nextMap, name2, defaultValue) * scale;
			var curve:Dynamic = keyMap.curve;
			if (curve != null) {
				bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value1, nvalue1, scale);
				bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, value2, nvalue2, scale);
			}
			time = time2;
			value1 = nvalue1;
			value2 = nvalue2;
			keyMap = nextMap;

			frame++;
		}
		return timeline;
	}

	static private function readCurve(curve:Dynamic, timeline:CurveTimeline, bezier:Int, frame:Int, value:Int, time1:Float, time2:Float, value1:Float,
			value2:Float, scale:Float):Int {
		if (curve == "stepped") {
			timeline.setStepped(frame);
			return bezier;
		}

		var i:Int = value << 2;
		var cx1:Float = curve[i];
		var cy1:Float = curve[i + 1] * scale;
		var cx2:Float = curve[i + 2];
		var cy2:Float = curve[i + 3] * scale;
		timeline.setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
		return bezier + 1;
	}

	static private function getValue(map:Dynamic, name:String, defaultValue:Dynamic):Dynamic {
		if (map.hasOwnProperty(name))
			return Reflect.field(map, name);
		return defaultValue;
	}

	static private function getString(value:Dynamic, name:String, defaultValue:String):String {
		if (Std.isOfType(Reflect.field(value, name), String))
			return cast(Reflect.field(value, name), String);
		return defaultValue;
	}

	static private function getFloat(value:Dynamic, name:String, defaultValue:Float = 0):Float {
		if (Std.isOfType(Reflect.field(value, name), Float))
			return cast(Reflect.field(value, name), Float);
		return defaultValue;
	}

	static private function getFloatArray(map:Dynamic, name:String):Array<Float> {
		var list:Array<Dynamic> = cast(Reflect.field(map, name), Array<Dynamic>);
		var values:Array<Float> = new Array<Float>();
		values.resize(list.length);
		for (i in 0...list.length) {
			values[i] = cast(list[i], Float);
		}
		return values;
	}

	static private function getInt(value:Dynamic, name:String, defaultValue:Int = 0):Int {
		if (Std.isOfType(Reflect.field(value, name), Int))
			return cast(Reflect.field(value, name), Int);
		return defaultValue;
	}

	static private function getIntArray(map:Dynamic, name:String):Array<Int> {
		var list:Array<Dynamic> = cast(Reflect.field(map, name), Array<Dynamic>);
		var values:Array<Int> = new Array<Int>();
		values.resize(list.length);
		for (i in 0...list.length) {
			values[i] = Std.int(list[i]);
		}
		return values;
	}
}

class LinkedMesh {
	public var parent(default, null):String;
	public var skin(default, null):String;
	public var slotIndex(default, null):Int;
	public var mesh(default, null):MeshAttachment;
	public var inheritTimeline(default, null):Bool;

	public function new(mesh:MeshAttachment, skin:String, slotIndex:Int, parent:String, inheritTimeline:Bool) {
		this.mesh = mesh;
		this.skin = skin;
		this.slotIndex = slotIndex;
		this.parent = parent;
		this.inheritTimeline = inheritTimeline;
	}
}
