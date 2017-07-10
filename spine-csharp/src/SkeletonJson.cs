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

#if (UNITY_5 || UNITY_5_3_OR_NEWER || UNITY_WSA || UNITY_WP8 || UNITY_WP8_1)
#define IS_UNITY
#endif

using System;
using System.IO;
using System.Collections.Generic;

#if WINDOWS_STOREAPP
using System.Threading.Tasks;
using Windows.Storage;
#endif

namespace Spine {
	public class SkeletonJson {
		public float Scale { get; set; }

		private AttachmentLoader attachmentLoader;
		private List<LinkedMesh> linkedMeshes = new List<LinkedMesh>();

		public SkeletonJson (params Atlas[] atlasArray)
			: this(new AtlasAttachmentLoader(atlasArray)) {
		}

		public SkeletonJson (AttachmentLoader attachmentLoader) {
			if (attachmentLoader == null) throw new ArgumentNullException("attachmentLoader", "attachmentLoader cannot be null.");
			this.attachmentLoader = attachmentLoader;
			Scale = 1;
		}

		#if !IS_UNITY && WINDOWS_STOREAPP
		private async Task<SkeletonData> ReadFile(string path) {
			var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
			var file = await folder.GetFileAsync(path).AsTask().ConfigureAwait(false);
			using (var reader = new StreamReader(await file.OpenStreamForReadAsync().ConfigureAwait(false))) {
				SkeletonData skeletonData = ReadSkeletonData(reader);
				skeletonData.Name = Path.GetFileNameWithoutExtension(path);
				return skeletonData;
			}
		}

		public SkeletonData ReadSkeletonData (String path) {
			return this.ReadFile(path).Result;
		}
		#else
		public SkeletonData ReadSkeletonData (String path) {
		#if WINDOWS_PHONE
			using (var reader = new StreamReader(Microsoft.Xna.Framework.TitleContainer.OpenStream(path))) {
		#else
			using (var reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))) {
		#endif
				SkeletonData skeletonData = ReadSkeletonData(reader);
				skeletonData.name = Path.GetFileNameWithoutExtension(path);
				return skeletonData;
			}
		}
		#endif

		public SkeletonData ReadSkeletonData (TextReader reader) {
			if (reader == null) throw new ArgumentNullException("reader", "reader cannot be null.");

			var scale = this.Scale;
			var skeletonData = new SkeletonData();

			var root = Json.Deserialize(reader) as Dictionary<String, Object>;
			if (root == null) throw new Exception("Invalid JSON.");

			// Skeleton.
			if (root.ContainsKey("skeleton")) {
				var skeletonMap = (Dictionary<String, Object>)root["skeleton"];
				skeletonData.hash = (String)skeletonMap["hash"];
				skeletonData.version = (String)skeletonMap["spine"];
				skeletonData.width = GetFloat(skeletonMap, "width", 0);
				skeletonData.height = GetFloat(skeletonMap, "height", 0);
				skeletonData.fps = GetFloat(skeletonMap, "fps", 0);
				skeletonData.imagesPath = GetString(skeletonMap, "images", null);
			}

			// Bones.
			foreach (Dictionary<String, Object> boneMap in (List<Object>)root["bones"]) {
				BoneData parent = null;
				if (boneMap.ContainsKey("parent")) {
					parent = skeletonData.FindBone((String)boneMap["parent"]);
					if (parent == null)
						throw new Exception("Parent bone not found: " + boneMap["parent"]);
				}
				var data = new BoneData(skeletonData.Bones.Count, (String)boneMap["name"], parent);
				data.length = GetFloat(boneMap, "length", 0) * scale;
				data.x = GetFloat(boneMap, "x", 0) * scale;
				data.y = GetFloat(boneMap, "y", 0) * scale;
				data.rotation = GetFloat(boneMap, "rotation", 0);
				data.scaleX = GetFloat(boneMap, "scaleX", 1);
				data.scaleY = GetFloat(boneMap, "scaleY", 1);
				data.shearX = GetFloat(boneMap, "shearX", 0);
				data.shearY = GetFloat(boneMap, "shearY", 0);

				string tm = GetString(boneMap, "transform", TransformMode.Normal.ToString());
				data.transformMode = (TransformMode)Enum.Parse(typeof(TransformMode), tm, true);

				skeletonData.bones.Add(data);
			}

			// Slots.
			if (root.ContainsKey("slots")) {
				foreach (Dictionary<String, Object> slotMap in (List<Object>)root["slots"]) {
					var slotName = (String)slotMap["name"];
					var boneName = (String)slotMap["bone"];
					BoneData boneData = skeletonData.FindBone(boneName);
					if (boneData == null) throw new Exception("Slot bone not found: " + boneName);
					var data = new SlotData(skeletonData.Slots.Count, slotName, boneData);

					if (slotMap.ContainsKey("color")) {
						var color = (String)slotMap["color"];
						data.r = ToColor(color, 0);
						data.g = ToColor(color, 1);
						data.b = ToColor(color, 2);
						data.a = ToColor(color, 3);
					}

					if (slotMap.ContainsKey("dark")) {
						var color2 = (String)slotMap["dark"];
						data.r2 = ToColor(color2, 0, 6); // expectedLength = 6. ie. "RRGGBB"
						data.g2 = ToColor(color2, 1, 6);
						data.b2 = ToColor(color2, 2, 6);
						data.hasSecondColor = true;
					}
						
					data.attachmentName = GetString(slotMap, "attachment", null);
					if (slotMap.ContainsKey("blend"))
						data.blendMode = (BlendMode)Enum.Parse(typeof(BlendMode), (String)slotMap["blend"], true);
					else
						data.blendMode = BlendMode.Normal;
					skeletonData.slots.Add(data);
				}
			}

			// IK constraints.
			if (root.ContainsKey("ik")) {
				foreach (Dictionary<String, Object> constraintMap in (List<Object>)root["ik"]) {
					IkConstraintData data = new IkConstraintData((String)constraintMap["name"]);
					data.order = GetInt(constraintMap, "order", 0);

					foreach (String boneName in (List<Object>)constraintMap["bones"]) {
						BoneData bone = skeletonData.FindBone(boneName);
						if (bone == null) throw new Exception("IK constraint bone not found: " + boneName);
						data.bones.Add(bone);
					}
					
					String targetName = (String)constraintMap["target"];
					data.target = skeletonData.FindBone(targetName);
					if (data.target == null) throw new Exception("Target bone not found: " + targetName);

					data.bendDirection = GetBoolean(constraintMap, "bendPositive", true) ? 1 : -1;
					data.mix = GetFloat(constraintMap, "mix", 1);

					skeletonData.ikConstraints.Add(data);
				}
			}

			// Transform constraints.
			if (root.ContainsKey("transform")) {
				foreach (Dictionary<String, Object> constraintMap in (List<Object>)root["transform"]) {
					TransformConstraintData data = new TransformConstraintData((String)constraintMap["name"]);
					data.order = GetInt(constraintMap, "order", 0);

					foreach (String boneName in (List<Object>)constraintMap["bones"]) {
						BoneData bone = skeletonData.FindBone(boneName);
						if (bone == null) throw new Exception("Transform constraint bone not found: " + boneName);
						data.bones.Add(bone);
					}

					String targetName = (String)constraintMap["target"];
					data.target = skeletonData.FindBone(targetName);
					if (data.target == null) throw new Exception("Target bone not found: " + targetName);

					data.local = GetBoolean(constraintMap, "local", false);
					data.relative = GetBoolean(constraintMap, "relative", false);

					data.offsetRotation = GetFloat(constraintMap, "rotation", 0);
					data.offsetX = GetFloat(constraintMap, "x", 0) * scale;
					data.offsetY = GetFloat(constraintMap, "y", 0) * scale;
					data.offsetScaleX = GetFloat(constraintMap, "scaleX", 0);
					data.offsetScaleY = GetFloat(constraintMap, "scaleY", 0);
					data.offsetShearY = GetFloat(constraintMap, "shearY", 0);

					data.rotateMix = GetFloat(constraintMap, "rotateMix", 1);
					data.translateMix = GetFloat(constraintMap, "translateMix", 1);
					data.scaleMix = GetFloat(constraintMap, "scaleMix", 1);
					data.shearMix = GetFloat(constraintMap, "shearMix", 1);

					skeletonData.transformConstraints.Add(data);
				}
			}

			// Path constraints.
			if(root.ContainsKey("path")) {
				foreach (Dictionary<String, Object> constraintMap in (List<Object>)root["path"]) {
					PathConstraintData data = new PathConstraintData((String)constraintMap["name"]);
					data.order = GetInt(constraintMap, "order", 0);

					foreach (String boneName in (List<Object>)constraintMap["bones"]) {
						BoneData bone = skeletonData.FindBone(boneName);
						if (bone == null) throw new Exception("Path bone not found: " + boneName);
						data.bones.Add(bone);
					}

					String targetName = (String)constraintMap["target"];
					data.target = skeletonData.FindSlot(targetName);
					if (data.target == null) throw new Exception("Target slot not found: " + targetName);

					data.positionMode = (PositionMode)Enum.Parse(typeof(PositionMode), GetString(constraintMap, "positionMode", "percent"), true);
					data.spacingMode = (SpacingMode)Enum.Parse(typeof(SpacingMode), GetString(constraintMap, "spacingMode", "length"), true);
					data.rotateMode = (RotateMode)Enum.Parse(typeof(RotateMode), GetString(constraintMap, "rotateMode", "tangent"), true);
					data.offsetRotation = GetFloat(constraintMap, "rotation", 0);
					data.position = GetFloat(constraintMap, "position", 0);
					if (data.positionMode == PositionMode.Fixed) data.position *= scale;
					data.spacing = GetFloat(constraintMap, "spacing", 0);
					if (data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed) data.spacing *= scale;
					data.rotateMix = GetFloat(constraintMap, "rotateMix", 1);
					data.translateMix = GetFloat(constraintMap, "translateMix", 1);

					skeletonData.pathConstraints.Add(data);
				}
			}

			// Skins.
			if (root.ContainsKey("skins")) {
					foreach (KeyValuePair<String, Object> skinMap in (Dictionary<String, Object>)root["skins"]) {
					var skin = new Skin(skinMap.Key);
					foreach (KeyValuePair<String, Object> slotEntry in (Dictionary<String, Object>)skinMap.Value) {
						int slotIndex = skeletonData.FindSlotIndex(slotEntry.Key);
						foreach (KeyValuePair<String, Object> entry in ((Dictionary<String, Object>)slotEntry.Value)) {
							try {
								Attachment attachment = ReadAttachment((Dictionary<String, Object>)entry.Value, skin, slotIndex, entry.Key, skeletonData);
								if (attachment != null) skin.AddAttachment(slotIndex, entry.Key, attachment);
							} catch (Exception e) {
								throw new Exception("Error reading attachment: " + entry.Key + ", skin: " + skin, e);
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
				linkedMesh.mesh.ParentMesh = (MeshAttachment)parent;
				linkedMesh.mesh.UpdateUVs();
			}
			linkedMeshes.Clear();

			// Events.
			if (root.ContainsKey("events")) {
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)root["events"]) {
					var entryMap = (Dictionary<String, Object>)entry.Value;
					var data = new EventData(entry.Key);
					data.Int = GetInt(entryMap, "int", 0);
					data.Float = GetFloat(entryMap, "float", 0);
					data.String = GetString(entryMap, "string", string.Empty);
					skeletonData.events.Add(data);
				}
			}

			// Animations.
			if (root.ContainsKey("animations")) {
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)root["animations"]) {
					try {
						ReadAnimation((Dictionary<String, Object>)entry.Value, entry.Key, skeletonData);
					} catch (Exception e) {
						throw new Exception("Error reading animation: " + entry.Key, e);
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

		private Attachment ReadAttachment (Dictionary<String, Object> map, Skin skin, int slotIndex, String name, SkeletonData skeletonData) {
			var scale = this.Scale;
			name = GetString(map, "name", name);

			var typeName = GetString(map, "type", "region");
			if (typeName == "skinnedmesh") typeName = "weightedmesh";
			if (typeName == "weightedmesh") typeName = "mesh";
			if (typeName == "weightedlinkedmesh") typeName = "linkedmesh";
			var type = (AttachmentType)Enum.Parse(typeof(AttachmentType), typeName, true);

			String path = GetString(map, "path", name);

			switch (type) {
			case AttachmentType.Region:
				RegionAttachment region = attachmentLoader.NewRegionAttachment(skin, name, path);
				if (region == null) return null;
				region.Path = path;
				region.x = GetFloat(map, "x", 0) * scale;
				region.y = GetFloat(map, "y", 0) * scale;
				region.scaleX = GetFloat(map, "scaleX", 1);
				region.scaleY = GetFloat(map, "scaleY", 1);
				region.rotation = GetFloat(map, "rotation", 0);
				region.width = GetFloat(map, "width", 32) * scale;
				region.height = GetFloat(map, "height", 32) * scale;
				region.UpdateOffset();

				if (map.ContainsKey("color")) {
					var color = (String)map["color"];
					region.r = ToColor(color, 0);
					region.g = ToColor(color, 1);
					region.b = ToColor(color, 2);
					region.a = ToColor(color, 3);
				}

				region.UpdateOffset();
				return region;
			case AttachmentType.Boundingbox:
				BoundingBoxAttachment box = attachmentLoader.NewBoundingBoxAttachment(skin, name);
				if (box == null) return null;
				ReadVertices(map, box, GetInt(map, "vertexCount", 0) << 1);
				return box;
			case AttachmentType.Mesh:
			case AttachmentType.Linkedmesh: {
					MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, name, path);
					if (mesh == null) return null;
					mesh.Path = path;

					if (map.ContainsKey("color")) {
						var color = (String)map["color"];
						mesh.r = ToColor(color, 0);
						mesh.g = ToColor(color, 1);
						mesh.b = ToColor(color, 2);
						mesh.a = ToColor(color, 3);
					}

					mesh.Width = GetFloat(map, "width", 0) * scale;
					mesh.Height = GetFloat(map, "height", 0) * scale;

					String parent = GetString(map, "parent", null);
					if (parent != null) {
						mesh.InheritDeform = GetBoolean(map, "deform", true);
						linkedMeshes.Add(new LinkedMesh(mesh, GetString(map, "skin", null), slotIndex, parent));
						return mesh;
					}

					float[] uvs = GetFloatArray(map, "uvs", 1);
					ReadVertices(map, mesh, uvs.Length);
					mesh.triangles = GetIntArray(map, "triangles");
					mesh.regionUVs = uvs;
					mesh.UpdateUVs();

					if (map.ContainsKey("hull")) mesh.HullLength = GetInt(map, "hull", 0) * 2;
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

		private void ReadVertices (Dictionary<String, Object> map, VertexAttachment attachment, int verticesLength) {
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
				for (int nn = i + boneCount * 4; i < nn; i += 4) {
					bones.Add((int)vertices[i]);
					weights.Add(vertices[i + 1] * this.Scale);
					weights.Add(vertices[i + 2] * this.Scale);
					weights.Add(vertices[i + 3]);
				}
			}
			attachment.bones = bones.ToArray();
			attachment.vertices = weights.ToArray();
		}

		private void ReadAnimation (Dictionary<String, Object> map, String name, SkeletonData skeletonData) {
			var scale = this.Scale;
			var timelines = new ExposedList<Timeline>();
			float duration = 0;

			// Slot timelines.
			if (map.ContainsKey("slots")) {
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)map["slots"]) {
					String slotName = entry.Key;
					int slotIndex = skeletonData.FindSlotIndex(slotName);
					var timelineMap = (Dictionary<String, Object>)entry.Value;
					foreach (KeyValuePair<String, Object> timelineEntry in timelineMap) {
						var values = (List<Object>)timelineEntry.Value;
						var timelineName = (String)timelineEntry.Key;
						if (timelineName == "attachment") {
							var timeline = new AttachmentTimeline(values.Count);
							timeline.slotIndex = slotIndex;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								timeline.SetFrame(frameIndex++, time, (String)valueMap["name"]);
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[timeline.FrameCount - 1]);

						} else if (timelineName == "color") {
							var timeline = new ColorTimeline(values.Count);
							timeline.slotIndex = slotIndex;

							int frameIndex = 0;
							foreach (Dictionary<string, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								string c = (string)valueMap["color"];
								timeline.SetFrame(frameIndex, time, ToColor(c, 0), ToColor(c, 1), ToColor(c, 2), ToColor(c, 3));
								ReadCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[(timeline.FrameCount - 1) * ColorTimeline.ENTRIES]);

						} else if (timelineName == "twoColor") {
							var timeline = new TwoColorTimeline(values.Count);
							timeline.slotIndex = slotIndex;

							int frameIndex = 0;
							foreach (Dictionary<string, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								string light = (string)valueMap["light"];
								string dark = (string)valueMap["dark"];
								timeline.SetFrame(frameIndex, time, ToColor(light, 0), ToColor(light, 1), ToColor(light, 2), ToColor(light, 3),
									ToColor(dark, 0, 6), ToColor(dark, 1, 6), ToColor(dark, 2, 6));
								ReadCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[(timeline.FrameCount - 1) * TwoColorTimeline.ENTRIES]);

						} else
							throw new Exception("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
					}
				}
			}

			// Bone timelines.
			if (map.ContainsKey("bones")) {
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)map["bones"]) {
					String boneName = entry.Key;
					int boneIndex = skeletonData.FindBoneIndex(boneName);
					if (boneIndex == -1) throw new Exception("Bone not found: " + boneName);
					var timelineMap = (Dictionary<String, Object>)entry.Value;
					foreach (KeyValuePair<String, Object> timelineEntry in timelineMap) {
						var values = (List<Object>)timelineEntry.Value;
						var timelineName = (String)timelineEntry.Key;
						if (timelineName == "rotate") {
							var timeline = new RotateTimeline(values.Count);
							timeline.boneIndex = boneIndex;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								timeline.SetFrame(frameIndex, (float)valueMap["time"], (float)valueMap["angle"]);
								ReadCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[(timeline.FrameCount - 1) * RotateTimeline.ENTRIES]);

						} else if (timelineName == "translate" || timelineName == "scale" || timelineName == "shear") {
							TranslateTimeline timeline;
							float timelineScale = 1;
							if (timelineName == "scale")
								timeline = new ScaleTimeline(values.Count);
							else if (timelineName == "shear")
								timeline = new ShearTimeline(values.Count);
							else {
								timeline = new TranslateTimeline(values.Count);
								timelineScale = scale;
							}
							timeline.boneIndex = boneIndex;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								float x = GetFloat(valueMap, "x", 0);
								float y = GetFloat(valueMap, "y", 0);
								timeline.SetFrame(frameIndex, time, x * timelineScale, y * timelineScale);
								ReadCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[(timeline.FrameCount - 1) * TranslateTimeline.ENTRIES]);

						} else
							throw new Exception("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
					}
				}
			}

			// IK constraint timelines.
			if (map.ContainsKey("ik")) {
				foreach (KeyValuePair<String, Object> constraintMap in (Dictionary<String, Object>)map["ik"]) {
					IkConstraintData constraint = skeletonData.FindIkConstraint(constraintMap.Key);
					var values = (List<Object>)constraintMap.Value;
					var timeline = new IkConstraintTimeline(values.Count);
					timeline.ikConstraintIndex = skeletonData.ikConstraints.IndexOf(constraint);
					int frameIndex = 0;
					foreach (Dictionary<String, Object> valueMap in values) {
						float time = (float)valueMap["time"];
						float mix = GetFloat(valueMap, "mix", 1);
						bool bendPositive = GetBoolean(valueMap, "bendPositive", true);
						timeline.SetFrame(frameIndex, time, mix, bendPositive ? 1 : -1);
						ReadCurve(valueMap, timeline, frameIndex);
						frameIndex++;
					}
					timelines.Add(timeline);
					duration = Math.Max(duration, timeline.frames[(timeline.FrameCount - 1) * IkConstraintTimeline.ENTRIES]);
				}
			}

			// Transform constraint timelines.
			if (map.ContainsKey("transform")) {
				foreach (KeyValuePair<String, Object> constraintMap in (Dictionary<String, Object>)map["transform"]) {
					TransformConstraintData constraint = skeletonData.FindTransformConstraint(constraintMap.Key);
					var values = (List<Object>)constraintMap.Value;
					var timeline = new TransformConstraintTimeline(values.Count);
					timeline.transformConstraintIndex = skeletonData.transformConstraints.IndexOf(constraint);
					int frameIndex = 0;
					foreach (Dictionary<String, Object> valueMap in values) {
						float time = (float)valueMap["time"];
						float rotateMix = GetFloat(valueMap, "rotateMix", 1);
						float translateMix = GetFloat(valueMap, "translateMix", 1);
						float scaleMix = GetFloat(valueMap, "scaleMix", 1);
						float shearMix = GetFloat(valueMap, "shearMix", 1);
						timeline.SetFrame(frameIndex, time, rotateMix, translateMix, scaleMix, shearMix);
						ReadCurve(valueMap, timeline, frameIndex);
						frameIndex++;
					}
					timelines.Add(timeline);
					duration = Math.Max(duration, timeline.frames[(timeline.FrameCount - 1) * TransformConstraintTimeline.ENTRIES]);
				}
			}

			// Path constraint timelines.
			if (map.ContainsKey("paths")) {
				foreach (KeyValuePair<String, Object> constraintMap in (Dictionary<String, Object>)map["paths"]) {
					int index = skeletonData.FindPathConstraintIndex(constraintMap.Key);
					if (index == -1) throw new Exception("Path constraint not found: " + constraintMap.Key);
					PathConstraintData data = skeletonData.pathConstraints.Items[index];
					var timelineMap = (Dictionary<String, Object>)constraintMap.Value;
					foreach (KeyValuePair<String, Object> timelineEntry in timelineMap) {
						var values = (List<Object>)timelineEntry.Value;
						var timelineName = (String)timelineEntry.Key;
						if (timelineName == "position" || timelineName == "spacing") {
							PathConstraintPositionTimeline timeline;
							float timelineScale = 1;
							if (timelineName == "spacing") {
								timeline = new PathConstraintSpacingTimeline(values.Count);
								if (data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed) timelineScale = scale;
							}
							else {
								timeline = new PathConstraintPositionTimeline(values.Count);
								if (data.positionMode == PositionMode.Fixed) timelineScale = scale;
							}
							timeline.pathConstraintIndex = index;
							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								timeline.SetFrame(frameIndex, (float)valueMap["time"], GetFloat(valueMap, timelineName, 0) * timelineScale);
								ReadCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[(timeline.FrameCount - 1) * PathConstraintPositionTimeline.ENTRIES]);
						}
						else if (timelineName == "mix") {
							PathConstraintMixTimeline timeline = new PathConstraintMixTimeline(values.Count);
							timeline.pathConstraintIndex = index;
							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								timeline.SetFrame(frameIndex, (float)valueMap["time"], GetFloat(valueMap, "rotateMix", 1), GetFloat(valueMap, "translateMix", 1));
								ReadCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[(timeline.FrameCount - 1) * PathConstraintMixTimeline.ENTRIES]);
						}
					}
				}
			}

			// Deform timelines.
			if (map.ContainsKey("deform")) {
				foreach (KeyValuePair<String, Object> deformMap in (Dictionary<String, Object>)map["deform"]) {
					Skin skin = skeletonData.FindSkin(deformMap.Key);
					foreach (KeyValuePair<String, Object> slotMap in (Dictionary<String, Object>)deformMap.Value) {
						int slotIndex = skeletonData.FindSlotIndex(slotMap.Key);
						if (slotIndex == -1) throw new Exception("Slot not found: " + slotMap.Key);
						foreach (KeyValuePair<String, Object> timelineMap in (Dictionary<String, Object>)slotMap.Value) {
							var values = (List<Object>)timelineMap.Value;
							VertexAttachment attachment = (VertexAttachment)skin.GetAttachment(slotIndex, timelineMap.Key);
							if (attachment == null) throw new Exception("Deform attachment not found: " + timelineMap.Key);
							bool weighted = attachment.bones != null;
							float[] vertices = attachment.vertices;
							int deformLength = weighted ? vertices.Length / 3 * 2 : vertices.Length;

							var timeline = new DeformTimeline(values.Count);
							timeline.slotIndex = slotIndex;
							timeline.attachment = attachment;
							
							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float[] deform;
								if (!valueMap.ContainsKey("vertices")) {
									deform = weighted ? new float[deformLength] : vertices;
								} else {
									deform = new float[deformLength];
									int start = GetInt(valueMap, "offset", 0);
									float[] verticesValue = GetFloatArray(valueMap, "vertices", 1);
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

								timeline.SetFrame(frameIndex, (float)valueMap["time"], deform);
								ReadCurve(valueMap, timeline, frameIndex);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[timeline.FrameCount - 1]);
						}
					}
				}
			}

			// Draw order timeline.
			if (map.ContainsKey("drawOrder") || map.ContainsKey("draworder")) {
				var values = (List<Object>)map[map.ContainsKey("drawOrder") ? "drawOrder" : "draworder"];
				var timeline = new DrawOrderTimeline(values.Count);
				int slotCount = skeletonData.slots.Count;
				int frameIndex = 0;
				foreach (Dictionary<String, Object> drawOrderMap in values) {
					int[] drawOrder = null;
					if (drawOrderMap.ContainsKey("offsets")) {
						drawOrder = new int[slotCount];
						for (int i = slotCount - 1; i >= 0; i--)
							drawOrder[i] = -1;
						var offsets = (List<Object>)drawOrderMap["offsets"];
						int[] unchanged = new int[slotCount - offsets.Count];
						int originalIndex = 0, unchangedIndex = 0;
						foreach (Dictionary<String, Object> offsetMap in offsets) {
							int slotIndex = skeletonData.FindSlotIndex((String)offsetMap["slot"]);
							if (slotIndex == -1) throw new Exception("Slot not found: " + offsetMap["slot"]);
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
					timeline.SetFrame(frameIndex++, (float)drawOrderMap["time"], drawOrder);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[timeline.FrameCount - 1]);
			}

			// Event timeline.
			if (map.ContainsKey("events")) {
				var eventsMap = (List<Object>)map["events"];
				var timeline = new EventTimeline(eventsMap.Count);
				int frameIndex = 0;
				foreach (Dictionary<String, Object> eventMap in eventsMap) {
					EventData eventData = skeletonData.FindEvent((String)eventMap["name"]);
					if (eventData == null) throw new Exception("Event not found: " + eventMap["name"]);
					var e = new Event((float)eventMap["time"], eventData);
					e.Int = GetInt(eventMap, "int", eventData.Int);
					e.Float = GetFloat(eventMap, "float", eventData.Float);
					e.String = GetString(eventMap, "string", eventData.String);
					timeline.SetFrame(frameIndex++, e);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[timeline.FrameCount - 1]);
			}

			timelines.TrimExcess();
			skeletonData.animations.Add(new Animation(name, timelines, duration));
		}

		static void ReadCurve (Dictionary<String, Object> valueMap, CurveTimeline timeline, int frameIndex) {
			if (!valueMap.ContainsKey("curve"))
				return;
			Object curveObject = valueMap["curve"];
			if (curveObject.Equals("stepped"))
				timeline.SetStepped(frameIndex);
			else {
				var curve = curveObject as List<Object>;
				if (curve != null)
					timeline.SetCurve(frameIndex, (float)curve[0], (float)curve[1], (float)curve[2], (float)curve[3]);
			}
		}

		internal class LinkedMesh {
			internal String parent, skin;
			internal int slotIndex;
			internal MeshAttachment mesh;

			public LinkedMesh (MeshAttachment mesh, String skin, int slotIndex, String parent) {
				this.mesh = mesh;
				this.skin = skin;
				this.slotIndex = slotIndex;
				this.parent = parent;
			}
		}

		static float[] GetFloatArray(Dictionary<String, Object> map, String name, float scale) {
			var list = (List<Object>)map[name];
			var values = new float[list.Count];
			if (scale == 1) {
				for (int i = 0, n = list.Count; i < n; i++)
					values[i] = (float)list[i];
			} else {
				for (int i = 0, n = list.Count; i < n; i++)
					values[i] = (float)list[i] * scale;
			}
			return values;
		}

		static int[] GetIntArray(Dictionary<String, Object> map, String name) {
			var list = (List<Object>)map[name];
			var values = new int[list.Count];
			for (int i = 0, n = list.Count; i < n; i++)
				values[i] = (int)(float)list[i];
			return values;
		}

		static float GetFloat(Dictionary<String, Object> map, String name, float defaultValue) {
			if (!map.ContainsKey(name))
				return defaultValue;
			return (float)map[name];
		}

		static int GetInt(Dictionary<String, Object> map, String name, int defaultValue) {
			if (!map.ContainsKey(name))
				return defaultValue;
			return (int)(float)map[name];
		}

		static bool GetBoolean(Dictionary<String, Object> map, String name, bool defaultValue) {
			if (!map.ContainsKey(name))
				return defaultValue;
			return (bool)map[name];
		}

		static String GetString(Dictionary<String, Object> map, String name, String defaultValue) {
			if (!map.ContainsKey(name))
				return defaultValue;
			return (String)map[name];
		}

		static float ToColor(String hexString, int colorIndex, int expectedLength = 8) {
			if (hexString.Length != expectedLength)
				throw new ArgumentException("Color hexidecimal length must be " + expectedLength + ", recieved: " + hexString, "hexString");
			return Convert.ToInt32(hexString.Substring(colorIndex * 2, 2), 16) / (float)255;
		}
	}
}
