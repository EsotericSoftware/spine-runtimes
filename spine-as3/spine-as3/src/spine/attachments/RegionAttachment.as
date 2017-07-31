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
		public const BLX : int = 0;
		public const BLY : int = 1;
		public const ULX : int = 2;
		public const ULY : int = 3;
		public const URX : int = 4;
		public const URY : int = 5;
		public const BRX : int = 6;
		public const BRY : int = 7;
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
		private var offset : Vector.<Number> = new Vector.<Number>();
		public var uvs : Vector.<Number> = new Vector.<Number>();

		public function RegionAttachment(name : String) {
			super(name);
			offset.length = 8;
			uvs.length = 8;
		}

		public function updateOffset() : void {
			var regionScaleX : Number = width / regionOriginalWidth * scaleX;
			var regionScaleY : Number = height / regionOriginalHeight * scaleY;
			var localX : Number = -width * 0.5 * scaleX + regionOffsetX * regionScaleX;
			var localY : Number = -height * 0.5 * scaleY + regionOffsetY * regionScaleY;
			var localX2 : Number = localX + regionWidth * regionScaleX;
			var localY2 : Number = localY + regionHeight * regionScaleY;
					
			var radians : Number = rotation * Math.PI / 180;
			var ulDist : Number = Math.sqrt(localX * localX + localY * localY);
			var ulAngle : Number = Math.atan2(localY, localX);
			var urDist : Number = Math.sqrt(localX2 * localX2 + localY * localY);
			var urAngle : Number = Math.atan2(localY, localX2);
			var blDist : Number = Math.sqrt(localX * localX + localY2 * localY2);
			var blAngle : Number = Math.atan2(localY2, localX);
			var brDist : Number = Math.sqrt(localX2 * localX2 + localY2 * localY2);
			var brAngle : Number = Math.atan2(localY2, localX2);
					
			offset[BLX] = Math.cos(radians - blAngle) * blDist + x;
			offset[BLY] = Math.sin(radians - blAngle) * blDist + y;
			offset[ULX] = Math.cos(radians - ulAngle) * ulDist + x;
			offset[ULY] = Math.sin(radians - ulAngle) * ulDist + y;
			offset[URX] = Math.cos(radians - urAngle) * urDist + x;
			offset[URY] = Math.sin(radians - urAngle) * urDist + y;
			offset[BRX] = Math.cos(radians - brAngle) * brDist + x;
			offset[BRY] = Math.sin(radians - brAngle) * brDist + y;
		}

		public function setUVs(u : Number, v : Number, u2 : Number, v2 : Number, rotate : Boolean) : void {
			var uvs : Vector.<Number> = this.uvs;
			if (rotate) {
				uvs[4] = u;
				uvs[5] = v2;
				uvs[6] = u;
				uvs[7] = v;
				uvs[0] = u2;
				uvs[1] = v;
				uvs[2] = u2;
				uvs[3] = v2;
			} else {
				uvs[2] = u;
				uvs[3] = v2;
				uvs[4] = u;
				uvs[5] = v;
				uvs[6] = u2;
				uvs[7] = v;
				uvs[0] = u2;
				uvs[1] = v2;
			}
		}

		public function computeWorldVertices(bone : Bone, worldVertices : Vector.<Number>, offset : int, stride : int) : void {
			var vertexOffset : Vector.<Number> = this.offset;
			var x : Number = bone.worldX, y : Number = bone.worldY;
			var a : Number = bone.a, b : Number = bone.b, c : Number = bone.c, d : Number = bone.d;
			var offsetX : Number = 0, offsetY : Number = 0;

			offsetX = vertexOffset[BRX];
			offsetY = vertexOffset[BRY];
			worldVertices[offset] = offsetX * a + offsetY * b + x; // br
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
			offset += stride;
		
			offsetX = vertexOffset[BLX];
			offsetY = vertexOffset[BLY];
			worldVertices[offset] = offsetX * a + offsetY * b + x; // bl
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
			offset += stride;
		
			offsetX = vertexOffset[ULX];
			offsetY = vertexOffset[ULY];
			worldVertices[offset] = offsetX * a + offsetY * b + x; // ul
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
			offset += stride;
		
			offsetX = vertexOffset[URX];
			offsetY = vertexOffset[URY];
			worldVertices[offset] = offsetX * a + offsetY * b + x; // ur
			worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		}
	}
}