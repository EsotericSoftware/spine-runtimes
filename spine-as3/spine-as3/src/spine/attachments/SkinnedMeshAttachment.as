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

package spine.attachments {
import spine.Slot;
import spine.Bone;

public dynamic class SkinnedMeshAttachment extends Attachment {
	public var bones:Vector.<int>;
	public var weights:Vector.<Number>;
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

	public function SkinnedMeshAttachment (name:String) {
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

	public function computeWorldVertices (x:Number, y:Number, slot:Slot, worldVertices:Vector.<Number>) : void {
		var skeletonBones:Vector.<Bone> = slot.skeleton.bones;
		var weights:Vector.<Number> = this.weights;
		var bones:Vector.<int> = this.bones;

		var w:int = 0, v:int = 0, b:int = 0, f:int = 0, n:int = bones.length, nn:int;
		var wx:Number, wy:Number, bone:Bone, vx:Number, vy:Number, weight:Number;
		if (slot.attachmentVertices.length == 0) {
			for (; v < n; w += 2) {
				wx = 0;
				wy = 0;
				nn = bones[v++] + v;
				for (; v < nn; v++, b += 3) {
					bone = skeletonBones[bones[v]];
					vx = weights[b];
					vy = weights[int(b + 1)];
					weight = weights[int(b + 2)];
					wx += (vx * bone.m00 + vy * bone.m01 + bone.worldX) * weight;
					wy += (vx * bone.m10 + vy * bone.m11 + bone.worldY) * weight;
				}
				worldVertices[w] = wx + x;
				worldVertices[int(w + 1)] = wy + y;
			}
		} else {
			var ffd:Vector.<Number> = slot.attachmentVertices;
			for (; v < n; w += 2) {
				wx = 0;
				wy = 0;
				nn = bones[v++] + v;
				for (; v < nn; v++, b += 3, f += 2) {
					bone = skeletonBones[bones[v]];
					vx = weights[b] + ffd[f];
					vy = weights[int(b + 1)] + ffd[int(f + 1)];
					weight = weights[int(b + 2)];
					wx += (vx * bone.m00 + vy * bone.m01 + bone.worldX) * weight;
					wy += (vx * bone.m10 + vy * bone.m11 + bone.worldY) * weight;
				}
				worldVertices[w] = wx + x;
				worldVertices[int(w + 1)] = wy + y;
			}
		}
	}
}

}
