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

namespace Spine {
	/** Attachment that displays a texture region. */
	public class RegionAttachment : Attachment {
		public const int X1 = 0;
		public const int Y1 = 1;
		public const int X2 = 2;
		public const int Y2 = 3;
		public const int X3 = 4;
		public const int Y3 = 5;
		public const int X4 = 6;
		public const int Y4 = 7;

		public float X { get; set; }
		public float Y { get; set; }
		public float ScaleX { get; set; }
		public float ScaleY { get; set; }
		public float Rotation { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }

		public Object RendererObject { get; set; }
		public float RegionOffsetX { get; set; }
		public float RegionOffsetY { get; set; } // Pixels stripped from the bottom left, unrotated.
		public float RegionWidth { get; set; }
		public float RegionHeight { get; set; } // Unrotated, stripped size.
		public float RegionOriginalWidth { get; set; }
		public float RegionOriginalHeight { get; set; } // Unrotated, unstripped size.

		public float[] Offset { get; private set; }
		public float[] UVs { get; private set; }

		public RegionAttachment (string name)
			: base(name) {
			Offset = new float[8];
			UVs = new float[8];
			ScaleX = 1;
			ScaleY = 1;
		}

		public void SetUVs (float u, float v, float u2, float v2, bool rotate) {
			float[] uvs = UVs;
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
			float width = Width;
			float height = Height;
			float scaleX = ScaleX;
			float scaleY = ScaleY;
			float regionScaleX = width / RegionOriginalWidth * scaleX;
			float regionScaleY = height / RegionOriginalHeight * scaleY;
			float localX = -width / 2 * scaleX + RegionOffsetX * regionScaleX;
			float localY = -height / 2 * scaleY + RegionOffsetY * regionScaleY;
			float localX2 = localX + RegionWidth * regionScaleX;
			float localY2 = localY + RegionHeight * regionScaleY;
			float radians = Rotation * (float)Math.PI / 180;
			float cos = (float)Math.Cos(radians);
			float sin = (float)Math.Sin(radians);
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
			float[] offset = Offset;
			offset[X1] = localXCos - localYSin;
			offset[Y1] = localYCos + localXSin;
			offset[X2] = localXCos - localY2Sin;
			offset[Y2] = localY2Cos + localXSin;
			offset[X3] = localX2Cos - localY2Sin;
			offset[Y3] = localY2Cos + localX2Sin;
			offset[X4] = localX2Cos - localYSin;
			offset[Y4] = localYCos + localX2Sin;
		}

		public void ComputeVertices (float x, float y, Bone bone, float[] vertices) {
			x += bone.WorldX;
			y += bone.WorldY;
			float m00 = bone.M00;
			float m01 = bone.M01;
			float m10 = bone.M10;
			float m11 = bone.M11;
			float[] offset = Offset;
			vertices[X1] = offset[X1] * m00 + offset[Y1] * m01 + x;
			vertices[Y1] = offset[X1] * m10 + offset[Y1] * m11 + y;
			vertices[X2] = offset[X2] * m00 + offset[Y2] * m01 + x;
			vertices[Y2] = offset[X2] * m10 + offset[Y2] * m11 + y;
			vertices[X3] = offset[X3] * m00 + offset[Y3] * m01 + x;
			vertices[Y3] = offset[X3] * m10 + offset[Y3] * m11 + y;
			vertices[X4] = offset[X4] * m00 + offset[Y4] * m01 + x;
			vertices[Y4] = offset[X4] * m10 + offset[Y4] * m11 + y;
		}
	}
}
