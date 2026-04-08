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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

#if WINDOWS_STOREAPP
using System.Threading.Tasks;
using Windows.Storage;
#endif

namespace Spine {
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

	public class SkeletonBinary : SkeletonLoader {
		public const int BONE_ROTATE = 0;
		public const int BONE_TRANSLATE = 1;
		public const int BONE_TRANSLATEX = 2;
		public const int BONE_TRANSLATEY = 3;
		public const int BONE_SCALE = 4;
		public const int BONE_SCALEX = 5;
		public const int BONE_SCALEY = 6;
		public const int BONE_SHEAR = 7;
		public const int BONE_SHEARX = 8;
		public const int BONE_SHEARY = 9;
		public const int BONE_INHERIT = 10;

		public const int SLOT_ATTACHMENT = 0;
		public const int SLOT_RGBA = 1;
		public const int SLOT_RGB = 2;
		public const int SLOT_RGBA2 = 3;
		public const int SLOT_RGB2 = 4;
		public const int SLOT_ALPHA = 5;

		public const int CONSTRAINT_IK = 0;
		public const int CONSTRAINT_PATH = 1;
		public const int CONSTRAINT_TRANSFORM = 2;
		public const int CONSTRAINT_PHYSICS = 3;
		public const int CONSTRAINT_SLIDER = 4;

		public const int ATTACHMENT_DEFORM = 0;
		public const int ATTACHMENT_SEQUENCE = 1;

		public const int PATH_POSITION = 0;
		public const int PATH_SPACING = 1;
		public const int PATH_MIX = 2;

		public const int PHYSICS_INERTIA = 0;
		public const int PHYSICS_STRENGTH = 1;
		public const int PHYSICS_DAMPING = 2;
		public const int PHYSICS_MASS = 4;
		public const int PHYSICS_WIND = 5;
		public const int PHYSICS_GRAVITY = 6;
		public const int PHYSICS_MIX = 7;
		public const int PHYSICS_RESET = 8;

		public const int SLIDER_TIME = 0;
		public const int SLIDER_MIX = 1;

		public const int CURVE_LINEAR = 0;
		public const int CURVE_STEPPED = 1;
		public const int CURVE_BEZIER = 2;

		private readonly List<LinkedMesh> linkedMeshes = new List<LinkedMesh>();

		public SkeletonBinary (AttachmentLoader attachmentLoader)
			: base(attachmentLoader) {
		}

		public SkeletonBinary (params Atlas[] atlasArray)
			: base(atlasArray) {
		}

#if !ISUNITY && WINDOWS_STOREAPP
		private async Task<SkeletonData> ReadFile(string path) {
			var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
			using (BufferedStream input = new BufferedStream(await folder.GetFileAsync(path).AsTask().ConfigureAwait(false))) {
				SkeletonData skeletonData = ReadSkeletonData(input);
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
			using (BufferedStream input = new BufferedStream(Microsoft.Xna.Framework.TitleContainer.OpenStream(path))) {
#else
			using (FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
#endif
				try {
					SkeletonData skeletonData = ReadSkeletonData(input);
					skeletonData.name = Path.GetFileNameWithoutExtension(path);
					return skeletonData;
				} catch (Exception ex) {
					throw new SerializationException("Error reading binary skeleton file: " + path, ex);
				}
			}
		}
#endif // WINDOWS_STOREAPP

		/// <summary>Returns the version string of binary skeleton data.</summary>
		public static string GetVersionString (Stream file) {
			if (file == null) throw new ArgumentNullException("file");
			var input = new SkeletonInput(file);
			return input.GetVersionString();
		}

		public SkeletonData ReadSkeletonData (Stream file) {
			if (file == null) throw new ArgumentNullException("file");
			float scale = this.scale;

			var input = new SkeletonInput(file);
			var skeletonData = new SkeletonData();
			string version = null;
			try {
				long hash = input.ReadLong();
				skeletonData.hash = hash == 0 ? null : hash.ToString();
				skeletonData.version = input.ReadString();
				version = skeletonData.version;
				if (string.IsNullOrEmpty(skeletonData.version)) skeletonData.version = null;
				// early return for old 3.8 format instead of reading past the end
				if (skeletonData.version.Length > 13) return null;
				skeletonData.x = input.ReadFloat();
				skeletonData.y = input.ReadFloat();
				skeletonData.width = input.ReadFloat();
				skeletonData.height = input.ReadFloat();
				skeletonData.referenceScale = input.ReadFloat() * scale;

				bool nonessential = input.ReadBoolean();

				if (nonessential) {
					skeletonData.fps = input.ReadFloat();

					skeletonData.imagesPath = input.ReadString();
					if (string.IsNullOrEmpty(skeletonData.imagesPath)) skeletonData.imagesPath = null;

					skeletonData.audioPath = input.ReadString();
					if (string.IsNullOrEmpty(skeletonData.audioPath)) skeletonData.audioPath = null;
				}

				int n;
				object[] o;

				// Strings.
				o = input.strings = new String[n = input.ReadInt(true)];
				for (int i = 0; i < n; i++)
					o[i] = input.ReadString();

				// Bones.
				BoneData[] bones = skeletonData.bones.EnsureSize(n = input.ReadInt(true)).Items;
				for (int i = 0; i < n; i++) {
					string name = input.ReadString();
					BoneData parent = i == 0 ? null : bones[input.ReadInt(true)];
					var data = new BoneData(i, name, parent);
					BonePose setup = data.setupPose;
					setup.rotation = input.ReadFloat();
					setup.x = input.ReadFloat() * scale;
					setup.y = input.ReadFloat() * scale;
					setup.scaleX = input.ReadFloat();
					setup.scaleY = input.ReadFloat();
					setup.shearX = input.ReadFloat();
					setup.shearY = input.ReadFloat();
					setup.inherit = InheritEnum.Values[input.ReadSByte()];
					data.Length = input.ReadFloat() * scale;
					data.skinRequired = input.ReadBoolean();
					if (nonessential) { // discard non-essential data
						input.ReadInt(); // Color.rgba8888ToColor(data.color, input.readInt());
						input.ReadString(); // data.icon = input.readString();
						input.ReadBoolean(); // data.visible = input.readBoolean();
					}
					bones[i] = data;
				}

				// Slots.
				SlotData[] slots = skeletonData.slots.EnsureSize(n = input.ReadInt(true)).Items;
				for (int i = 0; i < n; i++) {
					string slotName = input.ReadString();

					BoneData boneData = bones[input.ReadInt(true)];
					var slotData = new SlotData(i, slotName, boneData);
					slotData.setupPose.SetColor(((uint)input.ReadInt()).RGBA8888ToColor());
					int darkColor = input.ReadInt(); // 0x00rrggbb
					if (darkColor != -1) {
						slotData.setupPose.SetDarkColor(((uint)darkColor).XRGB888ToColor());
					}

					slotData.attachmentName = input.ReadStringRef();
					slotData.blendMode = (BlendMode)input.ReadInt(true);
					if (nonessential) {
						input.ReadBoolean(); // data.visible = input.readBoolean(); data.path = path;
					}
					slots[i] = slotData;
				}

				// Constraints.
				int constraintCount = input.ReadInt(true);
				IConstraintData[] constraints = skeletonData.constraints.EnsureSize(constraintCount).Items;
				for (int i = 0; i < constraintCount; i++) {
					string name = input.ReadString();
					int nn;

					switch (input.ReadUByte()) {
					case CONSTRAINT_IK: {
						var data = new IkConstraintData(name);
						BoneData[] constraintBones = data.bones.EnsureSize(nn = input.ReadInt(true)).Items;
						for (int ii = 0; ii < nn; ii++)
							constraintBones[ii] = bones[input.ReadInt(true)];
						data.target = bones[input.ReadInt(true)];
						int flags = input.Read();
						data.skinRequired = (flags & 1) != 0;
						data.uniform = (flags & 2) != 0;
						IkConstraintPose setup = data.setupPose;
						setup.bendDirection = (flags & 4) != 0 ? -1 : 1;
						setup.compress = (flags & 8) != 0;
						setup.stretch = (flags & 16) != 0;
						if ((flags & 32) != 0) setup.mix = (flags & 64) != 0 ? input.ReadFloat() : 1;
						if ((flags & 128) != 0) setup.softness = input.ReadFloat() * scale;
						constraints[i] = data;
						break;
					}
					case CONSTRAINT_TRANSFORM: {
						var data = new TransformConstraintData(name);
						BoneData[] constraintBones = data.bones.EnsureSize(nn = input.ReadInt(true)).Items;
						for (int ii = 0; ii < nn; ii++)
							constraintBones[ii] = bones[input.ReadInt(true)];
						data.source = bones[input.ReadInt(true)];
						int flags = input.Read();
						data.skinRequired = (flags & 1) != 0;
						data.localSource = (flags & 2) != 0;
						data.localTarget = (flags & 4) != 0;
						data.additive = (flags & 8) != 0;
						data.clamp = (flags & 16) != 0;
						FromProperty[] froms = data.properties.EnsureSize(nn = flags >> 5).Items;
						for (int ii = 0, tn; ii < nn; ii++) {
							float fromScale = 1;
							FromProperty from;
							switch (input.ReadUByte()) {
							case 0: from = new FromRotate(); break;
							case 1: {
								from = new FromX();
								fromScale = scale;
								break;
							}
							case 2: {
								from = new FromY();
								fromScale = scale;
								break;
							}
							case 3: from = new FromScaleX(); break;
							case 4: from = new FromScaleY(); break;
							case 5: from = new FromShearY(); break;
							default: from = null; break;
							}
							from.offset = input.ReadFloat() * fromScale;
							ToProperty[] tos = from.to.EnsureSize(tn = input.ReadSByte()).Items;
							for (int t = 0; t < tn; t++) {
								float toScale = 1;
								ToProperty to;
								switch (input.ReadUByte()) {
								case 0: to = new ToRotate(); break;
								case 1: {
									to = new ToX();
									toScale = scale;
									break;
								}
								case 2: {
									to = new ToY();
									toScale = scale;
									break;
								}
								case 3: to = new ToScaleX(); break;
								case 4: to = new ToScaleY(); break;
								case 5: to = new ToShearY(); break;
								default: to = null; break;
								}
								to.offset = input.ReadFloat() * toScale;
								to.max = input.ReadFloat() * toScale;
								to.scale = input.ReadFloat() * (toScale / fromScale);
								tos[t] = to;
							}
							froms[ii] = from;
						}
						flags = input.Read();
						if ((flags & 1) != 0) data.offsets[TransformConstraintData.ROTATION] = input.ReadFloat();
						if ((flags & 2) != 0) data.offsets[TransformConstraintData.X] = input.ReadFloat() * scale;
						if ((flags & 4) != 0) data.offsets[TransformConstraintData.Y] = input.ReadFloat() * scale;
						if ((flags & 8) != 0) data.offsets[TransformConstraintData.SCALEX] = input.ReadFloat();
						if ((flags & 16) != 0) data.offsets[TransformConstraintData.SCALEY] = input.ReadFloat();
						if ((flags & 32) != 0) data.offsets[TransformConstraintData.SHEARY] = input.ReadFloat();
						flags = input.Read();
						TransformConstraintPose setup = data.setupPose;
						if ((flags & 1) != 0) setup.mixRotate = input.ReadFloat();
						if ((flags & 2) != 0) setup.mixX = input.ReadFloat();
						if ((flags & 4) != 0) setup.mixY = input.ReadFloat();
						if ((flags & 8) != 0) setup.mixScaleX = input.ReadFloat();
						if ((flags & 16) != 0) setup.mixScaleY = input.ReadFloat();
						if ((flags & 32) != 0) setup.mixShearY = input.ReadFloat();
						constraints[i] = data;
						break;
					}
					case CONSTRAINT_PATH: {
						var data = new PathConstraintData(name);
						BoneData[] constraintBones = data.bones.EnsureSize(nn = input.ReadInt(true)).Items;
						for (int ii = 0; ii < nn; ii++)
							constraintBones[ii] = bones[input.ReadInt(true)];
						data.slot = slots[input.ReadInt(true)];
						int flags = input.Read();
						data.skinRequired = (flags & 1) != 0;
						data.positionMode = (PositionMode)Enum.GetValues(typeof(PositionMode)).GetValue((flags >> 1) & 0x1); // 0b1
						data.spacingMode = (SpacingMode)Enum.GetValues(typeof(SpacingMode)).GetValue((flags >> 2) & 0x3); // 0b11
						data.rotateMode = (RotateMode)Enum.GetValues(typeof(RotateMode)).GetValue((flags >> 4) & 0x3); // 0b11
						if ((flags & 128) != 0) data.offsetRotation = input.ReadFloat();
						PathConstraintPose setup = data.setupPose;
						setup.position = input.ReadFloat();
						if (data.positionMode == PositionMode.Fixed) setup.position *= scale;
						setup.spacing = input.ReadFloat();
						if (data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed) setup.spacing *= scale;
						setup.mixRotate = input.ReadFloat();
						setup.mixX = input.ReadFloat();
						setup.mixY = input.ReadFloat();
						constraints[i] = data;
						break;
					}
					case CONSTRAINT_PHYSICS: {
						var data = new PhysicsConstraintData(name);
						data.bone = bones[input.ReadInt(true)];
						int flags = input.Read();
						data.skinRequired = (flags & 1) != 0;
						if ((flags & 2) != 0) data.x = input.ReadFloat();
						if ((flags & 4) != 0) data.y = input.ReadFloat();
						if ((flags & 8) != 0) data.rotate = input.ReadFloat();
						if ((flags & 16) != 0) data.scaleX = input.ReadFloat();
						if ((flags & 32) != 0) data.shearX = input.ReadFloat();
						data.limit = ((flags & 64) != 0 ? input.ReadFloat() : 5000) * scale;
						data.step = 1f / input.ReadUByte();
						PhysicsConstraintPose setup = data.setupPose;
						setup.inertia = input.ReadFloat();
						setup.strength = input.ReadFloat();
						setup.damping = input.ReadFloat();
						setup.massInverse = (flags & 128) != 0 ? input.ReadFloat() : 1;
						setup.wind = input.ReadFloat();
						setup.gravity = input.ReadFloat();
						flags = input.Read();
						if ((flags & 1) != 0) data.inertiaGlobal = true;
						if ((flags & 2) != 0) data.strengthGlobal = true;
						if ((flags & 4) != 0) data.dampingGlobal = true;
						if ((flags & 8) != 0) data.massGlobal = true;
						if ((flags & 16) != 0) data.windGlobal = true;
						if ((flags & 32) != 0) data.gravityGlobal = true;
						if ((flags & 64) != 0) data.mixGlobal = true;
						setup.mix = (flags & 128) != 0 ? input.ReadFloat() : 1;
						constraints[i] = data;
						break;
					}
					case CONSTRAINT_SLIDER: {
						var data = new SliderData(name);
						int flags = input.Read();
						data.skinRequired = (flags & 1) != 0;
						data.loop = (flags & 2) != 0;
						data.additive = (flags & 4) != 0;
						if ((flags & 8) != 0) data.setupPose.time = input.ReadFloat();
						if ((flags & 16) != 0) data.setupPose.mix = (flags & 32) != 0 ? input.ReadFloat() : 1;
						if ((flags & 64) != 0) {
							data.local = (flags & 128) != 0;
							data.bone = bones[input.ReadInt(true)];
							float offset = input.ReadFloat();
							float propertyScale = 1;
							switch (input.ReadUByte()) {
							case 0: data.property = new FromRotate(); break;
							case 1: {
								propertyScale = scale;
								data.property = new FromX();
								break;
							}
							case 2: {
								propertyScale = scale;
								data.property = new FromY();
								break;
							}
							case 3: data.property = new FromScaleX(); break;
							case 4: data.property = new FromScaleY(); break;
							case 5: data.property = new FromShearY(); break;
							default: data.property = null; break;
							}
							data.property.offset = offset * propertyScale;
							data.offset = input.ReadFloat();
							data.scale = input.ReadFloat() / propertyScale;
						}
						constraints[i] = data;
						break;
					}
					}
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
					o = skeletonData.skins.EnsureSize(n = i + input.ReadInt(true)).Items;
					for (; i < n; i++)
						o[i] = ReadSkin(input, skeletonData, false, nonessential);
				}

				// Linked meshes.
				n = linkedMeshes.Count;
				for (int i = 0; i < n; i++) {
					LinkedMesh linkedMesh = linkedMeshes[i];
					Skin skin = skeletonData.skins.Items[linkedMesh.skinIndex];
					Attachment parent = skin.GetAttachment(linkedMesh.slotIndex, linkedMesh.parent);
					if (parent == null) throw new Exception("Parent mesh not found: " + linkedMesh.parent);
					linkedMesh.mesh.TimelineAttachment = linkedMesh.inheritTimelines ? (VertexAttachment)parent : linkedMesh.mesh;
					linkedMesh.mesh.ParentMesh = (MeshAttachment)parent;
					linkedMesh.mesh.UpdateSequence();
				}
				linkedMeshes.Clear();

				// Events.
				o = skeletonData.events.EnsureSize(n = input.ReadInt(true)).Items;
				for (int i = 0; i < n; i++) {
					var data = new EventData(input.ReadString());
					Event setup = data.setupPose;
					setup.intValue = input.ReadInt(false);
					setup.floatValue = input.ReadFloat();
					setup.stringValue = input.ReadString();
					data.AudioPath = input.ReadString();
					if (data.AudioPath != null) {
						setup.volume = input.ReadFloat();
						setup.balance = input.ReadFloat();
					}
					o[i] = data;
				}

				// Animations.
				Animation[] animations = skeletonData.animations.EnsureSize(n = input.ReadInt(true)).Items;
				for (int i = 0; i < n; i++)
					animations[i] = ReadAnimation(input, input.ReadString(), skeletonData);

				for (int i = 0; i < constraintCount; i++) {
					SliderData data = constraints[i] as SliderData;
					if (data != null) data.animation = animations[input.ReadInt(true)];
				}
			} catch (IOException ex) {
				if (version != null) throw new SerializationException("Error reading binary skeleton data, version: " + version, ex);
				throw new SerializationException("Error binary skeleton data.", ex);
			} // note: no need to close Input as in reference implementation.

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
				skin = new Skin(input.ReadString());

				if (nonessential) input.ReadInt(); // discard, Color.rgba8888ToColor(skin.color, input.readInt());

				int n;
				object[] from = skeletonData.bones.Items, to = skin.bones.EnsureSize(n = input.ReadInt(true)).Items;
				for (int i = 0; i < n; i++)
					to[i] = from[input.ReadInt(true)];

				from = skeletonData.constraints.Items;
				to = skin.constraints.EnsureSize(n = input.ReadInt(true)).Items;
				for (int i = 0; i < n; i++)
					to[i] = from[input.ReadInt(true)];

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
			float scale = this.scale;

			int flags = input.ReadUByte();
			string name = (flags & 8) != 0 ? input.ReadStringRef() : attachmentName;

			switch ((AttachmentType)(flags & 0x7)) { // 0b111
			case AttachmentType.Region: {
				string path = (flags & 16) != 0 ? input.ReadStringRef() : null;
				uint color = (flags & 32) != 0 ? (uint)input.ReadInt() : 0xffffffff;
				Sequence sequence = ReadSequence(input, (flags & 64) != 0);
				float rotation = (flags & 128) != 0 ? input.ReadFloat() : 0;
				float x = input.ReadFloat();
				float y = input.ReadFloat();
				float scaleX = input.ReadFloat();
				float scaleY = input.ReadFloat();
				float width = input.ReadFloat();
				float height = input.ReadFloat();

				if (path == null) path = name;
				RegionAttachment region = attachmentLoader.NewRegionAttachment(skin, name, path, sequence);
				if (region == null) return null;
				region.Path = path;
				region.x = x * scale;
				region.y = y * scale;
				region.scaleX = scaleX;
				region.scaleY = scaleY;
				region.rotation = rotation;
				region.width = width * scale;
				region.height = height * scale;
				region.SetColor(color.RGBA8888ToColor());
				region.UpdateSequence();
				return region;
			}
			case AttachmentType.Boundingbox: {
				Vertices vertices = ReadVertices(input, (flags & 16) != 0);
				if (nonessential) input.ReadInt(); // discard, int color = nonessential ? input.readInt() : 0;

				BoundingBoxAttachment box = attachmentLoader.NewBoundingBoxAttachment(skin, name);
				if (box == null) return null;
				box.worldVerticesLength = vertices.length;
				box.vertices = vertices.vertices;
				box.bones = vertices.bones;
				// skipped porting: if (nonessential) Color.rgba8888ToColor(box.getColor(), color);
				return box;
			}
			case AttachmentType.Mesh: {
				string path = (flags & 16) != 0 ? input.ReadStringRef() : name;
				uint color = (flags & 32) != 0 ? (uint)input.ReadInt() : 0xffffffff;
				Sequence sequence = ReadSequence(input, (flags & 64) != 0);
				int hullLength = input.ReadInt(true);
				Vertices vertices = ReadVertices(input, (flags & 128) != 0);
				float[] uvs = ReadFloatArray(input, vertices.length, 1);
				int[] triangles = ReadShortArray(input, (vertices.length - hullLength - 2) * 3);

				int[] edges = null;
				float width = 0, height = 0;
				if (nonessential) {
					edges = ReadShortArray(input, input.ReadInt(true));
					width = input.ReadFloat();
					height = input.ReadFloat();
				}

				MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, name, path, sequence);
				if (mesh == null) return null;
				mesh.Path = path;
				mesh.SetColor(color.RGBA8888ToColor());
				mesh.HullLength = hullLength << 1;
				mesh.bones = vertices.bones;
				mesh.vertices = vertices.vertices;
				mesh.WorldVerticesLength = vertices.length;
				mesh.regionUVs = uvs;
				mesh.triangles = triangles;
				if (nonessential) {
					mesh.Edges = edges;
					mesh.Width = width * scale;
					mesh.Height = height * scale;
				}
				mesh.UpdateSequence();
				return mesh;
			}
			case AttachmentType.Linkedmesh: {
				string path = (flags & 16) != 0 ? input.ReadStringRef() : name;
				uint color = (flags & 32) != 0 ? (uint)input.ReadInt() : 0xffffffff;
				Sequence sequence = ReadSequence(input, (flags & 64) != 0);
				bool inheritTimelines = (flags & 128) != 0;
				int skinIndex = input.ReadInt(true);
				string parent = input.ReadStringRef();
				float width = 0, height = 0;
				if (nonessential) {
					width = input.ReadFloat();
					height = input.ReadFloat();
				}

				MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, name, path, sequence);
				if (mesh == null) return null;
				mesh.Path = path;
				mesh.SetColor(color.RGBA8888ToColor());
				if (nonessential) {
					mesh.Width = width * scale;
					mesh.Height = height * scale;
				}
				linkedMeshes.Add(new LinkedMesh(mesh, skinIndex, slotIndex, parent, inheritTimelines));
				return mesh;
			}
			case AttachmentType.Path: {
				bool closed = (flags & 16) != 0;
				bool constantSpeed = (flags & 32) != 0;
				Vertices vertices = ReadVertices(input, (flags & 64) != 0);
				var lengths = new float[vertices.length / 6];
				for (int i = 0, n = lengths.Length; i < n; i++)
					lengths[i] = input.ReadFloat() * scale;
				if (nonessential) input.ReadInt(); // discard, int color = nonessential ? input.ReadInt() : 0;

				PathAttachment path = attachmentLoader.NewPathAttachment(skin, name);
				if (path == null) return null;
				path.closed = closed;
				path.constantSpeed = constantSpeed;
				path.worldVerticesLength = vertices.length;
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
				if (nonessential) input.ReadInt(); // discard, int color = nonessential ? input.ReadInt() : 0;

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
				Vertices vertices = ReadVertices(input, (flags & 16) != 0);
				if (nonessential) input.ReadInt(); // discard, int color = nonessential ? input.readInt() : 0;

				ClippingAttachment clip = attachmentLoader.NewClippingAttachment(skin, name);
				if (clip == null) return null;
				clip.EndSlot = skeletonData.slots.Items[endSlotIndex];
				clip.worldVerticesLength = vertices.length;
				clip.vertices = vertices.vertices;
				clip.bones = vertices.bones;
				// skipped porting: if (nonessential) Color.rgba8888ToColor(clip.getColor(), color);
				return clip;
			}
			}
			return null;
		}

		private Sequence ReadSequence (SkeletonInput input, bool hasPathSuffix) {
			if (!hasPathSuffix) return new Sequence(1, false);
			var sequence = new Sequence(input.ReadInt(true), true);
			sequence.Start = input.ReadInt(true);
			sequence.Digits = input.ReadInt(true);
			sequence.SetupIndex = input.ReadInt(true);
			return sequence;
		}

		private Vertices ReadVertices (SkeletonInput input, bool weighted) {
			float scale = this.scale;
			int vertexCount = input.ReadInt(true);
			var vertices = new Vertices();
			vertices.length = vertexCount << 1;
			if (!weighted) {
				vertices.vertices = ReadFloatArray(input, vertices.length, scale);
				return vertices;
			}
			int n = input.ReadInt(true);
			var bones = new int[n];
			var weights = new float[(n - vertexCount) * 3];
			for (int b = 0, w = 0; b < n;) {
				int boneCount = input.ReadInt(true);
				bones[b++] = boneCount;
				for (int ii = 0; ii < boneCount; ii++, w += 3) {
					bones[b++] = input.ReadInt(true);
					weights[w] = input.ReadFloat() * scale;
					weights[w + 1] = input.ReadFloat() * scale;
					weights[w + 2] = input.ReadFloat();
				}
			}

			vertices.vertices = weights;
			vertices.bones = bones;
			return vertices;
		}

		private float[] ReadFloatArray (SkeletonInput input, int n, float scale) {
			var array = new float[n];
			if (scale == 1) {
				for (int i = 0; i < n; i++)
					array[i] = input.ReadFloat();
			} else {
				for (int i = 0; i < n; i++)
					array[i] = input.ReadFloat() * scale;
			}
			return array;
		}

		private int[] ReadShortArray (SkeletonInput input, int n) {
			var array = new int[n];
			for (int i = 0; i < n; i++)
				array[i] = input.ReadInt(true);
			return array;
		}

		/// <exception cref="SerializationException">SerializationException will be thrown when a Vertex attachment is not found.</exception>
		/// <exception cref="IOException">Throws IOException when a read operation fails.</exception>
		private Animation ReadAnimation (SkeletonInput input, string name, SkeletonData skeletonData) {
			var timelines = new ExposedList<Timeline>(input.ReadInt(true));
			float scale = this.scale;

			// Slot timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int slotIndex = input.ReadInt(true);
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int timelineType = input.ReadUByte(), frameCount = input.ReadInt(true), frameLast = frameCount - 1;
					switch (timelineType) {
					case SLOT_ATTACHMENT: {
						var timeline = new AttachmentTimeline(frameCount, slotIndex);
						for (int frame = 0; frame < frameCount; frame++)
							timeline.SetFrame(frame, input.ReadFloat(), input.ReadStringRef());
						timelines.Add(timeline);
						break;
					}
					case SLOT_RGBA: {
						var timeline = new RGBATimeline(frameCount, input.ReadInt(true), slotIndex);
						float time = input.ReadFloat();
						float r = input.Read() / 255f, g = input.Read() / 255f;
						float b = input.Read() / 255f, a = input.Read() / 255f;
						for (int frame = 0, bezier = 0; ; frame++) {
							timeline.SetFrame(frame, time, r, g, b, a);
							if (frame == frameLast) break;
							float time2 = input.ReadFloat();
							float r2 = input.Read() / 255f, g2 = input.Read() / 255f;
							float b2 = input.Read() / 255f, a2 = input.Read() / 255f;
							switch (input.ReadUByte()) {
							case CURVE_STEPPED:
								timeline.SetStepped(frame);
								break;
							case CURVE_BEZIER:
								SetBezier(input, timeline, bezier++, frame, 0, time, time2, r, r2, 1);
								SetBezier(input, timeline, bezier++, frame, 1, time, time2, g, g2, 1);
								SetBezier(input, timeline, bezier++, frame, 2, time, time2, b, b2, 1);
								SetBezier(input, timeline, bezier++, frame, 3, time, time2, a, a2, 1);
								break;
							}
							time = time2;
							r = r2;
							g = g2;
							b = b2;
							a = a2;
						}
						timelines.Add(timeline);
						break;
					}
					case SLOT_RGB: {
						var timeline = new RGBTimeline(frameCount, input.ReadInt(true), slotIndex);
						float time = input.ReadFloat();
						float r = input.Read() / 255f, g = input.Read() / 255f, b = input.Read() / 255f;
						for (int frame = 0, bezier = 0; ; frame++) {
							timeline.SetFrame(frame, time, r, g, b);
							if (frame == frameLast) break;
							float time2 = input.ReadFloat();
							float r2 = input.Read() / 255f, g2 = input.Read() / 255f, b2 = input.Read() / 255f;
							switch (input.ReadUByte()) {
							case CURVE_STEPPED:
								timeline.SetStepped(frame);
								break;
							case CURVE_BEZIER:
								SetBezier(input, timeline, bezier++, frame, 0, time, time2, r, r2, 1);
								SetBezier(input, timeline, bezier++, frame, 1, time, time2, g, g2, 1);
								SetBezier(input, timeline, bezier++, frame, 2, time, time2, b, b2, 1);
								break;
							}
							time = time2;
							r = r2;
							g = g2;
							b = b2;
						}
						timelines.Add(timeline);
						break;
					}
					case SLOT_RGBA2: {
						var timeline = new RGBA2Timeline(frameCount, input.ReadInt(true), slotIndex);
						float time = input.ReadFloat();
						float r = input.Read() / 255f, g = input.Read() / 255f;
						float b = input.Read() / 255f, a = input.Read() / 255f;
						float r2 = input.Read() / 255f, g2 = input.Read() / 255f, b2 = input.Read() / 255f;
						for (int frame = 0, bezier = 0; ; frame++) {
							timeline.SetFrame(frame, time, r, g, b, a, r2, g2, b2);
							if (frame == frameLast) break;
							float time2 = input.ReadFloat();
							float nr = input.Read() / 255f, ng = input.Read() / 255f;
							float nb = input.Read() / 255f, na = input.Read() / 255f;
							float nr2 = input.Read() / 255f, ng2 = input.Read() / 255f, nb2 = input.Read() / 255f;
							switch (input.ReadUByte()) {
							case CURVE_STEPPED:
								timeline.SetStepped(frame);
								break;
							case CURVE_BEZIER:
								SetBezier(input, timeline, bezier++, frame, 0, time, time2, r, nr, 1);
								SetBezier(input, timeline, bezier++, frame, 1, time, time2, g, ng, 1);
								SetBezier(input, timeline, bezier++, frame, 2, time, time2, b, nb, 1);
								SetBezier(input, timeline, bezier++, frame, 3, time, time2, a, na, 1);
								SetBezier(input, timeline, bezier++, frame, 4, time, time2, r2, nr2, 1);
								SetBezier(input, timeline, bezier++, frame, 5, time, time2, g2, ng2, 1);
								SetBezier(input, timeline, bezier++, frame, 6, time, time2, b2, nb2, 1);
								break;
							}
							time = time2;
							r = nr;
							g = ng;
							b = nb;
							a = na;
							r2 = nr2;
							g2 = ng2;
							b2 = nb2;
						}
						timelines.Add(timeline);
						break;
					}
					case SLOT_RGB2: {
						var timeline = new RGB2Timeline(frameCount, input.ReadInt(true), slotIndex);
						float time = input.ReadFloat();
						float r = input.Read() / 255f, g = input.Read() / 255f, b = input.Read() / 255f;
						float r2 = input.Read() / 255f, g2 = input.Read() / 255f, b2 = input.Read() / 255f;
						for (int frame = 0, bezier = 0; ; frame++) {
							timeline.SetFrame(frame, time, r, g, b, r2, g2, b2);
							if (frame == frameLast) break;
							float time2 = input.ReadFloat();
							float nr = input.Read() / 255f, ng = input.Read() / 255f, nb = input.Read() / 255f;
							float nr2 = input.Read() / 255f, ng2 = input.Read() / 255f, nb2 = input.Read() / 255f;
							switch (input.ReadUByte()) {
							case CURVE_STEPPED:
								timeline.SetStepped(frame);
								break;
							case CURVE_BEZIER:
								SetBezier(input, timeline, bezier++, frame, 0, time, time2, r, nr, 1);
								SetBezier(input, timeline, bezier++, frame, 1, time, time2, g, ng, 1);
								SetBezier(input, timeline, bezier++, frame, 2, time, time2, b, nb, 1);
								SetBezier(input, timeline, bezier++, frame, 3, time, time2, r2, nr2, 1);
								SetBezier(input, timeline, bezier++, frame, 4, time, time2, g2, ng2, 1);
								SetBezier(input, timeline, bezier++, frame, 5, time, time2, b2, nb2, 1);
								break;
							}
							time = time2;
							r = nr;
							g = ng;
							b = nb;
							r2 = nr2;
							g2 = ng2;
							b2 = nb2;
						}
						timelines.Add(timeline);
						break;
					}
					case SLOT_ALPHA: {
						var timeline = new AlphaTimeline(frameCount, input.ReadInt(true), slotIndex);
						float time = input.ReadFloat(), a = input.Read() / 255f;
						for (int frame = 0, bezier = 0; ; frame++) {
							timeline.SetFrame(frame, time, a);
							if (frame == frameLast) break;
							float time2 = input.ReadFloat();
							float a2 = input.Read() / 255f;
							switch (input.ReadUByte()) {
							case CURVE_STEPPED:
								timeline.SetStepped(frame);
								break;
							case CURVE_BEZIER:
								SetBezier(input, timeline, bezier++, frame, 0, time, time2, a, a2, 1);
								break;
							}
							time = time2;
							a = a2;
						}
						timelines.Add(timeline);
						break;
					}
					}
				}
			}

			// Bone timelines.
			int boneCount = input.ReadInt(true);
			var bones = new ExposedList<int>(boneCount);
			for (int i = 0; i < boneCount; i++) {
				int boneIndex = input.ReadInt(true);
				bones.Add(boneIndex);
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int type = input.ReadUByte(), frameCount = input.ReadInt(true);
					if (type == BONE_INHERIT) {
						var timeline = new InheritTimeline(frameCount, boneIndex);
						for (int frame = 0; frame < frameCount; frame++)
							timeline.SetFrame(frame, input.ReadFloat(), InheritEnum.Values[input.ReadUByte()]);
						timelines.Add(timeline);
						continue;
					}
					int bezierCount = input.ReadInt(true);
					switch (type) {
					case BONE_ROTATE:
						ReadTimeline(input, timelines, new RotateTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					case BONE_TRANSLATE:
						ReadTimeline(input, timelines, new TranslateTimeline(frameCount, bezierCount, boneIndex), scale);
						break;
					case BONE_TRANSLATEX:
						ReadTimeline(input, timelines, new TranslateXTimeline(frameCount, bezierCount, boneIndex), scale);
						break;
					case BONE_TRANSLATEY:
						ReadTimeline(input, timelines, new TranslateYTimeline(frameCount, bezierCount, boneIndex), scale);
						break;
					case BONE_SCALE:
						ReadTimeline(input, timelines, new ScaleTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					case BONE_SCALEX:
						ReadTimeline(input, timelines, new ScaleXTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					case BONE_SCALEY:
						ReadTimeline(input, timelines, new ScaleYTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					case BONE_SHEAR:
						ReadTimeline(input, timelines, new ShearTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					case BONE_SHEARX:
						ReadTimeline(input, timelines, new ShearXTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					case BONE_SHEARY:
						ReadTimeline(input, timelines, new ShearYTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					}
				}
			}

			// IK constraint timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int index = input.ReadInt(true), frameCount = input.ReadInt(true), frameLast = frameCount - 1;
				var timeline = new IkConstraintTimeline(frameCount, input.ReadInt(true), index);
				int flags = input.Read();
				float time = input.ReadFloat(), mix = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.ReadFloat() : 1) : 0;
				float softness = (flags & 4) != 0 ? input.ReadFloat() * scale : 0;
				for (int frame = 0, bezier = 0; ; frame++) {
					timeline.SetFrame(frame, time, mix, softness, (flags & 8) != 0 ? 1 : -1, (flags & 16) != 0, (flags & 32) != 0);

					if (frame == frameLast) break;
					flags = input.Read();
					float time2 = input.ReadFloat(), mix2 = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.ReadFloat() : 1) : 0;
					float softness2 = (flags & 4) != 0 ? input.ReadFloat() * scale : 0;
					if ((flags & 64) != 0)
						timeline.SetStepped(frame);
					else if ((flags & 128) != 0) {
						SetBezier(input, timeline, bezier++, frame, 0, time, time2, mix, mix2, 1);
						SetBezier(input, timeline, bezier++, frame, 1, time, time2, softness, softness2, scale);
					}
					time = time2;
					mix = mix2;
					softness = softness2;
				}
				timelines.Add(timeline);
			}

			// Transform constraint timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int index = input.ReadInt(true), frameCount = input.ReadInt(true), frameLast = frameCount - 1;
				var timeline = new TransformConstraintTimeline(frameCount, input.ReadInt(true), index);
				float time = input.ReadFloat(), mixRotate = input.ReadFloat(), mixX = input.ReadFloat(), mixY = input.ReadFloat(),
				mixScaleX = input.ReadFloat(), mixScaleY = input.ReadFloat(), mixShearY = input.ReadFloat();
				for (int frame = 0, bezier = 0; ; frame++) {
					timeline.SetFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
					if (frame == frameLast) break;
					float time2 = input.ReadFloat(), mixRotate2 = input.ReadFloat(), mixX2 = input.ReadFloat(), mixY2 = input.ReadFloat(),
					mixScaleX2 = input.ReadFloat(), mixScaleY2 = input.ReadFloat(), mixShearY2 = input.ReadFloat();
					switch (input.ReadUByte()) {
					case CURVE_STEPPED:
						timeline.SetStepped(frame);
						break;
					case CURVE_BEZIER:
						SetBezier(input, timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
						SetBezier(input, timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
						SetBezier(input, timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
						SetBezier(input, timeline, bezier++, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
						SetBezier(input, timeline, bezier++, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
						SetBezier(input, timeline, bezier++, frame, 5, time, time2, mixShearY, mixShearY2, 1);
						break;
					}
					time = time2;
					mixRotate = mixRotate2;
					mixX = mixX2;
					mixY = mixY2;
					mixScaleX = mixScaleX2;
					mixScaleY = mixScaleY2;
					mixShearY = mixShearY2;
				}
				timelines.Add(timeline);
			}

			// Path constraint timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int index = input.ReadInt(true);
				var data = (PathConstraintData)skeletonData.constraints.Items[index];
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int type = input.ReadUByte(), frameCount = input.ReadInt(true), bezierCount = input.ReadInt(true);
					switch (type) {
					case PATH_POSITION: {
						ReadTimeline(input, timelines, new PathConstraintPositionTimeline(frameCount, bezierCount, index),
							data.positionMode == PositionMode.Fixed ? scale : 1);
						break;
					}
					case PATH_SPACING: {
						ReadTimeline(input, timelines, new PathConstraintSpacingTimeline(frameCount, bezierCount, index),
							data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed ? scale : 1);
						break;
					}
					case PATH_MIX: {
						var timeline = new PathConstraintMixTimeline(frameCount, bezierCount, index);
						float time = input.ReadFloat(), mixRotate = input.ReadFloat(), mixX = input.ReadFloat(), mixY = input.ReadFloat();
						for (int frame = 0, bezier = 0, frameLast = timeline.FrameCount - 1; ; frame++) {
							timeline.SetFrame(frame, time, mixRotate, mixX, mixY);
							if (frame == frameLast) break;
							float time2 = input.ReadFloat(), mixRotate2 = input.ReadFloat(), mixX2 = input.ReadFloat(),
								mixY2 = input.ReadFloat();
							switch (input.ReadUByte()) {
							case CURVE_STEPPED:
								timeline.SetStepped(frame);
								break;
							case CURVE_BEZIER:
								SetBezier(input, timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
								SetBezier(input, timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
								SetBezier(input, timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
								break;
							}
							time = time2;
							mixRotate = mixRotate2;
							mixX = mixX2;
							mixY = mixY2;
						}
						timelines.Add(timeline);
						break;
					}
					}
				}
			}

			// Physics timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int index = input.ReadInt(true) - 1;
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int type = input.ReadUByte(), frameCount = input.ReadInt(true);
					if (type == PHYSICS_RESET) {
						var timeline = new PhysicsConstraintResetTimeline(frameCount, index);
						for (int frame = 0; frame < frameCount; frame++)
							timeline.SetFrame(frame, input.ReadFloat());
						timelines.Add(timeline);
						continue;
					}
					int bezierCount = input.ReadInt(true);
					PhysicsConstraintTimeline newTimeline;
					switch (type) {
					case PHYSICS_INERTIA:
						newTimeline = new PhysicsConstraintInertiaTimeline(frameCount, bezierCount, index);
						break;
					case PHYSICS_STRENGTH:
						newTimeline = new PhysicsConstraintStrengthTimeline(frameCount, bezierCount, index);
						break;
					case PHYSICS_DAMPING:
						newTimeline = new PhysicsConstraintDampingTimeline(frameCount, bezierCount, index);
						break;
					case PHYSICS_MASS:
						newTimeline = new PhysicsConstraintMassTimeline(frameCount, bezierCount, index);
						break;
					case PHYSICS_WIND:
						newTimeline = new PhysicsConstraintWindTimeline(frameCount, bezierCount, index);
						break;
					case PHYSICS_GRAVITY:
						newTimeline = new PhysicsConstraintGravityTimeline(frameCount, bezierCount, index);
						break;
					case PHYSICS_MIX:
						newTimeline = new PhysicsConstraintMixTimeline(frameCount, bezierCount, index);
						break;
					default:
						throw new SerializationException();
					}
					ReadTimeline(input, timelines, newTimeline, 1);
				}
			}

			// Slider timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int index = input.ReadInt(true);
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int type = input.ReadSByte(), frameCount = input.ReadInt(true), bezierCount = input.ReadInt(true);
					ConstraintTimeline1 newTimeline;
					switch (type) {
					case SLIDER_TIME: newTimeline = new SliderTimeline(frameCount, bezierCount, index); break;
					case SLIDER_MIX: newTimeline = new SliderMixTimeline(frameCount, bezierCount, index); break;
					default: throw new SerializationException();
					}
					ReadTimeline(input, timelines, newTimeline, 1);
				}
			}

			// Attachment timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				Skin skin = skeletonData.skins.Items[input.ReadInt(true)];
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int slotIndex = input.ReadInt(true);
					for (int iii = 0, nnn = input.ReadInt(true); iii < nnn; iii++) {
						string attachmentName = input.ReadStringRef();
						Attachment attachment = skin.GetAttachment(slotIndex, attachmentName);
						if (attachment == null) throw new SerializationException("Timeline attachment not found: " + attachmentName);

						int timelineType = input.ReadUByte(), frameCount = input.ReadInt(true), frameLast = frameCount - 1;
						switch (timelineType) {
						case ATTACHMENT_DEFORM: {
							var vertexAttachment = (VertexAttachment)attachment;
							bool weighted = vertexAttachment.Bones != null;
							float[] vertices = vertexAttachment.Vertices;
							int deformLength = weighted ? (vertices.Length / 3) << 1 : vertices.Length;

							var timeline = new DeformTimeline(frameCount, input.ReadInt(true), slotIndex, vertexAttachment);

							float time = input.ReadFloat();
							for (int frame = 0, bezier = 0; ; frame++) {
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
								timeline.SetFrame(frame, time, deform);
								if (frame == frameLast) break;
								float time2 = input.ReadFloat();
								switch (input.ReadUByte()) {
								case CURVE_STEPPED:
									timeline.SetStepped(frame);
									break;
								case CURVE_BEZIER:
									SetBezier(input, timeline, bezier++, frame, 0, time, time2, 0, 1, 1);
									break;
								}
								time = time2;
							}
							timelines.Add(timeline);
							break;
						}
						case ATTACHMENT_SEQUENCE: {
							var timeline = new SequenceTimeline(frameCount, slotIndex, attachment);
							for (int frame = 0; frame < frameCount; frame++) {
								float time = input.ReadFloat();
								int modeAndIndex = input.ReadInt();
								timeline.SetFrame(frame, time, (SequenceMode)(modeAndIndex & 0xf), modeAndIndex >> 4,
									input.ReadFloat());
							}
							timelines.Add(timeline);
							break;
						} // end case
						} // end switch
					}
				}
			}

			// Draw order timeline.
			int slotCount = skeletonData.slots.Count;
			int drawOrderCount = input.ReadInt(true);
			if (drawOrderCount > 0) {
				var timeline = new DrawOrderTimeline(drawOrderCount);
				for (int i = 0; i < drawOrderCount; i++)
					timeline.SetFrame(i, input.ReadFloat(), ReadDrawOrder(input, slotCount));
				timelines.Add(timeline);
			}

			// Draw order folder timelines.
			int folderCount = input.ReadInt(true);
			for (int i = 0; i < folderCount; i++) {
				int folderSlotCount = input.ReadInt(true);
				var folderSlots = new int[folderSlotCount];
				for (int ii = 0; ii < folderSlotCount; ii++)
					folderSlots[ii] = input.ReadInt(true);
				int keyCount = input.ReadInt(true);
				var timeline = new DrawOrderFolderTimeline(keyCount, folderSlots, slotCount);
				for (int ii = 0; ii < keyCount; ii++)
					timeline.SetFrame(ii, input.ReadFloat(), ReadDrawOrder(input, folderSlotCount));
				timelines.Add(timeline);
			}

			// Event timeline.
			int eventCount = input.ReadInt(true);
			if (eventCount > 0) {
				var timeline = new EventTimeline(eventCount);
				for (int i = 0; i < eventCount; i++) {
					float time = input.ReadFloat();
					EventData eventData = skeletonData.events.Items[input.ReadInt(true)];
					var e = new Event(time, eventData);
					e.intValue = input.ReadInt(false);
					e.floatValue = input.ReadFloat();
					e.stringValue = input.ReadString();
					if (e.stringValue == null) e.stringValue = eventData.setupPose.stringValue;
					if (e.data.AudioPath != null) {
						e.volume = input.ReadFloat();
						e.balance = input.ReadFloat();
					}
					timeline.SetFrame(i, e);
				}
				timelines.Add(timeline);
			}

			float duration = 0;
			Timeline[] items = timelines.Items;
			for (int i = 0, n = timelines.Count; i < n; i++)
				duration = Math.Max(duration, items[i].Duration);

			Animation animation = new Animation(name);
			animation.SetTimelines(timelines, bones);
			animation.Duration = duration;
			return animation;
		}

		/// <exception cref="IOException">Throws IOException when a read operation fails.</exception>
		private void ReadTimeline (SkeletonInput input, ExposedList<Timeline> timelines, CurveTimeline1 timeline, float scale) {
			float time = input.ReadFloat(), value = input.ReadFloat() * scale;
			for (int frame = 0, bezier = 0, frameLast = timeline.FrameCount - 1; ; frame++) {
				timeline.SetFrame(frame, time, value);
				if (frame == frameLast) break;
				float time2 = input.ReadFloat(), value2 = input.ReadFloat() * scale;
				switch (input.ReadUByte()) {
				case CURVE_STEPPED:
					timeline.SetStepped(frame);
					break;
				case CURVE_BEZIER:
					SetBezier(input, timeline, bezier++, frame, 0, time, time2, value, value2, scale);
					break;
				}
				time = time2;
				value = value2;
			}
			timelines.Add(timeline);
		}

		/// <exception cref="IOException">Throws IOException when a read operation fails.</exception>
		private void ReadTimeline (SkeletonInput input, ExposedList<Timeline> timelines, BoneTimeline2 timeline, float scale) {
			float time = input.ReadFloat(), value1 = input.ReadFloat() * scale, value2 = input.ReadFloat() * scale;
			for (int frame = 0, bezier = 0, frameLast = timeline.FrameCount - 1; ; frame++) {
				timeline.SetFrame(frame, time, value1, value2);
				if (frame == frameLast) break;
				float time2 = input.ReadFloat(), nvalue1 = input.ReadFloat() * scale, nvalue2 = input.ReadFloat() * scale;
				switch (input.ReadUByte()) {
				case CURVE_STEPPED:
					timeline.SetStepped(frame);
					break;
				case CURVE_BEZIER:
					SetBezier(input, timeline, bezier++, frame, 0, time, time2, value1, nvalue1, scale);
					SetBezier(input, timeline, bezier++, frame, 1, time, time2, value2, nvalue2, scale);
					break;
				}
				time = time2;
				value1 = nvalue1;
				value2 = nvalue2;
			}
			timelines.Add(timeline);
		}

		/// <exception cref="IOException">Throws IOException when a read operation fails.</exception>
		private int[] ReadDrawOrder (SkeletonInput input, int slotCount) {
			int changeCount = input.ReadInt(true);
			if (changeCount == 0) return null;
			var drawOrder = new int[slotCount];
			for (int ii = slotCount - 1; ii >= 0; ii--)
				drawOrder[ii] = -1;
			var unchanged = new int[slotCount - changeCount];
			int originalIndex = 0, unchangedIndex = 0;
			for (int i = 0; i < changeCount; i++) {
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
			for (int i = slotCount - 1; i >= 0; i--)
				if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
			return drawOrder;
		}

		/// <exception cref="IOException">Throws IOException when a read operation fails.</exception>
		void SetBezier (SkeletonInput input, CurveTimeline timeline, int bezier, int frame, int value, float time1, float time2,
			float value1, float value2, float scale) {
			timeline.SetBezier(bezier, frame, value, time1, value1, input.ReadFloat(), input.ReadFloat() * scale, input.ReadFloat(),
					input.ReadFloat() * scale, time2, value2);
		}

		internal class Vertices {
			public int length;
			public int[] bones;
			public float[] vertices;
		}

		internal class SkeletonInput {
			private byte[] chars = new byte[32];
			private byte[] bytesBigEndian = new byte[8];
			internal string[] strings;
			Stream input;

			public SkeletonInput (Stream input) {
				this.input = input;
			}

			public int Read () {
				return input.ReadByte();
			}

			/// <summary>Explicit unsigned byte variant to prevent pitfalls porting Java reference implementation
			/// where byte is signed vs C# where byte is unsigned.</summary>
			public byte ReadUByte () {
				return (byte)input.ReadByte();
			}

			/// <summary>Explicit signed byte variant to prevent pitfalls porting Java reference implementation
			/// where byte is signed vs C# where byte is unsigned.</summary>
			public sbyte ReadSByte () {
				int value = input.ReadByte();
				if (value == -1) throw new EndOfStreamException();
				return (sbyte)value;
			}

			public bool ReadBoolean () {
				return input.ReadByte() != 0;
			}

			public float ReadFloat () {
				input.Read(bytesBigEndian, 0, 4);
				chars[3] = bytesBigEndian[0];
				chars[2] = bytesBigEndian[1];
				chars[1] = bytesBigEndian[2];
				chars[0] = bytesBigEndian[3];
				return BitConverter.ToSingle(chars, 0);
			}

			public int ReadInt () {
				input.Read(bytesBigEndian, 0, 4);
				return (bytesBigEndian[0] << 24)
					+ (bytesBigEndian[1] << 16)
					+ (bytesBigEndian[2] << 8)
					+ bytesBigEndian[3];
			}

			public long ReadLong () {
				input.Read(bytesBigEndian, 0, 8);
				return ((long)(bytesBigEndian[0]) << 56)
					+ ((long)(bytesBigEndian[1]) << 48)
					+ ((long)(bytesBigEndian[2]) << 40)
					+ ((long)(bytesBigEndian[3]) << 32)
					+ ((long)(bytesBigEndian[4]) << 24)
					+ ((long)(bytesBigEndian[5]) << 16)
					+ ((long)(bytesBigEndian[6]) << 8)
					+ (long)(bytesBigEndian[7]);
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
				return optimizePositive ? result : ((int)((uint)result >> 1) ^ -(result & 1));
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

			/// <return>May be null.</return>
			public String ReadStringRef () {
				int index = ReadInt(true);
				return index == 0 ? null : strings[index - 1];
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
					// try reading 4.0+ format
					long initialPosition = input.Position;
					ReadLong(); // long hash

					long stringPosition = input.Position;
					int stringByteCount = ReadInt(true);
					input.Position = stringPosition;
					if (stringByteCount <= 13) {
						string version = ReadString();
						if (char.IsDigit(version[0]))
							return version;
					}
					// fallback to old version format
					input.Position = initialPosition;
					return GetVersionStringOld3X();
				} catch (Exception e) {
					throw new ArgumentException("Stream does not contain valid binary Skeleton Data.\n" + e, "input");
				}
			}

			/// <summary>Returns old 3.8 and earlier format version string of binary skeleton data.</summary>
			public string GetVersionStringOld3X () {
				// Hash.
				int byteCount = ReadInt(true);
				if (byteCount > 1) input.Position += byteCount - 1;

				// Version.
				byteCount = ReadInt(true);
				if (byteCount > 1 && byteCount <= 13) {
					byteCount--;
					var buffer = new byte[byteCount];
					ReadFully(buffer, 0, byteCount);
					return System.Text.Encoding.UTF8.GetString(buffer, 0, byteCount);
				}
				throw new ArgumentException("Stream does not contain valid binary Skeleton Data.");
			}
		}

		private class LinkedMesh {
			internal string parent;
			internal int skinIndex, slotIndex;
			internal MeshAttachment mesh;
			internal bool inheritTimelines;

			public LinkedMesh (MeshAttachment mesh, int skinIndex, int slotIndex, string parent, bool inheritTimelines) {
				this.mesh = mesh;
				this.skinIndex = skinIndex;
				this.slotIndex = slotIndex;
				this.parent = parent;
				this.inheritTimelines = inheritTimelines;
			}
		}
	}
}
