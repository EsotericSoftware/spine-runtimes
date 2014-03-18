/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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

		public SkeletonJson (Atlas atlas)
			: this(new AtlasAttachmentLoader(atlas)) {
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
            using (StreamReader reader = new StreamReader(stream))
            {
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

			SkeletonData skeletonData = new SkeletonData();

			var root = Json.Deserialize(reader) as Dictionary<String, Object>;
			if (root == null) throw new Exception("Invalid JSON.");

			// Bones.
			foreach (Dictionary<String, Object> boneMap in (List<Object>)root["bones"]) {
				BoneData parent = null;
				if (boneMap.ContainsKey("parent")) {
					parent = skeletonData.FindBone((String)boneMap["parent"]);
					if (parent == null)
						throw new Exception("Parent bone not found: " + boneMap["parent"]);
				}
				BoneData boneData = new BoneData((String)boneMap["name"], parent);
				boneData.length = GetFloat(boneMap, "length", 0) * Scale;
				boneData.x = GetFloat(boneMap, "x", 0) * Scale;
				boneData.y = GetFloat(boneMap, "y", 0) * Scale;
				boneData.rotation = GetFloat(boneMap, "rotation", 0);
				boneData.scaleX = GetFloat(boneMap, "scaleX", 1);
				boneData.scaleY = GetFloat(boneMap, "scaleY", 1);
				boneData.inheritScale = GetBoolean(boneMap, "inheritScale", true);
				boneData.inheritRotation = GetBoolean(boneMap, "inheritRotation", true);
				skeletonData.AddBone(boneData);
			}

			// Slots.
			if (root.ContainsKey("slots")) {
				foreach (Dictionary<String, Object> slotMap in (List<Object>)root["slots"]) {
					String slotName = (String)slotMap["name"];
					String boneName = (String)slotMap["bone"];
					BoneData boneData = skeletonData.FindBone(boneName);
					if (boneData == null)
						throw new Exception("Slot bone not found: " + boneName);
					SlotData slotData = new SlotData(slotName, boneData);

					if (slotMap.ContainsKey("color")) {
						String color = (String)slotMap["color"];
						slotData.r = ToColor(color, 0);
						slotData.g = ToColor(color, 1);
						slotData.b = ToColor(color, 2);
						slotData.a = ToColor(color, 3);
					}

					if (slotMap.ContainsKey("attachment"))
						slotData.attachmentName = (String)slotMap["attachment"];

					if (slotMap.ContainsKey("additive"))
						slotData.additiveBlending = (bool)slotMap["additive"];

					skeletonData.AddSlot(slotData);
				}
			}

			// Skins.
			if (root.ContainsKey("skins")) {
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)root["skins"]) {
					Skin skin = new Skin(entry.Key);
					foreach (KeyValuePair<String, Object> slotEntry in (Dictionary<String, Object>)entry.Value) {
						int slotIndex = skeletonData.FindSlotIndex(slotEntry.Key);
						foreach (KeyValuePair<String, Object> attachmentEntry in ((Dictionary<String, Object>)slotEntry.Value)) {
							Attachment attachment = ReadAttachment(skin, attachmentEntry.Key, (Dictionary<String, Object>)attachmentEntry.Value);
							if (attachment != null) skin.AddAttachment(slotIndex, attachmentEntry.Key, attachment);
						}
					}
					skeletonData.AddSkin(skin);
					if (skin.name == "default")
						skeletonData.defaultSkin = skin;
				}
			}

			// Events.
			if (root.ContainsKey("events")) {
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)root["events"]) {
					var entryMap = (Dictionary<String, Object>)entry.Value;
					EventData eventData = new EventData(entry.Key);
					eventData.Int = GetInt(entryMap, "int", 0);
					eventData.Float = GetFloat(entryMap, "float", 0);
					eventData.String = GetString(entryMap, "string", null);
					skeletonData.AddEvent(eventData);
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
			skeletonData.animations.TrimExcess();
			return skeletonData;
		}

		private Attachment ReadAttachment (Skin skin, String name, Dictionary<String, Object> map) {
			if (map.ContainsKey("name"))
				name = (String)map["name"];

			AttachmentType type = AttachmentType.region;
			if (map.ContainsKey("type"))
				type = (AttachmentType)Enum.Parse(typeof(AttachmentType), (String)map["type"], false);
			Attachment attachment = attachmentLoader.NewAttachment(skin, type, name);

			RegionAttachment regionAttachment = attachment as RegionAttachment;
			if (regionAttachment != null) {
				regionAttachment.x = GetFloat(map, "x", 0) * Scale;
				regionAttachment.y = GetFloat(map, "y", 0) * Scale;
				regionAttachment.scaleX = GetFloat(map, "scaleX", 1);
				regionAttachment.scaleY = GetFloat(map, "scaleY", 1);
				regionAttachment.rotation = GetFloat(map, "rotation", 0);
				regionAttachment.width = GetFloat(map, "width", 32) * Scale;
				regionAttachment.height = GetFloat(map, "height", 32) * Scale;
				regionAttachment.UpdateOffset();
			}

			BoundingBoxAttachment boundingBox = attachment as BoundingBoxAttachment;
			if (boundingBox != null) {
				List<Object> values = (List<Object>)map["vertices"];
				float[] vertices = new float[values.Count];
				for (int i = 0, n = values.Count; i < n; i++)
					vertices[i] = (float)values[i] * Scale;
				boundingBox.Vertices = vertices;
			}

			return attachment;
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

		public static float ToColor (String hexString, int colorIndex) {
			if (hexString.Length != 8)
				throw new ArgumentException("Color hexidecimal length must be 8, recieved: " + hexString);
			return Convert.ToInt32(hexString.Substring(colorIndex * 2, 2), 16) / (float)255;
		}

		private void ReadAnimation (String name, Dictionary<String, Object> map, SkeletonData skeletonData) {
			var timelines = new List<Timeline>();
			float duration = 0;

			if (map.ContainsKey("bones")) {
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)map["bones"]) {
					String boneName = entry.Key;
					int boneIndex = skeletonData.FindBoneIndex(boneName);
					if (boneIndex == -1)
						throw new Exception("Bone not found: " + boneName);

					var timelineMap = (Dictionary<String, Object>)entry.Value;
					foreach (KeyValuePair<String, Object> timelineEntry in timelineMap) {
						var values = (List<Object>)timelineEntry.Value;
						String timelineName = (String)timelineEntry.Key;
						if (timelineName.Equals("rotate")) {
							RotateTimeline timeline = new RotateTimeline(values.Count);
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

						} else if (timelineName.Equals("translate") || timelineName.Equals("scale")) {
							TranslateTimeline timeline;
							float timelineScale = 1;
							if (timelineName.Equals("scale"))
								timeline = new ScaleTimeline(values.Count);
							else {
								timeline = new TranslateTimeline(values.Count);
								timelineScale = Scale;
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

						} else
							throw new Exception("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
					}
				}
			}

			if (map.ContainsKey("slots")) {
				foreach (KeyValuePair<String, Object> entry in (Dictionary<String, Object>)map["slots"]) {
					String slotName = entry.Key;
					int slotIndex = skeletonData.FindSlotIndex(slotName);
					var timelineMap = (Dictionary<String, Object>)entry.Value;

					foreach (KeyValuePair<String, Object> timelineEntry in timelineMap) {
						var values = (List<Object>)timelineEntry.Value;
						String timelineName = (String)timelineEntry.Key;
						if (timelineName.Equals("color")) {
							ColorTimeline timeline = new ColorTimeline(values.Count);
							timeline.slotIndex = slotIndex;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								String c = (String)valueMap["color"];
								timeline.setFrame(frameIndex, time, ToColor(c, 0), ToColor(c, 1), ToColor(c, 2), ToColor(c, 3));
								ReadCurve(timeline, frameIndex, valueMap);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[timeline.FrameCount * 5 - 5]);

						} else if (timelineName.Equals("attachment")) {
							AttachmentTimeline timeline = new AttachmentTimeline(values.Count);
							timeline.slotIndex = slotIndex;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								timeline.setFrame(frameIndex++, time, (String)valueMap["name"]);
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[timeline.FrameCount - 1]);

						} else
							throw new Exception("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
					}
				}
			}

			if (map.ContainsKey("events")) {
				var eventsMap = (List<Object>)map["events"];
				EventTimeline timeline = new EventTimeline(eventsMap.Count);
				int frameIndex = 0;
				foreach (Dictionary<String, Object> eventMap in eventsMap) {
					EventData eventData = skeletonData.FindEvent((String)eventMap["name"]);
					if (eventData == null) throw new Exception("Event not found: " + eventMap["name"]);
					Event e = new Event(eventData);
					e.Int = GetInt(eventMap, "int", eventData.Int);
					e.Float = GetFloat(eventMap, "float", eventData.Float);
					e.String = GetString(eventMap, "string", eventData.String);
					timeline.setFrame(frameIndex++, (float)eventMap["time"], e);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[timeline.FrameCount - 1]);
			}

			if (map.ContainsKey("draworder")) {
				var values = (List<Object>)map["draworder"];
				DrawOrderTimeline timeline = new DrawOrderTimeline(values.Count);
				int slotCount = skeletonData.slots.Count;
				int frameIndex = 0;
				foreach (Dictionary<String, Object> drawOrderMap in values) {
					int[] drawOrder = null;
					if (drawOrderMap.ContainsKey("offsets")) {
						drawOrder = new int[slotCount];
						for (int i = slotCount - 1; i >= 0; i--)
							drawOrder[i] = -1;
						List<Object> offsets = (List<Object>)drawOrderMap["offsets"];
						int[] unchanged = new int[slotCount - offsets.Count];
						int originalIndex = 0, unchangedIndex = 0;
						foreach (Dictionary<String, Object> offsetMap in offsets) {
							int slotIndex = skeletonData.FindSlotIndex((String)offsetMap["slot"]);
							if (slotIndex == -1) throw new Exception("Slot not found: " + offsetMap["slot"]);
							// Collect unchanged items.
							while (originalIndex != slotIndex)
								unchanged[unchangedIndex++] = originalIndex++;
							// Set changed items.
							drawOrder[originalIndex + (int)(float)offsetMap["offset"]] = originalIndex++;
						}
						// Collect remaining unchanged items.
						while (originalIndex < slotCount)
							unchanged[unchangedIndex++] = originalIndex++;
						// Fill in unchanged items.
						for (int i = slotCount - 1; i >= 0; i--)
							if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
					}
					timeline.setFrame(frameIndex++, (float)drawOrderMap["time"], drawOrder);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[timeline.FrameCount - 1]);
			}

			timelines.TrimExcess();
			skeletonData.AddAnimation(new Animation(name, timelines, duration));
		}

		private void ReadCurve (CurveTimeline timeline, int frameIndex, Dictionary<String, Object> valueMap) {
			if (!valueMap.ContainsKey("curve"))
				return;
			Object curveObject = valueMap["curve"];
			if (curveObject.Equals("stepped"))
				timeline.SetStepped(frameIndex);
			else if (curveObject is List<Object>) {
				List<Object> curve = (List<Object>)curveObject;
				timeline.SetCurve(frameIndex, (float)curve[0], (float)curve[1], (float)curve[2], (float)curve[3]);
			}
		}
	}
}
