/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.attachments {
	import spine.Color;
	import spine.Bone;

	public dynamic class RegionAttachment extends Attachment {
		public const X1 : int = 0;
		public const Y1 : int = 1;
		public const X2 : int = 2;
		public const Y2 : int = 3;
		public const X3 : int = 4;
		public const Y3 : int = 5;
		public const X4 : int = 6;
		public const Y4 : int = 7;
		public var x : Number;
		public var y : Number;
		public var scaleX : Number = 1;
		public var scaleY : Number = 1;
		public var rotation : Number;
		public var width : Number;
		public var height : Number;
		public var color : Color = new Color(1, 1, 1, 1);
		public var path : String;
		public var rendererObject : Object;
		public var regionOffsetX : Number; // Pixels stripped from the bottom left, unrotated.
		public var regionOffsetY : Number;
		public var regionWidth : Number; // Unrotated, stripped size.
		public var regionHeight : Number;
		public var regionOriginalWidth : Number; // Unrotated, unstripped size.
		public var regionOriginalHeight : Number;
		public var offset : Vector.<Number> = new Vector.<Number>();
		public var uvs : Vector.<Number> = new Vector.<Number>();

		public function RegionAttachment(name : String) {
			super(name);
			offset.length = 8;
			uvs.length = 8;
		}

		public function updateOffset() : void {
			var regionScaleX : Number = width / regionOriginalWidth * scaleX;
			var regionScaleY : Number = height / regionOriginalHeight * scaleY;
			var localX : Number = -width / 2 * scaleX + regionOffsetX * regionScaleX;
			var localY : Number = -height / 2 * scaleY + regionOffsetY * regionScaleY;
			var localX2 : Number = localX + regionWidth * regionScaleX;
			var localY2 : Number = localY + regionHeight * regionScaleY;
			var radians : Number = rotation * Math.PI / 180;
			var cos : Number = Math.cos(radians);
			var sin : Number = Math.sin(radians);
			var localXCos : Number = localX * cos + x;
			var localXSin : Number = localX * sin;
			var localYCos : Number = localY * cos + y;
			var localYSin : Number = localY * sin;
			var localX2Cos : Number = localX2 * cos + x;
			var localX2Sin : Number = localX2 * sin;
			var localY2Cos : Number = localY2 * cos + y;
			var localY2Sin : Number = localY2 * sin;
			offset[X1] = localXCos - localYSin;
			offset[Y1] = localYCos + localXSin;
			offset[X2] = localXCos - localY2Sin;
			offset[Y2] = localY2Cos + localXSin;
			offset[X3] = localX2Cos - localY2Sin;
			offset[Y3] = localY2Cos + localX2Sin;
			offset[X4] = localX2Cos - localYSin;
			offset[Y4] = localYCos + localX2Sin;
		}

		public function setUVs(u : Number, v : Number, u2 : Number, v2 : Number, rotate : Boolean) : void {
			var uvs : Vector.<Number> = this.uvs;
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

		public function computeWorldVertices(bone : Bone, worldVertices : Vector.<Number>, offset : int, stride : int) : void {
			var vertexOffset : Vector.<Number> = this.offset;
			var x : Number = bone.worldX, y : Number = bone.worldY;
			var a : Number = bone.a, b : Number = bone.b, c : Number = bone.c, d : Number = bone.d;
			var offsetX : Number = 0, offsetY : Number = 0;

			offsetX = vertexOffset[X1];
			offsetY = vertexOffset[Y1];
			worldVertices[offset] = offsetX * a + offsetY * b + x; // br
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
			offset += stride;

			offsetX = vertexOffset[X2];
			offsetY = vertexOffset[Y2];
			worldVertices[offset] = offsetX * a + offsetY * b + x; // bl
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
			offset += stride;

			offsetX = vertexOffset[X3];
			offsetY = vertexOffset[Y3];
			worldVertices[offset] = offsetX * a + offsetY * b + x; // ul
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
			offset += stride;

			offsetX = vertexOffset[X4];
			offsetY = vertexOffset[Y4];
			worldVertices[offset] = offsetX * a + offsetY * b + x; // ur
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		}
	}
}