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

namespace Spine {
	/// <summary>Attachment that displays a texture region.</summary>
	public class RegionAttachment : Attachment {
		public const int X1 = 0;
		public const int Y1 = 1;
		public const int X2 = 2;
		public const int Y2 = 3;
		public const int X3 = 4;
		public const int Y3 = 5;
		public const int X4 = 6;
		public const int Y4 = 7;

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

		public String Path { get; set; }
		public Object RendererObject { get; set; }
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

		public void SetUVs (float u, float v, float u2, float v2, bool rotate) {
			float[] uvs = this.uvs;
			if (rotate) {
				uvs[X2] = u;
				uvs[Y2] = v2;
				uvs[X3] = u;
				uvs[Y3] = v;
				uvs[X4] = u2;
				uvs[Y4] = v;
				uvs[X1] = u2;
				uvs[Y1] = v2;
			} else {
				uvs[X1] = u;
				uvs[Y1] = v2;
				uvs[X2] = u;
				uvs[Y2] = v;
				uvs[X3] = u2;
				uvs[Y3] = v;
				uvs[X4] = u2;
				uvs[Y4] = v2;
			}
		}

		public void UpdateOffset () {
			float width = this.width;
			float height = this.height;
			float scaleX = this.scaleX;
			float scaleY = this.scaleY;
			float regionScaleX = width / regionOriginalWidth * scaleX;
			float regionScaleY = height / regionOriginalHeight * scaleY;
			float localX = -width / 2 * scaleX + regionOffsetX * regionScaleX;
			float localY = -height / 2 * scaleY + regionOffsetY * regionScaleY;
			float localX2 = localX + regionWidth * regionScaleX;
			float localY2 = localY + regionHeight * regionScaleY;
			float radians = rotation * (float)Math.PI / 180;
			float cos = (float)Math.Cos(radians);
			float sin = (float)Math.Sin(radians);
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
			offset[X1] = localXCos - localYSin;
			offset[Y1] = localYCos + localXSin;
			offset[X2] = localXCos - localY2Sin;
			offset[Y2] = localY2Cos + localXSin;
			offset[X3] = localX2Cos - localY2Sin;
			offset[Y3] = localY2Cos + localX2Sin;
			offset[X4] = localX2Cos - localYSin;
			offset[Y4] = localYCos + localX2Sin;
		}

		public void ComputeWorldVertices (Bone bone, float[] worldVertices) {
			float x = bone.skeleton.x + bone.worldX, y = bone.skeleton.y + bone.worldY;
			float m00 = bone.m00, m01 = bone.m01, m10 = bone.m10, m11 = bone.m11;
			float[] offset = this.offset;
			worldVertices[X1] = offset[X1] * m00 + offset[Y1] * m01 + x;
			worldVertices[Y1] = offset[X1] * m10 + offset[Y1] * m11 + y;
			worldVertices[X2] = offset[X2] * m00 + offset[Y2] * m01 + x;
			worldVertices[Y2] = offset[X2] * m10 + offset[Y2] * m11 + y;
			worldVertices[X3] = offset[X3] * m00 + offset[Y3] * m01 + x;
			worldVertices[Y3] = offset[X3] * m10 + offset[Y3] * m11 + y;
			worldVertices[X4] = offset[X4] * m00 + offset[Y4] * m01 + x;
			worldVertices[Y4] = offset[X4] * m10 + offset[Y4] * m11 + y;
		}
	}
}
