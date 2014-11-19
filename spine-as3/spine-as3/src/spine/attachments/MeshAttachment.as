/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 *
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.attachments {
import spine.Slot;
import spine.Bone;

public dynamic class MeshAttachment extends Attachment {
	public var vertices:Vector.<Number>;
	public var uvs:Vector.<Number>;
	public var regionUVs:Vector.<Number>;
	public var triangles:Vector.<uint>;
	public var hullLength:int;
	public var r:Number = 1;
	public var g:Number = 1;
	public var b:Number = 1;
	public var a:Number = 1;

	public var path:String;
	public var rendererObject:Object;
	public var regionU:Number;
	public var regionV:Number;
	public var regionU2:Number;
	public var regionV2:Number;
	public var regionRotate:Boolean;
	public var regionOffsetX:Number; // Pixels stripped from the bottom left, unrotated.
	public var regionOffsetY:Number;
	public var regionWidth:Number; // Unrotated, stripped size.
	public var regionHeight:Number;
	public var regionOriginalWidth:Number; // Unrotated, unstripped size.
	public var regionOriginalHeight:Number;

	// Nonessential.
	public var edges:Vector.<int>;
	public var width:Number;
	public var height:Number;

	public function MeshAttachment (name:String) {
		super(name);
	}

	public function updateUVs () : void {
		var width:Number = regionU2 - regionU, height:Number = regionV2 - regionV;
		var i:int, n:int = regionUVs.length;
		if (!uvs || uvs.length != n) uvs = new Vector.<Number>(n, true);
		if (regionRotate) {
			for (i = 0; i < n; i += 2) {
				uvs[i] = regionU + regionUVs[int(i + 1)] * width;
				uvs[int(i + 1)] = regionV + height - regionUVs[i] * height;
			}
		} else {
			for (i = 0; i < n; i += 2) {
				uvs[i] = regionU + regionUVs[i] * width;
				uvs[int(i + 1)] = regionV + regionUVs[int(i + 1)] * height;
			}
		}
	}

	public function computeWorldVertices (x:Number, y:Number, slot:Slot, worldVertices:Vector.<Number>, worldVerticesOffset:int = 0) : void {
		var bone:Bone = slot.bone;
		x += bone.worldX;
		y += bone.worldY;
		var m00:Number = bone.m00;
		var m01:Number = bone.m01;
		var m10:Number = bone.m10;
		var m11:Number = bone.m11;
		var vertices:Vector.<Number> = this.vertices;
		var verticesCount:int = vertices.length;
		if (slot.attachmentVertices.length == verticesCount) vertices = slot.attachmentVertices;
		for (var i:int = 0, ii:int = 0; i < verticesCount; i += 2, ii += 2) {
			var vx:Number = vertices[i];
			var vy:Number = vertices[int(i + 1)];
			worldVertices[worldVerticesOffset + ii] = vx * m00 + vy * m01 + x;
			worldVertices[worldVerticesOffset + int(ii + 1)] = vx * m10 + vy * m11 + y;
		}
	}
}

}
