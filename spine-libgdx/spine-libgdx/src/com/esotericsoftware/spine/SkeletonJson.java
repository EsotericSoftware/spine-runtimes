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

package com.esotericsoftware.spine;

import static com.esotericsoftware.spine.utils.SpineUtils.*;

import java.io.InputStream;
import java.util.Arrays;

import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.JsonMatcher;
import com.badlogic.gdx.utils.JsonValue;
import com.badlogic.gdx.utils.Null;
import com.badlogic.gdx.utils.SerializationException;

import com.esotericsoftware.spine.Animation.AlphaTimeline;
import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.BoneTimeline2;
import com.esotericsoftware.spine.Animation.CurveTimeline;
import com.esotericsoftware.spine.Animation.CurveTimeline1;
import com.esotericsoftware.spine.Animation.DeformTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderFolderTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderTimeline;
import com.esotericsoftware.spine.Animation.EventTimeline;
import com.esotericsoftware.spine.Animation.IkConstraintTimeline;
import com.esotericsoftware.spine.Animation.InheritTimeline;
import com.esotericsoftware.spine.Animation.PathConstraintMixTimeline;
import com.esotericsoftware.spine.Animation.PathConstraintPositionTimeline;
import com.esotericsoftware.spine.Animation.PathConstraintSpacingTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintDampingTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintGravityTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintInertiaTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintMassTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintMixTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintResetTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintStrengthTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintWindTimeline;
import com.esotericsoftware.spine.Animation.RGB2Timeline;
import com.esotericsoftware.spine.Animation.RGBA2Timeline;
import com.esotericsoftware.spine.Animation.RGBATimeline;
import com.esotericsoftware.spine.Animation.RGBTimeline;
import com.esotericsoftware.spine.Animation.RotateTimeline;
import com.esotericsoftware.spine.Animation.ScaleTimeline;
import com.esotericsoftware.spine.Animation.ScaleXTimeline;
import com.esotericsoftware.spine.Animation.ScaleYTimeline;
import com.esotericsoftware.spine.Animation.SequenceTimeline;
import com.esotericsoftware.spine.Animation.ShearTimeline;
import com.esotericsoftware.spine.Animation.ShearXTimeline;
import com.esotericsoftware.spine.Animation.ShearYTimeline;
import com.esotericsoftware.spine.Animation.SliderMixTimeline;
import com.esotericsoftware.spine.Animation.SliderTimeline;
import com.esotericsoftware.spine.Animation.Timeline;
import com.esotericsoftware.spine.Animation.TransformConstraintTimeline;
import com.esotericsoftware.spine.Animation.TranslateTimeline;
import com.esotericsoftware.spine.Animation.TranslateXTimeline;
import com.esotericsoftware.spine.Animation.TranslateYTimeline;
import com.esotericsoftware.spine.BoneData.Inherit;
import com.esotericsoftware.spine.ConstraintData.ScaleYMode;
import com.esotericsoftware.spine.PathConstraintData.PositionMode;
import com.esotericsoftware.spine.PathConstraintData.RotateMode;
import com.esotericsoftware.spine.PathConstraintData.SpacingMode;
import com.esotericsoftware.spine.TransformConstraintData.FromProperty;
import com.esotericsoftware.spine.TransformConstraintData.FromRotate;
import com.esotericsoftware.spine.TransformConstraintData.FromScaleX;
import com.esotericsoftware.spine.TransformConstraintData.FromScaleY;
import com.esotericsoftware.spine.TransformConstraintData.FromShearY;
import com.esotericsoftware.spine.TransformConstraintData.FromX;
import com.esotericsoftware.spine.TransformConstraintData.FromY;
import com.esotericsoftware.spine.TransformConstraintData.ToProperty;
import com.esotericsoftware.spine.TransformConstraintData.ToRotate;
import com.esotericsoftware.spine.TransformConstraintData.ToScaleX;
import com.esotericsoftware.spine.TransformConstraintData.ToScaleY;
import com.esotericsoftware.spine.TransformConstraintData.ToShearY;
import com.esotericsoftware.spine.TransformConstraintData.ToX;
import com.esotericsoftware.spine.TransformConstraintData.ToY;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.AttachmentLoader;
import com.esotericsoftware.spine.attachments.AttachmentType;
import com.esotericsoftware.spine.attachments.BoundingBoxAttachment;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.attachments.MeshAttachment;
import com.esotericsoftware.spine.attachments.PathAttachment;
import com.esotericsoftware.spine.attachments.PointAttachment;
import com.esotericsoftware.spine.attachments.RegionAttachment;
import com.esotericsoftware.spine.attachments.Sequence;
import com.esotericsoftware.spine.attachments.Sequence.SequenceMode;
import com.esotericsoftware.spine.attachments.VertexAttachment;

/** Loads skeleton data in the Spine JSON format.
 * <p>
 * JSON is human readable but the binary format is much smaller on disk and faster to load. See {@link SkeletonBinary}.
 * <p>
 * See <a href="https://esotericsoftware.com/spine-json-format">Spine JSON format</a> and
 * <a href="https://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data">JSON and binary data</a> in the Spine
 * Runtimes Guide. */
public class SkeletonJson extends SkeletonLoader {
	private final Array<LinkedMesh> linkedMeshes = new Array(true, 8, LinkedMesh[]::new);

	private final JsonMatcher parser = new JsonMatcher();

	public SkeletonJson (AttachmentLoader attachmentLoader) {
		super(attachmentLoader);
	}

	public SkeletonJson (TextureAtlas atlas) {
		super(atlas);
	}

	public SkeletonData readSkeletonData (FileHandle file) {
		if (file == null) throw new IllegalArgumentException("file cannot be null.");
		try {
			SkeletonData skeletonData = readSkeletonData(parser.parseValue(file));
			skeletonData.name = file.nameWithoutExtension();
			return skeletonData;
		} catch (Throwable ex) {
			throw new SerializationException("Error reading JSON skeleton file: " + file, ex);
		}
	}

	public SkeletonData readSkeletonData (InputStream input) {
		if (input == null) throw new IllegalArgumentException("dataInput cannot be null.");
		try {
			return readSkeletonData(parser.parseValue(input));
		} catch (Throwable ex) {
			throw new SerializationException("Error reading JSON skeleton data.", ex);
		}
	}

	public SkeletonData readSkeletonData (JsonValue root) {
		if (root == null) throw new IllegalArgumentException("root cannot be null.");

		var skeletonData = new SkeletonData();
		try {
			float scale = this.scale;

			// Skeleton.
			JsonValue skeletonMap = root.get("skeleton");
			if (skeletonMap != null) {
				skeletonData.hash = skeletonMap.getString("hash", null);
				skeletonData.version = skeletonMap.getString("spine", null);
				skeletonData.x = skeletonMap.getFloat("x", 0);
				skeletonData.y = skeletonMap.getFloat("y", 0);
				skeletonData.width = skeletonMap.getFloat("width", 0);
				skeletonData.height = skeletonMap.getFloat("height", 0);
				skeletonData.referenceScale = skeletonMap.getFloat("referenceScale", 100) * scale;
				skeletonData.fps = skeletonMap.getFloat("fps", 30);
				skeletonData.imagesPath = skeletonMap.getString("images", null);
				skeletonData.audioPath = skeletonMap.getString("audio", null);
			}

			// Bones.
			for (JsonValue boneMap = root.getChild("bones"); boneMap != null; boneMap = boneMap.next) {
				BoneData parent = null;
				String parentName = boneMap.getString("parent", null);
				if (parentName != null) {
					parent = skeletonData.findBone(parentName);
					if (parent == null) throw new SerializationException("Parent bone not found: " + parentName);
				}
				var data = new BoneData(skeletonData.bones.size, boneMap.getString("name"), parent);
				data.length = boneMap.getFloat("length", 0) * scale;
				BonePose setup = data.setupPose;
				setup.x = boneMap.getFloat("x", 0) * scale;
				setup.y = boneMap.getFloat("y", 0) * scale;
				setup.rotation = boneMap.getFloat("rotation", 0);
				setup.scaleX = boneMap.getFloat("scaleX", 1);
				setup.scaleY = boneMap.getFloat("scaleY", 1);
				setup.shearX = boneMap.getFloat("shearX", 0);
				setup.shearY = boneMap.getFloat("shearY", 0);
				setup.inherit = Inherit.valueOf(boneMap.getString("inherit", Inherit.normal.name()));
				data.skinRequired = boneMap.getBoolean("skin", false);

				String color = boneMap.getString("color", null);
				if (color != null) Color.valueOf(color, data.getColor());

				data.icon = boneMap.getString("icon", null);
				data.iconSize = boneMap.getFloat("iconSize", 1);
				data.iconRotation = boneMap.getFloat("iconRotation", 0);
				data.visible = boneMap.getBoolean("visible", true);

				skeletonData.bones.add(data);
			}

			// Slots.
			for (JsonValue slotMap = root.getChild("slots"); slotMap != null; slotMap = slotMap.next) {
				String slotName = slotMap.getString("name");
				String boneName = slotMap.getString("bone");
				BoneData boneData = skeletonData.findBone(boneName);
				if (boneData == null) throw new SerializationException("Slot bone not found: " + boneName);

				var data = new SlotData(skeletonData.slots.size, slotName, boneData);

				String color = slotMap.getString("color", null);
				if (color != null) Color.valueOf(color, data.setupPose.getColor());

				String dark = slotMap.getString("dark", null);
				if (dark != null) data.setupPose.darkColor = Color.valueOf(dark);

				data.attachmentName = slotMap.getString("attachment", null);
				data.blendMode = BlendMode.valueOf(slotMap.getString("blend", BlendMode.normal.name()));
				data.visible = slotMap.getBoolean("visible", true);
				skeletonData.slots.add(data);
			}

			// Constraints.
			for (JsonValue constraintMap = root.getChild("constraints"); constraintMap != null; constraintMap = constraintMap.next) {
				String name = constraintMap.getString("name");
				boolean skinRequired = constraintMap.getBoolean("skin", false);
				switch (constraintMap.getString("type")) {
				case "ik" -> {
					var data = new IkConstraintData(name);
					data.skinRequired = skinRequired;

					for (JsonValue entry = constraintMap.getChild("bones"); entry != null; entry = entry.next) {
						BoneData bone = skeletonData.findBone(entry.asString());
						if (bone == null) throw new SerializationException("IK bone not found: " + entry);
						data.bones.add(bone);
					}

					String targetName = constraintMap.getString("target");
					data.target = skeletonData.findBone(targetName);
					if (data.target == null) throw new SerializationException("IK target bone not found: " + targetName);

					String scaleY = constraintMap.getString("scaleY", null);
					if (scaleY != null) data.scaleYMode = ScaleYMode.valueOf(scaleY);

					IkConstraintPose setup = data.setupPose;
					setup.mix = constraintMap.getFloat("mix", 1);
					setup.softness = constraintMap.getFloat("softness", 0) * scale;
					setup.bendDirection = constraintMap.getBoolean("bendPositive", true) ? 1 : -1;
					setup.compress = constraintMap.getBoolean("compress", false);
					setup.stretch = constraintMap.getBoolean("stretch", false);

					skeletonData.constraints.add(data);
				}
				case "transform" -> {
					var data = new TransformConstraintData(name);
					data.skinRequired = skinRequired;

					for (JsonValue entry = constraintMap.getChild("bones"); entry != null; entry = entry.next) {
						BoneData bone = skeletonData.findBone(entry.asString());
						if (bone == null) throw new SerializationException("Transform constraint bone not found: " + entry);
						data.bones.add(bone);
					}

					String sourceName = constraintMap.getString("source");
					data.source = skeletonData.findBone(sourceName);
					if (data.source == null)
						throw new SerializationException("Transform constraint source bone not found: " + sourceName);

					data.localSource = constraintMap.getBoolean("localSource", false);
					data.localTarget = constraintMap.getBoolean("localTarget", false);
					data.additive = constraintMap.getBoolean("additive", false);
					data.clamp = constraintMap.getBoolean("clamp", false);

					boolean rotate = false, x = false, y = false, scaleX = false, scaleY = false, shearY = false;
					for (JsonValue fromEntry = constraintMap.getChild("properties"); fromEntry != null; fromEntry = fromEntry.next) {
						FromProperty from = fromProperty(fromEntry.name);
						float fromScale = propertyScale(fromEntry.name, scale);
						from.offset = fromEntry.getFloat("offset", 0) * fromScale;
						for (JsonValue toEntry = fromEntry.getChild("to"); toEntry != null; toEntry = toEntry.next) {
							float toScale = 1;
							ToProperty to;
							switch (toEntry.name) {
							case "rotate" -> {
								rotate = true;
								to = new ToRotate();
							}
							case "x" -> {
								x = true;
								to = new ToX();
								toScale = scale;
							}
							case "y" -> {
								y = true;
								to = new ToY();
								toScale = scale;
							}
							case "scaleX" -> {
								scaleX = true;
								to = new ToScaleX();
							}
							case "scaleY" -> {
								scaleY = true;
								to = new ToScaleY();
							}
							case "shearY" -> {
								shearY = true;
								to = new ToShearY();
							}
							default -> throw new SerializationException("Invalid transform constraint to property: " + toEntry.name);
							}
							to.offset = toEntry.getFloat("offset", 0) * toScale;
							to.max = toEntry.getFloat("max", 1) * toScale;
							to.scale = toEntry.getFloat("scale", 1) * toScale / fromScale;
							from.to.add(to);
						}
						if (from.to.notEmpty()) data.properties.add(from);
					}

					data.offsets[TransformConstraintData.ROTATION] = constraintMap.getFloat("rotation", 0);
					data.offsets[TransformConstraintData.X] = constraintMap.getFloat("x", 0) * scale;
					data.offsets[TransformConstraintData.Y] = constraintMap.getFloat("y", 0) * scale;
					data.offsets[TransformConstraintData.SCALEX] = constraintMap.getFloat("scaleX", 0);
					data.offsets[TransformConstraintData.SCALEY] = constraintMap.getFloat("scaleY", 0);
					data.offsets[TransformConstraintData.SHEARY] = constraintMap.getFloat("shearY", 0);

					TransformConstraintPose setup = data.setupPose;
					if (rotate) setup.mixRotate = constraintMap.getFloat("mixRotate", 1);
					if (x) setup.mixX = constraintMap.getFloat("mixX", 1);
					if (y) setup.mixY = constraintMap.getFloat("mixY", setup.mixX);
					if (scaleX) setup.mixScaleX = constraintMap.getFloat("mixScaleX", 1);
					if (scaleY) setup.mixScaleY = constraintMap.getFloat("mixScaleY", setup.mixScaleX);
					if (shearY) setup.mixShearY = constraintMap.getFloat("mixShearY", 1);

					skeletonData.constraints.add(data);
				}
				case "path" -> {
					var data = new PathConstraintData(name);
					data.skinRequired = skinRequired;

					for (JsonValue entry = constraintMap.getChild("bones"); entry != null; entry = entry.next) {
						BoneData bone = skeletonData.findBone(entry.asString());
						if (bone == null) throw new SerializationException("Path bone not found: " + entry);
						data.bones.add(bone);
					}

					String slotName = constraintMap.getString("slot");
					data.slot = skeletonData.findSlot(slotName);
					if (data.slot == null) throw new SerializationException("Path slot not found: " + slotName);

					data.positionMode = PositionMode.valueOf(constraintMap.getString("positionMode", "percent"));
					data.spacingMode = SpacingMode.valueOf(constraintMap.getString("spacingMode", "length"));
					data.rotateMode = RotateMode.valueOf(constraintMap.getString("rotateMode", "tangent"));
					data.offsetRotation = constraintMap.getFloat("rotation", 0);
					PathConstraintPose setup = data.setupPose;
					setup.position = constraintMap.getFloat("position", 0);
					if (data.positionMode == PositionMode.fixed) setup.position *= scale;
					setup.spacing = constraintMap.getFloat("spacing", 0);
					if (data.spacingMode == SpacingMode.length || data.spacingMode == SpacingMode.fixed) setup.spacing *= scale;
					setup.mixRotate = constraintMap.getFloat("mixRotate", 1);
					setup.mixX = constraintMap.getFloat("mixX", 1);
					setup.mixY = constraintMap.getFloat("mixY", setup.mixX);

					skeletonData.constraints.add(data);
				}
				case "physics" -> {
					var data = new PhysicsConstraintData(name);
					data.skinRequired = skinRequired;

					String boneName = constraintMap.getString("bone");
					data.bone = skeletonData.findBone(boneName);
					if (data.bone == null) throw new SerializationException("Physics bone not found: " + boneName);

					data.x = constraintMap.getFloat("x", 0);
					data.y = constraintMap.getFloat("y", 0);
					data.rotate = constraintMap.getFloat("rotate", 0);
					data.scaleX = constraintMap.getFloat("scaleX", 0);

					String scaleY = constraintMap.getString("scaleY", null);
					if (scaleY != null) data.scaleYMode = ScaleYMode.valueOf(scaleY);

					data.shearX = constraintMap.getFloat("shearX", 0);
					data.limit = constraintMap.getFloat("limit", 5000) * scale;
					data.step = 1f / constraintMap.getInt("fps", 60);
					PhysicsConstraintPose setup = data.setupPose;
					setup.inertia = constraintMap.getFloat("inertia", 0.5f);
					setup.strength = constraintMap.getFloat("strength", 100);
					setup.damping = constraintMap.getFloat("damping", 0.85f);
					setup.massInverse = 1 / constraintMap.getFloat("mass", 1);
					setup.wind = constraintMap.getFloat("wind", 0);
					setup.gravity = constraintMap.getFloat("gravity", 0);
					setup.mix = constraintMap.getFloat("mix", 1);
					data.inertiaGlobal = constraintMap.getBoolean("inertiaGlobal", false);
					data.strengthGlobal = constraintMap.getBoolean("strengthGlobal", false);
					data.dampingGlobal = constraintMap.getBoolean("dampingGlobal", false);
					data.massGlobal = constraintMap.getBoolean("massGlobal", false);
					data.windGlobal = constraintMap.getBoolean("windGlobal", false);
					data.gravityGlobal = constraintMap.getBoolean("gravityGlobal", false);
					data.mixGlobal = constraintMap.getBoolean("mixGlobal", false);

					skeletonData.constraints.add(data);
				}
				case "slider" -> {
					var data = new SliderData(name);
					data.skinRequired = skinRequired;
					data.additive = constraintMap.getBoolean("additive", false);
					data.loop = constraintMap.getBoolean("loop", false);
					data.setupPose.mix = constraintMap.getFloat("mix", 1);

					String boneName = constraintMap.getString("bone", null);
					if (boneName != null) {
						data.bone = skeletonData.findBone(boneName);
						if (data.bone == null) throw new SerializationException("Slider bone not found: " + boneName);
						String property = constraintMap.getString("property");
						data.property = fromProperty(property);
						float propertyScale = propertyScale(property, scale);
						data.property.offset = constraintMap.getFloat("from", 0) * propertyScale;
						data.offset = constraintMap.getFloat("to", 0);
						data.scale = constraintMap.getFloat("scale", 1) / propertyScale;
						data.max = constraintMap.getFloat("max", 0);
						data.local = constraintMap.getBoolean("local", false);
					} else
						data.setupPose.time = constraintMap.getFloat("time", 0);

					skeletonData.constraints.add(data);
				}
				}
			}

			// Skins.
			for (JsonValue skinMap = root.getChild("skins"); skinMap != null; skinMap = skinMap.next) {
				var skin = new Skin(skinMap.getString("name"));
				for (JsonValue entry = skinMap.getChild("bones"); entry != null; entry = entry.next) {
					BoneData bone = skeletonData.findBone(entry.asString());
					if (bone == null) throw new SerializationException("Skin bone not found: " + entry);
					skin.bones.add(bone);
				}
				skin.bones.shrink();
				for (JsonValue entry = skinMap.getChild("ik"); entry != null; entry = entry.next) {
					IkConstraintData constraint = skeletonData.findConstraint(entry.asString(), IkConstraintData.class);
					if (constraint == null) throw new SerializationException("Skin IK constraint not found: " + entry);
					skin.constraints.add(constraint);
				}
				for (JsonValue entry = skinMap.getChild("transform"); entry != null; entry = entry.next) {
					TransformConstraintData constraint = skeletonData.findConstraint(entry.asString(), TransformConstraintData.class);
					if (constraint == null) throw new SerializationException("Skin transform constraint not found: " + entry);
					skin.constraints.add(constraint);
				}
				for (JsonValue entry = skinMap.getChild("path"); entry != null; entry = entry.next) {
					PathConstraintData constraint = skeletonData.findConstraint(entry.asString(), PathConstraintData.class);
					if (constraint == null) throw new SerializationException("Skin path constraint not found: " + entry);
					skin.constraints.add(constraint);
				}
				for (JsonValue entry = skinMap.getChild("physics"); entry != null; entry = entry.next) {
					PhysicsConstraintData constraint = skeletonData.findConstraint(entry.asString(), PhysicsConstraintData.class);
					if (constraint == null) throw new SerializationException("Skin physics constraint not found: " + entry);
					skin.constraints.add(constraint);
				}
				for (JsonValue entry = skinMap.getChild("slider"); entry != null; entry = entry.next) {
					SliderData constraint = skeletonData.findConstraint(entry.asString(), SliderData.class);
					if (constraint == null) throw new SerializationException("Skin slider not found: " + entry);
					skin.constraints.add(constraint);
				}
				skin.constraints.shrink();
				for (JsonValue slotEntry = skinMap.getChild("attachments"); slotEntry != null; slotEntry = slotEntry.next) {
					SlotData slot = skeletonData.findSlot(slotEntry.name);
					if (slot == null) throw new SerializationException("Skin slot not found: " + slotEntry.name);
					for (JsonValue entry = slotEntry.child; entry != null; entry = entry.next) {
						try {
							Attachment attachment = readAttachment(entry, skin, slot.index, entry.name, skeletonData);
							if (attachment != null) skin.setAttachment(slot.index, entry.name, attachment);
						} catch (Throwable ex) {
							throw new SerializationException("Error reading attachment: " + entry.name + ", skin: " + skin, ex);
						}
					}
				}

				String color = skinMap.getString("color", null);
				if (color != null) Color.valueOf(color, skin.getColor());

				skeletonData.skins.add(skin);
				if (skin.name.equals("default")) skeletonData.defaultSkin = skin;
			}

			// Linked meshes.
			LinkedMesh[] items = linkedMeshes.items;
			for (int i = 0, n = linkedMeshes.size; i < n; i++) {
				LinkedMesh linkedMesh = items[i];
				Skin skin = linkedMesh.skin == null ? skeletonData.defaultSkin : skeletonData.findSkin(linkedMesh.skin);
				if (skin == null) throw new SerializationException("Skin not found: " + linkedMesh.skin);
				Attachment source = skin.getAttachment(linkedMesh.sourceIndex, linkedMesh.source);
				if (source == null) throw new SerializationException("Source mesh not found: " + linkedMesh.source);
				linkedMesh.mesh.setTimelineAttachment(linkedMesh.inheritTimelines ? (VertexAttachment)source : linkedMesh.mesh);
				linkedMesh.mesh.setSourceMesh((MeshAttachment)source);
				linkedMesh.mesh.updateSequence();
				outer:
				if (linkedMesh.inheritTimelines && linkedMesh.slotIndex != linkedMesh.sourceIndex) {
					int[] slots = source.getTimelineSlots();
					for (int existing : slots)
						if (existing == linkedMesh.slotIndex) break outer;
					int[] newSlots = Arrays.copyOf(slots, slots.length + 1);
					newSlots[slots.length] = linkedMesh.slotIndex;
					source.setTimelineSlots(newSlots);
				}
			}
			linkedMeshes.clear();

			// Events.
			for (JsonValue eventMap = root.getChild("events"); eventMap != null; eventMap = eventMap.next) {
				var data = new EventData(eventMap.name);
				Event setup = data.setupPose;
				setup.intValue = eventMap.getInt("int", 0);
				setup.floatValue = eventMap.getFloat("float", 0f);
				setup.stringValue = eventMap.getString("string", "");
				data.audioPath = eventMap.getString("audio", null);
				if (data.audioPath != null) {
					setup.volume = eventMap.getFloat("volume", 1);
					setup.balance = eventMap.getFloat("balance", 0);
				}
				skeletonData.events.add(data);
			}

			// Animations.
			for (JsonValue animationMap = root.getChild("animations"); animationMap != null; animationMap = animationMap.next) {
				try {
					readAnimation(animationMap, animationMap.name, skeletonData);
				} catch (Throwable ex) {
					throw new SerializationException("Error reading animation: " + animationMap.name, ex);
				}
			}

			// Slider animations.
			for (JsonValue constraintMap = root.getChild("constraints"); constraintMap != null; constraintMap = constraintMap.next) {
				if (constraintMap.getString("type").equals("slider")) {
					SliderData data = skeletonData.findConstraint(constraintMap.getString("name"), SliderData.class);
					String animationName = constraintMap.getString("animation");
					data.animation = skeletonData.findAnimation(animationName);
					if (data.animation == null) throw new SerializationException("Slider animation not found: " + animationName);
				}
			}

			skeletonData.bones.shrink();
			skeletonData.slots.shrink();
			skeletonData.skins.shrink();
			skeletonData.events.shrink();
			skeletonData.animations.shrink();
			skeletonData.constraints.shrink();
			return skeletonData;
		} catch (Throwable ex) {
			if (skeletonData.version != null)
				throw new SerializationException("Error reading JSON skeleton data, version: " + skeletonData.version, ex);
			throw new SerializationException("Error JSON skeleton data.", ex);
		}
	}

	private FromProperty fromProperty (String type) {
		return switch (type) {
		case "rotate" -> new FromRotate();
		case "x" -> new FromX();
		case "y" -> new FromY();
		case "scaleX" -> new FromScaleX();
		case "scaleY" -> new FromScaleY();
		case "shearY" -> new FromShearY();
		default -> throw new SerializationException("Invalid from property: " + type);
		};
	}

	private float propertyScale (String type, float scale) {
		return switch (type) {
		case "x", "y" -> scale;
		default -> 1;
		};
	}

	private Attachment readAttachment (JsonValue map, Skin skin, int slotIndex, String placeholder, SkeletonData skeletonData) {
		float scale = this.scale;
		String name = map.getString("name", placeholder);

		return switch (AttachmentType.valueOf(map.getString("type", AttachmentType.region.name()))) {
		case region -> {
			String path = map.getString("path", name);
			Sequence sequence = readSequence(map.get("sequence"));
			RegionAttachment region = attachmentLoader.newRegionAttachment(skin, placeholder, name, path, sequence);
			if (region == null) yield null;
			region.setPath(path);
			region.setX(map.getFloat("x", 0) * scale);
			region.setY(map.getFloat("y", 0) * scale);
			region.setScaleX(map.getFloat("scaleX", 1));
			region.setScaleY(map.getFloat("scaleY", 1));
			region.setRotation(map.getFloat("rotation", 0));
			region.setWidth(map.getFloat("width") * scale);
			region.setHeight(map.getFloat("height") * scale);

			String color = map.getString("color", null);
			if (color != null) Color.valueOf(color, region.getColor());

			region.updateSequence();
			yield region;
		}
		case boundingbox -> {
			BoundingBoxAttachment box = attachmentLoader.newBoundingBoxAttachment(skin, placeholder, name);
			if (box == null) yield null;
			readVertices(map, box, map.getInt("vertexCount") << 1);

			String color = map.getString("color", null);
			if (color != null) Color.valueOf(color, box.getColor());
			yield box;
		}
		case mesh, linkedmesh -> {
			String path = map.getString("path", name);
			Sequence sequence = readSequence(map.get("sequence"));
			MeshAttachment mesh = attachmentLoader.newMeshAttachment(skin, placeholder, name, path, sequence);
			if (mesh == null) yield null;
			mesh.setPath(path);

			String color = map.getString("color", null);
			if (color != null) Color.valueOf(color, mesh.getColor());

			mesh.setWidth(map.getFloat("width", 0) * scale);
			mesh.setHeight(map.getFloat("height", 0) * scale);

			String source = map.getString("source", null);
			if (source != null) {
				int sourceIndex = slotIndex;
				String slot = map.getString("slot", null);
				if (slot != null) {
					SlotData sourceSlot = skeletonData.findSlot(slot);
					if (sourceSlot == null) throw new SerializationException("Source mesh slot not found: " + slot);
					sourceIndex = sourceSlot.index;
				}
				linkedMeshes.add(new LinkedMesh(mesh, map.getString("skin", null), slotIndex, sourceIndex, source,
					map.getBoolean("timelines", true)));
				yield mesh;
			}

			float[] uvs = map.require("uvs").asFloatArray();
			readVertices(map, mesh, uvs.length);
			mesh.setTriangles(map.require("triangles").asShortArray());
			mesh.setRegionUVs(uvs);

			if (map.has("hull")) mesh.setHullLength(map.require("hull").asInt() << 1);
			if (map.has("edges")) mesh.setEdges(map.require("edges").asShortArray());

			mesh.updateSequence();
			yield mesh;
		}
		case path -> {
			PathAttachment path = attachmentLoader.newPathAttachment(skin, placeholder, name);
			if (path == null) yield null;
			path.setClosed(map.getBoolean("closed", false));
			path.setConstantSpeed(map.getBoolean("constantSpeed", true));

			int vertexCount = map.getInt("vertexCount");
			readVertices(map, path, vertexCount << 1);

			var lengths = new float[vertexCount / 3];
			int i = 0;
			for (JsonValue curves = map.require("lengths").child; curves != null; curves = curves.next)
				lengths[i++] = curves.asFloat() * scale;
			path.setLengths(lengths);

			String color = map.getString("color", null);
			if (color != null) Color.valueOf(color, path.getColor());
			yield path;
		}
		case point -> {
			PointAttachment point = attachmentLoader.newPointAttachment(skin, placeholder, name);
			if (point == null) yield null;
			point.setX(map.getFloat("x", 0) * scale);
			point.setY(map.getFloat("y", 0) * scale);
			point.setRotation(map.getFloat("rotation", 0));

			String color = map.getString("color", null);
			if (color != null) Color.valueOf(color, point.getColor());
			yield point;
		}
		case clipping -> {
			ClippingAttachment clip = attachmentLoader.newClippingAttachment(skin, placeholder, name);
			if (clip == null) yield null;

			String end = map.getString("end", null);
			if (end != null) {
				SlotData slot = skeletonData.findSlot(end);
				if (slot == null) throw new SerializationException("Clipping end slot not found: " + end);
				clip.setEndSlot(slot);
			}

			clip.setConvex(map.getBoolean("convex", false));
			clip.setInverse(map.getBoolean("inverse", false));

			readVertices(map, clip, map.getInt("vertexCount") << 1);

			String color = map.getString("color", null);
			if (color != null) Color.valueOf(color, clip.getColor());
			yield clip;
		}
		default -> null;
		};
	}

	private Sequence readSequence (@Null JsonValue map) {
		if (map == null) return new Sequence(1, false);
		var sequence = new Sequence(map.getInt("count"), true);
		sequence.setStart(map.getInt("start", 1));
		sequence.setDigits(map.getInt("digits", 0));
		sequence.setSetupIndex(map.getInt("setup", 0));
		return sequence;
	}

	private void readVertices (JsonValue map, VertexAttachment attachment, int verticesLength) {
		attachment.setWorldVerticesLength(verticesLength);
		float[] vertices = map.require("vertices").asFloatArray();
		if (verticesLength == vertices.length) {
			if (scale != 1) {
				for (int i = 0, n = vertices.length; i < n; i++)
					vertices[i] *= scale;
			}
			attachment.setVertices(vertices);
			return;
		}
		var weights = new FloatArray(verticesLength * 3 * 3);
		var bones = new IntArray(verticesLength * 3);
		for (int i = 0, n = vertices.length; i < n;) {
			int boneCount = (int)vertices[i++];
			bones.add(boneCount);
			for (int nn = i + (boneCount << 2); i < nn; i += 4) {
				bones.add((int)vertices[i]);
				weights.add(vertices[i + 1] * scale);
				weights.add(vertices[i + 2] * scale);
				weights.add(vertices[i + 3]);
			}
		}
		attachment.setBones(bones.toArray());
		attachment.setVertices(weights.toArray());
	}

	private void readAnimation (JsonValue map, String name, SkeletonData skeletonData) {
		float scale = this.scale;
		var timelines = new Array<Timeline>(true, 16, Timeline[]::new);

		// Slot timelines.
		for (JsonValue slotMap = map.getChild("slots"); slotMap != null; slotMap = slotMap.next) {
			SlotData slot = skeletonData.findSlot(slotMap.name);
			if (slot == null) throw new SerializationException("Slot not found: " + slotMap.name);
			for (JsonValue timelineMap = slotMap.child; timelineMap != null; timelineMap = timelineMap.next) {
				JsonValue keyMap = timelineMap.child;
				if (keyMap == null) continue;

				int frames = timelineMap.size;
				switch (timelineMap.name) {
				case "attachment" -> {
					var timeline = new AttachmentTimeline(frames, slot.index);
					for (int frame = 0; keyMap != null; keyMap = keyMap.next, frame++)
						timeline.setFrame(frame, keyMap.getFloat("time", 0), keyMap.getString("name", null));
					timelines.add(timeline);
				}
				case "rgba" -> {
					var timeline = new RGBATimeline(frames, frames << 2, slot.index);
					float time = keyMap.getFloat("time", 0);
					String color = keyMap.getString("color");
					float r = Integer.parseInt(color.substring(0, 2), 16) / 255f;
					float g = Integer.parseInt(color.substring(2, 4), 16) / 255f;
					float b = Integer.parseInt(color.substring(4, 6), 16) / 255f;
					float a = Integer.parseInt(color.substring(6, 8), 16) / 255f;
					for (int frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, r, g, b, a);
						JsonValue nextMap = keyMap.next;
						if (nextMap == null) {
							timeline.shrink(bezier);
							break;
						}
						float time2 = nextMap.getFloat("time", 0);
						color = nextMap.getString("color");
						float nr = Integer.parseInt(color.substring(0, 2), 16) / 255f;
						float ng = Integer.parseInt(color.substring(2, 4), 16) / 255f;
						float nb = Integer.parseInt(color.substring(4, 6), 16) / 255f;
						float na = Integer.parseInt(color.substring(6, 8), 16) / 255f;
						JsonValue curve = keyMap.get("curve");
						if (curve != null) {
							bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, r, nr, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, g, ng, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, b, nb, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, a, na, 1);
						}
						time = time2;
						r = nr;
						g = ng;
						b = nb;
						a = na;
						keyMap = nextMap;
					}
					timelines.add(timeline);
				}
				case "rgb" -> {
					var timeline = new RGBTimeline(frames, frames * 3, slot.index);
					float time = keyMap.getFloat("time", 0);
					String color = keyMap.getString("color");
					float r = Integer.parseInt(color.substring(0, 2), 16) / 255f;
					float g = Integer.parseInt(color.substring(2, 4), 16) / 255f;
					float b = Integer.parseInt(color.substring(4, 6), 16) / 255f;
					for (int frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, r, g, b);
						JsonValue nextMap = keyMap.next;
						if (nextMap == null) {
							timeline.shrink(bezier);
							break;
						}
						float time2 = nextMap.getFloat("time", 0);
						color = nextMap.getString("color");
						float nr = Integer.parseInt(color.substring(0, 2), 16) / 255f;
						float ng = Integer.parseInt(color.substring(2, 4), 16) / 255f;
						float nb = Integer.parseInt(color.substring(4, 6), 16) / 255f;
						JsonValue curve = keyMap.get("curve");
						if (curve != null) {
							bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, r, nr, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, g, ng, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, b, nb, 1);
						}
						time = time2;
						r = nr;
						g = ng;
						b = nb;
						keyMap = nextMap;
					}
					timelines.add(timeline);
				}
				case "alpha" -> readTimeline(timelines, keyMap, new AlphaTimeline(frames, frames, slot.index), 0, 1);
				case "rgba2" -> {
					var timeline = new RGBA2Timeline(frames, frames * 7, slot.index);
					float time = keyMap.getFloat("time", 0);
					String color = keyMap.getString("light");
					float r = Integer.parseInt(color.substring(0, 2), 16) / 255f;
					float g = Integer.parseInt(color.substring(2, 4), 16) / 255f;
					float b = Integer.parseInt(color.substring(4, 6), 16) / 255f;
					float a = Integer.parseInt(color.substring(6, 8), 16) / 255f;
					color = keyMap.getString("dark");
					float r2 = Integer.parseInt(color.substring(0, 2), 16) / 255f;
					float g2 = Integer.parseInt(color.substring(2, 4), 16) / 255f;
					float b2 = Integer.parseInt(color.substring(4, 6), 16) / 255f;
					for (int frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, r, g, b, a, r2, g2, b2);
						JsonValue nextMap = keyMap.next;
						if (nextMap == null) {
							timeline.shrink(bezier);
							break;
						}
						float time2 = nextMap.getFloat("time", 0);
						color = nextMap.getString("light");
						float nr = Integer.parseInt(color.substring(0, 2), 16) / 255f;
						float ng = Integer.parseInt(color.substring(2, 4), 16) / 255f;
						float nb = Integer.parseInt(color.substring(4, 6), 16) / 255f;
						float na = Integer.parseInt(color.substring(6, 8), 16) / 255f;
						color = nextMap.getString("dark");
						float nr2 = Integer.parseInt(color.substring(0, 2), 16) / 255f;
						float ng2 = Integer.parseInt(color.substring(2, 4), 16) / 255f;
						float nb2 = Integer.parseInt(color.substring(4, 6), 16) / 255f;
						JsonValue curve = keyMap.get("curve");
						if (curve != null) {
							bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, r, nr, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, g, ng, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, b, nb, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, a, na, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, r2, nr2, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, g2, ng2, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 6, time, time2, b2, nb2, 1);
						}
						time = time2;
						r = nr;
						g = ng;
						b = nb;
						a = na;
						r2 = nr2;
						g2 = ng2;
						b2 = nb2;
						keyMap = nextMap;
					}
					timelines.add(timeline);
				}
				case "rgb2" -> {
					var timeline = new RGB2Timeline(frames, frames * 6, slot.index);
					float time = keyMap.getFloat("time", 0);
					String color = keyMap.getString("light");
					float r = Integer.parseInt(color.substring(0, 2), 16) / 255f;
					float g = Integer.parseInt(color.substring(2, 4), 16) / 255f;
					float b = Integer.parseInt(color.substring(4, 6), 16) / 255f;
					color = keyMap.getString("dark");
					float r2 = Integer.parseInt(color.substring(0, 2), 16) / 255f;
					float g2 = Integer.parseInt(color.substring(2, 4), 16) / 255f;
					float b2 = Integer.parseInt(color.substring(4, 6), 16) / 255f;
					for (int frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, r, g, b, r2, g2, b2);
						JsonValue nextMap = keyMap.next;
						if (nextMap == null) {
							timeline.shrink(bezier);
							break;
						}
						float time2 = nextMap.getFloat("time", 0);
						color = nextMap.getString("light");
						float nr = Integer.parseInt(color.substring(0, 2), 16) / 255f;
						float ng = Integer.parseInt(color.substring(2, 4), 16) / 255f;
						float nb = Integer.parseInt(color.substring(4, 6), 16) / 255f;
						color = nextMap.getString("dark");
						float nr2 = Integer.parseInt(color.substring(0, 2), 16) / 255f;
						float ng2 = Integer.parseInt(color.substring(2, 4), 16) / 255f;
						float nb2 = Integer.parseInt(color.substring(4, 6), 16) / 255f;
						JsonValue curve = keyMap.get("curve");
						if (curve != null) {
							bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, r, nr, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, g, ng, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 2, time, time2, b, nb, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 3, time, time2, r2, nr2, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 4, time, time2, g2, ng2, 1);
							bezier = readCurve(curve, timeline, bezier, frame, 5, time, time2, b2, nb2, 1);
						}
						time = time2;
						r = nr;
						g = ng;
						b = nb;
						r2 = nr2;
						g2 = ng2;
						b2 = nb2;
						keyMap = nextMap;
					}
					timelines.add(timeline);
				}
				}
			}
		}

		// Bone timelines.
		JsonValue boneMap = map.getChild("bones");
		var bones = new IntArray(boneMap == null ? 0 : boneMap.size);
		for (; boneMap != null; boneMap = boneMap.next) {
			BoneData bone = skeletonData.findBone(boneMap.name);
			if (bone == null) throw new SerializationException("Bone not found: " + boneMap.name);
			bones.add(bone.index);
			for (JsonValue timelineMap = boneMap.child; timelineMap != null; timelineMap = timelineMap.next) {
				JsonValue keyMap = timelineMap.child;
				if (keyMap == null) continue;

				int frames = timelineMap.size;
				switch (timelineMap.name) {
				case "rotate" -> readTimeline(timelines, keyMap, new RotateTimeline(frames, frames, bone.index), 0, 1);
				case "translate" -> //
					readTimeline(timelines, keyMap, new TranslateTimeline(frames, frames << 1, bone.index), "x", "y", 0, scale);
				case "translatex" -> readTimeline(timelines, keyMap, new TranslateXTimeline(frames, frames, bone.index), 0, scale);
				case "translatey" -> readTimeline(timelines, keyMap, new TranslateYTimeline(frames, frames, bone.index), 0, scale);
				case "scale" -> readTimeline(timelines, keyMap, new ScaleTimeline(frames, frames << 1, bone.index), "x", "y", 1, 1);
				case "scalex" -> readTimeline(timelines, keyMap, new ScaleXTimeline(frames, frames, bone.index), 1, 1);
				case "scaley" -> readTimeline(timelines, keyMap, new ScaleYTimeline(frames, frames, bone.index), 1, 1);
				case "shear" -> readTimeline(timelines, keyMap, new ShearTimeline(frames, frames << 1, bone.index), "x", "y", 0, 1);
				case "shearx" -> readTimeline(timelines, keyMap, new ShearXTimeline(frames, frames, bone.index), 0, 1);
				case "sheary" -> readTimeline(timelines, keyMap, new ShearYTimeline(frames, frames, bone.index), 0, 1);
				case "inherit" -> {
					var timeline = new InheritTimeline(frames, bone.index);
					for (int frame = 0; keyMap != null; keyMap = keyMap.next, frame++) {
						timeline.setFrame(frame, keyMap.getFloat("time", 0),
							Inherit.valueOf(keyMap.getString("inherit", Inherit.normal.name())));
					}
					timelines.add(timeline);
				}
				}
			}
		}

		// IK constraint timelines.
		for (JsonValue timelineMap = map.getChild("ik"); timelineMap != null; timelineMap = timelineMap.next) {
			JsonValue keyMap = timelineMap.child;
			if (keyMap == null) continue;
			IkConstraintData constraint = skeletonData.findConstraint(timelineMap.name, IkConstraintData.class);
			if (constraint == null) throw new SerializationException("IK constraint not found: " + timelineMap.name);
			var timeline = new IkConstraintTimeline(timelineMap.size, timelineMap.size << 1,
				skeletonData.constraints.indexOf(constraint, true));
			float time = keyMap.getFloat("time", 0);
			float mix = keyMap.getFloat("mix", 1), softness = keyMap.getFloat("softness", 0) * scale;
			for (int frame = 0, bezier = 0;; frame++) {
				timeline.setFrame(frame, time, mix, softness, keyMap.getBoolean("bendPositive", true) ? 1 : -1,
					keyMap.getBoolean("compress", false), keyMap.getBoolean("stretch", false));
				JsonValue nextMap = keyMap.next;
				if (nextMap == null) {
					timeline.shrink(bezier);
					break;
				}
				float time2 = nextMap.getFloat("time", 0);
				float mix2 = nextMap.getFloat("mix", 1), softness2 = nextMap.getFloat("softness", 0) * scale;
				JsonValue curve = keyMap.get("curve");
				if (curve != null) {
					bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, mix, mix2, 1);
					bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, softness, softness2, scale);
				}
				time = time2;
				mix = mix2;
				softness = softness2;
				keyMap = nextMap;
			}
			timelines.add(timeline);
		}

		// Transform constraint timelines.
		for (JsonValue timelineMap = map.getChild("transform"); timelineMap != null; timelineMap = timelineMap.next) {
			JsonValue keyMap = timelineMap.child;
			if (keyMap == null) continue;
			TransformConstraintData constraint = skeletonData.findConstraint(timelineMap.name, TransformConstraintData.class);
			if (constraint == null) throw new SerializationException("Transform constraint not found: " + timelineMap.name);
			var timeline = new TransformConstraintTimeline(timelineMap.size, timelineMap.size * 6,
				skeletonData.constraints.indexOf(constraint, true));
			float time = keyMap.getFloat("time", 0);
			float mixRotate = keyMap.getFloat("mixRotate", 1);
			float mixX = keyMap.getFloat("mixX", 1), mixY = keyMap.getFloat("mixY", mixX);
			float mixScaleX = keyMap.getFloat("mixScaleX", 1), mixScaleY = keyMap.getFloat("mixScaleY", 1);
			float mixShearY = keyMap.getFloat("mixShearY", 1);
			for (int frame = 0, bezier = 0;; frame++) {
				timeline.setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
				JsonValue nextMap = keyMap.next;
				if (nextMap == null) {
					timeline.shrink(bezier);
					break;
				}
				float time2 = nextMap.getFloat("time", 0);
				float mixRotate2 = nextMap.getFloat("mixRotate", 1);
				float mixX2 = nextMap.getFloat("mixX", 1), mixY2 = nextMap.getFloat("mixY", mixX2);
				float mixScaleX2 = nextMap.getFloat("mixScaleX", 1), mixScaleY2 = nextMap.getFloat("mixScaleY", 1);
				float mixShearY2 = nextMap.getFloat("mixShearY", 1);
				JsonValue curve = keyMap.get("curve");
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
				mixShearY = mixShearY2;
				keyMap = nextMap;
			}
			timelines.add(timeline);
		}

		// Path constraint timelines.
		for (JsonValue constraintMap = map.getChild("path"); constraintMap != null; constraintMap = constraintMap.next) {
			PathConstraintData constraint = skeletonData.findConstraint(constraintMap.name, PathConstraintData.class);
			if (constraint == null) throw new SerializationException("Path constraint not found: " + constraintMap.name);
			int index = skeletonData.constraints.indexOf(constraint, true);
			for (JsonValue timelineMap = constraintMap.child; timelineMap != null; timelineMap = timelineMap.next) {
				JsonValue keyMap = timelineMap.child;
				if (keyMap == null) continue;

				int frames = timelineMap.size;
				switch (timelineMap.name) {
				case "position" -> {
					var timeline = new PathConstraintPositionTimeline(frames, frames, index);
					readTimeline(timelines, keyMap, timeline, 0, constraint.positionMode == PositionMode.fixed ? scale : 1);
				}
				case "spacing" -> {
					var timeline = new PathConstraintSpacingTimeline(frames, frames, index);
					readTimeline(timelines, keyMap, timeline, 0,
						constraint.spacingMode == SpacingMode.length || constraint.spacingMode == SpacingMode.fixed ? scale : 1);
				}
				case "mix" -> {
					var timeline = new PathConstraintMixTimeline(frames, frames * 3, index);
					float time = keyMap.getFloat("time", 0);
					float mixRotate = keyMap.getFloat("mixRotate", 1);
					float mixX = keyMap.getFloat("mixX", 1), mixY = keyMap.getFloat("mixY", mixX);
					for (int frame = 0, bezier = 0;; frame++) {
						timeline.setFrame(frame, time, mixRotate, mixX, mixY);
						JsonValue nextMap = keyMap.next;
						if (nextMap == null) {
							timeline.shrink(bezier);
							break;
						}
						float time2 = nextMap.getFloat("time", 0);
						float mixRotate2 = nextMap.getFloat("mixRotate", 1);
						float mixX2 = nextMap.getFloat("mixX", 1), mixY2 = nextMap.getFloat("mixY", mixX2);
						JsonValue curve = keyMap.get("curve");
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
					}
					timelines.add(timeline);
				}
				}
			}
		}

		// Physics constraint timelines.
		for (JsonValue constraintMap = map.getChild("physics"); constraintMap != null; constraintMap = constraintMap.next) {
			int index = -1;
			if (!constraintMap.name.isEmpty()) {
				PhysicsConstraintData constraint = skeletonData.findConstraint(constraintMap.name, PhysicsConstraintData.class);
				if (constraint == null) throw new SerializationException("Physics constraint not found: " + constraintMap.name);
				index = skeletonData.constraints.indexOf(constraint, true);
			}
			for (JsonValue timelineMap = constraintMap.child; timelineMap != null; timelineMap = timelineMap.next) {
				JsonValue keyMap = timelineMap.child;
				if (keyMap == null) continue;

				int frames = timelineMap.size;
				CurveTimeline1 timeline;
				float defaultValue = 0;
				switch (timelineMap.name) {
				case "reset" -> {
					var resetTimeline = new PhysicsConstraintResetTimeline(frames, index);
					for (int frame = 0; keyMap != null; keyMap = keyMap.next, frame++)
						resetTimeline.setFrame(frame, keyMap.getFloat("time", 0));
					timelines.add(resetTimeline);
					continue;
				}
				case "inertia" -> timeline = new PhysicsConstraintInertiaTimeline(frames, frames, index);
				case "strength" -> timeline = new PhysicsConstraintStrengthTimeline(frames, frames, index);
				case "damping" -> timeline = new PhysicsConstraintDampingTimeline(frames, frames, index);
				case "mass" -> timeline = new PhysicsConstraintMassTimeline(frames, frames, index);
				case "wind" -> timeline = new PhysicsConstraintWindTimeline(frames, frames, index);
				case "gravity" -> timeline = new PhysicsConstraintGravityTimeline(frames, frames, index);
				case "mix" -> {
					defaultValue = 1;
					timeline = new PhysicsConstraintMixTimeline(frames, frames, index);
				}
				default -> {
					continue;
				}
				}
				readTimeline(timelines, keyMap, timeline, defaultValue, 1);
			}
		}

		// Slider timelines.
		for (JsonValue constraintMap = map.getChild("slider"); constraintMap != null; constraintMap = constraintMap.next) {
			SliderData constraint = skeletonData.findConstraint(constraintMap.name, SliderData.class);
			if (constraint == null) throw new SerializationException("Slider not found: " + constraintMap.name);
			int index = skeletonData.constraints.indexOf(constraint, true);
			for (JsonValue timelineMap = constraintMap.child; timelineMap != null; timelineMap = timelineMap.next) {
				JsonValue keyMap = timelineMap.child;
				if (keyMap == null) continue;

				int frames = timelineMap.size;
				switch (timelineMap.name) {
				case "time" -> readTimeline(timelines, keyMap, new SliderTimeline(frames, frames, index), 1, 1);
				case "mix" -> readTimeline(timelines, keyMap, new SliderMixTimeline(frames, frames, index), 1, 1);
				}
			}
		}

		// Attachment timelines.
		for (JsonValue attachmentsMap = map.getChild("attachments"); attachmentsMap != null; attachmentsMap = attachmentsMap.next) {
			Skin skin = skeletonData.findSkin(attachmentsMap.name);
			if (skin == null) throw new SerializationException("Skin not found: " + attachmentsMap.name);
			for (JsonValue slotMap = attachmentsMap.child; slotMap != null; slotMap = slotMap.next) {
				SlotData slot = skeletonData.findSlot(slotMap.name);
				if (slot == null) throw new SerializationException("Attachment slot not found: " + slotMap.name);
				for (JsonValue attachmentMap = slotMap.child; attachmentMap != null; attachmentMap = attachmentMap.next) {
					Attachment attachment = skin.getAttachment(slot.index, attachmentMap.name);
					if (attachment == null) throw new SerializationException("Timeline attachment not found: " + attachmentMap.name);
					for (JsonValue timelineMap = attachmentMap.child; timelineMap != null; timelineMap = timelineMap.next) {
						JsonValue keyMap = timelineMap.child;
						int frames = timelineMap.size;
						switch (timelineMap.name) {
						case "deform" -> {
							var vertexAttachment = (VertexAttachment)attachment;
							boolean weighted = vertexAttachment.getBones() != null;
							float[] vertices = vertexAttachment.getVertices();
							int deformLength = weighted ? (vertices.length / 3) << 1 : vertices.length;

							var timeline = new DeformTimeline(frames, frames, slot.index, vertexAttachment);
							float time = keyMap.getFloat("time", 0);
							for (int frame = 0, bezier = 0;; frame++) {
								float[] deform;
								JsonValue verticesValue = keyMap.get("vertices");
								if (verticesValue == null)
									deform = weighted ? new float[deformLength] : vertices;
								else {
									deform = new float[deformLength];
									int start = keyMap.getInt("offset", 0);
									arraycopy(verticesValue.asFloatArray(), 0, deform, start, verticesValue.size);
									if (scale != 1) {
										for (int i = start, n = i + verticesValue.size; i < n; i++)
											deform[i] *= scale;
									}
									if (!weighted) {
										for (int i = 0; i < deformLength; i++)
											deform[i] += vertices[i];
									}
								}

								timeline.setFrame(frame, time, deform);
								JsonValue nextMap = keyMap.next;
								if (nextMap == null) {
									timeline.shrink(bezier);
									break;
								}
								float time2 = nextMap.getFloat("time", 0);
								JsonValue curve = keyMap.get("curve");
								if (curve != null) bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, 0, 1, 1);
								time = time2;
								keyMap = nextMap;
							}
							timelines.add(timeline);
						}
						case "sequence" -> {
							var timeline = new SequenceTimeline(frames, slot.index, attachment);
							float lastDelay = 0;
							for (int frame = 0; keyMap != null; keyMap = keyMap.next, frame++) {
								float delay = keyMap.getFloat("delay", lastDelay);
								timeline.setFrame(frame, keyMap.getFloat("time", 0),
									SequenceMode.valueOf(keyMap.getString("mode", "hold")), keyMap.getInt("index", 0), delay);
								lastDelay = delay;
							}
							timelines.add(timeline);
						}
						}
					}
				}
			}
		}

		// Draw order timeline.
		JsonValue drawOrderMap = map.get("drawOrder");
		if (drawOrderMap != null) {
			var timeline = new DrawOrderTimeline(drawOrderMap.size);
			int slotCount = skeletonData.slots.size, frame = 0;
			for (JsonValue keyMap = drawOrderMap.child; keyMap != null; keyMap = keyMap.next, frame++)
				timeline.setFrame(frame, keyMap.getFloat("time", 0), readDrawOrder(skeletonData, keyMap, slotCount, null));
			timelines.add(timeline);
		}

		// Draw order folder timelines.
		JsonValue drawOrderFoldersMap = map.get("drawOrderFolder");
		if (drawOrderFoldersMap != null) {
			for (JsonValue timelineMap = drawOrderFoldersMap.child; timelineMap != null; timelineMap = timelineMap.next) {
				JsonValue slotEntry = timelineMap.get("slots");
				var folderSlots = new int[slotEntry.size];
				int ii = 0;
				for (slotEntry = slotEntry.child; slotEntry != null; slotEntry = slotEntry.next, ii++) {
					SlotData slot = skeletonData.findSlot(slotEntry.asString());
					if (slot == null) throw new SerializationException("Draw order folder slot not found: " + slotEntry.asString());
					folderSlots[ii] = slot.index;
				}

				JsonValue keyMap = timelineMap.get("keys");
				var timeline = new DrawOrderFolderTimeline(keyMap.size, folderSlots, skeletonData.slots.size);
				int frame = 0;
				for (keyMap = keyMap.child; keyMap != null; keyMap = keyMap.next, frame++)
					timeline.setFrame(frame, keyMap.getFloat("time", 0),
						readDrawOrder(skeletonData, keyMap, folderSlots.length, folderSlots));
				timelines.add(timeline);
			}
		}

		// Event timeline.
		JsonValue eventsMap = map.get("events");
		if (eventsMap != null) {
			var timeline = new EventTimeline(eventsMap.size);
			int frame = 0;
			for (JsonValue keyMap = eventsMap.child; keyMap != null; keyMap = keyMap.next, frame++) {
				EventData data = skeletonData.findEvent(keyMap.getString("name"));
				if (data == null) throw new SerializationException("Event not found: " + keyMap.getString("name"));
				Event setup = data.setupPose;
				var event = new Event(keyMap.getFloat("time", 0), data);
				event.intValue = keyMap.getInt("int", setup.intValue);
				event.floatValue = keyMap.getFloat("float", setup.floatValue);
				event.stringValue = keyMap.getString("string", setup.stringValue);
				if (event.data.audioPath != null) {
					event.volume = keyMap.getFloat("volume", setup.volume);
					event.balance = keyMap.getFloat("balance", setup.balance);
				}
				timeline.setFrame(frame, event);
			}
			timelines.add(timeline);
		}

		timelines.shrink();
		float duration = 0;
		Timeline[] items = timelines.items;
		for (int i = 0, n = timelines.size; i < n; i++)
			duration = Math.max(duration, items[i].getDuration());

		Animation animation = new Animation(name);
		animation.setTimelines(timelines, bones);
		animation.setDuration(duration);

		String color = map.getString("color", null);
		if (color != null) Color.valueOf(color, animation.color);

		skeletonData.animations.add(animation);
	}

	private void readTimeline (Array<Timeline> timelines, JsonValue keyMap, CurveTimeline1 timeline, float defaultValue,
		float scale) {
		float time = keyMap.getFloat("time", 0), value = keyMap.getFloat("value", defaultValue) * scale;
		for (int frame = 0, bezier = 0;; frame++) {
			timeline.setFrame(frame, time, value);
			JsonValue nextMap = keyMap.next;
			if (nextMap == null) {
				timeline.shrink(bezier);
				timelines.add(timeline);
				return;
			}
			float time2 = nextMap.getFloat("time", 0);
			float value2 = nextMap.getFloat("value", defaultValue) * scale;
			JsonValue curve = keyMap.get("curve");
			if (curve != null) bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value, value2, scale);
			time = time2;
			value = value2;
			keyMap = nextMap;
		}
	}

	private void readTimeline (Array<Timeline> timelines, JsonValue keyMap, BoneTimeline2 timeline, String name1, String name2,
		float defaultValue, float scale) {
		float time = keyMap.getFloat("time", 0);
		float value1 = keyMap.getFloat(name1, defaultValue) * scale, value2 = keyMap.getFloat(name2, defaultValue) * scale;
		for (int frame = 0, bezier = 0;; frame++) {
			timeline.setFrame(frame, time, value1, value2);
			JsonValue nextMap = keyMap.next;
			if (nextMap == null) {
				timeline.shrink(bezier);
				timelines.add(timeline);
				return;
			}
			float time2 = nextMap.getFloat("time", 0);
			float nvalue1 = nextMap.getFloat(name1, defaultValue) * scale, nvalue2 = nextMap.getFloat(name2, defaultValue) * scale;
			JsonValue curve = keyMap.get("curve");
			if (curve != null) {
				bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value1, nvalue1, scale);
				bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, value2, nvalue2, scale);
			}
			time = time2;
			value1 = nvalue1;
			value2 = nvalue2;
			keyMap = nextMap;
		}
	}

	/** @param folderSlots Slot names are resolved to positions within this array. If null, slot indices are used as positions. */
	private @Null int[] readDrawOrder (SkeletonData skeletonData, JsonValue keyMap, int slotCount, @Null int[] folderSlots) {
		JsonValue changes = keyMap.get("offsets");
		if (changes == null) return null; // Setup draw order.
		var drawOrder = new int[slotCount];
		Arrays.fill(drawOrder, -1);
		var unchanged = new int[slotCount - changes.size];
		int originalIndex = 0, unchangedIndex = 0;
		for (JsonValue offsetMap = changes.child; offsetMap != null; offsetMap = offsetMap.next) {
			SlotData slot = skeletonData.findSlot(offsetMap.getString("slot"));
			if (slot == null) throw new SerializationException("Draw order slot not found: " + offsetMap.getString("slot"));
			int index;
			if (folderSlots == null)
				index = slot.index;
			else {
				index = -1;
				for (int i = 0; i < slotCount; i++) {
					if (folderSlots[i] == slot.index) {
						index = i;
						break;
					}
				}
				if (index == -1) throw new SerializationException("Slot not in folder: " + offsetMap.getString("slot"));
			}
			// Collect unchanged items.
			while (originalIndex != index)
				unchanged[unchangedIndex++] = originalIndex++;
			// Set changed items.
			drawOrder[originalIndex + offsetMap.getInt("offset")] = originalIndex++;
		}
		// Collect remaining unchanged items.
		while (originalIndex < slotCount)
			unchanged[unchangedIndex++] = originalIndex++;
		// Fill in unchanged items.
		for (int i = slotCount - 1; i >= 0; i--)
			if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
		return drawOrder;
	}

	int readCurve (JsonValue curve, CurveTimeline timeline, int bezier, int frame, int value, float time1, float time2,
		float value1, float value2, float scale) {
		if (curve.isString()) {
			if (curve.asString().equals("stepped")) timeline.setStepped(frame);
			return bezier;
		}
		curve = curve.get(value << 2);
		float cx1 = curve.asFloat();
		curve = curve.next;
		float cy1 = curve.asFloat() * scale;
		curve = curve.next;
		float cx2 = curve.asFloat();
		curve = curve.next;
		float cy2 = curve.asFloat() * scale;
		setBezier(timeline, frame, value, bezier, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
		return bezier + 1;
	}

	static void setBezier (CurveTimeline timeline, int frame, int value, int bezier, float time1, float value1, float cx1,
		float cy1, float cx2, float cy2, float time2, float value2) {
		timeline.setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
	}

	static class LinkedMesh {
		String source, skin;
		int slotIndex, sourceIndex;
		MeshAttachment mesh;
		boolean inheritTimelines;

		public LinkedMesh (MeshAttachment mesh, String skin, int slotIndex, int sourceIndex, String source,
			boolean inheritTimelines) {
			this.mesh = mesh;
			this.skin = skin;
			this.slotIndex = slotIndex;
			this.sourceIndex = sourceIndex;
			this.source = source;
			this.inheritTimelines = inheritTimelines;
		}
	}
}
