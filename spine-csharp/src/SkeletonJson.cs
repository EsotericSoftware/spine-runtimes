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

#if (UNITY_5 || UNITY_5_3_OR_NEWER || UNITY_WSA || UNITY_WP8 || UNITY_WP8_1)
#define IS_UNITY
#endif

using System;
using System.Collections.Generic;
using System.IO;

#if WINDOWS_STOREAPP
using System.Threading.Tasks;
using Windows.Storage;
#endif

namespace Spine {

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

			float scale = this.scale;
			SkeletonData skeletonData = new SkeletonData();

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
					BoneData data = new BoneData(skeletonData.Bones.Count, (string)boneMap["name"], parent);
					data.length = GetFloat(boneMap, "length", 0) * scale;
					data.x = GetFloat(boneMap, "x", 0) * scale;
					data.y = GetFloat(boneMap, "y", 0) * scale;
					data.rotation = GetFloat(boneMap, "rotation", 0);
					data.scaleX = GetFloat(boneMap, "scaleX", 1);
					data.scaleY = GetFloat(boneMap, "scaleY", 1);
					data.shearX = GetFloat(boneMap, "shearX", 0);
					data.shearY = GetFloat(boneMap, "shearY", 0);

					string inheritString = GetString(boneMap, "inherit", Inherit.Normal.ToString());
					data.inherit = (Inherit)Enum.Parse(typeof(Inherit), inheritString, true);
					data.skinRequired = GetBoolean(boneMap, "skin", false);

					skeletonData.bones.Add(data);
				}
			}

			// Slots.
			if (root.ContainsKey("slots")) {
				foreach (Dictionary<string, Object> slotMap in (List<Object>)root["slots"]) {
					string slotName = (string)slotMap["name"]; //, path = null;
					int slash = slotName.LastIndexOf('/');
					if (slash != -1) {
						//path = slotName.Substring(0, slash);
						slotName = slotName.Substring(slash + 1);
					}

					string boneName = (string)slotMap["bone"];
					BoneData boneData = skeletonData.FindBone(boneName);
					if (boneData == null) throw new Exception("Slot bone not found: " + boneName);
					SlotData data = new SlotData(skeletonData.Slots.Count, slotName, boneData);

					if (slotMap.ContainsKey("color")) {
						string color = (string)slotMap["color"];
						data.r = ToColor(color, 0);
						data.g = ToColor(color, 1);
						data.b = ToColor(color, 2);
						data.a = ToColor(color, 3);
					}

					if (slotMap.ContainsKey("dark")) {
						string color2 = (string)slotMap["dark"];
						data.r2 = ToColor(color2, 0, 6); // expectedLength = 6. ie. "RRGGBB"
						data.g2 = ToColor(color2, 1, 6);
						data.b2 = ToColor(color2, 2, 6);
						data.hasSecondColor = true;
					}

					data.attachmentName = GetString(slotMap, "attachment", null);
					if (slotMap.ContainsKey("blend"))
						data.blendMode = (BlendMode)Enum.Parse(typeof(BlendMode), (string)slotMap["blend"], true);
					else
						data.blendMode = BlendMode.Normal;
					//data.visible = slotMap.getBoolean("visible", true);
					//data.path = path;
					skeletonData.slots.Add(data);
				}
			}

			// IK constraints.
			if (root.ContainsKey("ik")) {
				foreach (Dictionary<string, Object> constraintMap in (List<Object>)root["ik"]) {
					IkConstraintData data = new IkConstraintData((string)constraintMap["name"]);
					data.order = GetInt(constraintMap, "order", 0);
					data.skinRequired = GetBoolean(constraintMap, "skin", false);

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
					data.mix = GetFloat(constraintMap, "mix", 1);
					data.softness = GetFloat(constraintMap, "softness", 0) * scale;
					data.bendDirection = GetBoolean(constraintMap, "bendPositive", true) ? 1 : -1;
					data.compress = GetBoolean(constraintMap, "compress", false);
					data.stretch = GetBoolean(constraintMap, "stretch", false);
					data.uniform = GetBoolean(constraintMap, "uniform", false);

					skeletonData.ikConstraints.Add(data);
				}
			}

			// Transform constraints.
			if (root.ContainsKey("transform")) {
				foreach (Dictionary<string, Object> constraintMap in (List<Object>)root["transform"]) {
					TransformConstraintData data = new TransformConstraintData((string)constraintMap["name"]);
					data.order = GetInt(constraintMap, "order", 0);
					data.skinRequired = GetBoolean(constraintMap, "skin", false);

					if (constraintMap.ContainsKey("bones")) {
						foreach (string boneName in (List<Object>)constraintMap["bones"]) {
							BoneData bone = skeletonData.FindBone(boneName);
							if (bone == null) throw new Exception("Transform constraint bone not found: " + boneName);
							data.bones.Add(bone);
						}
					}

					string targetName = (string)constraintMap["target"];
					data.target = skeletonData.FindBone(targetName);
					if (data.target == null) throw new Exception("Transform constraint target bone not found: " + targetName);

					data.local = GetBoolean(constraintMap, "local", false);
					data.relative = GetBoolean(constraintMap, "relative", false);

					data.offsetRotation = GetFloat(constraintMap, "rotation", 0);
					data.offsetX = GetFloat(constraintMap, "x", 0) * scale;
					data.offsetY = GetFloat(constraintMap, "y", 0) * scale;
					data.offsetScaleX = GetFloat(constraintMap, "scaleX", 0);
					data.offsetScaleY = GetFloat(constraintMap, "scaleY", 0);
					data.offsetShearY = GetFloat(constraintMap, "shearY", 0);

					data.mixRotate = GetFloat(constraintMap, "mixRotate", 1);
					data.mixX = GetFloat(constraintMap, "mixX", 1);
					data.mixY = GetFloat(constraintMap, "mixY", data.mixX);
					data.mixScaleX = GetFloat(constraintMap, "mixScaleX", 1);
					data.mixScaleY = GetFloat(constraintMap, "mixScaleY", data.mixScaleX);
					data.mixShearY = GetFloat(constraintMap, "mixShearY", 1);

					skeletonData.transformConstraints.Add(data);
				}
			}

			// Path constraints.
			if (root.ContainsKey("path")) {
				foreach (Dictionary<string, Object> constraintMap in (List<Object>)root["path"]) {
					PathConstraintData data = new PathConstraintData((string)constraintMap["name"]);
					data.order = GetInt(constraintMap, "order", 0);
					data.skinRequired = GetBoolean(constraintMap, "skin", false);

					if (constraintMap.ContainsKey("bones")) {
						foreach (string boneName in (List<Object>)constraintMap["bones"]) {
							BoneData bone = skeletonData.FindBone(boneName);
							if (bone == null) throw new Exception("Path bone not found: " + boneName);
							data.bones.Add(bone);
						}
					}

					string targetName = (string)constraintMap["target"];
					data.target = skeletonData.FindSlot(targetName);
					if (data.target == null) throw new Exception("Path target slot not found: " + targetName);

					data.positionMode = (PositionMode)Enum.Parse(typeof(PositionMode), GetString(constraintMap, "positionMode", "percent"), true);
					data.spacingMode = (SpacingMode)Enum.Parse(typeof(SpacingMode), GetString(constraintMap, "spacingMode", "length"), true);
					data.rotateMode = (RotateMode)Enum.Parse(typeof(RotateMode), GetString(constraintMap, "rotateMode", "tangent"), true);
					data.offsetRotation = GetFloat(constraintMap, "rotation", 0);
					data.position = GetFloat(constraintMap, "position", 0);
					if (data.positionMode == PositionMode.Fixed) data.position *= scale;
					data.spacing = GetFloat(constraintMap, "spacing", 0);
					if (data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed) data.spacing *= scale;
					data.mixRotate = GetFloat(constraintMap, "mixRotate", 1);
					data.mixX = GetFloat(constraintMap, "mixX", 1);
					data.mixY = GetFloat(constraintMap, "mixY", data.mixX);

					skeletonData.pathConstraints.Add(data);
				}
			}

			// Physics constraints.
			if (root.ContainsKey("physics")) {
				foreach (Dictionary<string, Object> constraintMap in (List<Object>)root["physics"]) {
					PhysicsConstraintData data = new PhysicsConstraintData((string)constraintMap["name"]);
					data.order = GetInt(constraintMap, "order", 0);
					data.skinRequired = GetBoolean(constraintMap, "skin", false);

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
					data.inertia = GetFloat(constraintMap, "inertia", 1);
					data.strength = GetFloat(constraintMap, "strength", 100);
					data.damping = GetFloat(constraintMap, "damping", 1);
					data.massInverse = 1f / GetFloat(constraintMap, "mass", 1);
					data.wind = GetFloat(constraintMap, "wind", 0);
					data.gravity = GetFloat(constraintMap, "gravity", 0);
					data.mix = GetFloat(constraintMap, "mix", 1);
					data.inertiaGlobal = GetBoolean(constraintMap, "inertiaGlobal", false);
					data.strengthGlobal = GetBoolean(constraintMap, "strengthGlobal", false);
					data.dampingGlobal = GetBoolean(constraintMap, "dampingGlobal", false);
					data.massGlobal = GetBoolean(constraintMap, "massGlobal", false);
					data.windGlobal = GetBoolean(constraintMap, "windGlobal", false);
					data.gravityGlobal = GetBoolean(constraintMap, "gravityGlobal", false);
					data.mixGlobal = GetBoolean(constraintMap, "mixGlobal", false);

					skeletonData.physicsConstraints.Add(data);
				}
			}

			// Skins.
			if (root.ContainsKey("skins")) {
				foreach (Dictionary<string, object> skinMap in (List<object>)root["skins"]) {
					Skin skin = new Skin((string)skinMap["name"]);
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
							IkConstraintData constraint = skeletonData.FindIkConstraint(entryName);
							if (constraint == null) throw new Exception("Skin IK constraint not found: " + entryName);
							skin.constraints.Add(constraint);
						}
					}
					if (skinMap.ContainsKey("transform")) {
						foreach (string entryName in (List<Object>)skinMap["transform"]) {
							TransformConstraintData constraint = skeletonData.FindTransformConstraint(entryName);
							if (constraint == null) throw new Exception("Skin transform constraint not found: " + entryName);
							skin.constraints.Add(constraint);
						}
					}
					if (skinMap.ContainsKey("path")) {
						foreach (string entryName in (List<Object>)skinMap["path"]) {
							PathConstraintData constraint = skeletonData.FindPathConstraint(entryName);
							if (constraint == null) throw new Exception("Skin path constraint not found: " + entryName);
							skin.constraints.Add(constraint);
						}
					}
					if (skinMap.ContainsKey("physics")) {
						foreach (string entryName in (List<Object>)skinMap["physics"]) {
							PhysicsConstraintData constraint = skeletonData.FindPhysicsConstraint(entryName);
							if (constraint == null) throw new Exception("Skin physics constraint not found: " + entryName);
							skin.constraints.Add(constraint);
						}
					}
					skin.constraints.TrimExcess();
					if (skinMap.ContainsKey("attachments")) {
						foreach (KeyValuePair<string, Object> slotEntry in (Dictionary<string, Object>)skinMap["attachments"]) {
							int slotIndex = FindSlotIndex(skeletonData, slotEntry.Key);
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
				if (linkedMesh.mesh.Region != null) linkedMesh.mesh.UpdateRegion();
			}
			linkedMeshes.Clear();

			// Events.
			if (root.ContainsKey("events")) {
				foreach (KeyValuePair<string, Object> entry in (Dictionary<string, Object>)root["events"]) {
					Dictionary<string, object> entryMap = (Dictionary<string, Object>)entry.Value;
					EventData data = new EventData(entry.Key);
					data.Int = GetInt(entryMap, "int", 0);
					data.Float = GetFloat(entryMap, "float", 0);
					data.String = GetString(entryMap, "string", string.Empty);
					data.AudioPath = GetString(entryMap, "audio", null);
					if (data.AudioPath != null) {
						data.Volume = GetFloat(entryMap, "volume", 1);
						data.Balance = GetFloat(entryMap, "balance", 0);
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

			skeletonData.bones.TrimExcess();
			skeletonData.slots.TrimExcess();
			skeletonData.skins.TrimExcess();
			skeletonData.events.TrimExcess();
			skeletonData.animations.TrimExcess();
			skeletonData.ikConstraints.TrimExcess();
			return skeletonData;
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
				region.width = GetFloat(map, "width", 32) * scale;
				region.height = GetFloat(map, "height", 32) * scale;
				region.sequence = sequence;

				if (map.ContainsKey("color")) {
					string color = (string)map["color"];
					region.r = ToColor(color, 0);
					region.g = ToColor(color, 1);
					region.b = ToColor(color, 2);
					region.a = ToColor(color, 3);
				}

				if (region.Region != null) region.UpdateRegion();
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
					mesh.r = ToColor(color, 0);
					mesh.g = ToColor(color, 1);
					mesh.b = ToColor(color, 2);
					mesh.a = ToColor(color, 3);
				}

				mesh.Width = GetFloat(map, "width", 0) * scale;
				mesh.Height = GetFloat(map, "height", 0) * scale;
				mesh.Sequence = sequence;

				string parent = GetString(map, "parent", null);
				if (parent != null) {
					linkedMeshes.Add(new LinkedMesh(mesh, GetString(map, "skin", null), slotIndex, parent, GetBoolean(map, "timelines", true)));
					return mesh;
				}

				float[] uvs = GetFloatArray(map, "uvs", 1);
				ReadVertices(map, mesh, uvs.Length);
				mesh.triangles = GetIntArray(map, "triangles");
				mesh.regionUVs = uvs;
				if (mesh.Region != null) mesh.UpdateRegion();

				if (map.ContainsKey("hull")) mesh.HullLength = GetInt(map, "hull", 0) << 1;
				if (map.ContainsKey("edges")) mesh.Edges = GetIntArray(map, "edges");
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
			if (map == null) return null;
			Sequence sequence = new Sequence(GetInt(map, "count"));
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
					for (int i = 0; i < vertices.Length; i++) {
						vertices[i] *= scale;
					}
				}
				attachment.vertices = vertices;
				return;
			}
			ExposedList<float> weights = new ExposedList<float>(verticesLength * 3 * 3);
			ExposedList<int> bones = new ExposedList<int>(verticesLength * 3);
			for (int i = 0, n = vertices.Length; i < n;) {
				int boneCount = (int)vertices[i++];
				bones.Add(boneCount);
				for (int nn = i + (boneCount << 2); i < nn; i += 4) {
					bones.Add((int)vertices[i]);
					weights.Add(vertices[i + 1] * this.Scale);
					weights.Add(vertices[i + 2] * this.Scale);
					weights.Add(vertices[i + 3]);
				}
			}
			attachment.bones = bones.ToArray();
			attachment.vertices = weights.ToArray();
		}

		private int FindSlotIndex (SkeletonData skeletonData, string slotName) {
			SlotData[] slots = skeletonData.slots.Items;
			for (int i = 0, n = skeletonData.slots.Count; i < n; i++)
				if (slots[i].name == slotName) return i;
			throw new Exception("Slot not found: " + slotName);
		}

		private void ReadAnimation (Dictionary<string, Object> map, string name, SkeletonData skeletonData) {
			float scale = this.scale;
			ExposedList<Timeline> timelines = new ExposedList<Timeline>();

			// Slot timelines.
			if (map.ContainsKey("slots")) {
				foreach (KeyValuePair<string, Object> entry in (Dictionary<string, Object>)map["slots"]) {
					string slotName = entry.Key;
					int slotIndex = FindSlotIndex(skeletonData, slotName);
					Dictionary<string, object> timelineMap = (Dictionary<string, Object>)entry.Value;
					foreach (KeyValuePair<string, Object> timelineEntry in timelineMap) {
						List<object> values = (List<Object>)timelineEntry.Value;
						int frames = values.Count;
						if (frames == 0) continue;
						string timelineName = (string)timelineEntry.Key;
						if (timelineName == "attachment") {
							AttachmentTimeline timeline = new AttachmentTimeline(frames, slotIndex);
							int frame = 0;
							foreach (Dictionary<string, Object> keyMap in values) {
								timeline.SetFrame(frame++, GetFloat(keyMap, "time", 0), GetString(keyMap, "name", null));
							}
							timelines.Add(timeline);

						} else if (timelineName == "rgba") {
							RGBATimeline timeline = new RGBATimeline(frames, frames << 2, slotIndex);

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

						} else if (timelineName == "rgb") {
							RGBTimeline timeline = new RGBTimeline(frames, frames * 3, slotIndex);

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

						} else if (timelineName == "alpha") {
							List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
							keyMapEnumerator.MoveNext();
							timelines.Add(ReadTimeline(ref keyMapEnumerator, new AlphaTimeline(frames, frames, slotIndex), 0, 1));

						} else if (timelineName == "rgba2") {
							RGBA2Timeline timeline = new RGBA2Timeline(frames, frames * 7, slotIndex);

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

						} else if (timelineName == "rgb2") {
							RGB2Timeline timeline = new RGB2Timeline(frames, frames * 6, slotIndex);

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

						} else
							throw new Exception("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
					}
				}
			}

			// Bone timelines.
			if (map.ContainsKey("bones")) {
				foreach (KeyValuePair<string, Object> entry in (Dictionary<string, Object>)map["bones"]) {
					string boneName = entry.Key;
					int boneIndex = -1;
					BoneData[] bones = skeletonData.bones.Items;
					for (int i = 0, n = skeletonData.bones.Count; i < n; i++) {
						if (bones[i].name == boneName) {
							boneIndex = i;
							break;
						}
					}
					if (boneIndex == -1) throw new Exception("Bone not found: " + boneName);
					Dictionary<string, object> timelineMap = (Dictionary<string, Object>)entry.Value;
					foreach (KeyValuePair<string, Object> timelineEntry in timelineMap) {
						List<object> values = (List<Object>)timelineEntry.Value;
						List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
						if (!keyMapEnumerator.MoveNext()) continue;
						int frames = values.Count;
						string timelineName = (string)timelineEntry.Key;
						if (timelineName == "rotate")
							timelines.Add(ReadTimeline(ref keyMapEnumerator, new RotateTimeline(frames, frames, boneIndex), 0, 1));
						else if (timelineName == "translate") {
							TranslateTimeline timeline = new TranslateTimeline(frames, frames << 1, boneIndex);
							timelines.Add(ReadTimeline(ref keyMapEnumerator, timeline, "x", "y", 0, scale));
						} else if (timelineName == "translatex") {
							timelines
								.Add(ReadTimeline(ref keyMapEnumerator, new TranslateXTimeline(frames, frames, boneIndex), 0, scale));
						} else if (timelineName == "translatey") {
							timelines
								.Add(ReadTimeline(ref keyMapEnumerator, new TranslateYTimeline(frames, frames, boneIndex), 0, scale));
						} else if (timelineName == "scale") {
							ScaleTimeline timeline = new ScaleTimeline(frames, frames << 1, boneIndex);
							timelines.Add(ReadTimeline(ref keyMapEnumerator, timeline, "x", "y", 1, 1));
						} else if (timelineName == "scalex")
							timelines.Add(ReadTimeline(ref keyMapEnumerator, new ScaleXTimeline(frames, frames, boneIndex), 1, 1));
						else if (timelineName == "scaley")
							timelines.Add(ReadTimeline(ref keyMapEnumerator, new ScaleYTimeline(frames, frames, boneIndex), 1, 1));
						else if (timelineName == "shear") {
							ShearTimeline timeline = new ShearTimeline(frames, frames << 1, boneIndex);
							timelines.Add(ReadTimeline(ref keyMapEnumerator, timeline, "x", "y", 0, 1));
						} else if (timelineName == "shearx")
							timelines.Add(ReadTimeline(ref keyMapEnumerator, new ShearXTimeline(frames, frames, boneIndex), 0, 1));
						else if (timelineName == "sheary")
							timelines.Add(ReadTimeline(ref keyMapEnumerator, new ShearYTimeline(frames, frames, boneIndex), 0, 1));
						else if (timelineName == "inherit") {
							InheritTimeline timeline = new InheritTimeline(frames, boneIndex);
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
						} else
							throw new Exception("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
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
					IkConstraintData constraint = skeletonData.FindIkConstraint(timelineMap.Key);
					IkConstraintTimeline timeline = new IkConstraintTimeline(values.Count, values.Count << 1,
						skeletonData.IkConstraints.IndexOf(constraint));
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
					TransformConstraintData constraint = skeletonData.FindTransformConstraint(timelineMap.Key);
					TransformConstraintTimeline timeline = new TransformConstraintTimeline(values.Count, values.Count * 6,
						skeletonData.TransformConstraints.IndexOf(constraint));
					float time = GetFloat(keyMap, "time", 0);
					float mixRotate = GetFloat(keyMap, "mixRotate", 1), mixShearY = GetFloat(keyMap, "mixShearY", 1);
					float mixX = GetFloat(keyMap, "mixX", 1), mixY = GetFloat(keyMap, "mixY", mixX);
					float mixScaleX = GetFloat(keyMap, "mixScaleX", 1), mixScaleY = GetFloat(keyMap, "mixScaleY", mixScaleX);
					for (int frame = 0, bezier = 0; ; frame++) {
						timeline.SetFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
						if (!keyMapEnumerator.MoveNext()) {
							timeline.Shrink(bezier);
							break;
						}
						Dictionary<string, object> nextMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
						float time2 = GetFloat(nextMap, "time", 0);
						float mixRotate2 = GetFloat(nextMap, "mixRotate", 1), mixShearY2 = GetFloat(nextMap, "mixShearY", 1);
						float mixX2 = GetFloat(nextMap, "mixX", 1), mixY2 = GetFloat(nextMap, "mixY", mixX2);
						float mixScaleX2 = GetFloat(nextMap, "mixScaleX", 1), mixScaleY2 = GetFloat(nextMap, "mixScaleY", mixScaleX2);
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
					PathConstraintData constraint = skeletonData.FindPathConstraint(constraintMap.Key);
					if (constraint == null) throw new Exception("Path constraint not found: " + constraintMap.Key);
					int constraintIndex = skeletonData.pathConstraints.IndexOf(constraint);
					Dictionary<string, object> timelineMap = (Dictionary<string, Object>)constraintMap.Value;
					foreach (KeyValuePair<string, Object> timelineEntry in timelineMap) {
						List<object> values = (List<Object>)timelineEntry.Value;
						List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
						if (!keyMapEnumerator.MoveNext()) continue;

						int frames = values.Count;
						string timelineName = (string)timelineEntry.Key;
						if (timelineName == "position") {
							CurveTimeline1 timeline = new PathConstraintPositionTimeline(frames, frames, constraintIndex);
							timelines.Add(ReadTimeline(ref keyMapEnumerator, timeline, 0, constraint.positionMode == PositionMode.Fixed ? scale : 1));
						} else if (timelineName == "spacing") {
							CurveTimeline1 timeline = new PathConstraintSpacingTimeline(frames, frames, constraintIndex);
							timelines.Add(ReadTimeline(ref keyMapEnumerator, timeline, 0,
								constraint.spacingMode == SpacingMode.Length || constraint.spacingMode == SpacingMode.Fixed ? scale : 1));
						} else if (timelineName == "mix") {
							PathConstraintMixTimeline timeline = new PathConstraintMixTimeline(frames, frames * 3, constraintIndex);
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
						}
					}
				}
			}

			// Physics constraint timelines.
			if (map.ContainsKey("physics")) {
				foreach (KeyValuePair<string, Object> constraintMap in (Dictionary<string, Object>)map["physics"]) {
					int index = -1;
					if (!string.IsNullOrEmpty(constraintMap.Key)) {
						PhysicsConstraintData constraint = skeletonData.FindPhysicsConstraint(constraintMap.Key);
						if (constraint == null) throw new Exception("Physics constraint not found: " + constraintMap.Key);
						index = skeletonData.physicsConstraints.IndexOf(constraint);
					}
					Dictionary<string, object> timelineMap = (Dictionary<string, Object>)constraintMap.Value;
					foreach (KeyValuePair<string, Object> timelineEntry in timelineMap) {
						List<object> values = (List<Object>)timelineEntry.Value;
						List<object>.Enumerator keyMapEnumerator = values.GetEnumerator();
						if (!keyMapEnumerator.MoveNext()) continue;

						int frames = values.Count;
						string timelineName = (string)timelineEntry.Key;
						if (timelineName == "reset") {
							PhysicsConstraintResetTimeline timeline1 = new PhysicsConstraintResetTimeline(frames, index);
							int frame = 0;
							foreach (Dictionary<string, Object> keyMap in values) {
								timeline1.SetFrame(frame++, GetFloat(keyMap, "time", 0));
							}
							timelines.Add(timeline1);
							continue;
						}

						CurveTimeline1 timeline;
						if (timelineName == "inertia")
							timeline = new PhysicsConstraintInertiaTimeline(frames, frames, index);
						else if (timelineName == "strength")
							timeline = new PhysicsConstraintStrengthTimeline(frames, frames, index);
						else if (timelineName == "damping")
							timeline = new PhysicsConstraintDampingTimeline(frames, frames, index);
						else if (timelineName == "mass")
							timeline = new PhysicsConstraintMassTimeline(frames, frames, index);
						else if (timelineName == "wind")
							timeline = new PhysicsConstraintWindTimeline(frames, frames, index);
						else if (timelineName == "gravity")
							timeline = new PhysicsConstraintGravityTimeline(frames, frames, index);
						else if (timelineName == "mix") //
							timeline = new PhysicsConstraintMixTimeline(frames, frames, index);
						else
							continue;
						timelines.Add(ReadTimeline(ref keyMapEnumerator, timeline, 0, 1));
					}
				}
			}

			// Attachment timelines.
			if (map.ContainsKey("attachments")) {
				foreach (KeyValuePair<string, Object> attachmentsMap in (Dictionary<string, Object>)map["attachments"]) {
					Skin skin = skeletonData.FindSkin(attachmentsMap.Key);
					foreach (KeyValuePair<string, Object> slotMap in (Dictionary<string, Object>)attachmentsMap.Value) {
						SlotData slot = skeletonData.FindSlot(slotMap.Key);
						if (slot == null) throw new Exception("Slot not found: " + slotMap.Key);
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
								if (timelineName == "deform") {
									VertexAttachment vertexAttachment = (VertexAttachment)attachment;
									bool weighted = vertexAttachment.bones != null;
									float[] vertices = vertexAttachment.vertices;
									int deformLength = weighted ? (vertices.Length / 3) << 1 : vertices.Length;

									DeformTimeline timeline = new DeformTimeline(frames, frames, slot.Index, vertexAttachment);
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
								} else if (timelineName == "sequence") {
									SequenceTimeline timeline = new SequenceTimeline(frames, slot.index, attachment);
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
								}
							}
						}
					}
				}
			}

			// Draw order timeline.
			if (map.ContainsKey("drawOrder")) {
				List<object> values = (List<Object>)map["drawOrder"];
				DrawOrderTimeline timeline = new DrawOrderTimeline(values.Count);
				int slotCount = skeletonData.slots.Count;
				int frame = 0;
				foreach (Dictionary<string, Object> keyMap in values) {
					int[] drawOrder = null;
					if (keyMap.ContainsKey("offsets")) {
						drawOrder = new int[slotCount];
						for (int i = slotCount - 1; i >= 0; i--)
							drawOrder[i] = -1;
						List<object> offsets = (List<Object>)keyMap["offsets"];
						int[] unchanged = new int[slotCount - offsets.Count];
						int originalIndex = 0, unchangedIndex = 0;
						foreach (Dictionary<string, Object> offsetMap in offsets) {
							int slotIndex = FindSlotIndex(skeletonData, (string)offsetMap["slot"]);
							// Collect unchanged items.
							while (originalIndex != slotIndex)
								unchanged[unchangedIndex++] = originalIndex++;
							// Set changed items.
							int index = originalIndex + (int)(float)offsetMap["offset"];
							drawOrder[index] = originalIndex++;
						}
						// Collect remaining unchanged items.
						while (originalIndex < slotCount)
							unchanged[unchangedIndex++] = originalIndex++;
						// Fill in unchanged items.
						for (int i = slotCount - 1; i >= 0; i--)
							if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
					}
					timeline.SetFrame(frame, GetFloat(keyMap, "time", 0), drawOrder);
					++frame;
				}
				timelines.Add(timeline);
			}

			// Event timeline.
			if (map.ContainsKey("events")) {
				List<object> eventsMap = (List<Object>)map["events"];
				EventTimeline timeline = new EventTimeline(eventsMap.Count);
				int frame = 0;
				foreach (Dictionary<string, Object> keyMap in eventsMap) {
					EventData eventData = skeletonData.FindEvent((string)keyMap["name"]);
					if (eventData == null) throw new Exception("Event not found: " + keyMap["name"]);
					Event e = new Event(GetFloat(keyMap, "time", 0), eventData) {
						intValue = GetInt(keyMap, "int", eventData.Int),
						floatValue = GetFloat(keyMap, "float", eventData.Float),
						stringValue = GetString(keyMap, "string", eventData.String)
					};
					if (e.data.AudioPath != null) {
						e.volume = GetFloat(keyMap, "volume", eventData.Volume);
						e.balance = GetFloat(keyMap, "balance", eventData.Balance);
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
			skeletonData.animations.Add(new Animation(name, timelines, duration));
		}

		static Timeline ReadTimeline (ref List<object>.Enumerator keyMapEnumerator, CurveTimeline1 timeline, float defaultValue, float scale) {
			Dictionary<string, object> keyMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
			float time = GetFloat(keyMap, "time", 0);
			float value = GetFloat(keyMap, "value", defaultValue) * scale;
			for (int frame = 0, bezier = 0; ; frame++) {
				timeline.SetFrame(frame, time, value);
				if (!keyMapEnumerator.MoveNext()) {
					timeline.Shrink(bezier);
					return timeline;
				}
				Dictionary<string, object> nextMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
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

		static Timeline ReadTimeline (ref List<object>.Enumerator keyMapEnumerator, CurveTimeline2 timeline, String name1, String name2, float defaultValue,
			float scale) {

			Dictionary<string, object> keyMap = (Dictionary<string, Object>)keyMapEnumerator.Current;
			float time = GetFloat(keyMap, "time", 0);
			float value1 = GetFloat(keyMap, name1, defaultValue) * scale, value2 = GetFloat(keyMap, name2, defaultValue) * scale;
			for (int frame = 0, bezier = 0; ; frame++) {
				timeline.SetFrame(frame, time, value1, value2);
				if (!keyMapEnumerator.MoveNext()) {
					timeline.Shrink(bezier);
					return timeline;
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
