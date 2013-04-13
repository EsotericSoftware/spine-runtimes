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

		public SkeletonJson (BaseAtlas atlas) {
			this.attachmentLoader = new AtlasAttachmentLoader(atlas);
			Scale = 1;
		}

		public SkeletonJson (AttachmentLoader attachmentLoader) {
			this.attachmentLoader = attachmentLoader;
			Scale = 1;
		}

		public SkeletonData readSkeletonData (String name, String json) {
			if (json == null)
				throw new ArgumentNullException("json cannot be null.");

			SkeletonData skeletonData = new SkeletonData();
			skeletonData.Name = name;

			var root = Json.Deserialize(json) as Dictionary<String, Object>;

			// Bones.
			foreach (Dictionary<String, Object> boneMap in (List<Object>)root["bones"]) {
				BoneData parent = null;
				if (boneMap.ContainsKey("parent")) {
					parent = skeletonData.FindBone((String)boneMap["parent"]);
					if (parent == null)
						throw new Exception("Parent bone not found: " + boneMap["parent"]);
				}
				BoneData boneData = new BoneData((String)boneMap["name"], parent);
				boneData.Length = getFloat(boneMap, "length", 0) * Scale;
				boneData.X = getFloat(boneMap, "x", 0) * Scale;
				boneData.Y = getFloat(boneMap, "y", 0) * Scale;
				boneData.Rotation = getFloat(boneMap, "rotation", 0);
				boneData.ScaleX = getFloat(boneMap, "scaleX", 1);
				boneData.ScaleY = getFloat(boneMap, "scaleY", 1);
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
						slotData.R = toColor(color, 0);
						slotData.G = toColor(color, 1);
						slotData.B = toColor(color, 2);
						slotData.A = toColor(color, 3);
					}

					if (slotMap.ContainsKey("attachment"))
						slotData.AttachmentName = (String)slotMap["attachment"];

					skeletonData.AddSlot(slotData);
				}
			}

			// Skins.
			if (root.ContainsKey("skins")) {
				Dictionary<String, Object> skinMap = (Dictionary<String, Object>)root["skins"];
				foreach (KeyValuePair<String, Object> entry in skinMap) {
					Skin skin = new Skin(entry.Key);
					foreach (KeyValuePair<String, Object> slotEntry in (Dictionary<String, Object>)entry.Value) {
						int slotIndex = skeletonData.FindSlotIndex(slotEntry.Key);
						foreach (KeyValuePair<String, Object> attachmentEntry in ((Dictionary<String, Object>)slotEntry.Value)) {
							Attachment attachment = readAttachment(attachmentEntry.Key, (Dictionary<String, Object>)attachmentEntry.Value);
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
				Dictionary<String, Object> animationMap = (Dictionary<String, Object>)root["animations"];
				foreach (KeyValuePair<String, Object> entry in animationMap)
					readAnimation(entry.Key, (Dictionary<String, Object>)entry.Value, skeletonData);
			}

			skeletonData.Bones.TrimExcess();
			skeletonData.Slots.TrimExcess();
			skeletonData.Skins.TrimExcess();
			skeletonData.Animations.TrimExcess();
			return skeletonData;
		}

		private Attachment readAttachment (String name, Dictionary<String, Object> map) {
			if (map.ContainsKey("name"))
				name = (String)map["name"];

			AttachmentType type = AttachmentType.region;
			if (map.ContainsKey("type"))
				type = (AttachmentType)Enum.Parse(typeof(AttachmentType), (String)map["type"], false);
			Attachment attachment = attachmentLoader.NewAttachment(type, name);

			if (attachment is RegionAttachment) {
				RegionAttachment regionAttachment = (RegionAttachment)attachment;
				regionAttachment.X = getFloat(map, "x", 0) * Scale;
				regionAttachment.Y = getFloat(map, "y", 0) * Scale;
				regionAttachment.ScaleX = getFloat(map, "scaleX", 1);
				regionAttachment.ScaleY = getFloat(map, "scaleY", 1);
				regionAttachment.Rotation = getFloat(map, "rotation", 0);
				regionAttachment.Width = getFloat(map, "width", 32) * Scale;
				regionAttachment.Height = getFloat(map, "height", 32) * Scale;
				regionAttachment.UpdateOffset();
			}

			return attachment;
		}

		private float getFloat (Dictionary<String, Object> map, String name, float defaultValue) {
			if (!map.ContainsKey(name))
				return (float)defaultValue;
			return (float)map[name];
		}

		public static float toColor (String hexString, int colorIndex) {
			if (hexString.Length != 8)
				throw new ArgumentException("Color hexidecimal length must be 8, recieved: " + hexString);
			return Convert.ToInt32(hexString.Substring(colorIndex * 2, 2), 16) / (float)255;
		}

		private void readAnimation (String name, Dictionary<String, Object> map, SkeletonData skeletonData) {
			var timelines = new List<Timeline>();
			float duration = 0;

			var bonesMap = (Dictionary<String, Object>)map["bones"];
			foreach (KeyValuePair<String, Object> entry in bonesMap) {
				String boneName = entry.Key;
				int boneIndex = skeletonData.FindBoneIndex(boneName);
				if (boneIndex == -1)
					throw new Exception("Bone not found: " + boneName);

				Dictionary<String, Object> timelineMap = (Dictionary<String, Object>)entry.Value;
				foreach (KeyValuePair<String, Object> timelineEntry in timelineMap) {
					List<Object> values = (List<Object>)timelineEntry.Value;
					String timelineName = (String)timelineEntry.Key;
					if (timelineName.Equals(TIMELINE_ROTATE)) {
						RotateTimeline timeline = new RotateTimeline(values.Count);
						timeline.BoneIndex = boneIndex;

						int frameIndex = 0;
						foreach (Dictionary<String, Object> valueMap in values) {
							float time = (float)valueMap["time"];
							timeline.SetFrame(frameIndex, time, (float)valueMap["angle"]);
							readCurve(timeline, frameIndex, valueMap);
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
							readCurve(timeline, frameIndex, valueMap);
							frameIndex++;
						}
						timelines.Add(timeline);
						duration = Math.Max(duration, timeline.Frames[timeline.FrameCount * 3 - 3]);

					} else
						throw new Exception("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
				}
			}

			if (map.ContainsKey("slots")) {
				Dictionary<String, Object> slotsMap = (Dictionary<String, Object>)map["slots"];
				foreach (KeyValuePair<String, Object> entry in slotsMap) {
					String slotName = entry.Key;
					int slotIndex = skeletonData.FindSlotIndex(slotName);
					Dictionary<String, Object> timelineMap = (Dictionary<String, Object>)entry.Value;

					foreach (KeyValuePair<String, Object> timelineEntry in timelineMap) {
						List<Object> values = (List<Object>)timelineEntry.Value;
						String timelineName = (String)timelineEntry.Key;
						if (timelineName.Equals(TIMELINE_COLOR)) {
							ColorTimeline timeline = new ColorTimeline(values.Count);
							timeline.SlotIndex = slotIndex;

							int frameIndex = 0;
							foreach (Dictionary<String, Object> valueMap in values) {
								float time = (float)valueMap["time"];
								String c = (String)valueMap["color"];
								timeline.setFrame(frameIndex, time, toColor(c, 0), toColor(c, 1), toColor(c, 2), toColor(c, 3));
								readCurve(timeline, frameIndex, valueMap);
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

		private void readCurve (CurveTimeline timeline, int frameIndex, Dictionary<String, Object> valueMap) {
			if (!valueMap.ContainsKey("curve"))
				return;
			Object curveObject = valueMap["curve"];
			if (curveObject.Equals("stepped"))
				timeline.SetStepped(frameIndex);
			else if (curveObject.GetType() == typeof(List<float>)) {
				List<float> curve = (List<float>)curveObject;
				timeline.SetCurve(frameIndex, (float)curve[0], (float)curve[1], (float)curve[2], (float)curve[3]);
			}
		}
	}
}
