/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;

#if WINDOWS_STOREAPP
using System.Threading.Tasks;
using Windows.Storage;
#endif

namespace Spine {
	public class SkeletonJson {
		private AttachmentLoader attachmentLoader;
		public float Scale { get; set; }

		public SkeletonJson (params Atlas[] atlasArray)
			: this(new AtlasAttachmentLoader(atlasArray)) {
		}

		public SkeletonJson (AttachmentLoader attachmentLoader) {
			if (attachmentLoader == null) throw new ArgumentNullException("attachmentLoader cannot be null.");
			this.attachmentLoader = attachmentLoader;
			Scale = 1;
		}

#if WINDOWS_STOREAPP
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
			Stream stream = Microsoft.Xna.Framework.TitleContainer.OpenStream(path);
			using (StreamReader reader = new StreamReader(stream)) {
#else
			using (StreamReader reader = new StreamReader(path)) {
#endif
				SkeletonData skeletonData = ReadSkeletonData(reader);
				skeletonData.name = Path.GetFileNameWithoutExtension(path);
				return skeletonData;
			}
		}
#endif

		public SkeletonData ReadSkeletonData (TextReader reader) {
			if (reader == null) throw new ArgumentNullException("reader cannot be null.");

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
			}

			// Bones.
			foreach (Dictionary<String, Object> boneMap in (List<Object>)root["bones"]) {
				BoneData parent = null;
				if (boneMap.ContainsKey("parent")) {
					parent = skeletonData.FindBone((String)boneMap["parent"]);
					if (parent == null)
						throw new Exception("Parent bone not found: " + boneMap["parent"]);
				}
				var boneData = new BoneData((String)boneMap["name"], parent);
				boneData.length = GetFloat(boneMap, "length", 0) * Scale;
				boneData.x = GetFloat(boneMap, "x", 0) * Scale;
				boneData.y = GetFloat(boneMap, "y", 0) * Scale;
				boneData.rotation = GetFloat(boneMap, "rotation", 0);
				boneData.scaleX = GetFloat(boneMap, "scaleX", 1);
				boneData.scaleY = GetFloat(boneMap, "scaleY", 1);
				boneData.flipX = GetBoolean(boneMap, "flipX", false);
				boneData.flipY = GetBoolean(boneMap, "flipY", false);
				boneData.inheritScale = GetBoolean(boneMap, "inheritScale", true);
				boneData.inheritRotation = GetBoolean(boneMap, "inheritRotation", true);
				skeletonData.bones.Add(boneData);
			}

			// IK constraints.
			if (root.ContainsKey("ik")) {
				foreach (Dictionary<String, Object> ikMap in (List<Object>)root["ik"]) {
					IkConstraintData ikConstraintData = new IkConstraintData((String)ikMap["name"]);

					foreach (String boneName in (List<Object>)ikMap["bones"]) {
						BoneData bone = skeletonData.FindBone(boneName);
						if (bone == null) throw new Exception("IK bone not found: " + boneName);
						ikConstraintData.bones.Add(bone);
					}

					String targetName = (String)ikMap["target"];
					ikConstraintData.target = skeletonData.FindBone(targetName);
					if (ikConstraintData.target == null) throw new Exception("Target bone not found: " + targetName);

					ikConstraintData.bendDirection = GetBoolean(ikMap, "bendPositive", true) ? 1 : -1;
					ikConstraintData.mix = GetFloat(ikMap, "mix", 1);

					skeletonData.ikConstraints.Add(ikConstraintData);
				}
			}

			// Slots.
			if (root.ContainsKey("slots")) {
				foreach (Dictionary<String, Object> slotMap in (List<Object>)root["slots"]) {
					var slotName = (String)slotMap["name"];
					var boneName = (String)slotMap["bone"];
					BoneData boneData = skeletonData.FindBone(boneName);
					if (boneData == null)
						throw new Exception("Slot bone not found: " + boneName);
					var slotData = new SlotData(slotName, boneData);

					if (slotMap.ContainsKey("color")) {
						var color = (String)slotMap["color"];
						slotData.r = ToColor(color, 0);
						slotData.g = ToColor(color, 1);
						slotData.b = ToColor(color, 2);
						slotData.a = ToColor(color, 3);
					}

					if (slotMap.ContainsKey("attachment"))
						slotData.attachmentName = (String)slotMap["attachment"];

					if (slotMap.ContainsKey("blend"))
						slotData.blendMode = (BlendMode)Enum.Parse(typeof(BlendMode), (String)slotMap["blend"], false);
					else
						slotData.blendMode = BlendMode.normal;

					skeletonData.slots.Add(slotData);
				}
			}

			// Skins.
			if (root.ContainsKey("skins")) {
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)root["skins"]) {
					var skin = new Skin(entry.Key);
					foreach (KeyValuePair<String, Object> slotEntry in (Dictionary<String, Object>)entry.Value) {
						int slotIndex = skeletonData.FindSlotIndex(slotEntry.Key);
						foreach (KeyValuePair<String, Object> attachmentEntry in ((Dictionary<String, Object>)slotEntry.Value)) {
							Attachment attachment = ReadAttachment(skin, attachmentEntry.Key, (Dictionary<String, Object>)attachmentEntry.Value);
							if (attachment != null) skin.AddAttachment(slotIndex, attachmentEntry.Key, attachment);
						}
					}
					skeletonData.skins.Add(skin);
					if (skin.name == "default")
						skeletonData.defaultSkin = skin;
				}
			}

			// Events.
			if (root.ContainsKey("events")) {
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)root["events"]) {
					var entryMap = (Dictionary<String, Object>)entry.Value;
					var eventData = new EventData(entry.Key);
					eventData.Int = GetInt(entryMap, "int", 0);
					eventData.Float = GetFloat(entryMap, "float", 0);
					eventData.String = GetString(entryMap, "string", null);
					skeletonData.events.Add(eventData);
				}
			}

			// Animations.
			if (root.ContainsKey("animations")) {
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)root["animations"])
					ReadAnimation(entry.Key, (Dictionary<String, Object>)entry.Value, skeletonData);
			}

			skeletonData.bones.TrimExcess();
			skeletonData.slots.TrimExcess();
			skeletonData.skins.TrimExcess();
			skeletonData.events.TrimExcess();
			skeletonData.animations.TrimExcess();
			skeletonData.ikConstraints.TrimExcess();
			return skeletonData;
		}

		private Attachment ReadAttachment (Skin skin, String name, Dictionary<String, Object> map) {
			if (map.ContainsKey("name"))
				name = (String)map["name"];

			var type = AttachmentType.region;
			if (map.ContainsKey("type"))
				type = (AttachmentType)Enum.Parse(typeof(AttachmentType), (String)map["type"], false);

			String path = name;
			if (map.ContainsKey("path"))
				path = (String)map["path"];

			switch (type) {
			case AttachmentType.region:
				RegionAttachment region = attachmentLoader.NewRegionAttachment(skin, name, path);
				if (region == null) return null;
				region.Path = path;
				region.x = GetFloat(map, "x", 0) * Scale;
				region.y = GetFloat(map, "y", 0) * Scale;
				region.scaleX = GetFloat(map, "scaleX", 1);
				region.scaleY = GetFloat(map, "scaleY", 1);
				region.rotation = GetFloat(map, "rotation", 0);
				region.width = GetFloat(map, "width", 32) * Scale;
				region.height = GetFloat(map, "height", 32) * Scale;
				region.UpdateOffset();

				if (map.ContainsKey("color")) {
					var color = (String)map["color"];
					region.r = ToColor(color, 0);
					region.g = ToColor(color, 1);
					region.b = ToColor(color, 2);
					region.a = ToColor(color, 3);
				}

				return region;
			case AttachmentType.mesh: {
					MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, name, path);
					if (mesh == null) return null;

					mesh.Path = path; 
					mesh.vertices = GetFloatArray(map, "vertices", Scale);
					mesh.triangles = GetIntArray(map, "triangles");
					mesh.regionUVs = GetFloatArray(map, "uvs", 1);
					mesh.UpdateUVs();

					if (map.ContainsKey("color")) {
						var color = (String)map["color"];
						mesh.r = ToColor(color, 0);
						mesh.g = ToColor(color, 1);
						mesh.b = ToColor(color, 2);
						mesh.a = ToColor(color, 3);
					}

					mesh.HullLength = GetInt(map, "hull", 0) * 2;
					if (map.ContainsKey("edges")) mesh.Edges = GetIntArray(map, "edges");
					mesh.Width = GetInt(map, "width", 0) * Scale;
					mesh.Height = GetInt(map, "height", 0) * Scale;

					return mesh;
				}
			case AttachmentType.skinnedmesh: {
					SkinnedMeshAttachment mesh = attachmentLoader.NewSkinnedMeshAttachment(skin, name, path);
					if (mesh == null) return null;

					mesh.Path = path;
					float[] uvs = GetFloatArray(map, "uvs", 1);
					float[] vertices = GetFloatArray(map, "vertices", 1);
					var weights = new List<float>(uvs.Length * 3 * 3);
					var bones = new List<int>(uvs.Length * 3);
					float scale = Scale;
					for (int i = 0, n = vertices.Length; i < n; ) {
						int boneCount = (int)vertices[i++];
						bones.Add(boneCount);
						for (int nn = i + boneCount * 4; i < nn; ) {
							bones.Add((int)vertices[i]);
							weights.Add(vertices[i + 1] * scale);
							weights.Add(vertices[i + 2] * scale);
							weights.Add(vertices[i + 3]);
							i += 4;
						}
					}
					mesh.bones = bones.ToArray();
					mesh.weights = weights.ToArray();
					mesh.triangles = GetIntArray(map, "triangles");
					mesh.regionUVs = uvs;
					mesh.UpdateUVs();

					if (map.ContainsKey("color")) {
						var color = (String)map["color"];
						mesh.r = ToColor(color, 0);
						mesh.g = ToColor(color, 1);
						mesh.b = ToColor(color, 2);
						mesh.a = ToColor(color, 3);
					}

					mesh.HullLength = GetInt(map, "hull", 0) * 2;
					if (map.ContainsKey("edges")) mesh.Edges = GetIntArray(map, "edges");
					mesh.Width = GetInt(map, "width", 0) * Scale;
					mesh.Height = GetInt(map, "height", 0) * Scale;

					return mesh;
				}
			case AttachmentType.boundingbox:
				BoundingBoxAttachment box = attachmentLoader.NewBoundingBoxAttachment(skin, name);
				if (box == null) return null;
				box.vertices = GetFloatArray(map, "vertices", Scale);
				return box;
			}
			return null;
		}

		private float[] GetFloatArray (Dictionary<String, Object> map, String name, float scale) {
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

		private int[] GetIntArray (Dictionary<String, Object> map, String name) {
			var list = (List<Object>)map[name];
			var values = new int[list.Count];
			for (int i = 0, n = list.Count; i < n; i++)
				values[i] = (int)(float)list[i];
			return values;
		}

		private float GetFloat (Dictionary<String, Object> map, String name, float defaultValue) {
			if (!map.ContainsKey(name))
				return defaultValue;
			return (float)map[name];
		}

		private int GetInt (Dictionary<String, Object> map, String name, int defaultValue) {
			if (!map.ContainsKey(name))
				return defaultValue;
			return (int)(float)map[name];
		}

		private bool GetBoolean (Dictionary<String, Object> map, String name, bool defaultValue) {
			if (!map.ContainsKey(name))
				return defaultValue;
			return (bool)map[name];
		}

		private String GetString (Dictionary<String, Object> map, String name, String defaultValue) {
			if (!map.ContainsKey(name))
				return defaultValue;
			return (String)map[name];
		}

		private float ToColor (String hexString, int colorIndex) {
			if (hexString.Length != 8)
				throw new ArgumentException("Color hexidecimal length must be 8, recieved: " + hexString);
			return Convert.ToInt32(hexString.Substring(colorIndex * 2, 2), 16) / (float)255;
		}

		private void ReadAnimation (String name, Dictionary<String, Object> map, SkeletonData skeletonData) {
			var timelines = new ExposedList<Timeline>();
			float duration = 0;
			float scale = Scale;

			if (map.ContainsKey("slots")) {
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)map["slots"]) {
					String slotName = entry.Key;
					int slotIndex = skeletonData.FindSlotIndex(slotName);
					var timelineMap = (Dictionary<String, Object>)entry.Value;

					foreach (KeyValuePair<String, Object> timelineEntry in timelineMap) {
						var values = (List<Object>)timelineEntry.Value;
						var timelineName = (String)timelineEntry.Key;
						if (timelineName == "color") {
							var timeline = new ColorTimeline(values.Count);
							timeline.slotIndex = slotIndex;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								String c = (String)valueMap["color"];
								timeline.SetFrame(frameIndex, time, ToColor(c, 0), ToColor(c, 1), ToColor(c, 2), ToColor(c, 3));
								ReadCurve(timeline, frameIndex, valueMap);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[timeline.FrameCount * 5 - 5]);

						} else if (timelineName == "attachment") {
							var timeline = new AttachmentTimeline(values.Count);
							timeline.slotIndex = slotIndex;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								timeline.SetFrame(frameIndex++, time, (String)valueMap["name"]);
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[timeline.FrameCount - 1]);

						} else
							throw new Exception("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
					}
				}
			}

			if (map.ContainsKey("bones")) {
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)map["bones"]) {
					String boneName = entry.Key;
					int boneIndex = skeletonData.FindBoneIndex(boneName);
					if (boneIndex == -1)
						throw new Exception("Bone not found: " + boneName);

					var timelineMap = (Dictionary<String, Object>)entry.Value;
					foreach (KeyValuePair<String, Object> timelineEntry in timelineMap) {
						var values = (List<Object>)timelineEntry.Value;
						var timelineName = (String)timelineEntry.Key;
						if (timelineName == "rotate") {
							var timeline = new RotateTimeline(values.Count);
							timeline.boneIndex = boneIndex;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								timeline.SetFrame(frameIndex, time, (float)valueMap["angle"]);
								ReadCurve(timeline, frameIndex, valueMap);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[timeline.FrameCount * 2 - 2]);

						} else if (timelineName == "translate" || timelineName == "scale") {
							TranslateTimeline timeline;
							float timelineScale = 1;
							if (timelineName == "scale")
								timeline = new ScaleTimeline(values.Count);
							else {
								timeline = new TranslateTimeline(values.Count);
								timelineScale = scale;
							}
							timeline.boneIndex = boneIndex;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								float x = valueMap.ContainsKey("x") ? (float)valueMap["x"] : 0;
								float y = valueMap.ContainsKey("y") ? (float)valueMap["y"] : 0;
								timeline.SetFrame(frameIndex, time, (float)x * timelineScale, (float)y * timelineScale);
								ReadCurve(timeline, frameIndex, valueMap);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[timeline.FrameCount * 3 - 3]);

						} else if (timelineName == "flipX" || timelineName == "flipY") {
							bool x = timelineName == "flipX";
							var timeline = x ? new FlipXTimeline(values.Count) : new FlipYTimeline(values.Count);
							timeline.boneIndex = boneIndex;

							String field = x ? "x" : "y";
							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								timeline.SetFrame(frameIndex, time, valueMap.ContainsKey(field) ? (bool)valueMap[field] : false);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[timeline.FrameCount * 2 - 2]);

						} else
							throw new Exception("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
					}
				}
			}

			if (map.ContainsKey("ik")) {
				foreach (KeyValuePair<String, Object> ikMap in (Dictionary<String, Object>)map["ik"]) {
					IkConstraintData ikConstraint = skeletonData.FindIkConstraint(ikMap.Key);
					var values = (List<Object>)ikMap.Value;
					var timeline = new IkConstraintTimeline(values.Count);
					timeline.ikConstraintIndex = skeletonData.ikConstraints.IndexOf(ikConstraint);
					int frameIndex = 0;
					foreach (Dictionary<String, Object> valueMap in values) {
						float time = (float)valueMap["time"];
						float mix = valueMap.ContainsKey("mix") ? (float)valueMap["mix"] : 1;
						bool bendPositive = valueMap.ContainsKey("bendPositive") ? (bool)valueMap["bendPositive"] : true;
						timeline.SetFrame(frameIndex, time, mix, bendPositive ? 1 : -1);
						ReadCurve(timeline, frameIndex, valueMap);
						frameIndex++;
					}
					timelines.Add(timeline);
					duration = Math.Max(duration, timeline.frames[timeline.FrameCount * 3 - 3]);
				}
			}

			if (map.ContainsKey("ffd")) {
				foreach (KeyValuePair<String, Object> ffdMap in (Dictionary<String, Object>)map["ffd"]) {
					Skin skin = skeletonData.FindSkin(ffdMap.Key);
					foreach (KeyValuePair<String, Object> slotMap in (Dictionary<String, Object>)ffdMap.Value) {
						int slotIndex = skeletonData.FindSlotIndex(slotMap.Key);
						foreach (KeyValuePair<String, Object> meshMap in (Dictionary<String, Object>)slotMap.Value) {
							var values = (List<Object>)meshMap.Value;
							var timeline = new FFDTimeline(values.Count);
							Attachment attachment = skin.GetAttachment(slotIndex, meshMap.Key);
							if (attachment == null) throw new Exception("FFD attachment not found: " + meshMap.Key);
							timeline.slotIndex = slotIndex;
							timeline.attachment = attachment;

							int vertexCount;
							if (attachment is MeshAttachment)
								vertexCount = ((MeshAttachment)attachment).vertices.Length;
							else
								vertexCount = ((SkinnedMeshAttachment)attachment).Weights.Length / 3 * 2;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float[] vertices;
								if (!valueMap.ContainsKey("vertices")) {
									if (attachment is MeshAttachment)
										vertices = ((MeshAttachment)attachment).vertices;
									else
										vertices = new float[vertexCount];
								} else {
									var verticesValue = (List<Object>)valueMap["vertices"];
									vertices = new float[vertexCount];
									int start = GetInt(valueMap, "offset", 0);
									if (scale == 1) {
										for (int i = 0, n = verticesValue.Count; i < n; i++)
											vertices[i + start] = (float)verticesValue[i];
									} else {
										for (int i = 0, n = verticesValue.Count; i < n; i++)
											vertices[i + start] = (float)verticesValue[i] * scale;
									}
									if (attachment is MeshAttachment) {
										float[] meshVertices = ((MeshAttachment)attachment).vertices;
										for (int i = 0; i < vertexCount; i++)
											vertices[i] += meshVertices[i];
									}
								}

								timeline.SetFrame(frameIndex, (float)valueMap["time"], vertices);
								ReadCurve(timeline, frameIndex, valueMap);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[timeline.FrameCount - 1]);
						}
					}
				}
			}

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

			if (map.ContainsKey("events")) {
				var eventsMap = (List<Object>)map["events"];
				var timeline = new EventTimeline(eventsMap.Count);
				int frameIndex = 0;
				foreach (Dictionary<String, Object> eventMap in eventsMap) {
					EventData eventData = skeletonData.FindEvent((String)eventMap["name"]);
					if (eventData == null) throw new Exception("Event not found: " + eventMap["name"]);
					var e = new Event(eventData);
					e.Int = GetInt(eventMap, "int", eventData.Int);
					e.Float = GetFloat(eventMap, "float", eventData.Float);
					e.String = GetString(eventMap, "string", eventData.String);
					timeline.SetFrame(frameIndex++, (float)eventMap["time"], e);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[timeline.FrameCount - 1]);
			}

			timelines.TrimExcess();
			skeletonData.animations.Add(new Animation(name, timelines, duration));
		}

		private void ReadCurve (CurveTimeline timeline, int frameIndex, Dictionary<String, Object> valueMap) {
			if (!valueMap.ContainsKey("curve"))
				return;
			Object curveObject = valueMap["curve"];
			if (curveObject.Equals("stepped"))
				timeline.SetStepped(frameIndex);
			else if (curveObject is List<Object>) {
				var curve = (List<Object>)curveObject;
				timeline.SetCurve(frameIndex, (float)curve[0], (float)curve[1], (float)curve[2], (float)curve[3]);
			}
		}
	}
}
