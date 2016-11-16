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
import spine.Bone;

public dynamic class RegionAttachment extends Attachment {
	public const X1:int = 0;
	public const Y1:int = 1;
	public const X2:int = 2;
	public const Y2:int = 3;
	public const X3:int = 4;
	public const Y3:int = 5;
	public const X4:int = 6;
	public const Y4:int = 7;

	public var x:Number;
	public var y:Number;
	public var scaleX:Number = 1;
	public var scaleY:Number = 1;
	public var rotation:Number;
	public var width:Number;
	public var height:Number;
	public var r:Number = 1;
	public var g:Number = 1;
	public var b:Number = 1;
	public var a:Number = 1;

	public var path:String;
	public var rendererObject:Object;
	public var regionOffsetX:Number; // Pixels stripped from the bottom left, unrotated.
	public var regionOffsetY:Number;
	public var regionWidth:Number; // Unrotated, stripped size.
	public var regionHeight:Number;
	public var regionOriginalWidth:Number; // Unrotated, unstripped size.
	public var regionOriginalHeight:Number;

	public var offset:Vector.<Number> = new Vector.<Number>();
	public var uvs:Vector.<Number> = new Vector.<Number>();

	public function RegionAttachment (name:String) {
		super(name);
		offset.length = 8;
		uvs.length = 8;
	}

	public function setUVs (u:Number, v:Number, u2:Number, v2:Number, rotate:Boolean) : void {
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

	public function updateOffset () : void {
		var regionScaleX:Number = width / regionOriginalWidth * scaleX;
		var regionScaleY:Number = height / regionOriginalHeight * scaleY;
		var localX:Number = -width / 2 * scaleX + regionOffsetX * regionScaleX;
		var localY:Number = -height / 2 * scaleY + regionOffsetY * regionScaleY;
		var localX2:Number = localX + regionWidth * regionScaleX;
		var localY2:Number = localY + regionHeight * regionScaleY;
		var radians:Number = rotation * Math.PI / 180;
		var cos:Number = Math.cos(radians);
		var sin:Number = Math.sin(radians);
		var localXCos:Number = localX * cos + x;
		var localXSin:Number = localX * sin;
		var localYCos:Number = localY * cos + y;
		var localYSin:Number = localY * sin;
		var localX2Cos:Number = localX2 * cos + x;
		var localX2Sin:Number = localX2 * sin;
		var localY2Cos:Number = localY2 * cos + y;
		var localY2Sin:Number = localY2 * sin;
		offset[X1] = localXCos - localYSin;
		offset[Y1] = localYCos + localXSin;
		offset[X2] = localXCos - localY2Sin;
		offset[Y2] = localY2Cos + localXSin;
		offset[X3] = localX2Cos - localY2Sin;
		offset[Y3] = localY2Cos + localX2Sin;
		offset[X4] = localX2Cos - localYSin;
		offset[Y4] = localYCos + localX2Sin;
	}

	public function computeWorldVertices (x:Number, y:Number, bone:Bone, worldVertices:Vector.<Number>) : void {
		x += bone.worldX;
		y += bone.worldY;
		var m00:Number = bone.a;
		var m01:Number = bone.b;
		var m10:Number = bone.c;
		var m11:Number = bone.d;
		var x1:Number = offset[X1];
		var y1:Number = offset[Y1];
		var x2:Number = offset[X2];
		var y2:Number = offset[Y2];
		var x3:Number = offset[X3];
		var y3:Number = offset[Y3];
		var x4:Number = offset[X4];
		var y4:Number = offset[Y4];
		worldVertices[X1] = x1 * m00 + y1 * m01 + x;
		worldVertices[Y1] = x1 * m10 + y1 * m11 + y;
		worldVertices[X2] = x2 * m00 + y2 * m01 + x;
		worldVertices[Y2] = x2 * m10 + y2 * m11 + y;
		worldVertices[X3] = x3 * m00 + y3 * m01 + x;
		worldVertices[Y3] = x3 * m10 + y3 * m11 + y;
		worldVertices[X4] = x4 * m00 + y4 * m01 + x;
		worldVertices[Y4] = x4 * m10 + y4 * m11 + y;
	}
}

}
