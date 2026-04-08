/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

#if (UNITY_5 || UNITY_5_3_OR_NEWER || UNITY_WSA || UNITY_WP8 || UNITY_WP8_1)
#define IS_UNITY
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

#if WINDOWS_STOREAPP
using System.Threading.Tasks;
using Windows.Storage;
#endif

namespace Spine {
#if IS_UNITY
	using Color32F = UnityEngine.Color;
#endif

	using FromProperty = TransformConstraintData.FromProperty;
	using FromRotate = TransformConstraintData.FromRotate;
	using FromScaleX = TransformConstraintData.FromScaleX;
	using FromScaleY = TransformConstraintData.FromScaleY;
	using FromShearY = TransformConstraintData.FromShearY;
	using FromX = TransformConstraintData.FromX;
	using FromY = TransformConstraintData.FromY;
	using ToProperty = TransformConstraintData.ToProperty;
	using ToRotate = TransformConstraintData.ToRotate;
	using ToScaleX = TransformConstraintData.ToScaleX;
	using ToScaleY = TransformConstraintData.ToScaleY;
	using ToShearY = TransformConstraintData.ToShearY;
	using ToX = TransformConstraintData.ToX;
	using ToY = TransformConstraintData.ToY;

	/// <summary>
	/// Loads skeleton data in the Spine JSON format.
	/// <para>
	/// JSON is human readable but the binary format is much smaller on disk and faster to load. See <see cref="SkeletonBinary"/>.</para>
	/// <para>
	/// See <a href="http://esotericsoftware.com/spine-json-format">Spine JSON format</a> and
	/// <a href = "http://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data" > JSON and binary data</a> in the Spine
	/// Runtimes Guide.</para>
	/// </summary>
	public class SkeletonJson : SkeletonLoader {
		private readonly List<LinkedMesh> linkedMeshes = new List<LinkedMesh>();

		public SkeletonJson (AttachmentLoader attachmentLoader)
			: base(attachmentLoader) {
		}

		public SkeletonJson (params Atlas[] atlasArray)
			: base(atlasArray) {
		}

#if !IS_UNITY && WINDOWS_STOREAPP
		private async Task<SkeletonData> ReadFile(string path) {
			var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
			var file = await folder.GetFileAsync(path).AsTask().ConfigureAwait(false);
			using (StreamReader reader = new StreamReader(await file.OpenStreamForReadAsync().ConfigureAwait(false))) {
				SkeletonData skeletonData = ReadSkeletonData(reader);
				skeletonData.Name = Path.GetFileNameWithoutExtension(path);
				return skeletonData;
			}
		}

		public override SkeletonData ReadSkeletonData (string path) {
			return this.ReadFile(path).Result;
		}
#else
		public override SkeletonData ReadSkeletonData (string path) {
#if WINDOWS_PHONE
			using (StreamReader reader = new StreamReader(Microsoft.Xna.Framework.TitleContainer.OpenStream(path))) {
#else
			using (StreamReader reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))) {
#endif
				SkeletonData skeletonData = ReadSkeletonData(reader);
				skeletonData.name = Path.GetFileNameWithoutExtension(path);
				return skeletonData;
			}
		}
#endif

		public SkeletonData ReadSkeletonData (TextReader reader) {
			if (reader == null) throw new ArgumentNullException("reader", "reader cannot be null.");

			var skeletonData = new SkeletonData();
			try {
				float scale = this.scale;
				Dictionary<string, object> root = Json.Deserialize(reader) as Dictionary<string, Object>;
				if (root == null) throw new Exception("Invalid JSON.");

				// Skeleton.
				if (root.ContainsKey("skeleton")) {
					Dictionary<string, object> skeletonMap = (Dictionary<string, Object>)root["skeleton"];
					skeletonData.hash = (string)skeletonMap["hash"];
					skeletonData.version = (string)skeletonMap["spine"];
					skeletonData.x = GetFloat(skeletonMap, "x", 0);
					skeletonData.y = GetFloat(skeletonMap, "y", 0);
					skeletonData.width = GetFloat(skeletonMap, "width", 0);
					skeletonData.height = GetFloat(skeletonMap, "height", 0);
					skeletonData.referenceScale = GetFloat(skeletonMap, "referenceScale", 100) * scale;
					skeletonData.fps = GetFloat(skeletonMap, "fps", 30);
					skeletonData.imagesPath = GetString(skeletonMap, "images", null);
					skeletonData.audioPath = GetString(skeletonMap, "audio", null);
				}

				// Bones.
				if (root.ContainsKey("bones")) {
					foreach (Dictionary<string, Object> boneMap in (List<Object>)root["bones"]) {
						BoneData parent = null;
						if (boneMap.ContainsKey("parent")) {
							parent = skeletonData.FindBone((string)boneMap["parent"]);
							if (parent == null)
								throw new Exception("Parent bone not found: " + boneMap["parent"]);
						}
						var data = new BoneData(skeletonData.Bones.Count, (string)boneMap["name"], parent);
						data.length = GetFloat(boneMap, "length", 0) * scale;
						BonePose setup = data.setupPose;
						setup.x = GetFloat(boneMap, "x", 0) * scale;
						setup.y = GetFloat(boneMap, "y", 0) * scale;
						setup.rotation = GetFloat(boneMap, "rotation", 0);
						setup.scaleX = GetFloat(boneMap, "scaleX", 1);
						setup.scaleY = GetFloat(boneMap, "scaleY", 1);
						setup.shearX = GetFloat(boneMap, "shearX", 0);
						setup.shearY = GetFloat(boneMap, "shearY", 0);
						string inheritString = GetString(boneMap, "inherit", Inherit.Normal.ToString());
						setup.inherit = (Inherit)Enum.Parse(typeof(Inherit), inheritString, true);
						data.skinRequired = GetBoolean(boneMap, "skin", false);

						skeletonData.bones.Add(data);
					}
				}

				// Slots.
				if (root.ContainsKey("slots")) {
					foreach (Dictionary<string, Object> slotMap in (List<Object>)root["slots"]) {
						string slotName = (string)slotMap["name"];
						string boneName = (string)slotMap["bone"];
						BoneData boneData = skeletonData.FindBone(boneName);
						if (boneData == null) throw new Exception("Slot bone not found: " + boneName);
						var data = new SlotData(skeletonData.Slots.Count, slotName, boneData);

						if (slotMap.ContainsKey("color")) {
							string color = (string)slotMap["color"];
							data.setupPose.SetColor(ToColor32(color, 8));
						}

						if (slotMap.ContainsKey("dark")) {
							string color2 = (string)slotMap["dark"];
							data.setupPose.SetDarkColor(ToColor24(color2, 6)); // expectedLength = 6. ie. "RRGGBB"
						}

						data.attachmentName = GetString(slotMap, "attachment", null);
						if (slotMap.ContainsKey("blend"))
							data.blendMode = (BlendMode)Enum.Parse(typeof(BlendMode), (string)slotMap["blend"], true);
						else
							data.blendMode = BlendMode.Normal;
						//data.visible = slotMap.getBoolean("visible", true);
						skeletonData.slots.Add(data);
					}
				}

				// Constraints.
				if (root.ContainsKey("constraints")) {
					foreach (Dictionary<string, Object> constraintMap in (List<Object>)root["constraints"]) {
						string name = (string)constraintMap["name"];
						bool skinRequired = GetBoolean(constraintMap, "skin", false);

						switch ((string)constraintMap["type"]) {
						case "ik": {
							var data = new IkConstraintData(name);
							data.skinRequired = skinRequired;

							if (constraintMap.ContainsKey("bones")) {
								foreach (string boneName in (List<Object>)constraintMap["bones"]) {
									BoneData bone = skeletonData.FindBone(boneName);
									if (bone == null) throw new Exception("IK bone not found: " + boneName);
									data.bones.Add(bone);
								}
							}

							string targetName = (string)constraintMap["target"];
							data.target = skeletonData.FindBone(targetName);
							if (data.target == null) throw new Exception("IK target bone not found: " + targetName);

							data.uniform = GetBoolean(constraintMap, "uniform", false);
							IkConstraintPose setup = data.setupPose;
							setup.mix = GetFloat(constraintMap, "mix", 1);
							setup.softness = GetFloat(constraintMap, "softness", 0) * scale;
							setup.bendDirection = GetBoolean(constraintMap, "bendPositive", true) ? 1 : -1;
							setup.compress = GetBoolean(constraintMap, "compress", false);
							setup.stretch = GetBoolean(constraintMap, "stretch", false);

							skeletonData.constraints.Add(data);
							break;
						}
						case "transform": {
							var data = new TransformConstraintData(name);
							data.skinRequired = skinRequired;

							if (constraintMap.ContainsKey("bones")) {
								foreach (string boneName in (List<Object>)constraintMap["bones"]) {
									BoneData bone = skeletonData.FindBone(boneName);
									if (bone == null) throw new Exception("Transform constraint bone not found: " + boneName);
									data.bones.Add(bone);
								}
							}

							string sourceName = (string)constraintMap["source"];
							data.source = skeletonData.FindBone(sourceName);
							if (data.source == null) throw new Exception("Transform constraint source bone not found: " + sourceName);

							data.localSource = GetBoolean(constraintMap, "localSource", false);
							data.localTarget = GetBoolean(constraintMap, "localTarget", false);
							data.additive = GetBoolean(constraintMap, "additive", false);
							data.clamp = GetBoolean(constraintMap, "clamp", false);

							bool rotate = false, x = false, y = false, scaleX = false, scaleY = false, shearY = false;
							if (constraintMap.ContainsKey("properties")) {
								foreach (KeyValuePair<string, Object> fromEntryObject in (Dictionary<string, Object>)constraintMap["properties"]) {
									var fromEntry = (Dictionary<string, Object>)fromEntryObject.Value;
									string fromEntryName = fromEntryObject.Key;

									FromProperty from = FromProperty(fromEntryName);
									float fromScale = PropertyScale(fromEntryName, scale);
									from.offset = GetFloat(fromEntry, "offset", 0) * fromScale;
									if (fromEntry.ContainsKey("to")) {
										foreach (KeyValuePair<string, Object> toEntryObject in (Dictionary<string, Object>)fromEntry["to"]) {
											var toEntry = (Dictionary<string, Object>)toEntryObject.Value;
											string toEntryName = toEntryObject.Key;

											float toScale = 1;
											ToProperty to;
											switch (toEntryName) {
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
											default: throw new Exception("Invalid transform constraint to property: " + toEntryName);
											}
											to.offset = GetFloat(toEntry, "offset", 0) * toScale;
											to.max = GetFloat(toEntry, "max", 1) * toScale;
											to.scale = GetFloat(toEntry, "scale", 1) * (toScale / fromScale);
											from.to.Add(to);
										}
									}
									if (from.to.Count != 0) data.properties.Add(from);
								}
							}

							data.offsets[TransformConstraintData.ROTATION] = GetFloat(constraintMap, "rotation", 0);
							data.offsets[TransformConstraintData.X] = GetFloat(constraintMap, "x", 0) * scale;
							data.offsets[TransformConstraintData.Y] = GetFloat(constraintMap, "y", 0) * scale;
							data.offsets[TransformConstraintData.SCALEX] = GetFloat(constraintMap, "scaleX", 0);
							data.offsets[TransformConstraintData.SCALEY] = GetFloat(constraintMap, "scaleY", 0);
							data.offsets[TransformConstraintData.SHEARY] = GetFloat(constraintMap, "shearY", 0);

							TransformConstraintPose setup = data.setupPose;
							if (rotate) setup.mixRotate = GetFloat(constraintMap, "mixRotate", 1);
							if (x) setup.mixX = GetFloat(constraintMap, "mixX", 1);
							if (y) setup.mixY = GetFloat(constraintMap, "mixY", setup.mixX);
							if (scaleX) setup.mixScaleX = GetFloat(constraintMap, "mixScaleX", 1);
							if (scaleY) setup.mixScaleY = GetFloat(constraintMap, "mixScaleY", setup.mixScaleX);
							if (shearY) setup.mixShearY = GetFloat(constraintMap, "mixShearY", 1);

							skeletonData.constraints.Add(data);
							break;
						}
						case "path": {
							var data = new PathConstraintData(name);
							data.skinRequired = skinRequired;

							if (constraintMap.ContainsKey("bones")) {
								foreach (string boneName in (List<Object>)constraintMap["bones"]) {
									BoneData bone = skeletonData.FindBone(boneName);
									if (bone == null) throw new Exception("Path bone not found: " + boneName);
									data.bones.Add(bone);
								}
							}

							string slotName = (string)constraintMap["slot"];
							data.slot = skeletonData.FindSlot(slotName);
							if (data.slot == null) throw new Exception("Path slot not found: " + slotName);

							data.positionMode = (PositionMode)Enum.Parse(typeof(PositionMode), GetString(constraintMap, "positionMode", "percent"), true);
							data.spacingMode = (SpacingMode)Enum.Parse(typeof(SpacingMode), GetString(constraintMap, "spacingMode", "length"), true);
							data.rotateMode = (RotateMode)Enum.Parse(typeof(RotateMode), GetString(constraintMap, "rotateMode", "tangent"), true);
							data.offsetRotation = GetFloat(constraintMap, "rotation", 0);
							PathConstraintPose setup = data.setupPose;
							setup.position = GetFloat(constraintMap, "position", 0);
							if (data.positionMode == PositionMode.Fixed) setup.position *= scale;
							setup.spacing = GetFloat(constraintMap, "spacing", 0);
							if (data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed) setup.spacing *= scale;
							setup.mixRotate = GetFloat(constraintMap, "mixRotate", 1);
							setup.mixX = GetFloat(constraintMap, "mixX", 1);
							setup.mixY = GetFloat(constraintMap, "mixY", setup.mixX);

							skeletonData.constraints.Add(data);
							break;
						}
						case "physics": {
							var data = new PhysicsConstraintData(name);
							data.skinRequired = skinRequired;

							string boneName = (string)constraintMap["bone"];
							data.bone = skeletonData.FindBone(boneName);
							if (data.bone == null) throw new Exception("Physics bone not found: " + boneName);

							data.x = GetFloat(constraintMap, "x", 0);
							data.y = GetFloat(constraintMap, "y", 0);
							data.rotate = GetFloat(constraintMap, "rotate", 0);
							data.scaleX = GetFloat(constraintMap, "scaleX", 0);
							data.shearX = GetFloat(constraintMap, "shearX", 0);
							data.limit = GetFloat(constraintMap, "limit", 5000) * scale;
							data.step = 1f / GetInt(constraintMap, "fps", 60);
							PhysicsConstraintPose setup = data.setupPose;
							setup.inertia = GetFloat(constraintMap, "inertia", 0.5f);
							setup.strength = GetFloat(constraintMap, "strength", 100);
							setup.damping = GetFloat(constraintMap, "damping", 0.85f);
							setup.massInverse = 1f / GetFloat(constraintMap, "mass", 1);
							setup.wind = GetFloat(constraintMap, "wind", 0);
							setup.gravity = GetFloat(constraintMap, "gravity", 0);
							setup.mix = GetFloat(constraintMap, "mix", 1);
							data.inertiaGlobal = GetBoolean(constraintMap, "inertiaGlobal", false);
							data.strengthGlobal = GetBoolean(constraintMap, "strengthGlobal", false);
							data.dampingGlobal = GetBoolean(constraintMap, "dampingGlobal", false);
							data.massGlobal = GetBoolean(constraintMap, "massGlobal", false);
							data.windGlobal = GetBoolean(constraintMap, "windGlobal", false);
							data.gravityGlobal = GetBoolean(constraintMap, "gravityGlobal", false);
							data.mixGlobal = GetBoolean(constraintMap, "mixGlobal", false);

							skeletonData.constraints.Add(data);
							break;
						}
						case "slider": {
							var data = new SliderData(name);
							data.skinRequired = skinRequired;

							data.additive = GetBoolean(constraintMap, "additive", false);
							data.loop = GetBoolean(constraintMap, "loop", false);
							data.setupPose.time = GetFloat(constraintMap, "time", 0);
							data.setupPose.mix = GetFloat(constraintMap, "mix", 1);

							string boneName = GetString(constraintMap, "bone", null);
							if (boneName != null) {
								data.bone = skeletonData.FindBone(boneName);
								if (data.bone == null) throw new Exception("Slider bone not found: " + boneName);
								string property = (string)constraintMap["property"];
								data.property = FromProperty(property);
								float propertyScale = PropertyScale(property, scale);
								data.property.offset = GetFloat(constraintMap, "from", 0) * propertyScale;
								data.offset = GetFloat(constraintMap, "to", 0);
								data.scale = GetFloat(constraintMap, "scale", 1) / propertyScale;
								data.local = GetBoolean(constraintMap, "local", false);
							}

							skeletonData.constraints.Add(data);
							break;
						}
						}
					}
				}

				// Skins.
				if (root.ContainsKey("skins")) {
					foreach (Dictionary<string, object> skinMap in (List<object>)root["skins"]) {
						var skin = new Skin((string)skinMap["name"]);
						if (skinMap.ContainsKey("bones")) {
							foreach (string entryName in (List<Object>)skinMap["bones"]) {
								BoneData bone = skeletonData.FindBone(entryName);
								if (bone == null) throw new Exception("Skin bone not found: " + entryName);
								skin.bones.Add(bone);
							}
						}
						skin.bones.TrimExcess();
						if (skinMap.ContainsKey("ik")) {
							foreach (string entryName in (List<Object>)skinMap["ik"]) {
								IkConstraintData constraint = skeletonData.FindConstraint<IkConstraintData>(entryName);
								if (constraint == null) throw new Exception("Skin IK constraint not found: " + entryName);
								skin.constraints.Add(constraint);
							}
						}
						if (skinMap.ContainsKey("transform")) {
							foreach (string entryName in (List<Object>)skinMap["transform"]) {
								TransformConstraintData constraint = skeletonData.FindConstraint<TransformConstraintData>(entryName);
								if (constraint == null) throw new Exception("Skin transform constraint not found: " + entryName);
								skin.constraints.Add(constraint);
							}
						}
						if (skinMap.ContainsKey("path")) {
							foreach (string entryName in (List<Object>)skinMap["path"]) {
								PathConstraintData constraint = skeletonData.FindConstraint<PathConstraintData>(entryName);
								if (constraint == null) throw new Exception("Skin path constraint not found: " + entryName);
								skin.constraints.Add(constraint);
							}
						}
						if (skinMap.ContainsKey("physics")) {
							foreach (string entryName in (List<Object>)skinMap["physics"]) {
								PhysicsConstraintData constraint = skeletonData.FindConstraint<PhysicsConstraintData>(entryName);
								if (constraint == null) throw new Exception("Skin physics constraint not found: " + entryName);
								skin.constraints.Add(constraint);
							}
						}
						if (skinMap.ContainsKey("slider")) {
							foreach (string entryName in (List<Object>)skinMap["slider"]) {
								SliderData constraint = skeletonData.FindConstraint<SliderData>(entryName);
								if (constraint == null) throw new Exception("Skin slider not found: " + entryName);
								skin.constraints.Add(constraint);
							}
						}

						skin.constraints.TrimExcess();
						if (skinMap.ContainsKey("attachments")) {
							foreach (KeyValuePair<string, Object> slotEntry in (Dictionary<string, Object>)skinMap["attachments"]) {
								SlotData slot = skeletonData.FindSlot(slotEntry.Key);
								if (slot == null) throw new Exception("Skin slot not found: " + slotEntry.Key);
								int slotIndex = slot.Index;
								foreach (KeyValuePair<string, Object> entry in ((Dictionary<string, Object>)slotEntry.Value)) {
									try {
										Attachment attachment = ReadAttachment((Dictionary<string, Object>)entry.Value, skin, slotIndex, entry.Key, skeletonData);
										if (attachment != null) skin.SetAttachment(slotIndex, entry.Key, attachment);
									} catch (Exception e) {
										throw new Exception("Error reading attachment: " + entry.Key + ", skin: " + skin, e);
									}
								}
							}
						}
						skeletonData.skins.Add(skin);
						if (skin.name == "default") skeletonData.defaultSkin = skin;
					}
				}

				// Linked meshes.
				for (int i = 0, n = linkedMeshes.Count; i < n; i++) {
					LinkedMesh linkedMesh = linkedMeshes[i];
					Skin skin = linkedMesh.skin == null ? skeletonData.defaultSkin : skeletonData.FindSkin(linkedMesh.skin);
					if (skin == null) throw new Exception("Slot not found: " + linkedMesh.skin);
					Attachment parent = skin.GetAttachment(linkedMesh.slotIndex, linkedMesh.parent);
					if (parent == null) throw new Exception("Parent mesh not found: " + linkedMesh.parent);
					linkedMesh.mesh.TimelineAttachment = linkedMesh.inheritTimelines ? (VertexAttachment)parent : linkedMesh.mesh;
					linkedMesh.mesh.ParentMesh = (MeshAttachment)parent;
					linkedMesh.mesh.UpdateSequence();
				}
				linkedMeshes.Clear();

				// Events.
				if (root.ContainsKey("events")) {
					foreach (KeyValuePair<string, Object> entry in (Dictionary<string, Object>)root["events"]) {
						Dictionary<string, object> entryMap = (Dictionary<string, Object>)entry.Value;
						var data = new EventData(entry.Key);
						Event setup = data.setupPose;
						setup.intValue = GetInt(entryMap, "int", 0);
						setup.floatValue = GetFloat(entryMap, "float", 0);
						setup.stringValue = GetString(entryMap, "string", string.Empty);
						data.AudioPath = GetString(entryMap, "audio", null);
						if (data.AudioPath != null) {
							setup.volume = GetFloat(entryMap, "volume", 1);
							setup.balance = GetFloat(entryMap, "balance", 0);
						}
						skeletonData.events.Add(data);
					}
				}

				// Animations.
				if (root.ContainsKey("animations")) {
					foreach (KeyValuePair<string, Object> entry in (Dictionary<string, Object>)root["animations"]) {
						try {
							ReadAnimation((Dictionary<string, Object>)entry.Value, entry.Key, skeletonData);
						} catch (Exception e) {
							throw new Exception("Error reading animation: " + entry.Key + "\n" + e.Message, e);
						}
					}
				}

				// Slider animations.
				if (root.ContainsKey("constraints")) {
					foreach (Dictionary<string, Object> constraintMap in (List<Object>)root["constraints"]) {
						if (GetString(constraintMap, "type", string.Empty) == "slider") {
							SliderData data = skeletonData.FindConstraint<SliderData>((string)constraintMap["name"]);
							string animationName = (string)constraintMap["animation"];
							data.animation = skeletonData.FindAnimation(animationName);
							if (data.animation == null) throw new Exception("Slider animation not found: " + animationName);
						}
					}
				}

				skeletonData.bones.TrimExcess();
				skeletonData.slots.TrimExcess();
				skeletonData.skins.TrimExcess();
				skeletonData.events.TrimExcess();
				skeletonData.animations.TrimExcess();
				skeletonData.constraints.TrimExcess();
				return skeletonData;
			} catch (Exception ex) {
				if (skeletonData.version != null)
					throw new SerializationException("Error reading JSON skeleton data, version: " + skeletonData.version, ex);
				throw new SerializationException("Error JSON skeleton data.", ex);
			}
		}

		private FromProperty FromProperty (String type) {
			switch (type) {
			case "rotate": return new FromRotate();
			case "x": return new FromX();
			case "y": return new FromY();
			case "scaleX": return new FromScaleX();
			case "scaleY": return new FromScaleY();
			case "shearY": return new FromShearY();
			default: throw new Exception("Invalid from property: " + type);
			}
		}

		private float PropertyScale (String type, float scale) {
			switch (type) {
			case "x":
			case "y":
				return scale;
			default: return 1;
			}
		}

		private Attachment ReadAttachment (Dictionary<string, Object> map, Skin skin, int slotIndex, string name, SkeletonData skeletonData) {
			float scale = this.scale;
			name = GetString(map, "name", name);

			string typeName = GetString(map, "type", "region");
			AttachmentType type = (AttachmentType)Enum.Parse(typeof(AttachmentType), typeName, true);

			switch (type) {
			case AttachmentType.Region: {
				string path = GetString(map, "path", name);
				object sequenceJson;
				map.TryGetValue("sequence", out sequenceJson);
				Sequence sequence = ReadSequence(sequenceJson);
				RegionAttachment region = attachmentLoader.NewRegionAttachment(skin, name, path, sequence);
				if (region == null) return null;
				region.Path = path;
				region.x = GetFloat(map, "x", 0) * scale;
				region.y = GetFloat(map, "y", 0) * scale;
				region.scaleX = GetFloat(map, "scaleX", 1);
				region.scaleY = GetFloat(map, "scaleY", 1);
				region.rotation = GetFloat(map, "rotation", 0);
				region.width = GetFloat(map, "width") * scale;
				region.height = GetFloat(map, "height") * scale;

				if (map.ContainsKey("color")) {
					string color = (string)map["color"];
					region.SetColor(ToColor32(color, 8));
				}

				region.UpdateSequence();
				return region;
			}
			case AttachmentType.Boundingbox:
				BoundingBoxAttachment box = attachmentLoader.NewBoundingBoxAttachment(skin, name);
				if (box == null) return null;
				ReadVertices(map, box, GetInt(map, "vertexCount", 0) << 1);
				return box;
			case AttachmentType.Mesh:
			case AttachmentType.Linkedmesh: {
				string path = GetString(map, "path", name);
				object sequenceJson;
				map.TryGetValue("sequence", out sequenceJson);
				Sequence sequence = ReadSequence(sequenceJson);
				MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, name, path, sequence);
				if (mesh == null) return null;
				mesh.Path = path;

				if (map.ContainsKey("color")) {
					string color = (string)map["color"];
					mesh.SetColor(ToColor32(color, 0));
				}

				mesh.Width = GetFloat(map, "width", 0) * scale;
				mesh.Height = GetFloat(map, "height", 0) * scale;

				string parent = GetString(map, "parent", null);
				if (parent != null) {
					linkedMeshes.Add(new LinkedMesh(mesh, GetString(map, "skin", null), slotIndex, parent, GetBoolean(map, "timelines", true)));
					return mesh;
				}

				float[] uvs = GetFloatArray(map, "uvs", 1);
				ReadVertices(map, mesh, uvs.Length);
				mesh.triangles = GetIntArray(map, "triangles");
				mesh.regionUVs = uvs;

				if (map.ContainsKey("hull")) mesh.HullLength = GetInt(map, "hull", 0) << 1;
				if (map.ContainsKey("edges")) mesh.Edges = GetIntArray(map, "edges");
				mesh.UpdateSequence();
				return mesh;
			}
			case AttachmentType.Path: {
				PathAttachment pathAttachment = attachmentLoader.NewPathAttachment(skin, name);
				if (pathAttachment == null) return null;
				pathAttachment.closed = GetBoolean(map, "closed", false);
				pathAttachment.constantSpeed = GetBoolean(map, "constantSpeed", true);

				int vertexCount = GetInt(map, "vertexCount", 0);
				ReadVertices(map, pathAttachment, vertexCount << 1);

				// potential BOZO see Java impl
				pathAttachment.lengths = GetFloatArray(map, "lengths", scale);
				return pathAttachment;
			}
			case AttachmentType.Point: {
				PointAttachment point = attachmentLoader.NewPointAttachment(skin, name);
				if (point == null) return null;
				point.x = GetFloat(map, "x", 0) * scale;
				point.y = GetFloat(map, "y", 0) * scale;
				point.rotation = GetFloat(map, "rotation", 0);

				//string color = GetString(map, "color", null);
				//if (color != null) point.color = color;
				return point;
			}
			case AttachmentType.Clipping: {
				ClippingAttachment clip = attachmentLoader.NewClippingAttachment(skin, name);
				if (clip == null) return null;

				string end = GetString(map, "end", null);
				if (end != null) {
					SlotData slot = skeletonData.FindSlot(end);
					if (slot == null) throw new Exception("Clipping end slot not found: " + end);
					clip.EndSlot = slot;
				}

				ReadVertices(map, clip, GetInt(map, "vertexCount", 0) << 1);

				//string color = GetString(map, "color", null);
				// if (color != null) clip.color = color;
				return clip;
			}
			}
			return null;
		}

		public static Sequence ReadSequence (object sequenceJson) {
			Dictionary<string, object> map = sequenceJson as Dictionary<string, Object>;
			if (map == null) return new Sequence(1, false);
			var sequence = new Sequence(GetInt(map, "count"), true);
			sequence.start = GetInt(map, "start", 1);
			sequence.digits = GetInt(map, "digits", 0);
			sequence.setupIndex = GetInt(map, "setup", 0);
			return sequence;
		}

		private void ReadVertices (Dictionary<string, Object> map, VertexAttachment attachment, int verticesLength) {
			attachment.WorldVerticesLength = verticesLength;
			float[] vertices = GetFloatArray(map, "vertices", 1);
			float scale = Scale;
			if (verticesLength == vertices.Length) {
				if (scale != 1) {
					for (int i = 0, n = vertices.Length; i < n; i++) {
						vertices[i] *= scale;
					}
				}
				attachment.vertices = vertices;
				return;
			}
			var weights = new ExposedList<float>(verticesLength * 3 * 3);
			var bones = new ExposedList<int>(verticesLength * 3);
			for (int i = 0, n = vertices.Length; i < n;) {
				int boneCount = (int)vertices[i++];
				bones.Add(boneCount);
				for (int nn = i + (boneCount << 2); i < nn; i += 4) {
					bones.Add((int)vertices[i]);
					weights.Add(vertices[i + 1] * scale);
					weights.Add(vertices[i + 2] * scale);
					weights.Add(vertices[i + 3]);
				}
			}
			attachment.bones = bones.ToArray();
			attachment.vertices = weights.ToArray();
		}

		private void ReadAnimation (Dictionary<string, Object> map, string name, SkeletonData skeletonData) {
			float scale = this.scale;
			var timelines = new ExposedList<Timeline>();

			// Slot timelines.
			if (map.ContainsKey("slots")) {
				foreach (KeyValuePair<string, Object> entry in (Dictionary<string, Object>)map["slots"]) {
					string slotName = entry.Key;
					SlotData slot = skeletonData.FindSlot(slotName);
					if (slot == null) throw new SerializationException("Slot not found: " + slotName);
					int slotIndex = slot.Index;
					Dictionary<string, object> timelineMap = (Dictionary<string, Object>)entry.Value;
					foreach (KeyValuePair<string, Object> timelineEntry in timelineMap) {
						List<object> values = (List<Object>)timelineEntry.Value;
						int frames = values.Count;
						if (frames == 0) continue;
						string timelineName = (string)timelineEntry.Key;
						switch (timelineName) {
						case "attachment": {
							var timeline = new AttachmentTimeline(frames, slot.Index);
							int frame = 0;
							foreach (Dictionary<string, Object> keyMap in values) {
								timeline.SetFrame(frame++, GetFloat(keyMap, "time", 0), GetString(keyMap, "name", null));
							}
							timelines.Add(timeline);
							break;
						}
						case "rgba": {
							var timeline = new RGBATimeline(frames, frames << 2, slotIndex);

							List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
							keyMapEnumerator.MoveNext();
							Dictionary<string, object> keyMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
							float time = GetFloat(keyMap, "time", 0);
							string color = (string)keyMap["color"];
							float r = ToColor(color, 0);
							float g = ToColor(color, 1);
							float b = ToColor(color, 2);
							float a = ToColor(color, 3);
							for (int frame = 0, bezier = 0; ; frame++) {
								timeline.SetFrame(frame, time, r, g, b, a);
								if (!keyMapEnumerator.MoveNext()) {
									timeline.Shrink(bezier);
									break;
								}
								Dictionary<string, object> nextMap = (Dictionary<string, Object>)keyMapEnumerator.Current;

								float time2 = GetFloat(nextMap, "time", 0);
								color = (string)nextMap["color"];
								float nr = ToColor(color, 0);
								float ng = ToColor(color, 1);
								float nb = ToColor(color, 2);
								float na = ToColor(color, 3);

								if (keyMap.ContainsKey("curve")) {
									object curve = keyMap["curve"];
									bezier = ReadCurve(curve, timeline, bezier, frame, 0, time, time2, r, nr, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 1, time, time2, g, ng, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 2, time, time2, b, nb, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 3, time, time2, a, na, 1);
								}
								time = time2;
								r = nr;
								g = ng;
								b = nb;
								a = na;
								keyMap = nextMap;
							}
							timelines.Add(timeline);
							break;
						}
						case "rgb": {
							var timeline = new RGBTimeline(frames, frames * 3, slotIndex);

							List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
							keyMapEnumerator.MoveNext();
							Dictionary<string, object> keyMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
							float time = GetFloat(keyMap, "time", 0);
							string color = (string)keyMap["color"];
							float r = ToColor(color, 0, 6);
							float g = ToColor(color, 1, 6);
							float b = ToColor(color, 2, 6);
							for (int frame = 0, bezier = 0; ; frame++) {
								timeline.SetFrame(frame, time, r, g, b);
								if (!keyMapEnumerator.MoveNext()) {
									timeline.Shrink(bezier);
									break;
								}
								Dictionary<string, object> nextMap = (Dictionary<string, Object>)keyMapEnumerator.Current;

								float time2 = GetFloat(nextMap, "time", 0);
								color = (string)nextMap["color"];
								float nr = ToColor(color, 0, 6);
								float ng = ToColor(color, 1, 6);
								float nb = ToColor(color, 2, 6);

								if (keyMap.ContainsKey("curve")) {
									object curve = keyMap["curve"];
									bezier = ReadCurve(curve, timeline, bezier, frame, 0, time, time2, r, nr, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 1, time, time2, g, ng, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 2, time, time2, b, nb, 1);
								}
								time = time2;
								r = nr;
								g = ng;
								b = nb;
								keyMap = nextMap;
							}
							timelines.Add(timeline);
							break;
						}
						case "alpha": {
							List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
							keyMapEnumerator.MoveNext();
							ReadTimeline(ref timelines, ref keyMapEnumerator, new AlphaTimeline(frames, frames, slotIndex), 0, 1);
							break;
						}
						case "rgba2": {
							var timeline = new RGBA2Timeline(frames, frames * 7, slotIndex);

							List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
							keyMapEnumerator.MoveNext();
							Dictionary<string, object> keyMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
							float time = GetFloat(keyMap, "time", 0);
							string color = (string)keyMap["light"];
							float r = ToColor(color, 0);
							float g = ToColor(color, 1);
							float b = ToColor(color, 2);
							float a = ToColor(color, 3);
							color = (string)keyMap["dark"];
							float r2 = ToColor(color, 0, 6);
							float g2 = ToColor(color, 1, 6);
							float b2 = ToColor(color, 2, 6);
							for (int frame = 0, bezier = 0; ; frame++) {
								timeline.SetFrame(frame, time, r, g, b, a, r2, g2, b2);
								if (!keyMapEnumerator.MoveNext()) {
									timeline.Shrink(bezier);
									break;
								}
								Dictionary<string, object> nextMap = (Dictionary<string, Object>)keyMapEnumerator.Current;

								float time2 = GetFloat(nextMap, "time", 0);
								color = (string)nextMap["light"];
								float nr = ToColor(color, 0);
								float ng = ToColor(color, 1);
								float nb = ToColor(color, 2);
								float na = ToColor(color, 3);
								color = (string)nextMap["dark"];
								float nr2 = ToColor(color, 0, 6);
								float ng2 = ToColor(color, 1, 6);
								float nb2 = ToColor(color, 2, 6);

								if (keyMap.ContainsKey("curve")) {
									object curve = keyMap["curve"];
									bezier = ReadCurve(curve, timeline, bezier, frame, 0, time, time2, r, nr, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 1, time, time2, g, ng, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 2, time, time2, b, nb, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 3, time, time2, a, na, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 4, time, time2, r2, nr2, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 5, time, time2, g2, ng2, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 6, time, time2, b2, nb2, 1);
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
							timelines.Add(timeline);
							break;
						}
						case "rgb2": {
							var timeline = new RGB2Timeline(frames, frames * 6, slotIndex);

							List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
							keyMapEnumerator.MoveNext();
							Dictionary<string, object> keyMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
							float time = GetFloat(keyMap, "time", 0);
							string color = (string)keyMap["light"];
							float r = ToColor(color, 0, 6);
							float g = ToColor(color, 1, 6);
							float b = ToColor(color, 2, 6);
							color = (string)keyMap["dark"];
							float r2 = ToColor(color, 0, 6);
							float g2 = ToColor(color, 1, 6);
							float b2 = ToColor(color, 2, 6);
							for (int frame = 0, bezier = 0; ; frame++) {
								timeline.SetFrame(frame, time, r, g, b, r2, g2, b2);
								if (!keyMapEnumerator.MoveNext()) {
									timeline.Shrink(bezier);
									break;
								}
								Dictionary<string, object> nextMap = (Dictionary<string, Object>)keyMapEnumerator.Current;

								float time2 = GetFloat(nextMap, "time", 0);
								color = (string)nextMap["light"];
								float nr = ToColor(color, 0, 6);
								float ng = ToColor(color, 1, 6);
								float nb = ToColor(color, 2, 6);
								color = (string)nextMap["dark"];
								float nr2 = ToColor(color, 0, 6);
								float ng2 = ToColor(color, 1, 6);
								float nb2 = ToColor(color, 2, 6);

								if (keyMap.ContainsKey("curve")) {
									object curve = keyMap["curve"];
									bezier = ReadCurve(curve, timeline, bezier, frame, 0, time, time2, r, nr, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 1, time, time2, g, ng, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 2, time, time2, b, nb, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 3, time, time2, r2, nr2, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 4, time, time2, g2, ng2, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 5, time, time2, b2, nb2, 1);
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
							timelines.Add(timeline);
							break;
						}
						}
					}
				}
			}

			// Bone timelines.
			ExposedList<int> bones = null;
			if (!map.ContainsKey("bones")) {
				bones = new ExposedList<int>(0);
			} else {
				int bonesCount = ((Dictionary<string, Object>)map["bones"]).Count;
				bones = new ExposedList<int>(bonesCount);
				foreach (KeyValuePair<string, Object> entry in (Dictionary<string, Object>)map["bones"]) {
					string boneName = entry.Key;
					BoneData bone = skeletonData.FindBone(boneName);
					if (bone == null) throw new Exception("Bone not found: " + boneName);
					bones.Add(bone.index);
					Dictionary<string, object> timelineMap = (Dictionary<string, Object>)entry.Value;
					foreach (KeyValuePair<string, Object> timelineEntry in timelineMap) {
						List<object> values = (List<Object>)timelineEntry.Value;
						List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
						if (!keyMapEnumerator.MoveNext()) continue;
						int frames = values.Count;
						string timelineName = (string)timelineEntry.Key;
						switch (timelineName) {
						case "rotate":
							ReadTimeline(ref timelines, ref keyMapEnumerator, new RotateTimeline(frames, frames, bone.index), 0, 1);
							break;
						case "translate": {
							ReadTimeline(ref timelines, ref keyMapEnumerator, new TranslateTimeline(frames, frames << 1, bone.index), "x", "y", 0, scale);
							break;
						}
						case "translatex": {
							ReadTimeline(ref timelines, ref keyMapEnumerator, new TranslateXTimeline(frames, frames, bone.index), 0, scale);
							break;
						}
						case "translatey": {
							ReadTimeline(ref timelines, ref keyMapEnumerator, new TranslateYTimeline(frames, frames, bone.index), 0, scale);
							break;
						}
						case "scale": {
							ReadTimeline(ref timelines, ref keyMapEnumerator, new ScaleTimeline(frames, frames << 1, bone.index), "x", "y", 1, 1);
							break;
						}
						case "scalex":
							ReadTimeline(ref timelines, ref keyMapEnumerator, new ScaleXTimeline(frames, frames, bone.index), 1, 1);
							break;
						case "scaley":
							ReadTimeline(ref timelines, ref keyMapEnumerator, new ScaleYTimeline(frames, frames, bone.index), 1, 1);
							break;
						case "shear": {
							ReadTimeline(ref timelines, ref keyMapEnumerator, new ShearTimeline(frames, frames << 1, bone.index), "x", "y", 0, 1);
							break;
						}
						case "shearx":
							ReadTimeline(ref timelines, ref keyMapEnumerator, new ShearXTimeline(frames, frames, bone.index), 0, 1);
							break;
						case "sheary":
							ReadTimeline(ref timelines, ref keyMapEnumerator, new ShearYTimeline(frames, frames, bone.index), 0, 1);
							break;
						case "inherit": {
							var timeline = new InheritTimeline(frames, bone.index);
							for (int frame = 0; ; frame++) {
								Dictionary<string, object> keyMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
								float time = GetFloat(keyMap, "time", 0);
								Inherit inherit = (Inherit)Enum.Parse(typeof(Inherit), GetString(keyMap, "inherit", Inherit.Normal.ToString()), true);
								timeline.SetFrame(frame, time, inherit);
								if (!keyMapEnumerator.MoveNext()) {
									break;
								}
							}
							timelines.Add(timeline);
							break;
						}
						}
					}
				}
			}

			// IK constraint timelines.
			if (map.ContainsKey("ik")) {
				foreach (KeyValuePair<string, Object> timelineMap in (Dictionary<string, Object>)map["ik"]) {
					List<object> values = (List<Object>)timelineMap.Value;
					List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
					if (!keyMapEnumerator.MoveNext()) continue;
					Dictionary<string, object> keyMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
					IkConstraintData constraint = skeletonData.FindConstraint<IkConstraintData>(timelineMap.Key);
					if (constraint == null) throw new SerializationException("IK constraint not found: " + timelineMap.Key);
					var timeline = new IkConstraintTimeline(values.Count, values.Count << 1,
						skeletonData.constraints.IndexOf(constraint));
					float time = GetFloat(keyMap, "time", 0);
					float mix = GetFloat(keyMap, "mix", 1), softness = GetFloat(keyMap, "softness", 0) * scale;
					for (int frame = 0, bezier = 0; ; frame++) {
						timeline.SetFrame(frame, time, mix, softness, GetBoolean(keyMap, "bendPositive", true) ? 1 : -1,
							GetBoolean(keyMap, "compress", false), GetBoolean(keyMap, "stretch", false));
						if (!keyMapEnumerator.MoveNext()) {
							timeline.Shrink(bezier);
							break;
						}
						Dictionary<string, object> nextMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
						float time2 = GetFloat(nextMap, "time", 0);
						float mix2 = GetFloat(nextMap, "mix", 1), softness2 = GetFloat(nextMap, "softness", 0) * scale;
						if (keyMap.ContainsKey("curve")) {
							object curve = keyMap["curve"];
							bezier = ReadCurve(curve, timeline, bezier, frame, 0, time, time2, mix, mix2, 1);
							bezier = ReadCurve(curve, timeline, bezier, frame, 1, time, time2, softness, softness2, scale);
						}
						time = time2;
						mix = mix2;
						softness = softness2;
						keyMap = nextMap;
					}
					timelines.Add(timeline);
				}
			}

			// Transform constraint timelines.
			if (map.ContainsKey("transform")) {
				foreach (KeyValuePair<string, Object> timelineMap in (Dictionary<string, Object>)map["transform"]) {
					List<object> values = (List<Object>)timelineMap.Value;
					List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
					if (!keyMapEnumerator.MoveNext()) continue;
					Dictionary<string, object> keyMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
					TransformConstraintData constraint = skeletonData.FindConstraint<TransformConstraintData>(timelineMap.Key);
					if (constraint == null) throw new SerializationException("Transform constraint not found: " + timelineMap.Key);
					var timeline = new TransformConstraintTimeline(values.Count, values.Count * 6,
						skeletonData.constraints.IndexOf(constraint));
					float time = GetFloat(keyMap, "time", 0);
					float mixRotate = GetFloat(keyMap, "mixRotate", 1);
					float mixX = GetFloat(keyMap, "mixX", 1), mixY = GetFloat(keyMap, "mixY", mixX);
					float mixScaleX = GetFloat(keyMap, "mixScaleX", 1), mixScaleY = GetFloat(keyMap, "mixScaleY", 1);
					float mixShearY = GetFloat(keyMap, "mixShearY", 1);
					for (int frame = 0, bezier = 0; ; frame++) {
						timeline.SetFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
						if (!keyMapEnumerator.MoveNext()) {
							timeline.Shrink(bezier);
							break;
						}
						Dictionary<string, object> nextMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
						float time2 = GetFloat(nextMap, "time", 0);
						float mixRotate2 = GetFloat(nextMap, "mixRotate", 1);
						float mixX2 = GetFloat(nextMap, "mixX", 1), mixY2 = GetFloat(nextMap, "mixY", mixX2);
						float mixScaleX2 = GetFloat(nextMap, "mixScaleX", 1), mixScaleY2 = GetFloat(nextMap, "mixScaleY", 1);
						float mixShearY2 = GetFloat(nextMap, "mixShearY", 1);
						if (keyMap.ContainsKey("curve")) {
							object curve = keyMap["curve"];
							bezier = ReadCurve(curve, timeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
							bezier = ReadCurve(curve, timeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
							bezier = ReadCurve(curve, timeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
							bezier = ReadCurve(curve, timeline, bezier, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
							bezier = ReadCurve(curve, timeline, bezier, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
							bezier = ReadCurve(curve, timeline, bezier, frame, 5, time, time2, mixShearY, mixShearY2, 1);
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
					timelines.Add(timeline);
				}
			}

			// Path constraint timelines.
			if (map.ContainsKey("path")) {
				foreach (KeyValuePair<string, Object> constraintMap in (Dictionary<string, Object>)map["path"]) {
					PathConstraintData constraint = skeletonData.FindConstraint<PathConstraintData>(constraintMap.Key);
					if (constraint == null) throw new Exception("Path constraint not found: " + constraintMap.Key);
					int constraintIndex = skeletonData.constraints.IndexOf(constraint);
					Dictionary<string, object> timelineMap = (Dictionary<string, Object>)constraintMap.Value;
					foreach (KeyValuePair<string, Object> timelineEntry in timelineMap) {
						List<object> values = (List<Object>)timelineEntry.Value;
						List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
						if (!keyMapEnumerator.MoveNext()) continue;

						int frames = values.Count;
						string timelineName = (string)timelineEntry.Key;
						switch (timelineName) {
						case "position": {
							CurveTimeline1 timeline = new PathConstraintPositionTimeline(frames, frames, constraintIndex);
							ReadTimeline(ref timelines, ref keyMapEnumerator, timeline, 0, constraint.positionMode == PositionMode.Fixed ? scale : 1);
							break;
						}
						case "spacing": {
							CurveTimeline1 timeline = new PathConstraintSpacingTimeline(frames, frames, constraintIndex);
							ReadTimeline(ref timelines, ref keyMapEnumerator, timeline, 0,
								constraint.spacingMode == SpacingMode.Length || constraint.spacingMode == SpacingMode.Fixed ? scale : 1);
							break;
						}
						case "mix": {
							var timeline = new PathConstraintMixTimeline(frames, frames * 3, constraintIndex);
							Dictionary<string, object> keyMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
							float time = GetFloat(keyMap, "time", 0);
							float mixRotate = GetFloat(keyMap, "mixRotate", 1);
							float mixX = GetFloat(keyMap, "mixX", 1), mixY = GetFloat(keyMap, "mixY", mixX);
							for (int frame = 0, bezier = 0; ; frame++) {
								timeline.SetFrame(frame, time, mixRotate, mixX, mixY);
								if (!keyMapEnumerator.MoveNext()) {
									timeline.Shrink(bezier);
									break;
								}
								Dictionary<string, object> nextMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
								float time2 = GetFloat(nextMap, "time", 0);
								float mixRotate2 = GetFloat(nextMap, "mixRotate", 1);
								float mixX2 = GetFloat(nextMap, "mixX", 1), mixY2 = GetFloat(nextMap, "mixY", mixX2);
								if (keyMap.ContainsKey("curve")) {
									object curve = keyMap["curve"];
									bezier = ReadCurve(curve, timeline, bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 1, time, time2, mixX, mixX2, 1);
									bezier = ReadCurve(curve, timeline, bezier, frame, 2, time, time2, mixY, mixY2, 1);
								}
								time = time2;
								mixRotate = mixRotate2;
								mixX = mixX2;
								mixY = mixY2;
								keyMap = nextMap;
							}
							timelines.Add(timeline);
							break;
						}
						}
					}
				}
			}

			// Physics constraint timelines.
			if (map.ContainsKey("physics")) {
				foreach (KeyValuePair<string, Object> constraintMap in (Dictionary<string, Object>)map["physics"]) {
					int index = -1;
					if (!string.IsNullOrEmpty(constraintMap.Key)) {
						PhysicsConstraintData constraint = skeletonData.FindConstraint<PhysicsConstraintData>(constraintMap.Key);
						if (constraint == null) throw new Exception("Physics constraint not found: " + constraintMap.Key);
						index = skeletonData.constraints.IndexOf(constraint);
					}
					Dictionary<string, object> timelineMap = (Dictionary<string, Object>)constraintMap.Value;
					foreach (KeyValuePair<string, Object> timelineEntry in timelineMap) {
						List<object> values = (List<Object>)timelineEntry.Value;
						List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
						if (!keyMapEnumerator.MoveNext()) continue;

						int frames = values.Count;
						string timelineName = (string)timelineEntry.Key;
						CurveTimeline1 timeline;
						float defaultValue = 0;
						switch (timelineName) {
						case "reset": {
							var resetTimeline = new PhysicsConstraintResetTimeline(frames, index);
							int frame = 0;
							foreach (Dictionary<string, Object> keyMap in values) {
								resetTimeline.SetFrame(frame++, GetFloat(keyMap, "time", 0));
							}
							timelines.Add(resetTimeline);
							continue;
						}
						case "inertia":
							timeline = new PhysicsConstraintInertiaTimeline(frames, frames, index);
							break;
						case "strength":
							timeline = new PhysicsConstraintStrengthTimeline(frames, frames, index);
							break;
						case "damping":
							timeline = new PhysicsConstraintDampingTimeline(frames, frames, index);
							break;
						case "mass":
							timeline = new PhysicsConstraintMassTimeline(frames, frames, index);
							break;
						case "wind":
							timeline = new PhysicsConstraintWindTimeline(frames, frames, index);
							break;
						case "gravity":
							timeline = new PhysicsConstraintGravityTimeline(frames, frames, index);
							break;
						case "mix":
							defaultValue = 1;
							timeline = new PhysicsConstraintMixTimeline(frames, frames, index);
							break;
						default:
							continue;
						}
						ReadTimeline(ref timelines, ref keyMapEnumerator, timeline, defaultValue, 1);
					}
				}
			}

			// Slider timelines.
			if (map.ContainsKey("slider")) {
				foreach (KeyValuePair<string, Object> constraintMap in (Dictionary<string, Object>)map["slider"]) {
					SliderData constraint = skeletonData.FindConstraint<SliderData>(constraintMap.Key);
					if (constraint == null) throw new Exception("Slider not found: " + constraintMap.Key);
					int index = skeletonData.constraints.IndexOf(constraint);

					Dictionary<string, object> timelineMap = (Dictionary<string, Object>)constraintMap.Value;
					foreach (KeyValuePair<string, Object> timelineEntry in timelineMap) {
						List<object> values = (List<Object>)timelineEntry.Value;
						List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
						if (!keyMapEnumerator.MoveNext()) continue;

						int frames = values.Count;
						string timelineName = (string)timelineEntry.Key;
						switch (timelineName) {
						case "time":
							ReadTimeline(ref timelines, ref keyMapEnumerator, new SliderTimeline(frames, frames, index), 1, 1);
							break;
						case "mix":
							ReadTimeline(ref timelines, ref keyMapEnumerator, new SliderMixTimeline(frames, frames, index), 1, 1);
							break;
						}
					}
				}
			}

			// Attachment timelines.
			if (map.ContainsKey("attachments")) {
				foreach (KeyValuePair<string, Object> attachmentsMap in (Dictionary<string, Object>)map["attachments"]) {
					Skin skin = skeletonData.FindSkin(attachmentsMap.Key);
					foreach (KeyValuePair<string, Object> slotMap in (Dictionary<string, Object>)attachmentsMap.Value) {
						SlotData slot = skeletonData.FindSlot(slotMap.Key);
						if (slot == null) throw new Exception("Attachment slot not found: " + slotMap.Key);
						foreach (KeyValuePair<string, Object> attachmentMap in (Dictionary<string, Object>)slotMap.Value) {
							Attachment attachment = skin.GetAttachment(slot.index, attachmentMap.Key);
							if (attachment == null) throw new Exception("Timeline attachment not found: " + attachmentMap.Key);
							foreach (KeyValuePair<string, Object> timelineMap in (Dictionary<string, Object>)attachmentMap.Value) {
								List<object> values = (List<Object>)timelineMap.Value;
								List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
								if (!keyMapEnumerator.MoveNext()) continue;
								Dictionary<string, object> keyMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
								int frames = values.Count;
								string timelineName = timelineMap.Key;
								switch (timelineName) {
								case "deform": {
									var vertexAttachment = (VertexAttachment)attachment;
									bool weighted = vertexAttachment.bones != null;
									float[] vertices = vertexAttachment.vertices;
									int deformLength = weighted ? (vertices.Length / 3) << 1 : vertices.Length;

									var timeline = new DeformTimeline(frames, frames, slot.Index, vertexAttachment);
									float time = GetFloat(keyMap, "time", 0);
									for (int frame = 0, bezier = 0; ; frame++) {
										float[] deform;
										if (!keyMap.ContainsKey("vertices")) {
											deform = weighted ? new float[deformLength] : vertices;
										} else {
											deform = new float[deformLength];
											int start = GetInt(keyMap, "offset", 0);
											float[] verticesValue = GetFloatArray(keyMap, "vertices", 1);
											Array.Copy(verticesValue, 0, deform, start, verticesValue.Length);
											if (scale != 1) {
												for (int i = start, n = i + verticesValue.Length; i < n; i++)
													deform[i] *= scale;
											}

											if (!weighted) {
												for (int i = 0; i < deformLength; i++)
													deform[i] += vertices[i];
											}
										}

										timeline.SetFrame(frame, time, deform);
										if (!keyMapEnumerator.MoveNext()) {
											timeline.Shrink(bezier);
											break;
										}
										Dictionary<string, object> nextMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
										float time2 = GetFloat(nextMap, "time", 0);
										if (keyMap.ContainsKey("curve")) {
											object curve = keyMap["curve"];
											bezier = ReadCurve(curve, timeline, bezier, frame, 0, time, time2, 0, 1, 1);
										}
										time = time2;
										keyMap = nextMap;
									}
									timelines.Add(timeline);
									break;
								}
								case "sequence": {
									var timeline = new SequenceTimeline(frames, slot.index, attachment);
									float lastDelay = 0;
									for (int frame = 0; keyMap != null; keyMap = keyMapEnumerator.MoveNext() ?
										(Dictionary<string, Object>)keyMapEnumerator.Current : null, frame++) {

										float delay = GetFloat(keyMap, "delay", lastDelay);
										SequenceMode sequenceMode = (SequenceMode)Enum.Parse(typeof(SequenceMode),
											GetString(keyMap, "mode", "hold"), true);
										timeline.SetFrame(frame, GetFloat(keyMap, "time", 0),
											sequenceMode, GetInt(keyMap, "index", 0), delay);
										lastDelay = delay;
									}
									timelines.Add(timeline);
									break;
								}
								}
							}
						}
					}
				}
			}

			// Draw order timeline.
			if (map.ContainsKey("drawOrder")) {
				List<object> drawOrderMap = (List<object>)map["drawOrder"];
				var timeline = new DrawOrderTimeline(drawOrderMap.Count);
				int slotCount = skeletonData.slots.Count, frame = 0;
				foreach (Dictionary<string, object> keyMap in drawOrderMap) {
					timeline.SetFrame(frame, GetFloat(keyMap, "time", 0), ReadDrawOrder(skeletonData, keyMap, slotCount, null));
					frame++;
				}
				timelines.Add(timeline);
			}

			// Draw order folder timelines.
			if (map.ContainsKey("drawOrderFolder")) {
				List<object> drawOrderFolderMap = (List<object>)map["drawOrderFolder"];
				foreach (Dictionary<string, object> timelineMap in drawOrderFolderMap) {
					List<object> slotEntries = (List<object>)timelineMap["slots"];
					var folderSlots = new int[slotEntries.Count];
					int ii = 0;
					foreach (string slotEntry in slotEntries) {
						SlotData slot = skeletonData.FindSlot(slotEntry);
						if (slot == null) throw new SerializationException("Draw order folder slot not found: " + slotEntry);
						folderSlots[ii] = slot.index;
						ii++;
					}

					List<object> keys = (List<object>)timelineMap["keys"];
					var timeline = new DrawOrderFolderTimeline(keys.Count, folderSlots, skeletonData.slots.Count);
					int frame = 0;
					foreach (Dictionary<string, object> keyMap in keys) {
						timeline.SetFrame(frame, GetFloat(keyMap, "time", 0),
							ReadDrawOrder(skeletonData, keyMap, folderSlots.Length, folderSlots));
						frame++;
					}
					timelines.Add(timeline);
				}
			}

			// Event timeline.
			if (map.ContainsKey("events")) {
				List<object> eventsMap = (List<Object>)map["events"];
				var timeline = new EventTimeline(eventsMap.Count);
				int frame = 0;
				foreach (Dictionary<string, Object> keyMap in eventsMap) {
					EventData data = skeletonData.FindEvent((string)keyMap["name"]);
					if (data == null) throw new Exception("Event not found: " + keyMap["name"]);
					Event setup = data.setupPose;
					var e = new Event(GetFloat(keyMap, "time", 0), data) {
						intValue = GetInt(keyMap, "int", setup.intValue),
						floatValue = GetFloat(keyMap, "float", setup.floatValue),
						stringValue = GetString(keyMap, "string", setup.stringValue)
					};
					if (e.data.AudioPath != null) {
						e.volume = GetFloat(keyMap, "volume", setup.volume);
						e.balance = GetFloat(keyMap, "balance", setup.balance);
					}
					timeline.SetFrame(frame, e);
					++frame;
				}
				timelines.Add(timeline);
			}
			timelines.TrimExcess();
			float duration = 0;
			Timeline[] items = timelines.Items;
			for (int i = 0, n = timelines.Count; i < n; i++)
				duration = Math.Max(duration, items[i].Duration);

			Animation animation = new Animation(name);
			animation.SetTimelines(timelines, bones);
			animation.Duration = duration;
			skeletonData.animations.Add(animation);
		}

		static void ReadTimeline (ref ExposedList<Timeline> timelines, ref List<object>.Enumerator keyMapEnumerator, CurveTimeline1 timeline, float defaultValue,
			float scale) {

			Dictionary<string, object> keyMap = (Dictionary<string, object>)keyMapEnumerator.Current;
			float time = GetFloat(keyMap, "time", 0);
			float value = GetFloat(keyMap, "value", defaultValue) * scale;
			for (int frame = 0, bezier = 0; ; frame++) {
				timeline.SetFrame(frame, time, value);
				if (!keyMapEnumerator.MoveNext()) {
					timeline.Shrink(bezier);
					timelines.Add(timeline);
					return;
				}
				Dictionary<string, object> nextMap = (Dictionary<string, object>)keyMapEnumerator.Current;
				float time2 = GetFloat(nextMap, "time", 0);
				float value2 = GetFloat(nextMap, "value", defaultValue) * scale;
				if (keyMap.ContainsKey("curve")) {
					object curve = keyMap["curve"];
					bezier = ReadCurve(curve, timeline, bezier, frame, 0, time, time2, value, value2, scale);
				}
				time = time2;
				value = value2;
				keyMap = nextMap;
			}
		}

		static void ReadTimeline (ref ExposedList<Timeline> timelines, ref List<object>.Enumerator keyMapEnumerator, BoneTimeline2 timeline, string name1, string name2,
			float defaultValue, float scale) {

			Dictionary<string, object> keyMap = (Dictionary<string, object>)keyMapEnumerator.Current;
			float time = GetFloat(keyMap, "time", 0);
			float value1 = GetFloat(keyMap, name1, defaultValue) * scale, value2 = GetFloat(keyMap, name2, defaultValue) * scale;
			for (int frame = 0, bezier = 0; ; frame++) {
				timeline.SetFrame(frame, time, value1, value2);
				if (!keyMapEnumerator.MoveNext()) {
					timeline.Shrink(bezier);
					timelines.Add(timeline);
					return;
				}
				Dictionary<string, object> nextMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
				float time2 = GetFloat(nextMap, "time", 0);
				float nvalue1 = GetFloat(nextMap, name1, defaultValue) * scale, nvalue2 = GetFloat(nextMap, name2, defaultValue) * scale;
				if (keyMap.ContainsKey("curve")) {
					object curve = keyMap["curve"];
					bezier = ReadCurve(curve, timeline, bezier, frame, 0, time, time2, value1, nvalue1, scale);
					bezier = ReadCurve(curve, timeline, bezier, frame, 1, time, time2, value2, nvalue2, scale);
				}
				time = time2;
				value1 = nvalue1;
				value2 = nvalue2;
				keyMap = nextMap;
			}
		}

		/// <param name="folderSlots">
		/// Slot names are resolved to positions within this array. If null, slot indices are used as positions.</param>
		private int[] ReadDrawOrder (SkeletonData skeletonData, Dictionary<string, object> keyMap, int slotCount, int[] folderSlots) {
			if (!keyMap.ContainsKey("offsets")) return null; // Setup draw order.
			List<object> changes = (List<object>)keyMap["offsets"];
			var drawOrder = new int[slotCount];
			for (int ii = slotCount - 1; ii >= 0; ii--)
				drawOrder[ii] = -1;
			var unchanged = new int[slotCount - changes.Count];
			int originalIndex = 0, unchangedIndex = 0;

			foreach (Dictionary<string, object> offsetMap in changes) {
				SlotData slot = skeletonData.FindSlot((string)offsetMap["slot"]);
				if (slot == null) throw new SerializationException("Draw order slot not found: " + (string)offsetMap["slot"]);
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
					if (index == -1) throw new SerializationException("Slot not in folder: " + (string)offsetMap["slot"]);
				}
				// Collect unchanged items.
				while (originalIndex != index)
					unchanged[unchangedIndex++] = originalIndex++;
				// Set changed items.
				drawOrder[originalIndex + GetInt(offsetMap, "offset")] = originalIndex++;
			}
			// Collect remaining unchanged items.
			while (originalIndex < slotCount)
				unchanged[unchangedIndex++] = originalIndex++;
			// Fill in unchanged items.
			for (int i = slotCount - 1; i >= 0; i--)
				if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
			return drawOrder;
		}

		static int ReadCurve (object curve, CurveTimeline timeline, int bezier, int frame, int value, float time1, float time2,
			float value1, float value2, float scale) {

			string curveString = curve as string;
			if (curveString != null) {
				if (curveString == "stepped") timeline.SetStepped(frame);
				return bezier;
			}
			List<object> curveValues = (List<object>)curve;
			int i = value << 2;
			float cx1 = (float)curveValues[i];
			float cy1 = (float)curveValues[i + 1] * scale;
			float cx2 = (float)curveValues[i + 2];
			float cy2 = (float)curveValues[i + 3] * scale;
			SetBezier(timeline, frame, value, bezier, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
			return bezier + 1;
		}

		static void SetBezier (CurveTimeline timeline, int frame, int value, int bezier, float time1, float value1, float cx1, float cy1,
			float cx2, float cy2, float time2, float value2) {
			timeline.SetBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
		}

		static float[] GetFloatArray (Dictionary<string, Object> map, string name, float scale) {
			List<object> list = (List<Object>)map[name];
			float[] values = new float[list.Count];
			if (scale == 1) {
				for (int i = 0, n = list.Count; i < n; i++)
					values[i] = (float)list[i];
			} else {
				for (int i = 0, n = list.Count; i < n; i++)
					values[i] = (float)list[i] * scale;
			}
			return values;
		}

		static int[] GetIntArray (Dictionary<string, Object> map, string name) {
			List<object> list = (List<Object>)map[name];
			int[] values = new int[list.Count];
			for (int i = 0, n = list.Count; i < n; i++)
				values[i] = (int)(float)list[i];
			return values;
		}

		static float GetFloat (Dictionary<string, Object> map, string name, float defaultValue) {
			if (!map.ContainsKey(name)) return defaultValue;
			return (float)map[name];
		}

		static float GetFloat (Dictionary<string, Object> map, string name) {
			if (!map.ContainsKey(name)) throw new ArgumentException("Named value not found: " + name);
			return (float)map[name];
		}

		static int GetInt (Dictionary<string, Object> map, string name, int defaultValue) {
			if (!map.ContainsKey(name)) return defaultValue;
			return (int)(float)map[name];
		}

		static int GetInt (Dictionary<string, Object> map, string name) {
			if (!map.ContainsKey(name)) throw new ArgumentException("Named value not found: " + name);
			return (int)(float)map[name];
		}

		static bool GetBoolean (Dictionary<string, Object> map, string name, bool defaultValue) {
			if (!map.ContainsKey(name)) return defaultValue;
			return (bool)map[name];
		}

		static string GetString (Dictionary<string, Object> map, string name, string defaultValue) {
			if (!map.ContainsKey(name)) return defaultValue;
			return (string)map[name];
		}

		static float ToColor (string hexString, int colorIndex, int expectedLength = 8) {
			if (hexString.Length < expectedLength)
				throw new ArgumentException("Color hexadecimal length must be " + expectedLength + ", received: " + hexString, "hexString");
			return Convert.ToInt32(hexString.Substring(colorIndex * 2, 2), 16) / (float)255;
		}

		static Color32F ToColor32 (string hexString, int expectedLength = 8) {
			if (hexString.Length < expectedLength)
				throw new ArgumentException("Color hexadecimal length must be " + expectedLength + ", received: " + hexString, "hexString");

			float r = Convert.ToInt32(hexString.Substring(0, 2), 16) / (float)255;
			float g = Convert.ToInt32(hexString.Substring(2, 2), 16) / (float)255;
			float b = Convert.ToInt32(hexString.Substring(4, 2), 16) / (float)255;
			float a = Convert.ToInt32(hexString.Substring(6, 2), 16) / (float)255;
			return new Color32F(r, g, b, a);
		}

		static Color32F ToColor24 (string hexString, int expectedLength = 6) {
			if (hexString.Length < expectedLength)
				throw new ArgumentException("Color hexadecimal length must be " + expectedLength + ", received: " + hexString, "hexString");

			float r = Convert.ToInt32(hexString.Substring(0, 2), 16) / (float)255;
			float g = Convert.ToInt32(hexString.Substring(2, 2), 16) / (float)255;
			float b = Convert.ToInt32(hexString.Substring(4, 2), 16) / (float)255;
			return new Color32F(r, g, b);
		}

		private class LinkedMesh {
			internal string parent, skin;
			internal int slotIndex;
			internal MeshAttachment mesh;
			internal bool inheritTimelines;

			public LinkedMesh (MeshAttachment mesh, string skin, int slotIndex, string parent, bool inheritTimelines) {
				this.mesh = mesh;
				this.skin = skin;
				this.slotIndex = slotIndex;
				this.parent = parent;
				this.inheritTimelines = inheritTimelines;
			}
		}
	}
}
