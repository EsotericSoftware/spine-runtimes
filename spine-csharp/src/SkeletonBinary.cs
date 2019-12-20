/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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
using System.IO;
using System.Collections.Generic;

#if WINDOWS_STOREAPP
using System.Threading.Tasks;
using Windows.Storage;
#endif

namespace Spine {
	public class SkeletonBinary {
		public const int BONE_ROTATE = 0;
		public const int BONE_TRANSLATE = 1;
		public const int BONE_SCALE = 2;
		public const int BONE_SHEAR = 3;

		public const int SLOT_ATTACHMENT = 0;
		public const int SLOT_COLOR = 1;
		public const int SLOT_TWO_COLOR = 2;

		public const int PATH_POSITION = 0;
		public const int PATH_SPACING = 1;
		public const int PATH_MIX = 2;

		public const int CURVE_LINEAR = 0;
		public const int CURVE_STEPPED = 1;
		public const int CURVE_BEZIER = 2;

		public float Scale { get; set; }

		private AttachmentLoader attachmentLoader;
		private List<SkeletonJson.LinkedMesh> linkedMeshes = new List<SkeletonJson.LinkedMesh>();

		public SkeletonBinary (params Atlas[] atlasArray)
			: this(new AtlasAttachmentLoader(atlasArray)) {
		}

		public SkeletonBinary (AttachmentLoader attachmentLoader) {
			if (attachmentLoader == null) throw new ArgumentNullException("attachmentLoader");
			this.attachmentLoader = attachmentLoader;
			Scale = 1;
		}

		#if !ISUNITY && WINDOWS_STOREAPP
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
			using (var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
		#endif
				SkeletonData skeletonData = ReadSkeletonData(input);
				skeletonData.name = Path.GetFileNameWithoutExtension(path);
				return skeletonData;
			}
		}
		#endif // WINDOWS_STOREAPP

		public static readonly TransformMode[] TransformModeValues = {
			TransformMode.Normal,
			TransformMode.OnlyTranslation,
			TransformMode.NoRotationOrReflection,
			TransformMode.NoScale,
			TransformMode.NoScaleOrReflection
		};

		/// <summary>Returns the version string of binary skeleton data.</summary>
		public static string GetVersionString (Stream file) {
			if (file == null) throw new ArgumentNullException("file");

			SkeletonInput input = new SkeletonInput(file);
			return input.GetVersionString();
		}

		public SkeletonData ReadSkeletonData (Stream file) {
			if (file == null) throw new ArgumentNullException("file");
			float scale = Scale;

			var skeletonData = new SkeletonData();
			SkeletonInput input = new SkeletonInput(file);

			skeletonData.hash = input.ReadString();
			if (skeletonData.hash.Length == 0) skeletonData.hash = null;
			skeletonData.version = input.ReadString();
			if (skeletonData.version.Length == 0) skeletonData.version = null;
			if ("3.8.75" == skeletonData.version)
					throw new Exception("Unsupported skeleton data, please export with a newer version of Spine.");
			skeletonData.x = input.ReadFloat();
			skeletonData.y = input.ReadFloat();
			skeletonData.width = input.ReadFloat();
			skeletonData.height = input.ReadFloat();

			bool nonessential = input.ReadBoolean();

			if (nonessential) {
				skeletonData.fps = input.ReadFloat();

				skeletonData.imagesPath = input.ReadString();
				if (string.IsNullOrEmpty(skeletonData.imagesPath)) skeletonData.imagesPath = null;

				skeletonData.audioPath = input.ReadString();
				if (string.IsNullOrEmpty(skeletonData.audioPath)) skeletonData.audioPath = null;
			}

			int n;
			Object[] o;

			// Strings.
			input.strings = new ExposedList<string>(n = input.ReadInt(true));
			o = input.strings.Resize(n).Items;
			for (int i = 0; i < n; i++)
				o[i] = input.ReadString();

			// Bones.
			o = skeletonData.bones.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0; i < n; i++) {
				String name = input.ReadString();
				BoneData parent = i == 0 ? null : skeletonData.bones.Items[input.ReadInt(true)];
				BoneData data = new BoneData(i, name, parent);
				data.rotation = input.ReadFloat();
				data.x = input.ReadFloat() * scale;
				data.y = input.ReadFloat() * scale;
				data.scaleX = input.ReadFloat();
				data.scaleY = input.ReadFloat();
				data.shearX = input.ReadFloat();
				data.shearY = input.ReadFloat();
				data.length = input.ReadFloat() * scale;
				data.transformMode = TransformModeValues[input.ReadInt(true)];
				data.skinRequired = input.ReadBoolean();
				if (nonessential) input.ReadInt(); // Skip bone color.
				o[i] = data;
			}

			// Slots.
			o = skeletonData.slots.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0; i < n; i++) {
				String slotName = input.ReadString();
				BoneData boneData = skeletonData.bones.Items[input.ReadInt(true)];
				SlotData slotData = new SlotData(i, slotName, boneData);
				int color = input.ReadInt();
				slotData.r = ((color & 0xff000000) >> 24) / 255f;
				slotData.g = ((color & 0x00ff0000) >> 16) / 255f;
				slotData.b = ((color & 0x0000ff00) >> 8) / 255f;
				slotData.a = ((color & 0x000000ff)) / 255f;

				int darkColor = input.ReadInt(); // 0x00rrggbb
				if (darkColor != -1) {
					slotData.hasSecondColor = true;
					slotData.r2 = ((darkColor & 0x00ff0000) >> 16) / 255f;
					slotData.g2 = ((darkColor & 0x0000ff00) >> 8) / 255f;
					slotData.b2 = ((darkColor & 0x000000ff)) / 255f;
				}

				slotData.attachmentName = input.ReadStringRef();
				slotData.blendMode = (BlendMode)input.ReadInt(true);
				o[i] = slotData;
			}

			// IK constraints.
			o = skeletonData.ikConstraints.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0, nn; i < n; i++) {
				IkConstraintData data = new IkConstraintData(input.ReadString());
				data.order = input.ReadInt(true);
				data.skinRequired = input.ReadBoolean();
				Object[] bones = data.bones.Resize(nn = input.ReadInt(true)).Items;
				for (int ii = 0; ii < nn; ii++)
					bones[ii] = skeletonData.bones.Items[input.ReadInt(true)];
				data.target = skeletonData.bones.Items[input.ReadInt(true)];
				data.mix = input.ReadFloat();
				data.softness = input.ReadFloat() * scale;
				data.bendDirection = input.ReadSByte();
				data.compress = input.ReadBoolean();
				data.stretch = input.ReadBoolean();
				data.uniform = input.ReadBoolean();
				o[i] = data;
			}

			// Transform constraints.
			o = skeletonData.transformConstraints.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0, nn; i < n; i++) {
				TransformConstraintData data = new TransformConstraintData(input.ReadString());
				data.order = input.ReadInt(true);
				data.skinRequired = input.ReadBoolean();
				Object[] bones = data.bones.Resize(nn = input.ReadInt(true)).Items;
				for (int ii = 0; ii < nn; ii++)
					bones[ii] = skeletonData.bones.Items[input.ReadInt(true)];
				data.target = skeletonData.bones.Items[input.ReadInt(true)];
				data.local = input.ReadBoolean();
				data.relative = input.ReadBoolean();
				data.offsetRotation = input.ReadFloat();
				data.offsetX = input.ReadFloat() * scale;
				data.offsetY = input.ReadFloat() * scale;
				data.offsetScaleX = input.ReadFloat();
				data.offsetScaleY = input.ReadFloat();
				data.offsetShearY = input.ReadFloat();
				data.rotateMix = input.ReadFloat();
				data.translateMix = input.ReadFloat();
				data.scaleMix = input.ReadFloat();
				data.shearMix = input.ReadFloat();
				o[i] = data;
			}

			// Path constraints
			o = skeletonData.pathConstraints.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0, nn; i < n; i++) {
				PathConstraintData data = new PathConstraintData(input.ReadString());
				data.order = input.ReadInt(true);
				data.skinRequired = input.ReadBoolean();
				Object[] bones = data.bones.Resize(nn = input.ReadInt(true)).Items;
				for (int ii = 0; ii < nn; ii++)
					bones[ii] = skeletonData.bones.Items[input.ReadInt(true)];
				data.target = skeletonData.slots.Items[input.ReadInt(true)];
				data.positionMode = (PositionMode)Enum.GetValues(typeof(PositionMode)).GetValue(input.ReadInt(true));
				data.spacingMode = (SpacingMode)Enum.GetValues(typeof(SpacingMode)).GetValue(input.ReadInt(true));
				data.rotateMode = (RotateMode)Enum.GetValues(typeof(RotateMode)).GetValue(input.ReadInt(true));
				data.offsetRotation = input.ReadFloat();
				data.position = input.ReadFloat();
				if (data.positionMode == PositionMode.Fixed) data.position *= scale;
				data.spacing = input.ReadFloat();
				if (data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed) data.spacing *= scale;
				data.rotateMix = input.ReadFloat();
				data.translateMix = input.ReadFloat();
				o[i] = data;
			}

			// Default skin.
			Skin defaultSkin = ReadSkin(input, skeletonData, true, nonessential);
			if (defaultSkin != null) {
				skeletonData.defaultSkin = defaultSkin;
				skeletonData.skins.Add(defaultSkin);
			}

			// Skins.
			{
				int i = skeletonData.skins.Count;
				o = skeletonData.skins.Resize(n = i + input.ReadInt(true)).Items;
				for (; i < n; i++)
					o[i] = ReadSkin(input, skeletonData, false, nonessential);
			}

			// Linked meshes.
			n = linkedMeshes.Count;
			for (int i = 0; i < n; i++) {
				SkeletonJson.LinkedMesh linkedMesh = linkedMeshes[i];
				Skin skin = linkedMesh.skin == null ? skeletonData.DefaultSkin : skeletonData.FindSkin(linkedMesh.skin);
				if (skin == null) throw new Exception("Skin not found: " + linkedMesh.skin);
				Attachment parent = skin.GetAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (parent == null) throw new Exception("Parent mesh not found: " + linkedMesh.parent);
				linkedMesh.mesh.DeformAttachment = linkedMesh.inheritDeform ? (VertexAttachment)parent : linkedMesh.mesh;
				linkedMesh.mesh.ParentMesh = (MeshAttachment)parent;
				linkedMesh.mesh.UpdateUVs();
			}
			linkedMeshes.Clear();

			// Events.
			o = skeletonData.events.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0; i < n; i++) {
				EventData data = new EventData(input.ReadStringRef());
				data.Int = input.ReadInt(false);
				data.Float = input.ReadFloat();
				data.String = input.ReadString();
				data.AudioPath = input.ReadString();
				if (data.AudioPath != null) {
					data.Volume = input.ReadFloat();
					data.Balance = input.ReadFloat();
				}
				o[i] = data;
			}

			// Animations.
			o = skeletonData.animations.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0; i < n; i++)
				o[i] = ReadAnimation(input.ReadString(), input, skeletonData);

			return skeletonData;
		}


		/// <returns>May be null.</returns>
		private Skin ReadSkin (SkeletonInput input, SkeletonData skeletonData, bool defaultSkin, bool nonessential) {

			Skin skin;
			int slotCount;

			if (defaultSkin) {
				slotCount = input.ReadInt(true);
				if (slotCount == 0) return null;
				skin = new Skin("default");
			} else {
				skin = new Skin(input.ReadStringRef());
				Object[] bones = skin.bones.Resize(input.ReadInt(true)).Items;
				for (int i = 0, n = skin.bones.Count; i < n; i++)
					bones[i] = skeletonData.bones.Items[input.ReadInt(true)];

				for (int i = 0, n = input.ReadInt(true); i < n; i++)
					skin.constraints.Add(skeletonData.ikConstraints.Items[input.ReadInt(true)]);
				for (int i = 0, n = input.ReadInt(true); i < n; i++)
					skin.constraints.Add(skeletonData.transformConstraints.Items[input.ReadInt(true)]);
				for (int i = 0, n = input.ReadInt(true); i < n; i++)
					skin.constraints.Add(skeletonData.pathConstraints.Items[input.ReadInt(true)]);
				skin.constraints.TrimExcess();
				slotCount = input.ReadInt(true);
			}
			for (int i = 0; i < slotCount; i++) {
				int slotIndex = input.ReadInt(true);
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					String name = input.ReadStringRef();
					Attachment attachment = ReadAttachment(input, skeletonData, skin, slotIndex, name, nonessential);
					if (attachment != null) skin.SetAttachment(slotIndex, name, attachment);
				}
			}
			return skin;
		}

		private Attachment ReadAttachment (SkeletonInput input, SkeletonData skeletonData, Skin skin, int slotIndex,
			String attachmentName, bool nonessential) {

			float scale = Scale;

			String name = input.ReadStringRef();
			if (name == null) name = attachmentName;

			AttachmentType type = (AttachmentType)input.ReadByte();
			switch (type) {
			case AttachmentType.Region: {
					String path = input.ReadStringRef();
					float rotation = input.ReadFloat();
					float x = input.ReadFloat();
					float y = input.ReadFloat();
					float scaleX = input.ReadFloat();
					float scaleY = input.ReadFloat();
					float width = input.ReadFloat();
					float height = input.ReadFloat();
					int color = input.ReadInt();

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
			case AttachmentType.Boundingbox: {
					int vertexCount = input.ReadInt(true);
					Vertices vertices = ReadVertices(input, vertexCount);
					if (nonessential) input.ReadInt(); //int color = nonessential ? input.ReadInt() : 0; // Avoid unused local warning.

					BoundingBoxAttachment box = attachmentLoader.NewBoundingBoxAttachment(skin, name);
					if (box == null) return null;
					box.worldVerticesLength = vertexCount << 1;
					box.vertices = vertices.vertices;
					box.bones = vertices.bones;
					// skipped porting: if (nonessential) Color.rgba8888ToColor(box.getColor(), color);
					return box;
				}
			case AttachmentType.Mesh: {
					String path = input.ReadStringRef();
					int color = input.ReadInt();
					int vertexCount = input.ReadInt(true);
					float[] uvs = ReadFloatArray(input, vertexCount << 1, 1);
					int[] triangles = ReadShortArray(input);
					Vertices vertices = ReadVertices(input, vertexCount);
					int hullLength = input.ReadInt(true);
					int[] edges = null;
					float width = 0, height = 0;
					if (nonessential) {
						edges = ReadShortArray(input);
						width = input.ReadFloat();
						height = input.ReadFloat();
					}

					if (path == null) path = name;
					MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, name, path);
					if (mesh == null) return null;
					mesh.Path = path;
					mesh.r = ((color & 0xff000000) >> 24) / 255f;
					mesh.g = ((color & 0x00ff0000) >> 16) / 255f;
					mesh.b = ((color & 0x0000ff00) >> 8) / 255f;
					mesh.a = ((color & 0x000000ff)) / 255f;
					mesh.bones = vertices.bones;
					mesh.vertices = vertices.vertices;
					mesh.WorldVerticesLength = vertexCount << 1;
					mesh.triangles = triangles;
					mesh.regionUVs = uvs;
					mesh.UpdateUVs();
					mesh.HullLength = hullLength << 1;
					if (nonessential) {
						mesh.Edges = edges;
						mesh.Width = width * scale;
						mesh.Height = height * scale;
					}
					return mesh;
				}
			case AttachmentType.Linkedmesh: {
					String path = input.ReadStringRef();
					int color = input.ReadInt();
					String skinName = input.ReadStringRef();
					String parent = input.ReadStringRef();
					bool inheritDeform = input.ReadBoolean();
					float width = 0, height = 0;
					if (nonessential) {
						width = input.ReadFloat();
						height = input.ReadFloat();
					}

					if (path == null) path = name;
					MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, name, path);
					if (mesh == null) return null;
					mesh.Path = path;
					mesh.r = ((color & 0xff000000) >> 24) / 255f;
					mesh.g = ((color & 0x00ff0000) >> 16) / 255f;
					mesh.b = ((color & 0x0000ff00) >> 8) / 255f;
					mesh.a = ((color & 0x000000ff)) / 255f;
					if (nonessential) {
						mesh.Width = width * scale;
						mesh.Height = height * scale;
					}
					linkedMeshes.Add(new SkeletonJson.LinkedMesh(mesh, skinName, slotIndex, parent, inheritDeform));
					return mesh;
				}
			case AttachmentType.Path: {
					bool closed = input.ReadBoolean();
					bool constantSpeed = input.ReadBoolean();
					int vertexCount = input.ReadInt(true);
					Vertices vertices = ReadVertices(input, vertexCount);
					float[] lengths = new float[vertexCount / 3];
					for (int i = 0, n = lengths.Length; i < n; i++)
						lengths[i] = input.ReadFloat() * scale;
					if (nonessential) input.ReadInt(); //int color = nonessential ? input.ReadInt() : 0;

					PathAttachment path = attachmentLoader.NewPathAttachment(skin, name);
					if (path == null) return null;
					path.closed = closed;
					path.constantSpeed = constantSpeed;
					path.worldVerticesLength = vertexCount << 1;
					path.vertices = vertices.vertices;
					path.bones = vertices.bones;
					path.lengths = lengths;
					// skipped porting: if (nonessential) Color.rgba8888ToColor(path.getColor(), color);
					return path;
				}
			case AttachmentType.Point: {
					float rotation = input.ReadFloat();
					float x = input.ReadFloat();
					float y = input.ReadFloat();
					if (nonessential) input.ReadInt(); //int color = nonessential ? input.ReadInt() : 0;

					PointAttachment point = attachmentLoader.NewPointAttachment(skin, name);
					if (point == null) return null;
					point.x = x * scale;
					point.y = y * scale;
					point.rotation = rotation;
					// skipped porting: if (nonessential) point.color = color;
					return point;
				}
			case AttachmentType.Clipping: {
					int endSlotIndex = input.ReadInt(true);
					int vertexCount = input.ReadInt(true);
					Vertices vertices = ReadVertices(input, vertexCount);
					if (nonessential) input.ReadInt();

					ClippingAttachment clip = attachmentLoader.NewClippingAttachment(skin, name);
					if (clip == null) return null;
					clip.EndSlot = skeletonData.slots.Items[endSlotIndex];
					clip.worldVerticesLength = vertexCount << 1;
					clip.vertices = vertices.vertices;
					clip.bones = vertices.bones;
					// skipped porting: if (nonessential) Color.rgba8888ToColor(clip.getColor(), color);
					return clip;
				}
			}
			return null;
		}

		private Vertices ReadVertices (SkeletonInput input, int vertexCount) {
			float scale = Scale;
			int verticesLength = vertexCount << 1;
			Vertices vertices = new Vertices();
			if(!input.ReadBoolean()) {
				vertices.vertices = ReadFloatArray(input, verticesLength, scale);
				return vertices;
			}
			var weights = new ExposedList<float>(verticesLength * 3 * 3);
			var bonesArray = new ExposedList<int>(verticesLength * 3);
			for (int i = 0; i < vertexCount; i++) {
				int boneCount = input.ReadInt(true);
				bonesArray.Add(boneCount);
				for (int ii = 0; ii < boneCount; ii++) {
					bonesArray.Add(input.ReadInt(true));
					weights.Add(input.ReadFloat() * scale);
					weights.Add(input.ReadFloat() * scale);
					weights.Add(input.ReadFloat());
				}
			}

			vertices.vertices = weights.ToArray();
			vertices.bones = bonesArray.ToArray();
			return vertices;
		}

		private float[] ReadFloatArray (SkeletonInput input, int n, float scale) {
			float[] array = new float[n];
			if (scale == 1) {
				for (int i = 0; i < n; i++)
					array[i] = input.ReadFloat();
			} else {
				for (int i = 0; i < n; i++)
					array[i] = input.ReadFloat() * scale;
			}
			return array;
		}

		private int[] ReadShortArray (SkeletonInput input) {
			int n = input.ReadInt(true);
			int[] array = new int[n];
			for (int i = 0; i < n; i++)
				array[i] = (input.ReadByte() << 8) | input.ReadByte();
			return array;
		}

		private Animation ReadAnimation (String name, SkeletonInput input, SkeletonData skeletonData) {
			var timelines = new ExposedList<Timeline>(32);
			float scale = Scale;
			float duration = 0;

			// Slot timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int slotIndex = input.ReadInt(true);
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int timelineType = input.ReadByte();
					int frameCount = input.ReadInt(true);
					switch (timelineType) {
					case SLOT_ATTACHMENT: {
							AttachmentTimeline timeline = new AttachmentTimeline(frameCount);
							timeline.slotIndex = slotIndex;
							for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
								timeline.SetFrame(frameIndex, input.ReadFloat(), input.ReadStringRef());
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[frameCount - 1]);
							break;
						}
					case SLOT_COLOR: {
							ColorTimeline timeline = new ColorTimeline(frameCount);
							timeline.slotIndex = slotIndex;
							for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								float time = input.ReadFloat();
								int color = input.ReadInt();
								float r = ((color & 0xff000000) >> 24) / 255f;
								float g = ((color & 0x00ff0000) >> 16) / 255f;
								float b = ((color & 0x0000ff00) >> 8) / 255f;
								float a = ((color & 0x000000ff)) / 255f;
								timeline.SetFrame(frameIndex, time, r, g, b, a);
								if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[(frameCount - 1) * ColorTimeline.ENTRIES]);
							break;
						}
					case SLOT_TWO_COLOR: {
							TwoColorTimeline timeline = new TwoColorTimeline(frameCount);
							timeline.slotIndex = slotIndex;
							for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								float time = input.ReadFloat();
								int color = input.ReadInt();
								float r = ((color & 0xff000000) >> 24) / 255f;
								float g = ((color & 0x00ff0000) >> 16) / 255f;
								float b = ((color & 0x0000ff00) >> 8) / 255f;
								float a = ((color & 0x000000ff)) / 255f;
								int color2 = input.ReadInt(); // 0x00rrggbb
								float r2 = ((color2 & 0x00ff0000) >> 16) / 255f;
								float g2 = ((color2 & 0x0000ff00) >> 8) / 255f;
								float b2 = ((color2 & 0x000000ff)) / 255f;

								timeline.SetFrame(frameIndex, time, r, g, b, a, r2, g2, b2);
								if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[(frameCount - 1) * TwoColorTimeline.ENTRIES]);
							break;
						}
					}
				}
			}

			// Bone timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int boneIndex = input.ReadInt(true);
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int timelineType = input.ReadByte();
					int frameCount = input.ReadInt(true);
					switch (timelineType) {
					case BONE_ROTATE: {
							RotateTimeline timeline = new RotateTimeline(frameCount);
							timeline.boneIndex = boneIndex;
							for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								timeline.SetFrame(frameIndex, input.ReadFloat(), input.ReadFloat());
								if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[(frameCount - 1) * RotateTimeline.ENTRIES]);
							break;
						}
					case BONE_TRANSLATE:
					case BONE_SCALE:
					case BONE_SHEAR: {
							TranslateTimeline timeline;
							float timelineScale = 1;
							if (timelineType == BONE_SCALE)
								timeline = new ScaleTimeline(frameCount);
							else if (timelineType == BONE_SHEAR)
								timeline = new ShearTimeline(frameCount);
							else {
								timeline = new TranslateTimeline(frameCount);
								timelineScale = scale;
							}
							timeline.boneIndex = boneIndex;
							for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								timeline.SetFrame(frameIndex, input.ReadFloat(), input.ReadFloat() * timelineScale,
									input.ReadFloat() * timelineScale);
								if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
							}
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[(frameCount - 1) * TranslateTimeline.ENTRIES]);
							break;
						}
					}
				}
			}

			// IK constraint timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int index = input.ReadInt(true);
				int frameCount = input.ReadInt(true);
				IkConstraintTimeline timeline = new IkConstraintTimeline(frameCount) {
					ikConstraintIndex = index
				};
				for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
					timeline.SetFrame(frameIndex, input.ReadFloat(), input.ReadFloat(), input.ReadFloat() * scale, input.ReadSByte(), input.ReadBoolean(),
						input.ReadBoolean());
					if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[(frameCount - 1) * IkConstraintTimeline.ENTRIES]);
			}

			// Transform constraint timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int index = input.ReadInt(true);
				int frameCount = input.ReadInt(true);
				TransformConstraintTimeline timeline = new TransformConstraintTimeline(frameCount);
				timeline.transformConstraintIndex = index;
				for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
					timeline.SetFrame(frameIndex, input.ReadFloat(), input.ReadFloat(), input.ReadFloat(), input.ReadFloat(),
						input.ReadFloat());
					if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[(frameCount - 1) * TransformConstraintTimeline.ENTRIES]);
			}

			// Path constraint timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int index = input.ReadInt(true);
				PathConstraintData data = skeletonData.pathConstraints.Items[index];
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int timelineType = input.ReadSByte();
					int frameCount = input.ReadInt(true);
					switch(timelineType) {
						case PATH_POSITION:
						case PATH_SPACING: {
								PathConstraintPositionTimeline timeline;
								float timelineScale = 1;
								if (timelineType == PATH_SPACING) {
									timeline = new PathConstraintSpacingTimeline(frameCount);
									if (data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed) timelineScale = scale;
								} else {
									timeline = new PathConstraintPositionTimeline(frameCount);
									if (data.positionMode == PositionMode.Fixed) timelineScale = scale;
								}
								timeline.pathConstraintIndex = index;
								for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
									timeline.SetFrame(frameIndex, input.ReadFloat(), input.ReadFloat() * timelineScale);
									if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
								}
								timelines.Add(timeline);
								duration = Math.Max(duration, timeline.frames[(frameCount - 1) * PathConstraintPositionTimeline.ENTRIES]);
								break;
							}
						case PATH_MIX: {
								PathConstraintMixTimeline timeline = new PathConstraintMixTimeline(frameCount);
								timeline.pathConstraintIndex = index;
								for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
									timeline.SetFrame(frameIndex, input.ReadFloat(), input.ReadFloat(), input.ReadFloat());
									if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
								}
								timelines.Add(timeline);
								duration = Math.Max(duration, timeline.frames[(frameCount - 1) * PathConstraintMixTimeline.ENTRIES]);
								break;
							}
					}
				}
			}

			// Deform timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				Skin skin = skeletonData.skins.Items[input.ReadInt(true)];
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int slotIndex = input.ReadInt(true);
					for (int iii = 0, nnn = input.ReadInt(true); iii < nnn; iii++) {
						VertexAttachment attachment = (VertexAttachment)skin.GetAttachment(slotIndex, input.ReadStringRef());
						bool weighted = attachment.bones != null;
						float[] vertices = attachment.vertices;
						int deformLength = weighted ? vertices.Length / 3 * 2 : vertices.Length;

						int frameCount = input.ReadInt(true);
						DeformTimeline timeline = new DeformTimeline(frameCount);
						timeline.slotIndex = slotIndex;
						timeline.attachment = attachment;

						for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							float time = input.ReadFloat();
							float[] deform;
							int end = input.ReadInt(true);
							if (end == 0)
								deform = weighted ? new float[deformLength] : vertices;
							else {
								deform = new float[deformLength];
								int start = input.ReadInt(true);
								end += start;
								if (scale == 1) {
									for (int v = start; v < end; v++)
										deform[v] = input.ReadFloat();
								} else {
									for (int v = start; v < end; v++)
										deform[v] = input.ReadFloat() * scale;
								}
								if (!weighted) {
									for (int v = 0, vn = deform.Length; v < vn; v++)
										deform[v] += vertices[v];
								}
							}

							timeline.SetFrame(frameIndex, time, deform);
							if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
						}
						timelines.Add(timeline);
						duration = Math.Max(duration, timeline.frames[frameCount - 1]);
					}
				}
			}

			// Draw order timeline.
			int drawOrderCount = input.ReadInt(true);
			if (drawOrderCount > 0) {
				DrawOrderTimeline timeline = new DrawOrderTimeline(drawOrderCount);
				int slotCount = skeletonData.slots.Count;
				for (int i = 0; i < drawOrderCount; i++) {
					float time = input.ReadFloat();
					int offsetCount = input.ReadInt(true);
					int[] drawOrder = new int[slotCount];
					for (int ii = slotCount - 1; ii >= 0; ii--)
						drawOrder[ii] = -1;
					int[] unchanged = new int[slotCount - offsetCount];
					int originalIndex = 0, unchangedIndex = 0;
					for (int ii = 0; ii < offsetCount; ii++) {
						int slotIndex = input.ReadInt(true);
						// Collect unchanged items.
						while (originalIndex != slotIndex)
							unchanged[unchangedIndex++] = originalIndex++;
						// Set changed items.
						drawOrder[originalIndex + input.ReadInt(true)] = originalIndex++;
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
			int eventCount = input.ReadInt(true);
			if (eventCount > 0) {
				EventTimeline timeline = new EventTimeline(eventCount);
				for (int i = 0; i < eventCount; i++) {
					float time = input.ReadFloat();
					EventData eventData = skeletonData.events.Items[input.ReadInt(true)];
					Event e = new Event(time, eventData) {
						Int = input.ReadInt(false),
						Float = input.ReadFloat(),
						String = input.ReadBoolean() ? input.ReadString() : eventData.String
					};
					if (e.data.AudioPath != null) {
						e.volume = input.ReadFloat();
						e.balance = input.ReadFloat();
					}
					timeline.SetFrame(i, e);
				}
				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[eventCount - 1]);
			}

			timelines.TrimExcess();
			return new Animation(name, timelines, duration);
		}

		private void ReadCurve (SkeletonInput input, int frameIndex, CurveTimeline timeline) {
			switch (input.ReadByte()) {
			case CURVE_STEPPED:
				timeline.SetStepped(frameIndex);
				break;
			case CURVE_BEZIER:
				timeline.SetCurve(frameIndex, input.ReadFloat(), input.ReadFloat(), input.ReadFloat(), input.ReadFloat());
				break;
			}
		}

		internal class Vertices
		{
			public int[] bones;
			public float[] vertices;
		}

		internal class SkeletonInput {
			private byte[] chars = new byte[32];
			internal ExposedList<String> strings;
			Stream input;

			public SkeletonInput (Stream input) {
				this.input = input;
			}

			public byte ReadByte () {
				return (byte)input.ReadByte();
			}

			public sbyte ReadSByte () {
				int value = input.ReadByte();
				if (value == -1) throw new EndOfStreamException();
				return (sbyte)value;
			}

			public bool ReadBoolean () {
				return input.ReadByte() != 0;
			}

			public float ReadFloat () {
				chars[3] = (byte)input.ReadByte();
				chars[2] = (byte)input.ReadByte();
				chars[1] = (byte)input.ReadByte();
				chars[0] = (byte)input.ReadByte();
				return BitConverter.ToSingle(chars, 0);
			}

			public int ReadInt () {
				return (input.ReadByte() << 24) + (input.ReadByte() << 16) + (input.ReadByte() << 8) + input.ReadByte();
			}

			public int ReadInt (bool optimizePositive) {
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

			public string ReadString () {
				int byteCount = ReadInt(true);
				switch (byteCount) {
				case 0:
					return null;
				case 1:
					return "";
				}
				byteCount--;
				byte[] buffer = this.chars;
				if (buffer.Length < byteCount) buffer = new byte[byteCount];
				ReadFully(buffer, 0, byteCount);
				return System.Text.Encoding.UTF8.GetString(buffer, 0, byteCount);
			}

			///<return>May be null.</return>
			public String ReadStringRef () {
				int index = ReadInt(true);
				return index == 0 ? null : strings.Items[index - 1];
			}

			public void ReadFully (byte[] buffer, int offset, int length) {
				while (length > 0) {
					int count = input.Read(buffer, offset, length);
					if (count <= 0) throw new EndOfStreamException();
					offset += count;
					length -= count;
				}
			}

			/// <summary>Returns the version string of binary skeleton data.</summary>
			public string GetVersionString () {
				try {
					// Hash.
					int byteCount = ReadInt(true);
					if (byteCount > 1) input.Position += byteCount - 1;

					// Version.
					byteCount = ReadInt(true);
					if (byteCount > 1) {
						byteCount--;
						var buffer = new byte[byteCount];
						ReadFully(buffer, 0, byteCount);
						return System.Text.Encoding.UTF8.GetString(buffer, 0, byteCount);
					}

					throw new ArgumentException("Stream does not contain a valid binary Skeleton Data.", "input");
				} catch (Exception e) {
					throw new ArgumentException("Stream does not contain a valid binary Skeleton Data.\n" + e, "input");
				}
			}
		}
	}
}
