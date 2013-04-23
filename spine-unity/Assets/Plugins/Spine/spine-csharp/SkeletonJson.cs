/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;

namespace Spine {
	public class SkeletonJson {
		static public String TIMELINE_SCALE = "scale";
		static public String TIMELINE_ROTATE = "rotate";
		static public String TIMELINE_TRANSLATE = "translate";
		static public String TIMELINE_ATTACHMENT = "attachment";
		static public String TIMELINE_COLOR = "color";

		static public String ATTACHMENT_REGION = "region";
		static public String ATTACHMENT_REGION_SEQUENCE = "regionSequence";

		private AttachmentLoader attachmentLoader;
		public float Scale { get; set; }

		public SkeletonJson (Atlas atlas) {
			this.attachmentLoader = new AtlasAttachmentLoader(atlas);
			Scale = 1;
		}

		public SkeletonJson (AttachmentLoader attachmentLoader) {
			if (attachmentLoader == null) throw new ArgumentNullException("attachmentLoader cannot be null.");
			this.attachmentLoader = attachmentLoader;
			Scale = 1;
		}

		public SkeletonData ReadSkeletonData (String path) {
			using (StreamReader reader = new StreamReader(path)) {
				SkeletonData skeletonData = ReadSkeletonData(reader);
				skeletonData.Name = Path.GetFileNameWithoutExtension(path);
				return skeletonData;
			}
		}

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
				boneData.Length = GetFloat(boneMap, "length", 0) * Scale;
				boneData.X = GetFloat(boneMap, "x", 0) * Scale;
				boneData.Y = GetFloat(boneMap, "y", 0) * Scale;
				boneData.Rotation = GetFloat(boneMap, "rotation", 0);
				boneData.ScaleX = GetFloat(boneMap, "scaleX", 1);
				boneData.ScaleY = GetFloat(boneMap, "scaleY", 1);
				skeletonData.AddBone(boneData);
			}

			// Slots.
			if (root.ContainsKey("slots")) {
				var slots = (List<Object>)root["slots"];
				foreach (Dictionary<String, Object> slotMap in (List<Object>)slots) {
					String slotName = (String)slotMap["name"];
					String boneName = (String)slotMap["bone"];
					BoneData boneData = skeletonData.FindBone(boneName);
					if (boneData == null)
						throw new Exception("Slot bone not found: " + boneName);
					SlotData slotData = new SlotData(slotName, boneData);

					if (slotMap.ContainsKey("color")) {
						String color = (String)slotMap["color"];
						slotData.R = ToColor(color, 0);
						slotData.G = ToColor(color, 1);
						slotData.B = ToColor(color, 2);
						slotData.A = ToColor(color, 3);
					}

					if (slotMap.ContainsKey("attachment"))
						slotData.AttachmentName = (String)slotMap["attachment"];

					skeletonData.AddSlot(slotData);
				}
			}

			// Skins.
			if (root.ContainsKey("skins")) {
				var skinMap = (Dictionary<String, Object>)root["skins"];
				foreach (KeyValuePair<String, Object> entry in skinMap) {
					Skin skin = new Skin(entry.Key);
					foreach (KeyValuePair<String, Object> slotEntry in (Dictionary<String, Object>)entry.Value) {
						int slotIndex = skeletonData.FindSlotIndex(slotEntry.Key);
						foreach (KeyValuePair<String, Object> attachmentEntry in ((Dictionary<String, Object>)slotEntry.Value)) {
							Attachment attachment = ReadAttachment(skin, attachmentEntry.Key, (Dictionary<String, Object>)attachmentEntry.Value);
							skin.AddAttachment(slotIndex, attachmentEntry.Key, attachment);
						}
					}
					skeletonData.AddSkin(skin);
					if (skin.Name == "default")
						skeletonData.DefaultSkin = skin;
				}
			}


			// Animations.
			if (root.ContainsKey("animations")) {
				var animationMap = (Dictionary<String, Object>)root["animations"];
				foreach (KeyValuePair<String, Object> entry in animationMap)
					ReadAnimation(entry.Key, (Dictionary<String, Object>)entry.Value, skeletonData);
			}

			skeletonData.Bones.TrimExcess();
			skeletonData.Slots.TrimExcess();
			skeletonData.Skins.TrimExcess();
			skeletonData.Animations.TrimExcess();
			return skeletonData;
		}

		private Attachment ReadAttachment (Skin skin, String name, Dictionary<String, Object> map) {
			if (map.ContainsKey("name"))
				name = (String)map["name"];

			AttachmentType type = AttachmentType.region;
			if (map.ContainsKey("type"))
				type = (AttachmentType)Enum.Parse(typeof(AttachmentType), (String)map["type"], false);
			Attachment attachment = attachmentLoader.NewAttachment(skin, type, name);

			if (attachment is RegionAttachment) {
				RegionAttachment regionAttachment = (RegionAttachment)attachment;
				regionAttachment.X = GetFloat(map, "x", 0) * Scale;
				regionAttachment.Y = GetFloat(map, "y", 0) * Scale;
				regionAttachment.ScaleX = GetFloat(map, "scaleX", 1);
				regionAttachment.ScaleY = GetFloat(map, "scaleY", 1);
				regionAttachment.Rotation = GetFloat(map, "rotation", 0);
				regionAttachment.Width = GetFloat(map, "width", 32) * Scale;
				regionAttachment.Height = GetFloat(map, "height", 32) * Scale;
				regionAttachment.UpdateOffset();
			}

			return attachment;
		}

		private float GetFloat (Dictionary<String, Object> map, String name, float defaultValue) {
			if (!map.ContainsKey(name))
				return (float)defaultValue;
			return (float)map[name];
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
				var bonesMap = (Dictionary<String, Object>)map["bones"];
				foreach (KeyValuePair<String, Object> entry in bonesMap) {
					String boneName = entry.Key;
					int boneIndex = skeletonData.FindBoneIndex(boneName);
					if (boneIndex == -1)
						throw new Exception("Bone not found: " + boneName);

					var timelineMap = (Dictionary<String, Object>)entry.Value;
					foreach (KeyValuePair<String, Object> timelineEntry in timelineMap) {
						var values = (List<Object>)timelineEntry.Value;
						String timelineName = (String)timelineEntry.Key;
						if (timelineName.Equals(TIMELINE_ROTATE)) {
							RotateTimeline timeline = new RotateTimeline(values.Count);
							timeline.BoneIndex = boneIndex;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								timeline.SetFrame(frameIndex, time, (float)valueMap["angle"]);
								ReadCurve(timeline, frameIndex, valueMap);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.Frames[timeline.FrameCount * 2 - 2]);

						} else if (timelineName.Equals(TIMELINE_TRANSLATE) || timelineName.Equals(TIMELINE_SCALE)) {
							TranslateTimeline timeline;
							float timelineScale = 1;
							if (timelineName.Equals(TIMELINE_SCALE))
								timeline = new ScaleTimeline(values.Count);
							else {
								timeline = new TranslateTimeline(values.Count);
								timelineScale = Scale;
							}
							timeline.BoneIndex = boneIndex;

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
							duration = Math.Max(duration, timeline.Frames[timeline.FrameCount * 3 - 3]);

						} else
							throw new Exception("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
					}
				}
			}

			if (map.ContainsKey("slots")) {
				var slotsMap = (Dictionary<String, Object>)map["slots"];
				foreach (KeyValuePair<String, Object> entry in slotsMap) {
					String slotName = entry.Key;
					int slotIndex = skeletonData.FindSlotIndex(slotName);
					var timelineMap = (Dictionary<String, Object>)entry.Value;

					foreach (KeyValuePair<String, Object> timelineEntry in timelineMap) {
						var values = (List<Object>)timelineEntry.Value;
						String timelineName = (String)timelineEntry.Key;
						if (timelineName.Equals(TIMELINE_COLOR)) {
							ColorTimeline timeline = new ColorTimeline(values.Count);
							timeline.SlotIndex = slotIndex;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								String c = (String)valueMap["color"];
								timeline.setFrame(frameIndex, time, ToColor(c, 0), ToColor(c, 1), ToColor(c, 2), ToColor(c, 3));
								ReadCurve(timeline, frameIndex, valueMap);
								frameIndex++;
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.Frames[timeline.FrameCount * 5 - 5]);

						} else if (timelineName.Equals(TIMELINE_ATTACHMENT)) {
							AttachmentTimeline timeline = new AttachmentTimeline(values.Count);
							timeline.SlotIndex = slotIndex;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								timeline.setFrame(frameIndex++, time, (String)valueMap["name"]);
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);

						} else
							throw new Exception("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
					}
				}
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
