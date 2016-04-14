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
	public class SkeletonBinary {
		public const int TIMELINE_SCALE = 0;
		public const int TIMELINE_ROTATE = 1;
		public const int TIMELINE_TRANSLATE = 2;
		public const int TIMELINE_ATTACHMENT = 3;
		public const int TIMELINE_COLOR = 4;

		public const int CURVE_LINEAR = 0;
		public const int CURVE_STEPPED = 1;
		public const int CURVE_BEZIER = 2;

		public float Scale { get; set; }

		private AttachmentLoader attachmentLoader;
		private byte[] buffer = new byte[32];
		private List<SkeletonJson.LinkedMesh> linkedMeshes = new List<SkeletonJson.LinkedMesh>();

		public SkeletonBinary (params Atlas[] atlasArray)
			: this(new AtlasAttachmentLoader(atlasArray)) {
		}

		public SkeletonBinary (AttachmentLoader attachmentLoader) {
			if (attachmentLoader == null) throw new ArgumentNullException("attachmentLoader");
			this.attachmentLoader = attachmentLoader;
			Scale = 1;
		}

		#if !(UNITY_5 || UNITY_4 || UNITY_WSA || UNITY_WP8 || UNITY_WP8_1)
		#if WINDOWS_STOREAPP

		private async Task<SkeletonData> ReadFile(string path) {
			var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
			using (var input = new BufferedStream(await folder.GetFileAsync(path).AsTask().ConfigureAwait(false))) {
				SkeletonData skeletonData = ReadSkeletonData(input);
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
			using (var input = new BufferedStream(Microsoft.Xna.Framework.TitleContainer.OpenStream(path))) {
		#else
			using (var input = new BufferedStream(new FileStream(path, FileMode.Open))) {
		#endif // WINDOWS_PHONE
				SkeletonData skeletonData = ReadSkeletonData(input);
				skeletonData.name = Path.GetFileNameWithoutExtension(path);
				return skeletonData;
			}
		}

		#endif // WINDOWS_STOREAPP
		#endif // !(UNITY)

		public SkeletonData ReadSkeletonData (Stream input) {
			if (input == null) throw new ArgumentNullException("input");
			float scale = Scale;

			var skeletonData = new SkeletonData();
			skeletonData.hash = ReadString(input);
			if (skeletonData.hash.Length == 0) skeletonData.hash = null;
			skeletonData.version = ReadString(input);
			if (skeletonData.version.Length == 0) skeletonData.version = null;
			skeletonData.width = ReadFloat(input);
			skeletonData.height = ReadFloat(input);

			bool nonessential = ReadBoolean(input);

			if (nonessential) {
				skeletonData.imagesPath = ReadString(input);
				if (skeletonData.imagesPath.Length == 0) skeletonData.imagesPath = null;
			}

			// Bones.
			for (int i = 0, n = ReadVarint(input, true); i < n; i++) {
				String name = ReadString(input);
				BoneData parent = i == 0 ? null : skeletonData.bones.Items[ReadVarint(input, true)];
				BoneData boneData = new BoneData(name, parent);
				boneData.x = ReadFloat(input) * scale;
				boneData.y = ReadFloat(input) * scale;
				boneData.scaleX = ReadFloat(input);
				boneData.scaleY = ReadFloat(input);
				boneData.rotation = ReadFloat(input);
				boneData.length = ReadFloat(input) * scale;
				boneData.inheritScale = ReadBoolean(input);
				boneData.inheritRotation = ReadBoolean(input);
				if (nonessential) ReadInt(input); // Skip bone color.
				skeletonData.bones.Add(boneData);
			}

			// IK constraints.
			for (int i = 0, n = ReadVarint(input, true); i < n; i++) {
				IkConstraintData ikConstraintData = new IkConstraintData(ReadString(input));
				for (int ii = 0, nn = ReadVarint(input, true); ii < nn; ii++)
					ikConstraintData.bones.Add(skeletonData.bones.Items[ReadVarint(input, true)]);
				ikConstraintData.target = skeletonData.bones.Items[ReadVarint(input, true)];
				ikConstraintData.mix = ReadFloat(input);
				ikConstraintData.bendDirection = ReadSByte(input);
				skeletonData.ikConstraints.Add(ikConstraintData);
			}

			// Transform constraints.
			for (int i = 0, n = ReadVarint(input, true); i < n; i++) {
				TransformConstraintData transformConstraintData = new TransformConstraintData(ReadString(input));
				transformConstraintData.bone = skeletonData.bones.Items[ReadVarint(input, true)];
				transformConstraintData.target = skeletonData.bones.Items[ReadVarint(input, true)];
				transformConstraintData.translateMix = ReadFloat(input);
				transformConstraintData.x = ReadFloat(input) * scale;
				transformConstraintData.y = ReadFloat(input) * scale;
				skeletonData.transformConstraints.Add(transformConstraintData);
			}

			// Slots.
			for (int i = 0, n = ReadVarint(input, true); i < n; i++) {
				String slotName = ReadString(input);
				BoneData boneData = skeletonData.bones.Items[ReadVarint(input, true)];
				SlotData slotData = new SlotData(slotName, boneData);
				int color = ReadInt(input);
				slotData.r = ((color & 0xff000000) >> 24) / 255f;
				slotData.g = ((color & 0x00ff0000) >> 16) / 255f;
				slotData.b = ((color & 0x0000ff00) >> 8) / 255f;
				slotData.a = ((color & 0x000000ff)) / 255f;
				slotData.attachmentName = ReadString(input);
				slotData.blendMode = (BlendMode)ReadVarint(input, true);
				skeletonData.slots.Add(slotData);
			}

			// Default skin.
			Skin defaultSkin = ReadSkin(input, "default", nonessential);
			if (defaultSkin != null) {
				skeletonData.defaultSkin = defaultSkin;
				skeletonData.skins.Add(defaultSkin);
			}

			// Skins.
			for (int i = 0, n = ReadVarint(input, true); i < n; i++)
				skeletonData.skins.Add(ReadSkin(input, ReadString(input), nonessential));

			// Linked meshes.
			for (int i = 0, n = linkedMeshes.Count; i < n; i++) {
				SkeletonJson.LinkedMesh linkedMesh = linkedMeshes[i];
				Skin skin = linkedMesh.skin == null ? skeletonData.DefaultSkin : skeletonData.FindSkin(linkedMesh.skin);
				if (skin == null) throw new Exception("Skin not found: " + linkedMesh.skin);
				Attachment parent = skin.GetAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (parent == null) throw new Exception("Parent mesh not found: " + linkedMesh.parent);
				if (linkedMesh.mesh is MeshAttachment) {
					MeshAttachment mesh = (MeshAttachment)linkedMesh.mesh;
					mesh.ParentMesh = (MeshAttachment)parent;
					mesh.UpdateUVs();
				} else {
					WeightedMeshAttachment mesh = (WeightedMeshAttachment)linkedMesh.mesh;
					mesh.ParentMesh = (WeightedMeshAttachment)parent;
					mesh.UpdateUVs();
				}
			}
			linkedMeshes.Clear();

			// Events.
			for (int i = 0, n = ReadVarint(input, true); i < n; i++) {
				EventData eventData = new EventData(ReadString(input));
				eventData.Int = ReadVarint(input, false);
				eventData.Float = ReadFloat(input);
				eventData.String = ReadString(input);
				skeletonData.events.Add(eventData);
			}

			// Animations.
			for (int i = 0, n = ReadVarint(input, true); i < n; i++)
				ReadAnimation(ReadString(input), input, skeletonData);

			skeletonData.bones.TrimExcess();
			skeletonData.slots.TrimExcess();
			skeletonData.skins.TrimExcess();
			skeletonData.events.TrimExcess();
			skeletonData.animations.TrimExcess();
			skeletonData.ikConstraints.TrimExcess();
			return skeletonData;
		}

		/** @return May be null. */
		private Skin ReadSkin (Stream input, String skinName, bool nonessential) {
			int slotCount = ReadVarint(input, true);
			if (slotCount == 0) return null;
			Skin skin = new Skin(skinName);
			for (int i = 0; i < slotCount; i++) {
				int slotIndex = ReadVarint(input, true);
				for (int ii = 0, nn = ReadVarint(input, true); ii < nn; ii++) {
					String name = ReadString(input);
					skin.AddAttachment(slotIndex, name, ReadAttachment(input, skin, slotIndex, name, nonessential));
				}
			}
			return skin;
		}

		private Attachment ReadAttachment (Stream input, Skin skin, int slotIndex, String attachmentName, bool nonessential) {
			float scale = Scale;

			String name = ReadString(input);
			if (name == null) name = attachmentName;

			AttachmentType type = (AttachmentType)input.ReadByte();
			switch (type) {
			case AttachmentType.region: {
					String path = ReadString(input);
					float x = ReadFloat(input);
					float y = ReadFloat(input);
					float scaleX = ReadFloat(input);
					float scaleY = ReadFloat(input);
					float rotation = ReadFloat(input);
					float width = ReadFloat(input);
					float height = ReadFloat(input);
					int color = ReadInt(input);

					if (path == null) path = name;
					RegionAttachment region = attachmentLoader.NewRegionAttachment(skin, name, path);
					if (region == null) return null;
					region.Path = path;
					region.x = x * scale;
					region.y = y * scale;
					region.scaleX = scaleX;
					region.scaleY = scaleY;
					region.rotation = rotation;
					region.width = width * scale;
					region.height = height * scale;
					region.r = ((color & 0xff000000) >> 24) / 255f;
					region.g = ((color & 0x00ff0000) >> 16) / 255f;
					region.b = ((color & 0x0000ff00) >> 8) / 255f;
					region.a = ((color & 0x000000ff)) / 255f;
					region.UpdateOffset();
					return region;
				}
			case AttachmentType.boundingbox: {
					float[] vertices = ReadFloatArray(input, ReadVarint(input, true) * 2, scale);
					BoundingBoxAttachment box = attachmentLoader.NewBoundingBoxAttachment(skin, name);
					if (box == null) return null;
					box.vertices = vertices;
					return box;
				}
			case AttachmentType.mesh: {
					String path = ReadString(input);
					int color = ReadInt(input);
					int hullLength = 0;
					int verticesLength = ReadVarint(input, true) * 2;
					float[] uvs = ReadFloatArray(input, verticesLength, 1);
					int[] triangles = ReadShortArray(input);
					float[] vertices = ReadFloatArray(input, verticesLength, scale);
					hullLength = ReadVarint(input, true);
					int[] edges = null;
					float width = 0, height = 0;
					if (nonessential) {
						edges = ReadShortArray(input);
						width = ReadFloat(input);
						height = ReadFloat(input);
					}

					if (path == null) path = name;
					MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, name, path);
					if (mesh == null) return null;
					mesh.Path = path;
					mesh.r = ((color & 0xff000000) >> 24) / 255f;
					mesh.g = ((color & 0x00ff0000) >> 16) / 255f;
					mesh.b = ((color & 0x0000ff00) >> 8) / 255f;
					mesh.a = ((color & 0x000000ff)) / 255f;
					mesh.vertices = vertices;
					mesh.triangles = triangles;
					mesh.regionUVs = uvs;
					mesh.UpdateUVs();
					mesh.HullLength = hullLength;
					if (nonessential) {
						mesh.Edges = edges;
						mesh.Width = width * scale;
						mesh.Height = height * scale;
					}
					return mesh;
				}
			case AttachmentType.linkedmesh: {
					String path = ReadString(input);
					int color = ReadInt(input);
					String skinName = ReadString(input);
					String parent = ReadString(input);
					bool inheritFFD = ReadBoolean(input);
					float width = 0, height = 0;
					if (nonessential) {
						width = ReadFloat(input);
						height = ReadFloat(input);
					}

					if (path == null) path = name;
					MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, name, path);
					if (mesh == null) return null;
					mesh.Path = path;
					mesh.r = ((color & 0xff000000) >> 24) / 255f;
					mesh.g = ((color & 0x00ff0000) >> 16) / 255f;
					mesh.b = ((color & 0x0000ff00) >> 8) / 255f;
					mesh.a = ((color & 0x000000ff)) / 255f;
					mesh.inheritFFD = inheritFFD;
					if (nonessential) {
						mesh.Width = width * scale;
						mesh.Height = height * scale;
					}
					linkedMeshes.Add(new SkeletonJson.LinkedMesh(mesh, skinName, slotIndex, parent));
					return mesh;
				}
			case AttachmentType.weightedmesh: {
					String path = ReadString(input);
					int color = ReadInt(input);
					int vertexCount = ReadVarint(input, true);		
					float[] uvs = ReadFloatArray(input, vertexCount * 2, 1);
					int[] triangles = ReadShortArray(input);
					var weights = new List<float>(uvs.Length * 3 * 3);
					var bones = new List<int>(uvs.Length * 3);
					for (int i = 0; i < vertexCount; i++) {
						int boneCount = (int)ReadFloat(input);
						bones.Add(boneCount);
						for (int ii = 0; ii < boneCount; ii++) {
							bones.Add((int)ReadFloat(input));
							weights.Add(ReadFloat(input) * scale);
							weights.Add(ReadFloat(input) * scale);
							weights.Add(ReadFloat(input));
						}
					}
					int hullLength = ReadVarint(input, true);
					int[] edges = null;
					float width = 0, height = 0;
					if (nonessential) {
						edges = ReadShortArray(input);
						width = ReadFloat(input);
						height = ReadFloat(input);
					}

					if (path == null) path = name;
					WeightedMeshAttachment mesh = attachmentLoader.NewWeightedMeshAttachment(skin, name, path);
					if (mesh == null) return null;
					mesh.Path = path;
					mesh.r = ((color & 0xff000000) >> 24) / 255f;
					mesh.g = ((color & 0x00ff0000) >> 16) / 255f;
					mesh.b = ((color & 0x0000ff00) >> 8) / 255f;
					mesh.a = ((color & 0x000000ff)) / 255f;
					mesh.bones = bones.ToArray();
					mesh.weights = weights.ToArray();
					mesh.triangles = triangles;
					mesh.regionUVs = uvs;
					mesh.UpdateUVs();
					mesh.HullLength = hullLength * 2;
					if (nonessential) {
						mesh.Edges = edges;
						mesh.Width = width * scale;
						mesh.Height = height * scale;
					}
					//
					return mesh;
				}
			case AttachmentType.weightedlinkedmesh: {
					String path = ReadString(input);
					int color = ReadInt(input);
					String skinName = ReadString(input);
					String parent = ReadString(input);
					bool inheritFFD = ReadBoolean(input);
					float width = 0, height = 0;
					if (nonessential) {
						width = ReadFloat(input);
						height = ReadFloat(input);
					}

					if (path == null) path = name;
					WeightedMeshAttachment mesh = attachmentLoader.NewWeightedMeshAttachment(skin, name, path);
					if (mesh == null) return null;
					mesh.Path = path;
					mesh.r = ((color & 0xff000000) >> 24) / 255f;
					mesh.g = ((color & 0x00ff0000) >> 16) / 255f;
					mesh.b = ((color & 0x0000ff00) >> 8) / 255f;
					mesh.a = ((color & 0x000000ff)) / 255f;
					mesh.inheritFFD = inheritFFD;
					if (nonessential) {
						mesh.Width = width * scale;
						mesh.Height = height * scale;
					}
					linkedMeshes.Add(new SkeletonJson.LinkedMesh(mesh, skinName, slotIndex, parent));
					return mesh;
				}
			}
			return null;
		}

		private float[] ReadFloatArray (Stream input, int n, float scale) {
			float[] array = new float[n];
			if (scale == 1) {
				for (int i = 0; i < n; i++)
					array[i] = ReadFloat(input);
			} else {
				for (int i = 0; i < n; i++)
					array[i] = ReadFloat(input) * scale;
			}
			return array;
		}

		private int[] ReadShortArray (Stream input) {
			int n = ReadVarint(input, true);
			int[] array = new int[n];
			for (int i = 0; i < n; i++) 
				array[i] = (input.ReadByte() << 8) | input.ReadByte();
			return array;
		}

		private void ReadAnimation (String name, Stream input, SkeletonData skeletonData) {
			var timelines = new ExposedList<Timeline>();
			float scale = Scale;
			float duration = 0;

			// Slot timelines.
			for (int i = 0, n = ReadVarint(input, true); i < n; i++) {
				int slotIndex = ReadVarint(input, true);
				for (int ii = 0, nn = ReadVarint(input, true); ii < nn; ii++) {
					int timelineType = input.ReadByte();
					int frameCount = ReadVarint(input, true);
					switch (timelineType) {
					case TIMELINE_COLOR: {
							ColorTimeline timeline = new ColorTimeline(frameCount);
							timeline.slotIndex = slotIndex;
							for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								float time = ReadFloat(input);
								int color = ReadInt(input);
								float r = ((color & 0xff000000) >> 24) / 255f;
								float g = ((color & 0x00ff0000) >> 16) / 255f;
								float b = ((color & 0x0000ff00) >> 8) / 255f;
								float a = ((color & 0x000000ff)) / 255f;
								timeline.SetFrame(frameIndex, time, r, g, b, a);
								if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[frameCount * 5 - 5]);
							break;
						}
					case TIMELINE_ATTACHMENT: {
							AttachmentTimeline timeline = new AttachmentTimeline(frameCount);
							timeline.slotIndex = slotIndex;
							for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
								timeline.SetFrame(frameIndex, ReadFloat(input), ReadString(input));
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[frameCount - 1]);
							break;
						}
					}
				}
			}

			// Bone timelines.
			for (int i = 0, n = ReadVarint(input, true); i < n; i++) {
				int boneIndex = ReadVarint(input, true);
				for (int ii = 0, nn = ReadVarint(input, true); ii < nn; ii++) {
					int timelineType = input.ReadByte();
					int frameCount = ReadVarint(input, true);
					switch (timelineType) {
					case TIMELINE_ROTATE: {
							RotateTimeline timeline = new RotateTimeline(frameCount);
							timeline.boneIndex = boneIndex;
							for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								timeline.SetFrame(frameIndex, ReadFloat(input), ReadFloat(input));
								if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[frameCount * 2 - 2]);
							break;
						}
					case TIMELINE_TRANSLATE:
					case TIMELINE_SCALE: {
							TranslateTimeline timeline;
							float timelineScale = 1;
							if (timelineType == TIMELINE_SCALE)
								timeline = new ScaleTimeline(frameCount);
							else {
								timeline = new TranslateTimeline(frameCount);
								timelineScale = scale;
							}
							timeline.boneIndex = boneIndex;
							for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								timeline.SetFrame(frameIndex, ReadFloat(input), ReadFloat(input) * timelineScale, ReadFloat(input)
									* timelineScale);
								if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[frameCount * 3 - 3]);
							break;
						}
					}
				}
			}

			// IK timelines.
			for (int i = 0, n = ReadVarint(input, true); i < n; i++) {
				IkConstraintData ikConstraint = skeletonData.ikConstraints.Items[ReadVarint(input, true)];
				int frameCount = ReadVarint(input, true);
				IkConstraintTimeline timeline = new IkConstraintTimeline(frameCount);
				timeline.ikConstraintIndex = skeletonData.ikConstraints.IndexOf(ikConstraint);
				for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
					timeline.SetFrame(frameIndex, ReadFloat(input), ReadFloat(input), ReadSByte(input));
					if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[frameCount * 3 - 3]);
			}

			// FFD timelines.
			for (int i = 0, n = ReadVarint(input, true); i < n; i++) {
				Skin skin = skeletonData.skins.Items[ReadVarint(input, true)];
				for (int ii = 0, nn = ReadVarint(input, true); ii < nn; ii++) {
					int slotIndex = ReadVarint(input, true);
					for (int iii = 0, nnn = ReadVarint(input, true); iii < nnn; iii++) {
						Attachment attachment = skin.GetAttachment(slotIndex, ReadString(input));
						int frameCount = ReadVarint(input, true);
						FfdTimeline timeline = new FfdTimeline(frameCount);
						timeline.slotIndex = slotIndex;
						timeline.attachment = attachment;
						for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							float time = ReadFloat(input);

							float[] vertices;
							int vertexCount;
							if (attachment is MeshAttachment)
								vertexCount = ((MeshAttachment)attachment).vertices.Length;
							else
								vertexCount = ((WeightedMeshAttachment)attachment).weights.Length / 3 * 2;

							int end = ReadVarint(input, true);
							if (end == 0) {
								if (attachment is MeshAttachment)
									vertices = ((MeshAttachment)attachment).vertices;
								else
									vertices = new float[vertexCount];
							} else {
								vertices = new float[vertexCount];
								int start = ReadVarint(input, true);
								end += start;
								if (scale == 1) {
									for (int v = start; v < end; v++)
										vertices[v] = ReadFloat(input);
								} else {
									for (int v = start; v < end; v++)
										vertices[v] = ReadFloat(input) * scale;
								}
								if (attachment is MeshAttachment) {
									float[] meshVertices = ((MeshAttachment)attachment).vertices;
									for (int v = 0, vn = vertices.Length; v < vn; v++)
										vertices[v] += meshVertices[v];
								}
							}

							timeline.SetFrame(frameIndex, time, vertices);
							if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
						}
						timelines.Add(timeline);
						duration = Math.Max(duration, timeline.frames[frameCount - 1]);
					}
				}
			}

			// Draw order timeline.
			int drawOrderCount = ReadVarint(input, true);
			if (drawOrderCount > 0) {
				DrawOrderTimeline timeline = new DrawOrderTimeline(drawOrderCount);
				int slotCount = skeletonData.slots.Count;
				for (int i = 0; i < drawOrderCount; i++) {
					float time = ReadFloat(input);
					int offsetCount = ReadVarint(input, true);
					int[] drawOrder = new int[slotCount];
					for (int ii = slotCount - 1; ii >= 0; ii--)
						drawOrder[ii] = -1;
					int[] unchanged = new int[slotCount - offsetCount];
					int originalIndex = 0, unchangedIndex = 0;
					for (int ii = 0; ii < offsetCount; ii++) {
						int slotIndex = ReadVarint(input, true);
						// Collect unchanged items.
						while (originalIndex != slotIndex)
							unchanged[unchangedIndex++] = originalIndex++;
						// Set changed items.
						drawOrder[originalIndex + ReadVarint(input, true)] = originalIndex++;
					}
					// Collect remaining unchanged items.
					while (originalIndex < slotCount)
						unchanged[unchangedIndex++] = originalIndex++;
					// Fill in unchanged items.
					for (int ii = slotCount - 1; ii >= 0; ii--)
						if (drawOrder[ii] == -1) drawOrder[ii] = unchanged[--unchangedIndex];
					timeline.SetFrame(i, time, drawOrder);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[drawOrderCount - 1]);
			}

			// Event timeline.
			int eventCount = ReadVarint(input, true);
			if (eventCount > 0) {
				EventTimeline timeline = new EventTimeline(eventCount);
				for (int i = 0; i < eventCount; i++) {
					float time = ReadFloat(input);
					EventData eventData = skeletonData.events.Items[ReadVarint(input, true)];
					Event e = new Event(time, eventData);
					e.Int = ReadVarint(input, false);
					e.Float = ReadFloat(input);
					e.String = ReadBoolean(input) ? ReadString(input) : eventData.String;
					timeline.SetFrame(i, e);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[eventCount - 1]);
			}

			timelines.TrimExcess();
			skeletonData.animations.Add(new Animation(name, timelines, duration));
		}

		private void ReadCurve (Stream input, int frameIndex, CurveTimeline timeline) {
			switch (input.ReadByte()) {
			case CURVE_STEPPED:
				timeline.SetStepped(frameIndex);
				break;
			case CURVE_BEZIER:
				timeline.SetCurve(frameIndex, ReadFloat(input), ReadFloat(input), ReadFloat(input), ReadFloat(input));
				break;
			}
		}

		private static sbyte ReadSByte (Stream input) {
			int value = input.ReadByte();
			if (value == -1) throw new EndOfStreamException();
			return (sbyte)value;
		}

		private static bool ReadBoolean (Stream input) {
			return input.ReadByte() != 0;
		}

		private float ReadFloat (Stream input) {
			buffer[3] = (byte)input.ReadByte();
			buffer[2] = (byte)input.ReadByte();
			buffer[1] = (byte)input.ReadByte();
			buffer[0] = (byte)input.ReadByte();
			return BitConverter.ToSingle(buffer, 0);
		}

		private static int ReadInt (Stream input) {
			return (input.ReadByte() << 24) + (input.ReadByte() << 16) + (input.ReadByte() << 8) + input.ReadByte();
		}

		private static int ReadVarint (Stream input, bool optimizePositive) {
			int b = input.ReadByte();
			int result = b & 0x7F;
			if ((b & 0x80) != 0) {
				b = input.ReadByte();
				result |= (b & 0x7F) << 7;
				if ((b & 0x80) != 0) {
					b = input.ReadByte();
					result |= (b & 0x7F) << 14;
					if ((b & 0x80) != 0) {
						b = input.ReadByte();
						result |= (b & 0x7F) << 21;
						if ((b & 0x80) != 0) result |= (input.ReadByte() & 0x7F) << 28;
					}
				}
			}
			return optimizePositive ? result : ((result >> 1) ^ -(result & 1));
		}

		private string ReadString (Stream input) {
			int byteCount = ReadVarint(input, true);
			switch (byteCount) {
			case 0:
				return null;
			case 1:
				return "";
			}
			byteCount--;
			byte[] buffer = this.buffer;
			if (buffer.Length < byteCount) buffer = new byte[byteCount];
			ReadFully(input, buffer, 0, byteCount);
			return System.Text.Encoding.UTF8.GetString(buffer, 0, byteCount);
		}

		private static void ReadFully (Stream input, byte[] buffer, int offset, int length) {
			while (length > 0) {
				int count = input.Read(buffer, offset, length);
				if (count <= 0) throw new EndOfStreamException();
				offset += count;
				length -= count;
			}
		}
	}
}
