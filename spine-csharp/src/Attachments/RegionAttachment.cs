/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;

namespace Spine {
	/// <summary>Attachment that displays a texture region.</summary>
	public class RegionAttachment : Attachment, IHasRendererObject {
		public const int BLX = 0;
		public const int BLY = 1;
		public const int ULX = 2;
		public const int ULY = 3;
		public const int URX = 4;
		public const int URY = 5;
		public const int BRX = 6;
		public const int BRY = 7;

		internal float x, y, rotation, scaleX = 1, scaleY = 1, width, height;
		internal float regionOffsetX, regionOffsetY, regionWidth, regionHeight, regionOriginalWidth, regionOriginalHeight;
		internal float[] offset = new float[8], uvs = new float[8];
		internal float r = 1, g = 1, b = 1, a = 1;

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
		public object RendererObject { get; set; }
		public float RegionOffsetX { get { return regionOffsetX; } set { regionOffsetX = value; } }
		public float RegionOffsetY { get { return regionOffsetY; } set { regionOffsetY = value; } } // Pixels stripped from the bottom left, unrotated.
		public float RegionWidth { get { return regionWidth; } set { regionWidth = value; } }
		public float RegionHeight { get { return regionHeight; } set { regionHeight = value; } } // Unrotated, stripped size.
		public float RegionOriginalWidth { get { return regionOriginalWidth; } set { regionOriginalWidth = value; } }
		public float RegionOriginalHeight { get { return regionOriginalHeight; } set { regionOriginalHeight = value; } } // Unrotated, unstripped size.

		public float[] Offset { get { return offset; } }
		public float[] UVs { get { return uvs; } }

		public RegionAttachment (string name)
			: base(name) {
		}

		public void UpdateOffset () {
			float width = this.width;
			float height = this.height;
			float localX2 = width * 0.5f;
			float localY2 = height * 0.5f;
			float localX = -localX2;
			float localY = -localY2;
			if (regionOriginalWidth != 0) { // if (region != null)
				localX += regionOffsetX / regionOriginalWidth * width;
				localY += regionOffsetY / regionOriginalHeight * height;
				localX2 -= (regionOriginalWidth - regionOffsetX - regionWidth) / regionOriginalWidth * width;
				localY2 -= (regionOriginalHeight - regionOffsetY - regionHeight) / regionOriginalHeight * height;
			}
			float scaleX = this.scaleX;
			float scaleY = this.scaleY;
			localX *= scaleX;
			localY *= scaleY;
			localX2 *= scaleX;
			localY2 *= scaleY;
			float rotation = this.rotation;
			float cos = MathUtils.CosDeg(rotation);
			float sin = MathUtils.SinDeg(rotation);
			float x = this.x;
			float y = this.y;
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
		}

		public void SetUVs (float u, float v, float u2, float v2, bool rotate) {
			float[] uvs = this.uvs;
			// UV values differ from RegionAttachment.java
			if (rotate) {
				uvs[URX] = u;
				uvs[URY] = v2;
				uvs[BRX] = u;
				uvs[BRY] = v;
				uvs[BLX] = u2;
				uvs[BLY] = v;
				uvs[ULX] = u2;
				uvs[ULY] = v2;
			} else {
				uvs[ULX] = u;
				uvs[ULY] = v2;
				uvs[URX] = u;
				uvs[URY] = v;
				uvs[BRX] = u2;
				uvs[BRY] = v;
				uvs[BLX] = u2;
				uvs[BLY] = v2;
			}
		}

		/// <summary>Transforms the attachment's four vertices to world coordinates.</summary>
		/// <param name="bone">The parent bone.</param>
		/// <param name="worldVertices">The output world vertices. Must have a length greater than or equal to offset + 8.</param>
		/// <param name="offset">The worldVertices index to begin writing values.</param>
		/// <param name="stride">The number of worldVertices entries between the value pairs written.</param>
		public void ComputeWorldVertices (Bone bone, float[] worldVertices, int offset, int stride = 2) {
			float[] vertexOffset = this.offset;
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
	}
}
