/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

namespace Spine {
	/// <summary>Attachment that displays a texture region.</summary>
	public class RegionAttachment : Attachment, IHasTextureRegion {
		public const int BLX = 0, BLY = 1;
		public const int ULX = 2, ULY = 3;
		public const int URX = 4, URY = 5;
		public const int BRX = 6, BRY = 7;

		internal TextureRegion region;
		internal float x, y, rotation, scaleX = 1, scaleY = 1, width, height;
		internal float[] offset = new float[8], uvs = new float[8];
		internal float r = 1, g = 1, b = 1, a = 1;
		internal Sequence sequence;

		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }
		public float Rotation { get { return rotation; } set { rotation = value; } }
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }
		public float ScaleY { get { return scaleY; } set { scaleY = value; } }
		public float Width { get { return width; } set { width = value; } }
		public float Height { get { return height; } set { height = value; } }

		public float R { get { return r; } set { r = value; } }
		public float G { get { return g; } set { g = value; } }
		public float B { get { return b; } set { b = value; } }
		public float A { get { return a; } set { a = value; } }

		public string Path { get; set; }
		public TextureRegion Region { get { return region; } set { region = value; } }

		/// <summary>For each of the 4 vertices, a pair of <code>x,y</code> values that is the local position of the vertex.</summary>
		/// <seealso cref="UpdateRegion"/>
		public float[] Offset { get { return offset; } }
		public float[] UVs { get { return uvs; } }
		public Sequence Sequence { get { return sequence; } set { sequence = value; } }

		public RegionAttachment (string name)
			: base(name) {
		}

		/// <summary>Copy constructor.</summary>
		public RegionAttachment (RegionAttachment other)
			: base(other) {
			region = other.region;
			Path = other.Path;
			x = other.x;
			y = other.y;
			scaleX = other.scaleX;
			scaleY = other.scaleY;
			rotation = other.rotation;
			width = other.width;
			height = other.height;
			Array.Copy(other.uvs, 0, uvs, 0, 8);
			Array.Copy(other.offset, 0, offset, 0, 8);
			r = other.r;
			g = other.g;
			b = other.b;
			a = other.a;
			sequence = other.sequence == null ? null : new Sequence(other.sequence);
		}

		/// <summary>Calculates the <see cref="Offset"/> and <see cref="UVs"/> using the region and the attachment's transform. Must be called if the
		/// region, the region's properties, or the transform are changed.</summary>
		public void UpdateRegion () {
			float[] uvs = this.uvs;
			if (region == null) {
				uvs[BLX] = 0;
				uvs[BLY] = 0;
				uvs[ULX] = 0;
				uvs[ULY] = 1;
				uvs[URX] = 1;
				uvs[URY] = 1;
				uvs[BRX] = 1;
				uvs[BRY] = 0;
				return;
			}

			float width = Width;
			float height = Height;
			float localX2 = width / 2;
			float localY2 = height / 2;
			float localX = -localX2;
			float localY = -localY2;
			bool rotated = false;
			if (region is AtlasRegion) {
				AtlasRegion region = (AtlasRegion)this.region;
				localX += region.offsetX / region.originalWidth * width;
				localY += region.offsetY / region.originalHeight * height;
				if (region.degrees == 90) {
					rotated = true;
					localX2 -= (region.originalWidth - region.offsetX - region.packedHeight) / region.originalWidth * width;
					localY2 -= (region.originalHeight - region.offsetY - region.packedWidth) / region.originalHeight * height;
				} else {
					localX2 -= (region.originalWidth - region.offsetX - region.packedWidth) / region.originalWidth * width;
					localY2 -= (region.originalHeight - region.offsetY - region.packedHeight) / region.originalHeight * height;
				}
			}
			float scaleX = ScaleX;
			float scaleY = ScaleY;
			localX *= scaleX;
			localY *= scaleY;
			localX2 *= scaleX;
			localY2 *= scaleY;
			float rotation = Rotation;
			float cos = MathUtils.CosDeg(this.rotation);
			float sin = MathUtils.SinDeg(this.rotation);
			float x = X;
			float y = Y;
			float localXCos = localX * cos + x;
			float localXSin = localX * sin;
			float localYCos = localY * cos + y;
			float localYSin = localY * sin;
			float localX2Cos = localX2 * cos + x;
			float localX2Sin = localX2 * sin;
			float localY2Cos = localY2 * cos + y;
			float localY2Sin = localY2 * sin;
			float[] offset = this.offset;
			offset[BLX] = localXCos - localYSin;
			offset[BLY] = localYCos + localXSin;
			offset[ULX] = localXCos - localY2Sin;
			offset[ULY] = localY2Cos + localXSin;
			offset[URX] = localX2Cos - localY2Sin;
			offset[URY] = localY2Cos + localX2Sin;
			offset[BRX] = localX2Cos - localYSin;
			offset[BRY] = localYCos + localX2Sin;

			if (rotated) {
				uvs[BLX] = region.u2;
				uvs[BLY] = region.v;
				uvs[ULX] = region.u2;
				uvs[ULY] = region.v2;
				uvs[URX] = region.u;
				uvs[URY] = region.v2;
				uvs[BRX] = region.u;
				uvs[BRY] = region.v;
			} else {
				uvs[BLX] = region.u2;
				uvs[BLY] = region.v2;
				uvs[ULX] = region.u;
				uvs[ULY] = region.v2;
				uvs[URX] = region.u;
				uvs[URY] = region.v;
				uvs[BRX] = region.u2;
				uvs[BRY] = region.v;
			}
		}

		/// <summary>
		/// Transforms the attachment's four vertices to world coordinates. If the attachment has a <see cref="Sequence"/> the region may
		/// be changed.</summary>
		/// <param name="bone">The parent bone.</param>
		/// <param name="worldVertices">The output world vertices. Must have a length greater than or equal to offset + 8.</param>
		/// <param name="offset">The worldVertices index to begin writing values.</param>
		/// <param name="stride">The number of worldVertices entries between the value pairs written.</param>
		public void ComputeWorldVertices (Slot slot, float[] worldVertices, int offset, int stride = 2) {
			if (sequence != null) sequence.Apply(slot, this);

			float[] vertexOffset = this.offset;
			Bone bone = slot.Bone;
			float bwx = bone.worldX, bwy = bone.worldY;
			float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
			float offsetX, offsetY;

			// Vertex order is different from RegionAttachment.java
			offsetX = vertexOffset[BRX]; // 0
			offsetY = vertexOffset[BRY]; // 1
			worldVertices[offset] = offsetX * a + offsetY * b + bwx; // bl
			worldVertices[offset + 1] = offsetX * c + offsetY * d + bwy;
			offset += stride;

			offsetX = vertexOffset[BLX]; // 2
			offsetY = vertexOffset[BLY]; // 3
			worldVertices[offset] = offsetX * a + offsetY * b + bwx; // ul
			worldVertices[offset + 1] = offsetX * c + offsetY * d + bwy;
			offset += stride;

			offsetX = vertexOffset[ULX]; // 4
			offsetY = vertexOffset[ULY]; // 5
			worldVertices[offset] = offsetX * a + offsetY * b + bwx; // ur
			worldVertices[offset + 1] = offsetX * c + offsetY * d + bwy;
			offset += stride;

			offsetX = vertexOffset[URX]; // 6
			offsetY = vertexOffset[URY]; // 7
			worldVertices[offset] = offsetX * a + offsetY * b + bwx; // br
			worldVertices[offset + 1] = offsetX * c + offsetY * d + bwy;
			//offset += stride;
		}

		public override Attachment Copy () {
			return new RegionAttachment(this);
		}
	}
}
