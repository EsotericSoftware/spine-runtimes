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

namespace Spine {
#if IS_UNITY
	using Color32F = UnityEngine.Color;
#endif

	/// <summary>Attachment that displays a texture region.</summary>
	public class RegionAttachment : Attachment, IHasSequence {
		public const int BLX = 0, BLY = 1;
		public const int ULX = 2, ULY = 3;
		public const int URX = 4, URY = 5;
		public const int BRX = 6, BRY = 7;

		internal readonly Sequence sequence;
		internal float x, y, rotation, scaleX = 1, scaleY = 1, width, height;
		// Color is a struct, set to protected to prevent
		// Color color = slot.color; color.a = 0.5;
		// modifying just a copy of the struct instead of the original
		// object as in reference implementation.
		protected Color32F color = new Color32F(1, 1, 1, 1);

		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }
		/// <summary>The local rotation in degrees, counter clockwise.</summary>
		public float Rotation { get { return rotation; } set { rotation = value; } }
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }
		public float ScaleY { get { return scaleY; } set { scaleY = value; } }
		public float Width { get { return width; } set { width = value; } }
		public float Height { get { return height; } set { height = value; } }

		public Color32F GetColor () {
			return color;
		}

		public void SetColor (Color32F color) {
			this.color = color;
		}

		public void SetColor (float r, float g, float b, float a) {
			color = new Color32F(r, g, b, a);
		}

		public string Path { get; set; }
		public Sequence Sequence { get { return sequence; } }

		public RegionAttachment (string name, Sequence sequence)
			: base(name) {
			if (sequence == null) throw new ArgumentException("sequence cannot be null.", "sequence");
			this.sequence = sequence;
		}

		/// <summary>Copy constructor.</summary>
		public RegionAttachment (RegionAttachment other)
			: base(other) {
			Path = other.Path;
			x = other.x;
			y = other.y;
			scaleX = other.scaleX;
			scaleY = other.scaleY;
			rotation = other.rotation;
			width = other.width;
			height = other.height;
			color = other.color;
			sequence = new Sequence(other.sequence);
		}

		/// <summary><para>
		/// Transforms the attachment's four vertices to world coordinates. If the attachment has a <see cref="Sequence"/> the region may
		/// be changed.</para>
		/// <para>
		/// See <see href='https://esotericsoftware.com/spine-runtime-skeletons#World-transforms'>World transforms</a> in the Spine
		/// Runtimes Guide.</para></summary>
		/// <param name="worldVertices">The output world vertices. Must have a length greater than or equal to offset + 8.</param>
		/// <param name="vertexOffsets">The vertex <see cref="Sequence.GetOffsets(int)">offsets</see>.</param>
		/// <param name="offset">The worldVertices index to begin writing values.</param>
		/// <param name="stride">The number of worldVertices entries between the value pairs written.</param>
		public void ComputeWorldVertices (Slot slot, float[] vertexOffsets, float[] worldVertices, int offset, int stride = 2) {
			BonePose bone = slot.Bone.AppliedPose;
			float bwx = bone.worldX, bwy = bone.worldY;
			float a = bone.a, b = bone.b, c = bone.c, d = bone.d;

			// Vertex order is different from RegionAttachment.java
			float offsetX = vertexOffsets[BRX]; // 0
			float offsetY = vertexOffsets[BRY]; // 1
			worldVertices[offset] = offsetX * a + offsetY * b + bwx; // bl
			worldVertices[offset + 1] = offsetX * c + offsetY * d + bwy;
			offset += stride;

			offsetX = vertexOffsets[BLX]; // 2
			offsetY = vertexOffsets[BLY]; // 3
			worldVertices[offset] = offsetX * a + offsetY * b + bwx; // ul
			worldVertices[offset + 1] = offsetX * c + offsetY * d + bwy;
			offset += stride;

			offsetX = vertexOffsets[ULX]; // 4
			offsetY = vertexOffsets[ULY]; // 5
			worldVertices[offset] = offsetX * a + offsetY * b + bwx; // ur
			worldVertices[offset + 1] = offsetX * c + offsetY * d + bwy;
			offset += stride;

			offsetX = vertexOffsets[URX]; // 6
			offsetY = vertexOffsets[URY]; // 7
			worldVertices[offset] = offsetX * a + offsetY * b + bwx; // br
			worldVertices[offset + 1] = offsetX * c + offsetY * d + bwy;
			//offset += stride;
		}

		/// <summary>
		/// Returns the vertex <see cref="Sequence.GetOffsets(int)">offsets</see> for the specified slot pose.
		/// </summary>
		public float[] GetOffsets (SlotPose pose) {
			return sequence.GetOffsets(sequence.ResolveIndex(pose));
		}

		public void UpdateSequence () {
			sequence.Update(this);
		}

		public override Attachment Copy () {
			return new RegionAttachment(this);
		}

		/// <summary>
		/// Computes <see cref="Sequence.GetUVs(int)">UVs</see> and <see cref="Sequence.GetOffsets(int)">offsets</see> for a region attachment.
		/// </summary>
		/// <param name="uvs">Output array for the computed UVs, length of 8.</param>
		/// <param name="offset">Output array for the computed vertex offsets, length of 8.</param>
		internal static void ComputeUVs (TextureRegion region, float x, float y, float scaleX, float scaleY, float rotation, float width,
		float height, float[] offset, float[] uvs) {
			float localX2 = width / 2, localY2 = height / 2;
			float localX = -localX2, localY = -localY2;
			bool rotated = false;
			AtlasRegion r = region as AtlasRegion;
			if (r != null) {
				localX += r.offsetX / r.originalWidth * width;
				localY += r.offsetY / r.originalHeight * height;
				if (r.degrees == 90) {
					rotated = true;
					localX2 -= (r.originalWidth - r.offsetX - r.packedHeight) / r.originalWidth * width;
					localY2 -= (r.originalHeight - r.offsetY - r.packedWidth) / r.originalHeight * height;
				} else {
					localX2 -= (r.originalWidth - r.offsetX - r.packedWidth) / r.originalWidth * width;
					localY2 -= (r.originalHeight - r.offsetY - r.packedHeight) / r.originalHeight * height;
				}
			}
			localX *= scaleX;
			localY *= scaleY;
			localX2 *= scaleX;
			localY2 *= scaleY;
			float rot = rotation * MathUtils.DegRad, cos = (float)Math.Cos(rot), sin = (float)Math.Sin(rot);
			float localXCos = localX * cos + x;
			float localXSin = localX * sin;
			float localYCos = localY * cos + y;
			float localYSin = localY * sin;
			float localX2Cos = localX2 * cos + x;
			float localX2Sin = localX2 * sin;
			float localY2Cos = localY2 * cos + y;
			float localY2Sin = localY2 * sin;
			offset[BLX] = localXCos - localYSin;
			offset[BLY] = localYCos + localXSin;
			offset[ULX] = localXCos - localY2Sin;
			offset[ULY] = localY2Cos + localXSin;
			offset[URX] = localX2Cos - localY2Sin;
			offset[URY] = localY2Cos + localX2Sin;
			offset[BRX] = localX2Cos - localYSin;
			offset[BRY] = localYCos + localX2Sin;
			if (region == null) {
				uvs[BLX] = 0;
				uvs[BLY] = 0;
				uvs[ULX] = 0;
				uvs[ULY] = 1;
				uvs[URX] = 1;
				uvs[URY] = 1;
				uvs[BRX] = 1;
				uvs[BRY] = 0;
			} else {
				uvs[BLX] = region.u2;
				uvs[ULY] = region.v2;
				uvs[URX] = region.u;
				uvs[BRY] = region.v;
				if (rotated) {
					uvs[BLY] = region.v;
					uvs[ULX] = region.u2;
					uvs[URY] = region.v2;
					uvs[BRX] = region.u;
				} else {
					uvs[BLY] = region.v2;
					uvs[ULX] = region.u;
					uvs[URY] = region.v;
					uvs[BRX] = region.u2;
				}
			}
		}
	}
}
