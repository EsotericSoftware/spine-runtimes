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

package spine;

import haxe.DynamicAccess;
import spine.IkConstraintData.ScaleY;
import spine.animation.BoneTimeline2;
import spine.animation.SliderMixTimeline;
import spine.animation.SliderTimeline;
import spine.TransformConstraintData;
import Reflect;
import haxe.Json;
import spine.animation.AlphaTimeline;
import spine.animation.Animation;
import spine.animation.AttachmentTimeline;
import spine.animation.CurveTimeline1;
import spine.animation.CurveTimeline;
import spine.animation.DeformTimeline;
import spine.animation.DrawOrderFolderTimeline;
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
			skeletonData.referenceScale = getFloat(skeletonMap, "referenceScale", 100) * scale;
			skeletonData.fps = getFloat(skeletonMap, "fps");
			skeletonData.imagesPath = getString(skeletonMap, "images", "");
			skeletonData.audioPath = getString(skeletonMap, "audio", "");
		}

		// Bones.
		for (boneMap in cast(Reflect.getProperty(root, "bones"), Array<Dynamic>)) {
			var parent:BoneData = null;
			var parentName:String = Reflect.getProperty(boneMap, "parent");
			if (parentName != null) {
				parent = skeletonData.findBone(parentName);
				if (parent == null)
					throw new SpineException("Parent bone not found: " + parentName);
			}
			var data = new BoneData(skeletonData.bones.length, Reflect.getProperty(boneMap, "name"), parent);
			data.length = getFloat(boneMap, "length") * scale;
			var setup = data.setupPose;
			setup.x = getFloat(boneMap, "x") * scale;
			setup.y = getFloat(boneMap, "y") * scale;
			setup.rotation = getFloat(boneMap, "rotation");
			setup.scaleX = getFloat(boneMap, "scaleX", 1);
			setup.scaleY = getFloat(boneMap, "scaleY", 1);
			setup.shearX = getFloat(boneMap, "shearX");
			setup.shearY = getFloat(boneMap, "shearY");
			setup.inherit = Reflect.hasField(boneMap, "inherit") ? Inherit.fromName(Reflect.getProperty(boneMap, "inherit")) : Inherit.normal;
			data.skinRequired = Reflect.hasField(boneMap, "skin") ? cast(Reflect.getProperty(boneMap, "skin"), Bool) : false;

			var color:String = Reflect.getProperty(boneMap, "color");
			if (color != null)
				data.color.setFromString(color);
			data.icon = Reflect.getProperty(boneMap, "icon");
			data.iconSize = getFloat(boneMap, "iconSize", 1);
			data.iconRotation = getFloat(boneMap, "iconRotation");
			data.visible = Reflect.hasField(boneMap, "visible") ? cast(Reflect.getProperty(boneMap, "visible"), Bool) : true;

			skeletonData.bones.push(data);
		}

		// Slots.
		for (slotMap in cast(Reflect.getProperty(root, "slots"), Array<Dynamic>)) {
			var slotName:String = Reflect.getProperty(slotMap, "name");
			var boneName:String = Reflect.getProperty(slotMap, "bone");
			var boneData = skeletonData.findBone(boneName);
			if (boneData == null)
				throw new SpineException("Slot bone not found: " + boneName);

			var data = new SlotData(skeletonData.slots.length, slotName, boneData);

			var color:String = Reflect.getProperty(slotMap, "color");
			if (color != null)
				data.setupPose.color.setFromString(color);

			var dark:String = Reflect.getProperty(slotMap, "dark");
			if (dark != null)
				data.setupPose.darkColor = new Color(0, 0, 0).setFromString(dark);

			data.attachmentName = Reflect.getProperty(slotMap, "attachment");
			data.blendMode = Reflect.hasField(slotMap, "blend") ? BlendMode.fromName(Reflect.getProperty(slotMap, "blend")) : BlendMode.normal;
			data.visible = getValue(slotMap, "visible", true);
			skeletonData.slots.push(data);
		}

		// Constraints.
		if (Reflect.hasField(root, "constraints")) {
			for (constraintMap in cast(Reflect.getProperty(root, "constraints"), Array<Dynamic>)) {
				var name:String = Reflect.getProperty(constraintMap, "name");
				var skinRequired:Bool = Reflect.getProperty(constraintMap, "skinRequired");
				var type:String = Reflect.getProperty(constraintMap, "type");
				switch (type) {
					case "ik":
						var data = new IkConstraintData(name);
						data.skinRequired = skinRequired;

						for (boneName in cast(Reflect.getProperty(constraintMap, "bones"), Array<Dynamic>)) {
							var bone = skeletonData.findBone(boneName);
							if (bone == null)
								throw new SpineException("IK constraint bone not found: " + boneName);
							data.bones.push(bone);
						}

						data.target = skeletonData.findBone(Reflect.getProperty(constraintMap, "target"));
						if (data.target == null)
							throw new SpineException("Target bone not found: " + Reflect.getProperty(constraintMap, "target"));

						var scaleY:String = Reflect.getProperty(constraintMap, "scaleY");
						if (scaleY != null)
							data.scaleY = ScaleY.fromName(scaleY);
						var setup = data.setupPose;
						setup.mix = getFloat(constraintMap, "mix", 1);
						setup.softness = getFloat(constraintMap, "softness", 0) * scale;
						setup.bendDirection = (!Reflect.hasField(constraintMap, "bendPositive")
							|| cast(Reflect.getProperty(constraintMap, "bendPositive"), Bool)) ? 1 : -1;
						setup.compress = (Reflect.hasField(constraintMap, "compress")
							&& cast(Reflect.getProperty(constraintMap, "compress"), Bool));
						setup.stretch = (Reflect.hasField(constraintMap, "stretch")
							&& cast(Reflect.getProperty(constraintMap, "stretch"), Bool));

						skeletonData.constraints.push(data);
					case "transform":
						var data = new TransformConstraintData(name);
						data.skinRequired = skinRequired;

						for (boneName in cast(Reflect.getProperty(constraintMap, "bones"), Array<Dynamic>)) {
							var bone = skeletonData.findBone(boneName);
							if (bone == null)
								throw new SpineException("Transform constraint bone not found: " + boneName);
							data.bones.push(bone);
						}

						data.source = skeletonData.findBone(Reflect.getProperty(constraintMap, "source"));
						if (data.source == null)
							throw new SpineException("Transform constraint source bone not found: " + Reflect.getProperty(constraintMap, "source"));

						data.localSource = Reflect.hasField(constraintMap,
							"localSource") ? cast(Reflect.getProperty(constraintMap, "localSource"), Bool) : false;
						data.localTarget = Reflect.hasField(constraintMap,
							"localTarget") ? cast(Reflect.getProperty(constraintMap, "localTarget"), Bool) : false;
						data.additive = Reflect.hasField(constraintMap, "additive") ? cast(Reflect.getProperty(constraintMap, "additive"), Bool) : false;
						data.clamp = Reflect.hasField(constraintMap, "clamp") ? cast(Reflect.getProperty(constraintMap, "clamp"), Bool) : false;

						var rotate = false, x = false, y = false, scaleX = false, scaleY = false, shearY = false;
						var propertiesMap:Dynamic = Reflect.getProperty(constraintMap, "properties");
						for (name in Reflect.fields(propertiesMap)) {
							var fromEntry = Reflect.field(propertiesMap, name);
							var from = fromProperty(name);
							var fromScale = propertyScale(name, scale);
							from.offset = getFloat(fromEntry, "offset", 0) * fromScale;
							var toMap:Dynamic = Reflect.getProperty(fromEntry, "to");
							for (name in Reflect.fields(toMap)) {
								var toEntry = Reflect.field(toMap, name);
								var toScale = 1.;
								var to:ToProperty;
								switch (name) {
									case "rotate": {
											rotate = true;
											to = new ToRotate();
										}
									case "x": {
											x = true;
											to = new ToX();
											toScale = scale;
										}
									case "y": {
											y = true;
											to = new ToY();
											toScale = scale;
										}
									case "scaleX": {
											scaleX = true;
											to = new ToScaleX();
										}
									case "scaleY": {
											scaleY = true;
											to = new ToScaleY();
										}
									case "shearY": {
											shearY = true;
											to = new ToShearY();
										}
									default: throw new SpineException("Invalid transform constraint to property: " + toEntry.name);
								}
								to.offset = getFloat(toEntry, "offset", 0) * toScale;
								to.max = getFloat(toEntry, "max", 1) * toScale;
								to.scale = getFloat(toEntry, "scale", 1) * toScale / fromScale;
								from.to.push(to);
							}
							if (from.to.length > 0)
								data.properties.push(from);
						}

						data.offsets[TransformConstraintData.ROTATION] = getFloat(constraintMap, "rotation", 0);
						data.offsets[TransformConstraintData.X] = getFloat(constraintMap, "x", 0) * scale;
						data.offsets[TransformConstraintData.Y] = getFloat(constraintMap, "y", 0) * scale;
						data.offsets[TransformConstraintData.SCALEX] = getFloat(constraintMap, "scaleX", 0);
						data.offsets[TransformConstraintData.SCALEY] = getFloat(constraintMap, "scaleY", 0);
						data.offsets[TransformConstraintData.SHEARY] = getFloat(constraintMap, "shearY", 0);

						var setup = data.setupPose;
						if (rotate)
							setup.mixRotate = getFloat(constraintMap, "mixRotate", 1);
						if (x)
							setup.mixX = getFloat(constraintMap, "mixX", 1);
						if (y)
							setup.mixY = getFloat(constraintMap, "mixY", setup.mixX);
						if (scaleX)
							setup.mixScaleX = getFloat(constraintMap, "mixScaleX", 1);
						if (scaleY)
							setup.mixScaleY = getFloat(constraintMap, "mixScaleY", setup.mixScaleX);
						if (shearY)
							setup.mixShearY = getFloat(constraintMap, "mixShearY", 1);

						skeletonData.constraints.push(data);
					case "path":
						var data = new PathConstraintData(name);
						data.skinRequired = skinRequired;

						for (boneName in cast(Reflect.getProperty(constraintMap, "bones"), Array<Dynamic>)) {
							var bone = skeletonData.findBone(boneName);
							if (bone == null)
								throw new SpineException("Path bone not found: " + boneName);
							data.bones.push(bone);
						}

						var slotName = getString(constraintMap, "slot", "");
						data.slot = skeletonData.findSlot(slotName);
						if (data.slot == null)
							throw new SpineException("Path slot not found: " + slotName);

						data.positionMode = Reflect.hasField(constraintMap,
							"positionMode") ? PositionMode.fromName(Reflect.getProperty(constraintMap, "positionMode")) : PositionMode.percent;
						data.spacingMode = Reflect.hasField(constraintMap,
							"spacingMode") ? SpacingMode.fromName(Reflect.getProperty(constraintMap, "spacingMode")) : SpacingMode.length;
						data.rotateMode = Reflect.hasField(constraintMap,
							"rotateMode") ? RotateMode.fromName(Reflect.getProperty(constraintMap, "rotateMode")) : RotateMode.tangent;
						data.offsetRotation = getFloat(constraintMap, "rotation", 0);
						var setup = data.setupPose;
						setup.position = getFloat(constraintMap, "position", 0);
						if (data.positionMode == PositionMode.fixed)
							setup.position *= scale;
						setup.spacing = getFloat(constraintMap, "spacing", 0);
						if (data.spacingMode == SpacingMode.length || data.spacingMode == SpacingMode.fixed)
							setup.spacing *= scale;
						setup.mixRotate = getFloat(constraintMap, "mixRotate", 1);
						setup.mixX = getFloat(constraintMap, "mixX", 1);
						setup.mixY = getFloat(constraintMap, "mixY", setup.mixX);

						skeletonData.constraints.push(data);
					case "physics":
						var data = new PhysicsConstraintData(name);
						data.skinRequired = skinRequired;

						var boneName:String = getString(constraintMap, "bone");
						data.bone = skeletonData.findBone(boneName);
						if (data.bone == null)
							throw new SpineException("Physics bone not found: " + boneName);

						data.x = getFloat(constraintMap, "x");
						data.y = getFloat(constraintMap, "y");
						data.rotate = getFloat(constraintMap, "rotate");
						data.scaleX = getFloat(constraintMap, "scaleX");
						data.shearX = getFloat(constraintMap, "shearX");
						data.limit = getFloat(constraintMap, "limit", 5000) * scale;
						data.step = 1 / getFloat(constraintMap, "fps", 60);
						var setup = data.setupPose;
						setup.inertia = getFloat(constraintMap, "inertia", .5);
						setup.strength = getFloat(constraintMap, "strength", 100);
						setup.damping = getFloat(constraintMap, "damping", .85);
						setup.massInverse = 1 / getFloat(constraintMap, "mass", 1);
						setup.wind = getFloat(constraintMap, "wind", 0);
						setup.gravity = getFloat(constraintMap, "gravity", 0);
						setup.mix = getValue(constraintMap, "mix", 1);
						data.inertiaGlobal = Reflect.hasField(constraintMap,
							"inertiaGlobal") ? cast(Reflect.getProperty(constraintMap, "inertiaGlobal"), Bool) : false;
						data.strengthGlobal = Reflect.hasField(constraintMap,
							"strengthGlobal") ? cast(Reflect.getProperty(constraintMap, "strengthGlobal"), Bool) : false;
						data.dampingGlobal = Reflect.hasField(constraintMap,
							"dampingGlobal") ? cast(Reflect.getProperty(constraintMap, "dampingGlobal"), Bool) : false;
						data.dampingGlobal = Reflect.hasField(constraintMap,
							"dampingGlobal") ? cast(Reflect.getProperty(constraintMap, "dampingGlobal"), Bool) : false;
						data.windGlobal = Reflect.hasField(constraintMap, "windGlobal") ? cast(Reflect.getProperty(constraintMap, "windGlobal"), Bool) : false;
						data.gravityGlobal = Reflect.hasField(constraintMap,
							"gravityGlobal") ? cast(Reflect.getProperty(constraintMap, "gravityGlobal"), Bool) : false;
						data.mixGlobal = Reflect.hasField(constraintMap, "mixGlobal") ? cast(Reflect.getProperty(constraintMap, "mixGlobal"), Bool) : false;

						skeletonData.constraints.push(data);
					case "slider":
						var data = new SliderData(name);
						data.skinRequired = skinRequired;
						data.additive = getBoolean(constraintMap, "additive", false);
						data.loop = getBoolean(constraintMap, "loop", false);
						data.setupPose.time = getFloat(constraintMap, "time", 0);
						data.setupPose.mix = getFloat(constraintMap, "mix", 1);

						var boneName = getString(constraintMap, "bone", null);
						if (boneName != null) {
							data.bone = skeletonData.findBone(boneName);
							if (data.bone == null)
								throw new SpineException("Slider bone not found: " + boneName);
							var property = getString(constraintMap, "property");
							data.property = fromProperty(property);
							var propertyScale = propertyScale(property, scale);
							data.property.offset = getFloat(constraintMap, "from", 0) * propertyScale;
							data.offset = getFloat(constraintMap, "to", 0);
							data.scale = getFloat(constraintMap, "scale", 1) / propertyScale;
							data.local = getBoolean(constraintMap, "local", false);
						}

						skeletonData.constraints.push(data);
				}
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
					var ik = cast(Reflect.getProperty(skinMap, "ik"), Array<Dynamic>);
					for (ii in 0...ik.length) {
						var constraint = skeletonData.findConstraint(ik[ii], IkConstraintData);
						if (constraint == null)
							throw new SpineException("Skin IK constraint not found: " + ik[ii]);
						skin.constraints.push(constraint);
					}
				}

				if (Reflect.hasField(skinMap, "transform")) {
					var transform = cast(Reflect.getProperty(skinMap, "transform"), Array<Dynamic>);
					for (ii in 0...transform.length) {
						var constraint = skeletonData.findConstraint(transform[ii], TransformConstraintData);
						if (constraint == null)
							throw new SpineException("Skin transform constraint not found: " + transform[ii]);
						skin.constraints.push(constraint);
					}
				}

				if (Reflect.hasField(skinMap, "path")) {
					var path = cast(Reflect.getProperty(skinMap, "path"), Array<Dynamic>);
					for (ii in 0...path.length) {
						var constraint = skeletonData.findConstraint(path[ii], PathConstraintData);
						if (constraint == null)
							throw new SpineException("Skin path constraint not found: " + path[ii]);
						skin.constraints.push(constraint);
					}
				}

				if (Reflect.hasField(skinMap, "physics")) {
					var physics = cast(Reflect.getProperty(skinMap, "physics"), Array<Dynamic>);
					for (ii in 0...physics.length) {
						var constraint = skeletonData.findConstraint(physics[ii], PhysicsConstraintData);
						if (constraint == null)
							throw new SpineException("Skin physics constraint not found: " + physics[ii]);
						skin.constraints.push(constraint);
					}
				}

				if (Reflect.hasField(skinMap, "slider")) {
					var slider = cast(Reflect.getProperty(skinMap, "slider"), Array<Dynamic>);
					for (ii in 0...slider.length) {
						var constraint = skeletonData.findConstraint(slider[ii], SliderData);
						if (constraint == null)
							throw new SpineException("Skin slider constraint not found: " + slider[ii]);
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
			var sourceSkin:Skin = linkedMesh.skin == null ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
			if (sourceSkin == null)
				throw new SpineException("Skin not found: " + linkedMesh.skin);
			var source:Attachment = sourceSkin.getAttachment(linkedMesh.sourceIndex, linkedMesh.source);
			if (source == null)
				throw new SpineException("Source mesh not found: " + linkedMesh.source);
			linkedMesh.mesh.timelineAttachment = linkedMesh.inheritTimelines ? source : linkedMesh.mesh;
			linkedMesh.mesh.sourceMesh = cast(source, MeshAttachment);
			linkedMesh.mesh.updateSequence();
			if (linkedMesh.inheritTimelines && linkedMesh.slotIndex != linkedMesh.sourceIndex) {
				var slots = source.timelineSlots;
				var found = false;
				for (existing in slots) {
					if (existing == linkedMesh.slotIndex) {
						found = true;
						break;
					}
				}
				if (!found) {
					var newSlots = slots.copy();
					newSlots.push(linkedMesh.slotIndex);
					source.timelineSlots = newSlots;
				}
			}
		}
		linkedMeshes.resize(0);

		// Events.
		var events:Dynamic = Reflect.getProperty(root, "events");
		for (eventName in Reflect.fields(events)) {
			var eventMap:DynamicAccess<Dynamic> = Reflect.field(events, eventName);
			var eventData:EventData = new EventData(eventName);
			var setup = eventData.setupPose;
			setup.intValue = getInt(eventMap, "int");
			setup.floatValue = getFloat(eventMap, "float");
			setup.stringValue = getString(eventMap, "string", "");
			eventData.audioPath = getString(eventMap, "audio", "");
			if (eventData.audioPath != null) {
				setup.volume = getFloat(eventMap, "volume", 1);
				setup.balance = getFloat(eventMap, "balance");
			}
			skeletonData.events.push(eventData);
		}

		// Animations.
		var animations:Dynamic = Reflect.getProperty(root, "animations");
		for (animationName in Reflect.fields(animations)) {
			readAnimation(Reflect.field(animations, animationName), animationName, skeletonData);
		}

		// Slider animations.
		if (Reflect.hasField(root, "constraints")) {
			for (constraintMap in cast(Reflect.getProperty(root, "constraints"), Array<Dynamic>)) {
				if (Reflect.getProperty(constraintMap, "type") == "slider") {
					var data = skeletonData.findConstraint(getString(constraintMap, "name"), SliderData);
					var animationName = getString(constraintMap, "animation", "");
					data.animation = skeletonData.findAnimation(animationName);
					if (data.animation == null)
						throw new SpineException("Slider animation not found: " + animationName);
				}
			}
		}

		return skeletonData;
	}

	private function fromProperty(type:String):FromProperty {
		var property:FromProperty;
		switch (type) {
			case "rotate":
				property = new FromRotate();
			case "x":
				property = new FromX();
			case "y":
				property = new FromY();
			case "scaleX":
				property = new FromScaleX();
			case "scaleY":
				property = new FromScaleY();
			case "shearY":
				property = new FromShearY();
			default:
				throw new SpineException("Invalid from property: " + type);
		};
		return property;
	}

	private function propertyScale(type:String, scale:Float):Float {
		var scaleValue:Float;
		switch (type) {
			case "x", "y":
				scaleValue = scale;
			default:
				scaleValue = 1;
		};
		return scaleValue;
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
				color = Reflect.getProperty(map, "color");
				if (color != null) {
					region.color.setFromString(color);
				}
				region.updateSequence();
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

				var source:String = Reflect.field(map, "source");
				if (source != null) {
					var inheritTimelines:Bool = Reflect.hasField(map, "timelines") ? cast(Reflect.field(map, "timelines"), Bool) : true;
					var sourceIndex = slotIndex;
					var slotName:String = Reflect.field(map, "slot");
					if (slotName != null) {
						var sourceSlot = skeletonData.findSlot(slotName);
						if (sourceSlot == null)
							throw new SpineException("Source mesh slot not found: " + slotName);
						sourceIndex = sourceSlot.index;
					}
					linkedMeshes.push(new LinkedMesh(mesh, Reflect.field(map, "skin"), slotIndex, sourceIndex, source, inheritTimelines));
					return mesh;
				}

				var uvs:Array<Float> = getFloatArray(map, "uvs");
				readVertices(map, mesh, uvs.length);
				mesh.triangles = getIntArray(map, "triangles");
				mesh.regionUVs = uvs;

				if (Reflect.field(map, "edges") != null)
					mesh.edges = getIntArray(map, "edges");
				mesh.hullLength = getInt(map, "hull") * 2;
				mesh.updateSequence();
				return mesh;
			case AttachmentType.boundingbox:
				var box:BoundingBoxAttachment = attachmentLoader.newBoundingBoxAttachment(skin, name);
				if (box == null)
					return null;
				readVertices(map, box, getInt(map, "vertexCount", 0) << 1);
				return box;
			case AttachmentType.path:
				var path:PathAttachment = attachmentLoader.newPathAttachment(skin, name);
				if (path == null)
					return null;
				path.closed = Reflect.hasField(map, "closed") ? cast(Reflect.field(map, "closed"), Bool) : false;
				path.constantSpeed = Reflect.hasField(map, "constantSpeed") ? cast(Reflect.field(map, "constantSpeed"), Bool) : true;
				readVertices(map, path, getInt(map, "vertexCount", 0) << 1);
				var lengths:Array<Float> = new Array<Float>();
				for (curves in cast(Reflect.field(map, "lengths"), Array<Dynamic>)) {
					lengths.push(curves * scale);
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
				clip.convex = getBoolean(map, "convex", false);
				clip.inverse = getBoolean(map, "inverse", false);
				readVertices(map, clip, getInt(map, "vertexCount", 0) << 1);
				color = Reflect.getProperty(map, "color");
				if (color != null) {
					clip.color.setFromString(color);
				}
				return clip;
		}
		return null;
	}

	private function readSequence(map:Dynamic) {
		if (map == null)
			return new Sequence(1, false);
		var sequence = new Sequence(getInt(map, "count", 0), true);
		sequence.start = getInt(map, "start", 1);
		sequence.digits = getInt(map, "digits", 0);
		sequence.setupIndex = getInt(map, "setup", 0);
		return sequence;
	}

	/** @param folderSlots Slot names are resolved to positions within this array. If null, slot indices are used as positions. */
	private function readDrawOrder(skeletonData:SkeletonData, keyMap:Dynamic, slotCount:Int, folderSlots:Array<Int>):Array<Int> {
		var changes:Array<Dynamic> = Reflect.getProperty(keyMap, "offsets");
		if (changes == null)
			return null;
		var drawOrder:Array<Int> = new Array<Int>();
		drawOrder.resize(slotCount);
		for (i in 0...slotCount)
			drawOrder[i] = -1;
		var unchanged:Array<Int> = new Array<Int>();
		unchanged.resize(slotCount - changes.length);
		var originalIndex:Int = 0, unchangedIndex:Int = 0;
		for (offsetMap in changes) {
			var slot = skeletonData.findSlot(Reflect.getProperty(offsetMap, "slot"));
			if (slot == null)
				throw new SpineException("Draw order slot not found: " + Reflect.getProperty(offsetMap, "slot"));
			var index:Int;
			if (folderSlots == null)
				index = slot.index;
			else {
				index = -1;
				for (i in 0...slotCount) {
					if (folderSlots[i] == slot.index) {
						index = i;
						break;
					}
				}
				if (index == -1)
					throw new SpineException("Slot not in folder: " + Reflect.getProperty(offsetMap, "slot"));
			}
			while (originalIndex != index)
				unchanged[unchangedIndex++] = originalIndex++;
			drawOrder[originalIndex + Reflect.getProperty(offsetMap, "offset")] = originalIndex++;
		}
		while (originalIndex < slotCount)
			unchanged[unchangedIndex++] = originalIndex++;
		var i:Int = slotCount - 1;
		while (i >= 0) {
			if (drawOrder[i] == -1)
				drawOrder[i] = unchanged[--unchangedIndex];
			i--;
		}
		return drawOrder;
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

				switch (timelineName) {
					case "attachment":
						var attachmentTimeline = new AttachmentTimeline(timelineMap.length, slotIndex);
						for (frame in 0...timelineMap.length) {
							keyMap = timelineMap[frame];
							attachmentTimeline.setFrame(frame, getFloat(keyMap, "time"), getString(keyMap, "name", null));
						}
						timelines.push(attachmentTimeline);
					case "rgba":
						var rgbaTimeline = new RGBATimeline(timelineMap.length, timelineMap.length << 2, slotIndex);
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
					case "rgb":
						var rgbTimeline = new RGBTimeline(timelineMap.length, timelineMap.length * 3, slotIndex);
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
					case "alpha":
						readTimeline(timelines, timelineMap, new AlphaTimeline(timelineMap.length, timelineMap.length, slotIndex), 0, 1);
					case "rgba2":
						var rgba2Timeline = new RGBA2Timeline(timelineMap.length, timelineMap.length * 7, slotIndex);

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
					case "rgb2":
						var rgb2Timeline = new RGB2Timeline(timelineMap.length, timelineMap.length * 6, slotIndex);

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
					default:
						throw new SpineException("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
				}
			}
		}

		// Bone timelines.
		var bones = Reflect.getProperty(map, "bones");
		for (boneName in Reflect.fields(bones)) {
			var boneIndex:Int = skeletonData.findBoneIndex(boneName);
			if (boneIndex == -1)
				throw new SpineException("Bone not found: " + boneName);
			var boneMap:Dynamic = Reflect.field(bones, boneName);
			for (timelineName in Reflect.fields(boneMap)) {
				timelineMap = Reflect.field(boneMap, timelineName);
				var frames = timelineMap.length;
				if (frames == 0)
					continue;

				switch (timelineName) {
					case "rotate":
						readTimeline(timelines, timelineMap, new RotateTimeline(frames, frames, boneIndex), 0, 1);
					case "translate":
						readTimeline2(timelines, timelineMap, new TranslateTimeline(frames, frames << 1, boneIndex), "x", "y", 0, scale);
					case "translatex":
						readTimeline(timelines, timelineMap, new TranslateXTimeline(frames, frames, boneIndex), 0, scale);
					case "translatey":
						readTimeline(timelines, timelineMap, new TranslateYTimeline(frames, frames, boneIndex), 0, scale);
					case "scale":
						readTimeline2(timelines, timelineMap, new ScaleTimeline(frames, frames << 1, boneIndex), "x", "y", 1, 1);
					case "scalex":
						readTimeline(timelines, timelineMap, new ScaleXTimeline(frames, frames, boneIndex), 1, 1);
					case "scaley":
						readTimeline(timelines, timelineMap, new ScaleYTimeline(frames, frames, boneIndex), 1, 1);
					case "shear":
						readTimeline2(timelines, timelineMap, new ShearTimeline(frames, frames << 1, boneIndex), "x", "y", 0, 1);
					case "shearx":
						readTimeline(timelines, timelineMap, new ShearXTimeline(frames, frames, boneIndex), 0, 1);
					case "sheary":
						readTimeline(timelines, timelineMap, new ShearYTimeline(frames, frames, boneIndex), 0, 1);
					case "inherit":
						var timeline = new InheritTimeline(frames, boneIndex);
						for (frame in 0...frames) {
							var aFrame:Dynamic = timelineMap[frame];
							timeline.setFrame(frame, getFloat(aFrame, "time"), Inherit.fromName(getValue(aFrame, "inherit", "Normal")));
						}
						timelines.push(timeline);
					default:
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

			var constraint = skeletonData.findConstraint(ikConstraintName, IkConstraintData);
			if (constraint == null)
				throw new SpineException("IK constraint not found: " + ikConstraintName);
			var timeline = new IkConstraintTimeline(timelineMap.length, timelineMap.length << 1, skeletonData.constraints.indexOf(constraint));

			time = getFloat(keyMap, "time");
			var mix:Float = getFloat(keyMap, "mix", 1);
			var softness:Float = getFloat(keyMap, "softness") * scale;

			frame = 0;
			bezier = 0;
			while (true) {
				timeline.setFrame(frame, time, mix, softness,
					Reflect.hasField(keyMap, "bendPositive") ? (cast(Reflect.getProperty(keyMap, "bendPositive"), Bool) ? 1 : -1) : 1,
					Reflect.hasField(keyMap, "compress") ? cast(Reflect.getProperty(keyMap, "compress"), Bool) : false,
					Reflect.hasField(keyMap, "stretch") ? cast(Reflect.getProperty(keyMap, "stretch"), Bool) : false);

				nextMap = timelineMap[frame + 1];
				if (nextMap == null) {
					timeline.shrink(bezier);
					break;
				}

				time2 = getFloat(nextMap, "time");
				var mix2:Float = getFloat(nextMap, "mix", 1);
				var softness2:Float = getFloat(nextMap, "softness") * scale;

				curve = keyMap.curve;
				if (curve != null) {
					bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mix, mix2, 1);
					bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, softness, softness2, scale);
				}
				time = time2;
				mix = mix2;
				softness = softness2;
				keyMap = nextMap;

				frame++;
			}
			timelines.push(timeline);
		}

		// Transform constraint timelines.
		var transforms:Dynamic = Reflect.getProperty(map, "transform");
		for (transformName in Reflect.fields(transforms)) {
			timelineMap = Reflect.field(transforms, transformName);
			keyMap = timelineMap[0];
			if (keyMap == null)
				continue;
			var constraint = skeletonData.findConstraint(transformName, TransformConstraintData);
			if (constraint == null)
				throw new SpineException("Transform constraint not found: " + transformName);
			var timeline = new TransformConstraintTimeline(timelineMap.length, timelineMap.length * 6, skeletonData.constraints.indexOf(constraint));
			var time = getFloat(keyMap, "time", 0);
			var mixRotate = getFloat(keyMap, "mixRotate", 1);
			var mixX = getFloat(keyMap, "mixX", 1),
				mixY = getFloat(keyMap, "mixY", mixX);
			var mixScaleX:Float = getFloat(keyMap, "mixScaleX", 1),
				mixScaleY:Float = getFloat(keyMap, "mixScaleY", 1);
			var mixShearY:Float = getFloat(keyMap, "mixShearY", 1);

			frame = 0;
			bezier = 0;
			while (true) {
				timeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
				nextMap = timelineMap[frame + 1];
				if (nextMap == null) {
					timeline.shrink(bezier);
					break;
				}

				var time2 = getFloat(nextMap, "time", 0);
				var mixRotate2 = getFloat(nextMap, "mixRotate", 1);
				var mixX2 = getFloat(nextMap, "mixX", 1),
					mixY2 = getFloat(nextMap, "mixY", mixX2);
				var mixScaleX2:Float = getFloat(nextMap, "mixScaleX", 1),
					mixScaleY2:Float = getFloat(nextMap, "mixScaleY", 1);
				var mixShearY2:Float = getFloat(nextMap, "mixShearY", 1);
				var curve = keyMap.curve;
				if (curve != null) {
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

				frame++;
			}

			timelines.push(timeline);
		}

		// Path constraint timelines.
		var paths:Dynamic = Reflect.getProperty(map, "path");
		for (pathName in Reflect.fields(paths)) {
			var constraint = skeletonData.findConstraint(pathName, PathConstraintData);
			if (constraint == null)
				throw new SpineException("Path constraint not found: " + pathName);
			var index = skeletonData.constraints.indexOf(constraint);

			var pathMap:Dynamic = Reflect.field(paths, pathName);
			for (timelineName in Reflect.fields(pathMap)) {
				timelineMap = Reflect.field(pathMap, timelineName);
				keyMap = timelineMap[0];
				if (keyMap == null)
					continue;

				switch (timelineName) {
					case "position":
						var timeline = new PathConstraintPositionTimeline(timelineMap.length, timelineMap.length, index);
						readTimeline(timelines, timelineMap, timeline, 0, constraint.positionMode == PositionMode.fixed ? scale : 1);
					case "spacing":
						var timeline = new PathConstraintSpacingTimeline(timelineMap.length, timelineMap.length, index);
						readTimeline(timelines, timelineMap, timeline,
							0, constraint.spacingMode == SpacingMode.length || constraint.spacingMode == SpacingMode.fixed ? scale : 1);
					case "mix":
						var timeline = new PathConstraintMixTimeline(timelineMap.length, timelineMap.length * 3, index);
						var time = getFloat(keyMap, "time");
						var mixRotate = getFloat(keyMap, "mixRotate", 1);
						var mixX = getFloat(keyMap, "mixX", 1);
						var mixY = getFloat(keyMap, "mixY", mixX);

						frame = 0;
						bezier = 0;
						while (true) {
							timeline.setFrame(frame, time, mixRotate, mixX, mixY);
							var nextMap = timelineMap[frame + 1];
							if (nextMap == null) {
								timeline.shrink(bezier);
								break;
							}
							var time2 = getFloat(nextMap, "time");
							var mixRotate2 = getFloat(nextMap, "mixRotate", 1);
							var mixX2 = getFloat(nextMap, "mixX", 1);
							var mixY2 = getFloat(nextMap, "mixY", mixX2);
							var curve = keyMap.curve;
							if (curve != null) {
								bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
								bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
								bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
							}
							time = time2;
							mixRotate = mixRotate2;
							mixX = mixX2;
							mixY = mixY2;
							keyMap = nextMap;

							frame++;
						}

						timelines.push(timeline);
				}
			}
		}

		// Physics constraint timelines.
		var physics:Dynamic = Reflect.getProperty(map, "physics");
		for (physicsName in Reflect.fields(physics)) {
			var index = -1;
			if (physicsName.length > 0) {
				var constraint = skeletonData.findConstraint(physicsName, PhysicsConstraintData);
				if (constraint == null)
					throw new SpineException("Physics constraint not found: " + physicsName);
				index = skeletonData.constraints.indexOf(constraint);
			}
			var physicsMap:Dynamic = Reflect.field(physics, physicsName);
			for (timelineName in Reflect.fields(physicsMap)) {
				timelineMap = Reflect.field(physicsMap, timelineName);
				keyMap = timelineMap[0];
				if (keyMap == null)
					continue;

				var frames = timelineMap.length;

				var timeline:CurveTimeline1;
				var defaultValue = 0.;
				switch (timelineName) {
					case "reset":
						var resetTimeline = new PhysicsConstraintResetTimeline(frames, index);
						for (frame => keyMap in timelineMap)
							resetTimeline.setFrame(frame, getFloat(keyMap, "time"));
						timelines.push(resetTimeline);
						continue;
					case "inertia":
						timeline = new PhysicsConstraintInertiaTimeline(frames, frames, index);
					case "strength":
						timeline = new PhysicsConstraintStrengthTimeline(frames, frames, index);
					case "damping":
						timeline = new PhysicsConstraintDampingTimeline(frames, frames, index);
					case "mass":
						timeline = new PhysicsConstraintMassTimeline(frames, frames, index);
					case "wind":
						timeline = new PhysicsConstraintWindTimeline(frames, frames, index);
					case "gravity":
						timeline = new PhysicsConstraintGravityTimeline(frames, frames, index);
					case "mix":
						{
							defaultValue = 1;
							timeline = new PhysicsConstraintMixTimeline(frames, frames, index);
						}
					default:
						continue;
				}
				readTimeline(timelines, timelineMap, timeline, defaultValue, 1);
			}
		}

		// Slider timelines.
		var sliders:Dynamic = Reflect.getProperty(map, "slider");
		for (sliderName in Reflect.fields(sliders)) {
			var constraint = skeletonData.findConstraint(sliderName, SliderData);
			if (constraint == null)
				throw new SpineException("Slider not found: " + sliderName);
			var index = skeletonData.constraints.indexOf(constraint);
			var timelineMap:Dynamic = Reflect.field(sliders, sliderName);
			for (timelineName in Reflect.fields(timelineMap)) {
				timelineMap = Reflect.field(timelineMap, timelineName);
				keyMap = timelineMap[0];
				if (keyMap == null)
					continue;

				var frames = timelineMap.length;
				switch (timelineName) {
					case "time":
						readTimeline(timelines, keyMap, new SliderTimeline(frames, frames, index), 1, 1);
					case "mix":
						readTimeline(timelines, keyMap, new SliderMixTimeline(frames, frames, index), 1, 1);
				}
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
					throw new SpineException("Attachment slot not found: " + slotMapName);
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

						switch (timelineMapName) {
							case "deform":
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
							case "sequence":
								var timeline = new SequenceTimeline(timelineMap.length, slotIndex, attachment);
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
		// Draw order timeline.
		if (Reflect.hasField(map, "drawOrder")) {
			var drawOrders:Array<Dynamic> = cast(Reflect.field(map, "drawOrder"), Array<Dynamic>);
			if (drawOrders != null) {
				var drawOrderTimeline:DrawOrderTimeline = new DrawOrderTimeline(drawOrders.length);
				var slotCount:Int = skeletonData.slots.length;
				frame = 0;
				for (drawOrderMap in drawOrders)
					drawOrderTimeline.setFrame(frame++, getFloat(drawOrderMap, "time"), readDrawOrder(skeletonData, drawOrderMap, slotCount, null));
				timelines.push(drawOrderTimeline);
			}
		}

		// Draw order folder timelines.
		if (Reflect.hasField(map, "drawOrderFolder")) {
			var drawOrderFolders:Array<Dynamic> = cast(Reflect.field(map, "drawOrderFolder"), Array<Dynamic>);
			if (drawOrderFolders != null) {
				for (timelineMap in drawOrderFolders) {
					var slotEntries:Array<Dynamic> = cast(Reflect.field(timelineMap, "slots"), Array<Dynamic>);
					var folderSlots:Array<Int> = new Array<Int>();
					folderSlots.resize(slotEntries.length);
					var ii:Int = 0;
					for (slotEntry in slotEntries) {
						var slot = skeletonData.findSlot(cast(slotEntry, String));
						if (slot == null)
							throw new SpineException("Draw order folder slot not found: " + cast(slotEntry, String));
						folderSlots[ii++] = slot.index;
					}

					var keys:Array<Dynamic> = cast(Reflect.field(timelineMap, "keys"), Array<Dynamic>);
					var folderTimeline = new DrawOrderFolderTimeline(keys.length, folderSlots, skeletonData.slots.length);
					frame = 0;
					for (keyMap in keys)
						folderTimeline.setFrame(frame++, getFloat(keyMap, "time"), readDrawOrder(skeletonData, keyMap, folderSlots.length, folderSlots));
					timelines.push(folderTimeline);
				}
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
					var setup = eventData.setupPose;
					var event:Event = new Event(getFloat(eventMap, "time"), eventData);
					event.intValue = Reflect.hasField(eventMap, "int") ? getInt(eventMap, "int") : setup.intValue;

					event.floatValue = Reflect.hasField(eventMap, "float") ? getFloat(eventMap, "float") : setup.floatValue;

					event.stringValue = Reflect.hasField(eventMap, "string") ? Reflect.getProperty(eventMap, "string") : setup.stringValue;
					if (event.data.audioPath != null) {
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

		var animation = new Animation(name, timelines, duration);
		var color:String = Reflect.getProperty(map, "color");
		if (color != null)
			animation.color.setFromString(color);
		skeletonData.animations.push(animation);
	}

	static private function readTimeline(timelines:Array<Timeline>, keys:Array<Dynamic>, timeline:CurveTimeline1, defaultValue:Float, scale:Float) {
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
				timelines.push(timeline);
				return;
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
	}

	static private function readTimeline2(timelines:Array<Timeline>, keys:Array<Dynamic>, timeline:BoneTimeline2, name1:String, name2:String,
			defaultValue:Float, scale:Float) {
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
				timelines.push(timeline);
				return;
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
		if (Reflect.hasField(map, name))
			return Reflect.field(map, name);
		return defaultValue;
	}

	static private function getString(value:Dynamic, name:String, defaultValue:String = ""):String {
		if (Std.isOfType(Reflect.field(value, name), String))
			return cast(Reflect.field(value, name), String);
		return defaultValue;
	}

	static private function getInt(value:Dynamic, name:String, defaultValue:Int = 0):Int {
		if (Std.isOfType(Reflect.field(value, name), Int))
			return cast(Reflect.field(value, name), Int);
		return defaultValue;
	}

	static private function getFloat(value:Dynamic, name:String, defaultValue:Float = 0.):Float {
		if (Std.isOfType(Reflect.field(value, name), Float))
			return cast(Reflect.field(value, name), Float);
		return defaultValue;
	}

	static private function getBoolean(value:Dynamic, name:String, defaultValue:Bool = false):Bool {
		if (Std.isOfType(Reflect.field(value, name), Bool))
			return cast(Reflect.field(value, name), Bool);
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
	public var source(default, null):String;
	public var skin(default, null):String;
	public var slotIndex(default, null):Int;
	public var sourceIndex(default, null):Int;
	public var mesh(default, null):MeshAttachment;
	public var inheritTimelines(default, null):Bool;

	public function new(mesh:MeshAttachment, skin:String, slotIndex:Int, sourceIndex:Int, source:String, inheritTimelines:Bool) {
		this.mesh = mesh;
		this.skin = skin;
		this.slotIndex = slotIndex;
		this.sourceIndex = sourceIndex;
		this.source = source;
		this.inheritTimelines = inheritTimelines;
	}
}
