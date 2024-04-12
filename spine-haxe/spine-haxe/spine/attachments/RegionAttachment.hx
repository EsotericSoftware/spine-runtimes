/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine.attachments;

import spine.Color;

class RegionAttachment extends Attachment implements HasTextureRegion {
	public static inline var BLX:Int = 0;
	public static inline var BLY:Int = 1;
	public static inline var ULX:Int = 2;
	public static inline var ULY:Int = 3;
	public static inline var URX:Int = 4;
	public static inline var URY:Int = 5;
	public static inline var BRX:Int = 6;
	public static inline var BRY:Int = 7;

	public var x:Float = 0;
	public var y:Float = 0;
	public var scaleX:Float = 1;
	public var scaleY:Float = 1;
	public var rotation:Float = 0;
	public var width:Float = 0;
	public var height:Float = 0;
	public var color:Color = new Color(1, 1, 1, 1);
	public var path:String;
	public var rendererObject:Dynamic;
	public var region:TextureRegion;
	public var sequence:Sequence;

	private var offsets:Array<Float> = new Array<Float>();

	public var uvs:Array<Float> = new Array<Float>();

	public function new(name:String, path:String) {
		super(name);
		this.path = path;
	}

	public function updateRegion():Void {
		if (region == null) {
			throw new SpineException("Region not set.");
			uvs[0] = 0;
			uvs[1] = 0;
			uvs[2] = 0;
			uvs[3] = 1;
			uvs[4] = 1;
			uvs[5] = 1;
			uvs[6] = 1;
			uvs[7] = 0;
			return;
		}

		var regionScaleX = width / region.originalWidth * scaleX;
		var regionScaleY = height / region.originalHeight * scaleY;
		var localX = -width / 2 * scaleX + region.offsetX * regionScaleX;
		var localY = -height / 2 * scaleY + region.offsetY * regionScaleY;
		var localX2 = localX + region.width * regionScaleX;
		var localY2 = localY + region.height * regionScaleY;
		var radians = rotation * MathUtils.degRad;
		var cos = Math.cos(radians);
		var sin = Math.sin(radians);
		var x = this.x, y = this.y;
		var localXCos = localX * cos + x;
		var localXSin = localX * sin;
		var localYCos = localY * cos + y;
		var localYSin = localY * sin;
		var localX2Cos = localX2 * cos + x;
		var localX2Sin = localX2 * sin;
		var localY2Cos = localY2 * cos + y;
		var localY2Sin = localY2 * sin;

		offsets[0] = localXCos - localYSin;
		offsets[1] = localYCos + localXSin;
		offsets[2] = localXCos - localY2Sin;
		offsets[3] = localY2Cos + localXSin;
		offsets[4] = localX2Cos - localY2Sin;
		offsets[5] = localY2Cos + localX2Sin;
		offsets[6] = localX2Cos - localYSin;
		offsets[7] = localYCos + localX2Sin;

		if (region.degrees == 90) {
			uvs[0] = region.u2;
			uvs[1] = region.v2;
			uvs[2] = region.u;
			uvs[3] = region.v2;
			uvs[4] = region.u;
			uvs[5] = region.v;
			uvs[6] = region.u2;
			uvs[7] = region.v;
		} else {
			uvs[0] = region.u;
			uvs[1] = region.v2;
			uvs[2] = region.u;
			uvs[3] = region.v;
			uvs[4] = region.u2;
			uvs[5] = region.v;
			uvs[6] = region.u2;
			uvs[7] = region.v2;
		}
	}

	public function computeWorldVertices(slot:Slot, worldVertices:Array<Float>, offset:Int, stride:Int):Void {
		if (sequence != null)
			sequence.apply(slot, this);

		var bone = slot.bone;
		var vertexOffset = this.offsets;
		var x = bone.worldX, y = bone.worldY;
		var a = bone.a, b = bone.b, c = bone.c, d = bone.d;
		var offsetX:Float = 0, offsetY:Float = 0;

		offsetX = vertexOffset[0];
		offsetY = vertexOffset[1];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // br
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = vertexOffset[2];
		offsetY = vertexOffset[3];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // bl
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = vertexOffset[4];
		offsetY = vertexOffset[5];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // ul
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
		offset += stride;

		offsetX = vertexOffset[6];
		offsetY = vertexOffset[7];
		worldVertices[offset] = offsetX * a + offsetY * b + x; // ur
		worldVertices[offset + 1] = offsetX * c + offsetY * d + y;
	}

	override public function copy():Attachment {
		var copy:RegionAttachment = new RegionAttachment(name, path);
		copy.region = region;
		copy.rendererObject = rendererObject;
		copy.x = x;
		copy.y = y;
		copy.scaleX = scaleX;
		copy.scaleY = scaleY;
		copy.rotation = rotation;
		copy.width = width;
		copy.height = height;
		copy.uvs = uvs.copy();
		copy.offsets = offsets.copy();
		copy.color.setFromColor(color);
		copy.sequence = sequence != null ? sequence.copy() : null;
		return copy;
	}
}
